# ☁️ **Clase 2: Azure Services y .NET Integration**

## 🎯 **Objetivo de la Clase**
Integrar servicios de Azure con aplicaciones .NET, incluyendo App Service, Functions, Cosmos DB, y otros servicios esenciales para aplicaciones cloud native.

## 📚 **Contenido Teórico**

### **1. Azure App Service**

#### **Configuración de App Service**
```csharp
// Program.cs - Configuración para Azure App Service
var builder = WebApplication.CreateBuilder(args);

// Configuración específica para Azure
if (builder.Environment.IsProduction())
{
    builder.Configuration.AddAzureKeyVault(
        builder.Configuration["KeyVaultUrl"],
        new DefaultAzureCredential());
}

// Configuración de logging para Azure
builder.Logging.AddAzureWebAppDiagnostics();

var app = builder.Build();

// Configuración de middleware para Azure
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

app.Run();
```

#### **Deployment con Azure CLI**
```bash
# Crear App Service
az webapp create \
  --resource-group MussikOnRG \
  --plan MussikOnPlan \
  --name mussikon-api \
  --runtime "DOTNET|8.0"

# Configurar variables de entorno
az webapp config appsettings set \
  --resource-group MussikOnRG \
  --name mussikon-api \
  --settings \
    ConnectionStrings__DefaultConnection="Server=tcp:mussikon.database.windows.net,1433;Initial Catalog=MussikOn;Persist Security Info=False;User ID=mussikon;Password=YourPassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Deploy desde GitHub
az webapp deployment source config \
  --resource-group MussikOnRG \
  --name mussikon-api \
  --repo-url https://github.com/yourusername/MussikOn \
  --branch main \
  --manual-integration
```

### **2. Azure Functions**

#### **Function para Procesamiento de Eventos**
```csharp
// Functions/EventProcessingFunction.cs
public class EventProcessingFunction
{
    private readonly IEventService _eventService;
    private readonly ILogger<EventProcessingFunction> _logger;

    public EventProcessingFunction(IEventService eventService, ILogger<EventProcessingFunction> logger)
    {
        _eventService = eventService;
        _logger = logger;
    }

    [FunctionName("ProcessMusicianApplication")]
    public async Task Run(
        [ServiceBusTrigger("musician-applications", Connection = "ServiceBusConnection")] 
        MusicianApplicationMessage message,
        [CosmosDB(
            databaseName: "MussikOn",
            collectionName: "Applications",
            ConnectionStringSetting = "CosmosDBConnection")] 
        IAsyncCollector<MusicianApplication> applications)
    {
        _logger.LogInformation("Processing musician application for event {EventId}", message.EventId);

        try
        {
            var application = await _eventService.ProcessApplicationAsync(message);
            await applications.AddAsync(application);
            
            _logger.LogInformation("Successfully processed application {ApplicationId}", application.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing musician application");
            throw;
        }
    }

    [FunctionName("SendNotification")]
    public async Task SendNotification(
        [ServiceBusTrigger("notifications", Connection = "ServiceBusConnection")] 
        NotificationMessage message,
        [SendGrid(ApiKey = "SendGridApiKey")] 
        IAsyncCollector<SendGridMessage> messageCollector)
    {
        var msg = new SendGridMessage()
        {
            From = new EmailAddress("noreply@mussikon.com", "MussikOn"),
            Subject = message.Subject,
            PlainTextContent = message.Content
        };
        
        msg.AddTo(new EmailAddress(message.RecipientEmail, message.RecipientName));
        await messageCollector.AddAsync(msg);
    }
}
```

#### **Function para Análisis de Datos**
```csharp
// Functions/AnalyticsFunction.cs
public class AnalyticsFunction
{
    private readonly IAnalyticsService _analyticsService;

    [FunctionName("DailyAnalytics")]
    public async Task RunDailyAnalytics(
        [TimerTrigger("0 0 2 * * *")] TimerInfo timer,
        [CosmosDB(
            databaseName: "MussikOn",
            collectionName: "Analytics",
            ConnectionStringSetting = "CosmosDBConnection")] 
        IAsyncCollector<AnalyticsReport> reports)
    {
        var report = await _analyticsService.GenerateDailyReportAsync();
        await reports.AddAsync(report);
    }

    [FunctionName("ProcessUserActivity")]
    public async Task ProcessUserActivity(
        [EventHubTrigger("user-activity", Connection = "EventHubConnection")] 
        EventData[] events)
    {
        foreach (var eventData in events)
        {
            var activity = JsonSerializer.Deserialize<UserActivity>(eventData.Body.ToArray());
            await _analyticsService.ProcessUserActivityAsync(activity);
        }
    }
}
```

