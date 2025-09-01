# 🚀 Clase 2: Configuración ASP.NET Core Web API

## 🧭 Navegación
- **⬅️ Anterior**: [Clase 1: Fundamentos de APIs REST](clase_1_fundamentos_apis_rest.md)
- **🏠 Inicio del Módulo**: [Módulo 10: APIs REST y Web APIs](README.md)
- **➡️ Siguiente**: [Clase 3: Controladores y Endpoints](clase_3_controladores_endpoints.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 📚 Descripción

En esta clase aprenderás a configurar un proyecto ASP.NET Core Web API desde cero, incluyendo la estructura del proyecto, configuración de servicios, middleware y archivos de configuración.

## 🎯 Objetivos de Aprendizaje

- Crear un proyecto ASP.NET Core Web API
- Configurar servicios y dependencias
- Entender el pipeline de middleware
- Configurar CORS y políticas de seguridad
- Configurar Entity Framework Core
- Configurar logging y monitoreo

## 📖 Contenido Teórico

### Estructura del Proyecto

#### Creación del Proyecto
```bash
# Crear nuevo proyecto Web API
dotnet new webapi -n MyApi

# Navegar al directorio
cd MyApi

# Restaurar dependencias
dotnet restore
```

#### Estructura de Carpetas Recomendada
```
MyApi/
├── Controllers/           # Controladores de la API
├── Models/               # Modelos de dominio y DTOs
├── Services/             # Lógica de negocio
├── Data/                 # Contexto de Entity Framework
├── Middleware/           # Middleware personalizado
├── Extensions/           # Extensiones de configuración
├── Configuration/        # Clases de configuración
├── Program.cs            # Punto de entrada de la aplicación
├── appsettings.json     # Configuración de la aplicación
└── appsettings.Development.json
```

### Configuración del Program.cs

#### Program.cs Básico
```csharp
var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configurar pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

#### Program.cs Completo con Configuraciones
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MyApi.Data;
using MyApi.Services;
using MyApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor
builder.Services.AddControllers(options =>
{
    // Configurar opciones de controladores
    options.SuppressAsyncSuffixInActionNames = false;
});

// Configurar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Mi API",
        Version = "v1",
        Description = "Una API de ejemplo con ASP.NET Core",
        Contact = new OpenApiContact
        {
            Name = "Tu Nombre",
            Email = "tu@email.com"
        }
    });
});

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    options.AddPolicy("AllowSpecific", policy =>
    {
        policy.WithOrigins("https://localhost:3000", "https://myapp.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configurar Entity Framework Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar servicios de aplicación
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Configurar logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
    logging.AddEventSourceLogger();
});

var app = builder.Build();

// Configurar pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mi API v1");
        c.RoutePrefix = string.Empty; // Swagger en la raíz
    });
}

// Middleware de manejo de errores personalizado
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();

// Configurar CORS
app.UseCors("AllowAll");

// Middleware de autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Mapear controladores
app.MapControllers();

// Endpoint de health check
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

app.Run();
```

### Configuración de appsettings.json

#### appsettings.json Básico
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

#### appsettings.json Completo
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MyApiDb;Trusted_Connection=true;MultipleActiveResultSets=true",
    "ProductionConnection": "Server=prod-server;Database=MyApiDb;User Id=myuser;Password=mypassword;"
  },
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-with-at-least-32-characters",
    "Issuer": "MyApi",
    "Audience": "MyApiUsers",
    "ExpirationInMinutes": 60,
    "RefreshTokenExpirationInDays": 7
  },
  "CorsSettings": {
    "AllowedOrigins": [
      "https://localhost:3000",
      "https://myapp.com"
    ],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE", "PATCH"],
    "AllowedHeaders": ["*"]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    },
    "Console": {
      "IncludeScopes": true
    },
    "File": {
      "Path": "logs/app.log",
      "LogLevel": {
        "Default": "Information"
      }
    }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/app-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ]
  },
  "AllowedHosts": "*",
  "Environment": "Development"
}
```

#### appsettings.Development.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MyApiDb_Dev;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "DetailedErrors": true,
  "Environment": "Development"
}
```

### Configuración de Servicios

#### Configuración de Entity Framework
```csharp
// Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    });
    
    // Habilitar logging de consultas en desarrollo
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Configurar repositorios
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
```

#### Configuración de Autenticación JWT
```csharp
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
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
        
        // Configurar eventos
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Add("Token-Expired", "true");
                }
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                
                var result = JsonSerializer.Serialize(new
                {
                    error = "No autorizado",
                    message = "Token de autenticación requerido"
                });
                
                return context.Response.WriteAsync(result);
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
    
    options.AddPolicy("UserOrAdmin", policy =>
        policy.RequireRole("User", "Admin"));
});
```

