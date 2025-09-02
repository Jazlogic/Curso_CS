# 🔐 **Clase 4: Data Encryption y Key Management**

## 🎯 **Objetivo de la Clase**
Dominar las técnicas avanzadas de encriptación de datos, gestión de claves criptográficas y protección de información sensible en aplicaciones .NET.

## 📚 **Contenido de la Clase**

### **1. Encriptación de Datos en Reposo**

#### **1.1 Encriptación de Base de Datos**
```csharp
// Servicio de encriptación de base de datos
public class DatabaseEncryptionService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseEncryptionService> _logger;
    private readonly string _encryptionKey;
    
    public DatabaseEncryptionService(IConfiguration configuration, ILogger<DatabaseEncryptionService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _encryptionKey = _configuration["Encryption:DatabaseKey"];
    }
    
    // Encriptar datos antes de guardar en base de datos
    public async Task<string> EncryptDataAsync(string plainText)
    {
        try
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;
            
            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(_encryptionKey);
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            
            using var encryptor = aes.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            
            // Escribir IV al inicio del stream
            await msEncrypt.WriteAsync(aes.IV, 0, aes.IV.Length);
            
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using var swEncrypt = new StreamWriter(csEncrypt);
            
            await swEncrypt.WriteAsync(plainText);
            await swEncrypt.FlushAsync();
            csEncrypt.FlushFinalBlock();
            
            var encrypted = msEncrypt.ToArray();
            var result = Convert.ToBase64String(encrypted);
            
            _logger.LogInformation("Data encrypted successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting data");
            throw new CryptographicException("Failed to encrypt data", ex);
        }
    }
    
    // Desencriptar datos después de leer de base de datos
    public async Task<string> DecryptDataAsync(string cipherText)
    {
        try
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;
            
            var fullCipher = Convert.FromBase64String(cipherText);
            
            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(_encryptionKey);
            
            // Extraer IV del inicio del cipher
            var iv = new byte[16];
            var cipher = new byte[fullCipher.Length - 16];
            
            Array.Copy(fullCipher, 0, iv, 0, 16);
            Array.Copy(fullCipher, 16, cipher, 0, cipher.Length);
            
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            
            using var decryptor = aes.CreateDecryptor();
            using var msDecrypt = new MemoryStream(cipher);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            
            var result = await srDecrypt.ReadToEndAsync();
            
            _logger.LogInformation("Data decrypted successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting data");
            throw new CryptographicException("Failed to decrypt data", ex);
        }
    }
    
    // Encriptar campos específicos de una entidad
    public async Task<T> EncryptEntityFieldsAsync<T>(T entity, List<string> fieldsToEncrypt)
    {
        try
        {
            var entityType = typeof(T);
            var encryptedEntity = Activator.CreateInstance<T>();
            
            foreach (var property in entityType.GetProperties())
            {
                var value = property.GetValue(entity);
                
                if (fieldsToEncrypt.Contains(property.Name) && value is string stringValue)
                {
                    var encryptedValue = await EncryptDataAsync(stringValue);
                    property.SetValue(encryptedEntity, encryptedValue);
                }
                else
                {
                    property.SetValue(encryptedEntity, value);
                }
            }
            
            return encryptedEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting entity fields");
            throw;
        }
    }
    
    // Desencriptar campos específicos de una entidad
    public async Task<T> DecryptEntityFieldsAsync<T>(T entity, List<string> fieldsToDecrypt)
    {
        try
        {
            var entityType = typeof(T);
            var decryptedEntity = Activator.CreateInstance<T>();
            
            foreach (var property in entityType.GetProperties())
            {
                var value = property.GetValue(entity);
                
                if (fieldsToDecrypt.Contains(property.Name) && value is string stringValue)
                {
                    var decryptedValue = await DecryptDataAsync(stringValue);
                    property.SetValue(decryptedEntity, decryptedValue);
                }
                else
                {
                    property.SetValue(decryptedEntity, value);
                }
            }
            
            return decryptedEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting entity fields");
            throw;
        }
    }
}

// Interceptor de Entity Framework para encriptación automática
public class EncryptionInterceptor : DbCommandInterceptor
{
    private readonly DatabaseEncryptionService _encryptionService;
    private readonly ILogger<EncryptionInterceptor> _logger;
    private readonly List<string> _encryptedFields;
    
    public EncryptionInterceptor(
        DatabaseEncryptionService encryptionService,
        ILogger<EncryptionInterceptor> logger)
    {
        _encryptionService = encryptionService;
        _logger = logger;
        _encryptedFields = new List<string> { "Email", "PhoneNumber", "SSN", "CreditCardNumber" };
    }
    
    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        if (result.HasResult)
        {
            var reader = result.Result;
            var encryptedReader = new EncryptedDataReader(reader, _encryptionService, _encryptedFields);
            return InterceptionResult<DbDataReader>.SuppressWithResult(encryptedReader);
        }
        
        return result;
    }
    
    public override async ValueTask<int> NonQueryExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    {
        // Log de operaciones de escritura
        _logger.LogInformation("Non-query executed: {CommandText}", command.CommandText);
        return result;
    }
}

// DataReader personalizado para desencriptación automática
public class EncryptedDataReader : DbDataReader
{
    private readonly DbDataReader _innerReader;
    private readonly DatabaseEncryptionService _encryptionService;
    private readonly List<string> _encryptedFields;
    
    public EncryptedDataReader(
        DbDataReader innerReader,
        DatabaseEncryptionService encryptionService,
        List<string> encryptedFields)
    {
        _innerReader = innerReader;
        _encryptionService = encryptionService;
        _encryptedFields = encryptedFields;
    }
    
    public override string GetString(int ordinal)
    {
        var value = _innerReader.GetString(ordinal);
        var fieldName = _innerReader.GetName(ordinal);
        
        if (_encryptedFields.Contains(fieldName) && !string.IsNullOrEmpty(value))
        {
            try
            {
                return _encryptionService.DecryptDataAsync(value).Result;
            }
            catch
            {
                return value; // Retornar valor original si falla la desencriptación
            }
        }
        
        return value;
    }
    
    // Implementar otros métodos requeridos por DbDataReader
    public override object this[int ordinal] => GetValue(ordinal);
    public override object this[string name] => GetValue(GetOrdinal(name));
    public override int Depth => _innerReader.Depth;
    public override int FieldCount => _innerReader.FieldCount;
    public override bool HasRows => _innerReader.HasRows;
    public override bool IsClosed => _innerReader.IsClosed;
    public override int RecordsAffected => _innerReader.RecordsAffected;
    
    public override bool GetBoolean(int ordinal) => _innerReader.GetBoolean(ordinal);
    public override byte GetByte(int ordinal) => _innerReader.GetByte(ordinal);
    public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) =>
        _innerReader.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
    public override char GetChar(int ordinal) => _innerReader.GetChar(ordinal);
    public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) =>
        _innerReader.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
    public override DateTime GetDateTime(int ordinal) => _innerReader.GetDateTime(ordinal);
    public override decimal GetDecimal(int ordinal) => _innerReader.GetDecimal(ordinal);
    public override double GetDouble(int ordinal) => _innerReader.GetDouble(ordinal);
    public override float GetFloat(int ordinal) => _innerReader.GetFloat(ordinal);
    public override Guid GetGuid(int ordinal) => _innerReader.GetGuid(ordinal);
    public override short GetInt16(int ordinal) => _innerReader.GetInt16(ordinal);
    public override int GetInt32(int ordinal) => _innerReader.GetInt32(ordinal);
    public override long GetInt64(int ordinal) => _innerReader.GetInt64(ordinal);
    public override string GetName(int ordinal) => _innerReader.GetName(ordinal);
    public override int GetOrdinal(string name) => _innerReader.GetOrdinal(name);
    public override Type GetFieldType(int ordinal) => _innerReader.GetFieldType(ordinal);
    public override object GetValue(int ordinal) => _innerReader.GetValue(ordinal);
    public override int GetValues(object[] values) => _innerReader.GetValues(values);
    public override bool IsDBNull(int ordinal) => _innerReader.IsDBNull(ordinal);
    public override bool NextResult() => _innerReader.NextResult();
    public override bool Read() => _innerReader.Read();
    
    public override void Close() => _innerReader.Close();
    public override DataTable GetSchemaTable() => _innerReader.GetSchemaTable();
    public override bool NextResult() => _innerReader.NextResult();
    
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _innerReader?.Dispose();
        }
        base.Dispose(disposing);
    }
}
```

