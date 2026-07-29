# Worker

## 1. Objetivo

O projeto `Worker` será o host responsável pelo processamento assíncrono do Infinite Content AI.

Ele executará trabalhos que:

- Não devem bloquear requisições HTTP.
    
- Possuem longa duração.
    
- Dependem de mensagens.
    
- Exigem retentativas.
    
- Precisam sobreviver à reinicialização do processo.
    
- Podem ser distribuídos entre múltiplas instâncias.
    
- Necessitam de reconciliação.
    
- Devem continuar mesmo sem um cliente conectado.
    

Exemplos:

- Publicação de mensagens da Outbox.
    
- Consumo de mensagens do RabbitMQ.
    
- Execução de pipelines.
    
- Execução de Pipeline Steps.
    
- Geração de conteúdo com IA.
    
- Processamento de arquivos.
    
- Publicação em plataformas externas.
    
- Recuperação de execuções travadas.
    
- Limpeza de registros técnicos.
    
- Jobs recorrentes.
    
- Reconciliação de estados externos.
    

Fluxo conceitual:

```text
RabbitMQ
    ↓
Worker
    ↓
Application
    ├── Domain
    ├── Data
    └── Infrastructure
```

O Worker deverá funcionar como host e adapter de processamento.

Ele não deverá concentrar regras de negócio.

---

# 2. Responsabilidades

O projeto `Worker` será responsável por:

- Hospedar Consumers.
    
- Receber mensagens do RabbitMQ.
    
- Desserializar envelopes.
    
- Validar contratos técnicos.
    
- Propagar contexto.
    
- Registrar mensagens na Inbox.
    
- Despachar Commands para a Application.
    
- Confirmar ou rejeitar mensagens.
    
- Aplicar retry de processamento.
    
- Encaminhar mensagens para Dead Letter.
    
- Executar o processamento da Outbox.
    
- Executar Pipeline Steps.
    
- Coordenar Background Services.
    
- Executar jobs recorrentes.
    
- Recuperar execuções interrompidas.
    
- Detectar execuções travadas.
    
- Aplicar shutdown gracioso.
    
- Controlar concorrência.
    
- Aplicar backpressure.
    
- Expor health checks.
    
- Produzir logs, métricas e traces.
    
- Inicializar topology de mensageria.
    
- Reconciliar resultados externos.
    
- Executar limpezas técnicas.
    

O projeto não será responsável por:

- Regras centrais de negócio.
    
- DbContext direto em Consumers.
    
- Queries SQL diretas em Consumers.
    
- Configurações do EF Core.
    
- Implementação de providers externos.
    
- Implementação do RabbitMQ client.
    
- Implementação do Redis client.
    
- Implementação do Azure Storage.
    
- Endpoints HTTP de negócio.
    
- Entidades do Domain.
    
- Contratos públicos HTTP.
    

---

# 3. Dependências Permitidas

O Worker poderá depender de:

```text
Application
Contracts
SharedKernel
Data
Infrastructure
```

As dependências de Data e Infrastructure serão utilizadas para:

- Composição.
    
- Registro de serviços.
    
- Execução de operações técnicas.
    
- Inicialização do host.
    
- Health checks.
    

Consumers e Background Services deverão depender prioritariamente de abstrações da Application.

Fluxo:

```text
Worker
    ├── Application
    ├── Contracts
    ├── Data
    └── Infrastructure
```

---

# 4. Worker como Host

O Worker é um host semelhante à API.

A diferença está no protocolo de entrada.

## API

```text
HTTP
    ↓
Endpoint
    ↓
Application
```

## Worker

```text
RabbitMQ, Scheduler ou BackgroundService
    ↓
Consumer ou Job
    ↓
Application
```

A mesma Application deverá poder ser reutilizada nos dois hosts.

---

# 5. Estrutura do Projeto

```text
Worker
│
├── Common
│   ├── Consumers
│   ├── Context
│   ├── Execution
│   ├── HealthChecks
│   ├── Middleware
│   ├── Resilience
│   ├── Scheduling
│   └── Shutdown
│
├── Consumers
│   ├── Pipelines
│   ├── Artifacts
│   ├── Publications
│   └── Webhooks
│
├── Features
│   ├── Pipelines
│   ├── Artifacts
│   ├── Publications
│   └── Maintenance
│
├── BackgroundServices
│   ├── OutboxPublisherService.cs
│   ├── ExecutionRecoveryService.cs
│   ├── InboxCleanupService.cs
│   ├── OutboxCleanupService.cs
│   └── IdempotencyCleanupService.cs
│
├── Jobs
│   ├── Reconciliation
│   ├── Cleanup
│   └── Maintenance
│
├── DependencyInjection.cs
├── WorkerOptions.cs
└── Program.cs
```

---

# 6. Organização por Tipo de Processamento

O Worker combinará duas formas de organização.

## Consumers por mensagem

```text
Consumers
└── Pipelines
    ├── PipelineExecutionRequestedConsumer.cs
    ├── PipelineStepExecutionRequestedConsumer.cs
    └── PipelineExecutionCancellationRequestedConsumer.cs
```

## Background Services por capacidade técnica

```text
BackgroundServices
├── OutboxPublisherService.cs
├── ExecutionRecoveryService.cs
└── TechnicalCleanupService.cs
```

## Features por fluxo de processamento

```text
Features
└── Pipelines
    ├── ExecutePipeline
    ├── ExecuteStep
    └── RecoverExecution
```

---

# 7. Tipos de Processamento

O Worker executará quatro categorias principais.

## Consumo de mensagens

Processamento acionado pelo RabbitMQ.

## Background Services

Processos contínuos dentro do host.

## Jobs recorrentes

Processos executados em intervalos definidos.

## Reconciliação

Processos que verificam divergências ou estados incompletos.

---

# 8. Consumers

Consumers receberão mensagens do broker.

Responsabilidades:

1. Receber envelope.
    
2. Validar metadados.
    
3. Criar escopo de dependências.
    
4. Propagar trace e correlação.
    
5. Verificar Inbox.
    
6. Converter mensagem em Command.
    
7. Despachar para Application.
    
8. Classificar resultado.
    
9. Confirmar, repetir ou rejeitar.
    

Consumers deverão permanecer pequenos.

---

# 9. Exemplo de Consumer

```csharp
public sealed class PipelineExecutionRequestedConsumer
    : IMessageConsumer<PipelineExecutionRequestedV1>
{
    private readonly ISender _sender;

    public PipelineExecutionRequestedConsumer(
        ISender sender)
    {
        _sender = sender;
    }

    public async Task<ConsumerResult> ConsumeAsync(
        MessageContext<PipelineExecutionRequestedV1> context,
        CancellationToken cancellationToken)
    {
        var command = new ExecutePipelineCommand(
            new OrganizationId(
                context.Message.OrganizationId),
            new PipelineExecutionId(
                context.Message.ExecutionId));

        var result = await _sender.Send(
            command,
            cancellationToken);

        return ConsumerResult.From(result);
    }
}
```

O Consumer não deverá:

- Acessar DbContext.
    
- Executar SQL.
    
- Chamar provider de IA diretamente.
    
- Alterar Entity.
    
- Controlar transação manualmente.
    
- Implementar regra de retry de negócio.
    

---

# 10. Contrato Base de Consumer

```csharp
public interface IMessageConsumer<TMessage>
{
    Task<ConsumerResult> ConsumeAsync(
        MessageContext<TMessage> context,
        CancellationToken cancellationToken);
}
```

Contexto:

```csharp
public sealed record MessageContext<TMessage>(
    Guid MessageId,
    string MessageType,
    string MessageVersion,
    OrganizationId OrganizationId,
    string? CorrelationId,
    string? CausationId,
    ActivityContext? ParentActivityContext,
    IReadOnlyDictionary<string, string> Headers,
    TMessage Message);
```

---

