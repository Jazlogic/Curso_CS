# 🧠 **Clase 8: Advanced ML Techniques y Custom Models**

## 🎯 **Objetivos de la Clase**
- Implementar técnicas avanzadas de ML
- Crear modelos personalizados
- Aplicar transfer learning
- Utilizar ensemble methods

## 📚 **Contenido Teórico**

### **1. Transfer Learning con ML.NET**

#### **Sistema de Transfer Learning**
```csharp
public class TransferLearningService : ITransferLearningService
{
    private readonly MLContext _mlContext;
    private readonly ILogger<TransferLearningService> _logger;

    public TransferLearningService(ILogger<TransferLearningService> logger)
    {
        _mlContext = new MLContext(seed: 1);
        _logger = logger;
    }

    public async Task<TransferLearningResult> FineTuneModelAsync(TransferLearningRequest request)
    {
        try
        {
            // Cargar modelo pre-entrenado
            var pretrainedModel = await LoadPretrainedModelAsync(request.BaseModelPath);
            
            // Preparar datos de fine-tuning
            var trainingData = await PrepareFineTuningDataAsync(request.TrainingDataPath);
            
            // Configurar pipeline de transfer learning
            var pipeline = CreateTransferLearningPipeline(pretrainedModel, request);
            
            // Entrenar modelo fine-tuned
            var fineTunedModel = pipeline.Fit(trainingData);
            
            // Evaluar modelo
            var evaluation = await EvaluateFineTunedModelAsync(fineTunedModel, request.TestDataPath);
            
            // Guardar modelo
            var modelPath = await SaveFineTunedModelAsync(fineTunedModel, request.OutputPath);
            
            return new TransferLearningResult
            {
                Success = true,
                ModelPath = modelPath,
                Evaluation = evaluation,
                TrainingTime = DateTime.UtcNow - request.StartTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in transfer learning");
            return new TransferLearningResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    private async Task<ITransformer> LoadPretrainedModelAsync(string modelPath)
    {
        try
        {
            // Cargar modelo pre-entrenado (ej: ResNet, BERT, etc.)
            var model = _mlContext.Model.Load(modelPath, out var schema);
            
            _logger.LogInformation("Loaded pretrained model from {ModelPath}", modelPath);
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading pretrained model from {ModelPath}", modelPath);
            throw;
        }
    }

    private IEstimator<ITransformer> CreateTransferLearningPipeline(ITransformer pretrainedModel, TransferLearningRequest request)
    {
        // Pipeline para transfer learning
        var pipeline = _mlContext.Transforms.LoadRawImageBytes(
                outputColumnName: "Image",
                imageFolder: request.ImageFolder,
                inputColumnName: "ImagePath")
            .Append(_mlContext.Transforms.ResizeImages(
                outputColumnName: "Image",
                imageWidth: request.ImageWidth,
                imageHeight: request.ImageHeight,
                inputColumnName: "Image"))
            .Append(_mlContext.Transforms.ExtractPixels(
                outputColumnName: "Features",
                inputColumnName: "Image"))
            .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label"))
            .Append(_mlContext.MulticlassClassification.Trainers.ImageClassification(
                labelColumnName: "Label",
                featureColumnName: "Features",
                arch: DnnArchitecture.ResnetV2101, // Arquitectura pre-entrenada
                epoch: request.Epochs,
                batchSize: request.BatchSize,
                learningRate: request.LearningRate));

        return pipeline;
    }

    private async Task<ModelEvaluation> EvaluateFineTunedModelAsync(ITransformer model, string testDataPath)
    {
        try
        {
            var testData = _mlContext.Data.LoadFromTextFile<ImageData>(
                path: testDataPath,
                hasHeader: true,
                separatorChar: ',');

            var predictions = model.Transform(testData);
            var metrics = _mlContext.MulticlassClassification.Evaluate(predictions);

            return new ModelEvaluation
            {
                Accuracy = metrics.MacroAccuracy,
                MicroAccuracy = metrics.MicroAccuracy,
                LogLoss = metrics.LogLoss,
                TopKAccuracy = metrics.TopKAccuracy
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating fine-tuned model");
            throw;
        }
    }
}
```

### **2. Ensemble Methods**

