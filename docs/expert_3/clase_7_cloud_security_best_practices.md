# 🔒 **Clase 7: Cloud Security y Best Practices**

## 🎯 **Objetivo de la Clase**
Implementar seguridad robusta en aplicaciones cloud native, incluyendo autenticación, autorización, encriptación y mejores prácticas de seguridad.

## 📚 **Contenido Teórico**

### **1. Autenticación y Autorización**

#### **Azure Active Directory (AAD)**
```csharp
// Services/AzureADService.cs
public class AzureADService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AzureADService> _logger;

    public AzureADService(IConfiguration configuration, ILogger<AzureADService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthenticationResult> AuthenticateUserAsync(string username, string password)
    {
        try
        {
            var clientId = _configuration["AzureAD:ClientId"];
            var tenantId = _configuration["AzureAD:TenantId"];
            var clientSecret = _configuration["AzureAD:ClientSecret"];

            var app = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
                .Build();

            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            _logger.LogInformation("User authenticated successfully: {Username}", username);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating user: {Username}", username);
            throw;
        }
    }

    public async Task<UserInfo> GetUserInfoAsync(string accessToken)
    {
        try
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.GetAsync("https://graph.microsoft.com/v1.0/me");
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var userInfo = JsonSerializer.Deserialize<UserInfo>(content);
                return userInfo;
            }

            throw new UnauthorizedAccessException("Failed to get user info");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info");
            throw;
        }
    }
}

// Models/UserInfo.cs
public class UserInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;
    
    [JsonPropertyName("mail")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("userPrincipalName")]
    public string UserPrincipalName { get; set; } = string.Empty;
}
```

#### **JWT Authentication**
```csharp
// Services/JwtService.cs
public class JwtService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtService> _logger;

    public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string GenerateToken(User user, List<string> roles)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("userId", user.Id),
                new Claim("userType", user.UserType.ToString())
            };

            // Agregar roles
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            
            _logger.LogInformation("JWT token generated for user: {UserId}", user.Id);
            return tokenString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating JWT token for user: {UserId}", user.Id);
            throw;
        }
    }

    public ClaimsPrincipal ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            
            _logger.LogInformation("JWT token validated successfully");
            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating JWT token");
            throw;
        }
    }
}
```

### **2. Encriptación y Protección de Datos**

#### **Servicio de Encriptación**
```csharp
// Services/EncryptionService.cs
public class EncryptionService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EncryptionService> _logger;

    public EncryptionService(IConfiguration configuration, ILogger<EncryptionService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string EncryptSensitiveData(string plainText)
    {
        try
        {
            var key = _configuration["Encryption:Key"];
            var iv = _configuration["Encryption:IV"];

            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(key);
            aes.IV = Convert.FromBase64String(iv);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using var swEncrypt = new StreamWriter(csEncrypt);

            swEncrypt.Write(plainText);
            swEncrypt.Close();

            var encrypted = msEncrypt.ToArray();
            return Convert.ToBase64String(encrypted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting sensitive data");
            throw;
        }
    }

    public string DecryptSensitiveData(string cipherText)
    {
        try
        {
            var key = _configuration["Encryption:Key"];
            var iv = _configuration["Encryption:IV"];

            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(key);
            aes.IV = Convert.FromBase64String(iv);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            using var msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText));
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);

            return srDecrypt.ReadToEnd();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting sensitive data");
            throw;
        }
    }

    public string HashPassword(string password)
    {
        try
        {
            var salt = _configuration["Encryption:Salt"];
            var saltedPassword = password + salt;

            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            return Convert.ToBase64String(hashedBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hashing password");
            throw;
        }
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying password");
            throw;
        }
    }
}
```

#### **Protección de Datos Personales**
```csharp
// Services/DataProtectionService.cs
public class DataProtectionService
{
    private readonly IDataProtector _protector;
    private readonly ILogger<DataProtectionService> _logger;

    public DataProtectionService(IDataProtectionProvider provider, ILogger<DataProtectionService> logger)
    {
        _protector = provider.CreateProtector("MussikOn.DataProtection");
        _logger = logger;
    }

    public string ProtectPersonalData(string personalData)
    {
        try
        {
            var protectedData = _protector.Protect(personalData);
            _logger.LogInformation("Personal data protected successfully");
            return protectedData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error protecting personal data");
            throw;
        }
    }

    public string UnprotectPersonalData(string protectedData)
    {
        try
        {
            var unprotectedData = _protector.Unprotect(protectedData);
            _logger.LogInformation("Personal data unprotected successfully");
            return unprotectedData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unprotecting personal data");
            throw;
        }
    }

    public string MaskSensitiveData(string data, int visibleChars = 4)
    {
        try
        {
            if (string.IsNullOrEmpty(data) || data.Length <= visibleChars)
                return data;

            var masked = new string('*', data.Length - visibleChars) + data.Substring(data.Length - visibleChars);
            return masked;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error masking sensitive data");
            throw;
        }
    }
}
```

