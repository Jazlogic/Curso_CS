# 🔐 **Clase 3: Authentication y Authorization Avanzado**

## 🎯 **Objetivo de la Clase**
Dominar los sistemas avanzados de autenticación y autorización, incluyendo OAuth2, OpenID Connect, JWT, y la implementación de sistemas de autenticación robustos y escalables.

## 📚 **Contenido de la Clase**

### **1. OAuth2 y OpenID Connect**

#### **1.1 Implementación de OAuth2 Server**
```csharp
// Servidor OAuth2 personalizado
public class OAuth2Server
{
    private readonly IClientRepository _clientRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly ILogger<OAuth2Server> _logger;
    
    public OAuth2Server(
        IClientRepository clientRepository,
        IUserRepository userRepository,
        ITokenService tokenService,
        ILogger<OAuth2Server> logger)
    {
        _clientRepository = clientRepository;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _logger = logger;
    }
    
    // Authorization Code Grant
    public async Task<AuthorizationCodeResult> GenerateAuthorizationCode(
        string clientId, string redirectUri, string scope, string state)
    {
        try
        {
            // 1. Validar cliente
            var client = await _clientRepository.GetByIdAsync(clientId);
            if (client == null || !client.IsActive)
            {
                throw new InvalidClientException("Invalid or inactive client");
            }
            
            // 2. Validar redirect URI
            if (!client.RedirectUris.Contains(redirectUri))
            {
                throw new InvalidRedirectUriException("Invalid redirect URI");
            }
            
            // 3. Generar código de autorización
            var authCode = GenerateSecureCode();
            var expiresAt = DateTime.UtcNow.AddMinutes(10); // Código expira en 10 minutos
            
            // 4. Almacenar código temporalmente
            await StoreAuthorizationCode(authCode, clientId, redirectUri, scope, expiresAt);
            
            _logger.LogInformation("Authorization code generated for client {ClientId}", clientId);
            
            return new AuthorizationCodeResult
            {
                Code = authCode,
                State = state,
                ExpiresAt = expiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating authorization code for client {ClientId}", clientId);
            throw;
        }
    }
    
    // Exchange Authorization Code for Access Token
    public async Task<TokenResult> ExchangeCodeForToken(
        string code, string clientId, string clientSecret, string redirectUri)
    {
        try
        {
            // 1. Validar cliente
            var client = await _clientRepository.GetByIdAsync(clientId);
            if (client == null || client.Secret != clientSecret)
            {
                throw new InvalidClientException("Invalid client credentials");
            }
            
            // 2. Validar código de autorización
            var authCode = await GetAuthorizationCode(code);
            if (authCode == null || authCode.ExpiresAt < DateTime.UtcNow)
            {
                throw new InvalidAuthorizationCodeException("Invalid or expired authorization code");
            }
            
            // 3. Verificar redirect URI
            if (authCode.RedirectUri != redirectUri)
            {
                throw new InvalidRedirectUriException("Redirect URI mismatch");
            }
            
            // 4. Generar tokens
            var accessToken = await _tokenService.GenerateAccessTokenAsync(
                authCode.UserId, authCode.Scope, clientId);
            
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(
                authCode.UserId, clientId);
            
            // 5. Limpiar código de autorización usado
            await RemoveAuthorizationCode(code);
            
            _logger.LogInformation("Tokens generated for user {UserId} via client {ClientId}", 
                authCode.UserId, clientId);
            
            return new TokenResult
            {
                AccessToken = accessToken.Token,
                TokenType = "Bearer",
                ExpiresIn = accessToken.ExpiresIn,
                RefreshToken = refreshToken.Token,
                Scope = authCode.Scope
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging code for token");
            throw;
        }
    }
    
    // Refresh Token Grant
    public async Task<TokenResult> RefreshAccessToken(string refreshToken, string clientId)
    {
        try
        {
            // 1. Validar refresh token
            var token = await _tokenService.ValidateRefreshTokenAsync(refreshToken);
            if (token == null || token.ClientId != clientId)
            {
                throw new InvalidRefreshTokenException("Invalid refresh token");
            }
            
            // 2. Verificar si el token no ha expirado
            if (token.ExpiresAt < DateTime.UtcNow)
            {
                throw new ExpiredRefreshTokenException("Refresh token has expired");
            }
            
            // 3. Generar nuevo access token
            var newAccessToken = await _tokenService.GenerateAccessTokenAsync(
                token.UserId, token.Scope, clientId);
            
            // 4. Opcionalmente, rotar refresh token
            var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(
                token.UserId, clientId);
            
            // 5. Invalidar refresh token anterior
            await _tokenService.RevokeRefreshTokenAsync(refreshToken);
            
            _logger.LogInformation("Access token refreshed for user {UserId}", token.UserId);
            
            return new TokenResult
            {
                AccessToken = newAccessToken.Token,
                TokenType = "Bearer",
                ExpiresIn = newAccessToken.ExpiresIn,
                RefreshToken = newRefreshToken.Token,
                Scope = token.Scope
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing access token");
            throw;
        }
    }
    
    private string GenerateSecureCode()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
    
    private async Task StoreAuthorizationCode(string code, string clientId, string redirectUri, string scope, DateTime expiresAt)
    {
        // Implementar almacenamiento temporal del código
        // Podría usar Redis, base de datos, etc.
    }
    
    private async Task<AuthorizationCode> GetAuthorizationCode(string code)
    {
        // Implementar obtención del código de autorización
        return new AuthorizationCode(); // Simplificado
    }
    
    private async Task RemoveAuthorizationCode(string code)
    {
        // Implementar eliminación del código usado
    }
}

// Modelos para OAuth2
public class AuthorizationCode
{
    public string Code { get; set; }
    public string ClientId { get; set; }
    public string UserId { get; set; }
    public string RedirectUri { get; set; }
    public string Scope { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class AuthorizationCodeResult
{
    public string Code { get; set; }
    public string State { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class TokenResult
{
    public string AccessToken { get; set; }
    public string TokenType { get; set; }
    public int ExpiresIn { get; set; }
    public string RefreshToken { get; set; }
    public string Scope { get; set; }
}
```

