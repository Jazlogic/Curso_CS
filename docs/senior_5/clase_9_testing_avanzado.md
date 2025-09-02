# Clase 9: Testing Avanzado

## Navegación
- [← Clase anterior: CQRS Avanzado](clase_8_cqrs_avanzado.md)
- [← Volver al README del módulo](README.md)
- [→ Siguiente clase: Proyecto Final](clase_10_proyecto_final.md)

## Objetivos de Aprendizaje
- Implementar pruebas unitarias avanzadas
- Crear pruebas de integración robustas
- Aplicar TDD y BDD
- Implementar pruebas de rendimiento

## Testing Avanzado en Arquitecturas Limpias

El **Testing Avanzado** es fundamental en arquitecturas limpias para garantizar la calidad del código, facilitar el mantenimiento y permitir refactoring seguro. Cubriremos diferentes tipos de pruebas y estrategias para sistemas complejos.

### Estrategias de Testing

```csharp
// 1. Testing Pyramid
// Unit Tests (70%) - Rápidos, baratos, aislados
// Integration Tests (20%) - Moderados, costos medios
// End-to-End Tests (10%) - Lentos, costosos, realistas

// 2. Testing por Capas
// Domain Layer - Pruebas unitarias puras
// Application Layer - Pruebas unitarias con mocks
// Infrastructure Layer - Pruebas de integración
// API Layer - Pruebas de integración y E2E

// 3. Testing por Tipos
// Unit Tests - Lógica de negocio
// Integration Tests - Persistencia, servicios externos
// Contract Tests - APIs entre servicios
// Performance Tests - Rendimiento y escalabilidad
```

## Pruebas Unitarias Avanzadas

### 1. Testing del Dominio

