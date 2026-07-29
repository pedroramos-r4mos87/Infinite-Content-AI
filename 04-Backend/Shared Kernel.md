# Shared Kernel

## 1. Objetivo

O projeto `SharedKernel` conterá os conceitos técnicos e fundamentais compartilhados entre diferentes partes do Infinite Content AI.

Seu objetivo será fornecer primitivas estáveis que possam ser reutilizadas por:

- Domain.
    
- Application.
    
- Data.
    
- Infrastructure.
    
- API.
    
- Worker.
    

O Shared Kernel deverá permanecer pequeno, estável e independente de tecnologias externas.

Fluxo conceitual:

```text
Api ───────────────┐
Application ───────┤
Domain ────────────┤
Data ──────────────┼──> SharedKernel
Infrastructure ────┤
Worker ────────────┘
```

Ele deverá conter apenas elementos que realmente sejam compartilhados e que não pertençam claramente a uma feature ou camada específica.

---

# 2. Princípio Central

O Shared Kernel não será uma pasta de utilidades genéricas.

Um elemento somente poderá ser adicionado quando:

- For utilizado por mais de uma camada ou módulo.
    
- Possuir significado estável.
    
- Não pertencer claramente a uma feature.
    
- Não depender de frameworks externos.
    
- Não carregar regras específicas de negócio.
    
- Reduzir duplicação relevante.
    
- Melhorar a consistência arquitetural.
    

A regra principal será:

> Quando houver dúvida sobre colocar algo no Shared Kernel, ele provavelmente ainda não deve ir para lá.

---

# 3. Responsabilidades

O projeto poderá conter:

- Result Pattern.
    
- Error.
    
- ErrorType.
    
- Entity base.
    
- AggregateRoot base.
    
- Domain Event contract.
    
- Value Object base, se realmente necessário.
    
- Abstração de relógio.
    
- Primitivas de paginação.
    
- Identificadores técnicos genéricos.
    
- Pequenas abstrações arquiteturais estáveis.
    

O projeto não deverá conter:

- Entidades específicas.
    
- Commands.
    
- Queries.
    
- DTOs HTTP.
    
- Integration Events.
    
- Contratos RabbitMQ.
    
- Repositories.
    
- Serviços de infraestrutura.
    
- Helpers genéricos.
    
- Extensions aleatórias.
    
- Mapeamentos.
    
- Configurações.
    
- Tipos do EF Core.
    
- Tipos do ASP.NET Core.
    
- Tipos de SDKs externos.
    
- Regras específicas de Project, Pipeline ou Execution.
    

---

# 4. Dependências

O Shared Kernel não deverá depender de nenhum outro projeto da solution.

```text
SharedKernel
    ↓
nenhum projeto interno
```

Também deverá evitar dependências externas sempre que possível.

Não deverá depender diretamente de:

```text
ASP.NET Core
Entity Framework Core
MediatR
FluentValidation
RabbitMQ
Redis
Azure SDK
OpenAI SDK
Polly
Serilog
OpenTelemetry
```

Dependências do próprio runtime .NET são permitidas.

---

# 5. Estrutura do Projeto

Estrutura inicial recomendada:

```text
SharedKernel
│
├── Domain
│   ├── Entity.cs
│   ├── AggregateRoot.cs
│   ├── IDomainEvent.cs
│   └── ValueObject.cs
│
├── Results
│   ├── Result.cs
│   ├── ResultOfT.cs
│   ├── Error.cs
│   └── ErrorType.cs
│
├── Time
│   └── IClock.cs
│
├── Pagination
│   ├── PageRequest.cs
│   └── PaginatedResult.cs
│
└── Primitives
```

Para o MVP, somente as pastas realmente utilizadas deverão ser criadas.

---

# 6. Escopo do MVP

No primeiro momento, o Shared Kernel deverá conter apenas:

```text
Result
Result<T>
Error
ErrorType
Entity<TId>
AggregateRoot<TId>
IDomainEvent
IClock
PaginatedResult<T>
```