#### **Sistema de Ensemble Learning**
```csharp
public class EnsembleLearningService : IEnsembleLearningService
{
    private readonly MLContext _mlContext;
    private readonly ILogger<EnsembleLearningService> _logger;

    public EnsembleLearningService(ILogger<EnsembleLearningService> logger)
    {
        _mlContext = new MLContext(seed: 1);
        _logger = logger;
    }

    public async Task<EnsembleModel> CreateEnsembleAsync(EnsembleRequest request)
    {
        try
        {
            var ensemble = new EnsembleModel
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                Models = new List<EnsembleMember>(),
                Weights = new Dictionary<string, float>(),
                Method = request.Method
            };

            // Entrenar modelos base
            foreach (var modelConfig in request.BaseModels)
            {
                var member = await TrainBaseModelAsync(modelConfig, request.TrainingData);
                ensemble.Models.Add(member);
            }

            // Calcular pesos del ensemble
            ensemble.Weights = await CalculateEnsembleWeightsAsync(ensemble.Models, request.ValidationData);

            // Evaluar ensemble
            ensemble.Evaluation = await EvaluateEnsembleAsync(ensemble, request.TestData);

            _logger.LogInformation("Created ensemble with {Count} base models", ensemble.Models.Count);
            return ensemble;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ensemble");
            throw;
        }
    }

    private async Task<EnsembleMember> TrainBaseModelAsync(BaseModelConfig config, IDataView trainingData)
    {
        try
        {
            IEstimator<ITransformer> pipeline;

            switch (config.Type)
            {
                case ModelType.RandomForest:
                    pipeline = _mlContext.Transforms.Concatenate("Features", config.FeatureColumns)
                        .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label"))
                        .Append(_mlContext.MulticlassClassification.Trainers.RandomForest(
                            labelColumnName: "Label",
                            featureColumnName: "Features",
                            numberOfTrees: config.Parameters.GetValueOrDefault("NumberOfTrees", 100)));
                    break;

                case ModelType.SVM:
                    pipeline = _mlContext.Transforms.Concatenate("Features", config.FeatureColumns)
                        .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label"))
                        .Append(_mlContext.MulticlassClassification.Trainers.LinearSvm(
                            labelColumnName: "Label",
                            featureColumnName: "Features"));
                    break;

                case ModelType.NeuralNetwork:
                    pipeline = _mlContext.Transforms.Concatenate("Features", config.FeatureColumns)
                        .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label"))
                        .Append(_mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy(
                            labelColumnName: "Label",
                            featureColumnName: "Features"));
                    break;

                default:
                    throw new ArgumentException($"Unsupported model type: {config.Type}");
            }

            var model = pipeline.Fit(trainingData);
            
            return new EnsembleMember
            {
                Id = Guid.NewGuid().ToString(),
                Type = config.Type,
                Model = model,
                Parameters = config.Parameters
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error training base model of type {Type}", config.Type);
            throw;
        }
    }

    private async Task<Dictionary<string, float>> CalculateEnsembleWeightsAsync(
        List<EnsembleMember> models, 
        IDataView validationData)
    {
        var weights = new Dictionary<string, float>();
        var performances = new Dictionary<string, float>();

        // Evaluar cada modelo en datos de validación
        foreach (var member in models)
        {
            var predictions = member.Model.Transform(validationData);
            var metrics = _mlContext.MulticlassClassification.Evaluate(predictions);
            performances[member.Id] = metrics.MacroAccuracy;
        }

        // Calcular pesos basados en rendimiento
        var totalPerformance = performances.Values.Sum();
        
        foreach (var kvp in performances)
        {
            weights[kvp.Key] = kvp.Value / totalPerformance;
        }

        return weights;
    }

    public async Task<EnsemblePrediction> MakeEnsemblePredictionAsync(EnsembleModel ensemble, object input)
    {
        try
        {
            var predictions = new List<ModelPrediction>();
            var weightedPredictions = new Dictionary<string, float>();

            // Obtener predicciones de cada modelo base
            foreach (var member in ensemble.Models)
            {
                var prediction = await MakeModelPredictionAsync(member, input);
                predictions.Add(prediction);
            }

            // Combinar predicciones según el método del ensemble
            switch (ensemble.Method)
            {
                case EnsembleMethod.Voting:
                    return CombineByVoting(predictions, ensemble.Weights);
                
                case EnsembleMethod.Averaging:
                    return CombineByAveraging(predictions, ensemble.Weights);
                
                case EnsembleMethod.Stacking:
                    return await CombineByStackingAsync(predictions, ensemble);
                
                default:
                    throw new ArgumentException($"Unsupported ensemble method: {ensemble.Method}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making ensemble prediction");
            throw;
        }
    }

    private async Task<ModelPrediction> MakeModelPredictionAsync(EnsembleMember member, object input)
    {
        // Crear prediction engine específico para el tipo de modelo
        var predictionEngine = _mlContext.Model.CreatePredictionEngine<object, object>(member.Model);
        var prediction = predictionEngine.Predict(input);
        
        return new ModelPrediction
        {
            ModelId = member.Id,
            Prediction = prediction,
            Confidence = 0.8f // Simplificado
        };
    }

    private EnsemblePrediction CombineByVoting(List<ModelPrediction> predictions, Dictionary<string, float> weights)
    {
        // Votación ponderada
        var voteCounts = new Dictionary<string, float>();
        
        foreach (var prediction in predictions)
        {
            var predictionClass = prediction.Prediction.ToString();
            var weight = weights.GetValueOrDefault(prediction.ModelId, 1.0f);
            
            if (voteCounts.ContainsKey(predictionClass))
            {
                voteCounts[predictionClass] += weight;
            }
            else
            {
                voteCounts[predictionClass] = weight;
            }
        }

        var winner = voteCounts.OrderByDescending(kvp => kvp.Value).First();
        
        return new EnsemblePrediction
        {
            Prediction = winner.Key,
            Confidence = winner.Value / voteCounts.Values.Sum(),
            Method = "Voting",
            IndividualPredictions = predictions
        };
    }

    private EnsemblePrediction CombineByAveraging(List<ModelPrediction> predictions, Dictionary<string, float> weights)
    {
        // Promedio ponderado de probabilidades
        var averagedProbabilities = new Dictionary<string, float>();
        
        foreach (var prediction in predictions)
        {
            var weight = weights.GetValueOrDefault(prediction.ModelId, 1.0f);
            // Aquí se promediarían las probabilidades de cada clase
            // Simplificado para el ejemplo
        }

        return new EnsemblePrediction
        {
            Prediction = "Averaged",
            Confidence = 0.85f,
            Method = "Averaging",
            IndividualPredictions = predictions
        };
    }

    private async Task<EnsemblePrediction> CombineByStackingAsync(List<ModelPrediction> predictions, EnsembleModel ensemble)
    {
        // Stacking: usar un meta-modelo para combinar predicciones
        // Implementación simplificada
        
        return new EnsemblePrediction
        {
            Prediction = "Stacked",
            Confidence = 0.9f,
            Method = "Stacking",
            IndividualPredictions = predictions
        };
    }
}
```

