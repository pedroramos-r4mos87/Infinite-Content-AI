# Application

## 1. Objetivo

O projeto `Application` representa a camada de casos de uso do Infinite Content AI.

Sua função é coordenar as ações necessárias para atender às intenções dos usuários e dos processos internos, utilizando o Domain para aplicar regras de negócio e abstrações para acessar recursos externos.

A Application deverá responder perguntas como:

- Como criar um projeto?
    
- Como iniciar uma execução de pipeline?
    
- Como consultar o estado de uma execução?
    
- Como coordenar a execução de uma etapa?
    
- Como validar uma solicitação?
    
- Como verificar autorização?
    
- Como salvar mudanças?
    
- Como registrar uma mensagem na Outbox?
    
- Como reagir a um Domain Event?
    
- Como acionar um provider de inteligência artificial sem depender do SDK?
    
- Como transformar falhas em resultados previsíveis?
    

A Application não representa o negócio isoladamente.

Ela coordena o negócio.

```text
Entrada
   ↓
Application
   ├── Valida
   ├── Autoriza
   ├── Carrega dados
   ├── Invoca o Domain
   ├── Coordena dependências
   ├── Persiste alterações
   └── Retorna resultado
```

---

# 2. Responsabilidades

O projeto `Application` será responsável por:

- Commands.
    
- Queries.
    
- Handlers.
    
- Validators.
    
- Casos de uso.
    
- Orquestração.
    
- Interfaces de persistência.
    
- Interfaces de serviços externos.
    
- Interfaces de mensageria.
    
- Interfaces de cache.
    
- Interfaces de storage.
    
- Interfaces de providers de IA.
    
- Autorização de casos de uso.
    
- Validação de entrada.
    
- Pipeline Behaviors.
    
- Transações de aplicação.
    
- Idempotência de casos de uso.
    
- Coordenação de Domain Events.
    
- Produção de Integration Events.
    
- Modelos de entrada e saída internos.
    
- Paginação.
    
- Policies de aplicação.
    
- Mapeamento entre contratos e Domain.
    
- Result Pattern.
    
- Telemetria de casos de uso.
    
- Coordenação de operações assíncronas.
    

A Application não será responsável por:

- Endpoints HTTP.
    
- Controllers.
    
- Serialização HTTP.
    
- DbContext.
    
- EF Core.
    
- Migrations.
    
- Repositories concretos.
    
- RabbitMQ concreto.
    
- Redis concreto.
    
- Azure Blob Storage concreto.
    
- SDKs de IA.
    
- Regras centrais do Domain.
    
- Configuração de hosts.
    
- Execução de Background Services.
    
- Tratamento específico de protocolos externos.
    

---

# 3. Dependências Permitidas

A Application poderá depender de:

```text
Domain
Contracts
SharedKernel
```

A Application não poderá depender de:

```text
Api
Data
Infrastructure
Worker
```

Fluxo de dependências:

```text
Api ───────────────┐
Worker ────────────┼──> Application ───> Domain
Data ──────────────┤
Infrastructure ────┘
```

A Application define abstrações.

Data e Infrastructure implementam essas abstrações.

---

# 4. Princípios

## 4.1 Casos de uso explícitos

Cada operação relevante deverá ser representada por um caso de uso claramente nomeado.

Exemplos:

```text
CreateProject
UpdateProject
ArchiveProject
CreatePipeline
PublishPipeline
StartExecution
CancelExecution
ExecutePipelineStep
GetExecution
GetArtifact
```

Evitar serviços genéricos como:

```text
ProjectService
PipelineManager
ExecutionProcessor
ApplicationHelper
```

Esses nomes normalmente concentram responsabilidades demais.

---

## 4.2 Um Handler por intenção

Cada Command ou Query deverá possuir um Handler principal.

Exemplo:

```text
CreateProjectCommand
    ↓
CreateProjectHandler
```

O Handler deverá coordenar uma única intenção de negócio.

---

## 4.3 Application não substitui o Domain

O Handler não deverá implementar diretamente regras que pertencem ao modelo de negócio.

Evitar:

```csharp
if (execution.Status == ExecutionStatus.Running)
{
    execution.Status = ExecutionStatus.Completed;
}
```

Preferir:

```csharp
var result = execution.Complete(completedAt);
```

A Application decide quando a ação deve acontecer.

O Domain decide se a ação é válida.

---

## 4.4 Infraestrutura através de abstrações

A Application não deverá conhecer implementações concretas.

Exemplo:

```csharp
public interface ITextGenerationProvider
{
    Task<Result<TextGenerationResponse>> GenerateAsync(
        TextGenerationRequest request,
        CancellationToken cancellationToken);
}
```

Implementações possíveis:

```text
OpenAiTextGenerationProvider
AnthropicTextGenerationProvider
GeminiTextGenerationProvider
FakeTextGenerationProvider
```

Essas implementações ficarão em Infrastructure.

---

## 4.5 Resultados explícitos

Falhas esperadas deverão ser retornadas por meio de `Result`.

Exemplo:

```csharp
Task<Result<CreateProjectResult>> Handle(
    CreateProjectCommand command,
    CancellationToken cancellationToken);
```

O fluxo esperado não deverá depender de exceções.

---

## 4.6 Operações longas são coordenadas assincronamente

A Application não deverá manter uma requisição HTTP aberta enquanto um pipeline completo é executado.

O caso de uso deverá:

1. Criar uma execução.
    
2. Persistir o estado inicial.
    
3. Registrar uma mensagem na Outbox.
    
4. Retornar o identificador da execução.
    

O Worker continuará o processamento.

---

# 5. Estrutura do Projeto

Estrutura recomendada:

```text
Application
│
├── Abstractions
│   ├── Authentication
│   ├── Authorization
│   ├── Caching
│   ├── Messaging
│   ├── Persistence
│   ├── Providers
│   ├── Storage
│   ├── Time
│   └── Transactions
│
├── Behaviors
│   ├── LoggingBehavior.cs
│   ├── AuthorizationBehavior.cs
│   ├── IdempotencyBehavior.cs
│   ├── ValidationBehavior.cs
│   ├── TransactionBehavior.cs
│   └── PerformanceBehavior.cs
│
├── Common
│   ├── Commands
│   ├── Queries
│   ├── Events
│   ├── Mapping
│   ├── Pagination
│   ├── Security
│   └── Telemetry
│
├── Features
│   ├── Projects
│   ├── Pipelines
│   ├── Executions
│   ├── Artifacts
│   ├── Approvals
│   ├── Publications
│   ├── Organizations
│   └── Identity
│
└── DependencyInjection.cs
```

---

# 6. Organização por Features

A Application será organizada principalmente por feature.

Exemplo:

