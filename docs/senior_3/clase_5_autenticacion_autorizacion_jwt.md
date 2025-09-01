# 🚀 Clase 5: Autenticación y Autorización JWT

## 🧭 Navegación
- **⬅️ Anterior**: [Clase 4: Validación y Manejo de Errores](clase_4_validacion_manejo_errores.md)
- **🏠 Inicio del Módulo**: [Módulo 10: APIs REST y Web APIs](README.md)
- **➡️ Siguiente**: [Clase 6: Entity Framework Core](clase_6_entity_framework_core.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 📚 Descripción

En esta clase aprenderás a implementar un sistema completo de autenticación y autorización usando JWT (JSON Web Tokens), incluyendo generación de tokens, validación, refresh tokens y políticas de autorización.

## 🎯 Objetivos de Aprendizaje

- Implementar autenticación JWT en ASP.NET Core
- Crear servicios de generación y validación de tokens
- Implementar refresh tokens para mayor seguridad
- Configurar políticas de autorización personalizadas
- Crear controladores de autenticación
- Implementar middleware de autorización personalizado

## 📖 Contenido Teórico

### Configuración de JWT

#### Configuración Básica en Program.cs
```csharp
// Program.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configurar JWT Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Configurar autenticación JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
    var key = Encoding.UTF8.GetBytes(jwtSettings!.SecretKey);
    
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
    
    // Configurar eventos
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        },
        
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            
            var result = JsonSerializer.Serialize(new
            {
                error = "No autorizado",
                message = "Token de autenticación requerido"
            });
            
            return context.Response.WriteAsync(result);
        },
        
        OnForbidden = context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            
            var result = JsonSerializer.Serialize(new
            {
                error = "Prohibido",
                message = "No tienes permisos para acceder a este recurso"
            });
            
            return context.Response.WriteAsync(result);
        }
    };
});

// Configurar autorización
builder.Services.AddAuthorization(options =>
{
    // Política para usuarios autenticados
    options.AddPolicy("AuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
    
    // Política para administradores
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
    
    // Política para usuarios o administradores
    options.AddPolicy("UserOrAdmin", policy =>
        policy.RequireRole("User", "Admin"));
    
    // Política personalizada para recursos específicos
    options.AddPolicy("ResourceOwner", policy =>
        policy.RequireAssertion(context =>
        {
            var user = context.User;
            var resourceId = context.Resource as string;
            
            if (string.IsNullOrEmpty(resourceId))
                return false;
            
            // Verificar si el usuario es propietario del recurso
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return !string.IsNullOrEmpty(userId) && userId == resourceId;
        }));
    
    // Política para operaciones de lectura
    options.AddPolicy("ReadAccess", policy =>
        policy.RequireAssertion(context =>
        {
            var user = context.User;
            var resource = context.Resource as string;
            
            if (string.IsNullOrEmpty(resource))
                return false;
            
            // Lógica de verificación de permisos de lectura
            return HasReadPermission(user, resource);
        }));
});

var app = builder.Build();

// Configurar pipeline HTTP
app.UseAuthentication();
app.UseAuthorization();

app.Run();

// Función auxiliar para verificar permisos de lectura
bool HasReadPermission(ClaimsPrincipal user, string resource)
{
    // Implementar lógica de verificación de permisos
    var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    // Administradores tienen acceso completo
    if (userRole == "Admin")
        return true;
    
    // Usuarios pueden leer recursos públicos
    if (resource.StartsWith("public/"))
        return true;
    
    // Usuarios pueden leer sus propios recursos
    if (resource.StartsWith($"user/{userId}/"))
        return true;
    
    return false;
}
```

#### Configuración en appsettings.json
```json
{
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-with-at-least-32-characters-for-production",
    "Issuer": "MyApi",
    "Audience": "MyApiUsers",
    "ExpirationInMinutes": 60,
    "RefreshTokenExpirationInDays": 7,
    "ClockSkewInMinutes": 0
  }
}
```

### Modelos y DTOs

#### DTOs de Autenticación
```csharp
public class LoginDto
{
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Recordar sesión")]
    public bool RememberMe { get; set; }
}

public class RegisterDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(50, ErrorMessage = "El apellido no puede exceder 50 caracteres")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]", 
        ErrorMessage = "La contraseña debe contener al menos una letra minúscula, una mayúscula, un número y un carácter especial")]
    public string Password { get; set; } = string.Empty;

    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "El rol no puede exceder 20 caracteres")]
    public string? Role { get; set; } = "User";
}

public class RefreshTokenDto
{
    [Required(ErrorMessage = "El token de acceso es obligatorio")]
    public string AccessToken { get; set; } = string.Empty;

    [Required(ErrorMessage = "El refresh token es obligatorio")]
    public string RefreshToken { get; set; } = string.Empty;
}

public class ChangePasswordDto
{
    [Required(ErrorMessage = "La contraseña actual es obligatoria")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]", 
        ErrorMessage = "La contraseña debe contener al menos una letra minúscula, una mayúscula, un número y un carácter especial")]
    public string NewPassword { get; set; } = string.Empty;

    [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
```

#### DTOs de Respuesta
```csharp
public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = new();
}

public class RefreshTokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
```

### Servicios de JWT

#### Servicio de JWT Básico
```csharp
public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    bool ValidateAccessToken(string token);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    Task<bool> ValidateRefreshTokenAsync(string refreshToken, int userId);
    Task<string> GenerateNewRefreshTokenAsync(int userId);
}

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public JwtService(IOptions<JwtSettings> jwtSettings, IRefreshTokenRepository refreshTokenRepository)
    {
        _jwtSettings = jwtSettings.Value;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Role, user.Role),
            new("userId", user.Id.ToString()),
            new("userEmail", user.Email),
            new("userRole", user.Role)
        };

        // Agregar claims personalizados si es necesario
        if (user.IsActive)
        {
            claims.Add(new Claim("isActive", "true"));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            notBefore: DateTime.UtcNow,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public bool ValidateAccessToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        try
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> ValidateRefreshTokenAsync(string refreshToken, int userId)
    {
        var storedRefreshToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
        
        if (storedRefreshToken == null || 
            storedRefreshToken.UserId != userId || 
            storedRefreshToken.IsExpired || 
            storedRefreshToken.IsRevoked)
        {
            return false;
        }

        return true;
    }

    public async Task<string> GenerateNewRefreshTokenAsync(int userId)
    {
        var refreshToken = GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays);

        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = userId,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _refreshTokenRepository.CreateAsync(refreshTokenEntity);
        return refreshToken;
    }
}
```

#### Servicio de Autenticación
```csharp
public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginDto loginDto);
    Task<UserDto> RegisterAsync(RegisterDto registerDto);
    Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
    Task<bool> RevokeRefreshTokenAsync(int userId, string refreshToken);
    Task<bool> LogoutAsync(int userId);
}

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserService userService,
        IJwtService jwtService,
        IPasswordHasher passwordHasher,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<AuthService> logger)
    {
        _userService = userService;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
    {
        // Verificar credenciales del usuario
        var user = await _userService.GetUserByEmailAsync(loginDto.Email);
        if (user == null)
        {
            _logger.LogWarning("Intento de login con email no registrado: {Email}", loginDto.Email);
            throw new UnauthorizedAccessException("Credenciales inválidas");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Intento de login de usuario inactivo: {Email}", loginDto.Email);
            throw new UnauthorizedAccessException("Usuario inactivo");
        }

        // Verificar contraseña
        if (!_passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Intento de login con contraseña incorrecta para usuario: {Email}", loginDto.Email);
            throw new UnauthorizedAccessException("Credenciales inválidas");
        }

        // Generar tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = await _jwtService.GenerateNewRefreshTokenAsync(user.Id);

        // Actualizar último login
        await _userService.UpdateLastLoginAsync(user.Id);

        _logger.LogInformation("Login exitoso para usuario: {Email}", loginDto.Email);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60), // Configurable
            User = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            }
        };
    }

    public async Task<UserDto> RegisterAsync(RegisterDto registerDto)
    {
        // Verificar si el email ya existe
        var existingUser = await _userService.GetUserByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("El email ya está registrado");
        }

        // Crear nuevo usuario
        var user = new User
        {
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Email = registerDto.Email,
            PasswordHash = _passwordHasher.HashPassword(registerDto.Password),
            Role = registerDto.Role ?? "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createdUser = await _userService.CreateUserAsync(user);

        _logger.LogInformation("Usuario registrado exitosamente: {Email}", registerDto.Email);

        return new UserDto
        {
            Id = createdUser.Id,
            FirstName = createdUser.FirstName,
            LastName = createdUser.LastName,
            Email = createdUser.Email,
            Role = createdUser.Role,
            IsActive = createdUser.IsActive,
            CreatedAt = createdUser.CreatedAt
        };
    }

    public async Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        // Obtener claims del token expirado
        var principal = _jwtService.GetPrincipalFromExpiredToken(refreshTokenDto.AccessToken);
        if (principal == null)
        {
            throw new UnauthorizedAccessException("Token inválido");
        }

        var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (userId == 0)
        {
            throw new UnauthorizedAccessException("Token inválido");
        }

        // Verificar refresh token
        if (!await _jwtService.ValidateRefreshTokenAsync(refreshTokenDto.RefreshToken, userId))
        {
            throw new UnauthorizedAccessException("Refresh token inválido");
        }

        // Obtener usuario
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Usuario no válido");
        }

        // Generar nuevos tokens
        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = await _jwtService.GenerateNewRefreshTokenAsync(userId);

        // Revocar refresh token anterior
        await RevokeRefreshTokenAsync(userId, refreshTokenDto.RefreshToken);

        _logger.LogInformation("Tokens refrescados para usuario: {UserId}", userId);

        return new RefreshTokenResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
    {
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("Usuario no encontrado");
        }

        // Verificar contraseña actual
        if (!_passwordHasher.VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Contraseña actual incorrecta");
        }

        // Cambiar contraseña
        var newPasswordHash = _passwordHasher.HashPassword(changePasswordDto.NewPassword);
        var result = await _userService.UpdatePasswordAsync(userId, newPasswordHash);

        if (result)
        {
            // Revocar todos los refresh tokens del usuario
            await _refreshTokenRepository.RevokeAllForUserAsync(userId);
            _logger.LogInformation("Contraseña cambiada para usuario: {UserId}", userId);
        }

        return result;
    }

    public async Task<bool> RevokeRefreshTokenAsync(int userId, string refreshToken)
    {
        var result = await _refreshTokenRepository.RevokeAsync(refreshToken, userId);
        if (result)
        {
            _logger.LogInformation("Refresh token revocado para usuario: {UserId}", userId);
        }
        return result;
    }

    public async Task<bool> LogoutAsync(int userId)
    {
        // Revocar todos los refresh tokens del usuario
        var result = await _refreshTokenRepository.RevokeAllForUserAsync(userId);
        if (result)
        {
            _logger.LogInformation("Logout exitoso para usuario: {UserId}", userId);
        }
        return result;
    }
}
```

### Controlador de Autenticación

#### Controlador Completo
```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Inicia sesión de un usuario
    /// </summary>
    /// <param name="loginDto">Credenciales de login</param>
    /// <returns>Token de acceso y refresh token</returns>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginDto loginDto)
    {
        try
        {
            var response = await _authService.LoginAsync(loginDto);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Login fallido: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el login");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Registra un nuevo usuario
    /// </summary>
    /// <param name="registerDto">Datos del usuario a registrar</param>
    /// <returns>Usuario creado</returns>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        try
        {
            var user = await _authService.RegisterAsync(registerDto);
            return CreatedAtAction(nameof(Register), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Registro fallido: {Message}", ex.Message);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el registro");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Refresca el token de acceso usando un refresh token
    /// </summary>
    /// <param name="refreshTokenDto">Token de acceso y refresh token</param>
    /// <returns>Nuevos tokens</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RefreshTokenResponseDto>> RefreshToken(RefreshTokenDto refreshTokenDto)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(refreshTokenDto);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Refresh token fallido: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el refresh token");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Cambia la contraseña del usuario autenticado
    /// </summary>
    /// <param name="changePasswordDto">Datos para cambiar contraseña</param>
    /// <returns>Resultado de la operación</returns>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
            {
                return Unauthorized(new { message = "Usuario no válido" });
            }

            var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);
            if (result)
            {
                return Ok(new { message = "Contraseña cambiada exitosamente" });
            }

            return BadRequest(new { message = "No se pudo cambiar la contraseña" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Cambio de contraseña fallido: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Usuario no encontrado: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el cambio de contraseña");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Cierra la sesión del usuario
    /// </summary>
    /// <returns>Resultado de la operación</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Logout()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
            {
                return Unauthorized(new { message = "Usuario no válido" });
            }

            var result = await _authService.LogoutAsync(userId);
            if (result)
            {
                return Ok(new { message = "Logout exitoso" });
            }

            return BadRequest(new { message = "No se pudo cerrar la sesión" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el logout");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene información del usuario autenticado
    /// </summary>
    /// <returns>Información del usuario</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
            {
                return Unauthorized(new { message = "Usuario no válido" });
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            return Ok(new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener información del usuario");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}
```

### Middleware de Autorización Personalizado

#### Middleware de Verificación de Permisos
```csharp
public class PermissionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PermissionMiddleware> _logger;

    public PermissionMiddleware(RequestDelegate next, ILogger<PermissionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            await _next(context);
            return;
        }

        var permissionAttribute = endpoint.Metadata.GetMetadata<RequirePermissionAttribute>();
        if (permissionAttribute == null)
        {
            await _next(context);
            return;
        }

        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { message = "No autenticado" });
            return;
        }

        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userRole) || string.IsNullOrEmpty(userId))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { message = "Permisos insuficientes" });
            return;
        }

        // Verificar permisos
        if (!HasPermission(userRole, permissionAttribute.Permission, permissionAttribute.Resource))
        {
            _logger.LogWarning("Acceso denegado: Usuario {UserId} con rol {Role} intentó acceder a {Permission} en {Resource}", 
                userId, userRole, permissionAttribute.Permission, permissionAttribute.Resource);
            
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { message = "Permisos insuficientes" });
            return;
        }

        await _next(context);
    }

    private bool HasPermission(string userRole, string permission, string? resource)
    {
        // Lógica de verificación de permisos
        switch (userRole.ToLower())
        {
            case "admin":
                return true; // Administradores tienen acceso completo
                
            case "manager":
                return permission switch
                {
                    "read" => true,
                    "create" => true,
                    "update" => true,
                    "delete" => resource?.StartsWith("user/") == false, // No pueden eliminar usuarios
                    _ => false
                };
                
            case "user":
                return permission switch
                {
                    "read" => true,
                    "create" => resource?.StartsWith("order/") == true, // Solo pueden crear órdenes
                    "update" => resource?.StartsWith($"user/{resource}") == true, // Solo sus propios recursos
                    "delete" => false,
                    _ => false
                };
                
            default:
                return false;
        }
    }
}

// Atributo personalizado para requerir permisos
[AttributeUsage(AttributeTargets.Method)]
public class RequirePermissionAttribute : Attribute
{
    public string Permission { get; }
    public string? Resource { get; }

    public RequirePermissionAttribute(string permission, string? resource = null)
    {
        Permission = permission;
        Resource = resource;
    }
}

// Uso del atributo
[HttpGet("admin/users")]
[Authorize(Roles = "Admin")]
[RequirePermission("read", "admin/users")]
public async Task<ActionResult<IEnumerable<UserDto>>> GetAdminUsers()
{
    // Implementación...
}
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Sistema de Autenticación Completo
Implementa un sistema completo que incluya:
- Login y registro de usuarios
- Generación y validación de JWT
- Refresh tokens
- Cambio de contraseña
- Logout

### Ejercicio 2: Políticas de Autorización
Crea políticas personalizadas para:
- Acceso a recursos por propietario
- Permisos basados en roles
- Permisos basados en claims personalizados
- Middleware de verificación de permisos

### Ejercicio 3: Refresh Tokens
Implementa un sistema robusto de refresh tokens con:
- Almacenamiento seguro en base de datos
- Rotación de tokens
- Revocación de tokens
- Expiración configurable

### Ejercicio 4: Seguridad Avanzada
Implementa características de seguridad como:
- Rate limiting para endpoints de autenticación
- Bloqueo de cuentas después de intentos fallidos
- Auditoría de accesos
- Headers de seguridad

## 📝 Quiz de Autoevaluación

1. ¿Cuáles son las ventajas de usar JWT sobre sesiones tradicionales?
2. ¿Por qué es importante implementar refresh tokens?
3. ¿Cómo implementarías políticas de autorización personalizadas?
4. ¿Qué medidas de seguridad adicionales considerarías para un sistema de autenticación?
5. ¿Cómo manejarías la revocación de tokens en un sistema distribuido?

## 🔗 Enlaces Útiles

- [JWT.io](https://jwt.io/)
- [ASP.NET Core Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)
- [ASP.NET Core Authorization](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/)
- [JWT Bearer Token Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)

## 🚀 Siguiente Clase

En la siguiente clase aprenderás a implementar Entity Framework Core, incluyendo configuración del contexto, repositorios genéricos y operaciones de base de datos.

---

**💡 Consejo**: Siempre implementa refresh tokens para mayor seguridad y considera implementar rate limiting en endpoints de autenticación para prevenir ataques de fuerza bruta.