### **3. Modelos Personalizados**

#### **Sistema de Modelos Custom**
```csharp
public class CustomModelService : ICustomModelService
{
    private readonly MLContext _mlContext;
    private readonly ILogger<CustomModelService> _logger;

    public CustomModelService(ILogger<CustomModelService> logger)
    {
        _mlContext = new MLContext(seed: 1);
        _logger = logger;
    }

    public async Task<CustomModel> CreateCustomModelAsync(CustomModelRequest request)
    {
        try
        {
            var customModel = new CustomModel
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                Architecture = request.Architecture,
                Parameters = request.Parameters,
                CreatedAt = DateTime.UtcNow
            };

            // Crear pipeline personalizado
            var pipeline = CreateCustomPipeline(request);
            
            // Entrenar modelo
            var trainingData = _mlContext.Data.LoadFromEnumerable(request.TrainingData);
            customModel.Model = pipeline.Fit(trainingData);
            
            // Evaluar modelo
            customModel.Evaluation = await EvaluateCustomModelAsync(customModel.Model, request.TestData);
            
            _logger.LogInformation("Created custom model {ModelName} with architecture {Architecture}", 
                request.Name, request.Architecture);
            
            return customModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating custom model");
            throw;
        }
    }

    private IEstimator<ITransformer> CreateCustomPipeline(CustomModelRequest request)
    {
        var pipeline = _mlContext.Transforms.Concatenate("Features", request.FeatureColumns);

        // Aplicar transformaciones personalizadas
        foreach (var transformation in request.Transformations)
        {
            pipeline = ApplyCustomTransformation(pipeline, transformation);
        }

        // Agregar algoritmo de entrenamiento personalizado
        pipeline = AddCustomAlgorithm(pipeline, request.Algorithm);

        return pipeline;
    }

    private IEstimator<ITransformer> ApplyCustomTransformation(IEstimator<ITransformer> pipeline, CustomTransformation transformation)
    {
        switch (transformation.Type)
        {
            case TransformationType.Normalization:
                return pipeline.Append(_mlContext.Transforms.NormalizeMinMax("Features"));
            
            case TransformationType.PCA:
                return pipeline.Append(_mlContext.Transforms.ProjectToPrincipalComponents(
                    "Features", "PCAFeatures", rank: transformation.Parameters.GetValueOrDefault("Rank", 10)));
            
            case TransformationType.FeatureSelection:
                return pipeline.Append(_mlContext.Transforms.FeatureSelection.SelectFeaturesBasedOnCount(
                    "Features", "SelectedFeatures", count: transformation.Parameters.GetValueOrDefault("Count", 100)));
            
            case TransformationType.CustomFunction:
                return ApplyCustomFunction(pipeline, transformation);
            
            default:
                throw new ArgumentException($"Unsupported transformation type: {transformation.Type}");
        }
    }

    private IEstimator<ITransformer> ApplyCustomFunction(IEstimator<ITransformer> pipeline, CustomTransformation transformation)
    {
        // Aplicar función personalizada
        var customFunction = transformation.Parameters.GetValueOrDefault("Function", "identity");
        
        switch (customFunction)
        {
            case "log_transform":
                return pipeline.Append(_mlContext.Transforms.CustomMapping<InputData, OutputData>(
                    (input, output) => output.Value = (float)Math.Log(input.Value + 1), "CustomLogTransform"));
            
            case "polynomial_features":
                return pipeline.Append(_mlContext.Transforms.CustomMapping<InputData, OutputData>(
                    (input, output) => output.Value = input.Value * input.Value, "PolynomialFeatures"));
            
            default:
                return pipeline;
        }
    }

    private IEstimator<ITransformer> AddCustomAlgorithm(IEstimator<ITransformer> pipeline, CustomAlgorithm algorithm)
    {
        switch (algorithm.Type)
        {
            case AlgorithmType.CustomRegression:
                return pipeline.Append(_mlContext.Regression.Trainers.Sdca(
                    labelColumnName: algorithm.LabelColumn,
                    featureColumnName: "Features",
                    l1Regularization: algorithm.Parameters.GetValueOrDefault("L1Regularization", 0.1f),
                    l2Regularization: algorithm.Parameters.GetValueOrDefault("L2Regularization", 0.1f)));
            
            case AlgorithmType.CustomClassification:
                return pipeline.Append(_mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy(
                    labelColumnName: algorithm.LabelColumn,
                    featureColumnName: "Features",
                    l1Regularization: algorithm.Parameters.GetValueOrDefault("L1Regularization", 0.1f),
                    l2Regularization: algorithm.Parameters.GetValueOrDefault("L2Regularization", 0.1f)));
            
            case AlgorithmType.CustomClustering:
                return pipeline.Append(_mlContext.Clustering.Trainers.KMeans(
                    featureColumnName: "Features",
                    numberOfClusters: algorithm.Parameters.GetValueOrDefault("NumberOfClusters", 3)));
            
            default:
                throw new ArgumentException($"Unsupported algorithm type: {algorithm.Type}");
        }
    }

    public async Task<CustomModel> OptimizeHyperparametersAsync(CustomModel model, HyperparameterOptimizationRequest request)
    {
        try
        {
            var bestModel = model;
            var bestScore = 0.0f;
            var bestParameters = new Dictionary<string, object>();

            // Grid search o random search para optimización de hiperparámetros
            var parameterCombinations = GenerateParameterCombinations(request.ParameterSpace, request.SearchMethod);

            foreach (var parameters in parameterCombinations)
            {
                // Crear modelo con nuevos parámetros
                var testModel = await CreateModelWithParametersAsync(model, parameters);
                
                // Evaluar modelo
                var score = await EvaluateModelScoreAsync(testModel, request.ValidationData);
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestModel = testModel;
                    bestParameters = parameters;
                }
            }

            bestModel.OptimizedParameters = bestParameters;
            bestModel.OptimizationScore = bestScore;

            _logger.LogInformation("Optimized hyperparameters with score {Score}", bestScore);
            return bestModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing hyperparameters");
            throw;
        }
    }

    private IEnumerable<Dictionary<string, object>> GenerateParameterCombinations(
        Dictionary<string, object[]> parameterSpace, 
        SearchMethod method)
    {
        var combinations = new List<Dictionary<string, object>>();

        switch (method)
        {
            case SearchMethod.GridSearch:
                combinations = GenerateGridSearchCombinations(parameterSpace);
                break;
            
            case SearchMethod.RandomSearch:
                combinations = GenerateRandomSearchCombinations(parameterSpace, 100);
                break;
            
            case SearchMethod.BayesianOptimization:
                combinations = GenerateBayesianOptimizationCombinations(parameterSpace, 50);
                break;
        }

        return combinations;
    }

    private List<Dictionary<string, object>> GenerateGridSearchCombinations(Dictionary<string, object[]> parameterSpace)
    {
        var combinations = new List<Dictionary<string, object>>();
        var keys = parameterSpace.Keys.ToArray();
        var values = parameterSpace.Values.ToArray();

        // Generar todas las combinaciones posibles
        var indices = new int[keys.Length];
        
        do
        {
            var combination = new Dictionary<string, object>();
            for (int i = 0; i < keys.Length; i++)
            {
                combination[keys[i]] = values[i][indices[i]];
            }
            combinations.Add(combination);
        }
        while (IncrementIndices(indices, values));

        return combinations;
    }

    private bool IncrementIndices(int[] indices, object[][] values)
    {
        for (int i = indices.Length - 1; i >= 0; i--)
        {
            indices[i]++;
            if (indices[i] < values[i].Length)
            {
                return true;
            }
            indices[i] = 0;
        }
        return false;
    }
}
```

