# Providers e Integrações

## Objetivo

Definir como o Infinite Content AI se integrará com provedores externos de inteligência artificial, publicação, armazenamento, mensageria, mídia e outros serviços.

A arquitetura deverá permitir substituir, combinar ou adicionar novos provedores sem alterar os casos de uso da aplicação.

Nenhum componente da camada Application deverá depender diretamente de SDKs, APIs ou modelos específicos de fornecedores externos.

---

# Princípio Fundamental

A aplicação depende de capacidades.

A Infrastructure depende de fornecedores.

Exemplo:

```text
Application
    ↓
IAiTextProvider
    ↓
Infrastructure
    ↓
OpenAiTextProvider
```

A Application sabe que precisa gerar texto.

Ela não precisa saber qual empresa, modelo ou SDK realizará essa tarefa.

---

# Provider versus Integração

## Provider

Um provider implementa uma capacidade utilizada pela aplicação.

Exemplos:

- Geração de texto.
    
- Geração de imagem.
    
- Síntese de voz.
    
- Transcrição.
    
- Renderização de vídeo.
    
- Publicação de conteúdo.
    
- Armazenamento de arquivos.
    

## Integração

Uma integração representa a comunicação técnica com um sistema externo.

Exemplos:

- OpenAI API.
    
- Gemini API.
    
- Anthropic API.
    
- YouTube API.
    
- Amazon S3.
    
- Redis.
    
- RabbitMQ.
    
- n8n.
    
- Serviço Python.
    

Um provider poderá utilizar uma ou mais integrações para cumprir sua responsabilidade.

---

# Localização das Abstrações

As interfaces utilizadas pelos casos de uso ficarão no projeto:

```text
InfiniteContent.Application
```

Estrutura sugerida:

```text
Application/
└── Abstractions/
    ├── Ai/
    │   ├── IAiTextProvider.cs
    │   ├── IAiImageProvider.cs
    │   ├── IAiVoiceProvider.cs
    │   ├── IAiTranscriptionProvider.cs
    │   └── IAiEmbeddingProvider.cs
    │
    ├── Publishing/
    │   └── IPublishingProvider.cs
    │
    ├── Storage/
    │   └── IFileStorage.cs
    │
    ├── Messaging/
    │   └── IMessagePublisher.cs
    │
    ├── Media/
    │   └── IVideoRenderingProvider.cs
    │
    ├── Webhooks/
    │   └── IWebhookPublisher.cs
    │
    └── Clock/
        └── IClock.cs
```

---

# Localização das Implementações

As implementações concretas ficarão no projeto:

```text
InfiniteContent.Infrastructure
```

Estrutura sugerida:

```text
Infrastructure/
├── Ai/
│   ├── OpenAI/
│   ├── Gemini/
│   ├── Anthropic/
│   ├── ElevenLabs/
│   └── Common/
│
├── Publishing/
│   ├── YouTube/
│   ├── TikTok/
│   └── Instagram/
│
├── Storage/
│   ├── S3/
│   ├── AzureBlob/
│   └── Local/
│
├── Messaging/
│   ├── RabbitMq/
│   └── AzureServiceBus/
│
├── Media/
│   ├── Ffmpeg/
│   └── PythonService/
│
├── N8n/
├── Webhooks/
├── Cache/
├── Observability/
└── DependencyInjection.cs
```

---

# Regra de Dependência

A Application nunca deverá referenciar:

- SDK da OpenAI.
    
- SDK do Google Gemini.
    
- SDK da Anthropic.
    
- SDK do YouTube.
    
- SDK da AWS.
    
- SDK do Azure.
    
- RabbitMQ.Client.
    
- Redis.
    
- FFmpeg.
    
- Clientes HTTP concretos.
    
- Tipos específicos de fornecedores.
    

A Infrastructure poderá depender dessas bibliotecas.

---

# Categorias de Providers

O sistema poderá possuir as seguintes categorias:

- Texto.
    
- Imagem.
    
- Voz.
    
- Transcrição.
    
- Embeddings.
    
- Moderação.
    
- Pesquisa.
    
- Vídeo.
    
- Storage.
    
- Publicação.
    
- Mensageria.
    
- Cache.
    
- Webhooks.
    
- Analytics.
    
- Notificações.
    

---

# Provider de Texto

## Responsabilidade

Gerar, revisar, resumir, classificar ou transformar texto.

Exemplos de uso:

- Criar roteiro.
    
- Revisar conteúdo.
    
- Gerar título.
    
- Criar descrição.
    
- Traduzir.
    
- Resumir pesquisa.
    
- Extrair informações.
    
- Classificar intenção.
    
- Produzir saída estruturada.
    

---

# Contrato de Texto

Exemplo conceitual:

```csharp
public interface IAiTextProvider
{
    Task<Result<AiTextResponse>> GenerateAsync(
        AiTextRequest request,
        CancellationToken cancellationToken);
}
```

---

# Request de Texto

```csharp
public sealed record AiTextRequest(
    string SystemPrompt,
    string UserPrompt,
    string? Model,
    decimal Temperature,
    int? MaximumOutputTokens,
    AiResponseFormat ResponseFormat,
    IReadOnlyDictionary<string, string>? Metadata);
```

