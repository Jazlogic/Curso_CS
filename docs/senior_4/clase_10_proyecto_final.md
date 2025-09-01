# Clase 10: Proyecto Final Integrador

## Navegación
- [← Clase 9: Seguridad en Microservicios](clase_9_seguridad_microservicios.md)
- [← Volver al README del módulo](README.md)
- [← Volver al módulo anterior (senior_3)](../senior_3/README.md)
- [→ Ir al siguiente módulo (senior_5)](../senior_5/README.md)

## Objetivos de Aprendizaje
- Integrar todos los conceptos aprendidos en el módulo
- Implementar un sistema de microservicios completo
- Aplicar patrones de arquitectura avanzados
- Implementar observabilidad y monitoreo
- Desplegar y orquestar el sistema completo

## Proyecto: Plataforma de E-commerce Distribuida

### 1. Arquitectura del Sistema

```csharp
// Arquitectura general del sistema
public class EcommerceArchitecture
{
    // Microservicios principales
    public void DefineMicroservices()
    {
        // 1. User Service - Gestión de usuarios y autenticación
        // 2. Product Service - Catálogo de productos
        // 3. Order Service - Gestión de pedidos
        // 4. Payment Service - Procesamiento de pagos
        // 5. Inventory Service - Control de inventario
        // 6. Notification Service - Envío de notificaciones
        // 7. Analytics Service - Análisis y reportes
    }
    
    // Patrones de comunicación
    public void DefineCommunicationPatterns()
    {
        // Síncrona: HTTP/REST para operaciones críticas
        // Asíncrona: Message Broker para eventos de dominio
        // Event Sourcing: Para auditoría y trazabilidad
        // CQRS: Para separar lecturas y escrituras
    }
}
```

### 2. Implementación del User Service

```csharp
// User Service - Gestión de usuarios
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterUserRequest request)
    {
        try
        {
            var user = await _userService.RegisterUserAsync(request);
            
            // Publicar evento de dominio
            await _eventPublisher.PublishAsync(new UserRegisteredEvent
            {
                UserId = user.Id,
                Email = user.Email,
                Timestamp = DateTime.UtcNow
            });
            
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var result = await _userService.AuthenticateAsync(request);
        
        if (result.Success)
        {
            return Ok(new AuthResponse
            {
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken,
                User = result.User
            });
        }
        
        return Unauthorized(result.Message);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        
        if (user == null)
            return NotFound();
            
        return Ok(user);
    }
}

// Implementación del servicio
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IEncryptionService encryptionService,
        IEventPublisher eventPublisher,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _encryptionService = encryptionService;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<UserDto> RegisterUserAsync(RegisterUserRequest request)
    {
        // Validar email único
        if (await _userRepository.ExistsByEmailAsync(request.Email))
        {
            throw new ValidationException("Email already exists");
        }

        // Crear usuario
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Name = request.Name,
            PasswordHash = _encryptionService.HashPassword(request.Password),
            Role = "Customer",
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);
        
        _logger.LogInformation("User registered: {UserId}", user.Id);
        
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<AuthResult> AuthenticateAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        
        if (user == null || !_encryptionService.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed for email: {Email}", request.Email);
            return AuthResult.Failure("Invalid credentials");
        }

        var accessToken = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();
        
        await _userRepository.SaveRefreshTokenAsync(user.Id, refreshToken);
        
        return AuthResult.Success(accessToken, refreshToken, user);
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
        
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### 3. Implementación del Product Service

```csharp
// Product Service - Catálogo de productos
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts(
        [FromQuery] ProductFilter filter,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var products = await _productService.GetProductsAsync(filter, page, pageSize);
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        
        if (product == null)
            return NotFound();
            
        return Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductRequest request)
    {
        var product = await _productService.CreateProductAsync(request);
        
        // Publicar evento de dominio
        await _eventPublisher.PublishAsync(new ProductCreatedEvent
        {
            ProductId = product.Id,
            Name = product.Name,
            Price = product.Price,
            Timestamp = DateTime.UtcNow
        });
        
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, UpdateProductRequest request)
    {
        var product = await _productService.UpdateProductAsync(id, request);
        
        if (product == null)
            return NotFound();
            
        return Ok(product);
    }
}

