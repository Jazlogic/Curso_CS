# 🤝 **Clase 4: Contract Testing con Pact**

## 🎯 **Objetivos de la Clase**
- Dominar Contract Testing con Pact
- Implementar Consumer-Driven Contracts
- Aplicar Contract Testing en microservicios
- Asegurar compatibilidad entre servicios de MussikOn

## 📚 **Contenido Teórico**

### **1. Fundamentos de Contract Testing**

#### **¿Qué es Contract Testing?**
```csharp
// Contract Testing asegura que los servicios se comuniquen correctamente
// sin necesidad de ejecutar todos los servicios juntos

// Ejemplo: MusicianService (Consumer) y EventService (Provider)
public interface IEventServiceContract
{
    Task<EventDetails> GetEventAsync(int eventId);
    Task<EventSearchResult> SearchEventsAsync(EventSearchCriteria criteria);
    Task<Event> CreateEventAsync(CreateEventRequest request);
}

// El contrato define la interfaz esperada
public class EventServiceContract
{
    public string Provider { get; set; } = "EventService";
    public string Consumer { get; set; } = "MusicianService";
    public List<Interaction> Interactions { get; set; } = new();
}
```

#### **Consumer-Driven Contracts**
```csharp
// MusicianService (Consumer) define qué espera del EventService
public class MusicianServiceEventContract
{
    [Fact]
    public async Task GetEvent_ShouldReturnEventDetails()
    {
        // Arrange
        var pact = new PactBuilder()
            .ServiceConsumer("MusicianService")
            .HasPactWith("EventService")
            .WithHttpInteractions()
            .Given("Event with ID 123 exists")
            .UponReceiving("A request for event details")
            .With(Request.Create()
                .WithMethod(HttpMethod.Get)
                .WithPath("/api/events/123")
                .WithHeader("Accept", "application/json")
                .WithHeader("Authorization", "Bearer valid-token"))
            .WillRespondWith(Response.Create()
                .WithStatus(200)
                .WithHeader("Content-Type", "application/json")
                .WithJsonBody(new
                {
                    id = 123,
                    title = "Rock Concert",
                    description = "Amazing rock concert",
                    genre = "Rock",
                    location = new
                    {
                        city = "Madrid",
                        country = "Spain"
                    },
                    date = "2024-06-15T20:00:00Z",
                    budget = 1500.00m,
                    status = "Active"
                }));

        // Act & Assert
        await pact.VerifyAsync(async (context) =>
        {
            var eventService = new EventService(context.MockServerUri);
            var eventDetails = await eventService.GetEventAsync(123);
            
            Assert.That(eventDetails, Is.Not.Null);
            Assert.That(eventDetails.Id, Is.EqualTo(123));
            Assert.That(eventDetails.Title, Is.EqualTo("Rock Concert"));
            Assert.That(eventDetails.Genre, Is.EqualTo("Rock"));
        });
    }
}
```

### **2. Implementación de Pact en .NET**

#### **Configuración de Pact**
```csharp
// Program.cs
using PactNet;
using PactNet.Infrastructure.Outputters;
using PactNet.Verifier;

var builder = WebApplication.CreateBuilder(args);

// Configurar Pact
builder.Services.AddPact(options =>
{
    options.PactDir = "./pacts";
    options.LogDir = "./logs";
    options.DefaultJsonSettings = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
});

var app = builder.Build();

// Configurar endpoints para Pact
app.MapGet("/api/events/{id}", async (int id) =>
{
    // Simular respuesta del EventService
    return Results.Ok(new
    {
        id = id,
        title = "Rock Concert",
        description = "Amazing rock concert",
        genre = "Rock",
        location = new
        {
            city = "Madrid",
            country = "Spain"
        },
        date = "2024-06-15T20:00:00Z",
        budget = 1500.00m,
        status = "Active"
    });
});

app.Run();
```

