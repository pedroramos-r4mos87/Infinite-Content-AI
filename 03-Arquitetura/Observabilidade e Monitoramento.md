# Observabilidade e Monitoramento

## Objetivo

Definir como o Infinite Content AI registrará, correlacionará, medirá e monitorará o comportamento da aplicação, dos pipelines, dos agentes, dos providers e das integrações externas.

A observabilidade deverá permitir responder rapidamente:

- O que aconteceu?
    
- Onde aconteceu?
    
- Quando aconteceu?
    
- Qual componente participou?
    
- Qual usuário ou organização iniciou a operação?
    
- Qual pipeline e etapa estavam em execução?
    
- Qual provider e modelo foram utilizados?
    
- Quanto tempo demorou?
    
- Quanto custou?
    
- Houve retentativa ou fallback?
    
- A operação pode ser retomada?
    
- O usuário foi impactado?
    

O objetivo não é apenas armazenar logs.

O objetivo é tornar o comportamento interno do sistema compreensível por meio de dados externos.

---

# Princípios

A observabilidade seguirá os seguintes princípios:

- Logs estruturados.
    
- Correlação ponta a ponta.
    
- Métricas orientadas a decisões.
    
- Traces distribuídos.
    
- Alertas acionáveis.
    
- Monitoramento de custos.
    
- Privacidade por padrão.
    
- Baixa cardinalidade em métricas.
    
- Contexto suficiente para investigação.
    
- Separação entre sinais técnicos e de negócio.
    
- Retenção proporcional à utilidade.
    
- Falhas observáveis e explícitas.
    
- Instrumentação consistente.
    
- Automação de diagnóstico quando possível.
    

---

# Pilares da Observabilidade

O sistema utilizará quatro sinais principais:

```text
Logs
Métricas
Traces
Eventos de negócio
```

Cada sinal possui uma finalidade diferente.

---

# Logs

Logs registram acontecimentos detalhados.

Exemplos:

- Pipeline iniciado.
    
- Etapa concluída.
    
- Provider retornou erro.
    
- Fallback foi utilizado.
    
- Webhook foi rejeitado.
    
- Publicação foi concluída.
    
- Conflito de concorrência ocorreu.
    
- Limite de custo foi atingido.
    

Logs são adequados para investigação e diagnóstico.

---

# Métricas

Métricas representam valores agregados ao longo do tempo.

Exemplos:

- Quantidade de pipelines executados.
    
- Taxa de sucesso.
    
- Latência média.
    
- Número de erros.
    
- Custo acumulado.
    
- Tamanho da fila.
    
- Quantidade de mensagens na DLQ.
    
- Tempo médio de aprovação.
    

Métricas são adequadas para dashboards, tendências e alertas.

---

# Traces

Traces mostram o caminho completo de uma operação entre componentes.

Exemplo:

```text
HTTP Request
    ↓
Application Handler
    ↓
Database
    ↓
Message Broker
    ↓
Worker
    ↓
Pipeline
    ↓
Agent
    ↓
Provider externo
```

Traces são adequados para entender latência e dependências.

---

# Eventos de Negócio

Eventos de negócio representam fatos relevantes para o produto.

Exemplos:

```text
ContentProjectCreated
PipelineCompleted
ScriptApproved
ContentPublished
PublicationFailed
CostLimitExceeded
```

Esses eventos ajudam a acompanhar o funcionamento do produto, não apenas da infraestrutura.

---

# Stack de Observabilidade

A arquitetura deverá preferir padrões abertos.

A recomendação inicial é utilizar:

```text
OpenTelemetry
```

para instrumentação de:

- Logs.
    
- Métricas.
    
- Traces.
    

Possíveis destinos:

- Grafana.
    
- Prometheus.
    
- Loki.
    
- Tempo.
    
- Jaeger.
    
- Elastic Stack.
    
- Azure Monitor.
    
- Application Insights.
    
- Datadog.
    
- New Relic.
    
- Outro backend compatível.
    

A tecnologia final deverá ser registrada em ADR.

---

# OpenTelemetry

O OpenTelemetry será utilizado como camada de instrumentação.

Benefícios:

- Evita dependência direta de um único fornecedor.
    
- Padroniza traces e métricas.
    
- Permite troca de backend.
    
- Possui integração com .NET.
    
- Suporta propagação de contexto.
    
- Facilita instrumentação de HTTP, banco e mensageria.
    

---

# Estrutura Sugerida

```text
Infrastructure/
└── Observability/
    ├── ObservabilityExtensions.cs
    ├── Logging/
    │   ├── LoggingConfiguration.cs
    │   ├── LogEnrichment.cs
    │   └── SensitiveDataFilter.cs
    │
    ├── Metrics/
    │   ├── ApplicationMetrics.cs
    │   ├── PipelineMetrics.cs
    │   ├── ProviderMetrics.cs
    │   └── MessagingMetrics.cs
    │
    ├── Tracing/
    │   ├── ActivitySources.cs
    │   ├── TraceEnrichment.cs
    │   └── TracePropagation.cs
    │
    ├── HealthChecks/
    │   ├── DatabaseHealthCheck.cs
    │   ├── BrokerHealthCheck.cs
    │   └── StorageHealthCheck.cs
    │
    └── DependencyInjection.cs
```

---

# Registro no Program.cs

API:

```csharp
builder.Services
    .AddApplication()
    .AddData(builder.Configuration)
    .AddInfrastructure(builder.Configuration)
    .AddPresentation()
    .AddObservability(builder.Configuration);
```

Worker:

```csharp
builder.Services
    .AddApplication()
    .AddData(builder.Configuration)
    .AddInfrastructure(builder.Configuration)
    .AddWorkers()
    .AddObservability(builder.Configuration);
```

---

# Logs Estruturados

Logs deverão utilizar propriedades nomeadas.

Exemplo:

```csharp
logger.LogInformation(
    "Pipeline {PipelineName} started for execution {PipelineExecutionId}",
    pipelineName,
    pipelineExecutionId);
```

Evitar:

```csharp
logger.LogInformation(
    $"Pipeline {pipelineName} started for execution {pipelineExecutionId}");
```

Logs estruturados permitem:

- Filtrar.
    
- Agrupar.
    
- Criar dashboards.
    
- Correlacionar.
    
- Consultar campos específicos.
    
- Gerar alertas.
    

---

# Campos Padrão

Sempre que aplicável, logs deverão conter:

- Timestamp.
    
- LogLevel.
    
- Message.
    
- ServiceName.
    
- Environment.
    
- ApplicationVersion.
    
- TraceId.
    
- SpanId.
    
- CorrelationId.
    
- CausationId.
    
- MessageId.
    
- OrganizationId.
    
- UserId.
    
- PipelineExecutionId.
    
- PipelineName.
    
- PipelineVersion.
    
- StepExecutionId.
    
- StepName.
    
- AgentName.
    
- Provider.
    
- Model.
    
- Attempt.
    
- DurationMilliseconds.
    
- Cost.
    
- ErrorCode.
    

Nem todos os campos estarão presentes em todos os eventos.

---

# Service Name

Cada processo deverá possuir identificação própria.

Exemplos:

```text
infinite-content-api
infinite-content-worker
infinite-content-outbox-worker
infinite-content-media-worker
infinite-content-python-renderer
```

Isso permite distinguir logs, métricas e traces.

---

# Environment

Todo sinal deverá identificar o ambiente.

