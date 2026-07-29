# Organização por Features

## 1. Objetivo

Este documento define como o código do backend do Infinite Content AI será organizado por funcionalidades de negócio.

A organização por features tem como objetivo manter próximos os arquivos que participam do mesmo caso de uso, evitando uma estrutura baseada exclusivamente em tipos técnicos.

Em vez de organizar toda a aplicação assim:

```text
Commands
Queries
Handlers
Validators
DTOs
```

o sistema será organizado principalmente desta forma:

```text
Features
├── Projects
├── Pipelines
├── Executions
├── Artifacts
├── Approvals
└── Publications
```

Cada feature conterá seus próprios:

- Commands.
    
- Queries.
    
- Handlers.
    
- Validators.
    
- Results.
    
- DTOs.
    
- Mappers.
    
- Endpoints.
    
- Testes.
    

A organização por features complementará a Clean Architecture.

A Clean Architecture continuará definindo:

- Limites entre projetos.
    
- Direção das dependências.
    
- Separação entre domínio e infraestrutura.
    
- Isolamento dos detalhes externos.
    

A organização por features definirá:

- Como o código será agrupado dentro de cada projeto.
    
- Como novos casos de uso serão adicionados.
    
- Como navegar pela solution.
    
- Como manter alta coesão entre arquivos relacionados.
    

---

# 2. Princípio Central

A unidade principal de organização será a capacidade de negócio.

Exemplos:

```text
Criar projeto
Iniciar pipeline
Cancelar execução
Consultar artefato
Aprovar roteiro
Publicar conteúdo
```

Cada caso de uso deverá possuir uma localização clara e previsível.

Exemplo:

```text
Application
└── Features
    └── Projects
        └── CreateProject
            ├── CreateProjectCommand.cs
            ├── CreateProjectHandler.cs
            ├── CreateProjectValidator.cs
            └── CreateProjectResult.cs
```

O objetivo é permitir que um desenvolvedor encontre rapidamente todo o fluxo relacionado a uma operação.

---

# 3. Organização Geral da Solution

A solution continuará separada em projetos arquiteturais:

```text
InfiniteContentAI.sln
│
├── Api
├── Application
├── Domain
├── Data
├── Infrastructure
├── Worker
├── Contracts
└── SharedKernel
```

Dentro desses projetos, o código deverá ser organizado por feature sempre que isso fizer sentido.

Visão conceitual:

```text
Api
└── Features
    ├── Projects
    ├── Pipelines
    ├── Executions
    └── Artifacts

Application
└── Features
    ├── Projects
    ├── Pipelines
    ├── Executions
    └── Artifacts

Domain
├── Projects
├── Pipelines
├── Executions
└── Artifacts

Data
├── Projects
├── Pipelines
├── Executions
└── Artifacts
```

Nem todos os projetos precisarão possuir exatamente as mesmas features.

Por exemplo:

- `Projects` poderá existir em Api, Application, Domain e Data.
    
- `RabbitMQ` existirá apenas em Infrastructure.
    
- `PipelineExecutionRequestedV1` existirá em Contracts.
    
- `Consumers` existirão no Worker.
    
- `Result` existirá no SharedKernel.
    

---

# 4. Features Iniciais

O backend será dividido inicialmente nos seguintes módulos:

```text
Projects
Pipelines
Executions
Artifacts
Approvals
Publications
Organizations
Identity
```

Para o MVP, o foco será:

```text
Projects
Pipelines
Executions
Artifacts
```

Os demais módulos serão implementados conforme a evolução do produto.

---

# 5. Tipos de Feature

As features poderão ser classificadas em quatro categorias principais.

## 5.1 Commands

Commands representam uma intenção de alterar o estado do sistema.

Exemplos:

```text
CreateProject
UpdateProject
StartPipeline
CancelExecution
ApproveArtifact
PublishContent
```

Um command deverá:

- Representar uma ação.
    
- Utilizar verbo no imperativo ou uma intenção explícita.
    
- Possuir dados suficientes para executar o caso de uso.
    
- Não expor entidades de domínio.
    
- Retornar um resultado explícito.
    
- Ser idempotente quando necessário.
    

Exemplo:

```csharp
public sealed record CreateProjectCommand(
    Guid OrganizationId,
    Guid UserId,
    string Name,
    string? Description);
```

---

## 5.2 Queries

Queries representam operações de leitura.

Exemplos:

```text
GetProject
ListProjects
GetPipeline
GetExecution
GetExecutionArtifacts
```

Uma query deverá:

- Não alterar estado de negócio.
    
- Retornar um modelo otimizado para leitura.
    
- Aplicar filtros de tenant.
    
- Aplicar autorização.
    
- Utilizar paginação quando necessário.
    
- Evitar carregar Aggregates completos sem necessidade.
    

Exemplo:

```csharp
public sealed record GetProjectQuery(
    Guid OrganizationId,
    Guid ProjectId);
```

---

## 5.3 Event Handlers

