# Infrastructure

## 1. Objetivo

O projeto `Infrastructure` será responsável pela implementação dos adapters e integrações técnicas externas utilizados pelo Infinite Content AI.

Ele conectará as abstrações definidas pela Application a tecnologias e serviços concretos, como:

- Providers de inteligência artificial.
    
- RabbitMQ.
    
- Redis.
    
- Azure Blob Storage.
    
- APIs externas.
    
- n8n.
    
- Webhooks.
    
- Serviços auxiliares em Python.
    
- Serviços de publicação.
    
- Gerenciamento de secrets.
    
- Clientes HTTP.
    
- Resiliência de rede.
    
- Observabilidade.
    

Fluxo conceitual:

```text
Application
    ↓ abstrações
Infrastructure
    ├── OpenAI
    ├── Anthropic
    ├── Gemini
    ├── RabbitMQ
    ├── Redis
    ├── Azure
    ├── n8n
    ├── APIs externas
    └── Observabilidade
```

A Infrastructure deverá conhecer as interfaces da Application e fornecer implementações concretas.

A Application não deverá conhecer as tecnologias utilizadas por essas implementações.

---

# 2. Responsabilidades

O projeto `Infrastructure` será responsável por:

- Providers de inteligência artificial.
    
- Seleção e resolução de providers.
    
- Integração com RabbitMQ.
    
- Publicação de mensagens.
    
- Serialização de mensagens.
    
- Configuração de exchanges, queues e bindings.
    
- Integração com Redis.
    
- Cache distribuído.
    
- Locks distribuídos.
    
- Rate limiting distribuído quando necessário.
    
- Integração com Azure Blob Storage.
    
- Upload e download de arquivos.
    
- Geração de URLs temporárias.
    
- Integração com Azure Key Vault.
    
- Integração com n8n.
    
- Clientes HTTP externos.
    
- Integrações com redes sociais.
    
- Integração com serviços auxiliares em Python.
    
- Validação técnica de Structured Outputs.
    
- Resiliência de rede.
    
- Timeouts.
    
- Retry técnico.
    
- Circuit breakers.
    
- Telemetria de integrações.
    
- Health checks técnicos.
    
- Resolução de credenciais.
    
- Assinatura e validação de webhooks.
    
- Adapters para recursos externos.
    

O projeto não será responsável por:

- DbContext.
    
- Entity Framework Core.
    
- Migrations.
    
- Repositories de PostgreSQL.
    
- Queries relacionais.
    
- Transações de banco.
    
- Outbox persistida.
    
- Inbox persistida.
    
- Regras centrais de negócio.
    
- Commands e Queries.
    
- Endpoints.
    
- Consumers do host Worker.
    
- Background Services.
    
- Máquinas de estado do Domain.
    

---

# 3. Regra Absoluta: Infrastructure não possui DbContext

O projeto Infrastructure não deverá possuir:

```text
ApplicationDbContext
DbContext
DbSet
IEntityTypeConfiguration
Migrations
Repository de PostgreSQL
Transaction de banco
```

Exemplo proibido:

```csharp
public sealed class OpenAiProvider
{
    private readonly ApplicationDbContext _dbContext;
}
```

Exemplo correto:

```csharp
public sealed class OpenAiTextGenerationProvider
    : ITextGenerationProvider
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<OpenAiOptions> _options;
}
```

Quando uma integração precisar registrar dados no banco, a coordenação deverá ocorrer na Application.

Fluxo correto:

```text
Application Handler
    ├── Chama Infrastructure
    ├── Recebe resultado
    ├── Atualiza Domain
    └── Persiste por abstrações de Data
```

A Infrastructure não deverá coordenar persistência relacional.

---

# 4. Dependências Permitidas

O projeto Infrastructure poderá depender de:

```text
Application
Domain
Contracts
SharedKernel
```

Não poderá depender de:

```text
Api
Data
Worker
```

Fluxo:

```text
Api ───────────────┐
Worker ────────────┼──> Infrastructure
                   │          ↓
                   └──> Application
```

Infrastructure implementa abstrações da Application.

Api e Worker registram essas implementações nos hosts.

---

# 5. Estrutura do Projeto

```text
Infrastructure
│
├── ArtificialIntelligence
│   ├── Common
│   ├── OpenAI
│   ├── Anthropic
│   ├── Google
│   ├── Fake
│   ├── Prompts
│   ├── StructuredOutputs
│   └── Usage
│
├── Messaging
│   ├── RabbitMQ
│   ├── Publishing
│   ├── Serialization
│   ├── Topology
│   ├── Headers
│   └── HealthChecks
│
├── Caching
│   ├── Redis
│   ├── Serialization
│   ├── Locks
│   └── HealthChecks
│
├── Storage
│   ├── AzureBlob
│   ├── Validation
│   ├── Checksums
│   └── HealthChecks
│
├── Automation
│   └── N8n
│
├── Integrations
│   ├── YouTube
│   ├── WordPress
│   ├── LinkedIn
│   └── Common
│
├── Python
│   ├── Client
│   ├── Contracts
│   └── HealthChecks
│
├── Webhooks
│   ├── Signing
│   ├── Validation
│   └── ReplayProtection
│
├── Security
│   ├── Secrets
│   ├── Credentials
│   └── KeyVault
│
├── Resilience
│   ├── Policies
│   ├── Timeouts
│   ├── Retry
│   └── CircuitBreakers
│
├── Observability
│   ├── Activities
│   ├── Metrics
│   ├── Logging
│   └── Redaction
│
├── Http
│   ├── DelegatingHandlers
│   ├── ClientFactories
│   └── Diagnostics
│
├── DependencyInjection.cs
└── InfrastructureOptions.cs
```

---

# 6. Organização por Capacidade Externa

Infrastructure será organizada principalmente por capacidade técnica.

Exemplos:

```text
ArtificialIntelligence
Messaging
Caching
Storage
Automation
Integrations
```

Ela não deverá ser organizada somente por features de negócio.

Um mesmo provider poderá atender:

- Research.
    
- Script.
    
- Review.
    
- Title Generation.
    
- Description Generation.
    
- Summarization.
    

Por isso, o adapter deverá permanecer agrupado pela tecnologia externa.

---

# 7. Dependency Injection

O projeto deverá expor um único ponto principal de registro:

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddArtificialIntelligence(configuration)
            .AddMessaging(configuration)
            .AddCaching(configuration)
            .AddStorage(configuration)
            .AddAutomation(configuration)
            .AddExternalIntegrations(configuration)
            .AddInfrastructureObservability();

        return services;
    }
}
```

Cada capacidade deverá possuir sua própria extensão.

Evitar um método com centenas de linhas e configurações misturadas.

---

# 8. Options Pattern

Toda integração deverá utilizar opções tipadas.

Exemplos:

```text
OpenAiOptions
AnthropicOptions
GeminiOptions
RabbitMqOptions
RedisOptions
AzureBlobStorageOptions
N8nOptions
PythonServiceOptions
YouTubeOptions
```

Exemplo:

```csharp
public sealed class OpenAiOptions
{
    public const string SectionName = "ArtificialIntelligence:OpenAI";

    public required string BaseUrl { get; init; }

    public required string ApiKeySecretName { get; init; }

    public required string DefaultModel { get; init; }

    public int TimeoutSeconds { get; init; } = 60;

    public int MaximumRetryCount { get; init; } = 2;
}
```

Secrets não deverão ser armazenados diretamente nos arquivos de configuração.

---

# 9. Validação de Configuração

Configurações obrigatórias deverão ser validadas no startup.

Exemplo:

```csharp
services
    .AddOptions<OpenAiOptions>()
    .BindConfiguration(OpenAiOptions.SectionName)
    .Validate(
        options => Uri.TryCreate(
            options.BaseUrl,
            UriKind.Absolute,
            out _),
        "A URL do OpenAI é inválida.")
    .Validate(
        options =>
            !string.IsNullOrWhiteSpace(
                options.ApiKeySecretName),
        "O secret da API é obrigatório.")
    .ValidateOnStart();
