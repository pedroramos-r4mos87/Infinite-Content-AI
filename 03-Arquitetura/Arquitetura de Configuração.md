# Arquitetura de Configuração

## Objetivo

Definir como o Infinite Content AI carregará, validará, organizará, versionará e aplicará configurações em todos os seus componentes.

A configuração deverá permitir que o sistema se adapte a diferentes ambientes sem exigir alterações no código.

Ela deverá ser:

- Explícita.
    
- Validada.
    
- Segura.
    
- Versionável.
    
- Observável.
    
- Separada por responsabilidade.
    
- Consistente entre API, Worker e serviços auxiliares.
    
- Compatível com múltiplos ambientes.
    
- Preparada para configuração dinâmica quando necessário.
    

O objetivo não é apenas ler valores de arquivos.

O objetivo é garantir que cada componente opere com configurações corretas, conhecidas e rastreáveis.

---

# Princípios

A arquitetura de configuração seguirá os seguintes princípios:

- Código não deverá conter valores específicos de ambiente.
    
- Segredos não deverão ser tratados como configuração comum.
    
- Configurações críticas deverão ser validadas na inicialização.
    
- Cada módulo deverá possuir sua própria seção de configuração.
    
- Configurações deverão utilizar tipos fortes.
    
- Defaults deverão ser explícitos.
    
- Precedência entre fontes deverá ser conhecida.
    
- Alterações críticas deverão ser auditáveis.
    
- Configuração dinâmica deverá ser utilizada apenas quando necessária.
    
- Feature flags não deverão substituir regras de negócio.
    
- Configurações deverão possuir limites válidos.
    
- Configurações sensíveis deverão ser protegidas.
    
- Ambientes não deverão compartilhar credenciais.
    
- A aplicação deverá falhar rapidamente quando não puder operar com segurança.
    

---

# O que é Configuração

Configuração representa valores que alteram o comportamento técnico ou operacional do sistema sem modificar o código.

Exemplos:

- Connection strings.
    
- Endpoints.
    
- Timeouts.
    
- Limites de retry.
    
- Tamanho de batch.
    
- Limites de concorrência.
    
- Feature flags.
    
- Modelos padrão.
    
- Providers habilitados.
    
- Tamanho máximo de upload.
    
- Retenção.
    
- Intervalos de jobs.
    
- Configuração de observabilidade.
    
- Políticas de cache.
    

---

# O que não é Configuração

Não deverá ser tratado como configuração técnica:

- Regras centrais de domínio.
    
- Estados de entidades.
    
- Permissões de usuários.
    
- Dados de organizações.
    
- Prompts versionados.
    
- Definições persistidas de pipelines.
    
- Limites comerciais por cliente.
    
- Preferências editoriais.
    
- Conteúdo de negócio.
    

Esses elementos pertencem ao domínio ou à persistência.

---

# Configuração versus Segredo

Configuração e segredo possuem ciclos de vida diferentes.

## Configuração

Exemplos:

```text
TimeoutSeconds
MaximumAttempts
DefaultModel
FeatureEnabled
BatchSize
```

Pode ser visível para operadores autorizados.

## Segredo

Exemplos:

```text
ApiKey
Password
ClientSecret
PrivateKey
RefreshToken
```

Deve permanecer protegido e possuir rotação.

---

# Regra Fundamental

A configuração poderá indicar onde um segredo está.

Ela não deverá necessariamente conter o segredo diretamente.

Exemplo:

```json
{
  "Providers": {
    "OpenAI": {
      "ApiKeySecretName": "prod-openai-api-key"
    }
  }
}
```

O valor real será resolvido por um Secret Manager.

---

# Fontes de Configuração

A aplicação poderá utilizar múltiplas fontes.

Ordem conceitual:

```text
Defaults de código
    ↓
appsettings.json
    ↓
appsettings.{Environment}.json
    ↓
Variáveis de ambiente
    ↓
Secret Manager
    ↓
Argumentos de linha de comando
    ↓
Configuração dinâmica
```

A fonte com maior precedência sobrescreve as anteriores.

---

# Precedência

A precedência deverá ser documentada e consistente.

Exemplo:

```text
appsettings.json
    <
appsettings.Production.json
    <
Environment Variables
    <
Command Line
```

Configurações dinâmicas poderão ter precedência apenas sobre campos explicitamente suportados.

---

# appsettings.json

O arquivo base deverá conter:

- Estrutura.
    
- Defaults seguros.
    
- Valores não sensíveis.
    
- Opções comuns entre ambientes.
    
- Seções necessárias.
    

Exemplo:

```json
{
  "Application": {
    "Name": "Infinite Content AI"
  },
  "Pipelines": {
    "DefaultTimeoutMinutes": 30
  },
  "Providers": {
    "OpenAI": {
      "Enabled": false
    }
  }
}
```

---

# appsettings por Ambiente

Arquivos específicos poderão existir:

```text
appsettings.Development.json
appsettings.Staging.json
appsettings.Production.json
```

Eles poderão alterar:

- Logging.
    
- Observabilidade.
    
- Endpoints.
    
- Timeouts.
    
- Cache.
    
- Feature flags.
    
- Integrações.
    

Eles não deverão conter segredos reais.

---

# Variáveis de Ambiente

Variáveis de ambiente serão utilizadas principalmente em:

- Containers.
    
- CI/CD.
    
- Produção.
    
- Secrets injection.
    
- Infraestrutura gerenciada.
    

Convenção .NET:

```text
Providers__OpenAI__Enabled=true
```

equivale a:

```json
{
  "Providers": {
    "OpenAI": {
      "Enabled": true
    }
  }
}
```

---

# Variáveis por Processo

API e Worker poderão possuir valores diferentes.

Exemplo:

```text
API
Pipelines__MaximumConcurrentExecutions=0

Worker
Pipelines__MaximumConcurrentExecutions=10
```

A configuração deverá refletir a responsabilidade de cada processo.

---

# Options Pattern

A aplicação deverá utilizar tipos fortes por meio do Options Pattern.

Exemplo:

```csharp
public sealed class PipelineOptions
{
    public const string SectionName = "Pipelines";

    public int DefaultTimeoutMinutes { get; init; }

    public int MaximumConcurrentExecutions { get; init; }

    public int MaximumAttempts { get; init; }
}
```

---

# Registro de Options

```csharp
services
    .AddOptions<PipelineOptions>()
    .BindConfiguration(PipelineOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

---

# Benefícios de Tipos Fortes

- IntelliSense.
    
- Validação.
    
- Refatoração segura.
    
- Testabilidade.
    
- Menor uso de strings mágicas.
    
- Documentação implícita.
    
- Separação por módulo.
    
- Detecção antecipada de erros.
    

---

# Proibição de IConfiguration Espalhado

`IConfiguration` não deverá ser injetado indiscriminadamente em qualquer classe.

Exemplo a evitar:

```csharp
public sealed class OpenAiProvider
{
    public OpenAiProvider(IConfiguration configuration)
    {
        var apiKey = configuration["Providers:OpenAI:ApiKey"];
    }
}
```

Preferir:

```csharp
public sealed class OpenAiProvider
{
    private readonly OpenAiOptions _options;

    public OpenAiProvider(
        IOptions<OpenAiOptions> options)
    {
        _options = options.Value;
    }
}
```

---

# IOptions

`IOptions<T>` será adequado quando a configuração:

- É carregada uma vez.
    
- Não muda durante a execução.
    
- É utilizada como singleton ou configuração estática.
    
- Não precisa de reload.
    

---

# IOptionsSnapshot

`IOptionsSnapshot<T>` poderá ser utilizado em aplicações web quando:

- A configuração pode mudar entre requests.
    
- O lifetime é scoped.
    
- O reload entre escopos é aceitável.
    

Não deverá ser utilizado automaticamente em Workers long-running sem avaliar o comportamento.

---

# IOptionsMonitor

`IOptionsMonitor<T>` será utilizado quando:

- Mudanças em runtime forem suportadas.
    
- Callbacks de alteração forem necessários.
    
- Serviços singleton precisarem ler valores atuais.
    
- O comportamento aceitar atualização dinâmica.
    

---

# Configuração Estática versus Dinâmica

## Configuração Estática

Carregada na inicialização.

Exemplos:

- Connection string.
    
- Broker.
    
- Secret Manager.
    
- Storage.
    
- Instrumentação.
    
- Algoritmos de segurança.
    
- Estrutura de filas.
    

Mudanças exigem restart ou deploy.

## Configuração Dinâmica

Pode mudar durante a execução.

Exemplos:

- Feature flags.
    
- Limites operacionais.
    
- Provider habilitado.
    
- Modelo padrão.
    
- Concorrência.
    
- Percentual de rollout.
    

---

# Regra para Configuração Dinâmica

Configuração dinâmica somente deverá ser adotada quando:

- O valor realmente precisa mudar sem deploy.
    
- A mudança é segura.
    
- Existe fallback.
    
- Existe auditoria.
    
- Existe validação.
    
- Existe estratégia de cache.
    
- Existe comportamento definido em caso de indisponibilidade.
    

---

# Validação

Toda configuração deverá ser validada.

Tipos de validação:

- Obrigatoriedade.
    
- Faixa.
    
- Formato.
    
- Relação entre campos.
    
- Compatibilidade.
    
- Dependência condicional.
    
- Segurança.
    
- Disponibilidade.
    

---

# Data Annotations

Exemplo:

```csharp
public sealed class MessagingOptions
{
    public const string SectionName = "Messaging";

    [Required]
    public string Host { get; init; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; init; }

    [Range(1, 1000)]
    public int PrefetchCount { get; init; }
}
```

---

# Validação Customizada

Exemplo:

```csharp
services
    .AddOptions<OpenAiOptions>()
    .BindConfiguration(OpenAiOptions.SectionName)
    .Validate(
        options =>
            !options.Enabled ||
            !string.IsNullOrWhiteSpace(options.ApiKeySecretName),
        "OpenAI secret reference is required when provider is enabled.")
    .ValidateOnStart();
```

---

# Validação entre Campos

Exemplo:

```text
MaximumAttempts >= 1
RetryDelaySeconds > 0
TotalTimeoutSeconds > RequestTimeoutSeconds
```

Outro exemplo:

```text
AutomaticPublishingEnabled = true
    exige