#### Configuración de CORS Avanzada
```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    // Política para desarrollo
    options.AddPolicy("Development", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    // Política para producción
    options.AddPolicy("Production", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>();
        var allowedMethods = builder.Configuration.GetSection("CorsSettings:AllowedMethods").Get<string[]>();
        var allowedHeaders = builder.Configuration.GetSection("CorsSettings:AllowedHeaders").Get<string[]>();

        policy.WithOrigins(allowedOrigins)
              .WithMethods(allowedMethods)
              .WithHeaders(allowedHeaders)
              .AllowCredentials();
    });
});

// En el pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseCors("Development");
}
else
{
    app.UseCors("Production");
}
```

### Configuración de Middleware

#### Middleware de Manejo de Errores
```csharp
// Middleware/ErrorHandlingMiddleware.cs
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no manejado en la aplicación");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            error = new
            {
                message = "Ha ocurrido un error interno del servidor",
                details = _environment.IsDevelopment() ? exception.Message : "Error interno",
                timestamp = DateTime.UtcNow
            }
        };

        context.Response.StatusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}
```

#### Middleware de Logging de Requests
```csharp
// Middleware/RequestLoggingMiddleware.cs
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;

        _logger.LogInformation("Iniciando request {Method} {Path}", requestMethod, requestPath);

        try
        {
            await _next(context);
        }
        finally
        {
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;
            var statusCode = context.Response.StatusCode;

            _logger.LogInformation(
                "Request {Method} {Path} completado con código {StatusCode} en {Duration}ms",
                requestMethod, requestPath, statusCode, duration.TotalMilliseconds);
        }
    }
}
```

### Configuración de Logging

#### Configuración con Serilog
```csharp
// Program.cs
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day)
    .WriteTo.Seq("http://localhost:5341"));
```

#### Configuración de Logging Estructurado
```csharp
// Program.cs
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
    
    // Configurar logging estructurado
    logging.AddJsonConsole(options =>
    {
        options.JsonWriterOptions = new JsonWriterOptions
        {
            Indented = true
        };
    });
    
    // Configurar filtros
    logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
    logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
});
```

### Configuración de Health Checks

#### Configuración Básica
```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddCheck("External API", () =>
    {
        // Verificar API externa
        return HealthCheckResult.Healthy();
    });

// En el pipeline HTTP
app.MapHealthChecks("/health");
```

#### Configuración Avanzada
```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("Database")
    .AddCheck("External API", () =>
    {
        try
        {
            // Verificar API externa
            return HealthCheckResult.Healthy("API externa disponible");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("API externa no disponible", ex);
        }
    }, tags: new[] { "external" })
    .AddCheck("File System", () =>
    {
        try
        {
            var tempPath = Path.GetTempPath();
            var testFile = Path.Combine(tempPath, "health-check-test.tmp");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
            return HealthCheckResult.Healthy("Sistema de archivos OK");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Sistema de archivos con problemas", ex);
        }
    }, tags: new[] { "system" });

// En el pipeline HTTP
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Configuración Básica
Crea un proyecto ASP.NET Core Web API con:
- Configuración básica de Swagger
- CORS configurado para desarrollo
- Logging configurado
- Health check básico

### Ejercicio 2: Configuración de Entity Framework
Configura Entity Framework Core con:
- Connection string configurado
- Retry policy configurado
- Logging de consultas en desarrollo
- Health check para la base de datos

### Ejercicio 3: Middleware Personalizado
Crea middleware personalizado para:
- Logging de requests y responses
- Manejo de errores global
- Rate limiting básico
- Compresión de respuestas

### Ejercicio 4: Configuración de Seguridad
Configura:
- CORS con políticas específicas
- Autenticación JWT básica
- Autorización con políticas
- Headers de seguridad

## 📝 Quiz de Autoevaluación

1. ¿Cuál es la diferencia entre `AddScoped` y `AddSingleton`?
2. ¿Por qué es importante configurar CORS correctamente?
3. ¿Qué ventajas tiene usar middleware personalizado?
4. ¿Cómo configurarías logging estructurado en producción?
5. ¿Qué son los health checks y por qué son importantes?

## 🔗 Enlaces Útiles

- [ASP.NET Core Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [ASP.NET Core Middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/)
- [Entity Framework Core Configuration](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Health Checks](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)

## 🚀 Siguiente Clase

En la siguiente clase aprenderás a crear controladores y endpoints, implementando operaciones CRUD y siguiendo las mejores prácticas de diseño de APIs.

---

**💡 Consejo**: Siempre configura tu aplicación para diferentes entornos (Development, Staging, Production) y usa archivos de configuración separados para cada uno.
