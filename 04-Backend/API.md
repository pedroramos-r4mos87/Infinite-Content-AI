# API

## 1. Objetivo

O projeto `Api` será o host HTTP do Infinite Content AI.

Sua responsabilidade será expor as capacidades da plataforma por meio de endpoints seguros, versionados, observáveis e consistentes.

A API deverá:

- Receber requisições HTTP.
    
- Autenticar o usuário.
    
- Resolver o contexto da Organization.
    
- Aplicar autorização HTTP.
    
- Validar contratos de entrada.
    
- Converter requests em Commands ou Queries.
    
- Invocar a Application.
    
- Converter Results em respostas HTTP.
    
- Expor documentação OpenAPI.
    
- Aplicar rate limiting.
    
- Produzir logs, métricas e traces.
    
- Expor health checks.
    
- Proteger os limites externos da aplicação.
    

A API não deverá conter regras centrais de negócio.

Fluxo principal:

```text
Cliente
   ↓
HTTP Request
   ↓
Middleware Pipeline
   ↓
Endpoint
   ↓
Command ou Query
   ↓
Application
   ↓
Result
   ↓
HTTP Response
```

---

# 2. Responsabilidades

O projeto `Api` será responsável por:

- Minimal APIs.
    
- Rotas.
    
- Contratos HTTP.
    
- Autenticação.
    
- Autorização HTTP.
    
- Versionamento da API.
    
- Problem Details.
    
- Serialização JSON.
    
- Middlewares.
    
- Filtros de endpoint.
    
- OpenAPI.
    
- Rate limiting.
    
- CORS.
    
- Uploads HTTP.
    
- Webhooks recebidos.
    
- Health checks.
    
- Composição de dependências.
    
- Configuração do host.
    
- Tratamento global de exceções.
    
- Correlação de requisições.
    
- Telemetria HTTP.
    
- Proteções contra abuso.
    

O projeto não será responsável por:

- Regras de negócio.
    
- Entidades.
    
- Persistência direta.
    
- DbContext em endpoints.
    
- Queries SQL.
    
- Publicação direta em RabbitMQ.
    
- Chamadas diretas a providers de IA.
    
- Acesso direto ao Redis.
    
- Acesso direto ao Azure Blob Storage.
    
- Execução de pipelines longos.
    
- Controle do ciclo de vida dos Workers.
    

---

# 3. Dependências

A API poderá depender de:

```text
Application
Contracts
SharedKernel
Data
Infrastructure
```

As dependências de `Data` e `Infrastructure` deverão existir somente para:

- Registro de serviços.
    
- Configuração do host.
    
- Execução de migrations controladas.
    
- Health checks.
    
- Composição de dependências.
    

Endpoints não deverão consumir implementações concretas de Data ou Infrastructure.

Exemplo permitido no bootstrap:

```csharp
builder.Services
    .AddApplication()
    .AddData(builder.Configuration)
    .AddInfrastructure(builder.Configuration);
```

Exemplo proibido em endpoint:

```csharp
app.MapGet("/projects", async (
    ApplicationDbContext dbContext) =>
{
    return await dbContext.Projects.ToListAsync();
});
```

---

# 4. Estilo de API

O Infinite Content AI utilizará **Minimal APIs organizadas por feature** como padrão inicial.

Motivos:

- Menor cerimônia.
    
- Organização próxima aos casos de uso.
    
- Composição explícita.
    
- Boa integração com OpenAPI.
    
- Facilidade para endpoints pequenos.
    
- Alinhamento com Vertical Slice.
    
- Menor necessidade de controllers genéricos.
    

Controllers poderão ser utilizados futuramente quando houver benefício concreto, como:

- Integrações que exijam filtros específicos.
    
- Compatibilidade com bibliotecas.
    
- Endpoints muito complexos de model binding.
    
- Convenções herdadas de sistemas externos.
    

A solution não deverá misturar estilos sem uma justificativa clara.

---

# 5. Estrutura do Projeto

```text
Api
│
├── Common
│   ├── Authentication
│   ├── Authorization
│   ├── Contracts
│   ├── Errors
│   ├── Extensions
│   ├── Filters
│   ├── HealthChecks
│   ├── Middleware
│   ├── OpenApi
│   ├── RateLimiting
│   ├── Security
│   ├── Serialization
│   └── Versioning
│
├── Features
│   ├── Projects
│   ├── Pipelines
│   ├── Executions
│   ├── Artifacts
│   ├── Approvals
│   ├── Publications
│   ├── Organizations
│   └── Identity
│
├── DependencyInjection.cs
├── EndpointRegistration.cs
├── Program.cs
└── appsettings.json
```

Para o MVP:

```text
Api
└── Features
    ├── Projects
    │   ├── CreateProject
    │   ├── GetProject
    │   └── ListProjects
    │
    ├── Pipelines
    │   ├── CreatePipeline
    │   ├── AddPipelineStep
    │   ├── PublishPipeline
    │   └── GetPipeline
    │
    ├── Executions
    │   ├── StartExecution
    │   ├── GetExecution
    │   └── CancelExecution
    │
    └── Artifacts
        ├── GetArtifact
        └── ListExecutionArtifacts
```

---

# 6. Organização de uma Feature HTTP

Exemplo:

```text
Api
└── Features
    └── Projects
        └── CreateProject
            ├── CreateProjectEndpoint.cs
            ├── CreateProjectRequest.cs
            ├── CreateProjectResponse.cs
            └── CreateProjectExamples.cs
```

Nem toda feature precisará de todos esses arquivos.

Uma operação simples poderá conter:

```text
CreateProject
├── CreateProjectEndpoint.cs
├── CreateProjectRequest.cs
└── CreateProjectResponse.cs
```

---

# 7. Rotas

As rotas deverão ser:

- Baseadas em recursos.
    
- Previsíveis.
    
- Consistentes.
    
- Versionadas.
    
- Preferencialmente no plural.
    
- Independentes de detalhes internos.
    

Prefixo inicial:

```text
/api/v1
```

Exemplos:

```text
POST   /api/v1/projects
GET    /api/v1/projects
GET    /api/v1/projects/{projectId}
PATCH  /api/v1/projects/{projectId}
POST   /api/v1/projects/{projectId}/archive

POST   /api/v1/projects/{projectId}/pipelines
GET    /api/v1/projects/{projectId}/pipelines
GET    /api/v1/pipelines/{pipelineId}
POST   /api/v1/pipelines/{pipelineId}/steps
POST   /api/v1/pipelines/{pipelineId}/publish

POST   /api/v1/pipelines/{pipelineId}/executions
GET    /api/v1/executions/{executionId}
POST   /api/v1/executions/{executionId}/cancel

GET    /api/v1/executions/{executionId}/artifacts
GET    /api/v1/artifacts/{artifactId}
```

---

# 8. Verbos HTTP

Convenções:

|Verbo|Uso|
|---|---|
|`GET`|Consultar recursos|
|`POST`|Criar recursos ou executar ações|
|`PUT`|Substituir completamente um recurso|
|`PATCH`|Alterar parcialmente um recurso|
|`DELETE`|Excluir quando a exclusão fizer sentido|

Ações de domínio que não são CRUD poderão utilizar sub-recursos ou operações explícitas.

Exemplos:

```text
POST /pipelines/{pipelineId}/publish
POST /executions/{executionId}/cancel
POST /artifacts/{artifactId}/approve
```

Evitar:

```text
POST /api/v1/doEverything
POST /api/v1/process
POST /api/v1/manageProject
```

---

# 9. Contratos HTTP

Requests e Responses deverão permanecer no projeto `Api`.

Exemplo:

```csharp
public sealed record CreateProjectRequest(
    string Name,
    string? Description);
```

```csharp
public sealed record CreateProjectResponse(
    Guid Id,
    string Name,
    string Status,
    DateTimeOffset CreatedAt);
```

Contratos HTTP não deverão ser reutilizados como:

- Entidades.
    
- Commands distribuídos.
    
- Integration Events.
    
- Modelos de persistência.
    
- Objetos de providers.
    
- Contratos internos da Application.
    

Cada limite deverá possuir seus próprios modelos.

---

# 10. Request, Command e Entity

Fluxo correto:

```text
CreateProjectRequest
   ↓
CreateProjectCommand
   ↓
Project
```

Cada objeto possui finalidade distinta.

## Request

Representa o contrato HTTP público.

## Command

Representa a intenção da Application.

## Entity

Representa o conceito do Domain.

Evitar utilizar um único objeto atravessando todas as camadas.

---

# 11. Endpoint de Criação

Exemplo:

```csharp
public static class CreateProjectEndpoint
{
    public static IEndpointRouteBuilder MapCreateProject(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/api/v1/projects",
            HandleAsync)
            .WithName("CreateProject")
            .WithTags("Projects")
            .WithSummary("Cria um novo projeto de conteúdo.")
            .Produces<CreateProjectResponse>(
                StatusCodes.Status201Created)
            .ProducesProblem(
                StatusCodes.Status400BadRequest)
            .ProducesProblem(
                StatusCodes.Status401Unauthorized)
            .ProducesProblem(
                StatusCodes.Status403Forbidden)
            .ProducesProblem(
                StatusCodes.Status409Conflict)
            .RequireAuthorization();

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        CreateProjectRequest request,
        ICurrentUser currentUser,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateProjectCommand(
            currentUser.OrganizationId,
            currentUser.UserId,
            request.Name,
            request.Description);

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.Match(
            project => Results.Created(
                $"/api/v1/projects/{project.ProjectId.Value}",
                new CreateProjectResponse(
                    project.ProjectId.Value,
                    project.Name,
                    "active",
                    project.CreatedAt)),
            ApiResults.Problem);
    }
}
```

---

# 12. Endpoint de Consulta

```csharp
public static class GetProjectEndpoint
{
    public static IEndpointRouteBuilder MapGetProject(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            "/api/v1/projects/{projectId:guid}",
            HandleAsync)
            .WithName("GetProject")
            .WithTags("Projects")
            .Produces<GetProjectResponse>()
            .ProducesProblem(
                StatusCodes.Status404NotFound)
            .RequireAuthorization();

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        Guid projectId,
        ICurrentUser currentUser,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetProjectQuery(
            currentUser.OrganizationId,
            new ProjectId(projectId));

        var result = await sender.Send(
            query,
            cancellationToken);

        return result.Match(
            project => Results.Ok(
                new GetProjectResponse(
                    project.Id.Value,
                    project.Name,
                    project.Description,
                    project.Status,
                    project.CreatedAt)),
            ApiResults.Problem);
    }
}
```

---

# 13. Registro de Endpoints

Cada módulo deverá registrar seus endpoints explicitamente.

```csharp
public static class ProjectEndpoints
{
    public static IEndpointRouteBuilder MapProjectEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapCreateProject();
        endpoints.MapGetProject();
        endpoints.MapListProjects();
        endpoints.MapArchiveProject();

        return endpoints;
    }
}
```

Registro global:

```csharp
public static class EndpointRegistration
{
    public static IEndpointRouteBuilder MapApplicationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapProjectEndpoints();
        endpoints.MapPipelineEndpoints();
        endpoints.MapExecutionEndpoints();
        endpoints.MapArtifactEndpoints();

        return endpoints;
    }
}
```

---

# 14. Route Groups

Route Groups poderão ser utilizados para reduzir repetição.

```csharp
var projects = app
    .MapGroup("/api/v1/projects")
    .WithTags("Projects")
    .RequireAuthorization();

projects.MapPost("/", CreateProjectEndpoint.HandleAsync);
projects.MapGet("/", ListProjectsEndpoint.HandleAsync);
projects.MapGet("/{projectId:guid}", GetProjectEndpoint.HandleAsync);
```

O agrupamento não deverá esconder a localização dos endpoints.

---

# 15. Versionamento

A API utilizará versionamento por URL:

```text
/api/v1
/api/v2
```

Motivos:

- Fácil visualização.
    
- Fácil roteamento.
    
- Compatibilidade com clientes.
    
- Simplicidade operacional.
    
- Clareza em logs e métricas.
    

Uma nova versão principal será criada quando houver mudança incompatível.

Exemplos de mudanças incompatíveis:

- Remoção de campo obrigatório.
    
- Mudança de significado.
    
- Mudança de tipo.
    
- Alteração de status code.
    
- Mudança de estrutura.
    
- Remoção de endpoint.
    
- Alteração incompatível de autenticação.
    

Campos opcionais poderão ser adicionados sem nova versão quando não quebrarem clientes existentes.

---

# 16. Compatibilidade

Durante a evolução da API:

- Campos existentes não deverão mudar de significado.
    
- Enums públicos deverão evoluir cuidadosamente.
    
- Campos novos deverão ser opcionais quando possível.
    
- Clientes não deverão depender da ordem dos campos JSON.
    
- Respostas não deverão expor propriedades internas acidentalmente.
    
- Versões antigas deverão possuir prazo de depreciação.
    
- Depreciações deverão ser documentadas.
    

---

# 17. Serialização JSON

Convenções:

- `camelCase`.
    
- Datas em formato ISO 8601.
    
- Datas internas em UTC.
    
- Enums como strings nos contratos públicos.
    
- Propriedades nulas omitidas quando apropriado.
    
- Números monetários com representação segura.
    
- Identificadores como strings ou GUIDs.
    
- JSON consistente entre endpoints.
    

Configuração conceitual:

```csharp
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy =
        JsonNamingPolicy.CamelCase;

    options.SerializerOptions.DefaultIgnoreCondition =
        JsonIgnoreCondition.WhenWritingNull;

    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter(
            JsonNamingPolicy.CamelCase));
});
```

---

# 18. Enums Públicos

Enums internos não deverão ser expostos automaticamente.

