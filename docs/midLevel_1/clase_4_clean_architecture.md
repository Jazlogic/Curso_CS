# 🚀 Clase 4: Clean Architecture

## 📋 Información de la Clase

- **Módulo**: Mid Level 1 - Programación Avanzada y Patrones de Diseño
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 3 (Programación Paralela y TPL)

## 🎯 Objetivos de Aprendizaje

- Comprender los principios fundamentales de Clean Architecture
- Implementar la separación de capas (Domain, Application, Infrastructure, Presentation)
- Aplicar el principio de inversión de dependencias (DIP)
- Implementar CQRS (Command Query Responsibility Segregation)
- Crear arquitecturas escalables y mantenibles

---

## 📚 Navegación del Módulo 4

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_patrones_diseno_intermedios.md) | Patrones de Diseño Intermedios | |
| [Clase 2](clase_2_programacion_asincrona_avanzada.md) | Programación Asíncrona Avanzada | |
| [Clase 3](clase_3_programacion_paralela.md) | Programación Paralela y TPL | ← Anterior |
| **Clase 4** | **Clean Architecture** | ← Estás aquí |
| [Clase 5](clase_5_dependency_injection.md) | Dependency Injection Avanzada | Siguiente → |
| [Clase 6](clase_6_logging_monitoreo.md) | Logging y Monitoreo | |
| [Clase 7](clase_7_refactoring_clean_code.md) | Refactoring y Clean Code | |
| [Clase 8](clase_8_testing_integracion.md) | Testing de Integración | |
| [Clase 9](clase_9_testing_comportamiento.md) | Testing de Comportamiento (BDD) | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de E-commerce | |

**← [Volver al README del Módulo 4](../midLevel_1/README.md)**

---

## 📚 Contenido Teórico

### 1. Principios Fundamentales de Clean Architecture

Clean Architecture se basa en la separación de responsabilidades y la independencia de frameworks.