```csharp
// Tests/Domain/OrderTests.cs
[TestFixture]
public class OrderTests
{
    [Test]
    public void CreateOrder_WithValidData_ShouldCreateOrder()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), 2, 10.99m),
            OrderItem.Create(Guid.NewGuid(), 1, 25.50m)
        };
        var shippingAddress = "123 Main St, City, State 12345";
        var paymentMethod = "CreditCard";
        
        // Act
        var order = Order.Create(customerId, items, shippingAddress, paymentMethod);
        
        // Assert
        Assert.That(order, Is.Not.Null);
        Assert.That(order.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(order.CustomerId, Is.EqualTo(customerId));
        Assert.That(order.Items, Has.Count.EqualTo(2));
        Assert.That(order.Status, Is.EqualTo(OrderStatus.Pending));
        Assert.That(order.ShippingAddress, Is.EqualTo(shippingAddress));
        Assert.That(order.PaymentMethod, Is.EqualTo(paymentMethod));
        Assert.That(order.Total, Is.EqualTo(47.48m));
        Assert.That(order.CreatedAt, Is.GreaterThan(DateTime.UtcNow.AddMinutes(-1)));
    }
    
    [Test]
    public void CreateOrder_WithEmptyItems_ShouldThrowException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var items = new List<OrderItem>();
        var shippingAddress = "123 Main St, City, State 12345";
        var paymentMethod = "CreditCard";
        
        // Act & Assert
        var ex = Assert.Throws<DomainException>(() => 
            Order.Create(customerId, items, shippingAddress, paymentMethod));
        Assert.That(ex.Message, Is.EqualTo("Order must have at least one item"));
    }
    
    [Test]
    public void CreateOrder_WithEmptyCustomerId_ShouldThrowException()
    {
        // Arrange
        var customerId = Guid.Empty;
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), 1, 10.99m)
        };
        var shippingAddress = "123 Main St, City, State 12345";
        var paymentMethod = "CreditCard";
        
        // Act & Assert
        var ex = Assert.Throws<DomainException>(() => 
            Order.Create(customerId, items, shippingAddress, paymentMethod));
        Assert.That(ex.Message, Is.EqualTo("Customer ID is required"));
    }
    
    [Test]
    public void ConfirmOrder_WhenPending_ShouldChangeStatusToConfirmed()
    {
        // Arrange
        var order = CreateValidOrder();
        
        // Act
        order.Confirm();
        
        // Assert
        Assert.That(order.Status, Is.EqualTo(OrderStatus.Confirmed));
        Assert.That(order.ConfirmedAt, Is.Not.Null);
        Assert.That(order.ConfirmedAt, Is.GreaterThan(DateTime.UtcNow.AddMinutes(-1)));
    }
    
    [Test]
    public void ConfirmOrder_WhenNotPending_ShouldThrowException()
    {
        // Arrange
        var order = CreateValidOrder();
        order.Confirm(); // Cambiar a Confirmed
        
        // Act & Assert
        var ex = Assert.Throws<DomainException>(() => order.Confirm());
        Assert.That(ex.Message, Is.EqualTo("Only pending orders can be confirmed"));
    }
    
    [Test]
    public void AddItem_WhenPending_ShouldAddItemToOrder()
    {
        // Arrange
        var order = CreateValidOrder();
        var newItem = OrderItem.Create(Guid.NewGuid(), 3, 15.99m);
        var originalTotal = order.Total;
        
        // Act
        order.AddItem(newItem);
        
        // Assert
        Assert.That(order.Items, Has.Count.EqualTo(2));
        Assert.That(order.Total, Is.EqualTo(originalTotal + newItem.Total));
    }
    
    [Test]
    public void AddItem_WhenNotPending_ShouldThrowException()
    {
        // Arrange
        var order = CreateValidOrder();
        order.Confirm(); // Cambiar a Confirmed
        var newItem = OrderItem.Create(Guid.NewGuid(), 3, 15.99m);
        
        // Act & Assert
        var ex = Assert.Throws<DomainException>(() => order.AddItem(newItem));
        Assert.That(ex.Message, Is.EqualTo("Cannot add items to non-pending orders"));
    }
    
    [Test]
    public void CancelOrder_WhenDelivered_ShouldThrowException()
    {
        // Arrange
        var order = CreateValidOrder();
        order.Confirm();
        order.Ship();
        order.Deliver();
        
        // Act & Assert
        var ex = Assert.Throws<DomainException>(() => order.Cancel("Test reason"));
        Assert.That(ex.Message, Is.EqualTo("Delivered orders cannot be cancelled"));
    }
    
    [Test]
    public void Order_ShouldRaiseDomainEvents()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), 1, 10.99m)
        };
        
        // Act
        var order = Order.Create(customerId, items, "Address", "Payment");
        
        // Assert
        var events = order.GetUncommittedEvents().ToList();
        Assert.That(events, Has.Count.EqualTo(1));
        Assert.That(events[0], Is.TypeOf<OrderCreatedEvent>());
        
        var orderCreatedEvent = events[0] as OrderCreatedEvent;
        Assert.That(orderCreatedEvent.OrderId, Is.EqualTo(order.Id));
        Assert.That(orderCreatedEvent.CustomerId, Is.EqualTo(customerId));
    }
    
    private Order CreateValidOrder()
    {
        var customerId = Guid.NewGuid();
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), 1, 10.99m)
        };
        return Order.Create(customerId, items, "123 Main St", "CreditCard");
    }
}

// Tests/Domain/OrderItemTests.cs
[TestFixture]
public class OrderItemTests
{
    [Test]
    public void Create_WithValidData_ShouldCreateOrderItem()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var quantity = 3;
        var unitPrice = 19.99m;
        
        // Act
        var orderItem = OrderItem.Create(productId, quantity, unitPrice);
        
        // Assert
        Assert.That(orderItem, Is.Not.Null);
        Assert.That(orderItem.ProductId, Is.EqualTo(productId));
        Assert.That(orderItem.Quantity, Is.EqualTo(quantity));
        Assert.That(orderItem.UnitPrice, Is.EqualTo(unitPrice));
        Assert.That(orderItem.Total, Is.EqualTo(quantity * unitPrice));
    }
    
    [Test]
    public void Create_WithZeroQuantity_ShouldThrowException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var quantity = 0;
        var unitPrice = 19.99m;
        
        // Act & Assert
        var ex = Assert.Throws<DomainException>(() => 
            OrderItem.Create(productId, quantity, unitPrice));
        Assert.That(ex.Message, Is.EqualTo("Quantity must be greater than 0"));
    }
    
    [Test]
    public void Create_WithNegativeQuantity_ShouldThrowException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var quantity = -1;
        var unitPrice = 19.99m;
        
        // Act & Assert
        var ex = Assert.Throws<DomainException>(() => 
            OrderItem.Create(productId, quantity, unitPrice));
        Assert.That(ex.Message, Is.EqualTo("Quantity must be greater than 0"));
    }
    
    [Test]
    public void Create_WithZeroUnitPrice_ShouldThrowException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var quantity = 1;
        var unitPrice = 0m;
        
        // Act & Assert
        var ex = Assert.Throws<DomainException>(() => 
            OrderItem.Create(productId, quantity, unitPrice));
        Assert.That(ex.Message, Is.EqualTo("Unit price must be greater than 0"));
    }
    
    [Test]
    public void Create_WithNegativeUnitPrice_ShouldThrowException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var quantity = 1;
        var unitPrice = -10.99m;
        
        // Act & Assert
        var ex = Assert.Throws<DomainException>(() => 
            OrderItem.Create(productId, quantity, unitPrice));
        Assert.That(ex.Message, Is.EqualTo("Unit price must be greater than 0"));
    }
}
```

