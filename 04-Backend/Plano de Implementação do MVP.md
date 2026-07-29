# Plano de Implementação do MVP

## 1. Objetivo

Este documento define o plano de implementação do MVP do Infinite Content AI.

O objetivo do MVP será validar o fluxo central de geração de conteúdo:

```text
Criar Project
    ↓
Criar Pipeline
    ↓
Iniciar PipelineExecution
    ↓
Executar Research Step
    ↓
Gerar Research Artifact
    ↓
Executar Script Step
    ↓
Gerar Script Artifact
    ↓
Concluir PipelineExecution
```

O MVP deverá provar que a arquitetura consegue:

- Receber uma solicitação pela API.
    
- Aplicar casos de uso pela Application.
    
- Proteger regras pelo Domain.
    
- Persistir dados no PostgreSQL.
    
- Executar processamento assíncrono.
    
- Chamar um provider de inteligência artificial.
    
- Gerar Artifacts.
    
- Recuperar o estado de uma execução.
    
- Tratar falhas previsíveis.
    
- Ser testado sem providers reais.
    

O MVP não deverá tentar implementar todas as capacidades futuras da plataforma.

---

# 2. Princípio de Implementação

A prioridade será construir um fluxo vertical completo antes de aprofundar componentes isolados.

Preferir:

```text
API
    ↓
Application
    ↓
Domain
    ↓
Data
    ↓
Resposta funcional
```

antes de implementar antecipadamente:

- Cache avançado.
    
- Multi-provider.
    
- Aprovações.
    
- Publicação externa.
    
- n8n.
    
- Observabilidade avançada.
    
- Retries complexos.
    
- Multi-região.
    
- Billing.
    
- Dashboard administrativo.
    

A regra principal será:

> Primeiro fazer o fluxo funcionar de ponta a ponta; depois aumentar sua confiabilidade, escalabilidade e sofisticação.

---

# 3. Estratégia em Dois Marcos

Para acelerar o desenvolvimento, o MVP será dividido em dois marcos.

## Marco 1 — MVP Funcional

Fluxo executado com:

- API.
    
- Application.
    
- Domain.
    
- Data.
    
- PostgreSQL.
    
- Fake AI Provider.
    
- Orquestração simples.
    

Objetivo:

> Validar o modelo, os casos de uso e a geração dos Artifacts sem depender inicialmente de RabbitMQ.

## Marco 2 — MVP Assíncrono

Adicionar:

- Worker.
    
- RabbitMQ.
    
- Outbox.
    
- Inbox básica.
    
- Consumers.
    
- Retry básico.
    
- Recuperação simples.
    

Objetivo:

> Transformar o fluxo funcional em um processamento assíncrono e recuperável.

O Marco 1 não será descartado.

Ele servirá como base para o Marco 2.

---

# 4. Escopo Funcional do MVP

O MVP deverá permitir:

## Projects

- Criar Project.
    
- Consultar Project.
    
- Listar Projects.
    

## Pipelines

- Criar Pipeline.
    
- Adicionar Research Step.
    
- Adicionar Script Step.
    
- Publicar Pipeline.
    
- Consultar Pipeline.
    

## Executions

- Iniciar PipelineExecution.
    
- Consultar estado.
    
- Cancelar execução, quando ainda não terminal.
    
- Visualizar etapas.
    
- Visualizar falha.
    

## Artifacts

- Criar Research Artifact.
    
- Criar Script Artifact.
    
- Consultar Artifact.
    
- Listar Artifacts de uma execução.
    

## Inteligência Artificial

- Executar Fake Provider.
    
- Executar um provider real após o fluxo estar estável.
    
- Registrar provider e modelo utilizados.
    
- Validar Structured Output básico.
    

---

# 5. Fora do Escopo

Não será implementado antes da conclusão do fluxo principal:

- Approval completo.
    
- Publication completo.
    
- YouTube.
    
- WordPress.
    
- LinkedIn.
    
- TikTok.
    
- Redis.
    
- n8n.
    
- Serviços Python.
    
- Geração de vídeo.
    
- Geração de áudio.
    
- Geração de imagem.
    
- Multi-provider automático.
    
- Provider fallback.
    
- Billing.
    
- Quotas avançadas.
    
- Feature Flags avançadas.
    
- Busca vetorial.
    
- RAG completo.
    
- Workflow visual.
    
- Dashboard operacional.
    
- Multi-região.
    
- Particionamento de banco.
    
- Scheduler avançado.
    
- WebSockets.
    
- Server-Sent Events.
    
- Webhooks de saída.
    
- Replay administrativo completo.
    
- Exactly-once processing.
    

Esses recursos poderão ser adicionados depois de o fluxo principal estar funcional.

---

# 6. Arquitetura Inicial

Projetos da solution:

```text
src/
├── Api
├── Application
├── Domain
├── Data
├── Infrastructure
├── Worker
├── Contracts
└── SharedKernel
```

Testes:

```text
tests/
├── ArchitectureTests
├── SharedKernel.UnitTests
├── Domain.UnitTests
├── Application.UnitTests
├── Data.IntegrationTests
├── Infrastructure.IntegrationTests
├── Api.IntegrationTests
└── Worker.IntegrationTests
```

Nem todos os projetos de teste precisam nascer no primeiro commit.

Prioridade inicial:

```text
SharedKernel.UnitTests
Domain.UnitTests
Data.IntegrationTests
Api.IntegrationTests
```

