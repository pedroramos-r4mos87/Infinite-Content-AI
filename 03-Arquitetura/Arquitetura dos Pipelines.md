# Arquitetura dos Pipelines

## Objetivo

Definir como os pipelines do Infinite Content AI serão estruturados, configurados, executados, monitorados e retomados.

Um pipeline representa um fluxo de trabalho composto por etapas organizadas.

Cada etapa poderá executar um agente, uma ação interna, uma validação, uma aprovação humana ou uma integração externa.

---

# Visão Geral

Os agentes executam tarefas especializadas.

Os pipelines coordenam essas tarefas.

Exemplo:

```text
Trend Agent
    ↓
Research Agent
    ↓
Strategy Agent
    ↓
Script Agent
    ↓
Review Agent
```

Nesse fluxo:

- Cada agente possui uma responsabilidade.
    
- O pipeline define a ordem.
    
- O pipeline controla o estado.
    
- O pipeline decide quando continuar.
    
- O pipeline trata falhas.
    
- O pipeline registra resultados.
    
- O pipeline solicita aprovação quando necessário.
    

---

# Responsabilidade do Pipeline

Um pipeline deverá ser responsável por:

- Definir quais etapas serão executadas.
    
- Definir a ordem das etapas.
    
- Preparar as entradas.
    
- Encaminhar saídas entre etapas.
    
- Persistir o progresso.
    
- Controlar falhas.
    
- Aplicar retentativas.
    
- Executar fallbacks.
    
- Solicitar aprovação humana.
    
- Interromper ou cancelar execuções.
    
- Registrar métricas.
    
- Finalizar o processo.
    

---

# O que o Pipeline não deve fazer

Um pipeline não deve:

- Conter regras específicas de geração de texto.
    
- Conhecer detalhes da OpenAI.
    
- Acessar diretamente APIs externas.
    
- Acessar diretamente o DbContext.
    
- Implementar regras que pertencem ao Domain.
    
- Gerar conteúdo por conta própria.
    
- Publicar diretamente em plataformas.
    
- Duplicar responsabilidades dos agentes.
    

O pipeline apenas coordena componentes.

---

# Tipos de Etapa

Um pipeline poderá possuir diferentes tipos de etapa.

## Agent Step

Executa um agente.

Exemplo:

```text
ExecuteScriptAgent
```

---

## Validation Step

Valida uma saída antes de continuar.

Exemplo:

```text
ValidateResearchSources
```

---

## Transformation Step

Transforma dados entre dois formatos.

Exemplo:

```text
ConvertScriptToVoiceInput
```

---

## Approval Step

Interrompe o pipeline até uma aprovação humana.

Exemplo:

```text
WaitForScriptApproval
```

---

## Integration Step

Executa uma integração externa por meio de uma abstração.

Exemplo:

```text
UploadVideoToStorage
```

---

## Decision Step

Escolhe o próximo caminho com base em uma condição.

Exemplo:

```text
IsContentApproved?
```

---

## Delay Step

Agenda a continuidade para outro momento.

Exemplo:

```text
WaitUntilPublishingDate
```

---

## Parallel Step

Executa etapas independentes em paralelo.

Exemplo:

```text
GenerateVoice
GenerateThumbnail
GenerateSubtitles
```

---

## Compensation Step

Desfaz ou compensa uma ação executada anteriormente.

Exemplo:

```text
RemoveUploadedTemporaryFile
```

---

# Estrutura Sugerida

```text
Application/
└── Pipelines/
    ├── Abstractions/
    │   ├── IPipeline.cs
    │   ├── IPipelineStep.cs
    │   ├── PipelineContext.cs
    │   ├── PipelineResult.cs
    │   ├── PipelineStepResult.cs
    │   └── PipelineDefinition.cs
    │
    ├── YouTubeLongForm/
    │   ├── YouTubeLongFormPipeline.cs
    │   ├── YouTubeLongFormContext.cs
    │   └── Steps/
    │
    ├── YouTubeShort/
    │   ├── YouTubeShortPipeline.cs
    │   ├── YouTubeShortContext.cs
    │   └── Steps/
    │
    ├── Article/
    │   ├── ArticlePipeline.cs
    │   ├── ArticlePipelineContext.cs
    │   └── Steps/
    │
    ├── SocialPost/
    │   ├── SocialPostPipeline.cs
    │   ├── SocialPostContext.cs
    │   └── Steps/
    │
    └── Services/
        ├── PipelineExecutor.cs
        ├── PipelineRegistry.cs
        └── PipelineStateManager.cs
```

