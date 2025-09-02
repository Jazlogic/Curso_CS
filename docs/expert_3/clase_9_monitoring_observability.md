# 📊 **Clase 9: Monitoring y Observabilidad en la Nube**

## 🎯 **Objetivo de la Clase**
Implementar sistemas completos de monitoreo y observabilidad para aplicaciones cloud native, incluyendo métricas, logs, traces y alertas.

## 📚 **Contenido Teórico**

### **1. Application Insights Avanzado**

#### **Configuración de Application Insights**
```csharp
// Services/ApplicationInsightsService.cs
public class ApplicationInsightsService
{
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<ApplicationInsightsService> _logger;

    public ApplicationInsightsService(TelemetryClient telemetryClient, ILogger<ApplicationInsightsService> logger)
    {
        _telemetryClient = telemetryClient;
        _logger = logger;
    }

    public void TrackCustomEvent(string eventName, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null)
    {
        try
        {
            _telemetryClient.TrackEvent(eventName, properties, metrics);
            _logger.LogDebug("Custom event tracked: {EventName}", eventName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking custom event: {EventName}", eventName);
        }
    }

    public void TrackCustomMetric(string metricName, double value, Dictionary<string, string> properties = null)
    {
        try
        {
            _telemetryClient.TrackMetric(metricName, value, properties);
            _logger.LogDebug("Custom metric tracked: {MetricName} = {Value}", metricName, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking custom metric: {MetricName}", metricName);
        }
    }

    public void TrackDependency(string dependencyName, string commandName, DateTime startTime, TimeSpan duration, bool success)
    {
        try
        {
            _telemetryClient.TrackDependency(dependencyName, commandName, startTime, duration, success);
            _logger.LogDebug("Dependency tracked: {DependencyName} - {Duration}ms", dependencyName, duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking dependency: {DependencyName}", dependencyName);
        }
    }

    public void TrackException(Exception exception, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null)
    {
        try
        {
            _telemetryClient.TrackException(exception, properties, metrics);
            _logger.LogError(exception, "Exception tracked in Application Insights");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking exception in Application Insights");
        }
    }

    public void TrackRequest(string name, DateTime startTime, TimeSpan duration, string responseCode, bool success)
    {
        try
        {
            _telemetryClient.TrackRequest(name, startTime, duration, responseCode, success);
            _logger.LogDebug("Request tracked: {Name} - {Duration}ms - {ResponseCode}", name, duration.TotalMilliseconds, responseCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking request: {Name}", name);
        }
    }

    public void TrackUserAction(string userId, string action, string resource, Dictionary<string, string> properties = null)
    {
        try
        {
            var eventProperties = new Dictionary<string, string>
            {
                { "UserId", userId },
                { "Action", action },
                { "Resource", resource }
            };

            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    eventProperties[prop.Key] = prop.Value;
                }
            }

            _telemetryClient.TrackEvent("UserAction", eventProperties);
            _logger.LogInformation("User action tracked: {UserId} - {Action} - {Resource}", userId, action, resource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking user action: {UserId} - {Action}", userId, action);
        }
    }

    public void TrackBusinessMetric(string metricName, double value, string category, Dictionary<string, string> dimensions = null)
    {
        try
        {
            var properties = new Dictionary<string, string>
            {
                { "Category", category }
            };

            if (dimensions != null)
            {
                foreach (var dimension in dimensions)
                {
                    properties[dimension.Key] = dimension.Value;
                }
            }

            _telemetryClient.TrackMetric(metricName, value, properties);
            _logger.LogDebug("Business metric tracked: {MetricName} = {Value} ({Category})", metricName, value, category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking business metric: {MetricName}", metricName);
        }
    }
}
```

### **2. Distributed Tracing**

