# 🏆 Senior Level 2: Testing y TDD

## 🧭 Navegación del Curso

- **⬅️ Anterior**: [Módulo 8: Patrones de Diseño](../senior_1/README.md)
- **➡️ Siguiente**: [Módulo 10: APIs REST](../senior_3/README.md)
- **📚 [Índice Completo](../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../NAVEGACION_RAPIDA.md)**

---

## 📋 Contenido del Nivel

### 🎯 Objetivos de Aprendizaje
- Dominar el desarrollo dirigido por pruebas (TDD)
- Implementar testing unitario, de integración y de comportamiento
- Usar frameworks de testing como xUnit, NUnit y MSTest
- Implementar mocking y testing de código asíncrono
- Crear suites de pruebas robustas y mantenibles

### ⏱️ Tiempo Estimado
- **Teoría**: 3-4 horas
- **Ejercicios**: 6-8 horas
- **Proyecto Integrador**: 4-5 horas
- **Total**: 13-17 horas

---

## 📚 Contenido Teórico

### 1. Fundamentos de Testing

#### 1.1 ¿Qué es el Testing?
El testing es el proceso de verificar que el software funciona correctamente y cumple con los requisitos especificados.

#### 1.2 Tipos de Testing

```csharp
// Testing Unitario - Prueba una unidad de código aislada
[Test]
public void CalculateTotal_WithValidItems_ReturnsCorrectTotal()
{
    // Arrange
    var calculator = new OrderCalculator();
    var items = new List<OrderItem>
    {
        new OrderItem { Price = 10.00m, Quantity = 2 },
        new OrderItem { Price = 15.00m, Quantity = 1 }
    };
    
    // Act
    var total = calculator.CalculateTotal(items);
    
    // Assert
    Assert.AreEqual(35.00m, total);
}

// Testing de Integración - Prueba la interacción entre componentes
[Test]
public void OrderService_WithValidOrder_CreatesOrderInDatabase()
{
    // Arrange
    var orderService = new OrderService(new OrderRepository());
    var order = new Order { CustomerId = 1, Total = 100.00m };
    
    // Act
    var result = orderService.CreateOrder(order);
    
    // Assert
    Assert.IsTrue(result.IsSuccess);
    Assert.IsNotNull(result.OrderId);
}
```

#### 1.3 Pirámide de Testing
```
        /\
       /  \     E2E Tests (Pocos, lentos, costosos)
      /____\
     /      \   Integration Tests (Algunos, medios)
    /________\
   /          \  Unit Tests (Muchos, rápidos, baratos)
  /____________\
```

### 2. Desarrollo Dirigido por Pruebas (TDD)

#### 2.1 Ciclo Red-Green-Refactor

```csharp
// 1. RED - Escribir la prueba que falle
[Test]
public void Calculator_Add_ReturnsSum()
{
    // Arrange
    var calculator = new Calculator();
    
    // Act
    var result = calculator.Add(2, 3);
    
    // Assert
    Assert.AreEqual(5, result); // Esta prueba fallará porque no existe el método
}

// 2. GREEN - Implementar el código mínimo para que pase la prueba
public class Calculator
{
    public int Add(int a, int b)
    {
        return a + b; // Implementación mínima
    }
}

// 3. REFACTOR - Mejorar el código sin cambiar el comportamiento
public class Calculator
{
    public int Add(int a, int b) => a + b; // Código más limpio
}
```

#### 2.2 Ejemplo Completo de TDD

```csharp
// Paso 1: Prueba para validación de email
[Test]
public void EmailValidator_WithValidEmail_ReturnsTrue()
{
    // Arrange
    var validator = new EmailValidator();
    
    // Act
    var result = validator.IsValid("test@example.com");
    
    // Assert
    Assert.IsTrue(result);
}

[Test]
public void EmailValidator_WithInvalidEmail_ReturnsFalse()
{
    // Arrange
    var validator = new EmailValidator();
    
    // Act
    var result = validator.IsValid("invalid-email");
    
    // Assert
    Assert.IsFalse(result);
}

// Paso 2: Implementación mínima
public class EmailValidator
{
    public bool IsValid(string email)
    {
        return email.Contains("@") && email.Contains(".");
    }
}

// Paso 3: Refactor y más pruebas
[Test]
public void EmailValidator_WithEmptyEmail_ReturnsFalse()
{
    var validator = new EmailValidator();
    var result = validator.IsValid("");
    Assert.IsFalse(result);
}

[Test]
public void EmailValidator_WithNullEmail_ReturnsFalse()
{
    var validator = new EmailValidator();
    var result = validator.IsValid(null);
    Assert.IsFalse(result);
}

// Implementación mejorada
public class EmailValidator
{
    public bool IsValid(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;
            
        return email.Contains("@") && email.Contains(".") && 
               email.IndexOf("@") < email.LastIndexOf(".");
    }
}
```

