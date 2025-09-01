# 🚀 Clase 7: Refactoring y Clean Code

## 📋 Información de la Clase

- **Módulo**: Mid Level 1 - Programación Avanzada y Patrones de Diseño
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 6 (Logging y Monitoreo)

## 🎯 Objetivos de Aprendizaje

- Aplicar técnicas de refactoring para mejorar la calidad del código
- Implementar principios de Clean Code en C#
- Identificar y eliminar code smells
- Refactorizar código legacy hacia Clean Architecture
- Implementar técnicas de refactoring automatizado

---

## 📚 Navegación del Módulo 4

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_patrones_diseno_intermedios.md) | Patrones de Diseño Intermedios | |
| [Clase 2](clase_2_programacion_asincrona_avanzada.md) | Programación Asíncrona Avanzada | |
| [Clase 3](clase_3_programacion_paralela.md) | Programación Paralela y TPL | |
| [Clase 4](clase_4_clean_architecture.md) | Clean Architecture | |
| [Clase 5](clase_5_dependency_injection.md) | Dependency Injection Avanzada | |
| [Clase 6](clase_6_logging_monitoreo.md) | Logging y Monitoreo | ← Anterior |
| **Clase 7** | **Refactoring y Clean Code** | ← Estás aquí |
| [Clase 8](clase_8_testing_integracion.md) | Testing de Integración | Siguiente → |
| [Clase 9](clase_9_testing_comportamiento.md) | Testing de Comportamiento (BDD) | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de E-commerce | |

**← [Volver al README del Módulo 4](../midLevel_1/README.md)**

---

## 📚 Contenido Teórico

### 1. Principios de Clean Code

Clean Code se basa en principios que hacen el código legible, mantenible y extensible.