#### **Implementación de Distributed Tracing**
```csharp
// Services/DistributedTracingService.cs
public class DistributedTracingService
{
    private readonly ILogger<DistributedTracingService> _logger;
    private readonly ActivitySource _activitySource;

    public DistributedTracingService(ILogger<DistributedTracingService> logger)
    {
        _logger = logger;
        _activitySource = new ActivitySource("MussikOn.DistributedTracing");
    }

    public async Task<T> TraceAsync<T>(string operationName, Func<Task<T>> operation, Dictionary<string, string> tags = null)
    {
        using var activity = _activitySource.StartActivity(operationName);
        
        try
        {
            if (activity != null)
            {
                activity.SetTag("service.name", "MussikOn.API");
                activity.SetTag("service.version", "1.0.0");
                
                if (tags != null)
                {
                    foreach (var tag in tags)
                    {
                        activity.SetTag(tag.Key, tag.Value);
                    }
                }
            }

            var result = await operation();
            
            if (activity != null)
            {
                activity.SetStatus(ActivityStatusCode.Ok);
            }

            return result;
        }
        catch (Exception ex)
        {
            if (activity != null)
            {
                activity.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity.SetTag("error", true);
                activity.SetTag("error.message", ex.Message);
            }

            _logger.LogError(ex, "Error in traced operation: {OperationName}", operationName);
            throw;
        }
    }

    public async Task TraceAsync(string operationName, Func<Task> operation, Dictionary<string, string> tags = null)
    {
        using var activity = _activitySource.StartActivity(operationName);
        
        try
        {
            if (activity != null)
            {
                activity.SetTag("service.name", "MussikOn.API");
                activity.SetTag("service.version", "1.0.0");
                
                if (tags != null)
                {
                    foreach (var tag in tags)
                    {
                        activity.SetTag(tag.Key, tag.Value);
                    }
                }
            }

            await operation();
            
            if (activity != null)
            {
                activity.SetStatus(ActivityStatusCode.Ok);
            }
        }
        catch (Exception ex)
        {
            if (activity != null)
            {
                activity.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity.SetTag("error", true);
                activity.SetTag("error.message", ex.Message);
            }

            _logger.LogError(ex, "Error in traced operation: {OperationName}", operationName);
            throw;
        }
    }

    public void AddBaggage(string key, string value)
    {
        Activity.Current?.SetBaggage(key, value);
    }

    public string GetBaggage(string key)
    {
        return Activity.Current?.GetBaggage(key);
    }
}

// Middleware/DistributedTracingMiddleware.cs
public class DistributedTracingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly DistributedTracingService _tracingService;
    private readonly ILogger<DistributedTracingMiddleware> _logger;

    public DistributedTracingMiddleware(
        RequestDelegate next, 
        DistributedTracingService tracingService, 
        ILogger<DistributedTracingMiddleware> logger)
    {
        _next = next;
        _tracingService = tracingService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var operationName = $"{context.Request.Method} {context.Request.Path}";
        
        await _tracingService.TraceAsync(operationName, async () =>
        {
            var tags = new Dictionary<string, string>
            {
                { "http.method", context.Request.Method },
                { "http.url", context.Request.Path },
                { "http.user_agent", context.Request.Headers.UserAgent.ToString() },
                { "http.client_ip", context.Connection.RemoteIpAddress?.ToString() ?? "unknown" }
            };

            await _next(context);

            tags["http.status_code"] = context.Response.StatusCode.ToString();
            tags["http.response_size"] = context.Response.ContentLength?.ToString() ?? "unknown";
        }, tags);
    }
}
```

### **3. Structured Logging**

#### **Servicio de Logging Estructurado**
```csharp
// Services/StructuredLoggingService.cs
public class StructuredLoggingService
{
    private readonly ILogger<StructuredLoggingService> _logger;
    private readonly IEventStore _eventStore;

    public StructuredLoggingService(ILogger<StructuredLoggingService> logger, IEventStore eventStore)
    {
        _logger = logger;
        _eventStore = eventStore;
    }

    public void LogUserAction(string userId, string action, string resource, object data = null)
    {
        var logData = new
        {
            UserId = userId,
            Action = action,
            Resource = resource,
            Data = data,
            Timestamp = DateTime.UtcNow,
            EventType = "UserAction"
        };

        _logger.LogInformation("User action: {UserId} performed {Action} on {Resource} with data {@Data}", 
            userId, action, resource, data);
    }

    public void LogBusinessEvent(string eventType, string description, object data = null)
    {
        var logData = new
        {
            EventType = eventType,
            Description = description,
            Data = data,
            Timestamp = DateTime.UtcNow
        };

        _logger.LogInformation("Business event: {EventType} - {Description} with data {@Data}", 
            eventType, description, data);
    }

    public void LogPerformanceMetric(string metricName, double value, string category, Dictionary<string, string> dimensions = null)
    {
        var logData = new
        {
            MetricName = metricName,
            Value = value,
            Category = category,
            Dimensions = dimensions,
            Timestamp = DateTime.UtcNow
        };

        _logger.LogInformation("Performance metric: {MetricName} = {Value} ({Category}) with dimensions {@Dimensions}", 
            metricName, value, category, dimensions);
    }

    public void LogSecurityEvent(string eventType, string description, string userId = null, object data = null)
    {
        var logData = new
        {
            EventType = eventType,
            Description = description,
            UserId = userId,
            Data = data,
            Timestamp = DateTime.UtcNow,
            Severity = "Security"
        };

        _logger.LogWarning("Security event: {EventType} - {Description} for user {UserId} with data {@Data}", 
            eventType, description, userId, data);
    }

    public void LogError(Exception exception, string context = null, object data = null)
    {
        var logData = new
        {
            Exception = exception.Message,
            StackTrace = exception.StackTrace,
            Context = context,
            Data = data,
            Timestamp = DateTime.UtcNow,
            Severity = "Error"
        };

        _logger.LogError(exception, "Error occurred in context {Context} with data {@Data}", context, data);
    }

    public void LogAuditEvent(string userId, string action, string resource, string result, object data = null)
    {
        var logData = new
        {
            UserId = userId,
            Action = action,
            Resource = resource,
            Result = result,
            Data = data,
            Timestamp = DateTime.UtcNow,
            EventType = "Audit"
        };

        _logger.LogInformation("Audit event: {UserId} performed {Action} on {Resource} with result {Result} and data {@Data}", 
            userId, action, resource, result, data);
    }
}
```

