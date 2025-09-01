# 🏆 Senior Level 5: Arquitectura Limpia y Microservicios

## 🧭 Navegación del Curso

- **⬅️ Anterior**: [Módulo 11: Arquitectura de Microservicios Avanzada](../senior_4/README.md)
- **➡️ Siguiente**: [Módulo 13: Performance y Deployment](../senior_6/README.md)
- **📚 [Índice Completo](../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../NAVEGACION_RAPIDA.md)**

---

## 📋 Contenido del Nivel

### 🎯 Objetivos de Aprendizaje
- Comprender y aplicar principios de Clean Architecture
- Diseñar e implementar arquitecturas de microservicios
- Implementar patrones como CQRS, Event Sourcing y Domain Events
- Crear aplicaciones escalables y mantenibles
- Dominar la comunicación entre servicios

### ⏱️ Tiempo Estimado
- **Teoría**: 5-6 horas
- **Ejercicios**: 8-10 horas
- **Proyecto Integrador**: 6-8 horas
- **Total**: 19-24 horas

---

## 📚 Contenido Teórico

### 1. Clean Architecture

#### 1.1 ¿Qué es Clean Architecture?
Clean Architecture es un patrón de arquitectura de software que enfatiza la separación de responsabilidades y la independencia de frameworks, bases de datos y tecnologías externas.

#### 1.2 Capas de Clean Architecture

```csharp
// Estructura de carpetas
src/
├── Domain/                    # Entidades y reglas de negocio
├── Application/               # Casos de uso y lógica de aplicación
├── Infrastructure/            # Implementaciones técnicas
└── Presentation/              # APIs, controladores, etc.

// Domain Layer - Entidades de negocio
public class Order
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public List<OrderItem> Items { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public decimal Total => Items.Sum(item => item.Total);
    
    private Order() { } // Para EF Core
    
    public static Order Create(Guid customerId, List<OrderItem> items)
    {
        if (items == null || !items.Any())
            throw new DomainException("Order must have at least one item");
            
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Items = items,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        
        return order;
    }
    
    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Only pending orders can be confirmed");
            
        Status = OrderStatus.Confirmed;
    }
    
    public void Cancel()
    {
        if (Status == OrderStatus.Delivered)
            throw new DomainException("Delivered orders cannot be cancelled");
            
        Status = OrderStatus.Cancelled;
    }
}

public class OrderItem
{
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Total => Quantity * UnitPrice;
    
    private OrderItem() { }
    
    public static OrderItem Create(Guid productId, int quantity, decimal unitPrice)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero");
            
        if (unitPrice <= 0)
            throw new DomainException("Unit price must be greater than zero");
            
        return new OrderItem
        {
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }
}

// Domain Exceptions
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

// Value Objects
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
    
    public Money(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new DomainException("Amount cannot be negative");
            
        Amount = amount;
        Currency = currency;
    }
    
    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new DomainException("Cannot add money with different currencies");
            
        return new Money(left.Amount + right.Amount, left.Currency);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}

public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();
    
    public override bool Equals(object obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;
            
        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }
    
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x != null ? x.GetHashCode() : 0)
            .Aggregate((x, y) => x ^ y);
    }
    
    public static bool operator ==(ValueObject left, ValueObject right)
    {
        return EqualOperator(left, right);
    }
    
    public static bool operator !=(ValueObject left, ValueObject right)
    {
        return !EqualOperator(left, right);
    }
    
    protected static bool EqualOperator(ValueObject left, ValueObject right)
    {
        if (left is null ^ right is null)
            return false;
        return left is null || left.Equals(right);
    }
}
```

#### 1.3 Application Layer - Casos de Uso

