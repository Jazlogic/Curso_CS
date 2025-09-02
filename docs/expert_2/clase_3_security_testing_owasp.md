# 🔒 **Clase 3: Security Testing y OWASP**

## 🎯 **Objetivos de la Clase**
- Dominar Security Testing con OWASP Top 10
- Implementar Penetration Testing
- Aplicar Vulnerability Scanning
- Asegurar MussikOn contra amenazas

## 📚 **Contenido Teórico**

### **1. OWASP Top 10 - Testing de Vulnerabilidades**

#### **A01: Broken Access Control**
```csharp
[Test]
public async Task MusicianProfile_UnauthorizedAccess_ShouldBeDenied()
{
    // Arrange
    var client = _factory.CreateClient();
    var unauthorizedToken = "invalid_token";
    
    // Act
    var response = await client.GetAsync("/api/musicians/123", 
        options => options.Headers.Add("Authorization", $"Bearer {unauthorizedToken}"));
    
    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
}

[Test]
public async Task MusicianProfile_CrossUserAccess_ShouldBeDenied()
{
    // Arrange
    var client = _factory.CreateClient();
    var user1Token = await GetValidTokenAsync("user1@example.com");
    var user2Id = "user2_musician_id";
    
    // Act
    var response = await client.GetAsync($"/api/musicians/{user2Id}", 
        options => options.Headers.Add("Authorization", $"Bearer {user1Token}"));
    
    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
}

[Test]
public async Task MusicianProfile_DirectObjectReference_ShouldBeProtected()
{
    // Arrange
    var client = _factory.CreateClient();
    var token = await GetValidTokenAsync("user@example.com");
    
    // Act - Try to access another user's profile directly
    var response = await client.GetAsync("/api/musicians/999999", 
        options => options.Headers.Add("Authorization", $"Bearer {token}"));
    
    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
}
```

#### **A02: Cryptographic Failures**
```csharp
[Test]
public async Task UserPassword_ShouldBeHashedWithStrongAlgorithm()
{
    // Arrange
    var userService = new UserService(_passwordHasher, _userRepository);
    var plainPassword = "MySecurePassword123!";
    
    // Act
    var user = await userService.CreateUserAsync(new CreateUserRequest
    {
        Email = "test@example.com",
        Password = plainPassword
    });
    
    // Assert
    Assert.That(user.PasswordHash, Is.Not.EqualTo(plainPassword));
    Assert.That(user.PasswordHash, Does.StartWith("$2a$")); // bcrypt
    Assert.That(user.PasswordHash.Length, Is.GreaterThan(50));
}

[Test]
public async Task SensitiveData_ShouldBeEncryptedAtRest()
{
    // Arrange
    var userService = new UserService(_passwordHasher, _userRepository);
    var sensitiveData = new UserProfile
    {
        Email = "test@example.com",
        PhoneNumber = "+1234567890",
        BankAccount = "1234567890"
    };
    
    // Act
    var savedUser = await userService.SaveUserProfileAsync(sensitiveData);
    
    // Assert - Verify data is encrypted in database
    var rawData = await _database.GetRawUserDataAsync(savedUser.Id);
    Assert.That(rawData.PhoneNumber, Is.Not.EqualTo("+1234567890"));
    Assert.That(rawData.BankAccount, Is.Not.EqualTo("1234567890"));
}

[Test]
public async Task JWT_Token_ShouldBeProperlySigned()
{
    // Arrange
    var authService = new AuthService(_jwtService, _userRepository);
    var loginRequest = new LoginRequest
    {
        Email = "test@example.com",
        Password = "password123"
    };
    
    // Act
    var token = await authService.LoginAsync(loginRequest);
    
    // Assert
    Assert.That(token, Is.Not.Null);
    Assert.That(token, Does.Contain("."));
    
    var parts = token.Split('.');
    Assert.That(parts, Has.Length.EqualTo(3)); // Header.Payload.Signature
    
    // Verify signature
    var isValid = _jwtService.ValidateToken(token);
    Assert.That(isValid, Is.True);
}
```

