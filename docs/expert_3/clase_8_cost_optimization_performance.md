# 💰 **Clase 8: Cost Optimization y Performance**

## 🎯 **Objetivo de la Clase**
Optimizar costos y rendimiento en aplicaciones cloud native, implementando estrategias de escalabilidad, monitoreo de costos y optimización de recursos.

## 📚 **Contenido Teórico**

### **1. Estrategias de Cost Optimization**

#### **Auto-scaling y Resource Management**
```csharp
// Services/AutoScalingService.cs
public class AutoScalingService
{
    private readonly ILogger<AutoScalingService> _logger;
    private readonly IMemoryCache _cache;

    public AutoScalingService(ILogger<AutoScalingService> logger, IMemoryCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    public async Task<ScalingDecision> EvaluateScalingAsync(string serviceName, ScalingMetrics metrics)
    {
        try
        {
            var scalingDecision = new ScalingDecision
            {
                ServiceName = serviceName,
                Timestamp = DateTime.UtcNow,
                CurrentInstances = metrics.CurrentInstances,
                TargetInstances = metrics.CurrentInstances
            };

            // Evaluar CPU
            if (metrics.CpuUsage > 80)
            {
                scalingDecision.TargetInstances = Math.Min(metrics.CurrentInstances * 2, 10);
                scalingDecision.Reason = "High CPU usage";
                scalingDecision.Action = ScalingAction.ScaleOut;
            }
            else if (metrics.CpuUsage < 20 && metrics.CurrentInstances > 1)
            {
                scalingDecision.TargetInstances = Math.Max(metrics.CurrentInstances / 2, 1);
                scalingDecision.Reason = "Low CPU usage";
                scalingDecision.Action = ScalingAction.ScaleIn;
            }

            // Evaluar memoria
            if (metrics.MemoryUsage > 85)
            {
                scalingDecision.TargetInstances = Math.Min(metrics.CurrentInstances * 2, 10);
                scalingDecision.Reason = "High memory usage";
                scalingDecision.Action = ScalingAction.ScaleOut;
            }

            // Evaluar requests por segundo
            if (metrics.RequestsPerSecond > 1000)
            {
                scalingDecision.TargetInstances = Math.Min(metrics.CurrentInstances * 2, 10);
                scalingDecision.Reason = "High request volume";
                scalingDecision.Action = ScalingAction.ScaleOut;
            }

            // Evaluar latencia
            if (metrics.AverageLatency > 2000) // 2 segundos
            {
                scalingDecision.TargetInstances = Math.Min(metrics.CurrentInstances * 2, 10);
                scalingDecision.Reason = "High latency";
                scalingDecision.Action = ScalingAction.ScaleOut;
            }

            if (scalingDecision.Action != ScalingAction.NoAction)
            {
                await LogScalingDecisionAsync(scalingDecision);
            }

            return scalingDecision;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating scaling for service: {ServiceName}", serviceName);
            throw;
        }
    }

    public async Task<CostOptimizationRecommendation> AnalyzeCostsAsync(string serviceName, CostMetrics metrics)
    {
        try
        {
            var recommendation = new CostOptimizationRecommendation
            {
                ServiceName = serviceName,
                Timestamp = DateTime.UtcNow,
                CurrentMonthlyCost = metrics.MonthlyCost,
                PotentialSavings = 0,
                Recommendations = new List<string>()
            };

            // Analizar uso de recursos
            if (metrics.CpuUtilization < 30)
            {
                recommendation.PotentialSavings += metrics.MonthlyCost * 0.3;
                recommendation.Recommendations.Add("Consider downsizing instances due to low CPU utilization");
            }

            if (metrics.MemoryUtilization < 40)
            {
                recommendation.PotentialSavings += metrics.MonthlyCost * 0.2;
                recommendation.Recommendations.Add("Consider reducing memory allocation");
            }

            if (metrics.StorageUtilization < 50)
            {
                recommendation.PotentialSavings += metrics.MonthlyCost * 0.1;
                recommendation.Recommendations.Add("Consider reducing storage allocation");
            }

            // Analizar patrones de uso
            if (metrics.PeakHoursUsage < 0.5)
            {
                recommendation.PotentialSavings += metrics.MonthlyCost * 0.4;
                recommendation.Recommendations.Add("Consider using spot instances for non-critical workloads");
            }

            return recommendation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing costs for service: {ServiceName}", serviceName);
            throw;
        }
    }

    private async Task LogScalingDecisionAsync(ScalingDecision decision)
    {
        _logger.LogInformation("Scaling decision: {ServiceName} - {Action} from {Current} to {Target} instances. Reason: {Reason}",
            decision.ServiceName, decision.Action, decision.CurrentInstances, decision.TargetInstances, decision.Reason);
    }
}

// Models/ScalingMetrics.cs
public class ScalingMetrics
{
    public int CurrentInstances { get; set; }
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double RequestsPerSecond { get; set; }
    public double AverageLatency { get; set; }
    public double ErrorRate { get; set; }
}

// Models/ScalingDecision.cs
public class ScalingDecision
{
    public string ServiceName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int CurrentInstances { get; set; }
    public int TargetInstances { get; set; }
    public ScalingAction Action { get; set; }
    public string Reason { get; set; } = string.Empty;
}

// Enums/ScalingAction.cs
public enum ScalingAction
{
    NoAction,
    ScaleOut,
    ScaleIn
}
```