```csharp
// Interfaces de repositorio
public interface IOrderRepository
{
    Task<Order> GetByIdAsync(Guid id);
    Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId);
    Task<Order> AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task DeleteAsync(Guid id);
}

// Interfaces de servicios externos
public interface IEmailService
{
    Task SendOrderConfirmationAsync(string email, Guid orderId);
    Task SendOrderCancellationAsync(string email, Guid orderId);
}

public interface IPaymentService
{
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
}

// Casos de uso
public class CreateOrderCommand : IRequest<CreateOrderResult>
{
    public Guid CustomerId { get; set; }
    public List<OrderItemDto> Items { get; set; }
}

public class CreateOrderResult
{
    public Guid OrderId { get; set; }
    public decimal Total { get; set; }
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    
    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validar cliente
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
                return CreateOrderResult.Failure("Customer not found");
                
            // Validar productos y crear items
            var orderItems = new List<OrderItem>();
            foreach (var itemDto in request.Items)
            {
                var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
                if (product == null)
                    return CreateOrderResult.Failure($"Product {itemDto.ProductId} not found");
                    
                if (product.Stock < itemDto.Quantity)
                    return CreateOrderResult.Failure($"Insufficient stock for product {product.Name}");
                    
                var orderItem = OrderItem.Create(itemDto.ProductId, itemDto.Quantity, product.Price);
                orderItems.Add(orderItem);
                
                // Actualizar stock
                product.UpdateStock(product.Stock - itemDto.Quantity);
                await _productRepository.UpdateAsync(product);
            }
            
            // Crear orden
            var order = Order.Create(request.CustomerId, orderItems);
            await _orderRepository.AddAsync(order);
            
            // Enviar email de confirmación
            await _emailService.SendOrderConfirmationAsync(customer.Email, order.Id);
            
            // Commit transacción
            await _unitOfWork.SaveChangesAsync();
            
            return CreateOrderResult.Success(order.Id, order.Total);
        }
        catch (Exception ex)
        {
            return CreateOrderResult.Failure($"Error creating order: {ex.Message}");
        }
    }
}

// Queries
public class GetCustomerOrdersQuery : IRequest<IEnumerable<CustomerOrderDto>>
{
    public Guid CustomerId { get; set; }
}

public class CustomerOrderDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; }
    public int ItemCount { get; set; }
}

public class GetCustomerOrdersQueryHandler : IRequestHandler<GetCustomerOrdersQuery, IEnumerable<CustomerOrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    
    public GetCustomerOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }
    
    public async Task<IEnumerable<CustomerOrderDto>> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(request.CustomerId);
        
        return orders.Select(o => new CustomerOrderDto
        {
            Id = o.Id,
            CreatedAt = o.CreatedAt,
            Total = o.Total,
            Status = o.Status.ToString(),
            ItemCount = o.Items.Count
        });
    }
}
```

#### 1.4 Infrastructure Layer

```csharp
// Implementación del repositorio
public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;
    
    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Order> GetByIdAsync(Guid id)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
    }
    
    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<Order> AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
        return order;
    }
    
    public async Task UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        await Task.CompletedTask;
    }
    
    public async Task DeleteAsync(Guid id)
    {
        var order = await GetByIdAsync(id);
        if (order != null)
        {
            _context.Orders.Remove(order);
        }
    }
}

// Implementación del servicio de email
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    
    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    
    public async Task SendOrderConfirmationAsync(string email, Guid orderId)
    {
        try
        {
            // Implementar lógica de envío de email
            _logger.LogInformation($"Sending order confirmation email to {email} for order {orderId}");
            await Task.Delay(100); // Simular envío
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending order confirmation email to {email}");
            throw;
        }
    }
    
    public async Task SendOrderCancellationAsync(string email, Guid orderId)
    {
        try
        {
            _logger.LogInformation($"Sending order cancellation email to {email} for order {orderId}");
            await Task.Delay(100); // Simular envío
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending order cancellation email to {email}");
            throw;
        }
    }
}

// Unit of Work
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction _transaction;
    
    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
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
        }
    }
    
    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
        }
    }
}
```

### 2. CQRS (Command Query Responsibility Segregation)

#### 2.1 Separación de Comandos y Consultas

