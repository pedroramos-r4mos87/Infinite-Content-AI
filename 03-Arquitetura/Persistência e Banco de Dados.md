# Persistência e Banco de Dados

## Objetivo

Definir como o Infinite Content AI armazenará, consultará, versionará e protegerá seus dados.

A persistência deverá ser confiável, auditável, testável e desacoplada das regras de negócio.

O banco de dados será tratado como um detalhe técnico da aplicação.

O Domain e a Application não deverão depender diretamente de Entity Framework Core, PostgreSQL ou qualquer tecnologia específica de persistência.

---

# Decisão Inicial

A stack inicial de persistência será:

```text
PostgreSQL
    +
Entity Framework Core
```

O PostgreSQL será o banco relacional principal.

O Entity Framework Core será utilizado como ORM para:

- Mapeamento de entidades.
    
- Controle de alterações.
    
- Consultas.
    
- Transações.
    
- Migrations.
    
- Concorrência otimista.
    
- Interceptors.
    
- Persistência da Outbox.
    

Essa escolha deverá ser registrada em ADR.

---

# Princípio Fundamental

A Application define as necessidades de persistência.

A Data implementa essas necessidades.

Fluxo:

```text
Application
    ↓
Repository Interface
    ↓
Data
    ↓
Entity Framework Core
    ↓
PostgreSQL
```

A Application não conhece `DbContext`.

A API não conhece `DbContext`.

O Worker não acessa `DbContext` diretamente.

A Infrastructure não contém persistência relacional.

---

# Responsabilidade do Projeto Data

O projeto:

```text
InfiniteContent.Data
```

será responsável por:

- DbContext.
    
- Mapeamentos do EF Core.
    
- Configurações de entidades.
    
- Repositórios.
    
- Queries.
    
- Migrations.
    
- Unit of Work.
    
- Transações locais.
    
- Interceptors.
    
- Outbox.
    
- Inbox.
    
- Seed técnico.
    
- Otimizações de persistência.
    
- Registro de dependências.
    

---

# O que não pertence ao Data

O projeto Data não deverá conter:

- Regras de negócio.
    
- Agentes.
    
- Pipelines.
    
- Providers de IA.
    
- Integrações com YouTube.
    
- Clientes HTTP.
    
- Redis.
    
- Storage de arquivos.
    
- Mensageria concreta.
    
- Webhooks.
    
- Orquestração de casos de uso.
    

O Data persiste dados.

Ele não decide o negócio.

---

# Estrutura Sugerida

```text
Data/
├── Context/
│   ├── InfiniteContentDbContext.cs
│   ├── DesignTimeDbContextFactory.cs
│   └── DatabaseConstants.cs
│
├── Configurations/
│   ├── ContentProjectConfiguration.cs
│   ├── PipelineExecutionConfiguration.cs
│   ├── PipelineStepExecutionConfiguration.cs
│   ├── PublicationConfiguration.cs
│   ├── ProviderUsageConfiguration.cs
│   └── OutboxMessageConfiguration.cs
│
├── Repositories/
│   ├── ContentProjectRepository.cs
│   ├── PipelineExecutionRepository.cs
│   ├── PublicationRepository.cs
│   └── PromptRepository.cs
│
├── Queries/
│   ├── ContentProjects/
│   ├── PipelineExecutions/
│   ├── Publications/
│   └── Analytics/
│
├── Transactions/
│   ├── UnitOfWork.cs
│   └── TransactionManager.cs
│
├── Interceptors/
│   ├── AuditableEntityInterceptor.cs
│   ├── DomainEventInterceptor.cs
│   └── OutboxInterceptor.cs
│
├── Outbox/
│   ├── OutboxMessage.cs
│   ├── OutboxProcessor.cs
│   └── OutboxMessageSerializer.cs
│
├── Inbox/
│   ├── InboxMessage.cs
│   └── InboxMessageRepository.cs
│
├── Seed/
│   ├── DatabaseSeeder.cs
│   └── SeedData.cs
│
├── Migrations/
├── DependencyInjection.cs
└── InfiniteContent.Data.csproj
```

---

# DbContext

O contexto principal será responsável por representar a unidade de persistência relacional.

Exemplo conceitual:

```csharp
public sealed class InfiniteContentDbContext : DbContext
{
    public InfiniteContentDbContext(
        DbContextOptions<InfiniteContentDbContext> options)
        : base(options)
    {
    }

    public DbSet<ContentProject> ContentProjects => Set<ContentProject>();

    public DbSet<PipelineExecution> PipelineExecutions => Set<PipelineExecution>();

    public DbSet<PipelineStepExecution> PipelineStepExecutions
        => Set<PipelineStepExecution>();

    public DbSet<Publication> Publications => Set<Publication>();

    public DbSet<ProviderUsage> ProviderUsages => Set<ProviderUsage>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(InfiniteContentDbContext).Assembly);
    }
}
```

---

# Regra para o DbContext

O `DbContext` deverá permanecer pequeno.

Ele não deverá conter:

- Regras de negócio.
    
- Consultas complexas.
    
- Lógica de integração.
    
- Decisões de domínio.
    
- Chamadas externas.
    
- Métodos específicos para cada caso de uso.
    

As configurações deverão ficar em classes separadas.

---

# Configurações de Entidades

Cada entidade persistida deverá possuir uma configuração própria.

Exemplo:

```csharp
public sealed class ContentProjectConfiguration
    : IEntityTypeConfiguration<ContentProject>
{
    public void Configure(
        EntityTypeBuilder<ContentProject> builder)
    {
        builder.ToTable("content_projects");

        builder.HasKey(project => project.Id);

        builder.Property(project => project.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(project => project.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
    }
}
```

