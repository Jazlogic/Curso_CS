# 📊 Clase 10: Sistema de Monitoreo y Logging

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 9: Sistema de Seguridad Avanzada](../senior_9/clase_9_sistema_seguridad_avanzada.md)
- **🏠 Inicio del Módulo**: [Módulo 16: Maestría Total y Liderazgo Técnico](../senior_9/README.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** sistema de logging estructurado
2. **Crear** sistema de monitoreo en tiempo real
3. **Configurar** métricas y alertas
4. **Implementar** trazabilidad distribuida
5. **Configurar** dashboards de observabilidad

---

## 📊 **Sistema de Monitoreo y Logging**

### **Servicio de Logging**

```csharp
// MusicalMatching.Application/Services/ILoggingService.cs
namespace MusicalMatching.Application.Services;

public interface ILoggingService
{
    Task LogAsync(LogLevel level, string message, Exception? exception = null, Dictionary<string, object>? properties = null);
    Task LogInformationAsync(string message, Dictionary<string, object>? properties = null);
    Task LogWarningAsync(string message, Dictionary<string, object>? properties = null);
    Task LogErrorAsync(string message, Exception? exception = null, Dictionary<string, object>? properties = null);
    Task LogCriticalAsync(string message, Exception? exception = null, Dictionary<string, object>? properties = null);
    Task LogUserActionAsync(Guid userId, string action, Dictionary<string, object>? properties = null);
    Task LogBusinessEventAsync(string eventType, Dictionary<string, object>? properties = null);
    Task LogPerformanceAsync(string operation, TimeSpan duration, Dictionary<string, object>? properties = null);
    Task<List<LogEntry>> GetLogsAsync(LogQuery query);
    Task<LogStatistics> GetLogStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<bool> ExportLogsAsync(ExportLogsRequest request);
}

public class LoggingService : ILoggingService
{
    private readonly ILogger<LoggingService> _logger;
    private readonly ILogEntryRepository _logRepository;
    private readonly ILogAggregationService _aggregationService;
    private readonly ILogExportService _exportService;
    private readonly ICorrelationIdService _correlationService;
    private readonly IUserContextService _userContextService;

    public LoggingService(
        ILogger<LoggingService> logger,
        ILogEntryRepository logRepository,
        ILogAggregationService aggregationService,
        ILogExportService exportService,
        ICorrelationIdService correlationService,
        IUserContextService userContextService)
    {
        _logger = logger;
        _logRepository = logRepository;
        _aggregationService = aggregationService;
        _exportService = exportService;
        _correlationService = correlationService;
        _userContextService = userContextService;
    }

    public async Task LogAsync(LogLevel level, string message, Exception? exception = null, Dictionary<string, object>? properties = null)
    {
        var logEntry = new LogEntry
        {
            Id = Guid.NewGuid(),
            Level = level,
            Message = message ?? throw new ArgumentNullException(nameof(message)),
            Exception = exception?.ToString(),
            Properties = properties ?? new Dictionary<string, object>(),
            CorrelationId = _correlationService.GetCurrentCorrelationId(),
            UserId = _userContextService.GetCurrentUserId(),
            Timestamp = DateTime.UtcNow,
            Source = GetCallerSource(),
            ThreadId = Environment.CurrentManagedThreadId,
            MachineName = Environment.MachineName,
            ProcessId = Environment.ProcessId
        };

        // Log estructurado con Serilog
        _logger.Log(level, exception, "{Message} {@Properties}", message, logEntry.Properties);

        // Guardar en base de datos
        await _logRepository.AddAsync(logEntry);

        // Agregar a métricas
        await _aggregationService.AggregateLogAsync(logEntry);
    }

    public async Task LogInformationAsync(string message, Dictionary<string, object>? properties = null)
    {
        await LogAsync(LogLevel.Information, message, null, properties);
    }

    public async Task LogWarningAsync(string message, Dictionary<string, object>? properties = null)
    {
        await LogAsync(LogLevel.Warning, message, null, properties);
    }

    public async Task LogErrorAsync(string message, Exception? exception = null, Dictionary<string, object>? properties = null)
    {
        await LogAsync(LogLevel.Error, message, exception, properties);
    }

    public async Task LogCriticalAsync(string message, Exception? exception = null, Dictionary<string, object>? properties = null)
    {
        await LogAsync(LogLevel.Critical, message, exception, properties);
    }

    public async Task LogUserActionAsync(Guid userId, string action, Dictionary<string, object>? properties = null)
    {
        var userProperties = new Dictionary<string, object>
        {
            ["UserId"] = userId,
            ["Action"] = action,
            ["EventType"] = "UserAction"
        };

        if (properties != null)
        {
            foreach (var prop in properties)
            {
                userProperties[prop.Key] = prop.Value;
            }
        }

        await LogInformationAsync($"User {userId} performed action: {action}", userProperties);
    }

    public async Task LogBusinessEventAsync(string eventType, Dictionary<string, object>? properties = null)
    {
        var businessProperties = new Dictionary<string, object>
        {
            ["EventType"] = eventType,
            ["Category"] = "Business"
        };

        if (properties != null)
        {
            foreach (var prop in properties)
            {
                businessProperties[prop.Key] = prop.Value;
            }
        }

        await LogInformationAsync($"Business event: {eventType}", businessProperties);
    }

    public async Task LogPerformanceAsync(string operation, TimeSpan duration, Dictionary<string, object>? properties = null)
    {
        var performanceProperties = new Dictionary<string, object>
        {
            ["Operation"] = operation,
            ["Duration"] = duration.TotalMilliseconds,
            ["EventType"] = "Performance"
        };

        if (properties != null)
        {
            foreach (var prop in properties)
            {
                performanceProperties[prop.Key] = prop.Value;
            }
        }

        var level = duration.TotalMilliseconds > 1000 ? LogLevel.Warning : LogLevel.Information;
        await LogAsync(level, $"Performance: {operation} took {duration.TotalMilliseconds}ms", null, performanceProperties);
    }

    public async Task<List<LogEntry>> GetLogsAsync(LogQuery query)
    {
        return await _logRepository.GetLogsAsync(query);
    }

    public async Task<LogStatistics> GetLogStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-1);
        var to = toDate ?? DateTime.UtcNow;

        return await _aggregationService.GetLogStatisticsAsync(from, to);
    }

    public async Task<bool> ExportLogsAsync(ExportLogsRequest request)
    {
        var logs = await _logRepository.GetLogsAsync(request.Query);
        return await _exportService.ExportLogsAsync(logs, request.Format, request.FilePath);
    }

    private string GetCallerSource()
    {
        var stackTrace = new StackTrace(true);
        var frame = stackTrace.GetFrame(2); // Skip this method and the calling method
        return frame?.GetMethod()?.DeclaringType?.FullName ?? "Unknown";
    }
}
```

---

## 📈 **Sistema de Monitoreo**

### **Servicio de Monitoreo**

```csharp
// MusicalMatching.Application/Services/IMonitoringService.cs
namespace MusicalMatching.Application.Services;

public interface IMonitoringService
{
    Task<HealthCheckResult> PerformHealthCheckAsync();
    Task<List<HealthCheckResult>> PerformHealthChecksAsync();
    Task<SystemMetrics> GetSystemMetricsAsync();
    Task<ApplicationMetrics> GetApplicationMetricsAsync();
    Task<List<PerformanceMetric>> GetPerformanceMetricsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<ErrorMetric>> GetErrorMetricsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<AvailabilityMetrics> GetAvailabilityMetricsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<Alert>> GetActiveAlertsAsync();
    Task<Alert> CreateAlertAsync(CreateAlertRequest request);
    Task<bool> ResolveAlertAsync(Guid alertId, Guid userId);
    Task<List<Incident>> GetIncidentsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<Incident> CreateIncidentAsync(CreateIncidentRequest request);
    Task<bool> ResolveIncidentAsync(Guid incidentId, Guid userId);
    Task<List<Metric>> GetCustomMetricsAsync(string metricName, DateTime? fromDate = null, DateTime? toDate = null);
    Task<bool> RecordCustomMetricAsync(string metricName, double value, Dictionary<string, string>? tags = null);
}

public class MonitoringService : IMonitoringService
{
    private readonly IHealthCheckService _healthCheckService;
    private readonly ISystemMetricsService _systemMetricsService;
    private readonly IApplicationMetricsService _applicationMetricsService;
    private readonly IPerformanceMetricsService _performanceMetricsService;
    private readonly IErrorMetricsService _errorMetricsService;
    private readonly IAvailabilityMetricsService _availabilityMetricsService;
    private readonly IAlertService _alertService;
    private readonly IIncidentService _incidentService;
    private readonly ICustomMetricsService _customMetricsService;
    private readonly ILogger<MonitoringService> _logger;

    public MonitoringService(
        IHealthCheckService healthCheckService,
        ISystemMetricsService systemMetricsService,
        IApplicationMetricsService applicationMetricsService,
        IPerformanceMetricsService performanceMetricsService,
        IErrorMetricsService errorMetricsService,
        IAvailabilityMetricsService availabilityMetricsService,
        IAlertService alertService,
        IIncidentService incidentService,
        ICustomMetricsService customMetricsService,
        ILogger<MonitoringService> logger)
    {
        _healthCheckService = healthCheckService;
        _systemMetricsService = systemMetricsService;
        _applicationMetricsService = applicationMetricsService;
        _performanceMetricsService = performanceMetricsService;
        _errorMetricsService = errorMetricsService;
        _availabilityMetricsService = availabilityMetricsService;
        _alertService = alertService;
        _incidentService = incidentService;
        _customMetricsService = customMetricsService;
        _logger = logger;
    }

    public async Task<HealthCheckResult> PerformHealthCheckAsync()
    {
        var result = new HealthCheckResult
        {
            Timestamp = DateTime.UtcNow,
            Status = HealthStatus.Healthy,
            Checks = new List<HealthCheck>()
        };

        // Verificar base de datos
        var dbCheck = await _healthCheckService.CheckDatabaseAsync();
        result.Checks.Add(dbCheck);

        // Verificar servicios externos
        var externalServicesCheck = await _healthCheckService.CheckExternalServicesAsync();
        result.Checks.Add(externalServicesCheck);

        // Verificar memoria
        var memoryCheck = await _healthCheckService.CheckMemoryAsync();
        result.Checks.Add(memoryCheck);

        // Verificar disco
        var diskCheck = await _healthCheckService.CheckDiskAsync();
        result.Checks.Add(diskCheck);

        // Determinar estado general
        if (result.Checks.Any(c => c.Status == HealthStatus.Unhealthy))
        {
            result.Status = HealthStatus.Unhealthy;
        }
        else if (result.Checks.Any(c => c.Status == HealthStatus.Degraded))
        {
            result.Status = HealthStatus.Degraded;
        }

        return result;
    }

    public async Task<List<HealthCheckResult>> PerformHealthChecksAsync()
    {
        var results = new List<HealthCheckResult>();

        // Health check general
        results.Add(await PerformHealthCheckAsync());

        // Health checks específicos
        results.Add(await _healthCheckService.CheckApiEndpointsAsync());
        results.Add(await _healthCheckService.CheckBackgroundServicesAsync());
        results.Add(await _healthCheckService.CheckCacheAsync());

        return results;
    }

    public async Task<SystemMetrics> GetSystemMetricsAsync()
    {
        return new SystemMetrics
        {
            Timestamp = DateTime.UtcNow,
            CpuUsage = await _systemMetricsService.GetCpuUsageAsync(),
            MemoryUsage = await _systemMetricsService.GetMemoryUsageAsync(),
            DiskUsage = await _systemMetricsService.GetDiskUsageAsync(),
            NetworkUsage = await _systemMetricsService.GetNetworkUsageAsync(),
            ProcessCount = await _systemMetricsService.GetProcessCountAsync(),
            ThreadCount = await _systemMetricsService.GetThreadCountAsync(),
            GcCollections = await _systemMetricsService.GetGcCollectionsAsync(),
            GcMemory = await _systemMetricsService.GetGcMemoryAsync()
        };
    }

    public async Task<ApplicationMetrics> GetApplicationMetricsAsync()
    {
        return new ApplicationMetrics
        {
            Timestamp = DateTime.UtcNow,
            ActiveUsers = await _applicationMetricsService.GetActiveUsersAsync(),
            RequestCount = await _applicationMetricsService.GetRequestCountAsync(),
            ResponseTime = await _applicationMetricsService.GetAverageResponseTimeAsync(),
            ErrorRate = await _applicationMetricsService.GetErrorRateAsync(),
            Throughput = await _applicationMetricsService.GetThroughputAsync(),
            DatabaseConnections = await _applicationMetricsService.GetDatabaseConnectionsAsync(),
            CacheHitRate = await _applicationMetricsService.GetCacheHitRateAsync(),
            QueueLength = await _applicationMetricsService.GetQueueLengthAsync()
        };
    }

    public async Task<List<PerformanceMetric>> GetPerformanceMetricsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var from = fromDate ?? DateTime.UtcNow.AddHours(-1);
        var to = toDate ?? DateTime.UtcNow;

        return await _performanceMetricsService.GetPerformanceMetricsAsync(from, to);
    }

    public async Task<List<ErrorMetric>> GetErrorMetricsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var from = fromDate ?? DateTime.UtcNow.AddHours(-1);
        var to = toDate ?? DateTime.UtcNow;

        return await _errorMetricsService.GetErrorMetricsAsync(from, to);
    }

    public async Task<AvailabilityMetrics> GetAvailabilityMetricsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-1);
        var to = toDate ?? DateTime.UtcNow;

        return await _availabilityMetricsService.GetAvailabilityMetricsAsync(from, to);
    }

    public async Task<List<Alert>> GetActiveAlertsAsync()
    {
        return await _alertService.GetActiveAlertsAsync();
    }

    public async Task<Alert> CreateAlertAsync(CreateAlertRequest request)
    {
        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            Type = request.Type,
            Severity = request.Severity,
            Title = request.Title,
            Description = request.Description,
            Condition = request.Condition,
            Threshold = request.Threshold,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        await _alertService.CreateAlertAsync(alert);

        _logger.LogInformation("Alert {AlertId} created: {Title}", alert.Id, alert.Title);

        return alert;
    }

    public async Task<bool> ResolveAlertAsync(Guid alertId, Guid userId)
    {
        var alert = await _alertService.GetAlertByIdAsync(alertId);
        if (alert == null)
            throw new NotFoundException("Alert not found");

        alert.Resolve(userId);
        await _alertService.UpdateAlertAsync(alert);

        _logger.LogInformation("Alert {AlertId} resolved by user {UserId}", alertId, userId);

        return true;
    }

    public async Task<List<Incident>> GetIncidentsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-7);
        var to = toDate ?? DateTime.UtcNow;

        return await _incidentService.GetIncidentsAsync(from, to);
    }

    public async Task<Incident> CreateIncidentAsync(CreateIncidentRequest request)
    {
        var incident = new Incident
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Severity = request.Severity,
            Status = IncidentStatus.Open,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        await _incidentService.CreateIncidentAsync(incident);

        _logger.LogWarning("Incident {IncidentId} created: {Title}", incident.Id, incident.Title);

        return incident;
    }

    public async Task<bool> ResolveIncidentAsync(Guid incidentId, Guid userId)
    {
        var incident = await _incidentService.GetIncidentByIdAsync(incidentId);
        if (incident == null)
            throw new NotFoundException("Incident not found");

        incident.Resolve(userId);
        await _incidentService.UpdateIncidentAsync(incident);

        _logger.LogInformation("Incident {IncidentId} resolved by user {UserId}", incidentId, userId);

        return true;
    }

    public async Task<List<Metric>> GetCustomMetricsAsync(string metricName, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var from = fromDate ?? DateTime.UtcNow.AddHours(-1);
        var to = toDate ?? DateTime.UtcNow;

        return await _customMetricsService.GetMetricsAsync(metricName, from, to);
    }

    public async Task<bool> RecordCustomMetricAsync(string metricName, double value, Dictionary<string, string>? tags = null)
    {
        var metric = new Metric
        {
            Id = Guid.NewGuid(),
            Name = metricName,
            Value = value,
            Tags = tags ?? new Dictionary<string, string>(),
            Timestamp = DateTime.UtcNow
        };

        await _customMetricsService.RecordMetricAsync(metric);

        return true;
    }
}
```

---

## 🔍 **Sistema de Trazabilidad Distribuida**

### **Servicio de Trazabilidad**

```csharp
// MusicalMatching.Application/Services/ITracingService.cs
namespace MusicalMatching.Application.Services;

public interface ITracingService
{
    Task<Trace> StartTraceAsync(string operationName, Dictionary<string, string>? tags = null);
    Task<Span> StartSpanAsync(string spanName, Guid? parentSpanId = null, Dictionary<string, string>? tags = null);
    Task EndSpanAsync(Guid spanId, Dictionary<string, string>? tags = null);
    Task EndTraceAsync(Guid traceId, Dictionary<string, string>? tags = null);
    Task<List<Trace>> GetTracesAsync(TraceQuery query);
    Task<Trace> GetTraceByIdAsync(Guid traceId);
    Task<List<Span>> GetSpansByTraceIdAsync(Guid traceId);
    Task<TraceStatistics> GetTraceStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<bool> ExportTracesAsync(ExportTracesRequest request);
}

public class TracingService : ITracingService
{
    private readonly ITraceRepository _traceRepository;
    private readonly ISpanRepository _spanRepository;
    private readonly ICorrelationIdService _correlationService;
    private readonly ILogger<TracingService> _logger;
    private readonly IMemoryCache _cache;

    public TracingService(
        ITraceRepository traceRepository,
        ISpanRepository spanRepository,
        ICorrelationIdService correlationService,
        ILogger<TracingService> logger,
        IMemoryCache cache)
    {
        _traceRepository = traceRepository;
        _spanRepository = spanRepository;
        _correlationService = correlationService;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Trace> StartTraceAsync(string operationName, Dictionary<string, string>? tags = null)
    {
        var trace = new Trace
        {
            Id = Guid.NewGuid(),
            OperationName = operationName ?? throw new ArgumentNullException(nameof(operationName)),
            CorrelationId = _correlationService.GetCurrentCorrelationId(),
            StartTime = DateTime.UtcNow,
            Tags = tags ?? new Dictionary<string, string>(),
            Status = TraceStatus.Started
        };

        await _traceRepository.AddAsync(trace);

        // Cachear para acceso rápido
        _cache.Set($"trace_{trace.Id}", trace, TimeSpan.FromHours(1));

        _logger.LogInformation("Trace {TraceId} started: {OperationName}", trace.Id, operationName);

        return trace;
    }

    public async Task<Span> StartSpanAsync(string spanName, Guid? parentSpanId = null, Dictionary<string, string>? tags = null)
    {
        var span = new Span
        {
            Id = Guid.NewGuid(),
            Name = spanName ?? throw new ArgumentNullException(nameof(spanName)),
            ParentSpanId = parentSpanId,
            StartTime = DateTime.UtcNow,
            Tags = tags ?? new Dictionary<string, string>(),
            Status = SpanStatus.Started
        };

        await _spanRepository.AddAsync(span);

        // Cachear para acceso rápido
        _cache.Set($"span_{span.Id}", span, TimeSpan.FromHours(1));

        _logger.LogDebug("Span {SpanId} started: {SpanName}", span.Id, spanName);

        return span;
    }

    public async Task EndSpanAsync(Guid spanId, Dictionary<string, string>? tags = null)
    {
        var span = await _spanRepository.GetByIdAsync(spanId);
        if (span == null)
            throw new NotFoundException("Span not found");

        span.EndTime = DateTime.UtcNow;
        span.Duration = span.EndTime.Value - span.StartTime;
        span.Status = SpanStatus.Completed;

        if (tags != null)
        {
            foreach (var tag in tags)
            {
                span.Tags[tag.Key] = tag.Value;
            }
        }

        await _spanRepository.UpdateAsync(span);

        _logger.LogDebug("Span {SpanId} ended: {Duration}ms", spanId, span.Duration.TotalMilliseconds);
    }

    public async Task EndTraceAsync(Guid traceId, Dictionary<string, string>? tags = null)
    {
        var trace = await _traceRepository.GetByIdAsync(traceId);
        if (trace == null)
            throw new NotFoundException("Trace not found");

        trace.EndTime = DateTime.UtcNow;
        trace.Duration = trace.EndTime.Value - trace.StartTime;
        trace.Status = TraceStatus.Completed;

        if (tags != null)
        {
            foreach (var tag in tags)
            {
                trace.Tags[tag.Key] = tag.Value;
            }
        }

        await _traceRepository.UpdateAsync(trace);

        _logger.LogInformation("Trace {TraceId} ended: {Duration}ms", traceId, trace.Duration.TotalMilliseconds);
    }

    public async Task<List<Trace>> GetTracesAsync(TraceQuery query)
    {
        return await _traceRepository.GetTracesAsync(query);
    }

    public async Task<Trace> GetTraceByIdAsync(Guid traceId)
    {
        var trace = _cache.Get<Trace>($"trace_{traceId}");
        if (trace != null)
            return trace;

        trace = await _traceRepository.GetByIdAsync(traceId);
        if (trace != null)
        {
            _cache.Set($"trace_{traceId}", trace, TimeSpan.FromHours(1));
        }

        return trace ?? throw new NotFoundException("Trace not found");
    }

    public async Task<List<Span>> GetSpansByTraceIdAsync(Guid traceId)
    {
        return await _spanRepository.GetSpansByTraceIdAsync(traceId);
    }

    public async Task<TraceStatistics> GetTraceStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var from = fromDate ?? DateTime.UtcNow.AddHours(-1);
        var to = toDate ?? DateTime.UtcNow;

        return await _traceRepository.GetTraceStatisticsAsync(from, to);
    }

    public async Task<bool> ExportTracesAsync(ExportTracesRequest request)
    {
        var traces = await _traceRepository.GetTracesAsync(request.Query);
        return await _traceRepository.ExportTracesAsync(traces, request.Format, request.FilePath);
    }
}
```

---

## 📊 **Entidades del Sistema de Monitoreo**

### **LogEntry, Trace, Span y Métricas**

```csharp
// MusicalMatching.Domain/Entities/LogEntry.cs
namespace MusicalMatching.Domain.Entities;

public class LogEntry : BaseEntity
{
    public LogLevel Level { get; private set; }
    public string Message { get; private set; }
    public string? Exception { get; private set; }
    public Dictionary<string, object> Properties { get; private set; } = new();
    public string? CorrelationId { get; private set; }
    public Guid? UserId { get; private set; }
    public virtual User? User { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string Source { get; private set; }
    public int ThreadId { get; private set; }
    public string MachineName { get; private set; }
    public int ProcessId { get; private set; }

    private LogEntry() { }

    public LogEntry(
        LogLevel level, string message, string? exception = null,
        Dictionary<string, object>? properties = null, string? correlationId = null,
        Guid? userId = null, string? source = null)
    {
        Level = level;
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Exception = exception;
        Properties = properties ?? new Dictionary<string, object>();
        CorrelationId = correlationId;
        UserId = userId;
        Timestamp = DateTime.UtcNow;
        Source = source ?? "Unknown";
        ThreadId = Environment.CurrentManagedThreadId;
        MachineName = Environment.MachineName;
        ProcessId = Environment.ProcessId;
    }

    public void AddProperty(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Property key cannot be empty");

        Properties[key] = value;
    }

    public bool IsError => Level >= LogLevel.Error;
    public bool IsWarning => Level >= LogLevel.Warning;
    public bool IsInformation => Level >= LogLevel.Information;
}

// MusicalMatching.Domain/Entities/Trace.cs
public class Trace : BaseEntity
{
    public string OperationName { get; private set; }
    public string? CorrelationId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public TimeSpan? Duration { get; private set; }
    public Dictionary<string, string> Tags { get; private set; } = new();
    public TraceStatus Status { get; private set; }
    public List<Span> Spans { get; private set; } = new();

    private Trace() { }

    public Trace(string operationName, string? correlationId = null, Dictionary<string, string>? tags = null)
    {
        OperationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
        CorrelationId = correlationId;
        StartTime = DateTime.UtcNow;
        Tags = tags ?? new Dictionary<string, string>();
        Status = TraceStatus.Started;
    }

    public void End(Dictionary<string, string>? tags = null)
    {
        EndTime = DateTime.UtcNow;
        Duration = EndTime.Value - StartTime;
        Status = TraceStatus.Completed;

        if (tags != null)
        {
            foreach (var tag in tags)
            {
                Tags[tag.Key] = tag.Value;
            }
        }
    }

    public void AddSpan(Span span)
    {
        if (span == null)
            throw new ArgumentNullException(nameof(span));

        Spans.Add(span);
    }

    public void AddTag(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Tag key cannot be empty");

        Tags[key] = value;
    }

    public bool IsCompleted => Status == TraceStatus.Completed;
    public bool IsStarted => Status == TraceStatus.Started;
    public bool IsFailed => Status == TraceStatus.Failed;
}

// MusicalMatching.Domain/Entities/Span.cs
public class Span : BaseEntity
{
    public string Name { get; private set; }
    public Guid? ParentSpanId { get; private set; }
    public virtual Span? ParentSpan { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public TimeSpan? Duration { get; private set; }
    public Dictionary<string, string> Tags { get; private set; } = new();
    public SpanStatus Status { get; private set; }
    public List<Span> ChildSpans { get; private set; } = new();

    private Span() { }

    public Span(string name, Guid? parentSpanId = null, Dictionary<string, string>? tags = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        ParentSpanId = parentSpanId;
        StartTime = DateTime.UtcNow;
        Tags = tags ?? new Dictionary<string, string>();
        Status = SpanStatus.Started;
    }

    public void End(Dictionary<string, string>? tags = null)
    {
        EndTime = DateTime.UtcNow;
        Duration = EndTime.Value - StartTime;
        Status = SpanStatus.Completed;

        if (tags != null)
        {
            foreach (var tag in tags)
            {
                Tags[tag.Key] = tag.Value;
            }
        }
    }

    public void Fail(Dictionary<string, string>? tags = null)
    {
        EndTime = DateTime.UtcNow;
        Duration = EndTime.Value - StartTime;
        Status = SpanStatus.Failed;

        if (tags != null)
        {
            foreach (var tag in tags)
            {
                Tags[tag.Key] = tag.Value;
            }
        }
    }

    public void AddChildSpan(Span childSpan)
    {
        if (childSpan == null)
            throw new ArgumentNullException(nameof(childSpan));

        ChildSpans.Add(childSpan);
    }

    public void AddTag(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Tag key cannot be empty");

        Tags[key] = value;
    }

    public bool IsCompleted => Status == SpanStatus.Completed;
    public bool IsStarted => Status == SpanStatus.Started;
    public bool IsFailed => Status == SpanStatus.Failed;
}

// MusicalMatching.Domain/Entities/Incident.cs
public class Incident : BaseEntity
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public IncidentSeverity Severity { get; private set; }
    public IncidentStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public virtual User Creator { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public Guid? ResolvedBy { get; private set; }
    public virtual User? Resolver { get; private set; }
    public string? ResolutionNotes { get; private set; }
    public List<IncidentUpdate> Updates { get; private set; } = new();

    private Incident() { }

    public Incident(string title, string description, IncidentSeverity severity, Guid createdBy)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Severity = severity;
        Status = IncidentStatus.Open;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
    }

    public void Resolve(Guid resolvedBy, string? notes = null)
    {
        if (Status != IncidentStatus.Open)
            throw new DomainException("Only open incidents can be resolved");

        Status = IncidentStatus.Resolved;
        ResolvedAt = DateTime.UtcNow;
        ResolvedBy = resolvedBy;
        ResolutionNotes = notes;
    }

    public void Close(Guid closedBy, string? notes = null)
    {
        if (Status != IncidentStatus.Resolved)
            throw new DomainException("Only resolved incidents can be closed");

        Status = IncidentStatus.Closed;
        ResolutionNotes = notes;
    }

    public void AddUpdate(string update, Guid updatedBy)
    {
        var incidentUpdate = new IncidentUpdate
        {
            Id = Guid.NewGuid(),
            IncidentId = Id,
            Update = update,
            UpdatedBy = updatedBy,
            UpdatedAt = DateTime.UtcNow
        };

        Updates.Add(incidentUpdate);
    }

    public bool IsOpen => Status == IncidentStatus.Open;
    public bool IsResolved => Status == IncidentStatus.Resolved;
    public bool IsClosed => Status == IncidentStatus.Closed;
}

// MusicalMatching.Domain/Entities/IncidentUpdate.cs
public class IncidentUpdate : BaseEntity
{
    public Guid IncidentId { get; private set; }
    public virtual Incident Incident { get; private set; }
    public string Update { get; private set; }
    public Guid UpdatedBy { get; private set; }
    public virtual User UpdatedByUser { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private IncidentUpdate() { }

    public IncidentUpdate(Guid incidentId, string update, Guid updatedBy)
    {
        IncidentId = incidentId;
        Update = update ?? throw new ArgumentNullException(nameof(update));
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}

// MusicalMatching.Domain/ValueObjects/MonitoringModels.cs
public class HealthCheckResult
{
    public DateTime Timestamp { get; set; }
    public HealthStatus Status { get; set; }
    public List<HealthCheck> Checks { get; set; } = new();
    public string? Message { get; set; }
}

public class HealthCheck
{
    public string Name { get; set; } = string.Empty;
    public HealthStatus Status { get; set; }
    public string? Message { get; set; }
    public TimeSpan Duration { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

public class SystemMetrics
{
    public DateTime Timestamp { get; set; }
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double DiskUsage { get; set; }
    public double NetworkUsage { get; set; }
    public int ProcessCount { get; set; }
    public int ThreadCount { get; set; }
    public int GcCollections { get; set; }
    public long GcMemory { get; set; }
}

public class ApplicationMetrics
{
    public DateTime Timestamp { get; set; }
    public int ActiveUsers { get; set; }
    public int RequestCount { get; set; }
    public double ResponseTime { get; set; }
    public double ErrorRate { get; set; }
    public double Throughput { get; set; }
    public int DatabaseConnections { get; set; }
    public double CacheHitRate { get; set; }
    public int QueueLength { get; set; }
}

public class PerformanceMetric
{
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string> Tags { get; set; } = new();
}

public class ErrorMetric
{
    public string ErrorType { get; set; } = string.Empty;
    public int Count { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string> Tags { get; set; } = new();
}

public class AvailabilityMetrics
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public double Uptime { get; set; }
    public double Downtime { get; set; }
    public int IncidentCount { get; set; }
    public TimeSpan MeanTimeToRecovery { get; set; }
    public TimeSpan MeanTimeBetweenFailures { get; set; }
}

public class LogStatistics
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int TotalLogs { get; set; }
    public int ErrorLogs { get; set; }
    public int WarningLogs { get; set; }
    public int InformationLogs { get; set; }
    public Dictionary<string, int> LogsBySource { get; set; } = new();
    public Dictionary<string, int> LogsByLevel { get; set; } = new();
}

public class TraceStatistics
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int TotalTraces { get; set; }
    public int CompletedTraces { get; set; }
    public int FailedTraces { get; set; }
    public double AverageDuration { get; set; }
    public double P95Duration { get; set; }
    public double P99Duration { get; set; }
    public Dictionary<string, int> TracesByOperation { get; set; } = new();
}

public class Metric
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public Dictionary<string, string> Tags { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

// MusicalMatching.Domain/Enums/MonitoringEnums.cs
public enum HealthStatus
{
    Healthy = 0,
    Degraded = 1,
    Unhealthy = 2
}

public enum TraceStatus
{
    Started = 0,
    Completed = 1,
    Failed = 2
}

public enum SpanStatus
{
    Started = 0,
    Completed = 1,
    Failed = 2
}

public enum IncidentSeverity
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public enum IncidentStatus
{
    Open = 0,
    Resolved = 1,
    Closed = 2
}
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Sistema de Logging**
```csharp
// Implementa:
// - Logging estructurado
// - Agregación de logs
// - Exportación de logs
// - Análisis de logs
```

### **Ejercicio 2: Sistema de Monitoreo**
```csharp
// Crea:
// - Health checks
// - Métricas del sistema
// - Alertas automáticas
// - Dashboards de monitoreo
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **📊 Sistema de Logging**: Logging estructurado y agregación
2. **📈 Sistema de Monitoreo**: Health checks y métricas
3. **🔍 Trazabilidad Distribuida**: Traces y spans
4. **🚨 Gestión de Incidentes**: Alertas y resolución
5. **📊 Observabilidad**: Dashboards y análisis

---

## 🎉 **¡Módulo 16 Completado!**

¡Felicidades! Has completado el **Módulo 16: Maestría Total y Liderazgo Técnico** con la implementación completa de la plataforma **MussikOn**.

### **Lo que has logrado:**

1. **🏗️ Arquitectura de Dominio**: Entidades y agregados del dominio musical
2. **🎵 Sistema de Matching**: Algoritmo de recomendación de músicos
3. **💬 Sistema de Chat**: Comunicación en tiempo real con SignalR
4. **🔔 Sistema de Notificaciones**: Alertas y notificaciones automáticas
5. **💳 Sistema de Pagos**: Procesamiento de transacciones y facturación
6. **⭐ Sistema de Reviews**: Calificaciones y análisis de sentimientos
7. **📊 Sistema de Analytics**: Métricas y reportes de la plataforma
8. **🔒 Sistema de Seguridad**: Autenticación y autorización avanzada
9. **📈 Sistema de Monitoreo**: Observabilidad y trazabilidad completa

### **Habilidades desarrolladas:**

- **Arquitectura de Software**: DDD, CQRS, Event Sourcing
- **Microservicios**: Comunicación asíncrona y resilencia
- **Real-time**: SignalR y WebSockets
- **Seguridad**: Autenticación, autorización y auditoría
- **Observabilidad**: Logging, métricas y trazabilidad
- **Liderazgo Técnico**: Toma de decisiones arquitectónicas

---

**¡Has alcanzado el nivel de Maestría Total en C# y Liderazgo Técnico! 🎯🏆**
