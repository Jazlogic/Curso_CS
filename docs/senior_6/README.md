# 🏆 Senior Level 6: Performance, Seguridad y Deployment

## 🧭 Navegación del Curso

- **⬅️ Anterior**: [Módulo 12: Arquitectura Limpia](../senior_5/README.md)
- **➡️ Siguiente**: [Módulo 14: Plataformas Empresariales](../senior_7/README.md)
- **📚 [Índice Completo](../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../NAVEGACION_RAPIDA.md)**

---

## 📋 Contenido del Nivel

### 🎯 Objetivos de Aprendizaje
- Optimizar el rendimiento de aplicaciones .NET
- Implementar medidas de seguridad robustas
- Dominar estrategias de deployment y CI/CD
- Implementar monitoreo y observabilidad
- Crear aplicaciones escalables y seguras para producción

### ⏱️ Tiempo Estimado
- **Teoría**: 4-5 horas
- **Ejercicios**: 6-8 horas
- **Proyecto Integrador**: 5-6 horas
- **Total**: 15-19 horas

---

## 📚 Contenido Teórico

### 1. Optimización de Performance

#### 1.1 Profiling y Análisis de Performance

```csharp
// Benchmarking con BenchmarkDotNet
[MemoryDiagnoser]
public class StringConcatenationBenchmarks
{
    private const int Iterations = 1000;
    
    [Benchmark]
    public string StringConcatenation()
    {
        string result = "";
        for (int i = 0; i < Iterations; i++)
        {
            result += i.ToString();
        }
        return result;
    }
    
    [Benchmark]
    public string StringBuilderConcatenation()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < Iterations; i++)
        {
            sb.Append(i.ToString());
        }
        return sb.ToString();
    }
    
    [Benchmark]
    public string StringInterpolation()
    {
        var result = new List<string>();
        for (int i = 0; i < Iterations; i++)
        {
            result.Add($"{i}");
        }
        return string.Join("", result);
    }
}

// Performance Counters
public class PerformanceMonitor
{
    private readonly PerformanceCounter _cpuCounter;
    private readonly PerformanceCounter _memoryCounter;
    
    public PerformanceMonitor()
    {
        _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        _memoryCounter = new PerformanceCounter("Memory", "Available MBytes");
    }
    
    public float GetCpuUsage()
    {
        return _cpuCounter.NextValue();
    }
    
    public float GetAvailableMemory()
    {
        return _memoryCounter.NextValue();
    }
    
    public void Dispose()
    {
        _cpuCounter?.Dispose();
        _memoryCounter?.Dispose();
    }
}

// Memory Profiling
public class MemoryProfiler
{
    public static void LogMemoryUsage(string operation)
    {
        var memoryInfo = GC.GetGCMemoryInfo();
        var totalMemory = GC.GetTotalMemory(false);
        
        Console.WriteLine($"[{operation}] Total Memory: {totalMemory / 1024 / 1024} MB");
        Console.WriteLine($"[{operation}] GC Memory: {memoryInfo.HeapSizeBytes / 1024 / 1024} MB");
        Console.WriteLine($"[{operation}] GC Collections: {GC.CollectionCount(0)} (Gen 0), {GC.CollectionCount(1)} (Gen 1), {GC.CollectionCount(2)} (Gen 2)");
    }
}
```

#### 1.2 Optimización de Código

