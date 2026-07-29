# Sprint 00 - Fundação e Primeiro Vertical Slice

## 1. Identificação

```text
Sprint: Sprint 01 — Fundação e Primeiro Vertical Slice
Status: Planejada
Marco: M1 — MVP Funcional
Prioridade: P0
```

---

# 2. Objetivo da Sprint

Construir a fundação técnica do Infinite Content AI e entregar o primeiro fluxo vertical funcional:

```text
POST /api/v1/projects
    ↓
CreateProjectCommand
    ↓
Project.Create
    ↓
ProjectRepository
    ↓
PostgreSQL
    ↓
201 Created
```

Ao final da Sprint, deverá ser possível criar, consultar e listar Projects persistidos em PostgreSQL, respeitando o isolamento por Organization.

---

# 3. Resultado Esperado

A Sprint deverá entregar:

- Solution criada.
    
- Projetos organizados.
    
- Referências arquiteturais configuradas.
    
- Shared Kernel implementado.
    
- Organization Context mínimo.
    
- Autenticação fake de desenvolvimento.
    
- Project implementado no Domain.
    
- Caso de uso `CreateProject`.
    
- Persistência com EF Core e PostgreSQL.
    
- Endpoints de criação, consulta e listagem.
    
- Problem Details.
    
- Testes unitários e de integração.
    
- Docker Compose com PostgreSQL.
    
- Primeiro vertical slice demonstrável.
    

---

# 4. Escopo da Sprint

## Incluído

```text
Fundação da solution
Shared Kernel
Organization Context mínimo
Autenticação fake local
Project Domain Model
Create Project
Get Project
List Projects
PostgreSQL
EF Core
Migration inicial
API endpoints
Problem Details
Testes essenciais
Docker Compose local
```

## Não incluído

```text
Pipeline
PipelineExecution
Artifact
RabbitMQ
Worker
Outbox
Inbox
Redis
Provider de IA
Research Agent
Script Agent
OpenAI
n8n
Deploy em produção
Interface gráfica
```

Esses itens deverão permanecer no Backlog.

---

# 5. Histórias Selecionadas

## US-001 — Criar a solution

**Prioridade:** P0

**Objetivo:** criar os projetos e referências iniciais.

### Tarefas

-  Criar solution.
    
-  Criar pasta `src`.
    
-  Criar pasta `tests`.
    
-  Criar projeto `Api`.
    
-  Criar projeto `Application`.
    
-  Criar projeto `Domain`.
    
-  Criar projeto `Data`.
    
-  Criar projeto `Infrastructure`.
    
-  Criar projeto `Worker`.
    
-  Criar projeto `Contracts`.
    
-  Criar projeto `SharedKernel`.
    
-  Adicionar projetos à solution.
    
-  Configurar referências permitidas.
    
-  Executar `dotnet build`.
    

### Critérios de aceite

- A solution compila.
    
- Não existem referências circulares.
    
- As dependências respeitam a arquitetura documentada.
    

---

## US-002 — Configurar padrões globais

**Prioridade:** P0

### Tarefas

-  Criar `Directory.Build.props`.
    
-  Habilitar nullable reference types.
    
-  Habilitar implicit usings.
    
-  Configurar warnings como erros.
    
-  Criar `.editorconfig`.
    
-  Configurar formatação.
    
-  Definir versão do .NET.
    
-  Configurar analisadores básicos.
    
-  Padronizar namespaces.
    

### Critérios de aceite

- Todos os projetos utilizam as mesmas configurações.
    
- Warnings relevantes impedem o build.
    
- O código segue formatação consistente.
    

---

## US-003 — Criar testes de arquitetura

**Prioridade:** P1

### Tarefas

-  Criar projeto `ArchitectureTests`.
    
-  Testar dependências do Domain.
    
-  Testar dependências da Application.
    
-  Testar dependências da Infrastructure.
    
-  Testar independência do Contracts.
    
-  Testar independência do SharedKernel.
    

### Critérios de aceite

Os testes impedem violações como:

```text
Domain → Application
Application → Data
Infrastructure → Data
Contracts → Domain
SharedKernel → qualquer projeto interno
```

---

## US-010 — Implementar Result Pattern

