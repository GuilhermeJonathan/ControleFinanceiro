# Referência de Arquitetura — CQRS com MediatR (.NET 8)

**Stack:** .NET 8 / C# 12  
**Padrão:** CQRS + MediatR + FluentValidation + Clean Architecture  
**Data:** Abril 2026

---

## Sumário

1. [Conceito CQRS](#1-conceito-cqrs)
2. [Estrutura de Pastas](#2-estrutura-de-pastas)
3. [Pacotes NuGet](#3-pacotes-nuget)
4. [Camada API](#4-camada-api)
5. [Camada Application](#5-camada-application)
6. [Camada Domain](#6-camada-domain)
7. [Camada Infrastructure](#7-camada-infrastructure)
8. [Pipeline Behaviors](#8-pipeline-behaviors)
9. [Injeção de Dependência](#9-injeção-de-dependência)
10. [Convenções e Boas Práticas](#10-convenções-e-boas-práticas)
11. [Fluxo Completo de uma Requisição](#11-fluxo-completo-de-uma-requisição)

---

## 1. Conceito CQRS

**CQRS (Command Query Responsibility Segregation)** separa as operações de escrita (**Commands**) das operações de leitura (**Queries**):

| Tipo    | Responsabilidade              | Retorno          | Exemplo                  |
|---------|-------------------------------|------------------|--------------------------|
| Command | Muda o estado do sistema      | void ou ID/status| `CreateUserCommand`      |
| Query   | Lê dados sem efeitos colaterais| DTO / lista      | `GetUserByIdQuery`       |

O **MediatR** atua como mediador: o Controller despacha um Command ou Query, e o Handler correspondente é invocado automaticamente via injeção de dependência.

**Fluxo:**
```
Controller → MediatR.Send(Command/Query) → Pipeline Behaviors → Handler → Repositório/DB
```

---

## 2. Estrutura de Pastas

```
Solution.sln
│
├── src/
│   ├── Projeto.Api/                        ← Controllers, Middleware, Program.cs
│   │   ├── Controllers/
│   │   │   └── UsersController.cs
│   │   ├── Middleware/
│   │   │   └── ExceptionHandlingMiddleware.cs
│   │   └── Program.cs
│   │
│   ├── Projeto.Application/                ← Commands, Queries, Handlers, Validators
│   │   ├── Common/
│   │   │   ├── Behaviors/
│   │   │   │   ├── ValidationBehavior.cs
│   │   │   │   └── LoggingBehavior.cs
│   │   │   └── Interfaces/
│   │   │       └── IUnitOfWork.cs
│   │   │
│   │   ├── Users/                          ← Módulo de Usuários (exemplo)
│   │   │   ├── Commands/
│   │   │   │   ├── CreateUser/
│   │   │   │   │   ├── CreateUserCommand.cs
│   │   │   │   │   ├── CreateUserCommandHandler.cs
│   │   │   │   │   └── CreateUserCommandValidator.cs
│   │   │   │   ├── UpdateUser/
│   │   │   │   │   ├── UpdateUserCommand.cs
│   │   │   │   │   ├── UpdateUserCommandHandler.cs
│   │   │   │   │   └── UpdateUserCommandValidator.cs
│   │   │   │   └── DeleteUser/
│   │   │   │       ├── DeleteUserCommand.cs
│   │   │   │       └── DeleteUserCommandHandler.cs
│   │   │   │
│   │   │   └── Queries/
│   │   │       ├── GetUserById/
│   │   │       │   ├── GetUserByIdQuery.cs
│   │   │       │   ├── GetUserByIdQueryHandler.cs
│   │   │       │   └── GetUserByIdQueryValidator.cs
│   │   │       └── GetUsers/
│   │   │           ├── GetUsersQuery.cs
│   │   │           └── GetUsersQueryHandler.cs
│   │   │
│   │   └── DependencyInjection.cs          ← Registro da camada Application
│   │
│   ├── Projeto.Domain/                     ← Entidades, Interfaces de repositório
│   │   ├── Entities/
│   │   │   └── User.cs
│   │   ├── Repositories/
│   │   │   └── IUserRepository.cs
│   │   └── Common/
│   │       └── Entity.cs                   ← Base entity com Id, CreatedAt, etc.
│   │
│   └── Projeto.Infrastructure/             ← EF Core, Repositórios, Migrations
│       ├── Persistence/
│       │   ├── AppDbContext.cs
│       │   ├── Configurations/
│       │   │   └── UserConfiguration.cs
│       │   ├── Migrations/
│       │   └── Repositories/
│       │       └── UserRepository.cs
│       └── DependencyInjection.cs          ← Registro da camada Infrastructure
│
└── tests/
    ├── Projeto.Application.Tests/
    │   └── Users/
    │       ├── Commands/
    │       │   └── CreateUserCommandHandlerTests.cs
    │       └── Queries/
    │           └── GetUserByIdQueryHandlerTests.cs
    └── Projeto.Api.Tests/
```

---

## 3. Pacotes NuGet

### Projeto.Api
```xml
<PackageReference Include="MediatR" Version="12.*" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.*" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.*" />
```

### Projeto.Application
```xml
<PackageReference Include="MediatR" Version="12.*" />
<PackageReference Include="FluentValidation" Version="11.*" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.*" />
```

### Projeto.Infrastructure
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.*" />
<PackageReference Include="Dapper" Version="2.*" />
```

---

## 4. Camada API

### Controller (thin — só despacha)

```csharp
// Controllers/UsersController.cs
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // QUERY — buscar usuário por ID
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    // QUERY — listar com filtros
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetUsersQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    // COMMAND — criar
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    // COMMAND — atualizar
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    // COMMAND — excluir
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteUserCommand(id), cancellationToken);
        return NoContent();
    }
}
```

---

## 5. Camada Application

### 5.1 Command + Handler + Validator

```csharp
// Users/Commands/CreateUser/CreateUserCommand.cs
public record CreateUserCommand(
    string Name,
    string Email,
    string Document,
    int UserTypeId
) : IRequest<Guid>;
```

```csharp
// Users/Commands/CreateUser/CreateUserCommandHandler.cs
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User(
            id: Guid.NewGuid(),
            name: request.Name,
            email: request.Email,
            document: request.Document,
            userTypeId: request.UserTypeId
        );

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
```

```csharp
// Users/Commands/CreateUser/CreateUserCommandValidator.cs
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.");

        RuleFor(x => x.Document)
            .NotEmpty().WithMessage("Documento é obrigatório.")
            .Length(11, 14).WithMessage("CPF/CNPJ inválido.");

        RuleFor(x => x.UserTypeId)
            .GreaterThan(0).WithMessage("Tipo de usuário inválido.");
    }
}
```

---

### 5.2 Query + Handler

```csharp
// Users/Queries/GetUserById/GetUserByIdQuery.cs
public record GetUserByIdQuery(Guid Id) : IRequest<UserDto?>;
```

```csharp
// Users/Queries/GetUserById/GetUserByIdQueryHandler.cs
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

        if (user is null) return null;

        return new UserDto(user.Id, user.Name, user.Email, user.Document);
    }
}
```

```csharp
// Users/Queries/GetUserById/UserDto.cs
public record UserDto(
    Guid Id,
    string Name,
    string Email,
    string Document
);
```

---

### 5.3 Query com filtros e paginação

```csharp
// Users/Queries/GetUsers/GetUsersQuery.cs
public record GetUsersQuery(
    string? Name,
    string? Email,
    int? UserTypeId,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<UserDto>>;
```

```csharp
// Common/PagedResult.cs
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
```

---

## 6. Camada Domain

### 6.1 Base Entity

```csharp
// Common/Entity.cs
public abstract class Entity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }

    protected Entity(Guid id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    protected void SetUpdated() => UpdatedAt = DateTime.UtcNow;
}
```

### 6.2 Entidade de Domínio

```csharp
// Entities/User.cs
public class User : Entity
{
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string Document { get; private set; }
    public int UserTypeId { get; private set; }
    public bool IsActive { get; private set; }

    // EF Core requer construtor privado sem parâmetros
    private User() : base(Guid.Empty) { }

    public User(Guid id, string name, string email, string document, int userTypeId)
        : base(id)
    {
        Name = name;
        Email = email;
        Document = document;
        UserTypeId = userTypeId;
        IsActive = true;
    }

    public void Update(string name, string email)
    {
        Name = name;
        Email = email;
        SetUpdated();
    }

    public void Deactivate() 
    {
        IsActive = false;
        SetUpdated();
    }
}
```

### 6.3 Interface de Repositório

```csharp
// Repositories/IUserRepository.cs
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    void Update(User user);
    void Remove(User user);
}
```

---

## 7. Camada Infrastructure

### 7.1 DbContext

```csharp
// Persistence/AppDbContext.cs
public class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await base.SaveChangesAsync(cancellationToken);
    }
}
```

### 7.2 Configuração de Entidade (EF Core Fluent API)

```csharp
// Persistence/Configurations/UserConfiguration.cs
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.Document)
            .IsRequired()
            .HasMaxLength(14);
    }
}
```

### 7.3 Repositório

```csharp
// Persistence/Repositories/UserRepository.cs
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Users.ToListAsync(cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        => await _context.Users.AddAsync(user, cancellationToken);

    public void Update(User user)
        => _context.Users.Update(user);

    public void Remove(User user)
        => _context.Users.Remove(user);
}
```

---

## 8. Pipeline Behaviors

Os behaviors são executados **antes e depois** de cada Handler, formando um pipeline de middlewares.

### 8.1 ValidationBehavior (FluentValidation automático)

```csharp
// Common/Behaviors/ValidationBehavior.cs
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
```

### 8.2 LoggingBehavior

```csharp
// Common/Behaviors/LoggingBehavior.cs
public class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("Handling {RequestName}: {@Request}", requestName, request);

        var response = await next();

        _logger.LogInformation("Handled {RequestName}", requestName);

        return response;
    }
}
```

---

## 9. Injeção de Dependência

### Application/DependencyInjection.cs

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // MediatR — escaneia todos os handlers na assembly
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            // Registra os pipeline behaviors (ordem importa!)
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // FluentValidation — escaneia todos os validators na assembly
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
```

