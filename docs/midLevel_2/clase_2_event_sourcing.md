# 🚀 Clase 2: Event Sourcing y CQRS Avanzado

## 📋 Información de la Clase

- **Módulo**: Mid Level 2 - Arquitectura de Software y Patrones Avanzados
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 1 (Arquitectura Hexagonal)

## 🎯 Objetivos de Aprendizaje

- Implementar Event Sourcing con CQRS
- Gestionar el estado de la aplicación a través de eventos
- Crear proyecciones de eventos
- Implementar snapshots y reconstrucción de estado
- Aplicar patrones de eventos avanzados

---

## 📚 Navegación del Módulo 5

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_arquitectura_hexagonal.md) | Arquitectura Hexagonal (Ports & Adapters) | ← Anterior |
| **Clase 2** | **Event Sourcing y CQRS Avanzado** | ← Estás aquí |
| [Clase 3](clase_3_microservicios.md) | Arquitectura de Microservicios | Siguiente → |
| [Clase 4](clase_4_patrones_arquitectonicos.md) | Patrones Arquitectónicos | |
| [Clase 5](clase_5_domain_driven_design.md) | Domain Driven Design (DDD) | |
| [Clase 6](clase_6_async_streams.md) | Async Streams y IAsyncEnumerable | |
| [Clase 7](clase_7_source_generators.md) | Source Generators y Compile-time Code Generation | |
| [Clase 8](clase_8_high_performance.md) | High Performance Programming | |
| [Clase 9](clase_9_seguridad_avanzada.md) | Seguridad Avanzada en .NET | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Gestión Empresarial | |

**← [Volver al README del Módulo 5](../midLevel_2/README.md)**

---

## 📚 Contenido Teórico

### 1. Introducción a Event Sourcing

Event Sourcing es un patrón que almacena todos los cambios en el estado de una aplicación como una secuencia de eventos, permitiendo reconstruir el estado en cualquier momento.

