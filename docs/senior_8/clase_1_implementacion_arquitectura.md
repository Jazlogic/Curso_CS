# 🏗️ Clase 1: Implementación Práctica de la Arquitectura

## 🧭 Navegación del Módulo

- **🏠 Inicio del Módulo**: [Módulo 15: Sistemas Avanzados y Distribuidos](../senior_8/README.md)
- **➡️ Siguiente**: [Clase 2: Containerización y Docker](../senior_8/clase_2_containerizacion_docker.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** estructura completa del proyecto MussikOn
2. **Configurar** dependencias y servicios
3. **Desarrollar** Program.cs y configuración
4. **Aplicar** Clean Architecture en práctica
5. **Configurar** middleware y extensiones

---

## 🏗️ **Estructura Completa del Proyecto**

### **Organización de la Solución**

```bash
MusicalMatchingPlatform/
├── src/
│   ├── MusicalMatching.API/                    # Web API principal
│   │   ├── Controllers/                        # Controladores de la API
│   │   ├── Middleware/                         # Middleware personalizado
│   │   ├── DTOs/                               # Data Transfer Objects
│   │   ├── Extensions/                         # Extensiones de configuración
│   │   ├── Program.cs                          # Punto de entrada
│   │   └── appsettings.json                    # Configuración
│   ├── MusicalMatching.Domain/                 # Capa de dominio
│   │   ├── Entities/                           # Entidades del dominio
│   │   ├── Interfaces/                         # Contratos del dominio
│   │   ├── Services/                           # Servicios de dominio
│   │   ├── Events/                             # Eventos de dominio
│   │   ├── ValueObjects/                       # Objetos de valor
│   │   └── Exceptions/                         # Excepciones personalizadas
│   ├── MusicalMatching.Application/             # Capa de aplicación
│   │   ├── Services/                           # Servicios de aplicación
│   │   ├── Validators/                         # Validadores con FluentValidation
│   │   ├── AutoMapper/                         # Perfiles de mapeo
│   │   ├── Commands/                           # Comandos CQRS
│   │   ├── Queries/                            # Consultas CQRS
│   │   └── Handlers/                           # Manejadores CQRS
│   └── MusicalMatching.Infrastructure/         # Capa de infraestructura
│       ├── Data/                               # Acceso a datos
│       ├── Repositories/                       # Implementaciones de repositorios
│       ├── External/                           # Servicios externos
│       ├── Configuration/                      # Configuración de servicios
│       └── Hubs/                               # SignalR Hubs
├── tests/
│   ├── MusicalMatching.UnitTests/              # Tests unitarios
│   ├── MusicalMatching.IntegrationTests/       # Tests de integración
│   └── MusicalMatching.E2ETests/               # Tests end-to-end
├── docs/                                        # Documentación
├── scripts/                                     # Scripts de deployment
├── docker/                                      # Configuración Docker
├── k8s/                                         # Configuración Kubernetes
└── .github/                                     # GitHub Actions
    └── workflows/                               # Pipelines de CI/CD
```

---

## 📦 **Configuración de Dependencias**

### **Proyecto API Principal**

```xml
<!-- MusicalMatching.API.csproj -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <!-- Core Framework -->
    <PackageReference Include="Microsoft.AspNetCore.App" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
    
    <!-- Identity y Autenticación -->
    <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
    <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.3.1" />
    
    <!-- SignalR -->
    <PackageReference Include="Microsoft.AspNetCore.SignalR" Version="1.1.0" />
    
    <!-- Validación y Mapping -->
    <PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
    <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
    
    <!-- Logging y Monitoreo -->
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="5.0.0" />
    <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
    
    <!-- Health Checks -->
    <PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="AspNetCore.HealthChecks.Redis" Version="8.0.0" />
    
    <!-- Swagger -->
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
    <PackageReference Include="Swashbuckle.AspNetCore.Annotations" Version="6.5.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\MusicalMatching.Domain\MusicalMatching.Domain.csproj" />
    <ProjectReference Include="..\MusicalMatching.Application\MusicalMatching.Application.csproj" />
    <ProjectReference Include="..\MusicalMatching.Infrastructure\MusicalMatching.Infrastructure.csproj" />
  </ItemGroup>
</Project>
```

---

## ⚙️ **Program.cs Configurado**

### **Configuración Principal de la Aplicación**

```csharp
// Program.cs
using MusicalMatching.Application;
using MusicalMatching.Infrastructure;
using MusicalMatching.API.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/musical-matching-.txt", 
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting Musical Matching Platform API");

    // Agregar servicios de aplicación
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);
    
    // Configurar CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigins", policy =>
        {
            policy.WithOrigins(
                    "http://localhost:3000",      // Frontend React
                    "http://localhost:19006",     // Expo
                    "https://musicalmatching.com" // Producción
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });

    // Configurar SignalR
    builder.Services.AddSignalR(options =>
    {
        options.EnableDetailedErrors = builder.Environment.IsDevelopment();
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
        options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    });

    // Configurar Health Checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>("Database")
        .AddRedis(builder.Configuration.GetConnectionString("Redis"), name: "Redis")
        .AddUrlGroup(new Uri("https://api.stripe.com"), "Stripe API");

    var app = builder.Build();

    // Configurar pipeline de middleware
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Musical Matching API v1");
            c.RoutePrefix = string.Empty; // Swagger en la raíz
        });
    }

    app.UseMiddleware<ExceptionMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();
    app.UseMiddleware<PerformanceMiddleware>();

    app.UseHttpsRedirection();
    app.UseCors("AllowSpecificOrigins");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHub<NotificationHub>("/hubs/notifications");
    app.MapHub<ChatHub>("/hubs/chat");
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
```

---

## 🔧 **Extensiones de Configuración**

### **Extensiones para Servicios de Aplicación**

```csharp
// MusicalMatching.Application/Extensions/ServiceCollectionExtensions.cs
using FluentValidation;
using MediatR;
using System.Reflection;

namespace MusicalMatching.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Registrar MediatR
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Registrar AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Registrar FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddFluentValidationAutoValidation();

        // Registrar servicios de aplicación
        services.AddScoped<IMusicianRequestService, MusicianRequestService>();
        services.AddScoped<IMusicianMatchingService, MusicianMatchingService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ICacheService, CacheService>();

        return services;
    }
}
```

### **Extensiones para Servicios de Infraestructura**

```csharp
// MusicalMatching.Infrastructure/Extensions/ServiceCollectionExtensions.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using StackExchange.Redis;

namespace MusicalMatching.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Configurar Entity Framework
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("MusicalMatching.Infrastructure")));

        // Configurar Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Configurar Redis
        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var redisConnectionString = configuration.GetConnectionString("Redis");
            return ConnectionMultiplexer.Connect(redisConnectionString);
        });

        // Configurar repositorios
        services.AddScoped<IMusicianRequestRepository, MusicianRequestRepository>();
        services.AddScoped<IMusicianRepository, MusicianRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Configurar servicios externos
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IFileStorageService, FileStorageService>();

        // Configurar SignalR
        services.AddSignalR();

        return services;
    }
}
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Crear Estructura del Proyecto**
```bash
# Crea la estructura completa del proyecto:
# - Solución .NET con múltiples proyectos
# - Configuración de dependencias
# - Archivos de proyecto (.csproj)
# - Estructura de carpetas
```

### **Ejercicio 2: Configurar Dependencias**
```xml
# Configura las dependencias para:
# - Entity Framework Core
# - Identity y JWT
# - SignalR
# - Validación y Mapping
# - Logging y Monitoreo
```

### **Ejercicio 3: Program.cs Configurado**
```csharp
# Implementa:
# - Configuración de servicios
# - Middleware personalizado
# - Health checks
# - OpenTelemetry
# - Rate limiting
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **🏗️ Estructura del Proyecto**: Organización completa con Clean Architecture
2. **📦 Configuración de Dependencias**: Paquetes NuGet para cada capa
3. **⚙️ Program.cs Configurado**: Configuración completa de la aplicación
4. **🔧 Extensiones de Servicios**: Organización modular de la configuración
5. **🚀 Configuración Avanzada**: Health checks, OpenTelemetry y rate limiting

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Containerización y Docker**, implementando Dockerfiles multi-stage y Docker Compose para diferentes entornos.

---

**¡Has completado la primera clase del Módulo 15! 🏗️📦**