O projeto não deverá receber novas abstrações antes que uma necessidade concreta apareça durante a implementação.

---

# 7. Result Pattern

O `Result` representará o resultado de uma operação que pode falhar de forma esperada.

Exemplos de falhas esperadas:

- Entrada inválida.
    
- Recurso não encontrado.
    
- Estado incompatível.
    
- Conflito.
    
- Falta de autorização.
    
- Limite excedido.
    
- Timeout conhecido.
    
- Dependência temporariamente indisponível.
    

Operações sem retorno:

```csharp
public Result Archive(...)
```

Operações com retorno:

```csharp
public Result<Project> Create(...)
```

O objetivo é evitar o uso de exceções para controle de fluxo esperado.

---

# 8. ErrorType

Tipos iniciais:

```csharp
public enum ErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
    Forbidden = 5,
    RateLimit = 6,
    Timeout = 7,
    Unavailable = 8,
    Failure = 9
}
```

Significados:

## Validation

Entrada ou valor inválido.

## NotFound

Recurso não localizado dentro do escopo permitido.

## Conflict

Conflito de estado, concorrência ou idempotência.

## Unauthorized

Ausência ou falha de autenticação.

## Forbidden

Usuário autenticado sem permissão.

## RateLimit

Limite de requisições, uso ou quota excedido.

## Timeout

A operação excedeu o tempo permitido.

## Unavailable

Dependência temporariamente indisponível.

## Failure

Falha esperada que não pertence às categorias anteriores.

---

# 9. Error

Estrutura inicial:

```csharp
public sealed record Error(
    string Code,
    string Description,
    ErrorType Type)
{
    public static readonly Error None =
        new(
            string.Empty,
            string.Empty,
            ErrorType.None);

    public static Error Validation(
        string code,
        string description)
    {
        return new Error(
            code,
            description,
            ErrorType.Validation);
    }

    public static Error NotFound(
        string code,
        string description)
    {
        return new Error(
            code,
            description,
            ErrorType.NotFound);
    }

    public static Error Conflict(
        string code,
        string description)
    {
        return new Error(
            code,
            description,
            ErrorType.Conflict);
    }

    public static Error Unauthorized(
        string code,
        string description)
    {
        return new Error(
            code,
            description,
            ErrorType.Unauthorized);
    }

    public static Error Forbidden(
        string code,
        string description)
    {
        return new Error(
            code,
            description,
            ErrorType.Forbidden);
    }

    public static Error RateLimit(
        string code,
        string description)
    {
        return new Error(
            code,
            description,
            ErrorType.RateLimit);
    }

    public static Error Timeout(
        string code,
        string description)
    {
        return new Error(
            code,
            description,
            ErrorType.Timeout);
    }

    public static Error Unavailable(
        string code,
        string description)
    {
        return new Error(
            code,
            description,
            ErrorType.Unavailable);
    }

    public static Error Failure(
        string code,
        string description)
    {
        return new Error(
            code,
            description,
            ErrorType.Failure);
    }
}
```

---

# 10. Código do Erro

Todo erro deverá possuir um código estável.

Exemplos:

```text
Project.NameRequired
Project.NotFound
Pipeline.NotPublished
Execution.InvalidState
Execution.ConcurrencyConflict
AI.ProviderTimeout
AI.ProviderUnavailable
```

O código deverá ser utilizado por:

- API.
    
- Worker.
    
- Logs.
    
- Métricas.
    
- Testes.
    
- Clientes.
    
- Mensagens de falha.
    

A descrição poderá mudar.

O código não deverá mudar sem necessidade de compatibilidade.

---

# 11. Mensagem do Erro

A descrição deverá:

- Ser compreensível.
    
- Não expor implementação interna.
    
- Não conter secrets.
    
- Não conter stack trace.
    
- Não conter SQL.
    
- Não revelar dados de outro tenant.
    
- Não ser usada como identificador lógico.
    

