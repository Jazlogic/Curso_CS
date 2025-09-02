# 🖼️ **Clase 4: Image Recognition y Computer Vision**

## 🎯 **Objetivos de la Clase**
- Implementar reconocimiento de imágenes con ML.NET
- Clasificar imágenes automáticamente
- Detectar objetos en imágenes
- Analizar contenido visual para MussikOn

## 📚 **Contenido Teórico**

### **1. Fundamentos de Computer Vision con ML.NET**

#### **Modelo de Datos para Clasificación de Imágenes**
```csharp
// Estructura de datos para clasificación de imágenes
public class ImageData
{
    [LoadColumn(0)]
    public string ImagePath { get; set; }

    [LoadColumn(1)]
    public string Label { get; set; }
}

public class ImagePrediction
{
    [ColumnName("PredictedLabel")]
    public string PredictedLabel { get; set; }

    [ColumnName("Score")]
    public float[] Score { get; set; }
}

// Servicio de reconocimiento de imágenes
public class ImageRecognitionService : IImageRecognitionService
{
    private readonly MLContext _mlContext;
    private readonly ITransformer _model;
    private readonly ILogger<ImageRecognitionService> _logger;

    public ImageRecognitionService(ILogger<ImageRecognitionService> logger)
    {
        _mlContext = new MLContext(seed: 1);
        _logger = logger;
        _model = LoadOrTrainImageModel();
    }

    public async Task<ImageRecognitionResult> RecognizeImageAsync(Stream imageStream)
    {
        try
        {
            // Guardar imagen temporalmente
            var tempPath = Path.GetTempFileName() + ".jpg";
            using (var fileStream = new FileStream(tempPath, FileMode.Create))
            {
                await imageStream.CopyToAsync(fileStream);
            }

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<ImageData, ImagePrediction>(_model);
            
            var input = new ImageData
            {
                ImagePath = tempPath,
                Label = "" // Valor dummy para predicción
            };

            var prediction = predictionEngine.Predict(input);
            
            // Limpiar archivo temporal
            File.Delete(tempPath);

            return new ImageRecognitionResult
            {
                PredictedLabel = prediction.PredictedLabel,
                Confidence = prediction.Score.Max(),
                AllScores = prediction.Score
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recognizing image");
            throw;
        }
    }

    public async Task<IEnumerable<ImageRecognitionResult>> RecognizeBatchAsync(IEnumerable<Stream> imageStreams)
    {
        try
        {
            var results = new List<ImageRecognitionResult>();
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<ImageData, ImagePrediction>(_model);
            
            foreach (var imageStream in imageStreams)
            {
                var result = await RecognizeImageAsync(imageStream);
                results.Add(result);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recognizing batch images");
            throw;
        }
    }

    private ITransformer LoadOrTrainImageModel()
    {
        var modelPath = "image_recognition_model.zip";
        
        if (File.Exists(modelPath))
        {
            try
            {
                return _mlContext.Model.Load(modelPath, out var modelSchema);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load pre-trained image model, training new one");
            }
        }

        return TrainImageModel();
    }

    private ITransformer TrainImageModel()
    {
        // Cargar datos de entrenamiento
        var data = _mlContext.Data.LoadFromTextFile<ImageData>(
            path: "image_training_data.csv",
            hasHeader: true,
            separatorChar: ',');

        var split = _mlContext.Data.TrainTestSplit(data, testFraction: 0.2);

        // Configurar pipeline para clasificación de imágenes
        var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label")
            .Append(_mlContext.Transforms.LoadRawImageBytes(
                outputColumnName: "Image",
                imageFolder: "images",
                inputColumnName: "ImagePath"))
            .Append(_mlContext.Transforms.ResizeImages(
                outputColumnName: "Image",
                imageWidth: 224,
                imageHeight: 224,
                inputColumnName: "Image"))
            .Append(_mlContext.Transforms.ExtractPixels(
                outputColumnName: "Features",
                inputColumnName: "Image"))
            .Append(_mlContext.MulticlassClassification.Trainers.ImageClassification(
                labelColumnName: "Label",
                featureColumnName: "Features"));

        var model = pipeline.Fit(split.TrainSet);

        // Evaluar modelo
        var predictions = model.Transform(split.TestSet);
        var metrics = _mlContext.MulticlassClassification.Evaluate(predictions);
        
        _logger.LogInformation("Image model trained with accuracy: {Accuracy}", metrics.MacroAccuracy);

        // Guardar modelo
        _mlContext.Model.Save(model, data.Schema, modelPath);

        return model;
    }
}
```