```csharp
// Pooling de objetos para reducir GC pressure
public class ObjectPool<T> where T : class, new()
{
    private readonly ConcurrentQueue<T> _pool;
    private readonly int _maxSize;
    
    public ObjectPool(int maxSize = 100)
    {
        _pool = new ConcurrentQueue<T>();
        _maxSize = maxSize;
    }
    
    public T Get()
    {
        if (_pool.TryDequeue(out T item))
        {
            return item;
        }
        return new T();
    }
    
    public void Return(T item)
    {
        if (_pool.Count < _maxSize)
        {
            _pool.Enqueue(item);
        }
    }
}

// Uso del pool
public class OptimizedService
{
    private readonly ObjectPool<StringBuilder> _stringBuilderPool;
    
    public OptimizedService()
    {
        _stringBuilderPool = new ObjectPool<StringBuilder>();
    }
    
    public string ProcessData(IEnumerable<string> items)
    {
        var sb = _stringBuilderPool.Get();
        try
        {
            foreach (var item in items)
            {
                sb.AppendLine(item);
            }
            return sb.ToString();
        }
        finally
        {
            sb.Clear();
            _stringBuilderPool.Return(sb);
        }
    }
}

// Optimización de LINQ
public class LinqOptimizer
{
    // Evitar múltiples enumeraciones
    public List<string> GetFilteredAndSortedNames(IEnumerable<User> users)
    {
        // ❌ Malo: múltiples enumeraciones
        // var filtered = users.Where(u => u.IsActive);
        // var sorted = filtered.OrderBy(u => u.Name);
        // return sorted.Select(u => u.Name).ToList();
        
        // ✅ Bueno: una sola enumeración
        return users
            .Where(u => u.IsActive)
            .OrderBy(u => u.Name)
            .Select(u => u.Name)
            .ToList();
    }
    
    // Usar AsParallel para operaciones costosas
    public List<int> ProcessLargeDataSet(IEnumerable<int> data)
    {
        return data
            .AsParallel()
            .Where(x => x % 2 == 0)
            .Select(x => x * x)
            .ToList();
    }
    
    // Usar compiled queries para EF Core
    private static readonly Func<ApplicationDbContext, string, Task<List<User>>> GetUsersByNameCompiled =
        EF.CompileAsyncQuery((ApplicationDbContext context, string name) =>
            context.Users
                .Where(u => u.Name.Contains(name))
                .ToList());
    
    public async Task<List<User>> GetUsersByNameAsync(string name)
    {
        return await GetUsersByNameCompiled(_context, name);
    }
}

// Caching inteligente
public class CacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    
    public CacheService(IMemoryCache memoryCache, IDistributedCache distributedCache)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
    }
    
    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        // Intentar obtener del cache local
        if (_memoryCache.TryGetValue(key, out T cachedValue))
        {
            return cachedValue;
        }
        
        // Intentar obtener del cache distribuido
        var distributedValue = await _distributedCache.GetStringAsync(key);
        if (!string.IsNullOrEmpty(distributedValue))
        {
            var deserialized = JsonSerializer.Deserialize<T>(distributedValue);
            _memoryCache.Set(key, deserialized, expiration ?? TimeSpan.FromMinutes(5));
            return deserialized;
        }
        
        // Generar valor y cachear
        var value = await factory();
        
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(2)
        };
        
        _memoryCache.Set(key, value, options);
        
        var serialized = JsonSerializer.Serialize(value);
        await _distributedCache.SetStringAsync(key, serialized, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(10)
        });
        
        return value;
    }
}
```

#### 1.3 Async/Await y Performance

```csharp
// Optimización de operaciones asíncronas
public class AsyncOptimizer
{
    // Ejecutar operaciones en paralelo cuando sea posible
    public async Task<OrderSummary> GetOrderSummaryAsync(int orderId)
    {
        // ❌ Malo: operaciones secuenciales
        // var order = await _orderRepository.GetByIdAsync(orderId);
        // var customer = await _customerRepository.GetByIdAsync(order.CustomerId);
        // var items = await _orderItemRepository.GetByOrderIdAsync(orderId);
        
        // ✅ Bueno: operaciones en paralelo
        var orderTask = _orderRepository.GetByIdAsync(orderId);
        var customerTask = _customerRepository.GetByIdAsync(orderId);
        var itemsTask = _orderItemRepository.GetByOrderIdAsync(orderId);
        
        await Task.WhenAll(orderTask, customerTask, itemsTask);
        
        var order = await orderTask;
        var customer = await customerTask;
        var items = await itemsTask;
        
        return new OrderSummary
        {
            OrderId = order.Id,
            CustomerName = customer.Name,
            TotalItems = items.Count,
            TotalAmount = items.Sum(i => i.Total)
        };
    }
    
    // Usar ConfigureAwait(false) para evitar deadlocks
    public async Task<string> GetExternalDataAsync()
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetStringAsync("https://api.example.com/data")
            .ConfigureAwait(false);
        
        return response;
    }
    
    // Cancellation tokens para operaciones largas
    public async Task<List<User>> SearchUsersAsync(string searchTerm, CancellationToken cancellationToken)
    {
        var users = new List<User>();
        
        await foreach (var user in _userRepository.SearchAsync(searchTerm)
            .WithCancellation(cancellationToken))
        {
            users.Add(user);
            
            if (users.Count >= 1000) // Límite de seguridad
                break;
        }
        
        return users;
    }
}

// Background Services optimizados
public class OptimizedBackgroundService : BackgroundService
{
    private readonly ILogger<OptimizedBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    
    public OptimizedBackgroundService(ILogger<OptimizedBackgroundService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in background service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
    
    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        
        var users = await repository.GetUsersForProcessingAsync(100, cancellationToken);
        
        var tasks = users.Select(user => ProcessUserAsync(user, cancellationToken));
        await Task.WhenAll(tasks);
    }
    
    private async Task ProcessUserAsync(User user, CancellationToken cancellationToken)
    {
        // Procesar usuario individual
        await Task.Delay(100, cancellationToken); // Simular trabajo
    }
}
```