Event Handlers reagem a eventos internos ou externos.

Exemplos:

```text
PipelineStarted
PipelineCompleted
ArtifactGenerated
PublicationCompleted
```

Um Event Handler deverá:

- Executar uma responsabilidade específica.
    
- Evitar concentrar várias ações não relacionadas.
    
- Ser idempotente quando processar eventos distribuídos.
    
- Registrar falhas observáveis.
    
- Não depender da ordem entre handlers, salvo quando explicitamente modelado.
    

---

## 5.4 Pipeline Steps

Pipeline Steps representam etapas executáveis de um pipeline.

Exemplos:

```text
Research
GenerateOutline
GenerateScript
ReviewScript
GenerateImage
PublishContent
```

Uma etapa deverá:

- Possuir tipo explícito.
    
- Receber um contexto definido.
    
- Validar pré-condições.
    
- Produzir um resultado estruturado.
    
- Persistir artefatos quando necessário.
    
- Suportar cancelamento.
    
- Ser recuperável.
    
- Ser idempotente.
    
- Registrar checkpoint.
    

---

# 6. Estrutura do Projeto Application

O projeto `Application` concentrará a maior parte da organização por features.

Estrutura recomendada:

```text
Application
│
├── Abstractions
│   ├── Authentication
│   ├── Authorization
│   ├── Caching
│   ├── Messaging
│   ├── Persistence
│   ├── Providers
│   ├── Storage
│   └── Time
│
├── Behaviors
│   ├── ValidationBehavior.cs
│   ├── AuthorizationBehavior.cs
│   ├── LoggingBehavior.cs
│   ├── TransactionBehavior.cs
│   └── IdempotencyBehavior.cs
│
├── Common
│   ├── DTOs
│   ├── Mapping
│   ├── Pagination
│   └── Security
│
├── Features
│   ├── Projects
│   ├── Pipelines
│   ├── Executions
│   ├── Artifacts
│   ├── Approvals
│   ├── Publications
│   ├── Organizations
│   └── Identity
│
└── DependencyInjection.cs
```

---

# 7. Estrutura Interna de uma Feature

Exemplo completo para `Projects`:

```text
Application
└── Features
    └── Projects
        ├── CreateProject
        │   ├── CreateProjectCommand.cs
        │   ├── CreateProjectHandler.cs
        │   ├── CreateProjectValidator.cs
        │   ├── CreateProjectResult.cs
        │   └── CreateProjectMapper.cs
        │
        ├── UpdateProject
        │   ├── UpdateProjectCommand.cs
        │   ├── UpdateProjectHandler.cs
        │   ├── UpdateProjectValidator.cs
        │   └── UpdateProjectResult.cs
        │
        ├── GetProject
        │   ├── GetProjectQuery.cs
        │   ├── GetProjectHandler.cs
        │   └── GetProjectResult.cs
        │
        ├── ListProjects
        │   ├── ListProjectsQuery.cs
        │   ├── ListProjectsHandler.cs
        │   └── ProjectListItem.cs
        │
        └── Common
            ├── ProjectErrors.cs
            ├── ProjectMappings.cs
            └── ProjectPermissions.cs
```

A pasta `Common` da feature deverá conter somente elementos compartilhados entre diferentes casos de uso daquela feature.

Ela não deverá concentrar toda a lógica do módulo.

---

# 8. Exemplo de Feature Completa

## 8.1 Command

```csharp
public sealed record CreateProjectCommand(
    Guid OrganizationId,
    Guid UserId,
    string Name,
    string? Description)
    : ICommand<CreateProjectResult>;
```

---

## 8.2 Validator

```csharp
public sealed class CreateProjectValidator
    : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectValidator()
    {
        RuleFor(command => command.OrganizationId)
            .NotEmpty();

        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(command => command.Description)
            .MaximumLength(2_000);
    }
}
```

O Validator deverá validar o formato e as regras de entrada do caso de uso.

Ele não deverá substituir invariantes do Domain.

---

## 8.3 Handler

```csharp
public sealed class CreateProjectHandler
    : ICommandHandler<CreateProjectCommand, CreateProjectResult>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public CreateProjectHandler(
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<CreateProjectResult>> Handle(
        CreateProjectCommand command,
        CancellationToken cancellationToken)
    {
        var project = Project.Create(
            command.OrganizationId,
            command.Name,
            command.Description,
            _clock.UtcNow);

        await _projectRepository.AddAsync(
            project,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new CreateProjectResult(
            project.Id,
            project.Name,
            project.CreatedAt);
    }
}
```

O Handler deverá coordenar o caso de uso.

Ele não deverá implementar regras que pertençam ao Domain.

---

## 8.4 Result

```csharp
public sealed record CreateProjectResult(
    Guid ProjectId,
    string Name,
    DateTimeOffset CreatedAt);
```

O resultado da Application não deverá ser necessariamente igual ao contrato HTTP.

---

# 9. Granularidade das Features

Uma feature deverá representar uma operação de negócio identificável.

