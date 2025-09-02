# Clase 3: Domain Events

## Navegación
- [← Clase anterior: CQRS](clase_2_cqrs.md)
- [← Volver al README del módulo](README.md)
- [→ Siguiente clase: Microservicios](clase_4_microservicios.md)

## Objetivos de Aprendizaje
- Comprender el concepto de Domain Events
- Implementar eventos de dominio en entidades
- Crear event handlers para procesar eventos
- Aplicar Domain Events en Clean Architecture

## ¿Qué son los Domain Events?

Los **Domain Events** son objetos que representan algo que sucedió en el dominio de la aplicación. Son una forma de comunicar cambios importantes entre diferentes partes del sistema de manera desacoplada, permitiendo que múltiples componentes reaccionen a estos eventos sin conocer los detalles de implementación de los demás.

### Beneficios de los Domain Events

```csharp
// 1. Desacoplamiento
// Las entidades no necesitan conocer los servicios externos
public class Order
{
    public void Confirm()
    {
        Status = OrderStatus.Confirmed;
        // Solo agregar el evento, no llamar directamente a servicios
        AddDomainEvent(new OrderConfirmedEvent(Id, CustomerId));
    }
}

// 2. Extensibilidad
// Nuevos handlers pueden ser agregados sin modificar las entidades
public class OrderConfirmedEventHandler : IDomainEventHandler<OrderConfirmedEvent>
{
    public async Task HandleAsync(OrderConfirmedEvent @event)
    {
        // Enviar email, actualizar inventario, etc.
    }
}

// 3. Testabilidad
// Los eventos pueden ser verificados en las pruebas unitarias
[Test]
public void ConfirmOrder_ShouldRaiseOrderConfirmedEvent()
{
    var order = Order.Create(customerId, items);
    order.Confirm();
    
    Assert.That(order.DomainEvents, Has.Some.Matches<DomainEvent>(
        e => e is OrderConfirmedEvent));
}

// 4. Auditoría y Trazabilidad
// Todos los cambios importantes quedan registrados
public class OrderAuditEventHandler : IDomainEventHandler<OrderConfirmedEvent>
{
    public async Task HandleAsync(OrderConfirmedEvent @event)
    {
        await _auditService.LogAsync("Order confirmed", @event);
    }
}
```

## Implementación de Domain Events

### 1. Base de Eventos de Dominio