```text
Features
└── Projects
    ├── CreateProject
    │   ├── CreateProjectCommand.cs
    │   ├── CreateProjectHandler.cs
    │   ├── CreateProjectValidator.cs
    │   └── CreateProjectResult.cs
    │
    ├── UpdateProject
    ├── ArchiveProject
    ├── GetProject
    └── ListProjects
```

Para execuções:

```text
Features
└── Executions
    ├── StartExecution
    ├── CancelExecution
    ├── GetExecution
    ├── ResumeExecution
    └── Steps
        ├── ExecuteStep
        ├── Research
        └── Script
```

Código que muda junto deverá permanecer próximo.

---

# 7. CQRS

A Application adotará uma separação lógica entre Commands e Queries.

Essa separação não exige bancos diferentes.

## Commands

Representam intenção de alterar o estado.

Exemplos:

```text
CreateProjectCommand
PublishPipelineCommand
StartExecutionCommand
CancelExecutionCommand
ExecutePipelineStepCommand
```

## Queries

Representam intenção de consultar dados.

Exemplos:

```text
GetProjectQuery
ListProjectsQuery
GetExecutionQuery
GetArtifactQuery
```

A separação permitirá:

- Modelos diferentes de leitura e escrita.
    
- Behaviors específicos.
    
- Transações somente em Commands.
    
- Otimização independente de Queries.
    
- Segurança mais explícita.
    
- Melhor observabilidade.
    

---

# 8. Contratos Base de Commands

Interfaces conceituais:

```csharp
public interface ICommand : IRequest<Result>;

public interface ICommand<TResponse>
    : IRequest<Result<TResponse>>;
```

Handlers:

```csharp
public interface ICommandHandler<TCommand>
    : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;

public interface ICommandHandler<TCommand, TResponse>
    : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;
```

O uso de MediatR será um detalhe de implementação.

Os conceitos de Command e Query pertencem à Application.

---

# 9. Contratos Base de Queries

```csharp
public interface IQuery<TResponse>
    : IRequest<Result<TResponse>>;
```

```csharp
public interface IQueryHandler<TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
```

Queries poderão retornar:

- Detalhes.
    
- Listas.
    
- Projeções.
    
- Paginação.
    
- Resumos.
    
- Modelos específicos para telas.
    

Elas não deverão retornar Aggregates diretamente.

---

# 10. Commands

Um Command deverá:

- Expressar uma intenção.
    
- Ser imutável.
    
- Possuir dados necessários para a ação.
    
- Carregar contexto explícito quando necessário.
    
- Não conter comportamento de infraestrutura.
    
- Não expor entidades.
    
- Ser serializável somente quando houver necessidade real.
    

Exemplo:

```csharp
public sealed record CreateProjectCommand(
    OrganizationId OrganizationId,
    UserId UserId,
    string Name,
    string? Description)
    : ICommand<CreateProjectResult>;
```

Outro exemplo:

```csharp
public sealed record StartExecutionCommand(
    OrganizationId OrganizationId,
    UserId RequestedBy,
    ProjectId ProjectId,
    PipelineId PipelineId,
    string? IdempotencyKey)
    : ICommand<StartExecutionResult>;
```

---

# 11. Queries

Uma Query deverá conter somente os dados necessários para a consulta.

Exemplo:

```csharp
public sealed record GetExecutionQuery(
    OrganizationId OrganizationId,
    PipelineExecutionId ExecutionId)
    : IQuery<ExecutionDetails>;
```

Consulta paginada:

```csharp
public sealed record ListProjectsQuery(
    OrganizationId OrganizationId,
    int Page,
    int PageSize,
    string? Search)
    : IQuery<PaginatedResult<ProjectListItem>>;
```

A Query deverá sempre considerar o limite de Organization.

---

# 12. Handlers

O Handler coordena um caso de uso.

Responsabilidades comuns:

1. Carregar recursos.
    
2. Validar existência.
    
3. Verificar autorização.
    
4. Invocar comportamento de domínio.
    
5. Chamar abstrações externas.
    
6. Persistir mudanças.
    
7. Retornar um resultado.
    

Exemplo:

```csharp
public sealed class CreateProjectHandler
    : ICommandHandler<CreateProjectCommand, CreateProjectResult>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public CreateProjectHandler(
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<CreateProjectResult>> Handle(
        CreateProjectCommand command,
        CancellationToken cancellationToken)
    {
        var nameResult = ProjectName.Create(command.Name);

        if (nameResult.IsFailure)
        {
            return Result.Failure<CreateProjectResult>(
                nameResult.Error);
        }

        var projectResult = Project.Create(
            command.OrganizationId,
            nameResult.Value,
            command.Description,
            command.UserId,
            _clock.UtcNow);

        if (projectResult.IsFailure)
        {
            return Result.Failure<CreateProjectResult>(
                projectResult.Error);
        }

        await _projectRepository.AddAsync(
            projectResult.Value,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new CreateProjectResult(
            projectResult.Value.Id,
            projectResult.Value.Name.Value,
            projectResult.Value.CreatedAt);
    }
}
```

---

# 13. Tamanho dos Handlers

Handlers deverão ser pequenos o suficiente para que seu fluxo seja compreendido rapidamente.

Um Handler não deverá:

- Executar um pipeline inteiro.
    
- Conter dezenas de condições de negócio.
    
- Montar SQL.
    
- Chamar SDK diretamente.
    
- Controlar retry técnico.
    
- Criar conexão com RabbitMQ.
    
- Resolver secrets.
    
- Ler `IConfiguration`.
    
- Manipular `DbContext`.
    
- Instanciar outros Handlers.
    

Quando um Handler crescer demais, considerar:

- Extrair uma Policy.
    
- Extrair um Application Service específico.
    
- Mover regra para o Domain.
    
- Dividir o caso de uso.
    
- Transformar operação longa em processamento assíncrono.
    

---

# 14. Validators

Validators serão responsáveis pela validação estrutural da entrada.

Exemplo:

```csharp
public sealed class CreateProjectValidator
    : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectValidator()
    {
        RuleFor(command => command.OrganizationId)
            .NotEqual(OrganizationId.Empty);

        RuleFor(command => command.UserId)
            .NotEqual(UserId.Empty);

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(ProjectName.MaximumLength);

        RuleFor(command => command.Description)
            .MaximumLength(2_000);
    }
}
```

Validators deverão verificar:

- Campos obrigatórios.
    
- Formatos.
    
- Tamanhos.
    
- Intervalos.
    
- Combinações simples.
    
- Paginação.
    
- Valores permitidos.
    

Validators não deverão ser usados para:

- Consultar banco.
    
- Executar regra de domínio complexa.
    
- Chamar provider externo.
    
- Aplicar autorização.
    
- Alterar estado.
    

---

# 15. Validação em Camadas

## API

Valida o contrato HTTP.

```text
JSON válido
Campo obrigatório
Formato de URL
Limite de tamanho
```

## Application

