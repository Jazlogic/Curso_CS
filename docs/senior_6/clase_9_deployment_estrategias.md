# 🚀 Clase 9: Deployment y Estrategias

## 🧭 Navegación
- **⬅️ Anterior**: [Clase 8: Monitoreo y Observabilidad](clase_8_monitoreo_observabilidad.md)
- **🏠 [Volver al Módulo 6](../README.md)**
- **➡️ Siguiente**: [Clase 10: Proyecto Final Integrador](clase_10_proyecto_final.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)**

---

## 🎯 Objetivos de la Clase

Al finalizar esta clase, serás capaz de:
- Implementar Blue-Green deployment
- Configurar Canary deployment
- Implementar estrategias de rollback
- Configurar feature flags

---

## 📚 Contenido Teórico

### 9.1 Blue-Green Deployment

#### Implementación Básica

```csharp
public class BlueGreenDeploymentService : IDeploymentService
{
    private readonly ILogger<BlueGreenDeploymentService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHealthCheckService _healthCheckService;
    
    public BlueGreenDeploymentService(
        ILogger<BlueGreenDeploymentService> logger,
        IConfiguration configuration,
        IHealthCheckService healthCheckService)
    {
        _logger = logger;
        _configuration = configuration;
        _healthCheckService = healthCheckService;
    }
    
    public async Task<DeploymentResult> DeployAsync(DeploymentRequest request)
    {
        _logger.LogInformation("Starting Blue-Green deployment for version {Version}", request.Version);
        
        try
        {
            // 1. Determina el ambiente actual (Blue o Green)
            var currentEnvironment = await GetCurrentEnvironmentAsync();
            var newEnvironment = currentEnvironment == "blue" ? "green" : "blue";
            
            _logger.LogInformation("Current environment: {Current}, New environment: {New}", 
                currentEnvironment, newEnvironment);
            
            // 2. Despliega la nueva versión en el ambiente inactivo
            var deploymentResult = await DeployToEnvironmentAsync(newEnvironment, request);
            
            if (!deploymentResult.IsSuccess)
            {
                _logger.LogError("Deployment to {Environment} failed: {Error}", 
                    newEnvironment, deploymentResult.ErrorMessage);
                return DeploymentResult.Failure($"Deployment to {newEnvironment} failed");
            }
            
            // 3. Ejecuta health checks en el nuevo ambiente
            var healthCheckResult = await _healthCheckService.CheckEnvironmentHealthAsync(newEnvironment);
            
            if (!healthCheckResult.IsHealthy)
            {
                _logger.LogWarning("Health check failed for {Environment}: {Issues}", 
                    newEnvironment, string.Join(", ", healthCheckResult.Issues));
                return DeploymentResult.Failure($"Health check failed for {newEnvironment}");
            }
            
            // 4. Ejecuta smoke tests
            var smokeTestResult = await RunSmokeTestsAsync(newEnvironment);
            
            if (!smokeTestResult.IsSuccess)
            {
                _logger.LogWarning("Smoke tests failed for {Environment}: {Issues}", 
                    newEnvironment, string.Join(", ", smokeTestResult.Issues));
                return DeploymentResult.Failure($"Smoke tests failed for {newEnvironment}");
            }
            
            // 5. Cambia el tráfico al nuevo ambiente
            var trafficSwitchResult = await SwitchTrafficAsync(newEnvironment);
            
            if (!trafficSwitchResult.IsSuccess)
            {
                _logger.LogError("Traffic switch to {Environment} failed: {Error}", 
                    newEnvironment, trafficSwitchResult.ErrorMessage);
                return DeploymentResult.Failure($"Traffic switch failed");
            }
            
            // 6. Actualiza la configuración del ambiente actual
            await UpdateCurrentEnvironmentAsync(newEnvironment);
            
            _logger.LogInformation("Blue-Green deployment completed successfully. New environment: {Environment}", 
                newEnvironment);
            
            return DeploymentResult.Success(newEnvironment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Blue-Green deployment failed");
            return DeploymentResult.Failure($"Deployment failed: {ex.Message}");
        }
    }
    
    private async Task<string> GetCurrentEnvironmentAsync()
    {
        // Obtiene el ambiente actual desde la configuración
        return _configuration["Deployment:CurrentEnvironment"] ?? "blue";
    }
    
    private async Task<DeploymentResult> DeployToEnvironmentAsync(string environment, DeploymentRequest request)
    {
        _logger.LogInformation("Deploying version {Version} to {Environment}", request.Version, environment);
        
        // Aquí iría la lógica real de deployment
        // Por ejemplo, actualizar Kubernetes deployments, Docker images, etc.
        
        await Task.Delay(5000); // Simula tiempo de deployment
        
        return DeploymentResult.Success(environment);
    }
    
    private async Task<TrafficSwitchResult> SwitchTrafficAsync(string newEnvironment)
    {
        _logger.LogInformation("Switching traffic to {Environment}", newEnvironment);
        
        // Aquí iría la lógica de cambio de tráfico
        // Por ejemplo, actualizar load balancer, DNS, etc.
        
        await Task.Delay(2000); // Simula tiempo de cambio de tráfico
        
        return TrafficSwitchResult.Success();
    }
    
    private async Task UpdateCurrentEnvironmentAsync(string newEnvironment)
    {
        // Actualiza la configuración para reflejar el nuevo ambiente activo
        _logger.LogInformation("Updating current environment to {Environment}", newEnvironment);
        
        // En un escenario real, esto podría ser una actualización en base de datos,
        // configuración de Kubernetes, etc.
    }
}

public class DeploymentRequest
{
    public string Version { get; set; }
    public string Environment { get; set; }
    public Dictionary<string, string> Configuration { get; set; }
}

public class DeploymentResult
{
    public bool IsSuccess { get; set; }
    public string Environment { get; set; }
    public string ErrorMessage { get; set; }
    
    public static DeploymentResult Success(string environment) =>
        new() { IsSuccess = true, Environment = environment };
    
    public static DeploymentResult Failure(string error) =>
        new() { IsSuccess = false, ErrorMessage = error };
}
```

