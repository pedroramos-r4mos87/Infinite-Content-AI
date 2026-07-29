# Estratégia de Testes

## Objetivo

Definir como o Infinite Content AI verificará a correção, segurança, resiliência, compatibilidade e qualidade de seus componentes.

A estratégia de testes deverá reduzir a probabilidade de falhas em produção e permitir evolução contínua da plataforma sem perda de confiança.

Os testes deverão validar não apenas unidades isoladas, mas também:

- Regras arquiteturais.
    
- Casos de uso.
    
- Persistência.
    
- Integrações.
    
- Mensageria.
    
- Pipelines.
    
- Agentes.
    
- Prompts.
    
- Contratos.
    
- Segurança.
    
- Resiliência.
    
- Performance.
    
- Migrações.
    
- Observabilidade.
    
- Recuperação de falhas.
    

O objetivo não será atingir uma porcentagem arbitrária de cobertura.

O objetivo será criar evidências confiáveis de que o sistema se comporta conforme sua arquitetura e seus requisitos.

---

# Princípios

A estratégia seguirá os seguintes princípios:

- Testes deverão proteger comportamentos, não detalhes internos.
    
- Regras críticas deverão possuir testes automatizados.
    
- Testes deverão ser determinísticos sempre que possível.
    
- Testes deverão ser rápidos no nível adequado.
    
- Integrações reais deverão ser testadas de forma controlada.
    
- Testes não deverão depender da ordem de execução.
    
- Cada teste deverá possuir isolamento.
    
- Falhas deverão produzir diagnóstico claro.
    
- Testes frágeis deverão ser corrigidos, não ignorados.
    
- Cobertura não substitui qualidade.
    
- Mocks deverão ser utilizados com moderação.
    
- Banco em memória não substitui PostgreSQL real.
    
- Contratos externos deverão possuir testes de compatibilidade.
    
- Pipelines deverão ser testados como máquinas de estado.
    
- Agentes deverão ser avaliados por comportamento e qualidade.
    
- Prompts deverão possuir regressão.
    
- Falhas transitórias deverão ser simuladas.
    
- Migrações deverão ser testadas.
    
- Testes deverão fazer parte do CI/CD.
    
- Produção deverá possuir validações contínuas seguras.
    

---

# Objetivos da Estratégia

A estratégia deverá responder:

- O domínio está correto?
    
- A Application executa os casos de uso corretamente?
    
- As dependências respeitam a Clean Architecture?
    
- O banco preserva integridade?
    
- Os contratos permanecem compatíveis?
    
- As mensagens são processadas com idempotência?
    
- Os pipelines podem ser retomados?
    
- Os providers são normalizados corretamente?
    
- Os agentes produzem saídas válidas?
    
- Os prompts mantêm qualidade?
    
- O sistema resiste a falhas?
    
- As migrations são seguras?
    
- O sistema atende aos limites de performance?
    
- As políticas de segurança funcionam?
    
- Os deploys podem ser realizados com confiança?
    

---

# Modelo de Qualidade

A qualidade do Infinite Content AI será analisada em múltiplas dimensões:

```text
Corretude
Confiabilidade
Segurança
Resiliência
Compatibilidade
Performance
Observabilidade
Manutenibilidade
Qualidade de IA
Experiência operacional
```

Nenhum único tipo de teste cobre todas essas dimensões.

---

# Pirâmide de Testes

A base deverá conter muitos testes rápidos.

```text
                 Testes Exploratórios
              Testes End-to-End Críticos
            Testes de Contrato e Sistema
          Testes de Integração
       Testes de Componentes
    Testes Unitários
```

A maior parte dos testes deverá estar nos níveis inferiores.

Os níveis superiores serão menos numerosos, porém cobrirão fluxos críticos.

---

# Distribuição Conceitual

Uma referência inicial poderá ser:

```text
Testes Unitários: muitos
Testes de Integração: quantidade moderada
Testes de Contrato: focados
Testes End-to-End: poucos e críticos
Testes de Performance: cenários selecionados
Testes de IA: conjuntos contínuos de avaliação
```

Esses valores não deverão ser transformados em metas rígidas.

---

# Tipos de Teste

A plataforma deverá utilizar:

- Testes unitários.
    
- Testes de componentes.
    
- Testes de integração.
    
- Testes de arquitetura.
    
- Testes de contrato.
    
- Testes de banco.
    
- Testes de mensageria.
    
- Testes de providers.
    
- Testes de pipelines.
    
- Testes de agentes.
    
- Testes de prompts.
    
- Testes de IA.
    
- Testes end-to-end.
    
- Testes de segurança.
    
- Testes de resiliência.
    
- Testes de performance.
    
- Testes de carga.
    
- Testes de migrations.
    
- Testes de observabilidade.
    
- Testes de recuperação.
    
- Testes exploratórios.
    

---

# Estrutura dos Projetos de Teste

Estrutura sugerida:

```text
tests/
├── Domain.UnitTests/
├── Application.UnitTests/
├── Application.ComponentTests/
├── Architecture.Tests/
├── Data.IntegrationTests/
├── Infrastructure.IntegrationTests/
├── Api.IntegrationTests/
├── Worker.IntegrationTests/
├── Contracts.Tests/
├── Messaging.IntegrationTests/
├── Pipelines.Tests/
├── Agents.Tests/
├── Ai.EvaluationTests/
├── EndToEnd.Tests/
├── Performance.Tests/
├── Security.Tests/
└── Shared.Testing/
```

---

# Shared.Testing

O projeto `Shared.Testing` poderá conter:

- Builders.
    
- Fixtures.
    
- Fakes.
    
- Test data factories.
    
- Test clocks.
    
- Test IDs.
    
- Containers compartilhados.
    
- Assertions customizadas.
    
- Fixtures de mensagens.
    
- Helpers de autenticação.
    
- Helpers de banco.
    
- Fake providers.
    
- Cenários reutilizáveis.
    

Ele não deverá se tornar um conjunto desorganizado de utilitários.

---

# Convenção de Nomes

Os testes deverão comunicar comportamento.

Exemplo:

```csharp
StartPipeline_WhenProjectIsInactive_ReturnsFailure();
```

ou:

```csharp
GivenInactiveProject_WhenPipelineIsStarted_ThenOperationIsRejected();
```

A equipe deverá escolher uma convenção consistente.

---

# Estrutura Arrange, Act, Assert

Exemplo:

```csharp
[Fact]
public void Approve_WhenArtifactIsAlreadyApproved_ReturnsConflict()
{
    // Arrange
    var artifact = ArtifactBuilder.Approved().Build();

    // Act
    var result = artifact.Approve();

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Code.Should().Be("artifact_already_approved");
}
```

---

# Testes Unitários

Testes unitários validarão comportamentos isolados sem infraestrutura real.

Devem ser:

- Rápidos.
    
- Determinísticos.
    
- Isolados.
    
- Fáceis de executar.
    
- Específicos.
    
- Independentes de rede.
    
- Independentes de banco.
    

---

# O que Testar no Domain

- Invariantes.
    
- Value Objects.
    
- Entidades.
    
- Aggregates.
    
- Transições de estado.
    
- Regras de negócio.
    
- Domain Services.
    
- Domain Events.
    
- Cálculos.
    
- Políticas.
    
- Result Pattern.
    
- Erros de domínio.
    

---

# Exemplo de Teste de Domínio

```csharp
[Fact]
public void Start_WhenExecutionIsPending_ChangesStatusToRunning()
{
    var execution = PipelineExecution.CreatePending();

    var result = execution.Start();

    result.IsSuccess.Should().BeTrue();
    execution.Status.Should().Be(PipelineExecutionStatus.Running);
}
```

---

# Testes de Invariantes

Exemplos:

- Pipeline concluído não pode ser reiniciado.
    
- Artefato rejeitado não pode ser publicado.
    
- Publicação exige aprovação.
    
- Custo acumulado não pode ser negativo.
    
- Uma etapa concluída não volta para Pending sem operação explícita.
    
- Tenant não pode acessar recurso de outro tenant.
    
- Versão publicada não pode ser alterada.
    

---

# Value Objects

Value Objects deverão possuir testes para:

- Criação válida.
    
- Criação inválida.
    
- Igualdade.
    
- Normalização.
    
- Serialização quando relevante.
    
- Limites.
    
- Formatos.
    

Exemplos:

```text
Email
Money
ProviderModelId
CorrelationId
StorageReference
ContentLanguage
```

---

# Testes da Application

A Application deverá ser testada por casos de uso.

Exemplos:

- Commands.
    
- Queries.
    
- Validators.
    
- Handlers.
    
- Policies.
    
- Resolvers.
    
- Orquestrações.
    
- Mapeamentos.
    
- Autorização de caso de uso.
    

---

# Teste de Handler

Um handler deverá ser testado com dependências controladas.

Exemplo:

```text
StartPipelineCommandHandler
    usa
IProjectRepository
IPipelineRepository
IUnitOfWork
IClock
```

O teste verificará:

- Resultado.
    
- Alterações.
    
- Interações relevantes.
    
- Evento gerado.
    
- Persistência solicitada.
    

---

# Evitar Testar Implementação

Não testar:

- Quantidade irrelevante de chamadas internas.
    
- Métodos privados.
    
- Ordem interna sem significado de negócio.
    
- Estruturas temporárias.
    
- Detalhes de biblioteca.
    

Testar o comportamento observável.

---

# Mocks

Mocks serão úteis para:

- Simular dependências lentas.
    
- Simular erros específicos.
    
- Verificar efeitos externos relevantes.
    
- Testar branches de resiliência.
    
- Isolar regras.
    

Mocks não deverão substituir testes de integração.

---

# Excesso de Mocks

Sinais de problema:

- Teste possui dezenas de setups.
    
- Refatorações internas quebram todos os testes.
    
- O teste replica a implementação.
    
- O teste passa, mas integração real falha.
    
- Interfaces são criadas apenas para facilitar mocking.
    

Nesses casos, considerar teste de componente ou integração.

---

# Fakes

Fakes poderão ser preferidos a mocks para comportamentos complexos.

Exemplos:

- FakeClock.
    
- FakeAiProvider.
    
- FakeMessagePublisher.
    
- InMemoryPromptRepository para testes específicos.
    
- FakeStorage.
    
- FakePermissionEvaluator.
    

Fakes deverão possuir comportamento previsível.

---

# Test Clock

Tempo não deverá depender diretamente de `DateTime.UtcNow`.

Utilizar:

```csharp
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
```

Nos testes:

```csharp
var clock = new FakeClock(
    new DateTimeOffset(2026, 7, 23, 12, 0, 0, TimeSpan.Zero));
```

Isso permite testar:

- Expiração.
    
- Agendamento.
    
- Timeout lógico.
    
- Retenção.
    
- Aprovações vencidas.
    
- Leases.
    

---

# Test Data Builders

Builders deverão reduzir duplicação.

Exemplo:

```csharp
var project = ProjectBuilder
    .Active()
    .ForOrganization(organizationId)
    .WithPublishingEnabled()
    .Build();
```

Builders deverão usar defaults válidos.

O teste altera apenas o que interessa.

---

# Object Mother

Poderá ser usada para cenários conhecidos.

Exemplo:

```text
ValidYouTubeProject
ExpiredApproval
CompletedPipelineExecution
FailedPublication
```

Evitar objetos mágicos difíceis de compreender.

---

# Testes de Componentes

Testes de componentes validam uma parte maior do sistema com algumas dependências reais ou substituídas.

Exemplo:

```text
Application
+
Validators
+
Handlers
+
Fake Infrastructure
```

Podem validar casos de uso completos sem HTTP nem banco real.

---

# Objetivo dos Component Tests

- Reduzir excesso de mocks.
    
- Testar colaboração entre classes.
    
- Executar fluxos rápidos.
    
- Validar pipeline de behaviors.
    
- Testar validação, autorização e handlers juntos.
    
- Testar resultados e eventos.
    

---

# Testes de Arquitetura

Testes de arquitetura deverão impedir violações estruturais.

Ferramentas possíveis:

- NetArchTest.
    
- ArchUnitNET.
    
- Testes customizados por reflection.
    
- Analyzers Roslyn.
    

---

# Regras de Dependência

Deverão ser testadas regras como:

```text
Domain não depende de Application
Domain não depende de Data
Domain não depende de Infrastructure
Application não depende de Data
Application não depende de Infrastructure
Data não depende de Infrastructure
Infrastructure não depende de Data
Api depende das camadas de composição permitidas
Worker depende das camadas de composição permitidas
```

---

# Teste de Arquitetura Exemplo

```csharp
[Fact]
public void Domain_ShouldNotDependOnInfrastructure()
{
    var result = Types
        .InAssembly(typeof(DomainAssemblyReference).Assembly)
        .ShouldNot()
        .HaveDependencyOn("InfiniteContent.Infrastructure")
        .GetResult();

    result.IsSuccessful.Should().BeTrue();
}
```

---

# Regras para Data

Testar que:

- DbContext existe somente em Data.
    
- Repositories concretos ficam em Data.
    
- Migrations ficam em Data.
    
- Data não referencia providers externos.
    
- Data não contém clientes HTTP de integração.
    
- Data não contém serviços de IA.
    

---

# Regras para Infrastructure

Testar que:

- Infrastructure não contém DbContext.
    
- Infrastructure implementa interfaces externas.
    
- Providers ficam em Infrastructure.
    
- Mensageria fica em Infrastructure.
    
- Storage fica em Infrastructure.
    
- n8n fica em Infrastructure.
    

---

# Regras de Nomenclatura

Testes poderão validar:

- Commands terminam com `Command`.
    
- Queries terminam com `Query`.
    
- Handlers terminam com `Handler`.
    
- Integration Events terminam com `IntegrationEvent`.
    
- Validators terminam com `Validator`.
    
- Repositories concretos terminam com `Repository`.
    
- Options terminam com `Options`.
    

---

# Regras de Imutabilidade

Testar que:

- Contratos são records ou tipos imutáveis.
    
- Integration Events não possuem setters mutáveis.
    
- Pipeline Definitions publicadas não expõem alteração livre.
    
- Value Objects são imutáveis.
    

---

# Testes de Integração

Testes de integração validarão comunicação entre componentes reais.

Exemplos:

- Application + PostgreSQL.
    
- Infrastructure + provider stub.
    
- API + Application + Data.
    
- Worker + broker.
    
- Outbox + banco + broker.
    
- Inbox + consumer.
    
- Storage + emulator.
    
- Redis + cache adapter.
    

---

# Testcontainers

Testcontainers deverá ser utilizado para dependências reais em testes.

Possíveis containers:

- PostgreSQL.
    
- RabbitMQ.
    
- Redis.
    
- Azurite.
    
- LocalStack, se necessário.
    
- WireMock.
    
- Serviço Python de teste.
    

---

# Benefícios do Testcontainers

- Ambiente próximo de produção.
    
- Isolamento.
    
- Reprodutibilidade.
    
- Execução local.
    
- Execução em CI.
    
- Validação de comportamento real.
    
- Menor diferença entre teste e produção.
    

---

# PostgreSQL Real

Testes do Data deverão usar PostgreSQL real.

Não utilizar EF Core InMemory como substituto de PostgreSQL para validar:

- Constraints.
    
- Transactions.
    
- JSONB.
    
- Índices.
    
- Concorrência.
    
- SQL.
    
- Migrations.
    
- Tipos específicos.
    
- Comportamento relacional.
    

---

# EF Core InMemory

Poderá ser utilizado somente em testes muito específicos onde o comportamento relacional não seja relevante.

Não deverá ser a estratégia principal.

---

# Fixture de Banco

Uma fixture poderá:

1. Iniciar container.
    
2. Aplicar migrations.
    
3. Criar connection string.
    
4. Construir DbContext.
    
5. Limpar dados entre testes.
    
6. Encerrar container.
    

---

# Isolamento de Banco

Possíveis estratégias:

- Database por classe.
    
- Schema por teste.
    
- Transaction rollback.
    
- Respawn.
    
- Truncate controlado.
    
- Container por suíte.
    

A escolha deverá equilibrar velocidade e isolamento.

---

# Respawn

Uma ferramenta como Respawn poderá limpar tabelas entre testes.

Cuidados:

- Preservar migrations.
    
- Preservar tabelas de referência quando necessário.
    
- Respeitar foreign keys.
    
- Não esconder problemas de transação.
    

---

# Testes de Repository

Validar:

- Inserção.
    
- Atualização.
    
- Exclusão lógica.
    
- Filtros.
    
- Paginação.
    
- Queries.
    
- Concurrency token.
    
- Includes.
    
- Multi-tenancy.
    
- Constraints.
    
- Transações.
    
- Cancelamento.
    

---

# Testes de Query

Queries complexas deverão possuir testes com dados reais.

Validar:

- Filtros combinados.
    
- Ordenação.
    
- Paginação.
    
- Performance básica.
    
- Isolamento por tenant.
    
- Projeções.
    
- Null handling.
    
- Datas.
    

---

# Testes de Constraints

Testar constraints como:

- Unique keys.
    
- Foreign keys.
    
- Check constraints.
    
- Not null.
    
- Idempotency keys.
    
- Inbox uniqueness.
    
- Artifact version uniqueness.
    
- Pipeline name e version.
    

---

# Testes de Concorrência

Deverão simular:

- Duas atualizações do mesmo aggregate.
    
- Dois Workers adquirindo lease.
    
- Duas aprovações.
    
- Duas publicações com mesma idempotency key.
    
- Dois processadores da Outbox.
    
- Reprocessamento duplicado.
    

---

# Teste de Optimistic Concurrency

Fluxo:

```text
Carregar entidade em contexto A
Carregar entidade em contexto B
Atualizar e salvar A
Atualizar e salvar B
Esperar conflito
```

O teste deverá verificar a resposta normalizada.

---

# Testes de Transactions

Validar:

- Commit bem-sucedido.
    
- Rollback.
    
- Falha antes do commit.
    
- Falha durante processamento.
    
- Outbox na mesma transação.
    
- Inbox e alteração de estado na mesma transação.
    
- Ausência de chamada externa dentro da transação.
    

---

# Testes de Migrations

Deverão validar:

- Criação do banco do zero.
    
- Migração da versão anterior.
    
- Backfills.
    
- Constraints.
    
- Índices.
    
- Dados existentes.
    
- Compatibilidade com código.
    
- Tempo de execução.
    
- Locks.
    
- Reversibilidade quando suportada.
    

---

# Banco Vazio até Atual

O pipeline deverá executar todas as migrations em um PostgreSQL vazio.

Resultado esperado:

- Schema criado.
    
- Constraints válidas.
    
- Seed técnico aplicado.
    
- Aplicação inicia.
    
- Health check passa.
    

---

# Banco Anterior até Atual

Deverá existir teste de upgrade.

Fluxo:

```text
Criar banco na versão anterior
Inserir dados representativos
Aplicar migrations novas
Executar validações
```

---

# Dados Representativos

Incluir cenários como:

- Pipelines em andamento.
    
- Outbox pendente.
    
- Inbox processada.
    
- Artefatos versionados.
    
- Configurações antigas.
    
- Dados JSONB.
    
- Soft deletes.
    
- Tenants diferentes.
    

---

# Migrations Destrutivas

Testes deverão avaliar:

- Perda de dados.
    
- Locks.
    
- Compatibilidade com deploy anterior.
    
- Necessidade de backfill.
    
- Estratégia expand and contract.
    
- Rollback ou forward fix.
    

---

# Testes da API

Testes da API deverão utilizar um host real em memória.

Ferramentas:

- `WebApplicationFactory`.
    
- `TestServer`.
    
- HttpClient real.
    
- PostgreSQL em container.
    
- Dependências externas substituídas.
    

---

# Casos da API

Validar:

- Rotas.
    
- Status HTTP.
    
- Autenticação.
    
- Autorização.
    
- Validation.
    
- Problem Details.
    
- Serialização.
    
- Versionamento.
    
- Paginação.
    
- CorrelationId.
    
- Headers.
    
- Rate limiting.
    
- Uploads.
    
- Cancelamento.
    

---

# Teste de Endpoint

Exemplo:

```text
POST /api/v1/pipeline-executions
```

Deverá validar:

- 401 sem autenticação.
    
- 403 sem permissão.
    
- 400 com payload inválido.
    
- 404 para projeto inexistente.
    
- 409 para estado incompatível.
    
- 202 para criação aceita.
    
- Persistência da execução.
    
- Outbox criada.
    
- Resposta correta.
    

---

# Autenticação em Testes

Poderá existir um authentication handler de teste.

Ele deverá permitir simular:

- Usuário autenticado.
    
- Claims.
    
- OrganizationId.
    
- Roles.
    
- Permissions.
    