#### **1.2 Encriptación de Archivos**
```csharp
// Servicio de encriptación de archivos
public class FileEncryptionService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileEncryptionService> _logger;
    private readonly string _encryptionKey;
    
    public FileEncryptionService(IConfiguration configuration, ILogger<FileEncryptionService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _encryptionKey = _configuration["Encryption:FileKey"];
    }
    
    // Encriptar archivo
    public async Task<string> EncryptFileAsync(string filePath, string outputPath = null)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }
            
            outputPath ??= filePath + ".encrypted";
            
            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(_encryptionKey);
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            
            using var encryptor = aes.CreateEncryptor();
            using var inputStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            
            // Escribir IV al inicio del archivo
            await outputStream.WriteAsync(aes.IV, 0, aes.IV.Length);
            
            using var cryptoStream = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write);
            await inputStream.CopyToAsync(cryptoStream);
            
            _logger.LogInformation("File encrypted successfully: {FilePath}", filePath);
            return outputPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting file: {FilePath}", filePath);
            throw new CryptographicException("Failed to encrypt file", ex);
        }
    }
    
    // Desencriptar archivo
    public async Task<string> DecryptFileAsync(string encryptedFilePath, string outputPath = null)
    {
        try
        {
            if (!File.Exists(encryptedFilePath))
            {
                throw new FileNotFoundException($"Encrypted file not found: {encryptedFilePath}");
            }
            
            outputPath ??= encryptedFilePath.Replace(".encrypted", "");
            
            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(_encryptionKey);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            
            using var inputStream = new FileStream(encryptedFilePath, FileMode.Open, FileAccess.Read);
            
            // Leer IV del inicio del archivo
            var iv = new byte[16];
            await inputStream.ReadAsync(iv, 0, 16);
            aes.IV = iv;
            
            using var decryptor = aes.CreateDecryptor();
            using var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            using var cryptoStream = new CryptoStream(inputStream, decryptor, CryptoStreamMode.Read);
            
            await cryptoStream.CopyToAsync(outputStream);
            
            _logger.LogInformation("File decrypted successfully: {FilePath}", encryptedFilePath);
            return outputPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting file: {FilePath}", encryptedFilePath);
            throw new CryptographicException("Failed to decrypt file", ex);
        }
    }
    
    // Encriptar archivo en memoria
    public async Task<byte[]> EncryptFileInMemoryAsync(byte[] fileData)
    {
        try
        {
            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(_encryptionKey);
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            
            using var encryptor = aes.CreateEncryptor();
            using var inputStream = new MemoryStream(fileData);
            using var outputStream = new MemoryStream();
            
            // Escribir IV al inicio
            await outputStream.WriteAsync(aes.IV, 0, aes.IV.Length);
            
            using var cryptoStream = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write);
            await inputStream.CopyToAsync(cryptoStream);
            
            _logger.LogInformation("File encrypted in memory successfully");
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting file in memory");
            throw new CryptographicException("Failed to encrypt file in memory", ex);
        }
    }
    
    // Desencriptar archivo en memoria
    public async Task<byte[]> DecryptFileInMemoryAsync(byte[] encryptedData)
    {
        try
        {
            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(_encryptionKey);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            
            // Extraer IV del inicio
            var iv = new byte[16];
            var cipher = new byte[encryptedData.Length - 16];
            
            Array.Copy(encryptedData, 0, iv, 0, 16);
            Array.Copy(encryptedData, 16, cipher, 0, cipher.Length);
            
            aes.IV = iv;
            
            using var decryptor = aes.CreateDecryptor();
            using var inputStream = new MemoryStream(cipher);
            using var outputStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(inputStream, decryptor, CryptoStreamMode.Read);
            
            await cryptoStream.CopyToAsync(outputStream);
            
            _logger.LogInformation("File decrypted in memory successfully");
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting file in memory");
            throw new CryptographicException("Failed to decrypt file in memory", ex);
        }
    }
}
```