```

O host deverá falhar rapidamente quando uma dependência obrigatória estiver configurada incorretamente.

Providers opcionais poderão ser desabilitados explicitamente.

---

# 10. Providers de Inteligência Artificial

A Application definirá abstrações como:

```csharp
public interface ITextGenerationProvider
{
    string ProviderName { get; }

    Task<Result<TextGenerationResponse>> GenerateAsync(
        TextGenerationRequest request,
        CancellationToken cancellationToken);
}
```

A Infrastructure fornecerá adapters concretos:

```text
OpenAiTextGenerationProvider
AnthropicTextGenerationProvider
GeminiTextGenerationProvider
FakeTextGenerationProvider
```

Cada adapter será responsável por:

- Converter request interno para o SDK ou HTTP externo.
    
- Executar a chamada.
    
- Aplicar timeout técnico.
    
- Classificar falhas.
    
- Converter resposta externa para modelo interno.
    
- Registrar métricas.
    
- Redigir dados sensíveis.
    
- Propagar CancellationToken.
    
- Não vazar tipos do SDK.
    

---

# 11. Isolamento dos SDKs

Tipos de SDK não deverão sair da pasta do provider.

Exemplo proibido na Application:

```csharp
ChatCompletionOptions options;
```

Exemplo correto:

```csharp
TextGenerationRequest request;
```

Adapter:

```csharp
public sealed class OpenAiTextGenerationProvider
    : ITextGenerationProvider
{
    public async Task<Result<TextGenerationResponse>> GenerateAsync(
        TextGenerationRequest request,
        CancellationToken cancellationToken)
    {
        var providerRequest = MapRequest(request);

        var providerResponse =
            await _client.GenerateAsync(
                providerRequest,
                cancellationToken);

        return MapResponse(providerResponse);
    }
}
```

---

# 12. Estrutura de um Provider

```text
Infrastructure
└── ArtificialIntelligence
    └── OpenAI
        ├── OpenAiOptions.cs
        ├── OpenAiClientFactory.cs
        ├── OpenAiTextGenerationProvider.cs
        ├── OpenAiRequestMapper.cs
        ├── OpenAiResponseMapper.cs
        ├── OpenAiErrorMapper.cs
        ├── OpenAiUsageMapper.cs
        └── OpenAiHealthCheck.cs
```

Responsabilidades deverão permanecer separadas.

---

# 13. Resolução de Providers

A Application poderá utilizar:

```csharp
public interface ITextGenerationProviderResolver
{
    Result<ITextGenerationProvider> Resolve(
        ProviderModel providerModel);
}
```

Implementação:

```csharp
public sealed class TextGenerationProviderResolver
    : ITextGenerationProviderResolver
{
    private readonly IReadOnlyDictionary<
        string,
        ITextGenerationProvider> _providers;

    public Result<ITextGenerationProvider> Resolve(
        ProviderModel providerModel)
    {
        if (!_providers.TryGetValue(
                providerModel.Provider,
                out var provider))
        {
            return Result.Failure<ITextGenerationProvider>(
                ArtificialIntelligenceErrors.ProviderNotConfigured(
                    providerModel.Provider));
        }

        return Result.Success(provider);
    }
}
```

A resolução não deverá depender de `switch` espalhado pelos Handlers.

---

# 14. Provider Fake

Um provider fake deverá existir desde o início.

Objetivos:

- Desenvolvimento local.
    
- Testes.
    
- Pipelines determinísticos.
    
- Simulação de erros.
    
- Simulação de timeout.
    
- Simulação de Structured Output inválido.
    
- Redução de custo.
    

Estrutura:

```text
ArtificialIntelligence
└── Fake
    ├── FakeTextGenerationProvider.cs
    ├── FakeProviderOptions.cs
    └── FakeResponseFactory.cs
```

O Fake Provider deverá ser explicitamente configurado.

Não deverá ser habilitado acidentalmente em produção.

---

# 15. Requests para IA

A Infrastructure receberá modelos próprios da Application.

Exemplo:

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

O adapter deverá validar limitações técnicas específicas do provider.

Exemplos:

- Modelo não suportado.
    
- Limite de tokens.
    
- Temperature fora do intervalo.
    
- Structured Output indisponível.
    
- Tamanho máximo.
    
- Região não suportada.
    

Falhas deverão ser retornadas com códigos estáveis.

---

# 16. Responses de IA

Resposta interna:

```csharp
public sealed record TextGenerationResponse(
    string Content,
    string Provider,
    string Model,
    TokenUsage Usage,
    string? FinishReason,
    string? ProviderRequestId);
```

O adapter deverá extrair:

- Conteúdo.
    
- Tokens de entrada.
    
- Tokens de saída.
    
- Provider.
    
- Modelo real utilizado.
    
- Finish reason.
    
- Request ID.
    
- Metadados técnicos relevantes.
    

O conteúdo bruto completo não deverá ser registrado automaticamente em logs.

---

# 17. Classificação de Erros de IA

Erros deverão ser classificados.

Exemplos:

```text
AI.AuthenticationFailed
AI.ProviderUnavailable
AI.RateLimitExceeded
AI.InvalidRequest
AI.ContentFiltered
AI.ContextLengthExceeded
AI.Timeout
AI.InvalidResponse
AI.StructuredOutputInvalid
AI.ModelNotFound
AI.UnknownFailure
```

Cada erro deverá indicar características úteis:

```csharp
public sealed record ProviderFailure(
    Error Error,
    bool IsTransient,
    TimeSpan? RetryAfter,
    string? ProviderRequestId);
```

O Worker e a Application utilizarão essa classificação para decidir retry.

---

# 18. Rate Limit de Provider

Quando o provider retornar rate limit:

- Capturar `Retry-After`, quando disponível.
    
- Classificar como falha transitória.
    
- Registrar métrica.
    
- Não registrar API key.
    
- Não realizar retries agressivos.
    
- Respeitar limites por Organization quando necessário.
    

A política técnica não deverá criar tempestade de retries.

---

# 19. Structured Outputs

A Infrastructure poderá implementar:

```csharp
public interface IStructuredOutputValidator
{
    Result Validate(
        string content,
        StructuredOutputSchema schema);
}
```

Responsabilidades:

- Parsear JSON.
    
- Validar schema.
    
- Limitar profundidade.
    
- Limitar tamanho.
    
- Proteger contra payloads excessivos.
    
- Retornar erros estruturados.
    
- Não interpretar regras de negócio.
    

A validação semântica continuará na Application ou Domain.

---

# 20. JSON Schema

Schemas deverão possuir:

- Nome.
    
- Versão.
    
- Conteúdo.
    
- Checksum opcional.
    
- Compatibilidade conhecida.
    

Exemplo:

```csharp
public sealed record StructuredOutputSchema(
    string Name,
    string Version,
    string JsonSchema);
```

O provider poderá utilizar o schema nativamente ou receber instruções no prompt.

O resultado deverá ser validado novamente internamente.

---

# 21. Prompt Rendering

Prompts poderão ser armazenados e versionados fora da Infrastructure, mas o rendering técnico poderá possuir adapter.

Exemplo:

```csharp
public interface IPromptTemplateRenderer
{
    Result<string> Render(
        string template,
        IReadOnlyDictionary<string, object?> values);
}
```

A Infrastructure poderá implementar o motor de template.

A escolha do prompt e da versão pertence à Application.

---

# 22. Proteção de Prompts

Logs não deverão registrar automaticamente:

- System prompts.
    
- User prompts.
    
- Contextos privados.
    
- Conteúdo de documentos.
    
- Outputs completos.
    

Poderão ser registrados:

- PromptVersion.
    
- Hash.
    
- Tamanho.
    
- Quantidade estimada de tokens.
    
- Tipo de step.
    
- Provider.
    
- Modelo.
    

Logs completos só deverão existir em diagnóstico controlado e com redaction.

---

# 23. Custo e Uso

A Infrastructure deverá mapear uso de tokens.

Exemplo:

```csharp
public sealed record TokenUsage(
    int InputTokens,
    int OutputTokens,
    int TotalTokens);
