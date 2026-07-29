# Data

## 1. Objetivo

O projeto `Data` será responsável pela persistência relacional do Infinite Content AI.

Ele implementará os mecanismos necessários para armazenar, consultar e atualizar dados no PostgreSQL, utilizando o Entity Framework Core como tecnologia principal de acesso.

O projeto deverá fornecer implementações para as abstrações de persistência definidas pela Application, mantendo os detalhes do banco isolados das regras de negócio.

Fluxo conceitual:

```text
Application
    ↓ abstrações
Data
    ├── EF Core
    ├── PostgreSQL
    ├── Repositories
    ├── Queries
    ├── Transactions
    ├── Outbox
    ├── Inbox
    └── Migrations
```

O Data deverá responder perguntas como:

- Como os Aggregates são persistidos?
    
- Como os Value Objects são mapeados?
    
- Como garantir isolamento entre Organizations?
    
- Como controlar concorrência?
    
- Como registrar Domain Events na Outbox?
    
- Como evitar processamento duplicado de mensagens?
    
- Como realizar migrations?
    
- Como executar consultas eficientes?
    
- Como manter transações consistentes?
    
- Como testar persistência com PostgreSQL real?
    

---

# 2. Responsabilidades

O projeto `Data` será responsável por:

- `DbContext`.
    
- Configuração do Entity Framework Core.
    
- PostgreSQL.
    
- Mapeamentos de entidades.
    
- Mapeamentos de Value Objects.
    
- Repositories concretos.
    
- Queries especializadas.
    
- Unit of Work.
    
- Transações.
    
- Migrations.
    
- Interceptors relacionados à persistência.
    
- Controle de concorrência.
    
- Outbox persistida.
    
- Inbox persistida.
    
- Idempotência persistente.
    
- Paginação.
    
- Projeções.
    
- Índices.
    
- Constraints.
    
- Estratégias de exclusão.
    
- Conversores.
    
- Seeds técnicos controlados.
    
- Testes de integração com banco.
    
- Limpeza e retenção de registros técnicos.
    

O projeto não será responsável por:

- Providers de inteligência artificial.
    
- RabbitMQ.
    
- Redis.
    
- Azure Blob Storage.
    
- APIs externas.
    
- n8n.
    
- Serviços de e-mail.
    
- Observabilidade externa.
    
- Clientes HTTP.
    
- Regras centrais de negócio.
    
- Endpoints.
    
- Consumers.
    
- Background Services.
    

---

# 3. Dependências Permitidas

O projeto `Data` poderá depender de:

```text
Application
Domain
SharedKernel
```

O projeto não poderá depender de:

```text
Api
Infrastructure
Worker
```

Fluxo:

```text
Application
    ↑
Data
    ↓
Domain
    ↓
SharedKernel
```

O Data implementa interfaces da Application e persiste objetos do Domain.

---

# 4. Separação entre Data e Infrastructure

A separação entre os projetos deverá ser rígida.

## Data

Responsável por:

```text
PostgreSQL
EF Core
DbContext
Repositories
Queries
Transactions
Migrations
Outbox persistida
Inbox persistida
```

## Infrastructure

Responsável por:

```text
RabbitMQ
Redis
Azure
IA Providers
n8n
APIs externas
Storage
Observabilidade
```

O Data nunca deverá:

- Publicar mensagens no RabbitMQ.
    
- Chamar providers de IA.
    
- Acessar Azure Storage.
    
- Executar workflows do n8n.
    
- Criar clientes HTTP.
    
- Resolver secrets externos.
    
- Utilizar SDKs de serviços externos.
    

A Infrastructure nunca deverá possuir `DbContext`.

---

# 5. Tecnologia Principal

O banco principal será:

```text
PostgreSQL
```

O ORM principal será:

```text
Entity Framework Core
```

O PostgreSQL será a fonte principal da verdade para:

- Organizations.
    
- Projects.
    
- Pipelines.
    
- Pipeline Definitions.
    
- Executions.
    
- Step Executions.
    
- Artifacts.
    
- Approvals.
    
- Publications.
    
- Outbox.
    
- Inbox.
    
- Idempotency Records.
    
- Auditoria persistente.
    
- Metadados de storage.
    
- Configurações de negócio.
    

---

# 6. Estrutura do Projeto

```text
Data
│
├── Context
│   ├── ApplicationDbContext.cs
│   ├── ApplicationDbContextFactory.cs
│   └── DatabaseConstants.cs
│
├── Common
│   ├── Configurations
│   ├── Converters
│   ├── Extensions
│   ├── Interceptors
│   ├── Queries
│   ├── Transactions
│   └── Concurrency
│
├── Features
│   ├── Organizations
│   ├── Projects
│   ├── Pipelines
│   ├── Executions
│   ├── Artifacts
│   ├── Approvals
│   └── Publications
│
├── Outbox
│   ├── OutboxMessage.cs
│   ├── OutboxMessageConfiguration.cs
│   ├── EfOutbox.cs
│   ├── DomainEventInterceptor.cs
│   └── OutboxQueries.cs
│
├── Inbox
│   ├── InboxMessage.cs
│   ├── InboxMessageConfiguration.cs
│   ├── EfInbox.cs
│   └── InboxQueries.cs
│
├── Idempotency
│   ├── IdempotencyRecord.cs
│   ├── IdempotencyRecordConfiguration.cs
│   └── EfIdempotencyStore.cs
│
├── Migrations
├── Seeds
├── DependencyInjection.cs
└── DataOptions.cs
```

---

# 7. Organização por Feature

Mapeamentos, repositories e queries deverão permanecer próximos ao módulo correspondente.

Exemplo:

```text
Data
└── Features
    └── Projects
        ├── ProjectConfiguration.cs
        ├── ProjectRepository.cs
        ├── ProjectQueries.cs
        └── ProjectReadModels.cs
```

Para execuções:

```text
Data
└── Features
    └── Executions
        ├── PipelineExecutionConfiguration.cs
        ├── StepExecutionConfiguration.cs
        ├── PipelineExecutionRepository.cs
        ├── ExecutionQueries.cs
        └── ExecutionReadModels.cs
```

Recursos técnicos compartilhados permanecerão em pastas próprias.

---

# 8. ApplicationDbContext

O `ApplicationDbContext` será o contexto principal de persistência.

Exemplo conceitual:

```csharp
public sealed class ApplicationDbContext
    : DbContext,
      IUnitOfWork
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations =>
        Set<Organization>();

    public DbSet<Project> Projects =>
        Set<Project>();

    public DbSet<Pipeline> Pipelines =>
        Set<Pipeline>();

    public DbSet<PipelineExecution> PipelineExecutions =>
        Set<PipelineExecution>();

    public DbSet<Artifact> Artifacts =>
        Set<Artifact>();

    internal DbSet<OutboxMessage> OutboxMessages =>
        Set<OutboxMessage>();

    internal DbSet<InboxMessage> InboxMessages =>
        Set<InboxMessage>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
```

---

# 9. Exposição de DbSets

Somente DbSets necessários deverão ser expostos publicamente.

Registros técnicos poderão permanecer internos:

```csharp
internal DbSet<OutboxMessage> OutboxMessages =>
    Set<OutboxMessage>();
```

Endpoints, Handlers e Consumers não deverão acessar o DbContext diretamente.

O acesso ocorrerá por:

- Repositories.
    
- Interfaces de Queries.
    
- Unit of Work.
    
- Abstrações técnicas específicas.
    

---

# 10. Configuração do DbContext

Registro conceitual:

```csharp
services.AddDbContext<ApplicationDbContext>(
    (serviceProvider, options) =>
    {
        var dataOptions =
            serviceProvider
                .GetRequiredService<IOptions<DataOptions>>()
                .Value;

        options.UseNpgsql(
            dataOptions.ConnectionString,
            npgsql =>
            {
                npgsql.MigrationsAssembly(
                    typeof(ApplicationDbContext)
                        .Assembly
                        .FullName);

                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay:
                        TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });

        options.UseSnakeCaseNamingConvention();
    });
```

Retries do provider do EF Core deverão ser utilizados cuidadosamente.

Retries não substituem:

- Idempotência.
    
- Outbox.
    
- Tratamento de concorrência.
    
- Políticas de mensagens.
    
- Reconciliação.
    

---

# 11. DataOptions

```csharp
public sealed class DataOptions
{
    public const string SectionName = "Data";

    public required string ConnectionString { get; init; }

    public int CommandTimeoutSeconds { get; init; } = 30;

    public int MaximumRetryCount { get; init; } = 3;

    public bool EnableDetailedErrors { get; init; }

    public bool EnableSensitiveDataLogging { get; init; }
}
```