Valida o caso de uso.

```text
OrganizationId presente
PageSize permitido
Combinação de parâmetros válida
```

## Domain

Valida invariantes.

```text
Pipeline publicado possui etapas
Execução concluída não pode reiniciar
Project arquivado não pode iniciar pipeline
```

## Data

Valida integridade persistente.

```text
Foreign key
Unique constraint
Not null
Concurrency token
```

Nenhuma camada substitui completamente as outras.

---

# 16. Pipeline Behaviors

Behaviors permitirão aplicar preocupações transversais de forma consistente.

Ordem conceitual:

```text
Request
   ↓
Logging
   ↓
Performance
   ↓
Authorization
   ↓
Idempotency
   ↓
Validation
   ↓
Transaction
   ↓
Handler
   ↓
Result
```

A ordem final deverá ser testada.

Nem todos os requests utilizarão todos os Behaviors.

---

# 17. LoggingBehavior

Responsabilidades:

- Registrar início do caso de uso.
    
- Registrar conclusão.
    
- Registrar duração.
    
- Registrar resultado.
    
- Adicionar contexto.
    
- Evitar dados sensíveis.
    

Contexto sugerido:

```text
RequestName
OrganizationId
UserId
ExecutionId
PipelineId
CorrelationId
TraceId
Duration
Result
ErrorCode
```

Não deverá registrar automaticamente o objeto completo do Command.

Commands podem conter:

- Prompts.
    
- Conteúdo privado.
    
- Tokens.
    
- Dados pessoais.
    
- Referências sensíveis.
    

---

# 18. PerformanceBehavior

O Behavior de performance poderá:

- Medir duração.
    
- Registrar operações lentas.
    
- Produzir métricas.
    
- Marcar traces.
    
- Identificar regressões.
    

Exemplo conceitual:

```text
CreateProject: 32 ms
StartExecution: 48 ms
GetExecution: 15 ms
ExecuteResearchStep: 14,8 s
```

Limites diferentes poderão ser aplicados a:

- Commands locais.
    
- Queries.
    
- Operações externas.
    
- Etapas de IA.
    

---

# 19. ValidationBehavior

O ValidationBehavior executará os Validators associados ao request.

Fluxo:

```text
Command
   ↓
Validators
   ├── Sucesso → Handler
   └── Falha → Result.Validation
```

Exemplo conceitual:

```csharp
public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
}
```

O Behavior deverá consolidar erros sem lançar exceções para falhas esperadas.

---

# 20. AuthorizationBehavior

O AuthorizationBehavior poderá aplicar políticas declarativas.

Exemplo:

```csharp
public interface IAuthorizedRequest
{
    string Permission { get; }
}
```

```csharp
public sealed record ArchiveProjectCommand(...)
    : ICommand,
      IAuthorizedRequest
{
    public string Permission =>
        Permissions.Projects.Archive;
}
```

O Behavior deverá verificar:

- Usuário autenticado.
    
- Organization atual.
    
- Permissão.
    
- Papel.
    
- Escopo.
    
- Políticas globais.
    

A autorização baseada em propriedade do recurso poderá permanecer no Handler, pois depende do objeto carregado.

---

# 21. Autorização de Recurso

Exemplo:

```csharp
var project = await _projectRepository.GetByIdAsync(
    command.OrganizationId,
    command.ProjectId,
    cancellationToken);

if (project is null)
{
    return Result.Failure(
        ProjectErrors.NotFound(command.ProjectId));
}

var authorizationResult =
    _projectAuthorization.CanArchive(
        currentUser,
        project);

if (authorizationResult.IsFailure)
{
    return authorizationResult;
}
```

Nunca buscar um recurso tenant-scoped apenas por seu ID.

Evitar:

```csharp
GetByIdAsync(projectId);
```

Preferir:

```csharp
GetByIdAsync(organizationId, projectId);
```

---

# 22. Contexto do Usuário

A Application poderá utilizar uma abstração:

```csharp
public interface ICurrentUser
{
    UserId UserId { get; }

    OrganizationId OrganizationId { get; }

    bool IsAuthenticated { get; }

    IReadOnlySet<string> Permissions { get; }
}
```

A implementação ficará no host.

Entretanto, Commands importantes poderão carregar `UserId` e `OrganizationId` explicitamente.

Isso favorece:

- Auditoria.
    
- Testes.
    
- Execução por Worker.
    
- Reprocessamento.
    
- Independência de contexto HTTP.
    

---

# 23. Multi-tenancy

Regras obrigatórias:

1. Toda operação tenant-scoped recebe OrganizationId.
    
2. Queries filtram por OrganizationId.
    
3. Repositories exigem OrganizationId.
    
4. Erros não revelam existência de dados de outro tenant.
    
5. Cache inclui OrganizationId na chave.
    
6. Mensagens incluem OrganizationId.
    
7. Logs incluem OrganizationId quando seguro.
    
8. Eventos preservam OrganizationId.
    
9. Policies verificam consistência entre recursos.
    

Exemplo de chave:

```text
organization:{organizationId}:execution:{executionId}
```

---

# 24. TransactionBehavior

Commands que modificam o PostgreSQL poderão utilizar um Behavior transacional.

Fluxo:

```text
Command
   ↓
Abrir transação
   ↓
Handler
   ↓
SaveChanges
   ↓
Persistir Outbox
   ↓
Commit
```

Queries não deverão utilizar transações de escrita.

Operações externas demoradas não deverão permanecer dentro de transações de banco.

---

# 25. Unit of Work

Abstração:

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken);
}
```

Quando controle explícito for necessário:

```csharp
public interface ITransaction
    : IAsyncDisposable
{
    Task CommitAsync(
        CancellationToken cancellationToken);

    Task RollbackAsync(
        CancellationToken cancellationToken);
}
```

A Application não deverá conhecer `DbContext` ou `IDbContextTransaction`.

---

# 26. Limites Transacionais

Exemplo correto:

```text
Criar PipelineExecution
Registrar OutboxMessage
SaveChanges
Commit
```

Exemplo incorreto:

```text
Abrir transação
Chamar provider de IA
Aguardar 30 segundos
Salvar resultado
Commit
```

Operações externas longas deverão utilizar estados intermediários.

```text
Persistir Running
Commit
   ↓
Chamar provider
   ↓
Persistir resultado
Commit
```

---

# 27. Repositories

Interfaces de repositories ficarão na Application.

Exemplo:

```csharp
public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(
        OrganizationId organizationId,
        ProjectId projectId,
        CancellationToken cancellationToken);

    Task AddAsync(
        Project project,
        CancellationToken cancellationToken);
}
```

Repositories deverão ser orientados ao Aggregate.

Evitar repository genérico universal:

```csharp
IRepository<TEntity>
```

Esse tipo costuma esconder operações e permitir uso inadequado dos Aggregates.

---

# 28. Queries Especializadas

Leituras poderão utilizar interfaces específicas.

```csharp
public interface IProjectQueries
{
    Task<ProjectDetails?> GetDetailsAsync(
        OrganizationId organizationId,
        ProjectId projectId,
        CancellationToken cancellationToken);

