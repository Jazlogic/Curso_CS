# 🚀 Clase 6: Async Streams y IAsyncEnumerable

## 📋 Información de la Clase

- **Módulo**: Mid Level 2 - Arquitectura de Software y Patrones Avanzados
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 5 (Domain Driven Design)

## 🎯 Objetivos de Aprendizaje

- Dominar IAsyncEnumerable<T>
- Implementar streaming asíncrono
- Crear pipelines de procesamiento de datos
- Optimizar el consumo de memoria

---

## 📚 Navegación del Módulo 5

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_arquitectura_hexagonal.md) | Arquitectura Hexagonal (Ports & Adapters) | |
| [Clase 2](clase_2_event_sourcing.md) | Event Sourcing y CQRS Avanzado | |
| [Clase 3](clase_3_microservicios.md) | Arquitectura de Microservicios | |
| [Clase 4](clase_4_patrones_arquitectonicos.md) | Patrones Arquitectónicos | |
| [Clase 5](clase_5_domain_driven_design.md) | Domain Driven Design (DDD) | ← Anterior |
| **Clase 6** | **Async Streams y IAsyncEnumerable** | ← Estás aquí |
| [Clase 7](clase_7_source_generators.md) | Source Generators y Compile-time Code Generation | Siguiente → |
| [Clase 8](clase_8_high_performance.md) | High Performance Programming | |
| [Clase 9](clase_9_seguridad_avanzada.md) | Seguridad Avanzada en .NET | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Gestión Empresarial | |

**← [Volver al README del Módulo 5](../midLevel_2/README.md)**

---

## 📚 Contenido Teórico

### 1. Async Streams y IAsyncEnumerable

Los Async Streams permiten procesar secuencias de datos de manera asíncrona, optimizando el uso de memoria y mejorando el rendimiento.