### **2. Performance Monitoring**

#### **Servicio de Monitoreo de Performance**
```csharp
// Services/PerformanceMonitoringService.cs
public class PerformanceMonitoringService
{
    private readonly ILogger<PerformanceMonitoringService> _logger;
    private readonly IMemoryCache _cache;

    public PerformanceMonitoringService(ILogger<PerformanceMonitoringService> logger, IMemoryCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    public async Task<PerformanceMetrics> CollectMetricsAsync(string serviceName)
    {
        try
        {
            var metrics = new PerformanceMetrics
            {
                ServiceName = serviceName,
                Timestamp = DateTime.UtcNow,
                CpuUsage = await GetCpuUsageAsync(),
                MemoryUsage = await GetMemoryUsageAsync(),
                RequestCount = await GetRequestCountAsync(serviceName),
                ErrorCount = await GetErrorCountAsync(serviceName),
                AverageResponseTime = await GetAverageResponseTimeAsync(serviceName),
                ActiveConnections = await GetActiveConnectionsAsync(serviceName)
            };

            // Calcular métricas derivadas
            metrics.ErrorRate = metrics.RequestCount > 0 ? (double)metrics.ErrorCount / metrics.RequestCount * 100 : 0;
            metrics.Throughput = metrics.RequestCount / 60; // requests per minute

            // Almacenar métricas en cache
            var cacheKey = $"metrics_{serviceName}_{DateTime.UtcNow:yyyyMMddHHmm}";
            _cache.Set(cacheKey, metrics, TimeSpan.FromMinutes(5));

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting metrics for service: {ServiceName}", serviceName);
            throw;
        }
    }

    public async Task<PerformanceAlert> CheckPerformanceThresholdsAsync(PerformanceMetrics metrics)
    {
        try
        {
            var alert = new PerformanceAlert
            {
                ServiceName = metrics.ServiceName,
                Timestamp = DateTime.UtcNow,
                Severity = AlertSeverity.Info,
                Alerts = new List<string>()
            };

            // Verificar umbrales de CPU
            if (metrics.CpuUsage > 90)
            {
                alert.Severity = AlertSeverity.Critical;
                alert.Alerts.Add($"Critical CPU usage: {metrics.CpuUsage:F2}%");
            }
            else if (metrics.CpuUsage > 80)
            {
                alert.Severity = AlertSeverity.Warning;
                alert.Alerts.Add($"High CPU usage: {metrics.CpuUsage:F2}%");
            }

            // Verificar umbrales de memoria
            if (metrics.MemoryUsage > 90)
            {
                alert.Severity = AlertSeverity.Critical;
                alert.Alerts.Add($"Critical memory usage: {metrics.MemoryUsage:F2}%");
            }
            else if (metrics.MemoryUsage > 80)
            {
                alert.Severity = AlertSeverity.Warning;
                alert.Alerts.Add($"High memory usage: {metrics.MemoryUsage:F2}%");
            }

            // Verificar tasa de errores
            if (metrics.ErrorRate > 10)
            {
                alert.Severity = AlertSeverity.Critical;
                alert.Alerts.Add($"High error rate: {metrics.ErrorRate:F2}%");
            }
            else if (metrics.ErrorRate > 5)
            {
                alert.Severity = AlertSeverity.Warning;
                alert.Alerts.Add($"Elevated error rate: {metrics.ErrorRate:F2}%");
            }

            // Verificar tiempo de respuesta
            if (metrics.AverageResponseTime > 5000) // 5 segundos
            {
                alert.Severity = AlertSeverity.Critical;
                alert.Alerts.Add($"High response time: {metrics.AverageResponseTime:F2}ms");
            }
            else if (metrics.AverageResponseTime > 2000) // 2 segundos
            {
                alert.Severity = AlertSeverity.Warning;
                alert.Alerts.Add($"Elevated response time: {metrics.AverageResponseTime:F2}ms");
            }

            if (alert.Alerts.Any())
            {
                await LogPerformanceAlertAsync(alert);
            }

            return alert;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking performance thresholds for service: {ServiceName}", metrics.ServiceName);
            throw;
        }
    }

    private async Task<double> GetCpuUsageAsync()
    {
        // Implementar obtención de uso de CPU
        return await Task.FromResult(45.5);
    }

    private async Task<double> GetMemoryUsageAsync()
    {
        // Implementar obtención de uso de memoria
        return await Task.FromResult(67.8);
    }

    private async Task<int> GetRequestCountAsync(string serviceName)
    {
        // Implementar obtención de conteo de requests
        return await Task.FromResult(1250);
    }

    private async Task<int> GetErrorCountAsync(string serviceName)
    {
        // Implementar obtención de conteo de errores
        return await Task.FromResult(25);
    }

    private async Task<double> GetAverageResponseTimeAsync(string serviceName)
        {
        // Implementar obtención de tiempo de respuesta promedio
        return await Task.FromResult(850.5);
    }

    private async Task<int> GetActiveConnectionsAsync(string serviceName)
    {
        // Implementar obtención de conexiones activas
        return await Task.FromResult(150);
    }

    private async Task LogPerformanceAlertAsync(PerformanceAlert alert)
    {
        _logger.LogWarning("Performance alert for {ServiceName}: {Severity} - {Alerts}",
            alert.ServiceName, alert.Severity, string.Join(", ", alert.Alerts));
    }
}

// Models/PerformanceMetrics.cs
public class PerformanceMetrics
{
    public string ServiceName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public int RequestCount { get; set; }
    public int ErrorCount { get; set; }
    public double ErrorRate { get; set; }
    public double AverageResponseTime { get; set; }
    public double Throughput { get; set; }
    public int ActiveConnections { get; set; }
}

// Models/PerformanceAlert.cs
public class PerformanceAlert
{
    public string ServiceName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public AlertSeverity Severity { get; set; }
    public List<string> Alerts { get; set; } = new();
}

// Enums/AlertSeverity.cs
public enum AlertSeverity
{
    Info,
    Warning,
    Critical
}
```

