# 🚀 Clase 3: Controladores y Endpoints

## 🧭 Navegación
- **⬅️ Anterior**: [Clase 2: Configuración ASP.NET Core Web API](clase_2_configuracion_aspnet_core.md)
- **🏠 Inicio del Módulo**: [Módulo 10: APIs REST y Web APIs](README.md)
- **➡️ Siguiente**: [Clase 4: Validación y Manejo de Errores](clase_4_validacion_manejo_errores.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 📚 Descripción

En esta clase aprenderás a crear controladores profesionales en ASP.NET Core, implementando operaciones CRUD, siguiendo las mejores prácticas de diseño y creando endpoints bien estructurados.

## 🎯 Objetivos de Aprendizaje

- Crear controladores siguiendo las mejores prácticas
- Implementar operaciones CRUD completas
- Usar DTOs para entrada y salida de datos
- Implementar filtros y paginación
- Crear endpoints con documentación Swagger
- Aplicar principios de diseño REST

## 📖 Contenido Teórico

### Estructura de Controladores

#### Controlador Básico
```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }
}
```

#### Controlador con Versionado
```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsersController : ControllerBase
{
    // Endpoints v1.0
}

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsersV2Controller : ControllerBase
{
    // Endpoints v2.0 con funcionalidades adicionales
}
```

### Operaciones CRUD Básicas

#### GET - Obtener Recursos
```csharp
[HttpGet]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
{
    try
    {
        _logger.LogInformation("Obteniendo lista de usuarios");
        
        var users = await _userService.GetAllUsersAsync();
        
        _logger.LogInformation("Se obtuvieron {Count} usuarios", users.Count());
        
        return Ok(users);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al obtener usuarios");
        return StatusCode(500, "Error interno del servidor");
    }
}

[HttpGet("{id:int}")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<UserDto>> GetUser(int id)
{
    try
    {
        _logger.LogInformation("Obteniendo usuario con ID: {UserId}", id);
        
        var user = await _userService.GetUserByIdAsync(id);
        
        if (user == null)
        {
            _logger.LogWarning("Usuario con ID {UserId} no encontrado", id);
            return NotFound(new { message = $"Usuario con ID {id} no encontrado" });
        }
        
        _logger.LogInformation("Usuario con ID {UserId} obtenido exitosamente", id);
        return Ok(user);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al obtener usuario con ID: {UserId}", id);
        return StatusCode(500, "Error interno del servidor");
    }
}
```

#### POST - Crear Recursos
```csharp
[HttpPost]
[ProducesResponseType(StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status409Conflict)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
{
    try
    {
        _logger.LogInformation("Creando nuevo usuario con email: {Email}", createUserDto.Email);
        
        // Validar si el usuario ya existe
        var existingUser = await _userService.GetUserByEmailAsync(createUserDto.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Intento de crear usuario con email duplicado: {Email}", createUserDto.Email);
            return Conflict(new { message = "El email ya está registrado" });
        }
        
        var user = await _userService.CreateUserAsync(createUserDto);
        
        _logger.LogInformation("Usuario creado exitosamente con ID: {UserId}", user.Id);
        
        // Retornar 201 Created con Location header
        return CreatedAtAction(
            nameof(GetUser), 
            new { id = user.Id }, 
            user);
    }
    catch (ValidationException ex)
    {
        _logger.LogWarning("Error de validación al crear usuario: {Message}", ex.Message);
        return BadRequest(new { message = "Datos de entrada inválidos", errors = ex.Errors });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al crear usuario");
        return StatusCode(500, "Error interno del servidor");
    }
}
```

#### PUT - Actualizar Recursos Completos
```csharp
[HttpPut("{id:int}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updateUserDto)
{
    try
    {
        _logger.LogInformation("Actualizando usuario con ID: {UserId}", id);
        
        // Validar que el ID en la URL coincida con el del DTO
        if (id != updateUserDto.Id)
        {
            _logger.LogWarning("ID en URL ({UrlId}) no coincide con ID en DTO ({DtoId})", id, updateUserDto.Id);
            return BadRequest(new { message = "El ID en la URL no coincide con el ID en el cuerpo de la solicitud" });
        }
        
        // Verificar que el usuario existe
        var existingUser = await _userService.GetUserByIdAsync(id);
        if (existingUser == null)
        {
            _logger.LogWarning("Usuario con ID {UserId} no encontrado para actualización", id);
            return NotFound(new { message = $"Usuario con ID {id} no encontrado" });
        }
        
        var result = await _userService.UpdateUserAsync(updateUserDto);
        
        if (!result)
        {
            _logger.LogWarning("No se pudo actualizar el usuario con ID: {UserId}", id);
            return BadRequest(new { message = "No se pudo actualizar el usuario" });
        }
        
        _logger.LogInformation("Usuario con ID {UserId} actualizado exitosamente", id);
        return NoContent(); // 204 No Content
    }
    catch (ValidationException ex)
    {
        _logger.LogWarning("Error de validación al actualizar usuario: {Message}", ex.Message);
        return BadRequest(new { message = "Datos de entrada inválidos", errors = ex.Errors });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al actualizar usuario con ID: {UserId}", id);
        return StatusCode(500, "Error interno del servidor");
    }
}
```

#### PATCH - Actualizar Recursos Parcialmente
```csharp
[HttpPatch("{id:int}")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<UserDto>> PatchUser(int id, JsonPatchDocument<UpdateUserDto> patchDoc)
{
    try
    {
        _logger.LogInformation("Aplicando patch al usuario con ID: {UserId}", id);
        
        if (patchDoc == null)
        {
            _logger.LogWarning("Documento de patch es null para usuario con ID: {UserId}", id);
            return BadRequest(new { message = "Documento de patch es requerido" });
        }
        
        // Obtener usuario existente
        var existingUser = await _userService.GetUserByIdAsync(id);
        if (existingUser == null)
        {
            _logger.LogWarning("Usuario con ID {UserId} no encontrado para patch", id);
            return NotFound(new { message = $"Usuario con ID {id} no encontrado" });
        }
        
        // Crear DTO de actualización
        var updateUserDto = new UpdateUserDto
        {
            Id = existingUser.Id,
            FirstName = existingUser.FirstName,
            LastName = existingUser.LastName,
            Email = existingUser.Email
        };
        
        // Aplicar patch
        patchDoc.ApplyTo(updateUserDto, ModelState);
        
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState inválido después de aplicar patch para usuario con ID: {UserId}", id);
            return BadRequest(ModelState);
        }
        
        // Validar el DTO resultante
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(updateUserDto, new ValidationContext(updateUserDto), validationResults, true);
        
        if (!isValid)
        {
            _logger.LogWarning("Validación falló después de aplicar patch para usuario con ID: {UserId}", id);
            return BadRequest(new { message = "Datos inválidos después de aplicar patch", errors = validationResults });
        }
        
        var result = await _userService.UpdateUserAsync(updateUserDto);
        
        if (!result)
        {
            _logger.LogWarning("No se pudo aplicar patch al usuario con ID: {UserId}", id);
            return BadRequest(new { message = "No se pudo actualizar el usuario" });
        }
        
        // Obtener usuario actualizado
        var updatedUser = await _userService.GetUserByIdAsync(id);
        
        _logger.LogInformation("Patch aplicado exitosamente al usuario con ID: {UserId}", id);
        return Ok(updatedUser);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al aplicar patch al usuario con ID: {UserId}", id);
        return StatusCode(500, "Error interno del servidor");
    }
}
```

#### DELETE - Eliminar Recursos
```csharp
[HttpDelete("{id:int}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> DeleteUser(int id)
{
    try
    {
        _logger.LogInformation("Eliminando usuario con ID: {UserId}", id);
        
        // Verificar que el usuario existe
        var existingUser = await _userService.GetUserByIdAsync(id);
        if (existingUser == null)
        {
            _logger.LogWarning("Usuario con ID {UserId} no encontrado para eliminación", id);
            return NotFound(new { message = $"Usuario con ID {id} no encontrado" });
        }
        
        var result = await _userService.DeleteUserAsync(id);
        
        if (!result)
        {
            _logger.LogWarning("No se pudo eliminar el usuario con ID: {UserId}", id);
            return BadRequest(new { message = "No se pudo eliminar el usuario" });
        }
        
        _logger.LogInformation("Usuario con ID {UserId} eliminado exitosamente", id);
        return NoContent(); // 204 No Content
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al eliminar usuario con ID: {UserId}", id);
        return StatusCode(500, "Error interno del servidor");
    }
}
```

### Filtros y Paginación

#### Filtros Básicos
```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers(
    [FromQuery] string? search,
    [FromQuery] string? role,
    [FromQuery] bool? active)
{
    try
    {
        _logger.LogInformation("Obteniendo usuarios con filtros: Search={Search}, Role={Role}, Active={Active}", 
            search, role, active);
        
        var users = await _userService.GetUsersWithFiltersAsync(search, role, active);
        
        return Ok(users);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al obtener usuarios con filtros");
        return StatusCode(500, "Error interno del servidor");
    }
}
```

#### Paginación Avanzada
```csharp
[HttpGet]
public async Task<ActionResult<PaginatedResult<UserDto>>> GetUsers(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? sortBy = "lastName",
    [FromQuery] string? sortOrder = "asc",
    [FromQuery] string? search = null,
    [FromQuery] string? role = null)
{
    try
    {
        // Validar parámetros de paginación
        if (page < 1)
        {
            return BadRequest(new { message = "El número de página debe ser mayor a 0" });
        }
        
        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { message = "El tamaño de página debe estar entre 1 y 100" });
        }
        
        // Validar parámetros de ordenamiento
        var allowedSortFields = new[] { "firstName", "lastName", "email", "createdAt" };
        if (!allowedSortFields.Contains(sortBy?.ToLower()))
        {
            return BadRequest(new { message = $"Campo de ordenamiento inválido. Campos permitidos: {string.Join(", ", allowedSortFields)}" });
        }
        
        var allowedSortOrders = new[] { "asc", "desc" };
        if (!allowedSortOrders.Contains(sortOrder?.ToLower()))
        {
            return BadRequest(new { message = "Orden de clasificación inválido. Use 'asc' o 'desc'" });
        }
        
        _logger.LogInformation("Obteniendo usuarios paginados: Page={Page}, PageSize={PageSize}, SortBy={SortBy}, SortOrder={SortOrder}", 
            page, pageSize, sortBy, sortOrder);
        
        var result = await _userService.GetUsersPaginatedAsync(page, pageSize, sortBy, sortOrder, search, role);
        
        // Agregar headers de paginación
        Response.Headers.Add("X-Total-Count", result.TotalCount.ToString());
        Response.Headers.Add("X-Total-Pages", result.TotalPages.ToString());
        Response.Headers.Add("X-Current-Page", result.CurrentPage.ToString());
        Response.Headers.Add("X-Page-Size", result.PageSize.ToString());
        Response.Headers.Add("X-Has-Previous", result.HasPrevious.ToString());
        Response.Headers.Add("X-Has-Next", result.HasNext.ToString());
        
        return Ok(result);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al obtener usuarios paginados");
        return StatusCode(500, "Error interno del servidor");
    }
}
```

### DTOs y Modelos

#### DTOs de Entrada
```csharp
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
    [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]", 
        ErrorMessage = "La contraseña debe contener al menos una letra minúscula, una mayúscula, un número y un carácter especial")]
    public string Password { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "El rol no puede exceder 20 caracteres")]
    public string? Role { get; set; } = "User";
}

public class UpdateUserDto
{
    public int Id { get; set; }
    
    [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
    public string? FirstName { get; set; }
    
    [StringLength(50, ErrorMessage = "El apellido no puede exceder 50 caracteres")]
    public string? LastName { get; set; }
    
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
    public string? Email { get; set; }
    
    [StringLength(20, ErrorMessage = "El rol no puede exceder 20 caracteres")]
    public string? Role { get; set; }
}
```

#### DTOs de Salida
```csharp
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

public class UserDetailDto : UserDto
{
    public string FullName => $"{FirstName} {LastName}";
    public int OrderCount { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
```

#### Resultado Paginado
```csharp
public class PaginatedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
    
    public PaginatedResult(IEnumerable<T> items, int totalCount, int currentPage, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }
}
```

### Filtros de Acción

#### Filtro de Validación
```csharp
public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .Select(x => new
                {
                    field = x.Key,
                    errors = x.Value?.Errors.Select(e => e.ErrorMessage) ?? Enumerable.Empty<string>()
                })
                .ToList();

            context.Result = new BadRequestObjectResult(new
            {
                message = "Error de validación",
                errors = errors
            });
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Lógica después de la ejecución si es necesaria
    }
}
```

#### Filtro de Logging
```csharp
public class LoggingActionFilter : IActionFilter
{
    private readonly ILogger<LoggingActionFilter> _logger;

    public LoggingActionFilter(ILogger<LoggingActionFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var controllerName = context.Controller.GetType().Name;
        var actionName = context.ActionDescriptor.DisplayName;
        var parameters = context.ActionArguments;

        _logger.LogInformation(
            "Ejecutando acción {ActionName} en controlador {ControllerName} con parámetros {@Parameters}",
            actionName, controllerName, parameters);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        var controllerName = context.Controller.GetType().Name;
        var actionName = context.ActionDescriptor.DisplayName;
        var result = context.Result;

        _logger.LogInformation(
            "Acción {ActionName} en controlador {ControllerName} completada con resultado {Result}",
            actionName, controllerName, result);
    }
}
```

### Documentación Swagger

#### Documentación de Endpoints
```csharp
/// <summary>
/// Obtiene todos los usuarios con filtros opcionales
/// </summary>
/// <param name="search">Término de búsqueda para filtrar usuarios</param>
/// <param name="role">Rol específico para filtrar</param>
/// <param name="active">Estado activo para filtrar</param>
/// <returns>Lista de usuarios que coinciden con los filtros</returns>
/// <response code="200">Lista de usuarios obtenida exitosamente</response>
/// <response code="400">Parámetros de filtro inválidos</response>
/// <response code="500">Error interno del servidor</response>
[HttpGet]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers(
    [FromQuery] string? search,
    [FromQuery] string? role,
    [FromQuery] bool? active)
{
    // Implementación...
}

/// <summary>
/// Crea un nuevo usuario en el sistema
/// </summary>
/// <param name="createUserDto">Datos del usuario a crear</param>
/// <returns>Usuario creado con ID asignado</returns>
/// <response code="201">Usuario creado exitosamente</response>
/// <response code="400">Datos de entrada inválidos</response>
/// <response code="409">El email ya está registrado</response>
/// <response code="500">Error interno del servidor</response>
[HttpPost]
[ProducesResponseType(StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status409Conflict)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
{
    // Implementación...
}
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Controlador de Productos
Crea un controlador completo para productos con:
- Operaciones CRUD completas
- Filtros por categoría, precio y disponibilidad
- Paginación y ordenamiento
- Validación de entrada
- Logging detallado

### Ejercicio 2: Controlador de Órdenes
Implementa un controlador para órdenes que incluya:
- Crear nueva orden
- Obtener órdenes por usuario
- Actualizar estado de orden
- Cancelar orden
- Historial de órdenes

### Ejercicio 3: Filtros Personalizados
Crea filtros personalizados para:
- Validación automática de entrada
- Logging de acciones
- Medición de tiempo de ejecución
- Cache de respuestas

### Ejercicio 4: Documentación Swagger
Documenta completamente todos los endpoints con:
- Descripciones detalladas
- Ejemplos de entrada y salida
- Códigos de respuesta
- Parámetros de consulta

## 📝 Quiz de Autoevaluación

1. ¿Cuál es la diferencia entre PUT y PATCH?
2. ¿Por qué es importante usar DTOs en lugar de modelos de dominio directamente?
3. ¿Qué ventajas tiene implementar paginación en los endpoints?
4. ¿Cómo implementarías validación personalizada en un controlador?
5. ¿Qué son los filtros de acción y cuándo los usarías?

## 🔗 Enlaces Útiles

- [ASP.NET Core Controllers](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/)
- [Model Validation](https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation/)
- [Action Filters](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters/)
- [Swagger Documentation](https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle)

## 🚀 Siguiente Clase

En la siguiente clase aprenderás a implementar validación robusta y manejo de errores, incluyendo middleware personalizado y filtros de excepción.

---

**💡 Consejo**: Siempre documenta tus endpoints con Swagger y usa logging estructurado para facilitar el debugging y monitoreo en producción.