**Prioridade:** P0

### Tarefas

-  Criar `ErrorType`.
    
-  Criar `Error`.
    
-  Criar `Result`.
    
-  Criar `Result<T>`.
    
-  Criar `Match`.
    
-  Criar factories de sucesso e falha.
    
-  Criar testes unitários.
    

### Critérios de aceite

- Resultado de sucesso não possui erro.
    
- Resultado de falha possui erro.
    
- `Result<T>` não permite acessar `Value` em falha.
    
- Erros possuem código estável.
    
- Nenhum tipo conhece HTTP ou RabbitMQ.
    

---

## US-011 — Implementar primitivas de Domain

**Prioridade:** P0

### Tarefas

-  Criar `Entity<TId>`.
    
-  Criar `AggregateRoot<TId>`.
    
-  Criar `IDomainEvent`.
    
-  Implementar igualdade por identidade.
    
-  Implementar armazenamento de Domain Events.
    
-  Implementar limpeza de Domain Events.
    
-  Criar testes.
    

### Critérios de aceite

- Entities com o mesmo ID são iguais.
    
- Aggregates registram eventos.
    
- Domain Events não dependem de MediatR.
    
- SharedKernel permanece sem dependências externas.
    

---

## US-012 — Implementar abstração de tempo

**Prioridade:** P0

### Tarefas

-  Criar `IClock`.
    
-  Criar `SystemClock`.
    
-  Criar `FakeClock` no projeto de testes.
    
-  Registrar `SystemClock` na DI.
    

### Critérios de aceite

- Domain e Application não utilizam `DateTimeOffset.UtcNow` diretamente.
    
- Testes podem controlar o tempo.
    

---

## US-013 — Implementar paginação

**Prioridade:** P1

### Tarefas

-  Criar `PaginatedResult<T>`.
    
-  Criar teste de cálculo de páginas.
    
-  Definir estrutura de paginação da Application.
    

### Critérios de aceite

- Total de páginas é calculado corretamente.
    
- Limites máximos permanecem na API ou Application.
    

---

## US-020 — Criar contexto de Organization

**Prioridade:** P0

### Tarefas

-  Criar `ICurrentOrganization`.
    
-  Criar `CurrentOrganization`.
    
-  Criar tipo `OrganizationId`.
    
-  Criar middleware ou adapter HTTP.
    
-  Criar implementação para testes.
    
-  Criar erro para Organization ausente.
    

### Critérios de aceite

- Handlers conseguem obter a Organization atual.
    
- OrganizationId não depende do body enviado pelo cliente.
    
- Ausência de Organization resulta em erro controlado.
    
- A estrutura pode ser substituída por autenticação real depois.
    

---

## US-021 — Criar autenticação fake de desenvolvimento

**Prioridade:** P0

### Tarefas

-  Criar Fake Authentication Handler.
    
-  Criar usuário fixo de desenvolvimento.
    
-  Criar Organization fixa de desenvolvimento.
    
-  Configurar claims.
    
-  Habilitar apenas em Development e Test.
    
-  Impedir ativação em Production.
    

### Critérios de aceite

- Endpoints protegidos funcionam localmente.
    
- Organization é obtida de claim.
    
- Produção não aceita autenticação fake.
    

---

## US-030 — Criar Project

**Prioridade:** P0

### Domain

-  Criar `Project`.
    
-  Criar `ProjectId`.
    
-  Criar `ProjectName`.
    
-  Criar `ProjectStatus`.
    
-  Criar `ProjectErrors`.
    
-  Criar `ProjectCreatedDomainEvent`.
    
-  Implementar factory de criação.
    
-  Criar testes do Domain.
    

### Regras

-  Nome obrigatório.
    
-  Nome não pode conter apenas espaços.
    
-  Nome respeita tamanho máximo.
    
-  OrganizationId obrigatório.
    
-  Status inicial é `Active`.
    
-  CreatedAt vem de `IClock`.
    
-  CreatedBy é registrado.
    

### Application

-  Criar `CreateProjectCommand`.
    
-  Criar `CreateProjectHandler`.
    
-  Criar `CreateProjectValidator`.
    
-  Criar `CreateProjectResult`.
    
