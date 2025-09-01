# 🚀 Clase 10: Proyecto Final - Plataforma Empresarial

## 📋 Información de la Clase

- **Módulo**: Senior 1 - Arquitectura de Software Empresarial
- **Duración**: 2 horas
- **Nivel**: Senior
- **Prerrequisitos**: Completar Clase 9 (Arquitectura de Seguridad Enterprise)

## 🎯 Objetivos de Aprendizaje

- Integrar todos los conceptos del módulo en un proyecto real
- Aplicar arquitecturas empresariales avanzadas
- Implementar patrones de diseño enterprise
- Crear una plataforma escalable y mantenible

---

## 📚 Navegación del Módulo 6

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_arquitectura_limpia_avanzada.md) | Arquitectura Limpia Avanzada | |
| [Clase 2](clase_2_event_driven_architecture.md) | Event-Driven Architecture | |
| [Clase 3](clase_3_microservicios_avanzada.md) | Arquitectura de Microservicios Avanzada | |
| [Clase 4](clase_4_patrones_enterprise.md) | Patrones de Diseño Enterprise | |
| [Clase 5](clase_5_arquitectura_datos_avanzada.md) | Arquitectura de Datos Avanzada | |
| [Clase 6](clase_6_calidad_codigo_metricas.md) | Calidad del Código y Métricas | |
| [Clase 7](clase_7_monitoreo_observabilidad.md) | Monitoreo y Observabilidad | |
| [Clase 8](clase_8_arquitectura_evolutiva.md) | Arquitectura Evolutiva | |
| [Clase 9](clase_9_seguridad_enterprise.md) | Arquitectura de Seguridad Enterprise | ← Anterior |
| **Clase 10** | **Proyecto Final: Plataforma Empresarial** | ← Estás aquí |

**← [Volver al README del Módulo 6](../senior_1/README.md)**

---

## 📚 Contenido Teórico

### 1. Proyecto Final: Plataforma Empresarial

Vamos a crear una plataforma empresarial completa que integre todos los conceptos aprendidos en este módulo.

