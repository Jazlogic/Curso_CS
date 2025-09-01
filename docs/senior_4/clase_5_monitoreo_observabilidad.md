# Clase 5: Monitoreo y Observabilidad

## Navegación
- [← Clase 4: Event Sourcing y CQRS](clase_4_event_sourcing_cqrs.md)
- [Clase 6: Distributed Tracing →](clase_6_distributed_tracing.md)
- [← Volver al README del módulo](README.md)
- [← Volver al módulo anterior (senior_3)](../senior_3/README.md)
- [→ Ir al siguiente módulo (senior_5)](../senior_5/README.md)

## Objetivos de Aprendizaje
- Comprender los pilares de la observabilidad (logs, métricas, traces)
- Implementar logging estructurado con Serilog
- Configurar métricas con Prometheus y Grafana
- Implementar health checks y readiness probes
- Configurar alertas y dashboards

## Contenido Teórico

### 1. Pilares de la Observabilidad

La observabilidad en microservicios se basa en tres pilares fundamentales:

```csharp
// Los tres pilares de la observabilidad
public class ObservabilityPillars
{
    // 1. Logs - Eventos estructurados con contexto
    public void LoggingExample()
    {
        // Logs deben ser estructurados, no solo texto
        // Incluir correlationId, userId, requestId, etc.
    }
    
    // 2. Métricas - Datos numéricos medibles en el tiempo
    public void MetricsExample()
    {
        // Contadores, gauges, histogramas
        // Latencia, throughput, error rates
    }
    
    // 3. Traces - Seguimiento de requests a través de servicios
    public void TracingExample()
    {
        // Distributed tracing con correlation IDs
        // Span tree para visualizar el flujo completo
    }
}
```

### 2. Logging Estructurado

Implementación de logging estructurado con Serilog:

```csharp
// Configuración de Serilog
public class LoggingConfiguration
{
    public static IHostBuilder ConfigureLogging(IHostBuilder builder)
    {
        return builder.UseSerilog((context, services, configuration) =>
        {
            // Configuración básica
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId()
                .Enrich.WithProcessId();
        });
    }
}

// Middleware para logging de requests
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
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
            ?? Guid.NewGuid().ToString();
        
        // Agregar correlation ID al contexto del log
        LogContext.PushProperty("CorrelationId", correlationId);
        LogContext.PushProperty("UserId", context.User?.Identity?.Name ?? "anonymous");
        LogContext.PushProperty("RequestPath", context.Request.Path);
        LogContext.PushProperty("RequestMethod", context.Request.Method);

        var sw = Stopwatch.StartNew();
        
        try
        {
            await _next(context);
            
            sw.Stop();
            
            _logger.LogInformation(
                "Request completed in {ElapsedMs}ms with status {StatusCode}",
                sw.ElapsedMilliseconds,
                context.Response.StatusCode);
        }
        catch (Exception ex)
        {
            sw.Stop();
            
            _logger.LogError(
                ex,
                "Request failed after {ElapsedMs}ms",
                sw.ElapsedMilliseconds);
            
            throw;
        }
    }
}
```

### 3. Métricas con Prometheus

Implementación de métricas personalizadas:

```csharp
// Métricas personalizadas
public class CustomMetrics
{
    private readonly Counter _requestCounter;
    private readonly Histogram _requestDuration;
    private readonly Gauge _activeConnections;
    private readonly Counter _errorCounter;

    public CustomMetrics()
    {
        _requestCounter = Metrics.CreateCounter(
            "http_requests_total",
            "Total number of HTTP requests",
            new CounterConfiguration
            {
                LabelNames = new[] { "method", "endpoint", "status_code" }
            });

        _requestDuration = Metrics.CreateHistogram(
            "http_request_duration_seconds",
            "HTTP request duration in seconds",
            new HistogramConfiguration
            {
                LabelNames = new[] { "method", "endpoint" },
                Buckets = new[] { 0.1, 0.25, 0.5, 1, 2.5, 5, 10 }
            });

        _activeConnections = Metrics.CreateGauge(
            "active_connections",
            "Number of active connections");

        _errorCounter = Metrics.CreateCounter(
            "errors_total",
            "Total number of errors",
            new CounterConfiguration
            {
                LabelNames = new[] { "type", "service" }
            });
    }

    public void RecordRequest(string method, string endpoint, int statusCode, double duration)
    {
        _requestCounter.WithLabels(method, endpoint, statusCode.ToString()).Inc();
        _requestDuration.WithLabels(method, endpoint).Observe(duration);
    }

    public void SetActiveConnections(int count)
    {
        _activeConnections.Set(count);
    }

    public void RecordError(string type, string service)
    {
        _errorCounter.WithLabels(type, service).Inc();
    }
}

// Middleware para métricas automáticas
public class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly CustomMetrics _metrics;

    public MetricsMiddleware(RequestDelegate next, CustomMetrics metrics)
    {
        _next = next;
        _metrics = metrics;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        
        try
        {
            await _next(context);
            
            sw.Stop();
            
            _metrics.RecordRequest(
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                sw.Elapsed.TotalSeconds);
        }
        catch
        {
            sw.Stop();
            
            _metrics.RecordRequest(
                context.Request.Method,
                context.Request.Path,
                500,
                sw.Elapsed.TotalSeconds);
            
            throw;
        }
    }
}
```

