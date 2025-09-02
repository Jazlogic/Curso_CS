# 🧬 **Clase 5: Mutation Testing**

## 🎯 **Objetivos de la Clase**
- Dominar Mutation Testing para evaluar calidad de tests
- Implementar Stryker.NET para .NET
- Aplicar Mutation Testing en MussikOn
- Mejorar cobertura y calidad de tests

## 📚 **Contenido Teórico**

### **1. Fundamentos de Mutation Testing**

#### **¿Qué es Mutation Testing?**
```csharp
// Mutation Testing evalúa la calidad de los tests introduciendo cambios (mutaciones)
// en el código y verificando si los tests los detectan

// Código original
public class MusicianMatchingService
{
    public decimal CalculateMatchScore(Musician musician, EventCriteria criteria)
    {
        if (musician.Genre == criteria.Genre)
        {
            return 100.0m; // Mutación: cambiar a 50.0m
        }
        
        if (musician.Location.City == criteria.Location.City)
        {
            return 80.0m; // Mutación: cambiar a 40.0m
        }
        
        return 0.0m; // Mutación: cambiar a 10.0m
    }
}

// Test que debería detectar las mutaciones
[Test]
public void CalculateMatchScore_WithMatchingGenre_ShouldReturn100()
{
    // Arrange
    var musician = new Musician { Genre = "Rock" };
    var criteria = new EventCriteria { Genre = "Rock" };
    var service = new MusicianMatchingService();
    
    // Act
    var score = service.CalculateMatchScore(musician, criteria);
    
    // Assert
    Assert.That(score, Is.EqualTo(100.0m)); // Debería fallar con mutación
}
```

#### **Tipos de Mutaciones**
```csharp
public class MusicianService
{
    // 1. Mutación de Operadores Aritméticos
    public decimal CalculateTotalRate(Musician musician, int hours)
    {
        return musician.HourlyRate * hours; // Mutación: cambiar * por +
    }
    
    // 2. Mutación de Operadores de Comparación
    public bool IsAvailable(Musician musician, DateTime date)
    {
        return musician.AvailabilityDate >= date; // Mutación: cambiar >= por >
    }
    
    // 3. Mutación de Operadores Lógicos
    public bool CanPerform(Musician musician, EventCriteria criteria)
    {
        return musician.Genre == criteria.Genre && musician.IsAvailable; // Mutación: cambiar && por ||
    }
    
    // 4. Mutación de Valores Literales
    public decimal GetDiscountRate(Musician musician)
    {
        if (musician.ExperienceYears > 5)
        {
            return 0.1m; // Mutación: cambiar 0.1m por 0.2m
        }
        return 0.0m; // Mutación: cambiar 0.0m por 0.05m
    }
    
    // 5. Mutación de Condiciones
    public string GetStatus(Musician musician)
    {
        if (musician.IsActive) // Mutación: cambiar condición
        {
            return "Active";
        }
        return "Inactive";
    }
}
```

### **2. Implementación con Stryker.NET**

#### **Configuración de Stryker.NET**
```json
// stryker-config.json
{
  "stryker-config": {
    "project": "MussikOn.sln",
    "test-projects": [
      "MussikOn.Tests.Unit",
      "MussikOn.Tests.Integration"
    ],
    "mutate": [
      "src/**/*.cs",
      "!src/**/*.Designer.cs",
      "!src/**/*.Generated.cs"
    ],
    "test-projects": [
      "tests/**/*.cs"
    ],
    "reporters": [
      "html",
      "json",
      "progress"
    ],
    "thresholds": {
      "high": 80,
      "low": 60,
      "break": 70
    },
    "log-level": "info",
    "log-file": true,
    "output": "./mutation-reports"
  }
}
```

#### **Ejecución de Mutation Testing**
```bash
# Instalar Stryker.NET
dotnet tool install -g dotnet-stryker

# Ejecutar mutation testing
dotnet stryker --config-file stryker-config.json

# Ejecutar con configuración específica
dotnet stryker --project MussikOn.sln --test-projects "MussikOn.Tests.Unit" --thresholds.high 85
```

