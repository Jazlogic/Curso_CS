# 🚀 Clase 4: Validación y Manejo de Errores

## 🧭 Navegación
- **⬅️ Anterior**: [Clase 3: Controladores y Endpoints](clase_3_controladores_endpoints.md)
- **🏠 Inicio del Módulo**: [Módulo 10: APIs REST y Web APIs](README.md)
- **➡️ Siguiente**: [Clase 5: Autenticación y Autorización JWT](clase_5_autenticacion_autorizacion_jwt.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 📚 Descripción

En esta clase aprenderás a implementar validación robusta y manejo de errores en tus APIs, incluyendo middleware personalizado, filtros de excepción y respuestas de error consistentes.

## 🎯 Objetivos de Aprendizaje

- Implementar validación con Data Annotations
- Crear validadores personalizados con FluentValidation
- Implementar middleware de manejo de errores
- Crear filtros de excepción personalizados
- Manejar diferentes tipos de errores de manera consistente
- Implementar logging estructurado de errores

## 📖 Contenido Teórico

### Validación con Data Annotations

#### Validaciones Básicas
```csharp
public class CreateProductDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    [Display(Name = "Nombre del Producto")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria")]
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    [Display(Name = "Descripción")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    [Display(Name = "Precio")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "El stock es obligatorio")]
    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
    [Display(Name = "Stock Disponible")]
    public int Stock { get; set; }

    [Url(ErrorMessage = "La URL de la imagen no es válida")]
    [Display(Name = "URL de Imagen")]
    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria")]
    [Display(Name = "Categoría")]
    public string Category { get; set; } = string.Empty;

    [Display(Name = "Etiquetas")]
    public List<string>? Tags { get; set; }

    [Display(Name = "Fecha de Vencimiento")]
    [FutureDate(ErrorMessage = "La fecha de vencimiento debe ser futura")]
    public DateTime? ExpiryDate { get; set; }
}
```

#### Validaciones Personalizadas
```csharp
// Atributo personalizado para validar fechas futuras
public class FutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success; // Las fechas null son válidas

        if (value is DateTime date)
        {
            if (date <= DateTime.UtcNow)
            {
                return new ValidationResult(ErrorMessage ?? "La fecha debe ser futura");
            }
            return ValidationResult.Success;
        }

        return new ValidationResult("El valor debe ser una fecha válida");
    }
}

// Atributo personalizado para validar códigos de producto
public class ProductCodeAttribute : ValidationAttribute
{
    private readonly string _pattern = @"^[A-Z]{2,3}-\d{4,6}$";

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (value is string code)
        {
            if (Regex.IsMatch(code, _pattern))
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(ErrorMessage ?? "El código debe tener el formato XX-1234 o XXX-123456");
        }

        return new ValidationResult("El valor debe ser una cadena");
    }
}

// Uso del atributo personalizado
public class CreateProductDto
{
    [Required(ErrorMessage = "El código de producto es obligatorio")]
    [ProductCode(ErrorMessage = "El código debe tener el formato XX-1234")]
    [Display(Name = "Código de Producto")]
    public string ProductCode { get; set; } = string.Empty;
    
    // ... otras propiedades
}
```

#### Validaciones Condicionales
```csharp
public class CreateOrderDto
{
    [Required(ErrorMessage = "El ID del usuario es obligatorio")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Los items de la orden son obligatorios")]
    [MinLength(1, ErrorMessage = "La orden debe tener al menos un item")]
    public List<OrderItemDto> Items { get; set; } = new();

    [Display(Name = "Código de Descuento")]
    public string? DiscountCode { get; set; }

    [Display(Name = "Dirección de Envío")]
    public string? ShippingAddress { get; set; }

    [Display(Name = "Método de Pago")]
    public string? PaymentMethod { get; set; }

    // Validación condicional: si hay código de descuento, debe ser válido
    [RequiredIf("DiscountCode", ErrorMessage = "El código de descuento es requerido cuando se especifica")]
    [StringLength(20, ErrorMessage = "El código de descuento no puede exceder 20 caracteres")]
    public string? DiscountCodeValidation { get; set; }

    // Validación condicional: si es envío a domicilio, la dirección es obligatoria
    [RequiredIf("RequiresShipping", true, ErrorMessage = "La dirección de envío es obligatoria para envíos a domicilio")]
    public string? ShippingAddressValidation { get; set; }

    public bool RequiresShipping => !string.IsNullOrEmpty(ShippingAddress);
}

// Atributo personalizado para validación condicional
public class RequiredIfAttribute : ValidationAttribute
{
    private readonly string _dependentProperty;
    private readonly object? _expectedValue;

    public RequiredIfAttribute(string dependentProperty, object? expectedValue = null)
    {
        _dependentProperty = dependentProperty;
        _expectedValue = expectedValue;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var dependentProperty = validationContext.ObjectType.GetProperty(_dependentProperty);
        
        if (dependentProperty == null)
            return new ValidationResult($"Propiedad dependiente {_dependentProperty} no encontrada");

        var dependentValue = dependentProperty.GetValue(validationContext.ObjectInstance);

        // Si no hay valor esperado, solo verificar que la propiedad dependiente tenga valor
        if (_expectedValue == null)
        {
            if (dependentValue != null && !string.IsNullOrEmpty(dependentValue.ToString()))
            {
                if (value == null || string.IsNullOrEmpty(value.ToString()))
                {
                    return new ValidationResult(ErrorMessage ?? "Este campo es requerido");
                }
            }
        }
        else
        {
            // Si hay valor esperado, verificar que coincida
            if (Equals(dependentValue, _expectedValue))
            {
                if (value == null || string.IsNullOrEmpty(value.ToString()))
                {
                    return new ValidationResult(ErrorMessage ?? "Este campo es requerido");
                }
            }
        }

        return ValidationResult.Success;
    }
}
```

### FluentValidation

#### Instalación y Configuración
```bash
# Instalar FluentValidation
dotnet add package FluentValidation.AspNetCore
```

#### Validador Básico
```csharp
public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres")
            .Matches(@"^[a-zA-Z0-9\s\-_]+$").WithMessage("El nombre solo puede contener letras, números, espacios, guiones y guiones bajos");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción es obligatoria")
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("El precio debe ser mayor a 0")
            .PrecisionScale(18, 2, false).WithMessage("El precio no puede tener más de 2 decimales");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("El stock no puede ser negativo");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("La categoría es obligatoria")
            .MaximumLength(50).WithMessage("La categoría no puede exceder 50 caracteres");

        RuleFor(x => x.ImageUrl)
            .Must(BeAValidUrl).When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("La URL de la imagen no es válida");

        RuleFor(x => x.Tags)
            .Must(x => x == null || x.Count <= 10).WithMessage("No se pueden tener más de 10 etiquetas")
            .ForEach(tag => tag.MaximumLength(20).WithMessage("Cada etiqueta no puede exceder 20 caracteres"));

        RuleFor(x => x.ExpiryDate)
            .Must(BeFutureDate).When(x => x.ExpiryDate.HasValue)
            .WithMessage("La fecha de vencimiento debe ser futura");
    }

    private bool BeAValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }

    private bool BeFutureDate(DateTime? date)
    {
        return date.HasValue && date.Value > DateTime.UtcNow;
    }
}
```

#### Validador Avanzado con Reglas Condicionales
```csharp
public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("El ID del usuario debe ser válido");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("La orden debe tener al menos un item")
            .Must(items => items.Count <= 50).WithMessage("La orden no puede tener más de 50 items");

        RuleForEach(x => x.Items).SetValidator(new OrderItemDtoValidator());

        // Validación condicional para código de descuento
        When(x => !string.IsNullOrEmpty(x.DiscountCode), () =>
        {
            RuleFor(x => x.DiscountCode)
                .Length(5, 20).WithMessage("El código de descuento debe tener entre 5 y 20 caracteres")
                .Matches(@"^[A-Z0-9]+$").WithMessage("El código de descuento solo puede contener letras mayúsculas y números");
        });

        // Validación condicional para dirección de envío
        When(x => !string.IsNullOrEmpty(x.ShippingAddress), () =>
        {
            RuleFor(x => x.ShippingAddress)
                .MinimumLength(10).WithMessage("La dirección de envío debe tener al menos 10 caracteres")
                .MaximumLength(200).WithMessage("La dirección de envío no puede exceder 200 caracteres");
        });

        // Validación condicional para método de pago
        When(x => !string.IsNullOrEmpty(x.PaymentMethod), () =>
        {
            RuleFor(x => x.PaymentMethod)
                .Must(BeValidPaymentMethod).WithMessage("El método de pago no es válido");
        });

        // Validación de negocio: el total de la orden debe ser mayor a 0
        RuleFor(x => x)
            .Must(order => CalculateOrderTotal(order.Items) > 0)
            .WithMessage("El total de la orden debe ser mayor a 0");
    }

    private bool BeValidPaymentMethod(string paymentMethod)
    {
        var validMethods = new[] { "credit_card", "debit_card", "paypal", "bank_transfer" };
        return validMethods.Contains(paymentMethod.ToLower());
    }

    private decimal CalculateOrderTotal(List<OrderItemDto> items)
    {
        return items.Sum(item => item.Price * item.Quantity);
    }
}

public class OrderItemDtoValidator : AbstractValidator<OrderItemDto>
{
    public OrderItemDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("El ID del producto debe ser válido");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0")
            .LessThanOrEqualTo(100).WithMessage("La cantidad no puede exceder 100");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("El precio debe ser mayor a 0");
    }
}
```

#### Registro de Validadores
```csharp
// Program.cs
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();

// O registro manual
builder.Services.AddScoped<IValidator<CreateProductDto>, CreateProductDtoValidator>();
builder.Services.AddScoped<IValidator<CreateOrderDto>, CreateOrderDtoValidator>();
```

### Middleware de Manejo de Errores

#### Middleware Básico
```csharp
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no manejado en la aplicación");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            error = new
            {
                message = "Ha ocurrido un error interno del servidor",
                details = _environment.IsDevelopment() ? exception.Message : "Error interno",
                timestamp = DateTime.UtcNow,
                traceId = context.TraceIdentifier
            }
        };

        context.Response.StatusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            ValidationException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}
```

#### Middleware Avanzado con Manejo de Errores Específicos
```csharp
public class AdvancedErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AdvancedErrorHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public AdvancedErrorHandlingMiddleware(RequestDelegate next, ILogger<AdvancedErrorHandlingMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, details) = GetErrorDetails(exception);
        
        _logger.LogError(exception, "Error en {Method} {Path}: {Message}", 
            context.Request.Method, context.Request.Path, message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new
        {
            error = new
            {
                message = message,
                details = details,
                timestamp = DateTime.UtcNow,
                traceId = context.TraceIdentifier,
                path = context.Request.Path,
                method = context.Request.Method
            }
        };

        await context.Response.WriteAsJsonAsync(response);
    }

    private (int statusCode, string message, object? details) GetErrorDetails(Exception exception)
    {
        return exception switch
        {
            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                "Error de validación",
                new { errors = validationEx.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage }) }
            ),
            
            ArgumentException argEx => (
                StatusCodes.Status400BadRequest,
                "Parámetro inválido",
                new { parameter = argEx.ParamName, message = argEx.Message }
            ),
            
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "No autorizado",
                new { message = "Se requieren credenciales válidas" }
            ),
            
            InvalidOperationException invalidOpEx => (
                StatusCodes.Status400BadRequest,
                "Operación inválida",
                new { message = invalidOpEx.Message }
            ),
            
            KeyNotFoundException keyNotFoundEx => (
                StatusCodes.Status404NotFound,
                "Recurso no encontrado",
                new { message = keyNotFoundEx.Message }
            ),
            
            TimeoutException => (
                StatusCodes.Status408RequestTimeout,
                "Tiempo de espera agotado",
                new { message = "La operación tardó demasiado en completarse" }
            ),
            
            _ => (
                StatusCodes.Status500InternalServerError,
                "Error interno del servidor",
                _environment.IsDevelopment() 
                    ? new { message = exception.Message, stackTrace = exception.StackTrace }
                    : new { message = "Ha ocurrido un error interno" }
            )
        };
    }
}
```

### Filtros de Excepción

#### Filtro de Excepción Global
```csharp
public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Excepción no manejada en {Controller}.{Action}", 
            context.RouteData.Values["controller"], context.RouteData.Values["action"]);

        var result = new ObjectResult(new
        {
            error = new
            {
                message = "Ha ocurrido un error en el servidor",
                details = _environment.IsDevelopment() ? context.Exception.Message : "Error interno",
                timestamp = DateTime.UtcNow,
                path = context.HttpContext.Request.Path,
                method = context.HttpContext.Request.Method
            }
        })
        {
            StatusCode = GetStatusCode(context.Exception)
        };

        context.Result = result;
        context.ExceptionHandled = true;
    }

    private int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            ArgumentException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}
```

#### Filtro de Excepción para Controladores Específicos
```csharp
public class ProductExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ProductExceptionFilter> _logger;

    public ProductExceptionFilter(ILogger<ProductExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        if (context.Exception is ProductNotFoundException productNotFoundEx)
        {
            _logger.LogWarning("Producto no encontrado: {ProductId}", productNotFoundEx.ProductId);
            
            context.Result = new NotFoundObjectResult(new
            {
                error = new
                {
                    message = "Producto no encontrado",
                    productId = productNotFoundEx.ProductId,
                    timestamp = DateTime.UtcNow
                }
            });
            
            context.ExceptionHandled = true;
        }
        else if (context.Exception is ProductValidationException productValidationEx)
        {
            _logger.LogWarning("Error de validación en producto: {Message}", productValidationEx.Message);
            
            context.Result = new BadRequestObjectResult(new
            {
                error = new
                {
                    message = "Error de validación del producto",
                    details = productValidationEx.ValidationErrors,
                    timestamp = DateTime.UtcNow
                }
            });
            
            context.ExceptionHandled = true;
        }
    }
}

// Excepciones personalizadas
public class ProductNotFoundException : Exception
{
    public int ProductId { get; }

    public ProductNotFoundException(int productId) 
        : base($"Producto con ID {productId} no encontrado")
    {
        ProductId = productId;
    }
}

public class ProductValidationException : Exception
{
    public List<string> ValidationErrors { get; }

    public ProductValidationException(List<string> validationErrors) 
        : base("Error de validación del producto")
    {
        ValidationErrors = validationErrors;
    }
}
```

### Respuestas de Error Consistentes

#### Estructura de Error Estándar
```csharp
public class ApiErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; }
    public string TraceId { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public List<ValidationError>? ValidationErrors { get; set; }
    public string? ErrorCode { get; set; }

    public ApiErrorResponse(string message)
    {
        Message = message;
        Timestamp = DateTime.UtcNow;
    }
}

public class ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Value { get; set; }
}

// Extensiones para respuestas de error
public static class ControllerExtensions
{
    public static ActionResult ValidationError(this ControllerBase controller, ModelStateDictionary modelState)
    {
        var errors = modelState
            .Where(x => x.Value?.Errors.Count > 0)
            .Select(x => new ValidationError
            {
                Field = x.Key,
                Message = x.Value?.Errors.FirstOrDefault()?.ErrorMessage ?? "Error de validación",
                Value = x.Value?.AttemptedValue?.ToString()
            })
            .ToList();

        var response = new ApiErrorResponse("Error de validación")
        {
            ValidationErrors = errors,
            TraceId = controller.HttpContext.TraceIdentifier,
            Path = controller.HttpContext.Request.Path,
            Method = controller.HttpContext.Request.Method
        };

        return controller.BadRequest(response);
    }

    public static ActionResult NotFoundError(this ControllerBase controller, string message, string? errorCode = null)
    {
        var response = new ApiErrorResponse(message)
        {
            ErrorCode = errorCode,
            TraceId = controller.HttpContext.TraceIdentifier,
            Path = controller.HttpContext.Request.Path,
            Method = controller.HttpContext.Request.Method
        };

        return controller.NotFound(response);
    }

    public static ActionResult ConflictError(this ControllerBase controller, string message, string? errorCode = null)
    {
        var response = new ApiErrorResponse(message)
        {
            ErrorCode = errorCode,
            TraceId = controller.HttpContext.TraceIdentifier,
            Path = controller.HttpContext.Request.Path,
            Method = controller.HttpContext.Request.Method
        };

        return controller.Conflict(response);
    }
}
```

### Logging de Errores

#### Logging Estructurado
```csharp
public class ErrorLoggingService
{
    private readonly ILogger<ErrorLoggingService> _logger;

    public ErrorLoggingService(ILogger<ErrorLoggingService> logger)
    {
        _logger = logger;
    }

    public void LogValidationError(string operation, object data, List<ValidationError> errors)
    {
        _logger.LogWarning("Error de validación en {Operation}: {@Data}. Errores: {@Errors}", 
            operation, data, errors);
    }

    public void LogBusinessRuleViolation(string operation, string rule, object data)
    {
        _logger.LogWarning("Violación de regla de negocio en {Operation}: Regla '{Rule}'. Datos: {@Data}", 
            operation, rule, data);
    }

    public void LogSecurityViolation(string operation, string user, string resource, string action)
    {
        _logger.LogWarning("Violación de seguridad en {Operation}: Usuario '{User}' intentó {Action} en {Resource}", 
            operation, user, action, resource);
    }

    public void LogSystemError(string operation, Exception exception, object? context = null)
    {
        _logger.LogError(exception, "Error del sistema en {Operation}. Contexto: {@Context}", 
            operation, context);
    }
}
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Validación de Productos
Implementa validación completa para productos usando:
- Data Annotations
- FluentValidation
- Validaciones personalizadas
- Validaciones condicionales

### Ejercicio 2: Middleware de Errores
Crea middleware personalizado que maneje:
- Errores de validación
- Errores de negocio
- Errores de seguridad
- Errores del sistema

### Ejercicio 3: Filtros de Excepción
Implementa filtros para:
- Controladores específicos
- Tipos de excepción específicos
- Logging automático de errores
- Respuestas de error consistentes

### Ejercicio 4: Sistema de Logging
Crea un sistema de logging que registre:
- Errores de validación
- Violaciones de reglas de negocio
- Intentos de acceso no autorizado
- Errores del sistema

## 📝 Quiz de Autoevaluación

1. ¿Cuál es la diferencia entre Data Annotations y FluentValidation?
2. ¿Por qué es importante manejar errores de manera consistente?
3. ¿Cómo implementarías validación condicional en FluentValidation?
4. ¿Qué ventajas tiene usar middleware personalizado para manejo de errores?
5. ¿Cómo estructurarías las respuestas de error para diferentes tipos de cliente?

## 🔗 Enlaces Útiles

- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [ASP.NET Core Model Validation](https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation/)
- [ASP.NET Core Exception Filters](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters#exception-filters)
- [Structured Logging with Serilog](https://serilog.net/)

## 🚀 Siguiente Clase

En la siguiente clase aprenderás a implementar autenticación y autorización JWT, incluyendo generación de tokens, validación y políticas de autorización.

---

**💡 Consejo**: Siempre implementa logging estructurado para errores y usa códigos de error consistentes para facilitar el debugging y monitoreo en producción.
