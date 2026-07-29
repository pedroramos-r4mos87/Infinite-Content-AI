# Contracts

## 1. Objetivo

O projeto `Contracts` será responsável pelos contratos compartilhados entre processos, hosts e componentes distribuídos do Infinite Content AI.

Seu principal uso será definir mensagens publicadas e consumidas por:

- API.
    
- Worker.
    
- RabbitMQ.
    
- Outbox.
    
- Inbox.
    
- Automações externas.
    
- Webhooks e callbacks futuros.
    

O projeto deverá conter somente contratos de comunicação.

Ele não deverá conter regras de negócio, persistência, SDKs, handlers ou implementações técnicas.

Fluxo principal:

```text
Application
    ↓ cria contrato
Outbox
    ↓
RabbitMQ
    ↓
Worker
    ↓ consome contrato
Application
```

---

# 2. Escopo do MVP

Para finalizar o MVP rapidamente, o projeto Contracts deverá implementar somente:

```text
MessageEnvelope
MessageMetadata
PipelineExecutionRequestedV1
PipelineStepExecutionRequestedV1
PipelineExecutionCompletedV1
PipelineExecutionFailedV1
ArtifactGeneratedV1
```

Não será necessário implementar inicialmente:

- Contratos de publicação em redes sociais.
    
- Contratos de approvals.
    
- Contratos de billing.
    
- Contratos de n8n.
    
- Contratos de serviço Python.
    
- Contratos de mídia.
    
- Contratos administrativos.
    
- Múltiplas versões da mesma mensagem.
    
- Registro dinâmico de schemas.
    
- Avro ou Protobuf.
    
- Schema Registry.
    

JSON será suficiente para o MVP.

---

# 3. Responsabilidades

O projeto Contracts será responsável por:

- Envelopes de mensagens.
    
- Commands distribuídos.
    
- Integration Events.
    
- Contratos de callbacks.
    
- Contratos de webhooks.
    
- Versionamento de mensagens.
    
- Metadados compartilhados.
    
- Identificação dos tipos de mensagem.
    
- Estruturas de compatibilidade entre processos.
    

Não será responsável por:

- Domain Events.
    
- Entidades.
    
- Commands internos da Application.
    
- Queries.
    
- Handlers.
    
- Consumers.
    
- Serialização concreta do RabbitMQ.
    
- Persistência da Outbox.
    
- Persistência da Inbox.
    
- Validação de negócio.
    
- Retry.
    
- Dead Letter.
    
- Publicação física.
    
- Autenticação.
    

---

# 4. Dependências

O projeto Contracts deverá possuir o menor número possível de dependências.

Preferencialmente:

```text
Contracts
    ↓
nenhum outro projeto da solution
```

Ele não poderá depender de:

```text
Domain
Application
Api
Data
Infrastructure
Worker
SharedKernel
```

Mesmo tipos simples como `ProjectId` ou `OrganizationId` não deverão ser utilizados diretamente nos contratos distribuídos.

Utilizar tipos primitivos interoperáveis:

```text
Guid
string
int
long
decimal
bool
DateTimeOffset
arrays
records simples
```

Isso evita acoplamento entre o modelo interno e os contratos externos.

---

# 5. Estrutura do Projeto

```text
Contracts
│
├── Messaging
│   ├── Envelopes
│   │   ├── MessageEnvelope.cs
│   │   └── MessageMetadata.cs
│   │
│   ├── MessageTypes.cs
│   │
│   ├── Pipelines
│   │   ├── Commands
│   │   │   ├── PipelineExecutionRequestedV1.cs
│   │   │   └── PipelineStepExecutionRequestedV1.cs
│   │   │
│   │   └── Events
│   │       ├── PipelineExecutionCompletedV1.cs
│   │       └── PipelineExecutionFailedV1.cs
│   │
│   └── Artifacts
│       └── Events
│           └── ArtifactGeneratedV1.cs
│
├── Webhooks
├── Callbacks
└── Common
```

As pastas `Webhooks` e `Callbacks` poderão permanecer vazias até existir necessidade concreta.

---

# 6. Tipos de Contrato

Existirão dois tipos principais de mensagens.

## Command distribuído

Representa uma solicitação para que algum processo execute uma ação.

Exemplos:

```text
PipelineExecutionRequestedV1
PipelineStepExecutionRequestedV1
PublicationRequestedV1
```

Command utiliza nome de intenção ou solicitação.

Ele não garante que a operação foi concluída.

