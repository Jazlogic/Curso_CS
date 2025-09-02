# ⚡ **Clase 4: Serverless Architecture con Azure Functions**

## 🎯 **Objetivo de la Clase**
Dominar la arquitectura serverless con Azure Functions, implementando patrones avanzados, escalabilidad automática y optimización de costos.

## 📚 **Contenido Teórico**

### **1. Fundamentos de Serverless**

#### **¿Qué es Serverless?**
**Serverless** es un modelo de ejecución donde el proveedor de la nube maneja automáticamente la infraestructura, permitiendo que los desarrolladores se enfoquen en el código.

#### **Ventajas del Serverless:**
- **Escalabilidad automática**
- **Pago por uso**
- **Sin gestión de servidores**
- **Despliegue rápido**
- **Alta disponibilidad**

### **2. Azure Functions Avanzadas**

#### **Function con Durable Functions**
```csharp
// Functions/MusicianMatchingOrchestrator.cs
public class MusicianMatchingOrchestrator
{
    private readonly ILogger<MusicianMatchingOrchestrator> _logger;

    public MusicianMatchingOrchestrator(ILogger<MusicianMatchingOrchestrator> logger)
    {
        _logger = logger;
    }

    [FunctionName("MusicianMatchingOrchestrator")]
    public async Task<MusicianMatchResult> RunOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        var eventRequest = context.GetInput<EventRequest>();
        
        // Paso 1: Buscar músicos disponibles
        var availableMusicians = await context.CallActivityAsync<List<Musician>>(
            "FindAvailableMusicians", eventRequest);

        // Paso 2: Filtrar por género musical
        var filteredMusicians = await context.CallActivityAsync<List<Musician>>(
            "FilterMusiciansByGenre", new { Musicians = availableMusicians, Genre = eventRequest.Genre });

        // Paso 3: Calcular scores de matching
        var musicianScores = await context.CallActivityAsync<List<MusicianScore>>(
            "CalculateMusicianScores", new { Musicians = filteredMusicians, Event = eventRequest });

        // Paso 4: Seleccionar mejores matches
        var topMatches = await context.CallActivityAsync<List<MusicianMatch>>(
            "SelectTopMatches", musicianScores);

        // Paso 5: Enviar notificaciones
        await context.CallActivityAsync("SendMatchNotifications", topMatches);

        return new MusicianMatchResult
        {
            EventId = eventRequest.Id,
            Matches = topMatches,
            ProcessedAt = context.CurrentUtcDateTime
        };
    }

    [FunctionName("FindAvailableMusicians")]
    public async Task<List<Musician>> FindAvailableMusicians(
        [ActivityTrigger] EventRequest eventRequest,
        [CosmosDB(
            databaseName: "MussikOn",
            collectionName: "Musicians",
            ConnectionStringSetting = "CosmosDBConnection")] 
        CosmosClient cosmosClient)
    {
        var container = cosmosClient.GetContainer("MussikOn", "Musicians");
        
        var query = new QueryDefinition("SELECT * FROM c WHERE c.IsAvailable = true AND c.Location = @location")
            .WithParameter("@location", eventRequest.Location);

        var musicians = new List<Musician>();
        var iterator = container.GetItemQueryIterator<Musician>(query);
        
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            musicians.AddRange(response.ToList());
        }

        return musicians;
    }

    [FunctionName("FilterMusiciansByGenre")]
    public async Task<List<Musician>> FilterMusiciansByGenre(
        [ActivityTrigger] dynamic input,
        [CosmosDB(
            databaseName: "MussikOn",
            collectionName: "Musicians",
            ConnectionStringSetting = "CosmosDBConnection")] 
        CosmosClient cosmosClient)
    {
        var musicians = JsonSerializer.Deserialize<List<Musician>>(input.Musicians.ToString());
        var genre = input.Genre.ToString();

        return musicians.Where(m => m.Genres.Contains(genre)).ToList();
    }

    [FunctionName("CalculateMusicianScores")]
    public async Task<List<MusicianScore>> CalculateMusicianScores(
        [ActivityTrigger] dynamic input,
        [CosmosDB(
            databaseName: "MussikOn",
            collectionName: "MusicianScores",
            ConnectionStringSetting = "CosmosDBConnection")] 
        CosmosClient cosmosClient)
    {
        var musicians = JsonSerializer.Deserialize<List<Musician>>(input.Musicians.ToString());
        var eventRequest = JsonSerializer.Deserialize<EventRequest>(input.Event.ToString());

        var scores = new List<MusicianScore>();
        
        foreach (var musician in musicians)
        {
            var score = CalculateScore(musician, eventRequest);
            scores.Add(new MusicianScore
            {
                MusicianId = musician.Id,
                Score = score,
                Factors = GetScoreFactors(musician, eventRequest)
            });
        }

        return scores.OrderByDescending(s => s.Score).ToList();
    }

    [FunctionName("SelectTopMatches")]
    public async Task<List<MusicianMatch>> SelectTopMatches(
        [ActivityTrigger] List<MusicianScore> scores,
        [CosmosDB(
            databaseName: "MussikOn",
            collectionName: "Matches",
            ConnectionStringSetting = "CosmosDBConnection")] 
        CosmosClient cosmosClient)
    {
        var topScores = scores.Take(5).ToList();
        var matches = new List<MusicianMatch>();

        foreach (var score in topScores)
        {
            matches.Add(new MusicianMatch
            {
                Id = Guid.NewGuid().ToString(),
                MusicianId = score.MusicianId,
                Score = score.Score,
                CreatedAt = DateTime.UtcNow,
                Status = MatchStatus.Pending
            });
        }

        return matches;
    }

    [FunctionName("SendMatchNotifications")]
    public async Task SendMatchNotifications(
        [ActivityTrigger] List<MusicianMatch> matches,
        [SendGrid(ApiKey = "SendGridApiKey")] 
        IAsyncCollector<SendGridMessage> messageCollector)
    {
        foreach (var match in matches)
        {
            var message = new SendGridMessage()
            {
                From = new EmailAddress("noreply@mussikon.com", "MussikOn"),
                Subject = "New Event Match Available!",
                PlainTextContent = $"You have a new event match with score {match.Score}!"
            };
            
            message.AddTo(new EmailAddress("musician@example.com", "Musician"));
            await messageCollector.AddAsync(message);
        }
    }

    private double CalculateScore(Musician musician, EventRequest eventRequest)
    {
        double score = 0;
        
        // Factor de experiencia
        score += musician.Experience * 0.3;
        
        // Factor de rating
        score += musician.Rating * 0.4;
        
        // Factor de distancia
        var distance = CalculateDistance(musician.Location, eventRequest.Location);
        score += Math.Max(0, 100 - distance) * 0.2;
        
        // Factor de disponibilidad
        score += musician.AvailabilityScore * 0.1;
        
        return Math.Min(100, score);
    }

    private double CalculateDistance(Location location1, Location location2)
    {
        // Implementar cálculo de distancia
        return 0;
    }

    private Dictionary<string, double> GetScoreFactors(Musician musician, EventRequest eventRequest)
    {
        return new Dictionary<string, double>
        {
            { "Experience", musician.Experience * 0.3 },
            { "Rating", musician.Rating * 0.4 },
            { "Distance", Math.Max(0, 100 - CalculateDistance(musician.Location, eventRequest.Location)) * 0.2 },
            { "Availability", musician.AvailabilityScore * 0.1 }
        };
    }
}
```

