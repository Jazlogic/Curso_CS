# Clase 7: Testing de Microservicios

## Navegación
- [← Clase 6: Distributed Tracing](clase_6_distributed_tracing.md)
- [Clase 8: Despliegue y Orquestación →](clase_8_despliegue_orquestacion.md)
- [← Volver al README del módulo](README.md)
- [← Volver al módulo anterior (senior_3)](../senior_3/README.md)
- [→ Ir al siguiente módulo (senior_5)](../senior_5/README.md)

## Objetivos de Aprendizaje
- Comprender las estrategias de testing para microservicios
- Implementar testing de integración con TestContainers
- Configurar testing de contrato con Pact
- Implementar testing de performance y carga
- Crear testing de resiliencia y circuit breakers

## Contenido Teórico

### 1. Estrategias de Testing para Microservicios

Las estrategias de testing en microservicios deben considerar la arquitectura distribuida:

```csharp
// Estrategias de testing para microservicios
public class MicroserviceTestingStrategies
{
    // 1. Testing Unitario - Prueba de componentes individuales
    public void UnitTesting()
    {
        // Prueba de lógica de negocio aislada
        // Mock de dependencias externas
        // Testing de edge cases y validaciones
    }
    
    // 2. Testing de Integración - Prueba de comunicación entre componentes
    public void IntegrationTesting()
    {
        // Prueba de APIs y contratos
        // Testing de base de datos
        // Verificación de mensajes y eventos
    }
    
    // 3. Testing de Contrato - Prueba de compatibilidad entre servicios
    public void ContractTesting()
    {
        // Verificación de APIs públicas
        // Testing de esquemas de mensajes
        // Validación de versiones de API
    }
    
    // 4. Testing de Performance - Prueba de escalabilidad y rendimiento
    public void PerformanceTesting()
    {
        // Testing de carga y estrés
        // Medición de latencia y throughput
        // Análisis de cuellos de botella
    }
}
```

### 2. Testing de Integración con TestContainers

Implementación de testing de integración usando contenedores:

```csharp
// Testing de integración con base de datos
public class UserServiceIntegrationTests : IAsyncDisposable
{
    private readonly TestcontainersContainer _postgresContainer;
    private readonly TestcontainersContainer _redisContainer;
    private readonly IServiceProvider _serviceProvider;
    private readonly UserService _userService;

    public UserServiceIntegrationTests()
    {
        // Configurar PostgreSQL container
        _postgresContainer = new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(new PostgreSqlTestcontainerConfiguration
            {
                Database = "testdb",
                Username = "testuser",
                Password = "testpass"
            })
            .Build();

        // Configurar Redis container
        _redisContainer = new TestcontainersBuilder<RedisTestcontainer>()
            .WithDatabase("0")
            .Build();

        // Configurar servicios
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
        _userService = _serviceProvider.GetRequiredService<UserService>();
    }

    [Fact]
    public async Task CreateUser_WithValidData_ShouldSucceed()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Email = "test@example.com",
            Name = "Test User",
            Role = "User"
        };

        // Act
        var result = await _userService.CreateUserAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Email, result.Email);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Role, result.Role);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task CreateUser_WithDuplicateEmail_ShouldFail()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Email = "duplicate@example.com",
            Name = "Test User",
            Role = "User"
        };

        // Crear primer usuario
        await _userService.CreateUserAsync(request);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _userService.CreateUserAsync(request));
        
        Assert.Contains("Email already exists", exception.Message);
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Configurar base de datos
        var connectionString = _postgresContainer.ConnectionString;
        services.AddDbContext<UserDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Configurar Redis
        var redisConnection = _redisContainer.ConnectionString;
        services.AddStackExchangeRedisCache(options =>
            options.Configuration = redisConnection);

        // Configurar servicios
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEmailService, MockEmailService>();
        services.AddScoped<UserService>();
    }

    public async ValueTask DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();
        
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}

// Testing de integración con APIs externas
public class ExternalApiIntegrationTests : IAsyncDisposable
{
    private readonly TestcontainersContainer _wiremockContainer;
    private readonly HttpClient _httpClient;
    private readonly ExternalServiceClient _externalServiceClient;

    public ExternalApiIntegrationTests()
    {
        // Configurar WireMock para simular APIs externas
        _wiremockContainer = new TestcontainersBuilder<WireMockContainer>()
            .WithImage("wiremock/wiremock:2.35.0")
            .WithExposedPort(8080)
            .WithCommand("--port", "8080")
            .Build();

        _httpClient = new HttpClient();
        _externalServiceClient = new ExternalServiceClient(_httpClient);
    }

    [Fact]
    public async Task GetUserData_FromExternalApi_ShouldReturnData()
    {
        // Arrange
        var userId = "123";
        var expectedData = new { id = userId, name = "John Doe" };

        // Configurar stub en WireMock
        await ConfigureWireMockStub(userId, expectedData);

        // Act
        var result = await _externalServiceClient.GetUserDataAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("John Doe", result.Name);
    }

    private async Task ConfigureWireMockStub(string userId, object responseData)
    {
        var stub = new
        {
            request = new
            {
                method = "GET",
                url = $"/api/users/{userId}"
            },
            response = new
            {
                status = 200,
                jsonBody = responseData
            }
        };

        var json = JsonSerializer.Serialize(stub);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(
            $"http://localhost:{_wiremockContainer.GetMappedPublicPort(8080)}/__admin/mappings/new",
            content);
        
        response.EnsureSuccessStatusCode();
    }

    public async ValueTask DisposeAsync()
    {
        await _wiremockContainer.DisposeAsync();
        _httpClient.Dispose();
    }
}
```

