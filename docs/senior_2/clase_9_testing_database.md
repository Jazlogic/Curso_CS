# 🚀 Clase 9: Testing de Base de Datos

## 📋 Información de la Clase

- **Módulo**: Senior Level 2 - Testing y TDD
- **Duración**: 2 horas
- **Nivel**: Avanzado
- **Prerrequisitos**: Testing de APIs (Clase 8)

## 🎯 Objetivos de Aprendizaje

- Implementar testing de bases de datos
- Usar bases de datos en memoria para testing
- Probar operaciones CRUD y transacciones
- Implementar testing de integridad referencial

---

## 📚 Navegación del Módulo 2

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_fundamentos_testing.md) | Fundamentos de Testing | |
| [Clase 2](clase_2_desarrollo_dirigido_pruebas.md) | Desarrollo Dirigido por Pruebas (TDD) | |
| [Clase 3](clase_3_testing_unitario.md) | Testing Unitario | |
| [Clase 4](clase_4_testing_integracion.md) | Testing de Integración | |
| [Clase 5](clase_5_testing_comportamiento.md) | Testing de Comportamiento | |
| [Clase 6](clase_6_mocking_framworks.md) | Frameworks de Mocking | |
| [Clase 7](clase_7_testing_asincrono.md) | Testing de Código Asíncrono | |
| [Clase 8](clase_8_testing_apis.md) | Testing de APIs | ← Anterior |
| **Clase 9** | **Testing de Base de Datos** | ← Estás aquí |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Testing | |

**← [Volver al README del Módulo 2](../senior_2/README.md)**

---

## 📚 Contenido Teórico

### 1. ¿Qué es el Testing de Base de Datos?

El testing de base de datos verifica que las operaciones de datos funcionen correctamente, incluyendo CRUD, transacciones, y la integridad de los datos.

### 2. Características del Testing de Base de Datos

- **Bases de datos en memoria** para testing rápido
- **Testing de transacciones** y rollback
- **Verificación de integridad** referencial
- **Testing de performance** de consultas