#### Health Checks para Blue-Green

```csharp
public class BlueGreenHealthCheckService : IHealthCheckService
{
    private readonly ILogger<BlueGreenHealthCheckService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    
    public async Task<HealthCheckResult> CheckEnvironmentHealthAsync(string environment)
    {
        _logger.LogInformation("Checking health for environment {Environment}", environment);
        
        var healthChecks = new List<HealthCheckItem>();
        
        // Health check básico
        var basicHealth = await CheckBasicHealthAsync(environment);
        healthChecks.Add(basicHealth);
        
        // Health check de base de datos
        var dbHealth = await CheckDatabaseHealthAsync(environment);
        healthChecks.Add(dbHealth);
        
        // Health check de servicios externos
        var externalHealth = await CheckExternalServicesHealthAsync(environment);
        healthChecks.Add(externalHealth);
        
        // Health check de performance
        var performanceHealth = await CheckPerformanceHealthAsync(environment);
        healthChecks.Add(performanceHealth);
        
        var failedChecks = healthChecks.Where(h => !h.IsHealthy).ToList();
        
        if (failedChecks.Any())
        {
            return HealthCheckResult.Unhealthy(
                $"Environment {environment} has {failedChecks.Count} failed health checks",
                failedChecks.Select(h => h.Issue).ToList());
        }
        
        return HealthCheckResult.Healthy($"Environment {environment} is healthy");
    }
    
    private async Task<HealthCheckItem> CheckBasicHealthAsync(string environment)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{GetEnvironmentUrl(environment)}/health");
            
            if (response.IsSuccessStatusCode)
            {
                return HealthCheckItem.Healthy("Basic health check passed");
            }
            
            return HealthCheckItem.Unhealthy($"Basic health check failed: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckItem.Unhealthy($"Basic health check error: {ex.Message}");
        }
    }
    
    private async Task<HealthCheckItem> CheckDatabaseHealthAsync(string environment)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{GetEnvironmentUrl(environment)}/health/db");
            
            if (response.IsSuccessStatusCode)
            {
                return HealthCheckItem.Healthy("Database health check passed");
            }
            
            return HealthCheckItem.Unhealthy($"Database health check failed: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckItem.Unhealthy($"Database health check error: {ex.Message}");
        }
    }
    
    private async Task<HealthCheckItem> CheckExternalServicesHealthAsync(string environment)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{GetEnvironmentUrl(environment)}/health/external");
            
            if (response.IsSuccessStatusCode)
            {
                return HealthCheckItem.Healthy("External services health check passed");
            }
            
            return HealthCheckItem.Unhealthy($"External services health check failed: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckItem.Unhealthy($"External services health check error: {ex.Message}");
        }
    }
    
    private async Task<HealthCheckItem> CheckPerformanceHealthAsync(string environment)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var stopwatch = Stopwatch.StartNew();
            
            var response = await client.GetAsync($"{GetEnvironmentUrl(environment)}/health/performance");
            stopwatch.Stop();
            
            if (response.IsSuccessStatusCode)
            {
                var responseTime = stopwatch.Elapsed.TotalMilliseconds;
                
                if (responseTime < 1000) // Menos de 1 segundo
                {
                    return HealthCheckItem.Healthy($"Performance health check passed ({responseTime:F0}ms)");
                }
                else if (responseTime < 3000) // Menos de 3 segundos
                {
                    return HealthCheckItem.Degraded($"Performance health check degraded ({responseTime:F0}ms)");
                }
                else
                {
                    return HealthCheckItem.Unhealthy($"Performance health check failed ({responseTime:F0}ms)");
                }
            }
            
            return HealthCheckItem.Unhealthy($"Performance health check failed: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckItem.Unhealthy($"Performance health check error: {ex.Message}");
        }
    }
    
    private string GetEnvironmentUrl(string environment)
    {
        return environment switch
        {
            "blue" => "https://blue.myapp.com",
            "green" => "https://green.myapp.com",
            _ => throw new ArgumentException($"Unknown environment: {environment}")
        };
    }
}

public class HealthCheckItem
{
    public bool IsHealthy { get; set; }
    public string Issue { get; set; }
    
    public static HealthCheckItem Healthy(string message) =>
        new() { IsHealthy = true, Issue = message };
    
    public static HealthCheckItem Unhealthy(string issue) =>
        new() { IsHealthy = false, Issue = issue };
    
    public static HealthCheckItem Degraded(string issue) =>
        new() { IsHealthy = false, Issue = issue };
}
```

