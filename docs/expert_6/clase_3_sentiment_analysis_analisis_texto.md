# 📝 **Clase 3: Sentiment Analysis y Análisis de Texto**

## 🎯 **Objetivos de la Clase**
- Implementar análisis de sentimientos con ML.NET
- Procesar lenguaje natural en aplicaciones .NET
- Clasificar texto automáticamente
- Extraer entidades y frases clave

## 📚 **Contenido Teórico**

### **1. Fundamentos de Análisis de Sentimientos**

#### **Modelo de Datos para Sentiment Analysis**
```csharp
// Estructura de datos para análisis de sentimientos
public class SentimentData
{
    [LoadColumn(0)]
    public string Text { get; set; }

    [LoadColumn(1)]
    public bool Label { get; set; } // true = positivo, false = negativo
}

public class SentimentPrediction
{
    [ColumnName("PredictedLabel")]
    public bool Prediction { get; set; }

    [ColumnName("Probability")]
    public float Probability { get; set; }

    [ColumnName("Score")]
    public float Score { get; set; }
}

// Servicio de análisis de sentimientos
public class SentimentAnalysisService : ISentimentAnalysisService
{
    private readonly MLContext _mlContext;
    private readonly ITransformer _model;
    private readonly ILogger<SentimentAnalysisService> _logger;

    public SentimentAnalysisService(ILogger<SentimentAnalysisService> logger)
    {
        _mlContext = new MLContext(seed: 1);
        _logger = logger;
        
        // Cargar modelo pre-entrenado o entrenar uno nuevo
        _model = LoadOrTrainModel();
    }

    public async Task<SentimentResult> AnalyzeSentimentAsync(string text)
    {
        try
        {
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(_model);
            
            var input = new SentimentData
            {
                Text = text,
                Label = false // Valor dummy para predicción
            };

            var prediction = predictionEngine.Predict(input);
            
            return new SentimentResult
            {
                Text = text,
                Sentiment = prediction.Prediction ? "Positive" : "Negative",
                Confidence = prediction.Probability,
                Score = prediction.Score
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing sentiment for text: {Text}", text);
            throw;
        }
    }

    public async Task<IEnumerable<SentimentResult>> AnalyzeBatchAsync(IEnumerable<string> texts)
    {
        try
        {
            var sentimentData = texts.Select(text => new SentimentData
            {
                Text = text,
                Label = false
            });

            var data = _mlContext.Data.LoadFromEnumerable(sentimentData);
            var predictions = _model.Transform(data);
            
            var results = new List<SentimentResult>();
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(_model);
            
            foreach (var text in texts)
            {
                var input = new SentimentData { Text = text, Label = false };
                var prediction = predictionEngine.Predict(input);
                
                results.Add(new SentimentResult
                {
                    Text = text,
                    Sentiment = prediction.Prediction ? "Positive" : "Negative",
                    Confidence = prediction.Probability,
                    Score = prediction.Score
                });
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing batch sentiment");
            throw;
        }
    }

    private ITransformer LoadOrTrainModel()
    {
        // Intentar cargar modelo pre-entrenado
        var modelPath = "sentiment_model.zip";
        
        if (File.Exists(modelPath))
        {
            try
            {
                return _mlContext.Model.Load(modelPath, out var modelSchema);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load pre-trained model, training new one");
            }
        }

        // Entrenar nuevo modelo
        return TrainNewModel();
    }

    private ITransformer TrainNewModel()
    {
        // Cargar datos de entrenamiento
        var data = _mlContext.Data.LoadFromTextFile<SentimentData>(
            path: "sentiment_training_data.csv",
            hasHeader: true,
            separatorChar: ',');

        // Dividir en entrenamiento y prueba
        var split = _mlContext.Data.TrainTestSplit(data, testFraction: 0.2);

        // Configurar pipeline
        var pipeline = _mlContext.Transforms.Text.FeaturizeText(
                outputColumnName: "Features",
                inputColumnName: nameof(SentimentData.Text))
            .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                labelColumnName: nameof(SentimentData.Label),
                featureColumnName: "Features"));

        // Entrenar modelo
        var model = pipeline.Fit(split.TrainSet);

        // Evaluar modelo
        var predictions = model.Transform(split.TestSet);
        var metrics = _mlContext.BinaryClassification.Evaluate(predictions);
        
        _logger.LogInformation("Model trained with accuracy: {Accuracy}", metrics.Accuracy);

        // Guardar modelo
        _mlContext.Model.Save(model, data.Schema, "sentiment_model.zip");

        return model;
    }
}
```

