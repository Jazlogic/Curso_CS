# 🛡️ **Clase 2: OWASP Top 10 y Vulnerabilidades Comunes**

## 🎯 **Objetivo de la Clase**
Dominar las 10 vulnerabilidades más críticas identificadas por OWASP y aprender a implementar protecciones específicas contra cada una de ellas en aplicaciones .NET.

## 📚 **Contenido de la Clase**

### **1. OWASP Top 10 - Análisis Detallado**

#### **1.1 A01:2021 - Broken Access Control**
```csharp
// Implementación de Access Control robusto
public class AccessControlService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<AccessControlService> _logger;
    
    public AccessControlService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        ILogger<AccessControlService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _logger = logger;
    }
    
    // Verificación de permisos a nivel de recurso
    public async Task<bool> CanAccessResource(string userId, string resourceId, string action)
    {
        try
        {
            // 1. Verificar si el usuario existe y está activo
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Access denied: User {UserId} not found or inactive", userId);
                return false;
            }
            
            // 2. Verificar permisos del usuario
            var userPermissions = await GetUserPermissions(userId);
            if (!userPermissions.Contains(action))
            {
                _logger.LogWarning("Access denied: User {UserId} lacks permission {Action}", userId, action);
                return false;
            }
            
            // 3. Verificar ownership del recurso
            var resource = await GetResourceById(resourceId);
            if (resource == null)
            {
                _logger.LogWarning("Access denied: Resource {ResourceId} not found", resourceId);
                return false;
            }
            
            // 4. Verificar si el usuario es propietario o tiene permisos globales
            if (resource.OwnerId != userId && !userPermissions.Contains("admin"))
            {
                _logger.LogWarning("Access denied: User {UserId} not owner of resource {ResourceId}", userId, resourceId);
                return false;
            }
            
            // 5. Log del acceso exitoso
            _logger.LogInformation("Access granted: User {UserId} accessed resource {ResourceId} with action {Action}", 
                userId, resourceId, action);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking access for user {UserId} to resource {ResourceId}", userId, resourceId);
            return false; // Fail secure
        }
    }
    
    // Implementación de RBAC (Role-Based Access Control)
    public async Task<bool> HasRole(string userId, string roleName)
    {
        var userRoles = await GetUserRoles(userId);
        return userRoles.Any(role => role.Name == roleName);
    }
    
    // Implementación de ABAC (Attribute-Based Access Control)
    public async Task<bool> CanAccessWithAttributes(string userId, string resourceId, Dictionary<string, string> context)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        var resource = await GetResourceById(resourceId);
        
        // Verificar atributos del usuario
        if (user.Department != context["department"])
        {
            return false;
        }
        
        // Verificar atributos del recurso
        if (resource.Classification == "confidential" && user.ClearanceLevel < 3)
        {
            return false;
        }
        
        // Verificar contexto (hora, ubicación, etc.)
        if (context.ContainsKey("time") && !IsBusinessHours(context["time"]))
        {
            return false;
        }
        
        return true;
    }
    
    private async Task<List<string>> GetUserPermissions(string userId)
    {
        var userRoles = await GetUserRoles(userId);
        var permissions = new List<string>();
        
        foreach (var role in userRoles)
        {
            var rolePermissions = await GetRolePermissions(role.Id);
            permissions.AddRange(rolePermissions);
        }
        
        return permissions.Distinct().ToList();
    }
    
    private async Task<List<Role>> GetUserRoles(string userId)
    {
        return await _roleRepository.GetByUserIdAsync(userId);
    }
    
    private async Task<List<string>> GetRolePermissions(string roleId)
    {
        return await _roleRepository.GetPermissionsByRoleIdAsync(roleId);
    }
    
    private async Task<Resource> GetResourceById(string resourceId)
    {
        // Implementar obtención del recurso
        return new Resource(); // Simplificado
    }
    
    private bool IsBusinessHours(string time)
    {
        // Verificar si es horario laboral
        return true; // Simplificado
    }
}

// Middleware para verificación automática de acceso
public class AccessControlMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AccessControlService _accessControlService;
    private readonly ILogger<AccessControlMiddleware> _logger;
    
    public AccessControlMiddleware(
        RequestDelegate next,
        AccessControlService accessControlService,
        ILogger<AccessControlMiddleware> logger)
    {
        _next = next;
        _accessControlService = accessControlService;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var resourceId = context.Request.RouteValues["id"]?.ToString();
        var action = context.Request.Method;
        
        if (userId != null && resourceId != null)
        {
            var canAccess = await _accessControlService.CanAccessResource(userId, resourceId, action);
            
            if (!canAccess)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Access Denied");
                return;
            }
        }
        
        await _next(context);
    }
}
```

