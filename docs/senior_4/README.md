# 🚀 Senior Level 4: Entity Framework y Bases de Datos

## 📋 Contenido del Nivel

### 🎯 Objetivos de Aprendizaje
- Dominar Entity Framework Core para acceso a datos
- Implementar patrones de acceso a datos robustos
- Optimizar consultas y manejar relaciones complejas
- Implementar migraciones y versionado de base de datos
- Crear arquitecturas de datos escalables

### ⏱️ Tiempo Estimado
- **Teoría**: 4-5 horas
- **Ejercicios**: 6-8 horas
- **Proyecto Integrador**: 4-5 horas
- **Total**: 14-18 horas

---

## 📚 Contenido Teórico

### 1. Entity Framework Core Fundamentals

#### 1.1 ¿Qué es Entity Framework Core?
Entity Framework Core es un ORM (Object-Relational Mapper) moderno y ligero para .NET que permite trabajar con bases de datos usando objetos .NET.

#### 1.2 Configuración Básica

```csharp
// Program.cs o Startup.cs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MyAppDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}

// DbContext
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Product> Products { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuraciones del modelo
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
            
        modelBuilder.Entity<Order>()
            .Property(o => o.Total)
            .HasColumnType("decimal(18,2)");
    }
}
```

#### 1.3 Entidades y Modelos

```csharp
public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Propiedades de navegación
    public virtual ICollection<Order> Orders { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal Total { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    
    // Propiedades de navegación
    public virtual User User { get; set; }
    public virtual ICollection<OrderItem> Items { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; }
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    
    // Propiedades de navegación
    public virtual Order Order { get; set; }
    public virtual Product Product { get; set; }
}
```

### 2. Configuración de Modelos

#### 2.1 Data Annotations

```csharp
public class User
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string PasswordHash { get; set; }
    
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; set; }
    
    [MaxLength(20)]
    public string PhoneNumber { get; set; }
    
    [Range(0, 120)]
    public int? Age { get; set; }
    
    [Column("UserStatus")]
    public UserStatus Status { get; set; }
}

public class Order
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [ForeignKey("User")]
    public int UserId { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }
    
    [Required]
    public DateTime OrderDate { get; set; }
    
    [Required]
    public OrderStatus Status { get; set; }
    
    // Propiedades de navegación
    public virtual User User { get; set; }
    public virtual ICollection<OrderItem> Items { get; set; }
}
```

#### 2.2 Fluent API

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Configuración de User
    modelBuilder.Entity<User>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.Email).IsUnique();
        entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
        entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
        
        // Relación uno a muchos con Order
        entity.HasMany(e => e.Orders)
              .WithOne(e => e.User)
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Restrict);
    });
    
    // Configuración de Order
    modelBuilder.Entity<Order>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
        entity.Property(e => e.OrderDate).HasDefaultValueSql("GETDATE()");
        
        // Relación uno a muchos con OrderItem
        entity.HasMany(e => e.Items)
              .WithOne(e => e.Order)
              .HasForeignKey(e => e.OrderId)
              .OnDelete(DeleteBehavior.Cascade);
    });
    
    // Configuración de Product
    modelBuilder.Entity<Product>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
        entity.HasIndex(e => e.Category);
    });
    
    // Configuración de OrderItem
    modelBuilder.Entity<OrderItem>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
        
        // Relación muchos a uno con Product
        entity.HasOne(e => e.Product)
              .WithMany()
              .HasForeignKey(e => e.ProductId)
              .OnDelete(DeleteBehavior.Restrict);
    });
}
```

### 3. Operaciones CRUD

#### 3.1 Crear (Create)

```csharp
public class UserService
{
    private readonly ApplicationDbContext _context;
    
    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<User> CreateUserAsync(User user)
    {
        // Validaciones
        if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            throw new InvalidOperationException("Email already exists");
        
        // Configurar valores por defecto
        user.CreatedAt = DateTime.UtcNow;
        user.Status = UserStatus.Active;
        
        // Agregar y guardar
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        return user;
    }
    