```csharp
// Código antes del refactoring (Code Smells)
public class UserManager
{
    public void ProcessUserData(string userData, string operation, bool validate, bool log, bool cache)
    {
        if (userData == null || userData == "")
        {
            throw new Exception("Invalid user data");
        }
        
        var parts = userData.Split(',');
        if (parts.Length != 4)
        {
            throw new Exception("Invalid format");
        }
        
        var user = new User();
        user.Id = int.Parse(parts[0]);
        user.Name = parts[1];
        user.Email = parts[2];
        user.Age = int.Parse(parts[3]);
        
        if (validate)
        {
            if (user.Age < 0 || user.Age > 150)
            {
                throw new Exception("Invalid age");
            }
            if (!user.Email.Contains("@"))
            {
                throw new Exception("Invalid email");
            }
        }
        
        if (log)
        {
            Console.WriteLine($"Processing user: {user.Id}");
        }
        
        // Lógica de procesamiento compleja
        if (operation == "create")
        {
            // Crear usuario
            Console.WriteLine("User created");
        }
        else if (operation == "update")
        {
            // Actualizar usuario
            Console.WriteLine("User updated");
        }
        else if (operation == "delete")
        {
            // Eliminar usuario
            Console.WriteLine("User deleted");
        }
        
        if (cache)
        {
            // Guardar en cache
            Console.WriteLine("User cached");
        }
    }
}

// Código después del refactoring (Clean Code)
public class UserManager
{
    private readonly IUserValidator _validator;
    private readonly IUserRepository _repository;
    private readonly ILogger _logger;
    private readonly ICacheService _cacheService;
    
    public UserManager(
        IUserValidator validator,
        IUserRepository repository,
        ILogger logger,
        ICacheService cacheService)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }
    
    public async Task ProcessUserAsync(UserOperationRequest request)
    {
        Guard.Against.Null(request, nameof(request));
        
        var user = ParseUserFromString(request.UserData);
        await ValidateUserAsync(user);
        await ProcessUserOperationAsync(user, request.Operation);
        await CacheUserIfRequiredAsync(user, request.EnableCaching);
    }
    
    private User ParseUserFromString(string userData)
    {
        Guard.Against.NullOrEmpty(userData, nameof(userData));
        
        var parts = userData.Split(',');
        if (parts.Length != UserDataParts.ExpectedCount)
        {
            throw new UserDataFormatException($"Expected {UserDataParts.ExpectedCount} parts, got {parts.Length}");
        }
        
        return new User
        {
            Id = int.Parse(parts[UserDataParts.IdIndex]),
            Name = parts[UserDataParts.NameIndex],
            Email = parts[UserDataParts.EmailIndex],
            Age = int.Parse(parts[UserDataParts.AgeIndex])
        };
    }
    
    private async Task ValidateUserAsync(User user)
    {
        var validationResult = await _validator.ValidateAsync(user);
        if (!validationResult.IsValid)
        {
            throw new UserValidationException(validationResult.Errors);
        }
    }
    
    private async Task ProcessUserOperationAsync(User user, UserOperation operation)
    {
        _logger.LogInformation("Processing user operation: {Operation} for user: {UserId}", operation, user.Id);
        
        var result = operation switch
        {
            UserOperation.Create => await _repository.CreateAsync(user),
            UserOperation.Update => await _repository.UpdateAsync(user),
            UserOperation.Delete => await _repository.DeleteAsync(user.Id),
            _ => throw new UnsupportedOperationException(operation)
        };
        
        _logger.LogInformation("User operation completed: {Operation} for user: {UserId}", operation, user.Id);
    }
    
    private async Task CacheUserIfRequiredAsync(User user, bool enableCaching)
    {
        if (enableCaching)
        {
            await _cacheService.SetAsync($"user_{user.Id}", user);
            _logger.LogDebug("User cached: {UserId}", user.Id);
        }
    }
}

// Clases de soporte para Clean Code
public class UserOperationRequest
{
    public string UserData { get; set; }
    public UserOperation Operation { get; set; }
    public bool EnableValidation { get; set; } = true;
    public bool EnableLogging { get; set; } = true;
    public bool EnableCaching { get; set; } = false;
}

public enum UserOperation
{
    Create,
    Update,
    Delete
}

public static class UserDataParts
{
    public const int ExpectedCount = 4;
    public const int IdIndex = 0;
    public const int NameIndex = 1;
    public const int EmailIndex = 2;
    public const int AgeIndex = 3;
}

public class UserDataFormatException : Exception
{
    public UserDataFormatException(string message) : base(message) { }
}

public class UserValidationException : Exception
{
    public IEnumerable<string> Errors { get; }
    
    public UserValidationException(IEnumerable<string> errors)
    {
        Errors = errors;
    }
}

public class UnsupportedOperationException : Exception
{
    public UnsupportedOperationException(UserOperation operation)
        : base($"Operation {operation} is not supported") { }
}
```

### 2. Técnicas de Refactoring

Las técnicas de refactoring mejoran la estructura del código sin cambiar su comportamiento.

