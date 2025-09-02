# 🚀 Clase 8: Monitoreo y Observabilidad

## 🧭 Navegación
- **⬅️ Anterior**: [Clase 7: CI/CD y Pipelines](clase_7_cicd_pipelines.md)
- **🏠 [Volver al Módulo 6](../README.md)**
- **➡️ Siguiente**: [Clase 9: Deployment y Estrategias](clase_9_deployment_estrategias.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)**

---

## 🎯 Objetivos de la Clase

Al finalizar esta clase, serás capaz de:
- Implementar logging estructurado con Serilog
- Configurar métricas con Prometheus
- Implementar health checks y readiness probes
- Configurar alertas y dashboards

---

## 📚 Contenido Teórico

### 8.1 Logging Estructurado con Serilog

#### Configuración de Serilog

```csharp
// Program.cs
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/app-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.Elasticsearch(
        node: new Uri("http://localhost:9200"),
        indexFormat: "myapp-logs-{0:yyyy.MM}")
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting web application");
    var app = builder.Build();
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

#### Logging Estructurado en Controladores

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly IUserService _userService;
    
    public UsersController(ILogger<UsersController> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["UserId"] = id,
            ["RequestId"] = HttpContext.TraceIdentifier
        });
        
        _logger.LogInformation("Getting user with ID {UserId}", id);
        
        try
        {
            var user = await _userService.GetByIdAsync(id);
            
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", id);
                return NotFound();
            }
            
            _logger.LogInformation("Successfully retrieved user {UserId} with email {UserEmail}", 
                user.Id, user.Email);
                
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID {UserId}", id);
            throw;
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["UserEmail"] = request.Email,
            ["RequestId"] = HttpContext.TraceIdentifier
        });
        
        _logger.LogInformation("Creating new user with email {UserEmail}", request.Email);
        
        try
        {
            var user = await _userService.CreateAsync(request);
            
            _logger.LogInformation("Successfully created user {UserId} with email {UserEmail}", 
                user.Id, user.Email);
                
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed for user creation: {ValidationErrors}", 
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            return BadRequest(ex.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user with email {UserEmail}", request.Email);
            throw;
        }
    }
}
```

#### Middleware de Logging

```csharp
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
        var requestBody = string.Empty;
        
        // Captura el cuerpo de la request para logging
        if (context.Request.Body.CanSeek)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }
        
        try
        {
            await _next(context);
        }
        finally
        {
            var elapsed = DateTime.UtcNow - startTime;
            
            var logData = new
            {
                Method = context.Request.Method,
                Path = context.Request.Path,
                QueryString = context.Request.QueryString.ToString(),
                StatusCode = context.Response.StatusCode,
                ElapsedMs = elapsed.TotalMilliseconds,
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                IPAddress = context.Connection.RemoteIpAddress?.ToString(),
                RequestBody = requestBody.Length > 1000 ? requestBody[..1000] + "..." : requestBody
            };
            
            if (context.Response.StatusCode >= 400)
            {
                _logger.LogWarning("Request completed with status {StatusCode} in {ElapsedMs}ms: {Method} {Path}", 
                    logData.StatusCode, logData.ElapsedMs, logData.Method, logData.Path);
            }
            else
            {
                _logger.LogInformation("Request completed with status {StatusCode} in {ElapsedMs}ms: {Method} {Path}", 
                    logData.StatusCode, logData.ElapsedMs, logData.Method, logData.Path);
            }
        }
    }
}
```

### 8.2 Métricas con Prometheus

#### Configuración de Métricas

```csharp
// Program.cs
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Configuración de métricas Prometheus
builder.Services.AddMetrics();

var app = builder.Build();

// Endpoint para métricas de Prometheus
app.MapMetrics("/metrics");

app.Run();
```

#### Métricas Personalizadas

