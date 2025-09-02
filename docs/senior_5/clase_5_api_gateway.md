# Clase 5: API Gateway

## Navegación
- [← Clase anterior: Microservicios](clase_4_microservicios.md)
- [← Volver al README del módulo](README.md)
- [→ Siguiente clase: Message Bus](clase_6_message_bus.md)

## Objetivos de Aprendizaje
- Comprender el patrón API Gateway
- Implementar enrutamiento y agregación de requests
- Aplicar autenticación y autorización centralizada
- Crear gateways escalables y robustos

## ¿Qué es un API Gateway?

Un **API Gateway** es un patrón arquitectónico que actúa como punto de entrada único para todas las solicitudes de los clientes. Se encarga de enrutar las solicitudes a los microservicios apropiados, agregar respuestas de múltiples servicios, y aplicar funcionalidades transversales como autenticación, logging, rate limiting y caching.

### Beneficios del API Gateway

```csharp
// 1. Punto de Entrada Único
// Los clientes solo necesitan conocer una URL
public class ApiGateway
{
    // Enruta requests a diferentes microservicios
    // /api/orders -> OrderService
    // /api/customers -> CustomerService
    // /api/products -> ProductService
}

// 2. Funcionalidades Transversales
// Autenticación, logging, rate limiting, etc.
public class AuthenticationMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Validar JWT token
        // Aplicar políticas de autorización
        // Logging de requests
    }
}

// 3. Agregación de Datos
// Combina respuestas de múltiples servicios
public class OrderDetailsAggregator
{
    public async Task<OrderDetailsDto> GetOrderDetailsAsync(Guid orderId)
    {
        // Obtener orden del OrderService
        // Obtener productos del ProductService
        // Obtener cliente del CustomerService
        // Combinar y retornar
    }
}
```

## Implementación del API Gateway

### 1. Configuración del Proxy Reverso

```csharp
// ApiGateway/Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar proxy reverso
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Configurar autenticación JWT
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
    
    options.AddPolicy("CustomerAccess", policy =>
        policy.RequireRole("Customer", "Admin"));
});

// Configurar servicios personalizados
builder.Services.AddScoped<IOrderDetailsAggregator, OrderDetailsAggregator>();
builder.Services.AddScoped<ICorrelationIdProvider, CorrelationIdProvider>();

// Configurar rate limiting
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

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Middleware personalizado
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Configurar endpoints del proxy reverso
app.MapReverseProxy();

app.Run();

// appsettings.json
{
  "ReverseProxy": {
    "Routes": {
      "order-route": {
        "ClusterId": "order-cluster",
        "Match": {
          "Path": "/api/orders/{**catch-all}"
        },
        "AuthorizationPolicy": "CustomerAccess"
      },
      "customer-route": {
        "ClusterId": "customer-cluster",
        "Match": {
          "Path": "/api/customers/{**catch-all}"
        },
        "AuthorizationPolicy": "CustomerAccess"
      },
      "product-route": {
        "ClusterId": "product-cluster",
        "Match": {
          "Path": "/api/products/{**catch-all}"
        }
      },
      "admin-route": {
        "ClusterId": "admin-cluster",
        "Match": {
          "Path": "/api/admin/{**catch-all}"
        },
        "AuthorizationPolicy": "AdminOnly"
      }
    },
    "Clusters": {
      "order-cluster": {
        "Destinations": {
          "order-destination": {
            "Address": "https://localhost:5002"
          }
        },
        "HealthCheck": {
          "Active": {
            "Enabled": true,
            "Interval": "00:00:10",
            "Timeout": "00:00:02"
          }
        }
      },
      "customer-cluster": {
        "Destinations": {
          "customer-destination": {
            "Address": "https://localhost:5003"
          }
        }
      },
      "product-cluster": {
        "Destinations": {
          "product-destination": {
            "Address": "https://localhost:5004"
          }
        }
      },
      "admin-cluster": {
        "Destinations": {
          "admin-destination": {
            "Address": "https://localhost:5005"
          }
        }
      }
    }
  },
  "Auth": {
    "Authority": "https://localhost:5001"
  }
}
```

### 2. Middleware Personalizado

