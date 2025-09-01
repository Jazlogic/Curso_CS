# 🚀 Clase 10: Proyecto Final - Sistema de E-commerce

## 📋 Información de la Clase

- **Módulo**: Mid Level 1 - Programación Avanzada y Patrones de Diseño
- **Duración**: 4 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar todas las clases anteriores del Módulo 4

## 🎯 Objetivos de Aprendizaje

- Integrar todos los conceptos aprendidos en un proyecto real
- Implementar Clean Architecture con CQRS
- Aplicar patrones de diseño intermedios
- Implementar testing completo (unit, integration, BDD)
- Configurar CI/CD pipeline
- Desplegar aplicación en contenedores

---

## 📚 Navegación del Módulo 4

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_patrones_diseno_intermedios.md) | Patrones de Diseño Intermedios | |
| [Clase 2](clase_2_programacion_asincrona_avanzada.md) | Programación Asíncrona Avanzada | |
| [Clase 3](clase_3_programacion_paralela.md) | Programación Paralela y TPL | |
| [Clase 4](clase_4_clean_architecture.md) | Clean Architecture | |
| [Clase 5](clase_5_dependency_injection.md) | Dependency Injection Avanzada | |
| [Clase 6](clase_6_logging_monitoreo.md) | Logging y Monitoreo | |
| [Clase 7](clase_7_refactoring_clean_code.md) | Refactoring y Clean Code | |
| [Clase 8](clase_8_testing_integracion.md) | Testing de Integración | |
| [Clase 9](clase_9_testing_comportamiento.md) | Testing de Comportamiento (BDD) | ← Anterior |
| **Clase 10** | **Proyecto Final: Sistema de E-commerce** | ← Estás aquí |

**← [Volver al README del Módulo 4](../midLevel_1/README.md)**

---

## 📚 Contenido Teórico

### 1. Arquitectura del Sistema de E-commerce

Sistema completo que integra todos los conceptos aprendidos en el módulo.