Exemplos:

```text
Development
Staging
Production
```

Dados de ambientes diferentes não deverão ser misturados sem identificação.

---

# Application Version

Toda execução deverá registrar a versão da aplicação.

Possíveis valores:

- Versão semântica.
    
- Git commit SHA.
    
- Número do build.
    
- Tag da imagem Docker.
    

Exemplo:

```text
ApplicationVersion: 1.4.0
CommitSha: a18f2d9
```

Isso facilita relacionar incidentes com deploys.

---

# Níveis de Log

## Trace

Utilizado para detalhes muito granulares.

Exemplos:

- Entrada e saída de métodos internos.
    
- Decisões detalhadas de resolução.
    
- Diagnóstico temporário.
    

Normalmente desabilitado em produção.

---

## Debug

Utilizado para diagnóstico de desenvolvimento.

Exemplos:

- Provider selecionado.
    
- Condição de etapa avaliada.
    
- Cache hit ou miss.
    
- Configuração resolvida sem segredos.
    

Deve ser utilizado com moderação em produção.

---

## Information

Representa acontecimentos normais relevantes.

Exemplos:

- Pipeline iniciado.
    
- Etapa concluída.
    
- Publicação agendada.
    
- Fallback utilizado.
    
- Mensagem processada.
    

Esse será o nível mais comum.

---

## Warning

Representa comportamento inesperado, mas recuperável.

Exemplos:

- Retentativa iniciada.
    
- Provider lento.
    
- Resposta parcialmente válida.
    
- Uso próximo do limite.
    
- Webhook recebido com atraso.
    
- Cache indisponível com fallback local.
    

---

## Error

Representa falhas que impediram uma operação.

Exemplos:

- Etapa falhou.
    
- Publicação rejeitada.
    
- Mensagem enviada para DLQ.
    
- Banco indisponível.
    
- Falha permanente do provider.
    

---

## Critical

Representa falha grave do sistema.

Exemplos:

- Perda de acesso ao banco principal.
    
- Corrupção de estado.
    
- Falha generalizada de segurança.
    
- Exposição de segredo.
    
- Impossibilidade de iniciar componentes essenciais.
    

O nível Critical deverá ser raro.

---

# Estratégia de Logging por Evento

Eventos de ciclo de vida importantes deverão possuir nomes consistentes.

Exemplos:

```text
PipelineStarted
PipelineCompleted
PipelineFailed

PipelineStepStarted
PipelineStepCompleted
PipelineStepFailed

AgentExecutionStarted
AgentExecutionCompleted
AgentExecutionFailed

ProviderRequestStarted
ProviderRequestCompleted
ProviderRequestFailed

MessageReceived
MessageProcessed
MessageFailed

WebhookReceived
WebhookAccepted
WebhookRejected

PublicationStarted
PublicationCompleted
PublicationFailed
```

---

# Event ID

Logs importantes poderão utilizar identificadores estáveis.

Exemplo conceitual:

```csharp
public static class LogEvents
{
    public static readonly EventId PipelineStarted =
        new(1001, nameof(PipelineStarted));

    public static readonly EventId PipelineFailed =
        new(1002, nameof(PipelineFailed));
}
```

Benefícios:

- Consultas estáveis.
    
- Dashboards.
    
- Alertas.
    
- Documentação.
    
- Menor dependência da mensagem textual.
    

---

# Templates Centralizados

Mensagens recorrentes poderão ser centralizadas.

Exemplo:

```csharp
internal static partial class PipelineLogMessages
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Pipeline {PipelineName} started")]
    public static partial void PipelineStarted(
        ILogger logger,
        string pipelineName);
}
```

Essa abordagem reduz alocações e melhora consistência.

---

# Correlação

Toda operação relevante deverá possuir um `CorrelationId`.

O mesmo identificador deverá acompanhar o fluxo entre:

- Frontend.
    
- API.
    
- Application.
    
- Banco.
    
- Outbox.
    
- Broker.
    
- Worker.
    
- Pipeline.
    
- Agentes.
    
- Providers.
    
- Webhooks.
    
- n8n.
    

---

# Exemplo de Correlação

```text
HTTP Request
CorrelationId: ABC
    ↓
StartPipelineCommand
CorrelationId: ABC
    ↓
Worker
CorrelationId: ABC
    ↓
Provider Request
CorrelationId: ABC
    ↓
PipelineCompletedEvent
CorrelationId: ABC
```

---

# CorrelationId de Entrada

Quando uma requisição chegar:

1. Verificar se existe um Correlation ID válido.
    
2. Caso não exista, gerar um novo.
    
3. Adicionar ao contexto.
    
4. Incluir na resposta.
    
5. Propagar para operações seguintes.
    

Headers possíveis:

```text
traceparent
tracestate
X-Correlation-ID
```

O padrão W3C deverá ser priorizado para tracing.

---

# CausationId

O `CausationId` identifica a ação que originou outra ação.

Exemplo:

```text
StartPipelineCommand
MessageId: A
    ↓
PipelineStartedEvent
CausationId: A
```

Isso permite reconstruir a cadeia causal.

---

# Propagação em Mensagens

Toda mensagem assíncrona deverá transportar:

- MessageId.
    
- CorrelationId.
    
- CausationId.
    
- Trace context.
    
- Timestamp.
    
- Version.
    

Exemplo:

```csharp
public sealed record MessageContext(
    Guid MessageId,
    string CorrelationId,
    string? CausationId,
    string? TraceParent,
    string? TraceState,
    DateTimeOffset CreatedAt);
```

---

# Contexto de Logging

O sistema poderá utilizar scopes.

Exemplo:

```csharp
using var scope = logger.BeginScope(
    new Dictionary<string, object>
    {
        ["OrganizationId"] = organizationId,
        ["PipelineExecutionId"] = executionId,
        ["CorrelationId"] = correlationId
    });
```

Todos os logs internos receberão essas propriedades.

---

# Trace Distribuído

Cada fluxo deverá gerar um trace.

Cada operação relevante será um span.

Exemplo:

```text
HTTP POST /pipeline-executions
├── Application.CreatePipelineExecution
├── Database.SaveChanges
├── Outbox.SaveMessage
└── HTTP Response
```

Processamento assíncrono:

```text
Consume StartPipelineCommand
├── LoadPipelineExecution
├── ExecuteTrendAgent
│   └── ExternalProvider.Search
├── ExecuteResearchAgent
│   └── ExternalProvider.GenerateText
└── SaveCheckpoint
```

---

# ActivitySource

Componentes internos deverão possuir `ActivitySource`.

Exemplo:

```csharp
public static class ActivitySources
{
    public static readonly ActivitySource Application =
        new("InfiniteContent.Application");

    public static readonly ActivitySource Pipelines =
        new("InfiniteContent.Pipelines");

    public static readonly ActivitySource Agents =
        new("InfiniteContent.Agents");

    public static readonly ActivitySource Providers =
        new("InfiniteContent.Providers");
}
```

---

# Span de Pipeline

Um pipeline poderá gerar um span principal.

Exemplo:

```csharp
using var activity = ActivitySources.Pipelines.StartActivity(
    "pipeline.execute",
    ActivityKind.Internal);
```

Tags:

```text
pipeline.name
pipeline.version
pipeline.execution_id
organization.id
content.channel
```

---

# Span de Etapa

Cada etapa deverá gerar um span.

Exemplo de nome:

```text
pipeline.step.execute
```

