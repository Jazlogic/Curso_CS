# Clase 7: Event Sourcing

## Navegación
- [← Clase anterior: Message Bus](clase_6_message_bus.md)
- [← Volver al README del módulo](README.md)
- [→ Siguiente clase: CQRS Avanzado](clase_8_cqrs_avanzado.md)

## Objetivos de Aprendizaje
- Comprender el patrón Event Sourcing
- Implementar almacenamiento de eventos
- Crear agregados basados en eventos
- Aplicar reconstrucción de estado

## ¿Qué es Event Sourcing?

**Event Sourcing** es un patrón arquitectónico que almacena todos los cambios en el estado de una aplicación como una secuencia de eventos. En lugar de almacenar solo el estado actual, se mantiene un historial completo de todos los eventos que han ocurrido, permitiendo reconstruir el estado en cualquier punto del tiempo y proporcionando auditoría completa.

### Beneficios del Event Sourcing

```csharp
// 1. Auditoría Completa
// Todos los cambios quedan registrados como eventos
public class OrderEventStore
{
    public async Task<IEnumerable<DomainEvent>> GetEventsAsync(Guid orderId)
    {
        // Retorna todos los eventos de una orden
        // OrderCreated, OrderItemAdded, OrderConfirmed, etc.
    }
}

// 2. Reconstrucción de Estado
// El estado actual se puede reconstruir desde los eventos
public class OrderAggregate
{
    public static Order ReconstructFromEvents(IEnumerable<DomainEvent> events)
    {
        var order = new Order();
        foreach (var @event in events.OrderBy(e => e.Timestamp))
        {
            order.Apply(@event);
        }
        return order;
    }
}

// 3. Análisis Temporal
// Se puede analizar el comportamiento a lo largo del tiempo
public class OrderAnalytics
{
    public async Task<OrderTimeline> GetOrderTimelineAsync(Guid orderId)
    {
        var events = await _eventStore.GetEventsAsync(orderId);
        return BuildTimeline(events);
    }
}

// 4. Debugging y Troubleshooting
// Se puede reproducir exactamente qué pasó
public class OrderDebugger
{
    public async Task<OrderDebugInfo> DebugOrderAsync(Guid orderId, DateTime pointInTime)
    {
        var events = await _eventStore.GetEventsAsync(orderId, pointInTime);
        return new OrderDebugInfo(events);
    }
}
```

## Implementación del Event Sourcing

### 1. Event Store