-  Criar `IProjectRepository`.
    
-  Criar `IUnitOfWork`.
    
-  Criar testes do Handler.
    

### Data

-  Criar `ApplicationDbContext`.
    
-  Criar `ProjectConfiguration`.
    
-  Criar `ProjectRepository`.
    
-  Adicionar DbSet.
    
-  Configurar PostgreSQL.
    
-  Criar migration inicial.
    
-  Criar testes com PostgreSQL real.
    

### API

-  Criar `CreateProjectRequest`.
    
-  Criar `CreateProjectResponse`.
    
-  Criar endpoint.
    
-  Mapear `Result` para HTTP.
    
-  Retornar `201 Created`.
    
-  Adicionar OpenAPI.
    
-  Criar teste de integração.
    

### Critérios de aceite

- Project válido é persistido.
    
- Nome inválido retorna `400`.
    
- Organization atual é registrada.
    
- A resposta contém o ID criado.
    
- O registro permanece após reinicialização.
    

---

## US-031 — Consultar Project

**Prioridade:** P0

### Tarefas

-  Criar `GetProjectQuery`.
    
-  Criar `GetProjectHandler`.
    
-  Criar `ProjectDetails`.
    
-  Criar `IProjectQueries`.
    
-  Implementar query com `AsNoTracking`.
    
-  Criar endpoint.
    
-  Criar testes cross-tenant.
    

### Critérios de aceite

- Project da Organization atual é retornado.
    
- Project inexistente retorna `404`.
    
- Project de outra Organization retorna `404`.
    
- A Entity não é retornada diretamente.
    

---

## US-032 — Listar Projects

**Prioridade:** P1

### Tarefas

-  Criar `ListProjectsQuery`.
    
-  Criar `ProjectListItem`.
    
-  Implementar paginação.
    
-  Implementar ordenação determinística.
    
-  Criar endpoint.
    
-  Criar testes.
    

### Critérios de aceite

- Somente Projects da Organization atual aparecem.
    
- A listagem é paginada.
    
- A query utiliza `AsNoTracking`.
    
- A query projeta somente campos necessários.
    

---

## US-190 — Criar Docker Compose com PostgreSQL

**Prioridade:** P0

### Tarefas

-  Criar `docker-compose.yml`.
    
-  Configurar PostgreSQL.
    
-  Criar volume persistente.
    
-  Configurar health check.
    
-  Criar `.env.example`.
    
-  Documentar comando de inicialização.
    
-  Testar conexão da API.
    

### Critérios de aceite

O ambiente inicia com:

```bash
docker compose up -d
```

O PostgreSQL fica disponível para API e testes locais.

---

# 6. Ordem de Execução

A implementação deverá seguir esta sequência:

```text
1. Criar solution e projetos
2. Configurar padrões globais
3. Configurar referências
4. Criar testes de arquitetura
5. Implementar Result Pattern
6. Implementar Entity e AggregateRoot
7. Implementar IClock
8. Criar Organization Context
9. Criar autenticação fake
10. Criar Project no Domain
11. Criar CreateProject na Application
12. Configurar PostgreSQL e EF Core
13. Implementar ProjectRepository
14. Criar migration
15. Criar endpoint POST
16. Criar endpoint GET por ID
17. Criar listagem
18. Criar testes end-to-end do slice
```

---

# 7. Primeiro Marco Intermediário

## Marco A — Solution saudável

Concluído quando:

-  Solution compila.
    
-  Referências estão corretas.
    
-  Padrões globais estão ativos.
    
-  Testes de arquitetura passam.
    

---

# 8. Segundo Marco Intermediário

## Marco B — Shared Kernel concluído

Concluído quando:

-  Result funciona.
    
-  Entity funciona.
    
-  AggregateRoot funciona.
    
-  Domain Events funcionam.
    
-  IClock funciona.
    
-  Testes unitários passam.
    

---

# 9. Terceiro Marco Intermediário

## Marco C — Project persistido

Concluído quando:

-  Project pode ser criado no Domain.
    
-  Handler persiste o Aggregate.
    
-  Migration está aplicada.
    
-  Registro aparece no PostgreSQL.
    
-  Teste de integração passa.
    

---

# 10. Marco Final da Sprint