```

O cálculo monetário poderá depender de tabela de preços versionada.

A Infrastructure pode coletar dados técnicos.

A regra de cobrança e orçamento pertence à Application ou ao domínio de Billing.

---

# 24. Streaming de IA

Streaming poderá ser introduzido futuramente.

Abstração possível:

```csharp
public interface IStreamingTextGenerationProvider
{
    IAsyncEnumerable<TextGenerationChunk> GenerateStreamAsync(
        TextGenerationRequest request,
        CancellationToken cancellationToken);
}
```

Para o MVP, o processamento em Worker poderá utilizar respostas completas.

Streaming não deverá ser implementado apenas porque o provider oferece suporte.

---

# 25. RabbitMQ

RabbitMQ será o broker principal de mensagens.

A Infrastructure será responsável por:

- Criar conexões.
    
- Criar canais.
    
- Declarar topology.
    
- Publicar mensagens.
    
- Serializar envelopes.
    
- Propagar headers.
    
- Confirmar publicação.
    
- Implementar health checks.
    
- Mapear falhas.
    
- Gerenciar reconexão.
    

O consumo e a execução dos Consumers permanecerão no Worker.

---

# 26. Separação RabbitMQ e Worker

## Infrastructure

```text
Conexão
Canal
Publisher
Serializer
Topology
Headers
Health Check
```

## Worker

```text
Consumer
Message Handler
Ack
Nack
Retry orchestration
Dead Letter handling
Application dispatch
```

Infrastructure fornece os componentes técnicos.

Worker hospeda o consumo.

---

# 27. Estrutura de Mensageria

```text
Infrastructure
└── Messaging
    └── RabbitMQ
        ├── RabbitMqOptions.cs
        ├── RabbitMqConnection.cs
        ├── RabbitMqChannelFactory.cs
        ├── RabbitMqMessagePublisher.cs
        ├── RabbitMqTopologyInitializer.cs
        ├── RabbitMqMessageSerializer.cs
        ├── RabbitMqHeaderMapper.cs
        ├── RabbitMqErrorMapper.cs
        └── RabbitMqHealthCheck.cs
```

---

# 28. RabbitMqOptions

```csharp
public sealed class RabbitMqOptions
{
    public const string SectionName = "Messaging:RabbitMQ";

    public required string HostName { get; init; }

    public int Port { get; init; } = 5672;

    public required string VirtualHost { get; init; }

    public required string UsernameSecretName { get; init; }

    public required string PasswordSecretName { get; init; }

    public bool UseTls { get; init; } = true;

    public int ConnectionTimeoutSeconds { get; init; } = 10;

    public int PublisherConfirmTimeoutSeconds { get; init; } = 10;
}
```

Credenciais serão resolvidas pelo mecanismo de secrets.

---

# 29. Conexões RabbitMQ

Conexões deverão ser:

- Reutilizadas.
    
- Thread-safe conforme cliente.
    
- Recuperáveis.
    
- Observáveis.
    
- Encerradas graciosamente.
    
- Criadas por host.
    

Canais não deverão ser compartilhados de forma insegura entre threads.

O modelo deverá respeitar as garantias do client RabbitMQ utilizado.

---

# 30. Message Publisher

Interface definida na Application:

```csharp
public interface IMessagePublisher
{
    Task<Result> PublishAsync<TMessage>(
        TMessage message,
        MessageMetadata metadata,
        CancellationToken cancellationToken);
}
```

Implementação:

```csharp
public sealed class RabbitMqMessagePublisher
    : IMessagePublisher
{
}
```

Para mensagens associadas a alterações no banco, o fluxo principal continuará utilizando Outbox.

---

# 31. Publisher Confirms

Publicações RabbitMQ deverão utilizar Publisher Confirms quando a garantia for necessária.

Fluxo:

```text
Publicar
    ↓
Aguardar confirmação
    ├── Ack → sucesso
    ├── Nack → falha
    └── Timeout → resultado incerto
```

Timeout de confirmação deverá ser tratado como falha potencialmente transitória.

A mensagem permanecerá pendente na Outbox até confirmação.

---

# 32. Envelope de Mensagem

Envelope conceitual:

```csharp
public sealed record MessageEnvelope<TPayload>(
    Guid MessageId,
    string MessageType,
    string MessageVersion,
    DateTimeOffset OccurredAt,
    Guid OrganizationId,
    string? CorrelationId,
    string? CausationId,
    string? IdempotencyKey,
    TPayload Payload,
    IReadOnlyDictionary<string, string> Metadata);
```

O envelope deverá ser independente do RabbitMQ.

A Infrastructure mapeará seus campos para:

- Payload.
    
- Headers.
    
- Routing key.
    
- Properties.
    

---

# 33. Headers RabbitMQ

Headers úteis:

```text
message_id
message_type
message_version
organization_id
correlation_id
causation_id
traceparent
tracestate
idempotency_key
occurred_at
content_type
```

Não incluir:

- Secrets.
    
- Prompts completos.
    
- Dados pessoais.
    
- Tokens de autenticação.
    
- Objetos complexos desnecessários.
    

---

# 34. Serialização de Mensagens

JSON será o formato inicial.

Regras:

- UTF-8.
    
- `camelCase`.
    
- Versão explícita.
    
- Content-Type definido.
    
- Tamanho limitado.
    
- Serialização determinística quando necessário.
    
- Contratos imutáveis.
    
- Compatibilidade retroativa.
    

Não serializar nomes completos de tipos CLR como contrato principal.

---

# 35. Routing Keys

Convenção sugerida:

```text
pipeline.execution.requested.v1
pipeline.execution.completed.v1
artifact.generated.v1
publication.requested.v1
```

O nome deverá representar:

```text
domínio.ação.versão
```

A versão também poderá permanecer em header, mas a convenção deverá ser consistente.

---

# 36. Exchanges

Tipos possíveis:

- Topic exchange para eventos.
    
- Direct exchange para comandos.
    
- Dead-letter exchange.
    
- Retry exchanges quando necessário.
    

Exemplo conceitual:

```text
infinite-content.commands
infinite-content.events
infinite-content.dead-letter
```

A topology final deverá ser documentada na arquitetura de mensageria.

---

# 37. Topology Initialization

A Infrastructure poderá declarar:

- Exchanges.
    
- Queues.
    
- Bindings.
    
- Dead-letter configuration.
    
- Durability.
    
- Message TTL.
    
- Maximum queue length.
    

A inicialização deverá ser:

- Idempotente.
    
- Compatível com múltiplas instâncias.
    
- Observável.
    
- Validada no startup.
    

Alterações incompatíveis de topology deverão ser tratadas por deploy controlado.

---

# 38. Mensagens Persistentes

Mensagens importantes deverão ser publicadas como persistentes.

Isso não garante processamento exatamente uma vez.

A garantia continuará sendo:

```text
At-least-once
+
Inbox
+
Idempotência
```

---

# 39. Dead Letter

Mensagens que não puderem ser processadas deverão seguir para Dead Letter quando apropriado.

A Infrastructure configura a topology.

O Worker decide quando:

- Rejeitar.
    
- Reagendar.
    
- Confirmar.
    
- Enviar para Dead Letter.
    

Mensagens em Dead Letter deverão gerar alerta e possuir ferramenta de replay controlado.

---

# 40. Redis

Redis será utilizado somente quando trouxer benefício concreto.

Usos permitidos:

- Cache distribuído.
    
- Rate limiting distribuído.
    
- Locks distribuídos.
    
- Deduplicação temporária.
    
- Estado efêmero.
    
- Coordenação técnica.
    
- Cache de metadados externos.
    

Redis não será fonte primária da verdade para:

- Projects.
    
- Pipelines.
    
- Executions.
    
- Artifacts.
    
- Approvals.
    
- Publications.
    

O sistema deverá continuar correto sem o cache.

---

# 41. Estrutura Redis

```text
Infrastructure
└── Caching
    └── Redis
        ├── RedisOptions.cs
        ├── RedisConnectionFactory.cs
        ├── RedisCacheService.cs
        ├── RedisKeyBuilder.cs
        ├── RedisDistributedLock.cs
        ├── RedisSerializer.cs
        └── RedisHealthCheck.cs
```

---

# 42. RedisOptions

```csharp
public sealed class RedisOptions
{
    public const string SectionName = "Caching:Redis";

