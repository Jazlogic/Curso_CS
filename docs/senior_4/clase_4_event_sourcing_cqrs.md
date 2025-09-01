# 🚀 Clase 4: Event Sourcing y CQRS en Microservicios

## 🧭 Navegación
- **⬅️ Anterior**: [Clase 3: Service Mesh y API Gateways](clase_3_service_mesh_api_gateways.md)
- **🏠 Inicio del Módulo**: [Módulo 11: Arquitectura de Microservicios Avanzada](README.md)
- **➡️ Siguiente**: [Clase 5: Monitoreo y Observabilidad Distribuida](clase_5_monitoreo_observabilidad.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 📚 Descripción

En esta clase aprenderás sobre Event Sourcing y CQRS (Command Query Responsibility Segregation), patrones fundamentales para mantener la consistencia de datos y escalabilidad en sistemas de microservicios distribuidos.

## 🎯 Objetivos de Aprendizaje

- Implementar Event Sourcing para auditoría y reconstrucción de estado
- Separar comandos y consultas con CQRS
- Crear proyecciones para diferentes vistas de datos
- Implementar sagas para transacciones distribuidas

## 📖 Contenido Teórico

### ¿Qué es Event Sourcing?

Event Sourcing es un patrón que almacena todos los cambios en el estado de una aplicación como una secuencia de eventos. En lugar de almacenar solo el estado actual, se mantiene un historial completo de todos los cambios.

### Ventajas del Event Sourcing

```csharp
public class EventSourcingBenefits
{
    // 1. Auditoría Completa
    public void CompleteAudit()
    {
        // Cada cambio está registrado con timestamp y usuario
        // Imposible modificar el historial sin dejar rastro
    }
    
    // 2. Reconstrucción de Estado
    public void StateReconstruction()
    {
        // Puedes recrear el estado en cualquier punto del tiempo
        // Útil para debugging y análisis
    }
    
    // 3. Análisis Temporal
    public void TemporalAnalysis()
    {
        // Analizar cómo cambió el estado a lo largo del tiempo
        // Identificar patrones y tendencias
    }
    
    // 4. Deshacer Cambios
    public void UndoChanges()
    {
        // Revertir a estados anteriores
        // Implementar funcionalidad de "deshacer"
    }
}
```

### Implementación de Event Sourcing

#### 1. Eventos del Dominio

```csharp
public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
    string AggregateId { get; }
    long Version { get; }
}

public abstract class DomainEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public abstract string AggregateId { get; }
    public abstract long Version { get; }
}

// Eventos específicos del dominio
public class UserCreatedEvent : DomainEvent
{
    public string UserId { get; }
    public string Email { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public DateTime CreatedAt { get; }

    public UserCreatedEvent(string userId, string email, string firstName, string lastName)
    {
        UserId = userId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        CreatedAt = DateTime.UtcNow;
    }

    public override string AggregateId => UserId;
    public override long Version => 1;
}

public class UserProfileUpdatedEvent : DomainEvent
{
    public string UserId { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public DateTime UpdatedAt { get; }

    public UserProfileUpdatedEvent(string userId, string firstName, string lastName)
    {
        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
        UpdatedAt = DateTime.UtcNow;
    }

    public override string AggregateId => UserId;
    public override long Version => 2;
}

public class UserRoleAddedEvent : DomainEvent
{
    public string UserId { get; }
    public string Role { get; }
    public DateTime AddedAt { get; }

    public UserRoleAddedEvent(string userId, string role)
    {
        UserId = userId;
        Role = role;
        AddedAt = DateTime.UtcNow;
    }

    public override string AggregateId => UserId;
    public override long Version => 3;
}
```

#### 2. Event Store

```csharp
public interface IEventStore
{
    Task SaveEventsAsync(string aggregateId, IEnumerable<IDomainEvent> events, long expectedVersion);
    Task<IEnumerable<IDomainEvent>> GetEventsAsync(string aggregateId);
    Task<IEnumerable<IDomainEvent>> GetEventsAsync(string aggregateId, long fromVersion);
    Task<long> GetLastVersionAsync(string aggregateId);
}

public class EventStore : IEventStore
{
    private readonly ILogger<EventStore> _logger;
    private readonly IDbContext _dbContext;

    public EventStore(ILogger<EventStore> logger, IDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task SaveEventsAsync(string aggregateId, IEnumerable<IDomainEvent> events, long expectedVersion)
    {
        var eventList = events.ToList();
        var lastVersion = await GetLastVersionAsync(aggregateId);

        if (expectedVersion != lastVersion)
        {
            throw new ConcurrencyException($"Expected version {expectedVersion}, but got {lastVersion}");
        }

        var eventEntities = eventList.Select((e, index) => new EventEntity
        {
            Id = e.Id,
            AggregateId = e.AggregateId,
            Version = expectedVersion + index + 1,
            EventType = e.GetType().Name,
            EventData = JsonSerializer.Serialize(e),
            OccurredOn = e.OccurredOn,
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.Events.AddRangeAsync(eventEntities);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Saved {EventCount} events for aggregate {AggregateId}", 
            eventList.Count, aggregateId);
    }

    public async Task<IEnumerable<IDomainEvent>> GetEventsAsync(string aggregateId)
    {
        var events = await _dbContext.Events
            .Where(e => e.AggregateId == aggregateId)
            .OrderBy(e => e.Version)
            .ToListAsync();

        return events.Select(DeserializeEvent);
    }

    public async Task<IEnumerable<IDomainEvent>> GetEventsAsync(string aggregateId, long fromVersion)
    {
        var events = await _dbContext.Events
            .Where(e => e.AggregateId == aggregateId && e.Version > fromVersion)
            .OrderBy(e => e.Version)
            .ToListAsync();

        return events.Select(DeserializeEvent);
    }

    public async Task<long> GetLastVersionAsync(string aggregateId)
    {
        var lastEvent = await _dbContext.Events
            .Where(e => e.AggregateId == aggregateId)
            .OrderByDescending(e => e.Version)
            .FirstOrDefaultAsync();

        return lastEvent?.Version ?? 0;
    }

    private IDomainEvent DeserializeEvent(EventEntity eventEntity)
    {
        var eventType = Type.GetType(eventEntity.EventType);
        if (eventType == null)
        {
            throw new InvalidOperationException($"Unknown event type: {eventEntity.EventType}");
        }

        return (IDomainEvent)JsonSerializer.Deserialize(eventEntity.EventData, eventType);
    }
}

public class EventEntity
{
    public Guid Id { get; set; }
    public string AggregateId { get; set; }
    public long Version { get; set; }
    public string EventType { get; set; }
    public string EventData { get; set; }
    public DateTime OccurredOn { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ConcurrencyException : Exception
{
    public ConcurrencyException(string message) : base(message) { }
}
```

#### 3. Agregados con Event Sourcing

```csharp
public abstract class EventSourcedAggregate
{
    private readonly List<IDomainEvent> _uncommittedEvents = new();
    private long _version = 0;

    public string Id { get; protected set; }
    public long Version => _version;

    protected void Apply(IDomainEvent @event)
    {
        When(@event);
        _uncommittedEvents.Add(@event);
    }

    protected abstract void When(IDomainEvent @event);

    public IEnumerable<IDomainEvent> GetUncommittedEvents() => _uncommittedEvents;

    public void MarkEventsAsCommitted()
    {
        _uncommittedEvents.Clear();
    }

    protected void LoadFromHistory(IEnumerable<IDomainEvent> history)
    {
        foreach (var @event in history)
        {
            When(@event);
            _version = @event.Version;
        }
    }
}

public class UserEventSourced : EventSourcedAggregate
{
    public string Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public List<string> Roles { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }

    public UserEventSourced()
    {
        Roles = new List<string>();
    }

    public static UserEventSourced Create(string email, string firstName, string lastName)
    {
        var user = new UserEventSourced();
        user.Apply(new UserCreatedEvent(Guid.NewGuid().ToString(), email, firstName, lastName));
        return user;
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        Apply(new UserProfileUpdatedEvent(Id, firstName, lastName));
    }

    public void AddRole(string role)
    {
        if (!Roles.Contains(role))
        {
            Apply(new UserRoleAddedEvent(Id, role));
        }
    }

    public void Deactivate()
    {
        Apply(new UserDeactivatedEvent(Id));
    }

    protected override void When(IDomainEvent @event)
    {
        switch (@event)
        {
            case UserCreatedEvent e:
                Id = e.UserId;
                Email = e.Email;
                FirstName = e.FirstName;
                LastName = e.LastName;
                CreatedAt = e.CreatedAt;
                IsActive = true;
                break;
            case UserProfileUpdatedEvent e:
                FirstName = e.FirstName;
                LastName = e.LastName;
                LastModifiedAt = e.UpdatedAt;
                break;
            case UserRoleAddedEvent e:
                if (!Roles.Contains(e.Role))
                {
                    Roles.Add(e.Role);
                }
                LastModifiedAt = e.AddedAt;
                break;
            case UserDeactivatedEvent e:
                IsActive = false;
                LastModifiedAt = e.DeactivatedAt;
                break;
        }
    }
}
```

### CQRS (Command Query Responsibility Segregation)

CQRS separa las operaciones de lectura (queries) de las operaciones de escritura (commands), permitiendo optimizar cada una de manera independiente.

#### 1. Commands

```csharp
public interface ICommand
{
    Guid Id { get; }
}

public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    Task HandleAsync(TCommand command);
}

public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand
{
    Task<TResult> HandleAsync(TCommand command);
}

// Commands específicos
public class CreateUserCommand : ICommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Password { get; set; }
}

public class UpdateUserProfileCommand : ICommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public string UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class AddUserRoleCommand : ICommand
{
    public Guid Id { get; } = Guid.NewGuid();
    public string UserId { get; set; }
    public string Role { get; set; }
}

// Command Handlers
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, string>
{
    private readonly IEventStore _eventStore;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(IEventStore eventStore, ILogger<CreateUserCommandHandler> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    public async Task<string> HandleAsync(CreateUserCommand command)
    {
        var user = UserEventSourced.Create(command.Email, command.FirstName, command.LastName);
        
        await _eventStore.SaveEventsAsync(user.Id, user.GetUncommittedEvents(), 0);
        
        user.MarkEventsAsCommitted();
        
        _logger.LogInformation("Created user {UserId} with email {Email}", user.Id, command.Email);
        
        return user.Id;
    }
}

public class UpdateUserProfileCommandHandler : ICommandHandler<UpdateUserProfileCommand>
{
    private readonly IEventStore _eventStore;
    private readonly ILogger<UpdateUserProfileCommandHandler> _logger;

    public UpdateUserProfileCommandHandler(IEventStore eventStore, ILogger<UpdateUserProfileCommandHandler> logger)
    {
        _eventStore = eventStore;
        _logger = logger;
    }

    public async Task HandleAsync(UpdateUserProfileCommand command)
    {
        var events = await _eventStore.GetEventsAsync(command.UserId);
        var user = new UserEventSourced();
        user.LoadFromHistory(events);

        user.UpdateProfile(command.FirstName, command.LastName);

        var lastVersion = await _eventStore.GetLastVersionAsync(command.UserId);
        await _eventStore.SaveEventsAsync(command.UserId, user.GetUncommittedEvents(), lastVersion);

        user.MarkEventsAsCommitted();

        _logger.LogInformation("Updated profile for user {UserId}", command.UserId);
    }
}
```

#### 2. Queries

```csharp
public interface IQuery<TResult>
{
}

public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query);
}

// Queries específicos
public class GetUserByIdQuery : IQuery<UserDto>
{
    public string UserId { get; set; }
}

public class GetUsersByRoleQuery : IQuery<IEnumerable<UserDto>>
{
    public string Role { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class SearchUsersQuery : IQuery<IEnumerable<UserDto>>
{
    public string SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

// Query Handlers
public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserReadRepository _userReadRepository;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;

    public GetUserByIdQueryHandler(IUserReadRepository userReadRepository, ILogger<GetUserByIdQueryHandler> logger)
    {
        _userReadRepository = userReadRepository;
        _logger = logger;
    }

    public async Task<UserDto> HandleAsync(GetUserByIdQuery query)
    {
        var user = await _userReadRepository.GetByIdAsync(query.UserId);
        
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", query.UserId);
            return null;
        }

        return user;
    }
}

public class GetUsersByRoleQueryHandler : IQueryHandler<GetUsersByRoleQuery, IEnumerable<UserDto>>
{
    private readonly IUserReadRepository _userReadRepository;
    private readonly ILogger<GetUsersByRoleQueryHandler> _logger;

    public GetUsersByRoleQueryHandler(IUserReadRepository userReadRepository, ILogger<GetUsersByRoleQueryHandler> logger)
    {
        _userReadRepository = userReadRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<UserDto>> HandleAsync(GetUsersByRoleQuery query)
    {
        var users = await _userReadRepository.GetByRoleAsync(query.Role, query.Page, query.PageSize);
        
        _logger.LogDebug("Retrieved {UserCount} users with role {Role}", users.Count(), query.Role);
        
        return users;
    }
}
```

#### 3. Mediator Pattern

```csharp
public interface IMediator
{
    Task<TResult> SendAsync<TResult>(IQuery<TResult> query);
    Task SendAsync(ICommand command);
    Task<TResult> SendAsync<TResult>(ICommand<TResult> command);
}

public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Mediator> _logger;

    public Mediator(IServiceProvider serviceProvider, ILogger<Mediator> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<TResult> SendAsync<TResult>(IQuery<TResult> query)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        var handler = _serviceProvider.GetService(handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException($"No handler found for query {query.GetType().Name}");
        }

        var method = handlerType.GetMethod("HandleAsync");
        var result = await (Task<TResult>)method.Invoke(handler, new object[] { query });

        return result;
    }

    public async Task SendAsync(ICommand command)
    {
        var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
        var handler = _serviceProvider.GetService(handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException($"No handler found for command {command.GetType().Name}");
        }

        var method = handlerType.GetMethod("HandleAsync");
        await (Task)method.Invoke(handler, new object[] { command });
    }

    public async Task<TResult> SendAsync<TResult>(ICommand<TResult> command)
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
        var handler = _serviceProvider.GetService(handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException($"No handler found for command {command.GetType().Name}");
        }

        var method = handlerType.GetMethod("HandleAsync");
        var result = await (Task<TResult>)method.Invoke(handler, new object[] { command });

        return result;
    }
}
```

### Proyecciones (Projections)

Las proyecciones transforman los eventos en vistas de datos optimizadas para consultas.

```csharp
public interface IProjection
{
    Task HandleAsync(IDomainEvent @event);
}

public class UserProjection : IProjection
{
    private readonly IUserReadRepository _userReadRepository;
    private readonly ILogger<UserProjection> _logger;

    public UserProjection(IUserReadRepository userReadRepository, ILogger<UserProjection> logger)
    {
        _userReadRepository = userReadRepository;
        _logger = logger;
    }

    public async Task HandleAsync(IDomainEvent @event)
    {
        switch (@event)
        {
            case UserCreatedEvent e:
                await HandleUserCreated(e);
                break;
            case UserProfileUpdatedEvent e:
                await HandleUserProfileUpdated(e);
                break;
            case UserRoleAddedEvent e:
                await HandleUserRoleAdded(e);
                break;
            case UserDeactivatedEvent e:
                await HandleUserDeactivated(e);
                break;
        }
    }

    private async Task HandleUserCreated(UserCreatedEvent @event)
    {
        var user = new UserDto
        {
            Id = @event.UserId,
            Email = @event.Email,
            FirstName = @event.FirstName,
            LastName = @event.LastName,
            Roles = new List<string>(),
            IsActive = true,
            CreatedAt = @event.CreatedAt,
            LastModifiedAt = @event.CreatedAt
        };

        await _userReadRepository.CreateAsync(user);
        
        _logger.LogInformation("Created projection for user {UserId}", @event.UserId);
    }

    private async Task HandleUserProfileUpdated(UserProfileUpdatedEvent @event)
    {
        var user = await _userReadRepository.GetByIdAsync(@event.UserId);
        if (user != null)
        {
            user.FirstName = @event.FirstName;
            user.LastName = @event.LastName;
            user.LastModifiedAt = @event.UpdatedAt;

            await _userReadRepository.UpdateAsync(user);
            
            _logger.LogInformation("Updated projection for user {UserId}", @event.UserId);
        }
    }

    private async Task HandleUserRoleAdded(UserRoleAddedEvent @event)
    {
        var user = await _userReadRepository.GetByIdAsync(@event.UserId);
        if (user != null && !user.Roles.Contains(@event.Role))
        {
            user.Roles.Add(@event.Role);
            user.LastModifiedAt = @event.AddedAt;

            await _userReadRepository.UpdateAsync(user);
            
            _logger.LogInformation("Added role {Role} to user {UserId}", @event.Role, @event.UserId);
        }
    }

    private async Task HandleUserDeactivated(UserDeactivatedEvent @event)
    {
        var user = await _userReadRepository.GetByIdAsync(@event.UserId);
        if (user != null)
        {
            user.IsActive = false;
            user.LastModifiedAt = @event.DeactivatedAt;

            await _userReadRepository.UpdateAsync(user);
            
            _logger.LogInformation("Deactivated user {UserId}", @event.UserId);
        }
    }
}
```

### Event Handlers y Procesamiento Asíncrono

```csharp
public interface IEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent @event);
}

public class UserEventHandler : IEventHandler<UserCreatedEvent>, 
                               IEventHandler<UserProfileUpdatedEvent>,
                               IEventHandler<UserRoleAddedEvent>
{
    private readonly IProjection _userProjection;
    private readonly IMessageBroker _messageBroker;
    private readonly ILogger<UserEventHandler> _logger;

    public UserEventHandler(
        IProjection userProjection,
        IMessageBroker messageBroker,
        ILogger<UserEventHandler> logger)
    {
        _userProjection = userProjection;
        _messageBroker = messageBroker;
        _logger = logger;
    }

    public async Task HandleAsync(UserCreatedEvent @event)
    {
        // Actualizar proyección
        await _userProjection.HandleAsync(@event);

        // Publicar evento de integración
        var integrationEvent = new UserCreatedIntegrationEvent
        {
            UserId = @event.UserId,
            Email = @event.Email,
            FirstName = @event.FirstName,
            LastName = @event.LastName,
            OccurredOn = @event.OccurredOn
        };

        await _messageBroker.PublishAsync("user.created", integrationEvent);
        
        _logger.LogInformation("Handled UserCreatedEvent for user {UserId}", @event.UserId);
    }

    public async Task HandleAsync(UserProfileUpdatedEvent @event)
    {
        await _userProjection.HandleAsync(@event);

        var integrationEvent = new UserProfileUpdatedIntegrationEvent
        {
            UserId = @event.UserId,
            FirstName = @event.FirstName,
            LastName = @event.LastName,
            OccurredOn = @event.OccurredOn
        };

        await _messageBroker.PublishAsync("user.profile.updated", integrationEvent);
        
        _logger.LogInformation("Handled UserProfileUpdatedEvent for user {UserId}", @event.UserId);
    }

    public async Task HandleAsync(UserRoleAddedEvent @event)
    {
        await _userProjection.HandleAsync(@event);

        var integrationEvent = new UserRoleAddedIntegrationEvent
        {
            UserId = @event.UserId,
            Role = @event.Role,
            OccurredOn = @event.OccurredOn
        };

        await _messageBroker.PublishAsync("user.role.added", integrationEvent);
        
        _logger.LogInformation("Handled UserRoleAddedEvent for user {UserId}", @event.UserId);
    }
}
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Event Store Básico
Implementa un Event Store que incluya:
- Persistencia de eventos
- Recuperación por aggregate ID
- Control de concurrencia
- Serialización de eventos

### Ejercicio 2: Agregado con Event Sourcing
Crea un agregado que use Event Sourcing para:
- Crear entidades
- Modificar estado
- Reconstruir desde eventos
- Manejar versiones

### Ejercicio 3: CQRS Completo
Implementa CQRS con:
- Commands y Command Handlers
- Queries y Query Handlers
- Mediator pattern
- Proyecciones asíncronas

## 📝 Quiz de Autoevaluación

1. ¿Cuáles son las principales ventajas de Event Sourcing sobre el almacenamiento tradicional?
2. ¿Cómo manejarías la concurrencia en un sistema con Event Sourcing?
3. ¿Qué estrategias usarías para optimizar las consultas en CQRS?
4. ¿Cómo implementarías proyecciones que se mantengan sincronizadas con los eventos?
5. ¿Qué consideraciones tendrías para implementar Event Sourcing en microservicios?

## 🔗 Enlaces Útiles

- [Event Sourcing Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/event-sourcing)
- [CQRS Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [MediatR - Simple mediator implementation](https://github.com/jbogard/MediatR)
- [EventStore - Event sourcing database](https://eventstore.com/)

## 🚀 Siguiente Clase

En la siguiente clase aprenderás sobre monitoreo y observabilidad distribuida, técnicas esenciales para mantener y operar sistemas de microservicios en producción.

---

**💡 Consejo**: Event Sourcing y CQRS pueden agregar complejidad significativa. Asegúrate de que los beneficios superen los costos antes de implementarlos.
