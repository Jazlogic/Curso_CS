# 🚀 Clase 1: Manejo de Excepciones

## 📋 Información de la Clase

- **Módulo**: Mid Level 3 - Manejo de Excepciones y Generics
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Módulo 2 (Mid Level 2)

## 🎯 Objetivos de Aprendizaje

- Comprender el sistema de manejo de excepciones en C#
- Implementar manejo robusto de errores en aplicaciones
- Crear excepciones personalizadas
- Usar el patrón try-catch-finally efectivamente

---

## 📚 Navegación del Módulo 3

| Clase | Tema | Enlace |
|-------|------|--------|
| **Clase 1** | **Manejo de Excepciones** | ← Estás aquí |
| [Clase 2](clase_2_generics_basicos.md) | Generics Básicos | Siguiente → |
| [Clase 3](clase_3_generics_avanzados.md) | Generics Avanzados | |
| [Clase 4](clase_4_colecciones_genericas.md) | Colecciones Genéricas | |
| [Clase 5](clase_5_interfaces_genericas.md) | Interfaces Genéricas | |
| [Clase 6](clase_6_restricciones_generics.md) | Restricciones de Generics | |
| [Clase 7](clase_7_generics_reflection.md) | Generics y Reflection | |
| [Clase 8](clase_8_generics_performance.md) | Generics y Performance | |
| [Clase 9](clase_9_patrones_generics.md) | Patrones con Generics | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Biblioteca | |

**← [Volver al README del Módulo 3](../midLevel_3/README.md)**

---

## 📚 Contenido Teórico

### 1. Fundamentos de Excepciones

Las excepciones son eventos que ocurren durante la ejecución de un programa que interrumpen el flujo normal de instrucciones.

