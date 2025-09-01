# Clase 8: Despliegue y Orquestación

## Navegación
- [← Clase 7: Testing de Microservicios](clase_7_testing_microservicios.md)
- [Clase 9: Seguridad en Microservicios →](clase_9_seguridad_microservicios.md)
- [← Volver al README del módulo](README.md)
- [← Volver al módulo anterior (senior_3)](../senior_3/README.md)
- [→ Ir al siguiente módulo (senior_5)](../senior_5/README.md)

## Objetivos de Aprendizaje
- Comprender containerización con Docker
- Implementar orquestación con Kubernetes
- Configurar CI/CD pipelines
- Implementar estrategias de despliegue
- Gestionar configuraciones y secretos

## Contenido Teórico

### 1. Containerización con Docker

```dockerfile
# Dockerfile multi-stage optimizado
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["UserService/UserService.csproj", "UserService/"]
RUN dotnet restore "UserService/UserService.csproj"
COPY . .
RUN dotnet build "UserService/UserService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
HEALTHCHECK --interval=30s --timeout=3s CMD curl -f http://localhost/health || exit 1
ENTRYPOINT ["dotnet", "UserService.dll"]
```

### 2. Docker Compose para Desarrollo

```yaml
# docker-compose.yml
version: '3.8'
services:
  postgres:
    image: postgres:15-alpine
    environment:
      POSTGRES_DB: userservice
      POSTGRES_USER: devuser
      POSTGRES_PASSWORD: devpass
    ports: ["5432:5432"]
    
  redis:
    image: redis:7-alpine
    ports: ["6379:6379"]
    
  userservice:
    build: .
    ports: ["5000:80"]
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=userservice;Username=devuser;Password=devpass
    depends_on: [postgres, redis]
```

### 3. Kubernetes Manifests

```yaml
# deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: userservice
spec:
  replicas: 3
  selector:
    matchLabels:
      app: userservice
  template:
    metadata:
      labels:
        app: userservice
    spec:
      containers:
      - name: userservice
        image: userservice:latest
        ports:
        - containerPort: 80
        livenessProbe:
          httpGet:
            path: /health/live
            port: 80
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 80
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"

---
# service.yaml
apiVersion: v1
kind: Service
metadata:
  name: userservice-service
spec:
  selector:
    app: userservice
  ports:
  - protocol: TCP
    port: 80
    targetPort: 80
  type: ClusterIP

---
# hpa.yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: userservice-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: userservice
  minReplicas: 3
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
```

### 4. CI/CD Pipeline

```yaml
# .github/workflows/ci-cd.yml
name: CI/CD Pipeline
on:
  push:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    - name: Test
      run: dotnet test --configuration Release

  build-and-deploy:
    runs-on: ubuntu-latest
    needs: test
    steps:
    - uses: actions/checkout@v4
    - name: Build and push Docker image
      uses: docker/build-push-action@v5
      with:
        context: .
        push: true
        tags: userservice:latest
    - name: Deploy to Kubernetes
      run: |
        kubectl set image deployment/userservice userservice=userservice:latest
        kubectl rollout status deployment/userservice
```

### 5. Estrategias de Despliegue

```yaml
# blue-green-deployment.yaml
apiVersion: argoproj.io/v1alpha1
kind: Rollout
metadata:
  name: userservice-rollout
spec:
  replicas: 5
  strategy:
    blueGreen:
      activeService: userservice-active
      previewService: userservice-preview
      autoPromotionEnabled: false
      scaleDownDelaySeconds: 30
  selector:
    matchLabels:
      app: userservice
  template:
    metadata:
      labels:
        app: userservice
    spec:
      containers:
      - name: userservice
        image: userservice:latest
        ports:
        - containerPort: 80

---
# canary-deployment.yaml
apiVersion: argoproj.io/v1alpha1
kind: Rollout
metadata:
  name: userservice-canary
spec:
  replicas: 10
  strategy:
    canary:
      steps:
      - setWeight: 10
      - pause: {duration: 60s}
      - setWeight: 20
      - pause: {duration: 60s}
      - setWeight: 100
  selector:
    matchLabels:
      app: userservice
  template:
    spec:
      containers:
      - name: userservice
        image: userservice:latest
```

## Ejercicios Prácticos

### Ejercicio 1: Containerización
Crea un Dockerfile optimizado para un microservicio .NET.

### Ejercicio 2: Kubernetes
Implementa manifiestos Kubernetes para un microservicio.

### Ejercicio 3: CI/CD
Configura un pipeline de CI/CD básico.

## Proyecto Integrador
Implementa un sistema de despliegue completo que incluya:
- Dockerfile optimizado
- Docker Compose para desarrollo
- Manifiestos Kubernetes
- Pipeline de CI/CD
- Estrategias de despliegue

## Recursos Adicionales
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Argo Rollouts](https://argoproj.github.io/argo-rollouts/)
- [GitHub Actions](https://docs.github.com/en/actions)
