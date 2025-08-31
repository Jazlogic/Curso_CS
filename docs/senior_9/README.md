# 🏆 Senior Level 9: Maestría Total y Liderazgo Técnico

## 🧭 Navegación del Curso

- **⬅️ Anterior**: [Módulo 15: Sistemas Avanzados](../senior_8/README.md)
- **➡️ Siguiente**: [🏠 Página Principal](../README.md)
- **📚 [Índice Completo](../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../NAVEGACION_RAPIDA.md)**

---

# 🎵 **Senior Level 9: Implementación Específica de MussikOn**

## 🎯 **Objetivo del Nivel**
Implementar **exactamente** la plataforma **MussikOn** - un sistema completo de matching musical que conecte músicos profesionales con organizadores de eventos, incluyendo toda la lógica de negocio, entidades y funcionalidades específicas.

---

## 📚 **Contenido Teórico**

### **🏗️ Arquitectura Específica de MussikOn**

#### **Estructura del Dominio Musical**
```csharp
// MusicalMatching.Domain/Entities/
public class User : BaseEntity
{
    public string Email { get; set; }
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public UserRole Role { get; set; }
    public UserType UserType { get; set; }
    public bool IsVerified { get; set; }
    public SubscriptionTier SubscriptionTier { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
    
    // Navegación
    public virtual MusicianProfile MusicianProfile { get; set; }
    public virtual List<MusicianRequest> CreatedRequests { get; set; }
    public virtual List<MusicianRequest> AssignedRequests { get; set; }
    public virtual List<Review> GivenReviews { get; set; }
    public virtual List<Review> ReceivedReviews { get; set; }
}

public class MusicianProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public virtual User User { get; set; }
    
    // Información musical
    public List<string> Instruments { get; set; } = new();
    public List<string> Genres { get; set; } = new();
    public string Bio { get; set; }
    public int YearsOfExperience { get; set; }
    public decimal HourlyRate { get; set; }
    public string Location { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    // Disponibilidad
    public bool IsAvailable { get; set; }
    public List<AvailabilitySlot> AvailabilitySlots { get; set; } = new();
    
    // Calificaciones
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int RecentCancellations { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
    
    // Portafolio
    public List<string> PortfolioUrls { get; set; } = new();
    public List<string> Certifications { get; set; } = new();
    public List<string> Languages { get; set; } = new();
}

public class MusicianRequest : BaseEntity
{
    // Información del evento
    public string EventType { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
    public TimeSpan Duration { get; set; }
    public string Location { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    // Requisitos musicales
    public string Instrument { get; set; }
    public List<string> AdditionalInstruments { get; set; } = new();
    public string Genre { get; set; }
    public string Style { get; set; }
    
    // Presupuesto y términos
    public decimal Budget { get; set; }
    public decimal MinBudget { get; set; }
    public decimal MaxBudget { get; set; }
    public bool IsNegotiable { get; set; }
    
    // Detalles del evento
    public string EventDescription { get; set; }
    public int ExpectedAttendees { get; set; }
    public string DressCode { get; set; }
    public bool RequiresEquipment { get; set; }
    public string EquipmentDetails { get; set; }
    
    // Estado y asignación
    public RequestStatus Status { get; set; }
    public Guid? AssignedMusicianId { get; set; }
    public virtual MusicianProfile AssignedMusician { get; set; }
    public DateTime? AssignedAt { get; set; }
    
    // Organizador
    public Guid OrganizerId { get; set; }
    public virtual User Organizer { get; set; }
    
    // Aplicaciones de músicos
    public virtual List<MusicianApplication> Applications { get; set; } = new();
    
    // Auditoría
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
}

public class MusicianApplication : BaseEntity
{
    public Guid MusicianId { get; set; }
    public virtual MusicianProfile Musician { get; set; }
    
    public Guid RequestId { get; set; }
    public virtual MusicianRequest Request { get; set; }
    
    // Propuesta del músico
    public decimal ProposedRate { get; set; }
    public string Proposal { get; set; }
    public List<string> AvailableInstruments { get; set; } = new();
    public bool CanProvideEquipment { get; set; }
    public string EquipmentDetails { get; set; }
    
    // Estado de la aplicación
    public ApplicationStatus Status { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public string RejectionReason { get; set; }
    
    // Calificación del match
    public decimal MatchScore { get; set; }
    public string MatchReason { get; set; }
}
```