## Marco D — Vertical Slice demonstrável

Concluído quando:

```text
POST /api/v1/projects
GET  /api/v1/projects/{projectId}
GET  /api/v1/projects
```

funcionarem de ponta a ponta.

Também deverá ser possível demonstrar:

- Criação bem-sucedida.
    
- Validação de nome.
    
- Persistência.
    
- Consulta.
    
- Paginação.
    
- Isolamento entre Organizations.
    
- Problem Details.
    
- Testes automatizados.
    

---

# 11. Endpoints da Sprint

## Criar Project

```http
POST /api/v1/projects
```

Request:

```json
{
  "name": "Canal de Tecnologia",
  "description": "Conteúdos sobre tecnologia e inteligência artificial."
}
```

Resposta:

```http
201 Created
```

```json
{
  "id": "019c...",
  "name": "Canal de Tecnologia",
  "description": "Conteúdos sobre tecnologia e inteligência artificial.",
  "status": "active",
  "createdAt": "2026-07-28T15:00:00Z"
}
```

---

## Consultar Project

```http
GET /api/v1/projects/{projectId}
```

Resposta:

```json
{
  "id": "019c...",
  "name": "Canal de Tecnologia",
  "description": "Conteúdos sobre tecnologia e inteligência artificial.",
  "status": "active",
  "createdAt": "2026-07-28T15:00:00Z"
}
```

---

## Listar Projects

```http
GET /api/v1/projects?page=1&pageSize=20
```

Resposta:

```json
{
  "items": [
    {
      "id": "019c...",
      "name": "Canal de Tecnologia",
      "status": "active",
      "createdAt": "2026-07-28T15:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 1,
  "totalPages": 1
}
```

---

# 12. Erros Esperados

## Nome obrigatório

```json
{
  "type": "https://errors.infinitecontent.ai/validation",
  "title": "A requisição possui dados inválidos.",
  "status": 400,
  "code": "Project.NameRequired",
  "detail": "O nome do projeto é obrigatório."
}
```

## Project não encontrado

```json
{
  "type": "https://errors.infinitecontent.ai/not-found",
  "title": "O recurso não foi encontrado.",
  "status": 404,
  "code": "Project.NotFound",
  "detail": "O projeto informado não foi encontrado."
}
```

## Organization ausente

```json
{
  "type": "https://errors.infinitecontent.ai/unauthorized",
  "title": "A identidade atual não possui uma organização válida.",
  "status": 401,
  "code": "Identity.OrganizationRequired",
  "detail": "Não foi possível identificar a organização atual."
}
```

---

# 13. Testes Obrigatórios

## Shared Kernel

-  Result de sucesso.
    
-  Result de falha.
    
-  Result com valor.
    
-  Acesso inválido ao valor.
    
-  Igualdade de Entity.
    
-  Domain Events.
    
-  Paginação.
    

## Domain

-  Criar Project válido.
    
-  Rejeitar nome vazio.
    
-  Rejeitar nome com espaços.
    
-  Rejeitar nome acima do limite.
    
-  Status inicial Active.
    
-  Evento de criação registrado.
    

## Application

-  Handler cria Project.
    
-  Handler utiliza Organization atual.
    
-  Handler chama Unit of Work.
    
-  Falha do Domain é propagada.
    

## Data

-  Project é persistido.
    
-  Project pode ser consultado.
    
-  Query filtra Organization.
    
-  Migration funciona em banco vazio.
    
-  Constraints são aplicadas.
    

## API

-  POST retorna `201`.
    
-  POST inválido retorna `400`.
    
-  GET existente retorna `200`.
    
-  GET inexistente retorna `404`.
    
-  Recurso de outra Organization retorna `404`.
    
-  Listagem retorna paginação.
    

---

# 14. Riscos da Sprint

## Excesso de fundação

### Risco

Passar a Sprint inteira criando abstrações sem endpoint funcional.

### Mitigação

O endpoint de Project deve começar assim que as primitivas mínimas estiverem prontas.

---

## Autenticação virar projeto separado

### Risco

Implementar Identity completo antes do primeiro slice.

### Mitigação

Utilizar autenticação fake restrita a Development e Test.

---

## Shared Kernel crescer demais

