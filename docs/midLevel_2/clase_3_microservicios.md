# 🚀 Clase 3: Arquitectura de Microservicios

## 📋 Información de la Clase

- **Módulo**: Mid Level 2 - Arquitectura de Software y Patrones Avanzados
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 2 (Event Sourcing y CQRS)

## 🎯 Objetivos de Aprendizaje

- Diseñar arquitectura de microservicios
- Implementar comunicación entre servicios
- Gestionar transacciones distribuidas
- Implementar circuit breakers y resiliencia

---

## 📚 Navegación del Módulo 5

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_arquitectura_hexagonal.md) | Arquitectura Hexagonal (Ports & Adapters) | |
| [Clase 2](clase_2_event_sourcing.md) | Event Sourcing y CQRS Avanzado | ← Anterior |
| **Clase 3** | **Arquitectura de Microservicios** | ← Estás aquí |
| [Clase 4](clase_4_patrones_arquitectonicos.md) | Patrones Arquitectónicos | Siguiente → |
| [Clase 5](clase_5_domain_driven_design.md) | Domain Driven Design (DDD) | |
| [Clase 6](clase_6_async_streams.md) | Async Streams y IAsyncEnumerable | |
| [Clase 7](clase_7_source_generators.md) | Source Generators y Compile-time Code Generation | |
| [Clase 8](clase_8_high_performance.md) | High Performance Programming | |
| [Clase 9](clase_9_seguridad_avanzada.md) | Seguridad Avanzada en .NET | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Gestión Empresarial | |

**← [Volver al README del Módulo 5](../midLevel_2/README.md)**

---

## 📚 Contenido Teórico

### 1. Introducción a Microservicios

Los microservicios son una arquitectura que descompone una aplicación en servicios pequeños, independientes y autónomos.

