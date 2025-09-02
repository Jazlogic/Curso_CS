# 🎯 **Clase 10: Proyecto Final - MussikOn Cloud Native**

## 🎯 **Objetivo de la Clase**
Implementar una aplicación cloud native completa para MussikOn, integrando todos los conceptos aprendidos en el módulo de Cloud Native Development.

## 📚 **Contenido Teórico**

### **1. Arquitectura Cloud Native Completa**

#### **Arquitectura de Microservicios**
```csharp
// MussikOn.CloudNative.sln
/*
├── MussikOn.API/                    # API Gateway
├── MussikOn.UserService/            # Microservicio de Usuarios
├── MussikOn.EventService/           # Microservicio de Eventos
├── MussikOn.MatchingService/        # Microservicio de Matching
├── MussikOn.NotificationService/    # Microservicio de Notificaciones
├── MussikOn.PaymentService/         # Microservicio de Pagos
├── MussikOn.AnalyticsService/       # Microservicio de Analytics
├── MussikOn.Shared/                 # Bibliotecas compartidas
├── MussikOn.Infrastructure/         # Infraestructura compartida
└── MussikOn.Tests/                  # Tests de integración
*/
```

#### **Configuración de API Gateway**
```csharp
// MussikOn.API/Program.cs
var builder = WebApplication.CreateBuilder(args);

// Configuración de servicios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuración de Azure
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddAzureKeyVault();

// Configuración de autenticación
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["AzureAD:Authority"];
        options.Audience = builder.Configuration["AzureAD:Audience"];
    });

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("MussikOnPolicy", policy =>
    {
        policy.WithOrigins("https://mussikon.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configuración de servicios
builder.Services.AddSingleton<ServiceBusEventService>();
builder.Services.AddSingleton<BlobStorageService>();
builder.Services.AddSingleton<CosmosDbService>();
builder.Services.AddSingleton<KeyVaultService>();
builder.Services.AddSingleton<ApplicationInsightsService>();
builder.Services.AddSingleton<DistributedTracingService>();
builder.Services.AddSingleton<StructuredLoggingService>();
builder.Services.AddSingleton<HealthCheckService>();
builder.Services.AddSingleton<AlertingService>();

// Configuración de health checks
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    .AddRedis(builder.Configuration.GetConnectionString("Redis"))
    .AddCosmosDb(builder.Configuration.GetConnectionString("CosmosDB"));

var app = builder.Build();

// Configuración de middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("MussikOnPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<DistributedTracingMiddleware>();
app.UseMiddleware<MonitoringMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
```

### **2. Microservicio de Usuarios**