    Task<PaginatedResult<ProjectListItem>> ListAsync(
        OrganizationId organizationId,
        ProjectSearchFilter filter,
        CancellationToken cancellationToken);
}
```

Essas implementações poderão usar:

- EF Core.
    
- SQL direto.
    
- Dapper.
    
- Projeções.
    

A Application não conhecerá a tecnologia utilizada.

---

# 29. Paginação

Modelos comuns:

```csharp
public sealed record PageRequest(
    int Page,
    int PageSize);
```

```csharp
public sealed record PaginatedResult<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    long TotalCount);
```

Regras:

- Page deve iniciar em 1.
    
- PageSize deve possuir limite máximo.
    
- Ordenação deve ser determinística.
    
- Consultas grandes devem ser paginadas.
    
- Paginação por cursor poderá ser usada futuramente.
    

---

# 30. Domain Events

Depois que o Domain gerar eventos, a Application poderá reagir a eles.

Exemplo:

```text
PipelineExecutionCompletedDomainEvent
   ↓
CreateCompletionIntegrationEventHandler
   ↓
Outbox
```

Outro exemplo:

```text
ArtifactCreatedDomainEvent
   ↓
UpdateExecutionArtifactReferenceHandler
```

Domain Event Handlers deverão:

- Ser pequenos.
    
- Ser idempotentes quando necessário.
    
- Não depender de ordem implícita.
    
- Evitar operações longas no mesmo commit.
    
- Produzir mensagens quando o trabalho for assíncrono.
    

---

# 31. Despacho de Domain Events

Duas estratégias são possíveis:

## Antes do SaveChanges

Permite alterar outros objetos na mesma transação.

Risco:

- Ordem complexa.
    
- Mais efeitos dentro do commit.
    

## Depois do SaveChanges

Permite reagir ao fato persistido.

Risco:

- Exige estratégia confiável para não perder efeitos.
    

Para eventos que originam comunicação externa, deverá ser utilizada a Outbox.

A estratégia detalhada ficará em `Data.md`.

---

# 32. Integration Events

Integration Events são contratos publicados para outros processos.

Exemplo:

```csharp
public sealed record PipelineExecutionRequestedV1(
    Guid ExecutionId,
    Guid PipelineId,
    Guid OrganizationId,
    DateTimeOffset RequestedAt);
```

A Application poderá criar Integration Events, mas os contratos ficarão no projeto `Contracts`.

A publicação física ficará em Infrastructure.

---

# 33. Outbox

Abstração conceitual:

```csharp
public interface IOutbox
{
    Task AddAsync<TMessage>(
        TMessage message,
        OutboxMetadata metadata,
        CancellationToken cancellationToken);
}
```

Fluxo:

```text
StartExecutionHandler
   ├── Cria PipelineExecution
   ├── Persiste execução
   ├── Registra PipelineExecutionRequestedV1
   └── Commit
```

O Handler não deverá publicar diretamente no RabbitMQ dentro da transação.

---

# 34. Idempotência

Commands sensíveis deverão implementar idempotência.

Exemplos:

- StartExecution.
    
- PublishContent.
    
- ProcessWebhook.
    
- ExecutePipelineStep.
    
- CompleteUpload.
    
- ReprocessArtifact.
    

Abstração:

```csharp
public interface IIdempotentCommand
{
    string IdempotencyKey { get; }
}
```

O IdempotencyBehavior poderá:

1. Procurar resultado anterior.
    
2. Bloquear execução concorrente.
    
3. Executar o Handler.
    
4. Persistir o resultado.
    
5. Retornar o mesmo resultado em repetição.
    

---

# 35. Escopo da Idempotência

A chave deverá considerar:

```text
OrganizationId
CommandType
IdempotencyKey
```

Exemplo:

```text
org-123:StartExecution:request-789
```

Uma mesma chave de tenants diferentes não deverá colidir.

A duração de retenção dependerá do caso de uso.

---

# 36. Concorrência

A Application deverá estar preparada para conflitos de concorrência.

Exemplos:

- Dois Workers iniciam a mesma etapa.
    
- Dois usuários alteram o mesmo Pipeline.
    
- Aprovação e rejeição simultâneas.
    
- Duas tentativas de publicar o mesmo conteúdo.
    

A Data poderá retornar um erro de concorrência abstrato.

```csharp
public interface IConcurrencyAwareUnitOfWork
{
    Task<Result<int>> SaveChangesAsync(
        CancellationToken cancellationToken);
}
```

A Application deverá mapear o conflito para um erro estável.

```text
Execution.ConcurrencyConflict
Pipeline.VersionConflict
Artifact.AlreadyChanged
```

---

# 37. Abstrações de Providers de IA

A Application definirá contratos próprios.

```csharp
public interface ITextGenerationProvider
{
    string ProviderName { get; }

    Task<Result<TextGenerationResponse>> GenerateAsync(
        TextGenerationRequest request,
        CancellationToken cancellationToken);
}
```

Request:

```csharp
public sealed record TextGenerationRequest(
    string Model,
    string SystemPrompt,
    string UserPrompt,
    decimal Temperature,
    int? MaximumOutputTokens,
    StructuredOutputSchema? OutputSchema,
    IReadOnlyDictionary<string, string> Metadata);
```

Response:

```csharp
public sealed record TextGenerationResponse(
    string Content,
    string Provider,
    string Model,
    TokenUsage Usage,
    string? FinishReason,
    string? ProviderRequestId);
```

Nenhum tipo do SDK deverá atravessar essa interface.

---

# 38. Seleção de Provider

A Application poderá utilizar um resolver:

```csharp
public interface ITextGenerationProviderResolver
{
    Result<ITextGenerationProvider> Resolve(
        ProviderModel providerModel);
}
```

A decisão poderá considerar:

- Configuração do Pipeline.
    
- Organization.
    
- Disponibilidade.
    
- Custo.
    
- Política de fallback.
    
- Tipo de conteúdo.
    
- Região.
    
- Limites do provider.
    

A Application coordena a seleção.

A Infrastructure cria e mantém os adapters.

---

# 39. Structured Outputs

Abstração:

```csharp
public sealed record StructuredOutputSchema(
    string Name,
    string Version,
    string JsonSchema);
```

A Application deverá:

- Solicitar output estruturado quando possível.
    
- Validar o resultado.
    
- Registrar a versão do schema.
    
- Retornar falha explícita quando inválido.
    
- Não confiar cegamente no texto do provider.
    

Possível interface:

```csharp
public interface IStructuredOutputValidator
{
    Result Validate(
        string content,
        StructuredOutputSchema schema);
}
```

A implementação técnica poderá ficar em Infrastructure.

---

# 40. Storage

Abstração:

```csharp
public interface IFileStorage
{
    Task<Result<StoredFile>> StoreAsync(
        StoreFileRequest request,
        CancellationToken cancellationToken);

