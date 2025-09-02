# 🎯 Clase 3: Solicitudes y Matching de Músicos

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 2: Entidades y Agregados del Dominio](../senior_9/clase_2_entidades_agregados_dominio.md)
- **🏠 Inicio del Módulo**: [Módulo 16: Maestría Total y Liderazgo Técnico](../senior_9/README.md)
- **➡️ Siguiente**: [Clase 4: Sistema de Mensajería y Chat](../senior_9/clase_4_sistema_mensajeria_chat.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** algoritmo de matching de músicos
2. **Gestionar** solicitudes musicales
3. **Crear** sistema de aplicaciones y propuestas
4. **Implementar** filtros y búsquedas avanzadas
5. **Configurar** notificaciones automáticas

---

## 🎵 **Algoritmo de Matching de Músicos**

### **Servicio de Matching**

```csharp
// MusicalMatching.Application/Services/MusicianMatchingService.cs
namespace MusicalMatching.Application.Services;

public interface IMusicianMatchingService
{
    Task<List<MusicianMatchResult>> FindMatchingMusiciansAsync(MusicianRequest request, int limit = 10);
    Task<double> CalculateMatchScoreAsync(MusicianProfile musician, MusicianRequest request);
    Task<List<MusicianMatchResult>> GetRecommendedMusiciansAsync(Guid userId, int limit = 5);
    Task<List<MusicianRequest>> GetRecommendedRequestsAsync(Guid musicianId, int limit = 5);
}

public class MusicianMatchingService : IMusicianMatchingService
{
    private readonly IMusicianProfileRepository _musicianRepository;
    private readonly IMusicianRequestRepository _requestRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly ILogger<MusicianMatchingService> _logger;
    private readonly IMemoryCache _cache;

    public MusicianMatchingService(
        IMusicianProfileRepository musicianRepository,
        IMusicianRequestRepository requestRepository,
        IReviewRepository reviewRepository,
        ILogger<MusicianMatchingService> logger,
        IMemoryCache cache)
    {
        _musicianRepository = musicianRepository;
        _requestRepository = requestRepository;
        _reviewRepository = reviewRepository;
        _logger = logger;
        _cache = cache;
    }

    public async Task<List<MusicianMatchResult>> FindMatchingMusiciansAsync(
        MusicianRequest request, int limit = 10)
    {
        var cacheKey = $"matching_musicians_{request.Id}_{limit}";
        
        if (_cache.TryGetValue(cacheKey, out List<MusicianMatchResult> cachedResult))
        {
            _logger.LogInformation("Returning cached matching results for request {RequestId}", request.Id);
            return cachedResult;
        }

        _logger.LogInformation("Finding matching musicians for request {RequestId}", request.Id);

        // Obtener músicos disponibles
        var availableMusicians = await _musicianRepository.GetAvailableMusiciansAsync(
            request.Instrument, request.Genre, request.Location, request.Latitude, request.Longitude);

        var matchResults = new List<MusicianMatchResult>();

        foreach (var musician in availableMusicians)
        {
            var matchScore = await CalculateMatchScoreAsync(musician, request);
            
            if (matchScore > 0.3) // Solo incluir matches con score > 30%
            {
                var matchResult = new MusicianMatchResult
                {
                    MusicianId = musician.Id,
                    MusicianName = musician.User.FullName,
                    MatchScore = matchScore,
                    InstrumentMatch = musician.Instruments.Contains(request.Instrument),
                    GenreMatch = musician.Genres.Contains(request.Genre),
                    LocationMatch = CalculateLocationScore(musician, request),
                    RatingMatch = musician.AverageRating / 5.0,
                    ExperienceMatch = CalculateExperienceScore(musician, request),
                    AvailabilityMatch = await CalculateAvailabilityScoreAsync(musician, request),
                    BudgetMatch = CalculateBudgetScore(musician, request),
                    ResponseTimeMatch = CalculateResponseTimeScore(musician),
                    RecentCancellationsPenalty = CalculateCancellationPenalty(musician)
                };

                matchResults.Add(matchResult);
            }
        }

        // Ordenar por score de matching
        var sortedResults = matchResults
            .OrderByDescending(r => r.MatchScore)
            .Take(limit)
            .ToList();

        // Cachear resultados por 15 minutos
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
        _cache.Set(cacheKey, sortedResults, cacheOptions);

        _logger.LogInformation("Found {Count} matching musicians for request {RequestId}", 
            sortedResults.Count, request.Id);

        return sortedResults;
    }

    public async Task<double> CalculateMatchScoreAsync(MusicianProfile musician, MusicianRequest request)
    {
        var weights = new MatchScoreWeights
        {
            Instrument = 0.25,
            Genre = 0.20,
            Location = 0.15,
            Rating = 0.15,
            Experience = 0.10,
            Availability = 0.10,
            Budget = 0.05
        };

        var scores = new Dictionary<string, double>
        {
            ["Instrument"] = musician.Instruments.Contains(request.Instrument) ? 1.0 : 0.0,
            ["Genre"] = musician.Genres.Contains(request.Genre) ? 1.0 : 0.0,
            ["Location"] = CalculateLocationScore(musician, request),
            ["Rating"] = musician.AverageRating / 5.0,
            ["Experience"] = CalculateExperienceScore(musician, request),
            ["Availability"] = await CalculateAvailabilityScoreAsync(musician, request),
            ["Budget"] = CalculateBudgetScore(musician, request)
        };

        var totalScore = scores.Sum(kvp => kvp.Value * weights.GetType()
            .GetProperty(kvp.Key)?.GetValue(weights) as double? ?? 0.0);

        // Aplicar penalizaciones
        var cancellationPenalty = CalculateCancellationPenalty(musician);
        var responseTimePenalty = CalculateResponseTimePenalty(musician);

        totalScore -= cancellationPenalty + responseTimePenalty;

        return Math.Max(0.0, Math.Min(1.0, totalScore));
    }

    private double CalculateLocationScore(MusicianProfile musician, MusicianRequest request)
    {
        var distance = CalculateDistance(
            musician.Latitude, musician.Longitude,
            request.Latitude, request.Longitude);

        // Score basado en distancia (0-50km = 1.0, 100km+ = 0.0)
        if (distance <= 50) return 1.0;
        if (distance >= 100) return 0.0;
        
        return 1.0 - ((distance - 50) / 50);
    }

    private double CalculateExperienceScore(MusicianProfile musician, MusicianRequest request)
    {
        var eventType = request.EventType.ToLower();
        
        // Ajustar score basado en tipo de evento y experiencia
        if (eventType.Contains("wedding") || eventType.Contains("corporate"))
        {
            // Eventos formales requieren más experiencia
            if (musician.YearsOfExperience >= 5) return 1.0;
            if (musician.YearsOfExperience >= 3) return 0.8;
            if (musician.YearsOfExperience >= 1) return 0.6;
            return 0.3;
        }
        else
        {
            // Eventos informales son más flexibles
            if (musician.YearsOfExperience >= 3) return 1.0;
            if (musician.YearsOfExperience >= 1) return 0.8;
            return 0.6;
        }
    }

    private async Task<double> CalculateAvailabilityScoreAsync(MusicianProfile musician, MusicianRequest request)
    {
        var eventDate = request.Date;
        var eventTime = request.Time;
        var eventDayOfWeek = eventDate.DayOfWeek;

        // Verificar si el músico tiene disponibilidad en esa fecha/hora
        var availableSlot = musician.AvailabilitySlots
            .FirstOrDefault(slot => 
                slot.Day == eventDayOfWeek && 
                slot.IsAvailable &&
                slot.StartTime <= eventTime && 
                slot.EndTime >= eventTime.Add(request.Duration));

        if (availableSlot == null) return 0.0;

        // Verificar si no hay conflictos con otros eventos
        var conflictingRequests = await _requestRepository.GetMusicianRequestsAsync(
            musician.UserId, eventDate, eventTime, request.Duration);

        return conflictingRequests.Any() ? 0.0 : 1.0;
    }

    private double CalculateBudgetScore(MusicianProfile musician, MusicianRequest request)
    {
        var musicianRate = musician.HourlyRate * (decimal)request.Duration.TotalHours;
        
        if (request.IsNegotiable)
        {
            // Si es negociable, ser más flexible
            if (musicianRate <= request.MaxBudget && musicianRate >= request.MinBudget)
                return 1.0;
            if (musicianRate <= request.MaxBudget * 1.2m)
                return 0.8;
            if (musicianRate <= request.MaxBudget * 1.5m)
                return 0.5;
            return 0.0;
        }
        else
        {
            // Si no es negociable, ser más estricto
            if (musicianRate <= request.Budget)
                return 1.0;
            if (musicianRate <= request.Budget * 1.1m)
                return 0.5;
            return 0.0;
        }
    }

    private double CalculateCancellationPenalty(MusicianProfile musician)
    {
        // Penalizar cancelaciones recientes
        if (musician.RecentCancellations == 0) return 0.0;
        if (musician.RecentCancellations == 1) return 0.05;
        if (musician.RecentCancellations == 2) return 0.15;
        return 0.25; // 3+ cancelaciones
    }

    private double CalculateResponseTimePenalty(MusicianProfile musician)
    {
        // Penalizar tiempos de respuesta lentos
        var avgResponseMinutes = musician.AverageResponseTime.TotalMinutes;
        
        if (avgResponseMinutes <= 60) return 0.0;      // 1 hora o menos
        if (avgResponseMinutes <= 240) return 0.05;    // 4 horas o menos
        if (avgResponseMinutes <= 1440) return 0.10;   // 24 horas o menos
        return 0.20; // Más de 24 horas
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadius = 6371; // Earth's radius in kilometers

        var lat1Rad = lat1 * Math.PI / 180;
        var lat2Rad = lat2 * Math.PI / 180;
        var deltaLatRad = (lat2 - lat1) * Math.PI / 180;
        var deltaLonRad = (lon2 - lon1) * Math.PI / 180;

        var a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadius * c;
    }

    public async Task<List<MusicianMatchResult>> GetRecommendedMusiciansAsync(Guid userId, int limit = 5)
    {
        var user = await _musicianRepository.GetUserByIdAsync(userId);
        if (user?.MusicianProfile == null)
            return new List<MusicianMatchResult>();

        // Obtener solicitudes recientes del usuario
        var recentRequests = await _requestRepository.GetUserRequestsAsync(userId, limit: 10);
        
        if (!recentRequests.Any())
            return new List<MusicianMatchResult>();

        // Calcular músicos recomendados basados en solicitudes previas
        var recommendedMusicians = new Dictionary<Guid, double>();

        foreach (var request in recentRequests)
        {
            var matches = await FindMatchingMusiciansAsync(request, limit: 5);
            
            foreach (var match in matches)
            {
                if (recommendedMusicians.ContainsKey(match.MusicianId))
                    recommendedMusicians[match.MusicianId] += match.MatchScore;
                else
                    recommendedMusicians[match.MusicianId] = match.MatchScore;
            }
        }

        // Ordenar y retornar top músicos
        return recommendedMusicians
            .OrderByDescending(kvp => kvp.Value)
            .Take(limit)
            .Select(kvp => new MusicianMatchResult
            {
                MusicianId = kvp.Key,
                MatchScore = kvp.Value / recentRequests.Count // Promedio
            })
            .ToList();
    }

    public async Task<List<MusicianRequest>> GetRecommendedRequestsAsync(Guid musicianId, int limit = 5)
    {
        var musician = await _musicianRepository.GetMusicianProfileAsync(musicianId);
        if (musician == null)
            return new List<MusicianRequest>();

        // Obtener solicitudes que coincidan con el perfil del músico
        var matchingRequests = await _requestRepository.GetMatchingRequestsAsync(
            musician.Instruments, musician.Genres, musician.Location, 
            musician.Latitude, musician.Longitude, limit: 20);

        var scoredRequests = new List<(MusicianRequest Request, double Score)>();

        foreach (var request in matchingRequests)
        {
            var score = await CalculateMatchScoreAsync(musician, request);
            scoredRequests.Add((request, score));
        }

        // Ordenar por score y retornar top solicitudes
        return scoredRequests
            .OrderByDescending(x => x.Score)
            .Take(limit)
            .Select(x => x.Request)
            .ToList();
    }
}

public class MusicianMatchResult
{
    public Guid MusicianId { get; set; }
    public string MusicianName { get; set; } = string.Empty;
    public double MatchScore { get; set; }
    public bool InstrumentMatch { get; set; }
    public bool GenreMatch { get; set; }
    public double LocationMatch { get; set; }
    public double RatingMatch { get; set; }
    public double ExperienceMatch { get; set; }
    public double AvailabilityMatch { get; set; }
    public double BudgetMatch { get; set; }
    public double ResponseTimeMatch { get; set; }
    public double RecentCancellationsPenalty { get; set; }
}

public class MatchScoreWeights
{
    public double Instrument { get; set; } = 0.25;
    public double Genre { get; set; } = 0.20;
    public double Location { get; set; } = 0.15;
    public double Rating { get; set; } = 0.15;
    public double Experience { get; set; } = 0.10;
    public double Availability { get; set; } = 0.10;
    public double Budget { get; set; } = 0.05;
}
```

---

## 📝 **Sistema de Aplicaciones y Propuestas**

### **Servicio de Aplicaciones**

```csharp
// MusicalMatching.Application/Services/MusicianApplicationService.cs
namespace MusicalMatching.Application.Services;

public interface IMusicianApplicationService
{
    Task<MusicianRequestApplication> ApplyToRequestAsync(CreateApplicationDto dto);
    Task<bool> WithdrawApplicationAsync(Guid applicationId, Guid musicianId);
    Task<MusicianRequestApplication> AcceptApplicationAsync(Guid applicationId, Guid clientId);
    Task<MusicianRequestApplication> RejectApplicationAsync(Guid applicationId, Guid clientId, string reason);
    Task<List<MusicianRequestApplication>> GetMusicianApplicationsAsync(Guid musicianId);
    Task<List<MusicianRequestApplication>> GetRequestApplicationsAsync(Guid requestId);
}

public class MusicianApplicationService : IMusicianApplicationService
{
    private readonly IMusicianRequestApplicationRepository _applicationRepository;
    private readonly IMusicianRequestRepository _requestRepository;
    private readonly IMusicianProfileRepository _musicianRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<MusicianApplicationService> _logger;

    public MusicianApplicationService(
        IMusicianRequestApplicationRepository applicationRepository,
        IMusicianRequestRepository requestRepository,
        IMusicianProfileRepository musicianRepository,
        INotificationService notificationService,
        ILogger<MusicianApplicationService> logger)
    {
        _applicationRepository = applicationRepository;
        _requestRepository = requestRepository;
        _musicianRepository = musicianRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<MusicianRequestApplication> ApplyToRequestAsync(CreateApplicationDto dto)
    {
        _logger.LogInformation("Musician {MusicianId} applying to request {RequestId}", 
            dto.MusicianId, dto.MusicianRequestId);

        // Validar que la solicitud esté abierta
        var request = await _requestRepository.GetByIdAsync(dto.MusicianRequestId);
        if (request == null)
            throw new NotFoundException("Musician request not found");

        if (request.Status != MusicianRequestStatus.Open)
            throw new DomainException("Cannot apply to non-open request");

        if (request.IsExpired())
            throw new DomainException("Request has expired");

        // Validar que el músico no haya aplicado antes
        var existingApplication = await _applicationRepository.GetByMusicianAndRequestAsync(
            dto.MusicianId, dto.MusicianRequestId);

        if (existingApplication != null)
            throw new DomainException("Musician has already applied to this request");

        // Validar que el músico esté disponible
        var musician = await _musicianRepository.GetMusicianProfileAsync(dto.MusicianId);
        if (musician == null)
            throw new NotFoundException("Musician profile not found");

        if (!musician.IsAvailable)
            throw new DomainException("Musician is not currently available");

        // Crear la aplicación
        var application = new MusicianRequestApplication(
            dto.MusicianRequestId, dto.MusicianId, dto.ProposedRate,
            dto.Proposal, dto.EstimatedResponseTime,
            dto.CanProvideEquipment, dto.EquipmentDetails);

        await _applicationRepository.AddAsync(application);

        // Notificar al cliente
        await _notificationService.NotifyNewApplicationAsync(request.CreatedById, application);

        _logger.LogInformation("Application {ApplicationId} created successfully", application.Id);

        return application;
    }

    public async Task<bool> WithdrawApplicationAsync(Guid applicationId, Guid musicianId)
    {
        var application = await _applicationRepository.GetByIdAsync(applicationId);
        if (application == null)
            throw new NotFoundException("Application not found");

        if (application.MusicianId != musicianId)
            throw new UnauthorizedException("Cannot withdraw another musician's application");

        if (application.Status != ApplicationStatus.Pending)
            throw new DomainException("Only pending applications can be withdrawn");

        application.Withdraw();
        await _applicationRepository.UpdateAsync(application);

        // Notificar al cliente
        await _notificationService.NotifyApplicationWithdrawnAsync(
            application.MusicianRequest.CreatedById, application);

        _logger.LogInformation("Application {ApplicationId} withdrawn by musician {MusicianId}", 
            applicationId, musicianId);

        return true;
    }

    public async Task<MusicianRequestApplication> AcceptApplicationAsync(Guid applicationId, Guid clientId)
    {
        var application = await _applicationRepository.GetByIdAsync(applicationId);
        if (application == null)
            throw new NotFoundException("Application not found");

        var request = application.MusicianRequest;
        if (request.CreatedById != clientId)
            throw new UnauthorizedException("Only the request creator can accept applications");

        if (request.Status != MusicianRequestStatus.Open)
            throw new DomainException("Cannot accept application for non-open request");

        if (application.Status != ApplicationStatus.Pending)
            throw new DomainException("Only pending applications can be accepted");

        // Aceptar la aplicación
        application.Accept();
        await _applicationRepository.UpdateAsync(application);

        // Asignar el músico a la solicitud
        request.AssignMusician(application.MusicianId);
        await _requestRepository.UpdateAsync(request);

        // Rechazar todas las otras aplicaciones
        var otherApplications = await _applicationRepository.GetByRequestAsync(request.Id);
        foreach (var otherApp in otherApplications.Where(a => a.Id != applicationId))
        {
            otherApp.Reject("Another application was accepted");
            await _applicationRepository.UpdateAsync(otherApp);
        }

        // Notificar al músico
        await _notificationService.NotifyApplicationAcceptedAsync(application.MusicianId, application);

        _logger.LogInformation("Application {ApplicationId} accepted for request {RequestId}", 
            applicationId, request.Id);

        return application;
    }

    public async Task<MusicianRequestApplication> RejectApplicationAsync(
        Guid applicationId, Guid clientId, string reason)
    {
        var application = await _applicationRepository.GetByIdAsync(applicationId);
        if (application == null)
            throw new NotFoundException("Application not found");

        var request = application.MusicianRequest;
        if (request.CreatedById != clientId)
            throw new UnauthorizedException("Only the request creator can reject applications");

        if (application.Status != ApplicationStatus.Pending)
            throw new DomainException("Only pending applications can be rejected");

        application.Reject(reason);
        await _applicationRepository.UpdateAsync(application);

        // Notificar al músico
        await _notificationService.NotifyApplicationRejectedAsync(application.MusicianId, application, reason);

        _logger.LogInformation("Application {ApplicationId} rejected for request {RequestId}", 
            applicationId, request.Id);

        return application;
    }

    public async Task<List<MusicianRequestApplication>> GetMusicianApplicationsAsync(Guid musicianId)
    {
        return await _applicationRepository.GetByMusicianAsync(musicianId);
    }

    public async Task<List<MusicianRequestApplication>> GetRequestApplicationsAsync(Guid requestId)
    {
        return await _applicationRepository.GetByRequestAsync(requestId);
    }
}

public class CreateApplicationDto
{
    public Guid MusicianRequestId { get; set; }
    public Guid MusicianId { get; set; }
    public decimal ProposedRate { get; set; }
    public string Proposal { get; set; } = string.Empty;
    public TimeSpan EstimatedResponseTime { get; set; }
    public bool CanProvideEquipment { get; set; }
    public string EquipmentDetails { get; set; } = string.Empty;
}
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Algoritmo de Matching**
```csharp
// Implementa:
// - Cálculo de scores de matching
// - Filtros por ubicación y disponibilidad
// - Sistema de recomendaciones
// - Cache de resultados
```

### **Ejercicio 2: Sistema de Aplicaciones**
```csharp
// Crea:
// - Aplicaciones a solicitudes
// - Aceptación/rechazo de propuestas
// - Notificaciones automáticas
// - Gestión de estado
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **🎯 Algoritmo de Matching**: Sistema inteligente de emparejamiento
2. **📝 Aplicaciones**: Gestión de propuestas de músicos
3. **⚖️ Cálculo de Scores**: Métricas de compatibilidad
4. **🔍 Filtros Avanzados**: Búsqueda por múltiples criterios
5. **📊 Recomendaciones**: Sistema de sugerencias personalizadas

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Sistema de Mensajería y Chat**, implementando la comunicación entre usuarios.

---

**¡Has completado la tercera clase del Módulo 16! 🎯🎵**


