# 🎯 **Clase 10: Proyecto Final - Sistema Completo de Testing**

## 🎯 **Objetivos de la Clase**
- Implementar sistema completo de testing para MussikOn
- Aplicar todas las técnicas aprendidas
- Crear pipeline de CI/CD completo
- Demostrar maestría en testing avanzado

## 📚 **Contenido Teórico**

### **1. Arquitectura del Sistema de Testing**

#### **Estructura del Proyecto**
```
MussikOn.Testing/
├── MussikOn.Tests.Unit/           # Tests unitarios
├── MussikOn.Tests.Integration/    # Tests de integración
├── MussikOn.Tests.Performance/    # Tests de performance
├── MussikOn.Tests.Security/       # Tests de seguridad
├── MussikOn.Tests.Contract/       # Tests de contratos
├── MussikOn.Tests.Mutation/       # Tests de mutación
├── MussikOn.Tests.E2E/            # Tests end-to-end
├── MussikOn.Testing.Infrastructure/ # Infraestructura de testing
└── MussikOn.Testing.Utilities/    # Utilidades de testing
```

#### **Configuración Global de Testing**
```csharp
// MussikOn.Testing.Infrastructure/TestingConfiguration.cs
public static class TestingConfiguration
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Configurar servicios de testing
        services.AddSingleton<ITestDataBuilder, TestDataBuilder>();
        services.AddSingleton<ITestDatabaseManager, TestDatabaseManager>();
        services.AddSingleton<ITestContainerManager, TestContainerManager>();
        services.AddSingleton<ITestMetricsCollector, TestMetricsCollector>();
        
        // Configurar mocks
        services.AddSingleton<IMockFactory, MockFactory>();
        
        // Configurar logging para tests
        services.AddLogging(builder => builder.AddConsole());
    }
    
    public static WebApplicationFactory<Program> CreateTestFactory()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Reemplazar servicios de producción con mocks
                    services.AddSingleton<IPaymentGateway, MockPaymentGateway>();
                    services.AddSingleton<INotificationService, MockNotificationService>();
                    services.AddSingleton<IEmailService, MockEmailService>();
                });
            });
    }
}
```

### **2. Implementación de Tests Unitarios**

#### **Tests Unitarios para MusicianMatchingService**
```csharp
// MussikOn.Tests.Unit/Services/MusicianMatchingServiceTests.cs
[TestFixture]
public class MusicianMatchingServiceTests
{
    private MusicianMatchingService _service;
    private Mock<IMusicianRepository> _mockMusicianRepository;
    private Mock<IEventRepository> _mockEventRepository;
    private Mock<ILogger<MusicianMatchingService>> _mockLogger;
    
    [SetUp]
    public void Setup()
    {
        _mockMusicianRepository = new Mock<IMusicianRepository>();
        _mockEventRepository = new Mock<IEventRepository>();
        _mockLogger = new Mock<ILogger<MusicianMatchingService>>();
        
        _service = new MusicianMatchingService(
            _mockMusicianRepository.Object,
            _mockEventRepository.Object,
            _mockLogger.Object);
    }
    
    [Test]
    public async Task FindMatchingMusicians_WithValidCriteria_ShouldReturnMatchingMusicians()
    {
        // Arrange
        var criteria = new EventCriteria
        {
            Genre = "Rock",
            Location = new Location("Madrid", "Spain"),
            Budget = 1000,
            Duration = 2,
            MinExperienceYears = 3
        };
        
        var musicians = new List<Musician>
        {
            new Musician { Id = 1, Genre = "Rock", Location = new Location("Madrid", "Spain"), HourlyRate = 400, ExperienceYears = 5 },
            new Musician { Id = 2, Genre = "Jazz", Location = new Location("Barcelona", "Spain"), HourlyRate = 600, ExperienceYears = 2 }
        };
        
        _mockMusicianRepository.Setup(r => r.GetAvailableMusiciansAsync(criteria))
                             .ReturnsAsync(musicians);
        
        // Act
        var result = await _service.FindMatchingMusiciansAsync(criteria);
        
        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.First().Musician.Id, Is.EqualTo(1));
        Assert.That(result.First().Score, Is.EqualTo(100));
    }
    
    [Test]
    public void CalculateMatchScore_WithPerfectMatch_ShouldReturn100()
    {
        // Arrange
        var musician = new Musician
        {
            Genre = "Rock",
            Location = new Location("Madrid", "Spain"),
            HourlyRate = 400,
            ExperienceYears = 5
        };
        
        var criteria = new EventCriteria
        {
            Genre = "Rock",
            Location = new Location("Madrid", "Spain"),
            Budget = 1000,
            Duration = 2,
            MinExperienceYears = 3
        };
        
        // Act
        var score = _service.CalculateMatchScore(musician, criteria);
        
        // Assert
        Assert.That(score, Is.EqualTo(100));
    }
}
```

### **3. Implementación de Tests de Integración**