Tags:

```text
pipeline.step.name
pipeline.step.type
pipeline.step.attempt
pipeline.step.status
pipeline.step.cost
```

---

# Span de Provider

Toda chamada externa deverá gerar um span do tipo cliente.

Exemplo:

```text
provider.request
```

Tags:

```text
provider.name
provider.capability
provider.model
provider.operation
provider.request_id
provider.input_units
provider.output_units
provider.cost
provider.status
```

Prompts completos não deverão ser adicionados como tags.

---

# Span de Banco

A instrumentação automática do EF Core poderá registrar:

- Comando.
    
- Duração.
    
- Banco.
    
- Status.
    
- Erro.
    

Dados sensíveis e parâmetros deverão ser controlados.

---

# Span de Mensageria

A publicação e o consumo de mensagens deverão gerar spans.

Publicação:

```text
messaging.publish
```

Consumo:

```text
messaging.consume
```

Tags:

```text
messaging.system
messaging.destination
messaging.message_id
messaging.operation
messaging.attempt
```

---

# Sampling

Nem todo trace precisa ser armazenado integralmente.

Estratégias possíveis:

- Todos os erros.
    
- Todas as operações críticas.
    
- Uma porcentagem das operações bem-sucedidas.
    
- Maior amostragem em staging.
    
- Menor amostragem em produção com alto volume.
    
- Amostragem adaptativa.
    

Pipelines caros ou raros poderão possuir sampling maior.

---

# Regras de Sampling

Nunca descartar completamente traces de:

- Falhas críticas.
    
- Publicações.
    
- Operações administrativas.
    
- Mudanças de permissão.
    
- Limites financeiros excedidos.
    
- Incidentes de segurança.
    

---

# Métricas Técnicas

Métricas técnicas deverão acompanhar a saúde dos componentes.

Exemplos:

- Uso de CPU.
    
- Uso de memória.
    
- Garbage collection.
    
- Threads.
    
- Conexões.
    
- Requests por segundo.
    
- Latência HTTP.
    
- Taxa de erro.
    
- Tamanho de filas.
    
- Mensagens pendentes.
    
- Jobs em execução.
    
- Banco de dados.
    
- Storage.
    
- Cache.
    
- Providers externos.
    

---

# Métricas da API

## Requests

```text
http.server.requests
```

Dimensões controladas:

- Método.
    
- Rota normalizada.
    
- Status.
    
- Ambiente.
    
- Serviço.
    

Não utilizar URLs completas com IDs como label.

---

# Latência HTTP

Acompanhar:

- Média.
    
- P50.
    
- P90.
    
- P95.
    
- P99.
    

A média isolada pode esconder operações muito lentas.

---

# Erros HTTP

Monitorar:

- 4xx.
    
- 5xx.
    
- Timeouts.
    
- Cancelamentos.
    
- Rate limits.
    
- Falhas de autenticação.
    
- Falhas de autorização.
    

Erros 4xx não devem ser tratados todos como falhas internas.

---

# Métricas de Pipeline

Métricas recomendadas:

```text
pipeline_executions_total
pipeline_execution_duration_seconds
pipeline_executions_running
pipeline_executions_waiting_approval
pipeline_executions_failed_total
pipeline_executions_cancelled_total
pipeline_execution_cost
```

Dimensões possíveis:

- PipelineName.
    
- PipelineVersion.
    
- Status.
    
- Canal.
    
- Ambiente.
    

Evitar usar `PipelineExecutionId` como label.

---

# Métricas de Etapa

```text
pipeline_step_executions_total
pipeline_step_duration_seconds
pipeline_step_failures_total
pipeline_step_retries_total
pipeline_step_fallbacks_total
pipeline_step_cost
```

Dimensões:

- PipelineName.
    
- StepName.
    
- StepType.
    
- Status.
    
- Provider.
    
- Modelo lógico.
    

---

# Métricas de Agentes

```text
agent_executions_total
agent_execution_duration_seconds
agent_execution_failures_total
agent_output_validation_failures_total
agent_quality_score
agent_cost
```

Dimensões:

- AgentName.
    
- AgentVersion.
    
- TaskType.
    
- Provider.
    
- Model.
    

---

# Métricas de Providers

```text
provider_requests_total
provider_request_duration_seconds
provider_errors_total
provider_timeouts_total
provider_rate_limits_total
provider_retries_total
provider_fallbacks_total
provider_cost
provider_input_units
provider_output_units
```

Dimensões:

- Provider.
    
- Capability.
    
- Model.
    
- Operation.
    
- Status.
    

---

# Métricas de Mensageria

```text
messages_published_total
messages_consumed_total
message_processing_duration_seconds
message_failures_total
message_retries_total
dead_letter_messages_total
queue_depth
oldest_message_age_seconds
```

Dimensões:

- Broker.
    
- Queue.
    
- MessageType.
    
- Consumer.
    
- Status.
    

---

# Métricas da Outbox

```text
outbox_pending_messages
outbox_processed_total
outbox_failures_total
outbox_processing_duration_seconds
outbox_oldest_message_age_seconds
```

A idade da mensagem mais antiga é um indicador importante.

Uma Outbox pequena, mas parada há muito tempo, ainda representa problema.

---

# Métricas da Inbox

```text
inbox_messages_processed_total
inbox_duplicates_total
inbox_failures_total
```

O aumento de duplicidades poderá indicar falhas de confirmação ou reentrega excessiva.

---

# Métricas de Banco

Monitorar:

- Latência de queries.
    
- Latência de commits.
    
- Pool de conexões.
    
- Conexões ativas.
    
- Timeouts.
    
- Deadlocks.
    
- Conflitos de concorrência.
    
- Queries lentas.
    
- Tamanho das tabelas.
    
- Espaço disponível.
    
- Replicação, se aplicável.
    
- Outbox pendente.
    

---

# Métricas de Cache

Monitorar:

```text
cache_hits_total
cache_misses_total
cache_errors_total
cache_operation_duration_seconds
```

Também acompanhar:

```text
cache_hit_ratio
```

Uma taxa de hit baixa pode indicar que o cache não está gerando valor.

---

# Métricas de Storage

Monitorar:

- Uploads.
    
- Downloads.
    
- Falhas.
    
- Duração.
    
- Bytes armazenados.
    
- Arquivos temporários.
    
- Arquivos órfãos.
    
- URLs pré-assinadas geradas.
    
- Erros de autorização.
    
- Espaço utilizado.
    

---

# Métricas de Publicação

```text
publications_total
publication_duration_seconds
publication_failures_total
publication_retries_total
publication_duplicates_prevented_total
scheduled_publications_pending
```

Dimensões:

- Plataforma.
    
- Status.
    
- Tipo de conteúdo.
    
- Canal.
    

---

# Métricas de Aprovação

```text
approvals_requested_total
approvals_granted_total
approvals_rejected_total
approval_wait_duration_seconds
approvals_expired_total
```

Essas métricas ajudam a identificar gargalos humanos.

---

# Métricas de Custos

Monitorar custos por:

- Organização.
    
- Projeto.
    
- Pipeline.
    
- Etapa.
    
- Agente.
    
- Provider.
    
- Modelo.
    
- Capacidade.
    
- Dia.
    
- Mês.
    

Exemplos:

```text
provider_cost_total
pipeline_cost_total
organization_daily_cost
organization_monthly_cost
cost_limit_exceeded_total
```

