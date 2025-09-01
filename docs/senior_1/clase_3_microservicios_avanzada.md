# 🚀 Clase 3: Arquitectura de Microservicios Avanzada

## 📋 Información de la Clase

- **Módulo**: Senior 1 - Arquitectura de Software Empresarial
- **Duración**: 2 horas
- **Nivel**: Senior
- **Prerrequisitos**: Completar Clase 2 (Event-Driven Architecture)

## 🎯 Objetivos de Aprendizaje

- Diseñar arquitecturas de microservicios escalables
- Implementar patrones de comunicación entre servicios
- Aplicar patrones de resiliencia y circuit breakers
- Implementar Service Mesh y API Gateway

---

## 📚 Navegación del Módulo 6

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_arquitectura_limpia_avanzada.md) | Arquitectura Limpia Avanzada | |
| [Clase 2](clase_2_event_driven_architecture.md) | Event-Driven Architecture | ← Anterior |
| **Clase 3** | **Arquitectura de Microservicios Avanzada** | ← Estás aquí |
| [Clase 4](clase_4_patrones_enterprise.md) | Patrones de Diseño Enterprise | Siguiente → |
| [Clase 5](clase_5_arquitectura_datos_avanzada.md) | Arquitectura de Datos Avanzada | |
| [Clase 6](clase_6_calidad_codigo_metricas.md) | Calidad del Código y Métricas | |
| [Clase 7](clase_7_monitoreo_observabilidad.md) | Monitoreo y Observabilidad | |
| [Clase 8](clase_8_arquitectura_evolutiva.md) | Arquitectura Evolutiva | |
| [Clase 9](clase_9_seguridad_enterprise.md) | Arquitectura de Seguridad Enterprise | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Plataforma Empresarial | |

**← [Volver al README del Módulo 6](../senior_1/README.md)**

---

## 📚 Contenido Teórico

### 1. Arquitectura de Microservicios Avanzada

Los microservicios son servicios independientes que se comunican entre sí para formar una aplicación completa.

