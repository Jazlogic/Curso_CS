# Clase 6: Message Bus

## Navegación
- [← Clase anterior: API Gateway](clase_5_api_gateway.md)
- [← Volver al README del módulo](README.md)
- [→ Siguiente clase: Event Sourcing](clase_7_event_sourcing.md)

## Objetivos de Aprendizaje
- Comprender el patrón Message Bus
- Implementar comunicación asíncrona entre servicios
- Aplicar patrones de mensajería robustos
- Crear sistemas de eventos distribuidos

## ¿Qué es un Message Bus?

Un **Message Bus** es un patrón arquitectónico que permite la comunicación asíncrona entre diferentes componentes de un sistema distribuido. Actúa como un intermediario que recibe mensajes de los productores y los distribuye a los consumidores apropiados, proporcionando desacoplamiento, escalabilidad y confiabilidad.

### Beneficios del Message Bus

```csharp
// 1. Desacoplamiento
// Los servicios no necesitan conocer la ubicación de otros servicios
public class OrderService
{
    private readonly IMessageBus _messageBus;
    
    public async Task CreateOrderAsync(Order order)
    {
        // Guardar orden
        await _orderRepository.AddAsync(order);
        
        // Publicar evento sin conocer quién lo consume
        await _messageBus.PublishAsync(new OrderCreatedEvent(order.Id));
    }
}

// 2. Escalabilidad
// Múltiples consumidores pueden procesar el mismo mensaje
public class EmailService : IMessageHandler<OrderCreatedEvent>
{
    public async Task HandleAsync(OrderCreatedEvent @event)
    {
        // Enviar email de confirmación
    }
}

public class InventoryService : IMessageHandler<OrderCreatedEvent>
{
    public async Task HandleAsync(OrderCreatedEvent @event)
    {
        // Actualizar inventario
    }
}

// 3. Confiabilidad
// Los mensajes se pueden persistir y reintentar
public class MessageBus : IMessageBus
{
    public async Task PublishAsync<T>(T message)
    {
        // Persistir mensaje
        await _messageStore.SaveAsync(message);
        
        // Enviar a todos los consumidores
        await _dispatcher.DispatchAsync(message);
    }
}
```

## Implementación del Message Bus

### 1. Interfaces y Contratos

```csharp
// Shared.Messaging/Contracts/IMessage.cs
public interface IMessage
{
    Guid Id { get; }
    DateTime Timestamp { get; }
    string MessageType { get; }
}

// Shared.Messaging/Contracts/IMessageBus.cs
public interface IMessageBus
{
    Task PublishAsync<T>(T message) where T : class;
    Task SubscribeAsync<T>(Func<T, Task> handler) where T : class;
    Task UnsubscribeAsync<T>(Func<T, Task> handler) where T : class;
}

// Shared.Messaging/Contracts/IMessageHandler.cs
public interface IMessageHandler<in TMessage> where TMessage : class
{
    Task HandleAsync(TMessage message);
}

// Shared.Messaging/Contracts/IMessageStore.cs
public interface IMessageStore
{
    Task SaveAsync<T>(T message) where T : class;
    Task<IEnumerable<T>> GetPendingMessagesAsync<T>() where T : class;
    Task MarkAsProcessedAsync(Guid messageId);
    Task MarkAsFailedAsync(Guid messageId, string error);
}

// Shared.Messaging/Contracts/Message.cs
public abstract class Message : IMessage
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string MessageType { get; set; }
    public string CorrelationId { get; set; }
    public string Source { get; set; }
    
    protected Message()
    {
        Id = Guid.NewGuid();
        Timestamp = DateTime.UtcNow;
        MessageType = GetType().Name;
    }
}

// Shared.Messaging/Contracts/OrderMessages.cs
public class OrderCreatedEvent : Message
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderStatusChangedEvent : Message
{
    public Guid OrderId { get; set; }
    public string OldStatus { get; set; }
    public string NewStatus { get; set; }
    public DateTime ChangedAt { get; set; }
}

public class OrderCancelledEvent : Message
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string Reason { get; set; }
    public DateTime CancelledAt { get; set; }
}

// Shared.Messaging/Contracts/CustomerMessages.cs
public class CustomerCreatedEvent : Message
{
    public Guid CustomerId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CustomerProfileUpdatedEvent : Message
{
    public Guid CustomerId { get; set; }
    public string OldName { get; set; }
    public string NewName { get; set; }
    public string OldEmail { get; set; }
    public string NewEmail { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Shared.Messaging/Contracts/ProductMessages.cs
public class ProductStockUpdatedEvent : Message
{
    public Guid ProductId { get; set; }
    public int OldStock { get; set; }
    public int NewStock { get; set; }
    public string Reason { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ProductPriceChangedEvent : Message
{
    public Guid ProductId { get; set; }
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public DateTime ChangedAt { get; set; }
}
```

