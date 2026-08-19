using InfiniteContentAI.Domain.Artifacts;
using InfiniteContentAI.Domain.Executions;
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

    public DbSet<PipelineExecution> PipelineExecutions => Set<PipelineExecution>();

    public DbSet<StepExecution> StepExecutions => Set<StepExecution>();

    public DbSet<Artifact> Artifacts => Set<Artifact>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