# 11. ConsumerResult

```csharp
public sealed record ConsumerResult(
    ConsumerDisposition Disposition,
    Error? Error,
    TimeSpan? RetryAfter)
{
    public static ConsumerResult Success()
        => new(
            ConsumerDisposition.Acknowledge,
            null,
            null);

    public static ConsumerResult Retry(
        Error error,
        TimeSpan retryAfter)
        => new(
            ConsumerDisposition.Retry,
            error,
            retryAfter);

    public static ConsumerResult DeadLetter(
        Error error)
        => new(
            ConsumerDisposition.DeadLetter,
            error,
            null);
}
```

Disposições:

```csharp
public enum ConsumerDisposition
{
    Acknowledge = 1,
    Retry = 2,
    DeadLetter = 3
}
```

---

# 12. Middleware de Consumers

O processamento poderá utilizar um pipeline técnico.

Ordem conceitual:

```text
Mensagem recebida
    ↓
Logging
    ↓
Trace propagation
    ↓
Tenant context
    ↓
Contract validation
    ↓
Inbox
    ↓
Consumer
    ↓
Result classification
    ↓
Ack, Retry ou Dead Letter
```

Middlewares deverão ser reutilizáveis entre Consumers.

---

# 13. Validação do Envelope

Toda mensagem deverá validar:

- MessageId.
    
- MessageType.
    
- MessageVersion.
    
- OrganizationId.
    
- Timestamp.
    
- Content-Type.
    
- Tamanho.
    
- Payload.
    
- Headers obrigatórios.
    

Mensagem inválida deverá normalmente seguir para Dead Letter.

Retry não corrigirá um contrato inválido.

---

# 14. Versionamento de Mensagens

O Worker deverá consumir versões explicitamente suportadas.

Exemplo:

```text
pipeline.execution.requested.v1
```

Quando uma nova versão for criada:

```text
pipeline.execution.requested.v2
```

o Worker poderá:

- Suportar ambas temporariamente.
    
- Utilizar Consumers separados.
    
- Mapear versões para um Command comum.
    
- Remover v1 após período de depreciação.
    

O Consumer não deverá desserializar mensagens desconhecidas como uma versão arbitrária.

---

# 15. Inbox

A Inbox impedirá efeitos duplicados.

Fluxo:

```text
Mensagem recebida
    ↓
Consultar Inbox
    ├── Processada → Ack
    ├── Em processamento válido → Retry
    └── Nova → continuar
```

Depois:

```text
Executar Command
    ↓
Salvar alterações
    ↓
Marcar Inbox como Processed
    ↓
Commit
```

---

# 16. Responsabilidade da Inbox

O Data implementará persistência da Inbox.

O Worker coordenará seu uso.

A Application poderá participar da transação do processamento.

O RabbitMQ continuará entregando mensagens em modelo at-least-once.

A Inbox torna os efeitos idempotentes.

---

# 17. Transação do Consumo

Quando possível:

```text
Abrir transação
    ↓
Registrar Inbox
    ↓
Executar Application
    ↓
Salvar alterações
    ↓
Marcar Inbox concluída
    ↓
Commit
    ↓
Ack RabbitMQ
```

Se o processo morrer antes do commit:

- A mensagem será entregue novamente.
    
- A Inbox não estará concluída.
    
- O processamento poderá recomeçar.
    

Se morrer após commit e antes do Ack:

- A mensagem será entregue novamente.
    
- A Inbox detectará duplicidade.
    
- O Consumer fará Ack sem repetir os efeitos.
    

---

# 18. Ack

Ack deverá ocorrer apenas quando:

- O processamento terminou.
    
- A transação foi confirmada.
    
- A Inbox foi concluída.
    
- O resultado é idempotentemente aceitável.
    
- A mensagem não precisa ser repetida.
    

Nunca confirmar antes da conclusão do processamento.

---

# 19. Nack e Requeue

Requeue imediato deverá ser usado com extremo cuidado.

Fluxo perigoso:

```text
Falha
    ↓
Nack requeue
    ↓
Entrega imediata
    ↓
Falha
    ↓
Loop
```

Esse comportamento pode criar:

- CPU alta.
    
- Sobrecarga do broker.
    
- Logs excessivos.
    
- Bloqueio da fila.
    
- Starvation.
    

Retries deverão possuir atraso.

---

# 20. Retry com Delay

Retries poderão utilizar:

- Retry queues.
    
- Dead-letter exchanges.
    
- Delayed message exchange.
    
- Reagendamento persistido.
    
- Nova mensagem com `NextAttemptAt`.
    

Exemplo:

```text
Falha transitória
    ↓
Retry após 30 segundos
    ↓
Retry após 2 minutos
    ↓
Retry após 10 minutos
```

A estratégia deverá evitar loops imediatos.

---

# 21. Retry Técnico x Retry de Negócio

## Retry técnico

Executado dentro de uma chamada curta.

Exemplos:

- Reconexão.
    
- Timeout transitório.
    
- HTTP 503.
    
- Falha breve de rede.
    

## Retry de negócio

Executado pelo Worker em momento posterior.

Exemplos:

- Provider indisponível por vários minutos.
    
- Limite de quota.
    
- Publicação externa pendente.
    
- Execução interrompida.
    
- Resultado externo incerto.
    

Retries de negócio deverão ser persistidos ou representados por mensagens.

---

# 22. Classificação de Falhas

Falhas deverão ser classificadas como:

```text
Transient
Permanent
Conflict
Duplicate
OutcomeUnknown
Cancelled
Unexpected
```

## Transient

Pode ser repetida.

## Permanent

Retry não resolverá.

## Conflict

O estado mudou e exige nova decisão.

## Duplicate

O efeito já foi aplicado.

## OutcomeUnknown

Não se sabe se o sistema externo concluiu.

## Cancelled

Cancelamento técnico ou de negócio.

## Unexpected

Falha não classificada.

---

# 23. Mapeamento de Resultado

Exemplo conceitual:

```csharp
public static ConsumerResult From(
    Result result)
{
    if (result.IsSuccess)
    {
        return ConsumerResult.Success();
    }

    return result.Error.Type switch
    {
        ErrorType.Validation =>
            ConsumerResult.DeadLetter(result.Error),

        ErrorType.NotFound =>
            ConsumerResult.DeadLetter(result.Error),

        ErrorType.Conflict =>
            ConsumerResult.Success(),

        ErrorType.RateLimit =>
            ConsumerResult.Retry(
                result.Error,
                TimeSpan.FromMinutes(1)),

        ErrorType.Timeout =>
            ConsumerResult.Retry(
                result.Error,
                TimeSpan.FromSeconds(30)),

        ErrorType.Unavailable =>
            ConsumerResult.Retry(
                result.Error,
                TimeSpan.FromMinutes(2)),

        _ =>
            ConsumerResult.DeadLetter(
                result.Error)
    };
}
```

A classificação final deverá considerar:

- Tipo de mensagem.
    
- Número de tentativas.
    
- Natureza da operação.
    
- Possibilidade de duplicação.
    
- Resultado externo incerto.
    

---

# 24. Dead Letter

Mensagens deverão seguir para Dead Letter quando:

- Contrato é inválido.
    
- Versão é desconhecida.
    
- Payload está corrompido.
    
- Erro é permanente.
    
- Número máximo de tentativas foi atingido.
    
- Handler não suporta a mensagem.
    
- Regra de negócio impede processamento definitivo.
    
- Falha manualmente classificada como irreparável.
    

Dead Letter não é lixeira.

Ela deverá possuir:

- Alertas.
    
- Dashboard.
    
- Metadados.
    
- Ferramenta de inspeção.
    
- Replay controlado.
    
- Auditoria.
    

---

# 25. Replay

Replay deverá ser uma ação administrativa explícita.

Antes de repetir:

- Corrigir a causa.
    
- Verificar idempotência.
    
