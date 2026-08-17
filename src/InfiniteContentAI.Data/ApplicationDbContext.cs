using InfiniteContentAI.Domain.Pipelines;
using InfiniteContentAI.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace InfiniteContentAI.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<Project> Projects => Set<Project>();

    public DbSet<Pipeline> Pipelines => Set<Pipeline>();

    public DbSet<PipelineStep> PipelineSteps => Set<PipelineStep>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