```csharp
// Estructura de capas de Clean Architecture
namespace CleanArchitecture
{
    // ===== CAPA DOMAIN (Entidades y Reglas de Negocio) =====
    namespace Domain.Entities
    {
        // Entidad base abstracta
        public abstract class Entity
        {
            public Guid Id { get; protected set; }
            public DateTime CreatedAt { get; protected set; }
            public DateTime? UpdatedAt { get; protected set; }
            
            protected Entity()
            {
                Id = Guid.NewGuid();
                CreatedAt = DateTime.UtcNow;
            }
            
            protected Entity(Guid id)
            {
                Id = id;
                CreatedAt = DateTime.UtcNow;
            }
            
            public override bool Equals(object obj)
            {
                if (obj is not Entity other)
                    return false;
                
                if (ReferenceEquals(this, other))
                    return true;
                
                if (GetType() != other.GetType())
                    return false;
                
                return Id == other.Id;
            }
            
            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }
        
        // Entidad de Usuario
        public class User : Entity
        {
            public string Email { get; private set; }
            public string FirstName { get; private set; }
            public string LastName { get; private set; }
            public bool IsActive { get; private set; }
            public List<Order> Orders { get; private set; }
            
            private User() { }
            
            public User(string email, string firstName, string lastName)
            {
                if (string.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("Email no puede estar vacío", nameof(email));
                
                if (string.IsNullOrWhiteSpace(firstName))
                    throw new ArgumentException("Nombre no puede estar vacío", nameof(firstName));
                
                if (string.IsNullOrWhiteSpace(lastName))
                    throw new ArgumentException("Apellido no puede estar vacío", nameof(lastName));
                
                Email = email.ToLowerInvariant();
                FirstName = firstName;
                LastName = lastName;
                IsActive = true;
                Orders = new List<Order>();
            }
            
            public void UpdateProfile(string firstName, string lastName)
            {
                if (string.IsNullOrWhiteSpace(firstName))
                    throw new ArgumentException("Nombre no puede estar vacío", nameof(firstName));
                
                if (string.IsNullOrWhiteSpace(lastName))
                    throw new ArgumentException("Apellido no puede estar vacío", nameof(lastName));
                
                FirstName = firstName;
                LastName = lastName;
                UpdatedAt = DateTime.UtcNow;
            }
            
            public void Deactivate()
            {
                IsActive = false;
                UpdatedAt = DateTime.UtcNow;
            }
            
            public void Activate()
            {
                IsActive = true;
                UpdatedAt = DateTime.UtcNow;
            }
            
            public void AddOrder(Order order)
            {
                if (order == null)
                    throw new ArgumentNullException(nameof(order));
                
                Orders.Add(order);
            }
            
            public string FullName => $"{FirstName} {LastName}";
        }
        
        // Entidad de Orden
        public class Order : Entity
        {
            public Guid UserId { get; private set; }
            public decimal TotalAmount { get; private set; }
            public OrderStatus Status { get; private set; }
            public List<OrderItem> Items { get; private set; }
            public DateTime OrderDate { get; private set; }
            
            private Order() { }
            
            public Order(Guid userId)
            {
                UserId = userId;
                Status = OrderStatus.Pending;
                Items = new List<OrderItem>();
                OrderDate = DateTime.UtcNow;
            }
            
            public void AddItem(OrderItem item)
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item));
                
                Items.Add(item);
                RecalculateTotal();
            }
            
            public void RemoveItem(Guid itemId)
            {
                var item = Items.FirstOrDefault(i => i.Id == itemId);
                if (item != null)
                {
                    Items.Remove(item);
                    RecalculateTotal();
                }
            }
            
            public void Confirm()
            {
                if (Status != OrderStatus.Pending)
                    throw new InvalidOperationException("Solo se pueden confirmar órdenes pendientes");
                
                if (!Items.Any())
                    throw new InvalidOperationException("No se puede confirmar una orden sin items");
                
                Status = OrderStatus.Confirmed;
                UpdatedAt = DateTime.UtcNow;
            }
            
            public void Cancel()
            {
                if (Status == OrderStatus.Delivered)
                    throw new InvalidOperationException("No se puede cancelar una orden entregada");
                
                Status = OrderStatus.Cancelled;
                UpdatedAt = DateTime.UtcNow;
            }
            
            private void RecalculateTotal()
            {
                TotalAmount = Items.Sum(item => item.Subtotal);
            }
        }
        
        // Entidad de Item de Orden
        public class OrderItem : Entity
        {
            public Guid ProductId { get; private set; }
            public string ProductName { get; private set; }
            public int Quantity { get; private set; }
            public decimal UnitPrice { get; private set; }
            public decimal Subtotal => Quantity * UnitPrice;
            
            private OrderItem() { }
            
            public OrderItem(Guid productId, string productName, int quantity, decimal unitPrice)
            {
                if (productId == Guid.Empty)
                    throw new ArgumentException("ProductId no puede estar vacío", nameof(productId));
                
                if (string.IsNullOrWhiteSpace(productName))
                    throw new ArgumentException("Nombre del producto no puede estar vacío", nameof(productName));
                
                if (quantity <= 0)
                    throw new ArgumentException("Cantidad debe ser mayor a 0", nameof(quantity));
                
                if (unitPrice <= 0)
                    throw new ArgumentException("Precio unitario debe ser mayor a 0", nameof(unitPrice));
                
                ProductId = productId;
                ProductName = productName;
                Quantity = quantity;
                UnitPrice = unitPrice;
            }
            
            public void UpdateQuantity(int newQuantity)
            {
                if (newQuantity <= 0)
                    throw new ArgumentException("Cantidad debe ser mayor a 0", nameof(newQuantity));
                
                Quantity = newQuantity;
                UpdatedAt = DateTime.UtcNow;
            }
        }
        
        // Enumeraciones
        public enum OrderStatus
        {
            Pending,
            Confirmed,
            Shipped,
            Delivered,
            Cancelled
        }
    }
    
    // ===== CAPA DOMAIN (Interfaces de Repositorio) =====
    namespace Domain.Interfaces
    {
        // Interfaz base para repositorios
        public interface IRepository<T> where T : Entity
        {
            Task<T> GetByIdAsync(Guid id);
            Task<IEnumerable<T>> GetAllAsync();
            Task<T> AddAsync(T entity);
            Task UpdateAsync(T entity);
            Task DeleteAsync(Guid id);
            Task<bool> ExistsAsync(Guid id);
        }
        
        // Interfaz específica para repositorio de usuarios
        public interface IUserRepository : IRepository<User>
        {
            Task<User> GetByEmailAsync(string email);
            Task<IEnumerable<User>> GetActiveUsersAsync();
            Task<bool> EmailExistsAsync(string email);
        }
        
        // Interfaz específica para repositorio de órdenes
        public interface IOrderRepository : IRepository<Order>
        {
            Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId);
            Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status);
            Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate);
        }
        
        // Interfaz para servicios de dominio
        public interface IUserService
        {
            Task<User> CreateUserAsync(string email, string firstName, string lastName);
            Task UpdateUserProfileAsync(Guid userId, string firstName, string lastName);
            Task DeactivateUserAsync(Guid userId);
            Task<bool> IsEmailUniqueAsync(string email);
        }
        
        public interface IOrderService
        {
            Task<Order> CreateOrderAsync(Guid userId);
            Task AddItemToOrderAsync(Guid orderId, Guid productId, string productName, int quantity, decimal unitPrice);
            Task ConfirmOrderAsync(Guid orderId);
            Task CancelOrderAsync(Guid orderId);
            Task<decimal> CalculateOrderTotalAsync(Guid orderId);
        }
    }
    
    // ===== CAPA APPLICATION (Casos de Uso) =====
    namespace Application.UseCases
    {
        // Caso de uso para crear usuario
        public class CreateUserUseCase
        {
            private readonly IUserRepository _userRepository;
            private readonly IUserService _userService;
            
            public CreateUserUseCase(IUserRepository userRepository, IUserService userService)
            {
                _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
                _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            }
            
            public async Task<User> ExecuteAsync(string email, string firstName, string lastName)
            {
                // Validaciones de negocio
                if (await _userService.IsEmailUniqueAsync(email))
                {
                    var user = await _userService.CreateUserAsync(email, firstName, lastName);
                    await _userRepository.AddAsync(user);
                    return user;
                }
                
                throw new InvalidOperationException($"El email {email} ya está registrado");
            }
        }
        
        // Caso de uso para crear orden
        public class CreateOrderUseCase
        {
            private readonly IOrderRepository _orderRepository;
            private readonly IUserRepository _userRepository;
            private readonly IOrderService _orderService;
            
            public CreateOrderUseCase(
                IOrderRepository orderRepository, 
                IUserRepository userRepository,
                IOrderService orderService)
            {
                _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
                _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
                _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            }
            
            public async Task<Order> ExecuteAsync(Guid userId)
            {
                // Verificar que el usuario existe y está activo
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new ArgumentException("Usuario no encontrado", nameof(userId));
                
                if (!user.IsActive)
                    throw new InvalidOperationException("Usuario inactivo");
                
                // Crear la orden
                var order = await _orderService.CreateOrderAsync(userId);
                await _orderRepository.AddAsync(order);
                
                return order;
            }
        }
        
        // Caso de uso para agregar item a orden
        public class AddItemToOrderUseCase
        {
            private readonly IOrderRepository _orderRepository;
            private readonly IOrderService _orderService;
            
            public AddItemToOrderUseCase(IOrderRepository orderRepository, IOrderService orderService)
            {
                _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
                _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            }
            
            public async Task<Order> ExecuteAsync(Guid orderId, Guid productId, string productName, int quantity, decimal unitPrice)
            {
                // Obtener la orden
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                    throw new ArgumentException("Orden no encontrada", nameof(orderId));
                
                if (order.Status != OrderStatus.Pending)
                    throw new InvalidOperationException("Solo se pueden agregar items a órdenes pendientes");
                
                // Agregar el item
                await _orderService.AddItemToOrderAsync(orderId, productId, productName, quantity, unitPrice);
                
                // Actualizar la orden
                await _orderRepository.UpdateAsync(order);
                
                return order;
            }
        }
    }
    
    // ===== CAPA INFRASTRUCTURE (Implementaciones) =====
    namespace Infrastructure.Repositories
    {
        // Implementación en memoria del repositorio de usuarios
        public class InMemoryUserRepository : IUserRepository
        {
            private readonly Dictionary<Guid, User> _users = new();
            private readonly Dictionary<string, Guid> _emailIndex = new();
            
            public async Task<User> GetByIdAsync(Guid id)
            {
                await Task.CompletedTask; // Simular operación asíncrona
                return _users.TryGetValue(id, out var user) ? user : null;
            }
            
            public async Task<IEnumerable<User>> GetAllAsync()
            {
                await Task.CompletedTask;
                return _users.Values.ToList();
            }
            
            public async Task<User> AddAsync(User entity)
            {
                await Task.CompletedTask;
                _users[entity.Id] = entity;
                _emailIndex[entity.Email] = entity.Id;
                return entity;
            }
            
            public async Task UpdateAsync(User entity)
            {
                await Task.CompletedTask;
                if (_users.ContainsKey(entity.Id))
                {
                    var oldUser = _users[entity.Id];
                    _emailIndex.Remove(oldUser.Email);
                    _users[entity.Id] = entity;
                    _emailIndex[entity.Email] = entity.Id;
                }
            }
            
            public async Task DeleteAsync(Guid id)
            {
                await Task.CompletedTask;
                if (_users.TryGetValue(id, out var user))
                {
                    _emailIndex.Remove(user.Email);
                    _users.Remove(id);
                }
            }
            
            public async Task<bool> ExistsAsync(Guid id)
            {
                await Task.CompletedTask;
                return _users.ContainsKey(id);
            }
            
            public async Task<User> GetByEmailAsync(string email)
            {
                await Task.CompletedTask;
                if (_emailIndex.TryGetValue(email.ToLowerInvariant(), out var userId))
                {
                    return _users[userId];
                }
                return null;
            }
            
            public async Task<IEnumerable<User>> GetActiveUsersAsync()
            {
                await Task.CompletedTask;
                return _users.Values.Where(u => u.IsActive).ToList();
            }
            
            public async Task<bool> EmailExistsAsync(string email)
            {
                await Task.CompletedTask;
                return _emailIndex.ContainsKey(email.ToLowerInvariant());
            }
        }
        
        // Implementación en memoria del repositorio de órdenes
        public class InMemoryOrderRepository : IOrderRepository
        {
            private readonly Dictionary<Guid, Order> _orders = new();
            private readonly Dictionary<Guid, List<Guid>> _userOrdersIndex = new();
            
            public async Task<Order> GetByIdAsync(Guid id)
            {
                await Task.CompletedTask;
                return _orders.TryGetValue(id, out var order) ? order : null;
            }
            
            public async Task<IEnumerable<Order>> GetAllAsync()
            {
                await Task.CompletedTask;
                return _orders.Values.ToList();
            }
            
            public async Task<Order> AddAsync(Order entity)
            {
                await Task.CompletedTask;
                _orders[entity.Id] = entity;
                
                if (!_userOrdersIndex.ContainsKey(entity.UserId))
                    _userOrdersIndex[entity.UserId] = new List<Guid>();
                
                _userOrdersIndex[entity.UserId].Add(entity.Id);
                return entity;
            }
            
            public async Task UpdateAsync(Order entity)
            {
                await Task.CompletedTask;
                if (_orders.ContainsKey(entity.Id))
                {
                    _orders[entity.Id] = entity;
                }
            }
            
            public async Task DeleteAsync(Guid id)
            {
                await Task.CompletedTask;
                if (_orders.TryGetValue(id, out var order))
                {
                    if (_userOrdersIndex.ContainsKey(order.UserId))
                    {
                        _userOrdersIndex[order.UserId].Remove(id);
                    }
                    _orders.Remove(id);
                }
            }
            
            public async Task<bool> ExistsAsync(Guid id)
            {
                await Task.CompletedTask;
                return _orders.ContainsKey(id);
            }
            
            public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId)
            {
                await Task.CompletedTask;
                if (_userOrdersIndex.TryGetValue(userId, out var orderIds))
                {
                    return orderIds.Select(id => _orders[id]).ToList();
                }
                return new List<Order>();
            }
            
            public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status)
            {
                await Task.CompletedTask;
                return _orders.Values.Where(o => o.Status == status).ToList();
            }
            
            public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
            {
                await Task.CompletedTask;
                return _orders.Values
                    .Where(o => o.Status == OrderStatus.Delivered && 
                               o.OrderDate >= startDate && o.OrderDate <= endDate)
                    .Sum(o => o.TotalAmount);
            }
        }
    }
    
    // ===== CAPA INFRASTRUCTURE (Servicios) =====
    namespace Infrastructure.Services
    {
        // Implementación del servicio de usuarios
        public class UserService : IUserService
        {
            private readonly IUserRepository _userRepository;
            
            public UserService(IUserRepository userRepository)
            {
                _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            }
            
            public async Task<User> CreateUserAsync(string email, string firstName, string lastName)
            {
                var user = new User(email, firstName, lastName);
                return user;
            }
            
            public async Task UpdateUserProfileAsync(Guid userId, string firstName, string lastName)
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new ArgumentException("Usuario no encontrado", nameof(userId));
                
                user.UpdateProfile(firstName, lastName);
                await _userRepository.UpdateAsync(user);
            }
            
            public async Task DeactivateUserAsync(Guid userId)
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new ArgumentException("Usuario no encontrado", nameof(userId));
                
                user.Deactivate();
                await _userRepository.UpdateAsync(user);
            }
            
            public async Task<bool> IsEmailUniqueAsync(string email)
            {
                return !await _userRepository.EmailExistsAsync(email);
            }
        }
        
        // Implementación del servicio de órdenes
        public class OrderService : IOrderService
        {
            public async Task<Order> CreateOrderAsync(Guid userId)
            {
                var order = new Order(userId);
                return order;
            }
            
            public async Task AddItemToOrderAsync(Guid orderId, Guid productId, string productName, int quantity, decimal unitPrice)
            {
                // En una implementación real, se obtendría la orden del repositorio
                // Por simplicidad, creamos un item directamente
                var item = new OrderItem(productId, productName, quantity, unitPrice);
                
                // Aquí se agregaría el item a la orden
                await Task.CompletedTask;
            }
            
            public async Task ConfirmOrderAsync(Guid orderId)
            {
                // En una implementación real, se obtendría y confirmaría la orden
                await Task.CompletedTask;
            }
            
            public async Task CancelOrderAsync(Guid orderId)
            {
                // En una implementación real, se obtendría y cancelaría la orden
                await Task.CompletedTask;
            }
            
            public async Task<decimal> CalculateOrderTotalAsync(Guid orderId)
            {
                // En una implementación real, se calcularía el total de la orden
                await Task.CompletedTask;
                return 0m;
            }
        }
    }
}

// Uso de Clean Architecture
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Clean Architecture - Sistema de Usuarios y Órdenes ===\n");
        
        // Configurar dependencias (en una aplicación real se usaría un DI container)
        IUserRepository userRepository = new CleanArchitecture.Infrastructure.Repositories.InMemoryUserRepository();
        IOrderRepository orderRepository = new CleanArchitecture.Infrastructure.Repositories.InMemoryOrderRepository();
        IUserService userService = new CleanArchitecture.Infrastructure.Services.UserService(userRepository);
        IOrderService orderService = new CleanArchitecture.Infrastructure.Services.OrderService();
        
        // Casos de uso
        var createUserUseCase = new CleanArchitecture.Application.UseCases.CreateUserUseCase(userRepository, userService);
        var createOrderUseCase = new CleanArchitecture.Application.UseCases.CreateOrderUseCase(orderRepository, userRepository, orderService);
        var addItemToOrderUseCase = new CleanArchitecture.Application.UseCases.AddItemToOrderUseCase(orderRepository, orderService);
        
        try
        {
            // 1. Crear usuario
            Console.WriteLine("1. Creando Usuario:");
            var user = await createUserUseCase.ExecuteAsync("usuario@email.com", "Juan", "Pérez");
            Console.WriteLine($"Usuario creado: {user.FullName} ({user.Email})");
            
            Console.WriteLine();
            
            // 2. Crear orden
            Console.WriteLine("2. Creando Orden:");
            var order = await createOrderUseCase.ExecuteAsync(user.Id);
            Console.WriteLine($"Orden creada: {order.Id} - Estado: {order.Status}");
            
            Console.WriteLine();
            
            // 3. Agregar item a la orden
            Console.WriteLine("3. Agregando Item a la Orden:");
            await addItemToOrderUseCase.ExecuteAsync(order.Id, Guid.NewGuid(), "Producto A", 2, 25.99m);
            Console.WriteLine("Item agregado a la orden");
            
            Console.WriteLine();
            
            // 4. Mostrar información del usuario
            Console.WriteLine("4. Información del Usuario:");
            var retrievedUser = await userRepository.GetByIdAsync(user.Id);
            Console.WriteLine($"Usuario: {retrievedUser.FullName}");
            Console.WriteLine($"Órdenes: {retrievedUser.Orders.Count}");
            
            Console.WriteLine();
            
            // 5. Mostrar información de la orden
            Console.WriteLine("5. Información de la Orden:");
            var retrievedOrder = await orderRepository.GetByIdAsync(order.Id);
            Console.WriteLine($"Orden: {retrievedOrder.Id}");
            Console.WriteLine($"Items: {retrievedOrder.Items.Count}");
            Console.WriteLine($"Total: ${retrievedOrder.TotalAmount:F2}");
            
            Console.WriteLine();
            
            // 6. Confirmar orden
            Console.WriteLine("6. Confirmando Orden:");
            retrievedOrder.Confirm();
            await orderRepository.UpdateAsync(retrievedOrder);
            Console.WriteLine($"Orden confirmada: {retrievedOrder.Status}");
            
            Console.WriteLine();
            
            // 7. Mostrar estadísticas
            Console.WriteLine("7. Estadísticas:");
            var activeUsers = await userRepository.GetActiveUsersAsync();
            var confirmedOrders = await orderRepository.GetByStatusAsync(CleanArchitecture.Domain.Entities.OrderStatus.Confirmed);
            var totalRevenue = await orderRepository.GetTotalRevenueAsync(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
            
            Console.WriteLine($"Usuarios activos: {activeUsers.Count()}");
            Console.WriteLine($"Órdenes confirmadas: {confirmedOrders.Count()}");
            Console.WriteLine($"Ingresos últimos 30 días: ${totalRevenue:F2}");
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

### 2. CQRS (Command Query Responsibility Segregation)

CQRS separa las operaciones de lectura y escritura para mejorar el rendimiento y la escalabilidad.

```csharp
// Implementación de CQRS
namespace CQRS
{
    // ===== COMMANDS (Comandos para modificar estado) =====
    namespace Commands
    {
        // Comando base
        public abstract class Command
        {
            public Guid Id { get; set; }
            public DateTime Timestamp { get; set; }
            