Cuidados com cardinalidade deverão ser aplicados.

IDs de organização poderão ser utilizados em sistemas de billing ou analytics específicos, não necessariamente em todas as métricas operacionais.

---

# Custos Estimados e Reais

O sistema deverá distinguir:

```text
EstimatedCost
ActualCost
```

Quando o provider não informar custo final, o sistema poderá calcular uma estimativa.

A diferença deverá ser monitorada.

---

# Métricas de Negócio

Exemplos:

- Projetos criados.
    
- Conteúdos gerados.
    
- Roteiros aprovados.
    
- Conteúdos publicados.
    
- Tempo entre criação e publicação.
    
- Taxa de aprovação.
    
- Taxa de retrabalho.
    
- Custo por conteúdo publicado.
    
- Conteúdos por plataforma.
    
- Execuções por estratégia.
    
- Engajamento após publicação.
    
- Receita associada, futuramente.
    

---

# Métricas de Qualidade

A qualidade poderá ser acompanhada por:

- Score de revisão.
    
- Taxa de aprovação na primeira tentativa.
    
- Quantidade de revisões.
    
- Saídas inválidas.
    
- Conteúdo rejeitado.
    
- Taxa de correção manual.
    
- Aderência ao schema.
    
- Fontes verificadas.
    
- Alucinações detectadas.
    
- Taxa de publicação cancelada.
    

---

# Cardinalidade

Cardinalidade alta pode tornar métricas caras e lentas.

Não utilizar como labels de métricas:

- UserId.
    
- PipelineExecutionId.
    
- StepExecutionId.
    
- CorrelationId.
    
- MessageId.
    
- URL completa.
    
- Prompt.
    
- Texto de erro completo.
    
- ExternalRequestId.
    

Esses dados pertencem a logs e traces.

---

# Labels Permitidas

Labels devem possuir conjunto limitado de valores.

Exemplos:

- ServiceName.
    
- Environment.
    
- PipelineName.
    
- StepName.
    
- AgentName.
    
- Provider.
    
- Model lógico.
    
- Capability.
    
- Status.
    
- ErrorType.
    
- Platform.
    
- Queue.
    

---

# Error Code

Erros deverão possuir códigos internos estáveis.

Exemplos:

```text
provider_timeout
provider_rate_limit
pipeline_invalid_state
pipeline_cost_limit_exceeded
publication_not_approved
webhook_invalid_signature
database_concurrency_conflict
message_processing_failed
```

O código deverá ser utilizado em:

- Logs.
    
- Métricas.
    
- Traces.
    
- Problem Details.
    
- Alertas.
    
- Auditoria.
    

---

# Exception Handling

Exceções inesperadas deverão ser capturadas nas bordas.

Bordas importantes:

- Middleware HTTP.
    
- Consumidor de mensagem.
    
- Job do Worker.
    
- Executor de pipeline.
    
- Chamada de provider.
    
- Processador de Outbox.
    
- Endpoint de webhook.
    

---

# Exceções Esperadas

Falhas esperadas deverão preferencialmente utilizar resultados controlados.

Exemplos:

- Conteúdo não aprovado.
    
- Limite excedido.
    
- Estado inválido.
    
- Provider indisponível.
    
- Entrada inválida.
    

Não é necessário lançar exceção para toda falha de negócio.

---

# Exceções Inesperadas

Exceções inesperadas deverão registrar:

- Tipo.
    
- Mensagem segura.
    
- Stack trace.
    
- Componente.
    
- Operação.
    
- Contexto.
    
- CorrelationId.
    
- Estado relevante.
    
- Versão da aplicação.
    

Stack traces não deverão ser enviados ao cliente.

---

# Agrupamento de Erros

Sistemas de monitoramento deverão agrupar erros por:

- Tipo de exceção.
    
- ErrorCode.
    
- Componente.
    
- Operação.
    
- Stack trace normalizado.
    
- Provider.
    
- Pipeline e etapa.
    

Valores dinâmicos não deverão impedir o agrupamento.

---

# Registro de Payloads

Requests e responses completos não deverão ser registrados por padrão.

Riscos:

- Dados pessoais.
    
- Segredos.
    
- Prompts confidenciais.
    
- Tokens.
    
- Propriedade intelectual.
    
- Alto volume.
    
- Alto custo de armazenamento.
    

---

# Estratégia de Payload

Quando necessário, registrar:

- Hash.
    
- Tamanho.
    
- Tipo.
    
- SchemaVersion.
    
- Identificador interno.
    
- Campos não sensíveis.
    
- Resumo.
    
- Local seguro de auditoria.
    

Payload completo deverá exigir justificativa explícita.

---

# Logging de Prompts

Prompts poderão conter dados confidenciais.

Possíveis níveis de política:

## Disabled

Nenhum prompt armazenado.

## Metadata Only

Armazena:

- PromptId.
    
- Versão.
    
- Tamanho.
    
- Hash.
    
- Variáveis utilizadas sem valores sensíveis.
    

## Redacted

Armazena conteúdo com dados sensíveis removidos.

## Full Audit

Armazena conteúdo completo em repositório protegido, com retenção limitada e autorização especial.

A política deverá ser configurável por ambiente e organização.

---

# Logging de Respostas de IA

As mesmas regras de prompts deverão ser aplicadas às respostas.

Preferir registrar:

- Tamanho.
    
- Hash.
    
- Modelo.
    
- Tokens.
    
- Custo.
    
- Finish reason.
    
- Resultado da validação.
    
- ArtifactId.
    

---

# Redação de Dados

Antes de enviar dados para logs, aplicar filtros para:

- API keys.
    
- Tokens.
    
- Authorization headers.
    
- Cookies.
    
- Senhas.
    
- E-mails, quando necessário.
    
- Telefones.
    
- Dados financeiros.
    
- Prompts sensíveis.
    
- Connection strings.
    

---

# Headers Sensíveis

Nunca registrar integralmente:

```text
Authorization
Cookie
Set-Cookie
X-Api-Key
Proxy-Authorization
```

Headers customizados contendo segredos também deverão ser filtrados.

---

# Logs de HTTP

Poderão registrar:

- Método.
    
- Rota normalizada.
    
- Status.
    
- Duração.
    
- Content-Length.
    
- User agent reduzido.
    
- CorrelationId.
    
- OrganizationId, quando seguro.
    

Evitar registrar query strings completas sem sanitização.

---

# Logs de Banco

Em produção:

- Sensitive data logging desabilitado.
    
- Parâmetros sensíveis ocultos.
    
- Queries lentas registradas.
    
- Falhas registradas.
    
- Comandos críticos monitorados.
    

---

# Logs de Providers

Registrar:

- Provider.
    
- Modelo.
    
- Operação.
    
- Duração.
    
- Status.
    
- Tokens ou unidades.
    
- Custo.
    
- ExternalRequestId.
    
- Tentativa.
    
- Fallback.
    
- ErrorCode.
    

Não registrar:

- API key.
    
- Headers secretos.
    
- Prompt completo sem política.
    
- Resposta completa sem política.
    

---

# Dashboards

O sistema deverá possuir dashboards orientados a diferentes públicos.

---

# Dashboard Operacional

Deverá apresentar:

- Saúde da API.
    
- Saúde dos Workers.
    
- Taxa de erro.
    
- Latência.
    
- Filas.
    
- Outbox.
    
- Banco.
    
- Cache.
    
- Storage.
    
