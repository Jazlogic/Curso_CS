# 🔄 **Clase 5: Event-Driven Architecture y Message Queues**

## 🎯 **Objetivo de la Clase**
Dominar la arquitectura basada en eventos, implementando patrones de mensajería asíncrona, event sourcing y sistemas reactivos con .NET.

## 📚 **Contenido Teórico**

### **1. Fundamentos de Event-Driven Architecture**

#### **¿Qué es Event-Driven Architecture?**
**Event-Driven Architecture (EDA)** es un patrón de diseño donde la producción, detección, consumo y reacción a eventos determinan el flujo de la aplicación.

#### **Componentes Principales:**
- **Event Producers**: Generan eventos
- **Event Consumers**: Procesan eventos
- **Event Store**: Almacena eventos
- **Message Brokers**: Distribuyen eventos

### **2. Azure Service Bus**

#### **Configuración de Service Bus**
```csharp
// Services/ServiceBusEventService.cs
public class ServiceBusEventService
{
    private readonly ServiceBusClient _client;
    private readonly ILogger<ServiceBusEventService> _logger;

    public ServiceBusEventService(IConfiguration configuration, ILogger<ServiceBusEventService> logger)
    {
        var connectionString = configuration.GetConnectionString("ServiceBus");
        _client = new ServiceBusClient(connectionString);
        _logger = logger;
    }

    public async Task PublishEventAsync<T>(T eventData, string topicName)
    {
        var sender = _client.CreateSender(topicName);
        
        try
        {
            var message = new ServiceBusMessage(JsonSerializer.Serialize(eventData))
            {
                ContentType = "application/json",
                MessageId = Guid.NewGuid().ToString(),
                Subject = typeof(T).Name,
                TimeToLive = TimeSpan.FromHours(24)
            };

            // Agregar propiedades personalizadas
            message.ApplicationProperties.Add("EventType", typeof(T).Name);
            message.ApplicationProperties.Add("Timestamp", DateTime.UtcNow);
            message.ApplicationProperties.Add("Version", "1.0");

            await sender.SendMessageAsync(message);
            _logger.LogInformation("Event published to topic {TopicName}: {EventType}", topicName, typeof(T).Name);
        }
        finally
        {
            await sender.CloseAsync();
        }
    }

    public async Task SubscribeToEventsAsync<T>(string topicName, string subscriptionName, Func<T, Task> eventHandler)
    {
        var processor = _client.CreateProcessor(topicName, subscriptionName, new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 1,
            AutoCompleteMessages = false,
            MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5)
        });

        processor.ProcessMessageAsync += async args =>
        {
            try
            {
                var messageBody = args.Message.Body.ToString();
                var eventData = JsonSerializer.Deserialize<T>(messageBody);
                
                await eventHandler(eventData);
                await args.CompleteMessageAsync(args.Message);
                
                _logger.LogInformation("Event processed successfully: {EventType}", typeof(T).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event: {EventType}", typeof(T).Name);
                await args.AbandonMessageAsync(args.Message);
            }
        };

        processor.ProcessErrorAsync += args =>
        {
            _logger.LogError(args.Exception, "Error in event processor");
            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync();
    }
}
```

#### **Eventos de Dominio**
```csharp
// Events/DomainEvents.cs
public abstract class DomainEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
}

public class MusicianRegisteredEvent : DomainEvent
{
    public string MusicianId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> Genres { get; set; } = new();
    public string Location { get; set; } = string.Empty;
}

public class EventCreatedEvent : DomainEvent
{
    public string EventId { get; set; } = string.Empty;
    public string OrganizerId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public decimal Budget { get; set; }
}

public class MusicianApplicationSubmittedEvent : DomainEvent
{
    public string ApplicationId { get; set; } = string.Empty;
    public string MusicianId { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public decimal ProposedFee { get; set; }
}

public class ContractSignedEvent : DomainEvent
{
    public string ContractId { get; set; } = string.Empty;
    public string MusicianId { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime EventDate { get; set; }
}

public class PaymentProcessedEvent : DomainEvent
{
    public string PaymentId { get; set; } = string.Empty;
    public string ContractId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
}
```