            protected Command()
            {
                Id = Guid.NewGuid();
                Timestamp = DateTime.UtcNow;
            }
        }
        
        // Comando para crear usuario
        public class CreateUserCommand : Command
        {
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }
        
        // Comando para actualizar usuario
        public class UpdateUserCommand : Command
        {
            public Guid UserId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }
        
        // Comando para crear orden
        public class CreateOrderCommand : Command
        {
            public Guid UserId { get; set; }
        }
        
        // Comando para agregar item a orden
        public class AddItemToOrderCommand : Command
        {
            public Guid OrderId { get; set; }
            public Guid ProductId { get; set; }
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
        }
        
        // Comando para confirmar orden
        public class ConfirmOrderCommand : Command
        {
            public Guid OrderId { get; set; }
        }
    }
    
    // ===== QUERIES (Consultas para leer datos) =====
    namespace Queries
    {
        // Query base
        public abstract class Query<TResult>
        {
            public Guid Id { get; set; }
            public DateTime Timestamp { get; set; }
            
            protected Query()
            {
                Id = Guid.NewGuid();
                Timestamp = DateTime.UtcNow;
            }
        }
        
        // Query para obtener usuario por ID
        public class GetUserByIdQuery : Query<UserDto>
        {
            public Guid UserId { get; set; }
        }
        
