# 🎯 Clase 10: Proyecto Final Integrador

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 9: Testing en Producción](../senior_8/clase_9_testing_produccion.md)
- **🏠 Inicio del Módulo**: [Módulo 15: Sistemas Avanzados y Distribuidos](../senior_8/README.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Integrar** todos los conceptos aprendidos en el módulo
2. **Implementar** una plataforma completa de producción
3. **Desplegar** en Kubernetes con CI/CD
4. **Configurar** monitoreo y observabilidad
5. **Implementar** estrategias de backup y disaster recovery

---

## 🏗️ **Arquitectura del Proyecto Final**

### **Estructura del Proyecto**

```
MusicalMatchingPlatform/
├── src/
│   ├── MusicalMatching.API/           # API principal
│   ├── MusicalMatching.Application/    # Lógica de aplicación
│   ├── MusicalMatching.Domain/        # Entidades y reglas de negocio
│   ├── MusicalMatching.Infrastructure/ # Implementaciones técnicas
│   └── MusicalMatching.Tests/         # Tests unitarios e integración
├── infrastructure/
│   ├── docker/                        # Configuración Docker
│   ├── k8s/                          # Manifiestos Kubernetes
│   ├── monitoring/                    # Configuración de monitoreo
│   └── ci-cd/                        # Pipelines CI/CD
├── docs/                              # Documentación
└── scripts/                           # Scripts de automatización
```

---

## 🚀 **Implementación Completa**

### **1. API Principal con Configuración Avanzada**

```csharp
// MusicalMatching.API/Program.cs
using MusicalMatching.API.Configuration;
using MusicalMatching.API.Middleware;
using MusicalMatching.Application.Configuration;
using MusicalMatching.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configuración avanzada
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

// Configuración de monitoreo
builder.Services.AddMonitoring(builder.Configuration);
builder.Services.AddHealthChecks(builder.Configuration);

// Configuración de seguridad
builder.Services.AddSecurity(builder.Configuration);

var app = builder.Build();

// Middleware de pipeline
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Configuración de métricas
app.UseMetrics();
app.UseMetricsEndpoint();

app.Run();
```

### **2. Configuración de Servicios**

```csharp
// MusicalMatching.API/Configuration/ApiServiceConfiguration.cs
namespace MusicalMatching.API.Configuration;

public static class ApiServiceConfiguration
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Configuración de CORS
        services.AddCors(options =>
        {
            options.AddPolicy("ProductionPolicy", policy =>
            {
                policy.WithOrigins(configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        // Configuración de rate limiting
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1)
                    }));
        });

        // Configuración de response caching
        services.AddResponseCaching();
        services.AddOutputCache(options =>
        {
            options.AddBasePolicy(builder =>
                builder.Expire(TimeSpan.FromMinutes(10)));
        });

        return services;
    }
}
```

### **3. Configuración de Monitoreo**

```csharp
// MusicalMatching.API/Configuration/MonitoringConfiguration.cs
namespace MusicalMatching.API.Configuration;

public static class MonitoringConfiguration
{
    public static IServiceCollection AddMonitoring(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Configuración de Serilog
        services.AddLogging(builder =>
        {
            builder.AddSerilog(new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithCorrelationId()
                .WriteTo.Console()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(
                    new Uri(configuration["Elasticsearch:Url"]))
                {
                    IndexFormat = "musical-matching-{0:yyyy.MM}",
                    AutoRegisterTemplate = true,
                    NumberOfReplicas = 1,
                    NumberOfShards = 2
                })
                .CreateLogger());
        });

        // Configuración de OpenTelemetry
        services.AddOpenTelemetry()
            .WithTracing(builder => builder
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddJaegerExporter(options =>
                {
                    options.AgentHost = configuration["Jaeger:Host"];
                    options.AgentPort = int.Parse(configuration["Jaeger:Port"]);
                }))
            .WithMetrics(builder => builder
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddPrometheusExporter());

        // Configuración de métricas personalizadas
        services.AddMetrics();

        return services;
    }
}
```

---

## 🐳 **Docker y Kubernetes**

### **Docker Compose para Desarrollo**

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
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=db;Database=MusicalMatching;User Id=sa;Password=YourStrong@Passw0rd;
      - Redis__ConnectionString=redis:6379
      - Elasticsearch__Url=http://elasticsearch:9200
      - Jaeger__Host=jaeger
      - Jaeger__Port=6831
    depends_on:
      - db
      - redis
      - elasticsearch
      - jaeger

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data

  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.11.0
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false
    ports:
      - "9200:9200"
    volumes:
      - elasticsearch_data:/usr/share/elasticsearch/data

  jaeger:
    image: jaegertracing/all-in-one:latest
    ports:
      - "16686:16686"
      - "6831:6831/udp"

  prometheus:
    image: prom/prometheus:latest
    ports:
      - "9090:9090"
    volumes:
      - ./monitoring/prometheus.yml:/etc/prometheus/prometheus.yml
      - prometheus_data:/prometheus

  grafana:
    image: grafana/grafana:latest
    ports:
      - "3000:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin
    volumes:
      - grafana_data:/var/lib/grafana

volumes:
  sqlserver_data:
  redis_data:
  elasticsearch_data:
  prometheus_data:
  grafana_data:
```

### **Manifiestos Kubernetes para Producción**

```yaml
# k8s/deployment.yml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: musical-matching-api
  namespace: production
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
        image: ghcr.io/your-org/musical-matching-api:v1.0.0
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: db-secrets
              key: connection-string
        - name: Redis__ConnectionString
          valueFrom:
            configMapKeyRef:
              name: app-config
              key: redis-connection
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health/live
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 5
        securityContext:
          runAsNonRoot: true
          runAsUser: 1000
          allowPrivilegeEscalation: false
          readOnlyRootFilesystem: true
        volumeMounts:
        - name: app-logs
          mountPath: /app/logs
      volumes:
      - name: app-logs
        emptyDir: {}
      securityContext:
        fsGroup: 1000
```

---

## 🔄 **CI/CD Pipeline Completo**

### **GitHub Actions para Despliegue**

```yaml
# .github/workflows/deploy-production.yml
name: Deploy to Production

on:
  push:
    branches: [ main ]
  workflow_dispatch:

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}

