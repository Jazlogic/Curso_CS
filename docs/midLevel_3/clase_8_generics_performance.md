# 🚀 Clase 8: Generics y Performance

## 📋 Información de la Clase

- **Módulo**: Mid Level 3 - Manejo de Excepciones y Generics
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 7 (Generics y Reflection)

## 🎯 Objetivos de Aprendizaje

- Comprender el impacto de generics en el performance
- Implementar técnicas de optimización para generics
- Usar benchmarking para medir performance
- Aplicar mejores prácticas para generics de alto rendimiento

---

## 📚 Navegación del Módulo 3

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_manejo_excepciones.md) | Manejo de Excepciones | |
| [Clase 2](clase_2_generics_basicos.md) | Generics Básicos | |
| [Clase 3](clase_3_generics_avanzados.md) | Generics Avanzados | |
| [Clase 4](clase_4_colecciones_genericas.md) | Colecciones Genéricas | |
| [Clase 5](clase_5_interfaces_genericas.md) | Interfaces Genéricas | |
| [Clase 6](clase_6_restricciones_generics.md) | Restricciones de Generics | |
| [Clase 7](clase_7_generics_reflection.md) | Generics y Reflection | ← Anterior |
| **Clase 8** | **Generics y Performance** | ← Estás aquí |
| [Clase 9](clase_9_patrones_generics.md) | Patrones con Generics | Siguiente → |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Biblioteca | |

**← [Volver al README del Módulo 3](../midLevel_3/README.md)**

---

## 📚 Contenido Teórico

### 1. Generics y Performance

Los generics tienen un impacto significativo en el performance, tanto positivo como negativo. Es crucial entender cómo optimizarlos para aplicaciones de alto rendimiento.

