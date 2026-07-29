# Arquitetura dos Agentes

## Objetivo

Definir como os agentes de inteligência artificial do Infinite Content AI serão organizados, executados e integrados.

Cada agente terá uma responsabilidade específica, entradas e saídas bem definidas e dependências explícitas.

Os agentes funcionarão como especialistas dentro de um pipeline de produção de conteúdo.

---

# Visão Geral

O sistema será composto por agentes especializados.

Cada agente executará uma etapa do processo de criação de conteúdo.

Exemplo de fluxo:

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
    ↓
Translation Agent
    ↓
Voice Agent
    ↓
Thumbnail Agent
    ↓
Video Agent
    ↓
SEO Agent
    ↓
Publishing Agent
    ↓
Analytics Agent
```

Nem todos os fluxos precisarão executar todos os agentes.

A composição dependerá do tipo de conteúdo, canal, idioma e estratégia escolhida.

---

# Princípio de Responsabilidade Única

Cada agente deverá possuir uma única responsabilidade principal.

Exemplo:

- O `Research Agent` pesquisa.
    
- O `Script Agent` escreve roteiros.
    
- O `Review Agent` revisa.
    
- O `Publishing Agent` publica.
    
- O `Analytics Agent` analisa resultados.
    

Um agente não deve assumir responsabilidades de outro.

---

# O que é um Agente

Dentro do Infinite Content AI, um agente é um componente da camada Application responsável por executar uma tarefa especializada.

Um agente pode:

- Receber contexto.
    
- Consultar provedores externos.
    
- Aplicar instruções.
    
- Produzir uma saída estruturada.
    
- Registrar métricas.
    
- Retornar falhas de forma controlada.
    

Um agente não deve ser confundido com um provider.

---

# Agente versus Provider

## Agente

Representa uma capacidade de negócio.

Exemplo:

```text
ScriptAgent
```

Responsável por gerar um roteiro adequado ao público, canal e objetivo.

## Provider

Representa uma integração técnica.

Exemplo:

```text
OpenAiTextProvider
GeminiTextProvider
AnthropicTextProvider
```

Responsável apenas por conversar com um serviço externo.

---

# Relação entre Agente e Provider

```text
ScriptAgent
    ↓
IAiTextProvider
    ↓
OpenAiTextProvider
    ↓
OpenAI API
```

O agente conhece apenas a abstração.

Ele nunca deve depender diretamente de OpenAI, Gemini, Anthropic ou qualquer outro fornecedor.

---

# Localização dos Agentes

As interfaces e regras de execução dos agentes ficarão no projeto:

```text
InfiniteContent.Application
```

As integrações utilizadas pelos agentes ficarão no projeto:

```text
InfiniteContent.Infrastructure
```

A camada Domain poderá conter entidades, estados e regras relacionadas às execuções dos agentes.

---

# Estrutura Sugerida

```text
Application/
└── Agents/
    ├── Abstractions/
    │   ├── IAgent.cs
    │   ├── AgentContext.cs
    │   ├── AgentResult.cs
    │   └── AgentExecutionContext.cs
    │
    ├── Trends/
    │   └── TrendAgent.cs
    │
    ├── Research/
    │   └── ResearchAgent.cs
    │
    ├── Strategy/
    │   └── StrategyAgent.cs
    │
    ├── Script/
    │   └── ScriptAgent.cs
    │
    ├── Review/
    │   └── ReviewAgent.cs
    │
    ├── Translation/
    │   └── TranslationAgent.cs
    │
    ├── Voice/
    │   └── VoiceAgent.cs
    │
    ├── Thumbnail/
    │   └── ThumbnailAgent.cs
    │
    ├── Video/
    │   └── VideoAgent.cs
    │
    ├── Seo/
    │   └── SeoAgent.cs
    │
    ├── Publishing/
    │   └── PublishingAgent.cs
    │
    └── Analytics/
        └── AnalyticsAgent.cs
