# Clase 6: Distributed Tracing

## Navegación
- [← Clase 5: Monitoreo y Observabilidad](clase_5_monitoreo_observabilidad.md)
- [Clase 7: Testing de Microservicios →](clase_7_testing_microservicios.md)
- [← Volver al README del módulo](README.md)
- [← Volver al módulo anterior (senior_3)](../senior_3/README.md)
- [→ Ir al siguiente módulo (senior_5)](../senior_5/README.md)

## Objetivos de Aprendizaje
- Comprender los conceptos fundamentales del distributed tracing
- Implementar OpenTelemetry en aplicaciones .NET
- Configurar Jaeger para visualización de traces
- Implementar correlation IDs y span propagation
- Crear traces personalizados para operaciones complejas

## Contenido Teórico

### 1. Conceptos Fundamentales del Distributed Tracing

El distributed tracing permite seguir el flujo de una solicitud a través de múltiples servicios:

```csharp
// Conceptos básicos del tracing
public class TracingConcepts
{
    // Trace: Representa una transacción completa
    // Span: Representa una operación individual dentro de un trace
    // Correlation ID: Identificador único que vincula todos los spans
    
    public void ExplainConcepts()
    {
        // 1. Trace ID: Identificador único para toda la transacción
        // 2. Span ID: Identificador único para cada operación
        // 3. Parent Span ID: Referencia al span padre
        // 4. Tags: Metadatos del span (método HTTP, status code, etc.)
        // 5. Baggage: Información que se propaga entre servicios
    }
}
```

### 2. Implementación con OpenTelemetry

Configuración básica de OpenTelemetry:

```csharp
// Configuración de OpenTelemetry
public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                // Agregar instrumentación automática
                builder
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.Filter = (httpContext) =>
                        {
                            // Filtrar endpoints de health checks
                            return !httpContext.Request.Path.StartsWithSegments("/health");
                        };
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.FilterHttpRequestMessage = (request) =>
                        {
                            // Filtrar requests internos
                            return !request.RequestUri.Host.Contains("localhost");
                        };
                    })
                    .AddSqlClientInstrumentation()
                    .AddRedisInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    
                    // Agregar procesadores personalizados
                    .AddProcessor<CustomSpanProcessor>()
                    
                    // Configurar exportador a Jaeger
                    .AddJaegerExporter(options =>
                    {
                        options.AgentHost = configuration["Jaeger:Host"];
                        options.AgentPort = int.Parse(configuration["Jaeger:Port"]);
                        options.ServiceName = configuration["ServiceName"];
                    });
            })
            .WithMetrics(builder =>
            {
                builder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddPrometheusExporter();
            });

        return services;
    }
}

// Procesador personalizado para enriquecer spans
public class CustomSpanProcessor : BaseProcessor<Activity>
{
    public override void OnStart(Activity activity, object parentContext)
    {
        // Agregar tags personalizados
        activity.SetTag("environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
        activity.SetTag("version", GetType().Assembly.GetName().Version?.ToString());
        activity.SetTag("deployment_id", Environment.GetEnvironmentVariable("DEPLOYMENT_ID"));
        
        // Agregar correlation ID si no existe
        if (string.IsNullOrEmpty(activity.TraceId.ToString()))
        {
            activity.SetTraceId(ActivityTraceId.CreateRandom());
        }
    }

    public override void OnEnd(Activity activity)
    {
        // Agregar métricas de duración
        var duration = activity.Duration.TotalMilliseconds;
        if (duration > 1000) // Más de 1 segundo
        {
            activity.SetTag("slow_operation", true);
            activity.SetTag("duration_ms", duration);
        }
    }
}
```

### 3. Implementación de Traces Personalizados

Creación de traces manuales para operaciones complejas:

```csharp
// Servicio con tracing personalizado
public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly ActivitySource _activitySource;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public UserService(
        ILogger<UserService> logger,
        IUserRepository userRepository,
        IEmailService emailService)
    {
        _logger = logger;
        _activitySource = new ActivitySource("UserService");
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        using var activity = _activitySource.StartActivity("CreateUser");
        
        try
        {
            // Agregar tags al span
            activity?.SetTag("user.email", request.Email);
            activity?.SetTag("user.role", request.Role);
            
            // Validar usuario
            using var validationActivity = _activitySource.StartActivity("ValidateUser");
            var validationResult = await ValidateUserAsync(request);
            validationActivity?.SetTag("validation.success", validationResult.IsValid);
            validationActivity?.SetTag("validation.errors", validationResult.Errors.Count);
            
            if (!validationResult.IsValid)
            {
                activity?.SetTag("operation.success", false);
                activity?.SetTag("operation.error", "Validation failed");
                throw new ValidationException(validationResult.Errors);
            }

            // Crear usuario en base de datos
            using var dbActivity = _activitySource.StartActivity("CreateUserInDatabase");
            var user = await _userRepository.CreateAsync(request);
            dbActivity?.SetTag("user.id", user.Id);
            dbActivity?.SetTag("database.operation", "INSERT");
            
            // Enviar email de bienvenida
            using var emailActivity = _activitySource.StartActivity("SendWelcomeEmail");
            await _emailService.SendWelcomeEmailAsync(user.Email);
            emailActivity?.SetTag("email.sent", true);
            
            activity?.SetTag("operation.success", true);
            return user;
        }
        catch (Exception ex)
        {
            activity?.SetTag("operation.success", false);
            activity?.SetTag("operation.error", ex.Message);
            activity?.SetTag("operation.error_type", ex.GetType().Name);
            
            _logger.LogError(ex, "Error creating user");
            throw;
        }
    }

    private async Task<ValidationResult> ValidateUserAsync(CreateUserRequest request)
    {
        using var activity = _activitySource.StartActivity("ValidateUser");
        
        var result = new ValidationResult();
        
        // Validar email
        if (string.IsNullOrEmpty(request.Email) || !IsValidEmail(request.Email))
        {
            result.AddError("Email", "Invalid email format");
        }
        
        // Validar que el email no exista
        if (await _userRepository.ExistsByEmailAsync(request.Email))
        {
            result.AddError("Email", "Email already exists");
        }
        
        // Validar rol
        if (!IsValidRole(request.Role))
        {
            result.AddError("Role", "Invalid role");
        }
        
        return result;
    }
}

// Middleware para propagar correlation IDs
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
            ?? context.Request.Headers["X-Request-ID"].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        // Agregar correlation ID a los headers de respuesta
        context.Response.Headers["X-Correlation-ID"] = correlationId;
        
        // Agregar al contexto del trace
        Activity.Current?.SetTag("correlation_id", correlationId);
        
        // Agregar al contexto del log
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        });

        await _next(context);
    }
}
```

### 4. Propagación de Contexto entre Servicios

Implementación de propagación de contexto en llamadas HTTP:

```csharp
// HttpClient configurado para propagar traces
public class TracingHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ActivitySource _activitySource;

    public TracingHttpClient(HttpClient httpClient, ActivitySource activitySource)
    {
        _httpClient = httpClient;
        _activitySource = activitySource;
    }

    public async Task<HttpResponseMessage> GetAsync(string url)
    {
        using var activity = _activitySource.StartActivity("HTTP_GET");
        
        try
        {
            activity?.SetTag("http.url", url);
            activity?.SetTag("http.method", "GET");
            
            // Crear request con headers de trace
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            
            // Propagar trace context
            var traceParent = $"00-{Activity.Current?.TraceId}-{Activity.Current?.SpanId}-01";
            request.Headers.Add("traceparent", traceParent);
            
            // Propagar correlation ID
            if (Activity.Current?.GetTag("correlation_id") is string correlationId)
            {
                request.Headers.Add("X-Correlation-ID", correlationId);
            }
            
            // Propagar baggage
            foreach (var baggage in Activity.Current?.Baggage ?? Enumerable.Empty<KeyValuePair<string, string>>())
            {
                request.Headers.Add($"baggage-{baggage.Key}", baggage.Value);
            }
            
            var response = await _httpClient.SendAsync(request);
            
            activity?.SetTag("http.status_code", (int)response.StatusCode);
            activity?.SetTag("http.response_size", response.Content.Headers.ContentLength);
            
            return response;
        }
        catch (Exception ex)
        {
            activity?.SetTag("http.error", ex.Message);
            activity?.SetTag("http.error_type", ex.GetType().Name);
            throw;
        }
    }
}

// Delegating handler para tracing automático
public class TracingDelegatingHandler : DelegatingHandler
{
    private readonly ActivitySource _activitySource;

    public TracingDelegatingHandler(ActivitySource activitySource)
    {
        _activitySource = activitySource;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity($"HTTP_{request.Method}");
        
        try
        {
            // Propagar trace context
            PropagateTraceContext(request);
            
            var response = await base.SendAsync(request, cancellationToken);
            
            // Agregar tags de respuesta
            activity?.SetTag("http.status_code", (int)response.StatusCode);
            activity?.SetTag("http.response_size", response.Content.Headers.ContentLength);
            
            return response;
        }
        catch (Exception ex)
        {
            activity?.SetTag("http.error", ex.Message);
            throw;
        }
    }

    private void PropagateTraceContext(HttpRequestMessage request)
    {
        if (Activity.Current != null)
        {
            // Traceparent header (W3C Trace Context)
            var traceParent = $"00-{Activity.Current.TraceId}-{Activity.Current.SpanId}-01";
            request.Headers.Add("traceparent", traceParent);
            
            // Tracestate header para información adicional
            if (Activity.Current.TraceStateString != null)
            {
                request.Headers.Add("tracestate", Activity.Current.TraceStateString);
            }
            
            // Baggage para información de negocio
            foreach (var baggage in Activity.Current.Baggage)
            {
                request.Headers.Add($"baggage-{baggage.Key}", baggage.Value);
            }
        }
    }
}
```