```csharp
// ApiGateway/Middleware/CorrelationIdMiddleware.cs
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ICorrelationIdProvider _correlationIdProvider;
    
    public CorrelationIdMiddleware(RequestDelegate next, ICorrelationIdProvider correlationIdProvider)
    {
        _next = next;
        _correlationIdProvider = correlationIdProvider;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
            ?? _correlationIdProvider.GenerateCorrelationId();
        
        context.Request.Headers["X-Correlation-ID"] = correlationId;
        context.Response.Headers["X-Correlation-ID"] = correlationId;
        
        // Agregar correlation ID al contexto de logging
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}

// ApiGateway/Middleware/RequestLoggingMiddleware.cs
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();
        var userId = context.User?.Identity?.Name ?? "anonymous";
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;
        
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("RequestPath", requestPath))
        using (LogContext.PushProperty("RequestMethod", requestMethod))
        {
            var sw = Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation("Request started: {Method} {Path}", requestMethod, requestPath);
                
                await _next(context);
                
                sw.Stop();
                
                _logger.LogInformation("Request completed: {Method} {Path} - Status: {StatusCode} - Duration: {Duration}ms", 
                    requestMethod, requestPath, context.Response.StatusCode, sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                
                _logger.LogError(ex, "Request failed: {Method} {Path} - Duration: {Duration}ms", 
                    requestMethod, requestPath, sw.ElapsedMilliseconds);
                
                throw;
            }
        }
    }
}

// ApiGateway/Middleware/ErrorHandlingMiddleware.cs
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    
    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            
            var errorResponse = new ErrorResponse
            {
                Message = "An unexpected error occurred",
                CorrelationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault(),
                Timestamp = DateTime.UtcNow
            };
            
            var json = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(json);
        }
    }
}

public class ErrorResponse
{
    public string Message { get; set; }
    public string CorrelationId { get; set; }
    public DateTime Timestamp { get; set; }
}
```

### 3. Agregadores de Datos

