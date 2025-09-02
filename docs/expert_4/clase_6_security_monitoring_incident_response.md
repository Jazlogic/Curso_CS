# 📊 **Clase 6: Security Monitoring y Incident Response**

## 🎯 **Objetivo de la Clase**
Dominar las técnicas de monitoreo de seguridad, detección de amenazas y respuesta a incidentes de seguridad en aplicaciones .NET.

## 📚 **Contenido de la Clase**

### **1. Security Monitoring**

#### **1.1 Real-time Security Monitoring**
```csharp
// Servicio de monitoreo de seguridad en tiempo real
public class SecurityMonitoringService
{
    private readonly ILogger<SecurityMonitoringService> _logger;
    private readonly ISecurityEventRepository _eventRepository;
    private readonly IAlertService _alertService;
    private readonly List<ISecurityRule> _monitoringRules;
    
    public SecurityMonitoringService(
        ILogger<SecurityMonitoringService> logger,
        ISecurityEventRepository eventRepository,
        IAlertService alertService)
    {
        _logger = logger;
        _eventRepository = eventRepository;
        _alertService = alertService;
        _monitoringRules = new List<ISecurityRule>
        {
            new BruteForceAttackRule(),
            new SuspiciousLoginRule(),
            new DataExfiltrationRule(),
            new PrivilegeEscalationRule(),
            new AnomalousBehaviorRule()
        };
    }
    
    // Procesar evento de seguridad
    public async Task ProcessSecurityEventAsync(SecurityEvent securityEvent)
    {
        try
        {
            // Almacenar evento
            await _eventRepository.StoreEventAsync(securityEvent);
            
            // Evaluar reglas de monitoreo
            foreach (var rule in _monitoringRules)
            {
                var result = await rule.EvaluateAsync(securityEvent);
                
                if (result.IsThreat)
                {
                    await HandleThreatAsync(result);
                }
            }
            
            _logger.LogInformation("Security event processed: {EventType} from {SourceIp}", 
                securityEvent.EventType, securityEvent.SourceIp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing security event");
            throw;
        }
    }
    
    // Manejar amenaza detectada
    private async Task HandleThreatAsync(ThreatDetectionResult threat)
    {
        try
        {
            // Crear alerta de seguridad
            var alert = new SecurityAlert
            {
                Id = Guid.NewGuid().ToString(),
                ThreatType = threat.ThreatType,
                Severity = threat.Severity,
                Description = threat.Description,
                SourceIp = threat.SourceIp,
                UserId = threat.UserId,
                Timestamp = DateTime.UtcNow,
                Status = AlertStatus.Active
            };
            
            // Enviar alerta
            await _alertService.SendAlertAsync(alert);
            
            // Ejecutar respuesta automática si está configurada
            if (threat.AutoResponse != null)
            {
                await threat.AutoResponse.ExecuteAsync(threat);
            }
            
            _logger.LogWarning("Security threat detected and handled: {ThreatType} from {SourceIp}", 
                threat.ThreatType, threat.SourceIp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling security threat");
            throw;
        }
    }
    
    // Obtener métricas de seguridad
    public async Task<SecurityMetrics> GetSecurityMetricsAsync(DateTime from, DateTime to)
    {
        try
        {
            var events = await _eventRepository.GetEventsAsync(from, to);
            
            var metrics = new SecurityMetrics
            {
                TotalEvents = events.Count,
                CriticalEvents = events.Count(e => e.Severity == SecuritySeverity.Critical),
                HighEvents = events.Count(e => e.Severity == SecuritySeverity.High),
                MediumEvents = events.Count(e => e.Severity == SecuritySeverity.Medium),
                LowEvents = events.Count(e => e.Severity == SecuritySeverity.Low),
                TopThreatTypes = events.GroupBy(e => e.EventType)
                    .OrderByDescending(g => g.Count())
                    .Take(10)
                    .ToDictionary(g => g.Key, g => g.Count()),
                TopSourceIps = events.GroupBy(e => e.SourceIp)
                    .OrderByDescending(g => g.Count())
                    .Take(10)
                    .ToDictionary(g => g.Key, g => g.Count()),
                EventsByHour = events.GroupBy(e => e.Timestamp.Hour)
                    .OrderBy(g => g.Key)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
            
            _logger.LogInformation("Security metrics retrieved: {TotalEvents} events", metrics.TotalEvents);
            
            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving security metrics");
            throw;
        }
    }
}

// Interfaz para reglas de monitoreo
public interface ISecurityRule
{
    Task<ThreatDetectionResult> EvaluateAsync(SecurityEvent securityEvent);
}

// Regla para detectar ataques de fuerza bruta
public class BruteForceAttackRule : ISecurityRule
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<BruteForceAttackRule> _logger;
    
    public BruteForceAttackRule(IMemoryCache cache, ILogger<BruteForceAttackRule> logger)
    {
        _cache = cache;
        _logger = logger;
    }
    
    public async Task<ThreatDetectionResult> EvaluateAsync(SecurityEvent securityEvent)
    {
        try
        {
            if (securityEvent.EventType != "LOGIN_FAILED")
            {
                return new ThreatDetectionResult { IsThreat = false };
            }
            
            var cacheKey = $"brute_force_{securityEvent.SourceIp}";
            var attempts = _cache.Get<int>(cacheKey);
            
            if (attempts >= 5) // 5 intentos fallidos
            {
                return new ThreatDetectionResult
                {
                    IsThreat = true,
                    ThreatType = "Brute Force Attack",
                    Severity = SecuritySeverity.High,
                    Description = $"Brute force attack detected from {securityEvent.SourceIp}",
                    SourceIp = securityEvent.SourceIp,
                    UserId = securityEvent.UserId,
                    AutoResponse = new BlockIpResponse(securityEvent.SourceIp)
                };
            }
            
            // Incrementar contador de intentos
            _cache.Set(cacheKey, attempts + 1, TimeSpan.FromMinutes(15));
            
            return new ThreatDetectionResult { IsThreat = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating brute force attack rule");
            return new ThreatDetectionResult { IsThreat = false };
        }
    }
}

// Regla para detectar logins sospechosos
public class SuspiciousLoginRule : ISecurityRule
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SuspiciousLoginRule> _logger;
    
    public SuspiciousLoginRule(IUserRepository userRepository, ILogger<SuspiciousLoginRule> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }
    
    public async Task<ThreatDetectionResult> EvaluateAsync(SecurityEvent securityEvent)
    {
        try
        {
            if (securityEvent.EventType != "LOGIN_SUCCESS")
            {
                return new ThreatDetectionResult { IsThreat = false };
            }
            
            var user = await _userRepository.GetByIdAsync(securityEvent.UserId);
            if (user == null)
            {
                return new ThreatDetectionResult { IsThreat = false };
            }
            
            // Verificar si el login es desde una ubicación inusual
            if (IsUnusualLocation(user, securityEvent.SourceIp))
            {
                return new ThreatDetectionResult
                {
                    IsThreat = true,
                    ThreatType = "Suspicious Login",
                    Severity = SecuritySeverity.Medium,
                    Description = $"Suspicious login detected for user {user.Username} from {securityEvent.SourceIp}",
                    SourceIp = securityEvent.SourceIp,
                    UserId = securityEvent.UserId,
                    AutoResponse = new RequireMfaResponse(securityEvent.UserId)
                };
            }
            
            return new ThreatDetectionResult { IsThreat = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating suspicious login rule");
            return new ThreatDetectionResult { IsThreat = false };
        }
    }
    
    private bool IsUnusualLocation(User user, string sourceIp)
    {
        // Implementar lógica para detectar ubicaciones inusuales
        // Esto podría incluir verificación de geolocalización, IPs conocidas, etc.
        return false; // Simplificado para el ejemplo
    }
}

// Modelos para monitoreo de seguridad
public class SecurityEvent
{
    public string Id { get; set; }
    public string EventType { get; set; }
    public SecuritySeverity Severity { get; set; }
    public string Description { get; set; }
    public string SourceIp { get; set; }
    public string UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class ThreatDetectionResult
{
    public bool IsThreat { get; set; }
    public string ThreatType { get; set; }
    public SecuritySeverity Severity { get; set; }
    public string Description { get; set; }
    public string SourceIp { get; set; }
    public string UserId { get; set; }
    public IAutoResponse AutoResponse { get; set; }
}

public class SecurityAlert
{
    public string Id { get; set; }
    public string ThreatType { get; set; }
    public SecuritySeverity Severity { get; set; }
    public string Description { get; set; }
    public string SourceIp { get; set; }
    public string UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public AlertStatus Status { get; set; }
}

public class SecurityMetrics
{
    public int TotalEvents { get; set; }
    public int CriticalEvents { get; set; }
    public int HighEvents { get; set; }
    public int MediumEvents { get; set; }
    public int LowEvents { get; set; }
    public Dictionary<string, int> TopThreatTypes { get; set; }
    public Dictionary<string, int> TopSourceIps { get; set; }
    public Dictionary<int, int> EventsByHour { get; set; }
}

public enum AlertStatus
{
    Active,
    Acknowledged,
    Resolved,
    FalsePositive
}
```