    Task<Result<Stream>> OpenReadAsync(
        StorageReference reference,
        CancellationToken cancellationToken);

    Task<Result> DeleteAsync(
        StorageReference reference,
        CancellationToken cancellationToken);
}
```

A Application utiliza `StorageReference`.

Ela não conhece:

- BlobContainerClient.
    
- Bucket.
    
- SAS Token.
    
- Azure SDK.
    
- AWS SDK.
    

---

# 41. Cache

Abstração opcional:

```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken);

    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan duration,
        CancellationToken cancellationToken);

    Task RemoveAsync(
        string key,
        CancellationToken cancellationToken);
}
```

Regras:

- Cache não é fonte da verdade.
    
- Falha de cache não deverá corromper o caso de uso.
    
- Chaves incluem OrganizationId.
    
- Dados sensíveis exigem cuidado.
    
- O MVP não dependerá de cache.
    

---

# 42. Mensageria

A Application poderá definir:

```csharp
public interface IMessagePublisher
{
    Task<Result> PublishAsync<TMessage>(
        TMessage message,
        MessageMetadata metadata,
        CancellationToken cancellationToken);
}
```

Entretanto, operações que exigem consistência com o banco deverão utilizar Outbox.

Publicação direta será reservada para cenários onde perda eventual seja aceitável ou onde não exista transação de banco associada.

---

# 43. Orquestração de Pipelines

A Application será responsável por coordenar a execução lógica do Pipeline.

Componentes possíveis:

```text
IPipelineExecutionOrchestrator
IPipelineStepHandler
IPipelineStepHandlerResolver
IPipelineExecutionRepository
IArtifactRepository
```

Fluxo:

```text
ExecutePipelineStepCommand
   ↓
Carregar execução
   ↓
Iniciar próxima etapa no Domain
   ↓
Persistir estado Running
   ↓
Executar Step Handler
   ↓
Criar Artifact
   ↓
Concluir etapa no Domain
   ↓
Persistir resultado
   ↓
Agendar próxima etapa
```

---

# 44. PipelineStepHandler

Abstração:

```csharp
public interface IPipelineStepHandler
{
    PipelineStepType StepType { get; }

    Task<Result<PipelineStepOutput>> ExecuteAsync(
        PipelineStepContext context,
        CancellationToken cancellationToken);
}
```

Contexto:

```csharp
public sealed record PipelineStepContext(
    OrganizationId OrganizationId,
    ProjectId ProjectId,
    PipelineExecutionId ExecutionId,
    StepExecutionId StepExecutionId,
    PipelineStepType StepType,
    string Configuration,
    IReadOnlyCollection<ArtifactReference> InputArtifacts);
```

Saída:

```csharp
public sealed record PipelineStepOutput(
    ArtifactType ArtifactType,
    ArtifactContent Content,
    PromptVersion? PromptVersion,
    ProviderModel? ProviderModel,
    TokenUsage? TokenUsage);
```

---

# 45. ResearchStepHandler

Responsabilidades:

- Carregar inputs necessários.
    
- Montar o prompt.
    
- Selecionar provider.
    
- Solicitar geração.
    
- Validar structured output.
    
- Produzir `PipelineStepOutput`.
    

Não deverá:

- Alterar diretamente o DbContext.
    
- Publicar RabbitMQ.
    
- Concluir a PipelineExecution.
    
- Controlar retry técnico.
    
- Registrar credenciais.
    
- Conhecer HTTP.
    

---

# 46. ScriptStepHandler

O Script Step poderá utilizar o Artifact de Research como entrada.

Fluxo:

```text
Research Artifact
   ↓
Prompt Builder
   ↓
Text Generation Provider
   ↓
Structured Output Validator
   ↓
Script Artifact
```

O Handler da etapa produz uma saída.

O orquestrador coordena a persistência e a transição da execução.

---

# 47. Prompt Builders

Prompts não deverão ser montados de forma espalhada em Handlers.

Abstrações possíveis:

```csharp
public interface IPromptBuilder<TContext>
{
    Result<PromptDefinition> Build(
        TContext context);
}
```

```csharp
public sealed record PromptDefinition(
    PromptVersion Version,
    string SystemPrompt,
    string UserPrompt,
    StructuredOutputSchema? OutputSchema);
```

Prompts deverão possuir versão explícita.

---

# 48. Operações Longas

Uma operação longa deverá ser dividida em checkpoints.

Evitar:

```text
StartExecutionHandler
   ├── Research
   ├── Script
   ├── Review
   ├── Image
   └── Publication
```

Preferir:

```text
StartExecution
   ↓
Execute Research Step
   ↓
Execute Script Step
   ↓
Await Approval
   ↓
Publish
```

Cada etapa deverá possuir:

- Estado persistido.
    
- Entrada.
    
- Saída.
    
- Tentativas.
    
- Erro.
    
- Checkpoint.
    
- Identificador.
    

---

# 49. CancellationToken

Todo método assíncrono relevante deverá aceitar e propagar `CancellationToken`.

Exemplo:

```csharp
await _provider.GenerateAsync(
    request,
    cancellationToken);
```

O token deverá ser propagado para:

- Repositories.
    
- Unit of Work.
    
- Queries.
    
- HTTP clients.
    
- Storage.
    
- Providers.
    
- Mensageria.
    
- Cache.
    

Cancelamento do request HTTP e cancelamento de negócio são conceitos diferentes.

```text
CancellationToken
    = parar o trabalho técnico atual

CancelExecutionCommand
    = alterar o estado de negócio
```

---

# 50. Timeouts

A Application poderá carregar requisitos de timeout na solicitação.

Exemplo:

```csharp
public sealed record ProviderExecutionOptions(
    TimeSpan Timeout,
    int MaximumOutputTokens);
```

A implementação técnica do timeout ficará em Infrastructure.

A Application deverá tratar timeout como falha classificada.

```text
AI.ProviderTimeout
Storage.Timeout
Messaging.Timeout
```

---

# 51. Result Pattern

Todo caso de uso deverá retornar:

```csharp
Result
```

ou:

```csharp
Result<T>
```

Tipos de erro sugeridos:

```text
Validation
NotFound
Conflict
Unauthorized
Forbidden
RateLimit
Unavailable
Timeout
Failure
```

Exemplo:

```csharp
return Result.Failure<StartExecutionResult>(
    ExecutionErrors.PipelineNotExecutable(
        command.PipelineId));
