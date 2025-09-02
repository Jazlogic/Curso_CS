# 📊 **Clase 6: Predictive Analytics y Forecasting**

## 🎯 **Objetivos de la Clase**
- Implementar análisis predictivo con ML.NET
- Crear modelos de forecasting y series temporales
- Predecir tendencias y patrones
- Analizar datos históricos para predicciones

## 📚 **Contenido Teórico**

### **1. Fundamentos de Predictive Analytics**

#### **Modelos de Predicción con ML.NET**
```csharp
// Estructura de datos para análisis predictivo
public class TimeSeriesData
{
    [LoadColumn(0)]
    public DateTime Timestamp { get; set; }

    [LoadColumn(1)]
    public float Value { get; set; }

    [LoadColumn(2)]
    public string Category { get; set; }
}

public class PredictionResult
{
    public DateTime Timestamp { get; set; }
    public float PredictedValue { get; set; }
    public float Confidence { get; set; }
    public float LowerBound { get; set; }
    public float UpperBound { get; set; }
}

// Servicio de análisis predictivo
public class PredictiveAnalyticsService : IPredictiveAnalyticsService
{
    private readonly MLContext _mlContext;
    private readonly ILogger<PredictiveAnalyticsService> _logger;
    private readonly Dictionary<string, ITransformer> _models;

    public PredictiveAnalyticsService(ILogger<PredictiveAnalyticsService> logger)
    {
        _mlContext = new MLContext(seed: 1);
        _logger = logger;
        _models = new Dictionary<string, ITransformer>();
    }

    public async Task<IEnumerable<PredictionResult>> PredictFutureValuesAsync(
        string category, 
        int periods, 
        IEnumerable<TimeSeriesData> historicalData)
    {
        try
        {
            // Cargar o entrenar modelo
            var model = await GetOrTrainModelAsync(category, historicalData);
            
            // Preparar datos para predicción
            var predictionData = PreparePredictionData(historicalData, periods);
            
            // Realizar predicciones
            var predictions = await MakePredictionsAsync(model, predictionData, periods);
            
            return predictions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting future values for category {Category}", category);
            throw;
        }
    }

    private async Task<ITransformer> GetOrTrainModelAsync(string category, IEnumerable<TimeSeriesData> data)
    {
        if (_models.ContainsKey(category))
        {
            return _models[category];
        }

        // Entrenar nuevo modelo
        var model = await TrainTimeSeriesModelAsync(category, data);
        _models[category] = model;
        
        return model;
    }

    private async Task<ITransformer> TrainTimeSeriesModelAsync(string category, IEnumerable<TimeSeriesData> data)
    {
        try
        {
            var mlData = _mlContext.Data.LoadFromEnumerable(data);
            
            // Configurar pipeline para series temporales
            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Category")
                .Append(_mlContext.Transforms.Concatenate("Features", "Timestamp", "Value"))
                .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(_mlContext.Regression.Trainers.Sdca(
                    labelColumnName: "Value",
                    featureColumnName: "Features"));

            var model = pipeline.Fit(mlData);
            
            _logger.LogInformation("Trained time series model for category {Category}", category);
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error training time series model for category {Category}", category);
            throw;
        }
    }

    private IEnumerable<TimeSeriesData> PreparePredictionData(
        IEnumerable<TimeSeriesData> historicalData, 
        int periods)
    {
        var lastTimestamp = historicalData.Max(d => d.Timestamp);
        var predictionData = new List<TimeSeriesData>();
        
        for (int i = 1; i <= periods; i++)
        {
            predictionData.Add(new TimeSeriesData
            {
                Timestamp = lastTimestamp.AddDays(i),
                Value = 0, // Valor dummy para predicción
                Category = historicalData.First().Category
            });
        }
        
        return predictionData;
    }

    private async Task<IEnumerable<PredictionResult>> MakePredictionsAsync(
        ITransformer model, 
        IEnumerable<TimeSeriesData> predictionData, 
        int periods)
    {
        var results = new List<PredictionResult>();
        var predictionEngine = _mlContext.Model.CreatePredictionEngine<TimeSeriesData, TimeSeriesPrediction>(model);
        
        foreach (var data in predictionData)
        {
            var prediction = predictionEngine.Predict(data);
            
            // Calcular intervalos de confianza
            var confidence = CalculateConfidence(prediction.Score);
            var margin = confidence * 0.1f; // 10% de margen
            
            results.Add(new PredictionResult
            {
                Timestamp = data.Timestamp,
                PredictedValue = prediction.Score,
                Confidence = confidence,
                LowerBound = prediction.Score - margin,
                UpperBound = prediction.Score + margin
            });
        }
        
        return results;
    }

    private float CalculateConfidence(float prediction)
    {
        // Cálculo simplificado de confianza
        // En producción, usar métodos más sofisticados
        return Math.Min(1.0f, Math.Max(0.0f, 0.8f + (prediction * 0.1f)));
    }
}
```