```csharp
// ===== TESTING DE BASE DE DATOS - IMPLEMENTACIÓN COMPLETA =====
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DatabaseTesting
{
    // ===== MODELOS DE DOMINIO =====
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Order> Orders { get; set; } = new List<Order>();
    }
    
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Category { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
    
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
    }
    
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }
    
    // ===== CONTEXTO DE BASE DE DATOS =====
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración de relaciones
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId);
            
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId);
            
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId);
            
            // Configuración de índices
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Category);
        }
    }
    
    // ===== REPOSITORIOS =====
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(int id);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> CreateAsync(User user);
        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);
        Task<User> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetActiveUsersAsync();
    }
    
    public class UserRepository : IUserRepository
    {
        private readonly TestDbContext _context;
        
        public UserRepository(TestDbContext context)
        {
            _context = context;
        }
        
        public async Task<User> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.Orders)
                .ToListAsync();
        }
        
        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
        
        public async Task<bool> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        
        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;
            
            _context.Users.Remove(user);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        
        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(u => u.Email == email);
        }
        
        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Orders)
                .Where(u => u.IsActive)
                .ToListAsync();
        }
    }
    
    public interface IProductRepository
    {
        Task<Product> GetByIdAsync(int id);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> CreateAsync(Product product);
        Task<bool> UpdateAsync(Product product);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Product>> GetByCategoryAsync(string category);
        Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold);
    }
    
    public class ProductRepository : IProductRepository
    {
        private readonly TestDbContext _context;
        
        public ProductRepository(TestDbContext context)
        {
            _context = context;
        }
        
        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.OrderItems)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        
        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.OrderItems)
                .ToListAsync();
        }
        
        public async Task<Product> CreateAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }
        
        public async Task<bool> UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        
        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;
            
            _context.Products.Remove(product);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        
        public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
        {
            return await _context.Products
                .Where(p => p.Category == category)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold)
        {
            return await _context.Products
                .Where(p => p.Stock <= threshold)
                .ToListAsync();
        }
    }
    
    // ===== SERVICIOS DE NEGOCIO =====
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(int userId, List<OrderItem> items);
        Task<bool> UpdateOrderStatusAsync(int orderId, string status);
        Task<bool> CancelOrderAsync(int orderId);
        Task<Order> GetOrderByIdAsync(int id);
        Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
        Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate);
    }
    
    public class OrderService : IOrderService
    {
        private readonly TestDbContext _context;
        private readonly IProductRepository _productRepository;
        
        public OrderService(TestDbContext context, IProductRepository productRepository)
        {
            _context = context;
            _productRepository = productRepository;
        }
        
        public async Task<Order> CreateOrderAsync(int userId, List<OrderItem> items)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Verificar stock disponible
                foreach (var item in items)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product == null)
                        throw new ArgumentException($"Product {item.ProductId} not found");
                    
                    if (product.Stock < item.Quantity)
                        throw new InvalidOperationException($"Insufficient stock for product {product.Name}");
                }
                
                // Crear orden
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.Now,
                    Status = "Pending",
                    Items = items
                };
                
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                
                // Actualizar stock
                foreach (var item in items)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    product.Stock -= item.Quantity;
                    await _productRepository.UpdateAsync(product);
                }
                
                await transaction.CommitAsync();
                return order;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        
        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;
            
            order.Status = status;
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        
        public async Task<bool> CancelOrderAsync(int orderId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == orderId);
                
                if (order == null || order.Status != "Pending") return false;
                
                order.Status = "Cancelled";
                
                // Restaurar stock
                foreach (var item in order.Items)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    product.Stock += item.Quantity;
                    await _productRepository.UpdateAsync(product);
                }
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
        
        public async Task<Order> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
        }
        
        public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .ToListAsync();
        }
        
        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status == "Completed")
                .SumAsync(o => o.TotalAmount);
        }
    }
    
    // ===== FACTORY PARA TESTING =====
    public class TestFactory
    {
        public static ServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();
            
            services.AddDbContext<TestDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
            
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IOrderService, OrderService>();
            
            return services.BuildServiceProvider();
        }
        
        public static async Task SeedTestData(TestDbContext context)
        {
            // Crear usuarios de prueba
            var users = new List<User>
            {
                new User { Username = "user1", Email = "user1@example.com", IsActive = true, CreatedAt = DateTime.Now },
                new User { Username = "user2", Email = "user2@example.com", IsActive = true, CreatedAt = DateTime.Now },
                new User { Username = "user3", Email = "user3@example.com", IsActive = false, CreatedAt = DateTime.Now }
            };
            
            context.Users.AddRange(users);
            await context.SaveChangesAsync();
            
            // Crear productos de prueba
            var products = new List<Product>
            {
                new Product { Name = "Laptop", Price = 999.99m, Stock = 10, Category = "Electronics" },
                new Product { Name = "Mouse", Price = 29.99m, Stock = 50, Category = "Electronics" },
                new Product { Name = "Keyboard", Price = 59.99m, Stock = 25, Category = "Electronics" },
                new Product { Name = "Book", Price = 19.99m, Stock = 100, Category = "Books" }
            };
            
            context.Products.AddRange(products);
            await context.SaveChangesAsync();
        }
    }
    
    // ===== TESTING DE BASE DE DATOS =====
    public class DatabaseTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly TestDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderService _orderService;
        
        public DatabaseTests()
        {
            _serviceProvider = TestFactory.CreateServiceProvider();
            _context = _serviceProvider.GetRequiredService<TestDbContext>();
            _userRepository = _serviceProvider.GetRequiredService<IUserRepository>();
            _productRepository = _serviceProvider.GetRequiredService<IProductRepository>();
            _orderService = _serviceProvider.GetRequiredService<IOrderService>();
            
            _context.Database.EnsureCreated();
            TestFactory.SeedTestData(_context).Wait();
        }
        
        [Fact]
        public async Task CreateUser_ShouldPersistToDatabase()
        {
            // Arrange
            var newUser = new User
            {
                Username = "newuser",
                Email = "newuser@example.com",
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            
            // Act
            var createdUser = await _userRepository.CreateAsync(newUser);
            
            // Assert
            Assert.NotEqual(0, createdUser.Id);
            
            var retrievedUser = await _userRepository.GetByIdAsync(createdUser.Id);
            Assert.NotNull(retrievedUser);
            Assert.Equal(newUser.Username, retrievedUser.Username);
            Assert.Equal(newUser.Email, retrievedUser.Email);
        }
        
        [Fact]
        public async Task CreateUser_WithDuplicateEmail_ShouldThrowException()
        {
            // Arrange
            var existingUser = await _userRepository.GetByEmailAsync("user1@example.com");
            var duplicateUser = new User
            {
                Username = "duplicate",
                Email = existingUser.Email, // Email duplicado
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _userRepository.CreateAsync(duplicateUser));
        }
        
        [Fact]
        public async Task UpdateUser_ShouldModifyDatabase()
        {
            // Arrange
            var user = await _userRepository.GetByEmailAsync("user1@example.com");
            var originalUsername = user.Username;
            user.Username = "updateduser";
            
            // Act
            var result = await _userRepository.UpdateAsync(user);
            
            // Assert
            Assert.True(result);
            
            var updatedUser = await _userRepository.GetByIdAsync(user.Id);
            Assert.Equal("updateduser", updatedUser.Username);
            Assert.NotEqual(originalUsername, updatedUser.Username);
        }
        
        [Fact]
        public async Task DeleteUser_ShouldRemoveFromDatabase()
        {
            // Arrange
            var user = await _userRepository.GetByEmailAsync("user2@example.com");
            
            // Act
            var result = await _userRepository.DeleteAsync(user.Id);
            
            // Assert
            Assert.True(result);
            
            var deletedUser = await _userRepository.GetByIdAsync(user.Id);
            Assert.Null(deletedUser);
        }
        
        [Fact]
        public async Task GetActiveUsers_ShouldReturnOnlyActiveUsers()
        {
            // Act
            var activeUsers = await _userRepository.GetActiveUsersAsync();
            
            // Assert
            Assert.NotNull(activeUsers);
            Assert.All(activeUsers, u => Assert.True(u.IsActive));
            Assert.Equal(2, activeUsers.Count()); // Solo 2 usuarios activos
        }
        
        [Fact]
        public async Task CreateOrder_ShouldReduceProductStock()
        {
            // Arrange
            var user = await _userRepository.GetByEmailAsync("user1@example.com");
            var product = await _productRepository.GetByNameAsync("Laptop");
            var initialStock = product.Stock;
            
            var orderItems = new List<OrderItem>
            {
                new OrderItem { ProductId = product.Id, Quantity = 2, UnitPrice = product.Price }
            };
            
            // Act
            var order = await _orderService.CreateOrderAsync(user.Id, orderItems);
            
            // Assert
            Assert.NotNull(order);
            Assert.Equal("Pending", order.Status);
            
            var updatedProduct = await _productRepository.GetByIdAsync(product.Id);
            Assert.Equal(initialStock - 2, updatedProduct.Stock);
        }
        
        [Fact]
        public async Task CreateOrder_WithInsufficientStock_ShouldThrowException()
        {
            // Arrange
            var user = await _userRepository.GetByEmailAsync("user1@example.com");
            var product = await _productRepository.GetByNameAsync("Laptop");
            
            var orderItems = new List<OrderItem>
            {
                new OrderItem { ProductId = product.Id, Quantity = product.Stock + 1, UnitPrice = product.Price }
            };
            
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _orderService.CreateOrderAsync(user.Id, orderItems));
        }
        
        [Fact]
        public async Task CancelOrder_ShouldRestoreProductStock()
        {
            // Arrange
            var user = await _userRepository.GetByEmailAsync("user1@example.com");
            var product = await _productRepository.GetByNameAsync("Mouse");
            var initialStock = product.Stock;
            
            var orderItems = new List<OrderItem>
            {
                new OrderItem { ProductId = product.Id, Quantity = 5, UnitPrice = product.Price }
            };
            
            var order = await _orderService.CreateOrderAsync(user.Id, orderItems);
            
            // Act
            var result = await _orderService.CancelOrderAsync(order.Id);
            
            // Assert
            Assert.True(result);
            
            var updatedProduct = await _productRepository.GetByIdAsync(product.Id);
            Assert.Equal(initialStock, updatedProduct.Stock); // Stock restaurado
        }
        
        [Fact]
        public async Task GetTotalRevenue_ShouldCalculateCorrectly()
        {
            // Arrange
            var user = await _userRepository.GetByEmailAsync("user1@example.com");
            var product = await _productRepository.GetByNameAsync("Laptop");
            
            var orderItems = new List<OrderItem>
            {
                new OrderItem { ProductId = product.Id, Quantity = 1, UnitPrice = product.Price }
            };
            
            var order = await _orderService.CreateOrderAsync(user.Id, orderItems);
            await _orderService.UpdateOrderStatusAsync(order.Id, "Completed");
            
            // Act
            var revenue = await _orderService.GetTotalRevenueAsync(
                DateTime.Today, DateTime.Today.AddDays(1));
            
            // Assert
            Assert.Equal(product.Price, revenue);
        }
        
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _serviceProvider.Dispose();
        }
    }
}

// ===== DEMOSTRACIÓN DE TESTING DE BASE DE DATOS =====
public class DatabaseTestingDemonstration
{
    public static async Task DemonstrateDatabaseTesting()
    {
        Console.WriteLine("=== Testing de Base de Datos - Clase 9 ===\n");
        
        Console.WriteLine("1. CREANDO SERVICIOS DE BASE DE DATOS:");
        var serviceProvider = DatabaseTesting.TestFactory.CreateServiceProvider();
        var context = serviceProvider.GetRequiredService<DatabaseTesting.TestDbContext>();
        var userRepository = serviceProvider.GetRequiredService<DatabaseTesting.IUserRepository>();
        var productRepository = serviceProvider.GetRequiredService<DatabaseTesting.IProductRepository>();
        var orderService = serviceProvider.GetRequiredService<DatabaseTesting.IOrderService>();
        
        Console.WriteLine("✅ Servicios creados exitosamente");
        
        Console.WriteLine("\n2. CREANDO BASE DE DATOS Y DATOS DE PRUEBA:");
        context.Database.EnsureCreated();
        await DatabaseTesting.TestFactory.SeedTestData(context);
        
        Console.WriteLine("✅ Base de datos creada y poblada");
        
        Console.WriteLine("\n3. PROBANDO OPERACIONES CRUD:");
        var users = await userRepository.GetAllAsync();
        Console.WriteLine($"✅ Usuarios en la base de datos: {users.Count()}");
        
        var products = await productRepository.GetAllAsync();
        Console.WriteLine($"✅ Productos en la base de datos: {products.Count()}");
        
        Console.WriteLine("\n4. PROBANDO CREACIÓN DE ORDEN:");
        var user = users.First();
        var product = products.First();
        
        var orderItems = new List<DatabaseTesting.OrderItem>
        {
            new DatabaseTesting.OrderItem { ProductId = product.Id, Quantity = 1, UnitPrice = product.Price }
        };
        
        var order = await orderService.CreateOrderAsync(user.Id, orderItems);
        Console.WriteLine($"✅ Orden creada: ID {order.Id}, Total: ${order.TotalAmount}");
        
        Console.WriteLine("\n5. VERIFICANDO CAMBIOS EN STOCK:");
        var updatedProduct = await productRepository.GetByIdAsync(product.Id);
        Console.WriteLine($"✅ Stock actualizado: {updatedProduct.Stock}");
        
        Console.WriteLine("\n✅ Testing de Base de Datos demostrado!");
        Console.WriteLine("El testing de base de datos permite verificar operaciones CRUD, transacciones e integridad de datos.");
    }
}

// Programa principal
public class Program
{
    public static async Task Main()
    {
        await DatabaseTestingDemonstration.DemonstrateDatabaseTesting();
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Testing de Operaciones CRUD
Implementa pruebas para:
- Creación, lectura, actualización y eliminación de entidades
- Validación de restricciones de base de datos
- Manejo de errores de integridad

### Ejercicio 2: Testing de Transacciones
Crea pruebas que verifiquen:
- Rollback en caso de errores
- Consistencia de datos en operaciones complejas
- Aislamiento de transacciones

### Ejercicio 3: Testing de Relaciones
Implementa testing para:
- Integridad referencial
- Carga de entidades relacionadas
- Operaciones en cascada

## 🔍 Puntos Clave

1. **Bases de datos en memoria** permiten testing rápido y aislado
2. **Transacciones** aseguran consistencia de datos
3. **Relaciones** entre entidades deben probarse correctamente
4. **Rollback** en caso de errores mantiene integridad
5. **Seeding de datos** proporciona estado consistente
6. **Testing de constraints** verifica reglas de negocio
7. **Performance** de consultas debe monitorearse
8. **Cleanup** apropiado evita interferencia entre pruebas

## 📚 Recursos Adicionales

- [Entity Framework Testing](https://docs.microsoft.com/en-us/ef/core/testing/)
- [In-Memory Database](https://docs.microsoft.com/en-us/ef/core/testing/testing-with-the-database)
- [Database Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/test-aspnet-core-mvc)

---

**🎯 ¡Has completado la Clase 9! Ahora comprendes el Testing de Base de Datos**

**📚 [Siguiente: Clase 10 - Proyecto Final: Sistema de Testing](clase_10_proyecto_final.md)**
