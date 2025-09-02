# 🚀 **Clase 7: Model Deployment y MLOps**

## 🎯 **Objetivos de la Clase**
- Desplegar modelos ML en producción
- Implementar MLOps y CI/CD para ML
- Monitorear modelos en tiempo real
- Gestionar versionado de modelos

## 📚 **Contenido Teórico**

### **1. Fundamentos de Model Deployment**

#### **Servicio de Despliegue de Modelos**
```csharp
public class ModelDeploymentService : IModelDeploymentService
{
    private readonly MLContext _mlContext;
    private readonly IModelRepository _modelRepository;
    private readonly ILogger<ModelDeploymentService> _logger;
    private readonly Dictionary<string, ITransformer> _deployedModels;

    public ModelDeploymentService(
        IModelRepository modelRepository,
        ILogger<ModelDeploymentService> logger)
    {
        _mlContext = new MLContext(seed: 1);
        _modelRepository = modelRepository;
        _logger = logger;
        _deployedModels = new Dictionary<string, ITransformer>();
    }

    public async Task<DeploymentResult> DeployModelAsync(ModelDeploymentRequest request)
    {
        try
        {
            // Validar modelo
            var validationResult = await ValidateModelAsync(request.ModelId);
            if (!validationResult.IsValid)
            {
                return new DeploymentResult
                {
                    Success = false,
                    Error = validationResult.ErrorMessage
                };
            }

            // Cargar modelo
            var model = await LoadModelAsync(request.ModelId);
            
            // Desplegar modelo
            var deploymentId = await DeployModelToProductionAsync(model, request);
            
            // Registrar deployment
            await _modelRepository.RecordDeploymentAsync(new ModelDeployment
            {
                ModelId = request.ModelId,
                DeploymentId = deploymentId,
                Environment = request.Environment,
                Version = request.Version,
                DeployedAt = DateTime.UtcNow,
                Status = DeploymentStatus.Active
            });

            _logger.LogInformation("Model {ModelId} deployed successfully with ID {DeploymentId}", 
                request.ModelId, deploymentId);

            return new DeploymentResult
            {
                Success = true,
                DeploymentId = deploymentId,
                Message = "Model deployed successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deploying model {ModelId}", request.ModelId);
            return new DeploymentResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    private async Task<ModelValidationResult> ValidateModelAsync(string modelId)
    {
        try
        {
            var model = await _modelRepository.GetModelAsync(modelId);
            
            if (model == null)
            {
                return new ModelValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Model not found"
                };
            }

            // Validar esquema del modelo
            var schemaValidation = ValidateModelSchema(model);
            if (!schemaValidation.IsValid)
            {
                return schemaValidation;
            }

            // Validar rendimiento del modelo
            var performanceValidation = await ValidateModelPerformanceAsync(model);
            if (!performanceValidation.IsValid)
            {
                return performanceValidation;
            }

            return new ModelValidationResult { IsValid = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating model {ModelId}", modelId);
            return new ModelValidationResult
            {
                IsValid = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private ModelValidationResult ValidateModelSchema(MLModel model)
    {
        try
        {
            // Cargar modelo y validar esquema
            var loadedModel = _mlContext.Model.Load(model.ModelPath, out var schema);
            
            // Validar que el esquema sea compatible
            if (schema == null)
            {
                return new ModelValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid model schema"
                };
            }

            return new ModelValidationResult { IsValid = true };
        }
        catch (Exception ex)
        {
            return new ModelValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Schema validation failed: {ex.Message}"
            };
        }
    }

    private async Task<ModelValidationResult> ValidateModelPerformanceAsync(MLModel model)
    {
        try
        {
            // Cargar datos de prueba
            var testData = await _modelRepository.GetTestDataAsync(model.Id);
            
            if (testData == null || !testData.Any())
            {
                return new ModelValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "No test data available"
                };
            }

            // Evaluar rendimiento
            var performance = await EvaluateModelPerformanceAsync(model, testData);
            
            // Verificar umbrales de rendimiento
            if (performance.Accuracy < 0.7f) // 70% de precisión mínima
            {
                return new ModelValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Model accuracy {performance.Accuracy:P} below threshold"
                };
            }

            return new ModelValidationResult { IsValid = true };
        }
        catch (Exception ex)
        {
            return new ModelValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Performance validation failed: {ex.Message}"
            };
        }
    }

    private async Task<string> DeployModelToProductionAsync(ITransformer model, ModelDeploymentRequest request)
    {
        var deploymentId = Guid.NewGuid().ToString();
        
        // Almacenar modelo en cache para acceso rápido
        _deployedModels[deploymentId] = model;
        
        // En un entorno real, aquí se desplegaría a:
        // - Azure ML Service
        // - AWS SageMaker
        // - Kubernetes
        // - Docker containers
        
        _logger.LogInformation("Model deployed to production with ID {DeploymentId}", deploymentId);
        
        return deploymentId;
    }

    public async Task<PredictionResult> MakePredictionAsync(string deploymentId, object input)
    {
        try
        {
            if (!_deployedModels.ContainsKey(deploymentId))
            {
                throw new InvalidOperationException($"Model {deploymentId} not deployed");
            }

            var model = _deployedModels[deploymentId];
            
            // Crear prediction engine
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<object, object>(model);
            
            // Realizar predicción
            var prediction = predictionEngine.Predict(input);
            
            return new PredictionResult
            {
                Prediction = prediction,
                Timestamp = DateTime.UtcNow,
                DeploymentId = deploymentId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making prediction with model {DeploymentId}", deploymentId);
            throw;
        }
    }
}
```

