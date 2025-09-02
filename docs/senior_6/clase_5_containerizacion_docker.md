# 🚀 Clase 5: Containerización con Docker

## 🧭 Navegación
- **⬅️ Anterior**: [Clase 4: Seguridad de APIs y Microservicios](clase_4_seguridad_apis.md)
- **🏠 [Volver al Módulo 6](../README.md)**
- **➡️ Siguiente**: [Clase 6: Orquestación con Kubernetes](clase_6_orquestacion_kubernetes.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)**

---

## 🎯 Objetivos de la Clase

Al finalizar esta clase, serás capaz de:
- Crear Dockerfiles optimizados para aplicaciones .NET
- Implementar multi-stage builds
- Configurar Docker Compose para desarrollo
- Optimizar imágenes de Docker

---

## 📚 Contenido Teórico

### 5.1 Dockerfiles Optimizados para .NET

#### Dockerfile Básico Optimizado

```dockerfile
# Multi-stage build para .NET 8
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Stage de build
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copia archivos de proyecto
COPY ["src/MyApp.API/MyApp.API.csproj", "src/MyApp.API/"]
COPY ["src/MyApp.Core/MyApp.Core.csproj", "src/MyApp.Core/"]
COPY ["src/MyApp.Infrastructure/MyApp.Infrastructure.csproj", "src/MyApp.Infrastructure/"]
RUN dotnet restore "src/MyApp.API/MyApp.API.csproj"

# Copia todo el código fuente
COPY . .
WORKDIR "/src/src/MyApp.API"

# Build de la aplicación
RUN dotnet build "MyApp.API.csproj" -c Release -o /app/build

# Stage de publish
FROM build AS publish
RUN dotnet publish "MyApp.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage final
FROM base AS final
WORKDIR /app

# Instala dependencias del sistema
RUN apk add --no-cache icu-libs

# Variables de entorno para .NET
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV ASPNETCORE_URLS=http://+:80;https://+:443

# Copia la aplicación publicada
COPY --from=publish /app/publish .

# Usuario no-root para seguridad
RUN addgroup -g 1001 -S appgroup && \
    adduser -u 1001 -S appuser -G appgroup
USER appuser

ENTRYPOINT ["dotnet", "MyApp.API.dll"]
```

#### Dockerfile para Microservicios

```dockerfile
# Dockerfile para microservicio específico
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080

# Variables de entorno específicas del microservicio
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Stage de build
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copia solo el proyecto del microservicio
COPY ["src/OrderService/OrderService.csproj", "src/OrderService/"]
RUN dotnet restore "src/OrderService/OrderService.csproj"

COPY . .
WORKDIR "/src/src/OrderService"

# Build optimizado
RUN dotnet build "OrderService.csproj" -c Release -o /app/build

# Stage de publish
FROM build AS publish
RUN dotnet publish "OrderService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage final
FROM base AS final
WORKDIR /app

# Copia la aplicación
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "OrderService.dll"]
```

### 5.2 Multi-Stage Builds Avanzados

#### Dockerfile con Testing y Análisis

```dockerfile
# Multi-stage build completo con testing
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS restore
WORKDIR /src
COPY ["src/MyApp.API/MyApp.API.csproj", "src/MyApp.API/"]
COPY ["src/MyApp.Tests/MyApp.Tests.csproj", "src/MyApp.Tests/"]
RUN dotnet restore "src/MyApp.API/MyApp.API.csproj"
RUN dotnet restore "src/MyApp.Tests/MyApp.Tests.csproj"

# Stage de build
FROM restore AS build
COPY . .
WORKDIR "/src/src/MyApp.API"
RUN dotnet build "MyApp.API.csproj" -c Release -o /app/build

# Stage de testing
FROM build AS test
WORKDIR "/src/src/MyApp.Tests"
RUN dotnet test "MyApp.Tests.csproj" -c Release --no-build --verbosity normal

# Stage de análisis de código
FROM build AS analyze
WORKDIR "/src"
RUN dotnet tool install --global dotnet-format
RUN dotnet format --verify-no-changes
RUN dotnet tool install --global dotnet-ef
RUN dotnet ef migrations list --project src/MyApp.Infrastructure

# Stage de publish
FROM build AS publish
WORKDIR "/src/src/MyApp.API"
RUN dotnet publish "MyApp.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage final
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyApp.API.dll"]
```