- Providers.
    
- Deploy atual.
    

---

# Dashboard de Pipelines

Deverá apresentar:

- Execuções iniciadas.
    
- Execuções concluídas.
    
- Execuções falhas.
    
- Execuções em andamento.
    
- Execuções aguardando aprovação.
    
- Duração por pipeline.
    
- Etapas mais lentas.
    
- Etapas com maior falha.
    
- Retentativas.
    
- Fallbacks.
    
- Custos.
    

---

# Dashboard de Providers

Deverá apresentar:

- Disponibilidade.
    
- Latência.
    
- Taxa de erro.
    
- Rate limits.
    
- Modelos utilizados.
    
- Custo.
    
- Tokens.
    
- Fallbacks.
    
- Respostas inválidas.
    
- Qualidade média.
    

---

# Dashboard de Mensageria

Deverá apresentar:

- Tamanho das filas.
    
- Idade da mensagem mais antiga.
    
- Taxa de consumo.
    
- Taxa de publicação.
    
- Mensagens com falha.
    
- Retentativas.
    
- DLQ.
    
- Consumidores ativos.
    

---

# Dashboard de Negócio

Poderá apresentar:

- Conteúdos gerados.
    
- Conteúdos publicados.
    
- Taxa de aprovação.
    
- Tempo até publicação.
    
- Custo por conteúdo.
    
- Distribuição por plataforma.
    
- Desempenho inicial.
    
- Uso por organização.
    
- Crescimento de projetos.
    

---

# Dashboard Financeiro

Deverá apresentar:

- Custo diário.
    
- Custo mensal.
    
- Custo por provider.
    
- Custo por modelo.
    
- Custo por pipeline.
    
- Custo por etapa.
    
- Custo por conteúdo publicado.
    
- Organizações próximas do limite.
    
- Diferença entre custo estimado e real.
    

---

# Alertas

Alertas deverão indicar situações que exigem ação.

Um alerta deve possuir:

- Condição clara.
    
- Severidade.
    
- Responsável.
    
- Canal.
    
- Runbook.
    
- Janela de avaliação.
    
- Estratégia de deduplicação.
    
- Critério de recuperação.
    

---

# Alertas Acionáveis

Evitar alertas que apenas informam que algo aconteceu.

Preferir alertas que indiquem:

- Impacto.
    
- Escopo.
    
- Possível causa.
    
- Próxima ação.
    
- Link para dashboard.
    
- Link para runbook.
    

---

# Severidades

## Informational

Não exige ação imediata.

Exemplos:

- Deploy concluído.
    
- Feature flag alterada.
    
- Provider secundário desativado.
    

---

## Warning

Exige acompanhamento.

Exemplos:

- Latência aumentando.
    
- Custo próximo do limite.
    
- Fila crescendo.
    
- Provider com erros intermitentes.
    
- Outbox atrasada.
    

---

## High

Exige ação rápida.

Exemplos:

- Taxa elevada de falhas.
    
- DLQ crescendo.
    
- Banco próximo do limite.
    
- Publicações falhando.
    
- Worker indisponível.
    

---

## Critical

Exige intervenção imediata.

Exemplos:

- API indisponível.
    
- Banco inacessível.
    
- Vazamento de segredo.
    
- Falha generalizada em publicações.
    
- Perda de isolamento entre organizações.
    
- Crescimento financeiro descontrolado.
    

---

# Alertas da API

Exemplos:

- Taxa de 5xx acima do limite.
    
- P95 acima da meta.
    
- Queda brusca de requisições.
    
- Aumento de 401 ou 403.
    
- Rate limits excessivos.
    
- Instâncias não saudáveis.
    

---

# Alertas de Pipeline

Exemplos:

- Muitas execuções falhando.
    
- Pipeline parado em uma etapa.
    
- Execução excedeu duração máxima.
    
- Muitas execuções aguardando aprovação.
    
- Número excessivo de retentativas.
    
- Falha recorrente na mesma etapa.
    
- Custos acima do esperado.
    

---

# Alertas de Provider

Exemplos:

- Taxa de erro elevada.
    
- Muitos timeouts.
    
- Muitos HTTP 429.
    
- Latência degradada.
    
- Fallback sendo usado excessivamente.
    
- Credencial inválida.
    
- Quota próxima do limite.
    
- Respostas inválidas aumentando.
    

---

# Alertas de Mensageria

Exemplos:

- Fila crescendo continuamente.
    
- Mensagem antiga demais.
    
- Consumidor indisponível.
    
- DLQ recebeu mensagens.
    
- Broker indisponível.
    
- Taxa de publicação maior que consumo.
    
- Redelivery excessivo.
    

---

# Alertas da Outbox

Exemplos:

- Mensagens pendentes acima do limite.
    
- Mensagem antiga demais.
    
- Falhas repetidas de publicação.
    
- Processador parado.
    
- Crescimento contínuo da tabela.
    

---

# Alertas de Banco

Exemplos:

- Pool de conexões esgotando.
    
- Deadlocks.
    
- Queries lentas.
    
- Uso elevado de CPU.
    
- Espaço em disco baixo.
    
- Replicação atrasada.
    
- Backup falhou.
    
- Migration falhou.
    

---

# Alertas de Custos

Exemplos:

- Organização atingiu 80% do orçamento.
    
- Organização atingiu 100% do orçamento.
    
- Custo diário acima da média.
    
- Provider apresentou aumento abrupto.
    
- Pipeline ultrapassou custo estimado.
    
- Modelo caro sendo usado excessivamente.
    
- Retentativas gerando custo anormal.
    

---

# Alertas de Segurança

Exemplos:

- Muitas falhas de autenticação.
    
- Tentativa de acesso cruzado.
    
- Webhooks inválidos.
    
- Secret detectado em log.
    
- API key revogada sendo utilizada.
    
- Operação administrativa incomum.
    
- Volume anormal de publicações.
    
- Aumento inesperado de uploads rejeitados.
    

---

# Deduplicação de Alertas

Alertas repetidos deverão ser agrupados.

Evitar enviar centenas de notificações para o mesmo incidente.

Estratégias:

- Janela de silêncio.
    
- Agrupamento por componente.
    
- Agrupamento por ErrorCode.
    
- Estado aberto e resolvido.
    
- Escalonamento progressivo.
    

---

# Alert Fatigue

Muitos alertas irrelevantes reduzem a capacidade de resposta.

Alertas deverão ser revisados periodicamente.

Cada alerta deverá responder:

- Alguém consegue agir?
    
- A condição indica impacto real?
    
- O limite está correto?
    
- O alerta possui dono?
    
- Existe runbook?
    
- O alerta ainda é necessário?
    

---

# SLO

Service Level Objectives definem metas de confiabilidade.

Exemplos iniciais:

```text
Disponibilidade mensal da API: 99,9%
P95 de endpoints rápidos: inferior a 500 ms
Processamento de comandos: iniciado em até 1 minuto
Publicação de mensagens Outbox: até 30 segundos
```

As metas deverão refletir necessidades reais do produto.

---

# SLI

Service Level Indicators são as métricas utilizadas para medir SLOs.

Exemplos:

- Percentual de requests bem-sucedidos.
    
- Latência da API.
    
- Tempo de espera na fila.
    
- Percentual de pipelines concluídos.
    
- Tempo de publicação da Outbox.
    
- Disponibilidade do banco.
    

---

# Error Budget

