# 🧪 Clase 9: Testing en Producción

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 8: Backup y Disaster Recovery](../senior_8/clase_8_backup_disaster_recovery.md)
- **🏠 Inicio del Módulo**: [Módulo 15: Sistemas Avanzados y Distribuidos](../senior_8/README.md)
- **➡️ Siguiente**: [Clase 10: Proyecto Final Integrador](../senior_8/clase_10_proyecto_final_integrador.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** smoke tests automatizados
2. **Configurar** load testing y stress testing
3. **Desarrollar** chaos engineering experiments
4. **Aplicar** A/B testing y feature flags
5. **Optimizar** testing en entornos de producción

---

## 🚬 **Smoke Tests Automatizados**

### **Servicio de Smoke Testing**

```csharp
// MusicalMatching.Application/Services/SmokeTestService.cs
using System.Net.Http.Json;

namespace MusicalMatching.Application.Services;

public interface ISmokeTestService
{
    Task<SmokeTestResult> RunSmokeTestsAsync();
    Task<SmokeTestResult> TestEndpointAsync(string endpoint, string method = "GET");
    Task<SmokeTestResult> TestDatabaseConnectionAsync();
    Task<SmokeTestResult> TestExternalServicesAsync();
}

public class SmokeTestService : ISmokeTestService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmokeTestService> _logger;
    private readonly string _baseUrl;

    public SmokeTestService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<SmokeTestService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _baseUrl = _configuration["Application:BaseUrl"] ?? "https://localhost:5001";
    }

    public async Task<SmokeTestResult> RunSmokeTestsAsync()
    {
        var results = new List<SmokeTestResult>();
        var overallSuccess = true;

        _logger.LogInformation("Starting smoke tests for {BaseUrl}", _baseUrl);

        // Test health endpoint
        var healthResult = await TestEndpointAsync("/health");
        results.Add(healthResult);
        overallSuccess &= healthResult.Success;

        // Test API endpoint
        var apiResult = await TestEndpointAsync("/api/health");
        results.Add(apiResult);
        overallSuccess &= apiResult.Success;

        // Test database connection
        var dbResult = await TestDatabaseConnectionAsync();
        results.Add(dbResult);
        overallSuccess &= dbResult.Success;

        // Test external services
        var externalResult = await TestExternalServicesAsync();
        results.Add(externalResult);
        overallSuccess &= externalResult.Success;

        var summary = new SmokeTestResult
        {
            Success = overallSuccess,
            TestName = "Smoke Test Suite",
            Duration = TimeSpan.FromMilliseconds(results.Sum(r => r.Duration.TotalMilliseconds)),
            Details = $"Executed {results.Count} tests. Success: {results.Count(r => r.Success)}/{results.Count}",
            SubResults = results
        };

        _logger.LogInformation("Smoke tests completed. Overall success: {Success}", overallSuccess);
        return summary;
    }

    public async Task<SmokeTestResult> TestEndpointAsync(string endpoint, string method = "GET")
    {
        var stopwatch = Stopwatch.StartNew();
        var fullUrl = $"{_baseUrl}{endpoint}";

        try
        {
            var request = new HttpRequestMessage(new HttpMethod(method), fullUrl);
            var response = await _httpClient.SendAsync(request);
            
            stopwatch.Stop();

            var success = response.IsSuccessStatusCode;
            var result = new SmokeTestResult
            {
                Success = success,
                TestName = $"HTTP {method} {endpoint}",
                Duration = stopwatch.Elapsed,
                Details = $"Status: {response.StatusCode}, Duration: {stopwatch.ElapsedMilliseconds}ms"
            };

            if (!success)
            {
                result.ErrorMessage = $"HTTP {response.StatusCode}: {response.ReasonPhrase}";
                _logger.LogWarning("Smoke test failed for {Endpoint}: {StatusCode}", endpoint, response.StatusCode);
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Smoke test failed for {Endpoint}", endpoint);

            return new SmokeTestResult
            {
                Success = false,
                TestName = $"HTTP {method} {endpoint}",
                Duration = stopwatch.Elapsed,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<SmokeTestResult> TestDatabaseConnectionAsync()
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Test database health endpoint
            var result = await TestEndpointAsync("/health/db");
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Database connection test failed");

            return new SmokeTestResult
            {
                Success = false,
                TestName = "Database Connection",
                Duration = stopwatch.Elapsed,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<SmokeTestResult> TestExternalServicesAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        var externalServices = new[] { "redis", "elasticsearch" };
        var results = new List<SmokeTestResult>();

        foreach (var service in externalServices)
        {
            var result = await TestEndpointAsync($"/health/{service}");
            results.Add(result);
        }

        stopwatch.Stop();

        var overallSuccess = results.All(r => r.Success);
        return new SmokeTestResult
        {
            Success = overallSuccess,
            TestName = "External Services",
            Duration = stopwatch.Elapsed,
            Details = $"Tested {externalServices.Length} services. Success: {results.Count(r => r.Success)}/{results.Count}",
            SubResults = results
        };
    }
}

public class SmokeTestResult
{
    public bool Success { get; set; }
    public string TestName { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public string Details { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public List<SmokeTestResult> SubResults { get; set; } = new();
}
```

---

## 📊 **Load Testing y Stress Testing**

### **Servicio de Load Testing**

```csharp
// MusicalMatching.Application/Services/LoadTestService.cs
using System.Collections.Concurrent;

namespace MusicalMatching.Application.Services;

public interface ILoadTestService
{
    Task<LoadTestResult> RunLoadTestAsync(LoadTestConfiguration config);
    Task<LoadTestResult> RunStressTestAsync(StressTestConfiguration config);
    Task<LoadTestResult> RunSpikeTestAsync(SpikeTestConfiguration config);
}

public class LoadTestService : ILoadTestService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LoadTestService> _logger;
    private readonly string _baseUrl;

    public LoadTestService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<LoadTestService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration["Application:BaseUrl"] ?? "https://localhost:5001";
    }

    public async Task<LoadTestResult> RunLoadTestAsync(LoadTestConfiguration config)
    {
        _logger.LogInformation("Starting load test: {VirtualUsers} users for {Duration} seconds", 
            config.VirtualUsers, config.Duration.TotalSeconds);

        var results = new ConcurrentBag<LoadTestResult>();
        var startTime = DateTime.UtcNow;
        var endTime = startTime.Add(config.Duration);

        var tasks = new List<Task>();
        var semaphore = new SemaphoreSlim(config.VirtualUsers, config.VirtualUsers);

        while (DateTime.UtcNow < endTime)
        {
            await semaphore.WaitAsync();
            
            var task = Task.Run(async () =>
            {
                try
                {
                    var result = await ExecuteLoadTestRequestAsync(config.Endpoint, config.Method);
                    results.Add(result);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            tasks.Add(task);
            await Task.Delay(config.RequestInterval);
        }

        await Task.WhenAll(tasks);

        var allResults = results.ToList();
        var summary = CreateLoadTestSummary(allResults, startTime, endTime);

        _logger.LogInformation("Load test completed. Total requests: {TotalRequests}, Success rate: {SuccessRate}%", 
            summary.TotalRequests, summary.SuccessRate);

        return summary;
    }

    public async Task<LoadTestResult> RunStressTestAsync(StressTestConfiguration config)
    {
        _logger.LogInformation("Starting stress test: {InitialUsers} to {MaxUsers} users", 
            config.InitialUsers, config.MaxUsers);

        var currentUsers = config.InitialUsers;
        var results = new ConcurrentBag<LoadTestResult>();
        var startTime = DateTime.UtcNow;

        while (currentUsers <= config.MaxUsers)
        {
            var configForCurrentUsers = new LoadTestConfiguration
            {
                VirtualUsers = currentUsers,
                Duration = config.StepDuration,
                Endpoint = config.Endpoint,
                Method = config.Method,
                RequestInterval = config.RequestInterval
            };

            var stepResult = await RunLoadTestAsync(configForCurrentUsers);
            results.Add(stepResult);

            // Check if we've hit the breaking point
            if (stepResult.SuccessRate < config.SuccessRateThreshold)
            {
                _logger.LogWarning("Stress test breaking point reached at {Users} users. Success rate: {SuccessRate}%", 
                    currentUsers, stepResult.SuccessRate);
                break;
            }

            currentUsers += config.UserIncrement;
            await Task.Delay(config.StepDelay);
        }

        var allResults = results.ToList();
        var summary = CreateLoadTestSummary(allResults, startTime, DateTime.UtcNow);
        summary.TestType = "Stress Test";

        return summary;
    }

    public async Task<LoadTestResult> RunSpikeTestAsync(SpikeTestConfiguration config)
    {
        _logger.LogInformation("Starting spike test: {SpikeUsers} users for {SpikeDuration} seconds", 
            config.SpikeUsers, config.SpikeDuration.TotalSeconds);

        var results = new ConcurrentBag<LoadTestResult>();

        // Normal load
        var normalConfig = new LoadTestConfiguration
        {
            VirtualUsers = config.NormalUsers,
            Duration = config.NormalDuration,
            Endpoint = config.Endpoint,
            Method = config.Method,
            RequestInterval = config.RequestInterval
        };

        var normalResult = await RunLoadTestAsync(normalConfig);
        results.Add(normalResult);

        // Spike load
        var spikeConfig = new LoadTestConfiguration
        {
            VirtualUsers = config.SpikeUsers,
            Duration = config.SpikeDuration,
            Endpoint = config.Endpoint,
            Method = config.Method,
            RequestInterval = config.RequestInterval
        };

        var spikeResult = await RunLoadTestAsync(spikeConfig);
        results.Add(spikeResult);

        // Return to normal load
        var returnToNormalResult = await RunLoadTestAsync(normalConfig);
        results.Add(returnToNormalResult);

        var allResults = results.ToList();
        var summary = CreateLoadTestSummary(allResults, DateTime.UtcNow.AddSeconds(-30), DateTime.UtcNow);
        summary.TestType = "Spike Test";

        return summary;
    }

    private async Task<LoadTestResult> ExecuteLoadTestRequestAsync(string endpoint, string method)
    {
        var stopwatch = Stopwatch.StartNew();
        var fullUrl = $"{_baseUrl}{endpoint}";

        try
        {
            var request = new HttpRequestMessage(new HttpMethod(method), fullUrl);
            var response = await _httpClient.SendAsync(request);
            
            stopwatch.Stop();

            return new LoadTestResult
            {
                Success = response.IsSuccessStatusCode,
                Duration = stopwatch.Elapsed,
                StatusCode = (int)response.StatusCode,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new LoadTestResult
            {
                Success = false,
                Duration = stopwatch.Elapsed,
                ErrorMessage = ex.Message,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    private LoadTestResult CreateLoadTestSummary(List<LoadTestResult> results, DateTime startTime, DateTime endTime)
    {
        var successfulRequests = results.Where(r => r.Success).ToList();
        var failedRequests = results.Where(r => !r.Success).ToList();

        var responseTimes = successfulRequests.Select(r => r.Duration.TotalMilliseconds).ToList();
        var avgResponseTime = responseTimes.Any() ? responseTimes.Average() : 0;
        var p95ResponseTime = responseTimes.Any() ? CalculatePercentile(responseTimes, 95) : 0;
        var p99ResponseTime = responseTimes.Any() ? CalculatePercentile(responseTimes, 99) : 0;

        return new LoadTestResult
        {
            Success = successfulRequests.Count > 0,
            TestType = "Load Test",
            TotalRequests = results.Count,
            SuccessfulRequests = successfulRequests.Count,
            FailedRequests = failedRequests.Count,
            SuccessRate = results.Count > 0 ? (double)successfulRequests.Count / results.Count * 100 : 0,
            AverageResponseTime = TimeSpan.FromMilliseconds(avgResponseTime),
            P95ResponseTime = TimeSpan.FromMilliseconds(p95ResponseTime),
            P99ResponseTime = TimeSpan.FromMilliseconds(p99ResponseTime),
            StartTime = startTime,
            EndTime = endTime,
            Duration = endTime - startTime
        };
    }

    private double CalculatePercentile(List<double> values, int percentile)
    {
        if (!values.Any()) return 0;
        
        values.Sort();
        var index = (int)Math.Ceiling((percentile / 100.0) * values.Count) - 1;
        return values[Math.Max(0, index)];
    }
}

public class LoadTestConfiguration
{
    public int VirtualUsers { get; set; } = 10;
    public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(5);
    public string Endpoint { get; set; } = "/api/health";
    public string Method { get; set; } = "GET";
    public TimeSpan RequestInterval { get; set; } = TimeSpan.FromSeconds(1);
}

public class StressTestConfiguration : LoadTestConfiguration
{
    public int InitialUsers { get; set; } = 10;
    public int MaxUsers { get; set; } = 100;
    public int UserIncrement { get; set; } = 10;
    public TimeSpan StepDuration { get; set; } = TimeSpan.FromMinutes(1);
    public TimeSpan StepDelay { get; set; } = TimeSpan.FromSeconds(30);
    public double SuccessRateThreshold { get; set; } = 95.0;
}

public class SpikeTestConfiguration : LoadTestConfiguration
{
    public int NormalUsers { get; set; } = 10;
    public int SpikeUsers { get; set; } = 50;
    public TimeSpan NormalDuration { get; set; } = TimeSpan.FromMinutes(2);
    public TimeSpan SpikeDuration { get; set; } = TimeSpan.FromMinutes(1);
}

public class LoadTestResult
{
    public bool Success { get; set; }
    public string TestType { get; set; } = "Load Test";
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public double SuccessRate { get; set; }
    public TimeSpan Duration { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
    public TimeSpan P95ResponseTime { get; set; }
    public TimeSpan P99ResponseTime { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime Timestamp { get; set; }
    public int StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
}
```

---

## 🌪️ **Chaos Engineering Experiments**

### **Servicio de Chaos Engineering**

```csharp
// MusicalMatching.Application/Services/ChaosEngineeringService.cs
namespace MusicalMatching.Application.Services;

public interface IChaosEngineeringService
{
    Task<ChaosExperimentResult> RunExperimentAsync(ChaosExperiment experiment);
    Task<List<ChaosExperiment>> GetAvailableExperimentsAsync();
    Task<bool> IsExperimentEnabledAsync(string experimentName);
}

public class ChaosEngineeringService : IChaosEngineeringService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChaosEngineeringService> _logger;
    private readonly ISmokeTestService _smokeTestService;
    private readonly ILoadTestService _loadTestService;

    public ChaosEngineeringService(
        IConfiguration configuration,
        ILogger<ChaosEngineeringService> logger,
        ISmokeTestService smokeTestService,
        ILoadTestService loadTestService)
    {
        _configuration = configuration;
        _logger = logger;
        _smokeTestService = smokeTestService;
        _loadTestService = loadTestService;
    }

    public async Task<ChaosExperimentResult> RunExperimentAsync(ChaosExperiment experiment)
    {
        if (!await IsExperimentEnabledAsync(experiment.Name))
        {
            _logger.LogWarning("Chaos experiment {ExperimentName} is disabled", experiment.Name);
            return new ChaosExperimentResult
            {
                Success = false,
                ExperimentName = experiment.Name,
                ErrorMessage = "Experiment is disabled"
            };
        }

        _logger.LogInformation("Starting chaos experiment: {ExperimentName}", experiment.Name);

        var result = new ChaosExperimentResult
        {
            ExperimentName = experiment.Name,
            StartTime = DateTime.UtcNow
        };

        try
        {
            // Baseline test
            var baselineResult = await _smokeTestService.RunSmokeTestsAsync();
            result.BaselineHealth = baselineResult.Success;

            // Inject chaos
            await InjectChaosAsync(experiment);
            result.ChaosInjected = true;

            // Wait for chaos to take effect
            await Task.Delay(experiment.ChaosDuration);

            // Test system under chaos
            var chaosResult = await _smokeTestService.RunSmokeTestsAsync();
            result.ChaosHealth = chaosResult.Success;

            // Stop chaos
            await StopChaosAsync(experiment);
            result.ChaosStopped = true;

            // Wait for recovery
            await Task.Delay(experiment.RecoveryDuration);

            // Test system recovery
            var recoveryResult = await _smokeTestService.RunSmokeTestsAsync();
            result.RecoveryHealth = recoveryResult.Success;

            result.Success = true;
            result.EndTime = DateTime.UtcNow;
            result.Duration = result.EndTime - result.StartTime;

            _logger.LogInformation("Chaos experiment {ExperimentName} completed successfully", experiment.Name);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.EndTime = DateTime.UtcNow;
            result.Duration = result.EndTime - result.StartTime;

            _logger.LogError(ex, "Chaos experiment {ExperimentName} failed", experiment.Name);

            // Ensure chaos is stopped even if experiment fails
            try
            {
                await StopChaosAsync(experiment);
            }
            catch (Exception stopEx)
            {
                _logger.LogError(stopEx, "Failed to stop chaos for experiment {ExperimentName}", experiment.Name);
            }
        }

        return result;
    }

    public async Task<List<ChaosExperiment>> GetAvailableExperimentsAsync()
    {
        return new List<ChaosExperiment>
        {
            new ChaosExperiment
            {
                Name = "Network Latency",
                Description = "Introduce network latency to simulate slow connections",
                Type = ChaosType.Network,
                ChaosDuration = TimeSpan.FromMinutes(2),
                RecoveryDuration = TimeSpan.FromMinutes(1)
            },
            new ChaosExperiment
            {
                Name = "Database Connection Failure",
                Description = "Simulate database connection failures",
                Type = ChaosType.Database,
                ChaosDuration = TimeSpan.FromMinutes(1),
                RecoveryDuration = TimeSpan.FromMinutes(2)
            },
            new ChaosExperiment
            {
                Name = "High CPU Load",
                Description = "Generate high CPU load to test system resilience",
                Type = ChaosType.System,
                ChaosDuration = TimeSpan.FromMinutes(3),
                RecoveryDuration = TimeSpan.FromMinutes(1)
            },
            new ChaosExperiment
            {
                Name = "Memory Pressure",
                Description = "Create memory pressure to test garbage collection",
                Type = ChaosType.System,
                ChaosDuration = TimeSpan.FromMinutes(2),
                RecoveryDuration = TimeSpan.FromMinutes(2)
            }
        };
    }

    public async Task<bool> IsExperimentEnabledAsync(string experimentName)
    {
        var enabledExperiments = _configuration.GetSection("ChaosEngineering:EnabledExperiments").Get<string[]>() ?? new string[0];
        return enabledExperiments.Contains(experimentName);
    }

    private async Task InjectChaosAsync(ChaosExperiment experiment)
    {
        _logger.LogInformation("Injecting chaos: {ExperimentName}", experiment.Name);

        switch (experiment.Type)
        {
            case ChaosType.Network:
                await InjectNetworkChaosAsync(experiment);
                break;
            case ChaosType.Database:
                await InjectDatabaseChaosAsync(experiment);
                break;
            case ChaosType.System:
                await InjectSystemChaosAsync(experiment);
                break;
            default:
                throw new NotSupportedException($"Chaos type {experiment.Type} is not supported");
        }
    }

    private async Task InjectNetworkChaosAsync(ChaosExperiment experiment)
    {
        // Simulate network latency using iptables (Linux) or similar
        // This is a simplified example
        _logger.LogInformation("Injecting network chaos for {ExperimentName}", experiment.Name);
        await Task.Delay(1000); // Simulate chaos injection
    }

    private async Task InjectDatabaseChaosAsync(ChaosExperiment experiment)
    {
        // Simulate database connection failures
        _logger.LogInformation("Injecting database chaos for {ExperimentName}", experiment.Name);
        await Task.Delay(1000); // Simulate chaos injection
    }

    private async Task InjectSystemChaosAsync(ChaosExperiment experiment)
    {
        // Simulate system resource pressure
        _logger.LogInformation("Injecting system chaos for {ExperimentName}", experiment.Name);
        await Task.Delay(1000); // Simulate chaos injection
    }

    private async Task StopChaosAsync(ChaosExperiment experiment)
    {
        _logger.LogInformation("Stopping chaos for {ExperimentName}", experiment.Name);
        await Task.Delay(1000); // Simulate chaos cleanup
    }
}

public class ChaosExperiment
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ChaosType Type { get; set; }
    public TimeSpan ChaosDuration { get; set; }
    public TimeSpan RecoveryDuration { get; set; }
}

public enum ChaosType
{
    Network,
    Database,
    System
}

public class ChaosExperimentResult
{
    public bool Success { get; set; }
    public string ExperimentName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public bool BaselineHealth { get; set; }
    public bool ChaosInjected { get; set; }
    public bool ChaosHealth { get; set; }
    public bool ChaosStopped { get; set; }
    public bool RecoveryHealth { get; set; }
    public string? ErrorMessage { get; set; }
}
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Smoke Tests**
```csharp
// Implementa:
// - Tests de endpoints críticos
// - Verificación de servicios externos
// - Tests de base de datos
// - Reportes de resultados
```

### **Ejercicio 2: Load Testing**
```csharp
// Crea:
// - Tests de carga normal
// - Tests de estrés
// - Tests de picos de tráfico
// - Análisis de rendimiento
```

### **Ejercicio 3: Chaos Engineering**
```csharp
// Implementa:
// - Experimentos de caos
// - Inyección de fallos
// - Monitoreo de resiliencia
// - Análisis de recuperación
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **🚬 Smoke Tests**: Verificación rápida de funcionalidad básica
2. **📊 Load Testing**: Pruebas de rendimiento bajo carga
3. **🌪️ Chaos Engineering**: Experimentos para probar resiliencia
4. **🧪 Testing en Producción**: Estrategias para entornos reales
5. **📈 Análisis de Resultados**: Métricas y reportes de testing

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Proyecto Final Integrador**, implementando todos los conceptos aprendidos en una aplicación completa.

---

**¡Has completado la novena clase del Módulo 15! 🧪🚬**