### 2. Seguridad

#### 2.1 Autenticación y Autorización Avanzada

```csharp
// JWT Service mejorado
public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IUserRepository _userRepository;
    
    public JwtService(IOptions<JwtSettings> jwtSettings, IUserRepository userRepository)
    {
        _jwtSettings = jwtSettings.Value;
        _userRepository = userRepository;
    }
    
    public async Task<string> GenerateTokenAsync(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("jti", Guid.NewGuid().ToString()), // JWT ID para revocación
            new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };
        
        // Agregar roles
        var roles = await _userRepository.GetUserRolesAsync(user.Id);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name));
        }
        
        // Agregar claims personalizados
        if (user.IsPremium)
        {
            claims.Add(new Claim("premium", "true"));
        }
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public ClaimsPrincipal ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
        
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RequireExpirationTime = true
        };
        
        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            return principal;
        }
        catch (SecurityTokenExpiredException)
        {
            throw new UnauthorizedAccessException("Token has expired");
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            throw new UnauthorizedAccessException("Invalid token signature");
        }
        catch (Exception ex)
        {
            throw new UnauthorizedAccessException($"Token validation failed: {ex.Message}");
        }
    }
}

// Authorization Policies
public class AuthorizationPolicies
{
    public static void ConfigurePolicies(AuthorizationOptions options)
    {
        // Política para usuarios premium
        options.AddPolicy("PremiumUsers", policy =>
            policy.RequireAssertion(context =>
                context.User.HasClaim("premium", "true")));
        
        // Política para administradores
        options.AddPolicy("AdminOnly", policy =>
            policy.RequireRole("Admin"));
        
        // Política para propietarios de recursos
        options.AddPolicy("ResourceOwner", policy =>
            policy.RequireAssertion(context =>
            {
                var resourceId = context.Resource as string;
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(resourceId) || string.IsNullOrEmpty(userId))
                    return false;
                
                // Verificar si el usuario es propietario del recurso
                return IsResourceOwner(resourceId, userId);
            }));
        
        // Política para operaciones de escritura
        options.AddPolicy("WriteAccess", policy =>
            policy.RequireAssertion(context =>
            {
                var httpContext = context.Resource as HttpContext;
                if (httpContext == null) return false;
                
                var method = httpContext.Request.Method;
                return method == "POST" || method == "PUT" || method == "DELETE";
            }));
    }
    
    private static bool IsResourceOwner(string resourceId, string userId)
    {
        // Implementar lógica de verificación
        return true; // Simplificado para el ejemplo
    }
}

// Custom Authorization Handler
public class ResourceOwnerAuthorizationHandler : AuthorizationHandler<ResourceOwnerRequirement, string>
{
    private readonly IUserRepository _userRepository;
    
    public ResourceOwnerAuthorizationHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourceOwnerRequirement requirement,
        string resourceId)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            return;
        }
        
        var isOwner = await _userRepository.IsResourceOwnerAsync(resourceId, userId);
        
        if (isOwner)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}

public class ResourceOwnerRequirement : IAuthorizationRequirement { }
```

