# Sprint 01 — Pipelines e Versionamento

## 1. Identificação

```text
Sprint: Sprint 01 — Pipelines e Versionamento
Status: Planejada
Marco: M1 — MVP Funcional
Prioridade: P0
Sprint anterior: Sprint 00 — Fundação e Primeiro Vertical Slice
```

---

# 2. Objetivo da Sprint

Implementar o segundo vertical slice do Infinite Content AI: a criação e configuração de Pipelines dentro de um Project.

Ao final da Sprint, deverá ser possível executar o fluxo:

```text
Project existente
    ↓
Criar Pipeline
    ↓
Adicionar Research Step
    ↓
Adicionar Script Step
    ↓
Validar configuração
    ↓
Publicar Pipeline
    ↓
Consultar Pipeline publicado
```

A Sprint deverá estabelecer as regras de domínio necessárias para que, futuramente, uma `PipelineExecution` possa executar uma versão imutável e conhecida de um Pipeline.

---

# 3. Contexto

A Sprint 00 entregou:

- Solution estruturada.
    
- Shared Kernel.
    
- Organization Context.
    
- Autenticação fake.
    
- Project Domain Model.
    
- PostgreSQL.
    
- EF Core.
    
- Create Project.
    
- Get Project.
    
- List Projects.
    
- Problem Details.
    
- Testes de integração.
    
- Isolamento multi-tenant.
    

A Sprint 01 utilizará essa fundação sem alterar seus conceitos centrais.

O foco agora será modelar:

```text
Project
    ↓
Pipeline
    ↓
Pipeline Steps
    ↓
Pipeline Version
```

---

# 4. Resultado Esperado

A Sprint deverá entregar:

- Aggregate `Pipeline`.
    
- Identificador fortemente tipado.
    
- Status de Pipeline.
    
- Etapas de Pipeline.
    
- Tipos de Step.
    
- Criação de Pipeline.
    
- Adição de Research Step.
    
- Adição de Script Step.
    
- Validação da ordem das etapas.
    
- Publicação do Pipeline.
    
- Versionamento mínimo.
    
- Imutabilidade após publicação.
    
- Consulta de Pipeline.
    
- Listagem de Pipelines por Project.
    
- Persistência PostgreSQL.
    
- Migration correspondente.
    
- Endpoints HTTP.
    
- Problem Details.
    
- Testes unitários.
    
- Testes de Application.
    
- Testes relacionais.
    
- Testes HTTP end-to-end.
    
- Isolamento por Organization.
    

---

# 5. Escopo da Sprint

## Incluído

```text
Pipeline
PipelineId
PipelineStatus
PipelineStep
PipelineStepId
PipelineStepType
PipelineVersion
Create Pipeline
Add Research Step
Add Script Step
Publish Pipeline
Get Pipeline
List Pipelines by Project
PostgreSQL
EF Core mappings
Migration
API endpoints
Problem Details
Cross-tenant tests
```

## Não incluído

```text
PipelineExecution
StepExecution
Artifact
Research Artifact
Script Artifact
Fake AI Provider
OpenAI
RabbitMQ
Worker
Outbox
Inbox
Retry
Recovery
n8n
Redis
Approval
Publication
```

Nenhum comportamento de execução deverá ser implementado nesta Sprint.

---

# 6. Princípio Central

Um Pipeline publicado deverá representar uma configuração válida e estável.

A regra principal será:

> Uma Execution futura nunca deverá depender de uma configuração de Pipeline que possa mudar silenciosamente durante sua execução.

Para isso:

```text
Draft
    ↓
Configuração editável
    ↓
Publish
    ↓
Versão estável
```

Depois de publicado, o Pipeline não deverá ser alterado diretamente.

Mudanças futuras deverão gerar uma nova versão ou um novo draft, conforme a evolução do produto.

---

# 7. Fluxo Principal da Sprint

```text
Criar Project
    ↓
Criar Pipeline
    ↓
Pipeline = Draft
    ↓
Adicionar Research Step
    ↓
Adicionar Script Step
    ↓
Publicar
    ↓
Pipeline = Published
    ↓
Consultar configuração publicada
```

---

# 8. Histórias Selecionadas

---

## US-040 — Criar Pipeline

**Prioridade:** P0

**Épico:** EPIC-04 — Pipelines

### História

**Como usuário**, quero criar um Pipeline dentro de um Project, para definir um processo reutilizável de geração de conteúdo.

