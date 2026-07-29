# Visão Geral do Backend

## 1. Objetivo

O backend do Infinite Content AI será responsável por executar os casos de uso da plataforma, aplicar regras de negócio, persistir dados, coordenar pipelines de geração de conteúdo, processar tarefas assíncronas e integrar serviços externos.

A arquitetura do backend deverá permitir:

- Evolução incremental.
    
- Testabilidade.
    
- Processamento síncrono e assíncrono.
    
- Execução de pipelines de longa duração.
    
- Integração com múltiplos providers de inteligência artificial.
    
- Recuperação após falhas.
    
- Idempotência.
    
- Observabilidade.
    
- Escalabilidade horizontal.
    
- Separação clara entre regras de negócio e detalhes técnicos.
    

O backend será construído inicialmente como um **monólito modular**, composto por uma API HTTP e um ou mais Worker Services.

A separação em microservices somente deverá ocorrer quando houver necessidade comprovada de:

- Escalabilidade independente.
    
- Deploy independente.
    
- Isolamento operacional.
    
- Limites de domínio claramente estabelecidos.
    
- Equipes independentes.
    
- Requisitos específicos de disponibilidade.
    

---

# 2. Visão Geral da Arquitetura

A solução será organizada nos seguintes projetos:

```text
InfiniteContentAI.sln
│
├── Api
├── Application
├── Domain
├── Data
├── Infrastructure
├── Worker
├── Contracts
└── SharedKernel
```

Cada projeto terá responsabilidades específicas.

```text
                    ┌─────────────────────┐
                    │       Clients       │
                    │ Web, Mobile, n8n    │
                    └──────────┬──────────┘
                               │ HTTP
                               ▼
                    ┌─────────────────────┐
                    │         Api         │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │     Application     │
                    │ Commands, Queries   │
                    │ Use Cases, Policies │
                    └──────────┬──────────┘
                               │
                ┌──────────────┴──────────────┐
                ▼                             ▼
       ┌─────────────────┐          ┌─────────────────┐
       │     Domain      │          │   Abstractions  │
       │ Business Rules  │          │ Ports/Interfaces│
       └─────────────────┘          └────────┬────────┘
                                             │
                          ┌──────────────────┴──────────────────┐
                          ▼                                     ▼
                 ┌─────────────────┐                   ┌─────────────────┐
                 │      Data       │                   │ Infrastructure  │
                 │ PostgreSQL / EF │                   │ AI, RabbitMQ,   │
                 │ Repositories    │                   │ Redis, Azure    │
                 └─────────────────┘                   └─────────────────┘
```

Processamentos assíncronos serão executados pelo Worker:

```text
RabbitMQ
    │
    ▼
Worker
    │
    ▼
Application
    │
    ├── Domain
    ├── Data
    └── Infrastructure
```

---

# 3. Princípios do Backend

## 3.1 Regras de negócio independentes de infraestrutura

O comportamento central do sistema não deverá depender diretamente de:

- PostgreSQL.
    
- Entity Framework Core.
    
- RabbitMQ.
    
- Redis.
    
- Azure.
    
- OpenAI.
    
- Anthropic.
    
- Google.
    
- n8n.
    
- ASP.NET Core.
    
- SDKs externos.
    

Essas tecnologias serão tratadas como detalhes externos.

---

## 3.2 Casos de uso explícitos

As operações do sistema deverão ser representadas por casos de uso explícitos.

Exemplos:

```text
CreateProject
StartPipeline
CancelPipeline
ResumePipeline
GenerateResearch
GenerateScript
ApproveArtifact
PublishContent
```

Cada caso de uso deverá possuir:

- Entrada definida.
    
- Validação.
    
- Regras de autorização.
    
- Comportamento previsível.
    
- Resultado explícito.
    
- Testes automatizados.
    

---

## 3.3 Hosts leves

A API e os Workers serão hosts da aplicação.

Eles deverão:

- Receber entradas.
    
- Autenticar o chamador.
    
- Converter contratos.
    
- Criar escopos de execução.
    
- Invocar a Application.
    
- Traduzir resultados.
    
- Registrar telemetria.
    

Eles não deverão concentrar regras de negócio.

---

## 3.4 Persistência isolada

Toda persistência relacional ficará no projeto `Data`.

O projeto `Infrastructure` não possuirá:

- DbContext.
    
- Migrations.
    
- Configurações do EF Core.
    
- Repositories de PostgreSQL.
    

---

## 3.5 Integrações externas isoladas

Integrações com serviços externos ficarão no projeto `Infrastructure`.

