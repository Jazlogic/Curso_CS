# 🚀 Clase 1: Arquitectura Limpia Avanzada

## 📋 Información de la Clase

- **Módulo**: Senior 1 - Arquitectura de Software Empresarial
- **Duración**: 2 horas
- **Nivel**: Senior
- **Prerrequisitos**: Completar Módulo 5 (Mid Level 2)

## 🎯 Objetivos de Aprendizaje

- Implementar Clean Architecture con principios SOLID avanzados
- Aplicar inversión de dependencias de manera efectiva
- Diseñar capas bien definidas y separadas
- Implementar patrones de inyección de dependencias avanzados

---

## 📚 Navegación del Módulo 6

| Clase | Tema | Enlace |
|-------|------|--------|
| **Clase 1** | **Arquitectura Limpia Avanzada** | ← Estás aquí |
| [Clase 2](clase_2_event_driven_architecture.md) | Event-Driven Architecture | Siguiente → |
| [Clase 3](clase_3_microservicios_avanzada.md) | Arquitectura de Microservicios Avanzada | |
| [Clase 4](clase_4_patrones_enterprise.md) | Patrones de Diseño Enterprise | |
| [Clase 5](clase_5_arquitectura_datos_avanzada.md) | Arquitectura de Datos Avanzada | |
| [Clase 6](clase_6_calidad_codigo_metricas.md) | Calidad del Código y Métricas | |
| [Clase 7](clase_7_monitoreo_observabilidad.md) | Monitoreo y Observabilidad | |
| [Clase 8](clase_8_arquitectura_evolutiva.md) | Arquitectura Evolutiva | |
| [Clase 9](clase_9_seguridad_enterprise.md) | Arquitectura de Seguridad Enterprise | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Plataforma Empresarial | |

**← [Volver al README del Módulo 6](../senior_1/README.md)**

---

## 📚 Contenido Teórico

### 1. Arquitectura Limpia Avanzada

La Clean Architecture se basa en la separación de responsabilidades y la inversión de dependencias para crear sistemas mantenibles y escalables.