### Risco

Adicionar helpers e abstrações ainda sem uso.

### Mitigação

Implementar somente os itens definidos nesta Sprint.

---

## Testes de integração ficarem complexos

### Risco

Criar infraestrutura sofisticada antes do primeiro teste.

### Mitigação

Começar com PostgreSQL em container e uma fixture simples.

---

## Organização por features ficar abstrata

### Risco

Criar muitas pastas vazias.

### Mitigação

Criar somente a feature `Projects`.

---

# 15. Decisões da Sprint

## Banco

```text
PostgreSQL
```

## ORM

```text
Entity Framework Core
```

## API

```text
Minimal APIs
```

## IDs

```text
UUID v7
```

## Erros esperados

```text
Result Pattern
```

## Testes de persistência

```text
PostgreSQL real em container
```

## Autenticação local

```text
Fake Authentication
```

---

# 16. Critérios para Não Adicionar Escopo

Durante esta Sprint, uma tarefa deverá ser rejeitada quando:

- Pertencer a Pipeline.
    
- Pertencer a Execution.
    
- Pertencer a Artifact.
    
- Exigir RabbitMQ.
    
- Exigir Worker.
    
- Exigir provider de IA.
    
- Exigir Redis.
    
- Exigir n8n.
    
- Não for necessária para criar, consultar ou listar Project.
    
- Não corrigir risco de segurança ou integridade.
    

O item deverá ser movido para o Backlog.

---

# 17. Demonstração da Sprint

A demonstração deverá seguir:

1. Iniciar PostgreSQL.
    
2. Iniciar API.
    
3. Autenticar com identidade fake.
    
4. Criar Project.
    
5. Consultar Project.
    
6. Listar Projects.
    
7. Mostrar registro no banco.
    
8. Tentar criar Project sem nome.
    
9. Demonstrar Problem Details.
    
10. Tentar acessar Project de outra Organization.
    
11. Demonstrar retorno `404`.
    
12. Executar testes automatizados.
    

---

# 18. Definition of Done da Sprint

A Sprint estará concluída quando:

-  A solution compilar sem warnings.
    
-  Testes de arquitetura passarem.
    
-  Shared Kernel estiver implementado.
    
-  Project Domain Model estiver testado.
    
-  PostgreSQL estiver configurado localmente.
    
-  Migration inicial estiver criada.
    
-  Project puder ser criado.
    
-  Project puder ser consultado.
    
-  Projects puderem ser listados.
    
-  OrganizationId for respeitado.
    
-  Problem Details estiver configurado.
    
-  Testes de integração passarem.
    
-  O fluxo puder ser demonstrado pelo Swagger ou cliente HTTP.
    
-  Nenhuma feature futura tiver sido adicionada.
    

---

# 19. Retrospectiva

Ao final da Sprint, registrar:

## O que funcionou

- Quais decisões aceleraram o desenvolvimento?
    
- Quais abstrações foram realmente úteis?
    
- Quais testes encontraram problemas?
    

## O que não funcionou

- Onde houve complexidade desnecessária?
    
- Qual configuração consumiu mais tempo?
    
- Houve violação de camada?
    

## O que melhorar

- O próximo vertical slice pode ser menor?
    
- Alguma convenção precisa ser atualizada?
    
- O ambiente local está simples?
    

## Próximo passo

A Sprint seguinte deverá iniciar o Épico de Pipelines:

```text
Criar Pipeline
    ↓
Adicionar Research Step
    ↓
Adicionar Script Step
    ↓
Publicar Pipeline
```

---

# 20. Resumo Executivo

## Meta

Entregar o primeiro vertical slice funcional do Infinite Content AI.

## Recurso entregue

```text
Project
```

## Endpoints

```text
POST /api/v1/projects
GET  /api/v1/projects/{projectId}
GET  /api/v1/projects
```

## Infraestrutura

```text
PostgreSQL
EF Core
Docker Compose
```

## Fundação

```text
Solution
Shared Kernel
Organization Context
Autenticação fake
Testes de arquitetura
```

## Critério central

> Ao final da Sprint, um Project deverá atravessar todas as camadas e permanecer salvo no PostgreSQL com isolamento por Organization.