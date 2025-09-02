# 🎯 **Clase 2: Recommendation Engines y Sistemas de Recomendación**

## 🎯 **Objetivos de la Clase**
- Implementar sistemas de recomendación con ML.NET
- Comprender filtrado colaborativo y basado en contenido
- Crear engines de recomendación personalizados
- Optimizar algoritmos de recomendación

## 📚 **Contenido Teórico**

### **1. Fundamentos de Sistemas de Recomendación**

#### **Tipos de Sistemas de Recomendación**
```csharp
// 1. Filtrado Colaborativo (Collaborative Filtering)
public class CollaborativeFilteringEngine
{
    private readonly MLContext _mlContext;
    private ITransformer _model;

    public CollaborativeFilteringEngine()
    {
        _mlContext = new MLContext(seed: 1);
    }

    public void TrainModel(IEnumerable<UserRating> ratings)
    {
        // Cargar datos de ratings
        var data = _mlContext.Data.LoadFromEnumerable(ratings);
        
        // Configurar pipeline para filtrado colaborativo
        var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("UserId")
            .Append(_mlContext.Transforms.Conversion.MapValueToKey("ItemId"))
            .Append(_mlContext.Recommendation().Trainers.MatrixFactorization(
                labelColumnName: "Rating",
                matrixColumnIndexColumnName: "UserId",
                matrixRowIndexColumnName: "ItemId",
                numberOfIterations: 20,
                approximationRank: 8));

        // Entrenar modelo
        _model = pipeline.Fit(data);
    }

    public float PredictRating(int userId, int itemId)
    {
        var predictionEngine = _mlContext.Model.CreatePredictionEngine<UserRating, RatingPrediction>(_model);
        
        var input = new UserRating
        {
            UserId = userId,
            ItemId = itemId,
            Rating = 0 // Valor dummy para predicción
        };

        var prediction = predictionEngine.Predict(input);
        return prediction.Score;
    }
}

// 2. Filtrado Basado en Contenido (Content-Based Filtering)
public class ContentBasedFilteringEngine
{
    private readonly MLContext _mlContext;
    private ITransformer _model;

    public void TrainModel(IEnumerable<ItemFeature> items, IEnumerable<UserPreference> preferences)
    {
        var data = _mlContext.Data.LoadFromEnumerable(
            items.Join(preferences, 
                item => item.ItemId, 
                pref => pref.ItemId, 
                (item, pref) => new { item, pref }));

        var pipeline = _mlContext.Transforms.Concatenate("Features",
                nameof(ItemFeature.Genre),
                nameof(ItemFeature.Year),
                nameof(ItemFeature.Duration))
            .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
            .Append(_mlContext.Regression.Trainers.Sdca(
                labelColumnName: nameof(UserPreference.Rating),
                featureColumnName: "Features"));

        _model = pipeline.Fit(data);
    }
}
```

### **2. Sistema de Recomendación para MussikOn**