### **2. Detección de Objetos y Análisis Visual**

#### **Sistema de Detección de Objetos**
```csharp
public class ObjectDetectionService : IObjectDetectionService
{
    private readonly MLContext _mlContext;
    private readonly ITransformer _model;
    private readonly ILogger<ObjectDetectionService> _logger;

    public ObjectDetectionService(ILogger<ObjectDetectionService> logger)
    {
        _mlContext = new MLContext(seed: 1);
        _logger = logger;
        _model = LoadObjectDetectionModel();
    }

    public async Task<IEnumerable<DetectedObject>> DetectObjectsAsync(Stream imageStream)
    {
        try
        {
            // Guardar imagen temporalmente
            var tempPath = Path.GetTempFileName() + ".jpg";
            using (var fileStream = new FileStream(tempPath, FileMode.Create))
            {
                await imageStream.CopyToAsync(fileStream);
            }

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<ObjectDetectionData, ObjectDetectionPrediction>(_model);
            
            var input = new ObjectDetectionData
            {
                ImagePath = tempPath
            };

            var prediction = predictionEngine.Predict(input);
            
            // Limpiar archivo temporal
            File.Delete(tempPath);

            return ParseDetectionResults(prediction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting objects in image");
            throw;
        }
    }

    private ITransformer LoadObjectDetectionModel()
    {
        // Cargar modelo pre-entrenado para detección de objetos
        // En este ejemplo, usamos un modelo simplificado
        var modelPath = "object_detection_model.zip";
        
        if (File.Exists(modelPath))
        {
            try
            {
                return _mlContext.Model.Load(modelPath, out var modelSchema);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load object detection model");
            }
        }

        // Crear modelo básico para demostración
        return CreateBasicObjectDetectionModel();
    }

    private ITransformer CreateBasicObjectDetectionModel()
    {
        // Crear un modelo básico para detección de objetos
        // En producción, usar un modelo pre-entrenado como YOLO o SSD
        
        var data = _mlContext.Data.LoadFromEnumerable(new List<ObjectDetectionData>());
        
        var pipeline = _mlContext.Transforms.LoadRawImageBytes(
                outputColumnName: "Image",
                imageFolder: "images",
                inputColumnName: "ImagePath")
            .Append(_mlContext.Transforms.ResizeImages(
                outputColumnName: "Image",
                imageWidth: 416,
                imageHeight: 416,
                inputColumnName: "Image"))
            .Append(_mlContext.Transforms.ExtractPixels(
                outputColumnName: "Features",
                inputColumnName: "Image"));

        return pipeline.Fit(data);
    }

    private IEnumerable<DetectedObject> ParseDetectionResults(ObjectDetectionPrediction prediction)
    {
        // Parsear resultados de detección
        // En un modelo real, esto dependería del formato de salida del modelo
        
        var detectedObjects = new List<DetectedObject>();
        
        // Simular detecciones para demostración
        detectedObjects.Add(new DetectedObject
        {
            ClassName = "person",
            Confidence = 0.95f,
            BoundingBox = new BoundingBox
            {
                X = 100,
                Y = 150,
                Width = 200,
                Height = 300
            }
        });

        return detectedObjects;
    }
}

// Estructuras de datos para detección de objetos
public class ObjectDetectionData
{
    public string ImagePath { get; set; }
}

public class ObjectDetectionPrediction
{
    public float[] Scores { get; set; }
    public string[] Labels { get; set; }
    public float[] BoundingBoxes { get; set; }
}

public class DetectedObject
{
    public string ClassName { get; set; }
    public float Confidence { get; set; }
    public BoundingBox BoundingBox { get; set; }
}

public class BoundingBox
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
}
```