---

# 7. Referências entre Projetos

Referências permitidas:

```text
Domain
    → SharedKernel
```

```text
Application
    → Domain
    → Contracts
    → SharedKernel
```

```text
Data
    → Application
    → Domain
    → SharedKernel
```

```text
Infrastructure
    → Application
    → Domain
    → Contracts
    → SharedKernel
```

```text
Api
    → Application
    → Data
    → Infrastructure
    → Contracts
    → SharedKernel
```

```text
Worker
    → Application
    → Data
    → Infrastructure
    → Contracts
    → SharedKernel
```

```text
Contracts
    → nenhuma dependência interna
```

---

# 8. Regras que Não Podem Ser Quebradas

1. Domain não depende de Application.
    
2. Domain não depende de EF Core.
    
3. Domain não depende de RabbitMQ.
    
4. Application não conhece DbContext.
    
5. Application não conhece SDK de IA.
    
6. Data não chama APIs externas.
    
7. Infrastructure não possui DbContext.
    
8. Worker não implementa regra de negócio.
    
9. API não acessa banco diretamente.
    
10. Contracts não depende de outros projetos.
    
11. Shared Kernel permanece pequeno.
    
12. OrganizationId participa de todas as operações tenant-scoped.
    

---

# 9. Fase 0 — Preparação da Solution

## Objetivo

Criar a estrutura mínima para começar a implementar.

## Tarefas

-  Criar solution.
    
-  Criar os oito projetos.
    
-  Criar os projetos de testes prioritários.
    
-  Configurar referências.
    
-  Configurar nullable reference types.
    
-  Configurar implicit usings.
    
-  Configurar warnings as errors.
    
-  Configurar `.editorconfig`.
    
-  Configurar Directory.Build.props.
    
-  Configurar formatação.
    
-  Configurar análise estática básica.
    
-  Criar estrutura inicial de pastas.
    
-  Confirmar que toda a solution compila.
    

## Entregável

```text
dotnet build
```

deverá terminar com sucesso.

## Definition of Done

- A solution compila.
    
- As referências respeitam a arquitetura.
    
- Nenhum projeto contém implementação de negócio ainda.
    
- Os testes vazios executam.
    

---

# 10. Fase 1 — Shared Kernel

## Objetivo

Criar as primitivas técnicas utilizadas pelo restante da solution.

## Implementar

-  `ErrorType`.
    
-  `Error`.
    
-  `Result`.
    
-  `Result<T>`.
    
-  `Match`.
    
-  `Entity<TId>`.
    
-  `AggregateRoot<TId>`.
    
-  `IDomainEvent`.
    
-  `IClock`.
    
-  `PaginatedResult<T>`.
    

## Testes

-  Result de sucesso.
    
-  Result de falha.
    
-  Result com valor.
    
-  Acesso inválido ao Value.
    
-  Match.
    
-  Igualdade de Entity.
    
-  Registro de Domain Event.
    
-  Limpeza de Domain Events.
    
-  Cálculo de TotalPages.
    

## Definition of Done

- SharedKernel não possui dependências internas.
    
- Todos os testes passam.
    
- Nenhum helper genérico foi criado.
    
- Nenhuma regra de feature está presente.
    

---

# 11. Fase 2 — Project Vertical Slice

## Objetivo

Implementar o primeiro fluxo completo:

```text
POST /api/v1/projects
    ↓
CreateProjectCommand
    ↓
Project.Create
    ↓
PostgreSQL
    ↓
201 Created
```

## Domain

Implementar:

```text
Project
ProjectId
ProjectName
ProjectStatus
ProjectErrors
ProjectCreatedDomainEvent
```

Regras:

- OrganizationId obrigatório.
    
- Nome obrigatório.
    
- Nome com tamanho máximo.
    
- Status inicial Active.
    
- Criador e data obrigatórios.
    
- OrganizationId imutável.
    

## Application

Implementar:

```text
CreateProjectCommand
CreateProjectHandler
CreateProjectValidator
CreateProjectResult
IProjectRepository
IProjectQueries
IUnitOfWork
```

## Data

Implementar:

```text
ApplicationDbContext
ProjectConfiguration
ProjectRepository
ProjectQueries
ApplicationDbContextFactory
Migration inicial
```

## API

Implementar:

```text
CreateProjectRequest
CreateProjectResponse
CreateProjectEndpoint
GetProjectEndpoint
ListProjectsEndpoint
Problem Details
Exception Handler
```

## Testes

-  Project válido.
    
-  Project inválido.
    
-  Persistência real no PostgreSQL.
    
-  Consulta por Organization.
    
-  API retorna 201.
    
-  API retorna 400.
    
-  API não acessa Project de outra Organization.
    

## Definition of Done

- Project pode ser criado pela API.
    
- Project permanece salvo após reiniciar a aplicação.
    
- Project pode ser consultado.
    
- Dados não vazam entre Organizations.
    
- Testes de integração passam.
    

---

# 12. Fase 3 — Pipeline

## Objetivo

Permitir configurar um pipeline linear de Research e Script.

## Domain

Implementar:

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

Tipos iniciais:

```text
research
script
```

Regras:

- Pipeline pertence a um Project.
    
- Pipeline pertence à mesma Organization.
    
- Pipeline inicia em Draft.
    
- Pipeline publicado deve possuir etapas.
    
- Posições não podem repetir.
    
- Research deve vir antes de Script.
    
