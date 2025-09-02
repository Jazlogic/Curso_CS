# 🚀 Clase 10: Proyecto Final Integrador

## 🧭 Navegación
- **⬅️ Anterior**: [Clase 9: Deployment y Estrategias](clase_9_deployment_estrategias.md)
- **🏠 [Volver al Módulo 6](../README.md)**
- **➡️ Siguiente**: [Módulo 14: Plataformas Empresariales](../senior_7/README.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)**

---

## 🎯 Objetivos de la Clase

Al finalizar esta clase, serás capaz de:
- Integrar todos los conceptos del módulo en un proyecto real
- Implementar una aplicación web optimizada para producción
- Configurar deployment completo en Kubernetes
- Aplicar todas las mejores prácticas aprendidas

---

## 📚 Contenido Teórico

### 10.1 Sistema de E-commerce Empresarial

#### Arquitectura del Sistema

```csharp
// Estructura del proyecto
MyApp/
├── src/
│   ├── MyApp.API/                 # API principal
│   ├── MyApp.Core/                # Dominio y lógica de negocio
│   ├── MyApp.Infrastructure/      # Implementaciones de infraestructura
│   ├── MyApp.Application/         # Casos de uso y servicios de aplicación
│   └── MyApp.Tests/               # Tests unitarios e integración
├── k8s/                           # Manifests de Kubernetes
├── docker/                        # Dockerfiles y docker-compose
├── scripts/                       # Scripts de deployment
└── docs/                          # Documentación
```

#### Configuración de la Aplicación