#### **Recomendación de Músicos**
```csharp
public class MusicianRecommendationService : IMusicianRecommendationService
{
    private readonly MLContext _mlContext;
    private readonly IMusicianRepository _musicianRepository;
    private readonly IEventRepository _eventRepository;
    private readonly ILogger<MusicianRecommendationService> _logger;
    
    private ITransformer _collaborativeModel;
    private ITransformer _contentBasedModel;

    public MusicianRecommendationService(
        IMusicianRepository musicianRepository,
        IEventRepository eventRepository,
        ILogger<MusicianRecommendationService> logger)
    {
        _mlContext = new MLContext(seed: 1);
        _musicianRepository = musicianRepository;
        _eventRepository = eventRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<MusicianRecommendation>> GetRecommendationsAsync(
        int eventId, 
        int limit = 10)
    {
        try
        {
            var eventData = await _eventRepository.GetByIdAsync(eventId);
            if (eventData == null)
            {
                throw new NotFoundException($"Event {eventId} not found");
            }

            // Obtener recomendaciones usando múltiples algoritmos
            var collaborativeRecommendations = await GetCollaborativeRecommendationsAsync(eventId, limit);
            var contentBasedRecommendations = await GetContentBasedRecommendationsAsync(eventData, limit);
            var hybridRecommendations = await GetHybridRecommendationsAsync(
                collaborativeRecommendations, 
                contentBasedRecommendations, 
                limit);

            return hybridRecommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting musician recommendations for event {EventId}", eventId);
            throw;
        }
    }

    private async Task<IEnumerable<MusicianRecommendation>> GetCollaborativeRecommendationsAsync(
        int eventId, 
        int limit)
    {
        // Obtener ratings históricos
        var historicalRatings = await _eventRepository.GetHistoricalRatingsAsync();
        
        if (!historicalRatings.Any())
        {
            return Enumerable.Empty<MusicianRecommendation>();
        }

        // Entrenar modelo colaborativo
        var data = _mlContext.Data.LoadFromEnumerable(historicalRatings);
        var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("EventId")
            .Append(_mlContext.Transforms.Conversion.MapValueToKey("MusicianId"))
            .Append(_mlContext.Recommendation().Trainers.MatrixFactorization(
                labelColumnName: "Rating",
                matrixColumnIndexColumnName: "EventId",
                matrixRowIndexColumnName: "MusicianId"));

        var model = pipeline.Fit(data);
        var predictionEngine = _mlContext.Model.CreatePredictionEngine<EventMusicianRating, MusicianRatingPrediction>(model);

        // Obtener todos los músicos disponibles
        var availableMusicians = await _musicianRepository.GetAvailableMusiciansAsync();
        
        var recommendations = new List<MusicianRecommendation>();
        
        foreach (var musician in availableMusicians)
        {
            var input = new EventMusicianRating
            {
                EventId = eventId,
                MusicianId = musician.Id,
                Rating = 0
            };

            var prediction = predictionEngine.Predict(input);
            
            recommendations.Add(new MusicianRecommendation
            {
                MusicianId = musician.Id,
                Musician = musician,
                Score = prediction.Score,
                Algorithm = "Collaborative Filtering"
            });
        }

        return recommendations
            .OrderByDescending(r => r.Score)
            .Take(limit);
    }

    private async Task<IEnumerable<MusicianRecommendation>> GetContentBasedRecommendationsAsync(
        Event eventData, 
        int limit)
    {
        // Obtener características del evento
        var eventFeatures = ExtractEventFeatures(eventData);
        
        // Obtener músicos con sus características
        var musicians = await _musicianRepository.GetAllAsync();
        
        var recommendations = new List<MusicianRecommendation>();
        
        foreach (var musician in musicians)
        {
            var musicianFeatures = ExtractMusicianFeatures(musician);
            var similarity = CalculateCosineSimilarity(eventFeatures, musicianFeatures);
            
            recommendations.Add(new MusicianRecommendation
            {
                MusicianId = musician.Id,
                Musician = musician,
                Score = similarity,
                Algorithm = "Content-Based Filtering"
            });
        }

        return recommendations
            .OrderByDescending(r => r.Score)
            .Take(limit);
    }

    private async Task<IEnumerable<MusicianRecommendation>> GetHybridRecommendationsAsync(
        IEnumerable<MusicianRecommendation> collaborative,
        IEnumerable<MusicianRecommendation> contentBased,
        int limit)
    {
        // Combinar recomendaciones usando weighted average
        var hybridRecommendations = new Dictionary<int, MusicianRecommendation>();
        
        // Peso para filtrado colaborativo
        var collaborativeWeight = 0.6f;
        var contentBasedWeight = 0.4f;

        // Procesar recomendaciones colaborativas
        foreach (var rec in collaborative)
        {
            hybridRecommendations[rec.MusicianId] = new MusicianRecommendation
            {
                MusicianId = rec.MusicianId,
                Musician = rec.Musician,
                Score = rec.Score * collaborativeWeight,
                Algorithm = "Hybrid"
            };
        }

        // Procesar recomendaciones basadas en contenido
        foreach (var rec in contentBased)
        {
            if (hybridRecommendations.ContainsKey(rec.MusicianId))
            {
                hybridRecommendations[rec.MusicianId].Score += rec.Score * contentBasedWeight;
            }
            else
            {
                hybridRecommendations[rec.MusicianId] = new MusicianRecommendation
                {
                    MusicianId = rec.MusicianId,
                    Musician = rec.Musician,
                    Score = rec.Score * contentBasedWeight,
                    Algorithm = "Hybrid"
                };
            }
        }

        return hybridRecommendations.Values
            .OrderByDescending(r => r.Score)
            .Take(limit);
    }
}
```