O error budget representa quanto o sistema pode falhar dentro do SLO.

Exemplo:

```text
SLO: 99,9%
Error Budget mensal: aproximadamente 0,1%
```

Quando o orçamento de erro for consumido rapidamente, novas mudanças arriscadas poderão ser reduzidas até a estabilização.

---

# SLO de Pipelines

Pipelines longos podem exigir metas diferentes da API.

Exemplos:

- Percentual de pipelines concluídos sem intervenção.
    
- Percentual de pipelines retomáveis após falha.
    
- Tempo médio por tipo de conteúdo.
    
- Percentual de publicações idempotentes.
    
- Tempo máximo aguardando processamento interno.
    

---

# SLO de Providers

Como providers externos não estão sob controle total, acompanhar:

- Disponibilidade percebida.
    
- Latência.
    
- Taxa de erro.
    
- Taxa de fallback.
    
- Taxa de resposta válida.
    

Um provider externo indisponível não deverá necessariamente derrubar o SLO global se houver fallback funcional.

---

# Health Checks

Health checks indicam o estado atual dos componentes.

O sistema deverá distinguir:

```text
Liveness
Readiness
Dependency Health
```

---

# Liveness

Indica se o processo está vivo.

Deverá ser simples.

Não deve depender de todas as integrações externas.

Exemplo:

```text
/health/live
```

Se falhar, o processo poderá ser reiniciado.

---

# Readiness

Indica se o processo está pronto para receber trabalho.

Exemplo:

```text
/health/ready
```

Poderá verificar:

- Banco.
    
- Configuração essencial.
    
- Broker.
    
- Storage, quando essencial.
    

Se falhar, o processo não deverá receber tráfego novo.

---

# Dependency Health

Endpoint ou dashboard detalhado para dependências.

Exemplos:

- PostgreSQL.
    
- Redis.
    
- Message Broker.
    
- Storage.
    
- Serviço Python.
    
- n8n.
    
- Providers críticos.
    

Esse endpoint não deverá expor detalhes sensíveis publicamente.

---

# Dependências Obrigatórias e Opcionais

Uma dependência opcional indisponível não deverá tornar toda a API não pronta.

Exemplo:

```text
Provider de thumbnail indisponível
```

A API ainda pode funcionar para outros casos de uso.

A criticidade deverá ser configurada.

---

# Health Check de Providers Pagos

Evitar chamadas pagas frequentes apenas para verificar saúde.

Preferir:

- Estado das últimas chamadas.
    
- Endpoint gratuito, quando disponível.
    
- Verificação de DNS e conexão.
    
- Circuit breaker.
    
- Métricas de erro recentes.
    

---

# Synthetic Monitoring

O sistema poderá executar testes sintéticos.

Exemplos:

- Login de teste.
    
- Criação de projeto de teste.
    
- Execução de pipeline simplificado.
    
- Upload e download de arquivo.
    
- Publicação privada de teste.
    
- Recebimento de webhook simulado.
    

Esses testes ajudam a detectar problemas antes dos usuários.

---

# Heartbeats

Workers poderão emitir heartbeats.

Exemplo:

```text
worker_heartbeat_timestamp
```

Isso permite detectar processos travados mesmo quando não há mensagens sendo consumidas.

---

# Monitoramento de Jobs Travados

Uma execução poderá ser considerada suspeita quando:

- Está em Running há tempo excessivo.
    
- Não atualiza heartbeat.
    
- Não cria checkpoint.
    
- Não emite logs.
    
- Não atualiza custo.
    
- Não possui atividade de provider.
    
- O lease expirou.
    

Um job de reconciliação poderá detectar e tratar esses casos.

---

# Reconciliação

Jobs periódicos poderão verificar inconsistências.

Exemplos:

- Pipeline marcado como Running sem Worker ativo.
    
- Publicação externa concluída, mas banco pendente.
    
- Arquivo no storage sem referência.
    
- Outbox pendente por tempo excessivo.
    
- Mensagem consumida sem atualização de estado.
    
- Job externo finalizado sem webhook processado.
    

---

# Auditoria versus Observabilidade

Auditoria e observabilidade possuem objetivos diferentes.

## Observabilidade

Focada em:

- Diagnóstico.
    
- Desempenho.
    
- Disponibilidade.
    
- Falhas.
    
- Operação.
    

## Auditoria

Focada em:

- Quem realizou a ação.
    
- Qual recurso foi alterado.
    
- Estado anterior e posterior.
    
- Motivo.
    
- Conformidade.
    
- Responsabilidade.
    

Um log técnico não substitui um audit log.

---

# Dados de Auditoria

Audit logs deverão possuir retenção e proteção próprias.

Eles não deverão depender apenas do backend de logs operacional.

---

# Retenção

A retenção deverá variar por sinal.

Exemplo inicial:

|Sinal|Retenção|
|---|--:|
|Logs de aplicação|30 dias|
|Logs de erro|90 dias|
|Traces completos|7 a 14 dias|
|Métricas agregadas|13 meses|
|Audit logs|Conforme política legal|
|Payloads sensíveis|Mínimo necessário|
|Logs de desenvolvimento|Curta duração|

Os valores finais dependerão de custo e requisitos.

---

# Níveis de Retenção

## Curta

Para diagnóstico recente.

Exemplos:

- Debug logs.
    
- Traces detalhados.
    
- Payloads temporários.
    

## Média

Para análise operacional.

Exemplos:

- Logs de aplicação.
    
- Erros.
    
- Métricas detalhadas.
    

## Longa

Para tendências e auditoria.

Exemplos:

- Métricas agregadas.
    
- Custos.
    
- Audit logs.
    
- Indicadores de negócio.
    

---

# Arquivamento

Logs antigos poderão ser enviados para armazenamento mais barato.

Exemplos:

- Object storage.
    
- Data lake.
    
- Arquivo comprimido.
    
- Repositório de auditoria.
    

A pesquisa poderá ser mais lenta nesses dados.

---

# Custo da Observabilidade

Observabilidade também possui custo.

Principais fontes:

- Volume de logs.
    
- Cardinalidade de métricas.
    
- Quantidade de traces.
    
- Retenção.
    
- Dashboards.
    
- Ingestão.
    
- Egress.
    
- Armazenamento.
    

A instrumentação deverá equilibrar utilidade e custo.

---

# Controle de Volume

Estratégias:

- Sampling de traces.
    
- Níveis de log por ambiente.
    
- Filtros.
    
- Agregação de métricas.
    
- Limites de tamanho.
    
- Redução de payloads.
    
- Retenção diferenciada.
    
- Desativação de logs repetitivos.
    
- Rate limit de erros idênticos.
    

---

# Logging em Desenvolvimento

No desenvolvimento, poderá haver:

- Console legível.
    
- Nível Debug.
    
- SQL detalhado.
    
- Traces locais.
    
- Ferramentas como Aspire Dashboard.
    

Dados reais de produção não deverão ser utilizados.

---

# Logging em Staging

Em staging:

- Estrutura semelhante à produção.
    
- Sampling maior.
    
- Logs mais detalhados.
    
- Testes sintéticos.
    
- Alertas controlados.
    
- Dados fictícios ou anonimizados.
    

---

# Logging em Produção

Em produção:

- Logs estruturados.
    
- Nível Information por padrão.
    
- Debug apenas temporariamente.
    
- Sensitive data logging desabilitado.
    
- Retenção controlada.
    