- Token expirado.
    
- Usuário sem tenant.
    

---

# Testes Multi-Tenant

Obrigatórios para operações relevantes.

Validar:

- Usuário da organização A não lê dados da B.
    
- IDs previsíveis não permitem acesso.
    
- Queries sempre filtram tenant.
    
- Consumers respeitam OrganizationId.
    
- Storage paths respeitam tenant.
    
- WebSockets respeitam grupos autorizados.
    

---

# Problem Details

Testar:

- Status.
    
- Code.
    
- Title.
    
- TraceId.
    
- Validation errors.
    
- Ausência de stack trace.
    
- Ausência de informações sensíveis.
    
- Consistência entre endpoints.
    

---

# Testes de Contratos HTTP

Validar:

- OpenAPI.
    
- Requests.
    
- Responses.
    
- Campos obrigatórios.
    
- Campos opcionais.
    
- Enums.
    
- Datas.
    
- Compatibilidade entre versões.
    
- Exemplos.
    

---

# OpenAPI Snapshot

O OpenAPI poderá ser salvo como snapshot.

Alterações deverão ser revisadas.

O CI deverá detectar:

- Endpoint removido.
    
- Campo removido.
    
- Mudança de tipo.
    
- Novo campo obrigatório.
    
- Alteração incompatível.
    

---

# Testes de Mensageria

Mensageria deverá ser testada com broker real.

Validar:

- Publicação.
    
- Consumo.
    
- Routing.
    
- Serialization.
    
- Acknowledgement.
    
- Retry.
    
- DLQ.
    
- Outbox.
    
- Inbox.
    
- Duplicidade.
    
- CorrelationId.
    
- Versionamento.
    
- Shutdown.
    

---

# RabbitMQ em Testes

Utilizar container do RabbitMQ.

Configurar:

- Exchanges.
    
- Queues.
    
- Bindings.
    
- DLX.
    
- Retry queues.
    
- Publisher confirms.
    
- Prefetch.
    

---

# Teste de Publicação

Fluxo:

```text
Publisher envia Integration Event
    ↓
Broker confirma
    ↓
Queue recebe
    ↓
Payload é desserializável
```

---

# Teste de Consumer

Validar:

- Mensagem válida processada.
    
- Mensagem duplicada ignorada.
    
- Mensagem inválida rejeitada.
    
- Falha transitória reagendada.
    
- Falha permanente enviada para DLQ.
    
- Ack após commit.
    
- CorrelationId propagado.
    

---

# Teste da Outbox

Cenários:

- Estado e Outbox no mesmo commit.
    
- Publicação bem-sucedida.
    
- Broker indisponível.
    
- Retry.
    
- Publicação duplicada.
    
- Duas instâncias processando.
    
- Lock expirado.
    
- Mensagem antiga.
    
- Contrato versionado.
    

---

# Teste da Inbox

Cenários:

- Primeira entrega.
    
- Entrega duplicada.
    
- Mesmo MessageId para consumers diferentes.
    
- Falha após registro parcial.
    
- Processamento e estado na mesma transação.
    
- Reprocessamento após falha.
    
- Mensagem já concluída.
    

---

# Teste de DLQ

Validar:

- Máximo de tentativas.
    
- Headers preservados.
    
- Payload original preservado.
    
- ErrorCode registrado.
    
- Fila original registrada.
    
- Reprocessamento controlado.
    
- Idempotência após reprocessamento.
    

---

# Testes de Ordenação

Quando a ordem for relevante:

- Eventos em sequência.
    
- Evento antigo após novo.
    
- Evento ausente.
    
- AggregateVersion duplicada.
    
- Particionamento por chave.
    
- Reconciliação.
    

---

# Testes de Contracts

O projeto `Contracts.Tests` deverá validar:

- Serialização.
    
- Desserialização.
    
- Imutabilidade.
    
- MessageType.
    
- MessageVersion.
    
- Campos obrigatórios.
    
- Compatibilidade.
    
- Fixtures históricas.
    
- Schemas.
    

---

# Fixtures Históricas

Manter exemplos como:

```text
pipeline-started.v1.json
pipeline-completed.v1.json
publication-completed.v2.json
```

O consumer atual deverá conseguir processar as versões suportadas.

---

# Teste de Round Trip

```text
Objeto
    ↓
Serialização
    ↓
JSON
    ↓
Desserialização
    ↓
Objeto equivalente
```

---

# Compatibilidade de Schema

O CI deverá detectar breaking changes em:

- JSON Schema.
    
- OpenAPI.
    
- AsyncAPI.
    
- Protobuf ou Avro, se adotados.
    
- Contratos C# compartilhados.
    

---

# Consumer-Driven Contracts

Poderão ser utilizados para integrações HTTP externas ou internas.

Objetivo:

- Garantir que provider e consumer concordem.
    
- Evitar mudanças incompatíveis.
    
- Registrar expectativas reais do consumer.
    

Ferramentas como Pact poderão ser avaliadas.

---

# Testes de Providers

Cada provider deverá possuir testes em múltiplos níveis.

## Unitários

- Mapeamento de request.
    
- Mapeamento de response.
    
- Normalização de erro.
    
- Seleção de modelo.
    
- Validação de capabilities.
    
- Cálculo de custo.
    

## Integração com Stub

- HTTP.
    
- Headers.
    
- Timeout.
    
- Retry.
    
- Rate limit.
    
- Respostas inválidas.
    
- Streaming, quando aplicável.
    

## Sandbox Real

- Credenciais de teste.
    
- Modelos reais.
    
- Limites baixos.
    
- Execução controlada.
    
- Não obrigatório em todo commit.
    

---

# WireMock

WireMock poderá simular APIs externas.

Cenários:

- HTTP 200.
    
- HTTP 400.
    
- HTTP 401.
    
- HTTP 429.
    
- HTTP 500.
    
- Timeout.
    
- Resposta truncada.
    
- JSON inválido.
    
- Header Retry-After.
    
- Resposta lenta.
    
- Conexão interrompida.
    

---

# Teste de Normalização

Exemplo:

```text
OpenAI retorna 429
    ↓
Infrastructure converte
    ↓
provider_rate_limit
    ↓
IsTransient = true
```

A Application não deverá receber detalhes proprietários desnecessários.

---

# Testes de Fallback

Validar:

- Provider principal funciona.
    
- Principal falha de forma transitória.
    
- Fallback permitido.
    
- Fallback não permitido.
    
- Modelo incompatível.
    
- Limite de custo.
    
- Policy version.
    
- Registro de provider final.
    
- Ausência de loop.
    

---

# Testes de Rate Limit

Validar:

- Respeito a Retry-After.
    
- Backoff.
    
- Limite de tentativas.
    
- Reagendamento assíncrono.
    
- Métricas.
    
- Circuit breaker quando apropriado.
    

---

# Testes de Storage

Validar:

- Upload.
    
- Download.
    
- Delete.
    
- URL temporária.
    
- Expiração.
    
- Metadata.
    
- Content type.
    
- Tamanho máximo.
    
- Path seguro.
    
- Isolamento por tenant.
    
- Arquivo inexistente.
    
- Upload interrompido.
    
- Integridade.
    

---

# Storage Emulator

Azurite, MinIO ou equivalente poderá ser utilizado conforme o provider escolhido.

A semântica deverá ser próxima da produção.

---

# Testes de Cache

Validar:

- Cache hit.
    
- Cache miss.
    
- TTL.
    
- Invalidação.
    
- Serialização.
    
- Namespace versionado.
    
- Falha do Redis.
    
- Fallback para fonte oficial.
    
- Stampede protection.
    
- Multi-tenancy.
    

---

# Testes de Pipelines

Pipelines deverão ser tratados como máquinas de estado versionadas.

Testar:

- Início.
    
- Ordem de steps.
    
- Inputs.
    
- Outputs.
    
- Checkpoints.
    
- Retry.
    
- Fallback.
    
- Aprovação.
    
- Cancelamento.
    
- Compensação.
    
- Resume.
    
- Timeout.
    
- Custo.
    
- Versionamento.
    

---

# Teste de Definição

Toda definição publicada deverá ser validada.

Verificar:

- Nome.
    
- Versão.
    
- Steps únicos.
    
- Handlers existentes.
    
- Dependências válidas.
    
- Ausência de ciclos.
    
- Entrada e saída compatíveis.
    
- Timeouts válidos.
    
- Retry válido.
    
- Steps obrigatórios.
    
- Compensações existentes.
    

---

# Teste de Pipeline Feliz

Exemplo:

```text
Trend
Research
Strategy
Script
Review
Translation
Voice
Thumbnail
Video
SEO
Publishing
Analytics
```

Validar:

- Todos os steps executados.
    
- Estado final Completed.
    
- Checkpoints.
    
- Artefatos.
    
- Custos.
    
- Eventos.
    
- Manifest.
    

---

# Teste de Falha de Step

Cenários:

- Falha transitória.
    
- Falha permanente.
    
- Step opcional.
    
- Step obrigatório.
    
- Retry esgotado.
    
- Fallback.
    
- Compensação.
    
- DLQ.
    
- Intervenção humana.
    

---

# Teste de Resume

Fluxo:

```text
Executar steps A, B e C
Interromper
Carregar checkpoint
Retomar
Garantir que A, B e C não repetem
Executar D
```

---

# Teste de Worker Interrompido

Simular:

- Worker morto durante provider call.
    
- Worker morto após efeito externo.
    
- Worker morto antes do checkpoint.
    
- Worker morto após checkpoint.
    
- Lease expirado.
    
- Novo Worker assume.
    

---

# Teste de Idempotência de Step

Executar duas vezes o mesmo step com:

```text
PipelineExecutionId
StepName
StepVersion
```

Esperar apenas um efeito válido.

---

# Teste de Aprovação

Validar:

- Pipeline entra em WaitingApproval.
    
- Evento é emitido.
    
- Aprovação válida continua.
    
- Rejeição falha ou retorna.
    
- Aprovação expirada.
    
- Aprovação duplicada.
    
- Usuário sem permissão.
    
- Versão do artefato aprovada.
    

---

# Teste de Cancelamento

Validar cancelamento:

- Antes do início.
    
- Durante step.
    
- Durante retry.
    
- Durante espera.
    
- Durante aprovação.
    
- Após conclusão.
    
- Com operação externa incerta.
    
- Com compensação.
    

---

# Testes de Agentes

Agentes exigem validação funcional e avaliação probabilística.

Deverão existir:

- Testes unitários.
    
- Testes de ferramentas.
    
- Testes de schema.
    
- Testes de políticas.
    
- Testes de segurança.
    
- Avaliações de qualidade.
    
- Testes de regressão.
    
- Testes com modelos reais controlados.
    

---

# Testes Unitários de Agentes

Validar:

- Construção do contexto.
    
- Seleção de prompt.
    
- Seleção de ferramentas.
    
- Validação de entrada.
    
- Validação de saída.
    
- Tratamento de recusa.
    
- Retry.
    
- Fallback.
    
- Custos.
    
- Limites.
    
- AgentResult.
    

---

# Fake AI Provider

Um Fake Provider deverá permitir respostas programadas.

Exemplo:

```csharp
fakeProvider
    .WhenPromptContains("Generate script")
    .ReturnsJson(validScript);
```

Também deverá simular:

- Timeout.
    

- JSON inválido.
    
- Resposta vazia.
    
- Tool call.
    
- Recusa.
    
- Conteúdo inseguro.
    

---

# Testes de Tool Calling

Validar:

- Ferramenta permitida.
    
- Ferramenta proibida.
    
- Argumentos válidos.
    
- Argumentos inválidos.
    
- Limite de chamadas.
    
- Timeout.
    
- Resultado da ferramenta.
    
- Prompt injection.
    
- Efeito externo exige aprovação.
    

---

# Segurança de Agentes

Testar:

- Prompt injection.
    
- Exfiltração de segredos.
    
- Tentativa de usar tool proibida.
    
- Conteúdo externo malicioso.
    
- Instruções conflitantes.
    
- Cross-tenant data leakage.
    
- Bypass de aprovação.
    
- Saída insegura.
    
- Uso de URLs maliciosas.
    

---

# Testes de Prompts

Prompts deverão possuir suites de regressão.

Validar:

- Variáveis.
    
- Template.
    
- Hash.
    
- Structured output.
    
- Idioma.
    
- Tom.
    
- Restrições.
    
- Conteúdo obrigatório.
    
- Conteúdo proibido.
    
- Qualidade.
    
- Custo.
    
- Latência.
    

---

# Prompt Test Cases

Cada prompt poderá possuir casos como:

```text
Caso simples
Caso ambíguo
Caso longo
Caso multilíngue
Caso adversarial
Caso com dados incompletos
Caso com conteúdo sensível
Caso próximo do limite de contexto
```

---

# Golden Dataset

A plataforma deverá manter conjuntos de avaliação.

Exemplos:

- Tópicos.
    
- Briefings.
    
- Roteiros esperados.
    
- Critérios de qualidade.
    
- Exemplos aprovados.
    
- Exemplos rejeitados.
    
- Casos de segurança.
    
- Casos multilíngues.
    

---

# Golden Answer

Para tarefas determinísticas, poderá existir resposta esperada.

Para tarefas criativas, utilizar critérios e rubricas em vez de igualdade exata.

---

# Avaliação de IA

A qualidade poderá ser medida por:

- Schema valid rate.
    
- Factual consistency.
    
- Relevance.
    
- Completeness.
    
- Tone adherence.
    
- Safety.
    
- Originality.
    
- Readability.
    
- Human approval rate.
    
- Cost.
    
- Latency.
    

---

# Rubricas

Exemplo para roteiro:

|Critério|Peso|
|---|--:|
|Aderência ao tema|20|
|Clareza|15|
|Estrutura narrativa|20|
|Precisão factual|20|
|Adequação ao público|15|
|CTA|10|

---

# LLM-as-Judge

Poderá ser utilizado como sinal auxiliar.

Não deverá ser a única fonte de verdade.

Cuidados:

- Viés.
    
- Instabilidade.
    
- Modelo julgando a si mesmo.
    
- Sensibilidade à ordem.
    
- Custo.
    
- Reprodutibilidade.
    

---

# Avaliação Humana

Será necessária para:

- Qualidade editorial.
    
- Criatividade.
    
- Tom.
    
- Adequação cultural.
    
- Segurança em casos ambíguos.
    
- Conteúdo de alto impacto.
    
- Validação de novas versões.
    

---

# Blind Evaluation

Comparações entre versões deverão esconder, quando possível:

- Nome do modelo.
    
- Versão do prompt.
    
- Provider.
    
- Hipótese da equipe.
    

Isso reduz viés.

---

# A/B Evaluation

Exemplo:

```text
Prompt v7
versus
Prompt v8
```

Comparar:

- Aprovação.
    
- Qualidade.
    
- Custo.
    
- Latência.
    
- Falhas.
    
- Segurança.
    

---

# Testes Não Determinísticos

Testes com IA real não deverão bloquear todo commit por pequenas variações.

Separar:

## Determinísticos

- Schema.
    
- Campos.
    
- Regras.
    
- Segurança explícita.
    
- Limites.
    
- Parsers.
    

## Estatísticos

- Qualidade média.
    
- Taxa de aprovação.
    
- Consistência.
    
- Criatividade.
    

---

# Tolerâncias

Avaliações probabilísticas deverão usar limites.

Exemplo:

```text
Schema valid rate >= 98%
Safety violations = 0
Approval score médio >= 4.2
Cost increase <= 10%
```

---

# Repetições

Casos probabilísticos poderão ser executados múltiplas vezes.

Registrar:

- Seed, quando suportado.
    
- Modelo.
    
- Timestamp.
    
- Prompt version.
    
- Parâmetros.
    
- Região.
    
- Request IDs.
    

---

# Testes de RAG

Validar separadamente:

- Ingestão.
    
- Parsing.
    
- Chunking.
    
- Embeddings.
    
- Indexação.
    
- Busca.
    
- Filtros.
    
- Reranking.
    
- Context assembly.
    
- Resposta final.
    
- Citações.
    

---

# Retrieval Tests

Métricas possíveis:

- Recall@K.
    
- Precision@K.
    
- Mean Reciprocal Rank.
    
- NDCG.
    
- Context relevance.
    
- Source coverage.
    

---

# Testes de Chunking

Validar:

- Tamanho.
    
- Overlap.
    
- Preservação de contexto.
    
- Tabelas.
    
- Títulos.
    
- Código.
    
- Documentos grandes.
    
- Idiomas.
    
- Caracteres especiais.
    

---

# Testes de Citações

Quando o sistema gerar citações, validar:

- Fonte existe.
    
- Trecho suporta a afirmação.
    
- Identificador correto.
    
- Ausência de fonte inventada.
    
- Tenant correto.
    
- Documento autorizado.
    

---

# Testes End-to-End

End-to-end deverá validar poucos fluxos críticos completos.

Exemplos:

- Criar projeto e executar pipeline.
    
- Gerar roteiro e aprovar.
    
- Publicar conteúdo.
    
- Receber webhook.
    
- Retomar pipeline interrompido.
    
- Reprocessar mensagem da DLQ.
    
- Rotacionar provider.
    

---

# Ambiente E2E

Deverá conter:

- API.
    
- Worker.
    
- PostgreSQL.
    
- Broker.
    
- Storage.
    
- Providers fake ou sandbox.
    
- Serviço Python fake ou real controlado.
    
- Observabilidade mínima.
    

---

# E2E com Providers Fakes

Será a estratégia principal para execução frequente.

Benefícios:

- Determinismo.
    
- Custo baixo.
    
- Velocidade.
    
- Simulação de falhas.
    
- Controle de respostas.
    

---

# E2E com Providers Reais

Executado em:

- Pipeline noturno.
    
- Pré-release.
    
- Ambiente staging.
    
- Validação manual.
    
- Smoke test controlado.
    

Com:

- Orçamento.
    
- Modelos permitidos.
    
- Dados não sensíveis.
    
- Limite de execução.
    
- Auditoria.
    

---

# Testes de Smoke

Após deploy, executar verificações rápidas:

- API responde.
    
- Banco acessível.
    
- Broker acessível.
    
- Storage acessível.
    
- Autenticação funciona.
    
- Command pode ser enfileirado.
    
- Worker consome.
    
- Provider essencial responde ou está corretamente degradado.
    
- Health checks passam.
    

---

# Testes Sintéticos

Produção poderá executar testes sintéticos seguros.

Exemplos:

- Consultar health.
    
- Criar execução técnica isolada.
    
- Publicar mensagem de teste.
    
- Validar webhook interno.
    
- Verificar storage temporário.
    
- Verificar autenticação de serviço.
    

Não deverão produzir conteúdo real ou efeitos externos perigosos.

---

# Testes de Segurança

Deverão fazer parte da estratégia desde o início.

Categorias:

- Autenticação.
    
- Autorização.
    
- Multi-tenancy.
    
- Input validation.
    
- Injection.
    
- SSRF.
    
- Upload.
    
- Secrets.
    
- Webhooks.
    
- Prompt injection.
    
- Supply chain.
    
- Criptografia.
    

---

# Testes de Autorização

Validar:

- Sem token.
    
- Token inválido.
    
- Token expirado.
    
- Scope ausente.
    
- Role ausente.
    
- Permission ausente.
    
- Tenant incorreto.
    
- Recurso inexistente.
    
- Admin autorizado.
    
- Service account.
    

---

# Testes de IDOR

Tentar acessar recurso de outro tenant utilizando IDs conhecidos.

O sistema deverá retornar resposta segura sem revelar existência indevida.

---

# SQL Injection

Embora EF Core reduza riscos, testar:

- Queries dinâmicas.
    
- Raw SQL.
    
- Filtros.
    
- Ordenação.
    
- Busca.
    
- Relatórios.
    
- Imports.
    

---

# SSRF

Testar URLs maliciosas:

```text
http://localhost
http://127.0.0.1
http://169.254.169.254
file://
ftp://
DNS rebinding
Redirect para rede interna
```

---

# Uploads

Testar:

- Extensão incompatível.
    
- MIME falso.
    
- Arquivo muito grande.
    
- Zip bomb.
    
- Path traversal.
    
- Nome malicioso.
    
- Conteúdo executável.
    
- Malware.
    
- Arquivo corrompido.
    
- Arquivo vazio.
    

---

# Webhooks

Testar:

- Assinatura ausente.
    
- Assinatura inválida.
    
- Timestamp expirado.
    
- Replay.
    
- Payload alterado.
    
- MessageId duplicado.
    
- Content-Type inválido.
    
- Tamanho excessivo.
    
- URL de saída insegura.
    

---

# Prompt Injection

Casos:

- Documento manda ignorar instruções.
    