#### **Enums y Estados del Sistema**
```csharp
public enum UserRole
{
    SuperAdmin,      // Acceso total al sistema
    Admin,           // Administración general
    Moderador,       // Moderación de contenido
    Organizador,     // Creación y gestión de solicitudes
    Musico,          // Músico profesional
    EventCreator,    // Creador de eventos
    Musician,        // Músico registrado
    User             // Usuario básico
}

public enum UserType
{
    Individual,      // Persona individual
    Professional,    // Profesional independiente
    Company,         // Empresa o corporación
    Agency,          // Agencia musical
    Venue,           // Venue o establecimiento
    EventPlanner     // Planificador de eventos
}

public enum RequestStatus
{
    Draft,          // Borrador
    Pending,        // Pendiente de revisión
    Active,         // Activa y visible para músicos
    InReview,       // En revisión por organizador
    Assigned,       // Asignada a un músico
    InProgress,     // Evento en progreso
    Completed,      // Evento completado
    Cancelled,      // Cancelada
    Expired         // Expirada
}

public enum ApplicationStatus
{
    Pending,        // Pendiente de revisión
    Accepted,       // Aceptada por organizador
    Rejected,       // Rechazada
    Withdrawn,      // Retirada por músico
    Expired         // Expirada
}

public enum SubscriptionTier
{
    Free,           // Hasta 10 solicitudes/mes
    Basic,          // 100 solicitudes/mes
    Professional,   // Solicitudes ilimitadas
    Enterprise      // White-label y soporte dedicado
}

public enum EventType
{
    Boda,           // Ceremonias de boda
    Cumpleaños,     // Celebraciones de cumpleaños
    Corporativo,    // Eventos empresariales
    Concierto,      // Presentaciones musicales
    Festival,       // Festivales musicales
    Culto,          // Servicios religiosos
    Graduación,     // Ceremonias de graduación
    Aniversario,    // Celebraciones de aniversario
    Bar,            // Bares y restaurantes
    Club,           // Clubes nocturnos
    Teatro,         // Teatros y auditorios
    Exterior,       // Eventos al aire libre
    Otro            // Otros tipos de eventos
}
```

### **🎵 Lógica de Negocio Core**