#### **Implementación del Servicio de Usuarios**
```csharp
// MussikOn.UserService/Controllers/UserController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ApplicationInsightsService _appInsightsService;
    private readonly StructuredLoggingService _loggingService;
    private readonly ILogger<UserController> _logger;

    public UserController(
        IUserService userService,
        ApplicationInsightsService appInsightsService,
        StructuredLoggingService loggingService,
        ILogger<UserController> logger)
    {
        _userService = userService;
        _appInsightsService = appInsightsService;
        _loggingService = loggingService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(string id)
    {
        try
        {
            var user = await _userService.GetUserAsync(id);
            
            if (user == null)
            {
                return NotFound();
            }

            _appInsightsService.TrackCustomEvent("UserRetrieved", new Dictionary<string, string>
            {
                { "UserId", id },
                { "UserType", user.UserType.ToString() }
            });

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user: {UserId}", id);
            _appInsightsService.TrackException(ex);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            var user = await _userService.CreateUserAsync(request);
            
            _loggingService.LogUserAction(user.Id, "UserCreated", "User", request);
            _appInsightsService.TrackCustomEvent("UserCreated", new Dictionary<string, string>
            {
                { "UserId", user.Id },
                { "UserType", user.UserType.ToString() }
            });

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            _appInsightsService.TrackException(ex);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<User>> UpdateUser(string id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var user = await _userService.UpdateUserAsync(id, request);
            
            _loggingService.LogUserAction(id, "UserUpdated", "User", request);
            _appInsightsService.TrackCustomEvent("UserUpdated", new Dictionary<string, string>
            {
                { "UserId", id }
            });

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", id);
            _appInsightsService.TrackException(ex);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        try
        {
            await _userService.DeleteUserAsync(id);
            
            _loggingService.LogUserAction(id, "UserDeleted", "User");
            _appInsightsService.TrackCustomEvent("UserDeleted", new Dictionary<string, string>
            {
                { "UserId", id }
            });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", id);
            _appInsightsService.TrackException(ex);
            return StatusCode(500, "Internal server error");
        }
    }
}

// MussikOn.UserService/Services/UserService.cs
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ServiceBusEventService _eventService;
    private readonly BlobStorageService _blobStorageService;
    private readonly EncryptionService _encryptionService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        ServiceBusEventService eventService,
        BlobStorageService blobStorageService,
        EncryptionService encryptionService,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _eventService = eventService;
        _blobStorageService = blobStorageService;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<User> GetUserAsync(string id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            
            if (user != null)
            {
                // Desencriptar datos sensibles
                user.Email = _encryptionService.DecryptSensitiveData(user.Email);
                user.PhoneNumber = _encryptionService.DecryptSensitiveData(user.PhoneNumber);
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user: {UserId}", id);
            throw;
        }
    }

    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = _encryptionService.EncryptSensitiveData(request.Email),
                Name = request.Name,
                PhoneNumber = _encryptionService.EncryptSensitiveData(request.PhoneNumber),
                UserType = request.UserType,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _userRepository.AddAsync(user);

            // Publicar evento
            var userCreatedEvent = new MusicianRegisteredEvent
            {
                MusicianId = user.Id,
                Email = request.Email,
                Name = request.Name,
                Genres = request.Genres ?? new List<string>(),
                Location = request.Location
            };

            await _eventService.PublishEventAsync(userCreatedEvent, "user-registered");

            _logger.LogInformation("User created successfully: {UserId}", user.Id);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            throw;
        }
    }

    public async Task<User> UpdateUserAsync(string id, UpdateUserRequest request)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            
            if (user == null)
            {
                throw new NotFoundException($"User with ID {id} not found");
            }

            user.Name = request.Name;
            user.PhoneNumber = _encryptionService.EncryptSensitiveData(request.PhoneNumber);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("User updated successfully: {UserId}", id);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", id);
            throw;
        }
    }

    public async Task DeleteUserAsync(string id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            
            if (user == null)
            {
                throw new NotFoundException($"User with ID {id} not found");
            }

            user.IsActive = false;
            user.DeletedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("User deleted successfully: {UserId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", id);
            throw;
        }
    }
}
```

### **3. Microservicio de Eventos**