Configurações obrigatórias deverão ser validadas no startup.

`EnableSensitiveDataLogging` não deverá ser habilitado em produção.

---

# 12. Convenções de Nomenclatura

O banco utilizará:

```text
snake_case
```

Exemplos:

```text
projects
pipeline_executions
step_executions
organization_id
created_at
pipeline_version
```

Convenções:

- Tabelas no plural.
    
- Nomes em inglês.
    
- Colunas em `snake_case`.
    
- Primary keys como `id`.
    
- Foreign keys como `{resource}_id`.
    
- Datas com sufixo `_at`.
    
- Flags booleanas com prefixo `is_`, `has_` ou nome semanticamente claro.
    
- Índices com nomes previsíveis.
    
- Constraints com nomes explícitos.
    

---

# 13. Schemas do PostgreSQL

Inicialmente poderá ser utilizado o schema padrão:

```text
public
```

Com o crescimento, schemas poderão separar áreas técnicas:

```text
app
messaging
audit
identity
```

Exemplo futuro:

```text
app.projects
app.pipeline_executions
messaging.outbox_messages
messaging.inbox_messages
```

Para o MVP, a simplicidade será priorizada.

A adoção de múltiplos schemas não deverá bloquear a implementação inicial.

---

# 14. Configurações do EF Core

Cada Entity deverá possuir uma configuração própria.

Exemplo:

```csharp
public sealed class ProjectConfiguration
    : IEntityTypeConfiguration<Project>
{
    public void Configure(
        EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");

        builder.HasKey(project => project.Id);

        builder.Property(project => project.Id)
            .HasConversion(
                id => id.Value,
                value => new ProjectId(value))
            .ValueGeneratedNever();

        builder.Property(project => project.Name)
            .HasConversion(
                name => name.Value,
                value => ProjectName.FromPersistence(value))
            .HasMaxLength(ProjectName.MaximumLength)
            .IsRequired();

        builder.Property(project => project.Description)
            .HasMaxLength(2_000);

        builder.Property(project => project.Status)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();
    }
}
```

O método `FromPersistence` deverá ser controlado para não permitir criação pública de estado inválido.

---

# 15. Configurações fora do DbContext

Evitar:

```csharp
protected override void OnModelCreating(
    ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Project>(builder =>
    {
        // centenas de linhas
    });
}
```

Preferir:

```text
ProjectConfiguration
PipelineConfiguration
PipelineExecutionConfiguration
ArtifactConfiguration
```

Benefícios:

- Organização.
    
- Testabilidade.
    
- Navegação.
    
- Revisão de código.
    
- Separação por feature.
    

---

# 16. Mapeamento de Identificadores Fortemente Tipados

Exemplo:

```csharp
builder.Property(project => project.Id)
    .HasConversion(
        id => id.Value,
        value => new ProjectId(value))
    .ValueGeneratedNever();
```

O identificador será gerado no Domain ou Application.

O banco não deverá gerar o ID por padrão.

Benefícios:

- Eventos podem referenciar o ID antes do commit.
    
- Criação distribuída.
    
- Menor acoplamento.
    
- Facilidade de idempotência.
    
- Uso de UUID v7.
    

---

# 17. UUID v7

UUID v7 será preferido para identificadores.

Benefícios:

- Ordenação temporal aproximada.
    
- Menor fragmentação de índices.
    
- Geração distribuída.
    
- Compatibilidade com UUID.
    
- Identificadores opacos.
    

Exemplo:

```csharp
Guid.CreateVersion7();
```

O banco deverá utilizar o tipo:

```text
uuid
```

Clientes continuarão tratando IDs como valores opacos.

---

# 18. Mapeamento de Value Objects

Value Objects simples poderão ser convertidos em colunas.

Exemplo:

```csharp
builder.Property(project => project.Name)
    .HasConversion(
        name => name.Value,
        value => ProjectName.FromPersistence(value));
```

Value Objects compostos poderão utilizar:

- Owned Types.
    
- Complex Types.
    
- Conversores.
    
- Colunas separadas.
    
- JSONB, quando apropriado.
    

A escolha deverá considerar:

- Necessidade de consulta.
    
- Indexação.
    
- Integridade.
    
- Evolução.
    
- Frequência de alteração.
    

---

# 19. Owned Types e Complex Types

Exemplo conceitual:

```csharp
builder.OwnsOne(
    artifact => artifact.ProviderModel,
    owned =>
    {
        owned.Property(model => model.Provider)
            .HasColumnName("provider");

        owned.Property(model => model.Model)
            .HasColumnName("provider_model");
    });
```

Owned Types não deverão ser utilizados automaticamente.

Eles podem introduzir:

- Mapeamentos complexos.
    
- Tracking inesperado.
    
- Dificuldade em migrations.
    
- Colunas pouco claras.
    

---

# 20. JSONB

PostgreSQL `jsonb` poderá ser utilizado para dados:

- Estruturados, mas variáveis.
    
- Específicos por tipo de Pipeline Step.
    
- Metadados de provider.
    
- Structured Outputs.
    
- Checkpoints.
    
- Configurações versionadas.
    

Exemplos:

```text
pipeline_step_definitions.configuration
execution_checkpoints.data
artifacts.structured_content
outbox_messages.payload
```

JSONB não deverá substituir modelagem relacional quando os dados:

- São consultados frequentemente.
    
- Participam de relacionamentos.
    
- Exigem constraints fortes.
    
- Precisam de índices específicos.
    
- Representam conceitos centrais.
    

---

# 21. Mapeamento de Pipeline

Estrutura possível:

```text
pipelines
pipeline_step_definitions
```

Relacionamento:

```text
pipelines
    └── pipeline_step_definitions
```

A configuração deverá proteger:

- Ordem.
    
- Identidade.
    
- Cascade apropriado.
    
- Unicidade de posição.
    
- Versionamento.
    

Exemplo:

```csharp
builder.HasMany<PipelineStepDefinition>("_steps")
    .WithOne()
    .HasForeignKey("pipeline_id")
    .OnDelete(DeleteBehavior.Cascade);
```

O acesso continuará encapsulado no Domain.

---

# 22. Constraint de Posição

A posição de uma etapa deverá ser única dentro do Pipeline.

Exemplo SQL conceitual:

```sql
create unique index
    ux_pipeline_steps_pipeline_position
on pipeline_step_definitions (
    pipeline_id,
    position
);
```

Essa constraint complementa a invariante do Domain.

---

# 23. Mapeamento de PipelineExecution

Estrutura possível:

```text
pipeline_executions
step_executions
execution_checkpoints
```

O Aggregate poderá ser persistido em múltiplas tabelas.

Exemplo:

```csharp
builder.HasMany<StepExecution>("_steps")
    .WithOne()
    .HasForeignKey("pipeline_execution_id")
    .OnDelete(DeleteBehavior.Cascade);
```

Step Executions não deverão ser carregadas sempre que uma query de listagem não precisar delas.

Queries de leitura deverão utilizar projeções.

---

# 24. Snapshots de Pipeline

Uma execução deverá preservar a definição utilizada.

Possíveis estratégias:

## Snapshot JSONB

```text
pipeline_executions.pipeline_snapshot
```

Vantagens:

- Simples.
    
- Imutável.
    
- Fácil reprodutibilidade.
    
- Leitura em um único registro.
    

Desvantagens:

- Menor integridade relacional.
    
- Consultas internas mais difíceis.
    

## Tabelas versionadas

```text
pipeline_versions
pipeline_version_steps
```

Vantagens:

- Forte estrutura.
    
- Melhor consulta.
    
- Versionamento explícito.
    

Desvantagens:

- Mais tabelas.
    
- Mais complexidade.
    

Para o MVP, poderá ser utilizada uma tabela de versões:

```text
pipeline_versions
pipeline_version_steps
```

ou um snapshot JSONB, desde que seja imutável.

A escolha final deverá priorizar reprodutibilidade sem excesso de complexidade.

---

# 25. Mapeamento de Artifact

Estrutura inicial:

```text
artifacts
```

Campos possíveis:

```text
id
organization_id
project_id
execution_id
step_execution_id
type
status
version
content_kind
text_content
structured_content
storage_provider
storage_object_key
content_type
size
checksum
provider
provider_model
prompt_version
previous_version_id
created_at
```

Nem todos os campos serão preenchidos para todos os tipos.

Uma estratégia alternativa será separar:

```text
artifacts
artifact_text_contents
artifact_storage_contents
```