### **2. Análisis de Texto Avanzado**

#### **Extracción de Características de Texto**
```csharp
public class TextFeatureExtractor
{
    private readonly MLContext _mlContext;

    public TextFeatureExtractor()
    {
        _mlContext = new MLContext(seed: 1);
    }

    public TextFeatures ExtractFeatures(string text)
    {
        return new TextFeatures
        {
            WordCount = CountWords(text),
            CharacterCount = text.Length,
            SentenceCount = CountSentences(text),
            AverageWordLength = CalculateAverageWordLength(text),
            ReadabilityScore = CalculateReadabilityScore(text),
            SentimentScore = AnalyzeSentimentScore(text),
            Language = DetectLanguage(text),
            KeyPhrases = ExtractKeyPhrases(text),
            Entities = ExtractEntities(text)
        };
    }

    private int CountWords(string text)
    {
        return text.Split(new char[] { ' ', '\t', '\n', '\r' }, 
            StringSplitOptions.RemoveEmptyEntries).Length;
    }

    private int CountSentences(string text)
    {
        return text.Split(new char[] { '.', '!', '?' }, 
            StringSplitOptions.RemoveEmptyEntries).Length;
    }

    private double CalculateAverageWordLength(string text)
    {
        var words = text.Split(new char[] { ' ', '\t', '\n', '\r' }, 
            StringSplitOptions.RemoveEmptyEntries);
        
        if (words.Length == 0) return 0;
        
        return words.Average(word => word.Length);
    }

    private double CalculateReadabilityScore(string text)
    {
        // Fórmula de Flesch Reading Ease
        var words = CountWords(text);
        var sentences = CountSentences(text);
        var syllables = CountSyllables(text);

        if (words == 0 || sentences == 0) return 0;

        var score = 206.835 - (1.015 * (words / (double)sentences)) - (84.6 * (syllables / (double)words));
        return Math.Max(0, Math.Min(100, score));
    }

    private int CountSyllables(string text)
    {
        var words = text.Split(new char[] { ' ', '\t', '\n', '\r' }, 
            StringSplitOptions.RemoveEmptyEntries);
        
        int totalSyllables = 0;
        foreach (var word in words)
        {
            totalSyllables += CountSyllablesInWord(word.ToLower());
        }
        
        return totalSyllables;
    }

    private int CountSyllablesInWord(string word)
    {
        if (string.IsNullOrEmpty(word)) return 0;
        
        int syllables = 0;
        bool previousWasVowel = false;
        
        foreach (char c in word)
        {
            bool isVowel = "aeiouy".Contains(c);
            
            if (isVowel && !previousWasVowel)
            {
                syllables++;
            }
            
            previousWasVowel = isVowel;
        }
        
        // Ajustar para palabras que terminan en 'e'
        if (word.EndsWith("e") && syllables > 1)
        {
            syllables--;
        }
        
        return Math.Max(1, syllables);
    }

    private float AnalyzeSentimentScore(string text)
    {
        // Implementación simplificada de análisis de sentimientos
        var positiveWords = new[] { "good", "great", "excellent", "amazing", "wonderful", "fantastic" };
        var negativeWords = new[] { "bad", "terrible", "awful", "horrible", "disappointing", "poor" };
        
        var words = text.ToLower().Split(new char[] { ' ', '\t', '\n', '\r' }, 
            StringSplitOptions.RemoveEmptyEntries);
        
        int positiveCount = words.Count(word => positiveWords.Contains(word));
        int negativeCount = words.Count(word => negativeWords.Contains(word));
        
        if (positiveCount + negativeCount == 0) return 0;
        
        return (positiveCount - negativeCount) / (float)(positiveCount + negativeCount);
    }

    private string DetectLanguage(string text)
    {
        // Implementación simplificada de detección de idioma
        // En producción, usar una librería como LanguageDetection.NET
        
        var englishWords = new[] { "the", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with", "by" };
        var spanishWords = new[] { "el", "la", "de", "que", "y", "a", "en", "un", "es", "se", "no", "te", "lo", "le" };
        
        var words = text.ToLower().Split(new char[] { ' ', '\t', '\n', '\r' }, 
            StringSplitOptions.RemoveEmptyEntries);
        
        int englishCount = words.Count(word => englishWords.Contains(word));
        int spanishCount = words.Count(word => spanishWords.Contains(word));
        
        if (englishCount > spanishCount) return "en";
        if (spanishCount > englishCount) return "es";
        return "unknown";
    }

    private IEnumerable<string> ExtractKeyPhrases(string text)
    {
        // Implementación simplificada de extracción de frases clave
        // En producción, usar Azure Text Analytics o similar
        
        var stopWords = new HashSet<string> { "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with", "by" };
        
        var words = text.ToLower()
            .Split(new char[] { ' ', '\t', '\n', '\r', '.', ',', '!', '?' }, 
                StringSplitOptions.RemoveEmptyEntries)
            .Where(word => word.Length > 3 && !stopWords.Contains(word))
            .GroupBy(word => word)
            .OrderByDescending(group => group.Count())
            .Take(5)
            .Select(group => group.Key);
        
        return words;
    }

    private IEnumerable<Entity> ExtractEntities(string text)
    {
        // Implementación simplificada de extracción de entidades
        // En producción, usar Azure Text Analytics o similar
        
        var entities = new List<Entity>();
        
        // Detectar emails
        var emailPattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";
        var emailMatches = Regex.Matches(text, emailPattern);
        foreach (Match match in emailMatches)
        {
            entities.Add(new Entity
            {
                Text = match.Value,
                Type = "Email",
                Confidence = 0.9f
            });
        }
        
        // Detectar URLs
        var urlPattern = @"https?://[^\s]+";
        var urlMatches = Regex.Matches(text, urlPattern);
        foreach (Match match in urlMatches)
        {
            entities.Add(new Entity
            {
                Text = match.Value,
                Type = "URL",
                Confidence = 0.9f
            });
        }
        
        return entities;
    }
}
```