### 9.2 Canary Deployment

#### Implementación de Canary

```csharp
public class CanaryDeploymentService : IDeploymentService
{
    private readonly ILogger<CanaryDeploymentService> _logger;
    private readonly ITrafficManagementService _trafficService;
    private readonly IMonitoringService _monitoringService;
    
    public async Task<DeploymentResult> DeployCanaryAsync(CanaryDeploymentRequest request)
    {
        _logger.LogInformation("Starting Canary deployment for version {Version} with {Percentage}% traffic", 
            request.Version, request.InitialTrafficPercentage);
        
        try
        {
            // 1. Despliega la nueva versión con 0% de tráfico
            var deploymentResult = await DeployNewVersionAsync(request);
            
            if (!deploymentResult.IsSuccess)
            {
                return DeploymentResult.Failure($"Failed to deploy new version: {deploymentResult.ErrorMessage}");
            }
            
            // 2. Inicia con el porcentaje inicial de tráfico
            var trafficResult = await _trafficService.SetTrafficPercentageAsync(request.Version, request.InitialTrafficPercentage);
            
            if (!trafficResult.IsSuccess)
            {
                return DeploymentResult.Failure($"Failed to set initial traffic: {trafficResult.ErrorMessage}");
            }
            
            // 3. Monitorea la nueva versión
            var monitoringResult = await MonitorCanaryAsync(request);
            
            if (!monitoringResult.IsSuccess)
            {
                _logger.LogWarning("Canary monitoring failed, rolling back");
                await RollbackCanaryAsync(request.Version);
                return DeploymentResult.Failure($"Canary monitoring failed: {monitoringResult.ErrorMessage}");
            }
            
            // 4. Incrementa gradualmente el tráfico
            var trafficIncreaseResult = await GraduallyIncreaseTrafficAsync(request);
            
            if (!trafficIncreaseResult.IsSuccess)
            {
                await RollbackCanaryAsync(request.Version);
                return DeploymentResult.Failure($"Traffic increase failed: {trafficIncreaseResult.ErrorMessage}");
            }
            
            // 5. Si todo va bien, despliega completamente
            var fullDeploymentResult = await DeployFullyAsync(request.Version);
            
            if (!fullDeploymentResult.IsSuccess)
            {
                await RollbackCanaryAsync(request.Version);
                return DeploymentResult.Failure($"Full deployment failed: {fullDeploymentResult.ErrorMessage}");
            }
            
            _logger.LogInformation("Canary deployment completed successfully for version {Version}", request.Version);
            
            return DeploymentResult.Success("production");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Canary deployment failed for version {Version}", request.Version);
            await RollbackCanaryAsync(request.Version);
            return DeploymentResult.Failure($"Canary deployment failed: {ex.Message}");
        }
    }
    
    private async Task<DeploymentResult> DeployNewVersionAsync(CanaryDeploymentRequest request)
    {
        _logger.LogInformation("Deploying new version {Version} with 0% traffic", request.Version);
        
        // Aquí iría la lógica real de deployment
        await Task.Delay(3000); // Simula tiempo de deployment
        
        return DeploymentResult.Success("canary");
    }
    
    private async Task<MonitoringResult> MonitorCanaryAsync(CanaryDeploymentRequest request)
    {
        _logger.LogInformation("Monitoring canary deployment for version {Version}", request.Version);
        
        var monitoringDuration = TimeSpan.FromMinutes(request.MonitoringDurationMinutes);
        var startTime = DateTime.UtcNow;
        
        while (DateTime.UtcNow - startTime < monitoringDuration)
        {
            var metrics = await _monitoringService.GetMetricsAsync(request.Version);
            
            // Verifica métricas críticas
            if (metrics.ErrorRate > request.MaxErrorRate)
            {
                return MonitoringResult.Failure($"Error rate too high: {metrics.ErrorRate:P}");
            }
            
            if (metrics.ResponseTime > request.MaxResponseTime)
            {
                return MonitoringResult.Failure($"Response time too high: {metrics.ResponseTime}ms");
            }
            
            if (metrics.CpuUsage > request.MaxCpuUsage)
            {
                return MonitoringResult.Failure($"CPU usage too high: {metrics.CpuUsage:P}");
            }
            
            // Espera antes de la siguiente verificación
            await Task.Delay(TimeSpan.FromSeconds(30));
        }
        
        return MonitoringResult.Success();
    }
    
    private async Task<TrafficResult> GraduallyIncreaseTrafficAsync(CanaryDeploymentRequest request)
    {
        _logger.LogInformation("Gradually increasing traffic for version {Version}", request.Version);
        
        var currentPercentage = request.InitialTrafficPercentage;
        var targetPercentage = 100;
        var increment = request.TrafficIncrementPercentage;
        
        while (currentPercentage < targetPercentage)
        {
            currentPercentage = Math.Min(currentPercentage + increment, targetPercentage);
            
            var trafficResult = await _trafficService.SetTrafficPercentageAsync(request.Version, currentPercentage);
            
            if (!trafficResult.IsSuccess)
            {
                return TrafficResult.Failure($"Failed to increase traffic to {currentPercentage}%");
            }
            
            _logger.LogInformation("Increased traffic to {Percentage}% for version {Version}", 
                currentPercentage, request.Version);
            
            // Monitorea después de cada incremento
            var monitoringResult = await MonitorCanaryAsync(request);
            
            if (!monitoringResult.IsSuccess)
            {
                return TrafficResult.Failure($"Monitoring failed after traffic increase to {currentPercentage}%");
            }
            
            // Espera antes del siguiente incremento
            await Task.Delay(TimeSpan.FromMinutes(request.TrafficIncrementDelayMinutes));
        }
        
        return TrafficResult.Success();
    }
    
    private async Task<DeploymentResult> DeployFullyAsync(string version)
    {
        _logger.LogInformation("Deploying version {Version} to 100% of traffic", version);
        
        // Aquí iría la lógica de deployment completo
        await Task.Delay(2000); // Simula tiempo de deployment
        
        return DeploymentResult.Success("production");
    }
    
    private async Task RollbackCanaryAsync(string version)
    {
        _logger.LogWarning("Rolling back canary deployment for version {Version}", version);
        
        // Aquí iría la lógica de rollback
        await Task.Delay(2000); // Simula tiempo de rollback
        
        _logger.LogInformation("Rollback completed for version {Version}", version);
    }
}

public class CanaryDeploymentRequest : DeploymentRequest
{
    public int InitialTrafficPercentage { get; set; } = 5;
    public int TrafficIncrementPercentage { get; set; } = 10;
    public int TrafficIncrementDelayMinutes { get; set; } = 5;
    public int MonitoringDurationMinutes { get; set; } = 10;
    public double MaxErrorRate { get; set; } = 0.05; // 5%
    public int MaxResponseTime { get; set; } = 1000; // 1 segundo
    public double MaxCpuUsage { get; set; } = 0.80; // 80%
}
```