### **3. Event Sourcing**

#### **Event Store**
```csharp
// Services/EventStore.cs
public class EventStore
{
    private readonly CosmosClient _cosmosClient;
    private readonly ILogger<EventStore> _logger;

    public EventStore(CosmosClient cosmosClient, ILogger<EventStore> logger)
    {
        _cosmosClient = cosmosClient;
        _logger = logger;
    }

    public async Task SaveEventsAsync(Guid aggregateId, IEnumerable<DomainEvent> events, int expectedVersion)
    {
        var container = _cosmosClient.GetContainer("MussikOn", "Events");
        
        foreach (var domainEvent in events)
        {
            var eventEntity = new EventEntity
            {
                Id = Guid.NewGuid().ToString(),
                AggregateId = aggregateId.ToString(),
                EventType = domainEvent.GetType().Name,
                EventData = JsonSerializer.Serialize(domainEvent),
                Version = expectedVersion + 1,
                Timestamp = domainEvent.OccurredAt,
                PartitionKey = aggregateId.ToString()
            };

            await container.CreateItemAsync(eventEntity);
        }
        
        _logger.LogInformation("Saved {EventCount} events for aggregate {AggregateId}", events.Count(), aggregateId);
    }

    public async Task<IEnumerable<DomainEvent>> GetEventsAsync(Guid aggregateId)
    {
        var container = _cosmosClient.GetContainer("MussikOn", "Events");
        
        var query = new QueryDefinition("SELECT * FROM c WHERE c.AggregateId = @aggregateId ORDER BY c.Version")
            .WithParameter("@aggregateId", aggregateId.ToString());

        var events = new List<DomainEvent>();
        var iterator = container.GetItemQueryIterator<EventEntity>(query);
        
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            
            foreach (var eventEntity in response)
            {
                var domainEvent = JsonSerializer.Deserialize<DomainEvent>(eventEntity.EventData);
                events.Add(domainEvent);
            }
        }

        return events;
    }

    public async Task<IEnumerable<DomainEvent>> GetEventsByTypeAsync(string eventType, DateTime fromDate)
    {
        var container = _cosmosClient.GetContainer("MussikOn", "Events");
        
        var query = new QueryDefinition("SELECT * FROM c WHERE c.EventType = @eventType AND c.Timestamp >= @fromDate ORDER BY c.Timestamp")
            .WithParameter("@eventType", eventType)
            .WithParameter("@fromDate", fromDate);

        var events = new List<DomainEvent>();
        var iterator = container.GetItemQueryIterator<EventEntity>(query);
        
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            
            foreach (var eventEntity in response)
            {
                var domainEvent = JsonSerializer.Deserialize<DomainEvent>(eventEntity.EventData);
                events.Add(domainEvent);
            }
        }

        return events;
    }
}

// Models/EventEntity.cs
public class EventEntity
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonProperty("aggregateId")]
    public string AggregateId { get; set; } = string.Empty;
    
    [JsonProperty("eventType")]
    public string EventType { get; set; } = string.Empty;
    
    [JsonProperty("eventData")]
    public string EventData { get; set; } = string.Empty;
    
    [JsonProperty("version")]
    public int Version { get; set; }
    
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }
    
    [JsonProperty("partitionKey")]
    public string PartitionKey { get; set; } = string.Empty;
}
```

### **4. Event Handlers**

