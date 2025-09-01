# 🚀 Clase 9: Seguridad Avanzada en .NET

## 📋 Información de la Clase

- **Módulo**: Mid Level 2 - Arquitectura de Software y Patrones Avanzados
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 8 (High Performance Programming)

## 🎯 Objetivos de Aprendizaje

- Implementar autenticación y autorización seguras
- Gestionar identidades y claims de manera avanzada
- Aplicar técnicas de encriptación y hashing
- Implementar auditoría y logging de seguridad

---

## 📚 Navegación del Módulo 5

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_arquitectura_hexagonal.md) | Arquitectura Hexagonal (Ports & Adapters) | |
| [Clase 2](clase_2_event_sourcing.md) | Event Sourcing y CQRS Avanzado | |
| [Clase 3](clase_3_microservicios.md) | Arquitectura de Microservicios | |
| [Clase 4](clase_4_patrones_arquitectonicos.md) | Patrones Arquitectónicos | |
| [Clase 5](clase_5_domain_driven_design.md) | Domain Driven Design (DDD) | |
| [Clase 6](clase_6_async_streams.md) | Async Streams y IAsyncEnumerable | |
| [Clase 7](clase_7_source_generators.md) | Source Generators y Compile-time Code Generation | |
| [Clase 8](clase_8_high_performance.md) | High Performance Programming | ← Anterior |
| **Clase 9** | **Seguridad Avanzada en .NET** | ← Estás aquí |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Gestión Empresarial | |

**← [Volver al README del Módulo 5](../midLevel_2/README.md)**

---

## 📚 Contenido Teórico

### 1. Seguridad Avanzada en .NET

La seguridad en aplicaciones .NET es fundamental para proteger datos sensibles y mantener la integridad del sistema.

