# ☁️ **Clase 3: AWS Services y .NET Integration**

## 🎯 **Objetivo de la Clase**
Integrar servicios de AWS con aplicaciones .NET, incluyendo Lambda, RDS, S3, y otros servicios esenciales para aplicaciones cloud native.

## 📚 **Contenido Teórico**

### **1. AWS Lambda con .NET**

#### **Configuración de Lambda Function**
```csharp
// Lambda/EventProcessingLambda.cs
public class EventProcessingLambda
{
    private readonly IEventService _eventService;
    private readonly ILogger<EventProcessingLambda> _logger;

    public EventProcessingLambda()
    {
        var serviceProvider = ConfigureServices();
        _eventService = serviceProvider.GetRequiredService<IEventService>();
        _logger = serviceProvider.GetRequiredService<ILogger<EventProcessingLambda>>();
    }

    [LambdaFunction]
    public async Task<APIGatewayProxyResponse> ProcessEvent(
        APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        _logger.LogInformation("Processing event request: {RequestId}", context.AwsRequestId);

        try
        {
            var eventData = JsonSerializer.Deserialize<EventProcessingRequest>(request.Body);
            var result = await _eventService.ProcessEventAsync(eventData);

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonSerializer.Serialize(result),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event");
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonSerializer.Serialize(new { error = ex.Message })
            };
        }
    }

    private IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        
        // Configurar servicios
        services.AddLogging();
        services.AddSingleton<IEventService, EventService>();
        
        // Configurar AWS
        services.AddAWSService<IAmazonDynamoDB>();
        services.AddAWSService<IAmazonS3>();
        
        return services.BuildServiceProvider();
    }
}
```

#### **Lambda para Procesamiento de Imágenes**
```csharp
// Lambda/ImageProcessingLambda.cs
public class ImageProcessingLambda
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<ImageProcessingLambda> _logger;

    public ImageProcessingLambda(IAmazonS3 s3Client, ILogger<ImageProcessingLambda> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
    }

    [LambdaFunction]
    public async Task<APIGatewayProxyResponse> ProcessImage(
        APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        try
        {
            var imageRequest = JsonSerializer.Deserialize<ImageProcessingRequest>(request.Body);
            
            // Descargar imagen desde S3
            var imageBytes = await DownloadImageFromS3Async(imageRequest.S3Key);
            
            // Procesar imagen
            var processedImage = await ProcessImageAsync(imageBytes, imageRequest.ProcessingOptions);
            
            // Subir imagen procesada
            var processedS3Key = await UploadProcessedImageAsync(processedImage, imageRequest.OutputKey);
            
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonSerializer.Serialize(new { processedImageKey = processedS3Key })
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image");
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonSerializer.Serialize(new { error = ex.Message })
            };
        }
    }

    private async Task<byte[]> DownloadImageFromS3Async(string s3Key)
    {
        var request = new GetObjectRequest
        {
            BucketName = "mussikon-images",
            Key = s3Key
        };

        using var response = await _s3Client.GetObjectAsync(request);
        using var memoryStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    private async Task<byte[]> ProcessImageAsync(byte[] imageBytes, ImageProcessingOptions options)
    {
        // Implementar procesamiento de imagen
        // Redimensionar, aplicar filtros, etc.
        return imageBytes;
    }

    private async Task<string> UploadProcessedImageAsync(byte[] imageBytes, string outputKey)
    {
        var request = new PutObjectRequest
        {
            BucketName = "mussikon-processed-images",
            Key = outputKey,
            InputStream = new MemoryStream(imageBytes),
            ContentType = "image/jpeg"
        };

        await _s3Client.PutObjectAsync(request);
        return outputKey;
    }
}
```

### **2. Amazon RDS con .NET**

