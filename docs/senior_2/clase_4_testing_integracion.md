# 🚀 Clase 4: Testing de Integración

## 📋 Información de la Clase

- **Módulo**: Senior Level 2 - Testing y TDD
- **Duración**: 2 horas
- **Nivel**: Avanzado
- **Prerrequisitos**: Testing Unitario (Clase 3)

## 🎯 Objetivos de Aprendizaje

- Implementar testing de integración efectivo
- Probar interacciones entre componentes
- Usar bases de datos en memoria para testing
- Crear pruebas de integración mantenibles

---

## 📚 Navegación del Módulo 2

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_fundamentos_testing.md) | Fundamentos de Testing | |
| [Clase 2](clase_2_desarrollo_dirigido_pruebas.md) | Desarrollo Dirigido por Pruebas (TDD) | |
| [Clase 3](clase_3_testing_unitario.md) | Testing Unitario | ← Anterior |
| **Clase 4** | **Testing de Integración** | ← Estás aquí |
| [Clase 5](clase_5_testing_comportamiento.md) | Testing de Comportamiento | Siguiente → |
| [Clase 6](clase_6_mocking_frameworks.md) | Frameworks de Mocking | |
| [Clase 7](clase_7_testing_asincrono.md) | Testing de Código Asíncrono | |
| [Clase 8](clase_8_testing_apis.md) | Testing de APIs | |
| [Clase 9](clase_9_testing_database.md) | Testing de Base de Datos | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Testing | |

**← [Volver al README del Módulo 2](../senior_2/README.md)**

---

## 📚 Contenido Teórico

### 1. ¿Qué es el Testing de Integración?

El testing de integración verifica que múltiples componentes trabajen correctamente juntos, probando las interacciones entre ellos.

### 2. Características del Testing de Integración

- **Más lento** que las pruebas unitarias
- **Depende** de sistemas externos (DB, APIs)
- **Prueba flujos** completos de negocio
- **Detecta problemas** de integración