A request poderá conter:

- Prompt de sistema.
    
- Prompt do usuário.
    
- Modelo preferencial.
    
- Temperatura.
    
- Limite de tokens.
    
- Formato esperado.
    
- Schema da resposta.
    
- Metadados.
    
- Identificador do prompt.
    
- Versão do prompt.
    
- Limite de custo.
    
- Correlation ID.
    

---

# Response de Texto

```csharp
public sealed record AiTextResponse(
    string Content,
    string Provider,
    string Model,
    int InputTokens,
    int OutputTokens,
    decimal EstimatedCost,
    TimeSpan Duration,
    string? FinishReason);
```

A resposta deverá normalizar diferenças entre fornecedores.

Ela poderá conter:

- Conteúdo.
    
- Provider.
    
- Modelo.
    
- Tokens de entrada.
    
- Tokens de saída.
    
- Custo estimado.
    
- Duração.
    
- Motivo de finalização.
    
- Identificador externo.
    
- Avisos.
    
- Metadados.
    

---

# Saída Estruturada

Sempre que possível, a geração deverá utilizar saída estruturada.

Exemplo conceitual:

```csharp
public sealed record ScriptGenerationResponse(
    string Title,
    string Hook,
    IReadOnlyCollection<ScriptSection> Sections,
    string CallToAction);
```

O provider deverá retornar um formato que possa ser validado e desserializado.

Fluxo:

```text
Prompt
    ↓
Provider
    ↓
JSON
    ↓
Schema Validation
    ↓
Objeto Tipado
```

Saídas inválidas deverão produzir falha controlada.

---

# Provider de Imagem

## Responsabilidade

Gerar ou editar imagens utilizadas no conteúdo.

Exemplos:

- Thumbnail.
    
- Ilustração.
    
- Fundo.
    
- Capa.
    
- Variação visual.
    
- Asset para vídeo.
    

Contrato conceitual:

```csharp
public interface IAiImageProvider
{
    Task<Result<AiImageResponse>> GenerateAsync(
        AiImageRequest request,
        CancellationToken cancellationToken);
}
```

---

# Request de Imagem

Poderá conter:

- Prompt.
    
- Prompt negativo.
    
- Dimensões.
    
- Proporção.
    
- Formato.
    
- Quantidade de imagens.
    
- Identidade visual.
    
- Imagem de referência.
    
- Estilo.
    
- Qualidade.
    
- Limite de custo.
    

---

# Response de Imagem

Poderá conter:

- Arquivos gerados.
    
- URLs temporárias.
    
- Provider.
    
- Modelo.
    
- Dimensões.
    
- Formato.
    
- Custo.
    
- Duração.
    
- Seed, quando disponível.
    
- Metadados.
    

A imagem deverá ser copiada para o storage oficial do sistema.

URLs temporárias de providers não deverão ser tratadas como permanentes.

---

# Provider de Voz

## Responsabilidade

Transformar texto em áudio.

Contrato conceitual:

```csharp
public interface IAiVoiceProvider
{
    Task<Result<VoiceGenerationResponse>> GenerateAsync(
        VoiceGenerationRequest request,
        CancellationToken cancellationToken);
}
```

A request poderá conter:

- Texto.
    
- Idioma.
    
- Identificador da voz.
    
- Velocidade.
    
- Estabilidade.
    
- Entonação.
    
- Formato.
    
- Sample rate.
    
- Limite de custo.
    

A resposta poderá conter:

- Arquivo de áudio.
    
- Duração.
    
- Provider.
    
- Modelo.
    
- Voz utilizada.
    
- Custo.
    
- Marcadores de tempo.
    
- Metadados.
    

---

# Provider de Transcrição

## Responsabilidade

Converter áudio ou vídeo em texto.

Contrato conceitual:

```csharp
public interface IAiTranscriptionProvider
{
    Task<Result<TranscriptionResponse>> TranscribeAsync(
        TranscriptionRequest request,
        CancellationToken cancellationToken);
}
```

A resposta poderá conter:

- Texto completo.
    
- Segmentos.
    
- Timestamps.
    
- Idioma detectado.
    
- Confiança.
    
- Identificação de locutores.
    
- Custo.
    
- Duração.
    

---

# Provider de Embeddings

## Responsabilidade

Gerar representações vetoriais de conteúdo.

Possíveis usos:

- Busca semântica.
    
- Recuperação de contexto.
    
- Agrupamento de conteúdo.
    
- Detecção de similaridade.
    
- Recomendação.
    
- Memória de agentes.
    
- RAG.
    

Contrato conceitual:

```csharp
public interface IAiEmbeddingProvider
{
    Task<Result<EmbeddingResponse>> GenerateAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken);
}
```

---

# Provider de Moderação

## Responsabilidade

Avaliar riscos e conformidade de conteúdos.

Exemplos:

- Conteúdo ofensivo.
    
- Violência.
    
- Discurso de ódio.
    
- Conteúdo sexual.
    
- Risco legal.
    
- Temas sensíveis.
    
- Políticas das plataformas.
    