```csharp
// Program.cs
using Serilog;
using Prometheus;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Elasticsearch(new Uri("http://elasticsearch:9200"))
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting MyApp E-commerce API");
    
    // Configuración de servicios
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    
    // Configuración de base de datos
    builder.Services.AddDbContext<MyAppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    
    // Configuración de Redis
    builder.Services.AddStackExchangeRedisCache(options =>
        options.Configuration = builder.Configuration.GetConnectionString("Redis"));
    
    // Configuración de RabbitMQ
    builder.Services.AddMassTransit(x =>
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ"));
            cfg.ConfigureEndpoints(context);
        });
    });
    
    // Configuración de autenticación JWT
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = builder.Configuration["Jwt:Authority"];
            options.Audience = builder.Configuration["Jwt:Audience"];
        });
    
    // Configuración de health checks
    builder.Services.AddHealthChecks()
        .AddCheck<DatabaseHealthCheck>("database")
        .AddCheck<RedisHealthCheck>("redis")
        .AddCheck<RabbitMQHealthCheck>("rabbitmq");
    
    // Configuración de métricas Prometheus
    builder.Services.AddMetrics();
    
    // Configuración de rate limiting
    builder.Services.AddRateLimiter(options =>
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
    
    // Configuración de CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowedOrigins", policy =>
        {
            policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>())
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });
    
    var app = builder.Build();
    
    // Configuración del pipeline HTTP
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    
    // Middleware de seguridad
    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();
    app.UseMiddleware<MetricsMiddleware>();
    
    // Middleware estándar
    app.UseHttpsRedirection();
    app.UseCors("AllowedOrigins");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
    
    // Endpoints de health checks
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    
    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("critical")
    });
    
    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("self")
    });
    
    // Endpoint de métricas Prometheus
    app.MapMetrics("/metrics");
    
    // Controllers
    app.MapControllers();
    
    // Inicialización de la base de datos
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<MyAppDbContext>();
        await context.Database.MigrateAsync();
    }
    
    Log.Information("MyApp E-commerce API started successfully");
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

### 10.2 Controladores Optimizados

#### Productos Controller

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;
    private readonly IFeatureFlagService _featureFlags;
    
    public ProductsController(
        IProductService productService,
        ILogger<ProductsController> logger,
        IFeatureFlagService featureFlags)
    {
        _productService = productService;
        _logger = logger;
        _featureFlags = featureFlags;
    }
    
    [HttpGet]
    [EnableRateLimiting("ProductsPolicy")]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts(
        [FromQuery] ProductQueryParameters parameters)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Query"] = parameters,
            ["User"] = User.Identity?.Name
        });
        
        _logger.LogInformation("Getting products with parameters: {Parameters}", parameters);
        
        try
        {
            var products = await _productService.GetProductsAsync(parameters);
            
            // Métricas de negocio
            ApplicationMetrics.ProductsViewed.Inc();
            
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products");
            throw;
        }
    }
    
    [HttpGet("{id}")]
    [EnableRateLimiting("ProductDetailPolicy")]
    public async Task<ActionResult<ProductDetailDto>> GetProduct(int id)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["ProductId"] = id,
            ["User"] = User.Identity?.Name
        });
        
        _logger.LogInformation("Getting product with ID {ProductId}", id);
        
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found", id);
                return NotFound();
            }
            
            // Feature flag para recomendaciones
            if (await _featureFlags.IsFeatureEnabledAsync("product-recommendations", User.Identity?.Name))
            {
                var recommendations = await _productService.GetRecommendationsAsync(id);
                product.Recommendations = recommendations;
            }
            
            // Métricas de negocio
            ApplicationMetrics.ProductDetailViewed.Inc();
            
            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product with ID {ProductId}", id);
            throw;
        }
    }
    
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("ProductCreationPolicy")]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductRequest request)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["ProductName"] = request.Name,
            ["User"] = User.Identity?.Name
        });
        
        _logger.LogInformation("Creating product: {ProductName}", request.Name);
        
        try
        {
            var product = await _productService.CreateProductAsync(request);
            
            _logger.LogInformation("Product created successfully with ID {ProductId}", product.Id);
            
            // Métricas de negocio
            ApplicationMetrics.ProductsCreated.Inc();
            
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Product creation validation failed: {Errors}", 
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            return BadRequest(ex.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product: {ProductName}", request.Name);
            throw;
        }
    }
    
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("ProductUpdatePolicy")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["ProductId"] = id,
            ["User"] = User.Identity?.Name
        });
        
        _logger.LogInformation("Updating product with ID {ProductId}", id);
        
        try
        {
            await _productService.UpdateProductAsync(id, request);
            
            _logger.LogInformation("Product with ID {ProductId} updated successfully", id);
            
            return NoContent();
        }
        catch (NotFoundException)
        {
            _logger.LogWarning("Product with ID {ProductId} not found for update", id);
            return NotFound();
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Product update validation failed: {Errors}", 
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            return BadRequest(ex.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with ID {ProductId}", id);
            throw;
        }
    }
    
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("ProductDeletionPolicy")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["ProductId"] = id,
            ["User"] = User.Identity?.Name
        });
        
        _logger.LogInformation("Deleting product with ID {ProductId}", id);
        
        try
        {
            await _productService.DeleteProductAsync(id);
            
            _logger.LogInformation("Product with ID {ProductId} deleted successfully", id);
            
            return NoContent();
        }
        catch (NotFoundException)
        {
            _logger.LogWarning("Product with ID {ProductId} not found for deletion", id);
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product with ID {ProductId}", id);
            throw;
        }
    }
}
```

### 10.3 Servicios de Dominio

#### Product Service