### 3. Frameworks de Testing

#### 3.1 xUnit (Recomendado)

```csharp
using Xunit;

public class CalculatorTests
{
    [Fact]
    public void Add_TwoNumbers_ReturnsSum()
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act
        var result = calculator.Add(2, 3);
        
        // Assert
        Assert.Equal(5, result);
    }
    
    [Theory]
    [InlineData(2, 3, 5)]
    [InlineData(0, 0, 0)]
    [InlineData(-1, 1, 0)]
    public void Add_VariousNumbers_ReturnsCorrectSum(int a, int b, int expected)
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act
        var result = calculator.Add(a, b);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public void Divide_ByZero_ThrowsException()
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act & Assert
        var exception = Assert.Throws<DivideByZeroException>(
            () => calculator.Divide(10, 0));
            
        Assert.Equal("Cannot divide by zero", exception.Message);
    }
}
```

#### 3.2 NUnit

```csharp
using NUnit.Framework;

[TestFixture]
public class CalculatorTests
{
    [Test]
    public void Add_TwoNumbers_ReturnsSum()
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act
        var result = calculator.Add(2, 3);
        
        // Assert
        Assert.That(result, Is.EqualTo(5));
    }
    
    [TestCase(2, 3, 5)]
    [TestCase(0, 0, 0)]
    [TestCase(-1, 1, 0)]
    public void Add_VariousNumbers_ReturnsCorrectSum(int a, int b, int expected)
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act
        var result = calculator.Add(a, b);
        
        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }
}
```

#### 3.3 MSTest

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class CalculatorTests
{
    [TestMethod]
    public void Add_TwoNumbers_ReturnsSum()
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act
        var result = calculator.Add(2, 3);
        
        // Assert
        Assert.AreEqual(5, result);
    }
    
    [DataTestMethod]
    [DataRow(2, 3, 5)]
    [DataRow(0, 0, 0)]
    [DataRow(-1, 1, 0)]
    public void Add_VariousNumbers_ReturnsCorrectSum(int a, int b, int expected)
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act
        var result = calculator.Add(a, b);
        
        // Assert
        Assert.AreEqual(expected, result);
    }
}
```

### 4. Mocking y Dependencias

#### 4.1 Moq Framework

```csharp
using Moq;

[Test]
public void OrderService_WithValidOrder_CallsRepository()
{
    // Arrange
    var mockRepository = new Mock<IOrderRepository>();
    var orderService = new OrderService(mockRepository.Object);
    var order = new Order { Id = 1, Total = 100.00m };
    
    mockRepository.Setup(r => r.Save(It.IsAny<Order>()))
                 .Returns(1);
    
    // Act
    var result = orderService.CreateOrder(order);
    
    // Assert
    mockRepository.Verify(r => r.Save(It.IsAny<Order>()), Times.Once);
    Assert.IsTrue(result.IsSuccess);
}

[Test]
public void OrderService_WhenRepositoryFails_ReturnsFailure()
{
    // Arrange
    var mockRepository = new Mock<IOrderRepository>();
    var orderService = new OrderService(mockRepository.Object);
    var order = new Order { Id = 1, Total = 100.00m };
    
    mockRepository.Setup(r => r.Save(It.IsAny<Order>()))
                 .Throws(new Exception("Database error"));
    
    // Act
    var result = orderService.CreateOrder(order);
    
    // Assert
    Assert.IsFalse(result.IsSuccess);
    Assert.Contains("Database error", result.ErrorMessage);
}
```

#### 4.2 Stubs vs Mocks

```csharp
// Stub - Proporciona respuestas predefinidas
public class StubEmailService : IEmailService
{
    public bool SendEmail(string to, string subject, string body)
    {
        return true; // Siempre retorna true
    }
}

// Mock - Verifica comportamiento
[Test]
public void UserService_WhenUserCreated_SendsWelcomeEmail()
{
    // Arrange
    var mockEmailService = new Mock<IEmailService>();
    var userService = new UserService(mockEmailService.Object);
    var user = new User { Email = "test@example.com" };
    
    // Act
    userService.CreateUser(user);
    
    // Assert
    mockEmailService.Verify(e => e.SendEmail(
        "test@example.com", 
        "Welcome!", 
        It.IsAny<string>()), 
        Times.Once);
}
```

### 5. Testing de Código Asíncrono

#### 5.1 Testing de Métodos Async

```csharp
[Test]
public async Task UserService_GetUserByIdAsync_ReturnsUser()
{
    // Arrange
    var mockRepository = new Mock<IUserRepository>();
    var userService = new UserService(mockRepository.Object);
    var expectedUser = new User { Id = 1, Name = "John" };
    
    mockRepository.Setup(r => r.GetByIdAsync(1))
                 .ReturnsAsync(expectedUser);
    
    // Act
    var result = await userService.GetUserByIdAsync(1);
    
    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual("John", result.Name);
}

