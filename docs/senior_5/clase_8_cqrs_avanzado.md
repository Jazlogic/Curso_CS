# Clase 8: CQRS Avanzado

## Navegación
- [← Clase anterior: Event Sourcing](clase_7_event_sourcing.md)
- [← Volver al README del módulo](README.md)
- [→ Siguiente clase: Testing Avanzado](clase_9_testing_avanzado.md)

## Objetivos de Aprendizaje
- Implementar CQRS con Event Sourcing
- Crear consultas optimizadas y proyecciones
- Aplicar patrones de validación avanzados
- Implementar manejo de errores robusto

## CQRS Avanzado con Event Sourcing

El **CQRS Avanzado** combina la separación de comandos y consultas con Event Sourcing para crear sistemas altamente escalables y mantenibles. Los comandos modifican el estado a través de eventos, mientras que las consultas se optimizan para diferentes escenarios de lectura.

### Arquitectura CQRS Avanzada

```csharp
// 1. Separación Completa de Modelos
// Write Model - Basado en eventos
public class OrderWriteModel
{
    public async Task<Order> CreateOrderAsync(CreateOrderCommand command)
    {
        var order = Order.Create(command.CustomerId, command.Items, command.ShippingAddress, command.PaymentMethod);
        await _eventStore.SaveEventsAsync(order.Id, order.GetUncommittedEvents(), -1);
        return order;
    }
}

// Read Model - Optimizado para consultas
public class OrderReadModel
{
    public async Task<OrderDto> GetOrderAsync(Guid orderId)
    {
        // Usar proyección optimizada en lugar de reconstruir desde eventos
        return await _orderProjection.GetOrderAsync(orderId);
    }
}

// 2. Proyecciones en Tiempo Real
// Los eventos se procesan para mantener modelos de lectura actualizados
public class OrderProjectionService
{
    public async Task ProcessEventAsync(DomainEvent @event)
    {
        switch (@event)
        {
            case OrderCreatedEvent e:
                await CreateOrderProjection(e);
                break;
            case OrderStatusChangedEvent e:
                await UpdateOrderStatus(e);
                break;
        }
    }
}
```

## Implementación de CQRS Avanzado

### 1. Comandos y Validaciones