- Verificar estado atual.
    
- Validar versão do contrato.
    
- Registrar operador.
    
- Gerar novo CorrelationId.
    
- Preservar CausationId original.
    
- Limitar quantidade.
    

Uma mensagem em Dead Letter não deverá ser reenviada automaticamente indefinidamente.

---

# 26. Outbox Publisher

O `OutboxPublisherService` publicará mensagens persistidas no PostgreSQL.

Fluxo:

```text
Ler lote da Outbox
    ↓
Claim com lock
    ↓
Publicar no RabbitMQ
    ↓
Aguardar Publisher Confirm
    ↓
Marcar como Processed
```

Se a publicação falhar:

```text
Incrementar tentativa
    ↓
Registrar erro
    ↓
Definir NextAttemptAt
```

---

# 27. Estrutura do OutboxPublisherService

```csharp
public sealed class OutboxPublisherService
    : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxPublisherService> _logger;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessBatchAsync(stoppingToken);

            await Task.Delay(
                TimeSpan.FromSeconds(1),
                stoppingToken);
        }
    }
}
```

O exemplo é conceitual.

A implementação real deverá incluir:

- Tratamento de falha.
    
- Backoff.
    
- Jitter.
    
- Batch.
    
- Locks.
    
- Telemetria.
    
- Shutdown gracioso.
    

---

# 28. Escopo por Lote

Cada lote deverá criar um escopo próprio.

```csharp
await using var scope =
    _scopeFactory.CreateAsyncScope();

var outboxProcessor =
    scope.ServiceProvider
        .GetRequiredService<IOutboxProcessor>();

await outboxProcessor.ProcessBatchAsync(
    cancellationToken);
```

Background Services singleton não deverão capturar serviços scoped diretamente.

---

# 29. Claim da Outbox

O claim deverá impedir que múltiplas instâncias publiquem o mesmo registro simultaneamente.

Estratégias:

- `FOR UPDATE SKIP LOCKED`.
    
- `LockId`.
    
- `LockedUntil`.
    
- Update atômico.
    
- Lease.
    

Exemplo conceitual:

```text
Worker A claim mensagens 1–20
Worker B claim mensagens 21–40
```

---

# 30. Lease da Outbox

Cada claim deverá possuir:

```text
LockId
LockedUntil
```

Se o Worker morrer:

- O lease expira.
    
- Outra instância poderá recuperar.
    
- A mensagem poderá ser publicada novamente.
    
- Idempotência do consumidor continua necessária.
    

A Outbox oferece at-least-once, não exactly-once.

---

# 31. Batch da Outbox

Configurações:

```text
BatchSize
PollingInterval
LockDuration
MaximumAttempts
MaximumParallelism
```

Valores deverão ser medidos.

Um batch excessivo pode:

- Aumentar locks.
    
- Aumentar memória.
    
- Atrasar shutdown.
    
- Sobrecarregar RabbitMQ.
    

---

# 32. Ordenação da Outbox

Mensagens poderão ser ordenadas por:

```text
OccurredAt
Id
```

Entretanto, não deverá ser prometida ordem global entre múltiplas partições e instâncias.

Quando ordem por Aggregate for necessária, utilizar:

- Partition key.
    
- Routing consistente.
    
- Sequence number.
    
- Uma fila por chave lógica.
    
- Validação de versão no consumidor.
    

---

# 33. Falha após Publicação

Cenário:

```text
RabbitMQ confirmou publicação
    ↓
Worker morreu antes de marcar Outbox
```

A mensagem será publicada novamente.

Por isso:

- MessageId deve permanecer o mesmo.
    
- Consumer deve utilizar Inbox.
    
- Operação deve ser idempotente.
    

---

# 34. Execução de Pipeline

O Worker executará pipelines por mensagens.

Fluxo inicial:

```text
PipelineExecutionRequestedV1
    ↓
ExecutePipelineCommand
    ↓
PipelineExecution.Start
    ↓
Registrar próxima etapa
```

Depois:

```text
PipelineStepExecutionRequestedV1
    ↓
ExecutePipelineStepCommand
    ↓
Executar Step
    ↓
Concluir, falhar ou aguardar
```

---

# 35. Estratégia de Mensagens por Etapa

Cada etapa deverá ser processada como unidade independente.

Exemplo:

```text
Research Step
    ↓
Script Step
    ↓
Review Step
    ↓
Approval Step
```

Não executar o pipeline completo dentro de uma única mensagem.

Benefícios:

- Recuperação.
    
- Retry isolado.
    
- Observabilidade.
    
- Escalabilidade.
    
- Checkpoints.
    
- Cancelamento.
    
- Dead Letter por etapa.
    

---

# 36. Claim da Step Execution

Antes de chamar um provider externo:

1. Carregar PipelineExecution.
    
2. Verificar estado.
    
3. Selecionar Step.
    
4. Alterar Step para Running.
    
5. Incrementar AttemptCount.
    
6. Registrar lease.
    
7. Commit.
    

Depois disso, executar o trabalho externo.

---

# 37. Por que Persistir Running Antes da Chamada

Sem persistência:

```text
Worker chama IA
    ↓
Processo morre
    ↓
Nenhum registro da tentativa
```

Com persistência:

```text
Step = Running
Attempt = 2
StartedAt = ...
LeaseUntil = ...
```

O sistema poderá detectar e recuperar a execução.

---

# 38. Execução Externa

Depois do claim:

```text
Carregar contexto
    ↓
Resolver IPipelineStepHandler
    ↓
Executar provider
    ↓
Validar resultado
    ↓
Criar Artifact
    ↓
Concluir Step
```

A chamada externa não deverá manter uma transação de banco aberta.

---

# 39. Conclusão de Step

Fluxo:

```text
Abrir nova transação
    ↓
Carregar Execution
    ↓
Confirmar que o claim ainda pertence à tentativa
    ↓
Criar Artifact
    ↓
CompleteStep
    ↓
Registrar próxima mensagem na Outbox
    ↓
Commit
```

A verificação do claim impede que um resultado atrasado sobrescreva uma tentativa posterior.

---

# 40. Attempt Token

Cada tentativa poderá possuir:

```text
AttemptNumber
AttemptId
LeaseId
```

Exemplo:

```csharp
public sealed record StepAttemptToken(
    StepExecutionId StepExecutionId,
    int AttemptNumber,
    Guid AttemptId);
```

Ao concluir:

- O AttemptId deve corresponder.
    
- O Step ainda deve estar Running.
    
- Nenhuma tentativa mais recente deve existir.
    

---

# 41. Resultado Atrasado

Cenário:

```text
Tentativa 1 demora
    ↓
Lease expira
    ↓
Tentativa 2 começa
    ↓
Tentativa 1 retorna
```

A tentativa 1 não poderá concluir a etapa.

O sistema deverá retornar conflito idempotente e descartar o resultado ou armazená-lo para análise.

---

# 42. PipelineStepHandlerResolver

```csharp
public interface IPipelineStepHandlerResolver
{
    Result<IPipelineStepHandler> Resolve(
        PipelineStepType stepType);
}
```

Implementações registradas:

```text
ResearchStepHandler
ScriptStepHandler
ReviewStepHandler
PublicationStepHandler
```

Tipo desconhecido deverá gerar erro permanente.

---

# 43. Research Step

Fluxo:

```text
Carregar inputs
    ↓
Construir prompt
    ↓
Resolver provider
    ↓
Gerar pesquisa
    ↓
Validar Structured Output
    ↓
Criar Research Artifact
    ↓
Concluir Step
```

O Worker coordena o ciclo de execução.

O comportamento específico fica no Step Handler da Application.

---

# 44. Script Step

Fluxo:

```text
Carregar Research Artifact
    ↓
Construir prompt
    ↓
Gerar roteiro
    ↓
Validar schema
    ↓
Criar Script Artifact
    ↓
Concluir Step
```