#### **Configuración de RDS**
```csharp
// Services/RdsService.cs
public class RdsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RdsService> _logger;

    public RdsService(IConfiguration configuration, ILogger<RdsService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Musician> GetMusicianAsync(string musicianId)
    {
        var connectionString = _configuration.GetConnectionString("RDS");
        
        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        var query = "SELECT * FROM Musicians WHERE Id = @Id";
        var musician = await connection.QueryFirstOrDefaultAsync<Musician>(query, new { Id = musicianId });
        
        return musician;
    }

    public async Task<IEnumerable<Event>> GetEventsByLocationAsync(string location)
    {
        var connectionString = _configuration.GetConnectionString("RDS");
        
        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        var query = @"
            SELECT e.*, l.City, l.State, l.Country 
            FROM Events e 
            INNER JOIN Locations l ON e.LocationId = l.Id 
            WHERE l.City = @Location OR l.State = @Location OR l.Country = @Location";
            
        var events = await connection.QueryAsync<Event>(query, new { Location = location });
        
        return events;
    }

    public async Task<bool> CreateMusicianAsync(Musician musician)
    {
        var connectionString = _configuration.GetConnectionString("RDS");
        
        using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        var query = @"
            INSERT INTO Musicians (Id, Name, Email, Genre, Experience, Rating, CreatedAt) 
            VALUES (@Id, @Name, @Email, @Genre, @Experience, @Rating, @CreatedAt)";
            
        var result = await connection.ExecuteAsync(query, musician);
        
        return result > 0;
    }
}
```

#### **Configuración de Connection String**
```json
// appsettings.json
{
  "ConnectionStrings": {
    "RDS": "Server=mussikon-db.cluster-xyz.us-east-1.rds.amazonaws.com;Port=3306;Database=MussikOn;Uid=admin;Pwd=YourPassword;SslMode=Required;"
  }
}
```

### **3. Amazon S3**

#### **Servicio de S3**
```csharp
// Services/S3Service.cs
public class S3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3Service> _logger;

    public S3Service(IAmazonS3 s3Client, ILogger<S3Service> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string bucketName)
    {
        try
        {
            var key = $"uploads/{DateTime.UtcNow:yyyy/MM/dd}/{fileName}";
            
            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                InputStream = fileStream,
                ContentType = GetContentType(fileName)
            };

            await _s3Client.PutObjectAsync(request);
            
            _logger.LogInformation("File uploaded successfully: {Key}", key);
            return key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", fileName);
            throw;
        }
    }

    public async Task<Stream> DownloadFileAsync(string key, string bucketName)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            var response = await _s3Client.GetObjectAsync(request);
            return response.ResponseStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file: {Key}", key);
            throw;
        }
    }

    public async Task<string> GeneratePresignedUrlAsync(string key, string bucketName, TimeSpan expiration)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(expiration)
        };

        return await _s3Client.GetPreSignedURLAsync(request);
    }

    public async Task DeleteFileAsync(string key, string bucketName)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(request);
            _logger.LogInformation("File deleted successfully: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {Key}", key);
            throw;
        }
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".mp3" => "audio/mpeg",
            ".mp4" => "video/mp4",
            _ => "application/octet-stream"
        };
    }
}
```

### **4. Amazon DynamoDB**

