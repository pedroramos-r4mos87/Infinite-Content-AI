using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfiniteContentAI.Data.Pipelines;

internal sealed class PipelineConfiguration : IEntityTypeConfiguration<Pipeline>
{
    public void Configure(EntityTypeBuilder<Pipeline> builder)
    {
        builder.ToTable("pipelines");
        builder.HasKey(pipeline => pipeline.Id);

        builder.Property(pipeline => pipeline.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new PipelineId(value));
        builder.Property(pipeline => pipeline.OrganizationId)
            .HasColumnName("organization_id")
            .HasConversion(id => id.Value, value => new OrganizationId(value));
        builder.Property(pipeline => pipeline.ProjectId)
            .HasColumnName("project_id")
            .HasConversion(id => id.Value, value => new ProjectId(value));
        builder.Property(pipeline => pipeline.Name)
            .HasColumnName("name")
            .HasMaxLength(PipelineName.MaximumLength)
            .HasConversion(name => name.Value, value => PipelineName.Create(value).Value);
        builder.Property(pipeline => pipeline.Description)
            .HasColumnName("description")
            .HasMaxLength(Pipeline.MaximumDescriptionLength);
        builder.Property(pipeline => pipeline.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32);
        builder.Property(pipeline => pipeline.Version)
            .HasColumnName("version");
        builder.Property(pipeline => pipeline.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone");
        builder.Property(pipeline => pipeline.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(Pipeline.MaximumCreatedByLength);
        builder.Property(pipeline => pipeline.PublishedAt)
            .HasColumnName("published_at")
            .HasColumnType("timestamp with time zone");

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(pipeline => pipeline.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(pipeline => pipeline.Steps)
            .WithOne()
            .HasForeignKey("PipelineId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(pipeline => pipeline.Steps)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(
                pipeline => new
                {
                    pipeline.OrganizationId,
                    pipeline.ProjectId,
                    pipeline.CreatedAt,
                    pipeline.Id,
                })
            .HasDatabaseName("ix_pipelines_organization_project_created_at_id");
        builder.HasIndex(pipeline => pipeline.ProjectId)
            .HasDatabaseName("ix_pipelines_project_id");

        builder.Ignore(pipeline => pipeline.DomainEvents);
    }
}