### **2. MLOps y CI/CD para ML**

#### **Pipeline de MLOps**
```csharp
public class MLOpsPipeline : IMLOpsPipeline
{
    private readonly IModelTrainingService _trainingService;
    private readonly IModelValidationService _validationService;
    private readonly IModelDeploymentService _deploymentService;
    private readonly IModelMonitoringService _monitoringService;
    private readonly ILogger<MLOpsPipeline> _logger;

    public MLOpsPipeline(
        IModelTrainingService trainingService,
        IModelValidationService validationService,
        IModelDeploymentService deploymentService,
        IModelMonitoringService monitoringService,
        ILogger<MLOpsPipeline> logger)
    {
        _trainingService = trainingService;
        _validationService = validationService;
        _deploymentService = deploymentService;
        _monitoringService = monitoringService;
        _logger = logger;
    }

    public async Task<MLOpsPipelineResult> ExecutePipelineAsync(MLOpsPipelineRequest request)
    {
        try
        {
            var result = new MLOpsPipelineResult
            {
                PipelineId = Guid.NewGuid().ToString(),
                StartTime = DateTime.UtcNow
            };

            // 1. Data Validation
            _logger.LogInformation("Starting data validation phase");
            var dataValidation = await _validationService.ValidateDataAsync(request.DataPath);
            if (!dataValidation.IsValid)
            {
                result.Status = PipelineStatus.Failed;
                result.Error = "Data validation failed";
                return result;
            }
            result.DataValidationResult = dataValidation;

            // 2. Model Training
            _logger.LogInformation("Starting model training phase");
            var trainingResult = await _trainingService.TrainModelAsync(new ModelTrainingRequest
            {
                DataPath = request.DataPath,
                ModelType = request.ModelType,
                Parameters = request.TrainingParameters
            });
            
            if (!trainingResult.Success)
            {
                result.Status = PipelineStatus.Failed;
                result.Error = "Model training failed";
                return result;
            }
            result.TrainingResult = trainingResult;

            // 3. Model Validation
            _logger.LogInformation("Starting model validation phase");
            var modelValidation = await _validationService.ValidateModelAsync(trainingResult.ModelId);
            if (!modelValidation.IsValid)
            {
                result.Status = PipelineStatus.Failed;
                result.Error = "Model validation failed";
                return result;
            }
            result.ModelValidationResult = modelValidation;

            // 4. Model Deployment
            _logger.LogInformation("Starting model deployment phase");
            var deploymentResult = await _deploymentService.DeployModelAsync(new ModelDeploymentRequest
            {
                ModelId = trainingResult.ModelId,
                Environment = request.Environment,
                Version = request.Version
            });
            
            if (!deploymentResult.Success)
            {
                result.Status = PipelineStatus.Failed;
                result.Error = "Model deployment failed";
                return result;
            }
            result.DeploymentResult = deploymentResult;

            // 5. Setup Monitoring
            _logger.LogInformation("Setting up model monitoring");
            await _monitoringService.SetupMonitoringAsync(deploymentResult.DeploymentId);

            result.Status = PipelineStatus.Completed;
            result.EndTime = DateTime.UtcNow;
            result.Duration = result.EndTime - result.StartTime;

            _logger.LogInformation("MLOps pipeline completed successfully in {Duration}", result.Duration);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MLOps pipeline failed");
            return new MLOpsPipelineResult
            {
                Status = PipelineStatus.Failed,
                Error = ex.Message,
                EndTime = DateTime.UtcNow
            };
        }
    }

    public async Task<ModelRetrainingResult> TriggerRetrainingAsync(string modelId, RetrainingTrigger trigger)
    {
        try
        {
            _logger.LogInformation("Triggering retraining for model {ModelId}", modelId);

            // 1. Evaluar si el modelo necesita retraining
            var needsRetraining = await EvaluateRetrainingNeedAsync(modelId, trigger);
            if (!needsRetraining)
            {
                return new ModelRetrainingResult
                {
                    Success = false,
                    Message = "Model does not need retraining"
                };
            }

            // 2. Obtener nuevos datos
            var newData = await GetNewTrainingDataAsync(modelId);
            
            // 3. Entrenar nuevo modelo
            var trainingResult = await _trainingService.TrainModelAsync(new ModelTrainingRequest
            {
                DataPath = newData,
                ModelType = "Retrained",
                Parameters = new Dictionary<string, object>()
            });

            // 4. Comparar con modelo actual
            var comparison = await CompareModelsAsync(modelId, trainingResult.ModelId);
            
            // 5. Desplegar si es mejor
            if (comparison.NewModelIsBetter)
            {
                var deploymentResult = await _deploymentService.DeployModelAsync(new ModelDeploymentRequest
                {
                    ModelId = trainingResult.ModelId,
                    Environment = "Production",
                    Version = "Retrained"
                });

                return new ModelRetrainingResult
                {
                    Success = true,
                    NewModelId = trainingResult.ModelId,
                    DeploymentId = deploymentResult.DeploymentId,
                    Improvement = comparison.Improvement
                };
            }

            return new ModelRetrainingResult
            {
                Success = false,
                Message = "New model is not better than current model"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in model retraining");
            throw;
        }
    }

    private async Task<bool> EvaluateRetrainingNeedAsync(string modelId, RetrainingTrigger trigger)
    {
        switch (trigger.Type)
        {
            case RetrainingTriggerType.Scheduled:
                // Retraining programado (ej: semanal)
                return true;
                
            case RetrainingTriggerType.PerformanceDegradation:
                // Degradación de rendimiento
                var performance = await _monitoringService.GetModelPerformanceAsync(modelId);
                return performance.Accuracy < 0.7f; // Umbral de degradación
                
            case RetrainingTriggerType.DataDrift:
                // Drift de datos
                var drift = await _monitoringService.DetectDataDriftAsync(modelId);
                return drift.HasDrift;
                
            default:
                return false;
        }
    }
}
```

