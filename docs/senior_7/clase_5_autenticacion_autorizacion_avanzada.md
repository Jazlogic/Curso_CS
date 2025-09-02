# 🔐 Clase 5: Autenticación y Autorización Avanzada

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 4: Sistema de Estados y Transiciones](../senior_7/clase_4_sistema_estados_transiciones.md)
- **🏠 Inicio del Módulo**: [Módulo 14: Plataformas Empresariales Reales](../senior_7/README.md)
- **➡️ Siguiente**: [Clase 6: Validaciones de Negocio Avanzadas](../senior_7/clase_6_validaciones_negocio_avanzadas.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** JWT con claims personalizados para permisos granulares
2. **Crear** sistemas de autorización basados en políticas
3. **Desarrollar** middleware de autenticación personalizado
4. **Diseñar** sistemas de permisos dinámicos
5. **Aplicar** patrones de seguridad avanzados

---

## 🔑 **JWT con Claims Personalizados**

### **Servicio JWT Avanzado**

```csharp
public interface IJwtService
{
    Task<string> GenerateTokenAsync(User user);
    Task<string> GenerateRefreshTokenAsync();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    Task<bool> ValidateTokenAsync(string token);
    Task<string> RefreshTokenAsync(string expiredToken, string refreshToken);
}

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IUserService _userService;
    private readonly ILogger<JwtService> _logger;

    public JwtService(IOptions<JwtSettings> jwtSettings, IUserService userService, ILogger<JwtService> logger)
    {
        _jwtSettings = jwtSettings.Value;
        _userService = userService;
        _logger = logger;
    }

    public async Task<string> GenerateTokenAsync(User user)
    {
        var userPermissions = await _userService.GetUserPermissionsAsync(user.Id);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("UserId", user.Id.ToString()),
            new Claim("UserType", user.UserType.ToString()),
            new Claim("IsVerified", user.IsVerified.ToString().ToLower()),
            new Claim("SubscriptionTier", user.SubscriptionTier.ToString())
        };

        foreach (var permission in userPermissions)
        {
            claims.Add(new Claim("Permission", permission));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateRefreshTokenAsync()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken || 
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }
}
```

---

## 🛡️ **Autorización Basada en Políticas**

### **Sistema de Políticas de Autorización**

```csharp
public class AuthorizationPolicies
{
    public const string RequireOrganizerRole = "RequireOrganizerRole";
    public const string RequireMusicianRole = "RequireMusicianRole";
    public const string RequireAdminRole = "RequireAdminRole";
    public const string CanManageRequests = "CanManageRequests";
    public const string CanViewMusicians = "CanViewMusicians";
    public const string CanCreateRequests = "CanCreateRequests";
}

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationPolicies.RequireOrganizerRole, policy =>
                policy.RequireRole("Organizador", "Admin", "SuperAdmin"));

            options.AddPolicy(AuthorizationPolicies.RequireMusicianRole, policy =>
                policy.RequireRole("Musico", "Admin", "SuperAdmin"));

            options.AddPolicy(AuthorizationPolicies.RequireAdminRole, policy =>
                policy.RequireRole("Admin", "SuperAdmin"));

            options.AddPolicy(AuthorizationPolicies.CanManageRequests, policy =>
                policy.RequireAssertion(context =>
                {
                    var user = context.User;
                    var hasRole = user.IsInRole("Admin") || user.IsInRole("SuperAdmin");
                    var hasPermission = user.HasClaim("Permission", "manage:requests");
                    return hasRole || hasPermission;
                }));

            options.AddPolicy(AuthorizationPolicies.CanViewMusicians, policy =>
                policy.RequireAssertion(context =>
                {
                    var user = context.User;
                    return user.IsInRole("Organizador") || 
                           user.IsInRole("Admin") || 
                           user.IsInRole("SuperAdmin") ||
                           user.HasClaim("Permission", "view:musicians");
                }));
        });
    }
}
```

---

## 🔒 **Middleware de Autenticación Personalizado**

### **Middleware de Claims Personalizados**

```csharp
public class ClaimsTransformationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IUserService _userService;
    private readonly ILogger<ClaimsTransformationMiddleware> _logger;

    public ClaimsTransformationMiddleware(RequestDelegate next, IUserService userService, ILogger<ClaimsTransformationMiddleware> logger)
    {
        _next = next;
        _userService = userService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userId, out var userIdGuid))
            {
                var userPermissions = await _userService.GetUserPermissionsAsync(userIdGuid);
                var userProfile = await _userService.GetUserProfileAsync(userIdGuid);

                var claimsIdentity = context.User.Identity as ClaimsIdentity;
                if (claimsIdentity != null)
                {
                    // Agregar claims personalizados
                    foreach (var permission in userPermissions)
                    {
                        if (!context.User.HasClaim("Permission", permission))
                        {
                            claimsIdentity.AddClaim(new Claim("Permission", permission));
                        }
                    }

                    // Agregar claims de perfil
                    if (userProfile != null)
                    {
                        var profileClaims = new[]
                        {
                            new Claim("ProfileComplete", userProfile.IsComplete.ToString().ToLower()),
                            new Claim("LastLogin", userProfile.LastLoginDate?.ToString("O") ?? ""),
                            new Claim("AccountStatus", userProfile.Status.ToString())
                        };

                        foreach (var claim in profileClaims)
                        {
                            if (!context.User.HasClaim(c => c.Type == claim.Type))
                            {
                                claimsIdentity.AddClaim(claim);
                            }
                        }
                    }
                }
            }
        }

        await _next(context);
    }
}
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: JWT con Claims**
```csharp
// Implementa un sistema JWT que:
// - Incluya claims personalizados para permisos
// - Implemente refresh tokens
// - Valide permisos en middleware
// - Maneje expiración y renovación automática
```

### **Ejercicio 2: Políticas de Autorización**
```csharp
// Crea políticas de autorización para:
// - Gestión de usuarios
// - Acceso a recursos
// - Operaciones CRUD
// - Validación de permisos granulares
```

### **Ejercicio 3: Middleware de Seguridad**
```csharp
// Implementa middleware para:
// - Validación de claims
// - Auditoría de acceso
// - Rate limiting
// - Logging de seguridad
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **🔑 JWT Avanzado**: Claims personalizados y refresh tokens
2. **🛡️ Políticas de Autorización**: Sistema granular de permisos
3. **🔒 Middleware Personalizado**: Transformación de claims en tiempo real
4. **🎯 Permisos Dinámicos**: Sistema flexible de autorización
5. **🔐 Seguridad Avanzada**: Patrones de autenticación robustos

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Validaciones de Negocio Avanzadas**, implementando reglas complejas y validaciones dinámicas.

---

**¡Has completado la quinta clase del Módulo 14! 🔐🛡️**

