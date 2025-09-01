# 🚀 Clase 4: LINQ to Objects

## 📋 Información de la Clase

- **Módulo**: Mid Level 4 - LINQ y Expresiones Lambda
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Conocimientos de operadores LINQ básicos y avanzados

## 🎯 Objetivos de Aprendizaje

- Dominar LINQ to Objects para consultar colecciones en memoria
- Implementar consultas complejas con múltiples colecciones
- Optimizar el rendimiento de consultas LINQ
- Crear consultas reutilizables y mantenibles

---

## 📚 Navegación del Módulo 4

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_expresiones_lambda_avanzadas.md) | Expresiones Lambda Avanzadas | |
| [Clase 2](clase_2_operadores_linq_basicos.md) | Operadores LINQ Básicos | |
| [Clase 3](clase_3_operadores_linq_avanzados.md) | Operadores LINQ Avanzados | ← Anterior |
| **Clase 4** | **LINQ to Objects** | ← Estás aquí |
| [Clase 5](clase_5_linq_to_xml.md) | LINQ to XML | Siguiente → |
| [Clase 6](clase_6_linq_to_sql.md) | LINQ to SQL | |
| [Clase 7](clase_7_linq_performance.md) | LINQ y Performance | |
| [Clase 8](clase_8_linq_optimization.md) | Optimización de LINQ | |
| [Clase 9](clase_9_linq_extension_methods.md) | Métodos de Extensión LINQ | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Biblioteca | |

**← [Volver al README del Módulo 4](../midLevel_4/README.md)**

---

## 📚 Contenido Teórico

### 1. LINQ to Objects

LINQ to Objects permite consultar colecciones en memoria usando la misma sintaxis que otras fuentes de datos.

