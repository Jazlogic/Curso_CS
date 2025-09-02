# 🚀 Clase 3: Seguridad en Aplicaciones .NET

## 🧭 Navegación
- **⬅️ Anterior**: [Clase 2: Optimización de Código y Memoria](clase_2_optimizacion_codigo.md)
- **🏠 [Volver al Módulo 6](../README.md)**
- **➡️ Siguiente**: [Clase 4: Seguridad de APIs y Microservicios](clase_4_seguridad_apis.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)**

---

## 🎯 Objetivos de la Clase

Al finalizar esta clase, serás capaz de:
- Implementar autenticación y autorización avanzada
- Proteger aplicaciones contra ataques comunes
- Implementar cifrado y hashing seguro
- Validar y sanitizar entradas de usuario

---

## 📚 Contenido Teórico

### 3.1 Autenticación y Autorización Avanzada

La seguridad comienza con una autenticación robusta y un sistema de autorización bien diseñado.

#### Sistema de Autenticación Personalizado

```csharp
public class CustomAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IUserService _userService;
    private readonly IJwtTokenService _jwtTokenService;
    
    public CustomAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IUserService userService,
        IJwtTokenService jwtTokenService)
        : base(options, logger, encoder, clock)
    {
        _userService = userService;
        _jwtTokenService = jwtTokenService;
    }
    
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Obtiene el token del header Authorization
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return AuthenticateResult.Fail("Authorization header not found.");
        }
        
        var authHeader = Request.Headers["Authorization"].ToString();
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.Fail("Bearer token not found.");
        }
        
        var token = authHeader.Substring("Bearer ".Length).Trim();
        
        try
        {
            // Valida el token JWT
            var principal = _jwtTokenService.ValidateToken(token);
            
            // Crea el ticket de autenticación
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            return AuthenticateResult.Fail($"Token validation failed: {ex.Message}");
        }
    }
    
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers["WWW-Authenticate"] = "Bearer";
        Response.StatusCode = 401;
        return Task.CompletedTask;
    }
}
```

#### Servicio de Usuarios con Hashing Seguro

```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UserService> _logger;
    
    public UserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }
    
    public async Task<AuthenticationResult> AuthenticateAsync(string username, string password)
    {
        try
        {
            // Busca el usuario por username
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
            {
                _logger.LogWarning("Authentication failed for username: {Username}", username);
                return AuthenticationResult.Failure("Invalid credentials");
            }
            
            // Verifica la contraseña
            if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
            {
                _logger.LogWarning("Authentication failed for username: {Username}", username);
                return AuthenticationResult.Failure("Invalid credentials");
            }
            
            // Verifica si la cuenta está bloqueada
            if (user.IsLocked)
            {
                _logger.LogWarning("Authentication blocked for locked account: {Username}", username);
                return AuthenticationResult.Failure("Account is locked");
            }
            
            // Verifica si la contraseña ha expirado
            if (user.PasswordExpiresAt.HasValue && user.PasswordExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Authentication failed for expired password: {Username}", username);
                return AuthenticationResult.Failure("Password has expired");
            }
            
            // Actualiza último login
            user.LastLoginAt = DateTime.UtcNow;
            user.FailedLoginAttempts = 0;
            await _userRepository.UpdateAsync(user);
            
            _logger.LogInformation("User authenticated successfully: {Username}", username);
            return AuthenticationResult.Success(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for username: {Username}", username);
            return AuthenticationResult.Failure("Authentication error occurred");
        }
    }
    
    public async Task<RegistrationResult> RegisterAsync(UserRegistrationRequest request)
    {
        try
        {
            // Valida la solicitud
            var validationResult = ValidateRegistrationRequest(request);
            if (!validationResult.IsValid)
            {
                return RegistrationResult.Failure(validationResult.Errors);
            }
            
            // Verifica si el username ya existe
            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                return RegistrationResult.Failure("Username already exists");
            }
            
            // Verifica si el email ya existe
            var existingEmail = await _userRepository.GetByEmailAsync(request.Email);
            if (existingEmail != null)
            {
                return RegistrationResult.Failure("Email already exists");
            }
            
            // Crea el nuevo usuario
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                FailedLoginAttempts = 0
            };
            
            await _userRepository.CreateAsync(user);
            
            _logger.LogInformation("User registered successfully: {Username}", user.Username);
            return RegistrationResult.Success(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for username: {Username}", request.Username);
            return RegistrationResult.Failure("Registration error occurred");
        }
    }
    
    private ValidationResult ValidateRegistrationRequest(UserRegistrationRequest request)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(request.Username))
            errors.Add("Username is required");
        else if (request.Username.Length < 3 || request.Username.Length > 50)
            errors.Add("Username must be between 3 and 50 characters");
        else if (!Regex.IsMatch(request.Username, @"^[a-zA-Z0-9_-]+$"))
            errors.Add("Username can only contain letters, numbers, underscores, and hyphens");
        
        if (string.IsNullOrWhiteSpace(request.Email))
            errors.Add("Email is required");
        else if (!IsValidEmail(request.Email))
            errors.Add("Invalid email format");
        
        if (string.IsNullOrWhiteSpace(request.Password))
            errors.Add("Password is required");
        else if (request.Password.Length < 8)
            errors.Add("Password must be at least 8 characters long");
        else if (!Regex.IsMatch(request.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]"))
            errors.Add("Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character");
        
        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
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
}
```