Exemplo interno:

```csharp
PipelineExecutionStatus.AwaitingApproval
```

Resposta pública:

```json
{
  "status": "awaitingApproval"
}
```

Mapeamento explícito evita:

- Vazamento de valores numéricos.
    
- Alterações acidentais.
    
- Dependência entre contrato e Domain.
    
- Quebra de compatibilidade.
    

---

# 19. Datas e Horários

Respostas deverão utilizar `DateTimeOffset`.

Exemplo:

```json
{
  "createdAt": "2026-07-23T14:30:00Z"
}
```

A API não deverá retornar datas locais sem offset.

Entradas com datas deverão exigir offset explícito quando representarem instantes.

---

# 20. Identificadores

Identificadores serão enviados como GUIDs.

Exemplo:

```json
{
  "id": "019c1234-5678-7abc-9123-456789abcdef"
}
```

A API poderá utilizar UUID v7 internamente.

Clientes deverão tratar identificadores como valores opacos.

Não deverão inferir:

- Ordem.
    
- Data.
    
- Tenant.
    
- Tipo de entidade.
    

---

# 21. Códigos de Status

Convenções principais:

|Status|Uso|
|---|---|
|`200 OK`|Consulta ou ação concluída|
|`201 Created`|Recurso criado|
|`202 Accepted`|Operação assíncrona aceita|
|`204 No Content`|Ação concluída sem corpo|
|`400 Bad Request`|Entrada inválida|
|`401 Unauthorized`|Ausência ou falha de autenticação|
|`403 Forbidden`|Usuário autenticado sem permissão|
|`404 Not Found`|Recurso não encontrado|
|`409 Conflict`|Conflito de estado, idempotência ou concorrência|
|`412 Precondition Failed`|Pré-condição de versão não atendida|
|`413 Content Too Large`|Payload ou arquivo muito grande|
|`415 Unsupported Media Type`|Tipo de conteúdo não suportado|
|`422 Unprocessable Content`|Contrato válido, mas semanticamente inválido|
|`429 Too Many Requests`|Limite excedido|
|`500 Internal Server Error`|Falha inesperada|
|`502 Bad Gateway`|Dependência externa retornou falha inválida|
|`503 Service Unavailable`|Serviço temporariamente indisponível|
|`504 Gateway Timeout`|Timeout de dependência|

O uso deverá ser consistente em toda a API.

---

# 22. Operações Assíncronas

Operações longas deverão retornar `202 Accepted`.

Exemplo:

```text
POST /api/v1/pipelines/{pipelineId}/executions
```

Resposta:

```http
HTTP/1.1 202 Accepted
Location: /api/v1/executions/019c...
```

```json
{
  "executionId": "019c1234-5678-7abc-9123-456789abcdef",
  "status": "queued",
  "requestedAt": "2026-07-23T14:30:00Z"
}
```

A resposta não deverá aguardar a conclusão da geração.

---

# 23. Acompanhamento de Operações

O cliente deverá consultar:

```text
GET /api/v1/executions/{executionId}
```

Exemplo:

```json
{
  "id": "019c1234-5678-7abc-9123-456789abcdef",
  "pipelineId": "019c9876-5432-7abc-9123-456789abcdef",
  "status": "running",
  "currentStep": {
    "type": "research",
    "status": "running",
    "attempt": 1
  },
  "requestedAt": "2026-07-23T14:30:00Z",
  "startedAt": "2026-07-23T14:30:02Z",
  "completedAt": null
}
```

Futuramente, atualizações em tempo real poderão utilizar:

- Server-Sent Events.
    
- WebSockets.
    
- Webhooks.
    
- Notifications.
    

O polling será suficiente para o MVP.

---

# 24. Problem Details

Falhas deverão utilizar um formato padronizado de Problem Details.

Exemplo:

```json
{
  "type": "https://errors.infinitecontent.ai/project/not-found",
  "title": "Project not found",
  "status": 404,
  "detail": "O projeto informado não foi encontrado.",
  "instance": "/api/v1/projects/019c...",
  "code": "Project.NotFound",
  "traceId": "00-a12b...",
  "errors": null
}
```

Campos adicionais:

```text
code
traceId
errors
correlationId
retryAfter
```

O campo `detail` não deverá expor:

- Stack traces.
    
- SQL.
    
- Connection strings.
    
- Secrets.
    
- Nomes internos de servidores.
    
- Dados de outros tenants.
    
- Respostas integrais de providers.
    

---

# 25. Mapeamento de Result para HTTP

Exemplo:

```csharp
public static class ApiResults
{
    public static IResult Problem(Error error)
    {
        return error.Type switch
        {
            ErrorType.Validation =>
                ValidationProblem(error),

            ErrorType.NotFound =>
                Results.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Resource not found",
                    extensions: CreateExtensions(error)),

            ErrorType.Conflict =>
                Results.Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Conflict",
                    extensions: CreateExtensions(error)),

            ErrorType.Unauthorized =>
                Results.Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Unauthorized",
                    extensions: CreateExtensions(error)),

            ErrorType.Forbidden =>
                Results.Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Forbidden",
                    extensions: CreateExtensions(error)),

            ErrorType.RateLimit =>
                Results.Problem(
                    statusCode: StatusCodes.Status429TooManyRequests,
                    title: "Rate limit exceeded",
                    extensions: CreateExtensions(error)),

            ErrorType.Timeout =>
                Results.Problem(
                    statusCode: StatusCodes.Status504GatewayTimeout,
                    title: "Operation timed out",
                    extensions: CreateExtensions(error)),

            _ =>
                Results.Problem(
                    statusCode:
                        StatusCodes.Status500InternalServerError,
                    title: "Unexpected failure",
                    extensions: CreateExtensions(error))
        };
    }
}
```

---

# 26. Erros de Validação

Exemplo:

```json
{
  "title": "Validation failed",
  "status": 400,
  "code": "Validation.Failed",
  "traceId": "00-a12b...",
  "errors": {
    "name": [
      "O nome é obrigatório.",
      "O nome deve possuir no máximo 150 caracteres."
    ]
  }
}
```

Nomes de campos deverão corresponder ao contrato HTTP, não necessariamente ao Command interno.

---

# 27. Tratamento Global de Exceções

Exceções inesperadas deverão ser tratadas por um middleware ou exception handler global.

Responsabilidades:

- Registrar a exceção.
    
- Associar TraceId.
    
- Retornar resposta segura.
    
- Não expor detalhes internos.
    
- Classificar exceções conhecidas quando necessário.
    
- Produzir métricas.
    

Exemplo conceitual:

```csharp
public sealed class GlobalExceptionHandler
    : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Unhandled exception. TraceId: {TraceId}",
            httpContext.TraceIdentifier);

        await Results.Problem(
            statusCode:
                StatusCodes.Status500InternalServerError,
            title: "Unexpected failure",
            extensions: new Dictionary<string, object?>
            {
                ["traceId"] = httpContext.TraceIdentifier
            })
            .ExecuteAsync(httpContext);

        return true;
    }
}
```