```csharp
// ===== SEGURIDAD AVANZADA EN .NET - IMPLEMENTACIÓN COMPLETA =====
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AdvancedSecurity
{
    // ===== AUTENTICACIÓN AVANZADA =====
    public class AdvancedAuthenticationService
    {
        private readonly ILogger<AdvancedAuthenticationService> _logger;
        private readonly IConfiguration _configuration;
        
        public AdvancedAuthenticationService(ILogger<AdvancedAuthenticationService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        
        public async Task<AuthenticationResult> AuthenticateUserAsync(string username, string password)
        {
            try
            {
                // Validar credenciales
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    _logger.LogWarning("Intento de autenticación con credenciales vacías");
                    return AuthenticationResult.Failed("Credenciales inválidas");
                }
                
                // Verificar usuario en base de datos
                var user = await GetUserByUsernameAsync(username);
                if (user == null)
                {
                    _logger.LogWarning("Usuario no encontrado: {Username}", username);
                    return AuthenticationResult.Failed("Usuario no encontrado");
                }
                
                // Verificar contraseña
                if (!VerifyPassword(password, user.PasswordHash))
                {
                    _logger.LogWarning("Contraseña incorrecta para usuario: {Username}", username);
                    return AuthenticationResult.Failed("Contraseña incorrecta");
                }
                
                // Verificar si la cuenta está bloqueada
                if (user.IsLocked)
                {
                    _logger.LogWarning("Intento de acceso a cuenta bloqueada: {Username}", username);
                    return AuthenticationResult.Failed("Cuenta bloqueada");
                }
                
                // Verificar si la contraseña ha expirado
                if (user.PasswordExpiresAt.HasValue && user.PasswordExpiresAt.Value < DateTime.UtcNow)
                {
                    _logger.LogWarning("Contraseña expirada para usuario: {Username}", username);
                    return AuthenticationResult.Failed("Contraseña expirada");
                }
                
                // Crear claims de identidad
                var claims = CreateUserClaims(user);
                
                // Registrar acceso exitoso
                _logger.LogInformation("Autenticación exitosa para usuario: {Username}", username);
                
                return AuthenticationResult.Success(claims);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la autenticación para usuario: {Username}", username);
                return AuthenticationResult.Failed("Error interno del sistema");
            }
        }
        
        private async Task<User> GetUserByUsernameAsync(string username)
        {
            // Simulación de acceso a base de datos
            await Task.Delay(100);
            
            // Usuario de ejemplo
            return new User
            {
                Id = 1,
                Username = username,
                Email = $"{username}@example.com",
                PasswordHash = HashPassword("password123"),
                IsLocked = false,
                PasswordExpiresAt = DateTime.UtcNow.AddDays(90),
                Roles = new[] { "User" },
                Permissions = new[] { "read", "write" }
            };
        }
        
        private bool VerifyPassword(string password, string hash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == hash;
        }
        
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
        
        private IEnumerable<Claim> CreateUserClaims(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };
            
            // Agregar roles
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            
            // Agregar permisos
            foreach (var permission in user.Permissions)
            {
                claims.Add(new Claim("Permission", permission));
            }
            
            return claims;
        }
    }
    
    // ===== AUTORIZACIÓN AVANZADA =====
    public class AdvancedAuthorizationService
    {
        private readonly ILogger<AdvancedAuthorizationService> _logger;
        
        public AdvancedAuthorizationService(ILogger<AdvancedAuthorizationService> logger)
        {
            _logger = logger;
        }
        
        public async Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, string resource, string action)
        {
            try
            {
                if (user == null || !user.Identity.IsAuthenticated)
                {
                    _logger.LogWarning("Intento de autorización sin autenticación");
                    return AuthorizationResult.Failed();
                }
                
                // Verificar permisos específicos
                var hasPermission = await CheckPermissionAsync(user, resource, action);
                if (!hasPermission)
                {
                    _logger.LogWarning("Usuario {Username} no tiene permiso para {Action} en {Resource}", 
                        user.Identity.Name, action, resource);
                    return AuthorizationResult.Failed();
                }
                
                // Verificar roles
                var hasRole = await CheckRoleAsync(user, resource, action);
                if (!hasRole)
                {
                    _logger.LogWarning("Usuario {Username} no tiene rol requerido para {Action} en {Resource}", 
                        user.Identity.Name, action, resource);
                    return AuthorizationResult.Failed();
                }
                
                _logger.LogInformation("Autorización exitosa para usuario {Username} en {Resource}", 
                    user.Identity.Name, resource);
                
                return AuthorizationResult.Succeeded();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la autorización para usuario {Username}", user?.Identity?.Name);
                return AuthorizationResult.Failed();
            }
        }
        
        private async Task<bool> CheckPermissionAsync(ClaimsPrincipal user, string resource, string action)
        {
            await Task.Delay(50);
            
            var requiredPermission = $"{resource}:{action}";
            var userPermissions = user.FindAll("Permission").Select(c => c.Value);
            
            return userPermissions.Contains(requiredPermission) || userPermissions.Contains("admin");
        }
        
        private async Task<bool> CheckRoleAsync(ClaimsPrincipal user, string resource, string action)
        {
            await Task.Delay(50);
            
            var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
            
            // Lógica de autorización basada en roles
            if (action == "read" && userRoles.Contains("User"))
                return true;
                
            if (action == "write" && userRoles.Contains("Editor"))
                return true;
                
            if (action == "delete" && userRoles.Contains("Admin"))
                return true;
                
            return false;
        }
    }
    
    // ===== ENCRIPTACIÓN Y HASHING =====
    public class AdvancedCryptographyService
    {
        private readonly ILogger<AdvancedCryptographyService> _logger;
        private readonly string _encryptionKey;
        
        public AdvancedCryptographyService(ILogger<AdvancedCryptographyService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _encryptionKey = configuration["Encryption:Key"] ?? "DefaultKey123!@#";
        }
        
        public string EncryptString(string plainText)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32, '0').Substring(0, 32));
                aes.IV = new byte[16];
                
                using var encryptor = aes.CreateEncryptor();
                using var msEncrypt = new MemoryStream();
                using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                using var swEncrypt = new StreamWriter(csEncrypt);
                
                swEncrypt.Write(plainText);
                swEncrypt.Flush();
                csEncrypt.FlushFinalBlock();
                
                var encrypted = msEncrypt.ToArray();
                return Convert.ToBase64String(encrypted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al encriptar texto");
                throw;
            }
        }
        
        public string DecryptString(string cipherText)
        {
            try
            {
                var cipherBytes = Convert.FromBase64String(cipherText);
                
                using var aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32, '0').Substring(0, 32));
                aes.IV = new byte[16];
                
                using var decryptor = aes.CreateDecryptor();
                using var msDecrypt = new MemoryStream(cipherBytes);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);
                
                return srDecrypt.ReadToEnd();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desencriptar texto");
                throw;
            }
        }
        
        public string HashWithSalt(string password, string salt = null)
        {
            try
            {
                if (string.IsNullOrEmpty(salt))
                {
                    salt = GenerateSalt();
                }
                
                using var pbkdf2 = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt), 10000, HashAlgorithmName.SHA256);
                var hash = pbkdf2.GetBytes(32);
                
                return $"{salt}:{Convert.ToBase64String(hash)}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar hash");
                throw;
            }
        }
        
        public bool VerifyHash(string password, string hashWithSalt)
        {
            try
            {
                var parts = hashWithSalt.Split(':');
                if (parts.Length != 2)
                    return false;
                
                var salt = parts[0];
                var hash = parts[1];
                
                var computedHash = HashWithSalt(password, salt);
                var computedParts = computedHash.Split(':');
                
                return computedParts[1] == hash;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar hash");
                return false;
            }
        }
        
        private string GenerateSalt()
        {
            var salt = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return Convert.ToBase64String(salt);
        }
        
        public string GenerateSecureToken()
        {
            var tokenBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(tokenBytes);
            return Convert.ToBase64String(tokenBytes);
        }
    }
    
    // ===== GESTIÓN DE IDENTIDADES =====
    public class IdentityManagementService
    {
        private readonly ILogger<IdentityManagementService> _logger;
        
        public IdentityManagementService(ILogger<IdentityManagementService> logger)
        {
            _logger = logger;
        }
        
        public async Task<IdentityResult> CreateUserAsync(CreateUserRequest request)
        {
            try
            {
                // Validar datos de entrada
                if (!ValidateUserRequest(request))
                {
                    return IdentityResult.Failed("Datos de usuario inválidos");
                }
                
                // Verificar si el usuario ya existe
                if (await UserExistsAsync(request.Username, request.Email))
                {
                    return IdentityResult.Failed("Usuario o email ya existe");
                }
                
                // Crear usuario
                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = HashPassword(request.Password),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Roles = new[] { "User" },
                    Permissions = new[] { "read" }
                };
                
                // Guardar usuario (simulado)
                await SaveUserAsync(user);
                
                _logger.LogInformation("Usuario creado exitosamente: {Username}", request.Username);
                
                return IdentityResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario: {Username}", request.Username);
                return IdentityResult.Failed("Error interno del sistema");
            }
        }
        
        public async Task<IdentityResult> UpdateUserAsync(int userId, UpdateUserRequest request)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null)
                {
                    return IdentityResult.Failed("Usuario no encontrado");
                }
                
                // Actualizar campos permitidos
                if (!string.IsNullOrEmpty(request.Email))
                    user.Email = request.Email;
                    
                if (!string.IsNullOrEmpty(request.Username))
                    user.Username = request.Username;
                
                // Guardar cambios
                await SaveUserAsync(user);
                
                _logger.LogInformation("Usuario actualizado exitosamente: {UserId}", userId);
                
                return IdentityResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario: {UserId}", userId);
                return IdentityResult.Failed("Error interno del sistema");
            }
        }
        
        public async Task<IdentityResult> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null)
                {
                    return IdentityResult.Failed("Usuario no encontrado");
                }
                
                // Verificar contraseña actual
                if (!VerifyPassword(currentPassword, user.PasswordHash))
                {
                    return IdentityResult.Failed("Contraseña actual incorrecta");
                }
                
                // Validar nueva contraseña
                if (!ValidatePassword(newPassword))
                {
                    return IdentityResult.Failed("Nueva contraseña no cumple los requisitos");
                }
                
                // Cambiar contraseña
                user.PasswordHash = HashPassword(newPassword);
                user.PasswordChangedAt = DateTime.UtcNow;
                
                await SaveUserAsync(user);
                
                _logger.LogInformation("Contraseña cambiada exitosamente para usuario: {UserId}", userId);
                
                return IdentityResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña para usuario: {UserId}", userId);
                return IdentityResult.Failed("Error interno del sistema");
            }
        }
        
        private bool ValidateUserRequest(CreateUserRequest request)
        {
            return !string.IsNullOrEmpty(request.Username) &&
                   !string.IsNullOrEmpty(request.Email) &&
                   !string.IsNullOrEmpty(request.Password) &&
                   request.Password.Length >= 8;
        }
        
        private bool ValidatePassword(string password)
        {
            return password.Length >= 8 &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsLower) &&
                   password.Any(char.IsDigit) &&
                   password.Any(c => !char.IsLetterOrDigit(c));
        }
        
        private async Task<bool> UserExistsAsync(string username, string email)
        {
            await Task.Delay(50);
            return false; // Simulado
        }
        
        private async Task<User> GetUserByIdAsync(int userId)
        {
            await Task.Delay(50);
            return new User { Id = userId, Username = "user" + userId };
        }
        
        private async Task SaveUserAsync(User user)
        {
            await Task.Delay(100);
            // Simulado
        }
        
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
        
        private bool VerifyPassword(string password, string hash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == hash;
        }
    }
    
    // ===== AUDITORÍA Y LOGGING =====
    public class SecurityAuditService
    {
        private readonly ILogger<SecurityAuditService> _logger;
        private readonly List<SecurityEvent> _securityEvents;
        
        public SecurityAuditService(ILogger<SecurityAuditService> logger)
        {
            _logger = logger;
            _securityEvents = new List<SecurityEvent>();
        }
        
        public void LogSecurityEvent(SecurityEvent securityEvent)
        {
            try
            {
                securityEvent.Timestamp = DateTime.UtcNow;
                securityEvent.Id = Guid.NewGuid();
                
                _securityEvents.Add(securityEvent);
                
                // Log estructurado
                _logger.LogInformation("Evento de seguridad: {EventType} - Usuario: {Username} - IP: {IpAddress} - Resultado: {Result}",
                    securityEvent.EventType,
                    securityEvent.Username,
                    securityEvent.IpAddress,
                    securityEvent.Result);
                
                // Almacenar en base de datos (simulado)
                StoreSecurityEventAsync(securityEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar evento de seguridad");
            }
        }
        
        public void LogAuthenticationAttempt(string username, string ipAddress, bool success, string reason = null)
        {
            var eventType = success ? SecurityEventType.AuthenticationSuccess : SecurityEventType.AuthenticationFailure;
            
            var securityEvent = new SecurityEvent
            {
                EventType = eventType,
                Username = username,
                IpAddress = ipAddress,
                Result = success ? "Success" : "Failure",
                Details = reason ?? (success ? "Autenticación exitosa" : "Autenticación fallida"),
                UserAgent = "Unknown", // Obtener del contexto HTTP
                SessionId = Guid.NewGuid().ToString()
            };
            
            LogSecurityEvent(securityEvent);
        }
        
        public void LogAuthorizationAttempt(string username, string resource, string action, bool success, string reason = null)
        {
            var eventType = success ? SecurityEventType.AuthorizationSuccess : SecurityEventType.AuthorizationFailure;
            
            var securityEvent = new SecurityEvent
            {
                EventType = eventType,
                Username = username,
                Resource = resource,
                Action = action,
                Result = success ? "Success" : "Failure",
                Details = reason ?? (success ? "Autorización exitosa" : "Autorización denegada"),
                IpAddress = "Unknown",
                UserAgent = "Unknown",
                SessionId = Guid.NewGuid().ToString()
            };
            
            LogSecurityEvent(securityEvent);
        }
        
        public void LogDataAccess(string username, string resource, string action, bool success, string details = null)
        {
            var eventType = SecurityEventType.DataAccess;
            
            var securityEvent = new SecurityEvent
            {
                EventType = eventType,
                Username = username,
                Resource = resource,
                Action = action,
                Result = success ? "Success" : "Failure",
                Details = details ?? "Acceso a datos",
                IpAddress = "Unknown",
                UserAgent = "Unknown",
                SessionId = Guid.NewGuid().ToString()
            };
            
            LogSecurityEvent(securityEvent);
        }
        
        public async Task<IEnumerable<SecurityEvent>> GetSecurityEventsAsync(DateTime from, DateTime to, string username = null)
        {
            await Task.Delay(100);
            
            var events = _securityEvents.Where(e => e.Timestamp >= from && e.Timestamp <= to);
            
            if (!string.IsNullOrEmpty(username))
            {
                events = events.Where(e => e.Username == username);
            }
            
            return events.OrderByDescending(e => e.Timestamp);
        }
        
        public async Task<SecurityReport> GenerateSecurityReportAsync(DateTime from, DateTime to)
        {
            var events = await GetSecurityEventsAsync(from, to);
            
            var report = new SecurityReport
            {
                PeriodFrom = from,
                PeriodTo = to,
                TotalEvents = events.Count(),
                AuthenticationFailures = events.Count(e => e.EventType == SecurityEventType.AuthenticationFailure),
                AuthorizationFailures = events.Count(e => e.EventType == SecurityEventType.AuthorizationFailure),
                DataAccessEvents = events.Count(e => e.EventType == SecurityEventType.DataAccess),
                TopUsers = events.GroupBy(e => e.Username)
                    .Select(g => new UserActivity { Username = g.Key, EventCount = g.Count() })
                    .OrderByDescending(u => u.EventCount)
                    .Take(10)
                    .ToList()
            };
            
            return report;
        }
        
        private async Task StoreSecurityEventAsync(SecurityEvent securityEvent)
        {
            await Task.Delay(50);
            // Simulado - almacenar en base de datos
        }
    }
    
    // ===== MODELOS DE DATOS =====
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PasswordExpiresAt { get; set; }
        public DateTime? PasswordChangedAt { get; set; }
        public string[] Roles { get; set; }
        public string[] Permissions { get; set; }
    }
    
    public class CreateUserRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
    
    public class UpdateUserRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
    }
    
    public class AuthenticationResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public IEnumerable<Claim> Claims { get; set; }
        
        public static AuthenticationResult Success(IEnumerable<Claim> claims)
        {
            return new AuthenticationResult { IsSuccess = true, Claims = claims };
        }
        
        public static AuthenticationResult Failed(string errorMessage)
        {
            return new AuthenticationResult { IsSuccess = false, ErrorMessage = errorMessage };
        }
    }
    
    public class AuthorizationResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        
        public static AuthorizationResult Success()
        {
            return new AuthorizationResult { IsSuccess = true };
        }
        
        public static AuthorizationResult Failed(string errorMessage = null)
        {
            return new AuthorizationResult { IsSuccess = false, ErrorMessage = errorMessage };
        }
    }
    
    public class IdentityResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        
        public static IdentityResult Success()
        {
            return new IdentityResult { IsSuccess = true };
        }
        
        public static IdentityResult Failed(string errorMessage)
        {
            return new IdentityResult { IsSuccess = false, ErrorMessage = errorMessage };
        }
    }
    
    public class SecurityEvent
    {
        public Guid Id { get; set; }
        public SecurityEventType EventType { get; set; }
        public string Username { get; set; }
        public string Resource { get; set; }
        public string Action { get; set; }
        public string Result { get; set; }
        public string Details { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string SessionId { get; set; }
        public DateTime Timestamp { get; set; }
    }
    
    public enum SecurityEventType
    {
        AuthenticationSuccess,
        AuthenticationFailure,
        AuthorizationSuccess,
        AuthorizationFailure,
        DataAccess,
        PasswordChange,
        AccountLockout,
        AccountUnlock
    }
    
    public class SecurityReport
    {
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public int TotalEvents { get; set; }
        public int AuthenticationFailures { get; set; }
        public int AuthorizationFailures { get; set; }
        public int DataAccessEvents { get; set; }
        public List<UserActivity> TopUsers { get; set; }
    }
    
    public class UserActivity
    {
        public string Username { get; set; }
        public int EventCount { get; set; }
    }
}

// Uso de Seguridad Avanzada en .NET
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Seguridad Avanzada en .NET ===\n");
        
        Console.WriteLine("Los componentes de seguridad incluyen:");
        Console.WriteLine("1. Autenticación avanzada con validaciones");
        Console.WriteLine("2. Autorización basada en roles y permisos");
        Console.WriteLine("3. Encriptación y hashing seguro");
        Console.WriteLine("4. Gestión de identidades");
        Console.WriteLine("5. Auditoría y logging de seguridad");
        
        Console.WriteLine("\nEjemplos implementados:");
        Console.WriteLine("- Servicio de autenticación con validaciones");
        Console.WriteLine("- Servicio de autorización con permisos");
        Console.WriteLine("- Servicio de criptografía con AES");
        Console.WriteLine("- Gestión de usuarios y contraseñas");
        Console.WriteLine("- Auditoría de eventos de seguridad");
        Console.WriteLine("- Generación de reportes de seguridad");
        
        // Ejemplo de uso
        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<Program>();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
        
        var authService = new AdvancedSecurity.AdvancedAuthenticationService(logger, configuration);
        var authResult = await authService.AuthenticateUserAsync("admin", "password123");
        
        if (authResult.IsSuccess)
        {
            Console.WriteLine("Autenticación exitosa");
        }
        else
        {
            Console.WriteLine($"Error de autenticación: {authResult.ErrorMessage}");
        }
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Sistema de Autenticación Multi-Factor
Implementa un sistema de autenticación que incluya SMS, email y aplicaciones autenticadoras.

### Ejercicio 2: Gestión de Permisos Granulares
Crea un sistema de permisos que permita control fino sobre recursos y acciones.

### Ejercicio 3: Auditoría de Seguridad en Tiempo Real
Implementa un sistema de monitoreo que detecte actividades sospechosas.

## 🔍 Puntos Clave

1. **Autenticación** incluye validaciones múltiples y bloqueo de cuentas
2. **Autorización** se basa en roles, permisos y recursos específicos
3. **Encriptación** usa algoritmos estándar como AES y PBKDF2
4. **Auditoría** registra todos los eventos de seguridad
5. **Logging** estructurado facilita el análisis de seguridad

## 📚 Recursos Adicionales

- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [Identity Framework](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [Authorization in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/)

---

**🎯 ¡Has completado la Clase 9! Ahora comprendes Seguridad Avanzada en .NET**

**📚 [Siguiente: Clase 10 - Proyecto Final: Sistema de Gestión Empresarial](clase_10_proyecto_final.md)**
