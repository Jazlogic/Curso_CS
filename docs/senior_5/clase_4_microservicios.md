# Clase 4: Microservicios

## Navegación
- [← Clase anterior: Domain Events](clase_3_domain_events.md)
- [← Volver al README del módulo](README.md)
- [→ Siguiente clase: API Gateway](clase_5_api_gateway.md)

## Objetivos de Aprendizaje
- Comprender la arquitectura de microservicios
- Implementar servicios independientes y escalables
- Aplicar patrones de comunicación entre servicios
- Crear arquitecturas distribuidas robustas

## ¿Qué son los Microservicios?

Los **Microservicios** son un estilo de arquitectura que estructura una aplicación como una colección de servicios pequeños e independientes, cada uno ejecutándose en su propio proceso y comunicándose a través de mecanismos ligeros como HTTP/REST o mensajes asíncronos.

### Características de los Microservicios

```csharp
// 1. Servicios Independientes
// Cada microservicio puede ser desarrollado, desplegado y escalado independientemente
public class OrderService
{
    // Solo lógica relacionada con órdenes
    public async Task<Order> CreateOrderAsync(CreateOrderRequest request) { /* ... */ }
    public async Task<Order> GetOrderAsync(Guid id) { /* ... */ }
    public async Task UpdateOrderStatusAsync(Guid id, OrderStatus status) { /* ... */ }
}

// 2. Base de Datos por Servicio
// Cada servicio puede tener su propia base de datos
public class OrderDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    // Solo entidades relacionadas con órdenes
}

// 3. Comunicación a través de APIs
// Los servicios se comunican a través de contratos bien definidos
public interface IProductServiceClient
{
    Task<ProductDto> GetProductAsync(Guid id);
    Task<bool> ReserveStockAsync(Guid productId, int quantity);
}
```

## Arquitectura de Microservicios

### 1. Estructura de Solución

```csharp
// Estructura de directorios
Microservices/
├── OrderService/
│   ├── OrderService.API/           # API REST
│   ├── OrderService.Domain/        # Entidades y lógica de negocio
│   ├── OrderService.Infrastructure/# Implementaciones técnicas
│   └── OrderService.Tests/         # Pruebas unitarias
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
    ├── Shared.Messaging/           # Contratos de mensajería
    └── Shared.Contracts/           # DTOs compartidos

// OrderService.API/Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar servicios del dominio
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Configurar clientes HTTP para otros servicios
builder.Services.AddHttpClient<IProductServiceClient, ProductServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ProductService"]);
});

builder.Services.AddHttpClient<ICustomerServiceClient, CustomerServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:CustomerService"]);
});

// Configurar mensajería
builder.Services.AddScoped<IMessageBus, RabbitMQMessageBus>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### 2. Implementación del Order Service

```csharp
// OrderService.Domain/Entities/Order.cs
public class Order : Entity
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public List<OrderItem> Items { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
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
        
        return order;
    }
    
    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Only pending orders can be confirmed");
            
        Status = OrderStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
    }
    
    public void Cancel()
    {
        if (Status == OrderStatus.Delivered)
            throw new DomainException("Delivered orders cannot be cancelled");
            
        Status = OrderStatus.Cancelled;
    }
}

// OrderService.Domain/Repositories/IOrderRepository.cs
public interface IOrderRepository
{
    Task<Order> GetByIdAsync(Guid id);
    Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId);
    Task<Order> AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task<bool> ExistsAsync(Guid id);
}

// OrderService.Application/Services/IOrderService.cs
public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(CreateOrderRequest request);
    Task<OrderDto> GetOrderAsync(Guid id);
    Task<IEnumerable<OrderDto>> GetCustomerOrdersAsync(Guid customerId);
    Task UpdateOrderStatusAsync(Guid id, OrderStatus status);
}