    public required string ConfigurationSecretName { get; init; }

    public string InstanceName { get; init; } =
        "infinite-content-ai";

    public int DefaultExpirationSeconds { get; init; } = 300;

    public int ConnectionTimeoutSeconds { get; init; } = 5;
}
```

---

# 43. Chaves de Cache

Convenção:

```text
{environment}:{application}:{organization}:{resource}:{identifier}:{version}
```

Exemplo:

```text
prod:infinite-content:org-123:project:project-456:v1
```

Regras:

- Incluir Environment.
    
- Incluir Organization.
    
- Utilizar prefixo da aplicação.
    
- Evitar dados sensíveis.
    
- Limitar tamanho.
    
- Utilizar versão de chave.
    
- Ser determinística.
    

---

# 44. Cache-aside

Estratégia principal:

```text
Application
    ↓
Consultar cache
    ├── Hit → retornar
    └── Miss
          ↓
       Consultar fonte
          ↓
       Atualizar cache
```

A Application decide quando usar cache.

Infrastructure implementa armazenamento e serialização.

---

# 45. Falha de Cache

Falha de Redis deverá ser classificada.

Para cache de leitura:

- Registrar falha.
    
- Consultar fonte principal.
    
- Não quebrar o caso de uso quando possível.
    

Para locks ou rate limiting:

- Aplicar política explícita de fail-open ou fail-closed.
    
- Não tomar decisão implicitamente.
    

---

# 46. Cache Stampede

Mitigações possíveis:

- Expiração com jitter.
    
- Single-flight.
    
- Lock curto.
    
- Refresh antecipado.
    
- Stale-while-revalidate.
    

Essas técnicas somente deverão ser adicionadas após necessidade real.

---

# 47. Locks Distribuídos

Interface possível:

```csharp
public interface IDistributedLockManager
{
    Task<Result<IDistributedLockHandle>> TryAcquireAsync(
        string key,
        TimeSpan duration,
        CancellationToken cancellationToken);
}
```

Usos possíveis:

- Reconciliação única.
    
- Job singleton.
    
- Proteção de operação externa não idempotente.
    
- Refresh de cache.
    

Locks distribuídos não substituirão:

- Constraints.
    
- Concorrência otimista.
    
- Inbox.
    
- Idempotência.
    

---

# 48. Segurança de Locks

Locks deverão possuir:

- Token exclusivo.
    
- Expiração.
    
- Renovação controlada.
    
- Liberação somente pelo proprietário.
    
- Timeout.
    
- Métricas.
    
- Tratamento de perda do lock.
    

Não usar apenas:

```text
SET key value
```

sem garantias de propriedade e expiração.

---

# 49. Azure Blob Storage

Azure Blob Storage será utilizado para arquivos e artefatos binários.

Exemplos:

- Imagens.
    
- Áudio.
    
- Vídeo.
    
- Documentos.
    
- Exportações.
    
- Artefatos intermediários.
    
- Payloads grandes.
    

O PostgreSQL armazenará apenas metadados e referências.

---

# 50. Estrutura de Storage

```text
Infrastructure
└── Storage
    └── AzureBlob
        ├── AzureBlobStorageOptions.cs
        ├── AzureBlobStorageService.cs
        ├── AzureBlobClientFactory.cs
        ├── AzureBlobReferenceMapper.cs
        ├── AzureBlobUploadValidator.cs
        ├── AzureBlobHealthCheck.cs
        └── AzureBlobErrorMapper.cs
```

---

# 51. Abstração de Storage

Definida pela Application:

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

    Task<Result<TemporaryAccessUrl>> CreateReadUrlAsync(
        StorageReference reference,
        TimeSpan lifetime,
        CancellationToken cancellationToken);
}
```

---

# 52. StorageReference

Modelo interno:

```csharp
public sealed record StorageReference(
    string Provider,
    string Container,
    string ObjectKey,
    string ContentType,
    long Size,
    string Checksum);
```

Não utilizar tipos do SDK Azure fora da Infrastructure.

---

# 53. Object Keys

Convenção possível:

```text
organizations/{organizationId}/projects/{projectId}/executions/{executionId}/artifacts/{artifactId}/{version}/{fileName}
```

Regras:

- Incluir Organization.
    
- Não confiar no nome enviado pelo usuário.
    
- Normalizar segmentos.
    
- Impedir path traversal.
    
- Utilizar IDs opacos.
    
- Evitar dados sensíveis.
    
- Manter versão do artefato.
    

---

# 54. Containers

Possíveis containers:

```text
artifacts
uploads
exports
temporary
```

A separação deverá considerar:

- Retenção.
    
- Permissões.
    
- Lifecycle.
    
- Criptografia.
    
- Acesso público.
    
- Volume.
    
- Políticas de rede.
    

Containers públicos não deverão ser usados para conteúdo privado.

---

# 55. Uploads Diretos

Fluxo recomendado:

```text
Cliente
    ↓
API solicita autorização temporária
    ↓
Infrastructure gera SAS restrita
    ↓
Cliente envia ao Blob Storage
    ↓
API confirma upload
    ↓
Infrastructure verifica objeto
    ↓
Application registra Artifact
```

A autorização temporária deverá limitar:

- Container.
    
- Object key.
    
- Operação.
    
- Tamanho.
    
- Content-Type, quando possível.
    
- Tempo de validade.
    

---

# 56. URLs Temporárias

URLs temporárias deverão:

- Expirar rapidamente.
    
- Possuir somente as permissões necessárias.
    
- Não ser registradas em logs completos.
    
- Ser emitidas após autorização.
    
- Ser específicas para um objeto.
    
- Utilizar HTTPS.
    

O Domain armazenará a referência, não a URL temporária.

---

# 57. Validação de Arquivos

Infrastructure poderá validar aspectos técnicos:

- Content-Type real.
    
- Assinatura do arquivo.
    
- Tamanho.
    
- Checksum.
    
- Extensão.
    
- Codificação.
    
- Presença no storage.
    
- Metadados.
    

Regras de negócio sobre tipos permitidos pertencem à Application.

---

# 58. Checksum

Algoritmo inicial:

```text
SHA-256
```

Usos:

- Verificação de integridade.
    
- Detecção de upload incompleto.
    
- Deduplicação controlada.
    
- Auditoria.
    
- Reprodutibilidade.
    

Checksums não deverão ser usados como mecanismo de autenticação.

---

# 59. Exclusão de Arquivos

Exclusão deverá ser coordenada pela Application.

Fluxo:

```text
Application
    ↓
Verifica regra de negócio
    ↓
Solicita exclusão ao Storage
    ↓
Atualiza metadados
```

Falhas parciais deverão ser reconciliadas.

Não excluir arquivos automaticamente a partir de eventos do banco sem estratégia de recuperação.

---

# 60. Azure Key Vault

Secrets poderão ser armazenados no Azure Key Vault.

Exemplos:

- API keys.
    
- Credenciais RabbitMQ.
    
- Connection strings Redis.
    
- Tokens de redes sociais.
    
- Certificados.
    
- Chaves de assinatura.
    

Infrastructure será responsável por resolver secrets.

---

# 61. Abstração de Secrets

```csharp
public interface ISecretProvider
{
    Task<Result<string>> GetSecretAsync(
        string secretName,
        CancellationToken cancellationToken);
}
```

Implementações:

```text
AzureKeyVaultSecretProvider
EnvironmentSecretProvider
DevelopmentSecretProvider
```

A Application deverá utilizar referências de credenciais, não valores secretos.

---

# 62. Cache de Secrets

Secrets poderão ser mantidos em memória por tempo controlado.

Cuidados:

- Expiração.
    
- Rotação.
    
- Não registrar valores.
    
- Evitar cópias desnecessárias.
    
- Não persistir no Redis.
    
- Limpar referências quando possível.
    

A estratégia dependerá do SDK e do ambiente.

---

# 63. Managed Identity

Em Azure, Managed Identity deverá ser preferida para:

- Key Vault.
    
- Blob Storage.
    
- Outros recursos Azure.
    

Isso reduz:

- Secrets estáticos.
    
