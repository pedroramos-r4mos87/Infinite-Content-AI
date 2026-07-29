# Mensageria e Comunicação entre Componentes

## Objetivo

Definir como os componentes do Infinite Content AI trocarão informações, solicitarão operações, publicarão eventos e coordenarão processos distribuídos.

A comunicação deverá ser explícita, versionada, observável, segura e resiliente.

O sistema deverá evitar acoplamento direto entre componentes que possuem ciclos de vida, responsabilidades ou necessidades de escalabilidade diferentes.

A arquitetura deverá distinguir claramente:

- Comunicação síncrona.
    
- Comunicação assíncrona.
    
- Comandos.
    
- Eventos.
    
- Requests e responses.
    
- Mensagens internas.
    
- Webhooks.
    
- Notificações em tempo real.
    
- Transferência de arquivos.
    
- Integração com n8n.
    

---

# Princípios

A estratégia de comunicação seguirá os seguintes princípios:

- Comunicação síncrona somente quando uma resposta imediata for necessária.
    
- Processos longos deverão ser assíncronos.
    
- Mensagens deverão possuir contratos explícitos.
    
- Comandos expressam intenção.
    
- Eventos representam fatos.
    
- Consumidores deverão ser idempotentes.
    
- Mensagens poderão ser entregues mais de uma vez.
    
- Eventos críticos deverão utilizar Outbox.
    
- Mensagens recebidas importantes deverão utilizar Inbox.
    
- Contratos deverão ser versionados.
    
- Mensagens não deverão carregar segredos.
    
- Payloads deverão ser pequenos.
    
- Arquivos deverão ser referenciados, não transportados diretamente.
    
- Falhas deverão ser observáveis.
    
- O broker não será a fonte oficial de verdade.
    
- Comunicação distribuída utilizará consistência eventual.
    
- Componentes não deverão depender de implementações concretas de transporte.
    

---

# Tipos de Comunicação

O sistema utilizará quatro formas principais de comunicação:

```text
HTTP
Mensageria
Webhooks
Comunicação em tempo real
```

Cada uma deverá ser utilizada para um propósito específico.

---

# HTTP

HTTP será utilizado para:

- Chamadas do frontend para a API.
    
- Operações administrativas.
    
- Consultas.
    
- Comandos que exigem resposta imediata.
    
- Integrações síncronas com serviços externos.
    
- Comunicação síncrona com serviços internos.
    
- Callbacks OAuth.
    
- Recebimento de webhooks.
    

---

# Mensageria

Mensageria será utilizada para:

- Processamento em background.
    
- Execução de pipelines.
    
- Processamento de etapas longas.
    
- Publicação de eventos.
    
- Desacoplamento entre módulos.
    
- Retentativas assíncronas.
    
- Distribuição de trabalho.
    
- Integração com Workers.
    
- Processamento de Outbox.
    
- Atualização de projeções.
    
- Notificações internas.
    

---

# Webhooks

Webhooks serão utilizados quando sistemas externos precisarem notificar o Infinite Content AI.

Exemplos:

- Renderização concluída.
    
- Publicação processada.
    
- Vídeo publicado.
    
- Token revogado.
    
- Upload concluído.
    
- Evento do n8n.
    
- Mudança de status de plataforma.
    

Também poderão ser enviados pelo Infinite Content AI para sistemas externos.

---

# Comunicação em Tempo Real

SignalR ou Server-Sent Events poderão ser utilizados para:

- Atualizar status de pipeline.
    
- Exibir progresso.
    
- Informar aprovação pendente.
    
- Notificar conclusão.
    
- Informar falha.
    
- Atualizar custos.
    
- Exibir eventos operacionais para o usuário.
    

Esses canais não serão fonte oficial de verdade.

O estado oficial continuará no PostgreSQL.

---

# Comunicação Síncrona versus Assíncrona

## Comunicação Síncrona

Utilizar quando:

- O chamador precisa da resposta imediatamente.
    
- A operação é curta.
    
- A dependência está diretamente relacionada ao request.
    
- O erro precisa ser retornado ao usuário.
    
- Não existe processamento prolongado.
    

Exemplos:

- Consultar projeto.
    
- Validar configuração.
    
- Criar execução.
    
- Obter URL temporária.
    
- Consultar status.
    

---

## Comunicação Assíncrona

Utilizar quando:

- A operação é longa.
    
- Pode ser retomada.
    
- Pode sofrer retry.
    
- O chamador não precisa aguardar.
    
- Existe necessidade de escalabilidade.
    
- A dependência pode ficar indisponível temporariamente.
    
- O processo atravessa múltiplos componentes.
    

Exemplos:

- Gerar roteiro.
    
- Renderizar vídeo.
    
- Publicar conteúdo.
    
- Processar webhook.
    
- Executar pipeline.
    
- Sincronizar métricas.
    
- Enviar notificações.
    
- Processar Outbox.
    

---

# Regra para Processos Longos

A API não deverá permanecer aberta aguardando:

- Geração de vídeo.
    
- Execução completa de pipeline.
    
- Publicação em plataforma.
    
- Espera de aprovação humana.
    
- Renderização.
    
- Transcrição longa.
    
- Processamento externo.
    

Fluxo recomendado:

```text
Cliente solicita operação
    ↓
API valida e persiste intenção
    ↓
API publica comando
    ↓
API retorna 202 Accepted
    ↓
Worker processa
    ↓
Estado é atualizado
    ↓
Cliente consulta ou recebe notificação
```

---

# HTTP 202 Accepted

Operações assíncronas poderão retornar:

```text
202 Accepted
```

Com uma referência à execução.

Exemplo:

```json
{
  "executionId": "a8d82d56-7ac5-4e37-9de8-b8ed83ce41f9",
  "status": "Pending",
  "statusUrl": "/api/pipeline-executions/a8d82d56-7ac5-4e37-9de8-b8ed83ce41f9"
}
```

---

# Commands

Commands representam uma intenção de executar uma ação.

Exemplos:

```text
StartPipelineCommand
RetryPipelineStepCommand
CancelPipelineCommand
GenerateScriptCommand
PublishContentCommand
ProcessWebhookCommand
SynchronizePublicationMetricsCommand
```

Um command utiliza verbo no imperativo.

---

# Características de Commands

Um command:

- Solicita uma ação.
    
- Pode ser aceito ou rejeitado.
    
- Possui um responsável principal.
    
- Pode alterar estado.
    
- Pode falhar.
    
- Pode produzir eventos.
    
- Pode exigir idempotência.
    
- Não representa um fato ocorrido.
    

---

# Exemplo de Command

```csharp
public sealed record StartPipelineCommand(
    Guid MessageId,
    Guid OrganizationId,
    Guid PipelineExecutionId,
    Guid RequestedBy,
    DateTimeOffset RequestedAt,
    string CorrelationId);
```

---

# Command Handler

Um command deverá possuir um responsável claro.

Exemplo:

```text
StartPipelineCommand
    ↓
StartPipelineCommandHandler
```

Evitar múltiplos consumidores independentes executando a mesma ação de negócio.

Quando múltiplas reações forem necessárias, o handler deverá produzir eventos.

---

# Events

Events representam fatos que já aconteceram.

Exemplos:

