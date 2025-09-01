# 🚀 Clase 2: Patrones de Comunicación entre Servicios

## 🧭 Navegación
- **⬅️ Anterior**: [Clase 1: Arquitectura de Microservicios Avanzada](clase_1_arquitectura_microservicios_avanzada.md)
- **🏠 Inicio del Módulo**: [Módulo 11: Arquitectura de Microservicios Avanzada](README.md)
- **➡️ Siguiente**: [Clase 3: Service Mesh y API Gateways](clase_3_service_mesh_api_gateways.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 📚 Descripción

En esta clase aprenderás los diferentes patrones de comunicación entre microservicios, incluyendo comunicación síncrona, asíncrona, y estrategias para mantener la resiliencia y el rendimiento en sistemas distribuidos.

## 🎯 Objetivos de Aprendizaje

- Implementar comunicación síncrona entre servicios
- Configurar comunicación asíncrona con message brokers
- Aplicar patrones de resiliencia en la comunicación
- Diseñar estrategias de fallback y degradación

## 📖 Contenido Teórico

### Tipos de Comunicación

#### 1. Comunicación Síncrona (HTTP/REST)

```csharp
// Cliente HTTP para comunicación síncrona
public interface IUserServiceClient
{
    Task<UserDto> GetUserAsync(int id);
    Task<bool> UpdateUserAsync(int id, UpdateUserDto dto);
    Task<UserDto> CreateUserAsync(CreateUserDto dto);
    Task<bool> DeleteUserAsync(int id);
}

public class UserServiceClient : IUserServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserServiceClient> _logger;
    private readonly CircuitBreaker _circuitBreaker;
    private readonly RetryPolicy _retryPolicy;

    public UserServiceClient(
        HttpClient httpClient,
        ILogger<UserServiceClient> logger,
        CircuitBreaker circuitBreaker,
        RetryPolicy retryPolicy)
    {
        _httpClient = httpClient;
        _logger = logger;
        _circuitBreaker = circuitBreaker;
        _retryPolicy = retryPolicy;
    }

    public async Task<UserDto> GetUserAsync(int id)
    {
        return await _circuitBreaker.ExecuteAsync(async () =>
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _httpClient.GetAsync($"/api/users/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<UserDto>();
                    _logger.LogDebug("Successfully retrieved user {UserId}", id);
                    return user;
                }
                
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                
                response.EnsureSuccessStatusCode();
                return null;
            });
        });
    }

    public async Task<bool> UpdateUserAsync(int id, UpdateUserDto dto)
    {
        return await _circuitBreaker.ExecuteAsync(async () =>
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"/api/users/{id}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Successfully updated user {UserId}", id);
                    return true;
                }
                
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }
                
                response.EnsureSuccessStatusCode();
                return false;
            });
        });
    }
}

// Configuración de HttpClient con Polly
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUserServiceClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient<IUserServiceClient, UserServiceClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["UserService:BaseUrl"]);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }
}
```

#### 2. Comunicación Asíncrona (Message Broker)

```csharp
// Interfaz para message broker
public interface IMessageBroker
{
    Task PublishAsync<T>(string topic, T message, IDictionary<string, object> headers = null);
    Task SubscribeAsync<T>(string topic, Func<T, IDictionary<string, object>, Task> handler);
    Task UnsubscribeAsync(string topic);
}

// Implementación con RabbitMQ
public class RabbitMQMessageBroker : IMessageBroker, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQMessageBroker> _logger;
    private readonly Dictionary<string, IModel> _consumerChannels;
    private readonly string _exchangeName;

    public RabbitMQMessageBroker(
        IConnection connection,
        ILogger<RabbitMQMessageBroker> logger,
        string exchangeName = "microservices.exchange")
    {
        _connection = connection;
        _channel = _connection.CreateModel();
        _logger = logger;
        _exchangeName = exchangeName;
        _consumerChannels = new Dictionary<string, IModel>();

        // Configurar exchange
        _channel.ExchangeDeclare(_exchangeName, ExchangeType.Topic, durable: true);
    }

    public async Task PublishAsync<T>(string topic, T message, IDictionary<string, object> headers = null)
    {
        try
        {
            var body = JsonSerializer.SerializeToUtf8Bytes(message);
            var properties = _channel.CreateBasicProperties();
            
            if (headers != null)
            {
                properties.Headers = headers;
            }
            
            properties.Persistent = true;
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(_exchangeName, topic, properties, body);
            
            _logger.LogDebug("Message published to topic {Topic} with ID {MessageId}", 
                topic, properties.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to topic {Topic}", topic);
            throw;
        }
    }

    public async Task SubscribeAsync<T>(string topic, Func<T, IDictionary<string, object>, Task> handler)
    {
        try
        {
            var queueName = $"{topic}.queue";
            var consumerChannel = _connection.CreateModel();
            
            // Declarar queue
            consumerChannel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
            consumerChannel.QueueBind(queueName, _exchangeName, topic);

            // Configurar QoS
            consumerChannel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(consumerChannel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var message = JsonSerializer.Deserialize<T>(ea.Body.Span);
                    var headers = ea.BasicProperties.Headers;

                    await handler(message, headers);

                    consumerChannel.BasicAck(ea.DeliveryTag, false);
                    
                    _logger.LogDebug("Message processed from topic {Topic}", topic);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from topic {Topic}", topic);
                    consumerChannel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            consumerChannel.BasicConsume(queueName, false, consumer);
            _consumerChannels[topic] = consumerChannel;
            
            _logger.LogInformation("Subscribed to topic {Topic}", topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to topic {Topic}", topic);
            throw;
        }
    }

    public void Dispose()
    {
        foreach (var channel in _consumerChannels.Values)
        {
            channel?.Dispose();
        }
        _consumerChannel?.Dispose();
        _connection?.Dispose();
    }
}

// Implementación con Azure Service Bus
public class AzureServiceBusMessageBroker : IMessageBroker, IDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ILogger<AzureServiceBusMessageBroker> _logger;
    private readonly Dictionary<string, ServiceBusProcessor> _processors;

    public AzureServiceBusMessageBroker(
        ServiceBusClient client,
        ILogger<AzureServiceBusMessageBroker> logger)
    {
        _client = client;
        _logger = logger;
        _processors = new Dictionary<string, ServiceBusProcessor>();
    }

    public async Task PublishAsync<T>(string topic, T message, IDictionary<string, object> headers = null)
    {
        try
        {
            var sender = _client.CreateSender(topic);
            var messageBody = JsonSerializer.SerializeToUtf8Bytes(message);
            
            var serviceBusMessage = new ServiceBusMessage(messageBody)
            {
                MessageId = Guid.NewGuid().ToString(),
                CorrelationId = Activity.Current?.Id ?? Guid.NewGuid().ToString()
            };

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    serviceBusMessage.ApplicationProperties[header.Key] = header.Value;
                }
            }

            await sender.SendMessageAsync(serviceBusMessage);
            await sender.DisposeAsync();
            
            _logger.LogDebug("Message published to topic {Topic} with ID {MessageId}", 
                topic, serviceBusMessage.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to topic {Topic}", topic);
            throw;
        }
    }

    public async Task SubscribeAsync<T>(string topic, Func<T, IDictionary<string, object>, Task> handler)
    {
        try
        {
            var processor = _client.CreateProcessor(topic, new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 1,
                AutoCompleteMessages = false
            });

            processor.ProcessMessageAsync += async args =>
            {
                try
                {
                    var message = JsonSerializer.Deserialize<T>(args.Message.Body.Span);
                    var headers = args.Message.ApplicationProperties.ToDictionary(
                        kvp => kvp.Key, kvp => kvp.Value);

                    await handler(message, headers);
                    await args.CompleteMessageAsync(args.Message);
                    
                    _logger.LogDebug("Message processed from topic {Topic}", topic);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from topic {Topic}", topic);
                    await args.DeadLetterMessageAsync(args.Message);
                }
            };

            processor.ProcessErrorAsync += args =>
            {
                _logger.LogError(args.Exception, "Error processing message from topic {Topic}", topic);
                return Task.CompletedTask;
            };

            await processor.StartProcessingAsync();
            _processors[topic] = processor;
            
            _logger.LogInformation("Subscribed to topic {Topic}", topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to topic {Topic}", topic);
            throw;
        }
    }
}
```

### Patrones de Resiliencia

#### 1. Retry Pattern

```csharp
public class RetryPolicy
{
    private readonly ILogger<RetryPolicy> _logger;
    private readonly int _maxRetries;
    private readonly TimeSpan _baseDelay;
    private readonly TimeSpan _maxDelay;

    public RetryPolicy(
        ILogger<RetryPolicy> logger,
        int maxRetries = 3,
        TimeSpan? baseDelay = null,
        TimeSpan? maxDelay = null)
    {
        _logger = logger;
        _maxRetries = maxRetries;
        _baseDelay = baseDelay ?? TimeSpan.FromSeconds(1);
        _maxDelay = maxDelay ?? TimeSpan.FromSeconds(30);
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, string operationName = null)
    {
        var lastException = default(Exception);
        
        for (int attempt = 0; attempt <= _maxRetries; attempt++)
        {
            try
            {
                if (attempt > 0)
                {
                    var delay = CalculateDelay(attempt);
                    _logger.LogInformation("Retrying {Operation} after {Delay}ms (attempt {Attempt}/{MaxRetries})", 
                        operationName ?? "operation", delay.TotalMilliseconds, attempt, _maxRetries);
                    
                    await Task.Delay(delay);
                }

                return await action();
            }
            catch (Exception ex) when (IsRetryableException(ex))
            {
                lastException = ex;
                _logger.LogWarning(ex, "Attempt {Attempt} failed for {Operation}", 
                    attempt + 1, operationName ?? "operation");
                
                if (attempt == _maxRetries)
                {
                    _logger.LogError(ex, "All {MaxRetries} attempts failed for {Operation}", 
                        _maxRetries, operationName ?? "operation");
                    throw;
                }
            }
        }

        throw lastException ?? new InvalidOperationException("Retry policy failed");
    }

    private TimeSpan CalculateDelay(int attempt)
    {
        var delay = TimeSpan.FromMilliseconds(_baseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
        return TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds, _maxDelay.TotalMilliseconds));
    }

    private bool IsRetryableException(Exception ex)
    {
        return ex switch
        {
            HttpRequestException => true,
            TaskCanceledException => true,
            TimeoutException => true,
            _ => false
        };
    }
}
```

#### 2. Timeout Pattern

```csharp
public class TimeoutPolicy
{
    private readonly ILogger<TimeoutPolicy> _logger;
    private readonly TimeSpan _defaultTimeout;

    public TimeoutPolicy(
        ILogger<TimeoutPolicy> logger,
        TimeSpan? defaultTimeout = null)
    {
        _logger = logger;
        _defaultTimeout = defaultTimeout ?? TimeSpan.FromSeconds(30);
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, TimeSpan? timeout = null)
    {
        var actualTimeout = timeout ?? _defaultTimeout;
        
        using var cts = new CancellationTokenSource(actualTimeout);
        
        try
        {
            var result = await action().WaitAsync(cts.Token);
            _logger.LogDebug("Operation completed within timeout {Timeout}", actualTimeout);
            return result;
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
        {
            _logger.LogWarning("Operation timed out after {Timeout}", actualTimeout);
            throw new TimeoutException($"Operation timed out after {actualTimeout}");
        }
    }

    public async Task ExecuteAsync(Func<Task> action, TimeSpan? timeout = null)
    {
        var actualTimeout = timeout ?? _defaultTimeout;
        
        using var cts = new CancellationTokenSource(actualTimeout);
        
        try
        {
            await action().WaitAsync(cts.Token);
            _logger.LogDebug("Operation completed within timeout {Timeout}", actualTimeout);
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
        {
            _logger.LogWarning("Operation timed out after {Timeout}", actualTimeout);
            throw new TimeoutException($"Operation timed out after {actualTimeout}");
        }
    }
}
```

#### 3. Fallback Pattern

```csharp
public class FallbackPolicy
{
    private readonly ILogger<FallbackPolicy> _logger;

    public FallbackPolicy(ILogger<FallbackPolicy> logger)
    {
        _logger = logger;
    }

    public async Task<T> ExecuteAsync<T>(
        Func<Task<T>> primaryAction,
        Func<Task<T>> fallbackAction,
        string operationName = null)
    {
        try
        {
            return await primaryAction();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Primary operation failed for {Operation}, trying fallback", 
                operationName ?? "operation");
            
            try
            {
                var fallbackResult = await fallbackAction();
                _logger.LogInformation("Fallback operation succeeded for {Operation}", 
                    operationName ?? "operation");
                return fallbackResult;
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "Fallback operation also failed for {Operation}", 
                    operationName ?? "operation");
                throw new AggregateException(ex, fallbackEx);
            }
        }
    }

    public async Task<T> ExecuteAsync<T>(
        Func<Task<T>> primaryAction,
        Func<Exception, Task<T>> fallbackAction,
        string operationName = null)
    {
        try
        {
            return await primaryAction();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Primary operation failed for {Operation}, trying fallback", 
                operationName ?? "operation");
            
            try
            {
                var fallbackResult = await fallbackAction(ex);
                _logger.LogInformation("Fallback operation succeeded for {Operation}", 
                    operationName ?? "operation");
                return fallbackResult;
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "Fallback operation also failed for {Operation}", 
                    operationName ?? "operation");
                throw new AggregateException(ex, fallbackEx);
            }
        }
    }
}
```

### Implementación Integrada

```csharp
// Servicio que usa todos los patrones de resiliencia
public class ResilientUserService
{
    private readonly IUserServiceClient _userServiceClient;
    private readonly IUserCacheService _userCacheService;
    private readonly CircuitBreaker _circuitBreaker;
    private readonly RetryPolicy _retryPolicy;
    private readonly TimeoutPolicy _timeoutPolicy;
    private readonly FallbackPolicy _fallbackPolicy;
    private readonly ILogger<ResilientUserService> _logger;

    public ResilientUserService(
        IUserServiceClient userServiceClient,
        IUserCacheService userCacheService,
        CircuitBreaker circuitBreaker,
        RetryPolicy retryPolicy,
        TimeoutPolicy timeoutPolicy,
        FallbackPolicy fallbackPolicy,
        ILogger<ResilientUserService> logger)
    {
        _userServiceClient = userServiceClient;
        _userCacheService = userCacheService;
        _circuitBreaker = circuitBreaker;
        _retryPolicy = retryPolicy;
        _timeoutPolicy = timeoutPolicy;
        _fallbackPolicy = fallbackPolicy;
        _logger = logger;
    }

    public async Task<UserDto> GetUserAsync(int id)
    {
        return await _fallbackPolicy.ExecuteAsync(
            // Operación principal: obtener del servicio remoto
            async () => await GetUserFromRemoteServiceAsync(id),
            // Fallback: obtener del caché local
            async () => await GetUserFromCacheAsync(id),
            "GetUser");
    }

    private async Task<UserDto> GetUserFromRemoteServiceAsync(int id)
    {
        return await _circuitBreaker.ExecuteAsync(async () =>
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                return await _timeoutPolicy.ExecuteAsync(async () =>
                {
                    var user = await _userServiceClient.GetUserAsync(id);
                    
                    if (user != null)
                    {
                        // Guardar en caché para futuros fallbacks
                        await _userCacheService.SetUserAsync(id, user);
                    }
                    
                    return user;
                }, TimeSpan.FromSeconds(10));
            }, "GetUserFromRemoteService");
        });
    }

    private async Task<UserDto> GetUserFromCacheAsync(int id)
    {
        try
        {
            var user = await _userCacheService.GetUserAsync(id);
            if (user != null)
            {
                _logger.LogInformation("User {UserId} retrieved from cache", id);
            }
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get user {UserId} from cache", id);
            return null;
        }
    }
}
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Cliente HTTP Resiliente
Implementa un cliente HTTP que incluya:
- Circuit breaker
- Retry policy
- Timeout policy
- Fallback strategy

### Ejercicio 2: Message Broker con RabbitMQ
Crea un sistema de mensajería que incluya:
- Publicación de mensajes
- Suscripción a topics
- Manejo de errores
- Persistencia de mensajes

### Ejercicio 3: Servicio Resiliente
Implementa un servicio que use todos los patrones de resiliencia:
- Comunicación síncrona con fallback
- Caché local como estrategia de degradación
- Logging de todas las operaciones

## 📝 Quiz de Autoevaluación

1. ¿Cuáles son las ventajas de la comunicación asíncrona sobre la síncrona?
2. ¿Cómo implementarías un fallback cuando un servicio remoto falla?
3. ¿Qué estrategias usarías para manejar timeouts en comunicaciones entre servicios?
4. ¿Cómo implementarías retry policies con backoff exponencial?
5. ¿Qué consideraciones tendrías para elegir entre HTTP y message brokers?

## 🔗 Enlaces Útiles

- [Polly - .NET resilience and transient-fault-handling library](https://github.com/App-vNext/Polly)
- [RabbitMQ .NET Client](https://www.rabbitmq.com/dotnet.html)
- [Azure Service Bus](https://docs.microsoft.com/en-us/azure/service-bus-messaging/)
- [Resilience Patterns](https://docs.microsoft.com/en-us/azure/architecture/patterns/category/resiliency)

## 🚀 Siguiente Clase

En la siguiente clase aprenderás sobre Service Mesh y API Gateways, tecnologías clave para la gestión de la comunicación entre microservicios.

---

**💡 Consejo**: La resiliencia en microservicios no es opcional. Siempre implementa múltiples capas de protección para mantener tu sistema funcionando incluso cuando algunos servicios fallen.