```csharp
// ===== EVENT SOURCING - IMPLEMENTACIÓN COMPLETA =====
namespace EventSourcingSystem
{
    // ===== DOMAIN EVENTS =====
    namespace Domain.Events
    {
        public abstract class DomainEvent : IDomainEvent
        {
            public Guid Id { get; private set; }
            public Guid AggregateId { get; private set; }
            public long Version { get; private set; }
            public DateTime OccurredOn { get; private set; }
            public string EventType { get; private set; }
            
            protected DomainEvent(Guid aggregateId, long version)
            {
                Id = Guid.NewGuid();
                AggregateId = aggregateId;
                Version = version;
                OccurredOn = DateTime.UtcNow;
                EventType = GetType().Name;
            }
        }
        
        // ===== USER EVENTS =====
        public class UserCreatedEvent : DomainEvent
        {
            public string Email { get; private set; }
            public string FirstName { get; private set; }
            public string LastName { get; private set; }
            
            public UserCreatedEvent(Guid aggregateId, long version, string email, string firstName, string lastName)
                : base(aggregateId, version)
            {
                Email = email;
                FirstName = firstName;
                LastName = lastName;
            }
        }
        
        public class UserProfileUpdatedEvent : DomainEvent
        {
            public string FirstName { get; private set; }
            public string LastName { get; private set; }
            
            public UserProfileUpdatedEvent(Guid aggregateId, long version, string firstName, string lastName)
                : base(aggregateId, version)
            {
                FirstName = firstName;
                LastName = lastName;
            }
        }
        
        public class UserDeactivatedEvent : DomainEvent
        {
            public string Reason { get; private set; }
            
            public UserDeactivatedEvent(Guid aggregateId, long version, string reason)
                : base(aggregateId, version)
            {
                Reason = reason;
            }
        }
        
        public class UserLoginRecordedEvent : DomainEvent
        {
            public DateTime LoginTime { get; private set; }
            public string IpAddress { get; private set; }
            
            public UserLoginRecordedEvent(Guid aggregateId, long version, DateTime loginTime, string ipAddress)
                : base(aggregateId, version)
            {
                LoginTime = loginTime;
                IpAddress = ipAddress;
            }
        }
        
        // ===== ORDER EVENTS =====
        public class OrderCreatedEvent : DomainEvent
        {
            public int UserId { get; private set; }
            public DateTime OrderDate { get; private set; }
            public List<OrderItemEvent> Items { get; private set; }
            
            public OrderCreatedEvent(Guid aggregateId, long version, int userId, DateTime orderDate, List<OrderItemEvent> items)
                : base(aggregateId, version)
            {
                UserId = userId;
                OrderDate = orderDate;
                Items = items;
            }
        }
        
        public class OrderItemAddedEvent : DomainEvent
        {
            public int ProductId { get; private set; }
            public int Quantity { get; private set; }
            public decimal UnitPrice { get; private set; }
            
            public OrderItemAddedEvent(Guid aggregateId, long version, int productId, int quantity, decimal unitPrice)
                : base(aggregateId, version)
            {
                ProductId = productId;
                Quantity = quantity;
                UnitPrice = unitPrice;
            }
        }
        
        public class OrderStatusChangedEvent : DomainEvent
        {
            public OrderStatus OldStatus { get; private set; }
            public OrderStatus NewStatus { get; private set; }
            public string Reason { get; private set; }
            
            public OrderStatusChangedEvent(Guid aggregateId, long version, OrderStatus oldStatus, OrderStatus newStatus, string reason)
                : base(aggregateId, version)
            {
                OldStatus = oldStatus;
                NewStatus = newStatus;
                Reason = reason;
            }
        }
        
        public class OrderCancelledEvent : DomainEvent
        {
            public string CancellationReason { get; private set; }
            public DateTime CancelledAt { get; private set; }
            
            public OrderCancelledEvent(Guid aggregateId, long version, string cancellationReason)
                : base(aggregateId, version)
            {
                CancellationReason = cancellationReason;
                CancelledAt = DateTime.UtcNow;
            }
        }
        
        // ===== ORDER ITEM EVENT =====
        public class OrderItemEvent
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
        }
        
        // ===== ENUMS =====
        public enum OrderStatus
        {
            Pending = 1,
            Confirmed = 2,
            Shipped = 3,
            Delivered = 4,
            Cancelled = 5
        }
    }
    
    // ===== AGGREGATES =====
    namespace Domain.Aggregates
    {
        public abstract class AggregateRoot : IAggregateRoot
        {
            private readonly List<IDomainEvent> _uncommittedEvents = new();
            private readonly List<IDomainEvent> _domainEvents = new();
            
            public Guid Id { get; protected set; }
            public long Version { get; protected set; }
            public bool IsDeleted { get; protected set; }
            
            public IReadOnlyCollection<IDomainEvent> UncommittedEvents => _uncommittedEvents.AsReadOnly();
            public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
            
            protected void AddDomainEvent(IDomainEvent domainEvent)
            {
                _uncommittedEvents.Add(domainEvent);
                _domainEvents.Add(domainEvent);
            }
            
            public void MarkEventsAsCommitted()
            {
                _uncommittedEvents.Clear();
            }
            
            public void LoadFromHistory(IEnumerable<IDomainEvent> history)
            {
                foreach (var @event in history)
                {
                    ApplyEvent(@event);
                    Version = @event.Version;
                }
            }
            
            protected abstract void ApplyEvent(IDomainEvent @event);
            
            protected void IncrementVersion()
            {
                Version++;
            }
        }
        
        public class User : AggregateRoot
        {
            public string Email { get; private set; }
            public string FirstName { get; private set; }
            public string LastName { get; private set; }
            public UserStatus Status { get; private set; }
            public DateTime CreatedAt { get; private set; }
            public DateTime? LastLoginAt { get; private set; }
            public List<UserLogin> LoginHistory { get; private set; }
            
            private User()
            {
                LoginHistory = new List<UserLogin>();
            }
            
            public User(string email, string firstName, string lastName)
            {
                Id = Guid.NewGuid();
                Email = email;
                FirstName = firstName;
                LastName = lastName;
                Status = UserStatus.Active;
                CreatedAt = DateTime.UtcNow;
                LoginHistory = new List<UserLogin>();
                
                AddDomainEvent(new UserCreatedEvent(Id, Version, email, firstName, lastName));
                IncrementVersion();
            }
            
            public void UpdateProfile(string firstName, string lastName)
            {
                if (Status != UserStatus.Active)
                    throw new InvalidOperationException("Cannot update profile of inactive user");
                
                FirstName = firstName;
                LastName = lastName;
                
                AddDomainEvent(new UserProfileUpdatedEvent(Id, Version, firstName, lastName));
                IncrementVersion();
            }
            
            public void Deactivate(string reason)
            {
                if (Status != UserStatus.Active)
                    throw new InvalidOperationException("User is already inactive");
                
                Status = UserStatus.Inactive;
                
                AddDomainEvent(new UserDeactivatedEvent(Id, Version, reason));
                IncrementVersion();
            }
            
            public void RecordLogin(string ipAddress)
            {
                if (Status != UserStatus.Active)
                    throw new InvalidOperationException("Cannot record login for inactive user");
                
                var loginTime = DateTime.UtcNow;
                LastLoginAt = loginTime;
                
                var login = new UserLogin(loginTime, ipAddress);
                LoginHistory.Add(login);
                
                AddDomainEvent(new UserLoginRecordedEvent(Id, Version, loginTime, ipAddress));
                IncrementVersion();
            }
            
            protected override void ApplyEvent(IDomainEvent @event)
            {
                switch (@event)
                {
                    case UserCreatedEvent e:
                        ApplyUserCreated(e);
                        break;
                    case UserProfileUpdatedEvent e:
                        ApplyUserProfileUpdated(e);
                        break;
                    case UserDeactivatedEvent e:
                        ApplyUserDeactivated(e);
                        break;
                    case UserLoginRecordedEvent e:
                        ApplyUserLoginRecorded(e);
                        break;
                }
            }
            
            private void ApplyUserCreated(UserCreatedEvent @event)
            {
                Id = @event.AggregateId;
                Email = @event.Email;
                FirstName = @event.FirstName;
                LastName = @event.LastName;
                Status = UserStatus.Active;
                CreatedAt = @event.OccurredOn;
            }
            
            private void ApplyUserProfileUpdated(UserProfileUpdatedEvent @event)
            {
                FirstName = @event.FirstName;
                LastName = @event.LastName;
            }
            
            private void ApplyUserDeactivated(UserDeactivatedEvent @event)
            {
                Status = UserStatus.Inactive;
            }
            
            private void ApplyUserLoginRecorded(UserLoginRecordedEvent @event)
            {
                LastLoginAt = @event.LoginTime;
                var login = new UserLogin(@event.LoginTime, @event.IpAddress);
                LoginHistory.Add(login);
            }
        }
        
        public class Order : AggregateRoot
        {
            public int UserId { get; private set; }
            public OrderStatus Status { get; private set; }
            public decimal TotalAmount { get; private set; }
            public DateTime OrderDate { get; private set; }
            public List<OrderItem> Items { get; private set; }
            public string CancellationReason { get; private set; }
            public DateTime? CancelledAt { get; private set; }
            
            private Order()
            {
                Items = new List<OrderItem>();
            }
            
            public Order(int userId)
            {
                Id = Guid.NewGuid();
                UserId = userId;
                Status = OrderStatus.Pending;
                OrderDate = DateTime.UtcNow;
                Items = new List<OrderItem>();
                TotalAmount = 0;
                
                AddDomainEvent(new OrderCreatedEvent(Id, Version, userId, OrderDate, new List<OrderItemEvent>()));
                IncrementVersion();
            }
            
            public void AddItem(int productId, int quantity, decimal unitPrice)
            {
                if (Status != OrderStatus.Pending)
                    throw new InvalidOperationException("Cannot add items to confirmed order");
                
                var item = new OrderItem(productId, quantity, unitPrice);
                Items.Add(item);
                CalculateTotal();
                
                AddDomainEvent(new OrderItemAddedEvent(Id, Version, productId, quantity, unitPrice));
                IncrementVersion();
            }
            
            public void Confirm()
            {
                if (Status != OrderStatus.Pending)
                    throw new InvalidOperationException("Order cannot be confirmed in current status");
                
                if (!Items.Any())
                    throw new InvalidOperationException("Cannot confirm order without items");
                
                var oldStatus = Status;
                Status = OrderStatus.Confirmed;
                
                AddDomainEvent(new OrderStatusChangedEvent(Id, Version, oldStatus, Status, "Order confirmed"));
                IncrementVersion();
            }
            
            public void Ship()
            {
                if (Status != OrderStatus.Confirmed)
                    throw new InvalidOperationException("Order cannot be shipped in current status");
                
                var oldStatus = Status;
                Status = OrderStatus.Shipped;
                
                AddDomainEvent(new OrderStatusChangedEvent(Id, Version, oldStatus, Status, "Order shipped"));
                IncrementVersion();
            }
            
            public void Deliver()
            {
                if (Status != OrderStatus.Shipped)
                    throw new InvalidOperationException("Order cannot be delivered in current status");
                
                var oldStatus = Status;
                Status = OrderStatus.Delivered;
                
                AddDomainEvent(new OrderStatusChangedEvent(Id, Version, oldStatus, Status, "Order delivered"));
                IncrementVersion();
            }
            
            public void Cancel(string reason)
            {
                if (Status == OrderStatus.Delivered)
                    throw new InvalidOperationException("Delivered orders cannot be cancelled");
                
                if (Status == OrderStatus.Cancelled)
                    throw new InvalidOperationException("Order is already cancelled");
                
                var oldStatus = Status;
                Status = OrderStatus.Cancelled;
                CancellationReason = reason;
                CancelledAt = DateTime.UtcNow;
                
                AddDomainEvent(new OrderCancelledEvent(Id, Version, reason));
                IncrementVersion();
            }
            
            private void CalculateTotal()
            {
                TotalAmount = Items.Sum(item => item.Quantity * item.UnitPrice);
            }
            
            protected override void ApplyEvent(IDomainEvent @event)
            {
                switch (@event)
                {
                    case OrderCreatedEvent e:
                        ApplyOrderCreated(e);
                        break;
                    case OrderItemAddedEvent e:
                        ApplyOrderItemAdded(e);
                        break;
                    case OrderStatusChangedEvent e:
                        ApplyOrderStatusChanged(e);
                        break;
                    case OrderCancelledEvent e:
                        ApplyOrderCancelled(e);
                        break;
                }
            }
            
            private void ApplyOrderCreated(OrderCreatedEvent @event)
            {
                Id = @event.AggregateId;
                UserId = @event.UserId;
                OrderDate = @event.OrderDate;
                Status = OrderStatus.Pending;
                Items = new List<OrderItem>();
                TotalAmount = 0;
            }
            
            private void ApplyOrderItemAdded(OrderItemAddedEvent @event)
            {
                var item = new OrderItem(@event.ProductId, @event.Quantity, @event.UnitPrice);
                Items.Add(item);
                CalculateTotal();
            }
            
            private void ApplyOrderStatusChanged(OrderStatusChangedEvent @event)
            {
                Status = @event.NewStatus;
            }
            
            private void ApplyOrderCancelled(OrderCancelledEvent @event)
            {
                Status = OrderStatus.Cancelled;
                CancellationReason = @event.CancellationReason;
                CancelledAt = @event.CancelledAt;
            }
        }
        
        // ===== VALUE OBJECTS =====
        public class UserLogin
        {
            public DateTime LoginTime { get; private set; }
            public string IpAddress { get; private set; }
            
            public UserLogin(DateTime loginTime, string ipAddress)
            {
                LoginTime = loginTime;
                IpAddress = ipAddress;
            }
        }
        
        public class OrderItem
        {
            public int ProductId { get; private set; }
            public int Quantity { get; private set; }
            public decimal UnitPrice { get; private set; }
            
            public OrderItem(int productId, int quantity, decimal unitPrice)
            {
                ProductId = productId;
                Quantity = quantity;
                UnitPrice = unitPrice;
            }
        }
        
        public enum UserStatus
        {
            Active = 1,
            Inactive = 2,
            Suspended = 3
        }
    }
    
    // ===== EVENT STORE =====
    namespace Infrastructure.EventStore
    {
        public interface IEventStore
        {
            Task SaveEventsAsync(Guid aggregateId, IEnumerable<IDomainEvent> events, long expectedVersion);
            Task<IEnumerable<IDomainEvent>> GetEventsAsync(Guid aggregateId);
            Task<IEnumerable<IDomainEvent>> GetEventsAsync(Guid aggregateId, long fromVersion);
            Task<IEnumerable<IDomainEvent>> GetAllEventsAsync();
            Task<IEnumerable<IDomainEvent>> GetEventsByTypeAsync(string eventType);
            Task<long> GetLastEventNumberAsync();
        }
        
        public class EventStore : IEventStore
        {
            private readonly ApplicationDbContext _context;
            private readonly ILogger<EventStore> _logger;
            private readonly IEventSerializer _eventSerializer;
            
            public EventStore(ApplicationDbContext context, ILogger<EventStore> logger, IEventSerializer eventSerializer)
            {
                _context = context;
                _logger = logger;
                _eventSerializer = eventSerializer;
            }
            
            public async Task SaveEventsAsync(Guid aggregateId, IEnumerable<IDomainEvent> events, long expectedVersion)
            {
                var eventList = events.ToList();
                if (!eventList.Any()) return;
                
                var lastEvent = await _context.Events
                    .Where(e => e.AggregateId == aggregateId)
                    .OrderByDescending(e => e.Version)
                    .FirstOrDefaultAsync();
                
                var lastVersion = lastEvent?.Version ?? 0;
                
                if (expectedVersion != lastVersion)
                {
                    throw new ConcurrencyException($"Expected version {expectedVersion} but found {lastVersion}");
                }
                
                var eventEntities = eventList.Select(@event => new EventEntity
                {
                    Id = @event.Id,
                    AggregateId = @event.AggregateId,
                    Version = @event.Version,
                    EventType = @event.EventType,
                    EventData = _eventSerializer.Serialize(@event),
                    OccurredOn = @event.OccurredOn,
                    CreatedAt = DateTime.UtcNow
                });
                
                _context.Events.AddRange(eventEntities);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Saved {EventCount} events for aggregate {AggregateId}", eventList.Count, aggregateId);
            }
            
            public async Task<IEnumerable<IDomainEvent>> GetEventsAsync(Guid aggregateId)
            {
                var events = await _context.Events
                    .Where(e => e.AggregateId == aggregateId)
                    .OrderBy(e => e.Version)
                    .ToListAsync();
                
                return events.Select(e => _eventSerializer.Deserialize(e.EventData, e.EventType));
            }
            
            public async Task<IEnumerable<IDomainEvent>> GetEventsAsync(Guid aggregateId, long fromVersion)
            {
                var events = await _context.Events
                    .Where(e => e.AggregateId == aggregateId && e.Version > fromVersion)
                    .OrderBy(e => e.Version)
                    .ToListAsync();
                
                return events.Select(e => _eventSerializer.Deserialize(e.EventData, e.EventType));
            }
            
            public async Task<IEnumerable<IDomainEvent>> GetAllEventsAsync()
            {
                var events = await _context.Events
                    .OrderBy(e => e.CreatedAt)
                    .ToListAsync();
                
                return events.Select(e => _eventSerializer.Deserialize(e.EventData, e.EventType));
            }
            
            public async Task<IEnumerable<IDomainEvent>> GetEventsByTypeAsync(string eventType)
            {
                var events = await _context.Events
                    .Where(e => e.EventType == eventType)
                    .OrderBy(e => e.CreatedAt)
                    .ToListAsync();
                
                return events.Select(e => _eventSerializer.Deserialize(e.EventData, e.EventType));
            }
            
            public async Task<long> GetLastEventNumberAsync()
            {
                var lastEvent = await _context.Events
                    .OrderByDescending(e => e.Id)
                    .FirstOrDefaultAsync();
                
                return lastEvent?.Id.GetHashCode() ?? 0;
            }
        }
        
        public class EventEntity
        {
            public Guid Id { get; set; }
            public Guid AggregateId { get; set; }
            public long Version { get; set; }
            public string EventType { get; set; }
            public string EventData { get; set; }
            public DateTime OccurredOn { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
    
    // ===== EVENT SERIALIZATION =====
    namespace Infrastructure.Serialization
    {
        public interface IEventSerializer
        {
            string Serialize(IDomainEvent @event);
            IDomainEvent Deserialize(string eventData, string eventType);
        }
        
        public class JsonEventSerializer : IEventSerializer
        {
            private readonly Dictionary<string, Type> _eventTypes;
            
            public JsonEventSerializer()
            {
                _eventTypes = new Dictionary<string, Type>
                {
                    { "UserCreatedEvent", typeof(UserCreatedEvent) },
                    { "UserProfileUpdatedEvent", typeof(UserProfileUpdatedEvent) },
                    { "UserDeactivatedEvent", typeof(UserDeactivatedEvent) },
                    { "UserLoginRecordedEvent", typeof(UserLoginRecordedEvent) },
                    { "OrderCreatedEvent", typeof(OrderCreatedEvent) },
                    { "OrderItemAddedEvent", typeof(OrderItemAddedEvent) },
                    { "OrderStatusChangedEvent", typeof(OrderStatusChangedEvent) },
                    { "OrderCancelledEvent", typeof(OrderCancelledEvent) }
                };
            }
            
            public string Serialize(IDomainEvent @event)
            {
                return JsonSerializer.Serialize(@event, @event.GetType());
            }
            
            public IDomainEvent Deserialize(string eventData, string eventType)
            {
                if (!_eventTypes.TryGetValue(eventType, out var type))
                {
                    throw new InvalidOperationException($"Unknown event type: {eventType}");
                }
                
                return (IDomainEvent)JsonSerializer.Deserialize(eventData, type);
            }
        }
    }
    
    // ===== REPOSITORIES =====
    namespace Infrastructure.Repositories
    {
        public interface IEventSourcedRepository<T> where T : AggregateRoot
        {
            Task<T> GetByIdAsync(Guid id);
            Task SaveAsync(T aggregate);
            Task<bool> ExistsAsync(Guid id);
        }
        
        public class EventSourcedRepository<T> : IEventSourcedRepository<T> where T : AggregateRoot
        {
            private readonly IEventStore _eventStore;
            private readonly ILogger<EventSourcedRepository<T>> _logger;
            private readonly Func<T> _aggregateFactory;
            
            public EventSourcedRepository(IEventStore eventStore, ILogger<EventSourcedRepository<T>> logger, Func<T> aggregateFactory)
            {
                _eventStore = eventStore;
                _logger = logger;
                _aggregateFactory = aggregateFactory;
            }
            
            public async Task<T> GetByIdAsync(Guid id)
            {
                var events = await _eventStore.GetEventsAsync(id);
                if (!events.Any())
                    return null;
                
                var aggregate = _aggregateFactory();
                aggregate.LoadFromHistory(events);
                
                return aggregate;
            }
            
            public async Task SaveAsync(T aggregate)
            {
                var uncommittedEvents = aggregate.UncommittedEvents.ToList();
                if (!uncommittedEvents.Any()) return;
                
                await _eventStore.SaveEventsAsync(aggregate.Id, uncommittedEvents, aggregate.Version - uncommittedEvents.Count);
                aggregate.MarkEventsAsCommitted();
                
                _logger.LogInformation("Saved aggregate {AggregateType} with ID {AggregateId}", typeof(T).Name, aggregate.Id);
            }
            
            public async Task<bool> ExistsAsync(Guid id)
            {
                var events = await _eventStore.GetEventsAsync(id);
                return events.Any();
            }
        }
    }
    
    // ===== PROJECTIONS =====
    namespace Infrastructure.Projections
    {
        public interface IProjection
        {
            Task HandleAsync(IDomainEvent @event);
        }
        
        public abstract class ProjectionBase : IProjection
        {
            protected readonly ApplicationDbContext _context;
            protected readonly ILogger _logger;
            
            protected ProjectionBase(ApplicationDbContext context, ILogger logger)
            {
                _context = context;
                _logger = logger;
            }
            
            public abstract Task HandleAsync(IDomainEvent @event);
            
            protected async Task SaveChangesAsync()
            {
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving projection changes");
                    throw;
                }
            }
        }
        
        public class UserProjection : ProjectionBase
        {
            public UserProjection(ApplicationDbContext context, ILogger<UserProjection> logger)
                : base(context, logger)
            {
            }
            
            public override async Task HandleAsync(IDomainEvent @event)
            {
                switch (@event)
                {
                    case UserCreatedEvent e:
                        await HandleUserCreated(e);
                        break;
                    case UserProfileUpdatedEvent e:
                        await HandleUserProfileUpdated(e);
                        break;
                    case UserDeactivatedEvent e:
                        await HandleUserDeactivated(e);
                        break;
                    case UserLoginRecordedEvent e:
                        await HandleUserLoginRecorded(e);
                        break;
                }
            }
            
            private async Task HandleUserCreated(UserCreatedEvent @event)
            {
                var user = new UserReadModel
                {
                    Id = @event.AggregateId,
                    Email = @event.Email,
                    FirstName = @event.FirstName,
                    LastName = @event.LastName,
                    Status = UserStatus.Active,
                    CreatedAt = @event.OccurredOn,
                    LastLoginAt = null
                };
                
                _context.Users.Add(user);
                await SaveChangesAsync();
                
                _logger.LogInformation("Created user projection for {UserId}", @event.AggregateId);
            }
            
            private async Task HandleUserProfileUpdated(UserProfileUpdatedEvent @event)
            {
                var user = await _context.Users.FindAsync(@event.AggregateId);
                if (user != null)
                {
                    user.FirstName = @event.FirstName;
                    user.LastName = @event.LastName;
                    await SaveChangesAsync();
                    
                    _logger.LogInformation("Updated user projection for {UserId}", @event.AggregateId);
                }
            }
            
            private async Task HandleUserDeactivated(UserDeactivatedEvent @event)
            {
                var user = await _context.Users.FindAsync(@event.AggregateId);
                if (user != null)
                {
                    user.Status = UserStatus.Inactive;
                    await SaveChangesAsync();
                    
                    _logger.LogInformation("Deactivated user projection for {UserId}", @event.AggregateId);
                }
            }
            
            private async Task HandleUserLoginRecorded(UserLoginRecordedEvent @event)
            {
                var user = await _context.Users.FindAsync(@event.AggregateId);
                if (user != null)
                {
                    user.LastLoginAt = @event.LoginTime;
                    await SaveChangesAsync();
                    
                    _logger.LogInformation("Updated user login projection for {UserId}", @event.AggregateId);
                }
            }
        }
        
        public class OrderProjection : ProjectionBase
        {
            public OrderProjection(ApplicationDbContext context, ILogger<OrderProjection> logger)
                : base(context, logger)
            {
            }
            
            public override async Task HandleAsync(IDomainEvent @event)
            {
                switch (@event)
                {
                    case OrderCreatedEvent e:
                        await HandleOrderCreated(e);
                        break;
                    case OrderItemAddedEvent e:
                        await HandleOrderItemAdded(e);
                        break;
                    case OrderStatusChangedEvent e:
                        await HandleOrderStatusChanged(e);
                        break;
                    case OrderCancelledEvent e:
                        await HandleOrderCancelled(e);
                        break;
                }
            }
            
            private async Task HandleOrderCreated(OrderCreatedEvent @event)
            {
                var order = new OrderReadModel
                {
                    Id = @event.AggregateId,
                    UserId = @event.UserId,
                    Status = OrderStatus.Pending,
                    TotalAmount = 0,
                    OrderDate = @event.OrderDate,
                    CancellationReason = null,
                    CancelledAt = null
                };
                
                _context.Orders.Add(order);
                await SaveChangesAsync();
                
                _logger.LogInformation("Created order projection for {OrderId}", @event.AggregateId);
            }
            
            private async Task HandleOrderItemAdded(OrderItemAddedEvent @event)
            {
                var order = await _context.Orders.FindAsync(@event.AggregateId);
                if (order != null)
                {
                    var item = new OrderItemReadModel
                    {
                        OrderId = @event.AggregateId,
                        ProductId = @event.ProductId,
                        Quantity = @event.Quantity,
                        UnitPrice = @event.UnitPrice
                    };
                    
                    _context.OrderItems.Add(item);
                    
                    // Recalculate total
                    var total = await _context.OrderItems
                        .Where(oi => oi.OrderId == @event.AggregateId)
                        .SumAsync(oi => oi.Quantity * oi.UnitPrice);
                    
                    order.TotalAmount = total;
                    await SaveChangesAsync();
                    
                    _logger.LogInformation("Added item to order projection for {OrderId}", @event.AggregateId);
                }
            }
            
            private async Task HandleOrderStatusChanged(OrderStatusChangedEvent @event)
            {
                var order = await _context.Orders.FindAsync(@event.AggregateId);
                if (order != null)
                {
                    order.Status = @event.NewStatus;
                    await SaveChangesAsync();
                    
                    _logger.LogInformation("Updated order status projection for {OrderId}", @event.AggregateId);
                }
            }
            
            private async Task HandleOrderCancelled(OrderCancelledEvent @event)
            {
                var order = await _context.Orders.FindAsync(@event.AggregateId);
                if (order != null)
                {
                    order.Status = OrderStatus.Cancelled;
                    order.CancellationReason = @event.CancellationReason;
                    order.CancelledAt = @event.CancelledAt;
                    await SaveChangesAsync();
                    
                    _logger.LogInformation("Cancelled order projection for {OrderId}", @event.AggregateId);
                }
            }
        }
    }
    
    // ===== READ MODELS =====
    namespace Infrastructure.ReadModels
    {
        public class UserReadModel
        {
            public Guid Id { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public UserStatus Status { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? LastLoginAt { get; set; }
        }
        
        public class OrderReadModel
        {
            public Guid Id { get; set; }
            public int UserId { get; set; }
            public OrderStatus Status { get; set; }
            public decimal TotalAmount { get; set; }
            public DateTime OrderDate { get; set; }
            public string CancellationReason { get; set; }
            public DateTime? CancelledAt { get; set; }
        }
        
        public class OrderItemReadModel
        {
            public Guid Id { get; set; }
            public Guid OrderId { get; set; }
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
        }
    }
    
    // ===== EVENT HANDLERS =====
    namespace Application.EventHandlers
    {
        public interface IEventHandler<TEvent> where TEvent : IDomainEvent
        {
            Task HandleAsync(TEvent @event);
        }
        
        public class UserEventHandler : IEventHandler<UserCreatedEvent>, IEventHandler<UserProfileUpdatedEvent>, IEventHandler<UserDeactivatedEvent>, IEventHandler<UserLoginRecordedEvent>
        {
            private readonly UserProjection _userProjection;
            private readonly ILogger<UserEventHandler> _logger;
            
            public UserEventHandler(UserProjection userProjection, ILogger<UserEventHandler> logger)
            {
                _userProjection = userProjection;
                _logger = logger;
            }
            
            public async Task HandleAsync(UserCreatedEvent @event)
            {
                await _userProjection.HandleAsync(@event);
            }
            
            public async Task HandleAsync(UserProfileUpdatedEvent @event)
            {
                await _userProjection.HandleAsync(@event);
            }
            
            public async Task HandleAsync(UserDeactivatedEvent @event)
            {
                await _userProjection.HandleAsync(@event);
            }
            
            public async Task HandleAsync(UserLoginRecordedEvent @event)
            {
                await _userProjection.HandleAsync(@event);
            }
        }
        
        public class OrderEventHandler : IEventHandler<OrderCreatedEvent>, IEventHandler<OrderItemAddedEvent>, IEventHandler<OrderStatusChangedEvent>, IEventHandler<OrderCancelledEvent>
        {
            private readonly OrderProjection _orderProjection;
            private readonly ILogger<OrderEventHandler> _logger;
            
            public OrderEventHandler(OrderProjection orderProjection, ILogger<OrderEventHandler> logger)
            {
                _orderProjection = orderProjection;
                _logger = logger;
            }
            
            public async Task HandleAsync(OrderCreatedEvent @event)
            {
                await _orderProjection.HandleAsync(@event);
            }
            
            public async Task HandleAsync(OrderItemAddedEvent @event)
            {
                await _orderProjection.HandleAsync(@event);
            }
            
            public async Task HandleAsync(OrderStatusChangedEvent @event)
            {
                await _orderProjection.HandleAsync(@event);
            }
            
            public async Task HandleAsync(OrderCancelledEvent @event)
            {
                await _orderProjection.HandleAsync(@event);
            }
        }
    }
    
    // ===== SNAPSHOTS =====
    namespace Infrastructure.Snapshots
    {
        public interface ISnapshotStore
        {
            Task SaveSnapshotAsync<T>(T aggregate) where T : AggregateRoot;
            Task<T> GetSnapshotAsync<T>(Guid aggregateId) where T : AggregateRoot;
            Task<bool> HasSnapshotAsync<T>(Guid aggregateId) where T : AggregateRoot;
        }
        
        public class SnapshotStore : ISnapshotStore
        {
            private readonly ApplicationDbContext _context;
            private readonly ILogger<SnapshotStore> _logger;
            private readonly IEventSerializer _eventSerializer;
            
            public SnapshotStore(ApplicationDbContext context, ILogger<SnapshotStore> logger, IEventSerializer eventSerializer)
            {
                _context = context;
                _logger = logger;
                _eventSerializer = eventSerializer;
            }
            
            public async Task SaveSnapshotAsync<T>(T aggregate) where T : AggregateRoot
            {
                var snapshot = new SnapshotEntity
                {
                    AggregateId = aggregate.Id,
                    AggregateType = typeof(T).Name,
                    Version = aggregate.Version,
                    SnapshotData = _eventSerializer.Serialize(aggregate),
                    CreatedAt = DateTime.UtcNow
                };
                
                var existingSnapshot = await _context.Snapshots
                    .FirstOrDefaultAsync(s => s.AggregateId == aggregate.Id);
                
                if (existingSnapshot != null)
                {
                    existingSnapshot.Version = aggregate.Version;
                    existingSnapshot.SnapshotData = snapshot.SnapshotData;
                    existingSnapshot.CreatedAt = DateTime.UtcNow;
                }
                else
                {
                    _context.Snapshots.Add(snapshot);
                }
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Saved snapshot for {AggregateType} {AggregateId} at version {Version}", 
                    typeof(T).Name, aggregate.Id, aggregate.Version);
            }
            
            public async Task<T> GetSnapshotAsync<T>(Guid aggregateId) where T : AggregateRoot
            {
                var snapshot = await _context.Snapshots
                    .FirstOrDefaultAsync(s => s.AggregateId == aggregateId);
                
                if (snapshot == null)
                    return null;
                
                // Deserialize snapshot and return
                // This is a simplified implementation
                return null;
            }
            
            public async Task<bool> HasSnapshotAsync<T>(Guid aggregateId) where T : AggregateRoot
            {
                return await _context.Snapshots.AnyAsync(s => s.AggregateId == aggregateId);
            }
        }
        
        public class SnapshotEntity
        {
            public Guid Id { get; set; }
            public Guid AggregateId { get; set; }
            public string AggregateType { get; set; }
            public long Version { get; set; }
            public string SnapshotData { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}

// ===== INTERFACES =====
namespace EventSourcingSystem.Interfaces
{
    public interface IDomainEvent
    {
        Guid Id { get; }
        Guid AggregateId { get; }
        long Version { get; }
        DateTime OccurredOn { get; }
        string EventType { get; }
    }
    
    public interface IAggregateRoot
    {
        Guid Id { get; }
        long Version { get; }
        IReadOnlyCollection<IDomainEvent> UncommittedEvents { get; }
        void MarkEventsAsCommitted();
        void LoadFromHistory(IEnumerable<IDomainEvent> history);
    }
}

// ===== EXCEPCIONES =====
namespace EventSourcingSystem.Exceptions
{
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(string message) : base(message) { }
    }
}

// Uso de Event Sourcing
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Event Sourcing y CQRS Avanzado ===\n");
        
        Console.WriteLine("Event Sourcing proporciona:");
        Console.WriteLine("1. Auditoría completa de todos los cambios");
        Console.WriteLine("2. Capacidad de reconstruir estado en cualquier momento");
        Console.WriteLine("3. Separación de comandos (write) y consultas (read)");
        Console.WriteLine("4. Escalabilidad horizontal para lecturas");
        Console.WriteLine("5. Capacidad de viajar en el tiempo del estado");
        
        Console.WriteLine("\nBeneficios principales:");
        Console.WriteLine("- Trazabilidad completa de cambios");
        Console.WriteLine("- Desacoplamiento de operaciones de lectura y escritura");
        Console.WriteLine("- Escalabilidad mejorada");
        Console.WriteLine("- Capacidad de análisis histórico");
        Console.WriteLine("- Recuperación de errores");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Implementación de Event Sourcing
Implementa un sistema de gestión de inventario usando Event Sourcing, incluyendo eventos para cambios de stock.

### Ejercicio 2: Proyecciones Avanzadas
Crea múltiples proyecciones para el mismo agregado (ej: vista de usuario para admin, vista para usuario, vista para auditoría).

### Ejercicio 3: Snapshots y Optimización
Implementa un sistema de snapshots que permita reconstruir el estado rápidamente sin procesar todos los eventos.

## 🔍 Puntos Clave

1. **Event Sourcing** almacena todos los cambios como eventos
2. **CQRS** separa operaciones de lectura y escritura
3. **Proyecciones** crean vistas optimizadas para consultas
4. **Snapshots** optimizan la reconstrucción de estado
5. **Event Handlers** procesan eventos para mantener proyecciones actualizadas

## 📚 Recursos Adicionales

- [Event Sourcing - Martin Fowler](https://martinfowler.com/eaaDev/EventSourcing.html)
- [CQRS - Greg Young](https://cqrs.files.wordpress.com/2010/11/cqrs_documents.pdf)
- [Event Store Documentation](https://eventstore.com/docs/)

---

**🎯 ¡Has completado la Clase 2! Ahora comprendes Event Sourcing y CQRS Avanzado en C#**

**📚 [Siguiente: Clase 3 - Arquitectura de Microservicios](clase_3_microservicios.md)**