```csharp
// ===== LINQ TO OBJECTS - IMPLEMENTACIÓN COMPLETA =====
namespace LinqToObjects
{
    // ===== MODELOS DE DATOS =====
    namespace Models
    {
        public class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public string Category { get; set; }
            public int Stock { get; set; }
            public DateTime CreatedDate { get; set; }
            public bool IsActive { get; set; }
            public List<string> Tags { get; set; }
            
            public Product(int id, string name, decimal price, string category, int stock)
            {
                Id = id;
                Name = name;
                Price = price;
                Category = category;
                Stock = stock;
                CreatedDate = DateTime.Now;
                IsActive = true;
                Tags = new List<string>();
            }
        }
        
        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public int Age { get; set; }
            public string City { get; set; }
            public decimal TotalSpent { get; set; }
            public DateTime RegistrationDate { get; set; }
            
            public Customer(int id, string name, string email, int age, string city)
            {
                Id = id;
                Name = name;
                Email = email;
                Age = age;
                City = city;
                TotalSpent = 0;
                RegistrationDate = DateTime.Now;
            }
        }
        
        public class Order
        {
            public int Id { get; set; }
            public int CustomerId { get; set; }
            public List<OrderItem> Items { get; set; }
            public DateTime OrderDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string Status { get; set; }
            
            public Order(int id, int customerId)
            {
                Id = id;
                CustomerId = customerId;
                Items = new List<OrderItem>();
                OrderDate = DateTime.Now;
                Status = "Pending";
            }
            
            public void AddItem(OrderItem item)
            {
                Items.Add(item);
                TotalAmount = Items.Sum(i => i.TotalPrice);
            }
        }
        
        public class OrderItem
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice => Quantity * UnitPrice;
            
            public OrderItem(int productId, int quantity, decimal unitPrice)
            {
                ProductId = productId;
                Quantity = quantity;
                UnitPrice = unitPrice;
            }
        }
    }
    
    // ===== CONSULTAS BÁSICAS =====
    namespace BasicQueries
    {
        public class BasicQueryExamples
        {
            // Consulta simple con Where y Select
            public static IEnumerable<string> GetProductNames(IEnumerable<Product> products)
            {
                return products.Where(p => p.IsActive).Select(p => p.Name);
            }
            
            // Consulta con ordenamiento
            public static IEnumerable<Product> GetProductsOrderedByPrice(IEnumerable<Product> products)
            {
                return products.OrderBy(p => p.Price);
            }
            
            // Consulta con agrupación
            public static IEnumerable<object> GetProductsByCategory(IEnumerable<Product> products)
            {
                return products.GroupBy(p => p.Category, (category, group) => new 
                { 
                    Category = category,
                    Count = group.Count(),
                    AveragePrice = group.Average(p => p.Price)
                });
            }
            
            // Consulta con filtrado complejo
            public static IEnumerable<Product> GetExpensiveActiveProducts(IEnumerable<Product> products, decimal minPrice)
            {
                return products.Where(p => p.IsActive && p.Price >= minPrice && p.Stock > 0);
            }
        }
    }
    
    // ===== CONSULTAS COMPLEJAS =====
    namespace ComplexQueries
    {
        public class ComplexQueryExamples
        {
            // Consulta con múltiples colecciones
            public static IEnumerable<object> GetCustomerOrderSummary(IEnumerable<Customer> customers, IEnumerable<Order> orders)
            {
                return customers.GroupJoin(orders,
                    customer => customer.Id,
                    order => order.CustomerId,
                    (customer, orderGroup) => new 
                    { 
                        customer.Name,
                        customer.Email,
                        OrderCount = orderGroup.Count(),
                        TotalSpent = orderGroup.Sum(o => o.TotalAmount),
                        LastOrderDate = orderGroup.Any() ? orderGroup.Max(o => o.OrderDate) : (DateTime?)null
                    });
            }
            
            // Consulta con subconsultas
            public static IEnumerable<object> GetProductOrderDetails(IEnumerable<Product> products, IEnumerable<Order> orders)
            {
                return products.Select(p => new 
                { 
                    p.Name,
                    p.Price,
                    p.Category,
                    OrderCount = orders.Count(o => o.Items.Any(i => i.ProductId == p.Id)),
                    TotalQuantity = orders.SelectMany(o => o.Items)
                        .Where(i => i.ProductId == p.Id)
                        .Sum(i => i.Quantity)
                });
            }
            
            // Consulta con agregación compleja
            public static object GetSalesAnalytics(IEnumerable<Order> orders, IEnumerable<Product> products)
            {
                var orderItems = orders.SelectMany(o => o.Items);
                var productSales = orderItems.GroupBy(i => i.ProductId, (productId, items) => new 
                { 
                    ProductId = productId,
                    TotalQuantity = items.Sum(i => i.Quantity),
                    TotalRevenue = items.Sum(i => i.TotalPrice)
                });
                
                return new 
                { 
                    TotalOrders = orders.Count(),
                    TotalRevenue = orders.Sum(o => o.TotalAmount),
                    AverageOrderValue = orders.Average(o => o.TotalAmount),
                    TopSellingProducts = productSales.OrderByDescending(p => p.TotalQuantity).Take(5)
                };
            }
        }
    }
    
    // ===== OPTIMIZACIÓN DE CONSULTAS =====
    namespace QueryOptimization
    {
        public class OptimizationExamples
        {
            // Consulta optimizada con ToList
            public static List<Product> GetActiveProductsOptimized(IEnumerable<Product> products)
            {
                return products.Where(p => p.IsActive).ToList();
            }
            
            // Consulta con paralelización
            public static IEnumerable<object> GetParallelProductStats(IEnumerable<Product> products)
            {
                return products.AsParallel()
                    .GroupBy(p => p.Category, (category, group) => new 
                    { 
                        Category = category,
                        Count = group.Count(),
                        AveragePrice = group.Average(p => p.Price)
                    });
            }
            
            // Consulta con lazy evaluation
            public static IEnumerable<Product> GetLazyProducts(IEnumerable<Product> products)
            {
                return products.Where(p => p.IsActive).Select(p => p);
            }
            
            // Consulta con eager evaluation
            public static List<Product> GetEagerProducts(IEnumerable<Product> products)
            {
                return products.Where(p => p.IsActive).ToList();
            }
        }
    }
    
    // ===== CONSULTAS REUTILIZABLES =====
    namespace ReusableQueries
    {
        public class QueryBuilder
        {
            private readonly IEnumerable<Product> _products;
            
            public QueryBuilder(IEnumerable<Product> products)
            {
                _products = products;
            }
            
            public IQueryable<Product> ActiveProducts => _products.AsQueryable().Where(p => p.IsActive);
            
            public IQueryable<Product> ProductsInStock => _products.AsQueryable().Where(p => p.Stock > 0);
            
            public IQueryable<Product> ExpensiveProducts(decimal minPrice) => 
                _products.AsQueryable().Where(p => p.Price >= minPrice);
            
            public IQueryable<Product> ProductsByCategory(string category) => 
                _products.AsQueryable().Where(p => p.Category == category);
            
            public IQueryable<Product> ProductsByPriceRange(decimal minPrice, decimal maxPrice) => 
                _products.AsQueryable().Where(p => p.Price >= minPrice && p.Price <= maxPrice);
        }
        
        public class ProductQueries
        {
            public static Func<IEnumerable<Product>, IEnumerable<Product>> GetActiveProducts = 
                products => products.Where(p => p.IsActive);
            
            public static Func<IEnumerable<Product>, decimal, IEnumerable<Product>> GetExpensiveProducts = 
                (products, minPrice) => products.Where(p => p.Price >= minPrice);
            
            public static Func<IEnumerable<Product>, string, IEnumerable<Product>> GetProductsByCategory = 
                (products, category) => products.Where(p => p.Category == category);
        }
    }
    
    // ===== CONSULTAS DINÁMICAS =====
    namespace DynamicQueries
    {
        public class DynamicQueryBuilder
        {
            public static IEnumerable<Product> BuildDynamicQuery(IEnumerable<Product> products, 
                string category = null, decimal? minPrice = null, decimal? maxPrice = null, bool? isActive = null)
            {
                var query = products.AsQueryable();
                
                if (!string.IsNullOrEmpty(category))
                    query = query.Where(p => p.Category == category);
                
                if (minPrice.HasValue)
                    query = query.Where(p => p.Price >= minPrice.Value);
                
                if (maxPrice.HasValue)
                    query = query.Where(p => p.Price <= maxPrice.Value);
                
                if (isActive.HasValue)
                    query = query.Where(p => p.IsActive == isActive.Value);
                
                return query.OrderBy(p => p.Name);
            }
            
            public static IEnumerable<Product> BuildOrderedQuery(IEnumerable<Product> products, 
                string sortBy = "Name", bool ascending = true)
            {
                var query = products.AsQueryable();
                
                query = sortBy.ToLower() switch
                {
                    "name" => ascending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                    "price" => ascending ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
                    "category" => ascending ? query.OrderBy(p => p.Category) : query.OrderByDescending(p => p.Category),
                    "stock" => ascending ? query.OrderBy(p => p.Stock) : query.OrderByDescending(p => p.Stock),
                    _ => query.OrderBy(p => p.Name)
                };
                
                return query;
            }
        }
    }
    
    // ===== CONSULTAS CON AGRUPACIÓN AVANZADA =====
    namespace AdvancedGrouping
    {
        public class AdvancedGroupingExamples
        {
            // Agrupación con múltiples claves
            public static IEnumerable<object> GroupProductsByCategoryAndPriceRange(IEnumerable<Product> products)
            {
                return products.GroupBy(p => new 
                { 
                    p.Category,
                    PriceRange = p.Price switch
                    {
                        < 50 => "Low",
                        < 100 => "Medium",
                        < 200 => "High",
                        _ => "Premium"
                    }
                }, (key, group) => new 
                { 
                    key.Category,
                    key.PriceRange,
                    Count = group.Count(),
                    AveragePrice = group.Average(p => p.Price)
                });
            }
            
            // Agrupación con ordenamiento
            public static IEnumerable<object> GetTopCategories(IEnumerable<Product> products, int topCount)
            {
                return products.GroupBy(p => p.Category, (category, group) => new 
                { 
                    Category = category,
                    ProductCount = group.Count(),
                    TotalValue = group.Sum(p => p.Price * p.Stock)
                })
                .OrderByDescending(g => g.TotalValue)
                .Take(topCount);
            }
            
            // Agrupación con filtrado
            public static IEnumerable<object> GetActiveCategoryStats(IEnumerable<Product> products)
            {
                return products.Where(p => p.IsActive)
                    .GroupBy(p => p.Category, (category, group) => new 
                    { 
                        Category = category,
                        ActiveProductCount = group.Count(),
                        TotalStock = group.Sum(p => p.Stock),
                        AveragePrice = group.Average(p => p.Price)
                    });
            }
        }
    }
    
    // ===== CONSULTAS CON PAGINACIÓN =====
    namespace PaginationQueries
    {
        public class PaginationExamples
        {
            // Paginación básica
            public static IEnumerable<Product> GetProductsPage(IEnumerable<Product> products, int pageNumber, int pageSize)
            {
                return products.OrderBy(p => p.Name)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);
            }
            
            // Paginación con información
            public static object GetProductsPageWithInfo(IEnumerable<Product> products, int pageNumber, int pageSize)
            {
                var totalCount = products.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                var items = products.OrderBy(p => p.Name)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);
                
                return new 
                { 
                    Items = items,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasNextPage = pageNumber < totalPages,
                    HasPreviousPage = pageNumber > 1
                };
            }
            
            // Paginación con filtrado
            public static IEnumerable<Product> GetFilteredProductsPage(IEnumerable<Product> products, 
                int pageNumber, int pageSize, string category = null, decimal? minPrice = null)
            {
                var query = products.AsQueryable();
                
                if (!string.IsNullOrEmpty(category))
                    query = query.Where(p => p.Category == category);
                
                if (minPrice.HasValue)
                    query = query.Where(p => p.Price >= minPrice.Value);
                
                return query.OrderBy(p => p.Name)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);
            }
        }
    }
}

// Uso de LINQ to Objects
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== LINQ to Objects - Clase 4 ===\n");
        
        // Crear datos de ejemplo
        var products = new List<Product>
        {
            new Product(1, "Laptop", 1200.00m, "Electronics", 15),
            new Product(2, "Mouse", 25.50m, "Electronics", 50),
            new Product(3, "Keyboard", 75.00m, "Electronics", 30),
            new Product(4, "Book", 15.99m, "Books", 100),
            new Product(5, "Pen", 2.50m, "Office", 200),
            new Product(6, "Desk", 350.00m, "Furniture", 8),
            new Product(7, "Chair", 150.00m, "Furniture", 12),
            new Product(8, "Monitor", 300.00m, "Electronics", 20),
            new Product(9, "Headphones", 89.99m, "Electronics", 25),
            new Product(10, "Notebook", 8.99m, "Office", 150)
        };
        
        var customers = new List<Customer>
        {
            new Customer(1, "John Doe", "john@example.com", 30, "New York"),
            new Customer(2, "Jane Smith", "jane@example.com", 25, "Los Angeles"),
            new Customer(3, "Bob Johnson", "bob@example.com", 35, "Chicago")
        };
        
        var orders = new List<Order>
        {
            new Order(1, 1),
            new Order(2, 2),
            new Order(3, 1)
        };
        
        // Agregar items a las órdenes
        orders[0].AddItem(new OrderItem(1, 1, 1200.00m));
        orders[0].AddItem(new OrderItem(2, 2, 25.50m));
        orders[1].AddItem(new OrderItem(3, 1, 75.00m));
        orders[2].AddItem(new OrderItem(4, 3, 15.99m));
        
        // Ejemplos de consultas básicas
        Console.WriteLine("1. Consultas Básicas:");
        var productNames = BasicQueries.BasicQueryExamples.GetProductNames(products);
        Console.WriteLine($"Nombres de productos activos: {string.Join(", ", productNames)}");
        
        // Ejemplos de consultas complejas
        Console.WriteLine("\n2. Consultas Complejas:");
        var customerSummary = ComplexQueries.ComplexQueryExamples.GetCustomerOrderSummary(customers, orders);
        foreach (var summary in customerSummary)
        {
            Console.WriteLine($"Cliente: {summary.Name}, Órdenes: {summary.OrderCount}, Total: {summary.TotalSpent:C}");
        }
        
        // Ejemplos de optimización
        Console.WriteLine("\n3. Optimización:");
        var optimizedProducts = QueryOptimization.OptimizationExamples.GetActiveProductsOptimized(products);
        Console.WriteLine($"Productos activos optimizados: {optimizedProducts.Count}");
        
        // Ejemplos de consultas reutilizables
        Console.WriteLine("\n4. Consultas Reutilizables:");
        var queryBuilder = new ReusableQueries.QueryBuilder(products);
        var expensiveProducts = queryBuilder.ExpensiveProducts(100).ToList();
        Console.WriteLine($"Productos caros: {expensiveProducts.Count}");
        
        // Ejemplos de consultas dinámicas
        Console.WriteLine("\n5. Consultas Dinámicas:");
        var dynamicProducts = DynamicQueries.DynamicQueryBuilder.BuildDynamicQuery(products, "Electronics", 50);
        Console.WriteLine($"Productos dinámicos: {dynamicProducts.Count()}");
        
        // Ejemplos de agrupación avanzada
        Console.WriteLine("\n6. Agrupación Avanzada:");
        var categoryStats = AdvancedGrouping.AdvancedGroupingExamples.GetActiveCategoryStats(products);
        foreach (var stat in categoryStats)
        {
            Console.WriteLine($"Categoría: {stat.Category}, Productos: {stat.ActiveProductCount}, Stock: {stat.TotalStock}");
        }
        
        // Ejemplos de paginación
        Console.WriteLine("\n7. Paginación:");
        var pageInfo = PaginationQueries.PaginationExamples.GetProductsPageWithInfo(products, 1, 5);
        Console.WriteLine($"Página 1: {pageInfo.Items.Count()} productos de {pageInfo.TotalCount} total");
        
        Console.WriteLine("\n✅ LINQ to Objects funcionando correctamente!");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Consultas Básicas
Crea consultas LINQ to Objects para filtrar, ordenar y agrupar productos.

### Ejercicio 2: Consultas Complejas
Implementa consultas que involucren múltiples colecciones y subconsultas.

### Ejercicio 3: Optimización
Optimiza consultas usando técnicas de evaluación diferida y paralelización.

## 🔍 Puntos Clave

1. **LINQ to Objects** para consultar colecciones en memoria
2. **Consultas básicas** con filtrado, ordenamiento y agrupación
3. **Consultas complejas** con múltiples colecciones y subconsultas
4. **Optimización** con evaluación diferida y paralelización
5. **Consultas reutilizables** usando QueryBuilder y funciones
6. **Consultas dinámicas** que se adaptan a parámetros variables
7. **Agrupación avanzada** con múltiples claves y agregaciones
8. **Paginación** con información de navegación

## 📚 Recursos Adicionales

- [Microsoft Docs - LINQ to Objects](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/linq-to-objects)
- [LINQ Performance](https://docs.microsoft.com/en-us/dotnet/standard/linq/performance)

---

**🎯 ¡Has completado la Clase 4! Ahora comprendes LINQ to Objects**

**📚 [Siguiente: Clase 5 - LINQ to XML](clase_5_linq_to_xml.md)**
