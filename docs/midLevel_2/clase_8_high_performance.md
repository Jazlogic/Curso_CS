# 🚀 Clase 8: High Performance Programming

## 📋 Información de la Clase

- **Módulo**: Mid Level 2 - Arquitectura de Software y Patrones Avanzados
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 7 (Source Generators)

## 🎯 Objetivos de Aprendizaje

- Optimizar el rendimiento del código C#
- Implementar técnicas de memoria eficiente
- Usar Span<T> y Memory<T>
- Aplicar paralelización avanzada

---

## 📚 Navegación del Módulo 5

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_arquitectura_hexagonal.md) | Arquitectura Hexagonal (Ports & Adapters) | |
| [Clase 2](clase_2_event_sourcing.md) | Event Sourcing y CQRS Avanzado | |
| [Clase 3](clase_3_microservicios.md) | Arquitectura de Microservicios | |
| [Clase 4](clase_4_patrones_arquitectonicos.md) | Patrones Arquitectónicos | |
| [Clase 5](clase_5_domain_driven_design.md) | Domain Driven Design (DDD) | |
| [Clase 6](clase_6_async_streams.md) | Async Streams y IAsyncEnumerable | |
| [Clase 7](clase_7_source_generators.md) | Source Generators y Compile-time Code Generation | ← Anterior |
| **Clase 8** | **High Performance Programming** | ← Estás aquí |
| [Clase 9](clase_9_seguridad_avanzada.md) | Seguridad Avanzada en .NET | Siguiente → |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Gestión Empresarial | |

**← [Volver al README del Módulo 5](../midLevel_2/README.md)**

---

## 📚 Contenido Teórico

### 1. High Performance Programming

La programación de alto rendimiento se enfoca en optimizar el código para máxima velocidad y eficiencia de memoria.

