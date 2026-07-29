# Comunicação entre Componentes

## Objetivo

Definir como os componentes do Infinite Content AI se comunicam.

A comunicação deverá ser simples, explícita, observável e adequada ao tipo de operação.

Nem toda comunicação precisa ser assíncrona.

Nem toda operação deve passar por fila.

A escolha dependerá de duração, criticidade, acoplamento e necessidade de resposta imediata.

---

# Princípios

A comunicação entre componentes deverá seguir estes princípios:

- Baixo acoplamento.
    
- Contratos claros.
    
- Idempotência.
    
- Observabilidade.
    
- Resiliência.
    
- Segurança.
    
- Versionamento.
    
- Tratamento explícito de falhas.
    
- Correlação entre operações.
    

---

# Tipos de Comunicação

O sistema utilizará cinco formas principais de comunicação:

1. Chamada síncrona interna.
    
2. Comunicação HTTP.
    
3. Mensageria assíncrona.
    
4. Eventos.
    
5. Webhooks.
    

O n8n poderá participar de algumas dessas comunicações como ferramenta de automação externa.

---

# Chamada Síncrona Interna

A chamada síncrona interna será utilizada dentro do mesmo processo.

Exemplo:

```text
Endpoint
    ↓
Application Handler
    ↓
Domain
    ↓
Repository Interface
```

Esse tipo de comunicação deverá ser utilizado quando:

- A resposta for necessária imediatamente.
    
- A operação for rápida.
    
- Os componentes estiverem no mesmo processo.
    
- Não houver necessidade de desacoplamento temporal.
    
- A falha precisar ser devolvida ao solicitante.
    

---

# Exemplo de Fluxo Síncrono

```text
POST /content-projects
    ↓
CreateContentProjectEndpoint
    ↓
CreateContentProjectHandler
    ↓
ContentProject.Create()
    ↓
IContentProjectRepository.AddAsync()
    ↓
IUnitOfWork.SaveChangesAsync()
    ↓
Response
```

---

# Quando não usar chamada síncrona

Não utilizar comunicação síncrona para:

- Renderização de vídeo.
    
- Geração de mídia demorada.
    
- Publicação em múltiplas plataformas.
    
- Coleta de métricas.
    
- Execução completa de pipelines.
    
- Processos sujeitos a muitas retentativas.
    
- Operações que podem durar minutos.
    

Essas operações deverão ser assíncronas.

---

# Comunicação HTTP

A comunicação HTTP será utilizada entre sistemas ou processos independentes.

Exemplos:

- Frontend para API.
    
- n8n para API.
    
- API para serviços externos.
    
- Webhook externo para API.
    
- Serviço auxiliar em Python para API.
    
- Aplicação para plataformas de publicação.
    

---

# Regras para HTTP

Toda integração HTTP deverá possuir:

- Timeout.
    
- CancellationToken.
    
- Tratamento de status HTTP.
    
- Retentativa apenas para erros transitórios.
    
- Logs estruturados.
    
- Correlation ID.
    
- Validação de resposta.
    
- Autenticação.
    
- Rate limiting, quando necessário.
    
- Circuit breaker, quando aplicável.
    

---

# HTTP interno

Caso no futuro existam microsserviços, a comunicação HTTP interna deverá ser utilizada apenas quando houver necessidade real de separação de processos.

Inicialmente, o projeto será um monólito modular com processos separados apenas quando necessário.

Não serão criados microsserviços sem justificativa.

---

# Serviço Auxiliar em Python

Python poderá ser utilizado quando alguma biblioteca específica tornar essa escolha vantajosa.

Exemplos:

- Processamento de vídeo.
    
- Modelos locais.
    
- Transcrição.
    
- Manipulação avançada de áudio.
    
- Visão computacional.
    
- Bibliotecas sem equivalente adequado em .NET.
    

A comunicação poderá ocorrer por:

- HTTP.
    
- Fila.
    
- Execução de job.
    
- Armazenamento compartilhado controlado.
    

---

# Exemplo com Serviço Python

```text
Worker .NET
    ↓
Message Queue
    ↓
Python Media Worker
    ↓
Processamento
    ↓
Evento de conclusão
    ↓
Pipeline retomado
```