#### **Servicio de Matching Musical Avanzado**
```csharp
public class AdvancedMusicianMatchingService : IMusicianMatchingService
{
    private readonly IMusicianRepository _musicianRepository;
    private readonly ILocationService _locationService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<AdvancedMusicianMatchingService> _logger;
    private readonly IMatchingConfigurationService _configService;

    public AdvancedMusicianMatchingService(
        IMusicianRepository musicianRepository,
        ILocationService locationService,
        ICacheService cacheService,
        ILogger<AdvancedMusicianMatchingService> logger,
        IMatchingConfigurationService configService)
    {
        _musicianRepository = musicianRepository;
        _locationService = locationService;
        _cacheService = cacheService;
        _logger = logger;
        _configService = configService;
    }

    public async Task<List<MusicianMatch>> FindOptimalMatchesAsync(MusicianRequest request, int maxResults = 20)
    {
        var cacheKey = $"matches_{request.Id}_{request.UpdatedAt:yyyyMMddHHmmss}";
        
        // Intentar obtener del cache
        var cachedMatches = await _cacheService.GetAsync<List<MusicianMatch>>(cacheKey);
        if (cachedMatches != null)
        {
            _logger.LogInformation("Returning cached matches for request {RequestId}", request.Id);
            return cachedMatches.Take(maxResults).ToList();
        }

        // Obtener músicos disponibles
        var availableMusicians = await _musicianRepository.GetAvailableMusiciansAsync(
            request.Instrument, 
            request.Date, 
            request.Location,
            request.Budget);

        var matches = new List<MusicianMatch>();

        foreach (var musician in availableMusicians)
        {
            var match = await CalculateComprehensiveMatchAsync(musician, request);
            
            if (match.Score >= _configService.GetMinimumMatchScore())
            {
                matches.Add(match);
            }
        }

        // Ordenar por score y aplicar filtros adicionales
        var sortedMatches = matches
            .OrderByDescending(m => m.Score)
            .ThenBy(m => m.Distance)
            .ThenByDescending(m => m.Musician.AverageRating)
            .Take(maxResults)
            .ToList();

        // Cachear resultados por 15 minutos
        await _cacheService.SetAsync(cacheKey, sortedMatches, TimeSpan.FromMinutes(15));

        _logger.LogInformation("Found {MatchCount} matches for request {RequestId}", sortedMatches.Count, request.Id);
        return sortedMatches;
    }

    private async Task<MusicianMatch> CalculateComprehensiveMatchAsync(MusicianProfile musician, MusicianRequest request)
    {
        var match = new MusicianMatch
        {
            Musician = musician,
            Request = request,
            CalculatedAt = DateTime.UtcNow
        };

        // Score por instrumento (35 puntos)
        match.InstrumentScore = CalculateInstrumentScore(musician, request);
        
        // Score por ubicación (25 puntos)
        match.LocationScore = await CalculateLocationScoreAsync(musician, request);
        
        // Score por presupuesto (20 puntos)
        match.BudgetScore = CalculateBudgetScore(musician, request);
        
        // Score por experiencia y reputación (15 puntos)
        match.ReputationScore = CalculateReputationScore(musician);
        
        // Score por disponibilidad (5 puntos)
        match.AvailabilityScore = CalculateAvailabilityScore(musician, request);

        // Calcular score total
        match.Score = match.InstrumentScore + match.LocationScore + match.BudgetScore + 
                     match.ReputationScore + match.AvailabilityScore;

        // Aplicar penalizaciones
        var penalties = CalculatePenalties(musician, request);
        match.Score = Math.Max(0, match.Score - penalties);

        // Generar razón del match
        match.MatchReason = GenerateMatchReason(match);

        return match;
    }

    private decimal CalculateInstrumentScore(MusicianProfile musician, MusicianRequest request)
    {
        decimal score = 0;

        // Instrumento principal (25 puntos)
        if (musician.Instruments.Contains(request.Instrument, StringComparer.OrdinalIgnoreCase))
        {
            score += 25;
        }

        // Instrumentos adicionales (5 puntos)
        var additionalMatches = musician.Instruments
            .Intersect(request.AdditionalInstruments, StringComparer.OrdinalIgnoreCase)
            .Count();
        score += Math.Min(additionalMatches * 2.5m, 5);

        // Género musical (5 puntos)
        if (musician.Genres.Contains(request.Genre, StringComparer.OrdinalIgnoreCase))
        {
            score += 5;
        }

        return score;
    }

    private async Task<decimal> CalculateLocationScoreAsync(MusicianProfile musician, MusicianRequest request)
    {
        var distance = await _locationService.CalculateDistanceAsync(
            musician.Latitude, musician.Longitude,
            request.Latitude, request.Longitude);

        match.Distance = distance;

        // Score basado en distancia
        if (distance <= 5) return 25;      // Misma ciudad (5km)
        if (distance <= 15) return 22;     // Ciudad cercana (15km)
        if (distance <= 30) return 18;     // Área metropolitana (30km)
        if (distance <= 50) return 15;     // Región (50km)
        if (distance <= 100) return 10;    // Estado/Provincia (100km)
        if (distance <= 200) return 5;     // País (200km)
        return 0;                           // Muy lejos
    }

    private decimal CalculateBudgetScore(MusicianProfile musician, MusicianRequest request)
    {
        var estimatedHours = EstimateEventHours(request);
        var totalCost = musician.HourlyRate * estimatedHours;
        
        // Score basado en relación presupuesto/costo
        var budgetRatio = request.Budget / totalCost;
        
        if (budgetRatio >= 1.5m) return 20;      // Muy económico
        if (budgetRatio >= 1.2m) return 18;      // Económico
        if (budgetRatio >= 1.0m) return 15;      // Dentro del presupuesto
        if (budgetRatio >= 0.8m) return 10;      // Ligeramente por encima
        if (budgetRatio >= 0.6m) return 5;       // Por encima pero aceptable
        return 0;                                 // Fuera del presupuesto
    }

    private decimal CalculateReputationScore(MusicianProfile musician)
    {
        var experienceScore = Math.Min(musician.YearsOfExperience * 0.5m, 5);
        var ratingScore = Math.Min(musician.AverageRating * 2, 5);
        var responseScore = CalculateResponseTimeScore(musician.AverageResponseTime);
        var cancellationScore = CalculateCancellationScore(musician.RecentCancellations);
        
        return experienceScore + ratingScore + responseScore + cancellationScore;
    }

    private decimal CalculateAvailabilityScore(MusicianProfile musician, MusicianRequest request)
    {
        if (!musician.IsAvailable) return 0;

        // Verificar si el músico tiene slots disponibles
        var hasAvailability = musician.AvailabilitySlots
            .Any(slot => slot.Date.Date == request.Date.Date && 
                        slot.StartTime <= request.Time && 
                        slot.EndTime >= request.Time.Add(request.Duration));

        return hasAvailability ? 5 : 0;
    }

    private decimal CalculatePenalties(MusicianProfile musician, MusicianRequest request)
    {
        decimal penalties = 0;

        // Penalización por cancelaciones recientes
        if (musician.RecentCancellations > 0)
        {
            penalties += Math.Min(musician.RecentCancellations * 3, 15);
        }

        // Penalización por respuesta lenta
        if (musician.AverageResponseTime > TimeSpan.FromHours(4))
        {
            penalties += 10;
        }

        // Penalización por conflictos de calendario
        if (HasCalendarConflicts(musician, request))
        {
            penalties += 20;
        }

        // Penalización por baja calificación
        if (musician.AverageRating < 3.5)
        {
            penalties += 15;
        }

        return penalties;
    }

    private string GenerateMatchReason(MusicianMatch match)
    {
        var reasons = new List<string>();

        if (match.InstrumentScore >= 25)
            reasons.Add("Instrumento perfecto");

        if (match.LocationScore >= 20)
            reasons.Add("Ubicación ideal");

        if (match.BudgetScore >= 15)
            reasons.Add("Presupuesto compatible");

        if (match.ReputationScore >= 10)
            reasons.Add("Excelente reputación");

        if (match.AvailabilityScore >= 5)
            reasons.Add("Disponible en la fecha");

        return string.Join(", ", reasons);
    }
}
```

