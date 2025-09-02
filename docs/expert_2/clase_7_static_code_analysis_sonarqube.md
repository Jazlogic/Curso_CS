# 🔍 **Clase 7: Static Code Analysis con SonarQube**

## 🎯 **Objetivos de la Clase**
- Dominar Static Code Analysis con SonarQube
- Implementar análisis de calidad de código
- Aplicar reglas de calidad en MussikOn
- Integrar análisis estático en CI/CD

## 📚 **Contenido Teórico**

### **1. Fundamentos de Static Code Analysis**

#### **¿Qué es Static Code Analysis?**
```csharp
// Static Code Analysis analiza el código sin ejecutarlo
// Detecta problemas de calidad, seguridad y mantenibilidad

// Ejemplo de código con problemas detectados por SonarQube
public class MusicianService
{
    private readonly IMusicianRepository _musicianRepository;
    private readonly ILogger<MusicianService> _logger;
    
    public MusicianService(IMusicianRepository musicianRepository, ILogger<MusicianService> logger)
    {
        _musicianRepository = musicianRepository;
        _logger = logger;
    }
    
    // Problema: Método muy largo (más de 20 líneas)
    public async Task<Musician> CreateMusicianAsync(CreateMusicianRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }
        
        if (string.IsNullOrEmpty(request.Name))
        {
            throw new ArgumentException("Name is required");
        }
        
        if (string.IsNullOrEmpty(request.Email))
        {
            throw new ArgumentException("Email is required");
        }
        
        if (request.Age < 18)
        {
            throw new ArgumentException("Age must be at least 18");
        }
        
        if (string.IsNullOrEmpty(request.Genre))
        {
            throw new ArgumentException("Genre is required");
        }
        
        if (request.HourlyRate <= 0)
        {
            throw new ArgumentException("Hourly rate must be positive");
        }
        
        var musician = new Musician
        {
            Name = request.Name,
            Email = request.Email,
            Age = request.Age,
            Genre = request.Genre,
            HourlyRate = request.HourlyRate,
            CreatedAt = DateTime.UtcNow
        };
        
        var createdMusician = await _musicianRepository.CreateAsync(musician);
        
        _logger.LogInformation("Musician created with ID: {Id}", createdMusician.Id);
        
        return createdMusician;
    }
    
    // Problema: Método con alta complejidad ciclomática
    public async Task<List<Musician>> SearchMusiciansAsync(SearchCriteria criteria)
    {
        var query = _musicianRepository.GetQueryable();
        
        if (!string.IsNullOrEmpty(criteria.Name))
        {
            query = query.Where(m => m.Name.Contains(criteria.Name));
        }
        
        if (!string.IsNullOrEmpty(criteria.Genre))
        {
            query = query.Where(m => m.Genre == criteria.Genre);
        }
        
        if (criteria.MinAge.HasValue)
        {
            query = query.Where(m => m.Age >= criteria.MinAge.Value);
        }
        
        if (criteria.MaxAge.HasValue)
        {
            query = query.Where(m => m.Age <= criteria.MaxAge.Value);
        }
        
        if (criteria.MinRate.HasValue)
        {
            query = query.Where(m => m.HourlyRate >= criteria.MinRate.Value);
        }
        
        if (criteria.MaxRate.HasValue)
        {
            query = query.Where(m => m.HourlyRate <= criteria.MaxRate.Value);
        }
        
        if (criteria.Location != null)
        {
            query = query.Where(m => m.Location.City == criteria.Location.City);
        }
        
        if (criteria.IsAvailable.HasValue)
        {
            query = query.Where(m => m.IsAvailable == criteria.IsAvailable.Value);
        }
        
        return await query.ToListAsync();
    }
    
    // Problema: Método con duplicación de código
    public async Task<Musician> UpdateMusicianAsync(int id, UpdateMusicianRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }
        
        if (string.IsNullOrEmpty(request.Name))
        {
            throw new ArgumentException("Name is required");
        }
        
        if (string.IsNullOrEmpty(request.Email))
        {
            throw new ArgumentException("Email is required");
        }
        
        if (request.Age < 18)
        {
            throw new ArgumentException("Age must be at least 18");
        }
        
        if (string.IsNullOrEmpty(request.Genre))
        {
            throw new ArgumentException("Genre is required");
        }
        
        if (request.HourlyRate <= 0)
        {
            throw new ArgumentException("Hourly rate must be positive");
        }
        
        var musician = await _musicianRepository.GetByIdAsync(id);
        if (musician == null)
        {
            throw new NotFoundException("Musician not found");
        }
        
        musician.Name = request.Name;
        musician.Email = request.Email;
        musician.Age = request.Age;
        musician.Genre = request.Genre;
        musician.HourlyRate = request.HourlyRate;
        musician.UpdatedAt = DateTime.UtcNow;
        
        var updatedMusician = await _musicianRepository.UpdateAsync(musician);
        
        _logger.LogInformation("Musician updated with ID: {Id}", updatedMusician.Id);
        
        return updatedMusician;
    }
}
```