- Pipeline publicado não é alterado diretamente.
    
- Publicação registra versão.
    

## Application

Implementar:

```text
CreatePipelineCommand
AddPipelineStepCommand
PublishPipelineCommand
GetPipelineQuery
ListProjectPipelinesQuery
IPipelineRepository
IPipelineQueries
```

## Data

Implementar:

```text
PipelineConfiguration
PipelineStepDefinitionConfiguration
PipelineRepository
PipelineQueries
Migration
```

Constraints:

```text
pipeline_id + position
pipeline_id + version
```

## API

Endpoints:

```text
POST /api/v1/projects/{projectId}/pipelines
POST /api/v1/pipelines/{pipelineId}/steps
POST /api/v1/pipelines/{pipelineId}/publish
GET  /api/v1/pipelines/{pipelineId}
```

## Testes

-  Criar Pipeline.
    
-  Adicionar Research.
    
-  Adicionar Script.
    
-  Rejeitar posição duplicada.
    
-  Rejeitar publicação sem Steps.
    
-  Publicar Pipeline válido.
    
-  Rejeitar acesso de outra Organization.
    

## Definition of Done

Um usuário consegue criar e publicar:

```text
Research
    ↓
Script
```

---

# 13. Fase 4 — PipelineExecution

## Objetivo

Criar uma execução persistida a partir de um Pipeline publicado.

## Domain

Implementar:

```text
PipelineExecution
PipelineExecutionId
PipelineExecutionStatus
StepExecution
StepExecutionId
StepExecutionStatus
ExecutionError
ExecutionErrors
```

Estados iniciais:

```text
Queued
Running
Completed
Failed
Cancelled
```

Estados das etapas:

```text
Pending
Running
Completed
Failed
Cancelled
```

## Application

Implementar:

```text
StartExecutionCommand
StartExecutionHandler
StartExecutionResult
GetExecutionQuery
CancelExecutionCommand
IPipelineExecutionRepository
IExecutionQueries
```

## Data

Implementar:

```text
PipelineExecutionConfiguration
StepExecutionConfiguration
PipelineExecutionRepository
ExecutionQueries
Migration
```

## API

Endpoints:

```text
POST /api/v1/pipelines/{pipelineId}/executions
GET  /api/v1/executions/{executionId}
POST /api/v1/executions/{executionId}/cancel
```

## Resposta inicial

```json
{
  "executionId": "019c...",
  "status": "queued",
  "requestedAt": "2026-07-28T15:00:00Z"
}
```

## Testes

-  Iniciar execução de Pipeline publicado.
    
-  Rejeitar Pipeline Draft.
    
-  Criar StepExecutions.
    
-  Registrar PipelineVersion.
    
-  Consultar estado.
    
-  Cancelar execução.
    
-  Rejeitar cancelamento após conclusão.
    

## Definition of Done

Uma execução pode ser criada, consultada e cancelada.

---

# 14. Fase 5 — Artifact

## Objetivo

Persistir os resultados gerados pelas etapas.

## Domain

Implementar:

```text
Artifact
ArtifactId
ArtifactType
ArtifactStatus
ArtifactContent
ArtifactErrors
ArtifactCreatedDomainEvent
```

Tipos iniciais:

```text
research
script
```

Conteúdo inicial:

```text
TextArtifactContent
StructuredArtifactContent
```

## Application

Implementar:

```text
CreateArtifact
GetArtifactQuery
ListExecutionArtifactsQuery
IArtifactRepository
IArtifactQueries
```

A criação normalmente ocorrerá durante a execução de um Step, não por endpoint público.

## Data

Implementar:

```text
ArtifactConfiguration
ArtifactRepository
ArtifactQueries
Migration
```

## API

Endpoints:

```text
GET /api/v1/executions/{executionId}/artifacts
GET /api/v1/artifacts/{artifactId}
```

## Testes

-  Criar Research Artifact.
    
-  Criar Script Artifact.
    
-  Vincular Artifact à execução.
    
-  Vincular Artifact à StepExecution.
    
-  Registrar versão.
    
-  Consultar Artifact.
    
-  Rejeitar consulta cross-tenant.
    

## Definition of Done

Research e Script podem ser salvos e consultados.

---

# 15. Fase 6 — Fake AI Provider

## Objetivo

Executar Research e Script sem custo ou dependência externa.

## Application

Definir:

```text
ITextGenerationProvider
ITextGenerationProviderResolver
TextGenerationRequest
TextGenerationResponse
TokenUsage
StructuredOutputSchema
IStructuredOutputValidator
```

## Infrastructure

Implementar:

```text
FakeTextGenerationProvider
FakeProviderOptions
FakeResponseFactory
TextGenerationProviderResolver
StructuredOutputValidator
```

## Respostas Fake

Research:

```json
{
  "summary": "Pesquisa simulada para o tema informado.",
  "keyPoints": [
    "Ponto principal 1",
    "Ponto principal 2"
  ],
  "sources": []
}
```

Script:

```json
{
  "title": "Título gerado pelo provider fake",
  "hook": "Introdução do roteiro.",
  "sections": [
    {
      "title": "Seção 1",
      "content": "Conteúdo da seção."
    }
  ],
  "conclusion": "Conclusão do roteiro."
}
```

## Modos de simulação

O Fake Provider deverá permitir simular:

-  Sucesso.
    
-  Timeout.
    
-  Rate limit.
    
-  Resposta inválida.
    
