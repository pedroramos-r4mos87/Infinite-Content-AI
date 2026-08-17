# Sprint 02 — Execução de Pipeline e Artifacts

## Status

**Planejada**

## Milestone

M1 — MVP

## Prioridade

P0

---

# Objetivo

Implementar o primeiro fluxo executável de ponta a ponta da plataforma.

Ao final desta Sprint, um usuário autenticado deverá conseguir executar um Pipeline publicado e obter os resultados produzidos pelas etapas Research e Script.

Fluxo esperado:

```text
Project
   ↓
Published Pipeline
   ↓
Start Execution
   ↓
PipelineExecution
   ↓
Research Step
   ↓
Fake AI Provider
   ↓
Research Artifact
   ↓
Script Step
   ↓
Fake AI Provider
   ↓
Script Artifact
   ↓
PipelineExecution Completed
```

Esta Sprint deve provar o modelo de execução antes da introdução de RabbitMQ, Worker, Outbox, retries ou providers reais de IA.

---

# Resultado esperado

Ao final da Sprint deverá ser possível:

1. criar um Project;
    
2. criar um Pipeline;
    
3. adicionar Research;
    
4. adicionar Script;
    
5. publicar o Pipeline;
    
6. iniciar uma execução;
    
7. executar Research;
    
8. persistir o resultado como Artifact;
    
9. executar Script usando o resultado de Research como contexto;
    
10. persistir o Script como Artifact;
    
11. concluir a execução;
    
12. consultar a execução;
    
13. consultar os Artifacts produzidos.
    

Tudo deve funcionar de ponta a ponta:

```text
HTTP
↓
Application
↓
Domain
↓
AI Abstraction
↓
Fake AI Provider
↓
Data
↓
PostgreSQL
```

---

# Princípio arquitetural da Sprint

Nesta Sprint, a execução será **síncrona**.

Isso é intencional.

O objetivo é validar primeiro:

- modelo de Domain;
    
- lifecycle da execução;
    
- lifecycle das Steps;
    
- persistência;
    
- contratos da Application;
    
- abstração de AI Provider;
    
- criação de Artifacts;
    
- composição Research → Script;
    
- isolamento multi-tenant;
    
- comportamento HTTP.
    

Somente depois desse fluxo estar sólido serão adicionados:

- RabbitMQ;
    
- Worker;
    
- Outbox;
    
- Inbox;
    
- retries;
    
- execução distribuída.
    

A Sprint não deve antecipar essas preocupações.

---

# Escopo

## Incluído

- PipelineExecution
    
- PipelineExecutionId
    
- PipelineExecutionStatus
    
- StepExecution
    
- StepExecutionId
    
- StepExecutionStatus
    
- Artifact
    
- ArtifactId
    
- ArtifactType
    
- execução de Pipeline publicado
    
- execução de Research Step
    
- execução de Script Step
    
- Fake AI Provider
    
- abstraction para AI Provider
    
- persistência PostgreSQL
    
- consulta de Execution
    
- consulta dos Artifacts
    
- endpoint para iniciar execução
    
- endpoint para consultar execução
    
- testes Domain
    
- testes Application
    
- testes PostgreSQL
    
- testes API E2E
    
- multi-tenancy
    
- ProblemDetails
    
- OpenAPI
    

---

# Fora de escopo

Não implementar nesta Sprint:

- RabbitMQ
    
- Worker assíncrono
    
- Outbox
    
- Inbox
    
- retry automático
    
- recovery distribuído
    
- dead-letter queue
    
- Redis
    
- n8n
    
- OpenAI real
    
- Anthropic
    
- Azure OpenAI
    
- provider selection
    
- streaming
    
- token accounting
    
- custo financeiro por execução
    
- quotas
    
- billing
    
- rate limiting de IA
    
- human approval
    
- cancelamento de execução
    
- pausa
    
- resume
    
- branching
    
- loops
    
- conditional Steps
    
- paralelismo
    
- version 2 de Pipeline
    
- clone de Pipeline
    
- edição de Pipeline publicado
    
- scheduler
    
- webhooks
    
- notificações
    

---

# Pré-condições

A Sprint assume como concluído:

## Sprint 00

- Project
    
- multi-tenancy
    
- fake authentication
    
- PostgreSQL
    
- Result Pattern
    
- ProblemDetails
    

## Sprint 01

- Pipeline
    
