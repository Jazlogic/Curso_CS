# 🔗 **Clase 9: Testing de Integración Avanzado**

## 🎯 **Objetivos de la Clase**
- Dominar Testing de Integración avanzado
- Implementar TestContainers para bases de datos
- Aplicar testing de microservicios
- Asegurar integración completa de MussikOn

## 📚 **Contenido Teórico**

### **1. Fundamentos de Testing de Integración**

#### **Tipos de Testing de Integración**
```csharp
// 1. Testing de Integración con Base de Datos
[TestFixture]
public class MusicianRepositoryIntegrationTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private IServiceScope _scope;
    private ApplicationDbContext _context;
    
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Reemplazar base de datos en memoria
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }
                    
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDatabase");
                    });
                });
            });
        
        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }
    
    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _scope?.Dispose();
        _client?.Dispose();
        _factory?.Dispose();
    }
    
    [SetUp]
    public void Setup()
    {
        // Limpiar base de datos antes de cada test
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
    }
    
    [Test]
    public async Task CreateMusician_ShouldPersistToDatabase()
    {
        // Arrange
        var musician = new Musician
        {
            Name = "John Doe",
            Email = "john@example.com",
            Genre = "Rock",
            HourlyRate = 100.00m,
            Location = new Location("Madrid", "Spain")
        };
        
        // Act
        _context.Musicians.Add(musician);
        await _context.SaveChangesAsync();
        
        // Assert
        var savedMusician = await _context.Musicians
            .FirstOrDefaultAsync(m => m.Email == "john@example.com");
        
        Assert.That(savedMusician, Is.Not.Null);
        Assert.That(savedMusician.Name, Is.EqualTo("John Doe"));
        Assert.That(savedMusician.Genre, Is.EqualTo("Rock"));
    }
    
    [Test]
    public async Task GetMusiciansByGenre_ShouldReturnFilteredResults()
    {
        // Arrange
        var musicians = new List<Musician>
        {
            new Musician { Name = "John", Genre = "Rock", Email = "john@example.com" },
            new Musician { Name = "Jane", Genre = "Jazz", Email = "jane@example.com" },
            new Musician { Name = "Bob", Genre = "Rock", Email = "bob@example.com" }
        };
        
        _context.Musicians.AddRange(musicians);
        await _context.SaveChangesAsync();
        
        // Act
        var rockMusicians = await _context.Musicians
            .Where(m => m.Genre == "Rock")
            .ToListAsync();
        
        // Assert
        Assert.That(rockMusicians, Has.Count.EqualTo(2));
        Assert.That(rockMusicians.All(m => m.Genre == "Rock"), Is.True);
    }
}

// 2. Testing de Integración con APIs
[TestFixture]
public class MusicianApiIntegrationTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }
    
    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }
    
    [Test]
    public async Task GetMusicians_ShouldReturnOkResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/musicians");
        
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var content = await response.Content.ReadAsStringAsync();
        var musicians = JsonSerializer.Deserialize<List<Musician>>(content);
        
        Assert.That(musicians, Is.Not.Null);
    }
    
    [Test]
    public async Task CreateMusician_ShouldReturnCreatedResponse()
    {
        // Arrange
        var musician = new CreateMusicianRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Genre = "Rock",
            HourlyRate = 100.00m
        };
        
        var json = JsonSerializer.Serialize(musician);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/musicians", content);
        
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdMusician = JsonSerializer.Deserialize<Musician>(responseContent);
        
        Assert.That(createdMusician, Is.Not.Null);
        Assert.That(createdMusician.Name, Is.EqualTo("John Doe"));
    }
}

// 3. Testing de Integración con Servicios Externos
[TestFixture]
public class PaymentServiceIntegrationTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private Mock<IPaymentGateway> _mockPaymentGateway;
    
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _mockPaymentGateway = new Mock<IPaymentGateway>();
        
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(_mockPaymentGateway.Object);
                });
            });
        
        _client = _factory.CreateClient();
    }
    
    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }
    
    [Test]
    public async Task ProcessPayment_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        _mockPaymentGateway.Setup(g => g.ProcessPaymentAsync(It.IsAny<PaymentRequest>()))
                          .ReturnsAsync(new PaymentResult { Success = true, TransactionId = "TXN123" });
        
        var paymentRequest = new PaymentRequest
        {
            Amount = 100.00m,
            CardNumber = "4111111111111111",
            CVV = "123",
            ExpiryDate = DateTime.Now.AddYears(1)
        };
        
        var json = JsonSerializer.Serialize(paymentRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/payments", content);
        
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PaymentResult>(responseContent);
        
        Assert.That(result.Success, Is.True);
        Assert.That(result.TransactionId, Is.EqualTo("TXN123"));
    }
}
```