### 2. Testing de Comandos y Handlers

```csharp
// Tests/Application/Commands/CreateOrderCommandHandlerTests.cs
[TestFixture]
public class CreateOrderCommandHandlerTests
{
    private Mock<IEventSourcedRepository<Order>> _mockOrderRepository;
    private Mock<ICustomerService> _mockCustomerService;
    private Mock<IProductService> _mockProductService;
    private Mock<IMessageBus> _mockMessageBus;
    private Mock<ILogger<CreateOrderCommandHandler>> _mockLogger;
    private CreateOrderCommandHandler _handler;
    
    [SetUp]
    public void Setup()
    {
        _mockOrderRepository = new Mock<IEventSourcedRepository<Order>>();
        _mockCustomerService = new Mock<ICustomerService>();
        _mockProductService = new Mock<IProductService>();
        _mockMessageBus = new Mock<IMessageBus>();
        _mockLogger = new Mock<ILogger<CreateOrderCommandHandler>>();
        
        _handler = new CreateOrderCommandHandler(
            _mockOrderRepository.Object,
            _mockCustomerService.Object,
            _mockProductService.Object,
            _mockMessageBus.Object,
            _mockLogger.Object);
    }
    
    [Test]
    public async Task Handle_WithValidCommand_ShouldCreateOrder()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new OrderItemDto { ProductId = Guid.NewGuid(), Quantity = 2, UnitPrice = 10.99m }
            },
            ShippingAddress = "123 Main St",
            PaymentMethod = "CreditCard",
            CustomerEmail = "test@example.com",
            CustomerPhone = "+1234567890"
        };
        
        var customer = new CustomerDto { Id = command.CustomerId, Name = "Test Customer", Email = "test@example.com" };
        var product = new ProductDto { Id = command.Items[0].ProductId, Name = "Test Product", Stock = 10 };
        
        _mockCustomerService.Setup(x => x.GetCustomerAsync(command.CustomerId))
            .ReturnsAsync(customer);
        _mockProductService.Setup(x => x.GetProductAsync(command.Items[0].ProductId))
            .ReturnsAsync(product);
        _mockOrderRepository.Setup(x => x.SaveAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);
        _mockMessageBus.Setup(x => x.PublishAsync(It.IsAny<OrderCreatedIntegrationEvent>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.OrderId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(result.Total, Is.EqualTo(21.98m));
        
        _mockOrderRepository.Verify(x => x.SaveAsync(It.IsAny<Order>()), Times.Once);
        _mockMessageBus.Verify(x => x.PublishAsync(It.IsAny<OrderCreatedIntegrationEvent>()), Times.Once);
    }
    
    [Test]
    public async Task Handle_WithCustomerNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new OrderItemDto { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 10.99m }
            },
            ShippingAddress = "123 Main St",
            PaymentMethod = "CreditCard",
            CustomerEmail = "test@example.com",
            CustomerPhone = "+1234567890"
        };
        
        _mockCustomerService.Setup(x => x.GetCustomerAsync(command.CustomerId))
            .ReturnsAsync((CustomerDto)null);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Is.EqualTo("Customer not found"));
        
        _mockOrderRepository.Verify(x => x.SaveAsync(It.IsAny<Order>()), Times.Never);
        _mockMessageBus.Verify(x => x.PublishAsync(It.IsAny<OrderCreatedIntegrationEvent>()), Times.Never);
    }
    
    [Test]
    public async Task Handle_WithProductNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new OrderItemDto { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 10.99m }
            },
            ShippingAddress = "123 Main St",
            PaymentMethod = "CreditCard",
            CustomerEmail = "test@example.com",
            CustomerPhone = "+1234567890"
        };
        
        var customer = new CustomerDto { Id = command.CustomerId, Name = "Test Customer", Email = "test@example.com" };
        
        _mockCustomerService.Setup(x => x.GetCustomerAsync(command.CustomerId))
            .ReturnsAsync(customer);
        _mockProductService.Setup(x => x.GetProductAsync(command.Items[0].ProductId))
            .ReturnsAsync((ProductDto)null);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Product") && Does.Contain("not found"));
        
        _mockOrderRepository.Verify(x => x.SaveAsync(It.IsAny<Order>()), Times.Never);
        _mockMessageBus.Verify(x => x.PublishAsync(It.IsAny<OrderCreatedIntegrationEvent>()), Times.Never);
    }
    
    [Test]
    public async Task Handle_WithInsufficientStock_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new OrderItemDto { ProductId = Guid.NewGuid(), Quantity = 5, UnitPrice = 10.99m }
            },
            ShippingAddress = "123 Main St",
            PaymentMethod = "CreditCard",
            CustomerEmail = "test@example.com",
            CustomerPhone = "+1234567890"
        };
        
        var customer = new CustomerDto { Id = command.CustomerId, Name = "Test Customer", Email = "test@example.com" };
        var product = new ProductDto { Id = command.Items[0].ProductId, Name = "Test Product", Stock = 3 };
        
        _mockCustomerService.Setup(x => x.GetCustomerAsync(command.CustomerId))
            .ReturnsAsync(customer);
        _mockProductService.Setup(x => x.GetProductAsync(command.Items[0].ProductId))
            .ReturnsAsync(product);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Insufficient stock"));
        
        _mockOrderRepository.Verify(x => x.SaveAsync(It.IsAny<Order>()), Times.Never);
        _mockMessageBus.Verify(x => x.PublishAsync(It.IsAny<OrderCreatedIntegrationEvent>()), Times.Never);
    }
}
```

