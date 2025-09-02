# 🎵 Clase 3: Lógica de Negocio Avanzada

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 2: Comunicación en Tiempo Real](../senior_7/clase_2_comunicacion_tiempo_real.md)
- **🏠 Inicio del Módulo**: [Módulo 14: Plataformas Empresariales Reales](../senior_7/README.md)
- **➡️ Siguiente**: [Clase 4: Sistema de Estados y Transiciones](../senior_7/clase_4_sistema_estados_transiciones.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** algoritmos de scoring avanzado para matching musical
2. **Diseñar** sistemas de estados con transiciones complejas
3. **Crear** reglas de negocio robustas y validaciones
4. **Desarrollar** servicios de dominio especializados
5. **Aplicar** patrones de diseño para lógica de negocio compleja

---

## 🎵 **Algoritmo de Scoring Avanzado**

### **Sistema de Matching Musical Inteligente**

El algoritmo de scoring evalúa múltiples criterios para encontrar la mejor coincidencia entre músicos y solicitudes:

```csharp
// Interface para el servicio de matching musical
public interface IMusicianMatchingService
{
    Task<List<MusicianMatch>> FindMatchesForRequest(MusicianRequest request);
    Task<decimal> CalculateMatchScore(Musician musician, MusicianRequest request);
    Task<List<MusicianMatch>> GetTopMatches(MusicianRequest request, int topCount = 10);
    Task<List<MusicianMatch>> GetMatchesByInstrument(string instrument, string location, DateTime date);
    Task<decimal> CalculateLocationScore(string musicianLocation, string requestLocation);
    Task<decimal> CalculateBudgetScore(decimal hourlyRate, decimal budget, int durationHours);
    Task<decimal> CalculateReputationScore(Musician musician);
    Task<decimal> CalculatePenalties(Musician musician, MusicianRequest request);
}

// Implementación del servicio de matching musical
public class MusicianMatchingService : IMusicianMatchingService
{
    private readonly IMusicianRepository _musicianRepository;           // Repositorio de músicos
    private readonly ILocationService _locationService;                 // Servicio de ubicaciones
    private readonly ICalendarService _calendarService;                 // Servicio de calendario
    private readonly IRatingService _ratingService;                     // Servicio de calificaciones
    private readonly ILogger<MusicianMatchingService> _logger;          // Logger para auditoría
    private readonly IMemoryCache _cache;                              // Cache para optimización

    public MusicianMatchingService(
        IMusicianRepository musicianRepository,
        ILocationService locationService,
        ICalendarService calendarService,
        IRatingService ratingService,
        ILogger<MusicianMatchingService> logger,
        IMemoryCache cache)
    {
        _musicianRepository = musicianRepository;
        _locationService = locationService;
        _calendarService = calendarService;
        _ratingService = ratingService;
        _logger = logger;
        _cache = cache;
    }

    // Encontrar matches para una solicitud específica
    public async Task<List<MusicianMatch>> FindMatchesForRequest(MusicianRequest request)
    {
        try
        {
            _logger.LogInformation("Finding matches for request: {RequestId}", request.Id);
            
            // Clave de cache para esta consulta
            var cacheKey = $"matches_request_{request.Id}";
            
            // Intentar obtener del cache
            if (_cache.TryGetValue(cacheKey, out List<MusicianMatch> cachedMatches))
            {
                _logger.LogDebug("Cache hit for matches of request: {RequestId}", request.Id);
                return cachedMatches;
            }
            
            // Obtener músicos disponibles para el instrumento y fecha
            var availableMusicians = await _musicianRepository.GetAvailableMusiciansAsync(
                request.Instrument, 
                request.Date, 
                request.Location,
                request.DurationHours);
            
            var matches = new List<MusicianMatch>();
            
            // Calcular score para cada músico disponible
            foreach (var musician in availableMusicians)
            {
                var score = await CalculateMatchScore(musician, request);
                
                // Solo incluir músicos con score mínimo aceptable
                if (score >= 70) // Umbral mínimo de 70%
                {
                    var match = new MusicianMatch
                    {
                        Musician = musician,
                        Score = score,
                        MatchReason = GetMatchReason(score, musician, request),
                        CalculatedAt = DateTime.UtcNow,
                        RequestId = request.Id
                    };
                    
                    matches.Add(match);
                }
            }
            
            // Ordenar por score descendente
            var orderedMatches = matches.OrderByDescending(m => m.Score).ToList();
            
            // Guardar en cache por 10 minutos
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
            
            _cache.Set(cacheKey, orderedMatches, cacheOptions);
            
            _logger.LogInformation("Found {Count} matches for request: {RequestId}", orderedMatches.Count, request.Id);
            
            return orderedMatches;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding matches for request: {RequestId}", request.Id);
            throw;
        }
    }

    // Calcular score de compatibilidad entre músico y solicitud
    public async Task<decimal> CalculateMatchScore(Musician musician, MusicianRequest request)
    {
        try
        {
            decimal totalScore = 0;
            
            // 1. Score por instrumento (40 puntos máximo)
            var instrumentScore = CalculateInstrumentScore(musician, request);
            totalScore += instrumentScore;
            
            // 2. Score por ubicación (30 puntos máximo)
            var locationScore = await CalculateLocationScore(musician.Location, request.Location);
            totalScore += locationScore;
            
            // 3. Score por presupuesto (20 puntos máximo)
            var budgetScore = CalculateBudgetScore(musician.HourlyRate, request.Budget, request.DurationHours);
            totalScore += budgetScore;
            
            // 4. Score por experiencia y calificación (10 puntos máximo)
            var reputationScore = CalculateReputationScore(musician);
            totalScore += reputationScore;
            
            // 5. Aplicar penalizaciones
            var penalties = await CalculatePenalties(musician, request);
            totalScore -= penalties;
            
            // 6. Aplicar bonificaciones especiales
            var bonuses = CalculateSpecialBonuses(musician, request);
            totalScore += bonuses;
            
            // Asegurar que el score esté entre 0 y 100
            return Math.Max(0, Math.Min(100, totalScore));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating match score for musician {MusicianId} and request {RequestId}", 
                musician.Id, request.Id);
            return 0;
        }
    }

    // Calcular score por instrumento
    private decimal CalculateInstrumentScore(Musician musician, MusicianRequest request)
    {
        decimal score = 0;
        
        // Verificar si el músico toca el instrumento requerido
        if (musician.Instruments.Contains(request.Instrument))
        {
            score += 30; // Base por instrumento principal
            
            // Bonificación por nivel de experiencia en el instrumento
            var instrumentExperience = musician.InstrumentExperience
                .FirstOrDefault(ie => ie.Instrument == request.Instrument);
            
            if (instrumentExperience != null)
            {
                // Bonificación por años de experiencia (máximo 10 puntos)
                score += Math.Min(instrumentExperience.YearsOfExperience * 2, 10);
            }
            
            // Bonificación por estilo musical si coincide
            if (musician.MusicalStyles.Contains(request.Style))
            {
                score += 5;
            }
        }
        else
        {
            // Penalización por no tocar el instrumento requerido
            score -= 50;
        }
        
        return score;
    }

    // Calcular score por ubicación
    public async Task<decimal> CalculateLocationScore(string musicianLocation, string requestLocation)
    {
        try
        {
            // Clave de cache para esta consulta de distancia
            var cacheKey = $"distance_{musicianLocation}_{requestLocation}";
            
            // Intentar obtener del cache
            if (_cache.TryGetValue(cacheKey, out decimal cachedDistance))
            {
                return CalculateLocationScoreFromDistance(cachedDistance);
            }
            
            // Calcular distancia real entre ubicaciones
            var distance = await _locationService.CalculateDistance(musicianLocation, requestLocation);
            
            // Guardar en cache por 1 hora (las distancias no cambian frecuentemente)
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));
            
            _cache.Set(cacheKey, distance, cacheOptions);
            
            return CalculateLocationScoreFromDistance(distance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating location score between {MusicianLocation} and {RequestLocation}", 
                musicianLocation, requestLocation);
            return 0; // Score mínimo en caso de error
        }
    }

    // Calcular score de ubicación basado en distancia
    private decimal CalculateLocationScoreFromDistance(decimal distance)
    {
        if (distance <= 5) return 30;      // Misma ciudad (0-5 km)
        if (distance <= 15) return 25;     // Ciudad cercana (5-15 km)
        if (distance <= 30) return 20;     // Área metropolitana (15-30 km)
        if (distance <= 50) return 15;     // Región (30-50 km)
        if (distance <= 100) return 10;    // Estado/Provincia (50-100 km)
        if (distance <= 200) return 5;     // País (100-200 km)
        return 0;                           // Muy lejos (>200 km)
    }

    // Calcular score por presupuesto
    public decimal CalculateBudgetScore(decimal hourlyRate, decimal budget, int durationHours)
    {
        try
        {
            // Estimar horas totales del evento
            var estimatedHours = EstimateEventHours(durationHours);
            
            // Calcular costo total estimado
            var totalCost = hourlyRate * estimatedHours;
            
            // Calcular porcentaje del presupuesto que representa
            var budgetPercentage = (totalCost / budget) * 100;
            
            // Asignar score basado en el porcentaje del presupuesto
            if (budgetPercentage <= 60) return 20;      // Muy económico (≤60%)
            if (budgetPercentage <= 75) return 18;      // Económico (60-75%)
            if (budgetPercentage <= 85) return 15;      // Dentro del presupuesto (75-85%)
            if (budgetPercentage <= 95) return 12;      // Aceptable (85-95%)
            if (budgetPercentage <= 105) return 8;      // Ligeramente por encima (95-105%)
            if (budgetPercentage <= 120) return 5;      // Por encima pero aceptable (105-120%)
            return 0;                                   // Fuera del presupuesto (>120%)
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating budget score for hourly rate {HourlyRate} and budget {Budget}", 
                hourlyRate, budget);
            return 0;
        }
    }

    // Estimar horas totales del evento
    private int EstimateEventHours(int durationHours)
    {
        // Agregar tiempo de preparación y transporte
        var preparationTime = 1; // 1 hora de preparación
        var transportTime = 1;   // 1 hora de transporte (ida y vuelta)
        
        return durationHours + preparationTime + transportTime;
    }

    // Calcular score por reputación
    public decimal CalculateReputationScore(Musician musician)
    {
        try
        {
            decimal score = 0;
            
            // Score por años de experiencia (máximo 5 puntos)
            var experienceScore = Math.Min(musician.YearsOfExperience * 0.5m, 5);
            score += experienceScore;
            
            // Score por calificación promedio (máximo 5 puntos)
            var ratingScore = Math.Min(musician.AverageRating * 1.25m, 5);
            score += ratingScore;
            
            // Bonificación por número de eventos completados exitosamente
            var completedEvents = musician.CompletedEventsCount;
            if (completedEvents >= 100) score += 2;
            else if (completedEvents >= 50) score += 1;
            else if (completedEvents >= 10) score += 0.5m;
            
            // Bonificación por certificaciones musicales
            var certificationBonus = musician.Certifications.Count * 0.5m;
            score += Math.Min(certificationBonus, 2);
            
            return score;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating reputation score for musician {MusicianId}", musician.Id);
            return 0;
        }
    }

    // Calcular penalizaciones
    public async Task<decimal> CalculatePenalties(Musician musician, MusicianRequest request)
    {
        try
        {
            decimal totalPenalties = 0;
            
            // 1. Penalización por cancelaciones recientes
            var recentCancellations = musician.CancellationsInLastYear;
            totalPenalties += recentCancellations * 3; // 3 puntos por cancelación
            
            // 2. Penalización por respuesta lenta
            var averageResponseTime = musician.AverageResponseTime;
            if (averageResponseTime > TimeSpan.FromHours(4))
                totalPenalties += 5;
            else if (averageResponseTime > TimeSpan.FromHours(2))
                totalPenalties += 3;
            
            // 3. Penalización por conflictos de calendario
            var hasCalendarConflicts = await _calendarService.HasConflictsAsync(
                musician.Id, request.Date, request.DurationHours);
            if (hasCalendarConflicts)
                totalPenalties += 15;
            
            // 4. Penalización por baja calificación reciente
            var recentRating = await _ratingService.GetRecentAverageRatingAsync(musician.Id, 6); // Últimos 6 meses
            if (recentRating < 3.0m)
                totalPenalties += 10;
            else if (recentRating < 3.5m)
                totalPenalties += 5;
            
            // 5. Penalización por distancia excesiva
            var distance = await _locationService.CalculateDistance(musician.Location, request.Location);
            if (distance > 100) // Más de 100 km
                totalPenalties += 8;
            
            return totalPenalties;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating penalties for musician {MusicianId}", musician.Id);
            return 0;
        }
    }

    // Calcular bonificaciones especiales
    private decimal CalculateSpecialBonuses(Musician musician, MusicianRequest request)
    {
        decimal totalBonuses = 0;
        
        // 1. Bonificación por disponibilidad inmediata
        if (musician.IsAvailableImmediately)
            totalBonuses += 3;
        
        // 2. Bonificación por experiencia específica en el tipo de evento
        if (musician.EventTypeExperience.Contains(request.EventType))
            totalBonuses += 4;
        
        // 3. Bonificación por recomendaciones de organizadores
        var organizerRecommendations = musician.OrganizerRecommendations
            .Count(r => r.OrganizerId == request.OrganizerId);
        totalBonuses += Math.Min(organizerRecommendations * 2, 6);
        
        // 4. Bonificación por instrumentos adicionales
        var additionalInstruments = musician.Instruments
            .Where(i => i != request.Instrument)
            .Count();
        totalBonuses += Math.Min(additionalInstruments, 3);
        
        // 5. Bonificación por horario flexible
        if (musician.IsFlexibleWithSchedule)
            totalBonuses += 2;
        
        return totalBonuses;
    }

    // Obtener razón del match para explicar el score
    private string GetMatchReason(decimal score, Musician musician, MusicianRequest request)
    {
        var reasons = new List<string>();
        
        if (score >= 90)
        {
            reasons.Add("Excelente compatibilidad");
            reasons.Add("Experiencia destacada");
            reasons.Add("Ubicación ideal");
        }
        else if (score >= 80)
        {
            reasons.Add("Muy buena compatibilidad");
            reasons.Add("Experiencia sólida");
        }
        else if (score >= 70)
        {
            reasons.Add("Buena compatibilidad");
            reasons.Add("Cumple requisitos básicos");
        }
        
        // Agregar razones específicas
        if (musician.Instruments.Contains(request.Instrument))
            reasons.Add($"Toca {request.Instrument}");
        
        if (musician.MusicalStyles.Contains(request.Style))
            reasons.Add($"Estilo musical compatible");
        
        if (musician.AverageRating >= 4.5m)
            reasons.Add("Alta calificación");
        
        return string.Join(", ", reasons);
    }

    // Obtener top matches para una solicitud
    public async Task<List<MusicianMatch>> GetTopMatches(MusicianRequest request, int topCount = 10)
    {
        try
        {
            var allMatches = await FindMatchesForRequest(request);
            return allMatches.Take(topCount).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top matches for request: {RequestId}", request.Id);
            throw;
        }
    }

    // Obtener matches por instrumento, ubicación y fecha
    public async Task<List<MusicianMatch>> GetMatchesByInstrument(string instrument, string location, DateTime date)
    {
        try
        {
            var availableMusicians = await _musicianRepository.GetAvailableMusiciansAsync(
                instrument, date, location, 4); // Duración estándar de 4 horas
            
            var matches = new List<MusicianMatch>();
            
            foreach (var musician in availableMusicians)
            {
                // Crear una solicitud temporal para calcular el score
                var tempRequest = new MusicianRequest
                {
                    Instrument = instrument,
                    Location = location,
                    Date = date,
                    DurationHours = 4
                };
                
                var score = await CalculateMatchScore(musician, tempRequest);
                
                if (score >= 60) // Umbral más bajo para búsquedas generales
                {
                    matches.Add(new MusicianMatch
                    {
                        Musician = musician,
                        Score = score,
                        MatchReason = GetMatchReason(score, musician, tempRequest),
                        CalculatedAt = DateTime.UtcNow
                    });
                }
            }
            
            return matches.OrderByDescending(m => m.Score).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting matches by instrument {Instrument} in {Location}", instrument, location);
            throw;
        }
    }
}
```

---

## 🎭 **Sistema de Estados y Transiciones**

### **Gestión del Ciclo de Vida de Solicitudes**

El sistema de estados maneja las transiciones permitidas y las reglas de negocio asociadas:

```csharp
// Enum de estados de solicitud
public enum RequestStatus
{
    Draft,          // Borrador - Solo visible para el organizador
    Pending,        // Pendiente de revisión - Enviada para aprobación
    Active,         // Activa - Visible para músicos
    InReview,       // En revisión - Organizador evaluando aplicaciones
    Assigned,       // Asignada - Músico seleccionado
    InProgress,     // En progreso - Evento en curso
    Completed,      // Completada - Evento finalizado exitosamente
    Cancelled,      // Cancelada - Evento cancelado
    Expired,        // Expirada - Fecha pasó sin asignación
    Disputed        // En disputa - Problema reportado
}

// Clase para definir transiciones de estado
public class RequestStatusTransition
{
    public RequestStatus FromStatus { get; set; }                    // Estado de origen
    public RequestStatus ToStatus { get; set; }                      // Estado de destino
    public string[] AllowedRoles { get; set; }                       // Roles que pueden realizar la transición
    public Func<MusicianRequest, bool> CanTransition { get; set; }  // Función que valida si se puede transicionar
    public Action<MusicianRequest> OnTransition { get; set; }       // Acción a ejecutar al transicionar
    public string Description { get; set; }                          // Descripción de la transición
    public bool RequiresApproval { get; set; }                      // Si requiere aprobación
    public string[] RequiredFields { get; set; }                     // Campos requeridos para la transición
}

// Gestor de estados de solicitud
public class RequestStatusManager
{
    private readonly List<RequestStatusTransition> _transitions;     // Lista de transiciones permitidas
    private readonly INotificationService _notificationService;      // Servicio de notificaciones
    private readonly IAuditService _auditService;                    // Servicio de auditoría
    private readonly ILogger<RequestStatusManager> _logger;          // Logger para auditoría

    public RequestStatusManager(
        INotificationService notificationService,
        IAuditService auditService,
        ILogger<RequestStatusManager> logger)
    {
        _notificationService = notificationService;
        _auditService = auditService;
        _logger = logger;
        InitializeTransitions();
    }

    // Inicializar todas las transiciones permitidas
    private void InitializeTransitions()
    {
        _transitions = new List<RequestStatusTransition>
        {
            // Transición: Draft -> Pending
            new RequestStatusTransition
            {
                FromStatus = RequestStatus.Draft,
                ToStatus = RequestStatus.Pending,
                AllowedRoles = new[] { "Organizador", "Admin" },
                CanTransition = request => 
                    !string.IsNullOrEmpty(request.EventType) && 
                    request.Date > DateTime.Today &&
                    request.Budget >= 50 &&
                    !string.IsNullOrEmpty(request.Description),
                OnTransition = request => 
                {
                    request.SubmittedAt = DateTime.UtcNow;
                    _logger.LogInformation("Request {Id} submitted for review", request.Id);
                },
                Description = "Enviar solicitud para revisión",
                RequiresApproval = false,
                RequiredFields = new[] { "EventType", "Date", "Budget", "Description" }
            },

            // Transición: Pending -> Active
            new RequestStatusTransition
            {
                FromStatus = RequestStatus.Pending,
                ToStatus = RequestStatus.Active,
                AllowedRoles = new[] { "Admin", "Moderador" },
                CanTransition = request => 
                    request.Budget >= 50 && 
                    request.Budget <= 10000 &&
                    request.Date > DateTime.Today.AddDays(1),
                OnTransition = async request => 
                {
                    request.ApprovedAt = DateTime.UtcNow;
                    request.ApprovedBy = GetCurrentUserId();
                    await _notificationService.NotifyMusiciansOfNewRequest(request);
                    _logger.LogInformation("Request {Id} approved and activated", request.Id);
                },
                Description = "Aprobar y activar solicitud",
                RequiresApproval = true,
                RequiredFields = new[] { "ApprovedBy", "ApprovedAt" }
            },

            // Transición: Active -> InReview
            new RequestStatusTransition
            {
                FromStatus = RequestStatus.Active,
                ToStatus = RequestStatus.InReview,
                AllowedRoles = new[] { "Organizador", "Admin" },
                CanTransition = request => 
                    request.Applications.Any() && 
                    request.Applications.Count >= 1,
                OnTransition = async request => 
                {
                    request.ReviewStartedAt = DateTime.UtcNow;
                    await _notificationService.NotifyOrganizerOfApplicationsReceived(request.Id);
                    _logger.LogInformation("Request {Id} moved to review phase", request.Id);
                },
                Description = "Iniciar revisión de aplicaciones",
                RequiresApproval = false,
                RequiredFields = new[] { "ReviewStartedAt" }
            },

            // Transición: InReview -> Assigned
            new RequestStatusTransition
            {
                FromStatus = RequestStatus.InReview,
                ToStatus = RequestStatus.Assigned,
                AllowedRoles = new[] { "Organizador", "Admin" },
                CanTransition = request => 
                    request.AssignedMusicianId.HasValue &&
                    request.AssignedMusician != null,
                OnTransition = async request => 
                {
                    request.AssignedAt = DateTime.UtcNow;
                    await _notificationService.NotifyMusicianOfRequestAssignment(
                        request.Id, request.AssignedMusicianId.Value);
                    await _notificationService.NotifyAllOfStatusChange(request.Id, RequestStatus.Assigned);
                    _logger.LogInformation("Request {Id} assigned to musician {MusicianId}", 
                        request.Id, request.AssignedMusicianId.Value);
                },
                Description = "Asignar solicitud a músico",
                RequiresApproval = false,
                RequiredFields = new[] { "AssignedMusicianId", "AssignedAt" }
            },

            // Transición: Assigned -> InProgress
            new RequestStatusTransition
            {
                FromStatus = RequestStatus.Assigned,
                ToStatus = RequestStatus.InProgress,
                AllowedRoles = new[] { "Organizador", "Admin", "Musico" },
                CanTransition = request => 
                    request.Date <= DateTime.Today &&
                    request.Date.AddHours(request.DurationHours) >= DateTime.Now,
                OnTransition = request => 
                {
                    request.StartedAt = DateTime.UtcNow;
                    _logger.LogInformation("Request {Id} started", request.Id);
                },
                Description = "Marcar evento como iniciado",
                RequiresApproval = false,
                RequiredFields = new[] { "StartedAt" }
            },

            // Transición: InProgress -> Completed
            new RequestStatusTransition
            {
                FromStatus = RequestStatus.InProgress,
                ToStatus = RequestStatus.Completed,
                AllowedRoles = new[] { "Organizador", "Admin", "Musico" },
                CanTransition = request => 
                    request.StartedAt.HasValue &&
                    request.Date.AddHours(request.DurationHours) < DateTime.Now,
                OnTransition = async request => 
                {
                    request.CompletedAt = DateTime.UtcNow;
                    await _notificationService.NotifyAllOfStatusChange(request.Id, RequestStatus.Completed);
                    _logger.LogInformation("Request {Id} completed", request.Id);
                },
                Description = "Marcar evento como completado",
                RequiresApproval = false,
                RequiredFields = new[] { "CompletedAt" }
            },

            // Transición: Active -> Cancelled
            new RequestStatusTransition
            {
                FromStatus = RequestStatus.Active,
                ToStatus = RequestStatus.Cancelled,
                AllowedRoles = new[] { "Organizador", "Admin" },
                CanTransition = request => 
                    request.Date > DateTime.Today.AddDays(1) &&
                    !request.AssignedMusicianId.HasValue,
                OnTransition = async request => 
                {
                    request.CancelledAt = DateTime.UtcNow;
                    request.CancelledBy = GetCurrentUserId();
                    await _notificationService.NotifyAllOfStatusChange(request.Id, RequestStatus.Cancelled);
                    _logger.LogInformation("Request {Id} cancelled by {UserId}", 
                        request.Id, GetCurrentUserId());
                },
                Description = "Cancelar solicitud",
                RequiresApproval = false,
                RequiredFields = new[] { "CancelledBy", "CancelledAt", "CancellationReason" }
            },

            // Transición: Assigned -> Cancelled
            new RequestStatusTransition
            {
                FromStatus = RequestStatus.Assigned,
                ToStatus = RequestStatus.Cancelled,
                AllowedRoles = new[] { "Organizador", "Admin" },
                CanTransition = request => 
                    request.Date > DateTime.Today.AddDays(1),
                OnTransition = async request => 
                {
                    request.CancelledAt = DateTime.UtcNow;
                    request.CancelledBy = GetCurrentUserId();
                    await _notificationService.NotifyMusicianOfRequestCancellation(
                        request.Id, request.AssignedMusicianId.Value);
                    await _notificationService.NotifyAllOfStatusChange(request.Id, RequestStatus.Cancelled);
                    _logger.LogInformation("Assigned request {Id} cancelled by {UserId}", 
                        request.Id, GetCurrentUserId());
                },
                Description = "Cancelar solicitud asignada",
                RequiresApproval = true,
                RequiredFields = new[] { "CancelledBy", "CancelledAt", "CancellationReason" }
            },

            // Transición: Active -> Expired
            new RequestStatusTransition
            {
                FromStatus = RequestStatus.Active,
                ToStatus = RequestStatus.Expired,
                AllowedRoles = new[] { "System", "Admin" },
                CanTransition = request => 
                    request.Date < DateTime.Today &&
                    !request.AssignedMusicianId.HasValue,
                OnTransition = async request => 
                {
                    request.ExpiredAt = DateTime.UtcNow;
                    await _notificationService.NotifyOrganizerOfRequestExpiration(request.Id);
                    _logger.LogInformation("Request {Id} expired automatically", request.Id);
                },
                Description = "Solicitud expirada automáticamente",
                RequiresApproval = false,
                RequiredFields = new[] { "ExpiredAt" }
            }
        };
    }

    // Intentar transicionar el estado de una solicitud
    public async Task<bool> TryTransitionStatus(
        MusicianRequest request, 
        RequestStatus newStatus, 
        string userRole,
        Dictionary<string, object> transitionData = null)
    {
        try
        {
            _logger.LogInformation("Attempting to transition request {Id} from {From} to {To} by role {Role}", 
                request.Id, request.Status, newStatus, userRole);
            
            // Buscar la transición válida
            var transition = _transitions.FirstOrDefault(t => 
                t.FromStatus == request.Status && 
                t.ToStatus == newStatus &&
                t.AllowedRoles.Contains(userRole));
            
            if (transition == null)
            {
                _logger.LogWarning("Invalid status transition from {From} to {To} for role {Role}", 
                    request.Status, newStatus, userRole);
                return false;
            }
            
            // Validar si se puede realizar la transición
            if (!transition.CanTransition(request))
            {
                _logger.LogWarning("Cannot transition request {Id} from {From} to {To}: validation failed", 
                    request.Id, request.Status, newStatus);
                return false;
            }
            
            // Validar campos requeridos si se proporcionan datos de transición
            if (transition.RequiredFields != null && transitionData != null)
            {
                var missingFields = transition.RequiredFields
                    .Where(field => !transitionData.ContainsKey(field))
                    .ToList();
                
                if (missingFields.Any())
                {
                    _logger.LogWarning("Missing required fields for transition: {MissingFields}", 
                        string.Join(", ", missingFields));
                    return false;
                }
            }
            
            // Registrar el estado anterior para auditoría
            var oldStatus = request.Status;
            var oldStatusData = new
            {
                Status = oldStatus,
                ChangedAt = request.UpdatedAt,
                ChangedBy = request.UpdatedBy
            };
            
            // Aplicar la transición
            request.Status = newStatus;
            request.UpdatedAt = DateTime.UtcNow;
            request.UpdatedBy = GetCurrentUserId();
            
            // Aplicar datos de transición si se proporcionan
            if (transitionData != null)
            {
                ApplyTransitionData(request, transitionData);
            }
            
            // Ejecutar la acción de transición
            transition.OnTransition?.Invoke(request);
            
            // Registrar la transición en auditoría
            await _auditService.LogStatusTransitionAsync(
                request.Id, 
                oldStatus, 
                newStatus, 
                GetCurrentUserId(), 
                userRole,
                transitionData);
            
            _logger.LogInformation("Request {Id} status changed from {Old} to {New} by {UserRole}", 
                request.Id, oldStatus, newStatus, userRole);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transitioning request {Id} from {From} to {To}", 
                request.Id, request.Status, newStatus);
            return false;
        }
    }

    // Aplicar datos de transición a la solicitud
    private void ApplyTransitionData(MusicianRequest request, Dictionary<string, object> transitionData)
    {
        foreach (var kvp in transitionData)
        {
            var property = typeof(MusicianRequest).GetProperty(kvp.Key);
            if (property != null && property.CanWrite)
            {
                try
                {
                    var value = Convert.ChangeType(kvp.Value, property.PropertyType);
                    property.SetValue(request, value);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Could not set property {Property} with value {Value}: {Error}", 
                        kvp.Key, kvp.Value, ex.Message);
                }
            }
        }
    }

    // Obtener transiciones disponibles para un estado
    public List<RequestStatusTransition> GetAvailableTransitions(RequestStatus currentStatus, string userRole)
    {
        return _transitions
            .Where(t => t.FromStatus == currentStatus && t.AllowedRoles.Contains(userRole))
            .ToList();
    }

    // Validar si una transición es válida
    public bool IsTransitionValid(RequestStatus fromStatus, RequestStatus toStatus, string userRole)
    {
        return _transitions.Any(t => 
            t.FromStatus == fromStatus && 
            t.ToStatus == toStatus && 
            t.AllowedRoles.Contains(userRole));
    }

    // Obtener el ID del usuario actual (implementar según el contexto)
    private Guid GetCurrentUserId()
    {
        // Implementar según el contexto de autenticación
        // Por ejemplo, desde HttpContext.User o similar
        return Guid.Empty; // Placeholder
    }
}
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Algoritmo de Scoring**
```csharp
// Implementa un algoritmo de scoring que considere:
// - Preferencias del organizador
// - Historial de colaboraciones
// - Disponibilidad de calendario
// - Calificaciones específicas por tipo de evento
```

### **Ejercicio 2: Sistema de Estados**
```csharp
// Crea un sistema de estados para un proceso de contratación:
// - Aplicación recibida
// - En revisión
// - Entrevista programada
// - Aprobada/Rechazada
// - Contrato firmado
```

### **Ejercicio 3: Reglas de Negocio**
```csharp
// Implementa reglas de negocio para:
// - Validación de presupuestos según tipo de evento
// - Verificación de disponibilidad de músicos
// - Cálculo de tarifas dinámicas
// - Gestión de descuentos y bonificaciones
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **🎵 Algoritmo de Scoring**: Sistema inteligente de matching musical con múltiples criterios
2. **🎭 Estados y Transiciones**: Gestión robusta del ciclo de vida de solicitudes
3. **🔧 Reglas de Negocio**: Validaciones y lógica compleja para el dominio musical
4. **📊 Scoring Avanzado**: Cálculo de compatibilidad con penalizaciones y bonificaciones
5. **🔄 Transiciones Seguras**: Sistema de estados con validaciones y auditoría

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Sistema de Estados y Transiciones**, profundizando en la gestión de flujos de trabajo complejos y validaciones avanzadas.

---

**¡Has completado la tercera clase del Módulo 14! 🎵🎭**