```csharp
// Application/Commands/CreateOrder/CreateOrderCommand.cs
public class CreateOrderCommand : IRequest<CreateOrderResult>
{
    public Guid CustomerId { get; set; }
    public List<OrderItemDto> Items { get; set; }
    public string ShippingAddress { get; set; }
    public string PaymentMethod { get; set; }
    public string CustomerEmail { get; set; }
    public string CustomerPhone { get; set; }
}

// Application/Commands/CreateOrder/CreateOrderCommandValidator.cs
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required");
            
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Order must have at least one item");
            
        RuleFor(x => x.Items)
            .Must(items => items.All(item => item.Quantity > 0))
            .WithMessage("All items must have quantity greater than 0");
            
        RuleFor(x => x.Items)
            .Must(items => items.All(item => item.UnitPrice > 0))
            .WithMessage("All items must have unit price greater than 0");
            
        RuleFor(x => x.ShippingAddress)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("Shipping address is required and must not exceed 500 characters");
            
        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .Must(BeValidPaymentMethod)
            .WithMessage("Payment method must be valid");
            
        RuleFor(x => x.CustomerEmail)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Valid customer email is required");
            
        RuleFor(x => x.CustomerPhone)
            .NotEmpty()
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Valid customer phone number is required");
    }
    
    private bool BeValidPaymentMethod(string paymentMethod)
    {
        var validMethods = new[] { "CreditCard", "DebitCard", "PayPal", "BankTransfer" };
        return validMethods.Contains(paymentMethod);
    }
}

// Application/Commands/CreateOrder/CreateOrderCommandHandler.cs
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IEventSourcedRepository<Order> _orderRepository;
    private readonly ICustomerService _customerService;
    private readonly IProductService _productService;
    private readonly IMessageBus _messageBus;
    private readonly ILogger<CreateOrderCommandHandler> _logger;
    
    public CreateOrderCommandHandler(
        IEventSourcedRepository<Order> orderRepository,
        ICustomerService customerService,
        IProductService productService,
        IMessageBus messageBus,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _customerService = customerService;
        _productService = productService;
        _messageBus = messageBus;
        _logger = logger;
    }
    
    public async Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating order for customer {CustomerId}", request.CustomerId);
            
            // Validar cliente
            var customer = await _customerService.GetCustomerAsync(request.CustomerId);
            if (customer == null)
            {
                return CreateOrderResult.Failure("Customer not found");
            }
            
            // Validar productos y crear items
            var orderItems = new List<OrderItem>();
            var totalAmount = 0m;
            
            foreach (var itemDto in request.Items)
            {
                var product = await _productService.GetProductAsync(itemDto.ProductId);
                if (product == null)
                {
                    return CreateOrderResult.Failure($"Product {itemDto.ProductId} not found");
                }
                
                if (product.Stock < itemDto.Quantity)
                {
                    return CreateOrderResult.Failure($"Insufficient stock for product {product.Name}");
                }
                
                var orderItem = OrderItem.Create(itemDto.ProductId, itemDto.Quantity, itemDto.UnitPrice);
                orderItems.Add(orderItem);
                totalAmount += orderItem.Total;
            }
            
            // Crear la orden
            var order = Order.Create(request.CustomerId, orderItems, request.ShippingAddress, request.PaymentMethod);
            
            // Guardar eventos
            await _orderRepository.SaveAsync(order);
            
            // Publicar evento de integración
            var orderCreatedMessage = new OrderCreatedIntegrationEvent
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                CustomerEmail = request.CustomerEmail,
                CustomerPhone = request.CustomerPhone,
                Total = totalAmount,
                CreatedAt = order.CreatedAt
            };
            
            await _messageBus.PublishAsync(orderCreatedMessage);
            
            _logger.LogInformation("Order {OrderId} created successfully for customer {CustomerId}", 
                order.Id, request.CustomerId);
            
            return CreateOrderResult.Success(order.Id, totalAmount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for customer {CustomerId}", request.CustomerId);
            return CreateOrderResult.Failure($"Error creating order: {ex.Message}");
        }
    }
}

// Application/Commands/UpdateOrderStatus/UpdateOrderStatusCommand.cs
public class UpdateOrderStatusCommand : IRequest<UpdateOrderStatusResult>
{
    public Guid OrderId { get; set; }
    public string NewStatus { get; set; }
    public string? Notes { get; set; }
    public string? UpdatedBy { get; set; }
}

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");
            
        RuleFor(x => x.NewStatus)
            .NotEmpty()
            .Must(BeValidOrderStatus)
            .WithMessage("New status must be valid");
            
        RuleFor(x => x.UpdatedBy)
            .NotEmpty()
            .WithMessage("User updating the status must be specified");
    }
    
    private bool BeValidOrderStatus(string status)
    {
        var validStatuses = new[] { "Pending", "Confirmed", "Shipped", "Delivered", "Cancelled" };
        return validStatuses.Contains(status);
    }
}

// Application/Commands/UpdateOrderStatus/UpdateOrderStatusCommandHandler.cs
public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, UpdateOrderStatusResult>
{
    private readonly IEventSourcedRepository<Order> _orderRepository;
    private readonly IMessageBus _messageBus;
    private readonly ILogger<UpdateOrderStatusCommandHandler> _logger;
    
    public UpdateOrderStatusCommandHandler(
        IEventSourcedRepository<Order> orderRepository,
        IMessageBus messageBus,
        ILogger<UpdateOrderStatusCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _messageBus = messageBus;
        _logger = logger;
    }
    
    public async Task<UpdateOrderStatusResult> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating order {OrderId} status to {NewStatus}", 
                request.OrderId, request.NewStatus);
            
            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
            {
                return UpdateOrderStatusResult.Failure("Order not found");
            }
            
            var oldStatus = order.Status.ToString();
            
            // Aplicar cambio de estado según el comando
            switch (request.NewStatus)
            {
                case "Confirmed":
                    order.Confirm();
                    break;
                case "Shipped":
                    order.Ship();
                    break;
                case "Delivered":
                    order.Deliver();
                    break;
                case "Cancelled":
                    order.Cancel(request.Notes ?? "No reason provided");
                    break;
                default:
                    return UpdateOrderStatusResult.Failure("Invalid status transition");
            }
            
            // Guardar eventos
            await _orderRepository.SaveAsync(order);
            
            // Publicar evento de integración
            var statusChangedMessage = new OrderStatusChangedIntegrationEvent
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                OldStatus = oldStatus,
                NewStatus = request.NewStatus,
                Notes = request.Notes,
                UpdatedBy = request.UpdatedBy,
                ChangedAt = DateTime.UtcNow
            };
            
            await _messageBus.PublishAsync(statusChangedMessage);
            
            _logger.LogInformation("Order {OrderId} status updated from {OldStatus} to {NewStatus}", 
                order.Id, oldStatus, request.NewStatus);
            
            return UpdateOrderStatusResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order {OrderId} status", request.OrderId);
            return UpdateOrderStatusResult.Failure($"Error updating order status: {ex.Message}");
        }
    }
}
```