-  Falha permanente.
    
-  Cancelamento.
    

## Definition of Done

Research e Script podem ser executados deterministicamente em testes e desenvolvimento local.

---

# 16. Fase 7 — Orquestração Funcional

## Objetivo

Concluir o Marco 1 executando o fluxo sem RabbitMQ.

Fluxo:

```text
StartExecution
    ↓
Execution = Running
    ↓
Executar ResearchStepHandler
    ↓
Salvar Research Artifact
    ↓
Executar ScriptStepHandler
    ↓
Salvar Script Artifact
    ↓
Execution = Completed
```

## Componentes

```text
IPipelineStepHandler
IPipelineStepHandlerResolver
ResearchStepHandler
ScriptStepHandler
PipelineExecutionOrchestrator
```

## Estratégia

Criar um caso de uso interno:

```text
ExecutePipelineCommand
```

Ele poderá ser acionado manualmente em desenvolvimento ou imediatamente após `StartExecution`.

Essa implementação será substituída pelo disparo assíncrono no Marco 2.

## Regras

- Cada Step deve persistir seu estado.
    
- Research deve gerar Artifact.
    
- Script deve usar Research Artifact.
    
- Falhas devem marcar Step e Execution.
    
- CancellationToken deve ser propagado.
    
- Provider e modelo devem ser registrados.
    

## Teste end-to-end funcional

```text
Criar Project
Criar Pipeline
Adicionar Research
Adicionar Script
Publicar Pipeline
Iniciar Execution
Executar Pipeline
Consultar Execution
Consultar Artifacts
```

## Definition of Done do Marco 1

- O fluxo completo funciona.
    
- A execução termina como Completed.
    
- Existem dois Artifacts.
    
- Script utiliza Research como input.
    
- Falhas são persistidas.
    
- O sistema funciona com Fake Provider.
    
- Nenhum RabbitMQ é necessário para esse teste.
    

---

# 17. Fase 8 — Contracts

## Objetivo

Criar os contratos necessários para distribuir o fluxo.

Implementar:

```text
MessageEnvelope
MessageMetadata
MessageTypes
PipelineExecutionRequestedV1
PipelineStepExecutionRequestedV1
PipelineExecutionCompletedV1
PipelineExecutionFailedV1
ArtifactGeneratedV1
```

## Testes

-  Round-trip JSON.
    
-  Exemplos JSON.
    
-  MessageType correto.
    
-  MessageVersion correta.
    
-  Campos obrigatórios.
    
-  OrganizationId consistente.
    

## Definition of Done

Contracts compila sem depender de nenhum projeto interno.

---

# 18. Fase 9 — Outbox

## Objetivo

Registrar mensagens no mesmo commit das alterações de negócio.

## Data

Implementar:

```text
OutboxMessage
OutboxMessageConfiguration
IOutbox implementation
Outbox mapper
Outbox claim queries
```

Campos essenciais:

```text
Id
MessageType
MessageVersion
Payload
OrganizationId
CorrelationId
CausationId
OccurredAt
ProcessedAt
AttemptCount
NextAttemptAt
LastError
```

## Fluxo

```text
Criar PipelineExecution
    ↓
Registrar PipelineExecutionRequestedV1
    ↓
Salvar execução e Outbox
    ↓
Mesmo commit
```

## Testes

-  Execution e Outbox são salvas juntas.
    
-  Falha no commit não salva nenhuma das duas.
    
-  Payload é serializado.
    
-  MessageId é estável.
    
-  OrganizationId é preservado.
    

## Definition of Done

Nenhuma mensagem necessária é criada fora da transação da alteração que a originou.

---

# 19. Fase 10 — RabbitMQ

## Objetivo

Publicar mensagens da Outbox no broker.

## Infrastructure

Implementar:

```text
RabbitMqOptions
RabbitMqConnection
RabbitMqChannelFactory
RabbitMqMessagePublisher
RabbitMqMessageSerializer
RabbitMqTopologyInitializer
RabbitMqHealthCheck
```

## Exchanges

```text
infinite-content.commands
infinite-content.events
infinite-content.dead-letter
```

## Filas

```text
infinite-content.pipeline.execution
infinite-content.pipeline.steps
```

## Testes

-  Declarar topology.
    
-  Publicar mensagem.
    
-  Confirmar publicação.
    
-  Validar headers.
    
-  Validar routing key.
    
-  Validar serialização.
    

## Definition of Done

Uma OutboxMessage pode ser publicada e recebida no RabbitMQ local.

---

# 20. Fase 11 — Worker e Outbox Publisher

## Objetivo

Criar o host de processamento assíncrono.

## Worker

Implementar:

```text
Program.cs
DependencyInjection
WorkerOptions
OutboxPublisherService
Health checks
Graceful shutdown
```

## Fluxo

```text
Outbox pendente
    ↓
Claim
    ↓
Publicar RabbitMQ
    ↓
Publisher Confirm
    ↓
Marcar como Processed
```

## Testes

-  Publicar lote.
    
-  Não publicar processadas.
    
-  Repetir após falha.
    
-  Preservar MessageId.
    
-  Múltiplos Workers não processam o mesmo claim.
    
-  Shutdown interrompe polling.
    

## Definition of Done

O Worker publica automaticamente mensagens pendentes da Outbox.

---

# 21. Fase 12 — Consumers

## Objetivo

Consumir as mensagens do fluxo de pipeline.

Implementar:

```text
MessageContext
IMessageConsumer<T>
ConsumerResult
PipelineExecutionRequestedConsumer
PipelineStepExecutionRequestedConsumer
```

Fluxo:

```text
PipelineExecutionRequestedV1
    ↓
ExecutePipelineCommand
    ↓
Solicitar Research Step
```

```text
PipelineStepExecutionRequestedV1
    ↓
ExecutePipelineStepCommand
    ↓
Executar Research ou Script
```

## Ack

Ack somente após:

- Application concluir.
    
- Alterações serem persistidas.
    
- Inbox ser concluída.
    

## Definition of Done

O fluxo funcional passa a ser acionado pelo RabbitMQ em vez de execução direta.

---

# 22. Fase 13 — Inbox

## Objetivo

Impedir efeitos duplicados durante redelivery.

## Data

Implementar:

```text
InboxMessage
InboxMessageConfiguration
IInbox implementation
Unique constraint por ConsumerName + MessageId
```

## Worker

Fluxo:

```text
Receber mensagem
    ↓
Registrar Inbox
    ↓
Executar Command
    ↓
Marcar Inbox Processed
    ↓
Commit
    ↓
Ack
```

## Testes críticos

-  Redelivery não duplica Execution.
    
-  Redelivery não duplica Artifact.
    
-  Falha após commit e antes de Ack não duplica efeito.
    
-  Consumers diferentes podem processar o mesmo Event.
    
-  Mensagem concluída retorna Ack.
    

## Definition of Done

A mesma mensagem pode ser entregue mais de uma vez sem repetir efeitos de negócio.

---

# 23. Fase 14 — Retry Básico

## Objetivo

Tratar falhas transitórias sem criar uma plataforma de retries completa.

## Classificação

Retry:

```text
Timeout
RateLimit
Unavailable
Falha de conexão
HTTP 502
HTTP 503
HTTP 504
```

Dead Letter:

```text
Payload inválido
Versão desconhecida
Validation permanente
Modelo inexistente
Configuração inválida
Structured Output continuamente inválido
```

## Agenda inicial

```text
Tentativa 1: imediata
Tentativa 2: 30 segundos
Tentativa 3: 2 minutos
Tentativa 4: 10 minutos
```

Para o MVP, quatro tentativas serão suficientes.

## Definition of Done

Falhas transitórias são repetidas com atraso e falhas permanentes não entram em loop.

---

# 24. Fase 15 — Recovery Básico

## Objetivo

Recuperar Steps que ficaram em Running após falha do Worker.

Campos necessários:

```text
AttemptId
AttemptNumber
StartedAt
LeaseUntil
```

Fluxo:

```text
Step = Running
LeaseUntil expirado
    ↓
ExecutionRecoveryService
    ↓
Reagendar ou falhar
```

## Regras iniciais

- Research e Script podem ser repetidos.
    
- AttemptNumber deve ser incrementado.
    
- Novo AttemptId deve ser gerado.
    
- Resultado atrasado com AttemptId antigo deve ser rejeitado.
    
- MaximumAttempts deve ser respeitado.
    

## Definition of Done

Uma reinicialização do Worker não deixa a execução permanentemente travada.

---

# 25. Fase 16 — Provider Real

## Objetivo

Substituir o Fake Provider por um provider real sem alterar Application ou Domain.

Implementação inicial recomendada:

```text
OpenAiTextGenerationProvider
```

Implementar:

-  Options.
    
-  Secret resolution.
    
-  Typed HttpClient ou SDK encapsulado.
    
-  Request mapper.
    
-  Response mapper.
    
-  Error mapper.
    
-  Token usage.
    
-  Timeout.
    
-  Retry técnico curto.
    
-  Structured Output.
    
-  Health check leve.
    

## Estratégia

Manter configuração:

```text
ArtificialIntelligence:DefaultProvider
```

Valores:

```text
fake
openai
```

## Definition of Done

O mesmo fluxo do MVP funciona com Fake e com provider real apenas mudando configuração.

---

# 26. Endpoints Finais do MVP

## Projects

```text
POST /api/v1/projects
GET  /api/v1/projects
GET  /api/v1/projects/{projectId}
```

## Pipelines

```text
POST /api/v1/projects/{projectId}/pipelines
GET  /api/v1/projects/{projectId}/pipelines
GET  /api/v1/pipelines/{pipelineId}
POST /api/v1/pipelines/{pipelineId}/steps
POST /api/v1/pipelines/{pipelineId}/publish
```

## Executions

```text
POST /api/v1/pipelines/{pipelineId}/executions
GET  /api/v1/executions/{executionId}
POST /api/v1/executions/{executionId}/cancel
```

## Artifacts

```text
GET /api/v1/executions/{executionId}/artifacts
GET /api/v1/artifacts/{artifactId}
```

## Operational

```text
GET /health/live
GET /health/ready
```

---

# 27. Banco de Dados do MVP

Tabelas iniciais:

```text
projects
pipelines
pipeline_step_definitions
pipeline_executions
step_executions
artifacts
outbox_messages
inbox_messages
```

Tabela opcional:

```text
idempotency_records
```

Ela poderá ser adicionada depois que o primeiro endpoint crítico exigir Idempotency-Key persistente.

---

# 28. Estados do Fluxo

## Pipeline

```text
Draft
Published
Archived
```

Para o MVP, `Archived` poderá existir no modelo sem endpoint.

## PipelineExecution