#### **1.2 Security Dashboard**
```csharp
// Servicio de dashboard de seguridad
public class SecurityDashboardService
{
    private readonly ISecurityEventRepository _eventRepository;
    private readonly ILogger<SecurityDashboardService> _logger;
    
    public SecurityDashboardService(
        ISecurityEventRepository eventRepository,
        ILogger<SecurityDashboardService> logger)
    {
        _eventRepository = eventRepository;
        _logger = logger;
    }
    
    // Obtener datos del dashboard
    public async Task<SecurityDashboardData> GetDashboardDataAsync()
    {
        try
        {
            var now = DateTime.UtcNow;
            var last24Hours = now.AddHours(-24);
            var last7Days = now.AddDays(-7);
            var last30Days = now.AddDays(-30);
            
            var dashboardData = new SecurityDashboardData
            {
                Last24Hours = await GetTimeRangeDataAsync(last24Hours, now),
                Last7Days = await GetTimeRangeDataAsync(last7Days, now),
                Last30Days = await GetTimeRangeDataAsync(last30Days, now),
                CurrentThreats = await GetCurrentThreatsAsync(),
                SecurityTrends = await GetSecurityTrendsAsync(),
                TopVulnerabilities = await GetTopVulnerabilitiesAsync()
            };
            
            _logger.LogInformation("Security dashboard data retrieved successfully");
            
            return dashboardData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving security dashboard data");
            throw;
        }
    }
    
    // Obtener datos para un rango de tiempo
    private async Task<TimeRangeData> GetTimeRangeDataAsync(DateTime from, DateTime to)
    {
        var events = await _eventRepository.GetEventsAsync(from, to);
        
        return new TimeRangeData
        {
            TotalEvents = events.Count,
            CriticalEvents = events.Count(e => e.Severity == SecuritySeverity.Critical),
            HighEvents = events.Count(e => e.Severity == SecuritySeverity.High),
            MediumEvents = events.Count(e => e.Severity == SecuritySeverity.Medium),
            LowEvents = events.Count(e => e.Severity == SecuritySeverity.Low),
            EventsByType = events.GroupBy(e => e.EventType)
                .ToDictionary(g => g.Key, g => g.Count()),
            EventsByHour = events.GroupBy(e => e.Timestamp.Hour)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }
    
    // Obtener amenazas actuales
    private async Task<List<CurrentThreat>> GetCurrentThreatsAsync()
    {
        var lastHour = DateTime.UtcNow.AddHours(-1);
        var events = await _eventRepository.GetEventsAsync(lastHour, DateTime.UtcNow);
        
        return events.Where(e => e.Severity >= SecuritySeverity.High)
            .Select(e => new CurrentThreat
            {
                Type = e.EventType,
                Severity = e.Severity,
                SourceIp = e.SourceIp,
                Timestamp = e.Timestamp,
                Description = e.Description
            })
            .OrderByDescending(t => t.Severity)
            .ThenByDescending(t => t.Timestamp)
            .Take(10)
            .ToList();
    }
    
    // Obtener tendencias de seguridad
    private async Task<List<SecurityTrend>> GetSecurityTrendsAsync()
    {
        var trends = new List<SecurityTrend>();
        var now = DateTime.UtcNow;
        
        for (int i = 6; i >= 0; i--)
        {
            var date = now.AddDays(-i);
            var dayStart = date.Date;
            var dayEnd = dayStart.AddDays(1);
            
            var events = await _eventRepository.GetEventsAsync(dayStart, dayEnd);
            
            trends.Add(new SecurityTrend
            {
                Date = dayStart,
                TotalEvents = events.Count,
                CriticalEvents = events.Count(e => e.Severity == SecuritySeverity.Critical),
                HighEvents = events.Count(e => e.Severity == SecuritySeverity.High)
            });
        }
        
        return trends;
    }
    
    // Obtener top vulnerabilidades
    private async Task<List<TopVulnerability>> GetTopVulnerabilitiesAsync()
    {
        var last7Days = DateTime.UtcNow.AddDays(-7);
        var events = await _eventRepository.GetEventsAsync(last7Days, DateTime.UtcNow);
        
        return events.GroupBy(e => e.EventType)
            .Select(g => new TopVulnerability
            {
                Type = g.Key,
                Count = g.Count(),
                Severity = g.Max(e => e.Severity),
                LastOccurrence = g.Max(e => e.Timestamp)
            })
            .OrderByDescending(v => v.Count)
            .Take(10)
            .ToList();
    }
}

// Modelos para dashboard de seguridad
public class SecurityDashboardData
{
    public TimeRangeData Last24Hours { get; set; }
    public TimeRangeData Last7Days { get; set; }
    public TimeRangeData Last30Days { get; set; }
    public List<CurrentThreat> CurrentThreats { get; set; }
    public List<SecurityTrend> SecurityTrends { get; set; }
    public List<TopVulnerability> TopVulnerabilities { get; set; }
}

public class TimeRangeData
{
    public int TotalEvents { get; set; }
    public int CriticalEvents { get; set; }
    public int HighEvents { get; set; }
    public int MediumEvents { get; set; }
    public int LowEvents { get; set; }
    public Dictionary<string, int> EventsByType { get; set; }
    public Dictionary<int, int> EventsByHour { get; set; }
}

public class CurrentThreat
{
    public string Type { get; set; }
    public SecuritySeverity Severity { get; set; }
    public string SourceIp { get; set; }
    public DateTime Timestamp { get; set; }
    public string Description { get; set; }
}

public class SecurityTrend
{
    public DateTime Date { get; set; }
    public int TotalEvents { get; set; }
    public int CriticalEvents { get; set; }
    public int HighEvents { get; set; }
}

public class TopVulnerability
{
    public string Type { get; set; }
    public int Count { get; set; }
    public SecuritySeverity Severity { get; set; }
    public DateTime LastOccurrence { get; set; }
}
```