---

## Integration Event

Representa um fato que já aconteceu e foi persistido.

Exemplos:

```text
PipelineExecutionCompletedV1
PipelineExecutionFailedV1
ArtifactGeneratedV1
```

Integration Event deverá utilizar nome no passado.

---

# 7. Command Interno x Command Distribuído

Command interno da Application:

```csharp
public sealed record StartExecutionCommand(
    OrganizationId OrganizationId,
    PipelineId PipelineId);
```

Command distribuído:

```csharp
public sealed record PipelineExecutionRequestedV1(
    Guid ExecutionId,
    Guid PipelineId,
    Guid OrganizationId,
    DateTimeOffset RequestedAt);
```

Eles não são o mesmo objeto.

O Command interno representa um caso de uso.

O contrato distribuído representa uma mensagem entre processos.

---

# 8. Domain Event x Integration Event

Domain Event:

```text
PipelineExecutionCompletedDomainEvent
```

Integration Event:

```text
PipelineExecutionCompletedV1
```

O Domain Event:

- Existe dentro do Domain.
    
- Pode conter tipos internos.
    
- Não é publicado diretamente.
    
- Não possui obrigação de compatibilidade externa.
    

O Integration Event:

- Existe em Contracts.
    
- Utiliza tipos interoperáveis.
    
- É versionado.
    
- É publicado pelo sistema.
    
- Deve manter compatibilidade.
    

Fluxo:

```text
Domain Event
    ↓
Application
    ↓
Integration Event
    ↓
Outbox
    ↓
RabbitMQ
```

---

# 9. MessageEnvelope

Todas as mensagens deverão utilizar um envelope padronizado.

```csharp
public sealed record MessageEnvelope<TPayload>(
    Guid MessageId,
    string MessageType,
    int MessageVersion,
    DateTimeOffset OccurredAt,
    Guid OrganizationId,
    string? CorrelationId,
    string? CausationId,
    string? IdempotencyKey,
    TPayload Payload,
    MessageMetadata Metadata);
```

O envelope fornece informações técnicas e de rastreamento.

O `Payload` contém os dados específicos da mensagem.

---

# 10. Campos do Envelope

## MessageId

Identificador único da mensagem.

Deverá permanecer o mesmo durante redelivery.

```text
MessageId = identidade lógica da mensagem
```

Não gerar um novo MessageId a cada tentativa de publicação da mesma Outbox.

---

## MessageType

Nome lógico e estável da mensagem.

Exemplo:

```text
pipeline.execution.requested
```

Não utilizar nome completo de classe CLR.

Evitar:

```text
InfiniteContentAI.Contracts.Messaging.Pipelines.Commands.PipelineExecutionRequestedV1
```

---

## MessageVersion

Versão numérica do contrato.

Exemplo:

```text
1
```

O sufixo `V1` da classe e o campo `MessageVersion` deverão permanecer consistentes.

---

## OccurredAt

Momento em que o fato ocorreu ou a solicitação foi criada.

Deverá utilizar UTC.

---

## OrganizationId

Identifica o tenant proprietário da operação.

Toda mensagem tenant-scoped deverá possuir esse campo.

---

## CorrelationId

Identifica o fluxo lógico completo.

Exemplo:

```text
Request HTTP
    ↓
StartExecution
    ↓
Research
    ↓
Script
```

Todas essas operações poderão compartilhar o mesmo CorrelationId.

---

## CausationId

Identifica o evento ou mensagem que causou a mensagem atual.

Exemplo:

```text
PipelineExecutionRequested
    ↓ causa
PipelineStepExecutionRequested
```

---

## IdempotencyKey

Utilizada quando a operação precisa de deduplicação por uma chave de negócio ou de request.

Não substitui `MessageId`.

---

# 11. MessageMetadata

```csharp
public sealed record MessageMetadata(
    string Producer,
    string Environment,
    string? TraceParent,
    string? TraceState,
    IReadOnlyDictionary<string, string>? Additional);
```

Campos:

- `Producer`: aplicação que criou a mensagem.
    
- `Environment`: ambiente de origem.
    
- `TraceParent`: contexto de trace.
    
- `TraceState`: contexto adicional de trace.
    
- `Additional`: metadados não críticos.
    

Não incluir em metadados:

- Secrets.
    
- Tokens.
    
- Prompts completos.
    
- Conteúdo de artefatos.
    
- Dados pessoais desnecessários.
    
- Credenciais.
    
- Connection strings.
    

