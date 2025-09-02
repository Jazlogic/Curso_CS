# Clase 10: Proyecto Final Integrador

## Navegación
- [← Clase anterior: Testing Avanzado](clase_9_testing_avanzado.md)
- [← Volver al README del módulo](README.md)
- [← Volver al módulo anterior (senior_4)](../senior_4/README.md)
- [→ Ir al siguiente módulo (senior_6)](../senior_6/README.md)

## Objetivos de Aprendizaje
- Integrar todos los conceptos aprendidos en el módulo
- Implementar un sistema completo con Clean Architecture
- Aplicar CQRS, Event Sourcing y patrones avanzados
- Crear un proyecto real y funcional

## Proyecto: Sistema de Gestión de Órdenes Empresarial

### 1. Arquitectura del Sistema

```csharp
// Estructura del proyecto
EcommerceSystem/
├── src/
│   ├── EcommerceSystem.Domain/           # Entidades y reglas de negocio
│   ├── EcommerceSystem.Application/      # Casos de uso y comandos/queries
│   ├── EcommerceSystem.Infrastructure/  # Implementaciones técnicas
│   ├── EcommerceSystem.API/             # API REST
│   └── EcommerceSystem.Projections/     # Proyecciones para consultas
├── tests/
│   ├── EcommerceSystem.Domain.Tests/    # Pruebas del dominio
│   ├── EcommerceSystem.Application.Tests/ # Pruebas de aplicación
│   └── EcommerceSystem.Integration.Tests/ # Pruebas de integración
└── docker/                              # Configuración Docker
```

### 2. Implementación del Dominio

```csharp
// Domain/Entities/Order.cs
public class Order : EventSourcedAggregate
{
    public Guid CustomerId { get; private set; }
    public List<OrderItem> Items { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public decimal Total => Items.Sum(item => item.Total);
    
    private Order() { Items = new List<OrderItem>(); }
    
    public static Order Create(Guid customerId, List<OrderItem> items, string shippingAddress)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Items = items,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        
        order.ApplyChange(new OrderCreatedEvent(order.Id, customerId, items, shippingAddress));
        return order;
    }
    
    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Only pending orders can be confirmed");
            
        Status = OrderStatus.Confirmed;
        ApplyChange(new OrderConfirmedEvent(Id, DateTime.UtcNow));
    }
    
    protected override void Apply(DomainEvent @event)
    {
        switch (@event)
        {
            case OrderCreatedEvent e:
                Id = e.AggregateId;
                CustomerId = e.CustomerId;
                Items = e.Items;
                CreatedAt = DateTime.UtcNow;
                Status = OrderStatus.Pending;
                break;
            case OrderConfirmedEvent e:
                Status = OrderStatus.Confirmed;
                break;
        }
    }
}

// Domain/Events/OrderEvents.cs
public class OrderCreatedEvent : DomainEvent
{
    public Guid CustomerId { get; set; }
    public List<OrderItem> Items { get; set; }
    public string ShippingAddress { get; set; }
    
    public OrderCreatedEvent(Guid orderId, Guid customerId, List<OrderItem> items, string shippingAddress) 
        : base(orderId)
    {
        CustomerId = customerId;
        Items = items;
        ShippingAddress = shippingAddress;
    }
}

public class OrderConfirmedEvent : DomainEvent
{
    public DateTime ConfirmedAt { get; set; }
    
    public OrderConfirmedEvent(Guid orderId, DateTime confirmedAt) : base(orderId)
    {
        ConfirmedAt = confirmedAt;
    }
}
```

### 3. Implementación de CQRS

```csharp
// Application/Commands/CreateOrder/CreateOrderCommand.cs
public class CreateOrderCommand : IRequest<CreateOrderResult>
{
    public Guid CustomerId { get; set; }
    public List<OrderItemDto> Items { get; set; }
    public string ShippingAddress { get; set; }
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IEventSourcedRepository<Order> _orderRepository;
    private readonly IMessageBus _messageBus;
    
    public CreateOrderCommandHandler(IEventSourcedRepository<Order> orderRepository, IMessageBus messageBus)
    {
        _orderRepository = orderRepository;
        _messageBus = messageBus;
    }
    
    public async Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var orderItems = request.Items.Select(i => 
            OrderItem.Create(i.ProductId, i.Quantity, i.UnitPrice)).ToList();
        
        var order = Order.Create(request.CustomerId, orderItems, request.ShippingAddress);
        
        await _orderRepository.SaveAsync(order);
        
        await _messageBus.PublishAsync(new OrderCreatedIntegrationEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            Total = order.Total
        });
        
        return CreateOrderResult.Success(order.Id, order.Total);
    }
}

// Application/Queries/GetOrder/GetOrderQuery.cs
public class GetOrderQuery : IRequest<OrderDto>
{
    public Guid OrderId { get; set; }
}

public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderDto>
{
    private readonly IOrderProjection _orderProjection;
    
    public GetOrderQueryHandler(IOrderProjection orderProjection)
    {
        _orderProjection = orderProjection;
    }
    
    public async Task<OrderDto> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        return await _orderProjection.GetOrderAsync(request.OrderId);
    }
}
```

### 4. Implementación de Proyecciones

