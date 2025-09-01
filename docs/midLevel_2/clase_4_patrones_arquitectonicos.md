# 🚀 Clase 4: Patrones Arquitectónicos

## 📋 Información de la Clase

- **Módulo**: Mid Level 2 - Arquitectura de Software y Patrones Avanzados
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 3 (Arquitectura de Microservicios)

## 🎯 Objetivos de Aprendizaje

- Implementar Repository Pattern avanzado
- Aplicar Unit of Work Pattern
- Implementar Specification Pattern
- Crear arquitecturas modulares escalables

---

## 📚 Navegación del Módulo 5

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_arquitectura_hexagonal.md) | Arquitectura Hexagonal (Ports & Adapters) | |
| [Clase 2](clase_2_event_sourcing.md) | Event Sourcing y CQRS Avanzado | |
| [Clase 3](clase_3_microservicios.md) | Arquitectura de Microservicios | ← Anterior |
| **Clase 4** | **Patrones Arquitectónicos** | ← Estás aquí |
| [Clase 5](clase_5_domain_driven_design.md) | Domain Driven Design (DDD) | Siguiente → |
| [Clase 6](clase_6_async_streams.md) | Async Streams y IAsyncEnumerable | |
| [Clase 7](clase_7_source_generators.md) | Source Generators y Compile-time Code Generation | |
| [Clase 8](clase_8_high_performance.md) | High Performance Programming | |
| [Clase 9](clase_9_seguridad_avanzada.md) | Seguridad Avanzada en .NET | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Gestión Empresarial | |

**← [Volver al README del Módulo 5](../midLevel_2/README.md)**

---

## 📚 Contenido Teórico

### 1. Patrones Arquitectónicos Avanzados

Los patrones arquitectónicos proporcionan soluciones reutilizables para problemas comunes de diseño de software.

