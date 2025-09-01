# 🚀 Clase 10: Proyecto Final - API de Gestión Empresarial

## 🧭 Navegación
- **⬅️ Anterior**: [Clase 9: Rate Limiting y Caching](clase_9_rate_limiting_caching.md)
- **🏠 Inicio del Módulo**: [Módulo 10: APIs REST y Web APIs](README.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 📚 Descripción

En esta clase implementarás un proyecto final completo que integra todos los conceptos aprendidos: una API de gestión empresarial con autenticación JWT, validación, rate limiting, caching, documentación Swagger y versionado.

## 🎯 Objetivos del Proyecto

- Implementar una API completa de gestión empresarial
- Integrar todos los conceptos del módulo
- Aplicar mejores prácticas de desarrollo
- Crear documentación completa y navegable

## 🏗️ Arquitectura del Proyecto

### Estructura de Carpetas
```
EnterpriseManagementAPI/
├── src/
│   ├── EnterpriseManagement.API/
│   ├── EnterpriseManagement.Core/
│   ├── EnterpriseManagement.Infrastructure/
│   └── EnterpriseManagement.Application/
├── tests/
│   ├── EnterpriseManagement.API.Tests/
│   └── EnterpriseManagement.Unit.Tests/
├── docs/
└── docker/
```

### Entidades del Dominio
```csharp
// EnterpriseManagement.Core/Entities/User.cs
public class User : IEntity, IAuditable, ISoftDelete
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

// EnterpriseManagement.Core/Entities/Department.cs
public class Department : IEntity, IAuditable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? ManagerId { get; set; }
    public User? Manager { get; set; }
    public List<User> Employees { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

// EnterpriseManagement.Core/Entities/Project.cs
public class Project : IEntity, IAuditable, ISoftDelete
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProjectStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal Budget { get; set; }
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public List<User> TeamMembers { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public string? DeletedBy { get; set; }
}

public enum ProjectStatus
{
    Planning,
    InProgress,
    OnHold,
    Completed,
    Cancelled
}
```

### DTOs y ViewModels
```csharp
// EnterpriseManagement.Application/DTOs/UserDtos.cs
public record CreateUserDto(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? Role = null);

public record UpdateUserDto(
    string? FirstName,
    string? LastName,
    string? Email,
    string? Role = null);

public record UserDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record LoginDto(string Email, string Password);

public record LoginResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User);

// EnterpriseManagement.Application/DTOs/ProjectDtos.cs
public record CreateProjectDto(
    string Name,
    string Description,
    DateTime StartDate,
    DateTime? EndDate,
    decimal Budget,
    int DepartmentId,
    List<int> TeamMemberIds);

public record UpdateProjectDto(
    string? Name,
    string? Description,
    ProjectStatus? Status,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal? Budget,
    int? DepartmentId,
    List<int>? TeamMemberIds);

public record ProjectDto(
    int Id,
    string Name,
    string Description,
    ProjectStatus Status,
    DateTime StartDate,
    DateTime? EndDate,
    decimal Budget,
    DepartmentDto Department,
    List<UserDto> TeamMembers,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
```

### Servicios de Aplicación
```csharp
// EnterpriseManagement.Application/Services/IUserService.cs
public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
    Task<bool> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
    Task<bool> DeleteUserAsync(int id);
    Task<LoginResponseDto> LoginAsync(LoginDto loginDto);
    Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<IEnumerable<UserDto>> GetUsersByDepartmentAsync(int departmentId);
    Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role);
}

// EnterpriseManagement.Application/Services/UserService.cs
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IJwtService jwtService,
        ICacheService cacheService,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var cacheKey = $"user:{id}";
        
        return await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user != null ? MapToUserDto(user) : null;
        }, TimeSpan.FromMinutes(30));
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        // Validar email único
        var existingUser = await _userRepository.GetByEmailAsync(createUserDto.Email);
        if (existingUser != null)
        {
            throw new ValidationException("El email ya está registrado");
        }

        var user = new User
        {
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            Email = createUserDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password),
            Role = createUserDto.Role ?? "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.AddAsync(user);
        var userDto = MapToUserDto(createdUser);

        // Invalidar cachés
        await InvalidateUserCaches(createdUser.Id);
        
        _logger.LogInformation("User created: {UserId}", createdUser.Id);
        return userDto;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _userRepository.GetByEmailAsync(loginDto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Credenciales inválidas");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Usuario inactivo");
        }

        var (accessToken, refreshToken, expiresAt) = await _jwtService.GenerateTokensAsync(user);
        
        _logger.LogInformation("User logged in: {UserId}", user.Id);
        
        return new LoginResponseDto(accessToken, refreshToken, expiresAt, MapToUserDto(user));
    }

    private async Task InvalidateUserCaches(int userId)
    {
        await _cacheService.RemoveAsync($"user:{userId}");
        await _cacheService.RemoveAsync("users:all");
        await _cacheService.RemoveByPatternAsync("users:search:*");
    }

    private static UserDto MapToUserDto(User user) => new(
        user.Id, user.FirstName, user.LastName, user.Email,
        user.Role, user.IsActive, user.CreatedAt, user.UpdatedAt);
}
```

### Controladores de API
```csharp
// EnterpriseManagement.API/Controllers/UsersController.cs
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[EnableRateLimiting("API")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    [EnableRateLimiting("Search")]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers(
        [FromQuery] string? search,
        [FromQuery] string? role,
        [FromQuery] int? departmentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var users = await _userService.GetAllUsersAsync();
        
        // Aplicar filtros
        if (!string.IsNullOrEmpty(search))
        {
            users = users.Where(u => 
                u.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                u.LastName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(role))
        {
            users = users.Where(u => u.Role.Equals(role, StringComparison.OrdinalIgnoreCase));
        }

        if (departmentId.HasValue)
        {
            users = await _userService.GetUsersByDepartmentAsync(departmentId.Value);
        }

        // Aplicar paginación
        var totalCount = users.Count();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        var pagedUsers = users.Skip((page - 1) * pageSize).Take(pageSize);

        Response.Headers.Add("X-Total-Count", totalCount.ToString());
        Response.Headers.Add("X-Total-Pages", totalPages.ToString());
        Response.Headers.Add("X-Current-Page", page.ToString());

        return Ok(pagedUsers);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
    {
        var user = await _userService.CreateUserAsync(createUserDto);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updateUserDto)
    {
        var result = await _userService.UpdateUserAsync(id, updateUserDto);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var result = await _userService.DeleteUserAsync(id);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}

// EnterpriseManagement.API/Controllers/AuthController.cs
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[EnableRateLimiting("Authentication")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginDto loginDto)
    {
        try
        {
            var response = await _userService.LoginAsync(loginDto);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponseDto>> RefreshToken([FromBody] string refreshToken)
    {
        try
        {
            var response = await _userService.RefreshTokenAsync(refreshToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }
}
```

### Configuración de Swagger
```csharp
// EnterpriseManagement.API/Program.cs
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Enterprise Management API",
        Version = "v1",
        Description = "API para gestión empresarial con autenticación JWT, rate limiting y caching",
        Contact = new OpenApiContact
        {
            Name = "Equipo de Desarrollo",
            Email = "dev@enterprise.com"
        }
    });

    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "Enterprise Management API",
        Version = "v2",
        Description = "Versión mejorada con nuevas funcionalidades",
        Contact = new OpenApiContact
        {
            Name = "Equipo de Desarrollo",
            Email = "dev@enterprise.com"
        }
    });

    // Configurar JWT
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Incluir comentarios XML
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

builder.Services.AddSwaggerGenNewtonsoftSupport();
```

### Configuración de Rate Limiting
```csharp
// EnterpriseManagement.API/Program.cs
builder.Services.AddRateLimiter(options =>
{
    // Política global
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 1000,
                Window = TimeSpan.FromMinutes(1)
            }));

    // Política para autenticación
    options.AddFixedWindowLimiter("Authentication", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 2;
    });

    // Política para API general
    options.AddFixedWindowLimiter("API", limiterOptions =>
    {
        limiterOptions.PermitLimit = 1000;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 10;
    });

    // Política para búsquedas
    options.AddFixedWindowLimiter("Search", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 5;
    });

    // Configurar respuesta de error
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        
        var response = new
        {
            error = "Too Many Requests",
            message = "Has excedido el límite de solicitudes. Intenta de nuevo más tarde.",
            retryAfter = 60,
            timestamp = DateTime.UtcNow
        };

        await context.HttpContext.Response.WriteAsJsonAsync(response, token);
    };
});
```

## 🧪 Testing

### Tests Unitarios
```csharp
// EnterpriseManagement.Unit.Tests/Services/UserServiceTests.cs
public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockJwtService = new Mock<IJwtService>();
        _mockCacheService = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<UserService>>();
        
        _userService = new UserService(
            _mockUserRepository.Object,
            _mockJwtService.Object,
            _mockCacheService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetUserByIdAsync_ExistingUser_ReturnsUserDto()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Role = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _mockCacheService.Setup(x => x.GetAsync<UserDto>(It.IsAny<string>()))
            .ReturnsAsync((UserDto?)null);
        
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal(user.FirstName, result.FirstName);
        Assert.Equal(user.LastName, result.LastName);
        Assert.Equal(user.Email, result.Email);
    }

    [Fact]
    public async Task CreateUserAsync_ValidUser_ReturnsUserDto()
    {
        // Arrange
        var createUserDto = new CreateUserDto(
            "John", "Doe", "john@example.com", "password123", "User");

        _mockUserRepository.Setup(x => x.GetByEmailAsync(createUserDto.Email))
            .ReturnsAsync((User?)null);

        var createdUser = new User
        {
            Id = 1,
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            Email = createUserDto.Email,
            Role = createUserDto.Role ?? "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _userService.CreateUserAsync(createUserDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdUser.Id, result.Id);
        Assert.Equal(createdUser.FirstName, result.FirstName);
        Assert.Equal(createdUser.LastName, result.LastName);
        Assert.Equal(createdUser.Email, result.Email);
    }

    [Fact]
    public async Task CreateUserAsync_DuplicateEmail_ThrowsValidationException()
    {
        // Arrange
        var createUserDto = new CreateUserDto(
            "John", "Doe", "john@example.com", "password123", "User");

        var existingUser = new User { Id = 1, Email = createUserDto.Email };

        _mockUserRepository.Setup(x => x.GetByEmailAsync(createUserDto.Email))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _userService.CreateUserAsync(createUserDto));
    }
}
```

### Tests de Integración
```csharp
// EnterpriseManagement.API.Tests/Controllers/UsersControllerTests.cs
public class UsersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public UsersControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetUsers_Unauthenticated_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/users");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_Authenticated_ReturnsOk()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var loginDto = new { email = "admin@example.com", password = "admin123" };
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        
        if (loginResponse.IsSuccessStatusCode)
        {
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
            return loginResult?.AccessToken ?? string.Empty;
        }

        return string.Empty;
    }
}
```

## 🚀 Despliegue

### Dockerfile
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["EnterpriseManagement.API/EnterpriseManagement.API.csproj", "EnterpriseManagement.API/"]
COPY ["EnterpriseManagement.Core/EnterpriseManagement.Core.csproj", "EnterpriseManagement.Core/"]
COPY ["EnterpriseManagement.Infrastructure/EnterpriseManagement.Infrastructure.csproj", "EnterpriseManagement.Infrastructure/"]
COPY ["EnterpriseManagement.Application/EnterpriseManagement.Application.csproj", "EnterpriseManagement.Application/"]
RUN dotnet restore "EnterpriseManagement.API/EnterpriseManagement.API.csproj"
COPY . .
WORKDIR "/src/EnterpriseManagement.API"
RUN dotnet build "EnterpriseManagement.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EnterpriseManagement.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EnterpriseManagement.API.dll"]
```

### docker-compose.yml
```yaml
version: '3.8'