```

---

# 52. Mapeamento de Erros

A Application retorna erros independentes de HTTP.

Exemplo:

```text
Project.NotFound
Execution.InvalidState
Pipeline.NotPublished
AI.ProviderUnavailable
```

A API decide:

```text
NotFound → HTTP 404
Validation → HTTP 400
Conflict → HTTP 409
Forbidden → HTTP 403
```

O Worker decide:

```text
Timeout transitório → Retry
Validation permanente → Dead Letter
Conflict idempotente → Ack
```

A mesma Application pode ser utilizada por hosts diferentes.

---

# 53. Exceções

Exceções poderão ocorrer para falhas inesperadas.

Exemplos:

- Erro de programação.
    
- Estado interno impossível.
    
- Dependência quebrada de maneira não classificada.
    
- Serialização inesperada.
    
- Corrupção de dados.
    

A Application não deverá capturar exceções indiscriminadamente.

Evitar:

```csharp
catch (Exception)
{
    return Result.Failure(
        Error.Failure("Generic.Error", "Erro."));
}
```

Isso apaga contexto e dificulta observabilidade.

Exceções inesperadas deverão chegar ao tratamento global do host.

---

# 54. Application Services

Application Services poderão existir quando houver lógica de coordenação reutilizada por diferentes casos de uso.

Exemplos aceitáveis:

```text
PipelineExecutionOrchestrator
ArtifactCreationService
PromptRenderingService
PublicationCoordinator
```

Eles deverão possuir responsabilidade específica.

Evitar:

```text
ApplicationService
GeneralManager
BusinessService
CommonProcessor
```

---

# 55. Reutilização entre Handlers

Handlers não deverão chamar outros Handlers diretamente.

Evitar:

```csharp
await _createArtifactHandler.Handle(...);
```

Preferir:

- Application Service específico.
    
- Domain Service.
    
- Command enviado explicitamente.
    
- Domain Event.
    
- Método compartilhado pequeno.
    
- Componente de orquestração.
    

A escolha dependerá do fluxo.

---

# 56. Mapeamento

Mapeamentos simples poderão ocorrer no Handler.

Mapeamentos complexos deverão possuir componentes específicos.

Exemplo:

```csharp
public interface IArtifactMapper
{
    ArtifactDetails Map(
        Artifact artifact);
}
```

Evitar dependência automática de ferramentas de mapping para todo caso de uso.

Mapeamento explícito costuma ser mais legível em operações críticas.

---

# 57. DTOs da Application

A Application poderá possuir modelos internos.

Exemplos:

```text
ProjectDetails
ProjectListItem
ExecutionDetails
StepExecutionDetails
ArtifactDetails
```

Esses modelos não deverão ser tratados automaticamente como contratos públicos.

A API poderá criar sua própria Response.

Contracts distribuídos continuarão no projeto `Contracts`.

---

# 58. Segurança

A Application deverá aplicar regras como:

- Autorização por permissão.
    
- Isolamento por Organization.
    
- Proteção contra IDOR.
    
- Limites de operação.
    
- Validação de referências externas.
    
- Restrições de tipo de arquivo.
    
- Controle de publicação.
    
- Auditoria de ações críticas.
    

Ela não deverá confiar apenas nas validações da API.

Workers e Consumers também utilizam a Application.

---

# 59. Dados Sensíveis

Commands e Results não deverão transportar secrets sem necessidade.

A Application poderá utilizar referências:

```csharp
CredentialReferenceId
```

A resolução do secret ficará em Infrastructure.

Prompts e outputs sensíveis não deverão ser registrados automaticamente em logs.

---

# 60. Observabilidade

Cada caso de uso deverá gerar telemetria suficiente.

Dimensões úteis:

```text
RequestName
OrganizationId
UserId
ProjectId
PipelineId
ExecutionId
StepType
Provider
Model
Result
ErrorCode
Duration
```

A Application poderá utilizar uma abstração leve:

```csharp
public interface IApplicationTelemetry
{
    IDisposable StartOperation(
        string operationName,
        IReadOnlyDictionary<string, object?> attributes);
}
```

Ou utilizar APIs neutras do .NET, desde que não introduzam dependência indevida.

---

# 61. Testes Unitários

Handlers deverão ser testados com dependências falsas ou mocks.

Testar:

- Fluxo de sucesso.
    
- Recurso não encontrado.
    
- Falha de autorização.
    
- Falha do Domain.
    
- Falha do provider.
    
- Persistência chamada corretamente.
    
- Outbox registrada.
    
- CancellationToken propagado.
    
- Resultado retornado.
    
- Idempotência.
    

Exemplo:

```csharp
[Fact]
public async Task Handle_ShouldCreateProject_WhenCommandIsValid()
{
    var repository = new InMemoryProjectRepository();
    var unitOfWork = new FakeUnitOfWork();
    var clock = new FakeClock(TestDates.Now);

    var handler = new CreateProjectHandler(
        repository,
        unitOfWork,
        clock);

    var command = CreateProjectCommandFactory.Valid();

    var result = await handler.Handle(
        command,
        CancellationToken.None);

    result.IsSuccess.Should().BeTrue();
    repository.Items.Should().ContainSingle();
}
```

---

# 62. Testes de Validators

```csharp
[Fact]
public void Validate_ShouldFail_WhenNameIsEmpty()
{
    var validator = new CreateProjectValidator();

    var command = new CreateProjectCommand(
        OrganizationId.New(),
        UserId.New(),
        string.Empty,
        null);

    var result = validator.Validate(command);

    result.IsValid.Should().BeFalse();
}
```

Validators devem ser testados separadamente quando possuírem regras relevantes.

---

# 63. Testes de Behaviors

Behaviors deverão possuir testes para:

- Ordem.
    
- Interrupção do pipeline.
    
- Resultado de validação.
    
- Autorização.
    
- Idempotência.
    
- Transação.
    
- Logging.
    
- Exceções.
    

Exemplo de comportamento esperado:

```text
Validator falhou
   ↓
Handler não é executado
   ↓
Transação não é aberta
```

---

# 64. Testes de Componentes

Testes de componentes poderão executar:

```text
Command
   ↓
Behaviors
   ↓
Handler
   ↓
Data real em container
```

Esses testes validarão a integração da Application com implementações reais sem passar pela API.

---

# 65. Exemplo: CreateProject

Fluxo:

```text
CreateProjectCommand
   ↓
ValidationBehavior
   ↓
AuthorizationBehavior
   ↓
TransactionBehavior
   ↓
CreateProjectHandler
   ├── Cria ProjectName
   ├── Cria Project
   ├── Adiciona repository
   └── SaveChanges
   ↓
CreateProjectResult
```

Resultado:

```csharp
public sealed record CreateProjectResult(
    ProjectId ProjectId,
    string Name,
    DateTimeOffset CreatedAt);