#### Sistema de Autorización Basado en Claims

```csharp
public class ClaimsBasedAuthorizationHandler : AuthorizationHandler<ClaimsRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ClaimsRequirement requirement)
    {
        // Verifica si el usuario tiene los claims requeridos
        var hasRequiredClaims = requirement.RequiredClaims.All(claim =>
            context.User.HasClaim(c => c.Type == claim.Type && c.Value == claim.Value));
        
        if (hasRequiredClaims)
        {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}

public class ClaimsRequirement : IAuthorizationRequirement
{
    public IEnumerable<Claim> RequiredClaims { get; }
    
    public ClaimsRequirement(params Claim[] requiredClaims)
    {
        RequiredClaims = requiredClaims;
    }
}

// Uso en controladores
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SecureController : ControllerBase
{
    [HttpGet("admin")]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult AdminOnly()
    {
        return Ok("Admin access granted");
    }
    
    [HttpGet("user/{id}")]
    [Authorize(Policy = "UserAccess")]
    public IActionResult UserAccess(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == id.ToString() || User.IsInRole("Admin"))
        {
            return Ok($"Access granted to user {id}");
        }
        
        return Forbid();
    }
    
    [HttpPost("data")]
    [Authorize(Policy = "DataModification")]
    public IActionResult ModifyData([FromBody] object data)
    {
        return Ok("Data modification allowed");
    }
}
```

### 3.2 Protección Contra Ataques Comunes

Implementar protección contra los ataques más comunes es esencial para la seguridad de la aplicación.

#### Protección Contra SQL Injection

```csharp
public class SecureUserRepository : IUserRepository
{
    private readonly DbContext _context;
    private readonly ILogger<SecureUserRepository> _logger;
    
    public SecureUserRepository(DbContext context, ILogger<SecureUserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    // ✅ Seguro: Usa Entity Framework con parámetros
    public async Task<User> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }
    
    // ✅ Seguro: Usa parámetros SQL si es necesario
    public async Task<List<User>> SearchUsersAsync(string searchTerm)
    {
        // Usa parámetros para evitar SQL injection
        var sql = @"
            SELECT * FROM Users 
            WHERE Username LIKE @SearchTerm 
            OR Email LIKE @SearchTerm";
        
        var parameter = new SqlParameter("@SearchTerm", $"%{searchTerm}%");
        
        return await _context.Users
            .FromSqlRaw(sql, parameter)
            .ToListAsync();
    }
    
    // ❌ Peligroso: Concatenación directa de strings
    // public async Task<List<User>> SearchUsersUnsafeAsync(string searchTerm)
    // {
    //     var sql = $"SELECT * FROM Users WHERE Username LIKE '%{searchTerm}%'";
    //     // Esto es vulnerable a SQL injection
    //     return await _context.Users.FromSqlRaw(sql).ToListAsync();
    // }
}
```

#### Protección Contra XSS (Cross-Site Scripting)