```text
PipelineStarted
PipelineCompleted
PipelineFailed
ScriptGenerated
ScriptApproved
ContentPublished
PublicationFailed
ProviderFallbackUsed
```

O nome deverá estar no passado.

---

# Características de Events

Um evento:

- Representa um fato.
    
- Não pode ser rejeitado.
    
- Pode possuir múltiplos consumidores.
    
- Pode iniciar outros processos.
    
- Deve ser imutável.
    
- Deve possuir versão.
    
- Deve carregar o contexto mínimo necessário.
    

---

# Exemplo de Event

```csharp
public sealed record PipelineCompletedIntegrationEvent(
    Guid MessageId,
    Guid OrganizationId,
    Guid PipelineExecutionId,
    string PipelineName,
    int PipelineVersion,
    DateTimeOffset CompletedAt,
    string CorrelationId);
```

---

# Commands versus Events

```text
StartPipelineCommand
```

significa:

> Inicie o pipeline.

```text
PipelineStartedEvent
```

significa:

> O pipeline foi iniciado.

Commands expressam intenção.

Events expressam fatos.

---

# Domain Events

Domain Events representam fatos relevantes dentro do modelo de domínio.

Exemplos:

```text
PipelineExecutionStartedDomainEvent
ScriptApprovedDomainEvent
PublicationRequestedDomainEvent
```

Eles nascem no Domain.

---

# Integration Events

Integration Events representam fatos compartilhados com outros componentes.

Exemplos:

```text
PipelineStartedIntegrationEvent
ContentPublishedIntegrationEvent
```

Eles serão transportados por mensageria.

---

# Domain Event versus Integration Event

Nem todo Domain Event precisa virar Integration Event.

Fluxo possível:

```text
Domain Event
    ↓
Application Handler
    ↓
Decisão de integração
    ↓
Integration Event
    ↓
Outbox
```

Isso evita expor detalhes internos desnecessários.

---

# Estrutura dos Contracts

Contratos de mensagens deverão ficar no projeto:

```text
Contracts
```

Estrutura sugerida:

```text
Contracts/
├── Commands/
│   ├── Pipelines/
│   ├── Publications/
│   ├── Providers/
│   ├── Webhooks/
│   └── Analytics/
│
├── Events/
│   ├── Pipelines/
│   ├── Content/
│   ├── Publications/
│   ├── Providers/
│   └── Organizations/
│
├── Common/
│   ├── MessageEnvelope.cs
│   ├── MessageContext.cs
│   └── ContractVersions.cs
│
└── Serialization/
```

---

# Responsabilidade do Projeto Contracts

O projeto Contracts conterá:

- Commands externos entre processos.
    
- Integration Events.
    
- Payloads compartilhados.
    
- Envelopes de mensagem.
    
- Versionamento de contratos.
    
- Metadados comuns.
    

Não deverá conter:

- Regras de negócio.
    
- Entidades.
    
- DbContext.
    
- Handlers.
    
- Providers.
    
- Implementações de broker.
    

---

# SharedKernel versus Contracts

## SharedKernel

Contém elementos compartilhados de baixo acoplamento.

Exemplos:

- Result.
    
- Error.
    
- Identificadores.
    
- Abstrações fundamentais.
    
- Tipos pequenos e estáveis.
    

## Contracts

Contém mensagens trocadas entre processos.

Exemplos:

- Commands.
    
- Integration Events.
    
- Envelopes.
    
- Schemas.
    

---

# Envelope de Mensagem

Todas as mensagens poderão utilizar um envelope padronizado.

Exemplo:

```csharp
public sealed record MessageEnvelope<T>(
    Guid MessageId,
    string MessageType,
    int MessageVersion,
    DateTimeOffset CreatedAt,
    string CorrelationId,
    string? CausationId,
    Guid? OrganizationId,
    IReadOnlyDictionary<string, string>? Headers,
    T Payload);
```

---

# Campos Comuns

Mensagens deverão possuir, quando aplicável:

- MessageId.
    
- MessageType.
    
- MessageVersion.
    
- CreatedAt.
    
- CorrelationId.
    
- CausationId.
    
- OrganizationId.
    
- ActorId.
    
- TraceParent.
    
- TraceState.
    
- IdempotencyKey.
    
- Source.
    
- Headers.
    

---

# MessageId

Cada mensagem deverá possuir identificador único.

Objetivos:

- Inbox.
    
- Deduplicação.
    
- Auditoria.
    
- Rastreamento.
    
- DLQ.
    
- Reprocessamento.
    

---

# CorrelationId

Agrupa mensagens pertencentes ao mesmo fluxo.

Exemplo:

```text
Request HTTP
    ↓
StartPipelineCommand
    ↓
PipelineStarted
    ↓
GenerateScriptCommand
    ↓
ScriptGenerated
```

Todas poderão compartilhar o mesmo `CorrelationId`.

---

# CausationId

Aponta para a mensagem ou ação que causou a mensagem atual.

Exemplo:

```text
StartPipelineCommand
MessageId: A

PipelineStarted
MessageId: B
CausationId: A
```

---

# Message Type

O tipo deverá ser estável.

Exemplo:

```text
pipelines.start
pipelines.started
pipelines.completed
publications.publish
publications.published
```

Evitar depender exclusivamente do nome completo da classe .NET.

---

# Message Version

Toda mensagem deverá possuir versão explícita.

Exemplo:

```text
pipelines.started.v1
```

ou:

```json
{
  "messageType": "pipelines.started",
  "messageVersion": 1
}
```

---

# Imutabilidade

Mensagens publicadas deverão ser tratadas como imutáveis.

Uma mensagem antiga não deverá mudar de significado.

Caso o contrato precise mudar, uma nova versão deverá ser criada.

---

# Compatibilidade

Alterações compatíveis incluem:

- Adicionar campo opcional.
    
- Adicionar header opcional.
    
- Adicionar novo valor que consumidores tolerem.
    
- Expandir metadados sem alterar significado.
    

Alterações incompatíveis incluem:

- Remover campo obrigatório.
    
- Renomear campo.
    
- Alterar tipo.
    
- Alterar significado.
    
- Tornar opcional um comportamento crítico.
    
- Alterar unidade de medida.
    

---

# Estratégia de Versionamento

Possíveis estratégias:

## Nova versão do contrato

```text
PipelineStartedV2
```

## Campo de versão

```text
MessageVersion: 2
```

## Novo tópico ou routing key

```text
pipelines.started.v2
```

A estratégia deverá ser registrada em ADR.

---

# Consumidores Tolerantes

Consumidores deverão:

- Ignorar campos desconhecidos.
    
- Validar campos obrigatórios.
    
- Rejeitar versões não suportadas.
    
- Não depender da ordem dos campos.
    
- Não depender de valores opcionais sempre presentes.
    

---

# Schema Registry

Um schema registry poderá ser adotado no futuro.

Possíveis formatos:

- JSON Schema.
    
- Avro.
    
- Protobuf.
    
- AsyncAPI.
    

Ele poderá ajudar em:

- Compatibilidade.
    
- Validação.
    
- Documentação.
    
- Geração de código.
    
- Governança.
    

Não será obrigatório no MVP.

---

# Serialização

A estratégia inicial poderá utilizar:

```text
JSON
```

Benefícios:

- Simplicidade.
    
- Legibilidade.
    
- Integração com n8n.
    
- Facilidade de diagnóstico.
    
- Compatibilidade ampla.
    

---

# Configuração de JSON

A serialização deverá ser consistente.

Decisões:

- camelCase.
    
- Datas em ISO 8601.
    
- UTC.
    
- Enums como string.
    
- Campos opcionais explícitos.
    
- Nulos controlados.
    
- Tamanho limitado.
    

---

# Exemplo de Payload

```json
{
  "messageId": "0f6ca3fa-c090-4ed5-b9f0-3cb57267781e",
  "messageType": "pipelines.started",
  "messageVersion": 1,
  "createdAt": "2026-07-23T16:20:00Z",
  "correlationId": "8ee574c7ef5c4e63940b9ab773f18674",
  "organizationId": "332f6893-86ee-45ae-af30-5c84a05a39e2",
  "payload": {
    "pipelineExecutionId": "93d59dcf-3fa4-4d95-82ea-7093fba71a36",
    "pipelineName": "YouTubeLongForm",
    "pipelineVersion": 3
  }
}
```

---

# Tamanho das Mensagens

Mensagens deverão ser pequenas.

Evitar transportar:

- Vídeos.
    
- Áudios.
    
- Imagens.
    
- Documentos grandes.
    
- Prompts completos extensos.
    
- Respostas completas de providers.
    
- Binários.
    
- Grandes coleções.
    

---

# Claim Check Pattern

Para payloads grandes, utilizar referência externa.

Fluxo:

```text
Arquivo ou payload grande
    ↓
Storage
    ↓
FileReference
    ↓
Mensagem transporta a referência
```

Exemplo:

```json
{
  "artifactId": "ba06fdd7-2a58-4491-b0b5-fcb7cc747fc1",
  "storageReference": "artifacts/ba06fdd7-2a58-4491-b0b5-fcb7cc747fc1"
}
```

---

# Dados Sensíveis em Mensagens

Mensagens não deverão conter:

- Senhas.
    
- API keys.
    
- Refresh tokens.
    
- Access tokens.
    
- Connection strings.
    
- Chaves privadas.
    
- Segredos de webhook.
    
- Credenciais de providers.
    
- Dados pessoais desnecessários.
    

Preferir:

- SecretReference.
    
- ConnectionId.
    
- ArtifactId.
    
- ResourceId.
    

---

# Abstrações de Mensageria

A Application não deverá depender do broker concreto.

Exemplos de abstrações:

```csharp
public interface IMessagePublisher
{
    Task PublishAsync<TMessage>(
        TMessage message,
        CancellationToken cancellationToken);
}
```

```csharp
public interface ICommandSender
{
    Task SendAsync<TCommand>(
        TCommand command,
        CancellationToken cancellationToken);
}
```

---

# Implementações

As implementações ficarão em Infrastructure.

Estrutura sugerida:

```text
Infrastructure/
└── Messaging/
    ├── Abstractions/
    ├── RabbitMq/
    │   ├── RabbitMqConnection.cs
    │   ├── RabbitMqPublisher.cs
    │   ├── RabbitMqConsumer.cs
    │   ├── RabbitMqTopology.cs
    │   └── RabbitMqOptions.cs
    │
    ├── Serialization/
    ├── Consumers/
    ├── Routing/
    ├── Resilience/
    ├── Observability/
    └── DependencyInjection.cs
```

---

# Escolha do Broker

A opção inicial sugerida poderá ser:

```text
RabbitMQ
```

Motivos:

- Filas.
    
- Routing.
    
- Dead Letter Exchanges.
    
- Retentativas.
    
- Confirmação de publicação.
    
- Bom suporte no ecossistema .NET.
    
- Adequado para jobs e commands.
    
- Operação relativamente simples.
    

Alternativas futuras:

- Azure Service Bus.
    
- Amazon SQS e SNS.
    
- Kafka.
    
- Google Pub/Sub.
    
- NATS.
    
- MassTransit sobre brokers suportados.
    

A decisão deverá ser registrada em ADR.

---

# RabbitMQ

No RabbitMQ, a arquitetura deverá considerar:

- Exchanges.
    
- Queues.
    
- Routing keys.
    
- Bindings.
    
- Dead Letter Exchanges.
    
- Durabilidade.
    
- Publisher confirms.
    
- Consumer acknowledgements.
    
- Prefetch.
    
- Retry.
    
- Quorum queues, quando apropriado.
    

---

# Exchange

Exchanges recebem mensagens e realizam roteamento.

Tipos relevantes:

- Direct.
    
- Topic.
    
- Fanout.
    
- Headers.
    

A recomendação inicial é utilizar:

```text
Topic Exchange
```

para Integration Events.

---

# Filas de Commands

Commands deverão ser direcionados para uma fila específica do consumidor responsável.

Exemplo:

```text
Command:
pipelines.start

Queue:
pipeline-execution-worker

Routing Key:
commands.pipelines.start.v1
```

---

# Filas de Events

Eventos poderão ser enviados para diferentes filas.

Exemplo:

```text
Event:
pipelines.completed

Queues:
analytics-pipeline-events
notification-pipeline-events
billing-pipeline-events
```

Cada consumidor possui sua própria fila.

---

# Topologia Sugerida

```text
Exchange: infinite-content.commands
Exchange: infinite-content.events
Exchange: infinite-content.dead-letter
```

Exemplo:

```text
infinite-content.commands
    └── commands.pipelines.start.v1
            ↓
       pipeline-execution-worker

infinite-content.events
    └── events.pipelines.completed.v1
            ├── analytics-worker
            ├── notification-worker
            └── audit-worker
```

---

# Nomes de Filas

Nomes deverão ser claros e estáveis.

Exemplos:

```text
pipeline-execution-worker
publication-worker
provider-callback-worker
analytics-projection-worker
webhook-processing-worker
outbox-publisher
```

Evitar nomes genéricos como:

```text
queue1
worker-events
messages
```

---

# Routing Keys

Convenção sugerida:

```text
commands.{context}.{action}.v{version}
events.{context}.{event}.v{version}
```

Exemplos:

```text
commands.pipelines.start.v1
commands.publications.publish.v1
events.pipelines.completed.v1
events.publications.failed.v1
```

---

# Durabilidade

Mensagens críticas deverão utilizar:

- Exchange durável.
    
- Queue durável.
    
- Mensagem persistente.
    
- Confirmação de publicação.
    

Isso reduz risco de perda após reinício do broker.

---

# Publisher Confirms

O publisher deverá confirmar que o broker aceitou a mensagem.

Uma chamada de publish não deverá ser considerada concluída apenas porque os dados foram enviados pela conexão.

---

# Acknowledgement

O consumidor deverá confirmar a mensagem somente após processamento seguro.

Fluxo:

```text
Mensagem recebida
    ↓
Inbox registrada
    ↓
Estado persistido
    ↓
Processamento concluído
    ↓
Acknowledgement
```

A ordem exata dependerá da estratégia transacional.

---

# Auto-Ack

Auto-ack não deverá ser utilizado em operações críticas.

Risco:

```text
Broker entrega
    ↓
Mensagem é automaticamente confirmada
    ↓
Worker falha
    ↓
Mensagem é perdida
```

---

# Prefetch

Prefetch controla quantas mensagens um consumer recebe antes de confirmar.

Deverá ser configurado conforme:

- Duração da tarefa.
    
- Memória.
    
- Concorrência.
    
- Tipo de operação.
    
- Capacidade externa.
    
- Custo.
    

Exemplo:

```text
Pipeline commands: prefetch baixo
Analytics events: prefetch maior
Video rendering: prefetch muito baixo
```

---

# Concorrência de Consumers

A concorrência deverá ser limitada por fila e operação.

Exemplos:

```text
pipeline-execution-worker: 10
publication-worker: 5
video-rendering-worker: 2
analytics-worker: 20
```

Esses valores deverão ser configuráveis.

---

# Consumer

Cada consumer deverá:

- Validar envelope.
    
- Validar versão.
    
- Aplicar tracing.
    
- Criar logging scope.
    
- Verificar Inbox.
    
- Aplicar idempotência.
    
- Executar handler.
    
- Persistir resultado.
    
- Confirmar ou rejeitar.
    
- Registrar métricas.
    

---

# Estrutura de Consumer

Exemplo conceitual:

```csharp
public sealed class StartPipelineConsumer
{
    public async Task ConsumeAsync(
        StartPipelineCommand command,
        CancellationToken cancellationToken)
    {
        // Validar contrato
        // Verificar Inbox
        // Criar contexto
        // Executar Application
        // Persistir resultado
        // Confirmar processamento
    }
}
```

O consumer deverá permanecer fino.

A regra de negócio ficará na Application.

---

# Middleware de Mensageria

Comportamentos transversais poderão ser aplicados por pipeline de consumo.

Exemplos:

- Logging.
    
- Tracing.
    
- Validation.
    
- Inbox.
    
- Retry.
    
- Métricas.
    
- Autorização técnica.
    
- Tenant context.
    
- Exception handling.
    

Fluxo:

```text
Message
    ↓
Deserialization
    ↓
Validation
    ↓
Trace Context
    ↓
Tenant Context
    ↓
Inbox
    ↓
Handler
    ↓
Metrics
    ↓
Ack
```

---

# Outbox Pattern

A Outbox protege a consistência entre banco e broker.

Problema:

```text
Banco confirmado
    ↓
Publicação no broker falha
```

Sem Outbox, o estado foi alterado, mas o evento não foi publicado.

---

# Fluxo da Outbox

```text
Application altera estado
    ↓
Integration Event é criado
    ↓
Estado e OutboxMessage são persistidos
    ↓
Commit único no PostgreSQL
    ↓
Outbox Processor lê mensagens pendentes
    ↓
Publica no broker
    ↓
Marca como processada
```

---

# Estrutura da OutboxMessage

Campos sugeridos:

```text
id
message_type
message_version
payload
headers
occurred_at
processed_at
attempt_count
next_attempt_at
last_error
correlation_id
causation_id
organization_id
locked_by
locked_until
```

---

# Criação da Outbox

A Outbox poderá ser criada por:

- Handler da Application.
    
- Handler de Domain Event.
    
- Interceptor do EF Core.
    
- Unit of Work.
    

A estratégia deverá ser consistente.

---

# Serialização da Outbox

O payload persistido deverá conter o contrato que será publicado.

Cuidados:

- Versionamento.
    
- Tipo estável.
    
- Compatibilidade.
    
- Tamanho.
    
- Segurança.
    
- Falha de serialização.
    

---

# Outbox Processor

O processador deverá:

1. Buscar mensagens pendentes.
    
2. Reservar um lote.
    
3. Publicar.
    
4. Aguardar confirmação do broker.
    
5. Marcar como processada.
    
6. Registrar falhas.
    
7. Reagendar mensagens transitórias.
    
8. Alertar falhas permanentes.
    

---

# Concorrência da Outbox

Múltiplas instâncias poderão processar a Outbox.

Estratégias possíveis:

- `FOR UPDATE SKIP LOCKED`.
    
- Lease.
    
- LockedBy.
    
- LockedUntil.
    
- Particionamento.
    

Exemplo:

```sql
SELECT *
FROM outbox_messages
WHERE processed_at IS NULL
  AND next_attempt_at <= now()
ORDER BY occurred_at
FOR UPDATE SKIP LOCKED
LIMIT 100;
```

---

# Publicação Duplicada da Outbox

Mesmo com Outbox, uma mensagem poderá ser publicada mais de uma vez.

Exemplo:

```text
Mensagem publicada
    ↓
Processo falha antes de marcar ProcessedAt
    ↓
Mensagem é publicada novamente
```

Consumidores deverão continuar idempotentes.

---

# Retenção da Outbox

Mensagens processadas poderão ser:

- Mantidas por período curto.
    
- Arquivadas.
    
- Excluídas por job.
    
- Agregadas para auditoria.
    

A política deverá considerar:

- Diagnóstico.
    
- Compliance.
    
- Volume.
    
- Custo.
    
- Reprocessamento.
    

---

# Inbox Pattern

A Inbox protege o consumidor contra mensagens duplicadas.

Fluxo:

```text
Mensagem recebida
    ↓
Verificar MessageId e Consumer
    ↓
Já processada?
    ├── Sim → confirmar
    └── Não → registrar processamento
```

---

# Estrutura da Inbox

Campos sugeridos:

```text
message_id
consumer
message_type
message_version
received_at
processed_at
status
attempt_count
last_error
correlation_id
organization_id
```

Constraint recomendada:

```text
unique(message_id, consumer)
```

---

# Inbox e Transação

Quando possível, o registro da Inbox e a alteração de estado deverão ocorrer na mesma transação local.

Fluxo:

```text
Abrir transação
    ↓
Registrar Inbox
    ↓
Alterar estado
    ↓
Salvar Outbox
    ↓
Commit
    ↓
Ack
```

---

# Entrega pelo Menos Uma Vez

A arquitetura deverá assumir:

```text
At-least-once delivery
```

Consequências:

- Mensagens duplicadas são possíveis.
    
- Consumers devem ser idempotentes.
    
- Commands críticos precisam de idempotency key.
    
- Publicações externas precisam de reconciliação.
    
- Ack deve ocorrer somente após processamento seguro.
    

---

# Exactly Once

Garantia absoluta de exactly-once entre múltiplos sistemas não será assumida.

O sistema buscará um efeito equivalente por meio de:

- Idempotência.
    
- Outbox.
    
- Inbox.
    
- Constraints únicas.
    
- Reconciliação.
    
- Estado persistido.
    
- Deduplicação.
    

---

# Retry de Mensagens

Falhas transitórias poderão gerar retry.

Estratégias:

- Requeue imediato.
    
- Delay queue.
    
- Retry exchange.
    
- Scheduled message.
    
- Reagendamento no banco.
    
- Parking queue.
    

Retry imediato repetido deverá ser evitado.

---

# Retry com Delay

Fluxo:

```text
Consumer falha
    ↓
Erro transitório
    ↓
Mensagem enviada para fila de retry
    ↓
TTL expira
    ↓
Mensagem retorna à fila original
```

---

# Filas de Retry