- Research Step
    
- Script Step
    
- Pipeline Published
    
- Version 1
    
- Create Pipeline
    
- Add Step
    
- Publish Pipeline
    
- Get Pipeline
    
- List Pipelines
    
- PostgreSQL mappings
    
- endpoints HTTP
    

---

# Fluxo principal do MVP

```text
POST Execute Pipeline
        ↓
validar Organization
        ↓
carregar Published Pipeline
        ↓
criar PipelineExecution
        ↓
criar Research StepExecution
        ↓
executar Research via IAIProvider
        ↓
criar Research Artifact
        ↓
Research StepExecution Completed
        ↓
criar Script StepExecution
        ↓
enviar Research Artifact como contexto
        ↓
executar Script via IAIProvider
        ↓
criar Script Artifact
        ↓
Script StepExecution Completed
        ↓
PipelineExecution Completed
```

---

# US-050 — Criar PipelineExecution

## História

Como usuário autenticado, quero iniciar a execução de um Pipeline publicado para produzir conteúdo através das Steps configuradas.

## Entrada

A execução deve ser iniciada para um Pipeline específico.

Endpoint planejado:

```http
POST /api/v1/pipelines/{pipelineId}/executions
```

Request inicial pode conter somente o input necessário para o fluxo.

Exemplo:

```json
{
  "topic": "Como agentes de IA estão mudando o desenvolvimento de software"
}
```

Não aceitar:

- OrganizationId
    
- ExecutionId
    
- status
    
- timestamps
    
- CreatedBy fornecido pelo cliente
    
- StepExecutionIds
    
- ArtifactIds
    

---

# Execution Input

Para o MVP, a execução terá um input simples.

Campo obrigatório:

```text
Topic
```

O Topic representa o assunto que será pesquisado pela Research Step e utilizado na geração do Script.

Não criar sistema genérico de formulário ou variável dinâmica nesta Sprint.

Não implementar:

```text
Dictionary<string, object>
```

ou mecanismo arbitrário de parâmetros.

O MVP possui apenas o conceito necessário para provar o fluxo.

---

# PipelineExecution

Criar Aggregate ou modelo de Domain equivalente a:

```text
PipelineExecution
```

Campos mínimos:

- PipelineExecutionId
    
- OrganizationId
    
- ProjectId
    
- PipelineId
    
- PipelineVersion
    
- Topic
    
- Status
    
- CreatedAt
    
- CreatedBy
    
- StartedAt
    
- CompletedAt
    
- FailedAt
    
- FailureCode
    
- FailureMessage
    
- coleção de StepExecutions
    

---

# PipelineExecutionId

Usar UUID v7.

---

# PipelineVersion

A execução deve registrar explicitamente:

```text
PipelineVersion = 1
```

Mesmo que atualmente só exista Version 1.

Isso garante que a execução saiba qual configuração de Pipeline originou aquele processamento.

Não implementar snapshot de configuração ainda.

---

# PipelineExecutionStatus

Estados permitidos nesta Sprint:

```text
Pending
Running
Completed
Failed
```

Não adicionar estados futuros como:

- Cancelling
    
- Cancelled
    
- Paused
    
- Retrying
    
- Queued
    
- WaitingForApproval
    

sem necessidade atual.

---

# Lifecycle de PipelineExecution

Estado inicial:

```text
Pending
```

Quando processamento iniciar:

```text
Pending
↓
Running
```

Quando todas as Steps concluírem:

```text
Running
↓
Completed
```

Em falha inesperada ou falha do AI Provider:

```text
Running
↓
Failed
```

Estados finais:

```text
Completed
Failed
```

Uma execução finalizada não pode ser iniciada novamente nesta Sprint.

---

# Regras de PipelineExecution

- OrganizationId obrigatório.
    
- ProjectId obrigatório.
    
- PipelineId obrigatório.
    
- Topic obrigatório.
    
- Topic deve ser normalizado com Trim.
    
- CreatedBy obrigatório.
    
- IDs novos utilizam UUID v7.
    
- timestamps utilizam `IClock`.
    
- PipelineVersion deve ser maior que zero.
    
- status inicial Pending.
    
- CompletedAt somente em Completed.
    
- FailedAt somente em Failed.
    
- FailureCode/FailureMessage somente em Failed.
    
- uma execução Completed é imutável para os comportamentos existentes.
    
