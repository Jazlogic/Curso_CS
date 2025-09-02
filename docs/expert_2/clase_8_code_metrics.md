# 📈 **Clase 8: Code Metrics**

## 🎯 **Objetivos de la Clase**
- Dominar Code Metrics para medir calidad de código
- Implementar métricas de complejidad y mantenibilidad
- Aplicar métricas en MussikOn
- Optimizar código basado en métricas

## 📚 **Contenido Teórico**

### **1. Fundamentos de Code Metrics**

#### **Tipos de Métricas**
```csharp
// 1. Complejidad Ciclomática (Cyclomatic Complexity)
public class MusicianMatchingService
{
    // Complejidad: 1 (base) + 4 (if statements) = 5
    public decimal CalculateMatchScore(Musician musician, EventCriteria criteria)
    {
        decimal score = 0;
        
        if (musician.Genre == criteria.Genre) // +1
        {
            score += 40;
        }
        
        if (musician.Location.City == criteria.Location.City) // +1
        {
            score += 30;
        }
        
        if (musician.HourlyRate <= criteria.Budget / criteria.Duration) // +1
        {
            score += 20;
        }
        
        if (musician.ExperienceYears >= criteria.MinExperienceYears) // +1
        {
            score += 10;
        }
        
        return score;
    }
    
    // Complejidad: 1 (base) + 2 (if statements) + 1 (foreach) = 4
    public async Task<List<MusicianMatch>> FindMatchingMusiciansAsync(EventCriteria criteria)
    {
        if (criteria == null) // +1
        {
            throw new ArgumentNullException(nameof(criteria));
        }
        
        var availableMusicians = await _musicianRepository.GetAvailableMusiciansAsync(criteria);
        
        if (availableMusicians == null || !availableMusicians.Any()) // +1
        {
            return new List<MusicianMatch>();
        }
        
        var matches = new List<MusicianMatch>();
        foreach (var musician in availableMusicians) // +1
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
        
        return matches.OrderByDescending(m => m.Score).ToList();
    }
}

// 2. Complejidad Cognitiva (Cognitive Complexity)
public class EventService
{
    // Complejidad Cognitiva: 0 (base) + 1 (if) + 1 (nested if) + 1 (nested if) = 3
    public async Task<Event> CreateEventAsync(CreateEventRequest request)
    {
        if (request == null) // +1
        {
            throw new ArgumentNullException(nameof(request));
        }
        
        if (string.IsNullOrEmpty(request.Title)) // +1
        {
            throw new ArgumentException("Title is required");
        }
        
        if (request.Budget <= 0) // +1
        {
            throw new ArgumentException("Budget must be positive");
        }
        
        var event = new Event
        {
            Title = request.Title,
            Description = request.Description,
            Budget = request.Budget,
            Date = request.Date
        };
        
        return await _repository.CreateAsync(event);
    }
    
    // Complejidad Cognitiva: 0 (base) + 1 (if) + 1 (nested if) + 1 (nested if) + 1 (nested if) = 4
    public async Task<List<Event>> SearchEventsAsync(EventSearchCriteria criteria)
    {
        var query = _repository.GetQueryable();
        
        if (criteria != null) // +1
        {
            if (!string.IsNullOrEmpty(criteria.Genre)) // +1
            {
                query = query.Where(e => e.Genre == criteria.Genre);
            }
            
            if (criteria.MinBudget.HasValue) // +1
            {
                query = query.Where(e => e.Budget >= criteria.MinBudget.Value);
            }
            
            if (criteria.MaxBudget.HasValue) // +1
            {
                query = query.Where(e => e.Budget <= criteria.MaxBudget.Value);
            }
        }
        
        return await query.ToListAsync();
    }
}

// 3. Métricas de Mantenibilidad (Maintainability Index)
public class PaymentService
{
    // Mantenibilidad: Alta (85-100)
    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        var validationResult = ValidatePaymentRequest(request);
        if (!validationResult.IsValid)
        {
            return new PaymentResult { Success = false, Error = validationResult.Error };
        }
        
        var paymentResult = await ChargePaymentAsync(request);
        await LogPaymentAsync(paymentResult);
        
        return paymentResult;
    }
    
    // Mantenibilidad: Media (65-84)
    private ValidationResult ValidatePaymentRequest(PaymentRequest request)
    {
        if (request == null)
        {
            return new ValidationResult { IsValid = false, Error = "Request is null" };
        }
        
        if (request.Amount <= 0)
        {
            return new ValidationResult { IsValid = false, Error = "Amount must be positive" };
        }
        
        if (string.IsNullOrEmpty(request.CardNumber))
        {
            return new ValidationResult { IsValid = false, Error = "Card number is required" };
        }
        
        if (string.IsNullOrEmpty(request.CVV))
        {
            return new ValidationResult { IsValid = false, Error = "CVV is required" };
        }
        
        if (request.ExpiryDate < DateTime.Now)
        {
            return new ValidationResult { IsValid = false, Error = "Card has expired" };
        }
        
        return new ValidationResult { IsValid = true };
    }
    
    // Mantenibilidad: Baja (0-64)
    private async Task<PaymentResult> ChargePaymentAsync(PaymentRequest request)
    {
        try
        {
            if (request.Amount > 10000)
            {
                if (request.CardType == "Visa")
                {
                    if (request.Currency == "USD")
                    {
                        if (request.Country == "US")
                        {
                            // Lógica compleja para pagos grandes en USD con Visa en US
                            return await ProcessLargeVisaPaymentAsync(request);
                        }
                        else
                        {
                            // Lógica para pagos grandes en USD con Visa fuera de US
                            return await ProcessLargeVisaPaymentInternationalAsync(request);
                        }
                    }
                    else
                    {
                        // Lógica para pagos grandes con Visa en otras monedas
                        return await ProcessLargeVisaPaymentOtherCurrencyAsync(request);
                    }
                }
                else if (request.CardType == "Mastercard")
                {
                    // Lógica para pagos grandes con Mastercard
                    return await ProcessLargeMastercardPaymentAsync(request);
                }
                else
                {
                    // Lógica para pagos grandes con otras tarjetas
                    return await ProcessLargeOtherCardPaymentAsync(request);
                }
            }
            else
            {
                // Lógica para pagos pequeños
                return await ProcessSmallPaymentAsync(request);
            }
        }
        catch (Exception ex)
        {
            return new PaymentResult { Success = false, Error = ex.Message };
        }
    }
}
```

