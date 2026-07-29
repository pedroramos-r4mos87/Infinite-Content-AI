# Tratamento de Erros e Resiliência

## Objetivo

Definir como o Infinite Content AI detectará, classificará, propagará, registrará e recuperará falhas.

A arquitetura deverá assumir que falhas acontecerão.

Providers ficarão indisponíveis.

Mensagens poderão ser entregues mais de uma vez.

Workers poderão ser interrompidos.

Chamadas poderão exceder o timeout.

Respostas de IA poderão vir inválidas.

Publicações poderão falhar parcialmente.

O objetivo da resiliência não é eliminar falhas.

O objetivo é impedir que falhas isoladas provoquem perda de dados, inconsistência, duplicidade ou indisponibilidade generalizada.

---

# Princípios

A estratégia de tratamento de erros seguirá os seguintes princípios:

- Falhas esperadas devem ser explícitas.
    
- Exceções representam situações excepcionais.
    
- Erros devem ser normalizados.
    
- Retentativas devem ser limitadas.
    
- Toda operação externa deve possuir timeout.
    
- Operações críticas devem ser idempotentes.
    
- Transações devem permanecer curtas.
    
- Falhas parciais devem ser recuperáveis.
    
- Pipelines devem possuir checkpoints.
    
- Mensagens devem suportar reprocessamento.
    
- Fallback deve respeitar custo e qualidade.
    
- Erros devem ser observáveis.
    
- Falhas não devem expor detalhes internos.
    
- A recuperação deve preservar o estado válido.
    
- O sistema deve falhar de forma segura.
    

---

# Categorias de Falha

As falhas poderão ser classificadas em:

```text
Falhas de Negócio
Falhas de Validação
Falhas de Autorização
Falhas de Concorrência
Falhas Transitórias
Falhas Permanentes
Falhas de Integração
Falhas de Infraestrutura
Falhas de Consistência
Falhas Inesperadas
```

A classificação determina o comportamento do sistema.

---

# Falhas de Negócio

Representam situações previstas pelas regras do produto.

Exemplos:

- Conteúdo ainda não aprovado.
    
- Pipeline em estado inválido.
    
- Limite financeiro excedido.
    
- Publicação não permitida.
    
- Projeto desativado.
    
- Operação já concluída.
    
- Artefato incompatível.
    
- Aprovação expirada.
    

Essas falhas não deverão ser tratadas como problemas técnicos.

---

# Falhas de Validação

Representam entradas inválidas.

Exemplos:

- Campo obrigatório ausente.
    
- Formato incorreto.
    
- Identificador inválido.
    
- Valor fora do limite.
    
- URL inválida.
    
- Payload incompatível.
    
- Arquivo não permitido.
    

Deverão ser detectadas o mais cedo possível.

---

# Falhas de Autorização

Representam operações não permitidas.

Exemplos:

- Usuário sem permissão.
    
- Recurso de outra organização.
    
- Credencial revogada.
    
- Token expirado.
    
- Agente tentando utilizar ferramenta não permitida.
    

Não deverão ser repetidas automaticamente.

---

# Falhas de Concorrência

Ocorrem quando múltiplos processos tentam alterar o mesmo estado.

Exemplos:

- Dois Workers avançando o mesmo pipeline.
    
- Duas aprovações simultâneas.
    
- Retomada duplicada.
    
- Atualização concorrente de publicação.
    
- Duas mensagens tentando processar a mesma etapa.
    

Deverão ser detectadas e tratadas explicitamente.

---

# Falhas Transitórias

São falhas temporárias que podem desaparecer em uma nova tentativa.

Exemplos:

- Timeout de rede.
    
- HTTP 429.
    
- HTTP 502.
    
- HTTP 503.
    
- Broker temporariamente indisponível.
    
- Falha momentânea de conexão.
    
- Lock temporário.
    
- Serviço externo sobrecarregado.
    

Essas falhas poderão ser elegíveis para retry.

---

# Falhas Permanentes

São falhas que não serão resolvidas com nova tentativa.

Exemplos:

- Credencial inválida.
    
- Modelo inexistente.
    
- Request incompatível.
    
- Conteúdo rejeitado.
    
- Recurso não encontrado.
    
- Schema inválido.
    
- Permissão ausente.
    
- Limite financeiro excedido.
    
- Plataforma recusou definitivamente a publicação.
    

Não deverão ser repetidas automaticamente.

---

# Falhas de Integração

Ocorrem na comunicação com sistemas externos.

Exemplos:

- Provider retornou formato inesperado.
    
- OAuth expirado.
    
- Webhook inválido.
    
- Storage rejeitou upload.
    
- Serviço Python retornou erro.
    
- Broker recusou publicação.
    
- API externa alterou contrato.
    

Essas falhas deverão ser convertidas para erros internos normalizados.

---

# Falhas de Infraestrutura

Exemplos:

- PostgreSQL indisponível.
    
- Redis indisponível.
    
- Broker indisponível.
    
- Storage indisponível.
    
- Falha de DNS.
    
- Certificado inválido.
    
- Falta de espaço em disco.
    
- Worker sem memória.
    

A reação dependerá da criticidade da dependência.

---

# Falhas de Consistência

Ocorrem quando sistemas diferentes apresentam estados incompatíveis.

Exemplos:

- Conteúdo publicado externamente, mas não registrado localmente.
    
- Arquivo salvo no storage sem referência no banco.
    
- Job externo concluído sem atualização interna.
    
- Evento publicado sem atualização esperada.
    
- Pipeline marcado como Running sem Worker ativo.
    

Essas falhas exigem reconciliação.

---

# Falhas Inesperadas

Representam defeitos ou cenários não previstos.

Exemplos:

- NullReferenceException.
    
- InvalidOperationException inesperada.
    
- Erro de serialização não previsto.
    
- Bug de mapeamento.
    
- Estado impossível.
    
- Exceção de biblioteca.
    