```csharp
// Domain/Events/IDomainEvent.cs
public interface IDomainEvent
{
    Guid Id { get; }
    Guid AggregateId { get; }
    long Version { get; }
    DateTime Timestamp { get; }
    string EventType { get; }
}

// Domain/Events/DomainEvent.cs
public abstract class DomainEvent : IDomainEvent
{
    public Guid Id { get; set; }
    public Guid AggregateId { get; set; }
    public long Version { get; set; }
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; }
    public string CorrelationId { get; set; }
    public string Source { get; set; }
    
    protected DomainEvent(Guid aggregateId)
    {
        Id = Guid.NewGuid();
        AggregateId = aggregateId;
        Timestamp = DateTime.UtcNow;
        EventType = GetType().Name;
    }
}

// Domain/Events/OrderEvents.cs
public class OrderCreatedEvent : DomainEvent
{
    public Guid CustomerId { get; set; }
    public List<OrderItem> Items { get; set; }
    public string ShippingAddress { get; set; }
    public string PaymentMethod { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public OrderCreatedEvent(Guid orderId, Guid customerId, List<OrderItem> items, string shippingAddress, string paymentMethod) 
        : base(orderId)
    {
        CustomerId = customerId;
        Items = items;
        ShippingAddress = shippingAddress;
        PaymentMethod = paymentMethod;
        CreatedAt = DateTime.UtcNow;
    }
}

public class OrderItemAddedEvent : DomainEvent
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    
    public OrderItemAddedEvent(Guid orderId, Guid productId, int quantity, decimal unitPrice) 
        : base(orderId)
    {
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}

public class OrderStatusChangedEvent : DomainEvent
{
    public string OldStatus { get; set; }
    public string NewStatus { get; set; }
    public DateTime ChangedAt { get; set; }
    
    public OrderStatusChangedEvent(Guid orderId, string oldStatus, string newStatus) 
        : base(orderId)
    {
        OldStatus = oldStatus;
        NewStatus = newStatus;
        ChangedAt = DateTime.UtcNow;
    }
}

public class OrderCancelledEvent : DomainEvent
{
    public string Reason { get; set; }
    public DateTime CancelledAt { get; set; }
    
    public OrderCancelledEvent(Guid orderId, string reason) 
        : base(orderId)
    {
        Reason = reason;
        CancelledAt = DateTime.UtcNow;
    }
}

// Infrastructure/EventStore/IEventStore.cs
public interface IEventStore
{
    Task SaveEventsAsync(Guid aggregateId, IEnumerable<DomainEvent> events, long expectedVersion);
    Task<IEnumerable<DomainEvent>> GetEventsAsync(Guid aggregateId);
    Task<IEnumerable<DomainEvent>> GetEventsAsync(Guid aggregateId, DateTime until);
    Task<long> GetLastVersionAsync(Guid aggregateId);
    Task<bool> AggregateExistsAsync(Guid aggregateId);
}

// Infrastructure/EventStore/EventStore.cs
public class EventStore : IEventStore
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EventStore> _logger;
    
    public EventStore(ApplicationDbContext context, ILogger<EventStore> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task SaveEventsAsync(Guid aggregateId, IEnumerable<DomainEvent> events, long expectedVersion)
    {
        try
        {
            var eventList = events.ToList();
            if (!eventList.Any()) return;
            
            // Verificar concurrencia
            var lastVersion = await GetLastVersionAsync(aggregateId);
            if (expectedVersion != -1 && lastVersion != expectedVersion)
            {
                throw new ConcurrencyException($"Expected version {expectedVersion}, but found {lastVersion}");
            }
            
            var nextVersion = expectedVersion + 1;
            
            // Guardar eventos
            var eventEntities = eventList.Select((e, index) => new EventEntity
            {
                Id = e.Id,
                AggregateId = aggregateId,
                Version = nextVersion + index,
                EventType = e.EventType,
                EventData = JsonSerializer.Serialize(e, e.GetType()),
                Timestamp = e.Timestamp,
                CorrelationId = e.CorrelationId,
                Source = e.Source
            }).ToList();
            
            await _context.Events.AddRangeAsync(eventEntities);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Saved {EventCount} events for aggregate {AggregateId} starting from version {Version}", 
                eventList.Count, aggregateId, nextVersion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving events for aggregate {AggregateId}", aggregateId);
            throw;
        }
    }
    
    public async Task<IEnumerable<DomainEvent>> GetEventsAsync(Guid aggregateId)
    {
        try
        {
            var eventEntities = await _context.Events
                .Where(e => e.AggregateId == aggregateId)
                .OrderBy(e => e.Version)
                .ToListAsync();
            
            var events = new List<DomainEvent>();
            
            foreach (var eventEntity in eventEntities)
            {
                var eventType = Type.GetType(eventEntity.EventType);
                if (eventType != null)
                {
                    var domainEvent = JsonSerializer.Deserialize(eventEntity.EventData, eventType) as DomainEvent;
                    if (domainEvent != null)
                    {
                        domainEvent.Id = eventEntity.Id;
                        domainEvent.Version = eventEntity.Version;
                        domainEvent.Timestamp = eventEntity.Timestamp;
                        domainEvent.CorrelationId = eventEntity.CorrelationId;
                        domainEvent.Source = eventEntity.Source;
                        events.Add(domainEvent);
                    }
                }
            }
            
            _logger.LogInformation("Retrieved {EventCount} events for aggregate {AggregateId}", 
                events.Count, aggregateId);
            
            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events for aggregate {AggregateId}", aggregateId);
            throw;
        }
    }
    
    public async Task<IEnumerable<DomainEvent>> GetEventsAsync(Guid aggregateId, DateTime until)
    {
        try
        {
            var eventEntities = await _context.Events
                .Where(e => e.AggregateId == aggregateId && e.Timestamp <= until)
                .OrderBy(e => e.Version)
                .ToListAsync();
            
            var events = new List<DomainEvent>();
            
            foreach (var eventEntity in eventEntities)
            {
                var eventType = Type.GetType(eventEntity.EventType);
                if (eventType != null)
                {
                    var domainEvent = JsonSerializer.Deserialize(eventEntity.EventData, eventType) as DomainEvent;
                    if (domainEvent != null)
                    {
                        domainEvent.Id = eventEntity.Id;
                        domainEvent.Version = eventEntity.Version;
                        domainEvent.Timestamp = eventEntity.Timestamp;
                        domainEvent.CorrelationId = eventEntity.CorrelationId;
                        domainEvent.Source = eventEntity.Source;
                        events.Add(domainEvent);
                    }
                }
            }
            
            _logger.LogInformation("Retrieved {EventCount} events for aggregate {AggregateId} until {Until}", 
                events.Count, aggregateId, until);
            
            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events for aggregate {AggregateId} until {Until}", aggregateId, until);
            throw;
        }
    }
    
    public async Task<long> GetLastVersionAsync(Guid aggregateId)
    {
        var lastEvent = await _context.Events
            .Where(e => e.AggregateId == aggregateId)
            .OrderByDescending(e => e.Version)
            .FirstOrDefaultAsync();
        
        return lastEvent?.Version ?? -1;
    }
    
    public async Task<bool> AggregateExistsAsync(Guid aggregateId)
    {
        return await _context.Events.AnyAsync(e => e.AggregateId == aggregateId);
    }
}

// Infrastructure/Data/EventEntity.cs
public class EventEntity
{
    public Guid Id { get; set; }
    public Guid AggregateId { get; set; }
    public long Version { get; set; }
    public string EventType { get; set; }
    public string EventData { get; set; }
    public DateTime Timestamp { get; set; }
    public string CorrelationId { get; set; }
    public string Source { get; set; }
}
```