// Implementación del servicio
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository productRepository,
        IEventPublisher eventPublisher,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(
        ProductFilter filter, 
        int page, 
        int pageSize)
    {
        var query = _productRepository.GetQueryable();
        
        // Aplicar filtros
        if (!string.IsNullOrEmpty(filter.Name))
            query = query.Where(p => p.Name.Contains(filter.Name));
            
        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.Price >= filter.MinPrice.Value);
            
        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);
            
        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

        // Paginación
        var totalCount = await query.CountAsync();
        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                CategoryId = p.CategoryId,
                StockQuantity = p.StockQuantity,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<ProductDto>
        {
            Items = products,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            CategoryId = request.CategoryId,
            StockQuantity = request.StockQuantity,
            CreatedAt = DateTime.UtcNow
        };

        await _productRepository.CreateAsync(product);
        
        _logger.LogInformation("Product created: {ProductId}", product.Id);
        
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            CategoryId = product.CategoryId,
            StockQuantity = product.StockQuantity,
            CreatedAt = product.CreatedAt
        };
    }
}
```

### 4. Implementación del Order Service

```csharp
// Order Service - Gestión de pedidos
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
    [Authorize]
    public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderRequest request)
    {
        try
        {
            var order = await _orderService.CreateOrderAsync(request, User.GetUserId());
            
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InsufficientStockException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id)
    {
        var order = await _orderService.GetOrderByIdAsync(id, User.GetUserId());
        
        if (order == null)
            return NotFound();
            
        return Ok(order);
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetUserOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var orders = await _orderService.GetUserOrdersAsync(User.GetUserId(), page, pageSize);
        return Ok(orders);
    }

    [HttpPost("{id}/cancel")]
    [Authorize]
    public async Task<ActionResult> CancelOrder(Guid id)
    {
        try
        {
            await _orderService.CancelOrderAsync(id, User.GetUserId());
            return NoContent();
        }
        catch (OrderNotFoundException)
        {
            return NotFound();
        }
        catch (OrderCannotBeCancelledException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

// Implementación del servicio
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductServiceClient _productServiceClient;
    private readonly IInventoryServiceClient _inventoryServiceClient;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IProductServiceClient productServiceClient,
        IInventoryServiceClient inventoryServiceClient,
        IEventPublisher eventPublisher,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _productServiceClient = productServiceClient;
        _inventoryServiceClient = inventoryServiceClient;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, Guid userId)
    {
        // Validar productos
        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await _productServiceClient.GetProductsByIdsAsync(productIds);
        
        if (products.Count != productIds.Count)
        {
            throw new ValidationException("Some products not found");
        }

        // Verificar stock
        foreach (var item in request.Items)
        {
            var product = products.First(p => p.Id == item.ProductId);
            if (product.StockQuantity < item.Quantity)
            {
                throw new InsufficientStockException($"Insufficient stock for product {product.Name}");
            }
        }

        // Crear orden
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = OrderStatus.Pending,
            TotalAmount = CalculateTotal(request.Items, products),
            CreatedAt = DateTime.UtcNow,
            Items = request.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = products.First(p => p.Id == i.ProductId).Price
            }).ToList()
        };

        await _orderRepository.CreateAsync(order);
        
        // Reservar inventario
        await _inventoryServiceClient.ReserveStockAsync(order.Id, request.Items);
        
        // Publicar evento de dominio
        await _eventPublisher.PublishAsync(new OrderCreatedEvent
        {
            OrderId = order.Id,
            UserId = userId,
            TotalAmount = order.TotalAmount,
            Timestamp = DateTime.UtcNow
        });
        
        _logger.LogInformation("Order created: {OrderId} for user {UserId}", order.Id, userId);
        
        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            Items = order.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList(),
            CreatedAt = order.CreatedAt
        };
    }

    private decimal CalculateTotal(List<OrderItemRequest> items, List<ProductDto> products)
    {
        return items.Sum(item =>
        {
            var product = products.First(p => p.Id == item.ProductId);
            return product.Price * item.Quantity;
        });
    }
}
```

### 5. Implementación de Event Sourcing y CQRS

```csharp
// Event Store
public interface IEventStore
{
    Task SaveEventsAsync(Guid aggregateId, IEnumerable<IDomainEvent> events, int expectedVersion);
    Task<IEnumerable<IDomainEvent>> GetEventsAsync(Guid aggregateId);
}