### 3. Testing de Contrato con Pact

Implementación de testing de contrato para verificar compatibilidad:

```csharp
// Testing de contrato con Pact
public class UserApiContractTests : IAsyncDisposable
{
    private readonly IPactBuilderV3 _pactBuilder;
    private readonly string _pactDir;

    public UserApiContractTests()
    {
        _pactDir = Path.Join("..", "..", "..", "..", "pacts");
        _pactBuilder = Pact.V3("UserService", "UserApi", new PactConfig
        {
            PactDir = _pactDir,
            DefaultJsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }
        });
    }

    [Fact]
    public async Task CreateUser_ShouldMatchContract()
    {
        // Arrange
        var expectedRequest = new CreateUserRequest
        {
            Email = "test@example.com",
            Name = "Test User",
            Role = "User"
        };

        var expectedResponse = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        // Configurar expectativa del contrato
        _pactBuilder
            .UponReceiving("A request to create a user")
            .Given("A valid user request")
            .WithRequest(HttpMethod.Post, "/api/users")
            .WithJsonBody(expectedRequest)
            .WillRespond()
            .WithStatus(201)
            .WithJsonBody(expectedResponse);

        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Act
            var client = CreateHttpClient(ctx.MockServerUri);
            var response = await client.PostAsJsonAsync("/api/users", expectedRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            
            var result = await response.Content.ReadFromJsonAsync<User>();
            Assert.NotNull(result);
            Assert.Equal(expectedRequest.Email, result.Email);
        });
    }

    [Fact]
    public async Task GetUser_ShouldMatchContract()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedUser = new User
        {
            Id = userId,
            Email = "test@example.com",
            Name = "Test User",
            Role = "User"
        };

        // Configurar expectativa del contrato
        _pactBuilder
            .UponReceiving("A request to get a user")
            .Given($"A user with id {userId}")
            .WithRequest(HttpMethod.Get, $"/api/users/{userId}")
            .WillRespond()
            .WithStatus(200)
            .WithJsonBody(expectedUser);

        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Act
            var client = CreateHttpClient(ctx.MockServerUri);
            var response = await client.GetAsync($"/api/users/{userId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var result = await response.Content.ReadFromJsonAsync<User>();
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
        });
    }

    private HttpClient CreateHttpClient(Uri baseUri)
    {
        return new HttpClient
        {
            BaseAddress = baseUri
        };
    }

    public async ValueTask DisposeAsync()
    {
        await _pactBuilder.AsyncDispose();
    }
}
```

### 4. Testing de Performance y Carga

Implementación de testing de performance:

```csharp
// Testing de performance con NBomber
public class UserServicePerformanceTests
{
    [Fact]
    public void CreateUser_ShouldHandleConcurrentRequests()
    {
        var scenario = Scenario.Create("create_user_concurrent", async context =>
        {
            var request = new CreateUserRequest
            {
                Email = $"user_{context.Random.Next(1000)}@example.com",
                Name = $"User {context.Random.Next(1000)}",
                Role = "User"
            };

            try
            {
                using var client = new HttpClient();
                var response = await client.PostAsJsonAsync(
                    "http://localhost:5000/api/users", request);
                
                if (response.IsSuccessStatusCode)
                {
                    context.MarkAsOk();
                }
                else
                {
                    context.MarkAsFail($"HTTP {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                context.MarkAsFail(ex.Message);
            }
        })
        .WithLoadSimulations(
            Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(1))
        );

        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
    }

    [Fact]
    public void GetUser_ShouldHandleHighLoad()
    {
        var scenario = Scenario.Create("get_user_high_load", async context =>
        {
            var userId = context.Random.Next(1, 1000);

            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync(
                    $"http://localhost:5000/api/users/{userId}");
                
                if (response.IsSuccessStatusCode)
                {
                    context.MarkAsOk();
                }
                else
                {
                    context.MarkAsFail($"HTTP {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                context.MarkAsFail(ex.Message);
            }
        })
        .WithLoadSimulations(
            Simulation.Stress(rate: 200, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(2))
        );

        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
    }
}

// Testing de carga con Artillery
public class LoadTestingConfiguration
{
    public static void GenerateArtilleryConfig()
    {
        var config = new
        {
            config = new
            {
                target = "http://localhost:5000",
                phases = new[]
                {
                    new { duration = 60, arrivalRate = 10 },
                    new { duration = 120, arrivalRate = 50 },
                    new { duration = 60, arrivalRate = 100 }
                }
            },
            scenarios = new[]
            {
                new
                {
                    name = "User API Load Test",
                    weight = 1,
                    requests = new[]
                    {
                        new { method = "GET", url = "/api/users" },
                        new { method = "POST", url = "/api/users", json = new { email = "test@example.com", name = "Test User" } }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("artillery-config.json", json);
    }
}
```