#### **Implementación del Servicio de Eventos**
```csharp
// MussikOn.EventService/Controllers/EventController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly ApplicationInsightsService _appInsightsService;
    private readonly StructuredLoggingService _loggingService;
    private readonly ILogger<EventController> _logger;

    public EventController(
        IEventService eventService,
        ApplicationInsightsService appInsightsService,
        StructuredLoggingService loggingService,
        ILogger<EventController> logger)
    {
        _eventService = eventService;
        _appInsightsService = appInsightsService;
        _loggingService = loggingService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Event>>> GetEvents([FromQuery] EventSearchCriteria criteria)
    {
        try
        {
            var events = await _eventService.SearchEventsAsync(criteria);
            
            _appInsightsService.TrackCustomEvent("EventsSearched", new Dictionary<string, string>
            {
                { "SearchCriteria", JsonSerializer.Serialize(criteria) },
                { "ResultCount", events.Count().ToString() }
            });

            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching events");
            _appInsightsService.TrackException(ex);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Event>> GetEvent(string id)
    {
        try
        {
            var eventEntity = await _eventService.GetEventAsync(id);
            
            if (eventEntity == null)
            {
                return NotFound();
            }

            _appInsightsService.TrackCustomEvent("EventRetrieved", new Dictionary<string, string>
            {
                { "EventId", id }
            });

            return Ok(eventEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event: {EventId}", id);
            _appInsightsService.TrackException(ex);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Event>> CreateEvent([FromBody] CreateEventRequest request)
    {
        try
        {
            var eventEntity = await _eventService.CreateEventAsync(request);
            
            _loggingService.LogUserAction(request.OrganizerId, "EventCreated", "Event", request);
            _appInsightsService.TrackCustomEvent("EventCreated", new Dictionary<string, string>
            {
                { "EventId", eventEntity.Id },
                { "OrganizerId", request.OrganizerId },
                { "Genre", request.Genre }
            });

            return CreatedAtAction(nameof(GetEvent), new { id = eventEntity.Id }, eventEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event");
            _appInsightsService.TrackException(ex);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Event>> UpdateEvent(string id, [FromBody] UpdateEventRequest request)
    {
        try
        {
            var eventEntity = await _eventService.UpdateEventAsync(id, request);
            
            _loggingService.LogUserAction(request.OrganizerId, "EventUpdated", "Event", request);
            _appInsightsService.TrackCustomEvent("EventUpdated", new Dictionary<string, string>
            {
                { "EventId", id }
            });

            return Ok(eventEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event: {EventId}", id);
            _appInsightsService.TrackException(ex);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(string id)
    {
        try
        {
            await _eventService.DeleteEventAsync(id);
            
            _loggingService.LogUserAction(id, "EventDeleted", "Event");
            _appInsightsService.TrackCustomEvent("EventDeleted", new Dictionary<string, string>
            {
                { "EventId", id }
            });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event: {EventId}", id);
            _appInsightsService.TrackException(ex);
            return StatusCode(500, "Internal server error");
        }
    }
}

// MussikOn.EventService/Services/EventService.cs
public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly ServiceBusEventService _eventService;
    private readonly BlobStorageService _blobStorageService;
    private readonly ILogger<EventService> _logger;

    public EventService(
        IEventRepository eventRepository,
        ServiceBusEventService eventService,
        BlobStorageService blobStorageService,
        ILogger<EventService> logger)
    {
        _eventRepository = eventRepository;
        _eventService = eventService;
        _blobStorageService = blobStorageService;
        _logger = logger;
    }

    public async Task<Event> GetEventAsync(string id)
    {
        try
        {
            var eventEntity = await _eventRepository.GetByIdAsync(id);
            return eventEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event: {EventId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Event>> SearchEventsAsync(EventSearchCriteria criteria)
    {
        try
        {
            var events = await _eventRepository.SearchAsync(criteria);
            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching events");
            throw;
        }
    }

    public async Task<Event> CreateEventAsync(CreateEventRequest request)
    {
        try
        {
            var eventEntity = new Event
            {
                Id = Guid.NewGuid().ToString(),
                Title = request.Title,
                Description = request.Description,
                EventDate = request.EventDate,
                Location = request.Location,
                Genre = request.Genre,
                Budget = request.Budget,
                OrganizerId = request.OrganizerId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _eventRepository.AddAsync(eventEntity);

            // Publicar evento
            var eventCreatedEvent = new EventCreatedEvent
            {
                EventId = eventEntity.Id,
                OrganizerId = request.OrganizerId,
                Title = request.Title,
                EventDate = request.EventDate,
                Location = request.Location,
                Genre = request.Genre,
                Budget = request.Budget
            };

            await _eventService.PublishEventAsync(eventCreatedEvent, "event-created");

            _logger.LogInformation("Event created successfully: {EventId}", eventEntity.Id);
            return eventEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event");
            throw;
        }
    }

    public async Task<Event> UpdateEventAsync(string id, UpdateEventRequest request)
    {
        try
        {
            var eventEntity = await _eventRepository.GetByIdAsync(id);
            
            if (eventEntity == null)
            {
                throw new NotFoundException($"Event with ID {id} not found");
            }

            eventEntity.Title = request.Title;
            eventEntity.Description = request.Description;
            eventEntity.EventDate = request.EventDate;
            eventEntity.Location = request.Location;
            eventEntity.Genre = request.Genre;
            eventEntity.Budget = request.Budget;
            eventEntity.UpdatedAt = DateTime.UtcNow;

            await _eventRepository.UpdateAsync(eventEntity);

            _logger.LogInformation("Event updated successfully: {EventId}", id);
            return eventEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event: {EventId}", id);
            throw;
        }
    }

    public async Task DeleteEventAsync(string id)
    {
        try
        {
            var eventEntity = await _eventRepository.GetByIdAsync(id);
            
            if (eventEntity == null)
            {
                throw new NotFoundException($"Event with ID {id} not found");
            }

            eventEntity.IsActive = false;
            eventEntity.DeletedAt = DateTime.UtcNow;

            await _eventRepository.UpdateAsync(eventEntity);

            _logger.LogInformation("Event deleted successfully: {EventId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event: {EventId}", id);
            throw;
        }
    }
}
```

