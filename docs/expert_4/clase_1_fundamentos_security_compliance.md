# 🔒 **Clase 1: Fundamentos de Security y Compliance**

## 🎯 **Objetivo de la Clase**
Comprender los fundamentos de seguridad en aplicaciones .NET, principios de compliance y las mejores prácticas de seguridad que todo desarrollador senior debe dominar.

## 📚 **Contenido de la Clase**

### **1. Fundamentos de Seguridad en .NET**

#### **1.1 Principios de Seguridad**
```csharp
// Principios fundamentales de seguridad
public class SecurityPrinciples
{
    // 1. Defense in Depth - Múltiples capas de seguridad
    public class DefenseInDepth
    {
        // Validación en múltiples capas
        public bool ValidateUserInput(string input)
        {
            // Capa 1: Validación en el cliente
            if (string.IsNullOrEmpty(input)) return false;
            
            // Capa 2: Validación en el servidor
            if (input.Length > 100) return false;
            
            // Capa 3: Sanitización
            return SanitizeInput(input);
        }
        
        private bool SanitizeInput(string input)
        {
            // Remover caracteres peligrosos
            var sanitized = input.Replace("<", "&lt;")
                               .Replace(">", "&gt;")
                               .Replace("\"", "&quot;")
                               .Replace("'", "&#x27;");
            
            return !ContainsMaliciousPatterns(sanitized);
        }
        
        private bool ContainsMaliciousPatterns(string input)
        {
            var maliciousPatterns = new[]
            {
                "script", "javascript:", "vbscript:", "onload=", "onerror="
            };
            
            return maliciousPatterns.Any(pattern => 
                input.ToLower().Contains(pattern));
        }
    }
    
    // 2. Principle of Least Privilege
    public class LeastPrivilege
    {
        // Usuarios con permisos mínimos necesarios
        public class UserPermissions
        {
            public string UserId { get; set; }
            public List<string> Permissions { get; set; } = new();
            
            public bool HasPermission(string permission)
            {
                return Permissions.Contains(permission);
            }
            
            public void GrantPermission(string permission)
            {
                if (!Permissions.Contains(permission))
                {
                    Permissions.Add(permission);
                }
            }
            
            public void RevokePermission(string permission)
            {
                Permissions.Remove(permission);
            }
        }
    }
    
    // 3. Fail Secure - Fallar de manera segura
    public class FailSecure
    {
        public bool AuthenticateUser(string username, string password)
        {
            try
            {
                // Intentar autenticación
                var user = GetUserByUsername(username);
                if (user == null)
                {
                    // Fallar de manera segura - no revelar información
                    LogFailedAttempt(username, "User not found");
                    return false;
                }
                
                var isValidPassword = VerifyPassword(password, user.PasswordHash);
                if (!isValidPassword)
                {
                    // Fallar de manera segura
                    LogFailedAttempt(username, "Invalid password");
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                // Fallar de manera segura en caso de error
                LogError(ex);
                return false;
            }
        }
        
        private void LogFailedAttempt(string username, string reason)
        {
            // Log sin exponer información sensible
            Console.WriteLine($"Authentication failed for user: {username}, Reason: {reason}");
        }
        
        private void LogError(Exception ex)
        {
            // Log de errores sin exponer detalles internos
            Console.WriteLine($"Authentication error: {ex.Message}");
        }
    }
}
```