---

# 28. Middleware Pipeline

Ordem conceitual:

```text
Forwarded Headers
   ↓
Exception Handling
   ↓
Correlation
   ↓
Security Headers
   ↓
HTTPS
   ↓
CORS
   ↓
Rate Limiting
   ↓
Authentication
   ↓
Authorization
   ↓
Request Logging
   ↓
Endpoints
```

A ordem deverá ser validada por testes.

Uma configuração incorreta pode causar:

- Logs sem usuário.
    
- Rate limiting ineficaz.
    
- Erros sem correlação.
    
- CORS incorreto.
    
- Autorização ignorada.
    

---

# 29. Correlation ID

Cada requisição deverá possuir um identificador de correlação.

Header sugerido:

```text
X-Correlation-ID
```

Fluxo:

1. Cliente envia um valor válido ou a API gera um.
    
2. A API inclui o valor nos logs.
    
3. O valor é propagado para Application.
    
4. O valor é propagado para mensagens.
    
5. O valor é retornado na resposta.
    

Resposta:

```http
X-Correlation-ID: 019c1234...
```

O valor enviado pelo cliente deverá ser validado e limitado em tamanho.

---

# 30. Trace ID

A API deverá utilizar o TraceId da instrumentação distribuída.

Diferença:

```text
CorrelationId
    Identificador lógico de uma operação de negócio.

TraceId
    Identificador técnico do trace distribuído.
```

Ambos poderão ser iguais em alguns cenários, mas não deverão ser tratados como o mesmo conceito obrigatoriamente.

---

# 31. Autenticação

A API deverá suportar autenticação baseada em tokens.

Cenários possíveis:

- JWT Bearer.
    
- OpenID Connect.
    
- Identity provider externo.
    
- Tokens de serviço.
    
- API keys específicas para integrações.
    
- Assinaturas de webhook.
    

Para usuários da aplicação, o padrão será:

```text
Authorization: Bearer <token>
```

A autenticação deverá validar:

- Assinatura.
    
- Emissor.
    
- Audiência.
    
- Expiração.
    
- Algoritmo.
    
- Claims obrigatórias.
    

---

# 32. Claims

Claims esperadas poderão incluir:

```text
sub
organization_id
email
role
permissions
```

A API deverá converter as claims para abstrações próprias.

Exemplo:

```csharp
public sealed class HttpCurrentUser : ICurrentUser
{
    public UserId UserId { get; }

    public OrganizationId OrganizationId { get; }

    public bool IsAuthenticated { get; }

    public IReadOnlySet<string> Permissions { get; }
}
```

A Application não deverá depender diretamente de `ClaimsPrincipal`.

---

# 33. Autorização

A autorização ocorrerá em dois níveis.

## Nível HTTP

Verifica:

- Autenticação.
    
- Papel.
    
- Permissão geral.
    
- Escopo do token.
    

Exemplo:

```csharp
.RequireAuthorization(
    Policies.ProjectsCreate);
```

## Nível Application

Verifica:

- Propriedade do recurso.
    
- Organization correta.
    
- Estado do recurso.
    
- Regras específicas do caso de uso.
    

A API não deverá ser a única barreira de autorização.

---

# 34. Permissões

Convenção sugerida:

```text
projects.read
projects.create
projects.update
projects.archive

pipelines.read
pipelines.create
pipelines.update
pipelines.publish

executions.read
executions.start
executions.cancel

artifacts.read
artifacts.approve
artifacts.publish
```

As permissões deverão ser:

- Estáveis.
    
- Documentadas.
    
- Granulares o suficiente.
    
- Independentes de rotas.
    
- Testáveis.
    

---

# 35. Isolamento por Organization

O `OrganizationId` deverá ser obtido do contexto autenticado.

O cliente não deverá controlar livremente a Organization através de body ou query string em operações comuns.

Exemplo correto:

```csharp
var query = new GetProjectQuery(
    currentUser.OrganizationId,
    new ProjectId(projectId));
```

Evitar:

```json
{
  "organizationId": "organization-de-outro-cliente",
  "projectId": "..."
}
```

Para operações administrativas multi-tenant, deverá existir uma API e autorização específicas.

---

# 36. Proteção contra IDOR

Nunca considerar que um recurso pertence ao usuário apenas porque ele conhece o ID.

Toda busca tenant-scoped deverá incluir a Organization.

```text
GET /projects/{projectId}
   ↓
Buscar por OrganizationId + ProjectId
```

Quando o recurso pertencer a outro tenant, a resposta deverá normalmente ser `404`, evitando revelar sua existência.

---

# 37. Rate Limiting

A API deverá aplicar limites conforme:

- Usuário.
    
- Organization.
    
- IP.
    
- Endpoint.
    
- Tipo de operação.
    
- Plano.
    
- Custo estimado.
    

Categorias possíveis:

```text
General
Authentication
Read
Write
ExecutionStart
Upload
Webhook
Administrative
```

Exemplo:

```csharp
.RequireRateLimiting(
    RateLimitPolicies.ExecutionStart);
```

Operações de IA devem possuir limites mais restritos que consultas simples.

---

# 38. Resposta de Rate Limit

```http
HTTP/1.1 429 Too Many Requests
Retry-After: 60
```

```json
{
  "title": "Rate limit exceeded",
  "status": 429,
  "code": "RateLimit.ExecutionStart",
  "retryAfter": 60,
  "traceId": "00-a12b..."
}
```

A API não deverá prometer que a operação será permitida exatamente após o período informado quando outros limites puderem existir.

---

# 39. Idempotência HTTP

Operações críticas deverão aceitar:

```text
Idempotency-Key
```

Exemplos:

```text
POST /pipelines/{pipelineId}/executions
POST /artifacts/{artifactId}/publish
POST /uploads/complete
```

Fluxo:

1. Cliente gera uma chave única.
    
2. API valida formato e tamanho.
    
3. A chave é enviada ao Command.
    
4. Application aplica idempotência.
    
5. Requisições repetidas retornam o mesmo resultado.
    

A chave deverá ser associada a:

```text
OrganizationId
UserId ou ClientId
Endpoint ou CommandType
Payload relevante
```

---

# 40. Conflito de Idempotência

Se a mesma chave for reutilizada com payload diferente:

```http
HTTP/1.1 409 Conflict
```

```json
{
  "title": "Idempotency conflict",
  "status": 409,
  "code": "Idempotency.PayloadMismatch",
  "traceId": "00-a12b..."
}
```

A API não deverá executar a segunda operação.

---

# 41. Concorrência Otimista

Recursos editáveis poderão expor versão.

Exemplo:

```json
{
  "id": "019c...",
  "name": "Pipeline de vídeo",
  "version": 4
}
```

Atualização:

```http
If-Match: "4"
```

Caso a versão tenha mudado:

```http
HTTP/1.1 412 Precondition Failed
```