- Rotação manual.
    
- Risco de vazamento.
    
- Complexidade operacional.
    

A configuração local poderá utilizar credenciais de desenvolvimento.

---

# 64. n8n

A integração com n8n será implementada como adapter externo.

Usos:

- Acionar workflows.
    
- Receber callbacks.
    
- Automatizar integrações.
    
- Coordenar processos externos não críticos.
    
- Integrar ferramentas com baixo acoplamento.
    

O n8n não deverá conter regras centrais indispensáveis ao Domain.

---

# 65. Estrutura n8n

```text
Infrastructure
└── Automation
    └── N8n
        ├── N8nOptions.cs
        ├── N8nClient.cs
        ├── N8nRequestMapper.cs
        ├── N8nResponseMapper.cs
        ├── N8nSignatureValidator.cs
        └── N8nHealthCheck.cs
```

---

# 66. Abstração de Automação

```csharp
public interface IAutomationService
{
    Task<Result<AutomationExecution>> TriggerAsync(
        AutomationRequest request,
        CancellationToken cancellationToken);
}
```

Request:

```csharp
public sealed record AutomationRequest(
    string Workflow,
    string Version,
    IReadOnlyDictionary<string, object?> Inputs,
    string CallbackReference);
```

O nome técnico do webhook do n8n não deverá vazar para o Domain.

---

# 67. Callbacks do n8n

Callbacks deverão possuir:

- Assinatura.
    
- Timestamp.
    
- MessageId.
    
- CorrelationId.
    
- OrganizationId.
    
- Workflow version.
    
- Idempotency key.
    

Fluxo:

```text
n8n
    ↓
API Webhook
    ↓
Validação técnica
    ↓
Inbox
    ↓
Application
```

Infrastructure poderá validar assinatura.

Api recebe o HTTP.

Data persiste Inbox.

Application processa o caso de uso.

---

# 68. Serviços Auxiliares em Python

Um serviço Python poderá ser utilizado para capacidades específicas.

Exemplos:

- Processamento de mídia.
    
- Bibliotecas exclusivas.
    
- Manipulação avançada de documentos.
    
- Modelos locais.
    
- Extração.
    
- Conversão.
    
- Análise especializada.
    

Ele será tratado como serviço externo.

---

# 69. Estrutura do Client Python

```text
Infrastructure
└── Python
    ├── PythonServiceOptions.cs
    ├── PythonServiceClient.cs
    ├── PythonRequestMapper.cs
    ├── PythonResponseMapper.cs
    ├── PythonErrorMapper.cs
    └── PythonServiceHealthCheck.cs
```

A Application definirá uma abstração baseada na capacidade, não na linguagem.

Evitar:

```csharp
IPythonService
```

Preferir:

```csharp
IMediaProcessingService
IDocumentExtractionService
IAudioTranscriptionService
```

---

# 70. Clientes HTTP

Clientes HTTP deverão utilizar `IHttpClientFactory`.

Exemplo:

```csharp
services.AddHttpClient<
    ITextGenerationProvider,
    OpenAiTextGenerationProvider>(
        client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = Timeout.InfiniteTimeSpan;
        });
```

Timeouts deverão ser aplicados por policies e CancellationToken.

Evitar criar `HttpClient` manualmente por requisição.

---

# 71. Named e Typed Clients

Typed clients serão preferidos.

Exemplos:

```text
OpenAiClient
N8nClient
YouTubeClient
PythonServiceClient
```

Benefícios:

- Configuração isolada.
    
- Policies específicas.
    
- Testabilidade.
    
- Telemetria.
    
- Headers próprios.
    
- Timeouts próprios.
    

---

# 72. Delegating Handlers

Possíveis handlers:

```text
CorrelationPropagationHandler
AuthenticationHandler
RequestSigningHandler
TelemetryHandler
RedactionHandler
```

Cada handler deverá possuir responsabilidade única.

A ordem deve ser explícita.

---

# 73. Propagação de Correlação

Headers possíveis:

```text
X-Correlation-ID
traceparent
tracestate
```

A Infrastructure deverá propagar contexto técnico para dependências externas quando permitido.

Não enviar headers internos para serviços não confiáveis sem necessidade.

---

# 74. Autenticação Externa

Formas possíveis:

- Bearer token.
    
- API key.
    
- OAuth 2.0.
    
- Managed Identity.
    
- Assinatura HMAC.
    
- Certificado cliente.
    

Credenciais deverão ser adicionadas por handler ou client dedicado.

Não deverão aparecer em Commands da Application.

---

# 75. OAuth de Integrações

Integrações com YouTube ou redes sociais poderão exigir:

- Access token.
    
- Refresh token.
    
- Escopos.
    
- Expiração.
    
- Revogação.
    

O Domain poderá armazenar uma `CredentialReferenceId`.

Infrastructure será responsável por:

- Resolver tokens.
    
- Renovar tokens.
    
- Mapear erros.
    
- Proteger dados.
    
- Não registrar credenciais.
    

Persistência cifrada de tokens deverá possuir componente apropriado, sem adicionar DbContext à Infrastructure.

A coordenação com persistência ocorrerá por abstrações da Application.

---

# 76. Resiliência

A Infrastructure aplicará resiliência técnica em operações externas.

Mecanismos:

- Timeout.
    
- Retry.
    
- Circuit breaker.
    
- Rate limiting.
    
- Hedging, somente se seguro.
    
- Bulkhead.
    
- Fallback técnico controlado.
    

Esses mecanismos não deverão alterar regras de negócio silenciosamente.

---

# 77. Timeout

Toda chamada externa deverá possuir timeout explícito.

Categorias possíveis:

```text
AI provider: 30–120 segundos
RabbitMQ publish: poucos segundos
Redis: poucos segundos
Storage: variável por tamanho
Webhook: poucos segundos
API externa: conforme contrato
```

Valores finais deverão ser configuráveis e medidos.

---

# 78. Timeout por Operação

Uma mesma integração poderá possuir timeouts diferentes.

Exemplo:

```text
OpenAI metadata request: 10 s
OpenAI text generation: 90 s
Blob metadata: 5 s
Blob upload: 120 s
```

Não utilizar um único timeout global para toda a Infrastructure.

---

# 79. Retry Técnico

Retry deverá ser aplicado somente a falhas transitórias.

Exemplos:

- Timeout.
    
- HTTP 429.
    
- HTTP 502.
    
- HTTP 503.
    
- HTTP 504.
    
- Falha temporária de conexão.
    
- Channel indisponível.
    

Não repetir automaticamente:

- HTTP 400.
    
- Autenticação inválida.
    
- Payload inválido.
    
- Conteúdo bloqueado.
    
- Recurso inexistente.
    
- Operação não idempotente sem proteção.
    

---

# 80. Backoff

Estratégia preferida:

```text
Exponential backoff
+
Jitter
```

Exemplo conceitual:

```text
1 s
2 s
4 s
8 s
```

com variação aleatória.

Isso reduz sincronização entre instâncias.

---

# 81. Retry e Operações Longas

Retries internos devem ser curtos.

Retries de negócio ou que exigem espera longa deverão ser coordenados pelo Worker.

Exemplo:

```text
Infrastructure
    1 ou 2 retries rápidos

Worker
    retry após 30 s, 5 min ou 30 min
```

Não manter thread ou conexão aguardando longos períodos.

---

# 82. Circuit Breaker

Circuit breaker poderá ser aplicado a dependências instáveis.

Estados:

```text
Closed
Open
Half-open
```

Quando aberto:

- Falhar rapidamente.
    
- Registrar métrica.
    
- Evitar sobrecarregar o provider.
    
- Permitir recuperação gradual.
    

O erro deverá indicar indisponibilidade transitória.

---

# 83. Fallback

Fallback somente deverá ser aplicado quando for explicitamente permitido.

Exemplo aceitável:

```text
Provider primário indisponível
    ↓
Provider secundário compatível
```

Somente se:

- A política permitir.
    
- O modelo alternativo for aceito.
    
- O custo for permitido.
    
- Structured Output for compatível.
    
- A mudança for registrada.
    
- O resultado registrar o provider real.
    