### **2. Incident Response**

#### **2.1 Incident Response Service**
```csharp
// Servicio de respuesta a incidentes
public class IncidentResponseService
{
    private readonly ILogger<IncidentResponseService> _logger;
    private readonly IIncidentRepository _incidentRepository;
    private readonly INotificationService _notificationService;
    private readonly IAutoResponseService _autoResponseService;
    
    public IncidentResponseService(
        ILogger<IncidentResponseService> logger,
        IIncidentRepository incidentRepository,
        INotificationService notificationService,
        IAutoResponseService autoResponseService)
    {
        _logger = logger;
        _incidentRepository = incidentRepository;
        _notificationService = notificationService;
        _autoResponseService = autoResponseService;
    }
    
    // Crear incidente de seguridad
    public async Task<SecurityIncident> CreateIncidentAsync(SecurityAlert alert)
    {
        try
        {
            var incident = new SecurityIncident
            {
                Id = Guid.NewGuid().ToString(),
                AlertId = alert.Id,
                ThreatType = alert.ThreatType,
                Severity = alert.Severity,
                Description = alert.Description,
                SourceIp = alert.SourceIp,
                UserId = alert.UserId,
                Status = IncidentStatus.Open,
                CreatedAt = DateTime.UtcNow,
                AssignedTo = null,
                Steps = new List<IncidentStep>()
            };
            
            // Agregar paso inicial
            incident.Steps.Add(new IncidentStep
            {
                Id = Guid.NewGuid().ToString(),
                Step = "Incident created",
                Description = "Security incident created from alert",
                Timestamp = DateTime.UtcNow,
                PerformedBy = "System"
            });
            
            // Almacenar incidente
            await _incidentRepository.StoreIncidentAsync(incident);
            
            // Enviar notificaciones
            await _notificationService.SendIncidentNotificationAsync(incident);
            
            // Ejecutar respuesta automática
            await _autoResponseService.ExecuteAutoResponseAsync(incident);
            
            _logger.LogInformation("Security incident created: {IncidentId}", incident.Id);
            
            return incident;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating security incident");
            throw;
        }
    }
    
    // Actualizar estado del incidente
    public async Task UpdateIncidentStatusAsync(string incidentId, IncidentStatus status, string notes = null)
    {
        try
        {
            var incident = await _incidentRepository.GetIncidentAsync(incidentId);
            if (incident == null)
            {
                throw new IncidentNotFoundException($"Incident {incidentId} not found");
            }
            
            var previousStatus = incident.Status;
            incident.Status = status;
            incident.UpdatedAt = DateTime.UtcNow;
            
            // Agregar paso de actualización
            incident.Steps.Add(new IncidentStep
            {
                Id = Guid.NewGuid().ToString(),
                Step = $"Status changed from {previousStatus} to {status}",
                Description = notes ?? $"Incident status updated to {status}",
                Timestamp = DateTime.UtcNow,
                PerformedBy = "System"
            });
            
            // Actualizar incidente
            await _incidentRepository.UpdateIncidentAsync(incident);
            
            // Enviar notificaciones si es necesario
            if (status == IncidentStatus.Resolved)
            {
                await _notificationService.SendIncidentResolvedNotificationAsync(incident);
            }
            
            _logger.LogInformation("Incident status updated: {IncidentId} to {Status}", incidentId, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating incident status");
            throw;
        }
    }
    
    // Asignar incidente
    public async Task AssignIncidentAsync(string incidentId, string assignedTo)
    {
        try
        {
            var incident = await _incidentRepository.GetIncidentAsync(incidentId);
            if (incident == null)
            {
                throw new IncidentNotFoundException($"Incident {incidentId} not found");
            }
            
            incident.AssignedTo = assignedTo;
            incident.UpdatedAt = DateTime.UtcNow;
            
            // Agregar paso de asignación
            incident.Steps.Add(new IncidentStep
            {
                Id = Guid.NewGuid().ToString(),
                Step = "Incident assigned",
                Description = $"Incident assigned to {assignedTo}",
                Timestamp = DateTime.UtcNow,
                PerformedBy = "System"
            });
            
            // Actualizar incidente
            await _incidentRepository.UpdateIncidentAsync(incident);
            
            // Enviar notificación de asignación
            await _notificationService.SendIncidentAssignedNotificationAsync(incident, assignedTo);
            
            _logger.LogInformation("Incident assigned: {IncidentId} to {AssignedTo}", incidentId, assignedTo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning incident");
            throw;
        }
    }
    
    // Agregar paso al incidente
    public async Task AddIncidentStepAsync(string incidentId, string step, string description, string performedBy)
    {
        try
        {
            var incident = await _incidentRepository.GetIncidentAsync(incidentId);
            if (incident == null)
            {
                throw new IncidentNotFoundException($"Incident {incidentId} not found");
            }
            
            var incidentStep = new IncidentStep
            {
                Id = Guid.NewGuid().ToString(),
                Step = step,
                Description = description,
                Timestamp = DateTime.UtcNow,
                PerformedBy = performedBy
            };
            
            incident.Steps.Add(incidentStep);
            incident.UpdatedAt = DateTime.UtcNow;
            
            // Actualizar incidente
            await _incidentRepository.UpdateIncidentAsync(incident);
            
            _logger.LogInformation("Incident step added: {IncidentId} - {Step}", incidentId, step);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding incident step");
            throw;
        }
    }
    
    // Obtener incidentes
    public async Task<List<SecurityIncident>> GetIncidentsAsync(IncidentStatus? status = null, int limit = 50)
    {
        try
        {
            var incidents = await _incidentRepository.GetIncidentsAsync(status, limit);
            
            _logger.LogInformation("Incidents retrieved: {Count} incidents", incidents.Count);
            
            return incidents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving incidents");
            throw;
        }
    }
    
    // Obtener incidente por ID
    public async Task<SecurityIncident> GetIncidentAsync(string incidentId)
    {
        try
        {
            var incident = await _incidentRepository.GetIncidentAsync(incidentId);
            if (incident == null)
            {
                throw new IncidentNotFoundException($"Incident {incidentId} not found");
            }
            
            return incident;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving incident: {IncidentId}", incidentId);
            throw;
        }
    }
}

// Modelos para respuesta a incidentes
public class SecurityIncident
{
    public string Id { get; set; }
    public string AlertId { get; set; }
    public string ThreatType { get; set; }
    public SecuritySeverity Severity { get; set; }
    public string Description { get; set; }
    public string SourceIp { get; set; }
    public string UserId { get; set; }
    public IncidentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string AssignedTo { get; set; }
    public List<IncidentStep> Steps { get; set; } = new();
}

public class IncidentStep
{
    public string Id { get; set; }
    public string Step { get; set; }
    public string Description { get; set; }
    public DateTime Timestamp { get; set; }
    public string PerformedBy { get; set; }
}

public enum IncidentStatus
{
    Open,
    InProgress,
    Escalated,
    Resolved,
    Closed
}
```