#### **Código Refactorizado**
```csharp
// Código refactorizado siguiendo las reglas de SonarQube
public class MusicianService
{
    private readonly IMusicianRepository _musicianRepository;
    private readonly ILogger<MusicianService> _logger;
    private readonly IMusicianValidator _validator;
    
    public MusicianService(
        IMusicianRepository musicianRepository, 
        ILogger<MusicianService> logger,
        IMusicianValidator validator)
    {
        _musicianRepository = musicianRepository;
        _logger = logger;
        _validator = validator;
    }
    
    // Método refactorizado: más corto y enfocado
    public async Task<Musician> CreateMusicianAsync(CreateMusicianRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        _validator.ValidateCreateRequest(request);
        
        var musician = MapToMusician(request);
        var createdMusician = await _musicianRepository.CreateAsync(musician);
        
        _logger.LogInformation("Musician created with ID: {Id}", createdMusician.Id);
        return createdMusician;
    }
    
    // Método refactorizado: complejidad reducida
    public async Task<List<Musician>> SearchMusiciansAsync(SearchCriteria criteria)
    {
        var query = _musicianRepository.GetQueryable();
        query = ApplySearchFilters(query, criteria);
        
        return await query.ToListAsync();
    }
    
    // Método refactorizado: sin duplicación
    public async Task<Musician> UpdateMusicianAsync(int id, UpdateMusicianRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        _validator.ValidateUpdateRequest(request);
        
        var musician = await GetMusicianByIdAsync(id);
        UpdateMusicianProperties(musician, request);
        
        var updatedMusician = await _musicianRepository.UpdateAsync(musician);
        
        _logger.LogInformation("Musician updated with ID: {Id}", updatedMusician.Id);
        return updatedMusician;
    }
    
    private static Musician MapToMusician(CreateMusicianRequest request)
    {
        return new Musician
        {
            Name = request.Name,
            Email = request.Email,
            Age = request.Age,
            Genre = request.Genre,
            HourlyRate = request.HourlyRate,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    private static IQueryable<Musician> ApplySearchFilters(IQueryable<Musician> query, SearchCriteria criteria)
    {
        if (!string.IsNullOrEmpty(criteria.Name))
            query = query.Where(m => m.Name.Contains(criteria.Name));
            
        if (!string.IsNullOrEmpty(criteria.Genre))
            query = query.Where(m => m.Genre == criteria.Genre);
            
        if (criteria.MinAge.HasValue)
            query = query.Where(m => m.Age >= criteria.MinAge.Value);
            
        if (criteria.MaxAge.HasValue)
            query = query.Where(m => m.Age <= criteria.MaxAge.Value);
            
        if (criteria.MinRate.HasValue)
            query = query.Where(m => m.HourlyRate >= criteria.MinRate.Value);
            
        if (criteria.MaxRate.HasValue)
            query = query.Where(m => m.HourlyRate <= criteria.MaxRate.Value);
            
        if (criteria.Location != null)
            query = query.Where(m => m.Location.City == criteria.Location.City);
            
        if (criteria.IsAvailable.HasValue)
            query = query.Where(m => m.IsAvailable == criteria.IsAvailable.Value);
            
        return query;
    }
    
    private async Task<Musician> GetMusicianByIdAsync(int id)
    {
        var musician = await _musicianRepository.GetByIdAsync(id);
        return musician ?? throw new NotFoundException("Musician not found");
    }
    
    private static void UpdateMusicianProperties(Musician musician, UpdateMusicianRequest request)
    {
        musician.Name = request.Name;
        musician.Email = request.Email;
        musician.Age = request.Age;
        musician.Genre = request.Genre;
        musician.HourlyRate = request.HourlyRate;
        musician.UpdatedAt = DateTime.UtcNow;
    }
}
```