Para o MVP, uma tabela com colunas opcionais controladas poderá ser suficiente.

---

# 26. Constraints de Artifact

Exemplos:

- `version > 0`.
    
- `size >= 0`.
    
- `text_content` obrigatório quando `content_kind = text`.
    
- `storage_object_key` obrigatório quando `content_kind = storage`.
    
- `checksum` obrigatório para arquivos.
    
- Unicidade por versão dentro da cadeia.
    

Constraints de banco deverão complementar o Domain.

---

# 27. Enums no Banco

Enums poderão ser persistidos como strings.

Exemplo:

```csharp
builder.Property(execution => execution.Status)
    .HasConversion<string>()
    .HasMaxLength(40);
```

Vantagens:

- Melhor legibilidade.
    
- Menor risco ao reordenar enum.
    
- Queries manuais compreensíveis.
    

Desvantagens:

- Maior espaço.
    
- Renomeações exigem migration.
    

Enums numéricos somente deverão ser utilizados quando houver benefício mensurável e controle rígido.

---

# 28. Datas

Datas serão persistidas como:

```text
timestamp with time zone
```

No .NET:

```csharp
DateTimeOffset
```

Toda data deverá representar UTC.

Evitar:

```text
timestamp without time zone
```

para instantes de negócio.

Datas locais de calendário, quando necessárias, deverão utilizar tipos próprios, como `DateOnly`.

---

# 29. Precisão Temporal

O banco e a aplicação deverão utilizar precisão consistente.

Datas não deverão depender de comparações com precisão superior à suportada.

Testes deverão evitar expectativas frágeis com nanossegundos ou ticks diferentes.

---

# 30. Repositories

Repositories concretos implementarão interfaces da Application.

Exemplo:

```csharp
public sealed class ProjectRepository
    : IProjectRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ProjectRepository(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Project?> GetByIdAsync(
        OrganizationId organizationId,
        ProjectId projectId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Projects
            .SingleOrDefaultAsync(
                project =>
                    project.OrganizationId ==
                        organizationId &&
                    project.Id == projectId,
                cancellationToken);
    }

    public async Task AddAsync(
        Project project,
        CancellationToken cancellationToken)
    {
        await _dbContext.Projects.AddAsync(
            project,
            cancellationToken);
    }
}
```

---

# 31. Repositories Orientados a Aggregates

Cada Repository deverá possuir operações necessárias ao Aggregate.

Exemplos:

```text
IProjectRepository
IPipelineRepository
IPipelineExecutionRepository
IArtifactRepository
```

Evitar:

```csharp
IRepository<TEntity>
```

Um repository genérico costuma permitir:

- Atualização de entidades internas.
    
- Queries sem tenant.
    
- Operações inadequadas.
    
- Vazamento de persistência.
    
- Falta de intenção.
    

---

# 32. Métodos de Repository

Métodos deverão expressar necessidades reais.

Exemplos:

```csharp
Task<Pipeline?> GetDraftAsync(
    OrganizationId organizationId,
    PipelineId pipelineId,
    CancellationToken cancellationToken);

Task<Pipeline?> GetPublishedAsync(
    OrganizationId organizationId,
    PipelineId pipelineId,
    CancellationToken cancellationToken);

Task<PipelineExecution?> GetForProcessingAsync(
    OrganizationId organizationId,
    PipelineExecutionId executionId,
    CancellationToken cancellationToken);
```

Evitar métodos excessivamente genéricos:

```csharp
FindAsync(Expression<Func<TEntity, bool>> predicate);
```

A expressão vaza detalhes de persistência para a Application.

---

# 33. Tracking

Repositories de escrita utilizarão tracking.

Queries de leitura deverão utilizar:

```csharp
AsNoTracking()
```

Exemplo:

```csharp
return await _dbContext.Projects
    .AsNoTracking()
    .Where(project =>
        project.OrganizationId == organizationId)
    .Select(project => new ProjectListItem(
        project.Id,
        project.Name.Value,
        project.Status.ToString(),
        project.CreatedAt))
    .ToListAsync(cancellationToken);
```

Tracking desnecessário aumenta:

- Uso de memória.
    
- Custo de CPU.
    
- Complexidade do Change Tracker.
    

---

# 34. Queries Especializadas

Interfaces de leitura serão implementadas no Data.

Exemplo:

```csharp
public sealed class ProjectQueries
    : IProjectQueries
{
    private readonly ApplicationDbContext _dbContext;

    public ProjectQueries(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ProjectDetails?> GetDetailsAsync(
        OrganizationId organizationId,
        ProjectId projectId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Projects
            .AsNoTracking()
            .Where(project =>
                project.OrganizationId == organizationId &&
                project.Id == projectId)
            .Select(project => new ProjectDetails(
                project.Id,
                project.Name.Value,
                project.Description,
                project.Status.ToString(),
                project.CreatedAt))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
```

Queries não precisam materializar Aggregates.

---

# 35. Projeções

Preferir projeções no banco:

```csharp
.Select(project => new ProjectListItem(...))
```

Evitar:

```csharp
var projects = await query.ToListAsync();
return projects.Select(project => Map(project));
```

quando isso carrega colunas desnecessárias.

Benefícios:

- Menor tráfego.
    
- Menor uso de memória.
    
- Melhor desempenho.
    
- SQL mais específico.
    

---

# 36. Paginação

Exemplo:

```csharp
var totalCount = await query.LongCountAsync(
    cancellationToken);

var items = await query
    .OrderByDescending(project => project.CreatedAt)
    .ThenBy(project => project.Id)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .Select(...)
    .ToListAsync(cancellationToken);
```

A ordenação deverá ser determinística.

Sempre adicionar um desempate estável:

```text
created_at desc, id desc
```

---

# 37. Paginação por Cursor

Para volumes maiores:

```sql
where (created_at, id) < (@created_at, @id)
order by created_at desc, id desc
limit @limit
```

Cursor pagination reduz o custo de offsets elevados.

Ela poderá ser introduzida em:

- Executions.
    
- Artifacts.
    
- Publications.
    
- Logs de auditoria.
    

---

# 38. Compiled Queries

Compiled Queries poderão ser utilizadas em consultas:

- Frequentes.
    
- Estáveis.
    
- Identificadas como gargalo.
    
- Medidas por profiling.
    

Não deverão ser usadas prematuramente.

O código deve permanecer simples até existir evidência de benefício.

---

# 39. SQL Direto

SQL direto poderá ser utilizado quando:

- LINQ gerar SQL inadequado.
    
- A query for muito complexa.
    
- Houver necessidade de recursos específicos do PostgreSQL.
    
- Performance for crítica.
    
- O comportamento estiver coberto por testes.
    

Possíveis opções:

```text
EF Core ExecuteSql
FromSql
NpgsqlCommand
Dapper
```

SQL deverá permanecer no projeto Data.

---

# 40. Dapper

Dapper poderá ser introduzido futuramente para consultas especializadas.

Regras:

- Apenas leitura ou operações claramente controladas.
    
- Parâmetros obrigatórios.
    
- Nenhuma concatenação de entrada.
    
- Mapeamento explícito.
    
- Testes de integração.
    
- Mesma transação quando necessário.
    
- Não substituir EF Core sem justificativa.
    

---

# 41. Unit of Work

O `ApplicationDbContext` implementará:

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken);
}
```

A Application não conhecerá o DbContext.

O Unit of Work agrupará alterações realizadas dentro do mesmo escopo.

---

# 42. SaveChanges

O `SaveChangesAsync` será o ponto de persistência.

Ele poderá coordenar:

- Domain Events.
    
- Outbox.
    
- Auditoria técnica.
    
- Concurrency tokens.
    
- Normalização.
    
- Validações adicionais.
    

A quantidade de efeitos no SaveChanges deverá permanecer controlada e testável.

---

# 43. TransactionBehavior

Commands poderão ser envolvidos por um Behavior transacional.

Fluxo:

```text
Command
    ↓
TransactionBehavior
    ↓
Handler
    ↓
SaveChanges
    ↓