### **3. Algoritmos de Similitud**

#### **Cálculo de Similitud**
```csharp
public class SimilarityCalculator
{
    // Similitud del coseno
    public static float CalculateCosineSimilarity(float[] vectorA, float[] vectorB)
    {
        if (vectorA.Length != vectorB.Length)
        {
            throw new ArgumentException("Vectors must have the same length");
        }

        float dotProduct = 0;
        float magnitudeA = 0;
        float magnitudeB = 0;

        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            magnitudeA += vectorA[i] * vectorA[i];
            magnitudeB += vectorB[i] * vectorB[i];
        }

        magnitudeA = (float)Math.Sqrt(magnitudeA);
        magnitudeB = (float)Math.Sqrt(magnitudeB);

        if (magnitudeA == 0 || magnitudeB == 0)
        {
            return 0;
        }

        return dotProduct / (magnitudeA * magnitudeB);
    }

    // Similitud de Jaccard
    public static float CalculateJaccardSimilarity<T>(IEnumerable<T> setA, IEnumerable<T> setB)
    {
        var setAHash = new HashSet<T>(setA);
        var setBHash = new HashSet<T>(setB);

        var intersection = setAHash.Intersect(setBHash).Count();
        var union = setAHash.Union(setBHash).Count();

        if (union == 0)
        {
            return 0;
        }

        return (float)intersection / union;
    }

    // Distancia euclidiana
    public static float CalculateEuclideanDistance(float[] vectorA, float[] vectorB)
    {
        if (vectorA.Length != vectorB.Length)
        {
            throw new ArgumentException("Vectors must have the same length");
        }

        float sum = 0;
        for (int i = 0; i < vectorA.Length; i++)
        {
            float difference = vectorA[i] - vectorB[i];
            sum += difference * difference;
        }

        return (float)Math.Sqrt(sum);
    }
}
```

### **4. Sistema de Recomendación en Tiempo Real**

#### **Recomendaciones Dinámicas**
```csharp
public class RealTimeRecommendationService : IRealTimeRecommendationService
{
    private readonly IMemoryCache _cache;
    private readonly IMusicianRecommendationService _recommendationService;
    private readonly ILogger<RealTimeRecommendationService> _logger;
    
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(15);

    public RealTimeRecommendationService(
        IMemoryCache cache,
        IMusicianRecommendationService recommendationService,
        ILogger<RealTimeRecommendationService> logger)
    {
        _cache = cache;
        _recommendationService = recommendationService;
        _logger = logger;
    }

    public async Task<IEnumerable<MusicianRecommendation>> GetCachedRecommendationsAsync(
        int eventId, 
        int limit = 10)
    {
        var cacheKey = $"recommendations_{eventId}_{limit}";
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<MusicianRecommendation> cachedRecommendations))
        {
            _logger.LogInformation("Returning cached recommendations for event {EventId}", eventId);
            return cachedRecommendations;
        }

        // Generar nuevas recomendaciones
        var recommendations = await _recommendationService.GetRecommendationsAsync(eventId, limit);
        
        // Cachear resultados
        _cache.Set(cacheKey, recommendations, _cacheExpiration);
        
        _logger.LogInformation("Generated and cached new recommendations for event {EventId}", eventId);
        return recommendations;
    }

    public void InvalidateRecommendations(int eventId)
    {
        // Invalidar cache cuando hay cambios relevantes
        var keysToRemove = new List<string>();
        
        // Buscar todas las claves relacionadas con el evento
        if (_cache is MemoryCache memoryCache)
        {
            var field = typeof(MemoryCache).GetField("_coherentState", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (field?.GetValue(memoryCache) is object coherentState)
            {
                var entriesCollection = coherentState.GetType()
                    .GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (entriesCollection?.GetValue(coherentState) is IDictionary entries)
                {
                    foreach (DictionaryEntry entry in entries)
                    {
                        if (entry.Key.ToString().Contains($"recommendations_{eventId}_"))
                        {
                            keysToRemove.Add(entry.Key.ToString());
                        }
                    }
                }
            }
        }

        // Remover claves del cache
        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
        }

        _logger.LogInformation("Invalidated recommendations cache for event {EventId}", eventId);
    }
}
```