### 2. Consultas Optimizadas

```csharp
// Application/Queries/GetOrderDetails/GetOrderDetailsQuery.cs
public class GetOrderDetailsQuery : IRequest<OrderDetailsDto>
{
    public Guid OrderId { get; set; }
    public bool IncludeTimeline { get; set; } = false;
}

// Application/Queries/GetOrderDetails/GetOrderDetailsQueryHandler.cs
public class GetOrderDetailsQueryHandler : IRequestHandler<GetOrderDetailsQuery, OrderDetailsDto>
{
    private readonly IOrderProjection _orderProjection;
    private readonly IOrderTimelineProjection _timelineProjection;
    private readonly ILogger<GetOrderDetailsQueryHandler> _logger;
    
    public GetOrderDetailsQueryHandler(
        IOrderProjection orderProjection,
        IOrderTimelineProjection timelineProjection,
        ILogger<GetOrderDetailsQueryHandler> logger)
    {
        _orderProjection = orderProjection;
        _timelineProjection = timelineProjection;
        _logger = logger;
    }
    
    public async Task<OrderDetailsDto> Handle(GetOrderDetailsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting order details for order {OrderId}", request.OrderId);
            
            var orderTask = _orderProjection.GetOrderAsync(request.OrderId);
            var timelineTask = request.IncludeTimeline 
                ? _timelineProjection.GetOrderTimelineAsync(request.OrderId) 
                : Task.FromResult<OrderTimelineDto>(null);
            
            await Task.WhenAll(orderTask, timelineTask);
            
            var order = await orderTask;
            if (order == null)
            {
                return null;
            }
            
            var timeline = await timelineTask;
            
            var orderDetails = new OrderDetailsDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                Items = order.Items,
                Total = order.Total,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress,
                PaymentMethod = order.PaymentMethod,
                CreatedAt = order.CreatedAt,
                ConfirmedAt = order.ConfirmedAt,
                ShippedAt = order.ShippedAt,
                DeliveredAt = order.DeliveredAt,
                Timeline = timeline
            };
            
            _logger.LogInformation("Successfully retrieved order details for order {OrderId}", request.OrderId);
            return orderDetails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order details for order {OrderId}", request.OrderId);
            throw;
        }
    }
}

// Application/Queries/GetOrdersPaged/GetOrdersPagedQuery.cs
public class GetOrdersPagedQuery : IRequest<PagedResult<OrderSummaryDto>>
{
    public OrderFilter Filter { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
    public bool IncludeCustomerInfo { get; set; } = false;
}

public class OrderFilter
{
    public Guid? CustomerId { get; set; }
    public string? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public decimal? MinTotal { get; set; }
    public decimal? MaxTotal { get; set; }
    public string? PaymentMethod { get; set; }
    public string? ShippingAddress { get; set; }
}

// Application/Queries/GetOrdersPaged/GetOrdersPagedQueryHandler.cs
public class GetOrdersPagedQueryHandler : IRequestHandler<GetOrdersPagedQuery, PagedResult<OrderSummaryDto>>
{
    private readonly IOrderProjection _orderProjection;
    private readonly ICustomerProjection _customerProjection;
    private readonly ILogger<GetOrdersPagedQueryHandler> _logger;
    
    public GetOrdersPagedQueryHandler(
        IOrderProjection orderProjection,
        ICustomerProjection customerProjection,
        ILogger<GetOrdersPagedQueryHandler> logger)
    {
        _orderProjection = orderProjection;
        _customerProjection = customerProjection;
        _logger = logger;
    }
    
    public async Task<PagedResult<OrderSummaryDto>> Handle(GetOrdersPagedQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting paged orders with filter: {@Filter}", request.Filter);
            
            var result = await _orderProjection.GetOrdersPagedAsync(
                request.Filter,
                request.Page,
                request.PageSize,
                request.SortBy,
                request.SortDescending);
            
            // Enriquecer con información del cliente si se solicita
            if (request.IncludeCustomerInfo && result.Items.Any())
            {
                var customerIds = result.Items.Select(o => o.CustomerId).Distinct().ToList();
                var customers = await _customerProjection.GetCustomersByIdsAsync(customerIds);
                
                foreach (var order in result.Items)
                {
                    var customer = customers.FirstOrDefault(c => c.Id == order.CustomerId);
                    if (customer != null)
                    {
                        order.CustomerName = customer.Name;
                        order.CustomerEmail = customer.Email;
                    }
                }
            }
            
            _logger.LogInformation("Successfully retrieved {Count} orders (page {Page} of {TotalPages})", 
                result.Items.Count(), request.Page, result.TotalPages);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged orders");
            throw;
        }
    }
}
```