```text
Queued
Running
Completed
Failed
Cancelled
```

## StepExecution

```text
Pending
Running
Completed
Failed
Cancelled
```

## Artifact

```text
Generated
Archived
```

Estados adicionais deverão ser criados somente quando um fluxo real precisar deles.

---

# 29. Contratos de Structured Output

## Research

```json
{
  "summary": "string",
  "keyPoints": [
    "string"
  ],
  "sources": [
    {
      "title": "string",
      "url": "string"
    }
  ]
}
```

## Script

```json
{
  "title": "string",
  "hook": "string",
  "sections": [
    {
      "title": "string",
      "content": "string"
    }
  ],
  "conclusion": "string"
}
```

Schemas deverão possuir versão:

```text
research-output-v1
script-output-v1
```

---

# 30. Dados Mínimos de Reprodutibilidade

Cada Artifact gerado deverá registrar:

```text
PipelineId
PipelineVersion
ExecutionId
StepExecutionId
ArtifactType
ArtifactVersion
PromptVersion
Provider
Model
StructuredOutputSchemaVersion
CreatedAt
```

Token usage poderá ser registrado quando disponível.

O MVP não precisa guardar todos os parâmetros possíveis do provider.

---

# 31. Configuração Local

Dependências locais:

```text
PostgreSQL
RabbitMQ
Azurite, apenas se Storage for utilizado
```

Redis não será necessário.

Exemplo de Docker Compose:

```text
postgres
rabbitmq
```

A API e o Worker poderão executar localmente pelo `dotnet run`.

---

# 32. Configurações Necessárias

## Data

```text
Data:ConnectionString
Data:CommandTimeoutSeconds
```

## RabbitMQ

```text
Messaging:RabbitMQ:HostName
Messaging:RabbitMQ:Port
Messaging:RabbitMQ:VirtualHost
Messaging:RabbitMQ:Username
Messaging:RabbitMQ:Password
```

Em produção, usuário e senha deverão vir de secrets.

## Artificial Intelligence

```text
ArtificialIntelligence:DefaultProvider
ArtificialIntelligence:Fake:Enabled
ArtificialIntelligence:OpenAI:BaseUrl
ArtificialIntelligence:OpenAI:ApiKeySecretName
ArtificialIntelligence:OpenAI:DefaultModel
```

## Worker

```text
Worker:MaximumConcurrentMessages
Worker:OutboxBatchSize
Worker:ConsumerMaximumAttempts
Worker:RecoveryIntervalSeconds
```

---

# 33. Migrações

Migrações iniciais sugeridas:

```text
InitialProjects
AddPipelines
AddPipelineExecutions
AddArtifacts
AddOutbox
AddInbox
```

Durante desenvolvimento inicial, algumas poderão ser agrupadas.

Antes do primeiro deploy compartilhado, migrations já aplicadas não deverão ser reescritas.

---

# 34. Estratégia de Testes

## Shared Kernel

Testes unitários puros.

## Domain

Testes unitários de regras e estados.

## Application

Testes unitários de Handlers com fakes.

## Data

Testes de integração com PostgreSQL real.

## Infrastructure

Testes de contrato e adapters.

## API

Testes de integração com WebApplicationFactory.

## Worker

Testes de integração com RabbitMQ e PostgreSQL.

## End-to-End

Executar o fluxo completo com Fake Provider.

---

# 35. Testes Obrigatórios antes do MVP

## Project

-  Criar Project.
    
-  Consultar Project.
    
-  Isolamento por Organization.
    

## Pipeline

-  Criar Pipeline.
    
-  Adicionar Steps.
    
-  Publicar.
    
-  Rejeitar Pipeline inválido.
    

## Execution

-  Iniciar.
    
-  Executar Research.
    
-  Executar Script.
    
-  Concluir.
    
-  Falhar.
    
-  Cancelar.
    

## Artifact

-  Criar Research Artifact.
    
-  Criar Script Artifact.
    
-  Consultar por execução.
    

## Mensageria

-  Outbox.
    
-  Publicação.
    
-  Consumo.
    
-  Inbox.
    
-  Redelivery.
    
-  Retry.
    
-  Dead Letter.
    

## Recuperação

-  Worker morre durante Step.
    
-  Lease expira.
    
-  Step é reagendada.
    
-  Resultado atrasado é rejeitado.
    

---

# 36. Cenário End-to-End Principal

## Preparação

1. Criar Organization de teste.
    
2. Criar usuário de teste.
    
3. Criar Project.
    
4. Criar Pipeline.
    
5. Adicionar Research.
    
6. Adicionar Script.
    
7. Publicar Pipeline.
    

## Execução

1. Chamar endpoint de Start Execution.
    
2. Receber `202 Accepted`.
    
3. Confirmar Execution `Queued`.
    
4. Outbox publicar mensagem.
    
5. Worker iniciar Execution.
    
6. Research Step executar.
    
7. Research Artifact ser persistido.
    
8. Script Step executar.
    
9. Script Artifact ser persistido.
    
10. Execution finalizar como `Completed`.
    

## Validação

```text
Execution.Status = Completed
Research.Status = Completed
Script.Status = Completed
Artifacts.Count = 2
```

---

# 37. Cenários de Falha Obrigatórios

## Provider indisponível

Resultado:

```text
Step permanece recuperável
Retry é agendado
Execution não é concluída
```

## Structured Output inválido

Resultado:

```text
Retry limitado
Falha terminal após máximo
ErrorCode persistido
```

## Worker interrompido

Resultado:

```text
Lease expira
Recovery reagenda
Execution continua
```

## Mensagem duplicada

Resultado:

```text
Inbox detecta duplicidade
Artifact não duplica
Step não executa novamente
```

## Pipeline inválido

Resultado:

```text
StartExecution retorna falha
Nenhuma execução é criada
```

---

# 38. Observabilidade Mínima

O MVP deverá possuir logs estruturados com:

```text
CorrelationId
TraceId
OrganizationId
ProjectId
PipelineId
ExecutionId
StepExecutionId
StepType
Provider
Model
MessageId
AttemptNumber
ErrorCode
Duration
```

Métricas iniciais:

```text
pipeline.executions.started
pipeline.executions.completed
pipeline.executions.failed
pipeline.step.duration
pipeline.step.retries
outbox.pending.count
worker.messages.processed
worker.messages.failed
```

Não será necessário criar dashboards sofisticados antes de o fluxo funcionar.

---

# 39. Segurança Mínima

O MVP deverá possuir:

- Autenticação configurável.
    
- OrganizationId derivado do usuário autenticado.
    
- Autorização básica.
    
- Proteção contra IDOR.
    
- Secrets fora do código.
    
- Logs sem API keys.
    
- Payloads limitados.
    
- HTTPS em ambientes externos.
    
- CORS restrito.
    
- Queries tenant-scoped.
    
- Mensagens com OrganizationId.
    
- URLs externas validadas quando existirem.
    

Autenticação fake poderá ser utilizada apenas em desenvolvimento e testes.

---

# 40. Estratégia de Commits

Commits deverão ser pequenos e executáveis.

Exemplos:

```text
feat(shared-kernel): add result pattern
feat(projects): add project domain model
feat(projects): add create project use case
feat(data): persist projects with ef core
feat(api): expose create project endpoint
feat(pipelines): add pipeline domain model
feat(executions): queue pipeline execution
feat(ai): add fake text generation provider
feat(worker): publish outbox messages
```

Evitar commits gigantes contendo várias fases ao mesmo tempo.

---

# 41. Estratégia de Branches

Para desenvolvimento individual ou equipe pequena:

```text
main
feature/*
fix/*
```

Branches deverão possuir vida curta.

O código deverá ser integrado frequentemente.

O MVP não precisa de um processo de branches excessivamente complexo.

---

# 42. Pipeline de CI Inicial

Etapas:

```text
Restore
    ↓
Build
    ↓
Unit Tests
    ↓
Architecture Tests
    ↓
Integration Tests
    ↓
Publish Artifacts
```

Depois:

```text
Build Container API
Build Container Worker
```

Migrations poderão ser validadas em PostgreSQL de teste.

---

# 43. Containers

Imagens iniciais:

```text
infinite-content-api
infinite-content-worker
```

Ambas deverão:

- Utilizar multi-stage build.
    
- Executar sem root quando possível.
    
- Expor somente portas necessárias.
    
- Não conter secrets.
    
- Possuir health checks.
    
- Utilizar a mesma versão do runtime.
    

---

# 44. Definition of Done do MVP Funcional

O Marco 1 estará concluído quando:

-  Project pode ser criado.
    
-  Pipeline pode ser criado.
    
-  Research e Script podem ser adicionados.
    
-  Pipeline pode ser publicado.
    
-  Execution pode ser criada.
    
-  Fake Provider gera Research.
    
-  Research Artifact é salvo.
    
-  Fake Provider gera Script.
    
-  Script Artifact é salvo.
    
-  Execution termina como Completed.
    
-  Execution pode ser consultada pela API.
    
-  Artifacts podem ser consultados.
    
-  Falha do provider é persistida.
    
-  Teste end-to-end passa.
    

---

# 45. Definition of Done do MVP Assíncrono

O Marco 2 estará concluído quando:

-  StartExecution retorna `202 Accepted`.
    
-  A Execution é salva como Queued.
    
-  A mensagem é registrada na Outbox.
    
-  O Worker publica a Outbox.
    
-  RabbitMQ entrega a mensagem.
    
-  Consumer registra Inbox.
    
-  Research é processada.
    
-  Script é processado.
    
-  Mensagens duplicadas não duplicam efeitos.
    
-  Falhas transitórias geram retry.
    
-  Falhas permanentes seguem para Dead Letter.
    
-  Worker suporta shutdown gracioso.
    
-  Execution travada pode ser recuperada.
    
-  Teste end-to-end assíncrono passa.
    

---

# 46. Definition of Done Final

O MVP do Infinite Content AI estará pronto quando um usuário puder:

1. Criar um Project.
    
2. Criar um Pipeline.
    
3. Configurar Research e Script.
    
4. Publicar o Pipeline.
    
5. Iniciar uma Execution.
    
6. Receber um identificador imediatamente.
    
7. Acompanhar o processamento.
    
8. Consultar o Research gerado.
    
9. Consultar o Script gerado.
    
10. Identificar uma falha quando ela ocorrer.
    

Além disso, o sistema deverá:

- Persistir dados no PostgreSQL.
    
- Processar assincronamente.
    
- Suportar redelivery.
    
- Evitar duplicação básica.
    
- Recuperar trabalho interrompido.
    
- Funcionar com Fake Provider.
    
- Funcionar com um provider real.
    
- Ser executável localmente.
    
- Possuir testes automatizados essenciais.
    

