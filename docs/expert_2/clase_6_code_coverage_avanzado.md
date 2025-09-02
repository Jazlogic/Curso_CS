# 📊 **Clase 6: Code Coverage Avanzado**

## 🎯 **Objetivos de la Clase**
- Dominar Code Coverage avanzado con Coverlet
- Implementar Branch Coverage y Line Coverage
- Aplicar Code Coverage en MussikOn
- Optimizar cobertura de código

## 📚 **Contenido Teórico**

### **1. Fundamentos de Code Coverage**

#### **Tipos de Cobertura**
```csharp
// 1. Line Coverage - Líneas de código ejecutadas
public class MusicianService
{
    public async Task<Musician> GetMusicianAsync(int id)
    {
        if (id <= 0) // Línea 1 - Cubierta
        {
            throw new ArgumentException("Invalid ID"); // Línea 2 - Cubierta
        }
        
        var musician = await _repository.GetByIdAsync(id); // Línea 3 - Cubierta
        if (musician == null) // Línea 4 - Cubierta
        {
            throw new NotFoundException("Musician not found"); // Línea 5 - Cubierta
        }
        
        return musician; // Línea 6 - Cubierta
    }
}

// 2. Branch Coverage - Ramas de código ejecutadas
public class EventService
{
    public async Task<Event> CreateEventAsync(CreateEventRequest request)
    {
        if (request == null) // Rama 1: request == null
        {
            throw new ArgumentNullException(nameof(request));
        }
        
        if (string.IsNullOrEmpty(request.Title)) // Rama 2: Title is null/empty
        {
            throw new ArgumentException("Title is required");
        }
        
        if (request.Budget <= 0) // Rama 3: Budget <= 0
        {
            throw new ArgumentException("Budget must be positive");
        }
        
        // Rama 4: All validations pass
        var event = new Event
        {
            Title = request.Title,
            Description = request.Description,
            Budget = request.Budget,
            Date = request.Date
        };
        
        return await _repository.CreateAsync(event);
    }
}

// 3. Method Coverage - Métodos ejecutados
public class PaymentService
{
    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        // Método 1: ProcessPaymentAsync - Cubierto
        var validationResult = ValidatePaymentRequest(request); // Método 2: ValidatePaymentRequest - Cubierto
        if (!validationResult.IsValid)
        {
            return new PaymentResult { Success = false, Error = validationResult.Error };
        }
        
        var paymentResult = await ChargePaymentAsync(request); // Método 3: ChargePaymentAsync - Cubierto
        await LogPaymentAsync(paymentResult); // Método 4: LogPaymentAsync - Cubierto
        
        return paymentResult;
    }
    
    private ValidationResult ValidatePaymentRequest(PaymentRequest request)
    {
        // Método 2: ValidatePaymentRequest - Cubierto
        if (request.Amount <= 0)
        {
            return new ValidationResult { IsValid = false, Error = "Amount must be positive" };
        }
        
        return new ValidationResult { IsValid = true };
    }
    
    private async Task<PaymentResult> ChargePaymentAsync(PaymentRequest request)
    {
        // Método 3: ChargePaymentAsync - Cubierto
        // Simular procesamiento de pago
        await Task.Delay(100);
        return new PaymentResult { Success = true, TransactionId = Guid.NewGuid().ToString() };
    }
    
    private async Task LogPaymentAsync(PaymentResult result)
    {
        // Método 4: LogPaymentAsync - Cubierto
        await _logger.LogAsync($"Payment processed: {result.TransactionId}");
    }
}
```

### **2. Implementación con Coverlet**

#### **Configuración de Coverlet**
```xml
<!-- MussikOn.Tests.Unit.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="NUnit" Version="4.0.1" />
    <PackageReference Include="NUnit3TestAdapter" Version="4.5.0" />
    <PackageReference Include="coverlet.collector" Version="6.0.0" />
    <PackageReference Include="coverlet.msbuild" Version="6.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\src\MussikOn.Core\MussikOn.Core.csproj" />
    <ProjectReference Include="..\src\MussikOn.Infrastructure\MussikOn.Infrastructure.csproj" />
  </ItemGroup>
</Project>
```