### **2. Encriptación de Datos en Tránsito**

#### **2.1 HTTPS y TLS Configuration**
```csharp
// Configuración avanzada de HTTPS y TLS
public class TlsConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TlsConfigurationService> _logger;
    
    public TlsConfigurationService(IConfiguration configuration, ILogger<TlsConfigurationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    
    // Configurar TLS en el servidor
    public void ConfigureTls(IServiceCollection services)
    {
        // Configuración de HTTPS
        services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(365);
        });
        
        // Configuración de certificados
        services.Configure<HttpsRedirectionOptions>(options =>
        {
            options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
            options.HttpsPort = 443;
        });
        
        // Configuración de TLS
        services.Configure<HttpsConnectionAdapterOptions>(options =>
        {
            options.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
            options.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
            options.ClientCertificateValidation = ValidateClientCertificate;
        });
    }
    
    // Validación de certificados de cliente
    private bool ValidateClientCertificate(X509Certificate2 clientCertificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        try
        {
            // Verificar que el certificado no ha expirado
            if (clientCertificate.NotAfter < DateTime.UtcNow)
            {
                _logger.LogWarning("Client certificate has expired: {Thumbprint}", clientCertificate.Thumbprint);
                return false;
            }
            
            // Verificar que el certificado no ha sido revocado
            if (chain.ChainStatus.Any(status => status.Status == X509ChainStatusFlags.Revoked))
            {
                _logger.LogWarning("Client certificate has been revoked: {Thumbprint}", clientCertificate.Thumbprint);
                return false;
            }
            
            // Verificar que el certificado es válido
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                _logger.LogWarning("SSL policy errors for client certificate: {Errors}", sslPolicyErrors);
                return false;
            }
            
            _logger.LogInformation("Client certificate validated successfully: {Thumbprint}", clientCertificate.Thumbprint);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating client certificate");
            return false;
        }
    }
    
    // Configurar HttpClient con TLS
    public void ConfigureHttpClient(IServiceCollection services)
    {
        services.AddHttpClient("SecureClient", client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "SecureApp/1.0");
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            var handler = new HttpClientHandler();
            handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
            handler.ServerCertificateCustomValidationCallback = ValidateServerCertificate;
            return handler;
        });
    }
    
    // Validación de certificados de servidor
    private bool ValidateServerCertificate(HttpRequestMessage request, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        try
        {
            // Verificar que el certificado no ha expirado
            if (certificate.NotAfter < DateTime.UtcNow)
            {
                _logger.LogWarning("Server certificate has expired: {Thumbprint}", certificate.Thumbprint);
                return false;
            }
            
            // Verificar que el certificado no ha sido revocado
            if (chain.ChainStatus.Any(status => status.Status == X509ChainStatusFlags.Revoked))
            {
                _logger.LogWarning("Server certificate has been revoked: {Thumbprint}", certificate.Thumbprint);
                return false;
            }
            
            // Verificar que el certificado es válido
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                _logger.LogWarning("SSL policy errors for server certificate: {Errors}", sslPolicyErrors);
                return false;
            }
            
            _logger.LogInformation("Server certificate validated successfully: {Thumbprint}", certificate.Thumbprint);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating server certificate");
            return false;
        }
    }
}
```