Nunca trocar provider silenciosamente em operações sensíveis.

---

# 84. Bulkhead

Bulkhead poderá limitar concorrência por dependência.

Exemplos:

- Máximo de chamadas simultâneas para IA.
    
- Máximo de uploads.
    
- Máximo de publicações externas.
    
- Fila interna limitada.
    

Isso evita que uma integração esgote todos os recursos do host.

---

# 85. Rate Limiting de Saída

A Infrastructure poderá limitar chamadas por:

- Provider.
    
- Modelo.
    
- Organization.
    
- Credencial.
    
- Endpoint.
    
- Região.
    

A política de negócio sobre cotas pertence à Application.

A implementação técnica do limitador pode ficar em Infrastructure.

---

# 86. Hedging

Hedging envia requisições concorrentes para reduzir latência.

Não deverá ser usado por padrão.

Riscos:

- Duplicação.
    
- Custo extra.
    
- Efeitos externos duplicados.
    
- Maior carga.
    
- Complexidade.
    

Somente consultas idempotentes e medidas poderão utilizar hedging.

---

# 87. Publicação em Plataformas Externas

Adapters poderão existir para:

```text
YouTube
WordPress
LinkedIn
Instagram
TikTok
Newsletter
```

Cada integração deverá implementar abstração baseada na capacidade.

Exemplo:

```csharp
public interface IContentPublisher
{
    string Target { get; }

    Task<Result<PublicationResponse>> PublishAsync(
        PublicationRequest request,
        CancellationToken cancellationToken);
}
```

---

# 88. PublicationRequest

```csharp
public sealed record PublicationRequest(
    OrganizationId OrganizationId,
    ArtifactReference Content,
    string Title,
    string? Description,
    IReadOnlyCollection<string> Tags,
    CredentialReferenceId CredentialReference,
    string IdempotencyKey);
```

A Infrastructure converte o request para o contrato externo.

---

# 89. Idempotência Externa

Quando a API externa oferecer idempotency key, ela deverá ser utilizada.

Quando não oferecer:

- Registrar identificadores externos.
    
- Consultar estado antes de repetir.
    
- Utilizar deduplicação local.
    
- Evitar retry automático de resultado incerto.
    
- Solicitar reconciliação.
    

Publicação externa é uma operação de alto risco para duplicação.

---

# 90. Resultado Incerto

Cenário:

```text
Requisição enviada
    ↓
Timeout antes da resposta
```

Não é possível saber imediatamente se o conteúdo foi publicado.

O erro deverá ser classificado como:

```text
OutcomeUnknown
```

A Application ou Worker deverá iniciar reconciliação antes de repetir.

---

# 91. Webhooks de Saída

A plataforma poderá enviar webhooks para clientes.

Infrastructure será responsável por:

- HTTP.
    
- Assinatura.
    
- Headers.
    
- Timeout.
    
- Classificação de erros.
    
- Telemetria.
    

A Application decide:

- Qual evento enviar.
    
- Para qual tenant.
    
- Qual payload.
    
- Qual versão.
    

---

# 92. Assinatura HMAC

Exemplo conceitual:

```text
timestamp.payload
    ↓
HMAC-SHA256
    ↓
Signature header
```

Headers:

```text
X-Webhook-ID
X-Webhook-Timestamp
X-Webhook-Signature
```

O secret não deverá ser incluído no payload.

---

# 93. Validação de Webhooks Recebidos

Infrastructure poderá implementar:

```csharp
public interface IWebhookSignatureValidator
{
    Result Validate(
        WebhookSignatureContext context);
}
```

Validar:

- Assinatura.
    
- Timestamp.
    
- Tolerância de relógio.
    
- Algoritmo.
    
- Secret correto.
    
- Payload original.
    
- Replay.
    

---

# 94. Proteção contra Replay

Utilizar:

- MessageId.
    
- Timestamp.
    
- Janela máxima.
    
- Inbox.
    
- Nonce quando suportado.
    

Uma assinatura válida não impede replay do mesmo payload.

---

# 95. Observabilidade

Infrastructure deverá produzir telemetria para todas as dependências externas.

Dados úteis:

```text
dependency.name
dependency.type
operation
provider
model
status
duration
retry_count
circuit_state
error_code
request_size
response_size
```

Dimensões deverão possuir cardinalidade controlada.

---

# 96. Activities

Poderá existir uma fonte própria:

```csharp
public static class InfrastructureActivitySource
{
    public static readonly ActivitySource Instance =
        new("InfiniteContentAI.Infrastructure");
}
```

Spans poderão representar:

```text
ai.generate
rabbitmq.publish
redis.get
redis.set
blob.upload
n8n.trigger
youtube.publish
python.process
```

---

# 97. Métricas

Métricas sugeridas:

```text
ai.requests
ai.request.duration
ai.tokens.input
ai.tokens.output
ai.failures
ai.rate_limits

messaging.publish.duration
messaging.publish.failures
messaging.connection.state

cache.operations
cache.hit
cache.miss
cache.failures

storage.upload.duration
storage.download.duration
storage.bytes
storage.failures

external_http.duration
external_http.failures
resilience.retry.count
resilience.circuit.open
```

---

# 98. Cardinalidade

Não utilizar como labels:

- Prompt completo.
    
- UserId.
    
- ExecutionId.
    
- ArtifactId.
    
- CorrelationId.
    
- URL completa.
    
- Object key completo.
    
- Provider request ID.
    

Esses valores poderão aparecer em logs ou traces controlados, não em métricas agregadas.

---

# 99. Logging Estruturado

Logs deverão utilizar propriedades estruturadas.

Exemplo:

```csharp
_logger.LogWarning(
    "AI provider request failed. Provider: {Provider}, Model: {Model}, ErrorCode: {ErrorCode}",
    provider,
    model,
    error.Code);
```

Evitar interpolação que destrua estrutura:

```csharp
_logger.LogWarning(
    $"Provider {provider} failed.");
```

---

# 100. Redaction

Dados que devem ser redigidos:

- API keys.
    
- Bearer tokens.
    
- Connection strings.
    
- SAS tokens.
    
- Refresh tokens.
    
- Cookies.
    
- Prompt privado.
    
- Conteúdo sensível.
    
- Credenciais RabbitMQ.
    
- Secrets de webhook.
    

Poderá existir um serviço:

```csharp
public interface ISensitiveDataRedactor
{
    string Redact(string value);
}
```

Logs não deverão depender de redaction tardia para proteger secrets conhecidos.

O ideal é nunca incluí-los.

---

# 101. Health Checks

Infrastructure poderá fornecer health checks para:

- RabbitMQ.
    
- Redis.
    
- Azure Blob Storage.
    
- Providers obrigatórios.
    
- n8n.
    
- Serviço Python.
    
- APIs externas críticas.
    

O host decide quais entram em readiness.

---

# 102. Dependências Opcionais

Um provider opcional indisponível não deverá tornar toda a API indisponível.

Exemplo:

```text
OpenAI configurado e obrigatório → readiness pode falhar
Anthropic opcional → readiness geral pode continuar saudável
```

A criticidade deverá ser configurável.

---

# 103. Health Check Leve

Health checks não deverão:

- Gerar conteúdo caro.
    
- Publicar mensagens reais.
    
- Fazer uploads grandes.
    
- Executar workflows.
    
- Consumir quotas relevantes.
    
- Alterar estado externo.
    

Preferir:

- Ping.
    
- Metadata.
    
- Conexão.
    
- Operação de baixo custo.
    
- Verificação de credencial.
    

---

# 104. Segurança

Infrastructure deverá aplicar:

- TLS.
    
- Validação de certificados.
    
- Menor privilégio.
    
- Rotação de credenciais.
    
- Secrets externos.
    
- URLs permitidas.
    
- Proteção contra SSRF.
    
- Validação de redirects.
    
- Timeouts.
    
- Limites de payload.
    
- Sanitização de logs.
    
- Criptografia.
    
- Assinaturas.
    

---

# 105. Proteção contra SSRF

Clientes que acessarem URLs dinâmicas deverão bloquear:

- `localhost`.
    
- Loopback.
    
