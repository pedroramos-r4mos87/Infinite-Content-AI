# Domain

## 1. Objetivo

O projeto `Domain` representa o núcleo de negócio do Infinite Content AI.

Ele deverá conter os conceitos, comportamentos, regras e invariantes que definem como a plataforma funciona, independentemente de:

- Banco de dados.
    
- Framework web.
    
- Message broker.
    
- Providers de inteligência artificial.
    
- Storage.
    
- Cache.
    
- Serviços externos.
    
- Infraestrutura de nuvem.
    
- Forma de execução do processo.
    

O Domain deverá responder perguntas como:

- O que é um projeto de conteúdo?
    
- O que torna um pipeline válido?
    
- Quando uma execução pode ser iniciada?
    
- Quais transições de estado são permitidas?
    
- Quando uma etapa pode ser concluída?
    
- O que representa um artefato?
    
- Quando um artefato pode ser aprovado?
    
- Quando um conteúdo pode ser publicado?
    
- Quais dados devem permanecer consistentes?
    
- Quais fatos de negócio precisam ser registrados?
    

O Domain não deverá saber se um caso de uso foi iniciado por:

- HTTP.
    
- RabbitMQ.
    
- Worker.
    
- n8n.
    
- Scheduler.
    
- Interface administrativa.
    
- Teste automatizado.
    

Esses são detalhes externos ao modelo de negócio.

---

# 2. Responsabilidades

O projeto `Domain` será responsável por:

- Entidades.
    
- Aggregate Roots.
    
- Value Objects.
    
- Invariantes.
    
- Regras de negócio.
    
- Domain Events.
    
- Domain Services.
    
- Policies.
    
- Specifications de domínio.
    
- Estados e transições.
    
- Erros de negócio.
    
- Identificadores fortemente tipados.
    
- Comportamentos relacionados ao ciclo de vida das entidades.
    

O projeto não será responsável por:

- Commands e Queries.
    
- Handlers.
    
- Controllers.
    
- Endpoints.
    
- DTOs HTTP.
    
- DbContext.
    
- Migrations.
    
- Configurações do EF Core.
    
- Repositories concretos.
    
- Mensageria.
    
- Cache.
    
- Logs.
    
- Telemetria.
    
- Chamadas HTTP.
    
- SDKs de inteligência artificial.
    
- Autenticação.
    
- Serialização de mensagens.
    
- Configuração da aplicação.
    

---

# 3. Dependências Permitidas

O Domain deverá possuir o menor número possível de dependências.

Dependência permitida:

```text
Domain
    ↓
SharedKernel
```

O Domain não poderá depender de:

```text
Application
Api
Data
Infrastructure
Worker
Contracts
```

Também não poderá depender diretamente de:

- ASP.NET Core.
    
- Entity Framework Core.
    
- RabbitMQ.
    
- Redis.
    
- Azure SDK.
    
- OpenAI SDK.
    
- Anthropic SDK.
    
- Google AI SDK.
    
- MediatR.
    
- FluentValidation.
    
- Polly.
    
- OpenTelemetry.
    
- Bibliotecas de serialização específicas.
    

Quando um comportamento depender de algo externo, o Domain deverá modelar apenas a decisão de negócio, deixando a execução técnica para a Application ou Infrastructure.

---

# 4. Princípios do Domain Model

## 4.1 Comportamento próximo dos dados

Entidades não deverão ser apenas estruturas contendo propriedades públicas.

Evitar:

```csharp
public sealed class PipelineExecution
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? CompletedAt { get; set; }
}
```

Preferir:

```csharp
public sealed class PipelineExecution
{
    public ExecutionStatus Status { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public Result Complete(DateTimeOffset completedAt)
    {
        if (Status != ExecutionStatus.Running)
        {
            return Result.Failure(
                ExecutionErrors.InvalidCompletionState(Status));
        }

        Status = ExecutionStatus.Completed;
        CompletedAt = completedAt;

        return Result.Success();
    }
}
```

A entidade deverá controlar suas próprias transições.

---

## 4.2 Estado válido por construção

Sempre que possível, objetos não deverão ser criados em estado inválido.

Construtores públicos sem validação deverão ser evitados.

Preferir:

```csharp
public static Result<Project> Create(
    OrganizationId organizationId,
    ProjectName name,
    string? description,
    UserId createdBy,
    DateTimeOffset createdAt)
{
    if (organizationId == OrganizationId.Empty)
    {
        return Result.Failure<Project>(
            ProjectErrors.OrganizationRequired);
    }

    var project = new Project(
        ProjectId.New(),
        organizationId,
        name,
        description,
        createdBy,
        createdAt);

    return Result.Success(project);
}
```

---

## 4.3 Encapsulamento

Propriedades que representam estado de negócio deverão utilizar setters privados.

Coleções internas não deverão ser expostas como coleções mutáveis.

Exemplo:

```csharp
private readonly List<PipelineStepDefinition> _steps = [];

public IReadOnlyCollection<PipelineStepDefinition> Steps =>
    _steps.AsReadOnly();
```

Alterações deverão ocorrer por métodos de negócio:

```csharp
pipeline.AddStep(...);
pipeline.RemoveStep(...);
pipeline.ReorderStep(...);
```

---

## 4.4 Linguagem de negócio

Métodos deverão expressar ações do domínio.

Preferir:

```csharp
execution.Start(now);
execution.CompleteStep(stepId, output, now);
execution.FailStep(stepId, error, now);
artifact.Approve(userId, now);
pipeline.PublishVersion(now);
```

Evitar:

```csharp
execution.SetStatus(2);
execution.UpdateData(...);
artifact.SetApproved(true);
pipeline.Change(...);
```

---

## 4.5 Domínio determinístico

O Domain não deverá buscar diretamente:

- Horário atual.
    
- Identidade do usuário atual.
    
- Configurações.
    
- Dados externos.
    
- Aleatoriedade.
    
- Valores no banco.
    

Esses valores deverão ser fornecidos explicitamente.

Exemplo:

```csharp
execution.Start(currentUserId, clock.UtcNow);
```

Isso mantém o comportamento testável e previsível.

---

# 5. Linguagem Ubíqua

A equipe deverá utilizar os mesmos termos no código, na documentação e nas discussões de negócio.

## Organization

Representa o limite de propriedade e isolamento dos dados.

Uma Organization poderá representar:

- Empresa.
    
- Equipe.
    
- Conta.
    
- Workspace.
    
- Cliente.
    

---

## Project

Representa um contexto de produção de conteúdo.

Exemplos:

- Canal do YouTube.
    
- Blog.
    
- Podcast.
    
- Marca.
    
- Produto.
    
- Campanha.
    

---

## Pipeline

Representa a definição de um processo de produção de conteúdo.

Ele descreve:

- Quais etapas existem.
    
- Em qual ordem são executadas.
    
- Quais entradas são necessárias.
    
- Quais condições devem ser respeitadas.
    
- Quais versões de configuração são utilizadas.
    

---

## Pipeline Definition

Representa uma versão imutável ou publicável da definição de um pipeline.

Uma execução deverá ser vinculada a uma versão específica da definição.

---

## Pipeline Step

Representa uma etapa configurada dentro de um pipeline.

Exemplos:

- Research.
    
- Outline.
    
- Script.
    
- Review.
    
- Approval.
    
- Image Generation.
    
- Publication.
    

---

## Pipeline Execution

Representa uma execução concreta de uma versão de pipeline.

Uma execução possui:

- Estado.
    
- Etapa atual.
    
- Histórico.
    
- Tentativas.
    
- Checkpoints.
    
- Artefatos.
    
- Erros.
    
- Datas de início e conclusão.
    

---

## Step Execution

Representa a execução concreta de uma etapa dentro de uma Pipeline Execution.

---

## Artifact

Representa um material produzido ou utilizado por uma execução.

Exemplos:

- Pesquisa.
    
- Outline.
    
- Roteiro.
    
- Título.
    
- Descrição.
    
- Imagem.
    
- Áudio.
    
- Vídeo.
    
- Metadados.
    
- Structured Output.
    

---

## Approval

Representa uma decisão humana ou automatizada sobre um artefato ou execução.

---

## Publication

Representa uma tentativa de publicação em um canal externo.

---

## Prompt Version

Identifica a versão do prompt utilizada para produzir um resultado.

---

## Provider Model

Identifica o provider e o modelo utilizados em uma operação de IA.

Exemplos:

```text
OpenAI / gpt-5
Anthropic / claude
Google / gemini
```

O Domain não conhecerá os SDKs desses providers.

---

## Checkpoint