```

---

# Contrato Base

Um contrato genérico poderá ser utilizado quando trouxer clareza.

Exemplo conceitual:

```csharp
public interface IAgent<in TInput, TOutput>
{
    Task<Result<TOutput>> ExecuteAsync(
        TInput input,
        AgentExecutionContext context,
        CancellationToken cancellationToken);
}
```

O contrato deverá ser simples e não poderá forçar agentes diferentes a possuírem comportamentos artificiais.

Interfaces específicas poderão ser utilizadas quando forem mais claras.

---

# Contexto de Execução

Toda execução deverá receber um contexto.

Exemplo conceitual:

```csharp
public sealed record AgentExecutionContext(
    Guid ExecutionId,
    Guid ProjectId,
    string Language,
    string TargetAudience,
    string Channel,
    string CorrelationId);
```

O contexto poderá transportar informações como:

- Identificador da execução.
    
- Identificador do projeto.
    
- Canal de publicação.
    
- Idioma.
    
- Público-alvo.
    
- Estratégia.
    
- Correlation ID.
    
- Limites de custo.
    
- Preferência de provider.
    
- Configurações do pipeline.
    

---

# Resultado do Agente

Todo agente deverá retornar uma resposta estruturada.

Exemplo:

```csharp
public sealed record AgentResult<T>(
    T Output,
    AgentExecutionMetrics Metrics,
    IReadOnlyCollection<string> Warnings);