#### **Configuración de DynamoDB**
```csharp
// Services/DynamoDbService.cs
public class DynamoDbService
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly ILogger<DynamoDbService> _logger;

    public DynamoDbService(IAmazonDynamoDB dynamoDbClient, ILogger<DynamoDbService> logger)
    {
        _dynamoDbClient = dynamoDbClient;
        _logger = logger;
    }

    public async Task<T> GetItemAsync<T>(string tableName, string partitionKey, string sortKey = null) where T : class
    {
        try
        {
            var request = new GetItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "PK", new AttributeValue { S = partitionKey } }
                }
            };

            if (!string.IsNullOrEmpty(sortKey))
            {
                request.Key["SK"] = new AttributeValue { S = sortKey };
            }

            var response = await _dynamoDbClient.GetItemAsync(request);
            
            if (response.Item.Count == 0)
                return null;

            return ConvertFromDynamoDb<T>(response.Item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting item from DynamoDB");
            throw;
        }
    }

    public async Task PutItemAsync<T>(string tableName, T item) where T : class
    {
        try
        {
            var request = new PutItemRequest
            {
                TableName = tableName,
                Item = ConvertToDynamoDb(item)
            };

            await _dynamoDbClient.PutItemAsync(request);
            _logger.LogInformation("Item saved to DynamoDB table: {TableName}", tableName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving item to DynamoDB");
            throw;
        }
    }

    public async Task<IEnumerable<T>> QueryItemsAsync<T>(string tableName, string partitionKey) where T : class
    {
        try
        {
            var request = new QueryRequest
            {
                TableName = tableName,
                KeyConditionExpression = "PK = :pk",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":pk", new AttributeValue { S = partitionKey } }
                }
            };

            var response = await _dynamoDbClient.QueryAsync(request);
            
            return response.Items.Select(item => ConvertFromDynamoDb<T>(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying items from DynamoDB");
            throw;
        }
    }

    private Dictionary<string, AttributeValue> ConvertToDynamoDb<T>(T item) where T : class
    {
        var json = JsonSerializer.Serialize(item);
        var document = Document.FromJson(json);
        return document.ToAttributeMap();
    }

    private T ConvertFromDynamoDb<T>(Dictionary<string, AttributeValue> item) where T : class
    {
        var document = Document.FromAttributeMap(item);
        var json = document.ToJson();
        return JsonSerializer.Deserialize<T>(json);
    }
}
```

### **5. Amazon SQS**

#### **Servicio de SQS**
```csharp
// Services/SqsService.cs
public class SqsService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly ILogger<SqsService> _logger;

    public SqsService(IAmazonSQS sqsClient, ILogger<SqsService> logger)
    {
        _sqsClient = sqsClient;
        _logger = logger;
    }

    public async Task SendMessageAsync<T>(T message, string queueUrl)
    {
        try
        {
            var messageBody = JsonSerializer.Serialize(message);
            
            var request = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = messageBody,
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    { "MessageType", new MessageAttributeValue 
                        { StringValue = typeof(T).Name, DataType = "String" } }
                }
            };

            await _sqsClient.SendMessageAsync(request);
            _logger.LogInformation("Message sent to queue: {QueueUrl}", queueUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to queue: {QueueUrl}", queueUrl);
            throw;
        }
    }

    public async Task<IEnumerable<T>> ReceiveMessagesAsync<T>(string queueUrl, int maxMessages = 10)
    {
        try
        {
            var request = new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = maxMessages,
                WaitTimeSeconds = 20,
                MessageAttributeNames = new List<string> { "All" }
            };

            var response = await _sqsClient.ReceiveMessageAsync(request);
            
            var messages = new List<T>();
            foreach (var message in response.Messages)
            {
                try
                {
                    var messageBody = JsonSerializer.Deserialize<T>(message.Body);
                    messages.Add(messageBody);
                    
                    // Eliminar mensaje de la cola
                    await _sqsClient.DeleteMessageAsync(queueUrl, message.ReceiptHandle);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message: {MessageId}", message.MessageId);
                }
            }

            return messages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error receiving messages from queue: {QueueUrl}", queueUrl);
            throw;
        }
    }
}
```

### **6. Amazon CloudWatch**