#### **Sistema de Notificaciones en Tiempo Real**
```csharp
public class MussikOnNotificationHub : Hub
{
    private readonly IMusicianRequestService _requestService;
    private readonly IUserService _userService;
    private readonly ILogger<MussikOnNotificationHub> _logger;
    private readonly IMetricsService _metricsService;

    public MussikOnNotificationHub(
        IMusicianRequestService requestService,
        IUserService userService,
        ILogger<MussikOnNotificationHub> logger,
        IMetricsService metricsService)
    {
        _requestService = requestService;
        _userService = userService;
        _logger = logger;
        _metricsService = metricsService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserIdFromContext();
        if (userId.HasValue)
        {
            var user = await _userService.GetByIdAsync(userId.Value);
            await JoinUserGroupsAsync(user);
            
            _logger.LogInformation("User {UserId} connected to hub", userId);
            _metricsService.IncrementActiveConnections();
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = GetUserIdFromContext();
        if (userId.HasValue)
        {
            _logger.LogInformation("User {UserId} disconnected from hub", userId);
            _metricsService.DecrementActiveConnections();
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinMusicianGroups(string instrument, string location)
    {
        var userId = GetUserIdFromContext();
        if (!userId.HasValue) return;

        var groupName = $"musicians_{instrument}_{location}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserId} joined musician group {Group}", userId, groupName);
    }

    public async Task JoinOrganizerGroups(Guid organizerId)
    {
        var groupName = $"organizer_{organizerId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User joined organizer group {Group}", groupName);
    }

    public async Task SendTypingIndicator(string requestId, bool isTyping)
    {
        var userId = GetUserIdFromContext();
        if (!userId.HasValue) return;

        var message = new
        {
            Type = "TypingIndicator",
            RequestId = requestId,
            UserId = userId,
            IsTyping = isTyping,
            Timestamp = DateTime.UtcNow
        };

        await Clients.Group($"chat_{requestId}").SendAsync("ReceiveTypingIndicator", message);
    }

    public async Task SendMessage(string requestId, string content)
    {
        var userId = GetUserIdFromContext();
        if (!userId.HasValue) return;

        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            RequestId = Guid.Parse(requestId),
            SenderId = userId.Value,
            Content = content,
            Timestamp = DateTime.UtcNow,
            MessageType = MessageType.Text
        };

        // Persistir mensaje en base de datos
        await _requestService.AddChatMessageAsync(message);

        // Enviar a todos los participantes del chat
        var notification = new
        {
            Type = "NewMessage",
            MessageId = message.Id,
            RequestId = message.RequestId,
            SenderId = message.SenderId,
            Content = message.Content,
            Timestamp = message.Timestamp
        };

        await Clients.Group($"chat_{requestId}").SendAsync("ReceiveMessage", notification);
    }

    private Guid? GetUserIdFromContext()
    {
        var userIdClaim = Context.User?.FindFirst("UserId");
        return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : null;
    }

    private async Task JoinUserGroupsAsync(User user)
    {
        // Unirse a grupos según el rol del usuario
        switch (user.Role)
        {
            case UserRole.Musico:
                var musicianProfile = await _userService.GetMusicianProfileAsync(user.Id);
                if (musicianProfile != null)
                {
                    foreach (var instrument in musicianProfile.Instruments)
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, $"musicians_{instrument}");
                    }
                }
                break;

            case UserRole.Organizador:
                await Groups.AddToGroupAsync(Context.ConnectionId, $"organizer_{user.Id}");
                break;

            case UserRole.Admin:
            case UserRole.SuperAdmin:
                await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
                break;
        }
    }
}
```