### **4. Microservicio de Matching**

#### **Implementación del Servicio de Matching**
```csharp
// MussikOn.MatchingService/Controllers/MatchingController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MatchingController : ControllerBase
{
    private readonly IMatchingService _matchingService;
    private readonly ApplicationInsightsService _appInsightsService;
    private readonly StructuredLoggingService _loggingService;
    private readonly ILogger<MatchingController> _logger;

    public MatchingController(
        IMatchingService matchingService,
        ApplicationInsightsService appInsightsService,
        StructuredLoggingService loggingService,
        ILogger<MatchingController> logger)
    {
        _matchingService = matchingService;
        _appInsightsService = appInsightsService;
        _loggingService = loggingService;
        _logger = logger;
    }

    [HttpPost("find-matches")]
    public async Task<ActionResult<IEnumerable<MusicianMatch>>> FindMatches([FromBody] FindMatchesRequest request)
    {
        try
        {
            var matches = await _matchingService.FindMatchingMusiciansAsync(request);
            
            _appInsightsService.TrackCustomEvent("MatchesFound", new Dictionary<string, string>
            {
                { "EventId", request.EventId },
                { "MatchCount", matches.Count().ToString() }
            });

            return Ok(matches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding matches for event: {EventId}", request.EventId);
            _appInsightsService.TrackException(ex);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("event/{eventId}/matches")]
    public async Task<ActionResult<IEnumerable<MusicianMatch>>> GetEventMatches(string eventId)
    {
        try
        {
            var matches = await _matchingService.GetEventMatchesAsync(eventId);
            
            _appInsightsService.TrackCustomEvent("EventMatchesRetrieved", new Dictionary<string, string>
            {
                { "EventId", eventId },
                { "MatchCount", matches.Count().ToString() }
            });

            return Ok(matches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving matches for event: {EventId}", eventId);
            _appInsightsService.TrackException(ex);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("musician/{musicianId}/apply")]
    public async Task<ActionResult<MusicianApplication>> ApplyToEvent(string musicianId, [FromBody] ApplyToEventRequest request)
    {
        try
        {
            var application = await _matchingService.ApplyToEventAsync(musicianId, request);
            
            _loggingService.LogUserAction(musicianId, "AppliedToEvent", "Event", request);
            _appInsightsService.TrackCustomEvent("MusicianApplied", new Dictionary<string, string>
            {
                { "MusicianId", musicianId },
                { "EventId", request.EventId }
            });

            return Ok(application);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying to event: {MusicianId} - {EventId}", musicianId, request.EventId);
            _appInsightsService.TrackException(ex);
            return StatusCode(500, "Internal server error");
        }
    }
}

// MussikOn.MatchingService/Services/MatchingService.cs
public class MatchingService : IMatchingService
{
    private readonly IMusicianRepository _musicianRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly ServiceBusEventService _eventService;
    private readonly ILogger<MatchingService> _logger;

    public MatchingService(
        IMusicianRepository musicianRepository,
        IEventRepository eventRepository,
        IApplicationRepository applicationRepository,
        ServiceBusEventService eventService,
        ILogger<MatchingService> logger)
    {
        _musicianRepository = musicianRepository;
        _eventRepository = eventRepository;
        _applicationRepository = applicationRepository;
        _eventService = eventService;
        _logger = logger;
    }

    public async Task<IEnumerable<MusicianMatch>> FindMatchingMusiciansAsync(FindMatchesRequest request)
    {
        try
        {
            var eventEntity = await _eventRepository.GetByIdAsync(request.EventId);
            
            if (eventEntity == null)
            {
                throw new NotFoundException($"Event with ID {request.EventId} not found");
            }

            var musicians = await _musicianRepository.GetAvailableMusiciansAsync();
            var matches = new List<MusicianMatch>();

            foreach (var musician in musicians)
            {
                var score = CalculateMatchScore(musician, eventEntity);
                
                if (score >= request.MinScore)
                {
                    matches.Add(new MusicianMatch
                    {
                        Id = Guid.NewGuid().ToString(),
                        MusicianId = musician.Id,
                        EventId = eventEntity.Id,
                        Score = score,
                        CreatedAt = DateTime.UtcNow,
                        Status = MatchStatus.Pending
                    });
                }
            }

            // Ordenar por score descendente
            matches = matches.OrderByDescending(m => m.Score).ToList();

            _logger.LogInformation("Found {MatchCount} matches for event {EventId}", matches.Count, request.EventId);
            return matches;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding matches for event: {EventId}", request.EventId);
            throw;
        }
    }

    public async Task<IEnumerable<MusicianMatch>> GetEventMatchesAsync(string eventId)
    {
        try
        {
            var matches = await _applicationRepository.GetEventMatchesAsync(eventId);
            return matches;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving matches for event: {EventId}", eventId);
            throw;
        }
    }

    public async Task<MusicianApplication> ApplyToEventAsync(string musicianId, ApplyToEventRequest request)
    {
        try
        {
            var application = new MusicianApplication
            {
                Id = Guid.NewGuid().ToString(),
                MusicianId = musicianId,
                EventId = request.EventId,
                Message = request.Message,
                ProposedFee = request.ProposedFee,
                Status = ApplicationStatus.Pending,
                AppliedAt = DateTime.UtcNow
            };

            await _applicationRepository.AddAsync(application);

            // Publicar evento
            var applicationSubmittedEvent = new MusicianApplicationSubmittedEvent
            {
                ApplicationId = application.Id,
                MusicianId = musicianId,
                EventId = request.EventId,
                Message = request.Message,
                ProposedFee = request.ProposedFee
            };

            await _eventService.PublishEventAsync(applicationSubmittedEvent, "application-submitted");

            _logger.LogInformation("Application submitted: {ApplicationId}", application.Id);
            return application;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying to event: {MusicianId} - {EventId}", musicianId, request.EventId);
            throw;
        }
    }

    private double CalculateMatchScore(Musician musician, Event eventEntity)
    {
        double score = 0;

        // Factor de género musical
        if (musician.Genres.Contains(eventEntity.Genre))
        {
            score += 40;
        }

        // Factor de experiencia
        score += Math.Min(musician.Experience * 2, 30);

        // Factor de rating
        score += musician.Rating * 6;

        // Factor de disponibilidad
        if (musician.IsAvailable)
        {
            score += 20;
        }

        // Factor de ubicación
        var distance = CalculateDistance(musician.Location, eventEntity.Location);
        score += Math.Max(0, 20 - distance);

        return Math.Min(100, score);
    }

    private double CalculateDistance(Location location1, Location location2)
    {
        // Implementar cálculo de distancia
        return 0;
    }
}
```