- uma execução Failed é final para esta Sprint.
    

---

# Pipeline elegível para execução

Somente Pipeline com:

```text
Status = Published
```

pode ser executado.

Pipeline Draft deve ser rejeitado.

Erro estável sugerido:

```text
PipelineExecution.PipelineNotPublished
```

Pipeline inexistente ou pertencente a outro tenant deve resultar em:

```text
PipelineExecution.PipelineNotFound
```

e comportamento HTTP:

```text
404
```

---

# US-051 — StepExecution

Cada Step do Pipeline deve gerar uma instância de execução.

Criar:

```text
StepExecution
```

Campos mínimos:

- StepExecutionId
    
- PipelineExecutionId
    
- PipelineStepId
    
- Type
    
- Position
    
- Status
    
- StartedAt
    
- CompletedAt
    
- FailedAt
    
- FailureCode
    
- FailureMessage
    

---

# StepExecutionStatus

Estados:

```text
Pending
Running
Completed
Failed
```

Não adicionar Retry status nesta Sprint.

---

# Regras de StepExecution

- ID utiliza UUID v7.
    
- Type deve corresponder à Step original do Pipeline.
    
- Position deve refletir a posição original.
    
- status inicial Pending.
    
- Running registra StartedAt.
    
- Completed registra CompletedAt.
    
- Failed registra FailedAt.
    
- uma Step Completed não pode executar novamente.
    
- uma Step Failed é final nesta Sprint.
    

---

# Ordem de execução

A ordem é determinada pelo Pipeline publicado.

Para o MVP:

```text
Research — Position 1
Script   — Position 2
```

Executar estritamente nessa ordem.

Não implementar engine genérica de dependências.

---

# US-052 — Artifact

Criar entidade/modelo:

```text
Artifact
```

Artifact representa um resultado persistido produzido durante uma execução.

Campos mínimos:

- ArtifactId
    
- OrganizationId
    
- ProjectId
    
- PipelineExecutionId
    
- StepExecutionId
    
- Type
    
- Content
    
- CreatedAt
    

---

# ArtifactId

UUID v7.

---

# ArtifactType

Tipos suportados nesta Sprint:

```text
Research
Script
```

Não criar tipos futuros.

---

# Artifact Content

Para esta Sprint, armazenar o conteúdo como texto.

Exemplo:

```text
text
```

ou `string` persistida como tipo PostgreSQL apropriado.

Não implementar:

- blobs;
    
- arquivos externos;
    
- Azure Blob Storage;
    
- Markdown AST;
    
- JSON arbitrário;
    
- embeddings;
    
- vectors.
    

O Artifact pode conter Markdown como texto.

---

# Research Artifact

Resultado produzido pela Research Step.

Deve conter conteúdo suficiente para servir de contexto para a Script Step.

Exemplo conceitual:

```markdown
# Research

## Topic

Como agentes de IA estão mudando o desenvolvimento de software

## Findings

- Agentes conseguem executar workflows multi-step.
- Ferramentas são utilizadas para interagir com sistemas externos.
- Guardrails continuam necessários.
```

O conteúdo real da Fake AI não precisa ser sofisticado.

Precisa apenas ser:

- determinístico;
    
- identificável;
    
- útil para testar o fluxo seguinte.
    

---

# Script Artifact

Resultado da Script Step.

O Script deve ser produzido usando:

- Topic da Execution;
    
- conteúdo do Research Artifact.
    

Isso é obrigatório para provar que existe composição real entre Steps.

Não basta chamar Fake AI duas vezes sem passar o resultado da Research para Script.

---

# US-053 — AI Provider Abstraction

Criar abstraction de Application ou boundary apropriada equivalente a:

```text
IAIProvider
```

O Domain não deve conhecer provider de IA.

O Domain não deve conhecer:

- OpenAI;
    
- HTTP;
    
- SDK;
    
- prompts;
    
- tokens;
    
- model names.
    

A abstraction deve viver na camada apropriada seguindo Clean Architecture.

---

# Interface de AI

Não criar uma interface gigante.

Preferir operações explícitas para o MVP.

Exemplo conceitual:

```text
ResearchAsync(...)
GenerateScriptAsync(...)
```

ou representação equivalente.

A interface deve permitir futura substituição por OpenAI sem alterar o caso de uso principal.

---

# Fake AI Provider

Implementar na Infrastructure.