#### **1.2 A02:2021 - Cryptographic Failures**
```csharp
// Implementación de criptografía segura
public class CryptographicService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CryptographicService> _logger;
    
    public CryptographicService(IConfiguration configuration, ILogger<CryptographicService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    
    // Hash seguro de contraseñas con salt
    public string HashPassword(string password)
    {
        // Usar BCrypt para hashing de contraseñas
        var saltRounds = 12; // Número de rondas de salt
        return BCrypt.Net.BCrypt.HashPassword(password, saltRounds);
    }
    
    // Verificación de contraseña
    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying password");
            return false;
        }
    }
    
    // Encriptación simétrica con AES
    public async Task<string> EncryptAsync(string plainText)
    {
        try
        {
            var key = GetEncryptionKey();
            var iv = GenerateIV();
            
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            
            using var encryptor = aes.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using var swEncrypt = new StreamWriter(csEncrypt);
            
            await swEncrypt.WriteAsync(plainText);
            await swEncrypt.FlushAsync();
            csEncrypt.FlushFinalBlock();
            
            var encrypted = msEncrypt.ToArray();
            var result = Convert.ToBase64String(iv.Concat(encrypted).ToArray());
            
            _logger.LogInformation("Data encrypted successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting data");
            throw new CryptographicException("Failed to encrypt data", ex);
        }
    }
    
    // Desencriptación simétrica
    public async Task<string> DecryptAsync(string cipherText)
    {
        try
        {
            var key = GetEncryptionKey();
            var fullCipher = Convert.FromBase64String(cipherText);
            
            var iv = new byte[16];
            var cipher = new byte[fullCipher.Length - 16];
            
            Array.Copy(fullCipher, 0, iv, 0, 16);
            Array.Copy(fullCipher, 16, cipher, 0, cipher.Length);
            
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            
            using var decryptor = aes.CreateDecryptor();
            using var msDecrypt = new MemoryStream(cipher);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            
            var result = await srDecrypt.ReadToEndAsync();
            
            _logger.LogInformation("Data decrypted successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting data");
            throw new CryptographicException("Failed to decrypt data", ex);
        }
    }
    
    // Generación de tokens seguros
    public string GenerateSecureToken(int length = 32)
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
    
    // Generación de IV seguro
    private byte[] GenerateIV()
    {
        using var rng = RandomNumberGenerator.Create();
        var iv = new byte[16];
        rng.GetBytes(iv);
        return iv;
    }
    
    // Obtención de clave de encriptación
    private byte[] GetEncryptionKey()
    {
        var keyString = _configuration["Encryption:Key"];
        if (string.IsNullOrEmpty(keyString))
        {
            throw new InvalidOperationException("Encryption key not configured");
        }
        
        return Convert.FromBase64String(keyString);
    }
}

// Configuración de encriptación en appsettings.json
/*
{
  "Encryption": {
    "Key": "base64-encoded-32-byte-key"
  }
}
*/
```

