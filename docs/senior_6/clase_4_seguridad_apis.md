# 🚀 Clase 4: Seguridad de APIs y Microservicios

## 🧭 Navegación
- **⬅️ Anterior**: [Clase 3: Seguridad en Aplicaciones .NET](clase_3_seguridad_aplicaciones.md)
- **🏠 [Volver al Módulo 6](../README.md)**
- **➡️ Siguiente**: [Clase 5: Containerización con Docker](clase_5_containerizacion_docker.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)**

---

## 🎯 Objetivos de la Clase

Al finalizar esta clase, serás capaz de:
- Implementar JWT y OAuth 2.0 avanzado
- Configurar rate limiting y protección CSRF
- Implementar API security headers
- Asegurar microservicios

---

## 📚 Contenido Teórico

### 4.1 JWT y OAuth 2.0 Avanzado

#### Servicio JWT Avanzado

```csharp
public class AdvancedJwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdvancedJwtService> _logger;
    
    public AdvancedJwtService(IConfiguration configuration, ILogger<AdvancedJwtService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    
    public string GenerateToken(User user, IEnumerable<string> roles, Dictionary<string, string> customClaims = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("jti", Guid.NewGuid().ToString()), // JWT ID
            new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()), // Issued at
            new Claim("sub", user.Id.ToString()) // Subject
        };
        
        // Agrega roles
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        
        // Agrega claims personalizados
        if (customClaims != null)
        {
            foreach (var claim in customClaims)
            {
                claims.Add(new Claim(claim.Key, claim.Value));
            }
        }
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            notBefore: DateTime.UtcNow,
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
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
            
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "JWT validation failed");
            throw;
        }
    }
}
```

#### OAuth 2.0 Implementation

```csharp
public class OAuth2Service : IOAuth2Service
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    
    public async Task<OAuth2TokenResponse> GetAccessTokenAsync(string authorizationCode, string redirectUri)
    {
        var client = _httpClientFactory.CreateClient("OAuth2");
        
        var request = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", authorizationCode),
            new KeyValuePair<string, string>("redirect_uri", redirectUri),
            new KeyValuePair<string, string>("client_id", _configuration["OAuth2:ClientId"]),
            new KeyValuePair<string, string>("client_secret", _configuration["OAuth2:ClientSecret"])
        });
        
        var response = await client.PostAsync("/token", request);
        var content = await response.Content.ReadAsStringAsync();
        
        return JsonSerializer.Deserialize<OAuth2TokenResponse>(content);
    }
    
    public async Task<OAuth2TokenResponse> RefreshTokenAsync(string refreshToken)
    {
        var client = _httpClientFactory.CreateClient("OAuth2");
        
        var request = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", refreshToken),
            new KeyValuePair<string, string>("client_id", _configuration["OAuth2:ClientId"]),
            new KeyValuePair<string, string>("client_secret", _configuration["OAuth2:ClientSecret"])
        });
        
        var response = await client.PostAsync("/token", request);
        var content = await response.Content.ReadAsStringAsync();
        
        return JsonSerializer.Deserialize<OAuth2TokenResponse>(content);
    }
}
```

### 4.2 Rate Limiting y Protección CSRF

#### Rate Limiting Avanzado

```csharp
public class AdvancedRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AdvancedRateLimitingMiddleware> _logger;
    
    public AdvancedRateLimitingMiddleware(RequestDelegate next, IMemoryCache cache, ILogger<AdvancedRateLimitingMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var endpoint = context.Request.Path;
        
        var key = $"rate_limit:{clientId}:{endpoint}";
        var limit = GetRateLimit(endpoint);
        
        if (!await CheckRateLimitAsync(key, limit))
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientId} on endpoint {Endpoint}", clientId, endpoint);
            context.Response.StatusCode = 429; // Too Many Requests
            context.Response.Headers.Add("Retry-After", "60");
            await context.Response.WriteAsync("Rate limit exceeded");
            return;
        }
        
        await _next(context);
    }
    
    private string GetClientIdentifier(HttpContext context)
    {
        // Identifica cliente por IP, API key, o user ID
        var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
        if (!string.IsNullOrEmpty(apiKey))
            return $"api:{apiKey}";
        
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
            return $"user:{userId}";
        
        return $"ip:{context.Connection.RemoteIpAddress}";
    }
    
    private RateLimit GetRateLimit(string endpoint)
    {
        return endpoint switch
        {
            "/api/auth/login" => new RateLimit { Requests = 5, Window = TimeSpan.FromMinutes(15) },
            "/api/users" => new RateLimit { Requests = 100, Window = TimeSpan.FromMinutes(1) },
            _ => new RateLimit { Requests = 1000, Window = TimeSpan.FromMinutes(1) }
        };
    }
    
    private async Task<bool> CheckRateLimitAsync(string key, RateLimit limit)
    {
        var current = await _cache.GetOrCreateAsync(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = limit.Window;
            return Task.FromResult(new RateLimitInfo { Count = 0, ResetTime = DateTime.UtcNow.Add(limit.Window) });
        });
        
        if (current.Count >= limit.Requests)
            return false;
        
        current.Count++;
        return true;
    }
}

public class RateLimit
{
    public int Requests { get; set; }
    public TimeSpan Window { get; set; }
}

public class RateLimitInfo
{
    public int Count { get; set; }
    public DateTime ResetTime { get; set; }
}
```

#### CSRF Protection para APIs