### **3. Resource Optimization**

#### **Servicio de Optimización de Recursos**
```csharp
// Services/ResourceOptimizationService.cs
public class ResourceOptimizationService
{
    private readonly ILogger<ResourceOptimizationService> _logger;
    private readonly IMemoryCache _cache;

    public ResourceOptimizationService(ILogger<ResourceOptimizationService> logger, IMemoryCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    public async Task<ResourceOptimizationReport> AnalyzeResourceUsageAsync(string serviceName, ResourceMetrics metrics)
    {
        try
        {
            var report = new ResourceOptimizationReport
            {
                ServiceName = serviceName,
                Timestamp = DateTime.UtcNow,
                CurrentResources = metrics,
                OptimizationRecommendations = new List<OptimizationRecommendation>(),
                PotentialSavings = 0
            };

            // Analizar CPU
            if (metrics.CpuUtilization < 30)
            {
                report.OptimizationRecommendations.Add(new OptimizationRecommendation
                {
                    ResourceType = "CPU",
                    CurrentValue = metrics.CpuCores,
                    RecommendedValue = Math.Max(1, metrics.CpuCores / 2),
                    Reason = "Low CPU utilization",
                    PotentialSavings = metrics.CpuCores * 0.5 * 0.1 // 50% reduction, 10% cost per core
                });
            }

            // Analizar memoria
            if (metrics.MemoryUtilization < 40)
            {
                report.OptimizationRecommendations.Add(new OptimizationRecommendation
                {
                    ResourceType = "Memory",
                    CurrentValue = metrics.MemoryGB,
                    RecommendedValue = Math.Max(1, metrics.MemoryGB / 2),
                    Reason = "Low memory utilization",
                    PotentialSavings = metrics.MemoryGB * 0.5 * 0.05 // 50% reduction, 5% cost per GB
                });
            }

            // Analizar almacenamiento
            if (metrics.StorageUtilization < 50)
            {
                report.OptimizationRecommendations.Add(new OptimizationRecommendation
                {
                    ResourceType = "Storage",
                    CurrentValue = metrics.StorageGB,
                    RecommendedValue = Math.Max(10, metrics.StorageGB / 2),
                    Reason = "Low storage utilization",
                    PotentialSavings = metrics.StorageGB * 0.5 * 0.02 // 50% reduction, 2% cost per GB
                });
            }

            // Analizar red
            if (metrics.NetworkUtilization < 20)
            {
                report.OptimizationRecommendations.Add(new OptimizationRecommendation
                {
                    ResourceType = "Network",
                    CurrentValue = metrics.NetworkBandwidth,
                    RecommendedValue = Math.Max(100, metrics.NetworkBandwidth / 2),
                    Reason = "Low network utilization",
                    PotentialSavings = metrics.NetworkBandwidth * 0.5 * 0.01 // 50% reduction, 1% cost per Mbps
                });
            }

            // Calcular ahorros totales
            report.PotentialSavings = report.OptimizationRecommendations.Sum(r => r.PotentialSavings);

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing resource usage for service: {ServiceName}", serviceName);
            throw;
        }
    }

    public async Task<CostAnalysis> AnalyzeCostsAsync(string serviceName, CostMetrics metrics)
    {
        try
        {
            var analysis = new CostAnalysis
            {
                ServiceName = serviceName,
                Timestamp = DateTime.UtcNow,
                CurrentMonthlyCost = metrics.MonthlyCost,
                CostBreakdown = new Dictionary<string, decimal>(),
                CostTrends = new List<CostTrend>(),
                OptimizationOpportunities = new List<CostOptimizationOpportunity>()
            };

            // Desglose de costos
            analysis.CostBreakdown.Add("Compute", metrics.ComputeCost);
            analysis.CostBreakdown.Add("Storage", metrics.StorageCost);
            analysis.CostBreakdown.Add("Network", metrics.NetworkCost);
            analysis.CostBreakdown.Add("Database", metrics.DatabaseCost);
            analysis.CostBreakdown.Add("Other", metrics.OtherCost);

            // Identificar oportunidades de optimización
            if (metrics.ComputeCost > metrics.MonthlyCost * 0.6)
            {
                analysis.OptimizationOpportunities.Add(new CostOptimizationOpportunity
                {
                    Category = "Compute",
                    Description = "High compute costs - consider reserved instances or spot instances",
                    PotentialSavings = metrics.ComputeCost * 0.3,
                    Priority = OptimizationPriority.High
                });
            }

            if (metrics.StorageCost > metrics.MonthlyCost * 0.3)
            {
                analysis.OptimizationOpportunities.Add(new CostOptimizationOpportunity
                {
                    Category = "Storage",
                    Description = "High storage costs - consider lifecycle policies and compression",
                    PotentialSavings = metrics.StorageCost * 0.4,
                    Priority = OptimizationPriority.Medium
                });
            }

            if (metrics.NetworkCost > metrics.MonthlyCost * 0.2)
            {
                analysis.OptimizationOpportunities.Add(new CostOptimizationOpportunity
                {
                    Category = "Network",
                    Description = "High network costs - consider CDN and data transfer optimization",
                    PotentialSavings = metrics.NetworkCost * 0.5,
                    Priority = OptimizationPriority.Medium
                });
            }

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing costs for service: {ServiceName}", serviceName);
            throw;
        }
    }
}

// Models/ResourceMetrics.cs
public class ResourceMetrics
{
    public int CpuCores { get; set; }
    public double CpuUtilization { get; set; }
    public int MemoryGB { get; set; }
    public double MemoryUtilization { get; set; }
    public int StorageGB { get; set; }
    public double StorageUtilization { get; set; }
    public int NetworkBandwidth { get; set; }
    public double NetworkUtilization { get; set; }
}

// Models/ResourceOptimizationReport.cs
public class ResourceOptimizationReport
{
    public string ServiceName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public ResourceMetrics CurrentResources { get; set; } = new();
    public List<OptimizationRecommendation> OptimizationRecommendations { get; set; } = new();
    public decimal PotentialSavings { get; set; }
}

// Models/OptimizationRecommendation.cs
public class OptimizationRecommendation
{
    public string ResourceType { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public double RecommendedValue { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal PotentialSavings { get; set; }
}
```

