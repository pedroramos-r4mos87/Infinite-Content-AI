# Backlog

## 1. Objetivo

Este documento organiza o trabalho necessário para implementar o MVP do Infinite Content AI.

O backlog deverá transformar o escopo do produto e o plano técnico em:

- Épicos.
    
- Histórias.
    
- Tarefas.
    
- Critérios de aceite.
    
- Dependências.
    
- Prioridades.
    
- Entregas incrementais.
    

O foco principal será concluir o fluxo:

```text
Criar Project
    ↓
Criar Pipeline
    ↓
Adicionar Research e Script
    ↓
Publicar Pipeline
    ↓
Iniciar Execution
    ↓
Gerar Research Artifact
    ↓
Gerar Script Artifact
    ↓
Concluir Execution
```

---

# 2. Princípio de Priorização

A ordem do backlog seguirá quatro princípios.

## 2.1 Fluxo vertical primeiro

Priorizar funcionalidades que atravessem:

```text
API
    ↓
Application
    ↓
Domain
    ↓
Data
    ↓
PostgreSQL
```

antes de aprofundar componentes técnicos isolados.

## 2.2 Fake antes de integração real

O fluxo deverá funcionar com Fake AI Provider antes da integração com provider pago.

## 2.3 Funcional antes de assíncrono

Primeiro validar:

```text
Research
    ↓
Script
```

em uma orquestração funcional.

Depois adicionar:

- Outbox.
    
- RabbitMQ.
    
- Worker.
    
- Inbox.
    
- Retry.
    
- Recovery.
    

## 2.4 Necessidade antes de sofisticação

Não implementar funcionalidades futuras sem um caso de uso atual.

---

# 3. Convenções

## Identificadores

```text
EPIC-XX
US-XXX
TASK-XXX
BUG-XXX
SPIKE-XXX
```

## Prioridades

|Prioridade|Significado|
|---|---|
|P0|Bloqueia o MVP ou protege segurança e integridade|
|P1|Necessário para concluir o MVP|
|P2|Importante após o fluxo principal|
|P3|Futuro ou melhoria opcional|

## Status

```text
Backlog
Ready
In Progress
Blocked
Review
Done
Cancelled
```

## Marcos

```text
M1 — MVP Funcional
M2 — MVP Assíncrono
M3 — Validação com Provider Real
```

---

# 4. Definition of Ready

Uma história estará pronta para implementação quando:

- O objetivo estiver claro.
    
- O comportamento esperado estiver definido.
    
- Os critérios de aceite estiverem escritos.
    
- As dependências estiverem identificadas.
    
- A camada responsável estiver conhecida.
    
- Não houver decisão arquitetural bloqueante.
    
- O trabalho puder ser testado.
    
- O item for pequeno o suficiente para uma entrega incremental.
    

---

# 5. Definition of Done

Uma história será considerada concluída quando:

- O código compilar.
    
- Os testes relacionados passarem.
    
- O comportamento estiver coberto por teste.
    
- O código respeitar os limites arquiteturais.
    
- `OrganizationId` for respeitado.
    
- `CancellationToken` for propagado quando aplicável.
    
- Erros esperados utilizarem Result Pattern.
    
- Logs não expuserem dados sensíveis.
    
- Migrations necessárias estiverem criadas.
    
- Documentação afetada estiver atualizada.
    
- O fluxo puder ser demonstrado.
    

---

# 6. Visão Geral dos Épicos

|Épico|Nome|Marco|Prioridade|
|---|---|--:|--:|
|EPIC-00|Fundação da Solution|M1|P0|
|EPIC-01|Shared Kernel|M1|P0|
|EPIC-02|Organização e Identidade Mínima|M1|P0|
|EPIC-03|Projects|M1|P0|
|EPIC-04|Pipelines|M1|P0|
|EPIC-05|Pipeline Executions|M1|P0|
|EPIC-06|Artifacts|M1|P0|
|EPIC-07|Fake AI Provider|M1|P0|
|EPIC-08|Orquestração Funcional|M1|P0|
|EPIC-09|Contracts|M2|P1|
|EPIC-10|Transactional Outbox|M2|P1|
|EPIC-11|RabbitMQ|M2|P1|
|EPIC-12|Worker e Consumers|M2|P1|
|EPIC-13|Inbox e Deduplicação|M2|P1|
|EPIC-14|Retry e Recovery|M2|P1|
|EPIC-15|Provider Real|M3|P1|
|EPIC-16|Segurança Mínima|M1/M2|P0|
|EPIC-17|Observabilidade|M1/M2|P1|
|EPIC-18|Testes End-to-End|M1/M2|P0|
|EPIC-19|Ambiente Local e CI|M1/M2|P1|
|EPIC-20|Demonstração do MVP|M3|P1|

---