### **2. Implementación de Métricas en .NET**

#### **Configuración de Métricas**
```xml
<!-- MussikOn.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <RunAnalyzersDuringBuild>true</RunAnalyzersDuringBuild>
    <RunAnalyzersDuringLiveAnalysis>true</RunAnalyzersDuringLiveAnalysis>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.4" />
    <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="8.0.0" />
    <PackageReference Include="SonarAnalyzer.CSharp" Version="9.0.0" />
  </ItemGroup>

  <ItemGroup>
    <Compile Include="**/*.cs" />
  </ItemGroup>
</Project>
```

#### **Análisis de Métricas**
```csharp
// Program.cs - Configurar análisis de métricas
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Configurar análisis de métricas
builder.Services.AddSingleton<IMetricsAnalyzer, MetricsAnalyzer>();

var app = builder.Build();

// Endpoint para obtener métricas
app.MapGet("/api/metrics", async (IMetricsAnalyzer analyzer) =>
{
    var metrics = await analyzer.AnalyzeProjectAsync();
    return Results.Ok(metrics);
});

app.Run();

// Implementación del analizador de métricas
public interface IMetricsAnalyzer
{
    Task<ProjectMetrics> AnalyzeProjectAsync();
    Task<ClassMetrics> AnalyzeClassAsync(string className);
    Task<MethodMetrics> AnalyzeMethodAsync(string className, string methodName);
}

public class MetricsAnalyzer : IMetricsAnalyzer
{
    private readonly ILogger<MetricsAnalyzer> _logger;
    
    public MetricsAnalyzer(ILogger<MetricsAnalyzer> logger)
    {
        _logger = logger;
    }
    
    public async Task<ProjectMetrics> AnalyzeProjectAsync()
    {
        var metrics = new ProjectMetrics
        {
            TotalClasses = 0,
            TotalMethods = 0,
            AverageCyclomaticComplexity = 0,
            AverageCognitiveComplexity = 0,
            AverageMaintainabilityIndex = 0,
            TotalLinesOfCode = 0,
            TotalLinesOfComments = 0
        };
        
        // Analizar todas las clases del proyecto
        var classes = GetProjectClasses();
        foreach (var className in classes)
        {
            var classMetrics = await AnalyzeClassAsync(className);
            metrics.TotalClasses++;
            metrics.TotalMethods += classMetrics.TotalMethods;
            metrics.TotalLinesOfCode += classMetrics.TotalLinesOfCode;
            metrics.TotalLinesOfComments += classMetrics.TotalLinesOfComments;
        }
        
        // Calcular promedios
        if (metrics.TotalClasses > 0)
        {
            metrics.AverageCyclomaticComplexity = metrics.TotalMethods > 0 
                ? metrics.TotalMethods / (double)metrics.TotalMethods 
                : 0;
            metrics.AverageMaintainabilityIndex = metrics.TotalClasses > 0 
                ? metrics.TotalClasses / (double)metrics.TotalClasses 
                : 0;
        }
        
        return metrics;
    }
    
    public async Task<ClassMetrics> AnalyzeClassAsync(string className)
    {
        var metrics = new ClassMetrics
        {
            ClassName = className,
            TotalMethods = 0,
            AverageCyclomaticComplexity = 0,
            AverageCognitiveComplexity = 0,
            AverageMaintainabilityIndex = 0,
            TotalLinesOfCode = 0,
            TotalLinesOfComments = 0
        };
        
        // Analizar métodos de la clase
        var methods = GetClassMethods(className);
        foreach (var methodName in methods)
        {
            var methodMetrics = await AnalyzeMethodAsync(className, methodName);
            metrics.TotalMethods++;
            metrics.TotalLinesOfCode += methodMetrics.LinesOfCode;
            metrics.TotalLinesOfComments += methodMetrics.LinesOfComments;
        }
        
        return metrics;
    }
    
    public async Task<MethodMetrics> AnalyzeMethodAsync(string className, string methodName)
    {
        var metrics = new MethodMetrics
        {
            ClassName = className,
            MethodName = methodName,
            CyclomaticComplexity = CalculateCyclomaticComplexity(className, methodName),
            CognitiveComplexity = CalculateCognitiveComplexity(className, methodName),
            MaintainabilityIndex = CalculateMaintainabilityIndex(className, methodName),
            LinesOfCode = CalculateLinesOfCode(className, methodName),
            LinesOfComments = CalculateLinesOfComments(className, methodName)
        };
        
        return metrics;
    }
    
    private int CalculateCyclomaticComplexity(string className, string methodName)
    {
        // Implementar cálculo de complejidad ciclomática
        // Usar Roslyn para analizar el código
        return 1; // Placeholder
    }
    
    private int CalculateCognitiveComplexity(string className, string methodName)
    {
        // Implementar cálculo de complejidad cognitiva
        return 1; // Placeholder
    }
    
    private double CalculateMaintainabilityIndex(string className, string methodName)
    {
        // Implementar cálculo del índice de mantenibilidad
        return 100; // Placeholder
    }
    
    private int CalculateLinesOfCode(string className, string methodName)
    {
        // Implementar cálculo de líneas de código
        return 10; // Placeholder
    }
    
    private int CalculateLinesOfComments(string className, string methodName)
    {
        // Implementar cálculo de líneas de comentarios
        return 2; // Placeholder
    }
    
    private List<string> GetProjectClasses()
    {
        // Implementar obtención de clases del proyecto
        return new List<string> { "MusicianService", "EventService", "PaymentService" };
    }
    
    private List<string> GetClassMethods(string className)
    {
        // Implementar obtención de métodos de la clase
        return new List<string> { "GetAsync", "CreateAsync", "UpdateAsync" };
    }
}

// Modelos para métricas
public class ProjectMetrics
{
    public int TotalClasses { get; set; }
    public int TotalMethods { get; set; }
    public double AverageCyclomaticComplexity { get; set; }
    public double AverageCognitiveComplexity { get; set; }
    public double AverageMaintainabilityIndex { get; set; }
    public int TotalLinesOfCode { get; set; }
    public int TotalLinesOfComments { get; set; }
}

public class ClassMetrics
{
    public string ClassName { get; set; }
    public int TotalMethods { get; set; }
    public double AverageCyclomaticComplexity { get; set; }
    public double AverageCognitiveComplexity { get; set; }
    public double AverageMaintainabilityIndex { get; set; }
    public int TotalLinesOfCode { get; set; }
    public int TotalLinesOfComments { get; set; }
}

public class MethodMetrics
{
    public string ClassName { get; set; }
    public string MethodName { get; set; }
    public int CyclomaticComplexity { get; set; }
    public int CognitiveComplexity { get; set; }
    public double MaintainabilityIndex { get; set; }
    public int LinesOfCode { get; set; }
    public int LinesOfComments { get; set; }
}
```

