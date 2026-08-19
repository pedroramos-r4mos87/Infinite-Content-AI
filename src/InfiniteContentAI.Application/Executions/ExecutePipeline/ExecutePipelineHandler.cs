using System.Diagnostics.CodeAnalysis;
using InfiniteContentAI.Application.Abstractions.Data;
using InfiniteContentAI.Application.Artifacts;
using InfiniteContentAI.Application.ArtificialIntelligence;
using InfiniteContentAI.Application.Identity;
using InfiniteContentAI.Application.Pipelines;
using InfiniteContentAI.Domain.Artifacts;
using InfiniteContentAI.Domain.Executions;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.SharedKernel.Results;
using InfiniteContentAI.SharedKernel.Time;

namespace InfiniteContentAI.Application.Executions.ExecutePipeline;

public sealed class ExecutePipelineHandler(
    ICurrentOrganization currentOrganization,
    ICurrentUser currentUser,
    IPipelineRepository pipelineRepository,
    IPipelineExecutionRepository executionRepository,
    IArtifactRepository artifactRepository,
    IAIProvider aiProvider,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    [SuppressMessage(
        "Design",
        "CA1031:Do not catch general exception types",
        Justification = "Unexpected provider exceptions are converted into a sanitized terminal execution state.")]
    public async Task<Result<ExecutePipelineResult>> HandleAsync(
        ExecutePipelineCommand command,
        CancellationToken cancellationToken)
    {
        Result validation = ExecutePipelineValidator.Validate(command);
        if (validation.IsFailure)
        {
            return Result.Failure<ExecutePipelineResult>(validation.Error);
        }

        Result<OrganizationId> organization = currentOrganization.Require();
        if (organization.IsFailure)
        {
            return Result.Failure<ExecutePipelineResult>(organization.Error);
        }

        if (string.IsNullOrWhiteSpace(currentUser.UserId))
        {
            return Result.Failure<ExecutePipelineResult>(IdentityErrors.UserRequired);
        }

        var pipelineId = new PipelineId(command.PipelineId);
        Pipeline? pipeline = await pipelineRepository.GetForUpdateAsync(
            organization.Value,
            pipelineId,
            cancellationToken);
        if (pipeline is null)
        {
            return Result.Failure<ExecutePipelineResult>(
                PipelineExecutionApplicationErrors.PipelineNotFound);
        }

        if (pipeline.Status != PipelineStatus.Published)
        {
            return Result.Failure<ExecutePipelineResult>(
                PipelineExecutionApplicationErrors.PipelineNotPublished);
        }

        Result<PipelineExecution> creation = PipelineExecution.Create(
            organization.Value,
            pipeline.ProjectId,
            pipeline.Id,
            pipeline.Version,
            command.Topic,
            currentUser.UserId,
            clock);
        if (creation.IsFailure)
        {
            return Result.Failure<ExecutePipelineResult>(creation.Error);
        }

        PipelineExecution execution = creation.Value;
        foreach (PipelineStep pipelineStep in pipeline.Steps.OrderBy(step => step.Position))
        {
            Result<StepExecutionId> stepCreation = execution.AddStep(
                pipelineStep.Id,
                pipelineStep.Type,
                pipelineStep.Position);
            if (stepCreation.IsFailure)
            {
                return Result.Failure<ExecutePipelineResult>(stepCreation.Error);
            }
        }

        await executionRepository.AddAsync(execution, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        StepExecution researchStep = execution.Steps.Single(
            step => step.Type == PipelineStepType.Research);
        StepExecution scriptStep = execution.Steps.Single(
            step => step.Type == PipelineStepType.Script);

        Result start = execution.Start(clock);
        if (start.IsFailure)
        {
            return await FailExecutionAsync(
                execution,
                researchStep.Id,
                start.Error,
                researchArtifact: null,
                cancellationToken);
        }

        Result researchStart = execution.StartStep(researchStep.Id, clock);
        if (researchStart.IsFailure)
        {
            return await FailExecutionAsync(
                execution,
                researchStep.Id,
                researchStart.Error,
                researchArtifact: null,
                cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        Result<AIResearchResult> research;
        try
        {
            research = await aiProvider.ResearchAsync(
                execution.Topic,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return await FailExecutionAsync(
                execution,
                researchStep.Id,
                AIProviderErrors.UnexpectedFailure,
                researchArtifact: null,
                cancellationToken);
        }

        if (research.IsFailure)
        {
            return await FailExecutionAsync(
                execution,
                researchStep.Id,
                research.Error,
                researchArtifact: null,
                cancellationToken);
        }

        Result<Artifact> researchArtifactCreation = Artifact.Create(
            execution.OrganizationId,
            execution.ProjectId,
            execution.Id,
            researchStep.Id,
            ArtifactType.Research,
            research.Value.Content,
            clock);
        if (researchArtifactCreation.IsFailure)
        {
            return await FailExecutionAsync(
                execution,
                researchStep.Id,
                researchArtifactCreation.Error,
                researchArtifact: null,
                cancellationToken);
        }

        Artifact researchArtifact = researchArtifactCreation.Value;
        await artifactRepository.AddAsync(researchArtifact, cancellationToken);

        Result researchCompletion = execution.CompleteStep(researchStep.Id, clock);
        if (researchCompletion.IsFailure)
        {
            return await FailExecutionAsync(
                execution,
                researchStep.Id,
                researchCompletion.Error,
                researchArtifact,
                cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        Result scriptStart = execution.StartStep(scriptStep.Id, clock);
        if (scriptStart.IsFailure)
        {
            return await FailExecutionAsync(
                execution,
                scriptStep.Id,
                scriptStart.Error,
                researchArtifact,
                cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        Result<AIScriptResult> script;
        try
        {
            script = await aiProvider.GenerateScriptAsync(
                execution.Topic,
                researchArtifact.Content,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return await FailExecutionAsync(
                execution,
                scriptStep.Id,
                AIProviderErrors.UnexpectedFailure,
                researchArtifact,
                cancellationToken);
        }

        if (script.IsFailure)
        {
            return await FailExecutionAsync(
                execution,
                scriptStep.Id,
                script.Error,
                researchArtifact,
                cancellationToken);
        }

        Result<Artifact> scriptArtifactCreation = Artifact.Create(
            execution.OrganizationId,
            execution.ProjectId,
            execution.Id,
            scriptStep.Id,
            ArtifactType.Script,
            script.Value.Content,
            clock);
        if (scriptArtifactCreation.IsFailure)
        {
            return await FailExecutionAsync(
                execution,
                scriptStep.Id,
                scriptArtifactCreation.Error,
                researchArtifact,
                cancellationToken);
        }

        Artifact scriptArtifact = scriptArtifactCreation.Value;
        await artifactRepository.AddAsync(scriptArtifact, cancellationToken);

        Result scriptCompletion = execution.CompleteStep(scriptStep.Id, clock);
        if (scriptCompletion.IsFailure)
        {
            return await FailExecutionAsync(
                execution,
                scriptStep.Id,
                scriptCompletion.Error,
                researchArtifact,
                cancellationToken);
        }

        Result completion = execution.Complete(clock);
        if (completion.IsFailure)
        {
            return await FailExecutionAsync(
                execution,
                scriptStep.Id,
                completion.Error,
                researchArtifact,
                cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(
            MapResult(execution, researchArtifact, scriptArtifact));
    }

    private async Task<Result<ExecutePipelineResult>> FailExecutionAsync(
        PipelineExecution execution,
        StepExecutionId stepExecutionId,
        Error error,
        Artifact? researchArtifact,
        CancellationToken cancellationToken)
    {
        string failureCode = NormalizeFailureCode(error.Code);
        string failureMessage = NormalizeFailureMessage(error.Description);

        Result stepFailure = execution.FailStep(
            stepExecutionId,
            failureCode,
            failureMessage,
            clock);
        if (stepFailure.IsFailure)
        {
            return Result.Failure<ExecutePipelineResult>(stepFailure.Error);
        }

        Result executionFailure = execution.Fail(
            failureCode,
            failureMessage,
            clock);
        if (executionFailure.IsFailure)
        {
            return Result.Failure<ExecutePipelineResult>(executionFailure.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(
            MapResult(
                execution,
                researchArtifact,
                scriptArtifact: null));
    }

    private static ExecutePipelineResult MapResult(
        PipelineExecution execution,
        Artifact? researchArtifact,
        Artifact? scriptArtifact)
    {
        return new ExecutePipelineResult(
            execution.Id.Value,
            execution.PipelineId.Value,
            execution.PipelineVersion,
            execution.Status.ToString().ToLowerInvariant(),
            execution.CreatedAt,
            execution.StartedAt,
            execution.CompletedAt,
            execution.FailedAt,
            execution.FailureCode,
            researchArtifact?.Id.Value,
            scriptArtifact?.Id.Value);
    }

    private static string NormalizeFailureCode(string failureCode)
    {
        return string.IsNullOrWhiteSpace(failureCode) ||
            failureCode.Length > PipelineExecution.MaximumFailureCodeLength
            ? AIProviderErrors.UnexpectedFailure.Code
            : failureCode;
    }

    private static string NormalizeFailureMessage(string failureMessage)
    {
        string message = string.IsNullOrWhiteSpace(failureMessage)
            ? AIProviderErrors.UnexpectedFailure.Description
            : failureMessage;

        return message.Length <= PipelineExecution.MaximumFailureMessageLength
            ? message
            : message[..PipelineExecution.MaximumFailureMessageLength];
    }
}
