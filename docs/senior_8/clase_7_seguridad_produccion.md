# 🔒 Clase 7: Seguridad en Producción

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 6: Performance y Escalabilidad](../senior_8/clase_6_performance_escalabilidad.md)
- **🏠 Inicio del Módulo**: [Módulo 15: Sistemas Avanzados y Distribuidos](../senior_8/README.md)
- **➡️ Siguiente**: [Clase 8: Backup y Disaster Recovery](../senior_8/clase_8_backup_disaster_recovery.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** autenticación JWT avanzada
2. **Configurar** autorización basada en políticas
3. **Desarrollar** protección contra ataques comunes
4. **Aplicar** encriptación y hashing seguro
5. **Optimizar** auditoría y logging de seguridad

---

## 🔐 **Autenticación JWT Avanzada**

### **Servicio JWT con Refresh Tokens**

```csharp
// MusicalMatching.Application/Services/JwtService.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace MusicalMatching.Application.Services;

public interface IJwtService
{
    Task<string> GenerateTokenAsync(ApplicationUser user);
    Task<string> GenerateRefreshTokenAsync();
    Task<ClaimsPrincipal?> GetPrincipalFromExpiredTokenAsync(string token);
    Task<bool> ValidateTokenAsync(string token);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtService> _logger;
    private readonly SigningCredentials _signingCredentials;
    private readonly TokenValidationParameters _validationParameters;

    public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key not configured")));
        
        _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = _configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = _configuration["JwtSettings:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }

    public async Task<string> GenerateTokenAsync(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? ""),
            new(ClaimTypes.Email, user.Email ?? ""),
            new("Permission", "MusicianMatching.Read"),
            new("UserType", user.UserType.ToString()),
            new("SubscriptionTier", user.SubscriptionTier.ToString()),
            new("ProfileComplete", user.IsProfileComplete.ToString()),
            new("LastLogin", user.LastLoginDate?.ToString("O") ?? ""),
            new("AccountStatus", user.AccountStatus.ToString())
        };

        // Add role claims
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                int.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "60")),
            signingCredentials: _signingCredentials
        );

        _logger.LogInformation("Generated JWT token for user {UserId}", user.Id);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateRefreshTokenAsync()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task<ClaimsPrincipal?> GetPrincipalFromExpiredTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, _validationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogWarning("Invalid JWT token algorithm");
                return null;
            }

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error validating expired token");
            return null;
        }
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ValidateToken(token, _validationParameters, out _);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return false;
        }
    }
}
```

---

## 🛡️ **Autorización Basada en Políticas**

### **Políticas de Autorización Personalizadas**

```csharp
// MusicalMatching.API/Authorization/AuthorizationPolicies.cs
using Microsoft.AspNetCore.Authorization;

namespace MusicalMatching.API.Authorization;

public static class AuthorizationPolicies
{
    public const string RequirePremiumSubscription = "RequirePremiumSubscription";
    public const string RequireVerifiedProfile = "RequireVerifiedProfile";
    public const string RequireMusicianRole = "RequireMusicianRole";
    public const string RequireEventOrganizerRole = "RequireEventOrganizerRole";
    public const string RequireAdminRole = "RequireAdminRole";
    public const string RequireActiveAccount = "RequireActiveAccount";
}

public class PremiumSubscriptionRequirement : IAuthorizationRequirement
{
    public string MinimumTier { get; }

    public PremiumSubscriptionRequirement(string minimumTier = "Premium")
    {
        MinimumTier = minimumTier;
    }
}

public class PremiumSubscriptionHandler : AuthorizationHandler<PremiumSubscriptionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        PremiumSubscriptionRequirement requirement)
    {
        var subscriptionTier = context.User.FindFirst("SubscriptionTier")?.Value;
        
        if (string.IsNullOrEmpty(subscriptionTier))
        {
            return Task.CompletedTask;
        }

        var tierHierarchy = new Dictionary<string, int>
        {
            ["Basic"] = 1,
            ["Premium"] = 2,
            ["Enterprise"] = 3
        };

        if (tierHierarchy.TryGetValue(subscriptionTier, out var userTier) &&
            tierHierarchy.TryGetValue(requirement.MinimumTier, out var requiredTier))
        {
            if (userTier >= requiredTier)
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}

public class VerifiedProfileRequirement : IAuthorizationRequirement { }

public class VerifiedProfileHandler : AuthorizationHandler<VerifiedProfileRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        VerifiedProfileRequirement requirement)
    {
        var profileComplete = context.User.FindFirst("ProfileComplete")?.Value;
        
        if (bool.TryParse(profileComplete, out var isComplete) && isComplete)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
```

---

## 🚫 **Protección Contra Ataques Comunes**

### **Rate Limiting y Throttling**

```csharp
// MusicalMatching.API/Middleware/RateLimitingMiddleware.cs
using System.Collections.Concurrent;

namespace MusicalMatching.API.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly ConcurrentDictionary<string, RateLimitInfo> _rateLimitStore;
    private readonly int _maxRequestsPerMinute;
    private readonly int _maxRequestsPerHour;

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _rateLimitStore = new ConcurrentDictionary<string, RateLimitInfo>();
        _maxRequestsPerMinute = configuration.GetValue<int>("RateLimiting:MaxRequestsPerMinute", 100);
        _maxRequestsPerHour = configuration.GetValue<int>("RateLimiting:MaxRequestsPerHour", 1000);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        
        if (!await CheckRateLimitAsync(clientId))
        {
            context.Response.StatusCode = 429; // Too Many Requests
            context.Response.Headers["Retry-After"] = "60";
            await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
            return;
        }

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Use IP address or user ID if authenticated
        var user = context.User.Identity;
        if (user?.IsAuthenticated == true)
        {
            return $"user:{user.Name}";
        }

        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ipAddress}";
    }

    private async Task<bool> CheckRateLimitAsync(string clientId)
    {
        var now = DateTime.UtcNow;
        var rateLimitInfo = _rateLimitStore.GetOrAdd(clientId, _ => new RateLimitInfo());

        // Clean up old entries
        rateLimitInfo.Requests.RemoveAll(r => r < now.AddMinutes(-1));
        rateLimitInfo.HourlyRequests.RemoveAll(r => r < now.AddHours(-1));

        // Check minute limit
        if (rateLimitInfo.Requests.Count >= _maxRequestsPerMinute)
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientId} per minute", clientId);
            return false;
        }

        // Check hour limit
        if (rateLimitInfo.HourlyRequests.Count >= _maxRequestsPerHour)
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientId} per hour", clientId);
            return false;
        }

        // Add current request
        rateLimitInfo.Requests.Add(now);
        rateLimitInfo.HourlyRequests.Add(now);

        return true;
    }

    private class RateLimitInfo
    {
        public List<DateTime> Requests { get; } = new();
        public List<DateTime> HourlyRequests { get; } = new();
    }
}
```

---

## 🔒 **Encriptación y Hashing Seguro**

### **Servicio de Encriptación**

```csharp
// MusicalMatching.Application/Services/EncryptionService.cs
using System.Security.Cryptography;

namespace MusicalMatching.Application.Services;

public interface IEncryptionService
{
    Task<string> EncryptAsync(string plainText, string key);
    Task<string> DecryptAsync(string cipherText, string key);
    Task<string> HashAsync(string input);
    Task<bool> VerifyHashAsync(string input, string hash);
}

public class EncryptionService : IEncryptionService
{
    private readonly ILogger<EncryptionService> _logger;
    private readonly string _masterKey;

    public EncryptionService(IConfiguration configuration, ILogger<EncryptionService> logger)
    {
        _logger = logger;
        _masterKey = configuration["Encryption:MasterKey"] ?? 
            throw new InvalidOperationException("Encryption master key not configured");
    }

    public async Task<string> EncryptAsync(string plainText, string key)
    {
        try
        {
            using var aes = Aes.Create();
            aes.Key = DeriveKey(key, aes.KeySize / 8);
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                await swEncrypt.WriteAsync(plainText);
            }

            var encrypted = msEncrypt.ToArray();
            var result = new byte[aes.IV.Length + encrypted.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(encrypted, 0, result, aes.IV.Length, encrypted.Length);

            return Convert.ToBase64String(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting data");
            throw;
        }
    }

    public async Task<string> DecryptAsync(string cipherText, string key)
    {
        try
        {
            var fullCipher = Convert.FromBase64String(cipherText);
            
            using var aes = Aes.Create();
            var iv = new byte[aes.BlockSize / 8];
            var cipher = new byte[fullCipher.Length - iv.Length];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

            aes.Key = DeriveKey(key, aes.KeySize / 8);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            using var msDecrypt = new MemoryStream(cipher);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            
            return await srDecrypt.ReadToEndAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting data");
            throw;
        }
    }

    public async Task<string> HashAsync(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = await Task.Run(() => sha256.ComputeHash(bytes));
        return Convert.ToBase64String(hash);
    }

    public async Task<bool> VerifyHashAsync(string input, string hash)
    {
        var computedHash = await HashAsync(input);
        return computedHash == hash;
    }

    private byte[] DeriveKey(string password, int keySize)
    {
        using var deriveBytes = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(_masterKey), 10000);
        return deriveBytes.GetBytes(keySize);
    }
}
```

---

## 📝 **Auditoría y Logging de Seguridad**

### **Servicio de Auditoría de Seguridad**

```csharp
// MusicalMatching.Application/Services/SecurityAuditService.cs
using System.Security.Claims;

namespace MusicalMatching.Application.Services;

public interface ISecurityAuditService
{
    Task LogAuthenticationAsync(string userId, bool success, string? failureReason = null);
    Task LogAuthorizationAsync(string userId, string resource, string action, bool allowed);
    Task LogSecurityEventAsync(string userId, string eventType, string details);
    Task<List<SecurityAuditLog>> GetAuditLogsAsync(string userId, DateTime? from = null, DateTime? to = null);
}

public class SecurityAuditService : ISecurityAuditService
{
    private readonly ILogger<SecurityAuditService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApplicationDbContext _context;

    public SecurityAuditService(
        ILogger<SecurityAuditService> logger,
        IHttpContextAccessor httpContextAccessor,
        ApplicationDbContext context)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    public async Task LogAuthenticationAsync(string userId, bool success, string? failureReason = null)
    {
        var log = new SecurityAuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EventType = success ? "AuthenticationSuccess" : "AuthenticationFailure",
            Resource = "Authentication",
            Action = "Login",
            Details = failureReason ?? "Success",
            IpAddress = GetClientIpAddress(),
            UserAgent = GetUserAgent(),
            Timestamp = DateTime.UtcNow
        };

        await _context.SecurityAuditLogs.AddAsync(log);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Authentication {Result} for user {UserId} from {IpAddress}", 
            success ? "succeeded" : "failed", userId, log.IpAddress);
    }

    public async Task LogAuthorizationAsync(string userId, string resource, string action, bool allowed)
    {
        var log = new SecurityAuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EventType = allowed ? "AuthorizationGranted" : "AuthorizationDenied",
            Resource = resource,
            Action = action,
            Details = allowed ? "Access granted" : "Access denied",
            IpAddress = GetClientIpAddress(),
            UserAgent = GetUserAgent(),
            Timestamp = DateTime.UtcNow
        };

        await _context.SecurityAuditLogs.AddAsync(log);
        await _context.SaveChangesAsync();

        if (!allowed)
        {
            _logger.LogWarning("Authorization denied for user {UserId} on {Resource}:{Action} from {IpAddress}", 
                userId, resource, action, log.IpAddress);
        }
    }

    public async Task LogSecurityEventAsync(string userId, string eventType, string details)
    {
        var log = new SecurityAuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EventType = eventType,
            Resource = "Security",
            Action = "Event",
            Details = details,
            IpAddress = GetClientIpAddress(),
            UserAgent = GetUserAgent(),
            Timestamp = DateTime.UtcNow
        };

        await _context.SecurityAuditLogs.AddAsync(log);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Security event {EventType} for user {UserId}: {Details}", 
            eventType, userId, details);
    }

    public async Task<List<SecurityAuditLog>> GetAuditLogsAsync(string userId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.SecurityAuditLogs.Where(l => l.UserId == userId);

        if (from.HasValue)
            query = query.Where(l => l.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(l => l.Timestamp <= to.Value);

        return await query.OrderByDescending(l => l.Timestamp).ToListAsync();
    }

    private string GetClientIpAddress()
    {
        var context = _httpContextAccessor.HttpContext;
        return context?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private string GetUserAgent()
    {
        var context = _httpContextAccessor.HttpContext;
        return context?.Request.Headers["User-Agent"].ToString() ?? "unknown";
    }
}

public class SecurityAuditLog
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Autenticación JWT**
```csharp
// Implementa:
// - JWT con claims personalizados
// - Refresh tokens
// - Validación de tokens
// - Manejo de expiración
```

### **Ejercicio 2: Autorización Basada en Políticas**
```csharp
// Crea:
// - Políticas personalizadas
// - Handlers de autorización
// - Requisitos complejos
// - Evaluación de políticas
```

### **Ejercicio 3: Protección Contra Ataques**
```csharp
// Implementa:
// - Rate limiting
// - Input validation
// - SQL injection prevention
// - XSS protection
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **🔐 Autenticación JWT Avanzada**: Tokens con claims personalizados y refresh
2. **🛡️ Autorización Basada en Políticas**: Políticas personalizadas y handlers
3. **🚫 Protección Contra Ataques**: Rate limiting y validación de entrada
4. **🔒 Encriptación y Hashing**: Servicios seguros de encriptación
5. **📝 Auditoría y Logging**: Registro completo de eventos de seguridad

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Backup y Disaster Recovery**, implementando estrategias de respaldo y recuperación ante desastres.

---

**¡Has completado la séptima clase del Módulo 15! 🔒🛡️**