#### **1.2 OpenID Connect Implementation**
```csharp
// Implementación de OpenID Connect
public class OpenIdConnectService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly ILogger<OpenIdConnectService> _logger;
    
    public OpenIdConnectService(
        IUserRepository userRepository,
        ITokenService tokenService,
        ILogger<OpenIdConnectService> logger)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _logger = logger;
    }
    
    // Generar ID Token
    public async Task<string> GenerateIdTokenAsync(string userId, string clientId, string nonce)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException($"User {userId} not found");
            }
            
            var claims = new List<Claim>
            {
                new("iss", "https://your-identity-provider.com"), // Issuer
                new("sub", user.Id), // Subject (user ID)
                new("aud", clientId), // Audience (client ID)
                new("exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds().ToString()), // Expiration
                new("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()), // Issued at
                new("nonce", nonce), // Nonce
                new("email", user.Email),
                new("name", user.Name),
                new("given_name", user.FirstName),
                new("family_name", user.LastName),
                new("email_verified", user.EmailVerified.ToString().ToLower())
            };
            
            var idToken = await _tokenService.GenerateJwtTokenAsync(claims);
            
            _logger.LogInformation("ID token generated for user {UserId}", userId);
            
            return idToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating ID token for user {UserId}", userId);
            throw;
        }
    }
    
    // Validar ID Token
    public async Task<IdTokenValidationResult> ValidateIdTokenAsync(string idToken, string clientId)
    {
        try
        {
            var token = await _tokenService.ValidateJwtTokenAsync(idToken);
            
            if (token == null)
            {
                return new IdTokenValidationResult { IsValid = false, Error = "Invalid token" };
            }
            
            // Verificar issuer
            var issuer = token.Claims.FirstOrDefault(c => c.Type == "iss")?.Value;
            if (issuer != "https://your-identity-provider.com")
            {
                return new IdTokenValidationResult { IsValid = false, Error = "Invalid issuer" };
            }
            
            // Verificar audience
            var audience = token.Claims.FirstOrDefault(c => c.Type == "aud")?.Value;
            if (audience != clientId)
            {
                return new IdTokenValidationResult { IsValid = false, Error = "Invalid audience" };
            }
            
            // Verificar expiración
            var exp = token.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
            if (exp != null && long.TryParse(exp, out var expUnix))
            {
                var expDateTime = DateTimeOffset.FromUnixTimeSeconds(expUnix);
                if (expDateTime < DateTimeOffset.UtcNow)
                {
                    return new IdTokenValidationResult { IsValid = false, Error = "Token expired" };
                }
            }
            
            _logger.LogInformation("ID token validated successfully");
            
            return new IdTokenValidationResult
            {
                IsValid = true,
                Claims = token.Claims.ToDictionary(c => c.Type, c => c.Value)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating ID token");
            return new IdTokenValidationResult { IsValid = false, Error = ex.Message };
        }
    }
    
    // UserInfo Endpoint
    public async Task<UserInfoResult> GetUserInfoAsync(string accessToken)
    {
        try
        {
            var token = await _tokenService.ValidateAccessTokenAsync(accessToken);
            if (token == null)
            {
                throw new InvalidAccessTokenException("Invalid access token");
            }
            
            var user = await _userRepository.GetByIdAsync(token.UserId);
            if (user == null)
            {
                throw new UserNotFoundException($"User {token.UserId} not found");
            }
            
            var userInfo = new UserInfoResult
            {
                Sub = user.Id,
                Email = user.Email,
                Name = user.Name,
                GivenName = user.FirstName,
                FamilyName = user.LastName,
                EmailVerified = user.EmailVerified,
                Picture = user.ProfilePictureUrl
            };
            
            _logger.LogInformation("User info retrieved for user {UserId}", user.Id);
            
            return userInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user info");
            throw;
        }
    }
}

// Modelos para OpenID Connect
public class IdTokenValidationResult
{
    public bool IsValid { get; set; }
    public string Error { get; set; }
    public Dictionary<string, string> Claims { get; set; } = new();
}

public class UserInfoResult
{
    public string Sub { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string GivenName { get; set; }
    public string FamilyName { get; set; }
    public bool EmailVerified { get; set; }
    public string Picture { get; set; }
}
```