```csharp
// ===== ARQUITECTURA DE MICROSERVICIOS =====
namespace MicroservicesArchitecture
{
    // ===== SERVICIO DE USUARIOS =====
    namespace UserService
    {
        public class UserController : ControllerBase
        {
            [HttpGet("{id}")]
            public async Task<IActionResult> GetUser(int id)
            {
                var user = await _userService.GetByIdAsync(id);
                return Ok(user);
            }
        }
        
        public class UserService : IUserService
        {
            public async Task<UserDto> GetByIdAsync(int id)
            {
                // Lógica del servicio de usuarios
                return new UserDto { Id = id, Name = "User" };
            }
        }
    }
    
    // ===== SERVICIO DE PRODUCTOS =====
    namespace ProductService
    {
        public class ProductController : ControllerBase
        {
            [HttpGet("{id}")]
            public async Task<IActionResult> GetProduct(int id)
            {
                var product = await _productService.GetByIdAsync(id);
                return Ok(product);
            }
        }
    }
    
    // ===== SERVICIO DE ÓRDENES =====
    namespace OrderService
    {
        public class OrderController : ControllerBase
        {
            [HttpPost]
            public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
            {
                // Validar usuario y productos antes de crear orden
                var user = await _userClient.GetUserAsync(request.UserId);
                var products = await _productClient.GetProductsAsync(request.ProductIds);
                
                var order = await _orderService.CreateAsync(request);
                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
            }
        }
    }
    
    // ===== CLIENTES HTTP =====
    namespace Infrastructure.Clients
    {
        public class UserClient : IUserClient
        {
            private readonly HttpClient _httpClient;
            private readonly ILogger<UserClient> _logger;
            
            public UserClient(HttpClient httpClient, ILogger<UserClient> logger)
            {
                _httpClient = httpClient;
                _logger = logger;
            }
            
            public async Task<UserDto> GetUserAsync(int id)
            {
                try
                {
                    var response = await _httpClient.GetAsync($"/api/users/{id}");
                    response.EnsureSuccessStatusCode();
                    
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<UserDto>(content);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Error calling user service for ID: {UserId}", id);
                    throw new ServiceUnavailableException("User service is unavailable");
                }
            }
        }
        
        public class ProductClient : IProductClient
        {
            private readonly HttpClient _httpClient;
            private readonly ILogger<ProductClient> _logger;
            
            public async Task<IEnumerable<ProductDto>> GetProductsAsync(IEnumerable<int> ids)
            {
                var idList = string.Join(",", ids);
                var response = await _httpClient.GetAsync($"/api/products?ids={idList}");
                
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<ProductDto>>(content);
            }
        }
    }
    
    // ===== CIRCUIT BREAKER =====
    namespace Infrastructure.Resilience
    {
        public class CircuitBreakerPolicy
        {
            private readonly ILogger<CircuitBreakerPolicy> _logger;
            private CircuitBreakerState _state = CircuitBreakerState.Closed;
            private int _failureCount = 0;
            private readonly int _threshold;
            private readonly TimeSpan _timeout;
            private DateTime _lastFailureTime;
            
            public CircuitBreakerPolicy(int threshold, TimeSpan timeout, ILogger<CircuitBreakerPolicy> logger)
            {
                _threshold = threshold;
                _timeout = timeout;
                _logger = logger;
            }
            
            public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
            {
                if (_state == CircuitBreakerState.Open)
                {
                    if (DateTime.UtcNow - _lastFailureTime > _timeout)
                    {
                        _state = CircuitBreakerState.HalfOpen;
                        _logger.LogInformation("Circuit breaker moved to HalfOpen state");
                    }
                    else
                    {
                        throw new CircuitBreakerOpenException("Circuit breaker is open");
                    }
                }
                
                try
                {
                    var result = await action();
                    OnSuccess();
                    return result;
                }
                catch (Exception ex)
                {
                    OnFailure();
                    throw;
                }
            }
            
            private void OnSuccess()
            {
                if (_state == CircuitBreakerState.HalfOpen)
                {
                    _state = CircuitBreakerState.Closed;
                    _failureCount = 0;
                    _logger.LogInformation("Circuit breaker moved to Closed state");
                }
            }
            
            private void OnFailure()
            {
                _failureCount++;
                _lastFailureTime = DateTime.UtcNow;
                
                if (_failureCount >= _threshold)
                {
                    _state = CircuitBreakerState.Open;
                    _logger.LogWarning("Circuit breaker moved to Open state after {FailureCount} failures", _failureCount);
                }
            }
        }
        
        public enum CircuitBreakerState
        {
            Closed,
            Open,
            HalfOpen
        }
    }
    
    // ===== SERVICE DISCOVERY =====
    namespace Infrastructure.Discovery
    {
        public interface IServiceDiscovery
        {
            Task<string> GetServiceUrlAsync(string serviceName);
            Task<IEnumerable<string>> GetServiceUrlsAsync(string serviceName);
        }
        
        public class ConsulServiceDiscovery : IServiceDiscovery
        {
            private readonly IConsulClient _consulClient;
            private readonly ILogger<ConsulServiceDiscovery> _logger;
            
            public async Task<string> GetServiceUrlAsync(string serviceName)
            {
                var services = await _consulClient.Catalog.Service(serviceName);
                var service = services.Response.FirstOrDefault();
                
                if (service == null)
                    throw new ServiceNotFoundException($"Service {serviceName} not found");
                
                return $"http://{service.ServiceAddress}:{service.ServicePort}";
            }
            
            public async Task<IEnumerable<string>> GetServiceUrlsAsync(string serviceName)
            {
                var services = await _consulClient.Catalog.Service(serviceName);
                return services.Response.Select(s => $"http://{s.ServiceAddress}:{s.ServicePort}");
            }
        }
    }
    
    // ===== API GATEWAY =====
    namespace Infrastructure.Gateway
    {
        public class ApiGateway
        {
            private readonly IServiceDiscovery _serviceDiscovery;
            private readonly HttpClient _httpClient;
            
            public async Task<IActionResult> RouteRequest(string service, string path, HttpContext context)
            {
                var serviceUrl = await _serviceDiscovery.GetServiceUrlAsync(service);
                var targetUrl = $"{serviceUrl}{path}";
                
                var request = new HttpRequestMessage
                {
                    Method = new HttpMethod(context.Request.Method),
                    RequestUri = new Uri(targetUrl),
                    Content = new StreamContent(context.Request.Body)
                };
                
                var response = await _httpClient.SendAsync(request);
                return new HttpResponseMessageResult(response);
            }
        }
    }
    
    // ===== MESSAGE QUEUE =====
    namespace Infrastructure.Messaging
    {
        public interface IMessageBus
        {
            Task PublishAsync<T>(T message) where T : class;
            Task SubscribeAsync<T>(Func<T, Task> handler) where T : class;
        }
        
        public class RabbitMQMessageBus : IMessageBus
        {
            private readonly IConnection _connection;
            private readonly IModel _channel;
            
            public async Task PublishAsync<T>(T message) where T : class
            {
                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);
                
                _channel.BasicPublish(
                    exchange: "microservices",
                    routingKey: typeof(T).Name.ToLower(),
                    basicProperties: null,
                    body: body);
                
                await Task.CompletedTask;
            }
            
            public async Task SubscribeAsync<T>(Func<T, Task> handler) where T : class
            {
                var queueName = $"queue_{typeof(T).Name.ToLower()}";
                _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
                
                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<T>(json);
                    
                    await handler(message);
                    _channel.BasicAck(ea.DeliveryTag, false);
                };
                
                _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
                await Task.CompletedTask;
            }
        }
    }
    
    // ===== DISTRIBUTED TRANSACTIONS =====
    namespace Infrastructure.Transactions
    {
        public class SagaOrchestrator
        {
            private readonly IMessageBus _messageBus;
            private readonly ILogger<SagaOrchestrator> _logger;
            
            public async Task ExecuteOrderSagaAsync(CreateOrderRequest request)
            {
                try
                {
                    // Paso 1: Reservar inventario
                    await _messageBus.PublishAsync(new ReserveInventoryCommand
                    {
                        ProductIds = request.ProductIds,
                        Quantities = request.Quantities
                    });
                    
                    // Paso 2: Validar usuario
                    await _messageBus.PublishAsync(new ValidateUserCommand
                    {
                        UserId = request.UserId
                    });
                    
                    // Paso 3: Crear orden
                    await _messageBus.PublishAsync(new CreateOrderCommand
                    {
                        UserId = request.UserId,
                        ProductIds = request.ProductIds,
                        Quantities = request.Quantities
                    });
                    
                    _logger.LogInformation("Order saga completed successfully for user {UserId}", request.UserId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Order saga failed for user {UserId}", request.UserId);
                    await CompensateOrderSagaAsync(request);
                }
            }
            
            private async Task CompensateOrderSagaAsync(CreateOrderRequest request)
            {
                // Lógica de compensación
                await _messageBus.PublishAsync(new ReleaseInventoryCommand
                {
                    ProductIds = request.ProductIds,
                    Quantities = request.Quantities
                });
            }
        }
    }
    
    // ===== HEALTH CHECKS =====
    namespace Infrastructure.Health
    {
        public class HealthCheckService : IHealthCheck
        {
            public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
            {
                try
                {
                    // Verificar dependencias
                    var dbHealth = await CheckDatabaseHealthAsync();
                    var externalServiceHealth = await CheckExternalServiceHealthAsync();
                    
                    if (dbHealth && externalServiceHealth)
                    {
                        return HealthCheckResult.Healthy("All dependencies are healthy");
                    }
                    
                    return HealthCheckResult.Unhealthy("Some dependencies are unhealthy");
                }
                catch (Exception ex)
                {
                    return HealthCheckResult.Unhealthy("Health check failed", ex);
                }
            }
            
            private async Task<bool> CheckDatabaseHealthAsync()
            {
                // Implementar verificación de base de datos
                return true;
            }
            
            private async Task<bool> CheckExternalServiceHealthAsync()
            {
                // Implementar verificación de servicios externos
                return true;
            }
        }
    }
}

// ===== CONFIGURACIÓN =====
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMicroservices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar HttpClient para servicios
        services.AddHttpClient("UserService", client =>
        {
            client.BaseAddress = new Uri(configuration["Services:UserService"]);
        });
        
        services.AddHttpClient("ProductService", client =>
        {
            client.BaseAddress = new Uri(configuration["Services:ProductService"]);
        });
        
        // Registrar clientes
        services.AddScoped<IUserClient, UserClient>();
        services.AddScoped<IProductClient, ProductClient>();
        
        // Registrar circuit breaker
        services.AddSingleton<CircuitBreakerPolicy>();
        
        // Registrar service discovery
        services.AddSingleton<IServiceDiscovery, ConsulServiceDiscovery>();
        
        // Registrar message bus
        services.AddSingleton<IMessageBus, RabbitMQMessageBus>();
        
        // Registrar health checks
        services.AddHealthChecks()
            .AddCheck<HealthCheckService>("microservice_health");
        
        return services;
    }
}

// Uso de Microservicios
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Arquitectura de Microservicios ===\n");
        
        Console.WriteLine("Los Microservicios proporcionan:");
        Console.WriteLine("1. Desacoplamiento entre servicios");
        Console.WriteLine("2. Escalabilidad independiente");
        Console.WriteLine("3. Tecnologías heterogéneas");
        Console.WriteLine("4. Despliegue independiente");
        Console.WriteLine("5. Fallos aislados");
        
        Console.WriteLine("\nPatrones implementados:");
        Console.WriteLine("- Circuit Breaker para resiliencia");
        Console.WriteLine("- Service Discovery para localización");
        Console.WriteLine("- API Gateway para enrutamiento");
        Console.WriteLine("- Message Queue para comunicación asíncrona");
        Console.WriteLine("- Saga para transacciones distribuidas");
    }
}

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Implementación de Microservicios
Implementa un sistema de microservicios con al menos 3 servicios (usuarios, productos, órdenes).

### Ejercicio 2: Circuit Breaker
Implementa un circuit breaker personalizado con diferentes estrategias de fallback.

### Ejercicio 3: Saga Pattern
Implementa una saga para el proceso de creación de órdenes con compensación.

## 🔍 Puntos Clave

1. **Desacoplamiento** entre servicios independientes
2. **Comunicación** a través de HTTP, mensajes o eventos
3. **Resiliencia** con circuit breakers y retry policies
4. **Service Discovery** para localización dinámica
5. **Transacciones distribuidas** con patrones Saga

## 📚 Recursos Adicionales

- [Microservices - Martin Fowler](https://martinfowler.com/articles/microservices.html)
- [Circuit Breaker Pattern](https://martinfowler.com/bliki/CircuitBreaker.html)
- [Saga Pattern](https://microservices.io/patterns/data/saga.html)

---

**🎯 ¡Has completado la Clase 3! Ahora comprendes la Arquitectura de Microservicios en C#**

**📚 [Siguiente: Clase 4 - Patrones Arquitectónicos](clase_4_patrones_arquitectonicos.md)**