### 5. Testing de Resiliencia

Implementación de testing para circuit breakers y patrones de resiliencia:

```csharp
// Testing de circuit breaker
public class CircuitBreakerTests
{
    [Fact]
    public async Task CircuitBreaker_ShouldOpenAfterThreshold()
    {
        // Arrange
        var circuitBreaker = new CircuitBreaker(
            failureThreshold: 3,
            resetTimeout: TimeSpan.FromSeconds(5));

        var failingService = new Mock<IFailingService>();
        failingService.Setup(x => x.CallAsync())
            .ThrowsAsync(new Exception("Service unavailable"));

        // Act & Assert
        // Primeras 3 llamadas fallan
        for (int i = 0; i < 3; i++)
        {
            await Assert.ThrowsAsync<Exception>(
                () => circuitBreaker.ExecuteAsync(() => failingService.Object.CallAsync()));
        }

        // Circuit breaker se abre
        Assert.Equal(CircuitState.Open, circuitBreaker.State);

        // Las siguientes llamadas fallan inmediatamente
        var stopwatch = Stopwatch.StartNew();
        await Assert.ThrowsAsync<CircuitBreakerOpenException>(
            () => circuitBreaker.ExecuteAsync(() => failingService.Object.CallAsync()));
        stopwatch.Stop();

        // Debe fallar rápidamente (sin esperar timeout)
        Assert.True(stopwatch.ElapsedMilliseconds < 100);
    }

    [Fact]
    public async Task CircuitBreaker_ShouldCloseAfterResetTimeout()
    {
        // Arrange
        var circuitBreaker = new CircuitBreaker(
            failureThreshold: 2,
            resetTimeout: TimeSpan.FromMilliseconds(100));

        var failingService = new Mock<IFailingService>();
        failingService.Setup(x => x.CallAsync())
            .ThrowsAsync(new Exception("Service unavailable"));

        // Act
        // Abrir circuit breaker
        for (int i = 0; i < 2; i++)
        {
            await Assert.ThrowsAsync<Exception>(
                () => circuitBreaker.ExecuteAsync(() => failingService.Object.CallAsync()));
        }

        Assert.Equal(CircuitState.Open, circuitBreaker.State);

        // Esperar reset timeout
        await Task.Delay(150);

        // Cambiar comportamiento del servicio
        failingService.Setup(x => x.CallAsync())
            .ReturnsAsync("Success");

        // Circuit breaker debe estar en estado half-open
        Assert.Equal(CircuitState.HalfOpen, circuitBreaker.State);

        // Llamada exitosa debe cerrar el circuit breaker
        var result = await circuitBreaker.ExecuteAsync(() => failingService.Object.CallAsync());
        
        Assert.Equal("Success", result);
        Assert.Equal(CircuitState.Closed, circuitBreaker.State);
    }
}

// Testing de retry policies
public class RetryPolicyTests
{
    [Fact]
    public async Task RetryPolicy_ShouldRetryOnFailure()
    {
        // Arrange
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        var callCount = 0;
        var service = new Mock<IFailingService>();
        service.Setup(x => x.CallAsync())
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount < 3)
                {
                    throw new Exception("Temporary failure");
                }
                return "Success";
            });

        // Act
        var result = await retryPolicy.ExecuteAsync(() => service.Object.CallAsync());

        // Assert
        Assert.Equal("Success", result);
        Assert.Equal(3, callCount);
    }

    [Fact]
    public async Task RetryPolicy_ShouldFailAfterMaxRetries()
    {
        // Arrange
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(2, retryAttempt => 
                TimeSpan.FromMilliseconds(10));

        var service = new Mock<IFailingService>();
        service.Setup(x => x.CallAsync())
            .ThrowsAsync(new Exception("Persistent failure"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(
            () => retryPolicy.ExecuteAsync(() => service.Object.CallAsync()));
    }
}
```

## Ejercicios Prácticos

### Ejercicio 1: Testing de Integración
Implementa tests de integración para un microservicio de usuarios usando TestContainers.

### Ejercicio 2: Testing de Contrato
Crea tests de contrato con Pact para verificar la compatibilidad de APIs entre servicios.

### Ejercicio 3: Testing de Performance
Implementa tests de carga y performance para identificar cuellos de botella en microservicios.

## Proyecto Integrador
Implementa un sistema completo de testing para un microservicio que incluya:
- Tests unitarios con mocks
- Tests de integración con contenedores
- Tests de contrato con Pact
- Tests de performance con NBomber
- Tests de resiliencia para circuit breakers

## Recursos Adicionales
- [TestContainers .NET](https://testcontainers.com/dotnet/)
- [Pact .NET](https://docs.pact.io/implementation_guides/dotnet/)
- [NBomber](https://nbomber.com/)
- [Artillery](https://www.artillery.io/)
- [Polly](https://github.com/App-vNext/Polly)
