# MVP

## 1. Objetivo

O MVP do Infinite Content AI deverá validar se criadores de conteúdo e pequenas equipes percebem valor em transformar um tema em uma pesquisa organizada e em um roteiro utilizável por meio de um processo automatizado e rastreável.

O produto inicial deverá provar que um usuário consegue:

1. Criar um projeto.
    
2. Configurar um processo de conteúdo.
    
3. Informar um tema.
    
4. Iniciar uma geração.
    
5. Acompanhar o processamento.
    
6. Receber uma pesquisa estruturada.
    
7. Receber um roteiro estruturado.
    
8. Consultar os resultados posteriormente.
    

O MVP não pretende automatizar toda a produção de conteúdo.

Ele deverá resolver muito bem o fluxo:

```text
Tema
    ↓
Pesquisa
    ↓
Roteiro
```

---

# 2. Problema que o MVP Resolve

Criadores e pequenas equipes frequentemente utilizam várias ferramentas para produzir um único conteúdo.

O processo normalmente envolve:

- Pesquisar em diferentes fontes.
    
- Organizar informações manualmente.
    
- Conversar com ferramentas de IA.
    
- Copiar respostas entre chats e documentos.
    
- Reescrever prompts.
    
- Criar o roteiro.
    
- Revisar o material.
    
- Perder o histórico do processo.
    

Isso gera:

- Trabalho repetitivo.
    
- Falta de consistência.
    
- Dificuldade de reprodução.
    
- Resultados espalhados.
    
- Falta de rastreabilidade.
    
- Dependência de prompts improvisados.
    
- Tempo elevado entre ideia e roteiro.
    

O MVP deverá reduzir esse esforço.

---

# 3. Proposta do MVP

O Infinite Content AI permitirá que o usuário configure um Pipeline composto por duas etapas:

```text
Research Step
    ↓
Script Step
```

A Research Step transforma o tema em uma pesquisa estruturada.

A Script Step utiliza essa pesquisa para gerar um roteiro.

O sistema deverá preservar:

- Project.
    
- Pipeline.
    
- Pipeline Version.
    
- Execution.
    
- Step Executions.
    
- Artifacts.
    
- Provider.
    
- Model.
    
- Estado do processamento.
    
- Erros encontrados.
    

---

# 4. Público-Alvo Inicial

O MVP será direcionado principalmente para:

- Criadores individuais.
    
- Pequenas equipes de conteúdo.
    
- Freelancers.
    
- Roteiristas.
    
- Social media managers.
    
- Agências pequenas.
    
- Profissionais que produzem conteúdo para clientes.
    
- Especialistas que desejam transformar conhecimento em vídeos.
    

O caso de uso inicial será otimizado para vídeos educativos ou informativos.

Duração sugerida:

```text
5 a 15 minutos
```

---

# 5. Persona Principal

## Criador Especialista

Profissional que domina um tema, mas possui dificuldade em transformar ideias em conteúdos recorrentes.

Características:

- Produz conteúdo semanalmente.
    
- Trabalha sozinho ou com uma equipe pequena.
    
- Utiliza ferramentas de IA.
    
- Possui várias ideias acumuladas.
    
- Perde tempo pesquisando.
    
- Reescreve prompts frequentemente.
    
- Deseja acelerar a produção.
    
- Ainda quer revisar e controlar o resultado final.
    

---

# 6. Job to Be Done

> Quando eu tiver um tema para produzir conteúdo, quero receber uma pesquisa organizada e um roteiro estruturado, para que eu possa avançar para gravação ou edição sem começar do zero.

---

# 7. Fluxo Principal do Usuário

```text
Criar Project
    ↓
Criar Pipeline
    ↓
Adicionar Research Step
    ↓
Adicionar Script Step
    ↓
Publicar Pipeline
    ↓
Informar tema
    ↓
Iniciar Execution
    ↓
Acompanhar processamento
    ↓
Consultar Research Artifact
    ↓
Consultar Script Artifact
```

Esse será o fluxo principal de demonstração e validação.

---