#### **Mutation Testing de MusicianMatchingService**
```csharp
public class MusicianMatchingService
{
    private readonly IMusicianRepository _musicianRepository;
    private readonly IEventRepository _eventRepository;
    
    public MusicianMatchingService(IMusicianRepository musicianRepository, IEventRepository eventRepository)
    {
        _musicianRepository = musicianRepository;
        _eventRepository = eventRepository;
    }
    
    public async Task<List<MusicianMatch>> FindMatchingMusiciansAsync(EventCriteria criteria)
    {
        var availableMusicians = await _musicianRepository.GetAvailableMusiciansAsync(criteria);
        var matches = new List<MusicianMatch>();
        
        foreach (var musician in availableMusicians)
        {
            var score = CalculateMatchScore(musician, criteria);
            if (score > 0) // Mutación: cambiar > por >=
            {
                matches.Add(new MusicianMatch
                {
                    Musician = musician,
                    Score = score,
                    MatchReasons = GetMatchReasons(musician, criteria)
                });
            }
        }
        
        return matches.OrderByDescending(m => m.Score).ToList();
    }
    
    private decimal CalculateMatchScore(Musician musician, EventCriteria criteria)
    {
        decimal score = 0;
        
        // Genre matching (40% weight)
        if (musician.Genre == criteria.Genre)
        {
            score += 40; // Mutación: cambiar 40 por 20
        }
        
        // Location matching (30% weight)
        if (musician.Location.City == criteria.Location.City)
        {
            score += 30; // Mutación: cambiar 30 por 15
        }
        
        // Budget matching (20% weight)
        if (musician.HourlyRate <= criteria.Budget / criteria.Duration)
        {
            score += 20; // Mutación: cambiar 20 por 10
        }
        
        // Experience matching (10% weight)
        if (musician.ExperienceYears >= criteria.MinExperienceYears)
        {
            score += 10; // Mutación: cambiar 10 por 5
        }
        
        return score;
    }
    
    private List<string> GetMatchReasons(Musician musician, EventCriteria criteria)
    {
        var reasons = new List<string>();
        
        if (musician.Genre == criteria.Genre)
        {
            reasons.Add("Genre match"); // Mutación: cambiar texto
        }
        
        if (musician.Location.City == criteria.Location.City)
        {
            reasons.Add("Location match"); // Mutación: cambiar texto
        }
        
        if (musician.HourlyRate <= criteria.Budget / criteria.Duration)
        {
            reasons.Add("Budget match"); // Mutación: cambiar texto
        }
        
        return reasons;
    }
}
```

#### **Tests que Deben Detectar Mutaciones**
```csharp
[TestFixture]
public class MusicianMatchingServiceTests
{
    private MusicianMatchingService _service;
    private Mock<IMusicianRepository> _mockMusicianRepository;
    private Mock<IEventRepository> _mockEventRepository;
    
    [SetUp]
    public void Setup()
    {
        _mockMusicianRepository = new Mock<IMusicianRepository>();
        _mockEventRepository = new Mock<IEventRepository>();
        _service = new MusicianMatchingService(_mockMusicianRepository.Object, _mockEventRepository.Object);
    }
    
    [Test]
    public async Task FindMatchingMusicians_WithPerfectMatch_ShouldReturnMusicianWithScore100()
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
        
        var musician = new Musician
        {
            Id = 1,
            Genre = "Rock",
            Location = new Location("Madrid", "Spain"),
            HourlyRate = 400,
            ExperienceYears = 5
        };
        
        _mockMusicianRepository.Setup(r => r.GetAvailableMusiciansAsync(criteria))
                             .ReturnsAsync(new List<Musician> { musician });
        
        // Act
        var matches = await _service.FindMatchingMusiciansAsync(criteria);
        
        // Assert
        Assert.That(matches, Has.Count.EqualTo(1));
        Assert.That(matches.First().Score, Is.EqualTo(100));
        Assert.That(matches.First().MatchReasons, Has.Count.EqualTo(4));
    }
    
    [Test]
    public async Task FindMatchingMusicians_WithNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var criteria = new EventCriteria
        {
            Genre = "Classical",
            Location = new Location("Madrid", "Spain"),
            Budget = 500,
            Duration = 2,
            MinExperienceYears = 10
        };
        
        var musician = new Musician
        {
            Id = 1,
            Genre = "Rock",
            Location = new Location("Barcelona", "Spain"),
            HourlyRate = 800,
            ExperienceYears = 2
        };
        
        _mockMusicianRepository.Setup(r => r.GetAvailableMusiciansAsync(criteria))
                             .ReturnsAsync(new List<Musician> { musician });
        
        // Act
        var matches = await _service.FindMatchingMusiciansAsync(criteria);
        
        // Assert
        Assert.That(matches, Is.Empty);
    }
    
    [Test]
    public async Task FindMatchingMusicians_WithPartialMatch_ShouldReturnMusicianWithCorrectScore()
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
        
        var musician = new Musician
        {
            Id = 1,
            Genre = "Rock",
            Location = new Location("Barcelona", "Spain"), // Different location
            HourlyRate = 400,
            ExperienceYears = 5
        };
        
        _mockMusicianRepository.Setup(r => r.GetAvailableMusiciansAsync(criteria))
                             .ReturnsAsync(new List<Musician> { musician });
        
        // Act
        var matches = await _service.FindMatchingMusiciansAsync(criteria);
        
        // Assert
        Assert.That(matches, Has.Count.EqualTo(1));
        Assert.That(matches.First().Score, Is.EqualTo(70)); // 40 (genre) + 20 (budget) + 10 (experience)
        Assert.That(matches.First().MatchReasons, Has.Count.EqualTo(3));
    }
    
    [Test]
    public async Task FindMatchingMusicians_WithMultipleMusicians_ShouldReturnOrderedByScore()
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
            new Musician
            {
                Id = 1,
                Genre = "Rock",
                Location = new Location("Barcelona", "Spain"),
                HourlyRate = 400,
                ExperienceYears = 5
            },
            new Musician
            {
                Id = 2,
                Genre = "Rock",
                Location = new Location("Madrid", "Spain"),
                HourlyRate = 400,
                ExperienceYears = 5
            }
        };
        
        _mockMusicianRepository.Setup(r => r.GetAvailableMusiciansAsync(criteria))
                             .ReturnsAsync(musicians);
        
        // Act
        var matches = await _service.FindMatchingMusiciansAsync(criteria);
        
        // Assert
        Assert.That(matches, Has.Count.EqualTo(2));
        Assert.That(matches.First().Score, Is.EqualTo(100)); // Perfect match
        Assert.That(matches.Last().Score, Is.EqualTo(70)); // Partial match
        Assert.That(matches.First().Musician.Id, Is.EqualTo(2));
    }
}
```