---

# Contrato Base do Pipeline

Exemplo conceitual:

```csharp
public interface IPipeline<in TInput, TOutput>
{
    string Name { get; }

    int Version { get; }

    Task<Result<TOutput>> ExecuteAsync(
        TInput input,
        PipelineExecutionContext context,
        CancellationToken cancellationToken);
}
```

Cada pipeline deverá possuir:

- Nome.
    
- Versão.
    
- Entrada.
    
- Saída.
    
- Identificador.
    
- Lista de etapas.
    
- Política de execução.
    

---

# Contrato Base da Etapa

Exemplo conceitual:

```csharp
public interface IPipelineStep<TContext>
{
    string Name { get; }

    Task<PipelineStepResult> ExecuteAsync(
        TContext context,
        CancellationToken cancellationToken);
}
```

Cada etapa deverá possuir:

- Nome único dentro do pipeline.
    
- Tipo.
    
- Ordem.
    
- Status.
    
- Política de retentativa.
    
- Condição de execução.
    
- Timeout.
    
- Dependências.
    
- Entradas.
    
- Saídas.
    

---

# Contexto do Pipeline

O pipeline deverá trabalhar com um contexto de execução.

Exemplo conceitual:

```csharp
public sealed class PipelineExecutionContext
{
    public Guid ExecutionId { get; init; }

    public Guid ProjectId { get; init; }

    public string PipelineName { get; init; } = string.Empty;

    public int PipelineVersion { get; init; }

    public string CorrelationId { get; init; } = string.Empty;

    public IDictionary<string, object> Data { get; init; }
        = new Dictionary<string, object>();

    public decimal MaximumCost { get; init; }

    public DateTimeOffset StartedAt { get; init; }
}
```

O contexto poderá conter:

- Identificador da execução.
    
- Projeto.
    
- Pipeline.
    
- Versão.
    
- Canal.
    
- Idioma.
    
- Público.
    
- Dados produzidos.
    
- Configurações.
    
- Limite de custo.
    
- Preferências de provider.
    
- Correlation ID.
    
- Status atual.
    

---

# Contexto Tipado

Sempre que possível, cada pipeline deverá utilizar um contexto tipado.

Exemplo:

```csharp
public sealed class YouTubeLongFormPipelineContext
{
    public required ContentProject Project { get; init; }

    public TrendResult? Trend { get; set; }

    public ResearchResult? Research { get; set; }

    public StrategyResult? Strategy { get; set; }

    public ScriptResult? Script { get; set; }

    public ReviewResult? Review { get; set; }

    public VoiceResult? Voice { get; set; }

    public ThumbnailResult? Thumbnail { get; set; }

    public VideoResult? Video { get; set; }

    public SeoResult? Seo { get; set; }

    public PublicationResult? Publication { get; set; }
}
```

O uso de dicionários genéricos deverá ser restrito a metadados ou extensões.

Os dados principais devem ser fortemente tipados.

---

# Definição de Pipeline

Um pipeline poderá ser definido por código inicialmente.

Exemplo conceitual:

```csharp
public sealed class YouTubeLongFormPipeline
{
    private readonly IReadOnlyCollection<IPipelineStep<YouTubeLongFormPipelineContext>> _steps;

    public YouTubeLongFormPipeline(
        IEnumerable<IPipelineStep<YouTubeLongFormPipelineContext>> steps)
    {
        _steps = steps.OrderBy(step => step.Order).ToArray();
    }
}
```

No futuro, parte da configuração poderá ser armazenada externamente.

Exemplos:

- Banco de dados.
    
