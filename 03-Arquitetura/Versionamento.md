# Versionamento

## Objetivo

Definir como os elementos do Infinite Content AI evoluirão sem quebrar integrações, execuções em andamento, consumidores, dados históricos ou comportamentos já publicados.

O versionamento deverá permitir evolução contínua da plataforma com:

- Compatibilidade.
    
- Rastreabilidade.
    
- Reprodutibilidade.
    
- Rollback.
    
- Migração controlada.
    
- Depreciação explícita.
    
- Auditoria.
    
- Suporte a execuções antigas.
    
- Evolução independente entre componentes.
    

O sistema não deverá tratar versionamento apenas como um número na API.

Versionamento será uma capacidade transversal aplicada a:

- APIs.
    
- Contratos.
    
- Commands.
    
- Events.
    
- Pipelines.
    
- Steps.
    
- Agents.
    
- Prompts.
    
- Structured Outputs.
    
- Schemas.
    
- Providers.
    
- Modelos.
    
- Configurações.
    
- Artefatos.
    
- Banco de dados.
    
- Workflows do n8n.
    
- Integrações externas.
    
- Regras de negócio.
    
- Definições editoriais.
    

A pergunta central será:

> Como alterar qualquer parte do sistema sem tornar o estado existente inválido?

---

# Princípios

A estratégia de versionamento seguirá os seguintes princípios:

- Alterações incompatíveis exigem nova versão.
    
- Contratos publicados deverão ser considerados imutáveis.
    
- Execuções longas deverão utilizar snapshots.
    
- Versões deverão ser explícitas.
    
- Histórico não deverá ser sobrescrito.
    
- Migrações deverão ser controladas.
    
- Rollback deverá ser considerado antes do deploy.
    
- Depreciações deverão possuir prazo.
    
- Compatibilidade deverá ser testada.
    
- Consumidores deverão tolerar evolução compatível.
    
- A versão técnica não deverá depender apenas da versão do deploy.
    
- Mudanças de comportamento deverão ser rastreáveis.
    
- Versões antigas deverão possuir política de suporte.
    
- O sistema deverá saber qual versão produziu cada resultado.
    
- Configurações e modelos utilizados deverão ser registrados.
    
- Alterações silenciosas de comportamento deverão ser evitadas.
    

---

# Por que Versionar

Sem versionamento explícito, mudanças aparentemente simples podem causar:

- Quebra de consumidores.
    
- Pipelines impossíveis de retomar.
    
- Resultados impossíveis de reproduzir.
    
- Prompts antigos produzindo saídas diferentes.
    
- Eventos incompatíveis.
    
- Jobs antigos sendo processados por código novo.
    
- Dados históricos sem contexto.
    
- Publicações duplicadas.
    
- Configurações inconsistentes.
    
- Dificuldade de rollback.
    
- Falhas em integrações externas.
    
- Incompatibilidade com workflows do n8n.
    
- Corrupção de estado.
    

---

# Escopos de Versionamento

O Infinite Content AI deverá distinguir versões diferentes.

```text
Versão da Aplicação
Versão da API
Versão de Contrato
Versão de Mensagem
Versão de Pipeline
Versão de Step
Versão de Agent
Versão de Prompt
Versão de Schema
Versão de Provider Policy
Versão de Configuração
Versão de Artefato
Versão de Banco
Versão de Workflow
```

Essas versões não deverão ser confundidas.

---

# Versão da Aplicação

A versão da aplicação identifica o build ou release implantado.

Exemplo:

```text
Infinite Content AI 1.8.3
```

Ela poderá seguir Semantic Versioning.

Formato:

```text
MAJOR.MINOR.PATCH
```

Exemplo:

```text
2.4.1
```

---

# Semantic Versioning

## MAJOR

Alteração incompatível.

Exemplos:

- Remoção de API.
    
- Mudança incompatível de contrato.
    
- Alteração de comportamento central.
    
- Migração irreversível.
    
- Remoção de suporte antigo.
    

## MINOR

Nova funcionalidade compatível.

Exemplos:

- Novo endpoint.
    
- Novo campo opcional.
    
- Novo pipeline.
    
- Novo provider.
    
- Nova capability.
    

## PATCH

Correção compatível.

Exemplos:

- Correção de bug.
    
- Melhoria de performance.
    
- Ajuste interno.
    
- Correção de observabilidade.
    

---

# Versão do Build

Além da versão semântica, o sistema poderá registrar:

- Commit SHA.
    
- Build number.
    
- Branch.
    
- Build timestamp.
    
- Image digest.
    
- Release ID.
    
- Deployment ID.
    

Exemplo:

```text
Version: 1.8.3
Commit: a82f7c4
Build: 1842
ImageDigest: sha256:...
```

---

# Exposição da Versão

A versão poderá ser exposta em:

- Health endpoint.
    
- Info endpoint.
    
- Logs de startup.
    
- Traces.
    
- Métricas.
    
- Dashboards.
    
- Respostas administrativas.
    

Exemplo:

```json
{
  "service": "InfiniteContent.Api",
  "version": "1.8.3",
  "commit": "a82f7c4",
  "environment": "Production"
}
```

---

# Versionamento da API

A API deverá possuir estratégia explícita de versionamento.

Possibilidades:

- URL.
    
- Header.
    
- Query string.
    
- Media type.
    

Recomendação inicial:

```text
/api/v1/
```

Exemplo:

```text
GET /api/v1/projects
POST /api/v1/pipeline-executions
```

---

# Versão na URL

Benefícios:

- Visível.
    
- Fácil de documentar.
    
- Simples para clientes.
    
- Fácil de rotear.
    
- Adequada para OpenAPI.
    

Desvantagens:

- Aumenta duplicação.
    
- Pode incentivar versões permanentes.
    
- Exige estratégia clara de depreciação.
    

---

# Versão por Header

Exemplo:

```http
X-Api-Version: 1
```

ou:

```http
Accept: application/vnd.infinitecontent.v1+json
```

Pode ser útil em cenários avançados.

Para o MVP, a versão na URL tende a ser mais simples.

---

# O que Deve Gerar Nova Versão de API

Exemplos:

- Remover campo.
    
- Renomear campo.
    
- Alterar tipo.
    
- Alterar semântica.
    
- Alterar código de status de forma incompatível.
    
- Alterar autenticação.
    
- Alterar paginação.
    
- Tornar campo opcional em obrigatório.
    
- Alterar unidade de medida.
    
- Alterar formato de data.
    
- Alterar comportamento esperado.
    

---

# Alterações Compatíveis na API

Exemplos:

- Adicionar endpoint.
    
- Adicionar campo opcional.
    
- Adicionar filtro opcional.
    
- Adicionar header opcional.
    
- Adicionar enum tolerado por clientes.
    
- Melhorar mensagem sem mudar código.
    
- Corrigir documentação.
    

---

# Contratos HTTP

Contratos HTTP deverão ser separados das entidades do domínio.

Estrutura:

```text
Api/
└── Contracts/
    ├── V1/
    │   ├── Requests/
    │   └── Responses/
    └── V2/
        ├── Requests/
        └── Responses/
```

Alternativamente, a versão poderá ser organizada por feature.

---

# Mapeamento entre Versões

Cada versão da API poderá mapear para o mesmo caso de uso interno.

Exemplo:

```text
CreateProjectRequestV1
    ↓
CreateProjectCommand
```

```text
CreateProjectRequestV2
    ↓
CreateProjectCommand
```

A Application não deverá ser duplicada automaticamente apenas porque a API mudou.

---

# Problem Details Versionado

O formato de erro também deverá ser compatível.

Exemplo:

```json
{
  "type": "https://errors.infinitecontent.ai/project-not-found",
  "title": "Project not found",
  "status": 404,
  "code": "project_not_found",
  "traceId": "..."
}
```

Os códigos de erro deverão permanecer estáveis.

---

# Depreciação de API

Uma versão não deverá ser removida sem processo de depreciação.

Etapas:

1. Marcar como deprecated.
    
2. Publicar documentação.
    