### **3. Azure Key Vault**

#### **Integración con Key Vault**
```csharp
// Services/KeyVaultService.cs
public class KeyVaultService
{
    private readonly SecretClient _secretClient;
    private readonly KeyClient _keyClient;
    private readonly ILogger<KeyVaultService> _logger;

    public KeyVaultService(IConfiguration configuration, ILogger<KeyVaultService> logger)
    {
        var keyVaultUrl = configuration["KeyVault:Url"];
        var credential = new DefaultAzureCredential();
        
        _secretClient = new SecretClient(new Uri(keyVaultUrl), credential);
        _keyClient = new KeyClient(new Uri(keyVaultUrl), credential);
        _logger = logger;
    }

    public async Task<string> GetSecretAsync(string secretName)
    {
        try
        {
            var secret = await _secretClient.GetSecretAsync(secretName);
            _logger.LogInformation("Secret retrieved successfully: {SecretName}", secretName);
            return secret.Value.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving secret: {SecretName}", secretName);
            throw;
        }
    }

    public async Task SetSecretAsync(string secretName, string secretValue)
    {
        try
        {
            await _secretClient.SetSecretAsync(secretName, secretValue);
            _logger.LogInformation("Secret set successfully: {SecretName}", secretName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting secret: {SecretName}", secretName);
            throw;
        }
    }

    public async Task<string> EncryptDataAsync(string keyName, string data)
    {
        try
        {
            var key = await _keyClient.GetKeyAsync(keyName);
            var encryptResult = await _keyClient.EncryptAsync(EncryptionAlgorithm.RsaOaep256, Encoding.UTF8.GetBytes(data));
            
            var encryptedData = Convert.ToBase64String(encryptResult.EncryptedData);
            _logger.LogInformation("Data encrypted successfully with key: {KeyName}", keyName);
            
            return encryptedData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting data with key: {KeyName}", keyName);
            throw;
        }
    }

    public async Task<string> DecryptDataAsync(string keyName, string encryptedData)
    {
        try
        {
            var key = await _keyClient.GetKeyAsync(keyName);
            var encryptedBytes = Convert.FromBase64String(encryptedData);
            var decryptResult = await _keyClient.DecryptAsync(EncryptionAlgorithm.RsaOaep256, encryptedBytes);
            
            var decryptedData = Encoding.UTF8.GetString(decryptResult.Plaintext);
            _logger.LogInformation("Data decrypted successfully with key: {KeyName}", keyName);
            
            return decryptedData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting data with key: {KeyName}", keyName);
            throw;
        }
    }
}
```

### **4. Security Headers y CORS**

#### **Configuración de Security Headers**
```csharp
// Middleware/SecurityHeadersMiddleware.cs
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;

    public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Security Headers
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Add("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
        
        // Content Security Policy
        context.Response.Headers.Add("Content-Security-Policy", 
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data: https:; " +
            "font-src 'self' data:; " +
            "connect-src 'self' https:; " +
            "frame-ancestors 'none';");

        await _next(context);
    }
}

// Program.cs
app.UseMiddleware<SecurityHeadersMiddleware>();
```

#### **Configuración de CORS**
```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("MussikOnPolicy", policy =>
    {
        policy.WithOrigins("https://mussikon.com", "https://www.mussikon.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

app.UseCors("MussikOnPolicy");
```

### **5. Rate Limiting**

#### **Implementación de Rate Limiting**
```csharp
// Services/RateLimitingService.cs
public class RateLimitingService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<RateLimitingService> _logger;

    public RateLimitingService(IMemoryCache cache, ILogger<RateLimitingService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> IsRateLimitedAsync(string key, int maxRequests, TimeSpan window)
    {
        try
        {
            var cacheKey = $"rate_limit_{key}";
            var requests = _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = window;
                return new List<DateTime>();
            });

            var now = DateTime.UtcNow;
            var validRequests = requests.Where(r => r > now - window).ToList();

            if (validRequests.Count >= maxRequests)
            {
                _logger.LogWarning("Rate limit exceeded for key: {Key}", key);
                return true;
            }

            validRequests.Add(now);
            _cache.Set(cacheKey, validRequests, window);

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking rate limit for key: {Key}", key);
            return false;
        }
    }
}

// Middleware/RateLimitingMiddleware.cs
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitingService _rateLimitingService;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    public RateLimitingMiddleware(
        RequestDelegate next, 
        RateLimitingService rateLimitingService, 
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _rateLimitingService = rateLimitingService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var endpoint = context.Request.Path.Value ?? "";
        var key = $"{clientIp}:{endpoint}";

        var isRateLimited = await _rateLimitingService.IsRateLimitedAsync(key, 100, TimeSpan.FromMinutes(1));

        if (isRateLimited)
        {
            context.Response.StatusCode = 429;
            context.Response.Headers.Add("Retry-After", "60");
            await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
            return;
        }

        await _next(context);
    }
}
```

### **6. Audit Logging**