- Conteúdo solicita segredos.
    
- Conteúdo tenta chamar ferramenta.
    
- Conteúdo tenta acessar outro tenant.
    
- Conteúdo tenta publicar sem aprovação.
    
- Conteúdo usa encoding ou obfuscação.
    

---

# Testes de Secrets

Validar que segredos não aparecem em:

- Logs.
    
- Traces.
    
- Errors.
    
- Responses.
    
- Messages.
    
- Outbox.
    
- Snapshots.
    
- Audit events.
    
- Test reports.
    

---

# SAST

CI deverá executar análise estática.

Possíveis ferramentas:

- CodeQL.
    
- SonarQube.
    
- Semgrep.
    
- Roslyn analyzers.
    
- Microsoft Security Code Analysis.
    

---

# Dependency Scanning

Validar:

- Vulnerabilidades conhecidas.
    
- Pacotes desatualizados.
    
- Licenças proibidas.
    
- Dependências transitivas.
    
- Imagens base.
    
- Ferramentas de build.
    

---

# DAST

Ambientes de teste poderão executar scanners dinâmicos contra a API.

Exemplos:

- OWASP ZAP.
    
- Burp em testes manuais.
    
- Scanners gerenciados.
    

---

# Testes de Resiliência

Devem validar comportamentos sob falha.

Cenários:

- Timeout.
    

- DNS.
    
- Conexão interrompida.
    
- Broker indisponível.
    
- Banco indisponível.
    
- Redis indisponível.
    
- Storage indisponível.
    
- Worker interrompido.
    
- Circuit breaker.
    
- Retry.
    
- Fallback.
    
- Bulkhead.
    

---

# Testes de Retry

Validar:

- Somente erros transitórios.
    
- Máximo de tentativas.
    
- Backoff.
    
- Jitter dentro do limite.
    
- Retry-After.
    
- Cancelamento.
    
- Orçamento total.
    
- Métricas.
    
- Custos.
    

---

# Testes de Timeout

Validar:

- Timeout por tentativa.
    
- Timeout total.
    
- Cancelamento propagado.
    
- Operação externa cancelada.
    
- Estado persistido corretamente.
    
- Erro normalizado.
    
- Ausência de thread bloqueada.
    

---

# Testes de Circuit Breaker

Fluxo:

```text
Dependência falha repetidamente
    ↓
Circuito abre
    ↓
Chamadas são bloqueadas
    ↓
Tempo passa
    ↓
Half-open
    ↓
Chamada de teste
    ↓
Circuito fecha ou reabre
```

---

# Testes de Bulkhead

Validar:

- Limite de concorrência.
    
- Rejeição controlada.
    
- Fila interna limitada.
    
- Um provider não consome toda a capacidade.
    
- Organizações não monopolizam recursos.
    
- Liberação após conclusão.
    

---

# Testes de Fallback

Validar:

- Ordem da policy.
    
- Compatibilidade.
    
- Limite financeiro.
    
- Qualidade mínima.
    
- Registro do motivo.
    
- Ausência de fallback para erro permanente inadequado.
    
- Falha de todos os providers.
    

---

# Chaos Testing

Em estágios mais avançados, injetar falhas controladas.

Exemplos:

- Latência.
    
- Encerramento de container.
    
- Perda de rede.
    
- Falha de DNS.
    
- CPU alta.
    
- Memória limitada.
    
- Disco cheio.
    
- Broker reiniciado.
    
- PostgreSQL reiniciado.
    

---

# Regras de Chaos

- Executar em ambiente controlado.
    
- Possuir hipótese.
    
- Possuir limite de impacto.
    
- Possuir observabilidade.
    
- Possuir rollback.
    
- Não utilizar dados sensíveis.
    
- Não iniciar em produção sem maturidade.
    

---

# Game Days

A equipe poderá realizar exercícios:

- Broker indisponível.
    
- Provider principal fora do ar.
    
- Banco em failover.
    
- DLQ crescendo.
    
- Worker travado.
    
- Credencial revogada.
    
- Storage inconsistente.
    
- Publicação incerta.
    

Objetivo:

- Validar runbooks.
    
- Validar alertas.
    
- Medir recuperação.
    
- Identificar lacunas.
    

---

# Testes de Disaster Recovery

Validar:

- Restore de banco.
    
- Restore de storage.
    
- Recriação de broker.
    
- Reconstrução de projeções.
    
- Recuperação da Outbox.
    
- Reprocessamento de eventos.
    
- Rotação de credenciais.
    
- Recuperação em região alternativa.
    

---

# RPO e RTO

Testes deverão verificar metas definidas.

```text
RPO — perda máxima de dados aceitável
RTO — tempo máximo para recuperação
```

---

# Backup Restore Test

Backup não é confiável sem teste de restore.

Fluxo:

```text
Criar backup
    ↓
Restaurar em ambiente isolado
    ↓
Executar validações
    ↓
Comparar integridade
    ↓
Registrar duração
```

---

# Testes de Performance

Deverão validar capacidade, latência e consumo de recursos.

Tipos:

- Benchmark.
    
- Load test.
    
- Stress test.
    
- Spike test.
    
- Soak test.
    
- Scalability test.
    
- Capacity test.
    

---

# Benchmarks

Utilizar BenchmarkDotNet para:

- Parsers.
    
- Serialização.
    
- Mapeamentos.
    
- Cálculos.
    
- Chunking.
    
- Validação.
    
- Algoritmos críticos.
    

Benchmarks não substituem testes de carga do sistema.

---

# Testes de Carga

Validar:

- Requests por segundo.
    
- Latência.
    
- Erros.
    
- CPU.
    
- Memória.
    
- Pool de conexões.
    
- Fila.
    
- Banco.
    
- Workers.
    
- Providers.
    
- Custos.
    

---

# Cenários de Carga

Exemplos:

- Consulta de projetos.
    
- Criação de pipelines.
    
- Consumo de mensagens.
    
- Processamento de Outbox.
    
- Atualização de status.
    
- Upload.
    
- Notificações em tempo real.
    
- Analytics.
    

---

# Métricas de Latência

Avaliar:

```text
p50
p90
p95
p99
```

Média isolada não será suficiente.

---

# Stress Test

Aumentar carga até:

- Saturação.
    
- Degradação.
    
- Erros.
    
- Backpressure.
    
- Recuperação.
    

Objetivo:

- Conhecer limite.
    
- Validar degradação controlada.
    
- Identificar gargalos.
    

---

# Spike Test

Simular aumento repentino.

Exemplo:

```text
10 req/s
    ↓
500 req/s
    ↓
10 req/s
```

Validar:

- Broker absorve pico.
    
- API mantém estabilidade.
    
- Rate limiting.
    
- Recuperação.
    
- Sem perda de mensagens.
    

---

# Soak Test

Executar carga por longo período.

Detectar:

- Memory leak.
    
- Connection leak.
    
- Crescimento de fila.
    
- Locks.
    
- Degradação gradual.
    
- Acúmulo de Outbox.
    
- Problemas de cache.
    
- Recursos não liberados.
    

---

# Performance do Banco

Validar:

- Query plans.
    
- Índices.
    
- Paginação.
    
- N+1.
    
- Locks.
    
- Deadlocks.
    
- Pool.
    
- Tempo de migrations.
    
- Volume de JSONB.
    
- Crescimento de tabelas.
    

---

# Explain Analyze

Queries críticas deverão ser analisadas com:

```sql
EXPLAIN ANALYZE
```

Principalmente:

- Busca de Outbox.
    
- Inbox.
    
- Pipeline executions.
    
- Analytics.
    
- Reconciliação.
    
- Listagens paginadas.
    
- Jobs agendados.
    

---

# Performance de IA

Medir:

- Time to first token.
    
- Tempo total.
    
- Tokens de entrada.
    
- Tokens de saída.
    
- Custo.
    
- Taxa de retry.
    
- Taxa de fallback.
    
- Schema valid rate.
    
- Concorrência.
    

---

# Orçamento de Performance

Cada fluxo crítico deverá possuir metas.

Exemplo:

```text
Criar pipeline: p95 < 500 ms
Consultar status: p95 < 300 ms
Processar Outbox: atraso < 10 s
Receber webhook: p95 < 1 s
```

Valores finais deverão ser definidos por SLOs.

---

# Testes de Observabilidade

Validar que o sistema produz sinais corretos.

Testar:

- Logs.
    
- Metrics.
    
- Traces.
    
- CorrelationId.
    
- CausationId.
    
- Error codes.
    
- MessageId.
    
- PipelineVersion.
    
- AgentVersion.
    
- Provider.
    
- Custos.
    

---

# Teste de Logging

Validar:

- Evento esperado foi registrado.
    
- Campos obrigatórios existem.
    
- Segredos não existem.
    
- Nível correto.
    
- CorrelationId presente.
    
- Exception preservada internamente.
    
- Mensagem útil.
    

Evitar testar texto exato quando não necessário.

---

# Teste de Tracing

Validar propagação:

```text
HTTP request
    ↓
Application
    ↓
Outbox
    ↓
Broker
    ↓
Worker
    ↓
Provider
```

Os spans deverão permanecer correlacionados.

---

# Teste de Métricas

Validar:

- Incremento correto.
    
- Labels limitadas.
    
- Ausência de cardinalidade explosiva.
    
- Falhas registradas.
    
- Retries registrados.
    
- Custos registrados.
    
- Duração medida.
    

---

# Testes de Health Checks

Validar estados:

- Healthy.
    
- Degraded.
    
- Unhealthy.
    

Dependências obrigatórias e opcionais deverão produzir resultados diferentes.

---

# Testes de Configuração

Validar:

- Binding.
    
- Defaults.
    
- Precedência.
    
- Validation.
    
- ValidateOnStart.
    
- Reload.
    
- Last known good.
    
- Feature flags.
    
- Kill switches.
    
- Snapshot.
    
- Máscara de segredos.
    

---

# Startup Tests

A aplicação deverá falhar ao iniciar com:

- Connection string ausente.
    
- Broker obrigatório ausente.
    
- Configuração inválida.
    
- Secret reference inexistente.
    
- Timeout negativo.
    
- Heartbeat maior que lease.
    
- Provider obrigatório sem configuração.
    

---

# Testes de Feature Flags

Validar:

- Default.
    
- Habilitada.
    
- Desabilitada.
    
- Rollout.
    
- Tenant.
    
- Kill switch.
    
- Mudança em runtime.
    
- Auditoria.
    