# 8. Conceitos Visíveis para o Usuário

## Project

Representa um contexto de produção.

Exemplos:

```text
Canal de Tecnologia
Conteúdo do Cliente A
Newsletter de Inteligência Artificial
```

## Pipeline

Representa o processo utilizado para produzir conteúdo.

Exemplo:

```text
Pesquisa e Roteiro
```

## Execution

Representa uma execução específica do Pipeline.

Exemplo:

```text
Tema: Como a IA está mudando o desenvolvimento de software
```

## Artifact

Representa um resultado produzido.

Tipos iniciais:

```text
Research
Script
```

---

# 9. Funcionalidades Essenciais

## Projects

O usuário deverá poder:

- Criar um Project.
    
- Consultar um Project.
    
- Listar seus Projects.
    

Campos iniciais:

```text
Nome
Descrição
Status
Data de criação
```

---

## Pipelines

O usuário deverá poder:

- Criar um Pipeline.
    
- Vincular o Pipeline a um Project.
    
- Adicionar etapas.
    
- Definir a ordem das etapas.
    
- Publicar o Pipeline.
    
- Consultar sua configuração.
    

Tipos de etapa no MVP:

```text
Research
Script
```

---

## Executions

O usuário deverá poder:

- Iniciar uma Execution.
    
- Receber seu identificador.
    
- Consultar o estado.
    
- Visualizar a etapa atual.
    
- Visualizar falhas.
    
- Cancelar quando permitido.
    

Estados iniciais:

```text
Queued
Running
Completed
Failed
Cancelled
```

---

## Artifacts

O usuário deverá poder:

- Consultar o Research Artifact.
    
- Consultar o Script Artifact.
    
- Listar Artifacts da Execution.
    
- Identificar qual etapa gerou cada Artifact.
    
- Identificar quando o Artifact foi criado.
    

---

# 10. Entrada da Execution

A entrada mínima deverá conter:

```text
Tema
```

Exemplo:

```text
Como agentes de inteligência artificial estão mudando o desenvolvimento de software
```

Campos opcionais poderão incluir:

- Público desejado.
    
- Tom.
    
- Objetivo do conteúdo.
    
- Duração esperada.
    
- Pontos obrigatórios.
    
- Pontos que devem ser evitados.
    

Para manter o MVP simples, somente o tema será obrigatório.

---

# 11. Research Artifact

O Research Artifact deverá conter uma estrutura previsível.

Exemplo:

```json
{
  "summary": "Resumo geral do tema.",
  "keyPoints": [
    "Ponto principal 1",
    "Ponto principal 2"
  ],
  "sources": [
    {
      "title": "Título da fonte",
      "url": "https://exemplo.com"
    }
  ]
}
```

Campos essenciais:

- Resumo.
    
- Pontos principais.
    
- Fontes, quando disponíveis.
    
- Questões que precisam de verificação.
    
- Contexto para o roteiro.
    

---

# 12. Script Artifact

O Script Artifact deverá conter:

```json
{
  "title": "Título sugerido",
  "hook": "Abertura do vídeo",
  "sections": [
    {
      "title": "Seção 1",
      "content": "Conteúdo da seção"
    }
  ],
  "conclusion": "Conclusão"
}
```

Campos essenciais:

- Título.
    
- Hook.
    
- Introdução.
    
- Seções.
    
- Transições quando necessárias.
    
- Conclusão.
    
- Chamada para ação opcional.
    

O roteiro deverá utilizar o Research Artifact como uma de suas entradas.

---

# 13. Controle Humano

O MVP não publicará conteúdo automaticamente.

O usuário deverá revisar os Artifacts antes de utilizá-los.

A plataforma deverá tratar os outputs como:

```text
Materiais de apoio para produção
```

e não como conteúdo necessariamente pronto para publicação.

A revisão humana será especialmente importante para:

- Confirmar informações.
    
- Avaliar fontes.
    
- Ajustar tom.
    
- Corrigir contexto.
    
- Remover alucinações.
    
- Adaptar o roteiro à identidade do criador.
    

