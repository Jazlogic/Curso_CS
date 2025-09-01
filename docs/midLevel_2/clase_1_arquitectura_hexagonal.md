# 🚀 Clase 1: Arquitectura Hexagonal (Ports & Adapters)

## 📋 Información de la Clase

- **Módulo**: Mid Level 2 - Arquitectura de Software y Patrones Avanzados
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Módulo 4 (Mid Level 1)

## 🎯 Objetivos de Aprendizaje

- Comprender los principios de Arquitectura Hexagonal
- Implementar Ports & Adapters en C#
- Separar lógica de negocio de infraestructura
- Crear adaptadores para diferentes tecnologías
- Aplicar inversión de dependencias

---

## 📚 Navegación del Módulo 5

| Clase | Tema | Enlace |
|-------|------|--------|
| **Clase 1** | **Arquitectura Hexagonal (Ports & Adapters)** | ← Estás aquí |
| [Clase 2](clase_2_event_sourcing.md) | Event Sourcing y CQRS Avanzado | Siguiente → |
| [Clase 3](clase_3_microservicios.md) | Arquitectura de Microservicios | |
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

### 1. Introducción a la Arquitectura Hexagonal

La Arquitectura Hexagonal (también conocida como Ports & Adapters) es un patrón arquitectónico que permite crear aplicaciones independientes de frameworks, bases de datos y tecnologías externas.

