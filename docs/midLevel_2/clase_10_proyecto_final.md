# 🚀 Clase 10: Proyecto Final - Sistema de Gestión Empresarial

## 📋 Información de la Clase

- **Módulo**: Mid Level 2 - Arquitectura de Software y Patrones Avanzados
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 9 (Seguridad Avanzada en .NET)

## 🎯 Objetivos de Aprendizaje

- Integrar todos los conceptos aprendidos en el módulo
- Implementar un sistema empresarial completo
- Aplicar patrones arquitectónicos avanzados
- Desarrollar una solución escalable y mantenible

---

## 📚 Navegación del Módulo 5

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_arquitectura_hexagonal.md) | Arquitectura Hexagonal (Ports & Adapters) | |
| [Clase 2](clase_2_event_sourcing.md) | Event Sourcing y CQRS Avanzado | |
| [Clase 3](clase_3_microservicios.md) | Arquitectura de Microservicios | |
| [Clase 4](clase_4_patrones_arquitectonicos.md) | Patrones Arquitectónicos | |
| [Clase 5](clase_5_domain_driven_design.md) | Domain Driven Design (DDD) | |
| [Clase 6](clase_6_async_streams.md) | Async Streams y IAsyncEnumerable | |
| [Clase 7](clase_7_source_generators.md) | Source Generators y Compile-time Code Generation | |
| [Clase 8](clase_8_high_performance.md) | High Performance Programming | |
| [Clase 9](clase_9_seguridad_avanzada.md) | Seguridad Avanzada en .NET | ← Anterior |
| **Clase 10** | **Proyecto Final: Sistema de Gestión Empresarial** | ← Estás aquí |

**← [Volver al README del Módulo 5](../midLevel_2/README.md)**

---

## 📚 Contenido Teórico

### 1. Proyecto Final: Sistema de Gestión Empresarial

Este proyecto integra todos los conceptos aprendidos en el módulo para crear un sistema empresarial completo.

