# 🧪 **Clase 1: Fundamentos de Testing Avanzado**

## 🎯 **Objetivos de la Clase**
- Comprender los principios del Test-Driven Development (TDD)
- Implementar Behavior-Driven Development (BDD) con SpecFlow
- Dominar estrategias avanzadas de testing
- Aplicar testing en arquitecturas complejas

## 📚 **Contenido Teórico**

### **1. Test-Driven Development (TDD) Avanzado**

#### **Ciclo Red-Green-Refactor**
```csharp
// 1. RED: Escribir test que falle
[Test]
public void CalculateTotal_WithValidItems_ShouldReturnCorrectTotal()
{
    // Arrange
    var cart = new ShoppingCart();
    cart.AddItem(new CartItem("Product1", 10.00m, 2));
    cart.AddItem(new CartItem("Product2", 15.00m, 1));
    
    // Act
    var total = cart.CalculateTotal();
    
    // Assert
    Assert.That(total, Is.EqualTo(35.00m));
}

// 2. GREEN: Implementar código mínimo para pasar
public class ShoppingCart
{
    private readonly List<CartItem> _items = new();
    
    public void AddItem(CartItem item) => _items.Add(item);
    
    public decimal CalculateTotal() => _items.Sum(i => i.Price * i.Quantity);
}

// 3. REFACTOR: Mejorar el código manteniendo los tests
public class ShoppingCart
{
    private readonly List<CartItem> _items = new();
    
    public void AddItem(CartItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _items.Add(item);
    }
    
    public decimal CalculateTotal() => _items.Sum(i => i.Price * i.Quantity);
    
    public int ItemCount => _items.Count;
    public bool IsEmpty => !_items.Any();
}
```

#### **TDD con Mocks y Stubs**
```csharp
[Test]
public void ProcessOrder_WithValidPayment_ShouldCompleteOrder()
{
    // Arrange
    var mockPaymentService = new Mock<IPaymentService>();
    var mockInventoryService = new Mock<IInventoryService>();
    var orderService = new OrderService(mockPaymentService.Object, mockInventoryService.Object);
    
    var order = new Order { Id = 1, Total = 100.00m };
    
    mockPaymentService.Setup(p => p.ProcessPayment(It.IsAny<decimal>()))
                     .ReturnsAsync(new PaymentResult { Success = true, TransactionId = "TXN123" });
    
    mockInventoryService.Setup(i => i.ReserveItems(It.IsAny<int>()))
                       .ReturnsAsync(true);
    
    // Act
    var result = await orderService.ProcessOrderAsync(order);
    
    // Assert
    Assert.That(result.Success, Is.True);
    Assert.That(result.TransactionId, Is.EqualTo("TXN123"));
    
    mockPaymentService.Verify(p => p.ProcessPayment(100.00m), Times.Once);
    mockInventoryService.Verify(i => i.ReserveItems(1), Times.Once);
}
```

### **2. Behavior-Driven Development (BDD) con SpecFlow**

#### **Configuración de SpecFlow**
```csharp
// SpecFlowFeature.cs
[Binding]
public class OrderProcessingSteps
{
    private Order _order;
    private OrderService _orderService;
    private PaymentResult _paymentResult;
    private readonly Mock<IPaymentService> _mockPaymentService;
    private readonly Mock<IInventoryService> _mockInventoryService;
    
    public OrderProcessingSteps()
    {
        _mockPaymentService = new Mock<IPaymentService>();
        _mockInventoryService = new Mock<IInventoryService>();
        _orderService = new OrderService(_mockPaymentService.Object, _mockInventoryService.Object);
    }
    
    [Given(@"I have an order with total amount of (.*)")]
    public void GivenIHaveAnOrderWithTotalAmountOf(decimal amount)
    {
        _order = new Order { Id = 1, Total = amount };
    }
    
    [Given(@"the payment service is available")]
    public void GivenThePaymentServiceIsAvailable()
    {
        _mockPaymentService.Setup(p => p.ProcessPayment(It.IsAny<decimal>()))
                          .ReturnsAsync(new PaymentResult { Success = true, TransactionId = "TXN123" });
    }
    
    [When(@"I process the order")]
    public async Task WhenIProcessTheOrder()
    {
        _paymentResult = await _orderService.ProcessOrderAsync(_order);
    }
    
    [Then(@"the order should be completed successfully")]
    public void ThenTheOrderShouldBeCompletedSuccessfully()
    {
        Assert.That(_paymentResult.Success, Is.True);
        Assert.That(_paymentResult.TransactionId, Is.Not.Null);
    }
}
```