### **3. Event-Driven Architecture**

#### **Function para Procesamiento de Eventos**
```csharp
// Functions/EventProcessor.cs
public class EventProcessor
{
    private readonly ILogger<EventProcessor> _logger;

    [FunctionName("ProcessEventCreated")]
    public async Task ProcessEventCreated(
        [EventHubTrigger("event-created", Connection = "EventHubConnection")] 
        EventData[] events)
    {
        foreach (var eventData in events)
        {
            try
            {
                var eventCreated = JsonSerializer.Deserialize<EventCreatedEvent>(eventData.Body.ToArray());
                
                // Procesar evento creado
                await ProcessEventCreatedAsync(eventCreated);
                
                _logger.LogInformation("Event created processed: {EventId}", eventCreated.EventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event created");
            }
        }
    }

    [FunctionName("ProcessMusicianApplication")]
    public async Task ProcessMusicianApplication(
        [ServiceBusTrigger("musician-applications", Connection = "ServiceBusConnection")] 
        MusicianApplicationMessage message,
        [CosmosDB(
            databaseName: "MussikOn",
            collectionName: "Applications",
            ConnectionStringSetting = "CosmosDBConnection")] 
        IAsyncCollector<MusicianApplication> applications)
    {
        try
        {
            var application = new MusicianApplication
            {
                Id = Guid.NewGuid().ToString(),
                MusicianId = message.MusicianId,
                EventId = message.EventId,
                Status = ApplicationStatus.Pending,
                AppliedAt = DateTime.UtcNow,
                Message = message.Message
            };

            await applications.AddAsync(application);
            
            // Enviar notificación al organizador
            await SendNotificationToOrganizerAsync(application);
            
            _logger.LogInformation("Musician application processed: {ApplicationId}", application.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing musician application");
            throw;
        }
    }

    [FunctionName("ProcessPaymentCompleted")]
    public async Task ProcessPaymentCompleted(
        [ServiceBusTrigger("payment-completed", Connection = "ServiceBusConnection")] 
        PaymentCompletedMessage message,
        [CosmosDB(
            databaseName: "MussikOn",
            collectionName: "Contracts",
            ConnectionStringSetting = "CosmosDBConnection")] 
        IAsyncCollector<Contract> contracts)
    {
        try
        {
            var contract = new Contract
            {
                Id = Guid.NewGuid().ToString(),
                MusicianId = message.MusicianId,
                EventId = message.EventId,
                Amount = message.Amount,
                Status = ContractStatus.Active,
                CreatedAt = DateTime.UtcNow,
                PaymentId = message.PaymentId
            };

            await contracts.AddAsync(contract);
            
            // Enviar confirmación
            await SendContractConfirmationAsync(contract);
            
            _logger.LogInformation("Payment completed processed: {PaymentId}", message.PaymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment completed");
            throw;
        }
    }

    private async Task ProcessEventCreatedAsync(EventCreatedEvent eventCreated)
    {
        // Lógica para procesar evento creado
        // Buscar músicos disponibles, enviar notificaciones, etc.
    }

    private async Task SendNotificationToOrganizerAsync(MusicianApplication application)
    {
        // Implementar envío de notificación
    }

    private async Task SendContractConfirmationAsync(Contract contract)
    {
        // Implementar envío de confirmación
    }
}
```