- Arquivos JSON.
    
- Arquivos YAML.
    
- Interface administrativa.
    
- n8n.
    

As regras essenciais continuarão protegidas pela aplicação.

---

# Pipeline Inicial: YouTube Long Form

## Objetivo

Produzir vídeos longos para YouTube.

## Etapas

```text
CreateExecution
    ↓
FindTrend
    ↓
PerformResearch
    ↓
DefineStrategy
    ↓
GenerateScript
    ↓
ReviewScript
    ↓
WaitForScriptApproval
    ↓
GenerateVoice
    ↓
GenerateThumbnail
    ↓
GenerateVisualAssets
    ↓
RenderVideo
    ↓
GenerateSeoMetadata
    ↓
WaitForPublishingApproval
    ↓
SchedulePublication
    ↓
PublishContent
    ↓
CollectInitialMetrics
    ↓
CompleteExecution
```

---

# Pipeline Inicial: Short Video

## Objetivo

Produzir conteúdo curto para YouTube Shorts, TikTok ou Instagram Reels.

## Etapas

```text
CreateExecution
    ↓
FindTrend
    ↓
DefineShortStrategy
    ↓
GenerateShortScript
    ↓
ReviewScript
    ↓
GenerateVoice
    ↓
GenerateVisualAssets
    ↓
RenderVerticalVideo
    ↓
GenerateCaptionAndHashtags
    ↓
ApprovePublication
    ↓
PublishContent
    ↓
CompleteExecution
```

---

# Pipeline Inicial: Article

## Objetivo

Produzir artigos para blogs ou portais.

## Etapas

```text
CreateExecution
    ↓
SelectTopic
    ↓
PerformResearch
    ↓
CreateOutline
    ↓
GenerateArticle
    ↓
ReviewFacts
    ↓
OptimizeSeo
    ↓
ApproveArticle
    ↓
PublishArticle
    ↓
CompleteExecution
```

---

# Pipeline Inicial: Social Post

## Objetivo

Produzir publicações para redes sociais.

## Etapas

```text
CreateExecution
    ↓
SelectTopic
    ↓
DefinePlatformStrategy
    ↓
GenerateCopy
    ↓
GenerateImage
    ↓
ReviewContent
    ↓
ApprovePublication
    ↓
SchedulePost
    ↓
CompleteExecution
```

---

# Sequência de Etapas

Por padrão, as etapas serão executadas sequencialmente.

Exemplo:

```text
Step 1
    ↓
Step 2
    ↓
Step 3
```

Uma etapa somente poderá começar quando suas dependências forem concluídas.

---

# Execução Paralela

Etapas independentes poderão ser executadas em paralelo.

Exemplo:

```text
              ┌── GenerateVoice
ApprovedScript
              ├── GenerateThumbnail
              │
              └── GenerateSubtitles
```

Depois:

```text
GenerateVoice ────────┐
GenerateThumbnail ────┼── RenderVideo
GenerateSubtitles ────┘
```

A execução paralela deverá ser utilizada apenas quando:

- As etapas forem independentes.
    
- Não houver conflito de dados.
    
- A redução de tempo for relevante.
    
- Os limites dos providers permitirem.
    
- O custo estiver controlado.
    

---

# Dependências entre Etapas

Cada etapa poderá declarar dependências.

Exemplo:

```text
RenderVideo
```

Depende de:

- VoiceGenerated.
    
- VisualAssetsGenerated.
    
- SubtitlesGenerated.
    
- ScriptApproved.
    

A etapa não poderá começar antes da conclusão das dependências obrigatórias.

---

# Condições de Execução

Uma etapa poderá possuir condições.

Exemplo:

```text
TranslationStep
```

Executar somente quando:

```text
SourceLanguage != TargetLanguage
```

Outro exemplo:

```text
HumanApprovalStep
```

Executar somente quando:

```text
Project.RequiresManualApproval == true
```

---

# Etapas Opcionais

Algumas etapas poderão ser opcionais.

Exemplos:

- Tradução.
    
- Revisão humana.
    
