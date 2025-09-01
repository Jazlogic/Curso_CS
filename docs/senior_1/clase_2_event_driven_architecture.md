# 🚀 Clase 2: Event-Driven Architecture

## 📋 Información de la Clase

- **Módulo**: Senior 1 - Arquitectura de Software Empresarial
- **Duración**: 2 horas
- **Nivel**: Senior
- **Prerrequisitos**: Completar Clase 1 (Arquitectura Limpia Avanzada)

## 🎯 Objetivos de Aprendizaje

- Implementar sistemas basados en eventos
- Aplicar el patrón publish-subscribe
- Implementar Event Sourcing avanzado
- Crear Sagas para transacciones distribuidas

---

## 📚 Navegación del Módulo 6

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_arquitectura_limpia_avanzada.md) | Arquitectura Limpia Avanzada | ← Anterior |
| **Clase 2** | **Event-Driven Architecture** | ← Estás aquí |
| [Clase 3](clase_3_microservicios_avanzada.md) | Arquitectura de Microservicios Avanzada | Siguiente → |
| [Clase 4](clase_4_patrones_enterprise.md) | Patrones de Diseño Enterprise | |
| [Clase 5](clase_5_arquitectura_datos_avanzada.md) | Arquitectura de Datos Avanzada | |
| [Clase 6](clase_6_calidad_codigo_metricas.md) | Calidad del Código y Métricas | |
| [Clase 7](clase_7_monitoreo_observabilidad.md) | Monitoreo y Observabilidad | |
| [Clase 8](clase_8_arquitectura_evolutiva.md) | Arquitectura Evolutiva | |
| [Clase 9](clase_9_seguridad_enterprise.md) | Arquitectura de Seguridad Enterprise | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Plataforma Empresarial | |

**← [Volver al README del Módulo 6](../senior_1/README.md)**

---

## 📚 Contenido Teórico

### 1. Event-Driven Architecture

La arquitectura basada en eventos permite que los componentes se comuniquen de manera desacoplada a través de eventos.

