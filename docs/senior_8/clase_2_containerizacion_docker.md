# 🐳 Clase 2: Containerización y Docker

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 1: Implementación Práctica de la Arquitectura](../senior_8/clase_1_implementacion_arquitectura.md)
- **🏠 Inicio del Módulo**: [Módulo 15: Sistemas Avanzados y Distribuidos](../senior_8/README.md)
- **➡️ Siguiente**: [Clase 3: CI/CD con GitHub Actions](../senior_8/clase_3_cicd_github_actions.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** Dockerfiles multi-stage optimizados
2. **Configurar** Docker Compose para desarrollo
3. **Desarrollar** Docker Compose para producción
4. **Aplicar** mejores prácticas de Docker
5. **Optimizar** imágenes y contenedores

---

## 🐳 **Dockerfile Multi-Stage**

### **Dockerfile Optimizado para Producción**

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

### **Dockerfile para Desarrollo**

```dockerfile
# Dockerfile para desarrollo con debugging
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS development
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

# Exponer puertos
EXPOSE 5000
EXPOSE 5001

# Comando para desarrollo
CMD ["dotnet", "run", "--project", "MusicalMatching.API", "--urls", "http://0.0.0.0:5000;https://0.0.0.0:5001"]
```

---

## 🚀 **Docker Compose para Desarrollo**

### **Configuración Completa de Desarrollo**

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
      - JwtSettings__SecretKey=DevelopmentSecretKey12345678901234567890
      - JwtSettings__Issuer=MusicalMatching.Dev
      - JwtSettings__Audience=MusicalMatching.Dev
    depends_on:
      - db
      - redis
    volumes:
      - ./logs:/app/logs
      - ./src:/src
    networks:
      - musical-network
    profiles:
      - development

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
      - ./scripts/db:/docker-entrypoint-initdb.d
    networks:
      - musical-network
    profiles:
      - development

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    networks:
      - musical-network
    profiles:
      - development

  adminer:
    image: adminer
    ports:
      - "8080:8080"
    depends_on:
      - db
    networks:
      - musical-network
    profiles:
      - development

  jaeger:
    image: jaegertracing/all-in-one:latest
    ports:
      - "16686:16686"
      - "14268:14268"
      - "6831:6831/udp"
    environment:
      - COLLECTOR_OTLP_ENABLED=true
    networks:
      - musical-network
    profiles:
      - development

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
      - musical-network
    profiles:
      - development

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
      - musical-network
    profiles:
      - development

volumes:
  sqlserver_data:
  redis_data:
  prometheus_data:
  grafana_data:

networks:
  musical-network:
    driver: bridge
```

---

## 🏭 **Docker Compose para Producción**

### **Configuración de Producción Optimizada**

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
      - JwtSettings__Issuer=${JWT_ISSUER}
      - JwtSettings__Audience=${JWT_AUDIENCE}
      - Elasticsearch__Uri=${ELASTICSEARCH_URI}
      - Jaeger__Host=${JAEGER_HOST}
      - Jaeger__Port=${JAEGER_PORT}
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
      update_config:
        parallelism: 1
        delay: 10s
        order: start-first
      restart_policy:
        condition: on-failure
        delay: 5s
        max_attempts: 3
        window: 120s

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
      placement:
        constraints:
          - node.role == manager

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
      placement:
        constraints:
          - node.role == manager

  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx/nginx.conf:/etc/nginx/nginx.conf:ro
      - ./ssl:/etc/nginx/ssl:ro
      - ./logs/nginx:/var/log/nginx
    depends_on:
      - api
    networks:
      - musical-network
    restart: unless-stopped
    deploy:
      replicas: 2
      resources:
        limits:
          cpus: '0.5'
          memory: 256M

  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.11.0
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false
      - "ES_JAVA_OPTS=-Xms512m -Xmx512m"
    volumes:
      - elasticsearch_data:/usr/share/elasticsearch/data
    networks:
      - musical-network
    restart: unless-stopped
    deploy:
      resources:
        limits:
          cpus: '1.0'
          memory: 1G

  kibana:
    image: docker.elastic.co/kibana/kibana:8.11.0
    environment:
      - ELASTICSEARCH_HOSTS=http://elasticsearch:9200
    ports:
      - "5601:5601"
    depends_on:
      - elasticsearch
    networks:
      - musical-network
    restart: unless-stopped
    deploy:
      resources:
        limits:
          cpus: '0.5'
          memory: 512M

volumes:
  sqlserver_data:
    driver: local
  redis_data:
    driver: local
  elasticsearch_data:
    driver: local

networks:
  musical-network:
    driver: overlay
    attachable: true
```

---

## 🔧 **Configuración de Nginx**

### **Reverse Proxy y Load Balancer**

```nginx
# nginx/nginx.conf
events {
    worker_connections 1024;
}

http {
    include       /etc/nginx/mime.types;
    default_type  application/octet-stream;

    # Logging
    log_format main '$remote_addr - $remote_user [$time_local] "$request" '
                    '$status $body_bytes_sent "$http_referer" '
                    '"$http_user_agent" "$http_x_forwarded_for"';

    access_log /var/log/nginx/access.log main;
    error_log /var/log/nginx/error.log warn;

    # Gzip compression
    gzip on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_proxied any;
    gzip_comp_level 6;
    gzip_types
        text/plain
        text/css
        text/xml
        text/javascript
        application/json
        application/javascript
        application/xml+rss
        application/atom+xml
        image/svg+xml;

    # Rate limiting
    limit_req_zone $binary_remote_addr zone=api:10m rate=10r/s;
    limit_req_zone $binary_remote_addr zone=login:10m rate=5r/m;

    # Upstream API servers
    upstream api_servers {
        least_conn;
        server api:80 max_fails=3 fail_timeout=30s;
        keepalive 32;
    }

    # HTTP to HTTPS redirect
    server {
        listen 80;
        server_name _;
        return 301 https://$host$request_uri;
    }

    # HTTPS server
    server {
        listen 443 ssl http2;
        server_name _;

        # SSL configuration
        ssl_certificate /etc/nginx/ssl/cert.pem;
        ssl_certificate_key /etc/nginx/ssl/key.pem;
        ssl_protocols TLSv1.2 TLSv1.3;
        ssl_ciphers ECDHE-RSA-AES128-GCM-SHA256:ECDHE-RSA-AES256-GCM-SHA384;
        ssl_prefer_server_ciphers off;
        ssl_session_cache shared:SSL:10m;
        ssl_session_timeout 10m;

        # Security headers
        add_header X-Frame-Options DENY;
        add_header X-Content-Type-Options nosniff;
        add_header X-XSS-Protection "1; mode=block";
        add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;

        # API endpoints
        location /api/ {
            limit_req zone=api burst=20 nodelay;
            
            proxy_pass http://api_servers;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            
            proxy_connect_timeout 30s;
            proxy_send_timeout 30s;
            proxy_read_timeout 30s;
            
            proxy_buffering off;
            proxy_request_buffering off;
        }

        # SignalR endpoints
        location /hubs/ {
            limit_req zone=api burst=50 nodelay;
            
            proxy_pass http://api_servers;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            
            # WebSocket support
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection "upgrade";
            
            proxy_connect_timeout 30s;
            proxy_send_timeout 30s;
            proxy_read_timeout 30s;
        }

        # Health check endpoint
        location /health {
            access_log off;
            return 200 "healthy\n";
            add_header Content-Type text/plain;
        }

        # Static files
        location / {
            root /usr/share/nginx/html;
            try_files $uri $uri/ /index.html;
            
            # Cache static assets
            location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg)$ {
                expires 1y;
                add_header Cache-Control "public, immutable";
            }
        }
    }
}
```

---

## 📝 **Archivos de Configuración**

### **Variables de Entorno**

```bash
# .env.production
DB_CONNECTION_STRING=Server=db;Database=MusicalMatching;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=true
REDIS_CONNECTION_STRING=redis:6379,password=YourRedisPassword123!
JWT_SECRET_KEY=YourSuperSecretJWTKey123456789012345678901234567890
JWT_ISSUER=MusicalMatching.Production
JWT_AUDIENCE=MusicalMatching.Production
ELASTICSEARCH_URI=http://elasticsearch:9200
JAEGER_HOST=jaeger
JAEGER_PORT=6831
DB_PASSWORD=YourStrongPassword123!
REDIS_PASSWORD=YourRedisPassword123!
```

### **Docker Ignore**

```dockerignore
# .dockerignore
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
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Dockerfile Multi-Stage**
```dockerfile
# Implementa:
# - Dockerfile multi-stage optimizado
# - Usuario no-root para seguridad
# - Health checks
# - Optimización de capas
```

### **Ejercicio 2: Docker Compose Desarrollo**
```yaml
# Crea:
# - Docker Compose para desarrollo
# - Servicios de base de datos y cache
# - Herramientas de monitoreo
# - Volúmenes y redes
```

### **Ejercicio 3: Docker Compose Producción**
```yaml
# Implementa:
# - Docker Compose para producción
# - Nginx como reverse proxy
# - Elasticsearch y Kibana
# - Configuración de seguridad
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **🐳 Dockerfiles Multi-Stage**: Optimización de imágenes y seguridad
2. **🚀 Docker Compose Desarrollo**: Entorno completo para desarrollo
3. **🏭 Docker Compose Producción**: Configuración escalable para producción
4. **🔧 Nginx Configuration**: Reverse proxy y load balancing
5. **📝 Archivos de Configuración**: Variables de entorno y .dockerignore

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **CI/CD con GitHub Actions**, implementando pipelines automáticos de build, test y deployment.

---

**¡Has completado la segunda clase del Módulo 15! 🐳🚀**

