# 🚀 Clase 6: Frameworks de Mocking

## 📋 Información de la Clase

- **Módulo**: Senior Level 2 - Testing y TDD
- **Duración**: 2 horas
- **Nivel**: Avanzado
- **Prerrequisitos**: Testing de Comportamiento (Clase 5)

## 🎯 Objetivos de Aprendizaje

- Implementar mocking con Moq y NSubstitute
- Crear mocks de interfaces y clases
- Configurar comportamientos de mocks
- Usar mocks para testing aislado

---

## 📚 Navegación del Módulo 2

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_fundamentos_testing.md) | Fundamentos de Testing | |
| [Clase 2](clase_2_desarrollo_dirigido_pruebas.md) | Desarrollo Dirigido por Pruebas (TDD) | |
| [Clase 3](clase_3_testing_unitario.md) | Testing Unitario | |
| [Clase 4](clase_4_testing_integracion.md) | Testing de Integración | |
| [Clase 5](clase_5_testing_comportamiento.md) | Testing de Comportamiento | ← Anterior |
| **Clase 6** | **Frameworks de Mocking** | ← Estás aquí |
| [Clase 7](clase_7_testing_asincrono.md) | Testing de Código Asíncrono | Siguiente → |
| [Clase 8](clase_8_testing_apis.md) | Testing de APIs | |
| [Clase 9](clase_9_testing_database.md) | Testing de Base de Datos | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Testing | |

**← [Volver al README del Módulo 2](../senior_2/README.md)**

---

## 📚 Contenido Teórico

### 1. ¿Qué es Mocking?

Mocking es una técnica que permite crear objetos simulados que reemplazan dependencias reales durante el testing, permitiendo control total sobre su comportamiento.

### 2. Ventajas del Mocking

- **Aislamiento** de código bajo prueba
- **Control** sobre dependencias externas
- **Velocidad** en ejecución de pruebas
- **Predictibilidad** en resultados

