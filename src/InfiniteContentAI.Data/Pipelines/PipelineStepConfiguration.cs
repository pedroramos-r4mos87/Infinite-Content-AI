using InfiniteContentAI.Domain.Pipelines;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfiniteContentAI.Data.Pipelines;

internal sealed class PipelineStepConfiguration : IEntityTypeConfiguration<PipelineStep>
{
    public void Configure(EntityTypeBuilder<PipelineStep> builder)
    {
        builder.ToTable(
            "pipeline_steps",
            table => table.HasCheckConstraint(
                "ck_pipeline_steps_position_positive",
                "position > 0"));
        builder.HasKey(step => step.Id);

        builder.Property(step => step.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new PipelineStepId(value));
        builder.Property<PipelineId>("PipelineId")
            .HasColumnName("pipeline_id")
            .HasConversion(id => id.Value, value => new PipelineId(value));
        builder.Property(step => step.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(32);
        builder.Property(step => step.Position)
            .HasColumnName("position");

        builder.HasIndex("PipelineId", nameof(PipelineStep.Position))
            .IsUnique()
            .HasDatabaseName("ux_pipeline_steps_pipeline_position");
        builder.HasIndex("PipelineId", nameof(PipelineStep.Type))
            .IsUnique()
            .HasDatabaseName("ux_pipeline_steps_pipeline_type");
    }
}