Granularidade adequada:

```text
CreateProject
StartPipeline
CancelExecution
GetExecution
ApproveArtifact
```

Granularidade excessivamente ampla:

```text
ProjectManagement
PipelineService
ArtifactOperations
```

Granularidade excessivamente pequena:

```text
ValidateProjectName
SetCreatedAt
MapProjectId
```

Funções auxiliares pequenas não deverão virar features independentes.

---

# 10. Organização do Projeto Api

O projeto `Api` também será organizado por features.

Estrutura recomendada:

```text
Api
│
├── Common
│   ├── Authentication
│   ├── Authorization
│   ├── Errors
│   ├── Extensions
│   ├── Middleware
│   ├── OpenApi
│   └── Versioning
│
├── Features
│   ├── Projects
│   │   ├── CreateProject
│   │   ├── UpdateProject
│   │   ├── GetProject
│   │   └── ListProjects
│   │
│   ├── Pipelines
│   ├── Executions
│   └── Artifacts
│
├── DependencyInjection.cs
└── Program.cs
```

Cada endpoint ficará próximo dos contratos HTTP que utiliza.

Exemplo:

```text
Api
└── Features
    └── Projects
        └── CreateProject
            ├── CreateProjectEndpoint.cs
            ├── CreateProjectRequest.cs
            └── CreateProjectResponse.cs
```

---

# 11. Endpoint por Feature

Exemplo conceitual com Minimal API:

```csharp
public static class CreateProjectEndpoint
{
    public static void MapCreateProjectEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/api/v1/projects",
            HandleAsync)
            .WithName("CreateProject")
            .WithTags("Projects")
            .Produces<CreateProjectResponse>(
                StatusCodes.Status201Created)
            .ProducesProblem(
                StatusCodes.Status400BadRequest)
            .RequireAuthorization();
    }

    private static async Task<IResult> HandleAsync(
        CreateProjectRequest request,
        ICurrentUser currentUser,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateProjectCommand(
            currentUser.OrganizationId,
            currentUser.UserId,
            request.Name,
            request.Description);

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.Match(
            value => Results.Created(
                $"/api/v1/projects/{value.ProjectId}",
                new CreateProjectResponse(
                    value.ProjectId,
                    value.Name,
                    value.CreatedAt)),
            ApiResults.Problem);
    }
}
```

O endpoint deverá apenas:

- Receber a requisição.
    
- Obter o contexto autenticado.
    
- Criar o Command ou Query.
    
- Invocar a Application.
    
- Converter o resultado para HTTP.
    

---

# 12. Contratos HTTP

Contratos HTTP deverão permanecer no projeto Api.

Exemplo:

```csharp
public sealed record CreateProjectRequest(
    string Name,
    string? Description);
```

```csharp
public sealed record CreateProjectResponse(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt);
```

Não deverão ser utilizados diretamente como:

- Entidades.
    
- Commands distribuídos.
    
- Integration Events.
    
- Objetos de persistência.
    
- Modelos de providers.
    

Cada limite deverá possuir seu próprio contrato quando necessário.

---

# 13. Organização do Projeto Domain

O Domain será organizado por módulos e Aggregates.

Estrutura recomendada:

```text
Domain
│
├── Projects
│   ├── Project.cs
│   ├── ProjectId.cs
│   ├── ProjectName.cs
│   ├── ProjectErrors.cs
│   ├── Events
│   └── Rules
│
├── Pipelines
│   ├── Pipeline.cs
│   ├── PipelineDefinition.cs
│   ├── PipelineStep.cs
│   ├── PipelineId.cs
│   └── Events
│
├── Executions
│   ├── PipelineExecution.cs
│   ├── StepExecution.cs
│   ├── ExecutionStatus.cs
│   ├── ExecutionId.cs
│   └── Events
│
├── Artifacts
│   ├── Artifact.cs
│   ├── ArtifactId.cs
│   ├── ArtifactType.cs
│   └── Events
│
├── Approvals
├── Publications
├── Organizations
└── Common
```

O Domain não será organizado por Commands e Queries.

Sua organização representará conceitos do negócio.

---

# 14. Aggregates

Cada Aggregate deverá possuir uma raiz clara.

Exemplos possíveis:

```text
Project
Pipeline
PipelineExecution
Artifact
Publication
Organization
```

Não deverá existir acesso indiscriminado a entidades internas de um Aggregate.

Exemplo:

```text
PipelineExecution
└── StepExecutions
```

Alterações em `StepExecution` deverão ocorrer por meio de comportamentos do `PipelineExecution` quando fizerem parte do mesmo limite de consistência.

---

# 15. Value Objects

Value Objects deverão ser utilizados quando um conceito possuir:

- Validação própria.
    
- Semântica de negócio.
    
- Imutabilidade.
    
- Comparação por valor.
    

Exemplos:

```text
ProjectName
ContentLanguage
PromptVersion
PipelineVersion
ProviderModel
StorageObjectKey
TokenUsage
Money
```