#### **Feature File**
```gherkin
Feature: Order Processing
  As a customer
  I want to process my order
  So that I can complete my purchase

Scenario: Process order with valid payment
  Given I have an order with total amount of 100.00
  And the payment service is available
  When I process the order
  Then the order should be completed successfully
  And the payment should be processed
  And the inventory should be updated
```

### **3. Estrategias de Testing Avanzadas**

#### **Testing de Arquitecturas Hexagonales**
```csharp
[Test]
public void MusicianMatchingService_WithValidCriteria_ShouldReturnMatchingMusicians()
{
    // Arrange
    var mockMusicianRepository = new Mock<IMusicianRepository>();
    var mockEventRepository = new Mock<IEventRepository>();
    var matchingService = new MusicianMatchingService(mockMusicianRepository.Object, mockEventRepository.Object);
    
    var eventCriteria = new EventCriteria
    {
        Genre = "Rock",
        Location = new Location("Madrid", "Spain"),
        Date = DateTime.Now.AddDays(30),
        Budget = 1000.00m
    };
    
    var availableMusicians = new List<Musician>
    {
        new Musician { Id = 1, Genre = "Rock", Location = new Location("Madrid", "Spain"), Rate = 800.00m },
        new Musician { Id = 2, Genre = "Jazz", Location = new Location("Barcelona", "Spain"), Rate = 1200.00m }
    };
    
    mockMusicianRepository.Setup(r => r.GetAvailableMusiciansAsync(It.IsAny<EventCriteria>()))
                         .ReturnsAsync(availableMusicians);
    
    // Act
    var result = await matchingService.FindMatchingMusiciansAsync(eventCriteria);
    
    // Assert
    Assert.That(result, Has.Count.EqualTo(1));
    Assert.That(result.First().Id, Is.EqualTo(1));
}

[Test]
public void MusicianMatchingService_WithNoMatchingMusicians_ShouldReturnEmptyList()
{
    // Arrange
    var mockMusicianRepository = new Mock<IMusicianRepository>();
    var mockEventRepository = new Mock<IEventRepository>();
    var matchingService = new MusicianMatchingService(mockMusicianRepository.Object, mockEventRepository.Object);
    
    var eventCriteria = new EventCriteria
    {
        Genre = "Classical",
        Location = new Location("Madrid", "Spain"),
        Date = DateTime.Now.AddDays(30),
        Budget = 500.00m
    };
    
    mockMusicianRepository.Setup(r => r.GetAvailableMusiciansAsync(It.IsAny<EventCriteria>()))
                         .ReturnsAsync(new List<Musician>());
    
    // Act
    var result = await matchingService.FindMatchingMusiciansAsync(eventCriteria);
    
    // Assert
    Assert.That(result, Is.Empty);
}
```

