using InfiniteContentAI.Domain.Organizations;
using InfiniteContentAI.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfiniteContentAI.Data.Projects;

internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");
        builder.HasKey(project => project.Id);

        builder.Property(project => project.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new ProjectId(value));
        builder.Property(project => project.OrganizationId)
            .HasColumnName("organization_id")
            .HasConversion(id => id.Value, value => new OrganizationId(value));
        builder.Property(project => project.Name)
            .HasColumnName("name")
            .HasMaxLength(ProjectName.MaximumLength)
            .HasConversion(name => name.Value, value => ProjectName.Create(value).Value);
        builder.Property(project => project.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);
        builder.Property(project => project.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32);
        builder.Property(project => project.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone");
        builder.Property(project => project.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(200);

        builder.HasIndex(project => new { project.OrganizationId, project.CreatedAt, project.Id })
            .HasDatabaseName("ix_projects_organization_created_at_id");

        builder.Ignore(project => project.DomainEvents);
    }
}