Não será necessário criar Value Objects para toda string ou número.

O benefício deverá justificar a abstração.

---

# 16. Domain Events

Domain Events deverão ficar próximos do Aggregate que os produz.

Exemplo:

```text
Domain
└── Executions
    └── Events
        ├── PipelineExecutionStartedDomainEvent.cs
        ├── PipelineStepCompletedDomainEvent.cs
        └── PipelineExecutionCompletedDomainEvent.cs
```

Um Domain Event representa algo que ocorreu dentro do domínio.

Ele não é diretamente um contrato de RabbitMQ.

---

# 17. Organização do Projeto Data

O projeto `Data` será organizado parcialmente por feature e parcialmente por capacidade técnica.

Estrutura recomendada:

```text
Data
│
├── Context
│   ├── ApplicationDbContext.cs
│   └── ApplicationDbContextFactory.cs
│
├── Common
│   ├── Converters
│   ├── Interceptors
│   ├── Transactions
│   └── Concurrency
│
├── Features
│   ├── Projects
│   │   ├── ProjectConfiguration.cs
│   │   └── ProjectRepository.cs
│   │
│   ├── Pipelines
│   ├── Executions
│   └── Artifacts
│
├── Outbox
├── Inbox
├── Migrations
└── DependencyInjection.cs
```

Mapeamentos e repositories deverão permanecer próximos ao módulo de negócio correspondente.

Recursos técnicos compartilhados permanecerão em áreas específicas.

---

# 18. Configurações do EF Core

Cada entidade persistida deverá possuir uma configuração própria.

Exemplo:

```text
Data
└── Features
    └── Projects
        └── ProjectConfiguration.cs
```

```csharp
public sealed class ProjectConfiguration
    : IEntityTypeConfiguration<Project>
{
    public void Configure(
        EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");

        builder.HasKey(project => project.Id);

        builder.Property(project => project.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.HasIndex(project => new
        {
            project.OrganizationId,
            project.Name
        });
    }
}
```

Configurações extensas não deverão permanecer dentro do DbContext.

---

# 19. Repositories por Feature

Repositories deverão representar necessidades reais do domínio ou da Application.

Exemplo:

```csharp
public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(
        Guid organizationId,
        Guid projectId,
        CancellationToken cancellationToken);

    Task AddAsync(
        Project project,
        CancellationToken cancellationToken);
}
```

Implementação:

```text
Data
└── Features
    └── Projects
        └── ProjectRepository.cs
```

Não será utilizado um repository genérico como interface principal de todos os Aggregates.

---

# 20. Queries de Leitura

Queries de leitura não precisarão obrigatoriamente utilizar repositories de domínio.

Poderão utilizar abstrações especializadas.

Exemplo:

```csharp
public interface IProjectQueries
{
    Task<ProjectDetails?> GetDetailsAsync(
        Guid organizationId,
        Guid projectId,
        CancellationToken cancellationToken);

    Task<PaginatedResult<ProjectListItem>> ListAsync(
        Guid organizationId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
```

A implementação ficará em Data.

Isso evita carregar Aggregates completos para consultas simples.

---

# 21. Organização do Projeto Infrastructure

O projeto `Infrastructure` será organizado por capacidade externa, e não necessariamente por feature de negócio.

Estrutura recomendada:

```text
Infrastructure
│
├── ArtificialIntelligence
│   ├── Common
│   ├── OpenAI
│   ├── Anthropic
│   ├── Google
│   └── Fake
│
├── Messaging
│   ├── RabbitMQ
│   ├── Serialization
│   ├── Publishing
│   └── Topology
│
├── Caching
│   └── Redis
│
├── Storage
│   └── AzureBlob
│
├── Automation
│   └── N8n
│
├── Integrations
│   ├── YouTube
│   ├── WordPress
│   └── LinkedIn
│
├── Python
├── Webhooks
├── Observability
├── Resilience
└── DependencyInjection.cs
```

A organização por capacidade externa será usada porque uma mesma integração poderá atender múltiplas features.

Por exemplo, o provider de IA poderá ser usado por:

- Research.
    
- Script.
    
- Review.
    
- Title Generation.
    
- Description Generation.
    

---

# 22. Adapters de Providers

Cada provider deverá possuir sua própria implementação isolada.

Exemplo:

```text
Infrastructure
└── ArtificialIntelligence
    └── OpenAI
        ├── OpenAiOptions.cs
        ├── OpenAiClientFactory.cs
        ├── OpenAiTextGenerationProvider.cs
        ├── OpenAiResponseMapper.cs
        └── OpenAiErrorMapper.cs
```

Tipos de SDK não deverão escapar dessa pasta.

A Application deverá trabalhar apenas com modelos próprios.

---

# 23. Organização do Projeto Worker

O Worker será organizado por tipo de processamento e feature.

Estrutura inicial:

```text
Worker
│
├── Common
│   ├── Consumers
│   ├── Execution
│   ├── HealthChecks
│   ├── Middleware
│   └── Shutdown
│
├── Features
│   ├── Pipelines
│   │   ├── StartPipeline
│   │   ├── ExecuteStep
│   │   └── ResumePipeline
│   │
│   ├── Publications
│   └── Artifacts
│
├── BackgroundServices
│   ├── OutboxPublisherService.cs
│   ├── InboxCleanupService.cs
│   └── ExecutionRecoveryService.cs
│
├── Consumers
├── Jobs
├── DependencyInjection.cs
└── Program.cs
```

Consumers deverão permanecer pequenos.

Exemplo:

```csharp
public sealed class PipelineExecutionRequestedConsumer
{
    private readonly ISender _sender;

    public PipelineExecutionRequestedConsumer(
        ISender sender)
    {
        _sender = sender;
    }

    public Task ConsumeAsync(
        PipelineExecutionRequestedV1 message,
        CancellationToken cancellationToken)
    {
        var command = new ExecutePipelineCommand(
            message.OrganizationId,
            message.ExecutionId);

        return _sender.Send(
            command,
            cancellationToken);
    }
}
```

O Consumer deverá adaptar a mensagem e delegar para a Application.

---

# 24. Organização do Projeto Contracts

O projeto `Contracts` será organizado pelo tipo de integração e domínio da mensagem.

Estrutura recomendada:

```text
Contracts
│
├── Messaging
│   ├── Envelopes
│   │   └── MessageEnvelope.cs
│   │
│   ├── Pipelines
│   │   ├── Commands
│   │   │   └── PipelineExecutionRequestedV1.cs
│   │   └── Events
│   │       ├── PipelineExecutionStartedV1.cs
│   │       └── PipelineExecutionCompletedV1.cs
│   │
│   ├── Artifacts
│   └── Publications
│
├── Webhooks
├── Callbacks
└── Common
```

Cada contrato distribuído deverá possuir versão explícita.

---

# 25. Organização do SharedKernel

O `SharedKernel` deverá ser pequeno.

Estrutura inicial:

```text
SharedKernel
│
├── Results
│   ├── Result.cs
│   ├── ResultOfT.cs
│   ├── Error.cs
│   └── ErrorType.cs
│
├── Domain
│   ├── Entity.cs
│   ├── AggregateRoot.cs
│   └── ValueObject.cs
│
├── Time
│   └── IClock.cs
│
├── Identifiers
├── Pagination
└── Primitives
```

Um código somente deverá ser colocado no SharedKernel quando:

- For usado por múltiplos módulos.
    
- Tiver significado estável.
    
- Não pertencer claramente a uma feature.
    
- Não introduzir dependência externa.
    
- Representar um conceito transversal real.
    

Não deverão ser adicionados ao SharedKernel:

- Helpers genéricos.
    
- Extensions sem relação arquitetural.
    
- DTOs de features.
    
- Interfaces de providers.
    
- Contratos de API.
    
- Tipos de banco.
    
- Tipos de mensageria específica.
    
- Clientes HTTP.
    

---

# 26. Namespaces

Namespaces deverão refletir a estrutura de projeto e feature.

Exemplos:

```csharp
InfiniteContentAI.Application.Features.Projects.CreateProject
```

```csharp
InfiniteContentAI.Api.Features.Projects.CreateProject
```

```csharp
InfiniteContentAI.Domain.Projects
```

```csharp
InfiniteContentAI.Data.Features.Projects
```

```csharp
InfiniteContentAI.Infrastructure.ArtificialIntelligence.OpenAI
```

```csharp
InfiniteContentAI.Worker.Features.Pipelines.ExecuteStep
```

Evitar namespaces genéricos como:

```csharp
InfiniteContentAI.Services
InfiniteContentAI.Helpers
InfiniteContentAI.Managers
InfiniteContentAI.Utils
```

---

# 27. Convenção de Nomes

## Commands

```text
CreateProjectCommand
StartPipelineCommand
CancelExecutionCommand
```

## Command Handlers

```text
CreateProjectHandler
StartPipelineHandler
CancelExecutionHandler
```

Não será necessário adicionar `Command` ao nome do Handler se o contexto já estiver claro.

---

## Queries

```text
GetProjectQuery
ListProjectsQuery
GetExecutionQuery
```

## Query Handlers

```text
GetProjectHandler
ListProjectsHandler
GetExecutionHandler
```

---

## Validators

```text
CreateProjectValidator
StartPipelineValidator
```

---

## Results

```text
CreateProjectResult
StartPipelineResult
ProjectDetails
ExecutionDetails
```

---

## Requests e Responses HTTP

```text
CreateProjectRequest
CreateProjectResponse
GetProjectResponse
```

---

## Integration Events

```text
PipelineExecutionStartedV1
PipelineExecutionCompletedV1
ArtifactGeneratedV1
```

O sufixo `Event` poderá ser omitido quando o nome já expressar claramente um fato ocorrido.

---

## Consumers