### Regras

- Pipeline pertence a uma Organization.
    
- Pipeline pertence a um Project.
    
- Project deve existir.
    
- Project e Pipeline devem pertencer à mesma Organization.
    
- Nome é obrigatório.
    
- Nome deverá possuir limite explícito.
    
- Descrição será opcional.
    
- Status inicial será `Draft`.
    
- Versão inicial deverá ser definida.
    
- Pipeline não possuirá Steps automaticamente.
    
- `CreatedAt` deverá utilizar `IClock`.
    
- `CreatedBy` deverá ser registrado.
    
- IDs novos deverão utilizar UUID v7.
    

### Domain

Criar:

```text
Pipeline
PipelineId
PipelineName
PipelineStatus
PipelineErrors
PipelineCreatedDomainEvent
```

### Application

Criar:

```text
CreatePipelineCommand
CreatePipelineHandler
CreatePipelineValidator
CreatePipelineResult
IPipelineRepository
```

### Data

Implementar:

- Mapping.
    
- Repository.
    
- DbSet.
    
- Constraints.
    
- Índices.
    
- Migration.
    

### API

Endpoint:

```http
POST /api/v1/projects/{projectId}/pipelines
```

### Critérios de aceite

- Project válido permite criar Pipeline.
    
- Project inexistente retorna `404`.
    
- Project de outra Organization retorna `404`.
    
- Nome inválido retorna `400`.
    
- Pipeline inicia como `Draft`.
    
- Pipeline é persistido.
    
- Pipeline retorna versão inicial.
    
- API retorna `201 Created`.
    

---

# 9. US-041 — Adicionar Research Step

**Prioridade:** P0

### História

**Como usuário**, quero adicionar uma Research Step ao Pipeline, para definir que a pesquisa será executada antes da criação do roteiro.

### Conceito

```text
Pipeline
└── Research Step
```

### Regras

- Pipeline deve existir.
    
- Pipeline deve estar em `Draft`.
    
- Step deve pertencer ao mesmo Aggregate.
    
- `StepType` será `Research`.
    
- Step possui posição.
    
- Posição deve ser positiva.
    
- Posição não poderá ser duplicada.
    
- Um Pipeline MVP poderá possuir apenas uma Research Step.
    
- A Step não deverá conter lógica de IA.
    
- A Step deverá representar apenas a configuração do processo.
    

### Domain

Criar:

```text
PipelineStep
PipelineStepId
PipelineStepType
```

Tipo inicial:

```text
Research
```

### Application

Criar:

```text
AddPipelineStepCommand
AddPipelineStepHandler
```

ou uma estrutura específica equivalente que preserve a organização por feature.

### API

Endpoint recomendado:

```http
POST /api/v1/pipelines/{pipelineId}/steps
```

Request:

```json
{
  "type": "research",
  "position": 1
}
```

### Critérios de aceite

- Research pode ser adicionada a Pipeline Draft.
    
- Step recebe identificador.
    
- Step é persistida.
    
- Posição duplicada retorna `409` ou erro de validação conforme a decisão adotada.
    
- Research duplicada é rejeitada.
    
- Pipeline publicado rejeita alteração.
    
- Cross-tenant retorna `404`.
    

---

# 10. US-042 — Adicionar Script Step

**Prioridade:** P0

### História

**Como usuário**, quero adicionar uma Script Step, para definir que um roteiro será produzido após a pesquisa.

### Fluxo esperado

```text
1. Research
2. Script
```

### Regras

- Pipeline deve estar em `Draft`.
    
- Script deve vir depois de Research no MVP.
    
- Apenas uma Script Step será permitida.
    
- Position deve ser única.
    
- Pipeline não pode aceitar Script antes de Research caso isso produza configuração inválida.
    
- A regra final de publicação deverá confirmar toda a sequência.
    

### StepType

Adicionar:

```text
Script
```

### Critérios de aceite

- Script após Research é aceita.
    
- Script duplicada é rejeitada.
    
- Script antes de Research é rejeitada ou torna publicação impossível, conforme a regra adotada pelo Domain.
    
- Position duplicada é rejeitada.
    
- Pipeline publicado não pode receber Step.
    

---

# 11. US-043 — Publicar Pipeline

**Prioridade:** P0

### História

**Como usuário**, quero publicar um Pipeline válido, para torná-lo disponível para futuras Executions.

### Estado

Antes:

```text
Draft
```

Depois:

```text
Published
```

### Regras obrigatórias

Para publicação, o Pipeline deverá possuir exatamente o fluxo mínimo válido:

```text
Position 1 → Research
Position 2 → Script
```

Ou equivalente ordenado que respeite:

```text
Research
    ↓
Script
```

### Publicação deve falhar quando

- Não existem Steps.
    
- Research está ausente.
    
- Script está ausente.
    
- Script aparece antes de Research.
    
- Existem posições duplicadas.
    
- Pipeline já está Published.
    
- Pipeline pertence a outra Organization.
    

### Versionamento

O Pipeline deverá possuir uma versão explícita.

Modelo inicial:

```text
Version = 1
```

A publicação deverá fixar a configuração correspondente à versão publicada.

Nesta Sprint, não será necessário implementar edição completa e geração de versão 2.

Entretanto, o modelo deverá evitar uma decisão que torne versões futuras impossíveis.

### Regra de imutabilidade

Depois de `Published`:

- Nome não deverá ser alterado nesta Sprint.
    
- Steps não poderão ser adicionadas.
    
- Steps não poderão ser removidas.
    
- Steps não poderão mudar de posição.
    
- Configuração não poderá ser alterada.
    

### Domain Event

Criar, se consistente com o modelo atual:

```text
PipelinePublishedDomainEvent
```

### API

```http
POST /api/v1/pipelines/{pipelineId}/publish
```

### Critérios de aceite

- Pipeline válido é publicado.
    
- `PublishedAt` é registrado.
    
- Status passa para Published.
    
- Versão publicada fica registrada.
    
- Pipeline inválido retorna erro controlado.
    
- Operação repetida não altera silenciosamente o Aggregate.
    

---

# 12. US-044 — Consultar Pipeline

**Prioridade:** P0

### História

**Como usuário**, quero consultar um Pipeline, para entender sua configuração e suas etapas.

### Endpoint

```http
GET /api/v1/pipelines/{pipelineId}
```

### Resposta esperada

```json
{
  "id": "019c...",
  "projectId": "019c...",
  "name": "Pesquisa e Roteiro",
  "status": "published",
  "version": 1,
  "steps": [
    {
      "id": "019c...",
      "type": "research",
      "position": 1
    },
    {
      "id": "019c...",
      "type": "script",
      "position": 2
    }
  ],
  "createdAt": "2026-08-07T12:00:00Z",
  "publishedAt": "2026-08-07T12:05:00Z"
}
```

### Critérios de aceite

- Steps retornam ordenadas.
    
- Aggregate de Domain não é retornado diretamente.
    
- Query utiliza read model.
    
- Query utiliza `AsNoTracking`.
    
- Organization é aplicada obrigatoriamente.
    
- Recurso de outro tenant retorna `404`.
    

---

# 13. US-045 — Listar Pipelines de um Project

**Prioridade:** P1

### História

**Como usuário**, quero listar os Pipelines de um Project, para visualizar e reutilizar processos existentes.

### Endpoint

```http
GET /api/v1/projects/{projectId}/pipelines?page=1&pageSize=20
```

### Item da listagem

```json
{
  "id": "019c...",
  "name": "Pesquisa e Roteiro",
  "status": "published",
  "version": 1,
  "stepCount": 2,
  "createdAt": "2026-08-07T12:00:00Z"
}
```

### Regras

- Project deve pertencer à Organization atual.
    
- Listagem deve ser paginada.
    
- Ordenação deve ser determinística.
    
- Não carregar Aggregate completo.
    
- Utilizar projeção.
    
- Steps completas não precisam ser carregadas na listagem.
    

### Critérios de aceite

- Retorna somente Pipelines do Project.
    
- Retorna somente Pipelines da Organization.
    
- Project de outro tenant retorna `404`.
    
- Página e tamanho são respeitados.
    

---

# 14. Modelo de Domínio Inicial

Estrutura conceitual:

```text
Pipeline
├── PipelineId
├── OrganizationId
├── ProjectId
├── PipelineName
├── PipelineStatus
├── Version
├── CreatedAt
├── CreatedBy
├── PublishedAt
└── Steps
    ├── PipelineStep
    │   ├── PipelineStepId
    │   ├── StepType
    │   └── Position
    └── ...
```

---

# 15. PipelineStatus

Estados iniciais:

```text
Draft
Published
```

Não adicionar ainda:

```text
Archived
Deprecated
Disabled
Deleted
```

Esses estados somente deverão ser adicionados quando existir comportamento que os utilize.

---

# 16. PipelineStepType

Tipos permitidos nesta Sprint:

```text
Research
Script
```

Não adicionar:

```text
SEO
Thumbnail
Voice
Video
Translation
Publishing
Analytics
Trend
Custom
```

Esses tipos pertencem a fases futuras.

---

# 17. Versionamento

## Objetivo

Permitir que uma Execution futura saiba exatamente qual configuração executou.

### Requisito mínimo

Pipeline deverá possuir:

```text
Version
```

Exemplo:

```text
PipelineId: A
Version: 1
Status: Published
```

### Nesta Sprint

Somente a primeira versão será necessária.

Não implementar ainda:

```text
Edit Published Pipeline
Clone Pipeline
Create Draft From Version
Version 2
Rollback
Compare Versions
```

Entretanto, o modelo deverá deixar claro que:

> Uma versão publicada não é modificável.

---

# 18. Decisão Importante de Modelagem

A implementação não deverá tratar `Version` apenas como um número decorativo.

A configuração publicada precisa poder ser identificada futuramente por:

```text
PipelineId + Version
```

A Sprint deverá avaliar se a versão será:

1. Propriedade do próprio Aggregate inicial.
    

ou

2. Um conceito persistido separado.
    

Para o MVP, escolher a solução mais simples que ainda preserve imutabilidade e permita evolução.

Não implementar infraestrutura de versionamento sofisticada sem necessidade atual.

---

# 19. Persistência

Tabelas esperadas:

```text
pipelines
pipeline_steps
```

Estrutura aproximada:

```text
pipelines
---------
id
organization_id
project_id
name
description
status
version
created_at
created_by
published_at
```

```text
pipeline_steps
--------------
id
pipeline_id
type
position
```

---

# 20. Constraints Importantes

O banco deverá reforçar invariantes úteis.

Avaliar:

- FK `Pipeline → Project`.
    
- FK `PipelineStep → Pipeline`.
    
- Índice por Organization + Project.
    
- Unique constraint de posição por Pipeline.
    
- Check ou configuração equivalente para position positiva.
    
- Índice para listagem.
    
- Limites de tamanho alinhados ao Domain.
    

O banco não deverá ser a única autoridade das regras.

As invariantes também deverão existir no Domain.

---

# 21. Queries

Criar abstração específica:

```text
IPipelineQueries
```

Operações mínimas:

```text
GetAsync
ListByProjectAsync
```

Regras:

- `AsNoTracking`.
    
- Projeção.
    
- Organization obrigatória.
    
- Ordenação determinística.
    
- Não retornar Aggregate diretamente.
    

---

# 22. Repository

Criar:

```text
IPipelineRepository
```

Somente operações realmente necessárias.

Exemplos possíveis:

```text
AddAsync
GetForUpdateAsync
```

Evitar:

```text
GenericRepository<T>
GetAll
UpdateGeneric
DeleteGeneric
Find(Expression<...>)
```

O repository deverá servir aos casos de escrita do Aggregate.

---

# 23. Unit of Work

Utilizar o `IUnitOfWork` existente.

Operações de criação, adição de Step e publicação deverão persistir mudanças através da mesma abstração.

Não introduzir nova abstração transacional nesta Sprint.

---

# 24. Organization Isolation

Todas as operações deverão respeitar:

```text
OrganizationId
```

### Criar Pipeline

Confirmar:

```text
Project.OrganizationId == CurrentOrganization
```

### Alterar Pipeline

Carregar utilizando:

```text
PipelineId + OrganizationId
```

### Consultar

Aplicar Organization na query.

### Cross-Tenant

Comportamento padrão:

```text
404 Not Found
```

Não revelar que o recurso existe em outro tenant.

---

# 25. Project Ownership

Um Pipeline nunca poderá ser associado arbitrariamente a um `ProjectId` que pertença a outra Organization.

Essa regra deverá ser validada antes da criação.

O usuário não deverá fornecer:

```text
OrganizationId
```

em nenhum request.

---

# 26. Endpoints da Sprint

## Criar Pipeline

```http
POST /api/v1/projects/{projectId}/pipelines
```

---

## Listar Pipelines

```http
GET /api/v1/projects/{projectId}/pipelines?page=1&pageSize=20
```

---

## Consultar Pipeline