# 7. EPIC-00 — Fundação da Solution

## Objetivo

Criar a base técnica da aplicação.

## US-001 — Criar a solution

**Como desenvolvedor**, quero uma solution organizada em projetos, para implementar o sistema respeitando os limites arquiteturais.

### Critérios de aceite

- Solution criada.
    
- Projetos criados.
    
- Referências configuradas.
    
- Solution compila.
    
- Nenhuma dependência circular existe.
    

### Projetos

```text
Api
Application
Domain
Data
Infrastructure
Worker
Contracts
SharedKernel
```

### Tarefas

-  Criar arquivo `.sln`.
    
-  Criar projetos em `src`.
    
-  Criar pasta `tests`.
    
-  Adicionar projetos à solution.
    
-  Configurar referências permitidas.
    
-  Executar `dotnet build`.
    

---

## US-002 — Configurar padrões globais

**Como desenvolvedor**, quero padrões globais de compilação e análise, para manter consistência entre os projetos.

### Critérios de aceite

- Nullable habilitado.
    
- Warnings tratados como erros.
    
- Implicit Usings configurados.
    
- Formatação consistente.
    
- Configuração compartilhada pelos projetos.
    

### Tarefas

-  Criar `Directory.Build.props`.
    
-  Criar `.editorconfig`.
    
-  Configurar versão do .NET.
    
-  Configurar `Nullable`.
    
-  Configurar `TreatWarningsAsErrors`.
    
-  Configurar analisadores iniciais.
    

---

## US-003 — Criar testes de arquitetura

**Como equipe**, queremos validar automaticamente as referências entre projetos, para evitar violações arquiteturais.

### Critérios de aceite

- Domain não depende de Application.
    
- Application não depende de Data.
    
- Infrastructure não depende de Data.
    
- Contracts não depende de projetos internos.
    
- SharedKernel não depende de projetos internos.
    

### Tarefas

-  Criar `ArchitectureTests`.
    
-  Adicionar testes de dependência.
    
-  Adicionar testes de namespaces.
    
-  Executar no CI.
    

---

# 8. EPIC-01 — Shared Kernel

## US-010 — Implementar Result Pattern

**Como desenvolvedor**, quero representar falhas esperadas sem exceções, para manter o fluxo de erro explícito.

### Critérios de aceite

- Existe `Result`.
    
- Existe `Result<T>`.
    
- Existe `Error`.
    
- Existe `ErrorType`.
    
- Result de sucesso não possui erro.
    
- Result de falha possui erro.
    
- Acesso ao valor de falha lança exceção de uso incorreto.
    

### Tarefas

-  Criar `ErrorType`.
    
-  Criar `Error`.
    
-  Criar `Result`.
    
-  Criar `Result<T>`.
    
-  Criar `Match`.
    
-  Criar testes unitários.
    

---

## US-011 — Implementar primitivas de Domain

**Como desenvolvedor**, quero primitivas comuns de Domain, para manter identidade e eventos consistentes.

### Critérios de aceite

- Existe `Entity<TId>`.
    
- Existe `AggregateRoot<TId>`.
    
- Existe `IDomainEvent`.
    
- Aggregate registra e limpa eventos.
    
- Igualdade de Entity utiliza identidade.
    

### Tarefas

-  Criar `Entity<TId>`.
    
-  Criar `AggregateRoot<TId>`.
    
-  Criar `IDomainEvent`.
    
-  Criar testes de igualdade.
    
-  Criar testes de Domain Events.
    

---

## US-012 — Implementar abstração de tempo

**Como desenvolvedor**, quero controlar o tempo em testes, para validar expiração, leases e timestamps.

### Critérios de aceite

- Existe `IClock`.
    
- Existe implementação real.
    
- Testes podem utilizar Fake Clock.
    

### Tarefas

-  Criar `IClock`.
    
-  Criar `SystemClock`.
    
-  Criar `FakeClock` nos testes.
    
-  Registrar implementação na DI.
    

---

## US-013 — Implementar paginação

**Como consumidor da Application**, quero receber listas paginadas, para evitar consultas ilimitadas.

### Critérios de aceite

- Existe `PaginatedResult<T>`.
    
- Total de páginas é calculado corretamente.
    
- Limites específicos permanecem fora do Shared Kernel.
    

---

# 9. EPIC-02 — Organização e Identidade Mínima

## US-020 — Criar contexto de Organization

**Como sistema multi-tenant**, quero identificar a Organization atual, para isolar os dados dos clientes.

### Critérios de aceite

- Existe `ICurrentOrganization`.
    
- OrganizationId pode ser obtido pela API.
    
- OrganizationId pode ser propagado pelo Worker.
    
- Ausência de Organization gera erro.
    
- O ID não é recebido cegamente do body.
    

### Tarefas

