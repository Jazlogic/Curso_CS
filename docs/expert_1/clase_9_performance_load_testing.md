# ⚡ Clase 9: Performance y Load Testing

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 8: Deployment Strategies](../expert_1/clase_8_deployment_strategies.md)
- **🏠 Inicio del Módulo**: [Expert Level 1: DevOps y CI/CD Avanzado](../expert_1/README.md)
- **➡️ Siguiente**: [Clase 10: Proyecto Final](../expert_1/clase_10_proyecto_final.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** load testing con Artillery
2. **Configurar** performance monitoring
3. **Establecer** capacity planning
4. **Optimizar** auto-scaling configuration
5. **Automatizar** performance testing

---

## 🚀 **Load Testing con Artillery**

### **Artillery Configuration**

```yaml
# artillery/load-test.yml
config:
  target: 'https://api.mussikon.com'
  phases:
    - duration: 60
      arrivalRate: 5
      name: "Warm up"
    - duration: 120
      arrivalRate: 10
      name: "Ramp up load"
    - duration: 300
      arrivalRate: 20
      name: "Sustained load"
    - duration: 60
      arrivalRate: 50
      name: "Peak load"
    - duration: 120
      arrivalRate: 10
      name: "Cool down"
  defaults:
    headers:
      Content-Type: 'application/json'
      Authorization: 'Bearer {{ $processEnvironment.API_TOKEN }}'
  plugins:
    metrics-by-endpoint: {}
    publish-metrics:
      - type: datadog
        prefix: 'artillery'
        apiKey: '{{ $processEnvironment.DATADOG_API_KEY }}'
      - type: cloudwatch
        region: 'us-east-1'
        namespace: 'MussikOn/Performance'
  variables:
    baseUrl: 'https://api.mussikon.com'
    userId: '{{ $randomString() }}'
    musicianId: '{{ $randomString() }}'

scenarios:
  - name: "User Registration Flow"
    weight: 30
    flow:
      - post:
          url: "/api/auth/register"
          json:
            email: "user{{ $randomString() }}@example.com"
            password: "TestPassword123!"
            firstName: "Test"
            lastName: "User"
          capture:
            - json: "$.token"
              as: "authToken"
      - post:
          url: "/api/auth/login"
          json:
            email: "user{{ $randomString() }}@example.com"
            password: "TestPassword123!"
          capture:
            - json: "$.token"
              as: "authToken"

  - name: "Musician Profile Management"
    weight: 25
    flow:
      - get:
          url: "/api/musicians"
          headers:
            Authorization: 'Bearer {{ authToken }}'
      - post:
          url: "/api/musicians"
          headers:
            Authorization: 'Bearer {{ authToken }}'
          json:
            name: "Test Musician {{ $randomString() }}"
            instruments: ["Guitar", "Piano"]
            genres: ["Rock", "Jazz"]
            location: "Madrid, Spain"
            hourlyRate: 50
      - get:
          url: "/api/musicians/{{ musicianId }}"
          headers:
            Authorization: 'Bearer {{ authToken }}'

  - name: "Event Management"
    weight: 20
    flow:
      - get:
          url: "/api/events"
          headers:
            Authorization: 'Bearer {{ authToken }}'
      - post:
          url: "/api/events"
          headers:
            Authorization: 'Bearer {{ authToken }}'
          json:
            title: "Test Event {{ $randomString() }}"
            description: "Test event description"
            date: "{{ $isoTimestamp }}"
            location: "Barcelona, Spain"
            budget: 1000
            requiredInstruments: ["Guitar", "Drums"]
      - get:
          url: "/api/events/{{ $randomString() }}"
          headers:
            Authorization: 'Bearer {{ authToken }}'

  - name: "Matching Algorithm"
    weight: 15
    flow:
      - post:
          url: "/api/matching/search"
          headers:
            Authorization: 'Bearer {{ authToken }}'
          json:
            location: "Madrid, Spain"
            date: "{{ $isoTimestamp }}"
            instruments: ["Guitar", "Piano"]
            genres: ["Rock", "Jazz"]
            budget: 500
      - get:
          url: "/api/matching/recommendations"
          headers:
            Authorization: 'Bearer {{ authToken }}'

  - name: "Chat System"
    weight: 10
    flow:
      - get:
          url: "/api/chat/conversations"
          headers:
            Authorization: 'Bearer {{ authToken }}'
      - post:
          url: "/api/chat/messages"
          headers:
            Authorization: 'Bearer {{ authToken }}'
          json:
            conversationId: "{{ $randomString() }}"
            message: "Test message {{ $randomString() }}"
      - get:
          url: "/api/chat/conversations/{{ $randomString() }}/messages"
          headers:
            Authorization: 'Bearer {{ authToken }}'
```

### **Artillery Load Test Script**

```bash
#!/bin/bash
# scripts/load-test.sh

set -e

echo "🚀 Starting load test..."

# Configuration
TEST_FILE="artillery/load-test.yml"
RESULTS_DIR="artillery/results"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
RESULTS_FILE="$RESULTS_DIR/load-test-$TIMESTAMP.json"

# Create results directory
mkdir -p $RESULTS_DIR

# Check if API token is set
if [ -z "$API_TOKEN" ]; then
    echo "❌ API_TOKEN environment variable is not set"
    exit 1
fi

# Run load test
echo "📊 Running load test..."
artillery run $TEST_FILE --output $RESULTS_FILE

# Generate report
echo "📈 Generating report..."
artillery report $RESULTS_FILE --output $RESULTS_DIR/load-test-$TIMESTAMP.html

# Check for failures
FAILURES=$(artillery report $RESULTS_FILE --output /dev/null 2>&1 | grep -o "failures: [0-9]*" | cut -d' ' -f2)
if [ "$FAILURES" -gt 0 ]; then
    echo "❌ Load test failed with $FAILURES failures"
    exit 1
fi

echo "✅ Load test completed successfully!"
echo "📊 Results saved to: $RESULTS_FILE"
echo "📈 Report saved to: $RESULTS_DIR/load-test-$TIMESTAMP.html"
```

---

## 📊 **Performance Monitoring**

### **Performance Metrics Collection**

```csharp
// MusicalMatching.API/Services/PerformanceMonitoringService.cs
namespace MusicalMatching.API.Services;

public interface IPerformanceMonitoringService
{
    void RecordRequestDuration(string endpoint, double duration);
    void RecordDatabaseQuery(string query, double duration);
    void RecordCacheHit(string key);
    void RecordCacheMiss(string key);
    void RecordBusinessMetric(string metric, double value);
}

public class PerformanceMonitoringService : IPerformanceMonitoringService
{
    private readonly ILogger<PerformanceMonitoringService> _logger;
    private readonly IMetricsService _metricsService;
    private readonly IMemoryCache _cache;

    public PerformanceMonitoringService(
        ILogger<PerformanceMonitoringService> logger,
        IMetricsService metricsService,
        IMemoryCache cache)
    {
        _logger = logger;
        _metricsService = metricsService;
        _cache = cache;
    }

    public void RecordRequestDuration(string endpoint, double duration)
    {
        _metricsService.RecordRequestDuration("GET", endpoint, duration);
        
        // Log slow requests
        if (duration > 1.0)
        {
            _logger.LogWarning("Slow request detected: {Endpoint} took {Duration}ms", endpoint, duration * 1000);
        }
    }

    public void RecordDatabaseQuery(string query, double duration)
    {
        _metricsService.RecordDatabaseQuery("SELECT", duration);
        
        // Log slow queries
        if (duration > 0.5)
        {
            _logger.LogWarning("Slow database query detected: {Query} took {Duration}ms", query, duration * 1000);
        }
    }

    public void RecordCacheHit(string key)
    {
        _metricsService.IncrementBusinessEvent("cache_hit", "success");
        _logger.LogDebug("Cache hit for key: {Key}", key);
    }

    public void RecordCacheMiss(string key)
    {
        _metricsService.IncrementBusinessEvent("cache_miss", "success");
        _logger.LogDebug("Cache miss for key: {Key}", key);
    }

    public void RecordBusinessMetric(string metric, double value)
    {
        _metricsService.IncrementBusinessEvent(metric, "success");
        _logger.LogInformation("Business metric recorded: {Metric} = {Value}", metric, value);
    }
}
```

### **Performance Middleware**

```csharp
// MusicalMatching.API/Middleware/PerformanceMiddleware.cs
namespace MusicalMatching.API.Middleware;

public class PerformanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IPerformanceMonitoringService _performanceService;
    private readonly ILogger<PerformanceMiddleware> _logger;

    public PerformanceMiddleware(
        RequestDelegate next,
        IPerformanceMonitoringService performanceService,
        ILogger<PerformanceMiddleware> logger)
    {
        _next = next;
        _performanceService = performanceService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = context.Request.Path.Value ?? "";

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var duration = stopwatch.Elapsed.TotalSeconds;

            _performanceService.RecordRequestDuration(endpoint, duration);

            // Log performance metrics
            _logger.LogInformation("Request {Method} {Endpoint} completed in {Duration}ms with status {StatusCode}",
                context.Request.Method, endpoint, stopwatch.ElapsedMilliseconds, context.Response.StatusCode);
        }
    }
}
```

---

## 📈 **Capacity Planning**

### **Capacity Planning Script**

```bash
#!/bin/bash
# scripts/capacity-planning.sh

set -e

echo "📈 Starting capacity planning analysis..."

# Configuration
NAMESPACE="mussikon"
APP_NAME="mussikon-api"
METRICS_DURATION="1h"

# Get current resource usage
echo "📊 Current resource usage:"
kubectl top pods -l app=$APP_NAME -n $NAMESPACE

# Get resource requests and limits
echo "📋 Resource configuration:"
kubectl get deployment $APP_NAME -n $NAMESPACE -o jsonpath='{.spec.template.spec.containers[0].resources}'

# Calculate capacity metrics
echo "🧮 Calculating capacity metrics..."

# CPU utilization
CPU_USAGE=$(kubectl top pods -l app=$APP_NAME -n $NAMESPACE --no-headers | awk '{sum+=$2} END {print sum}')
CPU_LIMIT=$(kubectl get pods -l app=$APP_NAME -n $NAMESPACE -o jsonpath='{.items[0].spec.containers[0].resources.limits.cpu}' | sed 's/m//')
CPU_UTILIZATION=$(echo "scale=2; $CPU_USAGE / $CPU_LIMIT * 100" | bc)

# Memory utilization
MEMORY_USAGE=$(kubectl top pods -l app=$APP_NAME -n $NAMESPACE --no-headers | awk '{sum+=$3} END {print sum}')
MEMORY_LIMIT=$(kubectl get pods -l app=$APP_NAME -n $NAMESPACE -o jsonpath='{.items[0].spec.containers[0].resources.limits.memory}' | sed 's/Mi//')
MEMORY_UTILIZATION=$(echo "scale=2; $MEMORY_USAGE / $MEMORY_LIMIT * 100" | bc)

echo "📊 Capacity Analysis Results:"
echo "CPU Utilization: ${CPU_UTILIZATION}%"
echo "Memory Utilization: ${MEMORY_UTILIZATION}%"

# Recommendations
echo "💡 Recommendations:"

if (( $(echo "$CPU_UTILIZATION > 80" | bc -l) )); then
    echo "⚠️ High CPU utilization detected. Consider scaling up or optimizing CPU usage."
fi

if (( $(echo "$MEMORY_UTILIZATION > 80" | bc -l) )); then
    echo "⚠️ High memory utilization detected. Consider scaling up or optimizing memory usage."
fi

if (( $(echo "$CPU_UTILIZATION < 20" | bc -l) )); then
    echo "💡 Low CPU utilization. Consider scaling down to save costs."
fi

if (( $(echo "$MEMORY_UTILIZATION < 20" | bc -l) )); then
    echo "💡 Low memory utilization. Consider scaling down to save costs."
fi

echo "✅ Capacity planning analysis completed!"
```

### **Capacity Planning Dashboard**

```json
{
  "dashboard": {
    "id": null,
    "title": "Capacity Planning Dashboard",
    "tags": ["capacity", "planning"],
    "panels": [
      {
        "id": 1,
        "title": "CPU Utilization",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(container_cpu_usage_seconds_total[5m]) * 100",
            "legendFormat": "CPU Usage %"
          }
        ],
        "yAxes": [
          {
            "label": "CPU %",
            "min": 0,
            "max": 100
          }
        ]
      },
      {
        "id": 2,
        "title": "Memory Utilization",
        "type": "graph",
        "targets": [
          {
            "expr": "(container_memory_usage_bytes / container_spec_memory_limit_bytes) * 100",
            "legendFormat": "Memory Usage %"
          }
        ],
        "yAxes": [
          {
            "label": "Memory %",
            "min": 0,
            "max": 100
          }
        ]
      },
      {
        "id": 3,
        "title": "Request Rate",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(http_requests_total[5m])",
            "legendFormat": "Requests/sec"
          }
        ]
      },
      {
        "id": 4,
        "title": "Response Time",
        "type": "graph",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))",
            "legendFormat": "95th percentile"
          }
        ]
      }
    ]
  }
}
```

---

## 🔄 **Auto-scaling Configuration**

### **Horizontal Pod Autoscaler**

```yaml
# k8s/autoscaling/hpa.yml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: mussikon-api-hpa
  namespace: mussikon
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: mussikon-api
  minReplicas: 3
  maxReplicas: 20
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
  - type: Pods
    pods:
      metric:
        name: http_requests_per_second
      target:
        type: AverageValue
        averageValue: "100"
  behavior:
    scaleDown:
      stabilizationWindowSeconds: 300
      policies:
      - type: Percent
        value: 10
        periodSeconds: 60
    scaleUp:
      stabilizationWindowSeconds: 60
      policies:
      - type: Percent
        value: 50
        periodSeconds: 60
      - type: Pods
        value: 2
        periodSeconds: 60
      selectPolicy: Max
```

### **Vertical Pod Autoscaler**

```yaml
# k8s/autoscaling/vpa.yml
apiVersion: autoscaling.k8s.io/v1
kind: VerticalPodAutoscaler
metadata:
  name: mussikon-api-vpa
  namespace: mussikon
spec:
  targetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: mussikon-api
  updatePolicy:
    updateMode: "Auto"
  resourcePolicy:
    containerPolicies:
    - containerName: api
      minAllowed:
        cpu: 100m
        memory: 128Mi
      maxAllowed:
        cpu: 2000m
        memory: 4Gi
      controlledResources: ["cpu", "memory"]
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Load Testing**
```yaml
# Configura Artillery para realizar
# load testing de la aplicación MussikOn
```

### **Ejercicio 2: Performance Monitoring**
```csharp
// Implementa monitoreo de performance
// en la aplicación .NET
```

### **Ejercicio 3: Auto-scaling**
```yaml
# Configura auto-scaling horizontal y vertical
# para optimizar recursos
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **🚀 Load Testing**: Pruebas de carga con Artillery
2. **📊 Performance Monitoring**: Monitoreo de rendimiento
3. **📈 Capacity Planning**: Planificación de capacidad
4. **🔄 Auto-scaling**: Escalado automático
5. **⚡ Performance Optimization**: Optimización de rendimiento
6. **📊 Metrics Collection**: Recopilación de métricas

---

## 🚀 **Próximos Pasos**

En la siguiente clase implementaremos el **Proyecto Final: Pipeline Completo para MussikOn**.

---

**¡Has completado la novena clase del Expert Level 1! ⚡🎯**