Devem ser registradas com contexto suficiente e convertidas em resposta segura nas bordas.

---

# Result Pattern

Falhas esperadas deverão utilizar um modelo explícito de resultado.

Exemplo conceitual:

```csharp
public sealed class Result
{
    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    protected Result(
        bool isSuccess,
        Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success()
        => new(true, Error.None);

    public static Result Failure(Error error)
        => new(false, error);
}
```

---

# Result com Valor

```csharp
public sealed class Result<T>
{
    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public T? Value { get; }

    public Error Error { get; }

    private Result(
        bool isSuccess,
        T? value,
        Error error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value)
        => new(true, value, Error.None);

    public static Result<T> Failure(Error error)
        => new(false, default, error);
}
```

---

# Estrutura de Error

```csharp
public sealed record Error(
    string Code,
    string Message,
    ErrorType Type,
    bool IsTransient = false,
    IReadOnlyDictionary<string, object?>? Metadata = null)
{
    public static readonly Error None =
        new(string.Empty, string.Empty, ErrorType.None);
}
```

---

# Tipos de Erro

```csharp
public enum ErrorType
{
    None,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden,
    BusinessRule,
    RateLimit,
    Timeout,
    Concurrency,
    ExternalProvider,
    Infrastructure,
    Unavailable,
    Unexpected
}
```

---

# Códigos de Erro

Códigos deverão ser estáveis e independentes das mensagens.

Exemplos:

```text
project_not_found
pipeline_invalid_state
pipeline_cost_limit_exceeded
content_not_approved
publication_already_completed
provider_timeout
provider_rate_limit
provider_response_invalid
webhook_signature_invalid
database_concurrency_conflict
message_processing_failed
```

A mensagem poderá mudar.

O código deverá permanecer estável para consumidores, logs e métricas.

---

# Result versus Exception

## Utilizar Result

Quando a falha for esperada.

Exemplos:

- Recurso não encontrado.
    
- Estado inválido.
    
- Regra de negócio violada.
    
- Permissão negada.
    
- Limite financeiro atingido.
    
- Provider indisponível de forma controlada.
    
- Saída inválida de IA.
    
- Publicação recusada.
    

## Utilizar Exception

Quando ocorrer uma situação excepcional ou um erro de programação.

Exemplos:

- Configuração obrigatória ausente.
    
- Estado interno impossível.
    
- Falha inesperada de serialização.
    
- Invariante técnica quebrada.
    
- Dependência não registrada.
    
- Corrupção de estado.
    

---

# Exceções de Domínio

Exceções de domínio deverão ser raras.

Na maioria dos casos, regras de negócio deverão retornar `Result`.

Uma exceção poderá ser utilizada quando o código atingir um estado que o próprio domínio afirma ser impossível.

Exemplo:

```csharp
throw new DomainInvariantException(
    "An approved publication must have an approved artifact.");
```

---

# Exceções Técnicas

Exceções de bibliotecas externas não deverão escapar da Infrastructure ou Data sem normalização.

Exemplos:

```text
HttpRequestException
DbUpdateConcurrencyException
NpgsqlException
TaskCanceledException
RedisConnectionException
```

Essas exceções deverão ser convertidas para:

- Erros internos.
    
- Exceções técnicas próprias.
    
- Resultados controlados.
    

---

# Normalização de Erros

A Application não deverá depender de mensagens ou tipos específicos de fornecedores.

Fluxo:

```text
Erro externo
    ↓
Infrastructure
    ↓
Classificação
    ↓
Erro normalizado
    ↓
Application
```

Exemplo:

```text
HTTP 429 da OpenAI
    ↓
ProviderErrorType.RateLimitExceeded
    ↓
provider_rate_limit
```

---

# Provider Error

```csharp
public sealed record ProviderError(
    string Provider,
    string Operation,
    string Code,
    ProviderErrorType Type,
    string Message,
    bool IsTransient,
    string? ExternalRequestId,
    TimeSpan? RetryAfter);
```

---

# Preservação da Causa

A normalização não deverá destruir informações úteis.

Internamente, deverão ser preservados:

- Tipo da exceção original.
    
- Código externo.
    
- Status HTTP.
    
- External Request ID.
    
- Provider.
    
- Operação.
    
- Stack trace.
    
- Número da tentativa.
    

Esses dados permanecem nos logs e traces.

Não deverão ser expostos integralmente ao usuário.

---

# Tratamento nas Bordas

As principais bordas são:

- Middleware HTTP.
    
- Endpoint de webhook.
    
- Consumidor de mensagem.
    
- Worker.
    
- Executor de pipeline.
    
- Processador da Outbox.
    
- Provider.
    
- Serviço Python.
    

Cada borda deverá impedir que exceções não tratadas escapem sem registro.

---

# Middleware Global

A API deverá possuir um middleware global de tratamento de exceções.

Responsabilidades:

- Capturar exceções inesperadas.
    
- Gerar Correlation ID.
    
- Registrar contexto.
    
- Mapear erro para status HTTP.
    
- Retornar Problem Details.
    
- Não expor stack trace.
    
- Não expor segredos.
    

---

# Mapeamento HTTP

Exemplo de mapeamento:

|Tipo de erro|HTTP|
|---|--:|
|Validation|400|
|Unauthorized|401|
|Forbidden|403|
|NotFound|404|
|Conflict|409|
|BusinessRule|422|
|RateLimit|429|
|Timeout|504|
|Unavailable|503|
|Unexpected|500|

O mapeamento deverá ser consistente.

---

# Problem Details

Exemplo:

```json
{
  "type": "https://errors.infinitecontent.ai/pipeline-invalid-state",
  "title": "Pipeline cannot be started",
  "status": 409,
  "code": "pipeline_invalid_state",
  "traceId": "00-abcd...",
  "errors": []
}
```

---

# Erros em Workers

Workers não possuem resposta HTTP.

Ao ocorrer falha, o consumidor deverá decidir entre:

- Confirmar a mensagem.
    
- Repetir.
    
- Reagendar.
    
- Enviar para DLQ.
    
- Pausar o processamento.
    
- Marcar execução como falha.
    
- Solicitar intervenção.
    

Essa decisão dependerá da classificação do erro.

---

# Política de Erro por Mensagem

Exemplo:

```text
Validation
    → rejeitar e enviar para DLQ

Transient Provider Error
    → retry

Concurrency Conflict
    → recarregar e reavaliar

Already Processed
    → confirmar sem repetir

Unexpected Error
    → retry limitado e depois DLQ
```

---

# Retry

Retry representa uma nova tentativa da mesma operação.

Ele deverá ser utilizado somente quando houver possibilidade real de recuperação.

Retry indiscriminado pode:

- Aumentar custo.
    
- Duplicar ações.
    
- Sobrecarregar o provider.
    
- Aumentar latência.
    
- Agravar uma indisponibilidade.
    
- Produzir efeito cascata.
    

---

# Elegibilidade para Retry

Antes de repetir, avaliar:

- O erro é transitório?
    
- A operação é idempotente?
    
- O limite de tentativas foi atingido?
    
- O custo permite nova tentativa?
    
- O timeout total permite nova tentativa?
    
- O provider informou `Retry-After`?
    
- O circuit breaker está aberto?
    
- O cancelamento foi solicitado?
    
- A execução ainda está válida?
    

---

# Erros Elegíveis

Exemplos:

- Timeout temporário.
    
- HTTP 429.
    
- HTTP 502.
    
- HTTP 503.
    
- Falha de conexão.
    
- Lock temporário.
    
- Broker indisponível.
    
- Storage temporariamente indisponível.
    

---

# Erros Não Elegíveis

Exemplos:

- Credencial inválida.
    
- Request inválido.
    
- Permissão ausente.
    
- Schema incompatível.
    
- Conteúdo rejeitado.
    
- Recurso inexistente.
    
- Limite financeiro atingido.
    
- Operação cancelada.
    
- Estado de negócio inválido.
    

---

# Backoff Exponencial

Retry deverá utilizar backoff progressivo.

Exemplo:

```text
Tentativa 1: imediata
Tentativa 2: 2 segundos
Tentativa 3: 8 segundos
Tentativa 4: 30 segundos
Tentativa 5: 2 minutos
```

Os valores deverão ser configuráveis.

---

# Jitter

Adicionar jitter evita que muitas instâncias repitam simultaneamente.

Exemplo:

```text
Delay calculado: 8 segundos
Jitter: entre 0 e 2 segundos
Delay final: entre 8 e 10 segundos
```

---

# Retry-After

Quando o provider informar `Retry-After`, o sistema deverá considerar esse valor.

O valor ainda deverá respeitar:

- Timeout máximo.
    
- Tempo de vida da execução.
    
- Limite financeiro.
    
- Política do pipeline.
    
- Cancelamento.
    

---

# Retry Local versus Retry Assíncrono

## Retry Local

Ocorre na mesma chamada ou processo.

Adequado para:

- Falhas rápidas.
    
- Poucas tentativas.
    
- Curto intervalo.
    
- Operações de baixa duração.
    

## Retry Assíncrono

A operação é reagendada.

Adequado para:

- Intervalos longos.
    
- Rate limits extensos.
    
- Providers indisponíveis.
    
- Publicações.
    
- Renderizações.
    
- Processamento de mensagens.
    

Não manter uma thread bloqueada por longos períodos.

---

# Orçamento de Retry

Cada operação deverá possuir um orçamento de tentativas.

Exemplo:

```text
Text Generation
MaximumAttempts: 3

Image Generation
MaximumAttempts: 2

Video Rendering
MaximumAttempts: 2

Publishing
MaximumAttempts: 5
```

O número deverá considerar custo e risco de duplicidade.

---

# Timeout

Toda operação externa deverá possuir timeout.

Sem timeout, uma operação pode permanecer bloqueada indefinidamente.

Tipos de timeout:

- Timeout de conexão.
    
- Timeout da requisição.
    
- Timeout da etapa.
    
- Timeout do pipeline.
    
- Timeout do job externo.
    
- Timeout de espera por aprovação.
    
- Timeout de processamento de mensagem.
    

---

# Timeouts em Camadas

Exemplo:

```text
HTTP Client Timeout: 60 segundos
Provider Operation Timeout: 90 segundos
Pipeline Step Timeout: 2 minutos
Pipeline Timeout: 30 minutos
```

O timeout externo não deverá ultrapassar o timeout da camada superior.

---

# Timeout Total

Uma política com retry deverá considerar o tempo total.

Exemplo:

```text
Timeout por tentativa: 30 segundos
Máximo de tentativas: 3
Backoff total: 15 segundos
Timeout máximo da operação: 90 segundos
```

A soma não poderá ultrapassar o limite do caso de uso.

---

# Cancelamento

Toda operação assíncrona deverá receber `CancellationToken`.

Exemplo:

```csharp
Task<Result<AiTextResponse>> GenerateAsync(
    AiTextRequest request,
    CancellationToken cancellationToken);
```

O cancelamento deverá ser propagado entre:

- API.
    
- Application.
    
- Repository.
    
- Provider.
    
- Worker.
    
- Pipeline.
    
- Agent.
    
- Storage.
    
- Mensageria.
    

---

# Cancelamento não é Falha

Cancelamento solicitado não deverá ser registrado como erro inesperado.

Pode ser:

- Cancelamento do usuário.
    
- Encerramento da aplicação.
    
- Timeout.
    
- Cancelamento do pipeline.
    
- Deploy.
    
- Perda de lease.
    

O motivo deverá ser diferenciado quando possível.

---

# Circuit Breaker

O circuit breaker interrompe chamadas para uma dependência que está falhando repetidamente.

Estados:

```text
Closed
Open
Half-Open
```

---

# Closed

Chamadas são permitidas normalmente.