#### **Consumer Tests con Pact**
```csharp
public class MusicianServiceEventContractTests
{
    private readonly IPactBuilderV4 _pactBuilder;
    private readonly string _consumerName = "MusicianService";
    private readonly string _providerName = "EventService";
    
    public MusicianServiceEventContractTests()
    {
        _pactBuilder = new PactBuilderV4(_consumerName, _providerName)
            .WithHttpInteractions()
            .WithPactDirectory("./pacts");
    }
    
    [Fact]
    public async Task GetEvent_WithValidId_ShouldReturnEventDetails()
    {
        // Arrange
        var eventId = 123;
        var expectedEvent = new
        {
            id = eventId,
            title = "Rock Concert",
            description = "Amazing rock concert",
            genre = "Rock",
            location = new
            {
                city = "Madrid",
                country = "Spain"
            },
            date = "2024-06-15T20:00:00Z",
            budget = 1500.00m,
            status = "Active"
        };
        
        _pactBuilder
            .WithHttpInteractions()
            .Given("Event with ID 123 exists")
            .UponReceiving("A request for event details")
            .With(Request.Create()
                .WithMethod(HttpMethod.Get)
                .WithPath($"/api/events/{eventId}")
                .WithHeader("Accept", "application/json")
                .WithHeader("Authorization", "Bearer valid-token"))
            .WillRespondWith(Response.Create()
                .WithStatus(200)
                .WithHeader("Content-Type", "application/json")
                .WithJsonBody(expectedEvent));
        
        // Act & Assert
        await _pactBuilder.VerifyAsync(async (context) =>
        {
            var eventService = new EventService(context.MockServerUri);
            var eventDetails = await eventService.GetEventAsync(eventId);
            
            Assert.That(eventDetails, Is.Not.Null);
            Assert.That(eventDetails.Id, Is.EqualTo(eventId));
            Assert.That(eventDetails.Title, Is.EqualTo("Rock Concert"));
            Assert.That(eventDetails.Genre, Is.EqualTo("Rock"));
            Assert.That(eventDetails.Budget, Is.EqualTo(1500.00m));
        });
    }
    
    [Fact]
    public async Task SearchEvents_WithValidCriteria_ShouldReturnMatchingEvents()
    {
        // Arrange
        var searchCriteria = new
        {
            genre = "Rock",
            location = "Madrid",
            budget = 1000.00m,
            date = "2024-06-15T20:00:00Z"
        };
        
        var expectedResults = new
        {
            events = new[]
            {
                new
                {
                    id = 123,
                    title = "Rock Concert",
                    genre = "Rock",
                    location = new { city = "Madrid", country = "Spain" },
                    budget = 1500.00m,
                    date = "2024-06-15T20:00:00Z"
                }
            },
            totalCount = 1,
            page = 1,
            pageSize = 10
        };
        
        _pactBuilder
            .WithHttpInteractions()
            .Given("Events exist matching the criteria")
            .UponReceiving("A request to search events")
            .With(Request.Create()
                .WithMethod(HttpMethod.Post)
                .WithPath("/api/events/search")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("Accept", "application/json")
                .WithJsonBody(searchCriteria))
            .WillRespondWith(Response.Create()
                .WithStatus(200)
                .WithHeader("Content-Type", "application/json")
                .WithJsonBody(expectedResults));
        
        // Act & Assert
        await _pactBuilder.VerifyAsync(async (context) =>
        {
            var eventService = new EventService(context.MockServerUri);
            var results = await eventService.SearchEventsAsync(new EventSearchCriteria
            {
                Genre = "Rock",
                Location = "Madrid",
                Budget = 1000.00m,
                Date = DateTime.Parse("2024-06-15T20:00:00Z")
            });
            
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Events, Has.Count.EqualTo(1));
            Assert.That(results.TotalCount, Is.EqualTo(1));
            Assert.That(results.Events.First().Genre, Is.EqualTo("Rock"));
        });
    }
    
    [Fact]
    public async Task GetEvent_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidEventId = 999999;
        
        _pactBuilder
            .WithHttpInteractions()
            .Given("Event with ID 999999 does not exist")
            .UponReceiving("A request for non-existent event")
            .With(Request.Create()
                .WithMethod(HttpMethod.Get)
                .WithPath($"/api/events/{invalidEventId}")
                .WithHeader("Accept", "application/json"))
            .WillRespondWith(Response.Create()
                .WithStatus(404)
                .WithHeader("Content-Type", "application/json")
                .WithJsonBody(new
                {
                    error = "Event not found",
                    message = $"Event with ID {invalidEventId} was not found"
                }));
        
        // Act & Assert
        await _pactBuilder.VerifyAsync(async (context) =>
        {
            var eventService = new EventService(context.MockServerUri);
            
            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                eventService.GetEventAsync(invalidEventId));
            
            Assert.That(exception.Message, Does.Contain("Event not found"));
        });
    }
}
```

