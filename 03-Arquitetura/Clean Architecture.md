# Clean Architecture

## Objetivo

Garantir que o Infinite Content AI permaneça modular, testável, escalável e independente de tecnologias específicas.

A arquitetura deve permitir trocar banco de dados, provedores de IA, plataformas de publicação e outras dependências externas sem impactar as regras de negócio.

---

# Princípios Fundamentais

## Independência

As regras de negócio não conhecem detalhes técnicos.

O domínio deve continuar funcionando mesmo que:

- PostgreSQL seja trocado por SQL Server.
    
- OpenAI seja trocada por Gemini.
    
- Entity Framework seja substituído.
    
- A API REST vire GraphQL.
    
- O n8n seja removido.
    

---

## Dependências apontam para dentro

A direção das dependências sempre será:

```text
API
     │
Worker
     │
Data
     │
Infrastructure
     │
Application
     │
Domain
```

Quem está acima pode conhecer quem está abaixo.

Nunca o contrário.

---

# Responsabilidade de cada camada

## Domain

É o coração do sistema.

Contém:

- Entidades
    
- Value Objects
    
- Regras de negócio
    
- Eventos
    
- Exceções
    
- Enums
    

Nunca deve conhecer:

- Entity Framework
    
- PostgreSQL
    
- OpenAI
    
- ASP.NET
    
- Docker
    
- n8n
    
- JSON
    
- APIs externas
    

---

## Application

Representa os casos de uso.

Contém:

- Commands
    
- Queries
    
- Handlers
    
- Interfaces
    
- Validações
    
- Orquestração
    

Conhece apenas abstrações.

Nunca conhece implementações.

---

## Data

Implementa persistência.

Contém:

- DbContext
    
- Repositórios
    
- Migrations
    
- Configurações do EF Core
    
- Queries
    

Nunca contém regra de negócio.

---

## Infrastructure

Implementa serviços externos.

Exemplos:

- OpenAI
    
- Gemini
    
- Anthropic
    
- YouTube
    
- Redis
    
- S3
    
- n8n
    
- SMTP
    

Nunca contém regra de negócio.

---

## API

É apenas uma porta de entrada.

Ela recebe uma requisição e chama um caso de uso.

Nada mais.

---

## Worker

Executa processos demorados.

Não contém regra de negócio.

Ele apenas dispara casos de uso da Application.

---

# Regra de Ouro

Sempre pergunte:

> Isso é uma regra de negócio?

Se a resposta for SIM...

Provavelmente pertence ao Domain ou à Application.

Se NÃO...

Provavelmente pertence ao Data ou Infrastructure.

---

# Interfaces

As interfaces pertencem à camada que precisa delas.

Exemplo:

```csharp
public interface IAiTextProvider
```

Essa interface pertence à Application.

Quem implementa?

```text
OpenAiTextProvider

GeminiTextProvider

AnthropicTextProvider
```

Essas implementações pertencem ao projeto Infrastructure.

---

Outro exemplo:

```csharp
public interface IContentProjectRepository
```

Ela pertence à Application.

Quem implementa?

```text
ContentProjectRepository
```

Essa implementação pertence ao projeto Data.

---

# Fluxo de uma requisição

```text
Cliente

↓

API

↓

Application

↓

Domain

↓

Repository Interface

↓

Repository (Data)

↓

PostgreSQL
```

---

# Fluxo de geração por IA

```text
Application

↓

IAiTextProvider

↓

OpenAiProvider

↓

OpenAI
```

A Application nunca conhece OpenAI diretamente.

---

# Regra para Controllers

Controllers devem possuir apenas:

- Receber Request
    
- Chamar caso de uso
    
- Retornar Response
    

Nunca:

- Validar negócio
    
- Acessar banco
    
- Chamar OpenAI
    
- Criar entidades
    

---

# Regra para Repositórios

Repositórios apenas persistem.

Nunca:

- Geram conteúdo
    
- Chamam IA
    
- Publicam vídeos
    
- Enviam e-mails
    

---

# Regra para Providers

Providers apenas conversam com serviços externos.

Nunca:

- Decidem regras
    
- Fazem orquestração
    
- Persistem dados
    

---

# Regra para Workers

Workers apenas executam tarefas.

Eles não decidem regras.

Eles apenas chamam casos de uso.

---

# Injeção de Dependência

Toda implementação será registrada via Extension Methods.

Exemplo:

```csharp
builder.Services
    .AddApplication()
    .AddData(configuration)
    .AddInfrastructure(configuration);
```

Nenhuma configuração grande deverá ficar no Program.cs.

---

# Program.cs

Deve permanecer extremamente pequeno.

Nosso objetivo é que ele tenha poucas linhas.

Toda configuração ficará em:

```text
DependencyInjection.cs
```

de cada projeto.

---

# Convenções

## Async

Toda operação I/O será assíncrona.

Sempre utilizar:

- async
    
- await
    
- CancellationToken
    

---

## Result Pattern

Evitar exceptions para fluxo esperado.

Preferir:

```csharp
Result.Success()

Result.Failure()
```

---

## Logging

Utilizar logs estruturados.

Nunca:

```csharp
Console.WriteLine()
```

---

## Configuração

Nunca utilizar valores fixos.

Tudo deverá vir de:

- appsettings
    
- Environment Variables
    
- Secret Manager
    

---

# Testabilidade

Toda regra importante deve ser testável.

Preferimos:

```
Interface

↓

Mock

↓

Teste
```

ao invés de depender de implementações concretas.

---

# Extensibilidade

Adicionar um novo provider deve exigir apenas:

1. Criar implementação.
    
2. Registrar DI.
    

Nada mais.

Exemplo:

Hoje:

- OpenAI
    

Amanhã:

- Claude
    

Depois:

- Mistral
    

Nenhum Handler deverá ser alterado.

---

# Objetivo Final

Queremos que o Infinite Content AI continue organizado mesmo quando possuir:

- dezenas de agentes;
    
- centenas de casos de uso;
    
- múltiplos bancos;
    
- vários provedores de IA;
    
- múltiplas plataformas de publicação.
    

A arquitetura deve permitir evolução contínua sem grandes refatorações.