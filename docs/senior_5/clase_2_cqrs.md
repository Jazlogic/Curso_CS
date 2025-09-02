# Clase 2: CQRS (Command Query Responsibility Segregation)

## Navegación
- [← Clase anterior: Clean Architecture](clase_1_clean_architecture.md)
- [← Volver al README del módulo](README.md)
- [→ Siguiente clase: Domain Events](clase_3_domain_events.md)

## Objetivos de Aprendizaje
- Comprender el patrón CQRS y sus beneficios
- Implementar separación de comandos y consultas
- Crear command handlers y query handlers
- Aplicar CQRS en aplicaciones reales

## ¿Qué es CQRS?

**CQRS (Command Query Responsibility Segregation)** es un patrón arquitectónico que separa las operaciones de **lectura** (Queries) de las operaciones de **escritura** (Commands). Esta separación permite optimizar cada tipo de operación de manera independiente y puede mejorar significativamente el rendimiento y la escalabilidad de la aplicación.

### Beneficios del CQRS

```csharp
// 1. Separación de Responsabilidades
// Commands - Modifican el estado
public class CreateOrderCommand : IRequest<CreateOrderResult>
{
    public Guid CustomerId { get; set; }
    public List<OrderItemDto> Items { get; set; }
}

// Queries - Solo leen datos
public class GetOrderDetailsQuery : IRequest<OrderDetailsDto>
{
    public Guid OrderId { get; set; }
}

// 2. Optimizaciones Independientes
// Commands pueden usar write-optimized databases
// Queries pueden usar read-optimized databases o caches

// 3. Escalabilidad
// Puedes escalar read y write databases por separado

// 4. Flexibilidad
// Diferentes modelos de datos para lectura y escritura
```

## Implementación de CQRS

### 1. Commands (Comandos)

Los comandos representan la intención de cambiar el estado del sistema. No devuelven datos, solo indican si la operación fue exitosa.