- Geração de thumbnail.
    
- Publicação automática.
    
- Coleta de métricas.
    
- Geração de legendas.
    
- Criação de variações.
    

Etapas opcionais deverão ser explicitamente configuradas.

---

# Decisões e Ramificações

Pipelines poderão possuir ramificações.

Exemplo:

```text
Review Script
    ↓
Approved?
   / \
 Yes  No
  |    |
  |    └── Rewrite Script
  |
  └── Continue
```

Outro exemplo:

```text
Provider available?
   / \
 Yes  No
  |    |
  |    └── Execute Fallback
  |
  └── Continue
```

---

# Loops Controlados

Um pipeline poderá repetir etapas em situações específicas.

Exemplo:

```text
Generate Script
    ↓
Review Script
    ↓
Score >= Minimum?
   / \
 Yes  No
  |    |
  |    └── Generate Script Again
  |
  └── Continue
```

Loops deverão possuir limites.

Exemplo:

```text
MaximumReviewAttempts = 3
```

Nenhum fluxo poderá repetir indefinidamente.

---

# Estados do Pipeline

Uma execução poderá possuir os seguintes estados:

```text
Created
Queued
Running
WaitingForApproval
WaitingForSchedule
Completed
Failed
PartiallyCompleted
Cancelled
Expired
```

---

# Estados das Etapas

Cada etapa poderá possuir os seguintes estados:

```text
NotStarted
Queued
Running
Succeeded
Failed
Skipped
WaitingForApproval
WaitingForRetry
Cancelled
Compensated
```

---

# Persistência de Estado

O estado deverá ser persistido após cada etapa relevante.

Isso permitirá:

- Retomar execuções interrompidas.
    
- Investigar falhas.
    
- Exibir progresso.
    
- Evitar reprocessamento desnecessário.
    
- Controlar custos.
    
- Auditar decisões.
    
- Suportar processos longos.
    

---

# Estrutura de Persistência

Entidades possíveis:

```text
PipelineExecution
PipelineStepExecution
PipelineApproval
PipelineArtifact
PipelineEvent
```

---

## PipelineExecution

Poderá armazenar:

- Identificador.
    
- Projeto.
    
- Nome do pipeline.
    
- Versão.
    
- Status.
    
- Etapa atual.
    
- Data de início.
    
- Data de término.
    
- Custo acumulado.
    
- Correlation ID.
    
- Erro final.
    
- Dados de entrada.
    
- Dados de saída.
    

---

## PipelineStepExecution

Poderá armazenar:

- Identificador.
    
- PipelineExecutionId.
    
- Nome da etapa.
    
- Tipo.
    
- Ordem.
    
- Status.
    
- Número de tentativas.
    
- Entrada.
    
- Saída.
    
- Erro.
    
- Provider utilizado.
    
- Modelo utilizado.
    
- Custo.
    
- Tokens.
    
- Data de início.
    
- Data de término.
    

---

# Checkpoint

Após uma etapa concluída, o pipeline poderá criar um checkpoint.

Exemplo:

```text
ResearchCompleted
ScriptGenerated
ScriptApproved
VideoRendered
ContentPublished
```

Um checkpoint indica que o fluxo pode ser retomado a partir daquele ponto.

---

# Retomada de Execução

Quando uma execução for retomada:

1. Carregar o estado persistido.
    
2. Identificar a última etapa concluída.
    
3. Validar os artefatos existentes.
    
4. Ignorar etapas já concluídas.
    
5. Continuar a partir da próxima etapa.
    
6. Manter o mesmo identificador de execução.
    
7. Registrar a retomada.
    

---

# Idempotência

Toda etapa deverá ser idempotente quando possível.

Uma reexecução não deverá produzir efeitos duplicados.

Exemplos de riscos:

- Publicar o mesmo vídeo duas vezes.
    
- Gerar múltiplas cobranças desnecessárias.
    
- Salvar arquivos duplicados.
    
- Criar eventos repetidos.
    
- Enviar notificações duplicadas.
    

Etapas sensíveis deverão utilizar uma chave de idempotência.