### **5. Configuración de Infraestructura**

#### **Docker Compose para Desarrollo**
```yaml
# docker-compose.yml
version: '3.8'

services:
  # API Gateway
  mussikon-api:
    build: ./MussikOn.API
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=db;Database=MussikOn;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;
      - ConnectionStrings__Redis=redis:6379
      - ConnectionStrings__CosmosDB=AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;
      - ServiceBus__ConnectionString=Endpoint=sb://localhost:5672;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YourKey
    depends_on:
      - db
      - redis
      - cosmosdb
      - servicebus

  # User Service
  mussikon-user-service:
    build: ./MussikOn.UserService
    ports:
      - "5001:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=db;Database=MussikOn;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;
    depends_on:
      - db

  # Event Service
  mussikon-event-service:
    build: ./MussikOn.EventService
    ports:
      - "5002:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=db;Database=MussikOn;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;
    depends_on:
      - db

  # Matching Service
  mussikon-matching-service:
    build: ./MussikOn.MatchingService
    ports:
      - "5003:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=db;Database=MussikOn;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;
    depends_on:
      - db

  # Database
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword123!
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql

  # Redis
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

  # Cosmos DB Emulator
  cosmosdb:
    image: mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest
    ports:
      - "8081:8081"
      - "10251:10251"
      - "10252:10252"
      - "10253:10253"
      - "10254:10254"
    environment:
      - AZURE_COSMOS_EMULATOR_PARTITION_COUNT=2
      - AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE=true

  # Service Bus
  servicebus:
    image: mcr.microsoft.com/azure-service-bus:latest
    ports:
      - "5672:5672"
      - "15672:15672"

volumes:
  sqlserver_data:
```

