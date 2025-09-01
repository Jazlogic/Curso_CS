# 🚀 Clase 1: Arquitectura de Microservicios Avanzada

## 🧭 Navegación
- **🏠 Inicio del Módulo**: [Módulo 11: Arquitectura de Microservicios Avanzada](README.md)
- **➡️ Siguiente**: [Clase 2: Patrones de Comunicación entre Servicios](clase_2_patrones_comunicacion.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 📚 Descripción

En esta clase aprenderás los fundamentos avanzados de la arquitectura de microservicios, incluyendo principios de diseño, patrones arquitectónicos y estrategias de implementación para sistemas empresariales escalables.

## 🎯 Objetivos de Aprendizaje

- Comprender los principios fundamentales de microservicios
- Identificar cuándo usar microservicios vs monolitos
- Diseñar arquitecturas de microservicios escalables
- Implementar patrones de resiliencia y tolerancia a fallos

## 📖 Contenido Teórico

### ¿Qué son los Microservicios?

Los microservicios son una arquitectura de software que estructura una aplicación como una colección de servicios pequeños, independientes y autónomos. Cada servicio:

- **Tiene una responsabilidad específica** (Single Responsibility Principle)
- **Puede desarrollarse independientemente** (Autonomía de desarrollo)
- **Puede desplegarse independientemente** (Autonomía de despliegue)
- **Puede escalar independientemente** (Autonomía de escalado)
- **Tiene su propia base de datos** (Independencia de datos)

### Ventajas de los Microservicios

```csharp
public class MicroserviceBenefits
{
    // 1. Escalabilidad Independiente
    public void ScaleIndependently()
    {
        // Solo escalar el servicio que necesita más recursos
        // Ejemplo: Servicio de pagos durante Black Friday
        // vs Servicio de catálogo que no cambia
    }
    
    // 2. Desarrollo Paralelo
    public void ParallelDevelopment()
    {
        // Equipos diferentes pueden trabajar en servicios diferentes
        // sin bloquearse entre sí
    }
    
    // 3. Tecnologías Heterogéneas
    public void HeterogeneousTechnologies()
    {
        // Cada servicio puede usar la tecnología más apropiada
        // User Service: .NET Core + SQL Server
        // Analytics Service: Python + MongoDB
        // Payment Service: Java + PostgreSQL
    }
    
    // 4. Despliegue Independiente
    public void IndependentDeployment()
    {
        // Desplegar solo el servicio que cambió
        // Reducir riesgo y tiempo de downtime
    }
    
    // 5. Fallos Aislados
    public void IsolatedFailures()
    {
        // Si un servicio falla, los otros continúan funcionando
        // Mejor resiliencia del sistema
    }
}
```

### Desventajas y Desafíos

```csharp
public class MicroserviceChallenges
{
    // 1. Complejidad de Distribución
    public void DistributionComplexity()
    {
        // Comunicación entre servicios
        // Latencia de red
        // Gestión de transacciones distribuidas
    }
    
    // 2. Testing Complejo
    public void ComplexTesting()
    {
        // Testing de integración entre servicios
        // Testing de contratos
        // Testing de escenarios de fallo
    }
    
    // 3. Operaciones Distribuidas
    public void DistributedOperations()
    {
        // Monitoreo de múltiples servicios
        // Logging distribuido
        // Tracing de requests
    }
    
    // 4. Consistencia de Datos
    public void DataConsistency()
    {
        // Eventual consistency
        // Saga patterns
        // Distributed transactions
    }
}
```

### Cuándo Usar Microservicios

```csharp
public class WhenToUseMicroservices
{
    public bool ShouldUseMicroservices(ProjectCharacteristics characteristics)
    {
        // Equipo grande (>10 desarrolladores)
        if (characteristics.TeamSize < 10)
            return false;
            
        // Dominio complejo con múltiples contextos
        if (!characteristics.HasComplexDomain)
            return false;
            
        // Necesidad de escalabilidad independiente
        if (!characteristics.NeedsIndependentScaling)
            return false;
            
        // Tecnologías heterogéneas
        if (!characteristics.NeedsHeterogeneousTechnologies)
            return false;
            
        // Despliegues frecuentes
        if (!characteristics.NeedsFrequentDeployments)
            return false;
            
        return true;
    }
}

public class ProjectCharacteristics
{
    public int TeamSize { get; set; }
    public bool HasComplexDomain { get; set; }
    public bool NeedsIndependentScaling { get; set; }
    public bool NeedsHeterogeneousTechnologies { get; set; }
    public bool NeedsFrequentDeployments { get; set; }
}
```

### Patrones Arquitectónicos de Microservicios

#### 1. API Gateway Pattern

```csharp
public class ApiGateway
{
    private readonly IUserServiceClient _userService;
    private readonly IProductServiceClient _productService;
    private readonly IOrderServiceClient _orderService;
    private readonly ILogger<ApiGateway> _logger;

    public ApiGateway(
        IUserServiceClient userService,
        IProductServiceClient productService,
        IOrderServiceClient orderService,
        ILogger<ApiGateway> logger)
    {
        _userService = userService;
        _productService = productService;
        _orderService = orderService;
        _logger = logger;
    }

    // Agregación de datos de múltiples servicios
    public async Task<OrderDetailsDto> GetOrderDetailsAsync(int orderId)
    {
        try
        {
            // Obtener orden
            var order = await _orderService.GetOrderAsync(orderId);
            if (order == null)
                return null;

            // Obtener usuario
            var user = await _userService.GetUserAsync(order.UserId);
            
            // Obtener productos
            var productIds = order.Items.Select(i => i.ProductId);
            var products = await _productService.GetProductsByIdsAsync(productIds);

            // Construir respuesta agregada
            return new OrderDetailsDto
            {
                Order = order,
                User = user,
                Products = products
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order details for order {OrderId}", orderId);
            throw;
        }
    }

    // Routing inteligente
    public async Task<IActionResult> RouteRequestAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower();
        
        if (path?.StartsWith("/api/users") == true)
        {
            return await RouteToUserServiceAsync(context);
        }
        else if (path?.StartsWith("/api/products") == true)
        {
            return await RouteToProductServiceAsync(context);
        }
        else if (path?.StartsWith("/api/orders") == true)
        {
            return await RouteToOrderServiceAsync(context);
        }
        
        return new NotFoundResult();
    }
}
```

#### 2. Circuit Breaker Pattern

```csharp
public class CircuitBreaker
{
    private readonly ILogger<CircuitBreaker> _logger;
    private CircuitState _state = CircuitState.Closed;
    private int _failureCount = 0;
    private DateTime _lastFailureTime;
    private readonly int _failureThreshold;
    private readonly TimeSpan _resetTimeout;

    public CircuitBreaker(
        ILogger<CircuitBreaker> logger,
        int failureThreshold = 3,
        TimeSpan? resetTimeout = null)
    {
        _logger = logger;
        _failureThreshold = failureThreshold;
        _resetTimeout = resetTimeout ?? TimeSpan.FromMinutes(1);
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        if (_state == CircuitState.Open)
        {
            if (DateTime.UtcNow - _lastFailureTime > _resetTimeout)
            {
                _logger.LogInformation("Circuit breaker attempting to close");
                _state = CircuitState.HalfOpen;
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
            OnFailure(ex);
            throw;
        }
    }

    private void OnSuccess()
    {
        if (_state == CircuitState.HalfOpen)
        {
            _logger.LogInformation("Circuit breaker closed after successful execution");
            _state = CircuitState.Closed;
        }
        _failureCount = 0;
    }

    private void OnFailure(Exception ex)
    {
        _failureCount++;
        _lastFailureTime = DateTime.UtcNow;

        if (_failureCount >= _failureThreshold)
        {
            _logger.LogWarning(ex, "Circuit breaker opened after {FailureCount} failures", _failureCount);
            _state = CircuitState.Open;
        }
    }
}

public enum CircuitState
{
    Closed,     // Funcionando normalmente
    Open,       // Bloqueado por fallos
    HalfOpen    // Probando si se puede cerrar
}

public class CircuitBreakerOpenException : Exception
{
    public CircuitBreakerOpenException(string message) : base(message) { }
}
```

#### 3. Bulkhead Pattern

```csharp
public class BulkheadIsolation
{
    private readonly SemaphoreSlim _userServiceSemaphore;
    private readonly SemaphoreSlim _productServiceSemaphore;
    private readonly SemaphoreSlim _orderServiceSemaphore;

    public BulkheadIsolation()
    {
        // Limitar conexiones concurrentes por servicio
        _userServiceSemaphore = new SemaphoreSlim(10, 10);      // Máximo 10 conexiones
        _productServiceSemaphore = new SemaphoreSlim(20, 20);   // Máximo 20 conexiones
        _orderServiceSemaphore = new SemaphoreSlim(5, 5);       // Máximo 5 conexiones
    }

    public async Task<T> ExecuteWithBulkheadAsync<T>(
        Func<Task<T>> action,
        ServiceType serviceType)
    {
        var semaphore = GetSemaphore(serviceType);
        
        await semaphore.WaitAsync();
        try
        {
            return await action();
        }
        finally
        {
            semaphore.Release();
        }
    }

    private SemaphoreSlim GetSemaphore(ServiceType serviceType)
    {
        return serviceType switch
        {
            ServiceType.User => _userServiceSemaphore,
            ServiceType.Product => _productServiceSemaphore,
            ServiceType.Order => _orderServiceSemaphore,
            _ => throw new ArgumentException("Invalid service type")
        };
    }
}

public enum ServiceType
{
    User,
    Product,
    Order
}
```

### Estrategias de Implementación

#### 1. Domain-Driven Design (DDD)

```csharp
// Bounded Context: User Management
public class UserBoundedContext
{
    public class User
    {
        public UserId Id { get; private set; }
        public Email Email { get; private set; }
        public UserProfile Profile { get; private set; }
        public List<UserRole> Roles { get; private set; }

        public User(Email email, UserProfile profile)
        {
            Id = UserId.New();
            Email = email;
            Profile = profile;
            Roles = new List<UserRole> { UserRole.Customer };
        }

        public void AddRole(UserRole role)
        {
            if (!Roles.Contains(role))
            {
                Roles.Add(role);
            }
        }

        public void UpdateProfile(UserProfile newProfile)
        {
            Profile = newProfile;
        }
    }

    public class UserId : ValueObject
    {
        public Guid Value { get; private set; }

        private UserId(Guid value)
        {
            Value = value;
        }

        public static UserId New() => new UserId(Guid.NewGuid());
        public static UserId From(Guid value) => new UserId(value);
    }

    public class Email : ValueObject
    {
        public string Value { get; private set; }

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !value.Contains("@"))
                throw new ArgumentException("Invalid email format");

            Value = value.ToLowerInvariant();
        }
    }
}

// Bounded Context: Order Management
public class OrderBoundedContext
{
    public class Order
    {
        public OrderId Id { get; private set; }
        public UserId CustomerId { get; private set; }
        public List<OrderItem> Items { get; private set; }
        public OrderStatus Status { get; private set; }
        public Money Total { get; private set; }

        public Order(UserId customerId)
        {
            Id = OrderId.New();
            CustomerId = customerId;
            Items = new List<OrderItem>();
            Status = OrderStatus.Draft;
            Total = Money.Zero;
        }

        public void AddItem(ProductId productId, int quantity, Money unitPrice)
        {
            var item = new OrderItem(productId, quantity, unitPrice);
            Items.Add(item);
            RecalculateTotal();
        }

        public void Confirm()
        {
            if (Items.Count == 0)
                throw new InvalidOperationException("Cannot confirm empty order");

            Status = OrderStatus.Confirmed;
        }

        private void RecalculateTotal()
        {
            Total = Items.Aggregate(Money.Zero, (sum, item) => sum + item.Subtotal);
        }
    }
}
```

#### 2. Event Sourcing

```csharp
public interface IEventStore
{
    Task SaveEventsAsync(string aggregateId, IEnumerable<IDomainEvent> events, int expectedVersion);
    Task<IEnumerable<IDomainEvent>> GetEventsAsync(string aggregateId);
}

public abstract class EventSourcedAggregate
{
    private readonly List<IDomainEvent> _uncommittedEvents = new();
    private int _version = 0;

    public string Id { get; protected set; }
    public int Version => _version;

    protected void Apply(IDomainEvent @event)
    {
        When(@event);
        _uncommittedEvents.Add(@event);
    }

    protected abstract void When(IDomainEvent @event);

    public IEnumerable<IDomainEvent> GetUncommittedEvents() => _uncommittedEvents;

    public void MarkEventsAsCommitted()
    {
        _uncommittedEvents.Clear();
    }

    protected void LoadFromHistory(IEnumerable<IDomainEvent> history)
    {
        foreach (var @event in history)
        {
            When(@event);
            _version++;
        }
    }
}

// Implementación para User
public class UserEventSourced : EventSourcedAggregate
{
    public Email Email { get; private set; }
    public UserProfile Profile { get; private set; }
    public List<UserRole> Roles { get; private set; }

    public UserEventSourced()
    {
        Roles = new List<UserRole>();
    }

    public static UserEventSourced Create(Email email, UserProfile profile)
    {
        var user = new UserEventSourced();
        user.Apply(new UserCreatedEvent(email, profile));
        return user;
    }

    public void AddRole(UserRole role)
    {
        Apply(new UserRoleAddedEvent(role));
    }

    public void UpdateProfile(UserProfile newProfile)
    {
        Apply(new UserProfileUpdatedEvent(newProfile));
    }

    protected override void When(IDomainEvent @event)
    {
        switch (@event)
        {
            case UserCreatedEvent e:
                Id = e.UserId;
                Email = e.Email;
                Profile = e.Profile;
                break;
            case UserRoleAddedEvent e:
                if (!Roles.Contains(e.Role))
                    Roles.Add(e.Role);
                break;
            case UserProfileUpdatedEvent e:
                Profile = e.NewProfile;
                break;
        }
    }
}
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Diseño de Microservicios
Diseña la arquitectura de microservicios para un sistema de e-commerce que incluya:
- Gestión de usuarios
- Catálogo de productos
- Gestión de pedidos
- Sistema de pagos
- Notificaciones
- Analytics

### Ejercicio 2: Implementación de Circuit Breaker
Implementa un circuit breaker que proteja las llamadas a servicios externos con:
- Umbral de fallos configurable
- Tiempo de reset configurable
- Estados de half-open
- Logging de eventos

### Ejercicio 3: Event Sourcing
Implementa un agregado con event sourcing que maneje:
- Creación de entidades
- Actualizaciones
- Reconstrucción del estado
- Persistencia de eventos

## 📝 Quiz de Autoevaluación

1. ¿Cuáles son las principales ventajas de los microservicios sobre la arquitectura monolítica?
2. ¿En qué situaciones NO es recomendable usar microservicios?
3. ¿Cómo implementarías el patrón Circuit Breaker en un servicio de pagos?
4. ¿Qué estrategias usarías para mantener la consistencia de datos entre microservicios?
5. ¿Cómo diseñarías la comunicación entre servicios para minimizar el acoplamiento?

## 🔗 Enlaces Útiles

- [Microservices.io](https://microservices.io/)
- [Martin Fowler on Microservices](https://martinfowler.com/articles/microservices.html)
- [.NET Microservices Architecture](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/)
- [Event Sourcing Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/event-sourcing)

## 🚀 Siguiente Clase

En la siguiente clase aprenderás sobre los patrones de comunicación entre servicios, incluyendo comunicación síncrona, asíncrona, y estrategias de resiliencia.

---

**💡 Consejo**: Los microservicios no son una solución para todos los problemas. Evalúa cuidadosamente si tu proyecto realmente necesita esta complejidad antes de implementarla.