#### **A03: Injection**
```csharp
[Test]
public async Task MusicianSearch_SQLInjection_ShouldBePrevented()
{
    // Arrange
    var client = _factory.CreateClient();
    var maliciousInput = "'; DROP TABLE Musicians; --";
    
    // Act
    var response = await client.GetAsync($"/api/musicians/search?genre={maliciousInput}");
    
    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    
    // Verify table still exists
    var tableExists = await _database.TableExistsAsync("Musicians");
    Assert.That(tableExists, Is.True);
}

[Test]
public async Task MusicianSearch_XSS_ShouldBePrevented()
{
    // Arrange
    var client = _factory.CreateClient();
    var xssPayload = "<script>alert('XSS')</script>";
    
    // Act
    var response = await client.PostAsync("/api/musicians/search", 
        new StringContent(JsonSerializer.Serialize(new { genre = xssPayload }), 
        Encoding.UTF8, "application/json"));
    
    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
}

[Test]
public async Task MusicianProfile_NoSQLInjection_ShouldBePrevented()
{
    // Arrange
    var client = _factory.CreateClient();
    var nosqlPayload = "{'$ne': null}";
    
    // Act
    var response = await client.GetAsync($"/api/musicians/search?id={nosqlPayload}");
    
    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
}
```

### **2. Penetration Testing con OWASP ZAP**

#### **Configuración de OWASP ZAP**
```csharp
public class OWASPZAPSecurityTests
{
    private OWASPZAPClient _zapClient;
    private string _targetUrl = "https://api.mussikon.com";
    
    [SetUp]
    public async Task Setup()
    {
        _zapClient = new OWASPZAPClient("http://localhost:8080");
        await _zapClient.StartSpiderScan(_targetUrl);
    }
    
    [Test]
    public async Task MussikOn_API_ShouldPassOWASPSecurityScan()
    {
        // Arrange
        var scanId = await _zapClient.StartActiveScan(_targetUrl);
        
        // Act
        await _zapClient.WaitForScanCompletion(scanId);
        var alerts = await _zapClient.GetAlerts();
        
        // Assert
        var highRiskAlerts = alerts.Where(a => a.Risk == "High").ToList();
        var mediumRiskAlerts = alerts.Where(a => a.Risk == "Medium").ToList();
        
        Assert.That(highRiskAlerts, Is.Empty, 
            $"High risk vulnerabilities found: {string.Join(", ", highRiskAlerts.Select(a => a.Name))}");
        
        Assert.That(mediumRiskAlerts.Count, Is.LessThan(5), 
            $"Too many medium risk vulnerabilities: {mediumRiskAlerts.Count}");
    }
    
    [Test]
    public async Task MussikOn_API_ShouldHaveProperSecurityHeaders()
    {
        // Arrange
        var client = new HttpClient();
        
        // Act
        var response = await client.GetAsync($"{_targetUrl}/api/health");
        var headers = response.Headers;
        
        // Assert
        Assert.That(headers.Contains("X-Content-Type-Options"), Is.True);
        Assert.That(headers.Contains("X-Frame-Options"), Is.True);
        Assert.That(headers.Contains("X-XSS-Protection"), Is.True);
        Assert.That(headers.Contains("Strict-Transport-Security"), Is.True);
        Assert.That(headers.Contains("Content-Security-Policy"), Is.True);
    }
}
```

#### **Testing de Autenticación y Autorización**
```csharp
[Test]
public async Task Authentication_ShouldBeRequiredForProtectedEndpoints()
{
    // Arrange
    var client = _factory.CreateClient();
    var protectedEndpoints = new[]
    {
        "/api/musicians",
        "/api/events",
        "/api/chat/messages",
        "/api/payments"
    };
    
    foreach (var endpoint in protectedEndpoints)
    {
        // Act
        var response = await client.GetAsync(endpoint);
        
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized), 
            $"Endpoint {endpoint} should require authentication");
    }
}

[Test]
public async Task Authorization_ShouldEnforceRoleBasedAccess()
{
    // Arrange
    var client = _factory.CreateClient();
    var musicianToken = await GetTokenForRoleAsync("Musician");
    var organizerToken = await GetTokenForRoleAsync("Organizer");
    
    // Act - Musician trying to access organizer-only endpoint
    var response = await client.GetAsync("/api/organizers/dashboard", 
        options => options.Headers.Add("Authorization", $"Bearer {musicianToken}"));
    
    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    
    // Act - Organizer accessing organizer endpoint
    response = await client.GetAsync("/api/organizers/dashboard", 
        options => options.Headers.Add("Authorization", $"Bearer {organizerToken}"));
    
    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
}
```