---

# 14. Provider de Inteligência Artificial

O MVP possuirá dois modos.

## Fake Provider

Utilizado para:

- Desenvolvimento.
    
- Testes.
    
- Demonstrações técnicas.
    
- Execuções determinísticas.
    
- Simulação de falhas.
    

## Provider Real

Utilizado para validar a qualidade real do produto.

Provider inicial sugerido:

```text
OpenAI
```

O usuário não precisará escolher entre vários providers no MVP.

A seleção poderá ocorrer por configuração.

---

# 15. Processamento Assíncrono

O início de uma Execution não deverá aguardar toda a geração.

Fluxo:

```text
Usuário inicia Execution
    ↓
Sistema retorna ExecutionId
    ↓
Processamento continua
    ↓
Usuário consulta o estado
```

Resposta inicial esperada:

```text
202 Accepted
```

Estado inicial:

```text
Queued
```

---

# 16. Feedback de Estado

O sistema deverá informar estados claros.

Exemplo:

```text
Queued
Running Research
Running Script
Completed
Failed
Cancelled
```

O usuário não deverá receber apenas:

```text
Processando
```

sem entender qual etapa está ativa.

---

# 17. Falhas

Quando uma execução falhar, o usuário deverá conseguir identificar:

- Qual etapa falhou.
    
- Quando ocorreu.
    
- Um código de erro.
    
- Uma descrição segura.
    
- Se a operação poderá ser repetida.
    

Exemplo:

```text
Step: Research
Code: AI.ProviderTimeout
Description: O provider excedeu o tempo permitido.
```

Não deverão ser expostos:

- Stack traces.
    
- API keys.
    
- Respostas internas completas.
    
- SQL.
    
- Dados técnicos sensíveis.
    

---

# 18. Cancelamento

O usuário poderá cancelar uma Execution que ainda não esteja em estado terminal.

Estados terminais:

```text
Completed
Failed
Cancelled
```

Depois do cancelamento:

- Nenhuma nova etapa deverá iniciar.
    
- Resultados atrasados não deverão alterar a execução.
    
- Artifacts já concluídos poderão permanecer disponíveis.
    

---

# 19. Autenticação e Organização

Cada usuário deverá operar dentro de uma Organization.

Todos os recursos deverão pertencer a uma Organization:

- Projects.
    
- Pipelines.
    
- Executions.
    
- Artifacts.
    

Um usuário não poderá acessar recursos pertencentes a outra Organization.

Para desenvolvimento local, uma autenticação fake poderá ser utilizada.

---

# 20. Experiência Mínima

Mesmo que o primeiro cliente seja apenas uma API ou uma interface simples, o fluxo deverá ser compreensível.

O usuário deverá conseguir responder:

- Em qual Project estou?
    
- Qual Pipeline será utilizado?
    
- Qual tema está sendo processado?
    
- Qual é o estado atual?
    
- Qual etapa está executando?
    
- Quais resultados já foram gerados?
    
- O que falhou?
    

---

# 21. Endpoints do MVP

## Projects

```text
POST /api/v1/projects
GET  /api/v1/projects
GET  /api/v1/projects/{projectId}
```

## Pipelines

```text
POST /api/v1/projects/{projectId}/pipelines
GET  /api/v1/projects/{projectId}/pipelines
GET  /api/v1/pipelines/{pipelineId}
POST /api/v1/pipelines/{pipelineId}/steps
POST /api/v1/pipelines/{pipelineId}/publish
```

## Executions

```text
POST /api/v1/pipelines/{pipelineId}/executions
GET  /api/v1/executions/{executionId}
POST /api/v1/executions/{executionId}/cancel
```

## Artifacts

```text
GET /api/v1/executions/{executionId}/artifacts
GET /api/v1/artifacts/{artifactId}
```

---

# 22. Interface Gráfica

Uma interface gráfica completa não será obrigatória para validar o backend do MVP.

Possíveis formas iniciais de uso:

- Swagger.
    
- Bruno.
    
- Postman.
    
- Interface web mínima.
    
