# ⚡ Clase 6: Performance y Escalabilidad

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 5: Monitoreo y Observabilidad](../senior_8/clase_5_monitoreo_observabilidad.md)
- **🏠 Inicio del Módulo**: [Módulo 15: Sistemas Avanzados y Distribuidos](../senior_8/README.md)
- **➡️ Siguiente**: [Clase 7: Seguridad en Producción](../senior_8/clase_7_seguridad_produccion.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** caching multi-nivel
2. **Configurar** connection pooling
3. **Desarrollar** async patterns optimizados
4. **Aplicar** técnicas de paralelización
5. **Optimizar** algoritmos y estructuras de datos

---

## 🚀 **Caching Multi-Nivel**

### **Servicio de Caching Inteligente**

```csharp
// MusicalMatching.Application/Services/CacheService.cs
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;

namespace MusicalMatching.Application.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
}

public class MultiLevelCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<MultiLevelCacheService> _logger;

    public MultiLevelCacheService(
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        ILogger<MultiLevelCacheService> logger)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        // L1: Memory Cache
        if (_memoryCache.TryGetValue(key, out T? memoryValue))
        {
            _logger.LogDebug("Cache hit in memory for key: {Key}", key);
            return memoryValue;
        }

        // L2: Distributed Cache (Redis)
        try
        {
            var redisValue = await _distributedCache.GetStringAsync(key);
            if (!string.IsNullOrEmpty(redisValue))
            {
                var value = JsonSerializer.Deserialize<T>(redisValue);
                if (value != null)
                {
                    // Populate L1 cache
                    _memoryCache.Set(key, value, TimeSpan.FromMinutes(5));
                    _logger.LogDebug("Cache hit in Redis for key: {Key}", key);
                    return value;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing Redis cache for key: {Key}", key);
        }

        _logger.LogDebug("Cache miss for key: {Key}", key);
        return default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30)
        };

        // Set in both caches
        var serializedValue = JsonSerializer.Serialize(value);
        await _distributedCache.SetStringAsync(key, serializedValue, options);
        _memoryCache.Set(key, value, expiration ?? TimeSpan.FromMinutes(5));

        _logger.LogDebug("Set cache for key: {Key} with expiration: {Expiration}", key, expiration);
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        var cachedValue = await GetAsync<T>(key);
        if (cachedValue != null)
        {
            return cachedValue;
        }

        // Use semaphore to prevent cache stampede
        var semaphore = new SemaphoreSlim(1, 1);
        await semaphore.WaitAsync();

        try
        {
            // Double-check after acquiring lock
            cachedValue = await GetAsync<T>(key);
            if (cachedValue != null)
            {
                return cachedValue;
            }

            // Execute factory and cache result
            var value = await factory();
            await SetAsync(key, value, expiration);
            return value;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task RemoveAsync(string key)
    {
        _memoryCache.Remove(key);
        await _distributedCache.RemoveAsync(key);
        _logger.LogDebug("Removed cache for key: {Key}", key);
    }
}
```

---

## 🔗 **Connection Pooling Optimizado**

### **Database Connection Manager**

```csharp
// MusicalMatching.Infrastructure/Data/ConnectionManager.cs
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace MusicalMatching.Infrastructure.Data;

public interface IConnectionManager
{
    Task<DbContext> GetConnectionAsync();
    Task ReleaseConnectionAsync(DbContext context);
    Task<int> GetActiveConnectionCountAsync();
}

public class ConnectionManager : IConnectionManager, IDisposable
{
    private readonly ConcurrentQueue<DbContext> _connectionPool;
    private readonly SemaphoreSlim _semaphore;
    private readonly int _maxPoolSize;
    private readonly Func<DbContext> _contextFactory;
    private readonly ILogger<ConnectionManager> _logger;

    public ConnectionManager(
        Func<DbContext> contextFactory,
        ILogger<ConnectionManager> logger,
        IConfiguration configuration)
    {
        _contextFactory = contextFactory;
        _logger = logger;
        _maxPoolSize = configuration.GetValue<int>("Database:MaxPoolSize", 100);
        _connectionPool = new ConcurrentQueue<DbContext>();
        _semaphore = new SemaphoreSlim(_maxPoolSize, _maxPoolSize);
    }

    public async Task<DbContext> GetConnectionAsync()
    {
        await _semaphore.WaitAsync();

        if (_connectionPool.TryDequeue(out var context))
        {
            _logger.LogDebug("Reusing connection from pool. Active connections: {Count}", 
                _maxPoolSize - _semaphore.CurrentCount);
            return context;
        }

        var newContext = _contextFactory();
        _logger.LogDebug("Created new connection. Active connections: {Count}", 
            _maxPoolSize - _semaphore.CurrentCount);
        return newContext;
    }

    public async Task ReleaseConnectionAsync(DbContext context)
    {
        if (context == null) return;

        try
        {
            // Reset context state
            context.ChangeTracker.Clear();
            
            if (_connectionPool.Count < _maxPoolSize)
            {
                _connectionPool.Enqueue(context);
                _logger.LogDebug("Returned connection to pool. Pool size: {Size}", _connectionPool.Count);
            }
            else
            {
                await context.DisposeAsync();
                _logger.LogDebug("Disposed connection. Pool is full.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing connection");
            await context.DisposeAsync();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public Task<int> GetActiveConnectionCountAsync()
    {
        return Task.FromResult(_maxPoolSize - _semaphore.CurrentCount);
    }

    public void Dispose()
    {
        _semaphore?.Dispose();
    }
}
```

---

## ⚡ **Async Patterns Optimizados**

### **Async Streams para Procesamiento de Datos**

```csharp
// MusicalMatching.Application/Services/DataProcessingService.cs
using System.Collections.Concurrent;

namespace MusicalMatching.Application.Services;

public interface IDataProcessingService
{
    IAsyncEnumerable<MusicianMatch> ProcessMatchesAsync(
        IEnumerable<MusicianRequest> requests,
        CancellationToken cancellationToken = default);
    
    Task<List<MusicianMatch>> ProcessMatchesParallelAsync(
        IEnumerable<MusicianRequest> requests,
        int maxDegreeOfParallelism = 4,
        CancellationToken cancellationToken = default);
}

public class DataProcessingService : IDataProcessingService
{
    private readonly IMusicianMatchingService _matchingService;
    private readonly ILogger<DataProcessingService> _logger;

    public DataProcessingService(
        IMusicianMatchingService matchingService,
        ILogger<DataProcessingService> logger)
    {
        _matchingService = matchingService;
        _logger = logger;
    }

    public async IAsyncEnumerable<MusicianMatch> ProcessMatchesAsync(
        IEnumerable<MusicianRequest> requests,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var batchSize = 100;
        var batch = new List<MusicianRequest>();

        foreach (var request in requests)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            batch.Add(request);

            if (batch.Count >= batchSize)
            {
                await foreach (var match in ProcessBatchAsync(batch, cancellationToken))
                {
                    yield return match;
                }
                batch.Clear();
            }
        }

        // Process remaining items
        if (batch.Any())
        {
            await foreach (var match in ProcessBatchAsync(batch, cancellationToken))
            {
                yield return match;
            }
        }
    }

    private async IAsyncEnumerable<MusicianMatch> ProcessBatchAsync(
        List<MusicianRequest> batch,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var tasks = batch.Select(request => _matchingService.FindMatchesForRequestAsync(request));
        var results = await Task.WhenAll(tasks);

        foreach (var result in results)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            foreach (var match in result)
            {
                yield return match;
            }
        }
    }

    public async Task<List<MusicianMatch>> ProcessMatchesParallelAsync(
        IEnumerable<MusicianRequest> requests,
        int maxDegreeOfParallelism = 4,
        CancellationToken cancellationToken = default)
    {
        var results = new ConcurrentBag<MusicianMatch>();
        var semaphore = new SemaphoreSlim(maxDegreeOfParallelism, maxDegreeOfParallelism);

        var tasks = requests.Select(async request =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var matches = await _matchingService.FindMatchesForRequestAsync(request, cancellationToken);
                foreach (var match in matches)
                {
                    results.Add(match);
                }
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
        return results.ToList();
    }
}
```

---

## 🔄 **Background Services Optimizados**

### **Background Job Processor**

```csharp
// MusicalMatching.API/BackgroundServices/JobProcessorService.cs
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace MusicalMatching.API.BackgroundServices;

public class JobProcessorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<JobProcessorService> _logger;
    private readonly Channel<BackgroundJob> _jobChannel;
    private readonly SemaphoreSlim _semaphore;
    private readonly int _maxConcurrentJobs;

    public JobProcessorService(
        IServiceProvider serviceProvider,
        ILogger<JobProcessorService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _maxConcurrentJobs = configuration.GetValue<int>("BackgroundJobs:MaxConcurrent", 5);
        _jobChannel = Channel.CreateUnbounded<BackgroundJob>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = false
        });
        _semaphore = new SemaphoreSlim(_maxConcurrentJobs, _maxConcurrentJobs);
    }

    public async Task EnqueueJobAsync(BackgroundJob job)
    {
        await _jobChannel.Writer.WriteAsync(job);
        _logger.LogInformation("Job enqueued: {JobType} {JobId}", job.Type, job.Id);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tasks = new List<Task>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var job = await _jobChannel.Reader.ReadAsync(stoppingToken);
                var task = ProcessJobAsync(job, stoppingToken);
                tasks.Add(task);

                // Clean up completed tasks
                tasks.RemoveAll(t => t.IsCompleted);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading from job channel");
            }
        }

        // Wait for remaining tasks to complete
        await Task.WhenAll(tasks);
    }

    private async Task ProcessJobAsync(BackgroundJob job, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            _logger.LogInformation("Processing job: {JobType} {JobId}", job.Type, job.Id);
            
            using var scope = _serviceProvider.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IJobProcessor>();
            
            await processor.ProcessAsync(job, cancellationToken);
            
            _logger.LogInformation("Job completed: {JobType} {JobId}", job.Type, job.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing job: {JobType} {JobId}", job.Type, job.Id);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}

public record BackgroundJob(Guid Id, string Type, object Data);
public interface IJobProcessor
{
    Task ProcessAsync(BackgroundJob job, CancellationToken cancellationToken);
}
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Caching Multi-Nivel**
```csharp
// Implementa:
// - Memory cache + Redis
// - Cache stampede prevention
// - Expiration policies
// - Cache invalidation
```

### **Ejercicio 2: Connection Pooling**
```csharp
// Crea:
// - Connection pool manager
// - Semaphore-based limiting
// - Connection reuse
// - Health monitoring
```

### **Ejercicio 3: Async Patterns**
```csharp
// Implementa:
// - Async streams
// - Parallel processing
// - Background services
// - Cancellation support
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **🚀 Caching Multi-Nivel**: Memory + Redis con prevención de stampede
2. **🔗 Connection Pooling**: Gestión optimizada de conexiones de base de datos
3. **⚡ Async Patterns**: Streams asíncronos y procesamiento paralelo
4. **🔄 Background Services**: Procesamiento de jobs en segundo plano
5. **📊 Performance Monitoring**: Métricas y optimización de rendimiento

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Seguridad en Producción**, implementando autenticación, autorización y protección contra amenazas.

---

**¡Has completado la sexta clase del Módulo 15! ⚡🚀**