- Redes privadas.
    
- Metadata endpoints.
    
- Portas não permitidas.
    
- Protocolos diferentes de HTTPS.
    
- Redirects para hosts proibidos.
    
- DNS rebinding.
    

Preferir allowlist de hosts.

---

# 106. DNS e Endereços Privados

A validação deverá ocorrer após resolução DNS quando URLs forem configuráveis.

Bloquear faixas como:

```text
127.0.0.0/8
10.0.0.0/8
172.16.0.0/12
192.168.0.0/16
169.254.0.0/16
::1
fc00::/7
```

Exceções internas deverão ser explicitamente configuradas.

---

# 107. Redirects

Clientes sensíveis deverão limitar ou desabilitar redirects automáticos.

Um host permitido poderá redirecionar para um endereço privado.

Quando redirects forem aceitos, cada destino deverá ser validado.

---

# 108. Certificados

Validação de certificado não deverá ser desabilitada em produção.

Exemplo proibido:

```csharp
handler.ServerCertificateCustomValidationCallback =
    (_, _, _, _) => true;
```

Certificados internos deverão utilizar cadeia confiável apropriada.

---

# 109. Ambientes

## Development

- Providers fake permitidos.
    
- Serviços locais.
    
- Secrets por User Secrets.
    
- Logs mais detalhados.
    
- Emuladores opcionais.
    

## Test

- Adapters fake.
    
- RabbitMQ e Redis em containers.
    
- Storage emulado ou isolado.
    
- Credenciais de teste.
    

## Staging

- Integrações sandbox.
    
- Configuração próxima à produção.
    
- Quotas reduzidas.
    
- Dados não produtivos.
    

## Production

- Managed Identity.
    
- Key Vault.
    
- TLS.
    
- Rede restrita.
    
- Redaction.
    
- Providers reais.
    
- Health checks controlados.
    
- Observabilidade completa.
    

---

# 110. Emuladores

Em desenvolvimento poderão ser usados:

- Azurite.
    
- RabbitMQ local.
    
- Redis local.
    
- Fake AI Provider.
    
- Mock server HTTP.
    
- Serviço Python local.
    

Emuladores não substituem testes periódicos com serviços reais.

---

# 111. Testes Unitários

Testes unitários poderão validar:

- Request mappers.
    
- Response mappers.
    
- Error mappers.
    
- Serialização.
    
- Header mapping.
    
- Key builders.
    
- Signature validation.
    
- Retry classification.
    
- URL validation.
    
- Redaction.
    
- Provider resolver.
    

---

# 112. Testes de Integração

Testar com dependências reais ou containers:

- RabbitMQ.
    
- Redis.
    
- Azurite.
    
- Mock HTTP server.
    
- Serviço Python de teste.
    
- Providers sandbox quando viável.
    

Os testes deverão validar protocolos e configurações reais.

---

# 113. Testes de Contrato de Providers

Testes deverão validar que cada adapter converte corretamente:

```text
Application Request
    ↓
Provider Request
    ↓
Provider Response
    ↓
Application Response
```

Também testar:

- Rate limit.
    
- Timeout.
    
- Resposta inválida.
    
- Erro de autenticação.
    
- Structured Output inválido.
    
- Modelo inexistente.
    
- Cancelamento.
    

---

# 114. Mock HTTP Server

Providers HTTP poderão ser testados com servidor controlado.

Cenários:

```text
200 válido
200 inválido
400
401
404
429
500
503
Timeout
Conexão encerrada
Payload excessivo
```

Evitar mocks que apenas confirmem chamadas de métodos internos.

Testar o contrato HTTP real do adapter.

---

# 115. Testes RabbitMQ

Validar:

- Declaração de topology.
    
- Publicação.
    
- Headers.
    
- Routing key.
    
- Publisher confirms.
    
- Reconexão.
    
- Falha de canal.
    
- Serialização.
    
- Dead-letter configuration.
    

O consumo completo será testado no Worker.

---

# 116. Testes Redis

Validar:

- Serialização.
    
- Expiração.
    
- Prefixos.
    
- Isolamento por Organization.
    
- Locks.
    
- Falha de conexão.
    
- Comportamento de cache miss.
    
- Remoção.
    
- Jitter de TTL.
    

---

# 117. Testes de Storage

Validar:

- Upload.
    
- Download.
    
- Metadados.
    
- Checksum.
    
- Content-Type.
    
- URLs temporárias.
    
- Expiração.
    
- Isolamento por Organization.
    
- Exclusão.
    
- Objeto inexistente.
    
- Cancelamento.
    

---

# 118. Testes de Resiliência

Testar:

- Retry apenas em falhas transitórias.
    
- Ausência de retry em falhas permanentes.
    
- Circuit breaker.
    
- Timeout.
    
- CancellationToken.
    
- Retry-After.
    
- Fallback autorizado.
    
- Resultado incerto.
    
- Limites de concorrência.
    

---

# 119. Testes de Segurança

Testar:

- Secrets não aparecem em logs.
    
- URLs privadas são bloqueadas.
    
- Redirects maliciosos são bloqueados.
    
- Assinaturas inválidas são rejeitadas.
    
- Webhooks antigos são rejeitados.
    
- Replay é detectado.
    
- Object keys não permitem path traversal.
    
- SAS possui escopo restrito.
    
- Certificados inválidos falham.
    

---

# 120. Antipadrões

## DbContext na Infrastructure

Proibido.

## Repository de PostgreSQL na Infrastructure

Pertence ao Data.

## SDK vazando para Application

Adapters devem mapear contratos.

## HttpClient criado manualmente

Utilizar factory.

## Retry em qualquer erro

Repetir somente falhas transitórias.

## Retry longo dentro da chamada

Delegar retry de negócio ao Worker.

## Fallback silencioso

Registrar e aplicar política explícita.

## Redis como fonte da verdade

PostgreSQL permanece principal.

## Secret no appsettings

Utilizar referência e provider de secrets.

## Prompt em log

Registrar versão e metadados, não conteúdo completo.

## URL dinâmica sem validação

Risco de SSRF.

## Operação externa sem timeout

Toda chamada deve possuir limite.

## Classe IntegrationService genérica

Criar adapters específicos.

## Capturar Exception e retornar erro genérico

Mapear falhas conhecidas e preservar exceções inesperadas.

## Health check caro

Utilizar operação leve.

---

# 121. Regras Arquiteturais

1. Infrastructure depende apenas de Application, Domain, Contracts e SharedKernel.
    
2. Infrastructure não depende de Data.
    
3. Infrastructure não depende de Api.
    
4. Infrastructure não depende de Worker.
    
5. Infrastructure não contém DbContext.
    
6. Infrastructure não contém migrations.
    
7. Infrastructure não contém repositories de PostgreSQL.
    
8. SDKs externos permanecem encapsulados.
    
9. Application utiliza contratos próprios.
    
10. Toda chamada externa possui timeout.
    
11. CancellationToken é propagado.
    
12. Retry ocorre somente em falhas transitórias.
    
13. Retry técnico permanece curto.
    
14. Retries longos são coordenados pelo Worker.
    
15. Fallback exige política explícita.
    
16. Secrets não são armazenados no código.
    
17. Logs não expõem secrets.
    
18. Providers retornam erros classificados.
    
19. Structured Outputs são validados.
    
20. RabbitMQ publica mensagens da Outbox.
    
21. Infrastructure configura topology; Worker hospeda consumers.
    
22. Redis não é fonte primária da verdade.
    
23. Chaves Redis incluem Organization.
    
24. Storage utiliza referências próprias.
    
25. URLs temporárias possuem escopo e expiração.
    
26. n8n não contém regras centrais do Domain.
    
27. Webhooks possuem assinatura e proteção contra replay.
    
28. Clientes HTTP utilizam IHttpClientFactory.
    
29. URLs dinâmicas são protegidas contra SSRF.
    
30. Toda integração produz telemetria.
    
31. Health checks são leves.
    
32. Providers opcionais não derrubam a aplicação inteira.
    
33. Operações externas não iniciam transações de banco.
    
34. Resultado incerto é tratado explicitamente.
    
35. Testes validam os contratos externos.
    