### **3. Clasificación de Texto**

#### **Sistema de Clasificación Multi-Clase**
```csharp
public class TextClassificationService : ITextClassificationService
{
    private readonly MLContext _mlContext;
    private readonly ITransformer _model;
    private readonly ILogger<TextClassificationService> _logger;

    public TextClassificationService(ILogger<TextClassificationService> logger)
    {
        _mlContext = new MLContext(seed: 1);
        _logger = logger;
        _model = LoadOrTrainClassificationModel();
    }

    public async Task<ClassificationResult> ClassifyTextAsync(string text)
    {
        try
        {
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<TextClassificationData, TextClassificationPrediction>(_model);
            
            var input = new TextClassificationData
            {
                Text = text,
                Category = "" // Valor dummy para predicción
            };

            var prediction = predictionEngine.Predict(input);
            
            return new ClassificationResult
            {
                Text = text,
                PredictedCategory = prediction.PredictedCategory,
                Confidence = prediction.Confidence,
                Scores = prediction.Scores
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error classifying text: {Text}", text);
            throw;
        }
    }

    public async Task<IEnumerable<ClassificationResult>> ClassifyBatchAsync(IEnumerable<string> texts)
    {
        try
        {
            var results = new List<ClassificationResult>();
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<TextClassificationData, TextClassificationPrediction>(_model);
            
            foreach (var text in texts)
            {
                var input = new TextClassificationData { Text = text, Category = "" };
                var prediction = predictionEngine.Predict(input);
                
                results.Add(new ClassificationResult
                {
                    Text = text,
                    PredictedCategory = prediction.PredictedCategory,
                    Confidence = prediction.Confidence,
                    Scores = prediction.Scores
                });
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error classifying batch texts");
            throw;
        }
    }

    private ITransformer LoadOrTrainClassificationModel()
    {
        var modelPath = "text_classification_model.zip";
        
        if (File.Exists(modelPath))
        {
            try
            {
                return _mlContext.Model.Load(modelPath, out var modelSchema);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load pre-trained classification model, training new one");
            }
        }

        return TrainClassificationModel();
    }

    private ITransformer TrainClassificationModel()
    {
        // Cargar datos de entrenamiento para clasificación
        var data = _mlContext.Data.LoadFromTextFile<TextClassificationData>(
            path: "text_classification_training_data.csv",
            hasHeader: true,
            separatorChar: ',');

        var split = _mlContext.Data.TrainTestSplit(data, testFraction: 0.2);

        // Configurar pipeline para clasificación multi-clase
        var pipeline = _mlContext.Transforms.Text.FeaturizeText(
                outputColumnName: "Features",
                inputColumnName: nameof(TextClassificationData.Text))
            .Append(_mlContext.Transforms.Conversion.MapValueToKey(
                outputColumnName: "Label",
                inputColumnName: nameof(TextClassificationData.Category)))
            .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(
                labelColumnName: "Label",
                featureColumnName: "Features"))
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue(
                outputColumnName: "PredictedCategory",
                inputColumnName: "PredictedLabel"));

        var model = pipeline.Fit(split.TrainSet);

        // Evaluar modelo
        var predictions = model.Transform(split.TestSet);
        var metrics = _mlContext.MulticlassClassification.Evaluate(predictions);
        
        _logger.LogInformation("Classification model trained with accuracy: {Accuracy}", metrics.MacroAccuracy);

        // Guardar modelo
        _mlContext.Model.Save(model, data.Schema, modelPath);

        return model;
    }
}
```

