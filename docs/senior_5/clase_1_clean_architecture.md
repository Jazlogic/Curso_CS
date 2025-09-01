# Clase 1: Clean Architecture

## Navegación
- [← Volver al README del módulo](README.md)
- [← Volver al módulo anterior (senior_4)](../senior_4/README.md)
- [→ Ir al siguiente módulo (senior_6)](../senior_6/README.md)
- [→ Siguiente clase: CQRS](clase_2_cqrs.md)

## Objetivos de Aprendizaje
- Comprender los principios fundamentales de Clean Architecture
- Aplicar la separación de responsabilidades en capas
- Implementar entidades de dominio y reglas de negocio
- Crear interfaces y abstracciones para inversión de dependencias

## ¿Qué es Clean Architecture?

Clean Architecture es un patrón de arquitectura de software que enfatiza la **separación de responsabilidades** y la **independencia de frameworks, bases de datos y tecnologías externas**. Fue popularizado por Robert C. Martin (Uncle Bob) y se basa en el principio de que las reglas de negocio deben ser el núcleo de la aplicación.

### Principios Fundamentales

```csharp
// 1. Independencia de Frameworks
// El dominio no debe depender de ASP.NET Core, Entity Framework, etc.
public class Order
{
    // Solo lógica de negocio, sin dependencias externas
    public void Confirm() { /* ... */ }
    public void Cancel() { /* ... */ }
}

// 2. Testabilidad
// Las reglas de negocio deben ser fáciles de probar
[Test]
public void ConfirmOrder_ShouldChangeStatusToConfirmed()
{
    var order = Order.Create(customerId, items);
    order.Confirm();
    Assert.AreEqual(OrderStatus.Confirmed, order.Status);
}

// 3. Independencia de UI
// El dominio no debe saber nada sobre la interfaz de usuario
public interface IOrderRepository
{
    // Solo operaciones de dominio, sin detalles de UI
    Task<Order> GetByIdAsync(Guid id);
}

// 4. Independencia de Base de Datos
// El dominio no debe depender de SQL Server, PostgreSQL, etc.
public interface IOrderRepository
{
    // Solo operaciones abstractas, sin SQL
    Task<Order> GetByIdAsync(Guid id);
}
```

## Capas de Clean Architecture

### 1. Domain Layer (Núcleo)

La capa de dominio contiene las **entidades de negocio**, **reglas de negocio** y **interfaces de repositorio**. Es la capa más interna y no depende de ninguna otra capa.

```csharp
// Domain/Entities/Order.cs
public class Order : Entity
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public List<OrderItem> Items { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public decimal Total => Items.Sum(item => item.Total);
    
    // Constructor privado para EF Core
    private Order() 
    { 
        Items = new List<OrderItem>();
    }
    
    // Factory method para crear órdenes
    public static Order Create(Guid customerId, List<OrderItem> items)
    {
        // Validaciones de negocio
        if (items == null || !items.Any())
            throw new DomainException("Order must have at least one item");
            
        if (customerId == Guid.Empty)
            throw new DomainException("Customer ID is required");
            
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Items = items,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        
        // Agregar evento de dominio
        order.AddDomainEvent(new OrderCreatedEvent(order.Id, customerId, order.Total));
        
        return order;
    }
    
    // Métodos de negocio
    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Only pending orders can be confirmed");
            
        var oldStatus = Status;
        Status = OrderStatus.Confirmed;
        
        // Agregar evento de dominio
        AddDomainEvent(new OrderStatusChangedEvent(Id, oldStatus, Status));
    }
    
    public void Cancel()
    {
        if (Status == OrderStatus.Delivered)
            throw new DomainException("Delivered orders cannot be cancelled");
            
        var oldStatus = Status;
        Status = OrderStatus.Cancelled;
        
        // Agregar evento de dominio
        AddDomainEvent(new OrderStatusChangedEvent(Id, oldStatus, Status));
    }
    
    public void AddItem(OrderItem item)
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Cannot add items to non-pending orders");
            
        if (item == null)
            throw new DomainException("Item cannot be null");
            
        Items.Add(item);
        
        // Agregar evento de dominio
        AddDomainEvent(new OrderItemAddedEvent(Id, item));
    }
    
    public void RemoveItem(Guid productId)
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Cannot remove items from non-pending orders");
            
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new DomainException("Item not found in order");
            
        Items.Remove(item);
        
        // Agregar evento de dominio
        AddDomainEvent(new OrderItemRemovedEvent(Id, productId));
    }
}

// Domain/Entities/OrderItem.cs
public class OrderItem : Entity
{
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Total => Quantity * UnitPrice;
    
    private OrderItem() { }
    
    public static OrderItem Create(Guid productId, int quantity, decimal unitPrice)
    {
        // Validaciones de negocio
        if (productId == Guid.Empty)
            throw new DomainException("Product ID is required");
            
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero");
            
        if (unitPrice <= 0)
            throw new DomainException("Unit price must be greater than zero");
            
        return new OrderItem
        {
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }
    
    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new DomainException("Quantity must be greater than zero");
            
        Quantity = newQuantity;
    }
    
    public void UpdateUnitPrice(decimal newUnitPrice)
    {
        if (newUnitPrice <= 0)
            throw new DomainException("Unit price must be greater than zero");
            
        UnitPrice = newUnitPrice;
    }
}

// Domain/Entities/Customer.cs
public class Customer : Entity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public CustomerStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastOrderDate { get; private set; }
    public int TotalOrders { get; private set; }
    public decimal TotalSpent { get; private set; }
    
    private Customer() { }
    
    public static Customer Create(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required");
            
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required");
            
        if (!IsValidEmail(email))
            throw new DomainException("Invalid email format");
            
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Email = email.Trim().ToLower(),
            Status = CustomerStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        
        // Agregar evento de dominio
        customer.AddDomainEvent(new CustomerCreatedEvent(customer.Id, customer.Name, customer.Email));
        
        return customer;
    }
    
    public void UpdateProfile(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required");
            
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required");
            
        if (!IsValidEmail(email))
            throw new DomainException("Invalid email format");
            
        var oldName = Name;
        var oldEmail = Email;
        
        Name = name.Trim();
        Email = email.Trim().ToLower();
        
        // Agregar evento de dominio
        AddDomainEvent(new CustomerProfileUpdatedEvent(Id, oldName, oldEmail, Name, Email));
    }
    
    public void Deactivate()
    {
        if (Status == CustomerStatus.Inactive)
            throw new DomainException("Customer is already inactive");
            
        Status = CustomerStatus.Inactive;
        
        // Agregar evento de dominio
        AddDomainEvent(new CustomerDeactivatedEvent(Id));
    }
    
    public void RecordOrder(decimal orderTotal)
    {
        TotalOrders++;
        TotalSpent += orderTotal;
        LastOrderDate = DateTime.UtcNow;
        
        // Agregar evento de dominio
        AddDomainEvent(new CustomerOrderRecordedEvent(Id, orderTotal));
    }
    
    private static bool IsValidEmail(string email)
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
}
```