ou:

```http
HTTP/1.1 409 Conflict
```

A escolha deverá permanecer consistente por recurso.

Para o MVP, `version` no corpo poderá ser suficiente, mas headers condicionais são recomendados para evolução.

---

# 42. Paginação

Parâmetros:

```text
page
pageSize
```

Exemplo:

```text
GET /api/v1/projects?page=1&pageSize=20
```

Resposta:

```json
{
  "items": [
    {
      "id": "019c...",
      "name": "Canal principal",
      "status": "active"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 47,
  "totalPages": 3
}
```

Regras:

- `page` inicia em 1.
    
- `pageSize` possui valor padrão.
    
- `pageSize` possui máximo.
    
- Ordenação deve ser determinística.
    
- Valores inválidos retornam erro de validação.
    

---

# 43. Paginação por Cursor

Para coleções grandes ou atualizadas frequentemente, poderá ser introduzida paginação por cursor.

Exemplo:

```text
GET /executions?limit=20&cursor=eyJ...
```

Resposta:

```json
{
  "items": [],
  "nextCursor": "eyJ..."
}
```

O cursor deverá ser:

- Opaco.
    
- Assinado ou validado.
    
- Limitado em tamanho.
    
- Independente de detalhes sensíveis.
    

A paginação por página será suficiente para o MVP.

---

# 44. Filtros

Exemplo:

```text
GET /api/v1/executions
    ?status=running
    &projectId=019c...
    &createdFrom=2026-07-01T00:00:00Z
    &createdTo=2026-07-31T23:59:59Z
```

Filtros deverão:

- Ser documentados.
    
- Possuir limites.
    
- Ser validados.
    
- Utilizar nomes consistentes.
    
- Não permitir SQL ou expressões arbitrárias.
    
- Considerar Organization automaticamente.
    

---

# 45. Ordenação

Formato possível:

```text
sort=-createdAt,name
```

Significado:

- `-createdAt`: decrescente.
    
- `name`: crescente.
    

Somente campos autorizados poderão ser utilizados.

Evitar permitir que o cliente informe livremente nomes de colunas internas.

---

# 46. Busca

Parâmetro sugerido:

```text
search
```

Exemplo:

```text
GET /api/v1/projects?search=tecnologia
```

Regras:

- Limitar tamanho.
    
- Normalizar espaços.
    
- Proteger contra abuso.
    
- Não expor detalhes da implementação.
    
- Aplicar paginação.
    
- Aplicar isolamento por Organization.
    

---

# 47. Campos Opcionais e Expansões

A API poderá futuramente suportar:

```text
fields
include
expand
```

Exemplo:

```text
GET /executions/{id}?include=steps,artifacts
```

Entretanto, expansões complexas não deverão ser implementadas no MVP.

Respostas muito configuráveis podem:

- Aumentar complexidade.
    
- Gerar queries imprevisíveis.
    
- Dificultar cache.
    
- Criar problemas de autorização.
    
- Tornar OpenAPI menos claro.
    

---

# 48. Uploads

Uploads grandes não deverão atravessar a API principal quando puderem ser enviados diretamente ao storage.

Fluxo recomendado:

```text
Cliente
   ↓
Solicita sessão de upload
   ↓
API cria autorização temporária
   ↓
Cliente envia ao Blob Storage
   ↓
Cliente confirma upload
   ↓
API valida e registra Artifact
```

Endpoints possíveis:

```text
POST /api/v1/uploads
POST /api/v1/uploads/{uploadId}/complete
```

---

# 49. Validação de Upload

Validar:

- Tamanho.
    
- Content-Type.
    
- Extensão.
    
- Assinatura do arquivo.
    
- Checksum.
    
- Organization.
    
- Expiração da sessão.
    
- Destino autorizado.
    
- Tipo de Artifact.
    
- Quantidade de uploads.
    

Não confiar apenas em:

- Nome do arquivo.
    
- Extensão.
    
- Content-Type enviado pelo cliente.
    

---

# 50. Downloads

Downloads privados deverão utilizar:

- Autorização prévia.
    
- URLs temporárias.
    
- Tempo de expiração curto.
    
- Escopo restrito.
    
- Proteção contra enumeração.
    

A API poderá retornar:

```json
{
  "downloadUrl": "https://storage/...token-temporario",
  "expiresAt": "2026-07-23T15:00:00Z"
}
```

Secrets permanentes não deverão ser enviados.

---

# 51. Webhooks Recebidos

Webhooks externos deverão possuir endpoints isolados.

Exemplo:

```text
POST /api/v1/webhooks/youtube
POST /api/v1/webhooks/n8n
POST /api/v1/webhooks/provider
```

Cada endpoint deverá:

- Validar assinatura.
    
- Validar timestamp.
    
- Prevenir replay.
    
- Limitar tamanho.
    
- Registrar MessageId.
    
- Aplicar idempotência.
    
- Responder rapidamente.
    
- Delegar processamento longo ao Worker.
    

---

# 52. Resposta de Webhook

Fluxo recomendado:

```text
Receber webhook
   ↓
Validar assinatura
   ↓
Persistir Inbox
   ↓
Agendar processamento
   ↓
Retornar 202 ou 204
```

Não executar processamento longo antes de responder ao serviço externo.

---

# 53. CORS

CORS deverá utilizar uma allowlist explícita.

Evitar em produção:

```csharp
.AllowAnyOrigin()
.AllowAnyHeader()
.AllowAnyMethod()
```

Configuração deverá considerar:

- Ambientes.
    
- Aplicações oficiais.
    
- Headers permitidos.
    
- Métodos permitidos.
    
- Uso de credenciais.
    
- Cache de preflight.
    

CORS não substitui autenticação.

---

# 54. Security Headers

A API deverá aplicar headers apropriados quando aplicável:

```text
X-Content-Type-Options
Referrer-Policy
Content-Security-Policy
Strict-Transport-Security
```

Nem todos possuem o mesmo impacto em APIs sem conteúdo HTML, mas devem ser avaliados.

Headers de identificação desnecessários deverão ser removidos.

---

# 55. HTTPS

Produção deverá utilizar HTTPS obrigatoriamente.

Requisitos:

- Redirecionamento ou rejeição de HTTP.
    
- Certificados válidos.
    
- TLS atualizado.
    
- Forwarded Headers configurados.
    
- Reconhecimento correto de proxies.
    
- HSTS quando apropriado.
    

---

# 56. Forwarded Headers

Quando executada atrás de proxy, gateway ou load balancer, a API deverá interpretar corretamente:

```text
X-Forwarded-For
X-Forwarded-Proto
X-Forwarded-Host
```

Somente proxies confiáveis deverão ser aceitos.

Configuração incorreta pode causar:

- IP falso.
    
- URLs incorretas.
    
- Redirecionamento em loop.
    
- Logs inválidos.
    
- Bypass de regras.
    

---

# 57. Limites de Requisição

A API deverá definir limites para:

- Tamanho de body.
    
- Tamanho de headers.
    
- Quantidade de headers.
    
- Tempo de leitura.
    
- Número de campos.
    
- Profundidade de JSON.
    
- Tamanho de strings.
    
- Quantidade de itens por coleção.
    
- Quantidade de arquivos.
    

Operações que exigirem payloads grandes deverão utilizar storage.

---

# 58. Proteção contra Mass Assignment

Requests deverão possuir somente campos permitidos.

Evitar receber diretamente uma Entity.

Exemplo inseguro:

```csharp
app.MapPost(
    "/projects",
    (Project project) => ...);
```

O cliente poderia tentar controlar:

- OrganizationId.
    
- Status.
    
- CreatedBy.
    
- CreatedAt.
    
- Permissões.
    
- Versão.
    
- Campos internos.
    

Utilizar contratos específicos impede esse problema.

---

# 59. Proteção contra SSRF

Qualquer endpoint que receba URLs deverá validar:

- Protocolo.
    
- Host.
    
- Porta.
    
- DNS.
    
- Redirecionamentos.
    
- Redes privadas.
    
- Endereços locais.
    
- Tamanho da resposta.
    
- Timeout.
    

Exemplos de risco:

- Importar conteúdo por URL.
    
- Baixar imagens.
    
- Validar websites.
    
- Processar callbacks configuráveis.
    

A validação profunda ficará em Infrastructure, mas a API deverá limitar o contrato aceito.

---

# 60. OpenAPI

Todos os endpoints públicos deverão possuir documentação OpenAPI.

Informações mínimas:

- Nome.
    
- Resumo.
    
- Descrição.
    
- Tags.
    
- Parâmetros.
    
- Request.
    
- Responses.
    
- Status codes.
    
- Autenticação.
    
- Exemplos.
    
- Depreciação.
    
- Idempotency-Key quando aplicável.
    

Exemplo:

```csharp
.WithName("StartPipelineExecution")
.WithTags("Executions")
.WithSummary("Inicia uma execução assíncrona de pipeline.")
.Produces<StartExecutionResponse>(
    StatusCodes.Status202Accepted)
.ProducesProblem(
    StatusCodes.Status400BadRequest)
.ProducesProblem(
    StatusCodes.Status404NotFound)
.ProducesProblem(
    StatusCodes.Status409Conflict);
```

---

# 61. Segurança no OpenAPI

O OpenAPI deverá descrever autenticação Bearer.

A interface de documentação em ambientes produtivos deverá ser:

- Protegida.
    
- Desativada.
    
- Limitada por rede.
    
- Ou publicada sem detalhes internos.
    

Endpoints administrativos não deverão ser expostos acidentalmente.

---

# 62. Exemplos de Contratos

OpenAPI deverá possuir exemplos realistas.

Exemplo de criação:

```json
{
  "name": "Canal de tecnologia",
  "description": "Projeto para produção de vídeos semanais."
}
```

Exemplo de erro:

```json
{
  "title": "Validation failed",
  "status": 400,
  "code": "Validation.Failed",
  "traceId": "00-a12b...",
  "errors": {
    "name": [
      "O nome é obrigatório."
    ]
  }
}
```

Não utilizar secrets ou dados reais nos exemplos.

---

# 63. Health Checks

A API deverá expor:

```text
GET /health/live
GET /health/ready
```

## Liveness

Verifica se o processo está vivo.

Não deverá depender de serviços externos.

## Readiness

Verifica se a API está pronta para servir requisições.

Poderá verificar:

- PostgreSQL.
    
- RabbitMQ, caso essencial.
    
- Configurações obrigatórias.
    
- Dependências críticas.
    

---

# 64. Resposta de Health Check

Em ambientes públicos, a resposta deverá ser mínima.

Exemplo:

```json
{
  "status": "healthy"
}
```

Detalhes internos deverão ser restritos a:

- Rede interna.
    
- Autenticação administrativa.
    
- Sistema de observabilidade.
    

Não expor:

- Connection strings.
    
- Hosts.
    
- Exceções.
    
- Nomes internos de servidores.
    
- Secrets.
    
- Versões vulneráveis.
    

---

# 65. Endpoints Administrativos

Endpoints administrativos deverão possuir:

- Prefixo próprio.
    
- Autorização forte.
    
- Auditoria.
    
- Rate limiting específico.
    
- Restrição de rede quando possível.
    

Exemplo:

```text
/api/v1/admin/executions/{id}/retry
/api/v1/admin/outbox
/api/v1/admin/health/details
```

Eles não deverão ser misturados aos endpoints públicos comuns.

---

# 66. Observabilidade HTTP

Cada requisição deverá produzir:

- Método.
    
- Rota normalizada.
    
- Status.
    
- Duração.
    
- Tamanho de request.
    
- Tamanho de response.
    
- OrganizationId, quando seguro.
    
- UserId, quando seguro.
    
- CorrelationId.
    
- TraceId.
    
- Código de erro.
    
- Rate limit aplicado.
    

Evitar registrar a URL completa quando query strings puderem conter dados sensíveis.

---

# 67. Logs de Requisição

Logs deverão utilizar o template da rota:

```text
/api/v1/projects/{projectId}
```

Evitar alta cardinalidade com:

```text
/api/v1/projects/019c1234...
```

IDs poderão existir como propriedades estruturadas, mas não no nome da métrica.

---

# 68. Métricas

Métricas iniciais:

```text
http.server.requests
http.server.duration
api.errors
api.validation.failures
api.authentication.failures
api.authorization.failures
api.rate_limit.rejections
api.idempotency.replays
api.upload.bytes
```

Dimensões deverão possuir cardinalidade controlada.

Não utilizar como labels:

- UserId.
    
- ProjectId.
    
- ExecutionId.
    
- CorrelationId.
    
- URL completa.
    

---

# 69. Traces

O trace deverá acompanhar:

```text
HTTP Request
   ↓
Endpoint
   ↓
Application Handler
   ↓
PostgreSQL
   ↓
Outbox
```

Para operações assíncronas:

```text
HTTP Request
   ↓
Outbox
   ↓
RabbitMQ
   ↓
Worker
   ↓
Provider
```

O contexto deverá ser propagado nas mensagens.

---

# 70. Cache HTTP

A API poderá utilizar cache HTTP em recursos públicos ou estáveis.

Possíveis headers:

```text
Cache-Control
ETag
Last-Modified
```

Recursos privados e tenant-scoped deverão utilizar regras seguras.

Evitar cache compartilhado para respostas contendo dados de usuário.

Para o MVP, cache HTTP não será obrigatório.

---

# 71. Compressão

Respostas grandes poderão utilizar compressão.

Cuidados:

- Não comprimir dados já comprimidos.
    
- Avaliar impacto de CPU.
    
- Evitar compressão em respostas com secrets refletidos.
    
- Configurar tamanho mínimo.
    
- Medir benefício.
    

JSON de listagens e detalhes pode se beneficiar.