```csharp
// Projections/OrderProjection.cs
public class OrderProjection : IOrderProjection
{
    private readonly ApplicationDbContext _context;
    
    public OrderProjection(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<OrderDto> GetOrderAsync(Guid orderId)
    {
        var order = await _context.OrderProjections
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId);
            
        if (order == null) return null;
        
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
            Status = order.Status,
            CreatedAt = order.CreatedAt
        };
    }
}

// Projections/OrderProjectionService.cs
public class OrderProjectionService : IOrderProjectionService
{
    private readonly ApplicationDbContext _context;
    
    public OrderProjectionService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task ProcessEventAsync(DomainEvent @event)
    {
        switch (@event)
        {
            case OrderCreatedEvent e:
                await ProcessOrderCreatedEvent(e);
                break;
            case OrderConfirmedEvent e:
                await ProcessOrderConfirmedEvent(e);
                break;
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
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Total = i.Total
            }).ToList(),
            Total = @event.Items.Sum(i => i.Total),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            Version = @event.Version
        };
        
        await _context.OrderProjections.AddAsync(projection);
        await _context.SaveChangesAsync();
    }
    
    private async Task ProcessOrderConfirmedEvent(OrderConfirmedEvent @event)
    {
        var order = await _context.OrderProjections
            .FirstOrDefaultAsync(o => o.Id == @event.AggregateId);
            
        if (order != null)
        {
            order.Status = "Confirmed";
            order.Version = @event.Version;
            await _context.SaveChangesAsync();
        }
    }
}
```

### 5. Implementación de la API

```csharp
// API/Controllers/OrdersController.cs
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    public async Task<ActionResult<CreateOrderResult>> CreateOrder(CreateOrderCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
            return CreatedAtAction(nameof(GetOrder), new { id = result.OrderId }, result);
            
        return BadRequest(result.ErrorMessage);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id)
    {
        var query = new GetOrderQuery { OrderId = id };
        var order = await _mediator.Send(query);
        
        if (order == null)
            return NotFound();
            
        return Ok(order);
    }
    
    [HttpPut("{id}/confirm")]
    public async Task<ActionResult> ConfirmOrder(Guid id)
    {
        var command = new ConfirmOrderCommand { OrderId = id };
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
            return NoContent();
            
        return BadRequest(result.ErrorMessage);
    }
}
```

### 6. Configuración de la Aplicación

```csharp
// API/Program.cs
var builder = WebApplication.CreateBuilder(args);

// Configurar servicios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar MediatR
builder.Services.AddMediatR(typeof(CreateOrderCommand));

// Configurar Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar servicios de dominio
builder.Services.AddScoped<IEventSourcedRepository<Order>, EventSourcedRepository<Order>>();
builder.Services.AddScoped<IEventStore, EventStore>();
builder.Services.AddScoped<IOrderProjection, OrderProjection>();
builder.Services.AddScoped<IOrderProjectionService, OrderProjectionService>();

// Configurar Message Bus
builder.Services.AddScoped<IMessageBus, InMemoryMessageBus>();

// Configurar proyecciones
builder.Services.AddHostedService<ProjectionBackgroundService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### 7. Testing del Sistema

```csharp
// Tests/Integration/OrderFlowTests.cs
[TestFixture]
public class OrderFlowTests
{
    private TestServer _server;
    private HttpClient _client;
    
    [SetUp]
    public void Setup()
    {
        var builder = new WebHostBuilder()
            .UseStartup<TestStartup>();
        
        _server = new TestServer(builder);
        _client = _server.CreateClient();
    }
    
    [Test]
    public async Task CreateOrder_ShouldReturnCreatedOrder()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new OrderItemDto { ProductId = Guid.NewGuid(), Quantity = 2, UnitPrice = 10.99m }
            },
            ShippingAddress = "123 Main St"
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", command);
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CreateOrderResult>();
        Assert.That(result.IsSuccess, Is.True);
    }
    
    [Test]
    public async Task GetOrder_ShouldReturnOrderDetails()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        
        // Act
        var response = await _client.GetAsync($"/api/orders/{orderId}");
        
        // Assert
        if (response.IsSuccessStatusCode)
        {
            var order = await response.Content.ReadFromJsonAsync<OrderDto>();
            Assert.That(order, Is.Not.Null);
            Assert.That(order.Id, Is.EqualTo(orderId));
        }
    }
}
```

## Ejercicios del Proyecto Final

### Ejercicio 1: Implementar Customer Management
- Crear entidades Customer con Event Sourcing
- Implementar comandos CreateCustomer, UpdateCustomer
- Crear proyecciones para Customer

### Ejercicio 2: Implementar Product Management
- Crear entidades Product con inventario
- Implementar comandos para gestión de stock
- Crear proyecciones para catálogo de productos

### Ejercicio 3: Implementar Order Processing
- Crear workflow completo de órdenes
- Implementar validaciones de negocio
- Crear notificaciones por email

### Ejercicio 4: Implementar Reporting
- Crear proyecciones para reportes
- Implementar consultas agregadas
- Crear dashboard de administración

## Resumen del Módulo

En este módulo hemos aprendido:

1. **Clean Architecture**: Separación de responsabilidades y dependencias
2. **CQRS**: Separación de comandos y consultas
3. **Event Sourcing**: Almacenamiento de eventos para auditoría
4. **Domain Events**: Comunicación entre agregados
5. **Testing Avanzado**: Estrategias de pruebas robustas
6. **Proyecciones**: Modelos de lectura optimizados
7. **Message Bus**: Comunicación asíncrona entre servicios

## Recursos Adicionales

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [Event Sourcing](https://martinfowler.com/eaaDev/EventSourcing.html)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)

¡Felicidades por completar el módulo de Clean Architecture y Microservicios!