### **4. Integración con MussikOn**

#### **Análisis de Reviews y Comentarios**
```csharp
public class ReviewAnalysisService : IReviewAnalysisService
{
    private readonly ISentimentAnalysisService _sentimentService;
    private readonly ITextClassificationService _classificationService;
    private readonly ITextFeatureExtractor _featureExtractor;
    private readonly ILogger<ReviewAnalysisService> _logger;

    public ReviewAnalysisService(
        ISentimentAnalysisService sentimentService,
        ITextClassificationService classificationService,
        ITextFeatureExtractor featureExtractor,
        ILogger<ReviewAnalysisService> logger)
    {
        _sentimentService = sentimentService;
        _classificationService = classificationService;
        _featureExtractor = featureExtractor;
        _logger = logger;
    }

    public async Task<ReviewAnalysisResult> AnalyzeReviewAsync(Review review)
    {
        try
        {
            var text = $"{review.Title} {review.Comment}";
            
            // Análisis de sentimientos
            var sentimentResult = await _sentimentService.AnalyzeSentimentAsync(text);
            
            // Clasificación de categorías
            var classificationResult = await _classificationService.ClassifyTextAsync(text);
            
            // Extracción de características
            var features = _featureExtractor.ExtractFeatures(text);
            
            return new ReviewAnalysisResult
            {
                ReviewId = review.Id,
                Sentiment = sentimentResult.Sentiment,
                SentimentConfidence = sentimentResult.Confidence,
                Category = classificationResult.PredictedCategory,
                CategoryConfidence = classificationResult.Confidence,
                Features = features,
                KeyInsights = ExtractKeyInsights(sentimentResult, classificationResult, features)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing review {ReviewId}", review.Id);
            throw;
        }
    }

    public async Task<IEnumerable<ReviewInsight>> GetReviewInsightsAsync(int musicianId)
    {
        try
        {
            // Obtener todas las reviews del músico
            var reviews = await GetReviewsForMusicianAsync(musicianId);
            
            var insights = new List<ReviewInsight>();
            
            foreach (var review in reviews)
            {
                var analysis = await AnalyzeReviewAsync(review);
                insights.Add(new ReviewInsight
                {
                    ReviewId = review.Id,
                    Analysis = analysis,
                    Timestamp = review.CreatedAt
                });
            }
            
            return insights;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting review insights for musician {MusicianId}", musicianId);
            throw;
        }
    }

    public async Task<MusicianSentimentSummary> GetMusicianSentimentSummaryAsync(int musicianId)
    {
        try
        {
            var insights = await GetReviewInsightsAsync(musicianId);
            
            if (!insights.Any())
            {
                return new MusicianSentimentSummary
                {
                    MusicianId = musicianId,
                    TotalReviews = 0,
                    AverageSentiment = 0,
                    PositivePercentage = 0,
                    NegativePercentage = 0,
                    TrendingTopics = new List<string>()
                };
            }
            
            var positiveCount = insights.Count(i => i.Analysis.Sentiment == "Positive");
            var negativeCount = insights.Count(i => i.Analysis.Sentiment == "Negative");
            var totalCount = insights.Count;
            
            var averageSentiment = insights.Average(i => i.Analysis.SentimentConfidence);
            
            var trendingTopics = insights
                .SelectMany(i => i.Analysis.Features.KeyPhrases)
                .GroupBy(phrase => phrase)
                .OrderByDescending(group => group.Count())
                .Take(5)
                .Select(group => group.Key);
            
            return new MusicianSentimentSummary
            {
                MusicianId = musicianId,
                TotalReviews = totalCount,
                AverageSentiment = averageSentiment,
                PositivePercentage = (positiveCount / (float)totalCount) * 100,
                NegativePercentage = (negativeCount / (float)totalCount) * 100,
                TrendingTopics = trendingTopics.ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sentiment summary for musician {MusicianId}", musicianId);
            throw;
        }
    }

    private IEnumerable<string> ExtractKeyInsights(
        SentimentResult sentiment, 
        ClassificationResult classification, 
        TextFeatures features)
    {
        var insights = new List<string>();
        
        if (sentiment.Confidence > 0.8f)
        {
            insights.Add($"Strong {sentiment.Sentiment.ToLower()} sentiment detected");
        }
        
        if (features.ReadabilityScore > 70)
        {
            insights.Add("Highly readable content");
        }
        else if (features.ReadabilityScore < 30)
        {
            insights.Add("Complex content requiring attention");
        }
        
        if (features.KeyPhrases.Any())
        {
            insights.Add($"Key topics: {string.Join(", ", features.KeyPhrases.Take(3))}");
        }
        
        return insights;
    }

    private async Task<IEnumerable<Review>> GetReviewsForMusicianAsync(int musicianId)
    {
        // Implementar lógica para obtener reviews del músico
        // Esto dependería de tu repositorio específico
        await Task.CompletedTask;
        return new List<Review>();
    }
}
```

