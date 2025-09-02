# 🚀 Clase 8: Deployment Strategies

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 7: Security en CI/CD](../expert_1/clase_7_security_cicd.md)
- **🏠 Inicio del Módulo**: [Expert Level 1: DevOps y CI/CD Avanzado](../expert_1/README.md)
- **➡️ Siguiente**: [Clase 9: Performance y Load Testing](../expert_1/clase_9_performance_load_testing.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** blue-green deployments
2. **Configurar** canary releases y feature flags
3. **Establecer** rollback strategies
4. **Automatizar** database migration
5. **Optimizar** deployment automation

---

## 🔵🟢 **Blue-Green Deployments**

### **Blue-Green Deployment Strategy**

```yaml
# k8s/deployments/blue-green-deployment.yml
apiVersion: argoproj.io/v1alpha1
kind: Rollout
metadata:
  name: mussikon-api
  namespace: mussikon
spec:
  replicas: 3
  strategy:
    blueGreen:
      activeService: mussikon-api-active
      previewService: mussikon-api-preview
      autoPromotionEnabled: false
      scaleDownDelaySeconds: 30
      prePromotionAnalysis:
        templates:
        - templateName: success-rate
        args:
        - name: service-name
          value: mussikon-api-preview
      postPromotionAnalysis:
        templates:
        - templateName: success-rate
        args:
        - name: service-name
          value: mussikon-api-active
  selector:
    matchLabels:
      app: mussikon-api
  template:
    metadata:
      labels:
        app: mussikon-api
    spec:
      containers:
      - name: api
        image: mussikon/api:latest
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
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
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: mussikon-api-active
  namespace: mussikon
spec:
  selector:
    app: mussikon-api
  ports:
  - port: 80
    targetPort: 8080
---
apiVersion: v1
kind: Service
metadata:
  name: mussikon-api-preview
  namespace: mussikon
spec:
  selector:
    app: mussikon-api
  ports:
  - port: 80
    targetPort: 8080
```

### **Blue-Green Deployment Script**

```bash
#!/bin/bash
# scripts/blue-green-deploy.sh

set -e

echo "🔵🟢 Starting Blue-Green Deployment..."

# Configuration
NAMESPACE="mussikon"
APP_NAME="mussikon-api"
NEW_VERSION=$1
CURRENT_VERSION=$(kubectl get deployment $APP_NAME -n $NAMESPACE -o jsonpath='{.spec.template.spec.containers[0].image}' | cut -d':' -f2)

if [ -z "$NEW_VERSION" ]; then
    echo "❌ Please provide a version number"
    exit 1
fi

echo "📋 Current version: $CURRENT_VERSION"
echo "📋 New version: $NEW_VERSION"

# Deploy to preview (green)
echo "🟢 Deploying to preview environment..."
kubectl set image deployment/$APP_NAME-preview $APP_NAME=mussikon/api:$NEW_VERSION -n $NAMESPACE

# Wait for rollout
echo "⏳ Waiting for preview deployment to be ready..."
kubectl rollout status deployment/$APP_NAME-preview -n $NAMESPACE --timeout=300s

# Run health checks
echo "🔍 Running health checks..."
kubectl port-forward service/$APP_NAME-preview 8080:80 -n $NAMESPACE &
PORT_FORWARD_PID=$!

sleep 10

# Health check
HEALTH_STATUS=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8080/health)
if [ "$HEALTH_STATUS" != "200" ]; then
    echo "❌ Health check failed with status: $HEALTH_STATUS"
    kill $PORT_FORWARD_PID
    exit 1
fi

kill $PORT_FORWARD_PID

# Promote to active (blue)
echo "🔵 Promoting to active environment..."
kubectl set image deployment/$APP_NAME $APP_NAME=mussikon/api:$NEW_VERSION -n $NAMESPACE

# Wait for rollout
echo "⏳ Waiting for active deployment to be ready..."
kubectl rollout status deployment/$APP_NAME -n $NAMESPACE --timeout=300s

# Scale down preview
echo "🔄 Scaling down preview environment..."
kubectl scale deployment $APP_NAME-preview --replicas=0 -n $NAMESPACE

echo "✅ Blue-Green deployment completed successfully!"
```

---

## 🐦 **Canary Releases y Feature Flags**

### **Canary Deployment Configuration**

```yaml
# k8s/deployments/canary-deployment.yml
apiVersion: argoproj.io/v1alpha1
kind: Rollout
metadata:
  name: mussikon-api-canary
  namespace: mussikon
spec:
  replicas: 5
  strategy:
    canary:
      steps:
      - setWeight: 20
      - pause: {duration: 10m}
      - setWeight: 40
      - pause: {duration: 10m}
      - setWeight: 60
      - pause: {duration: 10m}
      - setWeight: 80
      - pause: {duration: 10m}
      canaryService: mussikon-api-canary
      stableService: mussikon-api-stable
      trafficRouting:
        nginx:
          stableIngress: mussikon-api-stable
          annotationPrefix: nginx.ingress.kubernetes.io
      analysis:
        templates:
        - templateName: success-rate
        args:
        - name: service-name
          value: mussikon-api-canary
        startingStep: 2
        successfulHistoryLimit: 1
        unsuccessfulHistoryLimit: 1
  selector:
    matchLabels:
      app: mussikon-api
  template:
    metadata:
      labels:
        app: mussikon-api
    spec:
      containers:
      - name: api
        image: mussikon/api:latest
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: FEATURE_FLAGS__ENABLE_NEW_FEATURE
          value: "true"
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
```

### **Feature Flags Implementation**

```csharp
// MusicalMatching.API/Services/FeatureFlagService.cs
namespace MusicalMatching.API.Services;

public interface IFeatureFlagService
{
    Task<bool> IsEnabledAsync(string featureName, string userId = null);
    Task<T> GetValueAsync<T>(string featureName, T defaultValue = default);
}

public class FeatureFlagService : IFeatureFlagService
{
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;
    private readonly ILogger<FeatureFlagService> _logger;

    public FeatureFlagService(
        IConfiguration configuration,
        IMemoryCache cache,
        ILogger<FeatureFlagService> logger)
    {
        _configuration = configuration;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> IsEnabledAsync(string featureName, string userId = null)
    {
        var cacheKey = $"feature_flag_{featureName}_{userId}";
        
        if (_cache.TryGetValue(cacheKey, out bool cachedValue))
        {
            return cachedValue;
        }

        // Check configuration
        var configValue = _configuration.GetValue<bool>($"FeatureFlags:{featureName}");
        
        // Check user-specific flags
        if (!string.IsNullOrEmpty(userId))
        {
            var userFlag = await GetUserFeatureFlagAsync(featureName, userId);
            if (userFlag.HasValue)
            {
                configValue = userFlag.Value;
            }
        }

        // Cache for 5 minutes
        _cache.Set(cacheKey, configValue, TimeSpan.FromMinutes(5));
        
        return configValue;
    }

    public async Task<T> GetValueAsync<T>(string featureName, T defaultValue = default)
    {
        var cacheKey = $"feature_flag_value_{featureName}";
        
        if (_cache.TryGetValue(cacheKey, out T cachedValue))
        {
            return cachedValue;
        }

        var configValue = _configuration.GetValue<T>($"FeatureFlags:{featureName}", defaultValue);
        
        // Cache for 5 minutes
        _cache.Set(cacheKey, configValue, TimeSpan.FromMinutes(5));
        
        return configValue;
    }

    private async Task<bool?> GetUserFeatureFlagAsync(string featureName, string userId)
    {
        // Implement user-specific feature flag logic
        // This could query a database or external service
        return null;
    }
}
```

### **Feature Flag Middleware**

```csharp
// MusicalMatching.API/Middleware/FeatureFlagMiddleware.cs
namespace MusicalMatching.API.Middleware;

public class FeatureFlagMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IFeatureFlagService _featureFlagService;
    private readonly ILogger<FeatureFlagMiddleware> _logger;

    public FeatureFlagMiddleware(
        RequestDelegate next,
        IFeatureFlagService featureFlagService,
        ILogger<FeatureFlagMiddleware> logger)
    {
        _next = next;
        _featureFlagService = featureFlagService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if the request is for a feature-flagged endpoint
        var endpoint = context.Request.Path.Value;
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // Example: Check if new API version is enabled
        if (endpoint.StartsWith("/api/v2/"))
        {
            var isV2Enabled = await _featureFlagService.IsEnabledAsync("EnableV2API", userId);
            if (!isV2Enabled)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("API version not available");
                return;
            }
        }

        // Example: Check if new feature is enabled
        if (endpoint.Contains("/new-feature"))
        {
            var isNewFeatureEnabled = await _featureFlagService.IsEnabledAsync("EnableNewFeature", userId);
            if (!isNewFeatureEnabled)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Feature not available");
                return;
            }
        }

        await _next(context);
    }
}
```

---

## 🔄 **Rollback Strategies**

### **Automated Rollback Configuration**

```yaml
# k8s/deployments/rollback-config.yml
apiVersion: argoproj.io/v1alpha1
kind: Rollout
metadata:
  name: mussikon-api-rollback
  namespace: mussikon
spec:
  replicas: 3
  strategy:
    canary:
      steps:
      - setWeight: 20
      - pause: {duration: 5m}
      - setWeight: 40
      - pause: {duration: 5m}
      - setWeight: 60
      - pause: {duration: 5m}
      - setWeight: 80
      - pause: {duration: 5m}
      canaryService: mussikon-api-canary
      stableService: mussikon-api-stable
      trafficRouting:
        nginx:
          stableIngress: mussikon-api-stable
      analysis:
        templates:
        - templateName: success-rate
        args:
        - name: service-name
          value: mussikon-api-canary
        startingStep: 2
        successfulHistoryLimit: 1
        unsuccessfulHistoryLimit: 1
        # Auto-rollback on failure
        rollbackOnFailure: true
        # Rollback if error rate > 5%
        rollbackOnError: true
        rollbackOnErrorThreshold: 0.05
  selector:
    matchLabels:
      app: mussikon-api
  template:
    metadata:
      labels:
        app: mussikon-api
    spec:
      containers:
      - name: api
        image: mussikon/api:latest
        ports:
        - containerPort: 8080
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
```

### **Rollback Script**

```bash
#!/bin/bash
# scripts/rollback.sh

set -e

echo "🔄 Starting rollback process..."

# Configuration
NAMESPACE="mussikon"
APP_NAME="mussikon-api"
ROLLBACK_VERSION=$1

if [ -z "$ROLLBACK_VERSION" ]; then
    echo "📋 Available versions:"
    kubectl rollout history deployment/$APP_NAME -n $NAMESPACE
    echo "❌ Please provide a rollback version"
    exit 1
fi

echo "🔄 Rolling back to version: $ROLLBACK_VERSION"

# Perform rollback
kubectl rollout undo deployment/$APP_NAME --to-revision=$ROLLBACK_VERSION -n $NAMESPACE

# Wait for rollout
echo "⏳ Waiting for rollback to complete..."
kubectl rollout status deployment/$APP_NAME -n $NAMESPACE --timeout=300s

# Verify rollback
echo "🔍 Verifying rollback..."
kubectl get pods -l app=$APP_NAME -n $NAMESPACE

# Health check
echo "🔍 Running health checks..."
kubectl port-forward service/$APP_NAME 8080:80 -n $NAMESPACE &
PORT_FORWARD_PID=$!

sleep 10

HEALTH_STATUS=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8080/health)
if [ "$HEALTH_STATUS" != "200" ]; then
    echo "❌ Health check failed after rollback"
    kill $PORT_FORWARD_PID
    exit 1
fi

kill $PORT_FORWARD_PID

echo "✅ Rollback completed successfully!"
```

---

## 🗄️ **Database Migration Automation**

### **Database Migration Job**

```yaml
# k8s/jobs/database-migration.yml
apiVersion: batch/v1
kind: Job
metadata:
  name: database-migration
  namespace: mussikon
spec:
  template:
    spec:
      containers:
      - name: migration
        image: mussikon/api:latest
        command: ["dotnet", "ef", "database", "update"]
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: mussikon-secrets
              key: database-connection
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        resources:
          requests:
            memory: "128Mi"
            cpu: "100m"
          limits:
            memory: "256Mi"
            cpu: "200m"
      restartPolicy: Never
  backoffLimit: 3
---
apiVersion: batch/v1
kind: CronJob
metadata:
  name: database-migration-cron
  namespace: mussikon
spec:
  schedule: "0 2 * * *" # Daily at 2 AM
  jobTemplate:
    spec:
      template:
        spec:
          containers:
          - name: migration
            image: mussikon/api:latest
            command: ["dotnet", "ef", "database", "update"]
            env:
            - name: ConnectionStrings__DefaultConnection
              valueFrom:
                secretKeyRef:
                  name: mussikon-secrets
                  key: database-connection
            - name: ASPNETCORE_ENVIRONMENT
              value: "Production"
          restartPolicy: Never
      backoffLimit: 3
```

### **Database Migration Script**

```bash
#!/bin/bash
# scripts/database-migration.sh

set -e

echo "🗄️ Starting database migration..."

# Configuration
NAMESPACE="mussikon"
JOB_NAME="database-migration-$(date +%s)"

# Create migration job
echo "📋 Creating migration job..."
kubectl create job $JOB_NAME --from=cronjob/database-migration-cron -n $NAMESPACE

# Wait for job completion
echo "⏳ Waiting for migration to complete..."
kubectl wait --for=condition=complete job/$JOB_NAME -n $NAMESPACE --timeout=600s

# Check job status
JOB_STATUS=$(kubectl get job $JOB_NAME -n $NAMESPACE -o jsonpath='{.status.conditions[0].type}')
if [ "$JOB_STATUS" != "Complete" ]; then
    echo "❌ Migration failed"
    kubectl logs job/$JOB_NAME -n $NAMESPACE
    exit 1
fi

# Clean up job
echo "🧹 Cleaning up migration job..."
kubectl delete job $JOB_NAME -n $NAMESPACE

echo "✅ Database migration completed successfully!"
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Blue-Green Deployment**
```yaml
# Configura un deployment blue-green
# para la aplicación MussikOn
```

### **Ejercicio 2: Canary Release**
```yaml
# Implementa un canary release
# con feature flags
```

### **Ejercicio 3: Rollback Strategy**
```bash
# Crea un sistema de rollback
# automático y manual
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **🔵🟢 Blue-Green Deployments**: Estrategia de despliegue sin downtime
2. **🐦 Canary Releases**: Despliegue gradual con monitoreo
3. **🚩 Feature Flags**: Control de funcionalidades
4. **🔄 Rollback Strategies**: Estrategias de reversión
5. **🗄️ Database Migration**: Automatización de migraciones
6. **🚀 Deployment Automation**: Automatización completa

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Performance y Load Testing**, implementando pruebas de rendimiento.

---

**¡Has completado la octava clase del Expert Level 1! 🚀🎯**