```http
GET /api/v1/pipelines/{pipelineId}
```

---

## Adicionar Step

```http
POST /api/v1/pipelines/{pipelineId}/steps
```

---

## Publicar Pipeline

```http
POST /api/v1/pipelines/{pipelineId}/publish
```

---

# 27. Exemplo Completo

## Criar Pipeline

```http
POST /api/v1/projects/{projectId}/pipelines
```

```json
{
  "name": "Pesquisa e Roteiro",
  "description": "Fluxo padrão para produção de vídeos."
}
```

Resposta:

```http
201 Created
```

---

## Adicionar Research

```http
POST /api/v1/pipelines/{pipelineId}/steps
```

```json
{
  "type": "research",
  "position": 1
}
```

---

## Adicionar Script

```json
{
  "type": "script",
  "position": 2
}
```

---

## Publicar

```http
POST /api/v1/pipelines/{pipelineId}/publish
```

Resposta esperada:

```json
{
  "id": "019c...",
  "status": "published",
  "version": 1,
  "publishedAt": "2026-08-07T12:05:00Z"
}
```

---

# 28. Erros Esperados

Códigos sugeridos:

```text
Pipeline.NameRequired
Pipeline.NameTooLong
Pipeline.ProjectNotFound
Pipeline.NotFound
Pipeline.NotDraft
Pipeline.AlreadyPublished
Pipeline.StepPositionInvalid
Pipeline.StepPositionAlreadyExists
Pipeline.ResearchStepAlreadyExists
Pipeline.ScriptStepAlreadyExists
Pipeline.ResearchStepRequired
Pipeline.ScriptStepRequired
Pipeline.InvalidStepOrder
Pipeline.InvalidConfiguration
```

A implementação poderá ajustar nomes desde que:

- sejam estáveis;
    
- sejam específicos;
    
- não dependam de HTTP;
    
- permaneçam documentados nos testes.
    

---

# 29. Testes de Domain

Obrigatórios:

## Pipeline

-  Criar Pipeline válido.
    
-  Nome obrigatório.
    
-  Nome acima do limite.
    
-  Organization obrigatória.
    
-  Project obrigatório.
    
-  Status inicial Draft.
    
-  Version inicial correta.
    
-  Evento de criação registrado.
    

## Research Step

-  Adicionar Research válida.
    
-  Rejeitar position inválida.
    
-  Rejeitar position duplicada.
    
-  Rejeitar segunda Research.
    
-  Rejeitar alteração após publicação.
    

## Script Step

-  Adicionar Script válida.
    
-  Rejeitar segunda Script.
    
-  Rejeitar position duplicada.
    
-  Validar relação com Research.
    

## Publicação

-  Publicar configuração válida.
    
-  Rejeitar Pipeline sem Steps.
    
-  Rejeitar sem Research.
    
-  Rejeitar sem Script.
    
-  Rejeitar ordem inválida.
    
-  Rejeitar segunda publicação.
    
-  Registrar PublishedAt.
    
-  Manter Version estável.
    

---

# 30. Testes de Application

Obrigatórios:

-  CreatePipeline usa Organization atual.
    
-  CreatePipeline confirma existência do Project.
    
-  Project de outro tenant não pode receber Pipeline.
    
-  AddStep carrega Pipeline tenant-scoped.
    
-  AddStep persiste alterações.
    
-  Publish chama regra do Domain.
    
-  Falhas são propagadas como Result.
    
-  Unit of Work é chamado somente em sucesso.
    
-  CancellationToken é propagado.
    
-  Get retorna NotFound corretamente.
    
-  List valida paginação.
    

---

# 31. Testes de Data

Utilizar PostgreSQL real.

Validar:

-  Pipeline persistido.
    
-  Steps persistidas.
    
-  Relação Pipeline → Project.
    
-  Relação Step → Pipeline.
    
-  Value Objects materializados.
    
-  Status materializado.
    
-  Version materializada.
    
-  PublishedAt nullable.
    
-  Ordenação de Steps.
    
-  Unique position.
    
-  Filtro por Organization.
    
-  Listagem por Project.
    
-  Migration funciona partindo de banco vazio.
    

Não utilizar EF Core InMemory.

---

# 32. Testes HTTP End-to-End

## Create Pipeline

-  `201` para request válida.
    
-  `400` para nome inválido.
    
-  `404` para Project inexistente.
    
-  `404` para Project de outra Organization.
    