```csharp
public class XssProtectionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<XssProtectionMiddleware> _logger;
    
    public XssProtectionMiddleware(RequestDelegate next, ILogger<XssProtectionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Agrega headers de seguridad
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
        
        // Sanitiza datos de entrada
        if (context.Request.HasFormContentType)
        {
            await SanitizeFormDataAsync(context.Request);
        }
        
        await _next(context);
    }
    
    private async Task SanitizeFormDataAsync(HttpRequest request)
    {
        if (request.Form != null)
        {
            var sanitizedForm = new FormCollection(
                request.Form.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new StringValues(SanitizeInput(kvp.Value))
                )
            );
            
            // Reemplaza el Form original con el sanitizado
            request.Form = sanitizedForm;
        }
    }
    
    private string SanitizeInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        
        // Elimina tags HTML peligrosos
        var dangerousTags = new[] { "script", "iframe", "object", "embed", "form" };
        var sanitized = input;
        
        foreach (var tag in dangerousTags)
        {
            var pattern = $@"<{tag}[^>]*>.*?</{tag}>";
            sanitized = Regex.Replace(sanitized, pattern, "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }
        
        // Escapa caracteres especiales
        sanitized = HttpUtility.HtmlEncode(sanitized);
        
        return sanitized;
    }
}

// Uso en Program.cs
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // ... configuración de servicios
        
        var app = builder.Build();
        
        // Agrega el middleware de protección XSS
        app.UseMiddleware<XssProtectionMiddleware>();
        
        // ... resto de la configuración
        
        app.Run();
    }
}
```

#### Protección Contra CSRF (Cross-Site Request Forgery)

```csharp
public class CsrfProtectionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CsrfProtectionMiddleware> _logger;
    
    public CsrfProtectionMiddleware(RequestDelegate next, ILogger<CsrfProtectionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Solo aplica a métodos que modifican datos
        if (IsModifyingMethod(context.Request.Method))
        {
            if (!await ValidateCsrfTokenAsync(context))
            {
                _logger.LogWarning("CSRF token validation failed for request: {Method} {Path}", 
                    context.Request.Method, context.Request.Path);
                
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("CSRF token validation failed");
                return;
            }
        }
        
        await _next(context);
    }
    
    private bool IsModifyingMethod(string method)
    {
        return new[] { "POST", "PUT", "PATCH", "DELETE" }.Contains(method.ToUpper());
    }
    
    private async Task<bool> ValidateCsrfTokenAsync(HttpContext context)
    {
        // Obtiene el token del header o del form
        var tokenFromHeader = context.Request.Headers["X-CSRF-Token"].FirstOrDefault();
        var tokenFromForm = context.Request.Form["__RequestVerificationToken"].FirstOrDefault();
        
        var submittedToken = tokenFromHeader ?? tokenFromForm;
        
        if (string.IsNullOrEmpty(submittedToken))
        {
            return false;
        }
        
        // Obtiene el token de la sesión
        var sessionToken = context.Session.GetString("CSRFToken");
        
        if (string.IsNullOrEmpty(sessionToken))
        {
            return false;
        }
        
        // Compara los tokens
        return submittedToken == sessionToken;
    }
}

// Generador de tokens CSRF
public class CsrfTokenService
{
    public string GenerateToken()
    {
        var tokenBytes = new byte[32];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(tokenBytes);
        }
        
        return Convert.ToBase64String(tokenBytes);
    }
    
    public void StoreTokenInSession(ISession session, string token)
    {
        session.SetString("CSRFToken", token);
    }
}

// Uso en controladores
[ApiController]
[Route("api/[controller]")]
public class SecureDataController : ControllerBase
{
    [HttpPost]
    [ValidateAntiForgeryToken] // Atributo de ASP.NET Core
    public IActionResult CreateData([FromBody] object data)
    {
        // El token CSRF ya fue validado por el middleware
        return Ok("Data created successfully");
    }
}
```

### 3.3 Cifrado y Hashing Seguro

Implementar cifrado y hashing seguro es fundamental para proteger datos sensibles.

#### Servicio de Hashing de Contraseñas

```csharp
public class SecurePasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 10000; // Número de iteraciones
    
    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty");
        
        // Genera un salt aleatorio
        byte[] salt = new byte[SaltSize];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(salt);
        }
        
        // Genera el hash usando PBKDF2
        byte[] hash = GenerateHash(password, salt, Iterations, HashSize);
        
        // Combina salt, iteraciones y hash en un string
        var hashBytes = new byte[SaltSize + 4 + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(BitConverter.GetBytes(Iterations), 0, hashBytes, SaltSize, 4);
        Array.Copy(hash, 0, hashBytes, SaltSize + 4, HashSize);
        
        return Convert.ToBase64String(hashBytes);
    }
    
    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
            return false;
        
        try
        {
            // Decodifica el hash combinado
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);
            
            // Extrae el salt, iteraciones y hash
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);
            
            int iterations = BitConverter.ToInt32(hashBytes, SaltSize);
            byte[] hash = new byte[HashSize];
            Array.Copy(hashBytes, SaltSize + 4, hash, 0, HashSize);
            
            // Genera el hash con los mismos parámetros
            byte[] testHash = GenerateHash(password, salt, iterations, HashSize);
            
            // Compara los hashes de manera segura contra timing attacks
            return ConstantTimeEquals(hash, testHash);
        }
        catch
        {
            return false;
        }
    }
    
    private byte[] GenerateHash(string password, byte[] salt, int iterations, int hashSize)
    {
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
        {
            return pbkdf2.GetBytes(hashSize);
        }
    }
    
    private bool ConstantTimeEquals(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
            return false;
        
        int result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }
        
        return result == 0;
    }
}
```