- Expiração.
    

---

# Testes de Versionamento

Validar:

- API V1 e V2.
    
- Contratos antigos.
    
- Pipelines antigos.
    
- Steps antigos.
    
- Prompts antigos.
    
- Agents antigos.
    
- Configuração antiga.
    
- Outbox pendente de versão anterior.
    
- Cache versionado.
    
- Migrations compatíveis.
    

---

# Teste de Execução Antiga

Fluxo:

```text
Criar execução em Pipeline v2
Implantar código com Pipeline v3
Retomar execução
Esperar uso de Pipeline v2
```

---

# Teste de Outbox Antiga

Persistir uma `OutboxMessage` V1.

Executar processador atual.

Validar:

- Desserialização.
    
- Publicação.
    
- Routing.
    
- Versão preservada.
    
- Consumer suportado.
    

---

# Testes de Compatibilidade de Deploy

Simular coexistência:

- API antiga e Worker novo.
    
- API nova e Worker antigo.
    
- Producer antigo e Consumer novo.
    
- Producer novo e Consumer preparado.
    
- Banco expandido com aplicação antiga.
    
- Cache antigo com aplicação nova.
    

---

# Testes de Rollback

Validar quando possível:

- Rollback de deploy.
    
- Rollback de feature flag.
    
- Rollback de prompt.
    
- Rollback de pipeline para novas execuções.
    
- Rollback de provider policy.
    
- Compatibilidade com banco.
    

---

# Testes de n8n

Validar:

- Payload enviado.
    
- Assinatura.
    
- Timeout.
    
- Retry.
    
- Idempotência.
    
- Workflow version.
    
- Callback.
    
- Falha.
    
- Resposta inesperada.
    
- Segurança.
    

---

# Workflow Fixtures

Workflows exportados poderão ser validados por:

- JSON schema.
    
- Hash.
    
- Nodes obrigatórios.
    
- Credenciais referenciadas.
    
- Ausência de secrets.
    
- Versão.
    
- Input e output.
    

---

# Testes de Serviço Python

Validar:

- Contrato HTTP ou mensagem.
    
- Job creation.
    
- Job status.
    
- Callback.
    
- Arquivo produzido.
    
- Cancelamento.
    
- Timeout.
    
- Erro.
    
- Idempotência.
    
- Versão do serviço.
    

---

# Testes de Media

Validar:

- Formatos.
    
- Codecs.
    
- Duração.
    
- Resolução.
    
- Áudio.
    
- Legendas.
    
- Sincronização.
    
- Tamanho.
    
- Metadata.
    
- Arquivo corrompido.
    
- Compatibilidade.
    

---

# Golden Media Files

Manter arquivos pequenos de referência para:

- Áudio.
    
- Vídeo.
    
- Imagem.
    
- Legenda.
    
- Thumbnail.
    

Evitar arquivos grandes no repositório principal.

Poderão ficar em storage de testes versionado.

---

# Flaky Tests

Teste flaky é um defeito.

Possíveis causas:

- Tempo real.
    
- Concorrência.
    
- Dependência externa.
    
- Estado compartilhado.
    
- Portas fixas.
    
- Ordem de execução.
    
- Random sem seed.
    
- Esperas frágeis.
    
- Assincronismo incorreto.
    

---

# Política para Flaky Tests

Ao identificar:

1. Registrar.
    
2. Investigar.
    
3. Corrigir.
    
4. Isolar temporariamente apenas se necessário.
    
5. Definir responsável.
    
6. Não ignorar indefinidamente.
    

---

# Evitar Thread.Sleep

Testes assíncronos não deverão depender de sleeps arbitrários.

Preferir:

- Polling com timeout.
    
- Events.
    
- TaskCompletionSource.
    
- Fake clock.
    
- Wait conditions.
    
- Broker acknowledgements.
    
- Hooks de teste.
    

---

# Timeouts de Teste

Todo teste que aguarda sistema externo deverá possuir timeout.

Isso evita pipelines travados.

---

# Paralelismo

Testes poderão executar em paralelo quando isolados.

Desabilitar paralelismo somente para suites que realmente compartilham recurso.

---

# Randomização

Dados aleatórios poderão aumentar cobertura.

Cuidados:

- Registrar seed.
    
- Permitir reprodução.
    
- Limitar complexidade.
    
- Não ocultar intenção do teste.
    

Ferramentas como AutoFixture ou Bogus poderão ser utilizadas.

---

# Property-Based Testing

Poderá ser utilizado para:

- Value Objects.
    
- Parsers.
    
- Serialização.
    
- Estados.
    
- Cálculos.
    
- Idempotência.
    
- Invariantes.
    

Ferramentas possíveis:

- FsCheck.
    
- Hedgehog.
    

---

# Mutation Testing

Mutation testing poderá avaliar a efetividade dos testes.

Ferramentas possíveis:

- Stryker.NET.
    

Aplicar inicialmente em:

- Domain.
    
- Policies críticas.
    
- Validações.
    
- Cálculos.
    
- Result Pattern.
    

---

# Cobertura de Código

Cobertura será um indicador, não objetivo final.

Métricas:

- Line coverage.
    
- Branch coverage.
    
- Method coverage.
    

Branch coverage é especialmente relevante para regras e erros.

---

# Metas de Cobertura

Não deverá existir uma única meta global cega.

Possível orientação:

```text
Domain crítico: cobertura muito alta
Application crítica: cobertura alta
Infrastructure adapters: cobertura combinada com integração
DTOs e composição: menor prioridade
Código gerado: excluído
```

---

# Código que Deve Possuir Alta Cobertura

- Regras de domínio.
    
- Autorização.
    
- Políticas financeiras.
    
- Idempotência.
    
- Transições de pipeline.
    
- Seleção de fallback.
    
- Validação de assinatura.
    
- Versionamento.
    
- Migração de schema.
    
- Compensação.
    

---

# Código que Pode Não Exigir Teste Direto

- Propriedades triviais.
    
- DTOs sem lógica.
    
- Código gerado.
    
- Wiring simples já coberto por integration tests.
    
- Migrations vazias geradas automaticamente, embora o resultado deva ser testado.
    

---

# Qualidade dos Testes

Um teste de qualidade deve:

- Ser legível.
    
- Ter uma razão clara.
    
- Falhar por um motivo.
    
- Ser reproduzível.
    
- Ser independente.
    
- Produzir diagnóstico útil.
    
- Testar comportamento relevante.
    
- Evitar lógica excessiva dentro do teste.
    

---

# Testes como Documentação

Os testes deverão funcionar como exemplos executáveis.

Um novo engenheiro deverá conseguir entender:

- Regras.
    
- Casos de uso.
    
- Fluxos.
    
- Erros.
    
- Contratos.
    
- Estados.
    
- Comportamentos esperados.
    

---

# Organização por Feature

Dentro de cada projeto, preferir organização próxima da aplicação.

Exemplo:

```text
Application.UnitTests/
└── Pipelines/
    ├── StartPipeline/
    ├── CancelPipeline/
    └── RetryPipelineStep/
```

---

# Test Categories

Categorias poderão separar suites:

```text
Unit
Integration
Architecture
Contract
EndToEnd
Performance
Security
AI
Smoke
```

Exemplo:

```csharp
[Trait("Category", "Integration")]
```

---

# Execução Local

Comandos sugeridos:

```bash
dotnet test --filter Category=Unit
```

```bash
dotnet test --filter Category=Integration
```

```bash
dotnet test --filter Category=Architecture
```

---

# Pipeline de Pull Request

Em cada PR executar:

- Build.
    
- Formatting.
    
- Analyzers.
    
- Unit tests.
    
- Architecture tests.
    
- Component tests.
    
- Contract tests.
    
- Integration tests essenciais.
    
- SAST.
    
- Dependency scan.
    
- OpenAPI diff.
    
- Migration validation básica.
    

---

# Pipeline de Main

Após merge executar:

- Todos os testes do PR.
    
- Integração completa.
    
- Mensageria.
    
- Banco.
    
- API.
    
- Worker.
    
- E2E fake.
    
- Container build.
    
- Image scan.
    
- Deploy em ambiente de teste.
    
- Smoke tests.
    

---

# Pipeline Noturno

Executar:

- E2E completo.
    
- Providers sandbox.
    
- AI evaluations.
    
- Prompt regressions.
    
- Performance reduzida.
    
- Soak selecionado.
    
- Security scan ampliado.
    
- Backup restore periódico.
    
- Flaky test detection.
    

---

# Pipeline Pré-Release

Executar:

- Suite completa.
    
- Migrations com snapshot realista.
    
- Contract compatibility.
    
- E2E staging.
    
- Providers reais controlados.
    
- Performance.
    
- Security.
    
- Rollback simulation.
    
- Disaster recovery selecionado.
    
- Aprovação manual quando necessária.
    

---

# Quality Gates

Um deploy poderá ser bloqueado por:

- Build falhando.
    
- Teste crítico falhando.
    
- Breaking contract não aprovado.
    
- Migration incompatível.
    
- Vulnerabilidade crítica.
    
- Regressão de segurança.
    
- Regressão grave de IA.
    
- Performance abaixo do limite.
    
- Smoke test falhando.
    

---

# Falhas Aceitáveis

Algumas avaliações não determinísticas poderão gerar warning em vez de bloqueio imediato.

Exemplo:

- Pequena variação de score.
    
- Latência externa temporária.
    
- Sandbox indisponível.
    
- Avaliação estatística inconclusiva.
    

A política deverá ser explícita.

---

# Test Reports

Os relatórios deverão incluir:

- Testes executados.
    
- Falhas.
    
- Duração.
    
- Flaky tests.
    
- Cobertura.
    
- Mutações.
    
- Vulnerabilidades.
    
- Performance.
    
- Avaliações de IA.
    
- Compatibilidade.
    
- Migrations.
    

---

# Evidências de Release

Cada release poderá armazenar:

- Resultado de testes.
    
- OpenAPI.
    
- AsyncAPI.
    
- Coverage report.
    
- Security report.
    
- Performance report.
    
- Migration report.
    
- AI evaluation report.
    
- Image digest.
    
- Commit.
    

---

# Dados de Teste

Dados de produção não deverão ser copiados diretamente.

Utilizar:

- Dados sintéticos.
    
- Dados anonimizados.
    
- Fixtures.
    
- Geradores.
    
- Subsets autorizados.
    

---

# Dados Pessoais

Testes não deverão conter dados pessoais reais sem base, proteção e necessidade explícita.