---

# 12. MessageTypes

Os nomes lógicos deverão ser centralizados.

```csharp
public static class MessageTypes
{
    public const string PipelineExecutionRequested =
        "pipeline.execution.requested";

    public const string PipelineStepExecutionRequested =
        "pipeline.step.execution.requested";

    public const string PipelineExecutionCompleted =
        "pipeline.execution.completed";

    public const string PipelineExecutionFailed =
        "pipeline.execution.failed";

    public const string ArtifactGenerated =
        "artifact.generated";
}
```

Isso evita strings espalhadas pelo sistema.

---

# 13. PipelineExecutionRequestedV1

Essa mensagem solicita o início do processamento de uma Pipeline Execution já criada.

```csharp
public sealed record PipelineExecutionRequestedV1(
    Guid ExecutionId,
    Guid PipelineId,
    int PipelineVersion,
    Guid ProjectId,
    Guid OrganizationId,
    Guid RequestedBy,
    DateTimeOffset RequestedAt);
```

Ela deverá ser criada somente depois que a execução estiver persistida.

Fluxo:

```text
StartExecutionCommand
    ↓
PipelineExecution = Queued
    ↓
PipelineExecutionRequestedV1
    ↓
Outbox
```

---

# 14. Uso de PipelineExecutionRequestedV1

Consumer:

```text
PipelineExecutionRequestedConsumer
```

Responsabilidade:

```text
Receber mensagem
    ↓
Executar ExecutePipelineCommand
    ↓
Alterar execução para Running
    ↓
Solicitar primeira etapa
```

A mensagem não deverá carregar:

- Pipeline completo.
    
- Configuração completa.
    
- Prompts.
    
- Step definitions completas.
    
- Artifacts.
    

O Worker deverá carregar os dados necessários por identificador.

---

# 15. PipelineStepExecutionRequestedV1

Essa mensagem solicita a execução de uma etapa específica.

```csharp
public sealed record PipelineStepExecutionRequestedV1(
    Guid ExecutionId,
    Guid StepExecutionId,
    string StepType,
    int AttemptNumber,
    Guid AttemptId,
    Guid OrganizationId,
    DateTimeOffset RequestedAt);
```

Campos importantes:

- `ExecutionId`: execução principal.
    
- `StepExecutionId`: etapa concreta.
    
- `StepType`: tipo do handler.
    
- `AttemptNumber`: número da tentativa.
    
- `AttemptId`: identifica a tentativa específica.
    
- `OrganizationId`: tenant.
    
- `RequestedAt`: momento do agendamento.
    

---

# 16. AttemptId

O `AttemptId` evita que resultados antigos sobrescrevam tentativas novas.

Cenário:

```text
Tentativa 1 inicia
    ↓
Lease expira
    ↓
Tentativa 2 inicia
    ↓
Tentativa 1 termina atrasada
```

Ao concluir, a Application deverá validar:

```text
AttemptId recebido
==
AttemptId atual da etapa
```

Caso contrário, o resultado deverá ser ignorado ou tratado como conflito idempotente.

---

# 17. PipelineExecutionCompletedV1

Representa a conclusão bem-sucedida de uma execução.

```csharp
public sealed record PipelineExecutionCompletedV1(
    Guid ExecutionId,
    Guid PipelineId,
    int PipelineVersion,
    Guid ProjectId,
    Guid OrganizationId,
    DateTimeOffset CompletedAt);
```

Esse evento poderá futuramente ser consumido por:

- Notificações.
    
- Analytics.
    
- Billing.
    
- Webhooks.
    
- Automação.
    
- Publicação.
    

No MVP, ele poderá ser persistido e publicado mesmo sem possuir consumidor adicional.

---

# 18. PipelineExecutionFailedV1

Representa falha terminal da execução.

```csharp
public sealed record PipelineExecutionFailedV1(
    Guid ExecutionId,
    Guid PipelineId,
    Guid ProjectId,
    Guid OrganizationId,
    string ErrorCode,
    string ErrorCategory,
    bool IsRetryable,
    DateTimeOffset FailedAt);
```

Não incluir:

- Stack trace.
    
- Exception completa.
    
- Prompt.
    
- Resposta integral do provider.
    
- Secret.
    
- Mensagem técnica sem controle.
    

`ErrorCode` deverá ser estável.

Exemplos:

```text
AI.ProviderTimeout
AI.StructuredOutputInvalid
Execution.MaximumAttemptsReached
Pipeline.InvalidDefinition
```