### **4. Caching Strategies**

#### **Servicio de Caching Inteligente**
```csharp
// Services/IntelligentCachingService.cs
public class IntelligentCachingService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<IntelligentCachingService> _logger;

    public IntelligentCachingService(
        IMemoryCache memoryCache, 
        IDistributedCache distributedCache, 
        ILogger<IntelligentCachingService> logger)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, CacheOptions options = null)
    {
        try
        {
            options ??= new CacheOptions();

            // Intentar obtener desde cache local primero
            if (_memoryCache.TryGetValue(key, out T cachedValue))
            {
                _logger.LogDebug("Cache hit (local): {Key}", key);
                return cachedValue;
            }

            // Intentar obtener desde cache distribuido
            var distributedValue = await _distributedCache.GetStringAsync(key);
            if (!string.IsNullOrEmpty(distributedValue))
            {
                var deserializedValue = JsonSerializer.Deserialize<T>(distributedValue);
                
                // Almacenar en cache local
                _memoryCache.Set(key, deserializedValue, options.LocalExpiration);
                
                _logger.LogDebug("Cache hit (distributed): {Key}", key);
                return deserializedValue;
            }

            // Cache miss - obtener datos frescos
            var freshValue = await factory();
            
            // Almacenar en ambos caches
            var serializedValue = JsonSerializer.Serialize(freshValue);
            await _distributedCache.SetStringAsync(key, serializedValue, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = options.DistributedExpiration
            });
            
            _memoryCache.Set(key, freshValue, options.LocalExpiration);
            
            _logger.LogDebug("Cache miss - data fetched and cached: {Key}", key);
            return freshValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in caching operation for key: {Key}", key);
            // En caso de error, intentar obtener datos frescos
            return await factory();
        }
    }

    public async Task InvalidateAsync(string key)
    {
        try
        {
            _memoryCache.Remove(key);
            await _distributedCache.RemoveAsync(key);
            
            _logger.LogInformation("Cache invalidated: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for key: {Key}", key);
        }
    }

    public async Task InvalidatePatternAsync(string pattern)
    {
        try
        {
            // Implementar invalidación por patrón
            // Esto requeriría un índice de claves o un patrón específico
            _logger.LogInformation("Cache pattern invalidated: {Pattern}", pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache pattern: {Pattern}", pattern);
        }
    }

    public async Task<CacheStatistics> GetCacheStatisticsAsync()
    {
        try
        {
            var statistics = new CacheStatistics
            {
                Timestamp = DateTime.UtcNow,
                LocalCacheHits = 0, // Implementar contadores
                LocalCacheMisses = 0,
                DistributedCacheHits = 0,
                DistributedCacheMisses = 0,
                TotalRequests = 0
            };

            // Calcular tasas de acierto
            statistics.LocalHitRate = statistics.TotalRequests > 0 
                ? (double)statistics.LocalCacheHits / statistics.TotalRequests * 100 
                : 0;
            
            statistics.DistributedHitRate = statistics.TotalRequests > 0 
                ? (double)statistics.DistributedCacheHits / statistics.TotalRequests * 100 
                : 0;

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache statistics");
            throw;
        }
    }
}

// Models/CacheOptions.cs
public class CacheOptions
{
    public TimeSpan LocalExpiration { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan DistributedExpiration { get; set; } = TimeSpan.FromHours(1);
    public bool UseLocalCache { get; set; } = true;
    public bool UseDistributedCache { get; set; } = true;
}

// Models/CacheStatistics.cs
public class CacheStatistics
{
    public DateTime Timestamp { get; set; }
    public long LocalCacheHits { get; set; }
    public long LocalCacheMisses { get; set; }
    public long DistributedCacheHits { get; set; }
    public long DistributedCacheMisses { get; set; }
    public long TotalRequests { get; set; }
    public double LocalHitRate { get; set; }
    public double DistributedHitRate { get; set; }
}
```