### Infrastructure/DependencyInjection.cs

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
```

### Program.cs (.NET 8)

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Camadas
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## 10. Convenções e Boas Práticas

### Nomenclatura

| Tipo       | Convenção                         | Exemplo                        |
|------------|-----------------------------------|--------------------------------|
| Command    | `{Ação}{Entidade}Command`         | `CreateUserCommand`            |
| Query      | `Get{Entidade}By{Filtro}Query`    | `GetUserByIdQuery`             |
| Handler    | `{Command/Query}Handler`          | `CreateUserCommandHandler`     |
| Validator  | `{Command/Query}Validator`        | `CreateUserCommandValidator`   |
| DTO (saída)| `{Entidade}Dto`                   | `UserDto`                      |
| Repositório| `I{Entidade}Repository`           | `IUserRepository`              |

### Regras Gerais

1. **Controllers não têm lógica de negócio** — apenas despachamento via `_mediator.Send()`.
2. **Handlers não se chamam entre si** — se houver reuso, extraia para um serviço de domínio.
3. **Commands retornam o mínimo necessário** — preferencialmente `Guid` (ID criado) ou `Unit` (void).
4. **Queries nunca modificam estado** — sem `SaveChanges`, sem writes.
5. **Validators são colocados junto ao Command/Query** — um arquivo por operação.
6. **Entidades de domínio têm setters privados** — estado só muda por métodos de domínio.
7. **Use `record` para Commands e Queries** — são imutáveis por natureza.
8. **Um módulo por pasta** — agrupe `Commands/`, `Queries/` dentro de `Users/`, `Orders/`, etc.
9. **Nunca exponha entidades de domínio na API** — sempre mapeie para DTOs.
10. **`CancellationToken` em todos os métodos async** — propague do Controller ao Repositório.

---

## 11. Fluxo Completo de uma Requisição

```
POST /api/users
      │
      ▼
UsersController.Create(CreateUserCommand command)
      │ _mediator.Send(command)
      ▼
MediatR Pipeline
      │
      ├─► LoggingBehavior       ← loga o request
      │
      ├─► ValidationBehavior    ← executa CreateUserCommandValidator
      │       └─ se inválido → lança ValidationException
      │
      └─► CreateUserCommandHandler.Handle()
              │
              ├─ cria entidade User
              ├─ _userRepository.AddAsync(user)
              ├─ _unitOfWork.SaveChangesAsync()
              └─ retorna user.Id (Guid)
      │
      ▼
Controller recebe Guid → return CreatedAtAction(...)
      │
      ▼
Response 201 Created
Location: /api/users/{id}
```

---

## Referências

- [MediatR GitHub](https://github.com/jbogard/MediatR)
- [FluentValidation Docs](https://docs.fluentvalidation.net)
- [Clean Architecture (Jason Taylor template)](https://github.com/jasontaylordev/CleanArchitecture)
- [Microsoft: CQRS Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs)