#### 2.2 Protección contra Ataques Comunes

```csharp
// Rate Limiting
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    
    public RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientId(context);
        var endpoint = context.Request.Path;
        
        var key = $"rate_limit:{clientId}:{endpoint}";
        var requestCount = await _cache.GetOrCreateAsync(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return Task.FromResult(0);
        });
        
        if (requestCount >= 100) // 100 requests por minuto
        {
            _logger.LogWarning($"Rate limit exceeded for client {clientId} on endpoint {endpoint}");
            context.Response.StatusCode = 429; // Too Many Requests
            await context.Response.WriteAsync("Rate limit exceeded");
            return;
        }
        
        _cache.Set(key, requestCount + 1, TimeSpan.FromMinutes(1));
        await _next(context);
    }
    
    private string GetClientId(HttpContext context)
    {
        // Usar IP del cliente o token de usuario
        var user = context.User.Identity;
        if (user?.IsAuthenticated == true)
        {
            return user.Name;
        }
        
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

// Input Validation y Sanitization
public class InputValidationService
{
    public ValidationResult ValidateUserInput(CreateUserRequest request)
    {
        var result = new ValidationResult();
        
        // Validar email
        if (!IsValidEmail(request.Email))
        {
            result.AddError("Email", "Invalid email format");
        }
        
        // Validar contraseña
        if (!IsStrongPassword(request.Password))
        {
            result.AddError("Password", "Password must be at least 8 characters with uppercase, lowercase, number and special character");
        }
        
        // Sanitizar nombre
        request.Name = SanitizeString(request.Name);
        
        // Validar longitud
        if (request.Name.Length < 2 || request.Name.Length > 50)
        {
            result.AddError("Name", "Name must be between 2 and 50 characters");
        }
        
        return result;
    }
    
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    
    private bool IsStrongPassword(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8)
            return false;
        
        var hasUpper = password.Any(char.IsUpper);
        var hasLower = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));
        
        return hasUpper && hasLower && hasDigit && hasSpecial;
    }
    
    private string SanitizeString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        
        // Remover caracteres peligrosos
        var dangerousChars = new[] { '<', '>', '"', '\'', '&' };
        var sanitized = input;
        
        foreach (var c in dangerousChars)
        {
            sanitized = sanitized.Replace(c.ToString(), "");
        }
        
        return sanitized.Trim();
    }
}

// CSRF Protection
public class CsrfProtectionMiddleware
{
    private readonly RequestDelegate _next;
    
    public CsrfProtectionMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        if (IsModifyingRequest(context.Request))
        {
            var token = context.Request.Headers["X-CSRF-Token"].FirstOrDefault();
            var sessionToken = context.Session.GetString("CSRFToken");
            
            if (string.IsNullOrEmpty(token) || token != sessionToken)
            {
                context.Response.StatusCode = 403; // Forbidden
                await context.Response.WriteAsync("CSRF token validation failed");
                return;
            }
        }
        
        // Generar nuevo token para la sesión
        if (context.Session.GetString("CSRFToken") == null)
        {
            var newToken = GenerateCsrfToken();
            context.Session.SetString("CSRFToken", newToken);
        }
        
        await _next(context);
    }
    
    private bool IsModifyingRequest(HttpRequest request)
    {
        return request.Method == "POST" || request.Method == "PUT" || 
               request.Method == "DELETE" || request.Method == "PATCH";
    }
    
    private string GenerateCsrfToken()
    {
        var bytes = new byte[32];
        using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes);
    }
}
```

### 3. Deployment y CI/CD

#### 3.1 Docker y Containerización

```dockerfile
# Dockerfile para aplicación .NET
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MyApp.csproj", "MyApp/"]
RUN dotnet restore "MyApp/MyApp.csproj"
COPY . .
WORKDIR "/src/MyApp"
RUN dotnet build "MyApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

```yaml
# docker-compose.yml
version: '3.8'