---

# 19. ArtifactGeneratedV1

Representa a criação persistida de um Artifact.

```csharp
public sealed record ArtifactGeneratedV1(
    Guid ArtifactId,
    Guid ExecutionId,
    Guid StepExecutionId,
    Guid ProjectId,
    Guid OrganizationId,
    string ArtifactType,
    int ArtifactVersion,
    string? Provider,
    string? Model,
    DateTimeOffset GeneratedAt);
```

A mensagem não deverá carregar o conteúdo completo do Artifact.

O consumidor deverá consultar o Artifact por ID quando necessário.

---

# 20. Mensagens Leves

Contratos distribuídos deverão transportar somente:

- Identificadores.
    
- Versões.
    
- Estados.
    
- Tipos.
    
- Datas.
    
- Metadados mínimos.
    
- Referências.
    

Evitar:

```text
Conteúdo completo de pesquisa
Roteiro completo
JSON muito grande
Imagem em Base64
Áudio
Vídeo
Prompts completos
```

Dados grandes deverão permanecer em:

- PostgreSQL.
    
- Azure Blob Storage.
    
- Outro storage apropriado.
    

---

# 21. Versionamento

Toda mensagem pública deverá possuir versão.

Convenção:

```text
NomeDaMensagemV1
NomeDaMensagemV2
```

Exemplo:

```csharp
PipelineExecutionRequestedV1
```

Versão lógica:

```text
MessageType: pipeline.execution.requested
MessageVersion: 1
```

---

# 22. Quando Criar uma Nova Versão

Criar nova versão quando houver mudança incompatível.

Exemplos:

- Remover campo.
    
- Renomear campo.
    
- Alterar tipo.
    
- Alterar significado.
    
- Tornar campo opcional em obrigatório.
    
- Alterar estrutura interna.
    
- Mudar unidade.
    
- Mudar formato.
    

Exemplo:

```text
PipelineExecutionRequestedV1
    ↓ mudança incompatível
PipelineExecutionRequestedV2
```

---

# 23. Mudanças Compatíveis

Normalmente são compatíveis:

- Adicionar campo opcional.
    
- Adicionar metadado opcional.
    
- Adicionar novo valor aceito quando consumers toleram desconhecidos.
    
- Melhorar documentação.
    
- Corrigir mensagem textual.
    

Mesmo mudanças aparentemente compatíveis deverão ser testadas.

---

# 24. Compatibilidade de Consumers

Consumers deverão:

- Ignorar campos desconhecidos.
    
- Validar campos obrigatórios.
    
- Rejeitar versões desconhecidas.
    
- Não depender da ordem dos campos JSON.
    
- Não depender de nomes CLR.
    
- Possuir testes com payload real.
    
- Tratar valores desconhecidos com segurança.
    

---

# 25. Evolução de V1 para V2

Durante uma migração:

```text
Producer publica V1
Consumer entende V1
```

Depois:

```text
Consumer passa a entender V1 e V2
```

Em seguida:

```text
Producer começa a publicar V2
```

Por último:

```text
V1 é descontinuada
```

O Consumer deverá ser atualizado antes do Producer quando a nova versão não for compatível.

---

# 26. Serialização

O formato inicial será JSON.

Convenções:

```text
UTF-8
camelCase
ISO 8601
Enums como strings
Campos opcionais permitidos
Content-Type application/json
```

Exemplo:

```json
{
  "messageId": "019c1234-5678-7abc-9123-456789abcdef",
  "messageType": "pipeline.execution.requested",
  "messageVersion": 1,
  "occurredAt": "2026-07-28T15:00:00Z",
  "organizationId": "019c1234-5678-7abc-9123-456789abcdef",
  "correlationId": "019c...",
  "causationId": null,
  "idempotencyKey": "start-execution-123",
  "payload": {
    "executionId": "019c...",
    "pipelineId": "019c...",
    "pipelineVersion": 1,
    "projectId": "019c...",
    "organizationId": "019c...",
    "requestedBy": "019c...",
    "requestedAt": "2026-07-28T15:00:00Z"
  }
}
```

---

# 27. OrganizationId Duplicado

O `OrganizationId` poderá existir:

- No envelope.
    
- No payload.
    

Para o MVP, essa duplicação será permitida.

Motivos:

- Validação.
    
- Segurança.
    
- Contratos autossuficientes.
    
- Facilidade de roteamento.
    
- Facilidade de diagnóstico.
    