---

# Convenções de Banco

## Tabelas

Utilizar nomes em:

```text
snake_case
```

Exemplos:

```text
content_projects
pipeline_executions
pipeline_step_executions
provider_usages
outbox_messages
```

---

## Colunas

Também utilizar:

```text
snake_case
```

Exemplos:

```text
created_at
updated_at
pipeline_execution_id
correlation_id
```

---

## Chaves Primárias

A chave primária padrão será:

```text
id
```

O tipo preferencial será:

```text
Guid
```

ou um identificador fortemente tipado baseado em `Guid`.

---

# Identificadores Fortemente Tipados

Sempre que trouxerem clareza, identificadores poderão ser representados por Value Objects.

Exemplo:

```csharp
public readonly record struct ContentProjectId(Guid Value);
```

Benefícios:

- Evita troca acidental de IDs.
    
- Melhora legibilidade.
    
- Fortalece o domínio.
    
- Reduz erros de integração.
    

O Data será responsável por converter o identificador para o tipo persistido.

---

# Escolha entre Guid e Identificadores Sequenciais

A decisão inicial poderá utilizar:

```text
UUID v7
```

ou outro identificador ordenável no tempo.

Benefícios:

- Geração distribuída.
    
- Menor dependência do banco.
    
- Melhor ordenação que UUID aleatório.
    
- Adequado para eventos e mensagens.
    

Caso a versão utilizada do runtime não ofereça suporte nativo, a estratégia deverá ser registrada em ADR.

---

# Entidades Iniciais

As principais entidades persistidas poderão incluir:

```text
User
Organization
ContentProject
PromptTemplate
PromptVersion
AgentDefinition
PipelineDefinition
PipelineExecution
PipelineStepExecution
PipelineApproval
PipelineArtifact
Publication
PublicationMetric
ProviderConfiguration
ProviderUsage
OutboxMessage
InboxMessage
WebhookDelivery
```

Nem todas precisam ser implementadas no MVP.

A modelagem deverá evoluir conforme os casos de uso reais.

---

# ContentProject

Representa um projeto de conteúdo.

Campos possíveis:

- Id.
    
- OrganizationId.
    
- Name.
    
- Description.
    
- Niche.
    
- DefaultLanguage.
    
- TargetAudience.
    
- DefaultChannel.
    
- StrategyProfile.
    
- Status.
    
- RequiresManualApproval.
    
- MaximumExecutionCost.
    
- CreatedAt.
    
- UpdatedAt.
    
- Version.
    

---

# PipelineExecution

Representa uma execução de pipeline.

Campos possíveis:

- Id.
    
- ProjectId.
    
- PipelineName.
    
- PipelineVersion.
    
- Status.
    
- CurrentStep.
    
- Input.
    
- Output.
    
- MaximumCost.
    
- AccumulatedCost.
    
- CorrelationId.
    
- StartedAt.
    
- CompletedAt.
    
- FailedAt.
    
- CancelledAt.
    
- ErrorCode.
    
- ErrorMessage.
    
- Version.
    

---

# PipelineStepExecution

Representa a execução de uma etapa.

Campos possíveis:

- Id.
    
- PipelineExecutionId.
    
- StepName.
    
- StepType.
    
- StepVersion.
    
- Order.
    
- Status.
    
- AttemptCount.
    
- Input.
    
- Output.
    
- Provider.
    
- Model.
    
- InputTokens.
    
- OutputTokens.
    
- EstimatedCost.
    
- ActualCost.
    
- StartedAt.
    
- CompletedAt.
    
- ErrorCode.
    
- ErrorMessage.
    
- IdempotencyKey.
    
- Version.
    

---

# PipelineApproval

Representa uma decisão humana.

Campos possíveis:

- Id.
    
- PipelineExecutionId.
    
- StepExecutionId.
    
- ApprovalType.
    
- Status.
    
- RequestedAt.
    
- DecidedAt.
    
- DecidedBy.
    
- Comments.
    
- ArtifactVersion.
    
- ExpirationDate.
    

---

# PipelineArtifact

Representa um artefato produzido.

Exemplos:

- Roteiro.
    
- Pesquisa.
    
- Áudio.
    
- Thumbnail.
    
- Vídeo.
    
- Legenda.
    
- Relatório.
    
- Arquivo de publicação.
    

Campos possíveis:

- Id.
    
- PipelineExecutionId.
    
- StepExecutionId.
    
- ArtifactType.
    
- Name.
    
- FileReferenceId.
    
- Version.
    
- ContentType.
    
- Size.
    
- Metadata.
    
- CreatedAt.
    

Arquivos grandes não deverão ser armazenados diretamente no PostgreSQL.

O banco deverá armazenar apenas referências e metadados.

---

# Publication

Representa uma publicação externa.

Campos possíveis:

- Id.
    
- ProjectId.
    
- PipelineExecutionId.
    
- Platform.
    
- ExternalPublicationId.
    
- Status.
    
- Title.
    
- Description.
    
- Url.
    
- ScheduledAt.
    
- PublishedAt.
    
- Privacy.
    
- IdempotencyKey.
    
- LastSynchronizedAt.
    
- CreatedAt.
    
- UpdatedAt.
    

---

# ProviderUsage

Representa o consumo de um provider externo.

Campos possíveis:

- Id.
    
- Provider.
    
- Model.
    
- Capability.
    
- Operation.
    
- ProjectId.
    
- PipelineExecutionId.
    
- StepExecutionId.
    
- InputUnits.
    