```

---

# 66. Exemplo: StartExecution

Command:

```csharp
public sealed record StartExecutionCommand(
    OrganizationId OrganizationId,
    UserId RequestedBy,
    ProjectId ProjectId,
    PipelineId PipelineId,
    string IdempotencyKey)
    : ICommand<StartExecutionResult>,
      IIdempotentCommand;
```

Fluxo:

```text
Validar Command
   ↓
Carregar Project
   ↓
Carregar Pipeline publicado
   ↓
Validar mesma Organization
   ↓
Criar snapshot da definição
   ↓
PipelineExecution.Queue
   ↓
Persistir execução
   ↓
Registrar Outbox
   ↓
Commit
   ↓
Retornar ExecutionId
```

Resultado:

```csharp
public sealed record StartExecutionResult(
    PipelineExecutionId ExecutionId,
    PipelineExecutionStatus Status,
    DateTimeOffset RequestedAt);
```

---

# 67. Exemplo de StartExecutionHandler

```csharp
public sealed class StartExecutionHandler
    : ICommandHandler<
        StartExecutionCommand,
        StartExecutionResult>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IPipelineRepository _pipelineRepository;
    private readonly IPipelineExecutionRepository _executionRepository;
    private readonly IOutbox _outbox;
    private readonly IClock _clock;

    public async Task<Result<StartExecutionResult>> Handle(
        StartExecutionCommand command,
        CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(
            command.OrganizationId,
            command.ProjectId,
            cancellationToken);

        if (project is null)
        {
            return Result.Failure<StartExecutionResult>(
                ProjectErrors.NotFound(command.ProjectId));
        }

        if (project.Status != ProjectStatus.Active)
        {
            return Result.Failure<StartExecutionResult>(
                ProjectErrors.NotActive(command.ProjectId));
        }

        var pipeline = await _pipelineRepository.GetPublishedAsync(
            command.OrganizationId,
            command.PipelineId,
            cancellationToken);

        if (pipeline is null)
        {
            return Result.Failure<StartExecutionResult>(
                PipelineErrors.PublishedVersionNotFound(
                    command.PipelineId));
        }

        if (pipeline.ProjectId != project.Id)
        {
            return Result.Failure<StartExecutionResult>(
                PipelineErrors.ProjectMismatch);
        }

        var executionResult =
            PipelineExecution.Queue(
                command.OrganizationId,
                project.Id,
                pipeline.Id,
                pipeline.Version,
                command.RequestedBy,
                _clock.UtcNow,
                pipeline.CreateExecutionSnapshot());

        if (executionResult.IsFailure)
        {
            return Result.Failure<StartExecutionResult>(
                executionResult.Error);
        }

        await _executionRepository.AddAsync(
            executionResult.Value,
            cancellationToken);

        await _outbox.AddAsync(
            new PipelineExecutionRequestedV1(
                executionResult.Value.Id.Value,
                pipeline.Id.Value,
                command.OrganizationId.Value,
                _clock.UtcNow),
            OutboxMetadata.Create(
                command.OrganizationId,
                command.IdempotencyKey),
            cancellationToken);

        return new StartExecutionResult(
            executionResult.Value.Id,
            executionResult.Value.Status,
            executionResult.Value.RequestedAt);
    }
}
```

O commit poderá ser realizado pelo TransactionBehavior.

---

# 68. Exemplo: ExecutePipelineStep

Fluxo recomendado:

```text
Mensagem recebida
   ↓
ExecutePipelineStepCommand
   ↓
Carregar execução
   ↓
Verificar estado
   ↓
Iniciar etapa no Domain
   ↓
Persistir Running
   ↓
Executar IPipelineStepHandler
   ↓
Criar Artifact
   ↓
Concluir etapa
   ↓
Persistir mudanças
   ↓
Agendar próxima etapa
```

A chamada externa não deverá ocorrer dentro de uma transação longa.

A operação poderá ser dividida em dois ou mais commits.

---

# 69. Estratégia de Execução de Step

Uma possível divisão:

## Fase 1 — Claim

```text
Carregar execução
Iniciar etapa
Incrementar tentativa
Commit
```

## Fase 2 — Execute

```text
Chamar provider
Produzir resultado
```

## Fase 3 — Complete

```text
Criar Artifact
Concluir etapa
Registrar próxima mensagem
Commit
```

Essa divisão reduz locks e facilita recuperação.

---

# 70. Recuperação após Falha

Se o Worker falhar depois do Claim:

```text
Step = Running
Worker morreu
```

Um processo de reconciliação poderá detectar:

- Etapa Running por tempo excessivo.
    
- Lease expirado.
    
- Falta de heartbeat.
    
- Tentativa incompleta.
    

A Application poderá possuir um caso de uso:

```text
RecoverStalledExecutionCommand
```

Esse caso de uso decidirá:

- Reagendar.
    
- Marcar como falha.
    
- Incrementar tentativa.
    
- Solicitar intervenção.
    

---

# 71. Políticas de Retry

A Application poderá utilizar:

```csharp
public interface IRetryPolicyResolver
{
    RetryPolicy Resolve(
        PipelineStepType stepType,
        Error error,
        int attemptCount);
}
```

```csharp
public sealed record RetryPolicy(
    bool CanRetry,
    int MaximumAttempts,
    TimeSpan Delay);
```

O Domain decide se a etapa pode ser retomada.

Infrastructure aplica o mecanismo técnico.

Worker agenda a nova tentativa.

---

# 72. Feature Flags

A Application poderá consultar abstrações de feature flags:

```csharp
public interface IFeatureManager
{
    Task<bool> IsEnabledAsync(
        string feature,
        OrganizationId organizationId,
        CancellationToken cancellationToken);
}
```

Casos de uso críticos não deverão depender de flags espalhadas por todo o Handler.

Preferir Policies ou estratégias claramente nomeadas.

---

# 73. Configurações

A Application não deverá acessar `IConfiguration`.

Quando precisar de opções estáveis, receberá modelos tipados.

Exemplo:

```csharp
public sealed record PipelineExecutionLimits(
    int MaximumConcurrentExecutions,
    int MaximumSteps,
    TimeSpan MaximumDuration);