Exemplo:

```csharp
Error.NotFound(
    "Project.NotFound",
    "O projeto informado não foi encontrado.");
```

---

# 12. Result sem Valor

Implementação conceitual:

```csharp
public class Result
{
    protected Result(
        bool isSuccess,
        Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException(
                "Um resultado de sucesso não pode possuir erro.");
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException(
                "Um resultado de falha deve possuir erro.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success()
    {
        return new Result(
            true,
            Error.None);
    }

    public static Result Failure(
        Error error)
    {
        return new Result(
            false,
            error);
    }
}
```

As exceções internas protegem o uso incorreto da própria primitiva.

Elas não representam falhas esperadas de negócio.

---

# 13. Result com Valor

```csharp
public sealed class Result<TValue>
    : Result
{
    private readonly TValue? _value;

    private Result(
        TValue? value,
        bool isSuccess,
        Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public TValue Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException(
                "O valor de um resultado com falha não pode ser acessado.");

    public static Result<TValue> Success(
        TValue value)
    {
        return new Result<TValue>(
            value,
            true,
            Error.None);
    }

    public static Result<TValue> Failure(
        Error error)
    {
        return new Result<TValue>(
            default,
            false,
            error);
    }

    public static implicit operator Result<TValue>(
        TValue value)
    {
        return Success(value);
    }
}
```

O operador implícito é opcional.

Se ele prejudicar a leitura ou causar ambiguidades, deverá ser removido.

---

# 14. Factories de Result

Poderão existir factories na classe base:

```csharp
public static Result<TValue> Success<TValue>(
    TValue value)
{
    return Result<TValue>.Success(value);
}

public static Result<TValue> Failure<TValue>(
    Error error)
{
    return Result<TValue>.Failure(error);
}
```

Uso:

```csharp
return Result.Success(project);
```

```csharp
return Result.Failure<Project>(
    ProjectErrors.NameRequired);
```

A API final deverá ser simples e consistente.

---

# 15. Match

Poderão ser adicionados métodos de composição.

```csharp
public TResult Match<TResult>(
    Func<TValue, TResult> onSuccess,
    Func<Error, TResult> onFailure)
{
    return IsSuccess
        ? onSuccess(Value)
        : onFailure(Error);
}
```

Uso na API:

```csharp
return result.Match(
    value => Results.Ok(value),
    ApiResults.Problem);
```

O `Match` poderá ser incluído no MVP por ser diretamente útil.

---

# 16. Bind e Map

Métodos funcionais poderão ser adicionados somente se forem realmente utilizados.

Exemplo de `Map`:

```csharp
public Result<TOutput> Map<TOutput>(
    Func<TValue, TOutput> mapper)
{
    return IsSuccess
        ? Result.Success(
            mapper(Value))
        : Result.Failure<TOutput>(
            Error);
}
```

Exemplo de `Bind`:

```csharp
public Result<TOutput> Bind<TOutput>(
    Func<TValue, Result<TOutput>> binder)
{
    return IsSuccess
        ? binder(Value)
        : Result.Failure<TOutput>(
            Error);
}
```

Não criar uma biblioteca funcional completa antes de existir necessidade.

---

# 17. Validation Errors

No MVP, erros de validação poderão utilizar o mesmo tipo `Error`.

Exemplo:

```csharp
Error.Validation(
    "Project.NameRequired",
    "O nome do projeto é obrigatório.");
```

Quando for necessário retornar múltiplos erros por campo, poderá ser criado posteriormente:

```csharp
public sealed record ValidationError(
    string Code,
    string Description,
    IReadOnlyDictionary<string, string[]> Errors)
    : Error(...);
```

Não implementar antes de o pipeline de validação realmente precisar dessa estrutura.

---

# 18. Entity

A classe base de Entity deverá representar somente identidade.

