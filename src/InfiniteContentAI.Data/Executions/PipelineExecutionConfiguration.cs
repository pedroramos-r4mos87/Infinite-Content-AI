using InfiniteContentAI.Domain.Executions;
using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfiniteContentAI.Data.Executions;

internal sealed class PipelineExecutionConfiguration
    : IEntityTypeConfiguration<PipelineExecution>
{
    public void Configure(EntityTypeBuilder<PipelineExecution> builder)
    {
        builder.ToTable(
            "pipeline_executions",
            table =>
            {
                table.HasCheckConstraint(
                    "ck_pipeline_executions_pipeline_version_positive",
                    "pipeline_version > 0");
                table.HasCheckConstraint(
                    "ck_pipeline_executions_status_valid",
                    "status IN ('Pending', 'Running', 'Completed', 'Failed')");
            });
        builder.HasKey(execution => execution.Id);

        builder.Property(execution => execution.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new PipelineExecutionId(value));
        builder.Property(execution => execution.OrganizationId)
            .HasColumnName("organization_id")
            .HasConversion(id => id.Value, value => new OrganizationId(value));
        builder.Property(execution => execution.ProjectId)
            .HasColumnName("project_id")
            .HasConversion(id => id.Value, value => new ProjectId(value));
        builder.Property(execution => execution.PipelineId)
            .HasColumnName("pipeline_id")
            .HasConversion(id => id.Value, value => new PipelineId(value));
        builder.Property(execution => execution.PipelineVersion)
            .HasColumnName("pipeline_version");
        builder.Property(execution => execution.Topic)
            .HasColumnName("topic")
            .HasMaxLength(PipelineExecution.MaximumTopicLength);
        builder.Property(execution => execution.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32);
        builder.Property(execution => execution.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone");
        builder.Property(execution => execution.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(PipelineExecution.MaximumCreatedByLength);
        builder.Property(execution => execution.StartedAt)
            .HasColumnName("started_at")
            .HasColumnType("timestamp with time zone");
        builder.Property(execution => execution.CompletedAt)
            .HasColumnName("completed_at")
            .HasColumnType("timestamp with time zone");
        builder.Property(execution => execution.FailedAt)
            .HasColumnName("failed_at")
            .HasColumnType("timestamp with time zone");
        builder.Property(execution => execution.FailureCode)
            .HasColumnName("failure_code")
            .HasMaxLength(PipelineExecution.MaximumFailureCodeLength);
        builder.Property(execution => execution.FailureMessage)
            .HasColumnName("failure_message")
            .HasMaxLength(PipelineExecution.MaximumFailureMessageLength);

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(execution => execution.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Pipeline>()
            .WithMany()
            .HasForeignKey(execution => execution.PipelineId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(execution => execution.Steps)
            .WithOne()
            .HasForeignKey(step => step.PipelineExecutionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(execution => execution.Steps)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(execution => new
            {
                execution.OrganizationId,
                execution.PipelineId,
                execution.CreatedAt,
                execution.Id,
            })
            .HasDatabaseName("ix_pipeline_executions_organization_pipeline_created_at_id");
        builder.HasIndex(execution => new
            {
                execution.OrganizationId,
                execution.ProjectId,
                execution.CreatedAt,
                execution.Id,
            })
            .HasDatabaseName("ix_pipeline_executions_organization_project_created_at_id");
        builder.HasIndex(execution => execution.ProjectId)
            .HasDatabaseName("ix_pipeline_executions_project_id");
        builder.HasIndex(execution => execution.PipelineId)
            .HasDatabaseName("ix_pipeline_executions_pipeline_id");

        builder.Ignore(execution => execution.DomainEvents);
    }
}