```csharp
// Domain/Events/DomainEvent.cs
public abstract class DomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public string EventType { get; }
    
    protected DomainEvent()
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        EventType = GetType().Name;
    }
}

// Domain/Events/OrderEvents.cs
public class OrderCreatedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid CustomerId { get; }
    public decimal Total { get; }
    public DateTime CreatedAt { get; }
    
    public OrderCreatedEvent(Guid orderId, Guid customerId, decimal total, DateTime createdAt)
    {
        OrderId = orderId;
        CustomerId = customerId;
        Total = total;
        CreatedAt = createdAt;
    }
}

public class OrderConfirmedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid CustomerId { get; }
    public DateTime ConfirmedAt { get; }
    
    public OrderConfirmedEvent(Guid orderId, Guid customerId, DateTime confirmedAt)
    {
        OrderId = orderId;
        CustomerId = customerId;
        ConfirmedAt = confirmedAt;
    }
}

public class OrderStatusChangedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public OrderStatus OldStatus { get; }
    public OrderStatus NewStatus { get; }
    public DateTime ChangedAt { get; }
    
    public OrderStatusChangedEvent(Guid orderId, OrderStatus oldStatus, OrderStatus newStatus, DateTime changedAt)
    {
        OrderId = orderId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        ChangedAt = changedAt;
    }
}

public class OrderItemAddedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid ProductId { get; }
    public int Quantity { get; }
    public decimal UnitPrice { get; }
    
    public OrderItemAddedEvent(Guid orderId, Guid productId, int quantity, decimal unitPrice)
    {
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}

public class OrderItemRemovedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid ProductId { get; }
    
    public OrderItemRemovedEvent(Guid orderId, Guid productId)
    {
        OrderId = orderId;
        ProductId = productId;
    }
}

// Domain/Events/CustomerEvents.cs
public class CustomerCreatedEvent : DomainEvent
{
    public Guid CustomerId { get; }
    public string Name { get; }
    public string Email { get; }
    public DateTime CreatedAt { get; }
    
    public CustomerCreatedEvent(Guid customerId, string name, string email, DateTime createdAt)
    {
        CustomerId = customerId;
        Name = name;
        Email = email;
        CreatedAt = createdAt;
    }
}

public class CustomerProfileUpdatedEvent : DomainEvent
{
    public Guid CustomerId { get; }
    public string OldName { get; }
    public string NewName { get; }
    public string OldEmail { get; }
    public string NewEmail { get; }
    public DateTime UpdatedAt { get; }
    
    public CustomerProfileUpdatedEvent(Guid customerId, string oldName, string newName, string oldEmail, string newEmail, DateTime updatedAt)
    {
        CustomerId = customerId;
        OldName = oldName;
        NewName = newName;
        OldEmail = oldEmail;
        NewEmail = newEmail;
        UpdatedAt = updatedAt;
    }
}

public class CustomerOrderRecordedEvent : DomainEvent
{
    public Guid CustomerId { get; }
    public decimal OrderTotal { get; }
    public DateTime RecordedAt { get; }
    
    public CustomerOrderRecordedEvent(Guid customerId, decimal orderTotal, DateTime recordedAt)
    {
        CustomerId = customerId;
        OrderTotal = orderTotal;
        RecordedAt = recordedAt;
    }
}

// Domain/Events/ProductEvents.cs
public class ProductCreatedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public string Name { get; }
    public decimal Price { get; }
    public int InitialStock { get; }
    public DateTime CreatedAt { get; }
    
    public ProductCreatedEvent(Guid productId, string name, decimal price, int initialStock, DateTime createdAt)
    {
        ProductId = productId;
        Name = name;
        Price = price;
        InitialStock = initialStock;
        CreatedAt = createdAt;
    }
}

public class ProductStockUpdatedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public int OldStock { get; }
    public int NewStock { get; }
    public string Reason { get; }
    public DateTime UpdatedAt { get; }
    
    public ProductStockUpdatedEvent(Guid productId, int oldStock, int newStock, string reason, DateTime updatedAt)
    {
        ProductId = productId;
        OldStock = oldStock;
        NewStock = newStock;
        Reason = reason;
        UpdatedAt = updatedAt;
    }
}
```

### 2. Entidades con Soporte para Eventos