---

# 47. Próximos Passos após o MVP

Depois da conclusão:

## Prioridade 1

- Autenticação real.
    
- Autorização refinada.
    
- Idempotency-Key HTTP.
    
- Observabilidade melhor.
    
- Retry configurável.
    
- Painel básico de execução.
    

## Prioridade 2

- Approvals.
    
- Versionamento avançado de Artifacts.
    
- Redis.
    
- Cache.
    
- Storage de arquivos.
    
- n8n.
    
- Webhooks.
    

## Prioridade 3

- Publicações externas.
    
- Multi-provider.
    
- Provider fallback.
    
- Billing e quotas.
    
- RAG.
    
- Agents especializados.
    
- Processamento de mídia.
    

---

# 48. Checklist de Início

Antes de começar:

-  Solution criada.
    
-  PostgreSQL local funcionando.
    
-  RabbitMQ local funcionando.
    
-  SDK .NET configurado.
    
-  Docker disponível.
    
-  Estrutura de projetos definida.
    
-  Referências validadas.
    
-  Shared Kernel documentado.
    
-  Domain inicial definido.
    
-  Contratos do MVP definidos.
    
-  Este plano revisado.
    

---

# 49. Checklist Diário de Implementação

Antes de encerrar uma sessão:

-  O código compila?
    
-  Os testes passam?
    
-  A nova regra está na camada correta?
    
-  Foi criado código antecipado sem uso?
    
-  OrganizationId está sendo respeitado?
    
-  CancellationToken foi propagado?
    
-  Existe teste do comportamento principal?
    
-  A documentação precisa de atualização?
    
-  O próximo passo está claro?
    

---

# 50. Critério para Adicionar uma Nova Abstração

Antes de criar uma interface, base class ou framework interno, responder:

- Existe mais de uma implementação?
    
- Existe um limite externo?
    
- A abstração melhora os testes?
    
- A abstração representa um conceito real?
    
- A necessidade já existe?
    
- É possível implementar sem ela agora?
    

Se a resposta for predominantemente não, a abstração deverá aguardar.

---

# 51. Critério para Adicionar uma Nova Tecnologia

Antes de adicionar Redis, n8n, outro provider ou novo broker:

- Qual problema real resolve?
    
- O problema já aconteceu?
    
- É possível medir o benefício?
    
- A solução atual está insuficiente?
    
- Quem operará essa tecnologia?
    
- Quais falhas ela introduz?
    
- Existe teste e observabilidade?
    
- Ela bloqueia ou acelera o produto?
    

Tecnologia não deverá ser adicionada apenas por fazer parte da arquitetura futura.

---

# 52. Riscos Principais

## Excesso de arquitetura

Mitigação:

- Implementar vertical slices.
    
- Adiar abstrações sem uso.
    
- Concluir o Marco 1 rapidamente.
    

## Pipeline complexo cedo demais

Mitigação:

- Fluxo linear Research → Script.
    
- Sem branching.
    
- Sem loops.
    
- Sem workflow visual.
    

## Mensageria atrasando o produto

Mitigação:

- Validar fluxo funcional antes.
    
- Adicionar RabbitMQ depois do Marco 1.
    

## Falhas de provider

Mitigação:

- Fake Provider.
    
- Erros classificados.
    
- Retry limitado.
    
- Provider real somente após fluxo funcional.
    

## Documentação excessiva

Mitigação:

- Documentar novas decisões.
    
- Evitar repetir documentos já existentes.
    
- Atualizar documentos conforme implementação.
    

---

# 53. Ordem Resumida

```text
1. Solution e referências
2. Shared Kernel
3. Project
4. PostgreSQL e EF Core
5. API de Project
6. Pipeline
7. PipelineExecution
8. Artifact
9. Fake AI Provider
10. Orquestração funcional
11. Contracts
12. Outbox
13. RabbitMQ
14. Worker
15. Consumers
16. Inbox
17. Retry
18. Recovery
19. Provider real
20. End-to-end final
```

---

# 54. Regra de Parada

Depois de concluir o item 20, parar a expansão arquitetural e avaliar o produto.

Não adicionar automaticamente:

- Mais Agents.
    
- Mais providers.
    
- Mais tipos de Artifact.
    
- Mais filas.
    
- Mais Workers.
    
- Mais abstrações.
    
- Mais documentos.
    

Primeiro validar:

- O fluxo entrega valor?
    
- O output possui qualidade?
    
- O tempo de execução é aceitável?
    
- O custo é aceitável?
    
- Os usuários entendem o resultado?
    
- Qual é o próximo gargalo real?
    

---

# 55. Filosofia Final

O MVP não precisa provar que todas as decisões futuras estão corretas.

Ele precisa provar que o fluxo principal gera valor.

O plano deverá produzir o seguinte resultado:

```text
Uma solicitação entra
    ↓
Uma execução é criada
    ↓
A pesquisa é gerada
    ↓
O roteiro é gerado
    ↓
Os resultados são persistidos
    ↓
O usuário consegue consultá-los
```

A arquitetura deverá apoiar esse fluxo, e não competir com ele.

A regra principal será:

> Implementar primeiro a menor versão completa e funcional do produto; tornar essa versão confiável antes de torná-la sofisticada.

Quando o fluxo Research → Script estiver funcionando de ponta a ponta, o Infinite Content AI deixará de ser apenas uma arquitetura documentada e passará a ser um produto executável.