## 🛠️ **Ejercicios Prácticos**

### **Ejercicio 1: Sistema de Recomendación Básico**
```csharp
// Implementar un sistema de recomendación simple
public class BasicRecommendationEngine
{
    private readonly MLContext _mlContext;
    private ITransformer _model;

    public void TrainModel(IEnumerable<RatingData> ratings)
    {
        _mlContext = new MLContext(seed: 1);
        
        var data = _mlContext.Data.LoadFromEnumerable(ratings);
        
        var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("UserId")
            .Append(_mlContext.Transforms.Conversion.MapValueToKey("ItemId"))
            .Append(_mlContext.Recommendation().Trainers.MatrixFactorization(
                labelColumnName: "Rating",
                matrixColumnIndexColumnName: "UserId",
                matrixRowIndexColumnName: "ItemId"));

        _model = pipeline.Fit(data);
    }

    public IEnumerable<RecommendationResult> GetTopRecommendations(int userId, int topN = 5)
    {
        var predictionEngine = _mlContext.Model.CreatePredictionEngine<RatingData, RatingPrediction>(_model);
        
        // Simular todos los items disponibles
        var allItems = Enumerable.Range(1, 1000);
        
        var recommendations = allItems
            .Select(itemId => new
            {
                ItemId = itemId,
                Score = predictionEngine.Predict(new RatingData
                {
                    UserId = userId,
                    ItemId = itemId,
                    Rating = 0
                }).Score
            })
            .OrderByDescending(r => r.Score)
            .Take(topN)
            .Select(r => new RecommendationResult
            {
                ItemId = r.ItemId,
                Score = r.Score
            });

        return recommendations;
    }
}
```

### **Ejercicio 2: API de Recomendaciones**
```csharp
// Crear API REST para recomendaciones
[ApiController]
[Route("api/[controller]")]
public class RecommendationsController : ControllerBase
{
    private readonly IMusicianRecommendationService _recommendationService;
    private readonly IRealTimeRecommendationService _realTimeService;
    private readonly ILogger<RecommendationsController> _logger;

    public RecommendationsController(
        IMusicianRecommendationService recommendationService,
        IRealTimeRecommendationService realTimeService,
        ILogger<RecommendationsController> logger)
    {
        _recommendationService = recommendationService;
        _realTimeService = realTimeService;
        _logger = logger;
    }

    [HttpGet("event/{eventId}")]
    public async Task<ActionResult<IEnumerable<MusicianRecommendation>>> GetEventRecommendations(
        int eventId, 
        [FromQuery] int limit = 10)
    {
        try
        {
            var recommendations = await _realTimeService.GetCachedRecommendationsAsync(eventId, limit);
            return Ok(recommendations);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommendations for event {EventId}", eventId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("event/{eventId}/feedback")]
    public async Task<ActionResult> SubmitFeedback(
        int eventId, 
        [FromBody] RecommendationFeedback feedback)
    {
        try
        {
            // Procesar feedback del usuario
            await ProcessFeedbackAsync(eventId, feedback);
            
            // Invalidar cache para regenerar recomendaciones
            _realTimeService.InvalidateRecommendations(eventId);
            
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing feedback for event {EventId}", eventId);
            return StatusCode(500, "Internal server error");
        }
    }

    private async Task ProcessFeedbackAsync(int eventId, RecommendationFeedback feedback)
    {
        // Implementar lógica para procesar feedback
        // Esto podría incluir actualizar ratings, preferencias, etc.
        await Task.CompletedTask;
    }
}
```

