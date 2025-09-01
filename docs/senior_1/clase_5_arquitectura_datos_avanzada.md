# 🚀 Clase 5: Arquitectura de Datos Avanzada

## 📋 Información de la Clase

- **Módulo**: Senior 1 - Arquitectura de Software Empresarial
- **Duración**: 2 horas
- **Nivel**: Senior
- **Prerrequisitos**: Completar Clase 4 (Patrones de Diseño Enterprise)

## 🎯 Objetivos de Aprendizaje

- Implementar estrategias de persistencia avanzadas
- Aplicar CQRS avanzado con separación de responsabilidades
- Implementar Event Sourcing con bases de datos
- Diseñar patrones de acceso a datos optimizados

---

## 📚 Navegación del Módulo 6

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_arquitectura_limpia_avanzada.md) | Arquitectura Limpia Avanzada | |
| [Clase 2](clase_2_event_driven_architecture.md) | Event-Driven Architecture | |
| [Clase 3](clase_3_microservicios_avanzada.md) | Arquitectura de Microservicios Avanzada | |
| [Clase 4](clase_4_patrones_enterprise.md) | Patrones de Diseño Enterprise | ← Anterior |
| **Clase 5** | **Arquitectura de Datos Avanzada** | ← Estás aquí |
| [Clase 6](clase_6_calidad_codigo_metricas.md) | Calidad del Código y Métricas | Siguiente → |
| [Clase 7](clase_7_monitoreo_observabilidad.md) | Monitoreo y Observabilidad | |
| [Clase 8](clase_8_arquitectura_evolutiva.md) | Arquitectura Evolutiva | |
| [Clase 9](clase_9_seguridad_enterprise.md) | Arquitectura de Seguridad Enterprise | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Plataforma Empresarial | |

**← [Volver al README del Módulo 6](../senior_1/README.md)**

---

## 📚 Contenido Teórico

### 1. Arquitectura de Datos Avanzada

La arquitectura de datos avanzada se enfoca en optimizar el acceso, almacenamiento y procesamiento de datos en aplicaciones empresariales.

