# 🚀 Clase 7: Monitoreo y Observabilidad

## 📋 Información de la Clase

- **Módulo**: Senior 1 - Arquitectura de Software Empresarial
- **Duración**: 2 horas
- **Nivel**: Senior
- **Prerrequisitos**: Completar Clase 6 (Calidad del Código y Métricas)

## 🎯 Objetivos de Aprendizaje

- Implementar logging estructurado avanzado
- Aplicar distributed tracing
- Implementar métricas de aplicación
- Crear dashboards y alertas

---

## 📚 Navegación del Módulo 6

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_arquitectura_limpia_avanzada.md) | Arquitectura Limpia Avanzada | |
| [Clase 2](clase_2_event_driven_architecture.md) | Event-Driven Architecture | |
| [Clase 3](clase_3_microservicios_avanzada.md) | Arquitectura de Microservicios Avanzada | |
| [Clase 4](clase_4_patrones_enterprise.md) | Patrones de Diseño Enterprise | |
| [Clase 5](clase_5_arquitectura_datos_avanzada.md) | Arquitectura de Datos Avanzada | |
| [Clase 6](clase_6_calidad_codigo_metricas.md) | Calidad del Código y Métricas | ← Anterior |
| **Clase 7** | **Monitoreo y Observabilidad** | ← Estás aquí |
| [Clase 8](clase_8_arquitectura_evolutiva.md) | Arquitectura Evolutiva | Siguiente → |
| [Clase 9](clase_9_seguridad_enterprise.md) | Arquitectura de Seguridad Enterprise | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Plataforma Empresarial | |

**← [Volver al README del Módulo 6](../senior_1/README.md)**

---

## 📚 Contenido Teórico

### 1. Monitoreo y Observabilidad

La observabilidad permite entender el comportamiento interno de un sistema a través de logs, métricas y traces.