#### **Configuración de CloudWatch**
```csharp
// Services/CloudWatchService.cs
public class CloudWatchService
{
    private readonly IAmazonCloudWatch _cloudWatchClient;
    private readonly ILogger<CloudWatchService> _logger;

    public CloudWatchService(IAmazonCloudWatch cloudWatchClient, ILogger<CloudWatchService> logger)
    {
        _cloudWatchClient = cloudWatchClient;
        _logger = logger;
    }

    public async Task PutMetricAsync(string metricName, double value, string unit = "Count")
    {
        try
        {
            var request = new PutMetricDataRequest
            {
                Namespace = "MussikOn/Application",
                MetricData = new List<MetricDatum>
                {
                    new MetricDatum
                    {
                        MetricName = metricName,
                        Value = value,
                        Unit = unit,
                        Timestamp = DateTime.UtcNow
                    }
                }
            };

            await _cloudWatchClient.PutMetricDataAsync(request);
            _logger.LogInformation("Metric sent to CloudWatch: {MetricName} = {Value}", metricName, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending metric to CloudWatch: {MetricName}", metricName);
            throw;
        }
    }

    public async Task PutCustomMetricAsync(string metricName, double value, Dictionary<string, string> dimensions)
    {
        try
        {
            var request = new PutMetricDataRequest
            {
                Namespace = "MussikOn/Custom",
                MetricData = new List<MetricDatum>
                {
                    new MetricDatum
                    {
                        MetricName = metricName,
                        Value = value,
                        Unit = "Count",
                        Timestamp = DateTime.UtcNow,
                        Dimensions = dimensions.Select(d => new Dimension
                        {
                            Name = d.Key,
                            Value = d.Value
                        }).ToList()
                    }
                }
            };

            await _cloudWatchClient.PutMetricDataAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending custom metric to CloudWatch: {MetricName}", metricName);
            throw;
        }
    }
}
```

## 🛠️ **Ejercicio Práctico**

### **Ejercicio 1: Integración Completa con AWS**

Crea una aplicación que integre múltiples servicios de AWS:

```csharp
// 1. Configurar servicios AWS
public class AwsServiceConfig
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // AWS SDK
        services.AddAWSService<IAmazonS3>();
        services.AddAWSService<IAmazonDynamoDB>();
        services.AddAWSService<IAmazonSQS>();
        services.AddAWSService<IAmazonCloudWatch>();
        
        // Servicios personalizados
        services.AddSingleton<S3Service>();
        services.AddSingleton<DynamoDbService>();
        services.AddSingleton<SqsService>();
        services.AddSingleton<CloudWatchService>();
    }
}

// 2. Implementar Lambda para procesamiento de eventos
[LambdaFunction]
public async Task<APIGatewayProxyResponse> ProcessMusicianEvent(
    APIGatewayProxyRequest request,
    ILambdaContext context)
{
    var eventData = JsonSerializer.Deserialize<MusicianEventRequest>(request.Body);
    
    // Procesar evento
    var result = await ProcessMusicianEventAsync(eventData);
    
    // Enviar métrica a CloudWatch
    await _cloudWatchService.PutMetricAsync("MusicianEventsProcessed", 1);
    
    return new APIGatewayProxyResponse
    {
        StatusCode = 200,
        Body = JsonSerializer.Serialize(result)
    };
}

// 3. Implementar servicio de notificaciones con SQS
public class NotificationService
{
    private readonly SqsService _sqsService;
    private readonly CloudWatchService _cloudWatchService;
    
    public async Task SendNotificationAsync(NotificationRequest request)
    {
        await _sqsService.SendMessageAsync(request, "https://sqs.us-east-1.amazonaws.com/123456789012/mussikon-notifications");
        await _cloudWatchService.PutMetricAsync("NotificationsSent", 1);
    }
}
```

## 📋 **Resumen de la Clase**

### **Conceptos Clave:**
- **AWS Lambda**: Serverless computing
- **Amazon RDS**: Base de datos relacional
- **Amazon S3**: Almacenamiento de objetos
- **Amazon DynamoDB**: Base de datos NoSQL
- **Amazon SQS**: Cola de mensajes
- **Amazon CloudWatch**: Monitoreo y métricas

### **Próxima Clase:**
**Serverless Architecture con Azure Functions** - Arquitectura serverless

## 🎯 **Objetivos de Aprendizaje**

Al finalizar esta clase, serás capaz de:
- ✅ Implementar AWS Lambda functions con .NET
- ✅ Integrar Amazon RDS para bases de datos relacionales
- ✅ Usar Amazon S3 para almacenamiento de archivos
- ✅ Implementar Amazon DynamoDB para NoSQL
- ✅ Configurar Amazon SQS para mensajería
- ✅ Implementar métricas con Amazon CloudWatch