### **3. Azure Cosmos DB**

#### **Configuración de Cosmos DB**
```csharp
// Services/CosmosDbService.cs
public class CosmosDbService
{
    private readonly CosmosClient _cosmosClient;
    private readonly Database _database;

    public CosmosDbService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CosmosDB");
        _cosmosClient = new CosmosClient(connectionString);
        _database = _cosmosClient.GetDatabase("MussikOn");
    }

    public async Task<T> CreateItemAsync<T>(T item, string containerName) where T : class
    {
        var container = _database.GetContainer(containerName);
        var response = await container.CreateItemAsync(item);
        return response.Resource;
    }

    public async Task<T> GetItemAsync<T>(string id, string partitionKey, string containerName) where T : class
    {
        var container = _database.GetContainer(containerName);
        var response = await container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));
        return response.Resource;
    }

    public async Task<IEnumerable<T>> QueryItemsAsync<T>(string query, string containerName) where T : class
    {
        var container = _database.GetContainer(containerName);
        var queryDefinition = new QueryDefinition(query);
        var iterator = container.GetItemQueryIterator<T>(queryDefinition);
        
        var results = new List<T>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response.ToList());
        }
        
        return results;
    }
}
```

#### **Modelos para Cosmos DB**
```csharp
// Models/UserActivity.cs
public class UserActivity
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonProperty("userId")]
    public string UserId { get; set; }
    
    [JsonProperty("activityType")]
    public string ActivityType { get; set; }
    
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }
    
    [JsonProperty("data")]
    public Dictionary<string, object> Data { get; set; } = new();
    
    [JsonProperty("_etag")]
    public string ETag { get; set; }
}

// Models/AnalyticsReport.cs
public class AnalyticsReport
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonProperty("reportType")]
    public string ReportType { get; set; }
    
    [JsonProperty("date")]
    public DateTime Date { get; set; }
    
    [JsonProperty("metrics")]
    public Dictionary<string, object> Metrics { get; set; } = new();
    
    [JsonProperty("_etag")]
    public string ETag { get; set; }
}
```

### **4. Azure Service Bus**

#### **Configuración de Service Bus**
```csharp
// Services/ServiceBusService.cs
public class ServiceBusService
{
    private readonly ServiceBusClient _client;
    private readonly ILogger<ServiceBusService> _logger;

    public ServiceBusService(IConfiguration configuration, ILogger<ServiceBusService> logger)
    {
        var connectionString = configuration.GetConnectionString("ServiceBus");
        _client = new ServiceBusClient(connectionString);
        _logger = logger;
    }

    public async Task SendMessageAsync<T>(T message, string queueName)
    {
        var sender = _client.CreateSender(queueName);
        var messageBody = JsonSerializer.Serialize(message);
        var serviceBusMessage = new ServiceBusMessage(messageBody)
        {
            ContentType = "application/json"
        };
        
        await sender.SendMessageAsync(serviceBusMessage);
        await sender.CloseAsync();
        
        _logger.LogInformation("Message sent to queue {QueueName}", queueName);
    }

    public async Task ProcessMessagesAsync<T>(string queueName, Func<T, Task> messageHandler)
    {
        var processor = _client.CreateProcessor(queueName, new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 1,
            AutoCompleteMessages = false
        });

        processor.ProcessMessageAsync += async args =>
        {
            try
            {
                var messageBody = args.Message.Body.ToString();
                var message = JsonSerializer.Deserialize<T>(messageBody);
                await messageHandler(message);
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                await args.AbandonMessageAsync(args.Message);
            }
        };

        processor.ProcessErrorAsync += args =>
        {
            _logger.LogError(args.Exception, "Error in message processor");
            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync();
    }
}
```

### **5. Azure Key Vault**