### 2. Implementación con RabbitMQ

```csharp
// Infrastructure/Messaging/RabbitMQMessageBus.cs
public class RabbitMQMessageBus : IMessageBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQMessageBus> _logger;
    private readonly IConfiguration _configuration;
    private readonly Dictionary<Type, List<Func<object, Task>>> _handlers;
    private readonly Dictionary<Type, string> _exchangeNames;
    
    public RabbitMQMessageBus(IConfiguration configuration, ILogger<RabbitMQMessageBus> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _handlers = new Dictionary<Type, List<Func<object, Task>>>();
        _exchangeNames = new Dictionary<Type, string>();
        
        InitializeConnection();
        DeclareExchanges();
    }
    
    private void InitializeConnection()
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:Host"],
            UserName = _configuration["RabbitMQ:Username"],
            Password = _configuration["RabbitMQ:Password"],
            VirtualHost = _configuration["RabbitMQ:VirtualHost"] ?? "/",
            Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672")
        };
        
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        // Configurar confirmaciones de publicación
        _channel.ConfirmSelect();
        
        // Configurar QoS
        _channel.BasicQos(0, 1, false);
    }
    
    private void DeclareExchanges()
    {
        // Declarar exchanges para diferentes tipos de mensajes
        _exchangeNames[typeof(OrderCreatedEvent)] = "order-events";
        _exchangeNames[typeof(OrderStatusChangedEvent)] = "order-events";
        _exchangeNames[typeof(OrderCancelledEvent)] = "order-events";
        _exchangeNames[typeof(CustomerCreatedEvent)] = "customer-events";
        _exchangeNames[typeof(CustomerProfileUpdatedEvent)] = "customer-events";
        _exchangeNames[typeof(ProductStockUpdatedEvent)] = "product-events";
        _exchangeNames[typeof(ProductPriceChangedEvent)] = "product-events";
        
        foreach (var exchangeName in _exchangeNames.Values.Distinct())
        {
            _channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true, autoDelete: false);
        }
    }
    
    public async Task PublishAsync<T>(T message) where T : class
    {
        try
        {
            var exchangeName = GetExchangeName<T>();
            var routingKey = GetRoutingKey<T>();
            
            var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            var body = Encoding.UTF8.GetBytes(json);
            
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.CorrelationId = GetCorrelationId();
            
            _channel.BasicPublish(
                exchange: exchangeName,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: properties,
                body: body);
            
            // Esperar confirmación
            if (_channel.WaitForConfirms(TimeSpan.FromSeconds(5)))
            {
                _logger.LogInformation("Message published successfully: {MessageType} with ID {MessageId}", 
                    typeof(T).Name, properties.MessageId);
            }
            else
            {
                throw new InvalidOperationException("Message publication not confirmed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message of type {MessageType}", typeof(T).Name);
            throw;
        }
    }
    
    public async Task SubscribeAsync<T>(Func<T, Task> handler) where T : class
    {
        try
        {
            var messageType = typeof(T);
            var exchangeName = GetExchangeName<T>();
            var queueName = $"{messageType.Name.ToLower()}-{Guid.NewGuid():N}";
            var routingKey = GetRoutingKey<T>();
            
            // Declarar cola
            _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(queueName, exchangeName, routingKey);
            
            // Registrar handler
            if (!_handlers.ContainsKey(messageType))
            {
                _handlers[messageType] = new List<Func<object, Task>>();
            }
            
            var handlerWrapper = new Func<object, Task>(async obj => await handler((T)obj));
            _handlers[messageType].Add(handlerWrapper);
            
            // Configurar consumidor
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    
                    // Ejecutar todos los handlers registrados
                    var handlers = _handlers[messageType];
                    var tasks = handlers.Select(h => h(message));
                    await Task.WhenAll(tasks);
                    
                    // Confirmar procesamiento
                    _channel.BasicAck(ea.DeliveryTag, false);
                    
                    _logger.LogInformation("Message processed successfully: {MessageType} with ID {MessageId}", 
                        messageType.Name, ea.BasicProperties.MessageId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message of type {MessageType}", messageType.Name);
                    
                    // Rechazar mensaje y no reintentar
                    _channel.BasicNack(ea.DeliveryTag, false, false);
                }
            };
            
            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
            
            _logger.LogInformation("Subscribed to messages of type {MessageType} on queue {QueueName}", 
                messageType.Name, queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to messages of type {MessageType}", typeof(T).Name);
            throw;
        }
    }
    
    public async Task UnsubscribeAsync<T>(Func<T, Task> handler) where T : class
    {
        var messageType = typeof(T);
        if (_handlers.ContainsKey(messageType))
        {
            var handlerToRemove = _handlers[messageType].FirstOrDefault(h => h == handler);
            if (handlerToRemove != null)
            {
                _handlers[messageType].Remove(handlerToRemove);
                _logger.LogInformation("Unsubscribed from messages of type {MessageType}", messageType.Name);
            }
        }
        
        await Task.CompletedTask;
    }
    
    private string GetExchangeName<T>()
    {
        var messageType = typeof(T);
        return _exchangeNames.TryGetValue(messageType, out var exchangeName) ? exchangeName : "default-events";
    }
    
    private string GetRoutingKey<T>()
    {
        var messageType = typeof(T).Name;
        return messageType.ToLower().Replace("event", "").Replace("message", "");
    }
    
    private string GetCorrelationId()
    {
        // Obtener correlation ID del contexto actual si está disponible
        return Activity.Current?.Id ?? Guid.NewGuid().ToString();
    }
    
    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
```