### 2. Agregados Event-Sourced

```csharp
// Domain/Entities/EventSourcedAggregate.cs
public abstract class EventSourcedAggregate
{
    private readonly List<DomainEvent> _uncommittedEvents = new();
    private long _version = -1;
    
    public Guid Id { get; protected set; }
    public long Version => _version;
    
    protected void ApplyChange(DomainEvent @event)
    {
        Apply(@event);
        _uncommittedEvents.Add(@event);
    }
    
    protected abstract void Apply(DomainEvent @event);
    
    public IEnumerable<DomainEvent> GetUncommittedEvents()
    {
        return _uncommittedEvents.AsReadOnly();
    }
    
    public void MarkEventsAsCommitted()
    {
        _uncommittedEvents.Clear();
    }
    
    public void LoadFromHistory(IEnumerable<DomainEvent> history)
    {
        foreach (var @event in history.OrderBy(e => e.Version))
        {
            Apply(@event);
            _version = @event.Version;
        }
    }
}

// Domain/Entities/Order.cs (Event-Sourced)
public class Order : EventSourcedAggregate
{
    public Guid CustomerId { get; private set; }
    public List<OrderItem> Items { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public string ShippingAddress { get; private set; }
    public string PaymentMethod { get; private set; }
    public decimal Total => Items.Sum(item => item.Total);
    
    private Order() 
    { 
        Items = new List<OrderItem>();
    }
    
    public static Order Create(Guid customerId, List<OrderItem> items, string shippingAddress, string paymentMethod)
    {
        if (items == null || !items.Any())
            throw new DomainException("Order must have at least one item");
            
        if (customerId == Guid.Empty)
            throw new DomainException("Customer ID is required");
            
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Items = items,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ShippingAddress = shippingAddress,
            PaymentMethod = paymentMethod
        };
        
        // Aplicar evento
        order.ApplyChange(new OrderCreatedEvent(order.Id, customerId, items, shippingAddress, paymentMethod));
        
        return order;
    }
    
    public void AddItem(OrderItem item)
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Cannot add items to non-pending orders");
            
        if (item == null)
            throw new DomainException("Item cannot be null");
            
        Items.Add(item);
        
        // Aplicar evento
        ApplyChange(new OrderItemAddedEvent(Id, item.ProductId, item.Quantity, item.UnitPrice));
    }
    
    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Only pending orders can be confirmed");
            
        Status = OrderStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
        
        // Aplicar evento
        ApplyChange(new OrderStatusChangedEvent(Id, OrderStatus.Pending.ToString(), OrderStatus.Confirmed.ToString()));
    }
    
    public void Ship()
    {
        if (Status != OrderStatus.Confirmed)
            throw new DomainException("Only confirmed orders can be shipped");
            
        Status = OrderStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
        
        // Aplicar evento
        ApplyChange(new OrderStatusChangedEvent(Id, OrderStatus.Confirmed.ToString(), OrderStatus.Shipped.ToString()));
    }
    
    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new DomainException("Only shipped orders can be delivered");
            
        Status = OrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        
        // Aplicar evento
        ApplyChange(new OrderStatusChangedEvent(Id, OrderStatus.Shipped.ToString(), OrderStatus.Delivered.ToString()));
    }
    
    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Delivered)
            throw new DomainException("Delivered orders cannot be cancelled");
            
        Status = OrderStatus.Cancelled;
        
        // Aplicar evento
        ApplyChange(new OrderCancelledEvent(Id, reason));
    }
    
    protected override void Apply(DomainEvent @event)
    {
        switch (@event)
        {
            case OrderCreatedEvent e:
                Apply(e);
                break;
            case OrderItemAddedEvent e:
                Apply(e);
                break;
            case OrderStatusChangedEvent e:
                Apply(e);
                break;
            case OrderCancelledEvent e:
                Apply(e);
                break;
        }
    }
    
    private void Apply(OrderCreatedEvent @event)
    {
        Id = @event.AggregateId;
        CustomerId = @event.CustomerId;
        Items = @event.Items;
        ShippingAddress = @event.ShippingAddress;
        PaymentMethod = @event.PaymentMethod;
        CreatedAt = @event.CreatedAt;
        Status = OrderStatus.Pending;
    }
    
    private void Apply(OrderItemAddedEvent @event)
    {
        var item = new OrderItem(@event.ProductId, @event.Quantity, @event.UnitPrice);
        Items.Add(item);
    }
    
    private void Apply(OrderStatusChangedEvent @event)
    {
        Status = Enum.Parse<OrderStatus>(@event.NewStatus);
        
        switch (Status)
        {
            case OrderStatus.Confirmed:
                ConfirmedAt = @event.ChangedAt;
                break;
            case OrderStatus.Shipped:
                ShippedAt = @event.ChangedAt;
                break;
            case OrderStatus.Delivered:
                DeliveredAt = @event.ChangedAt;
                break;
        }
    }
    
    private void Apply(OrderCancelledEvent @event)
    {
        Status = OrderStatus.Cancelled;
    }
}
```