Contrato conceitual:

```csharp
public interface IContentModerationProvider
{
    Task<Result<ModerationResponse>> EvaluateAsync(
        ModerationRequest request,
        CancellationToken cancellationToken);
}
```

A moderação automática não substitui regras internas nem aprovação humana quando necessária.

---

# Provider de Pesquisa

## Responsabilidade

Obter informações externas para alimentar o Research Agent.

Exemplos:

- Busca web.
    
- Notícias.
    
- Tendências.
    
- Dados públicos.
    
- Fontes especializadas.
    

Contrato conceitual:

```csharp
public interface ISearchProvider
{
    Task<Result<SearchResponse>> SearchAsync(
        SearchRequest request,
        CancellationToken cancellationToken);
}
```

A resposta deverá incluir fontes e metadados suficientes para rastreabilidade.

---

# Provider de Renderização de Vídeo

## Responsabilidade

Criar o vídeo final a partir dos assets produzidos.

Contrato conceitual:

```csharp
public interface IVideoRenderingProvider
{
    Task<Result<VideoRenderResponse>> RenderAsync(
        VideoRenderRequest request,
        CancellationToken cancellationToken);
}
```

A implementação poderá utilizar:

- FFmpeg local.
    
- Serviço Python.
    
- Serviço externo.
    
- Worker especializado.
    
- Plataforma de renderização.
    

---

# Renderização Assíncrona

Renderizações longas poderão utilizar o modelo de job.

Exemplo:

```text
Application
    ↓
StartVideoRender
    ↓
Provider retorna JobId
    ↓
Worker acompanha o job
    ↓
Webhook ou polling
    ↓
VideoRenderedEvent
```

Contrato conceitual:

```csharp
public interface IAsyncVideoRenderingProvider
{
    Task<Result<ExternalJobReference>> StartAsync(
        VideoRenderRequest request,
        CancellationToken cancellationToken);

    Task<Result<ExternalJobStatus>> GetStatusAsync(
        string externalJobId,
        CancellationToken cancellationToken);
}
```

---

# Provider de Storage

## Responsabilidade

Armazenar e recuperar arquivos.

Contrato conceitual:

```csharp
public interface IFileStorage
{
    Task<Result<FileReference>> SaveAsync(
        Stream content,
        FileUploadRequest request,
        CancellationToken cancellationToken);

    Task<Result<Stream>> OpenReadAsync(
        string fileId,
        CancellationToken cancellationToken);

    Task<Result> DeleteAsync(
        string fileId,
        CancellationToken cancellationToken);
}
```

---

# Tipos de Arquivo

O storage poderá armazenar:

- Imagens.
    
- Áudios.
    
- Vídeos.
    
- Legendas.
    
- Roteiros.
    
- Arquivos temporários.
    
- Assets de projeto.
    
- Relatórios.
    
- Exportações.
    

---

# Referência de Arquivo

A aplicação deverá trabalhar com uma referência interna.

Exemplo:

```csharp
public sealed record FileReference(
    string Id,
    string FileName,
    string ContentType,
    long Size,
    string StorageProvider,
    string StorageKey);
```

A aplicação não deverá depender diretamente de URLs específicas de S3 ou Azure Blob.

---

# Provider de Publicação

## Responsabilidade

Publicar, atualizar, agendar ou remover conteúdos em plataformas externas.

Contrato conceitual:

```csharp
public interface IPublishingProvider
{
    string Platform { get; }

    Task<Result<PublicationResponse>> PublishAsync(
        PublicationRequest request,
        CancellationToken cancellationToken);
}
```

---

# Operações de Publicação

Um provider de publicação poderá implementar:

- Publicar.
    
- Agendar.
    
- Atualizar metadados.
    
- Consultar status.
    
- Remover.
    
- Alterar privacidade.
    
- Obter métricas básicas.
    

Interfaces poderão ser separadas caso a plataforma não suporte todas as operações.

---

# Capability Interfaces

Evitar uma interface gigante para todas as plataformas.

Preferir capacidades específicas.

Exemplo:

```csharp
public interface IContentPublisher
{
    Task<Result<PublicationResponse>> PublishAsync(
        PublicationRequest request,
        CancellationToken cancellationToken);
}
```

```csharp
public interface IScheduledPublisher
{
    Task<Result<PublicationResponse>> ScheduleAsync(
        ScheduledPublicationRequest request,
        CancellationToken cancellationToken);
}
```

```csharp
public interface IPublicationAnalyticsProvider
{
    Task<Result<PublicationMetricsResponse>> GetMetricsAsync(
        PublicationMetricsRequest request,
        CancellationToken cancellationToken);
}
```

---

# Provider de Mensageria

## Responsabilidade

Publicar mensagens e eventos no broker.

Contrato conceitual:

```csharp
public interface IMessagePublisher
{
    Task PublishAsync<T>(
        T message,
        MessageContext context,
        CancellationToken cancellationToken);
}
```

A Application não deverá conhecer RabbitMQ, Kafka, SQS ou Azure Service Bus diretamente.

---

# Provider de Cache

## Responsabilidade

Armazenar dados temporários e acelerar consultas.