A mensagem não deverá carregar todo o conteúdo da pesquisa.

Ela deverá carregar identificadores.

---

# 45. Mensagens Leves

Mensagens deverão transportar:

- IDs.
    
- Versões.
    
- Metadados.
    
- Referências.
    
- Contexto mínimo.
    

Evitar payloads com:

- Arquivos.
    
- Vídeos.
    
- Prompts enormes.
    
- Artefatos completos.
    
- Documentos grandes.
    
- Dados sensíveis.
    

Dados grandes permanecem em PostgreSQL ou Storage.

---

# 46. Cancelamento de Negócio

Um `CancelExecutionCommand` altera o estado da execução.

O Worker deverá verificar cancelamento:

- Antes do claim.
    
- Antes da chamada externa.
    
- Depois da chamada externa.
    
- Antes do commit final.
    
- Entre etapas.
    

Se a execução estiver cancelada:

- Não iniciar nova etapa.
    
- Não publicar próxima mensagem.
    
- Descartar resultado tardio quando apropriado.
    
- Registrar telemetria.
    

---

# 47. CancellationToken Técnico

`CancellationToken` do host representa:

- Shutdown.
    
- Cancelamento do consumo.
    
- Timeout técnico.
    
- Interrupção operacional.
    

Ele não substitui o estado `Cancelled` do Domain.

O token deverá ser propagado para:

- Application.
    
- Providers.
    
- Storage.
    
- Mensageria.
    
- Queries.
    
- Unit of Work.
    
- Delays.
    

---

# 48. Shutdown Gracioso

Quando o host receber sinal de encerramento:

1. Parar de aceitar novas mensagens.
    
2. Cancelar polling.
    
3. Aguardar mensagens em processamento.
    
4. Respeitar timeout máximo.
    
5. Finalizar scopes.
    
6. Encerrar conexões.
    
7. Liberar leases quando possível.
    
8. Registrar estado.
    

Fluxo:

```text
SIGTERM
    ↓
Stop consuming
    ↓
Drain in-flight work
    ↓
Close connections
    ↓
Exit
```

---

# 49. Timeout de Shutdown

Configuração:

```csharp
services.Configure<HostOptions>(options =>
{
    options.ShutdownTimeout =
        TimeSpan.FromSeconds(30);
});
```

O valor deverá considerar:

- Duração média das operações.
    
- Plataforma de deploy.
    
- Grace period do container.
    
- Possibilidade de cancelamento seguro.
    

Operações muito longas deverão utilizar checkpoints.

---

# 50. Trabalho em Andamento no Shutdown

Se a operação puder ser cancelada com segurança:

- Propagar o token.
    
- Marcar tentativa como interrompida posteriormente.
    
- Permitir redelivery.
    

Se o resultado externo for incerto:

- Não repetir imediatamente.
    
- Marcar para reconciliação.
    
- Preservar AttemptId.
    
- Registrar OutcomeUnknown.
    

---

# 51. Backpressure

O Worker deverá limitar a quantidade de trabalho em andamento.

Mecanismos:

- Prefetch do RabbitMQ.
    
- MaximumConcurrency.
    
- Semaphore.
    
- Channels limitados.
    
- Bulkhead.
    
- Rate limit por provider.
    
- Batch size.
    

Sem backpressure, o Worker pode:

- Esgotar memória.
    
- Esgotar connection pool.
    
- Exceder quota.
    
- Sobrecarregar providers.
    
- Aumentar timeouts.
    

---

# 52. Prefetch

Prefetch define quantas mensagens podem ficar não confirmadas por Consumer.

Exemplo conceitual:

```text
Prefetch = 10
Concurrency = 5
```

O valor deverá considerar:

- Duração da mensagem.
    
- Memória.
    
- Número de instâncias.
    
- Limite do banco.
    
- Limite do provider.
    

Não utilizar valores altos por padrão.

---

# 53. Concorrência Global

Configurações possíveis:

```csharp
public sealed class WorkerOptions
{
    public int MaximumConcurrentMessages { get; init; } = 8;

    public int OutboxBatchSize { get; init; } = 50;

    public int PipelineStepConcurrency { get; init; } = 4;

    public int PublicationConcurrency { get; init; } = 2;

    public TimeSpan ShutdownTimeout { get; init; } =
        TimeSpan.FromSeconds(30);
}
```

Configurações obrigatórias deverão ser validadas.

---

# 54. Concorrência por Tipo de Step

Research e Script podem possuir limites diferentes.

Exemplo:

```text
Research: 5 simultâneas
Script: 3 simultâneas
Video Generation: 1 simultânea
Publication: 2 simultâneas
```

Isso protege:

- CPU.
    
- Memória.
    
- Quotas.
    
- Custos.
    
- Dependências.
    

---

# 55. Concorrência por Organization

Uma única Organization não deverá necessariamente consumir toda a capacidade.

Poderão ser aplicados:

- Limites por tenant.
    
- Fair scheduling.
    
- Filas por prioridade.
    
- Rate limiting.
    
- Particionamento.
    

Para o MVP, um limite global será suficiente.

A necessidade de fairness será medida.

---

# 56. Ordenação e Particionamento

Mensagens relacionadas à mesma execução podem precisar de ordenação lógica.

Estratégias:

- Routing key por ExecutionId.
    
- Consistent hashing.
    
- Lock por ExecutionId.
    
- Validação de estado.
    
- Sequence number.
    

Mesmo com ordenação no broker, o Domain deverá rejeitar transições inválidas.

---

# 57. Locks Distribuídos

Locks distribuídos poderão ser utilizados em:

- Jobs singleton.
    
- Reconciliação global.
    
- Processamento de recurso externo não idempotente.
    
- Recuperação de execução.
    

Não utilizar lock distribuído como única proteção.

Combinar com:

- Concurrency token.
    
- Claim persistido.
    
- Idempotência.
    
- Constraints.
    

---

# 58. Prioridade

Filas poderão possuir prioridades:

```text
High
Normal
Low
Maintenance
```

Exemplos:

- Cancelamento: alta prioridade.
    
- Pipeline comum: normal.
    
- Backfill: baixa.
    
- Limpeza: manutenção.
    

Prioridade não deverá causar starvation permanente.

---

# 59. Jobs Recorrentes

Jobs poderão executar:

- Limpeza da Outbox.
    
- Limpeza da Inbox.
    
- Expiração de Idempotency Records.
    
- Recuperação de execuções.
    
- Reconciliação externa.
    
- Atualização de metadados.
    
- Retenção de arquivos temporários.
    

Jobs deverão ser idempotentes.

---

# 60. Scheduling

Para jobs simples, um `BackgroundService` com intervalo poderá ser suficiente.

Exemplo:

```csharp
public sealed class InboxCleanupService
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        using var timer =
            new PeriodicTimer(
                TimeSpan.FromHours(1));

        while (await timer.WaitForNextTickAsync(
                   stoppingToken))
        {
            await CleanupAsync(stoppingToken);
        }
    }
}
```

Para agendamento avançado, poderá ser utilizado um scheduler dedicado.

---

# 61. Scheduler Externo

Um scheduler externo poderá ser preferido quando houver:

- Cron complexo.
    
- Histórico.
    
- Painel operacional.
    
- Retry de jobs.
    
- Coordenação distribuída.
    
- Dependências entre jobs.
    
- Calendários.
    

Possíveis opções futuras:

- Hangfire.
    
- Quartz.
    
- Azure Functions.
    
- Kubernetes CronJob.
    
- n8n.
    

A escolha não deverá ser antecipada sem necessidade.

---

# 62. Jobs Singleton

Quando apenas uma instância puder executar:

1. Adquirir lock distribuído.
    
2. Executar em lotes.
    
3. Renovar lock.
    
4. Liberar ao finalizar.
    
5. Permitir recuperação após expiração.
    

