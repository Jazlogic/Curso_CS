# 🚀 Clase 7: Documentación con Swagger

## 🧭 Navegación
- **⬅️ Anterior**: [Clase 6: Entity Framework Core](clase_6_entity_framework_core.md)
- **🏠 Inicio del Módulo**: [Módulo 10: APIs REST y Web APIs](README.md)
- **➡️ Siguiente**: [Clase 8: Versionado de APIs](clase_8_versionado_apis.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 📚 Descripción

En esta clase aprenderás a documentar completamente tus APIs usando Swagger/OpenAPI, incluyendo configuración avanzada, autenticación JWT, ejemplos de uso y documentación personalizada.

## 🎯 Objetivos de Aprendizaje

- Configurar Swagger/OpenAPI en ASP.NET Core
- Documentar endpoints con comentarios XML
- Configurar autenticación JWT en Swagger
- Crear ejemplos de entrada y salida
- Personalizar la interfaz de Swagger
- Generar documentación automática

## 📖 Contenido Teórico

### Configuración Básica de Swagger

#### Instalación y Configuración
```bash
# Instalar paquetes necesarios
dotnet add package Swashbuckle.AspNetCore
dotnet add package Swashbuckle.AspNetCore.Annotations
dotnet add package Swashbuckle.AspNetCore.Filters
```

#### Configuración en Program.cs
```csharp
// Program.cs
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Configurar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Mi API",
        Version = "v1",
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
    });

    // Incluir comentarios XML
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // Configurar esquemas de respuesta por defecto
    c.MapType<DateTime>(() => new OpenApiSchema { Type = "string", Format = "date-time" });
    c.MapType<DateTime?>(() => new OpenApiSchema { Type = "string", Format = "date-time", Nullable = true });
    c.MapType<decimal>(() => new OpenApiSchema { Type = "number", Format = "decimal" });
    c.MapType<decimal?>(() => new OpenApiSchema { Type = "number", Format = "decimal", Nullable = true });
});

var app = builder.Build();

// Configurar pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mi API v1");
        c.RoutePrefix = string.Empty; // Swagger en la raíz
        c.DocumentTitle = "Mi API - Documentación";
        c.DefaultModelsExpandDepth(2);
        c.DefaultModelExpandDepth(2);
    });
}

app.Run();
```

#### Configuración Avanzada
```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Mi API",
        Version = "v1",
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
        },
        TermsOfService = new Uri("https://example.com/terms")
    });

    // Incluir comentarios XML
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // Configurar esquemas de respuesta por defecto
    c.MapType<DateTime>(() => new OpenApiSchema { Type = "string", Format = "date-time" });
    c.MapType<DateTime?>(() => new OpenApiSchema { Type = "string", Format = "date-time", Nullable = true });
    c.MapType<decimal>(() => new OpenApiSchema { Type = "number", Format = "decimal" });
    c.MapType<decimal?>(() => new OpenApiSchema { Type = "number", Format = "decimal", Nullable = true });

    // Configurar operaciones
    c.OperationFilter<SwaggerDefaultValues>();
    c.OperationFilter<SwaggerFileOperationFilter>();

    // Configurar esquemas
    c.SchemaFilter<SwaggerSchemaFilter>();

    // Configurar tags
    c.TagActionsBy(api =>
    {
        if (api.GroupName != null)
        {
            return new[] { api.GroupName.ToString() };
        }

        var controllerActionDescriptor = api.ActionDescriptor as ControllerActionDescriptor;
        if (controllerActionDescriptor != null)
        {
            return new[] { controllerActionDescriptor.ControllerName };
        }

        throw new InvalidOperationException("Unable to determine tag for endpoint.");
    });

    c.DocInclusionPredicate((name, api) => true);

    // Configurar respuestas por defecto
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

    // Configurar ejemplos
    c.ExampleFilters();

    // Configurar filtros personalizados
    c.DocumentFilter<SwaggerDocumentFilter>();
});
```

### Documentación de Endpoints

#### Documentación Básica
```csharp
/// <summary>
/// Obtiene todos los usuarios del sistema
/// </summary>
/// <remarks>
/// Este endpoint retorna una lista paginada de todos los usuarios activos.
/// Puedes usar los parámetros de consulta para filtrar y paginar los resultados.
/// </remarks>
/// <param name="page">Número de página (por defecto: 1)</param>
/// <param name="pageSize">Tamaño de página (por defecto: 10, máximo: 100)</param>
/// <param name="search">Término de búsqueda opcional</param>
/// <param name="role">Filtrar por rol específico</param>
/// <returns>Lista paginada de usuarios</returns>
/// <response code="200">Lista de usuarios obtenida exitosamente</response>
/// <response code="400">Parámetros de consulta inválidos</response>
/// <response code="401">No autenticado</response>
/// <response code="403">No autorizado</response>
/// <response code="500">Error interno del servidor</response>
[HttpGet]
[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResult<UserDto>))]
[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationErrorResponse))]
[ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(UnauthorizedResponse))]
[ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ForbiddenResponse))]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerErrorResponse))]
[SwaggerOperation(
    Summary = "Obtener usuarios",
    Description = "Retorna una lista paginada de usuarios con opciones de filtrado y búsqueda",
    OperationId = "GetUsers",
    Tags = new[] { "Users" }
)]
[SwaggerResponse(200, "Lista de usuarios obtenida exitosamente", typeof(PaginatedResult<UserDto>))]
[SwaggerResponse(400, "Parámetros de consulta inválidos", typeof(ValidationErrorResponse))]
[SwaggerResponse(401, "No autenticado", typeof(UnauthorizedResponse))]
[SwaggerResponse(403, "No autorizado", typeof(ForbiddenResponse))]
[SwaggerResponse(500, "Error interno del servidor", typeof(InternalServerErrorResponse))]
public async Task<ActionResult<PaginatedResult<UserDto>>> GetUsers(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? search = null,
    [FromQuery] string? role = null)
{
    // Implementación...
}
```

#### Documentación de Endpoints POST
```csharp
/// <summary>
/// Crea un nuevo usuario en el sistema
/// </summary>
/// <remarks>
/// Crea un nuevo usuario con la información proporcionada. El email debe ser único.
/// La contraseña será hasheada antes de almacenarse en la base de datos.
/// </remarks>
/// <param name="createUserDto">Datos del usuario a crear</param>
/// <returns>Usuario creado con ID asignado</returns>
/// <response code="201">Usuario creado exitosamente</response>
/// <response code="400">Datos de entrada inválidos</response>
/// <response code="409">El email ya está registrado</response>
/// <response code="500">Error interno del servidor</response>
[HttpPost]
[ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserDto))]
[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationErrorResponse))]
[ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ConflictResponse))]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerErrorResponse))]
[SwaggerOperation(
    Summary = "Crear usuario",
    Description = "Crea un nuevo usuario en el sistema",
    OperationId = "CreateUser",
    Tags = new[] { "Users" }
)]
[SwaggerRequestExample(typeof(CreateUserDto), typeof(CreateUserDtoExample))]
[SwaggerResponse(201, "Usuario creado exitosamente", typeof(UserDto))]
[SwaggerResponse(400, "Datos de entrada inválidos", typeof(ValidationErrorResponse))]
[SwaggerResponse(409, "El email ya está registrado", typeof(ConflictResponse))]
[SwaggerResponse(500, "Error interno del servidor", typeof(InternalServerErrorResponse))]
public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
{
    // Implementación...
}
```

#### Documentación de Endpoints con Parámetros de Ruta
```csharp
/// <summary>
/// Obtiene un usuario específico por su ID
/// </summary>
/// <remarks>
/// Retorna la información completa de un usuario específico.
/// Si el usuario no existe, retorna un error 404.
/// </remarks>
/// <param name="id">ID único del usuario</param>
/// <returns>Información del usuario</returns>
/// <response code="200">Usuario encontrado exitosamente</response>
/// <response code="404">Usuario no encontrado</response>
/// <response code="500">Error interno del servidor</response>
[HttpGet("{id:int}")]
[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
[ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(NotFoundResponse))]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerErrorResponse))]
[SwaggerOperation(
    Summary = "Obtener usuario por ID",
    Description = "Retorna la información de un usuario específico",
    OperationId = "GetUserById",
    Tags = new[] { "Users" }
)]
[SwaggerParameter(
    Name = "id",
    Description = "ID único del usuario",
    Required = true,
    In = ParameterLocation.Path,
    Schema = new OpenApiSchema { Type = "integer", Minimum = 1 }
)]
[SwaggerResponse(200, "Usuario encontrado exitosamente", typeof(UserDto))]
[SwaggerResponse(404, "Usuario no encontrado", typeof(NotFoundResponse))]
[SwaggerResponse(500, "Error interno del servidor", typeof(InternalServerErrorResponse))]
public async Task<ActionResult<UserDto>> GetUser(int id)
{
    // Implementación...
}
```

### Configuración de Autenticación JWT

#### Configuración de Seguridad en Swagger
```csharp
builder.Services.AddSwaggerGen(c =>
{
    // ... configuración básica ...

    // Configurar autenticación JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

    // Configurar esquemas de respuesta de autenticación
    c.MapType<LoginResponseDto>(() => new OpenApiSchema
    {
        Type = "object",
        Properties = new Dictionary<string, OpenApiSchema>
        {
            ["accessToken"] = new OpenApiSchema { Type = "string", Description = "JWT access token" },
            ["refreshToken"] = new OpenApiSchema { Type = "string", Description = "JWT refresh token" },
            ["expiresAt"] = new OpenApiSchema { Type = "string", Format = "date-time", Description = "Fecha de expiración del token" },
            ["user"] = new OpenApiSchema { Type = "object", Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "UserDto" } }
        }
    });
});
```

#### Endpoints de Autenticación Documentados
```csharp
/// <summary>
/// Inicia sesión de un usuario
/// </summary>
/// <remarks>
/// Autentica un usuario con sus credenciales y retorna un token JWT.
/// El token debe incluirse en el header Authorization para endpoints protegidos.
/// </remarks>
/// <param name="loginDto">Credenciales de autenticación</param>
/// <returns>Token de acceso y información del usuario</returns>
/// <response code="200">Login exitoso</response>
/// <response code="400">Datos de entrada inválidos</response>
/// <response code="401">Credenciales inválidas</response>
/// <response code="500">Error interno del servidor</response>
[HttpPost("login")]
[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponseDto))]
[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationErrorResponse))]
[ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(UnauthorizedResponse))]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerErrorResponse))]
[SwaggerOperation(
    Summary = "Iniciar sesión",
    Description = "Autentica un usuario y retorna un token JWT",
    OperationId = "Login",
    Tags = new[] { "Authentication" }
)]
[SwaggerRequestExample(typeof(LoginDto), typeof(LoginDtoExample))]
[SwaggerResponse(200, "Login exitoso", typeof(LoginResponseDto))]
[SwaggerResponse(400, "Datos de entrada inválidos", typeof(ValidationErrorResponse))]
[SwaggerResponse(401, "Credenciales inválidas", typeof(UnauthorizedResponse))]
[SwaggerResponse(500, "Error interno del servidor", typeof(InternalServerErrorResponse))]
public async Task<ActionResult<LoginResponseDto>> Login(LoginDto loginDto)
{
    // Implementación...
}

/// <summary>
/// Refresca el token de acceso
/// </summary>
/// <remarks>
/// Usa un refresh token válido para obtener un nuevo access token.
/// El refresh token debe ser válido y no haber expirado.
/// </remarks>
/// <param name="refreshTokenDto">Token de acceso y refresh token</param>
/// <returns>Nuevos tokens de acceso y refresh</returns>
/// <response code="200">Tokens refrescados exitosamente</response>
/// <response code="400">Datos de entrada inválidos</response>
/// <response code="401">Refresh token inválido o expirado</response>
/// <response code="500">Error interno del servidor</response>
[HttpPost("refresh")]
[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RefreshTokenResponseDto))]
[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationErrorResponse))]
[ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(UnauthorizedResponse))]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(InternalServerErrorResponse))]
[SwaggerOperation(
    Summary = "Refrescar token",
    Description = "Obtiene nuevos tokens usando un refresh token válido",
    OperationId = "RefreshToken",
    Tags = new[] { "Authentication" }
)]
[SwaggerRequestExample(typeof(RefreshTokenDto), typeof(RefreshTokenDtoExample))]
[SwaggerResponse(200, "Tokens refrescados exitosamente", typeof(RefreshTokenResponseDto))]
[SwaggerResponse(400, "Datos de entrada inválidos", typeof(ValidationErrorResponse))]
[SwaggerResponse(401, "Refresh token inválido o expirado", typeof(UnauthorizedResponse))]
[SwaggerResponse(500, "Error interno del servidor", typeof(InternalServerErrorResponse))]
public async Task<ActionResult<RefreshTokenResponseDto>> RefreshToken(RefreshTokenDto refreshTokenDto)
{
    // Implementación...
}
```

### Ejemplos de Entrada y Salida

#### Ejemplos de DTOs
```csharp
public class CreateUserDtoExample : IExamplesProvider<CreateUserDto>
{
    public CreateUserDto GetExamples()
    {
        return new CreateUserDto
        {
            FirstName = "Juan",
            LastName = "Pérez",
            Email = "juan.perez@example.com",
            Password = "SecurePass123!",
            Role = "User"
        };
    }
}

public class LoginDtoExample : IExamplesProvider<LoginDto>
{
    public LoginDto GetExamples()
    {
        return new LoginDto
        {
            Email = "usuario@example.com",
            Password = "MiContraseña123!",
            RememberMe = false
        };
    }
}

public class RefreshTokenDtoExample : IExamplesProvider<RefreshTokenDto>
{
    public RefreshTokenDto GetExamples()
    {
        return new RefreshTokenDto
        {
            AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
            RefreshToken = "refresh_token_example_12345"
        };
    }
}
```

#### Ejemplos de Respuestas
```csharp
public class UserDtoExample : IExamplesProvider<UserDto>
{
    public UserDto GetExamples()
    {
        return new UserDto
        {
            Id = 1,
            FirstName = "Juan",
            LastName = "Pérez",
            Email = "juan.perez@example.com",
            Role = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            LastLoginAt = DateTime.UtcNow.AddHours(-2)
        };
    }
}

public class PaginatedResultExample : IExamplesProvider<PaginatedResult<UserDto>>
{
    public PaginatedResult<UserDto> GetExamples()
    {
        var users = new List<UserDto>
        {
            new UserDto
            {
                Id = 1,
                FirstName = "Juan",
                LastName = "Pérez",
                Email = "juan.perez@example.com",
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new UserDto
            {
                Id = 2,
                FirstName = "María",
                LastName = "García",
                Email = "maria.garcia@example.com",
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            }
        };

        return new PaginatedResult<UserDto>(users, 2, 1, 10);
    }
}
```

### Personalización de la Interfaz de Swagger

#### Configuración de la UI
```csharp
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mi API v1");
    c.RoutePrefix = string.Empty; // Swagger en la raíz
    
    // Personalizar la interfaz
    c.DocumentTitle = "Mi API - Documentación Completa";
    c.DefaultModelsExpandDepth(2);
    c.DefaultModelExpandDepth(2);
    c.DisplayRequestDuration();
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    
    // Configurar CSS personalizado
    c.InjectStylesheet("/swagger-ui/custom.css");
    c.InjectJavascript("/swagger-ui/custom.js");
    
    // Configurar OAuth2 si es necesario
    c.OAuthClientId("swagger-ui");
    c.OAuthClientSecret("swagger-secret");
    c.OAuthRealm("swagger-ui-realm");
    c.OAuthAppName("Swagger UI");
    c.OAuthScopeSeparator(" ");
    c.OAuthUsePkce();
    
    // Configurar parámetros por defecto
    c.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example);
    
    // Configurar filtros
    c.DisplayOperationId();
    c.Filter();
    
    // Configurar persistencia
    c.PersistAuthorization(true);
});
```

#### CSS Personalizado
```css
/* wwwroot/swagger-ui/custom.css */
.swagger-ui .topbar {
    background-color: #2c3e50;
}

.swagger-ui .topbar .download-url-wrapper input {
    border-color: #3498db;
}

.swagger-ui .info .title {
    color: #2c3e50;
}

.swagger-ui .opblock.opblock-get .opblock-summary-method {
    background-color: #61affe;
}

.swagger-ui .opblock.opblock-post .opblock-summary-method {
    background-color: #49cc90;
}

.swagger-ui .opblock.opblock-put .opblock-summary-method {
    background-color: #fca130;
}

.swagger-ui .opblock.opblock-delete .opblock-summary-method {
    background-color: #f93e3e;
}

.swagger-ui .opblock.opblock-patch .opblock-summary-method {
    background-color: #50e3c2;
}

.swagger-ui .btn.execute {
    background-color: #4990e2;
}

.swagger-ui .btn.execute:hover {
    background-color: #357abd;
}

.swagger-ui .scheme-container {
    background-color: #f8f9fa;
    border: 1px solid #dee2e6;
}

.swagger-ui .auth-wrapper {
    background-color: #f8f9fa;
    border: 1px solid #dee2e6;
}
```

#### JavaScript Personalizado
```javascript
// wwwroot/swagger-ui/custom.js
(function() {
    'use strict';
    
    // Personalizar la interfaz de Swagger
    window.addEventListener('load', function() {
        // Agregar botón de copiar URL
        addCopyUrlButton();
        
        // Personalizar ejemplos
        customizeExamples();
        
        // Agregar tooltips personalizados
        addCustomTooltips();
    });
    
    function addCopyUrlButton() {
        const opblocks = document.querySelectorAll('.opblock');
        opblocks.forEach(function(opblock) {
            const header = opblock.querySelector('.opblock-summary');
            if (header) {
                const copyButton = document.createElement('button');
                copyButton.className = 'copy-url-btn';
                copyButton.innerHTML = '📋';
                copyButton.title = 'Copiar URL';
                copyButton.onclick = function() {
                    const url = opblock.querySelector('.opblock-summary-path a').textContent;
                    navigator.clipboard.writeText(url).then(function() {
                        copyButton.innerHTML = '✅';
                        setTimeout(function() {
                            copyButton.innerHTML = '📋';
                        }, 2000);
                    });
                };
                header.appendChild(copyButton);
            }
        });
    }
    
    function customizeExamples() {
        // Personalizar ejemplos de entrada
        const examples = document.querySelectorAll('.examples-select');
        examples.forEach(function(example) {
            example.addEventListener('change', function() {
                const selectedExample = this.value;
                if (selectedExample) {
                    // Aplicar formato personalizado al ejemplo seleccionado
                    formatSelectedExample(selectedExample);
                }
            });
        });
    }
    
    function addCustomTooltips() {
        // Agregar tooltips personalizados a los parámetros
        const parameters = document.querySelectorAll('.parameters-container .parameter');
        parameters.forEach(function(parameter) {
            const name = parameter.querySelector('.parameter__name');
            if (name) {
                const tooltip = document.createElement('div');
                tooltip.className = 'custom-tooltip';
                tooltip.innerHTML = getParameterTooltip(name.textContent);
                parameter.appendChild(tooltip);
            }
        });
    }
    
    function getParameterTooltip(parameterName) {
        const tooltips = {
            'page': 'Número de página (comienza en 1)',
            'pageSize': 'Cantidad de elementos por página (máximo 100)',
            'search': 'Término de búsqueda para filtrar resultados',
            'sortBy': 'Campo por el cual ordenar los resultados',
            'sortOrder': 'Orden de clasificación (asc o desc)'
        };
        
        return tooltips[parameterName] || 'Parámetro de consulta';
    }
    
    function formatSelectedExample(exampleName) {
        // Implementar lógica de formateo personalizado
        console.log('Ejemplo seleccionado:', exampleName);
    }
})();
```

### Filtros y Operaciones Personalizadas

#### Filtros de Operación
```csharp
public class SwaggerDefaultValues : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiDescription = context.ApiDescription;
        
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
    }
}

public class SwaggerFileOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileUploadMethods = context.MethodInfo.GetCustomAttributes(true)
            .OfType<SwaggerFileUploadAttribute>()
            .Any();

        if (fileUploadMethods)
        {
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["file"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Format = "binary"
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class SwaggerFileUploadAttribute : Attribute
{
}
```

#### Filtros de Esquema
```csharp
public class SwaggerSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(DateTime))
        {
            schema.Format = "date-time";
            schema.Example = new OpenApiString(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        }
        else if (context.Type == typeof(DateTime?))
        {
            schema.Format = "date-time";
            schema.Nullable = true;
        }
        else if (context.Type == typeof(decimal))
        {
            schema.Format = "decimal";
            schema.Example = new OpenApiString("99.99");
        }
        else if (context.Type == typeof(decimal?))
        {
            schema.Format = "decimal";
            schema.Nullable = true;
        }
        else if (context.Type == typeof(Guid))
        {
            schema.Format = "uuid";
            schema.Example = new OpenApiString(Guid.NewGuid().ToString());
        }
        else if (context.Type == typeof(Guid?))
        {
            schema.Format = "uuid";
            schema.Nullable = true;
        }
    }
}
```

#### Filtros de Documento
```csharp
public class SwaggerDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Agregar información adicional al documento
        swaggerDoc.Info.Extensions.Add("x-api-version", new OpenApiString("1.0.0"));
        swaggerDoc.Info.Extensions.Add("x-supported-languages", new OpenApiArray
        {
            new OpenApiString("es"),
            new OpenApiString("en")
        });
        
        // Agregar servidores adicionales
        swaggerDoc.Servers.Add(new OpenApiServer
        {
            Url = "https://staging-api.example.com",
            Description = "Servidor de staging"
        });
        
        swaggerDoc.Servers.Add(new OpenApiServer
        {
            Url = "https://api.example.com",
            Description = "Servidor de producción"
        });
    }
}
```

### Generación de Documentación Automática

#### Configuración de Comentarios XML
```xml
<!-- MiApi.csproj -->
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

#### Documentación de Modelos
```csharp
/// <summary>
/// DTO para crear un nuevo usuario
/// </summary>
/// <remarks>
/// Este DTO contiene toda la información necesaria para crear un nuevo usuario en el sistema.
/// La contraseña debe cumplir con los requisitos de seguridad establecidos.
/// </remarks>
public class CreateUserDto
{
    /// <summary>
    /// Nombre del usuario
    /// </summary>
    /// <example>Juan</example>
    /// <remarks>
    /// Debe ser un nombre válido con solo letras y espacios.
    /// </remarks>
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Apellido del usuario
    /// </summary>
    /// <example>Pérez</example>
    /// <remarks>
    /// Debe ser un apellido válido con solo letras y espacios.
    /// </remarks>
    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(50, ErrorMessage = "El apellido no puede exceder 50 caracteres")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El apellido solo puede contener letras y espacios")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Dirección de correo electrónico del usuario
    /// </summary>
    /// <example>juan.perez@example.com</example>
    /// <remarks>
    /// Debe ser un email válido y único en el sistema.
    /// </remarks>
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña del usuario
    /// </summary>
    /// <example>SecurePass123!</example>
    /// <remarks>
    /// Debe cumplir con los requisitos de seguridad:
    /// - Mínimo 8 caracteres
    /// - Al menos una letra mayúscula
    /// - Al menos una letra minúscula
    /// - Al menos un número
    /// - Al menos un carácter especial
    /// </remarks>
    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]", 
        ErrorMessage = "La contraseña debe contener al menos una letra minúscula, una mayúscula, un número y un carácter especial")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Rol del usuario en el sistema
    /// </summary>
    /// <example>User</example>
    /// <remarks>
    /// Los roles disponibles son: User, Admin, Manager.
    /// Por defecto se asigna el rol "User".
    /// </remarks>
    [StringLength(20, ErrorMessage = "El rol no puede exceder 20 caracteres")]
    public string? Role { get; set; } = "User";
}
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Configuración Básica de Swagger
Configura Swagger en tu API con:
- Información básica de la API
- Comentarios XML
- Esquemas de respuesta por defecto
- Configuración de la UI