## 🛠️ **Ejercicio Práctico**

### **Ejercicio 1: Implementar Sistema de Optimización Completo**

Crea un sistema completo de optimización de costos y rendimiento:

```csharp
// 1. Configurar servicios de optimización
public class OptimizationServiceConfig
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Auto-scaling
        services.AddSingleton<AutoScalingService>();
        
        // Performance monitoring
        services.AddSingleton<PerformanceMonitoringService>();
        
        // Resource optimization
        services.AddSingleton<ResourceOptimizationService>();
        
        // Intelligent caching
        services.AddMemoryCache();
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });
        services.AddSingleton<IntelligentCachingService>();
    }
}

// 2. Implementar controlador de optimización
[ApiController]
[Route("api/[controller]")]
public class OptimizationController : ControllerBase
{
    private readonly AutoScalingService _autoScalingService;
    private readonly PerformanceMonitoringService _performanceService;
    private readonly ResourceOptimizationService _resourceService;

    [HttpGet("metrics/{serviceName}")]
    public async Task<ActionResult<PerformanceMetrics>> GetMetrics(string serviceName)
    {
        var metrics = await _performanceService.CollectMetricsAsync(serviceName);
        return Ok(metrics);
    }

    [HttpGet("scaling/{serviceName}")]
    public async Task<ActionResult<ScalingDecision>> GetScalingDecision(string serviceName)
    {
        var metrics = await _performanceService.CollectMetricsAsync(serviceName);
        var scalingMetrics = new ScalingMetrics
        {
            CurrentInstances = 2,
            CpuUsage = metrics.CpuUsage,
            MemoryUsage = metrics.MemoryUsage,
            RequestsPerSecond = metrics.Throughput,
            AverageLatency = metrics.AverageResponseTime,
            ErrorRate = metrics.ErrorRate
        };
        
        var decision = await _autoScalingService.EvaluateScalingAsync(serviceName, scalingMetrics);
        return Ok(decision);
    }

    [HttpGet("cost-analysis/{serviceName}")]
    public async Task<ActionResult<CostAnalysis>> GetCostAnalysis(string serviceName)
    {
        var costMetrics = new CostMetrics
        {
            MonthlyCost = 1000,
            ComputeCost = 600,
            StorageCost = 200,
            NetworkCost = 100,
            DatabaseCost = 80,
            OtherCost = 20
        };
        
        var analysis = await _resourceService.AnalyzeCostsAsync(serviceName, costMetrics);
        return Ok(analysis);
    }
}

// 3. Implementar middleware de optimización
public class OptimizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IntelligentCachingService _cachingService;
    private readonly ILogger<OptimizationMiddleware> _logger;

    public OptimizationMiddleware(RequestDelegate next, IntelligentCachingService cachingService, ILogger<OptimizationMiddleware> logger)
    {
        _next = next;
        _cachingService = cachingService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            // Log performance metrics
            _logger.LogInformation("Request completed in {ElapsedMs}ms: {Method} {Path}",
                stopwatch.ElapsedMilliseconds, context.Request.Method, context.Request.Path);
        }
    }
}
```

## 📋 **Resumen de la Clase**

### **Conceptos Clave:**
- **Auto-scaling**: Escalabilidad automática
- **Performance Monitoring**: Monitoreo de rendimiento
- **Resource Optimization**: Optimización de recursos
- **Cost Analysis**: Análisis de costos
- **Intelligent Caching**: Caching inteligente
- **Cost Optimization**: Optimización de costos

### **Próxima Clase:**
**Monitoring y Observabilidad en la Nube** - Monitoreo y observabilidad

## 🎯 **Objetivos de Aprendizaje**

Al finalizar esta clase, serás capaz de:
- ✅ Implementar auto-scaling
- ✅ Monitorear performance
- ✅ Optimizar recursos
- ✅ Analizar costos
- ✅ Implementar caching inteligente
- ✅ Optimizar costos de la nube
