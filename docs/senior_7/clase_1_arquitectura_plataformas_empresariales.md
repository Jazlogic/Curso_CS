# 🏗️ Clase 1: Arquitectura de Plataformas Empresariales

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Módulo 13: Performance, Seguridad y Deployment](../senior_6/README.md)
- **🏠 Inicio del Módulo**: [Módulo 14: Plataformas Empresariales Reales](../senior_7/README.md)
- **➡️ Siguiente**: [Clase 2: Comunicación en Tiempo Real](../senior_7/clase_2_comunicacion_tiempo_real.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Comprender** los principios de Clean Architecture en proyectos empresariales reales
2. **Implementar** el patrón CQRS para separar operaciones de lectura y escritura
3. **Diseñar** sistemas de Domain Events para comunicación entre capas
4. **Aplicar** Event Sourcing para auditoría y trazabilidad completa
5. **Crear** una estructura de proyecto escalable y mantenible

---

## 🏗️ **Clean Architecture Avanzada**

### **Estructura de Capas para MussikOn**

La arquitectura limpia divide la aplicación en capas bien definidas, cada una con responsabilidades específicas:

```csharp
// Estructura de directorios del proyecto
MussikOn.API/
├── Presentation/          # Controllers, Middleware, DTOs
├── Application/           # Services, Validators, AutoMapper
├── Domain/               # Entities, Interfaces, Domain Services
└── Infrastructure/       # Repositories, External Services, Configuration
```

#### **1. Capa de Presentación (Presentation)**

```csharp
// Controllers que manejan las peticiones HTTP
[ApiController]
[Route("api/[controller]")]
public class MusicianRequestController : ControllerBase
{
    private readonly IMediator _mediator; // Patrón Mediator para CQRS
    private readonly ILogger<MusicianRequestController> _logger;

    public MusicianRequestController(IMediator mediator, ILogger<MusicianRequestController> logger)
    {
        _mediator = mediator; // Inyección del mediator para enviar commands/queries
        _logger = logger;     // Logger para auditoría y debugging
    }

    // POST: api/musicianrequest - Crear nueva solicitud
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.CanCreateRequests)] // Autorización basada en políticas
    public async Task<ActionResult<Guid>> CreateRequest([FromBody] CreateMusicianRequestCommand command)
    {
        try
        {
            _logger.LogInformation("Creating new musician request for event: {EventType}", command.EventType);
            
            var requestId = await _mediator.Send(command); // Envía el command al handler correspondiente
            
            _logger.LogInformation("Musician request created successfully with ID: {RequestId}", requestId);
            
            return CreatedAtAction(nameof(GetRequest), new { id = requestId }, requestId);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed for musician request: {Errors}", ex.Errors);
            return BadRequest(ex.Errors);
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning("Business rule violation: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    // GET: api/musicianrequest/{id} - Obtener solicitud por ID
    [HttpGet("{id}")]
    [Authorize(Policy = AuthorizationPolicies.CanViewRequests)]
    public async Task<ActionResult<MusicianRequestDto>> GetRequest(Guid id)
    {
        var query = new GetMusicianRequestByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    // GET: api/musicianrequest - Listar solicitudes con filtros
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.CanViewRequests)]
    public async Task<ActionResult<PagedResult<MusicianRequestDto>>> GetRequests(
        [FromQuery] GetMusicianRequestsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
```

#### **2. Capa de Aplicación (Application)**

```csharp
// Commands para modificar el estado del sistema
public class CreateMusicianRequestCommand : IRequest<Guid>
{
    public string EventType { get; set; }        // Tipo de evento (boda, fiesta, etc.)
    public DateTime Date { get; set; }           // Fecha del evento
    public string Location { get; set; }         // Ubicación del evento
    public string Instrument { get; set; }       // Instrumento requerido
    public decimal Budget { get; set; }          // Presupuesto disponible
    public string Description { get; set; }      // Descripción adicional
    public int DurationHours { get; set; }       // Duración en horas
    public string Style { get; set; }            // Estilo musical preferido
}

// Handler que procesa el command
public class CreateMusicianRequestCommandHandler : IRequestHandler<CreateMusicianRequestCommand, Guid>
{
    private readonly IMusicianRequestRepository _repository;           // Repositorio para persistencia
    private readonly IUnitOfWork _unitOfWork;                         // Unidad de trabajo para transacciones
    private readonly IMusicianRequestDomainService _domainService;     // Servicio de dominio para validaciones
    private readonly INotificationService _notificationService;        // Servicio de notificaciones
    private readonly ILogger<CreateMusicianRequestCommandHandler> _logger; // Logger para auditoría

    public CreateMusicianRequestCommandHandler(
        IMusicianRequestRepository repository,
        IUnitOfWork unitOfWork,
        IMusicianRequestDomainService domainService,
        INotificationService notificationService,
        ILogger<CreateMusicianRequestCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _domainService = domainService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateMusicianRequestCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar reglas de negocio del dominio
        await _domainService.ValidateRequestCreation(request);
        
        // 2. Crear la entidad de dominio
        var musicianRequest = MusicianRequest.Create(
            request.EventType,
            request.Date,
            request.Location,
            request.Instrument,
            request.Budget,
            request.Description,
            request.DurationHours,
            request.Style
        );
        
        // 3. Persistir en el repositorio
        _repository.Add(musicianRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // 4. Publicar evento de dominio
        musicianRequest.AddDomainEvent(new MusicianRequestCreatedEvent
        {
            RequestId = musicianRequest.Id,
            EventType = musicianRequest.EventType,
            Location = musicianRequest.Location,
            Instrument = musicianRequest.Instrument
        });
        
        // 5. Notificar a músicos disponibles
        await _notificationService.NotifyMusiciansOfNewRequest(musicianRequest);
        
        _logger.LogInformation("Musician request created successfully: {RequestId}", musicianRequest.Id);
        
        return musicianRequest.Id;
    }
}

// Queries para consultar datos (sin modificar estado)
public class GetMusicianRequestByIdQuery : IRequest<MusicianRequestDto>
{
    public Guid Id { get; set; }
}

public class GetMusicianRequestsQuery : IRequest<PagedResult<MusicianRequestDto>>
{
    public string? EventType { get; set; }       // Filtro opcional por tipo de evento
    public string? Location { get; set; }        // Filtro opcional por ubicación
    public string? Instrument { get; set; }      // Filtro opcional por instrumento
    public decimal? MinBudget { get; set; }      // Presupuesto mínimo
    public decimal? MaxBudget { get; set; }      // Presupuesto máximo
    public DateTime? FromDate { get; set; }      // Fecha desde
    public DateTime? ToDate { get; set; }        // Fecha hasta
    public RequestStatus? Status { get; set; }   // Estado de la solicitud
    public int Page { get; set; } = 1;           // Número de página
    public int PageSize { get; set; } = 10;      // Tamaño de página
}

// Handler para queries
public class GetMusicianRequestByIdQueryHandler : IRequestHandler<GetMusicianRequestByIdQuery, MusicianRequestDto>
{
    private readonly IMusicianRequestReadRepository _readRepository;  // Repositorio de solo lectura
    private readonly ILogger<GetMusicianRequestByIdQueryHandler> _logger;

    public GetMusicianRequestByIdQueryHandler(
        IMusicianRequestReadRepository readRepository,
        ILogger<GetMusicianRequestByIdQueryHandler> logger)
    {
        _readRepository = readRepository;
        _logger = logger;
    }

    public async Task<MusicianRequestDto> Handle(GetMusicianRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await _readRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (dto == null)
        {
            _logger.LogWarning("Musician request not found: {RequestId}", request.Id);
            return null;
        }
        
        return dto;
    }
}
```

---

## 🎭 **Patrón CQRS (Command Query Responsibility Segregation)**

### **Separación de Responsabilidades**

CQRS separa las operaciones de lectura (Queries) de las de escritura (Commands), permitiendo optimizaciones específicas para cada caso:

```csharp
// Interface para repositorio de escritura
public interface IMusicianRequestRepository : IRepository<MusicianRequest>
{
    Task<MusicianRequest> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<MusicianRequest>> GetByOrganizerAsync(Guid organizerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<MusicianRequest>> GetByStatusAsync(RequestStatus status, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}

// Interface para repositorio de solo lectura (optimizado para queries)
public interface IMusicianRequestReadRepository
{
    Task<MusicianRequestDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<MusicianRequestDto>> GetPagedAsync(GetMusicianRequestsQuery query, CancellationToken cancellationToken = default);
    Task<IEnumerable<MusicianRequestSummaryDto>> GetSummariesByOrganizerAsync(Guid organizerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<MusicianRequestDto>> GetByInstrumentAndLocationAsync(string instrument, string location, CancellationToken cancellationToken = default);
}

// Implementación del repositorio de escritura
public class MusicianRequestRepository : IMusicianRequestRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MusicianRequestRepository> _logger;

    public MusicianRequestRepository(ApplicationDbContext context, ILogger<MusicianRequestRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MusicianRequest> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Carga eager de relaciones para operaciones de escritura
        return await _context.MusicianRequests
            .Include(r => r.Organizer)           // Incluye datos del organizador
            .Include(r => r.AssignedMusician)    // Incluye músico asignado si existe
            .Include(r => r.Applications)        // Incluye aplicaciones de músicos
            .Include(r => r.DomainEvents)        // Incluye eventos de dominio pendientes
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<MusicianRequest>> GetByOrganizerAsync(Guid organizerId, CancellationToken cancellationToken = default)
    {
        return await _context.MusicianRequests
            .Include(r => r.AssignedMusician)
            .Where(r => r.OrganizerId == organizerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.MusicianRequests.AnyAsync(r => r.Id == id, cancellationToken);
    }

    public void Add(MusicianRequest entity)
    {
        _context.MusicianRequests.Add(entity);
        _logger.LogInformation("Added musician request to context: {RequestId}", entity.Id);
    }

    public void Update(MusicianRequest entity)
    {
        _context.MusicianRequests.Update(entity);
        _logger.LogInformation("Updated musician request in context: {RequestId}", entity.Id);
    }

    public void Delete(MusicianRequest entity)
    {
        _context.MusicianRequests.Remove(entity);
        _logger.LogInformation("Removed musician request from context: {RequestId}", entity.Id);
    }
}

// Implementación del repositorio de solo lectura (optimizado para performance)
public class MusicianRequestReadRepository : IMusicianRequestReadRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;        // Cache en memoria para queries frecuentes
    private readonly ILogger<MusicianRequestReadRepository> _logger;

    public MusicianRequestReadRepository(
        ApplicationDbContext context, 
        IMemoryCache cache,
        ILogger<MusicianRequestReadRepository> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<MusicianRequestDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Clave de cache para esta consulta específica
        var cacheKey = $"musician_request_{id}";
        
        // Intentar obtener del cache primero
        if (_cache.TryGetValue(cacheKey, out MusicianRequestDto cachedResult))
        {
            _logger.LogDebug("Cache hit for musician request: {RequestId}", id);
            return cachedResult;
        }

        // Si no está en cache, consultar la base de datos
        var result = await _context.MusicianRequests
            .AsNoTracking()                      // No tracking para mejor performance en queries
            .Where(r => r.Id == id)
            .Select(r => new MusicianRequestDto
            {
                Id = r.Id,
                EventType = r.EventType,
                Date = r.Date,
                Location = r.Location,
                Instrument = r.Instrument,
                Budget = r.Budget,
                Description = r.Description,
                Status = r.Status,
                OrganizerName = r.Organizer.FullName,
                AssignedMusicianName = r.AssignedMusician != null ? r.AssignedMusician.FullName : null,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (result != null)
        {
            // Guardar en cache por 5 minutos
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
            
            _cache.Set(cacheKey, result, cacheOptions);
            _logger.LogDebug("Cached musician request: {RequestId}", id);
        }

        return result;
    }

    public async Task<PagedResult<MusicianRequestDto>> GetPagedAsync(GetMusicianRequestsQuery query, CancellationToken cancellationToken = default)
    {
        // Construir query base con filtros
        var queryable = _context.MusicianRequests
            .AsNoTracking()
            .AsQueryable();

        // Aplicar filtros si están especificados
        if (!string.IsNullOrEmpty(query.EventType))
            queryable = queryable.Where(r => r.EventType.Contains(query.EventType));
            
        if (!string.IsNullOrEmpty(query.Location))
            queryable = queryable.Where(r => r.Location.Contains(query.Location));
            
        if (!string.IsNullOrEmpty(query.Instrument))
            queryable = queryable.Where(r => r.Instrument.Contains(query.Instrument));
            
        if (query.MinBudget.HasValue)
            queryable = queryable.Where(r => r.Budget >= query.MinBudget.Value);
            
        if (query.MaxBudget.HasValue)
            queryable = queryable.Where(r => r.Budget <= query.MaxBudget.Value);
            
        if (query.FromDate.HasValue)
            queryable = queryable.Where(r => r.Date >= query.FromDate.Value);
            
        if (query.ToDate.HasValue)
            queryable = queryable.Where(r => r.Date <= query.ToDate.Value);
            
        if (query.Status.HasValue)
            queryable = queryable.Where(r => r.Status == query.Status.Value);

        // Contar total de registros para paginación
        var totalCount = await queryable.CountAsync(cancellationToken);
        
        // Aplicar paginación y ordenamiento
        var items = await queryable
            .OrderByDescending(r => r.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(r => new MusicianRequestDto
            {
                Id = r.Id,
                EventType = r.EventType,
                Date = r.Date,
                Location = r.Location,
                Instrument = r.Instrument,
                Budget = r.Budget,
                Status = r.Status,
                OrganizerName = r.Organizer.FullName,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<MusicianRequestDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
        };
    }
}
```

---

## 🎪 **Domain Events y Event Sourcing**

### **Sistema de Eventos de Dominio**

Los eventos de dominio permiten que diferentes partes del sistema reaccionen a cambios sin acoplamiento directo:

```csharp
// Evento base abstracto
public abstract class DomainEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();           // Identificador único del evento
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow; // Timestamp de cuando ocurrió
    public string EventType { get; set; }                     // Tipo del evento para categorización
    public string AggregateId { get; set; }                   // ID de la entidad agregada
    public long Version { get; set; }                         // Versión para Event Sourcing
}

// Evento específico: Solicitud de músico creada
public class MusicianRequestCreatedEvent : DomainEvent
{
    public Guid RequestId { get; set; }                       // ID de la solicitud creada
    public string EventType { get; set; }                     // Tipo de evento musical
    public string Location { get; set; }                      // Ubicación del evento
    public string Instrument { get; set; }                    // Instrumento requerido
    public decimal Budget { get; set; }                       // Presupuesto disponible
    public Guid OrganizerId { get; set; }                     // ID del organizador
    public DateTime EventDate { get; set; }                   // Fecha del evento

    public MusicianRequestCreatedEvent()
    {
        EventType = "MusicianRequestCreated";                 // Tipo del evento
    }
}

// Evento: Solicitud asignada a un músico
public class MusicianRequestAssignedEvent : DomainEvent
{
    public Guid RequestId { get; set; }                       // ID de la solicitud
    public Guid MusicianId { get; set; }                      // ID del músico asignado
    public DateTime AssignedOn { get; set; }                  // Fecha de asignación
    public string MusicianName { get; set; }                  // Nombre del músico
    public decimal AgreedRate { get; set; }                   // Tarifa acordada

    public MusicianRequestAssignedEvent()
    {
        EventType = "MusicianRequestAssigned";
    }
}

// Evento: Estado de solicitud cambiado
public class MusicianRequestStatusChangedEvent : DomainEvent
{
    public Guid RequestId { get; set; }                       // ID de la solicitud
    public RequestStatus OldStatus { get; set; }              // Estado anterior
    public RequestStatus NewStatus { get; set; }              // Nuevo estado
    public string Reason { get; set; }                        // Razón del cambio
    public Guid ChangedByUserId { get; set; }                 // Usuario que realizó el cambio

    public MusicianRequestStatusChangedEvent()
    {
        EventType = "MusicianRequestStatusChanged";
    }
}

// Evento: Músico aplicó a una solicitud
public class MusicianAppliedToRequestEvent : DomainEvent
{
    public Guid RequestId { get; set; }                       // ID de la solicitud
    public Guid MusicianId { get; set; }                      // ID del músico
    public string MusicianName { get; set; }                  // Nombre del músico
    public decimal ProposedRate { get; set; }                 // Tarifa propuesta
    public string CoverLetter { get; set; }                   // Carta de presentación
    public DateTime AppliedOn { get; set; }                   // Fecha de aplicación

    public MusicianAppliedToRequestEvent()
    {
        EventType = "MusicianAppliedToRequest";
    }
}
```

### **Manejo de Eventos de Dominio**

```csharp
// Interface para el servicio de eventos de dominio
public interface IDomainEventService
{
    Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default);
    Task PublishAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default);
}

// Implementación del servicio de eventos
public class DomainEventService : IDomainEventService
{
    private readonly IPublisher _mediator;                     // Mediator para publicar eventos
    private readonly ILogger<DomainEventService> _logger;

    public DomainEventService(IPublisher mediator, ILogger<DomainEventService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Publishing domain event: {EventType} for aggregate {AggregateId}", 
                domainEvent.EventType, domainEvent.AggregateId);
            
            await _mediator.Publish(domainEvent, cancellationToken);
            
            _logger.LogInformation("Domain event published successfully: {EventType}", domainEvent.EventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing domain event: {EventType}", domainEvent.EventType);
            throw;
        }
    }

    public async Task PublishAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            await PublishAsync(domainEvent, cancellationToken);
        }
    }
}

// Handler para el evento de solicitud creada
public class MusicianRequestCreatedEventHandler : INotificationHandler<MusicianRequestCreatedEvent>
{
    private readonly INotificationService _notificationService;        // Servicio de notificaciones
    private readonly IMusicianMatchingService _matchingService;        // Servicio de matching
    private readonly ILogger<MusicianRequestCreatedEventHandler> _logger;

    public MusicianRequestCreatedEventHandler(
        INotificationService notificationService,
        IMusicianMatchingService matchingService,
        ILogger<MusicianRequestCreatedEventHandler> logger)
    {
        _notificationService = notificationService;
        _matchingService = matchingService;
        _logger = logger;
    }

    public async Task Handle(MusicianRequestCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Handling MusicianRequestCreatedEvent for request: {RequestId}", notification.RequestId);
            
            // 1. Notificar a músicos disponibles sobre la nueva solicitud
            await _notificationService.NotifyMusiciansOfNewRequest(notification.RequestId);
            
            // 2. Buscar músicos que coincidan con la solicitud
            var matches = await _matchingService.FindMatchesForRequest(notification.RequestId);
            
            // 3. Notificar a los mejores matches
            if (matches.Any())
            {
                await _notificationService.NotifyTopMatches(notification.RequestId, matches.Take(5));
            }
            
            _logger.LogInformation("MusicianRequestCreatedEvent handled successfully for request: {RequestId}", notification.RequestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling MusicianRequestCreatedEvent for request: {RequestId}", notification.RequestId);
            throw;
        }
    }
}

// Handler para el evento de solicitud asignada
public class MusicianRequestAssignedEventHandler : INotificationHandler<MusicianRequestAssignedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly ILogger<MusicianRequestAssignedEventHandler> _logger;

    public MusicianRequestAssignedEventHandler(
        INotificationService notificationService,
        IEmailService emailService,
        ILogger<MusicianRequestAssignedEventHandler> logger)
    {
        _notificationService = notificationService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(MusicianRequestAssignedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Handling MusicianRequestAssignedEvent for request: {RequestId}", notification.RequestId);
            
            // 1. Notificar al músico sobre la asignación
            await _notificationService.NotifyMusicianOfRequestAssignment(
                notification.RequestId, 
                notification.MusicianId);
            
            // 2. Enviar email de confirmación al músico
            await _emailService.SendAssignmentConfirmationEmail(
                notification.MusicianId, 
                notification.RequestId);
            
            // 3. Notificar al organizador sobre la asignación
            await _notificationService.NotifyOrganizerOfMusicianAssignment(
                notification.RequestId, 
                notification.MusicianId);
            
            _logger.LogInformation("MusicianRequestAssignedEvent handled successfully for request: {RequestId}", notification.RequestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling MusicianRequestAssignedEvent for request: {RequestId}", notification.RequestId);
            throw;
        }
    }
}
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Implementar Clean Architecture**
```csharp
// Crea la estructura de carpetas para un proyecto de e-commerce:
// - Presentation: Controllers, Middleware, DTOs
// - Application: Services, Validators, Commands/Queries
// - Domain: Entities, Interfaces, Domain Services
// - Infrastructure: Repositories, External Services
```

### **Ejercicio 2: Implementar CQRS**
```csharp
// Crea un sistema de gestión de productos con:
// - Commands: CreateProduct, UpdateProduct, DeleteProduct
// - Queries: GetProduct, GetProducts, SearchProducts
// - Handlers separados para cada command/query
```

### **Ejercicio 3: Sistema de Eventos de Dominio**
```csharp
// Implementa eventos para un sistema de pedidos:
// - OrderCreatedEvent
// - OrderStatusChangedEvent
// - OrderCancelledEvent
// - Handlers para cada evento
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **🏗️ Clean Architecture**: Estructura de capas bien definidas para proyectos empresariales
2. **🎭 CQRS**: Separación de operaciones de lectura y escritura para optimización
3. **🎪 Domain Events**: Sistema de eventos para comunicación desacoplada entre capas
4. **📊 Event Sourcing**: Trazabilidad completa de cambios en el sistema
5. **🔧 Implementación Práctica**: Código real para la plataforma MussikOn

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Comunicación en Tiempo Real con SignalR**, implementando hubs para notificaciones instantáneas y chat en vivo entre usuarios.

---

**¡Has completado la primera clase del Módulo 14! 🎵🏗️**