```csharp
// ApiGateway/Services/IOrderDetailsAggregator.cs
public interface IOrderDetailsAggregator
{
    Task<OrderDetailsDto> GetOrderDetailsAsync(Guid orderId);
    Task<CustomerOrdersDto> GetCustomerOrdersWithDetailsAsync(Guid customerId);
    Task<OrderSummaryDto> GetOrderSummaryAsync(Guid orderId);
}

// ApiGateway/Services/OrderDetailsAggregator.cs
public class OrderDetailsAggregator : IOrderDetailsAggregator
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OrderDetailsAggregator> _logger;
    
    public OrderDetailsAggregator(IHttpClientFactory httpClientFactory, ILogger<OrderDetailsAggregator> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }
    
    public async Task<OrderDetailsDto> GetOrderDetailsAsync(Guid orderId)
    {
        try
        {
            _logger.LogInformation("Aggregating order details for order {OrderId}", orderId);
            
            var orderClient = _httpClientFactory.CreateClient("OrderService");
            var productClient = _httpClientFactory.CreateClient("ProductService");
            var customerClient = _httpClientFactory.CreateClient("CustomerService");
            
            // Ejecutar requests en paralelo
            var orderTask = orderClient.GetAsync($"/api/orders/{orderId}");
            var customerTask = Task.CompletedTask; // Se obtendrá del order
            var productTasks = new List<Task<HttpResponseMessage>>();
            
            // Obtener orden primero para conocer customer y productos
            var orderResponse = await orderTask;
            if (!orderResponse.IsSuccessStatusCode)
            {
                if (orderResponse.StatusCode == HttpStatusCode.NotFound)
                    return null;
                    
                orderResponse.EnsureSuccessStatusCode();
            }
            
            var order = await orderResponse.Content.ReadFromJsonAsync<OrderDto>();
            
            // Obtener cliente
            customerTask = customerClient.GetAsync($"/api/customers/{order.CustomerId}");
            
            // Obtener productos
            var productIds = order.Items.Select(i => i.ProductId).ToList();
            foreach (var productId in productIds)
            {
                productTasks.Add(productClient.GetAsync($"/api/products/{productId}"));
            }
            
            // Esperar todas las respuestas
            var customerResponse = await customerTask;
            var productResponses = await Task.WhenAll(productTasks);
            
            // Procesar respuestas
            CustomerDto customer = null;
            if (customerResponse.IsSuccessStatusCode)
            {
                customer = await customerResponse.Content.ReadFromJsonAsync<CustomerDto>();
            }
            
            var products = new List<ProductDto>();
            foreach (var productResponse in productResponses)
            {
                if (productResponse.IsSuccessStatusCode)
                {
                    var product = await productResponse.Content.ReadFromJsonAsync<ProductDto>();
                    products.Add(product);
                }
            }
            
            // Construir respuesta agregada
            var orderDetails = new OrderDetailsDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                CustomerName = customer?.Name ?? "Unknown",
                CustomerEmail = customer?.Email ?? "Unknown",
                Items = order.Items.Select(item =>
                {
                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    return new OrderItemDetailsDto
                    {
                        ProductId = item.ProductId,
                        ProductName = product?.Name ?? "Unknown Product",
                        ProductDescription = product?.Description ?? "",
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Total = item.Total
                    };
                }).ToList(),
                Total = order.Total,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress,
                PaymentMethod = order.PaymentMethod,
                CreatedAt = order.CreatedAt
            };
            
            _logger.LogInformation("Successfully aggregated order details for order {OrderId}", orderId);
            return orderDetails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aggregating order details for order {OrderId}", orderId);
            throw;
        }
    }
    
    public async Task<CustomerOrdersDto> GetCustomerOrdersWithDetailsAsync(Guid customerId)
    {
        try
        {
            _logger.LogInformation("Aggregating customer orders with details for customer {CustomerId}", customerId);
            
            var orderClient = _httpClientFactory.CreateClient("OrderService");
            var customerClient = _httpClientFactory.CreateClient("CustomerService");
            
            // Obtener cliente y órdenes en paralelo
            var customerTask = customerClient.GetAsync($"/api/customers/{customerId}");
            var ordersTask = orderClient.GetAsync($"/api/orders/customer/{customerId}");
            
            var customerResponse = await customerTask;
            var ordersResponse = await ordersTask;
            
            if (!customerResponse.IsSuccessStatusCode)
            {
                if (customerResponse.StatusCode == HttpStatusCode.NotFound)
                    return null;
                    
                customerResponse.EnsureSuccessStatusCode();
            }
            
            if (!ordersResponse.IsSuccessStatusCode)
            {
                ordersResponse.EnsureSuccessStatusCode();
            }
            
            var customer = await customerResponse.Content.ReadFromJsonAsync<CustomerDto>();
            var orders = await ordersResponse.Content.ReadFromJsonAsync<IEnumerable<OrderDto>>();
            
            var customerOrders = new CustomerOrdersDto
            {
                CustomerId = customer.Id,
                CustomerName = customer.Name,
                CustomerEmail = customer.Email,
                TotalOrders = customer.TotalOrders,
                TotalSpent = customer.TotalSpent,
                Orders = orders.Select(o => new OrderSummaryDto
                {
                    Id = o.Id,
                    Total = o.Total,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt,
                    ItemCount = o.Items.Count
                }).ToList()
            };
            
            _logger.LogInformation("Successfully aggregated customer orders for customer {CustomerId}", customerId);
            return customerOrders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aggregating customer orders for customer {CustomerId}", customerId);
            throw;
        }
    }
    
    public async Task<OrderSummaryDto> GetOrderSummaryAsync(Guid orderId)
    {
        try
        {
            var orderClient = _httpClientFactory.CreateClient("OrderService");
            var response = await orderClient.GetAsync($"/api/orders/{orderId}");
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;
                    
                response.EnsureSuccessStatusCode();
            }
            
            var order = await response.Content.ReadFromJsonAsync<OrderDto>();
            
            return new OrderSummaryDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                Total = order.Total,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                ItemCount = order.Items.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order summary for order {OrderId}", orderId);
            throw;
        }
    }
}
```

### 4. Controladores del Gateway

```csharp
// ApiGateway/Controllers/OrdersController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderDetailsAggregator _orderDetailsAggregator;
    private readonly ILogger<OrdersController> _logger;
    
    public OrdersController(IOrderDetailsAggregator orderDetailsAggregator, ILogger<OrdersController> logger)
    {
        _orderDetailsAggregator = orderDetailsAggregator;
        _logger = logger;
    }
    
    [HttpGet("{id}/details")]
    public async Task<ActionResult<OrderDetailsDto>> GetOrderDetails(Guid id)
    {
        try
        {
            var orderDetails = await _orderDetailsAggregator.GetOrderDetailsAsync(id);
            
            if (orderDetails == null)
                return NotFound();
                
            return Ok(orderDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order details for order {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpGet("{id}/summary")]
    public async Task<ActionResult<OrderSummaryDto>> GetOrderSummary(Guid id)
    {
        try
        {
            var orderSummary = await _orderDetailsAggregator.GetOrderSummaryAsync(id);
            
            if (orderSummary == null)
                return NotFound();
                
            return Ok(orderSummary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order summary for order {OrderId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}

// ApiGateway/Controllers/CustomersController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly IOrderDetailsAggregator _orderDetailsAggregator;
    private readonly ILogger<CustomersController> _logger;
    
    public CustomersController(IOrderDetailsAggregator orderDetailsAggregator, ILogger<CustomersController> logger)
    {
        _orderDetailsAggregator = orderDetailsAggregator;
        _logger = logger;
    }
    
    [HttpGet("{id}/orders")]
    public async Task<ActionResult<CustomerOrdersDto>> GetCustomerOrders(Guid id)
    {
        try
        {
            var customerOrders = await _orderDetailsAggregator.GetCustomerOrdersWithDetailsAsync(id);
            
            if (customerOrders == null)
                return NotFound();
                
            return Ok(customerOrders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer orders for customer {CustomerId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
```