#### **1.2 Tipos de Ataques Comunes**
```csharp
// Protección contra ataques comunes
public class AttackProtection
{
    // 1. SQL Injection Protection
    public class SqlInjectionProtection
    {
        private readonly IDbConnection _connection;
        
        public SqlInjectionProtection(IDbConnection connection)
        {
            _connection = connection;
        }
        
        // ❌ VULNERABLE - No usar nunca
        public List<User> GetUsersVulnerable(string searchTerm)
        {
            var sql = $"SELECT * FROM Users WHERE Name LIKE '%{searchTerm}%'";
            // Esto es vulnerable a SQL Injection
            return _connection.Query<User>(sql).ToList();
        }
        
        // ✅ SEGURO - Usar parámetros
        public List<User> GetUsersSecure(string searchTerm)
        {
            var sql = "SELECT * FROM Users WHERE Name LIKE @searchTerm";
            var parameters = new { searchTerm = $"%{searchTerm}%" };
            return _connection.Query<User>(sql, parameters).ToList();
        }
        
        // ✅ SEGURO - Usar Entity Framework
        public List<User> GetUsersWithEF(string searchTerm)
        {
            using var context = new ApplicationDbContext();
            return context.Users
                .Where(u => u.Name.Contains(searchTerm))
                .ToList();
        }
    }
    
    // 2. XSS Protection
    public class XssProtection
    {
        // Sanitización de HTML
        public string SanitizeHtml(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            
            // Usar HtmlSanitizer para limpiar HTML
            var sanitizer = new HtmlSanitizer();
            return sanitizer.Sanitize(input);
        }
        
        // Encoding para prevenir XSS
        public string EncodeForHtml(string input)
        {
            return System.Web.HttpUtility.HtmlEncode(input);
        }
        
        // Encoding para JavaScript
        public string EncodeForJavaScript(string input)
        {
            return System.Web.HttpUtility.JavaScriptStringEncode(input);
        }
    }
    
    // 3. CSRF Protection
    public class CsrfProtection
    {
        private readonly IMemoryCache _cache;
        
        public CsrfProtection(IMemoryCache cache)
        {
            _cache = cache;
        }
        
        public string GenerateCsrfToken(string userId)
        {
            var token = Guid.NewGuid().ToString();
            var key = $"csrf_{userId}";
            
            // Almacenar token en cache con expiración
            _cache.Set(key, token, TimeSpan.FromMinutes(30));
            
            return token;
        }
        
        public bool ValidateCsrfToken(string userId, string token)
        {
            var key = $"csrf_{userId}";
            var storedToken = _cache.Get<string>(key);
            
            if (storedToken == null) return false;
            
            return storedToken == token;
        }
    }
}
```

### **2. Compliance y Regulaciones**

#### **2.1 GDPR (General Data Protection Regulation)**
```csharp
// Implementación de GDPR
public class GdprCompliance
{
    public class DataSubject
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public bool ConsentGiven { get; set; }
        public DateTime? ConsentDate { get; set; }
    }
    
    public class GdprService
    {
        private readonly IRepository<DataSubject> _repository;
        private readonly ILogger<GdprService> _logger;
        
        public GdprService(IRepository<DataSubject> repository, ILogger<GdprService> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        
        // Right to Access - Derecho de acceso
        public async Task<DataSubject> GetDataSubjectData(string subjectId)
        {
            var data = await _repository.GetByIdAsync(subjectId);
            
            if (data == null)
            {
                throw new DataSubjectNotFoundException($"Data subject {subjectId} not found");
            }
            
            // Log del acceso
            _logger.LogInformation("Data subject {SubjectId} accessed their data", subjectId);
            
            return data;
        }
        
        // Right to Rectification - Derecho de rectificación
        public async Task UpdateDataSubjectData(string subjectId, DataSubject updatedData)
        {
            var existingData = await _repository.GetByIdAsync(subjectId);
            
            if (existingData == null)
            {
                throw new DataSubjectNotFoundException($"Data subject {subjectId} not found");
            }
            
            // Actualizar datos
            existingData.Name = updatedData.Name;
            existingData.Email = updatedData.Email;
            
            await _repository.UpdateAsync(existingData);
            
            // Log de la modificación
            _logger.LogInformation("Data subject {SubjectId} data updated", subjectId);
        }
        
        // Right to Erasure - Derecho al olvido
        public async Task DeleteDataSubjectData(string subjectId)
        {
            var data = await _repository.GetByIdAsync(subjectId);
            
            if (data == null)
            {
                throw new DataSubjectNotFoundException($"Data subject {subjectId} not found");
            }
            
            // Soft delete - marcar como eliminado
            data.DeletedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(data);
            
            // Log de la eliminación
            _logger.LogInformation("Data subject {SubjectId} data deleted", subjectId);
        }
        
        // Right to Portability - Derecho a la portabilidad
        public async Task<string> ExportDataSubjectData(string subjectId)
        {
            var data = await _repository.GetByIdAsync(subjectId);
            
            if (data == null)
            {
                throw new DataSubjectNotFoundException($"Data subject {subjectId} not found");
            }
            
            // Exportar en formato JSON
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            // Log de la exportación
            _logger.LogInformation("Data subject {SubjectId} data exported", subjectId);
            
            return json;
        }
        
        // Consent Management
        public async Task<bool> GiveConsent(string subjectId, bool consent)
        {
            var data = await _repository.GetByIdAsync(subjectId);
            
            if (data == null)
            {
                throw new DataSubjectNotFoundException($"Data subject {subjectId} not found");
            }
            
            data.ConsentGiven = consent;
            data.ConsentDate = DateTime.UtcNow;
            
            await _repository.UpdateAsync(data);
            
            // Log del consentimiento
            _logger.LogInformation("Data subject {SubjectId} consent updated to {Consent}", 
                subjectId, consent);
            
            return consent;
        }
    }
}
```