```csharp
public class ApplicationMetrics
{
    // Contadores
    public static readonly Counter HttpRequestsTotal = Metrics
        .CreateCounter("http_requests_total", "Total HTTP requests", 
            new CounterConfiguration
            {
                LabelNames = new[] { "method", "endpoint", "status_code" }
            });
    
    public static readonly Counter DatabaseQueriesTotal = Metrics
        .CreateCounter("database_queries_total", "Total database queries",
            new CounterConfiguration
            {
                LabelNames = new[] { "operation", "table" }
            });
    
    // Gauges
    public static readonly Gauge ActiveConnections = Metrics
        .CreateGauge("active_connections", "Number of active connections");
    
    public static readonly Gauge MemoryUsageBytes = Metrics
        .CreateGauge("memory_usage_bytes", "Memory usage in bytes");
    
    // Histogramas
    public static readonly Histogram HttpRequestDuration = Metrics
        .CreateHistogram("http_request_duration_seconds", "HTTP request duration",
            new HistogramConfiguration
            {
                LabelNames = new[] { "method", "endpoint" },
                Buckets = new[] { 0.1, 0.25, 0.5, 1, 2.5, 5, 10 }
            });
    
    public static readonly Histogram DatabaseQueryDuration = Metrics
        .CreateHistogram("database_query_duration_seconds", "Database query duration",
            new HistogramConfiguration
            {
                LabelNames = new[] { "operation", "table" },
                Buckets = new[] { 0.01, 0.05, 0.1, 0.25, 0.5, 1, 2.5 }
            });
}

// Middleware para métricas HTTP
public class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MetricsMiddleware> _logger;
    
    public MetricsMiddleware(RequestDelegate next, ILogger<MetricsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            var method = context.Request.Method;
            var endpoint = context.Request.Path.Value ?? "/";
            var statusCode = context.Response.StatusCode.ToString();
            
            // Incrementa contador de requests
            ApplicationMetrics.HttpRequestsTotal
                .WithLabels(method, endpoint, statusCode)
                .Inc();
            
            // Registra duración de la request
            ApplicationMetrics.HttpRequestDuration
                .WithLabels(method, endpoint)
                .Observe(stopwatch.Elapsed.TotalSeconds);
        }
    }
}
```

#### Métricas de Base de Datos

```csharp
public class DatabaseMetrics
{
    public static readonly Counter QueriesTotal = Metrics
        .CreateCounter("database_queries_total", "Total database queries",
            new CounterConfiguration
            {
                LabelNames = new[] { "operation", "table", "status" }
            });
    
    public static readonly Histogram QueryDuration = Metrics
        .CreateHistogram("database_query_duration_seconds", "Database query duration",
            new HistogramConfiguration
            {
                LabelNames = new[] { "operation", "table" },
                Buckets = new[] { 0.01, 0.05, 0.1, 0.25, 0.5, 1, 2.5 }
            });
    
    public static readonly Gauge ConnectionPoolSize = Metrics
        .CreateGauge("database_connection_pool_size", "Database connection pool size");
    
    public static readonly Gauge ActiveConnections = Metrics
        .CreateGauge("database_active_connections", "Number of active database connections");
}

// Interceptor para Entity Framework
public class MetricsInterceptor : DbCommandInterceptor
{
    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, 
        CommandEventData eventData, 
        InterceptionResult<DbDataReader> result, 
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var dbResult = await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
            
            stopwatch.Stop();
            
            var operation = command.CommandType.ToString();
            var table = ExtractTableName(command.CommandText);
            
            // Incrementa contador de queries
            DatabaseMetrics.QueriesTotal
                .WithLabels(operation, table, "success")
                .Inc();
            
            // Registra duración de la query
            DatabaseMetrics.QueryDuration
                .WithLabels(operation, table)
                .Observe(stopwatch.Elapsed.TotalSeconds);
            
            return dbResult;
        }
        catch
        {
            var operation = command.CommandType.ToString();
            var table = ExtractTableName(command.CommandText);
            
            // Incrementa contador de queries fallidas
            DatabaseMetrics.QueriesTotal
                .WithLabels(operation, table, "error")
                .Inc();
            
            throw;
        }
    }
    
    private string ExtractTableName(string commandText)
    {
        // Lógica simple para extraer nombre de tabla
        if (commandText.Contains("FROM"))
        {
            var fromIndex = commandText.IndexOf("FROM", StringComparison.OrdinalIgnoreCase);
            var afterFrom = commandText.Substring(fromIndex + 4).Trim();
            var spaceIndex = afterFrom.IndexOf(' ');
            return spaceIndex > 0 ? afterFrom.Substring(0, spaceIndex).Trim() : afterFrom.Trim();
        }
        
        return "unknown";
    }
}
```

### 8.3 Health Checks y Readiness Probes

#### Health Checks Personalizados