### 3. Testing de Validadores

```csharp
// Tests/Application/Commands/CreateOrderCommandValidatorTests.cs
[TestFixture]
public class CreateOrderCommandValidatorTests
{
    private CreateOrderCommandValidator _validator;
    
    [SetUp]
    public void Setup()
    {
        _validator = new CreateOrderCommandValidator();
    }
    
    [Test]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new OrderItemDto { ProductId = Guid.NewGuid(), Quantity = 2, UnitPrice = 10.99m }
            },
            ShippingAddress = "123 Main St, City, State 12345",
            PaymentMethod = "CreditCard",
            CustomerEmail = "test@example.com",
            CustomerPhone = "+1234567890"
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Errors, Has.Count.EqualTo(0));
    }
    
    [Test]
    public void Validate_WithEmptyCustomerId_ShouldFail()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.Empty,
            Items = new List<OrderItemDto>
            {
                new OrderItemDto { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 10.99m }
            },
            ShippingAddress = "123 Main St",
            PaymentMethod = "CreditCard",
            CustomerEmail = "test@example.com",
            CustomerPhone = "+1234567890"
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Property("PropertyName").EqualTo("CustomerId"));
        Assert.That(result.Errors, Has.Some.Property("ErrorMessage").EqualTo("Customer ID is required"));
    }
    
    [Test]
    public void Validate_WithEmptyItems_ShouldFail()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>(),
            ShippingAddress = "123 Main St",
            PaymentMethod = "CreditCard",
            CustomerEmail = "test@example.com",
            CustomerPhone = "+1234567890"
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Property("PropertyName").EqualTo("Items"));
        Assert.That(result.Errors, Has.Some.Property("ErrorMessage").EqualTo("Order must have at least one item"));
    }
    
    [Test]
    public void Validate_WithInvalidPaymentMethod_ShouldFail()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new OrderItemDto { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 10.99m }
            },
            ShippingAddress = "123 Main St",
            PaymentMethod = "InvalidMethod",
            CustomerEmail = "test@example.com",
            CustomerPhone = "+1234567890"
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Property("PropertyName").EqualTo("PaymentMethod"));
        Assert.That(result.Errors, Has.Some.Property("ErrorMessage").EqualTo("Payment method must be valid"));
    }
    
    [Test]
    public void Validate_WithInvalidEmail_ShouldFail()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new OrderItemDto { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 10.99m }
            },
            ShippingAddress = "123 Main St",
            PaymentMethod = "CreditCard",
            CustomerEmail = "invalid-email",
            CustomerPhone = "+1234567890"
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Property("PropertyName").EqualTo("CustomerEmail"));
        Assert.That(result.Errors, Has.Some.Property("ErrorMessage").EqualTo("Valid customer email is required"));
    }
    
    [Test]
    public void Validate_WithInvalidPhone_ShouldFail()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new OrderItemDto { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 10.99m }
            },
            ShippingAddress = "123 Main St",
            PaymentMethod = "CreditCard",
            CustomerEmail = "test@example.com",
            CustomerPhone = "invalid-phone"
        };
        
        // Act
        var result = _validator.Validate(command);
        
        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Some.Property("PropertyName").EqualTo("CustomerPhone"));
        Assert.That(result.Errors, Has.Some.Property("ErrorMessage").EqualTo("Valid customer phone number is required"));
    }
}
```