```csharp
// ===== ESTRUCTURA DEL PROYECTO =====
namespace ECommerceSystem
{
    // ===== DOMAIN LAYER - ENTIDADES Y VALIDACIONES =====
    namespace Domain.Entities
    {
        public class User : BaseEntity
        {
            public string Email { get; private set; }
            public string FirstName { get; private set; }
            public string LastName { get; private set; }
            public string PasswordHash { get; private set; }
            public UserRole Role { get; private set; }
            public bool IsActive { get; private set; }
            public DateTime CreatedAt { get; private set; }
            public DateTime? LastLoginAt { get; private set; }
            
            // Constructor privado para EF Core
            private User() { }
            
            public User(string email, string firstName, string lastName, string passwordHash)
            {
                Email = email;
                FirstName = firstName;
                LastName = lastName;
                PasswordHash = passwordHash;
                Role = UserRole.Customer;
                IsActive = true;
                CreatedAt = DateTime.UtcNow;
            }
            
            public void UpdateProfile(string firstName, string lastName)
            {
                FirstName = firstName;
                LastName = lastName;
            }
            
            public void Deactivate()
            {
                IsActive = false;
            }
            
            public void RecordLogin()
            {
                LastLoginAt = DateTime.UtcNow;
            }
        }
        
        public class Product : BaseEntity
        {
            public string Name { get; private set; }
            public string Description { get; private set; }
            public decimal Price { get; private set; }
            public int Stock { get; private set; }
            public string SKU { get; private set; }
            public ProductCategory Category { get; private set; }
            public bool IsActive { get; private set; }
            public List<ProductImage> Images { get; private set; }
            public List<ProductReview> Reviews { get; private set; }
            
            private Product()
            {
                Images = new List<ProductImage>();
                Reviews = new List<ProductReview>();
            }
            
            public Product(string name, string description, decimal price, int stock, string sku, ProductCategory category)
            {
                Name = name;
                Description = description;
                Price = price;
                Stock = stock;
                SKU = sku;
                Category = category;
                IsActive = true;
                Images = new List<ProductImage>();
                Reviews = new List<ProductReview>();
            }
            
            public void UpdateStock(int newStock)
            {
                if (newStock < 0)
                    throw new ArgumentException("Stock cannot be negative");
                
                Stock = newStock;
            }
            
            public void UpdatePrice(decimal newPrice)
            {
                if (newPrice < 0)
                    throw new ArgumentException("Price cannot be negative");
                
                Price = newPrice;
            }
            
            public void AddImage(string imageUrl, string altText)
            {
                var image = new ProductImage(imageUrl, altText);
                Images.Add(image);
            }
            
            public void AddReview(int rating, string comment, int userId)
            {
                if (rating < 1 || rating > 5)
                    throw new ArgumentException("Rating must be between 1 and 5");
                
                var review = new ProductReview(rating, comment, userId);
                Reviews.Add(review);
            }
        }
        
        public class Order : BaseEntity
        {
            public int UserId { get; private set; }
            public OrderStatus Status { get; private set; }
            public decimal TotalAmount { get; private set; }
            public string ShippingAddress { get; private set; }
            public string PaymentMethod { get; private set; }
            public DateTime OrderDate { get; private set; }
            public DateTime? ShippedDate { get; private set; }
            public DateTime? DeliveredDate { get; private set; }
            public List<OrderItem> Items { get; private set; }
            
            private Order()
            {
                Items = new List<OrderItem>();
            }
            
            public Order(int userId, string shippingAddress, string paymentMethod)
            {
                UserId = userId;
                Status = OrderStatus.Pending;
                ShippingAddress = shippingAddress;
                PaymentMethod = paymentMethod;
                OrderDate = DateTime.UtcNow;
                Items = new List<OrderItem>();
            }
            
            public void AddItem(int productId, int quantity, decimal unitPrice)
            {
                var item = new OrderItem(productId, quantity, unitPrice);
                Items.Add(item);
                CalculateTotal();
            }
            
            public void Confirm()
            {
                if (Status != OrderStatus.Pending)
                    throw new InvalidOperationException("Order cannot be confirmed in current status");
                
                Status = OrderStatus.Confirmed;
            }
            
            public void Ship()
            {
                if (Status != OrderStatus.Confirmed)
                    throw new InvalidOperationException("Order cannot be shipped in current status");
                
                Status = OrderStatus.Shipped;
                ShippedDate = DateTime.UtcNow;
            }
            
            public void Deliver()
            {
                if (Status != OrderStatus.Shipped)
                    throw new InvalidOperationException("Order cannot be delivered in current status");
                
                Status = OrderStatus.Delivered;
                DeliveredDate = DateTime.UtcNow;
            }
            
            public void Cancel()
            {
                if (Status == OrderStatus.Delivered)
                    throw new InvalidOperationException("Delivered orders cannot be cancelled");
                
                Status = OrderStatus.Cancelled;
            }
            
            private void CalculateTotal()
            {
                TotalAmount = Items.Sum(item => item.Quantity * item.UnitPrice);
            }
        }
        
        public class Cart : BaseEntity
        {
            public int UserId { get; private set; }
            public DateTime CreatedAt { get; private set; }
            public DateTime? LastModifiedAt { get; private set; }
            public List<CartItem> Items { get; private set; }
            public string? PromotionCode { get; private set; }
            public decimal DiscountAmount { get; private set; }
            
            private Cart()
            {
                Items = new List<CartItem>();
            }
            
            public Cart(int userId)
            {
                UserId = userId;
                CreatedAt = DateTime.UtcNow;
                LastModifiedAt = DateTime.UtcNow;
                Items = new List<CartItem>();
                DiscountAmount = 0;
            }
            
            public void AddItem(int productId, int quantity, decimal unitPrice)
            {
                var existingItem = Items.FirstOrDefault(item => item.ProductId == productId);
                
                if (existingItem != null)
                {
                    existingItem.UpdateQuantity(existingItem.Quantity + quantity);
                }
                else
                {
                    var item = new CartItem(productId, quantity, unitPrice);
                    Items.Add(item);
                }
                
                LastModifiedAt = DateTime.UtcNow;
            }
            
            public void RemoveItem(int productId)
            {
                var item = Items.FirstOrDefault(item => item.ProductId == productId);
                if (item != null)
                {
                    Items.Remove(item);
                    LastModifiedAt = DateTime.UtcNow;
                }
            }
            
            public void UpdateItemQuantity(int productId, int quantity)
            {
                var item = Items.FirstOrDefault(item => item.ProductId == productId);
                if (item != null)
                {
                    if (quantity <= 0)
                    {
                        Items.Remove(item);
                    }
                    else
                    {
                        item.UpdateQuantity(quantity);
                    }
                    LastModifiedAt = DateTime.UtcNow;
                }
            }
            
            public void ApplyPromotion(string promotionCode, decimal discountAmount)
            {
                PromotionCode = promotionCode;
                DiscountAmount = discountAmount;
                LastModifiedAt = DateTime.UtcNow;
            }
            
            public void ClearPromotion()
            {
                PromotionCode = null;
                DiscountAmount = 0;
                LastModifiedAt = DateTime.UtcNow;
            }
            
            public decimal GetSubtotal()
            {
                return Items.Sum(item => item.Quantity * item.UnitPrice);
            }
            
            public decimal GetTotal()
            {
                return GetSubtotal() - DiscountAmount;
            }
            
            public void Clear()
            {
                Items.Clear();
                ClearPromotion();
                LastModifiedAt = DateTime.UtcNow;
            }
        }
        
        // ===== ENUMS =====
        public enum UserRole
        {
            Customer = 1,
            Admin = 2,
            Moderator = 3
        }
        
        public enum OrderStatus
        {
            Pending = 1,
            Confirmed = 2,
            Shipped = 3,
            Delivered = 4,
            Cancelled = 5
        }
        
        public enum ProductCategory
        {
            Electronics = 1,
            Clothing = 2,
            Books = 3,
            Home = 4,
            Sports = 5
        }
    }
    
    // ===== APPLICATION LAYER - USE CASES Y DTOs =====
    namespace Application.UseCases
    {
        public class CreateUserUseCase : ICreateUserUseCase
        {
            private readonly IUserRepository _userRepository;
            private readonly IPasswordHasher _passwordHasher;
            private readonly IEmailService _emailService;
            private readonly ILogger<CreateUserUseCase> _logger;
            
            public CreateUserUseCase(
                IUserRepository userRepository,
                IPasswordHasher passwordHasher,
                IEmailService emailService,
                ILogger<CreateUserUseCase> logger)
            {
                _userRepository = userRepository;
                _passwordHasher = passwordHasher;
                _emailService = emailService;
                _logger = logger;
            }
            
            public async Task<CreateUserResult> ExecuteAsync(CreateUserRequest request)
            {
                try
                {
                    _logger.LogInformation("Creating user with email: {Email}", request.Email);
                    
                    // Validar que el email no exista
                    var existingUser = await _userRepository.GetByEmailAsync(request.Email);
                    if (existingUser != null)
                    {
                        _logger.LogWarning("User with email {Email} already exists", request.Email);
                        return CreateUserResult.Failure("User with this email already exists");
                    }
                    
                    // Hashear contraseña
                    var passwordHash = _passwordHasher.HashPassword(request.Password);
                    
                    // Crear usuario
                    var user = new User(request.Email, request.FirstName, request.LastName, passwordHash);
                    
                    // Guardar en base de datos
                    await _userRepository.AddAsync(user);
                    
                    // Enviar email de bienvenida
                    await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName);
                    
                    _logger.LogInformation("User created successfully with ID: {UserId}", user.Id);
                    
                    return CreateUserResult.Success(new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Role = user.Role,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating user with email: {Email}", request.Email);
                    return CreateUserResult.Failure("An error occurred while creating the user");
                }
            }
        }
        
        public class CreateOrderUseCase : ICreateOrderUseCase
        {
            private readonly IOrderRepository _orderRepository;
            private readonly ICartRepository _cartRepository;
            private readonly IProductRepository _productRepository;
            private readonly IUserRepository _userRepository;
            private readonly IPaymentService _paymentService;
            private readonly IEmailService _emailService;
            private readonly ILogger<CreateOrderUseCase> _logger;
            
            public CreateOrderUseCase(
                IOrderRepository orderRepository,
                ICartRepository cartRepository,
                IProductRepository productRepository,
                IUserRepository userRepository,
                IPaymentService paymentService,
                IEmailService emailService,
                ILogger<CreateOrderUseCase> logger)
            {
                _orderRepository = orderRepository;
                _cartRepository = cartRepository;
                _productRepository = productRepository;
                _userRepository = userRepository;
                _paymentService = paymentService;
                _emailService = emailService;
                _logger = logger;
            }
            
            public async Task<CreateOrderResult> ExecuteAsync(CreateOrderRequest request)
            {
                try
                {
                    _logger.LogInformation("Creating order for user: {UserId}", request.UserId);
                    
                    // Obtener carrito del usuario
                    var cart = await _cartRepository.GetByUserIdAsync(request.UserId);
                    if (cart == null || !cart.Items.Any())
                    {
                        return CreateOrderResult.Failure("Cart is empty");
                    }
                    
                    // Verificar stock de productos
                    foreach (var item in cart.Items)
                    {
                        var product = await _productRepository.GetByIdAsync(item.ProductId);
                        if (product == null)
                        {
                            return CreateOrderResult.Failure($"Product {item.ProductId} not found");
                        }
                        
                        if (product.Stock < item.Quantity)
                        {
                            return CreateOrderResult.Failure($"Insufficient stock for product {product.Name}");
                        }
                    }
                    
                    // Procesar pago
                    var paymentResult = await _paymentService.ProcessPaymentAsync(new PaymentRequest
                    {
                        Amount = cart.GetTotal(),
                        Currency = "USD",
                        PaymentMethod = request.PaymentMethod,
                        CustomerId = request.UserId.ToString()
                    });
                    
                    if (!paymentResult.Success)
                    {
                        return CreateOrderResult.Failure($"Payment failed: {paymentResult.ErrorMessage}");
                    }
                    
                    // Crear orden
                    var order = new Order(request.UserId, request.ShippingAddress, request.PaymentMethod);
                    
                    foreach (var item in cart.Items)
                    {
                        order.AddItem(item.ProductId, item.Quantity, item.UnitPrice);
                    }
                    
                    // Confirmar orden
                    order.Confirm();
                    
                    // Guardar orden
                    await _orderRepository.AddAsync(order);
                    
                    // Actualizar stock de productos
                    foreach (var item in cart.Items)
                    {
                        var product = await _productRepository.GetByIdAsync(item.ProductId);
                        product.UpdateStock(product.Stock - item.Quantity);
                        await _productRepository.UpdateAsync(product);
                    }
                    
                    // Limpiar carrito
                    cart.Clear();
                    await _cartRepository.UpdateAsync(cart);
                    
                    // Enviar email de confirmación
                    var user = await _userRepository.GetByIdAsync(request.UserId);
                    await _emailService.SendOrderConfirmationEmailAsync(user.Email, order.Id, order.TotalAmount);
                    
                    _logger.LogInformation("Order created successfully with ID: {OrderId}", order.Id);
                    
                    return CreateOrderResult.Success(new OrderDto
                    {
                        Id = order.Id,
                        UserId = order.UserId,
                        Status = order.Status,
                        TotalAmount = order.TotalAmount,
                        OrderDate = order.OrderDate,
                        Items = order.Items.Select(item => new OrderItemDto
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice
                        }).ToList()
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating order for user: {UserId}", request.UserId);
                    return CreateOrderResult.Failure("An error occurred while creating the order");
                }
            }
        }
        
        public class SearchProductsUseCase : ISearchProductsUseCase
        {
            private readonly IProductRepository _productRepository;
            private readonly ILogger<SearchProductsUseCase> _logger;
            
            public SearchProductsUseCase(
                IProductRepository productRepository,
                ILogger<SearchProductsUseCase> logger)
            {
                _productRepository = productRepository;
                _logger = logger;
            }
            
            public async Task<SearchProductsResult> ExecuteAsync(SearchProductsRequest request)
            {
                try
                {
                    _logger.LogInformation("Searching products with criteria: {Criteria}", request.SearchTerm);
                    
                    var products = await _productRepository.SearchAsync(
                        request.SearchTerm,
                        request.Category,
                        request.MinPrice,
                        request.MaxPrice,
                        request.Page,
                        request.PageSize);
                    
                    var totalCount = await _productRepository.GetSearchCountAsync(
                        request.SearchTerm,
                        request.Category,
                        request.MinPrice,
                        request.MaxPrice);
                    
                    var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
                    
                    _logger.LogInformation("Found {Count} products in {TotalPages} pages", products.Count(), totalPages);
                    
                    return SearchProductsResult.Success(new ProductSearchResult
                    {
                        Products = products.Select(p => new ProductDto
                        {
                            Id = p.Id,
                            Name = p.Name,
                            Description = p.Description,
                            Price = p.Price,
                            Stock = p.Stock,
                            Category = p.Category,
                            AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0
                        }).ToList(),
                        TotalCount = totalCount,
                        Page = request.Page,
                        PageSize = request.PageSize,
                        TotalPages = totalPages
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error searching products with criteria: {Criteria}", request.SearchTerm);
                    return SearchProductsResult.Failure("An error occurred while searching products");
                }
            }
        }
    }
    
    // ===== INFRASTRUCTURE LAYER - IMPLEMENTACIONES =====
    namespace Infrastructure.Repositories
    {
        public class UserRepository : IUserRepository
        {
            private readonly ApplicationDbContext _context;
            private readonly ILogger<UserRepository> _logger;
            
            public UserRepository(ApplicationDbContext context, ILogger<UserRepository> logger)
            {
                _context = context;
                _logger = logger;
            }
            
            public async Task<User> GetByIdAsync(int id)
            {
                return await _context.Users
                    .Include(u => u.Orders)
                    .FirstOrDefaultAsync(u => u.Id == id);
            }
            
            public async Task<User> GetByEmailAsync(string email)
            {
                return await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);
            }
            
            public async Task<IEnumerable<User>> GetAllAsync(int page, int pageSize)
            {
                return await _context.Users
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            
            public async Task<User> AddAsync(User user)
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return user;
            }
            
            public async Task UpdateAsync(User user)
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
            
            public async Task DeleteAsync(int id)
            {
                var user = await GetByIdAsync(id);
                if (user != null)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                }
            }
        }
        
        public class ProductRepository : IProductRepository
        {
            private readonly ApplicationDbContext _context;
            private readonly ILogger<ProductRepository> _logger;
            
            public ProductRepository(ApplicationDbContext context, ILogger<ProductRepository> logger)
            {
                _context = context;
                _logger = logger;
            }
            
            public async Task<IEnumerable<Product>> SearchAsync(
                string searchTerm,
                ProductCategory? category,
                decimal? minPrice,
                decimal? maxPrice,
                int page,
                int pageSize)
            {
                var query = _context.Products
                    .Include(p => p.Images)
                    .Include(p => p.Reviews)
                    .Where(p => p.IsActive);
                
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(p => 
                        p.Name.Contains(searchTerm) || 
                        p.Description.Contains(searchTerm));
                }
                
                if (category.HasValue)
                {
                    query = query.Where(p => p.Category == category.Value);
                }
                
                if (minPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= minPrice.Value);
                }
                
                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= maxPrice.Value);
                }
                
                return await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            
            public async Task<int> GetSearchCountAsync(
                string searchTerm,
                ProductCategory? category,
                decimal? minPrice,
                decimal? maxPrice)
            {
                var query = _context.Products.Where(p => p.IsActive);
                
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(p => 
                        p.Name.Contains(searchTerm) || 
                        p.Description.Contains(searchTerm));
                }
                
                if (category.HasValue)
                {
                    query = query.Where(p => p.Category == category.Value);
                }
                
                if (minPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= minPrice.Value);
                }
                
                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= maxPrice.Value);
                }
                
                return await query.CountAsync();
            }
        }
    }
    
    // ===== PRESENTATION LAYER - API CONTROLLERS =====
    namespace Presentation.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        public class UsersController : ControllerBase
        {
            private readonly ICreateUserUseCase _createUserUseCase;
            private readonly IGetUserUseCase _getUserUseCase;
            private readonly IUpdateUserUseCase _updateUserUseCase;
            private readonly ILogger<UsersController> _logger;
            
            public UsersController(
                ICreateUserUseCase createUserUseCase,
                IGetUserUseCase getUserUseCase,
                IUpdateUserUseCase updateUserUseCase,
                ILogger<UsersController> logger)
            {
                _createUserUseCase = createUserUseCase;
                _getUserUseCase = getUserUseCase;
                _updateUserUseCase = updateUserUseCase;
                _logger = logger;
            }
            
            [HttpPost]
            public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
            {
                try
                {
                    var result = await _createUserUseCase.ExecuteAsync(request);
                    
                    if (result.IsSuccess)
                    {
                        return CreatedAtAction(nameof(GetUser), new { id = result.Data.Id }, result.Data);
                    }
                    
                    return BadRequest(result.ErrorMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in CreateUser endpoint");
                    return StatusCode(500, "Internal server error");
                }
            }
            
            [HttpGet("{id}")]
            public async Task<IActionResult> GetUser(int id)
            {
                try
                {
                    var result = await _getUserUseCase.ExecuteAsync(id);
                    
                    if (result.IsSuccess)
                    {
                        return Ok(result.Data);
                    }
                    
                    return NotFound(result.ErrorMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in GetUser endpoint for ID: {UserId}", id);
                    return StatusCode(500, "Internal server error");
                }
            }
            
            [HttpPut("{id}")]
            public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
            {
                try
                {
                    var result = await _updateUserUseCase.ExecuteAsync(id, request);
                    
                    if (result.IsSuccess)
                    {
                        return Ok(result.Data);
                    }
                    
                    return BadRequest(result.ErrorMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in UpdateUser endpoint for ID: {UserId}", id);
                    return StatusCode(500, "Internal server error");
                }
            }
        }
        
        [ApiController]
        [Route("api/[controller]")]
        public class ProductsController : ControllerBase
        {
            private readonly ISearchProductsUseCase _searchProductsUseCase;
            private readonly IGetProductUseCase _getProductUseCase;
            private readonly ILogger<ProductsController> _logger;
            
            public ProductsController(
                ISearchProductsUseCase searchProductsUseCase,
                IGetProductUseCase getProductUseCase,
                ILogger<ProductsController> logger)
            {
                _searchProductsUseCase = searchProductsUseCase;
                _getProductUseCase = getProductUseCase;
                _logger = logger;
            }
            
            [HttpGet("search")]
            public async Task<IActionResult> SearchProducts([FromQuery] SearchProductsRequest request)
            {
                try
                {
                    var result = await _searchProductsUseCase.ExecuteAsync(request);
                    
                    if (result.IsSuccess)
                    {
                        return Ok(result.Data);
                    }
                    
                    return BadRequest(result.ErrorMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in SearchProducts endpoint");
                    return StatusCode(500, "Internal server error");
                }
            }
            
            [HttpGet("{id}")]
            public async Task<IActionResult> GetProduct(int id)
            {
                try
                {
                    var result = await _getProductUseCase.ExecuteAsync(id);
                    
                    if (result.IsSuccess)
                    {
                        return Ok(result.Data);
                    }
                    
                    return NotFound(result.ErrorMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in GetProduct endpoint for ID: {ProductId}", id);
                    return StatusCode(500, "Internal server error");
                }
            }
        }
        
        [ApiController]
        [Route("api/[controller]")]
        public class OrdersController : ControllerBase
        {
            private readonly ICreateOrderUseCase _createOrderUseCase;
            private readonly IGetOrderUseCase _getOrderUseCase;
            private readonly IGetUserOrdersUseCase _getUserOrdersUseCase;
            private readonly ILogger<OrdersController> _logger;
            
            public OrdersController(
                ICreateOrderUseCase createOrderUseCase,
                IGetOrderUseCase getOrderUseCase,
                IGetUserOrdersUseCase getUserOrdersUseCase,
                ILogger<OrdersController> logger)
            {
                _createOrderUseCase = createOrderUseCase;
                _getOrderUseCase = getOrderUseCase;
                _getUserOrdersUseCase = getUserOrdersUseCase;
                _logger = logger;
            }
            
            [HttpPost]
            public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
            {
                try
                {
                    var result = await _createOrderUseCase.ExecuteAsync(request);
                    
                    if (result.IsSuccess)
                    {
                        return CreatedAtAction(nameof(GetOrder), new { id = result.Data.Id }, result.Data);
                    }
                    
                    return BadRequest(result.ErrorMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in CreateOrder endpoint");
                    return StatusCode(500, "Internal server error");
                }
            }
            
            [HttpGet("{id}")]
            public async Task<IActionResult> GetOrder(int id)
            {
                try
                {
                    var result = await _getOrderUseCase.ExecuteAsync(id);
                    
                    if (result.IsSuccess)
                    {
                        return Ok(result.Data);
                    }
                    
                    return NotFound(result.ErrorMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in GetOrder endpoint for ID: {OrderId}", id);
                    return StatusCode(500, "Internal server error");
                }
            }
            
            [HttpGet("user/{userId}")]
            public async Task<IActionResult> GetUserOrders(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
            {
                try
                {
                    var result = await _getUserOrdersUseCase.ExecuteAsync(userId, page, pageSize);
                    
                    if (result.IsSuccess)
                    {
                        return Ok(result.Data);
                    }
                    
                    return BadRequest(result.ErrorMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in GetUserOrders endpoint for user ID: {UserId}", userId);
                    return StatusCode(500, "Internal server error");
                }
            }
        }
    }
    
    // ===== CONFIGURACIÓN DE DEPENDENCY INJECTION =====
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddECommerceServices(this IServiceCollection services, IConfiguration configuration)
        {
            // ===== REPOSITORIES =====
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            
            // ===== USE CASES =====
            services.AddScoped<ICreateUserUseCase, CreateUserUseCase>();
            services.AddScoped<IGetUserUseCase, GetUserUseCase>();
            services.AddScoped<IUpdateUserUseCase, UpdateUserUseCase>();
            services.AddScoped<ICreateOrderUseCase, CreateOrderUseCase>();
            services.AddScoped<IGetOrderUseCase, GetOrderUseCase>();
            services.AddScoped<IGetUserOrdersUseCase, GetUserOrdersUseCase>();
            services.AddScoped<ISearchProductsUseCase, SearchProductsUseCase>();
            services.AddScoped<IGetProductUseCase, GetProductUseCase>();
            
            // ===== SERVICES =====
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IEmailService, EmailService>();
            
            // ===== VALIDATORS =====
            services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
            services.AddScoped<IValidator<CreateOrderRequest>, CreateOrderRequestValidator>();
            services.AddScoped<IValidator<SearchProductsRequest>, SearchProductsRequestValidator>();
            
            // ===== LOGGING =====
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
                builder.AddSeq(configuration.GetSection("Seq"));
            });
            
            // ===== CACHING =====
            services.AddMemoryCache();
            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis");
            });
            
            // ===== HEALTH CHECKS =====
            services.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>()
                .AddRedis(configuration.GetConnectionString("Redis"));
            
            return services;
        }
    }
    
    // ===== CONFIGURACIÓN DE BASE DE DATOS =====
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddECommerceDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });
            });
            
            return services;
        }
    }
    
    // ===== CONFIGURACIÓN DE AUTENTICACIÓN =====
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddECommerceAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
                    };
                });
            
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin"));
                
                options.AddPolicy("CustomerOnly", policy =>
                    policy.RequireRole("Customer"));
            });
            
            return services;
        }
    }
    
    // ===== CONFIGURACIÓN DE SWAGGER =====
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddECommerceSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "E-Commerce API",
                    Version = "v1",
                    Description = "API para sistema de e-commerce con Clean Architecture",
                    Contact = new OpenApiContact
                    {
                        Name = "Development Team",
                        Email = "dev@ecommerce.com"
                    }
                });
                
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });
            
            return services;
        }
    }
}

// ===== PROGRAMA PRINCIPAL =====
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // ===== CONFIGURACIÓN DE SERVICIOS =====
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        
        // ===== CONFIGURACIÓN DE E-COMMERCE =====
        builder.Services.AddECommerceDatabase(builder.Configuration);
        builder.Services.AddECommerceServices(builder.Configuration);
        builder.Services.AddECommerceAuthentication(builder.Configuration);
        builder.Services.AddECommerceSwagger();
        
        // ===== CONFIGURACIÓN DE CORS =====
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });
        
        // ===== CONFIGURACIÓN DE RATE LIMITING =====
        builder.Services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1)
                    }));
        });
        
        var app = builder.Build();
        
        // ===== CONFIGURACIÓN DE MIDDLEWARE =====
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        
        // ===== HEALTH CHECKS =====
        app.MapHealthChecks("/health");
        
        app.MapControllers();
        
        // ===== MIGRACIÓN DE BASE DE DATOS =====
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate();
        }
        
        app.Run();
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Implementación Completa
Implementa el sistema completo siguiendo la arquitectura propuesta, incluyendo todos los endpoints y validaciones.

### Ejercicio 2: Testing Completo
Crea tests unitarios, de integración y BDD para todo el sistema.

### Ejercicio 3: Despliegue
Configura Docker, CI/CD pipeline y despliega la aplicación en un entorno cloud.

## 🔍 Puntos Clave

1. **Clean Architecture** separa responsabilidades en capas bien definidas
2. **CQRS** separa operaciones de lectura y escritura
3. **Dependency Injection** permite testing y flexibilidad
4. **Testing completo** incluye unit, integration y BDD
5. **Logging y monitoreo** son esenciales para producción

## 📚 Recursos Adicionales

- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern - Microsoft Docs](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [ASP.NET Core Best Practices](https://docs.microsoft.com/en-us/aspnet/core/performance/performance-best-practices)

---

**🎯 ¡Has completado el Módulo 4 completo! Ahora dominas la programación avanzada y patrones de diseño en C#**

**📚 [Siguiente Módulo: Módulo 5 - Mid Level 2](../midLevel_2/README.md)**
