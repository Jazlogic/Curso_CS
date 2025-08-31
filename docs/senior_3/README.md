# 🏆 Senior Level 3: APIs REST y Web APIs

## 🧭 Navegación del Curso

**← Anterior**: [Senior Level 2: Testing y TDD](../senior_2/README.md)  
**Siguiente →**: [Senior Level 4: Entity Framework](../senior_4/README.md)

---

## 📚 Descripción

En este nivel aprenderás a crear APIs RESTful profesionales usando ASP.NET Core, implementando mejores prácticas, autenticación, autorización y documentación. Este es un paso crucial para convertirte en un desarrollador backend senior.

## 🎯 Objetivos de Aprendizaje

- Crear APIs RESTful con ASP.NET Core
- Implementar autenticación y autorización JWT
- Usar Entity Framework Core para acceso a datos
- Implementar validación y manejo de errores
- Crear documentación de API con Swagger
- Implementar versionado de APIs
- Crear APIs escalables y mantenibles

## 📖 Contenido Teórico

### 1. Fundamentos de APIs REST

#### ¿Qué es REST?
REST (Representational State Transfer) es un estilo de arquitectura para sistemas distribuidos que se basa en el protocolo HTTP.

#### Principios REST
- **Stateless**: Cada request debe contener toda la información necesaria
- **Client-Server**: Separación clara entre cliente y servidor
- **Cacheable**: Las respuestas deben ser cacheables
- **Uniform Interface**: Interfaz consistente y predecible
- **Layered System**: Arquitectura en capas
- **Code on Demand**: El servidor puede enviar código ejecutable al cliente

#### Verbos HTTP
```csharp
// GET - Obtener recursos
GET /api/users          // Obtener todos los usuarios
GET /api/users/1        // Obtener usuario específico

// POST - Crear nuevos recursos
POST /api/users         // Crear nuevo usuario

// PUT - Actualizar recursos completos
PUT /api/users/1        // Actualizar usuario completo

// PATCH - Actualizar recursos parcialmente
PATCH /api/users/1      // Actualizar solo algunos campos

// DELETE - Eliminar recursos
DELETE /api/users/1     // Eliminar usuario
```

### 2. Configuración de ASP.NET Core Web API

#### Estructura del Proyecto
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Agregar servicios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configurar Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configurar pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
```

#### Configuración de appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MyApiDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-with-at-least-32-characters",
    "Issuer": "YourApi",
    "Audience": "YourApiUsers",
    "ExpirationInMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### 3. Controladores y Endpoints

#### Controlador Básico
```csharp
[ApiController]
[Route("api/[controller]")]
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
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuarios");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userService.CreateUserAsync(createUserDto);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updateUserDto)
    {
        if (id != updateUserDto.Id)
            return BadRequest();

        var result = await _userService.UpdateUserAsync(updateUserDto);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var result = await _userService.DeleteUserAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
```

#### DTOs (Data Transfer Objects)
```csharp
public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateUserDto
{
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(50)]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; }
}

public class UpdateUserDto
{
    public int Id { get; set; }
    
    [StringLength(50)]
    public string FirstName { get; set; }
    
    [StringLength(50)]
    public string LastName { get; set; }
    
    [EmailAddress]
    public string Email { get; set; }
}
```

### 4. Validación y Manejo de Errores

#### Validación con Data Annotations
```csharp
public class CreateProductDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Name { get; set; }

    [Required(ErrorMessage = "La descripción es obligatoria")]
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string Description { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
    public int Stock { get; set; }

    [Url(ErrorMessage = "La URL de la imagen no es válida")]
    public string ImageUrl { get; set; }
}
```

#### Middleware de Manejo de Errores
```csharp
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no manejado");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            error = new
            {
                message = "Ha ocurrido un error interno del servidor",
                details = exception.Message
            }
        };

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(response);
    }
}

// Registro en Program.cs
app.UseMiddleware<ErrorHandlingMiddleware>();
```

#### Filtros de Acción
```csharp
public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .Select(x => new
                {
                    field = x.Key,
                    errors = x.Value.Errors.Select(e => e.ErrorMessage)
                })
                .ToList();

            context.Result = new BadRequestObjectResult(new
            {
                message = "Error de validación",
                errors = errors
            });
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Lógica después de la ejecución
    }
}

// Registro en Program.cs
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});
```

### 5. Autenticación y Autorización JWT

#### Configuración de JWT
```csharp
public class JwtSettings
{
    public string SecretKey { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int ExpirationInMinutes { get; set; }
}

// Program.cs
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

builder.Services.AddAuthorization();
```

#### Servicio de JWT
```csharp
public interface IJwtService
{
    string GenerateToken(User user);
    bool ValidateToken(string token);
}

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;

    public JwtService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
```

#### Controlador de Autenticación
```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;