Falhas são monitoradas.

---

# Open

Chamadas são bloqueadas temporariamente.

O sistema deverá:

- Utilizar fallback.
    
- Retornar indisponibilidade controlada.
    
- Reagendar a operação.
    
- Evitar sobrecarregar a dependência.
    

---

# Half-Open

Após um período, algumas chamadas de teste são permitidas.

Se funcionarem, o circuito fecha.

Se falharem, o circuito abre novamente.

---

# Escopo do Circuit Breaker

O circuito poderá ser separado por:

- Provider.
    
- Capacidade.
    
- Modelo.
    
- Região.
    
- Endpoint.
    
- Conta.
    
- Plataforma.
    

Exemplo:

```text
OpenAI Text Generation
```

não precisa compartilhar circuito com:

```text
OpenAI Image Generation
```

---

# Bulkhead

Bulkhead limita o impacto de uma dependência ou operação.

Exemplos:

- Limite de gerações de imagem simultâneas.
    
- Limite de renderizações.
    
- Limite por organização.
    
- Pool separado por provider.
    
- Fila específica para publicação.
    

Objetivo:

> Evitar que uma operação consuma todos os recursos do sistema.

---

# Controle de Concorrência

Exemplo:

```text
TextGeneration: 20 simultâneas
ImageGeneration: 5 simultâneas
VideoRendering: 2 simultâneas
PublishingPerChannel: 1 simultânea
```

Valores deverão ser configuráveis e observáveis.

---

# Fallback

Fallback utiliza uma alternativa quando a opção principal falha.

Possíveis estratégias:

- Mesmo provider, outro modelo.
    
- Outro provider.
    
- Modelo mais barato.
    
- Modelo mais rápido.
    
- Serviço interno.
    
- Resultado em cache.
    
- Etapa degradada.
    
- Execução manual.
    
- Intervenção humana.
    

---

# Critérios de Fallback

Antes de aplicar fallback, validar:

- Capacidade equivalente.
    
- Formato compatível.
    
- Qualidade mínima.
    
- Limite de custo.
    
- Política da organização.
    
- Região.
    
- Segurança.
    
- Disponibilidade.
    
- Compatibilidade multimodal.
    
- Context window.
    
- Permissão de uso.
    

---

# Fallback e Reprodutibilidade

Trocar o provider pode alterar significativamente a saída.

A execução deverá registrar:

- Provider inicial.
    
- Provider final.
    
- Modelo inicial.
    
- Modelo final.
    
- Motivo do fallback.
    
- Tentativas.
    
- Custos.
    
- Versão da política.
    

---

# Degradação Graciosa

Nem toda falha precisa interromper o pipeline inteiro.

Possibilidades:

- Pular etapa opcional.
    
- Utilizar thumbnail padrão.
    
- Continuar sem analytics.
    
- Solicitar upload manual.
    
- Gerar saída em resolução menor.
    
- Utilizar voz alternativa.
    
- Publicar sem etapa opcional.
    
- Aguardar aprovação.
    

A decisão pertence à definição do pipeline.

---

# Etapas Obrigatórias e Opcionais

Cada step deverá indicar sua criticidade.

Exemplo:

```csharp
public enum PipelineStepCriticality
{
    Required,
    Optional,
    Recoverable,
    RequiresApproval
}
```

---

# Compensação

Compensação tenta desfazer ou neutralizar uma operação já concluída.

Exemplos:

- Remover arquivo temporário.
    
- Cancelar job externo.
    
- Excluir publicação criada incorretamente.
    
- Revogar URL temporária.
    
- Liberar reserva.
    
- Marcar artefato como órfão.
    

Compensação não é rollback distribuído.

---

# Limites da Compensação

Nem toda ação pode ser desfeita.

Exemplo:

```text
Conteúdo publicado e já visualizado
```

Mesmo removendo a publicação, o efeito externo já aconteceu.

Essas ações deverão exigir maior proteção antes da execução.

---

# Saga

Pipelines longos poderão ser tratados como uma saga.

Cada etapa possui:

- Ação.
    
- Estado.
    
- Resultado.
    
- Possível compensação.
    
- Próximo passo.
    

Exemplo:

```text
Gerar áudio
    ↓
Salvar áudio
    ↓
Renderizar vídeo
    ↓
Publicar
```

Em caso de falha, o pipeline decide:

- Repetir.
    
- Retomar.
    
- Compensar.
    
- Aguardar.
    
- Falhar.
    

---

# Idempotência

Uma operação idempotente pode ser repetida sem produzir efeitos adicionais incorretos.

Idempotência é essencial para:

- Mensageria.
    
- Webhooks.
    
- Publicação.
    
- Criação de execução.
    
- Jobs externos.
    
- Processamento de etapas.
    
- Operações de cobrança.
    

---

# Chave de Idempotência

Exemplo:

```text
organization_id
+
operation_type
+
idempotency_key
```

A chave deverá ser persistida e protegida por constraint única quando aplicável.

---

# Idempotência de Publicação

Fluxo:

```text
PublishContentCommand
    ↓
Verificar IdempotencyKey
    ↓
Publicação já existe?
    ├── Sim → retornar resultado existente
    └── Não → executar publicação
```

---

# Idempotência de Step

Uma etapa poderá utilizar:

```text
PipelineExecutionId
+
StepName
+
StepVersion
```

Se o resultado já estiver concluído e válido, não deverá ser executado novamente sem decisão explícita.

---

# Idempotência de Provider

Nem todos os providers suportam idempotency key.

Quando não suportarem, o sistema deverá:

- Registrar operação antes da chamada.
    
- Utilizar identificador externo.
    
- Consultar status.
    
- Reconhecer respostas duplicadas.
    
- Evitar retry cego após resposta incerta.
    

---

# Resultado Incerto

Um cenário perigoso ocorre quando:

```text
Request enviada
    ↓
Provider processa
    ↓
Resposta se perde
```