- Aplicação administrativa simples.
    

Para validação com usuários não técnicos, será necessária posteriormente uma interface simples com:

- Lista de Projects.
    
- Configuração de Pipeline.
    
- Formulário para iniciar Execution.
    
- Página de acompanhamento.
    
- Visualização dos Artifacts.
    

---

# 23. Persistência

O MVP deverá persistir:

```text
Projects
Pipelines
Pipeline Steps
Pipeline Executions
Step Executions
Artifacts
Outbox Messages
Inbox Messages
```

Banco principal:

```text
PostgreSQL
```

ORM:

```text
Entity Framework Core
```

---

# 24. Mensageria

O fluxo assíncrono utilizará:

```text
RabbitMQ
```

A confiabilidade será apoiada por:

```text
Transactional Outbox
Inbox
Idempotência
At-least-once delivery
```

O MVP não buscará processamento exactly-once.

---

# 25. Fluxo Técnico Resumido

```text
API recebe Start Execution
    ↓
Execution é criada no PostgreSQL
    ↓
Mensagem é salva na Outbox
    ↓
Worker publica no RabbitMQ
    ↓
Consumer inicia Execution
    ↓
Research Step é executada
    ↓
Research Artifact é salvo
    ↓
Script Step é executada
    ↓
Script Artifact é salvo
    ↓
Execution é concluída
```

---

# 26. Fora do Escopo

Não fará parte do MVP:

- Geração de imagens.
    
- Geração de thumbnails.
    
- Geração de vídeo.
    
- Geração de voz.
    
- Tradução automática.
    
- SEO Agent.
    
- Trend Agent.
    
- Analytics Agent.
    
- Publishing Agent.
    
- Publicação em redes sociais.
    
- Aprovações avançadas.
    
- Workflows do n8n.
    
- Redis.
    
- RAG completo.
    
- Busca vetorial.
    
- Billing.
    
- Assinaturas.
    
- Controle de planos.
    
- Aplicativo mobile.
    
- Marketplace.
    
- Colaboração em tempo real.
    
- Editor de roteiro completo.
    
- Workflow visual.
    
- Multi-provider configurável pelo usuário.
    
- Processamento em múltiplas regiões.
    

---

# 27. Requisitos Não Funcionais Mínimos

## Segurança

- Isolamento por Organization.
    
- Secrets fora do código.
    
- Logs sem dados sensíveis.
    
- Autorização básica.
    
- Proteção contra acesso por ID de outro tenant.
    

## Confiabilidade

- Outbox.
    
- Inbox.
    
- Retry básico.
    
- Dead Letter.
    
- Recuperação de execução travada.
    

## Observabilidade

- Logs estruturados.
    
- CorrelationId.
    
- TraceId.
    
- ExecutionId.
    
- StepExecutionId.
    
- ErrorCode.
    

## Testabilidade

- Fake Provider.
    
- Testes de Domain.
    
- Testes de Application.
    
- Testes com PostgreSQL real.
    
- Teste end-to-end.
    

---

# 28. Métricas de Produto

## Ativação

Percentual de usuários que:

1. Criam um Project.
    
2. Criam ou utilizam um Pipeline.
    
3. Iniciam uma Execution.
    
4. Recebem um Script Artifact.
    

## Tempo para Valor

Tempo entre:

```text
Criação do Project
e
Primeiro Script Artifact concluído
```

## Taxa de Conclusão

```text
Executions concluídas
÷
Executions iniciadas
```

## Qualidade Percebida

Avaliação do usuário sobre:

- Utilidade da pesquisa.
    
- Qualidade do roteiro.
    
- Quantidade de reescrita.
    
- Relevância.
    
- Organização.
    
- Intenção de reutilizar.
    

## Retenção Inicial

Quantidade de usuários que iniciam uma segunda Execution.

---

# 29. Métricas Técnicas

- Duração média da Execution.
    
- Duração média da Research Step.
    
- Duração média da Script Step.
    
- Taxa de falha por etapa.
    
- Quantidade de retries.
    