Exemplo:

```text
pipeline-execution-worker.retry.30s
pipeline-execution-worker.retry.5m
pipeline-execution-worker.retry.30m
```

A estratégia deverá evitar topologias excessivamente complexas.

---

# Máximo de Tentativas

Cada tipo de mensagem deverá possuir limite.

Exemplo:

```text
StartPipelineCommand: 5
PublishContentCommand: 5
ProcessWebhookCommand: 3
SynchronizeMetricsCommand: 8
```

O limite deverá considerar:

- Custo.
    
- Idempotência.
    
- Probabilidade de recuperação.
    
- Tempo de negócio.
    
- Dependência externa.
    

---

# Dead Letter Queue

Mensagens irrecuperáveis deverão ser enviadas para DLQ.

Exemplo:

```text
pipeline-execution-worker.dlq
publication-worker.dlq
webhook-processing-worker.dlq
```

---

# DLQ

A DLQ deverá armazenar:

- Mensagem original.
    
- Headers.
    
- MessageId.
    
- Tipo.
    
- Versão.
    
- Quantidade de tentativas.
    
- Erro final.
    
- CorrelationId.
    
- Consumer.
    
- Horário.
    
- Fila original.
    

---

# Tratamento da DLQ

A DLQ deverá possuir:

- Dashboard.
    
- Alertas.
    
- Ferramenta de inspeção.
    
- Processo de reprocessamento.
    
- Registro de responsável.
    
- Histórico de ações.
    

Não deverá ser ignorada.

---

# Parking Queue

Uma parking queue poderá ser utilizada para mensagens que aguardam ação externa.

Exemplos:

- Credencial expirada.
    
- Organização suspensa.
    
- Aprovação pendente.
    
- Provider indisponível por longo período.
    
- Configuração incompleta.
    

---

# Poison Messages

Mensagens que falham permanentemente deverão ser isoladas.

Exemplos:

- JSON inválido.
    
- Versão não suportada.
    
- Campo obrigatório ausente.
    
- Tipo desconhecido.
    
- Payload incompatível.
    

Elas não deverão bloquear a fila principal.

---

# Ordenação

A arquitetura não deverá assumir ordenação global de mensagens.

Mesmo quando o broker preserva ordem em uma fila, múltiplos consumers podem alterar o comportamento.

Quando a ordem for necessária, utilizar:

- Particionamento por aggregate.
    
- Uma fila por chave.
    
- Consumer único por recurso.
    
- Sequence number.
    
- Versionamento de aggregate.
    
- Controle de estado.
    

---

# Sequence Number

Mensagens relacionadas a um mesmo aggregate poderão possuir:

```text
AggregateVersion
```

Exemplo:

```json
{
  "pipelineExecutionId": "...",
  "aggregateVersion": 12
}
```

O consumidor poderá detectar eventos fora de ordem.

---

# Eventos Fora de Ordem

Ao receber uma mensagem antiga:

- Ignorar se o estado já avançou.
    
- Registrar como duplicada.
    
- Reagendar se uma versão anterior estiver ausente.
    
- Consultar o estado oficial.
    
- Executar reconciliação.
    

---

# Particionamento

Filas poderão ser particionadas por:

- OrganizationId.
    
- PipelineExecutionId.
    
- ProjectId.
    
- Platform.
    
- Tipo de trabalho.
    

A estratégia deverá equilibrar:

- Ordem.
    
- Escalabilidade.
    
- Complexidade.
    
- Isolamento.
    

---

# Concorrência por Aggregate

Para evitar dois consumers alterando a mesma execução:

- Lease.
    
- Concorrência otimista.
    
- Particionamento.
    
- Lock distribuído, quando necessário.
    
- Idempotência.
    
- Verificação de estado.
    

---

# Mensagens Agendadas

O sistema poderá precisar publicar mensagens no futuro.

Exemplos:

- Publicação agendada.
    
- Retry futuro.
    
- Expiração de aprovação.
    
- Sincronização periódica.
    
- Verificação de job externo.
    
- Reconciliação.
    

---

# Estratégias de Agendamento

Possibilidades:

- Scheduler interno.
    
- Tabela de jobs.
    
- Delayed messages.
    
- Hangfire.
    
- Quartz.
    
- Broker com suporte a scheduling.
    
- n8n.
    

A fonte oficial do agendamento deverá estar persistida.

---

# Scheduler Interno

Um scheduler poderá buscar registros com:

```text
scheduled_at <= now()
status = Pending
```

Depois:

- Reservar.
    
- Publicar command.
    
- Atualizar estado.
    
- Renovar lease quando necessário.
    

---

# Agendamento não Deve Depender Apenas do Broker

Para operações críticas, o agendamento deverá ser persistido no banco.

Isso permite:

- Auditoria.
    
- Consulta.
    
- Cancelamento.
    
- Recuperação.
    
- Reconciliação.
    
- Alteração de horário.
    

---

# Comunicação API para Worker

Fluxo recomendado:

```text
API
    ↓
Application
    ↓
Persistência
    ↓
Outbox
    ↓
Broker
    ↓
Worker
```

A API não deverá publicar diretamente no broker antes do commit do banco para eventos críticos.

---

# Comunicação Worker para API

O Worker não deverá chamar a API interna apenas para atualizar estado.

Preferir:

```text
Worker
    ↓
Application
    ↓
Data
```

A API e o Worker utilizam a mesma camada de Application.

---

# Comunicação entre Workers

Workers não deverão chamar uns aos outros diretamente.

Preferir:

- Commands.
    
- Events.
    
- Estado persistido.
    
- Broker.
    

Exemplo:

```text
Pipeline Worker
    ↓
RenderVideoCommand
    ↓
Media Worker
    ↓
VideoRenderedEvent
    ↓
Pipeline Worker continua
```

---

# Serviço Python

O serviço Python poderá se comunicar por:

- HTTP síncrono.
    
- Mensageria.
    
- Job assíncrono.
    

Para tarefas longas, preferir job assíncrono.

Fluxo:

```text
Worker publica RenderVideoCommand
    ↓
Serviço Python inicia job
    ↓
JobId é persistido
    ↓
Serviço processa
    ↓
Publica VideoRenderedEvent
```

---

# HTTP Interno

Quando HTTP interno for utilizado:

- Autenticar.
    
- Utilizar TLS.
    
- Definir timeout.
    
- Aplicar retry seletivo.
    
- Propagar tracing.
    
- Validar response.
    
- Utilizar contratos explícitos.
    
- Evitar chamadas circulares.
    

---

# Evitar Cadeias Síncronas Longas

Exemplo ruim:

```text
API
    ↓
Worker Service
    ↓
Python Service
    ↓
Storage Service
    ↓
Provider
```

Uma falha ou lentidão em qualquer componente afeta toda a cadeia.

Preferir assíncrono para processos longos.

---

# n8n

O n8n será tratado como plataforma externa de automação.

Ele poderá:

- Receber eventos.
    
- Iniciar workflows.
    
- Integrar serviços externos.
    
- Executar automações administrativas.
    
- Coordenar tarefas não centrais.
    
- Notificar pessoas.
    
- Integrar ferramentas SaaS.
    

---

# O que o n8n não Deve Fazer

O n8n não deverá:

- Acessar diretamente o banco de produção.
    
- Ser a fonte oficial do estado.
    
- Conter regras centrais de domínio.
    
- Armazenar segredos sem proteção adequada.
    
- Controlar sozinho a consistência de pipelines.
    
- Alterar estado crítico sem passar pela Application.
    
- Publicar conteúdo sem autorização e idempotência.
    

---

# Comunicação com n8n

Possibilidades:

```text
Infinite Content AI
    ↓
Webhook assinado
    ↓
n8n
```

ou:

```text
n8n
    ↓
API autenticada
    ↓
Infinite Content AI
```

ou:

```text
Broker
    ↓
Adapter
    ↓
n8n
```

---

# Adapter para n8n

A comunicação com n8n deverá ficar em Infrastructure.

Exemplo:

```text
Infrastructure/
└── N8n/
    ├── N8nClient.cs
    ├── N8nWebhookPublisher.cs
    ├── N8nOptions.cs
    └── DependencyInjection.cs
```

---

# Webhooks para n8n

Webhooks enviados deverão possuir:

- HTTPS.
    
- Assinatura.
    
- Timestamp.
    
- MessageId.
    
- CorrelationId.
    
- Versão.
    
- Timeout.
    
- Retry.
    
- Idempotência.
    
- Payload mínimo.
    

---

# Webhooks Recebidos

Todo webhook recebido deverá:

1. Validar Content-Type.
    
2. Limitar tamanho.
    
3. Validar assinatura.
    
4. Validar timestamp.
    
5. Validar schema.
    
6. Verificar MessageId.
    
7. Registrar Inbox.
    
8. Responder rapidamente.
    
9. Processar em background.
    
10. Registrar auditoria.
    

---

# Resposta de Webhook

O endpoint deverá responder rapidamente.

Fluxo:

```text
Webhook recebido
    ↓
Validação
    ↓
Persistência
    ↓
Command publicado
    ↓
HTTP 202 ou 200
```

O processamento longo não deverá ocorrer dentro do request.

---

# Assinatura

Exemplo:

```text
HMAC-SHA256(
    timestamp + "." + payload,
    secret
)
```

Headers possíveis:

```text
X-Webhook-Id
X-Webhook-Timestamp
X-Webhook-Signature
```

---

# Replay

Webhooks antigos ou duplicados deverão ser rejeitados ou tratados idempotentemente.

Controles:

- Timestamp.
    
- MessageId.
    
- Inbox.
    
- Janela de validade.
    
- Assinatura.
    
- Nonce, quando necessário.
    

---

# Webhooks de Saída

A aplicação poderá expor integração por webhooks para clientes.

Eventos possíveis:

```text
pipeline.started
pipeline.completed
pipeline.failed
content.approved
publication.completed
publication.failed
```

---

# Webhook Subscription

Estrutura possível:

```text
webhook_subscriptions
├── id
├── organization_id
├── url
├── secret_reference
├── status
├── event_types
├── created_at
└── updated_at
```

---

# Webhook Delivery

Cada tentativa deverá ser persistida.

```text
webhook_deliveries
├── id
├── subscription_id
├── event_id
├── attempt
├── status
├── response_status_code
├── next_attempt_at
├── delivered_at
└── last_error
```

---

# Retry de Webhooks

Webhooks de saída deverão utilizar retry limitado.

Exemplo:

```text
1 minuto
5 minutos
30 minutos
2 horas
12 horas
```

O cliente deverá receber o mesmo `MessageId` em todas as tentativas.

---

# Desativação de Subscription

Uma subscription poderá ser desativada após:

- Falhas permanentes.
    
- HTTP 410.
    
- Quantidade excessiva de falhas.
    
- Secret inválido.
    
- URL insegura.
    
- Solicitação do cliente.
    

A desativação deverá gerar auditoria.

---

# SignalR

SignalR poderá ser utilizado para comunicação bidirecional em tempo real.

Casos:

- Progresso de pipeline.
    
- Atualização de status.
    
- Aprovações.
    
- Notificações.
    
- Feedback de execução.
    

---

# Server-Sent Events

SSE poderá ser utilizado quando apenas servidor para cliente for necessário.

Benefícios:

- Simplicidade.
    
- Reconexão nativa.
    
- Adequado para status.
    
- Funciona sobre HTTP.
    

---

# Escolha entre SignalR e SSE

## SignalR

Utilizar quando:

- Comunicação bidirecional.
    
- Grupos.
    
- Presença.
    
- Alta interação.
    
- Múltiplos transportes.
    

## SSE

Utilizar quando:

- Fluxo unidirecional.
    
- Atualizações simples.
    
- Timeline.
    
- Progresso.
    
- Menor complexidade.
    

---

# Estado em Tempo Real

Mensagens em tempo real deverão conter apenas projeções de estado.

Exemplo:

```json
{
  "type": "pipeline.step.completed",
  "pipelineExecutionId": "...",
  "stepName": "GenerateScript",
  "status": "Succeeded",
  "occurredAt": "2026-07-23T16:40:00Z"
}
```

---

# Reconexão do Cliente

Ao reconectar, o cliente deverá consultar o estado oficial.

Não deverá depender de receber todos os eventos em tempo real.

Fluxo:

```text
Cliente reconecta
    ↓
Consulta status atual
    ↓
Assina atualizações futuras
```

---

# Autorização em Tempo Real

Um usuário somente poderá assinar atualizações de recursos autorizados.

Grupos poderão ser definidos por:

```text
organization:{organizationId}
pipeline:{pipelineExecutionId}
project:{projectId}
```

A autorização deverá ser verificada antes de adicionar o cliente ao grupo.

---

# API Contracts

Requests e responses HTTP deverão ser separados das entidades.

Estrutura sugerida:

```text
Api/
└── Contracts/
    ├── Requests/
    ├── Responses/
    └── Mappings/
```

Não reutilizar diretamente Integration Events como responses HTTP.

---

# Contratos Internos versus Externos

## Internos

Utilizados entre componentes controlados.

Podem evoluir de forma coordenada.

## Externos

Expostos para clientes e integrações.

Exigem:

- Compatibilidade.
    
- Documentação.
    
- Versionamento.
    
- Política de depreciação.
    
- Segurança.
    

---

# API Versioning

A API poderá utilizar:

```text
/api/v1/
```

ou versão por header.

A estratégia deverá ser registrada em ADR.

---

# AsyncAPI

A mensageria poderá ser documentada com AsyncAPI.

A documentação poderá incluir:

- Channels.
    
- Messages.
    
- Producers.
    
- Consumers.
    
- Schemas.
    
- Segurança.
    
- Exemplos.
    
- Versões.
    

---

# OpenAPI

A comunicação HTTP deverá ser documentada com OpenAPI.

Isso permitirá:

- Clientes.
    
- Testes.
    
- Documentação.
    
- Validação.
    
- Governança.
    

---

# Contratos e Testes

Todo contrato relevante deverá possuir testes.

Tipos:

- Serialization tests.
    
- Deserialization tests.
    
- Schema validation.
    
- Compatibility tests.
    
- Consumer-driven contract tests.
    
- Version tests.
    
- Round-trip tests.
    

---

# Contract Test

Exemplo:

```text
Producer publica PipelineCompletedV1
    ↓
Consumer consegue desserializar
    ↓
Campos obrigatórios são preservados
```

---

# Consumer-Driven Contract Tests

Consumidores poderão declarar quais campos realmente utilizam.

Isso ajuda a evitar mudanças incompatíveis.

Ferramentas como Pact poderão ser avaliadas para HTTP.

Para mensageria, contratos poderão ser validados por schemas e fixtures.

---

# Fixtures de Mensagens

O projeto deverá manter exemplos versionados.

Estrutura:

```text
Contracts.Tests/
└── Fixtures/
    ├── pipelines.started.v1.json
    ├── pipelines.completed.v1.json
    └── publications.published.v1.json
```

---

# Observabilidade

Toda publicação e consumo deverá registrar:

- MessageId.
    
- MessageType.
    
- MessageVersion.
    
- CorrelationId.
    
- CausationId.
    
- Queue.
    
- Exchange.
    
- RoutingKey.
    
- Consumer.
    
- Attempt.
    
- Duration.
    
- Status.
    
- ErrorCode.
    

---

# Métricas

Métricas recomendadas:

```text
messages_published_total
messages_publish_failures_total
messages_consumed_total
messages_processing_failures_total
message_processing_duration_seconds
message_retries_total
message_duplicates_total
dead_letter_messages_total
queue_depth
oldest_message_age_seconds
outbox_pending_messages
outbox_oldest_message_age_seconds
```

---

# Tracing

Spans recomendados:

```text
messaging.publish
messaging.consume
outbox.process
webhook.receive
webhook.deliver
realtime.notify
```

---

# Segurança do Broker

O broker deverá:

- Utilizar autenticação.
    
- Utilizar TLS.
    
- Não ficar exposto publicamente.
    
- Possuir credenciais por serviço.
    
- Aplicar menor privilégio.
    
- Separar ambientes.
    
- Restringir filas e exchanges.
    
- Registrar acessos.
    
- Rotacionar credenciais.
    

---

# Permissões por Serviço

Exemplo:

## API

Pode:

- Publicar commands.
    
- Publicar events via Outbox Processor, se fizer parte do processo.
    

## Pipeline Worker

Pode:

- Consumir commands de pipeline.
    
- Publicar events de pipeline.
    

## Publication Worker

Pode:

- Consumir commands de publicação.
    
- Publicar events de publicação.
    

Não deverá possuir acesso irrestrito a todas as filas.

---

# Ambientes

Cada ambiente deverá possuir isolamento.

Exemplos:

```text
dev.infinite-content.commands
staging.infinite-content.commands
prod.infinite-content.commands
```

Ou brokers separados.

Produção nunca deverá compartilhar fila com desenvolvimento.

---

# Configuração

Exemplo conceitual:

```json
{
  "Messaging": {
    "Provider": "RabbitMQ",
    "Host": "",
    "VirtualHost": "/",
    "Username": "",
    "Password": "",
    "PrefetchCount": 10,
    "PublisherConfirmTimeoutSeconds": 10
  }
}
```

Credenciais deverão vir de Secret Manager.

---

# Dependency Injection

Exemplo conceitual:

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<MessagingOptions>()
            .BindConfiguration("Messaging")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IMessageConnection, RabbitMqConnection>();
        services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

        return services;
    }
}
```

---

# Uso de Biblioteca

A implementação poderá utilizar:

- Cliente oficial do RabbitMQ.
    
- MassTransit.
    
- Wolverine.
    
- NServiceBus.
    
- Rebus.
    

A escolha deverá considerar:

- Complexidade.
    
- Outbox.
    
- Retry.
    
- Saga.
    
- Observabilidade.
    
- Licença.
    
- Portabilidade.
    
- Curva de aprendizagem.
    

A decisão deverá ser registrada em ADR.

---

# MassTransit

MassTransit poderá simplificar:

- Consumers.
    
- Retry.
    
- Outbox.
    
- Routing.
    
- Sagas.
    
- Observabilidade.
    
- Integração com brokers.
    

Porém, adiciona abstrações e convenções próprias.

A equipe deverá avaliar se o ganho compensa a dependência.

---

# Implementação Própria

Uma implementação própria com cliente oficial oferece:

- Maior controle.
    
- Menos abstrações.
    
- Aprendizado explícito.
    

Porém exige implementar corretamente:

- Conexões.
    
- Reconexão.
    
- Confirms.
    
- Consumers.
    
- Topologia.
    
- Retry.
    
- DLQ.
    
- Observabilidade.
    
- Shutdown gracioso.
    

---

# Shutdown Gracioso

Ao encerrar um Worker:

1. Parar de receber novas mensagens.
    
2. Aguardar processamento atual dentro do limite.
    
3. Cancelar operações quando necessário.
    
4. Não confirmar mensagens incompletas.
    
5. Liberar conexões.
    
6. Atualizar heartbeat.
    

---

# Backpressure

O sistema deverá limitar a entrada quando consumidores não acompanharem o volume.

Estratégias:

- Prefetch.
    
- Limite de concorrência.
    
- Rate limiting.
    
- Quotas.
    
- Fila.
    
- Rejeição controlada.
    
- Escalabilidade horizontal.
    
- Circuit breaker.
    

---

# Broker como Buffer

O broker absorve picos temporários.

Ele não substitui planejamento de capacidade.

Uma fila crescendo indefinidamente indica problema.

---

# Escalabilidade

Consumers poderão ser escalados horizontalmente.

Condições:

- Idempotência.
    
- Concorrência segura.
    
- Ausência de estado local obrigatório.
    
- Controle de lease.
    
- Particionamento quando necessário.
    
- Limites de providers.
    

---

# Escalabilidade por Tipo de Trabalho

Workers poderão ser separados.

Exemplo:

```text
Pipeline Worker
AI Worker
Media Worker
Publication Worker
Analytics Worker
Webhook Worker
Outbox Worker
```

Benefícios:

- Escala independente.
    
- Isolamento de falhas.
    
- Limites diferentes.
    
- Deploy independente no futuro.
    
- Recursos diferentes.
    

---

# Prioridades

Mensagens poderão possuir prioridade.

Exemplos:

```text
Critical
High
Normal
Low
Background
```

Casos:

- Publicação agendada próxima.
    
- Webhook de conclusão.
    
- Execução manual.
    
- Analytics histórico.
    

Prioridade deverá ser usada com moderação para evitar starvation.

---

# Filas Separadas por Prioridade

Alternativa:

```text
pipeline.high
pipeline.normal
pipeline.low
```

Pode ser mais previsível que prioridades internas do broker.

---

# Fairness entre Organizações

Uma organização não deverá consumir toda a capacidade.

Estratégias:

- Limite por organização.
    
- Rate limit.
    
- Quotas.
    
- Particionamento.
    
- Scheduler justo.
    
- Concorrência máxima por tenant.
    

---

# Mensagens Administrativas

Operações administrativas deverão utilizar canais específicos quando necessário.

Exemplos:

- Reprocessar DLQ.
    
- Pausar pipeline.
    
- Suspender organização.
    
- Reconciliar publicação.
    
- Rotacionar conexão.
    

Essas operações deverão gerar auditoria.

---

# Reprocessamento Manual

Uma ferramenta administrativa poderá permitir:

- Inspecionar mensagem.
    
- Identificar erro.
    
- Corrigir configuração.
    
- Reprocessar.
    
- Mover para parking queue.
    
- Descartar com justificativa.
    

Toda ação deverá ser auditada.

---

# Descarte de Mensagens

Descartar uma mensagem crítica deverá ser excepcional.

Exigir:

- Permissão.
    
- Motivo.
    
- Identificação do operador.
    
- Registro de payload seguro.
    
- Avaliação do impacto.
    
- Auditoria.
    

---

# Regras Arquiteturais

- Comunicação síncrona deve ser utilizada apenas quando necessária.
    
- Processos longos devem ser assíncronos.
    
- Commands expressam intenção.
    
- Events representam fatos.
    
- Events devem utilizar nomes no passado.
    
- Contratos de mensagens ficam em Contracts.
    
- Regras de negócio não ficam nos consumers.
    
- Consumers devem ser finos.
    
- A Application não conhece o broker concreto.
    
- Implementações de mensageria ficam em Infrastructure.
    
- Mensagens devem possuir MessageId.
    
- Mensagens devem possuir versão.
    
- CorrelationId deve ser propagado.
    
- CausationId deve ser preservado quando aplicável.
    
- Mensagens devem ser imutáveis.
    
- Consumers devem tolerar campos desconhecidos.
    
- Payloads grandes devem utilizar Claim Check.
    
- Segredos não devem ser enviados em mensagens.
    
- Arquivos não devem trafegar diretamente pelo broker.
    
- Eventos críticos devem utilizar Outbox.
    
- Consumers críticos devem utilizar Inbox.
    
- Mensagens duplicadas devem ser esperadas.
    
- Exactly-once distribuído não será assumido.
    
- Ack deve ocorrer apenas após processamento seguro.
    
- Auto-ack não deve ser utilizado em operações críticas.
    
- Retry deve ser limitado e observável.
    
- Mensagens irrecuperáveis devem ir para DLQ.
    
- DLQ deve possuir processo operacional.
    
- Ordenação global não deve ser assumida.
    
- Agendamentos críticos devem ser persistidos.
    
- Workers não devem chamar outros Workers diretamente.
    
- O n8n não deve acessar o banco de produção.
    
- Webhooks devem ser assinados.
    
- Webhooks recebidos devem utilizar idempotência.
    
- Estado em tempo real não é fonte oficial de verdade.
    
- O cliente deve consultar o estado após reconexão.
    
- Broker e filas devem ser isolados por ambiente.
    
- Credenciais devem seguir menor privilégio.
    
- Toda comunicação deve ser observável.
    
- Contratos devem possuir testes de compatibilidade.
    

---

# Decisões Pendentes

As seguintes decisões deverão ser registradas em ADRs:

- Broker principal.
    
- Uso de RabbitMQ.
    
- Uso de MassTransit, Wolverine ou cliente oficial.
    
- Convenção de exchanges.
    
- Convenção de filas.
    
- Convenção de routing keys.
    
- Estratégia de versionamento de mensagens.
    
- Formato de serialização.
    
- Uso futuro de schema registry.
    
- Estratégia de Inbox.
    
- Estratégia de Outbox.
    
- Estratégia de retry.
    
- Estratégia de DLQ.
    
- Estratégia de parking queue.
    
- Estratégia de mensagens agendadas.
    
- Política de retenção da Outbox.
    
- Política de retenção da Inbox.
    
- Política de retenção da DLQ.
    
- Estratégia de prioridade.
    
- Estratégia de particionamento.
    
- Estratégia de comunicação com serviço Python.
    
- Uso de SignalR ou SSE.
    
- Estratégia de integração com n8n.
    
- Uso de AsyncAPI.
    
- Política de reprocessamento manual.
    

---

# Exemplo Completo: Início de Pipeline

```text
Usuário inicia pipeline
    ↓