#### **2.2 Auto Response System**
```csharp
// Sistema de respuesta automática
public class AutoResponseService
{
    private readonly ILogger<AutoResponseService> _logger;
    private readonly List<IAutoResponse> _autoResponses;
    
    public AutoResponseService(ILogger<AutoResponseService> logger)
    {
        _logger = logger;
        _autoResponses = new List<IAutoResponse>
        {
            new BlockIpResponse(null),
            new RequireMfaResponse(null),
            new DisableUserResponse(null),
            new QuarantineFileResponse(null),
            new NotifyAdminResponse(null)
        };
    }
    
    // Ejecutar respuesta automática
    public async Task ExecuteAutoResponseAsync(SecurityIncident incident)
    {
        try
        {
            var applicableResponses = _autoResponses
                .Where(r => r.CanExecute(incident))
                .OrderBy(r => r.Priority);
            
            foreach (var response in applicableResponses)
            {
                await response.ExecuteAsync(incident);
                _logger.LogInformation("Auto response executed: {ResponseType} for incident {IncidentId}", 
                    response.GetType().Name, incident.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing auto response for incident {IncidentId}", incident.Id);
            throw;
        }
    }
}

// Interfaz para respuestas automáticas
public interface IAutoResponse
{
    bool CanExecute(SecurityIncident incident);
    Task ExecuteAsync(SecurityIncident incident);
    int Priority { get; }
}

// Respuesta para bloquear IP
public class BlockIpResponse : IAutoResponse
{
    private readonly string _ipAddress;
    private readonly ILogger<BlockIpResponse> _logger;
    
    public BlockIpResponse(string ipAddress)
    {
        _ipAddress = ipAddress;
    }
    
    public bool CanExecute(SecurityIncident incident)
    {
        return !string.IsNullOrEmpty(incident.SourceIp) && 
               (incident.ThreatType == "Brute Force Attack" || 
                incident.ThreatType == "DDoS Attack");
    }
    
    public async Task ExecuteAsync(SecurityIncident incident)
    {
        try
        {
            // Implementar bloqueo de IP
            // Esto podría incluir actualizar firewall, WAF, etc.
            _logger.LogInformation("IP blocked: {IpAddress}", incident.SourceIp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blocking IP: {IpAddress}", incident.SourceIp);
            throw;
        }
    }
    
    public int Priority => 1;
}

// Respuesta para requerir MFA
public class RequireMfaResponse : IAutoResponse
{
    private readonly string _userId;
    private readonly ILogger<RequireMfaResponse> _logger;
    
    public RequireMfaResponse(string userId)
    {
        _userId = userId;
    }
    
    public bool CanExecute(SecurityIncident incident)
    {
        return !string.IsNullOrEmpty(incident.UserId) && 
               incident.ThreatType == "Suspicious Login";
    }
    
    public async Task ExecuteAsync(SecurityIncident incident)
    {
        try
        {
            // Implementar requerimiento de MFA
            // Esto podría incluir actualizar configuración de usuario
            _logger.LogInformation("MFA required for user: {UserId}", incident.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requiring MFA for user: {UserId}", incident.UserId);
            throw;
        }
    }
    
    public int Priority => 2;
}

// Respuesta para deshabilitar usuario
public class DisableUserResponse : IAutoResponse
{
    private readonly string _userId;
    private readonly ILogger<DisableUserResponse> _logger;
    
    public DisableUserResponse(string userId)
    {
        _userId = userId;
    }
    
    public bool CanExecute(SecurityIncident incident)
    {
        return !string.IsNullOrEmpty(incident.UserId) && 
               incident.Severity == SecuritySeverity.Critical;
    }
    
    public async Task ExecuteAsync(SecurityIncident incident)
    {
        try
        {
            // Implementar deshabilitación de usuario
            // Esto podría incluir actualizar estado del usuario en la base de datos
            _logger.LogInformation("User disabled: {UserId}", incident.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling user: {UserId}", incident.UserId);
            throw;
        }
    }
    
    public int Priority => 3;
}
```