### 3. Proyecciones en Tiempo Real

```csharp
// Application/Projections/IOrderProjection.cs
public interface IOrderProjection
{
    Task<OrderDto> GetOrderAsync(Guid orderId);
    Task<PagedResult<OrderSummaryDto>> GetOrdersPagedAsync(OrderFilter filter, int page, int pageSize, string sortBy, bool sortDescending);
    Task<IEnumerable<OrderSummaryDto>> GetCustomerOrdersAsync(Guid customerId);
    Task<OrderStatistics> GetOrderStatisticsAsync(DateTime from, DateTime to);
}

// Application/Projections/OrderProjection.cs
public class OrderProjection : IOrderProjection
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OrderProjection> _logger;
    
    public OrderProjection(ApplicationDbContext context, ILogger<OrderProjection> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<OrderDto> GetOrderAsync(Guid orderId)
    {
        var order = await _context.OrderProjections
            .Include(o => o.Items)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == orderId);
            
        if (order == null)
            return null;
            
        return new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer?.Name ?? "Unknown",
            CustomerEmail = order.Customer?.Email ?? "Unknown",
            Items = order.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Total = i.Total
            }).ToList(),
            Total = order.Total,
            Status = order.Status,
            ShippingAddress = order.ShippingAddress,
            PaymentMethod = order.PaymentMethod,
            CreatedAt = order.CreatedAt,
            ConfirmedAt = order.ConfirmedAt,
            ShippedAt = order.ShippedAt,
            DeliveredAt = order.DeliveredAt
        };
    }
    
    public async Task<PagedResult<OrderSummaryDto>> GetOrdersPagedAsync(
        OrderFilter filter, int page, int pageSize, string sortBy, bool sortDescending)
    {
        var query = _context.OrderProjections.AsQueryable();
        
        // Aplicar filtros
        query = ApplyFilters(query, filter);
        
        // Aplicar ordenamiento
        query = ApplySorting(query, sortBy, sortDescending);
        
        var totalCount = await query.CountAsync();
        var orders = await query
            .Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer.Name,
                Total = o.Total,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                ItemCount = o.Items.Count
            })
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
    
    public async Task<IEnumerable<OrderSummaryDto>> GetCustomerOrdersAsync(Guid customerId)
    {
        return await _context.OrderProjections
            .Where(o => o.CustomerId == customerId)
            .Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer.Name,
                Total = o.Total,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                ItemCount = o.Items.Count
            })
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<OrderStatistics> GetOrderStatisticsAsync(DateTime from, DateTime to)
    {
        var orders = await _context.OrderProjections
            .Where(o => o.CreatedAt >= from && o.CreatedAt <= to)
            .ToListAsync();
            
        return new OrderStatistics
        {
            TotalOrders = orders.Count,
            TotalRevenue = orders.Sum(o => o.Total),
            AverageOrderValue = orders.Any() ? orders.Average(o => o.Total) : 0,
            OrdersByStatus = orders.GroupBy(o => o.Status)
                .Select(g => new StatusCount { Status = g.Key, Count = g.Count() })
                .ToList(),
            OrdersByDate = orders.GroupBy(o => o.CreatedAt.Date)
                .Select(g => new DateCount { Date = g.Key, Count = g.Count(), Revenue = g.Sum(o => o.Total) })
                .OrderBy(d => d.Date)
                .ToList()
        };
    }
    
    private IQueryable<OrderProjectionEntity> ApplyFilters(IQueryable<OrderProjectionEntity> query, OrderFilter filter)
    {
        if (filter.CustomerId.HasValue)
            query = query.Where(o => o.CustomerId == filter.CustomerId.Value);
            
        if (!string.IsNullOrEmpty(filter.Status))
            query = query.Where(o => o.Status == filter.Status);
            
        if (filter.DateFrom.HasValue)
            query = query.Where(o => o.CreatedAt >= filter.DateFrom.Value);
            
        if (filter.DateTo.HasValue)
            query = query.Where(o => o.CreatedAt <= filter.DateTo.Value);
            
        if (filter.MinTotal.HasValue)
            query = query.Where(o => o.Total >= filter.MinTotal.Value);
            
        if (filter.MaxTotal.HasValue)
            query = query.Where(o => o.Total <= filter.MaxTotal.Value);
            
        if (!string.IsNullOrEmpty(filter.PaymentMethod))
            query = query.Where(o => o.PaymentMethod == filter.PaymentMethod);
            
        if (!string.IsNullOrEmpty(filter.ShippingAddress))
            query = query.Where(o => o.ShippingAddress.Contains(filter.ShippingAddress));
            
        return query;
    }
    
    private IQueryable<OrderProjectionEntity> ApplySorting(IQueryable<OrderProjectionEntity> query, string sortBy, bool sortDescending)
    {
        return sortBy.ToLower() switch
        {
            "total" => sortDescending ? query.OrderByDescending(o => o.Total) : query.OrderBy(o => o.Total),
            "status" => sortDescending ? query.OrderByDescending(o => o.Status) : query.OrderBy(o => o.Status),
            "customername" => sortDescending ? query.OrderByDescending(o => o.Customer.Name) : query.OrderBy(o => o.Customer.Name),
            "createdat" => sortDescending ? query.OrderByDescending(o => o.CreatedAt) : query.OrderBy(o => o.CreatedAt),
            _ => sortDescending ? query.OrderByDescending(o => o.CreatedAt) : query.OrderBy(o => o.CreatedAt)
        };
    }
}

// Infrastructure/Data/OrderProjectionEntity.cs
public class OrderProjectionEntity
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public CustomerProjectionEntity Customer { get; set; }
    public List<OrderItemProjectionEntity> Items { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; }
    public string ShippingAddress { get; set; }
    public string PaymentMethod { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public long Version { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class OrderItemProjectionEntity
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}

public class CustomerProjectionEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}
```