## 🛠️ **Ejercicios Prácticos**

### **Ejercicio 1: Análisis de Sentimientos Básico**
```csharp
// Implementar análisis de sentimientos simple
public class BasicSentimentAnalyzer
{
    private readonly MLContext _mlContext;
    private ITransformer _model;

    public void TrainModel(IEnumerable<SentimentData> trainingData)
    {
        _mlContext = new MLContext(seed: 1);
        
        var data = _mlContext.Data.LoadFromEnumerable(trainingData);
        var split = _mlContext.Data.TrainTestSplit(data, testFraction: 0.2);
        
        var pipeline = _mlContext.Transforms.Text.FeaturizeText(
                outputColumnName: "Features",
                inputColumnName: nameof(SentimentData.Text))
            .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                labelColumnName: nameof(SentimentData.Label),
                featureColumnName: "Features"));

        _model = pipeline.Fit(split.TrainSet);
        
        // Evaluar modelo
        var predictions = _model.Transform(split.TestSet);
        var metrics = _mlContext.BinaryClassification.Evaluate(predictions);
        
        Console.WriteLine($"Accuracy: {metrics.Accuracy}");
        Console.WriteLine($"F1 Score: {metrics.F1Score}");
    }

    public SentimentResult Analyze(string text)
    {
        var predictionEngine = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(_model);
        
        var input = new SentimentData { Text = text, Label = false };
        var prediction = predictionEngine.Predict(input);
        
        return new SentimentResult
        {
            Text = text,
            Sentiment = prediction.Prediction ? "Positive" : "Negative",
            Confidence = prediction.Probability
        };
    }
}
```

