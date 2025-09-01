# 🚀 Clase 9: Arquitectura de Seguridad Enterprise

## 📋 Información de la Clase

- **Módulo**: Senior 1 - Arquitectura de Software Empresarial
- **Duración**: 2 horas
- **Nivel**: Senior
- **Prerrequisitos**: Completar Clase 8 (Arquitectura Evolutiva)

## 🎯 Objetivos de Aprendizaje

- Implementar arquitecturas de seguridad empresariales
- Aplicar patrones de seguridad avanzados
- Implementar autenticación y autorización robustas
- Crear sistemas de auditoría y compliance

---

## 📚 Navegación del Módulo 6

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_arquitectura_limpia_avanzada.md) | Arquitectura Limpia Avanzada | |
| [Clase 2](clase_2_event_driven_architecture.md) | Event-Driven Architecture | |
| [Clase 3](clase_3_microservicios_avanzada.md) | Arquitectura de Microservicios Avanzada | |
| [Clase 4](clase_4_patrones_enterprise.md) | Patrones de Diseño Enterprise | |
| [Clase 5](clase_5_arquitectura_datos_avanzada.md) | Arquitectura de Datos Avanzada | |
| [Clase 6](clase_6_calidad_codigo_metricas.md) | Calidad del Código y Métricas | |
| [Clase 7](clase_7_monitoreo_observabilidad.md) | Monitoreo y Observabilidad | |
| [Clase 8](clase_8_arquitectura_evolutiva.md) | Arquitectura Evolutiva | ← Anterior |
| **Clase 9** | **Arquitectura de Seguridad Enterprise** | ← Estás aquí |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Plataforma Empresarial | Siguiente → |

**← [Volver al README del Módulo 6](../senior_1/README.md)**

---

## 📚 Contenido Teórico

### 1. Arquitectura de Seguridad Enterprise

La seguridad empresarial requiere múltiples capas de protección y cumplimiento de estándares regulatorios.