```

Esses modelos poderão ser registrados pelo host.

---

# 74. Antipadrões

## Handler como Domain

Evitar concentrar regras de negócio no Handler.

## Handler como Infrastructure

Evitar chamadas diretas a SDKs.

## DbContext na Application

A Application não deverá depender de EF Core.

## IConfiguration espalhado

Configurações deverão ser tipadas.

## Exceção para fluxo esperado

Not found e conflito devem usar Result.

## Queries carregando Aggregate sem necessidade

Utilizar projeções especializadas.

## Handler chamando Handler

Extrair coordenação explícita.

## Transação envolvendo chamada externa longa

Persistir estados intermediários.

## Commands com dezenas de propriedades opcionais

Dividir a intenção ou criar modelos estruturados.

## DTO único para todas as camadas

Cada limite possui seu contrato.

## Interface para toda classe

Criar interfaces somente em limites reais ou pontos de substituição.

---

# 75. Regras Arquiteturais

1. Application depende apenas de Domain, Contracts e SharedKernel.
    
2. Commands alteram estado.
    
3. Queries não alteram estado de negócio.
    
4. Cada caso de uso possui Handler explícito.
    
5. Handlers coordenam, não concentram regras de domínio.
    
6. Validators não acessam banco.
    
7. Autorização considera Organization.
    
8. Repositories são orientados a Aggregates.
    
9. Queries usam modelos de leitura.
    
10. Application não conhece DbContext.
    
11. Application não conhece SDKs externos.
    
12. Application não publica diretamente no RabbitMQ em transações.
    
13. Operações consistentes utilizam Outbox.
    
14. Commands sensíveis suportam idempotência.
    
15. Operações longas são divididas em etapas.
    
16. CancellationToken é propagado.
    
17. Erros esperados utilizam Result.
    
18. Exceções inesperadas chegam ao tratamento global.
    
19. DTOs da Application não são entidades.
    
20. Integration Events utilizam Contracts.
    
21. OrganizationId compõe consultas e chaves.
    
22. Logs não registram dados sensíveis automaticamente.
    
23. Configurações são tipadas.
    
24. Handlers não instanciam outros Handlers.
    
25. Testes validam comportamento e coordenação.
    

---

# 76. Escopo do MVP

A primeira versão da Application deverá implementar:

## Projects

```text
CreateProject
GetProject
ListProjects
```

## Pipelines

```text
CreatePipeline
AddPipelineStep
PublishPipeline
GetPipeline
```

## Executions

```text
StartExecution
GetExecution
ExecutePipelineStep
FailPipelineStep
CancelExecution
```

## Artifacts

```text
CreateArtifact
GetArtifact
ListExecutionArtifacts
```

## Steps

```text
ResearchStepHandler
ScriptStepHandler
```

---

# 77. Abstrações Necessárias no MVP

```text
IProjectRepository
IPipelineRepository
IPipelineExecutionRepository
IArtifactRepository
IProjectQueries
IPipelineQueries
IExecutionQueries
IArtifactQueries
IUnitOfWork
IOutbox
IClock
ICurrentUser
ITextGenerationProvider
ITextGenerationProviderResolver
IStructuredOutputValidator
IPipelineStepHandler
IPipelineStepHandlerResolver
```

Abstrações não necessárias inicialmente deverão aguardar uma necessidade concreta.

---

# 78. Ordem de Implementação

## Fundação

- Result contracts.
    
- ICommand.
    
- IQuery.
    
- Handlers base.
    
- ValidationBehavior.
    
- LoggingBehavior.
    
- TransactionBehavior.
    
- Abstrações de persistência.
    

## Projects

- CreateProject.
    
- GetProject.
    
- ListProjects.
    

## Pipelines

- CreatePipeline.
    
- AddPipelineStep.
    
- PublishPipeline.
    
- GetPipeline.
    

## Executions

- StartExecution.
    
- GetExecution.
    
- ExecutePipelineStep.
    
- CancelExecution.
    

## IA

- TextGeneration contracts.
    
- Fake provider.
    
- Research Step.
    
- Script Step.
    
- Structured Output.
    

## Artifacts

- CreateArtifact.
    
- GetArtifact.
    
- ListExecutionArtifacts.
    

---

# 79. Checklist para Novo Command

- Representa uma intenção clara?
    
- O nome utiliza um verbo?
    
- Altera estado?
    
- Possui OrganizationId?
    
- Possui UserId quando necessário?
    
- Possui IdempotencyKey quando necessário?
    
- Os dados são mínimos?
    
- Existe Validator?
    
- Existe autorização?
    
- O Handler coordena apenas esse caso?
    
- A regra central está no Domain?
    
- A operação precisa de transação?
    
- A operação precisa de Outbox?
    
- CancellationToken é propagado?
    
- Há testes de sucesso e falha?
    

---

# 80. Checklist para Nova Query

- Representa uma consulta clara?
    
- Não altera estado?
    
- Possui OrganizationId?
    
- Precisa de paginação?
    
- Possui ordenação determinística?
    
- Retorna DTO específico?
    
- Evita carregar Aggregate completo?
    
- Aplica autorização?
    
- Não expõe dados de outro tenant?
    
- Possui testes?
    
- Pode usar cache com segurança?
    

---

# 81. Checklist para Novo Handler

- Está pequeno e legível?
    
- Carrega somente os dados necessários?
    
- Verifica not found?
    
- Verifica autorização?
    
- Invoca métodos do Domain?
    
- Não acessa DbContext?
    
- Não chama SDK externo?
    
- Não captura Exception genericamente?
    
- Propaga CancellationToken?
    
- Retorna Result?
    
- Registra Outbox quando necessário?
    
- Evita transação longa?
    
- Possui testes proporcionais ao risco?
    

---

# 82. Critérios de Qualidade

A Application será considerada saudável quando:

- Casos de uso forem facilmente localizáveis.
    
- Handlers puderem ser compreendidos rapidamente.
    
- Regras de negócio permanecerem no Domain.
    
- Banco e providers puderem ser substituídos.
    
- API e Worker reutilizarem os mesmos casos de uso.
    
- Falhas esperadas forem previsíveis.
    
- Autorização e tenancy forem consistentes.
    
- Operações longas puderem ser recuperadas.
    
- Mensagens não forem perdidas após commits.
    
- Testes rodarem sem serviços externos reais.
    
- Novas features puderem ser adicionadas sem criar services gigantes.
    

---

# 83. Documentos Relacionados

```text
04 - Backend/Visão Geral do Backend.md
04 - Backend/Organização por Features.md
04 - Backend/Domain.md
04 - Backend/API.md
04 - Backend/Data.md
04 - Backend/Infrastructure.md
04 - Backend/Worker.md
04 - Backend/Contracts.md
04 - Backend/Shared Kernel.md
```

---

# 84. Filosofia Final

A Application transforma intenções em fluxos coordenados.

Ela deverá expressar operações como:

```text
Criar projeto
Publicar pipeline
Iniciar execução
Executar etapa
Criar artefato
Cancelar execução
Consultar resultado
```

Ela não deverá expressar detalhes como:

```text
Executar SQL
Abrir canal RabbitMQ
Montar requisição HTTP
Resolver secret
Manipular BlobClient
Configurar retry Polly
```

Esses detalhes pertencem às camadas externas.

A regra principal será:

> A Application coordena os casos de uso, o Domain protege as regras e a Infrastructure executa os detalhes técnicos.

Quando essa separação for respeitada, o mesmo caso de uso poderá ser acionado por API, Worker, mensageria, scheduler ou automação sem duplicar a lógica central.