### **Ejercicio 2: API de Análisis de Texto**
```csharp
// Crear API REST para análisis de texto
[ApiController]
[Route("api/[controller]")]
public class TextAnalysisController : ControllerBase
{
    private readonly ISentimentAnalysisService _sentimentService;
    private readonly ITextClassificationService _classificationService;
    private readonly ITextFeatureExtractor _featureExtractor;
    private readonly ILogger<TextAnalysisController> _logger;

    public TextAnalysisController(
        ISentimentAnalysisService sentimentService,
        ITextClassificationService classificationService,
        ITextFeatureExtractor featureExtractor,
        ILogger<TextAnalysisController> logger)
    {
        _sentimentService = sentimentService;
        _classificationService = classificationService;
        _featureExtractor = featureExtractor;
        _logger = logger;
    }

    [HttpPost("sentiment")]
    public async Task<ActionResult<SentimentResult>> AnalyzeSentiment([FromBody] string text)
    {
        try
        {
            var result = await _sentimentService.AnalyzeSentimentAsync(text);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing sentiment");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("classify")]
    public async Task<ActionResult<ClassificationResult>> ClassifyText([FromBody] string text)
    {
        try
        {
            var result = await _classificationService.ClassifyTextAsync(text);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error classifying text");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("features")]
    public ActionResult<TextFeatures> ExtractFeatures([FromBody] string text)
    {
        try
        {
            var features = _featureExtractor.ExtractFeatures(text);
            return Ok(features);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting features");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("analyze")]
    public async Task<ActionResult<ComprehensiveTextAnalysis>> AnalyzeComprehensive([FromBody] string text)
    {
        try
        {
            var sentimentTask = _sentimentService.AnalyzeSentimentAsync(text);
            var classificationTask = _classificationService.ClassifyTextAsync(text);
            var featuresTask = Task.Run(() => _featureExtractor.ExtractFeatures(text));

            await Task.WhenAll(sentimentTask, classificationTask, featuresTask);

            var result = new ComprehensiveTextAnalysis
            {
                Text = text,
                Sentiment = await sentimentTask,
                Classification = await classificationTask,
                Features = featuresTask.Result
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing comprehensive analysis");
            return StatusCode(500, "Internal server error");
        }
    }
}
```