```csharp
// Application/Commands/CreateOrder/CreateOrderCommand.cs
public class CreateOrderCommand : IRequest<CreateOrderResult>
{
    public Guid CustomerId { get; set; }
    public List<OrderItemDto> Items { get; set; }
    public string ShippingAddress { get; set; }
    public string PaymentMethod { get; set; }
}

public class CreateOrderResult
{
    public bool IsSuccess { get; set; }
    public Guid? OrderId { get; set; }
    public decimal Total { get; set; }
    public string ErrorMessage { get; set; }
    
    public static CreateOrderResult Success(Guid orderId, decimal total)
    {
        return new CreateOrderResult
        {
            IsSuccess = true,
            OrderId = orderId,
            Total = total
        };
    }
    
    public static CreateOrderResult Failure(string errorMessage)
    {
        return new CreateOrderResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}

// Application/Commands/CreateOrder/CreateOrderCommandHandler.cs
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<CreateOrderCommandHandler> _logger;
    
    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }
    
    public async Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating order for customer {CustomerId}", request.CustomerId);
            
            // Validar cliente
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
            {
                _logger.LogWarning("Customer {CustomerId} not found", request.CustomerId);
                return CreateOrderResult.Failure("Customer not found");
            }
            
            // Validar productos y crear items
            var orderItems = new List<OrderItem>();
            var totalAmount = 0m;
            
            foreach (var itemDto in request.Items)
            {
                var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
                if (product == null)
                {
                    _logger.LogWarning("Product {ProductId} not found", itemDto.ProductId);
                    return CreateOrderResult.Failure($"Product {itemDto.ProductId} not found");
                }
                
                if (product.Stock < itemDto.Quantity)
                {
                    _logger.LogWarning("Insufficient stock for product {ProductId}", itemDto.ProductId);
                    return CreateOrderResult.Failure($"Insufficient stock for product {product.Name}");
                }
                
                var orderItem = OrderItem.Create(itemDto.ProductId, itemDto.Quantity, product.Price);
                orderItems.Add(orderItem);
                totalAmount += orderItem.Total;
                
                // Actualizar stock del producto
                product.UpdateStock(product.Stock - itemDto.Quantity);
                await _productRepository.UpdateAsync(product);
            }
            
            // Crear la orden
            var order = Order.Create(request.CustomerId, orderItems);
            order.SetShippingAddress(request.ShippingAddress);
            order.SetPaymentMethod(request.PaymentMethod);
            
            // Guardar la orden
            await _orderRepository.AddAsync(order);
            
            // Actualizar estadísticas del cliente
            customer.RecordOrder(totalAmount);
            await _customerRepository.UpdateAsync(customer);
            
            // Commit de la transacción
            await _unitOfWork.SaveChangesAsync();
            
            // Enviar email de confirmación
            await _emailService.SendOrderConfirmationAsync(customer.Email, order.Id);
            
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
    public OrderStatus NewStatus { get; set; }
    public string? Notes { get; set; }
}

public class UpdateOrderStatusResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
    
    public static UpdateOrderStatusResult Success()
    {
        return new UpdateOrderStatusResult { IsSuccess = true };
    }
    
    public static UpdateOrderStatusResult Failure(string errorMessage)
    {
        return new UpdateOrderStatusResult 
        { 
            IsSuccess = false, 
            ErrorMessage = errorMessage 
        };
    }
}

// Application/Commands/UpdateOrderStatus/UpdateOrderStatusCommandHandler.cs
public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, UpdateOrderStatusResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<UpdateOrderStatusCommandHandler> _logger;
    
    public UpdateOrderStatusCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<UpdateOrderStatusCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
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
                _logger.LogWarning("Order {OrderId} not found", request.OrderId);
                return UpdateOrderStatusResult.Failure("Order not found");
            }
            
            var oldStatus = order.Status;
            
            // Aplicar lógica de negocio según el nuevo estado
            switch (request.NewStatus)
            {
                case OrderStatus.Confirmed:
                    order.Confirm();
                    break;
                case OrderStatus.Shipped:
                    order.Ship();
                    break;
                case OrderStatus.Delivered:
                    order.Deliver();
                    break;
                case OrderStatus.Cancelled:
                    order.Cancel();
                    break;
                default:
                    return UpdateOrderStatusResult.Failure("Invalid status transition");
            }
            
            // Guardar cambios
            await _orderRepository.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();
            
            // Enviar notificaciones según el cambio de estado
            await SendStatusChangeNotificationAsync(order, oldStatus, request.NewStatus, request.Notes);
            
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
    
    private async Task SendStatusChangeNotificationAsync(Order order, OrderStatus oldStatus, OrderStatus newStatus, string? notes)
    {
        try
        {
            switch (newStatus)
            {
                case OrderStatus.Confirmed:
                    await _emailService.SendOrderConfirmedAsync(order.CustomerId.ToString(), order.Id);
                    break;
                case OrderStatus.Shipped:
                    await _emailService.SendOrderShippedAsync(order.CustomerId.ToString(), order.Id);
                    break;
                case OrderStatus.Delivered:
                    await _emailService.SendOrderDeliveredAsync(order.CustomerId.ToString(), order.Id);
                    break;
                case OrderStatus.Cancelled:
                    await _emailService.SendOrderCancelledAsync(order.CustomerId.ToString(), order.Id, notes);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send status change notification for order {OrderId}", order.Id);
        }
    }
}
```

### 2. Queries (Consultas)

Las consultas solo leen datos y no modifican el estado del sistema. Pueden usar modelos de datos optimizados para lectura.