```csharp
public abstract class Entity<TId>
    where TId : notnull
{
    protected Entity(TId id)
    {
        Id = id;
    }

    public TId Id { get; protected init; }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return EqualityComparer<TId>
            .Default
            .Equals(Id, other.Id);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<TId>
            .Default
            .GetHashCode(Id);
    }

    public static bool operator ==(
        Entity<TId>? left,
        Entity<TId>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(
        Entity<TId>? left,
        Entity<TId>? right)
    {
        return !Equals(left, right);
    }
}
```

---

# 19. O que Entity não Deve Conter

A classe `Entity` não deverá possuir automaticamente:

```text
OrganizationId
CreatedAt
UpdatedAt
CreatedBy
UpdatedBy
Status
DeletedAt
Version
```

Essas propriedades não pertencem a todas as entidades.

Adicionar tudo à classe base cria acoplamento e dados sem significado.

Cada Entity deverá declarar apenas o que realmente faz parte de seu modelo.

---

# 20. AggregateRoot

```csharp
public abstract class AggregateRoot<TId>
    : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot(TId id)
        : base(id)
    {
    }

    public IReadOnlyCollection<IDomainEvent> DomainEvents =>
        _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(
        IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

A classe base deverá apenas:

- Manter a identidade.
    
- Registrar Domain Events.
    
- Expor eventos de forma somente leitura.
    
- Permitir limpeza após processamento.
    

---

# 21. IDomainEvent

```csharp
public interface IDomainEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredAt { get; }
}
```

O contrato não deverá depender de:

- MediatR.
    
- RabbitMQ.
    
- Contracts.
    
- OpenTelemetry.
    
- Serialização.
    
- Persistência.
    

Adapters externos poderão transformar `IDomainEvent` em outros tipos.

---

# 22. Domain Events

Exemplo de evento no projeto Domain:

```csharp
public sealed record ProjectCreatedDomainEvent(
    Guid EventId,
    ProjectId ProjectId,
    OrganizationId OrganizationId,
    UserId CreatedBy,
    DateTimeOffset OccurredAt)
    : IDomainEvent;
```

O Shared Kernel define apenas a interface.

Os eventos específicos permanecem próximos aos Aggregates que os produzem.

---

# 23. Limpeza de Domain Events

Os eventos deverão ser removidos somente após sua captura segura.

Fluxo conceitual:

```text
Aggregate gera eventos
    ↓
Data coleta eventos
    ↓
Outbox é criada
    ↓
SaveChanges concluído
    ↓