### **🔐 Sistema de Autenticación y Autorización Específico**

#### **JWT Service para MussikOn**
```csharp
public class MussikOnJwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IUserService _userService;
    private readonly ILogger<MussikOnJwtService> _logger;

    public MussikOnJwtService(
        IOptions<JwtSettings> jwtSettings,
        IUserService userService,
        ILogger<MussikOnJwtService> logger)
    {
        _jwtSettings = jwtSettings.Value;
        _userService = userService;
        _logger = logger;
    }

    public async Task<JwtTokenResponse> GenerateTokensAsync(User user)
    {
        var userPermissions = await _userService.GetUserPermissionsAsync(user.Id);
        var userPreferences = await _userService.GetUserPreferencesAsync(user.Id);
        
        var claims = new List<Claim>
        {
            // Claims estándar
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            
            // Claims personalizados de MussikOn
            new Claim("UserId", user.Id.ToString()),
            new Claim("UserType", user.UserType.ToString()),
            new Claim("IsVerified", user.IsVerified.ToString().ToLower()),
            new Claim("SubscriptionTier", user.SubscriptionTier.ToString()),
            new Claim("CreatedAt", user.CreatedAt.ToString("yyyy-MM-dd")),
            new Claim("LastLoginAt", user.LastLoginAt.ToString("yyyy-MM-dd HH:mm:ss")),
            
            // Claims de ubicación (si aplica)
            new Claim("Location", user.Location ?? ""),
            new Claim("Timezone", user.Timezone ?? "UTC"),
            
            // Claims de preferencias
            new Claim("Language", userPreferences.Language ?? "es"),
            new Claim("Currency", userPreferences.Currency ?? "USD"),
            new Claim("NotificationsEnabled", userPreferences.NotificationsEnabled.ToString().ToLower())
        };

        // Agregar permisos como claims
        foreach (var permission in userPermissions)
        {
            claims.Add(new Claim("Permission", permission));
        }

        // Claims específicos del rol
        if (user.Role == UserRole.Musico)
        {
            var musicianProfile = await _userService.GetMusicianProfileAsync(user.Id);
            if (musicianProfile != null)
            {
                claims.Add(new Claim("Instruments", string.Join(",", musicianProfile.Instruments)));
                claims.Add(new Claim("Genres", string.Join(",", musicianProfile.Genres)));
                claims.Add(new Claim("HourlyRate", musicianProfile.HourlyRate.ToString()));
                claims.Add(new Claim("IsAvailable", musicianProfile.IsAvailable.ToString().ToLower()));
            }
        }

        var accessToken = GenerateAccessToken(claims);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id);

        _logger.LogInformation("Generated tokens for user {UserId} with role {Role}", user.Id, user.Role);

        return new JwtTokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = _jwtSettings.ExpirationMinutes * 60,
            TokenType = "Bearer"
        };
    }

    private string GenerateAccessToken(List<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials,
            notBefore: DateTime.UtcNow
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<JwtTokenResponse> RefreshTokenAsync(string refreshToken)
    {
        var storedRefreshToken = await _userService.GetRefreshTokenAsync(refreshToken);
        if (storedRefreshToken == null || storedRefreshToken.IsExpired)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token");
        }

        var user = await _userService.GetByIdAsync(storedRefreshToken.UserId);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        // Generar nuevos tokens
        var newTokens = await GenerateTokensAsync(user);

        // Invalidar el refresh token anterior
        await _userService.InvalidateRefreshTokenAsync(refreshToken);

        _logger.LogInformation("Refreshed tokens for user {UserId}", user.Id);

        return newTokens;
    }

    public ClaimsPrincipal ValidateToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            throw new UnauthorizedAccessException("Invalid token");
        }
    }
}
```

