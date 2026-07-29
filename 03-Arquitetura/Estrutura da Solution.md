# Estrutura da Solution

## Nome da Solution

```text
InfiniteContent.sln
```

A solution será organizada em projetos separados por responsabilidade.

A persistência de dados ficará isolada no projeto `InfiniteContent.Data`, enquanto integrações externas permanecerão no projeto `InfiniteContent.Infrastructure`.

---

# Estrutura Geral

```text
InfiniteContent/
│
├── src/
│   ├── InfiniteContent.Api/
│   ├── InfiniteContent.Application/
│   ├── InfiniteContent.Domain/
│   ├── InfiniteContent.Data/
│   ├── InfiniteContent.Infrastructure/
│   ├── InfiniteContent.Worker/
│   ├── InfiniteContent.Contracts/
│   └── InfiniteContent.SharedKernel/
│
├── tests/
│   ├── InfiniteContent.UnitTests/
│   ├── InfiniteContent.IntegrationTests/
│   └── InfiniteContent.ArchitectureTests/
│
├── docs/
│
├── n8n/
│   └── workflows/
│
├── docker/
│
├── scripts/
│
├── .github/
│   └── workflows/
│
├── AGENTS.md
├── docker-compose.yml
├── README.md
└── InfiniteContent.sln
```

---

# Projetos da Solution

## InfiniteContent.Api

Projeto responsável pela entrada HTTP da aplicação.

### Responsabilidades

- Endpoints REST.
    
- Autenticação.
    
- Autorização.
    
- Validação de requisições.
    
- Swagger e OpenAPI.
    
- Health checks.
    
- Tratamento global de exceções.
    
- Configuração da aplicação.
    
- Injeção de dependência.
    

### Não deve conter

- Regras de negócio.
    
- Acesso direto ao banco de dados.
    
- Integrações externas.
    
- Processamento de conteúdo.
    

---

## InfiniteContent.Application

Projeto responsável pelos casos de uso da aplicação.

### Responsabilidades

- Commands.
    
- Queries.
    
- Handlers.
    
- Casos de uso.
    
- Orquestração.
    
- Validações.
    
- Interfaces para persistência.
    
- Interfaces para integrações externas.
    
- Mapeamentos.
    
- DTOs internos.
    

### Exemplos

```text
CreateContentProject
GenerateScript
GenerateSeoMetadata
ApproveContent
PublishContent
```

### Não deve conter

- Implementações de banco de dados.
    
- Chamadas diretas para OpenAI.
    
- Dependência da API.
    
- Dependência da Infrastructure.
    
- Dependência da Data.
    

---

## InfiniteContent.Domain

Projeto responsável pelo núcleo de negócio.

### Responsabilidades

- Entidades.
    
- Agregados.
    
- Value Objects.
    
- Regras de negócio.
    
- Eventos de domínio.
    
- Exceções de domínio.
    
- Enums do domínio.
    

### Exemplos de entidades

```text
ContentProject
ContentJob
GeneratedAsset
PromptExecution
ProviderUsage
Publication
ContentMetric
```

### Regra principal

O projeto Domain não deve depender de frameworks ou detalhes externos.

Ele poderá depender apenas do `InfiniteContent.SharedKernel`.

---

## InfiniteContent.Data

Projeto responsável exclusivamente pela persistência de dados.

### Responsabilidades

- Entity Framework Core.
    
- PostgreSQL.
    
- DbContext.
    
- Configurações das entidades.
    
- Migrations.
    
- Repositórios.
    
- Unit of Work, caso seja necessário.
    
- Consultas específicas.
    
- Controle de transações.
    
- Persistência de eventos e execuções.
    

### Exemplos

```text
InfiniteContentDbContext
ContentProjectConfiguration
ContentProjectRepository
PublicationRepository
UnitOfWork
```

### Deve conter

```text
Data/
├── Context/
├── Configurations/
├── Migrations/
├── Repositories/
├── Queries/
├── Interceptors/
└── DependencyInjection.cs
```

### Não deve conter

- Regras de negócio.
    
- Integração com OpenAI.
    
- Integração com YouTube.
    
- Lógica de publicação.
    
- Processamento de vídeo.
    
- Controllers.
    
- Casos de uso.
    

### Dependências permitidas

- Application.
    
- Domain.
    
- SharedKernel.
    
- Contracts, quando necessário.
    

---

## InfiniteContent.Infrastructure

Projeto responsável pelas integrações e implementações externas que não pertencem à persistência.

### Responsabilidades

- Providers de inteligência artificial.
    
- Integração com OpenAI.
    
- Integração com Gemini.
    
- Integração com Anthropic.
    
- Integração com YouTube.
    
- Armazenamento de arquivos.
    
- Mensageria.
    
- Cache.
    
- Serviços externos.
    
- Observabilidade.
    
- E-mail.
    
- Webhooks.
    
- Processamento de mídia.
    
- Integração com n8n.
    

### Exemplos

```text
OpenAiTextProvider
GeminiTextProvider
YouTubePublishingProvider
S3StorageProvider
RedisCacheProvider
N8nWebhookClient
```

### Dependências permitidas