#### **2.2 Encriptación de Comunicaciones**
```csharp
// Servicio de encriptación de comunicaciones
public class CommunicationEncryptionService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CommunicationEncryptionService> _logger;
    private readonly string _publicKey;
    private readonly string _privateKey;
    
    public CommunicationEncryptionService(IConfiguration configuration, ILogger<CommunicationEncryptionService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _publicKey = _configuration["Encryption:PublicKey"];
        _privateKey = _configuration["Encryption:PrivateKey"];
    }
    
    // Encriptar mensaje con clave pública
    public async Task<string> EncryptMessageAsync(string message, string recipientPublicKey)
    {
        try
        {
            using var rsa = RSA.Create();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(recipientPublicKey), out _);
            
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var encryptedBytes = rsa.Encrypt(messageBytes, RSAEncryptionPadding.OaepSHA256);
            
            var result = Convert.ToBase64String(encryptedBytes);
            
            _logger.LogInformation("Message encrypted successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting message");
            throw new CryptographicException("Failed to encrypt message", ex);
        }
    }
    
    // Desencriptar mensaje con clave privada
    public async Task<string> DecryptMessageAsync(string encryptedMessage)
    {
        try
        {
            using var rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(_privateKey), out _);
            
            var encryptedBytes = Convert.FromBase64String(encryptedMessage);
            var decryptedBytes = rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA256);
            
            var result = Encoding.UTF8.GetString(decryptedBytes);
            
            _logger.LogInformation("Message decrypted successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting message");
            throw new CryptographicException("Failed to decrypt message", ex);
        }
    }
    
    // Firmar mensaje con clave privada
    public async Task<string> SignMessageAsync(string message)
    {
        try
        {
            using var rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(_privateKey), out _);
            
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var signatureBytes = rsa.SignData(messageBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            
            var result = Convert.ToBase64String(signatureBytes);
            
            _logger.LogInformation("Message signed successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error signing message");
            throw new CryptographicException("Failed to sign message", ex);
        }
    }
    
    // Verificar firma con clave pública
    public async Task<bool> VerifySignatureAsync(string message, string signature, string signerPublicKey)
    {
        try
        {
            using var rsa = RSA.Create();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(signerPublicKey), out _);
            
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var signatureBytes = Convert.FromBase64String(signature);
            
            var isValid = rsa.VerifyData(messageBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            
            _logger.LogInformation("Signature verification result: {IsValid}", isValid);
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying signature");
            return false;
        }
    }
    
    // Generar par de claves RSA
    public async Task<KeyPair> GenerateKeyPairAsync(int keySize = 2048)
    {
        try
        {
            using var rsa = RSA.Create(keySize);
            
            var publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
            var privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
            
            _logger.LogInformation("Key pair generated successfully");
            
            return new KeyPair
            {
                PublicKey = publicKey,
                PrivateKey = privateKey,
                KeySize = keySize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating key pair");
            throw new CryptographicException("Failed to generate key pair", ex);
        }
    }
}

// Modelo para par de claves
public class KeyPair
{
    public string PublicKey { get; set; }
    public string PrivateKey { get; set; }
    public int KeySize { get; set; }
}
```