Exemplos:

- Providers de IA.
    
- RabbitMQ.
    
- Redis.
    
- Azure Blob Storage.
    
- APIs de redes sociais.
    
- Webhooks.
    
- n8n.
    
- Serviço Python.
    
- Serviços de e-mail.
    
- Observabilidade externa.
    

---

## 3.6 Operações longas não bloqueiam a API

Operações de longa duração não deverão ser processadas durante uma requisição HTTP.

A API deverá iniciar a operação e devolver um identificador de acompanhamento.

Exemplo:

```text
POST /api/v1/pipelines/{pipelineId}/executions
```

Resposta:

```json
{
  "executionId": "01JZ6N8DTW7VYQ1P3N4F8M6K2R",
  "status": "queued"
}
```

O processamento será realizado por Workers.

---

## 3.7 Idempotência por padrão

Operações sensíveis a repetição deverão suportar idempotência.

Exemplos:

- Início de pipeline.
    
- Consumo de mensagens.
    
- Publicação de conteúdo.
    
- Processamento de callbacks.
    
- Criação de pagamentos.
    
- Uploads concluídos.
    
- Execução de etapas.
    

---

## 3.8 Observabilidade desde o início

Toda operação relevante deverá produzir contexto suficiente para investigação.

Informações mínimas:

- CorrelationId.
    
- TraceId.
    
- CausationId.
    
- TenantId.
    
- UserId.
    
- ExecutionId.
    
- PipelineId.
    
- StepId.
    
- Provider.
    
- Duração.
    
- Resultado.
    
- Código de erro.
    

---

# 4. Responsabilidades por Projeto

## 4.1 Api

O projeto `Api` será o ponto de entrada HTTP da plataforma.

Responsabilidades:

- Endpoints.
    
- Controllers ou Minimal APIs.
    
- Autenticação.
    
- Autorização HTTP.
    
- Rate limiting.
    
- Versionamento.
    
- OpenAPI.
    
- Problem Details.
    
- Serialização.
    
- Validação de contratos HTTP.
    
- Middleware.
    
- Health checks.
    
- Composição de dependências.
    

A Api poderá depender de:

```text
Application
Contracts
SharedKernel
Data
Infrastructure
```

A dependência de `Data` e `Infrastructure` deverá existir apenas para composição e inicialização.

Endpoints não deverão acessar diretamente:

- DbContext.
    
- Repositories concretos.
    
- RabbitMQ.
    
- Redis.
    
- SDKs de IA.
    
- Azure Storage.
    

---

## 4.2 Application

O projeto `Application` conterá os casos de uso.

Responsabilidades:

- Commands.
    
- Queries.
    
- Handlers.
    
- Validators.
    
- Application Services.
    
- Policies.
    
- Interfaces de persistência.
    
- Interfaces de mensageria.
    
- Interfaces de providers.
    
- Interfaces de storage.
    
- Autorização de casos de uso.
    
- Orquestração.
    
- Mapeamento para resultados da aplicação.
    
- Behaviors transversais.
    

Exemplos de pastas:

```text
Application
│
├── Abstractions
├── Behaviors
├── Common
├── Features
│   ├── Projects
│   ├── Pipelines
│   ├── Executions
│   ├── Artifacts
│   ├── Approvals
│   └── Publications
└── DependencyInjection.cs
```

A organização principal será por feature.

Exemplo:

```text
Features
└── Pipelines
    ├── StartPipeline
    │   ├── StartPipelineCommand.cs
    │   ├── StartPipelineValidator.cs
    │   ├── StartPipelineHandler.cs
    │   └── StartPipelineResult.cs
    ├── CancelPipeline
    └── GetPipelineExecution
```

---

## 4.3 Domain

O projeto `Domain` conterá o modelo de negócio.

Responsabilidades:

- Entidades.
    
- Aggregates.
    
- Value Objects.
    
- Domain Events.
    
- Domain Services.
    
- Invariantes.
    
- Políticas de domínio.
    
- Estados.
    
- Transições válidas.
    
- Erros do domínio.
    

Estrutura inicial:

```text
Domain
│
├── Projects
├── Pipelines
├── Executions
├── Artifacts
├── Approvals
├── Publications
└── Common
```

O Domain não deverá depender dos demais projetos da solução, exceto de elementos mínimos do `SharedKernel`, quando necessário.

---

## 4.4 Data

O projeto `Data` será responsável pela persistência.

Responsabilidades:

- DbContext.
    
- Mapeamentos do EF Core.
    