Exemplo:

```text
PipelineExecutionId + StepName + Version
```

---

# Retentativas

Cada etapa poderá possuir uma política de retentativa.

Exemplo:

```text
MaximumAttempts: 3
InitialDelay: 5 seconds
Backoff: Exponential
```

Uma política poderá considerar:

- Tipo do erro.
    
- Provider.
    
- Custo.
    
- Timeout.
    
- Limite de requisições.
    
- Disponibilidade do serviço.
    

---

# Erros Transitórios

Exemplos:

- Timeout.
    
- Limite temporário de requisições.
    
- Falha de rede.
    
- Serviço indisponível.
    
- Erro HTTP 429.
    
- Erro HTTP 503.
    

Esses erros poderão gerar retentativa automática.

---

# Erros Permanentes

Exemplos:

- Entrada inválida.
    
- Projeto inexistente.
    
- Conteúdo bloqueado.
    
- Credencial inválida.
    
- Configuração ausente.
    
- Limite de custo excedido.
    
- Aprovação rejeitada.
    

Esses erros não devem gerar retentativas automáticas indiscriminadas.

---

# Estratégia de Fallback

Uma etapa poderá executar providers alternativos.

Exemplo:

```text
OpenAI
    ↓ falhou
Gemini
    ↓ falhou
Anthropic
    ↓ falhou
Controlled Failure
```

O fallback deverá respeitar:

- Providers habilitados.
    
- Prioridade.
    
- Tipo de tarefa.
    
- Modelo requerido.
    
- Limite de custo.
    
- Região.
    
- Qualidade mínima.
    
- Política do projeto.
    

---

# Timeout

Toda etapa externa deverá possuir timeout.

Exemplo:

```text
TextGenerationTimeout = 90 seconds
VideoRenderingTimeout = 30 minutes
PublishingTimeout = 2 minutes
```

Timeouts deverão ser configuráveis.

---

# Cancelamento

Toda execução deverá aceitar `CancellationToken`.

O cancelamento poderá ocorrer por:

- Solicitação do usuário.
    
- Limite de custo.
    
- Timeout global.
    
- Falha crítica.
    
- Regra de negócio.
    
- Desativação do projeto.
    
- Encerramento do Worker.
    

Ao cancelar:

1. Interromper novas etapas.
    
2. Solicitar cancelamento das etapas atuais.
    
3. Persistir o estado.
    
4. Registrar o motivo.
    
5. Executar compensações necessárias.
    

---

# Aprovação Humana

Uma etapa de aprovação deverá interromper o fluxo.

Exemplo:

```text
GenerateScript
    ↓
WaitForApproval
    ↓
GenerateVoice
```

Enquanto aguarda aprovação, o pipeline ficará no estado:

```text
WaitingForApproval
```

A aprovação deverá registrar:

- Usuário responsável.
    
- Data e hora.
    
- Decisão.
    
- Comentários.
    
- Versão do artefato aprovado.
    
- Etapa relacionada.
    

---

# Tipos de Aprovação

## Aprovação de Roteiro

Antes da geração de voz e vídeo.

## Aprovação de Mídia

Antes da publicação.

## Aprovação Editorial

Quando o conteúdo envolver temas sensíveis.

## Aprovação Financeira

Quando o custo estimado ultrapassar um limite.

## Aprovação de Exceção

Quando alguma regra for ignorada manualmente.

---

# Compensação

Nem toda operação poderá ser desfeita.

Quando possível, deverão existir ações compensatórias.

Exemplos:

- Remover arquivo temporário.
    
- Cancelar publicação agendada.
    
- Excluir upload incompleto.
    
- Liberar recurso.
    
- Reverter uma reserva.
    
- Marcar conteúdo como inválido.
    

A compensação não deverá apagar o histórico da execução.

---

# Custos

O pipeline deverá acompanhar o custo acumulado.

Exemplos:

- Texto.
    
- Imagem.
    
- Voz.
    
- Vídeo.
    
- Storage.
    
- Publicação.
    
- Processamento.
    

Antes de executar uma etapa, o sistema poderá verificar:

```text
CurrentCost + EstimatedStepCost <= MaximumPipelineCost
```

Caso o limite seja excedido:

- Interromper o pipeline.
    
- Solicitar aprovação.
    
- Trocar o provider.
    
- Escolher um modelo mais barato.
    
- Reduzir a qualidade.
    
- Marcar a execução como falha controlada.
    

---

# Orçamento por Etapa

Cada etapa poderá possuir:

- Custo estimado.
    
- Custo máximo.
    
- Provider preferencial.
    
- Modelo preferencial.
    
- Estratégia econômica.
    
- Estratégia de qualidade.
    

Exemplo:

```text
ScriptGeneration
EstimatedCost: $0.08
MaximumCost: $0.15
```

---

# Observabilidade

Toda execução deverá registrar logs estruturados.

Campos mínimos:

- PipelineExecutionId.
    
- PipelineName.
    
- PipelineVersion.
    
- StepName.
    
- StepType.
    
- Status.
    
- Attempt.
    
- Duration.
    
- Provider.
    
- Model.
    
- Cost.
    
- CorrelationId.
    
- ErrorCode.
    

---

# Métricas Técnicas

- Quantidade de execuções.
    
- Taxa de sucesso.
    
- Taxa de falha.
    
- Duração média.
    
- Duração por etapa.
    
- Etapas com maior falha.
    
- Número de retentativas.
    
- Número de fallbacks.
    
- Custo médio.
    
- Custo por pipeline.
    
- Tempo de espera por aprovação.
    
- Número de cancelamentos.
    

---

# Eventos

O pipeline poderá produzir eventos durante a execução.

Exemplos:

```text
PipelineStarted
StepStarted
StepCompleted
StepFailed
ApprovalRequested
ApprovalGranted
ApprovalRejected
PipelineCompleted
PipelineFailed
PipelineCancelled
```

Esses eventos poderão ser utilizados para:

- Atualizar a interface.
    
- Notificar usuários.
    
- Iniciar automações.
    
- Alimentar métricas.
    
- Disparar webhooks.
    
- Integrar com n8n.
    

---

# Mensageria

Execuções longas deverão ser processadas de forma assíncrona.

Exemplo:

```text
API
    ↓
CreatePipelineExecution
    ↓
Message Queue
    ↓
Worker
    ↓
Pipeline Executor
```

A API não deverá permanecer aguardando a conclusão de um pipeline longo.

---

# Papel do Worker

O Worker deverá:

- Consumir mensagens.
    
- Carregar a execução.
    
- Iniciar ou retomar o pipeline.
    
- Executar etapas.
    
- Persistir progresso.
    
- Publicar eventos.
    
- Aplicar retentativas.
    
- Tratar cancelamentos.
    

O Worker não deverá conter regras específicas dos agentes.

---

# Papel do n8n

O n8n poderá:

- Agendar execuções.
    
- Disparar pipelines.
    
- Receber webhooks.
    
- Enviar notificações.
    
- Integrar serviços auxiliares.
    
- Aguardar horários externos.
    
- Coordenar processos administrativos.
    

O n8n não deverá:

- Conter regras centrais do negócio.
    
- Controlar estados críticos sozinho.
    
- Ser a única fonte de verdade.
    
- Conhecer detalhes internos do domínio.
    
- Substituir a camada Application.
    

---

# Relação entre Aplicação e n8n

```text
n8n
    ↓
API ou Webhook
    ↓
Application
    ↓
Pipeline
    ↓
Worker
```

Ou:

```text
Pipeline Event
    ↓
Webhook
    ↓
n8n
    ↓
Notification or External Automation
```

A aplicação continuará sendo responsável pela execução e persistência oficial.

---

# Configuração

Inicialmente, os pipelines serão definidos por código.

Essa abordagem oferece:

- Segurança de tipos.
    
- Facilidade de testes.
    
- Refatoração segura.
    
- Menor complexidade.
    
- Melhor controle arquitetural.
    

No futuro, partes do fluxo poderão ser configuráveis.