services:
  app:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=db;Database=MyAppDb;User=sa;Password=YourStrong@Passw0rd
    depends_on:
      - db
      - redis
    networks:
      - app-network

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    networks:
      - app-network

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    networks:
      - app-network

  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf
      - ./ssl:/etc/nginx/ssl
    depends_on:
      - app
    networks:
      - app-network

volumes:
  sqlserver_data:
  redis_data:

networks:
  app-network:
    driver: bridge
```

#### 3.2 GitHub Actions CI/CD

```yaml
# .github/workflows/ci-cd.yml
name: CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: 8.0.x
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Test
      run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
    
    - name: Upload coverage reports
      uses: codecov/codecov-action@v3
      with:
        file: ./**/coverage.cobertura.xml

  security-scan:
    runs-on: ubuntu-latest
    needs: test
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Run security scan
      uses: snyk/actions/dotnet@master
      env:
        SNYK_TOKEN: ${{ secrets.SNYK_TOKEN }}
      with:
        args: --severity-threshold=high

  build-image:
    runs-on: ubuntu-latest
    needs: [test, security-scan]
    if: github.ref == 'refs/heads/main'
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Set up Docker Buildx
      uses: docker/setup-buildx-action@v3
    
    - name: Login to Docker Hub
      uses: docker/login-action@v3
      with:
        username: ${{ secrets.DOCKER_USERNAME }}
        password: ${{ secrets.DOCKER_PASSWORD }}
    
    - name: Build and push Docker image
      uses: docker/build-push-action@v5
      with:
        context: .
        push: true
        tags: |
          myapp:latest
          myapp:${{ github.sha }}
        cache-from: type=gha
        cache-to: type=gha,mode=max

  deploy:
    runs-on: ubuntu-latest
    needs: build-image
    if: github.ref == 'refs/heads/main'
    
    steps:
    - name: Deploy to production
      run: |
        echo "Deploying to production..."
        # Aquí irían los comandos de deployment
        # Por ejemplo, kubectl apply, terraform apply, etc.
```

#### 3.3 Kubernetes Deployment

```yaml
# k8s/deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: myapp
  labels:
    app: myapp
spec:
  replicas: 3
  selector:
    matchLabels:
      app: myapp
  template:
    metadata:
      labels:
        app: myapp
    spec:
      containers:
      - name: myapp
        image: myapp:latest
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: db-secret
              key: connection-string
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
          allowPrivilegeEscalation: false
          runAsNonRoot: true
          runAsUser: 1000
          capabilities:
            drop:
            - ALL

---
apiVersion: v1
kind: Service
metadata:
  name: myapp-service
spec:
  selector:
    app: myapp
  ports:
  - protocol: TCP
    port: 80
    targetPort: 80
  type: ClusterIP

---
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: myapp-ingress
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
spec:
  tls:
  - hosts:
    - myapp.example.com
    secretName: myapp-tls
  rules:
  - host: myapp.example.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: myapp-service
            port:
              number: 80
```

### 4. Monitoreo y Observabilidad

#### 4.1 Health Checks

```csharp
// Health Checks personalizados
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _context;
    
    public DatabaseHealthCheck(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Database.CanConnectAsync(cancellationToken);
            return HealthCheckResult.Healthy("Database is accessible");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database is not accessible", ex);
        }
    }
}

public class ExternalApiHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalApiHealthCheck> _logger;
    
    public ExternalApiHealthCheck(HttpClient httpClient, ILogger<ExternalApiHealthCheck> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("https://api.external.com/health", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("External API is accessible");
            }
            
            return HealthCheckResult.Degraded($"External API returned status code: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "External API health check failed");
            return HealthCheckResult.Unhealthy("External API is not accessible", ex);
        }
    }
}