#### **Event Handlers para MussikOn**
```csharp
// Handlers/MusicianEventHandler.cs
public class MusicianEventHandler
{
    private readonly INotificationService _notificationService;
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<MusicianEventHandler> _logger;

    public MusicianEventHandler(
        INotificationService notificationService,
        IAnalyticsService analyticsService,
        ILogger<MusicianEventHandler> logger)
    {
        _notificationService = notificationService;
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task HandleAsync(MusicianRegisteredEvent eventData)
    {
        _logger.LogInformation("Handling MusicianRegisteredEvent for {MusicianId}", eventData.MusicianId);

        // Enviar email de bienvenida
        await _notificationService.SendWelcomeEmailAsync(eventData.Email, eventData.Name);

        // Registrar en analytics
        await _analyticsService.TrackUserRegistrationAsync(eventData.MusicianId, "Musician");

        // Crear perfil inicial
        await CreateInitialProfileAsync(eventData);
    }

    public async Task HandleAsync(MusicianApplicationSubmittedEvent eventData)
    {
        _logger.LogInformation("Handling MusicianApplicationSubmittedEvent for {ApplicationId}", eventData.ApplicationId);

        // Notificar al organizador
        await _notificationService.NotifyOrganizerOfApplicationAsync(eventData.EventId, eventData.MusicianId);

        // Actualizar analytics
        await _analyticsService.TrackApplicationSubmittedAsync(eventData.ApplicationId);

        // Verificar disponibilidad
        await CheckMusicianAvailabilityAsync(eventData.MusicianId, eventData.EventId);
    }

    private async Task CreateInitialProfileAsync(MusicianRegisteredEvent eventData)
    {
        // Lógica para crear perfil inicial
    }

    private async Task CheckMusicianAvailabilityAsync(string musicianId, string eventId)
    {
        // Lógica para verificar disponibilidad
    }
}

// Handlers/EventEventHandler.cs
public class EventEventHandler
{
    private readonly IMusicianMatchingService _matchingService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<EventEventHandler> _logger;

    public EventEventHandler(
        IMusicianMatchingService matchingService,
        INotificationService notificationService,
        ILogger<EventEventHandler> logger)
    {
        _matchingService = matchingService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task HandleAsync(EventCreatedEvent eventData)
    {
        _logger.LogInformation("Handling EventCreatedEvent for {EventId}", eventData.EventId);

        // Buscar músicos compatibles
        var matches = await _matchingService.FindMatchingMusiciansAsync(eventData);

        // Enviar notificaciones a músicos
        await _notificationService.NotifyMusiciansOfNewEventAsync(matches, eventData);

        // Registrar en analytics
        await _analyticsService.TrackEventCreatedAsync(eventData.EventId, eventData.Genre);
    }
}

// Handlers/ContractEventHandler.cs
public class ContractEventHandler
{
    private readonly INotificationService _notificationService;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<ContractEventHandler> _logger;

    public ContractEventHandler(
        INotificationService notificationService,
        IPaymentService paymentService,
        ILogger<ContractEventHandler> logger)
    {
        _notificationService = notificationService;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task HandleAsync(ContractSignedEvent eventData)
    {
        _logger.LogInformation("Handling ContractSignedEvent for {ContractId}", eventData.ContractId);

        // Enviar confirmación a ambas partes
        await _notificationService.SendContractConfirmationAsync(eventData);

        // Procesar pago inicial
        await _paymentService.ProcessInitialPaymentAsync(eventData.ContractId, eventData.Amount);

        // Actualizar analytics
        await _analyticsService.TrackContractSignedAsync(eventData.ContractId, eventData.Amount);
    }

    public async Task HandleAsync(PaymentProcessedEvent eventData)
    {
        _logger.LogInformation("Handling PaymentProcessedEvent for {PaymentId}", eventData.PaymentId);

        if (eventData.Status == "Completed")
        {
            // Enviar confirmación de pago
            await _notificationService.SendPaymentConfirmationAsync(eventData);

            // Actualizar estado del contrato
            await UpdateContractStatusAsync(eventData.ContractId, "Paid");
        }
        else
        {
            // Manejar pago fallido
            await HandleFailedPaymentAsync(eventData);
        }
    }

    private async Task UpdateContractStatusAsync(string contractId, string status)
    {
        // Lógica para actualizar estado del contrato
    }

    private async Task HandleFailedPaymentAsync(PaymentProcessedEvent eventData)
    {
        // Lógica para manejar pago fallido
    }
}
```