    public async Task<Order> CreateOrderAsync(int userId, List<OrderItemDto> items)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");
        
        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            Items = new List<OrderItem>()
        };
        
        decimal total = 0;
        foreach (var itemDto in items)
        {
            var product = await _context.Products.FindAsync(itemDto.ProductId);
            if (product == null)
                throw new InvalidOperationException($"Product {itemDto.ProductId} not found");
            
            if (product.Stock < itemDto.Quantity)
                throw new InvalidOperationException($"Insufficient stock for product {product.Name}");
            
            var orderItem = new OrderItem
            {
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                UnitPrice = product.Price
            };
            
            order.Items.Add(orderItem);
            total += product.Price * itemDto.Quantity;
            
            // Actualizar stock
            product.Stock -= itemDto.Quantity;
        }
        
        order.Total = total;
        
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        
        return order;
    }
}
```

#### 3.2 Leer (Read)

```csharp
public class UserService
{
    // Obtener usuario por ID
    public async Task<User> GetUserByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Orders)
            .ThenInclude(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(u => u.Id == id);
    }
    
    // Obtener usuarios con filtros y paginación
    public async Task<PagedResult<User>> GetUsersAsync(
        string searchTerm = null,
        string sortBy = "Name",
        bool ascending = true,
        int page = 1,
        int pageSize = 10)
    {
        var query = _context.Users.AsQueryable();
        
        // Aplicar filtros
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(u => 
                u.Name.Contains(searchTerm) || 
                u.Email.Contains(searchTerm));
        }
        
        // Aplicar ordenamiento
        query = sortBy.ToLower() switch
        {
            "name" => ascending ? query.OrderBy(u => u.Name) : query.OrderByDescending(u => u.Name),
            "email" => ascending ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email),
            "createdat" => ascending ? query.OrderBy(u => u.CreatedAt) : query.OrderByDescending(u => u.CreatedAt),
            _ => query.OrderBy(u => u.Name)
        };
        
        // Aplicar paginación
        var totalCount = await query.CountAsync();
        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return new PagedResult<User>
        {
            Items = users,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }
    
    // Consultas complejas con LINQ
    public async Task<IEnumerable<UserOrderSummary>> GetUserOrderSummariesAsync()
    {
        return await _context.Users
            .Select(u => new UserOrderSummary
            {
                UserId = u.Id,
                UserName = u.Name,
                UserEmail = u.Email,
                TotalOrders = u.Orders.Count,
                TotalSpent = u.Orders.Sum(o => o.Total),
                LastOrderDate = u.Orders.Max(o => o.OrderDate),
                AverageOrderValue = u.Orders.Average(o => o.Total)
            })
            .Where(s => s.TotalOrders > 0)
            .OrderByDescending(s => s.TotalSpent)
            .ToListAsync();
    }
}

public class UserOrderSummary
{
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string UserEmail { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastOrderDate { get; set; }
    public decimal AverageOrderValue { get; set; }
}
```

#### 3.3 Actualizar (Update)

```csharp
public class UserService
{
    public async Task<bool> UpdateUserAsync(int id, UserUpdateDto updateDto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return false;
        
        // Verificar si el email ya existe en otro usuario
        if (updateDto.Email != user.Email && 
            await _context.Users.AnyAsync(u => u.Email == updateDto.Email))
        {
            throw new InvalidOperationException("Email already exists");
        }
        
        // Actualizar propiedades
        user.Name = updateDto.Name;
        user.Email = updateDto.Email;
        user.PhoneNumber = updateDto.PhoneNumber;
        user.Age = updateDto.Age;
        
        // Marcar como modificado
        _context.Entry(user).State = EntityState.Modified;
        
        var rowsAffected = await _context.SaveChangesAsync();
        return rowsAffected > 0;
    }
    
    public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);
            
        if (order == null)
            return false;
        
        // Validar transición de estado
        if (!IsValidStatusTransition(order.Status, newStatus))
            throw new InvalidOperationException($"Invalid status transition from {order.Status} to {newStatus}");
        