Representa um ponto persistido de progresso que permite retomar uma execução.

---

# 6. Módulos do Domain

Estrutura inicial:

```text
Domain
│
├── Organizations
├── Projects
├── Pipelines
├── Executions
├── Artifacts
├── Approvals
├── Publications
└── Common
```

Para o MVP, os módulos prioritários serão:

```text
Projects
Pipelines
Executions
Artifacts
```

Os módulos `Approvals` e `Publications` poderão ser introduzidos após o fluxo principal estar funcional.

---

# 7. Estrutura Recomendada

```text
Domain
│
├── Organizations
│   ├── Organization.cs
│   ├── OrganizationId.cs
│   ├── OrganizationStatus.cs
│   ├── OrganizationErrors.cs
│   └── Events
│
├── Projects
│   ├── Project.cs
│   ├── ProjectId.cs
│   ├── ProjectName.cs
│   ├── ProjectStatus.cs
│   ├── ProjectErrors.cs
│   └── Events
│
├── Pipelines
│   ├── Pipeline.cs
│   ├── PipelineId.cs
│   ├── PipelineName.cs
│   ├── PipelineVersion.cs
│   ├── PipelineStatus.cs
│   ├── PipelineStepDefinition.cs
│   ├── PipelineStepDefinitionId.cs
│   ├── PipelineStepType.cs
│   ├── PipelineErrors.cs
│   ├── Policies
│   └── Events
│
├── Executions
│   ├── PipelineExecution.cs
│   ├── PipelineExecutionId.cs
│   ├── PipelineExecutionStatus.cs
│   ├── StepExecution.cs
│   ├── StepExecutionId.cs
│   ├── StepExecutionStatus.cs
│   ├── ExecutionCheckpoint.cs
│   ├── ExecutionErrors.cs
│   ├── Policies
│   └── Events
│
├── Artifacts
│   ├── Artifact.cs
│   ├── ArtifactId.cs
│   ├── ArtifactType.cs
│   ├── ArtifactStatus.cs
│   ├── ArtifactContent.cs
│   ├── ArtifactErrors.cs
│   └── Events
│
├── Approvals
│   ├── Approval.cs
│   ├── ApprovalId.cs
│   ├── ApprovalDecision.cs
│   ├── ApprovalStatus.cs
│   └── Events
│
├── Publications
│   ├── Publication.cs
│   ├── PublicationId.cs
│   ├── PublicationStatus.cs
│   ├── PublicationTarget.cs
│   └── Events
│
└── Common
    ├── Errors
    ├── Policies
    └── Specifications
```

Elementos realmente genéricos, como `Entity`, `AggregateRoot` e `Result`, deverão permanecer no `SharedKernel`.

---

# 8. Entidades

Uma Entity possui:

- Identidade própria.
    
- Ciclo de vida.
    
- Estado mutável controlado.
    
- Comparação por identidade.
    

Exemplos:

```text
Project
Pipeline
PipelineStepDefinition
PipelineExecution
StepExecution
Artifact
Approval
Publication
Organization
```

Duas entidades com os mesmos dados, mas identificadores diferentes, representam objetos diferentes.

Exemplo:

```csharp
var projectA = Project.Create(...);
var projectB = Project.Create(...);
```

Mesmo que ambos possuam o nome `"Canal de Tecnologia"`, continuam sendo projetos diferentes.

---

# 9. Classe Base de Entidade

A classe base deverá permanecer mínima.

Exemplo conceitual no SharedKernel:

```csharp
public abstract class Entity<TId>
    where TId : notnull
{
    protected Entity(TId id)
    {
        Id = id;
    }

    public TId Id { get; protected init; }

    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> entity &&
               EqualityComparer<TId>.Default.Equals(Id, entity.Id);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<TId>.Default.GetHashCode(Id);
    }
}
```

A classe base não deverá conter automaticamente:

- CreatedAt.
    
- UpdatedAt.
    
- DeletedAt.
    
- OrganizationId.
    
- Status.
    
- Auditoria.
    
- Métodos de persistência.
    

Essas propriedades deverão existir somente quando fizerem sentido para o conceito.

---

# 10. Aggregate Roots

Aggregate Root é a entrada oficial para alteração de um conjunto consistente de objetos.

No Infinite Content AI, os principais Aggregates iniciais serão:

```text
Organization
Project
Pipeline
PipelineExecution
Artifact
Approval
Publication
```

A raiz deverá proteger as invariantes do conjunto.

Exemplo:

```text
PipelineExecution
└── StepExecutions
```

Uma `StepExecution` não deverá ser alterada diretamente por código externo.

Preferir:

```csharp
execution.StartStep(stepDefinitionId, now);
execution.CompleteStep(stepExecutionId, outputReference, now);
execution.FailStep(stepExecutionId, error, now);
```

Evitar:

```csharp
stepExecution.Status = StepExecutionStatus.Completed;
```

---

# 11. Limites dos Aggregates

## Project Aggregate

Responsável por:

- Identidade do projeto.
    
- Organization proprietária.
    
- Nome.
    
- Descrição.
    
- Estado.
    
- Dados básicos do contexto de conteúdo.
    
- Ativação e arquivamento.
    

Não deverá conter diretamente:

- Todas as execuções.
    
- Todos os artefatos.
    
- Todos os pipelines.
    
- Histórico completo.
    
- Dados de publicação.
    

Esses elementos serão referenciados por identificadores.

---

## Pipeline Aggregate

Responsável por:

- Definição do pipeline.
    
- Nome.
    
- Estado.
    
- Versão.
    
- Etapas.
    
- Ordem das etapas.
    
- Validade estrutural.
    
- Publicação de uma versão.
    

O Pipeline poderá conter `PipelineStepDefinition` como entidade interna.

---

## PipelineExecution Aggregate

Responsável por:

- Estado geral da execução.
    
- Etapas executadas.
    
- Etapa atual.
    
- Tentativas.
    
- Checkpoints.
    
- Cancelamento.
    
- Falha.
    
- Conclusão.
    
- Versão de pipeline utilizada.
    

No MVP, `StepExecution` poderá permanecer dentro do Aggregate.

Caso uma execução passe a possuir centenas ou milhares de etapas, esse limite deverá ser reavaliado.

---

## Artifact Aggregate

Responsável por:

- Tipo do artefato.
    
- Origem.
    
- Versão.
    
- Conteúdo ou referência de storage.
    
- Estado.
    
- Aprovação.
    
- Integridade.
    
- Relação com execução e etapa.
    

Artefatos deverão ser separados da PipelineExecution para evitar que o Aggregate de execução cresça excessivamente.

---

# 12. Identificadores Fortemente Tipados

Identificadores deverão expressar seu significado.

Evitar:

```csharp
Guid projectId;
Guid pipelineId;
Guid executionId;
```

Preferir:

```csharp
ProjectId projectId;
PipelineId pipelineId;
PipelineExecutionId executionId;
```

Exemplo:

```csharp
public readonly record struct ProjectId(Guid Value)
{
    public static ProjectId New()
    {
        return new ProjectId(Guid.CreateVersion7());
    }

    public static readonly ProjectId Empty = new(Guid.Empty);
}
```

Outros identificadores:

```text
OrganizationId
UserId
ProjectId
PipelineId
PipelineStepDefinitionId
PipelineExecutionId
StepExecutionId
ArtifactId
ApprovalId
PublicationId
```

Benefícios:

- Evita troca acidental de identificadores.
    
- Melhora legibilidade.
    
- Melhora contratos internos.
    
- Expressa intenção.
    
- Facilita validações específicas.
    

---

# 13. Value Objects

Value Objects representam conceitos identificados por seus valores.

Características:

- Imutáveis.
    
- Comparados por valor.
    
- Sem identidade própria.
    
- Validados na criação.
    
- Expressam significado de negócio.
    

Exemplos:

```text
ProjectName
PipelineName
PipelineVersion
PromptVersion
ContentLanguage
ProviderModel
TokenUsage
Money
StorageReference
ArtifactContent
ExecutionError
```

---

# 14. ProjectName

Exemplo:

```csharp
public sealed record ProjectName
{
    public const int MaximumLength = 150;

    private ProjectName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<ProjectName> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<ProjectName>(
                ProjectErrors.NameRequired);
        }

        var normalized = value.Trim();

        if (normalized.Length > MaximumLength)
        {
            return Result.Failure<ProjectName>(
                ProjectErrors.NameTooLong(MaximumLength));
        }

        return Result.Success(
            new ProjectName(normalized));
    }

    public override string ToString() => Value;
}
```