### **5. Message Queues con RabbitMQ**

#### **Configuración de RabbitMQ**
```csharp
// Services/RabbitMQService.cs
public class RabbitMQService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQService> _logger;

    public RabbitMQService(IConfiguration configuration, ILogger<RabbitMQService> logger)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"],
            Port = int.Parse(configuration["RabbitMQ:Port"]),
            UserName = configuration["RabbitMQ:UserName"],
            Password = configuration["RabbitMQ:Password"]
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _logger = logger;

        SetupQueues();
    }

    private void SetupQueues()
    {
        // Declarar exchanges
        _channel.ExchangeDeclare("mussikon.events", ExchangeType.Topic, true);
        _channel.ExchangeDeclare("mussikon.notifications", ExchangeType.Direct, true);

        // Declarar colas
        _channel.QueueDeclare("musician.registered", true, false, false);
        _channel.QueueDeclare("event.created", true, false, false);
        _channel.QueueDeclare("application.submitted", true, false, false);
        _channel.QueueDeclare("contract.signed", true, false, false);
        _channel.QueueDeclare("payment.processed", true, false, false);

        // Bindings
        _channel.QueueBind("musician.registered", "mussikon.events", "musician.registered");
        _channel.QueueBind("event.created", "mussikon.events", "event.created");
        _channel.QueueBind("application.submitted", "mussikon.events", "application.submitted");
        _channel.QueueBind("contract.signed", "mussikon.events", "contract.signed");
        _channel.QueueBind("payment.processed", "mussikon.events", "payment.processed");
    }

    public async Task PublishEventAsync<T>(T eventData, string routingKey)
    {
        try
        {
            var message = JsonSerializer.Serialize(eventData);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Headers = new Dictionary<string, object>
            {
                { "EventType", typeof(T).Name },
                { "Version", "1.0" }
            };

            _channel.BasicPublish("mussikon.events", routingKey, properties, body);
            
            _logger.LogInformation("Event published to RabbitMQ: {RoutingKey}", routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event to RabbitMQ: {RoutingKey}", routingKey);
            throw;
        }
    }

    public async Task ConsumeEventsAsync<T>(string queueName, Func<T, Task> eventHandler)
    {
        var consumer = new EventingBasicConsumer(_channel);
        
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var eventData = JsonSerializer.Deserialize<T>(message);

                await eventHandler(eventData);
                
                _channel.BasicAck(ea.DeliveryTag, false);
                _logger.LogInformation("Event consumed successfully: {QueueName}", queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consuming event from queue: {QueueName}", queueName);
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume(queueName, false, consumer);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
```

### **6. Event-Driven Patterns**

#### **Saga Pattern**
```csharp
// Patterns/SagaPattern.cs
public class MusicianApplicationSaga
{
    private readonly IEventStore _eventStore;
    private readonly INotificationService _notificationService;
    private readonly ILogger<MusicianApplicationSaga> _logger;

    public MusicianApplicationSaga(
        IEventStore eventStore,
        INotificationService notificationService,
        ILogger<MusicianApplicationSaga> logger)
    {
        _eventStore = eventStore;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task HandleAsync(MusicianApplicationSubmittedEvent eventData)
    {
        try
        {
            // Paso 1: Validar aplicación
            await ValidateApplicationAsync(eventData);

            // Paso 2: Verificar disponibilidad
            await CheckAvailabilityAsync(eventData);

            // Paso 3: Notificar al organizador
            await NotifyOrganizerAsync(eventData);

            // Paso 4: Marcar como procesada
            await MarkAsProcessedAsync(eventData);

            _logger.LogInformation("Saga completed successfully for application {ApplicationId}", eventData.ApplicationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Saga failed for application {ApplicationId}", eventData.ApplicationId);
            await CompensateAsync(eventData, ex);
        }
    }

    private async Task ValidateApplicationAsync(MusicianApplicationSubmittedEvent eventData)
    {
        // Lógica de validación
    }

    private async Task CheckAvailabilityAsync(MusicianApplicationSubmittedEvent eventData)
    {
        // Lógica de verificación de disponibilidad
    }

    private async Task NotifyOrganizerAsync(MusicianApplicationSubmittedEvent eventData)
    {
        // Lógica de notificación
    }

    private async Task MarkAsProcessedAsync(MusicianApplicationSubmittedEvent eventData)
    {
        // Lógica para marcar como procesada
    }

    private async Task CompensateAsync(MusicianApplicationSubmittedEvent eventData, Exception exception)
    {
        // Lógica de compensación
        await _notificationService.SendErrorNotificationAsync(eventData.ApplicationId, exception.Message);
    }
}
```

