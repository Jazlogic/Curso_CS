# 🚀 Clase 6: LINQ to SQL

## 📋 Información de la Clase

- **Módulo**: Mid Level 4 - LINQ y Expresiones Lambda
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Conocimientos de LINQ y SQL básico

## 🎯 Objetivos de Aprendizaje

- Dominar LINQ to SQL para consultar bases de datos relacionales
- Crear y configurar DataContext y entidades
- Implementar operaciones CRUD con LINQ to SQL
- Optimizar consultas y manejar transacciones

---

## 📚 Navegación del Módulo 4

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_expresiones_lambda_avanzadas.md) | Expresiones Lambda Avanzadas | |
| [Clase 2](clase_2_operadores_linq_basicos.md) | Operadores LINQ Básicos | |
| [Clase 3](clase_3_operadores_linq_avanzados.md) | Operadores LINQ Avanzados | |
| [Clase 4](clase_4_linq_to_objects.md) | LINQ to Objects | |
| [Clase 5](clase_5_linq_to_xml.md) | LINQ to XML | ← Anterior |
| **Clase 6** | **LINQ to SQL** | ← Estás aquí |
| [Clase 7](clase_7_linq_performance.md) | LINQ y Performance | Siguiente → |
| [Clase 8](clase_8_linq_optimization.md) | Optimización de LINQ | |
| [Clase 9](clase_9_linq_extension_methods.md) | Métodos de Extensión LINQ | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Biblioteca | |

**← [Volver al README del Módulo 4](../midLevel_4/README.md)**

---

## 📚 Contenido Teórico

### 1. LINQ to SQL

LINQ to SQL permite consultar bases de datos SQL Server usando la sintaxis de LINQ.

