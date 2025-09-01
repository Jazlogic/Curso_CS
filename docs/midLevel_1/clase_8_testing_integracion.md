# 🚀 Clase 8: Testing de Integración

## 📋 Información de la Clase

- **Módulo**: Mid Level 1 - Programación Avanzada y Patrones de Diseño
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 7 (Refactoring y Clean Code)

## 🎯 Objetivos de Aprendizaje

- Implementar tests de integración para sistemas complejos
- Configurar bases de datos de prueba (TestContainers, SQLite en memoria)
- Crear tests de API con WebApplicationFactory
- Implementar tests de integración con servicios externos
- Configurar pipelines de CI/CD para tests de integración

---

## 📚 Navegación del Módulo 4

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_patrones_diseno_intermedios.md) | Patrones de Diseño Intermedios | |
| [Clase 2](clase_2_programacion_asincrona_avanzada.md) | Programación Asíncrona Avanzada | |
| [Clase 3](clase_3_programacion_paralela.md) | Programación Paralela y TPL | |
| [Clase 4](clase_4_clean_architecture.md) | Clean Architecture | |
| [Clase 5](clase_5_dependency_injection.md) | Dependency Injection Avanzada | |
| [Clase 6](clase_6_logging_monitoreo.md) | Logging y Monitoreo | |
| [Clase 7](clase_7_refactoring_clean_code.md) | Refactoring y Clean Code | ← Anterior |
| **Clase 8** | **Testing de Integración** | ← Estás aquí |
| [Clase 9](clase_9_testing_comportamiento.md) | Testing de Comportamiento (BDD) | Siguiente → |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de E-commerce | |

**← [Volver al README del Módulo 4](../midLevel_1/README.md)**

---

## 📚 Contenido Teórico

### 1. Configuración de Tests de Integración

Los tests de integración verifican que múltiples componentes trabajen juntos correctamente.