-  Criar abstração de Organization Context.
    
-  Criar implementação HTTP.
    
-  Criar implementação para Worker.
    
-  Criar Fake para testes.
    

---

## US-021 — Criar usuário e Organization de desenvolvimento

**Como desenvolvedor**, quero uma identidade fake local, para testar o MVP sem implementar autenticação completa.

### Critérios de aceite

- Development utiliza usuário e Organization conhecidos.
    
- Produção não habilita autenticação fake.
    
- Identidade pode ser substituída futuramente.
    

### Tarefas

-  Criar Fake Authentication Handler.
    
-  Configurar claims.
    
-  Criar constantes apenas no projeto de desenvolvimento.
    
-  Proteger configuração por ambiente.
    

---

# 10. EPIC-03 — Projects

## US-030 — Criar Project

**Como criador de conteúdo**, quero criar um Project, para organizar conteúdos de um canal ou cliente.

### Critérios de aceite

- Nome obrigatório.
    
- Nome respeita tamanho máximo.
    
- Project pertence à Organization atual.
    
- Status inicial é Active.
    
- `201 Created` é retornado.
    
- Project é persistido no PostgreSQL.
    

### Tarefas de Domain

-  Criar `Project`.
    
-  Criar `ProjectId`.
    
-  Criar `ProjectName`.
    
-  Criar `ProjectStatus`.
    
-  Criar `ProjectErrors`.
    
-  Criar evento de criação.
    

### Tarefas de Application

-  Criar `CreateProjectCommand`.
    
-  Criar Handler.
    
-  Criar Validator.
    
-  Criar `IProjectRepository`.
    
-  Criar resultado.
    

### Tarefas de Data

-  Criar mapping.
    
-  Criar Repository.
    
-  Adicionar DbSet.
    
-  Criar migration.
    
-  Criar testes de integração.
    

### Tarefas de API

-  Criar request.
    
-  Criar response.
    
-  Criar endpoint.
    
-  Mapear Result para HTTP.
    
-  Adicionar documentação OpenAPI.
    

---

## US-031 — Consultar Project

**Como usuário**, quero consultar um Project, para visualizar seu contexto.

### Critérios de aceite

- Project só pode ser acessado pela Organization proprietária.
    
- Recurso inexistente retorna `404`.
    
- Recurso de outra Organization também retorna `404`.
    
- Query utiliza `AsNoTracking`.
    

---

## US-032 — Listar Projects

**Como usuário**, quero listar meus Projects, para selecionar onde trabalhar.

### Critérios de aceite

- Listagem é paginada.
    
- Apenas Projects da Organization atual aparecem.
    
- Ordenação é determinística.
    
- Query não carrega Aggregates completos.
    

---

# 11. EPIC-04 — Pipelines

## US-040 — Criar Pipeline

**Como usuário**, quero criar um Pipeline dentro de um Project, para definir meu processo de conteúdo.

### Critérios de aceite

- Pipeline pertence a um Project existente.
    
- Pipeline e Project pertencem à mesma Organization.
    
- Status inicial é Draft.
    
- Versão inicial é definida.
    
- Pipeline pode ser consultado após criação.
    

---

## US-041 — Adicionar Research Step

**Como usuário**, quero adicionar uma etapa de pesquisa, para gerar contexto antes do roteiro.

### Critérios de aceite

- StepType é `research`.
    
- Posição é válida.
    
- Posição não pode repetir.
    
- Pipeline deve estar em Draft.
    
- Configuração é validada.
    

---

## US-042 — Adicionar Script Step

**Como usuário**, quero adicionar uma etapa de roteiro, para transformar pesquisa em conteúdo.

### Critérios de aceite

- StepType é `script`.
    
- Pipeline deve estar em Draft.
    
- Posição não pode repetir.
    
- Script deve vir após Research no MVP.
    

---

## US-043 — Publicar Pipeline

**Como usuário**, quero publicar um Pipeline, para utilizá-lo em Executions.

### Critérios de aceite

- Pipeline possui Research.
    
- Pipeline possui Script.
    
- Research vem antes de Script.
    
- Pipeline passa para Published.
    
- Versão publicada fica registrada.
    
- Pipeline publicado não é alterado diretamente.
    

---

## US-044 — Consultar Pipeline

**Como usuário**, quero visualizar o Pipeline e suas etapas, para entender o processo configurado.

### Critérios de aceite

- Retorna etapas ordenadas.
    
- Retorna status e versão.
    
- Respeita Organization.
    
- Utiliza read model.
    

---

## US-045 — Listar Pipelines de um Project

**Como usuário**, quero listar Pipelines de um Project, para reutilizar processos existentes.

### Critérios de aceite

- Retorna somente Pipelines do Project e Organization.
    
- Possui paginação.
    
- Retorna status e versão atual.
    