### Ejercicio 2: Documentación de Endpoints
Documenta completamente todos tus endpoints con:
- Descripciones detalladas
- Ejemplos de entrada y salida
- Códigos de respuesta
- Parámetros de consulta

### Ejercicio 3: Autenticación JWT en Swagger
Configura la autenticación JWT en Swagger con:
- Definición de esquema de seguridad
- Requisitos de seguridad
- Ejemplos de tokens
- Configuración de OAuth2

### Ejercicio 4: Personalización Avanzada
Personaliza la interfaz de Swagger con:
- CSS personalizado
- JavaScript personalizado
- Filtros de operación
- Filtros de esquema

## 📝 Quiz de Autoevaluación

1. ¿Cuáles son las ventajas de usar Swagger/OpenAPI?
2. ¿Cómo configurarías autenticación JWT en Swagger?
3. ¿Qué estrategias usarías para personalizar la interfaz de Swagger?
4. ¿Cómo implementarías ejemplos personalizados en Swagger?
5. ¿Qué consideraciones tendrías para la documentación en producción?

## 🔗 Enlaces Útiles

- [Swagger/OpenAPI Documentation](https://swagger.io/docs/)
- [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle)
- [OpenAPI Specification](https://swagger.io/specification/)
- [Swagger UI Customization](https://swagger.io/docs/open-source-tools/swagger-ui/customization/)

## 🚀 Siguiente Clase

En la siguiente clase aprenderás a implementar versionado de APIs, incluyendo diferentes estrategias de versionado y compatibilidad hacia atrás.

---

**💡 Consejo**: Siempre documenta tus APIs de manera completa y clara, incluyendo ejemplos prácticos y casos de uso reales para facilitar la adopción por parte de otros desarrolladores.
