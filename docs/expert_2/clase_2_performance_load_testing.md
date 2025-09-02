# ⚡ **Clase 2: Performance Testing y Load Testing**

## 🎯 **Objetivos de la Clase**
- Dominar Performance Testing con NBomber
- Implementar Load Testing con Artillery
- Aplicar Stress Testing y Spike Testing
- Optimizar rendimiento de MussikOn

## 📚 **Contenido Teórico**

### **1. Performance Testing con NBomber**

#### **Configuración de NBomber**
```csharp
// Program.cs
using NBomber.CSharp;
using NBomber.Http.CSharp;

var scenario = Scenario.Create("musician_search", async context =>
{
    var request = Http.CreateRequest("GET", "https://api.mussikon.com/musicians/search")
        .WithHeader("Accept", "application/json")
        .WithHeader("Authorization", "Bearer " + context.CorrelationId);

    var response = await Http.Send(request, context);
    
    return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
})
.WithLoadSimulations(
    Simulation.InjectPerSec(rate: 10, during: TimeSpan.FromMinutes(1))
);

NBomberRunner
    .RegisterScenarios(scenario)
    .Run();
```

#### **Testing de Endpoints Críticos**
```csharp
public class MusicianSearchPerformanceTests
{
    [Test]
    public async Task MusicianSearch_UnderNormalLoad_ShouldMeetPerformanceRequirements()
    {
        var scenario = Scenario.Create("musician_search_normal_load", async context =>
        {
            var searchRequest = Http.CreateRequest("GET", "https://api.mussikon.com/musicians/search")
                .WithHeader("Accept", "application/json")
                .WithQueryParam("genre", "Rock")
                .WithQueryParam("location", "Madrid")
                .WithQueryParam("budget", "1000");

            var response = await Http.Send(searchRequest, context);
            
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 50, during: TimeSpan.FromMinutes(2))
        )
        .WithWarmUpDuration(TimeSpan.FromSeconds(30));

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();

        // Assertions
        Assert.That(stats.AllOkCount, Is.GreaterThan(0));
        Assert.That(stats.AllRequestCount, Is.GreaterThan(0));
        Assert.That(stats.AllOkCount / (double)stats.AllRequestCount, Is.GreaterThan(0.95)); // 95% success rate
    }

    [Test]
    public async Task MusicianSearch_UnderHighLoad_ShouldMaintainPerformance()
    {
        var scenario = Scenario.Create("musician_search_high_load", async context =>
        {
            var searchRequest = Http.CreateRequest("GET", "https://api.mussikon.com/musicians/search")
                .WithHeader("Accept", "application/json")
                .WithQueryParam("genre", "Jazz")
                .WithQueryParam("location", "Barcelona")
                .WithQueryParam("budget", "2000");

            var response = await Http.Send(searchRequest, context);
            
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 200, during: TimeSpan.FromMinutes(5))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();

        // Performance assertions
        Assert.That(stats.AllOkCount / (double)stats.AllRequestCount, Is.GreaterThan(0.90)); // 90% success rate
        Assert.That(stats.AllOkCount, Is.GreaterThan(0));
    }
}
```

#### **Testing de Chat en Tiempo Real**
```csharp
public class ChatPerformanceTests
{
    [Test]
    public async Task ChatSystem_WithMultipleUsers_ShouldHandleConcurrentMessages()
    {
        var scenario = Scenario.Create("chat_concurrent_messages", async context =>
        {
            var chatRequest = Http.CreateRequest("POST", "https://api.mussikon.com/chat/messages")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("Authorization", "Bearer " + context.CorrelationId)
                .WithJsonBody(new
                {
                    conversationId = Guid.NewGuid().ToString(),
                    message = $"Test message from {context.CorrelationId}",
                    timestamp = DateTime.UtcNow
                });

            var response = await Http.Send(chatRequest, context);
            
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 100, during: TimeSpan.FromMinutes(3))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();

        Assert.That(stats.AllOkCount / (double)stats.AllRequestCount, Is.GreaterThan(0.95));
    }
}
```

### **2. Load Testing con Artillery**