### **Ejercicio 3: Evaluación de Recomendaciones**
```csharp
// Sistema de evaluación de recomendaciones
public class RecommendationEvaluator
{
    private readonly MLContext _mlContext;

    public RecommendationEvaluator()
    {
        _mlContext = new MLContext(seed: 1);
    }

    public RecommendationMetrics EvaluateModel(
        ITransformer model, 
        IDataView testData)
    {
        var predictions = model.Transform(testData);
        
        // Calcular métricas de regresión
        var regressionMetrics = _mlContext.Regression.Evaluate(predictions);
        
        // Calcular métricas específicas de recomendación
        var recommendationMetrics = new RecommendationMetrics
        {
            RMSE = regressionMetrics.RootMeanSquaredError,
            MAE = regressionMetrics.MeanAbsoluteError,
            R2 = regressionMetrics.RSquared,
            Precision = CalculatePrecision(predictions),
            Recall = CalculateRecall(predictions),
            F1Score = CalculateF1Score(predictions)
        };

        return recommendationMetrics;
    }

    private float CalculatePrecision(IDataView predictions)
    {
        // Implementar cálculo de precisión
        // Para recomendaciones, esto podría ser la proporción de items recomendados
        // que el usuario realmente le gustó
        return 0.0f; // Placeholder
    }

    private float CalculateRecall(IDataView predictions)
    {
        // Implementar cálculo de recall
        // Para recomendaciones, esto podría ser la proporción de items que le gustaron
        // al usuario que fueron recomendados
        return 0.0f; // Placeholder
    }

    private float CalculateF1Score(IDataView predictions)
    {
        var precision = CalculatePrecision(predictions);
        var recall = CalculateRecall(predictions);
        
        if (precision + recall == 0)
        {
            return 0;
        }
        
        return 2 * (precision * recall) / (precision + recall);
    }
}
```

## 📋 **Resumen de la Clase**

### **Conceptos Clave Aprendidos:**
1. **Sistemas de Recomendación**: Filtrado colaborativo y basado en contenido
2. **ML.NET Recommendation**: Algoritmos de factorización de matrices
3. **Algoritmos Híbridos**: Combinación de múltiples enfoques
4. **Cálculo de Similitud**: Coseno, Jaccard, Euclidiana
5. **Recomendaciones en Tiempo Real**: Cache y invalidación

### **Habilidades Desarrolladas:**
- Implementación de engines de recomendación
- Integración con ML.NET
- Optimización de algoritmos
- APIs REST para recomendaciones
- Evaluación de modelos

### **Próxima Clase:**
**Clase 3: Sentiment Analysis y Análisis de Texto**
- Análisis de sentimientos con ML.NET
- Procesamiento de lenguaje natural
- Clasificación de texto
- Extracción de entidades

---

## 🔗 **Enlaces Útiles**
- [ML.NET Recommendation](https://docs.microsoft.com/en-us/dotnet/machine-learning/tutorials/movie-recommendation)
- [Matrix Factorization](https://docs.microsoft.com/en-us/dotnet/machine-learning/how-to-guides/train-matrix-factorization)
- [Recommendation Systems](https://en.wikipedia.org/wiki/Recommender_system)
- [Collaborative Filtering](https://en.wikipedia.org/wiki/Collaborative_filtering)
- [Content-Based Filtering](https://en.wikipedia.org/wiki/Content-based_filtering)