API valida request
    ↓
Application cria PipelineExecution
    ↓
Domain gera PipelineExecutionCreated
    ↓
Application cria StartPipelineCommand
    ↓
Data salva execução e OutboxMessage
    ↓
Commit no PostgreSQL
    ↓
API retorna 202 Accepted
    ↓
Outbox Processor busca mensagem
    ↓
Publica em infinite-content.commands
    ↓
Routing key commands.pipelines.start.v1
    ↓
Pipeline Worker recebe
    ↓
Inbox verifica MessageId
    ↓
Worker executa Application
    ↓
Pipeline é iniciado
    ↓
Estado é persistido
    ↓
PipelineStartedEvent é salvo na Outbox
    ↓
Mensagem é confirmada
```

---

# Exemplo Completo: Etapa de Renderização

```text
Pipeline chega à etapa RenderVideo
    ↓
Pipeline Worker cria RenderVideoCommand
    ↓
Command é salvo na Outbox
    ↓
Media Worker recebe o command
    ↓
Inbox verifica duplicidade
    ↓
Job de renderização é criado
    ↓
JobId é persistido
    ↓
Mensagem é confirmada
    ↓
Serviço Python processa o vídeo
    ↓
VideoRenderedEvent é publicado
    ↓
Pipeline Worker consome o evento
    ↓
Valida versão e organização
    ↓
Atualiza checkpoint
    ↓
Pipeline continua
```

---

# Exemplo Completo: Falha e DLQ

```text
Publication Worker recebe PublishContentCommand
    ↓
Token OAuth está temporariamente indisponível
    ↓
Erro classificado como transitório
    ↓
Mensagem vai para retry de 1 minuto
    ↓
Nova tentativa falha
    ↓
Mensagem vai para retry de 5 minutos
    ↓
Limite máximo é atingido
    ↓
Mensagem é enviada para publication-worker.dlq
    ↓
Alerta é disparado
    ↓
Operador identifica token revogado
    ↓
Organização reconecta a plataforma
    ↓
Mensagem é reprocessada
    ↓
Inbox e idempotência evitam duplicidade
    ↓
Publicação é concluída
```

---

# Exemplo Completo: Webhook

```text
Plataforma envia webhook
    ↓
API valida assinatura
    ↓
Valida timestamp
    ↓
Valida MessageId
    ↓
Persiste WebhookDelivery e Inbox
    ↓
Publica ProcessPublicationWebhookCommand
    ↓
Responde HTTP 202
    ↓
Webhook Worker recebe
    ↓
Application carrega publicação
    ↓
Confirma OrganizationId
    ↓
Atualiza estado
    ↓
Salva PublicationCompletedEvent na Outbox
    ↓
Notificação em tempo real é enviada
```

---

# Objetivo Final

Criar uma arquitetura de comunicação previsível, desacoplada e preparada para processos longos e distribuídos.

O Infinite Content AI deverá trocar mensagens sem perder estado, suportar duplicidades, recuperar falhas e evoluir contratos sem quebrar consumidores.

Commands deverão expressar intenções.

Events deverão representar fatos.

Outbox deverá proteger a publicação.

Inbox deverá proteger o consumo.

DLQ deverá tornar falhas operáveis.

Webhooks deverão ser seguros.

O broker deverá desacoplar componentes sem se tornar a fonte oficial de verdade.