```csharp
// ===== TESTING DE INTEGRACIÓN - IMPLEMENTACIÓN COMPLETA =====
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTesting
{
    // ===== MODELOS DE DATOS =====
    namespace Models
    {
        public class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int Stock { get; set; }
            public string Category { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreatedDate { get; set; }
        }
        
        public class Order
        {
            public int Id { get; set; }
            public int CustomerId { get; set; }
            public List<OrderItem> Items { get; set; }
            public DateTime OrderDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string Status { get; set; }
        }
        
        public class OrderItem
        {
            public int Id { get; set; }
            public int OrderId { get; set; }
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice => Quantity * UnitPrice;
        }
        
        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public bool IsActive { get; set; }
            public decimal CreditLimit { get; set; }
        }
    }
    
    // ===== CONTEXTO DE BASE DE DATOS =====
    namespace Data
    {
        public class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
            
            public DbSet<Product> Products { get; set; }
            public DbSet<Order> Orders { get; set; }
            public DbSet<OrderItem> OrderItems { get; set; }
            public DbSet<Customer> Customers { get; set; }
            
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Order>()
                    .HasMany(o => o.Items)
                    .WithOne(i => i.Order)
                    .HasForeignKey(i => i.OrderId);
                
                modelBuilder.Entity<OrderItem>()
                    .HasOne(i => i.Order)
                    .WithMany(o => o.Items)
                    .HasForeignKey(i => i.OrderId);
            }
        }
    }
    
    // ===== REPOSITORIOS =====
    namespace Repositories
    {
        public interface IProductRepository
        {
            Task<Product> GetByIdAsync(int id);
            Task<IEnumerable<Product>> GetAllAsync();
            Task<Product> CreateAsync(Product product);
            Task<bool> UpdateAsync(Product product);
            Task<bool> DeleteAsync(int id);
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
                return await _context.Products.FindAsync(id);
            }
            
            public async Task<IEnumerable<Product>> GetAllAsync()
            {
                return await _context.Products.ToListAsync();
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
        }
        
        public interface IOrderRepository
        {
            Task<Order> GetByIdAsync(int id);
            Task<IEnumerable<Order>> GetByCustomerAsync(int customerId);
            Task<Order> CreateAsync(Order order);
            Task<bool> UpdateAsync(Order order);
        }
        
        public class OrderRepository : IOrderRepository
        {
            private readonly TestDbContext _context;
            
            public OrderRepository(TestDbContext context)
            {
                _context = context;
            }
            
            public async Task<Order> GetByIdAsync(int id)
            {
                return await _context.Orders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == id);
            }
            
            public async Task<IEnumerable<Order>> GetByCustomerAsync(int customerId)
            {
                return await _context.Orders
                    .Include(o => o.Items)
                    .Where(o => o.CustomerId == customerId)
                    .ToListAsync();
            }
            
            public async Task<Order> CreateAsync(Order order)
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                return order;
            }
            
            public async Task<bool> UpdateAsync(Order order)
            {
                _context.Orders.Update(order);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
        }
    }
    
    // ===== SERVICIOS DE NEGOCIO =====
    namespace Services
    {
        public interface IOrderService
        {
            Task<Order> CreateOrderAsync(int customerId, List<OrderItem> items);
            Task<bool> ProcessOrderAsync(int orderId);
            Task<decimal> CalculateOrderTotalAsync(int orderId);
        }
        
        public class OrderService : IOrderService
        {
            private readonly IOrderRepository _orderRepository;
            private readonly IProductRepository _productRepository;
            
            public OrderService(IOrderRepository orderRepository, IProductRepository productRepository)
            {
                _orderRepository = orderRepository;
                _productRepository = productRepository;
            }
            
            public async Task<Order> CreateOrderAsync(int customerId, List<OrderItem> items)
            {
                if (items == null || !items.Any())
                    throw new ArgumentException("Order must have at least one item");
                
                var order = new Order
                {
                    CustomerId = customerId,
                    Items = items,
                    OrderDate = DateTime.Now,
                    Status = "Pending"
                };
                
                // Calcular total
                order.TotalAmount = items.Sum(i => i.TotalPrice);
                
                return await _orderRepository.CreateAsync(order);
            }
            
            public async Task<bool> ProcessOrderAsync(int orderId)
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null) return false;
                
                if (order.Status != "Pending") return false;
                
                // Verificar stock
                foreach (var item in order.Items)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product == null || product.Stock < item.Quantity)
                        return false;
                }
                
                // Actualizar stock
                foreach (var item in order.Items)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    product.Stock -= item.Quantity;
                    await _productRepository.UpdateAsync(product);
                }
                
                order.Status = "Processed";
                return await _orderRepository.UpdateAsync(order);
            }
            
            public async Task<decimal> CalculateOrderTotalAsync(int orderId)
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                return order?.TotalAmount ?? 0;
            }
        }
    }
    
    // ===== FACTORY PARA TESTING =====
    namespace TestFactory
    {
        public class TestFactory
        {
            public static ServiceProvider CreateServiceProvider()
            {
                var services = new ServiceCollection();
                
                services.AddDbContext<TestDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));
                
                services.AddScoped<IProductRepository, ProductRepository>();
                services.AddScoped<IOrderRepository, OrderRepository>();
                services.AddScoped<IOrderService, OrderService>();
                
                return services.BuildServiceProvider();
            }
            
            public static async Task SeedTestData(TestDbContext context)
            {
                // Agregar productos de prueba
                var products = new List<Product>
                {
                    new Product { Name = "Laptop", Price = 999.99m, Stock = 10, Category = "Electronics", IsActive = true, CreatedDate = DateTime.Now },
                    new Product { Name = "Mouse", Price = 29.99m, Stock = 50, Category = "Electronics", IsActive = true, CreatedDate = DateTime.Now },
                    new Product { Name = "Keyboard", Price = 59.99m, Stock = 25, Category = "Electronics", IsActive = true, CreatedDate = DateTime.Now }
                };
                
                context.Products.AddRange(products);
                await context.SaveChangesAsync();
                
                // Agregar cliente de prueba
                var customer = new Customer { Name = "Test Customer", Email = "test@example.com", IsActive = true, CreditLimit = 1000.00m };
                context.Customers.Add(customer);
                await context.SaveChangesAsync();
            }
        }
    }
    
    // ===== PRUEBAS DE INTEGRACIÓN =====
    namespace IntegrationTests
    {
        public class OrderServiceIntegrationTests : IDisposable
        {
            private readonly ServiceProvider _serviceProvider;
            private readonly TestDbContext _context;
            private readonly IOrderService _orderService;
            private readonly IProductRepository _productRepository;
            
            public OrderServiceIntegrationTests()
            {
                _serviceProvider = TestFactory.TestFactory.CreateServiceProvider();
                _context = _serviceProvider.GetRequiredService<TestDbContext>();
                _orderService = _serviceProvider.GetRequiredService<IOrderService>();
                _productRepository = _serviceProvider.GetRequiredService<IProductRepository>();
                
                // Crear base de datos y datos de prueba
                _context.Database.EnsureCreated();
                TestFactory.TestFactory.SeedTestData(_context).Wait();
            }
            
            [Fact]
            public async Task CreateOrder_WithValidItems_CreatesOrderSuccessfully()
            {
                // Arrange
                var products = await _productRepository.GetAllAsync();
                var firstProduct = products.First();
                
                var orderItems = new List<OrderItem>
                {
                    new OrderItem { ProductId = firstProduct.Id, Quantity = 2, UnitPrice = firstProduct.Price }
                };
                
                // Act
                var order = await _orderService.CreateOrderAsync(1, orderItems);
                
                // Assert
                Assert.NotNull(order);
                Assert.Equal(1, order.CustomerId);
                Assert.Equal("Pending", order.Status);
                Assert.Equal(orderItems.Sum(i => i.TotalPrice), order.TotalAmount);
                Assert.Single(order.Items);
            }
            
            [Fact]
            public async Task CreateOrder_WithEmptyItems_ThrowsException()
            {
                // Arrange
                var emptyItems = new List<OrderItem>();
                
                // Act & Assert
                await Assert.ThrowsAsync<ArgumentException>(() => 
                    _orderService.CreateOrderAsync(1, emptyItems));
            }
            
            [Fact]
            public async Task ProcessOrder_WithSufficientStock_ProcessesOrderSuccessfully()
            {
                // Arrange
                var products = await _productRepository.GetAllAsync();
                var product = products.First();
                var initialStock = product.Stock;
                
                var orderItems = new List<OrderItem>
                {
                    new OrderItem { ProductId = product.Id, Quantity = 2, UnitPrice = product.Price }
                };
                
                var order = await _orderService.CreateOrderAsync(1, orderItems);
                
                // Act
                var result = await _orderService.ProcessOrderAsync(order.Id);
                
                // Assert
                Assert.True(result);
                
                var processedOrder = await _orderService.CalculateOrderTotalAsync(order.Id);
                Assert.Equal(order.TotalAmount, processedOrder);
                
                var updatedProduct = await _productRepository.GetByIdAsync(product.Id);
                Assert.Equal(initialStock - 2, updatedProduct.Stock);
            }
            
            [Fact]
            public async Task ProcessOrder_WithInsufficientStock_ReturnsFalse()
            {
                // Arrange
                var products = await _productRepository.GetAllAsync();
                var product = products.First();
                
                var orderItems = new List<OrderItem>
                {
                    new OrderItem { ProductId = product.Id, Quantity = product.Stock + 1, UnitPrice = product.Price }
                };
                
                var order = await _orderService.CreateOrderAsync(1, orderItems);
                
                // Act
                var result = await _orderService.ProcessOrderAsync(order.Id);
                
                // Assert
                Assert.False(result);
                
                var updatedProduct = await _productRepository.GetByIdAsync(product.Id);
                Assert.Equal(product.Stock, updatedProduct.Stock); // Stock no cambió
            }
            
            [Fact]
            public async Task ProcessOrder_WithNonExistentOrder_ReturnsFalse()
            {
                // Act
                var result = await _orderService.ProcessOrderAsync(999);
                
                // Assert
                Assert.False(result);
            }
            
            [Fact]
            public async Task CalculateOrderTotal_WithValidOrder_ReturnsCorrectTotal()
            {
                // Arrange
                var products = await _productRepository.GetAllAsync();
                var product1 = products.First();
                var product2 = products.Skip(1).First();
                
                var orderItems = new List<OrderItem>
                {
                    new OrderItem { ProductId = product1.Id, Quantity = 2, UnitPrice = product1.Price },
                    new OrderItem { ProductId = product2.Id, Quantity = 1, UnitPrice = product2.Price }
                };
                
                var order = await _orderService.CreateOrderAsync(1, orderItems);
                var expectedTotal = orderItems.Sum(i => i.TotalPrice);
                
                // Act
                var actualTotal = await _orderService.CalculateOrderTotalAsync(order.Id);
                
                // Assert
                Assert.Equal(expectedTotal, actualTotal);
            }
            
            [Fact]
            public async Task CalculateOrderTotal_WithNonExistentOrder_ReturnsZero()
            {
                // Act
                var total = await _orderService.CalculateOrderTotalAsync(999);
                
                // Assert
                Assert.Equal(0, total);
            }
            
            public void Dispose()
            {
                _context.Database.EnsureDeleted();
                _serviceProvider.Dispose();
            }
        }
        
        public class ProductRepositoryIntegrationTests : IDisposable
        {
            private readonly ServiceProvider _serviceProvider;
            private readonly TestDbContext _context;
            private readonly IProductRepository _productRepository;
            
            public ProductRepositoryIntegrationTests()
            {
                _serviceProvider = TestFactory.TestFactory.CreateServiceProvider();
                _context = _serviceProvider.GetRequiredService<TestDbContext>();
                _productRepository = _serviceProvider.GetRequiredService<IProductRepository>();
                
                _context.Database.EnsureCreated();
                TestFactory.TestFactory.SeedTestData(_context).Wait();
            }
            
            [Fact]
            public async Task GetById_WithExistingProduct_ReturnsProduct()
            {
                // Arrange
                var products = await _productRepository.GetAllAsync();
                var expectedProduct = products.First();
                
                // Act
                var actualProduct = await _productRepository.GetByIdAsync(expectedProduct.Id);
                
                // Assert
                Assert.NotNull(actualProduct);
                Assert.Equal(expectedProduct.Name, actualProduct.Name);
                Assert.Equal(expectedProduct.Price, actualProduct.Price);
            }
            
            [Fact]
            public async Task GetById_WithNonExistentProduct_ReturnsNull()
            {
                // Act
                var product = await _productRepository.GetByIdAsync(999);
                
                // Assert
                Assert.Null(product);
            }
            
            [Fact]
            public async Task Create_WithValidProduct_CreatesProductSuccessfully()
            {
                // Arrange
                var newProduct = new Product
                {
                    Name = "New Product",
                    Price = 99.99m,
                    Stock = 5,
                    Category = "Test",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };
                
                // Act
                var createdProduct = await _productRepository.CreateAsync(newProduct);
                
                // Assert
                Assert.NotNull(createdProduct);
                Assert.NotEqual(0, createdProduct.Id);
                Assert.Equal("New Product", createdProduct.Name);
                
                var retrievedProduct = await _productRepository.GetByIdAsync(createdProduct.Id);
                Assert.NotNull(retrievedProduct);
                Assert.Equal("New Product", retrievedProduct.Name);
            }
            
            [Fact]
            public async Task Update_WithValidProduct_UpdatesProductSuccessfully()
            {
                // Arrange
                var products = await _productRepository.GetAllAsync();
                var product = products.First();
                var originalPrice = product.Price;
                var newPrice = originalPrice + 10.00m;
                
                product.Price = newPrice;
                
                // Act
                var result = await _productRepository.UpdateAsync(product);
                
                // Assert
                Assert.True(result);
                
                var updatedProduct = await _productRepository.GetByIdAsync(product.Id);
                Assert.Equal(newPrice, updatedProduct.Price);
            }
            
            [Fact]
            public async Task Delete_WithExistingProduct_DeletesProductSuccessfully()
            {
                // Arrange
                var products = await _productRepository.GetAllAsync();
                var productToDelete = products.First();
                
                // Act
                var result = await _productRepository.DeleteAsync(productToDelete.Id);
                
                // Assert
                Assert.True(result);
                
                var deletedProduct = await _productRepository.GetByIdAsync(productToDelete.Id);
                Assert.Null(deletedProduct);
            }
            
            [Fact]
            public async Task Delete_WithNonExistentProduct_ReturnsFalse()
            {
                // Act
                var result = await _productRepository.DeleteAsync(999);
                
                // Assert
                Assert.False(result);
            }
            
            public void Dispose()
            {
                _context.Database.EnsureDeleted();
                _serviceProvider.Dispose();
            }
        }
    }
}

// ===== DEMOSTRACIÓN DE TESTING DE INTEGRACIÓN =====
public class IntegrationTestingDemonstration
{
    public static async Task DemonstrateIntegrationTesting()
    {
        Console.WriteLine("=== Testing de Integración - Clase 4 ===\n");
        
        // Crear proveedor de servicios para testing
        var serviceProvider = IntegrationTesting.TestFactory.TestFactory.CreateServiceProvider();
        var context = serviceProvider.GetRequiredService<IntegrationTesting.Data.TestDbContext>();
        var orderService = serviceProvider.GetRequiredService<IntegrationTesting.Services.IOrderService>();
        var productRepository = serviceProvider.GetRequiredService<IntegrationTesting.Repositories.IProductRepository>();
        
        try
        {
            // Crear base de datos y datos de prueba
            context.Database.EnsureCreated();
            await IntegrationTesting.TestFactory.TestFactory.SeedTestData(context);
            
            Console.WriteLine("1. BASE DE DATOS CREADA:");
            var products = await productRepository.GetAllAsync();
            Console.WriteLine($"Productos disponibles: {products.Count()}");
            foreach (var product in products)
            {
                Console.WriteLine($"  - {product.Name}: ${product.Price} (Stock: {product.Stock})");
            }
            
            Console.WriteLine("\n2. CREAR ORDEN:");
            var orderItems = new List<IntegrationTesting.Models.OrderItem>
            {
                new IntegrationTesting.Models.OrderItem 
                { 
                    ProductId = products.First().Id, 
                    Quantity = 2, 
                    UnitPrice = products.First().Price 
                }
            };
            
            var order = await orderService.CreateOrderAsync(1, orderItems);
            Console.WriteLine($"Orden creada: ID {order.Id}, Total: ${order.TotalAmount}, Estado: {order.Status}");
            
            Console.WriteLine("\n3. PROCESAR ORDEN:");
            var processResult = await orderService.ProcessOrderAsync(order.Id);
            Console.WriteLine($"Orden procesada: {processResult}");
            
            if (processResult)
            {
                var total = await orderService.CalculateOrderTotalAsync(order.Id);
                Console.WriteLine($"Total de la orden: ${total}");
                
                var updatedProduct = await productRepository.GetByIdAsync(products.First().Id);
                Console.WriteLine($"Stock actualizado: {updatedProduct.Stock}");
            }
            
            Console.WriteLine("\n✅ Testing de Integración demostrado!");
            Console.WriteLine("Las pruebas de integración verifican que los componentes trabajen juntos correctamente.");
        }
        finally
        {
            // Limpiar recursos
            context.Database.EnsureDeleted();
            serviceProvider.Dispose();
        }
    }
}

// Programa principal
public class Program
{
    public static async Task Main()
    {
        await IntegrationTestingDemonstration.DemonstrateIntegrationTesting();
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Testing de Servicios Complejos
Implementa pruebas de integración para:
- Flujo completo de creación de usuario
- Proceso de autenticación y autorización
- Sistema de notificaciones

### Ejercicio 2: Testing de APIs
Crea pruebas de integración para:
- Endpoints de REST API
- Validación de respuestas HTTP
- Manejo de errores

### Ejercicio 3: Testing de Base de Datos
Implementa pruebas que verifiquen:
- Transacciones complejas
- Integridad referencial
- Performance de consultas

## 🔍 Puntos Clave

1. **Testing de integración** verifica interacciones entre componentes
2. **Bases de datos en memoria** facilitan testing sin dependencias externas
3. **Factory pattern** simplifica la configuración de pruebas
4. **Seeding de datos** proporciona estado consistente para testing
5. **Cleanup apropiado** evita interferencia entre pruebas
6. **Pruebas de flujos completos** detectan problemas de integración
7. **Aislamiento de pruebas** mantiene independencia entre casos
8. **Verificación de estado** asegura cambios persistentes

## 📚 Recursos Adicionales

- [Entity Framework Testing](https://docs.microsoft.com/en-us/ef/core/testing/)
- [Integration Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/test-aspnet-core-mvc)
- [Testing with In-Memory Database](https://docs.microsoft.com/en-us/ef/core/testing/testing-with-the-database)

---

**🎯 ¡Has completado la Clase 4! Ahora comprendes el Testing de Integración**

**📚 [Siguiente: Clase 5 - Testing de Comportamiento](clase_5_testing_comportamiento.md)**