### **3. Monitoreo de Modelos**

#### **Sistema de Monitoreo en Tiempo Real**
```csharp
public class ModelMonitoringService : IModelMonitoringService
{
    private readonly IModelRepository _modelRepository;
    private readonly ILogger<ModelMonitoringService> _logger;
    private readonly Dictionary<string, ModelMetrics> _modelMetrics;

    public ModelMonitoringService(
        IModelRepository modelRepository,
        ILogger<ModelMonitoringService> logger)
    {
        _modelRepository = modelRepository;
        _logger = logger;
        _modelMetrics = new Dictionary<string, ModelMetrics>();
    }

    public async Task SetupMonitoringAsync(string deploymentId)
    {
        try
        {
            var monitoringConfig = new ModelMonitoringConfig
            {
                DeploymentId = deploymentId,
                MetricsInterval = TimeSpan.FromMinutes(5),
                AlertThresholds = new AlertThresholds
                {
                    AccuracyThreshold = 0.7f,
                    LatencyThreshold = TimeSpan.FromSeconds(1),
                    ErrorRateThreshold = 0.05f
                }
            };

            // Configurar monitoreo
            await _modelRepository.SaveMonitoringConfigAsync(monitoringConfig);
            
            // Iniciar monitoreo en background
            _ = Task.Run(() => MonitorModelAsync(deploymentId));
            
            _logger.LogInformation("Monitoring setup for deployment {DeploymentId}", deploymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up monitoring for deployment {DeploymentId}", deploymentId);
            throw;
        }
    }

    private async Task MonitorModelAsync(string deploymentId)
    {
        while (true)
        {
            try
            {
                // Recopilar métricas
                var metrics = await CollectModelMetricsAsync(deploymentId);
                
                // Almacenar métricas
                _modelMetrics[deploymentId] = metrics;
                await _modelRepository.SaveModelMetricsAsync(deploymentId, metrics);
                
                // Verificar alertas
                await CheckAlertsAsync(deploymentId, metrics);
                
                // Esperar antes de la siguiente recolección
                await Task.Delay(TimeSpan.FromMinutes(5));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error monitoring model {DeploymentId}", deploymentId);
                await Task.Delay(TimeSpan.FromMinutes(1)); // Reintentar en 1 minuto
            }
        }
    }

    private async Task<ModelMetrics> CollectModelMetricsAsync(string deploymentId)
    {
        var metrics = new ModelMetrics
        {
            DeploymentId = deploymentId,
            Timestamp = DateTime.UtcNow,
            PredictionsCount = await GetPredictionCountAsync(deploymentId),
            AverageLatency = await GetAverageLatencyAsync(deploymentId),
            ErrorRate = await GetErrorRateAsync(deploymentId),
            Accuracy = await GetModelAccuracyAsync(deploymentId),
            DataDrift = await DetectDataDriftAsync(deploymentId)
        };

        return metrics;
    }

    private async Task CheckAlertsAsync(string deploymentId, ModelMetrics metrics)
    {
        var config = await _modelRepository.GetMonitoringConfigAsync(deploymentId);
        if (config == null) return;

        var alerts = new List<ModelAlert>();

        // Verificar precisión
        if (metrics.Accuracy < config.AlertThresholds.AccuracyThreshold)
        {
            alerts.Add(new ModelAlert
            {
                DeploymentId = deploymentId,
                Type = AlertType.PerformanceDegradation,
                Severity = AlertSeverity.High,
                Message = $"Model accuracy {metrics.Accuracy:P} below threshold {config.AlertThresholds.AccuracyThreshold:P}",
                Timestamp = DateTime.UtcNow
            });
        }

        // Verificar latencia
        if (metrics.AverageLatency > config.AlertThresholds.LatencyThreshold)
        {
            alerts.Add(new ModelAlert
            {
                DeploymentId = deploymentId,
                Type = AlertType.HighLatency,
                Severity = AlertSeverity.Medium,
                Message = $"Model latency {metrics.AverageLatency} exceeds threshold {config.AlertThresholds.LatencyThreshold}",
                Timestamp = DateTime.UtcNow
            });
        }

        // Verificar tasa de error
        if (metrics.ErrorRate > config.AlertThresholds.ErrorRateThreshold)
        {
            alerts.Add(new ModelAlert
            {
                DeploymentId = deploymentId,
                Type = AlertType.HighErrorRate,
                Severity = AlertSeverity.High,
                Message = $"Error rate {metrics.ErrorRate:P} exceeds threshold {config.AlertThresholds.ErrorRateThreshold:P}",
                Timestamp = DateTime.UtcNow
            });
        }

        // Verificar data drift
        if (metrics.DataDrift.HasDrift)
        {
            alerts.Add(new ModelAlert
            {
                DeploymentId = deploymentId,
                Type = AlertType.DataDrift,
                Severity = AlertSeverity.Medium,
                Message = $"Data drift detected: {metrics.DataDrift.DriftScore:F2}",
                Timestamp = DateTime.UtcNow
            });
        }

        // Enviar alertas
        foreach (var alert in alerts)
        {
            await SendAlertAsync(alert);
        }
    }

    private async Task SendAlertAsync(ModelAlert alert)
    {
        try
        {
            // En un entorno real, enviar a:
            // - Email
            // - Slack
            // - Teams
            // - PagerDuty
            
            _logger.LogWarning("Model Alert: {Type} - {Message}", alert.Type, alert.Message);
            
            // Guardar alerta en base de datos
            await _modelRepository.SaveModelAlertAsync(alert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending alert");
        }
    }

    public async Task<ModelPerformanceReport> GeneratePerformanceReportAsync(string deploymentId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var metrics = await _modelRepository.GetModelMetricsAsync(deploymentId, startDate, endDate);
            
            var report = new ModelPerformanceReport
            {
                DeploymentId = deploymentId,
                Period = new DateRange { Start = startDate, End = endDate },
                TotalPredictions = metrics.Sum(m => m.PredictionsCount),
                AverageAccuracy = metrics.Average(m => m.Accuracy),
                AverageLatency = TimeSpan.FromMilliseconds(metrics.Average(m => m.AverageLatency.TotalMilliseconds)),
                AverageErrorRate = metrics.Average(m => m.ErrorRate),
                DataDriftEvents = metrics.Count(m => m.DataDrift.HasDrift),
                Alerts = await _modelRepository.GetModelAlertsAsync(deploymentId, startDate, endDate)
            };

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating performance report for deployment {DeploymentId}", deploymentId);
            throw;
        }
    }
}
```