jobs:
  security-scan:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    - name: Run Snyk to check for vulnerabilities
      uses: snyk/actions/dotnet@master
      env:
        SNYK_TOKEN: ${{ secrets.SNYK_TOKEN }}
      with:
        args: --severity-threshold=high

    - name: Run OWASP Dependency Check
      uses: dependency-check/Dependency-Check_Action@main
      with:
        project: 'MusicalMatching'
        path: '.'
        format: 'HTML'
        out: 'reports'

  build-and-test:
    needs: security-scan
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore --configuration Release
    
    - name: Run tests
      run: dotnet test --no-build --verbosity normal --configuration Release --collect:"XPlat Code Coverage" --results-directory ./coverage
    
    - name: Upload coverage to Codecov
      uses: codecov/codecov-action@v3
      with:
        file: ./coverage/**/coverage.cobertura.xml
        flags: unittests
        name: codecov-umbrella

  docker-build:
    needs: build-and-test
    runs-on: ubuntu-latest
    permissions:
      contents: read
      packages: write
    steps:
    - uses: actions/checkout@v4
    
    - name: Set up Docker Buildx
      uses: docker/setup-buildx-action@v3
    
    - name: Log in to Container Registry
      uses: docker/login-action@v3
      with:
        registry: ${{ env.REGISTRY }}
        username: ${{ github.actor }}
        password: ${{ secrets.GITHUB_TOKEN }}
    
    - name: Extract metadata
      id: meta
      uses: docker/metadata-action@v5
      with:
        images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}
        tags: |
          type=ref,event=branch
          type=ref,event=pr
          type=semver,pattern={{version}}
          type=semver,pattern={{major}}.{{minor}}
          type=sha,prefix={{branch}}-
    
    - name: Build and push Docker image
      uses: docker/build-push-action@v4
      with:
        context: .
        push: true
        tags: ${{ steps.meta.outputs.tags }}
        labels: ${{ steps.meta.outputs.labels }}
        cache-from: type=gha
        cache-to: type=gha,mode=max

  deploy-production:
    needs: docker-build
    runs-on: ubuntu-latest
    environment: production
    steps:
    - uses: actions/checkout@v4
    
    - name: Configure AWS credentials
      uses: aws-actions/configure-aws-credentials@v4
      with:
        aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
        aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
        aws-region: ${{ secrets.AWS_REGION }}
    
    - name: Update kubeconfig
      run: aws eks update-kubeconfig --region ${{ secrets.AWS_REGION }} --name musical-matching-cluster
    
    - name: Deploy to Kubernetes
      run: |
        kubectl set image deployment/musical-matching-api api=${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ github.sha }} -n production
        kubectl rollout status deployment/musical-matching-api -n production
    
    - name: Run smoke tests
      run: |
        # Wait for deployment to be ready
        kubectl wait --for=condition=available --timeout=300s deployment/musical-matching-api -n production
        
        # Run smoke tests
        ./scripts/run-smoke-tests.sh production
    
    - name: Notify deployment success
      if: success()
      uses: 8398a7/action-slack@v3
      with:
        status: success
        text: 'Production deployment successful! 🚀'
        webhook_url: ${{ secrets.SLACK_WEBHOOK_URL }}

  rollback:
    if: failure()
    needs: deploy-production
    runs-on: ubuntu-latest
    environment: production
    steps:
    - uses: actions/checkout@v4
    
    - name: Configure AWS credentials
      uses: aws-actions/configure-aws-credentials@v4
      with:
        aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
        aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
        aws-region: ${{ secrets.AWS_REGION }}
    
    - name: Update kubeconfig
      run: aws eks update-kubeconfig --region ${{ secrets.AWS_REGION }} --name musical-matching-cluster
    
    - name: Rollback deployment
      run: |
        kubectl rollout undo deployment/musical-matching-api -n production
        kubectl rollout status deployment/musical-matching-api -n production
    
    - name: Notify rollback
      uses: 8398a7/action-slack@v3
      with:
        status: failure
        text: 'Production deployment failed, rolled back to previous version ⚠️'
        webhook_url: ${{ secrets.SLACK_WEBHOOK_URL }}