- OutputUnits.
    
- EstimatedCost.
    
- ActualCost.
    
- Currency.
    
- DurationMilliseconds.
    
- Status.
    
- ExternalRequestId.
    
- CorrelationId.
    
- OccurredAt.
    

---

# JSON no PostgreSQL

O PostgreSQL oferece suporte a `jsonb`.

Esse recurso poderá ser utilizado para:

- Metadados flexíveis.
    
- Inputs de execução.
    
- Outputs intermediários.
    
- Configurações específicas.
    
- Payloads da Outbox.
    
- Respostas normalizadas.
    
- Dados que variam entre providers.
    

---

# Quando utilizar jsonb

Utilizar `jsonb` quando:

- A estrutura puder variar.
    
- O conteúdo não exigir relacionamentos fortes.
    
- A flexibilidade for realmente necessária.
    
- O dado for principalmente histórico ou de auditoria.
    
- A consulta por campos internos for limitada.
    

---

# Quando não utilizar jsonb

Evitar `jsonb` para:

- Campos centrais do domínio.
    
- Dados usados frequentemente em filtros.
    
- Relacionamentos importantes.
    
- Campos com integridade referencial.
    
- Valores que exigem constraints.
    
- Informações usadas em ordenação constante.
    

Regra prática:

> Flexibilidade não deve substituir modelagem.

---

# Value Objects

Value Objects deverão ser persistidos de forma explícita.

Exemplos:

- Money.
    
- LanguageCode.
    
- ProviderName.
    
- ModelName.
    
- CorrelationId.
    
- FileReference.
    
- ContentChannel.
    

O mapeamento poderá utilizar:

- Owned entities.
    
- Value converters.
    
- Colunas separadas.
    
- Tipos complexos do EF Core.
    

A escolha dependerá da necessidade de consulta.

---

# Enums

Enums poderão ser persistidos como texto.

Exemplo:

```csharp
builder.Property(entity => entity.Status)
    .HasConversion<string>()
    .HasMaxLength(50);
```

Benefícios:

- Melhor leitura no banco.
    
- Menor dependência da ordem numérica.
    
- Migrações mais seguras.
    

Alterações de nome deverão ser tratadas com cuidado.

---

# Datas e Horários

Todas as datas deverão utilizar UTC.

Preferir:

```csharp
DateTimeOffset
```

No banco:

```text
timestamp with time zone
```

O sistema não deverá persistir horários locais sem timezone.

Conversões para horário do usuário ocorrerão na borda da aplicação.

---

# Valores Monetários

Custos deverão ser armazenados com precisão decimal.

Exemplo:

```text
numeric(18, 8)
```

ou precisão definida conforme a necessidade.

Sempre armazenar:

- Valor.
    
- Moeda.
    
- Data de referência de preço, quando relevante.
    

Nunca utilizar `float` ou `double` para valores financeiros.

---

# Soft Delete

Soft delete não será aplicado automaticamente a todas as entidades.

Ele deverá ser utilizado apenas quando houver necessidade real de:

- Auditoria.
    
- Recuperação.
    
- Regras legais.
    
- Histórico de negócio.
    
- Referências que não podem ser quebradas.
    

Campos possíveis:

- IsDeleted.
    
- DeletedAt.
    
- DeletedBy.
    

Soft delete adiciona complexidade e deverá ser uma decisão consciente.

---

# Auditoria

Entidades relevantes poderão implementar auditoria.

Campos comuns:

- CreatedAt.
    
- CreatedBy.
    
- UpdatedAt.
    
- UpdatedBy.
    
- DeletedAt.
    
- DeletedBy.
    

Um interceptor poderá preencher esses valores automaticamente.

---

# Entidade Auditável

Exemplo conceitual:

```csharp
public abstract class AuditableEntity
{
    public DateTimeOffset CreatedAt { get; private set; }

    public string? CreatedBy { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public string? UpdatedBy { get; private set; }
}
```

A abstração não deverá conter dependência do EF Core.

---

# Interceptors

Interceptors poderão ser utilizados para comportamentos técnicos.

Exemplos:

- Auditoria.
    
- Criação de mensagens Outbox.
    
- Controle de datas.
    
- Logging técnico.
    
- Conversão de eventos de domínio.
    

Interceptors não deverão conter regras de negócio.

---

# Repositórios

Repositórios serão utilizados principalmente para aggregates e operações de escrita.

Exemplo:

```csharp
public interface IContentProjectRepository
{
    Task<ContentProject?> GetByIdAsync(
        ContentProjectId id,
        CancellationToken cancellationToken);

    Task AddAsync(
        ContentProject project,
        CancellationToken cancellationToken);
}
```

A interface ficará na Application ou no módulo que necessita da abstração.

A implementação ficará em Data.

---

# Regra para Repositórios

Repositórios deverão representar coleções de aggregates.

Eles não deverão virar uma camada genérica de CRUD.

Evitar:

```csharp
IGenericRepository<TEntity>
```

com dezenas de métodos genéricos.

Esse padrão costuma esconder intenção e produzir abstrações fracas.

Preferir interfaces específicas.

---

# Métodos com Intenção

Preferir:

```csharp
GetActiveByIdAsync()
GetPendingApprovalAsync()
ExistsByExternalPublicationIdAsync()
```

em vez de:

```csharp
FindAsync(expression)
GetAllAsync()
Query()
```

A interface deve expressar a necessidade da Application.

---

# IQueryable

O projeto Data não deverá expor `IQueryable` para a Application.

Motivos:

- Vaza detalhes do ORM.
    
- Dificulta testes.
    