PublishingProviderEnabled = true
```

---

# Fail Fast

Configurações críticas inválidas deverão impedir o processo de iniciar.

Exemplos:

- Banco sem connection string.
    
- Broker obrigatório não configurado.
    
- Chave de assinatura ausente.
    
- Storage principal sem configuração.
    
- Provider marcado como obrigatório sem credencial.
    
- Porta inválida.
    
- Política inconsistente.
    

---

# Dependências Opcionais

Uma dependência opcional inválida não deverá necessariamente impedir toda a aplicação de iniciar.

Exemplo:

```text
Provider de imagens desabilitado
```

poderá apenas remover uma capacidade.

A criticidade deverá ser explícita.

---

# Configuração por Módulo

Cada módulo deverá possuir sua própria seção.

Exemplo:

```text
Application
Database
Messaging
Storage
Cache
Observability
Security
Pipelines
Providers
Workers
Webhooks
N8n
Media
```

---

# Estrutura Sugerida

```text
Infrastructure/
└── Configuration/
    ├── ApplicationOptions.cs
    ├── MessagingOptions.cs
    ├── StorageOptions.cs
    ├── ObservabilityOptions.cs
    ├── SecurityOptions.cs
    ├── Providers/
    │   ├── OpenAiOptions.cs
    │   ├── GeminiOptions.cs
    │   ├── AnthropicOptions.cs
    │   └── ElevenLabsOptions.cs
    ├── Validation/
    └── DependencyInjection.cs
```

---

# Configuração da Application

A Application poderá possuir opções para comportamento interno.

Exemplos:

- Limites de paginação.
    
- Tamanho máximo de batch lógico.
    
- Tempo padrão de aprovação.
    
- Limites de execução.
    
- Políticas gerais.
    

Essas opções deverão estar em Application quando forem consumidas apenas por ela.

---

# Configuração da Infrastructure

A Infrastructure será responsável por opções relacionadas a:

- Providers.
    
- Broker.
    
- Storage.
    
- Cache.
    
- n8n.
    
- HTTP clients.
    
- Observabilidade.
    
- Serviços externos.
    
- Secret Manager.
    

---

# Configuração do Data

O projeto Data será responsável por:

- Connection string.
    
- Timeout de comandos.
    
- Pooling.
    
- Migrations.
    
- Retry de conexão.
    
- Naming conventions.
    
- Health checks.
    
- Interceptors.
    

---

# Configuração do Worker

O Worker poderá possuir:

- Concorrência.
    
- Prefetch.
    
- Intervalos.
    
- Batch size.
    
- Shutdown timeout.
    
- Heartbeat.
    
- Lease duration.
    
- Polling interval.
    
- Jobs habilitados.
    

---

# Application Options

Exemplo:

```csharp
public sealed class ApplicationOptions
{
    public const string SectionName = "Application";

    public string Name { get; init; } = "Infinite Content AI";

    public string Environment { get; init; } = string.Empty;

    public string Version { get; init; } = string.Empty;
}
```

---

# Pipeline Options

Exemplo:

```csharp
public sealed class PipelineOptions
{
    public const string SectionName = "Pipelines";

    public int DefaultTimeoutMinutes { get; init; } = 30;

    public int MaximumAttempts { get; init; } = 3;

    public int LeaseDurationMinutes { get; init; } = 5;

    public int HeartbeatIntervalSeconds { get; init; } = 30;

    public int MaximumConcurrentExecutions { get; init; } = 10;
}
```

---

# Provider Options

Exemplo:

```csharp
public sealed class OpenAiOptions
{
    public const string SectionName = "Providers:OpenAI";

    public bool Enabled { get; init; }

    public string BaseUrl { get; init; } = string.Empty;

    public string ApiKeySecretName { get; init; } = string.Empty;

    public string DefaultTextModel { get; init; } = string.Empty;

    public int TimeoutSeconds { get; init; } = 90;