## 🛠️ **Ejercicios Prácticos**

### **Ejercicio 1: Transfer Learning Básico**
```csharp
// Implementar transfer learning simple
public class SimpleTransferLearning
{
    private readonly MLContext _mlContext;

    public SimpleTransferLearning()
    {
        _mlContext = new MLContext(seed: 1);
    }

    public ITransformer FineTuneModel(string pretrainedModelPath, string trainingDataPath)
    {
        // Cargar modelo pre-entrenado
        var pretrainedModel = _mlContext.Model.Load(pretrainedModelPath, out var schema);
        
        // Cargar datos de entrenamiento
        var trainingData = _mlContext.Data.LoadFromTextFile<ImageData>(
            path: trainingDataPath,
            hasHeader: true,
            separatorChar: ',');

        // Pipeline de fine-tuning
        var pipeline = _mlContext.Transforms.LoadRawImageBytes(
                outputColumnName: "Image",
                imageFolder: "images",
                inputColumnName: "ImagePath")
            .Append(_mlContext.Transforms.ResizeImages(
                outputColumnName: "Image",
                imageWidth: 224,
                imageHeight: 224,
                inputColumnName: "Image"))
            .Append(_mlContext.Transforms.ExtractPixels(
                outputColumnName: "Features",
                inputColumnName: "Image"))
            .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label"))
            .Append(_mlContext.MulticlassClassification.Trainers.ImageClassification(
                labelColumnName: "Label",
                featureColumnName: "Features",
                arch: DnnArchitecture.ResnetV2101,
                epoch: 10));

        return pipeline.Fit(trainingData);
    }
}
```