## 🛠️ **Ejercicios Prácticos**

### **Ejercicio 1: Despliegue de Modelo Básico**
```csharp
// Implementar despliegue de modelo simple
public class SimpleModelDeployment
{
    private readonly MLContext _mlContext;
    private readonly Dictionary<string, ITransformer> _models;

    public SimpleModelDeployment()
    {
        _mlContext = new MLContext(seed: 1);
        _models = new Dictionary<string, ITransformer>();
    }

    public async Task<string> DeployModelAsync(string modelPath, string modelName)
    {
        try
        {
            // Cargar modelo
            var model = _mlContext.Model.Load(modelPath, out var schema);
            
            // Generar ID de deployment
            var deploymentId = $"{modelName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
            
            // Almacenar modelo
            _models[deploymentId] = model;
            
            Console.WriteLine($"Model {modelName} deployed with ID: {deploymentId}");
            return deploymentId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deploying model: {ex.Message}");
            throw;
        }
    }

    public T MakePrediction<TInput, TOutput>(string deploymentId, TInput input) 
        where TInput : class 
        where TOutput : class, new()
    {
        if (!_models.ContainsKey(deploymentId))
        {
            throw new InvalidOperationException($"Model {deploymentId} not found");
        }

        var model = _models[deploymentId];
        var predictionEngine = _mlContext.Model.CreatePredictionEngine<TInput, TOutput>(model);
        
        return predictionEngine.Predict(input);
    }
}
```