### 4. Health Checks

Implementación de health checks personalizados:

```csharp
// Health check personalizado para base de datos
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDbConnection _connection;
    private readonly string _query;

    public DatabaseHealthCheck(IDbConnection connection, string query = "SELECT 1")
    {
        _connection = connection;
        _query = query;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = _query;
            command.CommandTimeout = 5; // 5 segundos máximo
            
            await command.ExecuteScalarAsync(cancellationToken);
            
            return HealthCheckResult.Healthy("Database is responding");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database is not responding", ex);
        }
    }
}

// Health check para dependencias externas
public class ExternalServiceHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly string _healthEndpoint;

    public ExternalServiceHealthCheck(HttpClient httpClient, string healthEndpoint)
    {
        _httpClient = httpClient;
        _healthEndpoint = healthEndpoint;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(_healthEndpoint, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("External service is responding");
            }
            
            return HealthCheckResult.Degraded(
                $"External service returned status {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("External service is not responding", ex);
        }
    }
}

// Configuración de health checks
public static class HealthCheckExtensions
{
    public static IServiceCollection AddCustomHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database")
            .AddCheck<ExternalServiceHealthCheck>("external_service")
            .AddCheck("self", () => HealthCheckResult.Healthy())
            .AddUrlGroup(
                new Uri("https://api.external.com/health"),
                "external_api")
            .AddRedis(configuration.GetConnectionString("Redis"))
            .AddSqlServer(configuration.GetConnectionString("DefaultConnection"));

        return services;
    }
}
```

### 5. Alertas y Dashboards

Configuración de alertas con Prometheus y Grafana:

```csharp
// Configuración de alertas
public class AlertingConfiguration
{
    public static void ConfigureAlerts(IServiceCollection services)
    {
        // Alertas para alta latencia
        services.AddSingleton<IAlertRule>(new AlertRule
        {
            Name = "HighLatency",
            Condition = "http_request_duration_seconds > 2",
            Duration = "5m",
            Severity = AlertSeverity.Warning,
            Message = "High latency detected: {{ $value }}s"
        });

        // Alertas para alta tasa de errores
        services.AddSingleton<IAlertRule>(new AlertRule
        {
            Name = "HighErrorRate",
            Condition = "rate(errors_total[5m]) > 0.1",
            Duration = "2m",
            Severity = AlertSeverity.Critical,
            Message = "High error rate: {{ $value }} errors/sec"
        });

        // Alertas para servicios no saludables
        services.AddSingleton<IAlertRule>(new AlertRule
        {
            Name = "ServiceUnhealthy",
            Condition = "up == 0",
            Duration = "1m",
            Severity = AlertSeverity.Critical,
            Message = "Service {{ $labels.instance }} is down"
        });
    }
}

// Servicio de notificaciones
public interface INotificationService
{
    Task SendAlertAsync(Alert alert);
}

public class SlackNotificationService : INotificationService
{
    private readonly HttpClient _httpClient;
    private readonly string _webhookUrl;

    public SlackNotificationService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _webhookUrl = configuration["Slack:WebhookUrl"];
    }

    public async Task SendAlertAsync(Alert alert)
    {
        var message = new
        {
            text = $"🚨 *{alert.Severity} Alert: {alert.Name}*",
            attachments = new[]
            {
                new
                {
                    color = GetColorForSeverity(alert.Severity),
                    fields = new[]
                    {
                        new { title = "Message", value = alert.Message, @short = false },
                        new { title = "Duration", value = alert.Duration, @short = true },
                        new { title = "Timestamp", value = DateTime.UtcNow.ToString("O"), @short = true }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(message);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        await _httpClient.PostAsync(_webhookUrl, content);
    }

    private string GetColorForSeverity(AlertSeverity severity) => severity switch
    {
        AlertSeverity.Info => "#36a64f",
        AlertSeverity.Warning => "#ff8c00",
        AlertSeverity.Critical => "#ff0000",
        _ => "#808080"
    };
}
```

## Ejercicios Prácticos

### Ejercicio 1: Implementar Logging Estructurado
Crea un middleware que registre todas las operaciones de base de datos con contexto completo.

### Ejercicio 2: Métricas Personalizadas
Implementa métricas para el tiempo de respuesta de diferentes endpoints y la tasa de éxito/fallo.

### Ejercicio 3: Health Checks Avanzados
Crea health checks que verifiquen la conectividad a múltiples servicios externos y bases de datos.

## Proyecto Integrador
Implementa un sistema de monitoreo completo para un microservicio de usuarios que incluya:
- Logging estructurado con correlation IDs
- Métricas de performance y negocio
- Health checks para todas las dependencias
- Alertas automáticas para condiciones críticas
- Dashboard en Grafana para visualización

## Recursos Adicionales
- [Serilog Documentation](https://serilog.net/)
- [Prometheus .NET Client](https://prometheus.io/docs/guides/aspnetcore/)
- [ASP.NET Core Health Checks](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [Grafana Dashboards](https://grafana.com/docs/grafana/latest/dashboards/)