### **3. Gestión de Claves Criptográficas**

#### **3.1 Key Management Service**
```csharp
// Servicio de gestión de claves
public class KeyManagementService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<KeyManagementService> _logger;
    private readonly IKeyVaultService _keyVaultService;
    
    public KeyManagementService(
        IConfiguration configuration,
        ILogger<KeyManagementService> logger,
        IKeyVaultService keyVaultService)
    {
        _configuration = configuration;
        _logger = logger;
        _keyVaultService = keyVaultService;
    }
    
    // Generar nueva clave de encriptación
    public async Task<EncryptionKey> GenerateEncryptionKeyAsync(string keyName, int keySize = 256)
    {
        try
        {
            var keyId = Guid.NewGuid().ToString();
            var keyBytes = new byte[keySize / 8];
            
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(keyBytes);
            
            var key = new EncryptionKey
            {
                Id = keyId,
                Name = keyName,
                KeyData = Convert.ToBase64String(keyBytes),
                KeySize = keySize,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            
            // Almacenar clave en Key Vault
            await _keyVaultService.StoreKeyAsync(key);
            
            _logger.LogInformation("Encryption key generated: {KeyName}", keyName);
            return key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating encryption key: {KeyName}", keyName);
            throw;
        }
    }
    
    // Rotar clave de encriptación
    public async Task<EncryptionKey> RotateEncryptionKeyAsync(string keyName)
    {
        try
        {
            // Obtener clave actual
            var currentKey = await _keyVaultService.GetKeyAsync(keyName);
            if (currentKey == null)
            {
                throw new KeyNotFoundException($"Key not found: {keyName}");
            }
            
            // Generar nueva clave
            var newKey = await GenerateEncryptionKeyAsync($"{keyName}_v{DateTime.UtcNow:yyyyMMddHHmmss}");
            
            // Marcar clave anterior como inactiva
            currentKey.IsActive = false;
            currentKey.RotatedAt = DateTime.UtcNow;
            currentKey.RotatedToKeyId = newKey.Id;
            
            await _keyVaultService.UpdateKeyAsync(currentKey);
            
            _logger.LogInformation("Encryption key rotated: {KeyName}", keyName);
            return newKey;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rotating encryption key: {KeyName}", keyName);
            throw;
        }
    }
    
    // Obtener clave de encriptación
    public async Task<EncryptionKey> GetEncryptionKeyAsync(string keyName)
    {
        try
        {
            var key = await _keyVaultService.GetKeyAsync(keyName);
            if (key == null)
            {
                throw new KeyNotFoundException($"Key not found: {keyName}");
            }
            
            if (!key.IsActive)
            {
                throw new InvalidOperationException($"Key is not active: {keyName}");
            }
            
            _logger.LogInformation("Encryption key retrieved: {KeyName}", keyName);
            return key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving encryption key: {KeyName}", keyName);
            throw;
        }
    }
    
    // Revocar clave de encriptación
    public async Task RevokeEncryptionKeyAsync(string keyName, string reason)
    {
        try
        {
            var key = await _keyVaultService.GetKeyAsync(keyName);
            if (key == null)
            {
                throw new KeyNotFoundException($"Key not found: {keyName}");
            }
            
            key.IsActive = false;
            key.RevokedAt = DateTime.UtcNow;
            key.RevocationReason = reason;
            
            await _keyVaultService.UpdateKeyAsync(key);
            
            _logger.LogInformation("Encryption key revoked: {KeyName}, Reason: {Reason}", keyName, reason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking encryption key: {KeyName}", keyName);
            throw;
        }
    }
    
    // Listar claves de encriptación
    public async Task<List<EncryptionKey>> ListEncryptionKeysAsync(bool includeInactive = false)
    {
        try
        {
            var keys = await _keyVaultService.ListKeysAsync();
            
            if (!includeInactive)
            {
                keys = keys.Where(k => k.IsActive).ToList();
            }
            
            _logger.LogInformation("Encryption keys listed: {Count} keys", keys.Count);
            return keys;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing encryption keys");
            throw;
        }
    }
    
    // Verificar integridad de clave
    public async Task<bool> VerifyKeyIntegrityAsync(string keyName)
    {
        try
        {
            var key = await _keyVaultService.GetKeyAsync(keyName);
            if (key == null)
            {
                return false;
            }
            
            // Verificar que la clave no ha expirado
            if (key.ExpiresAt.HasValue && key.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Key has expired: {KeyName}", keyName);
                return false;
            }
            
            // Verificar que la clave está activa
            if (!key.IsActive)
            {
                _logger.LogWarning("Key is not active: {KeyName}", keyName);
                return false;
            }
            
            // Verificar que la clave no ha sido revocada
            if (key.RevokedAt.HasValue)
            {
                _logger.LogWarning("Key has been revoked: {KeyName}", keyName);
                return false;
            }
            
            _logger.LogInformation("Key integrity verified: {KeyName}", keyName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying key integrity: {KeyName}", keyName);
            return false;
        }
    }
}

// Modelo para clave de encriptación
public class EncryptionKey
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string KeyData { get; set; }
    public int KeySize { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime? RotatedAt { get; set; }
    public string RotatedToKeyId { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string RevocationReason { get; set; }
}

// Interfaz para Key Vault Service
public interface IKeyVaultService
{
    Task StoreKeyAsync(EncryptionKey key);
    Task<EncryptionKey> GetKeyAsync(string keyName);
    Task UpdateKeyAsync(EncryptionKey key);
    Task<List<EncryptionKey>> ListKeysAsync();
    Task DeleteKeyAsync(string keyName);
}
```