### **3. Vulnerability Scanning**

#### **Testing de Dependencias**
```csharp
[Test]
public async Task Dependencies_ShouldNotHaveKnownVulnerabilities()
{
    // Arrange
    var vulnerabilityScanner = new VulnerabilityScanner();
    
    // Act
    var vulnerabilities = await vulnerabilityScanner.ScanDependenciesAsync();
    
    // Assert
    var highRiskVulns = vulnerabilities.Where(v => v.Severity == "High").ToList();
    var criticalVulns = vulnerabilities.Where(v => v.Severity == "Critical").ToList();
    
    Assert.That(criticalVulns, Is.Empty, 
        $"Critical vulnerabilities found: {string.Join(", ", criticalVulns.Select(v => v.Name))}");
    
    Assert.That(highRiskVulns, Is.Empty, 
        $"High risk vulnerabilities found: {string.Join(", ", highRiskVulns.Select(v => v.Name))}");
}

[Test]
public async Task Docker_Images_ShouldBeScannedForVulnerabilities()
{
    // Arrange
    var dockerScanner = new DockerVulnerabilityScanner();
    
    // Act
    var vulnerabilities = await dockerScanner.ScanImageAsync("mussikon/api:latest");
    
    // Assert
    var highRiskVulns = vulnerabilities.Where(v => v.Severity == "High").ToList();
    
    Assert.That(highRiskVulns, Is.Empty, 
        $"High risk vulnerabilities in Docker image: {string.Join(", ", highRiskVulns.Select(v => v.Name))}");
}
```

#### **Testing de Configuración**
```csharp
[Test]
public async Task Configuration_ShouldBeSecure()
{
    // Arrange
    var configScanner = new ConfigurationScanner();
    
    // Act
    var securityIssues = await configScanner.ScanConfigurationAsync();
    
    // Assert
    var criticalIssues = securityIssues.Where(i => i.Severity == "Critical").ToList();
    var highIssues = securityIssues.Where(i => i.Severity == "High").ToList();
    
    Assert.That(criticalIssues, Is.Empty, 
        $"Critical configuration issues: {string.Join(", ", criticalIssues.Select(i => i.Description))}");
    
    Assert.That(highIssues, Is.Empty, 
        $"High risk configuration issues: {string.Join(", ", highIssues.Select(i => i.Description))}");
}

[Test]
public async Task Secrets_ShouldNotBeExposed()
{
    // Arrange
    var secretsScanner = new SecretsScanner();
    
    // Act
    var exposedSecrets = await secretsScanner.ScanForSecretsAsync();
    
    // Assert
    Assert.That(exposedSecrets, Is.Empty, 
        $"Exposed secrets found: {string.Join(", ", exposedSecrets.Select(s => s.Type))}");
}
```

### **4. Security Headers y CORS**