```

O resultado poderá conter:

- Saída produzida.
    
- Provider utilizado.
    
- Modelo utilizado.
    
- Tokens consumidos.
    
- Custo estimado.
    
- Tempo de execução.
    
- Avisos.
    
- Metadados.
    
- Indicadores de qualidade.
    

---

# Agentes Iniciais

## Trend Agent

### Responsabilidade

Identificar tendências, temas e oportunidades de conteúdo.

### Entradas

- Nicho.
    
- Idioma.
    
- Região.
    
- Canal.
    
- Período analisado.
    
- Palavras-chave.
    

### Saídas

- Lista de tendências.
    
- Grau de relevância.
    
- Potencial de audiência.
    
- Concorrência estimada.
    
- Justificativa.
    
- Fontes utilizadas.
    

### Não deve

- Escrever o roteiro final.
    
- Criar mídia.
    
- Publicar conteúdo.
    

---

## Research Agent

### Responsabilidade

Pesquisar informações confiáveis sobre o tema escolhido.

### Entradas

- Tema.
    
- Público.
    
- Idioma.
    
- Profundidade desejada.
    
- Fontes permitidas.
    

### Saídas

- Resumo da pesquisa.
    
- Fatos relevantes.
    
- Dados.
    
- Referências.
    
- Pontos de atenção.
    
- Lacunas encontradas.
    

### Não deve

- Inventar fontes.
    
- Publicar conteúdo.
    
- Definir sozinho a estratégia editorial.
    

---

## Strategy Agent

### Responsabilidade

Transformar uma oportunidade em uma estratégia de conteúdo.

### Entradas

- Tendência.
    
- Pesquisa.
    
- Canal.
    
- Público-alvo.
    
- Objetivo do conteúdo.
    

### Saídas

- Ângulo do conteúdo.
    
- Formato.
    
- Tom.
    
- Estrutura.
    
- Duração estimada.
    
- Chamada para ação.
    
- Hipótese de desempenho.
    

---

## Script Agent

### Responsabilidade

Criar o roteiro do conteúdo.

### Entradas

- Estratégia.
    
- Pesquisa.
    
- Público-alvo.
    
- Canal.
    
- Idioma.
    
- Duração.
    
- Tom de voz.
    

### Saídas

- Título provisório.
    
- Hook.
    
- Introdução.
    
- Desenvolvimento.
    
- Conclusão.
    
- Chamada para ação.
    
- Texto completo.
    
- Marcação de cenas, quando aplicável.
    

### Não deve

- Publicar.
    
- Gerar áudio.
    
- Alterar dados da pesquisa sem sinalização.
    

---

## Review Agent

### Responsabilidade

Revisar e avaliar o conteúdo produzido.

### Entradas

- Roteiro.
    
- Pesquisa.
    
- Regras editoriais.
    
- Critérios de qualidade.
    
- Público-alvo.
    

### Saídas

- Conteúdo aprovado ou rejeitado.
    
- Pontuação.
    
- Problemas encontrados.
    
- Correções sugeridas.
    
- Versão revisada, quando permitido.
    

### Critérios possíveis

- Clareza.
    
- Coerência.
    
- Precisão.
    
- Adequação ao público.
    
- Originalidade.
    
- Retenção.
    
- Segurança.
    
- Conformidade editorial.
    

---

## Translation Agent

### Responsabilidade

Traduzir e localizar o conteúdo.

### Entradas

- Conteúdo original.
    
- Idioma de origem.
    
- Idioma de destino.
    
- Público.
    
- Região.
    
- Tom de voz.
    

### Saídas

- Conteúdo traduzido.
    
- Adaptações culturais.
    
- Termos preservados.
    
- Alertas de localização.
    

O agente deverá priorizar localização, não tradução literal.

---

## Voice Agent

### Responsabilidade

Gerar ou preparar a narração.

### Entradas

- Roteiro final.
    
- Voz.
    
- Idioma.
    
- Ritmo.
    
- Entonação.
    
- Formato de áudio.
    

### Saídas

- Arquivo de áudio.
    
- Duração.
    
- Metadados da voz.
    
- Marcadores de tempo.
    
- Custo da geração.
    

---

## Thumbnail Agent

### Responsabilidade

Criar instruções ou imagens para a capa do conteúdo.

### Entradas

- Tema.
    
- Título.
    
- Público.
    
- Canal.
    
- Identidade visual.
    
- Restrições de texto.
    

### Saídas

- Prompt visual.
    
- Composição sugerida.
    
- Texto da thumbnail.
    
- Imagem gerada.
    
- Variações para teste.
    

---

## Video Agent

### Responsabilidade

Montar ou preparar a composição do vídeo.

### Entradas

- Narração.
    
- Roteiro.
    
- Imagens.
    
- Clipes.
    
- Legendas.
    
- Formato do canal.
    

### Saídas

- Arquivo de vídeo.
    
- Timeline.
    
- Legendas.
    
- Metadados.
    
- Duração final.
    
- Relatório de renderização.
    

---

## SEO Agent

### Responsabilidade

Otimizar a descoberta do conteúdo.

### Entradas

- Conteúdo.
    
- Canal.
    
- Tema.
    
- Público.
    
- Palavras-chave.
    
- Tendências.
    

### Saídas

- Título otimizado.
    
- Descrição.
    
- Tags.
    
- Palavras-chave.
    
- Hashtags.
    
- Capítulos.
    
- Sugestões de posicionamento.
    

---

## Publishing Agent

### Responsabilidade

Publicar ou agendar conteúdo nas plataformas.

### Entradas

- Conteúdo aprovado.
    
- Arquivo de mídia.
    
- Metadados.
    
- Plataforma.
    
- Data de publicação.
    
- Configurações de privacidade.
    

### Saídas

- Identificador da publicação.
    
- URL.
    
- Data e hora.
    
- Status.
    
- Resposta da plataforma.
    

### Não deve

- Publicar conteúdo não aprovado.
    
- Alterar roteiro.
    
- Ignorar regras de agendamento.
    

---

## Analytics Agent

### Responsabilidade

Analisar o desempenho dos conteúdos publicados.

### Entradas

- Métricas da plataforma.
    
- Conteúdo publicado.
    
- Estratégia utilizada.
    
- Período.
    
- Metas.
    

### Saídas

- Resumo de desempenho.
    
- Retenção.
    
- Cliques.
    
- Engajamento.
    
- Conversão.
    
- Pontos fortes.
    
- Pontos fracos.
    
- Recomendações.
    
- Novas hipóteses.
    

---

# Orquestração

Os agentes não deverão chamar uns aos outros diretamente por padrão.

A orquestração será realizada por um caso de uso, pipeline ou serviço de aplicação.

Exemplo:

```text
GenerateContentHandler
    ↓