#### **Configuración de Cobertura**
```json
// coverlet.runsettings
{
  "DataCollectionRunSettings": {
    "DataCollectors": [
      {
        "friendlyName": "XPlat code coverage",
        "uri": "datacollector://Microsoft/CodeCoverage/2.0",
        "assemblyQualifiedName": "Coverlet.Collector.DataCollection.CoverletInProcDataCollector, coverlet.collector, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
        "configuration": {
          "exclude": [
            "[*.Tests]*",
            "[*.Test]*",
            "[*]*.Program",
            "[*]*.Startup"
          ],
          "excludeByAttribute": [
            "Obsolete",
            "GeneratedCodeAttribute",
            "CompilerGeneratedAttribute"
          ],
          "excludeByFile": [
            "**/Migrations/**",
            "**/bin/**",
            "**/obj/**"
          ],
          "include": [
            "[MussikOn.Core]*",
            "[MussikOn.Infrastructure]*",
            "[MussikOn.Application]*"
          ],
          "singleHit": false,
          "skipAutoProps": true,
          "threshold": 80
        }
      }
    ]
  }
}
```

#### **Ejecución de Code Coverage**
```bash
# Ejecutar tests con cobertura
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings

# Generar reporte HTML
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Usar reportgenerator para generar reportes
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"./coverage/**/coverage.cobertura.xml" -targetdir:"./coverage/report" -reporttypes:Html
```

### **3. Análisis de Cobertura en MussikOn**

#### **Cobertura de MusicianMatchingService**
```csharp
public class MusicianMatchingService
{
    private readonly IMusicianRepository _musicianRepository;
    private readonly IEventRepository _eventRepository;
    private readonly ILogger<MusicianMatchingService> _logger;
    
    public MusicianMatchingService(
        IMusicianRepository musicianRepository,
        IEventRepository eventRepository,
        ILogger<MusicianMatchingService> logger)
    {
        _musicianRepository = musicianRepository;
        _eventRepository = eventRepository;
        _logger = logger;
    }
    
    public async Task<List<MusicianMatch>> FindMatchingMusiciansAsync(EventCriteria criteria)
    {
        // Línea 1: Validación de entrada
        if (criteria == null)
        {
            throw new ArgumentNullException(nameof(criteria));
        }
        
        // Línea 2: Logging
        _logger.LogInformation("Finding matching musicians for event criteria: {Criteria}", criteria);
        
        try
        {
            // Línea 3: Obtener músicos disponibles
            var availableMusicians = await _musicianRepository.GetAvailableMusiciansAsync(criteria);
            
            // Línea 4: Validar resultados
            if (availableMusicians == null || !availableMusicians.Any())
            {
                _logger.LogWarning("No available musicians found for criteria: {Criteria}", criteria);
                return new List<MusicianMatch>();
            }
            
            // Línea 5: Calcular matches
            var matches = new List<MusicianMatch>();
            foreach (var musician in availableMusicians)
            {
                var score = CalculateMatchScore(musician, criteria);
                if (score > 0)
                {
                    matches.Add(new MusicianMatch
                    {
                        Musician = musician,
                        Score = score,
                        MatchReasons = GetMatchReasons(musician, criteria)
                    });
                }
            }
            
            // Línea 6: Ordenar por score
            var orderedMatches = matches.OrderByDescending(m => m.Score).ToList();
            
            // Línea 7: Logging de resultados
            _logger.LogInformation("Found {Count} matching musicians", orderedMatches.Count);
            
            return orderedMatches;
        }
        catch (Exception ex)
        {
            // Línea 8: Manejo de errores
            _logger.LogError(ex, "Error finding matching musicians for criteria: {Criteria}", criteria);
            throw;
        }
    }
    
    private decimal CalculateMatchScore(Musician musician, EventCriteria criteria)
    {
        // Línea 9: Validación de entrada
        if (musician == null || criteria == null)
        {
            return 0;
        }
        
        decimal score = 0;
        
        // Línea 10: Matching por género
        if (musician.Genre == criteria.Genre)
        {
            score += 40;
        }
        
        // Línea 11: Matching por ubicación
        if (musician.Location?.City == criteria.Location?.City)
        {
            score += 30;
        }
        
        // Línea 12: Matching por presupuesto
        if (musician.HourlyRate <= criteria.Budget / criteria.Duration)
        {
            score += 20;
        }
        
        // Línea 13: Matching por experiencia
        if (musician.ExperienceYears >= criteria.MinExperienceYears)
        {
            score += 10;
        }
        
        return score;
    }
    
    private List<string> GetMatchReasons(Musician musician, EventCriteria criteria)
    {
        // Línea 14: Validación de entrada
        if (musician == null || criteria == null)
        {
            return new List<string>();
        }
        
        var reasons = new List<string>();
        
        // Línea 15: Razón de género
        if (musician.Genre == criteria.Genre)
        {
            reasons.Add("Genre match");
        }
        
        // Línea 16: Razón de ubicación
        if (musician.Location?.City == criteria.Location?.City)
        {
            reasons.Add("Location match");
        }
        
        // Línea 17: Razón de presupuesto
        if (musician.HourlyRate <= criteria.Budget / criteria.Duration)
        {
            reasons.Add("Budget match");
        }
        
        // Línea 18: Razón de experiencia
        if (musician.ExperienceYears >= criteria.MinExperienceYears)
        {
            reasons.Add("Experience match");
        }
        
        return reasons;
    }
}
```

