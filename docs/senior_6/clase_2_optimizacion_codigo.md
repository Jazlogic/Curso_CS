# 🚀 Clase 2: Optimización de Código y Memoria

## 🧭 Navegación
- **⬅️ Anterior**: [Clase 1: Optimización de Performance y Profiling](clase_1_optimizacion_performance.md)
- **🏠 [Volver al Módulo 6](../README.md)**
- **➡️ Siguiente**: [Clase 3: Seguridad en Aplicaciones .NET](clase_3_seguridad_aplicaciones.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)**

---

## 🎯 Objetivos de la Clase

Al finalizar esta clase, serás capaz de:
- Implementar object pooling para reducir GC pressure
- Optimizar consultas LINQ para mejor rendimiento
- Utilizar estructuras de datos eficientes
- Implementar async/await de manera optimizada

---

## 📚 Contenido Teórico

### 2.1 Object Pooling y Reducción de GC Pressure

El Object Pooling es una técnica fundamental para reducir la presión del Garbage Collector y mejorar el rendimiento de aplicaciones .NET.

#### ¿Por qué Object Pooling?

Cuando creamos y destruimos objetos frecuentemente:
- **Aumenta la presión del GC**: Más objetos en el heap
- **Fragmentación de memoria**: Espacios vacíos entre objetos
- **Overhead de asignación**: Tiempo para buscar memoria libre
- **Colecciones frecuentes**: El GC se ejecuta más a menudo

#### Implementación Básica de Object Pool

```csharp
public class ObjectPool<T> where T : class, new()
{
    private readonly ConcurrentQueue<T> _pool;
    private readonly int _maxSize;
    private readonly Func<T> _factory;
    private readonly Action<T> _resetAction;
    
    public ObjectPool(int maxSize = 100, Func<T> factory = null, Action<T> resetAction = null)
    {
        _pool = new ConcurrentQueue<T>();
        _maxSize = maxSize;
        _factory = factory ?? (() => new T());
        _resetAction = resetAction ?? (obj => { });
    }
    
    public T Get()
    {
        // Intenta obtener un objeto del pool
        if (_pool.TryDequeue(out T item))
        {
            return item;
        }
        
        // Si no hay objetos disponibles, crea uno nuevo
        return _factory();
    }
    
    public void Return(T item)
    {
        if (item == null) return;
        
        // Solo agrega al pool si no está lleno
        if (_pool.Count < _maxSize)
        {
            // Resetea el objeto antes de devolverlo al pool
            _resetAction(item);
            _pool.Enqueue(item);
        }
        // Si el pool está lleno, el objeto será recolectado por el GC
    }
    
    public int Count => _pool.Count;
    public int MaxSize => _maxSize;
}
```

#### Uso del Object Pool

```csharp
public class OptimizedService
{
    private readonly ObjectPool<StringBuilder> _stringBuilderPool;
    private readonly ObjectPool<List<string>> _listPool;
    
    public OptimizedService()
    {
        // Pool de StringBuilders con reset personalizado
        _stringBuilderPool = new ObjectPool<StringBuilder>(
            maxSize: 50,
            factory: () => new StringBuilder(1000), // Capacidad inicial
            resetAction: sb => sb.Clear() // Limpia el StringBuilder
        );
        
        // Pool de Listas con reset personalizado
        _listPool = new ObjectPool<List<string>>(
            maxSize: 100,
            factory: () => new List<string>(100), // Capacidad inicial
            resetAction: list => list.Clear() // Limpia la lista
        );
    }
    
    public string ProcessData(IEnumerable<string> items)
    {
        // Obtiene un StringBuilder del pool
        var sb = _stringBuilderPool.Get();
        try
        {
            foreach (var item in items)
            {
                sb.AppendLine(item);
            }
            return sb.ToString();
        }
       finally
        {
            // Siempre devuelve el StringBuilder al pool
            _stringBuilderPool.Return(sb);
        }
    }
    
    public List<string> FilterAndTransform(IEnumerable<string> items, Func<string, bool> filter)
    {
        // Obtiene una lista del pool
        var result = _listPool.Get();
        try
        {
            foreach (var item in items)
            {
                if (filter(item))
                {
                    result.Add(item.ToUpper());
                }
            }
            return result.ToList(); // Crea una nueva lista para el resultado
        }
        finally
        {
            // Devuelve la lista al pool
            _listPool.Return(result);
        }
    }
}
```