### **3. Security Analytics**

#### **3.1 Threat Intelligence**
```csharp
// Servicio de inteligencia de amenazas
public class ThreatIntelligenceService
{
    private readonly ILogger<ThreatIntelligenceService> _logger;
    private readonly IThreatFeedService _threatFeedService;
    private readonly IThreatRepository _threatRepository;
    
    public ThreatIntelligenceService(
        ILogger<ThreatIntelligenceService> logger,
        IThreatFeedService threatFeedService,
        IThreatRepository threatRepository)
    {
        _logger = logger;
        _threatFeedService = threatFeedService;
        _threatRepository = threatRepository;
    }
    
    // Actualizar inteligencia de amenazas
    public async Task UpdateThreatIntelligenceAsync()
    {
        try
        {
            // Obtener feeds de amenazas
            var threatFeeds = await _threatFeedService.GetThreatFeedsAsync();
            
            foreach (var feed in threatFeeds)
            {
                var threats = await _threatFeedService.GetThreatsFromFeedAsync(feed);
                
                foreach (var threat in threats)
                {
                    await _threatRepository.StoreThreatAsync(threat);
                }
            }
            
            _logger.LogInformation("Threat intelligence updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating threat intelligence");
            throw;
        }
    }
    
    // Verificar si una IP es maliciosa
    public async Task<bool> IsMaliciousIpAsync(string ipAddress)
    {
        try
        {
            var threat = await _threatRepository.GetThreatByIpAsync(ipAddress);
            return threat != null && threat.IsActive;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if IP is malicious: {IpAddress}", ipAddress);
            return false;
        }
    }
    
    // Verificar si un dominio es malicioso
    public async Task<bool> IsMaliciousDomainAsync(string domain)
    {
        try
        {
            var threat = await _threatRepository.GetThreatByDomainAsync(domain);
            return threat != null && threat.IsActive;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if domain is malicious: {Domain}", domain);
            return false;
        }
    }
    
    // Obtener información de amenaza
    public async Task<ThreatInfo> GetThreatInfoAsync(string indicator)
    {
        try
        {
            var threat = await _threatRepository.GetThreatByIndicatorAsync(indicator);
            if (threat == null)
            {
                return null;
            }
            
            return new ThreatInfo
            {
                Indicator = threat.Indicator,
                Type = threat.Type,
                Severity = threat.Severity,
                Description = threat.Description,
                Source = threat.Source,
                FirstSeen = threat.FirstSeen,
                LastSeen = threat.LastSeen,
                IsActive = threat.IsActive
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting threat info for indicator: {Indicator}", indicator);
            throw;
        }
    }
}

// Modelos para inteligencia de amenazas
public class ThreatInfo
{
    public string Indicator { get; set; }
    public string Type { get; set; }
    public SecuritySeverity Severity { get; set; }
    public string Description { get; set; }
    public string Source { get; set; }
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
    public bool IsActive { get; set; }
}

public class ThreatFeed
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public string Type { get; set; }
    public DateTime LastUpdated { get; set; }
    public bool IsActive { get; set; }
}

public class Threat
{
    public string Id { get; set; }
    public string Indicator { get; set; }
    public string Type { get; set; }
    public SecuritySeverity Severity { get; set; }
    public string Description { get; set; }
    public string Source { get; set; }
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
    public bool IsActive { get; set; }
}
```

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Implementar Regla de Monitoreo Personalizada**
```csharp
// Crear una regla de monitoreo personalizada
public class CustomSecurityRule : ISecurityRule
{
    public async Task<ThreatDetectionResult> EvaluateAsync(SecurityEvent securityEvent)
    {
        // Implementar lógica de detección personalizada
        // 1. Analizar el evento de seguridad
        // 2. Determinar si es una amenaza
        // 3. Retornar resultado de detección
    }
}
```