public class EventStore : IEventStore
{
    private readonly IEventRepository _eventRepository;
    private readonly ILogger<EventStore> _logger;

    public EventStore(IEventRepository eventRepository, ILogger<EventStore> logger)
    {
        _eventRepository = eventRepository;
        _logger = logger;
    }

    public async Task SaveEventsAsync(Guid aggregateId, IEnumerable<IDomainEvent> events, int expectedVersion)
    {
        var eventList = events.ToList();
        var version = expectedVersion;

        foreach (var @event in eventList)
        {
            version++;
            @event.Version = version;
            @event.AggregateId = aggregateId;
            @event.Timestamp = DateTime.UtcNow;
        }

        await _eventRepository.SaveEventsAsync(aggregateId, eventList, expectedVersion);
        
        _logger.LogInformation("Saved {EventCount} events for aggregate {AggregateId}", 
            eventList.Count, aggregateId);
    }

    public async Task<IEnumerable<IDomainEvent>> GetEventsAsync(Guid aggregateId)
    {
        var events = await _eventRepository.GetEventsAsync(aggregateId);
        
        _logger.LogInformation("Retrieved {EventCount} events for aggregate {AggregateId}", 
            events.Count(), aggregateId);
            
        return events;
    }
}

// Aggregate base class
public abstract class EventSourcedAggregate
{
    private readonly List<IDomainEvent> _uncommittedEvents = new();
    public Guid Id { get; protected set; }
    public int Version { get; protected set; }

    protected void Apply(IDomainEvent @event)
    {
        _uncommittedEvents.Add(@event);
        When(@event);
        Version++;
    }

    protected abstract void When(IDomainEvent @event);

    public IEnumerable<IDomainEvent> GetUncommittedEvents()
    {
        return _uncommittedEvents.ToList();
    }

    public void MarkEventsAsCommitted()
    {
        _uncommittedEvents.Clear();
    }
}

// Order aggregate
public class OrderAggregate : EventSourcedAggregate
{
    public Guid UserId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public List<OrderItem> Items { get; private set; } = new();
    public DateTime CreatedAt { get; private set; }

    public OrderAggregate() { }

    public OrderAggregate(Guid id, Guid userId, List<OrderItem> items, decimal totalAmount)
    {
        Apply(new OrderCreatedEvent
        {
            OrderId = id,
            UserId = userId,
            Items = items,
            TotalAmount = totalAmount,
            Timestamp = DateTime.UtcNow
        });
    }

    public void Cancel()
    {
        if (Status != OrderStatus.Pending)
        {
            throw new OrderCannotBeCancelledException("Order cannot be cancelled in current status");
        }

        Apply(new OrderCancelledEvent
        {
            OrderId = Id,
            Timestamp = DateTime.UtcNow
        });
    }

    protected override void When(IDomainEvent @event)
    {
        switch (@event)
        {
            case OrderCreatedEvent e:
                Id = e.OrderId;
                UserId = e.UserId;
                Items = e.Items;
                TotalAmount = e.TotalAmount;
                Status = OrderStatus.Pending;
                CreatedAt = e.Timestamp;
                break;
                
            case OrderCancelledEvent e:
                Status = OrderStatus.Cancelled;
                break;
        }
    }
}
```

### 6. Implementación de Monitoreo y Observabilidad

```csharp
// Health checks
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDbConnection _connection;

    public DatabaseHealthCheck(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT 1";
            command.CommandTimeout = 5;
            
            await command.ExecuteScalarAsync(cancellationToken);
            
            return HealthCheckResult.Healthy("Database is responding");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database is not responding", ex);
        }
    }
}

// Métricas personalizadas
public class OrderMetrics
{
    private readonly Counter _orderCreatedCounter;
    private readonly Counter _orderCancelledCounter;
    private readonly Histogram _orderProcessingTime;
    private readonly Gauge _activeOrders;

    public OrderMetrics()
    {
        _orderCreatedCounter = Metrics.CreateCounter(
            "orders_created_total",
            "Total number of orders created");

        _orderCancelledCounter = Metrics.CreateCounter(
            "orders_cancelled_total",
            "Total number of orders cancelled");

        _orderProcessingTime = Metrics.CreateHistogram(
            "order_processing_time_seconds",
            "Order processing time in seconds");

        _activeOrders = Metrics.CreateGauge(
            "active_orders",
            "Number of active orders");
    }