```csharp
// ===== HIGH PERFORMANCE PROGRAMMING - IMPLEMENTACIÓN COMPLETA =====
namespace HighPerformanceProgramming
{
    // ===== SPAN<T> Y MEMORY<T> =====
    public class SpanMemoryExamples
    {
        public static int SumArrayWithSpan(int[] array)
        {
            var span = array.AsSpan();
            var sum = 0;
            
            for (int i = 0; i < span.Length; i++)
            {
                sum += span[i];
            }
            
            return sum;
        }
        
        public static int SumArrayWithSpanOptimized(int[] array)
        {
            var span = array.AsSpan();
            var sum = 0;
            
            // Procesar en bloques de 4 para mejor rendimiento
            var length = span.Length;
            var blockSize = 4;
            var i = 0;
            
            for (; i <= length - blockSize; i += blockSize)
            {
                sum += span[i] + span[i + 1] + span[i + 2] + span[i + 3];
            }
            
            // Procesar elementos restantes
            for (; i < length; i++)
            {
                sum += span[i];
            }
            
            return sum;
        }
        
        public static void ProcessStringWithSpan(string input)
        {
            var span = input.AsSpan();
            
            // Procesar caracteres sin asignar memoria
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] == ' ')
                {
                    // Procesar espacio
                }
            }
        }
        
        public static void ProcessMemoryWithMemory<T>(Memory<T> memory) where T : struct
        {
            var span = memory.Span;
            
            for (int i = 0; i < span.Length; i++)
            {
                // Procesar elemento
                var element = span[i];
            }
        }
    }
    
    // ===== POOLING DE OBJETOS =====
    public class ObjectPooling
    {
        private static readonly ObjectPool<StringBuilder> _stringBuilderPool = 
            new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());
        
        public static string BuildStringWithPooling(IEnumerable<string> parts)
        {
            var sb = _stringBuilderPool.Get();
            
            try
            {
                foreach (var part in parts)
                {
                    sb.Append(part);
                }
                
                return sb.ToString();
            }
            finally
            {
                _stringBuilderPool.Return(sb);
            }
        }
        
        public static void ProcessDataWithPooling<T>(IEnumerable<T> data, Action<T> processor)
        {
            var buffer = ArrayPool<T>.Shared.Rent(1000);
            
            try
            {
                var index = 0;
                foreach (var item in data)
                {
                    buffer[index++] = item;
                    
                    if (index >= buffer.Length)
                    {
                        ProcessBuffer(buffer, index, processor);
                        index = 0;
                    }
                }
                
                if (index > 0)
                {
                    ProcessBuffer(buffer, index, processor);
                }
            }
            finally
            {
                ArrayPool<T>.Shared.Return(buffer);
            }
        }
        
        private static void ProcessBuffer<T>(T[] buffer, int count, Action<T> processor)
        {
            for (int i = 0; i < count; i++)
            {
                processor(buffer[i]);
            }
        }
    }
    
    // ===== STRUCTS Y VALUE TYPES =====
    public struct HighPerformancePoint
    {
        public float X, Y, Z;
        
        public HighPerformancePoint(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        
        public float DistanceTo(in HighPerformancePoint other)
        {
            var dx = X - other.X;
            var dy = Y - other.Y;
            var dz = Z - other.Z;
            
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
        
        public static HighPerformancePoint operator +(in HighPerformancePoint a, in HighPerformancePoint b)
        {
            return new HighPerformancePoint(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }
        
        public static HighPerformancePoint operator -(in HighPerformancePoint a, in HighPerformancePoint b)
        {
            return new HighPerformancePoint(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
    }
    
    public struct HighPerformanceVector
    {
        public float X, Y, Z, W;
        
        public HighPerformanceVector(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        
        public float Magnitude => (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
        
        public void Normalize()
        {
            var mag = Magnitude;
            if (mag > 0)
            {
                X /= mag;
                Y /= mag;
                Z /= mag;
                W /= mag;
            }
        }
        
        public static HighPerformanceVector operator *(in HighPerformanceVector v, float scalar)
        {
            return new HighPerformanceVector(v.X * scalar, v.Y * scalar, v.Z * scalar, v.W * scalar);
        }
    }
    
    // ===== ALGORITMOS OPTIMIZADOS =====
    public class OptimizedAlgorithms
    {
        public static void QuickSortOptimized<T>(Span<T> span) where T : IComparable<T>
        {
            if (span.Length <= 1) return;
            
            var pivot = span[span.Length / 2];
            var left = 0;
            var right = span.Length - 1;
            
            while (left <= right)
            {
                while (span[left].CompareTo(pivot) < 0) left++;
                while (span[right].CompareTo(pivot) > 0) right--;
                
                if (left <= right)
                {
                    var temp = span[left];
                    span[left] = span[right];
                    span[right] = temp;
                    left++;
                    right--;
                }
            }
            
            if (right > 0) QuickSortOptimized(span.Slice(0, right + 1));
            if (left < span.Length) QuickSortOptimized(span.Slice(left));
        }
        
        public static int BinarySearchOptimized<T>(ReadOnlySpan<T> span, T value) where T : IComparable<T>
        {
            var left = 0;
            var right = span.Length - 1;
            
            while (left <= right)
            {
                var mid = left + (right - left) / 2;
                var comparison = span[mid].CompareTo(value);
                
                if (comparison == 0)
                    return mid;
                
                if (comparison < 0)
                    left = mid + 1;
                else
                    right = mid - 1;
            }
            
            return -1;
        }
        
        public static void BubbleSortOptimized<T>(Span<T> span) where T : IComparable<T>
        {
            var n = span.Length;
            var swapped = true;
            
            while (swapped)
            {
                swapped = false;
                for (int i = 1; i < n; i++)
                {
                    if (span[i - 1].CompareTo(span[i]) > 0)
                    {
                        var temp = span[i - 1];
                        span[i - 1] = span[i];
                        span[i] = temp;
                        swapped = true;
                    }
                }
                n--;
            }
        }
    }
    
    // ===== PARALELIZACIÓN AVANZADA =====
    public class AdvancedParallelization
    {
        public static void ParallelForOptimized(int fromInclusive, int toExclusive, Action<int> body)
        {
            var processorCount = Environment.ProcessorCount;
            var chunkSize = Math.Max(1, (toExclusive - fromInclusive) / processorCount);
            
            var tasks = new Task[processorCount];
            
            for (int i = 0; i < processorCount; i++)
            {
                var start = fromInclusive + i * chunkSize;
                var end = i == processorCount - 1 ? toExclusive : start + chunkSize;
                
                tasks[i] = Task.Run(() =>
                {
                    for (int j = start; j < end; j++)
                    {
                        body(j);
                    }
                });
            }
            
            Task.WaitAll(tasks);
        }
        
        public static void ParallelForEachOptimized<T>(IEnumerable<T> source, Action<T> body, int maxDegreeOfParallelism = -1)
        {
            if (maxDegreeOfParallelism == -1)
                maxDegreeOfParallelism = Environment.ProcessorCount;
            
            var partitioner = Partitioner.Create(source, true);
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            };
            
            Parallel.ForEach(partitioner, parallelOptions, body);
        }
        
        public static async Task<T[]> ParallelMapAsync<T, TResult>(IEnumerable<T> source, Func<T, Task<TResult>> selector)
        {
            var tasks = source.Select(selector).ToArray();
            return await Task.WhenAll(tasks);
        }
        
        public static async Task<TResult> ParallelReduceAsync<T, TResult>(
            IEnumerable<T> source, 
            TResult seed, 
            Func<TResult, T, Task<TResult>> reducer)
        {
            var tasks = new List<Task<TResult>>();
            var items = source.ToArray();
            var chunkSize = Math.Max(1, items.Length / Environment.ProcessorCount);
            
            for (int i = 0; i < items.Length; i += chunkSize)
            {
                var chunk = items.Skip(i).Take(chunkSize);
                var task = Task.Run(async () =>
                {
                    var result = seed;
                    foreach (var item in chunk)
                    {
                        result = await reducer(result, item);
                    }
                    return result;
                });
                
                tasks.Add(task);
            }
            
            var results = await Task.WhenAll(tasks);
            var finalResult = seed;
            
            foreach (var result in results)
            {
                finalResult = await reducer(finalResult, result);
            }
            
            return finalResult;
        }
    }
    
    // ===== MEMORY MANAGEMENT =====
    public class MemoryManagement
    {
        public static void ProcessLargeFileWithMemoryMapping(string filePath)
        {
            using var fileStream = File.OpenRead(filePath);
            using var memoryMappedFile = MemoryMappedFile.CreateFromFile(fileStream, null, 0, MemoryMappedFileAccess.Read, HandleInheritability.None, false);
            
            using var accessor = memoryMappedFile.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
            var length = accessor.Capacity;
            
            // Procesar archivo en chunks
            var chunkSize = 1024 * 1024; // 1MB
            var position = 0L;
            
            while (position < length)
            {
                var currentChunkSize = (int)Math.Min(chunkSize, length - position);
                var buffer = new byte[currentChunkSize];
                
                accessor.ReadArray(position, buffer, 0, currentChunkSize);
                ProcessChunk(buffer);
                
                position += currentChunkSize;
            }
        }
        
        private static void ProcessChunk(byte[] chunk)
        {
            // Procesar chunk de datos
            for (int i = 0; i < chunk.Length; i++)
            {
                // Procesar byte
                var b = chunk[i];
            }
        }
        
        public static void ProcessDataWithStackalloc<T>(int count) where T : unmanaged
        {
            if (count <= 0) return;
            
            // Usar stackalloc para arrays pequeños
            if (count <= 1024)
            {
                Span<T> buffer = stackalloc T[count];
                ProcessBuffer(buffer);
            }
            else
            {
                // Usar ArrayPool para arrays grandes
                var buffer = ArrayPool<T>.Shared.Rent(count);
                try
                {
                    var span = buffer.AsSpan(0, count);
                    ProcessBuffer(span);
                }
                finally
                {
                    ArrayPool<T>.Shared.Return(buffer);
                }
            }
        }
        
        private static void ProcessBuffer<T>(Span<T> buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                // Procesar elemento
                var element = buffer[i];
            }
        }
    }
    
    // ===== CACHING Y OPTIMIZACIÓN =====
    public class CachingAndOptimization
    {
        private static readonly ConcurrentDictionary<string, object> _cache = new();
        private static readonly MemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());
        
        public static T GetOrCompute<T>(string key, Func<T> factory)
        {
            if (_cache.TryGetValue(key, out var cached))
            {
                return (T)cached;
            }
            
            var value = factory();
            _cache.TryAdd(key, value);
            return value;
        }
        
        public static async Task<T> GetOrComputeAsync<T>(string key, Func<Task<T>> factory)
        {
            if (_memoryCache.TryGetValue(key, out T cached))
            {
                return cached;
            }
            
            var value = await factory();
            _memoryCache.Set(key, value, TimeSpan.FromMinutes(10));
            return value;
        }
        
        public static void PrecomputeValues<T>(IEnumerable<string> keys, Func<string, T> factory)
        {
            var tasks = keys.Select(key => Task.Run(() =>
            {
                var value = factory(key);
                _cache.TryAdd(key, value);
            }));
            
            Task.WaitAll(tasks.ToArray());
        }
    }
    
    // ===== BENCHMARKING Y PROFILING =====
    public class Benchmarking
    {
        public static TimeSpan MeasureExecutionTime(Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
        
        public static async Task<TimeSpan> MeasureExecutionTimeAsync(Func<Task> action)
        {
            var stopwatch = Stopwatch.StartNew();
            await action();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
        
        public static T MeasureExecutionTime<T>(Func<T> func, out TimeSpan executionTime)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = func();
            stopwatch.Stop();
            executionTime = stopwatch.Elapsed;
            return result;
        }
        
        public static async Task<T> MeasureExecutionTimeAsync<T>(Func<Task<T>> func, out TimeSpan executionTime)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = await func();
            stopwatch.Stop();
            executionTime = stopwatch.Elapsed;
            return result;
        }
        
        public static void RunBenchmark(string name, Action action, int iterations = 1000)
        {
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                action();
            }
            
            stopwatch.Stop();
            var averageTime = stopwatch.Elapsed.TotalMilliseconds / iterations;
            
            Console.WriteLine($"{name}: {averageTime:F4} ms average over {iterations} iterations");
        }
    }
    
    // ===== EJEMPLOS PRÁCTICOS =====
    public class HighPerformanceExamples
    {
        public static void DemonstrateSpanPerformance()
        {
            var array = Enumerable.Range(0, 1000000).ToArray();
            
            var stopwatch = Stopwatch.StartNew();
            var sum1 = array.Sum();
            stopwatch.Stop();
            var time1 = stopwatch.Elapsed;
            
            stopwatch.Restart();
            var sum2 = SpanMemoryExamples.SumArrayWithSpan(array);
            stopwatch.Stop();
            var time2 = stopwatch.Elapsed;
            
            stopwatch.Restart();
            var sum3 = SpanMemoryExamples.SumArrayWithSpanOptimized(array);
            stopwatch.Stop();
            var time3 = stopwatch.Elapsed;
            
            Console.WriteLine($"LINQ Sum: {sum1} in {time1.TotalMilliseconds:F2} ms");
            Console.WriteLine($"Span Sum: {sum2} in {time2.TotalMilliseconds:F2} ms");
            Console.WriteLine($"Optimized Span: {sum3} in {time3.TotalMilliseconds:F2} ms");
        }
        
        public static void DemonstrateObjectPooling()
        {
            var parts = Enumerable.Range(0, 10000).Select(i => i.ToString()).ToArray();
            
            var stopwatch = Stopwatch.StartNew();
            var result1 = string.Join("", parts);
            stopwatch.Stop();
            var time1 = stopwatch.Elapsed;
            
            stopwatch.Restart();
            var result2 = ObjectPooling.BuildStringWithPooling(parts);
            stopwatch.Stop();
            var time2 = stopwatch.Elapsed;
            
            Console.WriteLine($"String.Join: {time1.TotalMilliseconds:F2} ms");
            Console.WriteLine($"Object Pooling: {time2.TotalMilliseconds:F2} ms");
        }
        
        public static void DemonstrateParallelization()
        {
            var data = Enumerable.Range(0, 10000000).ToArray();
            
            var stopwatch = Stopwatch.StartNew();
            var sum1 = data.Sum();
            stopwatch.Stop();
            var time1 = stopwatch.Elapsed;
            
            stopwatch.Restart();
            var sum2 = 0;
            AdvancedParallelization.ParallelForOptimized(0, data.Length, i => sum2 += data[i]);
            stopwatch.Stop();
            var time2 = stopwatch.Elapsed;
            
            Console.WriteLine($"Sequential Sum: {sum1} in {time1.TotalMilliseconds:F2} ms");
            Console.WriteLine($"Parallel Sum: {sum2} in {time2.TotalMilliseconds:F2} ms");
        }
    }
}

// Uso de High Performance Programming
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== High Performance Programming ===\n");
        
        Console.WriteLine("Las técnicas de alto rendimiento incluyen:");
        Console.WriteLine("1. Span<T> y Memory<T> para acceso eficiente a memoria");
        Console.WriteLine("2. Object Pooling para reducir asignaciones");
        Console.WriteLine("3. Structs y Value Types para evitar boxing");
        Console.WriteLine("4. Algoritmos optimizados y paralelización");
        Console.WriteLine("5. Gestión eficiente de memoria y caching");
        
        Console.WriteLine("\nEjemplos implementados:");
        Console.WriteLine("- Optimización de arrays con Span<T>");
        Console.WriteLine("- Pooling de objetos para StringBuilder");
        Console.WriteLine("- Structs de alto rendimiento para matemáticas");
        Console.WriteLine("- Algoritmos de ordenamiento optimizados");
        Console.WriteLine("- Paralelización avanzada con TPL");
        Console.WriteLine("- Gestión de memoria con MemoryMappedFile");
        Console.WriteLine("- Caching y benchmarking");
        
        // Ejecutar ejemplos
        HighPerformanceProgramming.HighPerformanceExamples.DemonstrateSpanPerformance();
        HighPerformanceProgramming.HighPerformanceExamples.DemonstrateObjectPooling();
        HighPerformanceProgramming.HighPerformanceExamples.DemonstrateParallelization();
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Optimización de Algoritmos
Implementa versiones optimizadas de algoritmos comunes usando Span<T> y técnicas de alto rendimiento.

### Ejercicio 2: Sistema de Caching
Crea un sistema de caching de múltiples niveles con diferentes estrategias de evicción.

### Ejercicio 3: Paralelización Personalizada
Implementa un sistema de paralelización personalizado para algoritmos específicos.

## 🔍 Puntos Clave

1. **Span<T> y Memory<T>** evitan asignaciones de memoria
2. **Object Pooling** reduce la presión del garbage collector
3. **Structs** evitan boxing y mejoran el rendimiento
4. **Paralelización** aprovecha múltiples núcleos
5. **Memory Management** optimiza el uso de memoria

## 📚 Recursos Adicionales

- [Performance Best Practices - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/standard/performance/)
- [Span<T> and Memory<T>](https://docs.microsoft.com/en-us/dotnet/standard/memory-and-spans/)
- [Object Pooling](https://docs.microsoft.com/en-us/dotnet/standard/collections/thread-safe/)

---

**🎯 ¡Has completado la Clase 8! Ahora comprendes High Performance Programming en C#**

**📚 [Siguiente: Clase 9 - Seguridad Avanzada en .NET](clase_9_seguridad_avanzada.md)**