- Espalha lógica de consulta.
    
- Cria dependência implícita do EF Core.
    
- Permite consultas imprevisíveis.
    

Consultas deverão ser encapsuladas.

---

# Unit of Work

O `DbContext` já representa uma Unit of Work técnica.

A Application poderá depender de uma abstração simples.

Exemplo:

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken);
}
```

A implementação será o próprio contexto ou um adaptador.

---

# Regra de Commit

Os repositórios não deverão chamar `SaveChangesAsync` individualmente.

O caso de uso controla o momento do commit.

Exemplo:

```text
Handler
    ↓
Repository.AddAsync()
    ↓
Repository.Update()
    ↓
UnitOfWork.SaveChangesAsync()
```

Isso permite uma única transação local.

---

# Transações

Transações deverão ser curtas.

Uma transação poderá envolver:

- Alteração de aggregate.
    
- Registro de auditoria.
    
- Persistência de Outbox.
    
- Atualizações relacionadas dentro do mesmo banco.
    

Uma transação não deverá permanecer aberta durante:

- Chamada de IA.
    
- Upload de arquivo.
    
- Renderização de vídeo.
    
- Publicação externa.
    
- Envio de webhook.
    
- Espera de aprovação humana.
    
- Processamento de fila.
    

---

# Exemplo Correto

```text
Carregar aggregate
    ↓
Aplicar regra
    ↓
Salvar aggregate
    ↓
Salvar Outbox
    ↓
Commit
```

Depois:

```text
Outbox Processor
    ↓
Publica evento
```

---

# Exemplo Incorreto

```text
Abrir transação
    ↓
Chamar OpenAI
    ↓
Renderizar vídeo
    ↓
Publicar no YouTube
    ↓
Salvar banco
    ↓
Commit
```

Essa abordagem cria locks longos, baixa resiliência e alto risco de inconsistência.

---

# Consistência

Dentro do mesmo banco e da mesma transação, o sistema utilizará consistência forte.

Entre:

- Banco.
    
- Broker.
    
- Storage.
    
- Providers.
    
- Plataformas externas.
    
- n8n.
    

será utilizada consistência eventual.

---

# Outbox Pattern

Eventos críticos serão persistidos na mesma transação dos dados de negócio.

Fluxo:

```text
Application altera aggregate
    ↓
Domain Event criado
    ↓
Data salva aggregate
    ↓
Data salva OutboxMessage
    ↓
Commit
    ↓
Outbox Processor publica evento
```

---

# Estrutura da Outbox

Campos sugeridos:

- Id.
    
- Type.
    
- Version.
    
- Payload.
    
- Headers.
    
- OccurredAt.
    
- ProcessedAt.
    
- AttemptCount.
    
- NextAttemptAt.
    
- Error.
    
- CorrelationId.
    
- CausationId.
    

---

# Processamento da Outbox

O Outbox Processor deverá:

1. Buscar mensagens pendentes.
    
2. Bloquear ou reservar um lote.
    
3. Publicar cada mensagem.
    
4. Marcar como processada.
    
5. Registrar falhas.
    
6. Aplicar retentativas.
    
7. Encaminhar falhas permanentes para tratamento.
    

---

# Concorrência na Outbox

Múltiplas instâncias poderão processar a Outbox.

Estratégias possíveis:

- `FOR UPDATE SKIP LOCKED`.
    
- Lease temporária.
    
- Coluna de processamento.
    
- Particionamento.
    
- Controle otimista.
    

A estratégia deverá evitar publicação duplicada sempre que possível.

Consumidores ainda deverão ser idempotentes.

---

# Inbox Pattern

A Inbox armazenará identificadores de mensagens recebidas.

Objetivo:

- Evitar reprocessamento duplicado.
    
- Garantir idempotência.
    
- Manter histórico mínimo de consumo.
    

Campos possíveis:

- MessageId.
    
- Consumer.
    
- ReceivedAt.
    
- ProcessedAt.
    
- Status.
    
- Error.
    
- CorrelationId.
    

---

# Idempotência

Operações críticas deverão possuir chave de idempotência.

Exemplos:

- Publicação.
    
- Agendamento.
    
- Processamento de webhook.
    
- Geração de cobrança.
    
- Criação de execução.
    
- Consumo de mensagem.
    

Uma constraint única poderá proteger a operação.

Exemplo:

```text
unique(platform, idempotency_key)
```

---

# Concorrência Otimista

Entidades críticas deverão utilizar controle de versão.

Exemplo:

```csharp
public byte[] RowVersion { get; private set; } = [];
```

ou uma coluna numérica:

```text
version
```

O EF Core deverá detectar atualizações concorrentes.

---

# Casos de Uso para Concorrência

- Dois Workers tentando avançar a mesma execução.
    
- Duas aprovações simultâneas.
    
- Atualização concorrente de publicação.
    
- Retomada duplicada de pipeline.
    
- Alteração simultânea de configuração.
    

---

# Tratamento de Concurrency Conflict

Quando ocorrer conflito:

1. Identificar a entidade.
    
2. Recarregar o estado atual.
    
3. Avaliar se a operação pode ser repetida.
    
4. Evitar sobrescrita silenciosa.
    
5. Registrar o conflito.
    
6. Retornar falha controlada quando necessário.
    

---

# Locks

Locks pessimistas serão utilizados apenas quando necessário.

Exemplos:

- Reservar jobs.
    
- Processar lote da Outbox.
    
- Garantir exclusividade temporária.
    
- Controlar recursos escassos.
    

Evitar locks longos e distribuídos sem necessidade.

---

# Queries

Consultas de leitura não precisam obrigatoriamente utilizar repositórios de aggregates.

A Application poderá definir query services ou read models.

Exemplo:

```csharp
public interface IPipelineExecutionQueries
{
    Task<PipelineExecutionDetails?> GetDetailsAsync(
        Guid executionId,
        CancellationToken cancellationToken);
}
```

A implementação ficará no Data.

---

# CQRS Pragmático

O projeto poderá utilizar separação entre leitura e escrita sem exigir bancos diferentes.

## Escrita

- Aggregates.
    
- Repositórios.
    
- Domain.
    
- Unit of Work.
    

## Leitura

- Projeções.
    
- DTOs.
    
- Queries específicas.
    
- `AsNoTracking`.
    
- SQL otimizado, quando necessário.
    

---

# No Tracking

Consultas somente leitura deverão utilizar:

```csharp
AsNoTracking()
```

quando o rastreamento não for necessário.

Benefícios:

- Menor uso de memória.
    
- Melhor desempenho.
    
- Menor custo de processamento.
    

---

# Projeção Direta

Preferir projetar diretamente para DTOs.

Exemplo:

```csharp
var result = await context.PipelineExecutions
    .AsNoTracking()
    .Where(execution => execution.Id == executionId)
    .Select(execution => new PipelineExecutionDetails(
        execution.Id,
        execution.Status,
        execution.CurrentStep,
        execution.AccumulatedCost))
    .SingleOrDefaultAsync(cancellationToken);