---

## 🧪 **Ejercicios Prácticos**

### **Ejercicio 1: Implementar Entidades del Dominio Musical**
```csharp
// Crea las entidades completas:
// - User con roles y tipos específicos
// - MusicianProfile con información musical detallada
// - MusicianRequest con todos los campos del evento
// - MusicianApplication con propuestas y scoring
// - Review y Rating system
```

### **Ejercicio 2: Algoritmo de Matching Musical Avanzado**
```csharp
// Implementa el algoritmo que:
// - Considere múltiples criterios de scoring
// - Use machine learning para mejorar matches
// - Implemente filtros por ubicación, presupuesto, género
// - Genere razones detalladas del match
```

### **Ejercicio 3: Sistema de Notificaciones en Tiempo Real**
```csharp
// Crea el sistema SignalR que:
// - Notifique a músicos de nuevas solicitudes
// - Permita chat en tiempo real entre usuarios
// - Maneje grupos por instrumento y ubicación
// - Persista mensajes en base de datos
```

### **Ejercicio 4: Sistema de Estados y Workflow**
```csharp
// Implementa el workflow completo:
// - Estados de solicitudes con transiciones válidas
// - Estados de aplicaciones de músicos
// - Notificaciones automáticas en cambios de estado
// - Auditoría completa de cambios
```

### **Ejercicio 5: Sistema de Calificaciones y Reviews**
```csharp
// Crea el sistema que:
// - Permita calificar eventos completados
// - Genere reviews detallados
// - Calcule ratings promedio
// - Implemente sistema de reputación
```

### **Ejercicio 6: Gestión de Disponibilidad y Calendario**
```csharp
// Implementa:
// - Slots de disponibilidad para músicos
// - Verificación de conflictos de calendario
// - Sistema de reservas y confirmaciones
// - Notificaciones de cambios de disponibilidad
```

### **Ejercicio 7: Sistema de Pagos y Facturación**
```csharp
// Crea el sistema que:
// - Integre con Stripe/PayPal
// - Maneje diferentes tipos de pago
// - Genere facturas automáticas
// - Implemente sistema de comisiones
```

### **Ejercicio 8: Analytics y Reportes**
```csharp
// Implementa:
// - Métricas de matching y éxito
// - Reportes de eventos por tipo/ubicación
// - Analytics de usuarios y engagement
// - Dashboard para administradores
```

### **Ejercicio 9: Sistema de Moderación y Seguridad**
```csharp
// Crea el sistema que:
// - Valide contenido de solicitudes
// - Implemente sistema de reportes
// - Maneje disputas entre usuarios
// - Implemente medidas anti-fraude
```

### **Ejercicio 10: Proyecto Integrador: Plataforma MussikOn Completa**
```csharp
// Construye la plataforma completa que incluya:
// - Todas las entidades y lógica de negocio
// - Sistema de matching avanzado
// - Comunicación en tiempo real
// - Autenticación y autorización robusta
// - Testing completo y documentación
// - Deployment en producción
```

---

## 📊 **Proyecto Integrador: Plataforma MussikOn Completa**

### **🎯 Objetivo**
Construir **exactamente** la plataforma **MussikOn** - un sistema completo de matching musical que conecte músicos profesionales con organizadores de eventos.

### **🏗️ Arquitectura del Proyecto**
```
MussikOn.API/
├── Controllers/                    # API endpoints
├── Middleware/                     # Middleware personalizado
├── DTOs/                          # Data Transfer Objects
├── Extensions/                     # Configuración de servicios
├── Program.cs                      # Punto de entrada
└── appsettings.json               # Configuración

MussikOn.Domain/
├── Entities/                       # Entidades del dominio musical
├── Interfaces/                     # Contratos del dominio
├── Services/                       # Servicios de dominio
├── Events/                         # Eventos de dominio
├── ValueObjects/                   # Objetos de valor
└── Exceptions/                     # Excepciones personalizadas

MussikOn.Application/
├── Services/                       # Servicios de aplicación
├── Validators/                     # Validadores con FluentValidation
├── AutoMapper/                     # Perfiles de mapeo
├── Commands/                       # Comandos CQRS
├── Queries/                        # Consultas CQRS
└── Handlers/                       # Manejadores CQRS

MussikOn.Infrastructure/
├── Data/                           # Acceso a datos
├── Repositories/                   # Implementaciones de repositorios
├── External/                       # Servicios externos (Stripe, etc.)
├── Configuration/                  # Configuración de servicios
└── Hubs/                           # SignalR Hubs
```