### **3. Provider Tests con Pact**

#### **Verificación de Contratos en el Provider**
```csharp
public class EventServiceProviderTests
{
    private readonly string _pactDir = "./pacts";
    private readonly string _providerName = "EventService";
    
    [Fact]
    public async Task EventService_ShouldHonorContractWithMusicianService()
    {
        // Arrange
        var config = new PactVerifierConfig
        {
            Outputters = new List<IOutput>
            {
                new ConsoleOutput()
            },
            LogLevel = PactLogLevel.Information
        };
        
        var verifier = new PactVerifier(config);
        
        // Act & Assert
        verifier
            .ServiceProvider(_providerName, new Uri("https://api.mussikon.com"))
            .WithHttpEndpoint(new Uri("https://api.mussikon.com"))
            .WithPactBrokerSource(new Uri("https://pact-broker.mussikon.com"))
            .WithProviderStateUrl(new Uri("https://api.mussikon.com/provider-states"))
            .Verify();
    }
    
    [Fact]
    public async Task EventService_ShouldHonorContractWithOrganizerService()
    {
        // Arrange
        var config = new PactVerifierConfig
        {
            Outputters = new List<IOutput>
            {
                new ConsoleOutput()
            },
            LogLevel = PactLogLevel.Information
        };
        
        var verifier = new PactVerifier(config);
        
        // Act & Assert
        verifier
            .ServiceProvider(_providerName, new Uri("https://api.mussikon.com"))
            .WithHttpEndpoint(new Uri("https://api.mussikon.com"))
            .WithPactBrokerSource(new Uri("https://pact-broker.mussikon.com"))
            .WithProviderStateUrl(new Uri("https://api.mussikon.com/provider-states"))
            .Verify();
    }
}
```

#### **Provider States**
```csharp
// Program.cs - Configurar provider states
app.MapPost("/provider-states", async (ProviderStateRequest request) =>
{
    switch (request.State)
    {
        case "Event with ID 123 exists":
            // Configurar estado para que el evento 123 exista
            await _eventRepository.CreateEventAsync(new Event
            {
                Id = 123,
                Title = "Rock Concert",
                Description = "Amazing rock concert",
                Genre = "Rock",
                Location = new Location("Madrid", "Spain"),
                Date = DateTime.Parse("2024-06-15T20:00:00Z"),
                Budget = 1500.00m,
                Status = EventStatus.Active
            });
            break;
            
        case "Event with ID 999999 does not exist":
            // Asegurar que el evento 999999 no existe
            await _eventRepository.DeleteEventAsync(999999);
            break;
            
        case "Events exist matching the criteria":
            // Configurar eventos que coincidan con los criterios de búsqueda
            await _eventRepository.CreateEventAsync(new Event
            {
                Id = 123,
                Title = "Rock Concert",
                Genre = "Rock",
                Location = new Location("Madrid", "Spain"),
                Budget = 1500.00m,
                Date = DateTime.Parse("2024-06-15T20:00:00Z")
            });
            break;
    }
    
    return Results.Ok();
});
```

### **4. Pact Broker y CI/CD**

#### **Configuración de Pact Broker**
```yaml
# docker-compose.yml
version: '3.8'
services:
  pact-broker:
    image: pactfoundation/pact-broker:latest
    ports:
      - "9292:9292"
    environment:
      PACT_BROKER_DATABASE_URL: postgres://pact_broker:password@postgres:5432/pact_broker
      PACT_BROKER_BASIC_AUTH_USERNAME: admin
      PACT_BROKER_BASIC_AUTH_PASSWORD: password
      PACT_BROKER_PUBLIC_HEARTBEAT: true
    depends_on:
      - postgres
      
  postgres:
    image: postgres:13
    environment:
      POSTGRES_DB: pact_broker
      POSTGRES_USER: pact_broker
      POSTGRES_PASSWORD: password
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```