O lock deverá possuir token de propriedade.

---

# 63. Recuperação de Execuções

O `ExecutionRecoveryService` identificará execuções que parecem travadas.

Critérios possíveis:

```text
Status = Running
LeaseUntil < agora
LastHeartbeatAt antigo
Attempt sem conclusão
```

A recuperação deverá ser cuidadosa.

Uma execução lenta não é necessariamente travada.

---

# 64. Heartbeat

Etapas muito longas poderão atualizar heartbeat.

Campos possíveis:

```text
LastHeartbeatAt
LeaseUntil
WorkerInstanceId
```

O heartbeat deverá:

- Ser leve.
    
- Possuir intervalo razoável.
    
- Não criar escrita excessiva.
    
- Falhar sem interromper automaticamente o trabalho.
    
- Ser comparado com tolerância.
    

---

# 65. Detecção de Stalled Step

Exemplo:

```text
Step = Running
LeaseUntil = 14:00
Agora = 14:10
```

Possíveis decisões:

- Reagendar.
    
- Marcar como falha.
    
- Aguardar janela adicional.
    
- Reconciliar provider.
    
- Solicitar intervenção.
    

A decisão depende do tipo de Step.

---

# 66. Recovery Policy

```csharp
public interface IExecutionRecoveryPolicy
{
    RecoveryDecision Evaluate(
        StalledExecutionContext context);
}
```

Resultado:

```csharp
public sealed record RecoveryDecision(
    RecoveryAction Action,
    TimeSpan? RetryAfter,
    string Reason);
```

Ações:

```text
Retry
Fail
Reconcile
Wait
ManualIntervention
```

---

# 67. Reconciliação

Reconciliação verifica o estado externo quando o resultado é incerto.

Exemplos:

- Vídeo foi publicado?
    
- Arquivo foi armazenado?
    
- Workflow do n8n terminou?
    
- Provider concluiu a operação?
    
- Callback foi perdido?
    

Fluxo:

```text
OutcomeUnknown
    ↓
Reconciliation Job
    ↓
Consultar sistema externo
    ├── Concluído → atualizar estado
    ├── Não encontrado → repetir com segurança
    └── Indeterminado → aguardar ou alertar
```

---

# 68. Reconciliation Command

Exemplo:

```csharp
public sealed record ReconcilePublicationCommand(
    OrganizationId OrganizationId,
    PublicationId PublicationId)
    : ICommand;
```

O Worker agenda.

A Application coordena.

Infrastructure consulta o serviço externo.

---

# 69. Limpeza da Outbox

Registros processados poderão ser removidos após retenção.

Fluxo:

```text
ProcessedAt < limite
    ↓
Delete batch
```

Regras:

- Processar em lotes.
    
- Evitar locks longos.
    
- Registrar quantidade.
    
- Respeitar auditoria.
    
- Não excluir pendentes.
    
- Não excluir Dead Messages sem política.
    

---

# 70. Limpeza da Inbox

A Inbox deverá ser retida por período superior à janela de redelivery.

Exemplo:

```text
ProcessedAt < 30 dias
```

Antes de reduzir retenção, considerar:

- Replay.
    
- Recovery.
    
- Atrasos do broker.
    
- Auditoria.
    
- Backups.
    

---

# 71. Limpeza de Idempotência

Registros expirados poderão ser removidos em lotes.

A expiração deve respeitar o contrato público.

Se a API promete idempotência por 24 horas, não remover antes disso.

---

# 72. Retenção de Artifacts Temporários

Artifacts temporários poderão possuir:

```text
ExpiresAt
RetentionPolicy
```

O Worker poderá:

1. Identificar expirados.
    
2. Confirmar que não são referenciados.
    
3. Excluir no Storage.
    
4. Atualizar metadados.
    
5. Registrar auditoria.
    

Falhas parciais deverão ser reconciliadas.

---

# 73. Worker Identity

Cada instância deverá possuir identificador.

Exemplo:

```text
WorkerInstanceId
```

Possível formato:

```text
hostname-processid-random
```

Usos:

- Leases.
    
- Logs.
    
- Diagnóstico.
    
- Heartbeat.
    
- Claims.
    
- Shutdown.
    

Não utilizar o ID como dimensão de métrica de alta cardinalidade.

---

# 74. Contexto de Processamento

Cada mensagem deverá estabelecer:

- MessageId.
    
- CorrelationId.
    
- CausationId.
    
- TraceId.
    
- OrganizationId.
    
- WorkerInstanceId.
    
- ConsumerName.
    
- AttemptCount.
    

Esses valores deverão acompanhar logs e traces.

---

# 75. Propagação de Trace

O Worker deverá extrair:

```text
traceparent
tracestate
```

e iniciar um span consumidor.

Fluxo:

```text
API Trace
    ↓
Outbox headers
    ↓
RabbitMQ
    ↓
Worker Trace
    ↓
Provider Trace
```

A ligação deverá permitir investigação ponta a ponta.

---

# 76. Activities

Spans sugeridos:

```text
messaging.consume
worker.dispatch
pipeline.execute
pipeline.step.execute
outbox.publish
execution.recover
publication.reconcile
maintenance.cleanup
```

A Activity deverá incluir atributos de baixa cardinalidade.

---

# 77. Logs

Logs de mensagem deverão incluir:

```text
MessageType
MessageVersion
MessageId
Consumer
OrganizationId
CorrelationId
Attempt
Disposition
Duration
ErrorCode
```

Não registrar payload completo automaticamente.

---

# 78. Logs de Pipeline

Contexto:

```text
ExecutionId
PipelineId
PipelineVersion
StepExecutionId
StepType
AttemptNumber
Provider
Model
Duration
Result
```

Prompts e Artifacts completos não deverão aparecer em logs comuns.

---

# 79. Métricas

Métricas iniciais:

```text
worker.messages.received
worker.messages.processed
worker.messages.retried
worker.messages.dead_lettered
worker.message.duration
worker.inflight.count

outbox.pending.count
outbox.publish.duration
outbox.publish.failures
outbox.oldest.age

pipeline.executions.started
pipeline.executions.completed
pipeline.executions.failed
pipeline.step.duration
pipeline.step.retries
pipeline.stalled.count

worker.recovery.actions
worker.shutdown.duration
```

---

# 80. Cardinalidade de Métricas

Não utilizar como labels:

- MessageId.
    
- ExecutionId.
    
- ProjectId.
    
- UserId.
    
- CorrelationId.
    
- WorkerInstanceId.
    
- PromptVersion muito variável.
    
- Error message.
    

Labels apropriadas:

```text
consumer
message_type
step_type
provider
model_family
result
error_code_controlado
```

---

# 81. Health Checks

O Worker deverá possuir health checks.

Possíveis endpoints internos:

```text
/health/live
/health/ready
```

Caso o Worker não exponha HTTP público, poderá hospedar um servidor mínimo somente para operação.

---

# 82. Liveness

Deverá indicar que o processo está vivo.

Não deverá depender de serviços externos.

Falhas transitórias de PostgreSQL ou RabbitMQ não deverão reiniciar o Worker indefinidamente sem avaliação.

---

# 83. Readiness

Poderá verificar:

- PostgreSQL.
    
- RabbitMQ.
    
- Configuração.
    
- Topology.
    
- Serviços obrigatórios.
    
- Capacidade de criar scopes.
    
- Estado de inicialização.
    

Se o Worker não estiver ready:

- Não deverá consumir novas mensagens.
    
- Poderá continuar finalizando trabalho em andamento.
    
- A plataforma poderá removê-lo do pool.
    

---

# 84. Health por Consumer

Poderão existir sinais internos para:

- Consumer parado.
    
- Conexão encerrada.
    
- Outbox atrasada.
    
- Fila crescendo.
    
- Circuit breaker aberto.
    
- Falhas consecutivas.
    
- Heartbeat interno.
    