## Pruebas de Integración

### 1. Testing de Repositorios

```csharp
// Tests/Integration/Repositories/EventSourcedRepositoryTests.cs
[TestFixture]
public class EventSourcedRepositoryTests
{
    private ApplicationDbContext _context;
    private EventStore _eventStore;
    private EventSourcedRepository<Order> _repository;
    
    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new ApplicationDbContext(options);
        _eventStore = new EventStore(_context, Mock.Of<ILogger<EventStore>>());
        _repository = new EventSourcedRepository<Order>(_eventStore, Mock.Of<ILogger<EventSourcedRepository<Order>>>());
    }
    
    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }
    
    [Test]
    public async Task SaveAsync_WithNewOrder_ShouldSaveEvents()
    {
        // Arrange
        var order = Order.Create(
            Guid.NewGuid(),
            new List<OrderItem> { OrderItem.Create(Guid.NewGuid(), 1, 10.99m) },
            "123 Main St",
            "CreditCard");
        
        // Act
        await _repository.SaveAsync(order);
        
        // Assert
        var savedEvents = await _eventStore.GetEventsAsync(order.Id);
        Assert.That(savedEvents, Has.Count.EqualTo(1));
        Assert.That(savedEvents.First(), Is.TypeOf<OrderCreatedEvent>());
    }
    
    [Test]
    public async Task GetByIdAsync_WithExistingOrder_ShouldReconstructOrder()
    {
        // Arrange
        var order = Order.Create(
            Guid.NewGuid(),
            new List<OrderItem> { OrderItem.Create(Guid.NewGuid(), 1, 10.99m) },
            "123 Main St",
            "CreditCard");
        
        await _repository.SaveAsync(order);
        
        // Act
        var retrievedOrder = await _repository.GetByIdAsync(order.Id);
        
        // Assert
        Assert.That(retrievedOrder, Is.Not.Null);
        Assert.That(retrievedOrder.Id, Is.EqualTo(order.Id));
        Assert.That(retrievedOrder.CustomerId, Is.EqualTo(order.CustomerId));
        Assert.That(retrievedOrder.Status, Is.EqualTo(OrderStatus.Pending));
        Assert.That(retrievedOrder.Items, Has.Count.EqualTo(1));
    }
    
    [Test]
    public async Task GetByIdAsync_WithNonExistentOrder_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        
        // Act
        var order = await _repository.GetByIdAsync(nonExistentId);
        
        // Assert
        Assert.That(order, Is.Null);
    }
    
    [Test]
    public async Task SaveAsync_WithUpdatedOrder_ShouldSaveNewEvents()
    {
        // Arrange
        var order = Order.Create(
            Guid.NewGuid(),
            new List<OrderItem> { OrderItem.Create(Guid.NewGuid(), 1, 10.99m) },
            "123 Main St",
            "CreditCard");
        
        await _repository.SaveAsync(order);
        
        // Act
        order.Confirm();
        await _repository.SaveAsync(order);
        
        // Assert
        var savedEvents = await _eventStore.GetEventsAsync(order.Id);
        Assert.That(savedEvents, Has.Count.EqualTo(2));
        Assert.That(savedEvents.First(), Is.TypeOf<OrderCreatedEvent>());
        Assert.That(savedEvents.Last(), Is.TypeOf<OrderStatusChangedEvent>());
    }
}
```