```csharp
// ===== ARQUITECTURA DE DATOS AVANZADA - IMPLEMENTACIÓN COMPLETA =====
namespace AdvancedDataArchitecture
{
    // ===== CQRS AVANZADO =====
    namespace CQRS.Advanced
    {
        public interface ICommand
        {
            Guid Id { get; }
            DateTime Timestamp { get; }
        }
        
        public interface IQuery<TResult>
        {
            Guid Id { get; }
            DateTime Timestamp { get; }
        }
        
        public interface ICommandHandler<in TCommand> where TCommand : ICommand
        {
            Task<CommandResult> HandleAsync(TCommand command);
        }
        
        public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
        {
            Task<QueryResult<TResult>> HandleAsync(TQuery query);
        }
        
        public class CommandResult
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
            public object Data { get; set; }
            public List<string> Errors { get; set; } = new();
            
            public static CommandResult Success(string message = null, object data = null)
            {
                return new CommandResult
                {
                    IsSuccess = true,
                    Message = message,
                    Data = data
                };
            }
            
            public static CommandResult Failure(string message, List<string> errors = null)
            {
                return new CommandResult
                {
                    IsSuccess = false,
                    Message = message,
                    Errors = errors ?? new List<string>()
                };
            }
        }
        
        public class QueryResult<T>
        {
            public bool IsSuccess { get; set; }
            public T Data { get; set; }
            public string Message { get; set; }
            public List<string> Errors { get; set; } = new();
            
            public static QueryResult<T> Success(T data, string message = null)
            {
                return new QueryResult<T>
                {
                    IsSuccess = true,
                    Data = data,
                    Message = message
                };
            }
            
            public static QueryResult<T> Failure(string message, List<string> errors = null)
            {
                return new QueryResult<T>
                {
                    IsSuccess = false,
                    Message = message,
                    Errors = errors ?? new List<string>()
                };
            }
        }
        
        public class CreateUserCommand : ICommand
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public DateTime Timestamp { get; set; } = DateTime.UtcNow;
            public string Username { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }
        
        public class GetUserQuery : IQuery<UserDto>
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public DateTime Timestamp { get; set; } = DateTime.UtcNow;
            public int UserId { get; set; }
        }
        
        public class GetUsersQuery : IQuery<IEnumerable<UserDto>>
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public DateTime Timestamp { get; set; } = DateTime.UtcNow;
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public string SearchTerm { get; set; }
        }
        
        public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
        {
            private readonly IUserWriteRepository _userWriteRepository;
            private readonly IEventStore _eventStore;
            private readonly ILogger<CreateUserCommandHandler> _logger;
            
            public CreateUserCommandHandler(
                IUserWriteRepository userWriteRepository,
                IEventStore eventStore,
                ILogger<CreateUserCommandHandler> logger)
            {
                _userWriteRepository = userWriteRepository;
                _eventStore = eventStore;
                _logger = logger;
            }
            
            public async Task<CommandResult> HandleAsync(CreateUserCommand command)
            {
                try
                {
                    // Validar comando
                    var validationResult = await ValidateCommandAsync(command);
                    if (!validationResult.IsSuccess)
                    {
                        return CommandResult.Failure("Validation failed", validationResult.Errors);
                    }
                    
                    // Crear usuario
                    var user = User.Create(command.Username, command.Email, command.Password);
                    
                    // Guardar en write model
                    await _userWriteRepository.AddAsync(user);
                    
                    // Guardar eventos
                    foreach (var domainEvent in user.DomainEvents)
                    {
                        await _eventStore.SaveEventAsync(domainEvent);
                    }
                    
                    _logger.LogInformation("User created successfully: {UserId}", user.Id);
                    
                    return CommandResult.Success("User created successfully", new { UserId = user.Id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating user: {Username}", command.Username);
                    return CommandResult.Failure("Error creating user", new List<string> { ex.Message });
                }
            }
            
            private async Task<ValidationResult> ValidateCommandAsync(CreateUserCommand command)
            {
                var errors = new List<string>();
                
                if (string.IsNullOrEmpty(command.Username))
                    errors.Add("Username is required");
                
                if (string.IsNullOrEmpty(command.Email))
                    errors.Add("Email is required");
                
                if (string.IsNullOrEmpty(command.Password))
                    errors.Add("Password is required");
                
                if (await _userWriteRepository.ExistsByUsernameAsync(command.Username))
                    errors.Add("Username already exists");
                
                if (await _userWriteRepository.ExistsByEmailAsync(command.Email))
                    errors.Add("Email already exists");
                
                return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
            }
        }
        
        public class GetUserQueryHandler : IQueryHandler<GetUserQuery, UserDto>
        {
            private readonly IUserReadRepository _userReadRepository;
            private readonly ILogger<GetUserQueryHandler> _logger;
            
            public GetUserQueryHandler(
                IUserReadRepository userReadRepository,
                ILogger<GetUserQueryHandler> logger)
            {
                _userReadRepository = userReadRepository;
                _logger = logger;
            }
            
            public async Task<QueryResult<UserDto>> HandleAsync(GetUserQuery query)
            {
                try
                {
                    var user = await _userReadRepository.GetByIdAsync(query.UserId);
                    
                    if (user == null)
                    {
                        return QueryResult<UserDto>.Failure("User not found");
                    }
                    
                    return QueryResult<UserDto>.Success(user);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting user: {UserId}", query.UserId);
                    return QueryResult<UserDto>.Failure("Error getting user", new List<string> { ex.Message });
                }
            }
        }
        
        public class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, IEnumerable<UserDto>>
        {
            private readonly IUserReadRepository _userReadRepository;
            private readonly ILogger<GetUsersQueryHandler> _logger;
            
            public GetUsersQueryHandler(
                IUserReadRepository userReadRepository,
                ILogger<GetUsersQueryHandler> logger)
            {
                _userReadRepository = userReadRepository;
                _logger = logger;
            }
            
            public async Task<QueryResult<IEnumerable<UserDto>>> HandleAsync(GetUsersQuery query)
            {
                try
                {
                    var users = await _userReadRepository.GetAllAsync(query.Page, query.PageSize, query.SearchTerm);
                    
                    return QueryResult<IEnumerable<UserDto>>.Success(users);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting users");
                    return QueryResult<IEnumerable<UserDto>>.Failure("Error getting users", new List<string> { ex.Message });
                }
            }
        }
    }
    
    // ===== EVENT SOURCING AVANZADO =====
    namespace EventSourcing.Advanced
    {
        public interface IEventStore
        {
            Task SaveEventsAsync(string aggregateId, IEnumerable<IDomainEvent> events, int expectedVersion);
            Task<IEnumerable<IDomainEvent>> GetEventsAsync(string aggregateId);
            Task<int> GetLatestVersionAsync(string aggregateId);
            Task<IEnumerable<IDomainEvent>> GetEventsByTypeAsync(string eventType);
            Task<IEnumerable<IDomainEvent>> GetEventsByDateRangeAsync(DateTime from, DateTime to);
        }
        
        public class SqlEventStore : IEventStore
        {
            private readonly ApplicationDbContext _context;
            private readonly ILogger<SqlEventStore> _logger;
            private readonly IEventSerializer _eventSerializer;
            
            public SqlEventStore(
                ApplicationDbContext context,
                ILogger<SqlEventStore> logger,
                IEventSerializer eventSerializer)
            {
                _context = context;
                _logger = logger;
                _eventSerializer = eventSerializer;
            }
            
            public async Task SaveEventsAsync(string aggregateId, IEnumerable<IDomainEvent> events, int expectedVersion)
            {
                var eventList = events.ToList();
                var version = expectedVersion;
                
                foreach (var @event in eventList)
                {
                    version++;
                    @event.Version = version;
                    
                    var eventData = new EventData
                    {
                        AggregateId = aggregateId,
                        Version = version,
                        EventType = @event.GetType().Name,
                        EventData = _eventSerializer.Serialize(@event),
                        OccurredOn = @event.OccurredOn,
                        Metadata = _eventSerializer.SerializeMetadata(@event)
                    };
                    
                    _context.Events.Add(eventData);
                }
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Saved {EventCount} events for aggregate {AggregateId}", 
                    eventList.Count, aggregateId);
            }
            
            public async Task<IEnumerable<IDomainEvent>> GetEventsAsync(string aggregateId)
            {
                var eventDataList = await _context.Events
                    .Where(e => e.AggregateId == aggregateId)
                    .OrderBy(e => e.Version)
                    .ToListAsync();
                
                var events = new List<IDomainEvent>();
                
                foreach (var eventData in eventDataList)
                {
                    var @event = _eventSerializer.Deserialize(eventData.EventData, eventData.EventType);
                    if (@event != null)
                    {
                        events.Add(@event);
                    }
                }
                
                return events;
            }
            
            public async Task<int> GetLatestVersionAsync(string aggregateId)
            {
                var latestEvent = await _context.Events
                    .Where(e => e.AggregateId == aggregateId)
                    .OrderByDescending(e => e.Version)
                    .FirstOrDefaultAsync();
                
                return latestEvent?.Version ?? 0;
            }
            
            public async Task<IEnumerable<IDomainEvent>> GetEventsByTypeAsync(string eventType)
            {
                var eventDataList = await _context.Events
                    .Where(e => e.EventType == eventType)
                    .OrderBy(e => e.OccurredOn)
                    .ToListAsync();
                
                var events = new List<IDomainEvent>();
                
                foreach (var eventData in eventDataList)
                {
                    var @event = _eventSerializer.Deserialize(eventData.EventData, eventData.EventType);
                    if (@event != null)
                    {
                        events.Add(@event);
                    }
                }
                
                return events;
            }
            
            public async Task<IEnumerable<IDomainEvent>> GetEventsByDateRangeAsync(DateTime from, DateTime to)
            {
                var eventDataList = await _context.Events
                    .Where(e => e.OccurredOn >= from && e.OccurredOn <= to)
                    .OrderBy(e => e.OccurredOn)
                    .ToListAsync();
                
                var events = new List<IDomainEvent>();
                
                foreach (var eventData in eventDataList)
                {
                    var @event = _eventSerializer.Deserialize(eventData.EventData, eventData.EventType);
                    if (@event != null)
                    {
                        events.Add(@event);
                    }
                }
                
                return events;
            }
        }
        
        public interface IEventSerializer
        {
            string Serialize(IDomainEvent domainEvent);
            IDomainEvent Deserialize(string eventData, string eventType);
            string SerializeMetadata(IDomainEvent domainEvent);
        }
        
        public class JsonEventSerializer : IEventSerializer
        {
            private readonly Dictionary<string, Type> _eventTypes;
            
            public JsonEventSerializer()
            {
                _eventTypes = new Dictionary<string, Type>
                {
                    { "UserCreatedEvent", typeof(UserCreatedEvent) },
                    { "UserUpdatedEvent", typeof(UserUpdatedEvent) },
                    { "OrderCreatedEvent", typeof(OrderCreatedEvent) },
                    { "OrderStatusChangedEvent", typeof(OrderStatusChangedEvent) }
                };
            }
            
            public string Serialize(IDomainEvent domainEvent)
            {
                return JsonSerializer.Serialize(domainEvent, domainEvent.GetType());
            }
            
            public IDomainEvent Deserialize(string eventData, string eventType)
            {
                if (_eventTypes.TryGetValue(eventType, out var type))
                {
                    return (IDomainEvent)JsonSerializer.Deserialize(eventData, type);
                }
                
                throw new InvalidOperationException($"Unknown event type: {eventType}");
            }
            
            public string SerializeMetadata(IDomainEvent domainEvent)
            {
                var metadata = new
                {
                    EventType = domainEvent.GetType().Name,
                    AggregateId = domainEvent.AggregateId,
                    Version = domainEvent.Version,
                    OccurredOn = domainEvent.OccurredOn
                };
                
                return JsonSerializer.Serialize(metadata);
            }
        }
        
        public class EventData
        {
            public int Id { get; set; }
            public string AggregateId { get; set; }
            public int Version { get; set; }
            public string EventType { get; set; }
            public string EventData { get; set; }
            public string Metadata { get; set; }
            public DateTime OccurredOn { get; set; }
        }
    }
    
    // ===== PATRONES DE ACCESO A DATOS =====
    namespace DataAccess.Patterns
    {
        public interface IRepository<TEntity, TKey> where TEntity : class
        {
            Task<TEntity> GetByIdAsync(TKey id);
            Task<IEnumerable<TEntity>> GetAllAsync();
            Task<TEntity> AddAsync(TEntity entity);
            Task<TEntity> UpdateAsync(TEntity entity);
            Task<bool> DeleteAsync(TKey id);
            Task<bool> ExistsAsync(TKey id);
        }
        
        public abstract class RepositoryBase<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class
        {
            protected readonly ApplicationDbContext _context;
            protected readonly ILogger<RepositoryBase<TEntity, TKey>> _logger;
            
            protected RepositoryBase(ApplicationDbContext context, ILogger<RepositoryBase<TEntity, TKey>> logger)
            {
                _context = context;
                _logger = logger;
            }
            
            public virtual async Task<TEntity> GetByIdAsync(TKey id)
            {
                return await _context.Set<TEntity>().FindAsync(id);
            }
            
            public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
            {
                return await _context.Set<TEntity>().ToListAsync();
            }
            
            public virtual async Task<TEntity> AddAsync(TEntity entity)
            {
                var entry = _context.Set<TEntity>().Add(entity);
                await _context.SaveChangesAsync();
                return entry.Entity;
            }
            
            public virtual async Task<TEntity> UpdateAsync(TEntity entity)
            {
                var entry = _context.Set<TEntity>().Update(entity);
                await _context.SaveChangesAsync();
                return entry.Entity;
            }
            
            public virtual async Task<bool> DeleteAsync(TKey id)
            {
                var entity = await GetByIdAsync(id);
                if (entity == null)
                    return false;
                
                _context.Set<TEntity>().Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            
            public virtual async Task<bool> ExistsAsync(TKey id)
            {
                return await _context.Set<TEntity>().FindAsync(id) != null;
            }
        }
        
        public interface IUserWriteRepository : IRepository<User, int>
        {
            Task<bool> ExistsByUsernameAsync(string username);
            Task<bool> ExistsByEmailAsync(string email);
        }
        
        public interface IUserReadRepository
        {
            Task<UserDto> GetByIdAsync(int id);
            Task<IEnumerable<UserDto>> GetAllAsync(int page, int pageSize, string searchTerm = null);
            Task<UserDto> GetByUsernameAsync(string username);
            Task<UserDto> GetByEmailAsync(string email);
        }
        
        public class UserWriteRepository : RepositoryBase<User, int>, IUserWriteRepository
        {
            public UserWriteRepository(ApplicationDbContext context, ILogger<UserWriteRepository> logger) 
                : base(context, logger) { }
            
            public async Task<bool> ExistsByUsernameAsync(string username)
            {
                return await _context.Users.AnyAsync(u => u.Username == username);
            }
            
            public async Task<bool> ExistsByEmailAsync(string email)
            {
                return await _context.Users.AnyAsync(u => u.Email == email);
            }
        }
        
        public class UserReadRepository : IUserReadRepository
        {
            private readonly ApplicationDbContext _context;
            private readonly ILogger<UserReadRepository> _logger;
            
            public UserReadRepository(ApplicationDbContext context, ILogger<UserReadRepository> logger)
            {
                _context = context;
                _logger = logger;
            }
            
            public async Task<UserDto> GetByIdAsync(int id)
            {
                var user = await _context.Users
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Id == id);
                
                return user != null ? MapToDto(user) : null;
            }
            
            public async Task<IEnumerable<UserDto>> GetAllAsync(int page, int pageSize, string searchTerm = null)
            {
                var query = _context.Users.Include(u => u.Roles).AsQueryable();
                
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(u => u.Username.Contains(searchTerm) || u.Email.Contains(searchTerm));
                }
                
                var users = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                
                return users.Select(MapToDto);
            }
            
            public async Task<UserDto> GetByUsernameAsync(string username)
            {
                var user = await _context.Users
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Username == username);
                
                return user != null ? MapToDto(user) : null;
            }
            
            public async Task<UserDto> GetByEmailAsync(string email)
            {
                var user = await _context.Users
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Email == email);
                
                return user != null ? MapToDto(user) : null;
            }
            
            private UserDto MapToDto(User user)
            {
                return new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Status = user.Status.ToString(),
                    Roles = user.Roles.Select(r => r.ToString()).ToList(),
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };
            }
        }
    }
    
    // ===== UNIT OF WORK AVANZADO =====
    namespace UnitOfWork.Advanced
    {
        public interface IUnitOfWork : IDisposable
        {
            IUserWriteRepository Users { get; }
            IOrderWriteRepository Orders { get; }
            IProductWriteRepository Products { get; }
            
            Task<int> SaveChangesAsync();
            Task BeginTransactionAsync();
            Task CommitTransactionAsync();
            Task RollbackTransactionAsync();
        }
        
        public class UnitOfWork : IUnitOfWork
        {
            private readonly ApplicationDbContext _context;
            private readonly ILogger<UnitOfWork> _logger;
            private IDbContextTransaction _transaction;
            
            private IUserWriteRepository _users;
            private IOrderWriteRepository _orders;
            private IProductWriteRepository _products;
            
            public UnitOfWork(ApplicationDbContext context, ILogger<UnitOfWork> logger)
            {
                _context = context;
                _logger = logger;
            }
            
            public IUserWriteRepository Users => _users ??= new UserWriteRepository(_context, _logger);
            public IOrderWriteRepository Orders => _orders ??= new OrderWriteRepository(_context, _logger);
            public IProductWriteRepository Products => _products ??= new ProductWriteRepository(_context, _logger);
            
            public async Task<int> SaveChangesAsync()
            {
                try
                {
                    var result = await _context.SaveChangesAsync();
                    _logger.LogInformation("Unit of Work saved {Count} changes", result);
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving changes in Unit of Work");
                    throw;
                }
            }
            
            public async Task BeginTransactionAsync()
            {
                _transaction = await _context.Database.BeginTransactionAsync();
                _logger.LogInformation("Transaction begun in Unit of Work");
            }
            
            public async Task CommitTransactionAsync()
            {
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                    _logger.LogInformation("Transaction committed in Unit of Work");
                }
            }
            
            public async Task RollbackTransactionAsync()
            {
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync();
                    _logger.LogWarning("Transaction rolled back in Unit of Work");
                }
            }
            
            public void Dispose()
            {
                _transaction?.Dispose();
                _context?.Dispose();
            }
        }
    }
    
    // ===== SPECIFICATION PATTERN =====
    namespace SpecificationPattern
    {
        public interface ISpecification<T>
        {
            bool IsSatisfiedBy(T entity);
            ISpecification<T> And(ISpecification<T> specification);
            ISpecification<T> Or(ISpecification<T> specification);
            ISpecification<T> Not();
        }
        
        public abstract class Specification<T> : ISpecification<T>
        {
            public abstract bool IsSatisfiedBy(T entity);
            
            public ISpecification<T> And(ISpecification<T> specification)
            {
                return new AndSpecification<T>(this, specification);
            }
            
            public ISpecification<T> Or(ISpecification<T> specification)
            {
                return new OrSpecification<T>(this, specification);
            }
            
            public ISpecification<T> Not()
            {
                return new NotSpecification<T>(this);
            }
        }
        
        public class AndSpecification<T> : Specification<T>
        {
            private readonly ISpecification<T> _left;
            private readonly ISpecification<T> _right;
            
            public AndSpecification(ISpecification<T> left, ISpecification<T> right)
            {
                _left = left;
                _right = right;
            }
            
            public override bool IsSatisfiedBy(T entity)
            {
                return _left.IsSatisfiedBy(entity) && _right.IsSatisfiedBy(entity);
            }
        }
        
        public class OrSpecification<T> : Specification<T>
        {
            private readonly ISpecification<T> _left;
            private readonly ISpecification<T> _right;
            
            public OrSpecification(ISpecification<T> left, ISpecification<T> right)
            {
                _left = left;
                _right = right;
            }
            
            public override bool IsSatisfiedBy(T entity)
            {
                return _left.IsSatisfiedBy(entity) || _right.IsSatisfiedBy(entity);
            }
        }
        
        public class NotSpecification<T> : Specification<T>
        {
            private readonly ISpecification<T> _specification;
            
            public NotSpecification(ISpecification<T> specification)
            {
                _specification = specification;
            }
            
            public override bool IsSatisfiedBy(T entity)
            {
                return !_specification.IsSatisfiedBy(entity);
            }
        }
        
        public class ActiveUserSpecification : Specification<User>
        {
            public override bool IsSatisfiedBy(User entity)
            {
                return entity.Status == UserStatus.Active;
            }
        }
        
        public class UserByRoleSpecification : Specification<User>
        {
            private readonly Role _role;
            
            public UserByRoleSpecification(Role role)
            {
                _role = role;
            }
            
            public override bool IsSatisfiedBy(User entity)
            {
                return entity.Roles.Contains(_role);
            }
        }
        
        public class UserByEmailDomainSpecification : Specification<User>
        {
            private readonly string _domain;
            
            public UserByEmailDomainSpecification(string domain)
            {
                _domain = domain;
            }
            
            public override bool IsSatisfiedBy(User entity)
            {
                return entity.Email.EndsWith($"@{_domain}");
            }
        }
    }
    
    // ===== DEPENDENCY INJECTION =====
    namespace Infrastructure.DependencyInjection
    {
        public static class ServiceCollectionExtensions
        {
            public static IServiceCollection AddAdvancedDataArchitecture(this IServiceCollection services)
            {
                // CQRS
                services.AddScoped<ICommandHandler<CreateUserCommand>, CreateUserCommandHandler>();
                services.AddScoped<IQueryHandler<GetUserQuery, UserDto>, GetUserQueryHandler>();
                services.AddScoped<IQueryHandler<GetUsersQuery, IEnumerable<UserDto>>, GetUsersQueryHandler>();
                
                // Event Sourcing
                services.AddScoped<IEventStore, SqlEventStore>();
                services.AddScoped<IEventSerializer, JsonEventSerializer>();
                
                // Data Access
                services.AddScoped<IUserWriteRepository, UserWriteRepository>();
                services.AddScoped<IUserReadRepository, UserReadRepository>();
                services.AddScoped<IOrderWriteRepository, OrderWriteRepository>();
                services.AddScoped<IProductWriteRepository, ProductWriteRepository>();
                
                // Unit of Work
                services.AddScoped<IUnitOfWork, UnitOfWork>();
                
                return services;
            }
        }
    }
}

// Uso de Arquitectura de Datos Avanzada
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Arquitectura de Datos Avanzada ===\n");
        
        Console.WriteLine("Los componentes implementados incluyen:");
        Console.WriteLine("1. CQRS Avanzado con separación de responsabilidades");
        Console.WriteLine("2. Event Sourcing con serialización JSON");
        Console.WriteLine("3. Patrones de acceso a datos optimizados");
        Console.WriteLine("4. Unit of Work con transacciones");
        Console.WriteLine("5. Specification Pattern para consultas complejas");
        Console.WriteLine("6. Repositorios separados para lectura y escritura");
        
        Console.WriteLine("\nBeneficios de esta arquitectura:");
        Console.WriteLine("- Separación clara entre comandos y consultas");
        Console.WriteLine("- Trazabilidad completa de cambios");
        Console.WriteLine("- Optimización de consultas de lectura");
        Console.WriteLine("- Manejo robusto de transacciones");
        Console.WriteLine("- Flexibilidad en consultas complejas");
        Console.WriteLine("- Escalabilidad de la capa de datos");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Implementar CQRS Completo
Crea un sistema CQRS completo para una entidad de negocio.

### Ejercicio 2: Event Sourcing con Snapshots
Implementa Event Sourcing con sistema de snapshots para optimización.

### Ejercicio 3: Specification Pattern Avanzado
Crea especificaciones complejas combinando múltiples criterios.

## 🔍 Puntos Clave

1. **CQRS** separa comandos de consultas para optimización
2. **Event Sourcing** mantiene historial completo de cambios
3. **Unit of Work** maneja transacciones de manera coherente
4. **Specification Pattern** encapsula lógica de consultas complejas
5. **Repositorios separados** optimizan lectura y escritura

## 📚 Recursos Adicionales

- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [Event Sourcing](https://martinfowler.com/eaaDev/EventSourcing.html)
- [Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html)

---

**🎯 ¡Has completado la Clase 5! Ahora comprendes Arquitectura de Datos Avanzada**

**📚 [Siguiente: Clase 6 - Calidad del Código y Métricas](clase_6_calidad_codigo_metricas.md)**