### **3. Análisis de Imágenes para MussikOn**

#### **Sistema de Análisis de Fotos de Músicos**
```csharp
public class MusicianImageAnalysisService : IMusicianImageAnalysisService
{
    private readonly IImageRecognitionService _imageRecognitionService;
    private readonly IObjectDetectionService _objectDetectionService;
    private readonly ILogger<MusicianImageAnalysisService> _logger;

    public MusicianImageAnalysisService(
        IImageRecognitionService imageRecognitionService,
        IObjectDetectionService objectDetectionService,
        ILogger<MusicianImageAnalysisService> logger)
    {
        _imageRecognitionService = imageRecognitionService;
        _objectDetectionService = objectDetectionService;
        _logger = logger;
    }

    public async Task<MusicianImageAnalysisResult> AnalyzeMusicianImageAsync(
        Stream imageStream, 
        int musicianId)
    {
        try
        {
            // Análisis de reconocimiento de imagen
            var recognitionResult = await _imageRecognitionService.RecognizeImageAsync(imageStream);
            
            // Detección de objetos
            imageStream.Position = 0; // Resetear stream
            var detectedObjects = await _objectDetectionService.DetectObjectsAsync(imageStream);
            
            // Análisis específico para músicos
            var musicianAnalysis = await AnalyzeMusicianSpecificFeaturesAsync(imageStream, detectedObjects);
            
            return new MusicianImageAnalysisResult
            {
                MusicianId = musicianId,
                RecognitionResult = recognitionResult,
                DetectedObjects = detectedObjects,
                MusicianAnalysis = musicianAnalysis,
                AnalysisTimestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing musician image for musician {MusicianId}", musicianId);
            throw;
        }
    }

    private async Task<MusicianSpecificAnalysis> AnalyzeMusicianSpecificFeaturesAsync(
        Stream imageStream, 
        IEnumerable<DetectedObject> detectedObjects)
    {
        try
        {
            var analysis = new MusicianSpecificAnalysis();
            
            // Detectar instrumentos musicales
            var instruments = detectedObjects.Where(obj => 
                IsMusicalInstrument(obj.ClassName)).ToList();
            analysis.DetectedInstruments = instruments;
            
            // Detectar equipamiento musical
            var equipment = detectedObjects.Where(obj => 
                IsMusicalEquipment(obj.ClassName)).ToList();
            analysis.DetectedEquipment = equipment;
            
            // Analizar ambiente (estudio, escenario, etc.)
            analysis.Environment = AnalyzeEnvironment(detectedObjects);
            
            // Calcular score de profesionalismo
            analysis.ProfessionalismScore = CalculateProfessionalismScore(instruments, equipment, analysis.Environment);
            
            // Generar recomendaciones
            analysis.Recommendations = GenerateImageRecommendations(analysis);
            
            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing musician-specific features");
            throw;
        }
    }

    private bool IsMusicalInstrument(string className)
    {
        var instruments = new[] { "guitar", "piano", "drums", "violin", "saxophone", "trumpet", "bass", "keyboard" };
        return instruments.Contains(className.ToLower());
    }

    private bool IsMusicalEquipment(string className)
    {
        var equipment = new[] { "microphone", "amplifier", "speaker", "mixer", "headphones", "cable" };
        return equipment.Contains(className.ToLower());
    }

    private string AnalyzeEnvironment(IEnumerable<DetectedObject> detectedObjects)
    {
        var environmentObjects = detectedObjects.Select(obj => obj.ClassName.ToLower()).ToList();
        
        if (environmentObjects.Contains("stage") || environmentObjects.Contains("spotlight"))
        {
            return "Stage/Performance";
        }
        else if (environmentObjects.Contains("studio") || environmentObjects.Contains("microphone"))
        {
            return "Recording Studio";
        }
        else if (environmentObjects.Contains("home") || environmentObjects.Contains("living_room"))
        {
            return "Home/Private";
        }
        else
        {
            return "Unknown";
        }
    }

    private float CalculateProfessionalismScore(
        IEnumerable<DetectedObject> instruments,
        IEnumerable<DetectedObject> equipment,
        string environment)
    {
        float score = 0.0f;
        
        // Puntos por instrumentos detectados
        score += instruments.Count() * 10;
        
        // Puntos por equipamiento profesional
        score += equipment.Count() * 5;
        
        // Puntos por ambiente profesional
        switch (environment)
        {
            case "Stage/Performance":
                score += 20;
                break;
            case "Recording Studio":
                score += 25;
                break;
            case "Home/Private":
                score += 5;
                break;
            default:
                score += 0;
                break;
        }
        
        // Normalizar score (0-100)
        return Math.Min(100, Math.Max(0, score));
    }

    private IEnumerable<string> GenerateImageRecommendations(MusicianSpecificAnalysis analysis)
    {
        var recommendations = new List<string>();
        
        if (analysis.ProfessionalismScore < 30)
        {
            recommendations.Add("Consider adding more professional equipment to your photos");
            recommendations.Add("Try taking photos in a studio or performance setting");
        }
        
        if (!analysis.DetectedInstruments.Any())
        {
            recommendations.Add("Include your musical instruments in your photos");
        }
        
        if (analysis.Environment == "Unknown")
        {
            recommendations.Add("Consider taking photos in a more recognizable musical setting");
        }
        
        if (analysis.ProfessionalismScore > 80)
        {
            recommendations.Add("Excellent professional presentation!");
        }
        
        return recommendations;
    }
}
```