// Configuración de Health Checks
public static class HealthCheckExtensions
{
    public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "database" })
            .AddCheck<ExternalApiHealthCheck>("external-api", tags: new[] { "external" })
            .AddCheck("memory", () =>
            {
                var memoryInfo = GC.GetGCMemoryInfo();
                var memoryThreshold = 1024 * 1024 * 1024; // 1GB
                
                if (memoryInfo.HeapSizeBytes > memoryThreshold)
                {
                    return HealthCheckResult.Degraded("Memory usage is high");
                }
                
                return HealthCheckResult.Healthy("Memory usage is normal");
            }, tags: new[] { "system" });
        
        return services;
    }
}
```

#### 4.2 Logging y Tracing

```csharp
// Structured Logging
public class LoggingService
{
    private readonly ILogger<LoggingService> _logger;
    
    public LoggingService(ILogger<LoggingService> logger)
    {
        _logger = logger;
    }
    
    public void LogUserAction(string userId, string action, object details)
    {
        _logger.LogInformation("User {UserId} performed {Action} with details {@Details}", 
            userId, action, details);
    }
    
    public void LogPerformance(string operation, long elapsedMs, object context)
    {
        _logger.LogInformation("Operation {Operation} completed in {ElapsedMs}ms with context {@Context}", 
            operation, elapsedMs, context);
    }
    
    public void LogSecurityEvent(string eventType, string userId, string ipAddress, object details)
    {
        _logger.LogWarning("Security event {EventType} for user {UserId} from IP {IpAddress}: {@Details}", 
            eventType, userId, ipAddress, details);
    }
}

// Distributed Tracing
public class TracingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TracingMiddleware> _logger;
    
    public TracingMiddleware(RequestDelegate next, ILogger<TracingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
        
        using var activity = ActivitySource.StartActivity("HTTP Request");
        activity?.SetTag("http.url", context.Request.Path);
        activity?.SetTag("http.method", context.Request.Method);
        activity?.SetTag("correlation.id", correlationId);
        
        context.Response.Headers["X-Correlation-ID"] = correlationId;
        context.Response.Headers["X-Trace-ID"] = traceId;
        
        var sw = Stopwatch.StartNew();
        
        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            activity?.SetTag("http.duration_ms", sw.ElapsedMilliseconds);
            activity?.SetTag("http.status_code", context.Response.StatusCode);
            
            _logger.LogInformation("Request {Method} {Path} completed in {ElapsedMs}ms with status {StatusCode}", 
                context.Request.Method, context.Request.Path, sw.ElapsedMilliseconds, context.Response.StatusCode);
        }
    }
}

// Metrics con Prometheus
public class MetricsService
{
    private readonly Counter _requestCounter;
    private readonly Histogram _requestDuration;
    private readonly Gauge _activeConnections;
    
    public MetricsService()
    {
        _requestCounter = Metrics.CreateCounter("http_requests_total", "Total HTTP requests", 
            new CounterConfiguration
            {
                LabelNames = new[] { "method", "endpoint", "status_code" }
            });
        
        _requestDuration = Metrics.CreateHistogram("http_request_duration_seconds", "HTTP request duration", 
            new HistogramConfiguration
            {
                LabelNames = new[] { "method", "endpoint" }
            });
        
        _activeConnections = Metrics.CreateGauge("http_active_connections", "Active HTTP connections");
    }
    
    public void RecordRequest(string method, string endpoint, int statusCode, double durationSeconds)
    {
        _requestCounter.WithLabels(method, endpoint, statusCode.ToString()).Inc();
        _requestDuration.WithLabels(method, endpoint).Observe(durationSeconds);
    }
    
