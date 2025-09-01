# 🚀 Clase 8: Versionado de APIs

## 🧭 Navegación
- **⬅️ Anterior**: [Clase 7: Documentación con Swagger](clase_7_documentacion_swagger.md)
- **🏠 Inicio del Módulo**: [Módulo 10: APIs REST y Web APIs](README.md)
- **➡️ Siguiente**: [Clase 9: Rate Limiting y Caching](clase_9_rate_limiting_caching.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 📚 Descripción

En esta clase aprenderás a implementar diferentes estrategias de versionado de APIs, incluyendo versionado por URL, headers, query parameters y compatibilidad hacia atrás.

## 🎯 Objetivos de Aprendizaje

- Implementar versionado de APIs con diferentes estrategias
- Configurar versionado por URL, headers y query parameters
- Manejar compatibilidad hacia atrás
- Implementar deprecación de versiones
- Crear documentación para múltiples versiones

## 📖 Contenido Teórico

### Configuración del Versionado

#### Instalación y Configuración Básica
```bash
# Instalar paquete de versionado
dotnet add package Microsoft.AspNetCore.Mvc.Versioning
dotnet add package Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer
```

#### Configuración en Program.cs
```csharp
// Program.cs
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

var builder = WebApplication.CreateBuilder(args);

// Configurar versionado de APIs
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-API-Version"),
        new QueryStringApiVersionReader("api-version")
    );
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Configurar Swagger para múltiples versiones
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

var app = builder.Build();

// Configurar Swagger para múltiples versiones
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    
    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint(
            $"/swagger/{description.GroupName}/swagger.json",
            $"Mi API {description.GroupName.ToUpperInvariant()}");
    }
    
    options.RoutePrefix = string.Empty;
});

app.Run();
```

#### Configuración Avanzada del Versionado
```csharp
builder.Services.AddApiVersioning(options =>
{
    // Versión por defecto
    options.DefaultApiVersion = new ApiVersion(1, 0);
    
    // Asumir versión por defecto cuando no se especifica
    options.AssumeDefaultVersionWhenUnspecified = true;
    
    // Reportar versiones disponibles en headers
    options.ReportApiVersions = true;
    
    // Configurar lectores de versión
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-API-Version"),
        new QueryStringApiVersionReader("api-version"),
        new MediaTypeApiVersionReader("version")
    );
    
    // Configurar respuesta de error para versiones no soportadas
    options.ErrorResponses = new ApiVersioningErrorResponseProvider();
    
    // Configurar deprecación
    options.DeprecationPolicy = new DeprecationPolicy();
});

builder.Services.AddVersionedApiExplorer(options =>
{
    // Formato del nombre del grupo
    options.GroupNameFormat = "'v'VVV";
    
    // Sustituir versión en URL
    options.SubstituteApiVersionInUrl = true;
    
    // Configurar formato de versión
    options.AssumeDefaultVersionWhenUnspecified = true;
});

// Configurar Swagger para múltiples versiones
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
```

### Estrategias de Versionado

#### Versionado por URL
```csharp
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los usuarios (v1.0)
    /// </summary>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserDto>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    /// <summary>
    /// Obtiene todos los usuarios con paginación (v2.0)
    /// </summary>
    [HttpGet]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResult<UserDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResult<UserDto>>> GetUsersV2(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest("Parámetros de paginación inválidos");
        }

        var result = await _userService.GetUsersPaginatedAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un usuario por ID (v1.0 y v2.0)
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    /// <summary>
    /// Crea un nuevo usuario (v1.0)
    /// </summary>
    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
    {
        var user = await _userService.CreateUserAsync(createUserDto);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    /// <summary>
    /// Crea un nuevo usuario con validación avanzada (v2.0)
    /// </summary>
    [HttpPost]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserDetailDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDetailDto>> CreateUserV2(CreateUserV2Dto createUserDto)
    {
        // Validación adicional en v2.0
        if (await _userService.EmailExistsAsync(createUserDto.Email))
        {
            return Conflict("El email ya está registrado");
        }

        var user = await _userService.CreateUserV2Async(createUserDto);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }
}
```

#### Versionado por Headers
```csharp
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los productos (v1.0)
    /// </summary>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductDto>))]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(products);
    }

    /// <summary>
    /// Obtiene todos los productos con filtros avanzados (v2.0)
    /// </summary>
    [HttpGet]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResult<ProductDto>))]
    public async Task<ActionResult<PaginatedResult<ProductDto>>> GetProductsV2(
        [FromQuery] ProductFilterDto filter)
    {
        var result = await _productService.GetProductsWithFiltersAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un producto por ID (v1.0 y v2.0)
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }
}
```

#### Versionado por Query Parameters
```csharp
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las órdenes (v1.0)
    /// </summary>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<OrderDto>))]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    /// <summary>
    /// Obtiene todas las órdenes con filtros (v2.0)
    /// </summary>
    [HttpGet]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResult<OrderDto>))]
    public async Task<ActionResult<PaginatedResult<OrderDto>>> GetOrdersV2(
        [FromQuery] OrderFilterDto filter)
    {
        var result = await _orderService.GetOrdersWithFiltersAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Crea una nueva orden (v1.0)
    /// </summary>
    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OrderDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto createOrderDto)
    {
        var order = await _orderService.CreateOrderAsync(createOrderDto);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    /// <summary>
    /// Crea una nueva orden con validación avanzada (v2.0)
    /// </summary>
    [HttpPost]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OrderDetailDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<OrderDetailDto>> CreateOrderV2(CreateOrderV2Dto createOrderDto)
    {
        // Validación adicional en v2.0
        var validationResult = await _orderService.ValidateOrderAsync(createOrderDto);
        if (!validationResult.IsValid)
        {
            return UnprocessableEntity(validationResult.Errors);
        }

        var order = await _orderService.CreateOrderV2Async(createOrderDto);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }
}
```

### Configuración de Swagger para Múltiples Versiones

#### Configuración de Swagger
```csharp
public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        // Agregar un documento de Swagger para cada versión de la API
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
        }

        // Incluir comentarios XML
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);

        // Configurar autenticación JWT
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });

        // Configurar filtros
        options.OperationFilter<SwaggerDefaultValues>();
        options.DocumentFilter<SwaggerVersionDocumentFilter>();
    }

    private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
    {
        var info = new OpenApiInfo
        {
            Title = "Mi API",
            Version = description.ApiVersion.ToString(),
            Description = "Una API completa de ejemplo con ASP.NET Core",
            Contact = new OpenApiContact
            {
                Name = "Tu Nombre",
                Email = "tu@email.com",
                Url = new Uri("https://github.com/tuusuario")
            },
            License = new OpenApiLicense
            {
                Name = "MIT",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        };

        if (description.IsDeprecated)
        {
            info.Description += " Esta versión de la API está deprecada.";
        }

        return info;
    }
}
```

#### Filtros de Swagger para Versionado
```csharp
public class SwaggerDefaultValues : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiDescription = context.ApiDescription;
        var apiVersion = apiDescription.GetApiVersion();
        var modelMetadata = apiDescription.ModelMetadata;

        operation.Deprecated |= apiDescription.IsDeprecated();

        // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1752#issue-663991077
        foreach (var responseType in context.ApiDescription.SupportedResponseTypes)
        {
            var responseKey = responseType.IsDefaultResponse ? "default" : responseType.StatusCode.ToString();
            var response = operation.Responses[responseKey];

            foreach (var contentType in response.Content.Keys)
            {
                if (!responseType.ApiResponseFormats.Any(x => x.MediaType == contentType))
                {
                    response.Content.Remove(contentType);
                }
            }
        }

        // Agregar información de versión
        if (apiVersion != null)
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-API-Version",
                In = ParameterLocation.Header,
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = new OpenApiString(apiVersion.ToString())
                },
                Description = "Versión de la API"
            });
        }
    }
}

public class SwaggerVersionDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Agregar información de versiones disponibles
        var apiVersion = context.ApiDescriptions.FirstOrDefault()?.GetApiVersion();
        if (apiVersion != null)
        {
            swaggerDoc.Info.Extensions.Add("x-api-version", new OpenApiString(apiVersion.ToString()));
        }

        // Agregar información de deprecación
        var isDeprecated = context.ApiDescriptions.Any(api => api.IsDeprecated);
        if (isDeprecated)
        {
            swaggerDoc.Info.Extensions.Add("x-deprecated", new OpenApiBoolean(true));
        }
    }
}
```

### Manejo de Compatibilidad Hacia Atrás

#### Servicios Compatibles con Múltiples Versiones
```csharp
public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<PaginatedResult<UserDto>> GetUsersPaginatedAsync(int page, int pageSize);
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
    Task<UserDetailDto> CreateUserV2Async(CreateUserV2Dto createUserDto);
    Task<bool> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
    Task<bool> DeleteUserAsync(int id);
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToUserDto);
    }

    public async Task<PaginatedResult<UserDto>> GetUsersPaginatedAsync(int page, int pageSize)
    {
        var users = await _userRepository.GetPagedAsync(page, pageSize);
        var totalCount = await _userRepository.CountAsync();
        
        return new PaginatedResult<UserDto>(
            users.Select(MapToUserDto),
            totalCount,
            page,
            pageSize);
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user != null ? MapToUserDto(user) : null;
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
        return MapToUserDto(createdUser);
    }

    public async Task<UserDetailDto> CreateUserV2Async(CreateUserV2Dto createUserDto)
    {
        // Validación adicional en v2.0
        if (await _userRepository.EmailExistsAsync(createUserDto.Email))
        {
            throw new InvalidOperationException("El email ya está registrado");
        }

        var user = new User
        {
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            Email = createUserDto.Email,
            PasswordHash = HashPassword(createUserDto.Password),
            Role = createUserDto.Role ?? "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            PhoneNumber = createUserDto.PhoneNumber,
            DateOfBirth = createUserDto.DateOfBirth
        };

        var createdUser = await _userRepository.AddAsync(user);
        return MapToUserDetailDto(createdUser);
    }

    public async Task<bool> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return false;
        }

        user.FirstName = updateUserDto.FirstName ?? user.FirstName;
        user.LastName = updateUserDto.LastName ?? user.LastName;
        user.Email = updateUserDto.Email ?? user.Email;
        user.Role = updateUserDto.Role ?? user.Role;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return false;
        }

        await _userRepository.DeleteAsync(user);
        return true;
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

    private static UserDetailDto MapToUserDetailDto(User user)
    {
        return new UserDetailDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth,
            FullName = $"{user.FirstName} {user.LastName}"
        };
    }

    private static string HashPassword(string password)
    {
        // Implementar hashing de contraseña
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
```

#### DTOs Compatibles con Múltiples Versiones
```csharp
// DTOs para v1.0
public class CreateUserDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(50, ErrorMessage = "El apellido no puede exceder 50 caracteres")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
    public string Password { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "El rol no puede exceder 20 caracteres")]
    public string? Role { get; set; } = "User";
}

public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// DTOs para v2.0 (extendidos)
public class CreateUserV2Dto : CreateUserDto
{
    [Phone(ErrorMessage = "El formato del teléfono no es válido")]
    public string? PhoneNumber { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }
}

public class UserDetailDto : UserDto
{
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string FullName { get; set; } = string.Empty;
}
```

### Deprecación de Versiones

#### Configuración de Deprecación
```csharp
public class DeprecationPolicy : IDeprecationPolicy
{
    public bool IsDeprecated(ApiVersion apiVersion)
    {
        // Marcar v1.0 como deprecada después de cierta fecha
        if (apiVersion.Major == 1 && apiVersion.Minor == 0)
        {
            var deprecationDate = new DateTime(2024, 12, 31);
            return DateTime.UtcNow > deprecationDate;
        }

        return false;
    }

    public string? GetDeprecationMessage(ApiVersion apiVersion)
    {
        if (IsDeprecated(apiVersion))
        {
            return $"La versión {apiVersion} está deprecada. Por favor, actualiza a la versión más reciente.";
        }

        return null;
    }
}

public class ApiVersioningErrorResponseProvider : IErrorResponseProvider
{
    public IActionResult CreateResponse(ErrorResponseContext context)
    {
        var response = new
        {
            error = new
            {
                code = "UnsupportedApiVersion",
                message = "La versión de la API especificada no es compatible.",
                details = new
                {
                    requestedVersion = context.RequestedVersion?.ToString(),
                    supportedVersions = context.SupportedVersions?.Select(v => v.ToString()),
                    deprecatedVersions = context.DeprecatedApiVersions?.Select(v => v.ToString())
                },
                timestamp = DateTime.UtcNow
            }
        };

        return new BadRequestObjectResult(response);
    }
}
```

#### Middleware de Deprecación
```csharp
public class ApiDeprecationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiDeprecationMiddleware> _logger;

    public ApiDeprecationMiddleware(RequestDelegate next, ILogger<ApiDeprecationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var apiVersion = context.GetRequestedApiVersion();
        
        if (apiVersion != null)
        {
            // Verificar si la versión está deprecada
            var deprecationPolicy = context.RequestServices.GetService<IDeprecationPolicy>();
            if (deprecationPolicy?.IsDeprecated(apiVersion) == true)
            {
                var message = deprecationPolicy.GetDeprecationMessage(apiVersion);
                
                // Agregar headers de deprecación
                context.Response.Headers.Add("X-API-Deprecated", "true");
                context.Response.Headers.Add("X-API-Deprecation-Message", message ?? "API version deprecated");
                
                // Log de advertencia
                _logger.LogWarning("API version {ApiVersion} is deprecated. {Message}", 
                    apiVersion, message);
            }
        }

        await _next(context);
    }
}

// Extensión para registrar el middleware
public static class ApiDeprecationMiddlewareExtensions
{
    public static IApplicationBuilder UseApiDeprecation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApiDeprecationMiddleware>();
    }
}
```

### Documentación para Múltiples Versiones

#### Configuración de Swagger UI
```csharp
app.UseSwaggerUI(options =>
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    
    // Crear un endpoint de Swagger para cada versión
    foreach (var description in provider.ApiVersionDescriptions)
    {
        var isDeprecated = description.IsDeprecated;
        var title = isDeprecated ? $"Mi API {description.GroupName.ToUpperInvariant()} (DEPRECATED)" : $"Mi API {description.GroupName.ToUpperInvariant()}";
        
        options.SwaggerEndpoint(
            $"/swagger/{description.GroupName}/swagger.json",
            title);
    }
    
    options.RoutePrefix = string.Empty;
    options.DocumentTitle = "Mi API - Documentación de Versiones";
    options.DefaultModelsExpandDepth(2);
    options.DefaultModelExpandDepth(2);
    options.DisplayRequestDuration();
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    
    // Configurar CSS personalizado para versiones deprecadas
    options.InjectStylesheet("/swagger-ui/custom.css");
    options.InjectJavascript("/swagger-ui/custom.js");
});
```

#### CSS para Versiones Deprecadas
```css
/* wwwroot/swagger-ui/custom.css */
.swagger-ui .opblock.opblock-deprecated {
    opacity: 0.6;
    border-left: 4px solid #f39c12;
}

.swagger-ui .opblock.opblock-deprecated .opblock-summary-method {
    background-color: #f39c12;
}

.swagger-ui .opblock.opblock-deprecated::before {
    content: "DEPRECATED";
    position: absolute;
    top: 10px;
    right: 10px;
    background-color: #e74c3c;
    color: white;
    padding: 2px 8px;
    border-radius: 4px;
    font-size: 10px;
    font-weight: bold;
    z-index: 1;
}

.swagger-ui .info .title.deprecated::after {
    content: " (DEPRECATED)";
    color: #e74c3c;
    font-weight: bold;
}

.swagger-ui .deprecation-warning {
    background-color: #fff3cd;
    border: 1px solid #ffeaa7;
    color: #856404;
    padding: 10px;
    margin: 10px 0;
    border-radius: 4px;
    text-align: center;
    font-weight: bold;
}
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Configuración de Versionado
Implementa versionado de APIs con:
- Configuración básica
- Múltiples estrategias (URL, headers, query)
- Swagger para múltiples versiones
- Manejo de errores de versión

### Ejercicio 2: Compatibilidad Hacia Atrás
Crea servicios compatibles con:
- Múltiples versiones de DTOs
- Lógica de negocio extendida
- Validación progresiva
- Migración de datos

### Ejercicio 3: Deprecación de Versiones
Implementa sistema de deprecación con:
- Políticas de deprecación
- Middleware de advertencias
- Headers informativos
- Logging de uso

### Ejercicio 4: Documentación de Versiones
Crea documentación completa para:
- Múltiples versiones en Swagger
- Guías de migración
- Ejemplos de uso
- Notas de compatibilidad

## 📝 Quiz de Autoevaluación

1. ¿Cuáles son las ventajas e inconvenientes de cada estrategia de versionado?
2. ¿Cómo manejarías la compatibilidad hacia atrás en APIs con cambios breaking?
3. ¿Qué estrategias usarías para la deprecación gradual de versiones?
4. ¿Cómo implementarías migración automática entre versiones?
5. ¿Qué consideraciones tendrías para el versionado en microservicios?

## 🔗 Enlaces Útiles

- [API Versioning in ASP.NET Core](https://github.com/dotnet/aspnet-api-versioning)
- [API Versioning Best Practices](https://restfulapi.net/versioning/)
- [Semantic Versioning](https://semver.org/)
- [API Evolution](https://apisyouwonthate.com/blog/api-evolution-for-rest-http-apis)

## 🚀 Siguiente Clase

En la siguiente clase aprenderás a implementar rate limiting y caching, incluyendo políticas de limitación de velocidad y estrategias de caché.

---

**💡 Consejo**: Siempre planifica el versionado desde el inicio del proyecto y documenta claramente los cambios entre versiones para facilitar la migración de los consumidores de tu API.