### **4. Sistema de Moderación de Imágenes**

#### **Moderación Automática de Contenido**
```csharp
public class ImageModerationService : IImageModerationService
{
    private readonly IImageRecognitionService _imageRecognitionService;
    private readonly IObjectDetectionService _objectDetectionService;
    private readonly ILogger<ImageModerationService> _logger;

    public ImageModerationService(
        IImageRecognitionService imageRecognitionService,
        IObjectDetectionService objectDetectionService,
        ILogger<ImageModerationService> logger)
    {
        _imageRecognitionService = imageRecognitionService;
        _objectDetectionService = objectDetectionService;
        _logger = logger;
    }

    public async Task<ImageModerationResult> ModerateImageAsync(Stream imageStream)
    {
        try
        {
            // Análisis de contenido
            var recognitionResult = await _imageRecognitionService.RecognizeImageAsync(imageStream);
            
            // Detección de objetos
            imageStream.Position = 0;
            var detectedObjects = await _objectDetectionService.DetectObjectsAsync(imageStream);
            
            // Evaluar contenido inapropiado
            var inappropriateContent = EvaluateInappropriateContent(recognitionResult, detectedObjects);
            
            // Evaluar violencia
            var violenceContent = EvaluateViolenceContent(recognitionResult, detectedObjects);
            
            // Evaluar contenido sexual
            var sexualContent = EvaluateSexualContent(recognitionResult, detectedObjects);
            
            // Calcular score de moderación
            var moderationScore = CalculateModerationScore(inappropriateContent, violenceContent, sexualContent);
            
            // Determinar acción
            var action = DetermineModerationAction(moderationScore);
            
            return new ImageModerationResult
            {
                ModerationScore = moderationScore,
                Action = action,
                InappropriateContent = inappropriateContent,
                ViolenceContent = violenceContent,
                SexualContent = sexualContent,
                DetectedObjects = detectedObjects,
                AnalysisTimestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moderating image");
            throw;
        }
    }

    private bool EvaluateInappropriateContent(ImageRecognitionResult recognition, IEnumerable<DetectedObject> objects)
    {
        var inappropriateLabels = new[] { "drug", "alcohol", "weapon", "inappropriate" };
        
        // Verificar labels de reconocimiento
        if (inappropriateLabels.Any(label => recognition.PredictedLabel.ToLower().Contains(label)))
        {
            return true;
        }
        
        // Verificar objetos detectados
        if (objects.Any(obj => inappropriateLabels.Any(label => obj.ClassName.ToLower().Contains(label))))
        {
            return true;
        }
        
        return false;
    }

    private bool EvaluateViolenceContent(ImageRecognitionResult recognition, IEnumerable<DetectedObject> objects)
    {
        var violenceLabels = new[] { "violence", "fight", "weapon", "blood", "injury" };
        
        if (violenceLabels.Any(label => recognition.PredictedLabel.ToLower().Contains(label)))
        {
            return true;
        }
        
        if (objects.Any(obj => violenceLabels.Any(label => obj.ClassName.ToLower().Contains(label))))
        {
            return true;
        }
        
        return false;
    }

    private bool EvaluateSexualContent(ImageRecognitionResult recognition, IEnumerable<DetectedObject> objects)
    {
        var sexualLabels = new[] { "nude", "sexual", "explicit", "adult" };
        
        if (sexualLabels.Any(label => recognition.PredictedLabel.ToLower().Contains(label)))
        {
            return true;
        }
        
        if (objects.Any(obj => sexualLabels.Any(label => obj.ClassName.ToLower().Contains(label))))
        {
            return true;
        }
        
        return false;
    }

    private float CalculateModerationScore(bool inappropriate, bool violence, bool sexual)
    {
        float score = 0.0f;
        
        if (inappropriate) score += 30;
        if (violence) score += 40;
        if (sexual) score += 50;
        
        return Math.Min(100, score);
    }

    private ModerationAction DetermineModerationAction(float score)
    {
        if (score >= 80)
        {
            return ModerationAction.Reject;
        }
        else if (score >= 50)
        {
            return ModerationAction.FlagForReview;
        }
        else if (score >= 20)
        {
            return ModerationAction.Warn;
        }
        else
        {
            return ModerationAction.Approve;
        }
    }
}

public enum ModerationAction
{
    Approve,
    Warn,
    FlagForReview,
    Reject
}

public class ImageModerationResult
{
    public float ModerationScore { get; set; }
    public ModerationAction Action { get; set; }
    public bool InappropriateContent { get; set; }
    public bool ViolenceContent { get; set; }
    public bool SexualContent { get; set; }
    public IEnumerable<DetectedObject> DetectedObjects { get; set; }
    public DateTime AnalysisTimestamp { get; set; }
}
```

