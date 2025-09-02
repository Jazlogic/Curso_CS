# ✅ Clase 6: Validaciones de Negocio Avanzadas

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 5: Autenticación y Autorización Avanzada](../senior_7/clase_5_autenticacion_autorizacion_avanzada.md)
- **🏠 Inicio del Módulo**: [Módulo 14: Plataformas Empresariales Reales](../senior_7/README.md)
- **➡️ Siguiente**: [Clase 7: Sistema de Notificaciones](../senior_7/clase_7_sistema_notificaciones.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** validaciones de negocio complejas y dinámicas
2. **Crear** sistemas de reglas de negocio configurables
3. **Desarrollar** validadores de dominio especializados
4. **Diseñar** sistemas de validación en tiempo real
5. **Aplicar** patrones de validación avanzados

---

## 🏗️ **Sistema de Validaciones de Dominio**

### **Validadores de Entidades de Negocio**

```csharp
public interface IBusinessRuleValidator<TEntity>
{
    Task<ValidationResult> ValidateAsync(TEntity entity, ValidationContext context);
    IEnumerable<IBusinessRule<TEntity>> GetBusinessRules();
    void AddBusinessRule(IBusinessRule<TEntity> rule);
}

public interface IBusinessRule<TEntity>
{
    string RuleName { get; }
    string Description { get; }
    ValidationSeverity Severity { get; }
    Task<bool> IsValidAsync(TEntity entity, ValidationContext context);
    Task<string> GetErrorMessageAsync(TEntity entity, ValidationContext context);
    Task<string> GetSuggestionAsync(TEntity entity, ValidationContext context);
}

public class ValidationContext
{
    public Guid UserId { get; set; }
    public string UserRole { get; set; }
    public DateTime ValidationTime { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; }
    public ValidationMode Mode { get; set; }
}

public enum ValidationMode
{
    Create,     // Validación para creación
    Update,     // Validación para actualización
    Delete,     // Validación para eliminación
    Business    // Validación de reglas de negocio
}

// Validador para solicitudes musicales
public class MusicianRequestBusinessValidator : IBusinessRuleValidator<MusicianRequest>
{
    private readonly List<IBusinessRule<MusicianRequest>> _businessRules;
    private readonly IMusicianRepository _musicianRepository;
    private readonly ICalendarService _calendarService;
    private readonly ILocationService _locationService;
    private readonly ILogger<MusicianRequestBusinessValidator> _logger;

    public MusicianRequestBusinessValidator(
        IMusicianRepository musicianRepository,
        ICalendarService calendarService,
        ILocationService locationService,
        ILogger<MusicianRequestBusinessValidator> logger)
    {
        _businessRules = new List<IBusinessRule<MusicianRequest>>();
        _musicianRepository = musicianRepository;
        _calendarService = calendarService;
        _locationService = locationService;
        _logger = logger;
        InitializeBusinessRules();
    }

    private void InitializeBusinessRules()
    {
        // Regla: Disponibilidad de músicos
        AddBusinessRule(new MusicianAvailabilityRule(_musicianRepository));
        
        // Regla: Conflictos de calendario
        AddBusinessRule(new CalendarConflictRule(_calendarService));
        
        // Regla: Distancia máxima
        AddBusinessRule(new MaximumDistanceRule(_locationService));
        
        // Regla: Presupuesto por tipo de evento
        AddBusinessRule(new BudgetByEventTypeRule());
        
        // Regla: Duración del evento
        AddBusinessRule(new EventDurationRule());
        
        // Regla: Verificación de organizador
        AddBusinessRule(new OrganizerVerificationRule());
    }

    public void AddBusinessRule(IBusinessRule<MusicianRequest> rule)
    {
        _businessRules.Add(rule);
        _logger.LogDebug("Added business rule: {RuleName}", rule.RuleName);
    }

    public IEnumerable<IBusinessRule<MusicianRequest>> GetBusinessRules()
    {
        return _businessRules.AsReadOnly();
    }

    public async Task<ValidationResult> ValidateAsync(MusicianRequest entity, ValidationContext context)
    {
        var result = new ValidationResult();
        
        try
        {
            _logger.LogInformation("Validating business rules for request {RequestId}", entity.Id);

            foreach (var rule in _businessRules)
            {
                try
                {
                    var isValid = await rule.IsValidAsync(entity, context);
                    
                    if (!isValid)
                    {
                        var errorMessage = await rule.GetErrorMessageAsync(entity, context);
                        var suggestion = await rule.GetSuggestionAsync(entity, context);
                        
                        if (rule.Severity == ValidationSeverity.Error)
                        {
                            result.AddError("BusinessRule", errorMessage, rule.RuleName);
                        }
                        else if (rule.Severity == ValidationSeverity.Warning)
                        {
                            result.AddWarning("BusinessRule", errorMessage, rule.RuleName);
                        }
                        
                        if (!string.IsNullOrEmpty(suggestion))
                        {
                            result.AddMetadata($"Suggestion_{rule.RuleName}", suggestion);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing business rule {RuleName}", rule.RuleName);
                    result.AddError("System", $"Error en regla de negocio: {rule.RuleName}", rule.RuleName);
                }
            }

            result.AddMetadata("ValidationTimestamp", DateTime.UtcNow);
            result.AddMetadata("BusinessRulesExecuted", _businessRules.Count);
            result.AddMetadata("ValidationMode", context.Mode);

            _logger.LogInformation("Business validation completed for request {RequestId}. IsValid: {IsValid}", 
                entity.Id, result.IsValid);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during business validation for request {RequestId}", entity.Id);
            result.AddError("System", "Error interno durante la validación de negocio", "ValidationError");
            return result;
        }
    }
}
```

---

## 📋 **Reglas de Negocio Específicas**

### **Implementación de Reglas de Validación**

```csharp
// Regla: Disponibilidad de músicos
public class MusicianAvailabilityRule : IBusinessRule<MusicianRequest>
{
    public string RuleName => "MusicianAvailability";
    public string Description => "Debe haber músicos disponibles para el instrumento y fecha";
    public ValidationSeverity Severity => ValidationSeverity.Error;

    private readonly IMusicianRepository _musicianRepository;

    public MusicianAvailabilityRule(IMusicianRepository musicianRepository)
    {
        _musicianRepository = musicianRepository;
    }

    public async Task<bool> IsValidAsync(MusicianRequest entity, ValidationContext context)
    {
        if (context.Mode == ValidationMode.Create || context.Mode == ValidationMode.Update)
        {
            var availableMusicians = await _musicianRepository.GetAvailableMusiciansAsync(
                entity.Instrument, entity.Date, entity.Location, entity.DurationHours);
            return availableMusicians.Any();
        }
        return true;
    }

    public async Task<string> GetErrorMessageAsync(MusicianRequest entity, ValidationContext context)
    {
        return $"No hay músicos disponibles para {entity.Instrument} en {entity.Date:yyyy-MM-dd} en {entity.Location}";
    }

    public async Task<string> GetSuggestionAsync(MusicianRequest entity, ValidationContext context)
    {
        return "Considera cambiar la fecha, ubicación o instrumento, o aumentar el presupuesto para atraer más músicos";
    }
}

// Regla: Conflictos de calendario
public class CalendarConflictRule : IBusinessRule<MusicianRequest>
{
    public string RuleName => "CalendarConflict";
    public string Description => "No debe haber conflictos de calendario para el organizador";
    public ValidationSeverity Severity => ValidationSeverity.Error;

    private readonly ICalendarService _calendarService;

    public CalendarConflictRule(ICalendarService calendarService)
    {
        _calendarService = calendarService;
    }

    public async Task<bool> IsValidAsync(MusicianRequest entity, ValidationContext context)
    {
        if (context.Mode == ValidationMode.Create || context.Mode == ValidationMode.Update)
        {
            var hasConflict = await _calendarService.HasConflictsAsync(
                entity.OrganizerId, entity.Date, entity.DurationHours);
            return !hasConflict;
        }
        return true;
    }

    public async Task<string> GetErrorMessageAsync(MusicianRequest entity, ValidationContext context)
    {
        return $"El organizador tiene un conflicto de calendario en {entity.Date:yyyy-MM-dd}";
    }

    public async Task<string> GetSuggestionAsync(MusicianRequest entity, ValidationContext context)
    {
        return "Verifica tu calendario y elige una fecha y hora disponible";
    }
}

// Regla: Presupuesto por tipo de evento
public class BudgetByEventTypeRule : IBusinessRule<MusicianRequest>
{
    public string RuleName => "BudgetByEventType";
    public string Description => "El presupuesto debe ser apropiado para el tipo de evento";
    public ValidationSeverity Severity => ValidationSeverity.Warning;

    private readonly Dictionary<string, (decimal Min, decimal Max)> _eventTypeBudgets = new()
    {
        { "Boda", (200, 2000) },
        { "Fiesta", (100, 800) },
        { "Evento Corporativo", (300, 1500) },
        { "Concierto", (500, 3000) },
        { "Ceremonia", (150, 1000) }
    };

    public async Task<bool> IsValidAsync(MusicianRequest entity, ValidationContext context)
    {
        if (_eventTypeBudgets.TryGetValue(entity.EventType, out var budgetRange))
        {
            return entity.Budget >= budgetRange.Min && entity.Budget <= budgetRange.Max;
        }
        return true; // Tipo de evento no reconocido
    }

    public async Task<string> GetErrorMessageAsync(MusicianRequest entity, ValidationContext context)
    {
        if (_eventTypeBudgets.TryGetValue(entity.EventType, out var budgetRange))
        {
            return $"Para eventos tipo '{entity.EventType}', el presupuesto recomendado es entre ${budgetRange.Min} y ${budgetRange.Max}";
        }
        return $"Presupuesto de ${entity.Budget} para evento tipo '{entity.EventType}'";
    }

    public async Task<string> GetSuggestionAsync(MusicianRequest entity, ValidationContext context)
    {
        if (_eventTypeBudgets.TryGetValue(entity.EventType, out var budgetRange))
        {
            if (entity.Budget < budgetRange.Min)
            {
                return $"Considera aumentar el presupuesto a al menos ${budgetRange.Min} para este tipo de evento";
            }
            else if (entity.Budget > budgetRange.Max)
            {
                return $"El presupuesto es generoso. Considera reducirlo a ${budgetRange.Max} para optimizar costos";
            }
        }
        return "El presupuesto está dentro del rango recomendado";
    }
}

// Regla: Duración del evento
public class EventDurationRule : IBusinessRule<MusicianRequest>
{
    public string RuleName => "EventDuration";
    public string Description => "La duración del evento debe ser apropiada para el tipo";
    public ValidationSeverity Severity => ValidationSeverity.Warning;

    private readonly Dictionary<string, (int Min, int Max)> _eventTypeDurations = new()
    {
        { "Boda", (2, 8) },
        { "Fiesta", (2, 6) },
        { "Evento Corporativo", (1, 4) },
        { "Concierto", (1, 3) },
        { "Ceremonia", (1, 2) }
    };

    public async Task<bool> IsValidAsync(MusicianRequest entity, ValidationContext context)
    {
        if (_eventTypeDurations.TryGetValue(entity.EventType, out var durationRange))
        {
            return entity.DurationHours >= durationRange.Min && entity.DurationHours <= durationRange.Max;
        }
        return true;
    }

    public async Task<string> GetErrorMessageAsync(MusicianRequest entity, ValidationContext context)
    {
        if (_eventTypeDurations.TryGetValue(entity.EventType, out var durationRange))
        {
            return $"Para eventos tipo '{entity.EventType}', la duración recomendada es entre {durationRange.Min} y {durationRange.Max} horas";
        }
        return $"Duración de {entity.DurationHours} horas para evento tipo '{entity.EventType}'";
    }

    public async Task<string> GetSuggestionAsync(MusicianRequest entity, ValidationContext context)
    {
        if (_eventTypeDurations.TryGetValue(entity.EventType, out var durationRange))
        {
            if (entity.DurationHours < durationRange.Min)
            {
                return $"Considera aumentar la duración a al menos {durationRange.Min} horas para este tipo de evento";
            }
            else if (entity.DurationHours > durationRange.Max)
            {
                return $"La duración es extensa. Considera reducirla a {durationRange.Max} horas para optimizar costos";
            }
        }
        return "La duración está dentro del rango recomendado";
    }
}
```

---

## 🔄 **Validaciones en Tiempo Real**

### **Sistema de Validación Dinámica**

```csharp
public interface IRealTimeValidator<TEntity>
{
    Task<ValidationResult> ValidateInRealTimeAsync(TEntity entity, ValidationContext context);
    event EventHandler<ValidationResult> ValidationCompleted;
    void StartValidation(TEntity entity, ValidationContext context);
    void StopValidation();
}

public class RealTimeMusicianRequestValidator : IRealTimeValidator<MusicianRequest>
{
    private readonly IBusinessRuleValidator<MusicianRequest> _businessValidator;
    private readonly ILogger<RealTimeMusicianRequestValidator> _logger;
    private Timer _validationTimer;
    private MusicianRequest _currentEntity;
    private ValidationContext _currentContext;

    public event EventHandler<ValidationResult> ValidationCompleted;

    public RealTimeMusicianRequestValidator(
        IBusinessRuleValidator<MusicianRequest> businessValidator,
        ILogger<RealTimeMusicianRequestValidator> logger)
    {
        _businessValidator = businessValidator;
        _logger = logger;
    }

    public async Task<ValidationResult> ValidateInRealTimeAsync(MusicianRequest entity, ValidationContext context)
    {
        try
        {
            _logger.LogDebug("Performing real-time validation for request {RequestId}", entity.Id);
            
            var result = await _businessValidator.ValidateAsync(entity, context);
            
            ValidationCompleted?.Invoke(this, result);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in real-time validation for request {RequestId}", entity.Id);
            throw;
        }
    }

    public void StartValidation(MusicianRequest entity, ValidationContext context)
    {
        _currentEntity = entity;
        _currentContext = context;
        
        // Validar cada 5 segundos
        _validationTimer = new Timer(async _ => await PerformPeriodicValidation(), null, 0, 5000);
        
        _logger.LogInformation("Started real-time validation for request {RequestId}", entity.Id);
    }

    public void StopValidation()
    {
        _validationTimer?.Dispose();
        _validationTimer = null;
        
        _logger.LogInformation("Stopped real-time validation for request {RequestId}", 
            _currentEntity?.Id);
    }

    private async Task PerformPeriodicValidation()
    {
        if (_currentEntity != null && _currentContext != null)
        {
            try
            {
                var result = await ValidateInRealTimeAsync(_currentEntity, _currentContext);
                
                if (!result.IsValid)
                {
                    _logger.LogWarning("Real-time validation failed for request {RequestId}: {Errors}", 
                        _currentEntity.Id, string.Join(", ", result.Errors.Select(e => e.Message)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in periodic validation for request {RequestId}", _currentEntity.Id);
            }
        }
    }
}
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Reglas de Negocio**
```csharp
// Implementa reglas de negocio para:
// - Validación de disponibilidad de recursos
// - Verificación de conflictos de horario
// - Validación de presupuestos dinámicos
// - Reglas de negocio específicas del dominio
```

### **Ejercicio 2: Validación en Tiempo Real**
```csharp
// Crea un sistema que:
// - Valide formularios en tiempo real
// - Proporcione feedback inmediato
// - Maneje validaciones asíncronas
// - Actualice resultados dinámicamente
```

### **Ejercicio 3: Validadores Personalizados**
```csharp
// Implementa validadores para:
// - Entidades complejas
// - Reglas de negocio específicas
// - Validaciones cruzadas
// - Validaciones condicionales
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **🏗️ Validaciones de Dominio**: Sistema robusto de reglas de negocio
2. **📋 Reglas Específicas**: Implementación de validaciones especializadas
3. **🔄 Validación en Tiempo Real**: Sistema dinámico de validación
4. **✅ Reglas Configurables**: Sistema flexible de validación
5. **🔧 Patrones Avanzados**: Implementación de patrones de validación

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Sistema de Notificaciones**, implementando múltiples canales y notificaciones inteligentes.

---

**¡Has completado la sexta clase del Módulo 14! ✅🏗️**