```csharp
// Commands - Modifican el estado
public class UpdateOrderStatusCommand : IRequest<UpdateOrderStatusResult>
{
    public Guid OrderId { get; set; }
    public OrderStatus NewStatus { get; set; }
}

public class UpdateOrderStatusResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
}

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, UpdateOrderStatusResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public UpdateOrderStatusCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<UpdateOrderStatusResult> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
                return UpdateOrderStatusResult.Failure("Order not found");
            
            // Aplicar lógica de negocio según el nuevo estado
            switch (request.NewStatus)
            {
                case OrderStatus.Confirmed:
                    order.Confirm();
                    break;
                case OrderStatus.Cancelled:
                    order.Cancel();
                    break;
                default:
                    return UpdateOrderStatusResult.Failure("Invalid status transition");
            }
            
            await _orderRepository.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();
            
            return UpdateOrderStatusResult.Success();
        }
        catch (Exception ex)
        {
            return UpdateOrderStatusResult.Failure($"Error updating order status: {ex.Message}");
        }
    }
}

// Queries - Solo leen datos
public class GetOrderDetailsQuery : IRequest<OrderDetailsDto>
{
    public Guid OrderId { get; set; }
}

public class OrderDetailsDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; }
    public List<OrderItemDto> Items { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GetOrderDetailsQueryHandler : IRequestHandler<GetOrderDetailsQuery, OrderDetailsDto>
{
    private readonly IOrderQueryService _orderQueryService;
    
    public GetOrderDetailsQueryHandler(IOrderQueryService orderQueryService)
    {
        _orderQueryService = orderQueryService;
    }
    
    public async Task<OrderDetailsDto> Handle(GetOrderDetailsQuery request, CancellationToken cancellationToken)
    {
        return await _orderQueryService.GetOrderDetailsAsync(request.OrderId);
    }
}

// Query Service para consultas complejas
public interface IOrderQueryService
{
    Task<OrderDetailsDto> GetOrderDetailsAsync(Guid orderId);
    Task<IEnumerable<OrderSummaryDto>> GetCustomerOrdersAsync(Guid customerId);
    Task<PagedResult<OrderSummaryDto>> GetOrdersAsync(OrderFilter filter, int page, int pageSize);
}

public class OrderQueryService : IOrderQueryService
{
    private readonly ApplicationDbContext _context;
    
    public OrderQueryService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<OrderDetailsDto> GetOrderDetailsAsync(Guid orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == orderId);
            
        if (order == null)
            return null;
            
        return new OrderDetailsDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer.Name,
            Items = order.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Total = i.Total
            }).ToList(),
            Total = order.Total,
            Status = order.Status.ToString(),
            CreatedAt = order.CreatedAt
        };
    }
    
    public async Task<IEnumerable<OrderSummaryDto>> GetCustomerOrdersAsync(Guid customerId)
    {
        return await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                Total = o.Total,
                Status = o.Status.ToString(),
                CreatedAt = o.CreatedAt,
                ItemCount = o.Items.Count
            })
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<PagedResult<OrderSummaryDto>> GetOrdersAsync(OrderFilter filter, int page, int pageSize)
    {
        var query = _context.Orders.AsQueryable();
        
        // Aplicar filtros
        if (filter.CustomerId.HasValue)
            query = query.Where(o => o.CustomerId == filter.CustomerId);
            
        if (filter.Status.HasValue)
            query = query.Where(o => o.Status == filter.Status);
            
        if (filter.DateFrom.HasValue)
            query = query.Where(o => o.CreatedAt >= filter.DateFrom);
            
        if (filter.DateTo.HasValue)
            query = query.Where(o => o.CreatedAt <= filter.DateTo);
        
        var totalCount = await query.CountAsync();
        var orders = await query
            .Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer.Name,
                Total = o.Total,
                Status = o.Status.ToString(),
                CreatedAt = o.CreatedAt,
                ItemCount = o.Items.Count
            })
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        return new PagedResult<OrderSummaryDto>
        {
            Items = orders,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }
}
```

### 3. Domain Events

#### 3.1 Implementación de Domain Events