---

# 12. EPIC-05 — Pipeline Executions

## US-050 — Iniciar Execution

**Como usuário**, quero iniciar uma Execution de um Pipeline publicado, para gerar conteúdo.

### Critérios de aceite

- Pipeline existe.
    
- Pipeline está Published.
    
- Execution registra PipelineVersion.
    
- StepExecutions são criadas.
    
- Estado inicial é Queued.
    
- Tema é obrigatório.
    
- API retorna `202 Accepted`.
    

---

## US-051 — Consultar Execution

**Como usuário**, quero acompanhar a Execution, para entender o estado da geração.

### Critérios de aceite

- Retorna estado geral.
    
- Retorna etapas.
    
- Retorna etapa atual.
    
- Retorna timestamps.
    
- Retorna falha segura quando existente.
    
- Respeita Organization.
    

---

## US-052 — Cancelar Execution

**Como usuário**, quero cancelar uma Execution, para interromper um processamento que não desejo mais.

### Critérios de aceite

- Queued pode ser cancelada.
    
- Running pode ser cancelada.
    
- Completed não pode ser cancelada.
    
- Failed não pode ser cancelada.
    
- Cancelled é idempotente.
    
- Nenhuma nova Step inicia após cancelamento.
    

---

## US-053 — Registrar falha da Execution

**Como sistema**, quero persistir falhas terminais, para permitir diagnóstico pelo usuário.

### Critérios de aceite

- ErrorCode é persistido.
    
- ErrorDescription segura é persistida.
    
- Step responsável é identificada.
    
- Stack trace não é exposto.
    
- Execution passa para Failed.
    

---

# 13. EPIC-06 — Artifacts

## US-060 — Criar Research Artifact

**Como sistema**, quero persistir o resultado da pesquisa, para utilizá-lo na etapa de roteiro.

### Critérios de aceite

- Artifact pertence à Execution.
    
- Artifact pertence à StepExecution.
    
- Tipo é Research.
    
- Conteúdo estruturado é válido.
    
- Provider e modelo são registrados.
    
- Versão é registrada.
    

---

## US-061 — Criar Script Artifact

**Como sistema**, quero persistir o roteiro gerado, para disponibilizá-lo ao usuário.

### Critérios de aceite

- Tipo é Script.
    
- Artifact referencia a StepExecution.
    
- Roteiro utiliza Research como input.
    
- Conteúdo estruturado é válido.
    
- Provider e modelo são registrados.
    

---

## US-062 — Consultar Artifact

**Como usuário**, quero abrir um Artifact, para utilizar o conteúdo gerado.

### Critérios de aceite

- Retorna conteúdo.
    
- Retorna tipo.
    
- Retorna versão.
    
- Retorna provider e modelo.
    
- Respeita Organization.
    

---

## US-063 — Listar Artifacts da Execution

**Como usuário**, quero listar os resultados de uma Execution, para acompanhar o que foi produzido.

### Critérios de aceite

- Retorna Research e Script.
    
- Ordena pela execução das Steps.
    
- Não retorna conteúdo desnecessário na listagem.
    
- Respeita Organization.
    

---

# 14. EPIC-07 — Fake AI Provider

## US-070 — Criar abstração de geração de texto

**Como Application**, quero utilizar uma abstração de IA, para não depender de um provider específico.

### Critérios de aceite

- Application não conhece SDK externo.
    
- Request e response são internos.
    
- Token usage é suportado.
    
- CancellationToken é propagado.
    

---

## US-071 — Implementar Fake Provider

**Como desenvolvedor**, quero respostas determinísticas, para testar o fluxo sem custos externos.

### Critérios de aceite

- Gera Research válido.
    
- Gera Script válido.
    
- Respostas são determinísticas.
    
- Provider é selecionável por configuração.
    

---

## US-072 — Simular falhas

**Como desenvolvedor**, quero simular falhas de IA, para testar o comportamento do sistema.

### Cenários

-  Timeout.
    
-  Rate limit.
    
-  Provider indisponível.
    
-  Structured Output inválido.
    
-  Falha permanente.
    
-  Cancelamento.
    

---

## US-073 — Validar Structured Output

**Como sistema**, quero validar o JSON gerado, para impedir Artifacts inválidos.

### Critérios de aceite

- Research Schema V1 existe.
    
- Script Schema V1 existe.
    
- JSON inválido é rejeitado.
    
- Schema inválido gera ErrorCode estável.
    
- Limite de tamanho é aplicado.
    

---

# 15. EPIC-08 — Orquestração Funcional

## US-080 — Executar Research Step

**Como sistema**, quero executar a pesquisa, para produzir o primeiro Artifact.

### Critérios de aceite

- Step passa de Pending para Running.
    
- Provider é chamado.
    
- Output é validado.
    