- Repositories.
    
- Unit of Work.
    
- Transactions.
    
- Migrations.
    
- Interceptors.
    
- Queries SQL especializadas.
    
- Outbox persistida.
    
- Inbox persistida.
    
- Controle de concorrência.
    
- Configurações do PostgreSQL.
    

Estrutura conceitual:

```text
Data
│
├── Context
│   └── ApplicationDbContext.cs
├── Configurations
├── Repositories
├── Queries
├── Interceptors
├── Outbox
├── Inbox
├── Migrations
└── DependencyInjection.cs
```

---

## 4.5 Infrastructure

O projeto `Infrastructure` implementará adapters externos.

Estrutura conceitual:

```text
Infrastructure
│
├── ArtificialIntelligence
│   ├── OpenAI
│   ├── Anthropic
│   ├── Google
│   └── Fake
├── Messaging
│   └── RabbitMQ
├── Caching
│   └── Redis
├── Storage
│   └── AzureBlob
├── Automation
│   └── N8n
├── Python
├── Webhooks
├── Observability
├── Resilience
└── DependencyInjection.cs
```

O Infrastructure deverá implementar interfaces definidas pela Application.

---

## 4.6 Worker

O projeto `Worker` executará processamento em segundo plano.

Responsabilidades:

- Consumo de mensagens.
    
- Execução de etapas de pipelines.
    
- Processamento da Outbox.
    
- Jobs recorrentes.
    
- Reconciliação.
    
- Recuperação de execuções interrompidas.
    
- Retentativas.
    
- Cancelamento cooperativo.
    
- Shutdown gracioso.
    
- Health checks.
    

Estrutura inicial:

```text
Worker
│
├── Consumers
├── BackgroundServices
├── Jobs
├── PipelineExecution
├── DependencyInjection.cs
└── Program.cs
```

No início, um único projeto Worker poderá hospedar todos os processos.

Com o crescimento da plataforma, ele poderá ser separado em hosts específicos:

```text
Workers
├── PipelineWorker
├── OutboxWorker
├── PublicationWorker
├── MediaWorker
└── MaintenanceWorker
```

A separação somente ocorrerá quando trouxer benefício operacional.

---

## 4.7 Contracts

O projeto `Contracts` conterá contratos compartilhados entre processos.

Exemplos:

```text
Contracts
│
├── Messaging
│   ├── Commands
│   ├── Events
│   └── Envelopes
├── Webhooks
└── Callbacks
```

Contratos deverão possuir versão explícita.

Exemplo:

```csharp
public sealed record PipelineExecutionRequestedV1(
    Guid ExecutionId,
    Guid PipelineId,
    Guid TenantId,
    DateTimeOffset RequestedAt);
```

Entidades do Domain não deverão ser utilizadas como contratos de integração.

---

## 4.8 SharedKernel

O projeto `SharedKernel` conterá elementos mínimos e estáveis.

Exemplos:

```text
SharedKernel
│
├── Results
├── Errors
├── Time
├── Identifiers
└── Primitives
```

Elementos candidatos:

- Result.
    
- Error.
    
- ErrorType.
    
- IClock.
    
- EntityId.
    
- PaginatedResult.
    
- Guard clauses essenciais.
    

O SharedKernel não deverá se transformar em uma pasta de utilitários genéricos.

---

# 5. Organização por Módulos

O backend será organizado inicialmente nos seguintes módulos de negócio:

```text
Projects
Pipelines
Executions
Artifacts
Approvals
Publications
Organizations
Identity
```

## Projects

Representa os projetos de conteúdo do usuário.

Exemplos:

- Canal do YouTube.
    
- Blog.
    
- Podcast.
    
- Campanha.
    
- Marca.
    
- Produto.
    

---

## Pipelines

Define processos de produção de conteúdo.

Um pipeline poderá conter etapas como:

```text
Research
Outline
Script
Review
Approval
MediaGeneration
Publication
```

---

## Executions

Representa uma execução concreta de um pipeline.

Responsabilidades:

- Estado da execução.
    
- Etapa atual.
    
- Histórico.
    
- Tentativas.
    
- Erros.
    
- Checkpoints.
    
- Cancelamento.
    
- Continuação.
    
- Versões utilizadas.
    

---

## Artifacts

Representa os materiais produzidos durante uma execução.

Exemplos:

- Pesquisa.
    
- Roteiro.
    
- Título.
    
- Descrição.
    
- Thumbnail.
    
- Áudio.
    
- Vídeo.
    
- Legenda.
    
- Metadados.
    