## 🛠️ **Ejercicio Práctico**

### **Ejercicio 1: Implementar Sistema Event-Driven Completo**

Crea un sistema event-driven completo para MussikOn:

```csharp
// 1. Configurar Event Bus
public class EventBus
{
    private readonly ServiceBusEventService _serviceBusService;
    private readonly RabbitMQService _rabbitMQService;
    private readonly ILogger<EventBus> _logger;

    public async Task PublishAsync<T>(T eventData) where T : DomainEvent
    {
        // Publicar en Service Bus
        await _serviceBusService.PublishEventAsync(eventData, "mussikon-events");
        
        // Publicar en RabbitMQ
        await _rabbitMQService.PublishEventAsync(eventData, eventData.EventType.ToLower());
        
        _logger.LogInformation("Event published: {EventType}", typeof(T).Name);
    }
}

// 2. Implementar Event Handlers
public class MussikOnEventHandlers
{
    private readonly MusicianEventHandler _musicianEventHandler;
    private readonly EventEventHandler _eventEventHandler;
    private readonly ContractEventHandler _contractEventHandler;

    public async Task HandleMusicianRegisteredAsync(MusicianRegisteredEvent eventData)
    {
        await _musicianEventHandler.HandleAsync(eventData);
    }

    public async Task HandleEventCreatedAsync(EventCreatedEvent eventData)
    {
        await _eventEventHandler.HandleAsync(eventData);
    }

    public async Task HandleContractSignedAsync(ContractSignedEvent eventData)
    {
        await _contractEventHandler.HandleAsync(eventData);
    }
}

// 3. Implementar Event Sourcing
public class MussikOnEventSourcing
{
    private readonly EventStore _eventStore;
    private readonly ILogger<MussikOnEventSourcing> _logger;

    public async Task SaveEventAsync<T>(T eventData) where T : DomainEvent
    {
        await _eventStore.SaveEventsAsync(Guid.Parse(eventData.Id.ToString()), new[] { eventData }, 1);
    }

    public async Task<IEnumerable<DomainEvent>> GetEventsAsync(Guid aggregateId)
    {
        return await _eventStore.GetEventsAsync(aggregateId);
    }
}
```

## 📋 **Resumen de la Clase**

### **Conceptos Clave:**
- **Event-Driven Architecture**: Arquitectura basada en eventos
- **Event Sourcing**: Almacenamiento de eventos
- **Message Queues**: Colas de mensajes
- **Event Handlers**: Procesadores de eventos
- **Saga Pattern**: Patrón de transacciones distribuidas
- **Service Bus**: Mensajería empresarial

### **Próxima Clase:**
**Cloud Storage y CDN** - Almacenamiento en la nube

## 🎯 **Objetivos de Aprendizaje**

Al finalizar esta clase, serás capaz de:
- ✅ Diseñar arquitecturas event-driven
- ✅ Implementar event sourcing
- ✅ Usar Service Bus para mensajería
- ✅ Configurar RabbitMQ para colas
- ✅ Implementar event handlers
- ✅ Aplicar patrones de saga