Commit
```

O Behavior poderá detectar Commands que exigem transação.

Queries não deverão abrir transações de escrita.

---

# 44. Transações Explícitas

Abstração:

```csharp
public interface ITransactionManager
{
    Task<IApplicationTransaction> BeginAsync(
        CancellationToken cancellationToken);
}
```

Implementação:

```csharp
public sealed class EfTransactionManager
    : ITransactionManager
{
    private readonly ApplicationDbContext _dbContext;
}
```

A Application não deverá conhecer `IDbContextTransaction`.

---

# 45. Isolation Level

O nível padrão deverá ser:

```text
Read Committed
```

Níveis mais fortes poderão ser utilizados quando necessário:

```text
Repeatable Read
Serializable
```

Eles deverão ser aplicados apenas em casos específicos porque podem aumentar:

- Locks.
    
- Contenção.
    
- Deadlocks.
    
- Latência.
    
- Retries.
    

Concorrência otimista será preferida.

---

# 46. Chamadas Externas e Transações

Nunca manter uma transação aberta durante:

- Chamada de IA.
    
- Upload de arquivo.
    
- Download externo.
    
- Publicação em rede social.
    
- Comunicação com n8n.
    
- Espera de usuário.
    
- Delay de retry.
    

Fluxo correto:

```text
Persistir estado Running
Commit
    ↓
Executar chamada externa
    ↓
Persistir resultado
Commit
```

---

# 47. Concorrência Otimista

Aggregates sujeitos a concorrência deverão possuir um token.

Possibilidades:

```text
xmin do PostgreSQL
version bigint
concurrency_token uuid
```

Uma coluna explícita `version` é mais clara para o negócio e para APIs.

Exemplo:

```csharp
builder.Property<long>("version")
    .IsConcurrencyToken();
```

Ou propriedade no Aggregate:

```csharp
public long Version { get; private set; }
```

---

# 48. PostgreSQL xmin

O PostgreSQL fornece a coluna de sistema:

```text
xmin
```

Ela pode ser utilizada pelo Npgsql como token de concorrência.

Vantagens:

- Sem coluna adicional.
    
- Atualizada automaticamente.
    

Desvantagens:

- Conceito técnico.
    
- Menos explícito.
    
- Não deve ser exposto diretamente como versão de negócio.
    
- Pode dificultar portabilidade.
    

Para recursos com versão pública, uma coluna explícita será preferida.

---

# 49. Tratamento de DbUpdateConcurrencyException

O Data deverá traduzir a exceção para uma abstração compreensível.

Exemplo:

```csharp
try
{
    return await base.SaveChangesAsync(
        cancellationToken);
}
catch (DbUpdateConcurrencyException exception)
{
    throw new DataConcurrencyException(
        "A record was modified concurrently.",
        exception);
}
```

Uma estratégia alternativa será retornar `Result`.

A exceção técnica não deverá chegar diretamente aos endpoints.

---

# 50. Locks Pessimistas

Locks como:

```sql
select ... for update
```

poderão ser utilizados em casos específicos.

Exemplos:

- Claim de mensagem da Outbox.
    
- Claim de execução.
    
- Atualização de contador altamente concorrente.
    
- Eleição de trabalho.
    

Devem ser usados com:

- Transações curtas.
    
- Ordem consistente.
    
- Timeout.
    
- Testes de concorrência.
    
- Métricas.
    

---

# 51. Claim de Trabalho

Para impedir dois Workers de processar o mesmo item:

```sql
select *
from outbox_messages
where processed_at is null
order by occurred_at
for update skip locked
limit @batch_size;
```

O `SKIP LOCKED` permite processamento concorrente sem espera excessiva.

O mesmo padrão poderá ser usado em:

- Outbox.
    
- Jobs.
    
- Reconciliação.
    
- Execuções pendentes.
    

---

# 52. Outbox Pattern

A Outbox garantirá consistência entre:

- Alterações no PostgreSQL.
    
- Mensagens que precisam ser publicadas.
    

Fluxo:

```text
Handler
    ├── Altera Aggregate
    ├── Gera evento
    ├── Salva Aggregate
    ├── Salva OutboxMessage
    └── Commit
```

Depois:

```text
Outbox Worker
    ↓
Lê mensagens pendentes
    ↓
Publica no RabbitMQ
    ↓
Marca como processada
```

O projeto Data persiste e consulta a Outbox.

A publicação física será responsabilidade da Infrastructure e do Worker.

---

# 53. OutboxMessage

Estrutura sugerida:

```csharp
public sealed class OutboxMessage
{
    public Guid Id { get; init; }

    public required string Type { get; init; }

    public required string Version { get; init; }

    public required string Payload { get; init; }

    public Guid OrganizationId { get; init; }

    public string? CorrelationId { get; init; }

    public string? CausationId { get; init; }

    public DateTimeOffset OccurredAt { get; init; }

    public DateTimeOffset? ProcessedAt { get; set; }

    public DateTimeOffset? LockedUntil { get; set; }

    public string? LockId { get; set; }

    public int AttemptCount { get; set; }

    public DateTimeOffset? NextAttemptAt { get; set; }

    public string? LastError { get; set; }
}
```

---

# 54. Tabela da Outbox

Campos:

```text
id
organization_id
message_type
message_version
payload
correlation_id
causation_id
occurred_at
processed_at
locked_until
lock_id
attempt_count
next_attempt_at
last_error
```

Índices:

```text
processed_at
next_attempt_at
occurred_at
lock_id
organization_id
```

Índice parcial recomendado:

```sql
create index ix_outbox_pending
on outbox_messages (
    next_attempt_at,
    occurred_at
)
where processed_at is null;
```

---

# 55. Captura de Domain Events

Um interceptor poderá converter Domain Events em Outbox Messages.

Fluxo:

```text
Aggregate.DomainEvents
    ↓
SaveChangesInterceptor
    ↓
Serialização
    ↓
OutboxMessage
```

Exemplo conceitual:

```csharp
public sealed class DomainEventOutboxInterceptor
    : SaveChangesInterceptor
{
}
```

O interceptor deverá:

1. Encontrar Aggregates rastreados.
    
2. Coletar Domain Events.
    
3. Transformar eventos publicáveis.
    
4. Adicionar registros à Outbox.
    
5. Limpar Domain Events após persistência segura.
    

---

# 56. Domain Event x Integration Event

Nem todo Domain Event deve ser publicado.

Um mapper explícito deverá decidir:

```text
PipelineExecutionQueuedDomainEvent
    ↓
PipelineExecutionRequestedV1
```

A transformação poderá ocorrer:

- Na Application.
    
- Em Handler de Domain Event.
    
- Em mapper da Outbox.
    

O Data não deverá inventar regras de integração.

Ele deverá persistir mensagens já definidas ou utilizar um mapper configurado.

---

# 57. Serialização da Outbox

O payload poderá utilizar JSONB.

Campos de tipo e versão deverão permanecer separados.

Exemplo:

```json
{
  "executionId": "...",
  "pipelineId": "...",
  "organizationId": "...",
  "requestedAt": "..."
}
```

Nunca depender apenas do nome CLR completo:

```text
InfiniteContentAI.Contracts.Events...
```

O tipo lógico deverá ser estável:

```text
pipeline.execution.requested
```

Versão:

```text
v1
```

---

# 58. Falhas na Outbox

Falhas de publicação deverão registrar:

- AttemptCount.
    
- LastError.
    
- NextAttemptAt.
    
- LockedUntil.
    
- LockId.
    

Após exceder o limite:

- Permanecer para investigação.
    
- Ser movida logicamente para estado Dead.
    
- Gerar alerta.
    
- Permitir replay administrativo.
    

O Data não publicará a mensagem.

Ele apenas controlará o estado persistido.

---

# 59. Inbox Pattern

A Inbox garantirá consumo idempotente de mensagens.

Fluxo:

```text
Mensagem recebida
    ↓
Verificar Inbox por MessageId
    ├── Existe e concluída → ignorar
    └── Não existe → registrar processamento
```

Depois:

```text
Executar caso de uso
    ↓
Salvar alterações
    ↓
Marcar Inbox como concluída
    ↓
Commit
```

A Inbox deverá participar da mesma transação das alterações de negócio quando possível.

---

# 60. InboxMessage

```csharp
public sealed class InboxMessage
{
    public Guid MessageId { get; init; }

    public required string MessageType { get; init; }

    public Guid OrganizationId { get; init; }

    public DateTimeOffset ReceivedAt { get; init; }

    public DateTimeOffset? ProcessedAt { get; set; }

    public DateTimeOffset? FailedAt { get; set; }

    public int AttemptCount { get; set; }

    public string? LastError { get; set; }