        // Query para obtener usuario por email
        public class GetUserByEmailQuery : Query<UserDto>
        {
            public string Email { get; set; }
        }
        
        // Query para obtener usuarios activos
        public class GetActiveUsersQuery : Query<IEnumerable<UserDto>>
        {
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 10;
        }
        
        // Query para obtener orden por ID
        public class GetOrderByIdQuery : Query<OrderDto>
        {
            public Guid OrderId { get; set; }
        }
        
        // Query para obtener órdenes de usuario
        public class GetUserOrdersQuery : Query<IEnumerable<OrderDto>>
        {
            public Guid UserId { get; set; }
            public OrderStatus? Status { get; set; }
        }
        
        // Query para obtener estadísticas
        public class GetOrderStatisticsQuery : Query<OrderStatisticsDto>
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }
    }
    
    // ===== DTOs (Data Transfer Objects) =====
    namespace DTOs
    {
        public class UserDto
        {
            public Guid Id { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreatedAt { get; set; }
            public string FullName => $"{FirstName} {LastName}";
        }
        
        public class OrderDto
        {
            public Guid Id { get; set; }
            public Guid UserId { get; set; }
            public decimal TotalAmount { get; set; }
            public OrderStatus Status { get; set; }
            public DateTime OrderDate { get; set; }
            public List<OrderItemDto> Items { get; set; } = new();
        }
        
        public class OrderItemDto
        {
            public Guid Id { get; set; }
            public Guid ProductId { get; set; }
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Subtotal { get; set; }
        }
        
        public class OrderStatisticsDto
        {
            public int TotalOrders { get; set; }
            public decimal TotalRevenue { get; set; }
            public int ConfirmedOrders { get; set; }
            public int CancelledOrders { get; set; }
            public decimal AverageOrderValue { get; set; }
        }
    }
    
    // ===== COMMAND HANDLERS =====
    namespace CommandHandlers
    {
        public interface ICommandHandler<TCommand> where TCommand : Command
        {
            Task HandleAsync(TCommand command);
        }
        
        // Manejador para crear usuario
        public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
        {
            private readonly IUserRepository _userRepository;
            private readonly IUserService _userService;
            
            public CreateUserCommandHandler(IUserRepository userRepository, IUserService userService)
            {
                _userRepository = userRepository;
                _userService = userService;
            }
            
            public async Task HandleAsync(CreateUserCommand command)
            {
                if (await _userService.IsEmailUniqueAsync(command.Email))
                {
                    var user = await _userService.CreateUserAsync(command.Email, command.FirstName, command.LastName);
                    await _userRepository.AddAsync(user);
                }
                else
                {
                    throw new InvalidOperationException($"El email {command.Email} ya está registrado");
                }
            }
        }
        
        // Manejador para actualizar usuario
        public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand>
        {
            private readonly IUserRepository _userRepository;
            private readonly IUserService _userService;
            
            public UpdateUserCommandHandler(IUserRepository userRepository, IUserService userService)
            {
                _userRepository = userRepository;
                _userService = userService;
            }
            
            public async Task HandleAsync(UpdateUserCommand command)
            {
                await _userService.UpdateUserProfileAsync(command.UserId, command.FirstName, command.LastName);
            }
        }
        
        // Manejador para crear orden
        public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand>
        {
            private readonly IOrderRepository _orderRepository;
            private readonly IUserRepository _userRepository;
            private readonly IOrderService _orderService;
            
            public CreateOrderCommandHandler(IOrderRepository orderRepository, IUserRepository userRepository, IOrderService orderService)
            {
                _orderRepository = orderRepository;
                _userRepository = userRepository;
                _orderService = orderService;
            }
            
            public async Task HandleAsync(CreateOrderCommand command)
            {
                var user = await _userRepository.GetByIdAsync(command.UserId);
                if (user == null)
                    throw new ArgumentException("Usuario no encontrado");
                
                if (!user.IsActive)
                    throw new InvalidOperationException("Usuario inactivo");
                
                var order = await _orderService.CreateOrderAsync(command.UserId);
                await _orderRepository.AddAsync(order);
            }
        }
    }
    
    // ===== QUERY HANDLERS =====
    namespace QueryHandlers
    {
        public interface IQueryHandler<TQuery, TResult> where TQuery : Query<TResult>
        {
            Task<TResult> HandleAsync(TQuery query);
        }
        
        // Manejador para obtener usuario por ID
        public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserDto>
        {
            private readonly IUserRepository _userRepository;
            
            public GetUserByIdQueryHandler(IUserRepository userRepository)
            {
                _userRepository = userRepository;
            }
            
            public async Task<UserDto> HandleAsync(GetUserByIdQuery query)
            {
                var user = await _userRepository.GetByIdAsync(query.UserId);
                if (user == null)
                    return null;
                
                return new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                };
            }
        }
        
        // Manejador para obtener usuarios activos
        public class GetActiveUsersQueryHandler : IQueryHandler<GetActiveUsersQuery, IEnumerable<UserDto>>
        {
            private readonly IUserRepository _userRepository;
            
            public GetActiveUsersQueryHandler(IUserRepository userRepository)
            {
                _userRepository = userRepository;
            }
            
            public async Task<IEnumerable<UserDto>> HandleAsync(GetActiveUsersQuery query)
            {
                var users = await _userRepository.GetActiveUsersAsync();
                
                return users
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(user => new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt
                    });
            }
        }
        
        // Manejador para obtener estadísticas de órdenes
        public class GetOrderStatisticsQueryHandler : IQueryHandler<GetOrderStatisticsQuery, OrderStatisticsDto>
        {
            private readonly IOrderRepository _orderRepository;
            
            public GetOrderStatisticsQueryHandler(IOrderRepository orderRepository)
            {
                _orderRepository = orderRepository;
            }
            
            public async Task<OrderStatisticsDto> HandleAsync(GetOrderStatisticsQuery query)
            {
                var confirmedOrders = await _orderRepository.GetByStatusAsync(OrderStatus.Confirmed);
                var cancelledOrders = await _orderRepository.GetByStatusAsync(OrderStatus.Cancelled);
                var totalRevenue = await _orderRepository.GetTotalRevenueAsync(query.StartDate, query.EndDate);
                
                var allOrders = confirmedOrders.Concat(cancelledOrders).ToList();
                
                return new OrderStatisticsDto
                {
                    TotalOrders = allOrders.Count,
                    TotalRevenue = totalRevenue,
                    ConfirmedOrders = confirmedOrders.Count(),
                    CancelledOrders = cancelledOrders.Count(),
                    AverageOrderValue = allOrders.Any() ? allOrders.Average(o => o.TotalAmount) : 0
                };
            }
        }
    }
    
    // ===== MEDIATOR (Orquestador de comandos y queries) =====
    public class Mediator
    {
        private readonly Dictionary<Type, object> _commandHandlers = new();
        private readonly Dictionary<Type, object> _queryHandlers = new();
        
        public void RegisterCommandHandler<TCommand>(ICommandHandler<TCommand> handler) where TCommand : Command
        {
            _commandHandlers[typeof(TCommand)] = handler;
        }
        
        public void RegisterQueryHandler<TQuery, TResult>(IQueryHandler<TQuery, TResult> handler) where TQuery : Query<TResult>
        {
            _queryHandlers[typeof(TQuery)] = handler;
        }
        
        public async Task SendAsync<TCommand>(TCommand command) where TCommand : Command
        {
            if (_commandHandlers.TryGetValue(typeof(TCommand), out var handler))
            {
                var commandHandler = (ICommandHandler<TCommand>)handler;
                await commandHandler.HandleAsync(command);
            }
            else
            {
                throw new InvalidOperationException($"No se encontró manejador para el comando {typeof(TCommand).Name}");
            }
        }
        
        public async Task<TResult> SendAsync<TQuery, TResult>(TQuery query) where TQuery : Query<TResult>
        {
            if (_queryHandlers.TryGetValue(typeof(TQuery), out var handler))
            {
                var queryHandler = (IQueryHandler<TQuery, TResult>)handler;
                return await queryHandler.HandleAsync(query);
            }
            else
            {
                throw new InvalidOperationException($"No se encontró manejador para la query {typeof(TQuery).Name}");
            }
        }
    }
}

