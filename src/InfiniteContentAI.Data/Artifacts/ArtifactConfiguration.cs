using InfiniteContentAI.Domain.Artifacts;
using InfiniteContentAI.Domain.Executions;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfiniteContentAI.Data.Artifacts;

internal sealed class ArtifactConfiguration : IEntityTypeConfiguration<Artifact>
{
    public void Configure(EntityTypeBuilder<Artifact> builder)
    {
        builder.ToTable(
            "artifacts",
            table =>
            {
                table.HasCheckConstraint(
                    "ck_artifacts_type_valid",
                    "type IN ('Research', 'Script')");
                table.HasCheckConstraint(
                    "ck_artifacts_content_length",
                    $"char_length(content) BETWEEN 1 AND {Artifact.MaximumContentLength}");
            });
        builder.HasKey(artifact => artifact.Id);

        builder.Property(artifact => artifact.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new ArtifactId(value));
        builder.Property(artifact => artifact.OrganizationId)
            .HasColumnName("organization_id")
            .HasConversion(id => id.Value, value => new OrganizationId(value));
        builder.Property(artifact => artifact.ProjectId)
            .HasColumnName("project_id")
            .HasConversion(id => id.Value, value => new ProjectId(value));
        builder.Property(artifact => artifact.PipelineExecutionId)
            .HasColumnName("pipeline_execution_id")
            .HasConversion(id => id.Value, value => new PipelineExecutionId(value));
        builder.Property(artifact => artifact.StepExecutionId)
            .HasColumnName("step_execution_id")
            .HasConversion(id => id.Value, value => new StepExecutionId(value));
        builder.Property(artifact => artifact.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(32);
        builder.Property(artifact => artifact.Content)
            .HasColumnName("content")
            .HasColumnType("text");
        builder.Property(artifact => artifact.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone");

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(artifact => artifact.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PipelineExecution>()
            .WithMany()
            .HasForeignKey(artifact => artifact.PipelineExecutionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<StepExecution>()
            .WithMany()
            .HasForeignKey(artifact => artifact.StepExecutionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(artifact => artifact.StepExecutionId)
            .IsUnique()
            .HasDatabaseName("ux_artifacts_step_execution_id");
        builder.HasIndex(artifact => new
            {
                artifact.PipelineExecutionId,
                artifact.CreatedAt,
                artifact.Id,
            })
            .HasDatabaseName("ix_artifacts_execution_created_at_id");
        builder.HasIndex(artifact => artifact.ProjectId)
            .HasDatabaseName("ix_artifacts_project_id");
    }
}
