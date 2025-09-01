# 🚀 Clase 5: Testing de Comportamiento

## 📋 Información de la Clase

- **Módulo**: Senior Level 2 - Testing y TDD
- **Duración**: 2 horas
- **Nivel**: Avanzado
- **Prerrequisitos**: Testing de Integración (Clase 4)

## 🎯 Objetivos de Aprendizaje

- Implementar BDD (Behavior Driven Development)
- Usar Gherkin para especificar comportamientos
- Crear pruebas de comportamiento con SpecFlow
- Implementar testing de escenarios de usuario

---

## 📚 Navegación del Módulo 2

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_fundamentos_testing.md) | Fundamentos de Testing | |
| [Clase 2](clase_2_desarrollo_dirigido_pruebas.md) | Desarrollo Dirigido por Pruebas (TDD) | |
| [Clase 3](clase_3_testing_unitario.md) | Testing Unitario | |
| [Clase 4](clase_4_testing_integracion.md) | Testing de Integración | ← Anterior |
| **Clase 5** | **Testing de Comportamiento** | ← Estás aquí |
| [Clase 6](clase_6_mocking_frameworks.md) | Frameworks de Mocking | Siguiente → |
| [Clase 7](clase_7_testing_asincrono.md) | Testing de Código Asíncrono | |
| [Clase 8](clase_8_testing_apis.md) | Testing de APIs | |
| [Clase 9](clase_9_testing_database.md) | Testing de Base de Datos | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Testing | |

**← [Volver al README del Módulo 2](../senior_2/README.md)**

---

## 📚 Contenido Teórico

### 1. ¿Qué es BDD (Behavior Driven Development)?

BDD es una metodología que combina las mejores prácticas de TDD con el análisis de requisitos, enfocándose en el comportamiento del software desde la perspectiva del usuario.

### 2. Características del BDD

- **Lenguaje natural** (Gherkin)
- **Colaboración** entre desarrolladores, QA y stakeholders
- **Escenarios** de usuario claros
- **Documentación viva** del comportamiento