#### Servicio de Cifrado Simétrico

```csharp
public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;
    
    public EncryptionService(IConfiguration configuration)
    {
        // Obtiene la clave de cifrado de la configuración
        var keyString = configuration["Encryption:Key"];
        var ivString = configuration["Encryption:IV"];
        
        if (string.IsNullOrEmpty(keyString) || string.IsNullOrEmpty(ivString))
        {
            throw new InvalidOperationException("Encryption key and IV must be configured");
        }
        
        _key = Convert.FromBase64String(keyString);
        _iv = Convert.FromBase64String(ivString);
        
        // Valida que la clave tenga el tamaño correcto para AES-256
        if (_key.Length != 32)
        {
            throw new InvalidOperationException("Encryption key must be 32 bytes (256 bits) for AES-256");
        }
        
        if (_iv.Length != 16)
        {
            throw new InvalidOperationException("IV must be 16 bytes (128 bits)");
        }
    }
    
    public string Encrypt(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
            return plaintext;
        
        using (var aes = Aes.Create())
        {
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            
            using (var encryptor = aes.CreateEncryptor())
            using (var msEncrypt = new MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plaintext);
                }
                
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }
    
    public string Decrypt(string ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext))
            return ciphertext;
        
        try
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                
                using (var decryptor = aes.CreateDecryptor())
                using (var msDecrypt = new MemoryStream(Convert.FromBase64String(ciphertext)))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }
        catch (Exception ex)
        {
            throw new CryptographicException("Failed to decrypt data", ex);
        }
    }
    
    public async Task<string> EncryptAsync(string plaintext)
    {
        return await Task.Run(() => Encrypt(plaintext));
    }
    
    public async Task<string> DecryptAsync(string ciphertext)
    {
        return await Task.Run(() => Decrypt(ciphertext));
    }
}
```

#### Servicio de Hashing de Datos

```csharp
public class DataHashingService : IDataHashingService
{
    public string ComputeHash(string data, string algorithm = "SHA256")
    {
        if (string.IsNullOrEmpty(data))
            return string.Empty;
        
        using (var hashAlgorithm = GetHashAlgorithm(algorithm))
        {
            var hashBytes = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hashBytes);
        }
    }
    
    public string ComputeFileHash(string filePath, string algorithm = "SHA256")
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found", filePath);
        
        using (var hashAlgorithm = GetHashAlgorithm(algorithm))
        using (var stream = File.OpenRead(filePath))
        {
            var hashBytes = hashAlgorithm.ComputeHash(stream);
            return Convert.ToBase64String(hashBytes);
        }
    }
    
    public async Task<string> ComputeFileHashAsync(string filePath, string algorithm = "SHA256")
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found", filePath);
        
        using (var hashAlgorithm = GetHashAlgorithm(algorithm))
        using (var stream = File.OpenRead(filePath))
        {
            var hashBytes = await hashAlgorithm.ComputeHashAsync(stream);
            return Convert.ToBase64String(hashBytes);
        }
    }
    
    public bool VerifyHash(string data, string expectedHash, string algorithm = "SHA256")
    {
        var actualHash = ComputeHash(data, algorithm);
        return ConstantTimeEquals(actualHash, expectedHash);
    }
    
    private HashAlgorithm GetHashAlgorithm(string algorithm)
    {
        return algorithm.ToUpper() switch
        {
            "MD5" => MD5.Create(),
            "SHA1" => SHA1.Create(),
            "SHA256" => SHA256.Create(),
            "SHA384" => SHA384.Create(),
            "SHA512" => SHA512.Create(),
            _ => throw new ArgumentException($"Unsupported hash algorithm: {algorithm}")
        };
    }
    
    private bool ConstantTimeEquals(string a, string b)
    {
        if (a.Length != b.Length)
            return false;
        
        int result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }
        
        return result == 0;
    }
}
```