### **2. JWT (JSON Web Tokens) Avanzado**

#### **2.1 Implementación de JWT Service**
```csharp
// Servicio avanzado de JWT
public class JwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtTokenService> _logger;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    
    public JwtTokenService(IConfiguration configuration, ILogger<JwtTokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _secretKey = _configuration["Jwt:SecretKey"];
        _issuer = _configuration["Jwt:Issuer"];
        _audience = _configuration["Jwt:Audience"];
    }
    
    // Generar Access Token
    public async Task<AccessToken> GenerateAccessTokenAsync(string userId, string scope, string clientId)
    {
        try
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new("scope", scope),
                new("client_id", clientId),
                new("token_type", "access"),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };
            
            var expiresAt = DateTime.UtcNow.AddHours(1);
            var token = GenerateJwtToken(claims, expiresAt);
            
            _logger.LogInformation("Access token generated for user {UserId}", userId);
            
            return new AccessToken
            {
                Token = token,
                ExpiresAt = expiresAt,
                ExpiresIn = (int)(expiresAt - DateTime.UtcNow).TotalSeconds,
                TokenType = "Bearer",
                Scope = scope
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating access token for user {UserId}", userId);
            throw;
        }
    }
    
    // Generar Refresh Token
    public async Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string clientId)
    {
        try
        {
            var token = GenerateSecureToken(64);
            var expiresAt = DateTime.UtcNow.AddDays(30);
            
            // Almacenar refresh token en base de datos
            await StoreRefreshToken(token, userId, clientId, expiresAt);
            
            _logger.LogInformation("Refresh token generated for user {UserId}", userId);
            
            return new RefreshToken
            {
                Token = token,
                ExpiresAt = expiresAt,
                UserId = userId,
                ClientId = clientId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating refresh token for user {UserId}", userId);
            throw;
        }
    }
    
    // Validar Access Token
    public async Task<AccessTokenValidationResult> ValidateAccessTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);
            
            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            var scope = jwtToken.Claims.First(x => x.Type == "scope").Value;
            var clientId = jwtToken.Claims.First(x => x.Type == "client_id").Value;
            
            _logger.LogInformation("Access token validated for user {UserId}", userId);
            
            return new AccessTokenValidationResult
            {
                IsValid = true,
                UserId = userId,
                Scope = scope,
                ClientId = clientId,
                Claims = jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating access token");
            return new AccessTokenValidationResult { IsValid = false, Error = ex.Message };
        }
    }
    
    // Validar Refresh Token
    public async Task<RefreshTokenValidationResult> ValidateRefreshTokenAsync(string token)
    {
        try
        {
            var refreshToken = await GetRefreshToken(token);
            
            if (refreshToken == null)
            {
                return new RefreshTokenValidationResult { IsValid = false, Error = "Token not found" };
            }
            
            if (refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                return new RefreshTokenValidationResult { IsValid = false, Error = "Token expired" };
            }
            
            if (refreshToken.IsRevoked)
            {
                return new RefreshTokenValidationResult { IsValid = false, Error = "Token revoked" };
            }
            
            _logger.LogInformation("Refresh token validated for user {UserId}", refreshToken.UserId);
            
            return new RefreshTokenValidationResult
            {
                IsValid = true,
                UserId = refreshToken.UserId,
                ClientId = refreshToken.ClientId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating refresh token");
            return new RefreshTokenValidationResult { IsValid = false, Error = ex.Message };
        }
    }
    
    // Revocar Refresh Token
    public async Task RevokeRefreshTokenAsync(string token)
    {
        try
        {
            await MarkRefreshTokenAsRevoked(token);
            _logger.LogInformation("Refresh token revoked");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking refresh token");
            throw;
        }
    }
    
    private string GenerateJwtToken(List<Claim> claims, DateTime expiresAt)
    {
        var key = Encoding.ASCII.GetBytes(_secretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    
    private string GenerateSecureToken(int length)
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
    
    private async Task StoreRefreshToken(string token, string userId, string clientId, DateTime expiresAt)
    {
        // Implementar almacenamiento del refresh token
    }
    
    private async Task<RefreshToken> GetRefreshToken(string token)
    {
        // Implementar obtención del refresh token
        return new RefreshToken(); // Simplificado
    }
    
    private async Task MarkRefreshTokenAsRevoked(string token)
    {
        // Implementar marcado del refresh token como revocado
    }
}

// Modelos para JWT
public class AccessToken
{
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; }
    public string Scope { get; set; }
}

public class RefreshToken
{
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string UserId { get; set; }
    public string ClientId { get; set; }
    public bool IsRevoked { get; set; }
}

public class AccessTokenValidationResult
{
    public bool IsValid { get; set; }
    public string Error { get; set; }
    public string UserId { get; set; }
    public string Scope { get; set; }
    public string ClientId { get; set; }
    public Dictionary<string, string> Claims { get; set; } = new();
}

public class RefreshTokenValidationResult
{
    public bool IsValid { get; set; }
    public string Error { get; set; }
    public string UserId { get; set; }
    public string ClientId { get; set; }
}
```