### **2. Configuración de SonarQube**

#### **Docker Compose para SonarQube**
```yaml
# docker-compose.sonarqube.yml
version: '3.8'

services:
  sonarqube:
    image: sonarqube:9.9-community
    container_name: sonarqube
    ports:
      - "9000:9000"
    environment:
      - SONAR_ES_BOOTSTRAP_CHECKS_DISABLE=true
      - SONAR_JDBC_URL=jdbc:postgresql://postgres:5432/sonar
      - SONAR_JDBC_USERNAME=sonar
      - SONAR_JDBC_PASSWORD=sonar
    volumes:
      - sonarqube_data:/opt/sonarqube/data
      - sonarqube_logs:/opt/sonarqube/logs
      - sonarqube_extensions:/opt/sonarqube/extensions
    depends_on:
      - postgres
    networks:
      - sonar

  postgres:
    image: postgres:13
    container_name: sonarqube-postgres
    environment:
      - POSTGRES_DB=sonar
      - POSTGRES_USER=sonar
      - POSTGRES_PASSWORD=sonar
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - sonar

volumes:
  sonarqube_data:
  sonarqube_logs:
  sonarqube_extensions:
  postgres_data:

networks:
  sonar:
    driver: bridge
```

#### **Configuración de SonarQube**
```properties
# sonar-project.properties
sonar.projectKey=mussikon
sonar.projectName=MussikOn
sonar.projectVersion=1.0.0
sonar.organization=mussikon

# Source code
sonar.sources=src
sonar.tests=tests

# Exclusions
sonar.exclusions=**/bin/**,**/obj/**,**/Migrations/**,**/*.Designer.cs,**/*.Generated.cs
sonar.test.exclusions=**/bin/**,**/obj/**

# Coverage
sonar.cs.opencover.reportsPaths=coverage/coverage.opencover.xml
sonar.cs.vstest.reportsPaths=coverage/test-results.trx

# Quality Gate
sonar.qualitygate.wait=true

# Additional settings
sonar.verbose=true
sonar.log.level=INFO
```

#### **Configuración de SonarScanner**
```bash
# Instalar SonarScanner
dotnet tool install -g dotnet-sonarscanner

# Ejecutar análisis
dotnet sonarscanner begin /k:"mussikon" /d:sonar.host.url="http://localhost:9000" /d:sonar.login="admin" /d:sonar.password="admin"

# Build y test
dotnet build
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Finalizar análisis
dotnet sonarscanner end /d:sonar.login="admin" /d:sonar.password="admin"
```

### **3. Reglas de Calidad de SonarQube**