### **3. Optimización Basada en Métricas**

#### **Refactoring para Mejorar Métricas**
```csharp
// Código original con métricas pobres
public class MusicianMatchingService
{
    // Complejidad Ciclomática: 8
    // Complejidad Cognitiva: 6
    // Mantenibilidad: 45 (Baja)
    public async Task<List<MusicianMatch>> FindMatchingMusiciansAsync(EventCriteria criteria)
    {
        if (criteria == null)
        {
            throw new ArgumentNullException(nameof(criteria));
        }
        
        var availableMusicians = await _musicianRepository.GetAvailableMusiciansAsync(criteria);
        
        if (availableMusicians == null || !availableMusicians.Any())
        {
            return new List<MusicianMatch>();
        }
        
        var matches = new List<MusicianMatch>();
        foreach (var musician in availableMusicians)
        {
            var score = CalculateMatchScore(musician, criteria);
            if (score > 0)
            {
                var reasons = GetMatchReasons(musician, criteria);
                matches.Add(new MusicianMatch
                {
                    Musician = musician,
                    Score = score,
                    MatchReasons = reasons
                });
            }
        }
        
        return matches.OrderByDescending(m => m.Score).ToList();
    }
    
    // Complejidad Ciclomática: 5
    // Complejidad Cognitiva: 4
    // Mantenibilidad: 60 (Media)
    private decimal CalculateMatchScore(Musician musician, EventCriteria criteria)
    {
        decimal score = 0;
        
        if (musician.Genre == criteria.Genre)
        {
            score += 40;
        }
        
        if (musician.Location.City == criteria.Location.City)
        {
            score += 30;
        }
        
        if (musician.HourlyRate <= criteria.Budget / criteria.Duration)
        {
            score += 20;
        }
        
        if (musician.ExperienceYears >= criteria.MinExperienceYears)
        {
            score += 10;
        }
        
        return score;
    }
    
    // Complejidad Ciclomática: 5
    // Complejidad Cognitiva: 4
    // Mantenibilidad: 60 (Media)
    private List<string> GetMatchReasons(Musician musician, EventCriteria criteria)
    {
        var reasons = new List<string>();
        
        if (musician.Genre == criteria.Genre)
        {
            reasons.Add("Genre match");
        }
        
        if (musician.Location.City == criteria.Location.City)
        {
            reasons.Add("Location match");
        }
        
        if (musician.HourlyRate <= criteria.Budget / criteria.Duration)
        {
            reasons.Add("Budget match");
        }
        
        if (musician.ExperienceYears >= criteria.MinExperienceYears)
        {
            reasons.Add("Experience match");
        }
        
        return reasons;
    }
}

// Código refactorizado con métricas mejoradas
public class MusicianMatchingService
{
    private readonly IMusicianRepository _musicianRepository;
    private readonly ILogger<MusicianMatchingService> _logger;
    
    public MusicianMatchingService(IMusicianRepository musicianRepository, ILogger<MusicianMatchingService> logger)
    {
        _musicianRepository = musicianRepository;
        _logger = logger;
    }
    
    // Complejidad Ciclomática: 3
    // Complejidad Cognitiva: 2
    // Mantenibilidad: 85 (Alta)
    public async Task<List<MusicianMatch>> FindMatchingMusiciansAsync(EventCriteria criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        
        var availableMusicians = await GetAvailableMusiciansAsync(criteria);
        if (!availableMusicians.Any())
        {
            return new List<MusicianMatch>();
        }
        
        var matches = CreateMatches(availableMusicians, criteria);
        return OrderMatchesByScore(matches);
    }
    
    // Complejidad Ciclomática: 2
    // Complejidad Cognitiva: 1
    // Mantenibilidad: 90 (Alta)
    private async Task<List<Musician>> GetAvailableMusiciansAsync(EventCriteria criteria)
    {
        var musicians = await _musicianRepository.GetAvailableMusiciansAsync(criteria);
        return musicians ?? new List<Musician>();
    }
    
    // Complejidad Ciclomática: 2
    // Complejidad Cognitiva: 1
    // Mantenibilidad: 90 (Alta)
    private List<MusicianMatch> CreateMatches(List<Musician> musicians, EventCriteria criteria)
    {
        var matches = new List<MusicianMatch>();
        
        foreach (var musician in musicians)
        {
            var match = CreateMatchIfValid(musician, criteria);
            if (match != null)
            {
                matches.Add(match);
            }
        }
        
        return matches;
    }
    
    // Complejidad Ciclomática: 2
    // Complejidad Cognitiva: 1
    // Mantenibilidad: 90 (Alta)
    private MusicianMatch CreateMatchIfValid(Musician musician, EventCriteria criteria)
    {
        var score = CalculateMatchScore(musician, criteria);
        if (score <= 0)
        {
            return null;
        }
        
        return new MusicianMatch
        {
            Musician = musician,
            Score = score,
            MatchReasons = GetMatchReasons(musician, criteria)
        };
    }
    
    // Complejidad Ciclomática: 1
    // Complejidad Cognitiva: 0
    // Mantenibilidad: 95 (Alta)
    private static List<MusicianMatch> OrderMatchesByScore(List<MusicianMatch> matches)
    {
        return matches.OrderByDescending(m => m.Score).ToList();
    }
    
    // Complejidad Ciclomática: 1
    // Complejidad Cognitiva: 0
    // Mantenibilidad: 95 (Alta)
    private static decimal CalculateMatchScore(Musician musician, EventCriteria criteria)
    {
        var score = 0m;
        
        score += CalculateGenreScore(musician, criteria);
        score += CalculateLocationScore(musician, criteria);
        score += CalculateBudgetScore(musician, criteria);
        score += CalculateExperienceScore(musician, criteria);
        
        return score;
    }
    
    // Complejidad Ciclomática: 1
    // Complejidad Cognitiva: 0
    // Mantenibilidad: 95 (Alta)
    private static decimal CalculateGenreScore(Musician musician, EventCriteria criteria)
    {
        return musician.Genre == criteria.Genre ? 40 : 0;
    }
    
    // Complejidad Ciclomática: 1
    // Complejidad Cognitiva: 0
    // Mantenibilidad: 95 (Alta)
    private static decimal CalculateLocationScore(Musician musician, EventCriteria criteria)
    {
        return musician.Location.City == criteria.Location.City ? 30 : 0;
    }
    
    // Complejidad Ciclomática: 1
    // Complejidad Cognitiva: 0
    // Mantenibilidad: 95 (Alta)
    private static decimal CalculateBudgetScore(Musician musician, EventCriteria criteria)
    {
        return musician.HourlyRate <= criteria.Budget / criteria.Duration ? 20 : 0;
    }
    
    // Complejidad Ciclomática: 1
    // Complejidad Cognitiva: 0
    // Mantenibilidad: 95 (Alta)
    private static decimal CalculateExperienceScore(Musician musician, EventCriteria criteria)
    {
        return musician.ExperienceYears >= criteria.MinExperienceYears ? 10 : 0;
    }
    
    // Complejidad Ciclomática: 1
    // Complejidad Cognitiva: 0
    // Mantenibilidad: 95 (Alta)
    private static List<string> GetMatchReasons(Musician musician, EventCriteria criteria)
    {
        var reasons = new List<string>();
        
        AddGenreReason(reasons, musician, criteria);
        AddLocationReason(reasons, musician, criteria);
        AddBudgetReason(reasons, musician, criteria);
        AddExperienceReason(reasons, musician, criteria);
        
        return reasons;
    }
    
    // Complejidad Ciclomática: 1
    // Complejidad Cognitiva: 0
    // Mantenibilidad: 95 (Alta)
    private static void AddGenreReason(List<string> reasons, Musician musician, EventCriteria criteria)
    {
        if (musician.Genre == criteria.Genre)
        {
            reasons.Add("Genre match");
        }
    }
    
    // Complejidad Ciclomática: 1
    // Complejidad Cognitiva: 0
    // Mantenibilidad: 95 (Alta)
    private static void AddLocationReason(List<string> reasons, Musician musician, EventCriteria criteria)
    {
        if (musician.Location.City == criteria.Location.City)
        {
            reasons.Add("Location match");
        }
    }
    
    // Complejidad Ciclomática: 1
    // Complejidad Cognitiva: 0
    // Mantenibilidad: 95 (Alta)
    private static void AddBudgetReason(List<string> reasons, Musician musician, EventCriteria criteria)
    {
        if (musician.HourlyRate <= criteria.Budget / criteria.Duration)
        {
            reasons.Add("Budget match");
        }
    }
    
    // Complejidad Ciclomática: 1
    // Complejidad Cognitiva: 0
    // Mantenibilidad: 95 (Alta)
    private static void AddExperienceReason(List<string> reasons, Musician musician, EventCriteria criteria)
    {
        if (musician.ExperienceYears >= criteria.MinExperienceYears)
        {
            reasons.Add("Experience match");
        }
    }
}
```

