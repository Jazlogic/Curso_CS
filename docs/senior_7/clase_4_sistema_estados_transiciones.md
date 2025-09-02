# 🔄 Clase 4: Sistema de Estados y Transiciones

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 3: Lógica de Negocio Avanzada](../senior_7/clase_3_logica_negocio_avanzada.md)
- **🏠 Inicio del Módulo**: [Módulo 14: Plataformas Empresariales Reales](../senior_7/README.md)
- **➡️ Siguiente**: [Clase 5: Autenticación y Autorización Avanzada](../senior_7/clase_5_autenticacion_autorizacion_avanzada.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** flujos de trabajo complejos con estados anidados
2. **Crear** validaciones avanzadas para transiciones de estado
3. **Diseñar** sistemas de auditoría para cambios de estado
4. **Desarrollar** máquinas de estado para procesos complejos
5. **Aplicar** patrones de diseño para gestión de estados

---

## 🔄 **Flujos de Trabajo Complejos**

### **Máquina de Estados para Procesos Musicales**

Los flujos de trabajo complejos requieren máquinas de estado que manejen múltiples niveles de validación:

```csharp
// Interface para máquina de estados
public interface IStateMachine<TState, TTrigger>
{
    TState CurrentState { get; }
    bool CanFire(TTrigger trigger);
    Task<bool> FireAsync(TTrigger trigger, object context = null);
    IEnumerable<TTrigger> GetAvailableTriggers();
    IEnumerable<TState> GetReachableStates();
    void AddTransition(TState fromState, TTrigger trigger, TState toState, Func<object, bool> guard = null);
}

// Implementación de máquina de estados genérica
public class StateMachine<TState, TTrigger> : IStateMachine<TState, TTrigger>
{
    private readonly Dictionary<(TState, TTrigger), (TState, Func<object, bool>)> _transitions;
    private readonly ILogger<StateMachine<TState, TTrigger>> _logger;
    private readonly string _entityName;
    private readonly Guid _entityId;

    public TState CurrentState { get; private set; }
    public TState InitialState { get; }

    public StateMachine(TState initialState, string entityName, Guid entityId, ILogger<StateMachine<TState, TTrigger>> logger)
    {
        CurrentState = initialState;
        InitialState = initialState;
        _transitions = new Dictionary<(TState, TTrigger), (TState, Func<object, bool>)>();
        _logger = logger;
        _entityName = entityName;
        _entityId = entityId;
    }

    // Agregar transición con guard opcional
    public void AddTransition(TState fromState, TTrigger trigger, TState toState, Func<object, bool> guard = null)
    {
        var key = (fromState, trigger);
        _transitions[key] = (toState, guard);
        
        _logger.LogDebug("Added transition for {Entity} {EntityId}: {From} -> {Trigger} -> {To}", 
            _entityName, _entityId, fromState, trigger, toState);
    }

    // Verificar si se puede disparar un trigger
    public bool CanFire(TTrigger trigger)
    {
        var key = (CurrentState, trigger);
        if (!_transitions.ContainsKey(key))
            return false;

        var (_, guard) = _transitions[key];
        return guard?.Invoke(null) ?? true;
    }

    // Disparar un trigger para cambiar de estado
    public async Task<bool> FireAsync(TTrigger trigger, object context = null)
    {
        try
        {
            var key = (CurrentState, trigger);
            if (!_transitions.ContainsKey(key))
            {
                _logger.LogWarning("Invalid transition for {Entity} {EntityId}: {From} -> {Trigger}", 
                    _entityName, _entityId, CurrentState, trigger);
                return false;
            }

            var (toState, guard) = _transitions[key];
            
            // Verificar guard si existe
            if (guard != null && !guard(context))
            {
                _logger.LogWarning("Guard failed for transition {Entity} {EntityId}: {From} -> {Trigger}", 
                    _entityName, _entityId, CurrentState, trigger);
                return false;
            }

            var oldState = CurrentState;
            CurrentState = toState;

            _logger.LogInformation("State transition for {Entity} {EntityId}: {From} -> {Trigger} -> {To}", 
                _entityName, _entityId, oldState, trigger, toState);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error firing trigger {Trigger} for {Entity} {EntityId}", 
                trigger, _entityName, _entityId);
            return false;
        }
    }

    // Obtener triggers disponibles
    public IEnumerable<TTrigger> GetAvailableTriggers()
    {
        return _transitions
            .Where(kvp => kvp.Key.Item1.Equals(CurrentState))
            .Select(kvp => kvp.Key.Item2);
    }

    // Obtener estados alcanzables
    public IEnumerable<TState> GetReachableStates()
    {
        var reachable = new HashSet<TState> { CurrentState };
        var toProcess = new Queue<TState>();
        toProcess.Enqueue(CurrentState);

        while (toProcess.Count > 0)
        {
            var current = toProcess.Dequeue();
            var transitions = _transitions.Where(kvp => kvp.Key.Item1.Equals(current));

            foreach (var transition in transitions)
            {
                var nextState = transition.Value.Item1;
                if (reachable.Add(nextState))
                {
                    toProcess.Enqueue(nextState);
                }
            }
        }

        return reachable;
    }

    // Resetear a estado inicial
    public void Reset()
    {
        CurrentState = InitialState;
        _logger.LogInformation("Reset {Entity} {EntityId} to initial state: {State}", 
            _entityName, _entityId, InitialState);
    }
}

// Estados para solicitudes musicales
public enum MusicianRequestState
{
    Draft,              // Borrador
    Submitted,          // Enviada
    UnderReview,        // En revisión
    Approved,           // Aprobada
    Rejected,           // Rechazada
    Active,             // Activa
    InProgress,         // En progreso
    Completed,          // Completada
    Cancelled,          // Cancelada
    Expired,            // Expirada
    Disputed,           // En disputa
    Resolved            // Resuelta
}

// Triggers para cambios de estado
public enum MusicianRequestTrigger
{
    Submit,             // Enviar
    Review,             // Revisar
    Approve,            // Aprobar
    Reject,             // Rechazar
    Activate,           // Activar
    Start,              // Iniciar
    Complete,           // Completar
    Cancel,             // Cancelar
    Expire,             // Expirar
    Dispute,            // Disputar
    Resolve,            // Resolver
    Reset               // Resetear
}

// Máquina de estados para solicitudes musicales
public class MusicianRequestStateMachine : StateMachine<MusicianRequestState, MusicianRequestTrigger>
{
    public MusicianRequestStateMachine(Guid requestId, ILogger<MusicianRequestStateMachine> logger) 
        : base(MusicianRequestState.Draft, "MusicianRequest", requestId, logger)
    {
        InitializeTransitions();
    }

    private void InitializeTransitions()
    {
        // Draft -> Submitted
        AddTransition(
            MusicianRequestState.Draft, 
            MusicianRequestTrigger.Submit, 
            MusicianRequestState.Submitted,
            context => ValidateSubmission(context));

        // Submitted -> UnderReview
        AddTransition(
            MusicianRequestState.Submitted, 
            MusicianRequestTrigger.Review, 
            MusicianRequestState.UnderReview);

        // UnderReview -> Approved
        AddTransition(
            MusicianRequestState.UnderReview, 
            MusicianRequestTrigger.Approve, 
            MusicianRequestState.Approved,
            context => ValidateApproval(context));

        // UnderReview -> Rejected
        AddTransition(
            MusicianRequestState.UnderReview, 
            MusicianRequestTrigger.Reject, 
            MusicianRequestState.Rejected,
            context => ValidateRejection(context));

        // Approved -> Active
        AddTransition(
            MusicianRequestState.Approved, 
            MusicianRequestTrigger.Activate, 
            MusicianRequestState.Active,
            context => ValidateActivation(context));

        // Active -> InProgress
        AddTransition(
            MusicianRequestState.Active, 
            MusicianRequestTrigger.Start, 
            MusicianRequestState.InProgress,
            context => ValidateStart(context));

        // InProgress -> Completed
        AddTransition(
            MusicianRequestState.InProgress, 
            MusicianRequestTrigger.Complete, 
            MusicianRequestState.Completed,
            context => ValidateCompletion(context));

        // Active -> Cancelled
        AddTransition(
            MusicianRequestState.Active, 
            MusicianRequestTrigger.Cancel, 
            MusicianRequestState.Cancelled,
            context => ValidateCancellation(context));

        // Active -> Expired
        AddTransition(
            MusicianRequestState.Active, 
            MusicianRequestTrigger.Expire, 
            MusicianRequestState.Expired);

        // Active -> Disputed
        AddTransition(
            MusicianRequestState.Active, 
            MusicianRequestTrigger.Dispute, 
            MusicianRequestState.Disputed,
            context => ValidateDispute(context));

        // Disputed -> Resolved
        AddTransition(
            MusicianRequestState.Disputed, 
            MusicianRequestTrigger.Resolve, 
            MusicianRequestState.Resolved,
            context => ValidateResolution(context));

        // Cualquier estado -> Draft (Reset)
        foreach (MusicianRequestState state in Enum.GetValues(typeof(MusicianRequestState)))
        {
            if (state != MusicianRequestState.Draft)
            {
                AddTransition(state, MusicianRequestTrigger.Reset, MusicianRequestState.Draft);
            }
        }
    }

    // Validaciones específicas para cada transición
    private bool ValidateSubmission(object context)
    {
        if (context is MusicianRequest request)
        {
            return !string.IsNullOrEmpty(request.EventType) &&
                   request.Date > DateTime.Today &&
                   request.Budget >= 50 &&
                   !string.IsNullOrEmpty(request.Description);
        }
        return false;
    }

    private bool ValidateApproval(object context)
    {
        if (context is MusicianRequest request)
        {
            return request.Budget >= 50 && request.Budget <= 10000;
        }
        return false;
    }

    private bool ValidateRejection(object context)
    {
        if (context is MusicianRequest request)
        {
            return !string.IsNullOrEmpty(request.RejectionReason);
        }
        return false;
    }

    private bool ValidateActivation(object context)
    {
        if (context is MusicianRequest request)
        {
            return request.Date > DateTime.Today.AddDays(1);
        }
        return false;
    }

    private bool ValidateStart(object context)
    {
        if (context is MusicianRequest request)
        {
            return request.Date <= DateTime.Today &&
                   request.AssignedMusicianId.HasValue;
        }
        return false;
    }

    private bool ValidateCompletion(object context)
    {
        if (context is MusicianRequest request)
        {
            return request.StartedAt.HasValue &&
                   request.Date.AddHours(request.DurationHours) < DateTime.Now;
        }
        return false;
    }

    private bool ValidateCancellation(object context)
    {
        if (context is MusicianRequest request)
        {
            return request.Date > DateTime.Today.AddDays(1);
        }
        return false;
    }

    private bool ValidateDispute(object context)
    {
        if (context is MusicianRequest request)
        {
            return !string.IsNullOrEmpty(request.DisputeReason);
        }
        return false;
    }

    private bool ValidateResolution(object context)
    {
        if (context is MusicianRequest request)
        {
            return !string.IsNullOrEmpty(request.ResolutionDetails);
        }
        return false;
    }
}
```

---

## ✅ **Validaciones Avanzadas para Transiciones**

### **Sistema de Validación Multi-Nivel**

Las validaciones avanzadas aseguran que las transiciones cumplan con todas las reglas de negocio:

```csharp
// Interface para validadores de transición
public interface ITransitionValidator<TEntity, TState, TTrigger>
{
    Task<ValidationResult> ValidateTransitionAsync(TEntity entity, TState fromState, TState toState, TTrigger trigger, object context);
    IEnumerable<ValidationRule<TEntity, TState, TTrigger>> GetValidationRules();
    void AddValidationRule(ValidationRule<TEntity, TState, TTrigger> rule);
}

// Resultado de validación
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; } = new List<ValidationError>();
    public List<ValidationWarning> Warnings { get; set; } = new List<ValidationWarning>();
    public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

    public ValidationResult()
    {
        IsValid = true;
    }

    public void AddError(string field, string message, string code = null)
    {
        Errors.Add(new ValidationError { Field = field, Message = message, Code = code });
        IsValid = false;
    }

    public void AddWarning(string field, string message, string code = null)
    {
        Warnings.Add(new ValidationWarning { Field = field, Message = message, Code = code });
    }

    public void AddMetadata(string key, object value)
    {
        Metadata[key] = value;
    }
}

// Error de validación
public class ValidationError
{
    public string Field { get; set; }
    public string Message { get; set; }
    public string Code { get; set; }
    public object InvalidValue { get; set; }
    public string Suggestion { get; set; }
}

// Advertencia de validación
public class ValidationWarning
{
    public string Field { get; set; }
    public string Message { get; set; }
    public string Code { get; set; }
    public string Recommendation { get; set; }
}

// Regla de validación
public class ValidationRule<TEntity, TState, TTrigger>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Func<TEntity, TState, TState, TTrigger, object, Task<bool>> Condition { get; set; }
    public Func<TEntity, TState, TState, TTrigger, object, Task<string>> ErrorMessage { get; set; }
    public Func<TEntity, TState, TState, TTrigger, object, Task<string>> WarningMessage { get; set; }
    public ValidationSeverity Severity { get; set; }
    public int Priority { get; set; }
    public string[] AffectedFields { get; set; }
}

// Severidad de validación
public enum ValidationSeverity
{
    Info,       // Solo información
    Warning,    // Advertencia
    Error,      // Error que impide la transición
    Critical    // Error crítico del sistema
}

// Validador de transiciones para solicitudes musicales
public class MusicianRequestTransitionValidator : ITransitionValidator<MusicianRequest, MusicianRequestState, MusicianRequestTrigger>
{
    private readonly List<ValidationRule<MusicianRequest, MusicianRequestState, MusicianRequestTrigger>> _rules;
    private readonly IMusicianRepository _musicianRepository;
    private readonly ILocationService _locationService;
    private readonly ICalendarService _calendarService;
    private readonly ILogger<MusicianRequestTransitionValidator> _logger;

    public MusicianRequestTransitionValidator(
        IMusicianRepository musicianRepository,
        ILocationService locationService,
        ICalendarService calendarService,
        ILogger<MusicianRequestTransitionValidator> logger)
    {
        _rules = new List<ValidationRule<MusicianRequest, MusicianRequestState, MusicianRequestTrigger>>();
        _musicianRepository = musicianRepository;
        _locationService = locationService;
        _calendarService = calendarService;
        _logger = logger;
        InitializeValidationRules();
    }

    // Inicializar reglas de validación
    private void InitializeValidationRules()
    {
        // Regla: Validar presupuesto mínimo
        AddValidationRule(new ValidationRule<MusicianRequest, MusicianRequestState, MusicianRequestTrigger>
        {
            Name = "MinimumBudget",
            Description = "El presupuesto debe ser al menos $50",
            Condition = async (request, fromState, toState, trigger, context) => request.Budget >= 50,
            ErrorMessage = async (request, fromState, toState, trigger, context) => 
                $"El presupuesto mínimo es $50, pero se proporcionó ${request.Budget}",
            Severity = ValidationSeverity.Error,
            Priority = 1,
            AffectedFields = new[] { "Budget" }
        });

        // Regla: Validar presupuesto máximo
        AddValidationRule(new ValidationRule<MusicianRequest, MusicianRequestState, MusicianRequestTrigger>
        {
            Name = "MaximumBudget",
            Description = "El presupuesto no puede exceder $10,000",
            Condition = async (request, fromState, toState, trigger, context) => request.Budget <= 10000,
            ErrorMessage = async (request, fromState, toState, trigger, context) => 
                $"El presupuesto máximo es $10,000, pero se proporcionó ${request.Budget}",
            Severity = ValidationSeverity.Error,
            Priority = 1,
            AffectedFields = new[] { "Budget" }
        });

        // Regla: Validar fecha futura
        AddValidationRule(new ValidationRule<MusicianRequest, MusicianRequestState, MusicianRequestTrigger>
        {
            Name = "FutureDate",
            Description = "La fecha del evento debe ser futura",
            Condition = async (request, fromState, toState, trigger, context) => request.Date > DateTime.Today,
            ErrorMessage = async (request, fromState, toState, trigger, context) => 
                $"La fecha del evento ({request.Date:yyyy-MM-dd}) debe ser futura",
            Severity = ValidationSeverity.Error,
            Priority = 1,
            AffectedFields = new[] { "Date" }
        });

        // Regla: Validar disponibilidad de músicos
        AddValidationRule(new ValidationRule<MusicianRequest, MusicianRequestState, MusicianRequestTrigger>
        {
            Name = "MusicianAvailability",
            Description = "Debe haber músicos disponibles para el instrumento y fecha",
            Condition = async (request, fromState, toState, trigger, context) => 
            {
                if (trigger == MusicianRequestTrigger.Activate)
                {
                    var availableMusicians = await _musicianRepository.GetAvailableMusiciansAsync(
                        request.Instrument, request.Date, request.Location, request.DurationHours);
                    return availableMusicians.Any();
                }
                return true;
            },
            ErrorMessage = async (request, fromState, toState, trigger, context) => 
                $"No hay músicos disponibles para {request.Instrument} en {request.Date:yyyy-MM-dd}",
            Severity = ValidationSeverity.Error,
            Priority = 2,
            AffectedFields = new[] { "Instrument", "Date", "Location" }
        });

        // Regla: Validar conflicto de calendario
        AddValidationRule(new ValidationRule<MusicianRequest, MusicianRequestState, MusicianRequestTrigger>
        {
            Name = "CalendarConflict",
            Description = "No debe haber conflictos de calendario para el organizador",
            Condition = async (request, fromState, toState, trigger, context) => 
            {
                if (trigger == MusicianRequestTrigger.Submit)
                {
                    var hasConflict = await _calendarService.HasConflictsAsync(
                        request.OrganizerId, request.Date, request.DurationHours);
                    return !hasConflict;
                }
                return true;
            },
            ErrorMessage = async (request, fromState, toState, trigger, context) => 
                $"El organizador tiene un conflicto de calendario en {request.Date:yyyy-MM-dd}",
            Severity = ValidationSeverity.Error,
            Priority = 2,
            AffectedFields = new[] { "Date", "DurationHours" }
        });

        // Regla: Validar distancia máxima
        AddValidationRule(new ValidationRule<MusicianRequest, MusicianRequestState, MusicianRequestTrigger>
        {
            Name = "MaximumDistance",
            Description = "La distancia máxima permitida es 200 km",
            Condition = async (request, fromState, toState, trigger, context) => 
            {
                if (trigger == MusicianRequestTrigger.Submit)
                {
                    var distance = await _locationService.CalculateDistance(
                        request.OrganizerLocation, request.Location);
                    return distance <= 200;
                }
                return true;
            },
            ErrorMessage = async (request, fromState, toState, trigger, context) => 
                $"La distancia máxima permitida es 200 km",
            Severity = ValidationSeverity.Warning,
            Priority = 3,
            AffectedFields = new[] { "Location" }
        });

        // Regla: Validar duración del evento
        AddValidationRule(new ValidationRule<MusicianRequest, MusicianRequestState, MusicianRequestTrigger>
        {
            Name = "EventDuration",
            Description = "La duración del evento debe estar entre 1 y 12 horas",
            Condition = async (request, fromState, toState, trigger, context) => 
                request.DurationHours >= 1 && request.DurationHours <= 12,
            ErrorMessage = async (request, fromState, toState, trigger, context) => 
                $"La duración del evento debe estar entre 1 y 12 horas, pero se especificó {request.DurationHours} horas",
            Severity = ValidationSeverity.Error,
            Priority = 1,
            AffectedFields = new[] { "DurationHours" }
        });

        // Regla: Validar campos requeridos para activación
        AddValidationRule(new ValidationRule<MusicianRequest, MusicianRequestState, MusicianRequestTrigger>
        {
            Name = "RequiredFieldsForActivation",
            Description = "Todos los campos requeridos deben estar completos para activar",
            Condition = async (request, fromState, toState, trigger, context) => 
            {
                if (trigger == MusicianRequestTrigger.Activate)
                {
                    return !string.IsNullOrEmpty(request.EventType) &&
                           !string.IsNullOrEmpty(request.Description) &&
                           !string.IsNullOrEmpty(request.Location) &&
                           !string.IsNullOrEmpty(request.Instrument) &&
                           request.Budget > 0 &&
                           request.DurationHours > 0;
                }
                return true;
            },
            ErrorMessage = async (request, fromState, toState, trigger, context) => 
                "Todos los campos requeridos deben estar completos para activar la solicitud",
            Severity = ValidationSeverity.Error,
            Priority = 1,
            AffectedFields = new[] { "EventType", "Description", "Location", "Instrument", "Budget", "DurationHours" }
        });

        // Regla: Validar aprobación para transiciones críticas
        AddValidationRule(new ValidationRule<MusicianRequest, MusicianRequestState, MusicianRequestTrigger>
        {
            Name = "ApprovalRequired",
            Description = "Se requiere aprobación para transiciones críticas",
            Condition = async (request, fromState, toState, trigger, context) => 
            {
                if (trigger == MusicianRequestTrigger.Approve && 
                    (toState == MusicianRequestState.Active || toState == MusicianRequestState.InProgress))
                {
                    return request.ApprovedBy.HasValue && request.ApprovedAt.HasValue;
                }
                return true;
            },
            ErrorMessage = async (request, fromState, toState, trigger, context) => 
                "Se requiere aprobación para activar o iniciar la solicitud",
            Severity = ValidationSeverity.Error,
            Priority = 1,
            AffectedFields = new[] { "ApprovedBy", "ApprovedAt" }
        });
    }

    // Agregar regla de validación
    public void AddValidationRule(ValidationRule<MusicianRequest, MusicianRequestState, MusicianRequestTrigger> rule)
    {
        _rules.Add(rule);
        _rules.Sort((a, b) => a.Priority.CompareTo(b.Priority));
    }

    // Obtener reglas de validación
    public IEnumerable<ValidationRule<MusicianRequest, MusicianRequestState, MusicianRequestTrigger>> GetValidationRules()
    {
        return _rules.AsReadOnly();
    }

    // Validar transición
    public async Task<ValidationResult> ValidateTransitionAsync(
        MusicianRequest request, 
        MusicianRequestState fromState, 
        MusicianRequestState toState, 
        MusicianRequestTrigger trigger, 
        object context)
    {
        var result = new ValidationResult();
        
        try
        {
            _logger.LogInformation("Validating transition for request {RequestId}: {From} -> {Trigger} -> {To}", 
                request.Id, fromState, trigger, toState);

            // Ejecutar todas las reglas de validación
            foreach (var rule in _rules)
            {
                try
                {
                    var isValid = await rule.Condition(request, fromState, toState, trigger, context);
                    
                    if (!isValid)
                    {
                        var errorMessage = await rule.ErrorMessage(request, fromState, toState, trigger, context);
                        
                        if (rule.Severity == ValidationSeverity.Error)
                        {
                            result.AddError(rule.AffectedFields?.FirstOrDefault() ?? "General", errorMessage, rule.Name);
                        }
                        else if (rule.Severity == ValidationSeverity.Warning)
                        {
                            result.AddWarning(rule.AffectedFields?.FirstOrDefault() ?? "General", errorMessage, rule.Name);
                        }
                    }
                    else if (rule.Severity == ValidationSeverity.Warning)
                    {
                        var warningMessage = await rule.WarningMessage(request, fromState, toState, trigger, context);
                        if (!string.IsNullOrEmpty(warningMessage))
                        {
                            result.AddWarning(rule.AffectedFields?.FirstOrDefault() ?? "General", warningMessage, rule.Name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing validation rule {RuleName} for request {RequestId}", 
                        rule.Name, request.Id);
                    
                    if (rule.Severity == ValidationSeverity.Critical)
                    {
                        result.AddError("System", $"Error en validación del sistema: {rule.Name}", rule.Name);
                    }
                }
            }

            // Agregar metadatos de validación
            result.AddMetadata("ValidationTimestamp", DateTime.UtcNow);
            result.AddMetadata("ValidationRulesExecuted", _rules.Count);
            result.AddMetadata("FromState", fromState);
            result.AddMetadata("ToState", toState);
            result.AddMetadata("Trigger", trigger);

            _logger.LogInformation("Validation completed for request {RequestId}. IsValid: {IsValid}, Errors: {ErrorCount}, Warnings: {WarningCount}", 
                request.Id, result.IsValid, result.Errors.Count, result.Warnings.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during validation for request {RequestId}", request.Id);
            result.AddError("System", "Error interno durante la validación", "ValidationError");
            return result;
        }
    }
}
```

---

## 📊 **Sistema de Auditoría para Cambios de Estado**

### **Trazabilidad Completa de Transiciones**

El sistema de auditoría registra todos los cambios de estado para cumplimiento y análisis:

```csharp
// Interface para servicio de auditoría
public interface IAuditService
{
    Task LogStatusTransitionAsync(Guid entityId, object oldStatus, object newStatus, Guid userId, string userRole, object context);
    Task<List<AuditLog>> GetAuditLogsAsync(Guid entityId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<AuditLog>> GetAuditLogsByUserAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<AuditLog>> GetAuditLogsByActionAsync(string action, DateTime? fromDate = null, DateTime? toDate = null);
    Task<AuditSummary> GetAuditSummaryAsync(Guid entityId);
}

// Entidad de log de auditoría
public class AuditLog
{
    public Guid Id { get; set; }
    public string EntityType { get; set; }                    // Tipo de entidad auditada
    public Guid EntityId { get; set; }                        // ID de la entidad
    public string Action { get; set; }                        // Acción realizada
    public string OldValue { get; set; }                      // Valor anterior
    public string NewValue { get; set; }                      // Nuevo valor
    public Guid UserId { get; set; }                          // ID del usuario que realizó la acción
    public string UserRole { get; set; }                      // Rol del usuario
    public string UserName { get; set; }                      // Nombre del usuario
    public DateTime Timestamp { get; set; }                   // Timestamp de la acción
    public string IpAddress { get; set; }                     // Dirección IP del usuario
    public string UserAgent { get; set; }                     // User agent del navegador
    public string SessionId { get; set; }                     // ID de sesión
    public string Context { get; set; }                       // Contexto adicional en JSON
    public Dictionary<string, object> Metadata { get; set; }  // Metadatos adicionales
    public bool IsSuccessful { get; set; }                    // Si la acción fue exitosa
    public string ErrorMessage { get; set; }                  // Mensaje de error si falló
    public TimeSpan Duration { get; set; }                    // Duración de la operación
}

// Resumen de auditoría
public class AuditSummary
{
    public Guid EntityId { get; set; }
    public string EntityType { get; set; }
    public int TotalActions { get; set; }
    public int SuccessfulActions { get; set; }
    public int FailedActions { get; set; }
    public DateTime FirstAction { get; set; }
    public DateTime LastAction { get; set; }
    public Dictionary<string, int> ActionsByUser { get; set; }
    public Dictionary<string, int> ActionsByRole { get; set; }
    public Dictionary<string, int> ActionsByType { get; set; }
    public List<string> CommonErrors { get; set; }
    public TimeSpan AverageDuration { get; set; }
}

// Implementación del servicio de auditoría
public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditService> _logger;
    private readonly IMemoryCache _cache;

    public AuditService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditService> logger,
        IMemoryCache cache)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _cache = cache;
    }

    // Registrar transición de estado
    public async Task LogStatusTransitionAsync(
        Guid entityId, 
        object oldStatus, 
        object newStatus, 
        Guid userId, 
        string userRole, 
        object context)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = "MusicianRequest",
                EntityId = entityId,
                Action = "StatusTransition",
                OldValue = oldStatus?.ToString(),
                NewValue = newStatus?.ToString(),
                UserId = userId,
                UserRole = userRole,
                Timestamp = DateTime.UtcNow,
                IpAddress = GetClientIpAddress(httpContext),
                UserAgent = GetUserAgent(httpContext),
                SessionId = GetSessionId(httpContext),
                Context = context != null ? System.Text.Json.JsonSerializer.Serialize(context) : null,
                Metadata = new Dictionary<string, object>
                {
                    ["OldStatus"] = oldStatus,
                    ["NewStatus"] = newStatus,
                    ["TransitionType"] = "StateChange"
                },
                IsSuccessful = true,
                Duration = TimeSpan.Zero // Se puede calcular si se pasa el tiempo de inicio
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            // Invalidar cache relacionado
            var cacheKey = $"audit_summary_{entityId}";
            _cache.Remove(cacheKey);

            _logger.LogDebug("Audit log created for entity {EntityId}: {OldStatus} -> {NewStatus}", 
                entityId, oldStatus, newStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging status transition for entity {EntityId}", entityId);
        }
    }

    // Obtener logs de auditoría por entidad
    public async Task<List<AuditLog>> GetAuditLogsAsync(Guid entityId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var query = _context.AuditLogs
                .Where(log => log.EntityId == entityId)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(log => log.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(log => log.Timestamp <= toDate.Value);

            return await query
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for entity {EntityId}", entityId);
            return new List<AuditLog>();
        }
    }

    // Obtener logs de auditoría por usuario
    public async Task<List<AuditLog>> GetAuditLogsByUserAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var query = _context.AuditLogs
                .Where(log => log.UserId == userId)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(log => log.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(log => log.Timestamp <= toDate.Value);

            return await query
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for user {UserId}", userId);
            return new List<AuditLog>();
        }
    }

    // Obtener logs de auditoría por acción
    public async Task<List<AuditLog>> GetAuditLogsByActionAsync(string action, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var query = _context.AuditLogs
                .Where(log => log.Action == action)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(log => log.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(log => log.Timestamp <= toDate.Value);

            return await query
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for action {Action}", action);
            return new List<AuditLog>();
        }
    }

    // Obtener resumen de auditoría
    public async Task<AuditSummary> GetAuditSummaryAsync(Guid entityId)
    {
        try
        {
            var cacheKey = $"audit_summary_{entityId}";
            
            if (_cache.TryGetValue(cacheKey, out AuditSummary cachedSummary))
            {
                return cachedSummary;
            }

            var logs = await GetAuditLogsAsync(entityId);
            
            if (!logs.Any())
            {
                return new AuditSummary { EntityId = entityId, EntityType = "MusicianRequest" };
            }

            var summary = new AuditSummary
            {
                EntityId = entityId,
                EntityType = logs.First().EntityType,
                TotalActions = logs.Count,
                SuccessfulActions = logs.Count(log => log.IsSuccessful),
                FailedActions = logs.Count(log => !log.IsSuccessful),
                FirstAction = logs.Min(log => log.Timestamp),
                LastAction = logs.Max(log => log.Timestamp),
                ActionsByUser = logs.GroupBy(log => log.UserId)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                ActionsByRole = logs.GroupBy(log => log.UserRole)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ActionsByType = logs.GroupBy(log => log.Action)
                    .ToDictionary(g => g.Key, g => g.Count()),
                CommonErrors = logs.Where(log => !log.IsSuccessful)
                    .GroupBy(log => log.ErrorMessage)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .Select(g => g.Key)
                    .ToList(),
                AverageDuration = TimeSpan.FromTicks((long)logs.Average(log => log.Duration.Ticks))
            };

            // Guardar en cache por 5 minutos
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5));
            
            _cache.Set(cacheKey, summary, cacheOptions);

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating audit summary for entity {EntityId}", entityId);
            return new AuditSummary { EntityId = entityId, EntityType = "MusicianRequest" };
        }
    }

    // Métodos auxiliares para obtener información del contexto HTTP
    private string GetClientIpAddress(HttpContext httpContext)
    {
        if (httpContext?.Connection?.RemoteIpAddress != null)
        {
            return httpContext.Connection.RemoteIpAddress.ToString();
        }
        return "Unknown";
    }

    private string GetUserAgent(HttpContext httpContext)
    {
        return httpContext?.Request?.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";
    }

    private string GetSessionId(HttpContext httpContext)
    {
        return httpContext?.Session?.Id ?? "Unknown";
    }
}
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Máquina de Estados**
```csharp
// Implementa una máquina de estados para un proceso de contratación:
// - Aplicación recibida
// - En revisión
// - Entrevista programada
// - Aprobada/Rechazada
// - Contrato firmado
// - Empleado activo
```

### **Ejercicio 2: Validaciones Avanzadas**
```csharp
// Crea validaciones para un sistema de reservas:
// - Disponibilidad de recursos
// - Conflictos de horario
// - Validación de permisos
// - Verificación de capacidad
```

### **Ejercicio 3: Sistema de Auditoría**
```csharp
// Implementa un sistema de auditoría que:
// - Registre todas las acciones del usuario
// - Genere reportes de actividad
// - Detecte patrones sospechosos
// - Mantenga historial de cambios
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **🔄 Flujos de Trabajo**: Máquinas de estado para procesos complejos
2. **✅ Validaciones Avanzadas**: Sistema multi-nivel de reglas de validación
3. **📊 Auditoría**: Trazabilidad completa de cambios de estado
4. **🎭 Estados Anidados**: Gestión de estados complejos con transiciones
5. **🔧 Patrones de Diseño**: Implementación de patrones para gestión de estados

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Autenticación y Autorización Avanzada**, implementando JWT con claims personalizados y sistemas de permisos granulares.

---

**¡Has completado la cuarta clase del Módulo 14! 🔄✅**