```csharp
// Configuración de tests de integración
namespace IntegrationTests
{
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    
    // ===== CONFIGURACIÓN DE BASE DE DATOS DE PRUEBA =====
    public class TestDatabaseFixture : IDisposable
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly ApplicationDbContext _context;
        
        public TestDatabaseFixture()
        {
            // Usar SQLite en memoria para tests
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new ApplicationDbContext(_options);
            _context.Database.EnsureCreated();
            
            // Seed data de prueba
            SeedTestData();
        }
        
        public ApplicationDbContext CreateContext()
        {
            return new ApplicationDbContext(_options);
        }
        
        private void SeedTestData()
        {
            var users = new[]
            {
                new User { Id = 1, Email = "test1@example.com", FirstName = "Test", LastName = "User1" },
                new User { Id = 2, Email = "test2@example.com", FirstName = "Test", LastName = "User2" }
            };
            
            var products = new[]
            {
                new Product { Id = 1, Name = "Product 1", Price = 10.99m, Stock = 100 },
                new Product { Id = 2, Name = "Product 2", Price = 20.99m, Stock = 50 }
            };
            
            _context.Users.AddRange(users);
            _context.Products.AddRange(products);
            _context.SaveChanges();
        }
        
        public void Dispose()
        {
            _context?.Dispose();
        }
    }
    
    // ===== FACTORY PERSONALIZADA PARA TESTS =====
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly TestDatabaseFixture _databaseFixture;
        
        public CustomWebApplicationFactory(TestDatabaseFixture databaseFixture)
        {
            _databaseFixture = databaseFixture;
        }
        
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remover servicios reales
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                
                // Agregar servicios de prueba
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });
                
                // Mock de servicios externos
                services.AddScoped<IEmailService, MockEmailService>();
                services.AddScoped<IPaymentService, MockPaymentService>();
                
                // Configurar logging para tests
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Warning);
                });
            });
            
            builder.UseEnvironment("Test");
        }
        
        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);
            
            // Asegurar que la base de datos esté creada
            using (var scope = host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureCreated();
            }
            
            return host;
        }
    }
    
    // ===== SERVICIOS MOCK =====
    public class MockEmailService : IEmailService
    {
        public List<EmailMessage> SentEmails { get; } = new();
        
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            SentEmails.Add(new EmailMessage { To = to, Subject = subject, Body = body });
            await Task.CompletedTask;
        }
        
        public async Task SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string body)
        {
            foreach (var recipient in recipients)
            {
                await SendEmailAsync(recipient, subject, body);
            }
        }
    }
    
    public class MockPaymentService : IPaymentService
    {
        public List<PaymentRequest> ProcessedPayments { get; } = new();
        public bool ShouldFail { get; set; } = false;
        
        public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
        {
            ProcessedPayments.Add(request);
            
            if (ShouldFail)
            {
                return new PaymentResult
                {
                    Success = false,
                    ErrorMessage = "Payment failed for testing purposes"
                };
            }
            
            return new PaymentResult
            {
                Success = true,
                TransactionId = Guid.NewGuid().ToString(),
                Amount = request.Amount
            };
        }
    }
    
    // ===== TESTS DE INTEGRACIÓN =====
    public class UserControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly TestDatabaseFixture _databaseFixture;
        
        public UserControllerIntegrationTests(CustomWebApplicationFactory<Program> factory, TestDatabaseFixture databaseFixture)
        {
            _factory = factory;
            _databaseFixture = databaseFixture;
        }
        
        [Fact]
        public async Task GetUsers_ReturnsAllUsers()
        {
            // Arrange
            var client = _factory.CreateClient();
            
            // Act
            var response = await client.GetAsync("/api/users");
            
            // Assert
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<UserDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            Assert.NotNull(users);
            Assert.Equal(2, users.Count);
            Assert.Contains(users, u => u.Email == "test1@example.com");
            Assert.Contains(users, u => u.Email == "test2@example.com");
        }
        
        [Fact]
        public async Task CreateUser_WithValidData_ReturnsCreatedUser()
        {
            // Arrange
            var client = _factory.CreateClient();
            var newUser = new CreateUserRequest
            {
                Email = "newuser@example.com",
                FirstName = "New",
                LastName = "User"
            };
            
            var json = JsonSerializer.Serialize(newUser);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            // Act
            var response = await client.PostAsync("/api/users", content);
            
            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var createdUser = JsonSerializer.Deserialize<UserDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            Assert.NotNull(createdUser);
            Assert.Equal("newuser@example.com", createdUser.Email);
            Assert.Equal("New", createdUser.FirstName);
            Assert.Equal("User", createdUser.LastName);
            
            // Verificar que se guardó en la base de datos
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var dbUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "newuser@example.com");
            Assert.NotNull(dbUser);
        }
        
        [Fact]
        public async Task CreateUser_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();
            var invalidUser = new CreateUserRequest
            {
                Email = "invalid-email",
                FirstName = "",
                LastName = ""
            };
            
            var json = JsonSerializer.Serialize(invalidUser);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            // Act
            var response = await client.PostAsync("/api/users", content);
            
            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("validation", responseContent.ToLower());
        }
        
        [Fact]
        public async Task UpdateUser_WithValidData_ReturnsUpdatedUser()
        {
            // Arrange
            var client = _factory.CreateClient();
            var updateRequest = new UpdateUserRequest
            {
                FirstName = "Updated",
                LastName = "Name"
            };
            
            var json = JsonSerializer.Serialize(updateRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            // Act
            var response = await client.PutAsync("/api/users/1", content);
            
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var updatedUser = JsonSerializer.Deserialize<UserDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            Assert.NotNull(updatedUser);
            Assert.Equal("Updated", updatedUser.FirstName);
            Assert.Equal("Name", updatedUser.LastName);
        }
        
        [Fact]
        public async Task DeleteUser_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var client = _factory.CreateClient();
            
            // Act
            var response = await client.DeleteAsync("/api/users/1");
            
            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            
            // Verificar que se eliminó de la base de datos
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var deletedUser = await context.Users.FindAsync(1);
            Assert.Null(deletedUser);
        }
    }
    
    // ===== TESTS DE INTEGRACIÓN DE SERVICIOS =====
    public class UserServiceIntegrationTests : IClassFixture<TestDatabaseFixture>
    {
        private readonly TestDatabaseFixture _databaseFixture;
        private readonly UserService _userService;
        private readonly MockEmailService _emailService;
        
        public UserServiceIntegrationTests(TestDatabaseFixture databaseFixture)
        {
            _databaseFixture = databaseFixture;
            
            var context = _databaseFixture.CreateContext();
            _emailService = new MockEmailService();
            
            var userRepository = new UserRepository(context);
            var userValidator = new UserValidator();
            
            _userService = new UserService(userRepository, userValidator, _emailService);
        }
        
        [Fact]
        public async Task CreateUser_WithValidData_CreatesUserAndSendsEmail()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Email = "integration@example.com",
                FirstName = "Integration",
                LastName = "Test"
            };
            
            // Act
            var result = await _userService.CreateUserAsync(request);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("integration@example.com", result.Email);
            Assert.Equal("Integration", result.FirstName);
            Assert.Equal("Test", result.LastName);
            
            // Verificar que se envió el email
            Assert.Single(_emailService.SentEmails);
            var email = _emailService.SentEmails.First();
            Assert.Equal("integration@example.com", email.To);
            Assert.Contains("Bienvenido", email.Subject);
            
            // Verificar que se guardó en la base de datos
            using var context = _databaseFixture.CreateContext();
            var dbUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "integration@example.com");
            Assert.NotNull(dbUser);
        }
        
        [Fact]
        public async Task CreateUser_WithDuplicateEmail_ThrowsException()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Email = "test1@example.com", // Email ya existe
                FirstName = "Duplicate",
                LastName = "User"
            };
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _userService.CreateUserAsync(request));
            
            Assert.Contains("already exists", exception.Message);
            
            // Verificar que no se envió email
            Assert.Empty(_emailService.SentEmails);
        }
        
        [Fact]
        public async Task GetUser_WithValidId_ReturnsUser()
        {
            // Arrange
            var userId = 1;
            
            // Act
            var result = await _userService.GetUserAsync(userId);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal("test1@example.com", result.Email);
        }
        
        [Fact]
        public async Task GetUser_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var userId = 999; // ID que no existe
            
            // Act
            var result = await _userService.GetUserAsync(userId);
            
            // Assert
            Assert.Null(result);
        }
        
        [Fact]
        public async Task UpdateUser_WithValidData_UpdatesUser()
        {
            // Arrange
            var userId = 1;
            var request = new UpdateUserRequest
            {
                FirstName = "Updated",
                LastName = "Name"
            };
            
            // Act
            await _userService.UpdateUserAsync(userId, request);
            
            // Assert
            using var context = _databaseFixture.CreateContext();
            var updatedUser = await context.Users.FindAsync(userId);
            
            Assert.NotNull(updatedUser);
            Assert.Equal("Updated", updatedUser.FirstName);
            Assert.Equal("Name", updatedUser.LastName);
        }
        
        [Fact]
        public async Task DeleteUser_WithValidId_DeletesUser()
        {
            // Arrange
            var userId = 2;
            
            // Act
            await _userService.DeleteUserAsync(userId);
            
            // Assert
            using var context = _databaseFixture.CreateContext();
            var deletedUser = await context.Users.FindAsync(userId);
            Assert.Null(deletedUser);
        }
    }
    
    // ===== TESTS DE INTEGRACIÓN DE BASE DE DATOS =====
    public class DatabaseIntegrationTests : IClassFixture<TestDatabaseFixture>
    {
        private readonly TestDatabaseFixture _databaseFixture;
        
        public DatabaseIntegrationTests(TestDatabaseFixture databaseFixture)
        {
            _databaseFixture = databaseFixture;
        }
        
        [Fact]
        public async Task Database_CanCreateAndRetrieveUser()
        {
            // Arrange
            using var context = _databaseFixture.CreateContext();
            var user = new User
            {
                Email = "db@example.com",
                FirstName = "Database",
                LastName = "Test"
            };
            
            // Act
            context.Users.Add(user);
            await context.SaveChangesAsync();
            
            // Assert
            var retrievedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "db@example.com");
            Assert.NotNull(retrievedUser);
            Assert.Equal("Database", retrievedUser.FirstName);
            Assert.Equal("Test", retrievedUser.LastName);
        }
        
        [Fact]
        public async Task Database_CanUpdateUser()
        {
            // Arrange
            using var context = _databaseFixture.CreateContext();
            var user = await context.Users.FindAsync(1);
            Assert.NotNull(user);
            
            // Act
            user.FirstName = "Updated";
            await context.SaveChangesAsync();
            
            // Assert
            var updatedUser = await context.Users.FindAsync(1);
            Assert.NotNull(updatedUser);
            Assert.Equal("Updated", updatedUser.FirstName);
        }
        
        [Fact]
        public async Task Database_CanDeleteUser()
        {
            // Arrange
            using var context = _databaseFixture.CreateContext();
            var user = await context.Users.FindAsync(2);
            Assert.NotNull(user);
            
            // Act
            context.Users.Remove(user);
            await context.SaveChangesAsync();
            
            // Assert
            var deletedUser = await context.Users.FindAsync(2);
            Assert.Null(deletedUser);
        }
        
        [Fact]
        public async Task Database_TransactionRollback_WorksCorrectly()
        {
            // Arrange
            using var context = _databaseFixture.CreateContext();
            var initialUserCount = await context.Users.CountAsync();
            
            // Act
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var user = new User
                {
                    Email = "transaction@example.com",
                    FirstName = "Transaction",
                    LastName = "Test"
                };
                
                context.Users.Add(user);
                await context.SaveChangesAsync();
                
                // Simular error
                throw new Exception("Simulated error");
            }
            catch
            {
                await transaction.RollbackAsync();
            }
            
            // Assert
            var finalUserCount = await context.Users.CountAsync();
            Assert.Equal(initialUserCount, finalUserCount);
            
            var addedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "transaction@example.com");
            Assert.Null(addedUser);
        }
    }
    
    // ===== TESTS DE INTEGRACIÓN DE SERVICIOS EXTERNOS =====
    public class ExternalServiceIntegrationTests
    {
        [Fact]
        public async Task PaymentService_WithValidRequest_ProcessesPayment()
        {
            // Arrange
            var paymentService = new MockPaymentService();
            var request = new PaymentRequest
            {
                Amount = 100.00m,
                Currency = "USD",
                PaymentMethod = "CreditCard",
                CustomerId = "customer123"
            };
            
            // Act
            var result = await paymentService.ProcessPaymentAsync(request);
            
            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.TransactionId);
            Assert.Equal(100.00m, result.Amount);
            Assert.Single(paymentService.ProcessedPayments);
        }
        
        [Fact]
        public async Task PaymentService_WithFailureFlag_ReturnsError()
        {
            // Arrange
            var paymentService = new MockPaymentService { ShouldFail = true };
            var request = new PaymentRequest
            {
                Amount = 100.00m,
                Currency = "USD",
                PaymentMethod = "CreditCard",
                CustomerId = "customer123"
            };
            
            // Act
            var result = await paymentService.ProcessPaymentAsync(request);
            
            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.ErrorMessage);
            Assert.Contains("failed", result.ErrorMessage);
        }
        
        [Fact]
        public async Task EmailService_SendsBulkEmails()
        {
            // Arrange
            var emailService = new MockEmailService();
            var recipients = new[] { "user1@example.com", "user2@example.com", "user3@example.com" };
            var subject = "Bulk Test";
            var body = "This is a bulk test email";
            
            // Act
            await emailService.SendBulkEmailAsync(recipients, subject, body);
            
            // Assert
            Assert.Equal(3, emailService.SentEmails.Count);
            Assert.All(emailService.SentEmails, email =>
            {
                Assert.Equal(subject, email.Subject);
                Assert.Equal(body, email.Body);
            });
            
            Assert.Contains(emailService.SentEmails, e => e.To == "user1@example.com");
            Assert.Contains(emailService.SentEmails, e => e.To == "user2@example.com");
            Assert.Contains(emailService.SentEmails, e => e.To == "user3@example.com");
        }
    }
    
    // ===== CONFIGURACIÓN DE TESTS DE INTEGRACIÓN =====
    public class IntegrationTestConfiguration
    {
        public static IServiceCollection ConfigureTestServices(IServiceCollection services)
        {
            // Configurar base de datos de prueba
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });
            
            // Configurar servicios mock
            services.AddScoped<IEmailService, MockEmailService>();
            services.AddScoped<IPaymentService, MockPaymentService>();
            
            // Configurar logging para tests
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Warning);
            });
            
            return services;
        }
        
        public static WebApplicationFactory<Program> CreateTestWebApplicationFactory()
        {
            return new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(ConfigureTestServices);
                    builder.UseEnvironment("Test");
                });
        }
    }
}

// Uso de tests de integración
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Tests de Integración - Configuración y Ejecución ===\n");
        
        // En una aplicación real, estos tests se ejecutarían con:
        // dotnet test --filter "Category=Integration"
        
        Console.WriteLine("Los tests de integración están configurados para:");
        Console.WriteLine("1. Usar SQLite en memoria para base de datos de prueba");
        Console.WriteLine("2. Mock de servicios externos (email, pagos)");
        Console.WriteLine("3. WebApplicationFactory para tests de API");
        Console.WriteLine("4. Fixtures para configuración de base de datos");
        Console.WriteLine("5. Tests de transacciones y rollback");
        
        Console.WriteLine("\nPara ejecutar los tests:");
        Console.WriteLine("dotnet test --filter \"Category=Integration\"");
        Console.WriteLine("dotnet test --filter \"FullyQualifiedName~IntegrationTests\"");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Tests de Integración para API REST
Crea tests de integración completos para una API REST que incluya CRUD operations y validaciones.

### Ejercicio 2: Tests de Base de Datos con Transacciones
Implementa tests que verifiquen el comportamiento de transacciones de base de datos, incluyendo rollback.

### Ejercicio 3: Tests de Servicios Externos
Desarrolla tests de integración para servicios externos como APIs de terceros y sistemas de pago.

## 🔍 Puntos Clave

1. **Tests de Integración** verifican que múltiples componentes trabajen juntos
2. **TestDatabaseFixture** proporciona base de datos limpia para cada test
3. **WebApplicationFactory** permite tests de API completos
4. **Mock Services** simulan servicios externos para tests controlados
5. **Transacciones** permiten tests de rollback y comportamiento de base de datos

## 📚 Recursos Adicionales

- [Integration Testing - Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [Entity Framework Testing - Microsoft Docs](https://docs.microsoft.com/en-us/ef/core/testing/)
- [WebApplicationFactory - Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests#customize-webapplicationfactory)

---

**🎯 ¡Has completado la Clase 8! Ahora dominas el testing de integración en C#**

**📚 [Siguiente: Clase 9 - Testing de Comportamiento (BDD)](clase_9_testing_comportamiento.md)**