```csharp
// ===== ARQUITECTURA DE MICROSERVICIOS AVANZADA - IMPLEMENTACIÓN COMPLETA =====
namespace MicroservicesAdvanced
{
    // ===== SERVICE DISCOVERY =====
    namespace Infrastructure.ServiceDiscovery
    {
        public interface IServiceDiscovery
        {
            Task<ServiceInstance> GetServiceInstanceAsync(string serviceName);
            Task<IEnumerable<ServiceInstance>> GetAllServiceInstancesAsync(string serviceName);
            Task RegisterServiceAsync(ServiceInstance serviceInstance);
            Task UnregisterServiceAsync(string serviceId);
        }
        
        public class ConsulServiceDiscovery : IServiceDiscovery
        {
            private readonly IConsulClient _consulClient;
            private readonly ILogger<ConsulServiceDiscovery> _logger;
            
            public ConsulServiceDiscovery(IConsulClient consulClient, ILogger<ConsulServiceDiscovery> logger)
            {
                _consulClient = consulClient;
                _logger = logger;
            }
            
            public async Task<ServiceInstance> GetServiceInstanceAsync(string serviceName)
            {
                try
                {
                    var queryResult = await _consulClient.Health.Service(serviceName, null, true);
                    
                    if (queryResult.Response?.Any() == true)
                    {
                        var healthyService = queryResult.Response.First();
                        return new ServiceInstance
                        {
                            Id = healthyService.Service.ID,
                            Name = healthyService.Service.Service,
                            Address = healthyService.Service.Address,
                            Port = healthyService.Service.Port,
                            Tags = healthyService.Service.Tags?.ToArray() ?? Array.Empty<string>()
                        };
                    }
                    
                    throw new ServiceNotFoundException($"No healthy instances found for service: {serviceName}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error discovering service: {ServiceName}", serviceName);
                    throw;
                }
            }
            
            public async Task<IEnumerable<ServiceInstance>> GetAllServiceInstancesAsync(string serviceName)
            {
                try
                {
                    var queryResult = await _consulClient.Health.Service(serviceName, null, true);
                    
                    return queryResult.Response?.Select(s => new ServiceInstance
                    {
                        Id = s.Service.ID,
                        Name = s.Service.Service,
                        Address = s.Service.Address,
                        Port = s.Service.Port,
                        Tags = s.Service.Tags?.ToArray() ?? Array.Empty<string>()
                    }) ?? Enumerable.Empty<ServiceInstance>();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error discovering all instances for service: {ServiceName}", serviceName);
                    throw;
                }
            }
            
            public async Task RegisterServiceAsync(ServiceInstance serviceInstance)
            {
                try
                {
                    var registration = new AgentServiceRegistration
                    {
                        ID = serviceInstance.Id,
                        Name = serviceInstance.Name,
                        Address = serviceInstance.Address,
                        Port = serviceInstance.Port,
                        Tags = serviceInstance.Tags,
                        Check = new AgentServiceCheck
                        {
                            HTTP = $"http://{serviceInstance.Address}:{serviceInstance.Port}/health",
                            Interval = TimeSpan.FromSeconds(30),
                            Timeout = TimeSpan.FromSeconds(5)
                        }
                    };
                    
                    await _consulClient.Agent.ServiceRegister(registration);
                    
                    _logger.LogInformation("Service {ServiceName} registered with Consul", serviceInstance.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error registering service: {ServiceName}", serviceInstance.Name);
                    throw;
                }
            }
            
            public async Task UnregisterServiceAsync(string serviceId)
            {
                try
                {
                    await _consulClient.Agent.ServiceDeregister(serviceId);
                    
                    _logger.LogInformation("Service {ServiceId} unregistered from Consul", serviceId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error unregistering service: {ServiceId}", serviceId);
                    throw;
                }
            }
        }
        
        public class ServiceInstance
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Address { get; set; }
            public int Port { get; set; }
            public string[] Tags { get; set; }
            public string HealthCheckUrl => $"http://{Address}:{Port}/health";
        }
    }
    
    // ===== API GATEWAY =====
    namespace Infrastructure.Gateway
    {
        public class ApiGateway
        {
            private readonly IHttpClientFactory _httpClientFactory;
            private readonly IServiceDiscovery _serviceDiscovery;
            private readonly ILogger<ApiGateway> _logger;
            private readonly Dictionary<string, string> _routeTable;
            
            public ApiGateway(
                IHttpClientFactory httpClientFactory,
                IServiceDiscovery serviceDiscovery,
                ILogger<ApiGateway> logger)
            {
                _httpClientFactory = httpClientFactory;
                _serviceDiscovery = serviceDiscovery;
                _logger = logger;
                
                _routeTable = new Dictionary<string, string>
                {
                    { "/users", "user-service" },
                    { "/orders", "order-service" },
                    { "/products", "product-service" },
                    { "/payments", "payment-service" }
                };
            }
            
            public async Task<HttpResponseMessage> RouteRequestAsync(HttpRequestMessage request)
            {
                try
                {
                    var serviceName = GetServiceName(request.RequestUri.AbsolutePath);
                    var serviceInstance = await _serviceDiscovery.GetServiceInstanceAsync(serviceName);
                    
                    var targetUri = new Uri($"{serviceInstance.HealthCheckUrl.Replace("/health", "")}{request.RequestUri.PathAndQuery}");
                    request.RequestUri = targetUri;
                    
                    var client = _httpClientFactory.CreateClient();
                    var response = await client.SendAsync(request);
                    
                    _logger.LogInformation("Request routed to {ServiceName} at {TargetUri}", serviceName, targetUri);
                    
                    return response;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error routing request to {Path}", request.RequestUri.AbsolutePath);
                    return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                    {
                        Content = new StringContent("Service temporarily unavailable")
                    };
                }
            }
            
            private string GetServiceName(string path)
            {
                foreach (var route in _routeTable)
                {
                    if (path.StartsWith(route.Key))
                    {
                        return route.Value;
                    }
                }
                
                throw new RouteNotFoundException($"No route found for path: {path}");
            }
        }
    }
    
    // ===== CIRCUIT BREAKER =====
    namespace Infrastructure.Resilience
    {
        public class CircuitBreakerPolicy<T>
        {
            private readonly ILogger<CircuitBreakerPolicy<T>> _logger;
            private readonly int _failureThreshold;
            private readonly TimeSpan _resetTimeout;
            
            private CircuitBreakerState _state = CircuitBreakerState.Closed;
            private int _failureCount = 0;
            private DateTime _lastFailureTime;
            
            public CircuitBreakerPolicy(
                int failureThreshold = 3,
                TimeSpan? resetTimeout = null,
                ILogger<CircuitBreakerPolicy<T>> logger = null)
            {
                _failureThreshold = failureThreshold;
                _resetTimeout = resetTimeout ?? TimeSpan.FromMinutes(1);
                _logger = logger;
            }
            
            public async Task<T> ExecuteAsync(Func<Task<T>> action)
            {
                if (_state == CircuitBreakerState.Open)
                {
                    if (DateTime.UtcNow - _lastFailureTime > _resetTimeout)
                    {
                        _logger?.LogInformation("Circuit breaker timeout reached, transitioning to HalfOpen");
                        _state = CircuitBreakerState.HalfOpen;
                    }
                    else
                    {
                        throw new CircuitBreakerOpenException("Circuit breaker is open");
                    }
                }
                
                try
                {
                    var result = await action();
                    
                    if (_state == CircuitBreakerState.HalfOpen)
                    {
                        _logger?.LogInformation("Circuit breaker transitioning to Closed");
                        _state = CircuitBreakerState.Closed;
                        _failureCount = 0;
                    }
                    
                    return result;
                }
                catch (Exception ex)
                {
                    _failureCount++;
                    _lastFailureTime = DateTime.UtcNow;
                    
                    if (_failureCount >= _failureThreshold)
                    {
                        _logger?.LogWarning("Failure threshold reached, circuit breaker transitioning to Open");
                        _state = CircuitBreakerState.Open;
                    }
                    
                    throw;
                }
            }
            
            public CircuitBreakerState GetState() => _state;
        }
        
        public enum CircuitBreakerState
        {
            Closed,
            Open,
            HalfOpen
        }
        
        public class CircuitBreakerOpenException : Exception
        {
            public CircuitBreakerOpenException(string message) : base(message) { }
        }
        
        public class RetryPolicy<T>
        {
            private readonly int _maxRetries;
            private readonly TimeSpan _delay;
            private readonly ILogger<RetryPolicy<T>> _logger;
            
            public RetryPolicy(
                int maxRetries = 3,
                TimeSpan? delay = null,
                ILogger<RetryPolicy<T>> logger = null)
            {
                _maxRetries = maxRetries;
                _delay = delay ?? TimeSpan.FromSeconds(1);
                _logger = logger;
            }
            
            public async Task<T> ExecuteAsync(Func<Task<T>> action)
            {
                var lastException = default(Exception);
                
                for (int attempt = 1; attempt <= _maxRetries; attempt++)
                {
                    try
                    {
                        return await action();
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                        
                        if (attempt < _maxRetries)
                        {
                            _logger?.LogWarning(ex, "Attempt {Attempt} failed, retrying in {Delay}ms", 
                                attempt, _delay.TotalMilliseconds);
                            
                            await Task.Delay(_delay);
                        }
                    }
                }
                
                throw new MaxRetriesExceededException($"Max retries ({_maxRetries}) exceeded", lastException);
            }
        }
        
        public class MaxRetriesExceededException : Exception
        {
            public MaxRetriesExceededException(string message, Exception innerException) 
                : base(message, innerException) { }
        }
    }
    
    // ===== SERVICE COMMUNICATION =====
    namespace Infrastructure.Communication
    {
        public interface IServiceClient
        {
            Task<TResponse> GetAsync<TResponse>(string serviceName, string endpoint);
            Task<TResponse> PostAsync<TRequest, TResponse>(string serviceName, string endpoint, TRequest request);
            Task<TResponse> PutAsync<TRequest, TResponse>(string serviceName, string endpoint, TRequest request);
            Task DeleteAsync(string serviceName, string endpoint);
        }
        
        public class ResilientServiceClient : IServiceClient
        {
            private readonly IHttpClientFactory _httpClientFactory;
            private readonly IServiceDiscovery _serviceDiscovery;
            private readonly CircuitBreakerPolicy<HttpResponseMessage> _circuitBreaker;
            private readonly RetryPolicy<HttpResponseMessage> _retryPolicy;
            private readonly ILogger<ResilientServiceClient> _logger;
            
            public ResilientServiceClient(
                IHttpClientFactory httpClientFactory,
                IServiceDiscovery serviceDiscovery,
                ILogger<ResilientServiceClient> logger)
            {
                _httpClientFactory = httpClientFactory;
                _serviceDiscovery = serviceDiscovery;
                _logger = logger;
                
                _circuitBreaker = new CircuitBreakerPolicy<HttpResponseMessage>(logger: logger);
                _retryPolicy = new RetryPolicy<HttpResponseMessage>(logger: logger);
            }
            
            public async Task<TResponse> GetAsync<TResponse>(string serviceName, string endpoint)
            {
                var response = await ExecuteWithResilienceAsync(async () =>
                {
                    var serviceInstance = await _serviceDiscovery.GetServiceInstanceAsync(serviceName);
                    var client = _httpClientFactory.CreateClient();
                    var uri = $"{serviceInstance.HealthCheckUrl.Replace("/health", "")}{endpoint}";
                    
                    return await client.GetAsync(uri);
                });
                
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(content);
            }
            
            public async Task<TResponse> PostAsync<TRequest, TResponse>(string serviceName, string endpoint, TRequest request)
            {
                var response = await ExecuteWithResilienceAsync(async () =>
                {
                    var serviceInstance = await _serviceDiscovery.GetServiceInstanceAsync(serviceName);
                    var client = _httpClientFactory.CreateClient();
                    var uri = $"{serviceInstance.HealthCheckUrl.Replace("/health", "")}{endpoint}";
                    
                    var json = JsonSerializer.Serialize(request);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    
                    return await client.PostAsync(uri, content);
                });
                
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(responseContent);
            }
            
            public async Task<TResponse> PutAsync<TRequest, TResponse>(string serviceName, string endpoint, TRequest request)
            {
                var response = await ExecuteWithResilienceAsync(async () =>
                {
                    var serviceInstance = await _serviceDiscovery.GetServiceInstanceAsync(serviceName);
                    var client = _httpClientFactory.CreateClient();
                    var uri = $"{serviceInstance.HealthCheckUrl.Replace("/health", "")}{endpoint}";
                    
                    var json = JsonSerializer.Serialize(request);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    
                    return await client.PutAsync(uri, content);
                });
                
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(responseContent);
            }
            
            public async Task DeleteAsync(string serviceName, string endpoint)
            {
                var response = await ExecuteWithResilienceAsync(async () =>
                {
                    var serviceInstance = await _serviceDiscovery.GetServiceInstanceAsync(serviceName);
                    var client = _httpClientFactory.CreateClient();
                    var uri = $"{serviceInstance.HealthCheckUrl.Replace("/health", "")}{endpoint}";
                    
                    return await client.DeleteAsync(uri);
                });
                
                response.EnsureSuccessStatusCode();
            }
            
            private async Task<HttpResponseMessage> ExecuteWithResilienceAsync(Func<Task<HttpResponseMessage>> action)
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _circuitBreaker.ExecuteAsync(action);
                });
            }
        }
    }
    
    // ===== SERVICE MESH =====
    namespace Infrastructure.ServiceMesh
    {
        public interface IServiceMesh
        {
            Task<TResponse> InvokeServiceAsync<TRequest, TResponse>(string serviceName, string method, TRequest request);
            Task RegisterServiceAsync(string serviceName, string address, int port);
            Task UnregisterServiceAsync(string serviceName);
        }
        
        public class IstioServiceMesh : IServiceMesh
        {
            private readonly ILogger<IstioServiceMesh> _logger;
            private readonly Dictionary<string, ServiceEndpoint> _serviceRegistry;
            
            public IstioServiceMesh(ILogger<IstioServiceMesh> logger)
            {
                _logger = logger;
                _serviceRegistry = new Dictionary<string, ServiceEndpoint>();
            }
            
            public async Task<TResponse> InvokeServiceAsync<TRequest, TResponse>(string serviceName, string method, TRequest request)
            {
                try
                {
                    if (!_serviceRegistry.TryGetValue(serviceName, out var endpoint))
                    {
                        throw new ServiceNotFoundException($"Service {serviceName} not found in mesh");
                    }
                    
                    // Simular invocación a través de Envoy proxy
                    var client = new HttpClient();
                    var uri = $"{endpoint.Address}:{endpoint.Port}/{method}";
                    
                    var json = JsonSerializer.Serialize(request);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    
                    var response = await client.PostAsync(uri, content);
                    response.EnsureSuccessStatusCode();
                    
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<TResponse>(responseContent);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error invoking service {ServiceName} method {Method}", serviceName, method);
                    throw;
                }
            }
            
            public async Task RegisterServiceAsync(string serviceName, string address, int port)
            {
                _serviceRegistry[serviceName] = new ServiceEndpoint
                {
                    Address = address,
                    Port = port
                };
                
                _logger.LogInformation("Service {ServiceName} registered in mesh at {Address}:{Port}", 
                    serviceName, address, port);
            }
            
            public async Task UnregisterServiceAsync(string serviceName)
            {
                if (_serviceRegistry.Remove(serviceName))
                {
                    _logger.LogInformation("Service {ServiceName} unregistered from mesh", serviceName);
                }
            }
        }
        
        public class ServiceEndpoint
        {
            public string Address { get; set; }
            public int Port { get; set; }
        }
    }
    
    // ===== HEALTH CHECKS =====
    namespace Infrastructure.Health
    {
        public interface IHealthCheck
        {
            Task<HealthCheckResult> CheckAsync();
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
                    await _context.Database.CanConnectAsync();
                    
                    return new HealthCheckResult
                    {
                        Status = HealthStatus.Healthy,
                        Description = "Database connection is healthy",
                        Timestamp = DateTime.UtcNow
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Database health check failed");
                    
                    return new HealthCheckResult
                    {
                        Status = HealthStatus.Unhealthy,
                        Description = "Database connection failed",
                        Timestamp = DateTime.UtcNow,
                        Error = ex.Message
                    };
                }
            }
        }
        
        public class ExternalServiceHealthCheck : IHealthCheck
        {
            private readonly IServiceClient _serviceClient;
            private readonly string _serviceName;
            private readonly ILogger<ExternalServiceHealthCheck> _logger;
            
            public ExternalServiceHealthCheck(
                IServiceClient serviceClient,
                string serviceName,
                ILogger<ExternalServiceHealthCheck> logger)
            {
                _serviceClient = serviceClient;
                _serviceName = serviceName;
                _logger = logger;
            }
            
            public async Task<HealthCheckResult> CheckAsync()
            {
                try
                {
                    await _serviceClient.GetAsync<object>(_serviceName, "/health");
                    
                    return new HealthCheckResult
                    {
                        Status = HealthStatus.Healthy,
                        Description = $"External service {_serviceName} is healthy",
                        Timestamp = DateTime.UtcNow
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "External service health check failed for {ServiceName}", _serviceName);
                    
                    return new HealthCheckResult
                    {
                        Status = HealthStatus.Unhealthy,
                        Description = $"External service {_serviceName} is unhealthy",
                        Timestamp = DateTime.UtcNow,
                        Error = ex.Message
                    };
                }
            }
        }
        
        public class HealthCheckResult
        {
            public HealthStatus Status { get; set; }
            public string Description { get; set; }
            public DateTime Timestamp { get; set; }
            public string Error { get; set; }
        }
        
        public enum HealthStatus
        {
            Healthy,
            Degraded,
            Unhealthy
        }
    }
    
    // ===== CONFIGURATION =====
    namespace Infrastructure.Configuration
    {
        public class MicroserviceConfiguration
        {
            public string ServiceName { get; set; }
            public string Version { get; set; }
            public string Environment { get; set; }
            public ServiceDiscoveryConfig ServiceDiscovery { get; set; }
            public ResilienceConfig Resilience { get; set; }
            public MonitoringConfig Monitoring { get; set; }
        }
        
        public class ServiceDiscoveryConfig
        {
            public string Provider { get; set; } = "Consul";
            public string Address { get; set; } = "localhost";
            public int Port { get; set; } = 8500;
        }
        
        public class ResilienceConfig
        {
            public int MaxRetries { get; set; } = 3;
            public int CircuitBreakerThreshold { get; set; } = 5;
            public int CircuitBreakerTimeout { get; set; } = 60;
        }
        
        public class MonitoringConfig
        {
            public bool EnableMetrics { get; set; } = true;
            public bool EnableTracing { get; set; } = true;
            public string MetricsEndpoint { get; set; } = "/metrics";
        }
    }
    
    // ===== DEPENDENCY INJECTION =====
    namespace Infrastructure.DependencyInjection
    {
        public static class ServiceCollectionExtensions
        {
            public static IServiceCollection AddMicroservicesArchitecture(this IServiceCollection services, 
                IConfiguration configuration)
            {
                var config = configuration.GetSection("Microservice").Get<MicroserviceConfiguration>();
                
                // Service Discovery
                services.AddSingleton<IServiceDiscovery, ConsulServiceDiscovery>();
                
                // Service Communication
                services.AddHttpClient();
                services.AddScoped<IServiceClient, ResilientServiceClient>();
                
                // Resilience
                services.AddScoped<CircuitBreakerPolicy<HttpResponseMessage>>();
                services.AddScoped<RetryPolicy<HttpResponseMessage>>();
                
                // Service Mesh
                services.AddScoped<IServiceMesh, IstioServiceMesh>();
                
                // Health Checks
                services.AddScoped<IHealthCheck, DatabaseHealthCheck>();
                services.AddScoped<IHealthCheck, ExternalServiceHealthCheck>();
                
                // Configuration
                services.Configure<MicroserviceConfiguration>(configuration.GetSection("Microservice"));
                
                return services;
            }
        }
    }
}

// Uso de Arquitectura de Microservicios Avanzada
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Arquitectura de Microservicios Avanzada ===\n");
        
        Console.WriteLine("Los componentes implementados incluyen:");
        Console.WriteLine("1. Service Discovery con Consul");
        Console.WriteLine("2. API Gateway para enrutamiento");
        Console.WriteLine("3. Circuit Breaker para resiliencia");
        Console.WriteLine("4. Retry Policy para reintentos");
        Console.WriteLine("5. Service Mesh con Istio");
        Console.WriteLine("6. Health Checks para monitoreo");
        Console.WriteLine("7. Configuración centralizada");
        
        Console.WriteLine("\nBeneficios de esta arquitectura:");
        Console.WriteLine("- Escalabilidad horizontal independiente");
        Console.WriteLine("- Resiliencia y tolerancia a fallos");
        Console.WriteLine("- Desacoplamiento entre servicios");
        Console.WriteLine("- Monitoreo y observabilidad");
        Console.WriteLine("- Despliegue independiente");
        Console.WriteLine("- Tecnologías heterogéneas");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Implementar Service Discovery
Crea un sistema de descubrimiento de servicios personalizado con diferentes proveedores.

### Ejercicio 2: Circuit Breaker Avanzado
Implementa un Circuit Breaker con estados más sofisticados y métricas.

### Ejercicio 3: Service Mesh
Crea un Service Mesh básico con funcionalidades de routing y load balancing.

## 🔍 Puntos Clave

1. **Service Discovery** permite localizar servicios dinámicamente
2. **API Gateway** centraliza el enrutamiento y cross-cutting concerns
3. **Circuit Breaker** previene fallos en cascada
4. **Retry Policy** maneja reintentos de manera inteligente
5. **Service Mesh** proporciona comunicación entre servicios

## 📚 Recursos Adicionales

- [Microservices Architecture](https://microservices.io/)
- [Circuit Breaker Pattern](https://martinfowler.com/bliki/CircuitBreaker.html)
- [Service Mesh](https://istio.io/latest/docs/concepts/what-is-istio/)

---

**🎯 ¡Has completado la Clase 3! Ahora comprendes Arquitectura de Microservicios Avanzada**

**📚 [Siguiente: Clase 4 - Patrones de Diseño Enterprise](clase_4_patrones_enterprise.md)**