3. Informar consumidores.
    
4. Definir prazo.
    
5. Medir uso.
    
6. Disponibilizar substituição.
    
7. Bloquear novos consumidores.
    
8. Remover após janela definida.
    

---

# Headers de Depreciação

A API poderá retornar:

```http
Deprecation: true
Sunset: Wed, 31 Dec 2027 23:59:59 GMT
```

Também poderá retornar um link para documentação de migração.

---

# Política de Suporte da API

Exemplo inicial:

```text
Versão atual
    Suporte completo

Versão anterior
    Correções críticas

Versões mais antigas
    Depreciadas ou removidas
```

A política deverá ser registrada em ADR.

---

# OpenAPI por Versão

Cada versão deverá possuir documentação separada.

Exemplo:

```text
/openapi/v1.json
/openapi/v2.json
```

A documentação deverá indicar:

- Status.
    
- Data de lançamento.
    
- Data de depreciação.
    
- Data de sunset.
    
- Guia de migração.
    

---

# Versionamento de Commands

Commands compartilhados entre processos deverão possuir versão.

Exemplo:

```text
commands.pipelines.start.v1
```

Um command publicado não deverá ter seu significado alterado.

---

# Alteração de Command

Exemplo V1:

```csharp
public sealed record StartPipelineCommandV1(
    Guid MessageId,
    Guid PipelineExecutionId,
    Guid OrganizationId);
```

Exemplo V2:

```csharp
public sealed record StartPipelineCommandV2(
    Guid MessageId,
    Guid PipelineExecutionId,
    Guid OrganizationId,
    int PipelineVersion,
    Guid RequestedBy);
```

---

# Compatibilidade de Commands

Adicionar campo opcional pode ser compatível.

Adicionar campo obrigatório pode exigir nova versão.

Modificar a semântica de um campo sempre exige análise de compatibilidade.

---

# Versionamento de Events

Integration Events deverão possuir versão explícita.

Exemplo:

```text
events.pipelines.completed.v1
```

ou:

```json
{
  "messageType": "pipelines.completed",
  "messageVersion": 1
}
```

---

# Imutabilidade de Events

Um evento já publicado representa um fato histórico.

Seu contrato não deverá ser alterado retroativamente.

Se novas informações forem necessárias:

- Criar nova versão.
    
- Criar novo evento.
    
- Publicar evento complementar.
    
- Enriquecer projeção por consulta.
    

---

# Event V1 e V2

Exemplo:

```csharp
public sealed record ContentPublishedV1(
    Guid PublicationId,
    Guid ContentId,
    DateTimeOffset PublishedAt);
```

```csharp
public sealed record ContentPublishedV2(
    Guid PublicationId,
    Guid ContentId,
    string Platform,
    string ExternalPublicationId,
    DateTimeOffset PublishedAt);
```

---

# Conversão entre Eventos

Um adapter poderá converter V1 para modelo interno.

```text
ContentPublishedV1
    ↓
Message Adapter
    ↓
Internal PublicationCompleted
```

Isso permite manter consumers antigos e novos durante migração.

---

# Upcasting

Upcasting transforma uma mensagem antiga em uma representação atual.

Exemplo:

```text
Event V1
    ↓
Upcaster
    ↓
Event V2 interno
```

O upcaster deverá:

- Ser determinístico.
    
- Não depender de estado externo quando possível.
    
- Preservar significado.
    
- Possuir testes.
    
- Ser versionado.
    

---

# Downcasting

Downcasting deve ser evitado.

Transformar uma versão nova em antiga pode causar perda de informação.

Quando necessário, deverá ser explícito e testado.

---

# Compatibilidade de Mensagens

Consumidores deverão:

- Ignorar campos desconhecidos.
    
- Validar campos obrigatórios.
    
- Rejeitar versões não suportadas.
    
- Não depender da ordem do JSON.
    
- Não assumir presença de campos opcionais.
    
- Tratar enums desconhecidos com segurança.
    

---

# Versões não Suportadas

Ao receber versão não suportada:

- Não processar silenciosamente.
    
- Registrar erro.
    
- Enviar para DLQ ou parking queue.
    
- Emitir alerta.
    
- Preservar payload.
    
- Informar o motivo.
    

Código sugerido:

```text
message_version_not_supported
```

---

# Versionamento da Outbox

A Outbox deverá persistir:

- MessageType.
    
- MessageVersion.
    
- Payload.
    
- SerializerVersion.
    
- ContractAssemblyVersion, quando necessário.
    

Isso garante que mensagens antigas possam ser publicadas corretamente após deploy.

---

# Deploy com Outbox Pendente

Problema:

```text
Aplicação V1 salva mensagem V1
    ↓
Deploy V2 acontece
    ↓
Outbox V2 tenta publicar payload antigo
```

A implementação deverá garantir suporte à serialização e publicação de contratos antigos pendentes.

---

# Versionamento da Inbox

A Inbox deverá registrar:

- MessageId.
    
- MessageType.
    
- MessageVersion.
    
- ConsumerVersion.
    
- ProcessedAt.
    
- Status.
    

Isso ajuda a diagnosticar qual versão do consumer processou a mensagem.

---

# Versionamento de Pipelines

Cada definição de pipeline deverá possuir versão explícita.

Exemplo:

```text
YouTubeLongForm v1
YouTubeLongForm v2
YouTubeLongForm v3
```

Uma versão publicada de pipeline deverá ser imutável.

---

# Pipeline Definition

Estrutura conceitual:

```text
pipeline_definitions
├── id
├── name
├── version
├── status
├── definition
├── created_at
├── published_at
├── deprecated_at
└── created_by
```

Constraint recomendada:

```text
unique(name, version)
```

---

# Estados da Definição

Exemplo:

```text
Draft
Published
Deprecated
Archived
```

Apenas versões `Published` poderão iniciar novas execuções.

---

# Imutabilidade de Pipeline

Após publicação:

- Steps não deverão mudar.
    
- Ordem não deverá mudar.
    
- Timeouts não deverão mudar silenciosamente.
    
- Regras de retry não deverão mudar.
    
- Contratos de entrada e saída não deverão mudar.
    
- Políticas de aprovação não deverão mudar.
    

Alterações exigem nova versão.

---

# Pipeline em Execução

Uma execução deverá registrar:

- PipelineDefinitionId.
    
- PipelineName.
    
- PipelineVersion.
    
- ConfigurationSnapshot.
    
- AgentVersions.
    
- PromptVersions.
    
- StepVersions.
    
- ProviderPolicyVersion.
    

---

# Regra de Execução

Uma execução iniciada em:

```text
YouTubeLongForm v2
```

deverá continuar em:

```text
YouTubeLongForm v2
```

mesmo que a versão v3 seja publicada.

---

# Migração de Execuções em Andamento

Migrar uma execução em andamento deverá ser excepcional.

Antes da migração, validar:

- Compatibilidade de estado.
    
- Steps concluídos.
    
- Inputs.
    
- Outputs.
    
- Artefatos.
    
- Checkpoints.
    
- Aprovações.
    
- Políticas de custo.
    
- Idempotência.
    
- Compensações.
    

---

# Estratégias de Migração de Pipeline

## Não Migrar

A execução termina na versão original.

É a estratégia padrão.

## Migração Manual

Um operador decide migrar uma execução específica.

## Migração Automática Compatível

Permitida somente quando formalmente comprovada.

## Reiniciar em Nova Versão

Criar nova execução e manter a antiga como cancelada ou superseded.

---

# Pipeline Compatibility Policy

Uma nova versão deverá declarar:

```text
CompatibleWithPreviousState: false
```

ou:

```text
CompatibleWithPreviousState: true
```

Essa informação sozinha não será suficiente.

A compatibilidade deverá possuir validação técnica.

---

# Versionamento de Steps

Steps reutilizáveis deverão possuir versão.

Exemplo:

```text
GenerateScript v1
GenerateScript v2
```

A versão poderá mudar por:

- Novo input.
    
- Novo output.
    
- Nova regra.
    