        // Si se cancela la orden, restaurar stock
        if (newStatus == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
        {
            foreach (var item in order.Items)
            {
                var product = item.Product;
                product.Stock += item.Quantity;
                _context.Entry(product).State = EntityState.Modified;
            }
        }
        
        order.Status = newStatus;
        _context.Entry(order).State = EntityState.Modified;
        
        var rowsAffected = await _context.SaveChangesAsync();
        return rowsAffected > 0;
    }
    
    private bool IsValidStatusTransition(OrderStatus current, OrderStatus newStatus)
    {
        return (current, newStatus) switch
        {
            (OrderStatus.Pending, OrderStatus.Processing) => true,
            (OrderStatus.Pending, OrderStatus.Cancelled) => true,
            (OrderStatus.Processing, OrderStatus.Shipped) => true,
            (OrderStatus.Processing, OrderStatus.Cancelled) => true,
            (OrderStatus.Shipped, OrderStatus.Delivered) => true,
            _ => false
        };
    }
}
```

#### 3.4 Eliminar (Delete)

```csharp
public class UserService
{
    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _context.Users
            .Include(u => u.Orders)
            .FirstOrDefaultAsync(u => u.Id == id);
            
        if (user == null)
            return false;
        
        // Verificar si tiene órdenes activas
        if (user.Orders.Any(o => o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Delivered))
            throw new InvalidOperationException("Cannot delete user with active orders");
        
        // Eliminar usuario (las órdenes se eliminarán en cascada si está configurado)
        _context.Users.Remove(user);
        
        var rowsAffected = await _context.SaveChangesAsync();
        return rowsAffected > 0;
    }
    
    public async Task<bool> SoftDeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return false;
        
        // Soft delete - marcar como eliminado
        user.Status = UserStatus.Deleted;
        user.DeletedAt = DateTime.UtcNow;
        
        _context.Entry(user).State = EntityState.Modified;
        
        var rowsAffected = await _context.SaveChangesAsync();
        return rowsAffected > 0;
    }
}
```

### 4. Relaciones y Navegación

#### 4.1 Relaciones Uno a Muchos

```csharp
// En el modelo
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public virtual ICollection<Order> Orders { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public virtual User User { get; set; }
}

// En la configuración
modelBuilder.Entity<User>()
    .HasMany(u => u.Orders)
    .WithOne(o => o.User)
    .HasForeignKey(o => o.UserId)
    .OnDelete(DeleteBehavior.Restrict);

// En el servicio
public async Task<UserWithOrders> GetUserWithOrdersAsync(int userId)
{
    return await _context.Users
        .Where(u => u.Id == userId)
        .Select(u => new UserWithOrders
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Orders = u.Orders.Select(o => new OrderSummary
            {
                Id = o.Id,
                Total = o.Total,
                Status = o.Status,
                OrderDate = o.OrderDate
            }).ToList()
        })
        .FirstOrDefaultAsync();
}
```

#### 4.2 Relaciones Muchos a Muchos

```csharp
// Modelo para relación muchos a muchos
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; set; }
}

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; set; }
}

public class UserRole
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public DateTime AssignedAt { get; set; }
    
    public virtual User User { get; set; }
    public virtual Role Role { get; set; }
}

// Configuración
modelBuilder.Entity<UserRole>()
    .HasKey(ur => new { ur.UserId, ur.RoleId });

modelBuilder.Entity<UserRole>()
    .HasOne(ur => ur.User)
    .WithMany(u => u.UserRoles)
    .HasForeignKey(ur => ur.UserId);

modelBuilder.Entity<UserRole>()
    .HasOne(ur => ur.Role)
    .WithMany(r => r.UserRoles)
    .HasForeignKey(ur => ur.RoleId);

// Servicio
public async Task<bool> AssignRoleToUserAsync(int userId, int roleId)
{
    var existingAssignment = await _context.UserRoles
        .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        
    if (existingAssignment != null)
        return false; // Ya tiene el rol
        
    var userRole = new UserRole
    {
        UserId = userId,
        RoleId = roleId,
        AssignedAt = DateTime.UtcNow
    };
    
    _context.UserRoles.Add(userRole);
    await _context.SaveChangesAsync();
    
    return true;
}
```

### 5. Consultas Optimizadas

#### 5.1 Eager Loading vs Lazy Loading

```csharp
// Eager Loading - Cargar todo de una vez
public async Task<User> GetUserWithOrdersEagerAsync(int userId)
{
    return await _context.Users
        .Include(u => u.Orders)
            .ThenInclude(o => o.Items)
                .ThenInclude(oi => oi.Product)
        .FirstOrDefaultAsync(u => u.Id == userId);
}