    public int MaximumAttempts { get; init; } = 3;
}
```

---

# HttpClient Configuration

Cada integração deverá possuir sua própria configuração.

Exemplos:

- BaseUrl.
    
- Timeout.
    
- Retry.
    
- Circuit breaker.
    
- User agent.
    
- Headers públicos.
    
- Tamanho máximo de resposta.
    

Segredos não deverão ser registrados.

---

# Named Options

Named Options poderão ser utilizados quando existirem múltiplas configurações do mesmo tipo.

Exemplo:

```text
Storage:Artifacts
Storage:Backups
Storage:Temporary
```

Ou:

```text
Providers:OpenAI:Text
Providers:OpenAI:Image
```

---

# Configuração Hierárquica

Exemplo:

```json
{
  "Providers": {
    "Defaults": {
      "TimeoutSeconds": 90,
      "MaximumAttempts": 3
    },
    "OpenAI": {
      "Enabled": true,
      "DefaultTextModel": "model-name"
    }
  }
}
```

A herança deverá ser resolvida explicitamente.

Evitar lógica implícita difícil de rastrear.

---

# Defaults

Defaults deverão ser:

- Seguros.
    
- Documentados.
    
- Conservadores.
    
- Testáveis.
    
- Compatíveis com o ambiente.
    

Exemplo:

```text
AutomaticPublishingEnabled = false
```

é um default mais seguro que:

```text
AutomaticPublishingEnabled = true
```

---

# Configuração Obrigatória

Uma configuração deverá ser obrigatória apenas quando realmente necessária.

Exemplo:

```text
OpenAI.Enabled = false
```

não deve exigir `ApiKeySecretName`.

Mas:

```text
OpenAI.Enabled = true
```

deve exigir.

---

# Configuração por Ambiente

## Development

Pode possuir:

- Logging detalhado.
    
- Providers fake.
    
- Storage local.
    
- Broker local.
    
- Banco local.
    
- Feature flags experimentais.
    
- Timeouts menores.
    
- Dados fictícios.
    

## Staging

Deverá se aproximar de produção.

Pode possuir:

- Providers sandbox.
    
- Sampling maior.
    
- Menor escala.
    
- Dados anonimizados.
    
- Testes sintéticos.
    

## Production

Deverá possuir:

- Segredos externos.
    
- Logging controlado.
    
- Timeouts reais.
    
- Limites rígidos.
    
- Observabilidade ativa.
    
- Feature flags aprovadas.
    
- Segurança reforçada.
    

---

# Ambiente como Valor Confiável

O ambiente deverá ser resolvido pela plataforma.

Exemplo:

```text
ASPNETCORE_ENVIRONMENT
DOTNET_ENVIRONMENT
```

A aplicação não deverá aceitar que um request escolha o ambiente.

---

# Feature Flags

Feature flags permitem alterar comportamento sem deploy completo.

Exemplos:

```text
EnableAutomaticPublishing
EnableGeminiFallback
EnableNewPipelineExecutor
EnableHighCostModels
EnableRealtimeProgress
```

---

# Tipos de Feature Flag

## Release Flag

Controla lançamento gradual.

## Experiment Flag

Controla experimentos.

## Operational Flag

Permite habilitar ou desabilitar comportamento operacional.

## Permission Flag

Não deverá substituir autorização.

## Kill Switch

Desativa rapidamente uma capacidade perigosa.

---

# Kill Switches

Exemplos:

```text
DisableAllPublishing
DisableProviderOpenAI
DisableVideoRendering
DisableExternalWebhooks
```

Kill switches deverão:

- Ser simples.
    
- Ser rápidos.
    
- Possuir acesso restrito.
    
- Gerar auditoria.
    
- Ser monitorados.
    
- Ter comportamento seguro.
    

---

# Feature Flags não são Permissões

Exemplo incorreto:

```text
EnableAdminArea = true
```

não substitui:

```text
Usuário possui permissão administrativa
```

Feature flag controla disponibilidade da funcionalidade.

Autorização controla quem pode usá-la.

---

# Feature Flags por Organização

Algumas flags poderão variar por organização.

Exemplos:

- Novo pipeline.
    
- Provider beta.
    
- Publicação automática.
    
- Recurso premium.
    
- Modelo experimental.
    

Essas flags pertencem à configuração de produto persistida, não necessariamente ao appsettings.

---

# Configuração Global versus por Organização

## Global

Exemplos:

- Timeout de infraestrutura.
    
- Limite máximo técnico.
    
- Endpoint de provider.
    
- Configuração de broker.
    
- Retenção de logs.
    

## Por Organização

Exemplos:

- Provider preferido.
    
- Limite mensal.
    
- Publicação automática.
    
- Idioma padrão.
    
- Modelos permitidos.
    
- Estratégia editorial.
    

Configuração por organização deverá ser persistida no banco e passar pela Application.

---

# Precedência por Organização

Uma política poderá seguir:

```text
Limite técnico global
    ↓
Plano comercial
    ↓
Configuração da organização
    ↓
Configuração do projeto
    ↓
Configuração da execução
```

A camada inferior não poderá ultrapassar limites superiores.

---

# Effective Configuration

O sistema deverá conseguir calcular e explicar a configuração efetiva.

Exemplo:

```text
Provider: OpenAI
Model: X
Origem: OrganizationSetting
Timeout: 90s
Origem: GlobalDefault
MaximumAttempts: 2
Origem: PipelineDefinition
```

Isso facilita diagnóstico.

---

# Configuration Resolver

Pode existir uma abstração para resolver políticas efetivas.

Exemplo:

```csharp
public interface IPipelineConfigurationResolver
{
    Task<PipelineExecutionConfiguration> ResolveAsync(
        Guid organizationId,
        Guid pipelineDefinitionId,
        CancellationToken cancellationToken);
}
```

---

# Snapshot de Configuração

Execuções longas deverão registrar um snapshot das configurações relevantes.

Motivo:

```text
Pipeline começa
    ↓
Configuração muda
    ↓