```csharp
// ===== LINQ TO SQL - IMPLEMENTACIÓN COMPLETA =====
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace LinqToSql
{
    // ===== ENTIDADES DE BASE DE DATOS =====
    namespace Entities
    {
        [Table(Name = "Products")]
        public class Product
        {
            [Column(IsPrimaryKey = true, IsDbGenerated = true)]
            public int Id { get; set; }
            
            [Column(CanBeNull = false)]
            public string Name { get; set; }
            
            [Column(DbType = "decimal(18,2)")]
            public decimal Price { get; set; }
            
            [Column(CanBeNull = false)]
            public string Category { get; set; }
            
            [Column]
            public int Stock { get; set; }
            
            [Column]
            public DateTime CreatedDate { get; set; }
            
            [Column]
            public bool IsActive { get; set; }
            
            [Column]
            public int? SupplierId { get; set; }
            
            // Relación con Supplier
            [Association(Storage = "_Supplier", ThisKey = "SupplierId", OtherKey = "Id")]
            public Supplier Supplier
            {
                get { return _Supplier.Entity; }
                set { _Supplier.Entity = value; }
            }
            private EntityRef<Supplier> _Supplier;
        }
        
        [Table(Name = "Customers")]
        public class Customer
        {
            [Column(IsPrimaryKey = true, IsDbGenerated = true)]
            public int Id { get; set; }
            
            [Column(CanBeNull = false)]
            public string Name { get; set; }
            
            [Column(CanBeNull = false)]
            public string Email { get; set; }
            
            [Column]
            public int Age { get; set; }
            
            [Column]
            public string City { get; set; }
            
            [Column(DbType = "decimal(18,2)")]
            public decimal TotalSpent { get; set; }
            
            [Column]
            public DateTime RegistrationDate { get; set; }
            
            // Relación con Orders
            [Association(Storage = "_Orders", OtherKey = "CustomerId")]
            public EntitySet<Order> Orders
            {
                get { return _Orders; }
                set { _Orders.Assign(value); }
            }
            private EntitySet<Order> _Orders = new EntitySet<Order>();
        }
        
        [Table(Name = "Orders")]
        public class Order
        {
            [Column(IsPrimaryKey = true, IsDbGenerated = true)]
            public int Id { get; set; }
            
            [Column(CanBeNull = false)]
            public int CustomerId { get; set; }
            
            [Column]
            public DateTime OrderDate { get; set; }
            
            [Column(DbType = "decimal(18,2)")]
            public decimal TotalAmount { get; set; }
            
            [Column]
            public string Status { get; set; }
            
            // Relaciones
            [Association(Storage = "_Customer", ThisKey = "CustomerId", OtherKey = "Id")]
            public Customer Customer
            {
                get { return _Customer.Entity; }
                set { _Customer.Entity = value; }
            }
            private EntityRef<Customer> _Customer;
            
            [Association(Storage = "_OrderItems", OtherKey = "OrderId")]
            public EntitySet<OrderItem> OrderItems
            {
                get { return _OrderItems; }
                set { _OrderItems.Assign(value); }
            }
            private EntitySet<OrderItem> _OrderItems = new EntitySet<OrderItem>();
        }
        
        [Table(Name = "OrderItems")]
        public class OrderItem
        {
            [Column(IsPrimaryKey = true, IsDbGenerated = true)]
            public int Id { get; set; }
            
            [Column(CanBeNull = false)]
            public int OrderId { get; set; }
            
            [Column(CanBeNull = false)]
            public int ProductId { get; set; }
            
            [Column]
            public int Quantity { get; set; }
            
            [Column(DbType = "decimal(18,2)")]
            public decimal UnitPrice { get; set; }
            
            [Column(DbType = "decimal(18,2)")]
            public decimal TotalPrice { get; set; }
            
            // Relaciones
            [Association(Storage = "_Order", ThisKey = "OrderId", OtherKey = "Id")]
            public Order Order
            {
                get { return _Order.Entity; }
                set { _Order.Entity = value; }
            }
            private EntityRef<Order> _Order;
            
            [Association(Storage = "_Product", ThisKey = "ProductId", OtherKey = "Id")]
            public Product Product
            {
                get { return _Product.Entity; }
                set { _Product.Entity = value; }
            }
            private EntityRef<Product> _Product;
        }
        
        [Table(Name = "Suppliers")]
        public class Supplier
        {
            [Column(IsPrimaryKey = true, IsDbGenerated = true)]
            public int Id { get; set; }
            
            [Column(CanBeNull = false)]
            public string Name { get; set; }
            
            [Column]
            public string ContactEmail { get; set; }
            
            [Column]
            public string Phone { get; set; }
            
            [Column]
            public string Address { get; set; }
            
            [Column]
            public bool IsActive { get; set; }
            
            [Column(DbType = "decimal(3,1)")]
            public decimal Rating { get; set; }
            
            // Relación con Products
            [Association(Storage = "_Products", OtherKey = "SupplierId")]
            public EntitySet<Product> Products
            {
                get { return _Products; }
                set { _Products.Assign(value); }
            }
            private EntitySet<Product> _Products = new EntitySet<Product>();
        }
    }
    
    // ===== DATACONTEXT =====
    namespace DataContext
    {
        public class StoreDataContext : System.Data.Linq.DataContext
        {
            public Table<Product> Products;
            public Table<Customer> Customers;
            public Table<Order> Orders;
            public Table<OrderItem> OrderItems;
            public Table<Supplier> Suppliers;
            
            public StoreDataContext(string connectionString) : base(connectionString)
            {
                Products = GetTable<Product>();
                Customers = GetTable<Customer>();
                Orders = GetTable<Order>();
                OrderItems = GetTable<OrderItem>();
                Suppliers = GetTable<Supplier>();
            }
            
            // Configuración de mapeo
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                // Configuraciones adicionales si es necesario
            }
        }
    }
    
    // ===== CONSULTAS BÁSICAS =====
    namespace BasicQueries
    {
        public class BasicQueryExamples
        {
            private readonly StoreDataContext _context;
            
            public BasicQueryExamples(StoreDataContext context)
            {
                _context = context;
            }
            
            // Consulta simple
            public IEnumerable<Product> GetAllProducts()
            {
                return from p in _context.Products
                       select p;
            }
            
            // Consulta con filtro
            public IEnumerable<Product> GetActiveProducts()
            {
                return from p in _context.Products
                       where p.IsActive
                       select p;
            }
            
            // Consulta con ordenamiento
            public IEnumerable<Product> GetProductsOrderedByPrice()
            {
                return from p in _context.Products
                       orderby p.Price descending
                       select p;
            }
            
            // Consulta con proyección
            public IEnumerable<object> GetProductSummary()
            {
                return from p in _context.Products
                       select new
                       {
                           p.Id,
                           p.Name,
                           p.Price,
                           p.Category,
                           IsExpensive = p.Price > 100
                       };
            }
            
            // Consulta con múltiples condiciones
            public IEnumerable<Product> GetExpensiveActiveProducts(decimal minPrice)
            {
                return from p in _context.Products
                       where p.IsActive && p.Price >= minPrice && p.Stock > 0
                       select p;
            }
        }
    }
    
    // ===== CONSULTAS AVANZADAS =====
    namespace AdvancedQueries
    {
        public class AdvancedQueryExamples
        {
            private readonly StoreDataContext _context;
            
            public AdvancedQueryExamples(StoreDataContext context)
            {
                _context = context;
            }
            
            // Consulta con join
            public IEnumerable<object> GetProductsWithSuppliers()
            {
                return from p in _context.Products
                       join s in _context.Suppliers on p.SupplierId equals s.Id
                       select new
                       {
                           p.Name,
                           p.Price,
                           p.Category,
                           SupplierName = s.Name,
                           SupplierEmail = s.ContactEmail
                       };
            }
            
            // Consulta con group by
            public IEnumerable<object> GetCategoryStats()
            {
                return from p in _context.Products
                       group p by p.Category into g
                       select new
                       {
                           Category = g.Key,
                           ProductCount = g.Count(),
                           AveragePrice = g.Average(p => p.Price),
                           TotalStock = g.Sum(p => p.Stock)
                       };
            }
            
            // Consulta con subconsulta
            public IEnumerable<Customer> GetCustomersWithOrders()
            {
                return from c in _context.Customers
                       where c.Orders.Any()
                       select c;
            }
            
            // Consulta con agregación compleja
            public object GetSalesAnalytics()
            {
                var orderStats = from o in _context.Orders
                                group o by 1 into g
                                select new
                                {
                                    TotalOrders = g.Count(),
                                    TotalRevenue = g.Sum(o => o.TotalAmount),
                                    AverageOrderValue = g.Average(o => o.TotalAmount)
                                };
                
                return orderStats.FirstOrDefault();
            }
            
            // Consulta con paginación
            public IEnumerable<Product> GetProductsPage(int pageNumber, int pageSize)
            {
                return (from p in _context.Products
                        orderby p.Name
                        select p)
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize);
            }
        }
    }
    
    // ===== OPERACIONES CRUD =====
    namespace CrudOperations
    {
        public class CrudExamples
        {
            private readonly StoreDataContext _context;
            
            public CrudExamples(StoreDataContext context)
            {
                _context = context;
            }
            
            // CREATE - Insertar producto
            public void AddProduct(Product product)
            {
                _context.Products.InsertOnSubmit(product);
                _context.SubmitChanges();
            }
            
            // READ - Obtener producto por ID
            public Product GetProductById(int id)
            {
                return (from p in _context.Products
                        where p.Id == id
                        select p).FirstOrDefault();
            }
            
            // UPDATE - Actualizar producto
            public void UpdateProduct(int id, decimal newPrice)
            {
                var product = GetProductById(id);
                if (product != null)
                {
                    product.Price = newPrice;
                    _context.SubmitChanges();
                }
            }
            
            // DELETE - Eliminar producto
            public void DeleteProduct(int id)
            {
                var product = GetProductById(id);
                if (product != null)
                {
                    _context.Products.DeleteOnSubmit(product);
                    _context.SubmitChanges();
                }
            }
            
            // Operaciones en lote
            public void UpdateProductPrices(string category, decimal discountPercent)
            {
                var products = from p in _context.Products
                              where p.Category == category
                              select p;
                
                foreach (var product in products)
                {
                    product.Price *= (1 - discountPercent / 100);
                }
                
                _context.SubmitChanges();
            }
            
            // Insertar múltiples productos
            public void AddProducts(IEnumerable<Product> products)
            {
                _context.Products.InsertAllOnSubmit(products);
                _context.SubmitChanges();
            }
        }
    }
    
    // ===== TRANSACCIONES =====
    namespace Transactions
    {
        public class TransactionExamples
        {
            private readonly StoreDataContext _context;
            
            public TransactionExamples(StoreDataContext context)
            {
                _context = context;
            }
            
            // Transacción simple
            public void CreateOrderWithItems(Order order, List<OrderItem> items)
            {
                using (var transaction = _context.Connection.BeginTransaction())
                {
                    try
                    {
                        _context.Transaction = transaction;
                        
                        // Insertar orden
                        _context.Orders.InsertOnSubmit(order);
                        _context.SubmitChanges();
                        
                        // Insertar items
                        foreach (var item in items)
                        {
                            item.OrderId = order.Id;
                        }
                        _context.OrderItems.InsertAllOnSubmit(items);
                        _context.SubmitChanges();
                        
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            
            // Transacción con validación
            public bool ProcessOrder(int orderId, string newStatus)
            {
                using (var transaction = _context.Connection.BeginTransaction())
                {
                    try
                    {
                        _context.Transaction = transaction;
                        
                        var order = (from o in _context.Orders
                                    where o.Id == orderId
                                    select o).FirstOrDefault();
                        
                        if (order == null)
                        {
                            transaction.Rollback();
                            return false;
                        }
                        
                        order.Status = newStatus;
                        
                        // Actualizar stock de productos
                        var orderItems = from oi in _context.OrderItems
                                        where oi.OrderId == orderId
                                        select oi;
                        
                        foreach (var item in orderItems)
                        {
                            var product = (from p in _context.Products
                                          where p.Id == item.ProductId
                                          select p).FirstOrDefault();
                            
                            if (product != null)
                            {
                                product.Stock -= item.Quantity;
                                if (product.Stock < 0)
                                {
                                    transaction.Rollback();
                                    return false;
                                }
                            }
                        }
                        
                        _context.SubmitChanges();
                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }
    }
    
    // ===== OPTIMIZACIÓN DE CONSULTAS =====
    namespace QueryOptimization
    {
        public class OptimizationExamples
        {
            private readonly StoreDataContext _context;
            
            public OptimizationExamples(StoreDataContext context)
            {
                _context = context;
            }
            
            // Cargar datos relacionados
            public IEnumerable<Order> GetOrdersWithDetails()
            {
                var options = new DataLoadOptions();
                options.LoadWith<Order>(o => o.Customer);
                options.LoadWith<Order>(o => o.OrderItems);
                options.LoadWith<OrderItem>(oi => oi.Product);
                
                _context.LoadOptions = options;
                
                return from o in _context.Orders
                       select o;
            }
            
            // Consulta compilada
            private static readonly Func<StoreDataContext, int, Product> GetProductByIdCompiled =
                CompiledQuery.Compile((StoreDataContext context, int id) =>
                    (from p in context.Products
                     where p.Id == id
                     select p).FirstOrDefault());
            
            public Product GetProductByIdOptimized(int id)
            {
                return GetProductByIdCompiled(_context, id);
            }
            
            // Consulta con proyección optimizada
            public IEnumerable<object> GetProductNamesOptimized()
            {
                return from p in _context.Products
                       select new { p.Id, p.Name };
            }
            
            // Evitar N+1 queries
            public IEnumerable<object> GetCustomerOrderCounts()
            {
                return from c in _context.Customers
                       select new
                       {
                           c.Name,
                           c.Email,
                           OrderCount = c.Orders.Count,
                           TotalSpent = c.Orders.Sum(o => o.TotalAmount)
                       };
            }
        }
    }
    
    // ===== MANEJO DE ERRORES =====
    namespace ErrorHandling
    {
        public class ErrorHandlingExamples
        {
            private readonly StoreDataContext _context;
            
            public ErrorHandlingExamples(StoreDataContext context)
            {
                _context = context;
            }
            
            // Manejo de errores en consultas
            public IEnumerable<Product> GetProductsSafely()
            {
                try
                {
                    return from p in _context.Products
                           where p.IsActive
                           select p;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en consulta: {ex.Message}");
                    return Enumerable.Empty<Product>();
                }
            }
            
            // Manejo de errores en operaciones CRUD
            public bool AddProductSafely(Product product)
            {
                try
                {
                    _context.Products.InsertOnSubmit(product);
                    _context.SubmitChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al agregar producto: {ex.Message}");
                    return false;
                }
            }
            
            // Validación antes de operaciones
            public bool UpdateProductSafely(int id, decimal newPrice)
            {
                if (newPrice <= 0)
                {
                    Console.WriteLine("Precio debe ser mayor que 0");
                    return false;
                }
                
                try
                {
                    var product = (from p in _context.Products
                                  where p.Id == id
                                  select p).FirstOrDefault();
                    
                    if (product == null)
                    {
                        Console.WriteLine("Producto no encontrado");
                        return false;
                    }
                    
                    product.Price = newPrice;
                    _context.SubmitChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al actualizar producto: {ex.Message}");
                    return false;
                }
            }
        }
    }
}

// Uso de LINQ to SQL
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== LINQ to SQL - Clase 6 ===\n");
        
        // Nota: En un entorno real, necesitarías una base de datos SQL Server configurada
        // Este es un ejemplo conceptual de cómo usar LINQ to SQL
        
        // string connectionString = "Data Source=localhost;Initial Catalog=StoreDB;Integrated Security=True";
        // var context = new StoreDataContext(connectionString);
        
        Console.WriteLine("LINQ to SQL requiere una base de datos SQL Server configurada.");
        Console.WriteLine("Los ejemplos mostrados son conceptuales para demostrar la sintaxis.");
        
        // Ejemplos conceptuales de uso
        Console.WriteLine("\n1. Consultas Básicas:");
        Console.WriteLine("- GetAllProducts(): Obtener todos los productos");
        Console.WriteLine("- GetActiveProducts(): Obtener productos activos");
        Console.WriteLine("- GetProductsOrderedByPrice(): Productos ordenados por precio");
        
        Console.WriteLine("\n2. Consultas Avanzadas:");
        Console.WriteLine("- GetProductsWithSuppliers(): Productos con información de proveedores");
        Console.WriteLine("- GetCategoryStats(): Estadísticas por categoría");
        Console.WriteLine("- GetCustomersWithOrders(): Clientes que tienen órdenes");
        
        Console.WriteLine("\n3. Operaciones CRUD:");
        Console.WriteLine("- AddProduct(): Insertar nuevo producto");
        Console.WriteLine("- GetProductById(): Obtener producto por ID");
        Console.WriteLine("- UpdateProduct(): Actualizar producto");
        Console.WriteLine("- DeleteProduct(): Eliminar producto");
        
        Console.WriteLine("\n4. Transacciones:");
        Console.WriteLine("- CreateOrderWithItems(): Crear orden con items en transacción");
        Console.WriteLine("- ProcessOrder(): Procesar orden con validación");
        
        Console.WriteLine("\n5. Optimización:");
        Console.WriteLine("- GetOrdersWithDetails(): Cargar datos relacionados");
        Console.WriteLine("- GetProductByIdOptimized(): Consulta compilada");
        Console.WriteLine("- GetCustomerOrderCounts(): Evitar N+1 queries");
        
        Console.WriteLine("\n✅ LINQ to SQL conceptos comprendidos!");
        Console.WriteLine("Para usar en producción, configura una base de datos SQL Server.");
    }
}