    public void SetActiveConnections(int count)
    {
        _activeConnections.Set(count);
    }
}
```

---

## 🎯 Ejercicios Prácticos

### Ejercicio 1: Performance Profiling
Implementa benchmarking para comparar diferentes algoritmos de ordenamiento.

### Ejercicio 2: Memory Optimization
Crea un pool de objetos para reducir la presión del garbage collector.

### Ejercicio 3: Security Implementation
Implementa autenticación JWT con refresh tokens y rate limiting.

### Ejercicio 4: Docker Containerization
Containeriza una aplicación .NET con múltiples servicios.

### Ejercicio 5: CI/CD Pipeline
Crea un pipeline completo con GitHub Actions.

### Ejercicio 6: Kubernetes Deployment
Despliega una aplicación en Kubernetes con health checks.

### Ejercicio 7: Monitoring Setup
Implementa health checks, logging estructurado y métricas.

### Ejercicio 8: Security Hardening
Implementa protección contra ataques comunes (CSRF, XSS, SQL Injection).

### Ejercicio 9: Performance Testing
Crea pruebas de carga y stress para una API.

### Ejercicio 10: Production Deployment
Configura un deployment completo para producción con SSL y CDN.

---

## 🚀 Proyecto Integrador: Aplicación Web de Alto Rendimiento

### Descripción
Crea una aplicación web completa optimizada para producción con todas las mejores prácticas.

### Requisitos
- Optimización de performance y memoria
- Implementación de seguridad robusta
- Containerización con Docker
- Pipeline CI/CD completo
- Deployment en Kubernetes
- Monitoreo y observabilidad
- Health checks y métricas
- Logging estructurado
- Rate limiting y protección CSRF

### Estructura Sugerida
```
HighPerformanceApp/
├── src/
│   ├── HighPerformanceApp.API/
│   ├── HighPerformanceApp.Core/
│   └── HighPerformanceApp.Infrastructure/
├── tests/
├── docker/
├── k8s/
├── .github/
└── docs/
```

---

## 📝 Autoevaluación

### Preguntas Teóricas
1. ¿Qué herramientas usarías para profiling de performance en .NET?
2. ¿Cómo optimizas el uso de memoria en aplicaciones .NET?
3. ¿Qué medidas de seguridad implementarías para una API pública?
4. ¿Cuáles son las ventajas de containerización con Docker?
5. ¿Cómo implementarías monitoreo y observabilidad en producción?

### Preguntas Prácticas
1. Optimiza una consulta LINQ compleja para mejor rendimiento
2. Implementa rate limiting y protección CSRF
3. Crea un Dockerfile optimizado para una aplicación .NET
4. Configura health checks y métricas para una API

---

## 🔗 Enlaces de Referencia

- [.NET Performance](https://docs.microsoft.com/en-us/dotnet/fundamentals/performance/)
- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [Docker for .NET](https://docs.microsoft.com/en-us/dotnet/core/docker/)
- [Kubernetes for .NET](https://docs.microsoft.com/en-us/dotnet/architecture/cloud-native/)

---

## 🎉 ¡Felicidades! Has Completado el Curso Completo

**Progreso**: 12 de 12 niveles completados ✅

**Nivel Anterior**: [Senior Level 5: Arquitectura Limpia](../senior_5/README.md)

---

## 🏆 ¡Eres un Desarrollador Senior Backend en C#!

Has completado exitosamente todos los niveles del curso. Ahora puedes:

### 🚀 Habilidades Adquiridas
- **Fundamentos Sólidos**: Variables, tipos de datos, estructuras de control, funciones
- **OOP Avanzado**: Herencia, polimorfismo, interfaces, abstract classes
- **Patrones y Principios**: SOLID, patrones de diseño, Clean Architecture
- **Testing Profesional**: TDD, testing unitario, de integración y de comportamiento
- **APIs y Microservicios**: REST APIs, Entity Framework, arquitectura de microservicios
- **Performance y Seguridad**: Optimización, seguridad, deployment y monitoreo

### 💼 Oportunidades de Carrera
- **Senior Backend Developer**
- **Software Architect**
- **Team Lead**
- **Technical Consultant**
- **DevOps Engineer**

### 🔄 Próximos Pasos Recomendados
1. **Practica**: Implementa proyectos reales usando lo aprendido
2. **Contribuye**: Participa en proyectos open source
3. **Mantente Actualizado**: Sigue las últimas tendencias en .NET
4. **Mentoría**: Ayuda a otros desarrolladores a crecer
5. **Certificaciones**: Considera certificaciones de Microsoft

### 🎯 Proyectos para Consolidar Conocimientos
- Sistema de e-commerce completo
- API de gestión empresarial
- Plataforma de microservicios
- Aplicación con Clean Architecture
- Sistema con testing completo

¡Has alcanzado un nivel excepcional en desarrollo backend con C#! 🎊