Eventos são limpos
```

O Shared Kernel não deverá decidir:

- Quando persistir.
    
- Como serializar.
    
- Como publicar.
    
- Qual evento se torna Integration Event.
    

---

# 24. ValueObject

Uma classe base poderá ser criada se reduzir código repetitivo real.

Exemplo:

```csharp
public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetAtomicValues();

    public override bool Equals(object? obj)
    {
        return obj is ValueObject other &&
               GetAtomicValues()
                   .SequenceEqual(
                       other.GetAtomicValues());
    }

    public override int GetHashCode()
    {
        return GetAtomicValues()
            .Aggregate(
                17,
                (current, value) =>
                    HashCode.Combine(
                        current,
                        value));
    }
}
```

Entretanto, records e record structs do C# já resolvem boa parte dos casos.

Para o MVP, deverá ser preferido:

```csharp
public sealed record ProjectName(string Value);
```

ou:

```csharp
public readonly record struct ProjectId(Guid Value);
```

A classe base `ValueObject` somente deverá ser criada quando houver necessidade concreta.

---

# 25. IClock

O relógio deverá ser uma abstração compartilhada.

```csharp
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
```

Implementação real:

```csharp
public sealed class SystemClock
    : IClock
{
    public DateTimeOffset UtcNow =>
        DateTimeOffset.UtcNow;
}
```

A interface fica no Shared Kernel.

A implementação pode ficar em Infrastructure ou ser registrada pelo host.

---

# 26. Por que Abstrair o Relógio

Evitar:

```csharp
DateTimeOffset.UtcNow
```

espalhado em:

- Domain.
    
- Application.
    
- Tests.
    
- Workers.
    
- Handlers.
    

Preferir:

```csharp
_clock.UtcNow
```

Benefícios:

- Testes determinísticos.
    
- Controle temporal.
    
- Simulação de expiração.
    
- Teste de retry.
    
- Teste de lease.
    
- Teste de idempotência.
    
- Padronização em UTC.
    

---

# 27. FakeClock

O Fake Clock deverá ficar nos projetos de teste, não no Shared Kernel de produção.

```csharp
public sealed class FakeClock
    : IClock
{
    public FakeClock(
        DateTimeOffset utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTimeOffset UtcNow { get; private set; }

    public void Advance(
        TimeSpan duration)
    {
        UtcNow = UtcNow.Add(duration);
    }
}
```

---

# 28. Paginação

Estrutura simples:

```csharp
public sealed record PageRequest(
    int Page,
    int PageSize);
```

Resultado:

```csharp
public sealed record PaginatedResult<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    long TotalCount)
{
    public int TotalPages =>
        PageSize <= 0
            ? 0
            : (int)Math.Ceiling(
                TotalCount /
                (double)PageSize);
}
```

Essa estrutura poderá ser utilizada pela Application e pelos adapters externos.

---

# 29. Onde Validar Paginação

O Shared Kernel não deverá definir regras específicas como:

```text
PageSize máximo = 100
```

Esses limites pertencem à Application ou API.

O Shared Kernel define somente a estrutura.

---

# 30. Identificadores

Identificadores específicos permanecem no Domain.

Exemplos:

```text
ProjectId
PipelineId
PipelineExecutionId
ArtifactId
```

Eles não deverão ficar no Shared Kernel apenas porque várias features os utilizam.

Cada identificador pertence ao conceito que representa.

---

# 31. Identificadores Técnicos Genéricos

O Shared Kernel poderá possuir identificadores realmente transversais quando houver necessidade.

Exemplos possíveis:

```text
CorrelationId
CausationId
```

Mesmo assim, no MVP poderão permanecer como strings ou GUIDs nos limites técnicos.

Não criar tipos apenas por preferência estética.

---

# 32. Primitives

A pasta `Primitives` poderá ser utilizada futuramente para tipos técnicos realmente compartilhados.

Exemplos possíveis:

```text
Optional<T>
DateRange
CursorToken
OperationId
```

Esses tipos não deverão ser criados antecipadamente.

Uma pasta vazia não precisa existir.

---

# 33. Extensions

Extensions não deverão ser colocadas automaticamente no Shared Kernel.

Evitar:

```text
StringExtensions
EnumerableExtensions
DateExtensions
ObjectExtensions
```

Uma extension somente deverá entrar se:

- Possuir semântica arquitetural.
    
- For usada em diversas camadas.
    
- Não depender de tecnologia externa.
    
- Não esconder comportamento importante.
    
- Permanecer pequena e previsível.
    

---

# 34. Constants

Constantes específicas deverão permanecer próximas ao conceito.

Exemplo:

```text
ProjectName.MaximumLength
```

não deverá virar:

```text
SharedConstants.ProjectNameMaximumLength
```

Constantes compartilhadas somente deverão ser centralizadas quando representarem um padrão realmente global.

---

# 35. Exceptions

O Shared Kernel poderá possuir exceções técnicas mínimas para proteger invariantes internas.

Exemplos possíveis:

```text
InvalidResultStateException
UnreachableStateException
```

Entretanto, `InvalidOperationException` poderá ser suficiente no MVP.

Não criar uma hierarquia de exceptions antes de existir necessidade.

Falhas esperadas continuam utilizando `Result`.

---

# 36. Guard Clauses

Uma biblioteca própria de Guard Clauses não será criada inicialmente.

Exemplo desnecessário:

```csharp
Guard.Against.Null(value);
Guard.Against.Empty(id);
Guard.Against.Negative(number);
```

O código poderá utilizar validações explícitas.

Isso mantém as regras visíveis e evita uma abstração genérica crescendo sem controle.

---

# 37. Specifications

Specifications específicas não deverão ficar no Shared Kernel.

Exemplos:

```text
PublishedPipelineSpecification
ExecutableProjectSpecification
ApprovedArtifactSpecification
```

Essas regras permanecem no Domain correspondente.

O Shared Kernel não deverá oferecer uma framework genérica de Specifications no MVP.

---

# 38. Policies

Policies de negócio não pertencem ao Shared Kernel.

Exemplos:

```text
CanRetryStepPolicy
PublicationEligibilityPolicy
ArtifactApprovalPolicy
```

Elas permanecem no Domain ou Application.

---

# 39. Result e Exceções Inesperadas

O Result não deverá capturar automaticamente qualquer exceção.

Evitar:

```csharp
public static async Task<Result<T>> TryAsync(...)
{
    try
    {
        return await operation();
    }
    catch (Exception)
    {
        return Result.Failure<T>(
            Error.Failure(...));
    }
}
```

Isso esconderia:

- Bugs.
    
- Falhas inesperadas.
    
- Estado corrompido.
    
- Problemas operacionais.
    
- Stack traces importantes.
    

Exceções inesperadas deverão seguir para o tratamento global.

---

# 40. Nullability

O projeto deverá utilizar nullable reference types.

No `.csproj`:

```xml
<Nullable>enable</Nullable>
```

Objetivos:

- Reduzir NullReferenceException.
    
- Tornar contratos explícitos.
    
- Melhorar análise estática.
    
- Evitar valores nulos implícitos.
    

---

# 41. Implicit Usings

Poderão ser habilitados:

```xml
<ImplicitUsings>enable</ImplicitUsings>
```

Entretanto, os arquivos deverão continuar claros sobre dependências não triviais.

---

# 42. Linguagem e Versão do .NET

O projeto deverá seguir a mesma versão do .NET utilizada pela solution.

Configuração sugerida:

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
</PropertyGroup>
```