#### **2.2 HIPAA (Health Insurance Portability and Accountability Act)**
```csharp
// Implementación de HIPAA
public class HipaaCompliance
{
    public class ProtectedHealthInformation
    {
        public string Id { get; set; }
        public string PatientId { get; set; }
        public string MedicalRecordNumber { get; set; }
        public string Diagnosis { get; set; }
        public string Treatment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public bool IsEncrypted { get; set; }
    }
    
    public class HipaaService
    {
        private readonly IEncryptionService _encryptionService;
        private readonly IAuditService _auditService;
        private readonly ILogger<HipaaService> _logger;
        
        public HipaaService(
            IEncryptionService encryptionService,
            IAuditService auditService,
            ILogger<HipaaService> logger)
        {
            _encryptionService = encryptionService;
            _auditService = auditService;
            _logger = logger;
        }
        
        // Administrative Safeguards
        public async Task<ProtectedHealthInformation> CreatePHI(
            ProtectedHealthInformation phi, string userId)
        {
            // Encriptar datos sensibles
            phi.Diagnosis = await _encryptionService.EncryptAsync(phi.Diagnosis);
            phi.Treatment = await _encryptionService.EncryptAsync(phi.Treatment);
            phi.IsEncrypted = true;
            phi.CreatedBy = userId;
            
            // Audit log
            await _auditService.LogAccessAsync(
                userId, 
                "CREATE_PHI", 
                phi.PatientId, 
                "PHI record created");
            
            _logger.LogInformation("PHI record created for patient {PatientId} by user {UserId}", 
                phi.PatientId, userId);
            
            return phi;
        }
        
        // Physical Safeguards
        public async Task<ProtectedHealthInformation> AccessPHI(
            string phiId, string userId, string reason)
        {
            // Verificar autorización
            if (!await HasAccessToPHI(userId, phiId))
            {
                throw new UnauthorizedAccessException("User not authorized to access this PHI");
            }
            
            // Audit log
            await _auditService.LogAccessAsync(
                userId, 
                "ACCESS_PHI", 
                phiId, 
                reason);
            
            _logger.LogInformation("PHI {PhiId} accessed by user {UserId} for reason: {Reason}", 
                phiId, userId, reason);
            
            // Retornar datos (desencriptados si es necesario)
            return await GetPHIById(phiId);
        }
        
        // Technical Safeguards
        public async Task<bool> HasAccessToPHI(string userId, string phiId)
        {
            // Implementar lógica de autorización
            // Verificar roles, permisos, etc.
            return await CheckUserPermissions(userId, phiId);
        }
        
        private async Task<bool> CheckUserPermissions(string userId, string phiId)
        {
            // Lógica de verificación de permisos
            // Esto dependería de tu sistema de autorización
            return true; // Simplificado para el ejemplo
        }
        
        private async Task<ProtectedHealthInformation> GetPHIById(string phiId)
        {
            // Obtener PHI de la base de datos
            // Desencriptar si es necesario
            return new ProtectedHealthInformation(); // Simplificado
        }
    }
}
```