O Consumer deverá validar:

```text
Envelope.OrganizationId
==
Payload.OrganizationId
```

Caso contrário, a mensagem deverá ser rejeitada.

---

# 28. Identidade da Mensagem

Uma nova mensagem deverá possuir novo `MessageId`.

Redelivery da mesma mensagem deverá preservar o `MessageId`.

Retry de negócio poderá seguir duas estratégias:

## Mesma mensagem

Preserva MessageId quando representa redelivery técnico.

## Nova mensagem

Gera novo MessageId quando representa uma nova tentativa de negócio.

Nesse caso:

```text
CausationId = MessageId anterior
```

A escolha deverá ser consistente.

Para o MVP:

- Redelivery do RabbitMQ preserva MessageId.
    
- Novo agendamento de Step gera novo MessageId.
    
- AttemptId identifica a tentativa.
    

---

# 29. Idempotência

O Consumer deverá utilizar:

```text
ConsumerName
+
MessageId
```

como chave de Inbox.

Exemplo:

```text
pipeline-step-consumer
+
019c1234...
```

Isso permite que consumidores diferentes processem o mesmo Integration Event independentemente.

---

# 30. CorrelationId e CausationId

Exemplo completo:

```text
HTTP StartExecution
CorrelationId = C1
```

```text
PipelineExecutionRequested
MessageId = M1
CorrelationId = C1
CausationId = HTTP request ou null
```

```text
PipelineStepExecutionRequested
MessageId = M2
CorrelationId = C1
CausationId = M1
```

```text
ArtifactGenerated
MessageId = M3
CorrelationId = C1
CausationId = M2
```

Isso permite reconstruir a cadeia da operação.

---

# 31. Dados Sensíveis

Contratos não deverão transportar:

- API keys.
    
- Access tokens.
    
- Refresh tokens.
    
- Connection strings.
    
- SAS tokens.
    
- Credenciais.
    
- Senhas.
    
- Cookies.
    
- Prompts privados completos.
    
- Conteúdo sensível sem necessidade.
    
- Informações pessoais excessivas.
    

Quando uma credencial for necessária, utilizar:

```text
CredentialReferenceId
```

---

# 32. Erros

Mensagens de falha deverão utilizar:

```text
ErrorCode
ErrorCategory
IsRetryable
```

Evitar depender apenas de:

```text
ErrorMessage
```

Mensagens textuais podem mudar.

Códigos devem permanecer estáveis.

Categorias iniciais:

```text
validation
notFound
conflict
timeout
unavailable
rateLimit
authentication
forbidden
unexpected
```

---

# 33. Contratos de Webhook

Contratos de webhook serão adicionados somente quando o MVP precisar integrar sistemas externos.

Estrutura futura:

```text
Contracts
└── Webhooks
    ├── N8n
    ├── YouTube
    └── Generic
```

Um contrato de webhook deverá possuir:

- MessageId.
    
- Timestamp.
    
- Version.
    
- CorrelationId.
    
- OrganizationId.
    
- Payload.
    
- Signature fora do body.
    

Não implementar agora sem consumidor real.

---

# 34. Contratos de Callback

Callbacks poderão ser usados para operações externas assíncronas.

Exemplo futuro:

```csharp
public sealed record ExternalOperationCompletedV1(
    Guid OperationId,
    Guid OrganizationId,
    string Status,
    string? ExternalReference,
    DateTimeOffset CompletedAt);
```

Para o MVP de Research e Script, callbacks não são necessários.

---

# 35. Contratos HTTP não Pertencem ao Contracts

Requests e Responses da API deverão permanecer no projeto `Api`.

Exemplo:

```text
CreateProjectRequest
CreateProjectResponse
GetExecutionResponse
```

O projeto Contracts não deverá virar uma coleção de todos os DTOs da solution.

Contracts representa comunicação compartilhada entre processos ou sistemas.

---

# 36. Commands da Application não Pertencem ao Contracts

Exemplos que permanecem na Application:

```text
CreateProjectCommand
StartExecutionCommand
ExecutePipelineStepCommand
GetExecutionQuery
```

Eles não são contratos distribuídos.

Mesmo que um Consumer transforme uma mensagem em Command, os objetos deverão permanecer separados.

---

# 37. Entidades não Pertencem ao Contracts

Não adicionar:

```text
Project
Pipeline
PipelineExecution
Artifact
```

ao projeto Contracts.

Também não expor entidades como payload:

```csharp
public sealed record PipelineExecutionRequestedV1(
    PipelineExecution Execution);
```

Isso criaria acoplamento com o Domain.

---

# 38. Factories de Envelope

O projeto Contracts poderá fornecer uma factory simples e sem dependências externas.

```csharp
public static class MessageEnvelopeFactory
{
    public static MessageEnvelope<TPayload> Create<TPayload>(
        Guid messageId,
        string messageType,
        int messageVersion,
        Guid organizationId,
        DateTimeOffset occurredAt,
        TPayload payload,
        string? correlationId = null,
        string? causationId = null,
        string? idempotencyKey = null,
        MessageMetadata? metadata = null)
    {
        return new MessageEnvelope<TPayload>(
            messageId,
            messageType,
            messageVersion,
            occurredAt,
            organizationId,
            correlationId,
            causationId,
            idempotencyKey,
            payload,
            metadata ?? MessageMetadata.Empty);
    }
}
```

Não adicionar relógio, DI ou serialização à factory.

---

# 39. MessageMetadata.Empty

```csharp
public sealed record MessageMetadata(
    string Producer,
    string Environment,
    string? TraceParent,
    string? TraceState,
    IReadOnlyDictionary<string, string>? Additional)
{
    public static readonly MessageMetadata Empty =
        new(
            Producer: "unknown",
            Environment: "unknown",
            TraceParent: null,
            TraceState: null,
            Additional: null);
}
```

Na prática, Producer e Environment deverão ser preenchidos pela Infrastructure.

---

# 40. Contratos do MVP

## PipelineExecutionRequestedV1

```csharp
namespace InfiniteContentAI.Contracts.Messaging.Pipelines.Commands;

public sealed record PipelineExecutionRequestedV1(
    Guid ExecutionId,
    Guid PipelineId,
    int PipelineVersion,
    Guid ProjectId,
    Guid OrganizationId,
    Guid RequestedBy,
    DateTimeOffset RequestedAt);
```

## PipelineStepExecutionRequestedV1

```csharp
namespace InfiniteContentAI.Contracts.Messaging.Pipelines.Commands;

public sealed record PipelineStepExecutionRequestedV1(
    Guid ExecutionId,
    Guid StepExecutionId,
    string StepType,
    int AttemptNumber,
    Guid AttemptId,
    Guid OrganizationId,
    DateTimeOffset RequestedAt);
```

## PipelineExecutionCompletedV1

```csharp
namespace InfiniteContentAI.Contracts.Messaging.Pipelines.Events;

public sealed record PipelineExecutionCompletedV1(
    Guid ExecutionId,
    Guid PipelineId,
    int PipelineVersion,
    Guid ProjectId,
    Guid OrganizationId,
    DateTimeOffset CompletedAt);
```

## PipelineExecutionFailedV1

```csharp
namespace InfiniteContentAI.Contracts.Messaging.Pipelines.Events;

public sealed record PipelineExecutionFailedV1(
    Guid ExecutionId,
    Guid PipelineId,
    Guid ProjectId,
    Guid OrganizationId,
    string ErrorCode,
    string ErrorCategory,
    bool IsRetryable,
    DateTimeOffset FailedAt);
```

## ArtifactGeneratedV1

```csharp
namespace InfiniteContentAI.Contracts.Messaging.Artifacts.Events;

public sealed record ArtifactGeneratedV1(
    Guid ArtifactId,
    Guid ExecutionId,
    Guid StepExecutionId,
    Guid ProjectId,
    Guid OrganizationId,
    string ArtifactType,
    int ArtifactVersion,
    string? Provider,
    string? Model,
    DateTimeOffset GeneratedAt);
```

---

# 41. Routing Keys do MVP

```text
pipeline.execution.requested.v1
pipeline.step.execution.requested.v1
pipeline.execution.completed.v1
pipeline.execution.failed.v1
artifact.generated.v1
```

Exchanges sugeridas:

```text
infinite-content.commands
infinite-content.events
```

Commands:

```text
infinite-content.commands
```

Events:

```text
infinite-content.events
```

---

# 42. Filas do MVP

```text
infinite-content.pipeline.execution
infinite-content.pipeline.steps
```

Bindings:

```text
infinite-content.pipeline.execution
    ← pipeline.execution.requested.v1
```

```text
infinite-content.pipeline.steps
    ← pipeline.step.execution.requested.v1
```

Os eventos de conclusão poderão inicialmente não possuir fila se nenhum consumidor existir.