### **Ejercicio 2: Implementar Respuesta Automática Personalizada**
```csharp
// Crear una respuesta automática personalizada
public class CustomAutoResponse : IAutoResponse
{
    public bool CanExecute(SecurityIncident incident)
    {
        // Implementar lógica para determinar si la respuesta es aplicable
    }
    
    public async Task ExecuteAsync(SecurityIncident incident)
    {
        // Implementar lógica de respuesta automática
    }
}
```

## 📝 **Resumen de la Clase**

### **Conceptos Clave Aprendidos:**
1. **Security Monitoring**: Monitoreo en tiempo real de eventos de seguridad
2. **Threat Detection**: Detección automática de amenazas
3. **Incident Response**: Gestión de incidentes de seguridad
4. **Auto Response**: Respuestas automáticas a amenazas
5. **Security Dashboard**: Visualización de métricas de seguridad
6. **Threat Intelligence**: Inteligencia de amenazas

### **Próxima Clase:**
En la siguiente clase exploraremos **Security Compliance y Auditing**, incluyendo cumplimiento de regulaciones y auditoría de seguridad.

---

## 🔗 **Recursos Adicionales**

- [NIST Cybersecurity Framework](https://www.nist.gov/cyberframework)
- [ISO 27001](https://www.iso.org/isoiec-27001-information-security.html)
- [SOC 2](https://www.aicpa.org/interestareas/frc/assuranceadvisoryservices/aicpasoc2report)
- [Security Monitoring Best Practices](https://owasp.org/www-project-security-monitoring/)
- [Incident Response Plan](https://www.sans.org/white-papers/incident-response/)