O Target Framework final deverá acompanhar a decisão do projeto.

---

# 43. Warnings as Errors

O Shared Kernel deverá utilizar:

```xml
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
```

Por ser uma dependência central, seus contratos devem permanecer limpos.

O mesmo padrão poderá ser aplicado aos demais projetos gradualmente.

---

# 44. Namespaces

Namespaces sugeridos:

```csharp
InfiniteContentAI.SharedKernel.Results
```

```csharp
InfiniteContentAI.SharedKernel.Domain
```

```csharp
InfiniteContentAI.SharedKernel.Time
```

```csharp
InfiniteContentAI.SharedKernel.Pagination
```

Evitar:

```csharp
InfiniteContentAI.Common
InfiniteContentAI.Core.Utils
InfiniteContentAI.Helpers
```

---

# 45. Public API

Como muitas camadas dependerão do Shared Kernel, sua API pública deverá ser pequena.

Cada tipo público aumenta:

- Acoplamento.
    
- Custo de manutenção.
    
- Risco de quebra.
    
- Dificuldade de remoção.
    
- Dependência entre projetos.
    

Tipos deverão ser `internal` quando não precisarem atravessar o projeto.

---

# 46. Compatibilidade Interna

Mudanças no Shared Kernel podem afetar toda a solution.

Antes de alterar:

- Result.
    
- Error.
    
- Entity.
    
- AggregateRoot.
    
- IDomainEvent.
    
- IClock.
    

deverá ser avaliado:

- Quantos projetos dependem?
    
- A mudança é compatível?
    
- Existe migration de código?
    
- Os testes cobrem o comportamento?
    
- A abstração continua necessária?
    

---

# 47. Testes Unitários

O Shared Kernel deverá possuir testes para:

- Result de sucesso.
    
- Result de falha.
    
- Acesso ao Value.
    
- Match.
    