Possíveis usos:

- Configuração de providers.
    
- Catálogo de modelos.
    
- Tokens de autenticação.
    
- Dados de tendências.
    
- Resultados temporários.
    
- Rate limits.
    

Contrato conceitual:

```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken);

    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration,
        CancellationToken cancellationToken);

    Task RemoveAsync(
        string key,
        CancellationToken cancellationToken);
}
```

Cache não será fonte oficial de verdade.

---

# Provider de Webhook

## Responsabilidade

Enviar notificações para sistemas externos.

Contrato conceitual:

```csharp
public interface IWebhookPublisher
{
    Task<Result<WebhookDeliveryResult>> SendAsync(
        WebhookMessage message,
        CancellationToken cancellationToken);
}
```

Deverá oferecer:

- Assinatura.
    
- Timeout.
    
- Retentativa.
    
- Logs.
    
- Idempotência.
    
- Registro de entrega.
    
- Tratamento de falha.
    

---

# Integração com n8n

A integração com n8n ficará na Infrastructure.

Exemplo:

```text
Infrastructure/
└── N8n/
    ├── N8nClient.cs
    ├── N8nOptions.cs
    ├── N8nWebhookPublisher.cs
    └── DependencyInjection.cs
```

O n8n poderá:

- Disparar pipelines.
    
- Receber eventos.
    
- Enviar notificações.
    
- Executar agendamentos.
    
- Integrar ferramentas externas.
    

A aplicação continuará sendo a fonte de verdade.

---

# Seleção de Provider

O sistema poderá possuir múltiplos providers para a mesma capacidade.

Exemplo:

```text
IAiTextProvider
├── OpenAiTextProvider
├── GeminiTextProvider
└── AnthropicTextProvider
```

A escolha não deverá ocorrer dentro do agente por meio de condicionais fixas.

Evitar:

```csharp
if (provider == "OpenAI")
{
    // ...
}
else if (provider == "Gemini")
{
    // ...
}
```

Preferir uma estratégia de seleção.

---

# Provider Resolver

Exemplo conceitual:

```csharp
public interface IAiTextProviderResolver
{
    IAiTextProvider Resolve(string providerName);
}
```

Implementação conceitual:

```csharp
public sealed class AiTextProviderResolver
    : IAiTextProviderResolver
{
    private readonly IReadOnlyDictionary<string, IAiTextProvider> _providers;

    public AiTextProviderResolver(
        IEnumerable<IAiTextProvider> providers)
    {
        _providers = providers.ToDictionary(
            provider => provider.Name,
            StringComparer.OrdinalIgnoreCase);
    }

    public IAiTextProvider Resolve(string providerName)
    {
        if (_providers.TryGetValue(providerName, out var provider))
        {
            return provider;
        }

        throw new ProviderNotFoundException(providerName);
    }
}
```

---

# Estratégia de Seleção

A seleção poderá considerar:

- Tipo de tarefa.
    
- Qualidade.
    
- Custo.
    
- Latência.
    
- Disponibilidade.
    
- Context window.
    
- Capacidade multimodal.
    
- Idioma.
    
- Região.
    
- Política do projeto.
    
- Preferência do usuário.
    
- Histórico de desempenho.
    
- Limites da conta.
    

---

# Provider Policy

Exemplo conceitual:

```csharp
public sealed record ProviderPolicy(
    string Capability,
    IReadOnlyCollection<string> PreferredProviders,
    decimal MaximumCost,
    TimeSpan MaximumLatency,
    bool AllowFallback);
```

---

# Catálogo de Modelos

O sistema deverá possuir um catálogo normalizado de modelos.

Exemplo:

```text
ProviderModel
├── Provider
├── ExternalModelId
├── Capability
├── ContextWindow
├── MaximumOutputTokens
├── SupportsStructuredOutput
├── SupportsImages
├── SupportsAudio
├── InputCost
├── OutputCost
├── Enabled
└── Deprecated
```

O código não deverá espalhar nomes de modelos em strings fixas.

Evitar:

```csharp
"gpt-x"
"gemini-x"
"claude-x"
```

Esses identificadores deverão vir de configuração ou catálogo persistido.

---

# Modelo Lógico

A Application poderá utilizar um identificador lógico.

Exemplo:

```text
HighQualityText
BalancedText
LowCostText
FastText
ImageGeneration
VoiceNarration
```

A Infrastructure ou uma política resolverá o modelo externo real.

Fluxo:

```text
Script Agent
    ↓
HighQualityText
    ↓
Provider Selection
    ↓
Modelo externo configurado
```

---

# Estratégias de Execução

O sistema poderá disponibilizar perfis.

## Economy

Prioriza:

- Menor custo.
    
- Modelos leves.
    
- Menos tentativas.
    
- Menor resolução.
    
- Menor quantidade de variações.
    

## Balanced

Prioriza equilíbrio entre:

- Custo.
    
- Qualidade.
    
- Latência.
    

## Quality

Prioriza:

- Modelos mais capazes.
    
- Mais validações.
    
- Mais variações.
    
- Revisão adicional.
    
- Maior resolução.
    

## Fast

Prioriza:

- Baixa latência.
    
- Resposta rápida.
    
- Modelos rápidos.
    
- Menos etapas opcionais.
    

---

# Fallback

Quando um provider falhar, outro poderá ser utilizado.

Exemplo:

```text
OpenAI
    ↓ falhou
Gemini
    ↓ falhou
Anthropic
    ↓ falha controlada
```

O fallback deverá ser configurável.

---

# Regras de Fallback

Antes de trocar de provider, o sistema deverá avaliar:

- O erro é transitório?
    
- O provider alternativo suporta a capacidade?
    
- O modelo alternativo aceita o mesmo formato?
    
- O custo continua dentro do limite?
    
- A qualidade mínima será respeitada?
    
- A região permite o uso?
    
- O provider está habilitado?
    
- A operação é idempotente?
    
- A troca pode alterar significativamente o resultado?
    

---

# Tipos de Fallback

## Mesmo Provider, Mesmo Modelo

Nova tentativa após erro transitório.

## Mesmo Provider, Outro Modelo

Utiliza outro modelo compatível.

## Outro Provider

Troca completamente de fornecedor.

## Estratégia Degradada

Reduz qualidade, resolução ou tamanho da saída.

## Intervenção Humana

Interrompe o fluxo e solicita decisão.

---

# Exemplo de Política

```text
Task: ScriptGeneration
Primary: OpenAI / HighQualityText
Fallback 1: Anthropic / HighQualityText
Fallback 2: Gemini / BalancedText
MaximumAttempts: 3
MaximumCost: 0.20
```

---

# Retentativas

Retentativas serão aplicadas apenas para falhas transitórias.

Exemplos:

- Timeout.
    
- HTTP 429.
    
- HTTP 502.
    
- HTTP 503.
    
- Falha temporária de rede.
    
- Indisponibilidade momentânea.
    

Não repetir automaticamente para:

- Credencial inválida.
    
- Entrada inválida.
    
- Conteúdo bloqueado.
    
- Modelo inexistente.
    
- Limite financeiro excedido.
    
- Schema incompatível.
    

---

# Backoff

Retentativas deverão utilizar atraso progressivo.

Exemplo:

```text
Tentativa 1: imediata
Tentativa 2: 2 segundos
Tentativa 3: 10 segundos
Tentativa 4: 30 segundos
```

Adicionar jitter quando necessário para evitar múltiplas tentativas simultâneas.

---

# Timeout

Toda integração externa deverá possuir timeout.

Exemplos:

```text
TextGeneration: 90 segundos
ImageGeneration: 5 minutos
VoiceGeneration: 5 minutos
VideoRendering: 30 minutos
Publishing: 2 minutos
Webhook: 15 segundos
```

Timeouts deverão ser configuráveis.

---

# Circuit Breaker

Providers instáveis poderão utilizar circuit breaker.

Fluxo:

```text
Muitas falhas consecutivas
    ↓
Circuito aberto
    ↓
Provider temporariamente indisponível
    ↓
Fallback utilizado
    ↓
Período de espera
    ↓
Teste de recuperação
```

O circuit breaker evita sobrecarregar um serviço já indisponível.

---

# Rate Limiting

Cada provider poderá possuir limites próprios.

Exemplos:

- Requisições por minuto.
    
- Tokens por minuto.
    
- Jobs simultâneos.
    
- Uploads por dia.
    
- Cotas financeiras.
    

O sistema deverá controlar esses limites para reduzir erros.

---

# Controle de Concorrência

Operações caras poderão possuir limites de execução simultânea.

Exemplo:

```text
TextGeneration: até 20 simultâneas
ImageGeneration: até 5 simultâneas
VideoRendering: até 2 simultâneas
Publishing: até 3 por canal
```

Esses valores deverão ser configuráveis.

---

# Controle de Custos

Toda chamada com custo deverá registrar:

- Provider.
    
- Modelo.
    
- Operação.
    
- Tokens.
    
- Unidade cobrada.
    
- Custo estimado.
    
- Custo final, quando disponível.
    
- Projeto.
    
- Pipeline.
    
- Agente.
    
- Etapa.
    
- Usuário ou organização.
    
- Data.
    
- Moeda.
    

---

# Estimativa Antes da Execução

Quando possível, o sistema deverá estimar o custo antes da chamada.

Exemplo:

```text
InputTokens estimados
    ×
Preço de entrada
    +
OutputTokens máximos
    ×
Preço de saída
```

Se o custo estimado ultrapassar o limite:

- Bloquear.
    
- Solicitar aprovação.
    
- Escolher modelo mais barato.
    
- Reduzir tamanho da entrada.
    
- Reduzir tamanho da saída.
    

---

# Registro de Uso

Entidade possível:

```text
ProviderUsage
```

Campos sugeridos:

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
    
- Duration.
    
- Status.
    
- OccurredAt.
    
- ExternalRequestId.
    
- CorrelationId.
    

---

# Normalização de Erros

Cada fornecedor retorna erros diferentes.

A Infrastructure deverá converter esses erros para categorias internas.

Exemplo:

```csharp
public enum ProviderErrorType
{
    Unknown,
    Timeout,
    RateLimitExceeded,
    AuthenticationFailed,
    InvalidRequest,
    ContentRejected,
    ModelUnavailable,
    ProviderUnavailable,
    QuotaExceeded,
    ResponseValidationFailed
}
```

---

# Provider Error

```csharp
public sealed record ProviderError(
    ProviderErrorType Type,
    string Code,
    string Message,
    bool IsTransient,
    string Provider,
    string? ExternalRequestId);
```

A Application deverá trabalhar com erros normalizados.

---

# Validação de Respostas

Nenhuma resposta externa deverá ser considerada confiável automaticamente.

As respostas deverão ser verificadas quanto a:

- Formato.
    
- Schema.
    
- Campos obrigatórios.
    
- Tamanho.
    
- Conteúdo vazio.
    
- Dados inválidos.
    
- URLs.
    
- Arquivos.
    
- MIME type.
    
- Limites.
    
- Segurança.
    
- Regras editoriais.
    

---

# Respostas Parciais

Alguns providers poderão retornar respostas incompletas.

O sistema deverá detectar:

- Conteúdo truncado.
    
- JSON incompleto.
    
- Arquivo corrompido.
    
- Job sem resultado.
    
- Resposta vazia.
    
- Término por limite de tokens.
    

O sistema poderá:

- Solicitar continuação.
    
- Repetir a chamada.
    
- Corrigir a saída.
    
- Utilizar fallback.
    
- Marcar como falha.
    

---

# Prompt Adaptation

Cada provider poderá exigir pequenas adaptações técnicas.

Exemplos:

- Formato de mensagem.
    
- Declaração de schema.
    
- Limites de tamanho.
    
- Nomes de parâmetros.
    
- Suporte a system prompt.
    
- Suporte a ferramentas.
    
- Suporte multimodal.
    

Essas adaptações deverão ficar na Infrastructure.

A regra de negócio do prompt continuará na Application ou no sistema de prompts.

---

# Anti-Corruption Layer

Cada integração deverá atuar como uma camada anticorrupção.

Ela deverá impedir que modelos externos contaminem o domínio.

Exemplo:

```text
YouTubeVideoResponse
    ↓
YouTubePublishingProvider
    ↓
PublicationResponse
```

A Application recebe um modelo interno normalizado.

Ela não recebe diretamente objetos do SDK do YouTube.

---

# Mapeamento

Cada implementação poderá possuir mapeadores próprios.

Exemplo:

```text
Infrastructure/
└── Publishing/
    └── YouTube/
        ├── YouTubePublishingProvider.cs
        ├── YouTubeRequestMapper.cs
        ├── YouTubeResponseMapper.cs
        └── YouTubeOptions.cs
```

---

# Configuração

Cada provider deverá possuir uma classe de opções.

Exemplo:

```csharp
public sealed class OpenAiOptions
{
    public const string SectionName = "Providers:OpenAI";

    public required string ApiKey { get; init; }

    public string BaseUrl { get; init; } = string.Empty;

    public string DefaultTextModel { get; init; } = string.Empty;

    public int TimeoutSeconds { get; init; } = 90;
}
```

Registro conceitual:

```csharp
services
    .AddOptions<OpenAiOptions>()
    .BindConfiguration(OpenAiOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

---

# Configuração Exemplo

```json
{
  "Providers": {
    "OpenAI": {
      "DefaultTextModel": "configured-model",
      "TimeoutSeconds": 90,
      "Enabled": true
    },
    "Gemini": {
      "DefaultTextModel": "configured-model",
      "TimeoutSeconds": 90,
      "Enabled": true
    }
  }
}
```

Segredos não deverão ficar no arquivo versionado.

---

# Gestão de Segredos

Segredos deverão ser armazenados em:

- Variáveis de ambiente.
    
- Secret Manager.
    
- Azure Key Vault.
    
- AWS Secrets Manager.
    
- Docker Secrets.
    
- Cofre equivalente.
    

Nunca registrar em logs:

- API keys.
    
- Access tokens.
    
- Refresh tokens.
    
- Senhas.
    
- Assinaturas privadas.
    
- Credenciais completas.
    

---

# Autenticação de Plataformas

Integrações como YouTube poderão utilizar OAuth.

O sistema deverá controlar:

- Access token.
    
- Refresh token.
    
- Expiração.
    
- Escopos.
    
- Conta conectada.
    
- Revogação.
    
- Renovação.
    
- Falhas de autenticação.
    

Tokens deverão ser criptografados em repouso.

---

# Health Checks

Providers importantes deverão possuir health checks quando viável.

Exemplos:

- Storage acessível.
    
- Broker acessível.
    
- Banco acessível.
    
- Serviço Python disponível.
    
- Redis disponível.
    

APIs externas pagas não deverão ser chamadas excessivamente apenas para health check.

---

# Tipos de Health Check

## Liveness

Verifica se o processo está vivo.

## Readiness

Verifica se o processo está pronto para trabalhar.

## Dependency Health

Verifica dependências essenciais.

Dependências opcionais não deverão necessariamente tornar toda a aplicação indisponível.

---

# Observabilidade

Toda chamada externa deverá registrar:

- Provider.
    
- Operação.
    
- Modelo.
    
- Duração.
    
- Status.
    
- Número de tentativas.
    
- Fallback utilizado.
    
- Custo.
    
- Unidades consumidas.
    
- Correlation ID.
    
- External Request ID.
    
- Código de erro.
    

---

# Logs

Exemplo conceitual:

```text
AI text generation completed
Provider: OpenAI
Model: configured-model
DurationMs: 3240
InputTokens: 1520
OutputTokens: 840
EstimatedCost: 0.04
CorrelationId: ABC
```

Prompts e respostas completas não deverão ser registrados indiscriminadamente.

---

# Privacidade de Logs

O registro de prompts e respostas deverá considerar:

- Dados pessoais.
    
- Conteúdo sensível.
    
- Segredos.
    
- Propriedade intelectual.
    
- Regras de retenção.
    
- Ambiente.
    
- Necessidade de auditoria.
    

Quando necessário, os dados deverão ser:

- Mascarados.
    
- Reduzidos.
    
- Criptografados.
    
- Armazenados separadamente.
    
- Retidos por tempo limitado.
    

---

# Métricas

Métricas por provider:

- Número de chamadas.
    
- Taxa de sucesso.
    
- Taxa de erro.
    
- Taxa de timeout.
    
- Latência média.
    
- Latência por percentil.
    
- Custo médio.
    
- Tokens consumidos.
    
- Número de fallbacks.
    
- Número de retentativas.
    
- Respostas inválidas.
    
- Disponibilidade.
    
- Uso por modelo.
    

---

# Avaliação de Qualidade

Providers não deverão ser avaliados apenas por custo e velocidade.

Também deverão ser avaliados por:

- Qualidade da saída.
    
- Taxa de aprovação.
    
- Necessidade de revisão.
    
- Aderência ao schema.
    
- Alucinações.
    
- Consistência.
    
- Desempenho por idioma.
    
- Desempenho por tipo de tarefa.
    

---

# Provider Performance

O sistema poderá manter estatísticas como:

```text
ProviderPerformance
├── Provider
├── Model
├── Capability
├── TaskType
├── SuccessRate
├── AverageLatency
├── AverageCost
├── AverageQualityScore
└── LastUpdatedAt
```

Esses dados poderão influenciar a seleção automática.

---

# Testes

## Testes Unitários

Devem validar:

- Mapeamento de requests.
    
- Mapeamento de responses.
    
- Normalização de erros.
    
- Resolução de providers.
    
- Seleção de fallback.
    
- Cálculo de custo.
    
- Validação de configuração.
    
- Adaptação de prompts.
    

---

## Testes de Integração

Devem validar:

- Cliente HTTP.
    
- Serialização.
    
- Autenticação.
    
- Timeouts.
    
- Retentativas.
    
- Storage.
    
- Broker.
    
- Webhooks.
    
- Serviço Python.
    
- Respostas simuladas.
    

---

## Contract Tests

Integrações importantes deverão possuir testes de contrato.

Esses testes deverão detectar:

- Mudanças no schema.
    
- Campos removidos.
    
- Respostas inesperadas.
    
- Incompatibilidades.
    
- Alterações de autenticação.
    

---

# Test Doubles

A Application deverá poder utilizar providers falsos ou simulados.

Exemplo:

```csharp
public sealed class FakeAiTextProvider : IAiTextProvider
{
    public Task<Result<AiTextResponse>> GenerateAsync(
        AiTextRequest request,
        CancellationToken cancellationToken)
    {
        var response = new AiTextResponse(
            "Generated test content",
            "Fake",
            "FakeModel",
            0,
            0,
            0,
            TimeSpan.Zero,
            "Completed");

        return Task.FromResult(Result.Success(response));
    }
}
```

Isso permitirá testes rápidos e previsíveis.

---

# Ambientes

Os providers poderão variar por ambiente.

## Development

- Providers falsos.
    
- Storage local.
    
- Broker local.
    
- Custos reduzidos.
    
- Modelos mais baratos.
    

## Staging

- Integrações reais controladas.
    
- Contas de teste.
    
- Limites baixos.
    
- Publicação privada.
    

## Production

- Providers oficiais.
    
- Credenciais protegidas.
    
- Monitoramento.
    
- Alertas.
    
- Limites financeiros.
    
- Auditoria.
    

---

# Feature Flags

Providers poderão ser habilitados ou desabilitados por feature flag.

Exemplos:

```text
EnableGeminiProvider
EnableAutomaticPublishing
EnablePythonVideoRenderer
EnableFallbackToAnthropic
```

Feature flags não deverão substituir regras permanentes de domínio.

---

# Degradação Graciosa

A indisponibilidade de um provider opcional não deverá derrubar todo o sistema.

Exemplo:

```text
Thumbnail provider indisponível
    ↓