### **3. Multi-Factor Authentication (MFA)**

#### **3.1 Implementación de MFA**
```csharp
// Servicio de Multi-Factor Authentication
public class MfaService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly ILogger<MfaService> _logger;
    
    public MfaService(
        IUserRepository userRepository,
        IEmailService emailService,
        ISmsService smsService,
        ILogger<MfaService> logger)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _smsService = smsService;
        _logger = logger;
    }
    
    // Configurar MFA para usuario
    public async Task<MfaSetupResult> SetupMfaAsync(string userId, MfaType type)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException($"User {userId} not found");
            }
            
            switch (type)
            {
                case MfaType.Totp:
                    return await SetupTotpMfaAsync(user);
                case MfaType.Email:
                    return await SetupEmailMfaAsync(user);
                case MfaType.Sms:
                    return await SetupSmsMfaAsync(user);
                default:
                    throw new ArgumentException($"Unsupported MFA type: {type}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up MFA for user {UserId}", userId);
            throw;
        }
    }
    
    // Configurar TOTP (Time-based One-Time Password)
    private async Task<MfaSetupResult> SetupTotpMfaAsync(User user)
    {
        var secretKey = GenerateSecretKey();
        var qrCodeUrl = GenerateQrCodeUrl(user.Email, secretKey);
        
        // Almacenar secret key de forma segura
        await StoreMfaSecret(user.Id, secretKey, MfaType.Totp);
        
        _logger.LogInformation("TOTP MFA setup for user {UserId}", user.Id);
        
        return new MfaSetupResult
        {
            Type = MfaType.Totp,
            SecretKey = secretKey,
            QrCodeUrl = qrCodeUrl,
            SetupComplete = false
        };
    }
    
    // Configurar Email MFA
    private async Task<MfaSetupResult> SetupEmailMfaAsync(User user)
    {
        // Verificar que el usuario tenga email verificado
        if (!user.EmailVerified)
        {
            throw new InvalidOperationException("Email must be verified to use email MFA");
        }
        
        await StoreMfaSecret(user.Id, user.Email, MfaType.Email);
        
        _logger.LogInformation("Email MFA setup for user {UserId}", user.Id);
        
        return new MfaSetupResult
        {
            Type = MfaType.Email,
            SetupComplete = true
        };
    }
    
    // Configurar SMS MFA
    private async Task<MfaSetupResult> SetupSmsMfaAsync(User user)
    {
        // Verificar que el usuario tenga número de teléfono
        if (string.IsNullOrEmpty(user.PhoneNumber))
        {
            throw new InvalidOperationException("Phone number is required for SMS MFA");
        }
        
        await StoreMfaSecret(user.Id, user.PhoneNumber, MfaType.Sms);
        
        _logger.LogInformation("SMS MFA setup for user {UserId}", user.Id);
        
        return new MfaSetupResult
        {
            Type = MfaType.Sms,
            SetupComplete = true
        };
    }
    
    // Verificar código MFA
    public async Task<bool> VerifyMfaCodeAsync(string userId, string code, MfaType type)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException($"User {userId} not found");
            }
            
            switch (type)
            {
                case MfaType.Totp:
                    return await VerifyTotpCodeAsync(userId, code);
                case MfaType.Email:
                    return await VerifyEmailCodeAsync(userId, code);
                case MfaType.Sms:
                    return await VerifySmsCodeAsync(userId, code);
                default:
                    throw new ArgumentException($"Unsupported MFA type: {type}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying MFA code for user {UserId}", userId);
            return false;
        }
    }
    
    // Verificar código TOTP
    private async Task<bool> VerifyTotpCodeAsync(string userId, string code)
    {
        var secretKey = await GetMfaSecret(userId, MfaType.Totp);
        if (string.IsNullOrEmpty(secretKey))
        {
            return false;
        }
        
        var totp = new Totp(Encoding.UTF8.GetBytes(secretKey));
        var isValid = totp.VerifyTotp(code, out var timeStepMatched, VerificationWindow.RfcSpecifiedWindow);
        
        if (isValid)
        {
            _logger.LogInformation("TOTP code verified for user {UserId}", userId);
        }
        
        return isValid;
    }
    
    // Verificar código Email
    private async Task<bool> VerifyEmailCodeAsync(string userId, string code)
    {
        var storedCode = await GetStoredMfaCode(userId, MfaType.Email);
        if (storedCode == null || storedCode.ExpiresAt < DateTime.UtcNow)
        {
            return false;
        }
        
        var isValid = storedCode.Code == code;
        
        if (isValid)
        {
            await RemoveStoredMfaCode(userId, MfaType.Email);
            _logger.LogInformation("Email MFA code verified for user {UserId}", userId);
        }
        
        return isValid;
    }
    
    // Verificar código SMS
    private async Task<bool> VerifySmsCodeAsync(string userId, string code)
    {
        var storedCode = await GetStoredMfaCode(userId, MfaType.Sms);
        if (storedCode == null || storedCode.ExpiresAt < DateTime.UtcNow)
        {
            return false;
        }
        
        var isValid = storedCode.Code == code;
        
        if (isValid)
        {
            await RemoveStoredMfaCode(userId, MfaType.Sms);
            _logger.LogInformation("SMS MFA code verified for user {UserId}", userId);
        }
        
        return isValid;
    }
    
    // Enviar código MFA
    public async Task SendMfaCodeAsync(string userId, MfaType type)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException($"User {userId} not found");
            }
            
            var code = GenerateMfaCode();
            var expiresAt = DateTime.UtcNow.AddMinutes(5);
            
            await StoreMfaCode(userId, code, type, expiresAt);
            
            switch (type)
            {
                case MfaType.Email:
                    await _emailService.SendMfaCodeAsync(user.Email, code);
                    break;
                case MfaType.Sms:
                    await _smsService.SendMfaCodeAsync(user.PhoneNumber, code);
                    break;
                default:
                    throw new ArgumentException($"Cannot send code for MFA type: {type}");
            }
            
            _logger.LogInformation("MFA code sent to user {UserId} via {Type}", userId, type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending MFA code to user {UserId}", userId);
            throw;
        }
    }
    
    private string GenerateSecretKey()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[20];
        rng.GetBytes(bytes);
        return Base32Encoding.ToString(bytes);
    }
    
    private string GenerateQrCodeUrl(string email, string secretKey)
    {
        var issuer = "YourApp";
        var accountName = email;
        return $"otpauth://totp/{issuer}:{accountName}?secret={secretKey}&issuer={issuer}";
    }
    
    private string GenerateMfaCode()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var code = BitConverter.ToInt32(bytes, 0);
        return Math.Abs(code).ToString("D6");
    }
    
    private async Task StoreMfaSecret(string userId, string secret, MfaType type)
    {
        // Implementar almacenamiento seguro del secret
    }
    
    private async Task<string> GetMfaSecret(string userId, MfaType type)
    {
        // Implementar obtención del secret
        return string.Empty; // Simplificado
    }
    
    private async Task StoreMfaCode(string userId, string code, MfaType type, DateTime expiresAt)
    {
        // Implementar almacenamiento del código MFA
    }
    
    private async Task<MfaCode> GetStoredMfaCode(string userId, MfaType type)
    {
        // Implementar obtención del código MFA almacenado
        return new MfaCode(); // Simplificado
    }
    
    private async Task RemoveStoredMfaCode(string userId, MfaType type)
    {
        // Implementar eliminación del código MFA usado
    }
}

// Modelos para MFA
public enum MfaType
{
    Totp,
    Email,
    Sms
}

public class MfaSetupResult
{
    public MfaType Type { get; set; }
    public string SecretKey { get; set; }
    public string QrCodeUrl { get; set; }
    public bool SetupComplete { get; set; }
}

public class MfaCode
{
    public string Code { get; set; }
    public DateTime ExpiresAt { get; set; }
    public MfaType Type { get; set; }
}
```

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Implementar OAuth2 Client**
```csharp
// Crear un cliente OAuth2 que pueda autenticarse con el servidor
public class OAuth2Client
{
    public async Task<TokenResult> AuthenticateAsync(string clientId, string clientSecret, string redirectUri)
    {
        // Implementar flujo de autenticación OAuth2
        // 1. Redirigir al servidor de autorización
        // 2. Intercambiar código por token
        // 3. Manejar refresh tokens
    }
}
```