### 3. Implementación con Azure Service Bus

```csharp
// Infrastructure/Messaging/AzureServiceBusMessageBus.cs
public class AzureServiceBusMessageBus : IMessageBus, IDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusProcessor _processor;
    private readonly ILogger<AzureServiceBusMessageBus> _logger;
    private readonly IConfiguration _configuration;
    private readonly Dictionary<Type, List<Func<object, Task>>> _handlers;
    
    public AzureServiceBusMessageBus(IConfiguration configuration, ILogger<AzureServiceBusMessageBus> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _handlers = new Dictionary<Type, List<Func<object, Task>>>();
        
        var connectionString = _configuration["AzureServiceBus:ConnectionString"];
        _client = new ServiceBusClient(connectionString);
        
        InitializeProcessor();
    }
    
    private void InitializeProcessor()
    {
        var queueName = _configuration["AzureServiceBus:QueueName"] ?? "default-queue";
        
        var options = new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 5,
            AutoCompleteMessages = false
        };
        
        _processor = _client.CreateProcessor(queueName, options);
        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;
    }
    
    public async Task PublishAsync<T>(T message) where T : class
    {
        try
        {
            var sender = _client.CreateSender(GetTopicName<T>());
            
            var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            var serviceBusMessage = new ServiceBusMessage(json)
            {
                MessageId = Guid.NewGuid().ToString(),
                CorrelationId = GetCorrelationId(),
                Subject = typeof(T).Name
            };
            
            await sender.SendMessageAsync(serviceBusMessage);
            await sender.DisposeAsync();
            
            _logger.LogInformation("Message published successfully: {MessageType} with ID {MessageId}", 
                typeof(T).Name, serviceBusMessage.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message of type {MessageType}", typeof(T).Name);
            throw;
        }
    }
    
    public async Task SubscribeAsync<T>(Func<T, Task> handler) where T : class
    {
        try
        {
            var messageType = typeof(T);
            
            if (!_handlers.ContainsKey(messageType))
            {
                _handlers[messageType] = new List<Func<object, Task>>();
            }
            
            var handlerWrapper = new Func<object, Task>(async obj => await handler((T)obj));
            _handlers[messageType].Add(handlerWrapper);
            
            if (!_processor.IsProcessing)
            {
                await _processor.StartProcessingAsync();
            }
            
            _logger.LogInformation("Subscribed to messages of type {MessageType}", messageType.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to messages of type {MessageType}", typeof(T).Name);
            throw;
        }
    }
    
    public async Task UnsubscribeAsync<T>(Func<T, Task> handler) where T : class
    {
        var messageType = typeof(T);
        if (_handlers.ContainsKey(messageType))
        {
            var handlerToRemove = _handlers[messageType].FirstOrDefault(h => h == handler);
            if (handlerToRemove != null)
            {
                _handlers[messageType].Remove(handlerToRemove);
                _logger.LogInformation("Unsubscribed from messages of type {MessageType}", messageType.Name);
            }
        }
        
        await Task.CompletedTask;
    }
    
    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var messageType = GetMessageType(args.Message.Subject);
            if (messageType == null)
            {
                await args.CompleteMessageAsync(args.Message);
                return;
            }
            
            var json = args.Message.Body.ToString();
            var message = JsonSerializer.Deserialize(json, messageType, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            if (_handlers.TryGetValue(messageType, out var handlers))
            {
                var tasks = handlers.Select(h => h(message));
                await Task.WhenAll(tasks);
            }
            
            await args.CompleteMessageAsync(args.Message);
            
            _logger.LogInformation("Message processed successfully: {MessageType} with ID {MessageId}", 
                messageType.Name, args.Message.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message with ID {MessageId}", args.Message.MessageId);
            
            // Abandonar mensaje para reintento
            await args.AbandonMessageAsync(args.Message);
        }
    }
    
    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Error processing message from {ErrorSource}", args.ErrorSource);
        return Task.CompletedTask;
    }
    
    private string GetTopicName<T>()
    {
        var messageType = typeof(T).Name;
        return messageType.ToLower().Replace("event", "").Replace("message", "");
    }
    
    private Type GetMessageType(string subject)
    {
        // Mapear subject a tipo de mensaje
        var messageTypes = new Dictionary<string, Type>
        {
            { "OrderCreatedEvent", typeof(OrderCreatedEvent) },
            { "OrderStatusChangedEvent", typeof(OrderStatusChangedEvent) },
            { "CustomerCreatedEvent", typeof(CustomerCreatedEvent) },
            { "ProductStockUpdatedEvent", typeof(ProductStockUpdatedEvent) }
        };
        
        return messageTypes.TryGetValue(subject, out var type) ? type : null;
    }
    
    private string GetCorrelationId()
    {
        return Activity.Current?.Id ?? Guid.NewGuid().ToString();
    }
    
    public void Dispose()
    {
        _processor?.DisposeAsync();
        _client?.DisposeAsync();
    }
}
```