```csharp
// Domain Events
public abstract class DomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    
    protected DomainEvent()
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }
}

public class OrderCreatedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Guid CustomerId { get; }
    public decimal Total { get; }
    
    public OrderCreatedEvent(Guid orderId, Guid customerId, decimal total)
    {
        OrderId = orderId;
        CustomerId = customerId;
        Total = total;
    }
}

public class OrderStatusChangedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public OrderStatus OldStatus { get; }
    public OrderStatus NewStatus { get; }
    
    public OrderStatusChangedEvent(Guid orderId, OrderStatus oldStatus, OrderStatus newStatus)
    {
        OrderId = orderId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}

// Entity Base con soporte para eventos
public abstract class Entity
{
    private readonly List<DomainEvent> _domainEvents = new();
    
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

// Actualizar la entidad Order
public class Order : Entity
{
    // ... propiedades existentes ...
    
    public static Order Create(Guid customerId, List<OrderItem> items)
    {
        if (items == null || !items.Any())
            throw new DomainException("Order must have at least one item");
            
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Items = items,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        
        // Agregar evento de dominio
        order.AddDomainEvent(new OrderCreatedEvent(order.Id, customerId, order.Total));
        
        return order;
    }
    
    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Only pending orders can be confirmed");
            
        var oldStatus = Status;
        Status = OrderStatus.Confirmed;
        
        // Agregar evento de dominio
        AddDomainEvent(new OrderStatusChangedEvent(Id, oldStatus, Status));
    }
    
    public void Cancel()
    {
        if (Status == OrderStatus.Delivered)
            throw new DomainException("Delivered orders cannot be cancelled");
            
        var oldStatus = Status;
        Status = OrderStatus.Cancelled;
        
        // Agregar evento de dominio
        AddDomainEvent(new OrderStatusChangedEvent(Id, oldStatus, Status));
    }
}

// Event Handlers
public interface IDomainEventHandler<in TEvent> where TEvent : DomainEvent
{
    Task HandleAsync(TEvent @event);
}

public class OrderCreatedEventHandler : IDomainEventHandler<OrderCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderCreatedEventHandler> _logger;
    
    public OrderCreatedEventHandler(IEmailService emailService, ILogger<OrderCreatedEventHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }
    
    public async Task HandleAsync(OrderCreatedEvent @event)
    {
        try
        {
            _logger.LogInformation($"Handling OrderCreatedEvent for order {@event.OrderId}");
            
            // Enviar email de confirmación
            await _emailService.SendOrderConfirmationAsync(@event.CustomerId.ToString(), @event.OrderId);
            
            // Aquí podrías agregar más lógica como:
            // - Actualizar inventario
            // - Generar factura
            // - Notificar a sistemas externos
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error handling OrderCreatedEvent for order {@event.OrderId}");
            throw;
        }
    }
}

public class OrderStatusChangedEventHandler : IDomainEventHandler<OrderStatusChangedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderStatusChangedEventHandler> _logger;
    
    public OrderStatusChangedEventHandler(IEmailService emailService, ILogger<OrderStatusChangedEventHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }
    
    public async Task HandleAsync(OrderStatusChangedEvent @event)
    {
        try
        {
            _logger.LogInformation($"Handling OrderStatusChangedEvent for order {@event.OrderId}: {@event.OldStatus} -> {@event.NewStatus}");
            
            // Lógica específica según el cambio de estado
            switch (@event.NewStatus)
            {
                case OrderStatus.Cancelled:
                    // Enviar email de cancelación
                    await _emailService.SendOrderCancellationAsync(@event.OrderId.ToString(), @event.OrderId);
                    break;
                case OrderStatus.Shipped:
                    // Enviar email de envío
                    await _emailService.SendOrderShippedAsync(@event.OrderId.ToString(), @event.OrderId);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error handling OrderStatusChangedEvent for order {@event.OrderId}");
            throw;
        }
    }
}

// Event Dispatcher
public interface IDomainEventDispatcher
{
    Task DispatchEventsAsync(IEnumerable<Entity> entities);
}

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
            foreach (var domainEvent in entity.DomainEvents)
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
            var eventType = domainEvent.GetType();
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
            
            var handlers = _serviceProvider.GetServices(handlerType);
            
            foreach (var handler in handlers)
            {
                var method = handlerType.GetMethod("HandleAsync");
                var task = (Task)method.Invoke(handler, new object[] { domainEvent });
                await task;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error dispatching domain event {domainEvent.GetType().Name}");
            throw;
        }
    }
}
```

### 4. Microservicios

#### 4.1 Arquitectura de Microservicios

