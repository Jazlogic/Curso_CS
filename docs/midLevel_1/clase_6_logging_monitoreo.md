# 🚀 Clase 6: Logging y Monitoreo

## 📋 Información de la Clase

- **Módulo**: Mid Level 1 - Programación Avanzada y Patrones de Diseño
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 5 (Dependency Injection Avanzada)

## 🎯 Objetivos de Aprendizaje

- Implementar sistemas de logging estructurado con Serilog
- Configurar diferentes sinks y formatters para logs
- Implementar correlation IDs para trazabilidad
- Crear métricas de rendimiento y health checks
- Implementar monitoreo en tiempo real

---

## 📚 Navegación del Módulo 4

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_patrones_diseno_intermedios.md) | Patrones de Diseño Intermedios | |
| [Clase 2](clase_2_programacion_asincrona_avanzada.md) | Programación Asíncrona Avanzada | |
| [Clase 3](clase_3_programacion_paralela.md) | Programación Paralela y TPL | |
| [Clase 4](clase_4_clean_architecture.md) | Clean Architecture | |
| [Clase 5](clase_5_dependency_injection.md) | Dependency Injection Avanzada | ← Anterior |
| **Clase 6** | **Logging y Monitoreo** | ← Estás aquí |
| [Clase 7](clase_7_refactoring_clean_code.md) | Refactoring y Clean Code | Siguiente → |
| [Clase 8](clase_8_testing_integracion.md) | Testing de Integración | |
| [Clase 9](clase_9_testing_comportamiento.md) | Testing de Comportamiento (BDD) | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de E-commerce | |

**← [Volver al README del Módulo 4](../midLevel_1/README.md)**

---

## 📚 Contenido Teórico

### 1. Sistema de Logging Estructurado con Serilog

Serilog proporciona logging estructurado con capacidades avanzadas de filtrado y formateo.