- Equality de Entity.
    
- Domain Events.
    
- PaginatedResult.
    
- Error factories.
    

---

# 48. Teste de Result

```csharp
[Fact]
public void Success_ShouldCreateSuccessfulResult()
{
    var result = Result.Success();

    result.IsSuccess.Should().BeTrue();
    result.IsFailure.Should().BeFalse();
    result.Error.Should().Be(Error.None);
}
```

---

# 49. Teste de Falha

```csharp
[Fact]
public void Failure_ShouldContainError()
{
    var error = Error.Validation(
        "Test.Validation",
        "Erro de validação.");

    var result = Result.Failure(error);

    result.IsFailure.Should().BeTrue();
    result.Error.Should().Be(error);
}
```

---

# 50. Teste de Result com Valor

```csharp
[Fact]
public void SuccessOfT_ShouldContainValue()
{
    var result = Result.Success(42);

    result.IsSuccess.Should().BeTrue();
    result.Value.Should().Be(42);
}
```

---

# 51. Teste de Acesso Inválido

```csharp
[Fact]
public void Value_ShouldThrow_WhenResultIsFailure()
{
    var result = Result.Failure<int>(
        Error.Failure(
            "Test.Failure",
            "Falha de teste."));

    var action = () => result.Value;

    action.Should()
        .Throw<InvalidOperationException>();
}
```

---

# 52. Teste de Entity

```csharp
[Fact]
public void EntitiesWithSameId_ShouldBeEqual()
{
    var id = Guid.CreateVersion7();

    var entityA = new TestEntity(id);
    var entityB = new TestEntity(id);

    entityA.Should().Be(entityB);
}
```

Duas Entities com IDs diferentes não deverão ser iguais, mesmo que os demais dados sejam idênticos.

---

# 53. Teste de Domain Events

```csharp
[Fact]
public void Aggregate_ShouldStoreDomainEvent()
{
    var aggregate = new TestAggregate();

    aggregate.ExecuteOperation();

    aggregate.DomainEvents.Should().ContainSingle();
}
```

Também testar:

```csharp
aggregate.ClearDomainEvents();
aggregate.DomainEvents.Should().BeEmpty();
```

---

# 54. Testes de Arquitetura

Regras:

- SharedKernel não depende de outros projetos.
    
- SharedKernel não depende de frameworks externos proibidos.
    
- Tipos de features não existem no SharedKernel.
    
- SharedKernel não possui classes chamadas Helper, Manager ou Service genérico.
    
- SharedKernel não possui DTOs HTTP.
    
- SharedKernel não possui Contracts distribuídos.
    

---

# 55. Antipadrões

## Pasta Common gigante

Evitar transformar o projeto em depósito.

## Helpers genéricos

Normalmente escondem falta de modelagem.

## DTOs compartilhados

Cada limite possui seu próprio contrato.

## Constantes de features

Permanecem próximas ao conceito.

## Dependência de framework

Shared Kernel deve ser independente.

## Value Object para toda primitiva

A abstração precisa trazer benefício.

## Classe base gigante

Entity e AggregateRoot devem permanecer mínimas.

## Auditoria automática em toda Entity

Nem toda entidade possui os mesmos dados.

## Result com HTTP Status Code

O Shared Kernel não conhece HTTP.

## Result com ação de RabbitMQ

O Shared Kernel não conhece mensageria.

## Error contendo Exception

Erros esperados não transportam exceções.

## Interface para tudo

Interfaces deverão representar limites reais.

---

# 56. Regras Arquiteturais

1. SharedKernel não depende de outros projetos.
    
2. SharedKernel evita dependências externas.
    
3. SharedKernel permanece pequeno.
    
4. Result representa falhas esperadas.
    
5. Exceções representam falhas inesperadas ou uso incorreto.
    
6. Error possui código estável.
    
7. Error não conhece HTTP.
    
8. Error não conhece RabbitMQ.
    
9. Entity representa apenas identidade.
    