#### **Testing de Security Headers**
```csharp
[Test]
public async Task API_ShouldHaveProperSecurityHeaders()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/api/health");
    var headers = response.Headers;
    
    // Assert
    Assert.That(headers.Contains("X-Content-Type-Options"), Is.True);
    Assert.That(headers.GetValues("X-Content-Type-Options").First(), Is.EqualTo("nosniff"));
    
    Assert.That(headers.Contains("X-Frame-Options"), Is.True);
    Assert.That(headers.GetValues("X-Frame-Options").First(), Is.EqualTo("DENY"));
    
    Assert.That(headers.Contains("X-XSS-Protection"), Is.True);
    Assert.That(headers.GetValues("X-XSS-Protection").First(), Is.EqualTo("1; mode=block"));
    
    Assert.That(headers.Contains("Strict-Transport-Security"), Is.True);
    Assert.That(headers.GetValues("Strict-Transport-Security").First(), Does.Contain("max-age"));
    
    Assert.That(headers.Contains("Content-Security-Policy"), Is.True);
}

[Test]
public async Task CORS_ShouldBeProperlyConfigured()
{
    // Arrange
    var client = _factory.CreateClient();
    var origin = "https://mussikon.com";
    
    // Act
    var request = new HttpRequestMessage(HttpMethod.Options, "/api/musicians");
    request.Headers.Add("Origin", origin);
    request.Headers.Add("Access-Control-Request-Method", "GET");
    request.Headers.Add("Access-Control-Request-Headers", "Authorization");
    
    var response = await client.SendAsync(request);
    var headers = response.Headers;
    
    // Assert
    Assert.That(headers.Contains("Access-Control-Allow-Origin"), Is.True);
    Assert.That(headers.Contains("Access-Control-Allow-Methods"), Is.True);
    Assert.That(headers.Contains("Access-Control-Allow-Headers"), Is.True);
    Assert.That(headers.Contains("Access-Control-Max-Age"), Is.True);
}
```

## 🛠️ **Ejercicios Prácticos**

### **Ejercicio 1: Implementar Security Tests para MussikOn**
```csharp
// Implementar tests de seguridad para:
// 1. Autenticación JWT
// 2. Autorización basada en roles
// 3. Protección contra SQL injection
// 4. Validación de entrada
// 5. Headers de seguridad

[Test]
public async Task MussikOn_API_ShouldBeSecureAgainstOWASPTop10()
{
    // TODO: Implementar tests completos de seguridad
}

[Test]
public async Task MusicianProfile_ShouldBeProtectedAgainstUnauthorizedAccess()
{
    // TODO: Implementar tests de autorización
}
```

### **Ejercicio 2: Configurar OWASP ZAP para MussikOn**
```yaml
# zap-config.yml
target: "https://api.mussikon.com"
context:
  name: "MussikOn API"
  urls:
    - "https://api.mussikon.com/*"
  authentication:
    method: "form"
    loginUrl: "https://api.mussikon.com/auth/login"
    loginRequestData: "email={email}&password={password}"
  sessionManagement:
    method: "cookieBasedSessionManagement"
  authorization:
    method: "httpAuthentication"
    header: "Authorization"
    headerValue: "Bearer {token}"

scanner:
  activeScan: true
  spider: true
  ajaxSpider: true
  policy: "API-Minimal"
  maxScanDurationInMins: 60

reports:
  - type: "html"
    output: "mussikon-security-report.html"
  - type: "json"
    output: "mussikon-security-report.json"
```

### **Ejercicio 3: Implementar Security Monitoring**
```csharp
[Test]
public async Task Security_ShouldBeMonitoredInRealTime()
{
    // TODO: Implementar monitoring de seguridad
    // - Detección de ataques
    // - Alertas de seguridad
    // - Logging de eventos de seguridad
    // - Rate limiting
}
```

## 🎯 **Objetivos de Aprendizaje**

Al finalizar esta clase, serás capaz de:

1. **Implementar Security Testing** contra OWASP Top 10
2. **Configurar Penetration Testing** con OWASP ZAP
3. **Aplicar Vulnerability Scanning** automatizado
4. **Implementar Security Headers** y CORS
5. **Monitorear seguridad** en tiempo real

## 📝 **Resumen**

En esta clase hemos cubierto:

- **OWASP Top 10**: Testing de vulnerabilidades comunes
- **Penetration Testing**: OWASP ZAP, testing de autenticación
- **Vulnerability Scanning**: Dependencias, Docker, configuración
- **Security Headers**: CORS, CSP, HSTS
- **Security Monitoring**: Detección de amenazas

## 🚀 **Siguiente Clase**

En la próxima clase exploraremos **Contract Testing** con Pact para asegurar la compatibilidad entre servicios en la arquitectura de MussikOn.

---

**💡 Tip**: La seguridad no es un feature, es un requisito fundamental. Siempre implementa security testing desde el primer día.