-  Pipeline realmente persistido.
    

## Add Step

-  Research retorna sucesso.
    
-  Script retorna sucesso.
    
-  Step duplicada retorna erro.
    
-  Pipeline publicado rejeita alteração.
    
-  Cross-tenant retorna `404`.
    

## Publish

-  Pipeline válido é publicado.
    
-  Pipeline incompleto retorna erro.
    
-  Segunda publicação retorna erro.
    
-  PublishedAt é retornado.
    
-  Version é retornada.
    

## Get

-  Steps aparecem ordenadas.
    
-  Outro tenant recebe `404`.
    

## List

-  Paginação funciona.
    
-  Somente Pipelines do Project aparecem.
    
-  Somente Pipelines da Organization aparecem.
    

---

# 33. Testes de Arquitetura

Garantir que as novas implementações não alterem os limites existentes.

Verificar:

- Domain continua sem EF Core.
    
- Application continua sem Data.
    
- Application continua sem ASP.NET Core.
    
- API não acessa DbContext diretamente.
    
- Infrastructure não recebe persistência de Pipeline.
    
- Queries permanecem no Data.
    
- Contracts permanece inalterado nesta Sprint.
    

---

# 34. Ordem de Implementação

A Sprint deverá seguir preferencialmente:

```text
1. PipelineId e PipelineName
2. PipelineStatus
3. Pipeline Aggregate
4. PipelineStep
5. PipelineStepType
6. Regras de adição de Steps
7. Regras de publicação
8. Domain tests
9. Create Pipeline Application
10. Add Step Application
11. Publish Pipeline Application
12. Get/List Queries
13. EF mappings
14. Migration
15. PostgreSQL tests
16. API endpoints
17. HTTP integration tests
18. Cross-tenant validation
19. Revisão arquitetural
20. Demo
```

---

# 35. Marcos Intermediários

## Marco A — Domain de Pipeline

Concluído quando:

-  Pipeline pode ser criado.
    
-  Research pode ser adicionada.
    
-  Script pode ser adicionada.
    
-  Configuração inválida é rejeitada.
    
-  Pipeline válido pode ser publicado.
    
-  Pipeline publicado é imutável.
    

---

## Marco B — Persistência

Concluído quando:

-  Pipeline é persistido.
    
-  Steps são persistidas.
    
-  Migration funciona em banco vazio.
    
-  Constraints são verificadas.
    
-  PostgreSQL tests passam.
    

---

## Marco C — Application

Concluído quando:

-  Create Pipeline funciona.
    
-  Add Step funciona.
    
-  Publish funciona.
    
-  Get funciona.
    
-  List funciona.
    
-  Organization é respeitada.
    

---

## Marco D — API

Concluído quando:

```text
POST /projects/{projectId}/pipelines
POST /pipelines/{pipelineId}/steps
POST /pipelines/{pipelineId}/publish
GET  /pipelines/{pipelineId}
GET  /projects/{projectId}/pipelines
```

funcionarem de ponta a ponta.

---

# 36. Definition of Done Funcional

A Sprint estará funcionalmente concluída quando:

-  Um Pipeline puder ser criado dentro de um Project.
    
-  Research Step puder ser adicionada.
    
-  Script Step puder ser adicionada.
    
-  A ordem for validada.
    
-  Um Pipeline completo puder ser publicado.
    
-  Um Pipeline publicado não puder mais ser alterado.
    
-  Pipeline puder ser consultado.
    
-  Pipelines puderem ser listados por Project.
    
-  Version estiver persistida e retornada.
    
-  Organization isolation estiver comprovado.
    

---

# 37. Definition of Done Técnica

-  Build com 0 warnings.
    
-  Todos os testes existentes continuam verdes.
    
-  Novos testes de Domain passam.
    
-  Novos testes de Application passam.
    
-  Novos testes PostgreSQL passam.
    
-  Novos testes HTTP passam.
    
-  Migration funciona em banco vazio.
    
-  `dotnet format --verify-no-changes` passa.
    
-  Nenhuma dependência arquitetural indevida foi criada.
    
-  Nenhuma funcionalidade fora do escopo foi adicionada.
    

---

# 38. Comandos de Validação

Ao final:

```text
dotnet restore InfiniteContentAI.sln
dotnet format InfiniteContentAI.sln --no-restore
dotnet build InfiniteContentAI.sln --no-restore
dotnet test InfiniteContentAI.sln --no-restore --no-build
dotnet format InfiniteContentAI.sln --verify-no-changes --no-restore
docker compose config
docker compose ps
```