## 🛠️ **Ejercicios Prácticos**

### **Ejercicio 1: Clasificación de Imágenes Básica**
```csharp
// Implementar clasificación de imágenes simple
public class BasicImageClassifier
{
    private readonly MLContext _mlContext;
    private ITransformer _model;

    public void TrainModel(IEnumerable<ImageData> trainingData)
    {
        _mlContext = new MLContext(seed: 1);
        
        var data = _mlContext.Data.LoadFromEnumerable(trainingData);
        var split = _mlContext.Data.TrainTestSplit(data, testFraction: 0.2);
        
        var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label")
            .Append(_mlContext.Transforms.LoadRawImageBytes(
                outputColumnName: "Image",
                imageFolder: "images",
                inputColumnName: "ImagePath"))
            .Append(_mlContext.Transforms.ResizeImages(
                outputColumnName: "Image",
                imageWidth: 224,
                imageHeight: 224,
                inputColumnName: "Image"))
            .Append(_mlContext.Transforms.ExtractPixels(
                outputColumnName: "Features",
                inputColumnName: "Image"))
            .Append(_mlContext.MulticlassClassification.Trainers.ImageClassification(
                labelColumnName: "Label",
                featureColumnName: "Features"));

        _model = pipeline.Fit(split.TrainSet);
        
        // Evaluar modelo
        var predictions = _model.Transform(split.TestSet);
        var metrics = _mlContext.MulticlassClassification.Evaluate(predictions);
        
        Console.WriteLine($"Accuracy: {metrics.MacroAccuracy}");
    }

    public ImageRecognitionResult Classify(string imagePath)
    {
        var predictionEngine = _mlContext.Model.CreatePredictionEngine<ImageData, ImagePrediction>(_model);
        
        var input = new ImageData { ImagePath = imagePath, Label = "" };
        var prediction = predictionEngine.Predict(input);
        
        return new ImageRecognitionResult
        {
            PredictedLabel = prediction.PredictedLabel,
            Confidence = prediction.Score.Max(),
            AllScores = prediction.Score
        };
    }
}
```