O sistema não sabe se a operação foi concluída.

Nesses casos, antes de repetir:

- Consultar status.
    
- Utilizar idempotency key.
    
- Buscar pelo identificador externo.
    
- Executar reconciliação.
    
- Solicitar intervenção.
    

---

# Inbox Pattern

A Inbox impede processamento duplicado de mensagens recebidas.

Fluxo:

```text
Mensagem recebida
    ↓
MessageId já existe?
    ├── Sim → confirmar sem reprocessar
    └── Não → registrar Inbox
              ↓
              processar
              ↓
              marcar como concluída
```

---

# Outbox Pattern

A Outbox garante que alterações locais e eventos sejam persistidos na mesma transação.

Fluxo:

```text
Alterar estado
    ↓
Criar OutboxMessage
    ↓
Commit
    ↓
Publicar posteriormente
```

---

# Entrega pelo Menos Uma Vez

A maioria dos brokers trabalha com possibilidade de reentrega.

Isso significa:

```text
At-least-once delivery
```

O sistema deverá assumir mensagens duplicadas.

Consumidores deverão ser idempotentes.

---

# Dead Letter Queue

Mensagens que não podem ser processadas após as tentativas deverão ir para DLQ.

A DLQ não é lixeira.

Ela representa falhas que exigem análise.

---

# Motivos para DLQ

- Payload inválido.
    
- Versão não suportada.
    
- Falha permanente.
    
- Máximo de tentativas.
    
- Recurso inconsistente.
    
- Erro inesperado recorrente.
    
- Dependência indisponível por período excessivo.
    

---

# Dados da DLQ

Registrar:

- MessageId.
    
- MessageType.
    
- Payload seguro.
    
- Version.
    
- Consumer.
    
- AttemptCount.
    
- LastError.
    
- ErrorCode.
    
- CorrelationId.
    
- CausationId.
    
- FailedAt.
    
- OriginalQueue.
    

---

# Reprocessamento da DLQ

O reprocessamento deverá ser controlado.

Antes de reprocessar:

1. Identificar a causa.
    
2. Corrigir configuração ou código.
    
3. Validar compatibilidade.
    
4. Garantir idempotência.
    
5. Selecionar mensagens.
    
6. Registrar responsável.
    
7. Reenviar.
    
8. Monitorar resultado.
    

Nunca reprocessar toda a DLQ cegamente.

---

# Poison Message

Uma mensagem que falha sempre é uma poison message.

Ela deverá ser isolada para não bloquear a fila.

Estratégias:

- Limite de tentativas.
    
- DLQ.
    
- Parking queue.
    
- Alerta.
    
- Ferramenta administrativa.
    

---

# Parking Queue

Uma parking queue poderá armazenar mensagens que aguardam uma condição externa.

Exemplos:

- Credencial precisa ser renovada.
    
- Organização precisa corrigir configuração.
    
- Provider está indisponível por longo período.
    
- Aprovação humana pendente.
    

Isso evita retentativas inúteis.

---

# Checkpoints de Pipeline

Pipelines deverão persistir checkpoints após etapas relevantes.

Um checkpoint poderá conter:

- Step atual.
    
- Status.
    
- Input.
    
- Output.
    
- Artefatos.
    
- Tentativas.
    
- Custos.
    
- Provider utilizado.
    
- Modelo.
    
- Próxima etapa.
    
- Data.
    
- Versão.
    

---

# Frequência de Checkpoint

Checkpoint deverá ocorrer:

- Antes de operação crítica.
    
- Depois de operação externa.
    
- Depois de gerar artefato.
    
- Antes de aguardar aprovação.
    
- Depois de fallback.
    
- Antes de publicação.
    
- Após conclusão de step.
    

---

# Retomada de Pipeline

Ao retomar uma execução:

1. Carregar estado persistido.
    
2. Validar versão do pipeline.
    
3. Identificar última etapa concluída.
    
4. Verificar artefatos.
    
5. Verificar operações externas.
    
6. Confirmar idempotência.
    
7. Recalcular próxima etapa.
    
8. Adquirir controle da execução.
    
9. Continuar.
    

---

# Estados da Execução

Exemplo:

```text
Pending
Running
Waiting
WaitingApproval
RetryScheduled
Paused
Completed
Failed
Cancelled
Compensating
Compensated
```

Transições deverão ser explícitas.

---

# Estados da Etapa

Exemplo:

```text
Pending
Running
Succeeded
Failed
Skipped
RetryScheduled
WaitingApproval
Cancelled
Compensated
```

---

# Recuperação após Reinício

O Worker poderá ser interrompido durante uma etapa.

Ao reiniciar, o sistema deverá identificar execuções abandonadas.

Sinais:

- Status Running.
    
- Heartbeat antigo.
    
- Lease expirado.
    
- Sem atividade recente.
    
- Mensagem não confirmada.
    

A execução poderá ser:

- Retomada.
    
- Reagendada.
    
- Marcada para reconciliação.
    
- Enviada para intervenção.
    

---

# Lease de Execução

Um lease poderá impedir múltiplos Workers de processar a mesma execução.

Campos possíveis:

- LockedBy.
    
- LockToken.
    
- LockedUntil.
    
- LastHeartbeatAt.
    

O lease deverá expirar automaticamente.

---

# Heartbeat

Etapas longas deverão atualizar heartbeat.

Exemplos:

- Renderização.
    
- Processamento de vídeo.
    
- Job externo.
    
- Transcrição longa.
    

Ausência de heartbeat poderá indicar abandono.

---

# Reconciliação

Jobs de reconciliação deverão procurar inconsistências.

Exemplos:

- Execução Running sem lease.
    
- Job externo concluído sem evento.
    
- Publicação externa existente sem registro local.
    
- Arquivo órfão.
    
- Outbox parada.
    
- Inbox incompleta.
    
- Step concluído sem artefato.
    
- Aprovação pendente expirada.
    

---

# Estratégias de Reconciliação