```csharp
// Domain/Entities/Order.cs (actualizado con eventos)
public class Order : Entity
{
    public Guid Id { get; private set; }
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
    
    public static Order Create(Guid customerId, List<OrderItem> items)
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
            CreatedAt = DateTime.UtcNow
        };
        
        // Agregar evento de dominio
        order.AddDomainEvent(new OrderCreatedEvent(order.Id, customerId, order.Total, order.CreatedAt));
        
        return order;
    }
    
    public void SetShippingAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new DomainException("Shipping address is required");
            
        ShippingAddress = address.Trim();
    }
    
    public void SetPaymentMethod(string paymentMethod)
    {
        if (string.IsNullOrWhiteSpace(paymentMethod))
            throw new DomainException("Payment method is required");
            
        PaymentMethod = paymentMethod.Trim();
    }
    
    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Only pending orders can be confirmed");
            
        var oldStatus = Status;
        Status = OrderStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
        
        // Agregar evento de dominio
        AddDomainEvent(new OrderStatusChangedEvent(Id, oldStatus, Status, ConfirmedAt.Value));
        AddDomainEvent(new OrderConfirmedEvent(Id, CustomerId, ConfirmedAt.Value));
    }
    
    public void Ship()
    {
        if (Status != OrderStatus.Confirmed)
            throw new DomainException("Only confirmed orders can be shipped");
            
        var oldStatus = Status;
        Status = OrderStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
        
        // Agregar evento de dominio
        AddDomainEvent(new OrderStatusChangedEvent(Id, oldStatus, Status, ShippedAt.Value));
    }
    
    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new DomainException("Only shipped orders can be delivered");
            
        var oldStatus = Status;
        Status = OrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        
        // Agregar evento de dominio
        AddDomainEvent(new OrderStatusChangedEvent(Id, oldStatus, Status, DeliveredAt.Value));
    }
    
    public void Cancel()
    {
        if (Status == OrderStatus.Delivered)
            throw new DomainException("Delivered orders cannot be cancelled");
            
        var oldStatus = Status;
        Status = OrderStatus.Cancelled;
        
        // Agregar evento de dominio
        AddDomainEvent(new OrderStatusChangedEvent(Id, oldStatus, Status, DateTime.UtcNow));
    }
    
    public void AddItem(OrderItem item)
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Cannot add items to non-pending orders");
            
        if (item == null)
            throw new DomainException("Item cannot be null");
            
        Items.Add(item);
        
        // Agregar evento de dominio
        AddDomainEvent(new OrderItemAddedEvent(Id, item.ProductId, item.Quantity, item.UnitPrice));
    }
    
    public void RemoveItem(Guid productId)
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Cannot remove items from non-pending orders");
            
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new DomainException("Item not found in order");
            
        Items.Remove(item);
        
        // Agregar evento de dominio
        AddDomainEvent(new OrderItemRemovedEvent(Id, productId));
    }
    
    protected override void Validate()
    {
        if (CustomerId == Guid.Empty)
            throw new DomainException("Customer ID is required");
            
        if (Items == null || !Items.Any())
            throw new DomainException("Order must have at least one item");
            
        if (string.IsNullOrWhiteSpace(ShippingAddress))
            throw new DomainException("Shipping address is required");
            
        if (string.IsNullOrWhiteSpace(PaymentMethod))
            throw new DomainException("Payment method is required");
    }
}

// Domain/Entities/Customer.cs (actualizado con eventos)
public class Customer : Entity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public CustomerStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastOrderDate { get; private set; }
    public int TotalOrders { get; private set; }
    public decimal TotalSpent { get; private set; }
    
    private Customer() { }
    
    public static Customer Create(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required");
            
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required");
            
        if (!IsValidEmail(email))
            throw new DomainException("Invalid email format");
            
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Email = email.Trim().ToLower(),
            Status = CustomerStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        
        // Agregar evento de dominio
        customer.AddDomainEvent(new CustomerCreatedEvent(customer.Id, customer.Name, customer.Email, customer.CreatedAt));
        
        return customer;
    }
    
    public void UpdateProfile(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required");
            
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required");
            
        if (!IsValidEmail(email))
            throw new DomainException("Invalid email format");
            
        var oldName = Name;
        var oldEmail = Email;
        
        Name = name.Trim();
        Email = email.Trim().ToLower();
        
        // Agregar evento de dominio
        AddDomainEvent(new CustomerProfileUpdatedEvent(Id, oldName, oldEmail, Name, Email, DateTime.UtcNow));
    }
    
    public void Deactivate()
    {
        if (Status == CustomerStatus.Inactive)
            throw new DomainException("Customer is already inactive");
            
        Status = CustomerStatus.Inactive;
        
        // Agregar evento de dominio
        AddDomainEvent(new CustomerDeactivatedEvent(Id));
    }
    
    public void RecordOrder(decimal orderTotal)
    {
        TotalOrders++;
        TotalSpent += orderTotal;
        LastOrderDate = DateTime.UtcNow;
        
        // Agregar evento de dominio
        AddDomainEvent(new CustomerOrderRecordedEvent(Id, orderTotal, DateTime.UtcNow));
    }
    
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    
    protected override void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new DomainException("Name is required");
            
        if (string.IsNullOrWhiteSpace(Email))
            throw new DomainException("Email is required");
            
        if (!IsValidEmail(Email))
            throw new DomainException("Invalid email format");
    }
}
```

### 3. Event Handlers

Los Event Handlers procesan los eventos de dominio y ejecutan la lógica correspondiente.