#### **Testing de Event Sourcing**
```csharp
[Test]
public void EventStore_WhenAppendingEvents_ShouldPersistCorrectly()
{
    // Arrange
    var eventStore = new InMemoryEventStore();
    var aggregateId = Guid.NewGuid();
    var events = new List<IDomainEvent>
    {
        new MusicianProfileCreated(aggregateId, "John Doe", "Rock"),
        new MusicianProfileUpdated(aggregateId, "John Doe", "Rock", "Updated bio"),
        new MusicianProfileDeactivated(aggregateId)
    };
    
    // Act
    foreach (var evt in events)
    {
        eventStore.AppendEventAsync(aggregateId, evt);
    }
    
    var retrievedEvents = eventStore.GetEventsAsync(aggregateId);
    
    // Assert
    Assert.That(retrievedEvents, Has.Count.EqualTo(3));
    Assert.That(retrievedEvents.First(), Is.TypeOf<MusicianProfileCreated>());
    Assert.That(retrievedEvents.Last(), Is.TypeOf<MusicianProfileDeactivated>());
}
```

## 🛠️ **Ejercicios Prácticos**

### **Ejercicio 1: Implementar TDD para MusicianMatchingService**
```csharp
// Implementar usando TDD:
// 1. Test para matching por género
// 2. Test para matching por ubicación
// 3. Test para matching por presupuesto
// 4. Test para scoring de relevancia

[Test]
public void MusicianMatchingService_WithGenreMatching_ShouldReturnRelevantMusicians()
{
    // TODO: Implementar test TDD
}

[Test]
public void MusicianMatchingService_WithLocationMatching_ShouldReturnLocalMusicians()
{
    // TODO: Implementar test TDD
}

[Test]
public void MusicianMatchingService_WithBudgetMatching_ShouldReturnAffordableMusicians()
{
    // TODO: Implementar test TDD
}

[Test]
public void MusicianMatchingService_WithMultipleCriteria_ShouldReturnScoredResults()
{
    // TODO: Implementar test TDD
}
```

### **Ejercicio 2: Crear Feature SpecFlow para Chat System**
```gherkin
Feature: Real-time Chat
  As a user
  I want to chat with other users
  So that I can communicate in real-time

Scenario: Send message in existing conversation
  Given I have an active conversation with user "musician123"
  When I send a message "Hello, are you available for the event?"
  Then the message should be delivered
  And the other user should receive the message
  And the conversation should be updated

Scenario: Create new conversation
  Given I am logged in as "organizer456"
  When I start a conversation with "musician789"
  Then a new conversation should be created
  And both users should be participants
  And I should be able to send messages
```

### **Ejercicio 3: Testing de CQRS Commands y Queries**
```csharp
[Test]
public void CreateMusicianProfileCommand_WithValidData_ShouldCreateProfile()
{
    // TODO: Implementar test para command
}

[Test]
public void GetMusicianProfileQuery_WithValidId_ShouldReturnProfile()
{
    // TODO: Implementar test para query
}

[Test]
public void UpdateMusicianProfileCommand_WithInvalidData_ShouldThrowException()
{
    // TODO: Implementar test para validación
}
```

## 🎯 **Objetivos de Aprendizaje**

Al finalizar esta clase, serás capaz de:

1. **Implementar TDD** siguiendo el ciclo Red-Green-Refactor
2. **Usar SpecFlow** para BDD y testing de comportamiento
3. **Aplicar mocks y stubs** en testing avanzado
4. **Testear arquitecturas complejas** como Hexagonal y CQRS
5. **Implementar testing de Event Sourcing** y sistemas distribuidos

## 📝 **Resumen**

En esta clase hemos cubierto:

- **TDD Avanzado**: Ciclo Red-Green-Refactor, mocks, stubs
- **BDD con SpecFlow**: Feature files, step definitions, testing de comportamiento
- **Estrategias Avanzadas**: Testing de arquitecturas hexagonales, Event Sourcing
- **Testing de CQRS**: Commands, queries, event handlers

## 🚀 **Siguiente Clase**

En la próxima clase exploraremos **Performance Testing y Load Testing** con herramientas como NBomber y Artillery para asegurar que MussikOn pueda manejar cargas de trabajo reales.

---

**💡 Tip**: El TDD no es solo sobre testing, es sobre diseño. Los tests te guían hacia un código más limpio y mantenible.