### **Ejercicio 2: API de Computer Vision**
```csharp
// Crear API REST para computer vision
[ApiController]
[Route("api/[controller]")]
public class ComputerVisionController : ControllerBase
{
    private readonly IImageRecognitionService _imageRecognitionService;
    private readonly IObjectDetectionService _objectDetectionService;
    private readonly IMusicianImageAnalysisService _musicianAnalysisService;
    private readonly IImageModerationService _moderationService;
    private readonly ILogger<ComputerVisionController> _logger;

    public ComputerVisionController(
        IImageRecognitionService imageRecognitionService,
        IObjectDetectionService objectDetectionService,
        IMusicianImageAnalysisService musicianAnalysisService,
        IImageModerationService moderationService,
        ILogger<ComputerVisionController> logger)
    {
        _imageRecognitionService = imageRecognitionService;
        _objectDetectionService = objectDetectionService;
        _musicianAnalysisService = musicianAnalysisService;
        _moderationService = moderationService;
        _logger = logger;
    }

    [HttpPost("recognize")]
    public async Task<ActionResult<ImageRecognitionResult>> RecognizeImage(IFormFile image)
    {
        try
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("No image provided");
            }

            using var stream = image.OpenReadStream();
            var result = await _imageRecognitionService.RecognizeImageAsync(stream);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recognizing image");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("detect-objects")]
    public async Task<ActionResult<IEnumerable<DetectedObject>>> DetectObjects(IFormFile image)
    {
        try
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("No image provided");
            }

            using var stream = image.OpenReadStream();
            var result = await _objectDetectionService.DetectObjectsAsync(stream);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting objects");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("musician/{musicianId}/analyze")]
    public async Task<ActionResult<MusicianImageAnalysisResult>> AnalyzeMusicianImage(
        int musicianId, 
        IFormFile image)
    {
        try
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("No image provided");
            }

            using var stream = image.OpenReadStream();
            var result = await _musicianAnalysisService.AnalyzeMusicianImageAsync(stream, musicianId);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing musician image");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("moderate")]
    public async Task<ActionResult<ImageModerationResult>> ModerateImage(IFormFile image)
    {
        try
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("No image provided");
            }

            using var stream = image.OpenReadStream();
            var result = await _moderationService.ModerateImageAsync(stream);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moderating image");
            return StatusCode(500, "Internal server error");
        }
    }
}
```