#### **Kubernetes Deployment**
```yaml
# k8s/namespace.yaml
apiVersion: v1
kind: Namespace
metadata:
  name: mussikon

---
# k8s/configmap.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: mussikon-config
  namespace: mussikon
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  ConnectionStrings__Redis: "redis-service:6379"
  ConnectionStrings__CosmosDB: "AccountEndpoint=https://cosmosdb-service:8081/;AccountKey=YourKey"

---
# k8s/secret.yaml
apiVersion: v1
kind: Secret
metadata:
  name: mussikon-secrets
  namespace: mussikon
type: Opaque
data:
  connection-string: U2VydmVyPWRiO0RhdGFiYXNlPU11c3Npa09uO1VzZXIgSWQ9c2E7UGFzc3dvcmQ9WW91clBhc3N3b3JkMTIzITtUcnVzdFNlcnZlckNlcnRpZmljYXRlPXRydWU7

---
# k8s/deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: mussikon-api
  namespace: mussikon
spec:
  replicas: 3
  selector:
    matchLabels:
      app: mussikon-api
  template:
    metadata:
      labels:
        app: mussikon-api
    spec:
      containers:
      - name: mussikon-api
        image: mussikon/api:latest
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          valueFrom:
            configMapKeyRef:
              name: mussikon-config
              key: ASPNETCORE_ENVIRONMENT
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: mussikon-secrets
              key: connection-string
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 5

---
# k8s/service.yaml
apiVersion: v1
kind: Service
metadata:
  name: mussikon-api-service
  namespace: mussikon
spec:
  selector:
    app: mussikon-api
  ports:
  - protocol: TCP
    port: 80
    targetPort: 80
  type: LoadBalancer
```

## 🛠️ **Ejercicio Práctico**

### **Ejercicio 1: Implementar Sistema Cloud Native Completo**