```csharp
// Application/EventHandlers/IEventHandler.cs
public interface IDomainEventHandler<in TEvent> where TEvent : DomainEvent
{
    Task HandleAsync(TEvent @event);
}

// Application/EventHandlers/OrderEventHandlers.cs
public class OrderCreatedEventHandler : IDomainEventHandler<OrderCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<OrderCreatedEventHandler> _logger;
    
    public OrderCreatedEventHandler(
        IEmailService emailService,
        IInventoryService inventoryService,
        ILogger<OrderCreatedEventHandler> logger)
    {
        _emailService = emailService;
        _inventoryService = inventoryService;
        _logger = logger;
    }
    
    public async Task HandleAsync(OrderCreatedEvent @event)
    {
        try
        {
            _logger.LogInformation("Handling OrderCreatedEvent for order {@OrderId}", @event.OrderId);
            
            // Enviar email de confirmación
            await _emailService.SendOrderConfirmationAsync(@event.CustomerId.ToString(), @event.OrderId);
            
            // Actualizar inventario
            await _inventoryService.ReserveStockForOrderAsync(@event.OrderId);
            
            // Aquí podrías agregar más lógica como:
            // - Generar factura
            // - Notificar a sistemas externos
            // - Actualizar analytics
            // - Enviar notificaciones push
            
            _logger.LogInformation("Successfully handled OrderCreatedEvent for order {@OrderId}", @event.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling OrderCreatedEvent for order {@OrderId}", @event.OrderId);
            throw;
        }
    }
}

public class OrderConfirmedEventHandler : IDomainEventHandler<OrderConfirmedEvent>
{
    private readonly IEmailService _emailService;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<OrderConfirmedEventHandler> _logger;
    
    public OrderConfirmedEventHandler(
        IEmailService emailService,
        IPaymentService paymentService,
        ILogger<OrderConfirmedEventHandler> logger)
    {
        _emailService = emailService;
        _paymentService = paymentService;
        _logger = logger;
    }
    
    public async Task HandleAsync(OrderConfirmedEvent @event)
    {
        try
        {
            _logger.LogInformation("Handling OrderConfirmedEvent for order {@OrderId}", @event.OrderId);
            
            // Enviar email de confirmación
            await _emailService.SendOrderConfirmedAsync(@event.CustomerId.ToString(), @event.OrderId);
            
            // Procesar pago
            await _paymentService.ProcessPaymentAsync(@event.OrderId);
            
            // Notificar al departamento de logística
            await _logisticsService.NotifyOrderReadyForShippingAsync(@event.OrderId);
            
            _logger.LogInformation("Successfully handled OrderConfirmedEvent for order {@OrderId}", @event.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling OrderConfirmedEvent for order {@OrderId}", @event.OrderId);
            throw;
        }
    }
}

public class OrderStatusChangedEventHandler : IDomainEventHandler<OrderStatusChangedEvent>
{
    private readonly IEmailService _emailService;
    private readonly IAuditService _auditService;
    private readonly ILogger<OrderStatusChangedEventHandler> _logger;
    
    public OrderStatusChangedEventHandler(
        IEmailService emailService,
        IAuditService auditService,
        ILogger<OrderStatusChangedEventHandler> logger)
    {
        _emailService = emailService;
        _auditService = auditService;
        _logger = logger;
    }
    
    public async Task HandleAsync(OrderStatusChangedEvent @event)
    {
        try
        {
            _logger.LogInformation("Handling OrderStatusChangedEvent for order {@OrderId}: {@OldStatus} -> {@NewStatus}", 
                @event.OrderId, @event.OldStatus, @event.NewStatus);
            
            // Registrar cambio en auditoría
            await _auditService.LogStatusChangeAsync(@event.OrderId, @event.OldStatus, @event.NewStatus, @event.ChangedAt);
            
            // Lógica específica según el cambio de estado
            switch (@event.NewStatus)
            {
                case OrderStatus.Shipped:
                    await _emailService.SendOrderShippedAsync(@event.OrderId.ToString(), @event.OrderId);
                    break;
                case OrderStatus.Delivered:
                    await _emailService.SendOrderDeliveredAsync(@event.OrderId.ToString(), @event.OrderId);
                    break;
                case OrderStatus.Cancelled:
                    await _emailService.SendOrderCancelledAsync(@event.OrderId.ToString(), @event.OrderId);
                    break;
            }
            
            _logger.LogInformation("Successfully handled OrderStatusChangedEvent for order {@OrderId}", @event.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling OrderStatusChangedEvent for order {@OrderId}", @event.OrderId);
            throw;
        }
    }
}

// Application/EventHandlers/CustomerEventHandlers.cs
public class CustomerCreatedEventHandler : IDomainEventHandler<CustomerCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly IWelcomeService _welcomeService;
    private readonly ILogger<CustomerCreatedEventHandler> _logger;
    
    public CustomerCreatedEventHandler(
        IEmailService emailService,
        IWelcomeService welcomeService,
        ILogger<CustomerCreatedEventHandler> logger)
    {
        _emailService = emailService;
        _welcomeService = welcomeService;
        _logger = logger;
    }
    
    public async Task HandleAsync(CustomerCreatedEvent @event)
    {
        try
        {
            _logger.LogInformation("Handling CustomerCreatedEvent for customer {@CustomerId}", @event.CustomerId);
            
            // Enviar email de bienvenida
            await _emailService.SendWelcomeEmailAsync(@event.Email, @event.Name);
            
            // Crear perfil de bienvenida
            await _welcomeService.CreateWelcomeProfileAsync(@event.CustomerId, @event.Name, @event.Email);
            
            // Notificar al sistema de marketing
            await _marketingService.SubscribeToNewsletterAsync(@event.Email, @event.Name);
            
            _logger.LogInformation("Successfully handled CustomerCreatedEvent for customer {@CustomerId}", @event.CustomerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling CustomerCreatedEvent for customer {@CustomerId}", @event.CustomerId);
            throw;
        }
    }
}

public class CustomerOrderRecordedEventHandler : IDomainEventHandler<CustomerOrderRecordedEvent>
{
    private readonly ILoyaltyService _loyaltyService;
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<CustomerOrderRecordedEventHandler> _logger;
    
    public CustomerOrderRecordedEventHandler(
        ILoyaltyService loyaltyService,
        IAnalyticsService analyticsService,
        ILogger<CustomerOrderRecordedEventHandler> logger)
    {
        _loyaltyService = loyaltyService;
        _analyticsService = analyticsService;
        _logger = logger;
    }
    
    public async Task HandleAsync(CustomerOrderRecordedEvent @event)
    {
        try
        {
            _logger.LogInformation("Handling CustomerOrderRecordedEvent for customer {@CustomerId}", @event.CustomerId);
            
            // Actualizar programa de lealtad
            await _loyaltyService.AddPointsAsync(@event.CustomerId, @event.OrderTotal);
            
            // Actualizar analytics
            await _analyticsService.RecordCustomerPurchaseAsync(@event.CustomerId, @event.OrderTotal, @event.RecordedAt);
            
            // Verificar si el cliente califica para descuentos especiales
            await _loyaltyService.CheckForSpecialOffersAsync(@event.CustomerId);
            
            _logger.LogInformation("Successfully handled CustomerOrderRecordedEvent for customer {@CustomerId}", @event.CustomerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling CustomerOrderRecordedEvent for customer {@CustomerId}", @event.CustomerId);
            throw;
        }
    }
}
```