### **3. Análisis de Resultados de Mutation Testing**

#### **Interpretación de Reportes**
```csharp
// Ejemplo de reporte de Stryker.NET
public class MutationTestReport
{
    public string ProjectName { get; set; } = "MussikOn";
    public DateTime GeneratedAt { get; set; }
    public MutationTestSummary Summary { get; set; }
    public List<MutationTestFile> Files { get; set; }
}

public class MutationTestSummary
{
    public int TotalMutations { get; set; }
    public int KilledMutations { get; set; }
    public int SurvivedMutations { get; set; }
    public int NoCoverageMutations { get; set; }
    public int CompileErrorMutations { get; set; }
    public int TimeoutMutations { get; set; }
    public double MutationScore { get; set; }
}

public class MutationTestFile
{
    public string FileName { get; set; }
    public List<Mutation> Mutations { get; set; }
    public double MutationScore { get; set; }
}

public class Mutation
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string Status { get; set; } // Killed, Survived, NoCoverage, CompileError, Timeout
    public string OriginalCode { get; set; }
    public string MutatedCode { get; set; }
    public string Description { get; set; }
    public List<string> TestResults { get; set; }
}
```

#### **Mejora de Tests Basada en Resultados**
```csharp
// Ejemplo de test mejorado basado en mutation testing
[Test]
public async Task FindMatchingMusicians_WithZeroScore_ShouldNotIncludeMusician()
{
    // Arrange
    var criteria = new EventCriteria
    {
        Genre = "Classical",
        Location = new Location("Madrid", "Spain"),
        Budget = 500,
        Duration = 2,
        MinExperienceYears = 10
    };
    
    var musician = new Musician
    {
        Id = 1,
        Genre = "Rock", // Different genre
        Location = new Location("Barcelona", "Spain"), // Different location
        HourlyRate = 800, // Over budget
        ExperienceYears = 2 // Insufficient experience
    };
    
    _mockMusicianRepository.Setup(r => r.GetAvailableMusiciansAsync(criteria))
                         .ReturnsAsync(new List<Musician> { musician });
    
    // Act
    var matches = await _service.FindMatchingMusiciansAsync(criteria);
    
    // Assert
    Assert.That(matches, Is.Empty);
    
    // Verificar que el score calculado es 0
    var score = _service.CalculateMatchScore(musician, criteria);
    Assert.That(score, Is.EqualTo(0));
}

[Test]
public void CalculateMatchScore_WithExactBudgetMatch_ShouldAddBudgetScore()
{
    // Arrange
    var musician = new Musician
    {
        HourlyRate = 500
    };
    
    var criteria = new EventCriteria
    {
        Budget = 1000,
        Duration = 2 // 500 per hour
    };
    
    // Act
    var score = _service.CalculateMatchScore(musician, criteria);
    
    // Assert
    Assert.That(score, Is.EqualTo(20)); // Budget score
}

[Test]
public void CalculateMatchScore_WithOverBudget_ShouldNotAddBudgetScore()
{
    // Arrange
    var musician = new Musician
    {
        HourlyRate = 600
    };
    
    var criteria = new EventCriteria
    {
        Budget = 1000,
        Duration = 2 // 500 per hour
    };
    
    // Act
    var score = _service.CalculateMatchScore(musician, criteria);
    
    // Assert
    Assert.That(score, Is.EqualTo(0)); // No budget score
}
```