```csharp
// Application/Queries/GetOrderDetails/GetOrderDetailsQuery.cs
public class GetOrderDetailsQuery : IRequest<OrderDetailsDto>
{
    public Guid OrderId { get; set; }
}

public class OrderDetailsDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public List<OrderItemDetailsDto> Items { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; }
    public string ShippingAddress { get; set; }
    public string PaymentMethod { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}

public class OrderItemDetailsDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public string ProductDescription { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}

// Application/Queries/GetOrderDetails/GetOrderDetailsQueryHandler.cs
public class GetOrderDetailsQueryHandler : IRequestHandler<GetOrderDetailsQuery, OrderDetailsDto>
{
    private readonly IOrderQueryService _orderQueryService;
    private readonly ILogger<GetOrderDetailsQueryHandler> _logger;
    
    public GetOrderDetailsQueryHandler(
        IOrderQueryService orderQueryService,
        ILogger<GetOrderDetailsQueryHandler> logger)
    {
        _orderQueryService = orderQueryService;
        _logger = logger;
    }
    
    public async Task<OrderDetailsDto> Handle(GetOrderDetailsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting details for order {OrderId}", request.OrderId);
            
            var orderDetails = await _orderQueryService.GetOrderDetailsAsync(request.OrderId);
            
            if (orderDetails == null)
            {
                _logger.LogWarning("Order {OrderId} not found", request.OrderId);
                return null;
            }
            
            _logger.LogInformation("Successfully retrieved details for order {OrderId}", request.OrderId);
            return orderDetails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting details for order {OrderId}", request.OrderId);
            throw;
        }
    }
}

// Application/Queries/GetCustomerOrders/GetCustomerOrdersQuery.cs
public class GetCustomerOrdersQuery : IRequest<IEnumerable<CustomerOrderDto>>
{
    public Guid CustomerId { get; set; }
    public OrderStatus? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class CustomerOrderDto
{
    public Guid Id { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ItemCount { get; set; }
    public string ShippingAddress { get; set; }
}

// Application/Queries/GetCustomerOrders/GetCustomerOrdersQueryHandler.cs
public class GetCustomerOrdersQueryHandler : IRequestHandler<GetCustomerOrdersQuery, IEnumerable<CustomerOrderDto>>
{
    private readonly IOrderQueryService _orderQueryService;
    private readonly ILogger<GetCustomerOrdersQueryHandler> _logger;
    
    public GetCustomerOrdersQueryHandler(
        IOrderQueryService orderQueryService,
        ILogger<GetCustomerOrdersQueryHandler> logger)
    {
        _orderQueryService = orderQueryService;
        _logger = logger;
    }
    
    public async Task<IEnumerable<CustomerOrderDto>> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting orders for customer {CustomerId}", request.CustomerId);
            
            var orders = await _orderQueryService.GetCustomerOrdersAsync(
                request.CustomerId, 
                request.Status, 
                request.DateFrom, 
                request.DateTo);
            
            _logger.LogInformation("Successfully retrieved {Count} orders for customer {CustomerId}", 
                orders.Count(), request.CustomerId);
            
            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for customer {CustomerId}", request.CustomerId);
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
}

public class OrderFilter
{
    public Guid? CustomerId { get; set; }
    public OrderStatus? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public decimal? MinTotal { get; set; }
    public decimal? MaxTotal { get; set; }
}

public class OrderSummaryDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ItemCount { get; set; }
}

// Application/Queries/GetOrdersPaged/GetOrdersPagedQueryHandler.cs
public class GetOrdersPagedQueryHandler : IRequestHandler<GetOrdersPagedQuery, PagedResult<OrderSummaryDto>>
{
    private readonly IOrderQueryService _orderQueryService;
    private readonly ILogger<GetOrdersPagedQueryHandler> _logger;
    
    public GetOrdersPagedQueryHandler(
        IOrderQueryService orderQueryService,
        ILogger<GetOrdersPagedQueryHandler> logger)
    {
        _orderQueryService = orderQueryService;
        _logger = logger;
    }
    
    public async Task<PagedResult<OrderSummaryDto>> Handle(GetOrdersPagedQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting paged orders with filter: {@Filter}", request.Filter);
            
            var result = await _orderQueryService.GetOrdersPagedAsync(
                request.Filter,
                request.Page,
                request.PageSize,
                request.SortBy,
                request.SortDescending);
            
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

### 3. Query Services

Los Query Services implementan la lógica de consulta optimizada para lectura, separada de la lógica de escritura.

```csharp
// Application/Queries/Services/IOrderQueryService.cs
public interface IOrderQueryService
{
    Task<OrderDetailsDto> GetOrderDetailsAsync(Guid orderId);
    Task<IEnumerable<CustomerOrderDto>> GetCustomerOrdersAsync(
        Guid customerId, 
        OrderStatus? status = null, 
        DateTime? dateFrom = null, 
        DateTime? dateTo = null);
    Task<PagedResult<OrderSummaryDto>> GetOrdersPagedAsync(
        OrderFilter filter, 
        int page, 
        int pageSize, 
        string sortBy = "CreatedAt", 
        bool sortDescending = true);
}

