# 🏆 Senior Level 8: Sistemas Avanzados y Distribuidos

## 🧭 Navegación del Curso

- **⬅️ Anterior**: [Módulo 14: Plataformas Empresariales](../senior_7/README.md)
- **➡️ Siguiente**: [Módulo 16: Maestría Total](../senior_9/README.md)
- **📚 [Índice Completo](../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivo del Nivel**
Implementar y desplegar en producción una plataforma completa como **MussikOn**, incluyendo CI/CD, monitoreo, escalabilidad y mantenimiento en producción.

---

## 📚 **Contenido Teórico**

### **🏗️ Implementación Práctica de la Arquitectura**

#### **Estructura Completa del Proyecto**
```bash
MusicalMatchingPlatform/
├── src/
│   ├── MusicalMatching.API/                    # Web API principal
│   │   ├── Controllers/                        # Controladores de la API
│   │   ├── Middleware/                         # Middleware personalizado
│   │   ├── DTOs/                               # Data Transfer Objects
│   │   ├── Extensions/                         # Extensiones de configuración
│   │   ├── Program.cs                          # Punto de entrada
│   │   └── appsettings.json                    # Configuración
│   ├── MusicalMatching.Domain/                 # Capa de dominio
│   │   ├── Entities/                           # Entidades del dominio
│   │   ├── Interfaces/                         # Contratos del dominio
│   │   ├── Services/                           # Servicios de dominio
│   │   ├── Events/                             # Eventos de dominio
│   │   ├── ValueObjects/                       # Objetos de valor
│   │   └── Exceptions/                         # Excepciones personalizadas
│   ├── MusicalMatching.Application/             # Capa de aplicación
│   │   ├── Services/                           # Servicios de aplicación
│   │   ├── Validators/                         # Validadores con FluentValidation
│   │   ├── AutoMapper/                         # Perfiles de mapeo
│   │   ├── Commands/                           # Comandos CQRS
│   │   ├── Queries/                            # Consultas CQRS
│   │   └── Handlers/                           # Manejadores CQRS
│   └── MusicalMatching.Infrastructure/         # Capa de infraestructura
│       ├── Data/                               # Acceso a datos
│       ├── Repositories/                       # Implementaciones de repositorios
│       ├── External/                           # Servicios externos
│       ├── Configuration/                      # Configuración de servicios
│       └── Hubs/                               # SignalR Hubs
├── tests/
│   ├── MusicalMatching.UnitTests/              # Tests unitarios
│   ├── MusicalMatching.IntegrationTests/       # Tests de integración
│   └── MusicalMatching.E2ETests/               # Tests end-to-end
├── docs/                                        # Documentación
├── scripts/                                     # Scripts de deployment
├── docker/                                      # Configuración Docker
├── k8s/                                         # Configuración Kubernetes
└── .github/                                     # GitHub Actions
    └── workflows/                               # Pipelines de CI/CD
```

#### **Configuración de Dependencias**
```xml
<!-- MusicalMatching.API.csproj -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <!-- Core Framework -->
    <PackageReference Include="Microsoft.AspNetCore.App" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
    
    <!-- Identity y Autenticación -->
    <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
    <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.3.1" />
    
    <!-- SignalR -->
    <PackageReference Include="Microsoft.AspNetCore.SignalR" Version="1.1.0" />
    
    <!-- Validación y Mapping -->
    <PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
    <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
    
    <!-- Logging y Monitoreo -->
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="5.0.0" />
    <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
    
    <!-- Health Checks -->
    <PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="AspNetCore.HealthChecks.Redis" Version="8.0.0" />
    
    <!-- Swagger -->
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
    <PackageReference Include="Swashbuckle.AspNetCore.Annotations" Version="6.5.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\MusicalMatching.Domain\MusicalMatching.Domain.csproj" />
    <ProjectReference Include="..\MusicalMatching.Application\MusicalMatching.Application.csproj" />
    <ProjectReference Include="..\MusicalMatching.Infrastructure\MusicalMatching.Infrastructure.csproj" />
  </ItemGroup>
</Project>
```

#### **Program.cs Configurado**
```csharp
using MusicalMatching.Application;
using MusicalMatching.Infrastructure;
using MusicalMatching.API.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/musical-matching-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting Musical Matching Platform API");

    // Agregar servicios
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);
    
    // Configurar CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigins", policy =>
        {
            policy.WithOrigins(
                    "http://localhost:3000",      // Frontend React
                    "http://localhost:19006",     // Expo
                    "https://musicalmatching.com" // Producción
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });

    // Configurar SignalR
    builder.Services.AddSignalR(options =>
    {
        options.EnableDetailedErrors = builder.Environment.IsDevelopment();
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
        options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    });

    // Configurar Health Checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>()
        .AddRedis(builder.Configuration.GetConnectionString("Redis"))
        .AddUrlGroup(new Uri("https://api.stripe.com"), "Stripe API");

    var app = builder.Build();

    // Configurar pipeline de middleware
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Musical Matching API v1");
            c.RoutePrefix = string.Empty; // Swagger en la raíz
        });
    }

    app.UseMiddleware<ExceptionMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();
    app.UseMiddleware<PerformanceMiddleware>();

    app.UseHttpsRedirection();
    app.UseCors("AllowSpecificOrigins");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHub<NotificationHub>("/hubs/notifications");
    app.MapHub<ChatHub>("/hubs/chat");
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
```

### **🐳 Containerización y Docker**

#### **Dockerfile Multi-Stage**
```dockerfile
# Multi-stage build para optimizar la imagen final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Instalar dependencias del sistema
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["MusicalMatching.API/MusicalMatching.API.csproj", "MusicalMatching.API/"]
COPY ["MusicalMatching.Application/MusicalMatching.Application.csproj", "MusicalMatching.Application/"]
COPY ["MusicalMatching.Domain/MusicalMatching.Domain.csproj", "MusicalMatching.Domain/"]
COPY ["MusicalMatching.Infrastructure/MusicalMatching.Infrastructure.csproj", "MusicalMatching.Infrastructure/"]

# Restaurar dependencias
RUN dotnet restore "MusicalMatching.API/MusicalMatching.API.csproj"

# Copiar código fuente
COPY . .

# Build de la aplicación
WORKDIR "/src/MusicalMatching.API"
RUN dotnet build "MusicalMatching.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MusicalMatching.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Crear usuario no-root para seguridad
RUN groupadd -r appuser && useradd -r -g appuser appuser

COPY --from=publish /app/publish .

# Cambiar ownership a usuario no-root
RUN chown -R appuser:appuser /app
USER appuser

ENTRYPOINT ["dotnet", "MusicalMatching.API.dll"]
```

#### **Docker Compose para Desarrollo**
```yaml
# docker-compose.yml
version: '3.8'

services:
  api:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "5000:80"
      - "5001:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=db;Database=MusicalMatching;User=sa;Password=Your_password123!;TrustServerCertificate=true
      - ConnectionStrings__Redis=redis:6379
    depends_on:
      - db
      - redis
    volumes:
      - ./logs:/app/logs
    networks:
      - musical-network

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=Your_password123!
      - MSSQL_PID=Developer
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    networks:
      - musical-network

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    networks:
      - musical-network

  adminer:
    image: adminer
    ports:
      - "8080:8080"
    depends_on:
      - db
    networks:
      - musical-network

volumes:
  sqlserver_data:
  redis_data:

networks:
  musical-network:
    driver: bridge
```

#### **Docker Compose para Producción**
```yaml
# docker-compose.prod.yml
version: '3.8'

services:
  api:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "80:80"
      - "443:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}
      - ConnectionStrings__Redis=${REDIS_CONNECTION_STRING}
      - JwtSettings__SecretKey=${JWT_SECRET_KEY}
    depends_on:
      - db
      - redis
    volumes:
      - ./logs:/app/logs
      - ./ssl:/app/ssl:ro
    networks:
      - musical-network
    restart: unless-stopped
    deploy:
      replicas: 3
      resources:
        limits:
          cpus: '1.0'
          memory: 1G
        reservations:
          cpus: '0.5'
          memory: 512M

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=${DB_PASSWORD}
      - MSSQL_PID=Enterprise
    volumes:
      - sqlserver_data:/var/opt/mssql
      - ./backups:/var/opt/mssql/backup
    networks:
      - musical-network
    restart: unless-stopped
    deploy:
      resources:
        limits:
          cpus: '2.0'
          memory: 4G

  redis:
    image: redis:7-alpine
    command: redis-server --appendonly yes --requirepass ${REDIS_PASSWORD}
    volumes:
      - redis_data:/data
    networks:
      - musical-network
    restart: unless-stopped
    deploy:
      resources:
        limits:
          cpus: '0.5'
          memory: 512M

  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx/nginx.conf:/etc/nginx/nginx.conf:ro
      - ./ssl:/etc/nginx/ssl:ro
    depends_on:
      - api
    networks:
      - musical-network
    restart: unless-stopped

volumes:
  sqlserver_data:
  redis_data:

networks:
  musical-network:
    driver: bridge
```

### **🚀 CI/CD con GitHub Actions**

#### **Pipeline de Build y Test**
```yaml
# .github/workflows/build-and-test.yml
name: Build and Test

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  build:
    runs-on: ubuntu-latest
    
    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          ACCEPT_EULA: Y
          SA_PASSWORD: Your_password123!
        options: >-
          --health-cmd "/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Your_password123! -Q 'SELECT 1'"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 1433:1433

    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: 8.0.x
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore --configuration Release
    
    - name: Run unit tests
      run: dotnet test --no-build --verbosity normal --configuration Release --collect:"XPlat Code Coverage" --results-directory ./coverage
    
    - name: Run integration tests
      run: dotnet test tests/MusicalMatching.IntegrationTests --no-build --verbosity normal --configuration Release
    
    - name: Upload coverage to Codecov
      uses: codecov/codecov-action@v3
      with:
        file: ./coverage/coverage.cobertura.xml
        flags: unittests
        name: codecov-umbrella
        fail_ci_if_error: false

  security-scan:
    runs-on: ubuntu-latest
    needs: build
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Run Snyk to check for vulnerabilities
      uses: snyk/actions/dotnet@master
      env:
        SNYK_TOKEN: ${{ secrets.SNYK_TOKEN }}
      with:
        args: --severity-threshold=high

  docker-build:
    runs-on: ubuntu-latest
    needs: [build, security-scan]
    if: github.ref == 'refs/heads/main'
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Set up Docker Buildx
      uses: docker/setup-buildx-action@v3
    
    - name: Login to Container Registry
      uses: docker/login-action@v3
      with:
        registry: ghcr.io
        username: ${{ github.actor }}
        password: ${{ secrets.GITHUB_TOKEN }}
    
    - name: Build and push Docker image
      uses: docker/build-push-action@v5
      with:
        context: .
        push: true
        tags: |
          ghcr.io/${{ github.repository }}:latest
          ghcr.io/${{ github.repository }}:${{ github.sha }}
        cache-from: type=gha
        cache-to: type=gha,mode=max
```

#### **Pipeline de Deployment**
```yaml
# .github/workflows/deploy.yml
name: Deploy to Production

on:
  workflow_run:
    workflows: ["Build and Test"]
    types:
      - completed
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    if: ${{ github.event.workflow_run.conclusion == 'success' }}
    
    environment: production
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Deploy to Azure
      uses: azure/webapps-deploy-action@v2
      with:
        app-name: 'musical-matching-api'
        package: .
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
    
    - name: Deploy to Kubernetes
      uses: azure/k8s-deploy-action@v1
      with:
        manifests: |
          k8s/deployment.yml
          k8s/service.yml
          k8s/ingress.yml
        images: |
          ghcr.io/${{ github.repository }}:${{ github.sha }}
        namespace: musical-matching
        kubectl-version: 'latest'
    
    - name: Run smoke tests
      run: |
        # Esperar a que la aplicación esté lista
        sleep 30
        
        # Ejecutar tests de humo
        curl -f http://musical-matching-api.azurewebsites.net/health || exit 1
    
    - name: Notify deployment status
      uses: 8398a7/action-slack@v3
      with:
        status: ${{ job.status }}
        channel: '#deployments'
        webhook_url: ${{ secrets.SLACK_WEBHOOK_URL }}
```

### **☸️ Kubernetes Deployment**

#### **Deployment Principal**
```yaml
# k8s/deployment.yml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: musical-matching-api
  namespace: musical-matching
  labels:
    app: musical-matching-api
    version: v1.0.0
spec:
  replicas: 3
  selector:
    matchLabels:
      app: musical-matching-api
  template:
    metadata:
      labels:
        app: musical-matching-api
        version: v1.0.0
    spec:
      containers:
      - name: api
        image: ghcr.io/your-org/musical-matching:latest
        ports:
        - containerPort: 80
          name: http
        - containerPort: 443
          name: https
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: db-secret
              key: connection-string
        - name: JwtSettings__SecretKey
          valueFrom:
            secretKeyRef:
              name: jwt-secret
              key: secret-key
        resources:
          requests:
            memory: "512Mi"
            cpu: "500m"
          limits:
            memory: "1Gi"
            cpu: "1000m"
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 3
        volumeMounts:
        - name: logs
          mountPath: /app/logs
      volumes:
      - name: logs
        emptyDir: {}
      imagePullSecrets:
      - name: ghcr-secret
```

#### **Service y Ingress**
```yaml
# k8s/service.yml
apiVersion: v1
kind: Service
metadata:
  name: musical-matching-api-service
  namespace: musical-matching
spec:
  selector:
    app: musical-matching-api
  ports:
  - name: http
    port: 80
    targetPort: 80
  - name: https
    port: 443
    targetPort: 443
  type: ClusterIP

---
# k8s/ingress.yml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: musical-matching-ingress
  namespace: musical-matching
  annotations:
    kubernetes.io/ingress.class: "nginx"
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
    nginx.ingress.kubernetes.io/force-ssl-redirect: "true"
spec:
  tls:
  - hosts:
    - api.musicalmatching.com
    secretName: musical-matching-tls
  rules:
  - host: api.musicalmatching.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: musical-matching-api-service
            port:
              number: 80
```

### **📊 Monitoreo y Observabilidad**

#### **Health Checks Personalizados**
```csharp
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(ApplicationDbContext context, ILogger<DatabaseHealthCheck> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verificar conexión a base de datos
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            if (!canConnect)
            {
                _logger.LogWarning("Database health check failed: Cannot connect to database");
                return HealthCheckResult.Unhealthy("Cannot connect to database");
            }

            // Verificar que las tablas principales existen
            var tableExists = await _context.Database.SqlQueryRaw<int>("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MusicianRequests'").FirstOrDefaultAsync(cancellationToken);
            if (tableExists == 0)
            {
                _logger.LogWarning("Database health check failed: Required tables do not exist");
                return HealthCheckResult.Unhealthy("Required tables do not exist");
            }

            // Verificar performance de consulta simple
            var stopwatch = Stopwatch.StartNew();
            var requestCount = await _context.MusicianRequests.CountAsync(cancellationToken);
            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning("Database health check warning: Slow query performance ({ElapsedMs}ms)", stopwatch.ElapsedMilliseconds);
                return HealthCheckResult.Degraded($"Slow query performance: {stopwatch.ElapsedMilliseconds}ms");
            }

            _logger.LogInformation("Database health check passed: {RequestCount} requests found in {ElapsedMs}ms", requestCount, stopwatch.ElapsedMilliseconds);
            return HealthCheckResult.Healthy($"Database is healthy. {requestCount} requests found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed with exception");
            return HealthCheckResult.Unhealthy("Database health check failed", ex);
        }
    }
}

public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisHealthCheck> _logger;

    public RedisHealthCheck(IConnectionMultiplexer redis, ILogger<RedisHealthCheck> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var result = await db.PingAsync();
            
            if (result.TotalMilliseconds > 100)
            {
                _logger.LogWarning("Redis health check warning: Slow response time ({ElapsedMs}ms)", result.TotalMilliseconds);
                return HealthCheckResult.Degraded($"Slow response time: {result.TotalMilliseconds}ms");
            }

            _logger.LogInformation("Redis health check passed: Response time {ElapsedMs}ms", result.TotalMilliseconds);
            return HealthCheckResult.Healthy($"Redis is healthy. Response time: {result.TotalMilliseconds}ms");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed with exception");
            return HealthCheckResult.Unhealthy("Redis health check failed", ex);
        }
    }
}
```

#### **Métricas Personalizadas con Prometheus**
```csharp
public class MetricsService
{
    private readonly Counter _requestsTotal;
    private readonly Histogram _requestDuration;
    private readonly Gauge _activeConnections;
    private readonly Counter _musicianMatchesTotal;
    private readonly Histogram _matchingDuration;

    public MetricsService()
    {
        var factory = Metrics.DefaultFactory;
        
        _requestsTotal = factory.CreateCounter("musical_matching_requests_total", "Total number of requests", "endpoint", "method", "status");
        _requestDuration = factory.CreateHistogram("musical_matching_request_duration_seconds", "Request duration in seconds", "endpoint", "method");
        _activeConnections = factory.CreateGauge("musical_matching_active_connections", "Number of active SignalR connections");
        _musicianMatchesTotal = factory.CreateCounter("musical_matching_matches_total", "Total number of musician matches", "instrument", "location");
        _matchingDuration = factory.CreateHistogram("musical_matching_matching_duration_seconds", "Matching algorithm duration in seconds");
    }

    public void RecordRequest(string endpoint, string method, int statusCode)
    {
        _requestsTotal.Add(1, endpoint, method, statusCode.ToString());
    }

    public void RecordRequestDuration(string endpoint, string method, TimeSpan duration)
    {
        _requestDuration.Record(duration.TotalSeconds, endpoint, method);
    }

    public void SetActiveConnections(int count)
    {
        _activeConnections.Set(count);
    }

    public void RecordMusicianMatch(string instrument, string location)
    {
        _musicianMatchesTotal.Add(1, instrument, location);
    }

    public void RecordMatchingDuration(TimeSpan duration)
    {
        _matchingDuration.Record(duration.TotalSeconds);
    }
}
```

---

## 🧪 **Ejercicios Prácticos**

### **Ejercicio 1: Configuración Completa del Proyecto**
```bash
# Crea la estructura completa del proyecto:
# - Solución .NET con múltiples proyectos
# - Configuración de dependencias
# - Program.cs configurado
# - appsettings.json para diferentes entornos
```

### **Ejercicio 2: Dockerización Completa**
```dockerfile
# Implementa:
# - Dockerfile multi-stage optimizado
# - Docker Compose para desarrollo
# - Docker Compose para producción
# - Scripts de build y deployment
```

### **Ejercicio 3: Pipeline de CI/CD**
```yaml
# Crea pipelines de GitHub Actions para:
# - Build y testing automático
# - Análisis de seguridad
# - Build de Docker images
# - Deployment automático
```

### **Ejercicio 4: Kubernetes Deployment**
```yaml
# Implementa:
# - Deployment con health checks
# - Service y Ingress
# - ConfigMaps y Secrets
# - HPA (Horizontal Pod Autoscaler)
```

### **Ejercicio 5: Monitoreo y Observabilidad**
```csharp
// Implementa:
// - Health checks personalizados
// - Métricas con Prometheus
// - Logging estructurado
// - Distributed tracing
```

### **Ejercicio 6: Performance y Escalabilidad**
```csharp
// Optimiza:
// - Caching con Redis
// - Database connection pooling
// - Async/await patterns
// - Background services
```

### **Ejercicio 7: Seguridad en Producción**
```csharp
// Implementa:
// - Rate limiting
// - Input validation
// - CORS policies
// - Security headers
```

### **Ejercicio 8: Backup y Disaster Recovery**
```bash
# Configura:
# - Backup automático de base de datos
# - Backup de archivos de configuración
# - Scripts de restore
# - Plan de disaster recovery
```

### **Ejercicio 9: Testing en Producción**
```csharp
// Implementa:
// - Smoke tests
// - Load testing
// - Chaos engineering
// - A/B testing
```

### **Ejercicio 10: Proyecto Integrador: Deployment Completo**
```bash
# Despliega la plataforma completa:
# - Build y testing automático
# - Containerización
# - Deployment en Kubernetes
# - Monitoreo y alertas
# - CI/CD pipeline completo
```

---

## 📊 **Proyecto Integrador: Deployment de Producción**

### **🎯 Objetivo**
Desplegar en producción una plataforma completa de matching musical con CI/CD, monitoreo y escalabilidad.

### **🏗️ Arquitectura de Deployment**
```
GitHub Repository
       ↓
GitHub Actions (CI/CD)
       ↓
Docker Registry
       ↓
Kubernetes Cluster
       ↓
Load Balancer
       ↓
API Instances (3+ replicas)
       ↓
Database + Redis
```

### **📋 Funcionalidades de Deployment**
1. **CI/CD Pipeline**: Build, test y deployment automático
2. **Containerización**: Docker multi-stage optimizado
3. **Orquestación**: Kubernetes con auto-scaling
4. **Monitoreo**: Health checks, métricas y alertas
5. **Seguridad**: HTTPS, rate limiting, validaciones
6. **Backup**: Base de datos y configuración
7. **Escalabilidad**: HPA y load balancing
8. **Observabilidad**: Logging, tracing y métricas

### **🔧 Tecnologías de Deployment**
- **GitHub Actions** para CI/CD
- **Docker** para containerización
- **Kubernetes** para orquestación
- **Azure/AWS** para infraestructura
- **Prometheus** + **Grafana** para monitoreo
- **Nginx** para reverse proxy
- **Let's Encrypt** para certificados SSL

### **📈 Métricas de Éxito**
- **Uptime**: 99.9%+
- **Response Time**: < 200ms (p95)
- **Throughput**: 1000+ requests/second
- **Error Rate**: < 0.1%
- **Deployment Time**: < 10 minutos
- **Rollback Time**: < 5 minutos

---

## 🎯 **Evaluación y Certificación**

### **📝 Autoevaluación**
- [ ] ¿Puedes configurar un proyecto .NET completo con Clean Architecture?
- [ ] ¿Sabes crear Dockerfiles multi-stage optimizados?
- [ ] ¿Puedes implementar pipelines de CI/CD con GitHub Actions?
- [ ] ¿Entiendes cómo desplegar en Kubernetes?
- [ ] ¿Sabes configurar monitoreo y observabilidad?
- [ ] ¿Puedes optimizar performance y escalabilidad?
- [ ] ¿Implementas seguridad en producción?
- [ ] ¿Configuras backup y disaster recovery?
- [ ] ¿Realizas testing en producción?
- [ ] ¿Has desplegado la plataforma completa?

### **🏆 Criterios de Aprobación**
- **Deployment exitoso** en producción
- **CI/CD pipeline** funcionando automáticamente
- **Monitoreo activo** con métricas y alertas
- **Performance optimizada** según métricas
- **Seguridad implementada** y validada
- **Documentación completa** del deployment

---

## 🚀 **Próximos Pasos**

1. **Implementa** el deployment completo paso a paso
2. **Configura** el pipeline de CI/CD
3. **Despliega** en un entorno de producción
4. **Monitorea** y optimiza el rendimiento
5. **Documenta** todo el proceso de deployment

---

## 📚 **Recursos Adicionales**

### **🔗 Enlaces Útiles**
- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [Azure DevOps](https://azure.microsoft.com/en-us/services/devops/)

### **📖 Libros Recomendados**
- "Kubernetes: Up and Running" - Kelsey Hightower
- "The Phoenix Project" - Gene Kim
- "Site Reliability Engineering" - Google
- "Continuous Delivery" - Jez Humble

---

**¡Ahora tienes todas las habilidades para implementar y desplegar plataformas empresariales en producción! 🚀☸️**