### 9.3 Rollback Strategies

#### Servicio de Rollback

```csharp
public class RollbackService : IRollbackService
{
    private readonly ILogger<RollbackService> _logger;
    private readonly IDeploymentHistoryService _deploymentHistory;
    private readonly ITrafficManagementService _trafficService;
    
    public async Task<RollbackResult> RollbackAsync(RollbackRequest request)
    {
        _logger.LogWarning("Initiating rollback to version {TargetVersion}", request.TargetVersion);
        
        try
        {
            // 1. Obtiene el historial de deployments
            var deploymentHistory = await _deploymentHistory.GetDeploymentHistoryAsync();
            var targetDeployment = deploymentHistory.FirstOrDefault(d => d.Version == request.TargetVersion);
            
            if (targetDeployment == null)
            {
                return RollbackResult.Failure($"Target version {request.TargetVersion} not found in deployment history");
            }
            
            // 2. Verifica que la versión objetivo sea estable
            if (!targetDeployment.IsStable)
            {
                return RollbackResult.Failure($"Target version {request.TargetVersion} is not marked as stable");
            }
            
            // 3. Ejecuta el rollback
            var rollbackResult = await ExecuteRollbackAsync(targetDeployment, request);
            
            if (!rollbackResult.IsSuccess)
            {
                return RollbackResult.Failure($"Rollback execution failed: {rollbackResult.ErrorMessage}");
            }
            
            // 4. Verifica la salud del sistema después del rollback
            var healthCheckResult = await VerifySystemHealthAsync(targetDeployment);
            
            if (!healthCheckResult.IsSuccess)
            {
                _logger.LogError("System health check failed after rollback to {Version}", request.TargetVersion);
                return RollbackResult.Failure($"System health check failed after rollback");
            }
            
            // 5. Actualiza el historial de deployments
            await _deploymentHistory.MarkRollbackAsync(request.TargetVersion, request.Reason);
            
            _logger.LogInformation("Rollback to version {Version} completed successfully", request.TargetVersion);
            
            return RollbackResult.Success(targetDeployment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rollback to version {Version} failed", request.TargetVersion);
            return RollbackResult.Failure($"Rollback failed: {ex.Message}");
        }
    }
    
    private async Task<RollbackResult> ExecuteRollbackAsync(DeploymentInfo targetDeployment, RollbackRequest request)
    {
        _logger.LogInformation("Executing rollback to version {Version}", targetDeployment.Version);
        
        try
        {
            // Cambia el tráfico a la versión objetivo
            var trafficResult = await _trafficService.SetTrafficPercentageAsync(targetDeployment.Version, 100);
            
            if (!trafficResult.IsSuccess)
            {
                return RollbackResult.Failure($"Failed to set traffic to target version: {trafficResult.ErrorMessage}");
            }
            
            // Desactiva la versión problemática
            if (!string.IsNullOrEmpty(request.ProblematicVersion))
            {
                var disableResult = await _trafficService.SetTrafficPercentageAsync(request.ProblematicVersion, 0);
                
                if (!disableResult.IsSuccess)
                {
                    _logger.LogWarning("Failed to disable problematic version {Version}: {Error}", 
                        request.ProblematicVersion, disableResult.ErrorMessage);
                }
            }
            
            return RollbackResult.Success(targetDeployment);
        }
        catch (Exception ex)
        {
            return RollbackResult.Failure($"Rollback execution failed: {ex.Message}");
        }
    }
    
    private async Task<HealthCheckResult> VerifySystemHealthAsync(DeploymentInfo deployment)
    {
        _logger.LogInformation("Verifying system health after rollback to version {Version}", deployment.Version);
        
        // Espera un poco para que el sistema se estabilice
        await Task.Delay(TimeSpan.FromSeconds(30));
        
        // Ejecuta health checks
        var healthChecks = new[]
        {
            await CheckBasicHealthAsync(deployment),
            await CheckDatabaseHealthAsync(deployment),
            await CheckPerformanceHealthAsync(deployment)
        };
        
        var failedChecks = healthChecks.Where(h => !h.IsHealthy).ToList();
        
        if (failedChecks.Any())
        {
            return HealthCheckResult.Failure(
                $"System health check failed after rollback: {string.Join(", ", failedChecks.Select(h => h.Issue))}");
        }
        
        return HealthCheckResult.Success("System is healthy after rollback");
    }
    
    private async Task<HealthCheckItem> CheckBasicHealthAsync(DeploymentInfo deployment)
    {
        // Implementación de health check básico
        await Task.Delay(1000); // Simula tiempo de verificación
        return HealthCheckItem.Healthy("Basic health check passed");
    }
    
    private async Task<HealthCheckItem> CheckDatabaseHealthAsync(DeploymentInfo deployment)
    {
        // Implementación de health check de base de datos
        await Task.Delay(1000); // Simula tiempo de verificación
        return HealthCheckItem.Healthy("Database health check passed");
    }
    
    private async Task<HealthCheckItem> CheckPerformanceHealthAsync(DeploymentInfo deployment)
    {
        // Implementación de health check de performance
        await Task.Delay(1000); // Simula tiempo de verificación
        return HealthCheckItem.Healthy("Performance health check passed");
    }
}

public class RollbackRequest
{
    public string TargetVersion { get; set; }
    public string ProblematicVersion { get; set; }
    public string Reason { get; set; }
    public RollbackType Type { get; set; } = RollbackType.Full;
}

public enum RollbackType
{
    Full,      // Rollback completo a la versión anterior
    Partial,   // Rollback parcial (solo algunos servicios)
    Traffic    // Solo cambio de tráfico
}
```

