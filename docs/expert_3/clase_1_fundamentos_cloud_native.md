# 🚀 **Clase 1: Fundamentos de Cloud Native Development**

## 🎯 **Objetivo de la Clase**
Comprender los principios fundamentales del desarrollo nativo de la nube, arquitecturas modernas y patrones de diseño para aplicaciones escalables y resilientes.

## 📚 **Contenido Teórico**

### **1. ¿Qué es Cloud Native?**

**Cloud Native** es un enfoque para construir y ejecutar aplicaciones que aprovechan al máximo las ventajas del modelo de computación en la nube.

#### **Características Principales:**
- **Escalabilidad automática**
- **Resiliencia y tolerancia a fallos**
- **Despliegue continuo**
- **Observabilidad**
- **Gestión de estado distribuido**

### **2. Los 12 Factores de la Aplicación**

#### **I. Código Base**
```csharp
// Un solo repositorio para múltiples despliegues
// Estructura del proyecto
MussikOn/
├── src/
│   ├── MussikOn.API/
│   ├── MussikOn.Core/
│   ├── MussikOn.Infrastructure/
│   └── MussikOn.Web/
├── tests/
├── docker/
└── k8s/
```

#### **II. Dependencias**
```csharp
// Declarar explícitamente las dependencias
// MussikOn.API.csproj
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
  </ItemGroup>
</Project>
```

#### **III. Configuración**
```csharp
// Configuración a través de variables de entorno
public class DatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}

// Program.cs
var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("DatabaseSettings"));
```

### **3. Patrones de Arquitectura Cloud Native**

#### **Microservicios**
```csharp
// Servicio de Usuarios
public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IEventBus _eventBus;
    
    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        var user = new User(request.Email, request.Name);
        await _userRepository.AddAsync(user);
        
        // Publicar evento
        await _eventBus.PublishAsync(new UserCreatedEvent(user.Id, user.Email));
        
        return user;
    }
}

// Servicio de Eventos
public class EventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IEventBus _eventBus;
    
    public async Task<Event> CreateEventAsync(CreateEventRequest request)
    {
        var eventEntity = new Event(request.Title, request.Date, request.Location);
        await _eventRepository.AddAsync(eventEntity);
        
        // Publicar evento
        await _eventBus.PublishAsync(new EventCreatedEvent(eventEntity.Id, eventEntity.Title));
        
        return eventEntity;
    }
}
```

#### **Event Sourcing**
```csharp
// Event Store
public interface IEventStore
{
    Task SaveEventsAsync(Guid aggregateId, IEnumerable<IDomainEvent> events, int expectedVersion);
    Task<IEnumerable<IDomainEvent>> GetEventsAsync(Guid aggregateId);
}

// Implementación
public class EventStore : IEventStore
{
    private readonly IEventRepository _eventRepository;
    
    public async Task SaveEventsAsync(Guid aggregateId, IEnumerable<IDomainEvent> events, int expectedVersion)
    {
        var eventEntities = events.Select(e => new EventEntity
        {
            AggregateId = aggregateId,
            EventType = e.GetType().Name,
            EventData = JsonSerializer.Serialize(e),
            Version = expectedVersion + 1,
            Timestamp = DateTime.UtcNow
        });
        
        await _eventRepository.AddRangeAsync(eventEntities);
    }
    
    public async Task<IEnumerable<IDomainEvent>> GetEventsAsync(Guid aggregateId)
    {
        var events = await _eventRepository.GetByAggregateIdAsync(aggregateId);
        return events.Select(e => JsonSerializer.Deserialize<IDomainEvent>(e.EventData));
    }
}
```

### **4. Containerización con Docker**

#### **Dockerfile para .NET**
```dockerfile
# Dockerfile para MussikOn API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MussikOn.API/MussikOn.API.csproj", "MussikOn.API/"]
COPY ["MussikOn.Core/MussikOn.Core.csproj", "MussikOn.Core/"]
COPY ["MussikOn.Infrastructure/MussikOn.Infrastructure.csproj", "MussikOn.Infrastructure/"]
RUN dotnet restore "MussikOn.API/MussikOn.API.csproj"
COPY . .
WORKDIR "/src/MussikOn.API"
RUN dotnet build "MussikOn.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MussikOn.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MussikOn.API.dll"]
```

#### **Docker Compose**
```yaml
# docker-compose.yml
version: '3.8'

services:
  mussikon-api:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=db;Database=MussikOn;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;
    depends_on:
      - db
      - redis

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword123!
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

volumes:
  sqlserver_data:
```

### **5. Orquestación con Kubernetes**

#### **Deployment**
```yaml
# k8s/deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: mussikon-api
  labels:
    app: mussikon-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: mussikon-api
  template:
    metadata:
      labels:
        app: mussikon-api
    spec:
      containers:
      - name: mussikon-api
        image: mussikon/api:latest
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: mussikon-secrets
              key: connection-string
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 5
```

#### **Service**
```yaml
# k8s/service.yaml
apiVersion: v1
kind: Service
metadata:
  name: mussikon-api-service
spec:
  selector:
    app: mussikon-api
  ports:
  - protocol: TCP
    port: 80
    targetPort: 80
  type: LoadBalancer
```

### **6. Health Checks**

```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString)
    .AddRedis(redisConnectionString)
    .AddCheck<ExternalApiHealthCheck>("external-api");

// HealthCheck personalizado
public class ExternalApiHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    
    public ExternalApiHealthCheck(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/health", cancellationToken);
            return response.IsSuccessStatusCode 
                ? HealthCheckResult.Healthy("External API is responding")
                : HealthCheckResult.Unhealthy("External API is not responding");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("External API check failed", ex);
        }
    }
}
```

## 🛠️ **Ejercicio Práctico**

### **Ejercicio 1: Configuración Cloud Native**

Crea un proyecto .NET con configuración cloud native:

```csharp
// 1. Crear configuración para diferentes entornos
public class CloudNativeSettings
{
    public DatabaseSettings Database { get; set; } = new();
    public CacheSettings Cache { get; set; } = new();
    public LoggingSettings Logging { get; set; } = new();
    public MonitoringSettings Monitoring { get; set; } = new();
}

// 2. Implementar health checks
public class MussikOnHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        // Verificar base de datos
        // Verificar cache
        // Verificar servicios externos
        return HealthCheckResult.Healthy();
    }
}

// 3. Configurar logging estructurado
public class StructuredLogger
{
    private readonly ILogger<StructuredLogger> _logger;
    
    public void LogUserAction(string userId, string action, object data)
    {
        _logger.LogInformation("User {UserId} performed {Action} with data {@Data}", 
            userId, action, data);
    }
}
```

## 📋 **Resumen de la Clase**

### **Conceptos Clave:**
- **Cloud Native**: Aplicaciones diseñadas para la nube
- **12 Factores**: Principios para aplicaciones escalables
- **Microservicios**: Arquitectura distribuida
- **Event Sourcing**: Almacenamiento de eventos
- **Containerización**: Docker y Kubernetes
- **Health Checks**: Monitoreo de salud

### **Próxima Clase:**
**Azure Services y .NET Integration** - Integración con servicios de Azure

## 🎯 **Objetivos de Aprendizaje**

Al finalizar esta clase, serás capaz de:
- ✅ Comprender los principios del desarrollo cloud native
- ✅ Aplicar los 12 factores de la aplicación
- ✅ Diseñar arquitecturas de microservicios
- ✅ Implementar event sourcing
- ✅ Containerizar aplicaciones .NET
- ✅ Configurar orquestación con Kubernetes
- ✅ Implementar health checks