#### **1.3 A03:2021 - Injection**
```csharp
// Protección contra inyección de código
public class InjectionProtectionService
{
    private readonly ILogger<InjectionProtectionService> _logger;
    
    public InjectionProtectionService(ILogger<InjectionProtectionService> logger)
    {
        _logger = logger;
    }
    
    // Protección contra SQL Injection
    public class SqlInjectionProtection
    {
        private readonly IDbConnection _connection;
        
        public SqlInjectionProtection(IDbConnection connection)
        {
            _connection = connection;
        }
        
        // ✅ Método seguro con parámetros
        public async Task<List<User>> GetUsersSecure(string searchTerm, int page, int pageSize)
        {
            var sql = @"
                SELECT * FROM Users 
                WHERE Name LIKE @searchTerm 
                ORDER BY Name 
                OFFSET @offset ROWS 
                FETCH NEXT @pageSize ROWS ONLY";
            
            var parameters = new
            {
                searchTerm = $"%{searchTerm}%",
                offset = (page - 1) * pageSize,
                pageSize = pageSize
            };
            
            return (await _connection.QueryAsync<User>(sql, parameters)).ToList();
        }
        
        // ✅ Método seguro con Entity Framework
        public async Task<List<User>> GetUsersWithEF(string searchTerm, int page, int pageSize)
        {
            using var context = new ApplicationDbContext();
            
            return await context.Users
                .Where(u => u.Name.Contains(searchTerm))
                .OrderBy(u => u.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
    
    // Protección contra NoSQL Injection
    public class NoSqlInjectionProtection
    {
        private readonly IMongoCollection<User> _collection;
        
        public NoSqlInjectionProtection(IMongoCollection<User> collection)
        {
            _collection = collection;
        }
        
        // ✅ Método seguro con MongoDB
        public async Task<List<User>> GetUsersSecure(string searchTerm)
        {
            // Usar filtros de MongoDB en lugar de concatenación de strings
            var filter = Builders<User>.Filter.Regex(u => u.Name, new BsonRegularExpression(searchTerm, "i"));
            
            return await _collection.Find(filter).ToListAsync();
        }
        
        // ❌ VULNERABLE - No usar nunca
        public async Task<List<User>> GetUsersVulnerable(string searchTerm)
        {
            var query = $"{{ \"name\": {{ \"$regex\": \"{searchTerm}\", \"$options\": \"i\" }} }}";
            // Esto es vulnerable a NoSQL Injection
            return await _collection.Find(query).ToListAsync();
        }
    }
    
    // Protección contra Command Injection
    public class CommandInjectionProtection
    {
        private readonly ILogger<CommandInjectionProtection> _logger;
        
        public CommandInjectionProtection(ILogger<CommandInjectionProtection> logger)
        {
            _logger = logger;
        }
        
        // ✅ Método seguro - validar y sanitizar entrada
        public async Task<string> ExecuteSecureCommand(string command, string[] arguments)
        {
            // 1. Validar comando permitido
            var allowedCommands = new[] { "ls", "dir", "echo", "date" };
            if (!allowedCommands.Contains(command))
            {
                throw new ArgumentException($"Command '{command}' is not allowed");
            }
            
            // 2. Sanitizar argumentos
            var sanitizedArgs = arguments.Select(SanitizeArgument).ToArray();
            
            // 3. Ejecutar comando de forma segura
            using var process = new Process();
            process.StartInfo.FileName = command;
            process.StartInfo.Arguments = string.Join(" ", sanitizedArgs);
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            
            process.Start();
            
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync();
            
            if (process.ExitCode != 0)
            {
                _logger.LogError("Command failed: {Error}", error);
                throw new InvalidOperationException($"Command failed: {error}");
            }
            
            return output;
        }
        
        private string SanitizeArgument(string argument)
        {
            // Remover caracteres peligrosos
            var dangerousChars = new[] { ';', '&', '|', '`', '$', '(', ')', '<', '>', '"', '\'' };
            
            foreach (var c in dangerousChars)
            {
                argument = argument.Replace(c.ToString(), "");
            }
            
            return argument;
        }
    }
}
```

### **2. Implementación de Protecciones Específicas**

#### **2.1 A04:2021 - Insecure Design**
```csharp
// Diseño seguro desde el principio
public class SecureDesignService
{
    private readonly ILogger<SecureDesignService> _logger;
    