```csharp
// Estructura de solución
Microservices/
├── OrderService/
│   ├── OrderService.API/
│   ├── OrderService.Domain/
│   ├── OrderService.Infrastructure/
│   └── OrderService.Tests/
├── CustomerService/
│   ├── CustomerService.API/
│   ├── CustomerService.Domain/
│   ├── CustomerService.Infrastructure/
│   └── CustomerService.Tests/
├── ProductService/
│   ├── ProductService.API/
│   ├── ProductService.Domain/
│   ├── ProductService.Infrastructure/
│   └── ProductService.Tests/
└── Shared/
    ├── Shared.Messaging/
    └── Shared.Contracts/

// API Gateway
public class ApiGateway
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddReverseProxy()
            .LoadFromConfig(Configuration.GetSection("ReverseProxy"));
            
        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = "https://localhost:5001";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false
                };
            });
    }
    
    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapReverseProxy();
        });
    }
}

// Configuración del proxy reverso
{
  "ReverseProxy": {
    "Routes": {
      "order-route": {
        "ClusterId": "order-cluster",
        "Match": {
          "Path": "/api/orders/{**catch-all}"
        }
      },
      "customer-route": {
        "ClusterId": "customer-cluster",
        "Match": {
          "Path": "/api/customers/{**catch-all}"
        }
      },
      "product-route": {
        "ClusterId": "product-cluster",
        "Match": {
          "Path": "/api/products/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "order-cluster": {
        "Destinations": {
          "order-destination": {
            "Address": "https://localhost:5002"
          }
        }
      },
      "customer-cluster": {
        "Destinations": {
          "customer-destination": {
            "Address": "https://localhost:5003"
          }
        }
      },
      "product-cluster": {
        "Destinations": {
          "product-destination": {
            "Address": "https://localhost:5004"
          }
        }
      }
    }
  }
}
```

#### 4.2 Comunicación entre Servicios

```csharp
// Shared Contracts
public class OrderCreatedMessage
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CustomerUpdatedMessage
{
    public Guid CustomerId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Message Bus Interface
public interface IMessageBus
{
    Task PublishAsync<T>(T message) where T : class;
    Task SubscribeAsync<T>(Func<T, Task> handler) where T : class;
}

// RabbitMQ Implementation
public class RabbitMQMessageBus : IMessageBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQMessageBus> _logger;
    
    public RabbitMQMessageBus(IConfiguration configuration, ILogger<RabbitMQMessageBus> logger)
    {
        _logger = logger;
        
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:Host"],
            UserName = configuration["RabbitMQ:Username"],
            Password = configuration["RabbitMQ:Password"]
        };
        
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        // Declarar exchanges
        _channel.ExchangeDeclare("order-events", ExchangeType.Topic, durable: true);
        _channel.ExchangeDeclare("customer-events", ExchangeType.Topic, durable: true);
    }
    
    public async Task PublishAsync<T>(T message) where T : class
    {
        try
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);
            
            var exchange = GetExchangeName<T>();
            var routingKey = GetRoutingKey<T>();
            
            _channel.BasicPublish(
                exchange: exchange,
                routingKey: routingKey,
                basicProperties: null,
                body: body);
                
            _logger.LogInformation($"Published message of type {typeof(T).Name}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error publishing message of type {typeof(T).Name}");
            throw;
        }
    }
    
    public async Task SubscribeAsync<T>(Func<T, Task> handler) where T : class
    {
        try
        {
            var queueName = $"{typeof(T).Name.ToLower()}-queue";
            var exchange = GetExchangeName<T>();
            var routingKey = GetRoutingKey<T>();
            
            _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(queueName, exchange, routingKey);
            
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<T>(json);
                    
                    await handler(message);
                    
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing message of type {typeof(T).Name}");
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };
            
            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
            
            _logger.LogInformation($"Subscribed to messages of type {typeof(T).Name}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error subscribing to messages of type {typeof(T).Name}");
            throw;
        }
    }
    
    private string GetExchangeName<T>()
    {
        return typeof(T).Name.ToLower().Contains("order") ? "order-events" : "customer-events";
    }
    
    private string GetRoutingKey<T>()
    {
        return typeof(T).Name.ToLower().Replace("message", "");
    }
    
    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}

// Order Service - Publisher
public class OrderService
{
    private readonly IMessageBus _messageBus;
    
    public async Task<Order> CreateOrderAsync(CreateOrderCommand command)
    {
        // ... lógica de creación de orden ...
        
        // Publicar evento
        var message = new OrderCreatedMessage
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            Total = order.Total,
            CreatedAt = order.CreatedAt
        };
        
        await _messageBus.PublishAsync(message);
        
        return order;
    }
}

// Customer Service - Subscriber
public class CustomerService
{
    private readonly IMessageBus _messageBus;
    
    public CustomerService(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Suscribirse a eventos de orden
        await _messageBus.SubscribeAsync<OrderCreatedMessage>(HandleOrderCreatedAsync);
    }
    
    private async Task HandleOrderCreatedAsync(OrderCreatedMessage message)
    {
        // Actualizar estadísticas del cliente
        var customer = await GetCustomerByIdAsync(message.CustomerId);
        if (customer != null)
        {
            customer.TotalOrders++;
            customer.TotalSpent += message.Total;
            customer.LastOrderDate = message.CreatedAt;
            
            await UpdateCustomerAsync(customer);
        }
    }
}
```