---

# Segredos de Teste

Credenciais de sandbox deverão:

- Ficar em Secret Manager.
    
- Possuir menor privilégio.
    
- Ser rotacionadas.
    
- Ter limite financeiro.
    
- Ser separadas de produção.
    
- Não aparecer em logs.
    

---

# Ambientes de Teste

Possíveis ambientes:

```text
Local
CI
Integration
Staging
Performance
Security
Sandbox
```

Cada ambiente deverá possuir objetivo e isolamento claros.

---

# Testes em Produção

Testar em produção não substitui testes anteriores.

Podem existir validações seguras como:

- Feature flags.
    
- Canary.
    
- Shadow traffic.
    
- Synthetic monitoring.
    
- Health checks.
    
- Smoke tests.
    
- Métricas de negócio.
    
- Rollback automático.
    

---

# Canary Validation

Comparar versão canary com estável:

- Erros.
    
- Latência.
    
- Custos.
    
- Qualidade.
    
- Retries.
    
- Fallback.
    
- Consumo de recursos.
    

---

# Shadow Traffic

Requests ou eventos poderão ser duplicados para nova versão sem gerar efeitos externos.

Cuidados:

- Dados.
    
- Custo.
    
- Privacidade.
    
- Idempotência.
    
- Publicação desabilitada.
    
- Storage isolado.
    

---

# Teste de Operações Irreversíveis

Publicações, cobranças e remoções deverão utilizar ambientes sandbox ou adapters fake.

Nunca executar efeito real durante testes automatizados comuns.

---

# Criticidade dos Fluxos

Fluxos poderão ser classificados:

```text
Critical
High
Medium
Low
```

Exemplos críticos:

- Autorização.
    
- Multi-tenancy.
    
- Publicação.
    
- Cobrança.
    
- Idempotência.
    
- Persistência.
    
- Recuperação de pipeline.
    
- Rotação de segredos.
    
- Deleção de dados.
    

---

# Matriz de Testes por Criticidade

## Critical

Exige:

- Unit.
    
- Integration.
    
- E2E.
    
- Security.
    
- Resilience.
    
- Observability.
    
- Manual review quando aplicável.
    

## High

Exige:

- Unit.
    
- Integration.
    
- Contract ou component.
    
- E2E selecionado.
    

## Medium

Exige:

- Unit ou component.
    
- Integration quando houver dependência relevante.
    

## Low

Pode ser coberto por:

- Unit.
    
- Smoke.
    
- Teste exploratório.
    

---

# Definition of Done

Uma feature não estará concluída apenas porque o código foi implementado.

Deverá incluir, quando aplicável:

- Testes unitários.
    
- Testes de integração.
    
- Testes de arquitetura.
    
- Contratos.
    
- Fixtures.
    
- Documentação.
    
- Observabilidade.
    
- Segurança.
    
- Migração.
    
- Resiliência.
    
- Cenários de falha.
    
- Atualização de ADR.
    

---

# Pull Request Checklist

Toda PR deverá responder:

- Qual comportamento foi alterado?
    
- Quais testes protegem esse comportamento?
    
- Existe breaking change?
    
- Existe migration?
    
- Existe novo contrato?
    
- Existe mudança de segurança?
    
- Existe impacto em custos?
    
- Existe impacto em IA?
    
- Existe estratégia de rollback?
    
- Existe observabilidade?
    
- Existe teste de falha?
    

---

# Bug Regression Test

Todo bug relevante deverá gerar um teste que falha antes da correção e passa depois.

Esse teste deverá permanecer na suite.

---

# Incidentes

Incidentes de produção deverão gerar:

- Root cause analysis.
    
- Teste de regressão.
    
- Novo cenário de resiliência.
    
- Ajuste de observabilidade.
    
- Atualização de runbook.
    
- ADR quando houver decisão arquitetural.
    

---

# Testes Exploratórios

Testes manuais continuarão importantes para:

- UX.
    
- Conteúdo.
    
- Qualidade editorial.
    
- Fluxos novos.
    
- Casos inesperados.
    
- Integrações externas.
    
- Operações administrativas.
    
- IA.
    

Eles deverão ser guiados por charters.

---

# Exploratory Testing Charter

Exemplo:

```text
Explorar publicação com token próximo da expiração,
focando em duplicidade, mensagens de erro,
reconciliação e experiência do operador.
```

---

# Bug Bash

Antes de releases importantes, equipes poderão realizar sessões focadas.

Participantes:

- Engenharia.
    
- Produto.
    
- QA.
    
- Segurança.
    
- Operações.
    
- Conteúdo.
    

---

# Testes de Acessibilidade

Interfaces deverão validar:

- Navegação por teclado.
    
- Contraste.
    
- Labels.
    
- Leitores de tela.
    
- Focus.
    
- Mensagens de erro.
    
- Progresso.
    
- Estados de loading.
    

Ferramentas:

- axe.
    
- Lighthouse.
    
- Playwright.
    
- Testes manuais.
    

---

# Testes de Frontend

Embora a arquitetura principal seja backend, o produto deverá utilizar:

- Unit tests.
    
- Component tests.
    
- E2E.
    
- Visual regression.
    
- Accessibility.
    
- Contract tests com API.
    

---

# Visual Regression

Útil para:

- Dashboard.
    
- Timeline.
    
- Status de pipelines.
    
- Modais de aprovação.
    
- Artefatos.
    
- Erros.
    
- Responsividade.
    

---

# Ferramentas .NET Sugeridas

- xUnit.
    
- FluentAssertions.
    
- NSubstitute ou Moq.
    
- AutoFixture.
    
- Bogus.
    
- Testcontainers.
    
- Respawn.
    
- WebApplicationFactory.
    
- WireMock.Net.
    
- NetArchTest ou ArchUnitNET.
    
- Verify.
    
- Stryker.NET.
    
- BenchmarkDotNet.
    
- NBomber ou k6 para carga.
    

A escolha final deverá ser registrada quando gerar impacto relevante.

---

# xUnit

Poderá ser o framework principal por:

- Integração com .NET.
    
- Fixtures.
    
- Paralelismo.
    
- Ecossistema.
    
- Simplicidade.
    

---

# FluentAssertions

Poderá ser utilizado para assertions legíveis.

A equipe deverá acompanhar licenciamento e políticas da versão adotada.

Alternativas poderão ser avaliadas quando necessário.

---

# Verify

Snapshot testing poderá ser útil para:

- JSON.
    
- OpenAPI.
    
- Contracts.
    
- Emails.
    
- Prompts renderizados.
    
- Manifests.
    
- Problem Details.
    

Snapshots deverão ser revisados conscientemente.

---

# Playwright

Poderá ser utilizado para:

- E2E web.
    
- Autenticação.
    
- Fluxos de aprovação.
    
- Dashboards.
    
- Upload.
    
- Realtime.
    
- Acessibilidade.
    

---

# k6

Poderá ser utilizado para:

- Load tests.
    
- Spike.
    
- Soak.
    
- Thresholds.
    
- Integração com CI/CD.
    

---

# Test Isolation Rules

- Nenhum teste depende de outro.
    
- Nenhum teste utiliza dados deixados por outro.
    
- IDs deverão ser únicos.
    
- Relógio deverá ser controlado.
    
- Ambiente externo deverá ser isolado.
    
- Credenciais deverão ser específicas.
    
- Filas deverão ser exclusivas ou limpas.
    
- Storage deverá utilizar prefixos de teste.
    
- Cache deverá utilizar namespace de teste.
    

---

# Test Environment Identification

Todo recurso criado deverá ser identificável.

Exemplo:

```text
test-run-id
test-suite
environment
created-at
```

Isso facilita limpeza.

---

# Limpeza de Recursos

Testes deverão remover:

- Arquivos.
    
- Filas temporárias.
    
- Dados.
    
- Jobs.
    
- Webhook subscriptions.
    
- Artefatos.
    
- Containers.
    

A limpeza deverá acontecer mesmo em falha, quando possível.

---

# Teste de Shutdown Gracioso

Validar:

- API encerra requests.
    
- Worker para de consumir.
    
- Mensagem incompleta não recebe ack.
    
- Lease é liberado ou expira.
    
- Telemetria é exportada.
    
- Conexões são fechadas.
    
- Tempo máximo é respeitado.
    

---

# Teste de Deploy

Simular:

- Rolling update.
    
- Instâncias antigas e novas.
    
- Mensagens durante deploy.
    
- Requests em andamento.
    
- Worker interrompido.
    
- Outbox pendente.
    
- Cache de versão antiga.
    

---

# Teste de Reconciliação

Criar inconsistências controladas:

- Pipeline Running sem lease.
    
- Job externo concluído sem callback.
    
- Arquivo órfão.
    
- Publicação externa sem estado local.
    
- Outbox parada.
    
- Step sem artefato.
    

Executar reconciler e verificar correção.

---

# Teste de Compensação

Validar:

- Ação compensável.
    
- Compensação bem-sucedida.
    
- Compensação idempotente.
    
- Compensação falha.
    
- Retry de compensação.
    
- Estado final.
    
- Auditoria.
    

---

# Testes Financeiros

Como chamadas de IA possuem custo, validar:

- Estimativa.
    
- Registro de custo.
    
- Limite por execução.
    
- Limite por organização.
    
- Retry contabilizado.
    
- Fallback contabilizado.
    
- Operação bloqueada ao atingir limite.
    
- Ausência de duplicidade.
    

---

# Testes de Quotas

Validar:

- Limite diário.
    
- Limite mensal.
    
- Concorrência.
    
- Tokens.
    
- Storage.
    
- Publicações.
    
- Rate limit por tenant.
    
- Reset da janela.
    
- Operação simultânea.
    

---

# Testes de Auditoria

Validar que ações críticas geram eventos de auditoria.

Exemplos:

- Aprovação.
    
- Publicação.
    
- Configuração.
    
- Versão.
    
- Reprocessamento.
    
- Descarte de DLQ.
    
- Alteração de permissão.
    
- Rotação de credencial.
    

A auditoria não deverá conter segredos.

---

# Métricas da Estratégia de Testes

A equipe poderá acompanhar:

```text
test_pass_rate
test_duration
flaky_test_rate
escaped_defects
code_coverage
mutation_score
contract_breaks_detected
migration_failures
security_findings
ai_regression_rate
mean_time_to_fix_tests
```

---

# Métricas de Qualidade de Release

- Defeitos por release.
    
- Rollbacks.
    