### 2. Testing de Proyecciones

```csharp
// Tests/Integration/Projections/OrderProjectionTests.cs
[TestFixture]
public class OrderProjectionTests
{
    private ApplicationDbContext _context;
    private OrderProjection _projection;
    
    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new ApplicationDbContext(options);
        _projection = new OrderProjection(_context, Mock.Of<ILogger<OrderProjection>>());
        
        // Configurar datos de prueba
        SetupTestData();
    }
    
    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }
    
    [Test]
    public async Task GetOrderAsync_WithExistingOrder_ShouldReturnOrderDto()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        
        // Act
        var result = await _projection.GetOrderAsync(orderId);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(orderId));
        Assert.That(result.Items, Has.Count.EqualTo(2));
        Assert.That(result.Total, Is.EqualTo(36.97m));
    }
    
    [Test]
    public async Task GetOrdersPagedAsync_WithFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var filter = new OrderFilter
        {
            Status = "Pending",
            DateFrom = DateTime.Today.AddDays(-7)
        };
        
        // Act
        var result = await _projection.GetOrdersPagedAsync(filter, 1, 10, "CreatedAt", true);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Items, Has.Count.GreaterThan(0));
        Assert.That(result.Items, Has.All.Property("Status").EqualTo("Pending"));
    }
    
    [Test]
    public async Task GetOrderStatisticsAsync_WithDateRange_ShouldReturnStatistics()
    {
        // Arrange
        var from = DateTime.Today.AddDays(-30);
        var to = DateTime.Today;
        
        // Act
        var result = await _projection.GetOrderStatisticsAsync(from, to);
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.TotalOrders, Is.GreaterThan(0));
        Assert.That(result.TotalRevenue, Is.GreaterThan(0));
        Assert.That(result.OrdersByStatus, Has.Count.GreaterThan(0));
    }
    
    private void SetupTestData()
    {
        var customer = new CustomerProjectionEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Customer",
            Email = "test@example.com",
            Phone = "+1234567890"
        };
        
        var order = new OrderProjectionEntity
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            Customer = customer,
            Items = new List<OrderItemProjectionEntity>
            {
                new OrderItemProjectionEntity
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product 1",
                    Quantity = 2,
                    UnitPrice = 10.99m,
                    Total = 21.98m
                },
                new OrderItemProjectionEntity
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product 2",
                    Quantity = 1,
                    UnitPrice = 14.99m,
                    Total = 14.99m
                }
            },
            Total = 36.97m,
            Status = "Pending",
            ShippingAddress = "123 Main St",
            PaymentMethod = "CreditCard",
            CreatedAt = DateTime.UtcNow,
            Version = 1,
            LastUpdated = DateTime.UtcNow
        };
        
        _context.CustomerProjections.Add(customer);
        _context.OrderProjections.Add(order);
        _context.SaveChanges();
    }
}
```

## Pruebas de Rendimiento

### 1. Testing de Performance