- Novo comportamento.
    
- Nova estratégia de retry.
    
- Nova forma de validação.
    

---

# Step Definition

Exemplo:

```csharp
public sealed record PipelineStepDescriptor(
    string Name,
    int Version,
    string InputSchemaVersion,
    string OutputSchemaVersion,
    bool IsRequired);
```

---

# Step Handler Registry

O executor poderá resolver handlers por nome e versão.

```text
GenerateScript v1
    ↓
GenerateScriptStepHandlerV1
```

```text
GenerateScript v2
    ↓
GenerateScriptStepHandlerV2
```

---

# Suporte a Steps Antigos

Handlers antigos poderão precisar permanecer disponíveis enquanto existirem:

- Execuções em andamento.
    
- Reprocessamentos.
    
- Checkpoints históricos.
    
- Mensagens pendentes.
    
- Reproduções.
    
- Auditorias.
    

A remoção deverá respeitar uma política de retenção.

---

# Versionamento de Agents

Agents deverão possuir versão comportamental.

Exemplo:

```text
ResearchAgent v1
ResearchAgent v2
ScriptAgent v4
```

A versão do Agent representa o conjunto de:

- Prompt.
    
- Ferramentas.
    
- Policies.
    
- Modelo de entrada.
    
- Modelo de saída.
    
- Validações.
    
- Estratégia de decisão.
    
- Configuração padrão.
    

---

# Agent Definition

Estrutura conceitual:

```text
agent_definitions
├── id
├── name
├── version
├── status
├── input_schema_version
├── output_schema_version
├── prompt_version
├── tool_policy_version
├── provider_policy_version
├── created_at
└── published_at
```

---

# Imutabilidade de Agent

Uma versão publicada de Agent não deverá mudar silenciosamente.

Se o prompt, tools ou comportamento mudarem de forma relevante, criar nova versão.

---

# Alteração Compatível em Agent

Nem toda alteração interna exige nova versão pública.

Exemplos possivelmente compatíveis:

- Correção de log.
    
- Otimização de cache.
    
- Correção de timeout sem alterar resultado esperado.
    
- Melhoria de observabilidade.
    
- Correção de bug que restaura comportamento documentado.
    

A equipe deverá avaliar impacto semântico.

---

# Alteração Incompatível em Agent

Exemplos:

- Mudar formato de saída.
    
- Mudar critério de decisão.
    
- Adicionar nova ferramenta com efeito externo.
    
- Alterar política de segurança.
    
- Alterar tom editorial.
    
- Alterar idioma padrão.
    
- Alterar modelo de raciocínio.
    
- Alterar comportamento de fallback.
    

---

# Agent Registry

Poderá existir um registry:

```csharp
public interface IAgentRegistry
{
    IAgent Resolve(
        string agentName,
        int agentVersion);
}
```

---

# Versionamento de Prompts

Prompts deverão ser tratados como artefatos versionados.

Exemplo:

```text
ScriptGenerationPrompt v1
ScriptGenerationPrompt v2
```

Prompts não deverão ficar espalhados como strings em código sem identificação.

---

# Prompt Definition

Estrutura conceitual:

```text
prompt_definitions
├── id
├── name
├── version
├── status
├── template
├── variables_schema
├── output_schema_version
├── model_constraints
├── created_at
├── published_at
└── created_by
```

---

# Estados de Prompt

```text
Draft
Testing
Published
Deprecated
Archived
```

---

# Prompt Publicado

Após publicação:

- Template não deverá ser alterado.
    
- Variáveis não deverão mudar.
    
- Instruções não deverão mudar.
    
- Schema esperado não deverá mudar.
    
- Critérios de validação não deverão mudar.
    

Qualquer alteração relevante gera nova versão.

---

# Prompt Snapshot

A execução deverá registrar:

- PromptName.
    
- PromptVersion.
    
- PromptHash.
    
- Variáveis resolvidas.
    
- Modelo.
    
- Provider.
    
- Parâmetros relevantes.
    

O snapshot deverá respeitar políticas de segurança e privacidade.

---

# Prompt Hash

Um hash ajuda a detectar alterações indevidas.

Exemplo:

```text
SHA-256(template normalizado)
```

A execução poderá registrar:

```text
prompt_hash
```

---

# Templates e Variáveis

A versão deverá definir claramente as variáveis.

Exemplo:

```json
{
  "topic": "string",
  "audience": "string",
  "language": "string",
  "durationMinutes": "integer"
}
```

Adicionar variável obrigatória exige nova versão.

---

# A/B Testing de Prompts

Duas versões poderão coexistir.

Exemplo:

```text
ScriptPrompt v4 → 90%
ScriptPrompt v5 → 10%
```

A execução deverá registrar qual versão foi utilizada.

---

# Rollback de Prompt

Rollback não significa editar a versão atual.

Significa selecionar novamente uma versão anterior para novas execuções.

Exemplo:

```text
Current: v5
Rollback target: v4
```

Execuções existentes permanecem com seu snapshot.

---

# Versionamento de Structured Outputs

Toda saída estruturada deverá possuir schema versionado.

Exemplo:

```text
script-output.v1
script-output.v2
seo-metadata.v3
```

---

# JSON Schema

Schemas poderão ser mantidos como JSON Schema.

Estrutura:

```text
Contracts/
└── Schemas/
    ├── ScriptOutput/
    │   ├── v1.json
    │   └── v2.json
    └── SeoMetadata/
        ├── v1.json
        └── v2.json
```

---

# Schema Version

A saída deverá indicar sua versão quando necessário.

```json
{
  "schemaVersion": 2,
  "title": "...",
  "sections": []
}
```

---

# Alteração de Schema

Nova versão será necessária ao:

- Remover campo.
    
- Renomear campo.
    
- Alterar tipo.
    
- Alterar enum.
    
- Alterar obrigatoriedade.
    
- Alterar estrutura.
    
- Alterar unidade.
    
- Alterar significado.
    

---

# Campos Opcionais

Adicionar campo opcional pode ser compatível.

Porém, os consumidores deverão ignorar campos desconhecidos.

---

# Migração de Schema

Um migrator poderá transformar dados antigos.

```csharp
public interface ISchemaMigrator<TSource, TTarget>
{
    TTarget Migrate(TSource source);
}
```

---

# Cadeia de Migração

Exemplo:

```text
Schema v1
    ↓
Migrator v1 → v2
    ↓
Schema v2
    ↓
Migrator v2 → v3
    ↓
Schema v3
```

Evitar migradores diretos entre todas as combinações.

---

# Versionamento de Providers

Providers externos possuem seu próprio ciclo de evolução.

Exemplos:

- API version.
    
- Model version.
    
- Endpoint version.
    
- SDK version.
    
- Capability version.
    

A Infrastructure deverá isolar essas mudanças.

---

# Provider Adapter

A Application não deverá conhecer:

- API version externa.
    
- Nome específico de endpoint.
    
- Estrutura proprietária.
    
- Formato de erro externo.
    
- Versão de SDK.
    

O adapter fará a normalização.

---

# Provider API Version

Exemplo:

```text
OpenAI API Version
Gemini API Version
Anthropic API Version
Azure API Version
```

A versão utilizada deverá ser configurável quando suportado.

---

# Versionamento de Modelos

Modelos de IA podem ser:

- Atualizados.
    
- Substituídos.
    
- Depreciados.
    
- Removidos.
    
- Redirecionados por alias.
    
- Alterados pelo provider.
    

Não se deve assumir que o mesmo nome sempre produz o mesmo comportamento.

---

# Model Catalog

O catálogo deverá registrar:

```text
provider
model_id
model_version
capabilities
released_at
deprecated_at
retired_at
context_window
pricing_version
availability
```

---

# Aliases de Modelo

Aliases como:

```text
latest
default
recommended
```

deverão ser usados com cautela.

Para reprodutibilidade, preferir um identificador específico quando disponível.

---

# Mudança Silenciosa do Provider

Quando um provider altera o comportamento de um modelo sem mudar o identificador, a reprodução perfeita pode ser impossível.