### **📋 Funcionalidades Core de MussikOn**
1. **Sistema de Usuarios**: Roles granulares, verificación, suscripciones
2. **Perfiles de Músicos**: Instrumentos, géneros, experiencia, portafolio
3. **Solicitudes de Eventos**: Tipos de evento, requisitos, presupuesto
4. **Matching Inteligente**: Algoritmo de scoring avanzado
5. **Chat en Tiempo Real**: Comunicación entre usuarios
6. **Sistema de Estados**: Workflow completo de solicitudes
7. **Calificaciones y Reviews**: Sistema de reputación
8. **Gestión de Calendario**: Disponibilidad y conflictos
9. **Sistema de Pagos**: Integración con gateways de pago
10. **Analytics y Reportes**: Métricas de negocio

### **🔧 Tecnologías Específicas**
- **.NET 8.0** + **ASP.NET Core Web API**
- **Entity Framework Core** + **SQL Server/PostgreSQL**
- **SignalR** para comunicación en tiempo real
- **JWT** + **ASP.NET Identity** para autenticación
- **FluentValidation** + **AutoMapper**
- **xUnit** + **Moq** para testing
- **Swagger/OpenAPI** para documentación
- **Redis** para caching y SignalR
- **Stripe** para pagos
- **Azure/AWS** para deployment

### **📈 Métricas de Éxito de MussikOn**
- **Tiempo de Matching**: < 30 segundos
- **Tasa de Éxito**: > 80% de solicitudes asignadas
- **Response Time**: < 200ms para APIs
- **Uptime**: 99.9%+
- **Cobertura de Testing**: 85%+
- **Performance**: 1000+ requests/second
- **Seguridad**: OWASP Top 10 compliance

---

## 🎯 **Evaluación y Certificación**

### **📝 Autoevaluación**
- [ ] ¿Puedes implementar todas las entidades del dominio musical?
- [ ] ¿Entiendes la lógica de negocio específica de MussikOn?
- [ ] ¿Puedes crear el algoritmo de matching avanzado?
- [ ] ¿Sabes implementar el sistema de notificaciones en tiempo real?
- [ ] ¿Puedes diseñar el workflow completo de solicitudes?
- [ ] ¿Entiendes cómo implementar el sistema de calificaciones?
- [ ] ¿Puedes gestionar disponibilidad y calendario?
- [ ] ¿Sabes integrar sistemas de pago?
- [ ] ¿Puedes crear analytics y reportes?
- [ ] ¿Has construido la plataforma MussikOn completa?

### **🏆 Criterios de Aprobación**
- **Implementación completa** de la plataforma MussikOn
- **Lógica de negocio** implementada correctamente
- **Sistema de matching** funcionando con algoritmos avanzados
- **Comunicación en tiempo real** implementada
- **Testing exhaustivo** con cobertura >85%
- **Documentación completa** de la API
- **Deployment exitoso** en producción

---

## 🚀 **Próximos Pasos**

1. **Implementa** la plataforma MussikOn paso a paso
2. **Practica** con los ejercicios específicos del dominio musical
3. **Optimiza** el algoritmo de matching
4. **Implementa** todas las funcionalidades core
5. **Despliega** en producción y valida el funcionamiento

---

## 📚 **Recursos Adicionales**

### **🔗 Enlaces Útiles**
- [MussikOn Documentation](https://github.com/your-org/mussikon)
- [SignalR for Real-time Communication](https://docs.microsoft.com/en-us/aspnet/core/signalr/)
- [JWT Best Practices](https://auth0.com/blog/a-look-at-the-latest-draft-for-jwt-bcp/)
- [Clean Architecture in .NET](https://docs.microsoft.com/en-us/dotnet/architecture/)

### **📖 Libros Recomendados**
- "Domain-Driven Design" - Eric Evans
- "Clean Architecture" - Robert C. Martin
- "Building Microservices" - Sam Newman
- "Patterns of Enterprise Application Architecture" - Martin Fowler

---

**¡Ahora tienes todas las habilidades para construir exactamente la plataforma MussikOn! 🎵🚀**