Para operações simples:

```text
Infrastructure
    ↓
HTTP Client
    ↓
Python Service
    ↓
Response
```

---

# Regra para Python

Python será um detalhe técnico.

As regras de negócio continuarão na aplicação .NET.

O código .NET deverá depender de uma abstração.

Exemplo:

```csharp
public interface IVideoRenderingProvider
{
    Task<Result<VideoRenderResult>> RenderAsync(
        VideoRenderRequest request,
        CancellationToken cancellationToken);
}
```

A implementação poderá chamar um serviço Python.

---

# Mensageria Assíncrona

Mensageria será utilizada quando o processamento não precisar terminar durante a requisição original.

Exemplo:

```text
API
    ↓
Publica mensagem
    ↓
Fila
    ↓
Worker
    ↓
Executa pipeline
```

---

# Quando usar mensageria

Utilizar filas para:

- Execuções longas.
    
- Processamento em background.
    
- Renderização.
    
- Geração de áudio.
    
- Publicação.
    
- Coleta de analytics.
    
- Retentativas.
    
- Comunicação entre processos.
    
- Controle de carga.
    
- Desacoplamento temporal.
    

---

# Benefícios da Mensageria

- A API responde rapidamente.
    
- Processos podem ser retomados.
    
- Workers podem escalar separadamente.
    
- Falhas podem gerar retentativas.
    
- Picos de carga podem ser absorvidos.
    
- Serviços não precisam estar disponíveis ao mesmo tempo.
    
- O processamento fica mais resiliente.
    

---

# Comandos Assíncronos

Um comando representa uma intenção de executar uma ação.

Exemplos:

```text
StartPipelineCommand
RenderVideoCommand
PublishContentCommand
CollectAnalyticsCommand
ResumePipelineCommand
```

Comandos normalmente possuem um consumidor responsável.

---

# Exemplo de Comando

```csharp
public sealed record StartPipelineCommand(
    Guid PipelineExecutionId,
    string CorrelationId);
```

Fluxo:

```text
Application
    ↓
StartPipelineCommand
    ↓
Message Broker
    ↓
Worker
    ↓
PipelineExecutor
```

---

# Eventos de Integração

Um evento representa algo que já aconteceu.

Exemplos:

```text
PipelineStarted
ScriptGenerated
ScriptApproved
VideoRendered
ContentPublished
PipelineCompleted
PipelineFailed
```

Eventos poderão possuir múltiplos consumidores.

---

# Diferença entre Comando e Evento

## Comando

Expressa uma intenção.

```text
PublishContent
```

Alguém deve executar essa ação.

## Evento

Expressa um fato ocorrido.

```text
ContentPublished
```

Outros componentes podem reagir.

---

# Regras para Comandos

- Usar nomes no imperativo.
    
- Possuir um responsável claro.
    
- Não representar fatos passados.
    
- Ser idempotente quando possível.
    
- Possuir identificador.
    
- Possuir Correlation ID.
    
- Ser validado antes da execução.
    

---

# Regras para Eventos

- Usar nomes no passado.
    
- Representar fatos imutáveis.
    
- Não exigir um único consumidor.
    
- Possuir data de ocorrência.
    
- Possuir versão.
    
- Possuir identificador.
    
- Possuir Correlation ID.
    
- Não conter informações sensíveis desnecessárias.
    

---

# Eventos de Domínio

Eventos de domínio representam fatos relevantes dentro do domínio.

Exemplo:

```text
ContentProjectCreated
ContentApproved
PublicationScheduled
```

Eles pertencem ao Domain.

Podem ser utilizados para desacoplar comportamentos internos.

---

# Eventos de Integração

Eventos de integração são publicados para fora do limite da aplicação ou do módulo.

Exemplo:

```text
ContentPublishedIntegrationEvent
PipelineCompletedIntegrationEvent
```

Eles pertencem aos contratos da aplicação.

---

# Evento de Domínio versus Evento de Integração

```text
Domain Event
    ↓
Application Handler
    ↓
Integration Event
    ↓
Message Broker
```

Nem todo evento de domínio precisa virar evento de integração.

A transformação deverá acontecer na Application.

---

# Publicação Confiável de Eventos