#### **Reglas de C#**
```csharp
// 1. Regla: S1186 - Methods should not be empty
public class MusicianService
{
    // ❌ Violación: Método vacío
    public void DoNothing()
    {
        // Método vacío
    }
    
    // ✅ Correcto: Método con implementación
    public void DoSomething()
    {
        _logger.LogInformation("Doing something");
    }
}

// 2. Regla: S1075 - URIs should not be hardcoded
public class ApiClient
{
    // ❌ Violación: URL hardcodeada
    private const string BaseUrl = "https://api.mussikon.com";
    
    // ✅ Correcto: URL desde configuración
    private readonly string _baseUrl;
    
    public ApiClient(IConfiguration configuration)
    {
        _baseUrl = configuration["ApiBaseUrl"];
    }
}

// 3. Regla: S1144 - Unused private types or members should be removed
public class MusicianService
{
    private readonly IMusicianRepository _musicianRepository;
    
    // ❌ Violación: Método privado no usado
    private void UnusedMethod()
    {
        // Método no usado
    }
    
    // ✅ Correcto: Método usado
    public async Task<Musician> GetMusicianAsync(int id)
    {
        return await _musicianRepository.GetByIdAsync(id);
    }
}

// 4. Regla: S3776 - Cognitive Complexity should not be too high
public class MusicianMatchingService
{
    // ❌ Violación: Alta complejidad cognitiva
    public decimal CalculateMatchScore(Musician musician, EventCriteria criteria)
    {
        decimal score = 0;
        
        if (musician.Genre == criteria.Genre)
        {
            score += 40;
            if (musician.Location.City == criteria.Location.City)
            {
                score += 30;
                if (musician.HourlyRate <= criteria.Budget / criteria.Duration)
                {
                    score += 20;
                    if (musician.ExperienceYears >= criteria.MinExperienceYears)
                    {
                        score += 10;
                    }
                }
            }
        }
        
        return score;
    }
    
    // ✅ Correcto: Complejidad reducida
    public decimal CalculateMatchScore(Musician musician, EventCriteria criteria)
    {
        var score = 0m;
        
        score += CalculateGenreScore(musician, criteria);
        score += CalculateLocationScore(musician, criteria);
        score += CalculateBudgetScore(musician, criteria);
        score += CalculateExperienceScore(musician, criteria);
        
        return score;
    }
    
    private static decimal CalculateGenreScore(Musician musician, EventCriteria criteria)
    {
        return musician.Genre == criteria.Genre ? 40 : 0;
    }
    
    private static decimal CalculateLocationScore(Musician musician, EventCriteria criteria)
    {
        return musician.Location.City == criteria.Location.City ? 30 : 0;
    }
    
    private static decimal CalculateBudgetScore(Musician musician, EventCriteria criteria)
    {
        return musician.HourlyRate <= criteria.Budget / criteria.Duration ? 20 : 0;
    }
    
    private static decimal CalculateExperienceScore(Musician musician, EventCriteria criteria)
    {
        return musician.ExperienceYears >= criteria.MinExperienceYears ? 10 : 0;
    }
}
```

#### **Reglas de Seguridad**
```csharp
// 1. Regla: S2078 - HTTP referers should not be relied on
public class SecurityController
{
    // ❌ Violación: Confiar en HTTP referer
    public IActionResult ValidateRequest()
    {
        var referer = Request.Headers["Referer"];
        if (referer != "https://mussikon.com")
        {
            return Unauthorized();
        }
        return Ok();
    }
    
    // ✅ Correcto: Usar tokens CSRF
    public IActionResult ValidateRequest()
    {
        var token = Request.Headers["X-CSRF-Token"];
        if (!_csrfService.ValidateToken(token))
        {
            return Unauthorized();
        }
        return Ok();
    }
}

// 2. Regla: S4790 - Hashing data is security-sensitive
public class PasswordService
{
    // ❌ Violación: Hash inseguro
    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
    
    // ✅ Correcto: Hash seguro con salt
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}

// 3. Regla: S5542 - Encryption algorithms should be used with secure mode and padding scheme
public class EncryptionService
{
    // ❌ Violación: Cifrado inseguro
    public string Encrypt(string plaintext)
    {
        using var aes = Aes.Create();
        aes.Mode = CipherMode.ECB; // Inseguro
        aes.Padding = PaddingMode.None; // Inseguro
        
        // ... implementación
        return encryptedText;
    }
    
    // ✅ Correcto: Cifrado seguro
    public string Encrypt(string plaintext)
    {
        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC; // Seguro
        aes.Padding = PaddingMode.PKCS7; // Seguro
        
        // ... implementación
        return encryptedText;
    }
}
```