Nesse caso, registrar:

- ModelId.
    
- Provider request ID.
    
- Timestamp.
    
- Parâmetros.
    
- Região.
    
- SDK version.
    
- Provider API version.
    
- Response metadata.
    

---

# Depreciação de Modelo

Ao detectar depreciação:

1. Marcar modelo como deprecated.
    
2. Bloquear novas configurações.
    
3. Mapear dependências.
    
4. Testar substituto.
    
5. Criar plano de migração.
    
6. Notificar responsáveis.
    
7. Atualizar políticas.
    
8. Remover após prazo.
    

---

# Fallback e Versionamento

A política de fallback também deverá possuir versão.

Exemplo:

```text
ProviderPolicy v3
```

Ela poderá definir:

```text
OpenAI Model A
    ↓
Anthropic Model B
    ↓
Gemini Model C
```

A execução deverá registrar qual policy foi aplicada.

---

# Versionamento de Configuração

Configurações persistidas deverão possuir versão ou revisão.

Exemplos:

```text
ConfigurationRevision: 184
ProviderPolicyVersion: 7
OrganizationSettingsVersion: 12
```

---

# Configuração Global

Alterações globais deverão gerar nova revisão.

A revisão poderá ser incremental.

```text
Revision 184
```

---

# Configuração por Organização

Uma organização poderá possuir:

```text
settings_version
```

Toda alteração incrementa a versão.

Isso permite:

- Concorrência otimista.
    
- Auditoria.
    
- Snapshot.
    
- Rollback.
    
- Comparação.
    

---

# Snapshot de Configuração

Execuções deverão persistir um snapshot das configurações relevantes.

Exemplo:

```json
{
  "configurationVersion": 18,
  "providerPolicyVersion": 4,
  "pipelineVersion": 3,
  "agentVersions": {
    "ResearchAgent": 2,
    "ScriptAgent": 5
  }
}
```

---

# Versão de Feature Flags

Feature flags deverão possuir:

- Revision.
    
- UpdatedAt.
    
- UpdatedBy.
    
- Rollout policy.
    
- Default.
    
- Expiration date.
    

A execução poderá registrar flags que afetaram seu comportamento.

---

# Feature Flag Temporária

Toda flag temporária deverá possuir:

- Owner.
    
- Data de criação.
    
- Data de revisão.
    
- Data esperada de remoção.
    
- Estado.
    
- Métricas.
    
- Plano de cleanup.
    

Flags permanentes não devem se acumular sem governança.

---

# Versionamento de Artefatos

Artefatos gerados deverão possuir versão.

Exemplos:

- Script.
    
- Thumbnail.
    
- Áudio.
    
- Vídeo.
    
- Legenda.
    
- SEO.
    
- Descrição.
    
- Tradução.
    
- Metadados.
    

---

# Artifact Version

Estrutura possível:

```text
artifacts
├── id
├── logical_artifact_id
├── version
├── artifact_type
├── status
├── storage_reference
├── content_hash
├── created_at
├── created_by
├── source_execution_id
└── supersedes_artifact_id
```

---

# Versão Lógica

Exemplo:

```text
Script A v1
Script A v2
Script A v3
```

Cada versão deverá ser imutável.

---

# Novo Artefato versus Nova Versão

## Nova versão

Quando representa evolução do mesmo artefato lógico.

Exemplo:

```text
Roteiro revisado
```

## Novo artefato

Quando representa uma alternativa independente.

Exemplo:

```text
Roteiro experimental B
```

---

# Status de Artefato

Exemplo:

```text
Draft
Generated
InReview
Approved
Rejected
Superseded
Published
Archived
```

---

# Artefato Aprovado

Uma versão aprovada não deverá ser sobrescrita.

Alterações geram nova versão que passa por nova aprovação quando necessário.

---

# Relação entre Artefatos

Exemplo:

```text
Script v3
    ↓
Voice v2
    ↓
Video v1
```

O sistema deverá saber exatamente quais versões foram utilizadas.

---

# Proveniência

Cada artefato deverá registrar sua proveniência.

Exemplos:

- PipelineExecutionId.
    
- PipelineVersion.
    
- AgentVersion.
    
- PromptVersion.
    
- Provider.
    
- Model.
    
- InputArtifactVersions.
    
- ConfigurationSnapshot.
    

---

# Content Hash

Artefatos poderão possuir hash para:

- Integridade.
    
- Deduplicação.
    
- Auditoria.
    
- Comparação.
    
- Cache.
    

Exemplo:

```text
SHA-256
```

---

# Versionamento de Banco de Dados

O banco será versionado por migrations.

Cada alteração de schema deverá possuir migration explícita.

---

# EF Core Migrations

Estrutura:

```text
Data/
└── Migrations/
```

Cada migration deverá possuir:

- Nome claro.
    
- Up.
    
- Down quando viável.
    
- Revisão.
    
- Teste.
    
- Impacto conhecido.
    

---

# Compatibilidade de Banco

Durante deploy, versões antigas e novas da aplicação podem coexistir temporariamente.

As migrations deverão considerar esse período.

---

# Expand and Contract

Estratégia recomendada para mudanças incompatíveis.

## Expand

Adicionar estrutura nova sem remover a antiga.

Exemplo:

```text
Adicionar coluna nova opcional
```

## Migrate

Migrar dados e código gradualmente.

## Contract

Remover estrutura antiga após não haver dependências.

---

# Exemplo Expand and Contract

Etapa 1:

```text
Adicionar default_text_model
Manter model
```

Etapa 2:

```text
Aplicação escreve nos dois campos
```

Etapa 3:

```text
Backfill
```

Etapa 4:

```text
Aplicação lê apenas default_text_model
```

Etapa 5:

```text
Remover model
```

---

# Migrations Destrutivas

Mudanças destrutivas exigem cuidado.

Exemplos:

- Drop column.
    
- Alterar tipo.
    
- Renomear coluna.
    
- Recriar índice grande.
    
- Alterar constraint.
    
- Reprocessar grande volume.
    

Elas deverão possuir:

- Plano.
    
- Backup.
    
- Rollback.
    
- Janela.
    
- Teste de duração.
    
- Avaliação de lock.
    
- Observabilidade.
    

---

# Versão do Schema do Banco

A aplicação poderá verificar a versão mínima esperada.

Exemplo:

```text
DatabaseSchemaVersion
```

Se incompatível, poderá falhar no startup.

---

# Aplicação de Migrations

Migrations de produção não deverão ser aplicadas automaticamente por toda instância da API.

Possibilidades:

- Job de deploy.
    
- Migration runner.
    
- Pipeline de CI/CD.
    
- Processo administrativo.
    

---

# Versionamento de Dados

Além do schema, alguns registros poderão possuir sua própria versão.

Exemplos:

- Pipeline definition.
    
- Prompt.
    
- Agent.
    
- Organization settings.
    
- Provider policy.
    
- Artifact.
    
- Workflow.
    

---

# Optimistic Concurrency

Entidades mutáveis poderão possuir:

```text
row_version
```

ou:

```text
xmin
```

ou:

```text
version
```

A versão será incrementada a cada alteração.

---

# Versionamento de Workflows do n8n

Workflows do n8n deverão possuir controle de versão.

Possibilidades:

- Export JSON no repositório.
    
- Tag de release.
    
- Nome com versão.
    
- Workflow ID persistido.
    
- Snapshot.
    
- Changelog.
    

---

# Workflow Definition

Registrar:

- WorkflowName.
    
- WorkflowVersion.
    
- n8nWorkflowId.
    
- Status.
    
- InputSchemaVersion.
    
- OutputSchemaVersion.
    
- PublishedAt.
    
- DeprecatedAt.
    

---

# Mudança em Workflow

Alterações relevantes exigem nova versão.

Exemplos:

- Alterar entrada.
    
- Alterar saída.
    
- Alterar sequência.
    
- Alterar integração.
    
- Alterar regra.
    
- Alterar efeito externo.
    

---

# Execuções do n8n

A aplicação deverá registrar qual versão de workflow foi chamada.