Eventos importantes não deverão ser publicados de forma insegura.

Problema:

```text
Salvar no banco
    ↓
Publicar evento
```

Se o banco salvar e a publicação falhar, o sistema fica inconsistente.

---

# Outbox Pattern

O Outbox Pattern será utilizado para eventos críticos.

Fluxo:

```text
Transação
    ├── Salva dados de negócio
    └── Salva evento na Outbox
            ↓
Commit
            ↓
Outbox Processor
            ↓
Message Broker
```

Isso garante que os dados e o evento sejam persistidos juntos.

---

# Estrutura da Outbox

Uma mensagem de Outbox poderá conter:

- Id.
    
- Type.
    
- Payload.
    
- Version.
    
- OccurredAt.
    
- ProcessedAt.
    
- Error.
    
- AttemptCount.
    
- CorrelationId.
    

---

# Inbox Pattern

O Inbox Pattern poderá ser utilizado para evitar processamento duplicado.

Fluxo:

```text
Mensagem recebida
    ↓
Verificar MessageId
    ↓
Já processada?
   / \
 Sim  Não
  |    |
Ignora Processa
       ↓
Registra na Inbox
```

Esse padrão será especialmente importante para:

- Publicações.
    
- Webhooks.
    
- Processamentos financeiros.
    
- Eventos externos.
    
- Operações não reversíveis.
    

---

# Idempotência

Toda mensagem deverá possuir um identificador único.

Exemplo:

```text
MessageId
CorrelationId
CausationId
```

## MessageId

Identifica a mensagem atual.

## CorrelationId

Agrupa todas as operações de um mesmo fluxo.

## CausationId

Identifica a mensagem ou ação que causou a mensagem atual.

---

# Exemplo de Correlação

```text
HTTP Request
CorrelationId: ABC
    ↓
StartPipelineCommand
CorrelationId: ABC
    ↓
ScriptGeneratedEvent
CorrelationId: ABC
    ↓
PublishContentCommand
CorrelationId: ABC
```

Isso permite rastrear todo o fluxo.

---

# Message Envelope

Mensagens poderão utilizar um envelope comum.

Exemplo conceitual:

```csharp
public sealed record MessageEnvelope<T>(
    Guid MessageId,
    string CorrelationId,
    string? CausationId,
    int Version,
    DateTimeOffset OccurredAt,
    T Payload);
```

---

# Contratos

Contratos de mensagens compartilhadas ficarão em:

```text
InfiniteContent.Contracts
```

Exemplo:

```text
Contracts/
├── Commands/
├── Events/
├── Webhooks/
└── Common/
```

---

# Versionamento de Mensagens

Toda mensagem pública deverá possuir versão.

Exemplo:

```text
PipelineCompletedEventV1
PipelineCompletedEventV2
```

Ou:

```json
{
  "type": "pipeline.completed",
  "version": 1
}
```

Mudanças incompatíveis exigirão nova versão.

---

# Compatibilidade

Sempre que possível:

- Novos campos devem ser opcionais.
    
- Campos antigos não devem ser removidos imediatamente.
    
- Consumidores devem ignorar campos desconhecidos.
    
- Mudanças devem ser documentadas.
    
- Contratos antigos devem possuir período de transição.
    

---

# Message Broker

A tecnologia será definida posteriormente por ADR.

Opções possíveis:

- RabbitMQ.
    
- Azure Service Bus.
    
- AWS SQS.
    
- Kafka.
    
- Redis Streams.
    

A escolha dependerá de:

- Custo.
    
- Facilidade operacional.
    
- Garantias necessárias.
    
- Volume.
    
- Experiência da equipe.
    
- Infraestrutura escolhida.
    

---

# Regra de Abstração

A Application não conhecerá diretamente a tecnologia de mensageria.

Exemplo:

```csharp
public interface IMessagePublisher
{
    Task PublishAsync<T>(
        T message,
        CancellationToken cancellationToken);
}
```

A implementação ficará em Infrastructure.

---

# Consumidores

Consumidores poderão ficar no Worker.

Exemplo:

```text
Worker/
└── Consumers/
    ├── StartPipelineCommandConsumer.cs
    ├── RenderVideoCommandConsumer.cs
    ├── PublishContentCommandConsumer.cs
    └── CollectAnalyticsCommandConsumer.cs
```

O consumidor deverá:

1. Validar a mensagem.
    
2. Criar escopo.
    
3. Chamar a Application.
    
4. Confirmar processamento.
    
5. Tratar falhas.
    
6. Registrar logs.
    

---

# O que um consumidor não deve fazer

Um consumidor não deve:

- Implementar regra de negócio.
    
- Acessar DbContext diretamente.
    
- Chamar provider externo diretamente.
    
- Orquestrar manualmente o domínio.
    
- Duplicar lógica da Application.
    

---

# Dead Letter Queue

Mensagens que falharem repetidamente deverão ir para uma Dead Letter Queue.

Exemplos de causas:

- Payload inválido.
    
- Erro permanente.
    
- Número máximo de tentativas.
    
- Contrato incompatível.
    
- Dependência ausente.
    

A DLQ deverá permitir:

- Inspeção.
    
- Correção.
    
- Reprocessamento manual.
    
- Auditoria.
    
- Alertas.
    

---

# Retentativas

Retentativas deverão utilizar backoff.

Exemplo:

```text
Tentativa 1: imediata
Tentativa 2: após 5 segundos
Tentativa 3: após 30 segundos
Tentativa 4: após 2 minutos
```

Erros permanentes não deverão gerar retentativas inúteis.

---

# Ordem de Mensagens

O sistema não deverá assumir ordem global entre mensagens.

Quando a ordem for necessária, deverá ser controlada por:

- AggregateId.
    
- SessionId.
    
- Partition Key.
    
- Sequência.
    
- Estado persistido.
    
- Verificação de versão.
    

---

# Concorrência

Dois Workers poderão tentar processar a mesma execução.

O sistema deverá utilizar estratégias como:

- Lock distribuído.
    
- Controle otimista de concorrência.
    
- Versionamento de registro.
    
- Chave de idempotência.
    
- Particionamento.
    
- Lease temporária.
    

---

# Controle Otimista

Entidades críticas poderão possuir uma versão.

Exemplo:

```text
PipelineExecution
Version: 12
```

Uma atualização deverá falhar quando tentar salvar uma versão antiga.

---

# Webhooks

Webhooks serão utilizados para receber ou enviar notificações entre sistemas.

Exemplos:

- Plataforma informa que um vídeo foi processado.
    
- Provider informa que um job foi concluído.
    
- Aplicação informa o n8n sobre uma conclusão.
    
- n8n solicita o início de uma execução.
    
- Serviço Python informa que o vídeo foi renderizado.
    

---

# Webhooks de Entrada

Fluxo:

```text
Sistema Externo
    ↓
Webhook Endpoint
    ↓
Validação
    ↓
Application
    ↓
Persistência
    ↓
Evento ou comando
```

---

# Regras para Webhooks de Entrada

Todo webhook recebido deverá:

- Validar assinatura.
    
- Validar timestamp.
    
- Evitar replay.
    
- Validar schema.
    
- Registrar MessageId.
    
- Ser idempotente.
    
- Responder rapidamente.
    
- Processar tarefas longas de forma assíncrona.
    
- Registrar logs.
    
- Proteger dados sensíveis.
    

---

# Resposta do Webhook

O endpoint deverá confirmar o recebimento rapidamente.

Exemplo:

```text
202 Accepted
```

O processamento pesado deverá continuar em background.

---

# Webhooks de Saída

A aplicação poderá enviar eventos para:

- n8n.
    
- Sistemas de notificação.
    
- Clientes externos.
    
- Serviços auxiliares.
    
- Integrações futuras.
    

---

# Regras para Webhooks de Saída

- Assinar payload.
    
- Possuir timeout.
    
- Possuir retentativas.
    
- Registrar tentativas.
    
- Utilizar idempotency key.
    
- Tratar respostas não bem-sucedidas.
    
- Possuir DLQ ou mecanismo equivalente.
    
- Permitir desativação.
    

---

# Papel do n8n

O n8n será utilizado como ferramenta de automação e integração.

Ele poderá:

- Agendar execuções.
    
- Chamar endpoints.
    