- Structured Output.
    

---

## Approvals

Representa aprovações humanas.

Exemplos:

- Aprovação de roteiro.
    
- Aprovação de título.
    
- Aprovação de mídia.
    
- Aprovação para publicação.
    

---

## Publications

Representa publicação em canais externos.

Exemplos:

- YouTube.
    
- WordPress.
    
- LinkedIn.
    
- Instagram.
    
- TikTok.
    
- Newsletter.
    

---

## Organizations

Representa o limite de tenancy e propriedade dos dados.

Mesmo que o MVP tenha apenas um usuário, os registros relevantes deverão possuir um limite de organização ou tenant quando isso não introduzir complexidade excessiva.

---

## Identity

Responsável por:

- Usuários.
    
- Autenticação.
    
- Papéis.
    
- Permissões.
    
- Associações com organizações.
    

O sistema de identidade não deverá concentrar regras específicas dos módulos de conteúdo.

---

# 6. Fluxo de uma Requisição HTTP

Exemplo: criação de um projeto.

```text
Cliente
    │
    ▼
POST /api/v1/projects
    │
    ▼
Endpoint
    │
    ├── Desserializa request
    ├── Autentica usuário
    ├── Resolve tenant
    └── Cria command
    │
    ▼
CreateProjectCommand
    │
    ▼
Application Handler
    │
    ├── Valida regras
    ├── Cria aggregate
    ├── Persiste
    └── Retorna resultado
    │
    ▼
Endpoint
    │
    ├── Mapeia resultado
    └── Retorna HTTP 201
```

Fluxo resumido:

```text
HTTP Request
    ↓
Endpoint
    ↓
Command ou Query
    ↓
Application
    ↓
Domain
    ↓
Data
    ↓
HTTP Response
```

---

# 7. Fluxo de Processamento Assíncrono

Exemplo: início de uma execução de pipeline.

```text
Cliente
    │
    ▼
POST /pipelines/{id}/executions
    │
    ▼
Application
    │
    ├── Cria PipelineExecution
    ├── Persiste estado Queued
    ├── Registra mensagem na Outbox
    └── Confirma transação
    │
    ▼
Outbox Worker
    │
    ├── Lê mensagem pendente
    └── Publica no RabbitMQ
    │
    ▼
RabbitMQ
    │
    ▼
Pipeline Worker
    │
    ├── Valida Inbox
    ├── Carrega execução
    ├── Executa etapa
    ├── Salva resultado
    ├── Cria próximo comando
    └── Confirma consumo
```

---

# 8. Fluxo de Execução de Pipeline

Um pipeline será tratado como uma máquina de estados persistida.

Exemplo:

```text
Queued
    ↓
Running
    ↓
Researching
    ↓
GeneratingScript
    ↓
AwaitingApproval
    ↓
Publishing
    ↓
Completed
```

Estados alternativos:

```text
Failed
Cancelled
Paused
Compensating
```

Cada etapa deverá:

1. Receber uma execução.
    
2. Validar o estado atual.
    
3. Verificar idempotência.
    
4. Executar o comportamento.
    
5. Persistir um checkpoint.
    
6. Produzir artefatos.
    
7. Registrar telemetria.
    
8. Agendar a próxima etapa.
    

---

# 9. Modelo de Processamento de Etapas

Uma etapa poderá ser representada por uma abstração como:

```csharp
public interface IPipelineStepHandler
{
    string StepType { get; }

    Task<Result<PipelineStepResult>> ExecuteAsync(
        PipelineStepContext context,
        CancellationToken cancellationToken);
}
```

Cada handler será responsável por um tipo de etapa.

Exemplos:

```text
ResearchStepHandler
ScriptStepHandler
ReviewStepHandler
ApprovalStepHandler
PublicationStepHandler
```

O handler não deverá controlar sozinho todo o pipeline.

A coordenação ficará em um componente de orquestração.

---

# 10. Consistência e Transações

## Operações locais

Alterações realizadas dentro do mesmo banco poderão utilizar transações do PostgreSQL.

Exemplo:

```text
Criar execução
Atualizar pipeline
Registrar Outbox
Commit
```

Essas operações deverão ocorrer na mesma transação quando fizerem parte de uma única unidade de consistência.

---

## Operações externas

Não será criada uma transação distribuída entre:

- PostgreSQL.
    
- RabbitMQ.
    
- Redis.
    
- Providers.
    
- Storage.
    
- APIs externas.
    

A consistência será obtida com:

- Outbox.
    
- Inbox.
    