---

# 122. Escopo do MVP

A primeira versão de Infrastructure deverá implementar:

## Inteligência Artificial

```text
ITextGenerationProvider implementations
FakeTextGenerationProvider
OpenAiTextGenerationProvider
TextGenerationProviderResolver
StructuredOutputValidator
```

## RabbitMQ

```text
RabbitMqConnection
RabbitMqMessagePublisher
RabbitMqMessageSerializer
RabbitMqTopologyInitializer
RabbitMqHealthCheck
```

## Storage

```text
AzureBlobStorageService
AzureBlobStorageOptions
StorageReference mapping
Checksum calculation
```

## Segurança

```text
EnvironmentSecretProvider
AzureKeyVaultSecretProvider
Credential resolution
```

## HTTP e Resiliência

```text
Typed HttpClients
Timeout policies
Retry policies
Correlation propagation
Error mapping
```

Redis e n8n poderão ser adiados até existir necessidade concreta.

---

# 123. Componentes Adiáveis

Não obrigatórios no primeiro fluxo:

- Anthropic Provider.
    
- Gemini Provider.
    
- Provider fallback automático.
    
- Redis.
    
- Locks distribuídos.
    
- n8n.
    
- YouTube.
    
- WordPress.
    
- LinkedIn.
    
- Serviço Python.
    
- Webhooks de saída.
    
- Streaming de IA.
    
- Circuit breaker sofisticado.
    
- Bulkheads avançados.
    
- Rate limiting de saída distribuído.
    
- Multi-região.
    
- Failover de storage.
    

---

# 124. Ordem de Implementação

## Etapa 1 — Fundação

- Criar projeto Infrastructure.
    
- Criar DependencyInjection.
    
- Configurar Options.
    
- Criar abstração de secrets.
    
- Configurar HttpClientFactory.
    
- Criar policies de timeout e retry.
    
- Configurar telemetria.
    

## Etapa 2 — Fake AI Provider

- Implementar provider fake.
    
- Criar cenários determinísticos.
    
- Simular falhas.
    
- Adicionar testes.
    

## Etapa 3 — Provider Real

- Implementar OpenAI.
    
- Mapear requests.
    
- Mapear responses.
    
- Classificar erros.
    
- Registrar tokens.
    
- Validar Structured Output.
    
- Adicionar testes de contrato.
    

## Etapa 4 — RabbitMQ

- Configurar conexão.
    
- Configurar topology.
    
- Implementar publisher.
    
- Implementar confirms.
    
- Propagar headers.
    
- Adicionar health check.
    
- Criar testes.
    

## Etapa 5 — Storage

- Configurar Azure Blob.
    
- Implementar upload.
    
- Implementar download.
    
- Implementar checksum.
    
- Implementar URLs temporárias.
    
- Adicionar testes.
    

## Etapa 6 — Secrets

- Configurar Key Vault.
    
- Configurar Managed Identity.
    
- Implementar cache seguro.
    
- Validar rotação.
    

## Etapa 7 — Evolução

- Redis.
    
- n8n.
    
- Python.
    
- Publicações.
    
- Webhooks.
    
- Providers adicionais.
    

---

# 125. Checklist para Novo Provider

- Implementa abstração da Application?
    
- SDK permanece encapsulado?
    
- Options estão tipadas?
    
- Configuração é validada?
    
- Secret é resolvido externamente?
    
- Existe timeout?
    
- Existe retry apenas para falhas transitórias?
    
- Erros são classificados?
    
- CancellationToken é propagado?
    
- Tokens são registrados?
    
- Structured Output é validado?
    
- Prompts não aparecem em logs?
    
- ProviderRequestId é capturado?
    
- Health check é leve?
    
- Existe Fake equivalente?
    
- Existem testes de contrato?
    

---

# 126. Checklist para Nova Integração HTTP

- Existe typed client?
    
- Base URL é validada?
    
- Autenticação está isolada?
    
- Headers de correlação são propagados?
    
- Timeout está definido?
    
- Retry está classificado?
    
- Circuit breaker é necessário?
    
- Payload possui limite?
    
- Resposta possui limite?
    
- Redirects são seguros?
    
- SSRF foi considerado?
    
- Erros são mapeados?
    
- Secrets estão protegidos?
    
- Há telemetria?
    
- Há testes com servidor controlado?
    

---

# 127. Checklist RabbitMQ

- Exchange está definida?
    
- Queue está definida?
    
- Routing key está definida?
    
- Durability está correta?
    
- Mensagem é persistente?
    
- Publisher confirms estão habilitados?
    
- Headers estão completos?
    
- MessageId está presente?
    
- OrganizationId está presente?
    
- Trace context está presente?
    
- Dead-letter está configurada?
    
- Contrato possui versão?
    
- Serialização está testada?
    
- Falhas são observáveis?
    
- Topology é idempotente?
    

---

# 128. Checklist Redis

- Cache é realmente necessário?
    
- A fonte da verdade permanece no PostgreSQL?
    
- A chave inclui Environment e Organization?
    
- Existe TTL?
    
- Existe jitter?
    
- A serialização possui versão?
    
- Dados sensíveis são permitidos?
    
- Falha de Redis quebra o fluxo?
    
- A política fail-open ou fail-closed está definida?
    
- Existem métricas?
    
- Existem testes de expiração?
    
- Locks possuem token e expiração?
    

---

# 129. Checklist Storage

- O Object Key é seguro?
    
- Inclui Organization?
    
- Content-Type é validado?
    
- Tamanho é limitado?
    
- Checksum é calculado?
    
- Dados são privados?
    
- URL temporária possui expiração?
    
- Permissões são mínimas?
    
- Upload pode ser direto?
    
- Falhas parciais são reconciliáveis?
    
- Exclusão é coordenada?
    
- Existem testes de isolamento?
    

---

# 130. Critérios de Qualidade

Infrastructure será considerada saudável quando:

- A Application não conhecer SDKs.
    
- Providers puderem ser trocados por configuração.
    
- Um provider fake permitir testes sem custo.
    
- Falhas externas forem classificadas.
    
- Toda chamada externa possuir timeout.
    
- Retries não causarem duplicação.
    
- RabbitMQ publicar mensagens confiavelmente.
    
- Redis não for necessário para correção.
    
- Storage não vazar detalhes Azure.
    
- Secrets não aparecerem no código ou logs.
    
- n8n permanecer complementar.
    
- Telemetria permitir investigar dependências.
    
- Health checks refletirem dependências críticas.
    
- DbContext estiver completamente ausente.
    
- Novos adapters puderem ser adicionados sem alterar o Domain.
    

---

# 131. Documentos Relacionados

```text
03 - Arquitetura/Mensageria e Comunicação entre Componentes.md
03 - Arquitetura/Tratamento de Erros e Resiliência.md
03 - Arquitetura/Arquitetura de Configuração.md
03 - Arquitetura/Estratégia de Testes.md

04 - Backend/Visão Geral do Backend.md
04 - Backend/Organização por Features.md
04 - Backend/Domain.md
04 - Backend/Application.md
04 - Backend/API.md
04 - Backend/Data.md
04 - Backend/Worker.md
04 - Backend/Contracts.md
04 - Backend/Shared Kernel.md
```

---

# 132. Filosofia Final

O projeto Infrastructure deverá transformar abstrações internas em integrações concretas.

Seu código deverá expressar ações como:

```text
Gerar texto com provider
Publicar mensagem
Consultar cache
Armazenar arquivo
Resolver secret
Acionar automação
Chamar serviço externo
Validar assinatura
```

Ele não deverá expressar ações como:

```text
Concluir PipelineExecution
Aprovar Artifact
Persistir Aggregate no PostgreSQL
Executar migration
Decidir regra de retry de negócio
Autorizar usuário
Mapear resposta HTTP
```

Essas responsabilidades pertencem a outras camadas.

A regra principal será:

> Infrastructure executa detalhes externos substituíveis, sem controlar o negócio e sem possuir persistência relacional.

Quando esse limite for respeitado, providers, brokers, caches, storages e serviços externos poderão mudar sem contaminar o Domain ou a Application do Infinite Content AI.