Pipeline continua?
```

A resposta dependerá da política do pipeline.

Possibilidades:

- Usar fallback.
    
- Pular etapa opcional.
    
- Solicitar upload manual.
    
- Aguardar recuperação.
    
- Falhar de forma controlada.
    

---

# Adição de Novo Provider

Para adicionar um novo provider:

1. Identificar a capacidade existente.
    
2. Implementar a interface correspondente.
    
3. Criar opções de configuração.
    
4. Criar mapeadores.
    
5. Normalizar erros.
    
6. Adicionar observabilidade.
    
7. Registrar no DI.
    
8. Incluir no resolver.
    
9. Adicionar testes.
    
10. Documentar limitações e custos.
    

Os agentes e handlers existentes não deverão precisar ser alterados.

---

# Exemplo de Registro

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddAiProviders(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<OpenAiOptions>(
            configuration.GetSection(OpenAiOptions.SectionName));

        services.Configure<GeminiOptions>(
            configuration.GetSection(GeminiOptions.SectionName));

        services.AddHttpClient<OpenAiTextProvider>();
        services.AddHttpClient<GeminiTextProvider>();

        services.AddScoped<IAiTextProvider, OpenAiTextProvider>();
        services.AddScoped<IAiTextProvider, GeminiTextProvider>();

        services.AddScoped<IAiTextProviderResolver, AiTextProviderResolver>();

        return services;
    }
}
```

---

# Exemplo de Execução

```text
ScriptAgent
    ↓
Solicita HighQualityText
    ↓
ProviderSelector avalia política
    ↓
OpenAiTextProvider selecionado
    ↓
Chamada executada
    ↓
Resposta validada
    ↓
Uso e custo registrados
    ↓
Resultado devolvido ao agente
```

Em caso de falha:

```text
OpenAiTextProvider falha
    ↓
Erro normalizado
    ↓
Política avalia fallback
    ↓
AnthropicTextProvider selecionado
    ↓
Nova chamada
    ↓
Resultado registrado com fallback
```

---

# Regras Arquiteturais

- Application depende de abstrações.
    
- Infrastructure implementa integrações.
    
- Domain não conhece providers.
    
- Agentes não conhecem SDKs.
    
- Handlers não utilizam clientes HTTP diretamente.
    
- Nomes de modelos não devem ficar espalhados no código.
    
- Respostas externas devem ser normalizadas.
    
- Erros externos devem ser normalizados.
    
- Toda chamada externa deve possuir timeout.
    
- Retentativas devem ser limitadas.
    
- Fall­backs devem respeitar custo e capacidade.
    
- Toda operação paga deve registrar uso.
    
- Segredos não devem ser versionados.
    
- Tokens não devem aparecer em logs.
    
- URLs temporárias não são armazenamento permanente.
    
- Saídas de IA devem ser validadas.
    
- Providers devem ser substituíveis.
    
- Integrações devem possuir testes.
    
- SDKs externos não devem escapar da Infrastructure.
    
- Configurações devem ser validadas na inicialização.
    
- Operações externas longas devem ser assíncronas.
    
- Toda chamada deve possuir Correlation ID.
    

---

# Matriz de Providers

| Capacidade  | Interface                     | Possíveis implementações                |
| ----------- | ----------------------------- | --------------------------------------- |
| Texto       | IAiTextProvider               | OpenAI, Gemini, Anthropic               |
| Imagem      | IAiImageProvider              | OpenAI, Stability, outro serviço        |
| Voz         | IAiVoiceProvider              | ElevenLabs, Azure Speech, outro serviço |
| Transcrição | IAiTranscriptionProvider      | OpenAI, Azure Speech, serviço Python    |
| Embeddings  | IAiEmbeddingProvider          | OpenAI, Gemini, modelo local            |
| Moderação   | IContentModerationProvider    | Provider externo ou regras internas     |
| Pesquisa    | ISearchProvider               | Busca web, APIs especializadas          |
| Vídeo       | IVideoRenderingProvider       | FFmpeg, Python, serviço externo         |
| Storage     | IFileStorage                  | S3, Azure Blob, local                   |
| Publicação  | IContentPublisher             | YouTube, TikTok, Instagram              |
| Mensageria  | IMessagePublisher             | RabbitMQ, SQS, Azure Service Bus        |
| Cache       | ICacheService                 | Redis, memória                          |
| Webhook     | IWebhookPublisher             | Cliente HTTP resiliente                 |
| Analytics   | IPublicationAnalyticsProvider | YouTube, TikTok, Instagram              |

---

# Decisões Pendentes

As seguintes escolhas deverão ser registradas em ADRs:

- Provider principal de texto.
    
- Estratégia inicial de fallback.
    
- Provider de geração de imagens.
    
- Provider de voz.
    
- Tecnologia de storage.
    
- Tecnologia de mensageria.
    
- Estratégia de renderização de vídeo.
    
- Uso de FFmpeg local ou serviço Python.
    
- Forma de armazenamento dos preços dos modelos.
    
- Estratégia de rotação de credenciais.
    
- Política de logging de prompts.
    
- Estratégia de OAuth para plataformas.
    

---

# Objetivo Final

Criar uma camada de integrações desacoplada, resiliente, observável e orientada a capacidades.

O Infinite Content AI deverá poder trocar fornecedores, adicionar novos modelos e adaptar estratégias de custo e qualidade sem alterar o núcleo do produto.