### 3. Repositorios Event-Sourced

```csharp
// Domain/Repositories/IEventSourcedRepository.cs
public interface IEventSourcedRepository<TAggregate> where TAggregate : EventSourcedAggregate
{
    Task<TAggregate> GetByIdAsync(Guid id);
    Task<TAggregate> GetByIdAsync(Guid id, DateTime until);
    Task SaveAsync(TAggregate aggregate);
    Task<bool> ExistsAsync(Guid id);
}

// Infrastructure/Repositories/EventSourcedRepository.cs
public class EventSourcedRepository<TAggregate> : IEventSourcedRepository<TAggregate> 
    where TAggregate : EventSourcedAggregate, new()
{
    private readonly IEventStore _eventStore;
    private readonly ILogger<EventSourcedRepository<TAggregate>> _logger;
    
    public EventSourcedRepository(IEventStore eventStore, ILogger<EventSourcedRepository<TAggregate>> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }
    
    public async Task<TAggregate> GetByIdAsync(Guid id)
    {
        try
        {
            var events = await _eventStore.GetEventsAsync(id);
            if (!events.Any())
                return null;
            
            var aggregate = new TAggregate();
            aggregate.LoadFromHistory(events);
            
            _logger.LogInformation("Retrieved aggregate {AggregateType} with ID {Id} from {EventCount} events", 
                typeof(TAggregate).Name, id, events.Count());
            
            return aggregate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving aggregate {AggregateType} with ID {Id}", typeof(TAggregate).Name, id);
            throw;
        }
    }
    
    public async Task<TAggregate> GetByIdAsync(Guid id, DateTime until)
    {
        try
        {
            var events = await _eventStore.GetEventsAsync(id, until);
            if (!events.Any())
                return null;
            
            var aggregate = new TAggregate();
            aggregate.LoadFromHistory(events);
            
            _logger.LogInformation("Retrieved aggregate {AggregateType} with ID {Id} from {EventCount} events until {Until}", 
                typeof(TAggregate).Name, id, events.Count(), until);
            
            return aggregate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving aggregate {AggregateType} with ID {Id} until {Until}", 
                typeof(TAggregate).Name, id, until);
            throw;
        }
    }
    
    public async Task SaveAsync(TAggregate aggregate)
    {
        try
        {
            var uncommittedEvents = aggregate.GetUncommittedEvents();
            if (!uncommittedEvents.Any())
                return;
            
            await _eventStore.SaveEventsAsync(aggregate.Id, uncommittedEvents, aggregate.Version);
            
            aggregate.MarkEventsAsCommitted();
            
            _logger.LogInformation("Saved aggregate {AggregateType} with ID {Id} with {EventCount} events", 
                typeof(TAggregate).Name, aggregate.Id, uncommittedEvents.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving aggregate {AggregateType} with ID {Id}", typeof(TAggregate).Name, aggregate.Id);
            throw;
        }
    }
    
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _eventStore.AggregateExistsAsync(id);
    }
}
```