```

Evitar carregar aggregates completos apenas para exibição.

---

# Paginação

Listagens deverão utilizar paginação.

Preferir paginação por cursor quando houver grande volume ou necessidade de estabilidade.

Paginação por offset poderá ser utilizada em telas administrativas simples.

Toda paginação deverá possuir limite máximo.

---

# Filtros

Filtros importantes deverão ser indexados.

Exemplos:

- ProjectId.
    
- PipelineExecutionId.
    
- Status.
    
- CreatedAt.
    
- ScheduledAt.
    
- Provider.
    
- Platform.
    
- CorrelationId.
    
- ExternalPublicationId.
    

---

# Índices

Índices deverão ser criados com base em consultas reais.

Exemplos iniciais:

```text
pipeline_executions(project_id, created_at desc)
pipeline_executions(status, created_at)
pipeline_step_executions(pipeline_execution_id, order)
publications(platform, external_publication_id)
provider_usages(project_id, occurred_at)
outbox_messages(processed_at, next_attempt_at)
```

Índices excessivos também possuem custo.

A estratégia deverá ser medida.

---

# Constraints

O banco deverá proteger invariantes técnicas.

Exemplos:

- Campos obrigatórios.
    
- Tamanhos máximos.
    
- Unicidade.
    
- Relacionamentos.
    
- Valores positivos.
    
- Datas coerentes.
    
- Chaves de idempotência.
    

Regras centrais de negócio continuarão no Domain.

---

# Foreign Keys

Relacionamentos importantes deverão utilizar foreign keys.

A estratégia de exclusão deverá ser explícita.

Evitar `Cascade Delete` indiscriminado.

Preferir:

- Restrict.
    
- Set null.
    
- Exclusão controlada.
    

Dados históricos não deverão desaparecer por acidente.

---

# Histórico

Algumas informações deverão ser imutáveis ou versionadas.

Exemplos:

- Versões de prompts.
    
- Versões de pipelines.
    
- Aprovações.
    
- Publicações.
    
- Uso de providers.
    
- Eventos.
    
- Execuções.
    
- Artefatos aprovados.
    

Atualizações não deverão destruir o histórico necessário para auditoria.

---

# Versionamento de Prompts

Estrutura possível:

```text
prompt_templates
prompt_versions
```

## PromptTemplate

- Id.
    
- Name.
    
- AgentType.
    
- Status.
    
- CurrentVersionId.
    

## PromptVersion

- Id.
    
- PromptTemplateId.
    
- Version.
    
- Content.
    
- Variables.
    
- ModelPolicy.
    
- CreatedAt.
    
- CreatedBy.
    
- IsActive.
    

Uma execução deverá registrar a versão utilizada.

---

# Versionamento de Pipelines

Estrutura possível:

```text
pipeline_definitions
pipeline_definition_versions
```

Uma execução iniciada deverá permanecer vinculada à versão original.

Alterações futuras não deverão modificar execuções antigas.

---

# Migrations

Todas as alterações de schema deverão ser feitas por migrations versionadas.

Comandos manuais em produção deverão ser evitados.

Cada migration deverá:

- Ter nome descritivo.
    
- Ser pequena.
    
- Ser revisável.
    
- Considerar rollback.
    
- Avaliar impacto em dados existentes.
    
- Avaliar locks.
    
- Avaliar tempo de execução.
    

---

# Convenção de Nome para Migrations

Exemplos:

```text
InitialSchema
AddPipelineExecutionTables
AddProviderUsageTracking
CreateOutboxMessages
AddPublicationIdempotencyKey
```

Evitar nomes vagos como:

```text
UpdateDatabase
Changes
Fix
Migration2
```

---

# Migrations em Produção

Migrations em produção não deverão ser executadas automaticamente por cada instância da aplicação.

Preferir:

- Job de deploy.
    
- Pipeline de CI/CD.
    
- Etapa controlada.
    
- Processo administrativo único.
    

Isso evita concorrência e alterações inesperadas.

---

# Expand and Contract

Mudanças incompatíveis deverão utilizar estratégia de expansão e contração.

Exemplo:

1. Adicionar nova coluna opcional.
    
2. Publicar código que escreve nos dois formatos.
    
3. Migrar dados antigos.
    
4. Atualizar consumidores.
    
5. Tornar a nova coluna obrigatória.
    
6. Remover a coluna antiga em deploy posterior.
    

Essa abordagem reduz downtime.

---

# Seed

Seed será utilizado apenas para dados controlados.

Exemplos:

- Perfis padrão.
    
- Tipos de agente.
    
- Status técnicos.
    
- Configurações iniciais.
    
- Dados de desenvolvimento.
    

Seed não deverá ser utilizado como substituto para migrations de dados complexas.

---

# Dados de Desenvolvimento

O ambiente de desenvolvimento poderá possuir dados fictícios.

Nunca utilizar:

- Dados reais de produção.
    
- Tokens reais.
    
- Informações pessoais reais.
    
- Conteúdo sensível.
    

---

# Backup

O banco deverá possuir estratégia de backup.

A estratégia deverá considerar:

- Frequência.
    
- Retenção.
    
- Criptografia.
    
- Região.
    
- Restauração ponto no tempo.
    
- Testes periódicos de restauração.
    

Backup sem teste de restauração não é uma garantia real.

---

# Restore

O processo de restore deverá ser documentado e testado.

Deverá incluir:

- Responsável.
    
- Procedimento.
    
- Tempo estimado operacional.
    
- Validação de integridade.
    
- Reativação de serviços.
    
- Reprocessamento de mensagens.
    
- Verificação de integrações.
    

---

# Retenção de Dados

Cada categoria de dado deverá possuir política de retenção.

Exemplos:

- Logs técnicos.
    
- Prompts.
    
- Respostas de IA.
    
- Artefatos temporários.
    
- Webhook deliveries.
    
- Mensagens Outbox processadas.
    
- Inbox.
    
- Métricas.
    
- Dados de auditoria.
    

Dados não deverão ser mantidos indefinidamente sem necessidade.

---

# Limpeza

Jobs de manutenção poderão remover ou arquivar:

- Outbox processada antiga.
    
- Inbox antiga.
    
- Artefatos temporários.
    
- Tokens expirados.
    
- Sessões antigas.
    
- Logs operacionais.
    
- Jobs abandonados.
    
- Webhooks antigos.
    

A limpeza deverá preservar auditoria e requisitos legais.

---

# Particionamento

Particionamento não será utilizado inicialmente sem necessidade.

Poderá ser considerado para tabelas de grande volume:

- ProviderUsage.
    
- PipelineStepExecution.
    
- PublicationMetric.
    
- AuditLog.
    
- OutboxMessage.
    
- WebhookDelivery.
    

A decisão deverá ser baseada em volume real.

---

# Arquivamento

Dados históricos antigos poderão ser movidos para armazenamento mais barato.

Exemplos:

- Métricas antigas.
    
- Logs detalhados.
    
- Payloads completos.
    
- Artefatos expirados.
    
- Execuções finalizadas há muito tempo.
    

O sistema deverá manter referências e capacidade de auditoria quando necessária.

---

# Segurança

O banco deverá utilizar:

- Conexão criptografada.
    
- Usuários com privilégios mínimos.
    
- Credenciais separadas por ambiente.
    
- Rotação de credenciais.
    
- Restrição de rede.
    
- Backups criptografados.
    
- Auditoria de acesso.
    
- Secret Manager.
    

---

# Princípio do Menor Privilégio

A aplicação deverá possuir apenas as permissões necessárias.

Exemplos:

- Aplicação de runtime não precisa criar banco.
    
- Worker não precisa administrar usuários.
    
- Ferramenta de migrations pode possuir permissões separadas.
    
- Usuário de leitura pode ser separado no futuro.
    

---

# Dados Sensíveis

Dados sensíveis deverão ser:

- Minimizados.
    
- Criptografados quando necessário.
    
- Mascarados em logs.
    
- Restritos por autorização.
    
- Excluídos conforme política.
    
- Auditados.
    

Tokens OAuth e credenciais externas deverão ser criptografados em repouso.

---

# Criptografia em Nível de Aplicação

Campos extremamente sensíveis poderão ser criptografados antes da persistência.

Exemplos:

- Refresh tokens.
    
- Chaves privadas.
    
- Credenciais de publicação.
    
- Segredos de webhooks.
    

A estratégia de gerenciamento de chaves deverá ficar fora do banco.

---

# Multi-Tenancy

O projeto deverá estar preparado para múltiplas organizações.

A abordagem inicial recomendada é:

```text
Shared Database
    +