- Idade da mensagem mais antiga.
    
- Quantidade de mensagens na Outbox.
    
- Quantidade de mensagens em Dead Letter.
    
- Tokens de entrada.
    
- Tokens de saída.
    
- Custo estimado por Execution.
    

---

# 30. Hipóteses que o MVP Deverá Validar

## Hipótese 1

O usuário percebe valor em receber pesquisa e roteiro dentro de um mesmo fluxo.

## Hipótese 2

A pesquisa estruturada melhora a qualidade do roteiro.

## Hipótese 3

Projects, Pipelines, Executions e Artifacts tornam o processo mais organizado que um chat isolado.

## Hipótese 4

O usuário aceita revisar os resultados antes da publicação.

## Hipótese 5

O tempo economizado compensa o custo do provider de IA.

## Hipótese 6

O usuário deseja repetir o processo para novos temas.

---

# 31. Critérios de Sucesso

O MVP poderá ser considerado promissor quando usuários de teste conseguirem:

- Completar o fluxo sem ajuda técnica constante.
    
- Entender o estado da Execution.
    
- Utilizar o Research Artifact.
    
- Utilizar o Script Artifact.
    
- Produzir um segundo conteúdo.
    
- Relatar economia de tempo.
    
- Relatar menor trabalho de organização.
    
- Perceber diferença em relação a usar somente um chat de IA.
    

Não será necessário atingir escala elevada nessa fase.

---

# 32. Critérios de Falha

O MVP deverá ser reconsiderado quando:

- Os usuários preferirem continuar usando chats isolados.
    
- A pesquisa não melhorar o roteiro.
    
- A estrutura adicionar mais trabalho do que reduzir.
    
- O usuário precisar reescrever quase todo o output.
    
- O processo for difícil de compreender.
    
- O custo por conteúdo for alto demais.
    
- Os usuários não retornarem para uma segunda Execution.
    
- O histórico não gerar valor percebido.
    

Uma hipótese rejeitada não significa necessariamente abandonar o produto.

Pode indicar necessidade de alterar:

- Público.
    
- Fluxo.
    
- Prompts.
    
- Inputs.
    
- Outputs.
    
- Experiência.
    

---

# 33. Cenário de Demonstração

## Project

```text
Canal de Tecnologia
```

## Pipeline

```text
Pesquisa e Roteiro
```

## Steps

```text
1. Research
2. Script
```

## Tema

```text
Como agentes de inteligência artificial estão mudando o desenvolvimento de software
```

## Resultado

Research Artifact com:

- Resumo.
    
- Principais mudanças.
    
- Benefícios.
    
- Riscos.
    
- Exemplos.
    
- Fontes.
    

Script Artifact com:

- Título.
    
- Hook.
    
- Introdução.
    
- Seções.
    
- Conclusão.
    
- Chamada para ação.
    

---

# 34. Entregas do MVP

## Entrega 1 — Fundação

- Solution.
    
- Shared Kernel.
    
- Domain inicial.
    
- PostgreSQL.
    
- API básica.
    

## Entrega 2 — Conteúdo

- Project.
    
- Pipeline.
    
- Execution.
    
- Artifact.
    
- Fake Provider.
    
- Research.
    
- Script.
    

## Entrega 3 — Processamento Assíncrono

- Contracts.
    
- Outbox.
    
- RabbitMQ.
    
- Worker.
    
- Inbox.
    
- Retry.
    
- Recovery.
    

## Entrega 4 — Validação Real

- Provider real.
    
- Testes end-to-end.
    
- Ambiente de demonstração.
    
- Execuções com temas reais.
    
- Coleta de feedback.
    

---

# 35. Definition of Done Funcional

O MVP funcional estará pronto quando:

-  Um Project puder ser criado.
    
-  Um Pipeline puder ser criado.
    
-  Research e Script puderem ser configurados.
    
-  O Pipeline puder ser publicado.
    
-  Uma Execution puder ser iniciada.
    
-  O estado puder ser consultado.
    
-  Um Research Artifact puder ser gerado.
    