### **3. Security Headers y Configuración**

#### **3.1 Configuración de Security Headers**
```csharp
// Configuración de security headers
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
        var csp = "default-src 'self'; " +
                 "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                 "style-src 'self' 'unsafe-inline'; " +
                 "img-src 'self' data: https:; " +
                 "font-src 'self' data:; " +
                 "connect-src 'self'; " +
                 "frame-ancestors 'none';";
        
        context.Response.Headers.Add("Content-Security-Policy", csp);
        
        // Strict Transport Security (solo en HTTPS)
        if (context.Request.IsHttps)
        {
            context.Response.Headers.Add("Strict-Transport-Security", 
                "max-age=31536000; includeSubDomains; preload");
        }
        
        await _next(context);
    }
}

// Extensión para registrar el middleware
public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
```

#### **3.2 Configuración en Program.cs**
```csharp
// Configuración de seguridad en Program.cs
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Configuración de seguridad
        builder.Services.AddSecurityServices();
        
        var app = builder.Build();
        
        // Middleware de seguridad
        app.UseSecurityHeaders();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.Run();
    }
}

// Extensión para servicios de seguridad
public static class SecurityServiceExtensions
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        // Configuración de CORS
        services.AddCors(options =>
        {
            options.AddPolicy("SecurePolicy", policy =>
            {
                policy.WithOrigins("https://trusted-domain.com")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });
        
        // Configuración de HTTPS
        services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(365);
        });
        
        // Configuración de cookies seguras
        services.Configure<CookiePolicyOptions>(options =>
        {
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = SameSiteMode.Strict;
            options.Secure = CookieSecurePolicy.Always;
        });
        
        return services;
    }
}
```

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Implementar Validación de Entrada Segura**
```csharp
// Crear un servicio de validación que implemente defense in depth
public class SecureInputValidator
{
    public ValidationResult ValidateUserInput(string input, InputType type)
    {
        // Implementar validación en múltiples capas
        // 1. Validación básica
        // 2. Sanitización
        // 3. Verificación de patrones maliciosos
        // 4. Validación específica por tipo
    }
}

public enum InputType
{
    Email,
    Username,
    Password,
    GeneralText,
    HtmlContent
}
```

### **Ejercicio 2: Implementar Logging de Seguridad**
```csharp
// Crear un sistema de logging de seguridad
public class SecurityLogger
{
    public void LogSecurityEvent(SecurityEvent securityEvent)
    {
        // Implementar logging de eventos de seguridad
        // Incluir información relevante sin exponer datos sensibles
    }
}

public class SecurityEvent
{
    public string EventType { get; set; }
    public string UserId { get; set; }
    public string IpAddress { get; set; }
    public DateTime Timestamp { get; set; }
    public string Details { get; set; }
}
```

## 📝 **Resumen de la Clase**

### **Conceptos Clave Aprendidos:**
1. **Principios de Seguridad**: Defense in Depth, Least Privilege, Fail Secure
2. **Protección contra Ataques**: SQL Injection, XSS, CSRF
3. **Compliance**: GDPR, HIPAA y sus implementaciones
4. **Security Headers**: Configuración de headers de seguridad
5. **Logging de Seguridad**: Auditoría y monitoreo

### **Próxima Clase:**
En la siguiente clase exploraremos **OWASP Top 10** y cómo implementar protecciones específicas contra las vulnerabilidades más comunes en aplicaciones web.

---

## 🔗 **Recursos Adicionales**

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [GDPR Compliance Guide](https://gdpr.eu/)
- [HIPAA Compliance](https://www.hhs.gov/hipaa/index.html)
- [.NET Security Best Practices](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [Security Headers](https://securityheaders.com/)