- Incidentes.
    
- Falhas de migration.
    
- Regressões de contrato.
    
- Regressões de IA.
    
- Falhas de segurança.
    
- Alterações emergenciais.
    
- Tempo de recuperação.
    

---

# Anti-Padrões

Evitar:

- Testar apenas happy path.
    
- Usar somente mocks.
    
- Usar EF InMemory como prova do banco.
    
- Ignorar testes flaky.
    
- Depender de providers reais em todo commit.
    
- Comparar texto de IA por igualdade exata.
    
- Testes com sleeps longos.
    
- Estado compartilhado.
    
- Dados reais sensíveis.
    
- Cobertura como única meta.
    
- E2E para tudo.
    
- Testar métodos privados.
    
- Snapshots aprovados sem revisão.
    
- Testes desabilitados indefinidamente.
    
- Migrations sem teste.
    
- Contratos sem fixtures.
    
- Bugs corrigidos sem regressão.
    

---

# Regras Arquiteturais

- Toda regra crítica deverá possuir teste automatizado.
    
- Domain deverá possuir testes unitários rápidos.
    
- Application deverá ser testada por casos de uso.
    
- Regras de dependência deverão possuir testes de arquitetura.
    
- Data deverá ser testado com PostgreSQL real.
    
- Infrastructure deverá possuir testes contra stubs ou serviços reais controlados.
    
- Mensageria deverá ser testada com broker real.
    
- Consumers deverão possuir testes de idempotência.
    
- Outbox e Inbox deverão possuir testes de integração.
    
- Contratos deverão possuir fixtures versionadas.
    
- APIs deverão possuir testes de compatibilidade.
    
- Pipelines deverão ser testados como máquinas de estado.
    
- Resume e recuperação deverão possuir testes.
    
- Agents deverão possuir testes determinísticos e avaliações de qualidade.
    
- Prompts publicados deverão possuir regressão.
    
- Structured Outputs deverão possuir validação de schema.
    
- Providers reais não deverão ser obrigatórios em todo commit.
    
- Migrations deverão ser testadas do zero e a partir da versão anterior.
    
- Multi-tenancy deverá possuir testes negativos.
    
- Segurança deverá ser testada em múltiplos níveis.
    
- Falhas transitórias deverão ser simuladas.
    
- Operações críticas deverão possuir testes de idempotência.
    
- Testes deverão ser independentes.
    
- Testes flaky deverão ser tratados como defeitos.
    
- Segredos não deverão aparecer em testes ou relatórios.
    
- Testes de performance deverão possuir thresholds.
    
- Testes de produção deverão ser seguros e sem efeitos perigosos.
    
- Todo bug crítico deverá gerar teste de regressão.
    
- Toda alteração incompatível deverá falhar no CI sem aprovação explícita.
    
- Evidências de teste deverão ser associadas à release.
    

---

# Checklist para Nova Feature

- Quais regras de domínio foram adicionadas?
    
- Existem testes unitários?
    
- Existe integração com banco?
    
- Existe novo contrato?
    
- Existe teste de compatibilidade?
    
- Existe mensageria?
    
- Existe idempotência?
    
- Existe cenário de falha?
    
- Existe autorização?
    
- Existe multi-tenancy?
    
- Existe observabilidade?
    
- Existe impacto de performance?
    
- Existe impacto de IA?
    
- Existe migration?
    
- Existe rollback?
    
- Existe teste end-to-end necessário?
    

---

# Checklist para Novo Endpoint

- Happy path.
    
- Payload inválido.
    
- Sem autenticação.
    
- Sem autorização.
    
- Tenant incorreto.
    
- Recurso inexistente.
    
- Conflito.
    
- Problem Details.
    
- CorrelationId.
    
- Rate limit.
    
- OpenAPI.
    
- Compatibilidade.
    
- Persistência.
    
- Outbox.
    
- Performance.
    

---

# Checklist para Novo Consumer

- Mensagem válida.
    
- Versão inválida.
    
- Payload inválido.
    
- Duplicidade.
    
- Inbox.
    
- Falha transitória.
    
- Falha permanente.
    
- Retry.
    
- DLQ.
    
- Ack.
    
- Idempotência.
    
- CorrelationId.
    
- Shutdown.
    
- Métricas.
    
- Reprocessamento.
    

---

# Checklist para Novo Provider

- Request mapping.
    
- Response mapping.
    
- Error mapping.
    
- Timeout.
    
- Retry.
    
- Rate limit.
    
- Circuit breaker.
    
- Fallback.
    
- Cancellation.
    
- Cost.
    
- Capabilities.
    
- Resposta inválida.
    
- Sandbox.
    
- Segurança.
    
- Observabilidade.
    

---

# Checklist para Novo Pipeline

- Definição válida.
    
- Ordem.
    
- Inputs.
    
- Outputs.
    
- Happy path.
    
- Step failure.
    
- Retry.
    
- Fallback.
    
- Timeout.
    
- Aprovação.
    
- Cancelamento.
    
- Resume.
    
- Compensação.
    
- Idempotência.
    
- Custos.
    
- Versionamento.
    
- Manifest.
    
- Observabilidade.
    

---

# Checklist para Novo Agent

- Entrada.
    
- Saída.
    
- Schema.
    
- Prompt.
    
- Tools.
    
- Permissões.
    
- Fake provider.
    
- Falhas.
    
- Retry.
    
- Fallback.
    
- Segurança.
    
- Custo.
    
- Latência.
    
- Golden dataset.
    
- Avaliação humana.
    
- Regressão.
    

---

# Checklist para Nova Migration

- Banco vazio.
    
- Banco anterior.
    
- Dados existentes.
    
- Locks.
    
- Backfill.
    
- Compatibilidade.
    
- Deploy antigo.
    
- Rollback.
    
- Performance.
    
- Backup.
    
- Observabilidade.
    
- Expand and contract.
    

---

# Decisões Pendentes

As seguintes decisões deverão ser registradas em ADRs:

- Framework principal de testes.
    
- Biblioteca de assertions.
    
- Biblioteca de mocks.
    
- Estratégia de Testcontainers.
    
- Estratégia de limpeza de banco.
    
- Ferramenta de testes de arquitetura.
    
- Estratégia de snapshots.
    
- Ferramenta de mutation testing.
    
- Metas de cobertura.
    
- Ferramenta de contract testing.
    
- Política de providers reais.
    
- Estrutura de golden datasets.
    
- Estratégia de LLM-as-Judge.
    
- Ferramenta de performance.
    
- Ferramenta de E2E.
    
- Política de flaky tests.
    
- Quality gates.
    
- Ambientes de teste.
    
- Política de testes em produção.
    
- Frequência de disaster recovery tests.
    
- Política de retenção de evidências.
    
- Estratégia de testes de n8n.
    
- Estratégia de teste do serviço Python.
    

---

# Exemplo Completo: Pull Request de Nova Etapa

```text
Desenvolvedor cria FactCheckStep v1
    ↓
Adiciona testes unitários de validação
    ↓
Adiciona testes do handler
    ↓
Adiciona teste de arquitetura
    ↓
Adiciona contrato de saída
    ↓
Adiciona fixture JSON
    ↓
Adiciona teste do provider fake
    ↓
Adiciona teste de resposta inválida
    ↓
Adiciona teste de retry
    ↓
Adiciona teste do pipeline com o novo step
    ↓
Adiciona teste de resume
    ↓
Executa regressão do prompt
    ↓
CI valida contratos
    ↓
CI executa PostgreSQL e RabbitMQ
    ↓
Security scan passa
    ↓
PR é aprovada
```

---

# Exemplo Completo: Teste de Publicação Idempotente

```text
PublishContentCommand é publicado
    ↓
Publication Worker recebe
    ↓
Inbox registra MessageId
    ↓
Publicação externa é criada
    ↓
Estado local é persistido
    ↓
Mensagem é confirmada
    ↓
Broker entrega novamente a mesma mensagem
    ↓
Inbox identifica duplicidade
    ↓
Consumer retorna sucesso
    ↓
Provider não é chamado novamente
    ↓
Existe apenas uma publicação externa
```

---

# Exemplo Completo: Teste de Recuperação

```text
Pipeline inicia
    ↓
Research Step conclui
    ↓
Checkpoint é persistido
    ↓
Script Step cria job externo
    ↓
Worker é encerrado
    ↓
Lease expira
    ↓
Novo Worker inicia
    ↓
Reconciliação consulta job externo
    ↓
Job está concluído
    ↓
Resultado é persistido
    ↓
Pipeline continua
    ↓
Research Step não é repetido
    ↓
Job externo não é duplicado
```

---

# Exemplo Completo: Regressão de Prompt

```text
ScriptPrompt v7 é versão estável
    ↓
ScriptPrompt v8 é criado
    ↓
Golden dataset é executado
    ↓
Schema valid rate é comparado
    ↓
Qualidade editorial é avaliada
    ↓
Custo é calculado
    ↓
Latência é medida
    ↓
Casos adversariais são executados
    ↓
v8 apresenta maior custo e pior factualidade
    ↓
Quality gate falha
    ↓
v8 não é promovido
    ↓
v7 permanece estável
```

---

# Exemplo Completo: Migration

```text
Nova migration adiciona configuration_snapshot
    ↓
Teste cria banco na versão anterior
    ↓
Insere pipelines ativos
    ↓
Aplica migration
    ↓
Valida coluna nova
    ↓
Valida dados antigos
    ↓
Inicia aplicação antiga
    ↓
Inicia aplicação nova
    ↓
Executa backfill
    ↓
Valida performance
    ↓
Valida rollback ou forward fix
    ↓
Migration é aprovada
```

---

# Objetivo Final

Criar uma estratégia de testes que permita evoluir o Infinite Content AI com confiança.

O sistema deverá provar continuamente que:

- Suas regras continuam corretas.
    
- Suas camadas permanecem isoladas.
    
- Seus contratos continuam compatíveis.
    
- Seus dados permanecem íntegros.
    
- Suas mensagens não geram efeitos duplicados.
    
- Seus pipelines podem ser retomados.
    
- Seus agentes respeitam regras e limites.
    
- Seus prompts mantêm qualidade.
    
- Suas integrações falham de forma controlada.
    
- Suas migrations são seguras.
    
- Sua segurança permanece ativa.
    
- Sua performance permanece aceitável.
    

Testes não serão uma etapa final.

Eles serão parte da arquitetura, do desenvolvimento, do deploy e da operação do Infinite Content AI.