### 5.3 Docker Compose para Desarrollo

#### docker-compose.yml Completo

```yaml
version: '3.8'

services:
  # API Principal
  api:
    build:
      context: .
      dockerfile: Dockerfile
      target: final
    ports:
      - "5000:80"
      - "5001:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=db;Database=MyAppDb;User=sa;Password=Your_password123
      - Redis__ConnectionString=redis:6379
    depends_on:
      - db
      - redis
    volumes:
      - ./logs:/app/logs
    networks:
      - app-network

  # Base de Datos SQL Server
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=Your_password123
      - MSSQL_PID=Developer
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
      - ./scripts:/scripts
    networks:
      - app-network
    healthcheck:
      test: ["CMD-SHELL", "/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Your_password123 -Q 'SELECT 1'"]
      interval: 30s
      timeout: 10s
      retries: 3

  # Redis Cache
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    networks:
      - app-network
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 30s
      timeout: 10s
      retries: 3

  # RabbitMQ para mensajería
  rabbitmq:
    image: rabbitmq:3-management-alpine
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      - RABBITMQ_DEFAULT_USER=admin
      - RABBITMQ_DEFAULT_PASS=admin123
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    networks:
      - app-network

  # Elasticsearch para logging
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
      - app-network

  # Kibana para visualización
  kibana:
    image: docker.elastic.co/kibana/kibana:8.11.0
    environment:
      - ELASTICSEARCH_HOSTS=http://elasticsearch:9200
    ports:
      - "5601:5601"
    depends_on:
      - elasticsearch
    networks:
      - app-network

  # Nginx como reverse proxy
  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx/nginx.conf:/etc/nginx/nginx.conf:ro
      - ./nginx/ssl:/etc/nginx/ssl:ro
    depends_on:
      - api
    networks:
      - app-network

volumes:
  sqlserver_data:
  redis_data:
  rabbitmq_data:
  elasticsearch_data:

networks:
  app-network:
    driver: bridge
```

#### docker-compose.override.yml para Desarrollo

```yaml
version: '3.8'

services:
  api:
    build:
      target: build
    volumes:
      - .:/src
      - /src/bin
      - /src/obj
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:80
    command: dotnet watch run --urls http://+:80

  db:
    ports:
      - "1433:1433"
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=Your_password123

  redis:
    ports:
      - "6379:6379"

  rabbitmq:
    ports:
      - "5672:5672"
      - "15672:15672"

  elasticsearch:
    ports:
      - "9200:9200"

  kibana:
    ports:
      - "5601:5601"
```

### 5.4 Optimización de Imágenes

#### Dockerfile con Optimizaciones

```dockerfile
# Dockerfile optimizado para producción
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base

# Instala dependencias del sistema de una vez
RUN apk add --no-cache \
    icu-libs \
    && rm -rf /var/cache/apk/*

WORKDIR /app
EXPOSE 80

# Stage de build optimizado
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

# Instala herramientas de build
RUN apk add --no-cache \
    git \
    && rm -rf /var/cache/apk/*

WORKDIR /src

# Copia archivos de proyecto primero para aprovechar cache de Docker
COPY ["src/MyApp.API/MyApp.API.csproj", "src/MyApp.API/"]
COPY ["src/MyApp.Core/MyApp.Core.csproj", "src/MyApp.Core/"]
COPY ["src/MyApp.Infrastructure/MyApp.Infrastructure.csproj", "src/MyApp.Infrastructure/"]
COPY ["MyApp.sln", "./"]

# Restore de dependencias
RUN dotnet restore "MyApp.sln" --verbosity quiet

# Copia código fuente
COPY . .

# Build optimizado
WORKDIR "/src/src/MyApp.API"
RUN dotnet build "MyApp.API.csproj" -c Release -o /app/build --no-restore

# Stage de publish optimizado
FROM build AS publish
RUN dotnet publish "MyApp.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-build \
    --no-restore \
    /p:UseAppHost=false \
    /p:PublishTrimmed=true \
    /p:PublishSingleFile=true

# Stage final optimizado
FROM base AS final

# Configuración de seguridad
RUN addgroup -g 1001 -S appgroup && \
    adduser -u 1001 -S appuser -G appgroup

# Variables de entorno optimizadas
ENV DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_URLS=http://+:80 \
    ASPNETCORE_ENVIRONMENT=Production

# Copia solo los archivos necesarios
COPY --from=publish /app/publish .

# Cambia a usuario no-root
USER appuser

# Health check optimizado
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "MyApp.API.dll"]
```