---

# 72. Localização

Mensagens de erro públicas poderão futuramente ser localizadas.

Entretanto:

- `code` permanece estável.
    
- `title` e `detail` podem mudar por idioma.
    
- Logs internos não dependem da mensagem localizada.
    
- Clientes devem reagir pelo `code`, não pelo texto.
    

Exemplo:

```text
Project.NotFound
```

permanece igual em português ou inglês.

---

# 73. Auditoria

Ações críticas deverão produzir registros de auditoria:

- Criação de Project.
    
- Publicação de Pipeline.
    
- Início de Execution.
    
- Cancelamento.
    
- Aprovação.
    
- Publicação externa.
    
- Alteração de permissões.
    
- Ações administrativas.
    

O endpoint não deverá persistir auditoria diretamente.

Ele fornece o contexto para a Application.

---

# 74. Configuração

A API deverá utilizar Options Pattern.

Exemplos:

```text
AuthenticationOptions
CorsOptions
RateLimitOptions
OpenApiOptions
UploadOptions
ForwardedHeadersOptions
ApiSecurityOptions
```

Configurações obrigatórias deverão ser validadas no startup.

Evitar `IConfiguration` diretamente em endpoints e serviços de feature.

---

# 75. Ambientes

Ambientes esperados:

```text
Development
Test
Staging
Production
```

Diferenças comuns:

## Development

- OpenAPI habilitado.
    
- Logs mais detalhados.
    
- Providers fake opcionais.
    
- CORS local.
    

## Test

- Dependências controladas.
    
- Autenticação de teste.
    
- Banco isolado.
    

## Staging

- Configuração semelhante à produção.
    
- Integrações sandbox.
    
- Dados não produtivos.
    

## Production

- Segurança máxima.
    
- Logs controlados.
    
- Secrets externos.
    
- OpenAPI restrito.
    
- Rate limiting ativo.
    

---

# 76. Program.cs

O `Program.cs` deverá permanecer pequeno.

Exemplo:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApi(builder.Configuration)
    .AddApplication()
    .AddData(builder.Configuration)
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseApiPipeline();

app.MapApplicationEndpoints();
app.MapHealthChecks();

await app.RunAsync();
```

Configurações detalhadas deverão ficar em métodos de extensão específicos.

---

# 77. DependencyInjection

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthenticationServices(configuration);
        services.AddAuthorizationServices();
        services.AddApiVersioningServices();
        services.AddProblemDetailsServices();
        services.AddOpenApiServices();
        services.AddRateLimitingServices(configuration);
        services.AddCorsServices(configuration);
        services.AddExceptionHandlingServices();

        return services;
    }
}
```

Evitar um único método com centenas de linhas.

---

# 78. Testes da API

A API deverá possuir:

- Testes de endpoints.
    
- Testes de autenticação.
    
- Testes de autorização.
    
- Testes de Problem Details.
    
- Testes de validação.
    
- Testes de versionamento.
    
- Testes de idempotência.
    
- Testes de rate limiting.
    
- Testes de uploads.
    
- Testes de CORS quando relevante.
    
- Testes de headers.
    
- Testes de OpenAPI.
    
- Testes de tenancy.
    

---

# 79. WebApplicationFactory

Testes de integração poderão utilizar:

```csharp
public sealed class ApiFactory
    : WebApplicationFactory<Program>
{
}
```

A factory poderá substituir:

- Autenticação.
    
- Clock.
    
- Providers.
    
- Banco.
    
- Mensageria.
    
- Configurações.
    

Os testes deverão utilizar banco isolado ou containers quando validarem persistência real.

---

# 80. Teste de Criação

```csharp
[Fact]
public async Task CreateProject_ShouldReturnCreated()
{
    var client = _factory.CreateAuthenticatedClient();

    var response = await client.PostAsJsonAsync(
        "/api/v1/projects",
        new
        {
            name = "Canal de tecnologia",
            description = "Conteúdo semanal."
        });

    response.StatusCode.Should().Be(
        HttpStatusCode.Created);

    var body =
        await response.Content
            .ReadFromJsonAsync<CreateProjectResponse>();

    body.Should().NotBeNull();
    body!.Name.Should().Be(
        "Canal de tecnologia");
}
```

---

# 81. Teste de Isolamento

```csharp
[Fact]
public async Task GetProject_ShouldReturnNotFound_ForAnotherOrganization()
{
    var project = await _fixture.CreateProjectAsync(
        organizationId: OrganizationA.Id);

    var client = _factory.CreateAuthenticatedClient(
        organizationId: OrganizationB.Id);

    var response = await client.GetAsync(
        $"/api/v1/projects/{project.Id}");

    response.StatusCode.Should().Be(
        HttpStatusCode.NotFound);
}
```

Esse teste é obrigatório para recursos tenant-scoped.

---

# 82. Teste de Validação

```csharp
[Fact]
public async Task CreateProject_ShouldReturnBadRequest_WhenNameIsEmpty()
{
    var client = _factory.CreateAuthenticatedClient();

    var response = await client.PostAsJsonAsync(
        "/api/v1/projects",
        new
        {
            name = "",
            description = "Descrição"
        });

    response.StatusCode.Should().Be(
        HttpStatusCode.BadRequest);
}
```

O corpo deverá seguir Problem Details.

---

# 83. Teste de Operação Assíncrona

```csharp
[Fact]
public async Task StartExecution_ShouldReturnAccepted()
{
    var client = _factory.CreateAuthenticatedClient();

    using var request = new HttpRequestMessage(
        HttpMethod.Post,
        $"/api/v1/pipelines/{_pipelineId}/executions");

    request.Headers.Add(
        "Idempotency-Key",
        Guid.NewGuid().ToString());

    request.Content = JsonContent.Create(
        new StartExecutionRequest());

    var response = await client.SendAsync(request);

    response.StatusCode.Should().Be(
        HttpStatusCode.Accepted);

    response.Headers.Location.Should().NotBeNull();
}
```

---

# 84. Testes de Contrato

A API deverá validar que:

- OpenAPI corresponde aos endpoints.
    
- Status documentados são retornáveis.
    
- Campos obrigatórios permanecem estáveis.
    
- Schemas não mudam acidentalmente.
    
- Versionamento está correto.
    

Snapshots de OpenAPI poderão ser utilizados com revisão cuidadosa.

---

# 85. Testes de Arquitetura

Regras:

- Endpoints não dependem de DbContext.
    
- Endpoints não dependem de repositories concretos.
    
- Endpoints não dependem de providers.
    
- Requests não são entidades.
    
- Responses não são entidades.
    
- Features da API dependem da Application.
    
- Program.cs permanece pequeno.
    
- Classes de endpoint ficam em `Api.Features`.
    

---

# 86. Antipadrões

## Regra de negócio no Endpoint

Evitar:

```csharp
if (request.Status == "completed")
{
    execution.Status = Completed;
}
```

## DbContext no Endpoint