#### Object Pool para HttpClient

```csharp
public class HttpClientPool : IDisposable
{
    private readonly ObjectPool<HttpClient> _httpClientPool;
    private readonly IHttpClientFactory _httpClientFactory;
    
    public HttpClientPool(IHttpClientFactory httpClientFactory, int maxSize = 20)
    {
        _httpClientFactory = httpClientFactory;
        _httpClientPool = new ObjectPool<HttpClient>(
            maxSize: maxSize,
            factory: () => _httpClientFactory.CreateClient(),
            resetAction: client => 
            {
                // No es necesario resetear HttpClient
                // Solo se devuelve al pool
            }
        );
    }
    
    public async Task<string> GetAsync(string url)
    {
        var client = _httpClientPool.Get();
        try
        {
            var response = await client.GetAsync(url);
            return await response.Content.ReadAsStringAsync();
        }
        finally
        {
            _httpClientPool.Return(client);
        }
    }
    
    public void Dispose()
    {
        // Cleanup si es necesario
    }
}
```

### 2.2 Optimización de LINQ

LINQ es poderoso pero puede ser ineficiente si no se usa correctamente. Vamos a ver técnicas de optimización.

#### Evitar Múltiples Enumeraciones

```csharp
public class LinqOptimizer
{
    // ❌ Malo: múltiples enumeraciones
    public List<string> GetFilteredAndSortedNames_Bad(IEnumerable<User> users)
    {
        var filtered = users.Where(u => u.IsActive); // Primera enumeración
        var sorted = filtered.OrderBy(u => u.Name);  // Segunda enumeración
        return sorted.Select(u => u.Name).ToList();  // Tercera enumeración
    }
    
    // ✅ Bueno: una sola enumeración
    public List<string> GetFilteredAndSortedNames_Good(IEnumerable<User> users)
    {
        return users
            .Where(u => u.IsActive)
            .OrderBy(u => u.Name)
            .Select(u => u.Name)
            .ToList();
    }
    
    // ✅ Mejor: materializar una vez y reutilizar
    public List<string> GetFilteredAndSortedNames_Better(IEnumerable<User> users)
    {
        var activeUsers = users.Where(u => u.IsActive).ToList(); // Materializa una vez
        
        return activeUsers
            .OrderBy(u => u.Name)
            .Select(u => u.Name)
            .ToList();
    }
}
```

#### Uso de AsParallel para Operaciones Costosas

```csharp
public class ParallelLinqOptimizer
{
    public List<ProcessedData> ProcessDataParallel(IEnumerable<RawData> rawData)
    {
        return rawData
            .AsParallel() // Paraleliza el procesamiento
            .WithDegreeOfParallelism(Environment.ProcessorCount) // Controla el número de threads
            .Where(data => data.IsValid)
            .Select(data => ProcessDataItem(data)) // Operación costosa
            .ToList();
    }
    
    private ProcessedData ProcessDataItem(RawData rawData)
    {
        // Simula procesamiento costoso
        Thread.Sleep(10);
        return new ProcessedData
        {
            Id = rawData.Id,
            Value = rawData.Value * 2,
            ProcessedAt = DateTime.UtcNow
        };
    }
    
    // ✅ Optimización para operaciones de agregación
    public decimal CalculateTotalValue(IEnumerable<Order> orders)
    {
        return orders
            .AsParallel()
            .Where(o => o.Status == OrderStatus.Confirmed)
            .Sum(o => o.Total);
    }
}
```

#### Optimización de Consultas de Base de Datos