```csharp
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<ProductService> _logger;
    
    public ProductService(
        IProductRepository productRepository,
        ICacheService cacheService,
        IEventPublisher eventPublisher,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }
    
    public async Task<PagedResult<ProductDto>> GetProductsAsync(ProductQueryParameters parameters)
    {
        var cacheKey = $"products:{parameters.GetCacheKey()}";
        
        // Intenta obtener del cache
        var cachedResult = await _cacheService.GetAsync<PagedResult<ProductDto>>(cacheKey);
        if (cachedResult != null)
        {
            _logger.LogDebug("Products retrieved from cache with key: {CacheKey}", cacheKey);
            return cachedResult;
        }
        
        // Obtiene de la base de datos
        var products = await _productRepository.GetProductsAsync(parameters);
        
        // Convierte a DTOs
        var productDtos = products.Items.Select(p => p.ToDto()).ToList();
        var result = new PagedResult<ProductDto>
        {
            Items = productDtos,
            TotalCount = products.TotalCount,
            PageNumber = products.PageNumber,
            PageSize = products.PageSize
        };
        
        // Guarda en cache
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(15));
        
        _logger.LogDebug("Products retrieved from database and cached with key: {CacheKey}", cacheKey);
        
        return result;
    }
    
    public async Task<ProductDetailDto> GetProductByIdAsync(int id)
    {
        var cacheKey = $"product:{id}";
        
        // Intenta obtener del cache
        var cachedProduct = await _cacheService.GetAsync<ProductDetailDto>(cacheKey);
        if (cachedProduct != null)
        {
            _logger.LogDebug("Product {ProductId} retrieved from cache", id);
            return cachedProduct;
        }
        
        // Obtiene de la base de datos
        var product = await _productRepository.GetProductByIdAsync(id);
        
        if (product == null)
        {
            return null;
        }
        
        // Convierte a DTO
        var productDto = product.ToDetailDto();
        
        // Guarda en cache
        await _cacheService.SetAsync(cacheKey, productDto, TimeSpan.FromMinutes(30));
        
        _logger.LogDebug("Product {ProductId} retrieved from database and cached", id);
        
        return productDto;
    }
    
    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request)
    {
        _logger.LogInformation("Creating product: {ProductName}", request.Name);
        
        // Valida la solicitud
        var validationResult = ValidateCreateProductRequest(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
        
        // Crea la entidad del producto
        var product = Product.Create(
            request.Name,
            request.Description,
            request.Price,
            request.CategoryId,
            request.StockQuantity,
            request.Sku
        );
        
        // Guarda en la base de datos
        var createdProduct = await _productRepository.CreateAsync(product);
        
        // Publica evento de dominio
        await _eventPublisher.PublishAsync(new ProductCreatedEvent
        {
            ProductId = createdProduct.Id,
            ProductName = createdProduct.Name,
            CategoryId = createdProduct.CategoryId,
            Timestamp = DateTime.UtcNow
        });
        
        // Invalida cache relacionado
        await InvalidateProductCacheAsync();
        
        _logger.LogInformation("Product {ProductName} created successfully with ID {ProductId}", 
            request.Name, createdProduct.Id);
        
        return createdProduct.ToDto();
    }
    
    public async Task UpdateProductAsync(int id, UpdateProductRequest request)
    {
        _logger.LogInformation("Updating product with ID {ProductId}", id);
        
        // Obtiene el producto existente
        var product = await _productRepository.GetProductByIdAsync(id);
        if (product == null)
        {
            throw new NotFoundException($"Product with ID {id} not found");
        }
        
        // Valida la solicitud
        var validationResult = ValidateUpdateProductRequest(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }
        
        // Actualiza el producto
        product.Update(
            request.Name,
            request.Description,
            request.Price,
            request.CategoryId,
            request.StockQuantity
        );
        
        // Guarda los cambios
        await _productRepository.UpdateAsync(product);
        
        // Publica evento de dominio
        await _eventPublisher.PublishAsync(new ProductUpdatedEvent
        {
            ProductId = product.Id,
            ProductName = product.Name,
            CategoryId = product.CategoryId,
            Timestamp = DateTime.UtcNow
        });
        
        // Invalida cache relacionado
        await InvalidateProductCacheAsync(product.Id);
        
        _logger.LogInformation("Product with ID {ProductId} updated successfully", id);
    }
    
    public async Task DeleteProductAsync(int id)
    {
        _logger.LogInformation("Deleting product with ID {ProductId}", id);
        
        // Obtiene el producto existente
        var product = await _productRepository.GetProductByIdAsync(id);
        if (product == null)
        {
            throw new NotFoundException($"Product with ID {id} not found");
        }
        
        // Verifica que no esté en uso
        if (await _productRepository.IsProductInUseAsync(id))
        {
            throw new InvalidOperationException($"Cannot delete product {id} as it is currently in use");
        }
        
        // Elimina el producto
        await _productRepository.DeleteAsync(id);
        
        // Publica evento de dominio
        await _eventPublisher.PublishAsync(new ProductDeletedEvent
        {
            ProductId = id,
            ProductName = product.Name,
            Timestamp = DateTime.UtcNow
        });
        
        // Invalida cache relacionado
        await InvalidateProductCacheAsync(id);
        
        _logger.LogInformation("Product with ID {ProductId} deleted successfully", id);
    }
    
    public async Task<IEnumerable<ProductDto>> GetRecommendationsAsync(int productId)
    {
        var cacheKey = $"product-recommendations:{productId}";
        
        // Intenta obtener del cache
        var cachedRecommendations = await _cacheService.GetAsync<IEnumerable<ProductDto>>(cacheKey);
        if (cachedRecommendations != null)
        {
            return cachedRecommendations;
        }
        
        // Obtiene recomendaciones de la base de datos
        var recommendations = await _productRepository.GetRecommendationsAsync(productId);
        
        // Convierte a DTOs
        var recommendationDtos = recommendations.Select(p => p.ToDto()).ToList();
        
        // Guarda en cache
        await _cacheService.SetAsync(cacheKey, recommendationDtos, TimeSpan.FromHours(1));
        
        return recommendationDtos;
    }
    
    private ValidationResult ValidateCreateProductRequest(CreateProductRequest request)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add("Product name is required");
        
        if (string.IsNullOrWhiteSpace(request.Sku))
            errors.Add("Product SKU is required");
        
        if (request.Price <= 0)
            errors.Add("Product price must be greater than zero");
        
        if (request.StockQuantity < 0)
            errors.Add("Product stock quantity cannot be negative");
        
        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
    
    private ValidationResult ValidateUpdateProductRequest(UpdateProductRequest request)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add("Product name is required");
        
        if (request.Price <= 0)
            errors.Add("Product price must be greater than zero");
        
        if (request.StockQuantity < 0)
            errors.Add("Product stock quantity cannot be negative");
        
        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
    
    private async Task InvalidateProductCacheAsync(int? productId = null)
    {
        if (productId.HasValue)
        {
            await _cacheService.RemoveAsync($"product:{productId.Value}");
            await _cacheService.RemoveAsync($"product-recommendations:{productId.Value}");
        }
        
        // Invalida cache de listas de productos
        var cacheKeys = await _cacheService.GetKeysAsync("products:*");
        foreach (var key in cacheKeys)
        {
            await _cacheService.RemoveAsync(key);
        }
    }
}
```