- Artifact é criado.
    
- Step passa para Completed.
    
- Falha é persistida.
    

---

## US-081 — Executar Script Step

**Como sistema**, quero gerar o roteiro usando a pesquisa, para produzir o resultado final.

### Critérios de aceite

- Research Artifact é carregado.
    
- Script Provider é chamado.
    
- Structured Output é validado.
    
- Script Artifact é criado.
    
- Step passa para Completed.
    

---

## US-082 — Concluir Pipeline Execution

**Como sistema**, quero concluir a Execution após todas as Steps, para informar que o conteúdo está pronto.

### Critérios de aceite

- Todas as Steps estão Completed.
    
- Execution passa para Completed.
    
- CompletedAt é registrado.
    
- Evento de conclusão é gerado.
    

---

## US-083 — Executar fluxo funcional end-to-end

**Como equipe**, queremos executar Research e Script sem RabbitMQ, para validar o produto antes da mensageria.

### Critérios de aceite

- Project é criado.
    
- Pipeline é publicado.
    
- Execution é iniciada.
    
- Research é gerada.
    
- Script é gerado.
    
- Dois Artifacts existem.
    
- Execution termina Completed.
    

---

# 16. EPIC-09 — Contracts

## US-090 — Criar envelope de mensagem

### Critérios de aceite

O envelope possui:

- MessageId.
    
- MessageType.
    
- MessageVersion.
    
- OrganizationId.
    
- OccurredAt.
    
- CorrelationId.
    
- CausationId.
    
- Payload.
    
- Metadata.
    

---

## US-091 — Criar Commands distribuídos

Implementar:

-  `PipelineExecutionRequestedV1`.
    
-  `PipelineStepExecutionRequestedV1`.
    

---

## US-092 — Criar Integration Events

Implementar:

-  `PipelineExecutionCompletedV1`.
    
-  `PipelineExecutionFailedV1`.
    
-  `ArtifactGeneratedV1`.
    

---

## US-093 — Testar compatibilidade JSON

### Critérios de aceite

- Round-trip funciona.
    
- Exemplos JSON existem.
    
- MessageTypes são estáveis.
    
- Contracts não possui dependências internas.
    

---

# 17. EPIC-10 — Transactional Outbox

## US-100 — Persistir mensagens na Outbox

**Como sistema**, quero salvar alterações e mensagens no mesmo commit, para não perder eventos.

### Critérios de aceite

- Alteração de negócio e Outbox usam a mesma transação.
    
- Falha no commit não salva nenhum registro.
    
- MessageId é estável.
    
- Payload possui versão.
    
- OrganizationId é preservado.
    

---

## US-101 — Capturar eventos publicáveis

### Critérios de aceite

- Domain Event não é publicado diretamente.
    
- Existe mapeamento explícito para Integration Event.
    
- Eventos não publicáveis são ignorados.
    
- SDKs externos não aparecem no Data.
    

---

## US-102 — Consultar lote pendente

### Critérios de aceite

- Registros processados não retornam.
    
- `NextAttemptAt` é respeitado.
    
- Lote possui tamanho configurável.
    
- Claim suporta múltiplas instâncias.
    

---

# 18. EPIC-11 — RabbitMQ

## US-110 — Configurar conexão

### Critérios de aceite

- Conexão é reutilizada.
    
- TLS é configurável.
    
- Credenciais vêm de configuração segura.
    
- Health check existe.
    

---

## US-111 — Declarar topology

Implementar:

```text
infinite-content.commands
infinite-content.events
infinite-content.dead-letter
```

Filas:

```text
infinite-content.pipeline.execution
infinite-content.pipeline.steps
```

---

## US-112 — Publicar mensagem

### Critérios de aceite

- Routing key correta.
    
- Headers completos.
    
- Mensagem persistente.
    
- Publisher Confirm utilizado.
    
- Timeout é tratado.
    

---

# 19. EPIC-12 — Worker e Consumers

## US-120 — Criar host Worker

### Critérios de aceite

- Worker compila.
    
- Configuração é validada.
    
- Health checks existem.
    
- Shutdown gracioso está configurado.
    
- Program.cs permanece pequeno.
    

---

## US-121 — Publicar Outbox

### Critérios de aceite

- Worker busca lote.
    
- Realiza claim.
    
- Publica.
    
- Aguarda confirmação.
    
- Marca como processado.
    
- Falha incrementa tentativa.
    

---

## US-122 — Consumir solicitação de Execution

### Critérios de aceite

- Mensagem é validada.
    
- Organization é propagada.
    
- Command interno é despachado.
    
- Ack ocorre após sucesso.
    
- Falhas são classificadas.
    

---

## US-123 — Consumir solicitação de Step

### Critérios de aceite

- StepType é resolvido.
    