Exemplo:

```text
FakeAIProvider
```

O Fake Provider deve:

- ser determinístico;
    
- não acessar internet;
    
- não acessar API externa;
    
- não usar secrets;
    
- respeitar CancellationToken;
    
- produzir Research;
    
- produzir Script;
    
- incorporar o input recebido na resposta.
    

---

# Comportamento da Fake Research

Entrada:

```text
Topic
```

Saída deve mencionar explicitamente o Topic.

Exemplo:

```text
Fake research for: {Topic}
```

Pode produzir Markdown estruturado.

---

# Comportamento da Fake Script

Entrada:

- Topic;
    
- Research Artifact.
    

Saída deve provar que recebeu ambos.

Exemplo conceitual:

```text
Fake script for: {Topic}

Based on research:
{ResearchContent}
```

Nos testes, deve ser possível confirmar que Research foi realmente repassada ao Script.

---

# Registro do Fake Provider

Fake AI Provider deve ser usado somente em ambientes apropriados.

Nesta Sprint, pode ser o provider padrão do MVP local/test.

A arquitetura deve permitir substituição futura sem alterar o Domain.

Não criar configuração complexa de múltiplos providers ainda.

---

# US-054 — Execute Pipeline

Criar caso de uso principal equivalente a:

```text
ExecutePipeline
```

Pode ser síncrono nesta Sprint.

Fluxo:

```text
Validate request
    ↓
Resolve Organization
    ↓
Resolve User
    ↓
Load Published Pipeline
    ↓
Create PipelineExecution
    ↓
Persist Execution
    ↓
Start Execution
    ↓
Execute Research
    ↓
Persist Research Artifact
    ↓
Complete Research Step
    ↓
Execute Script
    ↓
Persist Script Artifact
    ↓
Complete Script Step
    ↓
Complete PipelineExecution
    ↓
Save
    ↓
Result
```

A implementação pode usar múltiplos `SaveChanges` quando isso for necessário para preservar corretamente o estado da execução em caso de falha.

Não sacrificar consistência apenas para reduzir chamadas ao banco.

---

# Transações

A Sprint deve avaliar explicitamente onde utilizar transaction.

Evitar uma transação longa envolvendo chamada de AI.

Mesmo com Fake AI, modelar pensando no futuro.

Não manter transaction PostgreSQL aberta durante uma futura chamada real de IA.

Preferência arquitetural:

```text
persist state
commit

call AI

persist result/state
commit
```

Isso facilita a futura migração para Worker/RabbitMQ.

---

# Falhas do AI Provider

Mesmo Fake Provider sendo normalmente bem-sucedido, a abstraction e testes devem permitir simular falha.

Quando Research falhar:

```text
Research StepExecution → Failed
PipelineExecution → Failed
```

Script não deve executar.

Quando Script falhar:

```text
Research → Completed
Script → Failed
PipelineExecution → Failed
```

Research Artifact permanece persistido.

---

# FailureCode

Guardar código estável.

Exemplo:

```text
AI.ResearchFailed
AI.ScriptFailed
```

Não persistir stack trace.

---

# FailureMessage

Pode guardar mensagem sanitizada útil para diagnóstico.

Não armazenar secrets.

Não enviar exceção interna bruta para API.

---

# Result Pattern

Erros esperados antes do início da execução utilizam:

```text
Result
Result<T>
Error
```

Exemplos:

- Pipeline inexistente
    
- Pipeline Draft
    
- Organization ausente
    
- User ausente
    
- Topic inválido
    

Falhas inesperadas do provider devem ser transformadas em estado Failed da Execution de forma controlada.

---

# Domain Events

Criar somente eventos que possuam valor real nesta Sprint.

Possíveis eventos:

```text
PipelineExecutionCreated
PipelineExecutionStarted
PipelineExecutionCompleted
PipelineExecutionFailed
```

Não criar evento para cada alteração se não houver consumidor ou valor arquitetural claro.

Não publicar mensagens RabbitMQ ainda.

Domain Events continuam framework-independent.

---

# US-055 — Persistência

Criar schema PostgreSQL necessário.

Tabelas esperadas:

```text
pipeline_executions
step_executions
artifacts
```

---

# pipeline_executions

Campos esperados:

```text
id
organization_id
project_id
pipeline_id
pipeline_version
topic
status
created_at
created_by
started_at
completed_at
failed_at
failure_code
failure_message
```