services:
  api:
    build: .
    ports:
      - "5000:80"
      - "5001:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=db;Database=EnterpriseManagement;User Id=sa;Password=Your_password123;TrustServerCertificate=true
      - ConnectionStrings__Redis=redis:6379
    depends_on:
      - db
      - redis

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=Your_password123
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data

volumes:
  sqlserver_data:
  redis_data:
```

## 📊 Monitoreo y Métricas

### Health Checks
```csharp
// EnterpriseManagement.API/Program.cs
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddRedis(builder.Configuration.GetConnectionString("Redis"))
    .AddUrlGroup(new Uri("https://api.external.com"), "External API");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

### Métricas con Prometheus
```csharp
// EnterpriseManagement.API/Program.cs
builder.Services.AddMetrics();

app.UseMetricServer();
app.UseHttpMetrics();
```

## 🎯 Ejercicios del Proyecto

### Ejercicio 1: Implementación Básica
- Crea las entidades del dominio
- Implementa los repositorios básicos
- Crea los servicios de aplicación
- Implementa los controladores de API

### Ejercicio 2: Autenticación y Autorización
- Implementa JWT authentication
- Configura roles y políticas
- Agrega rate limiting
- Implementa refresh tokens

### Ejercicio 3: Validación y Manejo de Errores
- Implementa FluentValidation
- Crea middleware de manejo de excepciones
- Agrega logging estructurado
- Implementa health checks