```text
workflow_name
workflow_version
external_execution_id
```

---

# Webhooks e Versionamento

Endpoints de webhook deverão possuir versão quando o contrato puder evoluir.

Exemplo:

```text
/webhooks/v1/rendering/completed
```

---

# Webhooks de Saída

O cliente deverá assinar uma versão de contrato.

Exemplo:

```text
publication.completed.v1
```

A subscription deverá registrar quais versões suporta.

---

# Versionamento de Storage

A estrutura lógica de paths poderá evoluir.

Exemplo:

```text
artifacts/v1/{organizationId}/{artifactId}
```

Mudanças devem considerar:

- URLs existentes.
    
- Referências no banco.
    
- Retenção.
    
- Migração.
    
- Compatibilidade.
    

---

# Formato de Arquivos

Formatos também deverão ser tratados como versões.

Exemplos:

- JSON export v1.
    
- Manifest v2.
    
- Subtitle schema v1.
    
- Metadata package v3.
    

---

# Versionamento de Cache

Chaves de cache deverão possuir versão quando a estrutura do valor mudar.

Exemplo:

```text
project-summary:v2:{projectId}
```

Isso evita desserialização incompatível após deploy.

---

# Invalidação por Versão

Ao publicar nova versão:

```text
cache namespace v1
```

pode ser substituído por:

```text
cache namespace v2
```

Os dados antigos expiram naturalmente.

---

# Versionamento de Índices de Busca

Índices poderão possuir aliases.

Exemplo:

```text
content-index-v1
content-index-v2
```

Alias:

```text
content-index-current
```

Migração:

1. Criar índice novo.
    
2. Reindexar.
    
3. Validar.
    
4. Trocar alias.
    
5. Manter índice antigo temporariamente.
    
6. Remover.
    

---

# Versionamento de Embeddings

Embeddings produzidos por modelos diferentes não deverão ser misturados indiscriminadamente.

Registrar:

- Provider.
    
- Model.
    
- ModelVersion.
    
- Dimensions.
    
- PreprocessingVersion.
    
- ChunkingVersion.
    
- CreatedAt.
    

---

# Embedding Space

Ao trocar modelo de embedding, poderá ser necessário criar novo índice.

Exemplo:

```text
knowledge-embeddings-v1
knowledge-embeddings-v2
```

Não assumir compatibilidade entre espaços vetoriais.

---

# Versionamento de Chunking

A estratégia de chunking também afeta resultados.

Registrar:

- ChunkingStrategyVersion.
    
- ChunkSize.
    
- Overlap.
    
- Normalization.
    
- ParserVersion.
    

---

# Versionamento de RAG

Uma execução RAG poderá registrar:

- RetrievalPolicyVersion.
    
- EmbeddingModelVersion.
    
- IndexVersion.
    
- RerankerVersion.
    
- PromptVersion.
    
- TopK.
    
- Filters.
    

---

# Versionamento de Regras de Negócio

Mudanças de regra podem afetar resultados históricos.

Exemplos:

- Limites financeiros.
    
- Critérios de aprovação.
    
- Critérios de publicação.
    
- Políticas de qualidade.
    
- Regras de elegibilidade.
    

Quando necessário, registrar:

```text
policy_version
```

---

# Effective Date

Algumas regras podem utilizar vigência.

Exemplo:

```text
valid_from
valid_to
```

Isso permite saber qual regra estava ativa em determinado momento.

---

# Decisões Históricas

Uma decisão antiga não deverá ser reinterpretada apenas com a regra atual.

Exemplo:

```text
Conteúdo aprovado em 2026
```

deverá permanecer ligado à versão da policy usada na aprovação.

---

# Versionamento de Segurança

Alguns elementos de segurança também possuem versão.

Exemplos:

- Algoritmo de assinatura.
    
- Formato de token.
    
- Política de senha.
    
- Webhook signature version.
    
- Encryption key version.
    
- Consent policy version.
    

---

# Rotação de Chaves

Dados criptografados deverão registrar a versão da chave.

Exemplo:

```text
encryption_key_version
```

Isso permite:

- Descriptografar dados antigos.
    
- Recriptografar.
    
- Rotacionar.
    
- Revogar versões.
    

---

# Webhook Signature Version

Header possível:

```text
X-Webhook-Signature-Version: v1
```

Ao evoluir o algoritmo:

```text
v1 → HMAC-SHA256
v2 → nova canonicalização
```

---

# Versionamento de Auditoria

Eventos de auditoria também deverão possuir versão de schema.

Exemplo:

```text
audit.event.v1
```

A auditoria histórica não deverá depender do modelo atual.

---

# Changelog

Mudanças relevantes deverão possuir changelog.

Categorias:

```text
Added
Changed
Deprecated
Removed
Fixed
Security
Migration
```

---

# Changelog por Componente

Poderão existir changelogs para:

- API.
    
- Contracts.
    
- Pipelines.
    
- Prompts.
    
- Agents.
    
- Schemas.
    
- Workflows.
    
- Infraestrutura.
    

---

# Release Notes

Cada release deverá informar:

- Funcionalidades.
    
- Correções.
    
- Breaking changes.
    
- Migrations.
    
- Flags.
    
- Depreciações.
    
- Riscos.
    
- Rollback.
    
- Compatibilidade.
    
- Ações operacionais.
    

---

# Matriz de Compatibilidade

A plataforma poderá manter uma matriz.

|Componente|Versão|Compatível com|
|---|--:|---|
|API|v2|Clients v2|
|Pipeline Executor|3|Pipelines v2-v4|
|Script Agent|5|Prompt v7-v8|
|Contract|v2|Consumer 2.3+|
|Database|18|App 1.8+|

---

# Compatibilidade entre Deploys

Durante rolling deployment, versões diferentes podem coexistir.

A arquitetura deverá garantir:

- Mensagens compatíveis.
    
- Banco compatível.
    
- Cache compatível.
    
- Configuração compatível.
    
- Feature flags coordenadas.
    
- Nenhuma migration destrutiva antecipada.
    

---

# Ordem de Deploy

Exemplo para mudança compatível:

```text
1. Aplicar migration expansiva
2. Deploy consumers compatíveis
3. Deploy producers novos
4. Migrar dados
5. Medir
6. Remover comportamento antigo
7. Aplicar migration de contração
```

---

# Producer First versus Consumer First

Para mensagens, geralmente preferir:

```text
Consumer preparado primeiro
    ↓
Producer começa a publicar nova versão
```

Isso reduz risco de mensagens não suportadas.

---

# Dual Write

Durante migração, o producer poderá publicar:

```text
Event V1
Event V2
```

ou escrever em dois campos.

Dual write deverá ser temporário e observável.

---

# Dual Read

O consumer poderá aceitar:

```text
V1 ou V2
```

durante uma janela de migração.

---

# Shadow Processing

Uma nova versão poderá processar dados sem afetar o estado oficial.

Exemplo:

```text
Agent v5 oficial
Agent v6 shadow
```

Comparar:

- Qualidade.
    
- Custo.
    
- Latência.
    
- Segurança.
    
- Taxa de falha.
    

---

# Canary Release

Uma nova versão poderá ser liberada para pequena porcentagem.

Exemplo:

```text
5% das novas execuções usam Pipeline v4
```

A seleção deverá ser determinística quando necessário.

---

# Rollout por Organização

Uma versão poderá ser habilitada para:

- Equipe interna.
    
- Organização piloto.
    
- Plano específico.
    
- Região.
    
- Percentual de usuários.
    

---

# Rollback

Toda mudança relevante deverá possuir estratégia de rollback.

Tipos:

- Rollback de deploy.
    
- Rollback de configuração.
    
- Rollback de feature flag.
    
- Rollback de prompt.
    
- Rollback de pipeline para novas execuções.
    
- Rollback de provider policy.
    
- Rollback de workflow.
    

---

# Limites do Rollback

Nem toda mudança é reversível.

Exemplos:

- Dados removidos.
    
- Publicações externas.
    