- Idempotência.
    
- Retentativas.
    
- Compensações.
    
- Reconciliação.
    

---

# 11. Tratamento de Erros

Falhas esperadas deverão utilizar Result Pattern.

Exemplos:

- Projeto não encontrado.
    
- Pipeline inválido.
    
- Operação não permitida.
    
- Estado incompatível.
    
- Limite excedido.
    
- Aprovação pendente.
    
- Artefato inexistente.
    

Exceções serão reservadas para situações inesperadas ou técnicas.

Exemplos:

- Falha de conexão.
    
- Erro de serialização inesperado.
    
- Violação de invariante interna.
    
- Corrupção de estado.
    
- Falha inesperada do provider.
    

---

# 12. Validação

A validação ocorrerá em diferentes níveis.

## Contrato HTTP

Valida formato e presença de dados.

Exemplos:

- Campo obrigatório.
    
- Tamanho máximo.
    
- Formato de URL.
    
- Formato de idioma.
    

---

## Application

Valida regras do caso de uso.

Exemplos:

- Usuário possui acesso.
    
- Recurso pertence ao tenant.
    
- Pipeline está ativo.
    
- Limite de uso não foi excedido.
    

---

## Domain

Protege invariantes.

Exemplos:

- Execução concluída não pode voltar para Running.
    
- Artefato aprovado não pode ser alterado sem nova versão.
    
- Pipeline não pode iniciar sem etapas válidas.
    

---

## Banco de dados

Protege integridade persistente.

Exemplos:

- Foreign keys.
    
- Unique constraints.
    
- Not null.
    
- Check constraints.
    
- Controle de concorrência.
    

---

# 13. Autenticação e Autorização

A API será responsável pela autenticação do chamador.

A Application será responsável pelas regras de autorização do caso de uso.

Exemplo:

```text
Api
    └── Identifica usuário e tenant

Application
    └── Verifica se o usuário pode alterar o projeto
```

Nunca será suficiente validar apenas que o usuário está autenticado.

Toda operação deverá considerar:

- Tenant.
    
- Propriedade do recurso.
    
- Papel.
    
- Permissão.
    
- Escopo.
    
- Estado do recurso.
    

---

# 14. Multi-tenancy

O isolamento inicial será realizado por coluna.

Exemplo:

```text
Projects
├── Id
├── OrganizationId
└── Name
```

Todas as entidades pertencentes a um tenant deverão possuir `OrganizationId` ou uma associação equivalente.

Queries deverão filtrar explicitamente pelo tenant atual.

Não será permitido buscar um recurso apenas pelo seu identificador quando ele for tenant-scoped.

Exemplo incorreto:

```csharp
GetByIdAsync(projectId);
```

Exemplo esperado:

```csharp
GetByIdAsync(organizationId, projectId);
```

Filtros globais do EF Core poderão ser utilizados, mas não substituirão testes e políticas explícitas de isolamento.

---

# 15. Persistência

O PostgreSQL será o banco principal.

O EF Core será usado para:

- Escrita.
    
- Aggregates.
    
- Unit of Work.
    
- Migrations.
    
- Controle de concorrência.
    
- Transações.
    

Queries complexas poderão utilizar:

- LINQ otimizado.
    
- SQL direto.
    
- Projeções.
    
- Dapper, caso seja justificado futuramente.
    

Não será criado um repository genérico universal.

Repositories serão definidos quando representarem uma abstração real do domínio ou do caso de uso.

---

# 16. Mensageria

RabbitMQ será utilizado para comunicação assíncrona.

Tipos principais de mensagens:

- Commands distribuídos.
    
- Integration Events.
    
- Jobs.
    
- Solicitações de processamento.
    
- Notificações de conclusão.
    

Toda mensagem deverá utilizar um envelope padronizado.

Campos mínimos:

```text
MessageId
MessageType
MessageVersion
OccurredAt
CorrelationId
CausationId
TenantId
IdempotencyKey
Payload
Metadata
```

---

# 17. Cache

Redis será utilizado somente quando houver benefício mensurável.

Possíveis usos:

- Cache de leitura.
    
- Rate limiting distribuído.
    
- Locks distribuídos.
    
- Estado efêmero.
    
- Deduplicação temporária.
    
- Sessões técnicas.
    
- Dados de providers com baixa volatilidade.
    

Redis não será fonte primária da verdade para dados de negócio.

O MVP não dependerá de cache para funcionar corretamente.

---

# 18. Integrações com Inteligência Artificial

A Application dependerá de abstrações.

Exemplo:

```csharp
public interface ITextGenerationProvider
{
    Task<Result<TextGenerationResponse>> GenerateAsync(
        TextGenerationRequest request,
        CancellationToken cancellationToken);
}
```

Infrastructure conterá adapters concretos.

Exemplos:

```text
OpenAiTextGenerationProvider
AnthropicTextGenerationProvider
GeminiTextGenerationProvider
FakeTextGenerationProvider
```

A lógica de negócio não deverá depender de tipos específicos de SDK.

---

# 19. Structured Outputs

Sempre que possível, resultados de IA utilizados pelo sistema deverão seguir schemas estruturados.

Exemplo:

```json
{
  "title": "Como criar conteúdo com IA",
  "summary": "Resumo do conteúdo",
  "sections": [
    {
      "heading": "Introdução",
      "keyPoints": [
        "Ponto A",
        "Ponto B"
      ]
    }
  ]
}
```

O backend deverá:

- Validar o schema.
    
- Rejeitar respostas inválidas.
    
- Registrar a versão do schema.
    
- Registrar o prompt utilizado.
    
- Registrar o modelo.
    
- Registrar o provider.
    
- Registrar parâmetros relevantes.
    

---

# 20. Arquivos e Storage

Arquivos binários não deverão ser armazenados diretamente no PostgreSQL, exceto pequenos documentos quando houver justificativa.

Azure Blob Storage será utilizado para:

- Imagens.
    
- Áudio.
    
- Vídeo.
    
- Documentos grandes.
    
- Exportações.
    
- Artefatos intermediários.
    

O banco armazenará metadados como:

```text
ArtifactId
StorageProvider
Container
ObjectKey
ContentType
Size
Checksum
CreatedAt
```

---

# 21. Observabilidade

O backend adotará telemetria estruturada.

## Logs

Logs deverão ser:

- Estruturados.
    
- Pesquisáveis.
    
- Correlacionados.
    
- Sem dados sensíveis.
    
- Sem prompts completos quando houver informação privada.
    
- Sem secrets.
    

---

## Métricas

Métricas iniciais:

- Requisições HTTP.
    
- Duração de casos de uso.
    
- Erros por tipo.
    
- Mensagens publicadas.
    
- Mensagens consumidas.
    
- Falhas de consumers.
    
- Tamanho das filas.
    
- Duração de etapas.
    
- Execuções concluídas.
    
- Execuções falhadas.
    
- Tokens consumidos.
    
- Custos estimados.
    
- Latência por provider.
    

---

## Traces

Traces deverão acompanhar:

```text
HTTP
  → Application
    → PostgreSQL
    → Outbox
      → RabbitMQ
        → Worker
          → Provider de IA
          → Storage
```

---

# 22. Configuração

As aplicações utilizarão Options Pattern.

Exemplos:

```text
DatabaseOptions
RabbitMqOptions
RedisOptions
OpenAiOptions
AzureStorageOptions
PipelineOptions
```

Configurações obrigatórias deverão ser validadas no startup.

O acesso indiscriminado ao `IConfiguration` dentro de serviços será proibido.

---

# 23. Health Checks

A API e o Worker deverão expor verificações de saúde.

Tipos:

## Liveness

Indica se o processo está vivo.

Não deverá depender de serviços externos.

---

## Readiness

Indica se o processo está pronto para receber trabalho.

Poderá verificar:

- PostgreSQL.
    
- RabbitMQ.
    
- Configuração essencial.
    
- Dependências obrigatórias.
    

Providers de IA opcionais não deverão necessariamente tornar toda a aplicação indisponível.

---

# 24. Estratégia de Testes do Backend

O backend deverá possuir:

- Testes unitários do Domain.
    
- Testes unitários da Application.
    
- Testes de componentes.
    
- Testes de integração com PostgreSQL.
    
- Testes da API.
    
- Testes de mensageria.
    
- Testes de arquitetura.
    
- Testes de providers.
    
- Testes de pipeline.
    
- Testes de contratos.
    

Casos de uso críticos deverão ser testados sem depender de serviços externos reais.

---

# 25. Segurança

Regras mínimas:

- Secrets fora do código.
    
- Logs sem credenciais.
    
- Validação de arquivos.
    
- Limite de tamanho de uploads.
    
- Proteção contra IDOR.
    
- Autorização por tenant.
    
- Rate limiting.
    
- Timeouts em chamadas externas.
    
- Validação de URLs.
    
- Proteção contra SSRF.
    
- Sanitização de conteúdo.
    
- Controle de permissões.
    