    public SecureDesignService(ILogger<SecureDesignService> logger)
    {
        _logger = logger;
    }
    
    // Implementación de Rate Limiting
    public class RateLimitingService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<RateLimitingService> _logger;
        
        public RateLimitingService(IMemoryCache cache, ILogger<RateLimitingService> logger)
        {
            _cache = cache;
            _logger = logger;
        }
        
        public async Task<bool> IsRateLimited(string identifier, int maxRequests, TimeSpan window)
        {
            var key = $"rate_limit_{identifier}";
            var currentCount = _cache.Get<int>(key);
            
            if (currentCount >= maxRequests)
            {
                _logger.LogWarning("Rate limit exceeded for {Identifier}", identifier);
                return true;
            }
            
            _cache.Set(key, currentCount + 1, window);
            return false;
        }
    }
    
    // Implementación de Input Validation
    public class InputValidationService
    {
        private readonly ILogger<InputValidationService> _logger;
        
        public InputValidationService(ILogger<InputValidationService> logger)
        {
            _logger = logger;
        }
        
        public ValidationResult ValidateInput<T>(T input, ValidationRules rules)
        {
            var result = new ValidationResult();
            
            foreach (var rule in rules.Rules)
            {
                var validationResult = rule.Validate(input);
                if (!validationResult.IsValid)
                {
                    result.AddError(validationResult.ErrorMessage);
                }
            }
            
            if (!result.IsValid)
            {
                _logger.LogWarning("Input validation failed: {Errors}", string.Join(", ", result.Errors));
            }
            
            return result;
        }
    }
    
    // Implementación de Business Logic Validation
    public class BusinessLogicValidator
    {
        private readonly ILogger<BusinessLogicValidator> _logger;
        
        public BusinessLogicValidator(ILogger<BusinessLogicValidator> logger)
        {
            _logger = logger;
        }
        
        public async Task<ValidationResult> ValidateBusinessRules<T>(T entity, List<IBusinessRule<T>> rules)
        {
            var result = new ValidationResult();
            
            foreach (var rule in rules)
            {
                var ruleResult = await rule.ValidateAsync(entity);
                if (!ruleResult.IsValid)
                {
                    result.AddError(ruleResult.ErrorMessage);
                }
            }
            
            if (!result.IsValid)
            {
                _logger.LogWarning("Business logic validation failed: {Errors}", string.Join(", ", result.Errors));
            }
            
            return result;
        }
    }
}