```csharp
// ===== PLATAFORMA EMPRESARIAL - IMPLEMENTACIÓN COMPLETA =====
namespace EnterprisePlatform
{
    // ===== DOMAIN LAYER =====
    namespace Domain.Entities
    {
        public class User : AggregateRoot
        {
            public string Id { get; private set; }
            public string Username { get; private set; }
            public string Email { get; private set; }
            public string FirstName { get; private set; }
            public string LastName { get; private set; }
            public bool IsActive { get; private set; }
            public bool IsLocked { get; private set; }
            public DateTime CreatedAt { get; private set; }
            public DateTime? LastLoginAt { get; private set; }
            public List<string> Roles { get; private set; }
            public List<string> Permissions { get; private set; }
            
            private User() { }
            
            public static User Create(string username, string email, string firstName, string lastName)
            {
                var user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = username,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    IsActive = true,
                    IsLocked = false,
                    CreatedAt = DateTime.UtcNow,
                    Roles = new List<string>(),
                    Permissions = new List<string>()
                };
                
                user.AddDomainEvent(new UserCreatedEvent(user.Id, user.Username, user.Email));
                return user;
            }
            
            public void AssignRole(string role)
            {
                if (!Roles.Contains(role))
                {
                    Roles.Add(role);
                    AddDomainEvent(new UserRoleAssignedEvent(Id, role));
                }
            }
            
            public void RemoveRole(string role)
            {
                if (Roles.Contains(role))
                {
                    Roles.Remove(role);
                    AddDomainEvent(new UserRoleRemovedEvent(Id, role));
                }
            }
            
            public void LockAccount(string reason)
            {
                IsLocked = true;
                AddDomainEvent(new UserAccountLockedEvent(Id, reason));
            }
            
            public void UnlockAccount()
            {
                IsLocked = false;
                AddDomainEvent(new UserAccountUnlockedEvent(Id));
            }
            
            public void UpdateLastLogin()
            {
                LastLoginAt = DateTime.UtcNow;
            }
        }
        
        public class Product : AggregateRoot
        {
            public string Id { get; private set; }
            public string Name { get; private set; }
            public string Description { get; private set; }
            public decimal Price { get; private set; }
            public int StockQuantity { get; private set; }
            public string Category { get; private set; }
            public bool IsActive { get; private set; }
            public DateTime CreatedAt { get; private set; }
            public DateTime? UpdatedAt { get; private set; }
            
            private Product() { }
            
            public static Product Create(string name, string description, decimal price, int stockQuantity, string category)
            {
                var product = new Product
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = name,
                    Description = description,
                    Price = price,
                    StockQuantity = stockQuantity,
                    Category = category,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                
                product.AddDomainEvent(new ProductCreatedEvent(product.Id, product.Name, product.Price));
                return product;
            }
            
            public void UpdateStock(int quantity)
            {
                if (StockQuantity + quantity < 0)
                {
                    throw new InvalidOperationException("Insufficient stock");
                }
                
                StockQuantity += quantity;
                UpdatedAt = DateTime.UtcNow;
                
                if (StockQuantity == 0)
                {
                    AddDomainEvent(new ProductOutOfStockEvent(Id, Name));
                }
                else if (StockQuantity <= 10)
                {
                    AddDomainEvent(new ProductLowStockEvent(Id, Name, StockQuantity));
                }
            }
            
            public void UpdatePrice(decimal newPrice)
            {
                if (newPrice < 0)
                {
                    throw new ArgumentException("Price cannot be negative");
                }
                
                var oldPrice = Price;
                Price = newPrice;
                UpdatedAt = DateTime.UtcNow;
                
                AddDomainEvent(new ProductPriceChangedEvent(Id, Name, oldPrice, newPrice));
            }
            
            public void Deactivate()
            {
                IsActive = false;
                UpdatedAt = DateTime.UtcNow;
                AddDomainEvent(new ProductDeactivatedEvent(Id, Name));
            }
        }
        
        public class Order : AggregateRoot
        {
            public string Id { get; private set; }
            public string UserId { get; private set; }
            public List<OrderItem> Items { get; private set; }
            public decimal TotalAmount { get; private set; }
            public OrderStatus Status { get; private set; }
            public DateTime CreatedAt { get; private set; }
            public DateTime? UpdatedAt { get; private set; }
            public string ShippingAddress { get; private set; }
            public string BillingAddress { get; private set; }
            
            private Order() { }
            
            public static Order Create(string userId, List<OrderItem> items, string shippingAddress, string billingAddress)
            {
                var order = new Order
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Items = items,
                    TotalAmount = items.Sum(item => item.TotalPrice),
                    Status = OrderStatus.Created,
                    CreatedAt = DateTime.UtcNow,
                    ShippingAddress = shippingAddress,
                    BillingAddress = billingAddress
                };
                
                order.AddDomainEvent(new OrderCreatedEvent(order.Id, order.UserId, order.TotalAmount));
                return order;
            }
            
            public void Confirm()
            {
                if (Status != OrderStatus.Created)
                {
                    throw new InvalidOperationException("Order cannot be confirmed in current status");
                }
                
                Status = OrderStatus.Confirmed;
                UpdatedAt = DateTime.UtcNow;
                AddDomainEvent(new OrderConfirmedEvent(Id, UserId));
            }
            
            public void Process()
            {
                if (Status != OrderStatus.Confirmed)
                {
                    throw new InvalidOperationException("Order cannot be processed in current status");
                }
                
                Status = OrderStatus.Processing;
                UpdatedAt = DateTime.UtcNow;
                AddDomainEvent(new OrderProcessingEvent(Id, UserId));
            }
            
            public void Ship()
            {
                if (Status != OrderStatus.Processing)
                {
                    throw new InvalidOperationException("Order cannot be shipped in current status");
                }
                
                Status = OrderStatus.Shipped;
                UpdatedAt = DateTime.UtcNow;
                AddDomainEvent(new OrderShippedEvent(Id, UserId));
            }
            
            public void Deliver()
            {
                if (Status != OrderStatus.Shipped)
                {
                    throw new InvalidOperationException("Order cannot be delivered in current status");
                }
                
                Status = OrderStatus.Delivered;
                UpdatedAt = DateTime.UtcNow;
                AddDomainEvent(new OrderDeliveredEvent(Id, UserId));
            }
            
            public void Cancel(string reason)
            {
                if (Status == OrderStatus.Delivered)
                {
                    throw new InvalidOperationException("Delivered orders cannot be cancelled");
                }
                
                Status = OrderStatus.Cancelled;
                UpdatedAt = DateTime.UtcNow;
                AddDomainEvent(new OrderCancelledEvent(Id, UserId, reason));
            }
        }
        
        public class OrderItem : ValueObject
        {
            public string ProductId { get; private set; }
            public string ProductName { get; private set; }
            public int Quantity { get; private set; }
            public decimal UnitPrice { get; private set; }
            public decimal TotalPrice { get; private set; }
            
            public OrderItem(string productId, string productName, int quantity, decimal unitPrice)
            {
                ProductId = productId;
                ProductName = productName;
                Quantity = quantity;
                UnitPrice = unitPrice;
                TotalPrice = quantity * unitPrice;
            }
            
            protected override IEnumerable<object> GetEqualityComponents()
            {
                yield return ProductId;
                yield return ProductName;
                yield return Quantity;
                yield return UnitPrice;
            }
        }
        
        public enum OrderStatus
        {
            Created,
            Confirmed,
            Processing,
            Shipped,
            Delivered,
            Cancelled
        }
    }
    
    // ===== DOMAIN EVENTS =====
    namespace Domain.Events
    {
        public class UserCreatedEvent : DomainEvent
        {
            public string UserId { get; }
            public string Username { get; }
            public string Email { get; }
            
            public UserCreatedEvent(string userId, string username, string email)
            {
                UserId = userId;
                Username = username;
                Email = email;
            }
        }
        
        public class UserRoleAssignedEvent : DomainEvent
        {
            public string UserId { get; }
            public string Role { get; }
            
            public UserRoleAssignedEvent(string userId, string role)
            {
                UserId = userId;
                Role = role;
            }
        }
        
        public class ProductCreatedEvent : DomainEvent
        {
            public string ProductId { get; }
            public string Name { get; }
            public decimal Price { get; }
            
            public ProductCreatedEvent(string productId, string name, decimal price)
            {
                ProductId = productId;
                Name = name;
                Price = price;
            }
        }
        
        public class OrderCreatedEvent : DomainEvent
        {
            public string OrderId { get; }
            public string UserId { get; }
            public decimal TotalAmount { get; }
            
            public OrderCreatedEvent(string orderId, string userId, decimal totalAmount)
            {
                OrderId = orderId;
                UserId = userId;
                TotalAmount = totalAmount;
            }
        }
    }
    
    // ===== APPLICATION LAYER =====
    namespace Application.UseCases
    {
        public interface ICreateUserUseCase
        {
            Task<CreateUserResult> ExecuteAsync(CreateUserRequest request);
        }
        
        public class CreateUserUseCase : ICreateUserUseCase
        {
            private readonly IUserRepository _userRepository;
            private readonly IEventBus _eventBus;
            private readonly ILogger<CreateUserUseCase> _logger;
            
            public CreateUserUseCase(
                IUserRepository userRepository,
                IEventBus eventBus,
                ILogger<CreateUserUseCase> logger)
            {
                _userRepository = userRepository;
                _eventBus = eventBus;
                _logger = logger;
            }
            
            public async Task<CreateUserResult> ExecuteAsync(CreateUserRequest request)
            {
                try
                {
                    // Validate request
                    if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Email))
                    {
                        return CreateUserResult.Failure("Username and email are required");
                    }
                    
                    // Check if user already exists
                    var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
                    if (existingUser != null)
                    {
                        return CreateUserResult.Failure("Username already exists");
                    }
                    
                    var existingEmail = await _userRepository.GetByEmailAsync(request.Email);
                    if (existingEmail != null)
                    {
                        return CreateUserResult.Failure("Email already exists");
                    }
                    
                    // Create user
                    var user = User.Create(request.Username, request.Email, request.FirstName, request.LastName);
                    
                    // Assign default role
                    user.AssignRole("User");
                    
                    // Save user
                    await _userRepository.AddAsync(user);
                    
                    // Publish events
                    foreach (var domainEvent in user.DomainEvents)
                    {
                        await _eventBus.PublishAsync(domainEvent);
                    }
                    
                    _logger.LogInformation("User created successfully: {Username}", user.Username);
                    
                    return CreateUserResult.Success(user.Id, user.Username);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create user: {Username}", request.Username);
                    return CreateUserResult.Failure("Failed to create user");
                }
            }
        }
        
        public class CreateUserRequest
        {
            public string Username { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }
        
        public class CreateUserResult
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
            public string UserId { get; set; }
            public string Username { get; set; }
            
            public static CreateUserResult Success(string userId, string username)
            {
                return new CreateUserResult
                {
                    IsSuccess = true,
                    UserId = userId,
                    Username = username
                };
            }
            
            public static CreateUserResult Failure(string message)
            {
                return new CreateUserResult
                {
                    IsSuccess = false,
                    Message = message
                };
            }
        }
        
        public interface ICreateProductUseCase
        {
            Task<CreateProductResult> ExecuteAsync(CreateProductRequest request);
        }
        
        public class CreateProductUseCase : ICreateProductUseCase
        {
            private readonly IProductRepository _productRepository;
            private readonly IEventBus _eventBus;
            private readonly ILogger<CreateProductUseCase> _logger;
            
            public CreateProductUseCase(
                IProductRepository productRepository,
                IEventBus eventBus,
                ILogger<CreateProductUseCase> logger)
            {
                _productRepository = productRepository;
                _eventBus = eventBus;
                _logger = logger;
            }
            
            public async Task<CreateProductResult> ExecuteAsync(CreateProductRequest request)
            {
                try
                {
                    // Validate request
                    if (string.IsNullOrEmpty(request.Name) || request.Price < 0 || request.StockQuantity < 0)
                    {
                        return CreateProductResult.Failure("Invalid product data");
                    }
                    
                    // Create product
                    var product = Product.Create(request.Name, request.Description, request.Price, request.StockQuantity, request.Category);
                    
                    // Save product
                    await _productRepository.AddAsync(product);
                    
                    // Publish events
                    foreach (var domainEvent in product.DomainEvents)
                    {
                        await _eventBus.PublishAsync(domainEvent);
                    }
                    
                    _logger.LogInformation("Product created successfully: {Name}", product.Name);
                    
                    return CreateProductResult.Success(product.Id, product.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create product: {Name}", request.Name);
                    return CreateProductResult.Failure("Failed to create product");
                }
            }
        }
        
        public class CreateProductRequest
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public int StockQuantity { get; set; }
            public string Category { get; set; }
        }
        
        public class CreateProductResult
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
            public string ProductId { get; set; }
            public string Name { get; set; }
            
            public static CreateProductResult Success(string productId, string name)
            {
                return new CreateProductResult
                {
                    IsSuccess = true,
                    ProductId = productId,
                    Name = name
                };
            }
            
            public static CreateProductResult Failure(string message)
            {
                return new CreateProductResult
                {
                    IsSuccess = false,
                    Message = message
                };
            }
        }
        
        public interface ICreateOrderUseCase
        {
            Task<CreateOrderResult> ExecuteAsync(CreateOrderRequest request);
        }
        
        public class CreateOrderUseCase : ICreateOrderUseCase
        {
            private readonly IOrderRepository _orderRepository;
            private readonly IProductRepository _productRepository;
            private readonly IUserRepository _userRepository;
            private readonly IEventBus _eventBus;
            private readonly ILogger<CreateOrderUseCase> _logger;
            
            public CreateOrderUseCase(
                IOrderRepository orderRepository,
                IProductRepository productRepository,
                IUserRepository userRepository,
                IEventBus eventBus,
                ILogger<CreateOrderUseCase> logger)
            {
                _orderRepository = orderRepository;
                _productRepository = productRepository;
                _userRepository = userRepository;
                _eventBus = eventBus;
                _logger = logger;
            }
            
            public async Task<CreateOrderResult> ExecuteAsync(CreateOrderRequest request)
            {
                try
                {
                    // Validate user
                    var user = await _userRepository.GetByIdAsync(request.UserId);
                    if (user == null || !user.IsActive)
                    {
                        return CreateOrderResult.Failure("Invalid user");
                    }
                    
                    // Validate products and create order items
                    var orderItems = new List<OrderItem>();
                    foreach (var itemRequest in request.Items)
                    {
                        var product = await _productRepository.GetByIdAsync(itemRequest.ProductId);
                        if (product == null || !product.IsActive)
                        {
                            return CreateOrderResult.Failure($"Product {itemRequest.ProductId} not found or inactive");
                        }
                        
                        if (product.StockQuantity < itemRequest.Quantity)
                        {
                            return CreateOrderResult.Failure($"Insufficient stock for product {product.Name}");
                        }
                        
                        var orderItem = new OrderItem(product.Id, product.Name, itemRequest.Quantity, product.Price);
                        orderItems.Add(orderItem);
                        
                        // Update product stock
                        product.UpdateStock(-itemRequest.Quantity);
                        await _productRepository.UpdateAsync(product);
                    }
                    
                    // Create order
                    var order = Order.Create(request.UserId, orderItems, request.ShippingAddress, request.BillingAddress);
                    
                    // Save order
                    await _orderRepository.AddAsync(order);
                    
                    // Publish events
                    foreach (var domainEvent in order.DomainEvents)
                    {
                        await _eventBus.PublishAsync(domainEvent);
                    }
                    
                    _logger.LogInformation("Order created successfully: {OrderId} for user {UserId}", order.Id, order.UserId);
                    
                    return CreateOrderResult.Success(order.Id, order.TotalAmount);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create order for user {UserId}", request.UserId);
                    return CreateOrderResult.Failure("Failed to create order");
                }
            }
        }
        
        public class CreateOrderRequest
        {
            public string UserId { get; set; }
            public List<OrderItemRequest> Items { get; set; }
            public string ShippingAddress { get; set; }
            public string BillingAddress { get; set; }
        }
        
        public class OrderItemRequest
        {
            public string ProductId { get; set; }
            public int Quantity { get; set; }
        }
        
        public class CreateOrderResult
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
            public string OrderId { get; set; }
            public decimal TotalAmount { get; set; }
            
            public static CreateOrderResult Success(string orderId, decimal totalAmount)
            {
                return new CreateOrderResult
                {
                    IsSuccess = true,
                    OrderId = orderId,
                    TotalAmount = totalAmount
                };
            }
            
            public static CreateOrderResult Failure(string message)
            {
                return new CreateOrderResult
                {
                    IsSuccess = false,
                    Message = message
                };
            }
        }
    }
    
    // ===== INFRASTRUCTURE LAYER =====
    namespace Infrastructure.Persistence
    {
        public class ApplicationDbContext : DbContext
        {
            public DbSet<User> Users { get; set; }
            public DbSet<Product> Products { get; set; }
            public DbSet<Order> Orders { get; set; }
            public DbSet<AuditLog> AuditLogs { get; set; }
            
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
            {
            }
            
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                // User configuration
                modelBuilder.Entity<User>(entity =>
                {
                    entity.HasKey(e => e.Id);
                    entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                    entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                    entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                    entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                    entity.Property(e => e.Roles).HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null));
                    entity.Property(e => e.Permissions).HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null));
                    
                    entity.HasIndex(e => e.Username).IsUnique();
                    entity.HasIndex(e => e.Email).IsUnique();
                });
                
                // Product configuration
                modelBuilder.Entity<Product>(entity =>
                {
                    entity.HasKey(e => e.Id);
                    entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                    entity.Property(e => e.Description).HasMaxLength(500);
                    entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
                });
                
                // Order configuration
                modelBuilder.Entity<Order>(entity =>
                {
                    entity.HasKey(e => e.Id);
                    entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.Status).HasConversion<string>();
                    entity.Property(e => e.ShippingAddress).IsRequired().HasMaxLength(500);
                    entity.Property(e => e.BillingAddress).IsRequired().HasMaxLength(500);
                    
                    entity.HasOne<User>().WithMany().HasForeignKey(e => e.UserId);
                });
                
                // OrderItem configuration (owned entity)
                modelBuilder.Entity<Order>().OwnsMany(e => e.Items, item =>
                {
                    item.Property(i => i.ProductId).IsRequired();
                    item.Property(i => i.ProductName).IsRequired().HasMaxLength(100);
                    item.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
                    item.Property(i => i.TotalPrice).HasColumnType("decimal(18,2)");
                });
                
                // AuditLog configuration
                modelBuilder.Entity<AuditLog>(entity =>
                {
                    entity.HasKey(e => e.Id);
                    entity.Property(e => e.EventType).IsRequired().HasMaxLength(50);
                    entity.Property(e => e.SubEventType).HasMaxLength(100);
                    entity.Property(e => e.Username).HasMaxLength(50);
                    entity.Property(e => e.UserId).HasMaxLength(50);
                    entity.Property(e => e.Resource).HasMaxLength(100);
                    entity.Property(e => e.Action).HasMaxLength(50);
                    entity.Property(e => e.Result).HasMaxLength(20);
                    entity.Property(e => e.Details).HasMaxLength(1000);
                    entity.Property(e => e.IpAddress).HasMaxLength(45);
                    entity.Property(e => e.UserAgent).HasMaxLength(500);
                    
                    entity.HasIndex(e => e.Timestamp);
                    entity.HasIndex(e => e.EventType);
                    entity.HasIndex(e => e.UserId);
                    entity.HasIndex(e => e.Resource);
                });
            }
        }
    }
    
    // ===== PRESENTATION LAYER =====
    namespace Presentation.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        [Authorize]
        public class UsersController : ControllerBase
        {
            private readonly ICreateUserUseCase _createUserUseCase;
            private readonly ILogger<UsersController> _logger;
            
            public UsersController(
                ICreateUserUseCase createUserUseCase,
                ILogger<UsersController> logger)
            {
                _createUserUseCase = createUserUseCase;
                _logger = logger;
            }
            
            [HttpPost]
            [Authorize("Users", "Create", "Admin,Manager")]
            public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
            {
                try
                {
                    var result = await _createUserUseCase.ExecuteAsync(request);
                    
                    if (result.IsSuccess)
                    {
                        return CreatedAtAction(nameof(GetUser), new { id = result.UserId }, new
                        {
                            id = result.UserId,
                            username = result.Username,
                            message = "User created successfully"
                        });
                    }
                    
                    return BadRequest(new { message = result.Message });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating user");
                    return StatusCode(500, new { message = "Internal server error" });
                }
            }
            
            [HttpGet("{id}")]
            [Authorize("Users", "Read", "Admin,Manager,User")]
            public async Task<IActionResult> GetUser(string id)
            {
                // Implementation for getting user
                return Ok(new { message = "User retrieved successfully" });
            }
        }
        
        [ApiController]
        [Route("api/[controller]")]
        [Authorize]
        public class ProductsController : ControllerBase
        {
            private readonly ICreateProductUseCase _createProductUseCase;
            private readonly ILogger<ProductsController> _logger;
            
            public ProductsController(
                ICreateProductUseCase createProductUseCase,
                ILogger<ProductsController> logger)
            {
                _createProductUseCase = createProductUseCase;
                _logger = logger;
            }
            
            [HttpPost]
            [Authorize("Products", "Create", "Admin,Manager")]
            public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
            {
                try
                {
                    var result = await _createProductUseCase.ExecuteAsync(request);
                    
                    if (result.IsSuccess)
                    {
                        return CreatedAtAction(nameof(GetProduct), new { id = result.ProductId }, new
                        {
                            id = result.ProductId,
                            name = result.Name,
                            message = "Product created successfully"
                        });
                    }
                    
                    return BadRequest(new { message = result.Message });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating product");
                    return StatusCode(500, new { message = "Internal server error" });
                }
            }
            
            [HttpGet("{id}")]
            [Authorize("Products", "Read")]
            public async Task<IActionResult> GetProduct(string id)
            {
                // Implementation for getting product
                return Ok(new { message = "Product retrieved successfully" });
            }
        }
        
        [ApiController]
        [Route("api/[controller]")]
        [Authorize]
        public class OrdersController : ControllerBase
        {
            private readonly ICreateOrderUseCase _createOrderUseCase;
            private readonly ILogger<OrdersController> _logger;
            
            public OrdersController(
                ICreateOrderUseCase createOrderUseCase,
                ILogger<OrdersController> logger)
            {
                _createOrderUseCase = createOrderUseCase;
                _logger = logger;
            }
            
            [HttpPost]
            [Authorize("Orders", "Create", "User")]
            public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
            {
                try
                {
                    var result = await _createOrderUseCase.ExecuteAsync(request);
                    
                    if (result.IsSuccess)
                    {
                        return CreatedAtAction(nameof(GetOrder), new { id = result.OrderId }, new
                        {
                            id = result.OrderId,
                            totalAmount = result.TotalAmount,
                            message = "Order created successfully"
                        });
                    }
                    
                    return BadRequest(new { message = result.Message });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating order");
                    return StatusCode(500, new { message = "Internal server error" });
                }
            }
            
            [HttpGet("{id}")]
            [Authorize("Orders", "Read", "Admin,Manager,User")]
            public async Task<IActionResult> GetOrder(string id)
            {
                // Implementation for getting order
                return Ok(new { message = "Order retrieved successfully" });
            }
        }
    }
    
    // ===== DEPENDENCY INJECTION =====
    namespace Infrastructure.DependencyInjection
    {
        public static class ServiceCollectionExtensions
        {
            public static IServiceCollection AddEnterprisePlatform(this IServiceCollection services, IConfiguration configuration)
            {
                // Add database context
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
                
                // Add repositories
                services.AddScoped<IUserRepository, UserRepository>();
                services.AddScoped<IProductRepository, ProductRepository>();
                services.AddScoped<IOrderRepository, OrderRepository>();
                
                // Add use cases
                services.AddScoped<ICreateUserUseCase, CreateUserUseCase>();
                services.AddScoped<ICreateProductUseCase, CreateProductUseCase>();
                services.AddScoped<ICreateOrderUseCase, CreateOrderUseCase>();
                
                // Add event bus
                services.AddScoped<IEventBus, InMemoryEventBus>();
                
                // Add enterprise security
                services.AddEnterpriseSecurity();
                
                // Add monitoring and observability
                services.AddMonitoringAndObservability();
                
                return services;
            }
        }
    }
}

// Uso de la Plataforma Empresarial
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Plataforma Empresarial - Proyecto Final ===\n");
        
        Console.WriteLine("La plataforma implementa:");
        Console.WriteLine("1. Arquitectura Limpia con capas bien definidas");
        Console.WriteLine("2. Event-Driven Architecture con Domain Events");
        Console.WriteLine("3. Microservicios con comunicación asíncrona");
        Console.WriteLine("4. Patrones de Diseño Enterprise (Repository, Unit of Work)");
        Console.WriteLine("5. CQRS y Event Sourcing para datos");
        Console.WriteLine("6. Calidad del código con métricas y refactoring");
        Console.WriteLine("7. Monitoreo y observabilidad completa");
        Console.WriteLine("8. Arquitectura evolutiva con versionado");
        Console.WriteLine("9. Seguridad enterprise con autenticación y autorización");
        
        Console.WriteLine("\nBeneficios de esta plataforma:");
        Console.WriteLine("- Escalabilidad horizontal y vertical");
        Console.WriteLine("- Mantenibilidad y extensibilidad");
        Console.WriteLine("- Seguridad y compliance enterprise");
        Console.WriteLine("- Monitoreo y observabilidad en tiempo real");
        Console.WriteLine("- Arquitectura evolutiva y adaptable");
        Console.WriteLine("- Patrones probados y documentados");
        
        Console.WriteLine("\n¡Has completado el Módulo 6: Arquitectura de Software Empresarial!");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Extender la Plataforma
Agrega funcionalidades como gestión de inventario, reportes y notificaciones.

### Ejercicio 2: Implementar Microservicios
Divide la plataforma en microservicios independientes con comunicación asíncrona.

### Ejercicio 3: Agregar Monitoreo
Implementa dashboards, alertas y métricas de negocio.

## 🔍 Puntos Clave

1. **Arquitectura completa** integrando todos los conceptos del módulo
2. **Patrones enterprise** aplicados en un proyecto real
3. **Seguridad robusta** con autenticación y autorización
4. **Event-driven design** para escalabilidad
5. **Clean Architecture** con separación clara de responsabilidades

## 🎉 ¡Felicidades!

**Has completado exitosamente el Módulo 6: Arquitectura de Software Empresarial**

### **Resumen de lo Aprendido:**
- ✅ Arquitectura Limpia Avanzada
- ✅ Event-Driven Architecture
- ✅ Microservicios Avanzados
- ✅ Patrones de Diseño Enterprise
- ✅ Arquitectura de Datos Avanzada
- ✅ Calidad del Código y Métricas
- ✅ Monitoreo y Observabilidad
- ✅ Arquitectura Evolutiva
- ✅ Seguridad Enterprise
- ✅ **Proyecto Final Integrador**

### **Próximos Pasos:**
- Continuar con el siguiente módulo del curso
- Aplicar estos conceptos en proyectos reales
- Explorar tecnologías adicionales como Docker, Kubernetes
- Considerar certificaciones de arquitectura de software

---

**🏆 ¡Excelente trabajo! Ahora eres un desarrollador C# Senior con conocimientos sólidos en arquitectura empresarial**

**📚 [Volver al README del Módulo 6](../senior_1/README.md)**