### **2. TestContainers para Bases de Datos**

#### **Configuración de TestContainers**
```csharp
// Configuración de TestContainers para SQL Server
[TestFixture]
public class MusicianRepositoryTestContainersTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private IServiceScope _scope;
    private ApplicationDbContext _context;
    private MsSqlContainer _sqlServerContainer;
    
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Crear contenedor de SQL Server
        _sqlServerContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("YourStrong@Passw0rd")
            .WithPortBinding(1433, true)
            .Build();
        
        await _sqlServerContainer.StartAsync();
        
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Reemplazar configuración de base de datos
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }
                    
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseSqlServer(_sqlServerContainer.GetConnectionString());
                    });
                });
            });
        
        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Crear base de datos
        await _context.Database.EnsureCreatedAsync();
    }
    
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        _scope?.Dispose();
        _client?.Dispose();
        _factory?.Dispose();
        await _sqlServerContainer?.DisposeAsync();
    }
    
    [SetUp]
    public async Task Setup()
    {
        // Limpiar datos antes de cada test
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM Musicians");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM Events");
    }
    
    [Test]
    public async Task CreateMusician_ShouldPersistToRealDatabase()
    {
        // Arrange
        var musician = new Musician
        {
            Name = "John Doe",
            Email = "john@example.com",
            Genre = "Rock",
            HourlyRate = 100.00m,
            Location = new Location("Madrid", "Spain")
        };
        
        // Act
        _context.Musicians.Add(musician);
        await _context.SaveChangesAsync();
        
        // Assert
        var savedMusician = await _context.Musicians
            .FirstOrDefaultAsync(m => m.Email == "john@example.com");
        
        Assert.That(savedMusician, Is.Not.Null);
        Assert.That(savedMusician.Name, Is.EqualTo("John Doe"));
    }
    
    [Test]
    public async Task ComplexQuery_ShouldWorkWithRealDatabase()
    {
        // Arrange
        var musicians = new List<Musician>
        {
            new Musician { Name = "John", Genre = "Rock", HourlyRate = 100, Location = new Location("Madrid", "Spain") },
            new Musician { Name = "Jane", Genre = "Jazz", HourlyRate = 150, Location = new Location("Barcelona", "Spain") },
            new Musician { Name = "Bob", Genre = "Rock", HourlyRate = 80, Location = new Location("Madrid", "Spain") }
        };
        
        _context.Musicians.AddRange(musicians);
        await _context.SaveChangesAsync();
        
        // Act - Query compleja
        var result = await _context.Musicians
            .Where(m => m.Genre == "Rock" && m.Location.City == "Madrid" && m.HourlyRate <= 100)
            .OrderBy(m => m.HourlyRate)
            .ToListAsync();
        
        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.First().Name, Is.EqualTo("Bob"));
        Assert.That(result.Last().Name, Is.EqualTo("John"));
    }
}

// Configuración de TestContainers para Redis
[TestFixture]
public class CacheServiceTestContainersTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private IServiceScope _scope;
    private IDistributedCache _cache;
    private RedisContainer _redisContainer;
    
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Crear contenedor de Redis
        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithPortBinding(6379, true)
            .Build();
        
        await _redisContainer.StartAsync();
        
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Configurar Redis
                    services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = _redisContainer.GetConnectionString();
                    });
                });
            });
        
        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _cache = _scope.ServiceProvider.GetRequiredService<IDistributedCache>();
    }
    
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        _scope?.Dispose();
        _client?.Dispose();
        _factory?.Dispose();
        await _redisContainer?.DisposeAsync();
    }
    
    [Test]
    public async Task Cache_ShouldStoreAndRetrieveData()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";
        
        // Act
        await _cache.SetStringAsync(key, value);
        var retrievedValue = await _cache.GetStringAsync(key);
        
        // Assert
        Assert.That(retrievedValue, Is.EqualTo(value));
    }
}
```