```csharp
// ===== GENERICS Y PERFORMANCE - IMPLEMENTACIÓN COMPLETA =====
namespace GenericsAndPerformance
{
    // ===== BENCHMARKING DE GENERICS =====
    namespace Benchmarking
    {
        public class GenericPerformanceBenchmark
        {
            private readonly int _iterations;
            private readonly Stopwatch _stopwatch;
            
            public GenericPerformanceBenchmark(int iterations = 1000000)
            {
                _iterations = iterations;
                _stopwatch = new Stopwatch();
            }
            
            // Benchmark de creación de instancias genéricas
            public BenchmarkResult BenchmarkGenericCreation<T>() where T : class, new()
            {
                _stopwatch.Restart();
                
                for (int i = 0; i < _iterations; i++)
                {
                    var instance = new T();
                }
                
                _stopwatch.Stop();
                
                return new BenchmarkResult
                {
                    Operation = $"Generic Creation<{typeof(T).Name}>",
                    Iterations = _iterations,
                    ElapsedTime = _stopwatch.Elapsed,
                    AverageTimePerOperation = _stopwatch.Elapsed.TotalMilliseconds / _iterations
                };
            }
            
            // Benchmark de métodos genéricos
            public BenchmarkResult BenchmarkGenericMethod<T>(Func<T, T, T> operation, T value1, T value2)
            {
                _stopwatch.Restart();
                
                for (int i = 0; i < _iterations; i++)
                {
                    var result = operation(value1, value2);
                }
                
                _stopwatch.Stop();
                
                return new BenchmarkResult
                {
                    Operation = $"Generic Method<{typeof(T).Name}>",
                    Iterations = _iterations,
                    ElapsedTime = _stopwatch.Elapsed,
                    AverageTimePerOperation = _stopwatch.Elapsed.TotalMilliseconds / _iterations
                };
            }
            
            // Benchmark de colecciones genéricas
            public BenchmarkResult BenchmarkGenericCollection<T>(IEnumerable<T> collection, Action<T> operation)
            {
                _stopwatch.Restart();
                
                for (int i = 0; i < _iterations; i++)
                {
                    foreach (var item in collection)
                    {
                        operation(item);
                    }
                }
                
                _stopwatch.Stop();
                
                return new BenchmarkResult
                {
                    Operation = $"Generic Collection<{typeof(T).Name}>",
                    Iterations = _iterations,
                    ElapsedTime = _stopwatch.Elapsed,
                    AverageTimePerOperation = _stopwatch.Elapsed.TotalMilliseconds / _iterations
                };
            }
            
            // Benchmark comparativo entre generics y no-generics
            public BenchmarkComparisonResult CompareGenericVsNonGeneric<T>(Func<T, T, T> genericOperation, 
                Func<object, object, object> nonGenericOperation, T value1, T value2)
            {
                var genericResult = BenchmarkGenericMethod(genericOperation, value1, value2);
                var nonGenericResult = BenchmarkGenericMethod((a, b) => nonGenericOperation(a, b), value1, value2);
                
                return new BenchmarkComparisonResult
                {
                    GenericResult = genericResult,
                    NonGenericResult = nonGenericResult,
                    PerformanceImprovement = (nonGenericResult.AverageTimePerOperation - genericResult.AverageTimePerOperation) / nonGenericResult.AverageTimePerOperation * 100
                };
            }
        }
        
        public class BenchmarkResult
        {
            public string Operation { get; set; }
            public int Iterations { get; set; }
            public TimeSpan ElapsedTime { get; set; }
            public double AverageTimePerOperation { get; set; }
            
            public override string ToString()
            {
                return $"{Operation}: {Iterations:N0} iterations in {ElapsedTime.TotalMilliseconds:F2}ms " +
                       $"(avg: {AverageTimePerOperation:F6}ms per operation)";
            }
        }
        
        public class BenchmarkComparisonResult
        {
            public BenchmarkResult GenericResult { get; set; }
            public BenchmarkResult NonGenericResult { get; set; }
            public double PerformanceImprovement { get; set; }
            
            public override string ToString()
            {
                return $"Generic: {GenericResult}\n" +
                       $"Non-Generic: {NonGenericResult}\n" +
                       $"Performance Improvement: {PerformanceImprovement:F2}%";
            }
        }
    }
    
    // ===== OPTIMIZACIÓN DE GENERICS =====
    namespace Optimization
    {
        // Pool de objetos genérico para reducir allocations
        public class GenericObjectPool<T> where T : class, new()
        {
            private readonly ConcurrentQueue<T> _pool;
            private readonly int _maxPoolSize;
            private int _currentPoolSize;
            
            public GenericObjectPool(int maxPoolSize = 100)
            {
                _pool = new ConcurrentQueue<T>();
                _maxPoolSize = maxPoolSize;
                _currentPoolSize = 0;
            }
            
            public T Get()
            {
                if (_pool.TryDequeue(out T item))
                {
                    Interlocked.Decrement(ref _currentPoolSize);
                    return item;
                }
                
                return new T();
            }
            
            public void Return(T item)
            {
                if (item == null) return;
                
                if (_currentPoolSize < _maxPoolSize)
                {
                    _pool.Enqueue(item);
                    Interlocked.Increment(ref _currentPoolSize);
                }
            }
            
            public void Clear()
            {
                while (_pool.TryDequeue(out _))
                {
                    Interlocked.Decrement(ref _currentPoolSize);
                }
            }
        }
        
        // Cache genérico optimizado con LRU
        public class OptimizedGenericCache<TKey, TValue>
        {
            private readonly ConcurrentDictionary<TKey, CacheItem<TValue>> _cache;
            private readonly int _maxSize;
            private readonly object _lockObject = new object();
            
            public OptimizedGenericCache(int maxSize = 1000)
            {
                _cache = new ConcurrentDictionary<TKey, CacheItem<TValue>>();
                _maxSize = maxSize;
            }
            
            public bool TryGet(TKey key, out TValue value)
            {
                if (_cache.TryGetValue(key, out var item))
                {
                    item.LastAccessed = DateTime.UtcNow;
                    item.AccessCount++;
                    value = item.Value;
                    return true;
                }
                
                value = default(TValue);
                return false;
            }
            
            public void Set(TKey key, TValue value)
            {
                var item = new CacheItem<TValue>
                {
                    Value = value,
                    CreatedAt = DateTime.UtcNow,
                    LastAccessed = DateTime.UtcNow,
                    AccessCount = 1
                };
                
                _cache.AddOrUpdate(key, item, (k, v) => item);
                
                // Implementar LRU si excede el tamaño máximo
                if (_cache.Count > _maxSize)
                {
                    CleanupLRU();
                }
            }
            
            private void CleanupLRU()
            {
                lock (_lockObject)
                {
                    if (_cache.Count <= _maxSize) return;
                    
                    var itemsToRemove = _cache
                        .OrderBy(x => x.Value.LastAccessed)
                        .ThenBy(x => x.Value.AccessCount)
                        .Take(_cache.Count - _maxSize)
                        .Select(x => x.Key)
                        .ToList();
                    
                    foreach (var key in itemsToRemove)
                    {
                        _cache.TryRemove(key, out _);
                    }
                }
            }
            
            public void Clear() => _cache.Clear();
            public int Count => _cache.Count;
        }
        
        public class CacheItem<T>
        {
            public T Value { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastAccessed { get; set; }
            public int AccessCount { get; set; }
        }
        
        // Generics con structs para evitar boxing
        public class StructBasedGeneric<T> where T : struct
        {
            private T _value;
            
            public StructBasedGeneric(T value)
            {
                _value = value;
            }
            
            public T GetValue() => _value;
            public void SetValue(T value) => _value = value;
            
            // Métodos optimizados para structs
            public bool IsDefault() => _value.Equals(default(T));
            public T GetDefault() => default(T);
            
            // Comparación optimizada
            public bool Equals(StructBasedGeneric<T> other)
            {
                return _value.Equals(other._value);
            }
            
            public override bool Equals(object obj)
            {
                if (obj is StructBasedGeneric<T> other)
                    return Equals(other);
                return false;
            }
            
            public override int GetHashCode()
            {
                return _value.GetHashCode();
            }
        }
        
        // Generics con restricciones de valor para optimización
        public class ValueTypeGeneric<T> where T : struct, IComparable<T>, IEquatable<T>
        {
            private T _value;
            
            public ValueTypeGeneric(T value)
            {
                _value = value;
            }
            
            // Operaciones optimizadas para tipos de valor
            public T Max(T other) => _value.CompareTo(other) > 0 ? _value : other;
            public T Min(T other) => _value.CompareTo(other) < 0 ? _value : other;
            public bool IsGreaterThan(T other) => _value.CompareTo(other) > 0;
            public bool IsLessThan(T other) => _value.CompareTo(other) < 0;
            public bool IsEqualTo(T other) => _value.Equals(other);
            
            // Operaciones matemáticas optimizadas
            public T Add(T other) => (T)((dynamic)_value + (dynamic)other);
            public T Subtract(T other) => (T)((dynamic)_value - (dynamic)other);
            public T Multiply(T other) => (T)((dynamic)_value * (dynamic)other);
            public T Divide(T other) => (T)((dynamic)_value / (dynamic)other);
        }
    }
    
    // ===== GENERICS DE ALTO RENDIMIENTO =====
    namespace HighPerformanceGenerics
    {
        // Colección genérica optimizada para performance
        public class HighPerformanceGenericList<T>
        {
            private T[] _items;
            private int _count;
            private int _capacity;
            
            public HighPerformanceGenericList(int initialCapacity = 4)
            {
                _capacity = initialCapacity;
                _items = new T[initialCapacity];
                _count = 0;
            }
            
            public int Count => _count;
            public int Capacity => _capacity;
            
            public T this[int index]
            {
                get
                {
                    if (index < 0 || index >= _count)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    return _items[index];
                }
                set
                {
                    if (index < 0 || index >= _count)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    _items[index] = value;
                }
            }
            
            public void Add(T item)
            {
                if (_count == _capacity)
                {
                    Grow();
                }
                
                _items[_count] = item;
                _count++;
            }
            
            public void AddRange(IEnumerable<T> items)
            {
                foreach (var item in items)
                {
                    Add(item);
                }
            }
            
            public void Insert(int index, T item)
            {
                if (index < 0 || index > _count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                
                if (_count == _capacity)
                {
                    Grow();
                }
                
                if (index < _count)
                {
                    Array.Copy(_items, index, _items, index + 1, _count - index);
                }
                
                _items[index] = item;
                _count++;
            }
            
            public void RemoveAt(int index)
            {
                if (index < 0 || index >= _count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                
                _count--;
                if (index < _count)
                {
                    Array.Copy(_items, index + 1, _items, index, _count - index);
                }
                
                _items[_count] = default(T);
            }
            
            public void Clear()
            {
                if (_count > 0)
                {
                    Array.Clear(_items, 0, _count);
                    _count = 0;
                }
            }
            
            public bool Contains(T item)
            {
                return IndexOf(item) >= 0;
            }
            
            public int IndexOf(T item)
            {
                for (int i = 0; i < _count; i++)
                {
                    if (EqualityComparer<T>.Default.Equals(_items[i], item))
                        return i;
                }
                return -1;
            }
            
            public void Sort()
            {
                Sort(Comparer<T>.Default);
            }
            
            public void Sort(IComparer<T> comparer)
            {
                if (_count > 1)
                {
                    Array.Sort(_items, 0, _count, comparer);
                }
            }
            
            public T[] ToArray()
            {
                var result = new T[_count];
                Array.Copy(_items, result, _count);
                return result;
            }
            
            private void Grow()
            {
                var newCapacity = _capacity * 2;
                if (newCapacity < _capacity)
                {
                    newCapacity = int.MaxValue;
                }
                
                var newItems = new T[newCapacity];
                Array.Copy(_items, newItems, _count);
                _items = newItems;
                _capacity = newCapacity;
            }
        }
        
        // Diccionario genérico optimizado para performance
        public class HighPerformanceGenericDictionary<TKey, TValue>
        {
            private struct Entry
            {
                public TKey Key;
                public TValue Value;
                public int HashCode;
                public int Next;
            }
            
            private Entry[] _entries;
            private int[] _buckets;
            private int _count;
            private int _freeList;
            private int _freeCount;
            private IEqualityComparer<TKey> _comparer;
            
            public HighPerformanceGenericDictionary(int capacity = 0, IEqualityComparer<TKey> comparer = null)
            {
                if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
                
                _comparer = comparer ?? EqualityComparer<TKey>.Default;
                var size = capacity > 0 ? HashHelpers.GetPrime(capacity) : 0;
                
                _buckets = new int[size];
                _entries = new Entry[size];
                _freeList = -1;
            }
            
            public int Count => _count - _freeCount;
            
            public TValue this[TKey key]
            {
                get
                {
                    if (TryGetValue(key, out var value))
                        return value;
                    throw new KeyNotFoundException();
                }
                set => Insert(key, value, true);
            }
            
            public void Add(TKey key, TValue value)
            {
                Insert(key, value, false);
            }
            
            public bool TryGetValue(TKey key, out TValue value)
            {
                var i = FindEntry(key);
                if (i >= 0)
                {
                    value = _entries[i].Value;
                    return true;
                }
                
                value = default(TValue);
                return false;
            }
            
            public bool Remove(TKey key)
            {
                var i = FindEntry(key);
                if (i >= 0)
                {
                    RemoveEntry(i);
                    return true;
                }
                return false;
            }
            
            public void Clear()
            {
                if (_count > 0)
                {
                    Array.Clear(_buckets, 0, _buckets.Length);
                    Array.Clear(_entries, 0, _count);
                    _freeList = -1;
                    _count = 0;
                    _freeCount = 0;
                }
            }
            
            private int FindEntry(TKey key)
            {
                if (key == null) throw new ArgumentNullException(nameof(key));
                
                var comparer = _comparer;
                var hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
                var bucket = hashCode % _buckets.Length;
                
                for (var i = _buckets[bucket]; i >= 0; i = _entries[i].Next)
                {
                    if (_entries[i].HashCode == hashCode && comparer.Equals(_entries[i].Key, key))
                        return i;
                }
                
                return -1;
            }
            
            private void Insert(TKey key, TValue value, bool add)
            {
                if (key == null) throw new ArgumentNullException(nameof(key));
                
                var comparer = _comparer;
                var hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
                var targetBucket = hashCode % _buckets.Length;
                
                for (var i = _buckets[targetBucket]; i >= 0; i = _entries[i].Next)
                {
                    if (_entries[i].HashCode == hashCode && comparer.Equals(_entries[i].Key, key))
                    {
                        if (add) throw new ArgumentException("Key already exists");
                        _entries[i].Value = value;
                        return;
                    }
                }
                
                int index;
                if (_freeCount > 0)
                {
                    index = _freeList;
                    _freeList = _entries[index].Next;
                    _freeCount--;
                }
                else
                {
                    if (_count == _entries.Length)
                    {
                        Resize();
                        targetBucket = hashCode % _buckets.Length;
                    }
                    index = _count;
                    _count++;
                }
                
                _entries[index].HashCode = hashCode;
                _entries[index].Next = _buckets[targetBucket];
                _entries[index].Key = key;
                _entries[index].Value = value;
                _buckets[targetBucket] = index;
            }
            
            private void RemoveEntry(int index)
            {
                var hashCode = _entries[index].HashCode;
                var bucket = hashCode % _buckets.Length;
                
                var last = -1;
                for (var i = _buckets[bucket]; i >= 0; last = i, i = _entries[i].Next)
                {
                    if (i == index)
                    {
                        if (last < 0)
                            _buckets[bucket] = _entries[index].Next;
                        else
                            _entries[last].Next = _entries[index].Next;
                        
                        _entries[index].HashCode = -1;
                        _entries[index].Next = _freeList;
                        _entries[index].Key = default(TKey);
                        _entries[index].Value = default(TValue);
                        _freeList = index;
                        _freeCount++;
                        return;
                    }
                }
            }
            
            private void Resize()
            {
                var newSize = HashHelpers.GetPrime(_count * 2);
                var newBuckets = new int[newSize];
                var newEntries = new Entry[newSize];
                
                Array.Copy(_entries, 0, newEntries, 0, _count);
                
                for (var i = 0; i < _count; i++)
                {
                    var bucket = newEntries[i].HashCode % newSize;
                    newEntries[i].Next = newBuckets[bucket];
                    newBuckets[bucket] = i;
                }
                
                _buckets = newBuckets;
                _entries = newEntries;
            }
        }
        
        // Clase helper para hash
        internal static class HashHelpers
        {
            private static readonly int[] _primes = {
                3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
                1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591, 17519,
                21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437, 187751, 225307,
                270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263, 1674319, 2009191,
                2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369
            };
            
            public static int GetPrime(int min)
            {
                for (int i = 0; i < _primes.Length; i++)
                {
                    if (_primes[i] >= min)
                        return _primes[i];
                }
                
                return min;
            }
        }
    }
    
    // ===== GENERICS CON MEMORY MANAGEMENT =====
    namespace MemoryManagement
    {
        // Generics con Span<T> para operaciones de memoria eficientes
        public class SpanBasedGeneric<T> where T : struct
        {
            public static void ProcessSpan(Span<T> span, Func<T, T> operation)
            {
                for (int i = 0; i < span.Length; i++)
                {
                    span[i] = operation(span[i]);
                }
            }
            
            public static void ProcessSpanParallel(Span<T> span, Func<T, T> operation)
            {
                var partitioner = Partitioner.Create(0, span.Length);
                
                Parallel.ForEach(partitioner, range =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        span[i] = operation(span[i]);
                    }
                });
            }
            
            public static T[] FilterSpan(Span<T> span, Func<T, bool> predicate)
            {
                var result = new List<T>();
                
                for (int i = 0; i < span.Length; i++)
                {
                    if (predicate(span[i]))
                    {
                        result.Add(span[i]);
                    }
                }
                
                return result.ToArray();
            }
        }
        
        // Generics con Memory<T> para operaciones asíncronas
        public class MemoryBasedGeneric<T> where T : struct
        {
            public static async Task ProcessMemoryAsync(Memory<T> memory, Func<T, T> operation)
            {
                await Task.Run(() =>
                {
                    var span = memory.Span;
                    for (int i = 0; i < span.Length; i++)
                    {
                        span[i] = operation(span[i]);
                    }
                });
            }
            
            public static async Task<Memory<T>> TransformMemoryAsync(Memory<T> memory, Func<T, T> operation)
            {
                var result = new T[memory.Length];
                var resultMemory = new Memory<T>(result);
                
                await Task.Run(() =>
                {
                    var sourceSpan = memory.Span;
                    var resultSpan = resultMemory.Span;
                    
                    for (int i = 0; i < sourceSpan.Length; i++)
                    {
                        resultSpan[i] = operation(sourceSpan[i]);
                    }
                });
                
                return resultMemory;
            }
        }
        
        // Generics con ArrayPool para reducir allocations
        public class PooledGenericArray<T>
        {
            public static T[] Rent(int minimumLength)
            {
                return ArrayPool<T>.Shared.Rent(minimumLength);
            }
            
            public static void Return(T[] array, bool clearArray = false)
            {
                ArrayPool<T>.Shared.Return(array, clearArray);
            }
            
            public static void ProcessWithPooledArray(int size, Action<T[]> operation)
            {
                var array = Rent(size);
                try
                {
                    operation(array);
                }
                finally
                {
                    Return(array);
                }
            }
        }
    }
    
    // ===== GENERICS CON COMPILATION OPTIMIZATION =====
    namespace CompilationOptimization
    {
        // Generics con JIT optimization
        public class JitOptimizedGeneric<T> where T : struct
        {
            private T _value;
            
            public JitOptimizedGeneric(T value)
            {
                _value = value;
            }
            
            // Métodos que se optimizan mejor con JIT
            public T GetValue() => _value;
            public void SetValue(T value) => _value = value;
            
            // Operaciones que se optimizan en tiempo de compilación
            public bool IsDefault() => _value.Equals(default(T));
            public T GetDefault() => default(T);
            
            // Métodos que se pueden inlinar
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T Add(T other) => (T)((dynamic)_value + (dynamic)other);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T Subtract(T other) => (T)((dynamic)_value - (dynamic)other);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T Multiply(T other) => (T)((dynamic)_value * (dynamic)other);
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T Divide(T other) => (T)((dynamic)_value / (dynamic)other);
        }
        
        // Generics con conditional compilation
        public class ConditionalGeneric<T>
        {
            private T _value;
            
            public ConditionalGeneric(T value)
            {
                _value = value;
            }
            
            public T GetValue() => _value;
            
#if DEBUG
            public void SetValue(T value)
            {
                // Validación adicional en debug
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                _value = value;
            }
#else
            public void SetValue(T value)
            {
                // Sin validación en release para mejor performance
                _value = value;
            }
#endif
            
#if NET6_0_OR_GREATER
            public bool IsNull() => _value is null;
#else
            public bool IsNull() => _value == null;
#endif
        }
    }
    
    // ===== GENERICS CON MEASUREMENT =====
    namespace Measurement
    {
        public class GenericPerformanceMeasurement
        {
            private readonly Dictionary<string, List<long>> _measurements;
            private readonly Stopwatch _stopwatch;
            
            public GenericPerformanceMeasurement()
            {
                _measurements = new Dictionary<string, List<long>>();
                _stopwatch = new Stopwatch();
            }
            
            public void StartMeasurement(string operationName)
            {
                _stopwatch.Restart();
            }
            
            public void EndMeasurement(string operationName)
            {
                _stopwatch.Stop();
                
                if (!_measurements.ContainsKey(operationName))
                {
                    _measurements[operationName] = new List<long>();
                }
                
                _measurements[operationName].Add(_stopwatch.ElapsedTicks);
            }
            
            public PerformanceStats GetStats(string operationName)
            {
                if (!_measurements.ContainsKey(operationName))
                    return null;
                
                var measurements = _measurements[operationName];
                var ticks = measurements.ToArray();
                
                Array.Sort(ticks);
                
                return new PerformanceStats
                {
                    OperationName = operationName,
                    Count = measurements.Count,
                    MinTicks = ticks[0],
                    MaxTicks = ticks[ticks.Length - 1],
                    MedianTicks = ticks[ticks.Length / 2],
                    AverageTicks = (long)measurements.Average(),
                    P95Ticks = ticks[(int)(ticks.Length * 0.95)],
                    P99Ticks = ticks[(int)(ticks.Length * 0.99)]
                };
            }
            
            public void ClearMeasurements()
            {
                _measurements.Clear();
            }
        }
        
        public class PerformanceStats
        {
            public string OperationName { get; set; }
            public int Count { get; set; }
            public long MinTicks { get; set; }
            public long MaxTicks { get; set; }
            public long MedianTicks { get; set; }
            public long AverageTicks { get; set; }
            public long P95Ticks { get; set; }
            public long P99Ticks { get; set; }
            
            public override string ToString()
            {
                return $"{OperationName}:\n" +
                       $"  Count: {Count:N0}\n" +
                       $"  Min: {MinTicks:N0} ticks\n" +
                       $"  Max: {MaxTicks:N0} ticks\n" +
                       $"  Median: {MedianTicks:N0} ticks\n" +
                       $"  Average: {AverageTicks:N0} ticks\n" +
                       $"  P95: {P95Ticks:N0} ticks\n" +
                       $"  P99: {P99Ticks:N0} ticks";
            }
        }
    }
}

// Uso de Generics y Performance
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Generics y Performance - Clase 8 ===\n");
        
        Console.WriteLine("Los componentes implementados incluyen:");
        Console.WriteLine("1. Benchmarking de generics para medir performance");
        Console.WriteLine("2. Optimización de generics con object pooling y caching");
        Console.WriteLine("3. Generics de alto rendimiento con colecciones optimizadas");
        Console.WriteLine("4. Memory management con Span<T> y Memory<T>");
        Console.WriteLine("5. Compilation optimization con JIT y conditional compilation");
        Console.WriteLine("6. Measurement y estadísticas de performance");
        
        Console.WriteLine("\nBeneficios de esta implementación:");
        Console.WriteLine("- Performance optimizado para aplicaciones críticas");
        Console.WriteLine("- Reducción de allocations y garbage collection");
        Console.WriteLine("- Uso eficiente de memoria con Span<T> y Memory<T>");
        Console.WriteLine("- Benchmarking para identificar cuellos de botella");
        Console.WriteLine("- Optimizaciones específicas para diferentes escenarios");
        
        Console.WriteLine("\nCasos de uso principales:");
        Console.WriteLine("- Aplicaciones de alto rendimiento y baja latencia");
        Console.WriteLine("- Procesamiento de grandes volúmenes de datos");
        Console.WriteLine("- Sistemas en tiempo real");
        Console.WriteLine("- Microservicios con alta concurrencia");
        Console.WriteLine("- Aplicaciones de gaming y multimedia");
        Console.WriteLine("- Sistemas de trading y finanzas");
    }
}

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Benchmark de Generics
Implementa un benchmark que compare el performance de diferentes implementaciones genéricas.

### Ejercicio 2: Colección Optimizada
Crea una colección genérica optimizada para alto rendimiento.

### Ejercicio 3: Memory Pool
Implementa un pool de memoria genérico para reducir allocations.

## 🔍 Puntos Clave

1. **Benchmarking** para medir performance de generics
2. **Optimización** con object pooling y caching
3. **Alto rendimiento** con colecciones optimizadas
4. **Memory management** con Span<T> y Memory<T>
5. **Compilation optimization** para mejor performance

## 📚 Recursos Adicionales

- [Microsoft Docs - Performance Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/performance/)
- [High Performance .NET](https://docs.microsoft.com/en-us/dotnet/standard/performance/)

---

**🎯 ¡Has completado la Clase 8! Ahora comprendes Generics y Performance**

**📚 [Siguiente: Clase 9 - Patrones con Generics](clase_9_patrones_generics.md)**