- Receber webhooks.
    
- Enviar notificações.
    
- Integrar serviços administrativos.
    
- Disparar rotinas auxiliares.
    
- Coordenar processos externos.
    

---

# O que o n8n não deve controlar

O n8n não deverá ser responsável por:

- Estado oficial do pipeline.
    
- Regras centrais do domínio.
    
- Controle de consistência.
    
- Idempotência principal.
    
- Regras de aprovação.
    
- Persistência oficial.
    
- Decisões críticas de negócio.
    

A aplicação será a fonte de verdade.

---

# Comunicação com n8n

## n8n iniciando um processo

```text
Cron no n8n
    ↓
POST /pipeline-executions
    ↓
Application cria execução
    ↓
Fila
    ↓
Worker
```

## Aplicação notificando o n8n

```text
PipelineCompleted
    ↓
Outbox
    ↓
Webhook Publisher
    ↓
n8n
    ↓
Notificação
```

---

# Frontend

O frontend se comunicará com a API por HTTP.

Para atualizações em tempo real, poderão ser utilizados:

- Polling.
    
- Server-Sent Events.
    
- WebSockets.
    
- SignalR.
    

A escolha será feita conforme a necessidade.

---

# Atualização de Progresso

Inicialmente, o frontend poderá consultar:

```text
GET /pipeline-executions/{id}
```

Resposta:

```json
{
  "status": "Running",
  "currentStep": "GenerateScript",
  "progress": 35
}
```

No futuro, SignalR poderá enviar atualizações em tempo real.

---

# Uploads e Arquivos

Arquivos grandes não deverão trafegar desnecessariamente pela API.

Fluxo preferencial:

```text
Frontend
    ↓
Solicita URL de upload
    ↓
API gera URL temporária
    ↓
Frontend envia direto para Storage
    ↓
API registra metadados
```

Esse modelo reduz carga na aplicação.

---

# Comunicação com Storage

A Application deverá depender de uma abstração.

Exemplo:

```csharp
public interface IFileStorage
{
    Task<FileReference> SaveAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken);
}
```

A implementação ficará na Infrastructure.

---

# Banco de Dados

A comunicação com o banco ocorrerá apenas através do projeto Data.

Fluxo:

```text
Application
    ↓
Repository Interface
    ↓
Data Repository
    ↓
DbContext
    ↓
PostgreSQL
```

API, Worker e Infrastructure não deverão acessar o banco diretamente.

---

# Transações

Transações serão utilizadas dentro de limites claros.

Uma transação não deverá permanecer aberta enquanto:

- Uma IA responde.
    
- Um vídeo é renderizado.
    
- Um webhook é enviado.
    
- Uma API externa é chamada.
    
- Uma fila é consumida.
    

Transações devem ser curtas.

---

# Consistência

Dentro de uma transação local, será utilizada consistência forte.

Entre processos e integrações externas, o sistema aceitará consistência eventual.

Exemplo:

```text
Pipeline concluído no banco
    ↓
Evento enviado alguns segundos depois
    ↓
n8n recebe notificação
```

---

# Falhas Parciais

O sistema deverá assumir que integrações podem falhar.

Exemplo:

```text
Conteúdo salvo
    ↓
Publicação falhou
```

Nesse caso:

- O conteúdo permanece salvo.
    
- A publicação fica com status de falha.
    
- Uma retentativa pode ser agendada.
    
- O histórico é preservado.
    
- O usuário pode intervir.
    

---

# Observabilidade

Toda comunicação deverá gerar dados de observabilidade.

Campos importantes:

- MessageId.
    
- CorrelationId.
    
- CausationId.
    
- Sender.
    
- Receiver.
    
- Operation.
    
- Attempt.
    
- Duration.
    
- Status.
    
- ErrorCode.
    
- Timestamp.
    

---

# Logs

Exemplo conceitual:

```text
Message RenderVideoCommand received
MessageId: 123
CorrelationId: ABC
Attempt: 2
PipelineExecutionId: XYZ
```

Logs não deverão expor:

- Tokens de acesso.
    
- Segredos.
    
- Payloads sensíveis.
    
- Dados pessoais desnecessários.
    

---

# Métricas