-  Um Script Artifact puder ser gerado.
    
-  Os Artifacts puderem ser consultados.
    
-  Falhas puderem ser visualizadas.
    
-  O fluxo funcionar com Fake Provider.
    

---

# 36. Definition of Done Assíncrono

O processamento assíncrono estará pronto quando:

-  Start Execution retornar imediatamente.
    
-  A Execution iniciar como Queued.
    
-  Uma mensagem for salva na Outbox.
    
-  O Worker publicar a mensagem.
    
-  RabbitMQ entregar a mensagem.
    
-  Consumers processarem Research e Script.
    
-  Inbox impedir efeitos duplicados.
    
-  Retry tratar falhas transitórias.
    
-  Dead Letter receber falhas permanentes.
    
-  Recovery retomar uma Step interrompida.
    
-  Shutdown não perder trabalho confirmado.
    

---

# 37. Definition of Done do Produto

O MVP estará concluído quando uma pessoa conseguir:

1. Criar um Project.
    
2. Criar e publicar um Pipeline.
    
3. Informar um tema.
    
4. Iniciar uma Execution.
    
5. Receber um identificador.
    
6. Acompanhar as etapas.
    
7. Consultar uma pesquisa.
    
8. Consultar um roteiro.
    
9. Identificar falhas.
    
10. Repetir o processo com outro tema.
    

Além disso:

- O sistema deverá funcionar com um provider real.
    
- O fluxo deverá possuir testes automatizados.
    
- O ambiente local deverá ser reproduzível.
    
- Dados deverão permanecer isolados por Organization.
    
- Mensagens duplicadas não deverão duplicar Artifacts.
    

---

# 38. Próxima Fase Após o MVP

Depois da validação, as prioridades poderão incluir:

## Experiência

- Interface web.
    
- Templates de Pipeline.
    
- Edição de Artifacts.
    
- Reexecução de uma etapa.
    
- Comparação de versões.
    
- Exportação.
    

## Qualidade

- Contexto avançado do Project.
    
- Prompts refinados.
    
- Avaliação automática.
    
- Mais controle de tom.
    
- Mais formatos.
    

## Automação

- Approval.
    
- Publication.
    
- Webhooks.
    
- n8n.
    
- Agendamentos.
    

## Escala

- Redis.
    
- Limites por plano.
    
- Billing.
    
- Mais Workers.
    
- Separação de workloads.
    
- Providers adicionais.
    

---

# 39. Regra de Priorização

Uma funcionalidade somente deverá entrar antes da conclusão do MVP quando:

- For necessária para completar o fluxo principal.
    
- Corrigir um risco grave.
    
- Impedir a validação com usuários.
    
- Proteger segurança ou isolamento.
    
- Melhorar diretamente a qualidade de Research ou Script.
    
- Resolver um problema já observado.
    

Pedidos que não atendam a esses critérios deverão ir para o Backlog.

---

# 40. Resumo Executivo

## Público

Criadores individuais e pequenas equipes.

## Problema

O processo entre tema, pesquisa e roteiro é fragmentado, manual e difícil de reproduzir.

## Solução

Um Pipeline rastreável de Research e Script.

## Entrada

Tema e contexto opcional.

## Saídas

Research Artifact e Script Artifact.

## Diferencial

Projects, Pipelines, Executions e Artifacts persistentes, em vez de respostas isoladas em chats.

## Critério central de sucesso

O usuário consegue produzir um roteiro utilizável mais rapidamente e deseja repetir o processo.

---

# 41. Filosofia Final

O MVP deverá provar uma única promessa:

> O Infinite Content AI transforma um tema em pesquisa e roteiro de maneira organizada, reutilizável e rastreável.

Ele não precisa provar que consegue gerar, publicar e analisar todos os formatos de conteúdo.

Primeiro deverá entregar valor no caminho mais curto:

```text
Ideia
    ↓
Pesquisa
    ↓
Roteiro
    ↓
Revisão humana
```

Quando esse fluxo estiver funcionando e sendo reutilizado, existirá uma base concreta para expandir o produto.