Shared Schema
    +
OrganizationId
```

As entidades pertencentes a clientes deverão conter:

```text
organization_id
```

---

# Isolamento por Organização

Toda consulta de dados do cliente deverá aplicar filtro por organização.

Exemplo:

```text
organization_id = current_organization_id
```

O isolamento não deverá depender apenas do frontend.

A Application e o Data deverão proteger esse limite.

---

# Global Query Filters

Global query filters poderão ser utilizados para:

- OrganizationId.
    
- Soft delete.
    

Porém, deverão ser aplicados com cautela.

Riscos:

- Consultas administrativas incorretas.
    
- Filtros ocultos.
    
- Dificuldade de depuração.
    
- Vazamento por uso inadequado de `IgnoreQueryFilters`.
    

A estratégia deverá ser testada.

---

# Row-Level Security

No futuro, PostgreSQL Row-Level Security poderá ser avaliado como camada adicional.

RLS não substitui validação na aplicação.

Essa decisão deverá ser registrada em ADR se adotada.

---

# Testes Unitários

Testes unitários não deverão depender do PostgreSQL.

O Domain e a Application deverão ser testados com:

- Fakes.
    
- Mocks.
    
- Stubs.
    
- Repositórios em memória específicos para teste.
    

Não utilizar o provider InMemory do EF Core como substituto universal de banco relacional.

---

# Testes de Integração

Testes do Data deverão utilizar PostgreSQL real ou containerizado.

Motivos:

- Comportamento relacional real.
    
- Constraints reais.
    
- Migrations reais.
    
- Tipos PostgreSQL.
    
- Concorrência.
    
- Transações.
    
- `jsonb`.
    
- Índices.
    
- SQL específico.
    

---

# Testcontainers

Poderá ser utilizado Testcontainers para subir PostgreSQL durante os testes.

Fluxo:

```text
Teste inicia container
    ↓