```csharp
public class DatabaseQueryOptimizer
{
    private readonly DbContext _context;
    
    public DatabaseQueryOptimizer(DbContext context)
    {
        _context = context;
    }
    
    // ❌ Malo: N+1 queries
    public async Task<List<OrderDto>> GetOrdersWithCustomerInfo_Bad(List<int> orderIds)
    {
        var orders = new List<OrderDto>();
        
        foreach (var orderId in orderIds)
        {
            var order = await _context.Orders
                .Include(o => o.Customer) // Incluye customer para cada order
                .FirstOrDefaultAsync(o => o.Id == orderId);
                
            if (order != null)
            {
                orders.Add(MapToDto(order));
            }
        }
        
        return orders;
    }
    
    // ✅ Bueno: Una sola query con Include
    public async Task<List<OrderDto>> GetOrdersWithCustomerInfo_Good(List<int> orderIds)
    {
        var orders = await _context.Orders
            .Include(o => o.Customer)
            .Where(o => orderIds.Contains(o.Id))
            .ToListAsync();
            
        return orders.Select(MapToDto).ToList();
    }
    
    // ✅ Mejor: Proyección directa a DTO
    public async Task<List<OrderDto>> GetOrdersWithCustomerInfo_Best(List<int> orderIds)
    {
        return await _context.Orders
            .Where(o => orderIds.Contains(o.Id))
            .Select(o => new OrderDto
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                Total = o.Total,
                CustomerName = o.Customer.Name,
                CustomerEmail = o.Customer.Email
            })
            .ToListAsync();
    }
    
    private OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            Total = order.Total,
            CustomerName = order.Customer.Name,
            CustomerEmail = order.Customer.Email
        };
    }
}
```

### 2.3 Uso Eficiente de Estructuras de Datos

La elección correcta de estructuras de datos puede tener un impacto significativo en el rendimiento.

#### Comparación de Estructuras de Datos

```csharp
public class DataStructureOptimizer
{
    // ✅ List<T> para acceso secuencial y modificaciones frecuentes
    public List<int> GetSequentialData()
    {
        var list = new List<int>(1000); // Capacidad inicial
        
        for (int i = 0; i < 1000; i++)
        {
            list.Add(i); // O(1) amortizado
        }
        
        return list;
    }
    
    // ✅ HashSet<T> para búsquedas rápidas y sin duplicados
    public HashSet<string> GetUniqueItems(IEnumerable<string> items)
    {
        return new HashSet<string>(items, StringComparer.OrdinalIgnoreCase);
    }
    
    // ✅ Dictionary<TKey, TValue> para búsquedas por clave
    public Dictionary<int, User> CreateUserLookup(IEnumerable<User> users)
    {
        return users.ToDictionary(u => u.Id, u => u);
    }
    
    // ✅ Queue<T> para procesamiento FIFO
    public void ProcessQueue(IEnumerable<Task> tasks)
    {
        var queue = new Queue<Task>(tasks);
        
        while (queue.Count > 0)
        {
            var task = queue.Dequeue();
            task.Execute();
        }
    }
    
    // ✅ Stack<T> para procesamiento LIFO
    public void ProcessStack(IEnumerable<Operation> operations)
    {
        var stack = new Stack<Operation>(operations);
        
        while (stack.Count > 0)
        {
            var operation = stack.Pop();
            operation.Execute();
        }
    }
}
```

#### Uso de Span<T> y Memory<T>

```csharp
public class SpanOptimizer
{
    // ✅ Uso de Span<T> para operaciones sin asignación de memoria
    public int SumArray(Span<int> numbers)
    {
        int sum = 0;
        for (int i = 0; i < numbers.Length; i++)
        {
            sum += numbers[i];
        }
        return sum;
    }
    
    // ✅ Procesamiento de strings sin asignación
    public string ProcessString(ReadOnlySpan<char> input)
    {
        var result = new StringBuilder();
        
        for (int i = 0; i < input.Length; i++)
        {
            if (char.IsLetter(input[i]))
            {
                result.Append(char.ToUpper(input[i]));
            }
        }
        
        return result.ToString();
    }
    
    // ✅ Uso de Memory<T> para operaciones asíncronas
    public async Task<int> ProcessMemoryAsync(Memory<byte> data)
    {
        var sum = 0;
        
        for (int i = 0; i < data.Length; i++)
        {
            sum += data.Span[i];
            await Task.Delay(1); // Simula operación asíncrona
        }
        
        return sum;
    }
}
```