Pipeline termina com comportamento diferente
```

Para reprodutibilidade, registrar:

- Provider selecionado.
    
- Modelo.
    
- Timeouts.
    
- Retry.
    
- Política de fallback.
    
- Versão de pipeline.
    
- Feature flags relevantes.
    
- Limites de custo.
    

---

# Configuração por Execução

A execução poderá persistir:

```text
configuration_snapshot
configuration_version
resolved_at
```

O snapshot não deverá conter segredos.

---

# Alterações Durante Execução

A regra padrão será:

> Uma execução utiliza a configuração resolvida no início.

Exceções possíveis:

- Kill switch de segurança.
    
- Revogação de credencial.
    
- Suspensão de organização.
    
- Cancelamento.
    
- Limite financeiro global.
    

Essas mudanças podem interromper a execução.

---

# Versionamento de Configuração

Configurações persistidas deverão possuir versão.

Exemplos:

- ProviderPolicyVersion.
    
- PipelineConfigurationVersion.
    
- OrganizationSettingsVersion.
    
- FeatureFlagRevision.
    

Isso permite:

- Auditoria.
    
- Reprodutibilidade.
    
- Rollback.
    
- Comparação.
    
- Diagnóstico.
    

---

# Histórico de Alterações

Configurações críticas deverão registrar:

- Campo.
    
- Valor anterior.
    
- Valor novo.
    
- Autor.
    
- Horário.
    
- Motivo.
    
- Escopo.
    
- Versão.
    
- CorrelationId.
    

Valores secretos não deverão aparecer.

---

# Reload

Nem toda configuração deverá suportar reload.

## Pode Suportar

- Feature flags.
    
- Concorrência.
    
- Limites operacionais.
    
- Provider habilitado.
    
- Sampling.
    
- Alguns timeouts.
    

## Não Deve Suportar Automaticamente

- Connection string principal.
    
- Chaves de criptografia.
    
- Estrutura de filas.
    
- Identidade do serviço.
    
- Configuração de autenticação.
    
- Algoritmos de assinatura.
    

---

# OnChange

Exemplo:

```csharp
optionsMonitor.OnChange(options =>
{
    // Validar
    // Atualizar estado seguro
    // Registrar alteração
});
```

Callbacks deverão:

- Ser rápidos.
    
- Ser thread-safe.
    
- Não lançar exceções não tratadas.
    
- Registrar alterações.
    
- Manter último valor válido.
    

---

# Última Configuração Válida

Se uma configuração dinâmica nova for inválida, o sistema deverá:

- Rejeitar.
    
- Manter a última válida.
    
- Gerar alerta.
    
- Registrar erro.
    
- Não aplicar parcialmente.
    

---

# Configuração Parcial

Mudanças com múltiplos campos relacionados deverão ser aplicadas atomicamente.

Exemplo:

```text
Provider
Model
Fallback
CostLimit
```

não deverão ficar em combinações intermediárias inconsistentes.

---

# Cache de Configuração

Configuração remota poderá ser armazenada em cache.

O cache deverá possuir:

- TTL.
    
- Versão.
    
- Último valor válido.
    
- Invalidação.
    
- Métricas.
    
- Fallback.
    

---

# Indisponibilidade da Configuração Remota

A aplicação deverá possuir comportamento definido.

Possibilidades:

- Utilizar último valor válido.
    
- Utilizar default seguro.
    
- Desabilitar capacidade.
    
- Falhar startup.
    
- Pausar operações críticas.
    

A decisão dependerá da configuração.

---

# Configuração e Banco

Configurações técnicas não deverão ser colocadas automaticamente no PostgreSQL.

Utilizar banco quando:

- Precisam ser alteradas pelo produto.
    
- Variam por organização.
    
- Exigem histórico.
    
- Exigem autorização.
    
- Fazem parte do comportamento de negócio.
    

Utilizar appsettings ou plataforma quando:

- São específicas do deploy.
    
- São técnicas.
    
- São controladas por operações.
    
- Mudam junto da infraestrutura.
    

---

# Configuração e n8n

O n8n não deverá ser a fonte oficial de configuração do sistema.

Ele poderá receber configurações necessárias para um workflow, mas não deverá definir sozinho:

- Permissões.
    
- Limites globais.
    
- Estados de pipeline.
    
- Policies de segurança.
    
- Credenciais centrais.
    

---

# Configuração de Providers

Cada provider deverá possuir:

- Enabled.
    
- BaseUrl.
    
- SecretReference.
    
- Timeout.
    
- Retry.
    
- CircuitBreaker.
    
- DefaultModel.
    
- ModelAllowlist.
    
- CostRules.
    
- Region.
    
- HealthCheckPolicy.
    

---

# Model Catalog

Modelos disponíveis não deverão ficar espalhados em strings.

Poderá existir um catálogo persistido ou configurado.

Exemplo:

```text
ProviderModel
├── Provider
├── ModelId
├── Capability
├── Enabled
├── InputUnitCost
├── OutputUnitCost
├── ContextWindow
├── Region
└── DeprecatedAt
```

---

# Configuração de Custos

Custos de providers podem mudar.

Eles deverão ser atualizáveis sem recompilar.

Possibilidades:

- Tabela persistida.
    
- Arquivo versionado.
    
- Serviço de configuração.
    
- Atualização administrativa.
    

A origem e a data de validade deverão ser registradas.

---

# Configuração de Mensageria

Exemplos:

- Host.
    
- VirtualHost.
    
- Prefetch.
    
- Concurrency.
    
- Retry delays.
    
- Publisher confirm timeout.
    
- Queue names.
    
- Exchange names.
    
- DLQ.
    
- Batch size.
    

Nomes estruturais não deverão mudar livremente em runtime.

---

# Configuração de Jobs

Exemplos:

```text
OutboxPollingInterval
OutboxBatchSize
ReconciliationInterval
CleanupInterval
HeartbeatInterval
LeaseDuration
```

Esses valores deverão respeitar relações válidas.

Exemplo:

```text
HeartbeatInterval < LeaseDuration
```

---

# Configuração de Storage

Exemplos:

- Provider.
    
- Bucket.
    
- Container.
    
- Prefixo.
    
- Tamanho máximo.
    
- URL expiration.
    
- Retenção.
    
- Quarentena.
    
- Criptografia.
    
- Região.
    

---

# Configuração de Observabilidade

Exemplos:

- ServiceName.
    
- Sampling.
    
- Exporter.
    
- Endpoint.
    
- LogLevel.
    
- Retenção.
    
- Payload policy.
    
- Prompt logging policy.
    
- Health checks habilitados.
    

---

# Configuração de Segurança

Exemplos:

- Issuer.
    
- Audience.
    
- Token lifetime.
    
- MFA required.
    
- Allowed origins.
    
- HSTS.
    
- Webhook window.
    
- Allowed algorithms.
    
- Maximum login attempts.
    

Alguns valores não deverão ser alterados sem restart e revisão.

---

# CORS

CORS deverá ser configurado por allowlist.

Exemplo:

```json
{
  "Security": {
    "AllowedOrigins": [
      "https://app.infinitecontent.ai"
    ]
  }
}
```

Evitar:

```text
AllowAnyOrigin
```

em produção.

---

# Validação de URLs

URLs configuradas deverão ser validadas.

Exemplos:

- HTTPS obrigatório em produção.
    
- Host permitido.
    
- Porta válida.
    
- Sem credenciais embutidas.
    
- Sem URL local em produção.
    
- Sem protocolo inseguro.
    

---

# Configuração de Timeouts

Timeouts deverão ser organizados por operação.

Evitar um único timeout global.

Exemplo:

```json
{
  "Timeouts": {
    "DatabaseSeconds": 30,
    "TextGenerationSeconds": 90,
    "ImageGenerationSeconds": 180,
    "PublishingSeconds": 120,
    "WebhookSeconds": 10
  }
}
```

---

# Configuração de Retry

Cada operação deverá possuir sua política.

Exemplo:

```json
{
  "Resilience": {
    "TextGeneration": {
      "MaximumAttempts": 3,
      "BaseDelaySeconds": 2
    },
    "Publishing": {
      "MaximumAttempts": 5,
      "BaseDelaySeconds": 30
    }
  }
}
```

---

# Configuração de Limites

Limites deverão existir para:

- Upload.
    
- Payload.
    
- Concorrência.
    
- Custo.
    
- Tokens.
    
- Tamanho de resposta.
    
- Duração.
    
- Quantidade de steps.
    
- Número de retries.
    
- Batch size.
    

---

# Limites Técnicos e Comerciais

## Técnico

Protege a plataforma.

Exemplo:

```text
MaximumUploadSize = 5 GB
```

## Comercial

Depende do plano.

Exemplo:

```text
PlanUploadLimit = 1 GB
```

O limite efetivo será o menor.

---

# Logging de Configuração

A aplicação poderá registrar na inicialização:

- Seção carregada.
    
- Origem.
    
- Versão.
    
- Valores não sensíveis.
    
- Recursos habilitados.
    
- Providers habilitados.
    

Não deverá registrar:

- Segredos.
    
- Connection strings.
    
- Tokens.
    
- Senhas.
    
- Chaves.
    

---

# Startup Summary

Exemplo seguro:

```text
Environment: Production
Database: Configured
Messaging: RabbitMQ
Storage: AzureBlob
OpenAI: Enabled
Gemini: Enabled
AutomaticPublishing: Disabled
```

---

# Observabilidade de Configuração

Métricas possíveis:

```text
configuration_reload_total
configuration_reload_failures_total
feature_flag_changes_total
configuration_validation_failures_total
configuration_source_unavailable_total
```

---

# Alertas

Alertas possíveis:

- Configuração dinâmica inválida.
    
- Secret reference ausente.
    
- Provider habilitado sem credencial.
    
- Configuração remota indisponível.
    
- Alteração crítica fora da janela.
    
- Kill switch ativado.
    
- Muitos reloads.
    
- Divergência entre instâncias.
    
- Valores inconsistentes entre ambientes.
    

---

# Consistência entre Instâncias

Em ambientes distribuídos, múltiplas instâncias deverão convergir para a mesma configuração.

O sistema deverá acompanhar:

- Versão aplicada.
    
- Horário de atualização.
    
- Instâncias desatualizadas.
    
- Falhas de reload.
    

---

# Configuration Revision

Cada instância poderá expor:

```text
ConfigurationRevision
```

Isso ajuda a detectar divergência.

---

# Health Check de Configuração

Um health check poderá validar:

- Configuração inicial válida.
    
- Secret references resolvidas.
    
- Configuração remota acessível.
    
- Versão mínima aplicada.
    
- Ausência de inconsistências críticas.
    

---

# Testes Unitários

Devem validar:

- Binding.
    
- Defaults.
    
- Data annotations.
    
- Regras entre campos.
    
- Precedência.
    
- Configuração efetiva.
    
- Feature flags.
    
- Fallback.
    
- Snapshot.
    
- Máscara de segredos.
    

---

# Testes de Integração

Devem validar:

- appsettings.
    
- Variáveis de ambiente.
    
- Secret Manager fake.
    
- Startup validation.
    
- Reload.
    
- IOptionsMonitor.
    
- Configuração por ambiente.
    
- Falha segura.
    
- Divergência.
    
- Configuração por organização.
    

---

# Testes de Startup

Cenários:

- Connection string ausente.
    
- Broker inválido.
    
- Provider habilitado sem secret.
    
- URL HTTP em produção.
    
- Timeout negativo.
    
- Heartbeat maior que lease.
    
- Feature incompatível habilitada.
    
- Configuração mínima válida.
    

---

# Testes de Feature Flags

Devem validar:

- Flag desabilitada.
    
- Flag habilitada.
    
- Rollout por organização.
    
- Kill switch.
    
- Mudança em runtime.
    
- Auditoria.
    
- Último valor válido.
    

---

# Testes de Precedência

Exemplo:

```text
appsettings = 10
environment variable = 20
command line = 30
```

O resultado deverá ser:

```text
30
```

---

# Ferramentas Possíveis

Para configuração dinâmica, poderão ser avaliados:

- Azure App Configuration.
    
- AWS AppConfig.
    
- Consul.
    
- LaunchDarkly.
    
- Unleash.
    
- Redis.
    
- Configuração persistida no PostgreSQL.
    

A escolha dependerá de:

- Custo.
    
- Complexidade.
    
- Feature flags.
    
- Auditoria.
    
- Escala.
    
- Integração com .NET.
    
- Portabilidade.
    

---

# Azure App Configuration

Pode oferecer:

- Configuração centralizada.
    
- Feature flags.
    
- Refresh.
    
- Labels por ambiente.
    
- Integração com Key Vault.
    

Porém aumenta dependência de Azure.

---

# LaunchDarkly ou Unleash

Adequados para:

- Feature flags.
    
- Rollout.
    
- Segmentação.
    
- Experimentos.
    
- Kill switches.
    

Não devem ser utilizados como repositório geral de todos os valores técnicos.

---

# Configuração no PostgreSQL

Adequada para:

- Preferências de organização.
    
- Políticas de provider.
    
- Limites comerciais.
    
- Feature flags internas por tenant.
    
- Configuração de pipeline.
    
- Configuração de produto.
    

Não adequada para:

- Bootstrap do próprio banco.
    
- Secret root.
    
- Configuração de rede.
    
- Broker principal.
    
- Configuração necessária antes do DbContext.
    

---

# Bootstrap

Algumas configurações são necessárias para carregar outras.

Exemplo:

```text
Environment
Secret Manager
Database
Remote Configuration
```

A cadeia de bootstrap deverá permanecer pequena.

---

# Falha de Bootstrap

Se a aplicação não conseguir carregar:

- Secret Manager obrigatório.
    
- Connection string.
    
- Certificado.
    
- Configuração de autenticação.
    

ela deverá falhar no startup.

---

# Configuração e CI/CD

A pipeline deverá:

- Validar arquivos.
    
- Validar schemas.
    
- Detectar valores proibidos.
    
- Garantir ausência de segredos.
    
- Comparar configurações entre ambientes.
    
- Aplicar templates.
    
- Validar referências.
    

---

# Configuration as Code

Configurações de infraestrutura e deploy deverão ser versionadas quando possível.

Exemplos:

- Helm values.
    
- Terraform variables.
    
- Bicep parameters.
    
- Docker Compose.
    
- Kubernetes manifests.
    

Segredos continuarão fora do repositório.

---

# Templates de Ambiente

Poderão existir arquivos de exemplo:

```text
appsettings.example.json
.env.example
helm-values.example.yaml
```

Eles deverão conter:

- Chaves.
    
- Descrições.
    
- Valores fictícios.
    
- Defaults seguros.
    

---

# Documentação de Configuração

Cada opção deverá possuir:

- Nome.
    
- Seção.
    
- Tipo.
    
- Obrigatoriedade.
    
- Default.
    
- Faixa.
    
- Ambiente.
    
- Sensibilidade.
    
- Necessidade de restart.
    
- Descrição.
    

---

# Catálogo de Configuração

Exemplo:

|Chave|Tipo|Default|Sensível|Reload|
|---|---|--:|---|---|
|Pipelines:MaximumAttempts|int|3|Não|Sim|
|Messaging:Host|string|—|Não|Não|
|Providers:OpenAI:ApiKey|secret|—|Sim|Sim|
|Security:Issuer|string|—|Não|Não|

---

# Depreciação de Configuração

Opções antigas deverão possuir processo de remoção.

Etapas:

1. Marcar como deprecated.
    
2. Emitir warning.
    
3. Documentar substituição.
    
4. Manter compatibilidade temporária.
    
5. Remover em versão futura.
    

---

# Renomear Configuração

Uma chave não deverá ser renomeada silenciosamente.

Exemplo:

```text
Providers:OpenAI:Model
```

para:

```text
Providers:OpenAI:DefaultTextModel
```

A aplicação poderá suportar ambas temporariamente e emitir warning.

---

# Migração de Configuração

Configurações persistidas poderão exigir migração.

Exemplo:

```text
ProviderPolicy v1
    ↓