- Application.
    
- Domain.
    
- SharedKernel.
    
- Contracts, quando necessário.
    

### Não deve conter

- DbContext.
    
- Migrations.
    
- Configurações do Entity Framework.
    
- Repositórios do PostgreSQL.
    
- Regras de negócio.
    
- Controllers.
    

---

## InfiniteContent.Worker

Projeto responsável por tarefas executadas em segundo plano.

### Responsabilidades

- Processamento de filas.
    
- Background services.
    
- Jobs recorrentes.
    
- Retentativas.
    
- Processamento de vídeos.
    
- Geração de mídia.
    
- Coleta de métricas.
    
- Comunicação com workflows do n8n.
    
- Execução de pipelines demorados.
    

### Exemplos de tarefas

```text
GenerateContentJob
RenderVideoJob
PublishVideoJob
CollectAnalyticsJob
```

### Não deve conter

- Regras de negócio duplicadas.
    
- Acesso direto ao DbContext.
    
- Implementações específicas de providers.
    
- Lógica de domínio.
    

O Worker deverá chamar casos de uso da camada Application.

---

## InfiniteContent.Contracts

Projeto responsável pelos contratos compartilhados.

### Responsabilidades

- Requests públicos.
    
- Responses públicos.
    
- Eventos de integração.
    
- Mensagens de fila.
    
- Contratos de webhooks.
    
- Payloads externos.
    

### Exemplos

```text
CreateContentRequest
CreateContentResponse
ContentGeneratedEvent
PublishContentCommand
N8nWebhookPayload
```

### Regra

Este projeto não deve conter regras de negócio.

---

## InfiniteContent.SharedKernel

Projeto responsável por abstrações realmente compartilhadas.

### Responsabilidades

- Classe base de Entity.
    
- Classe base de Value Object.
    
- Result Pattern.
    
- Error.
    
- Domain Event.
    
- Paginação.
    
- Interfaces comuns.
    
- Tipos utilitários realmente compartilhados.
    

### Atenção

O Shared Kernel deve ser pequeno.

Ele não pode virar uma pasta genérica onde qualquer código é colocado.

---

# Projetos de Teste

## InfiniteContent.UnitTests

Responsável por testes rápidos e isolados.

### Deve testar

- Regras de domínio.
    
- Value Objects.
    
- Handlers.
    
- Serviços.
    
- Agentes.
    
- Validações.
    
- Casos de uso.
    

---

## InfiniteContent.IntegrationTests

Responsável por testar integrações reais.

### Deve testar

- API.
    
- PostgreSQL.
    
- Entity Framework.
    
- Repositórios.
    
- DbContext.
    
- Filas.
    
- Providers simulados.
    
- Fluxos completos.
    
- Comunicação entre Application, Data e Infrastructure.
    

### Ferramentas sugeridas

- xUnit.
    
- Testcontainers.
    
- WebApplicationFactory.
    
- FluentAssertions.
    
- Respawn.
    

---

## InfiniteContent.ArchitectureTests

Responsável por garantir que as regras arquiteturais sejam respeitadas.

### Exemplos de regras

- Domain não pode depender de Data.
    
- Domain não pode depender de Infrastructure.
    
- Application não pode depender de API.
    
- Application não pode depender de Data.
    
- Application não pode depender de Infrastructure.
    
- Controllers não podem acessar DbContext.
    
- Workers não podem acessar DbContext diretamente.
    
- Providers devem implementar interfaces.
    
- Repositórios devem implementar interfaces definidas na Application.
    
- Handlers devem permanecer na Application.
    
- Migrations devem permanecer na Data.
    

---

# Dependências entre Projetos

```text
InfiniteContent.Api
├── InfiniteContent.Application
├── InfiniteContent.Contracts
├── InfiniteContent.Data
└── InfiniteContent.Infrastructure

InfiniteContent.Worker
├── InfiniteContent.Application
├── InfiniteContent.Contracts
├── InfiniteContent.Data
└── InfiniteContent.Infrastructure

InfiniteContent.Data
├── InfiniteContent.Application
├── InfiniteContent.Domain
├── InfiniteContent.Contracts
└── InfiniteContent.SharedKernel

InfiniteContent.Infrastructure
├── InfiniteContent.Application
├── InfiniteContent.Domain
├── InfiniteContent.Contracts
└── InfiniteContent.SharedKernel

InfiniteContent.Application
├── InfiniteContent.Domain
├── InfiniteContent.Contracts
└── InfiniteContent.SharedKernel

InfiniteContent.Domain
└── InfiniteContent.SharedKernel
```

---

# Direção das Dependências

```text
Api ───────────────────────────┐
                               │
Worker ────────────────────────┤
                               ▼
                         Application
                          ▲         ▲
                          │         │
                       Data   Infrastructure
                          \         /
                           \       /
                            ▼     ▼
                             Domain
                               │
                               ▼
                         SharedKernel
```

As dependências devem apontar para as camadas internas.

A camada Data implementa interfaces de persistência definidas pela Application.

A camada Infrastructure implementa interfaces de serviços externos definidas pela Application.

---

# Fluxo de Persistência

