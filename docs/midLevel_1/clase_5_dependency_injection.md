# 🚀 Clase 5: Dependency Injection Avanzada

## 📋 Información de la Clase

- **Módulo**: Mid Level 1 - Programación Avanzada y Patrones de Diseño
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 4 (Clean Architecture)

## 🎯 Objetivos de Aprendizaje

- Dominar técnicas avanzadas de Dependency Injection en C#
- Implementar diferentes service lifetimes (Singleton, Scoped, Transient)
- Utilizar Factory Pattern con DI containers
- Implementar decorator pattern con inyección de dependencias
- Crear sistemas extensibles con interceptores

---

## 📚 Navegación del Módulo 4

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_patrones_diseno_intermedios.md) | Patrones de Diseño Intermedios | |
| [Clase 2](clase_2_programacion_asincrona_avanzada.md) | Programación Asíncrona Avanzada | |
| [Clase 3](clase_3_programacion_paralela.md) | Programación Paralela y TPL | |
| [Clase 4](clase_4_clean_architecture.md) | Clean Architecture | ← Anterior |
| **Clase 5** | **Dependency Injection Avanzada** | ← Estás aquí |
| [Clase 6](clase_6_logging_monitoreo.md) | Logging y Monitoreo | Siguiente → |
| [Clase 7](clase_7_refactoring_clean_code.md) | Refactoring y Clean Code | |
| [Clase 8](clase_8_testing_integracion.md) | Testing de Integración | |
| [Clase 9](clase_9_testing_comportamiento.md) | Testing de Comportamiento (BDD) | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de E-commerce | |

**← [Volver al README del Módulo 4](../midLevel_1/README.md)**

---

## 📚 Contenido Teórico

### 1. Service Lifetimes y Configuración Avanzada

Los service lifetimes determinan cómo se crean y gestionan las instancias de los servicios.