[Test]
public async Task UserService_GetUserByIdAsync_WhenUserNotFound_ReturnsNull()
{
    // Arrange
    var mockRepository = new Mock<IUserRepository>();
    var userService = new UserService(mockRepository.Object);
    
    mockRepository.Setup(r => r.GetByIdAsync(999))
                 .ReturnsAsync((User)null);
    
    // Act
    var result = await userService.GetUserByIdAsync(999);
    
    // Assert
    Assert.IsNull(result);
}
```

#### 5.2 Testing de Excepciones Asíncronas

```csharp
[Test]
public async Task UserService_GetUserByIdAsync_WhenRepositoryThrows_PropagatesException()
{
    // Arrange
    var mockRepository = new Mock<IUserRepository>();
    var userService = new UserService(mockRepository.Object);
    
    mockRepository.Setup(r => r.GetByIdAsync(1))
                 .ThrowsAsync(new DatabaseException("Connection failed"));
    
    // Act & Assert
    var exception = await Assert.ThrowsAsync<DatabaseException>(
        () => userService.GetUserByIdAsync(1));
        
    Assert.Contains("Connection failed", exception.Message);
}
```

### 6. Testing de Comportamiento (BDD)

#### 6.1 SpecFlow

```gherkin
Feature: User Registration
  As a user
  I want to register for an account
  So that I can access the system

Scenario: Successful user registration
  Given I am on the registration page
  When I enter valid user information
  And I click the register button
  Then I should see a success message
  And my account should be created
  And I should receive a welcome email

Scenario: Registration with invalid email
  Given I am on the registration page
  When I enter an invalid email address
  And I click the register button
  Then I should see an error message
  And my account should not be created
```

#### 6.2 Implementación en C#

```csharp
[Binding]
public class UserRegistrationSteps
{
    private UserRegistrationPage _registrationPage;
    private UserService _userService;
    private Mock<IEmailService> _mockEmailService;
    
    [Given(@"I am on the registration page")]
    public void GivenIAmOnTheRegistrationPage()
    {
        _registrationPage = new UserRegistrationPage();
    }
    
    [When(@"I enter valid user information")]
    public void WhenIEnterValidUserInformation()
    {
        _registrationPage.EnterEmail("test@example.com");
        _registrationPage.EnterPassword("SecurePass123!");
        _registrationPage.EnterConfirmPassword("SecurePass123!");
    }
    
    [When(@"I click the register button")]
    public void WhenIClickTheRegisterButton()
    {
        _registrationPage.ClickRegister();
    }
    
    [Then(@"I should see a success message")]
    public void ThenIShouldSeeASuccessMessage()
    {
        Assert.IsTrue(_registrationPage.IsSuccessMessageDisplayed());
    }
    
    [Then(@"my account should be created")]
    public void ThenMyAccountShouldBeCreated()
    {
        var user = _userService.GetUserByEmail("test@example.com");
        Assert.IsNotNull(user);
    }
}
```

### 7. Testing de Integración

#### 7.1 Testing con Base de Datos

```csharp
[TestFixture]
public class UserRepositoryIntegrationTests
{
    private TestDbContext _context;
    private UserRepository _repository;
    
    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _context = new TestDbContext(options);
        _repository = new UserRepository(_context);
    }
    
    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
    
    [Test]
    public async Task SaveAsync_WithValidUser_SavesToDatabase()
    {
        // Arrange
        var user = new User { Email = "test@example.com", Name = "Test User" };
        
        // Act
        await _repository.SaveAsync(user);
        
        // Assert
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        Assert.IsNotNull(savedUser);
        Assert.AreEqual("Test User", savedUser.Name);
    }
}
```

#### 7.2 Testing de APIs

```csharp
[TestFixture]
public class UserControllerIntegrationTests
{
    private TestServer _server;
    private HttpClient _client;
    
    [SetUp]
    public void Setup()
    {
        var builder = new WebHostBuilder()
            .UseStartup<TestStartup>();
            
        _server = new TestServer(builder);
        _client = _server.CreateClient();
    }
    