```csharp
// ===== ARQUITECTURA HEXAGONAL - ESTRUCTURA BASE =====
namespace HexagonalArchitecture
{
    // ===== DOMAIN LAYER - NÚCLEO DE LA APLICACIÓN =====
    namespace Domain.Entities
    {
        public class User : BaseEntity
        {
            public string Email { get; private set; }
            public string FirstName { get; private set; }
            public string LastName { get; private set; }
            public UserStatus Status { get; private set; }
            public DateTime CreatedAt { get; private set; }
            public DateTime? LastLoginAt { get; private set; }
            
            // Constructor privado para EF Core
            private User() { }
            
            public User(string email, string firstName, string lastName)
            {
                Email = email;
                FirstName = firstName;
                LastName = lastName;
                Status = UserStatus.Active;
                CreatedAt = DateTime.UtcNow;
            }
            
            public void UpdateProfile(string firstName, string lastName)
            {
                FirstName = firstName;
                LastName = lastName;
            }
            
            public void Deactivate()
            {
                Status = UserStatus.Inactive;
            }
            
            public void RecordLogin()
            {
                LastLoginAt = DateTime.UtcNow;
            }
            
            public bool IsActive()
            {
                return Status == UserStatus.Active;
            }
        }
        
        public class Order : BaseEntity
        {
            public int UserId { get; private set; }
            public OrderStatus Status { get; private set; }
            public decimal TotalAmount { get; private set; }
            public DateTime OrderDate { get; private set; }
            public List<OrderItem> Items { get; private set; }
            
            private Order()
            {
                Items = new List<OrderItem>();
            }
            
            public Order(int userId)
            {
                UserId = userId;
                Status = OrderStatus.Pending;
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
        
        public class OrderItem : BaseEntity
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
            
            public void UpdateQuantity(int newQuantity)
            {
                if (newQuantity <= 0)
                    throw new ArgumentException("Quantity must be greater than zero");
                
                Quantity = newQuantity;
            }
        }
        
        // ===== ENUMS =====
        public enum UserStatus
        {
            Active = 1,
            Inactive = 2,
            Suspended = 3
        }
        
        public enum OrderStatus
        {
            Pending = 1,
            Confirmed = 2,
            Shipped = 3,
            Delivered = 4,
            Cancelled = 5
        }
    }
    
    // ===== DOMAIN SERVICES =====
    namespace Domain.Services
    {
        public interface IUserDomainService
        {
            bool IsEmailValid(string email);
            bool IsPasswordStrong(string password);
            string GenerateUsername(string firstName, string lastName);
        }
        
        public class UserDomainService : IUserDomainService
        {
            public bool IsEmailValid(string email)
            {
                if (string.IsNullOrWhiteSpace(email))
                    return false;
                
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
            
            public bool IsPasswordStrong(string password)
            {
                if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                    return false;
                
                var hasUpperCase = password.Any(char.IsUpper);
                var hasLowerCase = password.Any(char.IsLower);
                var hasDigit = password.Any(char.IsDigit);
                var hasSpecialChar = password.Any(c => !char.IsLetterOrDigit(c));
                
                return hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
            }
            
            public string GenerateUsername(string firstName, string lastName)
            {
                var baseUsername = $"{firstName.ToLower()}.{lastName.ToLower()}";
                var random = new Random();
                var suffix = random.Next(100, 999);
                
                return $"{baseUsername}{suffix}";
            }
        }
    }
    
    // ===== PORTS (INTERFACES) =====
    namespace Domain.Ports
    {
        // ===== PORTS DE ENTRADA (DRIVING PORTS) =====
        public interface IUserService
        {
            Task<UserDto> CreateUserAsync(CreateUserRequest request);
            Task<UserDto> GetUserAsync(int id);
            Task<UserDto> UpdateUserAsync(int id, UpdateUserRequest request);
            Task DeleteUserAsync(int id);
            Task<IEnumerable<UserDto>> GetAllUsersAsync(int page, int pageSize);
        }
        
        public interface IOrderService
        {
            Task<OrderDto> CreateOrderAsync(CreateOrderRequest request);
            Task<OrderDto> GetOrderAsync(int id);
            Task<OrderDto> UpdateOrderStatusAsync(int id, OrderStatus newStatus);
            Task<IEnumerable<OrderDto>> GetUserOrdersAsync(int userId, int page, int pageSize);
        }
        
        // ===== PORTS DE SALIDA (DRIVEN PORTS) =====
        public interface IUserRepository
        {
            Task<User> GetByIdAsync(int id);
            Task<User> GetByEmailAsync(string email);
            Task<IEnumerable<User>> GetAllAsync(int page, int pageSize);
            Task<User> AddAsync(User user);
            Task UpdateAsync(User user);
            Task DeleteAsync(int id);
            Task<bool> ExistsAsync(int id);
            Task<bool> EmailExistsAsync(string email);
        }
        
        public interface IOrderRepository
        {
            Task<Order> GetByIdAsync(int id);
            Task<IEnumerable<Order>> GetByUserIdAsync(int userId, int page, int pageSize);
            Task<Order> AddAsync(Order order);
            Task UpdateAsync(Order order);
            Task DeleteAsync(int id);
            Task<int> GetCountByUserIdAsync(int userId);
        }
        
        public interface IEmailService
        {
            Task SendWelcomeEmailAsync(string email, string firstName);
            Task SendOrderConfirmationEmailAsync(string email, int orderId, decimal totalAmount);
            Task SendPasswordResetEmailAsync(string email, string resetToken);
        }
        
        public interface ILoggingService
        {
            void LogInformation(string message, params object[] args);
            void LogWarning(string message, params object[] args);
            void LogError(string message, Exception exception = null, params object[] args);
        }
        
        public interface IUnitOfWork
        {
            Task BeginTransactionAsync();
            Task CommitAsync();
            Task RollbackAsync();
            Task<int> SaveChangesAsync();
        }
    }
    
    // ===== APPLICATION LAYER - USE CASES =====
    namespace Application.UseCases
    {
        public class CreateUserUseCase : IUserService
        {
            private readonly IUserRepository _userRepository;
            private readonly IUserDomainService _userDomainService;
            private readonly IEmailService _emailService;
            private readonly ILoggingService _loggingService;
            private readonly IUnitOfWork _unitOfWork;
            
            public CreateUserUseCase(
                IUserRepository userRepository,
                IUserDomainService userDomainService,
                IEmailService emailService,
                ILoggingService loggingService,
                IUnitOfWork unitOfWork)
            {
                _userRepository = userRepository;
                _userDomainService = userDomainService;
                _emailService = emailService;
                _loggingService = loggingService;
                _unitOfWork = unitOfWork;
            }
            
            public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
            {
                try
                {
                    _loggingService.LogInformation("Creating user with email: {Email}", request.Email);
                    
                    // Validaciones de dominio
                    if (!_userDomainService.IsEmailValid(request.Email))
                    {
                        throw new ValidationException("Invalid email format");
                    }
                    
                    if (!_userDomainService.IsPasswordStrong(request.Password))
                    {
                        throw new ValidationException("Password does not meet strength requirements");
                    }
                    
                    // Verificar si el email ya existe
                    if (await _userRepository.EmailExistsAsync(request.Email))
                    {
                        throw new ValidationException("Email already exists");
                    }
                    
                    // Crear usuario
                    var user = new User(request.Email, request.FirstName, request.LastName);
                    
                    // Iniciar transacción
                    await _unitOfWork.BeginTransactionAsync();
                    
                    try
                    {
                        // Guardar usuario
                        var savedUser = await _userRepository.AddAsync(user);
                        
                        // Enviar email de bienvenida
                        await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName);
                        
                        // Confirmar transacción
                        await _unitOfWork.CommitAsync();
                        
                        _loggingService.LogInformation("User created successfully with ID: {UserId}", savedUser.Id);
                        
                        return new UserDto
                        {
                            Id = savedUser.Id,
                            Email = savedUser.Email,
                            FirstName = savedUser.FirstName,
                            LastName = savedUser.LastName,
                            Status = savedUser.Status,
                            CreatedAt = savedUser.CreatedAt
                        };
                    }
                    catch
                    {
                        await _unitOfWork.RollbackAsync();
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _loggingService.LogError(ex, "Error creating user with email: {Email}", request.Email);
                    throw;
                }
            }
            
            public async Task<UserDto> GetUserAsync(int id)
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                    throw new NotFoundException($"User with ID {id} not found");
                
                return new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Status = user.Status,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt
                };
            }
            
            public async Task<UserDto> UpdateUserAsync(int id, UpdateUserRequest request)
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                    throw new NotFoundException($"User with ID {id} not found");
                
                user.UpdateProfile(request.FirstName, request.LastName);
                await _userRepository.UpdateAsync(user);
                
                return new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Status = user.Status,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt
                };
            }
            
            public async Task DeleteUserAsync(int id)
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                    throw new NotFoundException($"User with ID {id} not found");
                
                await _userRepository.DeleteAsync(id);
            }
            
            public async Task<IEnumerable<UserDto>> GetAllUsersAsync(int page, int pageSize)
            {
                var users = await _userRepository.GetAllAsync(page, pageSize);
                
                return users.Select(user => new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Status = user.Status,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt
                });
            }
        }
        
        public class CreateOrderUseCase : IOrderService
        {
            private readonly IOrderRepository _orderRepository;
            private readonly IUserRepository _userRepository;
            private readonly IEmailService _emailService;
            private readonly ILoggingService _loggingService;
            private readonly IUnitOfWork _unitOfWork;
            
            public CreateOrderUseCase(
                IOrderRepository orderRepository,
                IUserRepository userRepository,
                IEmailService emailService,
                ILoggingService loggingService,
                IUnitOfWork unitOfWork)
            {
                _orderRepository = orderRepository;
                _userRepository = userRepository;
                _emailService = emailService;
                _loggingService = loggingService;
                _unitOfWork = unitOfWork;
            }
            
            public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request)
            {
                try
                {
                    _loggingService.LogInformation("Creating order for user: {UserId}", request.UserId);
                    
                    // Verificar que el usuario existe
                    var user = await _userRepository.GetByIdAsync(request.UserId);
                    if (user == null)
                        throw new NotFoundException($"User with ID {request.UserId} not found");
                    
                    if (!user.IsActive())
                        throw new ValidationException("User account is not active");
                    
                    // Crear orden
                    var order = new Order(request.UserId);
                    
                    foreach (var item in request.Items)
                    {
                        order.AddItem(item.ProductId, item.Quantity, item.UnitPrice);
                    }
                    
                    // Iniciar transacción
                    await _unitOfWork.BeginTransactionAsync();
                    
                    try
                    {
                        // Guardar orden
                        var savedOrder = await _orderRepository.AddAsync(order);
                        
                        // Enviar email de confirmación
                        await _emailService.SendOrderConfirmationEmailAsync(
                            user.Email, 
                            savedOrder.Id, 
                            savedOrder.TotalAmount);
                        
                        // Confirmar transacción
                        await _unitOfWork.CommitAsync();
                        
                        _loggingService.LogInformation("Order created successfully with ID: {OrderId}", savedOrder.Id);
                        
                        return new OrderDto
                        {
                            Id = savedOrder.Id,
                            UserId = savedOrder.UserId,
                            Status = savedOrder.Status,
                            TotalAmount = savedOrder.TotalAmount,
                            OrderDate = savedOrder.OrderDate,
                            Items = savedOrder.Items.Select(item => new OrderItemDto
                            {
                                ProductId = item.ProductId,
                                Quantity = item.Quantity,
                                UnitPrice = item.UnitPrice
                            }).ToList()
                        };
                    }
                    catch
                    {
                        await _unitOfWork.RollbackAsync();
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _loggingService.LogError(ex, "Error creating order for user: {UserId}", request.UserId);
                    throw;
                }
            }
            
            public async Task<OrderDto> GetOrderAsync(int id)
            {
                var order = await _orderRepository.GetByIdAsync(id);
                if (order == null)
                    throw new NotFoundException($"Order with ID {id} not found");
                
                return new OrderDto
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
                };
            }
            
            public async Task<OrderDto> UpdateOrderStatusAsync(int id, OrderStatus newStatus)
            {
                var order = await _orderRepository.GetByIdAsync(id);
                if (order == null)
                    throw new NotFoundException($"Order with ID {id} not found");
                
                switch (newStatus)
                {
                    case OrderStatus.Confirmed:
                        order.Confirm();
                        break;
                    case OrderStatus.Cancelled:
                        order.Cancel();
                        break;
                    default:
                        throw new ValidationException($"Cannot update order to status: {newStatus}");
                }
                
                await _orderRepository.UpdateAsync(order);
                
                return new OrderDto
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
                };
            }
            
            public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(int userId, int page, int pageSize)
            {
                var orders = await _orderRepository.GetByUserIdAsync(userId, page, pageSize);
                
                return orders.Select(order => new OrderDto
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
        }
    }
    
    // ===== ADAPTERS - IMPLEMENTACIONES CONCRETAS =====
    namespace Infrastructure.Adapters
    {
        // ===== ADAPTERS DE INFRAESTRUCTURA =====
        namespace Persistence
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
                
                public async Task<bool> ExistsAsync(int id)
                {
                    return await _context.Users.AnyAsync(u => u.Id == id);
                }
                
                public async Task<bool> EmailExistsAsync(string email)
                {
                    return await _context.Users.AnyAsync(u => u.Email == email);
                }
            }
            
            public class OrderRepository : IOrderRepository
            {
                private readonly ApplicationDbContext _context;
                private readonly ILogger<OrderRepository> _logger;
                
                public OrderRepository(ApplicationDbContext context, ILogger<OrderRepository> logger)
                {
                    _context = context;
                    _logger = logger;
                }
                
                public async Task<Order> GetByIdAsync(int id)
                {
                    return await _context.Orders
                        .Include(o => o.Items)
                        .FirstOrDefaultAsync(o => o.Id == id);
                }
                
                public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId, int page, int pageSize)
                {
                    return await _context.Orders
                        .Include(o => o.Items)
                        .Where(o => o.UserId == userId)
                        .OrderByDescending(o => o.OrderDate)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();
                }
                
                public async Task<Order> AddAsync(Order order)
                {
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();
                    return order;
                }
                
                public async Task UpdateAsync(Order order)
                {
                    _context.Orders.Update(order);
                    await _context.SaveChangesAsync();
                }
                
                public async Task DeleteAsync(int id)
                {
                    var order = await GetByIdAsync(id);
                    if (order != null)
                    {
                        _context.Orders.Remove(order);
                        await _context.SaveChangesAsync();
                    }
                }
                
                public async Task<int> GetCountByUserIdAsync(int userId)
                {
                    return await _context.Orders
                        .Where(o => o.UserId == userId)
                        .CountAsync();
                }
            }
            
            public class UnitOfWork : IUnitOfWork
            {
                private readonly ApplicationDbContext _context;
                private readonly IDbContextTransaction _transaction;
                
                public UnitOfWork(ApplicationDbContext context)
                {
                    _context = context;
                }
                
                public async Task BeginTransactionAsync()
                {
                    _transaction = await _context.Database.BeginTransactionAsync();
                }
                
                public async Task CommitAsync()
                {
                    if (_transaction != null)
                    {
                        await _transaction.CommitAsync();
                    }
                }
                
                public async Task RollbackAsync()
                {
                    if (_transaction != null)
                    {
                        await _transaction.RollbackAsync();
                    }
                }
                
                public async Task<int> SaveChangesAsync()
                {
                    return await _context.SaveChangesAsync();
                }
                
                public void Dispose()
                {
                    _transaction?.Dispose();
                }
            }
        }
        
        // ===== ADAPTERS DE SERVICIOS EXTERNOS =====
        namespace ExternalServices
        {
            public class EmailService : IEmailService
            {
                private readonly ILogger<EmailService> _logger;
                private readonly IConfiguration _configuration;
                
                public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
                {
                    _logger = logger;
                    _configuration = configuration;
                }
                
                public async Task SendWelcomeEmailAsync(string email, string firstName)
                {
                    try
                    {
                        // Implementación real de envío de email
                        _logger.LogInformation("Sending welcome email to: {Email}", email);
                        
                        // Simular envío de email
                        await Task.Delay(100);
                        
                        _logger.LogInformation("Welcome email sent successfully to: {Email}", email);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending welcome email to: {Email}", email);
                        throw;
                    }
                }
                
                public async Task SendOrderConfirmationEmailAsync(string email, int orderId, decimal totalAmount)
                {
                    try
                    {
                        _logger.LogInformation("Sending order confirmation email to: {Email} for order: {OrderId}", email, orderId);
                        
                        // Simular envío de email
                        await Task.Delay(100);
                        
                        _logger.LogInformation("Order confirmation email sent successfully to: {Email}", email);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending order confirmation email to: {Email}", email);
                        throw;
                    }
                }
                
                public async Task SendPasswordResetEmailAsync(string email, string resetToken)
                {
                    try
                    {
                        _logger.LogInformation("Sending password reset email to: {Email}", email);
                        
                        // Simular envío de email
                        await Task.Delay(100);
                        
                        _logger.LogInformation("Password reset email sent successfully to: {Email}", email);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending password reset email to: {Email}", email);
                        throw;
                    }
                }
            }
            
            public class LoggingService : ILoggingService
            {
                private readonly ILogger<LoggingService> _logger;
                
                public LoggingService(ILogger<LoggingService> logger)
                {
                    _logger = logger;
                }
                
                public void LogInformation(string message, params object[] args)
                {
                    _logger.LogInformation(message, args);
                }
                
                public void LogWarning(string message, params object[] args)
                {
                    _logger.LogWarning(message, args);
                }
                
                public void LogError(string message, Exception exception = null, params object[] args)
                {
                    if (exception != null)
                    {
                        _logger.LogError(exception, message, args);
                    }
                    else
                    {
                        _logger.LogError(message, args);
                    }
                }
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
            private readonly IUserService _userService;
            private readonly ILogger<UsersController> _logger;
            
            public UsersController(IUserService userService, ILogger<UsersController> logger)
            {
                _userService = userService;
                _logger = logger;
            }
            
            [HttpPost]
            public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
            {
                try
                {
                    var result = await _userService.CreateUserAsync(request);
                    return CreatedAtAction(nameof(GetUser), new { id = result.Id }, result);
                }
                catch (ValidationException ex)
                {
                    return BadRequest(ex.Message);
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
                    var result = await _userService.GetUserAsync(id);
                    return Ok(result);
                }
                catch (NotFoundException ex)
                {
                    return NotFound(ex.Message);
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
                    var result = await _userService.UpdateUserAsync(id, request);
                    return Ok(result);
                }
                catch (NotFoundException ex)
                {
                    return NotFound(ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in UpdateUser endpoint for ID: {UserId}", id);
                    return StatusCode(500, "Internal server error");
                }
            }
            
            [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteUser(int id)
            {
                try
                {
                    await _userService.DeleteUserAsync(id);
                    return NoContent();
                }
                catch (NotFoundException ex)
                {
                    return NotFound(ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in DeleteUser endpoint for ID: {UserId}", id);
                    return StatusCode(500, "Internal server error");
                }
            }
            
            [HttpGet]
            public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
            {
                try
                {
                    var result = await _userService.GetAllUsersAsync(page, pageSize);
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in GetAllUsers endpoint");
                    return StatusCode(500, "Internal server error");
                }
            }
        }
        
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
            public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
            {
                try
                {
                    var result = await _orderService.CreateOrderAsync(request);
                    return CreatedAtAction(nameof(GetOrder), new { id = result.Id }, result);
                }
                catch (ValidationException ex)
                {
                    return BadRequest(ex.Message);
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
                    var result = await _orderService.GetOrderAsync(id);
                    return Ok(result);
                }
                catch (NotFoundException ex)
                {
                    return NotFound(ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in GetOrder endpoint for ID: {OrderId}", id);
                    return StatusCode(500, "Internal server error");
                }
            }
            
            [HttpPut("{id}/status")]
            public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
            {
                try
                {
                    var result = await _orderService.UpdateOrderStatusAsync(id, request.Status);
                    return Ok(result);
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
                    _logger.LogError(ex, "Error in UpdateOrderStatus endpoint for ID: {OrderId}", id);
                    return StatusCode(500, "Internal server error");
                }
            }
            
            [HttpGet("user/{userId}")]
            public async Task<IActionResult> GetUserOrders(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
            {
                try
                {
                    var result = await _orderService.GetUserOrdersAsync(userId, page, pageSize);
                    return Ok(result);
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
        public static IServiceCollection AddHexagonalArchitecture(this IServiceCollection services, IConfiguration configuration)
        {
            // ===== DOMAIN SERVICES =====
            services.AddScoped<IUserDomainService, UserDomainService>();
            
            // ===== USE CASES =====
            services.AddScoped<IUserService, CreateUserUseCase>();
            services.AddScoped<IOrderService, CreateOrderUseCase>();
            
            // ===== REPOSITORIES =====
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            // ===== EXTERNAL SERVICES =====
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ILoggingService, LoggingService>();
            
            return services;
        }
    }
}

// ===== DTOs Y REQUEST MODELS =====
namespace HexagonalArchitecture.Models
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
    
    public class CreateUserRequest
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
    }
    
    public class UpdateUserRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    
    public class OrderDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }
    
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
    
    public class CreateOrderRequest
    {
        public int UserId { get; set; }
        public List<CreateOrderItemRequest> Items { get; set; }
    }
    
    public class CreateOrderItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
    
    public class UpdateOrderStatusRequest
    {
        public OrderStatus Status { get; set; }
    }
}

// ===== EXCEPCIONES PERSONALIZADAS =====
namespace HexagonalArchitecture.Exceptions
{
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }
    
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}

// Uso de Arquitectura Hexagonal
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Arquitectura Hexagonal (Ports & Adapters) ===\n");
        
        Console.WriteLine("La Arquitectura Hexagonal proporciona:");
        Console.WriteLine("1. Separación clara entre lógica de negocio e infraestructura");
        Console.WriteLine("2. Inversión de dependencias a través de interfaces");
        Console.WriteLine("3. Facilidad para cambiar tecnologías sin afectar el dominio");
        Console.WriteLine("4. Testing más sencillo con mocks de adaptadores");
        Console.WriteLine("5. Escalabilidad y mantenibilidad del código");
        
        Console.WriteLine("\nBeneficios principales:");
        Console.WriteLine("- Independencia de frameworks");
        Console.WriteLine("- Testing aislado del dominio");
        Console.WriteLine("- Cambios de tecnología sin impacto en lógica de negocio");
        Console.WriteLine("- Arquitectura clara y comprensible");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Implementación de Arquitectura Hexagonal
Implementa un sistema de gestión de productos siguiendo la arquitectura hexagonal, incluyendo puertos e interfaces.

### Ejercicio 2: Adaptadores Múltiples
Crea múltiples adaptadores para la misma funcionalidad (ej: diferentes proveedores de email, bases de datos).

### Ejercicio 3: Testing de Arquitectura
Implementa tests unitarios que verifiquen la separación entre dominio e infraestructura.

## 🔍 Puntos Clave

1. **Ports** definen contratos entre el dominio y el mundo exterior
2. **Adapters** implementan esos contratos para tecnologías específicas
3. **Inversión de Dependencias** mantiene el dominio independiente
4. **Separación de Responsabilidades** facilita testing y mantenimiento
5. **Flexibilidad** para cambiar tecnologías sin afectar lógica de negocio

## 📚 Recursos Adicionales

- [Hexagonal Architecture - Alistair Cockburn](https://alistair.cockburn.us/hexagonal-architecture/)
- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Ports and Adapters Pattern - Martin Fowler](https://martinfowler.com/articles/practical-test-pyramid.html)

---

**🎯 ¡Has completado la Clase 1! Ahora comprendes la Arquitectura Hexagonal en C#**

**📚 [Siguiente: Clase 2 - Event Sourcing y CQRS Avanzado](clase_2_event_sourcing.md)**