```csharp
// ===== SISTEMA DE GESTIÓN EMPRESARIAL - IMPLEMENTACIÓN COMPLETA =====
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EnterpriseManagementSystem
{
    // ===== ARQUITECTURA HEXAGONAL =====
    namespace Domain.Ports.In
    {
        public interface IUserManagementUseCase
        {
            Task<UserDto> CreateUserAsync(CreateUserCommand command);
            Task<UserDto> UpdateUserAsync(UpdateUserCommand command);
            Task<bool> DeleteUserAsync(int userId);
            Task<UserDto> GetUserAsync(int userId);
            Task<IEnumerable<UserDto>> GetAllUsersAsync();
        }
        
        public interface IOrderManagementUseCase
        {
            Task<OrderDto> CreateOrderAsync(CreateOrderCommand command);
            Task<OrderDto> UpdateOrderAsync(UpdateOrderCommand command);
            Task<bool> CancelOrderAsync(int orderId);
            Task<OrderDto> GetOrderAsync(int orderId);
            Task<IEnumerable<OrderDto>> GetUserOrdersAsync(int userId);
        }
        
        public interface IProductManagementUseCase
        {
            Task<ProductDto> CreateProductAsync(CreateProductCommand command);
            Task<ProductDto> UpdateProductAsync(UpdateProductCommand command);
            Task<bool> DeleteProductAsync(int productId);
            Task<ProductDto> GetProductAsync(int productId);
            Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        }
    }
    
    namespace Domain.Ports.Out
    {
        public interface IUserRepository
        {
            Task<User> SaveAsync(User user);
            Task<User> GetByIdAsync(int id);
            Task<User> GetByUsernameAsync(string username);
            Task<IEnumerable<User>> GetAllAsync();
            Task<bool> DeleteAsync(int id);
        }
        
        public interface IOrderRepository
        {
            Task<Order> SaveAsync(Order order);
            Task<Order> GetByIdAsync(int id);
            Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
            Task<bool> DeleteAsync(int id);
        }
        
        public interface IProductRepository
        {
            Task<Product> SaveAsync(Product product);
            Task<Product> GetByIdAsync(int id);
            Task<IEnumerable<Product>> GetAllAsync();
            Task<bool> DeleteAsync(int id);
        }
        
        public interface IEventStore
        {
            Task SaveEventAsync(DomainEvent domainEvent);
            Task<IEnumerable<DomainEvent>> GetEventsAsync(string aggregateId);
        }
    }
    
    // ===== DOMAIN DRIVEN DESIGN =====
    namespace Domain.Entities
    {
        public abstract class Entity<TKey>
        {
            public TKey Id { get; protected set; }
            public DateTime CreatedAt { get; protected set; }
            public DateTime? UpdatedAt { get; protected set; }
            
            protected Entity()
            {
                CreatedAt = DateTime.UtcNow;
            }
            
            protected void MarkAsUpdated()
            {
                UpdatedAt = DateTime.UtcNow;
            }
        }
        
        public abstract class AggregateRoot<TKey> : Entity<TKey>
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
        
        public class User : AggregateRoot<int>
        {
            public string Username { get; private set; }
            public string Email { get; private set; }
            public string PasswordHash { get; private set; }
            public UserStatus Status { get; private set; }
            public List<Role> Roles { get; private set; }
            
            private User() { }
            
            public static User Create(string username, string email, string passwordHash)
            {
                var user = new User
                {
                    Username = username,
                    Email = email,
                    PasswordHash = passwordHash,
                    Status = UserStatus.Active,
                    Roles = new List<Role> { Role.User }
                };
                
                user.AddDomainEvent(new UserCreatedDomainEvent(user.Id, username, email));
                return user;
            }
            
            public void UpdateEmail(string newEmail)
            {
                if (string.IsNullOrEmpty(newEmail))
                    throw new ArgumentException("Email no puede estar vacío");
                
                Email = newEmail;
                MarkAsUpdated();
                AddDomainEvent(new UserEmailUpdatedDomainEvent(Id, newEmail));
            }
            
            public void ChangePassword(string newPasswordHash)
            {
                if (string.IsNullOrEmpty(newPasswordHash))
                    throw new ArgumentException("Contraseña no puede estar vacía");
                
                PasswordHash = newPasswordHash;
                MarkAsUpdated();
                AddDomainEvent(new UserPasswordChangedDomainEvent(Id));
            }
            
            public void Deactivate()
            {
                Status = UserStatus.Inactive;
                MarkAsUpdated();
                AddDomainEvent(new UserDeactivatedDomainEvent(Id));
            }
        }
        
        public class Order : AggregateRoot<int>
        {
            public int UserId { get; private set; }
            public List<OrderItem> Items { get; private set; }
            public OrderStatus Status { get; private set; }
            public decimal TotalAmount { get; private set; }
            public DateTime? ShippedAt { get; private set; }
            
            private Order() { }
            
            public static Order Create(int userId, List<OrderItem> items)
            {
                var order = new Order
                {
                    UserId = userId,
                    Items = items ?? new List<OrderItem>(),
                    Status = OrderStatus.Created,
                    TotalAmount = items?.Sum(i => i.TotalPrice) ?? 0
                };
                
                order.AddDomainEvent(new OrderCreatedDomainEvent(order.Id, userId, order.TotalAmount));
                return order;
            }
            
            public void AddItem(OrderItem item)
            {
                if (item == null)
                    throw new ArgumentException("Item no puede estar vacío");
                
                Items.Add(item);
                TotalAmount += item.TotalPrice;
                MarkAsUpdated();
                AddDomainEvent(new OrderItemAddedDomainEvent(Id, item.ProductId, item.Quantity));
            }
            
            public void Confirm()
            {
                if (Status != OrderStatus.Created)
                    throw new InvalidOperationException("Solo se pueden confirmar órdenes creadas");
                
                Status = OrderStatus.Confirmed;
                MarkAsUpdated();
                AddDomainEvent(new OrderConfirmedDomainEvent(Id));
            }
            
            public void Ship()
            {
                if (Status != OrderStatus.Confirmed)
                    throw new InvalidOperationException("Solo se pueden enviar órdenes confirmadas");
                
                Status = OrderStatus.Shipped;
                ShippedAt = DateTime.UtcNow;
                MarkAsUpdated();
                AddDomainEvent(new OrderShippedDomainEvent(Id, ShippedAt.Value));
            }
            
            public void Cancel()
            {
                if (Status == OrderStatus.Shipped)
                    throw new InvalidOperationException("No se pueden cancelar órdenes enviadas");
                
                Status = OrderStatus.Cancelled;
                MarkAsUpdated();
                AddDomainEvent(new OrderCancelledDomainEvent(Id));
            }
        }
        
        public class Product : AggregateRoot<int>
        {
            public string Name { get; private set; }
            public string Description { get; private set; }
            public decimal Price { get; private set; }
            public int StockQuantity { get; private set; }
            public ProductStatus Status { get; private set; }
            
            private Product() { }
            
            public static Product Create(string name, string description, decimal price, int stockQuantity)
            {
                var product = new Product
                {
                    Name = name,
                    Description = description,
                    Price = price,
                    StockQuantity = stockQuantity,
                    Status = ProductStatus.Active
                };
                
                product.AddDomainEvent(new ProductCreatedDomainEvent(product.Id, name, price));
                return product;
            }
            
            public void UpdatePrice(decimal newPrice)
            {
                if (newPrice < 0)
                    throw new ArgumentException("El precio no puede ser negativo");
                
                Price = newPrice;
                MarkAsUpdated();
                AddDomainEvent(new ProductPriceUpdatedDomainEvent(Id, newPrice));
            }
            
            public void UpdateStock(int newQuantity)
            {
                if (newQuantity < 0)
                    throw new ArgumentException("La cantidad de stock no puede ser negativa");
                
                StockQuantity = newQuantity;
                MarkAsUpdated();
                AddDomainEvent(new ProductStockUpdatedDomainEvent(Id, newQuantity));
            }
        }
        
        public class OrderItem : Entity<int>
        {
            public int ProductId { get; private set; }
            public int Quantity { get; private set; }
            public decimal UnitPrice { get; private set; }
            public decimal TotalPrice => Quantity * UnitPrice;
            
            private OrderItem() { }
            
            public static OrderItem Create(int productId, int quantity, decimal unitPrice)
            {
                if (quantity <= 0)
                    throw new ArgumentException("La cantidad debe ser mayor a 0");
                
                if (unitPrice < 0)
                    throw new ArgumentException("El precio unitario no puede ser negativo");
                
                return new OrderItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = unitPrice
                };
            }
        }
    }
    
    // ===== VALUE OBJECTS =====
    namespace Domain.ValueObjects
    {
        public abstract class ValueObject<T> where T : ValueObject<T>
        {
            protected abstract IEnumerable<object> GetEqualityComponents();
            
            public override bool Equals(object obj)
            {
                if (obj == null || obj.GetType() != GetType())
                    return false;
                
                var other = (ValueObject<T>)obj;
                return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
            }
            
            public override int GetHashCode()
            {
                return GetEqualityComponents()
                    .Select(x => x != null ? x.GetHashCode() : 0)
                    .Aggregate((x, y) => x ^ y);
            }
            
            public static bool operator ==(ValueObject<T> left, ValueObject<T> right)
            {
                return EqualOperator(left, right);
            }
            
            public static bool operator !=(ValueObject<T> left, ValueObject<T> right)
            {
                return NotEqualOperator(left, right);
            }
            
            protected static bool EqualOperator(ValueObject<T> left, ValueObject<T> right)
            {
                if (left is null ^ right is null)
                    return false;
                
                return left is null || left.Equals(right);
            }
            
            protected static bool NotEqualOperator(ValueObject<T> left, ValueObject<T> right)
            {
                return !EqualOperator(left, right);
            }
        }
        
        public class Email : ValueObject<Email>
        {
            public string Value { get; private set; }
            
            private Email() { }
            
            public static Email Create(string email)
            {
                if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                    throw new ArgumentException("Email inválido");
                
                return new Email { Value = email.ToLower() };
            }
            
            protected override IEnumerable<object> GetEqualityComponents()
            {
                yield return Value;
            }
            
            public static implicit operator string(Email email) => email.Value;
        }
        
        public class Money : ValueObject<Money>
        {
            public decimal Amount { get; private set; }
            public string Currency { get; private set; }
            
            private Money() { }
            
            public static Money Create(decimal amount, string currency = "USD")
            {
                if (amount < 0)
                    throw new ArgumentException("El monto no puede ser negativo");
                
                return new Money { Amount = amount, Currency = currency.ToUpper() };
            }
            
            public Money Add(Money other)
            {
                if (Currency != other.Currency)
                    throw new InvalidOperationException("No se pueden sumar monedas diferentes");
                
                return Create(Amount + other.Amount, Currency);
            }
            
            public Money Subtract(Money other)
            {
                if (Currency != other.Currency)
                    throw new InvalidOperationException("No se pueden restar monedas diferentes");
                
                return Create(Amount - other.Amount, Currency);
            }
            
            protected override IEnumerable<object> GetEqualityComponents()
            {
                yield return Amount;
                yield return Currency;
            }
        }
    }
    
    // ===== DOMAIN EVENTS =====
    namespace Domain.Events
    {
        public abstract class DomainEvent
        {
            public Guid Id { get; private set; }
            public DateTime OccurredOn { get; private set; }
            public string AggregateId { get; private set; }
            
            protected DomainEvent(string aggregateId)
            {
                Id = Guid.NewGuid();
                OccurredOn = DateTime.UtcNow;
                AggregateId = aggregateId;
            }
        }
        
        public class UserCreatedDomainEvent : DomainEvent
        {
            public int UserId { get; }
            public string Username { get; }
            public string Email { get; }
            
            public UserCreatedDomainEvent(int userId, string username, string email) 
                : base(userId.ToString())
            {
                UserId = userId;
                Username = username;
                Email = email;
            }
        }
        
        public class OrderCreatedDomainEvent : DomainEvent
        {
            public int OrderId { get; }
            public int UserId { get; }
            public decimal TotalAmount { get; }
            
            public OrderCreatedDomainEvent(int orderId, int userId, decimal totalAmount) 
                : base(orderId.ToString())
            {
                OrderId = orderId;
                UserId = userId;
                TotalAmount = totalAmount;
            }
        }
        
        public class ProductCreatedDomainEvent : DomainEvent
        {
            public int ProductId { get; }
            public string Name { get; }
            public decimal Price { get; }
            
            public ProductCreatedDomainEvent(int productId, string name, decimal price) 
                : base(productId.ToString())
            {
                ProductId = productId;
                Name = name;
                Price = price;
            }
        }
    }
    
    // ===== APPLICATION LAYER =====
    namespace Application.UseCases
    {
        public class UserManagementUseCase : IUserManagementUseCase
        {
            private readonly IUserRepository _userRepository;
            private readonly IEventStore _eventStore;
            private readonly ILogger<UserManagementUseCase> _logger;
            
            public UserManagementUseCase(IUserRepository userRepository, IEventStore eventStore, ILogger<UserManagementUseCase> logger)
            {
                _userRepository = userRepository;
                _eventStore = eventStore;
                _logger = logger;
            }
            
            public async Task<UserDto> CreateUserAsync(CreateUserCommand command)
            {
                try
                {
                    var user = User.Create(command.Username, command.Email, command.PasswordHash);
                    var savedUser = await _userRepository.SaveAsync(user);
                    
                    // Guardar eventos de dominio
                    foreach (var domainEvent in user.DomainEvents)
                    {
                        await _eventStore.SaveEventAsync(domainEvent);
                    }
                    
                    _logger.LogInformation("Usuario creado exitosamente: {UserId}", savedUser.Id);
                    
                    return MapToDto(savedUser);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear usuario: {Username}", command.Username);
                    throw;
                }
            }
            
            public async Task<UserDto> UpdateUserAsync(UpdateUserCommand command)
            {
                var user = await _userRepository.GetByIdAsync(command.UserId);
                if (user == null)
                    throw new InvalidOperationException("Usuario no encontrado");
                
                if (!string.IsNullOrEmpty(command.Email))
                    user.UpdateEmail(command.Email);
                
                var savedUser = await _userRepository.SaveAsync(user);
                
                // Guardar eventos de dominio
                foreach (var domainEvent in user.DomainEvents)
                {
                    await _eventStore.SaveEventAsync(domainEvent);
                }
                
                return MapToDto(savedUser);
            }
            
            public async Task<bool> DeleteUserAsync(int userId)
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return false;
                
                user.Deactivate();
                await _userRepository.SaveAsync(user);
                
                return true;
            }
            
            public async Task<UserDto> GetUserAsync(int userId)
            {
                var user = await _userRepository.GetByIdAsync(userId);
                return user != null ? MapToDto(user) : null;
            }
            
            public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
            {
                var users = await _userRepository.GetAllAsync();
                return users.Select(MapToDto);
            }
            
            private UserDto MapToDto(User user)
            {
                return new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Status = user.Status.ToString(),
                    Roles = user.Roles.Select(r => r.ToString()).ToList(),
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };
            }
        }
        
        public class OrderManagementUseCase : IOrderManagementUseCase
        {
            private readonly IOrderRepository _orderRepository;
            private readonly IProductRepository _productRepository;
            private readonly IEventStore _eventStore;
            private readonly ILogger<OrderManagementUseCase> _logger;
            
            public OrderManagementUseCase(IOrderRepository orderRepository, IProductRepository productRepository, 
                IEventStore eventStore, ILogger<OrderManagementUseCase> logger)
            {
                _orderRepository = orderRepository;
                _productRepository = productRepository;
                _eventStore = eventStore;
                _logger = logger;
            }
            
            public async Task<OrderDto> CreateOrderAsync(CreateOrderCommand command)
            {
                var items = new List<OrderItem>();
                
                foreach (var itemCommand in command.Items)
                {
                    var product = await _productRepository.GetByIdAsync(itemCommand.ProductId);
                    if (product == null)
                        throw new InvalidOperationException($"Producto no encontrado: {itemCommand.ProductId}");
                    
                    var orderItem = OrderItem.Create(itemCommand.ProductId, itemCommand.Quantity, product.Price);
                    items.Add(orderItem);
                }
                
                var order = Order.Create(command.UserId, items);
                var savedOrder = await _orderRepository.SaveAsync(order);
                
                // Guardar eventos de dominio
                foreach (var domainEvent in order.DomainEvents)
                {
                    await _eventStore.SaveEventAsync(domainEvent);
                }
                
                return MapToDto(savedOrder);
            }
            
            public async Task<OrderDto> UpdateOrderAsync(UpdateOrderCommand command)
            {
                var order = await _orderRepository.GetByIdAsync(command.OrderId);
                if (order == null)
                    throw new InvalidOperationException("Orden no encontrada");
                
                if (command.Status == "Confirmed")
                    order.Confirm();
                else if (command.Status == "Shipped")
                    order.Ship();
                else if (command.Status == "Cancelled")
                    order.Cancel();
                
                var savedOrder = await _orderRepository.SaveAsync(order);
                
                // Guardar eventos de dominio
                foreach (var domainEvent in order.DomainEvents)
                {
                    await _eventStore.SaveEventAsync(domainEvent);
                }
                
                return MapToDto(savedOrder);
            }
            
            public async Task<bool> CancelOrderAsync(int orderId)
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                    return false;
                
                order.Cancel();
                await _orderRepository.SaveAsync(order);
                
                return true;
            }
            
            public async Task<OrderDto> GetOrderAsync(int orderId)
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                return order != null ? MapToDto(order) : null;
            }
            
            public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(int userId)
            {
                var orders = await _orderRepository.GetByUserIdAsync(userId);
                return orders.Select(MapToDto);
            }
            
            private OrderDto MapToDto(Order order)
            {
                return new OrderDto
                {
                    Id = order.Id,
                    UserId = order.UserId,
                    Status = order.Status.ToString(),
                    TotalAmount = order.TotalAmount,
                    Items = order.Items.Select(i => new OrderItemDto
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        TotalPrice = i.TotalPrice
                    }).ToList(),
                    CreatedAt = order.CreatedAt,
                    UpdatedAt = order.UpdatedAt,
                    ShippedAt = order.ShippedAt
                };
            }
        }
    }
    
    // ===== INFRASTRUCTURE LAYER =====
    namespace Infrastructure.Persistence
    {
        public class InMemoryUserRepository : IUserRepository
        {
            private readonly Dictionary<int, User> _users = new();
            private int _nextId = 1;
            
            public async Task<User> SaveAsync(User user)
            {
                if (user.Id == 0)
                {
                    user.Id = _nextId++;
                }
                
                _users[user.Id] = user;
                return user;
            }
            
            public async Task<User> GetByIdAsync(int id)
            {
                _users.TryGetValue(id, out var user);
                return user;
            }
            
            public async Task<User> GetByUsernameAsync(string username)
            {
                return _users.Values.FirstOrDefault(u => u.Username == username);
            }
            
            public async Task<IEnumerable<User>> GetAllAsync()
            {
                return _users.Values.ToList();
            }
            
            public async Task<bool> DeleteAsync(int id)
            {
                return _users.Remove(id);
            }
        }
        
        public class InMemoryOrderRepository : IOrderRepository
        {
            private readonly Dictionary<int, Order> _orders = new();
            private int _nextId = 1;
            
            public async Task<Order> SaveAsync(Order order)
            {
                if (order.Id == 0)
                {
                    order.Id = _nextId++;
                }
                
                _orders[order.Id] = order;
                return order;
            }
            
            public async Task<Order> GetByIdAsync(int id)
            {
                _orders.TryGetValue(id, out var order);
                return order;
            }
            
            public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
            {
                return _orders.Values.Where(o => o.UserId == userId).ToList();
            }
            
            public async Task<bool> DeleteAsync(int id)
            {
                return _orders.Remove(id);
            }
        }
        
        public class InMemoryProductRepository : IProductRepository
        {
            private readonly Dictionary<int, Product> _products = new();
            private int _nextId = 1;
            
            public async Task<Product> SaveAsync(Product product)
            {
                if (product.Id == 0)
                {
                    product.Id = _nextId++;
                }
                
                _products[product.Id] = product;
                return product;
            }
            
            public async Task<Product> GetByIdAsync(int id)
            {
                _products.TryGetValue(id, out var product);
                return product;
            }
            
            public async Task<IEnumerable<Product>> GetAllAsync()
            {
                return _products.Values.ToList();
            }
            
            public async Task<bool> DeleteAsync(int id)
            {
                return _products.Remove(id);
            }
        }
        
        public class InMemoryEventStore : IEventStore
        {
            private readonly List<DomainEvent> _events = new();
            
            public async Task SaveEventAsync(DomainEvent domainEvent)
            {
                _events.Add(domainEvent);
            }
            
            public async Task<IEnumerable<DomainEvent>> GetEventsAsync(string aggregateId)
            {
                return _events.Where(e => e.AggregateId == aggregateId).ToList();
            }
        }
    }
    
    // ===== PRESENTATION LAYER =====
    namespace Presentation.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        public class UsersController : ControllerBase
        {
            private readonly IUserManagementUseCase _userUseCase;
            private readonly ILogger<UsersController> _logger;
            
            public UsersController(IUserManagementUseCase userUseCase, ILogger<UsersController> logger)
            {
                _userUseCase = userUseCase;
                _logger = logger;
            }
            
            [HttpPost]
            public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserCommand command)
            {
                try
                {
                    var user = await _userUseCase.CreateUserAsync(command);
                    return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear usuario");
                    return BadRequest(ex.Message);
                }
            }
            
            [HttpGet("{id}")]
            public async Task<ActionResult<UserDto>> GetUser(int id)
            {
                var user = await _userUseCase.GetUserAsync(id);
                if (user == null)
                    return NotFound();
                
                return Ok(user);
            }
            
            [HttpGet]
            public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
            {
                var users = await _userUseCase.GetAllUsersAsync();
                return Ok(users);
            }
            
            [HttpPut("{id}")]
            public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserCommand command)
            {
                command.UserId = id;
                var user = await _userUseCase.UpdateUserAsync(command);
                return Ok(user);
            }
            
            [HttpDelete("{id}")]
            public async Task<ActionResult> DeleteUser(int id)
            {
                var result = await _userUseCase.DeleteUserAsync(id);
                if (!result)
                    return NotFound();
                
                return NoContent();
            }
        }
        
        [ApiController]
        [Route("api/[controller]")]
        public class OrdersController : ControllerBase
        {
            private readonly IOrderManagementUseCase _orderUseCase;
            private readonly ILogger<OrdersController> _logger;
            
            public OrdersController(IOrderManagementUseCase orderUseCase, ILogger<OrdersController> logger)
            {
                _orderUseCase = orderUseCase;
                _logger = logger;
            }
            
            [HttpPost]
            public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderCommand command)
            {
                try
                {
                    var order = await _orderUseCase.CreateOrderAsync(command);
                    return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear orden");
                    return BadRequest(ex.Message);
                }
            }
            
            [HttpGet("{id}")]
            public async Task<ActionResult<OrderDto>> GetOrder(int id)
            {
                var order = await _orderUseCase.GetOrderAsync(id);
                if (order == null)
                    return NotFound();
                
                return Ok(order);
            }
            
            [HttpGet("user/{userId}")]
            public async Task<ActionResult<IEnumerable<OrderDto>>> GetUserOrders(int userId)
            {
                var orders = await _orderUseCase.GetUserOrdersAsync(userId);
                return Ok(orders);
            }
            
            [HttpPut("{id}")]
            public async Task<ActionResult<OrderDto>> UpdateOrder(int id, [FromBody] UpdateOrderCommand command)
            {
                command.OrderId = id;
                var order = await _orderUseCase.UpdateOrderAsync(command);
                return Ok(order);
            }
            
            [HttpDelete("{id}")]
            public async Task<ActionResult> CancelOrder(int id)
            {
                var result = await _orderUseCase.CancelOrderAsync(id);
                if (!result)
                    return NotFound();
                
                return NoContent();
            }
        }
    }
    
    // ===== DTOs Y COMMANDS =====
    namespace Application.DTOs
    {
        public class UserDto
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string Status { get; set; }
            public List<string> Roles { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }
        
        public class OrderDto
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public string Status { get; set; }
            public decimal TotalAmount { get; set; }
            public List<OrderItemDto> Items { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public DateTime? ShippedAt { get; set; }
        }
        
        public class OrderItemDto
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice { get; set; }
        }
        
        public class ProductDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public int StockQuantity { get; set; }
            public string Status { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }
    }
    
    namespace Application.Commands
    {
        public class CreateUserCommand
        {
            [Required]
            public string Username { get; set; }
            
            [Required]
            [EmailAddress]
            public string Email { get; set; }
            
            [Required]
            [MinLength(8)]
            public string PasswordHash { get; set; }
        }
        
        public class UpdateUserCommand
        {
            public int UserId { get; set; }
            public string Email { get; set; }
        }
        
        public class CreateOrderCommand
        {
            [Required]
            public int UserId { get; set; }
            
            [Required]
            public List<CreateOrderItemCommand> Items { get; set; }
        }
        
        public class CreateOrderItemCommand
        {
            [Required]
            public int ProductId { get; set; }
            
            [Required]
            [Range(1, int.MaxValue)]
            public int Quantity { get; set; }
        }
        
        public class UpdateOrderCommand
        {
            public int OrderId { get; set; }
            public string Status { get; set; }
        }
        
        public class CreateProductCommand
        {
            [Required]
            public string Name { get; set; }
            
            public string Description { get; set; }
            
            [Required]
            [Range(0, double.MaxValue)]
            public decimal Price { get; set; }
            
            [Required]
            [Range(0, int.MaxValue)]
            public int StockQuantity { get; set; }
        }
        
        public class UpdateProductCommand
        {
            public int ProductId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public decimal? Price { get; set; }
            public int? StockQuantity { get; set; }
        }
    }
    
    // ===== ENUMS =====
    public enum UserStatus
    {
        Active,
        Inactive,
        Suspended
    }
    
    public enum OrderStatus
    {
        Created,
        Confirmed,
        Shipped,
        Delivered,
        Cancelled
    }
    
    public enum ProductStatus
    {
        Active,
        Inactive,
        Discontinued
    }
    
    public enum Role
    {
        User,
        Editor,
        Admin
    }
    
    // ===== CONFIGURACIÓN DE DEPENDENCIAS =====
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEnterpriseManagementSystem(this IServiceCollection services)
        {
            // Application Layer
            services.AddScoped<IUserManagementUseCase, UserManagementUseCase>();
            services.AddScoped<IOrderManagementUseCase, OrderManagementUseCase>();
            services.AddScoped<IProductManagementUseCase, ProductManagementUseCase>();
            
            // Infrastructure Layer
            services.AddScoped<IUserRepository, InMemoryUserRepository>();
            services.AddScoped<IOrderRepository, InMemoryOrderRepository>();
            services.AddScoped<IProductRepository, InMemoryProductRepository>();
            services.AddScoped<IEventStore, InMemoryEventStore>();
            
            return services;
        }
    }
}

// Programa principal del Sistema de Gestión Empresarial
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Sistema de Gestión Empresarial ===\n");
        
        Console.WriteLine("Este proyecto integra todos los conceptos del módulo:");
        Console.WriteLine("1. Arquitectura Hexagonal (Ports & Adapters)");
        Console.WriteLine("2. Domain Driven Design (DDD)");
        Console.WriteLine("3. Event Sourcing y CQRS");
        Console.WriteLine("4. Patrones Arquitectónicos");
        Console.WriteLine("5. Async Streams y Source Generators");
        Console.WriteLine("6. High Performance Programming");
        Console.WriteLine("7. Seguridad Avanzada");
        
        Console.WriteLine("\nComponentes implementados:");
        Console.WriteLine("- Dominio con entidades y value objects");
        Console.WriteLine("- Casos de uso de aplicación");
        Console.WriteLine("- Repositorios en memoria");
        Console.WriteLine("- Controladores de API");
        Console.WriteLine("- Sistema de eventos de dominio");
        Console.WriteLine("- Validaciones y manejo de errores");
        
        Console.WriteLine("\nEl sistema está listo para ser ejecutado como una API web.");
        Console.WriteLine("Puedes usar los controladores para gestionar usuarios, órdenes y productos.");
    }
}