#### .dockerignore Optimizado

```dockerignore
# Archivos de desarrollo
**/.dockerignore
**/.env
**/.git
**/.gitignore
**/.project
**/.settings
**/.toolstarget
**/.vs
**/.vscode
**/*.*proj.user
**/*.dbmdl
**/*.jfm
**/azds.yaml
**/bin
**/charts
**/docker-compose*
**/Dockerfile*
**/node_modules
**/npm-debug.log
**/obj
**/secrets.dev.yaml
**/values.dev.yaml
LICENSE
README.md

# Archivos temporales
**/bin
**/obj
**/out
**/publish
**/logs
**/*.log
**/temp
**/tmp

# Archivos de configuración local
**/appsettings.Development.json
**/appsettings.Local.json
**/appsettings.*.Development.json
**/appsettings.*.Local.json

# Archivos de testing
**/*.Tests
**/*.Test
**/test
**/tests
**/TestResults

# Archivos de documentación
**/docs
**/documentation
**/*.md
**/*.txt

# Archivos de IDE
**/.idea
**/.vs
**/.vscode
**/*.suo
**/*.user
**/*.userosscache
**/*.sln.docstates
```

---

## 💻 Ejercicios Prácticos

### Ejercicio 1: Crear Dockerfile para Microservicio

Crea un Dockerfile optimizado para un microservicio de usuarios:

```dockerfile
# Implementa:
# - Multi-stage build
# - Optimizaciones de seguridad
# - Health checks
# - Variables de entorno
```

### Ejercicio 2: Docker Compose para Microservicios

Implementa un docker-compose.yml para una arquitectura de microservicios:

```yaml
# Incluye:
# - API Gateway
# - Microservicios (Users, Orders, Products)
# - Base de datos por servicio
# - Redis compartido
# - RabbitMQ para mensajería
```

---

## 🔍 Casos de Uso Reales

### 1. Pipeline CI/CD con Docker

```yaml
# .github/workflows/docker-build.yml
name: Docker Build and Push

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
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
        push: true
        tags: |
          myapp:latest
          myapp:${{ github.sha }}
        cache-from: type=gha
        cache-to: type=gha,mode=max
```

### 2. Docker para Testing

```yaml
# docker-compose.test.yml
version: '3.8'

services:
  test-db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=Test_password123
    ports:
      - "1434:1433"
    
  test-redis:
    image: redis:7-alpine
    ports:
      - "6380:6379"
    
  test-rabbitmq:
    image: rabbitmq:3-management-alpine
    ports:
      - "5673:5672"
      - "15673:15672"
```

---

## 📊 Métricas de Docker

### KPIs de Containerización

1. **Image Size**: Tamaño de las imágenes Docker
2. **Build Time**: Tiempo de construcción de imágenes
3. **Layer Cache Hit Rate**: Tasa de acierto del cache de capas
4. **Security Vulnerabilities**: Vulnerabilidades de seguridad detectadas
5. **Resource Usage**: Uso de recursos de los contenedores

---

## 🎯 Resumen de la Clase

En esta clase hemos aprendido:

✅ **Dockerfiles Optimizados**: Creación de imágenes eficientes para .NET
✅ **Multi-Stage Builds**: Construcción en etapas para optimización
✅ **Docker Compose**: Orquestación de servicios para desarrollo
✅ **Optimización de Imágenes**: Técnicas para reducir tamaño y mejorar rendimiento
✅ **Casos de Uso Reales**: Implementación en entornos de producción

---

## 🚀 Próximos Pasos

En la siguiente clase aprenderemos sobre:
- **Orquestación con Kubernetes**
- Manifests de Kubernetes
- Deployments y servicios
- ConfigMaps y Secrets

---

## 🔗 Enlaces de Referencia

- [Docker for .NET](https://docs.microsoft.com/en-us/dotnet/core/docker/)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [Multi-Stage Builds](https://docs.docker.com/develop/dev-best-practices/dockerfile_best-practices/)
- [Docker Compose](https://docs.docker.com/compose/)