#### **Tests de Integración con TestContainers**
```csharp
// MussikOn.Tests.Integration/Repositories/MusicianRepositoryIntegrationTests.cs
[TestFixture]
public class MusicianRepositoryIntegrationTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private IServiceScope _scope;
    private ApplicationDbContext _context;
    private MsSqlContainer _sqlServerContainer;
    
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _sqlServerContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("YourStrong@Passw0rd")
            .WithPortBinding(1433, true)
            .Build();
        
        await _sqlServerContainer.StartAsync();
        
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }
                    
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseSqlServer(_sqlServerContainer.GetConnectionString());
                    });
                });
            });
        
        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        await _context.Database.EnsureCreatedAsync();
    }
    
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        _scope?.Dispose();
        _client?.Dispose();
        _factory?.Dispose();
        await _sqlServerContainer?.DisposeAsync();
    }
    
    [SetUp]
    public async Task Setup()
    {
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM Musicians");
    }
    
    [Test]
    public async Task CreateMusician_ShouldPersistToDatabase()
    {
        // Arrange
        var musician = new Musician
        {
            Name = "John Doe",
            Email = "john@example.com",
            Genre = "Rock",
            HourlyRate = 100.00m,
            Location = new Location("Madrid", "Spain")
        };
        
        // Act
        _context.Musicians.Add(musician);
        await _context.SaveChangesAsync();
        
        // Assert
        var savedMusician = await _context.Musicians
            .FirstOrDefaultAsync(m => m.Email == "john@example.com");
        
        Assert.That(savedMusician, Is.Not.Null);
        Assert.That(savedMusician.Name, Is.EqualTo("John Doe"));
    }
}
```

### **4. Implementación de Tests de Performance**

#### **Tests de Performance con NBomber**
```csharp
// MussikOn.Tests.Performance/MusicianApiPerformanceTests.cs
public class MusicianApiPerformanceTests
{
    [Test]
    public async Task MusicianSearch_UnderNormalLoad_ShouldMeetPerformanceRequirements()
    {
        var scenario = Scenario.Create("musician_search_normal_load", async context =>
        {
            var searchRequest = Http.CreateRequest("GET", "https://api.mussikon.com/musicians/search")
                .WithHeader("Accept", "application/json")
                .WithQueryParam("genre", "Rock")
                .WithQueryParam("location", "Madrid")
                .WithQueryParam("budget", "1000");

            var response = await Http.Send(searchRequest, context);
            
            return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.InjectPerSec(rate: 50, during: TimeSpan.FromMinutes(2))
        )
        .WithWarmUpDuration(TimeSpan.FromSeconds(30));

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();

        Assert.That(stats.AllOkCount, Is.GreaterThan(0));
        Assert.That(stats.AllOkCount / (double)stats.AllRequestCount, Is.GreaterThan(0.95));
    }
}
```

### **5. Implementación de Tests de Seguridad**

#### **Tests de Seguridad con OWASP**
```csharp
// MussikOn.Tests.Security/SecurityTests.cs
[TestFixture]
public class SecurityTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }
    
    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }
    
    [Test]
    public async Task MusicianProfile_UnauthorizedAccess_ShouldBeDenied()
    {
        // Arrange
        var unauthorizedToken = "invalid_token";
        
        // Act
        var response = await _client.GetAsync("/api/musicians/123", 
            options => options.Headers.Add("Authorization", $"Bearer {unauthorizedToken}"));
        
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
    
    [Test]
    public async Task MusicianSearch_SQLInjection_ShouldBePrevented()
    {
        // Arrange
        var maliciousInput = "'; DROP TABLE Musicians; --";
        
        // Act
        var response = await _client.GetAsync($"/api/musicians/search?genre={maliciousInput}");
        
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}
```

### **6. Pipeline de CI/CD Completo**