```text
PipelineExecutionRequestedConsumer
ArtifactGenerationRequestedConsumer
```

---

## Pipeline Steps

```text
ResearchStepHandler
ScriptStepHandler
ReviewStepHandler
```

---

# 28. Dependências Permitidas

## Api

Pode depender de:

```text
Application
Contracts
SharedKernel
Data
Infrastructure
```

A dependência de Data e Infrastructure será usada para composição.

---

## Application

Pode depender de:

```text
Domain
Contracts
SharedKernel
```

Não pode depender de:

```text
Api
Data
Infrastructure
Worker
```

---

## Domain

Pode depender apenas de:

```text
SharedKernel
```

Preferencialmente, deverá possuir o mínimo possível de dependências.

---

## Data

Pode depender de:

```text
Application
Domain
SharedKernel
```

Não pode depender de:

```text
Api
Infrastructure
Worker
```

---

## Infrastructure

Pode depender de:

```text
Application
Domain
Contracts
SharedKernel
```

Não pode depender de:

```text
Api
Data
Worker
```

---

## Worker

Pode depender de:

```text
Application
Contracts
SharedKernel
Data
Infrastructure
```

Data e Infrastructure serão utilizadas para composição e execução.

---

## Contracts

Deverá possuir poucas ou nenhuma dependência interna.

Não poderá depender de:

```text
Domain
Application
Data
Infrastructure
Api
Worker
```

---

## SharedKernel

Não deverá depender de outros projetos da solution.

---

# 29. Testes Organizados por Feature

Os testes deverão espelhar a organização do código.

Estrutura recomendada:

```text
tests
│
├── Domain.UnitTests
│   ├── Projects
│   ├── Pipelines
│   ├── Executions
│   └── Artifacts
│
├── Application.UnitTests
│   └── Features
│       ├── Projects
│       ├── Pipelines
│       └── Executions
│
├── Application.ComponentTests
│   └── Features
│
├── Data.IntegrationTests
│   └── Features
│
├── Api.IntegrationTests
│   └── Features
│
├── Worker.IntegrationTests
│   └── Features
│
└── ArchitectureTests
```

Exemplo:

```text
Application.UnitTests
└── Features
    └── Projects
        └── CreateProject
            ├── CreateProjectHandlerTests.cs
            └── CreateProjectValidatorTests.cs
```

---

# 30. Uma Feature não é um Projeto

Inicialmente, cada feature será uma pasta ou namespace.

Não deverá ser criado um projeto `.csproj` para cada feature.

Estrutura inadequada para o início:

```text
Projects.Application
Projects.Domain
Projects.Infrastructure
Pipelines.Application
Pipelines.Domain
Pipelines.Infrastructure
```

Isso criaria complexidade desnecessária para o MVP.

A separação física em módulos independentes poderá ocorrer futuramente se:

- O sistema crescer significativamente.
    
- Os limites estiverem maduros.
    
- Houver necessidade de deploy separado.
    
- A compilação se tornar problemática.
    
- Equipes independentes assumirem módulos diferentes.
    

---

# 31. Compartilhamento entre Features

Uma feature não deverá acessar diretamente detalhes internos de outra feature.

Exemplo inadequado:

```text
StartPipelineHandler
    ↓
Utiliza diretamente classes internas de CreateProject
```

Quando duas features precisarem colaborar, poderão utilizar:

- Interfaces da Application.
    
- Serviços de domínio.
    
- Domain Events.
    
- Commands internos.
    
- Queries internas.
    
- Modelos compartilhados cuidadosamente definidos.
    

Não deverá ser criado acoplamento circular entre módulos.

---

# 32. Serviços Compartilhados

Serviços compartilhados deverão ser utilizados apenas quando representarem comportamento transversal real.

Exemplos aceitáveis:

```text
ICurrentUser
IClock
IUnitOfWork
IMessagePublisher
IFileStorage
ITextGenerationProvider
```

Exemplos suspeitos:

```text
ProjectManager
PipelineHelper
GeneralService
CommonUtilities
ApplicationManager
```

Classes com nomes genéricos normalmente indicam responsabilidades mal definidas.

---

# 33. Uso de MediatR ou Dispatcher Próprio

A Application poderá utilizar MediatR ou uma abstração equivalente para despacho de Commands, Queries e Notifications.

Exemplo:

```csharp
await sender.Send(command, cancellationToken);
```

O uso do mediator deverá servir para:

- Desacoplar o host do Handler.
    
- Aplicar behaviors.
    
- Padronizar execução.
    
- Centralizar validação.
    
- Centralizar observabilidade.
    
- Centralizar autorização.
    

Não deverá ser utilizado para esconder fluxos difíceis de compreender.

O Handler deverá continuar facilmente localizável pela estrutura da feature.

---

# 34. Pipeline Behaviors

Behaviors transversais poderão ser aplicados ao fluxo da Application.

Ordem conceitual:

```text
Request
    ↓
Logging
    ↓
Correlation
    ↓
Authorization
    ↓
Idempotency
    ↓
Validation
    ↓
Transaction
    ↓
Handler
    ↓
Result
```

A ordem final deverá ser definida cuidadosamente.

Nem todo Command ou Query precisará utilizar todos os behaviors.

---

# 35. Transações

Commands que alteram o banco poderão utilizar um behavior transacional.

Queries não deverão abrir transações de escrita.

Exemplo:

```text
CreateProjectCommand
    ↓
TransactionBehavior
    ↓
CreateProjectHandler
    ↓
SaveChanges
    ↓
Commit
```

Operações externas não deverão ser mantidas dentro de transações longas de banco.

Exemplo inadequado:

```text
Abrir transação
    ↓
Chamar provider de IA por 40 segundos
    ↓
Salvar resultado
    ↓
Commit
```

Para operações longas, o estado deverá ser persistido antes da chamada externa e retomado pelo Worker.

---

# 36. Organização dos Pipeline Steps

As etapas poderão ser organizadas na Application assim:

```text
Application
└── Features
    └── Executions
        └── Steps
            ├── Research
            │   ├── ResearchStepHandler.cs
            │   ├── ResearchStepInput.cs
            │   ├── ResearchStepOutput.cs
            │   └── ResearchStepValidator.cs
            │
            ├── Script
            │   ├── ScriptStepHandler.cs
            │   ├── ScriptStepInput.cs
            │   ├── ScriptStepOutput.cs
            │   └── ScriptStepValidator.cs
            │
            └── Review
```

Alternativamente, caso os Steps se tornem um módulo grande:

```text
Application
└── Features
    └── PipelineSteps
```

A decisão será tomada conforme o crescimento real.

Para o MVP, os Steps ficarão dentro de `Executions`.

---

# 37. Estrutura Inicial do MVP

A estrutura mínima recomendada será:

```text
Api
└── Features
    ├── Projects
    │   ├── CreateProject
    │   └── GetProject
    ├── Pipelines
    │   └── GetPipeline
    └── Executions
        ├── StartExecution
        └── GetExecution
```

```text
Application
└── Features
    ├── Projects
    │   ├── CreateProject
    │   └── GetProject
    ├── Pipelines
    │   └── GetPipeline
    └── Executions
        ├── StartExecution
        ├── GetExecution
        └── Steps
            ├── Research
            └── Script
```

```text
Domain
├── Projects
├── Pipelines
├── Executions
└── Artifacts
```

```text
Data
└── Features
    ├── Projects
    ├── Pipelines
    ├── Executions
    └── Artifacts
```

```text
Infrastructure
├── ArtificialIntelligence
│   ├── Fake
│   └── OpenAI
└── Messaging
    └── RabbitMQ
```

```text
Worker
└── Features
    └── Pipelines
        └── ExecutePipeline
```

---

# 38. Template para Nova Feature

Ao criar uma feature de escrita:

```text
FeatureName
├── FeatureNameCommand.cs
├── FeatureNameHandler.cs
├── FeatureNameValidator.cs
└── FeatureNameResult.cs
```

Ao criar uma feature de leitura:

```text
FeatureName
├── FeatureNameQuery.cs
├── FeatureNameHandler.cs
└── FeatureNameResult.cs
```

Na API:

```text
FeatureName
├── FeatureNameEndpoint.cs
├── FeatureNameRequest.cs
└── FeatureNameResponse.cs
```

Nos testes:

```text
FeatureName
├── FeatureNameHandlerTests.cs
├── FeatureNameValidatorTests.cs
└── FeatureNameEndpointTests.cs
```

Nem todos os arquivos serão obrigatórios.

Uma feature simples não deverá possuir arquivos vazios apenas para seguir o template.

---

# 39. Checklist para Nova Feature

Antes de considerar uma feature concluída, verificar:

- O nome representa uma operação de negócio?
    
- Está no módulo correto?
    
- O Command ou Query possui entrada explícita?
    
- Existe validação de entrada?
    
- A autorização considera tenant e propriedade?
    
- O Handler está pequeno e focado?
    
- Regras de domínio estão no Domain?
    
- A persistência está em Data?
    
- Integrações estão em Infrastructure?
    
- O endpoint está leve?
    
- Entidades não foram expostas?
    
- O resultado utiliza Result Pattern?
    
- CancellationToken foi propagado?
    
- Logs possuem contexto?
    
- Há testes proporcionais ao risco?
    
- A feature é idempotente quando necessário?
    
- Existe tratamento de concorrência?
    
- Não foi criado compartilhamento prematuro?
    
- Os namespaces estão corretos?
    
- As dependências arquiteturais permanecem válidas?
    

---

# 40. Antipadrões

## Pasta Services

Evitar:

```text
Application
└── Services
    ├── ProjectService.cs
    ├── PipelineService.cs
    └── ArtifactService.cs
```

Esses serviços tendem a crescer indefinidamente e misturar casos de uso.

---

## Pasta Helpers

Evitar:

```text
Helpers
Utils
CommonServices
Managers
Processors
```

Esses nomes normalmente não expressam uma responsabilidade clara.

---

## Handler Gigante

Um Handler não deverá:

- Consultar muitos módulos sem coordenação clara.
    
- Chamar diversos providers sequencialmente.
    
- Executar pipeline inteiro.
    
- Implementar regras de domínio.
    
- Controlar manualmente todos os detalhes de infraestrutura.
    

Quando isso ocorrer, o comportamento deverá ser dividido em componentes explícitos.

---

## Feature Compartilhada Genérica

Evitar criar:

```text
Features
└── Common
    └── Everything
```

Código compartilhado deverá possuir um motivo real para existir.

---

## Entidade como DTO

Não retornar diretamente:

```csharp
return project;
```

O retorno deverá utilizar um modelo explícito:

```csharp
return new ProjectDetails(
    project.Id,
    project.Name,
    project.Status);
```

---

## Dependência entre Handlers

Um Handler não deverá instanciar ou chamar outro Handler diretamente.

Exemplo inadequado:

```csharp
var handler = new CreateArtifactHandler(...);
await handler.Handle(...);
```

Quando houver reutilização legítima, extrair:

- Serviço de domínio.
    
- Serviço da Application.
    
- Command despachado.
    
- Componente compartilhado específico.
    

---

# 41. Regras Arquiteturais

1. Organizar por feature dentro dos projetos sempre que aplicável.
    
2. Não criar pastas globais de Commands e Queries.
    
3. Cada caso de uso deve possuir localização previsível.
    
4. Commands alteram estado.
    
5. Queries não alteram estado de negócio.
    
6. Endpoints permanecem leves.
    
7. Consumers permanecem leves.
    
8. Handlers coordenam casos de uso.
    
9. Domain protege invariantes.
    
10. Data implementa persistência.
    
11. Infrastructure implementa integrações.
    
12. Contratos externos não são entidades.
    
13. Features não acessam detalhes internos umas das outras.
    
14. SharedKernel deve permanecer pequeno.
    
15. Não criar abstrações compartilhadas prematuramente.
    
16. Não criar um projeto por feature no MVP.
    
17. Namespaces devem refletir projetos e features.
    
18. Testes devem espelhar a estrutura das features.
    
19. Nenhum Handler deve depender diretamente de SDK externo.
    
20. Nenhum endpoint deve acessar DbContext.
    
21. Nenhum Consumer deve acessar DbContext.
    
22. Nenhuma feature deve ignorar o limite de tenant.
    
23. Operações de I/O devem aceitar CancellationToken.
    
24. Operações longas devem ser delegadas ao Worker.
    
25. Toda nova organização deve favorecer coesão e legibilidade.
    

---

# 42. Sequência de Implementação

A primeira sequência de features será:

```text
1. Projects/CreateProject
2. Projects/GetProject
3. Projects/ListProjects
4. Pipelines/CreatePipeline
5. Pipelines/GetPipeline
6. Executions/StartExecution
7. Executions/GetExecution
8. Executions/Steps/Research
9. Executions/Steps/Script
10. Artifacts/GetArtifact
```

Essa sequência deverá produzir o primeiro fluxo funcional:

```text
Criar projeto
    ↓
Criar pipeline
    ↓
Iniciar execução
    ↓
Gerar pesquisa
    ↓
Gerar roteiro
    ↓
Consultar artefato
```

---

# 43. Documentos Pendentes

Os seguintes documentos de detalhamento já foram criados, mas ainda precisam ser preenchidos:

```text
04 - Backend/API.md
04 - Backend/Application.md
04 - Backend/Domain.md
04 - Backend/Infrastructure.md
04 - Backend/Shared Kernel.md
04 - Backend/Worker.md
```

Também deverão ser criados ou preenchidos:

```text
04 - Backend/Data.md
04 - Backend/Contracts.md
```

Cada um aprofundará as regras introduzidas nesta visão de organização por features.

Ordem recomendada:

```text
1. Domain
2. Application
3. API
4. Data
5. Infrastructure
6. Worker
7. Contracts
8. Shared Kernel
```

A ordem começa pelo núcleo e avança para os detalhes externos.

---

# 44. Filosofia Final

A organização por features deverá facilitar a compreensão do sistema.

Um desenvolvedor deverá ser capaz de localizar uma operação de negócio e encontrar rapidamente:

- Sua entrada.
    
- Sua validação.
    
- Seu Handler.
    
- Seu resultado.
    
- Seu endpoint.
    
- Sua persistência.
    
- Seus testes.
    

A arquitetura não deverá obrigar o desenvolvedor a navegar por diversas pastas técnicas para compreender uma única funcionalidade.

A regra principal será:

> Código que muda junto deve permanecer próximo, sem violar os limites da arquitetura.

Clean Architecture define os limites.

Features definem a organização interna.

As duas abordagens serão utilizadas em conjunto para criar um backend modular, compreensível e preparado para evolução.