### Ejercicio 4: Caching y Optimización
- Implementa caché en memoria
- Agrega caché distribuido con Redis
- Optimiza consultas de base de datos
- Implementa compresión de respuestas

### Ejercicio 5: Testing y Documentación
- Escribe tests unitarios
- Implementa tests de integración
- Configura Swagger con ejemplos
- Documenta la API completamente

## 📝 Checklist de Completado

- [ ] Estructura del proyecto creada
- [ ] Entidades del dominio implementadas
- [ ] DTOs y ViewModels creados
- [ ] Servicios de aplicación implementados
- [ ] Controladores de API creados
- [ ] Autenticación JWT configurada
- [ ] Rate limiting implementado
- [ ] Caching configurado
- [ ] Validación implementada
- [ ] Manejo de errores configurado
- [ ] Swagger documentado
- [ ] Tests unitarios escritos
- [ ] Tests de integración implementados
- [ ] Docker configurado
- [ ] Health checks implementados
- [ ] Métricas configuradas

## 🎉 ¡Felicidades!

Has completado el **Módulo 10: APIs REST y Web APIs** del curso de C# Senior. 

En este módulo has aprendido:
- ✅ Fundamentos de APIs REST
- ✅ Configuración de ASP.NET Core Web API
- ✅ Controladores y endpoints
- ✅ Validación y manejo de errores
- ✅ Autenticación y autorización JWT
- ✅ Entity Framework Core
- ✅ Documentación con Swagger
- ✅ Versionado de APIs
- ✅ Rate limiting y caching
- ✅ Proyecto final integrador

## 🚀 Próximos Pasos

Continúa con el siguiente módulo para expandir tus conocimientos en:
- **Módulo 11**: Arquitectura de Microservicios Avanzada
- **Módulo 12**: DevOps y CI/CD para .NET

---

**💡 Consejo**: Este proyecto final demuestra las mejores prácticas para construir APIs empresariales robustas y escalables. Úsalo como base para tus propios proyectos profesionales.