---

## 🎯 Ejercicios Prácticos

### Ejercicio 1: Implementar Clean Architecture
Crea una aplicación de gestión de tareas siguiendo Clean Architecture.

### Ejercicio 2: Implementar CQRS
Separa comandos y consultas en un sistema de gestión de inventario.

### Ejercicio 3: Domain Events
Implementa eventos de dominio para un sistema de reservas.

### Ejercicio 4: Microservicios Básicos
Crea dos microservicios que se comuniquen entre sí.

### Ejercicio 5: API Gateway
Implementa un API Gateway para enrutar requests a diferentes servicios.

### Ejercicio 6: Message Bus
Implementa comunicación asíncrona entre servicios usando RabbitMQ.

### Ejercicio 7: Event Sourcing
Implementa un sistema básico de Event Sourcing para auditoría.

### Ejercicio 8: Saga Pattern
Implementa el patrón Saga para transacciones distribuidas.

### Ejercicio 9: Circuit Breaker
Implementa el patrón Circuit Breaker para manejo de fallos.

### Ejercicio 10: Health Checks
Implementa health checks para monitoreo de microservicios.

---

## 🚀 Proyecto Integrador: E-commerce con Microservicios

### Descripción
Crea un sistema de e-commerce completo usando Clean Architecture y microservicios.

### Requisitos
- Arquitectura limpia con separación de capas
- Microservicios para órdenes, clientes y productos
- CQRS para separación de comandos y consultas
- Eventos de dominio para comunicación asíncrona
- API Gateway para enrutamiento
- Message Bus para comunicación entre servicios
- Health checks y monitoreo

### Estructura Sugerida
```
ECommerceMicroservices/
├── ApiGateway/
├── OrderService/
├── CustomerService/
├── ProductService/
├── Shared/
│   ├── Contracts/
│   └── Messaging/
└── Docker/
```

---

## 📝 Autoevaluación

### Preguntas Teóricas
1. ¿Cuáles son las capas de Clean Architecture y cuál es su propósito?
2. ¿Qué ventajas ofrece CQRS sobre un enfoque tradicional?
3. ¿Cómo funcionan los Domain Events en Clean Architecture?
4. ¿Cuáles son los desafíos de implementar microservicios?
5. ¿Qué patrones usas para comunicación entre servicios?

### Preguntas Prácticas
1. Implementa Clean Architecture para un sistema de gestión de usuarios
2. Crea un microservicio con CQRS y eventos de dominio
3. Implementa comunicación asíncrona entre dos servicios
4. Diseña un API Gateway para múltiples microservicios

---

## 🔗 Enlaces de Referencia

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [Domain Events](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-events-design-implementation)
- [Microservices Architecture](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/)

---

## 📚 Siguiente Nivel

**Progreso**: 10 de 12 niveles completados

**Siguiente**: [Senior Level 6: Performance, Seguridad y Deployment](../senior_6/README.md)

**Anterior**: [Senior Level 4: Entity Framework y Bases de Datos](../senior_4/README.md)

---

## 🎉 ¡Felicidades!

Has completado el nivel senior de arquitectura limpia y microservicios. Ahora puedes:
- Diseñar aplicaciones siguiendo principios de Clean Architecture
- Implementar CQRS y eventos de dominio
- Crear arquitecturas de microservicios escalables
- Implementar comunicación asíncrona entre servicios
- Aplicar patrones avanzados de arquitectura de software

¡Continúa con el último nivel para dominar performance, seguridad y deployment!
