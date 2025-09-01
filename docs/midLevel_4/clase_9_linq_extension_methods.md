# 🚀 Clase 9: Métodos de Extensión LINQ

## 📋 Información de la Clase

- **Módulo**: Mid Level 4 - LINQ y Expresiones Lambda
- **Duración**: 2 horas
- **Nivel**: Avanzado
- **Prerrequisitos**: Conocimientos sólidos de LINQ y métodos de extensión

## 🎯 Objetivos de Aprendizaje

- Crear métodos de extensión personalizados para LINQ
- Extender funcionalidades de colecciones con LINQ
- Implementar operadores LINQ personalizados
- Usar métodos de extensión en escenarios reales

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
| [Clase 7](clase_7_linq_performance.md) | LINQ y Performance | |
| [Clase 8](clase_8_linq_optimization.md) | Optimización de LINQ | ← Anterior |
| **Clase 9** | **Métodos de Extensión LINQ** | ← Estás aquí |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Biblioteca | Siguiente → |

**← [Volver al README del Módulo 4](../midLevel_4/README.md)**

---

## 📚 Contenido Teórico

### 1. Métodos de Extensión LINQ

Los métodos de extensión permiten agregar funcionalidades a tipos existentes sin modificar su código fuente.

```csharp
// ===== MÉTODOS DE EXTENSIÓN LINQ - IMPLEMENTACIÓN COMPLETA =====
using System.Collections.Generic;
using System.Linq;

namespace LinqExtensionMethods
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
            public bool IsPremium { get; set; }
            
            public Customer(int id, string name, string email, int age, string city)
            {
                Id = id;
                Name = name;
                Email = email;
                Age = age;
                City = city;
                TotalSpent = 0;
                RegistrationDate = DateTime.Now;
                IsPremium = false;
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
            public string Notes { get; set; }
            
            public Order(int id, int customerId)
            {
                Id = id;
                CustomerId = customerId;
                Items = new List<OrderItem>();
                OrderDate = DateTime.Now;
                Status = "Pending";
                Notes = "";
            }
        }
        
        public class OrderItem
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice => Quantity * UnitPrice;
            public decimal Discount { get; set; }
            
            public OrderItem(int productId, int quantity, decimal unitPrice)
            {
                ProductId = productId;
                Quantity = quantity;
                UnitPrice = unitPrice;
                Discount = 0;
            }
        }
    }
    
    // ===== MÉTODOS DE EXTENSIÓN BÁSICOS =====
    namespace BasicExtensions
    {
        public static class EnumerableExtensions
        {
            // Método de extensión para filtrar productos activos
            public static IEnumerable<Product> WhereActive(this IEnumerable<Product> products)
            {
                return products.Where(p => p.IsActive);
            }
            
            // Método de extensión para obtener productos caros
            public static IEnumerable<Product> WhereExpensive(this IEnumerable<Product> products, decimal minPrice = 100)
            {
                return products.Where(p => p.Price >= minPrice);
            }
            
            // Método de extensión para obtener productos por categoría
            public static IEnumerable<Product> WhereCategory(this IEnumerable<Product> products, string category)
            {
                return products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            }
            
            // Método de extensión para obtener productos con stock bajo
            public static IEnumerable<Product> WhereLowStock(this IEnumerable<Product> products, int threshold = 10)
            {
                return products.Where(p => p.Stock <= threshold);
            }
            
            // Método de extensión para obtener productos recientes
            public static IEnumerable<Product> WhereRecent(this IEnumerable<Product> products, int days = 30)
            {
                var cutoffDate = DateTime.Now.AddDays(-days);
                return products.Where(p => p.CreatedDate >= cutoffDate);
            }
            
            // Método de extensión para obtener productos con descuento
            public static IEnumerable<Product> WhereDiscounted(this IEnumerable<Product> products, decimal discountPercentage = 10)
            {
                return products.Where(p => p.Price > 0 && p.Stock > 0);
            }
            
            // Método de extensión para obtener productos con tags específicos
            public static IEnumerable<Product> WhereHasTag(this IEnumerable<Product> products, string tag)
            {
                return products.Where(p => p.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase));
            }
            
            // Método de extensión para obtener productos ordenados por popularidad
            public static IEnumerable<Product> OrderByPopularity(this IEnumerable<Product> products)
            {
                return products.OrderByDescending(p => p.Stock).ThenBy(p => p.Price);
            }
            
            // Método de extensión para obtener productos ordenados por valor
            public static IEnumerable<Product> OrderByValue(this IEnumerable<Product> products)
            {
                return products.OrderByDescending(p => p.Price * p.Stock);
            }
            
            // Método de extensión para obtener productos ordenados por antigüedad
            public static IEnumerable<Product> OrderByAge(this IEnumerable<Product> products)
            {
                return products.OrderBy(p => p.CreatedDate);
            }
        }
        
        public static class CustomerExtensions
        {
            // Método de extensión para filtrar clientes premium
            public static IEnumerable<Customer> WherePremium(this IEnumerable<Customer> customers)
            {
                return customers.Where(c => c.IsPremium);
            }
            
            // Método de extensión para filtrar clientes por ciudad
            public static IEnumerable<Customer> WhereCity(this IEnumerable<Customer> customers, string city)
            {
                return customers.Where(c => c.City.Equals(city, StringComparison.OrdinalIgnoreCase));
            }
            
            // Método de extensión para filtrar clientes por rango de edad
            public static IEnumerable<Customer> WhereAgeRange(this IEnumerable<Customer> customers, int minAge, int maxAge)
            {
                return customers.Where(c => c.Age >= minAge && c.Age <= maxAge);
            }
            
            // Método de extensión para filtrar clientes con alto gasto
            public static IEnumerable<Customer> WhereHighSpender(this IEnumerable<Customer> customers, decimal threshold = 1000)
            {
                return customers.Where(c => c.TotalSpent >= threshold);
            }
            
            // Método de extensión para filtrar clientes recientes
            public static IEnumerable<Customer> WhereRecent(this IEnumerable<Customer> customers, int days = 90)
            {
                var cutoffDate = DateTime.Now.AddDays(-days);
                return customers.Where(c => c.RegistrationDate >= cutoffDate);
            }
            
            // Método de extensión para ordenar clientes por valor
            public static IEnumerable<Customer> OrderByValue(this IEnumerable<Customer> customers)
            {
                return customers.OrderByDescending(c => c.TotalSpent);
            }
            
            // Método de extensión para ordenar clientes por antigüedad
            public static IEnumerable<Customer> OrderBySeniority(this IEnumerable<Customer> customers)
            {
                return customers.OrderBy(c => c.RegistrationDate);
            }
        }
    }
    
    // ===== MÉTODOS DE EXTENSIÓN AVANZADOS =====
    namespace AdvancedExtensions
    {
        public static class AdvancedEnumerableExtensions
        {
            // Método de extensión para paginación
            public static IEnumerable<T> Page<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
            {
                return source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }
            
            // Método de extensión para paginación con información
            public static PagedResult<T> ToPagedResult<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
            {
                var totalCount = source.Count();
                var items = source.Page(pageNumber, pageSize).ToList();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                
                return new PagedResult<T>
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
            
            // Método de extensión para chunking
            public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunkSize)
            {
                var chunk = new List<T>();
                foreach (var item in source)
                {
                    chunk.Add(item);
                    if (chunk.Count == chunkSize)
                    {
                        yield return chunk;
                        chunk = new List<T>();
                    }
                }
                
                if (chunk.Count > 0)
                {
                    yield return chunk;
                }
            }
            
            // Método de extensión para batch processing
            public static void ProcessInBatches<T>(this IEnumerable<T> source, int batchSize, Action<IEnumerable<T>> processor)
            {
                foreach (var batch in source.Chunk(batchSize))
                {
                    processor(batch);
                }
            }
            
            // Método de extensión para async batch processing
            public static async Task ProcessInBatchesAsync<T>(this IEnumerable<T> source, int batchSize, Func<IEnumerable<T>, Task> processor)
            {
                foreach (var batch in source.Chunk(batchSize))
                {
                    await processor(batch);
                }
            }
            
            // Método de extensión para distinct by
            public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
            {
                var seenKeys = new HashSet<TKey>();
                foreach (var item in source)
                {
                    if (seenKeys.Add(keySelector(item)))
                    {
                        yield return item;
                    }
                }
            }
            
            // Método de extensión para except by
            public static IEnumerable<T> ExceptBy<T, TKey>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, TKey> keySelector)
            {
                var secondKeys = new HashSet<TKey>(second.Select(keySelector));
                return first.Where(item => !secondKeys.Contains(keySelector(item)));
            }
            
            // Método de extensión para intersect by
            public static IEnumerable<T> IntersectBy<T, TKey>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, TKey> keySelector)
            {
                var secondKeys = new HashSet<TKey>(second.Select(keySelector));
                return first.Where(item => secondKeys.Contains(keySelector(item)));
            }
            
            // Método de extensión para union by
            public static IEnumerable<T> UnionBy<T, TKey>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, TKey> keySelector)
            {
                var seenKeys = new HashSet<TKey>();
                foreach (var item in first.Concat(second))
                {
                    if (seenKeys.Add(keySelector(item)))
                    {
                        yield return item;
                    }
                }
            }
            
            // Método de extensión para group by multiple keys
            public static IEnumerable<IGrouping<TKey, T>> GroupByMultiple<T, TKey>(this IEnumerable<T> source, params Func<T, object>[] keySelectors)
            {
                return source.GroupBy(item => new CompositeKey(keySelectors.Select(selector => selector(item))));
            }
            
            // Método de extensión para flatten
            public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
            {
                return source.SelectMany(x => x);
            }
            
            // Método de extensión para zip with index
            public static IEnumerable<(T Item, int Index)> WithIndex<T>(this IEnumerable<T> source)
            {
                return source.Select((item, index) => (item, index));
            }
            
            // Método de extensión para zip with previous
            public static IEnumerable<(T Current, T Previous)> WithPrevious<T>(this IEnumerable<T> source)
            {
                T previous = default;
                bool first = true;
                
                foreach (var item in source)
                {
                    if (!first)
                    {
                        yield return (item, previous);
                    }
                    previous = item;
                    first = false;
                }
            }
            
            // Método de extensión para zip with next
            public static IEnumerable<(T Current, T Next)> WithNext<T>(this IEnumerable<T> source)
            {
                var enumerator = source.GetEnumerator();
                if (!enumerator.MoveNext())
                    yield break;
                
                var current = enumerator.Current;
                while (enumerator.MoveNext())
                {
                    yield return (current, enumerator.Current);
                    current = enumerator.Current;
                }
            }
        }
        
        // Clases auxiliares
        public class PagedResult<T>
        {
            public IEnumerable<T> Items { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public int TotalCount { get; set; }
            public int TotalPages { get; set; }
            public bool HasNextPage { get; set; }
            public bool HasPreviousPage { get; set; }
        }
        
        public class CompositeKey : IEquatable<CompositeKey>
        {
            private readonly object[] _keys;
            
            public CompositeKey(IEnumerable<object> keys)
            {
                _keys = keys.ToArray();
            }
            
            public bool Equals(CompositeKey other)
            {
                if (other == null || _keys.Length != other._keys.Length)
                    return false;
                
                for (int i = 0; i < _keys.Length; i++)
                {
                    if (!Equals(_keys[i], other._keys[i]))
                        return false;
                }
                
                return true;
            }
            
            public override bool Equals(object obj)
            {
                return Equals(obj as CompositeKey);
            }
            
            public override int GetHashCode()
            {
                var hash = 17;
                foreach (var key in _keys)
                {
                    hash = hash * 31 + (key?.GetHashCode() ?? 0);
                }
                return hash;
            }
        }
    }
    
    // ===== MÉTODOS DE EXTENSIÓN ESPECIALIZADOS =====
    namespace SpecializedExtensions
    {
        public static class ProductExtensions
        {
            // Método de extensión para calcular el valor total del inventario
            public static decimal CalculateInventoryValue(this IEnumerable<Product> products)
            {
                return products.Sum(p => p.Price * p.Stock);
            }
            
            // Método de extensión para obtener estadísticas de productos
            public static ProductStats GetStats(this IEnumerable<Product> products)
            {
                var activeProducts = products.WhereActive().ToList();
                
                return new ProductStats
                {
                    TotalProducts = products.Count(),
                    ActiveProducts = activeProducts.Count,
                    TotalInventoryValue = activeProducts.CalculateInventoryValue(),
                    AveragePrice = activeProducts.Any() ? activeProducts.Average(p => p.Price) : 0,
                    MinPrice = activeProducts.Any() ? activeProducts.Min(p => p.Price) : 0,
                    MaxPrice = activeProducts.Any() ? activeProducts.Max(p => p.Price) : 0,
                    LowStockProducts = activeProducts.WhereLowStock().Count(),
                    Categories = activeProducts.Select(p => p.Category).Distinct().Count()
                };
            }
            
            // Método de extensión para obtener productos recomendados
            public static IEnumerable<Product> GetRecommended(this IEnumerable<Product> products, Customer customer, int count = 5)
            {
                var customerPreferences = GetCustomerPreferences(customer);
                
                return products
                    .WhereActive()
                    .Where(p => p.Stock > 0)
                    .OrderByDescending(p => GetRecommendationScore(p, customerPreferences))
                    .Take(count);
            }
            
            // Método de extensión para obtener productos similares
            public static IEnumerable<Product> GetSimilar(this IEnumerable<Product> products, Product targetProduct, int count = 5)
            {
                return products
                    .WhereActive()
                    .Where(p => p.Id != targetProduct.Id && p.Category == targetProduct.Category)
                    .OrderBy(p => Math.Abs(p.Price - targetProduct.Price))
                    .Take(count);
            }
            
            // Método de extensión para aplicar descuentos
            public static IEnumerable<Product> ApplyDiscount(this IEnumerable<Product> products, decimal discountPercentage)
            {
                return products.Select(p => new Product(p.Id, p.Name, p.Price * (1 - discountPercentage / 100), p.Category, p.Stock)
                {
                    CreatedDate = p.CreatedDate,
                    IsActive = p.IsActive,
                    Tags = new List<string>(p.Tags)
                });
            }
            
            // Método de extensión para obtener productos por rango de precios
            public static IEnumerable<Product> WherePriceRange(this IEnumerable<Product> products, decimal minPrice, decimal maxPrice)
            {
                return products.Where(p => p.Price >= minPrice && p.Price <= maxPrice);
            }
            
            // Método de extensión para obtener productos por múltiples categorías
            public static IEnumerable<Product> WhereCategories(this IEnumerable<Product> products, params string[] categories)
            {
                return products.Where(p => categories.Contains(p.Category, StringComparer.OrdinalIgnoreCase));
            }
            
            // Método de extensión para obtener productos con múltiples tags
            public static IEnumerable<Product> WhereHasAnyTag(this IEnumerable<Product> products, params string[] tags)
            {
                return products.Where(p => p.Tags.Any(tag => tags.Contains(tag, StringComparer.OrdinalIgnoreCase)));
            }
            
            // Método de extensión para obtener productos con todos los tags
            public static IEnumerable<Product> WhereHasAllTags(this IEnumerable<Product> products, params string[] tags)
            {
                return products.Where(p => tags.All(tag => p.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)));
            }
            
            // Métodos auxiliares privados
            private static Dictionary<string, decimal> GetCustomerPreferences(Customer customer)
            {
                // Simulación de preferencias basadas en el perfil del cliente
                return new Dictionary<string, decimal>
                {
                    ["premium"] = customer.IsPremium ? 1.5m : 1.0m,
                    ["age_group"] = customer.Age > 50 ? 1.2m : 1.0m,
                    ["high_spender"] = customer.TotalSpent > 1000 ? 1.3m : 1.0m
                };
            }
            
            private static decimal GetRecommendationScore(Product product, Dictionary<string, decimal> preferences)
            {
                var score = 1.0m;
                
                if (product.Price > 100 && preferences.ContainsKey("premium"))
                    score *= preferences["premium"];
                
                if (product.Category == "Electronics" && preferences.ContainsKey("age_group"))
                    score *= preferences["age_group"];
                
                return score;
            }
        }
        
        public static class CustomerExtensions
        {
            // Método de extensión para calcular el valor del cliente
            public static decimal CalculateCustomerValue(this Customer customer, IEnumerable<Order> orders)
            {
                var customerOrders = orders.Where(o => o.CustomerId == customer.Id);
                return customerOrders.Sum(o => o.TotalAmount);
            }
            
            // Método de extensión para obtener el historial de compras
            public static IEnumerable<Order> GetPurchaseHistory(this Customer customer, IEnumerable<Order> orders)
            {
                return orders.Where(o => o.CustomerId == customer.Id).OrderByDescending(o => o.OrderDate);
            }
            
            // Método de extensión para obtener productos favoritos
            public static IEnumerable<Product> GetFavoriteProducts(this Customer customer, IEnumerable<Order> orders, IEnumerable<Product> products)
            {
                var productIds = orders
                    .Where(o => o.CustomerId == customer.Id)
                    .SelectMany(o => o.Items)
                    .GroupBy(i => i.ProductId)
                    .OrderByDescending(g => g.Sum(i => i.Quantity))
                    .Select(g => g.Key)
                    .Take(5);
                
                return products.Where(p => productIds.Contains(p.Id));
            }
            
            // Método de extensión para obtener categorías preferidas
            public static IEnumerable<string> GetPreferredCategories(this Customer customer, IEnumerable<Order> orders, IEnumerable<Product> products)
            {
                var productIds = orders
                    .Where(o => o.CustomerId == customer.Id)
                    .SelectMany(o => o.Items)
                    .Select(i => i.ProductId);
                
                return products
                    .Where(p => productIds.Contains(p.Id))
                    .GroupBy(p => p.Category)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key);
            }
            
            // Método de extensión para calcular el tiempo desde la última compra
            public static TimeSpan TimeSinceLastPurchase(this Customer customer, IEnumerable<Order> orders)
            {
                var lastOrder = orders
                    .Where(o => o.CustomerId == customer.Id)
                    .OrderByDescending(o => o.OrderDate)
                    .FirstOrDefault();
                
                return lastOrder != null ? DateTime.Now - lastOrder.OrderDate : TimeSpan.MaxValue;
            }
            
            // Método de extensión para determinar si es un cliente activo
            public static bool IsActiveCustomer(this Customer customer, IEnumerable<Order> orders, int daysThreshold = 90)
            {
                return customer.TimeSinceLastPurchase(orders).TotalDays <= daysThreshold;
            }
        }
        
        public static class OrderExtensions
        {
            // Método de extensión para calcular el descuento total
            public static decimal CalculateTotalDiscount(this Order order)
            {
                return order.Items.Sum(i => i.Discount);
            }
            
            // Método de extensión para calcular el total con descuentos
            public static decimal CalculateTotalWithDiscounts(this Order order)
            {
                return order.Items.Sum(i => i.TotalPrice - i.Discount);
            }
            
            // Método de extensión para obtener productos únicos
            public static IEnumerable<int> GetUniqueProductIds(this Order order)
            {
                return order.Items.Select(i => i.ProductId).Distinct();
            }
            
            // Método de extensión para obtener productos con mayor cantidad
            public static IEnumerable<OrderItem> GetTopItems(this Order order, int count = 3)
            {
                return order.Items.OrderByDescending(i => i.Quantity).Take(count);
            }
            
            // Método de extensión para obtener productos con mayor valor
            public static IEnumerable<OrderItem> GetTopValueItems(this Order order, int count = 3)
            {
                return order.Items.OrderByDescending(i => i.TotalPrice).Take(count);
            }
            
            // Método de extensión para verificar si es una orden grande
            public static bool IsLargeOrder(this Order order, decimal threshold = 500)
            {
                return order.TotalAmount >= threshold;
            }
            
            // Método de extensión para verificar si es una orden con muchos items
            public static bool IsMultiItemOrder(this Order order, int threshold = 3)
            {
                return order.Items.Count >= threshold;
            }
        }
        
        // Clases de datos
        public class ProductStats
        {
            public int TotalProducts { get; set; }
            public int ActiveProducts { get; set; }
            public decimal TotalInventoryValue { get; set; }
            public decimal AveragePrice { get; set; }
            public decimal MinPrice { get; set; }
            public decimal MaxPrice { get; set; }
            public int LowStockProducts { get; set; }
            public int Categories { get; set; }
        }
    }
    
    // ===== MÉTODOS DE EXTENSIÓN PARA TRANSFORMACIONES =====
    namespace TransformationExtensions
    {
        public static class TransformationExtensions
        {
            // Método de extensión para transformar productos a DTOs
            public static IEnumerable<ProductDto> ToProductDtos(this IEnumerable<Product> products)
            {
                return products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Category = p.Category,
                    Stock = p.Stock,
                    IsActive = p.IsActive,
                    Tags = p.Tags.ToArray()
                });
            }
            
            // Método de extensión para transformar clientes a DTOs
            public static IEnumerable<CustomerDto> ToCustomerDtos(this IEnumerable<Customer> customers)
            {
                return customers.Select(c => new CustomerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    Age = c.Age,
                    City = c.City,
                    TotalSpent = c.TotalSpent,
                    IsPremium = c.IsPremium,
                    DaysSinceRegistration = (DateTime.Now - c.RegistrationDate).Days
                });
            }
            
            // Método de extensión para transformar órdenes a DTOs
            public static IEnumerable<OrderDto> ToOrderDtos(this IEnumerable<Order> orders)
            {
                return orders.Select(o => new OrderDto
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    OrderDate = o.OrderDate,
                    ItemCount = o.Items.Count,
                    HasNotes = !string.IsNullOrEmpty(o.Notes)
                });
            }
            
            // Método de extensión para crear resúmenes de productos
            public static IEnumerable<ProductSummary> ToProductSummaries(this IEnumerable<Product> products)
            {
                return products.Select(p => new ProductSummary
                {
                    Name = p.Name,
                    Category = p.Category,
                    PriceRange = GetPriceRange(p.Price),
                    StockStatus = GetStockStatus(p.Stock),
                    IsActive = p.IsActive
                });
            }
            
            // Método de extensión para crear resúmenes de clientes
            public static IEnumerable<CustomerSummary> ToCustomerSummaries(this IEnumerable<Customer> customers)
            {
                return customers.Select(c => new CustomerSummary
                {
                    Name = c.Name,
                    AgeGroup = GetAgeGroup(c.Age),
                    SpendingLevel = GetSpendingLevel(c.TotalSpent),
                    IsPremium = c.IsPremium,
                    City = c.City
                });
            }
            
            // Métodos auxiliares privados
            private static string GetPriceRange(decimal price)
            {
                return price switch
                {
                    < 50 => "Low",
                    < 200 => "Medium",
                    < 500 => "High",
                    _ => "Premium"
                };
            }
            
            private static string GetStockStatus(int stock)
            {
                return stock switch
                {
                    0 => "Out of Stock",
                    <= 5 => "Low Stock",
                    <= 20 => "Medium Stock",
                    _ => "In Stock"
                };
            }
            
            private static string GetAgeGroup(int age)
            {
                return age switch
                {
                    < 25 => "Young",
                    < 40 => "Adult",
                    < 60 => "Middle-aged",
                    _ => "Senior"
                };
            }
            
            private static string GetSpendingLevel(decimal totalSpent)
            {
                return totalSpent switch
                {
                    < 100 => "Low",
                    < 500 => "Medium",
                    < 1000 => "High",
                    _ => "Premium"
                };
            }
        }
        
        // DTOs y clases de resumen
        public class ProductDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public string Category { get; set; }
            public int Stock { get; set; }
            public bool IsActive { get; set; }
            public string[] Tags { get; set; }
        }
        
        public class CustomerDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public int Age { get; set; }
            public string City { get; set; }
            public decimal TotalSpent { get; set; }
            public bool IsPremium { get; set; }
            public int DaysSinceRegistration { get; set; }
        }
        
        public class OrderDto
        {
            public int Id { get; set; }
            public int CustomerId { get; set; }
            public decimal TotalAmount { get; set; }
            public string Status { get; set; }
            public DateTime OrderDate { get; set; }
            public int ItemCount { get; set; }
            public bool HasNotes { get; set; }
        }
        
        public class ProductSummary
        {
            public string Name { get; set; }
            public string Category { get; set; }
            public string PriceRange { get; set; }
            public string StockStatus { get; set; }
            public bool IsActive { get; set; }
        }
        
        public class CustomerSummary
        {
            public string Name { get; set; }
            public string AgeGroup { get; set; }
            public string SpendingLevel { get; set; }
            public bool IsPremium { get; set; }
            public string City { get; set; }
        }
    }
}

// Uso de Métodos de Extensión LINQ
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Métodos de Extensión LINQ - Clase 9 ===\n");
        
        // Crear datos de ejemplo
        var products = Enumerable.Range(1, 100)
            .Select(i => new Product(i, $"Product {i}", i * 10.0m, $"Category {i % 5}", i * 2))
            .ToList();
        
        var customers = Enumerable.Range(1, 50)
            .Select(i => new Customer(i, $"Customer {i}", $"customer{i}@example.com", 20 + (i % 50), $"City {i % 10}"))
            .ToList();
        
        var orders = Enumerable.Range(1, 200)
            .Select(i => new Order(i, (i % 50) + 1))
            .ToList();
        
        // Ejemplos de métodos de extensión básicos
        Console.WriteLine("1. Métodos de Extensión Básicos:");
        var activeProducts = products.WhereActive();
        var expensiveProducts = products.WhereExpensive(200);
        var electronicsProducts = products.WhereCategory("Category 0");
        
        Console.WriteLine($"Productos activos: {activeProducts.Count()}");
        Console.WriteLine($"Productos caros: {expensiveProducts.Count()}");
        Console.WriteLine($"Productos de electrónica: {electronicsProducts.Count()}");
        
        // Ejemplos de métodos de extensión avanzados
        Console.WriteLine("\n2. Métodos de Extensión Avanzados:");
        var pagedProducts = products.ToPagedResult(1, 10);
        var chunkedProducts = products.Chunk(5).ToList();
        
        Console.WriteLine($"Página 1: {pagedProducts.Items.Count()} productos");
        Console.WriteLine($"Chunks: {chunkedProducts.Count} grupos de 5");
        
        // Ejemplos de métodos de extensión especializados
        Console.WriteLine("\n3. Métodos de Extensión Especializados:");
        var productStats = products.GetStats();
        var customerValue = customers.First().CalculateCustomerValue(orders);
        
        Console.WriteLine($"Valor total del inventario: ${productStats.TotalInventoryValue}");
        Console.WriteLine($"Valor del cliente: ${customerValue}");
        
        // Ejemplos de transformaciones
        Console.WriteLine("\n4. Transformaciones:");
        var productDtos = products.ToProductDtos();
        var customerDtos = customers.ToCustomerDtos();
        var productSummaries = products.ToProductSummaries();
        
        Console.WriteLine($"DTOs de productos: {productDtos.Count()}");
        Console.WriteLine($"DTOs de clientes: {customerDtos.Count()}");
        Console.WriteLine($"Resúmenes de productos: {productSummaries.Count()}");
        
        // Ejemplos de procesamiento en lotes
        Console.WriteLine("\n5. Procesamiento en Lotes:");
        var processedCount = 0;
        products.ProcessInBatches(10, batch =>
        {
            processedCount += batch.Count();
            Console.WriteLine($"Procesando lote de {batch.Count()} productos");
        });
        
        Console.WriteLine($"Total procesado: {processedCount} productos");
        
        Console.WriteLine("\n✅ Métodos de Extensión LINQ comprendidos!");
        Console.WriteLine("Recuerda: Los métodos de extensión hacen el código más legible y reutilizable.");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Métodos de Extensión Básicos
Crea métodos de extensión para filtrar y ordenar colecciones de manera personalizada.

### Ejercicio 2: Métodos de Extensión Avanzados
Implementa métodos de extensión para paginación, chunking y operaciones complejas.

### Ejercicio 3: Métodos de Extensión Especializados
Crea métodos de extensión específicos para tu dominio de negocio.

## 🔍 Puntos Clave

1. **Métodos de extensión básicos** para filtrar y ordenar colecciones
2. **Métodos de extensión avanzados** para paginación, chunking y operaciones complejas
3. **Métodos de extensión especializados** para dominios específicos
4. **Transformaciones** con métodos de extensión para DTOs y resúmenes
5. **Procesamiento en lotes** para operaciones eficientes
6. **Operaciones personalizadas** como DistinctBy, ExceptBy, IntersectBy
7. **Agrupación múltiple** y operaciones con índices
8. **Reutilización de código** y legibilidad mejorada

## 📚 Recursos Adicionales

- [Microsoft Docs - Extension Methods](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods)
- [LINQ Extension Methods](https://docs.microsoft.com/en-us/dotnet/api/system.linq.enumerable)

---

**🎯 ¡Has completado la Clase 9! Ahora comprendes los Métodos de Extensión LINQ**

**📚 [Siguiente: Clase 10 - Proyecto Final: Sistema de Biblioteca](clase_10_proyecto_final.md)**