PostgreSQL deverá estar saudável durante os testes relacionais.

---

# 39. Riscos da Sprint

## Versionamento excessivamente complexo

### Risco

Criar sistema de versões completo antes de existir Execution.

### Mitigação

Implementar apenas Version 1 e imutabilidade pós-publicação.

---

## Pipeline virar workflow engine

### Risco

Adicionar branching, condições, loops e dependências arbitrárias.

### Mitigação

No MVP, Pipeline é linear:

```text
Research
    ↓
Script
```

---

## Step armazenar comportamento técnico

### Risco

Misturar prompts, SDK de IA ou lógica de execução no Aggregate.

### Mitigação

PipelineStep representa somente configuração de domínio.

---

## Regras somente na API

### Risco

Validar ordem e publicação exclusivamente no endpoint.

### Mitigação

Domain deverá ser autoridade das invariantes.

---

## Tenant leak

### Risco

Pipeline ser carregado somente por ID.

### Mitigação

Todas as operações devem considerar Organization.

---

## Mutabilidade depois de Publish

### Risco

Futuras Executions utilizarem configuração que mudou.

### Mitigação

Published é imutável.

---

# 40. Itens Proibidos Nesta Sprint

Não adicionar:

```text
Execute()
RunPipeline()
ResearchAgent
ScriptAgent
IAiProvider
RabbitMQ
OutboxMessage
InboxMessage
PipelineExecution
StepExecution
Artifact
Prompt
Model
TokenUsage
Retry
DeadLetter
Worker consumer
```

Esses conceitos entram nas próximas fases.

---

# 41. Demonstração da Sprint

## Cenário

Project existente:

```text
Canal de Tecnologia
```

Criar Pipeline:

```text
Pesquisa e Roteiro
```

Adicionar:

```text
1. Research
2. Script
```

Publicar.

Consultar.

### Demonstração de regras

Também mostrar:

- tentativa de Script duplicada;
    
- tentativa de publicação sem Research;
    
- tentativa de alterar Pipeline publicado;
    
- tentativa cross-tenant;
    
- listagem paginada.
    

---

# 42. Critério de Demonstração

O fluxo abaixo deverá funcionar sem intervenção no banco:

```text
POST Project
    ↓
POST Pipeline
    ↓
POST Research Step
    ↓
POST Script Step
    ↓
POST Publish
    ↓
GET Pipeline
```

Resposta final deverá mostrar:

```text
Status: Published
Version: 1

Steps:
1. Research
2. Script
```

---

# 43. Retrospectiva

Ao final registrar:

## O que funcionou

- O modelo de Aggregate ficou simples?
    
- As regras ficaram concentradas no Domain?
    
- A persistência ficou natural?
    
- O versionamento ficou suficiente para Execution?
    

## O que não funcionou

- Houve excesso de abstrações?
    
- Alguma regra ficou dividida entre camadas?
    
- A API ficou complexa?
    

## Melhorias

- Algum conceito deverá ser simplificado antes da Sprint seguinte?
    
- O modelo suporta uma Execution referenciando `PipelineId + Version`?
    
- Há alguma decisão que precisa virar ADR?
    

---

# 44. Próxima Sprint

Após a conclusão desta Sprint, o próximo fluxo deverá introduzir:

```text
Pipeline publicado
    ↓
Start Pipeline Execution
    ↓
Step Executions
    ↓
Execution State
```

A próxima Sprint deverá começar o domínio de:

```text
PipelineExecution
StepExecution
```

Ainda podendo utilizar processamento funcional antes de RabbitMQ.

---

# 45. Resumo Executivo

## Meta

Construir e publicar Pipelines reutilizáveis.

## Aggregate principal

```text
Pipeline
```

## Configuração MVP

```text
Research
    ↓
Script
```

## Estados

```text
Draft
Published
```

## Versionamento

```text
Version 1
```

com configuração imutável após publicação.

## Persistência

```text
PostgreSQL
EF Core
```

## APIs

```text
Create Pipeline
Add Step
Publish Pipeline
Get Pipeline
List Pipelines
```

## Critério central

> Ao final da Sprint 01, um usuário deverá conseguir configurar e publicar um Pipeline linear e imutável composto por Research e Script, associado a um Project e isolado por Organization.