- AttemptId é validado.
    
- Research e Script podem ser executados.
    
- Falhas permanentes seguem para Dead Letter.
    

---

# 20. EPIC-13 — Inbox e Deduplicação

## US-130 — Registrar mensagem recebida

### Critérios de aceite

- Inbox possui ConsumerName.
    
- Inbox possui MessageId.
    
- Existe unique constraint.
    
- ReceivedAt é registrado.
    

---

## US-131 — Impedir efeito duplicado

### Critérios de aceite

- Redelivery não cria nova Execution.
    
- Redelivery não cria novo Artifact.
    
- Mensagem já processada recebe Ack.
    
- Consumers diferentes podem processar o mesmo evento.
    

---

## US-132 — Tratar falha após commit

### Cenário de aceite

```text
Commit concluído
    ↓
Worker falha antes do Ack
    ↓
Mensagem é entregue novamente
    ↓
Inbox detecta duplicidade
    ↓
Efeito não se repete
```

---

# 21. EPIC-14 — Retry e Recovery

## US-140 — Classificar falhas

### Categorias

```text
Transient
Permanent
Conflict
Duplicate
OutcomeUnknown
Cancelled
Unexpected
```

### Critérios de aceite

- Cada falha gera disposição conhecida.
    
- Erros permanentes não são repetidos.
    
- Erros transitórios possuem atraso.
    

---

## US-141 — Implementar retry básico

### Agenda inicial

```text
30 segundos
2 minutos
10 minutos
```

### Critérios de aceite

- Retry não é imediato.
    
- Número máximo é respeitado.
    
- Retry-After é utilizado quando disponível.
    
- Jitter é aplicado.
    

---

## US-142 — Encaminhar para Dead Letter

### Critérios de aceite

- Mensagem inválida vai para Dead Letter.
    
- Versão desconhecida vai para Dead Letter.
    
- Tentativas esgotadas vão para Dead Letter.
    
- Metadados originais são preservados.
    
- ErrorCode é registrado.
    

---

## US-143 — Recuperar Step travada

### Critérios de aceite

- Step Running possui lease.
    
- Lease expirado é detectado.
    
- Nova tentativa recebe novo AttemptId.
    
- AttemptNumber é incrementado.
    
- Resultado atrasado é rejeitado.
    

---

# 22. EPIC-15 — Provider Real

## US-150 — Configurar provider OpenAI

### Critérios de aceite

- Configuração tipada.
    
- Secret fora do código.
    
- Provider selecionável por configuração.
    
- Fake continua disponível.
    

---

## US-151 — Gerar Research real

### Critérios de aceite

- Prompt versionado.
    
- Structured Output validado.
    
- Provider e modelo registrados.
    
- Token usage registrado.
    
- Erros classificados.
    

---

## US-152 — Gerar Script real

### Critérios de aceite

- Research é utilizada como contexto.
    
- Output segue Script Schema V1.
    
- Timeout é aplicado.
    
- Conteúdo não é registrado em logs completos.
    

---

## US-153 — Medir custo por Execution

### Critérios de aceite

- Input tokens registrados.
    
- Output tokens registrados.
    
- Modelo real registrado.
    
- Estimativa de custo pode ser calculada posteriormente.
    

---

# 23. EPIC-16 — Segurança Mínima

## US-160 — Proteger recursos por Organization

### Critérios de aceite

- Toda query tenant-scoped filtra OrganizationId.
    
- IDs de outra Organization retornam `404`.
    
- Repositories exigem OrganizationId.
    
- Testes cross-tenant existem.
    

---

## US-161 — Configurar autenticação

### Critérios de aceite

- API exige identidade em ambiente externo.
    
- Organization é obtida de claim confiável.
    
- Autenticação fake só funciona em Development.
    

---

## US-162 — Proteger secrets

### Critérios de aceite

- API keys não estão no repositório.
    
- Connection strings não aparecem em logs.
    
- Credenciais são carregadas por configuração segura.
    
- Logs possuem redaction.
    

---

# 24. EPIC-17 — Observabilidade

## US-170 — Configurar logs estruturados

### Campos mínimos

```text
CorrelationId
TraceId
OrganizationId
ExecutionId
StepExecutionId
MessageId
AttemptNumber
ErrorCode
Duration
```

---

## US-171 — Configurar tracing

### Critérios de aceite

- Trace HTTP é criado.
    
- Contexto atravessa Outbox.
    
- Contexto chega ao Worker.
    
- Chamada ao provider aparece como dependência.
    

---

## US-172 — Configurar métricas do MVP

Implementar:

```text
pipeline.executions.started
pipeline.executions.completed
pipeline.executions.failed
pipeline.step.duration
pipeline.step.retries
outbox.pending.count
worker.messages.processed
worker.messages.failed
```

---