- Alertas ativos.
    
- Exportação resiliente.
    
- Acesso restrito.
    

---

# Falha do Backend de Observabilidade

A indisponibilidade do sistema de logs não deverá derrubar a aplicação.

A exportação deverá ser:

- Assíncrona.
    
- Bufferizada.
    
- Limitada.
    
- Resiliente.
    

A aplicação não deverá bloquear requisições aguardando envio de telemetria.

---

# Buffer de Telemetria

O buffer deverá possuir limites.

Se o backend permanecer indisponível:

- Descartar dados de menor prioridade.
    
- Preservar erros quando possível.
    
- Registrar métricas de descarte.
    
- Evitar consumo infinito de memória ou disco.
    

---

# Telemetry Drop

O sistema deverá monitorar quando telemetria for descartada.

Exemplos:

```text
telemetry_dropped_logs_total
telemetry_dropped_spans_total
telemetry_export_failures_total
```

---

# Privacidade

A observabilidade deverá respeitar privacidade.

Não registrar desnecessariamente:

- Dados pessoais.
    
- Prompts confidenciais.
    
- Conteúdo completo.
    
- Tokens.
    
- Credenciais.
    
- Dados financeiros detalhados.
    
- Arquivos.
    
- Informações de outras organizações.
    

---

# Controle de Acesso

O acesso aos sistemas de observabilidade deverá ser restrito.

Papéis possíveis:

- Operações.
    
- Engenharia.
    
- Segurança.
    
- Suporte limitado.
    
- Auditoria.
    

Nem todos precisam acessar logs completos.

---

# Logs por Organização

Usuários comuns não deverão acessar logs técnicos globais.

Se o produto expuser histórico de execução, deverá apresentar uma projeção segura.

Exemplo:

```text
Etapa GenerateScript concluída
Duração: 12 segundos
Provider: configurado
Status: sucesso
```

Sem expor:

- Stack trace.
    
- Credenciais.
    
- Prompt interno.
    
- Dados de outras organizações.
    
- Detalhes de infraestrutura.
    

---

# Timeline de Execução

O produto poderá exibir uma timeline baseada nos estados persistidos.

Exemplo:

```text
10:00 Pipeline iniciado
10:01 Pesquisa concluída
10:03 Roteiro gerado
10:05 Aguardando aprovação
11:22 Roteiro aprovado
11:25 Voz gerada
11:31 Vídeo renderizado
11:32 Publicação agendada
```

Essa timeline não deverá depender exclusivamente dos logs.

Os estados oficiais devem vir do banco.

---

# Status para Usuários

Mensagens de status deverão ser compreensíveis.

Evitar:

```text
NullReferenceException in PipelineExecutor
```

Preferir:

```text
Não foi possível gerar o roteiro.
A execução será tentada novamente.
```

Detalhes técnicos permanecem nos logs internos.

---

# Diagnóstico de Pipeline

Uma tela de diagnóstico poderá exibir:

- Pipeline.
    
- Versão.
    
- Status.
    
- Etapa atual.
    
- Tentativas.
    
- Providers utilizados.
    
- Fallbacks.
    
- Custos.
    
- Artefatos.
    
- Aprovações.
    
- Erros normalizados.
    
- CorrelationId.
    
- Horários.
    

---

# Diagnóstico de Provider

Deverá ser possível responder:

- Qual provider falhou?
    
- Qual modelo foi utilizado?
    
- Qual operação?
    
- Qual latência?
    
- Qual erro normalizado?
    
- Foi transitório?
    
- Houve retentativa?
    
- Houve fallback?
    
- Quanto custou?
    
- Qual ExternalRequestId?
    

---

# Diagnóstico de Mensagem

Deverá ser possível responder:

- Qual mensagem foi publicada?
    
- Quem publicou?
    
- Qual consumidor recebeu?
    
- Quantas tentativas?
    
- Foi processada?
    
- Foi para DLQ?
    
- Qual CorrelationId?
    
- Qual CausationId?
    
- Qual execução estava relacionada?
    

---

# Runbooks

Alertas críticos deverão possuir runbooks.

Um runbook deverá conter:

- Descrição.
    
- Sintomas.
    
- Impacto.
    
- Métricas relevantes.
    
- Queries de logs.
    
- Possíveis causas.
    
- Passos de diagnóstico.
    
- Ações seguras.
    
- Critério de escalonamento.
    
- Procedimento de recuperação.
    
- Verificação pós-incidente.
    

---

# Exemplo de Runbook

```text
Alerta: OutboxOldestMessageTooOld

1. Verificar se o Outbox Processor está ativo.
2. Verificar erros de conexão com o broker.
3. Consultar mensagens com maior AttemptCount.
4. Confirmar disponibilidade do broker.
5. Reiniciar o processador somente se seguro.
6. Não apagar mensagens manualmente.
7. Confirmar redução da idade da mensagem mais antiga.
```

---

# Incidentes

Todo incidente relevante deverá possuir:

- Identificador.
    
- Severidade.
    
- Início.
    
- Detecção.
    
- Impacto.
    
- Componentes.
    
- Responsável.
    
- Timeline.
    
- Mitigação.
    
- Resolução.
    
- Causa raiz.
    
- Ações preventivas.
    

---

# Postmortem

Incidentes importantes deverão gerar postmortem sem busca por culpados.

O documento deverá responder:

- O que aconteceu?
    
- Qual foi o impacto?
    
- Como foi detectado?
    
- Por que aconteceu?
    
- Por que não foi detectado antes?
    
- O que funcionou?
    
- O que falhou?
    
- Quais ações serão tomadas?
    

---

# Deploy Markers

Deploys deverão gerar marcadores nos dashboards.

Exemplo:

```text
Deployment 1.7.0
Commit: a18f2d9
StartedAt: 14:00
CompletedAt: 14:04
```

Isso facilita relacionar regressões com mudanças recentes.

---

# Métricas por Versão

Quando viável, acompanhar temporariamente:

- Taxa de erro por versão.
    
- Latência por versão.
    
- Falhas de pipeline por versão.
    
- Consumo de memória por versão.
    

Evitar manter versões como label indefinidamente se houver alta cardinalidade histórica.

---

# Feature Flags

Alterações de feature flags relevantes deverão gerar:

- Audit log.
    
- Evento.
    
- Marcador de observabilidade.
    

Exemplos:

```text
AutomaticPublishingEnabled
GeminiFallbackDisabled
HighCostModelsEnabled
```

---

# Monitoramento de Configuração

Mudanças críticas deverão ser observáveis.

Exemplos:

- Provider principal alterado.
    
- Timeout alterado.
    
- Limite financeiro alterado.
    
- Política de fallback alterada.
    
- Pipeline ativado ou desativado.
    
- Escopo OAuth alterado.
    

---

# Testes de Observabilidade

Testes deverão validar que eventos críticos geram sinais.

Exemplos:

- Pipeline iniciado gera log e métrica.
    
- Falha de provider gera trace com erro.
    
- Retentativa incrementa métrica.
    
- Fallback é registrado.
    
- CorrelationId é propagado.
    
- Dados sensíveis são removidos.
    
- Mensagem em DLQ gera alerta.
    
- Custo excedido gera evento e auditoria.
    

---

# Testes de Propagação

Testes de integração deverão validar:

```text
API
    ↓
Outbox
    ↓
Broker
    ↓
Worker
```

mantendo o mesmo contexto de trace e correlação.