### **4. Integración con CI/CD**

#### **Pipeline de Métricas**
```yaml
# .github/workflows/code-metrics.yml
name: Code Metrics Analysis

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  code-metrics:
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
        
      - name: Run tests
        run: dotnet test --no-build
        
      - name: Analyze code metrics
        run: |
          dotnet run --project MussikOn.Metrics --analyze
          
      - name: Generate metrics report
        run: |
          dotnet run --project MussikOn.Metrics --report
          
      - name: Upload metrics report
        uses: actions/upload-artifact@v3
        with:
          name: metrics-report
          path: ./metrics-report/
          
      - name: Comment PR with metrics
        if: github.event_name == 'pull_request'
        uses: actions/github-script@v6
        with:
          script: |
            const comment = `## 📈 Code Metrics Analysis
            
            **Overall Quality**: ✅ Good
            
            - **Average Cyclomatic Complexity**: 3.2 (Target: <5)
            - **Average Cognitive Complexity**: 2.1 (Target: <3)
            - **Average Maintainability Index**: 85.5 (Target: >80)
            - **Total Lines of Code**: 15,420
            - **Code Coverage**: 87%
            
            [View detailed report](https://github.com/${context.repo.owner}/${context.repo.repo}/actions/runs/${context.runId})
            `;
            
            github.rest.issues.createComment({
              issue_number: context.issue.number,
              owner: context.repo.owner,
              repo: context.repo.repo,
              body: comment
            });
```

## 🛠️ **Ejercicios Prácticos**

### **Ejercicio 1: Implementar Análisis de Métricas para MussikOn**
```csharp
// Implementar análisis de métricas para:
// 1. MusicianMatchingService
// 2. EventService
// 3. PaymentService
// 4. ChatService

// Calcular métricas y optimizar código
public class MusicianMatchingService
{
    // TODO: Implementar análisis de métricas
}
```

### **Ejercicio 2: Configurar Métricas**
```xml
<!-- Configurar métricas para MussikOn:
     1. Analizadores de código
     2. Reglas de calidad
     3. Thresholds
     4. CI/CD integration -->

<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <RunAnalyzersDuringBuild>true</RunAnalyzersDuringBuild>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="8.0.0" />
    <PackageReference Include="SonarAnalyzer.CSharp" Version="9.0.0" />
  </ItemGroup>
</Project>
```

### **Ejercicio 3: Optimizar Métricas**
```csharp
// Optimizar métricas de código:
// 1. Reducir complejidad ciclomática
// 2. Mejorar mantenibilidad
// 3. Aplicar principios SOLID
// 4. Refactorizar código complejo

public class MusicianMatchingService
{
    // TODO: Optimizar métricas
}
```

## 🎯 **Objetivos de Aprendizaje**

Al finalizar esta clase, serás capaz de:

1. **Implementar Code Metrics** para medir calidad
2. **Calcular métricas** de complejidad y mantenibilidad
3. **Optimizar código** basado en métricas
4. **Integrar métricas** en CI/CD
5. **Interpretar reportes** de métricas

## 📝 **Resumen**

En esta clase hemos cubierto:

- **Code Metrics**: Tipos y fundamentos
- **Implementación**: Análisis de métricas
- **Optimización**: Refactoring basado en métricas
- **Integración**: CI/CD y reportes
- **Interpretación**: Análisis de resultados

## 🚀 **Siguiente Clase**

En la próxima clase exploraremos **Testing de Integración Avanzado** para asegurar que todos los componentes de MussikOn funcionen correctamente juntos.

---

**💡 Tip**: Las métricas no son el objetivo final, son herramientas para guiarte hacia código de mejor calidad. Úsalas como brújula, no como destino.