```csharp
// ===== TESTING DE COMPORTAMIENTO - IMPLEMENTACIÓN COMPLETA =====
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Xunit;

namespace BehaviorTesting
{
    // ===== MODELOS DE DOMINIO =====
    namespace Domain
    {
        public class User
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreatedDate { get; set; }
            public List<Role> Roles { get; set; } = new List<Role>();
        }
        
        public class Role
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<Permission> Permissions { get; set; } = new List<Permission>();
        }
        
        public class Permission
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Resource { get; set; }
            public string Action { get; set; }
        }
        
        public class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int Stock { get; set; }
            public string Category { get; set; }
            public bool IsAvailable => Stock > 0;
        }
        
        public class Order
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public List<OrderItem> Items { get; set; } = new List<OrderItem>();
            public DateTime OrderDate { get; set; }
            public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
            public OrderStatus Status { get; set; }
        }
        
        public class OrderItem
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice => Quantity * UnitPrice;
        }
        
        public enum OrderStatus
        {
            Pending,
            Confirmed,
            Shipped,
            Delivered,
            Cancelled
        }
    }
    
    // ===== SERVICIOS DE NEGOCIO =====
    namespace Services
    {
        public interface IUserService
        {
            Task<User> RegisterUserAsync(string username, string email, string password);
            Task<bool> AuthenticateUserAsync(string username, string password);
            Task<bool> AssignRoleAsync(int userId, string roleName);
            Task<bool> HasPermissionAsync(int userId, string resource, string action);
        }
        
        public class UserService : IUserService
        {
            private readonly List<User> _users = new List<User>();
            private readonly List<Role> _roles = new List<Role>();
            private readonly List<Permission> _permissions = new List<Permission>();
            
            public UserService()
            {
                InitializeRolesAndPermissions();
            }
            
            private void InitializeRolesAndPermissions()
            {
                // Crear permisos básicos
                var viewProducts = new Permission { Id = 1, Name = "View Products", Resource = "Products", Action = "View" };
                var createOrders = new Permission { Id = 2, Name = "Create Orders", Resource = "Orders", Action = "Create" };
                var manageUsers = new Permission { Id = 3, Name = "Manage Users", Resource = "Users", Action = "Manage" };
                
                _permissions.AddRange(new[] { viewProducts, createOrders, manageUsers });
                
                // Crear roles
                var customerRole = new Role { Id = 1, Name = "Customer", Permissions = new List<Permission> { viewProducts, createOrders } };
                var adminRole = new Role { Id = 2, Name = "Admin", Permissions = new List<Permission> { viewProducts, createOrders, manageUsers } };
                
                _roles.AddRange(new[] { customerRole, adminRole });
            }
            
            public async Task<User> RegisterUserAsync(string username, string email, string password)
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                    throw new ArgumentException("Username, email and password are required");
                
                if (_users.Any(u => u.Username == username))
                    throw new InvalidOperationException("Username already exists");
                
                if (_users.Any(u => u.Email == email))
                    throw new InvalidOperationException("Email already exists");
                
                var user = new User
                {
                    Id = _users.Count + 1,
                    Username = username,
                    Email = email,
                    Password = HashPassword(password),
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };
                
                _users.Add(user);
                return user;
            }
            
            public async Task<bool> AuthenticateUserAsync(string username, string password)
            {
                var user = _users.FirstOrDefault(u => u.Username == username && u.IsActive);
                if (user == null) return false;
                
                return VerifyPassword(password, user.Password);
            }
            
            public async Task<bool> AssignRoleAsync(int userId, string roleName)
            {
                var user = _users.FirstOrDefault(u => u.Id == userId);
                var role = _roles.FirstOrDefault(r => r.Name == roleName);
                
                if (user == null || role == null) return false;
                
                if (!user.Roles.Any(r => r.Name == roleName))
                {
                    user.Roles.Add(role);
                }
                
                return true;
            }
            
            public async Task<bool> HasPermissionAsync(int userId, string resource, string action)
            {
                var user = _users.FirstOrDefault(u => u.Id == userId);
                if (user == null) return false;
                
                return user.Roles.Any(r => r.Permissions.Any(p => p.Resource == resource && p.Action == action));
            }
            
            private string HashPassword(string password)
            {
                // Simulación simple de hash
                return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
            }
            
            private bool VerifyPassword(string password, string hashedPassword)
            {
                var hashedInput = HashPassword(password);
                return hashedInput == hashedPassword;
            }
        }
        
        public interface IOrderService
        {
            Task<Order> CreateOrderAsync(int userId, List<OrderItem> items);
            Task<bool> ConfirmOrderAsync(int orderId);
            Task<bool> CancelOrderAsync(int orderId);
            Task<Order> GetOrderAsync(int orderId);
        }
        
        public class OrderService : IOrderService
        {
            private readonly List<Order> _orders = new List<Order>();
            private readonly List<Product> _products = new List<Product>();
            
            public OrderService()
            {
                InitializeProducts();
            }
            
            private void InitializeProducts()
            {
                _products.AddRange(new[]
                {
                    new Product { Id = 1, Name = "Laptop", Price = 999.99m, Stock = 10, Category = "Electronics" },
                    new Product { Id = 2, Name = "Mouse", Price = 29.99m, Stock = 50, Category = "Electronics" },
                    new Product { Id = 3, Name = "Keyboard", Price = 59.99m, Stock = 25, Category = "Electronics" }
                });
            }
            
            public async Task<Order> CreateOrderAsync(int userId, List<OrderItem> items)
            {
                if (items == null || !items.Any())
                    throw new ArgumentException("Order must have at least one item");
                
                // Verificar stock disponible
                foreach (var item in items)
                {
                    var product = _products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product == null)
                        throw new ArgumentException($"Product {item.ProductId} not found");
                    
                    if (product.Stock < item.Quantity)
                        throw new InvalidOperationException($"Insufficient stock for product {product.Name}");
                }
                
                var order = new Order
                {
                    Id = _orders.Count + 1,
                    UserId = userId,
                    Items = items,
                    OrderDate = DateTime.Now,
                    Status = OrderStatus.Pending
                };
                
                _orders.Add(order);
                return order;
            }
            
            public async Task<bool> ConfirmOrderAsync(int orderId)
            {
                var order = _orders.FirstOrDefault(o => o.Id == orderId);
                if (order == null || order.Status != OrderStatus.Pending) return false;
                
                // Verificar stock nuevamente
                foreach (var item in order.Items)
                {
                    var product = _products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product.Stock < item.Quantity) return false;
                }
                
                // Reducir stock
                foreach (var item in order.Items)
                {
                    var product = _products.FirstOrDefault(p => p.Id == item.ProductId);
                    product.Stock -= item.Quantity;
                }
                
                order.Status = OrderStatus.Confirmed;
                return true;
            }
            
            public async Task<bool> CancelOrderAsync(int orderId)
            {
                var order = _orders.FirstOrDefault(o => o.Id == orderId);
                if (order == null || order.Status != OrderStatus.Pending) return false;
                
                order.Status = OrderStatus.Cancelled;
                return true;
            }
            
            public async Task<Order> GetOrderAsync(int orderId)
            {
                return _orders.FirstOrDefault(o => o.Id == orderId);
            }
        }
    }
    
    // ===== ESCENARIOS GHERKIN =====
    namespace Scenarios
    {
        [Binding]
        public class UserRegistrationSteps
        {
            private readonly IUserService _userService;
            private User _registeredUser;
            private Exception _exception;
            
            public UserRegistrationSteps()
            {
                _userService = new UserService();
            }
            
            [Given(@"a new user wants to register")]
            public void GivenANewUserWantsToRegister()
            {
                // Preparar el escenario
                _registeredUser = null;
                _exception = null;
            }
            
            [When(@"they provide valid username ""(.*)"", email ""(.*)"", and password ""(.*)""")]
            public async Task WhenTheyProvideValidCredentials(string username, string email, string password)
            {
                try
                {
                    _registeredUser = await _userService.RegisterUserAsync(username, email, password);
                }
                catch (Exception ex)
                {
                    _exception = ex;
                }
            }
            
            [Then(@"the user should be registered successfully")]
            public void ThenTheUserShouldBeRegisteredSuccessfully()
            {
                Assert.NotNull(_registeredUser);
                Assert.True(_registeredUser.IsActive);
                Assert.Equal(DateTime.Now.Date, _registeredUser.CreatedDate.Date);
            }
            
            [Then(@"the user should have a unique ID")]
            public void ThenTheUserShouldHaveAUniqueId()
            {
                Assert.True(_registeredUser.Id > 0);
            }
            
            [When(@"they provide an existing username ""(.*)""")]
            public async Task WhenTheyProvideAnExistingUsername(string username)
            {
                try
                {
                    _registeredUser = await _userService.RegisterUserAsync(username, "new@email.com", "password123");
                }
                catch (Exception ex)
                {
                    _exception = ex;
                }
            }
            
            [Then(@"registration should fail with username already exists error")]
            public void ThenRegistrationShouldFailWithUsernameAlreadyExistsError()
            {
                Assert.NotNull(_exception);
                Assert.IsType<InvalidOperationException>(_exception);
                Assert.Contains("Username already exists", _exception.Message);
            }
        }
        
        [Binding]
        public class UserAuthenticationSteps
        {
            private readonly IUserService _userService;
            private User _user;
            private bool _authenticationResult;
            
            public UserAuthenticationSteps()
            {
                _userService = new UserService();
            }
            
            [Given(@"a registered user with username ""(.*)"" and password ""(.*)""")]
            public async Task GivenARegisteredUser(string username, string password)
            {
                _user = await _userService.RegisterUserAsync(username, "user@example.com", password);
            }
            
            [When(@"they attempt to authenticate with correct credentials")]
            public async Task WhenTheyAttemptToAuthenticateWithCorrectCredentials()
            {
                _authenticationResult = await _userService.AuthenticateUserAsync(_user.Username, "password123");
            }
            
            [Then(@"authentication should succeed")]
            public void ThenAuthenticationShouldSucceed()
            {
                Assert.True(_authenticationResult);
            }
            
            [When(@"they attempt to authenticate with incorrect password")]
            public async Task WhenTheyAttemptToAuthenticateWithIncorrectPassword()
            {
                _authenticationResult = await _userService.AuthenticateUserAsync(_user.Username, "wrongpassword");
            }
            
            [Then(@"authentication should fail")]
            public void ThenAuthenticationShouldFail()
            {
                Assert.False(_authenticationResult);
            }
        }
        
        [Binding]
        public class OrderManagementSteps
        {
            private readonly IOrderService _orderService;
            private readonly IUserService _userService;
            private Order _order;
            private Exception _exception;
            
            public OrderManagementSteps()
            {
                _orderService = new OrderService();
                _userService = new UserService();
            }
            
            [Given(@"a registered user")]
            public async Task GivenARegisteredUser()
            {
                await _userService.RegisterUserAsync("testuser", "test@example.com", "password123");
            }
            
            [When(@"they create an order with available products")]
            public async Task WhenTheyCreateAnOrderWithAvailableProducts()
            {
                var orderItems = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 1, UnitPrice = 999.99m }
                };
                
                try
                {
                    _order = await _orderService.CreateOrderAsync(1, orderItems);
                }
                catch (Exception ex)
                {
                    _exception = ex;
                }
            }
            
            [Then(@"the order should be created with pending status")]
            public void ThenTheOrderShouldBeCreatedWithPendingStatus()
            {
                Assert.NotNull(_order);
                Assert.Equal(OrderStatus.Pending, _order.Status);
                Assert.Equal(999.99m, _order.TotalAmount);
            }
            
            [When(@"they try to create an order with insufficient stock")]
            public async Task WhenTheyTryToCreateAnOrderWithInsufficientStock()
            {
                var orderItems = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 999, UnitPrice = 999.99m }
                };
                
                try
                {
                    _order = await _orderService.CreateOrderAsync(1, orderItems);
                }
                catch (Exception ex)
                {
                    _exception = ex;
                }
            }
            
            [Then(@"order creation should fail with insufficient stock error")]
            public void ThenOrderCreationShouldFailWithInsufficientStockError()
            {
                Assert.NotNull(_exception);
                Assert.IsType<InvalidOperationException>(_exception);
                Assert.Contains("Insufficient stock", _exception.Message);
            }
            
            [When(@"they confirm the order")]
            public async Task WhenTheyConfirmTheOrder()
            {
                var result = await _orderService.ConfirmOrderAsync(_order.Id);
                Assert.True(result);
            }
            
            [Then(@"the order status should change to confirmed")]
            public async Task ThenTheOrderStatusShouldChangeToConfirmed()
            {
                var updatedOrder = await _orderService.GetOrderAsync(_order.Id);
                Assert.Equal(OrderStatus.Confirmed, updatedOrder.Status);
            }
        }
        
        [Binding]
        public class PermissionManagementSteps
        {
            private readonly IUserService _userService;
            private User _user;
            private bool _permissionResult;
            
            public PermissionManagementSteps()
            {
                _userService = new UserService();
            }
            
            [Given(@"a registered user")]
            public async Task GivenARegisteredUser()
            {
                _user = await _userService.RegisterUserAsync("testuser", "test@example.com", "password123");
            }
            
            [When(@"they are assigned the ""(.*)"" role")]
            public async Task WhenTheyAreAssignedTheRole(string roleName)
            {
                var result = await _userService.AssignRoleAsync(_user.Id, roleName);
                Assert.True(result);
            }
            
            [Then(@"they should have permission to ""(.*)"" ""(.*)""")]
            public async Task ThenTheyShouldHavePermissionTo(string action, string resource)
            {
                _permissionResult = await _userService.HasPermissionAsync(_user.Id, resource, action);
                Assert.True(_permissionResult);
            }
            
            [Then(@"they should not have permission to ""(.*)"" ""(.*)""")]
            public async Task ThenTheyShouldNotHavePermissionTo(string action, string resource)
            {
                _permissionResult = await _userService.HasPermissionAsync(_user.Id, resource, action);
                Assert.False(_permissionResult);
            }
        }
    }
    
    // ===== PRUEBAS DE COMPORTAMIENTO =====
    namespace BehaviorTests
    {
        public class UserBehaviorTests
        {
            private readonly IUserService _userService;
            
            public UserBehaviorTests()
            {
                _userService = new UserService();
            }
            
            [Fact]
            public async Task UserRegistration_WithValidCredentials_ShouldSucceed()
            {
                // Arrange
                var username = "newuser";
                var email = "new@example.com";
                var password = "securepassword123";
                
                // Act
                var user = await _userService.RegisterUserAsync(username, email, password);
                
                // Assert
                Assert.NotNull(user);
                Assert.Equal(username, user.Username);
                Assert.Equal(email, user.Email);
                Assert.True(user.IsActive);
                Assert.Equal(DateTime.Now.Date, user.CreatedDate.Date);
            }
            
            [Fact]
            public async Task UserRegistration_WithDuplicateUsername_ShouldFail()
            {
                // Arrange
                var username = "duplicateuser";
                var email1 = "first@example.com";
                var email2 = "second@example.com";
                var password = "password123";
                
                // Act - First registration
                var firstUser = await _userService.RegisterUserAsync(username, email1, password);
                Assert.NotNull(firstUser);
                
                // Act - Second registration with same username
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    _userService.RegisterUserAsync(username, email2, password));
                
                // Assert
                Assert.Contains("Username already exists", exception.Message);
            }
            
            [Fact]
            public async Task UserAuthentication_WithValidCredentials_ShouldSucceed()
            {
                // Arrange
                var username = "authuser";
                var password = "mypassword";
                var user = await _userService.RegisterUserAsync(username, "auth@example.com", password);
                
                // Act
                var result = await _userService.AuthenticateUserAsync(username, password);
                
                // Assert
                Assert.True(result);
            }
            
            [Fact]
            public async Task UserAuthentication_WithInvalidCredentials_ShouldFail()
            {
                // Arrange
                var username = "authuser";
                var password = "mypassword";
                var user = await _userService.RegisterUserAsync(username, "auth@example.com", password);
                
                // Act
                var result = await _userService.AuthenticateUserAsync(username, "wrongpassword");
                
                // Assert
                Assert.False(result);
            }
        }
        
        public class OrderBehaviorTests
        {
            private readonly IOrderService _orderService;
            
            public OrderBehaviorTests()
            {
                _orderService = new OrderService();
            }
            
            [Fact]
            public async Task OrderCreation_WithValidItems_ShouldSucceed()
            {
                // Arrange
                var userId = 1;
                var orderItems = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 1, UnitPrice = 999.99m },
                    new OrderItem { ProductId = 2, Quantity = 2, UnitPrice = 29.99m }
                };
                
                // Act
                var order = await _orderService.CreateOrderAsync(userId, orderItems);
                
                // Assert
                Assert.NotNull(order);
                Assert.Equal(userId, order.UserId);
                Assert.Equal(OrderStatus.Pending, order.Status);
                Assert.Equal(2, order.Items.Count);
                Assert.Equal(999.99m + (2 * 29.99m), order.TotalAmount);
            }
            
            [Fact]
            public async Task OrderCreation_WithInsufficientStock_ShouldFail()
            {
                // Arrange
                var userId = 1;
                var orderItems = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 999, UnitPrice = 999.99m }
                };
                
                // Act & Assert
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    _orderService.CreateOrderAsync(userId, orderItems));
                
                Assert.Contains("Insufficient stock", exception.Message);
            }
            
            [Fact]
            public async Task OrderConfirmation_WithSufficientStock_ShouldSucceed()
            {
                // Arrange
                var userId = 1;
                var orderItems = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 1, UnitPrice = 999.99m }
                };
                
                var order = await _orderService.CreateOrderAsync(userId, orderItems);
                
                // Act
                var result = await _orderService.ConfirmOrderAsync(order.Id);
                
                // Assert
                Assert.True(result);
                
                var confirmedOrder = await _orderService.GetOrderAsync(order.Id);
                Assert.Equal(OrderStatus.Confirmed, confirmedOrder.Status);
            }
        }
    }
}

// ===== DEMOSTRACIÓN DE TESTING DE COMPORTAMIENTO =====
public class BehaviorTestingDemonstration
{
    public static async Task DemonstrateBehaviorTesting()
    {
        Console.WriteLine("=== Testing de Comportamiento - Clase 5 ===\n");
        
        // Crear servicios
        var userService = new BehaviorTesting.Services.UserService();
        var orderService = new BehaviorTesting.Services.OrderService();
        
        Console.WriteLine("1. REGISTRO DE USUARIO:");
        var user = await userService.RegisterUserAsync("testuser", "test@example.com", "password123");
        Console.WriteLine($"Usuario registrado: {user.Username} (ID: {user.Id})");
        
        Console.WriteLine("\n2. AUTENTICACIÓN:");
        var authResult = await userService.AuthenticateUserAsync("testuser", "password123");
        Console.WriteLine($"Autenticación exitosa: {authResult}");
        
        Console.WriteLine("\n3. ASIGNACIÓN DE ROL:");
        var roleResult = await userService.AssignRoleAsync(user.Id, "Customer");
        Console.WriteLine($"Rol asignado: {roleResult}");
        
        Console.WriteLine("\n4. VERIFICACIÓN DE PERMISOS:");
        var canViewProducts = await userService.HasPermissionAsync(user.Id, "Products", "View");
        var canManageUsers = await userService.HasPermissionAsync(user.Id, "Users", "Manage");
        Console.WriteLine($"Puede ver productos: {canViewProducts}");
        Console.WriteLine($"Puede gestionar usuarios: {canManageUsers}");
        
        Console.WriteLine("\n5. CREACIÓN DE ORDEN:");
        var orderItems = new List<BehaviorTesting.Domain.OrderItem>
        {
            new BehaviorTesting.Domain.OrderItem { ProductId = 1, Quantity = 1, UnitPrice = 999.99m }
        };
        
        var order = await orderService.CreateOrderAsync(user.Id, orderItems);
        Console.WriteLine($"Orden creada: ID {order.Id}, Total: ${order.TotalAmount}, Estado: {order.Status}");
        
        Console.WriteLine("\n6. CONFIRMACIÓN DE ORDEN:");
        var confirmResult = await orderService.ConfirmOrderAsync(order.Id);
        Console.WriteLine($"Orden confirmada: {confirmResult}");
        
        if (confirmResult)
        {
            var confirmedOrder = await orderService.GetOrderAsync(order.Id);
            Console.WriteLine($"Estado final: {confirmedOrder.Status}");
        }
        
        Console.WriteLine("\n✅ Testing de Comportamiento demostrado!");
        Console.WriteLine("BDD permite especificar y probar el comportamiento del software desde la perspectiva del usuario.");
    }
}

// Programa principal
public class Program
{
    public static async Task Main()
    {
        await BehaviorTestingDemonstration.DemonstrateBehaviorTesting();
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Escenarios de Usuario
Implementa escenarios BDD para:
- Flujo de compra completa
- Sistema de cupones y descuentos
- Gestión de inventario

### Ejercicio 2: Testing de Permisos
Crea pruebas de comportamiento para:
- Sistema de roles y permisos
- Control de acceso a recursos
- Auditoría de acciones

### Ejercicio 3: Flujos de Negocio
Implementa testing para:
- Aprobación de órdenes
- Procesamiento de pagos
- Notificaciones automáticas

## 🔍 Puntos Clave

1. **BDD** combina TDD con análisis de requisitos
2. **Gherkin** proporciona lenguaje natural para escenarios
3. **Escenarios** describen comportamiento desde perspectiva del usuario
4. **Colaboración** entre equipos técnicos y de negocio
5. **Documentación viva** que se mantiene actualizada
6. **Testing de comportamiento** verifica flujos completos
7. **Escenarios de usuario** claros y comprensibles
8. **Integración** con herramientas de testing existentes

## 📚 Recursos Adicionales

- [SpecFlow Documentation](https://docs.specflow.org/)
- [BDD Best Practices](https://cucumber.io/docs/bdd/)
- [Gherkin Reference](https://cucumber.io/docs/gherkin/reference/)

---

**🎯 ¡Has completado la Clase 5! Ahora comprendes el Testing de Comportamiento**

**📚 [Siguiente: Clase 6 - Frameworks de Mocking](clase_6_mocking_framworks.md)**
