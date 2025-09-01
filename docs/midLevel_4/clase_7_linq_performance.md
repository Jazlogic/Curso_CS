# 🚀 Clase 7: LINQ y Performance

## 📋 Información de la Clase

- **Módulo**: Mid Level 4 - LINQ y Expresiones Lambda
- **Duración**: 2 horas
- **Nivel**: Intermedio-Avanzado
- **Prerrequisitos**: Conocimientos sólidos de LINQ y programación C#

## 🎯 Objetivos de Aprendizaje

- Comprender el impacto del rendimiento en consultas LINQ
- Implementar técnicas de optimización para LINQ
- Usar herramientas de profiling y benchmarking
- Crear consultas eficientes y escalables

---

## 📚 Navegación del Módulo 4

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_expresiones_lambda_avanzadas.md) | Expresiones Lambda Avanzadas | |
| [Clase 2](clase_2_operadores_linq_basicos.md) | Operadores LINQ Básicos | |
| [Clase 3](clase_3_operadores_linq_avanzados.md) | Operadores LINQ Avanzados | |
| [Clase 4](clase_4_linq_to_objects.md) | LINQ to Objects | |
| [Clase 5](clase_5_linq_to_xml.md) | LINQ to XML | |
| [Clase 6](clase_6_linq_to_sql.md) | LINQ to SQL | ← Anterior |
| **Clase 7** | **LINQ y Performance** | ← Estás aquí |
| [Clase 8](clase_8_linq_optimization.md) | Optimización de LINQ | Siguiente → |
| [Clase 9](clase_9_linq_extension_methods.md) | Métodos de Extensión LINQ | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Biblioteca | |

**← [Volver al README del Módulo 4](../midLevel_4/README.md)**

---

## 📚 Contenido Teórico

### 1. LINQ y Performance

El rendimiento es crucial en aplicaciones en producción. Vamos a explorar técnicas para optimizar consultas LINQ.

