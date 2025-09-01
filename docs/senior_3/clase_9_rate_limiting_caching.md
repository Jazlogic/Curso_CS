# 🚀 Clase 9: Rate Limiting y Caching

## 🧭 Navegación
- **⬅️ Anterior**: [Clase 8: Versionado de APIs](clase_8_versionado_apis.md)
- **🏠 Inicio del Módulo**: [Módulo 10: APIs REST y Web APIs](README.md)
- **➡️ Siguiente**: [Clase 10: Proyecto Final](clase_10_proyecto_final.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 📚 Descripción

En esta clase aprenderás a implementar rate limiting y caching en tus APIs, incluyendo políticas de limitación de velocidad, estrategias de caché y optimización de rendimiento.

## 🎯 Objetivos de Aprendizaje

- Implementar rate limiting con diferentes estrategias
- Configurar políticas de limitación por usuario, IP y endpoint
- Implementar caching en memoria y distribuido
- Crear estrategias de invalidación de caché
- Optimizar el rendimiento de las APIs

## 📖 Contenido Teórico

### Rate Limiting

#### Instalación y Configuración Básica
```bash
# Instalar paquetes necesarios
dotnet add package Microsoft.AspNetCore.RateLimiting
dotnet add package System.Threading.RateLimiting
```

#### Configuración en Program.cs
```csharp
// Program.cs
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configurar rate limiting
builder.Services.AddRateLimiter(options =>
{
    // Política global por defecto
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));

    // Política para endpoints de autenticación
    options.AddFixedWindowLimiter("Authentication", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 2;
    });

    // Política para endpoints de API
    options.AddFixedWindowLimiter("API", limiterOptions =>
    {
        limiterOptions.PermitLimit = 1000;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 10;
    });

    // Política para endpoints de administración
    options.AddFixedWindowLimiter("Admin", limiterOptions =>
    {
        limiterOptions.PermitLimit = 500;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 5;
    });

    // Configurar respuesta de error
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        
        var response = new
        {
            error = "Too Many Requests",
            message = "Has excedido el límite de solicitudes. Intenta de nuevo más tarde.",
            retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter) 
                ? retryAfter.TotalSeconds 
                : 60,
            timestamp = DateTime.UtcNow
        };

        await context.HttpContext.Response.WriteAsJsonAsync(response, token);
    };
});

var app = builder.Build();

// Configurar pipeline HTTP
app.UseRateLimiter();

app.Run();
```

#### Configuración Avanzada de Rate Limiting
```csharp
builder.Services.AddRateLimiter(options =>
{
    // Política de token bucket para endpoints críticos
    options.AddTokenBucketLimiter("Critical", limiterOptions =>
    {
        limiterOptions.TokenLimit = 10;
        limiterOptions.TokensPerPeriod = 2;
        limiterOptions.ReplenishmentPeriod = TimeSpan.FromSeconds(1);
        limiterOptions.AutoReplenishment = true;
    });

    // Política de sliding window para endpoints de búsqueda
    options.AddSlidingWindowLimiter("Search", limiterOptions =>
    {
        limiterOptions.PermitLimit = 50;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.SegmentsPerWindow = 4;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 5;
    });

    // Política de concurrency para endpoints de procesamiento
    options.AddConcurrencyLimiter("Processing", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 3;
    });

    // Política personalizada por IP
    options.AddPolicy<string, IPRateLimitingPolicy>("IPBased");

    // Política personalizada por usuario autenticado
    options.AddPolicy<string, UserRateLimitingPolicy>("UserBased");
});

// Configurar políticas personalizadas
builder.Services.Configure<RateLimitingOptions>(builder.Configuration.GetSection("RateLimiting"));
```

#### Políticas Personalizadas de Rate Limiting
```csharp
public class IPRateLimitingPolicy : IRateLimiterPolicy<string>
{
    private readonly Func<OnRejectedContext, CancellationToken, ValueTask> _onRejected;

    public IPRateLimitingPolicy(ILogger<IPRateLimitingPolicy> logger)
    {
        _onRejected = async (context, token) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.HttpContext.Response.ContentType = "application/json";
            
            var response = new
            {
                error = "Rate Limit Exceeded",
                message = "Tu IP ha excedido el límite de solicitudes.",
                retryAfter = 60,
                timestamp = DateTime.UtcNow
            };

            await context.HttpContext.Response.WriteAsJsonAsync(response, token);
            
            logger.LogWarning("Rate limit exceeded for IP: {IP}", 
                context.HttpContext.Connection.RemoteIpAddress);
        };
    }

    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ =>
            new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 5
            });
    }

    public Func<OnRejectedContext, CancellationToken, ValueTask> OnRejected => _onRejected;
}

public class UserRateLimitingPolicy : IRateLimiterPolicy<string>
{
    private readonly Func<OnRejectedContext, CancellationToken, ValueTask> _onRejected;

    public UserRateLimitingPolicy(ILogger<UserRateLimitingPolicy> logger)
    {
        _onRejected = async (context, token) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.HttpContext.Response.ContentType = "application/json";
            
            var response = new
            {
                error = "Rate Limit Exceeded",
                message = "Has excedido el límite de solicitudes para tu cuenta.",
                retryAfter = 60,
                timestamp = DateTime.UtcNow
            };

            await context.HttpContext.Response.WriteAsJsonAsync(response, token);
            
            var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            logger.LogWarning("Rate limit exceeded for user: {UserId}", userId);
        };
    }

    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        
        // Diferentes límites según el rol del usuario
        var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value ?? "user";
        
        var (permitLimit, window) = userRole.ToLower() switch
        {
            "admin" => (1000, TimeSpan.FromMinutes(1)),
            "premium" => (500, TimeSpan.FromMinutes(1)),
            "user" => (100, TimeSpan.FromMinutes(1)),
            _ => (50, TimeSpan.FromMinutes(1))
        };
        
        return RateLimitPartition.GetFixedWindowLimiter(userId, _ =>
            new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = permitLimit,
                Window = window,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            });
    }

    public Func<OnRejectedContext, CancellationToken, ValueTask> OnRejected => _onRejected;
}
```

#### Aplicación de Rate Limiting en Controladores
```csharp
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("API")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    [EnableRateLimiting("Search")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpPost("login")]
    [EnableRateLimiting("Authentication")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginDto loginDto)
    {
        // Implementación del login
        var response = await _userService.LoginAsync(loginDto);
        return Ok(response);
    }

    [HttpPost("register")]
    [EnableRateLimiting("Authentication")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        // Implementación del registro
        var user = await _userService.RegisterAsync(registerDto);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpPost("bulk-process")]
    [EnableRateLimiting("Processing")]
    public async Task<ActionResult> BulkProcessUsers(List<int> userIds)
    {
        // Procesamiento en lote de usuarios
        var result = await _userService.BulkProcessAsync(userIds);
        return Ok(result);
    }

    [HttpGet("admin/stats")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("Admin")]
    public async Task<ActionResult<UserStatsDto>> GetUserStats()
    {
        // Estadísticas de usuarios (solo admin)
        var stats = await _userService.GetUserStatsAsync();
        return Ok(stats);
    }
}
```

### Caching

#### Configuración de Caching en Memoria
```csharp
// Program.cs
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024; // Límite de tamaño en MB
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
    options.CompactionPercentage = 0.25;
});

builder.Services.AddDistributedMemoryCache();

// Configurar opciones de caché
builder.Services.Configure<CacheOptions>(builder.Configuration.GetSection("Cache"));
```

#### Servicios de Caching
```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
    Task<bool> ExistsAsync(string key);
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
}

public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<CacheService> _logger;
    private readonly CacheOptions _options;

    public CacheService(
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        ILogger<CacheService> logger,
        IOptions<CacheOptions> options)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            // Intentar obtener del caché en memoria primero
            if (_memoryCache.TryGetValue(key, out T? value))
            {
                _logger.LogDebug("Cache hit for key: {Key}", key);
                return value;
            }

            // Si no está en memoria, intentar del caché distribuido
            var distributedValue = await _distributedCache.GetStringAsync(key);
            if (!string.IsNullOrEmpty(distributedValue))
            {
                var deserializedValue = JsonSerializer.Deserialize<T>(distributedValue);
                if (deserializedValue != null)
                {
                    // Agregar al caché en memoria
                    var memoryOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.MemoryCacheExpirationMinutes),
                        SlidingExpiration = TimeSpan.FromMinutes(_options.MemoryCacheSlidingExpirationMinutes)
                    };
                    _memoryCache.Set(key, deserializedValue, memoryOptions);
                    
                    _logger.LogDebug("Cache hit from distributed cache for key: {Key}", key);
                    return deserializedValue;
                }
            }

            _logger.LogDebug("Cache miss for key: {Key}", key);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from cache for key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var cacheExpiration = expiration ?? TimeSpan.FromMinutes(_options.DefaultExpirationMinutes);

            // Configurar opciones de caché en memoria
            var memoryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheExpiration,
                SlidingExpiration = TimeSpan.FromMinutes(_options.MemoryCacheSlidingExpirationMinutes),
                Size = 1 // Tamaño relativo para el límite de memoria
            };

            // Configurar opciones de caché distribuido
            var distributedOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheExpiration,
                SlidingExpiration = TimeSpan.FromMinutes(_options.DistributedCacheSlidingExpirationMinutes)
            };

            // Agregar al caché en memoria
            _memoryCache.Set(key, value, memoryOptions);

            // Agregar al caché distribuido
            var serializedValue = JsonSerializer.Serialize(value);
            await _distributedCache.SetStringAsync(key, serializedValue, distributedOptions);

            _logger.LogDebug("Value cached for key: {Key} with expiration: {Expiration}", key, cacheExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            _memoryCache.Remove(key);
            await _distributedCache.RemoveAsync(key);
            
            _logger.LogDebug("Value removed from cache for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing value from cache for key: {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            // Nota: IMemoryCache no soporta eliminación por patrón
            // Esta implementación es básica y podría mejorarse
            _logger.LogWarning("Pattern-based removal not fully supported for memory cache");
            
            // Para caché distribuido, podrías implementar una solución más sofisticada
            // usando Redis SCAN o similar
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing values by pattern: {Pattern}", pattern);
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            return _memoryCache.TryGetValue(key, out _) || 
                   await _distributedCache.GetAsync(key) != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence for key: {Key}", key);
            return false;
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        var cachedValue = await GetAsync<T>(key);
        if (cachedValue != null)
        {
            return cachedValue;
        }

        var value = await factory();
        await SetAsync(key, value, expiration);
        return value;
    }
}
```

#### Configuración de Caché Distribuido (Redis)
```csharp
// Program.cs
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "MyApi_";
});

// Configurar opciones de caché
builder.Services.Configure<CacheOptions>(builder.Configuration.GetSection("Cache"));

// appsettings.json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "Cache": {
    "DefaultExpirationMinutes": 30,
    "MemoryCacheExpirationMinutes": 15,
    "MemoryCacheSlidingExpirationMinutes": 10,
    "DistributedCacheSlidingExpirationMinutes": 20,
    "UserProfileExpirationMinutes": 60,
    "ProductCatalogExpirationMinutes": 120,
    "SearchResultsExpirationMinutes": 5
  }
}
```

#### Implementación de Caché en Servicios
```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        ICacheService cacheService,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var cacheKey = $"user:{id}";
        
        return await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            _logger.LogInformation("Fetching user from database: {UserId}", id);
            var user = await _userRepository.GetByIdAsync(id);
            return user != null ? MapToUserDto(user) : null;
        }, TimeSpan.FromMinutes(30));
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var cacheKey = "users:all";
        
        return await _cacheService.GetOrSetAsync(cacheKey, async () =>
        {
            _logger.LogInformation("Fetching all users from database");
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToUserDto);
        }, TimeSpan.FromMinutes(15));
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        var user = new User
        {
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            Email = createUserDto.Email,
            PasswordHash = HashPassword(createUserDto.Password),
            Role = createUserDto.Role ?? "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.AddAsync(user);
        var userDto = MapToUserDto(createdUser);

        // Invalidar cachés relacionados
        await InvalidateUserCaches(createdUser.Id);
        
        return userDto;
    }

    public async Task<bool> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
    {
        var result = await _userRepository.UpdateAsync(id, updateUserDto);
        
        if (result)
        {
            // Invalidar cachés relacionados
            await InvalidateUserCaches(id);
        }
        
        return result;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var result = await _userRepository.DeleteAsync(id);
        
        if (result)
        {
            // Invalidar cachés relacionados
            await InvalidateUserCaches(id);
        }
        
        return result;
    }

    private async Task InvalidateUserCaches(int userId)
    {
        try
        {
            // Invalidar caché específico del usuario
            await _cacheService.RemoveAsync($"user:{userId}");
            
            // Invalidar caché de todos los usuarios
            await _cacheService.RemoveAsync("users:all");
            
            // Invalidar cachés de búsqueda
            await _cacheService.RemoveByPatternAsync("users:search:*");
            
            _logger.LogDebug("User caches invalidated for user: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating user caches for user: {UserId}", userId);
        }
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
```

#### Atributos de Caché
```csharp
[AttributeUsage(AttributeTargets.Method)]
public class CacheAttribute : Attribute
{
    public string Key { get; set; }
    public int ExpirationMinutes { get; set; }
    public bool UseSlidingExpiration { get; set; }

    public CacheAttribute(string key, int expirationMinutes = 30, bool useSlidingExpiration = false)
    {
        Key = key;
        ExpirationMinutes = expirationMinutes;
        UseSlidingExpiration = useSlidingExpiration;
    }
}

public class CacheFilter : IAsyncActionFilter
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheFilter> _logger;

    public CacheFilter(ICacheService cacheService, ILogger<CacheFilter> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var cacheAttribute = context.ActionDescriptor.EndpointMetadata
            .OfType<CacheAttribute>()
            .FirstOrDefault();

        if (cacheAttribute == null)
        {
            await next();
            return;
        }

        var cacheKey = GenerateCacheKey(cacheAttribute.Key, context);
        var cachedValue = await _cacheService.GetAsync<object>(cacheKey);

        if (cachedValue != null)
        {
            _logger.LogDebug("Returning cached value for key: {Key}", cacheKey);
            context.Result = new OkObjectResult(cachedValue);
            return;
        }

        var executedContext = await next();
        
        if (executedContext.Result is ObjectResult objectResult && objectResult.Value != null)
        {
            var expiration = TimeSpan.FromMinutes(cacheAttribute.ExpirationMinutes);
            await _cacheService.SetAsync(cacheKey, objectResult.Value, expiration);
            
            _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", 
                cacheKey, expiration);
        }
    }

    private static string GenerateCacheKey(string baseKey, ActionExecutingContext context)
    {
        // Generar clave de caché basada en parámetros de la acción
        var parameters = context.ActionArguments
            .Where(arg => arg.Value != null)
            .OrderBy(arg => arg.Key)
            .Select(arg => $"{arg.Key}:{arg.Value}");

        var parameterString = string.Join("|", parameters);
        return string.IsNullOrEmpty(parameterString) ? baseKey : $"{baseKey}:{parameterString}";
    }
}
```

#### Configuración de Caché en Controladores
```csharp
[ApiController]
[Route("api/[controller]")]
[ServiceFilter(typeof(CacheFilter))]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    [Cache("products:all", 120)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(products);
    }

    [HttpGet("{id:int}")]
    [Cache("product", 60)]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpGet("search")]
    [Cache("products:search", 5)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> SearchProducts(
        [FromQuery] string query,
        [FromQuery] int? categoryId)
    {
        var products = await _productService.SearchProductsAsync(query, categoryId);
        return Ok(products);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto createProductDto)
    {
        var product = await _productService.CreateProductAsync(createProductDto);
        
        // Invalidar cachés relacionados
        await InvalidateProductCaches();
        
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto updateProductDto)
    {
        var result = await _productService.UpdateProductAsync(id, updateProductDto);
        if (!result)
        {
            return NotFound();
        }

        // Invalidar cachés relacionados
        await InvalidateProductCaches(id);
        
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var result = await _productService.DeleteProductAsync(id);
        if (!result)
        {
            return NotFound();
        }

        // Invalidar cachés relacionados
        await InvalidateProductCaches(id);
        
        return NoContent();
    }

    private async Task InvalidateProductCaches(int? productId = null)
    {
        try
        {
            if (productId.HasValue)
            {
                await _cacheService.RemoveAsync($"product:{productId.Value}");
            }

            // Invalidar cachés generales
            await _cacheService.RemoveAsync("products:all");
            await _cacheService.RemoveByPatternAsync("products:search:*");
            
            _logger.LogDebug("Product caches invalidated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating product caches");
        }
    }
}
```

### Optimización de Rendimiento

#### Configuración de Respuestas Comprimidas
```csharp
// Program.cs
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

var app = builder.Build();

app.UseResponseCompression();
```

#### Middleware de Optimización
```csharp
public class PerformanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMiddleware> _logger;

    public PerformanceMiddleware(RequestDelegate next, ILogger<PerformanceMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            var elapsed = stopwatch.ElapsedMilliseconds;
            var path = context.Request.Path;
            var method = context.Request.Method;
            var statusCode = context.Response.StatusCode;
            
            if (elapsed > 1000) // Log requests that take more than 1 second
            {
                _logger.LogWarning("Slow request: {Method} {Path} took {Elapsed}ms, status: {StatusCode}", 
                    method, path, elapsed, statusCode);
            }
            else
            {
                _logger.LogDebug("Request: {Method} {Path} took {Elapsed}ms, status: {StatusCode}", 
                    method, path, elapsed, statusCode);
            }
        }
    }
}

// Extensión para registrar el middleware
public static class PerformanceMiddlewareExtensions
{
    public static IApplicationBuilder UsePerformanceMonitoring(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<PerformanceMiddleware>();
    }
}
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Configuración de Rate Limiting
Implementa rate limiting con:
- Políticas globales y específicas
- Límites por usuario, IP y endpoint
- Respuestas personalizadas de error
- Logging de violaciones

### Ejercicio 2: Sistema de Caché
Crea un sistema de caché con:
- Caché en memoria y distribuido
- Estrategias de invalidación
- Atributos de caché personalizados
- Monitoreo de rendimiento

### Ejercicio 3: Optimización de Rendimiento
Implementa optimizaciones con:
- Compresión de respuestas
- Middleware de monitoreo
- Estrategias de caché inteligentes
- Métricas de rendimiento

### Ejercicio 4: Integración Completa
Integra rate limiting y caching en:
- Controladores de API
- Servicios de negocio
- Middleware personalizado
- Configuración de Swagger

## 📝 Quiz de Autoevaluación

1. ¿Cuáles son las ventajas de usar rate limiting en APIs públicas?
2. ¿Cómo implementarías diferentes políticas de rate limiting según el tipo de usuario?
3. ¿Qué estrategias usarías para la invalidación de caché en sistemas distribuidos?
4. ¿Cómo optimizarías el rendimiento de una API con alto tráfico?
5. ¿Qué consideraciones tendrías para implementar caché en microservicios?

## 🔗 Enlaces Útiles

- [Rate Limiting in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/performance/rate-limit)
- [Caching in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/)
- [Redis Caching](https://redis.io/)
- [Performance Best Practices](https://docs.microsoft.com/en-us/aspnet/core/performance/)

## 🚀 Siguiente Clase

En la siguiente clase implementarás el proyecto final del módulo, integrando todos los conceptos aprendidos en una API completa y funcional.

---

**💡 Consejo**: Siempre monitorea el rendimiento de tu API y ajusta las políticas de rate limiting y caché según los patrones de uso reales de tus usuarios.