```csharp
// Refactoring: Extract Method
public class OrderProcessor
{
    // Antes: Método largo con múltiples responsabilidades
    public async Task<OrderResult> ProcessOrderAsync(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));
        
        if (order.Items == null || !order.Items.Any())
            throw new InvalidOperationException("Order must have items");
        
        var total = 0m;
        foreach (var item in order.Items)
        {
            if (item.Quantity <= 0)
                throw new InvalidOperationException($"Invalid quantity for item {item.Id}");
            
            if (item.UnitPrice <= 0)
                throw new InvalidOperationException($"Invalid price for item {item.Id}");
            
            total += item.Quantity * item.UnitPrice;
        }
        
        if (order.Customer == null)
            throw new InvalidOperationException("Order must have a customer");
        
        if (string.IsNullOrEmpty(order.Customer.Email))
            throw new InvalidOperationException("Customer must have an email");
        
        if (total > 10000m)
        {
            // Aplicar descuento para órdenes grandes
            total = total * 0.95m;
        }
        
        order.TotalAmount = total;
        order.Status = OrderStatus.Processing;
        order.ProcessedAt = DateTime.UtcNow;
        
        return new OrderResult
        {
            OrderId = order.Id,
            TotalAmount = total,
            Status = order.Status,
            ProcessedAt = order.ProcessedAt
        };
    }
    
    // Después: Métodos extraídos con responsabilidades claras
    public async Task<OrderResult> ProcessOrderAsync(Order order)
    {
        ValidateOrder(order);
        var total = CalculateOrderTotal(order);
        var finalTotal = ApplyDiscounts(total);
        
        var result = await UpdateOrderStatusAsync(order, finalTotal);
        return CreateOrderResult(order, result);
    }
    
    private void ValidateOrder(Order order)
    {
        Guard.Against.Null(order, nameof(order));
        Guard.Against.NullOrEmpty(order.Items, nameof(order.Items));
        Guard.Against.Null(order.Customer, nameof(order.Customer));
        Guard.Against.NullOrEmpty(order.Customer.Email, nameof(order.Customer.Email));
        
        ValidateOrderItems(order.Items);
    }
    
    private void ValidateOrderItems(IEnumerable<OrderItem> items)
    {
        foreach (var item in items)
        {
            Guard.Against.NegativeOrZero(item.Quantity, nameof(item.Quantity));
            Guard.Against.NegativeOrZero(item.UnitPrice, nameof(item.UnitPrice));
        }
    }
    
    private decimal CalculateOrderTotal(Order order)
    {
        return order.Items.Sum(item => item.Quantity * item.UnitPrice);
    }
    
    private decimal ApplyDiscounts(decimal total)
    {
        const decimal largeOrderThreshold = 10000m;
        const decimal largeOrderDiscount = 0.95m;
        
        return total > largeOrderThreshold ? total * largeOrderDiscount : total;
    }
    
    private async Task<Order> UpdateOrderStatusAsync(Order order, decimal finalTotal)
    {
        order.TotalAmount = finalTotal;
        order.Status = OrderStatus.Processing;
        order.ProcessedAt = DateTime.UtcNow;
        
        // Aquí se guardaría en la base de datos
        await Task.CompletedTask;
        
        return order;
    }
    
    private OrderResult CreateOrderResult(Order order, Order updatedOrder)
    {
        return new OrderResult
        {
            OrderId = updatedOrder.Id,
            TotalAmount = updatedOrder.TotalAmount,
            Status = updatedOrder.Status,
            ProcessedAt = updatedOrder.ProcessedAt
        };
    }
}

// Refactoring: Replace Conditional with Polymorphism
public abstract class DiscountStrategy
{
    public abstract decimal ApplyDiscount(decimal amount);
    public abstract bool IsApplicable(Order order);
}

public class NoDiscountStrategy : DiscountStrategy
{
    public override decimal ApplyDiscount(decimal amount) => amount;
    public override bool IsApplicable(Order order) => true;
}

public class LargeOrderDiscountStrategy : DiscountStrategy
{
    private const decimal Threshold = 10000m;
    private const decimal DiscountRate = 0.95m;
    
    public override decimal ApplyDiscount(decimal amount) => amount * DiscountRate;
    public override bool IsApplicable(Order order) => order.TotalAmount > Threshold;
}

public class LoyalCustomerDiscountStrategy : DiscountStrategy
{
    private const decimal DiscountRate = 0.90m;
    
    public override decimal ApplyDiscount(decimal amount) => amount * DiscountRate;
    public override bool IsApplicable(Order order) => order.Customer.IsLoyal;
}

public class DiscountCalculator
{
    private readonly IEnumerable<DiscountStrategy> _strategies;
    
    public DiscountCalculator(IEnumerable<DiscountStrategy> strategies)
    {
        _strategies = strategies;
    }
    
    public decimal CalculateFinalAmount(Order order)
    {
        var applicableStrategy = _strategies.FirstOrDefault(s => s.IsApplicable(order));
        return applicableStrategy?.ApplyDiscount(order.TotalAmount) ?? order.TotalAmount;
    }
}
```

### 3. Eliminación de Code Smells

Los code smells son indicadores de problemas en el código que deben ser eliminados.