10. AggregateRoot registra Domain Events.
    
11. Domain Events não dependem de MediatR.
    
12. IClock representa horário UTC.
    
13. Test doubles permanecem nos projetos de teste.
    
14. Identificadores de negócio permanecem no Domain.
    
15. Contratos distribuídos permanecem em Contracts.
    
16. DTOs permanecem em suas camadas.
    
17. Helpers genéricos não são permitidos.
    
18. ValueObject base é opcional.
    
19. Paginação não contém limites específicos da API.
    
20. Toda nova abstração precisa de uso concreto.
    
21. Tipos públicos devem ser mínimos.
    
22. Alterações devem considerar impacto em toda a solution.
    
23. Warnings devem ser tratados.
    
24. Nullable deve permanecer habilitado.
    
25. O projeto deve possuir testes próprios.
    

---

# 57. Definition of Done do Shared Kernel

O Shared Kernel estará pronto para o MVP quando possuir:

```text
Result
Result<T>
Error
ErrorType
Entity<TId>
AggregateRoot<TId>
IDomainEvent
IClock
PaginatedResult<T>
```

Além disso:

- Compila sem depender de outros projetos.
    
- Possui nullable habilitado.
    
- Possui warnings as errors.
    
- Possui testes do Result.
    
- Possui testes de Entity.
    
- Possui testes de Domain Events.
    
- Não contém código específico de features.
    
- Não contém SDKs.
    
- Não contém Helpers genéricos.
    
- Não contém contratos HTTP.
    
- Não contém Integration Events.
    
- Não contém persistência.
    

---

# 58. Ordem de Implementação

## Etapa 1 — Results

Criar:

```text
ErrorType
Error
Result
Result<T>
Match
```

## Etapa 2 — Domain Primitives

Criar:

```text
Entity<TId>
AggregateRoot<TId>
IDomainEvent
```

## Etapa 3 — Time

Criar:

```text
IClock
```

## Etapa 4 — Pagination

Criar:

```text
PaginatedResult<T>
```

## Etapa 5 — Tests

Criar testes unitários para todos os tipos.

Depois disso, parar.

Novos elementos deverão surgir a partir de uma necessidade real da implementação.

---

# 59. Critérios de Qualidade

O Shared Kernel será considerado saudável quando:

- Possuir poucas classes.
    
- Não conhecer frameworks.
    
- Não conhecer features.
    
- Result for utilizado de forma consistente.
    
- Errors possuírem códigos estáveis.
    
- Entities possuírem igualdade previsível.
    
- Aggregates puderem registrar eventos.
    
- Testes não dependerem de infraestrutura.
    
- Alterações forem raras.
    
- Nenhum desenvolvedor o utilizar como pasta de descarte.
    

---

# 60. Documentos Relacionados

```text
04 - Backend/Visão Geral do Backend.md
04 - Backend/Organização por Features.md
04 - Backend/Domain.md
04 - Backend/Application.md
04 - Backend/API.md
04 - Backend/Data.md
04 - Backend/Infrastructure.md
04 - Backend/Worker.md
04 - Backend/Contracts.md
04 - Backend/Plano de Implementação do MVP.md
```

---

# 61. Filosofia Final

O Shared Kernel deverá conter apenas o vocabulário técnico mínimo necessário para manter a arquitetura consistente.

Ele deverá oferecer conceitos como:

```text
Resultado
Erro
Entidade
Aggregate Root
Domain Event
Relógio
Paginação
```

Ele não deverá tentar resolver:

```text
Persistência
Mensageria
Autenticação
Validação de features
Integrações
Regras de negócio
Mapeamento
Configuração
Observabilidade
```

A regra principal será:

> O Shared Kernel deve ser pequeno o suficiente para ser compreendido por inteiro e estável o suficiente para raramente precisar mudar.

Se ele começar a crescer rapidamente, isso será um sinal de que responsabilidades específicas estão sendo colocadas no lugar errado.