### 9.4 Feature Flags

#### Sistema de Feature Flags

```csharp
public class FeatureFlagService : IFeatureFlagService
{
    private readonly ILogger<FeatureFlagService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IFeatureFlagRepository _repository;
    
    public async Task<bool> IsFeatureEnabledAsync(string featureName, string userId = null, string environment = null)
    {
        try
        {
            // Obtiene la configuración del feature flag
            var featureFlag = await _repository.GetFeatureFlagAsync(featureName);
            
            if (featureFlag == null)
            {
                _logger.LogWarning("Feature flag {FeatureName} not found", featureName);
                return false;
            }
            
            // Verifica si el feature está habilitado globalmente
            if (!featureFlag.IsEnabled)
            {
                return false;
            }
            
            // Verifica el ambiente
            if (!string.IsNullOrEmpty(environment) && !featureFlag.EnabledEnvironments.Contains(environment))
            {
                return false;
            }
            
            // Verifica usuarios específicos
            if (!string.IsNullOrEmpty(userId) && featureFlag.EnabledUserIds.Contains(userId))
            {
                return true;
            }
            
            // Verifica porcentaje de usuarios
            if (featureFlag.UserPercentage > 0)
            {
                var userHash = GetUserHash(userId ?? "anonymous");
                var isInPercentage = (userHash % 100) < featureFlag.UserPercentage;
                
                if (isInPercentage)
                {
                    return true;
                }
            }
            
            // Verifica reglas personalizadas
            if (featureFlag.CustomRules.Any())
            {
                foreach (var rule in featureFlag.CustomRules)
                {
                    if (await EvaluateRuleAsync(rule, userId, environment))
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking feature flag {FeatureName}", featureName);
            return false; // Por defecto, deshabilitado en caso de error
        }
    }
    
    public async Task<T> GetFeatureValueAsync<T>(string featureName, T defaultValue = default, string userId = null, string environment = null)
    {
        try
        {
            if (!await IsFeatureEnabledAsync(featureName, userId, environment))
            {
                return defaultValue;
            }
            
            var featureFlag = await _repository.GetFeatureFlagAsync(featureName);
            
            if (featureFlag?.Value != null)
            {
                try
                {
                    return (T)Convert.ChangeType(featureFlag.Value, typeof(T));
                }
                catch
                {
                    _logger.LogWarning("Failed to convert feature flag value for {FeatureName}", featureName);
                    return defaultValue;
                }
            }
            
            return defaultValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting feature value for {FeatureName}", featureName);
            return defaultValue;
        }
    }
    
    public async Task<IEnumerable<string>> GetEnabledFeaturesAsync(string userId = null, string environment = null)
    {
        try
        {
            var allFeatures = await _repository.GetAllFeatureFlagsAsync();
            var enabledFeatures = new List<string>();
            
            foreach (var feature in allFeatures)
            {
                if (await IsFeatureEnabledAsync(feature.Name, userId, environment))
                {
                    enabledFeatures.Add(feature.Name);
                }
            }
            
            return enabledFeatures;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting enabled features");
            return Enumerable.Empty<string>();
        }
    }
    
    private int GetUserHash(string userId)
    {
        return userId.GetHashCode() & 0x7FFFFFFF; // Convierte a positivo
    }
    
    private async Task<bool> EvaluateRuleAsync(FeatureFlagRule rule, string userId, string environment)
    {
        // Implementación de evaluación de reglas personalizadas
        // Por ejemplo, reglas basadas en roles, ubicación, tiempo, etc.
        
        switch (rule.Type)
        {
            case RuleType.Role:
                return await EvaluateRoleRuleAsync(rule, userId);
            case RuleType.Location:
                return await EvaluateLocationRuleAsync(rule, userId);
            case RuleType.Time:
                return EvaluateTimeRuleAsync(rule);
            default:
                return false;
        }
    }
    
    private async Task<bool> EvaluateRoleRuleAsync(FeatureFlagRule rule, string userId)
    {
        // Implementación de evaluación de reglas de rol
        await Task.CompletedTask;
        return false;
    }
    
    private async Task<bool> EvaluateLocationRuleAsync(FeatureFlagRule rule, string userId)
    {
        // Implementación de evaluación de reglas de ubicación
        await Task.CompletedTask;
        return false;
    }
    
    private bool EvaluateTimeRuleAsync(FeatureFlagRule rule)
    {
        // Implementación de evaluación de reglas de tiempo
        return false;
    }
}

public class FeatureFlag
{
    public string Name { get; set; }
    public bool IsEnabled { get; set; }
    public object Value { get; set; }
    public List<string> EnabledEnvironments { get; set; } = new();
    public List<string> EnabledUserIds { get; set; } = new();
    public int UserPercentage { get; set; }
    public List<FeatureFlagRule> CustomRules { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class FeatureFlagRule
{
    public string Id { get; set; }
    public RuleType Type { get; set; }
    public string Condition { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public enum RuleType
{
    Role,
    Location,
    Time,
    Custom
}
```