### 4. Event Dispatcher

El Event Dispatcher es responsable de distribuir los eventos a sus respectivos handlers.

```csharp
// Application/EventHandlers/IDomainEventDispatcher.cs
public interface IDomainEventDispatcher
{
    Task DispatchEventsAsync(IEnumerable<Entity> entities);
}

// Application/EventHandlers/DomainEventDispatcher.cs
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;
    
    public DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    public async Task DispatchEventsAsync(IEnumerable<Entity> entities)
    {
        foreach (var entity in entities)
        {
            var events = entity.DomainEvents.ToList();
            
            foreach (var domainEvent in events)
            {
                await DispatchEventAsync(domainEvent);
            }
            
            entity.ClearDomainEvents();
        }
    }
    
    private async Task DispatchEventAsync(DomainEvent domainEvent)
    {
        try
        {
            _logger.LogInformation("Dispatching domain event {EventType} with ID {EventId}", 
                domainEvent.EventType, domainEvent.Id);
            
            var eventType = domainEvent.GetType();
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
            
            var handlers = _serviceProvider.GetServices(handlerType);
            
            var tasks = new List<Task>();
            
            foreach (var handler in handlers)
            {
                var method = handlerType.GetMethod("HandleAsync");
                var task = (Task)method.Invoke(handler, new object[] { domainEvent });
                tasks.Add(task);
            }
            
            // Ejecutar todos los handlers en paralelo
            await Task.WhenAll(tasks);
            
            _logger.LogInformation("Successfully dispatched domain event {EventType} with ID {EventId}", 
                domainEvent.EventType, domainEvent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching domain event {EventType} with ID {EventId}", 
                domainEvent.EventType, domainEvent.Id);
            throw;
        }
    }
}

// Infrastructure/Data/UnitOfWork.cs (actualizado)
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private IDbContextTransaction _transaction;
    
    public UnitOfWork(ApplicationDbContext context, IDomainEventDispatcher eventDispatcher)
    {
        _context = context;
        _eventDispatcher = eventDispatcher;
    }
    
    public async Task<int> SaveChangesAsync()
    {
        // Obtener entidades con eventos pendientes
        var entitiesWithEvents = _context.ChangeTracker.Entries<Entity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();
        
        // Guardar cambios en la base de datos
        var result = await _context.SaveChangesAsync();
        
        // Despachar eventos de dominio
        await _eventDispatcher.DispatchEventsAsync(entitiesWithEvents);
        
        return result;
    }
    
    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }
    
    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
    
    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
```