```csharp
// ===== ASYNC STREAMS - IMPLEMENTACIÓN COMPLETA =====
namespace AsyncStreams
{
    // ===== GENERADORES ASÍNCRONOS BÁSICOS =====
    public class AsyncStreamGenerators
    {
        public static async IAsyncEnumerable<int> GenerateNumbersAsync(int count)
        {
            for (int i = 0; i < count; i++)
            {
                await Task.Delay(100); // Simular trabajo asíncrono
                yield return i;
            }
        }
        
        public static async IAsyncEnumerable<string> GenerateStringsAsync(int count)
        {
            var random = new Random();
            for (int i = 0; i < count; i++)
            {
                await Task.Delay(50);
                var randomString = GenerateRandomString(random, 10);
                yield return randomString;
            }
        }
        
        private static string GenerateRandomString(Random random, int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        
        public static async IAsyncEnumerable<DateTime> GenerateTimestampsAsync(int count)
        {
            var startTime = DateTime.UtcNow;
            for (int i = 0; i < count; i++)
            {
                await Task.Delay(200);
                yield return startTime.AddSeconds(i);
            }
        }
    }
    
    // ===== PROCESAMIENTO DE ARCHIVOS ASÍNCRONO =====
    public class AsyncFileProcessor
    {
        public static async IAsyncEnumerable<string> ReadLinesAsync(string filePath)
        {
            using var reader = new StreamReader(filePath);
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                yield return line;
            }
        }
        
        public static async IAsyncEnumerable<string> ReadLinesWithFilterAsync(string filePath, Func<string, bool> filter)
        {
            using var reader = new StreamReader(filePath);
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (filter(line))
                {
                    yield return line;
                }
            }
        }
        
        public static async IAsyncEnumerable<string> ReadLinesInBatchesAsync(string filePath, int batchSize)
        {
            var batch = new List<string>();
            using var reader = new StreamReader(filePath);
            string line;
            
            while ((line = await reader.ReadLineAsync()) != null)
            {
                batch.Add(line);
                
                if (batch.Count >= batchSize)
                {
                    foreach (var batchLine in batch)
                    {
                        yield return batchLine;
                    }
                    batch.Clear();
                }
            }
            
            // Procesar líneas restantes
            foreach (var remainingLine in batch)
            {
                yield return remainingLine;
            }
        }
    }
    
    // ===== PROCESAMIENTO DE BASE DE DATOS ASÍNCRONO =====
    public class AsyncDatabaseProcessor
    {
        public static async IAsyncEnumerable<User> GetUsersAsync(int pageSize = 100)
        {
            var offset = 0;
            var hasMore = true;
            
            while (hasMore)
            {
                var users = await GetUsersPageAsync(offset, pageSize);
                if (!users.Any())
                {
                    hasMore = false;
                }
                else
                {
                    foreach (var user in users)
                    {
                        yield return user;
                    }
                    offset += pageSize;
                }
            }
        }
        
        public static async IAsyncEnumerable<Order> GetUserOrdersAsync(int userId, int pageSize = 50)
        {
            var offset = 0;
            var hasMore = true;
            
            while (hasMore)
            {
                var orders = await GetUserOrdersPageAsync(userId, offset, pageSize);
                if (!orders.Any())
                {
                    hasMore = false;
                }
                else
                {
                    foreach (var order in orders)
                    {
                        yield return order;
                    }
                    offset += pageSize;
                }
            }
        }
        
        private static async Task<List<User>> GetUsersPageAsync(int offset, int pageSize)
        {
            // Simulación de consulta a base de datos
            await Task.Delay(100);
            var users = new List<User>();
            
            for (int i = 0; i < Math.Min(pageSize, 10); i++)
            {
                users.Add(new User
                {
                    Id = offset + i,
                    Name = $"User {offset + i}",
                    Email = $"user{offset + i}@example.com"
                });
            }
            
            return users;
        }
        
        private static async Task<List<Order>> GetUserOrdersPageAsync(int userId, int offset, int pageSize)
        {
            // Simulación de consulta a base de datos
            await Task.Delay(50);
            var orders = new List<Order>();
            
            for (int i = 0; i < Math.Min(pageSize, 5); i++)
            {
                orders.Add(new Order
                {
                    Id = offset + i,
                    UserId = userId,
                    Amount = (offset + i) * 10.0m,
                    OrderDate = DateTime.UtcNow.AddDays(-(offset + i))
                });
            }
            
            return orders;
        }
    }
    
    // ===== PIPELINES DE PROCESAMIENTO =====
    public class AsyncDataPipeline
    {
        public static async IAsyncEnumerable<TResult> ProcessPipelineAsync<TSource, TResult>(
            IAsyncEnumerable<TSource> source,
            Func<TSource, Task<TResult>> processor)
        {
            await foreach (var item in source)
            {
                var result = await processor(item);
                yield return result;
            }
        }
        
        public static async IAsyncEnumerable<TResult> ProcessPipelineWithFilterAsync<TSource, TResult>(
            IAsyncEnumerable<TSource> source,
            Func<TSource, bool> filter,
            Func<TSource, Task<TResult>> processor)
        {
            await foreach (var item in source)
            {
                if (filter(item))
                {
                    var result = await processor(item);
                    yield return result;
                }
            }
        }
        
        public static async IAsyncEnumerable<TResult> ProcessPipelineWithBatchAsync<TSource, TResult>(
            IAsyncEnumerable<TSource> source,
            int batchSize,
            Func<IEnumerable<TSource>, Task<IEnumerable<TResult>>> batchProcessor)
        {
            var batch = new List<TSource>();
            
            await foreach (var item in source)
            {
                batch.Add(item);
                
                if (batch.Count >= batchSize)
                {
                    var results = await batchProcessor(batch);
                    foreach (var result in results)
                    {
                        yield return result;
                    }
                    batch.Clear();
                }
            }
            
            // Procesar elementos restantes
            if (batch.Any())
            {
                var results = await batchProcessor(batch);
                foreach (var result in results)
                {
                    yield return result;
                }
            }
        }
    }
    
    // ===== TRANSFORMACIONES ASÍNCRONAS =====
    public class AsyncTransformations
    {
        public static async IAsyncEnumerable<TResult> SelectAsync<TSource, TResult>(
            IAsyncEnumerable<TSource> source,
            Func<TSource, Task<TResult>> selector)
        {
            await foreach (var item in source)
            {
                var result = await selector(item);
                yield return result;
            }
        }
        
        public static async IAsyncEnumerable<TSource> WhereAsync<TSource>(
            IAsyncEnumerable<TSource> source,
            Func<TSource, Task<bool>> predicate)
        {
            await foreach (var item in source)
            {
                if (await predicate(item))
                {
                    yield return item;
                }
            }
        }
        
        public static async IAsyncEnumerable<TResult> SelectManyAsync<TSource, TResult>(
            IAsyncEnumerable<TSource> source,
            Func<TSource, Task<IEnumerable<TResult>>> selector)
        {
            await foreach (var item in source)
            {
                var results = await selector(item);
                foreach (var result in results)
                {
                    yield return result;
                }
            }
        }
        
        public static async IAsyncEnumerable<TSource> TakeAsync<TSource>(
            IAsyncEnumerable<TSource> source,
            int count)
        {
            var taken = 0;
            await foreach (var item in source)
            {
                if (taken >= count)
                    break;
                
                yield return item;
                taken++;
            }
        }
        
        public static async IAsyncEnumerable<TSource> SkipAsync<TSource>(
            IAsyncEnumerable<TSource> source,
            int count)
        {
            var skipped = 0;
            await foreach (var item in source)
            {
                if (skipped < count)
                {
                    skipped++;
                    continue;
                }
                
                yield return item;
            }
        }
    }
    
    // ===== AGREGACIONES ASÍNCRONAS =====
    public class AsyncAggregations
    {
        public static async Task<TResult> AggregateAsync<TSource, TResult>(
            IAsyncEnumerable<TSource> source,
            TResult seed,
            Func<TResult, TSource, Task<TResult>> func)
        {
            var result = seed;
            await foreach (var item in source)
            {
                result = await func(result, item);
            }
            return result;
        }
        
        public static async Task<TResult> AggregateAsync<TSource, TResult>(
            IAsyncEnumerable<TSource> source,
            TResult seed,
            Func<TResult, TSource, Task<TResult>> func,
            Func<TResult, Task<TResult>> resultSelector)
        {
            var result = await AggregateAsync(source, seed, func);
            return await resultSelector(result);
        }
        
        public static async Task<TSource> FirstAsync<TSource>(
            IAsyncEnumerable<TSource> source)
        {
            await foreach (var item in source)
            {
                return item;
            }
            throw new InvalidOperationException("Sequence contains no elements");
        }
        
        public static async Task<TSource> FirstOrDefaultAsync<TSource>(
            IAsyncEnumerable<TSource> source)
        {
            await foreach (var item in source)
            {
                return item;
            }
            return default(TSource);
        }
        
        public static async Task<TSource> FirstOrDefaultAsync<TSource>(
            IAsyncEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            await foreach (var item in source)
            {
                if (predicate(item))
                    return item;
            }
            return default(TSource);
        }
        
        public static async Task<bool> AnyAsync<TSource>(
            IAsyncEnumerable<TSource> source)
        {
            await foreach (var item in source)
            {
                return true;
            }
            return false;
        }
        
        public static async Task<bool> AnyAsync<TSource>(
            IAsyncEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            await foreach (var item in source)
            {
                if (predicate(item))
                    return true;
            }
            return false;
        }
        
        public static async Task<int> CountAsync<TSource>(
            IAsyncEnumerable<TSource> source)
        {
            var count = 0;
            await foreach (var item in source)
            {
                count++;
            }
            return count;
        }
        
        public static async Task<int> CountAsync<TSource>(
            IAsyncEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            var count = 0;
            await foreach (var item in source)
            {
                if (predicate(item))
                    count++;
            }
            return count;
        }
    }
    
    // ===== CONVERSIONES ASÍNCRONAS =====
    public class AsyncConversions
    {
        public static async Task<List<T>> ToListAsync<T>(IAsyncEnumerable<T> source)
        {
            var list = new List<T>();
            await foreach (var item in source)
            {
                list.Add(item);
            }
            return list;
        }
        
        public static async Task<T[]> ToArrayAsync<T>(IAsyncEnumerable<T> source)
        {
            var list = await ToListAsync(source);
            return list.ToArray();
        }
        
        public static async Task<Dictionary<TKey, TValue>> ToDictionaryAsync<T, TKey, TValue>(
            IAsyncEnumerable<T> source,
            Func<T, TKey> keySelector,
            Func<T, TValue> valueSelector)
        {
            var dictionary = new Dictionary<TKey, TValue>();
            await foreach (var item in source)
            {
                var key = keySelector(item);
                var value = valueSelector(item);
                dictionary[key] = value;
            }
            return dictionary;
        }
        
        public static async Task<ILookup<TKey, T>> ToLookupAsync<T, TKey>(
            IAsyncEnumerable<T> source,
            Func<T, TKey> keySelector)
        {
            var groups = new Dictionary<TKey, List<T>>();
            await foreach (var item in source)
            {
                var key = keySelector(item);
                if (!groups.ContainsKey(key))
                    groups[key] = new List<T>();
                groups[key].Add(item);
            }
            
            return groups.ToLookup(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
    
    // ===== EJEMPLOS PRÁCTICOS =====
    public class AsyncStreamExamples
    {
        public static async Task ProcessUserDataAsync()
        {
            Console.WriteLine("=== Procesamiento de Datos de Usuario ===");
            
            var userStream = AsyncDatabaseProcessor.GetUsersAsync(50);
            var processedUsers = userStream
                .WhereAsync(async user => await IsUserActiveAsync(user))
                .SelectAsync(async user => await EnrichUserDataAsync(user));
            
            await foreach (var user in processedUsers)
            {
                Console.WriteLine($"Usuario procesado: {user.Name} - {user.Email}");
            }
        }
        
        public static async Task ProcessFileDataAsync()
        {
            Console.WriteLine("=== Procesamiento de Archivo ===");
            
            var filePath = "sample.txt";
            var lines = AsyncFileProcessor.ReadLinesAsync(filePath);
            var processedLines = lines
                .WhereAsync(async line => await ValidateLineAsync(line))
                .SelectAsync(async line => await ProcessLineAsync(line));
            
            var results = new List<string>();
            await foreach (var line in processedLines)
            {
                results.Add(line);
            }
            
            Console.WriteLine($"Líneas procesadas: {results.Count}");
        }
        
        public static async Task ProcessOrderPipelineAsync()
        {
            Console.WriteLine("=== Pipeline de Órdenes ===");
            
            var orderStream = AsyncDatabaseProcessor.GetUserOrdersAsync(1, 100);
            var pipeline = AsyncDataPipeline.ProcessPipelineWithFilterAsync(
                orderStream,
                order => order.Amount > 50.0m,
                async order => await ProcessOrderAsync(order));
            
            var totalAmount = await AsyncAggregations.AggregateAsync(
                pipeline,
                0.0m,
                async (sum, order) => sum + order.ProcessedAmount);
            
            Console.WriteLine($"Total procesado: {totalAmount:C}");
        }
        
        private static async Task<bool> IsUserActiveAsync(User user)
        {
            await Task.Delay(10);
            return user.Id % 2 == 0; // Simulación
        }
        
        private static async Task<User> EnrichUserDataAsync(User user)
        {
            await Task.Delay(20);
            user.Email = $"{user.Name.ToLower()}@enriched.com";
            return user;
        }
        
        private static async Task<bool> ValidateLineAsync(string line)
        {
            await Task.Delay(5);
            return !string.IsNullOrWhiteSpace(line) && line.Length > 5;
        }
        
        private static async Task<string> ProcessLineAsync(string line)
        {
            await Task.Delay(10);
            return line.ToUpper();
        }
        
        private static async Task<ProcessedOrder> ProcessOrderAsync(Order order)
        {
            await Task.Delay(50);
            return new ProcessedOrder
            {
                OrderId = order.Id,
                ProcessedAmount = order.Amount * 1.1m,
                ProcessedDate = DateTime.UtcNow
            };
        }
    }
    
    // ===== MODELOS DE DATOS =====
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
    
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public DateTime OrderDate { get; set; }
    }
    
    public class ProcessedOrder
    {
        public int OrderId { get; set; }
        public decimal ProcessedAmount { get; set; }
        public DateTime ProcessedDate { get; set; }
    }
}

// Uso de Async Streams
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Async Streams y IAsyncEnumerable ===\n");
        
        Console.WriteLine("Los Async Streams proporcionan:");
        Console.WriteLine("1. Procesamiento asíncrono de secuencias");
        Console.WriteLine("2. Uso eficiente de memoria");
        Console.WriteLine("3. Pipelines de procesamiento de datos");
        Console.WriteLine("4. Transformaciones y filtros asíncronos");
        Console.WriteLine("5. Agregaciones y conversiones asíncronas");
        
        Console.WriteLine("\nEjemplos implementados:");
        Console.WriteLine("- Generadores de números y strings");
        Console.WriteLine("- Procesamiento de archivos");
        Console.WriteLine("- Consultas a base de datos");
        Console.WriteLine("- Pipelines de transformación");
        Console.WriteLine("- Agregaciones y conversiones");
        
        // Ejecutar ejemplos
        await AsyncStreams.AsyncStreamExamples.ProcessUserDataAsync();
        await AsyncStreams.AsyncStreamExamples.ProcessOrderPipelineAsync();
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Generador de Datos
Implementa un generador asíncrono que produzca datos de sensores en tiempo real.

### Ejercicio 2: Pipeline de Procesamiento
Crea un pipeline que procese logs de aplicación con filtros y transformaciones.

### Ejercicio 3: Agregación de Datos
Implementa agregaciones asíncronas para análisis de métricas en tiempo real.

## 🔍 Puntos Clave

1. **IAsyncEnumerable<T>** permite iteración asíncrona
2. **yield return** en métodos async genera streams
3. **await foreach** consume streams asíncronos
4. **Pipelines** permiten transformaciones en cadena
5. **Agregaciones** procesan streams de manera eficiente

## 📚 Recursos Adicionales

- [Async Streams - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-8#async-streams)
- [IAsyncEnumerable Interface](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.iasyncenumerable-1)
- [Async Streams Tutorial](https://docs.microsoft.com/en-us/dotnet/csharp/tutorials/generics/)

---

**🎯 ¡Has completado la Clase 6! Ahora comprendes Async Streams y IAsyncEnumerable en C#**

**📚 [Siguiente: Clase 7 - Source Generators y Compile-time Code Generation](clase_7_source_generators.md)**
