# 🚀 Clase 3: Operadores LINQ Avanzados

## 📋 Información de la Clase

- **Módulo**: Mid Level 4 - LINQ y Expresiones Lambda
- **Duración**: 2 horas
- **Nivel**: Intermedio-Avanzado
- **Prerrequisitos**: Conocimientos sólidos de operadores LINQ básicos

## 🎯 Objetivos de Aprendizaje

- Dominar operadores LINQ avanzados de unión y comparación
- Implementar consultas complejas con múltiples fuentes de datos
- Usar operadores de conversión y generación de secuencias
- Crear consultas dinámicas y optimizadas

---

## 📚 Navegación del Módulo 4

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_expresiones_lambda_avanzadas.md) | Expresiones Lambda Avanzadas | |
| [Clase 2](clase_2_operadores_linq_basicos.md) | Operadores LINQ Básicos | ← Anterior |
| **Clase 3** | **Operadores LINQ Avanzados** | ← Estás aquí |
| [Clase 4](clase_4_linq_to_objects.md) | LINQ to Objects | Siguiente → |
| [Clase 5](clase_5_linq_to_xml.md) | LINQ to XML | |
| [Clase 6](clase_6_linq_to_sql.md) | LINQ to SQL | |
| [Clase 7](clase_7_linq_performance.md) | LINQ y Performance | |
| [Clase 8](clase_8_linq_optimization.md) | Optimización de LINQ | |
| [Clase 9](clase_9_linq_extension_methods.md) | Métodos de Extensión LINQ | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Biblioteca | |

**← [Volver al README del Módulo 4](../midLevel_4/README.md)**

---

## 📚 Contenido Teórico

### 1. Operadores LINQ Avanzados

Los operadores LINQ avanzados permiten crear consultas más complejas y eficientes. Vamos a explorar técnicas avanzadas.