// Uso de CQRS
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== CQRS - Command Query Responsibility Segregation ===\n");
        
        // Configurar dependencias
        IUserRepository userRepository = new CleanArchitecture.Infrastructure.Repositories.InMemoryUserRepository();
        IOrderRepository orderRepository = new CleanArchitecture.Infrastructure.Repositories.InMemoryOrderRepository();
        IUserService userService = new CleanArchitecture.Infrastructure.Services.UserService(userRepository);
        IOrderService orderService = new CleanArchitecture.Infrastructure.Services.OrderService();
        
        // Configurar mediator
        var mediator = new CQRS.Mediator();
        
        // Registrar command handlers
        mediator.RegisterCommandHandler(new CQRS.CommandHandlers.CreateUserCommandHandler(userRepository, userService));
        mediator.RegisterCommandHandler(new CQRS.CommandHandlers.UpdateUserCommandHandler(userRepository, userService));
        mediator.RegisterCommandHandler(new CQRS.CommandHandlers.CreateOrderCommandHandler(orderRepository, userRepository, orderService));
        
        // Registrar query handlers
        mediator.RegisterQueryHandler(new CQRS.QueryHandlers.GetUserByIdQueryHandler(userRepository));
        mediator.RegisterQueryHandler(new CQRS.QueryHandlers.GetActiveUsersQueryHandler(userRepository));
        mediator.RegisterQueryHandler(new CQRS.QueryHandlers.GetOrderStatisticsQueryHandler(orderRepository));
        
        try
        {
            // 1. Crear usuario usando comando
            Console.WriteLine("1. Creando Usuario (Command):");
            var createUserCommand = new CQRS.Commands.CreateUserCommand
            {
                Email = "usuario@email.com",
                FirstName = "María",
                LastName = "García"
            };
            
            await mediator.SendAsync(createUserCommand);
            Console.WriteLine($"Usuario creado con comando: {createUserCommand.Id}");
            
            Console.WriteLine();
            
            // 2. Obtener usuario usando query
            Console.WriteLine("2. Obteniendo Usuario (Query):");
            var getUserQuery = new CQRS.Queries.GetUserByIdQuery
            {
                UserId = Guid.NewGuid() // En una implementación real, se usaría el ID del usuario creado
            };
            
            var user = await mediator.SendAsync<CQRS.Queries.GetUserByIdQuery, CQRS.DTOs.UserDto>(getUserQuery);
            if (user != null)
            {
                Console.WriteLine($"Usuario encontrado: {user.FullName} ({user.Email})");
            }
            else
            {
                Console.WriteLine("Usuario no encontrado");
            }
            
            Console.WriteLine();
            
            // 3. Obtener usuarios activos
            Console.WriteLine("3. Obteniendo Usuarios Activos (Query):");
            var getActiveUsersQuery = new CQRS.Queries.GetActiveUsersQuery
            {
                Page = 1,
                PageSize = 5
            };
            
            var activeUsers = await mediator.SendAsync<CQRS.Queries.GetActiveUsersQuery, IEnumerable<CQRS.DTOs.UserDto>>(getActiveUsersQuery);
            Console.WriteLine($"Usuarios activos encontrados: {activeUsers.Count()}");
            
            Console.WriteLine();
            
            // 4. Obtener estadísticas
            Console.WriteLine("4. Obteniendo Estadísticas (Query):");
            var getStatsQuery = new CQRS.Queries.GetOrderStatisticsQuery
            {
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow
            };
            
            var stats = await mediator.SendAsync<CQRS.Queries.GetOrderStatisticsQuery, CQRS.DTOs.OrderStatisticsDto>(getStatsQuery);
            Console.WriteLine($"Total de órdenes: {stats.TotalOrders}");
            Console.WriteLine($"Ingresos totales: ${stats.TotalRevenue:F2}");
            Console.WriteLine($"Órdenes confirmadas: {stats.ConfirmedOrders}");
            Console.WriteLine($"Órdenes canceladas: {stats.CancelledOrders}");
            Console.WriteLine($"Valor promedio por orden: ${stats.AverageOrderValue:F2}");
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Sistema de Gestión de Biblioteca
Implementa un sistema de gestión de biblioteca usando Clean Architecture con entidades Book, Author, Member y Loan.

### Ejercicio 2: API REST con CQRS
Crea una API REST que implemente CQRS para operaciones de usuarios y productos.

### Ejercicio 3: Sistema de Notificaciones
Desarrolla un sistema de notificaciones que siga los principios de Clean Architecture.

## 🔍 Puntos Clave

1. **Clean Architecture** separa las responsabilidades en capas bien definidas
2. **Domain Layer** contiene las entidades y reglas de negocio centrales
3. **Application Layer** implementa los casos de uso de la aplicación
4. **Infrastructure Layer** maneja la implementación de interfaces externas
5. **CQRS** separa las operaciones de lectura y escritura para mejor rendimiento

## 📚 Recursos Adicionales

- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern - Microsoft Docs](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [SOLID Principles - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/architectural-principles)

---

**🎯 ¡Has completado la Clase 4! Ahora dominas Clean Architecture y CQRS en C#**

**📚 [Siguiente: Clase 5 - Dependency Injection Avanzada](clase_5_dependency_injection.md)**
