# 🔒 Clase 9: Sistema de Seguridad Avanzada

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 8: Sistema de Analytics y Reportes](../senior_9/clase_8_sistema_analytics_reportes.md)
- **🏠 Inicio del Módulo**: [Módulo 16: Maestría Total y Liderazgo Técnico](../senior_9/README.md)
- **➡️ Siguiente**: [Clase 10: Sistema de Monitoreo y Logging](../senior_9/clase_10_sistema_monitoreo_logging.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** sistema de autenticación avanzada
2. **Crear** sistema de autorización granular
3. **Configurar** auditoría de seguridad
4. **Implementar** protección contra ataques
5. **Configurar** encriptación de datos

---

## 🔒 **Sistema de Seguridad Avanzada**

### **Servicio de Autenticación**

```csharp
// MusicalMatching.Application/Services/IAuthenticationService.cs
namespace MusicalMatching.Application.Services;

public interface IAuthenticationService
{
    Task<AuthenticationResult> AuthenticateAsync(LoginRequest request);
    Task<AuthenticationResult> RefreshTokenAsync(RefreshTokenRequest request);
    Task<bool> LogoutAsync(Guid userId, string token);
    Task<bool> RevokeTokenAsync(string token);
    Task<bool> ValidateTokenAsync(string token);
    Task<SecurityAudit> CreateSecurityAuditAsync(SecurityEvent securityEvent);
    Task<List<SecurityAudit>> GetSecurityAuditsAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<bool> IsAccountLockedAsync(Guid userId);
    Task<bool> LockAccountAsync(Guid userId, string reason);
    Task<bool> UnlockAccountAsync(Guid userId);
    Task<bool> ChangePasswordAsync(ChangePasswordRequest request);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
    Task<bool> EnableTwoFactorAsync(Guid userId);
    Task<bool> DisableTwoFactorAsync(Guid userId);
    Task<TwoFactorResult> VerifyTwoFactorAsync(VerifyTwoFactorRequest request);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserSessionRepository _sessionRepository;
    private readonly ISecurityAuditRepository _auditRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IMemoryCache _cache;

    public AuthenticationService(
        IUserRepository userRepository,
        IUserSessionRepository sessionRepository,
        ISecurityAuditRepository auditRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ITwoFactorService twoFactorService,
        IEmailService emailService,
        ILogger<AuthenticationService> logger,
        IMemoryCache cache)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _auditRepository = auditRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _twoFactorService = twoFactorService;
        _emailService = emailService;
        _logger = logger;
        _cache = cache;
    }

    public async Task<AuthenticationResult> AuthenticateAsync(LoginRequest request)
    {
        _logger.LogInformation("Authentication attempt for email {Email}", request.Email);

        // Verificar intentos de login fallidos
        var failedAttemptsKey = $"failed_login_{request.Email}";
        var failedAttempts = _cache.Get<int>(failedAttemptsKey);
        
        if (failedAttempts >= 5)
        {
            await CreateSecurityAuditAsync(new SecurityEvent
            {
                Type = SecurityEventType.AccountLocked,
                UserId = null,
                Email = request.Email,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                Description = "Account locked due to multiple failed login attempts"
            });

            throw new SecurityException("Account temporarily locked due to multiple failed attempts");
        }

        // Buscar usuario
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            _cache.Set(failedAttemptsKey, failedAttempts + 1, TimeSpan.FromMinutes(15));
            throw new SecurityException("Invalid credentials");
        }

        // Verificar si la cuenta está bloqueada
        if (await IsAccountLockedAsync(user.Id))
        {
            throw new SecurityException("Account is locked");
        }

        // Verificar contraseña
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            _cache.Set(failedAttemptsKey, failedAttempts + 1, TimeSpan.FromMinutes(15));
            
            await CreateSecurityAuditAsync(new SecurityEvent
            {
                Type = SecurityEventType.LoginFailed,
                UserId = user.Id,
                Email = request.Email,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                Description = "Invalid password"
            });

            throw new SecurityException("Invalid credentials");
        }

        // Limpiar intentos fallidos
        _cache.Remove(failedAttemptsKey);

        // Verificar 2FA si está habilitado
        if (user.TwoFactorEnabled)
        {
            var twoFactorCode = await _twoFactorService.GenerateCodeAsync(user.Id);
            await _emailService.SendTwoFactorCodeAsync(user.Email, twoFactorCode);

            await CreateSecurityAuditAsync(new SecurityEvent
            {
                Type = SecurityEventType.TwoFactorRequired,
                UserId = user.Id,
                Email = request.Email,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                Description = "Two-factor authentication required"
            });

            return new AuthenticationResult
            {
                Success = false,
                RequiresTwoFactor = true,
                UserId = user.Id,
                Message = "Two-factor authentication required"
            };
        }

        // Generar tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken(user);

        // Crear sesión
        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _sessionRepository.AddAsync(session);

        // Actualizar último login
        user.UpdateLastLogin(DateTime.UtcNow, request.IpAddress);
        await _userRepository.UpdateAsync(user);

        // Crear auditoría de seguridad
        await CreateSecurityAuditAsync(new SecurityEvent
        {
            Type = SecurityEventType.LoginSuccess,
            UserId = user.Id,
            Email = request.Email,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            Description = "Successful login"
        });

        _logger.LogInformation("User {UserId} authenticated successfully", user.Id);

        return new AuthenticationResult
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            UserId = user.Id,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
    }

    public async Task<AuthenticationResult> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var session = await _sessionRepository.GetByRefreshTokenAsync(request.RefreshToken);
        if (session == null || !session.IsActive || session.ExpiresAt < DateTime.UtcNow)
        {
            throw new SecurityException("Invalid refresh token");
        }

        var user = await _userRepository.GetByIdAsync(session.UserId);
        if (user == null)
        {
            throw new SecurityException("User not found");
        }

        // Generar nuevos tokens
        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken(user);

        // Actualizar sesión
        session.AccessToken = newAccessToken;
        session.RefreshToken = newRefreshToken;
        session.UpdatedAt = DateTime.UtcNow;
        session.ExpiresAt = DateTime.UtcNow.AddDays(7);

        await _sessionRepository.UpdateAsync(session);

        return new AuthenticationResult
        {
            Success = true,
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            UserId = user.Id,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
    }

    public async Task<bool> LogoutAsync(Guid userId, string token)
    {
        var session = await _sessionRepository.GetByAccessTokenAsync(token);
        if (session != null && session.UserId == userId)
        {
            session.IsActive = false;
            session.LoggedOutAt = DateTime.UtcNow;
            await _sessionRepository.UpdateAsync(session);

            await CreateSecurityAuditAsync(new SecurityEvent
            {
                Type = SecurityEventType.Logout,
                UserId = userId,
                Description = "User logged out"
            });

            _logger.LogInformation("User {UserId} logged out", userId);
        }

        return true;
    }

    public async Task<bool> RevokeTokenAsync(string token)
    {
        var session = await _sessionRepository.GetByAccessTokenAsync(token);
        if (session != null)
        {
            session.IsActive = false;
            session.RevokedAt = DateTime.UtcNow;
            await _sessionRepository.UpdateAsync(session);

            await CreateSecurityAuditAsync(new SecurityEvent
            {
                Type = SecurityEventType.TokenRevoked,
                UserId = session.UserId,
                Description = "Token revoked"
            });

            _logger.LogInformation("Token revoked for user {UserId}", session.UserId);
        }

        return true;
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var session = await _sessionRepository.GetByAccessTokenAsync(token);
            return session != null && session.IsActive && session.ExpiresAt > DateTime.UtcNow;
        }
        catch
        {
            return false;
        }
    }

    public async Task<SecurityAudit> CreateSecurityAuditAsync(SecurityEvent securityEvent)
    {
        var audit = new SecurityAudit
        {
            Id = Guid.NewGuid(),
            Type = securityEvent.Type,
            UserId = securityEvent.UserId,
            Email = securityEvent.Email,
            IpAddress = securityEvent.IpAddress,
            UserAgent = securityEvent.UserAgent,
            Description = securityEvent.Description,
            Metadata = securityEvent.Metadata ?? new Dictionary<string, string>(),
            CreatedAt = DateTime.UtcNow
        };

        await _auditRepository.AddAsync(audit);

        // Si es un evento crítico, enviar alerta
        if (IsCriticalEvent(securityEvent.Type))
        {
            await _emailService.SendSecurityAlertAsync(securityEvent);
        }

        return audit;
    }

    public async Task<bool> IsAccountLockedAsync(Guid userId)
    {
        var lockKey = $"account_locked_{userId}";
        return _cache.Get<bool>(lockKey);
    }

    public async Task<bool> LockAccountAsync(Guid userId, string reason)
    {
        var lockKey = $"account_locked_{userId}";
        _cache.Set(lockKey, true, TimeSpan.FromHours(24));

        await CreateSecurityAuditAsync(new SecurityEvent
        {
            Type = SecurityEventType.AccountLocked,
            UserId = userId,
            Description = $"Account locked: {reason}"
        });

        _logger.LogWarning("Account {UserId} locked: {Reason}", userId, reason);

        return true;
    }

    public async Task<bool> UnlockAccountAsync(Guid userId)
    {
        var lockKey = $"account_locked_{userId}";
        _cache.Remove(lockKey);

        await CreateSecurityAuditAsync(new SecurityEvent
        {
            Type = SecurityEventType.AccountUnlocked,
            UserId = userId,
            Description = "Account unlocked"
        });

        _logger.LogInformation("Account {UserId} unlocked", userId);

        return true;
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            throw new NotFoundException("User not found");

        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            throw new SecurityException("Current password is incorrect");

        var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.UpdatePassword(newPasswordHash);
        await _userRepository.UpdateAsync(user);

        // Revocar todas las sesiones activas
        await _sessionRepository.RevokeAllUserSessionsAsync(request.UserId);

        await CreateSecurityAuditAsync(new SecurityEvent
        {
            Type = SecurityEventType.PasswordChanged,
            UserId = request.UserId,
            Description = "Password changed"
        });

        _logger.LogInformation("Password changed for user {UserId}", request.UserId);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            throw new NotFoundException("User not found");

        var resetToken = _tokenService.GeneratePasswordResetToken(user);
        await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken);

        await CreateSecurityAuditAsync(new SecurityEvent
        {
            Type = SecurityEventType.PasswordResetRequested,
            UserId = user.Id,
            Email = request.Email,
            Description = "Password reset requested"
        });

        _logger.LogInformation("Password reset requested for user {UserId}", user.Id);

        return true;
    }

    public async Task<bool> EnableTwoFactorAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found");

        user.EnableTwoFactor();
        await _userRepository.UpdateAsync(user);

        await CreateSecurityAuditAsync(new SecurityEvent
        {
            Type = SecurityEventType.TwoFactorEnabled,
            UserId = userId,
            Description = "Two-factor authentication enabled"
        });

        _logger.LogInformation("Two-factor authentication enabled for user {UserId}", userId);

        return true;
    }

    public async Task<bool> DisableTwoFactorAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found");

        user.DisableTwoFactor();
        await _userRepository.UpdateAsync(user);

        await CreateSecurityAuditAsync(new SecurityEvent
        {
            Type = SecurityEventType.TwoFactorDisabled,
            UserId = userId,
            Description = "Two-factor authentication disabled"
        });

        _logger.LogInformation("Two-factor authentication disabled for user {UserId}", userId);

        return true;
    }

    public async Task<TwoFactorResult> VerifyTwoFactorAsync(VerifyTwoFactorRequest request)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
            throw new NotFoundException("User not found");

        var isValid = await _twoFactorService.VerifyCodeAsync(request.UserId, request.Code);
        if (!isValid)
        {
            await CreateSecurityAuditAsync(new SecurityEvent
            {
                Type = SecurityEventType.TwoFactorFailed,
                UserId = request.UserId,
                Description = "Two-factor authentication failed"
            });

            throw new SecurityException("Invalid two-factor code");
        }

        // Generar tokens después de 2FA exitoso
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken(user);

        // Crear sesión
        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _sessionRepository.AddAsync(session);

        await CreateSecurityAuditAsync(new SecurityEvent
        {
            Type = SecurityEventType.TwoFactorSuccess,
            UserId = request.UserId,
            Description = "Two-factor authentication successful"
        });

        return new TwoFactorResult
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            UserId = user.Id,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
    }

    public async Task<List<SecurityAudit>> GetSecurityAuditsAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
        var to = toDate ?? DateTime.UtcNow;

        return await _auditRepository.GetUserAuditsAsync(userId, from, to);
    }

    private bool IsCriticalEvent(SecurityEventType eventType)
    {
        return eventType switch
        {
            SecurityEventType.AccountLocked => true,
            SecurityEventType.MultipleFailedLogins => true,
            SecurityEventType.SuspiciousActivity => true,
            SecurityEventType.DataBreach => true,
            _ => false
        };
    }
}
```

---

## 🛡️ **Sistema de Autorización**

### **Servicio de Autorización**

```csharp
// MusicalMatching.Application/Services/IAuthorizationService.cs
namespace MusicalMatching.Application.Services;

public interface IAuthorizationService
{
    Task<bool> HasPermissionAsync(Guid userId, string permission);
    Task<bool> HasRoleAsync(Guid userId, string role);
    Task<bool> CanAccessResourceAsync(Guid userId, string resource, string action);
    Task<List<Permission>> GetUserPermissionsAsync(Guid userId);
    Task<List<Role>> GetUserRolesAsync(Guid userId);
    Task<bool> AssignRoleAsync(Guid userId, string role);
    Task<bool> RemoveRoleAsync(Guid userId, string role);
    Task<bool> GrantPermissionAsync(Guid userId, string permission);
    Task<bool> RevokePermissionAsync(Guid userId, string permission);
    Task<AuthorizationResult> AuthorizeAsync(AuthorizationRequest request);
}

public class AuthorizationService : IAuthorizationService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IUserPermissionRepository _userPermissionRepository;
    private readonly ILogger<AuthorizationService> _logger;

    public AuthorizationService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IUserRoleRepository userRoleRepository,
        IUserPermissionRepository userPermissionRepository,
        ILogger<AuthorizationService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _userRoleRepository = userRoleRepository;
        _userPermissionRepository = userPermissionRepository;
        _logger = logger;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permission)
    {
        // Verificar permisos directos
        var hasDirectPermission = await _userPermissionRepository.HasPermissionAsync(userId, permission);
        if (hasDirectPermission)
            return true;

        // Verificar permisos a través de roles
        var userRoles = await _userRoleRepository.GetUserRolesAsync(userId);
        foreach (var role in userRoles)
        {
            var hasRolePermission = await _roleRepository.HasPermissionAsync(role.Id, permission);
            if (hasRolePermission)
                return true;
        }

        return false;
    }

    public async Task<bool> HasRoleAsync(Guid userId, string role)
    {
        return await _userRoleRepository.HasRoleAsync(userId, role);
    }

    public async Task<bool> CanAccessResourceAsync(Guid userId, string resource, string action)
    {
        var permission = $"{resource}:{action}";
        return await HasPermissionAsync(userId, permission);
    }

    public async Task<List<Permission>> GetUserPermissionsAsync(Guid userId)
    {
        var permissions = new List<Permission>();

        // Obtener permisos directos
        var directPermissions = await _userPermissionRepository.GetUserPermissionsAsync(userId);
        permissions.AddRange(directPermissions);

        // Obtener permisos a través de roles
        var userRoles = await _userRoleRepository.GetUserRolesAsync(userId);
        foreach (var role in userRoles)
        {
            var rolePermissions = await _roleRepository.GetRolePermissionsAsync(role.Id);
            permissions.AddRange(rolePermissions);
        }

        return permissions.Distinct().ToList();
    }

    public async Task<List<Role>> GetUserRolesAsync(Guid userId)
    {
        return await _userRoleRepository.GetUserRolesAsync(userId);
    }

    public async Task<bool> AssignRoleAsync(Guid userId, string role)
    {
        var roleEntity = await _roleRepository.GetByNameAsync(role);
        if (roleEntity == null)
            throw new NotFoundException("Role not found");

        var userRole = new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoleId = roleEntity.Id,
            AssignedAt = DateTime.UtcNow
        };

        await _userRoleRepository.AddAsync(userRole);

        _logger.LogInformation("Role {Role} assigned to user {UserId}", role, userId);

        return true;
    }

    public async Task<bool> RemoveRoleAsync(Guid userId, string role)
    {
        var roleEntity = await _roleRepository.GetByNameAsync(role);
        if (roleEntity == null)
            throw new NotFoundException("Role not found");

        await _userRoleRepository.RemoveAsync(userId, roleEntity.Id);

        _logger.LogInformation("Role {Role} removed from user {UserId}", role, userId);

        return true;
    }

    public async Task<bool> GrantPermissionAsync(Guid userId, string permission)
    {
        var permissionEntity = await _permissionRepository.GetByNameAsync(permission);
        if (permissionEntity == null)
            throw new NotFoundException("Permission not found");

        var userPermission = new UserPermission
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PermissionId = permissionEntity.Id,
            GrantedAt = DateTime.UtcNow
        };

        await _userPermissionRepository.AddAsync(userPermission);

        _logger.LogInformation("Permission {Permission} granted to user {UserId}", permission, userId);

        return true;
    }

    public async Task<bool> RevokePermissionAsync(Guid userId, string permission)
    {
        var permissionEntity = await _permissionRepository.GetByNameAsync(permission);
        if (permissionEntity == null)
            throw new NotFoundException("Permission not found");

        await _userPermissionRepository.RemoveAsync(userId, permissionEntity.Id);

        _logger.LogInformation("Permission {Permission} revoked from user {UserId}", permission, userId);

        return true;
    }

    public async Task<AuthorizationResult> AuthorizeAsync(AuthorizationRequest request)
    {
        var result = new AuthorizationResult
        {
            UserId = request.UserId,
            Resource = request.Resource,
            Action = request.Action,
            IsAuthorized = false,
            Reason = "Not authorized"
        };

        // Verificar si el usuario existe
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            result.Reason = "User not found";
            return result;
        }

        // Verificar si el usuario está activo
        if (!user.IsActive)
        {
            result.Reason = "User account is inactive";
            return result;
        }

        // Verificar permisos
        var permission = $"{request.Resource}:{request.Action}";
        var hasPermission = await HasPermissionAsync(request.UserId, permission);

        if (hasPermission)
        {
            result.IsAuthorized = true;
            result.Reason = "Authorized";
        }
        else
        {
            result.Reason = "Insufficient permissions";
        }

        return result;
    }
}
```

---

## 🔐 **Entidades de Seguridad**

### **SecurityAudit, UserSession y Permisos**

```csharp
// MusicalMatching.Domain/Entities/SecurityAudit.cs
namespace MusicalMatching.Domain.Entities;

public class SecurityAudit : BaseEntity
{
    public SecurityEventType Type { get; private set; }
    public Guid? UserId { get; private set; }
    public virtual User? User { get; private set; }
    public string? Email { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string Description { get; private set; }
    public Dictionary<string, string> Metadata { get; private set; } = new();
    public DateTime CreatedAt { get; private set; }

    private SecurityAudit() { }

    public SecurityAudit(
        SecurityEventType type, string description,
        Guid? userId = null, string? email = null,
        string? ipAddress = null, string? userAgent = null)
    {
        Type = type;
        Description = description ?? throw new ArgumentNullException(nameof(description));
        UserId = userId;
        Email = email;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddMetadata(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be empty");

        Metadata[key] = value;
    }
}

// MusicalMatching.Domain/Entities/UserSession.cs
public class UserSession : BaseEntity
{
    public Guid UserId { get; private set; }
    public virtual User User { get; private set; }
    public string AccessToken { get; private set; }
    public string RefreshToken { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? LoggedOutAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    private UserSession() { }

    public UserSession(
        Guid userId, string accessToken, string refreshToken,
        DateTime expiresAt, string? ipAddress = null, string? userAgent = null)
    {
        UserId = userId;
        AccessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        RefreshToken = refreshToken ?? throw new ArgumentNullException(nameof(refreshToken));
        ExpiresAt = expiresAt;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateTokens(string accessToken, string refreshToken, DateTime expiresAt)
    {
        AccessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        RefreshToken = refreshToken ?? throw new ArgumentNullException(nameof(refreshToken));
        ExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Logout()
    {
        IsActive = false;
        LoggedOutAt = DateTime.UtcNow;
    }

    public void Revoke()
    {
        IsActive = false;
        RevokedAt = DateTime.UtcNow;
    }

    public bool IsExpired => ExpiresAt < DateTime.UtcNow;
    public bool IsValid => IsActive && !IsExpired;
}

// MusicalMatching.Domain/Entities/Role.cs
public class Role : BaseEntity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public bool IsSystemRole { get; private set; }
    public List<RolePermission> RolePermissions { get; private set; } = new();
    public List<UserRole> UserRoles { get; private set; } = new();

    private Role() { }

    public Role(string name, string description, bool isSystemRole = false)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        IsSystemRole = isSystemRole;
    }

    public void AddPermission(Permission permission)
    {
        if (permission == null)
            throw new ArgumentNullException(nameof(permission));

        if (!RolePermissions.Any(rp => rp.PermissionId == permission.Id))
        {
            RolePermissions.Add(new RolePermission
            {
                RoleId = Id,
                PermissionId = permission.Id
            });
        }
    }

    public void RemovePermission(Permission permission)
    {
        if (permission == null)
            throw new ArgumentNullException(nameof(permission));

        var rolePermission = RolePermissions.FirstOrDefault(rp => rp.PermissionId == permission.Id);
        if (rolePermission != null)
        {
            RolePermissions.Remove(rolePermission);
        }
    }
}

// MusicalMatching.Domain/Entities/Permission.cs
public class Permission : BaseEntity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Resource { get; private set; }
    public string Action { get; private set; }
    public List<RolePermission> RolePermissions { get; private set; } = new();
    public List<UserPermission> UserPermissions { get; private set; } = new();

    private Permission() { }

    public Permission(string name, string description, string resource, string action)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        Action = action ?? throw new ArgumentNullException(nameof(action));
    }
}

// MusicalMatching.Domain/Entities/UserRole.cs
public class UserRole : BaseEntity
{
    public Guid UserId { get; private set; }
    public virtual User User { get; private set; }
    public Guid RoleId { get; private set; }
    public virtual Role Role { get; private set; }
    public DateTime AssignedAt { get; private set; }
    public Guid? AssignedBy { get; private set; }
    public virtual User? AssignedByUser { get; private set; }

    private UserRole() { }

    public UserRole(Guid userId, Guid roleId, Guid? assignedBy = null)
    {
        UserId = userId;
        RoleId = roleId;
        AssignedBy = assignedBy;
        AssignedAt = DateTime.UtcNow;
    }
}

// MusicalMatching.Domain/Entities/UserPermission.cs
public class UserPermission : BaseEntity
{
    public Guid UserId { get; private set; }
    public virtual User User { get; private set; }
    public Guid PermissionId { get; private set; }
    public virtual Permission Permission { get; private set; }
    public DateTime GrantedAt { get; private set; }
    public Guid? GrantedBy { get; private set; }
    public virtual User? GrantedByUser { get; private set; }

    private UserPermission() { }

    public UserPermission(Guid userId, Guid permissionId, Guid? grantedBy = null)
    {
        UserId = userId;
        PermissionId = permissionId;
        GrantedBy = grantedBy;
        GrantedAt = DateTime.UtcNow;
    }
}

// MusicalMatching.Domain/Entities/RolePermission.cs
public class RolePermission : BaseEntity
{
    public Guid RoleId { get; private set; }
    public virtual Role Role { get; private set; }
    public Guid PermissionId { get; private set; }
    public virtual Permission Permission { get; private set; }

    private RolePermission() { }

    public RolePermission(Guid roleId, Guid permissionId)
    {
        RoleId = roleId;
        PermissionId = permissionId;
    }
}

// MusicalMatching.Domain/Enums/SecurityEnums.cs
public enum SecurityEventType
{
    LoginSuccess = 0,
    LoginFailed = 1,
    Logout = 2,
    PasswordChanged = 3,
    PasswordResetRequested = 4,
    PasswordResetCompleted = 5,
    TwoFactorEnabled = 6,
    TwoFactorDisabled = 7,
    TwoFactorRequired = 8,
    TwoFactorSuccess = 9,
    TwoFactorFailed = 10,
    AccountLocked = 11,
    AccountUnlocked = 12,
    TokenRevoked = 13,
    SuspiciousActivity = 14,
    MultipleFailedLogins = 15,
    DataBreach = 16,
    UnauthorizedAccess = 17
}

// MusicalMatching.Domain/ValueObjects/SecurityModels.cs
public class AuthenticationResult
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool RequiresTwoFactor { get; set; }
    public string? Message { get; set; }
}

public class TwoFactorResult
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Message { get; set; }
}

public class AuthorizationResult
{
    public Guid UserId { get; set; }
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public bool IsAuthorized { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class SecurityEvent
{
    public SecurityEventType Type { get; set; }
    public Guid? UserId { get; set; }
    public string? Email { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; set; }
}
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Sistema de Autenticación**
```csharp
// Implementa:
// - Login con 2FA
// - Gestión de sesiones
// - Auditoría de seguridad
// - Bloqueo de cuentas
```

### **Ejercicio 2: Sistema de Autorización**
```csharp
// Crea:
// - Roles y permisos
// - Autorización granular
// - Middleware de seguridad
// - Políticas de acceso
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **🔒 Sistema de Autenticación**: Login seguro con 2FA
2. **🛡️ Sistema de Autorización**: Roles y permisos granulares
3. **📊 Auditoría de Seguridad**: Registro de eventos de seguridad
4. **🔐 Gestión de Sesiones**: Tokens y refresh tokens
5. **🚨 Protección contra Ataques**: Bloqueo y monitoreo

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Sistema de Monitoreo y Logging**, implementando observabilidad completa.

---

**¡Has completado la novena clase del Módulo 16! 🔒🎯**