#### **Configuración de Artillery**
```yaml
# artillery-config.yml
config:
  target: 'https://api.mussikon.com'
  phases:
    - duration: 60
      arrivalRate: 10
      name: "Warm up"
    - duration: 120
      arrivalRate: 50
      name: "Normal load"
    - duration: 60
      arrivalRate: 100
      name: "High load"
    - duration: 30
      arrivalRate: 200
      name: "Peak load"
  defaults:
    headers:
      Content-Type: 'application/json'
      Accept: 'application/json'
  plugins:
    metrics-by-endpoint: {}
    publish-metrics:
      - type: 'influxdb'
        config:
          host: 'localhost'
          port: 8086
          database: 'mussikon_performance'

scenarios:
  - name: "Musician Search Flow"
    weight: 40
    flow:
      - get:
          url: "/musicians/search?genre=Rock&location=Madrid&budget=1000"
          headers:
            Authorization: "Bearer {{ $randomString() }}"
      - think: 2
      - get:
          url: "/musicians/{{ $randomInt(1, 1000) }}"
      - think: 1
      - post:
          url: "/musicians/{{ $randomInt(1, 1000) }}/applications"
          json:
            eventId: "{{ $randomInt(1, 100) }}"
            message: "I'm interested in this event"
            proposedRate: 800

  - name: "Chat System Flow"
    weight: 30
    flow:
      - post:
          url: "/chat/conversations"
          json:
            participantId: "{{ $randomInt(1, 1000) }}"
      - think: 1
      - post:
          url: "/chat/messages"
          json:
            conversationId: "{{ $randomString() }}"
            message: "Hello, are you available?"
            timestamp: "{{ $isoTimestamp() }}"
      - think: 3
      - get:
          url: "/chat/conversations/{{ $randomString() }}/messages"

  - name: "Event Management Flow"
    weight: 30
    flow:
      - post:
          url: "/events"
          json:
            title: "Rock Concert {{ $randomString() }}"
            description: "Amazing rock concert"
            genre: "Rock"
            location: "Madrid, Spain"
            date: "{{ $isoTimestamp() }}"
            budget: 1500
      - think: 2
      - get:
          url: "/events/{{ $randomInt(1, 100) }}"
      - think: 1
      - put:
          url: "/events/{{ $randomInt(1, 100) }}"
          json:
            status: "Active"
```

#### **Scripts de Artillery Personalizados**
```javascript
// artillery-scripts.js
module.exports = {
  generateMusicianData: function(context, events, done) {
    const genres = ['Rock', 'Jazz', 'Classical', 'Pop', 'Electronic'];
    const locations = ['Madrid', 'Barcelona', 'Valencia', 'Sevilla', 'Bilbao'];
    
    context.vars.genre = genres[Math.floor(Math.random() * genres.length)];
    context.vars.location = locations[Math.floor(Math.random() * locations.length)];
    context.vars.budget = Math.floor(Math.random() * 2000) + 500;
    
    return done();
  },
  
  generateChatMessage: function(context, events, done) {
    const messages = [
      "Hello, are you available for this event?",
      "I'm interested in your music style",
      "What's your rate for a 3-hour performance?",
      "Do you have any samples of your work?",
      "When are you available next week?"
    ];
    
    context.vars.message = messages[Math.floor(Math.random() * messages.length)];
    return done();
  }
};
```

### **3. Stress Testing y Spike Testing**

#### **Stress Testing con NBomber**
```csharp
public class StressTesting
{
    [Test]
    public async Task MusicianMatching_StressTest_ShouldIdentifyBreakingPoint()
    {
        var scenario = Scenario.Create("musician_matching_stress", async context =>
        {
            var matchingRequest = Http.CreateRequest("POST", "https://api.mussikon.com/musicians/match")
                .WithHeader("Content-Type", "application/json")
                .WithJsonBody(new
                {
                    eventId = context.CorrelationId,
                    criteria = new
                    {
                        genre = "Rock",
                        location = "Madrid",
                        budget = 1000,
                        date = DateTime.Now.AddDays(30)
                    }
                });

            var response = await Http.Send(matchingRequest, context);
            
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 10, during: TimeSpan.FromMinutes(1)),
            Simulation.InjectPerSec(rate: 50, during: TimeSpan.FromMinutes(2)),
            Simulation.InjectPerSec(rate: 100, during: TimeSpan.FromMinutes(2)),
            Simulation.InjectPerSec(rate: 200, during: TimeSpan.FromMinutes(2)),
            Simulation.InjectPerSec(rate: 500, during: TimeSpan.FromMinutes(1))
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();

        // Analyze results to identify breaking point
        Console.WriteLine($"Total Requests: {stats.AllRequestCount}");
        Console.WriteLine($"Successful Requests: {stats.AllOkCount}");
        Console.WriteLine($"Failed Requests: {stats.AllFailCount}");
        Console.WriteLine($"Success Rate: {stats.AllOkCount / (double)stats.AllRequestCount:P2}");
    }
}
```