### 4. Message Handlers

```csharp
// Application/MessageHandlers/OrderMessageHandlers.cs
public class OrderCreatedEventHandler : IMessageHandler<OrderCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly IInventoryService _inventoryService;
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<OrderCreatedEventHandler> _logger;
    
    public OrderCreatedEventHandler(
        IEmailService emailService,
        IInventoryService inventoryService,
        IAnalyticsService analyticsService,
        ILogger<OrderCreatedEventHandler> logger)
    {
        _emailService = emailService;
        _inventoryService = inventoryService;
        _analyticsService = analyticsService;
        _logger = logger;
    }
    
    public async Task HandleAsync(OrderCreatedEvent @event)
    {
        try
        {
            _logger.LogInformation("Handling OrderCreatedEvent for order {OrderId}", @event.OrderId);
            
            // Ejecutar tareas en paralelo
            var tasks = new List<Task>
            {
                SendOrderConfirmationEmailAsync(@event),
                UpdateInventoryAsync(@event),
                RecordAnalyticsAsync(@event)
            };
            
            await Task.WhenAll(tasks);
            
            _logger.LogInformation("Successfully handled OrderCreatedEvent for order {OrderId}", @event.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling OrderCreatedEvent for order {OrderId}", @event.OrderId);
            throw;
        }
    }
    
    private async Task SendOrderConfirmationEmailAsync(OrderCreatedEvent @event)
    {
        try
        {
            await _emailService.SendOrderConfirmationAsync(@event.CustomerId.ToString(), @event.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send order confirmation email for order {OrderId}", @event.OrderId);
        }
    }
    
    private async Task UpdateInventoryAsync(OrderCreatedEvent @event)
    {
        try
        {
            await _inventoryService.ReserveStockForOrderAsync(@event.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update inventory for order {OrderId}", @event.OrderId);
        }
    }
    
    private async Task RecordAnalyticsAsync(OrderCreatedEvent @event)
    {
        try
        {
            await _analyticsService.RecordOrderCreatedAsync(@event.OrderId, @event.CustomerId, @event.Total);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to record analytics for order {OrderId}", @event.OrderId);
        }
    }
}

public class OrderStatusChangedEventHandler : IMessageHandler<OrderStatusChangedEvent>
{
    private readonly IEmailService _emailService;
    private readonly IAuditService _auditService;
    private readonly ILogger<OrderStatusChangedEventHandler> _logger;
    
    public OrderStatusChangedEventHandler(
        IEmailService emailService,
        IAuditService auditService,
        ILogger<OrderStatusChangedEventHandler> logger)
    {
        _emailService = emailService;
        _auditService = auditService;
        _logger = logger;
    }
    
    public async Task HandleAsync(OrderStatusChangedEvent @event)
    {
        try
        {
            _logger.LogInformation("Handling OrderStatusChangedEvent for order {OrderId}: {OldStatus} -> {NewStatus}", 
                @event.OrderId, @event.OldStatus, @event.NewStatus);
            
            // Registrar en auditoría
            await _auditService.LogOrderStatusChangeAsync(@event.OrderId, @event.OldStatus, @event.NewStatus, @event.ChangedAt);
            
            // Enviar notificaciones según el cambio de estado
            await SendStatusChangeNotificationAsync(@event);
            
            _logger.LogInformation("Successfully handled OrderStatusChangedEvent for order {OrderId}", @event.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling OrderStatusChangedEvent for order {OrderId}", @event.OrderId);
            throw;
        }
    }
    
    private async Task SendStatusChangeNotificationAsync(OrderStatusChangedEvent @event)
    {
        try
        {
            switch (@event.NewStatus.ToLower())
            {
                case "confirmed":
                    await _emailService.SendOrderConfirmedAsync(@event.OrderId.ToString(), @event.OrderId);
                    break;
                case "shipped":
                    await _emailService.SendOrderShippedAsync(@event.OrderId.ToString(), @event.OrderId);
                    break;
                case "delivered":
                    await _emailService.SendOrderDeliveredAsync(@event.OrderId.ToString(), @event.OrderId);
                    break;
                case "cancelled":
                    await _emailService.SendOrderCancelledAsync(@event.OrderId.ToString(), @event.OrderId);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send status change notification for order {OrderId}", @event.OrderId);
        }
    }
}

// Application/MessageHandlers/CustomerMessageHandlers.cs
public class CustomerCreatedEventHandler : IMessageHandler<CustomerCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly IWelcomeService _welcomeService;
    private readonly IMarketingService _marketingService;
    private readonly ILogger<CustomerCreatedEventHandler> _logger;
    
    public CustomerCreatedEventHandler(
        IEmailService emailService,
        IWelcomeService welcomeService,
        IMarketingService marketingService,
        ILogger<CustomerCreatedEventHandler> logger)
    {
        _emailService = emailService;
        _welcomeService = welcomeService;
        _marketingService = marketingService;
        _logger = logger;
    }
    
    public async Task HandleAsync(CustomerCreatedEvent @event)
    {
        try
        {
            _logger.LogInformation("Handling CustomerCreatedEvent for customer {CustomerId}", @event.CustomerId);
            
            var tasks = new List<Task>
            {
                SendWelcomeEmailAsync(@event),
                CreateWelcomeProfileAsync(@event),
                SubscribeToNewsletterAsync(@event)
            };
            
            await Task.WhenAll(tasks);
            
            _logger.LogInformation("Successfully handled CustomerCreatedEvent for customer {CustomerId}", @event.CustomerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling CustomerCreatedEvent for customer {CustomerId}", @event.CustomerId);
            throw;
        }
    }
    
    private async Task SendWelcomeEmailAsync(CustomerCreatedEvent @event)
    {
        try
        {
            await _emailService.SendWelcomeEmailAsync(@event.Email, @event.Name);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send welcome email for customer {CustomerId}", @event.CustomerId);
        }
    }
    
    private async Task CreateWelcomeProfileAsync(CustomerCreatedEvent @event)
    {
        try
        {
            await _welcomeService.CreateWelcomeProfileAsync(@event.CustomerId, @event.Name, @event.Email);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create welcome profile for customer {CustomerId}", @event.CustomerId);
        }
    }
    
    private async Task SubscribeToNewsletterAsync(CustomerCreatedEvent @event)
    {
        try
        {
            await _marketingService.SubscribeToNewsletterAsync(@event.Email, @event.Name);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to subscribe customer {CustomerId} to newsletter", @event.CustomerId);
        }
    }
}
```