- Migration destrutiva.
    
- Evento já consumido.
    
- Artefato já publicado.
    
- Alteração de formato sem cópia antiga.
    

Nesses casos, utilizar compensação ou forward fix.

---

# Forward Fix

Em alguns cenários, corrigir para frente é mais seguro que reverter.

Exemplo:

```text
Migration incompatível já aplicada
    ↓
Novo deploy corrige o comportamento
```

---

# Rollback de Banco

Rollback de aplicação não deve depender automaticamente de `Down()`.

O banco poderá já conter dados novos incompatíveis com o código antigo.

A estratégia deverá ser testada.

---

# Reprodutibilidade

O sistema deverá conseguir explicar como um resultado foi produzido.

Registrar:

- ApplicationVersion.
    
- PipelineVersion.
    
- StepVersion.
    
- AgentVersion.
    
- PromptVersion.
    
- SchemaVersion.
    
- Provider.
    
- Model.
    
- ProviderPolicyVersion.
    
- ConfigurationVersion.
    
- InputArtifactVersions.
    
- WorkflowVersion.
    

---

# Execution Manifest

Uma execução poderá possuir um manifest.

Exemplo:

```json
{
  "applicationVersion": "1.8.3",
  "pipeline": {
    "name": "YouTubeLongForm",
    "version": 4
  },
  "agents": {
    "ResearchAgent": 2,
    "ScriptAgent": 5
  },
  "prompts": {
    "ResearchPrompt": 3,
    "ScriptPrompt": 8
  },
  "providerPolicyVersion": 7,
  "configurationRevision": 184
}
```

---

# Limites da Reprodutibilidade

Mesmo com versionamento, resultados de IA podem não ser idênticos devido a:

- Não determinismo.
    
- Mudanças internas do provider.
    
- Aleatoriedade.
    
- Atualização silenciosa do modelo.
    
- Mudança de dados externos.
    
- Alteração de busca.
    
- Conteúdo removido.
    
- Hora da execução.
    

O objetivo será reproduzir o contexto e o processo, não garantir sempre saída idêntica.

---

# Determinismo

Quando suportado, registrar:

- Seed.
    
- Temperature.
    
- TopP.
    
- MaxTokens.
    
- Sampling parameters.
    
- Tool policy.
    
- System prompt.
    
- Model ID.
    

---

# Registro Temporal

Toda versão deverá possuir datas.

Exemplos:

- CreatedAt.
    
- PublishedAt.
    
- DeprecatedAt.
    
- RetiredAt.
    
- EffectiveFrom.
    
- EffectiveTo.
    

---

# Identificadores de Versão

A versão poderá utilizar:

- Inteiro sequencial.
    
- Semantic Version.
    
- UUID de revisão.
    
- Hash.
    
- Timestamp.
    

A escolha dependerá do elemento.

---

# Inteiro Sequencial

Recomendado para:

- Pipeline.
    
- Prompt.
    
- Agent.
    
- Schema.
    
- Artifact.
    
- Policy.
    

Exemplo:

```text
v1
v2
v3
```

---

# Semantic Versioning

Recomendado para:

- Aplicação.
    
- SDK.
    
- Bibliotecas.
    
- Pacotes.
    
- Contratos públicos, quando apropriado.
    

---

# Hash

Recomendado como identificação complementar para:

- Prompt.
    
- Arquivo.
    
- Template.
    
- Workflow.
    
- Manifest.
    
- Artefato.
    

---

# Versão e Status

Versão e status são conceitos diferentes.

Exemplo:

```text
Prompt v5
Status: Draft
```

```text
Prompt v4
Status: Published
```

A maior versão não é necessariamente a versão ativa.

---

# Versão Ativa

Uma referência poderá apontar para a versão ativa.

Exemplo:

```text
ScriptAgent CurrentVersion = 5
```

Essa referência poderá mudar.

A definição da versão 5 permanece imutável.

---

# Aliases

Aliases possíveis:

```text
current
stable
beta
canary
deprecated
```

Aliases são ponteiros mutáveis.

Versões são imutáveis.

---

# Governança de Versões

Toda família versionada deverá possuir:

- Owner.
    
- Estado.
    
- Data de criação.
    
- Histórico.
    
- Política de suporte.
    
- Política de remoção.
    
- Testes.
    
- Changelog.
    
- Dependências conhecidas.
    

---

# Registro de Dependências

Exemplo:

```text
Pipeline v4
    usa ScriptAgent v5
    usa ScriptPrompt v8
    usa ScriptOutputSchema v3
    usa ProviderPolicy v7
```

Esse grafo deverá ser consultável.

---

# Impact Analysis

Antes de remover uma versão, identificar:

- Execuções ativas.
    
- Mensagens pendentes.
    
- Outbox.
    
- Inbox.
    
- Workflows.
    
- Consumers.
    
- Configurações.
    
- Artefatos.
    
- Relatórios.
    
- Integrações externas.
    
- Testes.
    
- Reprocessamentos.
    

---

# Remoção de Versão

Uma versão só poderá ser removida quando:

- Não houver novas execuções.
    
- Não houver execuções ativas.
    
- Não houver mensagens pendentes.
    
- Não houver dependências.
    
- Retenção mínima tiver passado.
    
- Migração estiver concluída.
    
- Rollback não depender dela.
    
- Auditoria estiver preservada.
    

---

# Archive versus Delete

Preferir arquivar definições antigas.

Excluir somente quando:

- Permitido.
    
- Seguro.
    
- Sem dependências.
    
- Compatível com retenção.
    
- Auditável.
    

---

# Política de Retenção

Exemplo conceitual:

```text
API antiga
    até sunset

Prompt antigo
    enquanto houver execução ou auditoria

Pipeline antigo
    enquanto houver execução ou reprocessamento

Mensagem antiga
    conforme retenção da Outbox, Inbox e DLQ

Artefato
    conforme política de negócio
```

---

# Compatibilidade Automatizada

O CI/CD deverá validar compatibilidade.

Possibilidades:

- OpenAPI diff.
    
- JSON Schema diff.
    
- Contract tests.
    
- Snapshot tests.
    
- Migration tests.
    
- Serialization tests.
    
- AsyncAPI diff.
    
- Fixtures históricas.
    

---

# OpenAPI Diff

O pipeline deverá detectar:

- Endpoint removido.
    
- Campo removido.
    
- Tipo alterado.
    
- Campo obrigatório adicionado.
    
- Status removido.
    
- Enum incompatível.
    

---

# Schema Compatibility Test

Exemplo:

```text
Fixture v1
    ↓
Consumer atual
    ↓
Desserialização válida
```

---

# Golden Files

Contratos poderão possuir arquivos de referência.

```text
Contracts.Tests/
└── Fixtures/
    ├── pipeline-started.v1.json
    ├── pipeline-completed.v1.json
    └── content-published.v2.json
```

---

# Migration Tests

Testar:

- Banco vazio até versão atual.
    
- Banco da versão anterior até atual.
    
- Dados reais anonimizados.
    
- Migrações expansivas.
    
- Rollback quando suportado.
    
- Tempo de execução.
    
- Locks.
    

---

# Pipeline Version Tests

Cada pipeline publicado deverá possuir testes que validem:

- Definição.
    
- Ordem.
    
- Inputs.
    
- Outputs.
    
- Steps.
    
- Agents.
    
- Prompts.
    
- Retry.
    
- Fallback.
    
- Checkpoints.
    
- Resume.
    

---

# Prompt Regression Tests

Novas versões de prompt deverão ser comparadas com versões anteriores.

Métricas:

- Qualidade.
    
- Taxa de schema válido.
    
- Custo.
    
- Latência.
    
- Segurança.
    
- Consistência.
    
- Taxa de aprovação.
    

---

# Agent Regression Tests

Validar:

- Entrada.
    
- Saída.
    
- Tools.
    
- Permissões.
    
- Fallback.
    
- Custos.
    
- Critérios de decisão.
    
- Segurança.
    
- Performance.
    

---

# Observabilidade de Versões