ProviderPolicy v2
```

A migração deverá ser:

- Versionada.
    
- Testável.
    
- Auditável.
    
- Reversível quando possível.
    

---

# Segurança

Configurações deverão ser protegidas contra:

- Alteração não autorizada.
    
- Exposição.
    
- Tampering.
    
- Divergência.
    
- Defaults inseguros.
    
- Injeção por ambiente.
    
- Segredos em logs.
    

---

# Acesso Administrativo

Alterações em configurações críticas deverão exigir:

- Permissão.
    
- MFA quando aplicável.
    
- Motivo.
    
- Auditoria.
    
- Aprovação adicional em casos sensíveis.
    

---

# Mudanças Críticas

Exemplos:

- Habilitar publicação automática.
    
- Alterar provider principal.
    
- Aumentar limite de custo.
    
- Alterar regras de retenção.
    
- Desabilitar validação.
    
- Alterar algoritmo de assinatura.
    
- Habilitar modelo experimental.
    

---

# Fail Closed

Configurações de segurança deverão falhar de forma fechada.

Exemplo:

```text
AllowAutomaticPublishing
```

se ausente ou inválida deverá resultar em:

```text
false
```

---

# Fail Open

Fail open somente deverá ser utilizado quando o risco for baixo.

Exemplo possível:

```text
CacheEnabled
```

se a configuração estiver indisponível, o sistema pode funcionar sem cache.

---

# Checklist para Nova Configuração

Toda nova configuração deverá responder:

- Qual problema resolve?
    
- Qual módulo é responsável?
    
- Qual seção?
    
- Qual tipo?
    
- Qual default?
    
- É obrigatória?
    
- É sensível?
    
- Precisa de reload?
    
- Precisa de restart?
    
- Qual faixa válida?
    
- Qual ambiente utiliza?
    
- Pode variar por organização?
    
- Precisa de auditoria?
    
- Precisa de snapshot?
    
- O que acontece se estiver ausente?
    
- Como será testada?
    

---

# Checklist para Nova Feature Flag

Toda nova flag deverá definir:

- Nome.
    
- Objetivo.
    
- Tipo.
    
- Owner.
    
- Default.
    
- Público alvo.
    
- Estratégia de rollout.
    
- Kill switch.
    
- Data de revisão.
    
- Data de remoção.
    
- Métricas.
    
- Auditoria.
    
- Comportamento de fallback.
    

---

# Checklist para Novo Provider

A configuração deverá definir:

- Enabled.
    
- BaseUrl.
    
- SecretReference.
    
- Modelos.
    
- Timeout.
    
- Retry.
    
- Circuit breaker.
    
- Limites.
    
- Região.
    
- Custos.
    
- Health check.
    
- Fallback.
    
- Política de logging.
    

---

# Checklist para Novo Worker

A configuração deverá definir:

- Enabled.
    
- Queue.
    
- Prefetch.
    
- Concurrency.
    
- Batch size.
    
- Polling interval.
    
- Lease.
    
- Heartbeat.
    
- Shutdown timeout.
    
- Retry.
    
- DLQ.
    
- Métricas.
    
- Health check.
    

---

# Regras Arquiteturais

- Código não deve conter valores específicos de ambiente.
    
- Segredos não devem ficar em appsettings versionados.
    
- Cada módulo deve possuir opções tipadas.
    
- IConfiguration não deve ser espalhado pela aplicação.
    
- Configurações críticas devem usar ValidateOnStart.
    
- Defaults devem ser seguros.
    
- Precedência deve ser documentada.
    
- Configuração técnica e configuração de negócio devem permanecer separadas.
    
- Feature flags não substituem autorização.
    
- Kill switches devem possuir auditoria.
    
- Configuração dinâmica deve manter último valor válido.
    
- Mudanças relacionadas devem ser aplicadas atomicamente.
    
- Execuções longas devem registrar snapshot de configuração.
    
- Snapshots não devem conter segredos.
    
- Configurações por organização devem ser persistidas.
    
- Limites técnicos não podem ser ultrapassados por configuração comercial.
    
- Alterações críticas devem ser auditadas.
    
- Configuração remota indisponível não pode causar comportamento indefinido.
    
- Logs não devem expor segredos.
    
- Configuração de produção deve usar Secret Manager.
    
- Ambientes não devem compartilhar credenciais.
    
- URLs de produção devem utilizar HTTPS.
    
- Chaves depreciadas devem possuir plano de remoção.
    
- Configurações devem possuir testes.
    
- Toda instância deve expor versão ou revisão de configuração.
    
- Configurações de segurança devem falhar de forma fechada.
    

---

# Decisões Pendentes

As seguintes decisões deverão ser registradas em ADRs:

- Fontes oficiais de configuração.
    
- Ordem final de precedência.
    
- Uso de configuração dinâmica.
    
- Ferramenta de feature flags.
    
- Uso de Azure App Configuration.
    
- Uso de LaunchDarkly ou Unleash.
    
- Estratégia de configuração por organização.
    
- Estratégia de snapshot.
    
- Política de reload.
    
- Configurações que exigem restart.
    
- Estrutura dos Options.
    
- Política de defaults.
    
- Estratégia de versionamento.
    
- Auditoria de mudanças.
    
- Catálogo de configuração.
    
- Política de depreciação.
    
- Estratégia de rollback.
    
- Health check de configuração.
    
- Política de consistência entre instâncias.
    
- Estratégia de bootstrap.
    

---

# Exemplo Completo

```text
Aplicação inicia
    ↓