### 10.4 Configuración de Kubernetes

#### Deployment Principal

```yaml
# k8s/deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: myapp-api
  namespace: production
  labels:
    app: myapp-api
    version: v1.0.0
spec:
  replicas: 3
  selector:
    matchLabels:
      app: myapp-api
  template:
    metadata:
      labels:
        app: myapp-api
        version: v1.0.0
      annotations:
        prometheus.io/scrape: "true"
        prometheus.io/port: "80"
        prometheus.io/path: "/metrics"
    spec:
      serviceAccountName: myapp-service-account
      containers:
      - name: myapp-api
        image: myapp/api:latest
        ports:
        - containerPort: 80
          protocol: TCP
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ASPNETCORE_URLS
          value: "http://+:80"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: myapp-secrets
              key: connection-string
        - name: Redis__ConnectionString
          valueFrom:
            secretKeyRef:
              name: myapp-secrets
              key: redis-connection
        - name: RabbitMQ__ConnectionString
          valueFrom:
            secretKeyRef:
              name: myapp-secrets
              key: rabbitmq-connection
        - name: Jwt__SecretKey
          valueFrom:
            secretKeyRef:
              name: myapp-secrets
              key: jwt-secret
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
        securityContext:
          runAsNonRoot: true
          runAsUser: 1001
          allowPrivilegeEscalation: false
          readOnlyRootFilesystem: true
          capabilities:
            drop:
            - ALL
      imagePullSecrets:
      - name: myapp-registry-secret
      restartPolicy: Always
      terminationGracePeriodSeconds: 30
```