- Auditoria de ações críticas.
    
- Validação de callbacks.
    
- Assinatura de webhooks.
    

---

# 26. Convenções Iniciais

## Datas

Utilizar UTC internamente.

Tipos preferidos:

```csharp
DateTimeOffset
```

Evitar `DateTime.Now`.

Utilizar uma abstração temporal:

```csharp
IClock
```

---

## Identificadores

Identificadores deverão ser gerados pela aplicação.

Poderão ser utilizados:

- UUID v7.
    
- ULID.
    

A escolha deverá favorecer:

- Ordenação temporal.
    
- Geração distribuída.
    
- Baixa fragmentação de índices.
    

---

## Cancelamento

Todo método assíncrono relevante deverá aceitar `CancellationToken`.

O token deverá ser propagado para:

- EF Core.
    
- HTTP clients.
    
- Providers.
    
- Mensageria.
    
- Storage.
    
- Delays.
    
- Processos externos.
    

---

## Métodos assíncronos

Operações de I/O deverão ser assíncronas.

Não será permitido bloquear tarefas com:

```csharp
.Result
.Wait()
.GetAwaiter().GetResult()
```

salvo em pontos de bootstrap tecnicamente justificados.

---

## Nullable Reference Types

Nullable Reference Types deverão permanecer habilitados.

Warnings não deverão ser ignorados de forma indiscriminada.

---

## Records

Records poderão ser utilizados para:

- Commands.
    
- Queries.
    
- DTOs.
    
- Resultados imutáveis.
    
- Contratos.
    

Entidades de domínio não deverão ser records por padrão.

---

# 27. Escopo do MVP

O backend inicial deverá implementar apenas o fluxo necessário para validar a proposta do produto.

## Fluxo do MVP

```text
Criar projeto
    ↓
Criar ou selecionar pipeline
    ↓
Iniciar execução
    ↓
Executar pesquisa
    ↓
Gerar roteiro
    ↓
Salvar artefatos
    ↓
Consultar resultado
```

---

## Módulos necessários

```text
Projects
Pipelines
Executions
Artifacts
```

Approvals e Publications poderão ser introduzidos depois do fluxo principal funcionar.

---

## Componentes necessários

- API HTTP.
    
- Application.
    
- Domain.
    
- PostgreSQL.
    
- EF Core.
    
- Um Worker.
    
- RabbitMQ.
    
- Um provider de IA.
    
- Fake provider para testes.
    
- Logs estruturados.
    
- Health checks básicos.
    

---

## Componentes adiáveis

Os seguintes recursos não serão obrigatórios na primeira entrega:

- Redis.
    
- n8n.
    
- Múltiplos providers.
    
- Circuit breaker sofisticado.
    
- Dashboards completos.
    
- Aprovações avançadas.
    
- Publicação em redes sociais.
    
- Serviço Python separado.
    
- Processamento de vídeo.
    
- Processamento de áudio.
    
- RAG.
    
- Vector database.
    
- Feature flags avançadas.
    
- Escalabilidade automática.
    
- Multi-região.
    
- Microservices.
    

---

# 28. Sequência Recomendada de Implementação

## Etapa 1 — Fundação

- Criar solution.
    
- Criar projetos.
    
- Configurar referências.
    
- Configurar injeção de dependência.
    
- Configurar Result Pattern.
    
- Configurar tratamento global de erros.
    
- Configurar logs.
    
- Configurar health checks.
    

---

## Etapa 2 — Projects

- Modelar Project.
    
- Criar migration.
    
- Implementar CreateProject.
    
- Implementar GetProject.
    
- Criar endpoints.
    
- Criar testes.
    

---

## Etapa 3 — Pipelines

- Modelar Pipeline.
    
- Modelar PipelineDefinition.
    
- Modelar PipelineStep.
    
- Criar pipeline inicial.
    
- Implementar consultas.
    

---

## Etapa 4 — Executions

- Modelar PipelineExecution.
    
- Modelar StepExecution.
    
- Implementar StartPipeline.
    
- Persistir estado Queued.
    
- Criar mensagem de início.
    

---

## Etapa 5 — Worker

- Configurar RabbitMQ.
    
- Criar consumer.
    
- Carregar execução.
    
- Executar primeira etapa.
    
- Persistir checkpoint.
    
- Encadear próxima etapa.
    

---

## Etapa 6 — IA

- Criar abstração de provider.
    
- Criar Fake Provider.
    
- Criar provider real.
    
- Implementar Research Step.
    
- Implementar Script Step.
    