Aplica migrations
    ↓
Executa cenário
    ↓
Valida resultado
    ↓
Descarta container
```

---

# Testes de Persistência

Devem validar:

- Mapeamentos.
    
- Constraints.
    
- Repositórios.
    
- Queries.
    
- Transações.
    
- Concorrência.
    
- Outbox.
    
- Inbox.
    
- Idempotência.
    
- Auditoria.
    
- Soft delete.
    
- Multi-tenancy.
    
- Migrations.
    

---

# Testes de Migration

O pipeline deverá validar que:

- O banco vazio pode ser criado.
    
- A última versão pode ser aplicada.
    
- Migrações executam em sequência.
    
- Migrações críticas preservam dados.
    
- O modelo do EF Core está sincronizado.
    

---

# Performance

O desempenho deverá ser medido.

Evitar otimização prematura, mas monitorar:

- Queries lentas.
    
- N+1.
    
- Falta de índices.
    
- Excesso de tracking.
    
- Carregamento excessivo.
    
- Payloads JSON grandes.
    
- Locks.
    
- Crescimento de tabelas.
    
- Tempo de migrations.
    

---

# N+1

Consultas deverão evitar múltiplas idas ao banco por item.

Estratégias:

- Projeção.
    
- Includes controlados.
    
- Consultas agregadas.
    
- Batch.
    
- Read models.
    

Lazy loading não será habilitado por padrão.

---

# Include

`Include` deverá ser utilizado com cuidado.

Carregar grafos grandes pode causar:

- Consultas pesadas.
    
- Duplicação de dados.
    
- Alto uso de memória.
    
- Cartesiano excessivo.
    

Preferir projeções específicas para leitura.

---

# Split Queries

`AsSplitQuery` poderá ser utilizado quando necessário para evitar explosão cartesiana.

A decisão deverá ser baseada na consulta e medida.

---

# SQL Direto

SQL direto poderá ser utilizado quando:

- A consulta for complexa.
    
- O desempenho exigir.
    
- O EF Core gerar SQL inadequado.
    
- Houver uso de funcionalidades específicas do PostgreSQL.
    

O SQL deverá:

- Ser parametrizado.
    
- Possuir testes.
    
- Ficar encapsulado no Data.
    
- Ser documentado.
    
- Não vazar para a Application.
    

---

# Dapper

Dapper poderá ser utilizado futuramente para consultas específicas.

A adoção não deverá substituir o EF Core indiscriminadamente.

Possível combinação:

```text
EF Core para escrita
Dapper para leituras críticas
```

Essa decisão deverá ser baseada em necessidade real.

---

# Observabilidade

A camada Data deverá registrar:

- Tempo de consulta.
    
- Tempo de commit.
    
- Falhas.
    
- Deadlocks.
    
- Conflitos de concorrência.
    
- Quantidade de registros afetados.
    
- Conexões indisponíveis.
    
- Retentativas.
    
- Migrations aplicadas.
    

---

# Logs de SQL

SQL detalhado poderá ser habilitado em desenvolvimento.

Em produção, evitar registrar:

- Dados pessoais.
    
- Tokens.
    
- Prompts completos.
    
- Valores sensíveis.
    
- Parâmetros confidenciais.
    

Sensitive data logging deverá permanecer desabilitado em produção.

---

# Métricas

Métricas importantes:

- Latência de queries.
    
- Latência de commits.
    
- Conexões ativas.
    
- Pool de conexões.
    
- Taxa de erro.
    
- Deadlocks.
    
- Timeouts.
    
- Tamanho das tabelas.
    
- Crescimento do banco.
    
- Outbox pendente.
    
- Inbox processada.
    
- Tempo de processamento da Outbox.
    
- Conflitos de concorrência.
    

---

# Health Checks

O sistema deverá possuir health check para o PostgreSQL.

## Readiness

Verifica se a aplicação consegue utilizar o banco.

## Liveness

Não deverá depender de uma consulta pesada ao banco.

Falhas de dependência deverão ser refletidas de forma adequada no ambiente de execução.

---

# Configuração

Exemplo conceitual:

```json
{
  "ConnectionStrings": {
    "Database": ""
  },
  "Database": {
    "CommandTimeoutSeconds": 30,
    "EnableDetailedErrors": false,
    "EnableSensitiveDataLogging": false,
    "MaximumRetryCount": 3
  }
}
```

A connection string deverá vir de Secret Manager ou variável de ambiente.

---

# Registro no DI

Exemplo conceitual:

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddData(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException(
                "Database connection string was not configured.");

        services.AddDbContext<InfiniteContentDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IUnitOfWork>(
            provider => provider.GetRequiredService<InfiniteContentDbContext>());

        services.AddScoped<IContentProjectRepository, ContentProjectRepository>();
        services.AddScoped<IPipelineExecutionRepository, PipelineExecutionRepository>();

        return services;
    }
}
```