#### **3.2 Azure Key Vault Integration**
```csharp
// Integración con Azure Key Vault
public class AzureKeyVaultService : IKeyVaultService
{
    private readonly SecretClient _secretClient;
    private readonly KeyClient _keyClient;
    private readonly ILogger<AzureKeyVaultService> _logger;
    
    public AzureKeyVaultService(
        SecretClient secretClient,
        KeyClient keyClient,
        ILogger<AzureKeyVaultService> logger)
    {
        _secretClient = secretClient;
        _keyClient = keyClient;
        _logger = logger;
    }
    
    // Almacenar clave en Key Vault
    public async Task StoreKeyAsync(EncryptionKey key)
    {
        try
        {
            var secretName = $"encryption-key-{key.Name}";
            var secretValue = JsonSerializer.Serialize(key);
            
            await _secretClient.SetSecretAsync(secretName, secretValue);
            
            _logger.LogInformation("Key stored in Azure Key Vault: {KeyName}", key.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing key in Azure Key Vault: {KeyName}", key.Name);
            throw;
        }
    }
    
    // Obtener clave de Key Vault
    public async Task<EncryptionKey> GetKeyAsync(string keyName)
    {
        try
        {
            var secretName = $"encryption-key-{keyName}";
            var secret = await _secretClient.GetSecretAsync(secretName);
            
            if (secret == null)
            {
                return null;
            }
            
            var key = JsonSerializer.Deserialize<EncryptionKey>(secret.Value.Value);
            
            _logger.LogInformation("Key retrieved from Azure Key Vault: {KeyName}", keyName);
            return key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving key from Azure Key Vault: {KeyName}", keyName);
            throw;
        }
    }
    
    // Actualizar clave en Key Vault
    public async Task UpdateKeyAsync(EncryptionKey key)
    {
        try
        {
            var secretName = $"encryption-key-{key.Name}";
            var secretValue = JsonSerializer.Serialize(key);
            
            await _secretClient.SetSecretAsync(secretName, secretValue);
            
            _logger.LogInformation("Key updated in Azure Key Vault: {KeyName}", key.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating key in Azure Key Vault: {KeyName}", key.Name);
            throw;
        }
    }
    
    // Listar claves de Key Vault
    public async Task<List<EncryptionKey>> ListKeysAsync()
    {
        try
        {
            var keys = new List<EncryptionKey>();
            
            await foreach (var secret in _secretClient.GetPropertiesOfSecretsAsync())
            {
                if (secret.Name.StartsWith("encryption-key-"))
                {
                    var keySecret = await _secretClient.GetSecretAsync(secret.Name);
                    var key = JsonSerializer.Deserialize<EncryptionKey>(keySecret.Value.Value);
                    keys.Add(key);
                }
            }
            
            _logger.LogInformation("Keys listed from Azure Key Vault: {Count} keys", keys.Count);
            return keys;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing keys from Azure Key Vault");
            throw;
        }
    }
    
    // Eliminar clave de Key Vault
    public async Task DeleteKeyAsync(string keyName)
    {
        try
        {
            var secretName = $"encryption-key-{keyName}";
            
            await _secretClient.StartDeleteSecretAsync(secretName);
            
            _logger.LogInformation("Key deleted from Azure Key Vault: {KeyName}", keyName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting key from Azure Key Vault: {KeyName}", keyName);
            throw;
        }
    }
}
```

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Implementar Encriptación de Base de Datos**
```csharp
// Crear un interceptor de Entity Framework que encripte/desencripte automáticamente
public class DatabaseEncryptionInterceptor : DbCommandInterceptor
{
    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        // Implementar desencriptación automática de datos
    }
    
    public override async ValueTask<InterceptionResult<int>> NonQueryExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        // Implementar encriptación automática de datos
    }
}
```