- Validar Structured Output.
    

---

## Etapa 7 — Consulta e acompanhamento

- Consultar execução.
    
- Consultar etapas.
    
- Consultar artefatos.
    
- Expor status pela API.
    
- Adicionar logs e métricas.
    

---

# 29. Definition of Done da Fundação do Backend

A fundação será considerada pronta quando:

- A solution compilar.
    
- Dependências arquiteturais estiverem corretas.
    
- A API iniciar.
    
- O Worker iniciar.
    
- PostgreSQL estiver configurado.
    
- Migrations funcionarem.
    
- RabbitMQ estiver configurado.
    
- Um endpoint criar um projeto.
    
- Um pipeline puder ser iniciado.
    
- O Worker processar uma mensagem.
    
- Uma execução puder ser consultada.
    
- Testes automatizados rodarem no CI.
    
- Logs possuírem correlação.
    
- Health checks estiverem disponíveis.
    
- Configurações obrigatórias forem validadas.
    
- Nenhum secret estiver no repositório.
    

---

# 30. Regras Arquiteturais

1. Domain não depende de tecnologias externas.
    
2. Application não depende de Data.
    
3. Application não depende de Infrastructure.
    
4. Data não depende de Infrastructure.
    
5. Infrastructure não contém DbContext.
    
6. Api não contém regra de negócio.
    
7. Worker não contém regra de negócio.
    
8. Consumers delegam para a Application.
    
9. Endpoints delegam para a Application.
    
10. Entidades não são expostas diretamente pela API.
    
11. Entidades não são publicadas como mensagens.
    
12. Toda operação longa é assíncrona.
    
13. Toda mensagem deve ser idempotente.
    
14. Toda chamada externa deve possuir timeout.
    
15. Toda operação assíncrona deve propagar CancellationToken.
    
16. Toda operação tenant-scoped deve validar o tenant.
    
17. Toda alteração relevante deve ser observável.
    
18. Toda configuração obrigatória deve ser validada no startup.
    
19. Toda nova feature deve possuir testes proporcionais ao risco.
    
20. Toda abstração deve representar um limite real.
    

---

# 31. Fluxo Completo do MVP

```text
Usuário
    │
    ▼
POST /api/v1/projects
    │
    ▼
CreateProjectCommand
    │
    ▼
Project criado no PostgreSQL
    │
    ▼
POST /api/v1/pipelines/{pipelineId}/executions
    │
    ▼
StartPipelineCommand
    │
    ├── Cria PipelineExecution
    ├── Define status Queued
    ├── Cria OutboxMessage
    └── Commit
    │
    ▼
Outbox Worker
    │
    ▼
RabbitMQ
    │
    ▼
Pipeline Worker
    │
    ├── Executa Research Step
    ├── Chama provider de IA
    ├── Salva Research Artifact
    ├── Cria checkpoint
    └── Agenda Script Step
    │
    ▼
Pipeline Worker
    │
    ├── Executa Script Step
    ├── Chama provider de IA
    ├── Salva Script Artifact
    ├── Atualiza execução
    └── Define status Completed
    │
    ▼
GET /api/v1/executions/{executionId}
    │
    ▼
Usuário visualiza roteiro
```

---

# 32. Decisões que Serão Refinadas Durante a Implementação

Alguns detalhes serão definidos com base no aprendizado obtido durante o desenvolvimento:

- Estrutura final dos Aggregates.
    
- Granularidade dos Pipeline Steps.
    
- Formato exato de checkpoints.
    
- Estratégia de concorrência.
    
- Granularidade dos repositories.
    
- Necessidade real de Redis.
    
- Estratégia de versionamento dos prompts.
    
- Divisão futura dos Workers.
    
- Necessidade de serviço Python.
    
- Modelo de aprovação.
    
- Estratégia de publicação.
    

Essas decisões não deverão bloquear o início do MVP.

---

# 33. Filosofia Final

O backend do Infinite Content AI deverá ser sólido, mas não excessivamente antecipado.

A arquitetura deverá proteger os limites essenciais:

- Domínio.
    
- Casos de uso.
    
- Persistência.
    
- Integrações.
    
- Processamento assíncrono.
    

Ao mesmo tempo, a equipe deverá evitar implementar capacidades enterprise que ainda não possuem uma necessidade concreta.

A regra principal será:

> Construir o menor backend que preserve os limites necessários para evoluir com segurança.

A arquitetura serve para acelerar a evolução do produto.

Ela não deverá se tornar um obstáculo para colocar o produto em funcionamento.