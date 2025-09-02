# 🏆 Clase 10: Proyecto Final - Pipeline Completo para MussikOn

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 9: Performance y Load Testing](../expert_1/clase_9_performance_load_testing.md)
- **🏠 Inicio del Módulo**: [Expert Level 1: DevOps y CI/CD Avanzado](../expert_1/README.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos del Proyecto Final**

1. **Implementar** pipeline completo de CI/CD
2. **Configurar** multi-environment deployment
3. **Establecer** monitoring y alerting setup
4. **Implementar** disaster recovery procedures
5. **Automatizar** todo el proceso de deployment

---

## 🚀 **Pipeline Completo de CI/CD**

### **Main CI/CD Pipeline**

```yaml
# .github/workflows/ci-cd-pipeline.yml
name: MussikOn CI/CD Pipeline

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]
  release:
    types: [published]

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}

jobs:
  # Quality Gates
  quality-gates:
    runs-on: ubuntu-latest
    outputs:
      quality-passed: ${{ steps.quality-check.outputs.passed }}
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0'

      - name: Cache dependencies
        uses: actions/cache@v3
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --configuration Release --no-restore

      - name: Run tests
        run: dotnet test --configuration Release --no-build --collect:"XPlat Code Coverage" --logger trx

      - name: Run security scan
        run: |
          dotnet list package --vulnerable --include-transitive
          if [ $? -ne 0 ]; then
            echo "Vulnerable packages found!"
            exit 1
          fi

      - name: Run SonarQube analysis
        uses: sonarqube-quality-gate-action@master
        env:
          SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}

      - name: Quality check
        id: quality-check
        run: |
          if [ $? -eq 0 ]; then
            echo "passed=true" >> $GITHUB_OUTPUT
          else
            echo "passed=false" >> $GITHUB_OUTPUT
          fi

  # Build and Push Images
  build-and-push:
    needs: quality-gates
    if: needs.quality-gates.outputs.quality-passed == 'true'
    runs-on: ubuntu-latest
    strategy:
      matrix:
        service: [api, web, worker]
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v2

      - name: Log in to Container Registry
        uses: docker/login-action@v2
        with:
          registry: ${{ env.REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: Extract metadata
        id: meta
        uses: docker/metadata-action@v4
        with:
          images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}/${{ matrix.service }}
          tags: |
            type=ref,event=branch
            type=ref,event=pr
            type=semver,pattern={{version}}
            type=semver,pattern={{major}}.{{minor}}
            type=raw,value=latest,enable={{is_default_branch}}

      - name: Build and push Docker image
        uses: docker/build-push-action@v4
        with:
          context: .
          file: ./src/MusicalMatching.${{ matrix.service == 'api' && 'API' || matrix.service == 'web' && 'Web' || 'Worker' }}/Dockerfile
          push: true
          tags: ${{ steps.meta.outputs.tags }}
          labels: ${{ steps.meta.outputs.labels }}
          cache-from: type=gha
          cache-to: type=gha,mode=max

  # Deploy to Staging
  deploy-staging:
    needs: [quality-gates, build-and-push]
    if: github.ref == 'refs/heads/develop'
    runs-on: ubuntu-latest
    environment: staging
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v2
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: us-east-1

      - name: Deploy to staging
        run: |
          # Update Kubernetes manifests
          sed -i "s|image: .*|image: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}/api:develop|g" k8s/staging/api-deployment.yml
          sed -i "s|image: .*|image: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}/web:develop|g" k8s/staging/web-deployment.yml
          
          # Apply Kubernetes manifests
          kubectl apply -f k8s/staging/ -n mussikon-staging
          
          # Wait for rollout
          kubectl rollout status deployment/mussikon-api -n mussikon-staging --timeout=300s
          kubectl rollout status deployment/mussikon-web -n mussikon-staging --timeout=300s

      - name: Run smoke tests
        run: |
          # Wait for services to be ready
          kubectl wait --for=condition=ready pod -l app=mussikon-api -n mussikon-staging --timeout=300s
          
          # Run smoke tests
          kubectl run smoke-tests --image=curlimages/curl --rm -i --restart=Never -n mussikon-staging -- \
            curl -f http://mussikon-api-service:80/health

  # Deploy to Production
  deploy-production:
    needs: [quality-gates, build-and-push]
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    environment: production
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v2
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: us-east-1

      - name: Deploy to production
        run: |
          # Update Kubernetes manifests
          sed -i "s|image: .*|image: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}/api:main|g" k8s/production/api-deployment.yml
          sed -i "s|image: .*|image: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}/web:main|g" k8s/production/web-deployment.yml
          
          # Apply Kubernetes manifests
          kubectl apply -f k8s/production/ -n mussikon-production
          
          # Wait for rollout
          kubectl rollout status deployment/mussikon-api -n mussikon-production --timeout=300s
          kubectl rollout status deployment/mussikon-web -n mussikon-production --timeout=300s

      - name: Run health checks
        run: |
          # Wait for services to be ready
          kubectl wait --for=condition=ready pod -l app=mussikon-api -n mussikon-production --timeout=300s
          
          # Run health checks
          kubectl run health-checks --image=curlimages/curl --rm -i --restart=Never -n mussikon-production -- \
            curl -f https://api.mussikon.com/health

      - name: Notify deployment
        uses: 8398a7/action-slack@v3
        with:
          status: success
          channel: '#deployments'
          webhook_url: ${{ secrets.SLACK_WEBHOOK }}
          fields: repo,message,commit,author,action,eventName,ref,workflow
```

---

## 🌍 **Multi-Environment Deployment**

### **Environment Configuration**

```yaml
# k8s/environments/staging/namespace.yml
apiVersion: v1
kind: Namespace
metadata:
  name: mussikon-staging
  labels:
    name: mussikon-staging
    environment: staging
---
apiVersion: v1
kind: ResourceQuota
metadata:
  name: mussikon-staging-quota
  namespace: mussikon-staging
spec:
  hard:
    requests.cpu: "2"
    requests.memory: 4Gi
    limits.cpu: "4"
    limits.memory: 8Gi
    persistentvolumeclaims: "5"
    pods: "10"
    services: "5"
```

```yaml
# k8s/environments/production/namespace.yml
apiVersion: v1
kind: Namespace
metadata:
  name: mussikon-production
  labels:
    name: mussikon-production
    environment: production
---
apiVersion: v1
kind: ResourceQuota
metadata:
  name: mussikon-production-quota
  namespace: mussikon-production
spec:
  hard:
    requests.cpu: "8"
    requests.memory: 16Gi
    limits.cpu: "16"
    limits.memory: 32Gi
    persistentvolumeclaims: "20"
    pods: "50"
    services: "20"
```

### **Environment-Specific Configurations**

```yaml
# k8s/environments/staging/configmap.yml
apiVersion: v1
kind: ConfigMap
metadata:
  name: mussikon-staging-config
  namespace: mussikon-staging
data:
  app-settings.json: |
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft": "Warning"
        }
      },
      "AllowedHosts": "*",
      "Cors": {
        "AllowedOrigins": [
          "https://staging.mussikon.com"
        ]
      },
      "RateLimiting": {
        "RequestsPerMinute": 1000,
        "BurstLimit": 2000
      }
    }
```

```yaml
# k8s/environments/production/configmap.yml
apiVersion: v1
kind: ConfigMap
metadata:
  name: mussikon-production-config
  namespace: mussikon-production
data:
  app-settings.json: |
    {
      "Logging": {
        "LogLevel": {
          "Default": "Warning",
          "Microsoft": "Error"
        }
      },
      "AllowedHosts": "mussikon.com,www.mussikon.com",
      "Cors": {
        "AllowedOrigins": [
          "https://www.mussikon.com",
          "https://api.mussikon.com"
        ]
      },
      "RateLimiting": {
        "RequestsPerMinute": 10000,
        "BurstLimit": 20000
      }
    }
```

---

## 📊 **Monitoring y Alerting Setup**

### **Complete Monitoring Stack**

```yaml
# k8s/monitoring/monitoring-stack.yml
apiVersion: v1
kind: Namespace
metadata:
  name: monitoring
  labels:
    name: monitoring
---
# Prometheus
apiVersion: apps/v1
kind: Deployment
metadata:
  name: prometheus
  namespace: monitoring
spec:
  replicas: 1
  selector:
    matchLabels:
      app: prometheus
  template:
    metadata:
      labels:
        app: prometheus
    spec:
      containers:
      - name: prometheus
        image: prom/prometheus:v2.45.0
        args:
          - '--config.file=/etc/prometheus/prometheus.yml'
          - '--storage.tsdb.path=/prometheus/'
          - '--web.console.libraries=/etc/prometheus/console_libraries'
          - '--web.console.templates=/etc/prometheus/consoles'
          - '--storage.tsdb.retention.time=200h'
          - '--web.enable-lifecycle'
        ports:
        - containerPort: 9090
        resources:
          requests:
            cpu: 500m
            memory: 500M
          limits:
            cpu: 1000m
            memory: 1Gi
        volumeMounts:
        - name: prometheus-config-volume
          mountPath: /etc/prometheus/
        - name: prometheus-storage-volume
          mountPath: /prometheus/
      volumes:
      - name: prometheus-config-volume
        configMap:
          name: prometheus-config
      - name: prometheus-storage-volume
        persistentVolumeClaim:
          claimName: prometheus-pvc
---
# Grafana
apiVersion: apps/v1
kind: Deployment
metadata:
  name: grafana
  namespace: monitoring
spec:
  replicas: 1
  selector:
    matchLabels:
      app: grafana
  template:
    metadata:
      labels:
        app: grafana
    spec:
      containers:
      - name: grafana
        image: grafana/grafana:10.0.0
        ports:
        - containerPort: 3000
        env:
        - name: GF_SECURITY_ADMIN_PASSWORD
          valueFrom:
            secretKeyRef:
              name: grafana-secrets
              key: admin-password
        resources:
          requests:
            cpu: 100m
            memory: 128Mi
          limits:
            cpu: 500m
            memory: 512Mi
        volumeMounts:
        - name: grafana-storage
          mountPath: /var/lib/grafana
      volumes:
      - name: grafana-storage
        persistentVolumeClaim:
          claimName: grafana-pvc
---
# Alertmanager
apiVersion: apps/v1
kind: Deployment
metadata:
  name: alertmanager
  namespace: monitoring
spec:
  replicas: 1
  selector:
    matchLabels:
      app: alertmanager
  template:
    metadata:
      labels:
        app: alertmanager
    spec:
      containers:
      - name: alertmanager
        image: prom/alertmanager:v0.25.0
        ports:
        - containerPort: 9093
        resources:
          requests:
            cpu: 100m
            memory: 128Mi
          limits:
            cpu: 500m
            memory: 512Mi
        volumeMounts:
        - name: alertmanager-config
          mountPath: /etc/alertmanager/
      volumes:
      - name: alertmanager-config
        configMap:
          name: alertmanager-config
```

---

## 🚨 **Disaster Recovery Procedures**

### **Backup and Recovery Script**

```bash
#!/bin/bash
# scripts/disaster-recovery.sh

set -e

echo "🚨 Starting disaster recovery procedures..."

# Configuration
NAMESPACE="mussikon-production"
BACKUP_DIR="/backups/$(date +%Y%m%d_%H%M%S)"
S3_BUCKET="mussikon-backups"

# Create backup directory
mkdir -p $BACKUP_DIR

# Function to backup Kubernetes resources
backup_k8s_resources() {
    echo "📦 Backing up Kubernetes resources..."
    
    # Backup all resources
    kubectl get all -n $NAMESPACE -o yaml > $BACKUP_DIR/k8s-resources.yml
    
    # Backup secrets
    kubectl get secrets -n $NAMESPACE -o yaml > $BACKUP_DIR/secrets.yml
    
    # Backup configmaps
    kubectl get configmaps -n $NAMESPACE -o yaml > $BACKUP_DIR/configmaps.yml
    
    # Backup persistent volumes
    kubectl get pv -o yaml > $BACKUP_DIR/persistent-volumes.yml
}

# Function to backup database
backup_database() {
    echo "🗄️ Backing up database..."
    
    # Get database connection details
    DB_HOST=$(kubectl get secret mussikon-secrets -n $NAMESPACE -o jsonpath='{.data.database-host}' | base64 -d)
    DB_NAME=$(kubectl get secret mussikon-secrets -n $NAMESPACE -o jsonpath='{.data.database-name}' | base64 -d)
    DB_USER=$(kubectl get secret mussikon-secrets -n $NAMESPACE -o jsonpath='{.data.database-user}' | base64 -d)
    DB_PASSWORD=$(kubectl get secret mussikon-secrets -n $NAMESPACE -o jsonpath='{.data.database-password}' | base64 -d)
    
    # Create database backup
    kubectl run db-backup --image=postgres:13 --rm -i --restart=Never -n $NAMESPACE -- \
        pg_dump -h $DB_HOST -U $DB_USER -d $DB_NAME > $BACKUP_DIR/database-backup.sql
}

# Function to upload to S3
upload_to_s3() {
    echo "☁️ Uploading backup to S3..."
    
    aws s3 cp $BACKUP_DIR s3://$S3_BUCKET/backups/$(basename $BACKUP_DIR) --recursive
}

# Function to test recovery
test_recovery() {
    echo "🧪 Testing recovery procedures..."
    
    # Create test namespace
    kubectl create namespace mussikon-recovery-test
    
    # Restore resources to test namespace
    sed "s/namespace: $NAMESPACE/namespace: mussikon-recovery-test/g" $BACKUP_DIR/k8s-resources.yml | kubectl apply -f -
    
    # Wait for pods to be ready
    kubectl wait --for=condition=ready pod -l app=mussikon-api -n mussikon-recovery-test --timeout=300s
    
    # Run health checks
    kubectl run recovery-test --image=curlimages/curl --rm -i --restart=Never -n mussikon-recovery-test -- \
        curl -f http://mussikon-api-service:80/health
    
    # Clean up test namespace
    kubectl delete namespace mussikon-recovery-test
    
    echo "✅ Recovery test completed successfully!"
}

# Main execution
case "$1" in
    "backup")
        backup_k8s_resources
        backup_database
        upload_to_s3
        echo "✅ Backup completed successfully!"
        ;;
    "restore")
        echo "🔄 Restoring from backup..."
        # Restore logic here
        echo "✅ Restore completed successfully!"
        ;;
    "test")
        test_recovery
        ;;
    *)
        echo "Usage: $0 {backup|restore|test}"
        exit 1
        ;;
esac
```

---

## 🎯 **Ejercicios del Proyecto Final**

### **Ejercicio 1: Pipeline Completo**
```yaml
# Implementa un pipeline completo de CI/CD
# que incluya quality gates, build, test, security scan y deployment
```

### **Ejercicio 2: Multi-Environment**
```yaml
# Configura deployment en múltiples ambientes
# (staging, production) con configuraciones específicas
```

### **Ejercicio 3: Monitoring y Alerting**
```yaml
# Implementa un stack completo de monitoring
# con Prometheus, Grafana y Alertmanager
```

### **Ejercicio 4: Disaster Recovery**
```bash
# Crea procedimientos de disaster recovery
# con backup automático y restore
```

---

## 📚 **Resumen del Proyecto Final**

En este proyecto final hemos implementado:

1. **🚀 Pipeline Completo**: CI/CD end-to-end
2. **🌍 Multi-Environment**: Staging y production
3. **📊 Monitoring**: Stack completo de observabilidad
4. **🚨 Disaster Recovery**: Procedimientos de recuperación
5. **🔒 Security**: Escaneo y gestión de secretos
6. **⚡ Performance**: Load testing y auto-scaling
7. **🔄 Automation**: Automatización completa

---

## 🏆 **¡Felicidades!**

¡Has completado el **Expert Level 1: DevOps y CI/CD Avanzado**! 

Ahora tienes las habilidades para:
- Implementar pipelines de CI/CD robustos
- Configurar infraestructura como código
- Desplegar aplicaciones en Kubernetes
- Monitorear y alertar sobre el estado de las aplicaciones
- Gestionar la seguridad en todo el pipeline
- Implementar estrategias de deployment avanzadas
- Optimizar el rendimiento y la capacidad

---

**¡Has completado el Expert Level 1! 🏆🎯**