Métricas de comunicação:

- Mensagens publicadas.
    
- Mensagens processadas.
    
- Mensagens com falha.
    
- Tamanho das filas.
    
- Tempo médio de processamento.
    
- Tempo de espera.
    
- Número de retentativas.
    
- Mensagens na DLQ.
    
- Webhooks enviados.
    
- Webhooks com falha.
    
- Latência HTTP.
    
- Taxa de timeout.
    

---

# Segurança

Toda comunicação externa deverá utilizar:

- HTTPS.
    
- Autenticação.
    
- Autorização.
    
- Assinatura, quando aplicável.
    
- Rotação de segredos.
    
- Rate limiting.
    
- Validação de entrada.
    
- Proteção contra replay.
    
- Controle de acesso.
    

---

# Resumo de Decisão

## Utilizar chamada direta quando

- A operação for local.
    
- A resposta for imediata.
    
- A duração for curta.
    
- O solicitante precisar do resultado.
    

## Utilizar fila quando

- A operação for longa.
    
- Puder ser executada depois.
    
- Precisar de retentativas.
    
- Precisar escalar separadamente.
    
- Houver risco de indisponibilidade externa.
    

## Utilizar evento quando

- Algo já aconteceu.
    
- Vários componentes puderem reagir.
    
- Houver necessidade de desacoplamento.
    

## Utilizar webhook quando

- A comunicação envolver sistemas externos.
    
- Um sistema precisar notificar outro.
    
- A integração não compartilhar o mesmo broker.
    

## Utilizar n8n quando

- A necessidade for automação externa.
    
- Houver agendamento.
    
- Houver integração administrativa.
    
- A regra não fizer parte do núcleo do negócio.
    

---

# Matriz de Comunicação

|Origem|Destino|Tipo recomendado|
|---|---|---|
|Frontend|API|HTTP|
|API|Application|Chamada interna|
|Application|Domain|Chamada interna|
|Application|Data|Interface|
|Application|Infrastructure|Interface|
|API|Worker|Fila|
|Worker|Application|Chamada interna|
|Application|n8n|Webhook ou evento|
|n8n|API|HTTP|
|Serviço externo|API|Webhook|
|.NET|Serviço Python|HTTP ou fila|
|Pipeline|Frontend|Polling, SSE ou SignalR|
|Application|Message Broker|Evento ou comando|
|Worker|Storage|Interface|

---

# Regras Arquiteturais

- API não chama Worker diretamente.
    
- API publica comandos para processos longos.
    
- Worker chama casos de uso da Application.
    
- Application não conhece o broker concreto.
    
- Application não conhece clientes HTTP concretos.
    
- Eventos críticos utilizam Outbox.
    
- Consumidores importantes utilizam idempotência.
    
- Webhooks recebidos devem ser validados.
    
- Mensagens públicas devem ser versionadas.
    
- Toda comunicação deve possuir Correlation ID.
    
- Operações externas devem possuir timeout.
    
- Transações não podem envolver chamadas externas longas.
    
- n8n não será fonte de verdade.
    
- Falhas externas devem ser tratadas como esperadas.
    
- Comunicação assíncrona deve suportar reprocessamento.
    
- Mensagens com falha permanente devem ir para DLQ.
    

---

# Exemplo Completo

```text
Usuário solicita um vídeo
    ↓
Frontend envia HTTP para API
    ↓
API chama Application
    ↓
Application cria PipelineExecution
    ↓
Data salva execução e mensagem de Outbox
    ↓
Outbox publica StartPipelineCommand
    ↓
Worker consome a mensagem
    ↓
Worker chama PipelineExecutor
    ↓
Pipeline executa agentes
    ↓
Provider externo gera conteúdo
    ↓
Estado é persistido após cada etapa
    ↓
PipelineCompletedEvent vai para Outbox
    ↓
Evento é publicado
    ↓
Webhook é enviado ao n8n
    ↓
n8n envia uma notificação
    ↓
Frontend consulta o status da execução
```

---

# Objetivo Final

Criar uma comunicação previsível, resiliente e observável entre todos os componentes.

Cada interação deverá utilizar o mecanismo mais simples que resolva o problema sem criar acoplamento desnecessário.