Uma reconciliação poderá:

- Consultar estado externo.
    
- Corrigir estado local.
    
- Reemitir evento.
    
- Reagendar step.
    
- Marcar falha.
    
- Criar tarefa manual.
    
- Executar compensação.
    
- Registrar incidente.
    

---

# Erros em IA

Chamadas de IA possuem falhas específicas.

Exemplos:

- JSON inválido.
    
- Resposta vazia.
    
- Conteúdo truncado.
    
- Recusa.
    
- Alucinação detectada.
    
- Schema incompatível.
    
- Token limit.
    
- Conteúdo inseguro.
    
- Resposta fora do idioma.
    
- Ferramenta chamada incorretamente.
    

---

# Validação de Saída

Toda saída de IA relevante deverá ser validada.

Fluxo:

```text
Resposta
    ↓
Validação estrutural
    ↓
Validação semântica
    ↓
Validação de negócio
    ↓
Resultado aceito ou rejeitado
```

---

# Estratégias para Saída Inválida

- Corrigir com parser.
    
- Solicitar regeneração.
    
- Utilizar prompt de reparo.
    
- Trocar modelo.
    
- Trocar provider.
    
- Reduzir complexidade.
    
- Solicitar aprovação humana.
    
- Falhar controladamente.
    

---

# Retry de IA

Retry de IA exige cuidado.

Uma nova tentativa pode:

- Aumentar custo.
    
- Produzir saída diferente.
    
- Alterar decisões editoriais.
    
- Dificultar reprodutibilidade.
    

A execução deverá registrar cada tentativa.

---

# Erros em Publicação

Publicação é uma operação crítica e potencialmente irreversível.

Possíveis falhas:

- Upload incompleto.
    
- Metadata inválida.
    
- Token expirado.
    
- Rate limit.
    
- Conteúdo rejeitado.
    
- Processamento externo pendente.
    
- Resposta perdida.
    
- Publicação criada sem retorno local.
    

---

# Estratégia de Publicação

Antes da publicação:

- Validar aprovação.
    
- Gerar idempotency key.
    
- Persistir intenção.
    
- Validar credencial.
    
- Registrar tentativa.
    
- Executar fora da transação.
    
- Persistir resultado.
    
- Reconciliar estado incerto.
    

---

# Estado Incerto de Publicação

Exemplo:

```text
Upload concluído
    ↓
Timeout antes da resposta
```

Não repetir imediatamente.

Primeiro:

- Consultar plataforma.
    
- Buscar por identificador.
    
- Consultar job.
    
- Verificar idempotency key.
    
- Marcar como ReconciliationRequired.
    

---

# Falhas de Storage

Possíveis cenários:

- Upload falhou.
    
- Arquivo foi salvo, mas banco falhou.
    
- Banco salvou referência, mas arquivo não existe.
    
- URL temporária expirou.
    
- Arquivo foi removido externamente.
    
- Conteúdo corrompido.
    

---

# Compensação de Storage

Possíveis ações:

- Excluir arquivo órfão.
    
- Repetir upload.
    
- Marcar artefato como indisponível.
    
- Restaurar backup.
    
- Gerar nova URL.
    
- Executar job de limpeza.
    

---

# Erros de Banco

Falhas de banco deverão ser classificadas.

Exemplos:

- Timeout.
    
- Deadlock.
    
- Constraint violation.
    
- Concurrency conflict.
    
- Connection failure.
    
- Migration incompatível.
    

---

# Constraint Violation

Constraints esperadas poderão ser convertidas para erros específicos.

Exemplo:

```text
unique_violation
    ↓
publication_already_exists
```

Não expor nomes internos de constraints ao cliente.

---

# Deadlocks

Deadlocks poderão ser transitórios.

A operação poderá ser repetida quando:

- For idempotente.
    
- A transação for curta.
    
- O limite de tentativas não tiver sido atingido.
    

Deadlocks frequentes exigem correção arquitetural.

---

# Concurrency Conflict

Conflitos de concorrência não devem ser repetidos cegamente.

Fluxo:

1. Recarregar estado.
    
2. Verificar se a operação ainda faz sentido.
    
3. Retornar sucesso idempotente, conflito ou repetir.
    
4. Registrar a decisão.
    

---

# Resiliência em HTTP

Clientes HTTP deverão utilizar:

- `IHttpClientFactory`.
    
- Timeout.
    
- Retry seletivo.
    
- Circuit breaker.
    
- Logging.
    
- Tracing.
    
- Propagação de cancelamento.
    
- Configuração por provider.
    

---

# Política por Integração

Não deverá existir uma única política genérica para todas as integrações.

Exemplo:

```text
Webhook externo
    Timeout: 10s
    Retry: 3

Text Provider
    Timeout: 90s
    Retry: 2

Video Renderer
    Timeout: assíncrono
    Retry: job-level

Publishing Provider
    Timeout: 120s
    Retry: somente com idempotência
```

---

# Polly ou Resilience Pipeline

A implementação .NET poderá utilizar mecanismos de resiliência compatíveis com a stack adotada.

Exemplo conceitual:

```csharp
services.AddHttpClient<OpenAiClient>()
    .AddStandardResilienceHandler();
```

As políticas deverão ser customizadas por operação.

A biblioteca é detalhe técnico da Infrastructure.

---

# Resiliência de Banco

Retentativas de conexão poderão ser usadas para falhas transitórias.

Não repetir automaticamente transações complexas sem garantir idempotência.

A política deverá distinguir:

- Falha antes do commit.
    
- Falha durante o commit.
    
- Resultado do commit desconhecido.
    

---

# Resultado de Commit Incerto

Se a conexão cair durante o commit, o sistema pode não saber se a transação foi confirmada.

Estratégias:

- Chave de idempotência.
    
- Verificação posterior.
    
- Constraint única.
    
- Recarregar estado.
    
- Reconciliação.
    

---

# Resiliência do Broker