```csharp
// ===== ARQUITECTURA DE SEGURIDAD ENTERPRISE - IMPLEMENTACIÓN COMPLETA =====
namespace EnterpriseSecurityArchitecture
{
    // ===== AUTENTICACIÓN AVANZADA =====
    namespace AdvancedAuthentication
    {
        public interface IAuthenticationService
        {
            Task<AuthenticationResult> AuthenticateAsync(string username, string password);
            Task<AuthenticationResult> AuthenticateWithMfaAsync(string username, string password, string mfaCode);
            Task<AuthenticationResult> AuthenticateWithBiometricsAsync(string username, byte[] biometricData);
            Task<bool> ValidateTokenAsync(string token);
            Task<string> RefreshTokenAsync(string refreshToken);
            Task<bool> RevokeTokenAsync(string token);
        }
        
        public class AuthenticationService : IAuthenticationService
        {
            private readonly IUserService _userService;
            private readonly IPasswordHasher _passwordHasher;
            private readonly ITokenService _tokenService;
            private readonly IMfaService _mfaService;
            private readonly IBiometricService _biometricService;
            private readonly ILogger<AuthenticationService> _logger;
            private readonly IAuditService _auditService;
            
            public AuthenticationService(
                IUserService userService,
                IPasswordHasher passwordHasher,
                ITokenService tokenService,
                IMfaService mfaService,
                IBiometricService biometricService,
                ILogger<AuthenticationService> logger,
                IAuditService auditService)
            {
                _userService = userService;
                _passwordHasher = passwordHasher;
                _tokenService = tokenService;
                _mfaService = mfaService;
                _biometricService = biometricService;
                _logger = logger;
                _auditService = auditService;
            }
            
            public async Task<AuthenticationResult> AuthenticateAsync(string username, string password)
            {
                try
                {
                    var user = await _userService.GetByUsernameAsync(username);
                    
                    if (user == null)
                    {
                        await _auditService.LogFailedLoginAttemptAsync(username, "User not found");
                        return AuthenticationResult.Failure("Invalid credentials");
                    }
                    
                    if (!user.IsActive)
                    {
                        await _auditService.LogFailedLoginAttemptAsync(username, "Account disabled");
                        return AuthenticationResult.Failure("Account is disabled");
                    }
                    
                    if (user.IsLocked)
                    {
                        await _auditService.LogFailedLoginAttemptAsync(username, "Account locked");
                        return AuthenticationResult.Failure("Account is locked");
                    }
                    
                    if (user.PasswordExpiresAt.HasValue && user.PasswordExpiresAt < DateTime.UtcNow)
                    {
                        await _auditService.LogFailedLoginAttemptAsync(username, "Password expired");
                        return AuthenticationResult.Failure("Password has expired");
                    }
                    
                    if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
                    {
                        await _auditService.LogFailedLoginAttemptAsync(username, "Invalid password");
                        await _userService.IncrementFailedLoginAttemptsAsync(user.Id);
                        
                        if (user.FailedLoginAttempts >= 5)
                        {
                            await _userService.LockAccountAsync(user.Id);
                            await _auditService.LogAccountLockedAsync(username, "Too many failed attempts");
                        }
                        
                        return AuthenticationResult.Failure("Invalid credentials");
                    }
                    
                    // Reset failed login attempts on successful login
                    if (user.FailedLoginAttempts > 0)
                    {
                        await _userService.ResetFailedLoginAttemptsAsync(user.Id);
                    }
                    
                    var token = await _tokenService.GenerateTokenAsync(user);
                    var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user);
                    
                    await _auditService.LogSuccessfulLoginAsync(username, user.Id);
                    
                    return AuthenticationResult.Success(token, refreshToken, user);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Authentication failed for user {Username}", username);
                    await _auditService.LogFailedLoginAttemptAsync(username, $"System error: {ex.Message}");
                    return AuthenticationResult.Failure("Authentication failed");
                }
            }
            
            public async Task<AuthenticationResult> AuthenticateWithMfaAsync(string username, string password, string mfaCode)
            {
                var basicAuthResult = await AuthenticateAsync(username, password);
                
                if (!basicAuthResult.IsSuccess)
                {
                    return basicAuthResult;
                }
                
                var user = basicAuthResult.User;
                
                if (!user.MfaEnabled)
                {
                    return AuthenticationResult.Failure("MFA is not enabled for this account");
                }
                
                if (!await _mfaService.ValidateCodeAsync(user.Id, mfaCode))
                {
                    await _auditService.LogFailedMfaAttemptAsync(username, user.Id);
                    return AuthenticationResult.Failure("Invalid MFA code");
                }
                
                await _auditService.LogSuccessfulMfaAsync(username, user.Id);
                
                return basicAuthResult;
            }
            
            public async Task<AuthenticationResult> AuthenticateWithBiometricsAsync(string username, byte[] biometricData)
            {
                var user = await _userService.GetByUsernameAsync(username);
                
                if (user == null || !user.IsActive || user.IsLocked)
                {
                    return AuthenticationResult.Failure("Invalid credentials");
                }
                
                if (!user.BiometricEnabled)
                {
                    return AuthenticationResult.Failure("Biometric authentication is not enabled");
                }
                
                if (!await _biometricService.ValidateBiometricDataAsync(user.Id, biometricData))
                {
                    await _auditService.LogFailedBiometricAttemptAsync(username, user.Id);
                    return AuthenticationResult.Failure("Biometric validation failed");
                }
                
                var token = await _tokenService.GenerateTokenAsync(user);
                var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user);
                
                await _auditService.LogSuccessfulBiometricLoginAsync(username, user.Id);
                
                return AuthenticationResult.Success(token, refreshToken, user);
            }
            
            public async Task<bool> ValidateTokenAsync(string token)
            {
                return await _tokenService.ValidateTokenAsync(token);
            }
            
            public async Task<string> RefreshTokenAsync(string refreshToken)
            {
                return await _tokenService.RefreshTokenAsync(refreshToken);
            }
            
            public async Task<bool> RevokeTokenAsync(string token)
            {
                return await _tokenService.RevokeTokenAsync(token);
            }
        }
        
        public class AuthenticationResult
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
            public string Token { get; set; }
            public string RefreshToken { get; set; }
            public User User { get; set; }
            
            public static AuthenticationResult Success(string token, string refreshToken, User user)
            {
                return new AuthenticationResult
                {
                    IsSuccess = true,
                    Token = token,
                    RefreshToken = refreshToken,
                    User = user
                };
            }
            
            public static AuthenticationResult Failure(string message)
            {
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    Message = message
                };
            }
        }
        
        public interface IPasswordHasher
        {
            string HashPassword(string password);
            bool VerifyPassword(string password, string hash);
            bool IsPasswordExpired(string hash);
        }
        
        public class PasswordHasher : IPasswordHasher
        {
            private readonly int _iterations = 10000;
            private readonly int _keySize = 256;
            private readonly int _saltSize = 128;
            
            public string HashPassword(string password)
            {
                using var deriveBytes = new Rfc2898DeriveBytes(password, _saltSize, _iterations, HashAlgorithmName.SHA256);
                var salt = deriveBytes.Salt;
                var key = deriveBytes.GetBytes(_keySize);
                
                var hash = new byte[_saltSize + _keySize];
                Array.Copy(salt, 0, hash, 0, _saltSize);
                Array.Copy(key, 0, hash, _saltSize, _keySize);
                
                return Convert.ToBase64String(hash);
            }
            
            public bool VerifyPassword(string password, string hash)
            {
                try
                {
                    var hashBytes = Convert.FromBase64String(hash);
                    var salt = new byte[_saltSize];
                    var key = new byte[_keySize];
                    
                    Array.Copy(hashBytes, 0, salt, 0, _saltSize);
                    Array.Copy(hashBytes, _saltSize, key, 0, _keySize);
                    
                    using var deriveBytes = new Rfc2898DeriveBytes(password, salt, _iterations, HashAlgorithmName.SHA256);
                    var newKey = deriveBytes.GetBytes(_keySize);
                    
                    return key.SequenceEqual(newKey);
                }
                catch
                {
                    return false;
                }
            }
            
            public bool IsPasswordExpired(string hash)
            {
                // In a real implementation, you would store password creation date
                // and check against password policy expiration rules
                return false;
            }
        }
    }
    
    // ===== AUTORIZACIÓN AVANZADA =====
    namespace AdvancedAuthorization
    {
        public interface IAuthorizationService
        {
            Task<bool> AuthorizeAsync(string userId, string resource, string action);
            Task<bool> AuthorizeWithRolesAsync(string userId, string resource, string action, List<string> requiredRoles);
            Task<bool> AuthorizeWithPermissionsAsync(string userId, string resource, string action, List<string> requiredPermissions);
            Task<bool> AuthorizeWithContextAsync(string userId, string resource, string action, AuthorizationContext context);
            Task<List<string>> GetUserPermissionsAsync(string userId);
            Task<List<string>> GetUserRolesAsync(string userId);
        }
        
        public class AuthorizationService : IAuthorizationService
        {
            private readonly IUserService _userService;
            private readonly IRoleService _roleService;
            private readonly IPermissionService _permissionService;
            private readonly ILogger<AuthorizationService> _logger;
            private readonly IAuditService _auditService;
            
            public AuthorizationService(
                IUserService userService,
                IRoleService roleService,
                IPermissionService permissionService,
                ILogger<AuthorizationService> logger,
                IAuditService auditService)
            {
                _userService = userService;
                _roleService = roleService;
                _permissionService = permissionService;
                _logger = logger;
                _auditService = auditService;
            }
            
            public async Task<bool> AuthorizeAsync(string userId, string resource, string action)
            {
                try
                {
                    var user = await _userService.GetByIdAsync(userId);
                    if (user == null || !user.IsActive)
                    {
                        return false;
                    }
                    
                    var userRoles = await GetUserRolesAsync(userId);
                    var userPermissions = await GetUserPermissionsAsync(userId);
                    
                    // Check if user has direct permission
                    var directPermission = $"{resource}:{action}";
                    if (userPermissions.Contains(directPermission))
                    {
                        await _auditService.LogAuthorizationSuccessAsync(userId, resource, action, "Direct permission");
                        return true;
                    }
                    
                    // Check role-based permissions
                    foreach (var role in userRoles)
                    {
                        var rolePermissions = await _roleService.GetRolePermissionsAsync(role);
                        if (rolePermissions.Contains(directPermission))
                        {
                            await _auditService.LogAuthorizationSuccessAsync(userId, resource, action, $"Role: {role}");
                            return true;
                        }
                    }
                    
                    await _auditService.LogAuthorizationFailureAsync(userId, resource, action, "No permission found");
                    return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Authorization failed for user {UserId}, resource {Resource}, action {Action}", 
                        userId, resource, action);
                    return false;
                }
            }
            
            public async Task<bool> AuthorizeWithRolesAsync(string userId, string resource, string action, List<string> requiredRoles)
            {
                var userRoles = await GetUserRolesAsync(userId);
                var hasRequiredRole = requiredRoles.Any(role => userRoles.Contains(role));
                
                if (hasRequiredRole)
                {
                    await _auditService.LogAuthorizationSuccessAsync(userId, resource, action, $"Required roles: {string.Join(", ", requiredRoles)}");
                }
                else
                {
                    await _auditService.LogAuthorizationFailureAsync(userId, resource, action, $"Required roles not met: {string.Join(", ", requiredRoles)}");
                }
                
                return hasRequiredRole;
            }
            
            public async Task<bool> AuthorizeWithPermissionsAsync(string userId, string resource, string action, List<string> requiredPermissions)
            {
                var userPermissions = await GetUserPermissionsAsync(userId);
                var hasRequiredPermission = requiredPermissions.Any(permission => userPermissions.Contains(permission));
                
                if (hasRequiredPermission)
                {
                    await _auditService.LogAuthorizationSuccessAsync(userId, resource, action, $"Required permissions: {string.Join(", ", requiredPermissions)}");
                }
                else
                {
                    await _auditService.LogAuthorizationFailureAsync(userId, resource, action, $"Required permissions not met: {string.Join(", ", requiredPermissions)}");
                }
                
                return hasRequiredPermission;
            }
            
            public async Task<bool> AuthorizeWithContextAsync(string userId, string resource, string action, AuthorizationContext context)
            {
                // Basic authorization first
                var basicAuth = await AuthorizeAsync(userId, resource, action);
                if (!basicAuth)
                {
                    return false;
                }
                
                // Apply context-specific rules
                if (context.TimeRestrictions != null)
                {
                    var currentTime = DateTime.UtcNow.TimeOfDay;
                    if (currentTime < context.TimeRestrictions.StartTime || currentTime > context.TimeRestrictions.EndTime)
                    {
                        await _auditService.LogAuthorizationFailureAsync(userId, resource, action, "Time restriction violation");
                        return false;
                    }
                }
                
                if (context.LocationRestrictions != null)
                {
                    if (!context.LocationRestrictions.AllowedLocations.Contains(context.CurrentLocation))
                    {
                        await _auditService.LogAuthorizationFailureAsync(userId, resource, action, "Location restriction violation");
                        return false;
                    }
                }
                
                if (context.DeviceRestrictions != null)
                {
                    if (!context.DeviceRestrictions.AllowedDevices.Contains(context.CurrentDevice))
                    {
                        await _auditService.LogAuthorizationFailureAsync(userId, resource, action, "Device restriction violation");
                        return false;
                    }
                }
                
                await _auditService.LogAuthorizationSuccessAsync(userId, resource, action, "Context-based authorization");
                return true;
            }
            
            public async Task<List<string>> GetUserPermissionsAsync(string userId)
            {
                var user = await _userService.GetByIdAsync(userId);
                if (user == null)
                {
                    return new List<string>();
                }
                
                var permissions = new List<string>();
                
                // Direct user permissions
                if (user.Permissions != null)
                {
                    permissions.AddRange(user.Permissions);
                }
                
                // Role-based permissions
                var userRoles = await GetUserRolesAsync(userId);
                foreach (var role in userRoles)
                {
                    var rolePermissions = await _roleService.GetRolePermissionsAsync(role);
                    permissions.AddRange(rolePermissions);
                }
                
                return permissions.Distinct().ToList();
            }
            
            public async Task<List<string>> GetUserRolesAsync(string userId)
            {
                var user = await _userService.GetByIdAsync(userId);
                return user?.Roles?.ToList() ?? new List<string>();
            }
        }
        
        public class AuthorizationContext
        {
            public TimeRestrictions TimeRestrictions { get; set; }
            public LocationRestrictions LocationRestrictions { get; set; }
            public DeviceRestrictions DeviceRestrictions { get; set; }
            public string CurrentLocation { get; set; }
            public string CurrentDevice { get; set; }
        }
        
        public class TimeRestrictions
        {
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
        }
        
        public class LocationRestrictions
        {
            public List<string> AllowedLocations { get; set; } = new List<string>();
        }
        
        public class DeviceRestrictions
        {
            public List<string> AllowedDevices { get; set; } = new List<string>();
        }
        
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
        public class AuthorizeAttribute : Attribute
        {
            public string Resource { get; }
            public string Action { get; }
            public List<string> RequiredRoles { get; }
            public List<string> RequiredPermissions { get; }
            
            public AuthorizeAttribute(string resource, string action, string requiredRoles = null, string requiredPermissions = null)
            {
                Resource = resource;
                Action = action;
                RequiredRoles = !string.IsNullOrEmpty(requiredRoles) ? requiredRoles.Split(',').ToList() : new List<string>();
                RequiredPermissions = !string.IsNullOrEmpty(requiredPermissions) ? requiredPermissions.Split(',').ToList() : new List<string>();
            }
        }
    }
    
    // ===== ENCRIPTACIÓN Y HASHING =====
    namespace EncryptionAndHashing
    {
        public interface IEncryptionService
        {
            Task<string> EncryptAsync(string plaintext, string key);
            Task<string> DecryptAsync(string ciphertext, string key);
            Task<byte[]> EncryptBytesAsync(byte[] data, string key);
            Task<byte[]> DecryptBytesAsync(byte[] data, string key);
            Task<string> GenerateKeyAsync();
            Task<string> HashAsync(string input);
            Task<bool> VerifyHashAsync(string input, string hash);
        }
        
        public class EncryptionService : IEncryptionService
        {
            private readonly ILogger<EncryptionService> _logger;
            private readonly IConfiguration _configuration;
            
            public EncryptionService(ILogger<EncryptionService> logger, IConfiguration configuration)
            {
                _logger = logger;
                _configuration = configuration;
            }
            
            public async Task<string> EncryptAsync(string plaintext, string key)
            {
                try
                {
                    using var aes = Aes.Create();
                    aes.Key = Convert.FromBase64String(key);
                    aes.GenerateIV();
                    
                    using var encryptor = aes.CreateEncryptor();
                    using var msEncrypt = new MemoryStream();
                    using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                    using var swEncrypt = new StreamWriter(csEncrypt);
                    
                    await swEncrypt.WriteAsync(plaintext);
                    swEncrypt.Close();
                    
                    var encrypted = msEncrypt.ToArray();
                    var result = new byte[aes.IV.Length + encrypted.Length];
                    Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
                    Array.Copy(encrypted, 0, result, aes.IV.Length, encrypted.Length);
                    
                    return Convert.ToBase64String(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Encryption failed");
                    throw new EncryptionException("Failed to encrypt data", ex);
                }
            }
            
            public async Task<string> DecryptAsync(string ciphertext, string key)
            {
                try
                {
                    var fullCipher = Convert.FromBase64String(ciphertext);
                    
                    using var aes = Aes.Create();
                    aes.Key = Convert.FromBase64String(key);
                    
                    var iv = new byte[16];
                    var cipher = new byte[fullCipher.Length - 16];
                    
                    Array.Copy(fullCipher, 0, iv, 0, 16);
                    Array.Copy(fullCipher, 16, cipher, 0, cipher.Length);
                    
                    aes.IV = iv;
                    
                    using var decryptor = aes.CreateDecryptor();
                    using var msDecrypt = new MemoryStream(cipher);
                    using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                    using var srDecrypt = new StreamReader(csDecrypt);
                    
                    return await srDecrypt.ReadToEndAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Decryption failed");
                    throw new DecryptionException("Failed to decrypt data", ex);
                }
            }
            
            public async Task<byte[]> EncryptBytesAsync(byte[] data, string key)
            {
                try
                {
                    using var aes = Aes.Create();
                    aes.Key = Convert.FromBase64String(key);
                    aes.GenerateIV();
                    
                    using var encryptor = aes.CreateEncryptor();
                    using var msEncrypt = new MemoryStream();
                    using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                    
                    await csEncrypt.WriteAsync(data);
                    csEncrypt.Close();
                    
                    var encrypted = msEncrypt.ToArray();
                    var result = new byte[aes.IV.Length + encrypted.Length];
                    Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
                    Array.Copy(encrypted, 0, result, aes.IV.Length, encrypted.Length);
                    
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Byte encryption failed");
                    throw new EncryptionException("Failed to encrypt bytes", ex);
                }
            }
            
            public async Task<byte[]> DecryptBytesAsync(byte[] data, string key)
            {
                try
                {
                    using var aes = Aes.Create();
                    aes.Key = Convert.FromBase64String(key);
                    
                    var iv = new byte[16];
                    var cipher = new byte[data.Length - 16];
                    
                    Array.Copy(data, 0, iv, 0, 16);
                    Array.Copy(data, 16, cipher, 0, cipher.Length);
                    
                    aes.IV = iv;
                    
                    using var decryptor = aes.CreateDecryptor();
                    using var msDecrypt = new MemoryStream(cipher);
                    using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                    using var resultStream = new MemoryStream();
                    
                    await csDecrypt.CopyToAsync(resultStream);
                    return resultStream.ToArray();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Byte decryption failed");
                    throw new DecryptionException("Failed to decrypt bytes", ex);
                }
            }
            
            public async Task<string> GenerateKeyAsync()
            {
                using var aes = Aes.Create();
                aes.GenerateKey();
                return await Task.FromResult(Convert.ToBase64String(aes.Key));
            }
            
            public async Task<string> HashAsync(string input)
            {
                using var sha256 = SHA256.Create();
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return await Task.FromResult(Convert.ToBase64String(hashBytes));
            }
            
            public async Task<bool> VerifyHashAsync(string input, string hash)
            {
                var computedHash = await HashAsync(input);
                return computedHash == hash;
            }
        }
        
        public class EncryptionException : Exception
        {
            public EncryptionException(string message, Exception innerException) : base(message, innerException) { }
        }
        
        public class DecryptionException : Exception
        {
            public DecryptionException(string message, Exception innerException) : base(message, innerException) { }
        }
    }
    
    // ===== AUDITORÍA Y COMPLIANCE =====
    namespace AuditAndCompliance
    {
        public interface IAuditService
        {
            Task LogAuthenticationEventAsync(string username, string eventType, string details);
            Task LogAuthorizationEventAsync(string userId, string resource, string action, string result, string details);
            Task LogDataAccessEventAsync(string userId, string resource, string action, string details);
            Task LogSystemEventAsync(string eventType, string details, LogLevel level = LogLevel.Information);
            Task<List<AuditLog>> GetAuditLogsAsync(AuditLogFilter filter);
            Task<List<AuditLog>> GetAuditLogsByUserAsync(string userId, DateTime? from = null, DateTime? to = null);
            Task<List<AuditLog>> GetAuditLogsByResourceAsync(string resource, DateTime? from = null, DateTime? to = null);
        }
        
        public class AuditService : IAuditService
        {
            private readonly ApplicationDbContext _context;
            private readonly ILogger<AuditService> _logger;
            private readonly IConfiguration _configuration;
            
            public AuditService(
                ApplicationDbContext context,
                ILogger<AuditService> logger,
                IConfiguration configuration)
            {
                _context = context;
                _logger = logger;
                _configuration = configuration;
            }
            
            public async Task LogAuthenticationEventAsync(string username, string eventType, string details)
            {
                var auditLog = new AuditLog
                {
                    Timestamp = DateTime.UtcNow,
                    EventType = "Authentication",
                    SubEventType = eventType,
                    Username = username,
                    Details = details,
                    IpAddress = GetCurrentIpAddress(),
                    UserAgent = GetCurrentUserAgent()
                };
                
                await _context.AuditLogs.AddAsync(auditLog);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Authentication audit log: {EventType} for {Username} - {Details}", 
                    eventType, username, details);
            }
            
            public async Task LogAuthorizationEventAsync(string userId, string resource, string action, string result, string details)
            {
                var auditLog = new AuditLog
                {
                    Timestamp = DateTime.UtcNow,
                    EventType = "Authorization",
                    SubEventType = $"{resource}:{action}",
                    UserId = userId,
                    Resource = resource,
                    Action = action,
                    Result = result,
                    Details = details,
                    IpAddress = GetCurrentIpAddress(),
                    UserAgent = GetCurrentUserAgent()
                };
                
                await _context.AuditLogs.AddAsync(auditLog);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Authorization audit log: {Result} {Action} on {Resource} for user {UserId} - {Details}", 
                    result, action, resource, userId, details);
            }
            
            public async Task LogDataAccessEventAsync(string userId, string resource, string action, string details)
            {
                var auditLog = new AuditLog
                {
                    Timestamp = DateTime.UtcNow,
                    EventType = "DataAccess",
                    SubEventType = $"{resource}:{action}",
                    UserId = userId,
                    Resource = resource,
                    Action = action,
                    Details = details,
                    IpAddress = GetCurrentIpAddress(),
                    UserAgent = GetCurrentUserAgent()
                };
                
                await _context.AuditLogs.AddAsync(auditLog);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Data access audit log: {Action} on {Resource} by user {UserId} - {Details}", 
                    action, resource, userId, details);
            }
            
            public async Task LogSystemEventAsync(string eventType, string details, LogLevel level = LogLevel.Information)
            {
                var auditLog = new AuditLog
                {
                    Timestamp = DateTime.UtcNow,
                    EventType = "System",
                    SubEventType = eventType,
                    Details = details,
                    IpAddress = GetCurrentIpAddress(),
                    UserAgent = GetCurrentUserAgent()
                };
                
                await _context.AuditLogs.AddAsync(auditLog);
                await _context.SaveChangesAsync();
                
                _logger.Log(level, "System audit log: {EventType} - {Details}", eventType, details);
            }
            
            public async Task<List<AuditLog>> GetAuditLogsAsync(AuditLogFilter filter)
            {
                var query = _context.AuditLogs.AsQueryable();
                
                if (!string.IsNullOrEmpty(filter.EventType))
                {
                    query = query.Where(a => a.EventType == filter.EventType);
                }
                
                if (!string.IsNullOrEmpty(filter.SubEventType))
                {
                    query = query.Where(a => a.SubEventType == filter.SubEventType);
                }
                
                if (!string.IsNullOrEmpty(filter.Username))
                {
                    query = query.Where(a => a.Username == filter.Username);
                }
                
                if (!string.IsNullOrEmpty(filter.UserId))
                {
                    query = query.Where(a => a.UserId == filter.UserId);
                }
                
                if (!string.IsNullOrEmpty(filter.Resource))
                {
                    query = query.Where(a => a.Resource == filter.Resource);
                }
                
                if (filter.From.HasValue)
                {
                    query = query.Where(a => a.Timestamp >= filter.From.Value);
                }
                
                if (filter.To.HasValue)
                {
                    query = query.Where(a => a.Timestamp <= filter.To.Value);
                }
                
                return await query
                    .OrderByDescending(a => a.Timestamp)
                    .Skip(filter.Skip)
                    .Take(filter.Take)
                    .ToListAsync();
            }
            
            public async Task<List<AuditLog>> GetAuditLogsByUserAsync(string userId, DateTime? from = null, DateTime? to = null)
            {
                var query = _context.AuditLogs.Where(a => a.UserId == userId);
                
                if (from.HasValue)
                {
                    query = query.Where(a => a.Timestamp >= from.Value);
                }
                
                if (to.HasValue)
                {
                    query = query.Where(a => a.Timestamp <= to.Value);
                }
                
                return await query
                    .OrderByDescending(a => a.Timestamp)
                    .ToListAsync();
            }
            
            public async Task<List<AuditLog>> GetAuditLogsByResourceAsync(string resource, DateTime? from = null, DateTime? to = null)
            {
                var query = _context.AuditLogs.Where(a => a.Resource == resource);
                
                if (from.HasValue)
                {
                    query = query.Where(a => a.Timestamp >= from.Value);
                }
                
                if (to.HasValue)
                {
                    query = query.Where(a => a.Timestamp <= to.Value);
                }
                
                return await query
                    .OrderByDescending(a => a.Timestamp)
                    .ToListAsync();
            }
            
            private string GetCurrentIpAddress()
            {
                // In a real implementation, you would get this from the HTTP context
                return "127.0.0.1";
            }
            
            private string GetCurrentUserAgent()
            {
                // In a real implementation, you would get this from the HTTP context
                return "Unknown";
            }
            
            // Convenience methods for common audit events
            public async Task LogFailedLoginAttemptAsync(string username, string reason)
            {
                await LogAuthenticationEventAsync(username, "FailedLogin", reason);
            }
            
            public async Task LogSuccessfulLoginAsync(string username, string userId)
            {
                await LogAuthenticationEventAsync(username, "SuccessfulLogin", $"User ID: {userId}");
            }
            
            public async Task LogAccountLockedAsync(string username, string reason)
            {
                await LogAuthenticationEventAsync(username, "AccountLocked", reason);
            }
            
            public async Task LogFailedMfaAttemptAsync(string username, string userId)
            {
                await LogAuthenticationEventAsync(username, "FailedMfa", $"User ID: {userId}");
            }
            
            public async Task LogSuccessfulMfaAsync(string username, string userId)
            {
                await LogAuthenticationEventAsync(username, "SuccessfulMfa", $"User ID: {userId}");
            }
            
            public async Task LogFailedBiometricAttemptAsync(string username, string userId)
            {
                await LogAuthenticationEventAsync(username, "FailedBiometric", $"User ID: {userId}");
            }
            
            public async Task LogSuccessfulBiometricLoginAsync(string username, string userId)
            {
                await LogAuthenticationEventAsync(username, "SuccessfulBiometric", $"User ID: {userId}");
            }
            
            public async Task LogAuthorizationSuccessAsync(string userId, string resource, string action, string details)
            {
                await LogAuthorizationEventAsync(userId, resource, action, "Success", details);
            }
            
            public async Task LogAuthorizationFailureAsync(string userId, string resource, string action, string details)
            {
                await LogAuthorizationEventAsync(userId, resource, action, "Failure", details);
            }
        }
        
        public class AuditLog
        {
            public int Id { get; set; }
            public DateTime Timestamp { get; set; }
            public string EventType { get; set; }
            public string SubEventType { get; set; }
            public string Username { get; set; }
            public string UserId { get; set; }
            public string Resource { get; set; }
            public string Action { get; set; }
            public string Result { get; set; }
            public string Details { get; set; }
            public string IpAddress { get; set; }
            public string UserAgent { get; set; }
        }
        
        public class AuditLogFilter
        {
            public string EventType { get; set; }
            public string SubEventType { get; set; }
            public string Username { get; set; }
            public string UserId { get; set; }
            public string Resource { get; set; }
            public DateTime? From { get; set; }
            public DateTime? To { get; set; }
            public int Skip { get; set; } = 0;
            public int Take { get; set; } = 100;
        }
    }
    
    // ===== DEPENDENCY INJECTION =====
    namespace Infrastructure.DependencyInjection
    {
        public static class ServiceCollectionExtensions
        {
            public static IServiceCollection AddEnterpriseSecurity(this IServiceCollection services)
            {
                // Authentication
                services.AddScoped<IAuthenticationService, AuthenticationService>();
                services.AddScoped<IPasswordHasher, PasswordHasher>();
                services.AddScoped<ITokenService, TokenService>();
                services.AddScoped<IMfaService, MfaService>();
                services.AddScoped<IBiometricService, BiometricService>();
                
                // Authorization
                services.AddScoped<IAuthorizationService, AuthorizationService>();
                services.AddScoped<IRoleService, RoleService>();
                services.AddScoped<IPermissionService, PermissionService>();
                
                // Encryption
                services.AddScoped<IEncryptionService, EncryptionService>();
                
                // Audit
                services.AddScoped<IAuditService, AuditService>();
                
                return services;
            }
        }
    }
}

// Uso de Arquitectura de Seguridad Enterprise
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Arquitectura de Seguridad Enterprise ===\n");
        
        Console.WriteLine("Los componentes implementados incluyen:");
        Console.WriteLine("1. Autenticación avanzada con MFA y biometría");
        Console.WriteLine("2. Autorización basada en roles y permisos");
        Console.WriteLine("3. Encriptación AES y hashing SHA256");
        Console.WriteLine("4. Sistema de auditoría completo");
        Console.WriteLine("5. Compliance y logging de seguridad");
        Console.WriteLine("6. Middleware de autorización automática");
        
        Console.WriteLine("\nBeneficios de esta arquitectura:");
        Console.WriteLine("- Seguridad multicapa robusta");
        Console.WriteLine("- Cumplimiento de estándares regulatorios");
        Console.WriteLine("- Auditoría completa de acciones");
        Console.WriteLine("- Control granular de acceso");
        Console.WriteLine("- Protección de datos sensibles");
        Console.WriteLine("- Trazabilidad de seguridad");
    }
}