// Lazy Loading - Cargar cuando se accede
public async Task<User> GetUserWithOrdersLazyAsync(int userId)
{
    // Habilitar lazy loading en el contexto
    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Id == userId);
    
    // Los datos se cargan automáticamente cuando se accede
    var orderCount = user.Orders.Count; // Carga las órdenes
    var firstOrder = user.Orders.FirstOrDefault(); // Carga la primera orden
    
    return user;
}

// Projection - Solo cargar lo necesario
public async Task<UserSummary> GetUserSummaryAsync(int userId)
{
    return await _context.Users
        .Where(u => u.Id == userId)
        .Select(u => new UserSummary
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            OrderCount = u.Orders.Count,
            TotalSpent = u.Orders.Sum(o => o.Total),
            LastOrderDate = u.Orders.Max(o => o.OrderDate)
        })
        .FirstOrDefaultAsync();
}
```

#### 5.2 Consultas Compiladas

```csharp
// Consulta compilada para mejor rendimiento
private static readonly Func<ApplicationDbContext, int, Task<User>> GetUserByIdCompiled =
    EF.CompileAsyncQuery((ApplicationDbContext context, int id) =>
        context.Users
            .Include(u => u.Orders)
            .FirstOrDefault(u => u.Id == id));

public async Task<User> GetUserByIdOptimizedAsync(int id)
{
    return await GetUserByIdCompiled(_context, id);
}

// Consulta compilada con parámetros múltiples
private static readonly Func<ApplicationDbContext, string, int, int, Task<List<User>>> GetUsersBySearchCompiled =
    EF.CompileAsyncQuery((ApplicationDbContext context, string searchTerm, int skip, int take) =>
        context.Users
            .Where(u => u.Name.Contains(searchTerm) || u.Email.Contains(searchTerm))
            .OrderBy(u => u.Name)
            .Skip(skip)
            .Take(take)
            .ToList());

public async Task<List<User>> GetUsersBySearchOptimizedAsync(string searchTerm, int page, int pageSize)
{
    var skip = (page - 1) * pageSize;
    return await GetUsersBySearchCompiled(_context, searchTerm, skip, pageSize);
}
```

### 6. Migraciones y Base de Datos

#### 6.1 Crear y Aplicar Migraciones

```bash
# Crear migración
dotnet ef migrations add InitialCreate

# Aplicar migración
dotnet ef database update

# Revertir migración
dotnet ef database update PreviousMigrationName

# Generar script SQL
dotnet ef migrations script

# Eliminar última migración
dotnet ef migrations remove
```

#### 6.2 Configuración de Migraciones

```csharp
// En Program.cs
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
    }
}

