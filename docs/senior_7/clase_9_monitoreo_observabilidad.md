# 📊 Clase 9: Monitoreo y Observabilidad

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 8: Caching y Performance](../senior_7/clase_8_caching_performance.md)
- **🏠 Inicio del Módulo**: [Módulo 14: Plataformas Empresariales Reales](../senior_7/README.md)
- **➡️ Siguiente**: [Clase 10: Proyecto Final](../senior_7/clase_10_proyecto_final.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** sistema de logging estructurado
2. **Configurar** métricas y health checks
3. **Desarrollar** trazabilidad distribuida
4. **Aplicar** patrones de observabilidad
5. **Monitorear** aplicaciones en producción

---

## 📝 **Sistema de Logging Estructurado**

### **Logging Avanzado con Serilog**

```csharp
public interface ILoggingService
{
    void LogInformation(string message, params object[] properties);
    void LogWarning(string message, params object[] properties);
    void LogError(Exception ex, string message, params object[] properties);
    void LogDebug(string message, params object[] properties);
    void LogTrace(string message, params object[] properties);
    IDisposable BeginScope(string operationName, Dictionary<string, object> properties = null);
}

// Servicio de logging estructurado
public class StructuredLoggingService : ILoggingService
{
    private readonly ILogger<StructuredLoggingService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICorrelationIdProvider _correlationIdProvider;

    public StructuredLoggingService(
        ILogger<StructuredLoggingService> logger,
        IHttpContextAccessor httpContextAccessor,
        ICorrelationIdProvider correlationIdProvider)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _correlationIdProvider = correlationIdProvider;
    }

    public void LogInformation(string message, params object[] properties)
    {
        var enrichedProperties = EnrichProperties(properties);
        _logger.LogInformation(message, enrichedProperties);
    }

    public void LogWarning(string message, params object[] properties)
    {
        var enrichedProperties = EnrichProperties(properties);
        _logger.LogWarning(message, enrichedProperties);
    }

    public void LogError(Exception ex, string message, params object[] properties)
    {
        var enrichedProperties = EnrichProperties(properties);
        enrichedProperties["ExceptionType"] = ex.GetType().Name;
        enrichedProperties["StackTrace"] = ex.StackTrace;
        
        _logger.LogError(ex, message, enrichedProperties);
    }

    public void LogDebug(string message, params object[] properties)
    {
        var enrichedProperties = EnrichProperties(properties);
        _logger.LogDebug(message, enrichedProperties);
    }

    public void LogTrace(string message, params object[] properties)
    {
        var enrichedProperties = EnrichProperties(properties);
        _logger.LogTrace(message, enrichedProperties);
    }

    public IDisposable BeginScope(string operationName, Dictionary<string, object> properties = null)
    {
        var scopeProperties = new Dictionary<string, object>
        {
            ["Operation"] = operationName,
            ["CorrelationId"] = _correlationIdProvider.GetCorrelationId()
        };

        if (properties != null)
        {
            foreach (var kvp in properties)
            {
                scopeProperties[kvp.Key] = kvp.Value;
            }
        }

        return _logger.BeginScope(scopeProperties);
    }

    private object[] EnrichProperties(object[] properties)
    {
        var enriched = new List<object>();
        
        // Agregar contexto HTTP si está disponible
        if (_httpContextAccessor.HttpContext != null)
        {
            enriched.Add("RequestId");
            enriched.Add(_httpContextAccessor.HttpContext.TraceIdentifier);
            enriched.Add("UserAgent");
            enriched.Add(_httpContextAccessor.HttpContext.Request.Headers["User-Agent"].ToString());
            enriched.Add("RemoteIpAddress");
            enriched.Add(_httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString());
        }

        // Agregar correlation ID
        enriched.Add("CorrelationId");
        enriched.Add(_correlationIdProvider.GetCorrelationId());

        // Agregar propiedades originales
        enriched.AddRange(properties);

        return enriched.ToArray();
    }
}

// Proveedor de Correlation ID
public interface ICorrelationIdProvider
{
    string GetCorrelationId();
    void SetCorrelationId(string correlationId);
}

public class CorrelationIdProvider : ICorrelationIdProvider
{
    private readonly AsyncLocal<string> _correlationId = new AsyncLocal<string>();

    public string GetCorrelationId()
    {
        if (string.IsNullOrEmpty(_correlationId.Value))
        {
            _correlationId.Value = Guid.NewGuid().ToString();
        }
        return _correlationId.Value;
    }

    public void SetCorrelationId(string correlationId)
    {
        _correlationId.Value = correlationId;
    }
}

// Middleware para correlation ID
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ICorrelationIdProvider _correlationIdProvider;

    public CorrelationIdMiddleware(RequestDelegate next, ICorrelationIdProvider correlationIdProvider)
    {
        _next = next;
        _correlationIdProvider = correlationIdProvider;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
                           ?? Guid.NewGuid().ToString();
        
        _correlationIdProvider.SetCorrelationId(correlationId);
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        await _next(context);
    }
}

// Configuración de Serilog
public static class SerilogConfiguration
{
    public static IHostBuilder ConfigureSerilog(this IHostBuilder builder)
    {
        return builder.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "logs/app-.log",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
                {
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                    IndexFormat = $"mussikon-logs-{context.HostingEnvironment.EnvironmentName.ToLower()}-{DateTime.UtcNow:yyyy-MM}"
                });
        });
    }
}
```

---

## 📊 **Sistema de Métricas y Health Checks**

### **Métricas Personalizadas y Health Checks**

```csharp
public interface IMetricsService
{
    void IncrementCounter(string name, Dictionary<string, string> labels = null);
    void RecordGauge(string name, double value, Dictionary<string, string> labels = null);
    void RecordHistogram(string name, double value, Dictionary<string, string> labels = null);
    void RecordSummary(string name, double value, Dictionary<string, string> labels = null);
    MetricsSnapshot GetMetrics();
}

// Servicio de métricas con Prometheus
public class PrometheusMetricsService : IMetricsService
{
    private readonly Counter _requestCounter;
    private readonly Gauge _activeConnections;
    private readonly Histogram _requestDuration;
    private readonly Summary _responseSize;
    private readonly ILogger<PrometheusMetricsService> _logger;

    public PrometheusMetricsService(ILogger<PrometheusMetricsService> logger)
    {
        _logger = logger;

        // Contador de requests
        _requestCounter = Metrics.CreateCounter("http_requests_total", "Total HTTP requests", 
            new CounterConfiguration
            {
                LabelNames = new[] { "method", "endpoint", "status_code" }
            });

        // Gauge de conexiones activas
        _activeConnections = Metrics.CreateGauge("http_active_connections", "Active HTTP connections");

        // Histograma de duración de requests
        _requestDuration = Metrics.CreateHistogram("http_request_duration_seconds", "HTTP request duration",
            new HistogramConfiguration
            {
                LabelNames = new[] { "method", "endpoint" },
                Buckets = new[] { 0.1, 0.25, 0.5, 1, 2.5, 5, 10 }
            });

        // Resumen de tamaño de respuesta
        _responseSize = Metrics.CreateSummary("http_response_size_bytes", "HTTP response size",
            new SummaryConfiguration
            {
                LabelNames = new[] { "method", "endpoint" },
                Objectives = new[]
                {
                    new QuantileEpsilonPair(0.5, 0.05),
                    new QuantileEpsilonPair(0.9, 0.05),
                    new QuantileEpsilonPair(0.95, 0.01),
                    new QuantileEpsilonPair(0.99, 0.01)
                }
            });
    }

    public void IncrementCounter(string name, Dictionary<string, string> labels = null)
    {
        try
        {
            var labelValues = labels?.Values.ToArray() ?? new string[0];
            _requestCounter.WithLabels(labelValues).Inc();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing counter {CounterName}", name);
        }
    }

    public void RecordGauge(string name, double value, Dictionary<string, string> labels = null)
    {
        try
        {
            _activeConnections.Set(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording gauge {GaugeName}", name);
        }
    }

    public void RecordHistogram(string name, double value, Dictionary<string, string> labels = null)
    {
        try
        {
            var labelValues = labels?.Values.ToArray() ?? new string[0];
            _requestDuration.WithLabels(labelValues).Observe(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording histogram {HistogramName}", name);
        }
    }

    public void RecordSummary(string name, double value, Dictionary<string, string> labels = null)
    {
        try
        {
            var labelValues = labels?.Values.ToArray() ?? new string[0];
            _responseSize.WithLabels(labelValues).Observe(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording summary {SummaryName}", name);
        }
    }

    public MetricsSnapshot GetMetrics()
    {
        return new MetricsSnapshot
        {
            RequestCount = _requestCounter.Value,
            ActiveConnections = _activeConnections.Value,
            RequestDuration = _requestDuration.Value,
            ResponseSize = _responseSize.Value,
            Timestamp = DateTime.UtcNow
        };
    }
}

// Health Checks personalizados
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDbConnection _connection;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(IDbConnection connection, ILogger<DatabaseHealthCheck> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT 1";
            command.CommandTimeout = 5;

            var result = await command.ExecuteScalarAsync(cancellationToken);
            
            if (result != null)
            {
                return HealthCheckResult.Healthy("Database is responding");
            }
            
            return HealthCheckResult.Unhealthy("Database query returned null");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
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
            var database = _redis.GetDatabase();
            var result = await database.PingAsync();
            
            if (result.TotalMilliseconds < 100)
            {
                return HealthCheckResult.Healthy($"Redis is responding in {result.TotalMilliseconds}ms");
            }
            else if (result.TotalMilliseconds < 500)
            {
                return HealthCheckResult.Degraded($"Redis is responding slowly: {result.TotalMilliseconds}ms");
            }
            else
            {
                return HealthCheckResult.Unhealthy($"Redis is responding very slowly: {result.TotalMilliseconds}ms");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed");
            return HealthCheckResult.Unhealthy("Redis health check failed", ex);
        }
    }
}

// Configuración de Health Checks
public static class HealthCheckConfiguration
{
    public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "database", "sql" })
            .AddCheck<RedisHealthCheck>("redis", tags: new[] { "cache", "redis" })
            .AddCheck("external_api", () =>
            {
                // Health check para API externa
                try
                {
                    using var client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(5);
                    var response = client.GetAsync("https://api.external.com/health").Result;
                    return response.IsSuccessStatusCode 
                        ? HealthCheckResult.Healthy("External API is responding")
                        : HealthCheckResult.Unhealthy("External API returned error status");
                }
                catch (Exception ex)
                {
                    return HealthCheckResult.Unhealthy("External API health check failed", ex);
                }
            }, tags: new[] { "external", "api" });

        return services;
    }
}
```

---

## 🔍 **Trazabilidad Distribuida**

### **OpenTelemetry y Distributed Tracing**

```csharp
public interface ITracingService
{
    Activity StartActivity(string operationName, Dictionary<string, object> attributes = null);
    void AddEvent(string name, Dictionary<string, object> attributes = null);
    void SetTag(string key, object value);
    void RecordException(Exception ex);
}

// Servicio de trazabilidad con OpenTelemetry
public class OpenTelemetryTracingService : ITracingService
{
    private readonly ActivitySource _activitySource;
    private readonly ILogger<OpenTelemetryTracingService> _logger;

    public OpenTelemetryTracingService(ILogger<OpenTelemetryTracingService> logger)
    {
        _logger = logger;
        _activitySource = new ActivitySource("MussikOn.API");
    }

    public Activity StartActivity(string operationName, Dictionary<string, object> attributes = null)
    {
        var activity = _activitySource.StartActivity(operationName);
        
        if (activity != null && attributes != null)
        {
            foreach (var kvp in attributes)
            {
                activity.SetTag(kvp.Key, kvp.Value?.ToString());
            }
        }

        return activity;
    }

    public void AddEvent(string name, Dictionary<string, object> attributes = null)
    {
        var currentActivity = Activity.Current;
        if (currentActivity != null)
        {
            var tags = attributes?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString()) 
                      ?? new Dictionary<string, string>();
            
            currentActivity.AddEvent(new ActivityEvent(name, tags: tags));
        }
    }

    public void SetTag(string key, object value)
    {
        var currentActivity = Activity.Current;
        if (currentActivity != null)
        {
            currentActivity.SetTag(key, value?.ToString());
        }
    }

    public void RecordException(Exception ex)
    {
        var currentActivity = Activity.Current;
        if (currentActivity != null)
        {
            currentActivity.RecordException(ex);
        }
    }
}

// Middleware para tracing automático
public class TracingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITracingService _tracingService;
    private readonly ILogger<TracingMiddleware> _logger;

    public TracingMiddleware(RequestDelegate next, ITracingService tracingService, ILogger<TracingMiddleware> logger)
    {
        _next = next;
        _tracingService = tracingService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var operationName = $"{context.Request.Method} {context.Request.Path}";
        
        using var activity = _tracingService.StartActivity(operationName, new Dictionary<string, object>
        {
            ["http.method"] = context.Request.Method,
            ["http.url"] = context.Request.Path,
            ["http.user_agent"] = context.Request.Headers["User-Agent"].ToString(),
            ["http.request_id"] = context.TraceIdentifier
        });

        try
        {
            var sw = Stopwatch.StartNew();
            
            await _next(context);
            
            sw.Stop();
            
            _tracingService.SetTag("http.status_code", context.Response.StatusCode);
            _tracingService.SetTag("http.response_time_ms", sw.ElapsedMilliseconds);
            
            _logger.LogInformation("Request {Method} {Path} completed in {ElapsedMs}ms with status {StatusCode}",
                context.Request.Method, context.Request.Path, sw.ElapsedMilliseconds, context.Response.StatusCode);
        }
        catch (Exception ex)
        {
            _tracingService.RecordException(ex);
            _tracingService.SetTag("error", true);
            _tracingService.SetTag("error.message", ex.Message);
            
            _logger.LogError(ex, "Request {Method} {Path} failed", context.Request.Method, context.Request.Path);
            throw;
        }
    }
}

// Configuración de OpenTelemetry
public static class OpenTelemetryConfiguration
{
    public static IServiceCollection AddOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation()
                    .AddRedisInstrumentation()
                    .AddSource("MussikOn.API")
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(serviceName: "MussikOn.API", serviceVersion: "1.0.0")
                        .AddAttributes(new KeyValuePair<string, object>[]
                        {
                            new("environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"),
                            new("deployment.region", Environment.GetEnvironmentVariable("DEPLOYMENT_REGION") ?? "Unknown")
                        }))
                    .AddJaegerExporter(options =>
                    {
                        options.AgentHost = configuration["Jaeger:Host"] ?? "localhost";
                        options.AgentPort = int.Parse(configuration["Jaeger:Port"] ?? "6831");
                    })
                    .AddConsoleExporter();
            })
            .WithMetrics(builder =>
            {
                builder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddPrometheusExporter();
            });

        return services;
    }
}
```

---

## 📈 **Dashboard y Alertas**

### **Sistema de Monitoreo y Alertas**

```csharp
public interface IMonitoringService
{
    Task SendAlertAsync(AlertLevel level, string message, Dictionary<string, object> context = null);
    Task<List<Alert>> GetActiveAlertsAsync();
    Task AcknowledgeAlertAsync(string alertId);
    Task ResolveAlertAsync(string alertId);
}

public enum AlertLevel
{
    Info,
    Warning,
    Critical,
    Emergency
}

public class Alert
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public AlertLevel Level { get; set; }
    public string Message { get; set; }
    public Dictionary<string, object> Context { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string AcknowledgedBy { get; set; }
    public string ResolvedBy { get; set; }
}

// Servicio de monitoreo con alertas
public class MonitoringService : IMonitoringService
{
    private readonly ILogger<MonitoringService> _logger;
    private readonly IEmailService _emailService;
    private readonly ISignalRNotificationService _signalRService;
    private readonly ConcurrentDictionary<string, Alert> _activeAlerts;
    private readonly IConfiguration _configuration;

    public MonitoringService(
        ILogger<MonitoringService> logger,
        IEmailService emailService,
        ISignalRNotificationService signalRService,
        IConfiguration configuration)
    {
        _logger = logger;
        _emailService = emailService;
        _signalRService = signalRService;
        _configuration = configuration;
        _activeAlerts = new ConcurrentDictionary<string, Alert>();
    }

    public async Task SendAlertAsync(AlertLevel level, string message, Dictionary<string, object> context = null)
    {
        var alert = new Alert
        {
            Level = level,
            Message = message,
            Context = context ?? new Dictionary<string, object>(),
            CreatedAt = DateTime.UtcNow
        };

        _activeAlerts.TryAdd(alert.Id, alert);

        _logger.LogWarning("Alert {Level}: {Message}", level, message);

        // Enviar notificación por SignalR
        await _signalRService.SendNotificationAsync("admin", new NotificationRequest
        {
            Title = $"Alert: {level}",
            Message = message,
            Type = level == AlertLevel.Critical || level == AlertLevel.Emergency ? NotificationType.Alert : NotificationType.Warning,
            Priority = level == AlertLevel.Emergency ? NotificationPriority.Urgent : NotificationPriority.High,
            Category = "Monitoring",
            Channels = new List<NotificationChannel> { NotificationChannel.SignalR, NotificationChannel.Email }
        });

        // Enviar email para alertas críticas
        if (level == AlertLevel.Critical || level == AlertLevel.Emergency)
        {
            var adminEmails = _configuration.GetSection("Monitoring:AdminEmails").Get<string[]>();
            if (adminEmails != null)
            {
                foreach (var email in adminEmails)
                {
                    await _emailService.SendEmailAsync(email, $"Critical Alert: {level}", message, "AlertTemplate");
                }
            }
        }
    }

    public async Task<List<Alert>> GetActiveAlertsAsync()
    {
        return _activeAlerts.Values
            .Where(a => a.ResolvedAt == null)
            .OrderByDescending(a => a.Level)
            .ThenByDescending(a => a.CreatedAt)
            .ToList();
    }

    public async Task AcknowledgeAlertAsync(string alertId)
    {
        if (_activeAlerts.TryGetValue(alertId, out var alert))
        {
            alert.AcknowledgedAt = DateTime.UtcNow;
            alert.AcknowledgedBy = "CurrentUser"; // Obtener del contexto de usuario
            
            _logger.LogInformation("Alert {AlertId} acknowledged", alertId);
        }
    }

    public async Task ResolveAlertAsync(string alertId)
    {
        if (_activeAlerts.TryRemove(alertId, out var alert))
        {
            alert.ResolvedAt = DateTime.UtcNow;
            alert.ResolvedBy = "CurrentUser"; // Obtener del contexto de usuario
            
            _logger.LogInformation("Alert {AlertId} resolved", alertId);
        }
    }
}

// Configuración de monitoreo
public static class MonitoringConfiguration
{
    public static IServiceCollection AddMonitoring(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IMonitoringService, MonitoringService>();
        
        // Configurar métricas de Prometheus
        services.AddMetrics();
        
        // Configurar health checks
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database")
            .AddCheck<RedisHealthCheck>("redis");
        
        return services;
    }
}
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Sistema de Logging Estructurado**
```csharp
// Implementa un sistema que:
// - Use Serilog con enriquecimiento de contexto
// - Implemente correlation ID
// - Envíe logs a múltiples destinos
// - Estructure logs para análisis
```

### **Ejercicio 2: Métricas y Health Checks**
```csharp
// Crea un sistema que:
// - Monitoree métricas de negocio
// - Implemente health checks personalizados
// - Genere alertas automáticas
// - Proporcione dashboards en tiempo real
```

### **Ejercicio 3: Trazabilidad Distribuida**
```csharp
// Implementa un sistema que:
// - Use OpenTelemetry para tracing
// - Correlacione requests entre servicios
// - Visualice flujos de datos
// - Analice performance end-to-end
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **📝 Logging Estructurado**: Sistema avanzado con Serilog y enriquecimiento de contexto
2. **📊 Métricas y Health Checks**: Monitoreo en tiempo real con Prometheus
3. **🔍 Trazabilidad Distribuida**: OpenTelemetry para tracing entre servicios
4. **📈 Dashboard y Alertas**: Sistema de monitoreo y notificaciones automáticas
5. **🚨 Observabilidad**: Patrones para monitorear aplicaciones en producción

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre el **Proyecto Final**, integrando todos los conceptos aprendidos en un sistema completo.

---

**¡Has completado la novena clase del Módulo 14! 📊🔍**