#### **Integración con Key Vault**
```csharp
// Services/KeyVaultService.cs
public class KeyVaultService
{
    private readonly SecretClient _secretClient;
    private readonly ILogger<KeyVaultService> _logger;

    public KeyVaultService(IConfiguration configuration, ILogger<KeyVaultService> logger)
    {
        var keyVaultUrl = configuration["KeyVaultUrl"];
        var credential = new DefaultAzureCredential();
        _secretClient = new SecretClient(new Uri(keyVaultUrl), credential);
        _logger = logger;
    }

    public async Task<string> GetSecretAsync(string secretName)
    {
        try
        {
            var secret = await _secretClient.GetSecretAsync(secretName);
            return secret.Value.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving secret {SecretName}", secretName);
            throw;
        }
    }

    public async Task SetSecretAsync(string secretName, string secretValue)
    {
        try
        {
            await _secretClient.SetSecretAsync(secretName, secretValue);
            _logger.LogInformation("Secret {SecretName} set successfully", secretName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting secret {SecretName}", secretName);
            throw;
        }
    }
}
```

### **6. Azure Application Insights**

#### **Configuración de Application Insights**
```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsights:ConnectionString"]);

// Configuración personalizada de telemetría
builder.Services.Configure<TelemetryConfiguration>(telemetryConfiguration =>
{
    telemetryConfiguration.TelemetryProcessorChainBuilder
        .Use((next) => new CustomTelemetryProcessor(next))
        .Build();
});

// CustomTelemetryProcessor.cs
public class CustomTelemetryProcessor : ITelemetryProcessor
{
    private readonly ITelemetryProcessor _next;

    public CustomTelemetryProcessor(ITelemetryProcessor next)
    {
        _next = next;
    }

    public void Process(ITelemetry item)
    {
        // Filtrar telemetría sensible
        if (item is RequestTelemetry request)
        {
            if (request.Url.Contains("/health"))
            {
                return; // No enviar health checks
            }
        }

        _next.Process(item);
    }
}
```

## 🛠️ **Ejercicio Práctico**

### **Ejercicio 1: Integración Completa con Azure**

Crea una aplicación que integre múltiples servicios de Azure:

```csharp
// 1. Configurar Azure App Service
public class AzureAppServiceConfig
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Cosmos DB
        services.AddSingleton<CosmosDbService>();
        
        // Service Bus
        services.AddSingleton<ServiceBusService>();
        
        // Key Vault
        services.AddSingleton<KeyVaultService>();
        
        // Application Insights
        services.AddApplicationInsightsTelemetry();
    }
}

// 2. Implementar Function para procesamiento de eventos
[FunctionName("ProcessMusicianMatch")]
public async Task ProcessMusicianMatch(
    [ServiceBusTrigger("musician-matches", Connection = "ServiceBusConnection")] 
    MusicianMatchMessage message,
    [CosmosDB(
        databaseName: "MussikOn",
        collectionName: "Matches",
        ConnectionStringSetting = "CosmosDBConnection")] 
    IAsyncCollector<MusicianMatch> matches)
{
    // Procesar matching de músicos
    var match = await ProcessMatchAsync(message);
    await matches.AddAsync(match);
}

// 3. Implementar telemetría personalizada
public class MussikOnTelemetry
{
    private readonly TelemetryClient _telemetryClient;
    
    public void TrackUserAction(string userId, string action, Dictionary<string, string> properties)
    {
        _telemetryClient.TrackEvent("UserAction", properties);
        _telemetryClient.TrackDependency("UserService", action, DateTime.UtcNow, TimeSpan.Zero, true);
    }
}
```

## 📋 **Resumen de la Clase**

### **Conceptos Clave:**
- **Azure App Service**: Hosting de aplicaciones web
- **Azure Functions**: Serverless computing
- **Cosmos DB**: Base de datos NoSQL global
- **Service Bus**: Mensajería asíncrona
- **Key Vault**: Gestión de secretos
- **Application Insights**: Monitoreo y telemetría

### **Próxima Clase:**
**AWS Services y .NET Integration** - Integración con servicios de AWS

## 🎯 **Objetivos de Aprendizaje**

Al finalizar esta clase, serás capaz de:
- ✅ Configurar Azure App Service para aplicaciones .NET
- ✅ Implementar Azure Functions para procesamiento serverless
- ✅ Integrar Cosmos DB para almacenamiento NoSQL
- ✅ Usar Service Bus para mensajería asíncrona
- ✅ Gestionar secretos con Azure Key Vault
- ✅ Implementar telemetría con Application Insights