### **Ejercicio 3: Sistema de Análisis de Imágenes en Lote**
```csharp
// Sistema para procesar múltiples imágenes
public class BatchImageAnalysisService : IBatchImageAnalysisService
{
    private readonly IImageRecognitionService _imageRecognitionService;
    private readonly IObjectDetectionService _objectDetectionService;
    private readonly ILogger<BatchImageAnalysisService> _logger;

    public BatchImageAnalysisService(
        IImageRecognitionService imageRecognitionService,
        IObjectDetectionService objectDetectionService,
        ILogger<BatchImageAnalysisService> logger)
    {
        _imageRecognitionService = imageRecognitionService;
        _objectDetectionService = objectDetectionService;
        _logger = logger;
    }

    public async Task<IEnumerable<BatchImageAnalysisResult>> AnalyzeBatchAsync(
        IEnumerable<ImageAnalysisRequest> requests)
    {
        try
        {
            var results = new List<BatchImageAnalysisResult>();
            var semaphore = new SemaphoreSlim(5); // Limitar concurrencia
            
            var tasks = requests.Select(async request =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await ProcessImageRequestAsync(request);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var batchResults = await Task.WhenAll(tasks);
            results.AddRange(batchResults);

            _logger.LogInformation("Completed batch analysis of {Count} images", results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in batch image analysis");
            throw;
        }
    }

    private async Task<BatchImageAnalysisResult> ProcessImageRequestAsync(ImageAnalysisRequest request)
    {
        try
        {
            using var stream = new MemoryStream(request.ImageData);
            
            var recognitionTask = _imageRecognitionService.RecognizeImageAsync(stream);
            stream.Position = 0;
            var detectionTask = _objectDetectionService.DetectObjectsAsync(stream);
            
            await Task.WhenAll(recognitionTask, detectionTask);

            return new BatchImageAnalysisResult
            {
                RequestId = request.RequestId,
                RecognitionResult = await recognitionTask,
                DetectedObjects = await detectionTask,
                ProcessingTime = DateTime.UtcNow - request.Timestamp,
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image request {RequestId}", request.RequestId);
            
            return new BatchImageAnalysisResult
            {
                RequestId = request.RequestId,
                Success = false,
                Error = ex.Message,
                ProcessingTime = DateTime.UtcNow - request.Timestamp
            };
        }
    }
}

public class ImageAnalysisRequest
{
    public string RequestId { get; set; }
    public byte[] ImageData { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
}

public class BatchImageAnalysisResult
{
    public string RequestId { get; set; }
    public ImageRecognitionResult RecognitionResult { get; set; }
    public IEnumerable<DetectedObject> DetectedObjects { get; set; }
    public bool Success { get; set; }
    public string Error { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}
```

## 📋 **Resumen de la Clase**

### **Conceptos Clave Aprendidos:**
1. **Reconocimiento de Imágenes**: Clasificación automática de imágenes
2. **Detección de Objetos**: Identificación y localización de objetos
3. **Análisis Visual Específico**: Análisis personalizado para dominios específicos
4. **Moderación de Contenido**: Filtrado automático de contenido inapropiado
5. **Procesamiento en Lote**: Análisis eficiente de múltiples imágenes

### **Habilidades Desarrolladas:**
- Implementación de computer vision con ML.NET
- Análisis de imágenes para aplicaciones específicas
- Sistemas de moderación automática
- APIs REST para computer vision
- Procesamiento eficiente de imágenes

### **Próxima Clase:**
**Clase 5: Natural Language Processing y Chatbots**
- Procesamiento de lenguaje natural avanzado
- Implementación de chatbots inteligentes
- Análisis de conversaciones
- Integración con servicios de IA

---

## 🔗 **Enlaces Útiles**
- [ML.NET Image Classification](https://docs.microsoft.com/en-us/dotnet/machine-learning/tutorials/image-classification-api-transfer-learning)
- [Computer Vision with ML.NET](https://docs.microsoft.com/en-us/dotnet/machine-learning/how-to-guides/train-image-classification-model)
- [Object Detection](https://docs.microsoft.com/en-us/azure/cognitive-services/computer-vision/concept-object-detection)
- [Image Classification](https://en.wikipedia.org/wiki/Image_classification)
- [Computer Vision](https://en.wikipedia.org/wiki/Computer_vision)