```csharp
// ===== FRAMEWORKS DE MOCKING - IMPLEMENTACIÓN COMPLETA =====
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NSubstitute;
using Xunit;

namespace MockingFrameworks
{
    // ===== INTERFACES Y MODELOS =====
    namespace Interfaces
    {
        public interface IUserRepository
        {
            Task<User> GetByIdAsync(int id);
            Task<IEnumerable<User>> GetAllAsync();
            Task<User> CreateAsync(User user);
            Task<bool> UpdateAsync(User user);
            Task<bool> DeleteAsync(int id);
            Task<User> GetByEmailAsync(string email);
        }
        
        public interface IEmailService
        {
            Task<bool> SendWelcomeEmailAsync(string email, string username);
            Task<bool> SendPasswordResetEmailAsync(string email, string resetToken);
            Task<bool> SendNotificationEmailAsync(string email, string subject, string message);
        }
        
        public interface ILogger
        {
            void LogInfo(string message);
            void LogWarning(string message);
            void LogError(string message, Exception exception = null);
        }
        
        public interface IConfigurationService
        {
            string GetConnectionString();
            int GetMaxRetryAttempts();
            TimeSpan GetTimeout();
            bool IsFeatureEnabled(string featureName);
        }
        
        public interface ICacheService
        {
            Task<T> GetAsync<T>(string key);
            Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
            Task RemoveAsync(string key);
            Task<bool> ExistsAsync(string key);
        }
    }
    
    // ===== MODELOS DE DOMINIO =====
    namespace Models
    {
        public class User
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string PasswordHash { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime? LastLoginDate { get; set; }
            public List<string> Roles { get; set; } = new List<string>();
        }
        
        public class UserRegistrationRequest
        {
            public string Username { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }
        
        public class UserRegistrationResult
        {
            public bool Success { get; set; }
            public User User { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
        }
        
        public class EmailNotification
        {
            public string To { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
            public DateTime SentDate { get; set; }
        }
    }
    
    // ===== SERVICIOS DE NEGOCIO =====
    namespace Services
    {
        public interface IUserService
        {
            Task<UserRegistrationResult> RegisterUserAsync(UserRegistrationRequest request);
            Task<bool> AuthenticateUserAsync(string username, string password);
            Task<bool> ResetPasswordAsync(string email);
            Task<User> GetUserProfileAsync(int userId);
        }
        
        public class UserService : IUserService
        {
            private readonly IUserRepository _userRepository;
            private readonly IEmailService _emailService;
            private readonly ILogger _logger;
            private readonly IConfigurationService _config;
            private readonly ICacheService _cache;
            
            public UserService(
                IUserRepository userRepository,
                IEmailService emailService,
                ILogger logger,
                IConfigurationService config,
                ICacheService cache)
            {
                _userRepository = userRepository;
                _emailService = emailService;
                _logger = logger;
                _config = config;
                _cache = cache;
            }
            
            public async Task<UserRegistrationResult> RegisterUserAsync(UserRegistrationRequest request)
            {
                try
                {
                    _logger.LogInfo($"Starting user registration for {request.Email}");
                    
                    // Validar entrada
                    var validationErrors = ValidateRegistrationRequest(request);
                    if (validationErrors.Any())
                    {
                        return new UserRegistrationResult { Success = false, Errors = validationErrors };
                    }
                    
                    // Verificar si el usuario ya existe
                    var existingUser = await _userRepository.GetByEmailAsync(request.Email);
                    if (existingUser != null)
                    {
                        _logger.LogWarning($"User with email {request.Email} already exists");
                        return new UserRegistrationResult 
                        { 
                            Success = false, 
                            Errors = new List<string> { "User with this email already exists" } 
                        };
                    }
                    
                    // Crear usuario
                    var user = new User
                    {
                        Username = request.Username,
                        Email = request.Email,
                        PasswordHash = HashPassword(request.Password),
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        Roles = new List<string> { "User" }
                    };
                    
                    var createdUser = await _userRepository.CreateAsync(user);
                    
                    // Enviar email de bienvenida
                    var emailSent = await _emailService.SendWelcomeEmailAsync(user.Email, user.Username);
                    if (!emailSent)
                    {
                        _logger.LogWarning($"Failed to send welcome email to {user.Email}");
                    }
                    
                    // Cachear información del usuario
                    await _cache.SetAsync($"user:{user.Id}", user, TimeSpan.FromHours(1));
                    
                    _logger.LogInfo($"User {user.Username} registered successfully");
                    
                    return new UserRegistrationResult { Success = true, User = createdUser };
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error during user registration", ex);
                    return new UserRegistrationResult 
                    { 
                        Success = false, 
                        Errors = new List<string> { "An unexpected error occurred" } 
                    };
                }
            }
            
            public async Task<bool> AuthenticateUserAsync(string username, string password)
            {
                try
                {
                    var user = await _userRepository.GetByEmailAsync(username);
                    if (user == null || !user.IsActive) return false;
                    
                    var isValidPassword = VerifyPassword(password, user.PasswordHash);
                    if (isValidPassword)
                    {
                        user.LastLoginDate = DateTime.Now;
                        await _userRepository.UpdateAsync(user);
                        
                        // Actualizar cache
                        await _cache.SetAsync($"user:{user.Id}", user, TimeSpan.FromHours(1));
                    }
                    
                    return isValidPassword;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error during authentication", ex);
                    return false;
                }
            }
            
            public async Task<bool> ResetPasswordAsync(string email)
            {
                try
                {
                    var user = await _userRepository.GetByEmailAsync(email);
                    if (user == null || !user.IsActive) return false;
                    
                    var resetToken = GenerateResetToken();
                    var emailSent = await _emailService.SendPasswordResetEmailAsync(email, resetToken);
                    
                    return emailSent;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error during password reset", ex);
                    return false;
                }
            }
            
            public async Task<User> GetUserProfileAsync(int userId)
            {
                try
                {
                    // Intentar obtener del cache primero
                    var cachedUser = await _cache.GetAsync<User>($"user:{userId}");
                    if (cachedUser != null)
                    {
                        return cachedUser;
                    }
                    
                    // Si no está en cache, obtener de la base de datos
                    var user = await _userRepository.GetByIdAsync(userId);
                    if (user != null)
                    {
                        // Cachear para futuras consultas
                        await _cache.SetAsync($"user:{userId}", user, TimeSpan.FromHours(1));
                    }
                    
                    return user;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error getting user profile for ID {userId}", ex);
                    return null;
                }
            }
            
            private List<string> ValidateRegistrationRequest(UserRegistrationRequest request)
            {
                var errors = new List<string>();
                
                if (string.IsNullOrWhiteSpace(request.Username))
                    errors.Add("Username is required");
                else if (request.Username.Length < 3)
                    errors.Add("Username must be at least 3 characters long");
                
                if (string.IsNullOrWhiteSpace(request.Email))
                    errors.Add("Email is required");
                else if (!IsValidEmail(request.Email))
                    errors.Add("Invalid email format");
                
                if (string.IsNullOrWhiteSpace(request.Password))
                    errors.Add("Password is required");
                else if (request.Password.Length < 8)
                    errors.Add("Password must be at least 8 characters long");
                
                return errors;
            }
            
            private string HashPassword(string password)
            {
                // Simulación simple de hash
                return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
            }
            
            private bool VerifyPassword(string password, string hash)
            {
                var hashedInput = HashPassword(password);
                return hashedInput == hash;
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
            
            private string GenerateResetToken()
            {
                return Guid.NewGuid().ToString("N");
            }
        }
    }
    
    // ===== TESTING CON MOQ =====
    namespace MoqTests
    {
        public class UserServiceMoqTests
        {
            private readonly Mock<IUserRepository> _mockUserRepository;
            private readonly Mock<IEmailService> _mockEmailService;
            private readonly Mock<ILogger> _mockLogger;
            private readonly Mock<IConfigurationService> _mockConfig;
            private readonly Mock<ICacheService> _mockCache;
            private readonly UserService _userService;
            
            public UserServiceMoqTests()
            {
                _mockUserRepository = new Mock<IUserRepository>();
                _mockEmailService = new Mock<IEmailService>();
                _mockLogger = new Mock<ILogger>();
                _mockConfig = new Mock<IConfigurationService>();
                _mockCache = new Mock<ICacheService>();
                
                _userService = new UserService(
                    _mockUserRepository.Object,
                    _mockEmailService.Object,
                    _mockLogger.Object,
                    _mockConfig.Object,
                    _mockCache.Object);
            }
            
            [Fact]
            public async Task RegisterUser_WithValidRequest_ShouldSucceed()
            {
                // Arrange
                var request = new UserRegistrationRequest
                {
                    Username = "testuser",
                    Email = "test@example.com",
                    Password = "password123"
                };
                
                var expectedUser = new User
                {
                    Id = 1,
                    Username = request.Username,
                    Email = request.Email,
                    IsActive = true
                };
                
                _mockUserRepository.Setup(r => r.GetByEmailAsync(request.Email))
                    .ReturnsAsync((User)null);
                
                _mockUserRepository.Setup(r => r.CreateAsync(It.IsAny<User>()))
                    .ReturnsAsync(expectedUser);
                
                _mockEmailService.Setup(e => e.SendWelcomeEmailAsync(request.Email, request.Username))
                    .ReturnsAsync(true);
                
                _mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<User>(), It.IsAny<TimeSpan?>()))
                    .Returns(Task.CompletedTask);
                
                // Act
                var result = await _userService.RegisterUserAsync(request);
                
                // Assert
                Assert.True(result.Success);
                Assert.NotNull(result.User);
                Assert.Equal(request.Username, result.User.Username);
                Assert.Equal(request.Email, result.User.Email);
                
                // Verificar que se llamaron los métodos esperados
                _mockUserRepository.Verify(r => r.GetByEmailAsync(request.Email), Times.Once);
                _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
                _mockEmailService.Verify(e => e.SendWelcomeEmailAsync(request.Email, request.Username), Times.Once);
                _mockCache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<User>(), It.IsAny<TimeSpan?>()), Times.Once);
            }
            
            [Fact]
            public async Task RegisterUser_WithExistingEmail_ShouldFail()
            {
                // Arrange
                var request = new UserRegistrationRequest
                {
                    Username = "testuser",
                    Email = "existing@example.com",
                    Password = "password123"
                };
                
                var existingUser = new User { Email = request.Email };
                
                _mockUserRepository.Setup(r => r.GetByEmailAsync(request.Email))
                    .ReturnsAsync(existingUser);
                
                // Act
                var result = await _userService.RegisterUserAsync(request);
                
                // Assert
                Assert.False(result.Success);
                Assert.Contains("User with this email already exists", result.Errors);
                
                // Verificar que no se creó el usuario
                _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
            }
            
            [Fact]
            public async Task RegisterUser_WithInvalidRequest_ShouldReturnValidationErrors()
            {
                // Arrange
                var request = new UserRegistrationRequest
                {
                    Username = "ab", // Muy corto
                    Email = "invalid-email", // Formato inválido
                    Password = "123" // Muy corto
                };
                
                // Act
                var result = await _userService.RegisterUserAsync(request);
                
                // Assert
                Assert.False(result.Success);
                Assert.Contains("Username must be at least 3 characters long", result.Errors);
                Assert.Contains("Invalid email format", result.Errors);
                Assert.Contains("Password must be at least 8 characters long", result.Errors);
                
                // Verificar que no se llamó al repositorio
                _mockUserRepository.Verify(r => r.GetByEmailAsync(It.IsAny<string>()), Times.Never);
            }
            
            [Fact]
            public async Task AuthenticateUser_WithValidCredentials_ShouldSucceed()
            {
                // Arrange
                var username = "test@example.com";
                var password = "password123";
                var user = new User
                {
                    Id = 1,
                    Email = username,
                    PasswordHash = "cGFzc3dvcmQxMjM=", // Base64 de "password123"
                    IsActive = true
                };
                
                _mockUserRepository.Setup(r => r.GetByEmailAsync(username))
                    .ReturnsAsync(user);
                
                _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>()))
                    .ReturnsAsync(true);
                
                _mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<User>(), It.IsAny<TimeSpan?>()))
                    .Returns(Task.CompletedTask);
                
                // Act
                var result = await _userService.AuthenticateUserAsync(username, password);
                
                // Assert
                Assert.True(result);
                
                // Verificar que se actualizó la fecha de último login
                _mockUserRepository.Verify(r => r.UpdateAsync(It.Is<User>(u => u.LastLoginDate.HasValue)), Times.Once);
            }
            
            [Fact]
            public async Task GetUserProfile_WithCachedUser_ShouldReturnFromCache()
            {
                // Arrange
                var userId = 1;
                var cachedUser = new User { Id = userId, Username = "cacheduser" };
                
                _mockCache.Setup(c => c.GetAsync<User>($"user:{userId}"))
                    .ReturnsAsync(cachedUser);
                
                // Act
                var result = await _userService.GetUserProfileAsync(userId);
                
                // Assert
                Assert.Equal(cachedUser, result);
                
                // Verificar que no se consultó la base de datos
                _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Never);
            }
            
            [Fact]
            public async Task GetUserProfile_WithoutCachedUser_ShouldFetchFromDatabase()
            {
                // Arrange
                var userId = 1;
                var dbUser = new User { Id = userId, Username = "dbuser" };
                
                _mockCache.Setup(c => c.GetAsync<User>($"user:{userId}"))
                    .ReturnsAsync((User)null);
                
                _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
                    .ReturnsAsync(dbUser);
                
                _mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<User>(), It.IsAny<TimeSpan?>()))
                    .Returns(Task.CompletedTask);
                
                // Act
                var result = await _userService.GetUserProfileAsync(userId);
                
                // Assert
                Assert.Equal(dbUser, result);
                
                // Verificar que se consultó la base de datos y se cacheó
                _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
                _mockCache.Verify(c => c.SetAsync($"user:{userId}", dbUser, It.IsAny<TimeSpan?>()), Times.Once);
            }
        }
    }
    
    // ===== TESTING CON NSUBSTITUTE =====
    namespace NSubstituteTests
    {
        public class UserServiceNSubstituteTests
        {
            private readonly IUserRepository _userRepository;
            private readonly IEmailService _emailService;
            private readonly ILogger _logger;
            private readonly IConfigurationService _config;
            private readonly ICacheService _cache;
            private readonly UserService _userService;
            
            public UserServiceNSubstituteTests()
            {
                _userRepository = Substitute.For<IUserRepository>();
                _emailService = Substitute.For<IEmailService>();
                _logger = Substitute.For<ILogger>();
                _config = Substitute.For<IConfigurationService>();
                _cache = Substitute.For<ICacheService>();
                
                _userService = new UserService(_userRepository, _emailService, _logger, _config, _cache);
            }
            
            [Fact]
            public async Task RegisterUser_WithValidRequest_ShouldSucceed()
            {
                // Arrange
                var request = new UserRegistrationRequest
                {
                    Username = "testuser",
                    Email = "test@example.com",
                    Password = "password123"
                };
                
                var expectedUser = new User
                {
                    Id = 1,
                    Username = request.Username,
                    Email = request.Email,
                    IsActive = true
                };
                
                _userRepository.GetByEmailAsync(request.Email).Returns((User)null);
                _userRepository.CreateAsync(Arg.Any<User>()).Returns(expectedUser);
                _emailService.SendWelcomeEmailAsync(request.Email, request.Username).Returns(true);
                _cache.SetAsync(Arg.Any<string>(), Arg.Any<User>(), Arg.Any<TimeSpan?>()).Returns(Task.CompletedTask);
                
                // Act
                var result = await _userService.RegisterUserAsync(request);
                
                // Assert
                Assert.True(result.Success);
                Assert.NotNull(result.User);
                Assert.Equal(request.Username, result.User.Username);
                
                // Verificar llamadas
                await _userRepository.Received(1).GetByEmailAsync(request.Email);
                await _userRepository.Received(1).CreateAsync(Arg.Any<User>());
                await _emailService.Received(1).SendWelcomeEmailAsync(request.Email, request.Username);
            }
            
            [Fact]
            public async Task AuthenticateUser_WithInvalidCredentials_ShouldFail()
            {
                // Arrange
                var username = "test@example.com";
                var password = "wrongpassword";
                
                _userRepository.GetByEmailAsync(username).Returns((User)null);
                
                // Act
                var result = await _userService.AuthenticateUserAsync(username, password);
                
                // Assert
                Assert.False(result);
                
                // Verificar que no se actualizó el usuario
                await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>());
            }
            
            [Fact]
            public async Task ResetPassword_WithValidEmail_ShouldSendResetEmail()
            {
                // Arrange
                var email = "test@example.com";
                var user = new User { Email = email, IsActive = true };
                
                _userRepository.GetByEmailAsync(email).Returns(user);
                _emailService.SendPasswordResetEmailAsync(email, Arg.Any<string>()).Returns(true);
                
                // Act
                var result = await _userService.ResetPasswordAsync(email);
                
                // Assert
                Assert.True(result);
                await _emailService.Received(1).SendPasswordResetEmailAsync(email, Arg.Any<string>());
            }
        }
    }
    
    // ===== TESTING DE COMPORTAMIENTOS COMPLEJOS =====
    namespace ComplexMockingTests
    {
        public class ComplexUserServiceTests
        {
            private readonly Mock<IUserRepository> _mockUserRepository;
            private readonly Mock<IEmailService> _mockEmailService;
            private readonly Mock<ILogger> _mockLogger;
            private readonly Mock<IConfigurationService> _mockConfig;
            private readonly Mock<ICacheService> _mockCache;
            private readonly UserService _userService;
            
            public ComplexUserServiceTests()
            {
                _mockUserRepository = new Mock<IUserRepository>();
                _mockEmailService = new Mock<IEmailService>();
                _mockLogger = new Mock<ILogger>();
                _mockConfig = new Mock<IConfigurationService>();
                _mockCache = new Mock<ICacheService>();
                
                _userService = new UserService(
                    _mockUserRepository.Object,
                    _mockEmailService.Object,
                    _mockLogger.Object,
                    _mockConfig.Object,
                    _mockCache.Object);
            }
            
            [Fact]
            public async Task RegisterUser_WithEmailServiceFailure_ShouldStillCreateUser()
            {
                // Arrange
                var request = new UserRegistrationRequest
                {
                    Username = "testuser",
                    Email = "test@example.com",
                    Password = "password123"
                };
                
                var expectedUser = new User
                {
                    Id = 1,
                    Username = request.Username,
                    Email = request.Email,
                    IsActive = true
                };
                
                _mockUserRepository.Setup(r => r.GetByEmailAsync(request.Email))
                    .ReturnsAsync((User)null);
                
                _mockUserRepository.Setup(r => r.CreateAsync(It.IsAny<User>()))
                    .ReturnsAsync(expectedUser);
                
                // Simular fallo en el servicio de email
                _mockEmailService.Setup(e => e.SendWelcomeEmailAsync(request.Email, request.Username))
                    .ReturnsAsync(false);
                
                _mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<User>(), It.IsAny<TimeSpan?>()))
                    .Returns(Task.CompletedTask);
                
                // Act
                var result = await _userService.RegisterUserAsync(request);
                
                // Assert
                Assert.True(result.Success);
                Assert.NotNull(result.User);
                
                // Verificar que se creó el usuario a pesar del fallo del email
                _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
                
                // Verificar que se registró el warning en el logger
                _mockLogger.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("Failed to send welcome email"))), Times.Once);
            }
            
            [Fact]
            public async Task RegisterUser_WithCacheFailure_ShouldStillWork()
            {
                // Arrange
                var request = new UserRegistrationRequest
                {
                    Username = "testuser",
                    Email = "test@example.com",
                    Password = "password123"
                };
                
                var expectedUser = new User
                {
                    Id = 1,
                    Username = request.Username,
                    Email = request.Email,
                    IsActive = true
                };
                
                _mockUserRepository.Setup(r => r.GetByEmailAsync(request.Email))
                    .ReturnsAsync((User)null);
                
                _mockUserRepository.Setup(r => r.CreateAsync(It.IsAny<User>()))
                    .ReturnsAsync(expectedUser);
                
                _mockEmailService.Setup(e => e.SendWelcomeEmailAsync(request.Email, request.Username))
                    .ReturnsAsync(true);
                
                // Simular fallo en el cache
                _mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<User>(), It.IsAny<TimeSpan?>()))
                    .ThrowsAsync(new Exception("Cache failure"));
                
                // Act
                var result = await _userService.RegisterUserAsync(request);
                
                // Assert
                Assert.True(result.Success);
                Assert.NotNull(result.User);
                
                // Verificar que se creó el usuario a pesar del fallo del cache
                _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
            }
            
            [Fact]
            public async Task GetUserProfile_WithDatabaseFailure_ShouldReturnNull()
            {
                // Arrange
                var userId = 1;
                
                _mockCache.Setup(c => c.GetAsync<User>($"user:{userId}"))
                    .ReturnsAsync((User)null);
                
                // Simular fallo en la base de datos
                _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
                    .ThrowsAsync(new Exception("Database connection failed"));
                
                // Act
                var result = await _userService.GetUserProfileAsync(userId);
                
                // Assert
                Assert.Null(result);
                
                // Verificar que se registró el error en el logger
                _mockLogger.Verify(l => l.LogError(It.Is<string>(s => s.Contains("Error getting user profile")), It.IsAny<Exception>()), Times.Once);
            }
        }
    }
}

// ===== DEMOSTRACIÓN DE FRAMEWORKS DE MOCKING =====
public class MockingFrameworksDemonstration
{
    public static async Task DemonstrateMockingFrameworks()
    {
        Console.WriteLine("=== Frameworks de Mocking - Clase 6 ===\n");
        
        Console.WriteLine("1. CREANDO MOCKS CON MOQ:");
        var mockUserRepository = new Moq.Mock<MockingFrameworks.Interfaces.IUserRepository>();
        var mockEmailService = new Moq.Mock<MockingFrameworks.Interfaces.IEmailService>();
        var mockLogger = new Moq.Mock<MockingFrameworks.Interfaces.ILogger>();
        var mockConfig = new Moq.Mock<MockingFrameworks.Interfaces.IConfigurationService>();
        var mockCache = new Moq.Mock<MockingFrameworks.Interfaces.ICacheService>();
        
        Console.WriteLine("✅ Mocks creados exitosamente");
        
        Console.WriteLine("\n2. CONFIGURANDO COMPORTAMIENTOS:");
        mockUserRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new MockingFrameworks.Models.User 
            { 
                Id = 1, 
                Username = "testuser", 
                Email = "test@example.com" 
            });
        
        mockEmailService.Setup(e => e.SendWelcomeEmailAsync("test@example.com", "testuser"))
            .ReturnsAsync(true);
        
        Console.WriteLine("✅ Comportamientos configurados");
        
        Console.WriteLine("\n3. CREANDO SERVICIO CON MOCKS:");
        var userService = new MockingFrameworks.Services.UserService(
            mockUserRepository.Object,
            mockEmailService.Object,
            mockLogger.Object,
            mockConfig.Object,
            mockCache.Object);
        
        Console.WriteLine("✅ Servicio creado con mocks");
        
        Console.WriteLine("\n4. PROBANDO FUNCIONALIDAD:");
        var request = new MockingFrameworks.Models.UserRegistrationRequest
        {
            Username = "newuser",
            Email = "new@example.com",
            Password = "password123"
        };
        
        var result = await userService.RegisterUserAsync(request);
        Console.WriteLine($"✅ Resultado del registro: {result.Success}");
        
        Console.WriteLine("\n5. VERIFICANDO LLAMADAS A MOCKS:");
        mockUserRepository.Verify(r => r.GetByEmailAsync("new@example.com"), Moq.Times.Once);
        mockEmailService.Verify(e => e.SendWelcomeEmailAsync("new@example.com", "newuser"), Moq.Times.Once);
        
        Console.WriteLine("✅ Llamadas verificadas correctamente");
        
        Console.WriteLine("\n✅ Frameworks de Mocking demostrados!");
        Console.WriteLine("Los mocks permiten testing aislado y control total sobre dependencias externas.");
    }
}

// Programa principal
public class Program
{
    public static async Task Main()
    {
        await MockingFrameworksDemonstration.DemonstrateMockingFrameworks();
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Mocking de Servicios Externos
Implementa mocks para:
- APIs de terceros
- Servicios de base de datos
- Sistemas de archivos

### Ejercicio 2: Testing de Casos de Error
Crea mocks que simulen:
- Fallos de red
- Timeouts
- Excepciones específicas

### Ejercicio 3: Verificación de Comportamiento
Implementa verificaciones para:
- Orden de llamadas
- Parámetros específicos
- Número de invocaciones

## 🔍 Puntos Clave

1. **Mocking** permite testing aislado y controlado
2. **Moq** es el framework más popular para C#
3. **NSubstitute** ofrece sintaxis más fluida
4. **Setup** configura el comportamiento esperado
5. **Verify** confirma que se llamaron los métodos
6. **Returns** especifica valores de retorno
7. **Throws** simula excepciones
8. **Arg** permite verificación de parámetros

## 📚 Recursos Adicionales

- [Moq Documentation](https://github.com/moq/moq4)
- [NSubstitute Documentation](https://nsubstitute.github.io/)
- [Mocking Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

---

**🎯 ¡Has completado la Clase 6! Ahora comprendes los Frameworks de Mocking**

**📚 [Siguiente: Clase 7 - Testing de Código Asíncrono](clase_7_testing_asincrono.md)**