### **2. Forecasting para MussikOn**

#### **Predicción de Demanda de Músicos**
```csharp
public class MusicianDemandForecastingService : IMusicianDemandForecastingService
{
    private readonly IPredictiveAnalyticsService _predictiveService;
    private readonly IEventRepository _eventRepository;
    private readonly IMusicianRepository _musicianRepository;
    private readonly ILogger<MusicianDemandForecastingService> _logger;

    public MusicianDemandForecastingService(
        IPredictiveAnalyticsService predictiveService,
        IEventRepository eventRepository,
        IMusicianRepository musicianRepository,
        ILogger<MusicianDemandForecastingService> logger)
    {
        _predictiveService = predictiveService;
        _eventRepository = eventRepository;
        _musicianRepository = musicianRepository;
        _logger = logger;
    }

    public async Task<DemandForecastResult> ForecastMusicianDemandAsync(
        string genre, 
        string location, 
        int daysAhead)
    {
        try
        {
            // Obtener datos históricos
            var historicalData = await GetHistoricalDemandDataAsync(genre, location);
            
            if (!historicalData.Any())
            {
                return new DemandForecastResult
                {
                    Genre = genre,
                    Location = location,
                    ForecastPeriod = daysAhead,
                    Predictions = new List<DemandPrediction>(),
                    Message = "Insufficient historical data for forecasting"
                };
            }

            // Realizar predicciones
            var predictions = await _predictiveService.PredictFutureValuesAsync(
                $"{genre}_{location}", 
                daysAhead, 
                historicalData);

            // Convertir a formato de demanda
            var demandPredictions = predictions.Select(p => new DemandPrediction
            {
                Date = p.Timestamp,
                PredictedDemand = Math.Max(0, p.PredictedValue),
                Confidence = p.Confidence,
                LowerBound = Math.Max(0, p.LowerBound),
                UpperBound = p.UpperBound
            }).ToList();

            // Calcular métricas adicionales
            var averageDemand = demandPredictions.Average(d => d.PredictedDemand);
            var peakDemand = demandPredictions.Max(d => d.PredictedDemand);
            var peakDate = demandPredictions.First(d => d.PredictedDemand == peakDemand).Date;

            return new DemandForecastResult
            {
                Genre = genre,
                Location = location,
                ForecastPeriod = daysAhead,
                Predictions = demandPredictions,
                AverageDemand = averageDemand,
                PeakDemand = peakDemand,
                PeakDate = peakDate,
                Confidence = demandPredictions.Average(d => d.Confidence)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error forecasting demand for genre {Genre} in location {Location}", genre, location);
            throw;
        }
    }

    private async Task<IEnumerable<TimeSeriesData>> GetHistoricalDemandDataAsync(string genre, string location)
    {
        try
        {
            // Obtener eventos históricos
            var historicalEvents = await _eventRepository.GetHistoricalEventsAsync(genre, location, 365); // Último año
            
            // Agrupar por fecha y contar demanda
            var demandData = historicalEvents
                .GroupBy(e => e.Date.Date)
                .Select(g => new TimeSeriesData
                {
                    Timestamp = g.Key,
                    Value = g.Count(), // Número de eventos por día
                    Category = $"{genre}_{location}"
                })
                .OrderBy(d => d.Timestamp)
                .ToList();

            return demandData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting historical demand data");
            throw;
        }
    }

    public async Task<IEnumerable<GenreDemandTrend>> AnalyzeGenreTrendsAsync(string location, int months = 6)
    {
        try
        {
            var genres = new[] { "jazz", "rock", "classical", "pop", "acoustic", "electronic" };
            var trends = new List<GenreDemandTrend>();

            foreach (var genre in genres)
            {
                var forecast = await ForecastMusicianDemandAsync(genre, location, 30);
                
                trends.Add(new GenreDemandTrend
                {
                    Genre = genre,
                    Location = location,
                    CurrentDemand = forecast.Predictions.Take(7).Average(p => p.PredictedDemand),
                    FutureDemand = forecast.Predictions.Skip(7).Average(p => p.PredictedDemand),
                    Trend = forecast.FutureDemand > forecast.CurrentDemand ? "Increasing" : "Decreasing",
                    GrowthRate = ((forecast.FutureDemand - forecast.CurrentDemand) / forecast.CurrentDemand) * 100
                });
            }

            return trends.OrderByDescending(t => t.GrowthRate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing genre trends");
            throw;
        }
    }
}
```