```csharp
// Sistema de Dependency Injection avanzado
namespace AdvancedDI
{
    // ===== INTERFACES DE SERVICIOS =====
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string body);
    }
    
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(Guid id);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(Guid id);
    }
    
    public interface IUserService
    {
        Task<User> CreateUserAsync(string email, string firstName, string lastName);
        Task<User> GetUserAsync(Guid id);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task UpdateUserAsync(Guid id, string firstName, string lastName);
        Task DeleteUserAsync(Guid id);
    }
    
    public interface IAuditService
    {
        Task LogActionAsync(string action, string userId, string details);
        Task<IEnumerable<AuditLog>> GetAuditLogsAsync(DateTime startDate, DateTime endDate);
    }
    
    public interface ICacheService
    {
        Task<T> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
    }
    
    public interface IConfigurationService
    {
        string GetConnectionString(string name);
        T GetValue<T>(string key, T defaultValue = default);
        bool GetFeatureFlag(string featureName);
    }
    
    // ===== ENTIDADES =====
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        public string FullName => $"{FirstName} {LastName}";
    }
    
    public class AuditLog
    {
        public Guid Id { get; set; }
        public string Action { get; set; }
        public string UserId { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; }
    }
    
    // ===== IMPLEMENTACIONES DE SERVICIOS =====
    namespace Services
    {
        // Servicio de email con lifetime Singleton
        public class EmailService : IEmailService
        {
            private readonly IConfigurationService _configuration;
            private readonly IAuditService _auditService;
            private readonly string _smtpServer;
            private readonly int _smtpPort;
            private readonly string _fromEmail;
            
            public EmailService(IConfigurationService configuration, IAuditService auditService)
            {
                _configuration = configuration;
                _auditService = auditService;
                _smtpServer = _configuration.GetValue<string>("Smtp:Server", "localhost");
                _smtpPort = _configuration.GetValue<int>("Smtp:Port", 587);
                _fromEmail = _configuration.GetValue<string>("Smtp:FromEmail", "noreply@company.com");
                
                Console.WriteLine($"EmailService creado - SMTP: {_smtpServer}:{_smtpPort}");
            }
            
            public async Task SendEmailAsync(string to, string subject, string body)
            {
                // Simular envío de email
                await Task.Delay(100);
                Console.WriteLine($"Email enviado a {to}: {subject}");
                
                // Registrar en auditoría
                await _auditService.LogActionAsync("EmailSent", "System", $"Email enviado a {to}");
            }
            
            public async Task SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string body)
            {
                var recipientList = recipients.ToList();
                Console.WriteLine($"Enviando {recipientList.Count} emails masivos");
                
                foreach (var recipient in recipientList)
                {
                    await SendEmailAsync(recipient, subject, body);
                }
                
                await _auditService.LogActionAsync("BulkEmailSent", "System", 
                    $"{recipientList.Count} emails enviados");
            }
        }
        
        // Servicio de usuarios con lifetime Scoped
        public class UserService : IUserService
        {
            private readonly IUserRepository _userRepository;
            private readonly IEmailService _emailService;
            private readonly IAuditService _auditService;
            private readonly ICacheService _cacheService;
            private readonly Guid _serviceId;
            
            public UserService(
                IUserRepository userRepository,
                IEmailService emailService,
                IAuditService auditService,
                ICacheService cacheService)
            {
                _userRepository = userRepository;
                _emailService = emailService;
                _auditService = auditService;
                _cacheService = cacheService;
                _serviceId = Guid.NewGuid();
                
                Console.WriteLine($"UserService creado con ID: {_serviceId}");
            }
            
            public async Task<User> CreateUserAsync(string email, string firstName, string lastName)
            {
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                
                var createdUser = await _userRepository.AddAsync(user);
                
                // Enviar email de bienvenida
                await _emailService.SendEmailAsync(email, "Bienvenido", 
                    $"Hola {firstName}, tu cuenta ha sido creada exitosamente.");
                
                // Registrar en auditoría
                await _auditService.LogActionAsync("UserCreated", user.Id.ToString(), 
                    $"Usuario creado: {email}");
                
                // Limpiar cache
                await _cacheService.RemoveAsync("users_all");
                
                return createdUser;
            }
            
            public async Task<User> GetUserAsync(Guid id)
            {
                // Intentar obtener del cache primero
                var cacheKey = $"user_{id}";
                var cachedUser = await _cacheService.GetAsync<User>(cacheKey);
                if (cachedUser != null)
                {
                    Console.WriteLine($"Usuario obtenido del cache: {id}");
                    return cachedUser;
                }
                
                // Si no está en cache, obtener de la base de datos
                var user = await _userRepository.GetByIdAsync(id);
                if (user != null)
                {
                    // Guardar en cache por 30 minutos
                    await _cacheService.SetAsync(cacheKey, user, TimeSpan.FromMinutes(30));
                }
                
                return user;
            }
            
            public async Task<IEnumerable<User>> GetAllUsersAsync()
            {
                // Intentar obtener del cache primero
                var cachedUsers = await _cacheService.GetAsync<IEnumerable<User>>("users_all");
                if (cachedUsers != null)
                {
                    Console.WriteLine("Usuarios obtenidos del cache");
                    return cachedUsers;
                }
                
                // Si no está en cache, obtener de la base de datos
                var users = await _userRepository.GetAllAsync();
                
                // Guardar en cache por 15 minutos
                await _cacheService.SetAsync("users_all", users, TimeSpan.FromMinutes(15));
                
                return users;
            }
            
            public async Task UpdateUserAsync(Guid id, string firstName, string lastName)
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                    throw new ArgumentException("Usuario no encontrado");
                
                user.FirstName = firstName;
                user.LastName = lastName;
                user.UpdatedAt = DateTime.UtcNow;
                
                await _userRepository.UpdateAsync(user);
                
                // Registrar en auditoría
                await _auditService.LogActionAsync("UserUpdated", id.ToString(), 
                    $"Usuario actualizado: {firstName} {lastName}");
                
                // Limpiar cache
                await _cacheService.RemoveAsync($"user_{id}");
                await _cacheService.RemoveAsync("users_all");
            }
            
            public async Task DeleteUserAsync(Guid id)
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                    throw new ArgumentException("Usuario no encontrado");
                
                await _userRepository.DeleteAsync(id);
                
                // Registrar en auditoría
                await _auditService.LogActionAsync("UserDeleted", id.ToString(), 
                    $"Usuario eliminado: {user.Email}");
                
                // Limpiar cache
                await _cacheService.RemoveAsync($"user_{id}");
                await _cacheService.RemoveAsync("users_all");
            }
        }
        
        // Servicio de auditoría con lifetime Transient
        public class AuditService : IAuditService
        {
            private readonly IConfigurationService _configuration;
            private readonly List<AuditLog> _logs;
            private readonly Guid _serviceId;
            
            public AuditService(IConfigurationService configuration)
            {
                _configuration = configuration;
                _logs = new List<AuditLog>();
                _serviceId = Guid.NewGuid();
                
                Console.WriteLine($"AuditService creado con ID: {_serviceId}");
            }
            
            public async Task LogActionAsync(string action, string userId, string details)
            {
                var log = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    Action = action,
                    UserId = userId,
                    Details = details,
                    Timestamp = DateTime.UtcNow,
                    IpAddress = "127.0.0.1" // En una implementación real se obtendría del contexto
                };
                
                _logs.Add(log);
                Console.WriteLine($"Acción registrada: {action} por {userId} - {details}");
                
                await Task.CompletedTask;
            }
            
            public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(DateTime startDate, DateTime endDate)
            {
                var filteredLogs = _logs.Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate);
                return await Task.FromResult(filteredLogs);
            }
        }
        
        // Servicio de cache con lifetime Singleton
        public class CacheService : ICacheService
        {
            private readonly Dictionary<string, (object Value, DateTime Expiration)> _cache;
            private readonly IConfigurationService _configuration;
            private readonly int _defaultExpirationMinutes;
            
            public CacheService(IConfigurationService configuration)
            {
                _cache = new Dictionary<string, (object, DateTime)>();
                _configuration = configuration;
                _defaultExpirationMinutes = _configuration.GetValue<int>("Cache:DefaultExpirationMinutes", 30);
                
                Console.WriteLine($"CacheService creado - Expiración por defecto: {_defaultExpirationMinutes} minutos");
            }
            
            public async Task<T> GetAsync<T>(string key)
            {
                if (_cache.TryGetValue(key, out var cacheEntry))
                {
                    if (cacheEntry.Expiration > DateTime.UtcNow)
                    {
                        Console.WriteLine($"Cache hit para clave: {key}");
                        return (T)cacheEntry.Value;
                    }
                    else
                    {
                        // Expiró, remover
                        _cache.Remove(key);
                        Console.WriteLine($"Cache expirado para clave: {key}");
                    }
                }
                
                Console.WriteLine($"Cache miss para clave: {key}");
                return default(T);
            }
            
            public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
            {
                var expirationTime = DateTime.UtcNow.Add(expiration ?? TimeSpan.FromMinutes(_defaultExpirationMinutes));
                _cache[key] = (value, expirationTime);
                
                Console.WriteLine($"Valor guardado en cache: {key} (expira: {expirationTime:HH:mm:ss})");
                await Task.CompletedTask;
            }
            
            public async Task RemoveAsync(string key)
            {
                if (_cache.Remove(key))
                {
                    Console.WriteLine($"Clave removida del cache: {key}");
                }
                await Task.CompletedTask;
            }
            
            public async Task<bool> ExistsAsync(string key)
            {
                if (_cache.TryGetValue(key, out var cacheEntry))
                {
                    return cacheEntry.Expiration > DateTime.UtcNow;
                }
                return false;
            }
            
            public void ClearExpiredEntries()
            {
                var expiredKeys = _cache.Where(kvp => kvp.Value.Expiration <= DateTime.UtcNow)
                                      .Select(kvp => kvp.Key)
                                      .ToList();
                
                foreach (var key in expiredKeys)
                {
                    _cache.Remove(key);
                }
                
                if (expiredKeys.Any())
                {
                    Console.WriteLine($"Removidas {expiredKeys.Count} entradas expiradas del cache");
                }
            }
        }
        
        // Servicio de configuración con lifetime Singleton
        public class ConfigurationService : IConfigurationService
        {
            private readonly Dictionary<string, object> _configuration;
            
            public ConfigurationService()
            {
                _configuration = new Dictionary<string, object>
                {
                    ["ConnectionStrings:Default"] = "Server=localhost;Database=MyApp;Trusted_Connection=true;",
                    ["ConnectionStrings:Audit"] = "Server=localhost;Database=Audit;Trusted_Connection=true;",
                    ["Smtp:Server"] = "smtp.gmail.com",
                    ["Smtp:Port"] = 587,
                    ["Smtp:FromEmail"] = "noreply@myapp.com",
                    ["Cache:DefaultExpirationMinutes"] = 30,
                    ["Features:EmailNotifications"] = true,
                    ["Features:AuditLogging"] = true,
                    ["Features:CacheEnabled"] = true
                };
                
                Console.WriteLine("ConfigurationService creado");
            }
            
            public string GetConnectionString(string name)
            {
                var key = $"ConnectionStrings:{name}";
                return _configuration.TryGetValue(key, out var value) ? value.ToString() : null;
            }
            
            public T GetValue<T>(string key, T defaultValue = default)
            {
                if (_configuration.TryGetValue(key, out var value))
                {
                    try
                    {
                        return (T)Convert.ChangeType(value, typeof(T));
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }
                return defaultValue;
            }
            
            public bool GetFeatureFlag(string featureName)
            {
                var key = $"Features:{featureName}";
                return GetValue(key, false);
            }
        }
    }
    
    // ===== REPOSITORIOS =====
    namespace Repositories
    {
        // Repositorio en memoria con lifetime Scoped
        public class InMemoryUserRepository : IUserRepository
        {
            private readonly Dictionary<Guid, User> _users;
            private readonly IAuditService _auditService;
            private readonly Guid _repositoryId;
            
            public InMemoryUserRepository(IAuditService auditService)
            {
                _users = new Dictionary<Guid, User>();
                _auditService = auditService;
                _repositoryId = Guid.NewGuid();
                
                Console.WriteLine($"InMemoryUserRepository creado con ID: {_repositoryId}");
                
                // Agregar algunos usuarios de prueba
                SeedData();
            }
            
            private void SeedData()
            {
                var users = new[]
                {
                    new User { Id = Guid.NewGuid(), Email = "admin@company.com", FirstName = "Admin", LastName = "User", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new User { Id = Guid.NewGuid(), Email = "user1@company.com", FirstName = "John", LastName = "Doe", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new User { Id = Guid.NewGuid(), Email = "user2@company.com", FirstName = "Jane", LastName = "Smith", IsActive = true, CreatedAt = DateTime.UtcNow }
                };
                
                foreach (var user in users)
                {
                    _users[user.Id] = user;
                }
            }
            
            public async Task<User> GetByIdAsync(Guid id)
            {
                await Task.CompletedTask;
                return _users.TryGetValue(id, out var user) ? user : null;
            }
            
            public async Task<IEnumerable<User>> GetAllAsync()
            {
                await Task.CompletedTask;
                return _users.Values.ToList();
            }
            
            public async Task<User> AddAsync(User user)
            {
                await Task.CompletedTask;
                _users[user.Id] = user;
                
                await _auditService.LogActionAsync("UserAdded", "System", 
                    $"Usuario agregado al repositorio: {user.Email}");
                
                return user;
            }
            
            public async Task UpdateAsync(User user)
            {
                await Task.CompletedTask;
                if (_users.ContainsKey(user.Id))
                {
                    _users[user.Id] = user;
                    
                    await _auditService.LogActionAsync("UserUpdated", "System", 
                        $"Usuario actualizado en repositorio: {user.Email}");
                }
            }
            
            public async Task DeleteAsync(Guid id)
            {
                await Task.CompletedTask;
                if (_users.TryGetValue(id, out var user))
                {
                    _users.Remove(id);
                    
                    await _auditService.LogActionAsync("UserDeleted", "System", 
                        $"Usuario eliminado del repositorio: {user.Email}");
                }
            }
        }
    }
    
    // ===== FACTORY PATTERN =====
    namespace Factories
    {
        public interface IUserServiceFactory
        {
            IUserService CreateUserService(string serviceType);
        }
        
        public class UserServiceFactory : IUserServiceFactory
        {
            private readonly IServiceProvider _serviceProvider;
            
            public UserServiceFactory(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }
            
            public IUserService CreateUserService(string serviceType)
            {
                return serviceType.ToLower() switch
                {
                    "standard" => _serviceProvider.GetRequiredService<IUserService>(),
                    "admin" => new AdminUserService(
                        _serviceProvider.GetRequiredService<IUserRepository>(),
                        _serviceProvider.GetRequiredService<IEmailService>(),
                        _serviceProvider.GetRequiredService<IAuditService>(),
                        _serviceProvider.GetRequiredService<ICacheService>()
                    ),
                    "readonly" => new ReadOnlyUserService(
                        _serviceProvider.GetRequiredService<IUserRepository>(),
                        _serviceProvider.GetRequiredService<ICacheService>()
                    ),
                    _ => throw new ArgumentException($"Tipo de servicio no válido: {serviceType}")
                };
            }
        }
        
        // Implementaciones especializadas
        public class AdminUserService : IUserService
        {
            private readonly IUserRepository _userRepository;
            private readonly IEmailService _emailService;
            private readonly IAuditService _auditService;
            private readonly ICacheService _cacheService;
            
            public AdminUserService(IUserRepository userRepository, IEmailService emailService, 
                IAuditService auditService, ICacheService cacheService)
            {
                _userRepository = userRepository;
                _emailService = emailService;
                _auditService = auditService;
                _cacheService = cacheService;
            }
            
            public async Task<User> CreateUserAsync(string email, string firstName, string lastName)
            {
                var user = await _userRepository.AddAsync(new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
                
                // Enviar email de bienvenida especial para admin
                await _emailService.SendEmailAsync(email, "Bienvenido Administrador", 
                    $"Hola {firstName}, tu cuenta de administrador ha sido creada.");
                
                await _auditService.LogActionAsync("AdminUserCreated", user.Id.ToString(), 
                    $"Usuario administrador creado: {email}");
                
                return user;
            }
            
            public async Task<User> GetUserAsync(Guid id) => await _userRepository.GetByIdAsync(id);
            public async Task<IEnumerable<User>> GetAllUsersAsync() => await _userRepository.GetAllAsync();
            public async Task UpdateUserAsync(Guid id, string firstName, string lastName) => await Task.CompletedTask;
            public async Task DeleteUserAsync(Guid id) => await Task.CompletedTask;
        }
        
        public class ReadOnlyUserService : IUserService
        {
            private readonly IUserRepository _userRepository;
            private readonly ICacheService _cacheService;
            
            public ReadOnlyUserService(IUserRepository userRepository, ICacheService cacheService)
            {
                _userRepository = userRepository;
                _cacheService = cacheService;
            }
            
            public async Task<User> CreateUserAsync(string email, string firstName, string lastName)
                => throw new NotSupportedException("Servicio de solo lectura");
            
            public async Task<User> GetUserAsync(Guid id) => await _userRepository.GetByIdAsync(id);
            public async Task<IEnumerable<User>> GetAllUsersAsync() => await _userRepository.GetAllAsync();
            public async Task UpdateUserAsync(Guid id, string firstName, string lastName)
                => throw new NotSupportedException("Servicio de solo lectura");
            public async Task DeleteUserAsync(Guid id)
                => throw new NotSupportedException("Servicio de solo lectura");
        }
    }
    
    // ===== DECORATOR PATTERN =====
    namespace Decorators
    {
        public class CachingUserServiceDecorator : IUserService
        {
            private readonly IUserService _userService;
            private readonly ICacheService _cacheService;
            private readonly IAuditService _auditService;
            
            public CachingUserServiceDecorator(IUserService userService, ICacheService cacheService, IAuditService auditService)
            {
                _userService = userService;
                _cacheService = cacheService;
                _auditService = auditService;
            }
            
            public async Task<User> CreateUserAsync(string email, string firstName, string lastName)
            {
                var user = await _userService.CreateUserAsync(email, firstName, lastName);
                
                // Limpiar cache después de crear usuario
                await _cacheService.RemoveAsync("users_all");
                
                await _auditService.LogActionAsync("UserCreatedWithCache", user.Id.ToString(), 
                    "Usuario creado con decorador de cache");
                
                return user;
            }
            
            public async Task<User> GetUserAsync(Guid id)
            {
                // Intentar obtener del cache primero
                var cacheKey = $"user_{id}";
                var cachedUser = await _cacheService.GetAsync<User>(cacheKey);
                if (cachedUser != null)
                {
                    await _auditService.LogActionAsync("UserRetrievedFromCache", id.ToString(), 
                        "Usuario obtenido del cache por decorador");
                    return cachedUser;
                }
                
                // Si no está en cache, obtener del servicio original
                var user = await _userService.GetUserAsync(id);
                if (user != null)
                {
                    // Guardar en cache por 1 hora
                    await _cacheService.SetAsync(cacheKey, user, TimeSpan.FromHours(1));
                    
                    await _auditService.LogActionAsync("UserCachedByDecorator", id.ToString(), 
                        "Usuario guardado en cache por decorador");
                }
                
                return user;
            }
            
            public async Task<IEnumerable<User>> GetAllUsersAsync()
            {
                // Intentar obtener del cache primero
                var cachedUsers = await _cacheService.GetAsync<IEnumerable<User>>("users_all");
                if (cachedUsers != null)
                {
                    await _auditService.LogActionAsync("UsersRetrievedFromCache", "System", 
                        "Usuarios obtenidos del cache por decorador");
                    return cachedUsers;
                }
                
                // Si no está en cache, obtener del servicio original
                var users = await _userService.GetAllUsersAsync();
                
                // Guardar en cache por 30 minutos
                await _cacheService.SetAsync("users_all", users, TimeSpan.FromMinutes(30));
                
                await _auditService.LogActionAsync("UsersCachedByDecorator", "System", 
                    "Usuarios guardados en cache por decorador");
                
                return users;
            }
            
            public async Task UpdateUserAsync(Guid id, string firstName, string lastName)
            {
                await _userService.UpdateUserAsync(id, firstName, lastName);
                
                // Limpiar cache después de actualizar
                await _cacheService.RemoveAsync($"user_{id}");
                await _cacheService.RemoveAsync("users_all");
                
                await _auditService.LogActionAsync("UserUpdatedWithCache", id.ToString(), 
                    "Usuario actualizado con decorador de cache");
            }
            
            public async Task DeleteUserAsync(Guid id)
            {
                await _userService.DeleteUserAsync(id);
                
                // Limpiar cache después de eliminar
                await _cacheService.RemoveAsync($"user_{id}");
                await _cacheService.RemoveAsync("users_all");
                
                await _auditService.LogActionAsync("UserDeletedWithCache", id.ToString(), 
                    "Usuario eliminado con decorador de cache");
            }
        }
        
        public class LoggingUserServiceDecorator : IUserService
        {
            private readonly IUserService _userService;
            private readonly IAuditService _auditService;
            
            public LoggingUserServiceDecorator(IUserService userService, IAuditService auditService)
            {
                _userService = userService;
                _auditService = auditService;
            }
            
            public async Task<User> CreateUserAsync(string email, string firstName, string lastName)
            {
                await _auditService.LogActionAsync("UserServiceCall", "System", 
                    $"Creando usuario: {email}");
                
                try
                {
                    var user = await _userService.CreateUserAsync(email, firstName, lastName);
                    
                    await _auditService.LogActionAsync("UserServiceSuccess", "System", 
                        $"Usuario creado exitosamente: {user.Id}");
                    
                    return user;
                }
                catch (Exception ex)
                {
                    await _auditService.LogActionAsync("UserServiceError", "System", 
                        $"Error creando usuario {email}: {ex.Message}");
                    throw;
                }
            }
            
            public async Task<User> GetUserAsync(Guid id)
            {
                await _auditService.LogActionAsync("UserServiceCall", "System", 
                    $"Obteniendo usuario: {id}");
                
                try
                {
                    var user = await _userService.GetUserAsync(id);
                    
                    await _auditService.LogActionAsync("UserServiceSuccess", "System", 
                        $"Usuario obtenido exitosamente: {id}");
                    
                    return user;
                }
                catch (Exception ex)
                {
                    await _auditService.LogActionAsync("UserServiceError", "System", 
                        $"Error obteniendo usuario {id}: {ex.Message}");
                    throw;
                }
            }
            
            public async Task<IEnumerable<User>> GetAllUsersAsync()
            {
                await _auditService.LogActionAsync("UserServiceCall", "System", 
                    "Obteniendo todos los usuarios");
                
                try
                {
                    var users = await _userService.GetAllUsersAsync();
                    
                    await _auditService.LogActionAsync("UserServiceSuccess", "System", 
                        $"Usuarios obtenidos exitosamente: {users.Count()}");
                    
                    return users;
                }
                catch (Exception ex)
                {
                    await _auditService.LogActionAsync("UserServiceError", "System", 
                        $"Error obteniendo usuarios: {ex.Message}");
                    throw;
                }
            }
            
            public async Task UpdateUserAsync(Guid id, string firstName, string lastName)
            {
                await _auditService.LogActionAsync("UserServiceCall", "System", 
                    $"Actualizando usuario: {id}");
                
                try
                {
                    await _userService.UpdateUserAsync(id, firstName, lastName);
                    
                    await _auditService.LogActionAsync("UserServiceSuccess", "System", 
                        $"Usuario actualizado exitosamente: {id}");
                }
                catch (Exception ex)
                {
                    await _auditService.LogActionAsync("UserServiceError", "System", 
                        $"Error actualizando usuario {id}: {ex.Message}");
                    throw;
                }
            }
            
            public async Task DeleteUserAsync(Guid id)
            {
                await _auditService.LogActionAsync("UserServiceCall", "System", 
                    $"Eliminando usuario: {id}");
                
                try
                {
                    await _userService.DeleteUserAsync(id);
                    
                    await _auditService.LogActionAsync("UserServiceSuccess", "System", 
                        $"Usuario eliminado exitosamente: {id}");
                }
                catch (Exception ex)
                {
                    await _auditService.LogActionAsync("UserServiceError", "System", 
                        $"Error eliminando usuario {id}: {ex.Message}");
                    throw;
                }
            }
        }
    }
}

// Configuración y uso del sistema de DI
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Dependency Injection Avanzada ===\n");
        
        // Configurar servicios
        var services = new ServiceCollection();
        
        // Configurar service lifetimes
        services.AddSingleton<IConfigurationService, AdvancedDI.Services.ConfigurationService>();
        services.AddSingleton<ICacheService, AdvancedDI.Services.CacheService>();
        services.AddSingleton<IAuditService, AdvancedDI.Services.AuditService>();
        services.AddScoped<IUserRepository, AdvancedDI.Repositories.InMemoryUserRepository>();
        services.AddScoped<IUserService, AdvancedDI.Services.UserService>();
        services.AddScoped<IEmailService, AdvancedDI.Services.EmailService>();
        
        // Configurar factory
        services.AddSingleton<AdvancedDI.Factories.IUserServiceFactory, AdvancedDI.Factories.UserServiceFactory>();
        
        // Configurar decoradores
        services.AddScoped<AdvancedDI.Decorators.CachingUserServiceDecorator>();
        services.AddScoped<AdvancedDI.Decorators.LoggingUserServiceDecorator>();
        
        // Construir el service provider
        var serviceProvider = services.BuildServiceProvider();
        
        try
        {
            // 1. Usar servicios con diferentes lifetimes
            Console.WriteLine("1. Probando Service Lifetimes:");
            
            using (var scope1 = serviceProvider.CreateScope())
            {
                var userService1 = scope1.ServiceProvider.GetRequiredService<IUserService>();
                var userService2 = scope1.ServiceProvider.GetRequiredService<IUserService>();
                Console.WriteLine($"Mismo scope - Misma instancia: {ReferenceEquals(userService1, userService2)}");
            }
            
            using (var scope2 = serviceProvider.CreateScope())
            {
                var userService3 = scope2.ServiceProvider.GetRequiredService<IUserService>();
                Console.WriteLine("Nuevo scope creado");
            }
            
            Console.WriteLine();
            
            // 2. Usar factory pattern
            Console.WriteLine("2. Probando Factory Pattern:");
            var factory = serviceProvider.GetRequiredService<AdvancedDI.Factories.IUserServiceFactory>();
            
            var standardService = factory.CreateUserService("standard");
            var adminService = factory.CreateUserService("admin");
            var readonlyService = factory.CreateUserService("readonly");
            
            Console.WriteLine($"Servicios creados: {standardService.GetType().Name}, {adminService.GetType().Name}, {readonlyService.GetType().Name}");
            
            Console.WriteLine();
            
            // 3. Usar decoradores
            Console.WriteLine("3. Probando Decorator Pattern:");
            
            using (var scope = serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                var cachingDecorator = scope.ServiceProvider.GetRequiredService<AdvancedDI.Decorators.CachingUserServiceDecorator>();
                var loggingDecorator = scope.ServiceProvider.GetRequiredService<AdvancedDI.Decorators.LoggingUserServiceDecorator>();
                
                // Crear usuario con logging
                Console.WriteLine("Creando usuario con logging:");
                var user = await loggingDecorator.CreateUserAsync("test@email.com", "Test", "User");
                Console.WriteLine($"Usuario creado: {user.FullName}");
                
                Console.WriteLine();
                
                // Obtener usuario con cache
                Console.WriteLine("Obteniendo usuario con cache:");
                var retrievedUser = await cachingDecorator.GetUserAsync(user.Id);
                Console.WriteLine($"Usuario obtenido: {retrievedUser.FullName}");
                
                Console.WriteLine();
                
                // Obtener todos los usuarios
                Console.WriteLine("Obteniendo todos los usuarios:");
                var allUsers = await cachingDecorator.GetAllUsersAsync();
                Console.WriteLine($"Total de usuarios: {allUsers.Count()}");
                
                Console.WriteLine();
                
                // Actualizar usuario
                Console.WriteLine("Actualizando usuario:");
                await loggingDecorator.UpdateUserAsync(user.Id, "Updated", "User");
                Console.WriteLine("Usuario actualizado");
                
                Console.WriteLine();
                
                // Mostrar logs de auditoría
                var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();
                var logs = await auditService.GetAuditLogsAsync(DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);
                Console.WriteLine($"Logs de auditoría generados: {logs.Count()}");
                
                foreach (var log in logs.Take(5))
                {
                    Console.WriteLine($"  {log.Timestamp:HH:mm:ss} - {log.Action}: {log.Details}");
                }
            }
            
            Console.WriteLine();
            
            // 4. Probar cache
            Console.WriteLine("4. Probando Cache Service:");
            var cacheService = serviceProvider.GetRequiredService<ICacheService>();
            
            await cacheService.SetAsync("test_key", "test_value", TimeSpan.FromMinutes(5));
            var cachedValue = await cacheService.GetAsync<string>("test_key");
            Console.WriteLine($"Valor en cache: {cachedValue}");
            
            var exists = await cacheService.ExistsAsync("test_key");
            Console.WriteLine($"Clave existe: {exists}");
            
            Console.WriteLine();
            
            // 5. Probar configuración
            Console.WriteLine("5. Probando Configuration Service:");
            var configService = serviceProvider.GetRequiredService<IConfigurationService>();
            
            var smtpServer = configService.GetValue<string>("Smtp:Server");
            var cacheExpiration = configService.GetValue<int>("Cache:DefaultExpirationMinutes");
            var emailEnabled = configService.GetFeatureFlag("EmailNotifications");
            
            Console.WriteLine($"SMTP Server: {smtpServer}");
            Console.WriteLine($"Cache Expiration: {cacheExpiration} minutos");
            Console.WriteLine($"Email Notifications: {emailEnabled}");
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            // Dispose del service provider
            if (serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Sistema de Notificaciones con DI
Implementa un sistema de notificaciones que use diferentes tipos de servicios (email, SMS, push) con factory pattern y decoradores.

### Ejercicio 2: Sistema de Logging con Interceptores
Crea un sistema de logging que use interceptores para registrar automáticamente todas las llamadas a métodos.

### Ejercicio 3: Sistema de Validación con Decoradores
Desarrolla un sistema de validación que use decoradores para validar datos antes de procesarlos.

## 🔍 Puntos Clave

1. **Service Lifetimes** determinan cuándo se crean y destruyen las instancias de servicios
2. **Factory Pattern** permite crear diferentes implementaciones de servicios según el contexto
3. **Decorator Pattern** extiende la funcionalidad de servicios sin modificar su código
4. **Singleton** es útil para servicios que mantienen estado global
5. **Scoped** es ideal para servicios que deben ser únicos por request/transacción

## 📚 Recursos Adicionales

- [Dependency Injection - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [Service Lifetimes - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-lifetimes)
- [Factory Pattern - Refactoring Guru](https://refactoring.guru/design-patterns/factory-method)

---

**🎯 ¡Has completado la Clase 5! Ahora dominas Dependency Injection avanzada en C#**

**📚 [Siguiente: Clase 6 - Logging y Monitoreo](clase_6_logging_monitoreo.md)**