## US-173 — Criar health checks

Endpoints:

```text
/health/live
/health/ready
```

Readiness verifica dependências obrigatórias.

Liveness não executa chamadas externas pesadas.

---

# 25. EPIC-18 — Testes End-to-End

## US-180 — Testar fluxo funcional

### Cenário

- Criar Project.
    
- Criar Pipeline.
    
- Adicionar Steps.
    
- Publicar.
    
- Iniciar Execution.
    
- Executar Fake Provider.
    
- Consultar Artifacts.
    

### Resultado

```text
Execution = Completed
Artifacts = 2
```

---

## US-181 — Testar fluxo assíncrono

### Cenário

- Start Execution.
    
- Outbox.
    
- RabbitMQ.
    
- Worker.
    
- Research.
    
- Script.
    
- Completed.
    

---

## US-182 — Testar mensagem duplicada

### Critérios de aceite

- Mesmo MessageId é entregue duas vezes.
    
- Efeito ocorre uma vez.
    
- Inbox registra processamento.
    

---

## US-183 — Testar interrupção do Worker

### Critérios de aceite

- Worker é interrompido durante uma Step.
    
- Lease expira.
    
- Recovery reagenda.
    
- Execution conclui posteriormente.
    

---

# 26. EPIC-19 — Ambiente Local e CI

## US-190 — Criar Docker Compose

Serviços:

```text
postgres
rabbitmq
```

### Critérios de aceite

- Ambiente inicia com um comando.
    
- Volumes são configurados.
    
- Health checks existem.
    
- Credenciais locais não são usadas em produção.
    

---

## US-191 — Criar migrations automatizadas para desenvolvimento

### Critérios de aceite

- Banco vazio recebe todas as migrations.
    
- Aplicação consegue iniciar.
    
- Migrations não são executadas automaticamente por múltiplas instâncias em produção.
    

---

## US-192 — Criar pipeline de CI

Etapas:

```text
Restore
Build
Unit Tests
Architecture Tests
Integration Tests
```

---

## US-193 — Criar containers da API e Worker

### Critérios de aceite

- Multi-stage build.
    
- Execução sem root quando possível.
    
- Sem secrets na imagem.
    
- Health checks configurados.
    

---

# 27. EPIC-20 — Demonstração do MVP

## US-200 — Criar dados de demonstração

Criar:

```text
Project: Canal de Tecnologia
Pipeline: Pesquisa e Roteiro
Steps:
1. Research
2. Script
```

---

## US-201 — Preparar tema de demonstração

Tema inicial:

```text
Como agentes de inteligência artificial estão mudando o desenvolvimento de software
```

---

## US-202 — Preparar roteiro da demonstração

A demonstração deverá mostrar:

1. Criação do Project.
    
2. Criação do Pipeline.
    
3. Publicação.
    
4. Início da Execution.
    
5. Acompanhamento.
    
6. Research Artifact.
    
7. Script Artifact.
    
8. Logs ou trace básico.
    

---

## US-203 — Coletar feedback

Perguntas:

- A pesquisa foi útil?
    
- O roteiro reduziu trabalho?
    
- O fluxo ficou claro?
    
- Quanto precisaria ser reescrito?
    
- O usuário repetiria o processo?
    
- Qual foi a parte menos útil?
    

---

# 28. Bugs e Dívida Técnica

Bugs deverão possuir:

- Passos para reprodução.
    
- Resultado atual.
    
- Resultado esperado.
    
- Ambiente.
    
- Logs relevantes.
    
- Prioridade.
    
- Teste de regressão.
    

Dívida técnica deverá ser adicionada somente quando:

- Possuir impacto conhecido.
    
- Tiver risco.
    
- Gerar custo de manutenção.
    
- Possuir uma ação clara.
    

Evitar tarefas vagas como:

```text
Melhorar arquitetura
Refatorar tudo
Otimizar aplicação
```

---

# 29. Spikes Permitidos

Spikes deverão possuir duração e pergunta claras.

Exemplos:

```text
SPIKE-001 — Validar suporte do provider a Structured Output
SPIKE-002 — Testar claim da Outbox com SKIP LOCKED
SPIKE-003 — Avaliar estratégia de retry no RabbitMQ
SPIKE-004 — Testar UUID v7 com EF Core e PostgreSQL
```

O resultado de um Spike deverá ser:

- Decisão.
    
- Protótipo descartável.
    
- ADR.
    
- Nova história.
    
- Rejeição de uma abordagem.
    

---

# 30. Backlog Fora do MVP

## Agents futuros

```text
SEO Agent
Trend Agent
Thumbnail Agent
Voice Agent
Video Agent
Translation Agent
Publishing Agent
Analytics Agent
```

## Automação

```text
n8n
Webhooks
Agendamentos avançados
Workflow visual
```