Persistência deve ser delegada à Application.

## SDK no Endpoint

Não chamar OpenAI, RabbitMQ ou Azure diretamente.

## Entity como Request

Expõe campos internos e permite mass assignment.

## Entity como Response

Acopla contrato público ao Domain.

## Retornar Exception.Message

Pode expor informações internas.

## HTTP 200 para todos os resultados

Status codes devem representar o resultado real.

## Rotas com verbos CRUD desnecessários

Evitar:

```text
/createProject
/getProject
/deleteProject
```

## Rotas excessivamente aninhadas

Evitar rotas difíceis de manter:

```text
/organizations/{organizationId}/projects/{projectId}/pipelines/{pipelineId}/executions/{executionId}/steps/{stepId}
```

Quando o recurso possui identificador global, preferir rota direta após a validação de tenancy.

## Endpoint genérico

Evitar:

```text
POST /api/v1/execute
```

## Capturar toda exceção no endpoint

O tratamento deve ser global.

---

# 87. Regras Arquiteturais

1. A API é um host, não o núcleo da aplicação.
    
2. Endpoints delegam para a Application.
    
3. Endpoints não acessam DbContext.
    
4. Endpoints não acessam repositories concretos.
    
5. Endpoints não chamam SDKs externos.
    
6. Requests são contratos próprios.
    
7. Responses são contratos próprios.
    
8. Entidades não são expostas.
    
9. Rotas são versionadas.
    
10. Erros utilizam Problem Details.
    
11. Exceções são tratadas globalmente.
    
12. Organization é derivada do contexto autenticado.
    
13. Recursos tenant-scoped são consultados por Organization e ID.
    
14. Operações longas retornam `202 Accepted`.
    
15. Operações críticas suportam Idempotency-Key.
    
16. CancellationToken é propagado.
    
17. Logs não expõem payloads sensíveis.
    
18. OpenAPI documenta endpoints públicos.
    
19. Rate limiting é aplicado por categoria.
    
20. Uploads grandes utilizam storage direto.
    
21. Webhooks são autenticados e idempotentes.
    
22. Health checks não expõem detalhes internos.
    
23. Program.cs permanece pequeno.
    
24. Configurações são tipadas.
    
25. Testes validam isolamento entre Organizations.
    

---

# 88. Endpoints do MVP

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

## Operational

```text
GET /health/live
GET /health/ready
```

---

# 89. Ordem de Implementação

## Etapa 1 — Fundação

- Criar projeto Api.
    
- Configurar Minimal APIs.
    
- Configurar JSON.
    
- Configurar Problem Details.
    
- Configurar exception handler.
    
- Configurar autenticação.
    
- Configurar autorização.
    
- Configurar OpenAPI.
    
- Configurar health checks.
    
- Configurar correlação.
    

## Etapa 2 — Projects

- CreateProject.
    
- GetProject.
    
- ListProjects.
    
- Testes de tenancy.
    

## Etapa 3 — Pipelines

- CreatePipeline.
    
- AddPipelineStep.
    
- PublishPipeline.
    
- GetPipeline.
    

## Etapa 4 — Executions

- StartExecution.
    
- GetExecution.
    
- CancelExecution.
    
- Idempotency-Key.
    
- Resposta `202 Accepted`.
    

## Etapa 5 — Artifacts

- ListExecutionArtifacts.
    
- GetArtifact.
    
- Downloads autorizados.
    

## Etapa 6 — Proteções

- Rate limiting.
    
- Limites de payload.
    
- Security headers.
    
- CORS.
    
- Testes de segurança.
    

---

# 90. Checklist para Novo Endpoint

- A rota representa um recurso ou ação clara?
    
- O método HTTP está correto?
    
- A versão está presente?
    
- O endpoint possui nome?
    
- Está associado a uma tag?
    
- Possui resumo?
    
- Possui autenticação?
    
- Possui autorização?
    
- Aplica rate limiting?
    
- Utiliza Request próprio?
    
- Utiliza Response próprio?
    
- Converte para Command ou Query?
    
- Propaga CancellationToken?
    
- Mapeia Result corretamente?
    
- Documenta status codes?
    
- Considera Organization?
    
- Protege contra IDOR?
    
- Precisa de Idempotency-Key?
    
- Precisa retornar `Location`?
    
- Precisa retornar `202 Accepted`?
    
- Possui testes?
    
- Está documentado no OpenAPI?
    

---

# 91. Checklist de Segurança

- Autenticação obrigatória?
    
- Permissão correta?
    
- Organization derivada do token?
    
- Recurso filtrado por Organization?
    
- Payload limitado?
    
- Strings limitadas?
    
- URLs validadas?
    
- Arquivos validados?
    
- Dados sensíveis fora dos logs?
    
- Erros sem detalhes internos?
    
- Rate limiting aplicado?
    
- CORS restrito?
    
- Webhook assinado?
    
- Idempotência aplicada?
    
- Headers de proxy configurados?
    
- OpenAPI protegido em produção?
    

---

# 92. Critérios de Qualidade

A API será considerada saudável quando:

- Endpoints forem pequenos.
    
- Contratos forem claros.
    
- Status codes forem consistentes.
    
- Erros seguirem o mesmo formato.
    
- Autenticação e autorização forem testadas.
    
- Dados não vazarem entre Organizations.
    
- Operações longas não bloquearem HTTP.
    
- Clientes puderem repetir operações críticas com segurança.
    
- OpenAPI refletir a implementação.
    
- Logs permitirem rastrear uma requisição.
    
- Health checks suportarem operação em containers.
    
- Nenhum endpoint depender diretamente de infraestrutura.
    

---

# 93. Documentos Relacionados

```text
04 - Backend/Visão Geral do Backend.md
04 - Backend/Organização por Features.md
04 - Backend/Domain.md
04 - Backend/Application.md
04 - Backend/Data.md
04 - Backend/Infrastructure.md
04 - Backend/Worker.md
04 - Backend/Contracts.md
04 - Backend/Shared Kernel.md
```

---

# 94. Filosofia Final

A API é a fronteira HTTP do Infinite Content AI.

Ela deverá traduzir:

```text
HTTP
   ↓
Intenção de negócio
```

e depois:

```text
Resultado da Application
   ↓
HTTP
```

Ela não deverá implementar o negócio.

O código da API deverá expressar ações como:

```text
Receber request
Autenticar usuário
Resolver Organization
Criar Command
Enviar Query
Mapear Result
Retornar resposta
```

Ele não deverá expressar detalhes como:

```text
Alterar estado de Aggregate
Executar SQL
Publicar no RabbitMQ
Chamar provider de IA
Manipular Blob Storage
Controlar pipeline
```

A regra principal será:

> A API protege e traduz o limite HTTP; a Application executa o caso de uso e o Domain protege as regras.

Quando essa separação for mantida, a API poderá evoluir, receber novas versões e servir diferentes clientes sem acoplar o produto ao protocolo HTTP.