### **3. Análisis de Tendencias y Patrones**

#### **Sistema de Detección de Tendencias**
```csharp
public class TrendAnalysisService : ITrendAnalysisService
{
    private readonly ILogger<TrendAnalysisService> _logger;

    public TrendAnalysisService(ILogger<TrendAnalysisService> logger)
    {
        _logger = logger;
    }

    public async Task<TrendAnalysisResult> AnalyzeTrendsAsync(IEnumerable<TimeSeriesData> data)
    {
        try
        {
            var orderedData = data.OrderBy(d => d.Timestamp).ToList();
            
            var analysis = new TrendAnalysisResult
            {
                DataPoints = orderedData.Count,
                TimeRange = new TimeRange
                {
                    Start = orderedData.First().Timestamp,
                    End = orderedData.Last().Timestamp
                },
                TrendDirection = CalculateTrendDirection(orderedData),
                TrendStrength = CalculateTrendStrength(orderedData),
                Seasonality = DetectSeasonality(orderedData),
                Anomalies = DetectAnomalies(orderedData),
                Forecast = GenerateTrendForecast(orderedData)
            };

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing trends");
            throw;
        }
    }

    private string CalculateTrendDirection(List<TimeSeriesData> data)
    {
        if (data.Count < 2) return "Insufficient Data";

        var firstHalf = data.Take(data.Count / 2).Average(d => d.Value);
        var secondHalf = data.Skip(data.Count / 2).Average(d => d.Value);

        var change = ((secondHalf - firstHalf) / firstHalf) * 100;

        if (change > 5) return "Strongly Increasing";
        if (change > 1) return "Increasing";
        if (change < -5) return "Strongly Decreasing";
        if (change < -1) return "Decreasing";
        
        return "Stable";
    }

    private float CalculateTrendStrength(List<TimeSeriesData> data)
    {
        if (data.Count < 2) return 0;

        var values = data.Select(d => d.Value).ToArray();
        var correlation = CalculateCorrelation(values, Enumerable.Range(0, values.Length).Select(i => (float)i).ToArray());
        
        return Math.Abs(correlation);
    }

    private float CalculateCorrelation(float[] x, float[] y)
    {
        if (x.Length != y.Length) return 0;

        var n = x.Length;
        var sumX = x.Sum();
        var sumY = y.Sum();
        var sumXY = x.Zip(y, (a, b) => a * b).Sum();
        var sumX2 = x.Sum(a => a * a);
        var sumY2 = y.Sum(b => b * b);

        var numerator = n * sumXY - sumX * sumY;
        var denominator = Math.Sqrt((n * sumX2 - sumX * sumX) * (n * sumY2 - sumY * sumY));

        return denominator == 0 ? 0 : (float)(numerator / denominator);
    }

    private SeasonalityInfo DetectSeasonality(List<TimeSeriesData> data)
    {
        if (data.Count < 30) return new SeasonalityInfo { HasSeasonality = false };

        // Agrupar por día de la semana
        var weeklyPattern = data
            .GroupBy(d => d.Timestamp.DayOfWeek)
            .ToDictionary(g => g.Key, g => g.Average(d => d.Value));

        // Calcular variación semanal
        var weeklyValues = weeklyPattern.Values.ToArray();
        var weeklyVariance = CalculateVariance(weeklyValues);
        var weeklyMean = weeklyValues.Average();

        var hasWeeklySeasonality = weeklyVariance / weeklyMean > 0.1; // 10% de variación

        return new SeasonalityInfo
        {
            HasSeasonality = hasWeeklySeasonality,
            Type = hasWeeklySeasonality ? "Weekly" : "None",
            Strength = hasWeeklySeasonality ? weeklyVariance / weeklyMean : 0,
            Pattern = weeklyPattern
        };
    }

    private float CalculateVariance(float[] values)
    {
        var mean = values.Average();
        return values.Sum(v => (v - mean) * (v - mean)) / values.Length;
    }

    private IEnumerable<Anomaly> DetectAnomalies(List<TimeSeriesData> data)
    {
        var anomalies = new List<Anomaly>();
        
        if (data.Count < 10) return anomalies;

        var values = data.Select(d => d.Value).ToArray();
        var mean = values.Average();
        var stdDev = CalculateStandardDeviation(values);

        for (int i = 0; i < data.Count; i++)
        {
            var zScore = Math.Abs((values[i] - mean) / stdDev);
            
            if (zScore > 2.5) // Umbral para anomalías
            {
                anomalies.Add(new Anomaly
                {
                    Timestamp = data[i].Timestamp,
                    Value = values[i],
                    ZScore = zScore,
                    Severity = zScore > 3 ? "High" : "Medium"
                });
            }
        }

        return anomalies;
    }

    private float CalculateStandardDeviation(float[] values)
    {
        var mean = values.Average();
        var variance = values.Sum(v => (v - mean) * (v - mean)) / values.Length;
        return (float)Math.Sqrt(variance);
    }

    private TrendForecast GenerateTrendForecast(List<TimeSeriesData> data)
    {
        if (data.Count < 5) return new TrendForecast { HasForecast = false };

        // Regresión lineal simple para proyección
        var n = data.Count;
        var x = Enumerable.Range(0, n).Select(i => (float)i).ToArray();
        var y = data.Select(d => d.Value).ToArray();

        var slope = CalculateSlope(x, y);
        var intercept = CalculateIntercept(x, y, slope);

        var lastTimestamp = data.Last().Timestamp;
        var futureValues = new List<ForecastPoint>();

        for (int i = 1; i <= 7; i++) // Próxima semana
        {
            var futureX = n + i - 1;
            var predictedValue = slope * futureX + intercept;
            
            futureValues.Add(new ForecastPoint
            {
                Timestamp = lastTimestamp.AddDays(i),
                PredictedValue = Math.Max(0, predictedValue)
            });
        }

        return new TrendForecast
        {
            HasForecast = true,
            ForecastPoints = futureValues,
            TrendSlope = slope,
            Confidence = CalculateForecastConfidence(data, slope)
        };
    }

    private float CalculateSlope(float[] x, float[] y)
    {
        var n = x.Length;
        var sumX = x.Sum();
        var sumY = y.Sum();
        var sumXY = x.Zip(y, (a, b) => a * b).Sum();
        var sumX2 = x.Sum(a => a * a);

        return (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
    }

    private float CalculateIntercept(float[] x, float[] y, float slope)
    {
        return y.Average() - slope * x.Average();
    }

    private float CalculateForecastConfidence(List<TimeSeriesData> data, float slope)
    {
        // Confianza basada en la consistencia de la tendencia
        var values = data.Select(d => d.Value).ToArray();
        var mean = values.Average();
        var variance = CalculateVariance(values);
        
        // Confianza inversamente proporcional a la varianza
        return Math.Max(0.1f, Math.Min(0.9f, 1.0f - (variance / mean)));
    }
}
```