    public AuthController(IUserService userService, IJwtService jwtService)
    {
        _userService = userService;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginDto loginDto)
    {
        var user = await _userService.ValidateUserAsync(loginDto.Email, loginDto.Password);
        if (user == null)
            return Unauthorized(new { message = "Credenciales inválidas" });

        var token = _jwtService.GenerateToken(user);
        
        return Ok(new LoginResponseDto
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            }
        });
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(CreateUserDto createUserDto)
    {
        var existingUser = await _userService.GetUserByEmailAsync(createUserDto.Email);
        if (existingUser != null)
            return BadRequest(new { message = "El email ya está registrado" });

        var user = await _userService.CreateUserAsync(createUserDto);
        return CreatedAtAction(nameof(Register), new { id = user.Id }, user);
    }
}
```

### 6. Entity Framework Core

#### Configuración del Contexto
```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuración de User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PasswordHash).IsRequired();
        });

        // Configuración de Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
        });

        // Configuración de Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Orders)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
```

#### Repositorios Genéricos
```csharp
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<bool> ExistsAsync(int id);
}

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.FindAsync(id) != null;
    }
}
```

### 7. Documentación con Swagger

#### Configuración de Swagger
```csharp
// Program.cs
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Mi API",
        Version = "v1",
        Description = "Una API de ejemplo",
        Contact = new OpenApiContact
        {
            Name = "Tu Nombre",
            Email = "tu@email.com"
        }
    });

    // Configurar autenticación JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });

    // Incluir comentarios XML
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
```

#### Documentación de Endpoints
```csharp
/// <summary>
/// Obtiene todos los usuarios
/// </summary>
/// <returns>Lista de usuarios</returns>
/// <response code="200">Retorna la lista de usuarios</response>
/// <response code="500">Error interno del servidor</response>
[HttpGet]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
{
    // Implementación
}

/// <summary>
/// Crea un nuevo usuario
/// </summary>
/// <param name="createUserDto">Datos del usuario a crear</param>
/// <returns>Usuario creado</returns>
/// <response code="201">Usuario creado exitosamente</response>
/// <response code="400">Datos de entrada inválidos</response>
/// <response code="409">El email ya está registrado</response>
[HttpPost]
[ProducesResponseType(StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status409Conflict)]
public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
{
    // Implementación
}
```

### 8. Versionado de APIs

#### Configuración de Versionado
```csharp
// Program.cs
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
```

#### Controladores con Versionado
```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsersController : ControllerBase
{
    // Endpoints v1.0
}

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsersV2Controller : ControllerBase
{
    // Endpoints v2.0 con funcionalidades adicionales
}
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: API de Usuarios Básica
Crea una API REST para gestionar usuarios con operaciones CRUD básicas.

### Ejercicio 2: Sistema de Autenticación JWT
Implementa un sistema completo de autenticación con JWT, incluyendo login, registro y protección de endpoints.

### Ejercicio 3: API de Productos con Validación
Crea una API para productos con validación robusta, manejo de errores y filtros de búsqueda.

### Ejercicio 4: Sistema de Pedidos
Implementa un sistema de pedidos con relaciones entre usuarios, productos y pedidos usando Entity Framework.

### Ejercicio 5: Middleware Personalizado
Crea middleware personalizado para logging, rate limiting y manejo de errores.

### Ejercicio 6: Filtros de Acción
Implementa filtros para validación, autorización y logging de acciones.

### Ejercicio 7: Documentación con Swagger
Configura Swagger con autenticación JWT y documenta todos los endpoints.

### Ejercicio 8: Versionado de API
Implementa versionado de API con diferentes funcionalidades en cada versión.

### Ejercicio 9: Testing de API
Crea tests unitarios e integración para todos los endpoints de la API.

### Ejercicio 10: Proyecto Integrador - E-commerce API
Crea una API completa de e-commerce que incluya:
- Gestión de usuarios y autenticación
- Catálogo de productos con categorías
- Sistema de pedidos y carrito de compras
- Sistema de pagos (simulado)
- Notificaciones por email
- Reportes y estadísticas
- Documentación completa con Swagger

## 📝 Quiz de Autoevaluación

1. ¿Cuáles son los principios fundamentales de REST?
2. ¿Cuándo usarías PUT vs PATCH en una API?
3. ¿Qué ventajas tiene usar JWT para autenticación?
4. ¿Por qué es importante la validación en el servidor?
5. ¿Cómo implementarías rate limiting en una API?

## 🚀 Siguiente Nivel

Una vez que hayas completado todos los ejercicios y comprendas los conceptos, estarás listo para el **Senior Level 4: Entity Framework y Bases de Datos**.

## 💡 Consejos de Estudio

- Practica creando APIs para diferentes dominios
- Experimenta con diferentes estrategias de autenticación
- Implementa validación robusta en todos los endpoints
- Usa Entity Framework para crear modelos de datos complejos
- Documenta tus APIs como si fueran para uso en producción

¡Estás desarrollando habilidades de arquitecto de APIs! 🚀
