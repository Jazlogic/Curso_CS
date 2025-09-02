# ⚡ Clase 8: Caching y Performance

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 7: Sistema de Notificaciones](../senior_7/clase_7_sistema_notificaciones.md)
- **🏠 Inicio del Módulo**: [Módulo 14: Plataformas Empresariales Reales](../senior_7/README.md)
- **➡️ Siguiente**: [Clase 9: Monitoreo y Observabilidad](../senior_7/clase_9_monitoreo_observabilidad.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** estrategias de caching multi-nivel
2. **Optimizar** consultas y operaciones de base de datos
3. **Desarrollar** sistemas de cache distribuido
4. **Aplicar** técnicas de performance avanzadas
5. **Monitorear** métricas de rendimiento

---

## 🗄️ **Sistema de Caching Multi-Nivel**

### **Arquitectura de Caching Inteligente**

```csharp
public interface ICacheService
{
    Task<T> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
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

    public async Task<T> GetAsync<T>(string key)
    {
        // Nivel 1: Cache en memoria
        if (_memoryCache.TryGetValue(key, out T memoryValue))
        {
            return memoryValue;
        }

        // Nivel 2: Cache distribuido
        var distributedValue = await _distributedCache.GetStringAsync(key);
        if (!string.IsNullOrEmpty(distributedValue))
        {
            var deserializedValue = JsonSerializer.Deserialize<T>(distributedValue);
            _memoryCache.Set(key, deserializedValue, TimeSpan.FromMinutes(5));
            return deserializedValue;
        }

        return default(T);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var expirationTime = expiration ?? TimeSpan.FromMinutes(30);
        var serializedValue = JsonSerializer.Serialize(value);

        var distributedOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expirationTime
        };

        var memoryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        await _distributedCache.SetStringAsync(key, serializedValue, distributedOptions);
        _memoryCache.Set(key, value, memoryOptions);
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        var cachedValue = await GetAsync<T>(key);
        if (cachedValue != null)
        {
            return cachedValue;
        }

        var value = await factory();
        await SetAsync(key, value, expiration);
        return value;
    }

    public async Task RemoveAsync(string key)
    {
        _memoryCache.Remove(key);
        await _distributedCache.RemoveAsync(key);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        return await GetAsync<object>(key) != null;
    }
}
```

---

## 🚀 **Optimización de Consultas y Base de Datos**

### **Query Optimization y Connection Pooling**

```csharp
public interface IQueryOptimizer
{
    Task<List<T>> ExecuteOptimizedQueryAsync<T>(string query, object parameters = null);
    Task<T> ExecuteOptimizedQuerySingleAsync<T>(string query, object parameters = null);
}

public class QueryOptimizer : IQueryOptimizer
{
    private readonly string _connectionString;
    private readonly SemaphoreSlim _semaphore;
    private readonly int _maxConcurrentConnections = 100;

    public QueryOptimizer(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
        _semaphore = new SemaphoreSlim(_maxConcurrentConnections, _maxConcurrentConnections);
    }

    public async Task<List<T>> ExecuteOptimizedQueryAsync<T>(string query, object parameters = null)
    {
        await _semaphore.WaitAsync();
        
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = new SqlCommand(query, connection);
            command.CommandTimeout = 30;
            
            if (parameters != null)
            {
                AddParameters(command, parameters);
            }

            var result = new List<T>();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                var item = MapReaderToObject<T>(reader);
                result.Add(item);
            }

            return result;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<T> ExecuteOptimizedQuerySingleAsync<T>(string query, object parameters = null)
    {
        var results = await ExecuteOptimizedQueryAsync<T>(query, parameters);
        return results.FirstOrDefault();
    }

    private void AddParameters(SqlCommand command, object parameters)
    {
        if (parameters is IDictionary<string, object> dict)
        {
            foreach (var kvp in dict)
            {
                command.Parameters.AddWithValue(kvp.Key, kvp.Value ?? DBNull.Value);
            }
        }
        else
        {
            var properties = parameters.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(parameters);
                command.Parameters.AddWithValue($"@{prop.Name}", value ?? DBNull.Value);
            }
        }
    }

    private T MapReaderToObject<T>(IDataReader reader)
    {
        var obj = Activator.CreateInstance<T>();
        var properties = typeof(T).GetProperties();

        for (int i = 0; i < reader.FieldCount; i++)
        {
            var columnName = reader.GetName(i);
            var property = properties.FirstOrDefault(p => 
                string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase));

            if (property != null && !reader.IsDBNull(i))
            {
                var value = reader.GetValue(i);
                var convertedValue = Convert.ChangeType(value, property.PropertyType);
                property.SetValue(obj, convertedValue);
            }
        }

        return obj;
    }
}
```

---

## 🔄 **Cache Distribuido con Redis**

### **Implementación de Redis Cache**

```csharp
public interface IRedisCacheService
{
    Task<T> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
    Task<long> IncrementAsync(string key, long value = 1);
}

public class RedisCacheService : IRedisCacheService
{
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _database = redis.GetDatabase();
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<T> GetAsync<T>(string key)
    {
        try
        {
            var value = await _database.StringGetAsync(key);
            if (value.HasValue)
            {
                return JsonSerializer.Deserialize<T>(value, _jsonOptions);
            }
            return default(T);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from Redis for key: {Key}", key);
            return default(T);
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            await _database.StringSetAsync(key, serializedValue, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in Redis for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing key from Redis: {Key}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            return await _database.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking key existence in Redis: {Key}", key);
            return false;
        }
    }

    public async Task<long> IncrementAsync(string key, long value = 1)
    {
        try
        {
            return await _database.StringIncrementAsync(key, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing value in Redis for key: {Key}", key);
            return 0;
        }
    }
}
```

---

## 📊 **Monitoreo de Performance**

### **Métricas y Health Checks**

```csharp
public interface IPerformanceMonitor
{
    void RecordOperation(string operation, TimeSpan duration, bool success);
    void RecordCacheHit(string cacheKey);
    void RecordCacheMiss(string cacheKey);
    PerformanceMetrics GetMetrics();
}

public class PerformanceMonitor : IPerformanceMonitor
{
    private readonly ConcurrentDictionary<string, OperationMetrics> _operationMetrics;
    private readonly ConcurrentDictionary<string, long> _cacheHits;
    private readonly ConcurrentDictionary<string, long> _cacheMisses;

    public PerformanceMonitor()
    {
        _operationMetrics = new ConcurrentDictionary<string, OperationMetrics>();
        _cacheHits = new ConcurrentDictionary<string, long>();
        _cacheMisses = new ConcurrentDictionary<string, long>();
    }

    public void RecordOperation(string operation, TimeSpan duration, bool success)
    {
        var metrics = _operationMetrics.GetOrAdd(operation, _ => new OperationMetrics());
        metrics.RecordOperation(duration, success);
    }

    public void RecordCacheHit(string cacheKey)
    {
        _cacheHits.AddOrUpdate(cacheKey, 1, (key, count) => count + 1);
    }

    public void RecordCacheMiss(string cacheKey)
    {
        _cacheMisses.AddOrUpdate(cacheKey, 1, (key, count) => count + 1);
    }

    public PerformanceMetrics GetMetrics()
    {
        return new PerformanceMetrics
        {
            Operations = _operationMetrics.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.GetSnapshot()),
            CacheHits = _cacheHits.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            CacheMisses = _cacheMisses.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            Timestamp = DateTime.UtcNow
        };
    }
}

public class PerformanceMetrics
{
    public Dictionary<string, OperationMetricsSnapshot> Operations { get; set; }
    public Dictionary<string, long> CacheHits { get; set; }
    public Dictionary<string, long> CacheMisses { get; set; }
    public DateTime Timestamp { get; set; }
    
    public double GetOverallCacheHitRate()
    {
        var totalHits = CacheHits.Values.Sum();
        var totalMisses = CacheMisses.Values.Sum();
        var total = totalHits + totalMisses;
        
        return total > 0 ? (double)totalHits / total : 0;
    }
}
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Sistema de Cache Multi-Nivel**
```csharp
// Implementa un sistema que:
// - Combine cache en memoria y distribuido
// - Maneje fallos de cache de forma elegante
// - Implemente estrategias de invalidación inteligente
```

### **Ejercicio 2: Optimización de Consultas**
```csharp
// Optimiza consultas que:
// - Utilicen índices apropiados
// - Implementen paginación eficiente
// - Apliquen técnicas de query optimization
```

### **Ejercicio 3: Monitoreo de Performance**
```csharp
// Crea un sistema que:
// - Monitoree métricas en tiempo real
// - Detecte cuellos de botella
// - Genere alertas automáticas
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **🗄️ Cache Multi-Nivel**: Sistema inteligente que combina memoria y cache distribuido
2. **🚀 Optimización de Consultas**: Técnicas avanzadas para mejorar rendimiento de base de datos
3. **🔄 Cache Distribuido**: Implementación robusta con Redis
4. **📊 Monitoreo de Performance**: Métricas en tiempo real y health checks
5. **⚡ Estrategias de Optimización**: Patrones para maximizar rendimiento

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Monitoreo y Observabilidad**, implementando sistemas de logging, métricas y trazabilidad distribuida.

---

**¡Has completado la octava clase del Módulo 14! ⚡🗄️**