TrendAgent
    ↓
ResearchAgent
    ↓
StrategyAgent
    ↓
ScriptAgent
    ↓
ReviewAgent
```

Essa abordagem evita acoplamento entre agentes.

---

# Pipeline

Um pipeline representa uma sequência configurável de agentes.

Exemplo:

```text
YouTubeLongFormPipeline
```

Etapas:

```text
Trend
Research
Strategy
Script
Review
Voice
Thumbnail
Video
SEO
Publishing
```

Outro exemplo:

```text
ShortVideoPipeline
```

Etapas:

```text
Trend
Script
Review
Voice
Video
SEO
Publishing
```

---

# Estrutura de Pipeline

```text
Application/
└── Pipelines/
    ├── Abstractions/
    ├── YouTubeLongForm/
    ├── ShortVideo/
    ├── Article/
    └── SocialPost/
```

Um pipeline deve decidir:

- Quais agentes executar.
    
- Em qual ordem.
    
- Quais etapas são opcionais.
    
- Quando interromper.
    
- Quando solicitar aprovação.
    
- Como tratar falhas.
    
- Como registrar o progresso.
    

---

# Estado da Execução

Cada execução deverá possuir um estado persistido.

Exemplo:

```text
Pending
Running
WaitingForApproval
Completed
Failed
Cancelled
```

Cada etapa também deverá possuir um estado.

Exemplo:

```text
NotStarted
Running
Succeeded
Failed
Skipped
Cancelled
```

---

# Aprovação Humana

O sistema deverá permitir pontos de aprovação humana.

Exemplos:

```text
Script gerado
    ↓
Aprovação manual
    ↓
Geração de voz e vídeo
```

Ou:

```text
Vídeo finalizado
    ↓
Aprovação manual
    ↓
Publicação
```

A aprovação poderá ser obrigatória ou opcional, de acordo com o projeto.

---

# Falhas e Retentativas

Cada agente deverá possuir uma política de falha.

Uma execução poderá:

- Tentar novamente.
    
- Trocar de provider.
    
- Utilizar outro modelo.
    
- Marcar a etapa como falha.
    
- Solicitar intervenção humana.
    
- Interromper o pipeline.
    

Retentativas não deverão ser infinitas.

---

# Estratégia de Fallback

Exemplo:

```text
OpenAI falhou
    ↓
Tentar novamente
    ↓
Gemini
    ↓
Anthropic
    ↓