```csharp
// ===== MONITOREO Y OBSERVABILIDAD - IMPLEMENTACIÓN COMPLETA =====
namespace MonitoringAndObservability
{
    // ===== LOGGING ESTRUCTURADO =====
    namespace StructuredLogging
    {
        public interface IStructuredLogger
        {
            void LogInformation(string message, params object[] properties);
            void LogWarning(string message, params object[] properties);
            void LogError(string message, Exception exception = null, params object[] properties);
            void LogDebug(string message, params object[] properties);
            void LogTrace(string message, params object[] properties);
            IDisposable BeginScope(string messageFormat, params object[] propertyValues);
        }
        
        public class StructuredLogger : IStructuredLogger
        {
            private readonly ILogger<StructuredLogger> _logger;
            private readonly ICorrelationIdProvider _correlationIdProvider;
            
            public StructuredLogger(ILogger<StructuredLogger> logger, ICorrelationIdProvider correlationIdProvider)
            {
                _logger = logger;
                _correlationIdProvider = correlationIdProvider;
            }
            
            public void LogInformation(string message, params object[] properties)
            {
                var logData = CreateLogData(LogLevel.Information, message, properties);
                _logger.LogInformation(message, logData);
            }
            
            public void LogWarning(string message, params object[] properties)
            {
                var logData = CreateLogData(LogLevel.Warning, message, properties);
                _logger.LogWarning(message, logData);
            }
            
            public void LogError(string message, Exception exception = null, params object[] properties)
            {
                var logData = CreateLogData(LogLevel.Error, message, properties, exception);
                _logger.LogError(exception, message, logData);
            }
            
            public void LogDebug(string message, params object[] properties)
            {
                var logData = CreateLogData(LogLevel.Debug, message, properties);
                _logger.LogDebug(message, logData);
            }
            
            public void LogTrace(string message, params object[] properties)
            {
                var logData = CreateLogData(LogLevel.Trace, message, properties);
                _logger.LogTrace(message, logData);
            }
            
            public IDisposable BeginScope(string messageFormat, params object[] propertyValues)
            {
                var scopeData = CreateLogData(LogLevel.Information, messageFormat, propertyValues);
                return _logger.BeginScope(scopeData);
            }
            
            private object[] CreateLogData(LogLevel level, string message, object[] properties, Exception exception = null)
            {
                var logData = new List<object>
                {
                    new { Level = level.ToString() },
                    new { Timestamp = DateTime.UtcNow },
                    new { CorrelationId = _correlationIdProvider.GetCorrelationId() },
                    new { Message = message }
                };
                
                if (properties != null)
                {
                    logData.AddRange(properties);
                }
                
                if (exception != null)
                {
                    logData.Add(new { Exception = new { exception.Message, exception.StackTrace, exception.Source } });
                }
                
                return logData.ToArray();
            }
        }
        
        public interface ICorrelationIdProvider
        {
            string GetCorrelationId();
            void SetCorrelationId(string correlationId);
        }
        
        public class CorrelationIdProvider : ICorrelationIdProvider
        {
            private readonly AsyncLocal<string> _correlationId = new();
            
            public string GetCorrelationId()
            {
                return _correlationId.Value ?? Guid.NewGuid().ToString();
            }
            
            public void SetCorrelationId(string correlationId)
            {
                _correlationId.Value = correlationId;
            }
        }
        
        public class LoggingMiddleware
        {
            private readonly RequestDelegate _next;
            private readonly IStructuredLogger _logger;
            private readonly ICorrelationIdProvider _correlationIdProvider;
            
            public LoggingMiddleware(RequestDelegate next, IStructuredLogger logger, ICorrelationIdProvider correlationIdProvider)
            {
                _next = next;
                _logger = logger;
                _correlationIdProvider = correlationIdProvider;
            }
            
            public async Task InvokeAsync(HttpContext context)
            {
                var correlationId = Guid.NewGuid().ToString();
                _correlationIdProvider.SetCorrelationId(correlationId);
                
                context.Response.Headers.Add("X-Correlation-ID", correlationId);
                
                var startTime = DateTime.UtcNow;
                
                try
                {
                    _logger.LogInformation("Request started", new { 
                        Method = context.Request.Method, 
                        Path = context.Request.Path, 
                        CorrelationId = correlationId 
                    });
                    
                    await _next(context);
                    
                    var duration = DateTime.UtcNow - startTime;
                    _logger.LogInformation("Request completed", new { 
                        Method = context.Request.Method, 
                        Path = context.Request.Path, 
                        StatusCode = context.Response.StatusCode, 
                        Duration = duration.TotalMilliseconds,
                        CorrelationId = correlationId 
                    });
                }
                catch (Exception ex)
                {
                    var duration = DateTime.UtcNow - startTime;
                    _logger.LogError("Request failed", ex, new { 
                        Method = context.Request.Method, 
                        Path = context.Request.Path, 
                        Duration = duration.TotalMilliseconds,
                        CorrelationId = correlationId 
                    });
                    throw;
                }
            }
        }
    }
    
    // ===== DISTRIBUTED TRACING =====
    namespace DistributedTracing
    {
        public interface ITraceContext
        {
            string TraceId { get; }
            string SpanId { get; }
            string ParentSpanId { get; }
            Dictionary<string, string> Baggage { get; }
            void AddBaggage(string key, string value);
            void SetParentSpanId(string parentSpanId);
        }
        
        public class TraceContext : ITraceContext
        {
            public string TraceId { get; private set; }
            public string SpanId { get; private set; }
            public string ParentSpanId { get; private set; }
            public Dictionary<string, string> Baggage { get; private set; }
            
            public TraceContext()
            {
                TraceId = GenerateTraceId();
                SpanId = GenerateSpanId();
                Baggage = new Dictionary<string, string>();
            }
            
            public void AddBaggage(string key, string value)
            {
                Baggage[key] = value;
            }
            
            public void SetParentSpanId(string parentSpanId)
            {
                ParentSpanId = parentSpanId;
            }
            
            private string GenerateTraceId()
            {
                return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("/", "_").Replace("+", "-").Substring(0, 22);
            }
            
            private string GenerateSpanId()
            {
                return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("/", "_").Replace("+", "-").Substring(0, 16);
            }
        }
        
        public interface ITracer
        {
            ISpan StartSpan(string operationName, ITraceContext context = null);
            void Inject(ITraceContext context, string format, object carrier);
            ITraceContext Extract(string format, object carrier);
        }
        
        public class Tracer : ITracer
        {
            private readonly ILogger<Tracer> _logger;
            
            public Tracer(ILogger<Tracer> logger)
            {
                _logger = logger;
            }
            
            public ISpan StartSpan(string operationName, ITraceContext context = null)
            {
                var traceContext = context ?? new TraceContext();
                
                if (context != null)
                {
                    traceContext.SetParentSpanId(context.SpanId);
                }
                
                _logger.LogInformation("Started span", new { 
                    OperationName = operationName, 
                    TraceId = traceContext.TraceId, 
                    SpanId = traceContext.SpanId,
                    ParentSpanId = traceContext.ParentSpanId 
                });
                
                return new Span(operationName, traceContext, _logger);
            }
            
            public void Inject(ITraceContext context, string format, object carrier)
            {
                if (carrier is HttpRequestMessage httpRequest)
                {
                    httpRequest.Headers.Add("X-Trace-ID", context.TraceId);
                    httpRequest.Headers.Add("X-Span-ID", context.SpanId);
                    httpRequest.Headers.Add("X-Parent-Span-ID", context.ParentSpanId);
                    
                    foreach (var baggage in context.Baggage)
                    {
                        httpRequest.Headers.Add($"X-Baggage-{baggage.Key}", baggage.Value);
                    }
                }
            }
            
            public ITraceContext Extract(string format, object carrier)
            {
                if (carrier is HttpRequestMessage httpRequest)
                {
                    var traceContext = new TraceContext();
                    
                    if (httpRequest.Headers.TryGetValues("X-Trace-ID", out var traceIdValues))
                    {
                        traceContext = new TraceContext();
                        // In a real implementation, you would set the extracted values
                    }
                    
                    return traceContext;
                }
                
                return new TraceContext();
            }
        }
        
        public interface ISpan : IDisposable
        {
            string OperationName { get; }
            ITraceContext Context { get; }
            void AddTag(string key, string value);
            void AddEvent(string eventName, Dictionary<string, string> attributes = null);
            void SetStatus(SpanStatus status, string description = null);
        }
        
        public class Span : ISpan
        {
            public string OperationName { get; }
            public ITraceContext Context { get; }
            private readonly ILogger _logger;
            private readonly DateTime _startTime;
            private readonly List<SpanEvent> _events;
            private readonly Dictionary<string, string> _tags;
            private SpanStatus _status;
            private string _statusDescription;
            
            public Span(string operationName, ITraceContext context, ILogger logger)
            {
                OperationName = operationName;
                Context = context;
                _logger = logger;
                _startTime = DateTime.UtcNow;
                _events = new List<SpanEvent>();
                _tags = new Dictionary<string, string>();
                _status = SpanStatus.Ok;
            }
            
            public void AddTag(string key, string value)
            {
                _tags[key] = value;
            }
            
            public void AddEvent(string eventName, Dictionary<string, string> attributes = null)
            {
                _events.Add(new SpanEvent
                {
                    Name = eventName,
                    Timestamp = DateTime.UtcNow,
                    Attributes = attributes ?? new Dictionary<string, string>()
                });
            }
            
            public void SetStatus(SpanStatus status, string description = null)
            {
                _status = status;
                _statusDescription = description;
            }
            
            public void Dispose()
            {
                var duration = DateTime.UtcNow - _startTime;
                
                _logger.LogInformation("Completed span", new { 
                    OperationName = OperationName, 
                    TraceId = Context.TraceId, 
                    SpanId = Context.SpanId,
                    Duration = duration.TotalMilliseconds,
                    Status = _status.ToString(),
                    StatusDescription = _statusDescription,
                    Tags = _tags,
                    Events = _events
                });
            }
        }
        
        public class SpanEvent
        {
            public string Name { get; set; }
            public DateTime Timestamp { get; set; }
            public Dictionary<string, string> Attributes { get; set; }
        }
        
        public enum SpanStatus
        {
            Ok,
            Error,
            Unset
        }
        
        public class TracingMiddleware
        {
            private readonly RequestDelegate _next;
            private readonly ITracer _tracer;
            
            public TracingMiddleware(RequestDelegate next, ITracer tracer)
            {
                _next = next;
                _tracer = tracer;
            }
            
            public async Task InvokeAsync(HttpContext context)
            {
                var traceContext = _tracer.Extract("http_headers", context.Request);
                using var span = _tracer.StartSpan($"{context.Request.Method} {context.Request.Path}", traceContext);
                
                span.AddTag("http.method", context.Request.Method);
                span.AddTag("http.url", context.Request.Path);
                span.AddTag("http.user_agent", context.Request.Headers["User-Agent"].ToString());
                
                try
                {
                    await _next(context);
                    
                    span.AddTag("http.status_code", context.Response.StatusCode.ToString());
                    span.SetStatus(SpanStatus.Ok);
                }
                catch (Exception ex)
                {
                    span.AddTag("error", "true");
                    span.AddTag("error.message", ex.Message);
                    span.SetStatus(SpanStatus.Error, ex.Message);
                    throw;
                }
            }
        }
    }
    
    // ===== MÉTRICAS DE APLICACIÓN =====
    namespace ApplicationMetrics
    {
        public interface IMetricsCollector
        {
            void IncrementCounter(string name, Dictionary<string, string> labels = null);
            void RecordGauge(string name, double value, Dictionary<string, string> labels = null);
            void RecordHistogram(string name, double value, Dictionary<string, string> labels = null);
            void RecordSummary(string name, double value, Dictionary<string, string> labels = null);
        }
        
        public class MetricsCollector : IMetricsCollector
        {
            private readonly ILogger<MetricsCollector> _logger;
            private readonly Dictionary<string, Counter> _counters;
            private readonly Dictionary<string, Gauge> _gauges;
            private readonly Dictionary<string, Histogram> _histograms;
            private readonly Dictionary<string, Summary> _summaries;
            
            public MetricsCollector(ILogger<MetricsCollector> logger)
            {
                _logger = logger;
                _counters = new Dictionary<string, Counter>();
                _gauges = new Dictionary<string, Gauge>();
                _histograms = new Dictionary<string, Histogram>();
                _summaries = new Dictionary<string, Summary>();
            }
            
            public void IncrementCounter(string name, Dictionary<string, string> labels = null)
            {
                var key = GetMetricKey(name, labels);
                
                if (!_counters.ContainsKey(key))
                {
                    _counters[key] = new Counter { Name = name, Labels = labels ?? new Dictionary<string, string>() };
                }
                
                _counters[key].Value++;
                
                _logger.LogDebug("Incremented counter", new { Name = name, Labels = labels, Value = _counters[key].Value });
            }
            
            public void RecordGauge(string name, double value, Dictionary<string, string> labels = null)
            {
                var key = GetMetricKey(name, labels);
                
                if (!_gauges.ContainsKey(key))
                {
                    _gauges[key] = new Gauge { Name = name, Labels = labels ?? new Dictionary<string, string>() };
                }
                
                _gauges[key].Value = value;
                
                _logger.LogDebug("Recorded gauge", new { Name = name, Labels = labels, Value = value });
            }
            
            public void RecordHistogram(string name, double value, Dictionary<string, string> labels = null)
            {
                var key = GetMetricKey(name, labels);
                
                if (!_histograms.ContainsKey(key))
                {
                    _histograms[key] = new Histogram { Name = name, Labels = labels ?? new Dictionary<string, string>() };
                }
                
                _histograms[key].AddValue(value);
                
                _logger.LogDebug("Recorded histogram", new { Name = name, Labels = labels, Value = value });
            }
            
            public void RecordSummary(string name, double value, Dictionary<string, string> labels = null)
            {
                var key = GetMetricKey(name, labels);
                
                if (!_summaries.ContainsKey(key))
                {
                    _summaries[key] = new Summary { Name = name, Labels = labels ?? new Dictionary<string, string>() };
                }
                
                _summaries[key].AddValue(value);
                
                _logger.LogDebug("Recorded summary", new { Name = name, Labels = labels, Value = value });
            }
            
            private string GetMetricKey(string name, Dictionary<string, string> labels)
            {
                if (labels == null || !labels.Any())
                    return name;
                
                var sortedLabels = labels.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}={kvp.Value}");
                return $"{name}_{string.Join("_", sortedLabels)}";
            }
            
            public MetricsSnapshot GetSnapshot()
            {
                return new MetricsSnapshot
                {
                    Counters = _counters.Values.ToList(),
                    Gauges = _gauges.Values.ToList(),
                    Histograms = _histograms.Values.ToList(),
                    Summaries = _summaries.Values.ToList(),
                    Timestamp = DateTime.UtcNow
                };
            }
        }
        
        public class Counter
        {
            public string Name { get; set; }
            public Dictionary<string, string> Labels { get; set; }
            public long Value { get; set; }
        }
        
        public class Gauge
        {
            public string Name { get; set; }
            public Dictionary<string, string> Labels { get; set; }
            public double Value { get; set; }
        }
        
        public class Histogram
        {
            public string Name { get; set; }
            public Dictionary<string, string> Labels { get; set; }
            public List<double> Values { get; set; } = new List<double>();
            
            public void AddValue(double value)
            {
                Values.Add(value);
            }
            
            public double Min => Values.Any() ? Values.Min() : 0;
            public double Max => Values.Any() ? Values.Max() : 0;
            public double Average => Values.Any() ? Values.Average() : 0;
            public int Count => Values.Count;
        }
        
        public class Summary
        {
            public string Name { get; set; }
            public Dictionary<string, string> Labels { get; set; }
            public List<double> Values { get; set; } = new List<double>();
            
            public void AddValue(double value)
            {
                Values.Add(value);
            }
            
            public double Min => Values.Any() ? Values.Min() : 0;
            public double Max => Values.Any() ? Values.Max() : 0;
            public double Average => Values.Any() ? Values.Average() : 0;
            public int Count => Values.Count;
        }
        
        public class MetricsSnapshot
        {
            public List<Counter> Counters { get; set; }
            public List<Gauge> Gauges { get; set; }
            public List<Histogram> Histograms { get; set; }
            public List<Summary> Summaries { get; set; }
            public DateTime Timestamp { get; set; }
        }
        
        public class MetricsMiddleware
        {
            private readonly RequestDelegate _next;
            private readonly IMetricsCollector _metricsCollector;
            
            public MetricsMiddleware(RequestDelegate next, IMetricsCollector metricsCollector)
            {
                _next = next;
                _metricsCollector = metricsCollector;
            }
            
            public async Task InvokeAsync(HttpContext context)
            {
                var startTime = DateTime.UtcNow;
                
                try
                {
                    await _next(context);
                    
                    var duration = DateTime.UtcNow - startTime;
                    
                    _metricsCollector.IncrementCounter("http_requests_total", new Dictionary<string, string>
                    {
                        ["method"] = context.Request.Method,
                        ["path"] = context.Request.Path,
                        ["status"] = context.Response.StatusCode.ToString()
                    });
                    
                    _metricsCollector.RecordHistogram("http_request_duration_seconds", duration.TotalSeconds, new Dictionary<string, string>
                    {
                        ["method"] = context.Request.Method,
                        ["path"] = context.Request.Path
                    });
                }
                catch (Exception)
                {
                    _metricsCollector.IncrementCounter("http_requests_total", new Dictionary<string, string>
                    {
                        ["method"] = context.Request.Method,
                        ["path"] = context.Request.Path,
                        ["status"] = "500"
                    });
                    throw;
                }
            }
        }
    }
    
    // ===== HEALTH CHECKS AVANZADOS =====
    namespace HealthChecks
    {
        public interface IHealthCheck
        {
            Task<HealthCheckResult> CheckAsync();
        }
        
        public class HealthCheckResult
        {
            public HealthStatus Status { get; set; }
            public string Description { get; set; }
            public Dictionary<string, object> Data { get; set; }
            public List<string> Tags { get; set; }
            public DateTime Timestamp { get; set; }
            
            public static HealthCheckResult Healthy(string description = null, Dictionary<string, object> data = null)
            {
                return new HealthCheckResult
                {
                    Status = HealthStatus.Healthy,
                    Description = description ?? "Healthy",
                    Data = data ?? new Dictionary<string, object>(),
                    Tags = new List<string>(),
                    Timestamp = DateTime.UtcNow
                };
            }
            
            public static HealthCheckResult Unhealthy(string description, Dictionary<string, object> data = null)
            {
                return new HealthCheckResult
                {
                    Status = HealthStatus.Unhealthy,
                    Description = description,
                    Data = data ?? new Dictionary<string, object>(),
                    Tags = new List<string>(),
                    Timestamp = DateTime.UtcNow
                };
            }
            
            public static HealthCheckResult Degraded(string description, Dictionary<string, object> data = null)
            {
                return new HealthCheckResult
                {
                    Status = HealthStatus.Degraded,
                    Description = description,
                    Data = data ?? new Dictionary<string, object>(),
                    Tags = new List<string>(),
                    Timestamp = DateTime.UtcNow
                };
            }
        }
        
        public enum HealthStatus
        {
            Healthy,
            Degraded,
            Unhealthy
        }
        
        public class DatabaseHealthCheck : IHealthCheck
        {
            private readonly ApplicationDbContext _context;
            private readonly ILogger<DatabaseHealthCheck> _logger;
            
            public DatabaseHealthCheck(ApplicationDbContext context, ILogger<DatabaseHealthCheck> logger)
            {
                _context = context;
                _logger = logger;
            }
            
            public async Task<HealthCheckResult> CheckAsync()
            {
                try
                {
                    var stopwatch = Stopwatch.StartNew();
                    await _context.Database.CanConnectAsync();
                    stopwatch.Stop();
                    
                    var data = new Dictionary<string, object>
                    {
                        ["response_time_ms"] = stopwatch.ElapsedMilliseconds,
                        ["database_name"] = _context.Database.GetDbConnection().Database
                    };
                    
                    return HealthCheckResult.Healthy("Database connection is healthy", data);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Database health check failed");
                    
                    var data = new Dictionary<string, object>
                    {
                        ["error"] = ex.Message,
                        ["error_type"] = ex.GetType().Name
                    };
                    
                    return HealthCheckResult.Unhealthy("Database connection failed", data);
                }
            }
        }
        
        public class ExternalServiceHealthCheck : IHealthCheck
        {
            private readonly HttpClient _httpClient;
            private readonly string _serviceUrl;
            private readonly ILogger<ExternalServiceHealthCheck> _logger;
            
            public ExternalServiceHealthCheck(HttpClient httpClient, string serviceUrl, ILogger<ExternalServiceHealthCheck> logger)
            {
                _httpClient = httpClient;
                _serviceUrl = serviceUrl;
                _logger = logger;
            }
            
            public async Task<HealthCheckResult> CheckAsync()
            {
                try
                {
                    var stopwatch = Stopwatch.StartNew();
                    var response = await _httpClient.GetAsync(_serviceUrl);
                    stopwatch.Stop();
                    
                    var data = new Dictionary<string, object>
                    {
                        ["response_time_ms"] = stopwatch.ElapsedMilliseconds,
                        ["status_code"] = (int)response.StatusCode,
                        ["service_url"] = _serviceUrl
                    };
                    
                    if (response.IsSuccessStatusCode)
                    {
                        return HealthCheckResult.Healthy("External service is healthy", data);
                    }
                    else
                    {
                        return HealthCheckResult.Degraded($"External service returned status {response.StatusCode}", data);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "External service health check failed for {ServiceUrl}", _serviceUrl);
                    
                    var data = new Dictionary<string, object>
                    {
                        ["error"] = ex.Message,
                        ["error_type"] = ex.GetType().Name,
                        ["service_url"] = _serviceUrl
                    };
                    
                    return HealthCheckResult.Unhealthy("External service health check failed", data);
                }
            }
        }
        
        public class MemoryHealthCheck : IHealthCheck
        {
            private readonly ILogger<MemoryHealthCheck> _logger;
            private readonly double _warningThreshold;
            private readonly double _criticalThreshold;
            
            public MemoryHealthCheck(ILogger<MemoryHealthCheck> logger, double warningThreshold = 0.8, double criticalThreshold = 0.9)
            {
                _logger = logger;
                _warningThreshold = warningThreshold;
                _criticalThreshold = criticalThreshold;
            }
            
            public Task<HealthCheckResult> CheckAsync()
            {
                var process = Process.GetCurrentProcess();
                var memoryUsage = process.WorkingSet64;
                var memoryLimit = Process.GetCurrentProcess().MaxWorkingSet.ToInt64();
                var memoryPercentage = (double)memoryUsage / memoryLimit;
                
                var data = new Dictionary<string, object>
                {
                    ["memory_usage_bytes"] = memoryUsage,
                    ["memory_limit_bytes"] = memoryLimit,
                    ["memory_percentage"] = memoryPercentage,
                    ["process_id"] = process.Id
                };
                
                if (memoryPercentage >= _criticalThreshold)
                {
                    return Task.FromResult(HealthCheckResult.Unhealthy("Memory usage is critical", data));
                }
                else if (memoryPercentage >= _warningThreshold)
                {
                    return Task.FromResult(HealthCheckResult.Degraded("Memory usage is high", data));
                }
                else
                {
                    return Task.FromResult(HealthCheckResult.Healthy("Memory usage is normal", data));
                }
            }
        }
    }
    
    // ===== ALERTAS Y NOTIFICACIONES =====
    namespace AlertsAndNotifications
    {
        public interface IAlertManager
        {
            Task SendAlertAsync(Alert alert);
            Task<List<Alert>> GetActiveAlertsAsync();
            Task AcknowledgeAlertAsync(string alertId);
            Task ResolveAlertAsync(string alertId);
        }
        
        public class AlertManager : IAlertManager
        {
            private readonly ILogger<AlertManager> _logger;
            private readonly List<Alert> _activeAlerts;
            private readonly IEmailService _emailService;
            private readonly ISlackService _slackService;
            
            public AlertManager(ILogger<AlertManager> logger, IEmailService emailService, ISlackService slackService)
            {
                _logger = logger;
                _emailService = emailService;
                _slackService = slackService;
                _activeAlerts = new List<Alert>();
            }
            
            public async Task SendAlertAsync(Alert alert)
            {
                try
                {
                    _activeAlerts.Add(alert);
                    
                    _logger.LogWarning("Alert triggered", new { 
                        AlertId = alert.Id, 
                        Severity = alert.Severity, 
                        Message = alert.Message 
                    });
                    
                    // Send email alert for high severity
                    if (alert.Severity == AlertSeverity.Critical || alert.Severity == AlertSeverity.High)
                    {
                        await _emailService.SendAlertEmailAsync(alert);
                    }
                    
                    // Send Slack notification
                    await _slackService.SendAlertAsync(alert);
                    
                    _logger.LogInformation("Alert sent successfully", new { AlertId = alert.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send alert", new { AlertId = alert.Id });
                }
            }
            
            public Task<List<Alert>> GetActiveAlertsAsync()
            {
                return Task.FromResult(_activeAlerts.Where(a => a.Status == AlertStatus.Active).ToList());
            }
            
            public async Task AcknowledgeAlertAsync(string alertId)
            {
                var alert = _activeAlerts.FirstOrDefault(a => a.Id == alertId);
                if (alert != null)
                {
                    alert.Status = AlertStatus.Acknowledged;
                    alert.AcknowledgedAt = DateTime.UtcNow;
                    
                    _logger.LogInformation("Alert acknowledged", new { AlertId = alertId });
                }
            }
            
            public async Task ResolveAlertAsync(string alertId)
            {
                var alert = _activeAlerts.FirstOrDefault(a => a.Id == alertId);
                if (alert != null)
                {
                    alert.Status = AlertStatus.Resolved;
                    alert.ResolvedAt = DateTime.UtcNow;
                    
                    _logger.LogInformation("Alert resolved", new { AlertId = alertId });
                }
            }
        }
        
        public class Alert
        {
            public string Id { get; set; } = Guid.NewGuid().ToString();
            public string Title { get; set; }
            public string Message { get; set; }
            public AlertSeverity Severity { get; set; }
            public AlertStatus Status { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? AcknowledgedAt { get; set; }
            public DateTime? ResolvedAt { get; set; }
            public Dictionary<string, object> Metadata { get; set; }
            
            public Alert()
            {
                Status = AlertStatus.Active;
                CreatedAt = DateTime.UtcNow;
                Metadata = new Dictionary<string, object>();
            }
        }
        
        public enum AlertSeverity
        {
            Low,
            Medium,
            High,
            Critical
        }
        
        public enum AlertStatus
        {
            Active,
            Acknowledged,
            Resolved
        }
        
        public interface IEmailService
        {
            Task SendAlertEmailAsync(Alert alert);
        }
        
        public interface ISlackService
        {
            Task SendAlertAsync(Alert alert);
        }
    }
    
    // ===== DEPENDENCY INJECTION =====
    namespace Infrastructure.DependencyInjection
    {
        public static class ServiceCollectionExtensions
        {
            public static IServiceCollection AddMonitoringAndObservability(this IServiceCollection services)
            {
                // Structured Logging
                services.AddScoped<IStructuredLogger, StructuredLogger>();
                services.AddScoped<ICorrelationIdProvider, CorrelationIdProvider>();
                
                // Distributed Tracing
                services.AddScoped<ITracer, Tracer>();
                
                // Application Metrics
                services.AddScoped<IMetricsCollector, MetricsCollector>();
                
                // Health Checks
                services.AddScoped<IHealthCheck, DatabaseHealthCheck>();
                services.AddScoped<IHealthCheck, ExternalServiceHealthCheck>();
                services.AddScoped<IHealthCheck, MemoryHealthCheck>();
                
                // Alerts and Notifications
                services.AddScoped<IAlertManager, AlertManager>();
                
                return services;
            }
        }
    }
}

// Uso de Monitoreo y Observabilidad
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Monitoreo y Observabilidad ===\n");
        
        Console.WriteLine("Los componentes implementados incluyen:");
        Console.WriteLine("1. Logging estructurado con correlación");
        Console.WriteLine("2. Distributed tracing con spans");
        Console.WriteLine("3. Métricas de aplicación (counters, gauges, histograms)");
        Console.WriteLine("4. Health checks avanzados");
        Console.WriteLine("5. Sistema de alertas y notificaciones");
        Console.WriteLine("6. Middleware para monitoreo automático");
        
        Console.WriteLine("\nBeneficios de esta implementación:");
        Console.WriteLine("- Visibilidad completa del sistema");
        Console.WriteLine("- Trazabilidad de requests");
        Console.WriteLine("- Métricas en tiempo real");
        Console.WriteLine("- Detección temprana de problemas");
        Console.WriteLine("- Alertas automáticas");
        Console.WriteLine("- Correlación de eventos");
    }
}