---

# step_executions

Campos esperados:

```text
id
pipeline_execution_id
pipeline_step_id
type
position
status
started_at
completed_at
failed_at
failure_code
failure_message
```

---

# artifacts

Campos esperados:

```text
id
organization_id
project_id
pipeline_execution_id
step_execution_id
type
content
created_at
```

---

# FKs

Esperado:

```text
PipelineExecution
    → Pipeline
```

```text
StepExecution
    → PipelineExecution
```

```text
Artifact
    → PipelineExecution
```

```text
Artifact
    → StepExecution
```

Considerar FK de Project quando fizer sentido e for consistente com schema existente.

---

# Índices

Criar índices necessários para:

- listar Executions por Pipeline;
    
- listar Executions por Project;
    
- tenant isolation;
    
- carregar StepExecutions;
    
- carregar Artifacts de uma Execution.
    

Exemplo conceitual:

```text
organization_id + pipeline_id + created_at + id
```

e:

```text
pipeline_execution_id + position
```

Não criar índices especulativos sem consulta correspondente.

---

# Constraints

Reforçar quando simples:

- IDs obrigatórios;
    
- PipelineVersion > 0;
    
- Step Position > 0;
    
- conteúdo Artifact obrigatório;
    
- uma StepExecution por posição dentro da execução, se apropriado;
    
- um Artifact por StepExecution no MVP, se o modelo realmente assumir isso.
    

Não criar constraint que dificulte futuras necessidades sem benefício atual.

---

# Migration

Criar migration nova.

Nome sugerido:

```text
AddPipelineExecutionsAndArtifacts
```

Não alterar:

- InitialCreate
    
- AddPipelines
    

Migration deve ser aditiva.

---

# US-056 — Get PipelineExecution

Endpoint:

```http
GET /api/v1/executions/{executionId}
```

Resultado mínimo:

```text
ExecutionId
ProjectId
PipelineId
PipelineVersion
Topic
Status
CreatedAt
StartedAt
CompletedAt
FailedAt
FailureCode
Steps
Artifacts
```

---

# Execution Step Response

Cada Step deve conter:

```text
StepExecutionId
PipelineStepId
Type
Position
Status
StartedAt
CompletedAt
FailedAt
FailureCode
```

Ordenar por:

```text
Position ASC
```

---

# Artifact Response

Cada Artifact:

```text
ArtifactId
StepExecutionId
Type
Content
CreatedAt
```

Ordenar de maneira determinística.

Preferência:

```text
CreatedAt ASC
Id ASC
```

ou pela posição da Step quando projetado junto.

---

# Tenant isolation

Todas as operações devem utilizar:

```text
OrganizationId
```

do contexto atual.

Nunca aceitar OrganizationId no request.

Cross-tenant:

```text
GET Execution
→ 404
```

```text
Execute Pipeline de outro tenant
→ 404
```

Nenhuma resposta deve revelar que o recurso existe.

---

# Endpoint de execução

## Start

```http
POST /api/v1/pipelines/{pipelineId}/executions
```

Request:

```json
{
  "topic": "..."
}
```

Como a execução é síncrona nesta Sprint, o sucesso pode retornar:

```text
201 Created
```

com a execução final já concluída.

Response mínimo:

```text
ExecutionId
PipelineId
Status
CreatedAt
StartedAt
CompletedAt
```

`Location`:

```text
/api/v1/executions/{executionId}
```

---

# HTTP Failure durante processamento

Se a Execution foi criada e o AI Provider falhou durante processamento, não apagar a Execution.

A execução Failed deve permanecer consultável.

Definir comportamento HTTP consistente.

Preferência desta Sprint:

- criação/processamento que termina Failed ainda pode retornar um erro controlado;
    
- `ExecutionId` deve permanecer recuperável quando possível;
    
- GET posterior deve mostrar status Failed.
    

A implementação exata pode ser decidida durante o caso de uso, desde que:

- falha seja persistida;
    
- não exista estado parcialmente invisível;
    
- ProblemDetails não vaze detalhes internos.
    

---

# Read Side

Criar:

```text
IPipelineExecutionQueries
```

separado do repository de escrita.

Get deve:

- usar `AsNoTracking`;
    
- projetar diretamente;
    
- ser tenant-scoped;
    
- não carregar Aggregate para leitura;
    