A criação deverá normalizar o valor quando isso for uma regra estável.

---

# 15. PipelineVersion

A versão de pipeline deverá ser explícita.

Exemplo:

```csharp
public readonly record struct PipelineVersion(int Value)
{
    public static PipelineVersion Initial => new(1);

    public PipelineVersion Next()
    {
        return new PipelineVersion(checked(Value + 1));
    }
}
```

Uma execução deverá registrar a versão da definição utilizada.

Isso garante que mudanças futuras no Pipeline não alterem retroativamente execuções anteriores.

---

# 16. Enums e Value Objects

Enums serão apropriados quando o conjunto de valores for:

- Pequeno.
    
- Fechado.
    
- Estável.
    
- Sem comportamento complexo.
    

Exemplo:

```csharp
public enum ProjectStatus
{
    Active = 1,
    Archived = 2
}
```

Value Objects ou classes serão preferidos quando houver:

- Dados adicionais.
    
- Validação.
    
- Comportamento.
    
- Evolução frequente.
    
- Valores configuráveis.
    

Exemplo:

```csharp
public sealed record PipelineStepType
{
    public static readonly PipelineStepType Research = new("research");
    public static readonly PipelineStepType Script = new("script");

    public string Value { get; }

    private PipelineStepType(string value)
    {
        Value = value;
    }
}
```

Para o MVP, tipos de etapa poderão ser strings controladas ou Value Objects extensíveis, evitando um enum rígido que exija alterar o Domain sempre que um novo plugin de etapa for introduzido.

---

# 17. Domain Events

Domain Events representam fatos relevantes que ocorreram no domínio.

Exemplos:

```text
ProjectCreatedDomainEvent
ProjectArchivedDomainEvent
PipelinePublishedDomainEvent
PipelineExecutionQueuedDomainEvent
PipelineExecutionStartedDomainEvent
PipelineStepStartedDomainEvent
PipelineStepCompletedDomainEvent
PipelineStepFailedDomainEvent
PipelineExecutionCompletedDomainEvent
PipelineExecutionFailedDomainEvent
ArtifactCreatedDomainEvent
ArtifactApprovedDomainEvent
```

Um Domain Event:

- É gerado pelo Aggregate.
    
- Representa algo que já aconteceu.
    
- Utiliza linguagem de negócio.
    
- Não conhece RabbitMQ.
    
- Não é necessariamente publicado externamente.
    
- Pode originar um Integration Event posteriormente.
    

---

# 18. Estrutura de Domain Event

Interface conceitual no SharedKernel:

```csharp
public interface IDomainEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredAt { get; }
}
```

Exemplo:

```csharp
public sealed record PipelineExecutionStartedDomainEvent(
    Guid EventId,
    PipelineExecutionId ExecutionId,
    PipelineId PipelineId,
    OrganizationId OrganizationId,
    DateTimeOffset OccurredAt)
    : IDomainEvent;
```

O evento deverá carregar somente os dados necessários para representar o fato.

Entidades completas não deverão ser colocadas no evento.

---

# 19. Registro de Domain Events

Aggregate Root conceitual:

```csharp
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot(TId id)
        : base(id)
    {
    }

    public IReadOnlyCollection<IDomainEvent> DomainEvents =>
        _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

O Domain apenas registra os eventos.

A captura, persistência e despacho serão responsabilidades de outras camadas.

---

# 20. Domain Event x Integration Event

Domain Event:

```text
PipelineExecutionCompletedDomainEvent
```

Integration Event:

```text
PipelineExecutionCompletedV1
```

O Domain Event representa o fato interno.

O Integration Event representa um contrato distribuído.

Fluxo:

```text
Aggregate
    ↓
Domain Event
    ↓
Application ou Data
    ↓
Outbox
    ↓
Integration Event
    ↓
RabbitMQ
```

O Domain não deverá depender do projeto `Contracts`.

---

# 21. Result Pattern no Domain

Falhas esperadas deverão ser representadas por `Result`.

Exemplos:

- Estado inválido.
    
- Nome inválido.
    
- Pipeline sem etapas.
    
- Etapa duplicada.
    
- Execução já concluída.
    
- Cancelamento não permitido.
    
- Artefato já aprovado.
    

Exemplo:

```csharp
public Result Archive(
    UserId archivedBy,
    DateTimeOffset archivedAt)
{
    if (Status == ProjectStatus.Archived)
    {
        return Result.Failure(
            ProjectErrors.AlreadyArchived(Id));
    }

    Status = ProjectStatus.Archived;
    ArchivedBy = archivedBy;
    ArchivedAt = archivedAt;

    RaiseDomainEvent(
        new ProjectArchivedDomainEvent(
            Guid.CreateVersion7(),
            Id,
            OrganizationId,
            archivedAt));

    return Result.Success();
}
```

Exceções deverão ser reservadas para:

- Violação impossível de invariante interna.
    
- Estado corrompido.
    
- Uso incorreto de API interna.
    
- Erros de programação.
    

---

# 22. Erros de Domínio

Erros deverão possuir:

- Código estável.
    
- Mensagem compreensível.
    
- Tipo.
    
- Contexto mínimo.
    

Exemplo:

```csharp
public static class ProjectErrors
{
    public static readonly Error NameRequired = Error.Validation(
        "Project.NameRequired",
        "O nome do projeto é obrigatório.");

    public static Error NameTooLong(int maximumLength) =>
        Error.Validation(
            "Project.NameTooLong",
            $"O nome do projeto deve possuir no máximo {maximumLength} caracteres.");

    public static Error AlreadyArchived(ProjectId projectId) =>
        Error.Conflict(
            "Project.AlreadyArchived",
            $"O projeto '{projectId}' já está arquivado.");
}
```

O código do erro deverá ser estável, mesmo que a mensagem mude.

A Application e a API poderão mapear o tipo do erro para respostas apropriadas.

---

# 23. Project Aggregate

Estrutura inicial:

```csharp
public sealed class Project : AggregateRoot<ProjectId>
{
    private Project(
        ProjectId id,
        OrganizationId organizationId,
        ProjectName name,
        string? description,
        UserId createdBy,
        DateTimeOffset createdAt)
        : base(id)
    {
        OrganizationId = organizationId;
        Name = name;
        Description = description;
        Status = ProjectStatus.Active;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
    }

    private Project()
        : base(ProjectId.Empty)
    {
    }

    public OrganizationId OrganizationId { get; private set; }

    public ProjectName Name { get; private set; }

    public string? Description { get; private set; }

    public ProjectStatus Status { get; private set; }