```

---

## 📊 **Monitoreo y Observabilidad**

### **Dashboard de Grafana**

```json
// monitoring/grafana/dashboards/production-overview.json
{
  "dashboard": {
    "title": "Musical Matching - Production Overview",
    "panels": [
      {
        "title": "API Response Time",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(http_request_duration_seconds_sum[5m]) / rate(http_request_duration_seconds_count[5m])",
            "legendFormat": "{{method}} {{route}}"
          }
        ]
      },
      {
        "title": "Request Rate",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(http_requests_total[5m])",
            "legendFormat": "{{method}} {{route}} {{status}}"
          }
        ]
      },
      {
        "title": "Error Rate",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(http_requests_total{status=~\"4..|5..\"}[5m])",
            "legendFormat": "{{method}} {{route}}"
          }
        ]
      },
      {
        "title": "Database Connections",
        "type": "stat",
        "targets": [
          {
            "expr": "sql_connections_current",
            "legendFormat": "Active Connections"
          }
        ]
      },
      {
        "title": "Memory Usage",
        "type": "graph",
        "targets": [
          {
            "expr": "process_resident_memory_bytes / 1024 / 1024",
            "legendFormat": "Memory (MB)"
          }
        ]
      }
    ]
  }
}
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Despliegue Completo**
```bash
# Implementa:
# - Despliegue en Kubernetes
# - Configuración de CI/CD
# - Monitoreo y alertas
# - Backup y disaster recovery
```

### **Ejercicio 2: Testing en Producción**
```bash
# Crea:
# - Smoke tests automatizados
# - Load testing
# - Chaos engineering
# - Métricas de rendimiento
```

### **Ejercicio 3: Monitoreo Avanzado**
```bash
# Implementa:
# - Dashboards de Grafana
# - Alertas de Prometheus
# - Logs estructurados
# - Trazado distribuido
```

---

## 📚 **Resumen del Módulo**

En este módulo hemos aprendido:

1. **🏗️ Arquitectura Avanzada**: Clean Architecture, CQRS, Event Sourcing
2. **🐳 Containerización**: Docker multi-stage, Docker Compose
3. **🔄 CI/CD**: GitHub Actions, pipelines automatizados
4. **☸️ Kubernetes**: Despliegue, escalado, gestión de configuración
5. **📊 Monitoreo**: Prometheus, Grafana, Serilog, Jaeger
6. **⚡ Performance**: Caching, optimización, background services
7. **🔒 Seguridad**: JWT, autorización, auditoría
8. **💾 Backup**: Estrategias de recuperación, disaster recovery
9. **🧪 Testing**: Smoke tests, load testing, chaos engineering
10. **🚀 Producción**: Despliegue completo, monitoreo, observabilidad

---

## 🎉 **¡Felicidades!**

Has completado el **Módulo 15: Sistemas Avanzados y Distribuidos** del curso de C#. 

Este módulo te ha preparado para:
- **Desarrollar** aplicaciones empresariales de alta calidad
- **Desplegar** en entornos de producción reales
- **Gestionar** sistemas distribuidos complejos
- **Monitorear** y mantener aplicaciones en producción
- **Implementar** estrategias de resiliencia y recuperación

---

**¡Has completado todo el curso de C#! 🎯🚀**