#### **Spike Testing**
```csharp
public class SpikeTesting
{
    [Test]
    public async Task MusicianSearch_SpikeTest_ShouldRecoverFromTrafficSpikes()
    {
        var scenario = Scenario.Create("musician_search_spike", async context =>
        {
            var searchRequest = Http.CreateRequest("GET", "https://api.mussikon.com/musicians/search")
                .WithHeader("Accept", "application/json")
                .WithQueryParam("genre", "Rock")
                .WithQueryParam("location", "Madrid");

            var response = await Http.Send(searchRequest, context);
            
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 10, during: TimeSpan.FromMinutes(2)), // Normal load
            Simulation.InjectPerSec(rate: 500, during: TimeSpan.FromSeconds(30)), // Spike
            Simulation.InjectPerSec(rate: 10, during: TimeSpan.FromMinutes(2))  // Recovery
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();

        // Verify system recovers from spike
        Assert.That(stats.AllOkCount, Is.GreaterThan(0));
    }
}
```

### **4. Monitoring y Métricas de Performance**

#### **Integración con Application Insights**
```csharp
public class PerformanceMonitoring
{
    private readonly TelemetryClient _telemetryClient;
    
    public PerformanceMonitoring(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }
    
    public async Task<T> TrackPerformance<T>(string operationName, Func<Task<T>> operation)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var result = await operation();
            
            _telemetryClient.TrackDependency(
                operationName,
                "MussikOn",
                DateTime.UtcNow.Subtract(stopwatch.Elapsed),
                stopwatch.Elapsed,
                true);
            
            return result;
        }
        catch (Exception ex)
        {
            _telemetryClient.TrackDependency(
                operationName,
                "MussikOn",
                DateTime.UtcNow.Subtract(stopwatch.Elapsed),
                stopwatch.Elapsed,
                false);
            
            _telemetryClient.TrackException(ex);
            throw;
        }
        finally
        {
            stopwatch.Stop();
        }
    }
}
```

## 🛠️ **Ejercicios Prácticos**

### **Ejercicio 1: Performance Testing de MusicianMatchingService**
```csharp
// Implementar performance tests para:
// 1. Búsqueda de músicos por criterios
// 2. Algoritmo de matching
// 3. Carga de perfiles de músicos
// 4. Sistema de notificaciones

[Test]
public async Task MusicianMatching_WithComplexCriteria_ShouldMeetPerformanceRequirements()
{
    // TODO: Implementar test de performance
}

[Test]
public async Task MusicianProfile_Loading_ShouldBeUnderThreshold()
{
    // TODO: Implementar test de performance
}
```

### **Ejercicio 2: Load Testing con Artillery**
```yaml
# Crear configuración de Artillery para:
# 1. Flujo completo de usuario (registro → búsqueda → aplicación → chat)
# 2. Testing de chat en tiempo real
# 3. Testing de sistema de pagos
# 4. Testing de notificaciones

config:
  target: 'https://api.mussikon.com'
  phases:
    - duration: 60
      arrivalRate: 20
      name: "Warm up"
    - duration: 180
      arrivalRate: 100
      name: "Sustained load"
    - duration: 60
      arrivalRate: 300
      name: "Peak load"

scenarios:
  - name: "Complete User Journey"
    weight: 50
    flow:
      # TODO: Implementar flujo completo
```

### **Ejercicio 3: Stress Testing de Chat System**
```csharp
[Test]
public async Task ChatSystem_StressTest_ShouldHandleConcurrentUsers()
{
    // TODO: Implementar stress test para chat
    // - 1000 usuarios concurrentes
    // - 10,000 mensajes por minuto
    // - Verificar latencia y throughput
}
```

## 🎯 **Objetivos de Aprendizaje**

Al finalizar esta clase, serás capaz de:

1. **Implementar Performance Testing** con NBomber
2. **Configurar Load Testing** con Artillery
3. **Aplicar Stress Testing** y Spike Testing
4. **Monitorear métricas de performance** en tiempo real
5. **Optimizar rendimiento** basado en resultados de testing

## 📝 **Resumen**

En esta clase hemos cubierto:

- **Performance Testing**: NBomber, métricas de rendimiento
- **Load Testing**: Artillery, simulaciones de carga
- **Stress Testing**: Identificación de puntos de quiebre
- **Spike Testing**: Recuperación de picos de tráfico
- **Monitoring**: Integración con Application Insights

## 🚀 **Siguiente Clase**

En la próxima clase exploraremos **Security Testing** con herramientas como OWASP ZAP y técnicas de penetration testing para asegurar la seguridad de MussikOn.

---

**💡 Tip**: El performance testing no es solo sobre velocidad, es sobre confiabilidad bajo carga. Siempre prueba más allá de los límites esperados.