Quando o broker estiver indisponível:

- Persistir Outbox.
    
- Não perder evento.
    
- Repetir publicação posteriormente.
    
- Monitorar idade da Outbox.
    
- Alertar quando o atraso ultrapassar limite.
    

---

# Resiliência da Telemetria

Falhas no backend de observabilidade não deverão interromper o produto.

A exportação deverá ser:

- Assíncrona.
    
- Bufferizada.
    
- Limitada.
    
- Descartável para sinais de baixa prioridade.
    

---

# Resiliência em Cache

Cache é otimização.

Quando indisponível:

- Buscar na fonte oficial.
    
- Evitar derrubar o caso de uso, quando possível.
    
- Registrar degradação.
    
- Aplicar circuit breaker.
    
- Limitar carga adicional no banco.
    

---

# Resiliência de Dependências Opcionais

Dependências opcionais não deverão tornar todo o sistema indisponível.

Exemplos:

- Analytics.
    
- Thumbnail alternativo.
    
- n8n.
    
- Cache.
    
- Provider secundário.
    

A aplicação deverá degradar de forma controlada.

---

# Falha Segura

Em caso de dúvida, o sistema deverá preferir a opção mais segura.

Exemplos:

- Não publicar conteúdo sem confirmação.
    
- Não repetir cobrança sem idempotência.
    
- Não conceder acesso em erro de autorização.
    
- Não assumir que webhook é válido.
    
- Não marcar pipeline como concluído sem estado persistido.
    
- Não remover artefato aprovado automaticamente.
    

---

# Observabilidade de Resiliência

Registrar e medir:

- Retries.
    
- Fallbacks.
    
- Timeouts.
    
- Circuit breaker aberto.
    
- Operações degradadas.
    
- Compensações.
    
- DLQ.
    
- Reprocessamentos.
    
- Conflitos de concorrência.
    
- Reconciliações.
    
- Jobs abandonados.
    
- Custos adicionais por retry.
    

---

# Métricas

Exemplos:

```text
operation_retries_total
operation_timeouts_total
circuit_breaker_open_total
fallback_executions_total
compensation_executions_total
dead_letter_messages_total
pipeline_recoveries_total
pipeline_abandoned_total
reconciliation_actions_total
idempotency_duplicates_total
```

---

# Logs

Um retry deverá registrar:

- Operação.
    
- Tentativa.
    
- Limite.
    
- Motivo.
    
- Delay.
    
- Provider.
    
- CorrelationId.
    
- Custo acumulado.
    

Exemplo:

```text
Provider request will be retried
Provider: OpenAI
Operation: GenerateScript
Attempt: 2
MaximumAttempts: 3
DelayMs: 8000
ErrorCode: provider_timeout
```

---

# Alertas

Alertas possíveis:

- Circuit breaker aberto por muito tempo.
    
- Fallback excessivo.
    
- DLQ crescendo.
    
- Muitas execuções abandonadas.
    
- Retentativas elevando custo.
    
- Reconciliação falhando.
    
- Outbox atrasada.
    
- Muitos conflitos de concorrência.
    
- Provider em degradação.
    
- Steps excedendo timeout.
    
- Compensações falhando.
    

---

# Testes Unitários

Devem validar:

- Result Pattern.
    
- Classificação de erros.
    
- Mapeamento de códigos.
    
- Elegibilidade de retry.
    
- Seleção de fallback.
    
- Transições de estado.
    
- Idempotência.
    
- Políticas de compensação.
    
- Regras de cancelamento.
    
- Limite de tentativas.
    

---

# Testes de Integração

Devem validar:

- Timeout real.
    
- Retry.
    
- Circuit breaker.
    
- Broker indisponível.
    
- Outbox.
    
- Inbox.
    
- DLQ.
    
- Concorrência.
    
- Constraints.
    
- Recuperação de Worker.
    
- Falha parcial de storage.
    
- Resposta inválida de provider.
    

---

# Testes de Falha

O sistema deverá possuir testes intencionais de falha.

Exemplos:

- Provider retorna 429.
    
- Provider retorna 503.
    
- Resposta de IA inválida.
    
- Banco cai antes do commit.
    
- Broker fica indisponível.
    
- Worker é interrompido.
    
- Webhook é duplicado.
    
- Job externo conclui sem callback.
    
- Upload salva, mas persistência falha.
    
- Publicação retorna timeout após sucesso externo.
    

---

# Chaos Testing

Em fases futuras, poderão ser introduzidas falhas controladas.

Exemplos:

- Latência artificial.
    
- Interrupção de Worker.
    
- Falha de DNS.
    
- Erros de storage.
    
- Perda temporária de broker.
    
- Provider lento.
    
- Redis indisponível.
    

O objetivo será validar:

- Recuperação.
    
- Alertas.
    
- Checkpoints.
    
- Idempotência.
    
- Fallback.
    
- Consistência.
    

---

# Checklist para Nova Operação Externa

Toda nova operação deverá responder:

- Possui timeout?
    
- Pode ser repetida?
    
- É idempotente?
    
- Qual erro é transitório?
    
- Qual erro é permanente?
    
- Qual o número máximo de tentativas?
    
- Existe fallback?
    
- Existe circuit breaker?
    
- Existe custo por tentativa?
    
- O resultado pode ficar incerto?
    
- Existe forma de consultar status?
    
- Existe compensação?
    
- Como será observada?
    
- Como será reconciliada?
    

---

# Checklist para Novo Step

Todo novo step deverá definir:

- Timeout.
    
- Máximo de tentativas.
    
- Erros elegíveis para retry.
    
- Política de fallback.
    
- Criticidade.
    
- Idempotency key.
    
- Checkpoint.
    
- Compensação.
    
- Condição de retomada.
    
- Estado de falha.
    
- Custo máximo.
    
- Métricas.
    
- Logs.
    
- Alertas.
    

---

# Checklist para Novo Consumer

Todo consumer deverá definir:

- MessageId.
    
- Idempotência.
    
- Inbox.
    
- Máximo de tentativas.
    
- Retry delay.
    
- DLQ.
    
- Tratamento de poison message.
    
- Error codes.
    
- CorrelationId.
    
- Timeout.
    
- Cancelamento.
    
- Critério de confirmação.
    
- Estratégia de reprocessamento.
    

---

# Checklist para Nova Publicação

Toda publicação deverá definir:

- Idempotency key.
    
- Estado anterior.
    
- Aprovação necessária.
    
- Timeout.
    
- Retry seguro.
    
- Consulta de status.
    
- Reconciliação.
    
- Compensação possível.
    
- Tratamento de resposta perdida.
    
- Auditoria.
    
- Limite financeiro.
    
- Operação irreversível.
    

---

# Regras Arquiteturais

- Falhas esperadas devem utilizar Result.
    
- Exceções externas devem ser normalizadas.
    
- Exceções inesperadas devem ser tratadas nas bordas.
    
- Toda operação externa deve possuir timeout.
    
- Retry só deve ocorrer para falhas transitórias.
    
- Retry deve ser limitado.
    
- Retry deve utilizar backoff e jitter.
    
- Operações não idempotentes não devem ser repetidas cegamente.
    
- Mensagens devem ser consideradas potencialmente duplicadas.
    
- Consumidores devem ser idempotentes.
    
- Mensagens irrecuperáveis devem ir para DLQ.
    
- DLQ deve possuir processo de reprocessamento.
    
- Pipelines devem possuir checkpoints.
    
- Pipelines devem suportar retomada.
    
- Etapas longas devem possuir heartbeat ou status externo.
    
- Workers devem utilizar lease ou proteção equivalente.
    
- Fallback deve respeitar custo, qualidade e segurança.
    
- Ações críticas devem possuir estratégia para resultado incerto.
    
- Compensação não deve ser tratada como rollback distribuído.
    
- Transações não devem envolver chamadas externas.
    
- Outbox deve proteger a publicação de eventos.
    
- Inbox deve proteger o consumo duplicado.
    
- Cancelamento deve ser propagado.
    
- Falhas de telemetria não devem derrubar a aplicação.
    
- Dependências opcionais devem permitir degradação graciosa.
    
- Erros não devem expor detalhes internos.
    
- Toda política de resiliência deve ser observável.
    
- Custos adicionais de retry e fallback devem ser registrados.
    

---

# Decisões Pendentes

As seguintes decisões deverão ser registradas em ADRs:

- Estrutura final do Result Pattern.
    
- Biblioteca de resiliência.
    
- Políticas de retry por provider.
    
- Timeouts padrão.
    
- Estratégia de circuit breaker.
    
- Estratégia de bulkhead.
    
- Política de fallback.
    
- Modelo de idempotência.
    
- Estrutura da Inbox.
    
- Tecnologia e política da DLQ.
    
- Estratégia de parking queue.
    
- Modelo de lease de pipelines.
    
- Frequência de heartbeat.
    
- Estratégia de reconciliação.
    
- Política de compensação.
    
- Tratamento de publicações com estado incerto.
    
- Política de retry financeiro.
    
- Processo de reprocessamento da DLQ.
    
- Política de cancelamento.
    
- Estratégia de chaos testing.
    

---

# Exemplo Completo

```text
Worker recebe StartPipelineCommand
    ↓
Inbox verifica MessageId
    ↓
Mensagem ainda não processada
    ↓
Worker adquire lease da execução
    ↓
Pipeline carrega último checkpoint
    ↓
Script Step é iniciado
    ↓
OpenAI retorna timeout
    ↓
Erro é normalizado como transitório
    ↓
Retry é permitido
    ↓
Backoff com jitter
    ↓
Segunda tentativa retorna HTTP 503
    ↓
Circuit breaker registra nova falha
    ↓
Política seleciona fallback
    ↓
Anthropic é utilizado
    ↓
Saída retorna JSON inválido
    ↓
Validação falha
    ↓
Prompt de reparo é executado
    ↓
Saída válida é produzida
    ↓
Custo total é registrado
    ↓
Artefato é salvo
    ↓
Checkpoint é persistido
    ↓
Outbox registra ScriptGenerated
    ↓
Lease é renovado
    ↓
Pipeline continua
```

Cenário de publicação incerta:

```text
Publishing Step inicia
    ↓
Intenção de publicação é persistida
    ↓
IdempotencyKey é criada
    ↓
Provider recebe a publicação
    ↓
A conexão cai antes da resposta
    ↓
Resultado é classificado como incerto
    ↓
Retry automático é bloqueado
    ↓
Execução entra em ReconciliationRequired
    ↓
Job consulta a plataforma
    ↓
Publicação é encontrada
    ↓
Estado local é atualizado
    ↓
Evento ContentPublished é salvo na Outbox
    ↓
Pipeline é concluído sem duplicidade
```

Cenário de Worker interrompido:

```text
Worker inicia renderização
    ↓
Pipeline salva checkpoint
    ↓
Job externo é criado
    ↓
Worker é interrompido
    ↓
Lease expira
    ↓
Job de reconciliação detecta execução abandonada
    ↓
Consulta status do job externo
    ↓
Job está concluído
    ↓
Resultado é recuperado
    ↓
Artefato é persistido
    ↓
Pipeline é retomado na próxima etapa
```

---

# Objetivo Final

Criar um sistema capaz de falhar sem perder controle do estado.

O Infinite Content AI deverá reconhecer falhas esperadas, limitar falhas transitórias, impedir duplicidades, proteger operações críticas e recuperar pipelines interrompidos.

Falhas deverão ser classificáveis.

Retentativas deverão ser seguras.

Operações externas deverão ser rastreáveis.

Estados incertos deverão ser reconciliáveis.

Pipelines deverão ser retomáveis.

O sistema deverá continuar confiável mesmo quando suas dependências não forem.