### **4. Health Checks Avanzados**

#### **Implementación de Health Checks**
```csharp
// Services/HealthCheckService.cs
public class HealthCheckService
{
    private readonly ILogger<HealthCheckService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public HealthCheckService(ILogger<HealthCheckService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<HealthCheckResult> CheckDatabaseHealthAsync()
    {
        try
        {
            var connectionString = _serviceProvider.GetRequiredService<IConfiguration>()
                .GetConnectionString("DefaultConnection");
            
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync();
            
            return new HealthCheckResult
            {
                Status = HealthStatus.Healthy,
                Description = "Database connection successful",
                Duration = TimeSpan.Zero
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return new HealthCheckResult
            {
                Status = HealthStatus.Unhealthy,
                Description = $"Database connection failed: {ex.Message}",
                Duration = TimeSpan.Zero
            };
        }
    }

    public async Task<HealthCheckResult> CheckRedisHealthAsync()
    {
        try
        {
            var redis = _serviceProvider.GetRequiredService<IDistributedCache>();
            await redis.SetStringAsync("health_check", "ok", new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
            });
            
            var value = await redis.GetStringAsync("health_check");
            
            if (value == "ok")
            {
                return new HealthCheckResult
                {
                    Status = HealthStatus.Healthy,
                    Description = "Redis connection successful",
                    Duration = TimeSpan.Zero
                };
            }
            
            return new HealthCheckResult
            {
                Status = HealthStatus.Unhealthy,
                Description = "Redis health check failed",
                Duration = TimeSpan.Zero
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed");
            return new HealthCheckResult
            {
                Status = HealthStatus.Unhealthy,
                Description = $"Redis connection failed: {ex.Message}",
                Duration = TimeSpan.Zero
            };
        }
    }

    public async Task<HealthCheckResult> CheckExternalApiHealthAsync()
    {
        try
        {
            var httpClient = _serviceProvider.GetRequiredService<HttpClient>();
            var response = await httpClient.GetAsync("/health");
            
            if (response.IsSuccessStatusCode)
            {
                return new HealthCheckResult
                {
                    Status = HealthStatus.Healthy,
                    Description = "External API is responding",
                    Duration = TimeSpan.Zero
                };
            }
            
            return new HealthCheckResult
            {
                Status = HealthStatus.Unhealthy,
                Description = $"External API returned status: {response.StatusCode}",
                Duration = TimeSpan.Zero
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "External API health check failed");
            return new HealthCheckResult
            {
                Status = HealthStatus.Unhealthy,
                Description = $"External API connection failed: {ex.Message}",
                Duration = TimeSpan.Zero
            };
        }
    }

    public async Task<HealthCheckResult> CheckStorageHealthAsync()
    {
        try
        {
            var blobService = _serviceProvider.GetRequiredService<BlobStorageService>();
            var testBlobName = $"health_check_{Guid.NewGuid()}";
            var testContent = "health check";
            
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));
            await blobService.UploadFileAsync(stream, testBlobName, "health-checks");
            
            return new HealthCheckResult
            {
                Status = HealthStatus.Healthy,
                Description = "Storage connection successful",
                Duration = TimeSpan.Zero
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Storage health check failed");
            return new HealthCheckResult
            {
                Status = HealthStatus.Unhealthy,
                Description = $"Storage connection failed: {ex.Message}",
                Duration = TimeSpan.Zero
            };
        }
    }
}

// Models/HealthCheckResult.cs
public class HealthCheckResult
{
    public HealthStatus Status { get; set; }
    public string Description { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

// Enums/HealthStatus.cs
public enum HealthStatus
{
    Healthy,
    Degraded,
    Unhealthy
}
```