### **Ejercicio 2: API de MLOps**
```csharp
// Crear API REST para MLOps
[ApiController]
[Route("api/[controller]")]
public class MLOpsController : ControllerBase
{
    private readonly IMLOpsPipeline _mlOpsPipeline;
    private readonly IModelDeploymentService _deploymentService;
    private readonly IModelMonitoringService _monitoringService;
    private readonly ILogger<MLOpsController> _logger;

    public MLOpsController(
        IMLOpsPipeline mlOpsPipeline,
        IModelDeploymentService deploymentService,
        IModelMonitoringService monitoringService,
        ILogger<MLOpsController> logger)
    {
        _mlOpsPipeline = mlOpsPipeline;
        _deploymentService = deploymentService;
        _monitoringService = monitoringService;
        _logger = logger;
    }

    [HttpPost("pipeline")]
    public async Task<ActionResult<MLOpsPipelineResult>> ExecutePipeline([FromBody] MLOpsPipelineRequest request)
    {
        try
        {
            var result = await _mlOpsPipeline.ExecutePipelineAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing MLOps pipeline");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("deploy")]
    public async Task<ActionResult<DeploymentResult>> DeployModel([FromBody] ModelDeploymentRequest request)
    {
        try
        {
            var result = await _deploymentService.DeployModelAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deploying model");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("monitoring/{deploymentId}")]
    public async Task<ActionResult<ModelPerformanceReport>> GetPerformanceReport(
        string deploymentId, 
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-7);
            var end = endDate ?? DateTime.UtcNow;
            
            var report = await _monitoringService.GeneratePerformanceReportAsync(deploymentId, start, end);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance report");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("retrain/{modelId}")]
    public async Task<ActionResult<ModelRetrainingResult>> TriggerRetraining(
        string modelId, 
        [FromBody] RetrainingTrigger trigger)
    {
        try
        {
            var result = await _mlOpsPipeline.TriggerRetrainingAsync(modelId, trigger);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering retraining");
            return StatusCode(500, "Internal server error");
        }
    }
}
```

## 📋 **Resumen de la Clase**

### **Conceptos Clave Aprendidos:**
1. **Model Deployment**: Despliegue de modelos en producción
2. **MLOps**: CI/CD para machine learning
3. **Model Monitoring**: Monitoreo en tiempo real
4. **Model Validation**: Validación de modelos
5. **Retraining**: Reentrenamiento automático

### **Habilidades Desarrolladas:**
- Despliegue de modelos ML
- Implementación de pipelines MLOps
- Monitoreo de modelos en producción
- APIs REST para MLOps
- Automatización de retraining

### **Próxima Clase:**
**Clase 8: Advanced ML Techniques y Custom Models**
- Técnicas avanzadas de ML
- Modelos personalizados
- Transfer learning
- Ensemble methods

---

## 🔗 **Enlaces Útiles**
- [ML.NET Model Deployment](https://docs.microsoft.com/en-us/dotnet/machine-learning/how-to-guides/serve-model-web-api-ml-net)
- [MLOps with Azure](https://docs.microsoft.com/en-us/azure/machine-learning/concept-model-management-and-deployment)
- [Model Monitoring](https://docs.microsoft.com/en-us/azure/machine-learning/concept-model-monitoring)
- [MLOps Best Practices](https://docs.microsoft.com/en-us/azure/machine-learning/concept-mlops)
- [Model Versioning](https://docs.microsoft.com/en-us/azure/machine-learning/concept-model-versioning)