### 4. Proyecciones y Read Models

```csharp
// Application/Projections/IOrderProjection.cs
public interface IOrderProjection
{
    Task<OrderReadModel> GetOrderAsync(Guid orderId);
    Task<IEnumerable<OrderReadModel>> GetCustomerOrdersAsync(Guid customerId);
    Task<OrderTimeline> GetOrderTimelineAsync(Guid orderId);
}

// Application/Projections/OrderProjection.cs
public class OrderProjection : IOrderProjection
{
    private readonly IEventStore _eventStore;
    private readonly ILogger<OrderProjection> _logger;
    
    public OrderProjection(IEventStore eventStore, ILogger<OrderProjection> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }
    
    public async Task<OrderReadModel> GetOrderAsync(Guid orderId)
    {
        try
        {
            var events = await _eventStore.GetEventsAsync(orderId);
            if (!events.Any())
                return null;
            
            var order = Order.Create(Guid.Empty, new List<OrderItem>(), "", "");
            order.LoadFromHistory(events);
            
            var readModel = new OrderReadModel
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                Items = order.Items.Select(i => new OrderItemReadModel
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Total = i.Total
                }).ToList(),
                Total = order.Total,
                Status = order.Status.ToString(),
                ShippingAddress = order.ShippingAddress,
                PaymentMethod = order.PaymentMethod,
                CreatedAt = order.CreatedAt,
                ConfirmedAt = order.ConfirmedAt,
                ShippedAt = order.ShippedAt,
                DeliveredAt = order.DeliveredAt
            };
            
            return readModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error projecting order {OrderId}", orderId);
            throw;
        }
    }
    
    public async Task<IEnumerable<OrderReadModel>> GetCustomerOrdersAsync(Guid customerId)
    {
        // Implementar búsqueda de órdenes por cliente
        // Esto requeriría un índice o una proyección separada
        throw new NotImplementedException();
    }
    
    public async Task<OrderTimeline> GetOrderTimelineAsync(Guid orderId)
    {
        try
        {
            var events = await _eventStore.GetEventsAsync(orderId);
            if (!events.Any())
                return null;
            
            var timeline = new OrderTimeline
            {
                OrderId = orderId,
                Events = events.Select(e => new TimelineEvent
                {
                    EventType = e.EventType,
                    Timestamp = e.Timestamp,
                    Data = GetEventData(e)
                }).ToList()
            };
            
            return timeline;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating timeline for order {OrderId}", orderId);
            throw;
        }
    }
    
    private object GetEventData(DomainEvent @event)
    {
        // Extraer datos específicos del evento para el timeline
        return @event switch
        {
            OrderCreatedEvent e => new { e.CustomerId, e.Items.Count, e.Total },
            OrderItemAddedEvent e => new { e.ProductId, e.Quantity, e.UnitPrice },
            OrderStatusChangedEvent e => new { e.OldStatus, e.NewStatus },
            OrderCancelledEvent e => new { e.Reason },
            _ => new { }
        };
    }
}

// Application/ReadModels/OrderReadModel.cs
public class OrderReadModel
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public List<OrderItemReadModel> Items { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; }
    public string ShippingAddress { get; set; }
    public string PaymentMethod { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}

public class OrderItemReadModel
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}

// Application/ReadModels/OrderTimeline.cs
public class OrderTimeline
{
    public Guid OrderId { get; set; }
    public List<TimelineEvent> Events { get; set; }
}

public class TimelineEvent
{
    public string EventType { get; set; }
    public DateTime Timestamp { get; set; }
    public object Data { get; set; }
}
```