```csharp
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly DbContext _context;
    private readonly ILogger<DatabaseHealthCheck> _logger;
    
    public DatabaseHealthCheck(DbContext context, ILogger<DatabaseHealthCheck> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verifica conectividad a la base de datos
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            
            if (!canConnect)
            {
                _logger.LogWarning("Database health check failed: Cannot connect to database");
                return HealthCheckResult.Unhealthy("Cannot connect to database");
            }
            
            // Verifica que se puede ejecutar una query simple
            var result = await _context.Database.SqlQueryRaw<int>("SELECT 1").FirstOrDefaultAsync(cancellationToken);
            
            if (result != 1)
            {
                _logger.LogWarning("Database health check failed: Query execution failed");
                return HealthCheckResult.Unhealthy("Query execution failed");
            }
            
            return HealthCheckResult.Healthy("Database is healthy");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed with exception");
            return HealthCheckResult.Unhealthy("Database health check failed", ex);
        }
    }
}

public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisHealthCheck> _logger;
    
    public RedisHealthCheck(IConnectionMultiplexer redis, ILogger<RedisHealthCheck> logger)
    {
        _redis = redis;
        _logger = logger;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var result = await db.PingAsync();
            
            if (result.TotalMilliseconds > 100)
            {
                _logger.LogWarning("Redis health check: High latency detected ({Latency}ms)", result.TotalMilliseconds);
                return HealthCheckResult.Degraded("High latency detected");
            }
            
            return HealthCheckResult.Healthy("Redis is healthy");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed with exception");
            return HealthCheckResult.Unhealthy("Redis health check failed", ex);
        }
    }
}

public class ExternalApiHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalApiHealthCheck> _logger;
    
    public ExternalApiHealthCheck(HttpClient httpClient, ILogger<ExternalApiHealthCheck> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            var response = await _httpClient.GetAsync("/health", cancellationToken);
            stopwatch.Stop();
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("External API health check failed: Status {StatusCode}", response.StatusCode);
                return HealthCheckResult.Unhealthy($"API returned status {response.StatusCode}");
            }
            
            if (stopwatch.Elapsed.TotalMilliseconds > 500)
            {
                _logger.LogWarning("External API health check: High latency detected ({Latency}ms)", stopwatch.Elapsed.TotalMilliseconds);
                return HealthCheckResult.Degraded("High latency detected");
            }
            
            return HealthCheckResult.Healthy("External API is healthy");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "External API health check failed with exception");
            return HealthCheckResult.Unhealthy("External API health check failed", ex);
        }
    }
}
```

#### Configuración de Health Checks

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Registra health checks
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "database", "critical" })
    .AddCheck<RedisHealthCheck>("redis", tags: new[] { "cache", "critical" })
    .AddCheck<ExternalApiHealthCheck>("external-api", tags: new[] { "external", "non-critical" })
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "self" });

var app = builder.Build();