### 2. Value Objects

Los Value Objects son objetos inmutables que representan conceptos del dominio sin identidad propia.

```csharp
// Domain/ValueObjects/Money.cs
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
    
    public Money(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new DomainException("Amount cannot be negative");
            
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency is required");
            
        Amount = amount;
        Currency = currency.ToUpper();
    }
    
    // Operadores matemáticos
    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new DomainException("Cannot add money with different currencies");
            
        return new Money(left.Amount + right.Amount, left.Currency);
    }
    
    public static Money operator -(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new DomainException("Cannot subtract money with different currencies");
            
        var result = left.Amount - right.Amount;
        if (result < 0)
            throw new DomainException("Result cannot be negative");
            
        return new Money(result, left.Currency);
    }
    
    public static Money operator *(Money money, int multiplier)
    {
        if (multiplier <= 0)
            throw new DomainException("Multiplier must be positive");
            
        return new Money(money.Amount * multiplier, money.Currency);
    }
    
    // Métodos de utilidad
    public Money Round(int decimals = 2)
    {
        var roundedAmount = Math.Round(Amount, decimals, MidpointRounding.AwayFromZero);
        return new Money(roundedAmount, Currency);
    }
    
    public override string ToString()
    {
        return $"{Amount:F2} {Currency}";
    }
    
    // Implementación de ValueObject
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}

// Domain/ValueObjects/Email.cs
public class Email : ValueObject
{
    public string Value { get; }
    
    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email cannot be empty");
            
        if (!IsValidEmail(value))
            throw new DomainException("Invalid email format");
            
        Value = value.Trim().ToLower();
    }
    
    private static bool IsValidEmail(string email)
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
    
    public static implicit operator string(Email email) => email.Value;
    
    public static explicit operator Email(string value) => new Email(value);
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}

// Domain/ValueObjects/PhoneNumber.cs
public class PhoneNumber : ValueObject
{
    public string Value { get; }
    public string CountryCode { get; }
    
    public PhoneNumber(string value, string countryCode = "+1")
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Phone number cannot be empty");
            
        if (string.IsNullOrWhiteSpace(countryCode))
            throw new DomainException("Country code is required");
            
        if (!IsValidPhoneNumber(value))
            throw new DomainException("Invalid phone number format");
            
        Value = CleanPhoneNumber(value);
        CountryCode = countryCode;
    }
    
    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        // Validación básica - solo dígitos y algunos caracteres especiales
        var cleaned = CleanPhoneNumber(phoneNumber);
        return cleaned.Length >= 10 && cleaned.Length <= 15;
    }
    
    private static string CleanPhoneNumber(string phoneNumber)
    {
        // Remover todos los caracteres no numéricos
        return new string(phoneNumber.Where(char.IsDigit).ToArray());
    }
    
    public string GetFullNumber()
    {
        return $"{CountryCode}{Value}";
    }
    
    public string GetFormattedNumber()
    {
        if (Value.Length == 10)
        {
            return $"({Value.Substring(0, 3)}) {Value.Substring(3, 3)}-{Value.Substring(6, 4)}";
        }
        
        return Value;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return CountryCode;
    }
}
```

