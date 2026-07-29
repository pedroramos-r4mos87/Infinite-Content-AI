using System.Reflection;

namespace InfiniteContentAI.ArchitectureTests;

public sealed class ProjectDependencyTests
{
    private const string Api = "InfiniteContentAI.Api";
    private const string Application = "InfiniteContentAI.Application";
    private const string Contracts = "InfiniteContentAI.Contracts";
    private const string Data = "InfiniteContentAI.Data";
    private const string Domain = "InfiniteContentAI.Domain";
    private const string Infrastructure = "InfiniteContentAI.Infrastructure";
    private const string SharedKernel = "InfiniteContentAI.SharedKernel";
    private const string Worker = "InfiniteContentAI.Worker";

    private static readonly HashSet<string> InternalProjects =
        new HashSet<string>(
            [
                Api,
                Application,
                Contracts,
                Data,
                Domain,
                Infrastructure,
                SharedKernel,
                Worker,
            ],
            StringComparer.Ordinal);

    [Fact]
    public void SharedKernelDoesNotReferenceInternalProjects()
    {
        Assert.Empty(GetInternalReferences(SharedKernel));
    }

    [Fact]
    public void ContractsDoesNotReferenceInternalProjects()
    {
        Assert.Empty(GetInternalReferences(Contracts));
    }

    [Fact]
    public void DomainReferencesOnlySharedKernel()
    {
        Assert.All(
            GetInternalReferences(Domain),
            reference => Assert.Equal(SharedKernel, reference));
    }

    [Fact]
    public void ApplicationDoesNotReferenceOuterLayers()
    {
        IReadOnlySet<string> references = GetInternalReferences(Application);
        string[] forbiddenReferences =
        [
            Api,
            Data,
            Infrastructure,
            Worker,
        ];

        Assert.DoesNotContain(
            forbiddenReferences,
            references.Contains);
    }

    [Fact]
    public void InfrastructureDoesNotReferenceData()
    {
        Assert.DoesNotContain(
            Data,
            GetInternalReferences(Infrastructure));
    }

    [Fact]
    public void ProductionProjectsDoNotContainCircularReferences()
    {
        var graph = InternalProjects.ToDictionary(
            project => project,
            GetInternalReferences,
            StringComparer.Ordinal);

        foreach (string project in InternalProjects)
        {
            Assert.False(
                HasCycle(
                    project,
                    graph,
                    [],
                    []),
                $"Foi detectada uma referência circular iniciando em {project}.");
        }
    }

    private static IReadOnlySet<string> GetInternalReferences(
        string assemblyName)
    {
        return Assembly
            .Load(assemblyName)
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name => name is not null && InternalProjects.Contains(name))
            .Cast<string>()
            .ToHashSet(StringComparer.Ordinal);
    }

    private static bool HasCycle(
        string project,
        IReadOnlyDictionary<string, IReadOnlySet<string>> graph,
        HashSet<string> visited,
        HashSet<string> activePath)
    {
        if (activePath.Contains(project))
        {
            return true;
        }

        if (!visited.Add(project))
        {
            return false;
        }

        activePath.Add(project);

        foreach (string dependency in graph[project])
        {
            if (HasCycle(
                dependency,
                graph,
                visited,
                activePath))
            {
                return true;
            }
        }

        activePath.Remove(project);
        return false;
    }
}