#### **Tests para Cobertura Completa**
```csharp
[TestFixture]
public class MusicianMatchingServiceCoverageTests
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
        _service = new MusicianMatchingService(_mockMusicianRepository.Object, _mockEventRepository.Object, _mockLogger.Object);
    }
    
    [Test]
    public async Task FindMatchingMusicians_WithNullCriteria_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.FindMatchingMusiciansAsync(null));
        
        Assert.That(exception.ParamName, Is.EqualTo("criteria"));
    }
    
    [Test]
    public async Task FindMatchingMusicians_WithNoAvailableMusicians_ShouldReturnEmptyList()
    {
        // Arrange
        var criteria = new EventCriteria { Genre = "Rock" };
        _mockMusicianRepository.Setup(r => r.GetAvailableMusiciansAsync(criteria))
                             .ReturnsAsync(new List<Musician>());
        
        // Act
        var result = await _service.FindMatchingMusiciansAsync(criteria);
        
        // Assert
        Assert.That(result, Is.Empty);
    }
    
    [Test]
    public async Task FindMatchingMusicians_WithNullMusicians_ShouldReturnEmptyList()
    {
        // Arrange
        var criteria = new EventCriteria { Genre = "Rock" };
        _mockMusicianRepository.Setup(r => r.GetAvailableMusiciansAsync(criteria))
                             .ReturnsAsync((List<Musician>)null);
        
        // Act
        var result = await _service.FindMatchingMusiciansAsync(criteria);
        
        // Assert
        Assert.That(result, Is.Empty);
    }
    
    [Test]
    public async Task FindMatchingMusicians_WithException_ShouldLogAndRethrow()
    {
        // Arrange
        var criteria = new EventCriteria { Genre = "Rock" };
        _mockMusicianRepository.Setup(r => r.GetAvailableMusiciansAsync(criteria))
                             .ThrowsAsync(new Exception("Database error"));
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _service.FindMatchingMusiciansAsync(criteria));
        
        Assert.That(exception.Message, Is.EqualTo("Database error"));
        
        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error finding matching musicians")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
    
    [Test]
    public void CalculateMatchScore_WithNullMusician_ShouldReturnZero()
    {
        // Arrange
        var criteria = new EventCriteria { Genre = "Rock" };
        
        // Act
        var score = _service.CalculateMatchScore(null, criteria);
        
        // Assert
        Assert.That(score, Is.EqualTo(0));
    }
    
    [Test]
    public void CalculateMatchScore_WithNullCriteria_ShouldReturnZero()
    {
        // Arrange
        var musician = new Musician { Genre = "Rock" };
        
        // Act
        var score = _service.CalculateMatchScore(musician, null);
        
        // Assert
        Assert.That(score, Is.EqualTo(0));
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
    
    [Test]
    public void CalculateMatchScore_WithPartialMatch_ShouldReturnCorrectScore()
    {
        // Arrange
        var musician = new Musician
        {
            Genre = "Rock",
            Location = new Location("Barcelona", "Spain"), // Different location
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
        Assert.That(score, Is.EqualTo(70)); // 40 (genre) + 20 (budget) + 10 (experience)
    }
    
    [Test]
    public void GetMatchReasons_WithNullMusician_ShouldReturnEmptyList()
    {
        // Arrange
        var criteria = new EventCriteria { Genre = "Rock" };
        
        // Act
        var reasons = _service.GetMatchReasons(null, criteria);
        
        // Assert
        Assert.That(reasons, Is.Empty);
    }
    
    [Test]
    public void GetMatchReasons_WithNullCriteria_ShouldReturnEmptyList()
    {
        // Arrange
        var musician = new Musician { Genre = "Rock" };
        
        // Act
        var reasons = _service.GetMatchReasons(musician, null);
        
        // Assert
        Assert.That(reasons, Is.Empty);
    }
    
    [Test]
    public void GetMatchReasons_WithPerfectMatch_ShouldReturnAllReasons()
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
        var reasons = _service.GetMatchReasons(musician, criteria);
        
        // Assert
        Assert.That(reasons, Has.Count.EqualTo(4));
        Assert.That(reasons, Contains.Item("Genre match"));
        Assert.That(reasons, Contains.Item("Location match"));
        Assert.That(reasons, Contains.Item("Budget match"));
        Assert.That(reasons, Contains.Item("Experience match"));
    }
}
```

### **4. Integración con CI/CD**