    public string? PayloadHash { get; init; }
}
```

A chave principal ou unique constraint deverá incluir o MessageId.

---

# 61. Constraint da Inbox

```sql
alter table inbox_messages
add constraint pk_inbox_messages
primary key (message_id);
```

Para ambientes multi-broker, poderá ser necessário:

```text
consumer_name + message_id
```

Exemplo:

```sql
unique (
    consumer_name,
    message_id
);
```

Isso permite que diferentes consumidores processem o mesmo evento independentemente.

---

# 62. Estados da Inbox

Possíveis estados:

```text
Processing
Processed
Failed
```

Ou datas:

```text
received_at
processed_at
failed_at
```

A modelagem deverá permitir detectar:

- Mensagem duplicada.
    
- Processamento incompleto.
    
- Worker interrompido.
    
- Falha permanente.
    
- Necessidade de retry.
    

---

# 63. Transação da Inbox

Fluxo recomendado:

```text
Abrir transação
    ↓
Inserir InboxMessage
    ↓
Executar Command
    ↓
Salvar alterações
    ↓
Marcar InboxMessage como Processed
    ↓
Commit
```

Se a inserção violar a unique constraint, a mensagem já foi registrada.

O Consumer poderá confirmar a mensagem sem repetir efeitos.

---

# 64. Idempotency Store

A idempotência de Commands HTTP poderá utilizar uma tabela própria.

Campos:

```text
organization_id
command_type
idempotency_key
request_hash
status
response
response_type
created_at
completed_at
expires_at
```

Unique constraint:

```text
organization_id
command_type
idempotency_key
```

---

# 65. IdempotencyRecord

```csharp
public sealed class IdempotencyRecord
{
    public Guid Id { get; init; }

    public Guid OrganizationId { get; init; }

    public required string CommandType { get; init; }

    public required string Key { get; init; }

    public required string RequestHash { get; init; }

    public required string Status { get; set; }

    public string? ResponsePayload { get; set; }

    public string? ResponseType { get; set; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? CompletedAt { get; set; }

    public DateTimeOffset ExpiresAt { get; init; }
}
```

---

# 66. Hash do Request

A mesma Idempotency Key não poderá ser reutilizada com payload diferente.

O payload relevante deverá ser:

1. Canonicalizado.
    
2. Serializado de forma estável.
    
3. Transformado em hash.
    
4. Comparado com o registro anterior.
    

O hash deverá excluir dados instáveis, como:

- CorrelationId.
    
- TraceId.
    
- Timestamps gerados pelo servidor.
    

---

# 67. Isolamento Multi-tenant

Todos os registros tenant-scoped deverão possuir:

```text
organization_id
```

Exemplos:

- Projects.
    
- Pipelines.
    
- Executions.
    
- Artifacts.
    
- Approvals.
    
- Publications.
    
- Outbox.
    
- Inbox.
    
- Idempotency Records.
    

Queries deverão filtrar por Organization.

---

# 68. Global Query Filters

Filtros globais do EF Core poderão ser utilizados:

```csharp
builder.HasQueryFilter(
    project =>
        project.OrganizationId ==
            _tenantContext.OrganizationId);
```

Entretanto, existem riscos:

- Filtro invisível.
    
- Problemas em jobs administrativos.
    
- Dificuldade em migrations.
    
- Vazamento quando o filtro é ignorado.
    
- Complexidade em DbContext pooling.
    

Para o MVP, queries explícitas por Organization são mais claras.

Filtros globais poderão ser adicionados como segunda barreira, nunca como única proteção.

---

# 69. Queries Tenant-scoped

Exemplo correto:

```csharp
_dbContext.Projects
    .Where(project =>
        project.OrganizationId ==
            organizationId);
```

Exemplo proibido:

```csharp
_dbContext.Projects
    .SingleAsync(
        project => project.Id == projectId);
```

Todo repository tenant-scoped deverá exigir `OrganizationId`.

---

# 70. Constraints Multi-tenant

Foreign keys poderão incluir OrganizationId quando isso trouxer proteção adicional.

Exemplo:

```text
pipelines (
    id,
    organization_id,
    project_id
)
```

Uma foreign key composta poderia garantir que Pipeline e Project pertencem à mesma Organization.

Essa estratégia aumenta segurança, mas também aumenta complexidade.

Ela deverá ser aplicada em relacionamentos críticos quando o benefício justificar.

---

# 71. Exclusão

A estratégia dependerá do tipo de registro.

## Negócio

Preferir:

```text
Archived
Cancelled
Disabled
Deprecated
```

## Técnico

Poderá utilizar exclusão física:

```text
Outbox processada expirada
Inbox antiga
Idempotency Records expirados
Locks
Dados temporários
```

O Domain decide o significado de arquivamento ou cancelamento.

O Data executa a persistência.

---

# 72. Soft Delete

Soft delete genérico não será aplicado automaticamente a todas as tabelas.

Problemas comuns:

- Queries esquecem filtros.
    
- Unique constraints ficam complexas.
    
- Dados crescem indefinidamente.
    
- Relacionamentos ficam ambíguos.
    
- Exclusão deixa de ter significado.
    

Estados explícitos do Domain serão preferidos.

---

# 73. Índices

Índices deverão ser definidos a partir das consultas reais.

Índices iniciais:

## Projects

```text
organization_id, created_at desc
organization_id, name
```

## Pipelines

```text
organization_id, project_id
organization_id, status
```

## Executions

```text
organization_id, requested_at desc
organization_id, project_id, requested_at desc
organization_id, status, requested_at
pipeline_id, pipeline_version
```

## Artifacts

```text
organization_id, execution_id
organization_id, project_id, created_at desc
execution_id, step_execution_id
```

## Outbox

```text
processed_at, next_attempt_at
```

## Inbox

```text
processed_at
received_at
```

---

# 74. Índices Compostos

A ordem das colunas deverá refletir filtros e ordenação.

Exemplo:

```sql
create index ix_executions_org_status_requested
on pipeline_executions (
    organization_id,
    status,
    requested_at desc
);
```

Um índice não deverá ser adicionado somente porque uma coluna parece importante.

Índices possuem custo de:

- Escrita.
    
- Armazenamento.
    
- Vacuum.
    
- Migrations.
    
- Manutenção.
    

---

# 75. Índices Parciais

PostgreSQL permite índices parciais.

Exemplo:

```sql
create index ix_executions_active
on pipeline_executions (
    organization_id,
    requested_at
)
where status in (
    'Queued',
    'Running',
    'AwaitingApproval'
);
```

Útil para:

- Outbox pendente.
    
- Inbox incompleta.
    
- Execuções ativas.
    
- Publications pendentes.
    
- Registros não arquivados.
    

---

# 76. Full-text Search

Busca textual poderá utilizar recursos do PostgreSQL.

Possibilidades:

- `ILIKE`.
    
- `pg_trgm`.
    
- Full-text search.
    
- Vetores de busca.
    
- Índices GIN.
    

Para o MVP, buscas simples poderão utilizar:

```sql
where name ilike '%' || @search || '%'
```

Com crescimento, `pg_trgm` deverá ser considerado.

Busca semântica e vetorial será tratada separadamente na arquitetura de IA.

---

# 77. Constraints

Constraints obrigatórias:

- Primary keys.
    
- Foreign keys.
    
- Not null.
    
- Unique constraints.
    
- Check constraints.
    
- Length limits quando aplicável.
    

Exemplo:

```sql
check (version > 0)
check (attempt_count >= 0)
check (size >= 0)
```

O banco é a última linha de defesa contra dados inválidos.

---

# 78. Unique Constraints

Exemplos possíveis:

```text
Organization + ProjectName normalizado
Pipeline + Version
Pipeline + StepPosition
Execution + StepDefinition
Artifact + Version
ConsumerName + MessageId
Organization + CommandType + IdempotencyKey
```

Uma unique constraint também pode ser utilizada como mecanismo de idempotência.

---

# 79. Normalização

Valores usados em unicidade poderão possuir uma coluna normalizada.

Exemplo:

```text
name
normalized_name
```

A normalização poderá incluir:

- Trim.
    
- Lowercase.
    
- Unicode normalization.
    
- Remoção de espaços duplicados.
    

A regra deverá ser consistente entre Domain e Data.

---

# 80. Migrations

Migrations serão mantidas no projeto Data.

Comandos conceituais:

```bash
dotnet ef migrations add InitialCreate \
  --project src/Data \
  --startup-project src/Api
```

```bash
dotnet ef database update \
  --project src/Data \
  --startup-project src/Api
```

Migrations deverão ser versionadas no repositório.

---

# 81. Design-time DbContext Factory

```csharp
public sealed class ApplicationDbContextFactory
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(
        string[] args)
    {
        var options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(
                    "Host=localhost;Database=infinite_content_ai;...")
                .UseSnakeCaseNamingConvention()
                .Options;

        return new ApplicationDbContext(options);
    }
}
```

A connection string de desenvolvimento não deverá conter credenciais reais versionadas.

Preferir:

- Environment variables.
    
- User Secrets.
    
- Arquivo local ignorado pelo Git.
    

---

# 82. Regras para Migrations

Cada migration deverá:

- Possuir nome claro.
    
- Ser revisada.
    
- Ser testada.
    
- Evitar perda de dados.
    
- Considerar rollback operacional.
    
- Considerar locks.
    
- Considerar duração.
    
- Considerar volume.
    
- Ser compatível com deploy gradual.
    

Evitar migrations geradas e aceitas sem revisão.

---

# 83. Expand and Contract

Mudanças incompatíveis deverão utilizar estratégia de expansão e contração.

Exemplo: renomear coluna.

## Fase 1

- Adicionar nova coluna.
    
- Aplicação escreve nas duas.
    
- Backfill.
    

## Fase 2

- Aplicação lê nova coluna.
    
- Validar dados.
    

## Fase 3

- Parar escrita antiga.
    
- Remover coluna antiga.
    

Isso reduz indisponibilidade em deploys.

---

# 84. Migrations Destrutivas

Operações destrutivas exigem cuidado:

```text
DropColumn
DropTable
AlterColumn com redução de tamanho
Mudança de tipo
Adicionar Not Null sem default
```

A migration deverá incluir estratégia para dados existentes.

---

# 85. Backfills

Backfills grandes não deverão necessariamente ocorrer dentro da migration transacional.

Preferir:

- Job separado.
    
- Script controlado.
    
- Processamento em lotes.
    
- Métricas.
    
- Checkpoint.
    
- Possibilidade de retomada.
    

Migrations devem permanecer previsíveis e rápidas.

---

# 86. Aplicação de Migrations

Em produção, migrations não deverão ser executadas automaticamente por cada instância da API.

Riscos:

- Corrida entre instâncias.
    
- Locks inesperados.
    
- Falha durante startup.
    
- Indisponibilidade.
    
- Permissões excessivas.
    

Preferir um passo dedicado no pipeline de deploy:

```text
Build
    ↓