### 3. Entidades Base

Las entidades base proporcionan funcionalidad común para todas las entidades del dominio.

```csharp
// Domain/Common/Entity.cs
public abstract class Entity
{
    private readonly List<DomainEvent> _domainEvents = new();
    
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
            
        _domainEvents.Add(domainEvent);
    }
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
    
    // Método abstracto para validaciones de entidad
    protected abstract void Validate();
}

// Domain/Common/ValueObject.cs
public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();
    
    public override bool Equals(object obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;
            
        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }
    
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x != null ? x.GetHashCode() : 0)
            .Aggregate((x, y) => x ^ y);
    }
    
    public static bool operator ==(ValueObject left, ValueObject right)
    {
        return EqualOperator(left, right);
    }
    
    public static bool operator !=(ValueObject left, ValueObject right)
    {
        return !EqualOperator(left, right);
    }
    
    protected static bool EqualOperator(ValueObject left, ValueObject right)
    {
        if (left is null ^ right is null)
            return false;
        return left is null || left.Equals(right);
    }
}

// Domain/Common/DomainException.cs
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    
    public DomainException(string message, Exception innerException) 
        : base(message, innerException) { }
}

// Domain/Common/Result.cs
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }
    
    private Result(bool isSuccess, T value, string error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }
    
    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, null);
    }
    
    public static Result<T> Failure(string error)
    {
        return new Result<T>(false, default(T), error);
    }
    
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        if (IsSuccess)
            return Result<TNew>.Success(mapper(Value));
        else
            return Result<TNew>.Failure(Error);
    }
    
    public async Task<Result<TNew>> MapAsync<TNew>(Func<T, Task<TNew>> mapper)
    {
        if (IsSuccess)
        {
            var newValue = await mapper(Value);
            return Result<TNew>.Success(newValue);
        }
        else
        {
            return Result<TNew>.Failure(Error);
        }
    }
}
```

### 4. Interfaces de Repositorio

Las interfaces de repositorio definen las operaciones que se pueden realizar con las entidades del dominio.

```csharp
// Domain/Repositories/IOrderRepository.cs
public interface IOrderRepository
{
    Task<Order> GetByIdAsync(Guid id);
    Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId);
    Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status);
    Task<PagedResult<Order>> GetPagedAsync(OrderFilter filter, int page, int pageSize);
    Task<Order> AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<int> GetCountByCustomerAsync(Guid customerId);
}

// Domain/Repositories/ICustomerRepository.cs
public interface ICustomerRepository
{
    Task<Customer> GetByIdAsync(Guid id);
    Task<Customer> GetByEmailAsync(string email);
    Task<IEnumerable<Customer>> GetByStatusAsync(CustomerStatus status);
    Task<PagedResult<Customer>> GetPagedAsync(CustomerFilter filter, int page, int pageSize);
    Task<Customer> AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> EmailExistsAsync(string email);
}

// Domain/Repositories/IProductRepository.cs
public interface IProductRepository
{
    Task<Product> GetByIdAsync(Guid id);
    Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId);
    Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);
    Task<PagedResult<Product>> GetPagedAsync(ProductFilter filter, int page, int pageSize);
    Task<Product> AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<int> GetStockLevelAsync(Guid id);
    Task UpdateStockAsync(Guid id, int newStock);
}
```

## Ejercicios Prácticos

### Ejercicio 1: Crear Entidad de Dominio
Implementa una entidad `Product` siguiendo los principios de Clean Architecture.

### Ejercicio 2: Implementar Value Object
Crea un Value Object `Address` para representar direcciones de clientes.

### Ejercicio 3: Validaciones de Negocio
Implementa validaciones de negocio para la entidad `Order` (mínimo de compra, límite de productos, etc.).

## Resumen

En esta clase hemos aprendido:

1. **Principios de Clean Architecture**: Independencia de frameworks, testabilidad, separación de responsabilidades
2. **Capas de la arquitectura**: Domain, Application, Infrastructure, Presentation
3. **Entidades de dominio**: Con reglas de negocio y validaciones
4. **Value Objects**: Objetos inmutables para conceptos del dominio
5. **Interfaces de repositorio**: Abstracciones para acceso a datos

En la siguiente clase continuaremos con **CQRS (Command Query Responsibility Segregation)** para separar las operaciones de lectura y escritura.

## Recursos Adicionales
- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [Value Objects](https://martinfowler.com/bliki/ValueObject.html)