Falha controlada
```

A estratégia de fallback deverá respeitar:

- Custo máximo.
    
- Provider habilitado.
    
- Tipo de tarefa.
    
- Requisitos de qualidade.
    
- Disponibilidade.
    
- Limites de uso.
    

---

# Idempotência

A execução de um agente deverá ser idempotente sempre que possível.

Reexecutar uma etapa não deve criar:

- Publicações duplicadas.
    
- Cobranças duplicadas evitáveis.
    
- Registros inconsistentes.
    
- Arquivos conflitantes.
    
- Eventos duplicados.
    

Operações sensíveis deverão utilizar uma chave de idempotência.

---

# Observabilidade

Toda execução de agente deverá registrar:

- Nome do agente.
    
- Identificador da execução.
    
- Início.
    
- Fim.
    
- Duração.
    
- Status.
    
- Provider.
    
- Modelo.
    
- Tokens.
    
- Custo.
    
- Erros.
    
- Número de tentativas.
    
- Correlation ID.
    

---

# Métricas

Métricas técnicas:

- Tempo médio por agente.
    
- Taxa de sucesso.
    
- Taxa de falha.
    
- Número de retentativas.
    
- Custo médio.
    
- Uso por provider.
    

Métricas de qualidade:

- Taxa de aprovação.
    
- Quantidade de revisões.
    
- Pontuação média.
    
- Taxa de publicação.
    
- Desempenho do conteúdo.
    

---

# Prompts

Os prompts não deverão ficar espalhados no código.

Eles poderão ser armazenados em:

- Arquivos versionados.
    
- Banco de dados.
    
- Sistema de templates.
    
- Serviço especializado.
    

Todo prompt deverá possuir:

- Identificador.
    
- Nome.
    
- Versão.
    
- Agente relacionado.
    
- Modelo recomendado.
    
- Variáveis.
    
- Data de criação.
    
- Status.
    

---

# Versionamento

Agentes, prompts e pipelines deverão ser versionáveis.

Exemplo:

```text
ScriptAgent v1
ScriptPrompt v3
YouTubeLongFormPipeline v2
```

Uma execução deverá registrar quais versões foram utilizadas.

Isso permitirá:

- Reproduzir resultados.
    
- Comparar versões.
    
- Executar testes A/B.
    
- Identificar regressões.
    
- Medir evolução.
    

---

# Segurança

Os agentes deverão respeitar:

- Limites de custo.
    
- Limites de tokens.
    
- Tipos de conteúdo permitidos.
    
- Regras de moderação.
    
- Proteção contra prompt injection.
    
- Validação das respostas.
    
- Controle de acesso.
    
- Proteção de segredos.
    

Nenhuma saída de IA deverá ser considerada confiável sem validação.

---

# Saídas Estruturadas

Sempre que possível, agentes de texto deverão retornar respostas estruturadas.

Exemplo:

```json
{
  "title": "Exemplo de título",
  "hook": "Texto de abertura",
  "sections": [
    {
      "name": "Introdução",
      "content": "Conteúdo"
    }
  ],
  "callToAction": "Inscreva-se no canal"
}
```

As respostas deverão ser validadas antes de seguir para a próxima etapa.

---

# Regras Arquiteturais

- Agentes não conhecem implementações de providers.
    
- Agentes não acessam banco de dados diretamente.
    
- Agentes não publicam eventos diretamente sem abstração.
    
- Agentes não conhecem a API.
    
- Agentes não dependem do Worker.
    
- Agentes não chamam outros agentes diretamente por padrão.
    
- Pipelines orquestram agentes.
    
- Application define contratos.
    
- Infrastructure implementa providers.
    
- Data persiste execuções.
    
- Domain protege regras e estados.
    
- Toda execução deve ser observável.
    
- Toda saída externa deve ser validada.
    

---

# Exemplo de Fluxo Completo

```text
Usuário solicita conteúdo
    ↓
API recebe a requisição
    ↓
Application cria a execução
    ↓
Data persiste o estado inicial
    ↓
Pipeline é iniciado
    ↓
Trend Agent encontra oportunidade
    ↓
Research Agent reúne informações
    ↓
Strategy Agent define a abordagem
    ↓
Script Agent gera o roteiro
    ↓
Review Agent valida o conteúdo
    ↓
Usuário aprova o roteiro
    ↓
Voice Agent gera a narração
    ↓
Thumbnail Agent cria a capa
    ↓
Video Agent monta o vídeo
    ↓
SEO Agent gera os metadados
    ↓
Usuário aprova a publicação
    ↓
Publishing Agent publica
    ↓
Analytics Agent coleta resultados
    ↓
Sistema gera aprendizados
```

---

# Objetivo Final

Criar uma arquitetura de agentes desacoplada, observável, extensível e preparada para múltiplos provedores, canais, idiomas e formatos.

O sistema deverá permitir adicionar novos agentes e pipelines sem alterar o núcleo existente.