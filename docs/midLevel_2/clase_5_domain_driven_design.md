# 🚀 Clase 5: Domain Driven Design (DDD)

## 📋 Información de la Clase

- **Módulo**: Mid Level 2 - Arquitectura de Software y Patrones Avanzados
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 4 (Patrones Arquitectónicos)

## 🎯 Objetivos de Aprendizaje

- Aplicar principios de DDD
- Implementar Value Objects y Entities
- Crear Aggregates y Domain Services
- Implementar Domain Events

---

## 📚 Navegación del Módulo 5

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_arquitectura_hexagonal.md) | Arquitectura Hexagonal (Ports & Adapters) | |
| [Clase 2](clase_2_event_sourcing.md) | Event Sourcing y CQRS Avanzado | |
| [Clase 3](clase_3_microservicios.md) | Arquitectura de Microservicios | |
| [Clase 4](clase_4_patrones_arquitectonicos.md) | Patrones Arquitectónicos | ← Anterior |
| **Clase 5** | **Domain Driven Design (DDD)** | ← Estás aquí |
| [Clase 6](clase_6_async_streams.md) | Async Streams y IAsyncEnumerable | Siguiente → |
| [Clase 7](clase_7_source_generators.md) | Source Generators y Compile-time Code Generation | |
| [Clase 8](clase_8_high_performance.md) | High Performance Programming | |
| [Clase 9](clase_9_seguridad_avanzada.md) | Seguridad Avanzada en .NET | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Gestión Empresarial | |

**← [Volver al README del Módulo 5](../midLevel_2/README.md)**

---

## 📚 Contenido Teórico

### 1. Domain Driven Design (DDD)

DDD es un enfoque para el desarrollo de software que prioriza el dominio del negocio y la lógica sobre la infraestructura técnica.