// Middleware de Rate Limiting
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
        var clientIp = context.Connection.RemoteIpAddress?.ToString();
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var identifier = userId ?? clientIp;
        
        if (await _rateLimitingService.IsRateLimited(identifier, 100, TimeSpan.FromMinutes(1)))
        {
            context.Response.StatusCode = 429;
            await context.Response.WriteAsync("Rate limit exceeded");
            return;
        }
        
        await _next(context);
    }
}
```

#### **2.2 A05:2021 - Security Misconfiguration**
```csharp
// Configuración segura de la aplicación
public class SecurityConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SecurityConfigurationService> _logger;
    
    public SecurityConfigurationService(IConfiguration configuration, ILogger<SecurityConfigurationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    
    // Validación de configuración de seguridad
    public async Task<SecurityConfigurationResult> ValidateSecurityConfiguration()
    {
        var result = new SecurityConfigurationResult();
        
        // 1. Verificar HTTPS
        if (!_configuration.GetValue<bool>("Security:RequireHttps"))
        {
            result.AddWarning("HTTPS is not required");
        }
        
        // 2. Verificar headers de seguridad
        var securityHeaders = _configuration.GetSection("Security:Headers").Get<Dictionary<string, string>>();
        if (securityHeaders == null || !securityHeaders.ContainsKey("X-Content-Type-Options"))
        {
            result.AddWarning("Security headers not properly configured");
        }
        
        // 3. Verificar configuración de CORS
        var corsOrigins = _configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (corsOrigins == null || corsOrigins.Contains("*"))
        {
            result.AddWarning("CORS configuration allows all origins");
        }
        
        // 4. Verificar configuración de cookies
        var cookieSecure = _configuration.GetValue<bool>("Security:Cookies:Secure");
        if (!cookieSecure)
        {
            result.AddWarning("Cookies are not configured as secure");
        }
        
        // 5. Verificar configuración de logging
        var logLevel = _configuration.GetValue<string>("Logging:LogLevel:Default");
        if (logLevel == "Debug" || logLevel == "Trace")
        {
            result.AddWarning("Logging level is too verbose for production");
        }
        
        return result;
    }
    
    // Configuración automática de seguridad
    public void ConfigureSecurity(IServiceCollection services)
    {
        // Configuración de HTTPS
        services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(365);
        });
        
        // Configuración de CORS
        services.AddCors(options =>
        {
            options.AddPolicy("SecurePolicy", policy =>
            {
                var allowedOrigins = _configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });
        
        // Configuración de cookies
        services.Configure<CookiePolicyOptions>(options =>
        {
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = SameSiteMode.Strict;
            options.Secure = CookieSecurePolicy.Always;
        });
        
        // Configuración de logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddEventLog();
            builder.SetMinimumLevel(LogLevel.Information);
        });
    }
}

// Configuración de seguridad en appsettings.json
/*
{
  "Security": {
    "RequireHttps": true,
    "Headers": {
      "X-Content-Type-Options": "nosniff",
      "X-Frame-Options": "DENY",
      "X-XSS-Protection": "1; mode=block"
    },
    "Cookies": {
      "Secure": true,
      "SameSite": "Strict"
    }
  },
  "Cors": {
    "AllowedOrigins": ["https://trusted-domain.com"]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
*/
```

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Implementar Access Control**
```csharp
// Crear un sistema de control de acceso que implemente RBAC y ABAC
public class AdvancedAccessControlService
{
    public async Task<bool> CanPerformAction(string userId, string action, string resourceId, Dictionary<string, string> context)
    {
        // Implementar lógica de control de acceso avanzada
        // 1. Verificar roles del usuario
        // 2. Verificar permisos específicos
        // 3. Verificar atributos del contexto
        // 4. Verificar ownership del recurso
    }
}
```

### **Ejercicio 2: Implementar Protección contra Injection**
```csharp
// Crear un servicio que proteja contra todos los tipos de inyección
public class ComprehensiveInjectionProtection
{
    public async Task<T> ExecuteSecureQuery<T>(string query, object parameters)
    {
        // Implementar protección contra SQL Injection
        // Implementar protección contra NoSQL Injection
        // Implementar protección contra Command Injection
    }
}
```

## 📝 **Resumen de la Clase**

### **Conceptos Clave Aprendidos:**
1. **OWASP Top 10**: Análisis detallado de las vulnerabilidades más críticas
2. **Access Control**: Implementación de RBAC y ABAC
3. **Cryptographic Failures**: Uso seguro de criptografía
4. **Injection Protection**: Protección contra múltiples tipos de inyección
5. **Secure Design**: Principios de diseño seguro
6. **Security Configuration**: Configuración segura de aplicaciones

### **Próxima Clase:**
En la siguiente clase exploraremos **Authentication y Authorization avanzados**, incluyendo OAuth2, OpenID Connect y JWT.

---

## 🔗 **Recursos Adicionales**

- [OWASP Top 10 2021](https://owasp.org/Top10/)
- [OWASP Cheat Sheets](https://cheatsheetseries.owasp.org/)
- [.NET Security Documentation](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [BCrypt.NET](https://github.com/BcryptNet/bcrypt.net)
- [Security Headers](https://securityheaders.com/)