Eles ainda poderão ser publicados para futura expansão.

---

# 43. Validação Técnica

O Worker deverá validar antes de processar:

- MessageId diferente de vazio.
    
- MessageType conhecido.
    
- MessageVersion suportada.
    
- OrganizationId presente.
    
- Payload não nulo.
    
- IDs obrigatórios presentes.
    
- AttemptNumber maior que zero.
    
- StepType não vazio.
    
- Envelope e payload com mesma Organization.
    
- Tamanho dentro do limite.
    

Validação de negócio continuará na Application e no Domain.

---

# 44. Testes Unitários

O projeto Contracts deverá possuir poucos testes.

Testar:

- Serialização.
    
- Desserialização.
    
- Nomes de propriedades.
    
- Campos obrigatórios.
    
- MessageTypes.
    
- Compatibilidade de payload.
    
- Valores de versão.
    

Exemplo:

```csharp
[Fact]
public void PipelineExecutionRequestedV1_ShouldRoundTripJson()
{
    var message = new PipelineExecutionRequestedV1(
        ExecutionId: Guid.CreateVersion7(),
        PipelineId: Guid.CreateVersion7(),
        PipelineVersion: 1,
        ProjectId: Guid.CreateVersion7(),
        OrganizationId: Guid.CreateVersion7(),
        RequestedBy: Guid.CreateVersion7(),
        RequestedAt: DateTimeOffset.UtcNow);

    var json = JsonSerializer.Serialize(message);

    var deserialized =
        JsonSerializer.Deserialize<
            PipelineExecutionRequestedV1>(json);

    deserialized.Should().BeEquivalentTo(message);
}
```

---

# 45. Testes de Compatibilidade

Ao alterar um contrato existente:

1. Manter um JSON de exemplo da versão anterior.
    
2. Desserializar com o código atual.
    
3. Confirmar resultado esperado.
    
4. Verificar campos opcionais.
    
5. Verificar valores desconhecidos.
    
6. Confirmar que a versão continua suportada.
    

Exemplo de arquivo:

```text
Contracts.Tests
└── Samples
    └── pipeline-execution-requested-v1.json
```

---

# 46. Testes de Snapshot

Snapshots de JSON poderão ser utilizados para detectar mudanças acidentais.

Cuidados:

- Revisar mudanças.
    
- Não aceitar snapshot automaticamente.
    
- Não armazenar dados sensíveis.
    
- Manter exemplos pequenos.
    
- Não depender da ordem quando a ordem não for contratual.
    

---

# 47. Antipadrões

## Contracts dependendo do Domain

Cria acoplamento interno.

## Entity como payload

Vaza comportamento e estrutura interna.

## Um DTO para todas as camadas

Mistura limites.

## Nome de classe CLR como MessageType

Dificulta evolução e interoperabilidade.

## Mensagem sem versão

Impede mudança segura.

## Conteúdo grande na mensagem

Sobrecarrega broker.

## Secret na mensagem

Cria risco grave de segurança.

## Campo genérico `object Data`

Remove contrato explícito.

## Dicionário como payload principal

Dificulta validação e compatibilidade.

## Alterar V1 diretamente

Mudanças incompatíveis exigem V2.

## MessageId novo em redelivery

Quebra deduplicação.

## Exceção completa no evento de falha

Pode vazar dados e acoplar tecnologias.

---

# 48. Regras Arquiteturais

1. Contracts não depende de outros projetos.
    
2. Contratos utilizam tipos interoperáveis.
    
3. Entidades não são contratos.
    
4. Domain Events não são Integration Events.
    
5. Commands internos não são Commands distribuídos.
    
6. Contratos HTTP permanecem na API.
    
7. Toda mensagem possui MessageId.
    
8. Toda mensagem possui MessageType.
    
9. Toda mensagem possui versão.
    
10. Toda mensagem tenant-scoped possui OrganizationId.
    
11. MessageId permanece no redelivery.
    
12. Mensagens grandes utilizam referências.
    
13. Secrets não são transportados.
    
14. Contratos incompatíveis criam nova versão.
    
15. Campos desconhecidos devem ser tolerados.
    
16. Versões desconhecidas devem ser rejeitadas.
    
17. Erros utilizam códigos estáveis.
    
18. Stack traces não são publicados.
    
19. CorrelationId e CausationId são propagados.
    
20. Contratos possuem testes de serialização.
    
21. Contratos críticos possuem exemplos JSON.
    
