# 🚀 Clase 8: Optimización de LINQ

## 📋 Información de la Clase

- **Módulo**: Mid Level 4 - LINQ y Expresiones Lambda
- **Duración**: 2 horas
- **Nivel**: Avanzado
- **Prerrequisitos**: Conocimientos sólidos de LINQ y performance

## 🎯 Objetivos de Aprendizaje

- Implementar técnicas avanzadas de optimización de LINQ
- Crear consultas compiladas y reutilizables
- Optimizar consultas de base de datos con LINQ
- Usar patrones de optimización específicos

---

## 📚 Navegación del Módulo 4

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_expresiones_lambda_avanzadas.md) | Expresiones Lambda Avanzadas | |
| [Clase 2](clase_2_operadores_linq_basicos.md) | Operadores LINQ Básicos | |
| [Clase 3](clase_3_operadores_linq_avanzados.md) | Operadores LINQ Avanzados | |
| [Clase 4](clase_4_linq_to_objects.md) | LINQ to Objects | |
| [Clase 5](clase_5_linq_to_xml.md) | LINQ to XML | |
| [Clase 6](clase_6_linq_to_sql.md) | LINQ to SQL | |
| [Clase 7](clase_7_linq_performance.md) | LINQ y Performance | ← Anterior |
| **Clase 8** | **Optimización de LINQ** | ← Estás aquí |
| [Clase 9](clase_9_linq_extension_methods.md) | Métodos de Extensión LINQ | Siguiente → |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Biblioteca | |

**← [Volver al README del Módulo 4](../midLevel_4/README.md)**

---

## 📚 Contenido Teórico

### 1. Optimización de LINQ

Las técnicas avanzadas de optimización permiten crear consultas LINQ más eficientes y escalables.