```csharp
// Code Smell: Long Parameter List
// Antes: Muchos parámetros
public class UserService
{
    public async Task<User> CreateUserAsync(
        string firstName, string lastName, string email, string phone,
        DateTime dateOfBirth, string address, string city, string state,
        string zipCode, string country, bool isActive, string password,
        string confirmPassword, bool sendWelcomeEmail, bool createProfile)
    {
        // Implementación compleja
        return new User();
    }
}

// Después: Usar objetos de parámetros
public class CreateUserRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Address Address { get; set; }
    public UserPreferences Preferences { get; set; }
    public SecurityInfo Security { get; set; }
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public string Country { get; set; }
}

public class UserPreferences
{
    public bool IsActive { get; set; }
    public bool SendWelcomeEmail { get; set; }
    public bool CreateProfile { get; set; }
}

public class SecurityInfo
{
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
}

public class UserService
{
    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        // Implementación más limpia
        return new User();
    }
}

// Code Smell: Feature Envy
// Antes: Método que usa más datos de otro objeto que del propio
public class OrderProcessor
{
    public decimal CalculateOrderTotal(Order order)
    {
        var total = 0m;
        foreach (var item in order.Items)
        {
            total += item.Quantity * item.UnitPrice;
        }
        
        if (order.Customer.IsVip)
        {
            total *= 0.90m; // 10% descuento VIP
        }
        
        return total;
    }
}

// Después: Mover lógica al objeto apropiado
public class Order
{
    public decimal CalculateTotal()
    {
        var subtotal = Items.Sum(item => item.CalculateSubtotal());
        return ApplyCustomerDiscounts(subtotal);
    }
    
    private decimal ApplyCustomerDiscounts(decimal subtotal)
    {
        return Customer.IsVip ? subtotal * 0.90m : subtotal;
    }
}

public class OrderItem
{
    public decimal CalculateSubtotal()
    {
        return Quantity * UnitPrice;
    }
}

public class OrderProcessor
{
    public decimal CalculateOrderTotal(Order order)
    {
        return order.CalculateTotal();
    }
}

// Code Smell: Data Clumps
// Antes: Datos que siempre aparecen juntos
public class UserService
{
    public void UpdateUserAddress(int userId, string street, string city, string state, string zipCode, string country)
    {
        // Implementación
    }
    
    public void ValidateAddress(string street, string city, string state, string zipCode, string country)
    {
        // Validación
    }
    
    public string FormatAddress(string street, string city, string state, string zipCode, string country)
    {
        return $"{street}, {city}, {state} {zipCode}, {country}";
    }
}

// Después: Extraer a clase
public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public string Country { get; set; }
    
    public string Format()
    {
        return $"{Street}, {City}, {State} {ZipCode}, {Country}";
    }
    
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Street) &&
               !string.IsNullOrEmpty(City) &&
               !string.IsNullOrEmpty(State) &&
               !string.IsNullOrEmpty(ZipCode) &&
               !string.IsNullOrEmpty(Country);
    }
}

public class UserService
{
    public void UpdateUserAddress(int userId, Address address)
    {
        if (!address.IsValid())
            throw new ArgumentException("Invalid address");
        
        // Implementación
    }
    
    public void ValidateAddress(Address address)
    {
        if (!address.IsValid())
            throw new ArgumentException("Invalid address");
    }
    
    public string FormatAddress(Address address)
    {
        return address.Format();
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Refactorizar Código Legacy
Toma un método largo y complejo y aplícale técnicas de refactoring para hacerlo más limpio y mantenible.

### Ejercicio 2: Eliminar Code Smells
Identifica y elimina code smells en un sistema existente, aplicando principios de Clean Code.

### Ejercicio 3: Implementar Strategy Pattern
Refactoriza código que use condicionales complejos implementando el patrón Strategy.

## 🔍 Puntos Clave

1. **Clean Code** es legible, mantenible y extensible
2. **Refactoring** mejora la estructura sin cambiar el comportamiento
3. **Code Smells** indican problemas que deben ser eliminados
4. **Extract Method** divide métodos largos en métodos más pequeños
5. **Replace Conditional with Polymorphism** elimina condicionales complejos

## 📚 Recursos Adicionales

- [Clean Code - Robert C. Martin](https://www.amazon.com/Clean-Code-Handbook-Software-Craftsmanship/dp/0132350884)
- [Refactoring - Martin Fowler](https://refactoring.com/)
- [Code Smells - Refactoring Guru](https://refactoring.guru/refactoring/smells)

---

**🎯 ¡Has completado la Clase 7! Ahora dominas las técnicas de refactoring y Clean Code**

**📚 [Siguiente: Clase 8 - Testing de Integración](clase_8_testing_integracion.md)**