// Migración personalizada
public partial class AddUserProfile : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ProfilePicture",
            table: "Users",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: true);
            
        migrationBuilder.AddColumn<string>(
            name: "Bio",
            table: "Users",
            type: "nvarchar(1000)",
            maxLength: 1000,
            nullable: true);
            
        // Crear índice
        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            table: "Users",
            column: "Email",
            unique: true);
    }
    
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Users_Email",
            table: "Users");
            
        migrationBuilder.DropColumn(
            name: "Bio",
            table: "Users");
            
        migrationBuilder.DropColumn(
            name: "ProfilePicture",
            table: "Users");
    }
}
```

### 7. Patrones de Repositorio

#### 7.1 Repository Genérico

```csharp
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<bool> ExistsAsync(int id);
    IQueryable<T> Query();
}

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;
    
    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    
    public virtual async Task<T> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }
    
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
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
        return await _dbSet.FindAsync(id) != null;
    }
    
    public virtual IQueryable<T> Query()
    {
        return _dbSet.AsQueryable();
    }
}
```

#### 7.2 Repository Específico

```csharp
public interface IUserRepository : IRepository<User>
{
    Task<User> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetByRoleAsync(string roleName);
    Task<PagedResult<User>> GetPagedAsync(int page, int pageSize, string searchTerm = null);
    Task<IEnumerable<UserOrderSummary>> GetUserOrderSummariesAsync();
}

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }
    
    public async Task<User> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Include(u => u.Orders)
            .FirstOrDefaultAsync(u => u.Email == email);
    }
    
    public async Task<IEnumerable<User>> GetByRoleAsync(string roleName)
    {
        return await _dbSet
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == roleName))
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ToListAsync();
    }
    
    public async Task<PagedResult<User>> GetPagedAsync(int page, int pageSize, string searchTerm = null)
    {
        var query = _dbSet.AsQueryable();
        
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(u => 
                u.Name.Contains(searchTerm) || 
                u.Email.Contains(searchTerm));
        }
        
        var totalCount = await query.CountAsync();
        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return new PagedResult<User>
        {
            Items = users,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }
    
    public async Task<IEnumerable<UserOrderSummary>> GetUserOrderSummariesAsync()
    {
        return await _dbSet
            .Select(u => new UserOrderSummary
            {
                UserId = u.Id,
                UserName = u.Name,
                UserEmail = u.Email,
                OrderCount = u.Orders.Count,
                TotalSpent = u.Orders.Sum(o => o.Total),
                LastOrderDate = u.Orders.Max(o => o.OrderDate)
            })
            .Where(s => s.OrderCount > 0)
            .OrderByDescending(s => s.TotalSpent)
            .ToListAsync();
    }
}
```

---

## 🎯 Ejercicios Prácticos

### Ejercicio 1: Configuración de DbContext
Configura un DbContext para un sistema de gestión de biblioteca.

### Ejercicio 2: Entidades y Relaciones
Crea entidades para un sistema de e-commerce con relaciones complejas.

### Ejercicio 3: Operaciones CRUD
Implementa operaciones CRUD completas para usuarios y productos.

### Ejercicio 4: Consultas LINQ
Crea consultas LINQ complejas con filtros, ordenamiento y paginación.

### Ejercicio 5: Relaciones y Navegación
Implementa relaciones uno a muchos y muchos a muchos.

### Ejercicio 6: Migraciones
Crea y gestiona migraciones para cambios en el modelo de datos.

### Ejercicio 7: Repository Pattern
Implementa el patrón repository con métodos genéricos y específicos.

### Ejercicio 8: Consultas Optimizadas
Optimiza consultas usando projection, compiled queries y eager loading.

### Ejercicio 9: Validaciones y Constraints
Implementa validaciones a nivel de base de datos y aplicación.

### Ejercicio 10: Testing de Base de Datos
Crea pruebas unitarias y de integración para el acceso a datos.

---

## 🚀 Proyecto Integrador: Sistema de Gestión de Biblioteca

### Descripción
Crea un sistema completo de gestión de biblioteca usando Entity Framework Core.

### Requisitos
- Modelo de datos completo con entidades relacionadas
- Operaciones CRUD para libros, usuarios y préstamos
- Sistema de búsqueda y filtrado avanzado
- Gestión de préstamos con validaciones de negocio
- Reportes y estadísticas usando LINQ
- Migraciones y versionado de base de datos

### Estructura Sugerida
```
LibraryManagement/
├── LibraryManagement.Domain/
│   ├── Entities/
│   ├── Enums/
│   └── DTOs/
├── LibraryManagement.Infrastructure/
│   ├── Data/
│   ├── Repositories/
│   └── Services/
├── LibraryManagement.API/
└── LibraryManagement.Tests/
```

### Ejemplo de Implementación

```csharp
// Entidades principales
public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string ISBN { get; set; }
    public string Author { get; set; }
    public string Publisher { get; set; }
    public int PublicationYear { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public string Category { get; set; }
    
    public virtual ICollection<Loan> Loans { get; set; }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime RegistrationDate { get; set; }
    public UserStatus Status { get; set; }
    
    public virtual ICollection<Loan> Loans { get; set; }
}