```csharp
// ===== LINQ Y PERFORMANCE - IMPLEMENTACIÓN COMPLETA =====
using System.Diagnostics;
using System.Collections.Generic;

namespace LinqPerformance
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
    
    // ===== BENCHMARKING =====
    namespace Benchmarking
    {
        public class PerformanceBenchmark
        {
            public static void MeasureExecutionTime(Action action, string operationName, int iterations = 1)
            {
                var stopwatch = Stopwatch.StartNew();
                
                for (int i = 0; i < iterations; i++)
                {
                    action();
                }
                
                stopwatch.Stop();
                var averageTime = stopwatch.ElapsedMilliseconds / (double)iterations;
                
                Console.WriteLine($"{operationName}: {averageTime:F2} ms (promedio de {iterations} iteraciones)");
            }
            
            public static void CompareOperations(Action operation1, Action operation2, 
                string name1, string name2, int iterations = 1000)
            {
                Console.WriteLine($"\nComparando {name1} vs {name2} ({iterations} iteraciones):");
                
                MeasureExecutionTime(operation1, name1, iterations);
                MeasureExecutionTime(operation2, name2, iterations);
            }
            
            public static void MeasureMemoryUsage(Action action, string operationName)
            {
                var initialMemory = GC.GetTotalMemory(true);
                
                action();
                
                var finalMemory = GC.GetTotalMemory(true);
                var memoryUsed = finalMemory - initialMemory;
                
                Console.WriteLine($"{operationName}: {memoryUsed:N0} bytes de memoria");
            }
        }
        
        public class BenchmarkResult
        {
            public string OperationName { get; set; }
            public long ExecutionTimeMs { get; set; }
            public long MemoryUsageBytes { get; set; }
            public int Iterations { get; set; }
            
            public double AverageExecutionTime => ExecutionTimeMs / (double)Iterations;
            
            public override string ToString()
            {
                return $"{OperationName}: {AverageExecutionTime:F2} ms, {MemoryUsageBytes:N0} bytes";
            }
        }
    }
    
    // ===== PROBLEMAS DE PERFORMANCE COMUNES =====
    namespace PerformanceIssues
    {
        public class CommonPerformanceIssues
        {
            // N+1 Query Problem
            public static void NPlusOneProblem(IEnumerable<Customer> customers, IEnumerable<Order> orders)
            {
                Console.WriteLine("=== N+1 Query Problem ===");
                
                var stopwatch = Stopwatch.StartNew();
                
                foreach (var customer in customers)
                {
                    var customerOrders = orders.Where(o => o.CustomerId == customer.Id).ToList();
                    Console.WriteLine($"Cliente {customer.Name}: {customerOrders.Count} órdenes");
                }
                
                stopwatch.Stop();
                Console.WriteLine($"N+1 Query: {stopwatch.ElapsedMilliseconds} ms");
            }
            
            // Solución optimizada
            public static void OptimizedNPlusOne(IEnumerable<Customer> customers, IEnumerable<Order> orders)
            {
                Console.WriteLine("=== Solución Optimizada ===");
                
                var stopwatch = Stopwatch.StartNew();
                
                var ordersByCustomer = orders.GroupBy(o => o.CustomerId)
                    .ToDictionary(g => g.Key, g => g.ToList());
                
                foreach (var customer in customers)
                {
                    var customerOrders = ordersByCustomer.GetValueOrDefault(customer.Id, new List<Order>());
                    Console.WriteLine($"Cliente {customer.Name}: {customerOrders.Count} órdenes");
                }
                
                stopwatch.Stop();
                Console.WriteLine($"Optimizado: {stopwatch.ElapsedMilliseconds} ms");
            }
            
            // Evaluación diferida vs inmediata
            public static void LazyVsEagerEvaluation(IEnumerable<Product> products)
            {
                Console.WriteLine("=== Evaluación Diferida vs Inmediata ===");
                
                // Evaluación diferida
                var lazyQuery = products.Where(p => p.IsActive).Select(p => p.Name);
                
                var stopwatch = Stopwatch.StartNew();
                var lazyResult = lazyQuery.ToList(); // La consulta se ejecuta aquí
                stopwatch.Stop();
                Console.WriteLine($"Evaluación diferida: {stopwatch.ElapsedMilliseconds} ms");
                
                // Evaluación inmediata
                stopwatch.Restart();
                var eagerResult = products.Where(p => p.IsActive).Select(p => p.Name).ToList();
                stopwatch.Stop();
                Console.WriteLine($"Evaluación inmediata: {stopwatch.ElapsedMilliseconds} ms");
            }
            
            // Consultas innecesariamente complejas
            public static void ComplexVsSimpleQuery(IEnumerable<Product> products)
            {
                Console.WriteLine("=== Consulta Compleja vs Simple ===");
                
                // Consulta compleja
                var stopwatch = Stopwatch.StartNew();
                var complexResult = products
                    .Where(p => p.IsActive)
                    .Select(p => new { p.Name, p.Price })
                    .Where(p => p.Price > 100)
                    .OrderBy(p => p.Name)
                    .Select(p => p.Name)
                    .ToList();
                stopwatch.Stop();
                Console.WriteLine($"Consulta compleja: {stopwatch.ElapsedMilliseconds} ms");
                
                // Consulta simple
                stopwatch.Restart();
                var simpleResult = products
                    .Where(p => p.IsActive && p.Price > 100)
                    .OrderBy(p => p.Name)
                    .Select(p => p.Name)
                    .ToList();
                stopwatch.Stop();
                Console.WriteLine($"Consulta simple: {stopwatch.ElapsedMilliseconds} ms");
            }
        }
    }
    
    // ===== TÉCNICAS DE OPTIMIZACIÓN =====
    namespace OptimizationTechniques
    {
        public class OptimizationExamples
        {
            // Uso de ToList() para evitar múltiples enumeraciones
            public static void AvoidMultipleEnumerations(IEnumerable<Product> products)
            {
                Console.WriteLine("=== Evitar Múltiples Enumeraciones ===");
                
                // Malo: múltiples enumeraciones
                var stopwatch = Stopwatch.StartNew();
                var expensiveProducts = products.Where(p => p.Price > 100);
                var count = expensiveProducts.Count();
                var average = expensiveProducts.Average(p => p.Price);
                var names = expensiveProducts.Select(p => p.Name).ToList();
                stopwatch.Stop();
                Console.WriteLine($"Múltiples enumeraciones: {stopwatch.ElapsedMilliseconds} ms");
                
                // Bueno: una sola enumeración
                stopwatch.Restart();
                var expensiveProductsList = products.Where(p => p.Price > 100).ToList();
                var count2 = expensiveProductsList.Count;
                var average2 = expensiveProductsList.Average(p => p.Price);
                var names2 = expensiveProductsList.Select(p => p.Name).ToList();
                stopwatch.Stop();
                Console.WriteLine($"Una enumeración: {stopwatch.ElapsedMilliseconds} ms");
            }
            
            // Uso de Any() vs Count() > 0
            public static void AnyVsCount(IEnumerable<Product> products)
            {
                Console.WriteLine("=== Any() vs Count() > 0 ===");
                
                // Usando Count()
                var stopwatch = Stopwatch.StartNew();
                var hasExpensiveProducts = products.Where(p => p.Price > 1000).Count() > 0;
                stopwatch.Stop();
                Console.WriteLine($"Count() > 0: {stopwatch.ElapsedMilliseconds} ms");
                
                // Usando Any()
                stopwatch.Restart();
                var hasExpensiveProducts2 = products.Any(p => p.Price > 1000);
                stopwatch.Stop();
                Console.WriteLine($"Any(): {stopwatch.ElapsedMilliseconds} ms");
            }
            
            // Uso de FirstOrDefault() vs Where().FirstOrDefault()
            public static void FirstOrDefaultOptimization(IEnumerable<Product> products)
            {
                Console.WriteLine("=== FirstOrDefault() Optimización ===");
                
                // Menos eficiente
                var stopwatch = Stopwatch.StartNew();
                var product1 = products.Where(p => p.Price > 100).FirstOrDefault();
                stopwatch.Stop();
                Console.WriteLine($"Where().FirstOrDefault(): {stopwatch.ElapsedMilliseconds} ms");
                
                // Más eficiente
                stopwatch.Restart();
                var product2 = products.FirstOrDefault(p => p.Price > 100);
                stopwatch.Stop();
                Console.WriteLine($"FirstOrDefault(): {stopwatch.ElapsedMilliseconds} ms");
            }
            
            // Uso de Select() para proyección temprana
            public static void EarlyProjection(IEnumerable<Product> products)
            {
                Console.WriteLine("=== Proyección Temprana ===");
                
                // Sin proyección temprana
                var stopwatch = Stopwatch.StartNew();
                var result1 = products
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Name)
                    .Select(p => new { p.Name, p.Price })
                    .ToList();
                stopwatch.Stop();
                Console.WriteLine($"Sin proyección temprana: {stopwatch.ElapsedMilliseconds} ms");
                
                // Con proyección temprana
                stopwatch.Restart();
                var result2 = products
                    .Where(p => p.IsActive)
                    .Select(p => new { p.Name, p.Price })
                    .OrderBy(p => p.Name)
                    .ToList();
                stopwatch.Stop();
                Console.WriteLine($"Con proyección temprana: {stopwatch.ElapsedMilliseconds} ms");
            }
        }
    }
    
    // ===== PARALELIZACIÓN =====
    namespace Parallelization
    {
        public class ParallelExamples
        {
            // PLINQ básico
            public static void BasicPLinq(IEnumerable<Product> products)
            {
                Console.WriteLine("=== PLINQ Básico ===");
                
                // LINQ secuencial
                var stopwatch = Stopwatch.StartNew();
                var expensiveProducts = products
                    .Where(p => p.Price > 100)
                    .Select(p => new { p.Name, p.Price })
                    .ToList();
                stopwatch.Stop();
                Console.WriteLine($"LINQ secuencial: {stopwatch.ElapsedMilliseconds} ms");
                
                // PLINQ paralelo
                stopwatch.Restart();
                var expensiveProductsParallel = products
                    .AsParallel()
                    .Where(p => p.Price > 100)
                    .Select(p => new { p.Name, p.Price })
                    .ToList();
                stopwatch.Stop();
                Console.WriteLine($"PLINQ paralelo: {stopwatch.ElapsedMilliseconds} ms");
            }
            
            // PLINQ con control de paralelización
            public static void ControlledPLinq(IEnumerable<Product> products)
            {
                Console.WriteLine("=== PLINQ Controlado ===");
                
                var stopwatch = Stopwatch.StartNew();
                var result = products
                    .AsParallel()
                    .WithDegreeOfParallelism(Environment.ProcessorCount)
                    .Where(p => p.IsActive)
                    .Select(p => new { p.Name, p.Price, p.Category })
                    .OrderBy(p => p.Name)
                    .ToList();
                stopwatch.Stop();
                Console.WriteLine($"PLINQ controlado: {stopwatch.ElapsedMilliseconds} ms");
            }
            
            // PLINQ con ordenamiento
            public static void ParallelOrdering(IEnumerable<Product> products)
            {
                Console.WriteLine("=== PLINQ con Ordenamiento ===");
                
                var stopwatch = Stopwatch.StartNew();
                var result = products
                    .AsParallel()
                    .Where(p => p.IsActive)
                    .AsSequential() // Volver a secuencial para ordenamiento
                    .OrderBy(p => p.Name)
                    .ToList();
                stopwatch.Stop();
                Console.WriteLine($"PLINQ con ordenamiento: {stopwatch.ElapsedMilliseconds} ms");
            }
        }
    }
    
    // ===== CACHE Y MEMOIZACIÓN =====
    namespace Caching
    {
        public class CachingExamples
        {
            private static readonly Dictionary<string, object> _cache = new Dictionary<string, object>();
            
            // Cache simple
            public static IEnumerable<Product> GetExpensiveProductsCached(IEnumerable<Product> products, decimal minPrice)
            {
                var cacheKey = $"expensive_products_{minPrice}";
                
                if (_cache.TryGetValue(cacheKey, out var cached))
                {
                    return (IEnumerable<Product>)cached;
                }
                
                var result = products.Where(p => p.Price >= minPrice).ToList();
                _cache[cacheKey] = result;
                
                return result;
            }
            
            // Cache con TTL
            public static class CacheWithTTL
            {
                private static readonly Dictionary<string, (object Value, DateTime Expiry)> _cache = 
                    new Dictionary<string, (object, DateTime)>();
                
                public static IEnumerable<Product> GetProductsCached(IEnumerable<Product> products, string category)
                {
                    var cacheKey = $"products_category_{category}";
                    
                    if (_cache.TryGetValue(cacheKey, out var cached) && DateTime.UtcNow < cached.Expiry)
                    {
                        return (IEnumerable<Product>)cached.Value;
                    }
                    
                    var result = products.Where(p => p.Category == category).ToList();
                    _cache[cacheKey] = (result, DateTime.UtcNow.AddMinutes(5)); // TTL de 5 minutos
                    
                    return result;
                }
            }
            
            // Memoización de funciones
            public static class Memoization
            {
                private static readonly Dictionary<string, object> _memoCache = new Dictionary<string, object>();
                
                public static Func<T, TResult> Memoize<T, TResult>(Func<T, TResult> function)
                {
                    return input =>
                    {
                        var key = input?.ToString() ?? "null";
                        
                        if (_memoCache.TryGetValue(key, out var cached))
                        {
                            return (TResult)cached;
                        }
                        
                        var result = function(input);
                        _memoCache[key] = result;
                        
                        return result;
                    };
                }
            }
        }
    }
    
    // ===== OPTIMIZACIÓN DE MEMORIA =====
    namespace MemoryOptimization
    {
        public class MemoryOptimizationExamples
        {
            // Uso de yield return para streaming
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
            
            // Uso de Span<T> para operaciones de alto rendimiento
            public static void SpanOptimization(int[] numbers)
            {
                Console.WriteLine("=== Optimización con Span<T> ===");
                
                var stopwatch = Stopwatch.StartNew();
                var result = numbers.Where(n => n > 100).Sum();
                stopwatch.Stop();
                Console.WriteLine($"LINQ tradicional: {stopwatch.ElapsedMilliseconds} ms");
                
                // Nota: Span<T> se usa principalmente para operaciones de bajo nivel
                // Este es un ejemplo conceptual
                stopwatch.Restart();
                var span = new Span<int>(numbers);
                var sum = 0;
                for (int i = 0; i < span.Length; i++)
                {
                    if (span[i] > 100)
                    {
                        sum += span[i];
                    }
                }
                stopwatch.Stop();
                Console.WriteLine($"Span<T>: {stopwatch.ElapsedMilliseconds} ms");
            }
            
            // Uso de structs para reducir allocations
            public struct ProductStruct
            {
                public int Id;
                public string Name;
                public decimal Price;
                public string Category;
                
                public ProductStruct(int id, string name, decimal price, string category)
                {
                    Id = id;
                    Name = name;
                    Price = price;
                    Category = category;
                }
            }
            
            public static void StructVsClass(IEnumerable<Product> products)
            {
                Console.WriteLine("=== Struct vs Class ===");
                
                // Usando class
                var stopwatch = Stopwatch.StartNew();
                var classResults = products
                    .Select(p => new { p.Name, p.Price })
                    .ToList();
                stopwatch.Stop();
                Console.WriteLine($"Class: {stopwatch.ElapsedMilliseconds} ms");
                
                // Usando struct (conceptual)
                stopwatch.Restart();
                var structResults = products
                    .Select(p => new ProductStruct(p.Id, p.Name, p.Price, p.Category))
                    .ToList();
                stopwatch.Stop();
                Console.WriteLine($"Struct: {stopwatch.ElapsedMilliseconds} ms");
            }
        }
    }
    
    // ===== HERRAMIENTAS DE PROFILING =====
    namespace ProfilingTools
    {
        public class ProfilingExamples
        {
            // Medición de tiempo con Stopwatch
            public static void MeasureQueryPerformance(IEnumerable<Product> products)
            {
                Console.WriteLine("=== Medición de Performance de Consultas ===");
                
                var queries = new Dictionary<string, Func<IEnumerable<Product>, object>>
                {
                    ["Filtrado simple"] = p => p.Where(prod => prod.IsActive).ToList(),
                    ["Filtrado y ordenamiento"] = p => p.Where(prod => prod.IsActive).OrderBy(prod => prod.Name).ToList(),
                    ["Agrupación"] = p => p.GroupBy(prod => prod.Category).ToList(),
                    ["Agregación"] = p => p.Where(prod => prod.IsActive).Average(prod => prod.Price),
                    ["Proyección compleja"] = p => p.Select(prod => new { prod.Name, prod.Price, prod.Category }).ToList()
                };
                
                foreach (var query in queries)
                {
                    var stopwatch = Stopwatch.StartNew();
                    var result = query.Value(products);
                    stopwatch.Stop();
                    
                    Console.WriteLine($"{query.Key}: {stopwatch.ElapsedMilliseconds} ms");
                }
            }
            
            // Medición de memoria
            public static void MeasureMemoryUsage(IEnumerable<Product> products)
            {
                Console.WriteLine("=== Medición de Uso de Memoria ===");
                
                var initialMemory = GC.GetTotalMemory(true);
                
                var result = products
                    .Where(p => p.IsActive)
                    .Select(p => new { p.Name, p.Price, p.Category })
                    .ToList();
                
                var finalMemory = GC.GetTotalMemory(true);
                var memoryUsed = finalMemory - initialMemory;
                
                Console.WriteLine($"Memoria utilizada: {memoryUsed:N0} bytes");
                Console.WriteLine($"Elementos procesados: {result.Count}");
            }
            
            // Análisis de complejidad
            public static void AnalyzeComplexity(IEnumerable<Product> products)
            {
                Console.WriteLine("=== Análisis de Complejidad ===");
                
                var sizes = new[] { 100, 1000, 10000 };
                
                foreach (var size in sizes)
                {
                    var testData = products.Take(size).ToList();
                    
                    var stopwatch = Stopwatch.StartNew();
                    var result = testData
                        .Where(p => p.IsActive)
                        .OrderBy(p => p.Name)
                        .Select(p => p.Name)
                        .ToList();
                    stopwatch.Stop();
                    
                    Console.WriteLine($"Tamaño {size}: {stopwatch.ElapsedMilliseconds} ms");
                }
            }
        }
    }
}

// Uso de LINQ y Performance
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== LINQ y Performance - Clase 7 ===\n");
        
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
        
        // Ejemplos de benchmarking
        Console.WriteLine("1. Benchmarking:");
        Benchmarking.PerformanceBenchmark.MeasureExecutionTime(
            () => products.Where(p => p.Price > 100).ToList(),
            "Filtrado de productos caros",
            100
        );
        
        // Ejemplos de problemas de performance
        Console.WriteLine("\n2. Problemas de Performance:");
        PerformanceIssues.CommonPerformanceIssues.NPlusOneProblem(customers.Take(10), orders);
        PerformanceIssues.CommonPerformanceIssues.OptimizedNPlusOne(customers.Take(10), orders);
        
        // Ejemplos de técnicas de optimización
        Console.WriteLine("\n3. Técnicas de Optimización:");
        OptimizationTechniques.OptimizationExamples.AvoidMultipleEnumerations(products);
        OptimizationTechniques.OptimizationExamples.AnyVsCount(products);
        
        // Ejemplos de paralelización
        Console.WriteLine("\n4. Paralelización:");
        Parallelization.ParallelExamples.BasicPLinq(products);
        
        // Ejemplos de cache
        Console.WriteLine("\n5. Cache y Memoización:");
        var cachedProducts = Caching.CachingExamples.GetExpensiveProductsCached(products, 100);
        Console.WriteLine($"Productos caros en cache: {cachedProducts.Count()}");
        
        // Ejemplos de profiling
        Console.WriteLine("\n6. Herramientas de Profiling:");
        ProfilingTools.ProfilingExamples.MeasureQueryPerformance(products.Take(1000));
        ProfilingTools.ProfilingExamples.MeasureMemoryUsage(products.Take(1000));
        
        Console.WriteLine("\n✅ LINQ y Performance comprendidos!");
        Console.WriteLine("Recuerda: 'Premature optimization is the root of all evil' - Donald Knuth");
        Console.WriteLine("Siempre mide antes de optimizar.");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Benchmarking
Crea benchmarks para comparar diferentes enfoques de consultas LINQ.

### Ejercicio 2: Optimización
Identifica y corrige problemas de performance en consultas LINQ.

### Ejercicio 3: Paralelización
Implementa PLINQ para mejorar el rendimiento de operaciones intensivas.

## 🔍 Puntos Clave

1. **Benchmarking** para medir el rendimiento de consultas LINQ
2. **Problemas comunes** como N+1 queries y evaluaciones múltiples
3. **Técnicas de optimización** como proyección temprana y cache
4. **Paralelización** con PLINQ para operaciones intensivas
5. **Cache y memoización** para evitar recálculos
6. **Optimización de memoria** con streaming y structs
7. **Herramientas de profiling** para análisis de rendimiento
8. **Principios de optimización** - medir antes de optimizar

## 📚 Recursos Adicionales

- [Microsoft Docs - LINQ Performance](https://docs.microsoft.com/en-us/dotnet/standard/linq/performance)
- [PLINQ Performance](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/performance-tips)

---

**🎯 ¡Has completado la Clase 7! Ahora comprendes LINQ y Performance**

**📚 [Siguiente: Clase 8 - Optimización de LINQ](clase_8_linq_optimization.md)**