---

# Resiliência de Conexão

Retentativas de conexão poderão ser habilitadas para falhas transitórias.

Elas deverão possuir:

- Limite.
    
- Backoff.
    
- Logs.
    
- Timeout.
    

Retentativas não deverão esconder falhas permanentes de configuração.

---

# Separação entre Banco e Storage

O PostgreSQL armazenará:

- Estado.
    
- Metadados.
    
- Relacionamentos.
    
- Auditoria.
    
- Referências.
    
- Configurações.
    
- Custos.
    
- Histórico.
    

O storage armazenará:

- Vídeos.
    
- Áudios.
    
- Imagens.
    
- Arquivos grandes.
    
- Legendas.
    
- Exportações.
    
- Artefatos binários.
    

---

# Estratégia para Arquivos

Fluxo:

```text
Arquivo produzido
    ↓
IFileStorage.SaveAsync()
    ↓
Storage externo
    ↓
FileReference
    ↓
Data persiste metadados
```

O banco não deverá ser usado como armazenamento principal de mídia.

---

# Falhas entre Storage e Banco

Problema possível:

```text
Arquivo salvo no storage
    ↓
Banco falha
```

Isso pode gerar arquivo órfão.

Estratégias:

- Upload temporário.
    
- Confirmação posterior.
    
- Job de limpeza.
    
- Estado Pending.
    
- Compensação.
    
- Idempotency key.
    

---

# Artefato Temporário

Fluxo recomendado:

```text
Upload com status Temporary
    ↓
Persistência confirmada
    ↓
Status Permanent
```

Arquivos temporários antigos poderão ser removidos por job.

---

# Dados Derivados

Resultados que podem ser recalculados não precisam ter o mesmo nível de retenção dos dados oficiais.

Exemplos:

- Cache.
    
- Projeções.
    
- Estatísticas temporárias.
    
- Pré-visualizações.
    
- Embeddings regeneráveis.
    

Mesmo assim, o custo de recomputação deverá ser considerado.

---

# Source of Truth

O PostgreSQL será a fonte oficial de verdade para:

- Projetos.
    
- Execuções.
    
- Estados.
    
- Aprovações.
    
- Publicações.
    
- Configurações.
    
- Custos registrados.
    
- Histórico.
    
- Referências de artefatos.
    

Redis, n8n, filas e storage não substituirão o banco como fonte de verdade desses dados.

---

# Regras Arquiteturais

- Domain não conhece Entity Framework Core.
    
- Application não conhece DbContext.
    
- API não acessa banco diretamente.
    
- Worker não acessa banco diretamente.
    
- Infrastructure não contém persistência relacional.
    
- Data implementa repositórios e queries.
    
- Repositórios não chamam SaveChanges individualmente.
    
- IQueryable não sai do Data.
    
- Transações devem ser curtas.
    
- Chamadas externas não ocorrem dentro de transações.
    
- Eventos críticos utilizam Outbox.
    
- Mensagens recebidas importantes utilizam idempotência.
    
- Arquivos grandes não são armazenados no PostgreSQL.
    
- Datas são persistidas em UTC.
    
- Valores financeiros utilizam decimal.
    
- Dados de clientes possuem OrganizationId.
    
- Toda migration é versionada.
    
- Migrations de produção são controladas.
    
- Queries de leitura usam projeção quando possível.
    
- Consultas somente leitura usam no tracking quando adequado.
    
- O banco protege constraints técnicas.
    
- O Domain protege regras de negócio.
    
- Dados sensíveis devem ser protegidos.
    
- Testes de integração utilizam PostgreSQL real.
    
- O banco é a fonte de verdade do estado da aplicação.
    

---

# Decisões Pendentes

As seguintes decisões deverão ser registradas em ADRs:

- Uso de UUID v7.
    
- Estratégia de multi-tenancy.
    
- Uso de Row-Level Security.
    
- Política de soft delete.
    
- Estratégia de criptografia de tokens.
    
- Uso de Dapper em consultas críticas.
    
- Estratégia de migrations em produção.
    
- Retenção da Outbox.
    
- Retenção da Inbox.
    
- Estratégia de backup.
    
- Estratégia de restore.
    
- Estratégia de particionamento futuro.
    
- Política de auditoria.
    
- Política de armazenamento de prompts e respostas.
    
- Estratégia de limpeza de artefatos órfãos.
    

---

# Exemplo Completo

```text
Usuário cria um projeto
    ↓
API chama CreateContentProjectHandler
    ↓
Domain cria ContentProject
    ↓
Repository adiciona o aggregate
    ↓
Domain Event é convertido em OutboxMessage
    ↓
UnitOfWork executa SaveChanges
    ↓
PostgreSQL confirma a transação
    ↓
Outbox Processor publica ContentProjectCreated
```

Execução de pipeline:

```text
Worker recebe StartPipelineCommand
    ↓
Application carrega PipelineExecution
    ↓
Domain valida estado atual
    ↓
Etapa é marcada como Running
    ↓
Data persiste o estado
    ↓
Provider externo é chamado fora da transação
    ↓
Resultado retorna
    ↓
Application atualiza a execução
    ↓
Data salva resultado, custo e Outbox
    ↓
Pipeline continua
```

---

# Objetivo Final

Criar uma camada de persistência sólida, previsível e preparada para crescimento.

O sistema deverá preservar consistência local, suportar processos distribuídos, manter histórico, controlar concorrência e permitir evolução do schema sem comprometer o núcleo do produto.