#### Service y Ingress

```yaml
# k8s/service.yaml
apiVersion: v1
kind: Service
metadata:
  name: myapp-api-service
  namespace: production
  labels:
    app: myapp-api
spec:
  type: ClusterIP
  ports:
  - port: 80
    targetPort: 80
    protocol: TCP
    name: http
  selector:
    app: myapp-api
  sessionAffinity: ClientIP
  sessionAffinityConfig:
    clientIP:
      timeoutSeconds: 10800

---
# k8s/ingress.yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: myapp-ingress
  namespace: production
  annotations:
    kubernetes.io/ingress.class: "nginx"
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
    nginx.ingress.kubernetes.io/force-ssl-redirect: "true"
    nginx.ingress.kubernetes.io/rate-limit: "100"
    nginx.ingress.kubernetes.io/rate-limit-window: "1m"
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
spec:
  tls:
  - hosts:
    - api.myapp.com
    secretName: myapp-tls-secret
  rules:
  - host: api.myapp.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: myapp-api-service
            port:
              number: 80
```

#### HPA y ConfigMaps

```yaml
# k8s/hpa.yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: myapp-api-hpa
  namespace: production
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: myapp-api
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
  behavior:
    scaleUp:
      stabilizationWindowSeconds: 60
      policies:
      - type: Percent
        value: 100
        periodSeconds: 15
    scaleDown:
      stabilizationWindowSeconds: 300
      policies:
      - type: Percent
        value: 10
        periodSeconds: 60

---
# k8s/configmap.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: myapp-config
  namespace: production
data:
  appsettings.json: |
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft": "Warning",
          "Microsoft.Hosting.Lifetime": "Information"
        }
      },
      "AllowedHosts": "*",
      "AllowedOrigins": [
        "https://myapp.com",
        "https://www.myapp.com"
      ],
      "RateLimiting": {
        "ProductsPolicy": {
          "PermitLimit": 1000,
          "Window": "00:01:00"
        },
        "ProductDetailPolicy": {
          "PermitLimit": 500,
          "Window": "00:01:00"
        }
      }
    }
```

### 10.5 Pipeline CI/CD Completo

#### GitHub Actions Workflow

```yaml
# .github/workflows/production-deployment.yml
name: Production Deployment Pipeline

on:
  push:
    tags:
      - 'v*'

env:
  DOTNET_VERSION: '8.0.x'
  DOCKER_REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}

jobs:
  # Build y Test
  build-and-test:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore --configuration Release
      
    - name: Run tests
      run: dotnet test --no-build --verbosity normal --configuration Release --collect:"XPlat Code Coverage"
      
    - name: Upload coverage reports
      uses: codecov/codecov-action@v3
      with:
        file: ./**/coverage.cobertura.xml
        
    - name: Run security scan
      run: dotnet list package --vulnerable
        
  # Build Docker Image
  build-docker:
    needs: build-and-test
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Set up Docker Buildx
      uses: docker/setup-buildx-action@v3
      
    - name: Log in to Container Registry
      uses: docker/login-action@v3
      with:
        registry: ${{ env.DOCKER_REGISTRY }}
        username: ${{ github.actor }}
        password: ${{ secrets.GITHUB_TOKEN }}
        
    - name: Extract metadata
      id: meta
      uses: docker/metadata-action@v5
      with:
        images: ${{ env.DOCKER_REGISTRY }}/${{ env.IMAGE_NAME }}
        tags: |
          type=semver,pattern={{version}}
          type=semver,pattern={{major}}.{{minor}}
          type=raw,value=latest
          
    - name: Build and push Docker image
      uses: docker/build-push-action@v5
      with:
        context: .
        push: true
        tags: ${{ steps.meta.outputs.tags }}
        labels: ${{ steps.meta.outputs.labels }}
        cache-from: type=gha
        cache-to: type=gha,mode=max
        
  # Deploy to Production
  deploy-production:
    needs: build-docker
    runs-on: ubuntu-latest
    environment: production
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Setup kubectl
      uses: azure/setup-kubectl@v3
      with:
        version: 'latest'
        
    - name: Configure kubectl
      run: |
        echo "${{ secrets.KUBE_CONFIG }}" | base64 -d > kubeconfig.yaml
        export KUBECONFIG=kubeconfig.yaml
        
    - name: Deploy to Kubernetes
      run: |
        kubectl apply -f k8s/namespace.yaml
        kubectl apply -f k8s/configmap.yaml
        kubectl apply -f k8s/secret.yaml
        kubectl apply -f k8s/deployment.yaml
        kubectl apply -f k8s/service.yaml
        kubectl apply -f k8s/ingress.yaml
        kubectl apply -f k8s/hpa.yaml
        
    - name: Wait for deployment
      run: |
        kubectl rollout status deployment/myapp-api -n production --timeout=300s
        
    - name: Run smoke tests
      run: |
        # Ejecuta tests básicos de funcionamiento
        echo "Running smoke tests..."
        
    - name: Notify deployment success
      run: |
        echo "Production deployment completed successfully for version ${{ github.ref_name }}"
```