### **4. HTTP Triggers Avanzados**

#### **API con Azure Functions**
```csharp
// Functions/MusicianApi.cs
public class MusicianApi
{
    private readonly IMusicianService _musicianService;
    private readonly ILogger<MusicianApi> _logger;

    public MusicianApi(IMusicianService musicianService, ILogger<MusicianApi> logger)
    {
        _musicianService = musicianService;
        _logger = logger;
    }

    [FunctionName("GetMusician")]
    public async Task<IActionResult> GetMusician(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "musicians/{id}")] 
        HttpRequest req,
        string id)
    {
        try
        {
            var musician = await _musicianService.GetMusicianAsync(id);
            
            if (musician == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(musician);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting musician: {MusicianId}", id);
            return new StatusCodeResult(500);
        }
    }

    [FunctionName("CreateMusician")]
    public async Task<IActionResult> CreateMusician(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "musicians")] 
        HttpRequest req)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var createRequest = JsonSerializer.Deserialize<CreateMusicianRequest>(requestBody);

            var musician = await _musicianService.CreateMusicianAsync(createRequest);
            
            return new CreatedResult($"/api/musicians/{musician.Id}", musician);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating musician");
            return new StatusCodeResult(500);
        }
    }

    [FunctionName("UpdateMusician")]
    public async Task<IActionResult> UpdateMusician(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "musicians/{id}")] 
        HttpRequest req,
        string id)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updateRequest = JsonSerializer.Deserialize<UpdateMusicianRequest>(requestBody);

            var musician = await _musicianService.UpdateMusicianAsync(id, updateRequest);
            
            return new OkObjectResult(musician);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating musician: {MusicianId}", id);
            return new StatusCodeResult(500);
        }
    }

    [FunctionName("DeleteMusician")]
    public async Task<IActionResult> DeleteMusician(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "musicians/{id}")] 
        HttpRequest req,
        string id)
    {
        try
        {
            await _musicianService.DeleteMusicianAsync(id);
            
            return new NoContentResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting musician: {MusicianId}", id);
            return new StatusCodeResult(500);
        }
    }

    [FunctionName("SearchMusicians")]
    public async Task<IActionResult> SearchMusicians(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "musicians/search")] 
        HttpRequest req)
    {
        try
        {
            var genre = req.Query["genre"];
            var location = req.Query["location"];
            var minRating = req.Query["minRating"];

            var searchCriteria = new MusicianSearchCriteria
            {
                Genre = genre,
                Location = location,
                MinRating = double.TryParse(minRating, out var rating) ? rating : 0
            };

            var musicians = await _musicianService.SearchMusiciansAsync(searchCriteria);
            
            return new OkObjectResult(musicians);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching musicians");
            return new StatusCodeResult(500);
        }
    }
}
```