## Conteúdo

```text
Imagem
Áudio
Vídeo
Thumbnail
Tradução
SEO avançado
```

## Produto

```text
Billing
Assinaturas
Planos
Quotas comerciais
Marketplace
Colaboração em tempo real
Editor completo
```

## Infraestrutura

```text
Redis
Multi-região
Particionamento
Autoscaling avançado
Storage de mídia
GPU Workers
```

Esses itens deverão permanecer em prioridade P3 até a conclusão do MVP.

---

# 31. Ordem Recomendada de Implementação

```text
1. EPIC-00 — Fundação
2. EPIC-01 — Shared Kernel
3. EPIC-02 — Organization mínima
4. EPIC-03 — Projects
5. EPIC-04 — Pipelines
6. EPIC-05 — Executions
7. EPIC-06 — Artifacts
8. EPIC-07 — Fake AI
9. EPIC-08 — Orquestração funcional
10. EPIC-18 — E2E funcional
11. EPIC-09 — Contracts
12. EPIC-10 — Outbox
13. EPIC-11 — RabbitMQ
14. EPIC-12 — Worker
15. EPIC-13 — Inbox
16. EPIC-14 — Retry e Recovery
17. EPIC-18 — E2E assíncrono
18. EPIC-15 — Provider real
19. EPIC-20 — Demonstração
```

Segurança, observabilidade e ambiente local deverão acompanhar todas as etapas.

---

# 32. Primeiro Vertical Slice

O primeiro vertical slice deverá ser:

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

Esse slice deverá validar:

- API.
    
- Result Pattern.
    
- Application.
    
- Domain.
    
- EF Core.
    
- PostgreSQL.
    
- Multi-tenancy.
    
- Testes de integração.
    
- Problem Details.
    

---

# 33. Critério de Conclusão do Marco 1

O Marco 1 estará concluído quando:

-  Project funciona.
    
-  Pipeline funciona.
    
-  Research e Script estão configuradas.
    
-  Execution funciona.
    
-  Fake Provider funciona.
    
-  Research Artifact é salvo.
    
-  Script Artifact é salvo.
    
-  Execution termina Completed.
    
-  Falhas são persistidas.
    
-  Teste end-to-end funcional passa.
    

---

# 34. Critério de Conclusão do Marco 2

O Marco 2 estará concluído quando:

-  Outbox registra mensagens.
    
-  RabbitMQ recebe mensagens.
    
-  Worker processa mensagens.
    
-  Inbox impede duplicidade.
    
-  Retry possui atraso.
    
-  Dead Letter recebe falhas permanentes.
    
-  Recovery retoma Steps travadas.
    
-  Shutdown é gracioso.
    
-  Teste end-to-end assíncrono passa.
    

---

# 35. Critério de Conclusão do Marco 3

O Marco 3 estará concluído quando:

-  Provider real gera Research.
    
-  Provider real gera Script.
    
-  Token usage é registrado.
    
-  Custos podem ser estimados.
    
-  Output é revisado com temas reais.
    
-  Demonstração está preparada.
    
-  Feedback inicial foi coletado.
    

---

# 36. Regras de Manutenção do Backlog

1. Toda história precisa de objetivo claro.
    
2. Toda história precisa de critério de aceite.
    
3. Itens grandes deverão ser divididos.
    
4. Itens sem valor ou necessidade deverão ser removidos.
    
5. Bugs graves podem ultrapassar a prioridade normal.
    
6. Segurança e isolamento são sempre P0.
    
7. O backlog deverá ser revisado ao final de cada Sprint.
    
8. O backlog não deverá ser usado como depósito de ideias sem contexto.
    
9. Funcionalidades futuras deverão permanecer separadas do MVP.
    
10. A implementação real poderá revelar mudanças necessárias.
    

---

# 37. Próximos Itens Ready

Os primeiros itens que deverão ficar com status `Ready` são:

```text
US-001 — Criar a solution
US-002 — Configurar padrões globais
US-003 — Criar testes de arquitetura
US-010 — Implementar Result Pattern
US-011 — Implementar primitivas de Domain
US-012 — Implementar abstração de tempo
US-020 — Criar contexto de Organization
US-021 — Criar identidade de desenvolvimento
```

Depois:

```text
US-030 — Criar Project
US-031 — Consultar Project
US-032 — Listar Projects
```

---

# 38. Filosofia Final

O backlog deverá servir para construir o produto, não apenas para listar possibilidades.

A prioridade absoluta será atravessar o fluxo completo:

```text
Tema
    ↓
Research
    ↓
Script
    ↓
Artifact persistido
```

Toda tarefa deverá responder:

> Isso aproxima o MVP de uma geração completa, utilizável e demonstrável?

Quando a resposta for não, o item deverá ser adiado, simplificado ou removido.