### **5. Alerting y Notifications**

#### **Servicio de Alertas**
```csharp
// Services/AlertingService.cs
public class AlertingService
{
    private readonly ILogger<AlertingService> _logger;
    private readonly INotificationService _notificationService;
    private readonly IEventStore _eventStore;

    public AlertingService(
        ILogger<AlertingService> logger, 
        INotificationService notificationService, 
        IEventStore eventStore)
    {
        _logger = logger;
        _notificationService = notificationService;
        _eventStore = eventStore;
    }

    public async Task SendAlertAsync(Alert alert)
    {
        try
        {
            // Log alert
            _logger.LogWarning("Alert triggered: {AlertType} - {Message}", alert.Type, alert.Message);
            
            // Store alert in event store
            await _eventStore.SaveEventsAsync(Guid.Parse(alert.Id), new[] { alert }, 1);
            
            // Send notifications based on severity
            switch (alert.Severity)
            {
                case AlertSeverity.Critical:
                    await SendCriticalAlertAsync(alert);
                    break;
                case AlertSeverity.Warning:
                    await SendWarningAlertAsync(alert);
                    break;
                case AlertSeverity.Info:
                    await SendInfoAlertAsync(alert);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending alert: {AlertId}", alert.Id);
        }
    }

    public async Task CheckThresholdsAsync(PerformanceMetrics metrics)
    {
        try
        {
            var alerts = new List<Alert>();

            // Check CPU threshold
            if (metrics.CpuUsage > 90)
            {
                alerts.Add(new Alert
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "HighCPUUsage",
                    Severity = AlertSeverity.Critical,
                    Message = $"Critical CPU usage: {metrics.CpuUsage:F2}%",
                    ServiceName = metrics.ServiceName,
                    Timestamp = DateTime.UtcNow,
                    Data = new Dictionary<string, object>
                    {
                        { "CpuUsage", metrics.CpuUsage },
                        { "Threshold", 90 }
                    }
                });
            }

            // Check memory threshold
            if (metrics.MemoryUsage > 90)
            {
                alerts.Add(new Alert
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "HighMemoryUsage",
                    Severity = AlertSeverity.Critical,
                    Message = $"Critical memory usage: {metrics.MemoryUsage:F2}%",
                    ServiceName = metrics.ServiceName,
                    Timestamp = DateTime.UtcNow,
                    Data = new Dictionary<string, object>
                    {
                        { "MemoryUsage", metrics.MemoryUsage },
                        { "Threshold", 90 }
                    }
                });
            }

            // Check error rate threshold
            if (metrics.ErrorRate > 10)
            {
                alerts.Add(new Alert
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "HighErrorRate",
                    Severity = AlertSeverity.Critical,
                    Message = $"High error rate: {metrics.ErrorRate:F2}%",
                    ServiceName = metrics.ServiceName,
                    Timestamp = DateTime.UtcNow,
                    Data = new Dictionary<string, object>
                    {
                        { "ErrorRate", metrics.ErrorRate },
                        { "Threshold", 10 }
                    }
                });
            }

            // Send all alerts
            foreach (var alert in alerts)
            {
                await SendAlertAsync(alert);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking thresholds for service: {ServiceName}", metrics.ServiceName);
        }
    }

    private async Task SendCriticalAlertAsync(Alert alert)
    {
        // Send to multiple channels for critical alerts
        await _notificationService.SendEmailAsync("admin@mussikon.com", "Critical Alert", alert.Message);
        await _notificationService.SendSmsAsync("+1234567890", alert.Message);
        await _notificationService.SendSlackMessageAsync("#alerts", alert.Message);
    }

    private async Task SendWarningAlertAsync(Alert alert)
    {
        // Send to email and Slack for warnings
        await _notificationService.SendEmailAsync("admin@mussikon.com", "Warning Alert", alert.Message);
        await _notificationService.SendSlackMessageAsync("#alerts", alert.Message);
    }

    private async Task SendInfoAlertAsync(Alert alert)
    {
        // Send to Slack only for info alerts
        await _notificationService.SendSlackMessageAsync("#alerts", alert.Message);
    }
}

// Models/Alert.cs
public class Alert : DomainEvent
{
    public string Type { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
}

// Enums/AlertSeverity.cs
public enum AlertSeverity
{
    Info,
    Warning,
    Critical
}
```

