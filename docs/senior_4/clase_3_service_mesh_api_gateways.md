# 🚀 Clase 3: Service Mesh y API Gateways

## 🧭 Navegación
- **⬅️ Anterior**: [Clase 2: Patrones de Comunicación entre Servicios](clase_2_patrones_comunicacion.md)
- **🏠 Inicio del Módulo**: [Módulo 11: Arquitectura de Microservicios Avanzada](README.md)
- **➡️ Siguiente**: [Clase 4: Event Sourcing y CQRS en Microservicios](clase_4_event_sourcing_cqrs.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 📚 Descripción

En esta clase aprenderás sobre Service Mesh e API Gateways, tecnologías fundamentales para la gestión de la comunicación, seguridad y observabilidad en arquitecturas de microservicios.

## 🎯 Objetivos de Aprendizaje

- Comprender el concepto de Service Mesh
- Implementar API Gateways con Ocelot
- Configurar enrutamiento y agregación
- Implementar autenticación y autorización centralizada

## 📖 Contenido Teórico

### ¿Qué es Service Mesh?

Un Service Mesh es una capa de infraestructura que maneja la comunicación entre servicios de manera transparente. Proporciona:

- **Service Discovery**: Encuentra servicios automáticamente
- **Load Balancing**: Distribuye el tráfico entre instancias
- **Security**: TLS, autenticación, autorización
- **Observability**: Métricas, logs, tracing
- **Resilience**: Retry, circuit breaker, timeout

### API Gateway con Ocelot

#### Configuración Básica

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Configurar Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

// Configurar servicios
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["IdentityServer:Authority"];
        options.Audience = builder.Configuration["IdentityServer:Audience"];
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Usar Ocelot
await app.UseOcelot();

app.Run();
```

#### Configuración de Ocelot (ocelot.json)

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/users/{everything}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "user-service",
          "Port": 443
        }
      ],
      "UpstreamPathTemplate": "/users/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer"
      },
      "RateLimitOptions": {
        "ClientWhitelist": [],
        "EnableRateLimiting": true,
        "Period": "1s",
        "PeriodTimespan": 1,
        "Limit": 1
      }
    },
    {
      "DownstreamPathTemplate": "/api/products/{everything}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "product-service",
          "Port": 443
        }
      ],
      "UpstreamPathTemplate": "/products/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer"
      }
    },
    {
      "DownstreamPathTemplate": "/api/orders/{everything}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "order-service",
          "Port": 443
        }
      ],
      "UpstreamPathTemplate": "/orders/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer"
      }
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "https://api-gateway:443"
  }
}
```

#### Middleware Personalizado

```csharp
public class CustomMiddleware : OcelotMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CustomMiddleware> _logger;

    public CustomMiddleware(RequestDelegate next, ILogger<CustomMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
            ?? Guid.NewGuid().ToString();
        
        context.Request.Headers["X-Correlation-ID"] = correlationId;
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        _logger.LogInformation("Processing request {CorrelationId} to {Path}", 
            correlationId, context.Request.Path);

        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation("Request {CorrelationId} completed in {Elapsed}ms with status {StatusCode}", 
                correlationId, stopwatch.ElapsedMilliseconds, context.Response.StatusCode);
        }
    }
}

// Extensión para registrar el middleware
public static class CustomMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CustomMiddleware>();
    }
}
```

#### Agregación de Respuestas

```csharp
public class OrderDetailsAggregator : IDefinedAggregator
{
    public async Task<DownstreamResponse> Aggregate(List<HttpContext> responses)
    {
        var orderResponse = responses.FirstOrDefault(r => r.Request.Path.Value.Contains("/orders"));
        var userResponse = responses.FirstOrDefault(r => r.Request.Path.Value.Contains("/users"));
        var productResponse = responses.FirstOrDefault(r => r.Request.Path.Value.Contains("/products"));

        if (orderResponse == null)
        {
            return new DownstreamResponse(new StringContent("Order not found"), 
                HttpStatusCode.NotFound, new List<KeyValuePair<string, IEnumerable<string>>>());
        }

        var order = await JsonSerializer.DeserializeAsync<OrderDto>(
            await orderResponse.Response.Content.ReadAsStreamAsync());

        var user = userResponse != null 
            ? await JsonSerializer.DeserializeAsync<UserDto>(
                await userResponse.Response.Content.ReadAsStreamAsync())
            : null;

        var products = productResponse != null
            ? await JsonSerializer.DeserializeAsync<List<ProductDto>>(
                await productResponse.Response.Content.ReadAsStreamAsync())
            : new List<ProductDto>();

        var aggregatedResponse = new OrderDetailsDto
        {
            Order = order,
            User = user,
            Products = products
        };

        var json = JsonSerializer.Serialize(aggregatedResponse);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        return new DownstreamResponse(content, HttpStatusCode.OK, 
            new List<KeyValuePair<string, IEnumerable<string>>>());
    }
}
```

### Service Mesh con Envoy Proxy

#### Configuración de Envoy

```yaml
# envoy.yaml
admin:
  access_log_path: /tmp/admin_access.log
  address:
    socket_address:
      address: 0.0.0.0
      port_value: 9901

static_resources:
  listeners:
  - name: listener_0
    address:
      socket_address:
        address: 0.0.0.0
        port_value: 10000
    filter_chains:
    - filters:
      - name: envoy.filters.network.http_connection_manager
        typed_config:
          "@type": type.googleapis.com/envoy.extensions.filters.network.http_connection_manager.v3.HttpConnectionManager
          stat_prefix: ingress_http
          codec_type: AUTO
          route_config:
            name: local_route
            virtual_hosts:
            - name: local_service
              domains: ["*"]
              routes:
              - match:
                  prefix: "/users"
                route:
                  cluster: user_service
                  timeout: 0s
                  retry_policy:
                    retry_on: 5xx,connect-failure,refused-stream
                    num_retries: 3
                    per_try_timeout: 5s
              - match:
                  prefix: "/products"
                route:
                  cluster: product_service
                  timeout: 0s
              - match:
                  prefix: "/orders"
                route:
                  cluster: order_service
                  timeout: 0s
          http_filters:
          - name: envoy.filters.http.router
            typed_config:
              "@type": type.googleapis.com/envoy.extensions.filters.http.router.v3.Router

  clusters:
  - name: user_service
    connect_timeout: 0.25s
    type: STRICT_DNS
    lb_policy: ROUND_ROBIN
    load_assignment:
      cluster_name: user_service
      endpoints:
      - lb_endpoints:
        - endpoint:
            address:
              socket_address:
                address: user-service
                port_value: 80
    health_checks:
    - timeout: 1s
      interval: 10s
      unhealthy_threshold: 3
      healthy_threshold: 2
      http_health_check:
        path: "/health"

  - name: product_service
    connect_timeout: 0.25s
    type: STRICT_DNS
    lb_policy: ROUND_ROBIN
    load_assignment:
      cluster_name: product_service
      endpoints:
      - lb_endpoints:
        - endpoint:
            address:
              socket_address:
                address: product-service
                port_value: 80

  - name: order_service
    connect_timeout: 0.25s
    type: STRICT_DNS
    lb_policy: ROUND_ROBIN
    load_assignment:
      cluster_name: order_service
      endpoints:
      - lb_endpoints:
        - endpoint:
            address:
              socket_address:
                address: order-service
                port_value: 80
```

#### Implementación en .NET

```csharp
public class EnvoyServiceDiscovery
{
    private readonly ILogger<EnvoyServiceDiscovery> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _envoyAdminUrl;

    public EnvoyServiceDiscovery(
        ILogger<EnvoyServiceDiscovery> logger,
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClient = httpClient;
        _envoyAdminUrl = configuration["Envoy:AdminUrl"] ?? "http://localhost:9901";
    }

    public async Task<ServiceHealthInfo> GetServiceHealthAsync(string serviceName)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_envoyAdminUrl}/clusters");
            var content = await response.Content.ReadAsStringAsync();
            
            // Parsear respuesta de Envoy para obtener información del cluster
            var clusterInfo = ParseClusterInfo(content, serviceName);
            
            return new ServiceHealthInfo
            {
                ServiceName = serviceName,
                HealthyInstances = clusterInfo.HealthyInstances,
                TotalInstances = clusterInfo.TotalInstances,
                LastUpdated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting health info for service {ServiceName}", serviceName);
            return new ServiceHealthInfo
            {
                ServiceName = serviceName,
                HealthyInstances = 0,
                TotalInstances = 0,
                LastUpdated = DateTime.UtcNow,
                IsHealthy = false
            };
        }
    }

    private ClusterInfo ParseClusterInfo(string content, string serviceName)
    {
        // Implementar parsing del formato de respuesta de Envoy
        // Este es un ejemplo simplificado
        return new ClusterInfo
        {
            HealthyInstances = 1,
            TotalInstances = 1
        };
    }
}

public class ServiceHealthInfo
{
    public string ServiceName { get; set; }
    public int HealthyInstances { get; set; }
    public int TotalInstances { get; set; }
    public DateTime LastUpdated { get; set; }
    public bool IsHealthy => HealthyInstances > 0;
}

public class ClusterInfo
{
    public int HealthyInstances { get; set; }
    public int TotalInstances { get; set; }
}
```

### API Gateway Avanzado

#### Rate Limiting

```csharp
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitingOptions _options;

    public RateLimitingMiddleware(
        RequestDelegate next,
        IMemoryCache cache,
        ILogger<RateLimitingMiddleware> logger,
        IOptions<RateLimitingOptions> options)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientId(context);
        var endpoint = context.Request.Path.Value;
        
        var key = $"rate_limit:{clientId}:{endpoint}";
        
        if (!await CheckRateLimitAsync(key))
        {
            context.Response.StatusCode = 429;
            context.Response.ContentType = "application/json";
            
            var response = new
            {
                error = "Too Many Requests",
                message = "Rate limit exceeded",
                retryAfter = _options.Window.TotalSeconds
            };
            
            await context.Response.WriteAsJsonAsync(response);
            return;
        }

        await _next(context);
    }

    private string GetClientId(HttpContext context)
    {
        // Obtener ID del cliente desde token JWT, IP, o header personalizado
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
            return userId;

        var clientIp = context.Connection.RemoteIpAddress?.ToString();
        return clientIp ?? "unknown";
    }

    private async Task<bool> CheckRateLimitAsync(string key)
    {
        var currentCount = await _cache.GetOrCreateAsync(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _options.Window;
            return Task.FromResult(0);
        });

        if (currentCount >= _options.MaxRequests)
        {
            return false;
        }

        await _cache.SetAsync(key, currentCount + 1, _options.Window);
        return true;
    }
}

public class RateLimitingOptions
{
    public int MaxRequests { get; set; } = 100;
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);
}

// Extensión para registrar el middleware
public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }
}
```

#### Caching

```csharp
public class CachingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachingMiddleware> _logger;

    public CachingMiddleware(
        RequestDelegate next,
        IDistributedCache cache,
        ILogger<CachingMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method != "GET")
        {
            await _next(context);
            return;
        }

        var cacheKey = GenerateCacheKey(context);
        var cachedResponse = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedResponse))
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(cachedResponse);
            return;
        }

        // Capturar la respuesta
        var originalBodyStream = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await _next(context);

        // Solo cachear respuestas exitosas
        if (context.Response.StatusCode == 200)
        {
            memoryStream.Position = 0;
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();
            
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            
            await _cache.SetStringAsync(cacheKey, responseBody, cacheOptions);
            
            _logger.LogDebug("Response cached for key: {CacheKey}", cacheKey);
        }

        // Restaurar el stream original
        memoryStream.Position = 0;
        await memoryStream.CopyToAsync(originalBodyStream);
    }

    private string GenerateCacheKey(HttpContext context)
    {
        var path = context.Request.Path.Value;
        var query = context.Request.QueryString.Value;
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        
        return $"cache:{userId}:{path}{query}";
    }
}
```

#### Load Balancing

```csharp
public class LoadBalancingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoadBalancingMiddleware> _logger;
    private readonly LoadBalancingOptions _options;
    private readonly IMemoryCache _cache;

    public LoadBalancingMiddleware(
        RequestDelegate next,
        ILogger<LoadBalancingMiddleware> logger,
        IOptions<LoadBalancingOptions> options,
        IMemoryCache cache)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var serviceName = GetServiceName(context.Request.Path.Value);
        if (string.IsNullOrEmpty(serviceName))
        {
            await _next(context);
            return;
        }

        var instance = await GetNextInstanceAsync(serviceName);
        if (instance == null)
        {
            context.Response.StatusCode = 503;
            await context.Response.WriteAsync("Service unavailable");
            return;
        }

        // Agregar header con la instancia seleccionada
        context.Request.Headers["X-Selected-Instance"] = instance;
        
        await _next(context);
    }

    private string GetServiceName(string path)
    {
        if (path?.StartsWith("/users") == true) return "user-service";
        if (path?.StartsWith("/products") == true) return "product-service";
        if (path?.StartsWith("/orders") == true) return "order-service";
        return null;
    }

    private async Task<string> GetNextInstanceAsync(string serviceName)
    {
        var instances = await GetServiceInstancesAsync(serviceName);
        if (!instances.Any())
            return null;

        var strategy = _options.Strategy;
        return strategy switch
        {
            LoadBalancingStrategy.RoundRobin => GetNextRoundRobin(serviceName, instances),
            LoadBalancingStrategy.LeastConnections => GetLeastConnections(instances),
            LoadBalancingStrategy.Random => GetRandomInstance(instances),
            _ => instances.First()
        };
    }

    private async Task<List<string>> GetServiceInstancesAsync(string serviceName)
    {
        var cacheKey = $"instances:{serviceName}";
        
        if (_cache.TryGetValue(cacheKey, out List<string> instances))
        {
            return instances;
        }

        // Obtener instancias del service discovery
        instances = await DiscoverServiceInstancesAsync(serviceName);
        
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
        };
        
        _cache.Set(cacheKey, instances, cacheOptions);
        
        return instances;
    }

    private string GetNextRoundRobin(string serviceName, List<string> instances)
    {
        var cacheKey = $"round_robin:{serviceName}";
        var currentIndex = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return 0;
        });

        var nextIndex = (currentIndex + 1) % instances.Count;
        _cache.Set(cacheKey, nextIndex, TimeSpan.FromMinutes(1));
        
        return instances[currentIndex];
    }

    private string GetLeastConnections(List<string> instances)
    {
        // Implementar lógica de menor número de conexiones
        return instances.First();
    }

    private string GetRandomInstance(List<string> instances)
    {
        var random = new Random();
        return instances[random.Next(instances.Count)];
    }

    private async Task<List<string>> DiscoverServiceInstancesAsync(string serviceName)
    {
        // Implementar discovery real (Consul, Eureka, etc.)
        return new List<string> { $"{serviceName}-1", $"{serviceName}-2" };
    }
}

public class LoadBalancingOptions
{
    public LoadBalancingStrategy Strategy { get; set; } = LoadBalancingStrategy.RoundRobin;
}

public enum LoadBalancingStrategy
{
    RoundRobin,
    LeastConnections,
    Random
}
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: API Gateway con Ocelot
Implementa un API Gateway que incluya:
- Enrutamiento a múltiples servicios
- Autenticación JWT
- Rate limiting
- Agregación de respuestas

### Ejercicio 2: Service Mesh con Envoy
Configura Envoy Proxy para:
- Service discovery
- Load balancing
- Health checks
- Circuit breaking

### Ejercicio 3: Middleware Personalizado
Crea middleware para:
- Logging de requests
- Métricas de rendimiento
- Caching inteligente
- Load balancing personalizado

## 📝 Quiz de Autoevaluación

1. ¿Cuáles son las ventajas de usar un Service Mesh sobre un API Gateway tradicional?
2. ¿Cómo implementarías rate limiting por usuario en un API Gateway?
3. ¿Qué estrategias de load balancing usarías para diferentes tipos de servicios?
4. ¿Cómo manejarías la agregación de respuestas de múltiples servicios?
5. ¿Qué consideraciones tendrías para implementar caching en un API Gateway?

## 🔗 Enlaces Útiles

- [Ocelot - .NET API Gateway](https://github.com/ThreeMammals/Ocelot)
- [Envoy Proxy](https://www.envoyproxy.io/)
- [Service Mesh Architecture](https://istio.io/latest/docs/concepts/what-is-istio/)
- [API Gateway Patterns](https://docs.microsoft.com/en-us/azure/architecture/microservices/design/gateway)

## 🚀 Siguiente Clase

En la siguiente clase aprenderás sobre Event Sourcing y CQRS en microservicios, patrones fundamentales para mantener la consistencia de datos en sistemas distribuidos.

---

**💡 Consejo**: Un Service Mesh puede simplificar significativamente la gestión de microservicios, pero también agrega complejidad. Evalúa si realmente necesitas todas sus funcionalidades antes de implementarlo.