- projetar Steps;
    
- projetar Artifacts.
    

---

# Repository

Criar abstraction mínima para escrita.

Possível:

```text
IPipelineExecutionRepository
```

Com apenas operações necessárias.

Não criar Generic Repository.

Artifact e StepExecution devem permanecer coerentes com o Aggregate escolhido.

Não criar repositories individuais sem necessidade concreta.

---

# E2E principal

Criar teste HTTP completo:

```text
Create Project
    ↓
Create Pipeline
    ↓
Add Research
    ↓
Add Script
    ↓
Publish
    ↓
Execute Pipeline
    ↓
Get Execution
```

Validar:

## Execution

```text
Status = Completed
PipelineVersion = 1
Topic correto
```

## Research

```text
Status = Completed
Artifact Research existe
```

## Script

```text
Status = Completed
Artifact Script existe
```

## Composição

O Script Artifact deve conter evidência de que recebeu Research como contexto.

Esse teste é essencial.

---

# Testes Domain

Cobrir pelo menos:

## PipelineExecution

- criação válida;
    
- UUID v7;
    
- status inicial;
    
- Topic trim;
    
- Topic obrigatório;
    
- IDs obrigatórios;
    
- version inválida;
    
- Start;
    
- Complete;
    
- Fail;
    
- não completar duas vezes;
    
- não iniciar Execution finalizada;
    
- timestamps pelo IClock.
    

## StepExecution

- criação;
    
- Start;
    
- Complete;
    
- Fail;
    
- posição inválida;
    
- estados finais imutáveis.
    

## Artifact

- criação válida;
    
- ID UUID v7;
    
- content obrigatório;
    
- type válido;
    
- IDs obrigatórios.
    

---

# Testes Application

Cobrir pelo menos:

## Execute Pipeline

- Pipeline Published executa;
    
- Pipeline Draft é rejeitado;
    
- Pipeline inexistente;
    
- Pipeline cross-tenant;
    
- Organization ausente;
    
- User ausente;
    
- Topic inválido;
    
- Research executa primeiro;
    
- Script executa depois;
    
- Research Artifact é enviado para Script;
    
- Artifacts são persistidos;
    
- Execution completa;
    
- version 1 é registrada;
    
- CancellationToken é propagado.
    

## Falhas

- falha Research marca Research Failed;
    
- falha Research marca Execution Failed;
    
- falha Research impede Script;
    
- falha Script mantém Research Completed;
    
- falha Script marca Script Failed;
    
- falha Script marca Execution Failed.
    

---

# Testes Data/PostgreSQL

Usar PostgreSQL real.

Cobrir:

- persistência PipelineExecution;
    
- materialização;
    
- StepExecutions;
    
- Artifacts;
    
- relationships;
    
- tracking;
    
- status;
    
- timestamps;
    
- failure state;
    
- queries AsNoTracking;
    
- tenant isolation;
    
- migration em banco vazio.
    

Não usar EF InMemory.

---

# Testes API

Cobrir:

## Happy path

```text
POST execution
→ 201
```

Depois:

```text
GET execution
→ 200
```

Com:

- Completed;
    
- duas Steps;
    
- Research Artifact;
    
- Script Artifact.
    

## Pipeline Draft

```text
POST execution
→ erro esperado
```

## Pipeline inexistente

```text
404
```

## Pipeline cross-tenant

```text
404
```

## Execution cross-tenant

```text
404
```

## Topic inválido

```text
400
```

---

# Fake AI tests

Validar explicitamente:

- Research recebe Topic;
    
- Research output é determinístico;
    
- Script recebe Topic;
    
- Script recebe Research Content;
    
- Script output é determinístico;
    
- CancellationToken é respeitado.
    

---

# OpenAPI

Documentar endpoints:

```text
POST /api/v1/pipelines/{pipelineId}/executions
GET /api/v1/executions/{executionId}
```

Incluir:

- request;
    
- success response;
    
- validation;
    
- not found;
    
- authorization;
    
- demais erros esperados.
    

---

# Observabilidade

Não implementar plataforma de observabilidade completa.

Mas utilizar logging estruturado existente quando apropriado.

Campos úteis:

```text
ExecutionId
PipelineId
OrganizationId
StepType
```

Nunca logar secrets.

Não logar conteúdo completo de Artifact por padrão.

---

# Segurança

Garantir:

- autenticação obrigatória;
    
- Organization somente da identidade;
    
- CreatedBy somente da identidade;
    
- cross-tenant 404;
    
- nenhum Aggregate exposto via API;
    
- stack traces não expostos;
    
- erros 500 sanitizados;
    
- Fake Provider sem secrets;
    
- conteúdo de Artifact não utilizado em logs indiscriminadamente.
    

---

# Definition of Done

A Sprint só está concluída quando:

-  PipelineExecution Domain implementado.
    
-  StepExecution Domain implementado.
    
-  Artifact implementado.
    
-  lifecycle de execução testado.
    
-  Fake AI Provider implementado.
    
-  Research executa.
    
-  Research Artifact é persistido.
    
-  Script recebe Research como contexto.
    
-  Script executa.
    
-  Script Artifact é persistido.
    
-  Execution termina Completed.
    
-  falha de Research é persistida corretamente.
    
-  falha de Script é persistida corretamente.
    
-  PostgreSQL mappings implementados.
    
-  migration criada.
    
-  migration validada em banco vazio.
    
-  Get Execution implementado.
    
-  Start Execution endpoint implementado.
    
-  Get Execution endpoint implementado.
    
-  multi-tenancy testado.
    
-  cross-tenant retorna 404.
    
-  OpenAPI atualizado.
    
-  E2E completo aprovado.
    
-  build 0 warnings.
    
-  build 0 errors.
    
-  todos os testes passam.
    
-  nenhuma funcionalidade assíncrona foi antecipada.
    

---

# Critério principal de aceite

O seguinte fluxo deve passar integralmente em teste E2E:

```text
Create Project
↓
Create Pipeline
↓
Add Research
↓
Add Script
↓
Publish Pipeline
↓
Execute Pipeline(topic)
↓
Research via Fake AI
↓
Research Artifact persistido
↓
Script recebe Research
↓
Script via Fake AI
↓
Script Artifact persistido
↓
Execution Completed
↓
GET Execution
```

Resultado final esperado:

```text
Execution
└── Completed
    ├── Research
    │   ├── Completed
    │   └── Research Artifact
    │
    └── Script
        ├── Completed
        └── Script Artifact
```

---

# Estratégia de implementação recomendada

Dividir a Sprint em iterações pequenas.

## Iteração 1 — Domain

Implementar:

- PipelineExecution
    
- StepExecution
    
- Artifact
    
- IDs
    
- statuses
    
- errors
    
- Domain Events necessários
    
- testes Domain
    

Nenhuma persistência.

---

## Iteração 2 — Application + AI Abstraction

Implementar:

- Execute Pipeline use case
    
- abstractions de repository
    
- AI Provider abstraction
    
- Fake AI Provider
    
- lifecycle orchestration
    
- testes Application
    
- testes Fake AI
    

Ainda sem API.

---

## Iteração 3 — Data

Implementar:

- EF mappings
    
- repositories
    
- queries
    
- migration
    
- PostgreSQL integration tests
    

---

## Iteração 4 — API + E2E

Implementar:

- POST Execution
    
- GET Execution
    
- HTTP contracts
    
- ProblemDetails
    
- OpenAPI
    
- E2E completo
    
- cross-tenant
    

---

# Commits sugeridos

## Domain

```text
feat(executions): implement execution domain model
```

## Application / AI

```text
feat(executions): orchestrate pipeline execution with fake ai
```

## Data

```text
feat(data): persist pipeline executions and artifacts
```

## API

```text
feat(api): expose pipeline execution endpoints
```

## Fechamento

```text
docs(sprint): complete sprint 02 execution vertical slice
```

---

# Tag planejada

Após merge e validação:

```text
sprint-02-complete
```

---

# Próxima Sprint

Após esta Sprint, o fluxo síncrono estará comprovado.

A próxima evolução será transformar:

```text
HTTP
↓
Execute tudo
↓
Response
```

em:

```text
HTTP
↓
Create Execution
↓
Outbox
↓
RabbitMQ
↓
Worker
↓
Research
↓
Script
↓
Artifacts
```

A próxima Sprint deverá introduzir, de forma incremental:

- Outbox;
    
- RabbitMQ;
    
- Worker;
    
- execução assíncrona;
    
- Inbox/idempotência;
    
- retry/recovery.
    

Somente depois disso deverá ser conectado um provider real de IA.