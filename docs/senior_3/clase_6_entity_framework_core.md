# 🚀 Clase 6: Entity Framework Core

## 🧭 Navegación
- **⬅️ Anterior**: [Clase 5: Autenticación y Autorización JWT](clase_5_autenticacion_autorizacion_jwt.md)
- **🏠 Inicio del Módulo**: [Módulo 10: APIs REST y Web APIs](README.md)
- **➡️ Siguiente**: [Clase 7: Documentación con Swagger](clase_7_documentacion_swagger.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 📚 Descripción

En esta clase aprenderás a implementar Entity Framework Core en tus APIs, incluyendo configuración del contexto, modelos de datos, repositorios genéricos y operaciones de base de datos optimizadas.

## 🎯 Objetivos de Aprendizaje

- Configurar Entity Framework Core con SQL Server
- Crear modelos de datos y relaciones
- Implementar repositorios genéricos y específicos
- Optimizar consultas y operaciones de base de datos
- Implementar migraciones y seeding de datos
- Manejar transacciones y concurrencia

## 📖 Contenido Teórico

### Configuración del Contexto

#### DbContext Básico
```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        // Configuración de Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.Stock).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            // Relación con Category
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración de Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            
            // Relación con User
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Orders)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración de OrderItem
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)").IsRequired();
            
            // Relación con Order
            entity.HasOne(e => e.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Relación con Product
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración de Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        // Configuración de RefreshToken
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.Property(e => e.ExpiresAt).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.IsRevoked).HasDefaultValue(false);
            
            // Relación con User
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
```

#### Configuración Avanzada del Contexto
```csharp
public class ApplicationDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService) : base(options)
    {
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar todas las configuraciones de una vez
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Configuraciones globales
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Configurar soft delete para entidades que implementen ISoftDelete
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(GetSoftDeleteFilter(entityType.ClrType));
            }

            // Configurar auditoría para entidades que implementen IAuditable
            if (typeof(IAuditable).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property("CreatedAt")
                    .HasDefaultValueSql("GETDATE()");
                
                modelBuilder.Entity(entityType.ClrType)
                    .Property("UpdatedAt")
                    .HasDefaultValueSql("GETDATE()");
            }
        }
    }

    private LambdaExpression GetSoftDeleteFilter(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "e");
        var property = Expression.Property(parameter, "IsDeleted");
        var falseConstant = Expression.Constant(false);
        var comparison = Expression.Equal(property, falseConstant);
        
        return Expression.Lambda(comparison, parameter);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditableEntities()
    {
        var entries = ChangeTracker.Entries<IAuditable>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = _dateTimeService.Now;
                entry.Entity.CreatedBy = _currentUserService.UserId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = _dateTimeService.Now;
                entry.Entity.UpdatedBy = _currentUserService.UserId;
            }
        }
    }
}
```

### Modelos de Datos

#### Modelos Básicos
```csharp
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Propiedades de navegación
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    // Propiedades calculadas
    public string FullName => $"{FirstName} {LastName}";
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Propiedades de navegación
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Propiedades de navegación
    public virtual User User { get; set; } = null!;
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    // Propiedades de navegación
    public virtual Order Order { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    // Propiedades de navegación
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRevoked { get; set; }

    // Propiedades de navegación
    public virtual User User { get; set; } = null!;
}
```

#### Modelos con Interfaces
```csharp
public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    int? CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    int? UpdatedBy { get; set; }
}

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
}

public interface IEntity
{
    int Id { get; set; }
}

// Modelos que implementan las interfaces
public class User : IEntity, IAuditable, ISoftDelete
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Propiedades de navegación
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

public class Product : IEntity, IAuditable, ISoftDelete
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }

    // Propiedades de navegación
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
```

### Repositorios Genéricos

#### Repositorio Genérico Base
```csharp
public interface IRepository<T> where T : class, IEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<bool> ExistsAsync(int id);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize, Expression<Func<T, bool>>? predicate = null);
}

public class Repository<T> : IRepository<T> where T : class, IEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.AnyAsync(e => e.Id == id);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        if (predicate == null)
            return await _dbSet.CountAsync();
        
        return await _dbSet.CountAsync(predicate);
    }

    public virtual async Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize, Expression<Func<T, bool>>? predicate = null)
    {
        var query = _dbSet.AsQueryable();
        
        if (predicate != null)
            query = query.Where(predicate);
        
        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
```

#### Repositorios Específicos
```csharp
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetByRoleAsync(string role);
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<bool> EmailExistsAsync(string email);
    Task UpdateLastLoginAsync(int userId);
    Task UpdatePasswordAsync(int userId, string newPasswordHash);
}

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Include(u => u.Orders)
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(string role)
    {
        return await _dbSet
            .Where(u => u.Role == role && !u.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _dbSet
            .Where(u => u.IsActive && !u.IsDeleted)
            .ToListAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email && !u.IsDeleted);
    }

    public async Task UpdateLastLoginAsync(int userId)
    {
        var user = await _dbSet.FindAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdatePasswordAsync(int userId, string newPasswordHash)
    {
        var user = await _dbSet.FindAsync(userId);
        if (user != null)
        {
            user.PasswordHash = newPasswordHash;
            await _context.SaveChangesAsync();
        }
    }
}

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
    Task<IEnumerable<Product>> SearchAsync(string searchTerm);
    Task<IEnumerable<Product>> GetActiveProductsAsync();
    Task UpdateStockAsync(int productId, int quantity);
    Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10);
}

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId && p.IsActive && !p.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> SearchAsync(string searchTerm)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Where(p => (p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm)) 
                       && p.IsActive && !p.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync()
    {
        return await _dbSet
            .Include(p => p.Category)
            .Where(p => p.IsActive && !p.IsDeleted)
            .ToListAsync();
    }

    public async Task UpdateStockAsync(int productId, int quantity)
    {
        var product = await _dbSet.FindAsync(productId);
        if (product != null)
        {
            product.Stock += quantity;
            if (product.Stock < 0) product.Stock = 0;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10)
    {
        return await _dbSet
            .Where(p => p.Stock <= threshold && p.IsActive && !p.IsDeleted)
            .ToListAsync();
    }
}
```

### Operaciones de Base de Datos Optimizadas

#### Consultas con Include y ThenInclude
```csharp
public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Order?> GetOrderWithDetailsAsync(int orderId)
    {
        return await _dbSet
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status)
    {
        return await _dbSet
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .Where(o => o.Status == status)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();
    }
}
```

#### Consultas con Proyección
```csharp
public class ProductRepository : Repository<Product>, IProductRepository
{
    public async Task<IEnumerable<ProductSummaryDto>> GetProductSummariesAsync()
    {
        return await _dbSet
            .Where(p => p.IsActive && !p.IsDeleted)
            .Select(p => new ProductSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                CategoryName = p.Category.Name
            })
            .ToListAsync();
    }

    public async Task<ProductDetailDto?> GetProductDetailAsync(int productId)
    {
        return await _dbSet
            .Where(p => p.Id == productId && p.IsActive && !p.IsDeleted)
            .Select(p => new ProductDetailDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                Category = new CategoryDto
                {
                    Id = p.Category.Id,
                    Name = p.Category.Name,
                    Description = p.Category.Description
                },
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }
}

// DTOs para proyección
public class ProductSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public class ProductDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public CategoryDto Category { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
```

#### Consultas con Filtros Dinámicos
```csharp
public class ProductRepository : Repository<Product>, IProductRepository
{
    public async Task<IEnumerable<Product>> GetProductsWithFiltersAsync(ProductFilterDto filter)
    {
        var query = _dbSet
            .Include(p => p.Category)
            .Where(p => p.IsActive && !p.IsDeleted);

        // Aplicar filtros dinámicamente
        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            query = query.Where(p => p.Name.Contains(filter.SearchTerm) || 
                                   p.Description.Contains(filter.SearchTerm));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);
        }

        if (filter.InStockOnly)
        {
            query = query.Where(p => p.Stock > 0);
        }

        // Aplicar ordenamiento
        query = filter.SortBy?.ToLower() switch
        {
            "name" => filter.SortOrder == "desc" ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => filter.SortOrder == "desc" ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "createdat" => filter.SortOrder == "desc" ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderBy(p => p.Name)
        };

        // Aplicar paginación
        if (filter.Page > 0 && filter.PageSize > 0)
        {
            query = query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize);
        }

        return await query.ToListAsync();
    }
}

public class ProductFilterDto
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool InStockOnly { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
```

### Transacciones y Concurrencia

#### Manejo de Transacciones
```csharp
public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        ApplicationDbContext context,
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        ILogger<OrderService> logger)
    {
        _context = context;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderDto createOrderDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Crear la orden
            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                UserId = createOrderDto.UserId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            var orderItems = new List<OrderItem>();
            decimal totalAmount = 0;

            // Procesar cada item de la orden
            foreach (var itemDto in createOrderDto.Items)
            {
                var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
                if (product == null)
                {
                    throw new InvalidOperationException($"Producto con ID {itemDto.ProductId} no encontrado");
                }

                if (product.Stock < itemDto.Quantity)
                {
                    throw new InvalidOperationException($"Stock insuficiente para el producto {product.Name}");
                }

                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price,
                    TotalPrice = product.Price * itemDto.Quantity
                };

                orderItems.Add(orderItem);
                totalAmount += orderItem.TotalPrice;

                // Actualizar stock
                await _productRepository.UpdateStockAsync(itemDto.ProductId, -itemDto.Quantity);
            }

            order.TotalAmount = totalAmount;
            order.OrderItems = orderItems;

            // Guardar la orden
            await _orderRepository.AddAsync(order);

            // Confirmar la transacción
            await transaction.CommitAsync();

            _logger.LogInformation("Orden creada exitosamente: {OrderNumber}", order.OrderNumber);

            return order;
        }
        catch (Exception ex)
        {
            // Revertir la transacción en caso de error
            await transaction.RollbackAsync();
            
            _logger.LogError(ex, "Error al crear la orden");
            throw;
        }
    }

    private string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
    }
}
```

#### Manejo de Concurrencia
```csharp
public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        ApplicationDbContext context,
        IProductRepository productRepository,
        ILogger<ProductService> logger)
    {
        _context = context;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<bool> UpdateProductStockAsync(int productId, int quantity, int expectedStock)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return false;
            }

            // Verificar que el stock no haya cambiado desde la última lectura
            if (product.Stock != expectedStock)
            {
                _logger.LogWarning("Stock del producto {ProductId} cambió durante la operación. Esperado: {Expected}, Actual: {Actual}", 
                    productId, expectedStock, product.Stock);
                return false;
            }

            // Actualizar stock
            product.Stock += quantity;
            if (product.Stock < 0) product.Stock = 0;

            await _productRepository.UpdateAsync(product);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Error de concurrencia al actualizar stock del producto {ProductId}", productId);
            return false;
        }
    }

    public async Task<bool> ReserveProductAsync(int productId, int quantity)
    {
        // Usar SQL raw para operación atómica
        var sql = @"
            UPDATE Products 
            SET Stock = Stock - @Quantity 
            WHERE Id = @ProductId 
            AND Stock >= @Quantity 
            AND IsActive = 1 
            AND IsDeleted = 0";

        var parameters = new[]
        {
            new SqlParameter("@ProductId", productId),
            new SqlParameter("@Quantity", quantity)
        };

        var rowsAffected = await _context.Database.ExecuteSqlRawAsync(sql, parameters);
        return rowsAffected > 0;
    }
}
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Configuración del Contexto
Configura Entity Framework Core con:
- Modelos de datos completos
- Relaciones entre entidades
- Configuraciones de propiedades
- Filtros globales

### Ejercicio 2: Repositorios Genéricos
Implementa repositorios para:
- Operaciones CRUD básicas
- Consultas con filtros dinámicos
- Paginación y ordenamiento
- Proyecciones optimizadas

### Ejercicio 3: Operaciones de Base de Datos
Crea servicios que implementen:
- Transacciones complejas
- Manejo de concurrencia
- Consultas optimizadas
- Operaciones en lote

### Ejercicio 4: Migraciones y Seeding
Implementa:
- Migraciones automáticas
- Datos de prueba
- Scripts de inicialización
- Manejo de versiones de base de datos

## 📝 Quiz de Autoevaluación

1. ¿Cuáles son las ventajas de usar repositorios genéricos?
2. ¿Cómo manejarías la concurrencia en operaciones de stock?
3. ¿Qué estrategias usarías para optimizar consultas complejas?
4. ¿Cómo implementarías soft delete en Entity Framework?
5. ¿Qué consideraciones tendrías para el manejo de transacciones distribuidas?

## 🔗 Enlaces Útiles

- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [EF Core Performance](https://docs.microsoft.com/en-us/ef/core/performance/)
- [EF Core Concurrency](https://docs.microsoft.com/en-us/ef/core/saving/concurrency)
- [EF Core Transactions](https://docs.microsoft.com/en-us/ef/core/saving/transactions)

## 🚀 Siguiente Clase

En la siguiente clase aprenderás a documentar tus APIs con Swagger, incluyendo configuración avanzada, autenticación JWT y ejemplos de uso.

---

**💡 Consejo**: Siempre usa Include y ThenInclude de manera estratégica para evitar el problema N+1, y considera usar proyecciones para reducir la cantidad de datos transferidos.