---

## 💻 Ejercicios Prácticos

### Ejercicio 1: Implementar Blue-Green Deployment

Crea un sistema completo de Blue-Green deployment:

```csharp
// Implementa:
// - Detección automática de ambientes
// - Health checks automatizados
// - Cambio de tráfico automático
// - Rollback automático en caso de fallo
```

### Ejercicio 2: Sistema de Feature Flags

Implementa un sistema de feature flags con:

```csharp
// Incluye:
// - Configuración por ambiente
// - Reglas personalizadas
// - A/B testing
// - Dashboard de administración
```

---

## 🔍 Casos de Uso Reales

### 1. Deployment Automatizado con Kubernetes

```csharp
public class KubernetesDeploymentService : IDeploymentService
{
    public async Task<DeploymentResult> DeployAsync(DeploymentRequest request)
    {
        // Implementa deployment usando Kubernetes API
        // Incluye actualización de deployments, services, ingress, etc.
        
        return DeploymentResult.Success("kubernetes");
    }
}
```

### 2. Rollback Automatizado

```csharp
public class AutomaticRollbackService : IRollbackService
{
    public async Task MonitorAndRollbackAsync(string version)
    {
        // Monitorea métricas en tiempo real
        // Ejecuta rollback automático si se detectan problemas
    }
}
```

