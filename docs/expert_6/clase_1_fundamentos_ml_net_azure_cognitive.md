# 🧠 **Clase 1: Fundamentos de ML.NET y Azure Cognitive Services**

## 🎯 **Objetivos de la Clase**
- Comprender los fundamentos de ML.NET y su arquitectura
- Integrar Azure Cognitive Services con aplicaciones .NET
- Implementar modelos de machine learning básicos
- Configurar servicios cognitivos en Azure

## 📚 **Contenido Teórico**

### **1. Introducción a ML.NET**

#### **¿Qué es ML.NET?**
```csharp
// ML.NET es el framework de machine learning de Microsoft para .NET
// Permite crear modelos ML sin salir del ecosistema .NET

// Ejemplo básico de pipeline ML.NET
var mlContext = new MLContext();

// Cargar datos
var data = mlContext.Data.LoadFromTextFile<SentimentData>(
    path: "sentiment-data.csv",
    hasHeader: true,
    separatorChar: ',');

// Dividir en entrenamiento y prueba
var split = mlContext.Data.TrainTestSplit(data, testFraction: 0.2);
var trainData = split.TrainSet;
var testData = split.TestSet;

// Configurar pipeline
var pipeline = mlContext.Transforms.Text.FeaturizeText(
        outputColumnName: "Features",
        inputColumnName: nameof(SentimentData.Text))
    .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
        labelColumnName: nameof(SentimentData.Label),
        featureColumnName: "Features"));

// Entrenar modelo
var model = pipeline.Fit(trainData);

// Evaluar modelo
var predictions = model.Transform(testData);
var metrics = mlContext.BinaryClassification.Evaluate(predictions);
```

#### **Arquitectura de ML.NET**
```csharp
// Componentes principales de ML.NET
public class MLNetArchitecture
{
    // 1. MLContext - Punto de entrada principal
    private MLContext _mlContext;
    
    // 2. IDataView - Representación de datos
    private IDataView _data;
    
    // 3. IEstimator - Pipeline de transformaciones
    private IEstimator<ITransformer> _pipeline;
    
    // 4. ITransformer - Modelo entrenado
    private ITransformer _model;
    
    // 5. IDataView - Predicciones
    private IDataView _predictions;
}
```

### **2. Azure Cognitive Services**

#### **Configuración de Cognitive Services**
```csharp
// Configuración de servicios cognitivos
public class CognitiveServicesConfig
{
    public string TextAnalyticsEndpoint { get; set; }
    public string TextAnalyticsKey { get; set; }
    public string ComputerVisionEndpoint { get; set; }
    public string ComputerVisionKey { get; set; }
    public string SpeechEndpoint { get; set; }
    public string SpeechKey { get; set; }
}

// Registro en DI
public void ConfigureServices(IServiceCollection services)
{
    services.Configure<CognitiveServicesConfig>(
        Configuration.GetSection("CognitiveServices"));
    
    services.AddScoped<ITextAnalyticsService, TextAnalyticsService>();
    services.AddScoped<IComputerVisionService, ComputerVisionService>();
    services.AddScoped<ISpeechService, SpeechService>();
}
```

#### **Text Analytics Service**
```csharp
public class TextAnalyticsService : ITextAnalyticsService
{
    private readonly TextAnalyticsClient _client;
    private readonly ILogger<TextAnalyticsService> _logger;

    public TextAnalyticsService(
        TextAnalyticsClient client,
        ILogger<TextAnalyticsService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<SentimentAnalysisResult> AnalyzeSentimentAsync(string text)
    {
        try
        {
            var response = await _client.AnalyzeSentimentAsync(text);
            
            return new SentimentAnalysisResult
            {
                Sentiment = response.Value.Sentiment.ToString(),
                Confidence = response.Value.ConfidenceScores,
                Text = text
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing sentiment for text: {Text}", text);
            throw;
        }
    }

    public async Task<KeyPhraseResult> ExtractKeyPhrasesAsync(string text)
    {
        try
        {
            var response = await _client.ExtractKeyPhrasesAsync(text);
            
            return new KeyPhraseResult
            {
                KeyPhrases = response.Value.ToList(),
                Text = text
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting key phrases for text: {Text}", text);
            throw;
        }
    }

    public async Task<EntityRecognitionResult> RecognizeEntitiesAsync(string text)
    {
        try
        {
            var response = await _client.RecognizeEntitiesAsync(text);
            
            return new EntityRecognitionResult
            {
                Entities = response.Value.Select(e => new Entity
                {
                    Text = e.Text,
                    Category = e.Category.ToString(),
                    Confidence = e.ConfidenceScore
                }).ToList(),
                Text = text
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recognizing entities for text: {Text}", text);
            throw;
        }
    }
}
```