### **5. Timer Triggers**

#### **Function para Tareas Programadas**
```csharp
// Functions/ScheduledTasks.cs
public class ScheduledTasks
{
    private readonly ILogger<ScheduledTasks> _logger;

    [FunctionName("DailyAnalytics")]
    public async Task RunDailyAnalytics(
        [TimerTrigger("0 0 2 * * *")] TimerInfo timer,
        [CosmosDB(
            databaseName: "MussikOn",
            collectionName: "Analytics",
            ConnectionStringSetting = "CosmosDBConnection")] 
        IAsyncCollector<AnalyticsReport> reports)
    {
        try
        {
            _logger.LogInformation("Starting daily analytics generation");

            var report = new AnalyticsReport
            {
                Id = Guid.NewGuid().ToString(),
                ReportType = "Daily",
                Date = DateTime.UtcNow.Date,
                Metrics = await GenerateDailyMetricsAsync()
            };

            await reports.AddAsync(report);
            
            _logger.LogInformation("Daily analytics report generated: {ReportId}", report.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating daily analytics");
            throw;
        }
    }

    [FunctionName("WeeklyReport")]
    public async Task RunWeeklyReport(
        [TimerTrigger("0 0 9 * * MON")] TimerInfo timer,
        [CosmosDB(
            databaseName: "MussikOn",
            collectionName: "Reports",
            ConnectionStringSetting = "CosmosDBConnection")] 
        IAsyncCollector<WeeklyReport> reports)
    {
        try
        {
            _logger.LogInformation("Starting weekly report generation");

            var report = new WeeklyReport
            {
                Id = Guid.NewGuid().ToString(),
                WeekStart = DateTime.UtcNow.Date.AddDays(-7),
                WeekEnd = DateTime.UtcNow.Date,
                Metrics = await GenerateWeeklyMetricsAsync()
            };

            await reports.AddAsync(report);
            
            _logger.LogInformation("Weekly report generated: {ReportId}", report.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating weekly report");
            throw;
        }
    }

    [FunctionName("CleanupExpiredData")]
    public async Task CleanupExpiredData(
        [TimerTrigger("0 0 3 * * *")] TimerInfo timer,
        [CosmosDB(
            databaseName: "MussikOn",
            collectionName: "TempData",
            ConnectionStringSetting = "CosmosDBConnection")] 
        CosmosClient cosmosClient)
    {
        try
        {
            _logger.LogInformation("Starting cleanup of expired data");

            var container = cosmosClient.GetContainer("MussikOn", "TempData");
            var cutoffDate = DateTime.UtcNow.AddDays(-30);

            var query = new QueryDefinition("SELECT * FROM c WHERE c.CreatedAt < @cutoffDate")
                .WithParameter("@cutoffDate", cutoffDate);

            var iterator = container.GetItemQueryIterator<dynamic>(query);
            
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                
                foreach (var item in response)
                {
                    await container.DeleteItemAsync<dynamic>(item.id, new PartitionKey(item.id));
                }
            }
            
            _logger.LogInformation("Cleanup of expired data completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cleanup of expired data");
            throw;
        }
    }

    private async Task<Dictionary<string, object>> GenerateDailyMetricsAsync()
    {
        // Implementar generación de métricas diarias
        return new Dictionary<string, object>
        {
            { "TotalUsers", 1000 },
            { "TotalEvents", 50 },
            { "TotalApplications", 200 },
            { "TotalRevenue", 5000.00 }
        };
    }

    private async Task<Dictionary<string, object>> GenerateWeeklyMetricsAsync()
    {
        // Implementar generación de métricas semanales
        return new Dictionary<string, object>
        {
            { "WeeklyUsers", 7000 },
            { "WeeklyEvents", 350 },
            { "WeeklyApplications", 1400 },
            { "WeeklyRevenue", 35000.00 }
        };
    }
}
```