## 🛠️ **Ejercicio Práctico**

### **Ejercicio 1: Implementar Sistema de Monitoreo Completo**

Crea un sistema completo de monitoreo y observabilidad:

```csharp
// 1. Configurar servicios de monitoreo
public class MonitoringServiceConfig
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Application Insights
        services.AddApplicationInsightsTelemetry();
        services.AddSingleton<ApplicationInsightsService>();
        
        // Distributed Tracing
        services.AddSingleton<DistributedTracingService>();
        
        // Structured Logging
        services.AddSingleton<StructuredLoggingService>();
        
        // Health Checks
        services.AddSingleton<HealthCheckService>();
        
        // Alerting
        services.AddSingleton<AlertingService>();
    }
}

// 2. Implementar controlador de monitoreo
[ApiController]
[Route("api/[controller]")]
public class MonitoringController : ControllerBase
{
    private readonly ApplicationInsightsService _appInsightsService;
    private readonly DistributedTracingService _tracingService;
    private readonly StructuredLoggingService _loggingService;
    private readonly HealthCheckService _healthService;
    private readonly AlertingService _alertingService;

    [HttpGet("health")]
    public async Task<ActionResult<Dictionary<string, HealthCheckResult>>> GetHealth()
    {
        var results = new Dictionary<string, HealthCheckResult>
        {
            { "Database", await _healthService.CheckDatabaseHealthAsync() },
            { "Redis", await _healthService.CheckRedisHealthAsync() },
            { "ExternalApi", await _healthService.CheckExternalApiHealthAsync() },
            { "Storage", await _healthService.CheckStorageHealthAsync() }
        };

        return Ok(results);
    }

    [HttpPost("track-event")]
    public IActionResult TrackEvent([FromBody] TrackEventRequest request)
    {
        _appInsightsService.TrackCustomEvent(request.EventName, request.Properties, request.Metrics);
        return Ok();
    }

    [HttpPost("track-metric")]
    public IActionResult TrackMetric([FromBody] TrackMetricRequest request)
    {
        _appInsightsService.TrackCustomMetric(request.MetricName, request.Value, request.Properties);
        return Ok();
    }

    [HttpPost("log-user-action")]
    public IActionResult LogUserAction([FromBody] LogUserActionRequest request)
    {
        _loggingService.LogUserAction(request.UserId, request.Action, request.Resource, request.Data);
        return Ok();
    }
}

// 3. Implementar middleware de monitoreo
public class MonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ApplicationInsightsService _appInsightsService;
    private readonly DistributedTracingService _tracingService;
    private readonly ILogger<MonitoringMiddleware> _logger;

    public MonitoringMiddleware(
        RequestDelegate next, 
        ApplicationInsightsService appInsightsService,
        DistributedTracingService tracingService,
        ILogger<MonitoringMiddleware> logger)
    {
        _next = next;
        _appInsightsService = appInsightsService;
        _tracingService = tracingService;
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
            
            // Track request in Application Insights
            _appInsightsService.TrackRequest(
                $"{context.Request.Method} {context.Request.Path}",
                DateTime.UtcNow - stopwatch.Elapsed,
                stopwatch.Elapsed,
                context.Response.StatusCode.ToString(),
                context.Response.StatusCode < 400);
            
            // Log structured information
            _logger.LogInformation("Request completed: {Method} {Path} - {StatusCode} in {ElapsedMs}ms",
                context.Request.Method, context.Request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
    }
}
```

## 📋 **Resumen de la Clase**

### **Conceptos Clave:**
- **Application Insights**: Telemetría y monitoreo
- **Distributed Tracing**: Trazabilidad distribuida
- **Structured Logging**: Logging estructurado
- **Health Checks**: Verificaciones de salud
- **Alerting**: Sistema de alertas
- **Observabilidad**: Visibilidad completa del sistema

### **Próxima Clase:**
**Proyecto Final: MussikOn Cloud Native** - Implementación completa

## 🎯 **Objetivos de Aprendizaje**

Al finalizar esta clase, serás capaz de:
- ✅ Implementar Application Insights
- ✅ Configurar distributed tracing
- ✅ Implementar structured logging
- ✅ Crear health checks
- ✅ Configurar sistema de alertas
- ✅ Implementar observabilidad completa
