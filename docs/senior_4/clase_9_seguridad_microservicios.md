# Clase 9: Seguridad en Microservicios

## Navegación
- [← Clase 8: Despliegue y Orquestación](clase_8_despliegue_orquestacion.md)
- [Clase 10: Proyecto Final →](clase_10_proyecto_final.md)
- [← Volver al README del módulo](README.md)
- [← Volver al módulo anterior (senior_3)](../senior_3/README.md)
- [→ Ir al siguiente módulo (senior_5)](../senior_5/README.md)

## Objetivos de Aprendizaje
- Implementar autenticación JWT
- Configurar autorización basada en roles
- Implementar encriptación de datos
- Configurar auditoría de seguridad
- Proteger contra ataques comunes

## Contenido Teórico

### 1. Autenticación JWT

```csharp
// Servicio de autenticación
public class AuthService : IAuthService
{
    public async Task<AuthResult> AuthenticateAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            return AuthResult.Failure("Invalid credentials");

        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken(user);
        
        return AuthResult.Success(accessToken, refreshToken, user);
    }

    private string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
        
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// Middleware JWT
public class JwtMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"]
            .FirstOrDefault()?.Split(" ").Last();

        if (token != null)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]);
                
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value),
                    new Claim(ClaimTypes.Email, jwtToken.Claims.First(x => x.Type == ClaimTypes.Email).Value),
                    new Claim(ClaimTypes.Role, jwtToken.Claims.First(x => x.Type == ClaimTypes.Role).Value)
                };

                context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
            }
            catch
            {
                context.Response.StatusCode = 401;
                return;
            }
        }

        await _next(context);
    }
}
```

### 2. Autorización Basada en Roles

```csharp
// Atributo de autorización
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _permission;

    public RequirePermissionAttribute(string permission)
    {
        _permission = permission;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        
        if (!user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var permissions = user.Claims
            .Where(c => c.Type == "permissions")
            .SelectMany(c => c.Value.Split(','))
            .ToList();

        if (!permissions.Contains(_permission))
        {
            context.Result = new ForbidResult();
        }
    }
}

// Controlador con autorización
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    [HttpGet]
    [RequirePermission("users.read")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpPost]
    [RequirePermission("users.write")]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserRequest request)
    {
        var user = await _userService.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpDelete("{id}")]
    [RequirePermission("users.delete")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        await _userService.DeleteUserAsync(id);
        return NoContent();
    }
}
```

### 3. Encriptación y Hashing

```csharp
// Servicio de encriptación
public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public EncryptionService(IConfiguration configuration)
    {
        var secretKey = configuration["Encryption:Key"];
        var secretIv = configuration["Encryption:IV"];
        
        _key = Convert.FromBase64String(secretKey);
        _iv = Convert.FromBase64String(secretIv);
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        using var encryptor = aes.CreateEncryptor();
        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        using var swEncrypt = new StreamWriter(csEncrypt);

        swEncrypt.Write(plainText);
        swEncrypt.Flush();
        csEncrypt.FlushFinalBlock();

        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using var decryptor = aes.CreateDecryptor();
            using var msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText));
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);

            return srDecrypt.ReadToEnd();
        }
        catch
        {
            return string.Empty;
        }
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
```

### 4. Auditoría de Seguridad