### **Ejercicio 2: Implementar JWT Middleware**
```csharp
// Crear middleware que valide JWT tokens automáticamente
public class JwtAuthenticationMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Implementar validación automática de JWT
        // 1. Extraer token del header Authorization
        // 2. Validar token
        // 3. Establecer claims en el contexto
    }
}
```

## 📝 **Resumen de la Clase**

### **Conceptos Clave Aprendidos:**
1. **OAuth2**: Implementación de servidor OAuth2 con múltiples grant types
2. **OpenID Connect**: ID tokens y UserInfo endpoint
3. **JWT**: Generación, validación y manejo de tokens
4. **MFA**: Multi-Factor Authentication con TOTP, Email y SMS
5. **Token Management**: Refresh tokens y revocación
6. **Security Best Practices**: Validación y logging de seguridad

### **Próxima Clase:**
En la siguiente clase exploraremos **Data Encryption y Key Management**, incluyendo encriptación de datos en reposo y en tránsito.

---

## 🔗 **Recursos Adicionales**

- [OAuth 2.0 Specification](https://tools.ietf.org/html/rfc6749)
- [OpenID Connect Specification](https://openid.net/connect/)
- [JWT Specification](https://tools.ietf.org/html/rfc7519)
- [.NET JWT Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
- [TOTP Library](https://github.com/kspearrin/Otp.NET)
