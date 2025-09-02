# 💾 Clase 8: Backup y Disaster Recovery

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 7: Seguridad en Producción](../senior_8/clase_7_seguridad_produccion.md)
- **🏠 Inicio del Módulo**: [Módulo 15: Sistemas Avanzados y Distribuidos](../senior_8/README.md)
- **➡️ Siguiente**: [Clase 9: Testing en Producción](../senior_8/clase_9_testing_produccion.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** estrategias de backup automático
2. **Configurar** disaster recovery procedures
3. **Desarrollar** scripts de restore y recuperación
4. **Aplicar** backup verification y testing
5. **Optimizar** RTO y RPO objectives

---

## 💾 **Estrategias de Backup Automático**

### **Servicio de Backup de Base de Datos**

```csharp
// MusicalMatching.Application/Services/BackupService.cs
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MusicalMatching.Application.Services;

public interface IBackupService
{
    Task<BackupResult> CreateDatabaseBackupAsync(string backupPath);
    Task<BackupResult> CreateFileBackupAsync(string sourcePath, string backupPath);
    Task<BackupResult> CreateConfigurationBackupAsync();
    Task<List<BackupInfo>> GetBackupHistoryAsync();
    Task<bool> VerifyBackupAsync(string backupPath);
}

public class BackupService : IBackupService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<BackupService> _logger;
    private readonly string _connectionString;
    private readonly string _backupBasePath;

    public BackupService(IConfiguration configuration, ILogger<BackupService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? 
            throw new InvalidOperationException("Database connection string not configured");
        _backupBasePath = _configuration["Backup:BasePath"] ?? "./backups";
    }

    public async Task<BackupResult> CreateDatabaseBackupAsync(string backupPath)
    {
        try
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fileName = $"MusicalMatching_{timestamp}.bak";
            var fullPath = Path.Combine(backupPath, fileName);

            // Ensure backup directory exists
            Directory.CreateDirectory(backupPath);

            // Create SQL Server backup command
            var backupCommand = $@"
                BACKUP DATABASE [MusicalMatching] 
                TO DISK = '{fullPath}' 
                WITH FORMAT, 
                     MEDIANAME = 'MusicalMatchingBackup',
                     NAME = 'MusicalMatching-Full Database Backup',
                     COMPRESSION,
                     CHECKSUM";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(backupCommand, connection);
            command.CommandTimeout = 3600; // 1 hour timeout

            var stopwatch = Stopwatch.StartNew();
            await command.ExecuteNonQueryAsync();
            stopwatch.Stop();

            var backupInfo = new BackupInfo
            {
                Id = Guid.NewGuid(),
                Type = BackupType.Database,
                Path = fullPath,
                Size = new FileInfo(fullPath).Length,
                CreatedAt = DateTime.UtcNow,
                Duration = stopwatch.Elapsed,
                Status = BackupStatus.Success
            };

            // Save backup info to database
            await SaveBackupInfoAsync(backupInfo);

            _logger.LogInformation("Database backup completed successfully: {Path} in {Duration}ms", 
                fullPath, stopwatch.ElapsedMilliseconds);

            return new BackupResult { Success = true, BackupInfo = backupInfo };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database backup failed");
            return new BackupResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<BackupResult> CreateFileBackupAsync(string sourcePath, string backupPath)
    {
        try
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fileName = $"Files_{timestamp}.zip";
            var fullPath = Path.Combine(backupPath, fileName);

            Directory.CreateDirectory(backupPath);

            // Create ZIP backup
            using var zip = new ZipArchive(File.Create(fullPath), ZipArchiveMode.Create);
            var sourceDirectory = new DirectoryInfo(sourcePath);

            foreach (var file in sourceDirectory.GetFiles("*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(sourcePath, file.FullName);
                var entry = zip.CreateEntry(relativePath, CompressionLevel.Optimal);
                
                using var entryStream = entry.Open();
                using var fileStream = File.OpenRead(file.FullName);
                await fileStream.CopyToAsync(entryStream);
            }

            var backupInfo = new BackupInfo
            {
                Id = Guid.NewGuid(),
                Type = BackupType.Files,
                Path = fullPath,
                Size = new FileInfo(fullPath).Length,
                CreatedAt = DateTime.UtcNow,
                Status = BackupStatus.Success
            };

            await SaveBackupInfoAsync(backupInfo);

            _logger.LogInformation("File backup completed successfully: {Path}", fullPath);
            return new BackupResult { Success = true, BackupInfo = backupInfo };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File backup failed");
            return new BackupResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<BackupResult> CreateConfigurationBackupAsync()
    {
        try
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fileName = $"Config_{timestamp}.json";
            var backupPath = Path.Combine(_backupBasePath, "config");
            var fullPath = Path.Combine(backupPath, fileName);

            Directory.CreateDirectory(backupPath);

            // Backup configuration files
            var configFiles = new[]
            {
                "appsettings.json",
                "appsettings.Production.json",
                "appsettings.Development.json"
            };

            var configData = new Dictionary<string, object>();
            foreach (var configFile in configFiles)
            {
                if (File.Exists(configFile))
                {
                    var content = await File.ReadAllTextAsync(configFile);
                    configData[configFile] = content;
                }
            }

            var jsonContent = JsonSerializer.Serialize(configData, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(fullPath, jsonContent);

            var backupInfo = new BackupInfo
            {
                Id = Guid.NewGuid(),
                Type = BackupType.Configuration,
                Path = fullPath,
                Size = new FileInfo(fullPath).Length,
                CreatedAt = DateTime.UtcNow,
                Status = BackupStatus.Success
            };

            await SaveBackupInfoAsync(backupInfo);

            _logger.LogInformation("Configuration backup completed successfully: {Path}", fullPath);
            return new BackupResult { Success = true, BackupInfo = backupInfo };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Configuration backup failed");
            return new BackupResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    private async Task SaveBackupInfoAsync(BackupInfo backupInfo)
    {
        // Implementation to save backup info to database
        // This would typically use a repository pattern
    }

    public async Task<List<BackupInfo>> GetBackupHistoryAsync()
    {
        // Implementation to retrieve backup history
        return new List<BackupInfo>();
    }

    public async Task<bool> VerifyBackupAsync(string backupPath)
    {
        try
        {
            if (!File.Exists(backupPath))
                return false;

            // Basic verification - check file size and integrity
            var fileInfo = new FileInfo(backupPath);
            if (fileInfo.Length == 0)
                return false;

            // For database backups, could add additional verification
            if (backupPath.EndsWith(".bak"))
            {
                // Verify SQL Server backup file
                return await VerifySqlBackupAsync(backupPath);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Backup verification failed for {Path}", backupPath);
            return false;
        }
    }

    private async Task<bool> VerifySqlBackupAsync(string backupPath)
    {
        try
        {
            var verifyCommand = $"RESTORE VERIFYONLY FROM DISK = '{backupPath}'";
            
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = new SqlCommand(verifyCommand, connection);
            await command.ExecuteNonQueryAsync();
            
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public record BackupResult
{
    public bool Success { get; init; }
    public BackupInfo? BackupInfo { get; init; }
    public string? ErrorMessage { get; init; }
}

public class BackupInfo
{
    public Guid Id { get; set; }
    public BackupType Type { get; set; }
    public string Path { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }
    public TimeSpan Duration { get; set; }
    public BackupStatus Status { get; set; }
}

public enum BackupType
{
    Database,
    Files,
    Configuration
}

public enum BackupStatus
{
    Success,
    Failed,
    InProgress
}
```

---

## 🔄 **Disaster Recovery Procedures**

### **Servicio de Disaster Recovery**

```csharp
// MusicalMatching.Application/Services/DisasterRecoveryService.cs
namespace MusicalMatching.Application.Services;

public interface IDisasterRecoveryService
{
    Task<RecoveryResult> RestoreDatabaseAsync(string backupPath);
    Task<RecoveryResult> RestoreFilesAsync(string backupPath, string restorePath);
    Task<RecoveryResult> RestoreConfigurationAsync(string backupPath);
    Task<RecoveryPlan> CreateRecoveryPlanAsync();
    Task<bool> TestRecoveryProcedureAsync();
}

public class DisasterRecoveryService : IDisasterRecoveryService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DisasterRecoveryService> _logger;
    private readonly string _connectionString;
    private readonly IBackupService _backupService;

    public DisasterRecoveryService(
        IConfiguration configuration,
        ILogger<DisasterRecoveryService> logger,
        IBackupService backupService)
    {
        _configuration = configuration;
        _logger = logger;
        _backupService = backupService;
        _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? 
            throw new InvalidOperationException("Database connection string not configured");
    }

    public async Task<RecoveryResult> RestoreDatabaseAsync(string backupPath)
    {
        try
        {
            _logger.LogInformation("Starting database restore from {BackupPath}", backupPath);

            // Verify backup file
            if (!await _backupService.VerifyBackupAsync(backupPath))
            {
                throw new InvalidOperationException("Backup file verification failed");
            }

            // Stop application services
            await StopApplicationServicesAsync();

            // Restore database
            var restoreCommand = $@"
                USE [master];
                ALTER DATABASE [MusicalMatching] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                RESTORE DATABASE [MusicalMatching] 
                FROM DISK = '{backupPath}' 
                WITH REPLACE, RECOVERY;
                ALTER DATABASE [MusicalMatching] SET MULTI_USER;";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(restoreCommand, connection);
            command.CommandTimeout = 3600; // 1 hour timeout

            var stopwatch = Stopwatch.StartNew();
            await command.ExecuteNonQueryAsync();
            stopwatch.Stop();

            // Restart application services
            await StartApplicationServicesAsync();

            _logger.LogInformation("Database restore completed successfully in {Duration}ms", 
                stopwatch.ElapsedMilliseconds);

            return new RecoveryResult
            {
                Success = true,
                Duration = stopwatch.Elapsed,
                Message = "Database restored successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database restore failed");
            
            // Attempt to restart services even if restore failed
            await StartApplicationServicesAsync();
            
            return new RecoveryResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<RecoveryResult> RestoreFilesAsync(string backupPath, string restorePath)
    {
        try
        {
            _logger.LogInformation("Starting file restore from {BackupPath} to {RestorePath}", 
                backupPath, restorePath);

            if (!File.Exists(backupPath))
                throw new FileNotFoundException("Backup file not found", backupPath);

            // Create restore directory
            Directory.CreateDirectory(restorePath);

            // Extract ZIP backup
            using var zip = ZipFile.OpenRead(backupPath);
            foreach (var entry in zip.Entries)
            {
                var targetPath = Path.Combine(restorePath, entry.FullName);
                var targetDir = Path.GetDirectoryName(targetPath);
                
                if (!string.IsNullOrEmpty(targetDir))
                    Directory.CreateDirectory(targetDir);

                entry.ExtractToFile(targetPath, true);
            }

            _logger.LogInformation("File restore completed successfully");
            return new RecoveryResult
            {
                Success = true,
                Message = "Files restored successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File restore failed");
            return new RecoveryResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<RecoveryResult> RestoreConfigurationAsync(string backupPath)
    {
        try
        {
            _logger.LogInformation("Starting configuration restore from {BackupPath}", backupPath);

            if (!File.Exists(backupPath))
                throw new FileNotFoundException("Configuration backup file not found", backupPath);

            var jsonContent = await File.ReadAllTextAsync(backupPath);
            var configData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent);

            if (configData == null)
                throw new InvalidOperationException("Invalid configuration backup format");

            // Restore configuration files
            foreach (var (fileName, content) in configData)
            {
                if (content is string configContent)
                {
                    await File.WriteAllTextAsync(fileName, configContent);
                }
            }

            _logger.LogInformation("Configuration restore completed successfully");
            return new RecoveryResult
            {
                Success = true,
                Message = "Configuration restored successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Configuration restore failed");
            return new RecoveryResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<RecoveryPlan> CreateRecoveryPlanAsync()
    {
        var plan = new RecoveryPlan
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Steps = new List<RecoveryStep>
            {
                new RecoveryStep
                {
                    Order = 1,
                    Description = "Stop application services",
                    Action = "StopApplicationServices",
                    EstimatedDuration = TimeSpan.FromMinutes(2)
                },
                new RecoveryStep
                {
                    Order = 2,
                    Description = "Restore database from latest backup",
                    Action = "RestoreDatabase",
                    EstimatedDuration = TimeSpan.FromMinutes(30)
                },
                new RecoveryStep
                {
                    Order = 3,
                    Description = "Restore configuration files",
                    Action = "RestoreConfiguration",
                    EstimatedDuration = TimeSpan.FromMinutes(5)
                },
                new RecoveryStep
                {
                    Order = 4,
                    Description = "Start application services",
                    Action = "StartApplicationServices",
                    EstimatedDuration = TimeSpan.FromMinutes(3)
                },
                new RecoveryStep
                {
                    Order = 5,
                    Description = "Verify system health",
                    Action = "HealthCheck",
                    EstimatedDuration = TimeSpan.FromMinutes(5)
                }
            }
        };

        return plan;
    }

    public async Task<bool> TestRecoveryProcedureAsync()
    {
        try
        {
            _logger.LogInformation("Starting recovery procedure test");

            // Create test backup
            var testBackup = await _backupService.CreateDatabaseBackupAsync("./test-backups");
            if (!testBackup.Success)
                return false;

            // Test restore to test database
            var testConnectionString = _connectionString.Replace("MusicalMatching", "MusicalMatching_Test");
            var testRestoreResult = await RestoreToTestDatabaseAsync(testBackup.BackupInfo!.Path, testConnectionString);

            // Cleanup test data
            await CleanupTestDataAsync(testConnectionString);

            _logger.LogInformation("Recovery procedure test completed successfully");
            return testRestoreResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Recovery procedure test failed");
            return false;
        }
    }

    private async Task StopApplicationServicesAsync()
    {
        // Implementation to stop application services
        _logger.LogInformation("Stopping application services");
        await Task.Delay(2000); // Simulate service stop
    }

    private async Task StartApplicationServicesAsync()
    {
        // Implementation to start application services
        _logger.LogInformation("Starting application services");
        await Task.Delay(3000); // Simulate service start
    }

    private async Task<bool> RestoreToTestDatabaseAsync(string backupPath, string testConnectionString)
    {
        // Implementation to restore to test database
        return true;
    }

    private async Task CleanupTestDataAsync(string testConnectionString)
    {
        // Implementation to cleanup test data
    }
}

public record RecoveryResult
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public string? ErrorMessage { get; init; }
    public TimeSpan Duration { get; init; }
}

public class RecoveryPlan
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<RecoveryStep> Steps { get; set; } = new();
}

public class RecoveryStep
{
    public int Order { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public TimeSpan EstimatedDuration { get; set; }
}
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Backup Automático**
```csharp
// Implementa:
// - Backup de base de datos
// - Backup de archivos
// - Backup de configuración
// - Verificación de backups
```

### **Ejercicio 2: Disaster Recovery**
```csharp
// Crea:
// - Procedimientos de recuperación
// - Restore de base de datos
// - Restore de archivos
// - Planes de recuperación
```

### **Ejercicio 3: Testing de Recuperación**
```csharp
// Implementa:
// - Testing de procedimientos
// - Verificación de RTO/RPO
// - Simulacros de desastre
// - Documentación de procedimientos
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **💾 Estrategias de Backup**: Backup automático de base de datos, archivos y configuración
2. **🔄 Disaster Recovery**: Procedimientos de recuperación y restore
3. **📋 Planes de Recuperación**: Pasos estructurados para recuperación ante desastres
4. **🧪 Testing de Recuperación**: Verificación de procedimientos de recuperación
5. **📊 RTO y RPO**: Objetivos de tiempo de recuperación y pérdida de datos

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Testing en Producción**, implementando pruebas de humo, carga y caos engineering.

---

**¡Has completado la octava clase del Módulo 15! 💾🔄**