```text
Endpoint
   │
   ▼
Application Handler
   │
   ▼
IContentProjectRepository
   │
   ▼
ContentProjectRepository
   │
   ▼
InfiniteContentDbContext
   │
   ▼
PostgreSQL
```

A Application conhece apenas a interface.

A implementação concreta fica no projeto Data.

---

# Fluxo de Integração Externa

```text
Application Handler
   │
   ▼
IAiTextProvider
   │
   ▼
OpenAiTextProvider
   │
   ▼
OpenAI API
```

A Application conhece apenas a abstração.

A implementação concreta fica no projeto Infrastructure.

---

# Estrutura Interna Sugerida

## Application

```text
Application/
├── Abstractions/
│   ├── Persistence/
│   ├── Ai/
│   ├── Publishing/
│   ├── Storage/
│   ├── Messaging/
│   └── Clock/
├── Behaviors/
├── Features/
│   ├── ContentProjects/
│   ├── ContentGeneration/
│   ├── Publications/
│   └── Analytics/
├── Mappings/
├── Validators/
└── DependencyInjection.cs
```

---

## Domain

```text
Domain/
├── ContentProjects/
├── ContentJobs/
├── Publications/
├── Analytics/
├── Events/
├── Exceptions/
└── Enums/
```

---

## Data

```text
Data/
├── Context/
│   └── InfiniteContentDbContext.cs
├── Configurations/
├── Migrations/
├── Repositories/
├── Queries/
├── Interceptors/
├── Seed/
└── DependencyInjection.cs
```

---

## Infrastructure

```text
Infrastructure/
├── Ai/
│   ├── OpenAI/
│   ├── Gemini/
│   └── Anthropic/
├── Publishing/
│   └── YouTube/
├── Storage/
├── Messaging/
├── Cache/
├── Media/
├── N8n/
├── Observability/
└── DependencyInjection.cs
```

---

## API

```text
Api/
├── Endpoints/
├── Middleware/
├── Extensions/
├── Filters/
├── Authentication/
└── Program.cs
```

---

## Worker

```text
Worker/
├── Jobs/
├── Consumers/
├── BackgroundServices/
├── Extensions/
└── Program.cs
```

---

# Program.cs da API

O arquivo `Program.cs` deve permanecer mínimo.

Exemplo conceitual:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddData(builder.Configuration)
    .AddInfrastructure(builder.Configuration)
    .AddPresentation();

var app = builder.Build();

app.UseApplicationPipeline();

app.Run();
```

As configurações devem ficar em métodos de extensão.

---

# Program.cs do Worker

```csharp
var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddApplication()
    .AddData(builder.Configuration)
    .AddInfrastructure(builder.Configuration)
    .AddWorkers();

var host = builder.Build();

await host.RunAsync();
```

---

# Registro de Dependências

## Data

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddData(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<InfiniteContentDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("Database"));
        });

        services.AddScoped<IContentProjectRepository, ContentProjectRepository>();

        return services;
    }
}
```

## Infrastructure

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IAiTextProvider, OpenAiTextProvider>();
        services.AddScoped<IPublishingProvider, YouTubePublishingProvider>();
        services.AddScoped<IStorageProvider, S3StorageProvider>();

        return services;
    }
}
```

---

# Responsabilidade de Cada Projeto

|Projeto|Responsabilidade|
|---|---|
|Api|Entrada HTTP e configuração da aplicação|
|Application|Casos de uso e abstrações|
|Domain|Regras de negócio|
|Data|Persistência e banco de dados|
|Infrastructure|Integrações externas|
|Worker|Processamento em segundo plano|
|Contracts|Contratos compartilhados|
|SharedKernel|Tipos fundamentais compartilhados|

---

# Convenções

- Um projeto por responsabilidade.
    
- Nenhuma regra de negócio na API.
    
- Nenhum acesso direto ao banco pela API.
    
- Nenhum acesso direto ao banco pelo Worker.
    
- Nenhuma dependência concreta na Application.
    
- Nenhum framework no Domain.
    
- Interfaces próximas de quem as utiliza.
    
- Métodos assíncronos com `CancellationToken`.
    
- Dependências registradas por extension methods.
    
- Testes organizados por funcionalidade.
    
- Segredos fora do repositório.
    
- Nomes claros e consistentes.
    
- Migrations sempre no projeto Data.
    
- Providers externos sempre no projeto Infrastructure.
    
- Repositórios concretos sempre no projeto Data.
    

---

# Decisão Arquitetural

A persistência será separada da infraestrutura geral.

## Data

Responsável por:

- PostgreSQL.
    
- Entity Framework Core.
    
- Migrations.
    
- Repositórios.
    
- Transações.
    

## Infrastructure

Responsável por:

- IA.
    
- YouTube.
    
- Storage.
    
- Cache.
    
- Mensageria.
    
- n8n.
    
- Serviços externos.
    

Essa separação torna a arquitetura mais explícita e facilita a manutenção, os testes e a substituição de tecnologias.

---

# Objetivo

Criar uma solution modular, testável e preparada para evolução, mantendo persistência, regras de negócio e integrações externas claramente separadas.