#### **Pipeline de Code Coverage**
```yaml
# .github/workflows/code-coverage.yml
name: Code Coverage

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  code-coverage:
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
        
      - name: Run tests with coverage
        run: dotnet test --no-build --collect:"XPlat Code Coverage" --results-directory ./coverage
        
      - name: Install reportgenerator
        run: dotnet tool install -g dotnet-reportgenerator-globaltool
        
      - name: Generate coverage report
        run: reportgenerator -reports:"./coverage/**/coverage.cobertura.xml" -targetdir:"./coverage/report" -reporttypes:Html
        
      - name: Upload coverage report
        uses: actions/upload-artifact@v3
        with:
          name: coverage-report
          path: ./coverage/report/
          
      - name: Comment PR with coverage
        if: github.event_name == 'pull_request'
        uses: actions/github-script@v6
        with:
          script: |
            const fs = require('fs');
            const path = require('path');
            
            // Read coverage report
            const reportPath = path.join('./coverage/report', 'index.html');
            if (fs.existsSync(reportPath)) {
              const report = fs.readFileSync(reportPath, 'utf8');
              
              // Extract coverage percentage (simplified)
              const coverageMatch = report.match(/Line coverage: (\d+\.?\d*)%/);
              const coverage = coverageMatch ? parseFloat(coverageMatch[1]) : 0;
              
              const comment = `## 📊 Code Coverage Results
              
              **Line Coverage**: ${coverage.toFixed(2)}%
              
              ${coverage < 80 ? '⚠️ **Warning**: Code coverage is below the threshold (80%)' : '✅ **Success**: Code coverage meets requirements'}
              
              [View detailed report](https://github.com/${context.repo.owner}/${context.repo.repo}/actions/runs/${context.runId})
              `;
              
              github.rest.issues.createComment({
                issue_number: context.issue.number,
                owner: context.repo.owner,
                repo: context.repo.repo,
                body: comment
              });
            }
```

## 🛠️ **Ejercicios Prácticos**

### **Ejercicio 1: Implementar Code Coverage para MussikOn**
```csharp
// Implementar code coverage para:
// 1. MusicianMatchingService
// 2. PaymentService
// 3. ChatService
// 4. NotificationService

[Test]
public void MusicianMatchingService_ShouldHaveHighCodeCoverage()
{
    // TODO: Implementar tests para alta cobertura
}

[Test]
public void PaymentService_ShouldHaveHighCodeCoverage()
{
    // TODO: Implementar tests para alta cobertura
}
```

### **Ejercicio 2: Configurar Coverlet**
```json
// Configurar Coverlet para MussikOn:
// 1. coverlet.runsettings
// 2. Exclusions apropiadas
// 3. Thresholds configurados
// 4. CI/CD integration

{
  "DataCollectionRunSettings": {
    "DataCollectors": [
      {
        "friendlyName": "XPlat code coverage",
        "uri": "datacollector://Microsoft/CodeCoverage/2.0",
        "assemblyQualifiedName": "Coverlet.Collector.DataCollection.CoverletInProcDataCollector, coverlet.collector, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
        "configuration": {
          "exclude": [
            "[*.Tests]*",
            "[*.Test]*"
          ],
          "threshold": 85
        }
      }
    ]
  }
}
```

### **Ejercicio 3: Optimizar Cobertura**
```csharp
// Optimizar cobertura de código:
// 1. Identificar líneas no cubiertas
// 2. Agregar tests para casos edge
// 3. Mejorar assertions
// 4. Cubrir ramas condicionales

[Test]
public void MusicianMatchingService_WithEdgeCases_ShouldHaveCompleteCoverage()
{
    // TODO: Implementar tests para casos edge
}
```

## 🎯 **Objetivos de Aprendizaje**

Al finalizar esta clase, serás capaz de:

1. **Implementar Code Coverage** con Coverlet
2. **Analizar cobertura** de línea, rama y método
3. **Optimizar cobertura** de código
4. **Integrar code coverage** en CI/CD
5. **Interpretar reportes** de cobertura

## 📝 **Resumen**

En esta clase hemos cubierto:

- **Code Coverage**: Tipos y fundamentos
- **Coverlet**: Configuración y ejecución
- **Análisis de Cobertura**: Línea, rama, método
- **Tests para Cobertura**: Casos completos
- **CI/CD Integration**: Pipeline automatizado

## 🚀 **Siguiente Clase**

En la próxima clase exploraremos **Static Code Analysis** con herramientas como SonarQube para mantener la calidad del código en MussikOn.

---

**💡 Tip**: La cobertura de código no es el objetivo final, es una métrica. Enfócate en la calidad de los tests, no solo en la cantidad.