### 2.4 Async/Await Optimizado

El uso correcto de async/await es crucial para el rendimiento de aplicaciones asíncronas.

#### Patrones de Async/Await

```csharp
public class AsyncOptimizer
{
    // ✅ Correcto: ConfigureAwait(false) para operaciones no-UI
    public async Task<List<string>> GetDataAsync()
    {
        var data = await FetchDataFromApiAsync()
            .ConfigureAwait(false); // Evita contexto de sincronización
            
        return await ProcessDataAsync(data)
            .ConfigureAwait(false);
    }
    
    // ✅ Correcto: CancellationToken para cancelación
    public async Task<List<string>> GetDataWithCancellationAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await FetchDataFromApiAsync(cancellationToken);
            return await ProcessDataAsync(data, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Maneja cancelación gracefully
            return new List<string>();
        }
    }
    
    // ✅ Correcto: Task.WhenAll para operaciones paralelas
    public async Task<List<string>> GetDataParallelAsync()
    {
        var tasks = new[]
        {
            FetchDataFromApi1Async(),
            FetchDataFromApi2Async(),
            FetchDataFromApi3Async()
        };
        
        var results = await Task.WhenAll(tasks);
        return results.SelectMany(r => r).ToList();
    }
    
    // ✅ Correcto: Task.WhenAny para operaciones con timeout
    public async Task<string> GetDataWithTimeoutAsync(TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        
        var dataTask = FetchDataFromApiAsync();
        var timeoutTask = Task.Delay(Timeout.Infinite, cts.Token);
        
        var completedTask = await Task.WhenAny(dataTask, timeoutTask);
        
        if (completedTask == dataTask)
        {
            return await dataTask;
        }
        
        throw new TimeoutException("Operation timed out");
    }
}
```

#### Optimización de Background Services

```csharp
public class OptimizedBackgroundService : BackgroundService
{
    private readonly ILogger<OptimizedBackgroundService> _logger;
    private readonly ObjectPool<HttpClient> _httpClientPool;
    
    public OptimizedBackgroundService(
        ILogger<OptimizedBackgroundService> logger,
        ObjectPool<HttpClient> httpClientPool)
    {
        _logger = logger;
        _httpClientPool = httpClientPool;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
                
                // Espera eficiente sin bloquear threads
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Cancelación normal, salir del loop
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch");
                
                // Espera antes de reintentar
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
    
    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        var client = _httpClientPool.Get();
        try
        {
            // Procesa batch de datos
            var data = await FetchBatchDataAsync(client, cancellationToken);
            await ProcessDataAsync(data, cancellationToken);
        }
        finally
        {
            _httpClientPool.Return(client);
        }
    }
    
    private async Task<List<string>> FetchBatchDataAsync(HttpClient client, CancellationToken cancellationToken)
    {
        var response = await client.GetAsync("/api/batch", cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<List<string>>(content);
    }
    
    private async Task ProcessDataAsync(List<string> data, CancellationToken cancellationToken)
    {
        // Procesa datos en paralelo con cancelación
        var tasks = data.Select(item => ProcessItemAsync(item, cancellationToken));
        await Task.WhenAll(tasks);
    }
    
    private async Task ProcessItemAsync(string item, CancellationToken cancellationToken)
    {
        // Simula procesamiento
        await Task.Delay(100, cancellationToken);
    }
}
```

---

## 💻 Ejercicios Prácticos

### Ejercicio 1: Implementar Object Pool Personalizado

Crea un object pool para `List<T>` con funcionalidades avanzadas:

```csharp
public class AdvancedListPool<T>
{
    // Implementa:
    // - Múltiples tamaños de pool
    // - Estadísticas de uso
    // - Cleanup automático
    // - Thread safety
}
```

### Ejercicio 2: Optimizar Consulta LINQ