22. O MVP utiliza JSON.
    
23. Schema Registry não é necessário inicialmente.
    
24. Contratos são imutáveis.
    
25. Records são preferidos.
    

---

# 49. Ordem de Implementação

Para acelerar o MVP:

## Etapa 1

Criar:

```text
MessageEnvelope
MessageMetadata
MessageTypes
```

## Etapa 2

Criar:

```text
PipelineExecutionRequestedV1
PipelineStepExecutionRequestedV1
```

## Etapa 3

Criar:

```text
PipelineExecutionCompletedV1
PipelineExecutionFailedV1
ArtifactGeneratedV1
```

## Etapa 4

Adicionar:

```text
Testes de serialização
Exemplos JSON
Routing keys
```

Depois disso, parar.

Não criar novos contratos até uma feature real precisar deles.

---

# 50. Checklist para Nova Mensagem

- É Command ou Event?
    
- O nome representa intenção ou fato?
    
- Possui versão?
    
- Possui MessageType estável?
    
- Possui MessageId?
    
- Possui OrganizationId?
    
- Precisa de CorrelationId?
    
- Precisa de CausationId?
    
- O payload está pequeno?
    
- Existe dado sensível?
    
- Os campos são interoperáveis?
    
- A mensagem poderia transportar apenas IDs?
    
- A mudança é compatível?
    
- Existe Consumer?
    
- Existe routing key?
    
- Existe teste de serialização?
    
- Existe exemplo JSON?
    
- Existe política de idempotência?
    
- Existe comportamento para versão desconhecida?
    

---

# 51. Critérios de Qualidade

O projeto Contracts será considerado saudável quando:

- Os contratos forem pequenos.
    
- Os contratos forem explícitos.
    
- O Domain não for exposto.
    
- API e Worker puderem evoluir independentemente.
    
- Mensagens antigas continuarem legíveis.
    
- Mudanças incompatíveis criarem nova versão.
    
- Consumers puderem validar contratos.
    
- Mensagens duplicadas forem identificáveis.
    
- O trace puder ser propagado.
    
- Nenhum secret aparecer em mensagens.
    
- O RabbitMQ transportar apenas informações necessárias.
    
- Novos contratos forem criados somente por necessidade real.
    

---

# 52. Fluxo Contratual do MVP

```text
StartExecutionCommand
    ↓
PipelineExecutionRequestedV1
    ↓
PipelineExecutionRequestedConsumer
    ↓
ExecutePipelineCommand
    ↓
PipelineStepExecutionRequestedV1
    ↓
PipelineStepExecutionRequestedConsumer
    ↓
ResearchStepHandler
    ↓
ArtifactGeneratedV1
    ↓
PipelineStepExecutionRequestedV1
    ↓
ScriptStepHandler
    ↓
ArtifactGeneratedV1
    ↓
PipelineExecutionCompletedV1
```

Em caso de falha terminal:

```text
PipelineStepExecutionRequestedV1
    ↓
Falha permanente
    ↓
PipelineExecutionFailedV1
```

---

# 53. Definition of Done do Contracts para o MVP

O projeto Contracts estará pronto para o MVP quando:

- Compilar sem depender de outros projetos.
    
- Possuir MessageEnvelope.
    
- Possuir MessageMetadata.
    
- Possuir MessageTypes.
    
- Possuir os cinco contratos do MVP.
    
- Utilizar records imutáveis.
    
- Utilizar tipos primitivos.
    
- Possuir versões explícitas.
    
- Possuir testes de round-trip JSON.
    
- Possuir exemplos JSON.
    
- Não conter regras de negócio.
    
- Não conter SDKs.
    
- Não conter Entity.
    
- Não conter Commands da Application.
    
- Não conter contratos sem uso real.
    

---

# 54. Filosofia Final

O projeto Contracts deverá ser pequeno, estável e previsível.

Ele deverá expressar mensagens como:

```text
Uma execução foi solicitada
Uma etapa deve ser executada
Um artefato foi gerado
Uma execução foi concluída
Uma execução falhou
```

Ele não deverá expressar:

```text
Como a execução será processada
Como o Artifact será persistido
Como o RabbitMQ será configurado
Como um provider será chamado
Como um erro será apresentado por HTTP
```

A regra principal será:

> Contracts define o que atravessa os limites do sistema, não como o sistema executa o trabalho.

Para o MVP, manter poucos contratos é melhor do que criar uma arquitetura de eventos completa antes de existir fluxo funcional.