```csharp
// ===== ARQUITECTURA LIMPIA AVANZADA - IMPLEMENTACIÓN COMPLETA =====
namespace CleanArchitectureAdvanced
{
    // ===== DOMAIN LAYER (CENTRO) =====
    namespace Domain.Entities
    {
        public abstract class Entity<TId> where TId : IEquatable<TId>
        {
            public TId Id { get; protected set; }
            public DateTime CreatedAt { get; protected set; }
            public DateTime? UpdatedAt { get; protected set; }
            
            protected Entity(TId id)
            {
                Id = id;
                CreatedAt = DateTime.UtcNow;
            }
            
            protected void MarkAsUpdated()
            {
                UpdatedAt = DateTime.UtcNow;
            }
            
            public override bool Equals(object obj)
            {
                if (obj is Entity<TId> other)
                    return Id.Equals(other.Id);
                return false;
            }
            
            public override int GetHashCode() => Id.GetHashCode();
            
            public static bool operator ==(Entity<TId> left, Entity<TId> right)
                => left?.Equals(right) ?? right is null;
            
            public static bool operator !=(Entity<TId> left, Entity<TId> right)
                => !(left == right);
        }
        
        public abstract class AggregateRoot<TId> : Entity<TId> where TId : IEquatable<TId>
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
        
        public class User : AggregateRoot<int>
        {
            public string Username { get; private set; }
            public string Email { get; private set; }
            public UserStatus Status { get; private set; }
            public List<Role> Roles { get; private set; }
            
            private User() : base(0) { }
            
            public static User Create(string username, string email)
            {
                if (string.IsNullOrEmpty(username))
                    throw new DomainException("Username no puede estar vacío");
                
                if (string.IsNullOrEmpty(email))
                    throw new DomainException("Email no puede estar vacío");
                
                var user = new User
                {
                    Username = username,
                    Email = email,
                    Status = UserStatus.Active,
                    Roles = new List<Role> { Role.User }
                };
                
                user.AddDomainEvent(new UserCreatedDomainEvent(user.Id, username, email));
                return user;
            }
            
            public void UpdateEmail(string newEmail)
            {
                if (string.IsNullOrEmpty(newEmail))
                    throw new DomainException("Email no puede estar vacío");
                
                Email = newEmail;
                MarkAsUpdated();
                AddDomainEvent(new UserEmailUpdatedDomainEvent(Id, newEmail));
            }
            
            public void AddRole(Role role)
            {
                if (!Roles.Contains(role))
                {
                    Roles.Add(role);
                    MarkAsUpdated();
                    AddDomainEvent(new UserRoleAddedDomainEvent(Id, role));
                }
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
            public Money TotalAmount { get; private set; }
            
            private Order() : base(0) { }
            
            public static Order Create(int userId, List<OrderItem> items)
            {
                if (items == null || !items.Any())
                    throw new DomainException("La orden debe tener al menos un item");
                
                var order = new Order
                {
                    UserId = userId,
                    Items = items.ToList(),
                    Status = OrderStatus.Created,
                    TotalAmount = CalculateTotal(items)
                };
                
                order.AddDomainEvent(new OrderCreatedDomainEvent(order.Id, userId, order.TotalAmount));
                return order;
            }
            
            public void Confirm()
            {
                if (Status != OrderStatus.Created)
                    throw new DomainException("Solo se pueden confirmar órdenes creadas");
                
                Status = OrderStatus.Confirmed;
                MarkAsUpdated();
                AddDomainEvent(new OrderConfirmedDomainEvent(Id));
            }
            
            public void Cancel()
            {
                if (Status == OrderStatus.Shipped)
                    throw new DomainException("No se pueden cancelar órdenes enviadas");
                
                Status = OrderStatus.Cancelled;
                MarkAsUpdated();
                AddDomainEvent(new OrderCancelledDomainEvent(Id));
            }
            
            private static Money CalculateTotal(List<OrderItem> items)
            {
                var total = items.Sum(item => item.TotalPrice);
                return Money.Create(total);
            }
        }
        
        public class OrderItem : Entity<int>
        {
            public int ProductId { get; private set; }
            public int Quantity { get; private set; }
            public Money UnitPrice { get; private set; }
            public Money TotalPrice => UnitPrice.Multiply(Quantity);
            
            private OrderItem() : base(0) { }
            
            public static OrderItem Create(int productId, int quantity, Money unitPrice)
            {
                if (quantity <= 0)
                    throw new DomainException("La cantidad debe ser mayor a 0");
                
                if (unitPrice.Amount < 0)
                    throw new DomainException("El precio unitario no puede ser negativo");
                
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
                => EqualOperator(left, right);
            
            public static bool operator !=(ValueObject<T> left, ValueObject<T> right)
                => NotEqualOperator(left, right);
            
            protected static bool EqualOperator(ValueObject<T> left, ValueObject<T> right)
            {
                if (left is null ^ right is null)
                    return false;
                
                return left is null || left.Equals(right);
            }
            
            protected static bool NotEqualOperator(ValueObject<T> left, ValueObject<T> right)
                => !EqualOperator(left, right);
        }
        
        public class Money : ValueObject<Money>
        {
            public decimal Amount { get; private set; }
            public string Currency { get; private set; }
            
            private Money() { }
            
            public static Money Create(decimal amount, string currency = "USD")
            {
                if (amount < 0)
                    throw new DomainException("El monto no puede ser negativo");
                
                return new Money { Amount = amount, Currency = currency.ToUpper() };
            }
            
            public Money Add(Money other)
            {
                if (Currency != other.Currency)
                    throw new DomainException("No se pueden sumar monedas diferentes");
                
                return Create(Amount + other.Amount, Currency);
            }
            
            public Money Subtract(Money other)
            {
                if (Currency != other.Currency)
                    throw new DomainException("No se pueden restar monedas diferentes");
                
                return Create(Amount - other.Amount, Currency);
            }
            
            public Money Multiply(int factor)
            {
                if (factor < 0)
                    throw new DomainException("El factor no puede ser negativo");
                
                return Create(Amount * factor, Currency);
            }
            
            protected override IEnumerable<object> GetEqualityComponents()
            {
                yield return Amount;
                yield return Currency;
            }
            
            public static implicit operator decimal(Money money) => money.Amount;
        }
        
        public class Email : ValueObject<Email>
        {
            public string Value { get; private set; }
            
            private Email() { }
            
            public static Email Create(string email)
            {
                if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                    throw new DomainException("Email inválido");
                
                return new Email { Value = email.ToLower() };
            }
            
            protected override IEnumerable<object> GetEqualityComponents()
            {
                yield return Value;
            }
            
            public static implicit operator string(Email email) => email.Value;
        }
    }
    
    // ===== DOMAIN EVENTS =====
    namespace Domain.Events
    {
        public interface IDomainEvent
        {
            Guid Id { get; }
            DateTime OccurredOn { get; }
            string AggregateId { get; }
        }
        
        public abstract class DomainEvent : IDomainEvent
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
            public Money TotalAmount { get; }
            
            public OrderCreatedDomainEvent(int orderId, int userId, Money totalAmount) 
                : base(orderId.ToString())
            {
                OrderId = orderId;
                UserId = userId;
                TotalAmount = totalAmount;
            }
        }
    }
    
    // ===== DOMAIN EXCEPTIONS =====
    namespace Domain.Exceptions
    {
        public class DomainException : Exception
        {
            public DomainException(string message) : base(message) { }
        }
        
        public class BusinessRuleViolationException : DomainException
        {
            public BusinessRuleViolationException(string message) : base(message) { }
        }
    }
    
    // ===== APPLICATION LAYER =====
    namespace Application.Interfaces
    {
        public interface IUserService
        {
            Task<UserDto> CreateUserAsync(CreateUserCommand command);
            Task<UserDto> UpdateUserAsync(UpdateUserCommand command);
            Task<bool> DeleteUserAsync(int userId);
            Task<UserDto> GetUserAsync(int userId);
            Task<IEnumerable<UserDto>> GetAllUsersAsync();
        }
        
        public interface IOrderService
        {
            Task<OrderDto> CreateOrderAsync(CreateOrderCommand command);
            Task<OrderDto> UpdateOrderAsync(UpdateOrderCommand command);
            Task<bool> CancelOrderAsync(int orderId);
            Task<OrderDto> GetOrderAsync(int orderId);
            Task<IEnumerable<OrderDto>> GetUserOrdersAsync(int userId);
        }
    }
    
    namespace Application.UseCases
    {
        public class CreateUserUseCase : ICreateUserUseCase
        {
            private readonly IUserRepository _userRepository;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IEventDispatcher _eventDispatcher;
            private readonly ILogger<CreateUserUseCase> _logger;
            
            public CreateUserUseCase(
                IUserRepository userRepository,
                IUnitOfWork unitOfWork,
                IEventDispatcher eventDispatcher,
                ILogger<CreateUserUseCase> logger)
            {
                _userRepository = userRepository;
                _unitOfWork = unitOfWork;
                _eventDispatcher = eventDispatcher;
                _logger = logger;
            }
            
            public async Task<Result<UserDto>> ExecuteAsync(CreateUserCommand command)
            {
                try
                {
                    // Validar comando
                    var validationResult = await ValidateCommandAsync(command);
                    if (!validationResult.IsSuccess)
                        return Result<UserDto>.Failure(validationResult.Errors);
                    
                    // Crear usuario
                    var user = User.Create(command.Username, command.Email);
                    
                    // Guardar en repositorio
                    await _userRepository.AddAsync(user);
                    
                    // Commit transacción
                    await _unitOfWork.CommitAsync();
                    
                    // Dispatch eventos de dominio
                    await DispatchDomainEventsAsync(user);
                    
                    _logger.LogInformation("Usuario creado exitosamente: {UserId}", user.Id);
                    
                    return Result<UserDto>.Success(MapToDto(user));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear usuario: {Username}", command.Username);
                    return Result<UserDto>.Failure("Error interno del sistema");
                }
            }
            
            private async Task<Result> ValidateCommandAsync(CreateUserCommand command)
            {
                var errors = new List<string>();
                
                if (string.IsNullOrEmpty(command.Username))
                    errors.Add("Username es requerido");
                
                if (string.IsNullOrEmpty(command.Email))
                    errors.Add("Email es requerido");
                
                if (await _userRepository.ExistsByUsernameAsync(command.Username))
                    errors.Add("Username ya existe");
                
                if (await _userRepository.ExistsByEmailAsync(command.Email))
                    errors.Add("Email ya existe");
                
                return errors.Any() ? Result.Failure(errors) : Result.Success();
            }
            
            private async Task DispatchDomainEventsAsync(User user)
            {
                foreach (var domainEvent in user.DomainEvents)
                {
                    await _eventDispatcher.DispatchAsync(domainEvent);
                }
                user.ClearDomainEvents();
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
    }
    
    // ===== INFRASTRUCTURE LAYER =====
    namespace Infrastructure.Persistence
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
            
            public async Task<User> AddAsync(User user)
            {
                var entity = MapToEntity(user);
                _context.Users.Add(entity);
                return user;
            }
            
            public async Task<User> GetByIdAsync(int id)
            {
                var entity = await _context.Users
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Id == id);
                
                return entity != null ? MapToDomain(entity) : null;
            }
            
            public async Task<bool> ExistsByUsernameAsync(string username)
            {
                return await _context.Users.AnyAsync(u => u.Username == username);
            }
            
            public async Task<bool> ExistsByEmailAsync(string email)
            {
                return await _context.Users.AnyAsync(u => u.Email == email);
            }
            
            private UserEntity MapToEntity(User user)
            {
                return new UserEntity
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Status = user.Status,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };
            }
            
            private User MapToDomain(UserEntity entity)
            {
                // Mapeo de entidad a dominio
                return User.Create(entity.Username, entity.Email);
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
            private readonly ICreateUserUseCase _createUserUseCase;
            private readonly IGetUserUseCase _getUserUseCase;
            private readonly ILogger<UsersController> _logger;
            
            public UsersController(
                ICreateUserUseCase createUserUseCase,
                IGetUserUseCase getUserUseCase,
                ILogger<UsersController> logger)
            {
                _createUserUseCase = createUserUseCase;
                _getUserUseCase = getUserUseCase;
                _logger = logger;
            }
            
            [HttpPost]
            public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserCommand command)
            {
                try
                {
                    var result = await _createUserUseCase.ExecuteAsync(command);
                    
                    if (!result.IsSuccess)
                        return BadRequest(result.Errors);
                    
                    return CreatedAtAction(nameof(GetUser), new { id = result.Value.Id }, result.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear usuario");
                    return StatusCode(500, "Error interno del servidor");
                }
            }
            
            [HttpGet("{id}")]
            public async Task<ActionResult<UserDto>> GetUser(int id)
            {
                var result = await _getUserUseCase.ExecuteAsync(id);
                
                if (!result.IsSuccess)
                    return NotFound();
                
                return Ok(result.Value);
            }
        }
    }
    
    // ===== DEPENDENCY INJECTION =====
    namespace Infrastructure.DependencyInjection
    {
        public static class ServiceCollectionExtensions
        {
            public static IServiceCollection AddCleanArchitecture(this IServiceCollection services)
            {
                // Application Layer
                services.AddScoped<ICreateUserUseCase, CreateUserUseCase>();
                services.AddScoped<IGetUserUseCase, GetUserUseCase>();
                
                // Infrastructure Layer
                services.AddScoped<IUserRepository, UserRepository>();
                services.AddScoped<IUnitOfWork, UnitOfWork>();
                services.AddScoped<IEventDispatcher, EventDispatcher>();
                
                // Presentation Layer
                services.AddScoped<IValidator<CreateUserCommand>, CreateUserCommandValidator>();
                
                return services;
            }
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
    
    public enum Role
    {
        User,
        Editor,
        Admin
    }
}

// Uso de Arquitectura Limpia Avanzada
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Arquitectura Limpia Avanzada ===\n");
        
        Console.WriteLine("Los principios implementados incluyen:");
        Console.WriteLine("1. Separación clara de capas (Domain, Application, Infrastructure, Presentation)");
        Console.WriteLine("2. Inversión de dependencias (Domain no depende de Infrastructure)");
        Console.WriteLine("3. Entidades y Value Objects bien definidos");
        Console.WriteLine("4. Casos de uso específicos y enfocados");
        Console.WriteLine("5. Repositorios abstractos y concretos");
        Console.WriteLine("6. Manejo de eventos de dominio");
        Console.WriteLine("7. Inyección de dependencias avanzada");
        
        Console.WriteLine("\nBeneficios de esta arquitectura:");
        Console.WriteLine("- Código mantenible y testeable");
        Console.WriteLine("- Separación clara de responsabilidades");
        Console.WriteLine("- Fácil de extender y modificar");
        Console.WriteLine("- Independiente de frameworks externos");
        Console.WriteLine("- Escalable para aplicaciones empresariales");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Implementar Clean Architecture
Crea una nueva entidad de dominio siguiendo los principios de Clean Architecture.

### Ejercicio 2: Patrones de Inyección
Implementa diferentes patrones de inyección de dependencias para el mismo servicio.

### Ejercicio 3: Eventos de Dominio
Crea un sistema de eventos de dominio para una entidad compleja.

## 🔍 Puntos Clave

1. **Separación de capas** clara y bien definida
2. **Inversión de dependencias** para mantener el dominio independiente
3. **Entidades y Value Objects** como base del dominio
4. **Casos de uso** específicos y enfocados
5. **Eventos de dominio** para comunicación entre agregados

## 📚 Recursos Adicionales

- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [Domain-Driven Design](https://domainlanguage.com/ddd/)

---

**🎯 ¡Has completado la Clase 1! Ahora comprendes Arquitectura Limpia Avanzada**

**📚 [Siguiente: Clase 2 - Event-Driven Architecture](clase_2_event_driven_architecture.md)**