#### **Servicio de Audit Logging**
```csharp
// Services/AuditLoggingService.cs
public class AuditLoggingService
{
    private readonly ILogger<AuditLoggingService> _logger;
    private readonly IEventStore _eventStore;

    public AuditLoggingService(ILogger<AuditLoggingService> logger, IEventStore eventStore)
    {
        _logger = logger;
        _eventStore = eventStore;
    }

    public async Task LogUserActionAsync(string userId, string action, string resource, object data = null)
    {
        try
        {
            var auditEvent = new AuditEvent
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Action = action,
                Resource = resource,
                Data = data,
                Timestamp = DateTime.UtcNow,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent()
            };

            _logger.LogInformation("Audit: User {UserId} performed {Action} on {Resource}", userId, action, resource);
            
            // Guardar en event store
            await _eventStore.SaveEventsAsync(Guid.Parse(auditEvent.Id), new[] { auditEvent }, 1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging audit event for user: {UserId}", userId);
        }
    }

    public async Task LogSecurityEventAsync(string eventType, string description, string userId = null)
    {
        try
        {
            var securityEvent = new SecurityEvent
            {
                Id = Guid.NewGuid().ToString(),
                EventType = eventType,
                Description = description,
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                IpAddress = GetClientIpAddress(),
                UserAgent = GetUserAgent()
            };

            _logger.LogWarning("Security Event: {EventType} - {Description}", eventType, description);
            
            // Guardar en event store
            await _eventStore.SaveEventsAsync(Guid.Parse(securityEvent.Id), new[] { securityEvent }, 1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging security event: {EventType}", eventType);
        }
    }

    private string GetClientIpAddress()
    {
        // Implementar obtención de IP del cliente
        return "127.0.0.1";
    }

    private string GetUserAgent()
    {
        // Implementar obtención de User Agent
        return "Unknown";
    }
}

// Models/AuditEvent.cs
public class AuditEvent : DomainEvent
{
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public object Data { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}

// Models/SecurityEvent.cs
public class SecurityEvent : DomainEvent
{
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}
```

## 🛠️ **Ejercicio Práctico**

### **Ejercicio 1: Implementar Sistema de Seguridad Completo**

Crea un sistema de seguridad completo para MussikOn:

```csharp
// 1. Configurar servicios de seguridad
public class SecurityServiceConfig
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // JWT Authentication
        services.AddSingleton<JwtService>();
        
        // Azure AD
        services.AddSingleton<AzureADService>();
        
        // Encryption
        services.AddSingleton<EncryptionService>();
        
        // Data Protection
        services.AddDataProtection();
        services.AddSingleton<DataProtectionService>();
        
        // Key Vault
        services.AddSingleton<KeyVaultService>();
        
        // Rate Limiting
        services.AddSingleton<RateLimitingService>();
        
        // Audit Logging
        services.AddSingleton<AuditLoggingService>();
    }
}

// 2. Implementar controlador seguro
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SecureController : ControllerBase
{
    private readonly AuditLoggingService _auditLoggingService;
    private readonly ILogger<SecureController> _logger;

    [HttpPost("sensitive-action")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PerformSensitiveAction([FromBody] SensitiveActionRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // Log audit
        await _auditLoggingService.LogUserActionAsync(userId, "SensitiveAction", "SecureController", request);
        
        // Perform action
        var result = await PerformActionAsync(request);
        
        return Ok(result);
    }

    private async Task<object> PerformActionAsync(SensitiveActionRequest request)
    {
        // Implementar acción sensible
        return new { Success = true };
    }
}

// 3. Implementar middleware de seguridad
public class SecurityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SecurityHeadersMiddleware _securityHeaders;
    private readonly RateLimitingMiddleware _rateLimiting;

    public SecurityMiddleware(RequestDelegate next)
    {
        _next = next;
        _securityHeaders = new SecurityHeadersMiddleware(next, null);
        _rateLimiting = new RateLimitingMiddleware(next, null, null);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Aplicar security headers
        await _securityHeaders.InvokeAsync(context);
        
        // Aplicar rate limiting
        await _rateLimiting.InvokeAsync(context);
        
        await _next(context);
    }
}
```

## 📋 **Resumen de la Clase**

### **Conceptos Clave:**
- **Azure AD**: Autenticación empresarial
- **JWT**: Tokens de autenticación
- **Encriptación**: Protección de datos
- **Key Vault**: Gestión de secretos
- **Security Headers**: Protección HTTP
- **Rate Limiting**: Control de tráfico
- **Audit Logging**: Registro de auditoría

### **Próxima Clase:**
**Cost Optimization y Performance** - Optimización de costos y rendimiento

## 🎯 **Objetivos de Aprendizaje**

Al finalizar esta clase, serás capaz de:
- ✅ Implementar autenticación con Azure AD
- ✅ Configurar JWT tokens
- ✅ Encriptar datos sensibles
- ✅ Usar Azure Key Vault
- ✅ Implementar security headers
- ✅ Configurar rate limiting
- ✅ Implementar audit logging