### **3. Computer Vision Service**

#### **Análisis de Imágenes**
```csharp
public class ComputerVisionService : IComputerVisionService
{
    private readonly ComputerVisionClient _client;
    private readonly ILogger<ComputerVisionService> _logger;

    public ComputerVisionService(
        ComputerVisionClient client,
        ILogger<ComputerVisionService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<ImageAnalysisResult> AnalyzeImageAsync(Stream imageStream)
    {
        try
        {
            var features = new List<VisualFeatureTypes>
            {
                VisualFeatureTypes.Categories,
                VisualFeatureTypes.Description,
                VisualFeatureTypes.Faces,
                VisualFeatureTypes.Objects,
                VisualFeatureTypes.Tags
            };

            var analysis = await _client.AnalyzeImageInStreamAsync(imageStream, features);

            return new ImageAnalysisResult
            {
                Categories = analysis.Categories?.Select(c => new Category
                {
                    Name = c.Name,
                    Confidence = c.Score
                }).ToList(),
                Description = analysis.Description?.Captions?.FirstOrDefault()?.Text,
                Tags = analysis.Tags?.Select(t => new Tag
                {
                    Name = t.Name,
                    Confidence = t.Confidence
                }).ToList(),
                Objects = analysis.Objects?.Select(o => new DetectedObject
                {
                    Name = o.ObjectProperty,
                    Confidence = o.Confidence,
                    Rectangle = o.Rectangle
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing image");
            throw;
        }
    }

    public async Task<OcrResult> ExtractTextAsync(Stream imageStream)
    {
        try
        {
            var result = await _client.ReadInStreamAsync(imageStream);
            await result.WaitForCompletionAsync();

            var textResults = result.Value.AnalyzeResult.ReadResults;
            var extractedText = string.Join(" ", 
                textResults.SelectMany(r => r.Lines.Select(l => l.Text)));

            return new OcrResult
            {
                Text = extractedText,
                Confidence = textResults.Average(r => r.Lines.Average(l => l.Confidence))
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from image");
            throw;
        }
    }
}
```

### **4. Speech Service**

#### **Reconocimiento y Síntesis de Voz**
```csharp
public class SpeechService : ISpeechService
{
    private readonly SpeechConfig _speechConfig;
    private readonly ILogger<SpeechService> _logger;

    public SpeechService(
        IConfiguration configuration,
        ILogger<SpeechService> logger)
    {
        _speechConfig = SpeechConfig.FromSubscription(
            configuration["CognitiveServices:SpeechKey"],
            configuration["CognitiveServices:SpeechRegion"]);
        _logger = logger;
    }

    public async Task<string> RecognizeSpeechAsync(Stream audioStream)
    {
        try
        {
            using var audioConfig = AudioConfig.FromStreamInput(audioStream);
            using var speechRecognizer = new SpeechRecognizer(_speechConfig, audioConfig);

            var result = await speechRecognizer.RecognizeOnceAsync();
            
            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                return result.Text;
            }
            else if (result.Reason == ResultReason.NoMatch)
            {
                _logger.LogWarning("No speech could be recognized");
                return string.Empty;
            }
            else
            {
                _logger.LogError("Speech recognition failed: {Reason}", result.Reason);
                throw new Exception($"Speech recognition failed: {result.Reason}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recognizing speech");
            throw;
        }
    }

    public async Task<byte[]> SynthesizeSpeechAsync(string text, string voiceName = "en-US-AriaNeural")
    {
        try
        {
            _speechConfig.SpeechSynthesisVoiceName = voiceName;
            
            using var synthesizer = new SpeechSynthesizer(_speechConfig, null);
            using var result = await synthesizer.SpeakTextAsync(text);

            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                return result.AudioData;
            }
            else
            {
                _logger.LogError("Speech synthesis failed: {Reason}", result.Reason);
                throw new Exception($"Speech synthesis failed: {result.Reason}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synthesizing speech");
            throw;
        }
    }
}
```

## 🛠️ **Ejercicios Prácticos**