    [Test]
    public async Task GetUser_WithValidId_ReturnsUser()
    {
        // Arrange
        var userId = 1;
        
        // Act
        var response = await _client.GetAsync($"/api/users/{userId}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<User>(content);
        Assert.AreEqual(userId, user.Id);
    }
}
```

---

## 🎯 Ejercicios Prácticos

### Ejercicio 1: Calculadora con TDD
Implementa una calculadora usando TDD que soporte operaciones básicas.

### Ejercicio 2: Validador de Email
Crea un validador de email siguiendo el ciclo Red-Green-Refactor.

### Ejercicio 3: Repository Pattern con Testing
Implementa un repository genérico y sus pruebas unitarias.

### Ejercicio 4: Service Layer Testing
Crea servicios de negocio con mocking de dependencias.

### Ejercicio 5: Testing de Código Asíncrono
Implementa y prueba métodos async/await.

### Ejercicio 6: Testing de APIs
Crea pruebas de integración para controladores de API.

### Ejercicio 7: Testing de Base de Datos
Implementa pruebas de integración con base de datos en memoria.

### Ejercicio 8: BDD con SpecFlow
Crea escenarios de comportamiento para un sistema de login.

### Ejercicio 9: Testing de Excepciones
Implementa pruebas para diferentes tipos de excepciones.

### Ejercicio 10: Performance Testing
Crea pruebas de rendimiento para operaciones críticas.

---

## 🚀 Proyecto Integrador: Sistema de Gestión de Pedidos con TDD

### Descripción
Crea un sistema de gestión de pedidos siguiendo completamente el enfoque TDD.

### Requisitos
- Implementación completa usando TDD
- Testing unitario de todos los componentes
- Testing de integración con base de datos
- Testing de comportamiento con SpecFlow
- Cobertura de código del 90%+

### Estructura Sugerida
```
OrderManagementTDD/
├── OrderManagement.Domain/
│   ├── Entities/
│   ├── Services/
│   └── Repositories/
├── OrderManagement.Tests/
│   ├── Unit/
│   ├── Integration/
│   └── Behavior/
├── OrderManagement.API/
└── OrderManagement.Infrastructure/
```

### Ejemplo de Implementación TDD

```csharp
// 1. Prueba para validación de pedido
[Test]
public void OrderValidator_WithValidOrder_ReturnsSuccess()
{
    // Arrange
    var validator = new OrderValidator();
    var order = new Order
    {
        CustomerId = 1,
        Items = new List<OrderItem>
        {
            new OrderItem { ProductId = 1, Quantity = 2, Price = 10.00m }
        }
    };
    
    // Act
    var result = validator.Validate(order);
    
    // Assert
    Assert.IsTrue(result.IsValid);
}

// 2. Implementación mínima
public class OrderValidator
{
    public ValidationResult Validate(Order order)
    {
        if (order.CustomerId <= 0)
            return ValidationResult.Failure("Invalid customer ID");
            
        if (order.Items == null || !order.Items.Any())
            return ValidationResult.Failure("Order must have items");
            
        return ValidationResult.Success();
    }
}
```

---

## 📝 Autoevaluación

### Preguntas Teóricas
1. ¿Cuáles son los tres pasos del ciclo TDD?
2. ¿Cuál es la diferencia entre un stub y un mock?
3. ¿Qué es la pirámide de testing y por qué es importante?
4. ¿Cuándo usarías testing de integración vs unitario?
5. ¿Qué ventajas ofrece BDD sobre testing tradicional?

### Preguntas Prácticas
1. Implementa una calculadora usando TDD
2. Crea un mock para un servicio de email
3. Escribe pruebas para un método async
4. Implementa testing de integración con base de datos

---

## 🔗 Enlaces de Referencia

- [xUnit Documentation](https://xunit.net/)
- [NUnit Documentation](https://docs.nunit.org/)
- [MSTest Documentation](https://docs.microsoft.com/en-us/visualstudio/test/unit-test-basics)
- [Moq Framework](https://github.com/moq/moq4)
- [SpecFlow](https://specflow.org/)
- [Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/)

---

## 📚 Siguiente Nivel

**Progreso**: 8 de 12 niveles completados

**Siguiente**: [Senior Level 3: APIs REST y Web APIs](../senior_3/README.md)

**Anterior**: [Senior Level 1: Patrones de Diseño y SOLID](../senior_1/README.md)

---

## 🎉 ¡Felicidades!

Has completado el nivel senior de testing y TDD. Ahora puedes:
- Desarrollar software siguiendo el enfoque TDD
- Crear suites de pruebas robustas y mantenibles
- Implementar testing unitario, de integración y de comportamiento
- Usar mocking para aislar dependencias
- Aplicar mejores prácticas de testing en proyectos reales

¡Continúa con el siguiente nivel para dominar APIs REST y Web APIs!
