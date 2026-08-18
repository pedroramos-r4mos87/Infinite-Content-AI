using InfiniteContentAI.Domain.Executions;
using InfiniteContentAI.Domain.Pipelines;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfiniteContentAI.Data.Executions;

internal sealed class StepExecutionConfiguration : IEntityTypeConfiguration<StepExecution>
{
    public void Configure(EntityTypeBuilder<StepExecution> builder)
    {
        builder.ToTable(
            "step_executions",
            table =>
            {
                table.HasCheckConstraint(
                    "ck_step_executions_position_positive",
                    "position > 0");
                table.HasCheckConstraint(
                    "ck_step_executions_type_valid",
                    "type IN ('Research', 'Script')");
                table.HasCheckConstraint(
                    "ck_step_executions_status_valid",
                    "status IN ('Pending', 'Running', 'Completed', 'Failed')");
            });
        builder.HasKey(step => step.Id);

        builder.Property(step => step.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new StepExecutionId(value));
        builder.Property(step => step.PipelineExecutionId)
            .HasColumnName("pipeline_execution_id")
            .HasConversion(id => id.Value, value => new PipelineExecutionId(value));
        builder.Property(step => step.PipelineStepId)
            .HasColumnName("pipeline_step_id")
            .HasConversion(id => id.Value, value => new PipelineStepId(value));
        builder.Property(step => step.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(32);
        builder.Property(step => step.Position)
            .HasColumnName("position");
        builder.Property(step => step.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32);
        builder.Property(step => step.StartedAt)
            .HasColumnName("started_at")
            .HasColumnType("timestamp with time zone");
        builder.Property(step => step.CompletedAt)
            .HasColumnName("completed_at")
            .HasColumnType("timestamp with time zone");
        builder.Property(step => step.FailedAt)
            .HasColumnName("failed_at")
            .HasColumnType("timestamp with time zone");
        builder.Property(step => step.FailureCode)
            .HasColumnName("failure_code")
            .HasMaxLength(StepExecution.MaximumFailureCodeLength);
        builder.Property(step => step.FailureMessage)
            .HasColumnName("failure_message")
            .HasMaxLength(StepExecution.MaximumFailureMessageLength);

        builder.HasOne<PipelineStep>()
            .WithMany()
            .HasForeignKey(step => step.PipelineStepId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(step => new { step.PipelineExecutionId, step.Position })
            .IsUnique()
            .HasDatabaseName("ux_step_executions_execution_position");
        builder.HasIndex(step => new { step.PipelineExecutionId, step.PipelineStepId })
            .IsUnique()
            .HasDatabaseName("ux_step_executions_execution_pipeline_step");
        builder.HasIndex(step => step.PipelineStepId)
            .HasDatabaseName("ix_step_executions_pipeline_step_id");
    }
}