Carrega appsettings.json
    ↓
Carrega appsettings.Production.json
    ↓
Aplica variáveis de ambiente
    ↓
Conecta ao Secret Manager
    ↓
Resolve referências de segredos
    ↓
Realiza binding dos Options
    ↓
Valida configurações
    ↓
Detecta que OpenAI está habilitado
    ↓
Confirma que ApiKeySecretName está presente
    ↓
Confirma que timeout é válido
    ↓
Registra providers
    ↓
Registra revisão de configuração
    ↓
Publica health status Ready
```

Cenário de configuração inválida:

```text
Worker inicia
    ↓
PipelineOptions são carregadas
    ↓
HeartbeatInterval = 120 segundos
LeaseDuration = 60 segundos
    ↓
Validação detecta inconsistência
    ↓
Startup falha
    ↓
Erro seguro é registrado
    ↓
Instância não começa a consumir mensagens
```

Cenário de alteração dinâmica:

```text
Operador reduz concorrência de vídeo
    ↓
Configuração dinâmica recebe nova revisão
    ↓
Valor é validado
    ↓
IOptionsMonitor detecta mudança
    ↓
Media Worker reduz novos jobs simultâneos
    ↓
Jobs atuais continuam
    ↓
Alteração é auditada
    ↓
Métrica configuration_reload_total é incrementada
```

Cenário de execução com snapshot:

```text
Pipeline é iniciado
    ↓
Configuration Resolver combina:
    Global
    Plano
    Organização
    Projeto
    Pipeline
    ↓
Configuração efetiva é validada
    ↓
Snapshot sem segredos é persistido
    ↓
Provider e modelo são selecionados
    ↓
Configuração global muda
    ↓
Pipeline atual continua com o snapshot original
    ↓
Novas execuções utilizam a nova versão
```

---

# Objetivo Final

Criar uma arquitetura de configuração previsível, segura e rastreável.

O Infinite Content AI deverá iniciar apenas quando possuir condições válidas para operar.

Configurações técnicas deverão permanecer fora do código.

Segredos deverão permanecer protegidos.

Mudanças dinâmicas deverão ser controladas.

Execuções deverão ser reproduzíveis.

Ambientes deverão permanecer isolados.

A configuração deverá apoiar a evolução do sistema sem introduzir comportamento oculto ou inseguro.