// Endpoints de health checks
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("critical"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("self"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
```

### 8.4 Alertas y Dashboards

#### Configuración de Alertas

```csharp
public class AlertingService : IAlertingService
{
    private readonly ILogger<AlertingService> _logger;
    private readonly IConfiguration _configuration;
    
    public AlertingService(ILogger<AlertingService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }
    
    public async Task SendAlertAsync(string alertType, string message, AlertSeverity severity, Dictionary<string, object> metadata = null)
    {
        var alert = new Alert
        {
            Type = alertType,
            Message = message,
            Severity = severity,
            Timestamp = DateTime.UtcNow,
            Metadata = metadata ?? new Dictionary<string, object>()
        };
        
        _logger.LogWarning("Alert: {AlertType} - {Message} (Severity: {Severity})", 
            alert.Type, alert.Message, alert.Severity);
        
        // Envía alerta por email
        if (severity >= AlertSeverity.High)
        {
            await SendEmailAlertAsync(alert);
        }
        
        // Envía alerta por Slack
        if (severity >= AlertSeverity.Medium)
        {
            await SendSlackAlertAsync(alert);
        }
        
        // Envía alerta por SMS (solo críticas)
        if (severity == AlertSeverity.Critical)
        {
            await SendSmsAlertAsync(alert);
        }
    }
    
    private async Task SendEmailAlertAsync(Alert alert)
    {
        // Implementación de envío por email
        _logger.LogInformation("Email alert sent for {AlertType}", alert.Type);
        await Task.CompletedTask;
    }
    
    private async Task SendSlackAlertAsync(Alert alert)
    {
        // Implementación de envío por Slack
        _logger.LogInformation("Slack alert sent for {AlertType}", alert.Type);
        await Task.CompletedTask;
    }
    
    private async Task SendSmsAlertAsync(Alert alert)
    {
        // Implementación de envío por SMS
        _logger.LogInformation("SMS alert sent for {AlertType}", alert.Type);
        await Task.CompletedTask;
    }
}

public enum AlertSeverity
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public class Alert
{
    public string Type { get; set; }
    public string Message { get; set; }
    public AlertSeverity Severity { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
}
```

---

## 💻 Ejercicios Prácticos

### Ejercicio 1: Implementar Health Check Completo

Crea un health check que verifique:

```csharp
// Implementa:
// - Conectividad a base de datos
// - Estado de servicios externos
// - Uso de recursos del sistema
// - Latencia de operaciones críticas
```

### Ejercicio 2: Dashboard de Métricas

Implementa un dashboard con:

```csharp
// Incluye:
// - Métricas de performance
// - Estado de servicios
// - Alertas configurables
// - Visualización en tiempo real
```

---

## 🔍 Casos de Uso Reales

### 1. Monitoreo de Microservicios

```csharp
public class MicroserviceHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MicroserviceHealthCheck> _logger;
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var services = new[] { "users-service", "orders-service", "products-service" };
        var results = new List<string>();
        
        foreach (var service in services)
        {
            try
            {
                var client = _httpClientFactory.CreateClient(service);
                var response = await client.GetAsync("/health", cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    results.Add($"{service}: Healthy");
                }
                else
                {
                    results.Add($"{service}: Unhealthy (Status: {response.StatusCode})");
                }
            }
            catch (Exception ex)
            {
                results.Add($"{service}: Error ({ex.Message})");
            }
        }
        
        var healthyServices = results.Count(r => r.Contains("Healthy"));
        var totalServices = services.Length;
        
        if (healthyServices == totalServices)
        {
            return HealthCheckResult.Healthy("All microservices are healthy");
        }
        else if (healthyServices > totalServices / 2)
        {
            return HealthCheckResult.Degraded($"Some microservices are unhealthy: {string.Join(", ", results)}");
        }
        else
        {
            return HealthCheckResult.Unhealthy($"Most microservices are unhealthy: {string.Join(", ", results)}");
        }
    }
}
```

### 2. Métricas de Business

```csharp
public class BusinessMetrics
{
    public static readonly Counter OrdersCreated = Metrics
        .CreateCounter("orders_created_total", "Total orders created",
            new CounterConfiguration
            {
                LabelNames = new[] { "customer_type", "payment_method" }
            });
    
    public static readonly Histogram OrderValue = Metrics
        .CreateHistogram("order_value_dollars", "Order value distribution",
            new HistogramConfiguration
            {
                LabelNames = new[] { "customer_type" },
                Buckets = new[] { 10, 50, 100, 250, 500, 1000, 2500, 5000 }
            });
    
    public static readonly Gauge ActiveUsers = Metrics
        .CreateGauge("active_users", "Number of active users");
    
    public static readonly Gauge InventoryLevel = Metrics
        .CreateGauge("inventory_level", "Current inventory level",
            new GaugeConfiguration
            {
                LabelNames = new[] { "product_category" }
            });
}
```

---

## 📊 Métricas de Observabilidad

### KPIs de Monitoreo

1. **System Uptime**: Tiempo de actividad del sistema
2. **Response Time**: Tiempo de respuesta promedio
3. **Error Rate**: Tasa de errores
4. **Resource Utilization**: Utilización de recursos
5. **Business Metrics**: Métricas de negocio

---

## 🎯 Resumen de la Clase

En esta clase hemos aprendido:

✅ **Logging Estructurado**: Implementación con Serilog
✅ **Métricas Prometheus**: Configuración y métricas personalizadas
✅ **Health Checks**: Verificación de salud de servicios
✅ **Alertas y Dashboards**: Sistema de notificaciones y visualización
✅ **Casos de Uso Reales**: Implementación en microservicios

---

## 🚀 Próximos Pasos

En la siguiente clase aprenderemos sobre:
- **Deployment y Estrategias**
- Blue-Green deployment
- Canary deployment
- Rollback strategies

---

## 🔗 Enlaces de Referencia

- [Serilog Documentation](https://serilog.net/)
- [Prometheus .NET](https://prometheus.io/docs/guides/aspnetcore/)
- [Health Checks in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [Observability Best Practices](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/observability/)