### 5. Configuración de HTTP Clients

```csharp
// ApiGateway/Program.cs - Configuración de HTTP Clients
builder.Services.AddHttpClient("OrderService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:OrderService"]);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

builder.Services.AddHttpClient("CustomerService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:CustomerService"]);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

builder.Services.AddHttpClient("ProductService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ProductService"]);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

// Políticas de resiliencia
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => 
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}
```

### 6. Health Checks y Monitoreo

```csharp
// ApiGateway/HealthChecks/MicroserviceHealthCheck.cs
public class MicroserviceHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    
    public MicroserviceHealthCheck(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var healthChecks = new List<HealthCheckResult>();
        
        // Verificar OrderService
        var orderServiceHealthy = await CheckServiceHealthAsync("OrderService", "/health", cancellationToken);
        healthChecks.Add(orderServiceHealthy);
        
        // Verificar CustomerService
        var customerServiceHealthy = await CheckServiceHealthAsync("CustomerService", "/health", cancellationToken);
        healthChecks.Add(customerServiceHealthy);
        
        // Verificar ProductService
        var productServiceHealthy = await CheckServiceHealthAsync("ProductService", "/health", cancellationToken);
        healthChecks.Add(productServiceHealthy);
        
        var allHealthy = healthChecks.All(h => h.Status == HealthStatus.Healthy);
        
        if (allHealthy)
        {
            return HealthCheckResult.Healthy("All microservices are healthy");
        }
        
        var unhealthyServices = healthChecks.Where(h => h.Status != HealthStatus.Healthy).ToList();
        return HealthCheckResult.Unhealthy($"Some microservices are unhealthy: {string.Join(", ", unhealthyServices.Select(h => h.Description))}");
    }
    
    private async Task<HealthCheckResult> CheckServiceHealthAsync(string serviceName, string healthEndpoint, CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(serviceName);
            var response = await client.GetAsync(healthEndpoint, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy($"{serviceName} is healthy");
            }
            
            return HealthCheckResult.Unhealthy($"{serviceName} returned status {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"{serviceName} health check failed", ex);
        }
    }
}

// Program.cs - Configuración de Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<MicroserviceHealthCheck>("microservices")
    .AddCheck("self", () => HealthCheckResult.Healthy());
```

## Ejercicios Prácticos

### Ejercicio 1: Implementar Rate Limiting
Configura diferentes límites de tasa para diferentes tipos de usuarios.

### Ejercicio 2: Crear Agregadores Personalizados
Implementa agregadores para otros escenarios como dashboard de administrador.

### Ejercicio 3: Implementar Caching
Agrega caching para respuestas agregadas frecuentemente solicitadas.

## Resumen

En esta clase hemos aprendido:

1. **Patrón API Gateway**: Punto de entrada único para microservicios
2. **Proxy Reverso**: Enrutamiento automático de requests
3. **Middleware Personalizado**: Logging, correlación, manejo de errores
4. **Agregadores de Datos**: Combinación de respuestas de múltiples servicios
5. **Health Checks**: Monitoreo del estado de los microservicios

En la siguiente clase continuaremos con **Message Bus** para implementar comunicación asíncrona robusta entre servicios.

## Recursos Adicionales
- [API Gateway Pattern](https://docs.microsoft.com/en-us/azure/architecture/microservices/microservices-api-gateway)
- [YARP (Yet Another Reverse Proxy)](https://github.com/microsoft/reverse-proxy)
- [API Gateway Best Practices](https://microservices.io/patterns/apigateway.html)

