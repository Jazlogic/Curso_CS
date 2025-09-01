# 🚀 Clase 2: Operadores LINQ Básicos

## 📋 Información de la Clase

- **Módulo**: Mid Level 4 - LINQ y Expresiones Lambda
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Conocimientos de expresiones lambda y colecciones en C#

## 🎯 Objetivos de Aprendizaje

- Dominar los operadores LINQ básicos de filtrado y proyección
- Comprender la diferencia entre sintaxis de consulta y método
- Implementar operaciones de ordenamiento y agrupación
- Usar operadores de agregación y particionamiento

---

## 📚 Navegación del Módulo 4

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_expresiones_lambda_avanzadas.md) | Expresiones Lambda Avanzadas | ← Anterior |
| **Clase 2** | **Operadores LINQ Básicos** | ← Estás aquí |
| [Clase 3](clase_3_operadores_linq_avanzados.md) | Operadores LINQ Avanzados | Siguiente → |
| [Clase 4](clase_4_linq_to_objects.md) | LINQ to Objects | |
| [Clase 5](clase_5_linq_to_xml.md) | LINQ to XML | |
| [Clase 6](clase_6_linq_to_sql.md) | LINQ to SQL | |
| [Clase 7](clase_7_linq_performance.md) | LINQ y Performance | |
| [Clase 8](clase_8_linq_optimization.md) | Optimización de LINQ | |
| [Clase 9](clase_9_linq_extension_methods.md) | Métodos de Extensión LINQ | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Biblioteca | |

**← [Volver al README del Módulo 4](../midLevel_4/README.md)**

---

## 📚 Contenido Teórico

### 1. Operadores LINQ Básicos

LINQ (Language Integrated Query) proporciona una forma unificada de consultar datos de diferentes fuentes. Vamos a explorar los operadores básicos.