### 5. Configuración y Registro

```csharp
// Program.cs - Configuración del Message Bus
public void ConfigureServices(IServiceCollection services)
{
    // Registrar Message Bus según configuración
    var messageBusType = Configuration["MessageBus:Type"];
    
    switch (messageBusType?.ToLower())
    {
        case "rabbitmq":
            services.AddSingleton<IMessageBus, RabbitMQMessageBus>();
            break;
        case "azureservicebus":
            services.AddSingleton<IMessageBus, AzureServiceBusMessageBus>();
            break;
        default:
            services.AddSingleton<IMessageBus, InMemoryMessageBus>();
            break;
    }
    
    // Registrar Message Handlers
    services.AddScoped<IMessageHandler<OrderCreatedEvent>, OrderCreatedEventHandler>();
    services.AddScoped<IMessageHandler<OrderStatusChangedEvent>, OrderStatusChangedEventHandler>();
    services.AddScoped<IMessageHandler<CustomerCreatedEvent>, CustomerCreatedEventHandler>();
    
    // Configurar Background Service para procesar mensajes
    services.AddHostedService<MessageProcessingService>();
}

// Background Service para procesar mensajes
public class MessageProcessingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessageBus _messageBus;
    private readonly ILogger<MessageProcessingService> _logger;
    
    public MessageProcessingService(
        IServiceProvider serviceProvider,
        IMessageBus messageBus,
        ILogger<MessageProcessingService> logger)
    {
        _serviceProvider = serviceProvider;
        _messageBus = messageBus;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Suscribirse a todos los tipos de mensajes
            await SubscribeToOrderEventsAsync();
            await SubscribeToCustomerEventsAsync();
            await SubscribeToProductEventsAsync();
            
            _logger.LogInformation("Message processing service started successfully");
            
            // Mantener el servicio ejecutándose
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in message processing service");
        }
    }
    
    private async Task SubscribeToOrderEventsAsync()
    {
        await _messageBus.SubscribeAsync<OrderCreatedEvent>(async @event =>
        {
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<OrderCreatedEvent>>();
            await handler.HandleAsync(@event);
        });
        
        await _messageBus.SubscribeAsync<OrderStatusChangedEvent>(async @event =>
        {
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<OrderStatusChangedEvent>>();
            await handler.HandleAsync(@event);
        });
    }
    
    private async Task SubscribeToCustomerEventsAsync()
    {
        await _messageBus.SubscribeAsync<CustomerCreatedEvent>(async @event =>
        {
            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<CustomerCreatedEvent>>();
            await handler.HandleAsync(@event);
        });
    }
    
    private async Task SubscribeToProductEventsAsync()
    {
        // Implementar suscripciones a eventos de productos
        await Task.CompletedTask;
    }
}
```

## Ejercicios Prácticos

### Ejercicio 1: Implementar Message Handlers
Crea handlers para eventos de productos y clientes.

### Ejercicio 2: Implementar Retry y Dead Letter Queue
Agrega lógica de reintento y cola de mensajes fallidos.

### Ejercicio 3: Implementar Message Store
Crea un store persistente para mensajes.

## Resumen

En esta clase hemos aprendido:

1. **Patrón Message Bus**: Comunicación asíncrona entre servicios
2. **Implementaciones**: RabbitMQ y Azure Service Bus
3. **Message Handlers**: Procesamiento de eventos de dominio
4. **Configuración**: Registro y suscripción a mensajes
5. **Background Services**: Procesamiento continuo de mensajes

En la siguiente clase continuaremos con **Event Sourcing** para implementar auditoría completa y trazabilidad de cambios.

## Recursos Adicionales
- [Message Bus Pattern](https://docs.microsoft.com/en-us/azure/architecture/microservices/microservices-api-gateway)
- [RabbitMQ .NET Client](https://www.rabbitmq.com/dotnet.html)
- [Azure Service Bus](https://docs.microsoft.com/en-us/azure/service-bus-messaging/)