### 4. Procesamiento de Eventos para Proyecciones

```csharp
// Application/Projections/OrderProjectionService.cs
public class OrderProjectionService : IOrderProjectionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OrderProjectionService> _logger;
    
    public OrderProjectionService(ApplicationDbContext context, ILogger<OrderProjectionService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task ProcessEventAsync(DomainEvent @event)
    {
        try
        {
            switch (@event)
            {
                case OrderCreatedEvent e:
                    await ProcessOrderCreatedEvent(e);
                    break;
                case OrderItemAddedEvent e:
                    await ProcessOrderItemAddedEvent(e);
                    break;
                case OrderStatusChangedEvent e:
                    await ProcessOrderStatusChangedEvent(e);
                    break;
                case OrderCancelledEvent e:
                    await ProcessOrderCancelledEvent(e);
                    break;
            }
            
            _logger.LogInformation("Successfully processed event {EventType} for order {OrderId}", 
                @event.EventType, @event.AggregateId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event {EventType} for order {OrderId}", 
                @event.EventType, @event.AggregateId);
            throw;
        }
    }
    
    private async Task ProcessOrderCreatedEvent(OrderCreatedEvent @event)
    {
        var projection = new OrderProjectionEntity
        {
            Id = @event.AggregateId,
            CustomerId = @event.CustomerId,
            Items = @event.Items.Select(i => new OrderItemProjectionEntity
            {
                ProductId = i.ProductId,
                ProductName = "Unknown", // Se actualizará cuando se procese el evento de producto
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Total = i.Quantity * i.UnitPrice
            }).ToList(),
            Total = @event.Items.Sum(i => i.Quantity * i.UnitPrice),
            Status = "Pending",
            ShippingAddress = @event.ShippingAddress,
            PaymentMethod = @event.PaymentMethod,
            CreatedAt = @event.CreatedAt,
            Version = @event.Version,
            LastUpdated = DateTime.UtcNow
        };
        
        await _context.OrderProjections.AddAsync(projection);
        await _context.SaveChangesAsync();
    }
    
    private async Task ProcessOrderItemAddedEvent(OrderItemAddedEvent @event)
    {
        var order = await _context.OrderProjections
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == @event.AggregateId);
            
        if (order != null)
        {
            var item = new OrderItemProjectionEntity
            {
                ProductId = @event.ProductId,
                ProductName = "Unknown", // Se actualizará cuando se procese el evento de producto
                Quantity = @event.Quantity,
                UnitPrice = @event.UnitPrice,
                Total = @event.Quantity * @event.UnitPrice
            };
            
            order.Items.Add(item);
            order.Total = order.Items.Sum(i => i.Total);
            order.Version = @event.Version;
            order.LastUpdated = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
        }
    }
    
    private async Task ProcessOrderStatusChangedEvent(OrderStatusChangedEvent @event)
    {
        var order = await _context.OrderProjections
            .FirstOrDefaultAsync(o => o.Id == @event.AggregateId);
            
        if (order != null)
        {
            order.Status = @event.NewStatus;
            
            // Actualizar timestamps según el estado
            switch (@event.NewStatus)
            {
                case "Confirmed":
                    order.ConfirmedAt = @event.ChangedAt;
                    break;
                case "Shipped":
                    order.ShippedAt = @event.ChangedAt;
                    break;
                case "Delivered":
                    order.DeliveredAt = @event.ChangedAt;
                    break;
            }
            
            order.Version = @event.Version;
            order.LastUpdated = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
        }
    }
    
    private async Task ProcessOrderCancelledEvent(OrderCancelledEvent @event)
    {
        var order = await _context.OrderProjections
            .FirstOrDefaultAsync(o => o.Id == @event.AggregateId);
            
        if (order != null)
        {
            order.Status = "Cancelled";
            order.Version = @event.Version;
            order.LastUpdated = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
        }
    }
}
```

## Ejercicios Prácticos

### Ejercicio 1: Implementar Validaciones Avanzadas
Crea validaciones complejas para comandos de negocio.

### Ejercicio 2: Crear Proyecciones Especializadas
Implementa proyecciones para diferentes escenarios de consulta.

### Ejercicio 3: Implementar Caching de Consultas
Agrega caching para optimizar consultas frecuentes.

## Resumen

En esta clase hemos aprendido:

1. **CQRS Avanzado**: Separación completa de modelos de escritura y lectura
2. **Validaciones Robustas**: Uso de FluentValidation para comandos
3. **Proyecciones en Tiempo Real**: Procesamiento de eventos para mantener modelos de lectura
4. **Consultas Optimizadas**: Filtrado, paginación y ordenamiento avanzado
5. **Manejo de Errores**: Resultados tipados y logging detallado

En la siguiente clase continuaremos con **Testing Avanzado** para implementar pruebas robustas del sistema.

## Recursos Adicionales
- [CQRS Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [FluentValidation](https://docs.fluentvalidation.net/)
- [Advanced CQRS](https://martinfowler.com/bliki/CQRS.html)