---

## 📊 Métricas de Deployment

### KPIs de Deployment

1. **Deployment Success Rate**: Tasa de éxito de deployments
2. **Rollback Frequency**: Frecuencia de rollbacks
3. **Time to Rollback**: Tiempo para ejecutar rollback
4. **Feature Flag Usage**: Uso de feature flags
5. **Deployment Duration**: Duración de deployments

---

## 🎯 Resumen de la Clase

En esta clase hemos aprendido:

✅ **Blue-Green Deployment**: Estrategia de deployment sin downtime
✅ **Canary Deployment**: Deployment gradual con monitoreo
✅ **Rollback Strategies**: Estrategias de reversión de cambios
✅ **Feature Flags**: Sistema de control de funcionalidades
✅ **Casos de Uso Reales**: Implementación en entornos de producción

---

## 🚀 Próximos Pasos

En la siguiente clase aprenderemos sobre:
- **Proyecto Final Integrador**
- Aplicación web optimizada para producción
- Implementación de todas las mejores prácticas
- Deployment completo en Kubernetes

---

## 🔗 Enlaces de Referencia

- [Deployment Strategies](https://docs.microsoft.com/en-us/azure/devops/pipelines/release/deployment-patterns/)
- [Feature Flags](https://docs.microsoft.com/en-us/azure/azure-app-configuration/concept-feature-management)
- [Kubernetes Deployment](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/)
- [Blue-Green Deployment](https://martinfowler.com/bliki/BlueGreenDeployment.html)