### 5. Visualización y Análisis de Traces

Configuración de Jaeger y análisis de traces:

```csharp
// Configuración de Jaeger
public static class JaegerExtensions
{
    public static IServiceCollection AddJaegerTracing(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddJaegerExporter(options =>
                    {
                        options.AgentHost = configuration["Jaeger:Host"] ?? "localhost";
                        options.AgentPort = int.Parse(configuration["Jaeger:Port"] ?? "6831");
                        options.ServiceName = configuration["ServiceName"] ?? "UnknownService";
                        
                        // Configuración adicional
                        options.ExportProcessorType = ExportProcessorType.Simple;
                        options.MaxPayloadSizeInBytes = 4096;
                    });
            });

        return services;
    }
}

// Servicio para análisis de traces
public interface ITraceAnalyzer
{
    Task<TraceAnalysis> AnalyzeTraceAsync(string traceId);
    Task<List<TraceSummary>> GetSlowTracesAsync(TimeSpan threshold);
    Task<Dictionary<string, int>> GetErrorRatesByEndpointAsync();
}

public class TraceAnalyzer : ITraceAnalyzer
{
    private readonly HttpClient _httpClient;
    private readonly string _jaegerApiUrl;

    public TraceAnalyzer(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _jaegerApiUrl = configuration["Jaeger:ApiUrl"] ?? "http://localhost:16686";
    }

    public async Task<TraceAnalysis> AnalyzeTraceAsync(string traceId)
    {
        var url = $"{_jaegerApiUrl}/api/traces/{traceId}";
        var response = await _httpClient.GetAsync(url);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var traceData = JsonSerializer.Deserialize<TraceData>(content);
            
            return new TraceAnalysis
            {
                TraceId = traceId,
                Duration = CalculateTotalDuration(traceData.Spans),
                SpanCount = traceData.Spans.Count,
                ErrorCount = traceData.Spans.Count(s => s.Tags.Any(t => t.Key == "error")),
                ServiceCount = traceData.Spans.Select(s => s.Process.ServiceName).Distinct().Count(),
                SlowSpans = traceData.Spans.Where(s => s.Duration > 1000000).ToList() // > 1 segundo
            };
        }
        
        throw new Exception($"Failed to retrieve trace: {response.StatusCode}");
    }

    public async Task<List<TraceSummary>> GetSlowTracesAsync(TimeSpan threshold)
    {
        var url = $"{_jaegerApiUrl}/api/traces?service={Uri.EscapeDataString("UserService")}&limit=100";
        var response = await _httpClient.GetAsync(url);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var tracesData = JsonSerializer.Deserialize<TracesData>(content);
            
            return tracesData.Traces
                .Where(t => t.Duration > threshold.TotalMicroseconds)
                .Select(t => new TraceSummary
                {
                    TraceId = t.TraceID,
                    Duration = TimeSpan.FromMicroseconds(t.Duration),
                    StartTime = DateTimeOffset.FromUnixTimeMilliseconds(t.StartTime / 1000),
                    ServiceCount = t.Spans.Select(s => s.Process.ServiceName).Distinct().Count()
                })
                .OrderByDescending(t => t.Duration)
                .ToList();
        }
        
        return new List<TraceSummary>();
    }

    private TimeSpan CalculateTotalDuration(List<Span> spans)
    {
        if (!spans.Any()) return TimeSpan.Zero;
        
        var startTime = spans.Min(s => s.StartTime);
        var endTime = spans.Max(s => s.StartTime + s.Duration);
        
        return TimeSpan.FromMicroseconds(endTime - startTime);
    }
}
```

## Ejercicios Prácticos

### Ejercicio 1: Implementar Tracing Básico
Configura OpenTelemetry en una aplicación ASP.NET Core y visualiza traces en Jaeger.

### Ejercicio 2: Traces Personalizados
Crea traces manuales para operaciones complejas como procesamiento de pagos o generación de reportes.

### Ejercicio 3: Propagación de Contexto
Implementa propagación de correlation IDs y baggage entre múltiples servicios.

## Proyecto Integrador
Implementa un sistema de tracing completo para un e-commerce que incluya:
- Traces automáticos para todas las operaciones HTTP
- Traces personalizados para operaciones de negocio
- Propagación de contexto entre servicios
- Análisis de performance basado en traces
- Dashboard de Jaeger para debugging

## Recursos Adicionales
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)
- [Jaeger Documentation](https://www.jaegertracing.io/docs/)
- [W3C Trace Context](https://www.w3.org/TR/trace-context/)
- [Distributed Tracing Best Practices](https://opentelemetry.io/docs/concepts/best-practices/)