Nem todos deverão tornar o processo unhealthy.

Alguns devem gerar alertas.

---

# 85. Startup

Ordem conceitual:

```text
Carregar configuração
    ↓
Validar Options
    ↓
Registrar dependências
    ↓
Conectar dependências essenciais
    ↓
Inicializar topology
    ↓
Iniciar health server
    ↓
Iniciar consumers
    ↓
Iniciar background services
```

O Worker não deverá começar a consumir antes de estar configurado corretamente.

---

# 86. Program.cs

```csharp
var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddApplication()
    .AddData(builder.Configuration)
    .AddInfrastructure(builder.Configuration)
    .AddWorker(builder.Configuration);

var host = builder.Build();

await host.RunAsync();
```

O `Program.cs` deverá permanecer pequeno.

---

# 87. DependencyInjection

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddWorker(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddWorkerOptions(configuration)
            .AddMessageConsumers()
            .AddWorkerBackgroundServices()
            .AddWorkerHealthChecks()
            .AddWorkerTelemetry();

        return services;
    }
}
```

---

# 88. WorkerOptions

```csharp
public sealed class WorkerOptions
{
    public const string SectionName = "Worker";

    public int MaximumConcurrentMessages { get; init; } = 8;

    public int PipelineStepMaximumConcurrency { get; init; } = 4;

    public int OutboxBatchSize { get; init; } = 50;

    public int OutboxMaximumAttempts { get; init; } = 20;

    public int ConsumerMaximumAttempts { get; init; } = 8;

    public int ShutdownTimeoutSeconds { get; init; } = 30;

    public int RecoveryIntervalSeconds { get; init; } = 60;

    public int StalledExecutionThresholdSeconds { get; init; } = 300;
}
```

Valores deverão ser validados.

---

# 89. Configuração por Consumer

Consumers diferentes poderão possuir configurações próprias.

Exemplo:

```text
PipelineExecutionConsumerOptions
PublicationConsumerOptions
MediaProcessingConsumerOptions
```

Campos:

- Queue.
    
- Prefetch.
    
- Concurrency.
    
- MaximumAttempts.
    
- Retry schedule.
    
- Dead-letter routing.
    
- Timeout.
    

---

# 90. Separação Futura de Workers

Inicialmente:

```text
InfiniteContentAI.Worker
```

Com crescimento:

```text
InfiniteContentAI.PipelineWorker
InfiniteContentAI.OutboxWorker
InfiniteContentAI.PublicationWorker
InfiniteContentAI.MediaWorker
InfiniteContentAI.MaintenanceWorker
```

A separação deverá ocorrer quando existir benefício operacional.

Exemplos:

- Escalabilidade diferente.
    
- Dependências pesadas.
    
- Quotas diferentes.
    
- Risco isolado.
    
- Deploy independente.
    
- Hardware específico.
    

---

# 91. Worker de Pipeline

Responsável por:

- Start Execution.
    
- Execute Step.
    
- Resume.
    
- Cancel.
    
- Recovery de execução.
    
- Checkpoints.
    

---

# 92. Worker de Outbox

Responsável por:

- Claim.
    
- Publish.
    
- Publisher confirms.
    
- Retry.
    
- Métricas de atraso.
    
- Limpeza técnica.
    

---

# 93. Worker de Publication

Responsável por:

- YouTube.
    
- WordPress.
    
- LinkedIn.
    
- Reconciliação.
    
- OutcomeUnknown.
    
- Idempotência externa.
    

---

# 94. Worker de Media

Poderá exigir:

- Mais CPU.
    
- Mais memória.
    
- GPU.
    
- FFMpeg.
    
- Serviço Python.
    
- Storage intensivo.
    

Por isso, poderá ser separado futuramente.

---

# 95. Worker de Maintenance

Responsável por:

- Limpezas.
    
- Backfills.
    
- Retenção.
    
- Reconciliação.
    
- Reprocessamentos administrativos.
    

Deverá utilizar baixa prioridade.

---

# 96. Escalabilidade Horizontal

O Worker deverá suportar múltiplas instâncias.

Requisitos:

- Consumers concorrentes.
    
- Inbox.
    
- Claims.
    
- Leases.
    
- Concorrência otimista.
    
- Jobs singleton quando necessário.
    
- Shutdown gracioso.
    
- Configuração externa.
    
- Sem estado local indispensável.
    

---

# 97. Estado Local

O Worker não deverá depender de estado mantido apenas em memória para correção.

Estado em memória poderá ser usado para:

- Cache.
    
- Métricas.
    
- Limites locais.
    
- Contexto da mensagem.
    
- Buffers.
    

Estados essenciais deverão estar em:

- PostgreSQL.
    
- RabbitMQ.
    
- Redis, quando apropriado.
    
- Storage.
    

---

# 98. Deploy

O Worker deverá ser implantado independentemente da API quando possível.

Fluxo:

```text
Build
    ↓
Tests
    ↓
Migrations
    ↓
Deploy Infrastructure dependencies
    ↓
Deploy API
    ↓