## 🛠️ **Ejercicios Prácticos**

### **Ejercicio 1: Modelo de Predicción Simple**
```csharp
// Implementar modelo de predicción básico
public class SimplePredictionModel
{
    private readonly MLContext _mlContext;
    private ITransformer _model;

    public void TrainModel(IEnumerable<TimeSeriesData> data)
    {
        _mlContext = new MLContext(seed: 1);
        
        var mlData = _mlContext.Data.LoadFromEnumerable(data);
        var split = _mlContext.Data.TrainTestSplit(mlData, testFraction: 0.2);
        
        var pipeline = _mlContext.Transforms.Concatenate("Features", "Timestamp")
            .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
            .Append(_mlContext.Regression.Trainers.Sdca(
                labelColumnName: "Value",
                featureColumnName: "Features"));

        _model = pipeline.Fit(split.TrainSet);
        
        // Evaluar modelo
        var predictions = _model.Transform(split.TestSet);
        var metrics = _mlContext.Regression.Evaluate(predictions);
        
        Console.WriteLine($"RMSE: {metrics.RootMeanSquaredError}");
        Console.WriteLine($"R²: {metrics.RSquared}");
    }

    public float Predict(DateTime timestamp)
    {
        var predictionEngine = _mlContext.Model.CreatePredictionEngine<TimeSeriesData, TimeSeriesPrediction>(_model);
        
        var input = new TimeSeriesData
        {
            Timestamp = timestamp,
            Value = 0,
            Category = ""
        };

        var prediction = predictionEngine.Predict(input);
        return prediction.Score;
    }
}
```