#### **Integración con CI/CD**
```yaml
# .github/workflows/contract-testing.yml
name: Contract Testing

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  consumer-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
          
      - name: Restore dependencies
        run: dotnet restore
        
      - name: Run consumer tests
        run: dotnet test --filter "Category=Contract" --logger "trx;LogFileName=consumer-tests.trx"
        
      - name: Publish pacts
        run: |
          dotnet run --project MusicianService --publish-pacts
          dotnet run --project OrganizerService --publish-pacts
        env:
          PACT_BROKER_BASE_URL: https://pact-broker.mussikon.com
          PACT_BROKER_TOKEN: ${{ secrets.PACT_BROKER_TOKEN }}
          
  provider-tests:
    runs-on: ubuntu-latest
    needs: consumer-tests
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
          
      - name: Start EventService
        run: |
          docker-compose up -d
          dotnet run --project EventService
        env:
          ASPNETCORE_ENVIRONMENT: Testing
          
      - name: Run provider tests
        run: dotnet test --filter "Category=Provider" --logger "trx;LogFileName=provider-tests.trx"
        
      - name: Verify contracts
        run: dotnet run --project EventService --verify-contracts
        env:
          PACT_BROKER_BASE_URL: https://pact-broker.mussikon.com
          PACT_BROKER_TOKEN: ${{ secrets.PACT_BROKER_TOKEN }}
```

## 🛠️ **Ejercicios Prácticos**

### **Ejercicio 1: Implementar Contract Testing para MussikOn**
```csharp
// Implementar contracts para:
// 1. MusicianService ↔ EventService
// 2. OrganizerService ↔ PaymentService
// 3. ChatService ↔ NotificationService
// 4. UserService ↔ AuthService

[Fact]
public async Task MusicianService_ShouldHaveContractWithEventService()
{
    // TODO: Implementar contract test
}

[Fact]
public async Task OrganizerService_ShouldHaveContractWithPaymentService()
{
    // TODO: Implementar contract test
}
```

### **Ejercicio 2: Configurar Pact Broker**
```yaml
# Configurar Pact Broker para MussikOn:
# 1. Docker Compose
# 2. CI/CD integration
# 3. Webhook notifications
# 4. Contract verification

version: '3.8'
services:
  pact-broker:
    image: pactfoundation/pact-broker:latest
    ports:
      - "9292:9292"
    environment:
      # TODO: Configurar variables de entorno
    depends_on:
      - postgres
```

### **Ejercicio 3: Implementar Provider States**
```csharp
// Implementar provider states para:
// 1. Eventos existentes
// 2. Músicos disponibles
// 3. Conversaciones activas
// 4. Pagos procesados

app.MapPost("/provider-states", async (ProviderStateRequest request) =>
{
    // TODO: Implementar provider states
});
```

## 🎯 **Objetivos de Aprendizaje**

Al finalizar esta clase, serás capaz de:

1. **Implementar Contract Testing** con Pact
2. **Crear Consumer-Driven Contracts** para microservicios
3. **Configurar Provider Tests** y verificación de contratos
4. **Integrar Pact Broker** en CI/CD
5. **Aplicar Contract Testing** en arquitecturas complejas

## 📝 **Resumen**

En esta clase hemos cubierto:

- **Contract Testing**: Fundamentos y beneficios
- **Consumer-Driven Contracts**: Definición de expectativas
- **Pact Implementation**: Consumer y Provider tests
- **Provider States**: Configuración de estados
- **Pact Broker**: Gestión centralizada de contratos

## 🚀 **Siguiente Clase**

En la próxima clase exploraremos **Mutation Testing** para evaluar la calidad de nuestros tests y asegurar que cubran todos los casos edge.

---

**💡 Tip**: Los contratos son la documentación viva de tu API. Manténlos actualizados y verifícalos en cada deploy.