```csharp
// ===== MANEJO DE EXCEPCIONES - IMPLEMENTACIÓN COMPLETA =====
namespace ExceptionHandling
{
    // ===== EXCEPCIONES PERSONALIZADAS =====
    namespace CustomExceptions
    {
        public class ValidationException : Exception
        {
            public string FieldName { get; }
            public object InvalidValue { get; }
            
            public ValidationException(string message, string fieldName, object invalidValue) 
                : base(message)
            {
                FieldName = fieldName;
                InvalidValue = invalidValue;
            }
            
            public ValidationException(string message, string fieldName, object invalidValue, Exception innerException) 
                : base(message, innerException)
            {
                FieldName = fieldName;
                InvalidValue = invalidValue;
            }
        }
        
        public class BusinessRuleException : Exception
        {
            public string RuleName { get; }
            public string BusinessContext { get; }
            
            public BusinessRuleException(string message, string ruleName, string businessContext) 
                : base(message)
            {
                RuleName = ruleName;
                BusinessContext = businessContext;
            }
        }
        
        public class ResourceNotFoundException : Exception
        {
            public string ResourceType { get; }
            public string ResourceId { get; }
            
            public ResourceNotFoundException(string resourceType, string resourceId) 
                : base($"Recurso {resourceType} con ID {resourceId} no encontrado")
            {
                ResourceType = resourceType;
                ResourceId = resourceId;
            }
        }
        
        public class InsufficientPermissionsException : Exception
        {
            public string RequiredPermission { get; }
            public string UserId { get; }
            
            public InsufficientPermissionsException(string requiredPermission, string userId) 
                : base($"Usuario {userId} no tiene el permiso {requiredPermission}")
            {
                RequiredPermission = requiredPermission;
                UserId = userId;
            }
        }
    }
    
    // ===== MANEJO BÁSICO DE EXCEPCIONES =====
    namespace BasicExceptionHandling
    {
        public class ExceptionHandler
        {
            private readonly ILogger<ExceptionHandler> _logger;
            
            public ExceptionHandler(ILogger<ExceptionHandler> logger)
            {
                _logger = logger;
            }
            
            public T ExecuteWithExceptionHandling<T>(Func<T> operation, string operationName)
            {
                try
                {
                    _logger.LogInformation("Ejecutando operación: {OperationName}", operationName);
                    var result = operation();
                    _logger.LogInformation("Operación {OperationName} completada exitosamente", operationName);
                    return result;
                }
                catch (ValidationException ex)
                {
                    _logger.LogWarning("Error de validación en {OperationName}: {Message}", 
                        operationName, ex.Message);
                    throw;
                }
                catch (BusinessRuleException ex)
                {
                    _logger.LogWarning("Violación de regla de negocio en {OperationName}: {RuleName}", 
                        operationName, ex.RuleName);
                    throw;
                }
                catch (ResourceNotFoundException ex)
                {
                    _logger.LogWarning("Recurso no encontrado en {OperationName}: {ResourceType} {ResourceId}", 
                        operationName, ex.ResourceType, ex.ResourceId);
                    throw;
                }
                catch (InsufficientPermissionsException ex)
                {
                    _logger.LogWarning("Permisos insuficientes en {OperationName}: Usuario {UserId} necesita {Permission}", 
                        operationName, ex.UserId, ex.RequiredPermission);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inesperado en {OperationName}", operationName);
                    throw new Exception($"Error interno en {operationName}", ex);
                }
            }
            
            public async Task<T> ExecuteWithExceptionHandlingAsync<T>(Func<Task<T>> operation, string operationName)
            {
                try
                {
                    _logger.LogInformation("Ejecutando operación asíncrona: {OperationName}", operationName);
                    var result = await operation();
                    _logger.LogInformation("Operación asíncrona {OperationName} completada exitosamente", operationName);
                    return result;
                }
                catch (ValidationException ex)
                {
                    _logger.LogWarning("Error de validación en {OperationName}: {Message}", 
                        operationName, ex.Message);
                    throw;
                }
                catch (BusinessRuleException ex)
                {
                    _logger.LogWarning("Violación de regla de negocio en {OperationName}: {RuleName}", 
                        operationName, ex.RuleName);
                    throw;
                }
                catch (ResourceNotFoundException ex)
                {
                    _logger.LogWarning("Recurso no encontrado en {OperationName}: {ResourceType} {ResourceId}", 
                        operationName, ex.ResourceType, ex.ResourceId);
                    throw;
                }
                catch (InsufficientPermissionsException ex)
                {
                    _logger.LogWarning("Permisos insuficientes en {OperationName}: Usuario {UserId} necesita {Permission}", 
                        operationName, ex.UserId, ex.RequiredPermission);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inesperado en {OperationName}", operationName);
                    throw new Exception($"Error interno en {operationName}", ex);
                }
            }
        }
    }
    
    // ===== VALIDACIÓN CON EXCEPCIONES =====
    namespace ValidationWithExceptions
    {
        public class UserValidator
        {
            public void ValidateUser(User user)
            {
                var errors = new List<string>();
                
                if (string.IsNullOrWhiteSpace(user.Username))
                {
                    errors.Add("El nombre de usuario es requerido");
                }
                else if (user.Username.Length < 3)
                {
                    errors.Add("El nombre de usuario debe tener al menos 3 caracteres");
                }
                else if (user.Username.Length > 50)
                {
                    errors.Add("El nombre de usuario no puede exceder 50 caracteres");
                }
                
                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    errors.Add("El email es requerido");
                }
                else if (!IsValidEmail(user.Email))
                {
                    errors.Add("El formato del email no es válido");
                }
                
                if (user.Age < 13)
                {
                    errors.Add("La edad mínima es 13 años");
                }
                else if (user.Age > 120)
                {
                    errors.Add("La edad máxima es 120 años");
                }
                
                if (errors.Any())
                {
                    throw new ValidationException(
                        $"Validación fallida: {string.Join(", ", errors)}",
                        "User",
                        user);
                }
            }
            
            private bool IsValidEmail(string email)
            {
                try
                {
                    var addr = new System.Net.Mail.MailAddress(email);
                    return addr.Address == email;
                }
                catch
                {
                    return false;
                }
            }
        }
        
        public class ProductValidator
        {
            public void ValidateProduct(Product product)
            {
                if (string.IsNullOrWhiteSpace(product.Name))
                {
                    throw new ValidationException("El nombre del producto es requerido", "Name", product.Name);
                }
                
                if (product.Price < 0)
                {
                    throw new ValidationException("El precio no puede ser negativo", "Price", product.Price);
                }
                
                if (product.StockQuantity < 0)
                {
                    throw new ValidationException("La cantidad en stock no puede ser negativa", "StockQuantity", product.StockQuantity);
                }
                
                if (string.IsNullOrWhiteSpace(product.Category))
                {
                    throw new ValidationException("La categoría es requerida", "Category", product.Category);
                }
            }
        }
    }
    
    // ===== MANEJO DE RECURSOS =====
    namespace ResourceManagement
    {
        public class FileProcessor
        {
            private readonly ILogger<FileProcessor> _logger;
            
            public FileProcessor(ILogger<FileProcessor> logger)
            {
                _logger = logger;
            }
            
            public string ReadFileContent(string filePath)
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Archivo no encontrado: {filePath}", filePath);
                }
                
                try
                {
                    using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    using var reader = new StreamReader(stream);
                    return reader.ReadToEnd();
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogError(ex, "Acceso denegado al archivo: {FilePath}", filePath);
                    throw new InsufficientPermissionsException("ReadFile", filePath);
                }
                catch (IOException ex)
                {
                    _logger.LogError(ex, "Error de E/S al leer el archivo: {FilePath}", filePath);
                    throw;
                }
            }
            
            public async Task<string> ReadFileContentAsync(string filePath)
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Archivo no encontrado: {filePath}", filePath);
                }
                
                try
                {
                    using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    using var reader = new StreamReader(stream);
                    return await reader.ReadToEndAsync();
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogError(ex, "Acceso denegado al archivo: {FilePath}", filePath);
                    throw new InsufficientPermissionsException("ReadFile", filePath);
                }
                catch (IOException ex)
                {
                    _logger.LogError(ex, "Error de E/S al leer el archivo: {FilePath}", filePath);
                    throw;
                }
            }
            
            public void WriteFileContent(string filePath, string content)
            {
                try
                {
                    var directory = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    
                    using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                    using var writer = new StreamWriter(stream);
                    writer.Write(content);
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogError(ex, "Acceso denegado al archivo: {FilePath}", filePath);
                    throw new InsufficientPermissionsException("WriteFile", filePath);
                }
                catch (IOException ex)
                {
                    _logger.LogError(ex, "Error de E/S al escribir el archivo: {FilePath}", filePath);
                    throw;
                }
            }
        }
        
        public class DatabaseConnection : IDisposable
        {
            private readonly string _connectionString;
            private SqlConnection _connection;
            private bool _disposed = false;
            
            public DatabaseConnection(string connectionString)
            {
                _connectionString = connectionString;
            }
            
            public SqlConnection GetConnection()
            {
                if (_connection == null)
                {
                    _connection = new SqlConnection(_connectionString);
                }
                
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }
                
                return _connection;
            }
            
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            
            protected virtual void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        _connection?.Dispose();
                    }
                    _disposed = true;
                }
            }
        }
    }
    
    // ===== MANEJO DE EXCEPCIONES EN OPERACIONES ASÍNCRONAS =====
    namespace AsyncExceptionHandling
    {
        public class AsyncOperationHandler
        {
            private readonly ILogger<AsyncOperationHandler> _logger;
            
            public AsyncOperationHandler(ILogger<AsyncOperationHandler> logger)
            {
                _logger = logger;
            }
            
            public async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3, TimeSpan? delay = null)
            {
                var retryCount = 0;
                var actualDelay = delay ?? TimeSpan.FromSeconds(1);
                
                while (true)
                {
                    try
                    {
                        return await operation();
                    }
                    catch (Exception ex) when (retryCount < maxRetries && ShouldRetry(ex))
                    {
                        retryCount++;
                        _logger.LogWarning(ex, "Intento {RetryCount} fallido, reintentando en {Delay}ms", 
                            retryCount, actualDelay.TotalMilliseconds);
                        
                        await Task.Delay(actualDelay);
                        actualDelay = TimeSpan.FromMilliseconds(actualDelay.TotalMilliseconds * 2); // Exponential backoff
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Operación falló después de {RetryCount} intentos", retryCount);
                        throw;
                    }
                }
            }
            
            private bool ShouldRetry(Exception ex)
            {
                // Retry on transient exceptions
                return ex is TimeoutException ||
                       ex is HttpRequestException ||
                       ex is SqlException sqlEx && IsTransientSqlException(sqlEx);
            }
            
            private bool IsTransientSqlException(SqlException ex)
            {
                // SQL Server transient error numbers
                var transientErrors = new[] { 2, 53, 64, 233, 10053, 10054, 10060, 40197, 40501, 40613, 49918, 49919, 49920 };
                return transientErrors.Contains(ex.Number);
            }
            
            public async Task ExecuteWithTimeoutAsync(Func<Task> operation, TimeSpan timeout)
            {
                using var cts = new CancellationTokenSource(timeout);
                
                try
                {
                    await operation().WaitAsync(cts.Token);
                }
                catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
                {
                    throw new TimeoutException($"Operación excedió el tiempo límite de {timeout.TotalSeconds} segundos");
                }
            }
        }
    }
    
    // ===== LOGGING Y MONITOREO DE EXCEPCIONES =====
    namespace ExceptionLogging
    {
        public class ExceptionLogger
        {
            private readonly ILogger<ExceptionLogger> _logger;
            private readonly IConfiguration _configuration;
            
            public ExceptionLogger(ILogger<ExceptionLogger> logger, IConfiguration configuration)
            {
                _logger = logger;
                _configuration = configuration;
            }
            
            public void LogException(Exception ex, string context = null, object additionalData = null)
            {
                var logLevel = GetLogLevelForException(ex);
                var message = FormatExceptionMessage(ex, context, additionalData);
                
                _logger.Log(logLevel, ex, message);
                
                // Log to external monitoring service if configured
                if (_configuration.GetValue<bool>("Logging:EnableExternalMonitoring"))
                {
                    LogToExternalService(ex, context, additionalData);
                }
            }
            
            private LogLevel GetLogLevelForException(Exception ex)
            {
                return ex switch
                {
                    ValidationException => LogLevel.Warning,
                    BusinessRuleException => LogLevel.Warning,
                    ResourceNotFoundException => LogLevel.Warning,
                    InsufficientPermissionsException => LogLevel.Warning,
                    TimeoutException => LogLevel.Warning,
                    _ => LogLevel.Error
                };
            }
            
            private string FormatExceptionMessage(Exception ex, string context, object additionalData)
            {
                var message = $"Excepción en contexto: {context ?? "No especificado"}";
                
                if (additionalData != null)
                {
                    message += $", Datos adicionales: {JsonSerializer.Serialize(additionalData)}";
                }
                
                message += $", Tipo: {ex.GetType().Name}, Mensaje: {ex.Message}";
                
                return message;
            }
            
            private void LogToExternalService(Exception ex, string context, object additionalData)
            {
                // Implementation for external logging service (e.g., Application Insights, Loggly)
                try
                {
                    var logEntry = new
                    {
                        Timestamp = DateTime.UtcNow,
                        ExceptionType = ex.GetType().Name,
                        Message = ex.Message,
                        StackTrace = ex.StackTrace,
                        Context = context,
                        AdditionalData = additionalData,
                        Source = ex.Source,
                        InnerException = ex.InnerException?.Message
                    };
                    
                    // Send to external service
                    // _externalLoggingService.LogAsync(logEntry);
                }
                catch (Exception loggingEx)
                {
                    _logger.LogError(loggingEx, "Error al enviar log a servicio externo");
                }
            }
        }
    }
    
    // ===== MODELOS DE DATOS =====
    namespace Models
    {
        public class User
        {
            public string Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public int Age { get; set; }
            public DateTime CreatedAt { get; set; }
        }
        
        public class Product
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int StockQuantity { get; set; }
            public string Category { get; set; }
        }
    }
}

// Uso de Manejo de Excepciones
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Manejo de Excepciones - Clase 1 ===\n");
        
        Console.WriteLine("Los componentes implementados incluyen:");
        Console.WriteLine("1. Excepciones personalizadas (Validation, BusinessRule, ResourceNotFound)");
        Console.WriteLine("2. Manejador de excepciones con logging");
        Console.WriteLine("3. Validadores con excepciones específicas");
        Console.WriteLine("4. Manejo de recursos con using statements");
        Console.WriteLine("5. Operaciones asíncronas con retry y timeout");
        Console.WriteLine("6. Sistema de logging y monitoreo de excepciones");
        
        Console.WriteLine("\nBeneficios de esta implementación:");
        Console.WriteLine("- Manejo robusto de errores en aplicaciones");
        Console.WriteLine("- Excepciones específicas para diferentes tipos de errores");
        Console.WriteLine("- Logging detallado para debugging y monitoreo");
        Console.WriteLine("- Manejo seguro de recursos con Dispose pattern");
        Console.WriteLine("- Operaciones asíncronas con manejo de errores");
        Console.WriteLine("- Validación robusta con mensajes claros");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Calculadora Segura
Crea una calculadora que maneje excepciones para división por cero y entradas inválidas.

### Ejercicio 2: Validador de Formularios
Implementa un validador que lance excepciones personalizadas para campos inválidos.

### Ejercicio 3: Procesador de Archivos
Crea un procesador que maneje excepciones de archivos no encontrados y permisos.

## 🔍 Puntos Clave

1. **Excepciones personalizadas** para diferentes tipos de errores
2. **Manejo robusto** con try-catch-finally
3. **Using statements** para manejo automático de recursos
4. **Logging detallado** para debugging y monitoreo
5. **Operaciones asíncronas** con manejo de errores

## 📚 Recursos Adicionales

- [Microsoft Docs - Exception Handling](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/exceptions/)
- [Exception Handling Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)

---

**🎯 ¡Has completado la Clase 1! Ahora comprendes el Manejo de Excepciones**

**📚 [Siguiente: Clase 2 - Generics Básicos](clase_2_generics_basicos.md)**