// Application/Queries/Services/OrderQueryService.cs
public class OrderQueryService : IOrderQueryService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OrderQueryService> _logger;
    
    public OrderQueryService(ApplicationDbContext context, ILogger<OrderQueryService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<OrderDetailsDto> GetOrderDetailsAsync(Guid orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Customer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);
            
        if (order == null)
            return null;
            
        return new OrderDetailsDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer.Name,
            CustomerEmail = order.Customer.Email,
            Items = order.Items.Select(i => new OrderItemDetailsDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                ProductDescription = i.Product.Description,
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
    }
    
    public async Task<IEnumerable<CustomerOrderDto>> GetCustomerOrdersAsync(
        Guid customerId, 
        OrderStatus? status = null, 
        DateTime? dateFrom = null, 
        DateTime? dateTo = null)
    {
        var query = _context.Orders
            .Where(o => o.CustomerId == customerId);
            
        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);
            
        if (dateFrom.HasValue)
            query = query.Where(o => o.CreatedAt >= dateFrom.Value);
            
        if (dateTo.HasValue)
            query = query.Where(o => o.CreatedAt <= dateTo.Value);
            
        return await query
            .Select(o => new CustomerOrderDto
            {
                Id = o.Id,
                Total = o.Total,
                Status = o.Status.ToString(),
                CreatedAt = o.CreatedAt,
                ItemCount = o.Items.Count,
                ShippingAddress = o.ShippingAddress
            })
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<PagedResult<OrderSummaryDto>> GetOrdersPagedAsync(
        OrderFilter filter, 
        int page, 
        int pageSize, 
        string sortBy = "CreatedAt", 
        bool sortDescending = true)
    {
        var query = _context.Orders.AsQueryable();
        
        // Aplicar filtros
        if (filter.CustomerId.HasValue)
            query = query.Where(o => o.CustomerId == filter.CustomerId.Value);
            
        if (filter.Status.HasValue)
            query = query.Where(o => o.Status == filter.Status.Value);
            
        if (filter.DateFrom.HasValue)
            query = query.Where(o => o.CreatedAt >= filter.DateFrom.Value);
            
        if (filter.DateTo.HasValue)
            query = query.Where(o => o.CreatedAt <= filter.DateTo.Value);
            
        if (filter.MinTotal.HasValue)
            query = query.Where(o => o.Total >= filter.MinTotal.Value);
            
        if (filter.MaxTotal.HasValue)
            query = query.Where(o => o.Total <= filter.MaxTotal.Value);
        
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
                Status = o.Status.ToString(),
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
    
    private IQueryable<Order> ApplySorting(IQueryable<Order> query, string sortBy, bool sortDescending)
    {
        return sortBy.ToLower() switch
        {
            "total" => sortDescending ? query.OrderByDescending(o => o.Total) : query.OrderBy(o => o.Total),
            "status" => sortDescending ? query.OrderByDescending(o => o.Status) : query.OrderBy(o => o.Status),
            "customername" => sortDescending ? query.OrderByDescending(o => o.Customer.Name) : query.OrderBy(o => o.Customer.Name),
            _ => sortDescending ? query.OrderByDescending(o => o.CreatedAt) : query.OrderBy(o => o.CreatedAt)
        };
    }
}
```

### 4. MediatR Integration

MediatR es una biblioteca que implementa el patrón Mediator y facilita la implementación de CQRS.

```csharp
// Program.cs - Configuración de MediatR
public void ConfigureServices(IServiceCollection services)
{
    // Registrar MediatR
    services.AddMediatR(cfg => 
    {
        cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly);
    });
    
    // Registrar handlers
    services.AddScoped<IRequestHandler<CreateOrderCommand, CreateOrderResult>, CreateOrderCommandHandler>();
    services.AddScoped<IRequestHandler<UpdateOrderStatusCommand, UpdateOrderStatusResult>, UpdateOrderStatusCommandHandler>();
    services.AddScoped<IRequestHandler<GetOrderDetailsQuery, OrderDetailsDto>, GetOrderDetailsQueryHandler>();
    services.AddScoped<IRequestHandler<GetCustomerOrdersQuery, IEnumerable<CustomerOrderDto>>, GetCustomerOrdersQueryHandler>();
    services.AddScoped<IRequestHandler<GetOrdersPagedQuery, PagedResult<OrderSummaryDto>>, GetOrdersPagedQueryHandler>();
    
    // Registrar query service
    services.AddScoped<IOrderQueryService, OrderQueryService>();
}