    public UserId CreatedBy { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public UserId? UpdatedBy { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public UserId? ArchivedBy { get; private set; }

    public DateTimeOffset? ArchivedAt { get; private set; }
}
```

O construtor privado sem parâmetros poderá existir exclusivamente para materialização pelo ORM, sem introduzir dependência do EF Core.

---

# 24. Criação de Project

```csharp
public static Result<Project> Create(
    OrganizationId organizationId,
    ProjectName name,
    string? description,
    UserId createdBy,
    DateTimeOffset createdAt)
{
    if (organizationId == OrganizationId.Empty)
    {
        return Result.Failure<Project>(
            ProjectErrors.OrganizationRequired);
    }

    if (createdBy == UserId.Empty)
    {
        return Result.Failure<Project>(
            ProjectErrors.CreatorRequired);
    }

    var project = new Project(
        ProjectId.New(),
        organizationId,
        name,
        NormalizeDescription(description),
        createdBy,
        createdAt);

    project.RaiseDomainEvent(
        new ProjectCreatedDomainEvent(
            Guid.CreateVersion7(),
            project.Id,
            organizationId,
            createdBy,
            createdAt));

    return Result.Success(project);
}
```

---

# 25. Invariantes de Project

Regras iniciais:

1. Todo Project pertence a uma Organization.
    
2. O nome é obrigatório.
    
3. O nome possui tamanho máximo.
    
4. Um Project arquivado não pode ser alterado.
    
5. Arquivamento deve registrar usuário e data.
    
6. Um Project arquivado não pode iniciar novas execuções.
    
7. Um Project não pode mudar de Organization.
    
8. O identificador é imutável.
    
9. A data de criação é imutável.
    
10. O criador é imutável.
    

---

# 26. Pipeline Aggregate

O Pipeline representa uma definição editável e versionável.

Estrutura conceitual:

```csharp
public sealed class Pipeline : AggregateRoot<PipelineId>
{
    private readonly List<PipelineStepDefinition> _steps = [];

    public OrganizationId OrganizationId { get; private set; }

    public ProjectId ProjectId { get; private set; }

    public PipelineName Name { get; private set; }

    public PipelineStatus Status { get; private set; }

    public PipelineVersion Version { get; private set; }

    public IReadOnlyCollection<PipelineStepDefinition> Steps =>
        _steps.AsReadOnly();

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }
}
```

---

# 27. PipelineStepDefinition

Uma etapa da definição poderá conter:

```csharp
public sealed class PipelineStepDefinition
    : Entity<PipelineStepDefinitionId>
{
    public PipelineStepType Type { get; private set; }

    public string Name { get; private set; }

    public int Position { get; private set; }

    public bool IsRequired { get; private set; }

    public string ConfigurationSchemaVersion { get; private set; }

    public string ConfigurationJson { get; private set; }
}
```

O Domain não deverá interpretar detalhes técnicos do JSON de configuração de cada provider.

Ele deverá proteger regras estruturais como:

- Etapa possui tipo.
    
- Nome é válido.
    
- Posição é válida.
    
- Não existem posições duplicadas.
    
- Schema possui versão.
    
- Configuração não está ausente quando obrigatória.
    

A validação profunda da configuração específica poderá ocorrer na Application por meio do adapter correspondente.

---

# 28. Invariantes de Pipeline

1. Todo Pipeline pertence a um Project.
    
2. Project e Pipeline devem pertencer à mesma Organization.
    
3. Nome é obrigatório.
    
4. Pipeline publicado deve possuir pelo menos uma etapa.
    
5. Posições de etapas não podem se repetir.
    
6. A ordem deve ser contínua.
    
7. Uma etapa obrigatória não pode possuir configuração vazia.
    
8. Pipeline arquivado não pode ser executado.
    
9. Alterações relevantes geram uma nova versão.
    
10. Uma versão utilizada por uma execução não pode ser alterada retroativamente.
    
11. Uma etapa não pode depender de si própria.
    
12. Dependências entre etapas não podem formar ciclos.
    
13. Pipeline deve possuir uma única etapa inicial quando utilizar fluxo linear.
    
14. Pipeline publicado deve ser estruturalmente válido.
    

---

# 29. Adição de Etapa

```csharp
public Result<PipelineStepDefinitionId> AddStep(
    PipelineStepType type,
    string name,
    bool isRequired,
    string configurationSchemaVersion,
    string configurationJson,
    DateTimeOffset changedAt)
{
    if (Status == PipelineStatus.Archived)
    {
        return Result.Failure<PipelineStepDefinitionId>(
            PipelineErrors.ArchivedPipelineCannotBeChanged(Id));
    }

    var normalizedName = name.Trim();

    if (string.IsNullOrWhiteSpace(normalizedName))
    {
        return Result.Failure<PipelineStepDefinitionId>(
            PipelineErrors.StepNameRequired);
    }

    var stepId = PipelineStepDefinitionId.New();
    var nextPosition = _steps.Count + 1;

    _steps.Add(
        PipelineStepDefinition.Create(
            stepId,
            type,
            normalizedName,
            nextPosition,
            isRequired,
            configurationSchemaVersion,
            configurationJson));

    Version = Version.Next();
    UpdatedAt = changedAt;

    return Result.Success(stepId);
}
```

---

# 30. Publicação de Pipeline

Publicar um Pipeline significa tornar uma versão apta a ser executada.

```csharp
public Result Publish(
    UserId publishedBy,
    DateTimeOffset publishedAt)
{
    if (Status == PipelineStatus.Archived)
    {
        return Result.Failure(
            PipelineErrors.ArchivedPipelineCannotBePublished(Id));
    }

    if (_steps.Count == 0)
    {
        return Result.Failure(
            PipelineErrors.PipelineRequiresSteps(Id));
    }

    var validationResult = ValidateStructure();

    if (validationResult.IsFailure)
    {
        return validationResult;
    }

    Status = PipelineStatus.Published;
    PublishedBy = publishedBy;
    PublishedAt = publishedAt;

    RaiseDomainEvent(
        new PipelinePublishedDomainEvent(
            Guid.CreateVersion7(),
            Id,
            ProjectId,
            OrganizationId,
            Version,
            publishedAt));

    return Result.Success();
}
```

---

# 31. Imutabilidade de Versões Publicadas

Uma versão publicada não deverá ser modificada em uso.

Existem duas estratégias possíveis:

## Cópia da definição

Ao editar um Pipeline publicado, uma nova versão editável é criada.

```text
Pipeline v1 — Published
Pipeline v2 — Draft
```

## Snapshot na execução

A execução armazena um snapshot completo da definição usada.

Para o MVP, poderá ser utilizada uma combinação:

- Pipeline possui versão.
    
- PipelineExecution registra a versão.
    
- A definição utilizada é persistida de forma imutável ou como snapshot.
    

A implementação de persistência será definida em `Data.md`.

A regra de domínio permanece:

> Uma execução sempre deve ser reproduzível a partir da versão registrada.

---

# 32. PipelineExecution Aggregate

Estrutura inicial:

```csharp
public sealed class PipelineExecution
    : AggregateRoot<PipelineExecutionId>
{
    private readonly List<StepExecution> _steps = [];

    public OrganizationId OrganizationId { get; private set; }

    public ProjectId ProjectId { get; private set; }

    public PipelineId PipelineId { get; private set; }

    public PipelineVersion PipelineVersion { get; private set; }

    public PipelineExecutionStatus Status { get; private set; }

    public UserId RequestedBy { get; private set; }

    public DateTimeOffset RequestedAt { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public DateTimeOffset? CancelledAt { get; private set; }

    public ExecutionError? Failure { get; private set; }

    public IReadOnlyCollection<StepExecution> Steps =>
        _steps.AsReadOnly();
}
```

---

# 33. Estados da PipelineExecution

Estados iniciais:

```csharp
public enum PipelineExecutionStatus
{
    Queued = 1,
    Running = 2,
    AwaitingApproval = 3,
    Paused = 4,
    Completed = 5,
    Failed = 6,
    Cancelled = 7
}
```

Fluxo principal:

```text
Queued
    ↓
Running
    ↓
Completed
```

Fluxos alternativos:

```text
Running → AwaitingApproval → Running
Running → Paused → Running
Queued → Cancelled
Running → Cancelled
Running → Failed
AwaitingApproval → Cancelled
```

Transições inválidas deverão ser rejeitadas.

Exemplos:

```text
Completed → Running
Cancelled → Running
Failed → Completed
Completed → Cancelled
```

Uma retomada de execução falhada deverá ser modelada explicitamente, por exemplo:

- Criar nova tentativa.
    
- Reabrir execução com política definida.
    
- Criar uma nova execução vinculada à anterior.
    

Não deverá ocorrer por simples alteração de status.

---

# 34. Criação de PipelineExecution

```csharp
public static Result<PipelineExecution> Queue(
    OrganizationId organizationId,
    ProjectId projectId,
    PipelineId pipelineId,
    PipelineVersion pipelineVersion,
    UserId requestedBy,
    DateTimeOffset requestedAt,
    IReadOnlyCollection<PipelineStepSnapshot> steps)
{
    if (steps.Count == 0)
    {
        return Result.Failure<PipelineExecution>(
            ExecutionErrors.PipelineHasNoSteps);
    }

    var execution = new PipelineExecution(
        PipelineExecutionId.New(),
        organizationId,
        projectId,
        pipelineId,
        pipelineVersion,
        requestedBy,
        requestedAt);

    foreach (var step in steps.OrderBy(step => step.Position))
    {
        execution._steps.Add(
            StepExecution.CreatePending(
                StepExecutionId.New(),
                step.StepDefinitionId,
                step.Type,
                step.Position));
    }

    execution.RaiseDomainEvent(
        new PipelineExecutionQueuedDomainEvent(
            Guid.CreateVersion7(),
            execution.Id,
            pipelineId,
            organizationId,
            requestedAt));

    return Result.Success(execution);
}
```

---

# 35. Início da Execução

```csharp
public Result Start(DateTimeOffset startedAt)
{
    if (Status != PipelineExecutionStatus.Queued)
    {
        return Result.Failure(
            ExecutionErrors.CannotStartFromState(Id, Status));
    }

    Status = PipelineExecutionStatus.Running;
    StartedAt = startedAt;

    RaiseDomainEvent(
        new PipelineExecutionStartedDomainEvent(
            Guid.CreateVersion7(),
            Id,
            PipelineId,
            OrganizationId,
            startedAt));

    return Result.Success();
}
```

---

# 36. StepExecution

```csharp
public sealed class StepExecution
    : Entity<StepExecutionId>
{
    public PipelineStepDefinitionId StepDefinitionId { get; private set; }

    public PipelineStepType Type { get; private set; }

    public int Position { get; private set; }

    public StepExecutionStatus Status { get; private set; }

    public int AttemptCount { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public ArtifactId? OutputArtifactId { get; private set; }

    public ExecutionError? Failure { get; private set; }
}
```

Estados:

```csharp
public enum StepExecutionStatus
{
    Pending = 1,
    Running = 2,
    AwaitingApproval = 3,
    Completed = 4,
    Failed = 5,
    Skipped = 6,
    Cancelled = 7
}
```

---

# 37. Invariantes de StepExecution

1. Uma etapa concluída não pode iniciar novamente.
    
2. Uma etapa cancelada não pode ser concluída.
    
3. Uma etapa pendente pode iniciar.
    
4. Uma etapa falhada poderá iniciar novamente somente se a política permitir retry.
    
5. `AttemptCount` aumenta quando uma tentativa começa.
    
6. Uma etapa concluída registra data de conclusão.
    
7. Uma etapa falhada registra erro.
    
8. Uma etapa concluída não mantém erro ativo.
    
9. Uma etapa que produz saída deve registrar o Artifact correspondente.
    
10. Posição é imutável durante uma execução.
    
11. Tipo é imutável durante uma execução.
    
12. A definição da etapa é imutável durante uma execução.
    

---

# 38. Início de uma Etapa

A alteração deverá ocorrer pela raiz:

```csharp
public Result<StepExecutionId> StartNextStep(
    DateTimeOffset startedAt)
{
    if (Status != PipelineExecutionStatus.Running)
    {
        return Result.Failure<StepExecutionId>(
            ExecutionErrors.NotRunning(Id, Status));
    }

    if (_steps.Any(step => step.Status == StepExecutionStatus.Running))
    {
        return Result.Failure<StepExecutionId>(
            ExecutionErrors.StepAlreadyRunning(Id));
    }

    var nextStep = _steps
        .Where(step => step.Status == StepExecutionStatus.Pending)
        .OrderBy(step => step.Position)
        .FirstOrDefault();

    if (nextStep is null)
    {
        return Result.Failure<StepExecutionId>(
            ExecutionErrors.NoPendingSteps(Id));
    }

    var startResult = nextStep.Start(startedAt);

    if (startResult.IsFailure)
    {
        return Result.Failure<StepExecutionId>(
            startResult.Error);
    }

    RaiseDomainEvent(
        new PipelineStepStartedDomainEvent(
            Guid.CreateVersion7(),
            Id,
            nextStep.Id,
            nextStep.Type,
            OrganizationId,
            startedAt));

    return Result.Success(nextStep.Id);
}
```

---

# 39. Conclusão de uma Etapa

```csharp
public Result CompleteStep(
    StepExecutionId stepExecutionId,
    ArtifactId? outputArtifactId,
    DateTimeOffset completedAt)
{
    if (Status != PipelineExecutionStatus.Running)
    {
        return Result.Failure(
            ExecutionErrors.NotRunning(Id, Status));
    }

    var step = _steps.SingleOrDefault(
        item => item.Id == stepExecutionId);

    if (step is null)
    {
        return Result.Failure(
            ExecutionErrors.StepNotFound(
                Id,
                stepExecutionId));
    }

    var result = step.Complete(
        outputArtifactId,
        completedAt);

    if (result.IsFailure)
    {
        return result;
    }

    RaiseDomainEvent(
        new PipelineStepCompletedDomainEvent(
            Guid.CreateVersion7(),
            Id,
            step.Id,
            step.Type,
            outputArtifactId,
            OrganizationId,
            completedAt));

    if (_steps.All(item =>
            item.Status is StepExecutionStatus.Completed
                or StepExecutionStatus.Skipped))
    {
        CompleteExecution(completedAt);
    }

    return Result.Success();
}
```

---

# 40. Falha de uma Etapa

```csharp
public Result FailStep(
    StepExecutionId stepExecutionId,
    ExecutionError error,
    bool retryAllowed,
    DateTimeOffset failedAt)
{
    var step = FindStep(stepExecutionId);

    if (step is null)
    {
        return Result.Failure(
            ExecutionErrors.StepNotFound(
                Id,
                stepExecutionId));
    }

    var result = step.Fail(error, failedAt);

    if (result.IsFailure)
    {
        return result;
    }

    RaiseDomainEvent(
        new PipelineStepFailedDomainEvent(
            Guid.CreateVersion7(),
            Id,
            step.Id,
            step.Type,
            error.Code,
            retryAllowed,
            OrganizationId,
            failedAt));

    if (!retryAllowed)
    {
        FailExecution(error, failedAt);
    }

    return Result.Success();
}
```

A decisão de quantas retentativas executar poderá depender de uma `RetryPolicy` calculada pela Application ou por uma Policy de domínio.

O Domain deverá controlar se a transição é permitida.

O Worker será responsável por aguardar e reagendar a execução.

---

# 41. Conclusão da PipelineExecution

```csharp
private void CompleteExecution(
    DateTimeOffset completedAt)
{
    Status = PipelineExecutionStatus.Completed;
    CompletedAt = completedAt;
    Failure = null;

    RaiseDomainEvent(
        new PipelineExecutionCompletedDomainEvent(
            Guid.CreateVersion7(),
            Id,
            PipelineId,
            ProjectId,
            OrganizationId,
            completedAt));
}
```

Invariantes:

1. Todas as etapas obrigatórias devem estar concluídas.
    
2. Nenhuma etapa pode estar em execução.
    
3. Nenhuma etapa obrigatória pode estar falhada.
    
4. A execução deve estar em estado Running.
    
5. `CompletedAt` deve ser preenchido.
    
6. Uma execução concluída torna-se terminal.
    

---

# 42. Cancelamento

```csharp
public Result Cancel(
    UserId cancelledBy,
    string? reason,
    DateTimeOffset cancelledAt)
{
    if (Status is PipelineExecutionStatus.Completed
        or PipelineExecutionStatus.Cancelled)
    {
        return Result.Failure(
            ExecutionErrors.CannotCancelFromState(
                Id,
                Status));
    }

    Status = PipelineExecutionStatus.Cancelled;
    CancelledBy = cancelledBy;
    CancellationReason = NormalizeReason(reason);
    CancelledAt = cancelledAt;

    foreach (var step in _steps.Where(
                 step => step.Status is
                     StepExecutionStatus.Pending or
                     StepExecutionStatus.Running or
                     StepExecutionStatus.AwaitingApproval))
    {
        step.Cancel(cancelledAt);
    }

    RaiseDomainEvent(
        new PipelineExecutionCancelledDomainEvent(
            Guid.CreateVersion7(),
            Id,
            cancelledBy,
            OrganizationId,
            cancelledAt));

    return Result.Success();
}
```

O Domain registra a intenção e altera o estado.

O cancelamento técnico de chamadas externas será realizado por `CancellationToken` no Worker.

---

# 43. Idempotência no Domain

A idempotência técnica completa será tratada em Application, Data e Worker.

Entretanto, o Domain deverá tornar operações repetidas previsíveis.

Exemplo:

- Concluir etapa já concluída com os mesmos dados pode retornar sucesso idempotente.
    
- Concluir etapa com artefato diferente deve retornar conflito.
    
- Cancelar execução já cancelada pode retornar sucesso ou conflito, conforme decisão explícita.
    
- Aprovar artefato já aprovado pela mesma decisão pode ser idempotente.
    

A política deverá ser consistente e documentada por operação.

Exemplo:

```csharp
if (Status == StepExecutionStatus.Completed)
{
    return OutputArtifactId == outputArtifactId
        ? Result.Success()
        : Result.Failure(
            ExecutionErrors.StepAlreadyCompletedWithDifferentOutput(Id));
}
```

---

# 44. Checkpoints

Checkpoint representa o progresso persistido de uma operação.

Exemplo conceitual:

```csharp
public sealed record ExecutionCheckpoint(
    string Name,
    int Version,
    string Data,
    DateTimeOffset CreatedAt);
```

O Domain deverá conhecer:

- Nome do checkpoint.
    
- Versão.
    
- Momento de criação.
    
- Relação com a execução ou etapa.
    

O Domain não deverá conhecer:

- Redis.
    
- Blob Storage.
    
- Formato de compressão.
    
- Estratégia de serialização.
    
- Local físico.
    

Checkpoints poderão ser usados para:

- Retomar execução.
    
- Evitar repetir trabalho.
    
- Auditar progresso.
    
- Recuperar falhas.
    
- Executar compensações.
    

---

# 45. Artifact Aggregate

Estrutura conceitual:

```csharp
public sealed class Artifact : AggregateRoot<ArtifactId>
{
    public OrganizationId OrganizationId { get; private set; }

    public ProjectId ProjectId { get; private set; }

    public PipelineExecutionId ExecutionId { get; private set; }

    public StepExecutionId? StepExecutionId { get; private set; }

    public ArtifactType Type { get; private set; }

    public ArtifactStatus Status { get; private set; }

    public int Version { get; private set; }

    public ArtifactContent Content { get; private set; }

    public PromptVersion? PromptVersion { get; private set; }

    public ProviderModel? ProviderModel { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
}
```

---

# 46. Tipos de Artifact

Tipos iniciais:

```text
Research
Outline
Script
Title
Description
Image
Audio
Video
Caption
Metadata
StructuredData
```

A modelagem deverá permitir expansão sem alterar excessivamente o Domain.

Exemplo:

```csharp
public sealed record ArtifactType
{
    public static readonly ArtifactType Research = new("research");
    public static readonly ArtifactType Script = new("script");
    public static readonly ArtifactType Image = new("image");

    public string Value { get; }

    private ArtifactType(string value)
    {
        Value = value;
    }
}
```

---

# 47. ArtifactContent

O conteúdo poderá ser:

- Texto armazenado no banco.
    
- JSON estruturado.
    
- Referência para storage.
    
- Metadados de arquivo.
    

Exemplo:

```csharp
public abstract record ArtifactContent;

public sealed record TextArtifactContent(
    string Text,
    string? SchemaVersion)
    : ArtifactContent;

public sealed record StorageArtifactContent(
    string StorageProvider,
    string ObjectKey,
    string ContentType,
    long Size,
    string Checksum)
    : ArtifactContent;
```

Esses tipos representam conceitos próprios do produto e não tipos de Azure ou EF Core.

---

# 48. Invariantes de Artifact

1. Todo Artifact pertence a uma Organization.
    
2. Todo Artifact pertence a um Project.
    
3. Todo Artifact deve estar associado a uma execução.
    
4. Tipo é obrigatório.
    
5. Conteúdo é obrigatório.
    
6. Artefato externo deve possuir checksum.
    
7. Versão deve ser positiva.
    
8. Uma versão aprovada não deve ser alterada.
    
9. Alterações significativas devem gerar nova versão.
    
10. Provider e modelo devem ser registrados quando o artefato for gerado por IA.
    
11. Prompt Version deve ser registrada quando aplicável.
    
12. Um Artifact rejeitado não pode ser publicado.
    
13. Um Artifact arquivado não pode ser aprovado.
    

---

# 49. Versionamento de Artifact

Artefatos não deverão ser sobrescritos quando houver alteração relevante.

Exemplo:

```text
Script v1 — Generated
Script v2 — Reviewed
Script v3 — Approved
```

Uma nova versão deverá referenciar opcionalmente a anterior:

```csharp
public ArtifactId? PreviousVersionId { get; private set; }
```

Benefícios:

- Auditoria.
    
- Comparação.
    
- Rollback.
    
- Reprodutibilidade.
    
- Aprovação segura.
    
- Análise da evolução produzida pela IA.
    

---

# 50. Approval Aggregate

O módulo de aprovação poderá ser introduzido após o MVP.

Estrutura conceitual:

```csharp
public sealed class Approval : AggregateRoot<ApprovalId>
{
    public OrganizationId OrganizationId { get; private set; }

    public ArtifactId ArtifactId { get; private set; }

    public ApprovalStatus Status { get; private set; }

    public UserId? DecidedBy { get; private set; }

    public ApprovalDecision? Decision { get; private set; }

    public string? Comment { get; private set; }

    public DateTimeOffset RequestedAt { get; private set; }

    public DateTimeOffset? DecidedAt { get; private set; }
}
```

Decisões:

```csharp
public enum ApprovalDecision
{
    Approved = 1,
    Rejected = 2,
    ChangesRequested = 3
}
```

---

# 51. Publication Aggregate

O Publication Aggregate representará uma tentativa de publicação.

Estrutura conceitual:

```csharp
public sealed class Publication : AggregateRoot<PublicationId>
{
    public OrganizationId OrganizationId { get; private set; }

    public ProjectId ProjectId { get; private set; }

    public ArtifactId ArtifactId { get; private set; }

    public PublicationTarget Target { get; private set; }

    public PublicationStatus Status { get; private set; }

    public string? ExternalPublicationId { get; private set; }

    public DateTimeOffset RequestedAt { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }

    public ExecutionError? Failure { get; private set; }
}
```

O Domain conhecerá o conceito de destino, mas não o SDK do YouTube, WordPress ou LinkedIn.

---

# 52. Relacionamentos entre Aggregates

Aggregates deverão se relacionar por identificadores.

Preferir:

```csharp
public ProjectId ProjectId { get; private set; }
```

Evitar no Domain:

```csharp
public Project Project { get; private set; }
```

Fluxo conceitual:

```text
Organization
    │
    ├── Project
    │     │
    │     ├── Pipeline
    │     │     │
    │     │     └── PipelineExecution
    │     │             │
    │     │             └── StepExecution
    │     │
    │     └── Artifact
    │
    ├── Approval
    └── Publication
```

Não significa que toda a árvore seja carregada ao mesmo tempo.

Cada Aggregate será carregado e persistido separadamente.

---

# 53. Consistência entre Aggregates

Uma transação não deverá carregar e modificar diversos Aggregates sem necessidade.

Exemplo aceitável:

```text
Carregar PipelineExecution
Concluir StepExecution
Salvar PipelineExecution
Registrar Domain Events
```

Quando uma ação exigir atualização em outro Aggregate:

```text
PipelineExecution concluída
    ↓
Domain Event
    ↓
Application Handler
    ↓
Atualiza outro Aggregate
```

Consistência imediata será usada somente quando a regra exigir atomicidade real.

Consistência eventual será usada quando os Aggregates puderem convergir com segurança.

---

# 54. Domain Services

Domain Service será utilizado quando uma regra:

- Pertence ao domínio.
    
- Não pertence naturalmente a uma única Entity.
    
- Envolve múltiplos conceitos.
    
- Não depende de infraestrutura.
    

Exemplos possíveis:

```text
PipelineStructureValidator
ExecutionCompletionPolicy
ArtifactApprovalPolicy
PublicationEligibilityPolicy
```

Exemplo:

```csharp
public sealed class PublicationEligibilityPolicy
{
    public Result Evaluate(
        Artifact artifact,
        Approval? approval)
    {
        if (artifact.Status != ArtifactStatus.Ready)
        {
            return Result.Failure(
                PublicationErrors.ArtifactNotReady);
        }

        if (approval is null ||
            approval.Decision != ApprovalDecision.Approved)
        {
            return Result.Failure(
                PublicationErrors.ApprovalRequired);
        }

        return Result.Success();
    }
}
```

O Domain Service não deverá acessar repository ou provider externo.

---

# 55. Policies

Policy representa uma decisão de negócio que pode evoluir independentemente das entidades.

Exemplos:

```text
CanRetryStepPolicy
CanResumeExecutionPolicy
RequiresApprovalPolicy
ArtifactRetentionPolicy
PipelineExecutionLimitPolicy
```

Policies deverão receber os dados necessários explicitamente.

Exemplo:

```csharp
public sealed class CanRetryStepPolicy
{
    public RetryDecision Evaluate(
        StepExecution step,
        ExecutionError error,
        int maximumAttempts)
    {
        if (!error.IsTransient)
        {
            return RetryDecision.Denied(
                "A falha não é transitória.");
        }

        if (step.AttemptCount >= maximumAttempts)
        {
            return RetryDecision.Denied(
                "O número máximo de tentativas foi atingido.");
        }

        return RetryDecision.Allowed();
    }
}
```

O cálculo do tempo de espera poderá ser responsabilidade de Infrastructure ou Application.

---

# 56. Specifications

Specifications poderão ser utilizadas em regras reutilizáveis de domínio.

Exemplos:

```text
ActiveProjectSpecification
PublishedPipelineSpecification
ExecutablePipelineSpecification
ApprovedArtifactSpecification
```

Elas não deverão ser utilizadas apenas para esconder expressões simples.

Uma Specification deverá representar uma regra com nome e significado de negócio.

---

# 57. Factories

Factories poderão ser usadas quando a criação envolver:

- Vários objetos internos.
    
- Regras complexas.
    
- Construção de snapshots.
    
- Escolha de estratégias.
    
- Validações compostas.
    

Exemplo:

```text
PipelineExecutionFactory
```

Ela poderá transformar uma versão publicada de Pipeline em uma nova PipelineExecution com suas StepExecutions.

A Factory continuará dentro do Domain somente se não depender de persistência ou infraestrutura.

---

# 58. Regras de Estado

Estados deverão ser alterados somente por métodos específicos.

Evitar:

```csharp
execution.Status = PipelineExecutionStatus.Completed;
```

Preferir:

```csharp
execution.Complete(completedAt);
```

Cada transição deverá validar:

- Estado atual.
    
- Pré-condições.
    
- Dados obrigatórios.
    
- Efeitos internos.
    
- Domain Events.
    
- Datas.
    
- Auditoria de domínio.
    

---

# 59. Concorrência

O Domain deverá possuir comportamentos seguros diante de comandos repetidos ou concorrentes.

Exemplos:

- Duas tentativas de iniciar a mesma execução.
    
- Dois Workers tentando concluir a mesma etapa.
    
- Aprovação e rejeição simultâneas.
    
- Duas atualizações da mesma versão de pipeline.
    

O controle técnico de concorrência ocorrerá no Data por:

- Version columns.
    
- Optimistic concurrency.
    
- Constraints.
    
- Transactions.
    

O Domain deverá detectar conflitos lógicos.

Exemplo:

```csharp
public long Version { get; private set; }
```

Essa propriedade poderá ser incrementada em alterações relevantes.

O EF Core poderá utilizar um token de concorrência, mas essa configuração ficará em Data.

---

# 60. Multi-tenancy no Domain

Aggregates tenant-scoped deverão possuir `OrganizationId`.

Exemplos:

```text
Project
Pipeline
PipelineExecution
Artifact
Approval
Publication
```

Regras:

1. OrganizationId é obrigatório.
    
2. OrganizationId é imutável.
    
3. Objetos relacionados devem pertencer à mesma Organization.
    
4. Nenhuma operação deve combinar dados entre Organizations.
    
5. A Application deverá validar acesso antes de carregar ou alterar dados.
    
6. O Domain deverá validar consistência de Organization quando receber objetos relacionados.
    

Exemplo:

```csharp
if (project.OrganizationId != pipeline.OrganizationId)
{
    return Result.Failure(
        PipelineErrors.OrganizationMismatch);
}
```

---

# 61. Tempo no Domain

O Domain não deverá utilizar diretamente:

```csharp
DateTime.Now
DateTime.UtcNow
DateTimeOffset.Now
```

O momento deverá ser fornecido:

```csharp
project.Archive(userId, clock.UtcNow);
```

Benefícios:

- Testes determinísticos.
    
- Reprodutibilidade.
    
- Controle de timezone.
    
- Simulação de cenários.
    
- Auditoria consistente.
    

Datas internas deverão utilizar `DateTimeOffset` em UTC.

---

# 62. Auditoria de Negócio

Auditoria técnica e auditoria de negócio são conceitos diferentes.

Auditoria de negócio poderá fazer parte do Aggregate:

```text
CreatedBy
CreatedAt
ApprovedBy
ApprovedAt
CancelledBy
CancelledAt
ArchivedBy
ArchivedAt
```

Auditoria técnica poderá ser adicionada por interceptors:

```text
UpdatedAt técnico
CorrelationId
Host
ProcessId
```

Somente informações relevantes para as regras de negócio deverão entrar no Domain.

---

# 63. Exclusão e Arquivamento

Entidades importantes não deverão ser excluídas fisicamente como regra padrão.

Preferir estados como:

```text
Archived
Disabled
Cancelled
Deprecated
```

Exclusão física poderá ocorrer em:

- Dados temporários.
    
- Dados expirados.
    
- Registros técnicos.
    
- Limpeza exigida por políticas legais.
    
- Artefatos sem valor histórico.
    

O Domain deverá diferenciar:

- Arquivar.
    
- Cancelar.
    
- Revogar.
    
- Excluir.
    
- Expirar.
    

Essas ações não são sinônimas.

---

# 64. Regras de Reprodutibilidade

Para reproduzir uma geração, deverão ser registrados quando aplicável:

- PipelineId.
    
- PipelineVersion.
    
- StepDefinitionId.
    
- PromptVersion.
    
- Provider.
    
- Modelo.
    
- Parâmetros relevantes.
    
- Schema de output.
    
- Inputs.
    
- Artefatos anteriores.
    
- Momento da execução.
    

O Domain deverá carregar essas referências no modelo, sem conhecer a implementação dos providers.

---

# 65. Dados Sensíveis

O Domain não deverá armazenar:

- API keys.
    
- Tokens de acesso externos.
    
- Secrets.
    
- Credenciais.
    
- Connection strings.
    

Ele poderá armazenar identificadores de credenciais:

```csharp
CredentialReferenceId
```

A resolução da credencial ocorrerá em Infrastructure.

---

# 66. Testes Unitários do Domain

O Domain deverá possuir testes para:

- Criação válida.
    
- Criação inválida.
    
- Invariantes.
    
- Transições de estado.
    
- Domain Events.
    
- Idempotência.
    
- Regras de retry.
    
- Cancelamento.
    
- Versionamento.
    
- Isolamento por Organization.
    
- Relacionamentos entre Aggregates.
    
- Casos limite.
    

Exemplo:

```csharp
[Fact]
public void Start_ShouldChangeStatusToRunning_WhenExecutionIsQueued()
{
    var execution = PipelineExecutionFactory.CreateQueued();

    var result = execution.Start(
        new DateTimeOffset(
            2026,
            7,
            23,
            12,
            0,
            0,
            TimeSpan.Zero));

    result.IsSuccess.Should().BeTrue();
    execution.Status.Should().Be(
        PipelineExecutionStatus.Running);
    execution.StartedAt.Should().NotBeNull();
}
```

---

# 67. Teste de Transição Inválida

```csharp
[Fact]
public void Start_ShouldFail_WhenExecutionIsCompleted()
{
    var execution = PipelineExecutionFactory.CreateCompleted();

    var result = execution.Start(
        TestClock.UtcNow);

    result.IsFailure.Should().BeTrue();
    result.Error.Code.Should().Be(
        "Execution.CannotStartFromState");
}
```

Testes deverão validar comportamento, não detalhes privados de implementação.

---

# 68. Teste de Domain Event

```csharp
[Fact]
public void CompleteStep_ShouldRaiseExecutionCompletedEvent_WhenLastStepCompletes()
{
    var execution =
        PipelineExecutionFactory.CreateRunningWithSingleStep();

    var stepId = execution.Steps.Single().Id;

    execution.StartNextStep(TestClock.UtcNow);

    execution.CompleteStep(
        stepId,
        ArtifactId.New(),
        TestClock.UtcNow);

    execution.DomainEvents
        .Should()
        .ContainSingle(domainEvent =>
            domainEvent is
                PipelineExecutionCompletedDomainEvent);
}
```

---

# 69. O que não testar no Domain

Testes do Domain não deverão testar:

- EF Core.
    
- Mapeamentos.
    
- Banco de dados.
    
- RabbitMQ.
    
- JSON.
    
- HTTP.
    
- OpenAI.
    
- Azure.
    
- Logs.
    
- Telemetria.
    
- Dependency Injection.
    

Esses testes pertencem a outras camadas.

---

# 70. Antipadrões

## Anemic Domain Model

Entidades com apenas getters e setters.

```csharp
project.Status = ProjectStatus.Archived;
```

Correção:

```csharp
project.Archive(userId, now);
```

---

## Domain dependendo de EF Core

Evitar:

```csharp
using Microsoft.EntityFrameworkCore;
```

Configurações de banco pertencem a Data.

---

## Domain dependendo de MediatR

Domain Events não deverão implementar interfaces do MediatR.

A Application poderá criar um adapter.

---

## Entidade expondo coleção mutável

Evitar:

```csharp
public List<StepExecution> Steps { get; set; }
```

Preferir coleção privada e somente leitura.

---

## Validação somente no Handler

Mesmo que o Handler valide a entrada, a entidade deverá continuar protegendo suas invariantes.

---

## Aggregate Gigante

Evitar carregar:

```text
Project
├── Pipelines
├── Executions
├── Artifacts
├── Approvals
└── Publications
```

em um único Aggregate.

---

## Repository no Domain

Interfaces de repository ficarão na Application.

O Domain deverá permanecer focado em comportamento.

---

## Serviços Genéricos

Evitar:

```text
DomainService
BusinessManager
EntityHelper
RulesEngine
```

Nomes deverão representar conceitos específicos.

---

## Primitive Obsession

Evitar utilizar strings e números para conceitos importantes quando isso causar ambiguidade.

Exemplo problemático:

```csharp
string provider;
string model;
string currency;
decimal cost;
```

Possível modelagem:

```csharp
ProviderModel providerModel;
Money cost;
```

---

## Value Object para Tudo

Também não será necessário transformar cada propriedade em um tipo próprio.

A abstração deverá possuir valor real.

---

## Excesso de Domain Events

Nem toda alteração precisa gerar evento.

Gerar evento quando:

- Outro comportamento precisa reagir.
    
- O fato possui valor de auditoria.
    
- O fato cruza limites.
    
- O fato representa uma mudança relevante.
    

---

# 71. Regras Arquiteturais

1. Domain depende apenas de SharedKernel.
    
2. Domain não conhece persistência.
    
3. Domain não conhece mensageria.
    
4. Domain não conhece providers externos.
    
5. Entidades protegem suas invariantes.
    
6. Setters de estado são privados.
    
7. Coleções internas não são mutáveis externamente.
    
8. Aggregates são alterados por suas raízes.
    
9. Aggregates se relacionam por IDs.
    
10. Falhas esperadas utilizam Result.
    
11. Exceções são reservadas para erros inesperados.
    
12. Domain Events representam fatos internos.
    
13. Domain Events não são contratos distribuídos.
    
14. Datas são fornecidas explicitamente.
    
15. OrganizationId é obrigatório em dados tenant-scoped.
    
16. OrganizationId é imutável.
    
17. Estados são alterados por métodos de negócio.
    
18. Versões utilizadas por execuções são imutáveis.
    
19. Operações repetidas possuem comportamento definido.
    
20. O Domain não acessa configuração.
    
21. O Domain não produz logs diretamente.
    
22. O Domain não resolve dependências.
    
23. O Domain não acessa usuário atual.
    
24. O Domain não chama serviços externos.
    
25. Regras críticas possuem testes unitários.
    

---

# 72. Modelo Inicial do MVP

Para o MVP, o Domain deverá implementar:

## Projects

```text
Project
ProjectId
ProjectName
ProjectStatus
ProjectErrors
ProjectCreatedDomainEvent
```

## Pipelines

```text
Pipeline
PipelineId
PipelineName
PipelineVersion
PipelineStatus
PipelineStepDefinition
PipelineStepDefinitionId
PipelineStepType
PipelineErrors
PipelinePublishedDomainEvent
```

## Executions

```text
PipelineExecution
PipelineExecutionId
PipelineExecutionStatus
StepExecution
StepExecutionId
StepExecutionStatus
ExecutionError
ExecutionErrors
Eventos de execução
```

## Artifacts

```text
Artifact
ArtifactId
ArtifactType
ArtifactStatus
ArtifactContent
ArtifactErrors
ArtifactCreatedDomainEvent
```

Não será necessário implementar inicialmente:

- Approval completo.
    
- Publication completo.
    
- Billing.
    
- Quotas avançadas.
    
- Workflow visual.
    
- Compensações complexas.
    
- Multi-região.
    
- Políticas avançadas de retenção.
    

---

# 73. Ordem de Implementação do Domain

## Etapa 1 — Base

- Identificadores.
    
- Result e Error no SharedKernel.
    
- Entity.
    
- AggregateRoot.
    
- IDomainEvent.
    
- ValueObject, caso necessário.
    

## Etapa 2 — Project

- ProjectId.
    
- ProjectName.
    
- ProjectStatus.
    
- Project.
    
- Erros.
    
- Eventos.
    
- Testes.
    

## Etapa 3 — Pipeline

- PipelineId.
    
- PipelineVersion.
    
- PipelineStepType.
    
- PipelineStepDefinition.
    
- Pipeline.
    
- Regras estruturais.
    
- Eventos.
    
- Testes.
    

## Etapa 4 — Execution

- PipelineExecutionId.
    
- StepExecutionId.
    
- Estados.
    
- PipelineExecution.
    
- StepExecution.
    
- Transições.
    
- Cancelamento.
    
- Falhas.
    
- Eventos.
    
- Testes.
    

## Etapa 5 — Artifact

- ArtifactId.
    
- ArtifactType.
    
- ArtifactContent.
    
- Artifact.
    
- Versionamento.
    
- Eventos.
    
- Testes.
    

---

# 74. Checklist de Nova Entidade

Antes de criar uma Entity, verificar:

- Ela possui identidade própria?
    
- Ela possui ciclo de vida?
    
- Duas instâncias com os mesmos dados podem representar objetos diferentes?
    
- Ela precisa mudar de estado?
    
- A identidade é relevante para o negócio?
    
- Ela pertence a qual Aggregate?
    
- Quem pode alterá-la?
    
- Quais invariantes ela deve proteger?
    
- Quais eventos ela produz?
    
- Ela realmente precisa ser uma raiz?
    

Se não possuir identidade própria, provavelmente deve ser um Value Object.

---

# 75. Checklist de Novo Aggregate

- Qual é a raiz?
    
- Qual é o limite de consistência?
    
- Quais objetos internos pertencem à raiz?
    
- Quais invariantes precisam de consistência imediata?
    
- O Aggregate pode crescer indefinidamente?
    
- Ele pode ser carregado de forma eficiente?
    
- Ele referencia outros Aggregates somente por ID?
    
- Suas transições estão encapsuladas?
    
- Seus eventos estão definidos?
    
- Ele suporta concorrência?
    
- Ele possui comportamento idempotente?
    
- Ele possui testes das regras críticas?
    

---

# 76. Checklist de Novo Value Object

- Representa um conceito de negócio?
    
- Possui validação?
    
- É imutável?
    
- É comparado por valor?
    
- Evita ambiguidade?
    
- Reduz primitive obsession?
    
- Possui comportamento próprio?
    
- O benefício compensa a complexidade?
    

---

# 77. Checklist de Novo Domain Event

- Representa algo que já aconteceu?
    
- O nome está no passado?
    
- O fato é relevante?
    
- Outro componente precisa reagir?
    
- Os dados são mínimos?
    
- O evento não contém entidades completas?
    
- Ele é interno ao Domain?
    
- A data do evento foi fornecida?
    
- Ele possui identificador?
    
- Existe risco de emissão duplicada?
    

---

# 78. Critérios de Qualidade

O Domain será considerado saudável quando:

- Regras críticas puderem ser entendidas lendo as entidades.
    
- Testes forem executados sem banco e sem rede.
    
- Estados inválidos não puderem ser criados facilmente.
    
- Mudanças de estado ocorrerem por métodos explícitos.
    
- Infraestrutura puder ser substituída sem alterar regras centrais.
    
- Aggregates permanecerem pequenos.
    
- Erros possuírem códigos estáveis.
    
- Eventos representarem fatos relevantes.
    
- O modelo utilizar a linguagem do produto.
    
- A complexidade das abstrações for proporcional ao problema.
    

---

# 79. Decisões que Permanecem Evolutivas

Os seguintes pontos serão refinados durante a implementação:

- Limite final entre Pipeline e PipelineDefinition.
    
- Forma de persistir versões publicadas.
    
- Granularidade de StepExecution.
    
- Estrutura exata dos checkpoints.
    
- Tipos finais de ArtifactContent.
    
- Política de retomada após falhas.
    
- Política de retry por tipo de etapa.
    
- Introdução de approvals.
    
- Introdução de publications.
    
- Limite entre Artifact e suas versões.
    
- Necessidade de Domain Services adicionais.
    

Essas decisões não deverão bloquear o MVP.

O modelo deverá começar simples e ser refinado a partir de comportamentos reais.

---

# 80. Filosofia Final

O Domain do Infinite Content AI deverá representar o negócio, não a tecnologia utilizada para implementá-lo.

O código deverá expressar ações como:

```text
Criar projeto
Publicar pipeline
Enfileirar execução
Iniciar etapa
Concluir etapa
Falhar execução
Criar artefato
Aprovar artefato
Solicitar publicação
```

Ele não deverá expressar detalhes como:

```text
Salvar linha
Publicar mensagem
Executar HTTP
Serializar JSON
Atualizar cache
Chamar SDK
```

Esses detalhes pertencem às camadas externas.

A regra principal será:

> O Domain protege o que precisa permanecer verdadeiro, independentemente de como o sistema é executado ou persistido.

Um Domain bem modelado permitirá que API, Workers, mensageria, banco e providers evoluam sem destruir as regras centrais do Infinite Content AI.