### **Ejercicio 2: API de Forecasting**
```csharp
// Crear API REST para forecasting
[ApiController]
[Route("api/[controller]")]
public class ForecastingController : ControllerBase
{
    private readonly IMusicianDemandForecastingService _demandForecastingService;
    private readonly ITrendAnalysisService _trendAnalysisService;
    private readonly ILogger<ForecastingController> _logger;

    public ForecastingController(
        IMusicianDemandForecastingService demandForecastingService,
        ITrendAnalysisService trendAnalysisService,
        ILogger<ForecastingController> logger)
    {
        _demandForecastingService = demandForecastingService;
        _trendAnalysisService = trendAnalysisService;
        _logger = logger;
    }

    [HttpGet("demand/{genre}/{location}")]
    public async Task<ActionResult<DemandForecastResult>> ForecastDemand(
        string genre, 
        string location, 
        [FromQuery] int days = 30)
    {
        try
        {
            var result = await _demandForecastingService.ForecastMusicianDemandAsync(genre, location, days);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error forecasting demand");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("trends/{location}")]
    public async Task<ActionResult<IEnumerable<GenreDemandTrend>>> GetGenreTrends(
        string location, 
        [FromQuery] int months = 6)
    {
        try
        {
            var trends = await _demandForecastingService.AnalyzeGenreTrendsAsync(location, months);
            return Ok(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing trends");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("analyze-trends")]
    public async Task<ActionResult<TrendAnalysisResult>> AnalyzeTrends(
        [FromBody] IEnumerable<TimeSeriesData> data)
    {
        try
        {
            var result = await _trendAnalysisService.AnalyzeTrendsAsync(data);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing trends");
            return StatusCode(500, "Internal server error");
        }
    }
}
```

## 📋 **Resumen de la Clase**

### **Conceptos Clave Aprendidos:**
1. **Predictive Analytics**: Análisis predictivo con ML.NET
2. **Time Series Forecasting**: Predicción de series temporales
3. **Trend Analysis**: Análisis de tendencias y patrones
4. **Demand Forecasting**: Predicción de demanda
5. **Anomaly Detection**: Detección de anomalías

### **Habilidades Desarrolladas:**
- Implementación de modelos predictivos
- Análisis de series temporales
- Forecasting de demanda
- Detección de tendencias
- APIs REST para analytics

### **Próxima Clase:**
**Clase 7: Model Deployment y MLOps**
- Despliegue de modelos ML
- MLOps y CI/CD para ML
- Monitoreo de modelos
- Versionado de modelos

---

## 🔗 **Enlaces Útiles**
- [ML.NET Time Series](https://docs.microsoft.com/en-us/dotnet/machine-learning/how-to-guides/train-time-series-forecasting-model)
- [Predictive Analytics](https://docs.microsoft.com/en-us/azure/machine-learning/concept-predictive-analytics)
- [Time Series Forecasting](https://docs.microsoft.com/en-us/azure/machine-learning/concept-time-series-forecasting)
- [Trend Analysis](https://en.wikipedia.org/wiki/Trend_analysis)
- [Forecasting](https://en.wikipedia.org/wiki/Forecasting)
