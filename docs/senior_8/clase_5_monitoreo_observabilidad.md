# 📊 Clase 5: Monitoreo y Observabilidad

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 4: Kubernetes Deployment](../senior_8/clase_4_kubernetes_deployment.md)
- **🏠 Inicio del Módulo**: [Módulo 15: Sistemas Avanzados y Distribuidos](../senior_8/README.md)
- **➡️ Siguiente**: [Clase 6: Performance y Escalabilidad](../senior_8/clase_6_performance_escalabilidad.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** métricas con Prometheus
2. **Configurar** logging estructurado con Serilog
3. **Desarrollar** distributed tracing con Jaeger
4. **Aplicar** dashboards de Grafana
5. **Optimizar** alertas y notificaciones

---

## 📈 **Métricas con Prometheus**

### **Configuración de Prometheus**

```yaml
# monitoring/prometheus.yml
global:
  scrape_interval: 15s
  evaluation_interval: 15s

rule_files:
  - "rules/*.yml"

alerting:
  alertmanagers:
    - static_configs:
        - targets:
          - alertmanager:9093

scrape_configs:
  - job_name: 'musical-matching-api'
    static_configs:
      - targets: ['musical-matching-api-service:80']
    metrics_path: '/metrics'
    scrape_interval: 10s
    
  - job_name: 'kubernetes-pods'
    kubernetes_sd_configs:
      - role: pod
    relabel_configs:
      - source_labels: [__meta_kubernetes_pod_annotation_prometheus_io_scrape]
        action: keep
        regex: true
      - source_labels: [__meta_kubernetes_pod_annotation_prometheus_io_path]
        action: replace
        target_label: __metrics_path__
        regex: (.+)
      - source_labels: [__address__, __meta_kubernetes_pod_annotation_prometheus_io_port]
        action: replace
        regex: ([^:]+)(?::\d+)?;(\d+)
        replacement: $1:$2
        target_label: __address__
```

### **Métricas Personalizadas en C#**

```csharp
// MusicalMatching.API/Metrics/MetricsService.cs
using Prometheus;

namespace MusicalMatching.API.Metrics;

public class MetricsService
{
    private readonly Counter _requestsTotal;
    private readonly Histogram _requestDuration;
    private readonly Gauge _activeConnections;
    private readonly Counter _musicianMatchesTotal;
    private readonly Histogram _matchingDuration;

    public MetricsService()
    {
        var factory = Metrics.DefaultFactory;
        
        _requestsTotal = factory.CreateCounter("musical_matching_requests_total", "Total number of requests", "endpoint", "method", "status");
        _requestDuration = factory.CreateHistogram("musical_matching_request_duration_seconds", "Request duration in seconds", "endpoint", "method");
        _activeConnections = factory.CreateGauge("musical_matching_active_connections", "Number of active SignalR connections");
        _musicianMatchesTotal = factory.CreateCounter("musical_matching_matches_total", "Total number of musician matches", "instrument", "location");
        _matchingDuration = factory.CreateHistogram("musical_matching_matching_duration_seconds", "Matching algorithm duration in seconds");
    }

    public void RecordRequest(string endpoint, string method, int statusCode)
    {
        _requestsTotal.Add(1, endpoint, method, statusCode.ToString());
    }

    public void RecordRequestDuration(string endpoint, string method, TimeSpan duration)
    {
        _requestDuration.Record(duration.TotalSeconds, endpoint, method);
    }

    public void SetActiveConnections(int count)
    {
        _activeConnections.Set(count);
    }

    public void RecordMusicianMatch(string instrument, string location)
    {
        _musicianMatchesTotal.Add(1, instrument, location);
    }

    public void RecordMatchingDuration(TimeSpan duration)
    {
        _matchingDuration.Record(duration.TotalSeconds);
    }
}
```

---

## 📝 **Logging Estructurado con Serilog**

### **Configuración de Serilog**

```csharp
// MusicalMatching.API/Configuration/SerilogConfiguration.cs
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace MusicalMatching.API.Configuration;

public static class SerilogConfiguration
{
    public static IHostBuilder ConfigureSerilog(this IHostBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithCorrelationId()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/musical-matching-.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(
                new Uri("http://elasticsearch:9200"))
            {
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                IndexFormat = $"musical-matching-logs-{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLower()}-{DateTime.UtcNow:yyyy-MM}"
            })
            .CreateLogger();

        builder.UseSerilog();
        return builder;
    }
}
```

### **Middleware de Logging**

```csharp
// MusicalMatching.API/Middleware/RequestLoggingMiddleware.cs
using Serilog;

namespace MusicalMatching.API.Middleware;

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
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
        
        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        try
        {
            _logger.Information("Request started {Method} {Path} {CorrelationId}", 
                context.Request.Method, context.Request.Path, correlationId);

            await _next(context);

            var duration = DateTime.UtcNow - startTime;
            _logger.Information("Request completed {Method} {Path} {StatusCode} {Duration}ms {CorrelationId}", 
                context.Request.Method, context.Request.Path, context.Response.StatusCode, 
                duration.TotalMilliseconds, correlationId);
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.Error(ex, "Request failed {Method} {Path} {Duration}ms {CorrelationId}", 
                context.Request.Method, context.Request.Path, duration.TotalMilliseconds, correlationId);
            throw;
        }
    }
}
```

---

## 🔍 **Distributed Tracing con Jaeger**

### **Configuración de OpenTelemetry**

```csharp
// MusicalMatching.API/Configuration/OpenTelemetryConfiguration.cs
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

namespace MusicalMatching.API.Configuration;

public static class OpenTelemetryConfiguration
{
    public static IServiceCollection AddOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSqlClientInstrumentation()
                .AddRedisInstrumentation()
                .AddSource("MusicalMatching.API")
                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(serviceName: "MusicalMatching.API", serviceVersion: "1.0.0")
                    .AddAttributes(new KeyValuePair<string, object>[]
                    {
                        new("environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"),
                        new("deployment.region", Environment.GetEnvironmentVariable("DEPLOYMENT_REGION") ?? "Unknown")
                    }))
                .AddJaegerExporter(options =>
                {
                    options.AgentHost = configuration["Jaeger:Host"] ?? "localhost";
                    options.AgentPort = int.Parse(configuration["Jaeger:Port"] ?? "6831");
                }))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddPrometheusExporter());

        return services;
    }
}
```

### **Tracing en Servicios**

```csharp
// MusicalMatching.Application/Services/MusicianMatchingService.cs
using System.Diagnostics;

namespace MusicalMatching.Application.Services;

public class MusicianMatchingService : IMusicianMatchingService
{
    private readonly ActivitySource _activitySource;
    private readonly ILogger<MusicianMatchingService> _logger;

    public MusicianMatchingService(ILogger<MusicianMatchingService> logger)
    {
        _activitySource = new ActivitySource("MusicalMatching.API");
        _logger = logger;
    }

    public async Task<List<MusicianMatch>> FindMatchesForRequestAsync(MusicianRequest request)
    {
        using var activity = _activitySource.StartActivity("FindMusicianMatches");
        activity?.SetTag("request.id", request.Id);
        activity?.SetTag("request.instrument", request.Instrument);
        activity?.SetTag("request.location", request.Location);

        try
        {
            _logger.LogInformation("Starting musician matching for request {RequestId}", request.Id);
            
            var stopwatch = Stopwatch.StartNew();
            var matches = await PerformMatchingAsync(request);
            stopwatch.Stop();

            activity?.SetTag("matches.count", matches.Count);
            activity?.SetTag("matching.duration_ms", stopwatch.ElapsedMilliseconds);

            _logger.LogInformation("Found {MatchCount} matches for request {RequestId} in {Duration}ms", 
                matches.Count, request.Id, stopwatch.ElapsedMilliseconds);

            return matches;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Error finding matches for request {RequestId}", request.Id);
            throw;
        }
    }

    private async Task<List<MusicianMatch>> PerformMatchingAsync(MusicianRequest request)
    {
        // Implementación del algoritmo de matching
        await Task.Delay(100); // Simulación de procesamiento
        return new List<MusicianMatch>();
    }
}
```

---

## 📊 **Dashboards de Grafana**

### **Dashboard de Métricas de la API**

```json
// monitoring/grafana/dashboards/api-metrics.json
{
  "dashboard": {
    "title": "Musical Matching API Metrics",
    "panels": [
      {
        "title": "Request Rate",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(musical_matching_requests_total[5m])",
            "legendFormat": "{{method}} {{endpoint}}"
          }
        ]
      },
      {
        "title": "Response Time",
        "type": "graph",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, rate(musical_matching_request_duration_seconds_bucket[5m]))",
            "legendFormat": "P95 Response Time"
          }
        ]
      },
      {
        "title": "Active Connections",
        "type": "stat",
        "targets": [
          {
            "expr": "musical_matching_active_connections",
            "legendFormat": "Active SignalR Connections"
          }
        ]
      },
      {
        "title": "Musician Matches",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(musical_matching_matches_total[5m])",
            "legendFormat": "{{instrument}} in {{location}}"
          }
        ]
      }
    ]
  }
}
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Métricas con Prometheus**
```csharp
// Implementa:
// - Métricas personalizadas
// - Exportación a Prometheus
// - Configuración de scraping
// - Reglas de alerta
```

### **Ejercicio 2: Logging Estructurado**
```csharp
// Crea:
// - Configuración de Serilog
// - Middleware de logging
// - Enriquecimiento de logs
// - Integración con Elasticsearch
```

### **Ejercicio 3: Distributed Tracing**
```csharp
// Implementa:
// - Configuración de OpenTelemetry
// - Instrumentación de servicios
// - Exportación a Jaeger
// - Correlación de trazas
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **📈 Métricas con Prometheus**: Monitoreo de métricas personalizadas
2. **📝 Logging Estructurado**: Serilog con enriquecimiento y Elasticsearch
3. **🔍 Distributed Tracing**: OpenTelemetry con Jaeger
4. **📊 Dashboards de Grafana**: Visualización de métricas y logs
5. **🚨 Alertas y Notificaciones**: Monitoreo proactivo del sistema

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Performance y Escalabilidad**, implementando optimizaciones y técnicas de escalado.

---

**¡Has completado la quinta clase del Módulo 15! 📊📝**