```csharp
// Tests/Performance/OrderServicePerformanceTests.cs
[TestFixture]
public class OrderServicePerformanceTests
{
    private IServiceProvider _serviceProvider;
    private IOrderService _orderService;
    
    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        
        // Configurar servicios con mocks
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IEventSourcedRepository<Order>, EventSourcedRepository<Order>>();
        services.AddScoped<IEventStore, EventStore>();
        services.AddScoped<ICustomerService, MockCustomerService>();
        services.AddScoped<IProductService, MockProductService>();
        services.AddScoped<IMessageBus, MockMessageBus>();
        
        _serviceProvider = services.BuildServiceProvider();
        _orderService = _serviceProvider.GetRequiredService<IOrderService>();
    }
    
    [Test]
    public async Task CreateOrder_WithMultipleItems_ShouldCompleteWithinTimeLimit()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = Enumerable.Range(0, 100)
                .Select(i => new OrderItemDto
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 1,
                    UnitPrice = 10.99m
                })
                .ToList(),
            ShippingAddress = "123 Main St",
            PaymentMethod = "CreditCard",
            CustomerEmail = "test@example.com",
            CustomerPhone = "+1234567890"
        };
        
        var stopwatch = Stopwatch.StartNew();
        
        // Act
        var result = await _orderService.CreateOrderAsync(command);
        
        // Assert
        stopwatch.Stop();
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(1000)); // Debe completarse en menos de 1 segundo
    }
    
    [Test]
    public async Task GetOrder_WithLargeOrder_ShouldCompleteWithinTimeLimit()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var stopwatch = Stopwatch.StartNew();
        
        // Act
        var result = await _orderService.GetOrderAsync(orderId);
        
        // Assert
        stopwatch.Stop();
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(100)); // Debe completarse en menos de 100ms
    }
}

// Tests/Performance/EventStorePerformanceTests.cs
[TestFixture]
public class EventStorePerformanceTests
{
    private ApplicationDbContext _context;
    private EventStore _eventStore;
    
    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new ApplicationDbContext(options);
        _eventStore = new EventStore(_context, Mock.Of<ILogger<EventStore>>());
    }
    
    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }
    
    [Test]
    public async Task SaveEvents_WithManyEvents_ShouldCompleteWithinTimeLimit()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var events = Enumerable.Range(0, 1000)
            .Select(i => new OrderStatusChangedEvent(aggregateId, "Old", "New"))
            .Cast<DomainEvent>()
            .ToList();
        
        var stopwatch = Stopwatch.StartNew();
        
        // Act
        await _eventStore.SaveEventsAsync(aggregateId, events, -1);
        
        // Assert
        stopwatch.Stop();
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(5000)); // Debe completarse en menos de 5 segundos
    }
    
    [Test]
    public async Task GetEvents_WithManyEvents_ShouldCompleteWithinTimeLimit()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var events = Enumerable.Range(0, 1000)
            .Select(i => new OrderStatusChangedEvent(aggregateId, "Old", "New"))
            .Cast<DomainEvent>()
            .ToList();
        
        await _eventStore.SaveEventsAsync(aggregateId, events, -1);
        
        var stopwatch = Stopwatch.StartNew();
        
        // Act
        var retrievedEvents = await _eventStore.GetEventsAsync(aggregateId);
        
        // Assert
        stopwatch.Stop();
        Assert.That(retrievedEvents, Has.Count.EqualTo(1000));
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(1000)); // Debe completarse en menos de 1 segundo
    }
}
```

## Ejercicios Prácticos

### Ejercicio 1: Implementar Pruebas Unitarias
Crea pruebas unitarias para Customer y Product aggregates.

### Ejercicio 2: Crear Pruebas de Integración
Implementa pruebas de integración para CustomerService y ProductService.

### Ejercicio 3: Implementar Pruebas de Rendimiento
Crea pruebas de rendimiento para operaciones críticas del sistema.

## Resumen

En esta clase hemos aprendido:

1. **Testing Avanzado**: Estrategias y tipos de pruebas para arquitecturas limpias
2. **Pruebas Unitarias**: Testing del dominio, comandos y validadores
3. **Pruebas de Integración**: Testing de repositorios y proyecciones
4. **Pruebas de Rendimiento**: Testing de performance y escalabilidad
5. **Mocking y Setup**: Configuración de pruebas con dependencias simuladas

En la siguiente clase continuaremos con el **Proyecto Final** para integrar todos los conceptos aprendidos.

## Recursos Adicionales
- [Unit Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
- [Integration Testing](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [Performance Testing](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test)