```csharp
public class ApiCsrfProtectionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiCsrfProtectionMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        if (IsModifyingRequest(context.Request))
        {
            var token = context.Request.Headers["X-CSRF-Token"].FirstOrDefault();
            var sessionToken = context.Session.GetString("CSRFToken");
            
            if (string.IsNullOrEmpty(token) || token != sessionToken)
            {
                _logger.LogWarning("CSRF token validation failed");
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("CSRF token validation failed");
                return;
            }
        }
        
        await _next(context);
    }
    
    private bool IsModifyingRequest(HttpRequest request)
    {
        return new[] { "POST", "PUT", "PATCH", "DELETE" }.Contains(request.Method);
    }
}
```

### 4.3 API Security Headers

#### Security Headers Middleware

```csharp
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Headers de seguridad estándar
        context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
        context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'");
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Add("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
        
        await _next(context);
    }
}
```

### 4.4 Seguridad en Microservicios

#### API Gateway Security

```csharp
public class SecureApiGateway
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IJwtService _jwtService;
    
    public async Task<HttpResponseMessage> ForwardRequestAsync(HttpRequest request, string serviceName)
    {
        var client = _httpClientFactory.CreateClient(serviceName);
        
        // Valida JWT token
        var token = ExtractToken(request);
        if (!string.IsNullOrEmpty(token))
        {
            var principal = _jwtService.ValidateToken(token);
            // Agrega claims al request
            request.Headers.Add("X-User-Id", principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            request.Headers.Add("X-User-Roles", string.Join(",", principal.FindAll(ClaimTypes.Role).Select(c => c.Value)));
        }
        
        // Agrega headers de seguridad
        request.Headers.Add("X-Forwarded-For", request.HttpContext.Connection.RemoteIpAddress?.ToString());
        request.Headers.Add("X-Request-Id", Guid.NewGuid().ToString());
        
        // Reenvía el request al microservicio
        var requestMessage = new HttpRequestMessage
        {
            Method = new HttpMethod(request.Method),
            RequestUri = new Uri($"{client.BaseAddress}{request.Path}{request.QueryString}"),
            Content = new StreamContent(request.Body)
        };
        
        // Copia headers
        foreach (var header in request.Headers)
        {
            requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }
        
        return await client.SendAsync(requestMessage);
    }
    
    private string ExtractToken(HttpRequest request)
    {
        var authHeader = request.Headers["Authorization"].FirstOrDefault();
        return authHeader?.StartsWith("Bearer ") == true ? authHeader.Substring(7) : null;
    }
}
```

#### Service-to-Service Authentication

```csharp
public class ServiceAuthenticationHandler : DelegatingHandler
{
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Agrega token de servicio
        var serviceToken = _jwtService.GenerateServiceToken();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceToken);
        
        // Agrega headers de correlación
        request.Headers.Add("X-Correlation-Id", Activity.Current?.Id ?? Guid.NewGuid().ToString());
        request.Headers.Add("X-Service-Name", _configuration["ServiceName"]);
        
        return await base.SendAsync(request, cancellationToken);
    }
}
```

---

## 💻 Ejercicios Prácticos

### Ejercicio 1: Implementar Rate Limiting por Usuario

```csharp
public class UserRateLimiter
{
    // Implementa rate limiting basado en usuario autenticado
    // con diferentes límites según el rol del usuario
}
```

### Ejercicio 2: CSRF Token para SPA

```csharp
public class SpaCsrfProtection
{
    // Implementa protección CSRF para Single Page Applications
    // usando tokens en headers HTTP
}
```

---

## 🔍 Casos de Uso Reales

### 1. API Gateway Seguro

```csharp
[ApiController]
[Route("api/[controller]")]
public class GatewayController : ControllerBase
{
    [HttpPost("forward/{service}")]
    [Authorize]
    public async Task<IActionResult> ForwardRequest(string service, [FromBody] object data)
    {
        // Implementa forwarding seguro con validación de permisos
        return Ok("Request forwarded successfully");
    }
}
```

### 2. Microservicio con Autenticación

```csharp
public class SecureMicroservice
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SecureController : ControllerBase
    {
        [HttpGet("data")]
        [Authorize(Policy = "DataAccess")]
        public IActionResult GetData()
        {
            return Ok("Secure data accessed");
        }
    }
}
```

---

## 📊 Métricas de Seguridad de API

### KPIs Clave

1. **Failed Authentication**: Intentos fallidos de autenticación
2. **Rate Limit Violations**: Violaciones de límites de tasa
3. **CSRF Attacks**: Ataques CSRF detectados
4. **Invalid Tokens**: Tokens JWT inválidos
5. **Security Headers**: Headers de seguridad implementados

---

## 🎯 Resumen de la Clase

En esta clase hemos aprendido:

✅ **JWT y OAuth 2.0**: Implementación avanzada de autenticación
✅ **Rate Limiting**: Control de tasa de requests
✅ **CSRF Protection**: Protección contra ataques CSRF
✅ **Security Headers**: Headers de seguridad para APIs
✅ **Microservices Security**: Seguridad en arquitecturas de microservicios

---

## 🚀 Próximos Pasos

En la siguiente clase aprenderemos sobre:
- **Containerización con Docker**
- Dockerfiles optimizados para .NET
- Multi-stage builds
- Docker Compose para desarrollo

---

## 🔗 Enlaces de Referencia

- [JWT Security](https://jwt.io/introduction)
- [OAuth 2.0](https://oauth.net/2/)
- [API Security Best Practices](https://owasp.org/www-project-api-security/)
- [Microservices Security](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/)