### 3.4 Validación y Sanitización de Entrada

La validación y sanitización de entrada es crucial para prevenir ataques y mantener la integridad de los datos.

#### Validador de Entrada Avanzado

```csharp
public class InputValidator : IInputValidator
{
    private readonly ILogger<InputValidator> _logger;
    
    public InputValidator(ILogger<InputValidator> logger)
    {
        _logger = logger;
    }
    
    public ValidationResult ValidateEmail(string email)
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrWhiteSpace(email))
        {
            result.AddError("Email is required");
            return result;
        }
        
        // Validación básica de formato
        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            result.AddError("Invalid email format");
        }
        
        // Validación de longitud
        if (email.Length > 254)
        {
            result.AddError("Email is too long");
        }
        
        // Validación de caracteres peligrosos
        if (ContainsDangerousCharacters(email))
        {
            result.AddError("Email contains dangerous characters");
            _logger.LogWarning("Potentially dangerous email input detected: {Email}", email);
        }
        
        return result;
    }
    
    public ValidationResult ValidateUsername(string username)
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrWhiteSpace(username))
        {
            result.AddError("Username is required");
            return result;
        }
        
        // Validación de longitud
        if (username.Length < 3 || username.Length > 50)
        {
            result.AddError("Username must be between 3 and 50 characters");
        }
        
        // Validación de caracteres permitidos
        if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_-]+$"))
        {
            result.AddError("Username can only contain letters, numbers, underscores, and hyphens");
        }
        
        // Validación de caracteres peligrosos
        if (ContainsDangerousCharacters(username))
        {
            result.AddError("Username contains dangerous characters");
            _logger.LogWarning("Potentially dangerous username input detected: {Username}", username);
        }
        
        return result;
    }
    
    public ValidationResult ValidatePassword(string password)
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrWhiteSpace(password))
        {
            result.AddError("Password is required");
            return result;
        }
        
        // Validación de longitud mínima
        if (password.Length < 8)
        {
            result.AddError("Password must be at least 8 characters long");
        }
        
        // Validación de complejidad
        if (!Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]"))
        {
            result.AddError("Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character");
        }
        
        // Validación de caracteres peligrosos
        if (ContainsDangerousCharacters(password))
        {
            result.AddError("Password contains dangerous characters");
            _logger.LogWarning("Potentially dangerous password input detected");
        }
        
        return result;
    }
    
    public string SanitizeHtml(string html)
    {
        if (string.IsNullOrEmpty(html))
            return html;
        
        // Lista de tags HTML permitidos
        var allowedTags = new[] { "p", "br", "strong", "em", "u", "ol", "ul", "li", "h1", "h2", "h3", "h4", "h5", "h6" };
        
        // Lista de atributos permitidos
        var allowedAttributes = new[] { "class", "id", "style" };
        
        // Crea un documento HTML
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        
        // Sanitiza el documento
        SanitizeNode(doc.DocumentNode, allowedTags, allowedAttributes);
        
        return doc.DocumentNode.InnerHtml;
    }
    
    private void SanitizeNode(HtmlNode node, string[] allowedTags, string[] allowedAttributes)
    {
        if (node.NodeType == HtmlNodeType.Element)
        {
            // Verifica si el tag está permitido
            if (!allowedTags.Contains(node.Name.ToLower()))
            {
                // Si no está permitido, convierte el contenido a texto plano
                var textNode = HtmlNode.CreateNode(node.InnerText);
                node.ParentNode.ReplaceChild(textNode, node);
                return;
            }
            
            // Sanitiza atributos
            var attributesToRemove = new List<HtmlAttribute>();
            foreach (var attribute in node.Attributes)
            {
                if (!allowedAttributes.Contains(attribute.Name.ToLower()))
                {
                    attributesToRemove.Add(attribute);
                }
                else
                {
                    // Sanitiza el valor del atributo
                    attribute.Value = HttpUtility.HtmlAttributeEncode(attribute.Value);
                }
            }
            
            foreach (var attribute in attributesToRemove)
            {
                node.Attributes.Remove(attribute);
            }
        }
        
        // Procesa nodos hijos
        var children = node.ChildNodes.ToList();
        foreach (var child in children)
        {
            SanitizeNode(child, allowedTags, allowedAttributes);
        }
    }
    
    private bool ContainsDangerousCharacters(string input)
    {
        // Caracteres que podrían ser usados en ataques
        var dangerousPatterns = new[]
        {
            @"<script", @"javascript:", @"vbscript:", @"onload=", @"onerror=",
            @"<iframe", @"<object", @"<embed", @"<form", @"<input"
        };
        
        return dangerousPatterns.Any(pattern => 
            Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase));
    }
}

public class ValidationResult
{
    public List<string> Errors { get; } = new List<string>();
    public bool IsValid => Errors.Count == 0;
    
    public void AddError(string error)
    {
        Errors.Add(error);
    }
    
    public void AddErrors(IEnumerable<string> errors)
    {
        Errors.AddRange(errors);
    }
}
```