Crea un sistema cloud native completo para MussikOn:

```csharp
// 1. Configurar servicios principales
public class MussikOnCloudNativeConfig
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Configuración de Azure
        services.AddApplicationInsightsTelemetry();
        services.AddAzureKeyVault();
        
        // Configuración de servicios
        services.AddSingleton<ServiceBusEventService>();
        services.AddSingleton<BlobStorageService>();
        services.AddSingleton<CosmosDbService>();
        services.AddSingleton<KeyVaultService>();
        services.AddSingleton<ApplicationInsightsService>();
        services.AddSingleton<DistributedTracingService>();
        services.AddSingleton<StructuredLoggingService>();
        services.AddSingleton<HealthCheckService>();
        services.AddSingleton<AlertingService>();
        
        // Configuración de autenticación
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["AzureAD:Authority"];
                options.Audience = configuration["AzureAD:Audience"];
            });
        
        // Configuración de health checks
        services.AddHealthChecks()
            .AddSqlServer(configuration.GetConnectionString("DefaultConnection"))
            .AddRedis(configuration.GetConnectionString("Redis"))
            .AddCosmosDb(configuration.GetConnectionString("CosmosDB"));
    }
}

// 2. Implementar controlador principal
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MussikOnController : ControllerBase
{
    private readonly ApplicationInsightsService _appInsightsService;
    private readonly StructuredLoggingService _loggingService;
    private readonly HealthCheckService _healthService;
    private readonly ILogger<MussikOnController> _logger;

    [HttpGet("health")]
    public async Task<ActionResult<Dictionary<string, HealthCheckResult>>> GetHealth()
    {
        var results = new Dictionary<string, HealthCheckResult>
        {
            { "Database", await _healthService.CheckDatabaseHealthAsync() },
            { "Redis", await _healthService.CheckRedisHealthAsync() },
            { "ExternalApi", await _healthService.CheckExternalApiHealthAsync() },
            { "Storage", await _healthService.CheckStorageHealthAsync() }
        };

        return Ok(results);
    }

    [HttpGet("metrics")]
    public ActionResult<object> GetMetrics()
    {
        var metrics = new
        {
            Timestamp = DateTime.UtcNow,
            ServiceName = "MussikOn.API",
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        };

        return Ok(metrics);
    }
}

// 3. Implementar middleware de monitoreo
public class MussikOnMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ApplicationInsightsService _appInsightsService;
    private readonly DistributedTracingService _tracingService;
    private readonly ILogger<MussikOnMonitoringMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            // Track request in Application Insights
            _appInsightsService.TrackRequest(
                $"{context.Request.Method} {context.Request.Path}",
                DateTime.UtcNow - stopwatch.Elapsed,
                stopwatch.Elapsed,
                context.Response.StatusCode.ToString(),
                context.Response.StatusCode < 400);
            
            // Log structured information
            _logger.LogInformation("Request completed: {Method} {Path} - {StatusCode} in {ElapsedMs}ms",
                context.Request.Method, context.Request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
    }
}
```

## 📋 **Resumen de la Clase**

### **Conceptos Clave:**
- **Microservicios**: Arquitectura distribuida
- **API Gateway**: Punto de entrada único
- **Event-Driven**: Comunicación asíncrona
- **Cloud Native**: Aplicaciones nativas de la nube
- **Observabilidad**: Visibilidad completa
- **Escalabilidad**: Crecimiento automático

### **Proyecto Completado:**
**MussikOn Cloud Native** - Sistema completo de matching musical

## 🎯 **Objetivos de Aprendizaje**

Al finalizar esta clase, serás capaz de:
- ✅ Implementar arquitectura de microservicios
- ✅ Configurar API Gateway
- ✅ Implementar comunicación event-driven
- ✅ Desplegar en la nube
- ✅ Implementar observabilidad completa
- ✅ Optimizar para escalabilidad