// OrderService.Application/Services/OrderService.cs
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductServiceClient _productServiceClient;
    private readonly ICustomerServiceClient _customerServiceClient;
    private readonly IMessageBus _messageBus;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderService> _logger;
    
    public OrderService(
        IOrderRepository orderRepository,
        IProductServiceClient productServiceClient,
        ICustomerServiceClient customerServiceClient,
        IMessageBus messageBus,
        IUnitOfWork unitOfWork,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _productServiceClient = productServiceClient;
        _customerServiceClient = customerServiceClient;
        _messageBus = messageBus;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request)
    {
        try
        {
            _logger.LogInformation("Creating order for customer {CustomerId}", request.CustomerId);
            
            // Validar cliente
            var customer = await _customerServiceClient.GetCustomerAsync(request.CustomerId);
            if (customer == null)
                throw new ValidationException("Customer not found");
            
            // Validar productos y crear items
            var orderItems = new List<OrderItem>();
            var totalAmount = 0m;
            
            foreach (var itemRequest in request.Items)
            {
                var product = await _productServiceClient.GetProductAsync(itemRequest.ProductId);
                if (product == null)
                    throw new ValidationException($"Product {itemRequest.ProductId} not found");
                
                if (product.Stock < itemRequest.Quantity)
                    throw new ValidationException($"Insufficient stock for product {product.Name}");
                
                var orderItem = OrderItem.Create(itemRequest.ProductId, itemRequest.Quantity, product.Price);
                orderItems.Add(orderItem);
                totalAmount += orderItem.Total;
                
                // Reservar stock
                var stockReserved = await _productServiceClient.ReserveStockAsync(itemRequest.ProductId, itemRequest.Quantity);
                if (!stockReserved)
                    throw new ValidationException($"Failed to reserve stock for product {product.Name}");
            }
            
            // Crear la orden
            var order = Order.Create(request.CustomerId, orderItems, request.ShippingAddress, request.PaymentMethod);
            
            // Guardar la orden
            await _orderRepository.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();
            
            // Publicar evento de dominio
            var orderCreatedMessage = new OrderCreatedMessage
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                Total = totalAmount,
                CreatedAt = order.CreatedAt
            };
            
            await _messageBus.PublishAsync(orderCreatedMessage);
            
            _logger.LogInformation("Order {OrderId} created successfully for customer {CustomerId}", 
                order.Id, request.CustomerId);
            
            return new OrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                Items = order.Items.Select(i => new OrderItemDto
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
                CreatedAt = order.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for customer {CustomerId}", request.CustomerId);
            throw;
        }
    }
    
    public async Task<OrderDto> GetOrderAsync(Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            return null;
            
        return new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Items = order.Items.Select(i => new OrderItemDto
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
            CreatedAt = order.CreatedAt
        };
    }
    
    public async Task<IEnumerable<OrderDto>> GetCustomerOrdersAsync(Guid customerId)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(customerId);
        
        return orders.Select(o => new OrderDto
        {
            Id = o.Id,
            CustomerId = o.CustomerId,
            Items = o.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Total = i.Total
            }).ToList(),
            Total = o.Total,
            Status = o.Status.ToString(),
            ShippingAddress = o.ShippingAddress,
            PaymentMethod = o.PaymentMethod,
            CreatedAt = o.CreatedAt
        });
    }
    
    public async Task UpdateOrderStatusAsync(Guid id, OrderStatus status)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            throw new NotFoundException("Order not found");
        
        switch (status)
        {
            case OrderStatus.Confirmed:
                order.Confirm();
                break;
            case OrderStatus.Cancelled:
                order.Cancel();
                break;
            default:
                throw new ValidationException("Invalid status transition");
        }
        
        await _orderRepository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync();
        
        // Publicar evento de cambio de estado
        var statusChangedMessage = new OrderStatusChangedMessage
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            OldStatus = order.Status.ToString(),
            NewStatus = status.ToString(),
            ChangedAt = DateTime.UtcNow
        };
        
        await _messageBus.PublishAsync(statusChangedMessage);
    }
}
```

### 3. Clientes HTTP para Otros Servicios

```csharp
// OrderService.Infrastructure/Clients/IProductServiceClient.cs
public interface IProductServiceClient
{
    Task<ProductDto> GetProductAsync(Guid id);
    Task<bool> ReserveStockAsync(Guid productId, int quantity);
    Task<IEnumerable<ProductDto>> GetProductsAsync(IEnumerable<Guid> ids);
}

// OrderService.Infrastructure/Clients/ProductServiceClient.cs
public class ProductServiceClient : IProductServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductServiceClient> _logger;
    
    public ProductServiceClient(HttpClient httpClient, ILogger<ProductServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<ProductDto> GetProductAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/products/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ProductDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;
                
            response.EnsureSuccessStatusCode();
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product {ProductId} from ProductService", id);
            throw;
        }
    }
    
    public async Task<bool> ReserveStockAsync(Guid productId, int quantity)
    {
        try
        {
            var request = new ReserveStockRequest
            {
                ProductId = productId,
                Quantity = quantity
            };
            
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/products/reserve-stock", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ReserveStockResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                return result.Success;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reserving stock for product {ProductId}", productId);
            return false;
        }
    }
    
    public async Task<IEnumerable<ProductDto>> GetProductsAsync(IEnumerable<Guid> ids)
    {
        try
        {
            var idsList = ids.ToList();
            var queryString = string.Join("&", idsList.Select(id => $"ids={id}"));
            
            var response = await _httpClient.GetAsync($"/api/products/batch?{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<ProductDto>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            
            response.EnsureSuccessStatusCode();
            return Enumerable.Empty<ProductDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products batch from ProductService");
            throw;
        }
    }
}