### **4. Integración con CI/CD**

#### **Pipeline de SonarQube**
```yaml
# .github/workflows/sonarqube.yml
name: SonarQube Analysis

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  sonarqube:
    runs-on: ubuntu-latest
    
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
        run: dotnet test --no-build --collect:"XPlat Code Coverage" --results-directory ./coverage
        
      - name: Install SonarScanner
        run: dotnet tool install -g dotnet-sonarscanner
        
      - name: Run SonarQube analysis
        run: |
          dotnet sonarscanner begin /k:"mussikon" /d:sonar.host.url="${{ secrets.SONAR_HOST_URL }}" /d:sonar.login="${{ secrets.SONAR_TOKEN }}" /d:sonar.cs.opencover.reportsPaths="./coverage/**/coverage.opencover.xml"
          dotnet build
          dotnet sonarscanner end /d:sonar.login="${{ secrets.SONAR_TOKEN }}"
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
          
      - name: Comment PR with SonarQube results
        if: github.event_name == 'pull_request'
        uses: actions/github-script@v6
        with:
          script: |
            const comment = `## 🔍 SonarQube Analysis Results
            
            **Quality Gate**: ✅ Passed
            
            - **Bugs**: 0
            - **Vulnerabilities**: 0
            - **Code Smells**: 0
            - **Coverage**: 85%
            - **Duplications**: 0%
            
            [View detailed report](https://sonarcloud.io/dashboard?id=mussikon)
            `;
            
            github.rest.issues.createComment({
              issue_number: context.issue.number,
              owner: context.repo.owner,
              repo: context.repo.repo,
              body: comment
            });
```

## 🛠️ **Ejercicios Prácticos**

### **Ejercicio 1: Implementar Análisis de SonarQube para MussikOn**
```csharp
// Implementar análisis de SonarQube para:
// 1. MusicianService
// 2. EventService
// 3. PaymentService
// 4. ChatService

// Refactorizar código para cumplir reglas de SonarQube
public class MusicianService
{
    // TODO: Refactorizar para cumplir reglas de calidad
}
```

### **Ejercicio 2: Configurar SonarQube**
```yaml
# Configurar SonarQube para MussikOn:
# 1. Docker Compose
# 2. sonar-project.properties
# 3. Quality Gate
# 4. CI/CD integration

version: '3.8'
services:
  sonarqube:
    image: sonarqube:9.9-community
    # TODO: Configurar SonarQube
```

### **Ejercicio 3: Implementar Reglas de Calidad**
```csharp
// Implementar reglas de calidad:
// 1. Reducir complejidad ciclomática
// 2. Eliminar duplicación de código
// 3. Mejorar legibilidad
// 4. Aplicar principios SOLID

public class MusicianMatchingService
{
    // TODO: Implementar reglas de calidad
}
```

## 🎯 **Objetivos de Aprendizaje**

Al finalizar esta clase, serás capaz de:

1. **Implementar Static Code Analysis** con SonarQube
2. **Configurar reglas de calidad** para C#
3. **Refactorizar código** para cumplir estándares
4. **Integrar SonarQube** en CI/CD
5. **Interpretar reportes** de calidad

## 📝 **Resumen**

En esta clase hemos cubierto:

- **Static Code Analysis**: Fundamentos y beneficios
- **SonarQube**: Configuración y reglas
- **Reglas de Calidad**: C# y seguridad
- **Refactoring**: Mejora de código
- **CI/CD Integration**: Pipeline automatizado

## 🚀 **Siguiente Clase**

En la próxima clase exploraremos **Code Metrics** para medir y mejorar la calidad del código en MussikOn.

---

**💡 Tip**: SonarQube no es solo una herramienta de análisis, es una guía para escribir código de mejor calidad. Úsala como mentor virtual.