## 🛠️ **Ejercicio Práctico**

### **Ejercicio 1: Implementar Sistema Serverless Completo**

Crea un sistema serverless completo para MussikOn:

```csharp
// 1. Configurar Durable Functions
public class MussikOnOrchestrator
{
    [FunctionName("ProcessEventRequest")]
    public async Task<EventProcessingResult> ProcessEventRequest(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        var eventRequest = context.GetInput<EventRequest>();
        
        // Orquestar el procesamiento completo
        var musicians = await context.CallActivityAsync<List<Musician>>("FindMusicians", eventRequest);
        var matches = await context.CallActivityAsync<List<MusicianMatch>>("CreateMatches", musicians);
        var notifications = await context.CallActivityAsync<List<Notification>>("SendNotifications", matches);
        
        return new EventProcessingResult
        {
            EventId = eventRequest.Id,
            MusiciansFound = musicians.Count,
            MatchesCreated = matches.Count,
            NotificationsSent = notifications.Count
        };
    }
}

// 2. Implementar Event-Driven Functions
[FunctionName("HandleUserRegistration")]
public async Task HandleUserRegistration(
    [EventHubTrigger("user-registration", Connection = "EventHubConnection")] 
    EventData[] events)
{
    foreach (var eventData in events)
    {
        var userRegistered = JsonSerializer.Deserialize<UserRegisteredEvent>(eventData.Body.ToArray());
        
        // Procesar registro de usuario
        await ProcessUserRegistrationAsync(userRegistered);
    }
}

// 3. Implementar API Functions
[FunctionName("GetEventMatches")]
public async Task<IActionResult> GetEventMatches(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = "events/{eventId}/matches")] 
    HttpRequest req,
    string eventId)
{
    var matches = await _matchService.GetEventMatchesAsync(eventId);
    return new OkObjectResult(matches);
}
```

## 📋 **Resumen de la Clase**

### **Conceptos Clave:**
- **Serverless**: Modelo de ejecución sin servidores
- **Azure Functions**: Computación serverless
- **Durable Functions**: Orquestación de workflows
- **Event-Driven**: Arquitectura basada en eventos
- **HTTP Triggers**: APIs serverless
- **Timer Triggers**: Tareas programadas

### **Próxima Clase:**
**Event-Driven Architecture y Message Queues** - Arquitectura basada en eventos

## 🎯 **Objetivos de Aprendizaje**

Al finalizar esta clase, serás capaz de:
- ✅ Implementar Azure Functions avanzadas
- ✅ Usar Durable Functions para orquestación
- ✅ Crear APIs serverless con HTTP Triggers
- ✅ Implementar tareas programadas con Timer Triggers
- ✅ Diseñar arquitecturas event-driven
- ✅ Optimizar funciones para escalabilidad