### **Ejercicio 1: Configuración de ML.NET**
```csharp
// Configurar un proyecto ML.NET básico
public class MLNetSetup
{
    public void SetupMLContext()
    {
        // Crear MLContext
        var mlContext = new MLContext(seed: 1);
        
        // Configurar logging
        mlContext.Log += (sender, e) =>
        {
            if (e.Kind == ChannelMessageKind.Error)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        };
        
        // Configurar componentes
        var data = mlContext.Data.LoadFromTextFile<SentimentData>(
            path: "data.csv",
            hasHeader: true,
            separatorChar: ',');
        
        Console.WriteLine($"Data loaded: {data.GetRowCount()} rows");
    }
}
```

### **Ejercicio 2: Integración con Text Analytics**
```csharp
// Integrar Text Analytics en una aplicación ASP.NET Core
[ApiController]
[Route("api/[controller]")]
public class TextAnalysisController : ControllerBase
{
    private readonly ITextAnalyticsService _textAnalyticsService;
    private readonly ILogger<TextAnalysisController> _logger;

    public TextAnalysisController(
        ITextAnalyticsService textAnalyticsService,
        ILogger<TextAnalysisController> logger)
    {
        _textAnalyticsService = textAnalyticsService;
        _logger = logger;
    }

    [HttpPost("sentiment")]
    public async Task<ActionResult<SentimentAnalysisResult>> AnalyzeSentiment(
        [FromBody] string text)
    {
        try
        {
            var result = await _textAnalyticsService.AnalyzeSentimentAsync(text);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing sentiment");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("keyphrases")]
    public async Task<ActionResult<KeyPhraseResult>> ExtractKeyPhrases(
        [FromBody] string text)
    {
        try
        {
            var result = await _textAnalyticsService.ExtractKeyPhrasesAsync(text);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting key phrases");
            return StatusCode(500, "Internal server error");
        }
    }
}
```

### **Ejercicio 3: Análisis de Imágenes**
```csharp
// Implementar análisis de imágenes con Computer Vision
[ApiController]
[Route("api/[controller]")]
public class ImageAnalysisController : ControllerBase
{
    private readonly IComputerVisionService _computerVisionService;
    private readonly ILogger<ImageAnalysisController> _logger;

    public ImageAnalysisController(
        IComputerVisionService computerVisionService,
        ILogger<ImageAnalysisController> logger)
    {
        _computerVisionService = computerVisionService;
        _logger = logger;
    }

    [HttpPost("analyze")]
    public async Task<ActionResult<ImageAnalysisResult>> AnalyzeImage(
        IFormFile image)
    {
        try
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("No image provided");
            }

            using var stream = image.OpenReadStream();
            var result = await _computerVisionService.AnalyzeImageAsync(stream);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing image");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("extract-text")]
    public async Task<ActionResult<OcrResult>> ExtractText(
        IFormFile image)
    {
        try
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("No image provided");
            }

            using var stream = image.OpenReadStream();
            var result = await _computerVisionService.ExtractTextAsync(stream);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from image");
            return StatusCode(500, "Internal server error");
        }
    }
}
```

## 📋 **Resumen de la Clase**

### **Conceptos Clave Aprendidos:**
1. **ML.NET Fundamentals**: Framework de ML para .NET
2. **Azure Cognitive Services**: Servicios de IA pre-entrenados
3. **Text Analytics**: Análisis de sentimientos, extracción de frases clave
4. **Computer Vision**: Análisis de imágenes, OCR
5. **Speech Services**: Reconocimiento y síntesis de voz

### **Habilidades Desarrolladas:**
- Configuración de ML.NET
- Integración con Azure Cognitive Services
- Implementación de servicios de IA
- Manejo de errores y logging
- APIs REST para servicios cognitivos

### **Próxima Clase:**
**Clase 2: Recommendation Engines y Sistemas de Recomendación**
- Implementación de sistemas de recomendación
- Filtrado colaborativo y basado en contenido
- Integración con ML.NET
- Optimización de recomendaciones

---

## 🔗 **Enlaces Útiles**
- [ML.NET Documentation](https://docs.microsoft.com/en-us/dotnet/machine-learning/)
- [Azure Cognitive Services](https://azure.microsoft.com/en-us/services/cognitive-services/)
- [Text Analytics API](https://docs.microsoft.com/en-us/azure/cognitive-services/text-analytics/)
- [Computer Vision API](https://docs.microsoft.com/en-us/azure/cognitive-services/computer-vision/)
- [Speech Services](https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/)