Exemplos:

- Ativar ou desativar etapas.
    
- Escolher provider.
    
- Definir aprovação.
    
- Alterar limite de custo.
    
- Configurar número de tentativas.
    
- Escolher idioma.
    
- Definir canal.
    
- Agendar publicação.
    

---

# Versionamento

Todo pipeline deverá possuir uma versão.

Exemplo:

```text
YouTubeLongFormPipeline v1
YouTubeLongFormPipeline v2
```

Uma execução iniciada com a versão 1 deverá permanecer vinculada à versão 1.

Alterações futuras não deverão modificar silenciosamente execuções em andamento.

---

# Compatibilidade

Ao alterar um pipeline, deverá ser avaliado:

- Mudança de entrada.
    
- Mudança de saída.
    
- Mudança de ordem.
    
- Remoção de etapas.
    
- Inclusão de etapas obrigatórias.
    
- Alteração de artefatos.
    
- Impacto em execuções existentes.
    

Mudanças incompatíveis deverão gerar uma nova versão.

---

# Testes

## Testes Unitários

Devem validar:

- Ordem das etapas.
    
- Condições.
    
- Ramificações.
    
- Interrupções.
    
- Limites de tentativas.
    
- Custos.
    
- Aprovações.
    
- Cancelamento.
    
- Fallback.
    

---

## Testes de Integração

Devem validar:

- Persistência de estado.
    
- Comunicação com fila.
    
- Execução pelo Worker.
    
- Retomada.
    
- Idempotência.
    
- Eventos.
    
- Integração entre Application, Data e Infrastructure.
    

---

## Testes de Fluxo

Devem validar cenários completos.

Exemplo:

```text
Generate
    ↓
Review
    ↓
Approve
    ↓
Publish
```

Também deverão existir testes para:

```text
Generate
    ↓
Review Failed
    ↓
Retry
    ↓
Approve
```

E:

```text
Generate
    ↓
Provider Failed
    ↓
Fallback
    ↓
Continue
```

---

# Regras Arquiteturais

- Pipelines pertencem à Application.
    
- Pipelines orquestram agentes.
    
- Agentes não orquestram pipelines.
    
- Pipelines não acessam DbContext.
    
- Pipelines utilizam abstrações.
    
- Data persiste o estado.
    
- Infrastructure implementa integrações.
    
- Workers executam pipelines.
    
- n8n dispara e acompanha automações.
    
- Toda etapa deve ser observável.
    
- Toda etapa externa deve possuir timeout.
    
- Toda repetição deve possuir limite.
    
- Toda execução longa deve ser retomável.
    
- Publicações devem ser idempotentes.
    
- Toda execução deve registrar sua versão.
    
- Custos devem ser controlados.
    
- Saídas devem ser validadas.
    
- Aprovações devem ser auditáveis.
    

---

# Exemplo Completo

```text
Usuário solicita um vídeo
    ↓
API valida a requisição
    ↓
Application cria PipelineExecution
    ↓
Data persiste a execução
    ↓
Mensagem é publicada na fila
    ↓
Worker consome a mensagem
    ↓
PipelineExecutor carrega a execução
    ↓
Trend Step
    ↓
Research Step
    ↓
Strategy Step
    ↓
Script Step
    ↓
Review Step
    ↓
Pipeline aguarda aprovação
    ↓
Usuário aprova
    ↓
Pipeline é retomado
    ↓
Voice, Thumbnail e Subtitles em paralelo
    ↓
Video Rendering
    ↓
SEO Generation
    ↓
Aprovação de publicação
    ↓
Publishing Step
    ↓
Data persiste o resultado
    ↓
Evento PipelineCompleted
    ↓
n8n envia uma notificação
```

---

# Objetivo Final

Criar pipelines confiáveis, retomáveis, observáveis e configuráveis.

A arquitetura deverá suportar processos longos, múltiplos agentes, aprovações humanas, falhas externas, controle de custos e expansão para novos formatos de conteúdo.

Novos pipelines deverão poder ser adicionados sem alterar o funcionamento dos fluxos existentes.