### **Ejercicio 2: Ensemble Simple**
```csharp
// Implementar ensemble básico
public class SimpleEnsemble
{
    private readonly MLContext _mlContext;
    private readonly List<ITransformer> _models;

    public SimpleEnsemble()
    {
        _mlContext = new MLContext(seed: 1);
        _models = new List<ITransformer>();
    }

    public void AddModel(ITransformer model)
    {
        _models.Add(model);
    }

    public string Predict(object input)
    {
        var predictions = new List<string>();
        
        foreach (var model in _models)
        {
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<object, object>(model);
            var prediction = predictionEngine.Predict(input);
            predictions.Add(prediction.ToString());
        }

        // Votación mayoritaria
        var groupedPredictions = predictions.GroupBy(p => p)
            .OrderByDescending(g => g.Count())
            .First();

        return groupedPredictions.Key;
    }
}
```

## 📋 **Resumen de la Clase**

### **Conceptos Clave Aprendidos:**
1. **Transfer Learning**: Fine-tuning de modelos pre-entrenados
2. **Ensemble Methods**: Combinación de múltiples modelos
3. **Custom Models**: Creación de modelos personalizados
4. **Hyperparameter Optimization**: Optimización de parámetros
5. **Advanced Architectures**: Arquitecturas ML avanzadas

### **Habilidades Desarrolladas:**
- Implementación de transfer learning
- Creación de ensembles
- Desarrollo de modelos custom
- Optimización de hiperparámetros
- Técnicas ML avanzadas

### **Próxima Clase:**
**Clase 9: Real-time ML y Edge Computing**
- ML en tiempo real
- Edge computing
- Modelos optimizados
- Inferencia distribuida

---

## 🔗 **Enlaces Útiles**
- [ML.NET Transfer Learning](https://docs.microsoft.com/en-us/dotnet/machine-learning/how-to-guides/train-image-classification-model)
- [Ensemble Methods](https://docs.microsoft.com/en-us/dotnet/machine-learning/how-to-guides/train-multiclass-classification-model)
- [Custom Models](https://docs.microsoft.com/en-us/dotnet/machine-learning/how-to-guides/train-custom-model)
- [Transfer Learning](https://en.wikipedia.org/wiki/Transfer_learning)
- [Ensemble Learning](https://en.wikipedia.org/wiki/Ensemble_learning)