```csharp
// ===== EVENT-DRIVEN ARCHITECTURE - IMPLEMENTACIÓN COMPLETA =====
namespace EventDrivenArchitecture
{
    // ===== EVENT BUS =====
    namespace Infrastructure.EventBus
    {
        public interface IEventBus
        {
            Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent;
            Task SubscribeAsync<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent;
            Task UnsubscribeAsync<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent;
        }
        
        public class InMemoryEventBus : IEventBus
        {
            private readonly Dictionary<Type, List<IEventHandler>> _handlers = new();
            private readonly ILogger<InMemoryEventBus> _logger;
            
            public InMemoryEventBus(ILogger<InMemoryEventBus> logger)
            {
                _logger = logger;
            }
            
            public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent
            {
                var eventType = typeof(TEvent);
                
                if (_handlers.TryGetValue(eventType, out var handlers))
                {
                    var tasks = handlers.Select(handler => 
                    {
                        try
                        {
                            return ((IEventHandler<TEvent>)handler).HandleAsync(@event);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error handling event {EventType}", eventType.Name);
                            return Task.CompletedTask;
                        }
                    });
                    
                    await Task.WhenAll(tasks);
                }
                
                _logger.LogInformation("Event {EventType} published to {HandlerCount} handlers", 
                    eventType.Name, handlers?.Count ?? 0);
            }
            
            public async Task SubscribeAsync<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent
            {
                var eventType = typeof(TEvent);
                
                if (!_handlers.ContainsKey(eventType))
                {
                    _handlers[eventType] = new List<IEventHandler>();
                }
                
                _handlers[eventType].Add(handler);
                _logger.LogInformation("Handler {HandlerType} subscribed to {EventType}", 
                    handler.GetType().Name, eventType.Name);
            }
            
            public async Task UnsubscribeAsync<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent
            {
                var eventType = typeof(TEvent);
                
                if (_handlers.TryGetValue(eventType, out var handlers))
                {
                    handlers.Remove(handler);
                    _logger.LogInformation("Handler {HandlerType} unsubscribed from {EventType}", 
                        handler.GetType().Name, eventType.Name);
                }
            }
        }
        
        public class RabbitMQEventBus : IEventBus
        {
            private readonly IConnection _connection;
            private readonly IModel _channel;
            private readonly ILogger<RabbitMQEventBus> _logger;
            
            public RabbitMQEventBus(IConnection connection, ILogger<RabbitMQEventBus> logger)
            {
                _connection = connection;
                _channel = connection.CreateModel();
                _logger = logger;
                
                // Declarar exchange
                _channel.ExchangeDeclare("domain_events", ExchangeType.Topic, true, false);
            }
            
            public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent
            {
                var eventType = typeof(TEvent).Name;
                var message = JsonSerializer.Serialize(@event);
                var body = Encoding.UTF8.GetBytes(message);
                
                _channel.BasicPublish(
                    exchange: "domain_events",
                    routingKey: eventType,
                    basicProperties: null,
                    body: body);
                
                _logger.LogInformation("Event {EventType} published to RabbitMQ", eventType);
            }
            
            public async Task SubscribeAsync<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent
            {
                var eventType = typeof(TEvent).Name;
                var queueName = $"queue_{eventType}_{Guid.NewGuid()}";
                
                _channel.QueueDeclare(queueName, true, false, false);
                _channel.QueueBind(queueName, "domain_events", eventType);
                
                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var @event = JsonSerializer.Deserialize<TEvent>(message);
                        
                        await handler.HandleAsync(@event);
                        
                        _channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error handling event {EventType}", eventType);
                        _channel.BasicNack(ea.DeliveryTag, false, true);
                    }
                };
                
                _channel.BasicConsume(queueName, false, consumer);
                _logger.LogInformation("Handler {HandlerType} subscribed to {EventType}", 
                    handler.GetType().Name, eventType);
            }
            
            public async Task UnsubscribeAsync<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent
            {
                // En RabbitMQ, la cancelación se maneja a nivel de conexión
                _logger.LogInformation("Handler {HandlerType} unsubscribed from {EventType}", 
                    handler.GetType().Name, typeof(TEvent).Name);
            }
        }
    }
    
    // ===== EVENT HANDLERS =====
    namespace Application.EventHandlers
    {
        public interface IEventHandler<in TEvent> where TEvent : IEvent
        {
            Task HandleAsync(TEvent @event);
        }
        
        public class UserCreatedEventHandler : IEventHandler<UserCreatedEvent>
        {
            private readonly IEmailService _emailService;
            private readonly ILogger<UserCreatedEventHandler> _logger;
            
            public UserCreatedEventHandler(IEmailService emailService, ILogger<UserCreatedEventHandler> logger)
            {
                _emailService = emailService;
                _logger = logger;
            }
            
            public async Task HandleAsync(UserCreatedEvent @event)
            {
                try
                {
                    _logger.LogInformation("Handling UserCreatedEvent for user {UserId}", @event.UserId);
                    
                    // Enviar email de bienvenida
                    var welcomeEmail = new EmailMessage
                    {
                        To = @event.Email,
                        Subject = "Bienvenido a nuestra plataforma",
                        Body = $"Hola {@event.Username}, ¡bienvenido a nuestra plataforma!"
                    };
                    
                    await _emailService.SendAsync(welcomeEmail);
                    
                    _logger.LogInformation("Welcome email sent to user {UserId}", @event.UserId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling UserCreatedEvent for user {UserId}", @event.UserId);
                    throw;
                }
            }
        }
        
        public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
        {
            private readonly IInventoryService _inventoryService;
            private readonly ILogger<OrderCreatedEventHandler> _logger;
            
            public OrderCreatedEventHandler(IInventoryService inventoryService, ILogger<OrderCreatedEventHandler> logger)
            {
                _inventoryService = inventoryService;
                _logger = logger;
            }
            
            public async Task HandleAsync(OrderCreatedEvent @event)
            {
                try
                {
                    _logger.LogInformation("Handling OrderCreatedEvent for order {OrderId}", @event.OrderId);
                    
                    // Reservar inventario
                    foreach (var item in @event.Items)
                    {
                        await _inventoryService.ReserveAsync(item.ProductId, item.Quantity);
                    }
                    
                    _logger.LogInformation("Inventory reserved for order {OrderId}", @event.OrderId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling OrderCreatedEvent for order {OrderId}", @event.OrderId);
                    throw;
                }
            }
        }
        
        public class OrderCancelledEventHandler : IEventHandler<OrderCancelledEvent>
        {
            private readonly IInventoryService _inventoryService;
            private readonly IRefundService _refundService;
            private readonly ILogger<OrderCancelledEventHandler> _logger;
            
            public OrderCancelledEventHandler(
                IInventoryService inventoryService,
                IRefundService refundService,
                ILogger<OrderCancelledEventHandler> logger)
            {
                _inventoryService = inventoryService;
                _refundService = refundService;
                _logger = logger;
            }
            
            public async Task HandleAsync(OrderCancelledEvent @event)
            {
                try
                {
                    _logger.LogInformation("Handling OrderCancelledEvent for order {OrderId}", @event.OrderId);
                    
                    // Liberar inventario reservado
                    foreach (var item in @event.Items)
                    {
                        await _inventoryService.ReleaseAsync(item.ProductId, item.Quantity);
                    }
                    
                    // Procesar reembolso
                    await _refundService.ProcessRefundAsync(@event.OrderId, @event.TotalAmount);
                    
                    _logger.LogInformation("Order cancellation processed for order {OrderId}", @event.OrderId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling OrderCancelledEvent for order {OrderId}", @event.OrderId);
                    throw;
                }
            }
        }
    }
    
    // ===== EVENT SOURCING =====
    namespace Infrastructure.EventSourcing
    {
        public interface IEventStore
        {
            Task SaveEventsAsync(string aggregateId, IEnumerable<IEvent> events, int expectedVersion);
            Task<IEnumerable<IEvent>> GetEventsAsync(string aggregateId);
            Task<int> GetLatestVersionAsync(string aggregateId);
        }
        
        public class SqlEventStore : IEventStore
        {
            private readonly ApplicationDbContext _context;
            private readonly ILogger<SqlEventStore> _logger;
            
            public SqlEventStore(ApplicationDbContext context, ILogger<SqlEventStore> logger)
            {
                _context = context;
                _logger = logger;
            }
            
            public async Task SaveEventsAsync(string aggregateId, IEnumerable<IEvent> events, int expectedVersion)
            {
                var eventList = events.ToList();
                var version = expectedVersion;
                
                foreach (var @event in eventList)
                {
                    version++;
                    @event.Version = version;
                    
                    var eventData = new EventData
                    {
                        AggregateId = aggregateId,
                        Version = version,
                        EventType = @event.GetType().Name,
                        EventData = JsonSerializer.Serialize(@event),
                        OccurredOn = @event.OccurredOn
                    };
                    
                    _context.Events.Add(eventData);
                }
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Saved {EventCount} events for aggregate {AggregateId}", 
                    eventList.Count, aggregateId);
            }
            
            public async Task<IEnumerable<IEvent>> GetEventsAsync(string aggregateId)
            {
                var eventDataList = await _context.Events
                    .Where(e => e.AggregateId == aggregateId)
                    .OrderBy(e => e.Version)
                    .ToListAsync();
                
                var events = new List<IEvent>();
                
                foreach (var eventData in eventDataList)
                {
                    var eventType = Type.GetType(eventData.EventType);
                    if (eventType != null)
                    {
                        var @event = (IEvent)JsonSerializer.Deserialize(eventData.EventData, eventType);
                        events.Add(@event);
                    }
                }
                
                return events;
            }
            
            public async Task<int> GetLatestVersionAsync(string aggregateId)
            {
                var latestEvent = await _context.Events
                    .Where(e => e.AggregateId == aggregateId)
                    .OrderByDescending(e => e.Version)
                    .FirstOrDefaultAsync();
                
                return latestEvent?.Version ?? 0;
            }
        }
        
        public class EventData
        {
            public int Id { get; set; }
            public string AggregateId { get; set; }
            public int Version { get; set; }
            public string EventType { get; set; }
            public string EventData { get; set; }
            public DateTime OccurredOn { get; set; }
        }
    }
    
    // ===== SAGAS =====
    namespace Application.Sagas
    {
        public interface ISaga<TState> where TState : class
        {
            Guid Id { get; }
            TState State { get; }
            Task ExecuteAsync();
            Task CompensateAsync();
        }
        
        public abstract class Saga<TState> : ISaga<TState> where TState : class
        {
            public Guid Id { get; protected set; }
            public TState State { get; protected set; }
            
            protected Saga()
            {
                Id = Guid.NewGuid();
            }
            
            public abstract Task ExecuteAsync();
            public abstract Task CompensateAsync();
            
            protected void UpdateState(TState newState)
            {
                State = newState;
            }
        }
        
        public class OrderProcessingSaga : Saga<OrderProcessingState>
        {
            private readonly IOrderService _orderService;
            private readonly IInventoryService _inventoryService;
            private readonly IPaymentService _paymentService;
            private readonly IShippingService _shippingService;
            private readonly ILogger<OrderProcessingSaga> _logger;
            
            public OrderProcessingSaga(
                IOrderService orderService,
                IInventoryService inventoryService,
                IPaymentService paymentService,
                IShippingService shippingService,
                ILogger<OrderProcessingSaga> logger)
            {
                _orderService = orderService;
                _inventoryService = inventoryService;
                _paymentService = paymentService;
                _shippingService = shippingService;
                _logger = logger;
                
                State = new OrderProcessingState();
            }
            
            public override async Task ExecuteAsync()
            {
                try
                {
                    _logger.LogInformation("Starting OrderProcessingSaga {SagaId} for order {OrderId}", 
                        Id, State.OrderId);
                    
                    // Paso 1: Reservar inventario
                    await ReserveInventoryAsync();
                    
                    // Paso 2: Procesar pago
                    await ProcessPaymentAsync();
                    
                    // Paso 3: Confirmar orden
                    await ConfirmOrderAsync();
                    
                    // Paso 4: Preparar envío
                    await PrepareShippingAsync();
                    
                    _logger.LogInformation("OrderProcessingSaga {SagaId} completed successfully", Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in OrderProcessingSaga {SagaId}", Id);
                    await CompensateAsync();
                }
            }
            
            public override async Task CompensateAsync()
            {
                try
                {
                    _logger.LogInformation("Starting compensation for OrderProcessingSaga {SagaId}", Id);
                    
                    // Compensar en orden inverso
                    if (State.ShippingPrepared)
                        await CancelShippingAsync();
                    
                    if (State.OrderConfirmed)
                        await CancelOrderAsync();
                    
                    if (State.PaymentProcessed)
                        await RefundPaymentAsync();
                    
                    if (State.InventoryReserved)
                        await ReleaseInventoryAsync();
                    
                    _logger.LogInformation("Compensation completed for OrderProcessingSaga {SagaId}", Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during compensation for OrderProcessingSaga {SagaId}", Id);
                    throw;
                }
            }
            
            private async Task ReserveInventoryAsync()
            {
                foreach (var item in State.OrderItems)
                {
                    await _inventoryService.ReserveAsync(item.ProductId, item.Quantity);
                }
                
                State.InventoryReserved = true;
                _logger.LogInformation("Inventory reserved for saga {SagaId}", Id);
            }
            
            private async Task ProcessPaymentAsync()
            {
                var paymentResult = await _paymentService.ProcessAsync(State.OrderId, State.TotalAmount);
                if (!paymentResult.IsSuccess)
                    throw new BusinessException("Payment processing failed");
                
                State.PaymentProcessed = true;
                _logger.LogInformation("Payment processed for saga {SagaId}", Id);
            }
            
            private async Task ConfirmOrderAsync()
            {
                await _orderService.ConfirmAsync(State.OrderId);
                State.OrderConfirmed = true;
                _logger.LogInformation("Order confirmed for saga {SagaId}", Id);
            }
            
            private async Task PrepareShippingAsync()
            {
                await _shippingService.PrepareAsync(State.OrderId);
                State.ShippingPrepared = true;
                _logger.LogInformation("Shipping prepared for saga {SagaId}", Id);
            }
            
            private async Task ReleaseInventoryAsync()
            {
                foreach (var item in State.OrderItems)
                {
                    await _inventoryService.ReleaseAsync(item.ProductId, item.Quantity);
                }
                
                _logger.LogInformation("Inventory released for saga {SagaId}", Id);
            }
            
            private async Task RefundPaymentAsync()
            {
                await _paymentService.RefundAsync(State.OrderId);
                _logger.LogInformation("Payment refunded for saga {SagaId}", Id);
            }
            
            private async Task CancelOrderAsync()
            {
                await _orderService.CancelAsync(State.OrderId);
                _logger.LogInformation("Order cancelled for saga {SagaId}", Id);
            }
            
            private async Task CancelShippingAsync()
            {
                await _shippingService.CancelAsync(State.OrderId);
                _logger.LogInformation("Shipping cancelled for saga {SagaId}", Id);
            }
        }
        
        public class OrderProcessingState
        {
            public int OrderId { get; set; }
            public List<OrderItemDto> OrderItems { get; set; } = new();
            public decimal TotalAmount { get; set; }
            public bool InventoryReserved { get; set; }
            public bool PaymentProcessed { get; set; }
            public bool OrderConfirmed { get; set; }
            public bool ShippingPrepared { get; set; }
        }
    }
    
    // ===== EVENT PROJECTIONS =====
    namespace Application.Projections
    {
        public interface IProjection<TEvent> where TEvent : IEvent
        {
            Task ProjectAsync(TEvent @event);
        }
        
        public class UserProjection : IProjection<UserCreatedEvent>
        {
            private readonly IUserReadRepository _userReadRepository;
            private readonly ILogger<UserProjection> _logger;
            
            public UserProjection(IUserReadRepository userReadRepository, ILogger<UserProjection> logger)
            {
                _userReadRepository = userReadRepository;
                _logger = logger;
            }
            
            public async Task ProjectAsync(UserCreatedEvent @event)
            {
                try
                {
                    var userReadModel = new UserReadModel
                    {
                        Id = @event.UserId,
                        Username = @event.Username,
                        Email = @event.Email,
                        Status = "Active",
                        CreatedAt = @event.OccurredOn
                    };
                    
                    await _userReadRepository.AddAsync(userReadModel);
                    
                    _logger.LogInformation("User projection created for user {UserId}", @event.UserId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error projecting UserCreatedEvent for user {UserId}", @event.UserId);
                    throw;
                }
            }
        }
        
        public class OrderProjection : IProjection<OrderCreatedEvent>
        {
            private readonly IOrderReadRepository _orderReadRepository;
            private readonly ILogger<OrderProjection> _logger;
            
            public OrderProjection(IOrderReadRepository orderReadRepository, ILogger<OrderProjection> logger)
            {
                _orderReadRepository = orderReadRepository;
                _logger = logger;
            }
            
            public async Task ProjectAsync(OrderCreatedEvent @event)
            {
                try
                {
                    var orderReadModel = new OrderReadModel
                    {
                        Id = @event.OrderId,
                        UserId = @event.UserId,
                        Status = "Created",
                        TotalAmount = @event.TotalAmount.Amount,
                        CreatedAt = @event.OccurredOn
                    };
                    
                    await _orderReadRepository.AddAsync(orderReadModel);
                    
                    _logger.LogInformation("Order projection created for order {OrderId}", @event.OrderId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error projecting OrderCreatedEvent for order {OrderId}", @event.OrderId);
                    throw;
                }
            }
        }
    }
    
    // ===== READ MODELS =====
    namespace Application.ReadModels
    {
        public class UserReadModel
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string Status { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }
        
        public class OrderReadModel
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public string Status { get; set; }
            public decimal TotalAmount { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }
    }
    
    // ===== DEPENDENCY INJECTION =====
    namespace Infrastructure.DependencyInjection
    {
        public static class ServiceCollectionExtensions
        {
            public static IServiceCollection AddEventDrivenArchitecture(this IServiceCollection services)
            {
                // Event Bus
                services.AddSingleton<IEventBus, InMemoryEventBus>();
                
                // Event Handlers
                services.AddScoped<IEventHandler<UserCreatedEvent>, UserCreatedEventHandler>();
                services.AddScoped<IEventHandler<OrderCreatedEvent>, OrderCreatedEventHandler>();
                services.AddScoped<IEventHandler<OrderCancelledEvent>, OrderCancelledEventHandler>();
                
                // Event Store
                services.AddScoped<IEventStore, SqlEventStore>();
                
                // Sagas
                services.AddScoped<OrderProcessingSaga>();
                
                // Projections
                services.AddScoped<IProjection<UserCreatedEvent>, UserProjection>();
                services.AddScoped<IProjection<OrderCreatedEvent>, OrderProjection>();
                
                return services;
            }
        }
    }
}

// Uso de Event-Driven Architecture
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Event-Driven Architecture ===\n");
        
        Console.WriteLine("Los componentes implementados incluyen:");
        Console.WriteLine("1. Event Bus (InMemory y RabbitMQ)");
        Console.WriteLine("2. Event Handlers para procesar eventos");
        Console.WriteLine("3. Event Sourcing con almacenamiento persistente");
        Console.WriteLine("4. Sagas para transacciones distribuidas");
        Console.WriteLine("5. Projections para modelos de lectura");
        Console.WriteLine("6. Read Models optimizados para consultas");
        
        Console.WriteLine("\nBeneficios de esta arquitectura:");
        Console.WriteLine("- Desacoplamiento entre componentes");
        Console.WriteLine("- Escalabilidad horizontal");
        Console.WriteLine("- Trazabilidad completa de eventos");
        Console.WriteLine("- Resiliencia y recuperación de errores");
        Console.WriteLine("- Flexibilidad para cambios de negocio");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Implementar Event Bus
Crea un Event Bus personalizado con diferentes estrategias de publicación.

### Ejercicio 2: Crear Saga Compleja
Implementa una Saga para un proceso de negocio complejo con múltiples pasos.

### Ejercicio 3: Event Projections
Crea proyecciones para diferentes tipos de eventos y modelos de lectura.

## 🔍 Puntos Clave

1. **Event Bus** permite comunicación desacoplada entre componentes
2. **Event Handlers** procesan eventos de manera asíncrona
3. **Event Sourcing** mantiene historial completo de cambios
4. **Sagas** manejan transacciones distribuidas complejas
5. **Projections** crean modelos de lectura optimizados

## 📚 Recursos Adicionales

- [Event-Driven Architecture](https://martinfowler.com/articles/201701-event-driven.html)
- [Event Sourcing](https://martinfowler.com/eaaDev/EventSourcing.html)
- [Saga Pattern](https://microservices.io/patterns/data/saga.html)

---

**🎯 ¡Has completado la Clase 2! Ahora comprendes Event-Driven Architecture**

**📚 [Siguiente: Clase 3 - Arquitectura de Microservicios Avanzada](clase_3_microservicios_avanzada.md)**