Test
    ↓
Run Migrations
    ↓
Deploy API
    ↓
Deploy Workers
```

---

# 87. Seeds

Seeds deverão ser usados com moderação.

Candidatos:

- Tipos técnicos estáveis.
    
- Configuração mínima de desenvolvimento.
    
- Dados de teste.
    
- Pipeline de exemplo em ambiente local.
    

Não utilizar seed automático para:

- Dados reais de clientes.
    
- Usuários de produção.
    
- Secrets.
    
- Configurações que mudam por ambiente.
    

---

# 88. Auditoria Técnica

Interceptors poderão preencher dados técnicos como:

```text
created_at
updated_at
```

Somente quando essas propriedades fizerem parte do modelo.

Não adicionar propriedades de auditoria genéricas a todas as entidades sem necessidade.

Auditoria de negócio deverá permanecer explícita no Domain.

---

# 89. SaveChangesInterceptor

Possíveis interceptors:

```text
DomainEventOutboxInterceptor
AuditableEntityInterceptor
ConcurrencyVersionInterceptor
SlowQueryInterceptor
```

Cada interceptor deverá possuir responsabilidade única.

Evitar um interceptor gigante com lógica de negócio.

---

# 90. Query Interceptors

Interceptores poderão medir:

- Duração.
    
- SQL lento.
    
- Quantidade de linhas.
    
- Falhas.
    
- Timeouts.
    

Eles não deverão registrar:

- Parâmetros sensíveis.
    
- Prompts.
    
- Tokens.
    
- Dados pessoais.
    
- Connection strings.
    

---

# 91. Command Timeout

Timeout padrão deverá ser configurado.

Exemplo:

```text
30 segundos
```

Queries ou migrations específicas poderão possuir timeout diferente.

Aumentar timeout não deverá ser a primeira resposta para uma query lenta.

Primeiro investigar:

- Plano de execução.
    
- Índices.
    
- Volume.
    
- Locks.
    
- N+1.
    
- Projeção.
    
- Tracking.
    
- Paginação.
    

---

# 92. N+1 Queries

O código deverá evitar N+1.

Exemplo inadequado:

```csharp
foreach (var execution in executions)
{
    var artifacts =
        await GetArtifactsAsync(execution.Id);
}
```

Preferir:

- Projeção.
    
- Join.
    
- Query em lote.
    
- `Include` controlado.
    
- Segunda consulta com todos os IDs.
    

Lazy loading não será habilitado por padrão.

---

# 93. Includes

`Include` deverá ser utilizado cuidadosamente.

Evitar carregar grafos grandes:

```csharp
.Include(project => project.Pipelines)
.ThenInclude(pipeline => pipeline.Executions)
.ThenInclude(execution => execution.Artifacts)
```

Preferir projeções específicas.

Para Aggregates de escrita, carregar apenas o limite necessário.

---

# 94. Split Queries

`AsSplitQuery()` poderá ser utilizado quando múltiplos Includes causarem produto cartesiano.

Entretanto, ele aumenta o número de round trips.

A decisão deverá ser baseada em:

- Volume.
    
- Forma do Aggregate.
    
- Perfil da consulta.
    
- Métricas.
    

---

# 95. Bulk Operations

Operações em lote poderão utilizar:

```text
ExecuteUpdateAsync
ExecuteDeleteAsync
COPY
Biblioteca especializada
SQL direto
```

Casos:

- Limpeza da Outbox.
    
- Expiração da Inbox.
    
- Atualização técnica.
    
- Backfills.
    
- Arquivamento em lote.
    

Bulk operations não executam automaticamente comportamentos do Domain.

Não deverão ser usadas para alterar regras de negócio sem cuidado.

---

# 96. Retenção

Políticas iniciais:

## Outbox processada

Reter por período suficiente para auditoria e investigação.

Exemplo:

```text
7 a 30 dias
```

## Inbox processada

Reter conforme janela máxima de redelivery.

Exemplo:

```text
7 a 30 dias
```

## Idempotency Records

Reter conforme contrato do endpoint.

Exemplo:

```text
24 horas a 30 dias
```

## Execuções e Artifacts

Retenção definida por regra de negócio e plano.

---

# 97. Limpeza

A limpeza será executada por Worker ou job dedicado.

Casos de uso:

```text
DeleteProcessedOutboxMessages
DeleteExpiredInboxMessages
DeleteExpiredIdempotencyRecords
DeleteTemporaryArtifacts
```

O Data implementará queries e comandos de exclusão.

O Worker controlará o agendamento.

---

# 98. Particionamento

Particionamento poderá ser considerado futuramente para tabelas grandes:

- Outbox.
    
- Inbox.
    
- Executions.
    
- Artifacts.
    
- Audit logs.
    
- Token usage.
    

Possíveis chaves:

- Data.
    
- Organization.
    
- Status.
    

Particionamento não será implementado no MVP sem necessidade medida.

---

# 99. Connection Pool

O Npgsql utilizará pool de conexões.

Configurações relevantes:

```text
Minimum Pool Size
Maximum Pool Size
Connection Idle Lifetime
Connection Pruning Interval
Timeout
Command Timeout
```

O tamanho do pool deverá considerar:

- Número de réplicas.
    
- API.
    
- Workers.
    
- Limite do PostgreSQL.
    
- Queries concorrentes.
    
- Duração média.
    

---

# 100. DbContext Pooling

`AddDbContextPool` poderá reduzir custo de criação de contextos.

Entretanto, exige cuidado com estado por request:

- Tenant Context.
    
- Interceptors com estado.
    
- Configuração dinâmica.
    
- Campos mutáveis.
    

Não será ativado inicialmente sem testes específicos.

---

# 101. Resiliência de Conexão

Falhas transitórias de conexão poderão ser repetidas pelo provider.

Operações repetidas devem ser idempotentes.

Uma transação manual combinada com retry exige estratégia de execução própria.

O comportamento deverá ser testado com falhas reais ou simuladas.

---

# 102. Deadlocks

Deadlocks poderão ocorrer em:

- Atualizações concorrentes.
    
- Outbox.
    
- Inbox.
    
- Claim de etapas.
    
- Alteração de múltiplos Aggregates.
    

Mitigações:

- Ordem consistente de acesso.
    
- Transações curtas.
    
- Índices adequados.
    
- Menor quantidade de registros bloqueados.
    
- Retry controlado.
    
- Observabilidade.
    

---

# 103. Health Check do Banco

A API e o Worker poderão registrar um health check de PostgreSQL.

Readiness deverá validar:

- Conexão.
    
- Query simples.
    
- Timeout curto.
    

Não deverá executar queries pesadas.

O Data poderá fornecer a configuração, mas a exposição do endpoint ficará no host.

---

# 104. Observabilidade

O Data deverá produzir telemetria sobre:

- Duração das queries.
    
- Quantidade de comandos.
    
- Falhas.
    
- Timeouts.
    
- Conflitos de concorrência.
    
- Pool de conexões.
    
- Transações.
    
- Outbox pendente.
    
- Inbox falhada.
    
- Locks prolongados.
    

A instrumentação principal poderá utilizar OpenTelemetry no host.

O Data não deverá depender de um fornecedor específico de observabilidade.

---

# 105. Logs SQL

Logs SQL detalhados deverão ser limitados a desenvolvimento e diagnóstico controlado.

Nunca habilitar dados sensíveis em produção sem investigação específica.

Evitar registrar:

- Prompts.
    
- Conteúdo de Artifacts.
    
- Tokens.
    
- E-mails.
    
- Secrets.
    
- Dados pessoais.
    
- Payloads completos de Outbox.
    

---

# 106. Métricas

Métricas úteis:

```text
database.command.duration
database.command.failures
database.concurrency.conflicts
database.transaction.duration
outbox.pending.count
outbox.oldest.age
outbox.publish.failures
inbox.failed.count
idempotency.replays
```

Labels deverão possuir baixa cardinalidade.

Não utilizar IDs de negócio como labels.

---

# 107. Backup e Recuperação

O PostgreSQL deverá possuir:

- Backups automáticos.
    
- Point-in-time recovery.
    
- Política de retenção.
    
- Testes de restauração.
    
- Criptografia.
    
- Monitoramento.
    
- Controle de acesso.
    

A recuperação não será considerada pronta apenas porque backups existem.

Restaurações deverão ser testadas periodicamente.

---

# 108. Segurança do Banco

Regras:

- Connection strings fora do código.
    
- TLS habilitado.
    
- Usuários com menor privilégio.
    
- Usuário de migration separado quando possível.
    
- Rede restrita.
    
- Rotação de credenciais.
    
- Logs sem secrets.
    
- Acesso administrativo auditado.
    
- Backups criptografados.
    
- Produção separada de desenvolvimento.
    

---

# 109. Permissões

Possíveis identidades:

## Runtime

Permissões necessárias para:

- Select.
    
- Insert.
    
- Update.
    
- Delete controlado.
    
- Uso de sequences, se existirem.
    

## Migration

Permissões adicionais:

- Create.
    
- Alter.
    
- Drop.
    
- Index.
    
- Constraints.
    

A aplicação em runtime não deverá possuir permissões de alteração de schema quando isso puder ser evitado.

---

# 110. Testes Unitários

Código de Data com lógica pequena poderá possuir testes unitários.

Exemplos:

- Conversores.
    
- Mapeamento de erros.
    
- Construção de filtros.
    
- Cursor encoding.
    
- Canonicalização de idempotência.
    

Entretanto, a maioria dos comportamentos de persistência deverá ser testada com PostgreSQL real.

---

# 111. Testes de Integração

Os testes de Data deverão utilizar PostgreSQL real, preferencialmente em container.

Testar:

- Mapeamentos.
    
- Constraints.
    
- Repositories.
    
- Queries.
    
- Transações.
    
- Concorrência.
    
- Migrations.
    
- Outbox.
    
- Inbox.
    
- JSONB.
    
- Índices importantes.
    
- Multi-tenancy.
    
- Value Objects.
    

Evitar usar banco InMemory do EF Core para validar comportamento relacional.

---

# 112. Testcontainers

Estrutura possível:

```csharp
public sealed class PostgreSqlFixture
    : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container =
        new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .Build();
}
```

A versão da imagem deverá ser fixada no projeto real.

Evitar utilizar `latest` no CI.

---

# 113. Isolamento de Testes

Estratégias:

- Banco por suíte.
    
- Schema por teste.
    
- Transação com rollback.
    
- Respawn.
    
- Truncate entre testes.
    
- Database template.
    

A escolha deverá equilibrar:

- Velocidade.
    
- Isolamento.
    
- Paralelismo.
    
- Fidelidade.
    

---

# 114. Teste de Repository

```csharp
[Fact]
public async Task GetByIdAsync_ShouldNotReturnProject_FromAnotherOrganization()
{
    var project = ProjectFactory.Create(
        organizationId: OrganizationA.Id);

    await _dbContext.Projects.AddAsync(project);
    await _dbContext.SaveChangesAsync();

    var repository =
        new ProjectRepository(_dbContext);

    var result = await repository.GetByIdAsync(
        OrganizationB.Id,
        project.Id,
        CancellationToken.None);

    result.Should().BeNull();
}
```

Testes de isolamento são obrigatórios.

---

# 115. Teste de Constraint

```csharp
[Fact]
public async Task SaveChanges_ShouldFail_WhenPipelineStepPositionIsDuplicated()
{
    var pipeline =
        PipelineFactory.CreateWithDuplicatedPositions();

    await _dbContext.Pipelines.AddAsync(pipeline);

    var action = async () =>
        await _dbContext.SaveChangesAsync();

    await action.Should()
        .ThrowAsync<DbUpdateException>();
}
```

O teste confirma que o banco protege a regra mesmo diante de dados inválidos.

---

# 116. Teste de Concorrência

```csharp
[Fact]
public async Task SaveChanges_ShouldFail_WhenExecutionWasChangedConcurrently()
{
    await using var contextA =
        CreateDbContext();

    await using var contextB =
        CreateDbContext();

    var executionA =
        await contextA.PipelineExecutions.SingleAsync();

    var executionB =
        await contextB.PipelineExecutions.SingleAsync();

    executionA.Cancel(...);
    await contextA.SaveChangesAsync();

    executionB.Start(...);

    var action = async () =>
        await contextB.SaveChangesAsync();

    await action.Should()
        .ThrowAsync<DbUpdateConcurrencyException>();
}
```

---

# 117. Teste da Outbox

Validar:

```text
Alteração do Aggregate
+
OutboxMessage
+
Mesmo commit
```

Cenário:

1. Criar PipelineExecution.
    
2. Gerar Domain Event.
    
3. Executar SaveChanges.
    
4. Confirmar registro da execução.
    
5. Confirmar registro da Outbox.
    
6. Confirmar mesmo transaction boundary.
    

---

# 118. Teste da Inbox

Validar:

1. Registrar MessageId.
    
2. Executar alteração.
    
3. Confirmar processamento.
    
4. Repetir MessageId.
    
5. Confirmar que o efeito não se repete.
    

Também testar concorrência entre dois Consumers.

---

# 119. Teste de Migrations

O CI deverá:

1. Criar banco vazio.
    
2. Aplicar todas as migrations.
    
3. Executar smoke tests.
    
4. Opcionalmente testar upgrade de versão anterior.
    
5. Validar ausência de alterações pendentes no modelo.
    

Uma migration que compila não está necessariamente correta.

---

# 120. Verificação de Migration Pendente

O pipeline poderá verificar se o modelo gera migration adicional.

Isso evita commits onde:

- Entidade mudou.
    
- Mapping mudou.
    
- Migration foi esquecida.
    

A verificação deverá ser compatível com a versão do EF Core utilizada.

---

# 121. Testes de Performance

Queries críticas deverão possuir testes ou benchmarks.

Exemplos:

- Listagem de Executions.
    
- Claim da Outbox.
    
- Consulta de execução com steps.
    
- Listagem de Artifacts.
    
- Reconciliação de execuções travadas.
    

Os testes deverão utilizar volume representativo.

---

# 122. Antipadrões

## DbContext na API

Endpoints não acessam banco diretamente.

## DbContext no Worker

Consumers delegam para Application.

## Repository genérico universal

Esconde intenção e limites.

## Entidade como modelo de leitura

Queries devem projetar DTOs.

## Include em toda consulta

Carrega dados desnecessários.

## Lazy Loading

Gera queries invisíveis e N+1.

## SaveChanges espalhado

O limite transacional deve ser explícito.

## Chamada externa dentro de transação

Aumenta locks e risco.

## Query sem OrganizationId

Cria risco de vazamento entre tenants.

## JSONB para tudo

Reduz integridade e consultabilidade.

## Soft delete genérico

Esconde regras de ciclo de vida.

## Migration automática por instância

Cria corrida e indisponibilidade.

## Sensitive Data Logging em produção

Expõe dados e secrets.

## Índice em toda coluna

Aumenta custo de escrita sem benefício real.

## Capturar DbUpdateException genericamente

Erros importantes devem ser classificados.

---

# 123. Regras Arquiteturais

1. Data depende apenas de Application, Domain e SharedKernel.
    
2. Infrastructure não é referenciada.
    
3. DbContext existe somente em Data.
    
4. Migrations existem somente em Data.
    
5. Repositories concretos existem em Data.
    
6. Interfaces de repositories existem na Application.
    
7. Repositories são orientados a Aggregates.
    
8. Queries especializadas utilizam projeções.
    
9. Queries de leitura usam `AsNoTracking`.
    
10. Toda consulta tenant-scoped filtra por OrganizationId.
    
11. Application não recebe IQueryable.
    
12. Data não chama APIs externas.
    
13. Data não publica RabbitMQ.
    
14. Outbox é persistida no mesmo commit das alterações.
    
15. Inbox participa da transação do consumo.
    
16. Operações críticas utilizam idempotência.
    
17. Transações são curtas.
    
18. Chamadas externas não ocorrem dentro de transações.
    
19. Concorrência otimista é o padrão.
    
20. Value Objects possuem mapeamentos explícitos.
    
21. Entidades não são expostas como read models.
    
22. Migrations são revisadas.
    
23. Migrations destrutivas exigem plano.
    
24. Índices são baseados em queries reais.
    
25. Testes utilizam PostgreSQL real.
    
26. Secrets não ficam em configurações versionadas.
    
27. Logs SQL não expõem dados sensíveis.
    
28. Soft delete não é aplicado genericamente.
    
29. Lazy loading permanece desabilitado.
    
30. CancellationToken é propagado.
    

---

# 124. Escopo do MVP

O Data inicial deverá implementar:

## Contexto

```text
ApplicationDbContext
ApplicationDbContextFactory
DataOptions
DependencyInjection
```

## Projects

```text
ProjectConfiguration
ProjectRepository
ProjectQueries
```

## Pipelines

```text
PipelineConfiguration
PipelineStepDefinitionConfiguration
PipelineRepository
PipelineQueries
```

## Executions

```text
PipelineExecutionConfiguration
StepExecutionConfiguration
PipelineExecutionRepository
ExecutionQueries
```

## Artifacts

```text
ArtifactConfiguration
ArtifactRepository
ArtifactQueries
```

## Mensageria Persistente

```text
OutboxMessage
OutboxMessageConfiguration
EfOutbox
InboxMessage
InboxMessageConfiguration
EfInbox
```

## Fundação

```text
UnitOfWork
TransactionManager
Concurrency handling
Initial migrations
Integration test infrastructure
```

---

# 125. Ordem de Implementação

## Etapa 1 — Fundação

- Adicionar EF Core.
    
- Adicionar provider Npgsql.
    
- Criar DataOptions.
    
- Criar ApplicationDbContext.
    
- Criar Design-time Factory.
    
- Configurar naming convention.
    
- Configurar Dependency Injection.
    
- Criar migration inicial vazia ou base.
    

## Etapa 2 — Projects

- ProjectConfiguration.
    
- ProjectRepository.
    
- ProjectQueries.
    
- Migration.
    
- Testes.
    

## Etapa 3 — Pipelines

- PipelineConfiguration.
    
- StepConfiguration.
    
- Constraints de posição.
    
- Repository.
    
- Queries.
    
- Migration.
    
- Testes.
    

## Etapa 4 — Executions

- ExecutionConfiguration.
    
- StepExecutionConfiguration.
    
- Concorrência.
    
- Repository.
    
- Queries.
    
- Migration.
    
- Testes.
    

## Etapa 5 — Artifacts

- ArtifactConfiguration.
    
- Content mapping.
    
- JSONB.
    
- Storage metadata.
    
- Repository.
    
- Queries.
    
- Testes.
    

## Etapa 6 — Outbox

- OutboxMessage.
    
- Mapping.
    
- Captura de eventos.
    
- Queries de claim.
    
- Retry fields.
    
- Testes.
    

## Etapa 7 — Inbox e Idempotência

- InboxMessage.
    
- Unique constraints.
    
- Idempotency Records.
    
- Stores.
    
- Testes concorrentes.
    

---

# 126. Checklist de Nova Entidade Persistida

- Qual tabela será utilizada?
    
- Qual é a primary key?
    
- O ID é gerado pela aplicação?
    
- Quais propriedades são obrigatórias?
    
- Quais tamanhos máximos existem?
    
- Quais Value Objects precisam de conversão?
    
- Quais foreign keys existem?
    
- Qual é o comportamento de delete?
    
- Quais constraints são necessárias?
    
- Quais índices são necessários?
    
- É tenant-scoped?
    
- Possui token de concorrência?
    
- Possui dados sensíveis?
    
- Possui campos JSONB?
    
- Precisa de migration?
    
- Possui testes de mapping?
    

---

# 127. Checklist de Novo Repository

- Representa um Aggregate?
    
- A interface está na Application?
    
- Exige OrganizationId?
    
- Carrega somente o necessário?
    
- Utiliza tracking apenas para escrita?
    
- Propaga CancellationToken?
    
- O nome do método expressa intenção?
    
- Evita IQueryable externo?
    
- Evita expressão genérica?
    
- Possui testes com PostgreSQL?
    
- Trata concorrência?
    
- Respeita o limite do Aggregate?
    

---

# 128. Checklist de Nova Query

- Retorna read model?
    
- Usa AsNoTracking?
    
- Filtra por OrganizationId?
    
- Projeta no banco?
    
- Possui ordenação determinística?
    
- Possui paginação?
    
- Evita N+1?
    
- Carrega apenas colunas necessárias?
    
- Possui índice correspondente?
    
- Propaga CancellationToken?
    
- Possui testes?
    
- Foi avaliado o plano de execução?
    

---

# 129. Checklist de Migration

- O nome descreve a mudança?
    
- O SQL gerado foi revisado?
    
- A migration preserva dados?
    
- Há operação destrutiva?
    
- Existe backfill?
    
- Existe risco de lock prolongado?
    
- É compatível com deploy gradual?
    
- Possui índices necessários?
    
- O rollback operacional foi considerado?
    
- Foi testada em banco limpo?
    
- Foi testada sobre versão anterior?
    
- O tempo de execução foi avaliado?
    

---

# 130. Critérios de Qualidade

O projeto Data será considerado saudável quando:

- O Domain puder ser persistido sem depender de EF Core.
    
- Queries forem eficientes e previsíveis.
    
- Repositories respeitarem os Aggregates.
    
- Dados não vazarem entre Organizations.
    
- Migrations forem seguras.
    
- Concorrência gerar conflitos controlados.
    
- Outbox impedir perda de mensagens após commit.
    
- Inbox impedir efeitos duplicados.
    
- Operações HTTP críticas forem idempotentes.
    
- Testes utilizarem PostgreSQL real.
    
- Queries lentas forem observáveis.
    
- Data não possuir integrações externas.
    
- O DbContext não escapar para outras camadas.
    
- A evolução do schema puder ocorrer sem bloquear o produto.
    

---

# 131. Documentos Relacionados

```text
03 - Arquitetura/Mensageria e Comunicação entre Componentes.md
03 - Arquitetura/Tratamento de Erros e Resiliência.md
03 - Arquitetura/Estratégia de Testes.md
03 - Arquitetura/Arquitetura de Configuração.md