Optimiza esta consulta LINQ ineficiente:

```csharp
public List<string> GetActiveUserNames(IEnumerable<User> users)
{
    // ❌ Optimiza esta implementación
    var activeUsers = users.Where(u => u.IsActive).ToList();
    var names = activeUsers.Select(u => u.Name).ToList();
    var sortedNames = names.OrderBy(n => n).ToList();
    return sortedNames;
}
```

---

## 🔍 Casos de Uso Reales

### 1. API con Object Pooling

```csharp
[ApiController]
[Route("api/[controller]")]
public class OptimizedApiController : ControllerBase
{
    private readonly ObjectPool<StringBuilder> _stringBuilderPool;
    private readonly ObjectPool<List<object>> _listPool;
    
    public OptimizedApiController(ObjectPool<StringBuilder> stringBuilderPool, ObjectPool<List<object>> listPool)
    {
        _stringBuilderPool = stringBuilderPool;
        _listPool = listPool;
    }
    
    [HttpGet("report")]
    public async Task<IActionResult> GenerateReport()
    {
        var sb = _stringBuilderPool.Get();
        var dataList = _listPool.Get();
        
        try
        {
            // Genera reporte usando objetos del pool
            await GenerateReportContentAsync(sb, dataList);
            return Ok(sb.ToString());
        }
        finally
        {
            _stringBuilderPool.Return(sb);
            _listPool.Return(dataList);
        }
    }
}
```

### 2. Procesamiento de Datos en Lote

```csharp
public class BatchDataProcessor
{
    private readonly ObjectPool<DataProcessor> _processorPool;
    
    public async Task ProcessBatchAsync(IEnumerable<DataItem> items)
    {
        var tasks = new List<Task>();
        
        foreach (var batch in items.Chunk(100))
        {
            var processor = _processorPool.Get();
            var task = ProcessBatchWithProcessorAsync(processor, batch);
            tasks.Add(task);
        }
        
        await Task.WhenAll(tasks);
    }
    
    private async Task ProcessBatchWithProcessorAsync(DataProcessor processor, IEnumerable<DataItem> batch)
    {
        try
        {
            await processor.ProcessAsync(batch);
        }
        finally
        {
            _processorPool.Return(processor);
        }
    }
}
```

---

## 📊 Métricas de Optimización

### KPIs de Performance

1. **Memory Allocation**: Reducción en asignaciones de memoria
2. **GC Pressure**: Menos colecciones del Garbage Collector
3. **Response Time**: Tiempo de respuesta mejorado
4. **Throughput**: Más operaciones por segundo
5. **CPU Usage**: Menor utilización del procesador

### Herramientas de Medición

- **dotMemory**: Análisis de memoria
- **dotTrace**: Profiling de performance
- **BenchmarkDotNet**: Benchmarking
- **PerfView**: Análisis de eventos

---

## 🎯 Resumen de la Clase

En esta clase hemos aprendido:

✅ **Object Pooling**: Técnicas para reducir GC pressure
✅ **Optimización de LINQ**: Evitar múltiples enumeraciones y usar paralelización
✅ **Estructuras de Datos Eficientes**: Elección correcta según el caso de uso
✅ **Async/Await Optimizado**: Patrones para mejor rendimiento asíncrono
✅ **Implementación Práctica**: Código real con object pooling y optimizaciones

---

## 🚀 Próximos Pasos

En la siguiente clase aprenderemos sobre:
- **Seguridad en Aplicaciones .NET**
- Autenticación y autorización avanzada
- Protección contra ataques comunes
- Cifrado y hashing seguro

---

## 🔗 Enlaces de Referencia

- [Object Pooling in .NET](https://docs.microsoft.com/en-us/dotnet/standard/collections/thread-safe/)
- [LINQ Performance](https://docs.microsoft.com/en-us/dotnet/standard/linq/)
- [Async/Await Best Practices](https://docs.microsoft.com/en-us/dotnet/csharp/async)
- [Performance Best Practices](https://docs.microsoft.com/en-us/dotnet/fundamentals/performance/)