### 5. Configuración de Dependencias

```csharp
// Program.cs - Configuración de Event Handlers
public void ConfigureServices(IServiceCollection services)
{
    // Registrar event dispatcher
    services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
    
    // Registrar event handlers
    services.AddScoped<IDomainEventHandler<OrderCreatedEvent>, OrderCreatedEventHandler>();
    services.AddScoped<IDomainEventHandler<OrderConfirmedEvent>, OrderConfirmedEventHandler>();
    services.AddScoped<IDomainEventHandler<OrderStatusChangedEvent>, OrderStatusChangedEventHandler>();
    services.AddScoped<IDomainEventHandler<CustomerCreatedEvent>, CustomerCreatedEventHandler>();
    services.AddScoped<IDomainEventHandler<CustomerOrderRecordedEvent>, CustomerOrderRecordedEventHandler>();
    
    // Registrar servicios externos
    services.AddScoped<IEmailService, EmailService>();
    services.AddScoped<IInventoryService, InventoryService>();
    services.AddScoped<IPaymentService, PaymentService>();
    services.AddScoped<IAuditService, AuditService>();
    services.AddScoped<IWelcomeService, WelcomeService>();
    services.AddScoped<ILoyaltyService, LoyaltyService>();
    services.AddScoped<IAnalyticsService, AnalyticsService>();
}
```

## Ejercicios Prácticos

### Ejercicio 1: Implementar Eventos de Producto
Crea eventos de dominio para la entidad Product (creación, actualización de stock, cambio de precio).

### Ejercicio 2: Crear Event Handlers
Implementa event handlers para procesar los eventos de producto.

### Ejercicio 3: Eventos de Auditoría
Crea un sistema de auditoría basado en eventos de dominio.

## Resumen

En esta clase hemos aprendido:

1. **Domain Events**: Objetos que representan cambios importantes en el dominio
2. **Entidades con Eventos**: Cómo agregar eventos a las entidades del dominio
3. **Event Handlers**: Componentes que procesan los eventos
4. **Event Dispatcher**: Sistema para distribuir eventos a sus handlers
5. **Integración con Unit of Work**: Cómo despachar eventos después de guardar cambios

En la siguiente clase continuaremos con **Microservicios** para implementar arquitecturas distribuidas.

## Recursos Adicionales
- [Domain Events](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-events-design-implementation)
- [Domain Events Pattern](https://martinfowler.com/eaaDev/DomainEvent.html)
- [Event Sourcing](https://martinfowler.com/eaaDev/EventSourcing.html)