// Controllers usando MediatR
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;
    
    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
    
    [HttpPost]
    public async Task<ActionResult<CreateOrderResult>> CreateOrder(CreateOrderCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
            return CreatedAtAction(nameof(GetOrder), new { id = result.OrderId }, result);
        else
            return BadRequest(result.ErrorMessage);
    }
    
    [HttpPut("{id}/status")]
    public async Task<ActionResult> UpdateStatus(Guid id, UpdateOrderStatusCommand command)
    {
        command.OrderId = id;
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
            return NoContent();
        else
            return BadRequest(result.ErrorMessage);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDetailsDto>> GetOrder(Guid id)
    {
        var query = new GetOrderDetailsQuery { OrderId = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }
    
    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<CustomerOrderDto>>> GetCustomerOrders(
        Guid customerId, 
        [FromQuery] OrderStatus? status,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        var query = new GetCustomerOrdersQuery
        {
            CustomerId = customerId,
            Status = status,
            DateFrom = dateFrom,
            DateTo = dateTo
        };
        
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    
    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderSummaryDto>>> GetOrders(
        [FromQuery] OrderFilter filter,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "CreatedAt",
        [FromQuery] bool sortDescending = true)
    {
        var query = new GetOrdersPagedQuery
        {
            Filter = filter,
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDescending = sortDescending
        };
        
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
```

## Ejercicios Prácticos

### Ejercicio 1: Implementar CQRS para Productos
Crea comandos y consultas para la gestión de productos siguiendo el patrón CQRS.

### Ejercicio 2: Optimizar Consultas
Implementa consultas optimizadas para diferentes escenarios de lectura (listado, búsqueda, filtros).

### Ejercicio 3: Validaciones de Comandos
Agrega validaciones a los comandos usando FluentValidation.

## Resumen

En esta clase hemos aprendido:

1. **Patrón CQRS**: Separación de comandos (escritura) y consultas (lectura)
2. **Commands**: Operaciones que modifican el estado del sistema
3. **Queries**: Operaciones que solo leen datos
4. **Query Services**: Servicios optimizados para consultas
5. **MediatR**: Biblioteca para implementar el patrón Mediator

En la siguiente clase continuaremos con **Domain Events** para implementar comunicación asíncrona entre componentes del dominio.

## Recursos Adicionales
- [CQRS Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [MediatR](https://github.com/jbogard/MediatR)
- [CQRS with MediatR](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/implement-reads-queries)