```csharp
// ===== DOMAIN DRIVEN DESIGN - IMPLEMENTACIÓN COMPLETA =====
namespace DomainDrivenDesign
{
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
            
            public Email(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Email cannot be empty");
                
                if (!IsValidEmail(value))
                    throw new ArgumentException("Invalid email format");
                
                Value = value.ToLowerInvariant();
            }
            
            private bool IsValidEmail(string email)
            {
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
            
            protected override IEnumerable<object> GetEqualityComponents()
            {
                yield return Value;
            }
            
            public static implicit operator string(Email email) => email.Value;
            public static explicit operator Email(string value) => new Email(value);
        }
        
        public class Money : ValueObject<Money>
        {
            public decimal Amount { get; private set; }
            public string Currency { get; private set; }
            
            private Money() { }
            
            public Money(decimal amount, string currency = "USD")
            {
                if (amount < 0)
                    throw new ArgumentException("Amount cannot be negative");
                
                if (string.IsNullOrWhiteSpace(currency))
                    throw new ArgumentException("Currency cannot be empty");
                
                Amount = amount;
                Currency = currency.ToUpperInvariant();
            }
            
            public Money Add(Money other)
            {
                if (Currency != other.Currency)
                    throw new InvalidOperationException("Cannot add money with different currencies");
                
                return new Money(Amount + other.Amount, Currency);
            }
            
            public Money Subtract(Money other)
            {
                if (Currency != other.Currency)
                    throw new InvalidOperationException("Cannot subtract money with different currencies");
                
                var result = Amount - other.Amount;
                if (result < 0)
                    throw new InvalidOperationException("Result cannot be negative");
                
                return new Money(result, Currency);
            }
            
            public Money Multiply(decimal factor)
            {
                if (factor < 0)
                    throw new ArgumentException("Factor cannot be negative");
                
                return new Money(Amount * factor, Currency);
            }
            
            protected override IEnumerable<object> GetEqualityComponents()
            {
                yield return Amount;
                yield return Currency;
            }
            
            public override string ToString()
            {
                return $"{Amount:F2} {Currency}";
            }
        }
        
        public class Address : ValueObject<Address>
        {
            public string Street { get; private set; }
            public string City { get; private set; }
            public string State { get; private set; }
            public string PostalCode { get; private set; }
            public string Country { get; private set; }
            
            private Address() { }
            
            public Address(string street, string city, string state, string postalCode, string country)
            {
                Street = street ?? throw new ArgumentException("Street cannot be null");
                City = city ?? throw new ArgumentException("City cannot be null");
                State = state ?? throw new ArgumentException("State cannot be null");
                PostalCode = postalCode ?? throw new ArgumentException("Postal code cannot be null");
                Country = country ?? throw new ArgumentException("Country cannot be null");
            }
            
            protected override IEnumerable<object> GetEqualityComponents()
            {
                yield return Street;
                yield return City;
                yield return State;
                yield return PostalCode;
                yield return Country;
            }
            
            public override string ToString()
            {
                return $"{Street}, {City}, {State} {PostalCode}, {Country}";
            }
        }
    }
    
    // ===== ENTITIES =====
    namespace Domain.Entities
    {
        public abstract class Entity<TKey>
        {
            public TKey Id { get; protected set; }
            
            protected Entity() { }
            
            protected Entity(TKey id)
            {
                Id = id;
            }
            
            public override bool Equals(object obj)
            {
                if (obj == null || obj.GetType() != GetType())
                    return false;
                
                var other = (Entity<TKey>)obj;
                return Id.Equals(other.Id);
            }
            
            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
            
            public static bool operator ==(Entity<TKey> left, Entity<TKey> right)
            {
                return left?.Equals(right) ?? right is null;
            }
            
            public static bool operator !=(Entity<TKey> left, Entity<TKey> right)
            {
                return !(left == right);
            }
        }
        
        public class User : Entity<int>
        {
            public Email Email { get; private set; }
            public string FirstName { get; private set; }
            public string LastName { get; private set; }
            public UserStatus Status { get; private set; }
            public Address Address { get; private set; }
            public DateTime CreatedAt { get; private set; }
            public DateTime? LastLoginAt { get; private set; }
            public List<Role> Roles { get; private set; }
            
            private readonly List<IDomainEvent> _domainEvents = new();
            public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
            
            private User() 
            {
                Roles = new List<Role>();
            }
            
            public User(Email email, string firstName, string lastName, Address address)
                : this()
            {
                Email = email;
                FirstName = firstName;
                LastName = lastName;
                Address = address;
                Status = UserStatus.Active;
                CreatedAt = DateTime.UtcNow;
                
                AddDomainEvent(new UserCreatedDomainEvent(this));
            }
            
            public void UpdateProfile(string firstName, string lastName, Address address)
            {
                if (Status != UserStatus.Active)
                    throw new InvalidOperationException("Cannot update profile of inactive user");
                
                FirstName = firstName;
                LastName = lastName;
                Address = address;
                
                AddDomainEvent(new UserProfileUpdatedDomainEvent(this));
            }
            
            public void Deactivate(string reason)
            {
                if (Status != UserStatus.Active)
                    throw new InvalidOperationException("User is already inactive");
                
                Status = UserStatus.Inactive;
                
                AddDomainEvent(new UserDeactivatedDomainEvent(this, reason));
            }
            
            public void AddRole(Role role)
            {
                if (role == null)
                    throw new ArgumentException("Role cannot be null");
                
                if (Roles.Any(r => r.Name == role.Name))
                    throw new InvalidOperationException($"User already has role: {role.Name}");
                
                Roles.Add(role);
                
                AddDomainEvent(new UserRoleAddedDomainEvent(this, role));
            }
            
            public void RemoveRole(string roleName)
            {
                var role = Roles.FirstOrDefault(r => r.Name == roleName);
                if (role == null)
                    throw new InvalidOperationException($"User does not have role: {roleName}");
                
                Roles.Remove(role);
                
                AddDomainEvent(new UserRoleRemovedDomainEvent(this, role));
            }
            
            public void RecordLogin()
            {
                if (Status != UserStatus.Active)
                    throw new InvalidOperationException("Cannot record login for inactive user");
                
                LastLoginAt = DateTime.UtcNow;
                
                AddDomainEvent(new UserLoginRecordedDomainEvent(this));
            }
            
            public bool HasRole(string roleName)
            {
                return Roles.Any(r => r.Name == roleName);
            }
            
            public bool IsActive()
            {
                return Status == UserStatus.Active;
            }
            
            private void AddDomainEvent(IDomainEvent domainEvent)
            {
                _domainEvents.Add(domainEvent);
            }
            
            public void ClearDomainEvents()
            {
                _domainEvents.Clear();
            }
        }
        
        public class Role : Entity<int>
        {
            public string Name { get; private set; }
            public string Description { get; private set; }
            public List<Permission> Permissions { get; private set; }
            
            private Role()
            {
                Permissions = new List<Permission>();
            }
            
            public Role(string name, string description = null)
                : this()
            {
                Name = name ?? throw new ArgumentException("Role name cannot be null");
                Description = description;
            }
            
            public void AddPermission(Permission permission)
            {
                if (permission == null)
                    throw new ArgumentException("Permission cannot be null");
                
                if (Permissions.Any(p => p.Name == permission.Name))
                    throw new InvalidOperationException($"Role already has permission: {permission.Name}");
                
                Permissions.Add(permission);
            }
            
            public void RemovePermission(string permissionName)
            {
                var permission = Permissions.FirstOrDefault(p => p.Name == permissionName);
                if (permission == null)
                    throw new InvalidOperationException($"Role does not have permission: {permissionName}");
                
                Permissions.Remove(permission);
            }
            
            public bool HasPermission(string permissionName)
            {
                return Permissions.Any(p => p.Name == permissionName);
            }
        }
        
        public class Permission : Entity<int>
        {
            public string Name { get; private set; }
            public string Description { get; private set; }
            
            private Permission() { }
            
            public Permission(string name, string description = null)
            {
                Name = name ?? throw new ArgumentException("Permission name cannot be null");
                Description = description;
            }
        }
        
        public enum UserStatus
        {
            Active = 1,
            Inactive = 2,
            Suspended = 3
        }
    }
    
    // ===== AGGREGATES =====
    namespace Domain.Aggregates
    {
        public abstract class AggregateRoot<TKey> : Entity<TKey>
        {
            private readonly List<IDomainEvent> _domainEvents = new();
            public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
            
            protected void AddDomainEvent(IDomainEvent domainEvent)
            {
                _domainEvents.Add(domainEvent);
            }
            
            public void ClearDomainEvents()
            {
                _domainEvents.Clear();
            }
        }
        
        public class Order : AggregateRoot<int>
        {
            public int UserId { get; private set; }
            public OrderStatus Status { get; private set; }
            public Money TotalAmount { get; private set; }
            public DateTime OrderDate { get; private set; }
            public Address ShippingAddress { get; private set; }
            public List<OrderItem> Items { get; private set; }
            public string CancellationReason { get; private set; }
            public DateTime? CancelledAt { get; private set; }
            
            private Order()
            {
                Items = new List<OrderItem>();
                TotalAmount = new Money(0);
            }
            
            public Order(int userId, Address shippingAddress)
                : this()
            {
                UserId = userId;
                ShippingAddress = shippingAddress ?? throw new ArgumentException("Shipping address cannot be null");
                Status = OrderStatus.Pending;
                OrderDate = DateTime.UtcNow;
                
                AddDomainEvent(new OrderCreatedDomainEvent(this));
            }
            
            public void AddItem(Product product, int quantity)
            {
                if (product == null)
                    throw new ArgumentException("Product cannot be null");
                
                if (quantity <= 0)
                    throw new ArgumentException("Quantity must be greater than zero");
                
                if (Status != OrderStatus.Pending)
                    throw new InvalidOperationException("Cannot add items to confirmed order");
                
                var existingItem = Items.FirstOrDefault(i => i.ProductId == product.Id);
                if (existingItem != null)
                {
                    existingItem.UpdateQuantity(existingItem.Quantity + quantity);
                }
                else
                {
                    var item = new OrderItem(product.Id, product.Name, quantity, product.Price);
                    Items.Add(item);
                }
                
                CalculateTotal();
                
                AddDomainEvent(new OrderItemAddedDomainEvent(this, product, quantity));
            }
            
            public void RemoveItem(int productId)
            {
                if (Status != OrderStatus.Pending)
                    throw new InvalidOperationException("Cannot remove items from confirmed order");
                
                var item = Items.FirstOrDefault(i => i.ProductId == productId);
                if (item == null)
                    throw new InvalidOperationException($"Item with product ID {productId} not found");
                
                Items.Remove(item);
                CalculateTotal();
                
                AddDomainEvent(new OrderItemRemovedDomainEvent(this, productId));
            }
            
            public void Confirm()
            {
                if (Status != OrderStatus.Pending)
                    throw new InvalidOperationException("Order cannot be confirmed in current status");
                
                if (!Items.Any())
                    throw new InvalidOperationException("Cannot confirm order without items");
                
                var oldStatus = Status;
                Status = OrderStatus.Confirmed;
                
                AddDomainEvent(new OrderStatusChangedDomainEvent(this, oldStatus, Status));
            }
            
            public void Ship()
            {
                if (Status != OrderStatus.Confirmed)
                    throw new InvalidOperationException("Order cannot be shipped in current status");
                
                var oldStatus = Status;
                Status = OrderStatus.Shipped;
                
                AddDomainEvent(new OrderStatusChangedDomainEvent(this, oldStatus, Status));
            }
            
            public void Deliver()
            {
                if (Status != OrderStatus.Shipped)
                    throw new InvalidOperationException("Order cannot be delivered in current status");
                
                var oldStatus = Status;
                Status = OrderStatus.Delivered;
                
                AddDomainEvent(new OrderStatusChangedDomainEvent(this, oldStatus, Status));
            }
            
            public void Cancel(string reason)
            {
                if (Status == OrderStatus.Delivered)
                    throw new InvalidOperationException("Delivered orders cannot be cancelled");
                
                if (Status == OrderStatus.Cancelled)
                    throw new InvalidOperationException("Order is already cancelled");
                
                if (string.IsNullOrWhiteSpace(reason))
                    throw new ArgumentException("Cancellation reason is required");
                
                var oldStatus = Status;
                Status = OrderStatus.Cancelled;
                CancellationReason = reason;
                CancelledAt = DateTime.UtcNow;
                
                AddDomainEvent(new OrderCancelledDomainEvent(this, reason));
            }
            
            private void CalculateTotal()
            {
                var total = Items.Sum(item => item.TotalPrice.Amount);
                TotalAmount = new Money(total);
            }
        }
        
        public class OrderItem : Entity<int>
        {
            public int ProductId { get; private set; }
            public string ProductName { get; private set; }
            public int Quantity { get; private set; }
            public Money UnitPrice { get; private set; }
            public Money TotalPrice { get; private set; }
            
            private OrderItem() { }
            
            public OrderItem(int productId, string productName, int quantity, Money unitPrice)
            {
                ProductId = productId;
                ProductName = productName ?? throw new ArgumentException("Product name cannot be null");
                Quantity = quantity;
                UnitPrice = unitPrice ?? throw new ArgumentException("Unit price cannot be null");
                CalculateTotalPrice();
            }
            
            public void UpdateQuantity(int newQuantity)
            {
                if (newQuantity <= 0)
                    throw new ArgumentException("Quantity must be greater than zero");
                
                Quantity = newQuantity;
                CalculateTotalPrice();
            }
            
            private void CalculateTotalPrice()
            {
                TotalPrice = UnitPrice.Multiply(Quantity);
            }
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
            bool IsEmailUnique(Email email);
            bool IsPasswordStrong(string password);
            string GenerateUsername(string firstName, string lastName);
            bool CanUserAccessResource(User user, string resource, string action);
        }
        
        public class UserDomainService : IUserDomainService
        {
            private readonly IUserRepository _userRepository;
            
            public UserDomainService(IUserRepository userRepository)
            {
                _userRepository = userRepository;
            }
            
            public bool IsEmailUnique(Email email)
            {
                var existingUser = _userRepository.GetByEmailAsync(email.Value).Result;
                return existingUser == null;
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
            
            public bool CanUserAccessResource(User user, string resource, string action)
            {
                if (user == null || !user.IsActive())
                    return false;
                
                var requiredPermission = $"{resource}.{action}";
                return user.Roles.Any(role => role.HasPermission(requiredPermission));
            }
        }
        
        public interface IOrderDomainService
        {
            bool CanUserCreateOrder(User user);
            bool IsProductAvailable(Product product, int quantity);
            Money CalculateOrderTotal(List<OrderItem> items);
            bool CanOrderBeCancelled(Order order);
        }
        
        public class OrderDomainService : IOrderDomainService
        {
            private readonly IProductRepository _productRepository;
            
            public OrderDomainService(IProductRepository productRepository)
            {
                _productRepository = productRepository;
            }
            
            public bool CanUserCreateOrder(User user)
            {
                return user != null && user.IsActive() && user.HasRole("Customer");
            }
            
            public bool IsProductAvailable(Product product, int quantity)
            {
                return product != null && product.IsActive && product.StockQuantity >= quantity;
            }
            
            public Money CalculateOrderTotal(List<OrderItem> items)
            {
                if (items == null || !items.Any())
                    return new Money(0);
                
                var total = items.Sum(item => item.TotalPrice.Amount);
                return new Money(total);
            }
            
            public bool CanOrderBeCancelled(Order order)
            {
                return order != null && 
                       order.Status != OrderStatus.Delivered && 
                       order.Status != OrderStatus.Cancelled;
            }
        }
    }
    
    // ===== DOMAIN EVENTS =====
    namespace Domain.Events
    {
        public interface IDomainEvent
        {
            DateTime OccurredOn { get; }
        }
        
        public abstract class DomainEvent : IDomainEvent
        {
            public DateTime OccurredOn { get; private set; }
            
            protected DomainEvent()
            {
                OccurredOn = DateTime.UtcNow;
            }
        }
        
        public class UserCreatedDomainEvent : DomainEvent
        {
            public User User { get; private set; }
            
            public UserCreatedDomainEvent(User user)
            {
                User = user;
            }
        }
        
        public class UserProfileUpdatedDomainEvent : DomainEvent
        {
            public User User { get; private set; }
            
            public UserProfileUpdatedDomainEvent(User user)
            {
                User = user;
            }
        }
        
        public class UserDeactivatedDomainEvent : DomainEvent
        {
            public User User { get; private set; }
            public string Reason { get; private set; }
            
            public UserDeactivatedDomainEvent(User user, string reason)
            {
                User = user;
                Reason = reason;
            }
        }
        
        public class UserRoleAddedDomainEvent : DomainEvent
        {
            public User User { get; private set; }
            public Role Role { get; private set; }
            
            public UserRoleAddedDomainEvent(User user, Role role)
            {
                User = user;
                Role = role;
            }
        }
        
        public class UserRoleRemovedDomainEvent : DomainEvent
        {
            public User User { get; private set; }
            public Role Role { get; private set; }
            
            public UserRoleRemovedDomainEvent(User user, Role role)
            {
                User = user;
                Role = role;
            }
        }
        
        public class UserLoginRecordedDomainEvent : DomainEvent
        {
            public User User { get; private set; }
            
            public UserLoginRecordedDomainEvent(User user)
            {
                User = user;
            }
        }
        
        public class OrderCreatedDomainEvent : DomainEvent
        {
            public Order Order { get; private set; }
            
            public OrderCreatedDomainEvent(Order order)
            {
                Order = order;
            }
        }
        
        public class OrderItemAddedDomainEvent : DomainEvent
        {
            public Order Order { get; private set; }
            public Product Product { get; private set; }
            public int Quantity { get; private set; }
            
            public OrderItemAddedDomainEvent(Order order, Product product, int quantity)
            {
                Order = order;
                Product = product;
                Quantity = quantity;
            }
        }
        
        public class OrderItemRemovedDomainEvent : DomainEvent
        {
            public Order Order { get; private set; }
            public int ProductId { get; private set; }
            
            public OrderItemRemovedDomainEvent(Order order, int productId)
            {
                Order = order;
                ProductId = productId;
            }
        }
        
        public class OrderStatusChangedDomainEvent : DomainEvent
        {
            public Order Order { get; private set; }
            public OrderStatus OldStatus { get; private set; }
            public OrderStatus NewStatus { get; private set; }
            
            public OrderStatusChangedDomainEvent(Order order, OrderStatus oldStatus, OrderStatus newStatus)
            {
                Order = order;
                OldStatus = oldStatus;
                NewStatus = newStatus;
            }
        }
        
        public class OrderCancelledDomainEvent : DomainEvent
        {
            public Order Order { get; private set; }
            public string Reason { get; private set; }
            
            public OrderCancelledDomainEvent(Order order, string reason)
            {
                Order = order;
                Reason = reason;
            }
        }
    }
    
    // ===== REPOSITORIES =====
    namespace Domain.Repositories
    {
        public interface IUserRepository
        {
            Task<User> GetByIdAsync(int id);
            Task<User> GetByEmailAsync(string email);
            Task<IEnumerable<User>> GetAllAsync();
            Task<User> AddAsync(User user);
            Task UpdateAsync(User user);
            Task DeleteAsync(int id);
        }
        
        public interface IOrderRepository
        {
            Task<Order> GetByIdAsync(int id);
            Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
            Task<IEnumerable<Order>> GetAllAsync();
            Task<Order> AddAsync(Order order);
            Task UpdateAsync(Order order);
            Task DeleteAsync(int id);
        }
        
        public interface IProductRepository
        {
            Task<Product> GetByIdAsync(int id);
            Task<IEnumerable<Product>> GetAllAsync();
            Task<Product> AddAsync(Product product);
            Task UpdateAsync(Product product);
            Task DeleteAsync(int id);
        }
    }
    
    // ===== ENTIDADES ADICIONALES =====
    public class Product : Entity<int>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Money Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
    }
}

// ===== CONFIGURACIÓN =====
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainDrivenDesign(this IServiceCollection services)
    {
        // Registrar domain services
        services.AddScoped<IUserDomainService, UserDomainService>();
        services.AddScoped<IOrderDomainService, OrderDomainService>();
        
        // Registrar repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        
        return services;
    }
}

// Uso de Domain Driven Design
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Domain Driven Design (DDD) ===\n");
        
        Console.WriteLine("DDD proporciona:");
        Console.WriteLine("1. Value Objects para conceptos inmutables");
        Console.WriteLine("2. Entities con identidad única");
        Console.WriteLine("3. Aggregates para consistencia de datos");
        Console.WriteLine("4. Domain Services para lógica de negocio");
        Console.WriteLine("5. Domain Events para comunicación");
        
        Console.WriteLine("\nBeneficios principales:");
        Console.WriteLine("- Modelo de dominio rico y expresivo");
        Console.WriteLine("- Lógica de negocio centralizada");
        Console.WriteLine("- Código más mantenible y testeable");
        Console.WriteLine("- Mejor comunicación con expertos del dominio");
        Console.WriteLine("- Arquitectura alineada con el negocio");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Value Objects
Implementa Value Objects para conceptos como PhoneNumber, CreditCard, y SocialSecurityNumber.

### Ejercicio 2: Aggregates
Crea un Aggregate para ShoppingCart con reglas de negocio complejas.

### Ejercicio 3: Domain Services
Implementa un Domain Service para validación de reglas de negocio complejas.

## 🔍 Puntos Clave

1. **Value Objects** son inmutables y se comparan por valor
2. **Entities** tienen identidad única y pueden cambiar estado
3. **Aggregates** mantienen consistencia de datos
4. **Domain Services** contienen lógica de negocio compleja
5. **Domain Events** comunican cambios importantes

## 📚 Recursos Adicionales

- [Domain-Driven Design - Eric Evans](https://www.domainlanguage.com/ddd/)
- [DDD Reference - Martin Fowler](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [DDD Patterns](https://martinfowler.com/bliki/DomainDrivenDesign.html)

---

**🎯 ¡Has completado la Clase 5! Ahora comprendes Domain Driven Design (DDD) en C#**

**📚 [Siguiente: Clase 6 - Async Streams y IAsyncEnumerable](clase_6_async_streams.md)**