---

# Testes de Redação

Entradas contendo dados sensíveis deverão ser testadas.

Exemplo:

```text
Authorization: Bearer secret-token
```

O log deverá armazenar:

```text
Authorization: [REDACTED]
```

---

# Testes de Métricas

Validar:

- Nome.
    
- Unidade.
    
- Labels permitidas.
    
- Ausência de IDs únicos.
    
- Incrementos.
    
- Histogramas.
    
- Erros.
    
- Custos.
    

---

# Testes de Alertas

Alertas críticos deverão ser testados.

Possibilidades:

- Ambiente de staging.
    
- Simulação.
    
- Falha controlada.
    
- Chaos testing.
    
- Redução temporária de thresholds.
    

Um alerta que nunca foi testado pode falhar quando for necessário.

---

# Chaos Testing

No futuro, o sistema poderá testar falhas controladas.

Exemplos:

- Provider indisponível.
    
- Broker lento.
    
- Banco com latência.
    
- Storage indisponível.
    
- Worker interrompido.
    
- Webhook duplicado.
    
- Timeout de renderização.
    

O objetivo é validar:

- Retentativa.
    
- Fallback.
    
- Idempotência.
    
- Alertas.
    
- Retomada.
    
- Consistência.
    

---

# Indicadores Iniciais do MVP

Para o MVP, monitorar no mínimo:

## API

- Requests.
    
- Latência.
    
- 5xx.
    
- Health checks.
    

## Banco

- Conexões.
    
- Latência.
    
- Erros.
    
- Espaço.
    

## Worker

- Heartbeat.
    
- Jobs processados.
    
- Jobs falhos.
    
- Duração.
    

## Pipelines

- Iniciados.
    
- Concluídos.
    
- Falhos.
    
- Em execução.
    
- Aguardando aprovação.
    

## Providers

- Chamadas.
    
- Erros.
    
- Latência.
    
- Custos.
    
- Retentativas.
    
- Fallbacks.
    

## Mensageria

- Queue depth.
    
- Mensagem mais antiga.
    
- DLQ.
    
- Erros de consumo.
    

## Outbox

- Pendentes.
    
- Falhas.
    
- Idade da mensagem mais antiga.
    

---

# Estratégia de Evolução

## Fase 1

- Logs estruturados.
    
- CorrelationId.
    
- OpenTelemetry básico.
    
- Health checks.
    
- Métricas essenciais.
    
- Dashboard operacional.
    
- Alertas críticos.
    

## Fase 2

- Tracing distribuído completo.
    
- Dashboards de pipelines.
    
- Monitoramento de custos.
    
- Alertas de providers.
    
- Runbooks.
    
- SLOs iniciais.
    

## Fase 3

- Métricas de qualidade.
    
- SLOs por pipeline.
    
- Synthetic monitoring.
    
- Reconciliação automatizada.
    
- Chaos testing.
    
- Análise preditiva de custo e falhas.
    

---

# Regras Arquiteturais

- Toda operação relevante deve possuir CorrelationId.
    
- Mensagens devem propagar trace context.
    
- Logs devem ser estruturados.
    
- Logs não devem conter segredos.
    
- Prompts e respostas não devem ser registrados sem política explícita.
    
- Métricas não devem utilizar IDs únicos como labels.
    
- Pipelines devem registrar início, fim e falha.
    
- Etapas devem registrar duração, status e tentativas.
    
- Providers devem registrar latência, custo e erros normalizados.
    
- Fall­backs devem ser observáveis.
    
- Retentativas devem ser mensuráveis.
    
- Mensagens em DLQ devem gerar alerta.
    
- A Outbox deve possuir métricas de atraso.
    
- Workers devem emitir heartbeat.
    
- Health checks devem distinguir liveness e readiness.
    
- Dependências opcionais não devem derrubar toda a aplicação.
    
- Alertas devem possuir responsável e runbook.
    
- Observabilidade não pode bloquear o fluxo principal.
    
- A falha do backend de telemetria não pode derrubar a aplicação.
    
- Auditoria não deve depender apenas de logs técnicos.
    
- Estados exibidos ao usuário devem vir da persistência oficial.
    
- Deploys devem gerar marcadores.
    
- Custos devem ser monitorados desde o MVP.
    
- Dados de produção devem possuir acesso restrito.
    
- A retenção deve ser proporcional à utilidade e ao risco.
    
- Todo incidente crítico deve gerar análise posterior.
    

---

# Decisões Pendentes

As seguintes decisões deverão ser registradas em ADRs:

- Backend principal de observabilidade.
    
- Uso de Grafana Stack ou serviço gerenciado.
    
- Biblioteca de logging.
    
- Exportador OpenTelemetry.
    
- Estratégia de sampling.
    
- Política de retenção.
    
- Política de logging de prompts.
    
- Política de logging de respostas.
    
- SLO inicial da API.
    
- SLO inicial dos pipelines.
    
- Canais de alerta.
    
- Ferramenta de gestão de incidentes.
    
- Estratégia de synthetic monitoring.
    
- Estratégia de monitoramento de custos.
    
- Política de acesso aos logs.
    
- Estratégia de arquivamento.
    
- Ferramenta de error tracking.
    
- Política de mascaramento de dados.
    
- Estratégia de tracing em mensagens.
    

---

# Exemplo Completo

```text
Usuário inicia um pipeline
    ↓
API recebe a requisição
    ↓
Trace e CorrelationId são criados
    ↓
Application cria PipelineExecution
    ↓
Data salva execução e OutboxMessage
    ↓
Métrica pipeline_executions_total é incrementada
    ↓
Outbox publica StartPipelineCommand
    ↓
Trace context é propagado
    ↓
Worker consome a mensagem
    ↓
Span pipeline.execute é iniciado
    ↓
Trend Step é executado
    ↓
Span pipeline.step.execute é criado
    ↓
Provider externo é chamado
    ↓
Latência, unidades e custo são registrados
    ↓
Etapa é concluída
    ↓
Checkpoint é persistido
    ↓
Pipeline continua
    ↓
Script Agent falha por timeout
    ↓
Warning registra a retentativa
    ↓
Métrica provider_timeouts_total é incrementada
    ↓
Fallback é executado
    ↓
Pipeline é concluído
    ↓
Métrica pipeline_execution_duration_seconds é registrada
    ↓
Evento PipelineCompleted é publicado
    ↓
Dashboard é atualizado
```

Cenário de falha:

```text
Worker deixa de processar mensagens
    ↓
Heartbeat para de atualizar
    ↓
Queue depth aumenta
    ↓
Idade da mensagem mais antiga ultrapassa o limite
    ↓
Alerta High é disparado
    ↓
Runbook orienta a investigação
    ↓
Equipe verifica logs e traces
    ↓
Worker é recuperado
    ↓
Fila começa a diminuir
    ↓
Alerta é resolvido
```

---

# Objetivo Final

Criar um sistema cujo comportamento possa ser compreendido, medido e investigado.

O Infinite Content AI deverá permitir acompanhar uma operação desde a requisição inicial até a conclusão do pipeline, incluindo banco, mensagens, agentes, providers, custos, aprovações e publicações.

Falhas deverão ser detectáveis.

Custos deverão ser visíveis.

Gargalos deverão ser mensuráveis.

Incidentes deverão ser investigáveis.

A observabilidade deverá apoiar tanto a operação técnica quanto a evolução do produto.