```csharp
// Sistema de logging avanzado con Serilog
namespace AdvancedLogging
{
    using Serilog;
    using Serilog.Events;
    using Serilog.Formatting.Compact;
    using Serilog.Formatting.Json;
    using Serilog.Sinks.SystemConsole.Themes;
    
    // ===== INTERFACES DE LOGGING =====
    public interface ILoggerService
    {
        void LogInformation(string message, params object[] propertyValues);
        void LogWarning(string message, params object[] propertyValues);
        void LogError(string message, Exception exception = null, params object[] propertyValues);
        void LogDebug(string message, params object[] propertyValues);
        void LogCritical(string message, Exception exception = null, params object[] propertyValues);
        
        IDisposable BeginScope(string operationName, Dictionary<string, object> properties = null);
        void SetCorrelationId(string correlationId);
        void AddProperty(string key, object value);
    }
    
    public interface IMetricsService
    {
        void IncrementCounter(string name, Dictionary<string, string> labels = null);
        void RecordGauge(string name, double value, Dictionary<string, string> labels = null);
        void RecordHistogram(string name, double value, Dictionary<string, string> labels = null);
        void RecordTimer(string name, TimeSpan duration, Dictionary<string, string> labels = null);
        Dictionary<string, object> GetMetrics();
    }
    
    public interface IHealthCheckService
    {
        Task<HealthStatus> CheckHealthAsync();
        Task<HealthStatus> CheckDatabaseHealthAsync();
        Task<HealthStatus> CheckExternalServiceHealthAsync();
        void RegisterHealthCheck(string name, Func<Task<HealthStatus>> check);
        Dictionary<string, HealthStatus> GetAllHealthChecks();
    }
    
    // ===== ENUMERACIONES =====
    public enum HealthStatus
    {
        Healthy,
        Degraded,
        Unhealthy
    }
    
    // ===== IMPLEMENTACIONES =====
    namespace Services
    {
        // Servicio de logging principal con Serilog
        public class SerilogService : ILoggerService, IDisposable
        {
            private readonly ILogger _logger;
            private readonly Dictionary<string, object> _properties;
            private string _correlationId;
            
            public SerilogService()
            {
                _properties = new Dictionary<string, object>();
                _correlationId = Guid.NewGuid().ToString();
                
                // Configurar Serilog
                _logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Application", "AdvancedLoggingApp")
                    .Enrich.WithProperty("Version", "1.0.0")
                    .WriteTo.Console(
                        theme: AnsiConsoleTheme.Code,
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                    .WriteTo.File(
                        path: "logs/app-.log",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 31,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                    .WriteTo.File(
                        formatter: new CompactJsonFormatter(),
                        path: "logs/app-structured-.json",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 31)
                    .CreateLogger();
                
                Log.Information("SerilogService inicializado");
            }
            
            public void LogInformation(string message, params object[] propertyValues)
            {
                var enrichedProperties = EnrichProperties(_properties);
                _logger.Information(message, propertyValues);
            }
            
            public void LogWarning(string message, params object[] propertyValues)
            {
                var enrichedProperties = EnrichProperties(_properties);
                _logger.Warning(message, propertyValues);
            }
            
            public void LogError(string message, Exception exception = null, params object[] propertyValues)
            {
                var enrichedProperties = EnrichProperties(_properties);
                if (exception != null)
                {
                    _logger.Error(exception, message, propertyValues);
                }
                else
                {
                    _logger.Error(message, propertyValues);
                }
            }
            
            public void LogDebug(string message, params object[] propertyValues)
            {
                var enrichedProperties = EnrichProperties(_properties);
                _logger.Debug(message, propertyValues);
            }
            
            public void LogCritical(string message, Exception exception = null, params object[] propertyValues)
            {
                var enrichedProperties = EnrichProperties(_properties);
                if (exception != null)
                {
                    _logger.Fatal(exception, message, propertyValues);
                }
                else
                {
                    _logger.Fatal(message, propertyValues);
                }
            }
            
            public IDisposable BeginScope(string operationName, Dictionary<string, object> properties = null)
            {
                var scopeProperties = new Dictionary<string, object>(_properties);
                if (properties != null)
                {
                    foreach (var kvp in properties)
                    {
                        scopeProperties[kvp.Key] = kvp.Value;
                    }
                }
                
                scopeProperties["Operation"] = operationName;
                scopeProperties["CorrelationId"] = _correlationId;
                
                return LogContext.PushProperty("Scope", scopeProperties);
            }
            
            public void SetCorrelationId(string correlationId)
            {
                _correlationId = correlationId;
                _properties["CorrelationId"] = correlationId;
            }
            
            public void AddProperty(string key, object value)
            {
                _properties[key] = value;
            }
            
            private Dictionary<string, object> EnrichProperties(Dictionary<string, object> properties)
            {
                var enriched = new Dictionary<string, object>(properties);
                enriched["CorrelationId"] = _correlationId;
                enriched["Timestamp"] = DateTime.UtcNow;
                enriched["ThreadId"] = Thread.CurrentThread.ManagedThreadId;
                return enriched;
            }
            
            public void Dispose()
            {
                _logger?.Dispose();
            }
        }
        
        // Servicio de métricas en memoria
        public class InMemoryMetricsService : IMetricsService
        {
            private readonly Dictionary<string, long> _counters;
            private readonly Dictionary<string, double> _gauges;
            private readonly Dictionary<string, List<double>> _histograms;
            private readonly Dictionary<string, List<TimeSpan>> _timers;
            private readonly object _lockObject;
            
            public InMemoryMetricsService()
            {
                _counters = new Dictionary<string, long>();
                _gauges = new Dictionary<string, double>();
                _histograms = new Dictionary<string, List<double>>();
                _timers = new Dictionary<string, List<TimeSpan>>();
                _lockObject = new object();
            }
            
            public void IncrementCounter(string name, Dictionary<string, string> labels = null)
            {
                var key = GetMetricKey(name, labels);
                lock (_lockObject)
                {
                    if (!_counters.ContainsKey(key))
                        _counters[key] = 0;
                    _counters[key]++;
                }
            }
            
            public void RecordGauge(string name, double value, Dictionary<string, string> labels = null)
            {
                var key = GetMetricKey(name, labels);
                lock (_lockObject)
                {
                    _gauges[key] = value;
                }
            }
            
            public void RecordHistogram(string name, double value, Dictionary<string, string> labels = null)
            {
                var key = GetMetricKey(name, labels);
                lock (_lockObject)
                {
                    if (!_histograms.ContainsKey(key))
                        _histograms[key] = new List<double>();
                    _histograms[key].Add(value);
                }
            }
            
            public void RecordTimer(string name, TimeSpan duration, Dictionary<string, string> labels = null)
            {
                var key = GetMetricKey(name, labels);
                lock (_lockObject)
                {
                    if (!_timers.ContainsKey(key))
                        _timers[key] = new List<TimeSpan>();
                    _timers[key].Add(duration);
                }
            }
            
            public Dictionary<string, object> GetMetrics()
            {
                lock (_lockObject)
                {
                    var metrics = new Dictionary<string, object>
                    {
                        ["Counters"] = _counters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                        ["Gauges"] = _gauges.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                        ["Histograms"] = _histograms.ToDictionary(kvp => kvp.Key, kvp => new
                        {
                            Count = kvp.Value.Count,
                            Min = kvp.Value.Min(),
                            Max = kvp.Value.Max(),
                            Average = kvp.Value.Average(),
                            Percentile95 = CalculatePercentile(kvp.Value, 95),
                            Percentile99 = CalculatePercentile(kvp.Value, 99)
                        }),
                        ["Timers"] = _timers.ToDictionary(kvp => kvp.Key, kvp => new
                        {
                            Count = kvp.Value.Count,
                            Min = kvp.Value.Min(),
                            Max = kvp.Value.Max(),
                            Average = TimeSpan.FromTicks((long)kvp.Value.Average(ts => ts.Ticks)),
                            Percentile95 = CalculatePercentile(kvp.Value.Select(ts => ts.TotalMilliseconds), 95),
                            Percentile99 = CalculatePercentile(kvp.Value.Select(ts => ts.TotalMilliseconds), 99)
                        })
                    };
                    
                    return metrics;
                }
            }
            
            private string GetMetricKey(string name, Dictionary<string, string> labels)
            {
                if (labels == null || !labels.Any())
                    return name;
                
                var labelString = string.Join("_", labels.Select(kvp => $"{kvp.Key}_{kvp.Value}"));
                return $"{name}_{labelString}";
            }
            
            private double CalculatePercentile(IEnumerable<double> values, int percentile)
            {
                var sortedValues = values.OrderBy(v => v).ToList();
                if (!sortedValues.Any())
                    return 0;
                
                var index = (int)Math.Ceiling((percentile / 100.0) * sortedValues.Count) - 1;
                return sortedValues[Math.Max(0, index)];
            }
        }
        
        // Servicio de health checks
        public class HealthCheckService : IHealthCheckService
        {
            private readonly Dictionary<string, Func<Task<HealthStatus>>> _healthChecks;
            private readonly ILoggerService _logger;
            private readonly IMetricsService _metrics;
            
            public HealthCheckService(ILoggerService logger, IMetricsService metrics)
            {
                _healthChecks = new Dictionary<string, Func<Task<HealthStatus>>>();
                _logger = logger;
                _metrics = metrics;
                
                // Registrar health checks por defecto
                RegisterDefaultHealthChecks();
            }
            
            private void RegisterDefaultHealthChecks()
            {
                RegisterHealthCheck("System", async () =>
                {
                    try
                    {
                        // Verificar memoria disponible
                        var memoryInfo = GC.GetGCMemoryInfo();
                        var availableMemoryMB = memoryInfo.TotalAvailableMemoryBytes / (1024 * 1024);
                        
                        if (availableMemoryMB < 100)
                        {
                            _logger.LogWarning("Memoria disponible baja: {AvailableMemoryMB} MB", availableMemoryMB);
                            return HealthStatus.Degraded;
                        }
                        
                        // Verificar CPU (simulado)
                        var cpuUsage = GetSimulatedCpuUsage();
                        if (cpuUsage > 90)
                        {
                            _logger.LogWarning("Uso de CPU alto: {CpuUsage}%", cpuUsage);
                            return HealthStatus.Degraded;
                        }
                        
                        return HealthStatus.Healthy;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error en health check del sistema", ex);
                        return HealthStatus.Unhealthy;
                    }
                });
                
                RegisterHealthCheck("Database", CheckDatabaseHealthAsync);
                RegisterHealthCheck("ExternalService", CheckExternalServiceHealthAsync);
            }
            
            public async Task<HealthStatus> CheckHealthAsync()
            {
                var overallStatus = HealthStatus.Healthy;
                var checkResults = new Dictionary<string, HealthStatus>();
                
                foreach (var healthCheck in _healthChecks)
                {
                    try
                    {
                        var status = await healthCheck.Value();
                        checkResults[healthCheck.Key] = status;
                        
                        if (status == HealthStatus.Unhealthy)
                            overallStatus = HealthStatus.Unhealthy;
                        else if (status == HealthStatus.Degraded && overallStatus != HealthStatus.Unhealthy)
                            overallStatus = HealthStatus.Degraded;
                        
                        _logger.LogInformation("Health check {CheckName}: {Status}", healthCheck.Key, status);
                    }
                    catch (Exception ex)
                    {
                        checkResults[healthCheck.Key] = HealthStatus.Unhealthy;
                        overallStatus = HealthStatus.Unhealthy;
                        _logger.LogError("Error en health check {CheckName}", ex, healthCheck.Key);
                    }
                }
                
                // Registrar métricas
                _metrics.RecordGauge("health_overall_status", (int)overallStatus);
                _metrics.RecordGauge("health_checks_total", checkResults.Count);
                _metrics.RecordGauge("health_checks_healthy", checkResults.Count(kvp => kvp.Value == HealthStatus.Healthy));
                _metrics.RecordGauge("health_checks_degraded", checkResults.Count(kvp => kvp.Value == HealthStatus.Degraded));
                _metrics.RecordGauge("health_checks_unhealthy", checkResults.Count(kvp => kvp.Value == HealthStatus.Unhealthy));
                
                return overallStatus;
            }
            
            public async Task<HealthStatus> CheckDatabaseHealthAsync()
            {
                try
                {
                    // Simular verificación de base de datos
                    await Task.Delay(100);
                    
                    var random = new Random();
                    var responseTime = random.Next(10, 200);
                    
                    if (responseTime > 150)
                    {
                        _logger.LogWarning("Base de datos lenta - Response time: {ResponseTime}ms", responseTime);
                        return HealthStatus.Degraded;
                    }
                    
                    if (responseTime > 500)
                    {
                        _logger.LogError("Base de datos no responde - Response time: {ResponseTime}ms", responseTime);
                        return HealthStatus.Unhealthy;
                    }
                    
                    _metrics.RecordHistogram("database_response_time", responseTime);
                    return HealthStatus.Healthy;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error verificando base de datos", ex);
                    return HealthStatus.Unhealthy;
                }
            }
            
            public async Task<HealthStatus> CheckExternalServiceHealthAsync()
            {
                try
                {
                    // Simular verificación de servicio externo
                    await Task.Delay(50);
                    
                    var random = new Random();
                    var isAvailable = random.Next(100) > 5; // 95% de disponibilidad
                    
                    if (!isAvailable)
                    {
                        _logger.LogWarning("Servicio externo no disponible");
                        return HealthStatus.Degraded;
                    }
                    
                    var responseTime = random.Next(20, 100);
                    _metrics.RecordHistogram("external_service_response_time", responseTime);
                    
                    return HealthStatus.Healthy;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error verificando servicio externo", ex);
                    return HealthStatus.Unhealthy;
                }
            }
            
            public void RegisterHealthCheck(string name, Func<Task<HealthStatus>> check)
            {
                _healthChecks[name] = check;
                _logger.LogInformation("Health check registrado: {CheckName}", name);
            }
            
            public Dictionary<string, HealthStatus> GetAllHealthChecks()
            {
                var results = new Dictionary<string, HealthStatus>();
                foreach (var healthCheck in _healthChecks.Keys)
                {
                    // Ejecutar health check de forma síncrona para obtener estado actual
                    try
                    {
                        var task = _healthChecks[healthCheck]();
                        task.Wait();
                        results[healthCheck] = task.Result;
                    }
                    catch
                    {
                        results[healthCheck] = HealthStatus.Unhealthy;
                    }
                }
                return results;
            }
            
            private double GetSimulatedCpuUsage()
            {
                // Simular uso de CPU
                var random = new Random();
                return random.Next(20, 80);
            }
        }
        
        // Servicio de monitoreo en tiempo real
        public class RealTimeMonitoringService
        {
            private readonly ILoggerService _logger;
            private readonly IMetricsService _metrics;
            private readonly IHealthCheckService _healthCheck;
            private readonly Timer _monitoringTimer;
            private readonly Timer _metricsCollectionTimer;
            private readonly Timer _healthCheckTimer;
            
            public RealTimeMonitoringService(
                ILoggerService logger,
                IMetricsService metrics,
                IHealthCheckService healthCheck)
            {
                _logger = logger;
                _metrics = metrics;
                _healthCheck = healthCheck;
                
                // Timer para monitoreo general cada 30 segundos
                _monitoringTimer = new Timer(PerformMonitoring, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
                
                // Timer para recolección de métricas cada 10 segundos
                _metricsCollectionTimer = new Timer(CollectMetrics, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
                
                // Timer para health checks cada 60 segundos
                _healthCheckTimer = new Timer(PerformHealthChecks, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
                
                _logger.LogInformation("Servicio de monitoreo en tiempo real inicializado");
            }
            
            private void PerformMonitoring(object state)
            {
                try
                {
                    var memoryInfo = GC.GetGCMemoryInfo();
                    var availableMemoryMB = memoryInfo.TotalAvailableMemoryBytes / (1024 * 1024);
                    var totalMemoryMB = memoryInfo.TotalCommittedMemoryBytes / (1024 * 1024);
                    
                    _metrics.RecordGauge("memory_available_mb", availableMemoryMB);
                    _metrics.RecordGauge("memory_total_mb", totalMemoryMB);
                    _metrics.RecordGauge("memory_usage_percent", (double)totalMemoryMB / (totalMemoryMB + availableMemoryMB) * 100);
                    
                    var threadCount = Process.GetCurrentProcess().Threads.Count;
                    _metrics.RecordGauge("thread_count", threadCount);
                    
                    var processTime = Process.GetCurrentProcess().TotalProcessorTime;
                    _metrics.RecordGauge("process_cpu_time_seconds", processTime.TotalSeconds);
                    
                    _logger.LogDebug("Monitoreo del sistema completado - Memoria: {AvailableMB}MB, Threads: {ThreadCount}", 
                        availableMemoryMB, threadCount);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error en monitoreo del sistema", ex);
                }
            }
            
            private void CollectMetrics(object state)
            {
                try
                {
                    var metrics = _metrics.GetMetrics();
                    
                    // Registrar métricas agregadas
                    if (metrics.ContainsKey("Counters"))
                    {
                        var counters = (Dictionary<string, object>)metrics["Counters"];
                        foreach (var counter in counters)
                        {
                            _logger.LogDebug("Métrica contador: {CounterName} = {Value}", counter.Key, counter.Value);
                        }
                    }
                    
                    if (metrics.ContainsKey("Gauges"))
                    {
                        var gauges = (Dictionary<string, object>)metrics["Gauges"];
                        foreach (var gauge in gauges)
                        {
                            _logger.LogDebug("Métrica gauge: {GaugeName} = {Value}", gauge.Key, gauge.Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error recolectando métricas", ex);
                }
            }
            
            private void PerformHealthChecks(object state)
            {
                try
                {
                    var healthStatus = _healthCheck.CheckHealthAsync().Result;
                    
                    if (healthStatus == HealthStatus.Unhealthy)
                    {
                        _logger.LogCritical("Sistema en estado no saludable");
                    }
                    else if (healthStatus == HealthStatus.Degraded)
                    {
                        _logger.LogWarning("Sistema en estado degradado");
                    }
                    else
                    {
                        _logger.LogInformation("Sistema en estado saludable");
                    }
                    
                    _metrics.RecordGauge("system_health_status", (int)healthStatus);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error en health checks", ex);
                }
            }
            
            public void StopMonitoring()
            {
                _monitoringTimer?.Dispose();
                _metricsCollectionTimer?.Dispose();
                _healthCheckTimer?.Dispose();
                _logger.LogInformation("Monitoreo en tiempo real detenido");
            }
        }
    }
    
    // ===== MIDDLEWARE Y EXTENSIONES =====
    namespace Middleware
    {
        // Middleware para logging de requests HTTP
        public class RequestLoggingMiddleware
        {
            private readonly RequestDelegate _next;
            private readonly ILoggerService _logger;
            private readonly IMetricsService _metrics;
            
            public RequestLoggingMiddleware(RequestDelegate next, ILoggerService logger, IMetricsService metrics)
            {
                _next = next;
                _logger = logger;
                _metrics = metrics;
            }
            
            public async Task InvokeAsync(HttpContext context)
            {
                var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
                var startTime = DateTime.UtcNow;
                
                using (_logger.BeginScope("HTTP Request", new Dictionary<string, object>
                {
                    ["CorrelationId"] = correlationId,
                    ["Method"] = context.Request.Method,
                    ["Path"] = context.Request.Path,
                    ["QueryString"] = context.Request.QueryString.ToString(),
                    ["UserAgent"] = context.Request.Headers["User-Agent"].ToString(),
                    ["RemoteIpAddress"] = context.Connection.RemoteIpAddress?.ToString()
                }))
                {
                    try
                    {
                        _logger.LogInformation("Iniciando request {Method} {Path}", context.Request.Method, context.Request.Path);
                        
                        // Incrementar contador de requests
                        _metrics.IncrementCounter("http_requests_total", new Dictionary<string, string>
                        {
                            ["method"] = context.Request.Method,
                            ["path"] = context.Request.Path.Value?.Split('/')[1] ?? "unknown"
                        });
                        
                        await _next(context);
                        
                        var duration = DateTime.UtcNow - startTime;
                        
                        // Registrar métricas de duración
                        _metrics.RecordTimer("http_request_duration", duration, new Dictionary<string, string>
                        {
                            ["method"] = context.Request.Method,
                            ["path"] = context.Request.Path.Value?.Split('/')[1] ?? "unknown",
                            ["status_code"] = context.Response.StatusCode.ToString()
                        });
                        
                        // Incrementar contador de responses por status code
                        _metrics.IncrementCounter("http_responses_total", new Dictionary<string, string>
                        {
                            ["method"] = context.Request.Method,
                            ["path"] = context.Request.Path.Value?.Split('/')[1] ?? "unknown",
                            ["status_code"] = context.Response.StatusCode.ToString()
                        });
                        
                        if (context.Response.StatusCode >= 400)
                        {
                            _logger.LogWarning("Request completado con error {StatusCode} en {Duration}ms", 
                                context.Response.StatusCode, duration.TotalMilliseconds);
                        }
                        else
                        {
                            _logger.LogInformation("Request completado exitosamente {StatusCode} en {Duration}ms", 
                                context.Response.StatusCode, duration.TotalMilliseconds);
                        }
                    }
                    catch (Exception ex)
                    {
                        var duration = DateTime.UtcNow - startTime;
                        
                        _logger.LogError("Error en request {Method} {Path}: {Error}", 
                            context.Request.Method, context.Request.Path, ex.Message);
                        
                        // Incrementar contador de errores
                        _metrics.IncrementCounter("http_errors_total", new Dictionary<string, string>
                        {
                            ["method"] = context.Request.Method,
                            ["path"] = context.Request.Path.Value?.Split('/')[1] ?? "unknown",
                            ["error_type"] = ex.GetType().Name
                        });
                        
                        throw;
                    }
                }
            }
        }
        
        // Middleware para correlation ID
        public class CorrelationIdMiddleware
        {
            private readonly RequestDelegate _next;
            
            public CorrelationIdMiddleware(RequestDelegate next)
            {
                _next = next;
            }
            
            public async Task InvokeAsync(HttpContext context)
            {
                var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
                
                context.Response.Headers["X-Correlation-ID"] = correlationId;
                context.Items["CorrelationId"] = correlationId;
                
                await _next(context);
            }
        }
    }
    
    // ===== EXTENSIONES =====
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAdvancedLogging(this IServiceCollection services)
        {
            services.AddSingleton<ILoggerService, SerilogService>();
            services.AddSingleton<IMetricsService, InMemoryMetricsService>();
            services.AddSingleton<IHealthCheckService, HealthCheckService>();
            services.AddSingleton<RealTimeMonitoringService>();
            
            return services;
        }
    }
}

// Uso del sistema de logging y monitoreo
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Sistema de Logging y Monitoreo Avanzado ===\n");
        
        // Configurar servicios
        var services = new ServiceCollection();
        services.AddAdvancedLogging();
        
        var serviceProvider = services.BuildServiceProvider();
        
        try
        {
            var logger = serviceProvider.GetRequiredService<ILoggerService>();
            var metrics = serviceProvider.GetRequiredService<IMetricsService>();
            var healthCheck = serviceProvider.GetRequiredService<IHealthCheckService>();
            var monitoring = serviceProvider.GetRequiredService<RealTimeMonitoringService>();
            
            // 1. Configurar correlation ID
            logger.SetCorrelationId("demo-session-001");
            logger.AddProperty("Environment", "Development");
            logger.AddProperty("Component", "MainProgram");
            
            logger.LogInformation("Sistema de logging y monitoreo iniciado");
            
            Console.WriteLine("1. Logging configurado con correlation ID");
            
            // 2. Probar diferentes niveles de logging
            Console.WriteLine("\n2. Probando diferentes niveles de logging:");
            
            logger.LogDebug("Mensaje de debug - Solo visible en desarrollo");
            logger.LogInformation("Mensaje informativo - Operación normal");
            logger.LogWarning("Mensaje de advertencia - Atención requerida");
            
            try
            {
                throw new InvalidOperationException("Error simulado para testing");
            }
            catch (Exception ex)
            {
                logger.LogError("Error capturado: {ErrorMessage}", ex, ex.Message);
            }
            
            // 3. Usar scopes de logging
            Console.WriteLine("\n3. Probando scopes de logging:");
            
            using (logger.BeginScope("Operación de Usuario", new Dictionary<string, object>
            {
                ["UserId"] = "user123",
                ["Action"] = "Login"
            }))
            {
                logger.LogInformation("Usuario iniciando sesión");
                
                using (logger.BeginScope("Validación de Credenciales"))
                {
                    logger.LogDebug("Validando email y contraseña");
                    logger.LogInformation("Credenciales validadas exitosamente");
                }
                
                logger.LogInformation("Sesión iniciada exitosamente");
            }
            
            // 4. Registrar métricas
            Console.WriteLine("\n4. Registrando métricas:");
            
            for (int i = 0; i < 10; i++)
            {
                metrics.IncrementCounter("user_actions", new Dictionary<string, string>
                {
                    ["action_type"] = "login",
                    ["user_type"] = "regular"
                });
                
                var responseTime = Random.Shared.Next(50, 200);
                metrics.RecordHistogram("api_response_time_ms", responseTime, new Dictionary<string, string>
                {
                    ["endpoint"] = "/api/login",
                    ["method"] = "POST"
                });
                
                var memoryUsage = Random.Shared.Next(100, 500);
                metrics.RecordGauge("memory_usage_mb", memoryUsage);
                
                await Task.Delay(100);
            }
            
            // 5. Verificar health checks
            Console.WriteLine("\n5. Verificando health checks:");
            
            var healthStatus = await healthCheck.CheckHealthAsync();
            Console.WriteLine($"Estado general del sistema: {healthStatus}");
            
            var allHealthChecks = healthCheck.GetAllHealthChecks();
            foreach (var check in allHealthChecks)
            {
                Console.WriteLine($"  {check.Key}: {check.Value}");
            }
            
            // 6. Mostrar métricas recolectadas
            Console.WriteLine("\n6. Métricas recolectadas:");
            
            var allMetrics = metrics.GetMetrics();
            foreach (var metricType in allMetrics)
            {
                Console.WriteLine($"\n{metricType.Key}:");
                if (metricType.Value is Dictionary<string, object> metricDict)
                {
                    foreach (var metric in metricDict.Take(5)) // Mostrar solo las primeras 5
                    {
                        Console.WriteLine($"  {metric.Key}: {metric.Value}");
                    }
                }
            }
            
            // 7. Simular operaciones para generar logs
            Console.WriteLine("\n7. Simulando operaciones para generar logs:");
            
            for (int i = 0; i < 5; i++)
            {
                logger.LogInformation("Operación {OperationNumber} completada", i + 1);
                
                if (i % 2 == 0)
                {
                    metrics.IncrementCounter("operations_completed");
                }
                
                await Task.Delay(200);
            }
            
            // 8. Esperar un poco para que el monitoreo en tiempo real genere datos
            Console.WriteLine("\n8. Esperando para monitoreo en tiempo real...");
            await Task.Delay(5000);
            
            // 9. Mostrar métricas finales
            Console.WriteLine("\n9. Métricas finales:");
            var finalMetrics = metrics.GetMetrics();
            
            if (finalMetrics.ContainsKey("Counters"))
            {
                var counters = (Dictionary<string, object>)finalMetrics["Counters"];
                Console.WriteLine($"Total de contadores: {counters.Count}");
            }
            
            if (finalMetrics.ContainsKey("Gauges"))
            {
                var gauges = (Dictionary<string, object>)finalMetrics["Gauges"];
                Console.WriteLine($"Total de gauges: {gauges.Count}");
            }
            
            logger.LogInformation("Demo de logging y monitoreo completado");
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en el programa: {ex.Message}");
        }
        finally
        {
            // Detener monitoreo
            var monitoring = serviceProvider.GetService<AdvancedLogging.Services.RealTimeMonitoringService>();
            monitoring?.StopMonitoring();
            
            // Dispose del service provider
            if (serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Sistema de Logging para API Web
Implementa un sistema de logging completo para una API web con Serilog, incluyendo middleware para requests y correlation IDs.

### Ejercicio 2: Dashboard de Métricas
Crea un dashboard simple que muestre métricas en tiempo real usando el sistema de métricas implementado.

### Ejercicio 3: Sistema de Alertas
Desarrolla un sistema de alertas que notifique cuando las métricas excedan ciertos umbrales.

## 🔍 Puntos Clave

1. **Serilog** proporciona logging estructurado con capacidades avanzadas de filtrado
2. **Correlation IDs** permiten rastrear operaciones a través de diferentes servicios
3. **Métricas** deben ser recolectadas de forma eficiente y con etiquetas apropiadas
4. **Health Checks** verifican el estado de diferentes componentes del sistema
5. **Monitoreo en tiempo real** requiere timers y recolección periódica de datos

## 📚 Recursos Adicionales

- [Serilog - Structured Logging for .NET](https://serilog.net/)
- [Application Insights - Microsoft Docs](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)
- [Health Checks - Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)

---

**🎯 ¡Has completado la Clase 6! Ahora dominas el logging y monitoreo avanzado en C#**

**📚 [Siguiente: Clase 7 - Refactoring y Clean Code](clase_7_refactoring_clean_code.md)**