### 5. Snapshots para Optimización

```csharp
// Infrastructure/EventStore/ISnapshotStore.cs
public interface ISnapshotStore
{
    Task SaveSnapshotAsync<T>(T aggregate) where T : EventSourcedAggregate;
    Task<T> GetSnapshotAsync<T>(Guid aggregateId) where T : EventSourcedAggregate;
    Task<bool> HasSnapshotAsync<T>(Guid aggregateId) where T : EventSourcedAggregate;
}

// Infrastructure/EventStore/SnapshotStore.cs
public class SnapshotStore : ISnapshotStore
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SnapshotStore> _logger;
    
    public SnapshotStore(ApplicationDbContext context, ILogger<SnapshotStore> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task SaveSnapshotAsync<T>(T aggregate) where T : EventSourcedAggregate
    {
        try
        {
            var snapshot = new SnapshotEntity
            {
                AggregateId = aggregate.Id,
                AggregateType = typeof(T).Name,
                Version = aggregate.Version,
                SnapshotData = JsonSerializer.Serialize(aggregate, typeof(T)),
                Timestamp = DateTime.UtcNow
            };
            
            // Eliminar snapshot anterior si existe
            var existingSnapshot = await _context.Snapshots
                .FirstOrDefaultAsync(s => s.AggregateId == aggregate.Id);
            
            if (existingSnapshot != null)
            {
                _context.Snapshots.Remove(existingSnapshot);
            }
            
            await _context.Snapshots.AddAsync(snapshot);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Saved snapshot for aggregate {AggregateType} with ID {Id} at version {Version}", 
                typeof(T).Name, aggregate.Id, aggregate.Version);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving snapshot for aggregate {AggregateType} with ID {Id}", 
                typeof(T).Name, aggregate.Id);
            throw;
        }
    }
    
    public async Task<T> GetSnapshotAsync<T>(Guid aggregateId) where T : EventSourcedAggregate
    {
        try
        {
            var snapshot = await _context.Snapshots
                .FirstOrDefaultAsync(s => s.AggregateId == aggregateId);
            
            if (snapshot == null)
                return null;
            
            var aggregate = JsonSerializer.Deserialize<T>(snapshot.SnapshotData);
            
            _logger.LogInformation("Retrieved snapshot for aggregate {AggregateType} with ID {Id} at version {Version}", 
                typeof(T).Name, aggregateId, snapshot.Version);
            
            return aggregate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving snapshot for aggregate {AggregateType} with ID {Id}", 
                typeof(T).Name, aggregateId);
            throw;
        }
    }
    
    public async Task<bool> HasSnapshotAsync<T>(Guid aggregateId) where T : EventSourcedAggregate
    {
        return await _context.Snapshots.AnyAsync(s => s.AggregateId == aggregateId);
    }
}

// Infrastructure/Data/SnapshotEntity.cs
public class SnapshotEntity
{
    public Guid Id { get; set; }
    public Guid AggregateId { get; set; }
    public string AggregateType { get; set; }
    public long Version { get; set; }
    public string SnapshotData { get; set; }
    public DateTime Timestamp { get; set; }
}
```

## Ejercicios Prácticos

### Ejercicio 1: Implementar Event Store
Crea un Event Store persistente usando Entity Framework.

### Ejercicio 2: Crear Agregados Event-Sourced
Implementa agregados para Customer y Product usando Event Sourcing.

### Ejercicio 3: Implementar Proyecciones
Crea proyecciones para diferentes vistas de los datos.

## Resumen

En esta clase hemos aprendido:

1. **Patrón Event Sourcing**: Almacenamiento de eventos en lugar de estado
2. **Event Store**: Persistencia y recuperación de eventos
3. **Agregados Event-Sourced**: Reconstrucción de estado desde eventos
4. **Proyecciones**: Creación de modelos de lectura optimizados
5. **Snapshots**: Optimización para agregados con muchos eventos

En la siguiente clase continuaremos con **CQRS Avanzado** para implementar consultas optimizadas y comandos robustos.

## Recursos Adicionales
- [Event Sourcing Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/event-sourcing)
- [Event Sourcing](https://martinfowler.com/eaaDev/EventSourcing.html)
- [CQRS with Event Sourcing](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/implement-event-sourcing)