public class Loan
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public int UserId { get; set; }
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public LoanStatus Status { get; set; }
    
    public virtual Book Book { get; set; }
    public virtual User User { get; set; }
}

// Servicio de préstamos
public class LoanService
{
    private readonly ApplicationDbContext _context;
    
    public LoanService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Loan> CreateLoanAsync(int bookId, int userId, int daysToReturn = 14)
    {
        var book = await _context.Books.FindAsync(bookId);
        if (book == null)
            throw new InvalidOperationException("Book not found");
            
        if (book.AvailableCopies <= 0)
            throw new InvalidOperationException("No copies available");
            
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");
            
        if (user.Status != UserStatus.Active)
            throw new InvalidOperationException("User account is not active");
            
        // Verificar si ya tiene un préstamo activo del mismo libro
        var activeLoan = await _context.Loans
            .FirstOrDefaultAsync(l => l.BookId == bookId && 
                                    l.UserId == userId && 
                                    l.Status == LoanStatus.Active);
                                    
        if (activeLoan != null)
            throw new InvalidOperationException("User already has an active loan for this book");
        
        var loan = new Loan
        {
            BookId = bookId,
            UserId = userId,
            LoanDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(daysToReturn),
            Status = LoanStatus.Active
        };
        
        // Actualizar disponibilidad del libro
        book.AvailableCopies--;
        
        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();
        
        return loan;
    }
    
    public async Task<bool> ReturnBookAsync(int loanId)
    {
        var loan = await _context.Loans
            .Include(l => l.Book)
            .FirstOrDefaultAsync(l => l.Id == loanId);
            
        if (loan == null)
            return false;
            
        if (loan.Status != LoanStatus.Active)
            return false;
        
        loan.ReturnDate = DateTime.UtcNow;
        loan.Status = LoanStatus.Returned;
        
        // Restaurar disponibilidad del libro
        loan.Book.AvailableCopies++;
        
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<IEnumerable<OverdueLoan>> GetOverdueLoansAsync()
    {
        return await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.User)
            .Where(l => l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow)
            .Select(l => new OverdueLoan
            {
                LoanId = l.Id,
                BookTitle = l.Book.Title,
                UserName = l.User.Name,
                UserEmail = l.User.Email,
                DueDate = l.DueDate,
                DaysOverdue = (DateTime.UtcNow - l.DueDate).Days
            })
            .OrderByDescending(l => l.DaysOverdue)
            .ToListAsync();
    }
}
```

---

## 📝 Autoevaluación

### Preguntas Teóricas
1. ¿Cuál es la diferencia entre Eager Loading y Lazy Loading?
2. ¿Qué son las migraciones y por qué son importantes?
3. ¿Cuándo usarías el patrón Repository?
4. ¿Cómo optimizas consultas en Entity Framework?
5. ¿Qué ventajas ofrece Fluent API sobre Data Annotations?

### Preguntas Prácticas
1. Configura un DbContext para un sistema de gestión de inventario
2. Implementa operaciones CRUD para entidades relacionadas
3. Crea consultas LINQ optimizadas con projection
4. Implementa el patrón Repository con métodos genéricos

---

## 🔗 Enlaces de Referencia

- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [EF Core Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [EF Core Performance](https://docs.microsoft.com/en-us/ef/core/performance/)
- [Repository Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)

---

## 📚 Siguiente Nivel

**Progreso**: 9 de 12 niveles completados

**Siguiente**: [Senior Level 5: Arquitectura Limpia y Microservicios](../senior_5/README.md)

**Anterior**: [Senior Level 3: APIs REST y Web APIs](../senior_3/README.md)

---

## 🎉 ¡Felicidades!

Has completado el nivel senior de Entity Framework y bases de datos. Ahora puedes:
- Diseñar y configurar modelos de datos complejos
- Implementar operaciones CRUD eficientes y seguras
- Optimizar consultas y manejar relaciones complejas
- Gestionar migraciones y versionado de base de datos
- Aplicar patrones de acceso a datos en aplicaciones reales

¡Continúa con el siguiente nivel para dominar arquitectura limpia y microservicios!