Logs, métricas e traces deverão incluir, quando aplicável:

- application.version.
    
- api.version.
    
- message.version.
    
- pipeline.version.
    
- step.version.
    
- agent.version.
    
- prompt.version.
    
- schema.version.
    
- provider.policy.version.
    
- configuration.revision.
    
- workflow.version.
    

---

# Métricas por Versão

Exemplos:

```text
pipeline_executions_total{pipeline_version}
agent_executions_total{agent_version}
prompt_validation_failures_total{prompt_version}
message_processing_failures_total{message_version}
api_requests_total{api_version}
```

Cuidado com cardinalidade.

Versões controladas possuem cardinalidade aceitável.

---

# Dashboards

Dashboards poderão comparar:

- Pipeline v3 versus v4.
    
- Prompt v7 versus v8.
    
- Agent v4 versus v5.
    
- API v1 versus v2.
    
- ProviderPolicy v6 versus v7.
    
- Workflow v2 versus v3.
    

---

# Alertas

Alertas possíveis:

- Mensagem de versão não suportada.
    
- Consumer antigo ativo.
    
- API depreciada ainda muito utilizada.
    
- Pipeline antigo iniciando novas execuções.
    
- Prompt deprecated ainda selecionado.
    
- Modelos retirados ainda configurados.
    
- Instâncias com versões divergentes.
    
- Migration pendente.
    
- Configuração incompatível.
    
- Outbox antiga não publicável.
    
- Workflow obsoleto ainda chamado.
    

---

# Segurança de Versionamento

Versões não deverão ser selecionadas livremente por usuários sem autorização.

Riscos:

- Utilizar prompt antigo inseguro.
    
- Executar pipeline depreciado.
    
- Forçar contrato vulnerável.
    
- Escolher provider policy não permitida.
    
- Utilizar modelo retirado.
    

---

# Allowlist de Versões

O sistema poderá definir versões permitidas por:

- Ambiente.
    
- Organização.
    
- Plano.
    
- Projeto.
    
- Usuário.
    
- Feature flag.
    

---

# Versões Vulneráveis

Ao identificar vulnerabilidade:

1. Marcar versão como blocked.
    
2. Impedir novas execuções.
    
3. Avaliar execuções ativas.
    
4. Criar substituição.
    
5. Migrar quando seguro.
    
6. Auditar uso.
    
7. Remover acesso.
    

---

# Status Bloqueado

Além de deprecated, poderá existir:

```text
Blocked
```

Uma versão bloqueada não pode ser usada, mesmo que referenciada por configuração antiga.

---

# Administração

Uma interface administrativa poderá permitir:

- Consultar versões.
    
- Publicar versão.
    
- Depreciar.
    
- Alterar alias.
    
- Comparar versões.
    
- Ver dependências.
    
- Ver execuções ativas.
    
- Realizar rollback.
    
- Bloquear versão.
    
- Consultar changelog.
    

---

# Permissões

Ações de versionamento poderão exigir permissões distintas.

Exemplos:

```text
prompts.publish
pipelines.publish
agents.publish
versions.deprecate
versions.rollback
versions.block
```

---

# Aprovação de Mudanças

Mudanças críticas poderão exigir dupla aprovação.

Exemplos:

- Publicar pipeline de produção.
    
- Alterar policy financeira.
    
- Alterar prompt de publicação automática.
    
- Alterar workflow com efeito externo.
    
- Bloquear versão em uso.
    

---

# Ambientes

Versões poderão possuir promoção entre ambientes.

Fluxo:

```text
Development
    ↓
Staging
    ↓
Production
```

A mesma definição deverá manter identidade ou hash entre ambientes.

---

# Promoção

Promover significa mover uma versão já testada.

Não significa recriar manualmente uma definição parecida em produção.

---

# Artifact Promotion

Exemplo:

```text
Prompt v8
Hash ABC
    ↓
Staging aprovado
    ↓
Produção recebe Prompt v8
Hash ABC
```

---

# Divergência entre Ambientes

O sistema deverá detectar quando:

```text
Prompt v8 em staging
```

não é igual a:

```text
Prompt v8 em produção
```

A mesma versão não deverá possuir conteúdos diferentes.

---

# Identidade Global

Uma definição poderá possuir:

- DefinitionId.
    
- Version.
    
- ContentHash.
    

Isso ajuda a garantir igualdade entre ambientes.

---

# Backup e Restore

Restore deverá preservar:

- Versões.
    
- Dependências.
    
- Status.
    
- Histórico.
    
- Aliases.
    
- Snapshots.
    
- Changelogs.
    
- Auditoria.
    

---

# Exportação

Definições versionadas poderão ser exportadas.

Exemplo:

```json
{
  "name": "YouTubeLongForm",
  "version": 4,
  "hash": "...",
  "definition": {}
}
```

---

# Importação

A importação deverá validar:

- Nome.
    
- Versão.
    
- Hash.
    
- Dependências.
    
- Schemas.
    
- Compatibilidade.
    
- Assinatura, quando aplicável.
    
- Existência prévia.
    

Não sobrescrever versão existente com conteúdo diferente.

---

# Conflito de Versão

Se uma versão já existir com hash diferente:

```text
version_content_conflict
```

A operação deverá falhar.

---

# Regras Arquiteturais

- Toda mudança incompatível deve criar nova versão.
    
- Contratos publicados devem ser imutáveis.
    
- Pipelines publicados devem ser imutáveis.
    
- Agents publicados devem ser imutáveis.
    
- Prompts publicados devem ser imutáveis.
    
- Schemas publicados devem ser imutáveis.
    
- Artefatos aprovados não devem ser sobrescritos.
    
- Versão da aplicação não substitui versões de negócio.
    
- Execuções longas devem registrar snapshots.
    
- Execuções devem continuar na versão em que começaram.
    
- Migração de execução ativa deve ser excepcional.
    
- Mensagens devem possuir versão explícita.
    
- Consumers devem rejeitar versões não suportadas.
    
- Campos desconhecidos devem ser tolerados quando possível.
    
- Outbox deve preservar a versão do contrato.
    
- Deploys devem suportar mensagens pendentes antigas.
    
- APIs devem possuir política de depreciação.
    
- Versões antigas não devem ser removidas sem análise de dependência.
    
- Banco deve evoluir por migrations.
    
- Mudanças destrutivas devem utilizar expand and contract.
    
- Cache deve utilizar versionamento quando o formato mudar.
    
- Índices de busca devem evoluir por versões e aliases.
    
- Embeddings de modelos diferentes não devem ser misturados sem controle.
    
- Workflows do n8n devem ser versionados.
    
- Feature flags temporárias devem possuir data de remoção.
    
- Aliases são mutáveis; versões são imutáveis.
    
- A mesma versão não pode possuir conteúdos diferentes entre ambientes.
    
- Toda definição versionada deve possuir owner e status.
    
- Toda versão crítica deve possuir changelog.
    
- Compatibilidade deve ser validada no CI/CD.
    
- Rollback deve ser planejado antes do deploy.
    
- Versões vulneráveis devem poder ser bloqueadas.
    
- Logs e traces devem registrar as versões relevantes.
    
- Segredos nunca devem fazer parte de snapshots versionados.
    
- Reprodutibilidade deve registrar provider, modelo e policy.
    
- Remoção de versões deve respeitar retenção e auditoria.
    

---

# Checklist para Nova Versão de API

- Existe breaking change?
    
- A nova versão é realmente necessária?
    
- Existe OpenAPI?
    
- Existe documentação?
    
- Existe guia de migração?
    
- Existe data de depreciação da anterior?
    
- Os códigos de erro permanecem estáveis?
    
- Os clientes foram testados?
    
- O uso da versão anterior é medido?
    
- Existe plano de sunset?
    

---

# Checklist para Nova Versão de Mensagem

- MessageType é estável?
    
- MessageVersion foi incrementada?
    
- O contrato antigo permanece disponível?
    
- O consumer novo aceita a versão antiga?
    
- O producer será liberado depois do consumer?
    
- Existe fixture?
    