### **3. Testing de Microservicios**

#### **Testing de Comunicación entre Servicios**
```csharp
// Testing de integración entre MusicianService y EventService
[TestFixture]
public class MusicianEventIntegrationTests
{
    private WebApplicationFactory<Program> _musicianServiceFactory;
    private WebApplicationFactory<Program> _eventServiceFactory;
    private HttpClient _musicianClient;
    private HttpClient _eventClient;
    private MsSqlContainer _sqlServerContainer;
    
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Crear contenedor de SQL Server compartido
        _sqlServerContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("YourStrong@Passw0rd")
            .WithPortBinding(1433, true)
            .Build();
        
        await _sqlServerContainer.StartAsync();
        
        // Configurar MusicianService
        _musicianServiceFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseSqlServer(_sqlServerContainer.GetConnectionString());
                    });
                });
            });
        
        _musicianClient = _musicianServiceFactory.CreateClient();
        
        // Configurar EventService
        _eventServiceFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseSqlServer(_sqlServerContainer.GetConnectionString());
                    });
                });
            });
        
        _eventClient = _eventServiceFactory.CreateClient();
    }
    
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        _musicianClient?.Dispose();
        _eventClient?.Dispose();
        _musicianServiceFactory?.Dispose();
        _eventServiceFactory?.Dispose();
        await _sqlServerContainer?.DisposeAsync();
    }
    
    [Test]
    public async Task MusicianEventIntegration_ShouldWorkEndToEnd()
    {
        // Arrange - Crear músico
        var musicianRequest = new CreateMusicianRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Genre = "Rock",
            HourlyRate = 100.00m
        };
        
        var musicianJson = JsonSerializer.Serialize(musicianRequest);
        var musicianContent = new StringContent(musicianJson, Encoding.UTF8, "application/json");
        
        var musicianResponse = await _musicianClient.PostAsync("/api/musicians", musicianContent);
        var musician = JsonSerializer.Deserialize<Musician>(await musicianResponse.Content.ReadAsStringAsync());
        
        // Arrange - Crear evento
        var eventRequest = new CreateEventRequest
        {
            Title = "Rock Concert",
            Description = "Amazing rock concert",
            Genre = "Rock",
            Budget = 1000.00m,
            Date = DateTime.Now.AddDays(30)
        };
        
        var eventJson = JsonSerializer.Serialize(eventRequest);
        var eventContent = new StringContent(eventJson, Encoding.UTF8, "application/json");
        
        var eventResponse = await _eventClient.PostAsync("/api/events", eventContent);
        var event = JsonSerializer.Deserialize<Event>(await eventResponse.Content.ReadAsStringAsync());
        
        // Act - Aplicar músico al evento
        var applicationRequest = new CreateApplicationRequest
        {
            MusicianId = musician.Id,
            EventId = event.Id,
            Message = "I'm interested in this event",
            ProposedRate = 100.00m
        };
        
        var applicationJson = JsonSerializer.Serialize(applicationRequest);
        var applicationContent = new StringContent(applicationJson, Encoding.UTF8, "application/json");
        
        var applicationResponse = await _eventClient.PostAsync("/api/events/applications", applicationContent);
        
        // Assert
        Assert.That(applicationResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        
        var application = JsonSerializer.Deserialize<MusicianApplication>(await applicationResponse.Content.ReadAsStringAsync());
        Assert.That(application.MusicianId, Is.EqualTo(musician.Id));
        Assert.That(application.EventId, Is.EqualTo(event.Id));
    }
}

// Testing de integración con Message Queue
[TestFixture]
public class MessageQueueIntegrationTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private RabbitMQContainer _rabbitMQContainer;
    private IConnection _connection;
    private IModel _channel;
    
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Crear contenedor de RabbitMQ
        _rabbitMQContainer = new RabbitMQBuilder()
            .WithImage("rabbitmq:3-management")
            .WithPortBinding(5672, true)
            .WithPortBinding(15672, true)
            .Build();
        
        await _rabbitMQContainer.StartAsync();
        
        // Configurar conexión a RabbitMQ
        var factory = new ConnectionFactory
        {
            HostName = _rabbitMQContainer.Hostname,
            Port = _rabbitMQContainer.GetMappedPublicPort(5672),
            UserName = "guest",
            Password = "guest"
        };
        
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.Configure<RabbitMQOptions>(options =>
                    {
                        options.HostName = _rabbitMQContainer.Hostname;
                        options.Port = _rabbitMQContainer.GetMappedPublicPort(5672);
                    });
                });
            });
        
        _client = _factory.CreateClient();
    }
    
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        _client?.Dispose();
        _factory?.Dispose();
        await _rabbitMQContainer?.DisposeAsync();
    }
    
    [Test]
    public async Task SendMessage_ShouldBeReceivedByQueue()
    {
        // Arrange
        var queueName = "test-queue";
        _channel.QueueDeclare(queueName, false, false, false, null);
        
        var message = new { Type = "TestMessage", Content = "Hello World" };
        var messageJson = JsonSerializer.Serialize(message);
        var messageBytes = Encoding.UTF8.GetBytes(messageJson);
        
        // Act
        _channel.BasicPublish("", queueName, null, messageBytes);
        
        // Assert
        var result = _channel.BasicGet(queueName, true);
        Assert.That(result, Is.Not.Null);
        
        var receivedMessage = Encoding.UTF8.GetString(result.Body.ToArray());
        Assert.That(receivedMessage, Is.EqualTo(messageJson));
    }
}
```