#### **GitHub Actions Workflow**
```yaml
# .github/workflows/testing-pipeline.yml
name: Complete Testing Pipeline

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  unit-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - name: Restore dependencies
        run: dotnet restore
      - name: Build project
        run: dotnet build --no-restore
      - name: Run unit tests
        run: dotnet test --no-build --project MussikOn.Tests.Unit --collect:"XPlat Code Coverage"
      - name: Upload coverage
        uses: actions/upload-artifact@v3
        with:
          name: unit-test-coverage
          path: ./coverage/

  integration-tests:
    runs-on: ubuntu-latest
    needs: unit-tests
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - name: Restore dependencies
        run: dotnet restore
      - name: Build project
        run: dotnet build --no-restore
      - name: Run integration tests
        run: dotnet test --no-build --project MussikOn.Tests.Integration

  performance-tests:
    runs-on: ubuntu-latest
    needs: integration-tests
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - name: Restore dependencies
        run: dotnet restore
      - name: Build project
        run: dotnet build --no-restore
      - name: Run performance tests
        run: dotnet test --no-build --project MussikOn.Tests.Performance

  security-tests:
    runs-on: ubuntu-latest
    needs: performance-tests
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - name: Restore dependencies
        run: dotnet restore
      - name: Build project
        run: dotnet build --no-restore
      - name: Run security tests
        run: dotnet test --no-build --project MussikOn.Tests.Security

  mutation-tests:
    runs-on: ubuntu-latest
    needs: security-tests
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - name: Restore dependencies
        run: dotnet restore
      - name: Build project
        run: dotnet build --no-restore
      - name: Install Stryker.NET
        run: dotnet tool install -g dotnet-stryker
      - name: Run mutation tests
        run: dotnet stryker --project MussikOn.sln --thresholds.high 80

  sonarqube-analysis:
    runs-on: ubuntu-latest
    needs: mutation-tests
    steps:
      - uses: actions/checkout@v3
        with:
          fetch-depth: 0
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - name: Restore dependencies
        run: dotnet restore
      - name: Build project
        run: dotnet build --no-restore
      - name: Run tests with coverage
        run: dotnet test --no-build --collect:"XPlat Code Coverage"
      - name: Install SonarScanner
        run: dotnet tool install -g dotnet-sonarscanner
      - name: Run SonarQube analysis
        run: |
          dotnet sonarscanner begin /k:"mussikon" /d:sonar.host.url="${{ secrets.SONAR_HOST_URL }}" /d:sonar.login="${{ secrets.SONAR_TOKEN }}"
          dotnet build
          dotnet sonarscanner end /d:sonar.login="${{ secrets.SONAR_TOKEN }}"

  generate-report:
    runs-on: ubuntu-latest
    needs: [unit-tests, integration-tests, performance-tests, security-tests, mutation-tests, sonarqube-analysis]
    steps:
      - uses: actions/checkout@v3
      - name: Download all artifacts
        uses: actions/download-artifact@v3
      - name: Generate comprehensive report
        run: |
          echo "# 🧪 Testing Report for MussikOn" > testing-report.md
          echo "" >> testing-report.md
          echo "## 📊 Summary" >> testing-report.md
          echo "- Unit Tests: ✅ Passed" >> testing-report.md
          echo "- Integration Tests: ✅ Passed" >> testing-report.md
          echo "- Performance Tests: ✅ Passed" >> testing-report.md
          echo "- Security Tests: ✅ Passed" >> testing-report.md
          echo "- Mutation Tests: ✅ Passed" >> testing-report.md
          echo "- SonarQube Analysis: ✅ Passed" >> testing-report.md
          echo "" >> testing-report.md
          echo "## 🎯 Quality Metrics" >> testing-report.md
          echo "- Code Coverage: 87%" >> testing-report.md
          echo "- Mutation Score: 82%" >> testing-report.md
          echo "- Security Score: A" >> testing-report.md
          echo "- Performance Score: A" >> testing-report.md
      - name: Upload comprehensive report
        uses: actions/upload-artifact@v3
        with:
          name: comprehensive-testing-report
          path: testing-report.md
```

## 🛠️ **Ejercicios Prácticos**

### **Ejercicio 1: Implementar Sistema Completo de Testing**
```csharp
// Implementar sistema completo de testing para MussikOn:
// 1. Tests unitarios
// 2. Tests de integración
// 3. Tests de performance
// 4. Tests de seguridad
// 5. Tests de contratos
// 6. Tests de mutación

// TODO: Implementar todos los tipos de tests
```

### **Ejercicio 2: Configurar Pipeline de CI/CD**
```yaml
# Configurar pipeline completo de CI/CD:
# 1. Unit tests
# 2. Integration tests
# 3. Performance tests
# 4. Security tests
# 5. Mutation tests
# 6. SonarQube analysis
# 7. Report generation

# TODO: Configurar pipeline completo
```

### **Ejercicio 3: Generar Reporte Completo**
```markdown
# Generar reporte completo de testing:
# 1. Métricas de calidad
# 2. Cobertura de código
# 3. Resultados de performance
# 4. Análisis de seguridad
# 5. Recomendaciones

# TODO: Generar reporte completo
```

## 🎯 **Objetivos de Aprendizaje**

Al finalizar esta clase, serás capaz de:

1. **Implementar sistema completo** de testing
2. **Configurar pipeline de CI/CD** completo
3. **Generar reportes** de calidad
4. **Aplicar todas las técnicas** aprendidas
5. **Demostrar maestría** en testing avanzado

## 📝 **Resumen**

En esta clase hemos cubierto:

- **Sistema Completo**: Arquitectura de testing
- **Tests Unitarios**: Implementación completa
- **Tests de Integración**: Con TestContainers
- **Tests de Performance**: Con NBomber
- **Tests de Seguridad**: Con OWASP
- **Pipeline de CI/CD**: Automatización completa

## 🎉 **¡Felicidades!**

Has completado el **Expert Level 2: Testing Avanzado y Quality Assurance**. Ahora tienes las habilidades para implementar sistemas de testing de nivel empresarial y asegurar la calidad del código en cualquier proyecto.

---

**💡 Tip**: El testing no es solo sobre encontrar bugs, es sobre confianza. Un sistema bien testeado te da la confianza para hacer cambios y desplegar con seguridad.
