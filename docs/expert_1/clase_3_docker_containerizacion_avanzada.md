# 🐳 Clase 3: Docker y Containerización Avanzada

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 2: GitHub Actions Avanzado](../expert_1/clase_2_github_actions_avanzado.md)
- **🏠 Inicio del Módulo**: [Expert Level 1: DevOps y CI/CD Avanzado](../expert_1/README.md)
- **➡️ Siguiente**: [Clase 4: Kubernetes y Orquestación](../expert_1/clase_4_kubernetes_orquestacion.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Dominar** multi-stage builds y optimización
2. **Implementar** Docker Compose para desarrollo
3. **Configurar** container security y best practices
4. **Gestionar** registry management y scanning
5. **Optimizar** performance y tamaño de imágenes

---

## 🏗️ **Multi-stage Builds y Optimización**

### **Dockerfile Optimizado para .NET**

```dockerfile
# Dockerfile para MussikOn API
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/MusicalMatching.API/MusicalMatching.API.csproj", "src/MusicalMatching.API/"]
COPY ["src/MusicalMatching.Application/MusicalMatching.Application.csproj", "src/MusicalMatching.Application/"]
COPY ["src/MusicalMatching.Domain/MusicalMatching.Domain.csproj", "src/MusicalMatching.Domain/"]
COPY ["src/MusicalMatching.Infrastructure/MusicalMatching.Infrastructure.csproj", "src/MusicalMatching.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/MusicalMatching.API/MusicalMatching.API.csproj"

# Copy source code
COPY . .

# Build application
RUN dotnet build "src/MusicalMatching.API/MusicalMatching.API.csproj" -c Release -o /app/build

# Stage 2: Test
FROM build AS test
WORKDIR /src
RUN dotnet test --no-build -c Release --logger trx --results-directory /testresults

# Stage 3: Publish
FROM build AS publish
RUN dotnet publish "src/MusicalMatching.API/MusicalMatching.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# Create non-root user
RUN addgroup -g 1001 -S appgroup && \
    adduser -S appuser -u 1001 -G appgroup

# Install necessary packages
RUN apk add --no-cache \
    ca-certificates \
    tzdata \
    && rm -rf /var/cache/apk/*

# Copy published application
COPY --from=publish /app/publish .

# Set ownership
RUN chown -R appuser:appgroup /app
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Start application
ENTRYPOINT ["dotnet", "MusicalMatching.API.dll"]
```

### **Dockerfile para Frontend (Blazor)**

```dockerfile
# Dockerfile para MussikOn Blazor Frontend
# Stage 1: Build
FROM node:18-alpine AS build-node
WORKDIR /src
COPY ["src/MusicalMatching.Web/package*.json", "./"]
RUN npm ci --only=production

# Stage 2: Build .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build-dotnet
WORKDIR /src
COPY ["src/MusicalMatching.Web/MusicalMatching.Web.csproj", "src/MusicalMatching.Web/"]
RUN dotnet restore "src/MusicalMatching.Web/MusicalMatching.Web.csproj"
COPY . .
RUN dotnet build "src/MusicalMatching.Web/MusicalMatching.Web.csproj" -c Release -o /app/build

# Stage 3: Publish
FROM build-dotnet AS publish
RUN dotnet publish "src/MusicalMatching.Web/MusicalMatching.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# Create non-root user
RUN addgroup -g 1001 -S appgroup && \
    adduser -S appuser -u 1001 -G appgroup

# Copy published application
COPY --from=publish /app/publish .

# Set ownership
RUN chown -R appuser:appgroup /app
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Start application
ENTRYPOINT ["dotnet", "MusicalMatching.Web.dll"]
```

---

## 🐙 **Docker Compose para Desarrollo**

### **Docker Compose Completo**

```yaml
# docker-compose.yml
version: '3.8'

services:
  # API Service
  api:
    build:
      context: .
      dockerfile: src/MusicalMatching.API/Dockerfile
      target: final
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=db;Database=MusicalMatching;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;
      - Redis__ConnectionString=redis:6379
    depends_on:
      - db
      - redis
    volumes:
      - ./logs:/app/logs
    networks:
      - mussikon-network
    restart: unless-stopped

  # Web Frontend
  web:
    build:
      context: .
      dockerfile: src/MusicalMatching.Web/Dockerfile
      target: final
    ports:
      - "5001:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - API__BaseUrl=http://api:8080
    depends_on:
      - api
    networks:
      - mussikon-network
    restart: unless-stopped

  # Database
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
      - MSSQL_PID=Express
    ports:
      - "1433:1433"
    volumes:
      - db_data:/var/opt/mssql
      - ./scripts:/docker-entrypoint-initdb.d
    networks:
      - mussikon-network
    restart: unless-stopped

  # Redis Cache
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    networks:
      - mussikon-network
    restart: unless-stopped
    command: redis-server --appendonly yes

  # RabbitMQ Message Broker
  rabbitmq:
    image: rabbitmq:3-management-alpine
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      - RABBITMQ_DEFAULT_USER=admin
      - RABBITMQ_DEFAULT_PASS=admin
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    networks:
      - mussikon-network
    restart: unless-stopped

  # Elasticsearch
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.11.0
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false
      - "ES_JAVA_OPTS=-Xms512m -Xmx512m"
    ports:
      - "9200:9200"
    volumes:
      - elasticsearch_data:/usr/share/elasticsearch/data
    networks:
      - mussikon-network
    restart: unless-stopped

  # Kibana
  kibana:
    image: docker.elastic.co/kibana/kibana:8.11.0
    ports:
      - "5601:5601"
    environment:
      - ELASTICSEARCH_HOSTS=http://elasticsearch:9200
    depends_on:
      - elasticsearch
    networks:
      - mussikon-network
    restart: unless-stopped

  # Prometheus
  prometheus:
    image: prom/prometheus:latest
    ports:
      - "9090:9090"
    volumes:
      - ./monitoring/prometheus.yml:/etc/prometheus/prometheus.yml
      - prometheus_data:/prometheus
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
      - '--storage.tsdb.path=/prometheus'
      - '--web.console.libraries=/etc/prometheus/console_libraries'
      - '--web.console.templates=/etc/prometheus/consoles'
      - '--storage.tsdb.retention.time=200h'
      - '--web.enable-lifecycle'
    networks:
      - mussikon-network
    restart: unless-stopped

  # Grafana
  grafana:
    image: grafana/grafana:latest
    ports:
      - "3000:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin
    volumes:
      - grafana_data:/var/lib/grafana
      - ./monitoring/grafana/dashboards:/etc/grafana/provisioning/dashboards
      - ./monitoring/grafana/datasources:/etc/grafana/provisioning/datasources
    depends_on:
      - prometheus
    networks:
      - mussikon-network
    restart: unless-stopped

volumes:
  db_data:
  redis_data:
  rabbitmq_data:
  elasticsearch_data:
  prometheus_data:
  grafana_data:

networks:
  mussikon-network:
    driver: bridge
```

### **Docker Compose para Testing**

```yaml
# docker-compose.test.yml
version: '3.8'

services:
  # Test Database
  test-db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=TestPassword123!
      - MSSQL_PID=Express
    ports:
      - "1434:1433"
    volumes:
      - ./scripts/test-data.sql:/docker-entrypoint-initdb.d/test-data.sql
    networks:
      - test-network

  # Test Redis
  test-redis:
    image: redis:7-alpine
    ports:
      - "6380:6379"
    networks:
      - test-network

  # Integration Tests
  integration-tests:
    build:
      context: .
      dockerfile: tests/MusicalMatching.IntegrationTests/Dockerfile
    environment:
      - ConnectionStrings__DefaultConnection=Server=test-db;Database=MusicalMatching_Test;User Id=sa;Password=TestPassword123!;TrustServerCertificate=true;
      - Redis__ConnectionString=test-redis:6379
    depends_on:
      - test-db
      - test-redis
    networks:
      - test-network
    command: dotnet test --logger trx --results-directory /testresults

networks:
  test-network:
    driver: bridge
```

---

## 🔒 **Container Security y Best Practices**

### **Security Hardened Dockerfile**

```dockerfile
# Security hardened Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base

# Install security updates
RUN apk update && apk upgrade && \
    apk add --no-cache \
    ca-certificates \
    tzdata \
    && rm -rf /var/cache/apk/*

# Create non-root user with specific UID/GID
RUN addgroup -g 1001 -S appgroup && \
    adduser -S appuser -u 1001 -G appgroup -h /app -s /bin/sh

# Set secure file permissions
RUN chmod 755 /app && \
    chown -R appuser:appgroup /app

WORKDIR /app

# Copy application with proper ownership
COPY --chown=appuser:appgroup --from=build /app/publish .

# Switch to non-root user
USER appuser

# Set security headers
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

# Health check with proper timeout
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

# Expose port
EXPOSE 8080

# Use exec form for better signal handling
ENTRYPOINT ["dotnet", "MusicalMatching.API.dll"]
```

### **Security Scanning Script**

```bash
#!/bin/bash
# security-scan.sh

set -e

echo "🔒 Starting container security scan..."

# Build image
docker build -t mussikon:security-test .

# Run Trivy vulnerability scanner
echo "🔍 Running Trivy vulnerability scan..."
docker run --rm -v /var/run/docker.sock:/var/run/docker.sock \
    aquasec/trivy image --severity HIGH,CRITICAL mussikon:security-test

# Run Docker Bench Security
echo "🔍 Running Docker Bench Security..."
docker run --rm --net host --pid host --userns host --cap-add audit_control \
    -e DOCKER_CONTENT_TRUST=$DOCKER_CONTENT_TRUST \
    -v /etc:/etc:ro \
    -v /usr/bin/containerd:/usr/bin/containerd:ro \
    -v /usr/bin/runc:/usr/bin/runc:ro \
    -v /usr/lib/systemd:/usr/lib/systemd:ro \
    -v /var/lib:/var/lib:ro \
    -v /var/run/docker.sock:/var/run/docker.sock:ro \
    --label docker_bench_security \
    docker/docker-bench-security

# Run Hadolint for Dockerfile linting
echo "🔍 Running Hadolint Dockerfile analysis..."
docker run --rm -i hadolint/hadolint < Dockerfile

echo "✅ Security scan completed!"
```

---

## 📦 **Registry Management y Scanning**

### **Multi-Architecture Builds**

```bash
#!/bin/bash
# build-multi-arch.sh

set -e

echo "🏗️ Building multi-architecture images..."

# Create and use buildx builder
docker buildx create --name multiarch --use

# Build for multiple architectures
docker buildx build \
    --platform linux/amd64,linux/arm64 \
    --tag mussikon/api:latest \
    --tag mussikon/api:v1.0.0 \
    --file src/MusicalMatching.API/Dockerfile \
    --push \
    .

# Build web frontend
docker buildx build \
    --platform linux/amd64,linux/arm64 \
    --tag mussikon/web:latest \
    --tag mussikon/web:v1.0.0 \
    --file src/MusicalMatching.Web/Dockerfile \
    --push \
    .

echo "✅ Multi-architecture build completed!"
```

### **Registry Scanning**

```yaml
# .github/workflows/registry-scan.yml
name: Registry Security Scan

on:
  push:
    tags:
      - 'v*'
  schedule:
    - cron: '0 2 * * 1' # Weekly on Monday

jobs:
  scan-registry:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v2

      - name: Login to Docker Hub
        uses: docker/login-action@v2
        with:
          username: ${{ secrets.DOCKER_USERNAME }}
          password: ${{ secrets.DOCKER_PASSWORD }}

      - name: Build and push
        uses: docker/build-push-action@v4
        with:
          context: .
          platforms: linux/amd64,linux/arm64
          push: true
          tags: |
            mussikon/api:latest
            mussikon/api:${{ github.ref_name }}

      - name: Run Trivy vulnerability scanner
        uses: aquasecurity/trivy-action@master
        with:
          image-ref: 'mussikon/api:latest'
          format: 'sarif'
          output: 'trivy-results.sarif'

      - name: Upload Trivy scan results
        uses: github/codeql-action/upload-sarif@v2
        with:
          sarif_file: 'trivy-results.sarif'
```

---

## ⚡ **Performance y Optimización**

### **Optimización de Imágenes**

```dockerfile
# Optimized Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

# Install only necessary packages
RUN apk add --no-cache \
    ca-certificates \
    && rm -rf /var/cache/apk/*

WORKDIR /src

# Copy only csproj files first for better layer caching
COPY ["src/MusicalMatching.API/MusicalMatching.API.csproj", "src/MusicalMatching.API/"]
COPY ["src/MusicalMatching.Application/MusicalMatching.Application.csproj", "src/MusicalMatching.Application/"]
COPY ["src/MusicalMatching.Domain/MusicalMatching.Domain.csproj", "src/MusicalMatching.Domain/"]
COPY ["src/MusicalMatching.Infrastructure/MusicalMatching.Infrastructure.csproj", "src/MusicalMatching.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/MusicalMatching.API/MusicalMatching.API.csproj" \
    --runtime alpine-x64 \
    --self-contained false

# Copy source code
COPY . .

# Build with optimizations
RUN dotnet build "src/MusicalMatching.API/MusicalMatching.API.csproj" \
    -c Release \
    -o /app/build \
    --no-restore \
    -p:PublishSingleFile=true \
    -p:PublishTrimmed=true \
    -p:TrimMode=link

# Publish with optimizations
FROM build AS publish
RUN dotnet publish "src/MusicalMatching.API/MusicalMatching.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-build \
    --self-contained false \
    -p:PublishSingleFile=true \
    -p:PublishTrimmed=true \
    -p:TrimMode=link

# Final stage with minimal runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final

# Install only necessary runtime packages
RUN apk add --no-cache \
    ca-certificates \
    tzdata \
    && rm -rf /var/cache/apk/*

WORKDIR /app

# Create non-root user
RUN addgroup -g 1001 -S appgroup && \
    adduser -S appuser -u 1001 -G appgroup

# Copy published application
COPY --from=publish /app/publish .

# Set ownership
RUN chown -R appuser:appgroup /app
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

# Start application
ENTRYPOINT ["dotnet", "MusicalMatching.API.dll"]
```

### **Docker Compose para Producción**

```yaml
# docker-compose.prod.yml
version: '3.8'

services:
  api:
    image: mussikon/api:latest
    ports:
      - "80:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}
      - Redis__ConnectionString=${REDIS_CONNECTION_STRING}
    deploy:
      replicas: 3
      resources:
        limits:
          cpus: '0.5'
          memory: 512M
        reservations:
          cpus: '0.25'
          memory: 256M
      restart_policy:
        condition: on-failure
        delay: 5s
        max_attempts: 3
        window: 120s
    networks:
      - mussikon-network
    logging:
      driver: "json-file"
      options:
        max-size: "10m"
        max-file: "3"

  web:
    image: mussikon/web:latest
    ports:
      - "443:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - API__BaseUrl=${API_BASE_URL}
    deploy:
      replicas: 2
      resources:
        limits:
          cpus: '0.5'
          memory: 512M
        reservations:
          cpus: '0.25'
          memory: 256M
    networks:
      - mussikon-network

  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx/nginx.conf:/etc/nginx/nginx.conf
      - ./nginx/ssl:/etc/nginx/ssl
    depends_on:
      - api
      - web
    networks:
      - mussikon-network

networks:
  mussikon-network:
    driver: bridge
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Multi-stage Build**
```dockerfile
# Crea un Dockerfile optimizado con multi-stage build
# para la aplicación MussikOn
```

### **Ejercicio 2: Docker Compose**
```yaml
# Configura un docker-compose.yml completo
# con todos los servicios necesarios
```

### **Ejercicio 3: Security Scanning**
```bash
# Implementa un script de security scanning
# para las imágenes Docker
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **🏗️ Multi-stage Builds**: Optimización de imágenes Docker
2. **🐙 Docker Compose**: Orquestación de servicios
3. **🔒 Container Security**: Mejores prácticas de seguridad
4. **📦 Registry Management**: Gestión de registros
5. **⚡ Performance**: Optimización y caching
6. **🔍 Security Scanning**: Escaneo de vulnerabilidades

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Kubernetes y Orquestación**, implementando deployments y servicios.

---

**¡Has completado la tercera clase del Expert Level 1! 🐳🎯**