```csharp
// ===== OPERADORES LINQ BÁSICOS - IMPLEMENTACIÓN COMPLETA =====
namespace BasicLinqOperators
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
            
            public override string ToString()
            {
                return $"ID: {Id}, Name: {Name}, Price: {Price:C}, Category: {Category}, Stock: {Stock}";
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
            
            public override string ToString()
            {
                return $"ID: {Id}, Name: {Name}, Email: {Email}, Age: {Age}, City: {City}";
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
            
            public override string ToString()
            {
                return $"Order ID: {Id}, Customer: {CustomerId}, Total: {TotalAmount:C}, Status: {Status}";
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
    
    // ===== OPERADORES DE FILTRADO =====
    namespace FilteringOperators
    {
        public class FilteringExamples
        {
            // Where - Filtrado básico
            public static IEnumerable<Product> GetExpensiveProducts(IEnumerable<Product> products, decimal minPrice)
            {
                return products.Where(p => p.Price >= minPrice);
            }
            
            // Where con múltiples condiciones
            public static IEnumerable<Product> GetActiveProductsInStock(IEnumerable<Product> products)
            {
                return products.Where(p => p.IsActive && p.Stock > 0);
            }
            
            // Where con índice
            public static IEnumerable<Product> GetProductsWithIndex(IEnumerable<Product> products, int startIndex)
            {
                return products.Where((product, index) => index >= startIndex);
            }
            
            // Where con validación compleja
            public static IEnumerable<Product> GetValidProducts(IEnumerable<Product> products)
            {
                return products.Where(p => 
                    !string.IsNullOrWhiteSpace(p.Name) &&
                    p.Price > 0 &&
                    p.Stock >= 0 &&
                    !string.IsNullOrWhiteSpace(p.Category));
            }
            
            // Where con fechas
            public static IEnumerable<Product> GetRecentProducts(IEnumerable<Product> products, int daysAgo)
            {
                var cutoffDate = DateTime.Now.AddDays(-daysAgo);
                return products.Where(p => p.CreatedDate >= cutoffDate);
            }
            
            // Where con categorías específicas
            public static IEnumerable<Product> GetProductsByCategories(IEnumerable<Product> products, params string[] categories)
            {
                return products.Where(p => categories.Contains(p.Category));
            }
            
            // Where con rango de precios
            public static IEnumerable<Product> GetProductsInPriceRange(IEnumerable<Product> products, decimal minPrice, decimal maxPrice)
            {
                return products.Where(p => p.Price >= minPrice && p.Price <= maxPrice);
            }
        }
    }
    
    // ===== OPERADORES DE PROYECCIÓN =====
    namespace ProjectionOperators
    {
        public class ProjectionExamples
        {
            // Select - Proyección básica
            public static IEnumerable<string> GetProductNames(IEnumerable<Product> products)
            {
                return products.Select(p => p.Name);
            }
            
            // Select con transformación
            public static IEnumerable<decimal> GetDiscountedPrices(IEnumerable<Product> products, decimal discountPercent)
            {
                return products.Select(p => p.Price * (1 - discountPercent / 100));
            }
            
            // Select con objeto anónimo
            public static IEnumerable<object> GetProductSummary(IEnumerable<Product> products)
            {
                return products.Select(p => new 
                { 
                    p.Name, 
                    p.Price, 
                    p.Category,
                    IsExpensive = p.Price > 100
                });
            }
            
            // Select con índice
            public static IEnumerable<object> GetProductsWithIndex(IEnumerable<Product> products)
            {
                return products.Select((product, index) => new 
                { 
                    Index = index,
                    product.Name,
                    product.Price
                });
            }
            
            // SelectMany - Aplanar colecciones
            public static IEnumerable<OrderItem> GetAllOrderItems(IEnumerable<Order> orders)
            {
                return orders.SelectMany(o => o.Items);
            }
            
            // SelectMany con transformación
            public static IEnumerable<object> GetOrderDetails(IEnumerable<Order> orders)
            {
                return orders.SelectMany(o => o.Items, (order, item) => new 
                { 
                    OrderId = order.Id,
                    CustomerId = order.CustomerId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    TotalPrice = item.TotalPrice
                });
            }
            
            // Select con validación
            public static IEnumerable<string> GetValidProductNames(IEnumerable<Product> products)
            {
                return products
                    .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                    .Select(p => p.Name.Trim());
            }
            
            // Select con cálculo
            public static IEnumerable<object> GetProductStats(IEnumerable<Product> products)
            {
                return products.Select(p => new 
                { 
                    p.Name,
                    p.Price,
                    p.Stock,
                    TotalValue = p.Price * p.Stock,
                    StockStatus = p.Stock > 10 ? "Good" : p.Stock > 0 ? "Low" : "Out"
                });
            }
        }
    }
    
    // ===== OPERADORES DE ORDENAMIENTO =====
    namespace OrderingOperators
    {
        public class OrderingExamples
        {
            // OrderBy - Ordenamiento ascendente
            public static IEnumerable<Product> GetProductsOrderedByName(IEnumerable<Product> products)
            {
                return products.OrderBy(p => p.Name);
            }
            
            // OrderByDescending - Ordenamiento descendente
            public static IEnumerable<Product> GetProductsOrderedByPriceDesc(IEnumerable<Product> products)
            {
                return products.OrderByDescending(p => p.Price);
            }
            
            // ThenBy - Ordenamiento secundario
            public static IEnumerable<Product> GetProductsOrderedByCategoryAndPrice(IEnumerable<Product> products)
            {
                return products.OrderBy(p => p.Category).ThenBy(p => p.Price);
            }
            
            // ThenByDescending - Ordenamiento secundario descendente
            public static IEnumerable<Product> GetProductsOrderedByCategoryAndPriceDesc(IEnumerable<Product> products)
            {
                return products.OrderBy(p => p.Category).ThenByDescending(p => p.Price);
            }
            
            // Ordenamiento con múltiples criterios
            public static IEnumerable<Product> GetProductsOrderedByMultipleCriteria(IEnumerable<Product> products)
            {
                return products
                    .OrderBy(p => p.IsActive)
                    .ThenByDescending(p => p.Stock)
                    .ThenBy(p => p.Name);
            }
            
            // Ordenamiento con comparador personalizado
            public static IEnumerable<string> GetStringsOrderedByLength(IEnumerable<string> strings)
            {
                return strings.OrderBy(s => s.Length);
            }
            
            // Ordenamiento con fechas
            public static IEnumerable<Product> GetProductsOrderedByDate(IEnumerable<Product> products)
            {
                return products.OrderByDescending(p => p.CreatedDate);
            }
            
            // Ordenamiento con validación
            public static IEnumerable<Product> GetValidProductsOrdered(IEnumerable<Product> products)
            {
                return products
                    .Where(p => p.Price > 0 && p.Stock >= 0)
                    .OrderBy(p => p.Category)
                    .ThenBy(p => p.Name);
            }
        }
    }
    
    // ===== OPERADORES DE AGRUPACIÓN =====
    namespace GroupingOperators
    {
        public class GroupingExamples
        {
            // GroupBy - Agrupación básica
            public static IEnumerable<IGrouping<string, Product>> GroupProductsByCategory(IEnumerable<Product> products)
            {
                return products.GroupBy(p => p.Category);
            }
            
            // GroupBy con proyección
            public static IEnumerable<object> GroupProductsByCategoryWithCount(IEnumerable<Product> products)
            {
                return products.GroupBy(p => p.Category, (category, group) => new 
                { 
                    Category = category,
                    Count = group.Count(),
                    TotalValue = group.Sum(p => p.Price * p.Stock)
                });
            }
            
            // GroupBy con múltiples claves
            public static IEnumerable<IGrouping<object, Product>> GroupProductsByCategoryAndActiveStatus(IEnumerable<Product> products)
            {
                return products.GroupBy(p => new { p.Category, p.IsActive });
            }
            
            // GroupBy con agregación
            public static IEnumerable<object> GetCategoryStats(IEnumerable<Product> products)
            {
                return products.GroupBy(p => p.Category, (category, group) => new 
                { 
                    Category = category,
                    ProductCount = group.Count(),
                    AveragePrice = group.Average(p => p.Price),
                    TotalStock = group.Sum(p => p.Stock),
                    MaxPrice = group.Max(p => p.Price),
                    MinPrice = group.Min(p => p.Price)
                });
            }
            
            // GroupBy con filtrado
            public static IEnumerable<object> GetActiveCategoryStats(IEnumerable<Product> products)
            {
                return products
                    .Where(p => p.IsActive)
                    .GroupBy(p => p.Category, (category, group) => new 
                    { 
                        Category = category,
                        ActiveProducts = group.Count(),
                        TotalValue = group.Sum(p => p.Price * p.Stock)
                    });
            }
            
            // GroupBy con ordenamiento
            public static IEnumerable<object> GetCategoriesOrderedByValue(IEnumerable<Product> products)
            {
                return products
                    .GroupBy(p => p.Category, (category, group) => new 
                    { 
                        Category = category,
                        TotalValue = group.Sum(p => p.Price * p.Stock)
                    })
                    .OrderByDescending(g => g.TotalValue);
            }
            
            // GroupBy con múltiples niveles
            public static IEnumerable<object> GetProductsGroupedByCategoryAndPriceRange(IEnumerable<Product> products)
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
        }
    }
    
    // ===== OPERADORES DE AGREGACIÓN =====
    namespace AggregationOperators
    {
        public class AggregationExamples
        {
            // Count - Contar elementos
            public static int GetProductCount(IEnumerable<Product> products)
            {
                return products.Count();
            }
            
            // Count con condición
            public static int GetExpensiveProductCount(IEnumerable<Product> products, decimal minPrice)
            {
                return products.Count(p => p.Price >= minPrice);
            }
            
            // Sum - Sumar valores
            public static decimal GetTotalInventoryValue(IEnumerable<Product> products)
            {
                return products.Sum(p => p.Price * p.Stock);
            }
            
            // Average - Promedio
            public static decimal GetAverageProductPrice(IEnumerable<Product> products)
            {
                return products.Average(p => p.Price);
            }
            
            // Min/Max - Valores mínimo y máximo
            public static object GetPriceRange(IEnumerable<Product> products)
            {
                return new 
                { 
                    MinPrice = products.Min(p => p.Price),
                    MaxPrice = products.Max(p => p.Price)
                };
            }
            
            // Aggregate - Agregación personalizada
            public static string GetProductNamesConcatenated(IEnumerable<Product> products, string separator = ", ")
            {
                return products.Aggregate("", (current, product) => 
                    current + (current == "" ? "" : separator) + product.Name);
            }
            
            // Aggregate con semilla
            public static decimal GetTotalValueWithTax(IEnumerable<Product> products, decimal taxRate)
            {
                return products.Aggregate(0m, (total, product) => 
                    total + (product.Price * product.Stock * (1 + taxRate / 100)));
            }
            
            // Aggregate con múltiples operaciones
            public static object GetInventoryStats(IEnumerable<Product> products)
            {
                return products.Aggregate(new 
                { 
                    TotalProducts = 0,
                    TotalValue = 0m,
                    AveragePrice = 0m,
                    Categories = new HashSet<string>()
                }, (stats, product) => new 
                { 
                    TotalProducts = stats.TotalProducts + 1,
                    TotalValue = stats.TotalValue + (product.Price * product.Stock),
                    AveragePrice = (stats.AveragePrice * stats.TotalProducts + product.Price) / (stats.TotalProducts + 1),
                    Categories = new HashSet<string>(stats.Categories) { product.Category }
                });
            }
            
            // Aggregate con validación
            public static object GetValidProductStats(IEnumerable<Product> products)
            {
                return products
                    .Where(p => p.Price > 0 && p.Stock >= 0)
                    .Aggregate(new 
                    { 
                        ValidCount = 0,
                        TotalValue = 0m,
                        Categories = new HashSet<string>()
                    }, (stats, product) => new 
                    { 
                        ValidCount = stats.ValidCount + 1,
                        TotalValue = stats.TotalValue + (product.Price * product.Stock),
                        Categories = new HashSet<string>(stats.Categories) { product.Category }
                    });
            }
        }
    }
    
    // ===== OPERADORES DE PARTICIONAMIENTO =====
    namespace PartitioningOperators
    {
        public class PartitioningExamples
        {
            // Take - Tomar elementos
            public static IEnumerable<Product> GetTopProducts(IEnumerable<Product> products, int count)
            {
                return products.OrderByDescending(p => p.Price).Take(count);
            }
            
            // Skip - Saltar elementos
            public static IEnumerable<Product> GetProductsAfterFirst(IEnumerable<Product> products, int skipCount)
            {
                return products.Skip(skipCount);
            }
            
            // TakeWhile - Tomar mientras se cumpla condición
            public static IEnumerable<Product> GetProductsUntilExpensive(IEnumerable<Product> products, decimal maxPrice)
            {
                return products.TakeWhile(p => p.Price <= maxPrice);
            }
            
            // SkipWhile - Saltar mientras se cumpla condición
            public static IEnumerable<Product> GetProductsAfterCheap(IEnumerable<Product> products, decimal minPrice)
            {
                return products.SkipWhile(p => p.Price < minPrice);
            }
            
            // Paginación
            public static IEnumerable<Product> GetProductsPage(IEnumerable<Product> products, int pageNumber, int pageSize)
            {
                return products
                    .OrderBy(p => p.Name)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);
            }
            
            // Particionamiento con ordenamiento
            public static IEnumerable<Product> GetTopExpensiveProducts(IEnumerable<Product> products, int count)
            {
                return products
                    .Where(p => p.Price > 100)
                    .OrderByDescending(p => p.Price)
                    .Take(count);
            }
            
            // Particionamiento con validación
            public static IEnumerable<Product> GetFirstValidProducts(IEnumerable<Product> products, int count)
            {
                return products
                    .Where(p => p.Price > 0 && p.Stock > 0)
                    .Take(count);
            }
            
            // Particionamiento con agrupación
            public static IEnumerable<object> GetTopCategories(IEnumerable<Product> products, int count)
            {
                return products
                    .GroupBy(p => p.Category, (category, group) => new 
                    { 
                        Category = category,
                        TotalValue = group.Sum(p => p.Price * p.Stock)
                    })
                    .OrderByDescending(g => g.TotalValue)
                    .Take(count);
            }
        }
    }
    
    // ===== OPERADORES DE CONJUNTO =====
    namespace SetOperators
    {
        public class SetExamples
        {
            // Distinct - Elementos únicos
            public static IEnumerable<string> GetUniqueCategories(IEnumerable<Product> products)
            {
                return products.Select(p => p.Category).Distinct();
            }
            
            // Union - Unión de conjuntos
            public static IEnumerable<string> GetAllCategories(IEnumerable<Product> products1, IEnumerable<Product> products2)
            {
                return products1.Select(p => p.Category)
                    .Union(products2.Select(p => p.Category));
            }
            
            // Intersect - Intersección de conjuntos
            public static IEnumerable<string> GetCommonCategories(IEnumerable<Product> products1, IEnumerable<Product> products2)
            {
                return products1.Select(p => p.Category)
                    .Intersect(products2.Select(p => p.Category));
            }
            
            // Except - Diferencia de conjuntos
            public static IEnumerable<string> GetUniqueCategoriesInFirst(IEnumerable<Product> products1, IEnumerable<Product> products2)
            {
                return products1.Select(p => p.Category)
                    .Except(products2.Select(p => p.Category));
            }
            
            // Concat - Concatenación
            public static IEnumerable<Product> GetAllProducts(IEnumerable<Product> products1, IEnumerable<Product> products2)
            {
                return products1.Concat(products2);
            }
            
            // Distinct con comparador personalizado
            public static IEnumerable<Product> GetUniqueProductsByName(IEnumerable<Product> products)
            {
                return products.Distinct(new ProductNameComparer());
            }
        }
        
        public class ProductNameComparer : IEqualityComparer<Product>
        {
            public bool Equals(Product x, Product y)
            {
                if (x == null && y == null) return true;
                if (x == null || y == null) return false;
                return x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase);
            }
            
            public int GetHashCode(Product obj)
            {
                return obj.Name?.ToLower().GetHashCode() ?? 0;
            }
        }
    }
    
    // ===== OPERADORES DE ELEMENTO =====
    namespace ElementOperators
    {
        public class ElementExamples
        {
            // First - Primer elemento
            public static Product GetFirstProduct(IEnumerable<Product> products)
            {
                return products.First();
            }
            
            // FirstOrDefault - Primer elemento o valor por defecto
            public static Product GetFirstExpensiveProduct(IEnumerable<Product> products, decimal minPrice)
            {
                return products.FirstOrDefault(p => p.Price >= minPrice);
            }
            
            // Last - Último elemento
            public static Product GetLastProduct(IEnumerable<Product> products)
            {
                return products.Last();
            }
            
            // LastOrDefault - Último elemento o valor por defecto
            public static Product GetLastCheapProduct(IEnumerable<Product> products, decimal maxPrice)
            {
                return products.LastOrDefault(p => p.Price <= maxPrice);
            }
            
            // Single - Elemento único
            public static Product GetProductById(IEnumerable<Product> products, int id)
            {
                return products.Single(p => p.Id == id);
            }
            
            // SingleOrDefault - Elemento único o valor por defecto
            public static Product GetProductByName(IEnumerable<Product> products, string name)
            {
                return products.SingleOrDefault(p => p.Name == name);
            }
            
            // ElementAt - Elemento en posición específica
            public static Product GetProductAtPosition(IEnumerable<Product> products, int position)
            {
                return products.ElementAt(position);
            }
            
            // ElementAtOrDefault - Elemento en posición o valor por defecto
            public static Product GetProductAtPositionSafe(IEnumerable<Product> products, int position)
            {
                return products.ElementAtOrDefault(position);
            }
            
            // DefaultIfEmpty - Valor por defecto si la secuencia está vacía
            public static IEnumerable<Product> GetProductsOrDefault(IEnumerable<Product> products)
            {
                return products.DefaultIfEmpty(new Product(0, "No products", 0, "None", 0));
            }
        }
    }
    
    // ===== SINTAXIS DE CONSULTA VS MÉTODO =====
    namespace QuerySyntaxVsMethodSyntax
    {
        public class SyntaxComparison
        {
            // Filtrado - Sintaxis de método
            public static IEnumerable<Product> GetExpensiveProductsMethod(IEnumerable<Product> products, decimal minPrice)
            {
                return products.Where(p => p.Price >= minPrice);
            }
            
            // Filtrado - Sintaxis de consulta
            public static IEnumerable<Product> GetExpensiveProductsQuery(IEnumerable<Product> products, decimal minPrice)
            {
                return from p in products
                       where p.Price >= minPrice
                       select p;
            }
            
            // Proyección - Sintaxis de método
            public static IEnumerable<object> GetProductSummaryMethod(IEnumerable<Product> products)
            {
                return products.Select(p => new 
                { 
                    p.Name, 
                    p.Price, 
                    p.Category,
                    IsExpensive = p.Price > 100
                });
            }
            
            // Proyección - Sintaxis de consulta
            public static IEnumerable<object> GetProductSummaryQuery(IEnumerable<Product> products)
            {
                return from p in products
                       select new 
                       { 
                           p.Name, 
                           p.Price, 
                           p.Category,
                           IsExpensive = p.Price > 100
                       };
            }
            
            // Ordenamiento - Sintaxis de método
            public static IEnumerable<Product> GetProductsOrderedMethod(IEnumerable<Product> products)
            {
                return products.OrderBy(p => p.Category).ThenByDescending(p => p.Price);
            }
            
            // Ordenamiento - Sintaxis de consulta
            public static IEnumerable<Product> GetProductsOrderedQuery(IEnumerable<Product> products)
            {
                return from p in products
                       orderby p.Category, p.Price descending
                       select p;
            }
            
            // Agrupación - Sintaxis de método
            public static IEnumerable<object> GetCategoryStatsMethod(IEnumerable<Product> products)
            {
                return products.GroupBy(p => p.Category, (category, group) => new 
                { 
                    Category = category,
                    Count = group.Count(),
                    AveragePrice = group.Average(p => p.Price)
                });
            }
            
            // Agrupación - Sintaxis de consulta
            public static IEnumerable<object> GetCategoryStatsQuery(IEnumerable<Product> products)
            {
                return from p in products
                       group p by p.Category into g
                       select new 
                       { 
                           Category = g.Key,
                           Count = g.Count(),
                           AveragePrice = g.Average(p => p.Price)
                       };
            }
            
            // Consulta compleja - Sintaxis de método
            public static IEnumerable<object> GetComplexQueryMethod(IEnumerable<Product> products)
            {
                return products
                    .Where(p => p.IsActive && p.Stock > 0)
                    .OrderBy(p => p.Category)
                    .ThenByDescending(p => p.Price)
                    .Select(p => new 
                    { 
                        p.Name,
                        p.Price,
                        p.Category,
                        StockStatus = p.Stock > 10 ? "Good" : "Low"
                    });
            }
            
            // Consulta compleja - Sintaxis de consulta
            public static IEnumerable<object> GetComplexQueryQuery(IEnumerable<Product> products)
            {
                return from p in products
                       where p.IsActive && p.Stock > 0
                       orderby p.Category, p.Price descending
                       select new 
                       { 
                           p.Name,
                           p.Price,
                           p.Category,
                           StockStatus = p.Stock > 10 ? "Good" : "Low"
                       };
            }
        }
    }
}

// Uso de Operadores LINQ Básicos
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Operadores LINQ Básicos - Clase 2 ===\n");
        
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
        
        // Ejemplos de filtrado
        Console.WriteLine("1. Filtrado:");
        var expensiveProducts = FilteringExamples.GetExpensiveProducts(products, 100);
        Console.WriteLine($"Productos caros (>$100): {expensiveProducts.Count()}");
        
        // Ejemplos de proyección
        Console.WriteLine("\n2. Proyección:");
        var productNames = ProjectionExamples.GetProductNames(products);
        Console.WriteLine("Nombres de productos: " + string.Join(", ", productNames));
        
        // Ejemplos de ordenamiento
        Console.WriteLine("\n3. Ordenamiento:");
        var orderedProducts = OrderingExamples.GetProductsOrderedByPriceDesc(products);
        Console.WriteLine("Productos ordenados por precio (desc):");
        foreach (var product in orderedProducts.Take(3))
        {
            Console.WriteLine($"  {product}");
        }
        
        // Ejemplos de agrupación
        Console.WriteLine("\n4. Agrupación:");
        var categoryStats = GroupingExamples.GetCategoryStats(products);
        foreach (var stat in categoryStats)
        {
            Console.WriteLine($"Categoría: {stat.Category}, Productos: {stat.ProductCount}, Precio Promedio: {stat.AveragePrice:C}");
        }
        
        // Ejemplos de agregación
        Console.WriteLine("\n5. Agregación:");
        var totalValue = AggregationExamples.GetTotalInventoryValue(products);
        Console.WriteLine($"Valor total del inventario: {totalValue:C}");
        
        // Ejemplos de particionamiento
        Console.WriteLine("\n6. Particionamiento:");
        var topProducts = PartitioningExamples.GetTopProducts(products, 3);
        Console.WriteLine("Top 3 productos más caros:");
        foreach (var product in topProducts)
        {
            Console.WriteLine($"  {product}");
        }
        
        // Ejemplos de operadores de conjunto
        Console.WriteLine("\n7. Operadores de Conjunto:");
        var uniqueCategories = SetExamples.GetUniqueCategories(products);
        Console.WriteLine("Categorías únicas: " + string.Join(", ", uniqueCategories));
        
        // Ejemplos de operadores de elemento
        Console.WriteLine("\n8. Operadores de Elemento:");
        var firstProduct = ElementExamples.GetFirstProduct(products);
        Console.WriteLine($"Primer producto: {firstProduct}");
        
        // Comparación de sintaxis
        Console.WriteLine("\n9. Comparación de Sintaxis:");
        var methodResult = QuerySyntaxVsMethodSyntax.GetExpensiveProductsMethod(products, 100);
        var queryResult = QuerySyntaxVsMethodSyntax.GetExpensiveProductsQuery(products, 100);
        Console.WriteLine($"Método: {methodResult.Count()} productos, Consulta: {queryResult.Count()} productos");
        
        Console.WriteLine("\n✅ Operadores LINQ Básicos funcionando correctamente!");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Filtrado y Proyección
Crea consultas LINQ que filtren productos por diferentes criterios y proyecten información específica.

### Ejercicio 2: Ordenamiento y Agrupación
Implementa consultas que ordenen y agrupen datos de diferentes maneras.

### Ejercicio 3: Agregación y Particionamiento
Crea consultas que calculen estadísticas y dividan datos en páginas.

## 🔍 Puntos Clave

1. **Operadores de filtrado** (Where) para seleccionar elementos
2. **Operadores de proyección** (Select, SelectMany) para transformar datos
3. **Operadores de ordenamiento** (OrderBy, ThenBy) para organizar resultados
4. **Operadores de agrupación** (GroupBy) para agrupar elementos relacionados
5. **Operadores de agregación** (Count, Sum, Average, Min, Max, Aggregate)
6. **Operadores de particionamiento** (Take, Skip, TakeWhile, SkipWhile)
7. **Operadores de conjunto** (Distinct, Union, Intersect, Except)
8. **Operadores de elemento** (First, Last, Single, ElementAt)
9. **Sintaxis de consulta vs método** para diferentes escenarios

## 📚 Recursos Adicionales

- [Microsoft Docs - LINQ](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/)
- [101 LINQ Samples](https://docs.microsoft.com/en-us/samples/dotnet/try-samples/101-linq-samples/)

---

**🎯 ¡Has completado la Clase 2! Ahora comprendes los Operadores LINQ Básicos**

**📚 [Siguiente: Clase 3 - Operadores LINQ Avanzados](clase_3_operadores_linq_avanzados.md)**