```csharp
// ===== PATRONES ARQUITECTÓNICOS AVANZADOS =====
namespace ArchitecturalPatterns
{
    // ===== REPOSITORY PATTERN AVANZADO =====
    namespace RepositoryPattern
    {
        public interface IRepository<TEntity, TKey> where TEntity : class
        {
            Task<TEntity> GetByIdAsync(TKey id);
            Task<IEnumerable<TEntity>> GetAllAsync();
            Task<TEntity> AddAsync(TEntity entity);
            Task UpdateAsync(TEntity entity);
            Task DeleteAsync(TKey id);
            Task<bool> ExistsAsync(TKey id);
            Task<int> CountAsync();
        }
        
        public interface IReadOnlyRepository<TEntity, TKey> where TEntity : class
        {
            Task<TEntity> GetByIdAsync(TKey id);
            Task<IEnumerable<TEntity>> GetAllAsync();
            Task<bool> ExistsAsync(TKey id);
            Task<int> CountAsync();
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
                var result = await _context.Set<TEntity>().AddAsync(entity);
                await _context.SaveChangesAsync();
                return result.Entity;
            }
            
            public virtual async Task UpdateAsync(TEntity entity)
            {
                _context.Set<TEntity>().Update(entity);
                await _context.SaveChangesAsync();
            }
            
            public virtual async Task DeleteAsync(TKey id)
            {
                var entity = await GetByIdAsync(id);
                if (entity != null)
                {
                    _context.Set<TEntity>().Remove(entity);
                    await _context.SaveChangesAsync();
                }
            }
            
            public virtual async Task<bool> ExistsAsync(TKey id)
            {
                return await _context.Set<TEntity>().FindAsync(id) != null;
            }
            
            public virtual async Task<int> CountAsync()
            {
                return await _context.Set<TEntity>().CountAsync();
            }
        }
        
        public class UserRepository : RepositoryBase<User, int>, IUserRepository
        {
            public UserRepository(ApplicationDbContext context, ILogger<UserRepository> logger)
                : base(context, logger)
            {
            }
            
            public async Task<User> GetByEmailAsync(string email)
            {
                return await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);
            }
            
            public async Task<IEnumerable<User>> GetActiveUsersAsync()
            {
                return await _context.Users
                    .Where(u => u.Status == UserStatus.Active)
                    .ToListAsync();
            }
            
            public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
            {
                return await _context.Users
                    .Where(u => u.Roles.Any(r => r.Name == role))
                    .Include(u => u.Roles)
                    .ToListAsync();
            }
        }
    }
    
    // ===== UNIT OF WORK PATTERN =====
    namespace UnitOfWorkPattern
    {
        public interface IUnitOfWork : IDisposable
        {
            IUserRepository Users { get; }
            IProductRepository Products { get; }
            IOrderRepository Orders { get; }
            Task<int> SaveChangesAsync();
            Task BeginTransactionAsync();
            Task CommitTransactionAsync();
            Task RollbackTransactionAsync();
        }
        
        public class UnitOfWork : IUnitOfWork
        {
            private readonly ApplicationDbContext _context;
            private readonly IDbContextTransaction _transaction;
            private readonly IUserRepository _users;
            private readonly IProductRepository _products;
            private readonly IOrderRepository _orders;
            
            public UnitOfWork(ApplicationDbContext context)
            {
                _context = context;
                _users = new UserRepository(context, null);
                _products = new ProductRepository(context, null);
                _orders = new OrderRepository(context, null);
            }
            
            public IUserRepository Users => _users;
            public IProductRepository Products => _products;
            public IOrderRepository Orders => _orders;
            
            public async Task<int> SaveChangesAsync()
            {
                return await _context.SaveChangesAsync();
            }
            
            public async Task BeginTransactionAsync()
            {
                _transaction = await _context.Database.BeginTransactionAsync();
            }
            
            public async Task CommitTransactionAsync()
            {
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            
            public async Task RollbackTransactionAsync()
            {
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync();
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
        public abstract class Specification<T>
        {
            public abstract Expression<Func<T, bool>> ToExpression();
            
            public bool IsSatisfiedBy(T entity)
            {
                var predicate = ToExpression().Compile();
                return predicate(entity);
            }
            
            public Specification<T> And(Specification<T> specification)
            {
                return new AndSpecification<T>(this, specification);
            }
            
            public Specification<T> Or(Specification<T> specification)
            {
                return new OrSpecification<T>(this, specification);
            }
            
            public Specification<T> Not()
            {
                return new NotSpecification<T>(this);
            }
        }
        
        public class AndSpecification<T> : Specification<T>
        {
            private readonly Specification<T> _left;
            private readonly Specification<T> _right;
            
            public AndSpecification(Specification<T> left, Specification<T> right)
            {
                _left = left;
                _right = right;
            }
            
            public override Expression<Func<T, bool>> ToExpression()
            {
                var leftExpression = _left.ToExpression();
                var rightExpression = _right.ToExpression();
                var invokedExpression = Expression.Invoke(rightExpression, leftExpression.Parameters);
                
                return Expression.Lambda<Func<T, bool>>(
                    Expression.AndAlso(leftExpression.Body, invokedExpression),
                    leftExpression.Parameters);
            }
        }
        
        public class OrSpecification<T> : Specification<T>
        {
            private readonly Specification<T> _left;
            private readonly Specification<T> _right;
            
            public OrSpecification(Specification<T> left, Specification<T> right)
            {
                _left = left;
                _right = right;
            }
            
            public override Expression<Func<T, bool>> ToExpression()
            {
                var leftExpression = _left.ToExpression();
                var rightExpression = _right.ToExpression();
                var invokedExpression = Expression.Invoke(rightExpression, leftExpression.Parameters);
                
                return Expression.Lambda<Func<T, bool>>(
                    Expression.OrElse(leftExpression.Body, invokedExpression),
                    leftExpression.Parameters);
            }
        }
        
        public class NotSpecification<T> : Specification<T>
        {
            private readonly Specification<T> _specification;
            
            public NotSpecification(Specification<T> specification)
            {
                _specification = specification;
            }
            
            public override Expression<Func<T, bool>> ToExpression()
            {
                var expression = _specification.ToExpression();
                var notExpression = Expression.Not(expression.Body);
                
                return Expression.Lambda<Func<T, bool>>(notExpression, expression.Parameters);
            }
        }
        
        // ===== SPECIFICATIONS CONCRETAS =====
        public class ActiveUserSpecification : Specification<User>
        {
            public override Expression<Func<User, bool>> ToExpression()
            {
                return user => user.Status == UserStatus.Active;
            }
        }
        
        public class UserEmailSpecification : Specification<User>
        {
            private readonly string _email;
            
            public UserEmailSpecification(string email)
            {
                _email = email;
            }
            
            public override Expression<Func<User, bool>> ToExpression()
            {
                return user => user.Email == _email;
            }
        }
        
        public class UserRoleSpecification : Specification<User>
        {
            private readonly string _role;
            
            public UserRoleSpecification(string role)
            {
                _role = role;
            }
            
            public override Expression<Func<User, bool>> ToExpression()
            {
                return user => user.Roles.Any(r => r.Name == _role);
            }
        }
    }
    
    // ===== FACTORY PATTERN =====
    namespace FactoryPattern
    {
        public interface IEntityFactory<TEntity>
        {
            TEntity Create();
            TEntity CreateWithId(int id);
        }
        
        public abstract class EntityFactoryBase<TEntity> : IEntityFactory<TEntity> where TEntity : class
        {
            public abstract TEntity Create();
            
            public virtual TEntity CreateWithId(int id)
            {
                var entity = Create();
                // Set ID using reflection or specific implementation
                return entity;
            }
        }
        
        public class UserFactory : EntityFactoryBase<User>
        {
            public override User Create()
            {
                return new User
                {
                    Status = UserStatus.Active,
                    CreatedAt = DateTime.UtcNow
                };
            }
            
            public override User CreateWithId(int id)
            {
                var user = Create();
                user.Id = id;
                return user;
            }
        }
    }
    
    // ===== BUILDER PATTERN =====
    namespace BuilderPattern
    {
        public class UserBuilder
        {
            private readonly User _user;
            
            public UserBuilder()
            {
                _user = new User
                {
                    Status = UserStatus.Active,
                    CreatedAt = DateTime.UtcNow
                };
            }
            
            public UserBuilder WithEmail(string email)
            {
                _user.Email = email;
                return this;
            }
            
            public UserBuilder WithName(string firstName, string lastName)
            {
                _user.FirstName = firstName;
                _user.LastName = lastName;
                return this;
            }
            
            public UserBuilder WithRole(string roleName)
            {
                var role = new Role { Name = roleName };
                _user.Roles.Add(role);
                return this;
            }
            
            public UserBuilder WithStatus(UserStatus status)
            {
                _user.Status = status;
                return this;
            }
            
            public User Build()
            {
                if (string.IsNullOrEmpty(_user.Email))
                    throw new InvalidOperationException("Email is required");
                
                return _user;
            }
        }
    }
    
    // ===== STRATEGY PATTERN =====
    namespace StrategyPattern
    {
        public interface IPricingStrategy
        {
            decimal CalculatePrice(Product product, int quantity);
        }
        
        public class RegularPricingStrategy : IPricingStrategy
        {
            public decimal CalculatePrice(Product product, int quantity)
            {
                return product.Price * quantity;
            }
        }
        
        public class DiscountPricingStrategy : IPricingStrategy
        {
            private readonly decimal _discountPercentage;
            
            public DiscountPricingStrategy(decimal discountPercentage)
            {
                _discountPercentage = discountPercentage;
            }
            
            public decimal CalculatePrice(Product product, int quantity)
            {
                var regularPrice = product.Price * quantity;
                var discount = regularPrice * (_discountPercentage / 100);
                return regularPrice - discount;
            }
        }
        
        public class BulkPricingStrategy : IPricingStrategy
        {
            private readonly int _bulkThreshold;
            private readonly decimal _bulkDiscount;
            
            public BulkPricingStrategy(int bulkThreshold, decimal bulkDiscount)
            {
                _bulkThreshold = bulkThreshold;
                _bulkDiscount = bulkDiscount;
            }
            
            public decimal CalculatePrice(Product product, int quantity)
            {
                var regularPrice = product.Price * quantity;
                
                if (quantity >= _bulkThreshold)
                {
                    var discount = regularPrice * (_bulkDiscount / 100);
                    return regularPrice - discount;
                }
                
                return regularPrice;
            }
        }
        
        public class PricingService
        {
            private readonly Dictionary<string, IPricingStrategy> _strategies;
            
            public PricingService()
            {
                _strategies = new Dictionary<string, IPricingStrategy>
                {
                    { "Regular", new RegularPricingStrategy() },
                    { "Discount", new DiscountPricingStrategy(10) },
                    { "Bulk", new BulkPricingStrategy(5, 15) }
                };
            }
            
            public decimal CalculatePrice(string strategyName, Product product, int quantity)
            {
                if (!_strategies.TryGetValue(strategyName, out var strategy))
                {
                    strategy = _strategies["Regular"];
                }
                
                return strategy.CalculatePrice(product, quantity);
            }
        }
    }
    
    // ===== DECORATOR PATTERN =====
    namespace DecoratorPattern
    {
        public interface IUserService
        {
            Task<User> GetUserAsync(int id);
            Task<IEnumerable<User>> GetUsersAsync();
        }
        
        public class UserService : IUserService
        {
            public async Task<User> GetUserAsync(int id)
            {
                // Implementación básica
                return new User { Id = id, FirstName = "John", LastName = "Doe" };
            }
            
            public async Task<IEnumerable<User>> GetUsersAsync()
            {
                // Implementación básica
                return new List<User>();
            }
        }
        
        public class CachingUserServiceDecorator : IUserService
        {
            private readonly IUserService _userService;
            private readonly IMemoryCache _cache;
            private readonly ILogger<CachingUserServiceDecorator> _logger;
            
            public CachingUserServiceDecorator(IUserService userService, IMemoryCache cache, ILogger<CachingUserServiceDecorator> logger)
            {
                _userService = userService;
                _cache = cache;
                _logger = logger;
            }
            
            public async Task<User> GetUserAsync(int id)
            {
                var cacheKey = $"user_{id}";
                
                if (_cache.TryGetValue(cacheKey, out User cachedUser))
                {
                    _logger.LogInformation("User {UserId} retrieved from cache", id);
                    return cachedUser;
                }
                
                var user = await _userService.GetUserAsync(id);
                
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));
                
                _cache.Set(cacheKey, user, cacheOptions);
                _logger.LogInformation("User {UserId} cached for future requests", id);
                
                return user;
            }
            
            public async Task<IEnumerable<User>> GetUsersAsync()
            {
                return await _userService.GetUsersAsync();
            }
        }
        
        public class LoggingUserServiceDecorator : IUserService
        {
            private readonly IUserService _userService;
            private readonly ILogger<LoggingUserServiceDecorator> _logger;
            
            public LoggingUserServiceDecorator(IUserService userService, ILogger<LoggingUserServiceDecorator> logger)
            {
                _userService = userService;
                _logger = logger;
            }
            
            public async Task<User> GetUserAsync(int id)
            {
                _logger.LogInformation("Getting user with ID: {UserId}", id);
                
                try
                {
                    var user = await _userService.GetUserAsync(id);
                    _logger.LogInformation("Successfully retrieved user {UserId}", id);
                    return user;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving user {UserId}", id);
                    throw;
                }
            }
            
            public async Task<IEnumerable<User>> GetUsersAsync()
            {
                _logger.LogInformation("Getting all users");
                
                try
                {
                    var users = await _userService.GetUsersAsync();
                    _logger.LogInformation("Successfully retrieved {UserCount} users", users.Count());
                    return users;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving users");
                    throw;
                }
            }
        }
    }
}

// ===== CONFIGURACIÓN =====
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddArchitecturalPatterns(this IServiceCollection services)
    {
        // Registrar repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        
        // Registrar Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Registrar factories
        services.AddScoped<IEntityFactory<User>, UserFactory>();
        
        // Registrar pricing strategies
        services.AddScoped<IPricingStrategy, RegularPricingStrategy>();
        services.AddScoped<PricingService>();
        
        // Registrar base user service
        services.AddScoped<UserService>();
        
        // Registrar decorators
        services.AddScoped<IUserService>(provider =>
        {
            var userService = provider.GetRequiredService<UserService>();
            var cache = provider.GetRequiredService<IMemoryCache>();
            var logger = provider.GetRequiredService<ILogger<CachingUserServiceDecorator>>();
            
            var cachingDecorator = new CachingUserServiceDecorator(userService, cache, logger);
            var loggingLogger = provider.GetRequiredService<ILogger<LoggingUserServiceDecorator>>();
            
            return new LoggingUserServiceDecorator(cachingDecorator, loggingLogger);
        });
        
        return services;
    }
}

// Uso de Patrones Arquitectónicos
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Patrones Arquitectónicos Avanzados ===\n");
        
        Console.WriteLine("Los patrones implementados incluyen:");
        Console.WriteLine("1. Repository Pattern con interfaces genéricas");
        Console.WriteLine("2. Unit of Work para transacciones");
        Console.WriteLine("3. Specification Pattern para consultas complejas");
        Console.WriteLine("4. Factory Pattern para creación de entidades");
        Console.WriteLine("5. Builder Pattern para construcción flexible");
        Console.WriteLine("6. Strategy Pattern para algoritmos intercambiables");
        Console.WriteLine("7. Decorator Pattern para funcionalidad adicional");
        
        Console.WriteLine("\nBeneficios principales:");
        Console.WriteLine("- Código reutilizable y mantenible");
        Console.WriteLine("- Separación clara de responsabilidades");
        Console.WriteLine("- Flexibilidad para cambios futuros");
        Console.WriteLine("- Testing más sencillo");
        Console.WriteLine("- Arquitectura escalable");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Repository Pattern Avanzado
Implementa un repository genérico con soporte para paginación y filtros dinámicos.

### Ejercicio 2: Specification Pattern
Crea especificaciones complejas para consultas de usuarios con múltiples criterios.

### Ejercicio 3: Decorator Pattern
Implementa un decorator para logging y métricas de rendimiento.

## 🔍 Puntos Clave

1. **Repository Pattern** abstrae el acceso a datos
2. **Unit of Work** gestiona transacciones
3. **Specification Pattern** encapsula lógica de consultas
4. **Factory Pattern** centraliza creación de objetos
5. **Decorator Pattern** añade funcionalidad sin modificar clases

## 📚 Recursos Adicionales

- [Repository Pattern - Martin Fowler](https://martinfowler.com/eaaCatalog/repository.html)
- [Unit of Work Pattern](https://martinfowler.com/eaaCatalog/unitOfWork.html)
- [Specification Pattern](https://martinfowler.com/apsupp/spec.pdf)

---

**🎯 ¡Has completado la Clase 4! Ahora comprendes los Patrones Arquitectónicos Avanzados en C#**

**📚 [Siguiente: Clase 5 - Domain Driven Design (DDD)](clase_5_domain_driven_design.md)**