// OrderService.Infrastructure/Clients/ICustomerServiceClient.cs
public interface ICustomerServiceClient
{
    Task<CustomerDto> GetCustomerAsync(Guid id);
    Task<bool> UpdateCustomerStatsAsync(Guid customerId, decimal orderTotal);
}

// OrderService.Infrastructure/Clients/CustomerServiceClient.cs
public class CustomerServiceClient : ICustomerServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomerServiceClient> _logger;
    
    public CustomerServiceClient(HttpClient httpClient, ILogger<CustomerServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<CustomerDto> GetCustomerAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/customers/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<CustomerDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;
                
            response.EnsureSuccessStatusCode();
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer {CustomerId} from CustomerService", id);
            throw;
        }
    }
    
    public async Task<bool> UpdateCustomerStatsAsync(Guid customerId, decimal orderTotal)
    {
        try
        {
            var request = new UpdateCustomerStatsRequest
            {
                CustomerId = customerId,
                OrderTotal = orderTotal
            };
            
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/customers/update-stats", content);
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer stats for customer {CustomerId}", customerId);
            return false;
        }
    }
}
```

### 4. Sistema de Mensajería

```csharp
// Shared.Messaging/Contracts/OrderMessages.cs
public class OrderCreatedMessage
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderStatusChangedMessage
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string OldStatus { get; set; }
    public string NewStatus { get; set; }
    public DateTime ChangedAt { get; set; }
}

// OrderService.Infrastructure/Messaging/IMessageBus.cs
public interface IMessageBus
{
    Task PublishAsync<T>(T message) where T : class;
    Task SubscribeAsync<T>(Func<T, Task> handler) where T : class;
}

// OrderService.Infrastructure/Messaging/RabbitMQMessageBus.cs
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
                
            _logger.LogInformation("Published message of type {MessageType}", typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message of type {MessageType}", typeof(T).Name);
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
                    _logger.LogError(ex, "Error processing message of type {MessageType}", typeof(T).Name);
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };
            
            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
            
            _logger.LogInformation("Subscribed to messages of type {MessageType}", typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to messages of type {MessageType}", typeof(T).Name);
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
```

### 5. Controladores de la API

```csharp
// OrderService.API/Controllers/OrdersController.cs
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;
    
    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }
    
    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderRequest request)
    {
        try
        {
            var order = await _orderService.CreateOrderAsync(request);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id)
    {
        var order = await _orderService.GetOrderAsync(id);
        
        if (order == null)
            return NotFound();
            
        return Ok(order);
    }
    
    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetCustomerOrders(Guid customerId)
    {
        var orders = await _orderService.GetCustomerOrdersAsync(customerId);
        return Ok(orders);
    }
    
    [HttpPut("{id}/status")]
    public async Task<ActionResult> UpdateOrderStatus(Guid id, UpdateOrderStatusRequest request)
    {
        try
        {
            await _orderService.UpdateOrderStatusAsync(id, request.Status);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status");
            return StatusCode(500, "Internal server error");
        }
    }
}
```

## Ejercicios Prácticos

### Ejercicio 1: Implementar Customer Service
Crea un microservicio completo para la gestión de clientes.

### Ejercicio 2: Implementar Product Service
Crea un microservicio para la gestión de productos con inventario.

### Ejercicio 3: Comunicación entre Servicios
Implementa la comunicación HTTP y por mensajes entre los microservicios.

## Resumen

En esta clase hemos aprendido:

1. **Arquitectura de Microservicios**: Servicios independientes y escalables
2. **Implementación de Servicios**: Estructura y lógica de negocio
3. **Comunicación HTTP**: Clientes para otros servicios
4. **Sistema de Mensajería**: Eventos asíncronos entre servicios
5. **APIs REST**: Controladores para exponer funcionalidad

En la siguiente clase continuaremos con **API Gateway** para enrutar y agregar requests a diferentes microservicios.

## Recursos Adicionales
- [Microservices Architecture](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/)
- [Microservices Patterns](https://microservices.io/patterns/)
- [Building Microservices](https://martinfowler.com/articles/microservices.html)