```csharp
// ===== OPTIMIZACIÓN DE LINQ - IMPLEMENTACIÓN COMPLETA =====
using System.Diagnostics;
using System.Data.Linq;

namespace LinqOptimization
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
            public int? SupplierId { get; set; }
            
            public Product(int id, string name, decimal price, string category, int stock)
            {
                Id = id;
                Name = name;
                Price = price;
                Category = category;
                Stock = stock;
                CreatedDate = DateTime.Now;
                IsActive = true;
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
    
    // ===== CONSULTAS COMPILADAS =====
    namespace CompiledQueries
    {
        public class CompiledQueryExamples
        {
            // Consulta compilada básica
            private static readonly Func<IEnumerable<Product>, IEnumerable<Product>> GetActiveProductsCompiled =
                CompiledQuery.Compile((IEnumerable<Product> products) =>
                    products.Where(p => p.IsActive));
            
            public static IEnumerable<Product> GetActiveProducts(IEnumerable<Product> products)
            {
                return GetActiveProductsCompiled(products);
            }
            
            // Consulta compilada con parámetros
            private static readonly Func<IEnumerable<Product>, decimal, IEnumerable<Product>> GetExpensiveProductsCompiled =
                CompiledQuery.Compile((IEnumerable<Product> products, decimal minPrice) =>
                    products.Where(p => p.Price >= minPrice));
            
            public static IEnumerable<Product> GetExpensiveProducts(IEnumerable<Product> products, decimal minPrice)
            {
                return GetExpensiveProductsCompiled(products, minPrice);
            }
            
            // Consulta compilada con múltiples parámetros
            private static readonly Func<IEnumerable<Product>, string, decimal, bool, IEnumerable<Product>> GetFilteredProductsCompiled =
                CompiledQuery.Compile((IEnumerable<Product> products, string category, decimal maxPrice, bool activeOnly) =>
                    products.Where(p => (string.IsNullOrEmpty(category) || p.Category == category) &&
                                       p.Price <= maxPrice &&
                                       (!activeOnly || p.IsActive)));
            
            public static IEnumerable<Product> GetFilteredProducts(IEnumerable<Product> products, string category, decimal maxPrice, bool activeOnly)
            {
                return GetFilteredProductsCompiled(products, category, maxPrice, activeOnly);
            }
            
            // Consulta compilada con proyección
            private static readonly Func<IEnumerable<Product>, IEnumerable<object>> GetProductSummaryCompiled =
                CompiledQuery.Compile((IEnumerable<Product> products) =>
                    products.Select(p => new { p.Name, p.Price, p.Category, IsExpensive = p.Price > 100 }));
            
            public static IEnumerable<object> GetProductSummary(IEnumerable<Product> products)
            {
                return GetProductSummaryCompiled(products);
            }
        }
    }
    
    // ===== OPTIMIZACIÓN DE CONSULTAS DE BASE DE DATOS =====
    namespace DatabaseOptimization
    {
        public class DatabaseQueryOptimization
        {
            // Cargar datos relacionados de una vez
            public static void LoadRelatedData(IEnumerable<Order> orders, IEnumerable<Customer> customers, IEnumerable<Product> products)
            {
                var customerIds = orders.Select(o => o.CustomerId).Distinct();
                var productIds = orders.SelectMany(o => o.Items).Select(i => i.ProductId).Distinct();
                
                var customersDict = customers.Where(c => customerIds.Contains(c.Id)).ToDictionary(c => c.Id);
                var productsDict = products.Where(p => productIds.Contains(p.Id)).ToDictionary(p => p.Id);
                
                // Ahora podemos acceder a los datos relacionados sin consultas adicionales
                foreach (var order in orders)
                {
                    var customer = customersDict.GetValueOrDefault(order.CustomerId);
                    var orderProducts = order.Items.Select(i => productsDict.GetValueOrDefault(i.ProductId));
                    
                    Console.WriteLine($"Order {order.Id}: Customer {customer?.Name}, Products: {string.Join(", ", orderProducts.Select(p => p?.Name))}");
                }
            }
            
            // Paginación eficiente
            public static IEnumerable<Product> GetProductsPageOptimized(IEnumerable<Product> products, int pageNumber, int pageSize, string sortBy = "Name")
            {
                var query = sortBy.ToLower() switch
                {
                    "name" => products.OrderBy(p => p.Name),
                    "price" => products.OrderBy(p => p.Price),
                    "category" => products.OrderBy(p => p.Category),
                    _ => products.OrderBy(p => p.Name)
                };
                
                return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }
            
            // Consulta con proyección optimizada
            public static IEnumerable<object> GetCustomerOrderSummaryOptimized(IEnumerable<Customer> customers, IEnumerable<Order> orders)
            {
                var ordersByCustomer = orders.GroupBy(o => o.CustomerId)
                    .ToDictionary(g => g.Key, g => g.ToList());
                
                return customers.Select(c => new
                {
                    c.Name,
                    c.Email,
                    OrderCount = ordersByCustomer.GetValueOrDefault(c.Id, new List<Order>()).Count,
                    TotalSpent = ordersByCustomer.GetValueOrDefault(c.Id, new List<Order>()).Sum(o => o.TotalAmount)
                });
            }
            
            // Consulta con filtrado temprano
            public static IEnumerable<object> GetActiveProductStatsOptimized(IEnumerable<Product> products)
            {
                return products
                    .Where(p => p.IsActive) // Filtrado temprano
                    .GroupBy(p => p.Category)
                    .Select(g => new
                    {
                        Category = g.Key,
                        ProductCount = g.Count(),
                        AveragePrice = g.Average(p => p.Price),
                        TotalStock = g.Sum(p => p.Stock)
                    });
            }
        }
    }
    
    // ===== PATRONES DE OPTIMIZACIÓN =====
    namespace OptimizationPatterns
    {
        public class OptimizationPatterns
        {
            // Patrón de cache con expiración
            public class QueryCache<TKey, TResult>
            {
                private readonly Dictionary<TKey, (TResult Result, DateTime Expiry)> _cache = new();
                private readonly TimeSpan _defaultExpiry;
                
                public QueryCache(TimeSpan defaultExpiry = default)
                {
                    _defaultExpiry = defaultExpiry == default ? TimeSpan.FromMinutes(5) : defaultExpiry;
                }
                
                public TResult GetOrAdd(TKey key, Func<TKey, TResult> factory, TimeSpan? expiry = null)
                {
                    if (_cache.TryGetValue(key, out var cached) && DateTime.UtcNow < cached.Expiry)
                    {
                        return cached.Result;
                    }
                    
                    var result = factory(key);
                    var expiration = DateTime.UtcNow.Add(expiry ?? _defaultExpiry);
                    _cache[key] = (result, expiration);
                    
                    return result;
                }
                
                public void Clear()
                {
                    _cache.Clear();
                }
                
                public void RemoveExpired()
                {
                    var expiredKeys = _cache.Where(kvp => DateTime.UtcNow >= kvp.Value.Expiry)
                        .Select(kvp => kvp.Key)
                        .ToList();
                    
                    foreach (var key in expiredKeys)
                    {
                        _cache.Remove(key);
                    }
                }
            }
            
            // Patrón de consulta builder
            public class QueryBuilder<T>
            {
                private readonly IEnumerable<T> _source;
                private IQueryable<T> _query;
                
                public QueryBuilder(IEnumerable<T> source)
                {
                    _source = source;
                    _query = source.AsQueryable();
                }
                
                public QueryBuilder<T> Where(Func<T, bool> predicate)
                {
                    _query = _query.Where(predicate);
                    return this;
                }
                
                public QueryBuilder<T> OrderBy<TKey>(Func<T, TKey> keySelector)
                {
                    _query = _query.OrderBy(keySelector);
                    return this;
                }
                
                public QueryBuilder<T> OrderByDescending<TKey>(Func<T, TKey> keySelector)
                {
                    _query = _query.OrderByDescending(keySelector);
                    return this;
                }
                
                public QueryBuilder<T> Skip(int count)
                {
                    _query = _query.Skip(count);
                    return this;
                }
                
                public QueryBuilder<T> Take(int count)
                {
                    _query = _query.Take(count);
                    return this;
                }
                
                public IEnumerable<T> ToList()
                {
                    return _query.ToList();
                }
                
                public T FirstOrDefault()
                {
                    return _query.FirstOrDefault();
                }
                
                public int Count()
                {
                    return _query.Count();
                }
            }
            
            // Patrón de consulta con especificaciones
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
            
            // Especificaciones concretas
            public class ActiveProductSpecification : Specification<Product>
            {
                public override bool IsSatisfiedBy(Product entity)
                {
                    return entity.IsActive;
                }
            }
            
            public class ExpensiveProductSpecification : Specification<Product>
            {
                private readonly decimal _minPrice;
                
                public ExpensiveProductSpecification(decimal minPrice)
                {
                    _minPrice = minPrice;
                }
                
                public override bool IsSatisfiedBy(Product entity)
                {
                    return entity.Price >= _minPrice;
                }
            }
            
            public class CategoryProductSpecification : Specification<Product>
            {
                private readonly string _category;
                
                public CategoryProductSpecification(string category)
                {
                    _category = category;
                }
                
                public override bool IsSatisfiedBy(Product entity)
                {
                    return entity.Category == _category;
                }
            }
        }
    }
    
    // ===== OPTIMIZACIÓN DE MEMORIA =====
    namespace MemoryOptimization
    {
        public class MemoryOptimizationExamples
        {
            // Uso de streaming para grandes colecciones
            public static IEnumerable<Product> StreamProducts(IEnumerable<Product> products, decimal maxPrice)
            {
                foreach (var product in products)
                {
                    if (product.Price <= maxPrice)
                    {
                        yield return product;
                    }
                }
            }
            
            // Uso de structs para reducir allocations
            public struct ProductInfo
            {
                public int Id;
                public string Name;
                public decimal Price;
                public string Category;
                
                public ProductInfo(Product product)
                {
                    Id = product.Id;
                    Name = product.Name;
                    Price = product.Price;
                    Category = product.Category;
                }
            }
            
            public static IEnumerable<ProductInfo> GetProductInfoStructs(IEnumerable<Product> products)
            {
                return products.Select(p => new ProductInfo(p));
            }
            
            // Uso de ArrayPool para reducir allocations
            public static void ProcessWithArrayPool(IEnumerable<int> numbers)
            {
                var array = ArrayPool<int>.Shared.Rent(numbers.Count());
                
                try
                {
                    var index = 0;
                    foreach (var number in numbers)
                    {
                        array[index++] = number;
                    }
                    
                    // Procesar array
                    for (int i = 0; i < index; i++)
                    {
                        // Procesar array[i]
                    }
                }
                finally
                {
                    ArrayPool<int>.Shared.Return(array);
                }
            }
            
            // Uso de Memory<T> para operaciones eficientes
            public static void ProcessWithMemory(int[] numbers)
            {
                var memory = new Memory<int>(numbers);
                var span = memory.Span;
                
                for (int i = 0; i < span.Length; i++)
                {
                    span[i] *= 2; // Operación in-place
                }
            }
        }
    }
    
    // ===== OPTIMIZACIÓN DE ALGORITMOS =====
    namespace AlgorithmOptimization
    {
        public class AlgorithmOptimizationExamples
        {
            // Búsqueda binaria optimizada
            public static Product BinarySearchProduct(IEnumerable<Product> products, int targetId)
            {
                var sortedProducts = products.OrderBy(p => p.Id).ToList();
                var left = 0;
                var right = sortedProducts.Count - 1;
                
                while (left <= right)
                {
                    var mid = left + (right - left) / 2;
                    var product = sortedProducts[mid];
                    
                    if (product.Id == targetId)
                        return product;
                    
                    if (product.Id < targetId)
                        left = mid + 1;
                    else
                        right = mid - 1;
                }
                
                return null;
            }
            
            // Algoritmo de particionamiento
            public static (IEnumerable<Product> Left, IEnumerable<Product> Right) PartitionProducts(IEnumerable<Product> products, decimal pivotPrice)
            {
                var left = new List<Product>();
                var right = new List<Product>();
                
                foreach (var product in products)
                {
                    if (product.Price <= pivotPrice)
                        left.Add(product);
                    else
                        right.Add(product);
                }
                
                return (left, right);
            }
            
            // Algoritmo de ordenamiento personalizado
            public static IEnumerable<Product> CustomSortProducts(IEnumerable<Product> products, string sortBy)
            {
                return sortBy.ToLower() switch
                {
                    "name" => products.OrderBy(p => p.Name),
                    "price" => products.OrderBy(p => p.Price),
                    "category" => products.OrderBy(p => p.Category).ThenBy(p => p.Name),
                    "stock" => products.OrderByDescending(p => p.Stock),
                    _ => products.OrderBy(p => p.Id)
                };
            }
            
            // Algoritmo de agrupación optimizada
            public static IEnumerable<IGrouping<string, Product>> OptimizedGroupByCategory(IEnumerable<Product> products)
            {
                var groups = new Dictionary<string, List<Product>>();
                
                foreach (var product in products)
                {
                    if (!groups.ContainsKey(product.Category))
                    {
                        groups[product.Category] = new List<Product>();
                    }
                    
                    groups[product.Category].Add(product);
                }
                
                return groups.Select(g => new Grouping<string, Product>(g.Key, g.Value));
            }
            
            // Clase auxiliar para agrupación
            public class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
            {
                private readonly List<TElement> _elements;
                
                public Grouping(TKey key, List<TElement> elements)
                {
                    Key = key;
                    _elements = elements;
                }
                
                public TKey Key { get; }
                
                public IEnumerator<TElement> GetEnumerator()
                {
                    return _elements.GetEnumerator();
                }
                
                System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }
            }
        }
    }
    
    // ===== OPTIMIZACIÓN DE CONSULTAS COMPLEJAS =====
    namespace ComplexQueryOptimization
    {
        public class ComplexQueryOptimizationExamples
        {
            // Consulta con múltiples joins optimizada
            public static IEnumerable<object> GetOrderDetailsOptimized(IEnumerable<Order> orders, IEnumerable<Customer> customers, IEnumerable<Product> products)
            {
                var customersDict = customers.ToDictionary(c => c.Id);
                var productsDict = products.ToDictionary(p => p.Id);
                
                return orders.Select(order => new
                {
                    OrderId = order.Id,
                    CustomerName = customersDict.GetValueOrDefault(order.CustomerId)?.Name,
                    CustomerEmail = customersDict.GetValueOrDefault(order.CustomerId)?.Email,
                    Items = order.Items.Select(item => new
                    {
                        ProductName = productsDict.GetValueOrDefault(item.ProductId)?.Name,
                        item.Quantity,
                        item.UnitPrice,
                        item.TotalPrice
                    }),
                    order.TotalAmount,
                    order.Status
                });
            }
            
            // Consulta con agregación optimizada
            public static object GetSalesAnalyticsOptimized(IEnumerable<Order> orders, IEnumerable<Product> products)
            {
                var orderItems = orders.SelectMany(o => o.Items).ToList();
                var productSales = orderItems.GroupBy(i => i.ProductId)
                    .ToDictionary(g => g.Key, g => new
                    {
                        TotalQuantity = g.Sum(i => i.Quantity),
                        TotalRevenue = g.Sum(i => i.TotalPrice)
                    });
                
                var productsDict = products.ToDictionary(p => p.Id);
                
                return new
                {
                    TotalOrders = orders.Count(),
                    TotalRevenue = orders.Sum(o => o.TotalAmount),
                    AverageOrderValue = orders.Average(o => o.TotalAmount),
                    TopSellingProducts = productSales
                        .OrderByDescending(p => p.Value.TotalQuantity)
                        .Take(5)
                        .Select(p => new
                        {
                            ProductName = productsDict.GetValueOrDefault(p.Key)?.Name,
                            p.Value.TotalQuantity,
                            p.Value.TotalRevenue
                        })
                };
            }
            
            // Consulta con filtrado dinámico optimizado
            public static IEnumerable<Product> GetProductsWithDynamicFilterOptimized(IEnumerable<Product> products, 
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
            
            // Consulta con paginación y ordenamiento optimizado
            public static object GetProductsPageOptimized(IEnumerable<Product> products, int pageNumber, int pageSize, string sortBy = "Name", bool ascending = true)
            {
                var totalCount = products.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                
                var query = sortBy.ToLower() switch
                {
                    "name" => ascending ? products.OrderBy(p => p.Name) : products.OrderByDescending(p => p.Name),
                    "price" => ascending ? products.OrderBy(p => p.Price) : products.OrderByDescending(p => p.Price),
                    "category" => ascending ? products.OrderBy(p => p.Category) : products.OrderByDescending(p => p.Category),
                    "stock" => ascending ? products.OrderBy(p => p.Stock) : products.OrderByDescending(p => p.Stock),
                    _ => products.OrderBy(p => p.Name)
                };
                
                var items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                
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
        }
    }
}

// Uso de Optimización de LINQ
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Optimización de LINQ - Clase 8 ===\n");
        
        // Crear datos de ejemplo
        var products = Enumerable.Range(1, 10000)
            .Select(i => new Product(i, $"Product {i}", i * 10.0m, $"Category {i % 5}", i * 2))
            .ToList();
        
        var customers = Enumerable.Range(1, 1000)
            .Select(i => new Customer(i, $"Customer {i}", $"customer{i}@example.com", 20 + (i % 50), $"City {i % 10}"))
            .ToList();
        
        var orders = Enumerable.Range(1, 5000)
            .Select(i => new Order(i, (i % 1000) + 1))
            .ToList();
        
        // Ejemplos de consultas compiladas
        Console.WriteLine("1. Consultas Compiladas:");
        var activeProducts = CompiledQueries.CompiledQueryExamples.GetActiveProducts(products);
        Console.WriteLine($"Productos activos: {activeProducts.Count()}");
        
        // Ejemplos de optimización de base de datos
        Console.WriteLine("\n2. Optimización de Base de Datos:");
        DatabaseOptimization.DatabaseQueryOptimization.LoadRelatedData(orders.Take(10), customers, products);
        
        // Ejemplos de patrones de optimización
        Console.WriteLine("\n3. Patrones de Optimización:");
        var cache = new OptimizationPatterns.OptimizationPatterns.QueryCache<string, IEnumerable<Product>>();
        var cachedProducts = cache.GetOrAdd("expensive", key => products.Where(p => p.Price > 100).ToList());
        Console.WriteLine($"Productos caros en cache: {cachedProducts.Count()}");
        
        // Ejemplos de especificaciones
        var activeSpec = new OptimizationPatterns.OptimizationPatterns.ActiveProductSpecification();
        var expensiveSpec = new OptimizationPatterns.OptimizationPatterns.ExpensiveProductSpecification(100);
        var combinedSpec = activeSpec.And(expensiveSpec);
        
        var filteredProducts = products.Where(p => combinedSpec.IsSatisfiedBy(p)).ToList();
        Console.WriteLine($"Productos activos y caros: {filteredProducts.Count}");
        
        // Ejemplos de optimización de memoria
        Console.WriteLine("\n4. Optimización de Memoria:");
        var productInfos = MemoryOptimization.MemoryOptimizationExamples.GetProductInfoStructs(products.Take(1000));
        Console.WriteLine($"Structs de productos: {productInfos.Count()}");
        
        // Ejemplos de optimización de algoritmos
        Console.WriteLine("\n5. Optimización de Algoritmos:");
        var foundProduct = AlgorithmOptimization.AlgorithmOptimizationExamples.BinarySearchProduct(products, 5000);
        Console.WriteLine($"Producto encontrado: {foundProduct?.Name}");
        
        // Ejemplos de consultas complejas optimizadas
        Console.WriteLine("\n6. Consultas Complejas Optimizadas:");
        var orderDetails = ComplexQueryOptimization.ComplexQueryOptimizationExamples.GetOrderDetailsOptimized(orders.Take(10), customers, products);
        Console.WriteLine($"Detalles de órdenes: {orderDetails.Count()}");
        
        Console.WriteLine("\n✅ Optimización de LINQ comprendida!");
        Console.WriteLine("Recuerda: La optimización debe ser medida y justificada.");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Consultas Compiladas
Crea consultas compiladas para operaciones frecuentes y mide su rendimiento.

### Ejercicio 2: Patrones de Optimización
Implementa patrones como QueryBuilder y Specification para consultas complejas.

### Ejercicio 3: Optimización de Algoritmos
Crea algoritmos optimizados para búsqueda y ordenamiento de datos.

## 🔍 Puntos Clave

1. **Consultas compiladas** para mejorar el rendimiento de operaciones repetitivas
2. **Optimización de base de datos** con carga eficiente de datos relacionados
3. **Patrones de optimización** como cache, QueryBuilder y Specification
4. **Optimización de memoria** con structs, ArrayPool y Memory<T>
5. **Optimización de algoritmos** con búsqueda binaria y particionamiento
6. **Consultas complejas optimizadas** con joins y agregaciones eficientes
7. **Paginación y ordenamiento** optimizados para grandes conjuntos de datos
8. **Medición y justificación** de optimizaciones

## 📚 Recursos Adicionales

- [Microsoft Docs - LINQ Optimization](https://docs.microsoft.com/en-us/dotnet/standard/linq/performance)
- [Compiled Queries](https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/linq/how-to-store-and-reuse-queries)

---

**🎯 ¡Has completado la Clase 8! Ahora comprendes la Optimización de LINQ**

**📚 [Siguiente: Clase 9 - Métodos de Extensión LINQ](clase_9_linq_extension_methods.md)**