### **4. Integración con CI/CD**

#### **Pipeline de Mutation Testing**
```yaml
# .github/workflows/mutation-testing.yml
name: Mutation Testing

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  mutation-testing:
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
        run: dotnet test --no-build --verbosity normal
        
      - name: Install Stryker.NET
        run: dotnet tool install -g dotnet-stryker
        
      - name: Run mutation testing
        run: dotnet stryker --project MussikOn.sln --thresholds.high 80 --thresholds.low 60 --thresholds.break 70
        
      - name: Upload mutation report
        uses: actions/upload-artifact@v3
        with:
          name: mutation-report
          path: ./mutation-reports/
          
      - name: Comment PR with mutation score
        if: github.event_name == 'pull_request'
        uses: actions/github-script@v6
        with:
          script: |
            const fs = require('fs');
            const path = require('path');
            
            // Read mutation report
            const reportPath = path.join('./mutation-reports', 'mutation-report.json');
            if (fs.existsSync(reportPath)) {
              const report = JSON.parse(fs.readFileSync(reportPath, 'utf8'));
              const score = report.summary.mutationScore;
              
              const comment = `## 🧬 Mutation Testing Results
              
              **Mutation Score**: ${score.toFixed(2)}%
              
              - **Total Mutations**: ${report.summary.totalMutations}
              - **Killed**: ${report.summary.killedMutations}
              - **Survived**: ${report.summary.survivedMutations}
              
              ${score < 70 ? '⚠️ **Warning**: Mutation score is below the break threshold (70%)' : '✅ **Success**: Mutation score meets requirements'}
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

### **Ejercicio 1: Implementar Mutation Testing para MussikOn**
```csharp
// Implementar mutation testing para:
// 1. MusicianMatchingService
// 2. PaymentService
// 3. ChatService
// 4. NotificationService

[Test]
public void MusicianMatchingService_ShouldHaveHighMutationScore()
{
    // TODO: Implementar tests que maten mutaciones
}

[Test]
public void PaymentService_ShouldHaveHighMutationScore()
{
    // TODO: Implementar tests que maten mutaciones
}
```

### **Ejercicio 2: Configurar Stryker.NET**
```json
// Configurar Stryker.NET para MussikOn:
// 1. stryker-config.json
// 2. Thresholds apropiados
// 3. Reporters configurados
// 4. CI/CD integration

{
  "stryker-config": {
    "project": "MussikOn.sln",
    "test-projects": [
      "MussikOn.Tests.Unit"
    ],
    "mutate": [
      "src/**/*.cs"
    ],
    "reporters": [
      "html",
      "json"
    ],
    "thresholds": {
      "high": 85,
      "low": 70,
      "break": 75
    }
  }
}
```

### **Ejercicio 3: Mejorar Tests Basado en Mutation Results**
```csharp
// Mejorar tests basado en resultados de mutation testing:
// 1. Agregar tests para casos edge
// 2. Mejorar assertions
// 3. Cubrir mutaciones sobrevivientes
// 4. Optimizar cobertura

[Test]
public void MusicianMatchingService_WithEdgeCases_ShouldHandleCorrectly()
{
    // TODO: Implementar tests para casos edge
}
```

## 🎯 **Objetivos de Aprendizaje**

Al finalizar esta clase, serás capaz de:

1. **Implementar Mutation Testing** con Stryker.NET
2. **Interpretar reportes** de mutation testing
3. **Mejorar tests** basado en resultados
4. **Integrar mutation testing** en CI/CD
5. **Optimizar cobertura** y calidad de tests

## 📝 **Resumen**

En esta clase hemos cubierto:

- **Mutation Testing**: Fundamentos y tipos de mutaciones
- **Stryker.NET**: Configuración y ejecución
- **Análisis de Resultados**: Interpretación de reportes
- **Mejora de Tests**: Basada en mutaciones sobrevivientes
- **CI/CD Integration**: Pipeline automatizado

## 🚀 **Siguiente Clase**

En la próxima clase exploraremos **Code Coverage Avanzado** para medir y mejorar la cobertura de código en MussikOn.

---

**💡 Tip**: El mutation testing no es sobre cobertura, es sobre calidad. Un test que no detecta mutaciones no está probando realmente el código.