Deploy Worker
```

A ordem poderá variar conforme compatibilidade dos contratos.

---

# 99. Compatibilidade no Deploy

Durante deploy gradual:

- Workers antigos podem continuar consumindo.
    
- Workers novos podem entrar gradualmente.
    
- Mensagens precisam ser compatíveis.
    
- Topology não pode quebrar consumidores existentes.
    
- Migrations devem seguir expand and contract.
    

---

# 100. Rolling Update

Durante rolling update:

1. Instância antiga recebe SIGTERM.
    
2. Para de consumir.
    
3. Finaliza mensagens.
    
4. Nova instância fica ready.
    
5. Broker redistribui mensagens.
    

O grace period deve ser maior que o shutdown timeout.

---

# 101. Autoscaling

Escala poderá considerar:

- Tamanho da fila.
    
- Idade da mensagem mais antiga.
    
- Quantidade de mensagens em processamento.
    
- CPU.
    
- Memória.
    
- Latência.
    
- Quota externa.
    
- Outbox pendente.
    

Somente tamanho da fila pode ser insuficiente.

---

# 102. Escala e Providers

Aumentar Workers pode exceder:

- Quota da IA.
    
- Rate limit.
    
- Pool de banco.
    
- Conexões RabbitMQ.
    
- Redis.
    
- Storage.
    

Autoscaling deverá considerar limites externos.

---

# 103. Graceful Degradation

Quando um provider opcional estiver indisponível:

- Workers de outras features continuam.
    
- Mensagens afetadas são reagendadas.
    
- Circuit breaker protege o provider.
    
- Readiness geral pode permanecer saudável.
    
- Alertas são gerados.
    

Não derrubar todo o host por uma integração opcional.

---

# 104. Poison Messages

Uma Poison Message falha repetidamente.

Sinais:

- Mesmo MessageId.
    
- Mesmo ErrorCode.
    
- Mesmo stack trace.
    
- Tentativas esgotadas.
    

Ação:

- Dead Letter.
    
- Alerta.
    
- Inspeção.
    
- Correção.
    
- Replay controlado.
    

---

# 105. Payload Excessivo

Mensagens acima do limite deverão ser rejeitadas.

O payload grande deverá ser armazenado externamente e referenciado por ID.

O Consumer deverá validar tamanho antes da desserialização completa quando possível.

---

# 106. Segurança

O Worker deverá aplicar:

- Secrets externos.
    
- TLS.
    
- Autenticação RabbitMQ.
    
- Menor privilégio.
    
- Validação de contratos.
    
- Redaction.
    
- Isolamento por Organization.
    
- Proteção de arquivos.
    
- Network policies.
    
- Imagens de container seguras.
    
- Execução sem root quando possível.
    

---

# 107. Credenciais

Consumers não deverão receber secrets por mensagem.

Mensagens poderão conter:

```text
CredentialReferenceId
```

A Infrastructure resolve a credencial.

Secrets não deverão aparecer em:

- Logs.
    
- Headers.
    
- Dead Letter payload adicional.
    
- Métricas.
    
- Exceptions públicas.
    

---

# 108. Organization Context

Toda mensagem tenant-scoped deverá possuir OrganizationId.

O Worker deverá:

- Validar OrganizationId.
    
- Propagá-la ao Command.
    
- Filtrar dados por Organization.
    
- Incluí-la em chaves.
    
- Preservá-la nos eventos seguintes.
    

Uma mensagem não poderá alterar dados de Organization diferente.

---

# 109. Testes Unitários

Testar:

- Consumer mapping.
    
- Result classification.
    
- Retry policy.
    
- Dead Letter decision.
    
- Recovery policy.
    
- Backoff.
    
- Envelope validation.
    
- Context propagation.
    
- Shutdown coordination.
    
- Attempt token validation.
    

---

# 110. Testes de Consumer

Exemplo:

```csharp
[Fact]
public async Task Consumer_ShouldDispatchCommand()
{
    var sender = new Mock<ISender>();

    sender
        .Setup(item => item.Send(
            It.IsAny<ExecutePipelineCommand>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success());

    var consumer =
        new PipelineExecutionRequestedConsumer(
            sender.Object);

    var result = await consumer.ConsumeAsync(
        MessageContextFactory.Valid<
            PipelineExecutionRequestedV1>(),
        CancellationToken.None);

    result.Disposition.Should().Be(
        ConsumerDisposition.Acknowledge);
}
```

---

# 111. Testes de Integração RabbitMQ

Testar:

- Publicação.
    
- Consumo.
    
- Ack.
    
- Retry.
    
- Dead Letter.
    
- Headers.
    
- Correlation.
    
- Redelivery.
    
- Prefetch.
    
- Shutdown.
    
- Multiple instances.
    

Utilizar RabbitMQ real em container.

---

# 112. Teste da Inbox

Cenário:

1. Publicar mensagem.
    
2. Consumer processa.
    
3. Forçar redelivery.
    
4. Consumer recebe novamente.
    
5. Confirmar que o efeito ocorreu uma vez.
    

---

# 113. Teste de Falha após Commit

Simular:

1. Processar Command.
    
2. Commit.
    
3. Falhar antes do Ack.
    
4. Redelivery.
    
5. Inbox detecta duplicidade.
    
6. Ack sem repetir.
    

Esse é um teste crítico.

---

# 114. Teste da Outbox

Cenário:

1. Criar registro Outbox.
    
2. Worker publica.
    
3. RabbitMQ confirma.
    
4. Data marca como processado.
    
5. Confirmar headers e MessageId.
    

Outro cenário:

1. Publicar.
    
2. Falhar antes de marcar.
    
3. Publicar novamente.
    
4. Consumer deduplica.
    

---

# 115. Teste de Retry

Validar:

- Erro transitório gera Retry.
    
- Delay cresce.
    
- Jitter está dentro do intervalo.
    
- Máximo de tentativas.
    
- Erro permanente não é repetido.
    
- Retry-After é respeitado.
    

---

# 116. Teste de Dead Letter

Validar:

- Payload inválido.
    
- Versão desconhecida.
    
- Tentativas esgotadas.
    
- Headers originais preservados.
    
- ErrorCode registrado.
    
- Mensagem removida da fila principal.
    

---

# 117. Teste de Concorrência

Executar duas instâncias tentando:

- Processar mesma mensagem.
    
- Claim mesma Outbox.
    
- Claim mesma Step.
    
- Recuperar mesma execução.
    

Confirmar:

- Um efeito de negócio.
    
- Um claim ativo.
    
- Conflitos tratados.
    
- Sem corrupção.
    

---

# 118. Teste de Shutdown

Cenário:

1. Iniciar processamento.
    
2. Enviar sinal de shutdown.
    
3. Confirmar que novas mensagens não são aceitas.
    
4. Confirmar finalização da atual.
    
5. Confirmar encerramento dentro do limite.
    

Outro cenário:

- Operação excede limite.
    
- CancellationToken é disparado.
    
- Mensagem é redeliverada.
    

---

# 119. Teste de Recovery

Criar:

```text
Step = Running
LeaseUntil expirado
```

Executar Recovery.

Confirmar decisão:

- Retry.
    
- Fail.
    
- Reconcile.
    
- Wait.
    

---

# 120. Testes End-to-End

Fluxo do MVP:

```text
POST StartExecution
    ↓
Outbox
    ↓
RabbitMQ
    ↓
Worker
    ↓
Research Step
    ↓
Artifact
    ↓
Script Step
    ↓
Artifact
    ↓
Execution Completed
```

O teste deverá validar todo o fluxo com providers fake.

---

# 121. Testes de Carga

Medir:

- Throughput.
    
- Latência.
    
- Tempo em fila.
    
- Conexões.
    
- CPU.
    
- Memória.
    
- DB pool.
    
- Rate limits.
    
- Retries.
    
- Backpressure.
    

Cenários:

- Muitas execuções curtas.
    
- Poucas execuções longas.
    
- Provider lento.
    
- RabbitMQ indisponível.
    
- PostgreSQL lento.
    
- Dead Letter crescendo.
    

---

# 122. Chaos Testing

Cenários futuros:

- Matar Worker durante chamada.
    
- Reiniciar RabbitMQ.
    
- Derrubar PostgreSQL.
    
- Criar timeouts.
    
- Falhar Publisher Confirm.
    
- Fazer lease expirar.
    
- Retornar 429.
    
- Corromper payload.
    
- Duplicar mensagem.
    

O objetivo é confirmar recuperação.

---

# 123. Antipadrões

## Regra de negócio no Consumer

Consumer deve delegar para Application.

## DbContext no Consumer

Acesso a banco deve ocorrer por abstrações.

## Pipeline completo em uma mensagem

Dividir em Steps.

## Ack antes do commit

Pode perder processamento.

## Requeue imediato infinito

Cria loop.

## Retry em erro permanente

Desperdiça recursos.

## Thread.Sleep

Utilizar timers, scheduling ou delay assíncrono.

## BackgroundService capturando scoped service

Criar escopo por operação.

## Estado apenas em memória

Impede recuperação.

## Mensagens gigantes

Utilizar referências.

## Lock sem expiração

Pode bloquear permanentemente.

## Lock como única garantia

Combinar com idempotência e concorrência.

## Shutdown abrupto

Implementar drain.

## Exceção genérica escondida

Preservar observabilidade.

## Dead Letter sem operação

Criar processo de inspeção e replay.

---

# 124. Regras Arquiteturais

1. Worker é host de processamento.
    
2. Consumers permanecem pequenos.
    
3. Consumers delegam para Application.
    
4. Consumers não acessam DbContext.
    
5. Consumers não executam SQL.
    
6. Consumers não chamam SDKs diretamente.
    
7. Mensagens utilizam Contracts.
    
8. Toda mensagem possui MessageId.
    
9. Toda mensagem tenant-scoped possui OrganizationId.
    
10. Inbox protege efeitos duplicados.
    
11. Ack ocorre depois do commit.
    
12. Retry imediato infinito é proibido.
    
13. Erros permanentes vão para Dead Letter.
    
14. Retries longos são assíncronos.
    
15. Operações longas possuem checkpoints.
    
16. Cada Pipeline Step é unidade independente.
    
17. Chamada externa não mantém transação aberta.
    
18. Tentativas possuem AttemptId.
    
19. Resultados atrasados não sobrescrevem tentativas novas.
    
20. CancellationToken é propagado.
    
21. Cancelamento técnico e de negócio são distintos.
    
22. Shutdown gracioso é obrigatório.
    
23. Backpressure é obrigatório.
    
24. Concorrência possui limites.
    
25. Leases possuem expiração.
    
26. Outbox suporta múltiplas instâncias.
    
27. Jobs são idempotentes.
    
28. Reconciliação trata resultados incertos.
    
29. Logs não expõem payloads sensíveis.
    
30. Métricas possuem baixa cardinalidade.
    
31. Health checks são operacionais.
    
32. Worker suporta escalabilidade horizontal.
    
33. Estado essencial não fica apenas em memória.
    
34. Dead Letter possui processo de replay.
    
35. Testes validam redelivery e duplicidade.
    

---

# 125. Escopo do MVP

A primeira versão do Worker deverá implementar:

## Consumers

```text
PipelineExecutionRequestedConsumer
PipelineStepExecutionRequestedConsumer
```

## Background Services

```text
OutboxPublisherService
ExecutionRecoveryService
```

## Pipeline

```text
Start Pipeline
Execute Research Step
Execute Script Step
Complete Execution
Fail Execution
```

## Mensageria

```text
Inbox
Ack
Retry básico
Dead Letter
Trace propagation
```

## Operação

```text
Graceful shutdown
Health checks
Logs estruturados
Métricas básicas
Concurrency limits
```

---

# 126. Componentes Adiáveis

Não obrigatórios inicialmente:

- Workers separados.
    
- Prioridades avançadas.
    
- Fair scheduling por tenant.
    
- Scheduler complexo.
    
- Retry dashboard.
    
- Replay UI.
    
- Media Worker.
    
- Publication Worker.
    
- Heartbeat frequente.
    
- Locks Redis.
    
- Autoscaling sofisticado.
    
- Multi-região.
    
- Exactly-once.
    
- Workflow visual.
    

---

# 127. Ordem de Implementação

## Etapa 1 — Fundação

- Criar projeto Worker.
    
- Criar Program.cs.
    
- Criar AddWorker.
    
- Configurar Options.
    
- Configurar health server.
    
- Configurar logs.
    
- Configurar shutdown.
    

## Etapa 2 — Outbox

- Criar OutboxPublisherService.
    
- Claim em lote.
    
- Publicar RabbitMQ.
    
- Publisher Confirm.
    
- Retry.
    
- Métricas.
    
- Testes.
    

## Etapa 3 — Consumer Base

- MessageContext.
    
- IMessageConsumer.
    
- ConsumerResult.
    
- Middleware.
    
- Inbox.
    
- Ack e Dead Letter.
    

## Etapa 4 — Pipeline Execution

- PipelineExecutionRequestedConsumer.
    
- ExecutePipelineCommand.
    
- Publicação da primeira Step.
    

## Etapa 5 — Steps

- PipelineStepExecutionRequestedConsumer.
    
- Claim.
    
- Research Step.
    
- Script Step.
    
- Completion.
    
- Próxima mensagem.
    

## Etapa 6 — Recovery

- Lease.
    
- AttemptId.
    
- Recovery Service.
    
- Stalled executions.
    
- Retry policy.
    

## Etapa 7 — Operação

- Limpeza.
    
- Dashboards.
    
- Alertas.
    
- Carga.
    
- Chaos tests.
    

---

# 128. Checklist para Novo Consumer

- Qual mensagem consome?
    
- Qual versão?
    
- Qual queue?
    
- Qual routing key?
    
- Possui MessageId?
    
- Possui OrganizationId?
    
- O payload é pequeno?
    
- Existe Inbox?
    
- Qual Command será despachado?
    
- Qual é o timeout?
    
- Quais falhas são transitórias?
    
- Quais são permanentes?
    
- Qual retry schedule?
    
- Qual máximo de tentativas?
    
- Qual Dead Letter?
    
- O Consumer propaga CancellationToken?
    
- O Ack ocorre após commit?
    
- Existem testes de duplicidade?
    
- Existem métricas?
    
- O payload é protegido nos logs?
    

---

# 129. Checklist para Novo Background Service

- Precisa realmente ser contínuo?
    
- Pode ser job recorrente?
    
- Cria scope por operação?
    
- Possui intervalo configurável?
    
- Possui backoff?
    
- Respeita CancellationToken?
    
- Possui shutdown gracioso?
    
- É idempotente?
    
- Suporta múltiplas instâncias?
    
- Precisa de lock?
    
- O lock expira?
    
- Trabalha em lotes?
    
- Produz métricas?
    
- Possui health signal?
    
- Possui testes?
    

---

# 130. Checklist para Novo Pipeline Step

- Possui StepType?
    
- Inputs são referências?
    
- Output é Artifact?
    
- É idempotente?
    
- Possui timeout?
    
- Possui retry policy?
    
- Pode ser cancelado?
    
- Possui checkpoint?
    
- Registra AttemptId?
    
- Protege resultado atrasado?
    
- Structured Output é validado?
    
- Provider é registrado?
    
- Tokens são registrados?
    
- Falhas são classificadas?
    
- Possui testes fake?
    
- Possui teste de recovery?
    

---

# 131. Critérios de Qualidade

O Worker será considerado saudável quando:

- Mensagens puderem ser entregues mais de uma vez sem duplicar efeitos.
    
- Ack ocorrer somente após persistência.
    
- Falhas transitórias forem repetidas com atraso.
    
- Falhas permanentes forem isoladas.
    
- Pipelines puderem ser retomados após reinício.
    
- Resultados atrasados não corromperem estado.
    
- Consumers permanecerem pequenos.
    
- Application concentrar os casos de uso.
    
- Operações externas não manterem transações abertas.
    
- Shutdown não perder mensagens.
    
- Múltiplas instâncias puderem trabalhar juntas.
    
- Backpressure proteger dependências.
    
- Outbox não perder mensagens.
    
- Inbox impedir duplicidade.
    
- Reconciliação resolver resultados incertos.
    
- Logs e traces permitirem investigação ponta a ponta.
    

---

# 132. Fluxo Completo do MVP

```text
API
    ↓
StartExecutionCommand
    ↓
PostgreSQL
    ├── PipelineExecution = Queued
    └── OutboxMessage
    ↓
OutboxPublisherService
    ↓
RabbitMQ
    ↓
PipelineExecutionRequestedConsumer
    ↓
ExecutePipelineCommand
    ├── Execution = Running
    └── Outbox: Research Step
    ↓
RabbitMQ
    ↓
PipelineStepExecutionRequestedConsumer
    ↓
Claim Research Step
    ↓
ResearchStepHandler
    ↓
AI Provider
    ↓
Research Artifact
    ↓
Complete Research Step
    ↓
Outbox: Script Step
    ↓
RabbitMQ
    ↓
Claim Script Step
    ↓
ScriptStepHandler
    ↓
AI Provider
    ↓
Script Artifact
    ↓
Complete Script Step
    ↓
PipelineExecution = Completed
```

---

# 133. Documentos Relacionados

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
04 - Backend/Data.md
04 - Backend/Infrastructure.md
04 - Backend/Contracts.md
04 - Backend/Shared Kernel.md
```

---

# 134. Filosofia Final

O Worker deverá transformar mensagens e agendamentos em execução confiável de casos de uso.

Seu código deverá expressar ações como:

```text
Receber mensagem
Validar envelope
Registrar Inbox
Despachar Command
Executar Step
Publicar Outbox
Aplicar retry
Recuperar execução
Confirmar mensagem
```

Ele não deverá expressar ações como:

```text
Alterar Entity diretamente
Executar SQL
Implementar regra de negócio
Construir request específico de SDK
Resolver persistência relacional
Mapear resposta HTTP
```

Essas responsabilidades pertencem às outras camadas.

A regra principal será:

> O Worker garante que o trabalho assíncrono seja executado, repetido e recuperado com segurança; a Application coordena o caso de uso e o Domain protege o estado.

Quando esse limite for respeitado, o Infinite Content AI poderá processar pipelines longos, falhar, reiniciar e escalar horizontalmente sem perder consistência.