```csharp
// ===== OPERADORES LINQ AVANZADOS - IMPLEMENTACIÓN COMPLETA =====
namespace AdvancedLinqOperators
{
    // ===== MODELOS DE DATOS EXTENDIDOS =====
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
            public int SupplierId { get; set; }
            public List<string> Tags { get; set; }
            
            public Product(int id, string name, decimal price, string category, int stock, int supplierId)
            {
                Id = id;
                Name = name;
                Price = price;
                Category = category;
                Stock = stock;
                SupplierId = supplierId;
                CreatedDate = DateTime.Now;
                IsActive = true;
                Tags = new List<string>();
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
            public List<int> FavoriteCategories { get; set; }
            
            public Customer(int id, string name, string email, int age, string city)
            {
                Id = id;
                Name = name;
                Email = email;
                Age = age;
                City = city;
                TotalSpent = 0;
                RegistrationDate = DateTime.Now;
                FavoriteCategories = new List<int>();
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
            public string ShippingAddress { get; set; }
            
            public Order(int id, int customerId, string shippingAddress)
            {
                Id = id;
                CustomerId = customerId;
                ShippingAddress = shippingAddress;
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
            public decimal Discount { get; set; }
            
            public OrderItem(int productId, int quantity, decimal unitPrice, decimal discount = 0)
            {
                ProductId = productId;
                Quantity = quantity;
                UnitPrice = unitPrice;
                Discount = discount;
            }
        }
        
        public class Supplier
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string ContactEmail { get; set; }
            public string Phone { get; set; }
            public string Address { get; set; }
            public bool IsActive { get; set; }
            public decimal Rating { get; set; }
            
            public Supplier(int id, string name, string contactEmail, string phone, string address)
            {
                Id = id;
                Name = name;
                ContactEmail = contactEmail;
                Phone = phone;
                Address = address;
                IsActive = true;
                Rating = 0;
            }
            
            public override string ToString()
            {
                return $"ID: {Id}, Name: {Name}, Email: {ContactEmail}, Rating: {Rating:F1}";
            }
        }
        
        public class Review
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public int CustomerId { get; set; }
            public int Rating { get; set; }
            public string Comment { get; set; }
            public DateTime ReviewDate { get; set; }
            public bool IsVerified { get; set; }
            
            public Review(int id, int productId, int customerId, int rating, string comment)
            {
                Id = id;
                ProductId = productId;
                CustomerId = customerId;
                Rating = rating;
                Comment = comment;
                ReviewDate = DateTime.Now;
                IsVerified = false;
            }
            
            public override string ToString()
            {
                return $"Product: {ProductId}, Customer: {CustomerId}, Rating: {Rating}/5, Comment: {Comment}";
            }
        }
    }
    
    // ===== OPERADORES DE UNIÓN =====
    namespace JoinOperators
    {
        public class JoinExamples
        {
            // Join - Unión interna
            public static IEnumerable<object> GetProductsWithSuppliers(IEnumerable<Product> products, IEnumerable<Supplier> suppliers)
            {
                return products.Join(suppliers,
                    product => product.SupplierId,
                    supplier => supplier.Id,
                    (product, supplier) => new 
                    { 
                        product.Name,
                        product.Price,
                        product.Category,
                        SupplierName = supplier.Name,
                        SupplierEmail = supplier.ContactEmail
                    });
            }
            
            // GroupJoin - Unión de grupo
            public static IEnumerable<object> GetSuppliersWithProducts(IEnumerable<Supplier> suppliers, IEnumerable<Product> products)
            {
                return suppliers.GroupJoin(products,
                    supplier => supplier.Id,
                    product => product.SupplierId,
                    (supplier, productGroup) => new 
                    { 
                        supplier.Name,
                        supplier.ContactEmail,
                        ProductCount = productGroup.Count(),
                        TotalValue = productGroup.Sum(p => p.Price * p.Stock),
                        Products = productGroup.Select(p => p.Name)
                    });
            }
            
            // Join con múltiples condiciones
            public static IEnumerable<object> GetProductsWithActiveSuppliers(IEnumerable<Product> products, IEnumerable<Supplier> suppliers)
            {
                return products.Join(suppliers,
                    product => new { product.SupplierId, IsActive = true },
                    supplier => new { SupplierId = supplier.Id, supplier.IsActive },
                    (product, supplier) => new 
                    { 
                        product.Name,
                        product.Price,
                        SupplierName = supplier.Name,
                        SupplierRating = supplier.Rating
                    });
            }
            
            // Join con filtrado adicional
            public static IEnumerable<object> GetExpensiveProductsWithSuppliers(IEnumerable<Product> products, IEnumerable<Supplier> suppliers, decimal minPrice)
            {
                return products.Where(p => p.Price >= minPrice)
                    .Join(suppliers,
                        product => product.SupplierId,
                        supplier => supplier.Id,
                        (product, supplier) => new 
                        { 
                            product.Name,
                            product.Price,
                            product.Category,
                            SupplierName = supplier.Name,
                            SupplierRating = supplier.Rating
                        });
            }
            
            // Join con ordenamiento
            public static IEnumerable<object> GetProductsWithSuppliersOrdered(IEnumerable<Product> products, IEnumerable<Supplier> suppliers)
            {
                return products.Join(suppliers,
                    product => product.SupplierId,
                    supplier => supplier.Id,
                    (product, supplier) => new 
                    { 
                        product.Name,
                        product.Price,
                        product.Category,
                        SupplierName = supplier.Name,
                        SupplierRating = supplier.Rating
                    })
                    .OrderBy(x => x.Category)
                    .ThenByDescending(x => x.Price);
            }
            
            // Join con agregación
            public static IEnumerable<object> GetSupplierStats(IEnumerable<Supplier> suppliers, IEnumerable<Product> products)
            {
                return suppliers.GroupJoin(products,
                    supplier => supplier.Id,
                    product => product.SupplierId,
                    (supplier, productGroup) => new 
                    { 
                        supplier.Name,
                        supplier.ContactEmail,
                        ProductCount = productGroup.Count(),
                        AveragePrice = productGroup.Any() ? productGroup.Average(p => p.Price) : 0,
                        TotalStock = productGroup.Sum(p => p.Stock),
                        TotalValue = productGroup.Sum(p => p.Price * p.Stock),
                        Categories = productGroup.Select(p => p.Category).Distinct()
                    });
            }
        }
    }
    
    // ===== OPERADORES DE COMPARACIÓN =====
    namespace ComparisonOperators
    {
        public class ComparisonExamples
        {
            // SequenceEqual - Comparar secuencias
            public static bool AreProductListsEqual(IEnumerable<Product> list1, IEnumerable<Product> list2)
            {
                return list1.Select(p => p.Id).SequenceEqual(list2.Select(p => p.Id));
            }
            
            // SequenceEqual con comparador personalizado
            public static bool AreProductListsEqualByName(IEnumerable<Product> list1, IEnumerable<Product> list2)
            {
                return list1.Select(p => p.Name).SequenceEqual(list2.Select(p => p.Name), StringComparer.OrdinalIgnoreCase);
            }
            
            // Except - Diferencia de conjuntos
            public static IEnumerable<Product> GetUniqueProductsInFirst(IEnumerable<Product> list1, IEnumerable<Product> list2)
            {
                return list1.Except(list2, new ProductIdComparer());
            }
            
            // Intersect - Intersección de conjuntos
            public static IEnumerable<Product> GetCommonProducts(IEnumerable<Product> list1, IEnumerable<Product> list2)
            {
                return list1.Intersect(list2, new ProductIdComparer());
            }
            
            // Union - Unión de conjuntos
            public static IEnumerable<Product> GetAllUniqueProducts(IEnumerable<Product> list1, IEnumerable<Product> list2)
            {
                return list1.Union(list2, new ProductIdComparer());
            }
            
            // Distinct - Elementos únicos
            public static IEnumerable<Product> GetUniqueProducts(IEnumerable<Product> products)
            {
                return products.Distinct(new ProductIdComparer());
            }
            
            // Distinct con múltiples propiedades
            public static IEnumerable<Product> GetUniqueProductsByNameAndCategory(IEnumerable<Product> products)
            {
                return products.Distinct(new ProductNameCategoryComparer());
            }
        }
        
        public class ProductIdComparer : IEqualityComparer<Product>
        {
            public bool Equals(Product x, Product y)
            {
                if (x == null && y == null) return true;
                if (x == null || y == null) return false;
                return x.Id == y.Id;
            }
            
            public int GetHashCode(Product obj)
            {
                return obj.Id.GetHashCode();
            }
        }
        
        public class ProductNameCategoryComparer : IEqualityComparer<Product>
        {
            public bool Equals(Product x, Product y)
            {
                if (x == null && y == null) return true;
                if (x == null || y == null) return false;
                return x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase) && 
                       x.Category.Equals(y.Category, StringComparison.OrdinalIgnoreCase);
            }
            
            public int GetHashCode(Product obj)
            {
                return HashCode.Combine(obj.Name?.ToLower(), obj.Category?.ToLower());
            }
        }
    }
    
    // ===== OPERADORES DE CONVERSIÓN =====
    namespace ConversionOperators
    {
        public class ConversionExamples
        {
            // ToList - Convertir a List
            public static List<Product> GetProductList(IEnumerable<Product> products)
            {
                return products.Where(p => p.IsActive).ToList();
            }
            
            // ToArray - Convertir a Array
            public static Product[] GetProductArray(IEnumerable<Product> products)
            {
                return products.Where(p => p.Stock > 0).ToArray();
            }
            
            // ToDictionary - Convertir a Dictionary
            public static Dictionary<int, Product> GetProductDictionary(IEnumerable<Product> products)
            {
                return products.ToDictionary(p => p.Id, p => p);
            }
            
            // ToDictionary con selector de valor
            public static Dictionary<string, decimal> GetProductPriceDictionary(IEnumerable<Product> products)
            {
                return products.ToDictionary(p => p.Name, p => p.Price);
            }
            
            // ToLookup - Convertir a Lookup
            public static ILookup<string, Product> GetProductLookupByCategory(IEnumerable<Product> products)
            {
                return products.ToLookup(p => p.Category);
            }
            
            // ToLookup con múltiples claves
            public static ILookup<object, Product> GetProductLookupByCategoryAndActive(IEnumerable<Product> products)
            {
                return products.ToLookup(p => new { p.Category, p.IsActive });
            }
            
            // Cast - Convertir tipos
            public static IEnumerable<int> GetProductIds(IEnumerable<object> objects)
            {
                return objects.Cast<int>();
            }
            
            // OfType - Filtrar por tipo
            public static IEnumerable<string> GetStringObjects(IEnumerable<object> objects)
            {
                return objects.OfType<string>();
            }
            
            // AsEnumerable - Convertir a IEnumerable
            public static IEnumerable<Product> GetEnumerableFromList(List<Product> products)
            {
                return products.AsEnumerable();
            }
            
            // AsQueryable - Convertir a IQueryable
            public static IQueryable<Product> GetQueryableFromList(List<Product> products)
            {
                return products.AsQueryable();
            }
        }
    }
    
    // ===== OPERADORES DE GENERACIÓN =====
    namespace GenerationOperators
    {
        public class GenerationExamples
        {
            // Range - Generar rango de números
            public static IEnumerable<int> GetNumberRange(int start, int count)
            {
                return Enumerable.Range(start, count);
            }
            
            // Repeat - Repetir elemento
            public static IEnumerable<string> GetRepeatedString(string value, int count)
            {
                return Enumerable.Repeat(value, count);
            }
            
            // Empty - Secuencia vacía
            public static IEnumerable<Product> GetEmptyProductList()
            {
                return Enumerable.Empty<Product>();
            }
            
            // DefaultIfEmpty - Valor por defecto si está vacío
            public static IEnumerable<Product> GetProductsOrDefault(IEnumerable<Product> products)
            {
                return products.DefaultIfEmpty(new Product(0, "No products available", 0, "None", 0, 0));
            }
            
            // Generate sequence with custom logic
            public static IEnumerable<int> GenerateFibonacci(int count)
            {
                if (count <= 0) yield break;
                
                int a = 0, b = 1;
                yield return a;
                
                for (int i = 1; i < count; i++)
                {
                    yield return b;
                    int temp = a + b;
                    a = b;
                    b = temp;
                }
            }
            
            // Generate sequence with condition
            public static IEnumerable<int> GenerateEvenNumbers(int max)
            {
                for (int i = 0; i <= max; i += 2)
                {
                    yield return i;
                }
            }
            
            // Generate sequence with transformation
            public static IEnumerable<object> GenerateProductPlaceholders(int count)
            {
                return Enumerable.Range(1, count).Select(i => new 
                { 
                    Id = i,
                    Name = $"Product {i}",
                    Price = i * 10.0m,
                    Category = i % 2 == 0 ? "Electronics" : "Books"
                });
            }
        }
    }
    
    // ===== OPERADORES DE CONCATENACIÓN =====
    namespace ConcatenationOperators
    {
        public class ConcatenationExamples
        {
            // Concat - Concatenar secuencias
            public static IEnumerable<Product> ConcatenateProductLists(IEnumerable<Product> list1, IEnumerable<Product> list2)
            {
                return list1.Concat(list2);
            }
            
            // Concat con filtrado
            public static IEnumerable<Product> ConcatenateActiveProducts(IEnumerable<Product> list1, IEnumerable<Product> list2)
            {
                return list1.Where(p => p.IsActive).Concat(list2.Where(p => p.IsActive));
            }
            
            // Concat con ordenamiento
            public static IEnumerable<Product> ConcatenateAndOrderProducts(IEnumerable<Product> list1, IEnumerable<Product> list2)
            {
                return list1.Concat(list2).OrderBy(p => p.Category).ThenBy(p => p.Name);
            }
            
            // Concat con transformación
            public static IEnumerable<object> ConcatenateProductSummaries(IEnumerable<Product> list1, IEnumerable<Product> list2)
            {
                var summary1 = list1.Select(p => new { p.Name, p.Price, Source = "List1" });
                var summary2 = list2.Select(p => new { p.Name, p.Price, Source = "List2" });
                return summary1.Concat(summary2);
            }
            
            // Concat con validación
            public static IEnumerable<Product> ConcatenateValidProducts(IEnumerable<Product> list1, IEnumerable<Product> list2)
            {
                var validProducts1 = list1.Where(p => p.Price > 0 && p.Stock >= 0);
                var validProducts2 = list2.Where(p => p.Price > 0 && p.Stock >= 0);
                return validProducts1.Concat(validProducts2);
            }
        }
    }
    
    // ===== OPERADORES DE AGRUPACIÓN AVANZADA =====
    namespace AdvancedGroupingOperators
    {
        public class AdvancedGroupingExamples
        {
            // GroupBy con múltiples claves
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
                    AveragePrice = group.Average(p => p.Price),
                    TotalValue = group.Sum(p => p.Price * p.Stock)
                });
            }
            
            // GroupBy con ordenamiento
            public static IEnumerable<object> GroupProductsByCategoryOrdered(IEnumerable<Product> products)
            {
                return products.GroupBy(p => p.Category, (category, group) => new 
                { 
                    Category = category,
                    ProductCount = group.Count(),
                    AveragePrice = group.Average(p => p.Price),
                    Products = group.OrderBy(p => p.Name).Select(p => p.Name)
                }).OrderByDescending(g => g.ProductCount);
            }
            
            // GroupBy con filtrado
            public static IEnumerable<object> GroupActiveProductsByCategory(IEnumerable<Product> products)
            {
                return products.Where(p => p.IsActive)
                    .GroupBy(p => p.Category, (category, group) => new 
                    { 
                        Category = category,
                        ActiveProductCount = group.Count(),
                        TotalStock = group.Sum(p => p.Stock),
                        Products = group.Select(p => new { p.Name, p.Price, p.Stock })
                    });
            }
            
            // GroupBy con agregación compleja
            public static IEnumerable<object> GetCategoryAnalytics(IEnumerable<Product> products)
            {
                return products.GroupBy(p => p.Category, (category, group) => new 
                { 
                    Category = category,
                    ProductCount = group.Count(),
                    AveragePrice = group.Average(p => p.Price),
                    MinPrice = group.Min(p => p.Price),
                    MaxPrice = group.Max(p => p.Price),
                    TotalStock = group.Sum(p => p.Stock),
                    TotalValue = group.Sum(p => p.Price * p.Stock),
                    ActiveProducts = group.Count(p => p.IsActive),
                    InactiveProducts = group.Count(p => !p.IsActive)
                });
            }
            
            // GroupBy con múltiples niveles
            public static IEnumerable<object> GetNestedGrouping(IEnumerable<Product> products, IEnumerable<Supplier> suppliers)
            {
                return suppliers.GroupJoin(products,
                    supplier => supplier.Id,
                    product => product.SupplierId,
                    (supplier, productGroup) => new 
                    { 
                        SupplierName = supplier.Name,
                        Categories = productGroup.GroupBy(p => p.Category, (category, products) => new 
                        { 
                            Category = category,
                            ProductCount = products.Count(),
                            AveragePrice = products.Average(p => p.Price),
                            Products = products.Select(p => p.Name)
                        })
                    });
            }
        }
    }
    
    // ===== OPERADORES DE PAGINACIÓN AVANZADA =====
    namespace AdvancedPaginationOperators
    {
        public class AdvancedPaginationExamples
        {
            // Paginación con ordenamiento
            public static IEnumerable<Product> GetProductsPage(IEnumerable<Product> products, int pageNumber, int pageSize, string sortBy = "Name")
            {
                var query = sortBy.ToLower() switch
                {
                    "name" => products.OrderBy(p => p.Name),
                    "price" => products.OrderBy(p => p.Price),
                    "category" => products.OrderBy(p => p.Category),
                    "stock" => products.OrderBy(p => p.Stock),
                    _ => products.OrderBy(p => p.Name)
                };
                
                return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }
            
            // Paginación con filtrado
            public static IEnumerable<Product> GetFilteredProductsPage(IEnumerable<Product> products, 
                int pageNumber, int pageSize, 
                string category = null, decimal? minPrice = null, decimal? maxPrice = null)
            {
                var query = products.AsQueryable();
                
                if (!string.IsNullOrEmpty(category))
                    query = query.Where(p => p.Category == category);
                
                if (minPrice.HasValue)
                    query = query.Where(p => p.Price >= minPrice.Value);
                
                if (maxPrice.HasValue)
                    query = query.Where(p => p.Price <= maxPrice.Value);
                
                return query.OrderBy(p => p.Name)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);
            }
            
            // Paginación con información de página
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
            
            // Paginación con agrupación
            public static object GetCategoryPageWithProducts(IEnumerable<Product> products, int pageNumber, int pageSize)
            {
                var categories = products.GroupBy(p => p.Category, (category, group) => new 
                { 
                    Category = category,
                    ProductCount = group.Count(),
                    Products = group.OrderBy(p => p.Name)
                }).OrderBy(g => g.Category);
                
                var totalCategories = categories.Count();
                var totalPages = (int)Math.Ceiling((double)totalCategories / pageSize);
                var pageCategories = categories.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                
                return new 
                { 
                    Categories = pageCategories,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCategories = totalCategories,
                    TotalPages = totalPages
                };
            }
        }
    }
    
    // ===== OPERADORES DE CONSULTAS DINÁMICAS =====
    namespace DynamicQueryOperators
    {
        public class DynamicQueryExamples
        {
            // Consulta dinámica con filtros opcionales
            public static IEnumerable<Product> GetProductsWithDynamicFilters(IEnumerable<Product> products, 
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
            
            // Consulta dinámica con ordenamiento
            public static IEnumerable<Product> GetProductsWithDynamicOrdering(IEnumerable<Product> products, 
                string sortBy = "Name", bool ascending = true)
            {
                var query = products.AsQueryable();
                
                query = sortBy.ToLower() switch
                {
                    "name" => ascending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                    "price" => ascending ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
                    "category" => ascending ? query.OrderBy(p => p.Category) : query.OrderByDescending(p => p.Category),
                    "stock" => ascending ? query.OrderBy(p => p.Stock) : query.OrderByDescending(p => p.Stock),
                    "createddate" => ascending ? query.OrderBy(p => p.CreatedDate) : query.OrderByDescending(p => p.CreatedDate),
                    _ => query.OrderBy(p => p.Name)
                };
                
                return query;
            }
            
            // Consulta dinámica con proyección
            public static IEnumerable<object> GetProductsWithDynamicProjection(IEnumerable<Product> products, 
                params string[] fields)
            {
                var query = products.AsQueryable();
                
                if (fields == null || fields.Length == 0)
                {
                    return query.Select(p => new { p.Id, p.Name, p.Price, p.Category });
                }
                
                // Implementación simplificada - en un caso real usarías Expression Trees
                return query.Select(p => new 
                { 
                    Id = fields.Contains("Id") ? p.Id : 0,
                    Name = fields.Contains("Name") ? p.Name : "",
                    Price = fields.Contains("Price") ? p.Price : 0m,
                    Category = fields.Contains("Category") ? p.Category : "",
                    Stock = fields.Contains("Stock") ? p.Stock : 0,
                    IsActive = fields.Contains("IsActive") ? p.IsActive : false
                });
            }
            
            // Consulta dinámica con agrupación
            public static IEnumerable<object> GetProductsWithDynamicGrouping(IEnumerable<Product> products, 
                string groupBy = "Category")
            {
                return groupBy.ToLower() switch
                {
                    "category" => products.GroupBy(p => p.Category, (key, group) => new 
                    { 
                        GroupKey = key,
                        Count = group.Count(),
                        AveragePrice = group.Average(p => p.Price)
                    }),
                    "supplierid" => products.GroupBy(p => p.SupplierId, (key, group) => new 
                    { 
                        GroupKey = key,
                        Count = group.Count(),
                        AveragePrice = group.Average(p => p.Price)
                    }),
                    "isactive" => products.GroupBy(p => p.IsActive, (key, group) => new 
                    { 
                        GroupKey = key,
                        Count = group.Count(),
                        AveragePrice = group.Average(p => p.Price)
                    }),
                    _ => products.GroupBy(p => p.Category, (key, group) => new 
                    { 
                        GroupKey = key,
                        Count = group.Count(),
                        AveragePrice = group.Average(p => p.Price)
                    })
                };
            }
        }
    }
    
    // ===== OPERADORES DE OPTIMIZACIÓN =====
    namespace OptimizationOperators
    {
        public class OptimizationExamples
        {
            // AsParallel - Paralelización
            public static IEnumerable<object> GetParallelProductStats(IEnumerable<Product> products)
            {
                return products.AsParallel()
                    .GroupBy(p => p.Category, (category, group) => new 
                    { 
                        Category = category,
                        Count = group.Count(),
                        AveragePrice = group.Average(p => p.Price),
                        TotalValue = group.Sum(p => p.Price * p.Stock)
                    });
            }
            
            // AsParallel con ordenamiento
            public static IEnumerable<Product> GetParallelOrderedProducts(IEnumerable<Product> products)
            {
                return products.AsParallel()
                    .OrderBy(p => p.Category)
                    .ThenBy(p => p.Name);
            }
            
            // AsParallel con filtrado
            public static IEnumerable<Product> GetParallelFilteredProducts(IEnumerable<Product> products, decimal minPrice)
            {
                return products.AsParallel()
                    .Where(p => p.Price >= minPrice)
                    .OrderBy(p => p.Price);
            }
            
            // WithDegreeOfParallelism - Control de paralelización
            public static IEnumerable<object> GetControlledParallelStats(IEnumerable<Product> products, int degreeOfParallelism)
            {
                return products.AsParallel()
                    .WithDegreeOfParallelism(degreeOfParallelism)
                    .GroupBy(p => p.Category, (category, group) => new 
                    { 
                        Category = category,
                        Count = group.Count(),
                        AveragePrice = group.Average(p => p.Price)
                    });
            }
            
            // AsSequential - Volver a secuencial
            public static IEnumerable<Product> GetSequentialProducts(IEnumerable<Product> products)
            {
                return products.AsParallel()
                    .Where(p => p.IsActive)
                    .AsSequential()
                    .OrderBy(p => p.Name);
            }
        }
    }
}

// Uso de Operadores LINQ Avanzados
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Operadores LINQ Avanzados - Clase 3 ===\n");
        
        // Crear datos de ejemplo
        var products = new List<Product>
        {
            new Product(1, "Laptop", 1200.00m, "Electronics", 15, 1),
            new Product(2, "Mouse", 25.50m, "Electronics", 50, 1),
            new Product(3, "Keyboard", 75.00m, "Electronics", 30, 2),
            new Product(4, "Book", 15.99m, "Books", 100, 3),
            new Product(5, "Pen", 2.50m, "Office", 200, 3),
            new Product(6, "Desk", 350.00m, "Furniture", 8, 2),
            new Product(7, "Chair", 150.00m, "Furniture", 12, 2),
            new Product(8, "Monitor", 300.00m, "Electronics", 20, 1),
            new Product(9, "Headphones", 89.99m, "Electronics", 25, 1),
            new Product(10, "Notebook", 8.99m, "Office", 150, 3)
        };
        
        var suppliers = new List<Supplier>
        {
            new Supplier(1, "TechCorp", "tech@corp.com", "555-0101", "123 Tech St"),
            new Supplier(2, "OfficeMax", "office@max.com", "555-0102", "456 Office Ave"),
            new Supplier(3, "BookWorld", "books@world.com", "555-0103", "789 Book Blvd")
        };
        
        // Ejemplos de unión
        Console.WriteLine("1. Operadores de Unión:");
        var productsWithSuppliers = JoinOperators.JoinExamples.GetProductsWithSuppliers(products, suppliers);
        Console.WriteLine($"Productos con proveedores: {productsWithSuppliers.Count()}");
        
        // Ejemplos de comparación
        Console.WriteLine("\n2. Operadores de Comparación:");
        var uniqueProducts = ComparisonOperators.ComparisonExamples.GetUniqueProducts(products);
        Console.WriteLine($"Productos únicos: {uniqueProducts.Count()}");
        
        // Ejemplos de conversión
        Console.WriteLine("\n3. Operadores de Conversión:");
        var productDict = ConversionOperators.ConversionExamples.GetProductDictionary(products);
        Console.WriteLine($"Diccionario de productos: {productDict.Count} elementos");
        
        // Ejemplos de generación
        Console.WriteLine("\n4. Operadores de Generación:");
        var numberRange = GenerationOperators.GenerationExamples.GetNumberRange(1, 10);
        Console.WriteLine("Rango de números: " + string.Join(", ", numberRange));
        
        // Ejemplos de concatenación
        Console.WriteLine("\n5. Operadores de Concatenación:");
        var concatenatedProducts = ConcatenationOperators.ConcatenationExamples.ConcatenateProductLists(products, products);
        Console.WriteLine($"Productos concatenados: {concatenatedProducts.Count()}");
        
        // Ejemplos de agrupación avanzada
        Console.WriteLine("\n6. Agrupación Avanzada:");
        var categoryAnalytics = AdvancedGroupingOperators.AdvancedGroupingExamples.GetCategoryAnalytics(products);
        foreach (var analytics in categoryAnalytics)
        {
            Console.WriteLine($"Categoría: {analytics.Category}, Productos: {analytics.ProductCount}, Valor Total: {analytics.TotalValue:C}");
        }
        
        // Ejemplos de paginación avanzada
        Console.WriteLine("\n7. Paginación Avanzada:");
        var pageInfo = AdvancedPaginationOperators.AdvancedPaginationExamples.GetProductsPageWithInfo(products, 1, 5);
        Console.WriteLine($"Página 1: {pageInfo.Items.Count()} productos de {pageInfo.TotalCount} total");
        
        // Ejemplos de consultas dinámicas
        Console.WriteLine("\n8. Consultas Dinámicas:");
        var filteredProducts = DynamicQueryOperators.DynamicQueryExamples.GetProductsWithDynamicFilters(products, "Electronics", 50);
        Console.WriteLine($"Productos filtrados: {filteredProducts.Count()}");
        
        // Ejemplos de optimización
        Console.WriteLine("\n9. Optimización:");
        var parallelStats = OptimizationOperators.OptimizationExamples.GetParallelProductStats(products);
        Console.WriteLine($"Estadísticas paralelas: {parallelStats.Count()} categorías");
        
        Console.WriteLine("\n✅ Operadores LINQ Avanzados funcionando correctamente!");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Uniones y Comparaciones
Crea consultas que unan múltiples fuentes de datos y comparen colecciones.

### Ejercicio 2: Conversiones y Generación
Implementa operadores de conversión y genera secuencias personalizadas.

### Ejercicio 3: Consultas Dinámicas
Crea consultas que se adapten dinámicamente a diferentes parámetros.

## 🔍 Puntos Clave

1. **Operadores de unión** (Join, GroupJoin) para combinar datos relacionados
2. **Operadores de comparación** (SequenceEqual, Except, Intersect, Union) para comparar colecciones
3. **Operadores de conversión** (ToList, ToArray, ToDictionary, ToLookup, Cast, OfType)
4. **Operadores de generación** (Range, Repeat, Empty, DefaultIfEmpty) para crear secuencias
5. **Operadores de concatenación** (Concat) para unir colecciones
6. **Agrupación avanzada** con múltiples claves y agregaciones complejas
7. **Paginación avanzada** con información de navegación
8. **Consultas dinámicas** que se adaptan a parámetros variables
9. **Optimización** con paralelización (AsParallel, WithDegreeOfParallelism)

## 📚 Recursos Adicionales

- [Microsoft Docs - Advanced LINQ](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/advanced-querying-techniques)
- [PLINQ Documentation](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/introduction-to-plinq)

---

**🎯 ¡Has completado la Clase 3! Ahora comprendes los Operadores LINQ Avanzados**

**📚 [Siguiente: Clase 4 - LINQ to Objects](clase_4_linq_to_objects.md)**