- Existe teste de serialização?
    
- Existe teste de compatibilidade?
    
- Existe estratégia de DLQ?
    
- A Outbox consegue publicar mensagens antigas?
    

---

# Checklist para Nova Versão de Pipeline

- Nome e versão definidos?
    
- Definição imutável?
    
- Steps versionados?
    
- Inputs e outputs versionados?
    
- Agents definidos?
    
- Prompts definidos?
    
- Provider policy definida?
    
- Timeouts definidos?
    
- Retries definidos?
    
- Checkpoints definidos?
    
- Resume testado?
    
- Migração de execução avaliada?
    
- Rollback para novas execuções definido?
    
- Changelog criado?
    
- Testes aprovados?
    

---

# Checklist para Nova Versão de Agent

- Input schema definido?
    
- Output schema definido?
    
- Prompt versionado?
    
- Tools versionadas?
    
- Permissões avaliadas?
    
- Provider policy definida?
    
- Regressão testada?
    
- Custo comparado?
    
- Latência comparada?
    
- Segurança validada?
    
- Estratégia de rollout definida?
    
- Versão anterior permanece suportada?
    

---

# Checklist para Nova Versão de Prompt

- Template imutável?
    
- Variáveis definidas?
    
- Output schema definido?
    
- Hash calculado?
    
- Testes de regressão executados?
    
- Custo medido?
    
- Qualidade comparada?
    
- Segurança avaliada?
    
- A/B test necessário?
    
- Rollback disponível?
    
- Owner definido?
    
- Changelog criado?
    

---

# Checklist para Nova Migration

- É expansiva ou destrutiva?
    
- Funciona com versão anterior da aplicação?
    
- Possui backfill?
    
- Pode gerar lock?
    
- Tempo foi testado?
    
- Existe backup?
    
- Existe rollback?
    
- Exige janela?
    
- Foi testada com volume realista?
    
- A aplicação tolera dados parcialmente migrados?
    
- Existe observabilidade?
    

---

# Checklist para Depreciação

- Existe substituição?
    
- Dependências foram identificadas?
    
- Novas utilizações foram bloqueadas?
    
- Consumidores foram notificados?
    
- Métricas de uso existem?
    
- Prazo foi definido?
    
- Guia de migração existe?
    
- Execuções ativas foram avaliadas?
    
- Mensagens pendentes foram avaliadas?
    
- Retenção foi respeitada?
    
- A remoção foi aprovada?
    

---

# Decisões Pendentes

As seguintes decisões deverão ser registradas em ADRs:

- Estratégia final de versionamento da API.
    
- Política de suporte de versões da API.
    
- Uso de Semantic Versioning.
    
- Convenção de versões de mensagens.
    
- Estratégia de upcasting.
    
- Política de compatibilidade de contracts.
    
- Organização de contracts por versão.
    
- Política de imutabilidade de pipelines.
    
- Política de migração de execuções.
    
- Versionamento de steps.
    
- Versionamento de agents.
    
- Versionamento e armazenamento de prompts.
    
- Formato de schemas.
    
- Estratégia de schema migration.
    
- Versionamento de provider policies.
    
- Tratamento de aliases de modelos.
    
- Estratégia de snapshots.
    
- Versionamento de artefatos.
    
- Política de retenção de versões antigas.
    
- Estratégia de workflows do n8n.
    
- Política de cache versionado.
    
- Estratégia de índices versionados.
    
- Versionamento de embeddings.
    
- Política de rollout.
    
- Política de rollback.
    
- Uso de expand and contract.
    
- Processo de depreciação.
    
- Ferramentas de compatibilidade no CI/CD.
    
- Modelo de promoção entre ambientes.
    
- Estratégia de bloqueio de versões vulneráveis.
    

---

# Exemplo Completo: Nova Versão de Pipeline

```text
YouTubeLongForm v3 está em produção
    ↓
Equipe deseja adicionar etapa de Fact Check
    ↓
A ordem e o comportamento serão alterados
    ↓
Nova definição YouTubeLongForm v4 é criada
    ↓
FactCheckStep v1 é adicionada
    ↓
ResearchAgent v3 é selecionado
    ↓
FactCheckPrompt v1 é publicado
    ↓
Output Schema v2 é definido
    ↓
Testes de regressão são executados
    ↓
Pipeline v4 é publicado em staging
    ↓
Execuções shadow são realizadas
    ↓
Qualidade e custo são comparados
    ↓
Pipeline v4 é promovido para produção
    ↓
10% das novas execuções usam v4
    ↓
Execuções iniciadas em v3 continuam em v3
    ↓
Métricas são avaliadas
    ↓
Rollout sobe para 100%
    ↓
v3 é marcada como Deprecated
    ↓
v3 permanece disponível para execuções antigas
```

---

# Exemplo Completo: Evento V2

```text
ContentPublishedV1 está em uso
    ↓
Novo consumer precisa da plataforma e do ExternalPublicationId
    ↓
Adicionar campos obrigatórios quebraria consumidores
    ↓
ContentPublishedV2 é criado
    ↓
Consumer novo recebe suporte a V1 e V2
    ↓
Deploy do consumer acontece
    ↓
Producer começa a publicar V2
    ↓
Durante transição, consumers antigos continuam em V1
    ↓
Uso de V1 é monitorado
    ↓
Consumers são migrados
    ↓
V1 é marcada como deprecated
    ↓
Após a retenção, suporte a V1 é removido
```

---

# Exemplo Completo: Prompt com Rollback

```text
ScriptPrompt v7 está ativo
    ↓
ScriptPrompt v8 é criado
    ↓
Testes mostram melhor estrutura
    ↓
v8 recebe 10% das execuções
    ↓
Métricas mostram maior custo e menor aprovação
    ↓
Alias stable volta para v7
    ↓
Novas execuções usam v7
    ↓
Execuções já iniciadas com v8 permanecem rastreáveis
    ↓
v8 é marcada como Rejected
    ↓
Resultados são preservados para análise
```

---

# Exemplo Completo: Migration Expand and Contract

```text
Campo model será substituído por default_text_model
    ↓
Migration adiciona default_text_model opcional
    ↓
Aplicação passa a escrever nos dois campos
    ↓
Job executa backfill
    ↓
Métricas confirmam preenchimento completo
    ↓
Aplicação passa a ler default_text_model
    ↓
Deploy antigo deixa de existir
    ↓
Campo model é marcado como deprecated
    ↓
Migration futura remove model
```

---

# Exemplo Completo: Reprodutibilidade

```text
Usuário questiona por que um roteiro foi produzido de determinada forma
    ↓
Sistema carrega Execution Manifest
    ↓
Pipeline: YouTubeLongForm v4
    ↓
ScriptAgent: v5
    ↓
ScriptPrompt: v8
    ↓
Schema: script-output.v3
    ↓
ProviderPolicy: v7
    ↓
Provider: OpenAI
    ↓
Model: model-x
    ↓
ConfigurationRevision: 184
    ↓
Input Script Brief: v2
    ↓
Logs, traces e custos são correlacionados
    ↓
Equipe consegue explicar o processo utilizado
```

---

# Objetivo Final

Criar uma plataforma capaz de evoluir sem perder compatibilidade, histórico ou controle.

O Infinite Content AI deverá saber:

- Qual versão recebeu uma requisição.
    
- Qual versão publicou uma mensagem.
    
- Qual versão executou um pipeline.
    
- Qual Agent tomou uma decisão.
    
- Qual Prompt foi utilizado.
    
- Qual Schema validou a saída.
    
- Qual Provider e modelo produziram o resultado.
    
- Qual configuração estava ativa.
    
- Quais artefatos serviram de entrada.
    
- Qual versão do sistema persistiu o estado.
    

Versões publicadas deverão ser imutáveis.

Mudanças incompatíveis deverão ser explícitas.

Execuções deverão ser reproduzíveis.

Depreciações deverão ser controladas.

Rollbacks deverão ser possíveis quando tecnicamente seguros.

A evolução da plataforma não deverá apagar o contexto do passado.