---

## 💻 Ejercicios Prácticos

### Ejercicio 1: Implementar Sistema Completo

Crea un sistema de e-commerce completo con:

```csharp
// Incluye:
// - API REST completa
// - Autenticación y autorización
// - Caching y optimización
// - Logging y métricas
// - Health checks
// - Tests unitarios e integración
```

### Ejercicio 2: Deployment en Kubernetes

Implementa deployment completo con:

```yaml
# Incluye:
# - Manifests de Kubernetes
# - Configuración de servicios
# - Ingress y TLS
# - HPA y monitoreo
# - Secrets y ConfigMaps
```

---

## 🔍 Casos de Uso Reales

### 1. Sistema de Monitoreo en Producción

```csharp
public class ProductionMonitoringService
{
    public async Task MonitorSystemHealthAsync()
    {
        // Monitorea métricas en tiempo real
        // Ejecuta alertas automáticas
        // Genera reportes de performance
    }
}
```

### 2. Sistema de Backup y Recuperación

```csharp
public class BackupService
{
    public async Task CreateBackupAsync()
    {
        // Crea backups automáticos
        // Verifica integridad de datos
        // Gestiona retención de backups
    }
}
```

---

## 📊 Métricas del Proyecto

### KPIs del Sistema

1. **API Response Time**: Tiempo de respuesta promedio
2. **System Uptime**: Tiempo de actividad del sistema
3. **Error Rate**: Tasa de errores
4. **Throughput**: Capacidad de procesamiento
5. **Resource Utilization**: Utilización de recursos

---

## 🎯 Resumen del Módulo

En este módulo hemos aprendido:

✅ **Performance y Optimización**: Técnicas avanzadas de optimización
✅ **Seguridad**: Implementación de medidas de seguridad robustas
✅ **Containerización**: Docker y orquestación con Kubernetes
✅ **CI/CD**: Pipelines automatizados de deployment
✅ **Monitoreo**: Observabilidad y métricas en tiempo real
✅ **Deployment**: Estrategias avanzadas de despliegue
✅ **Proyecto Final**: Integración de todos los conceptos

---

## 🚀 Próximos Pasos

En el siguiente módulo aprenderemos sobre:
- **Plataformas Empresariales**
- Azure y AWS para .NET
- Cloud Native Development
- Serverless Architecture

---

## 🔗 Enlaces de Referencia

- [ASP.NET Core Best Practices](https://docs.microsoft.com/en-us/aspnet/core/performance/)
- [Kubernetes Production Best Practices](https://kubernetes.io/docs/concepts/configuration/overview/)
- [Microservices Architecture](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/)
- [Cloud Native Development](https://docs.microsoft.com/en-us/dotnet/architecture/cloud-native/)