```csharp
// Servicio de auditoría
public class AuditService : IAuditService
{
    public async Task LogSecurityEventAsync(SecurityEvent securityEvent)
    {
        try
        {
            securityEvent.Timestamp = DateTime.UtcNow;
            securityEvent.Id = Guid.NewGuid();
            
            await _auditRepository.SaveSecurityEventAsync(securityEvent);
            
            _logger.LogInformation(
                "Security event logged: {EventType} for user {UserId}",
                securityEvent.EventType,
                securityEvent.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging security event");
        }
    }
}

// Middleware de auditoría
public class AuditMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        var originalBodyStream = context.Response.Body;

        try
        {
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context);

            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalBodyStream);

            await LogAuditEvent(context, startTime, "SUCCESS");
        }
        catch (Exception ex)
        {
            await LogAuditEvent(context, startTime, "FAILURE", ex.Message);
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task LogAuditEvent(HttpContext context, DateTime startTime, string status, string errorMessage = null)
    {
        var user = context.User?.Identity?.Name ?? "anonymous";
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var path = context.Request.Path;
        var method = context.Request.Method;
        var statusCode = context.Response.StatusCode;
        var duration = DateTime.UtcNow - startTime;

        var securityEvent = new SecurityEvent
        {
            EventType = "API_ACCESS",
            UserId = userId != null ? Guid.Parse(userId) : (Guid?)null,
            UserName = user,
            Resource = path,
            Action = method,
            Status = status,
            StatusCode = statusCode,
            Duration = duration,
            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
            UserAgent = context.Request.Headers["User-Agent"].ToString(),
            ErrorMessage = errorMessage
        };

        await _auditService.LogSecurityEventAsync(securityEvent);
    }
}
```

### 5. Protección contra Ataques

```csharp
// Rate limiting
public class RateLimitingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientId(context);
        var endpoint = context.Request.Path;

        if (await IsRateLimitExceeded(clientId, endpoint))
        {
            context.Response.StatusCode = 429;
            await context.Response.WriteAsync("Rate limit exceeded");
            return;
        }

        await _next(context);
    }

    private string GetClientId(HttpContext context)
    {
        var user = context.User?.Identity?.Name;
        if (!string.IsNullOrEmpty(user))
            return $"user_{user}";

        return $"ip_{context.Connection.RemoteIpAddress}";
    }

    private async Task<bool> IsRateLimitExceeded(string clientId, string endpoint)
    {
        var key = $"rate_limit_{clientId}_{endpoint}";
        var currentCount = await _cache.GetOrCreateAsync(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return Task.FromResult(0);
        });

        if (currentCount >= GetRateLimit(endpoint))
            return true;

        _cache.Set(key, currentCount + 1, TimeSpan.FromMinutes(1));
        return false;
    }

    private int GetRateLimit(string endpoint) => endpoint switch
    {
        "/api/auth/login" => 5,
        "/api/users" => 100,
        _ => 1000
    };
}

// Validación de entrada
public class InputValidationMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method == "POST" || context.Request.Method == "PUT")
        {
            var originalBodyStream = context.Request.Body;
            using var memoryStream = new MemoryStream();
            await context.Request.Body.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var requestBody = await new StreamReader(memoryStream).ReadToEndAsync();
            
            if (ContainsSuspiciousContent(requestBody))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid input detected");
                return;
            }

            memoryStream.Position = 0;
            context.Request.Body = memoryStream;
        }

        await _next(context);
    }

    private bool ContainsSuspiciousContent(string content)
    {
        if (string.IsNullOrEmpty(content))
            return false;

        var suspiciousPatterns = new[]
        {
            "<script>", "javascript:", "onload=", "onerror=", "eval(", "document.cookie"
        };

        return suspiciousPatterns.Any(pattern => 
            content.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }
}
```

## Ejercicios Prácticos

### Ejercicio 1: Autenticación JWT
Implementa autenticación JWT con refresh tokens.

### Ejercicio 2: Autorización
Crea sistema de autorización basado en permisos.

### Ejercicio 3: Auditoría
Implementa logging de eventos de seguridad.

## Proyecto Integrador
Implementa sistema de seguridad completo con:
- Autenticación JWT
- Autorización basada en roles
- Encriptación de datos
- Auditoría de eventos
- Protección contra ataques

## Recursos Adicionales
- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [JWT Best Practices](https://auth0.com/blog/a-look-at-the-latest-draft-for-jwt-bcp/)
- [OWASP Guidelines](https://owasp.org/www-project-top-ten/)
- [BCrypt.NET](https://github.com/BcryptNet/bcrypt.net)