---

## 💻 Ejercicios Prácticos

### Ejercicio 1: Implementar Sistema de Bloqueo de Cuentas

Crea un sistema que bloquee cuentas después de múltiples intentos fallidos de login:

```csharp
public class AccountLockoutService
{
    // Implementa:
    // - Conteo de intentos fallidos
    // - Bloqueo temporal de cuentas
    // - Desbloqueo automático
    // - Notificaciones de seguridad
}
```

### Ejercicio 2: Validador de Archivos

Implementa un validador que verifique la seguridad de archivos subidos:

```csharp
public class FileSecurityValidator
{
    // Implementa:
    // - Validación de tipos de archivo
    // - Escaneo de malware
    // - Verificación de contenido
    // - Sanitización de nombres
}
```

---

## 🔍 Casos de Uso Reales

### 1. API Segura con Rate Limiting

```csharp
[ApiController]
[Route("api/[controller]")]
public class SecureApiController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IEncryptionService _encryptionService;
    
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("LoginPolicy")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Valida la entrada
        var validationResult = ValidateLoginRequest(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }
        
        // Autentica al usuario
        var authResult = await _userService.AuthenticateAsync(request.Username, request.Password);
        if (!authResult.IsSuccess)
        {
            return Unauthorized("Invalid credentials");
        }
        
        // Genera token JWT
        var token = GenerateJwtToken(authResult.User);
        
        return Ok(new { token, user = authResult.User });
    }
    
    [HttpPost("encrypt")]
    [Authorize]
    public IActionResult EncryptData([FromBody] EncryptRequest request)
    {
        // Valida que el usuario tenga permisos de cifrado
        if (!User.HasClaim("Permission", "Encrypt"))
        {
            return Forbid();
        }
        
        // Cifra los datos
        var encryptedData = _encryptionService.Encrypt(request.Data);
        
        return Ok(new { encryptedData });
    }
}
```

### 2. Middleware de Seguridad Personalizado

```csharp
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    
    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Headers de seguridad
        context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'");
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
        
        await _next(context);
    }
}
```

---

## 📊 Métricas de Seguridad

### KPIs de Seguridad

1. **Failed Login Attempts**: Intentos fallidos de autenticación
2. **Account Lockouts**: Cuentas bloqueadas por seguridad
3. **Security Incidents**: Incidentes de seguridad detectados
4. **Vulnerability Scans**: Resultados de escaneos de vulnerabilidades
5. **Security Updates**: Tiempo de aplicación de parches

### Herramientas de Seguridad

- **OWASP ZAP**: Escaneo de vulnerabilidades
- **SonarQube**: Análisis de código seguro
- **Snyk**: Detección de dependencias vulnerables
- **Microsoft Security Code Analysis**: Análisis de seguridad

---

## 🎯 Resumen de la Clase

En esta clase hemos aprendido:

✅ **Autenticación y Autorización**: Sistemas robustos de seguridad
✅ **Protección Contra Ataques**: XSS, CSRF, SQL Injection
✅ **Cifrado y Hashing**: Protección de datos sensibles
✅ **Validación y Sanitización**: Entrada segura y limpia
✅ **Implementación Práctica**: Código real con medidas de seguridad

---

## 🚀 Próximos Pasos

En la siguiente clase aprenderemos sobre:
- **Seguridad de APIs y Microservicios**
- JWT y OAuth 2.0 avanzado
- Rate limiting y protección CSRF
- API security headers

---

## 🔗 Enlaces de Referencia

- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [.NET Cryptography](https://docs.microsoft.com/en-us/dotnet/standard/security/cryptography)
- [Security Best Practices](https://docs.microsoft.com/en-us/dotnet/architecture/security/)