### **Ejercicio 3: Sistema de Monitoreo de Sentimientos**
```csharp
// Sistema de monitoreo en tiempo real de sentimientos
public class SentimentMonitoringService : ISentimentMonitoringService
{
    private readonly ISentimentAnalysisService _sentimentService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SentimentMonitoringService> _logger;
    
    private readonly TimeSpan _monitoringInterval = TimeSpan.FromMinutes(5);

    public SentimentMonitoringService(
        ISentimentAnalysisService sentimentService,
        IMemoryCache cache,
        ILogger<SentimentMonitoringService> logger)
    {
        _sentimentService = sentimentService;
        _cache = cache;
        _logger = logger;
    }

    public async Task StartMonitoringAsync(int musicianId)
    {
        var timer = new Timer(async _ => await MonitorMusicianSentimentAsync(musicianId), 
            null, TimeSpan.Zero, _monitoringInterval);
        
        _logger.LogInformation("Started sentiment monitoring for musician {MusicianId}", musicianId);
    }

    private async Task MonitorMusicianSentimentAsync(int musicianId)
    {
        try
        {
            // Obtener reviews recientes
            var recentReviews = await GetRecentReviewsAsync(musicianId);
            
            if (!recentReviews.Any())
            {
                return;
            }

            // Analizar sentimientos
            var sentimentResults = await _sentimentService.AnalyzeBatchAsync(
                recentReviews.Select(r => r.Comment));

            // Calcular métricas
            var positiveCount = sentimentResults.Count(r => r.Sentiment == "Positive");
            var negativeCount = sentimentResults.Count(r => r.Sentiment == "Negative");
            var totalCount = sentimentResults.Count();

            var sentimentTrend = new SentimentTrend
            {
                MusicianId = musicianId,
                Timestamp = DateTime.UtcNow,
                PositiveCount = positiveCount,
                NegativeCount = negativeCount,
                TotalCount = totalCount,
                PositivePercentage = (positiveCount / (float)totalCount) * 100,
                NegativePercentage = (negativeCount / (float)totalCount) * 100,
                AverageConfidence = sentimentResults.Average(r => r.Confidence)
            };

            // Almacenar en cache
            var cacheKey = $"sentiment_trend_{musicianId}";
            _cache.Set(cacheKey, sentimentTrend, TimeSpan.FromHours(1));

            // Verificar alertas
            await CheckSentimentAlertsAsync(sentimentTrend);

            _logger.LogInformation("Updated sentiment monitoring for musician {MusicianId}", musicianId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error monitoring sentiment for musician {MusicianId}", musicianId);
        }
    }

    private async Task CheckSentimentAlertsAsync(SentimentTrend trend)
    {
        // Verificar si hay alertas de sentimiento negativo
        if (trend.NegativePercentage > 70)
        {
            await TriggerNegativeSentimentAlertAsync(trend);
        }

        // Verificar si hay una caída significativa en sentimiento positivo
        var previousTrend = _cache.Get<SentimentTrend>($"sentiment_trend_{trend.MusicianId}_previous");
        if (previousTrend != null && 
            trend.PositivePercentage < previousTrend.PositivePercentage - 20)
        {
            await TriggerSentimentDropAlertAsync(trend, previousTrend);
        }

        // Actualizar tendencia anterior
        _cache.Set($"sentiment_trend_{trend.MusicianId}_previous", trend, TimeSpan.FromHours(2));
    }

    private async Task TriggerNegativeSentimentAlertAsync(SentimentTrend trend)
    {
        _logger.LogWarning("Negative sentiment alert for musician {MusicianId}: {NegativePercentage}% negative", 
            trend.MusicianId, trend.NegativePercentage);
        
        // Implementar lógica de alerta (email, notificación, etc.)
        await Task.CompletedTask;
    }

    private async Task TriggerSentimentDropAlertAsync(SentimentTrend current, SentimentTrend previous)
    {
        _logger.LogWarning("Sentiment drop alert for musician {MusicianId}: {Previous}% -> {Current}% positive", 
            current.MusicianId, previous.PositivePercentage, current.PositivePercentage);
        
        // Implementar lógica de alerta
        await Task.CompletedTask;
    }

    private async Task<IEnumerable<Review>> GetRecentReviewsAsync(int musicianId)
    {
        // Implementar lógica para obtener reviews recientes
        await Task.CompletedTask;
        return new List<Review>();
    }
}
```

## 📋 **Resumen de la Clase**

### **Conceptos Clave Aprendidos:**
1. **Análisis de Sentimientos**: Clasificación binaria de texto
2. **Extracción de Características**: Métricas de legibilidad y complejidad
3. **Clasificación de Texto**: Categorización multi-clase
4. **Procesamiento de Lenguaje Natural**: Análisis de texto avanzado
5. **Monitoreo en Tiempo Real**: Alertas y tendencias

### **Habilidades Desarrolladas:**
- Implementación de análisis de sentimientos
- Extracción de características de texto
- Clasificación automática de contenido
- APIs REST para análisis de texto
- Sistemas de monitoreo y alertas

### **Próxima Clase:**
**Clase 4: Image Recognition y Computer Vision**
- Reconocimiento de imágenes con ML.NET
- Clasificación de imágenes
- Detección de objetos
- Análisis de contenido visual

---

## 🔗 **Enlaces Útiles**
- [ML.NET Text Classification](https://docs.microsoft.com/en-us/dotnet/machine-learning/tutorials/sentiment-analysis)
- [Text Featurization](https://docs.microsoft.com/en-us/dotnet/machine-learning/how-to-guides/prepare-data-ml-net)
- [Natural Language Processing](https://docs.microsoft.com/en-us/azure/cognitive-services/text-analytics/)
- [Sentiment Analysis](https://en.wikipedia.org/wiki/Sentiment_analysis)
- [Text Classification](https://en.wikipedia.org/wiki/Document_classification)