### **4. Testing de Performance en Integración**

#### **Testing de Performance de APIs**
```csharp
[TestFixture]
public class ApiPerformanceIntegrationTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }
    
    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }
    
    [Test]
    public async Task GetMusicians_ShouldRespondWithinTimeLimit()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();
        
        // Act
        var response = await _client.GetAsync("/api/musicians");
        
        // Assert
        stopwatch.Stop();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(1000)); // Menos de 1 segundo
    }
    
    [Test]
    public async Task CreateMusician_ShouldRespondWithinTimeLimit()
    {
        // Arrange
        var musician = new CreateMusicianRequest
        {
            Name = "John Doe",
            Email = "john@example.com",
            Genre = "Rock",
            HourlyRate = 100.00m
        };
        
        var json = JsonSerializer.Serialize(musician);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var stopwatch = Stopwatch.StartNew();
        
        // Act
        var response = await _client.PostAsync("/api/musicians", content);
        
        // Assert
        stopwatch.Stop();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(2000)); // Menos de 2 segundos
    }
}
```

## 🛠️ **Ejercicios Prácticos**

### **Ejercicio 1: Implementar Testing de Integración para MussikOn**
```csharp
// Implementar testing de integración para:
// 1. MusicianService con base de datos
// 2. EventService con base de datos
// 3. PaymentService con gateway externo
// 4. ChatService con SignalR

[TestFixture]
public class MussikOnIntegrationTests
{
    // TODO: Implementar tests de integración
}
```

### **Ejercicio 2: Configurar TestContainers**
```csharp
// Configurar TestContainers para MussikOn:
// 1. SQL Server
// 2. Redis
// 3. RabbitMQ
// 4. Elasticsearch

[TestFixture]
public class MussikOnTestContainersTests
{
    // TODO: Configurar TestContainers
}
```

### **Ejercicio 3: Testing de Microservicios**
```csharp
// Implementar testing de microservicios:
// 1. Comunicación entre servicios
// 2. Message queues
// 3. Event sourcing
// 4. CQRS

[TestFixture]
public class MussikOnMicroservicesTests
{
    // TODO: Implementar tests de microservicios
}
```

## 🎯 **Objetivos de Aprendizaje**

Al finalizar esta clase, serás capaz de:

1. **Implementar Testing de Integración** avanzado
2. **Configurar TestContainers** para bases de datos
3. **Testear microservicios** y comunicación
4. **Aplicar testing de performance** en integración
5. **Asegurar integración completa** de sistemas

## 📝 **Resumen**

En esta clase hemos cubierto:

- **Testing de Integración**: Tipos y fundamentos
- **TestContainers**: Bases de datos reales
- **Microservicios**: Comunicación entre servicios
- **Performance**: Testing de rendimiento
- **End-to-End**: Testing completo

## 🚀 **Siguiente Clase**

En la próxima clase exploraremos **Proyecto Final** donde implementaremos un sistema completo de testing para MussikOn.

---

**💡 Tip**: El testing de integración no es solo sobre conectar componentes, es sobre asegurar que el sistema completo funcione como se espera en producción.