04 - Backend/Visão Geral do Backend.md
04 - Backend/Organização por Features.md
04 - Backend/Domain.md
04 - Backend/Application.md
04 - Backend/API.md
04 - Backend/Infrastructure.md
04 - Backend/Worker.md
04 - Backend/Contracts.md
04 - Backend/Shared Kernel.md
```

---

# 132. Filosofia Final

O projeto Data deverá tratar o banco como um detalhe importante, mas ainda assim um detalhe externo ao núcleo do negócio.

Seu código deverá expressar ações como:

```text
Persistir Aggregate
Consultar Read Model
Confirmar transação
Controlar concorrência
Registrar Outbox
Registrar Inbox
Aplicar Migration
```

Ele não deverá expressar ações como:

```text
Gerar conteúdo com IA
Publicar no RabbitMQ
Enviar arquivo ao Azure
Executar workflow do n8n
Aplicar regra de aprovação
Decidir se uma execução pode concluir
```

Essas responsabilidades pertencem a outras camadas.

A regra principal será:

> Data garante persistência, integridade e consistência local; o Domain define o que deve permanecer verdadeiro e a Application coordena quando as mudanças acontecem.

Quando essa separação for respeitada, o PostgreSQL e o EF Core poderão evoluir sem contaminar os casos de uso ou as regras centrais do Infinite Content AI.