### **Ejercicio 2: Implementar Key Rotation**
```csharp
// Crear un servicio que rote claves automáticamente
public class AutomaticKeyRotationService
{
    public async Task RotateKeysAsync()
    {
        // Implementar rotación automática de claves
        // 1. Identificar claves que necesitan rotación
        // 2. Generar nuevas claves
        // 3. Migrar datos encriptados
        // 4. Actualizar referencias
    }
}
```

## 📝 **Resumen de la Clase**

### **Conceptos Clave Aprendidos:**
1. **Encriptación de Datos en Reposo**: Base de datos y archivos
2. **Encriptación de Datos en Tránsito**: HTTPS, TLS y comunicaciones
3. **Gestión de Claves**: Generación, rotación y revocación
4. **Azure Key Vault**: Integración con servicios de nube
5. **Interceptores de Entity Framework**: Encriptación automática
6. **Validación de Certificados**: TLS y HTTPS

### **Próxima Clase:**
En la siguiente clase exploraremos **Vulnerability Scanning y Penetration Testing**, incluyendo herramientas y técnicas para identificar y corregir vulnerabilidades.

---

## 🔗 **Recursos Adicionales**

- [.NET Cryptography](https://docs.microsoft.com/en-us/dotnet/standard/security/cryptography-model)
- [Azure Key Vault](https://docs.microsoft.com/en-us/azure/key-vault/)
- [Entity Framework Interceptors](https://docs.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors)
- [TLS Configuration](https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl)
- [Certificate Validation](https://docs.microsoft.com/en-us/dotnet/api/system.net.security.sslstream)