    public void RecordOrderCreated()
    {
        _orderCreatedCounter.Inc();
    }

    public void RecordOrderCancelled()
    {
        _orderCancelledCounter.Inc();
    }

    public void RecordProcessingTime(double seconds)
    {
        _orderProcessingTime.Observe(seconds);
    }

    public void SetActiveOrders(int count)
    {
        _activeOrders.Set(count);
    }
}

// Middleware de logging estructurado
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
            ?? Guid.NewGuid().ToString();
        
        LogContext.PushProperty("CorrelationId", correlationId);
        LogContext.PushProperty("UserId", context.User?.Identity?.Name ?? "anonymous");
        LogContext.PushProperty("RequestPath", context.Request.Path);
        LogContext.PushProperty("RequestMethod", context.Request.Method);

        var sw = Stopwatch.StartNew();
        
        try
        {
            await _next(context);
            
            sw.Stop();
            
            _logger.LogInformation(
                "Request completed in {ElapsedMs}ms with status {StatusCode}",
                sw.ElapsedMilliseconds,
                context.Response.StatusCode);
        }
        catch (Exception ex)
        {
            sw.Stop();
            
            _logger.LogError(
                ex,
                "Request failed after {ElapsedMs}ms",
                sw.ElapsedMilliseconds);
            
            throw;
        }
    }
}
```

### 7. Docker y Kubernetes

```dockerfile
# Dockerfile para microservicios
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["UserService/UserService.csproj", "UserService/"]
RUN dotnet restore "UserService/UserService.csproj"
COPY . .
RUN dotnet build "UserService/UserService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserService/UserService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
HEALTHCHECK --interval=30s --timeout=3s CMD curl -f http://localhost/health || exit 1
ENTRYPOINT ["dotnet", "UserService.dll"]
```

```yaml
# Kubernetes deployment
apiVersion: apps/v1
kind: Deployment
metadata:
  name: userservice
spec:
  replicas: 3
  selector:
    matchLabels:
      app: userservice
  template:
    metadata:
      labels:
        app: userservice
    spec:
      containers:
      - name: userservice
        image: userservice:latest
        ports:
        - containerPort: 80
        livenessProbe:
          httpGet:
            path: /health/live
            port: 80
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 80
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: userservice-secrets
              key: database-connection
        - name: Jwt__SecretKey
          valueFrom:
            secretKeyRef:
              name: userservice-secrets
              key: jwt-secret
```

### 8. CI/CD Pipeline

```yaml
# GitHub Actions workflow
name: E-commerce CI/CD
on:
  push:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    - name: Test
      run: dotnet test --configuration Release

  build-and-deploy:
    runs-on: ubuntu-latest
    needs: test
    steps:
    - uses: actions/checkout@v4
    - name: Build and push Docker images
      uses: docker/build-push-action@v5
      with:
        context: .
        push: true
        tags: userservice:latest,productservice:latest,orderservice:latest
    - name: Deploy to Kubernetes
      run: |
        kubectl set image deployment/userservice userservice=userservice:latest
        kubectl set image deployment/productservice productservice=productservice:latest
        kubectl set image deployment/orderservice orderservice=orderservice:latest
```

## Ejercicios Prácticos

### Ejercicio 1: Implementar Microservicio
Crea un microservicio completo con autenticación, autorización y eventos de dominio.

### Ejercicio 2: Event Sourcing
Implementa un agregado con Event Sourcing y CQRS.

### Ejercicio 3: Monitoreo
Configura health checks, métricas y logging estructurado.

## Proyecto Final
Implementa la plataforma de e-commerce completa que incluya:
- Todos los microservicios principales
- Event Sourcing y CQRS
- Autenticación y autorización
- Monitoreo y observabilidad
- Despliegue en Kubernetes
- Pipeline de CI/CD

## Recursos Adicionales
- [Microservices Patterns](https://microservices.io/patterns/)
- [Event Sourcing](https://martinfowler.com/eaaDev/EventSourcing.html)
- [CQRS](https://martinfowler.com/bliki/CQRS.html)
- [Kubernetes](https://kubernetes.io/docs/)
- [Docker](https://docs.docker.com/)

