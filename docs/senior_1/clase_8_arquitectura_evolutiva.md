# 🚀 Clase 8: Arquitectura Evolutiva

## 📋 Información de la Clase

- **Módulo**: Senior 1 - Arquitectura de Software Empresarial
- **Duración**: 2 horas
- **Nivel**: Senior
- **Prerrequisitos**: Completar Clase 7 (Monitoreo y Observabilidad)

## 🎯 Objetivos de Aprendizaje

- Implementar arquitecturas que evolucionan con el tiempo
- Aplicar patrones de migración de datos
- Implementar versionado de APIs
- Crear estrategias de evolución de sistemas

---

## 📚 Navegación del Módulo 6

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_arquitectura_limpia_avanzada.md) | Arquitectura Limpia Avanzada | |
| [Clase 2](clase_2_event_driven_architecture.md) | Event-Driven Architecture | |
| [Clase 3](clase_3_microservicios_avanzada.md) | Arquitectura de Microservicios Avanzada | |
| [Clase 4](clase_4_patrones_enterprise.md) | Patrones de Diseño Enterprise | |
| [Clase 5](clase_5_arquitectura_datos_avanzada.md) | Arquitectura de Datos Avanzada | |
| [Clase 6](clase_6_calidad_codigo_metricas.md) | Calidad del Código y Métricas | |
| [Clase 7](clase_7_monitoreo_observabilidad.md) | Monitoreo y Observabilidad | ← Anterior |
| **Clase 8** | **Arquitectura Evolutiva** | ← Estás aquí |
| [Clase 9](clase_9_seguridad_enterprise.md) | Arquitectura de Seguridad Enterprise | Siguiente → |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Plataforma Empresarial | |

**← [Volver al README del Módulo 6](../senior_1/README.md)**

---

## 📚 Contenido Teórico

### 1. Arquitectura Evolutiva

La arquitectura evolutiva permite que los sistemas se adapten y mejoren a lo largo del tiempo sin interrumpir el servicio.

```csharp
// ===== ARQUITECTURA EVOLUTIVA - IMPLEMENTACIÓN COMPLETA =====
namespace EvolutionaryArchitecture
{
    // ===== VERSIONADO DE APIS =====
    namespace ApiVersioning
    {
        public interface IApiVersionManager
        {
            Task<ApiVersion> GetCurrentVersionAsync();
            Task<List<ApiVersion>> GetSupportedVersionsAsync();
            Task<bool> IsVersionSupportedAsync(string version);
            Task<ApiVersion> GetVersionInfoAsync(string version);
        }
        
        public class ApiVersionManager : IApiVersionManager
        {
            private readonly ILogger<ApiVersionManager> _logger;
            private readonly Dictionary<string, ApiVersion> _supportedVersions;
            
            public ApiVersionManager(ILogger<ApiVersionManager> logger)
            {
                _logger = logger;
                _supportedVersions = InitializeSupportedVersions();
            }
            
            public async Task<ApiVersion> GetCurrentVersionAsync()
            {
                return await Task.FromResult(_supportedVersions["v2.0"]);
            }
            
            public async Task<List<ApiVersion>> GetSupportedVersionsAsync()
            {
                return await Task.FromResult(_supportedVersions.Values.ToList());
            }
            
            public async Task<bool> IsVersionSupportedAsync(string version)
            {
                return await Task.FromResult(_supportedVersions.ContainsKey(version));
            }
            
            public async Task<ApiVersion> GetVersionInfoAsync(string version)
            {
                if (_supportedVersions.TryGetValue(version, out var apiVersion))
                {
                    return await Task.FromResult(apiVersion);
                }
                
                throw new VersionNotSupportedException($"API version {version} is not supported");
            }
            
            private Dictionary<string, ApiVersion> InitializeSupportedVersions()
            {
                return new Dictionary<string, ApiVersion>
                {
                    ["v1.0"] = new ApiVersion
                    {
                        Version = "v1.0",
                        ReleaseDate = new DateTime(2023, 1, 1),
                        EndOfLife = new DateTime(2024, 12, 31),
                        Status = VersionStatus.Deprecated,
                        BreakingChanges = new List<string>(),
                        NewFeatures = new List<string>(),
                        DeprecatedFeatures = new List<string>()
                    },
                    ["v1.5"] = new ApiVersion
                    {
                        Version = "v1.5",
                        ReleaseDate = new DateTime(2023, 6, 1),
                        EndOfLife = new DateTime(2024, 12, 31),
                        Status = VersionStatus.Deprecated,
                        BreakingChanges = new List<string>(),
                        NewFeatures = new List<string> { "Enhanced filtering", "Bulk operations" },
                        DeprecatedFeatures = new List<string> { "Legacy authentication" }
                    },
                    ["v2.0"] = new ApiVersion
                    {
                        Version = "v2.0",
                        ReleaseDate = new DateTime(2024, 1, 1),
                        EndOfLife = null,
                        Status = VersionStatus.Current,
                        BreakingChanges = new List<string> { "Authentication model changed", "Response format updated" },
                        NewFeatures = new List<string> { "GraphQL support", "Real-time notifications", "Advanced caching" },
                        DeprecatedFeatures = new List<string> { "v1.0 endpoints", "XML format" }
                    }
                };
            }
        }
        
        public class ApiVersion
        {
            public string Version { get; set; }
            public DateTime ReleaseDate { get; set; }
            public DateTime? EndOfLife { get; set; }
            public VersionStatus Status { get; set; }
            public List<string> BreakingChanges { get; set; }
            public List<string> NewFeatures { get; set; }
            public List<string> DeprecatedFeatures { get; set; }
            
            public ApiVersion()
            {
                BreakingChanges = new List<string>();
                NewFeatures = new List<string>();
                DeprecatedFeatures = new List<string>();
            }
        }
        
        public enum VersionStatus
        {
            Current,
            Supported,
            Deprecated,
            EndOfLife
        }
        
        public class VersionNotSupportedException : Exception
        {
            public VersionNotSupportedException(string message) : base(message) { }
        }
        
        [ApiVersion("1.0")]
        [ApiVersion("1.5")]
        [ApiVersion("2.0")]
        [Route("api/v{version:apiVersion}/[controller]")]
        public class UsersController : ControllerBase
        {
            private readonly IUserService _userService;
            private readonly ILogger<UsersController> _logger;
            
            public UsersController(IUserService userService, ILogger<UsersController> logger)
            {
                _userService = userService;
                _logger = logger;
            }
            
            [HttpGet]
            [MapToApiVersion("1.0")]
            public async Task<ActionResult<List<UserV1Dto>>> GetUsersV1()
            {
                var users = await _userService.GetUsersAsync();
                var v1Users = users.Select(u => new UserV1Dto
                {
                    Id = u.Id,
                    Name = $"{u.FirstName} {u.LastName}",
                    Email = u.Email
                }).ToList();
                
                return Ok(v1Users);
            }
            
            [HttpGet]
            [MapToApiVersion("1.5")]
            public async Task<ActionResult<List<UserV1_5Dto>>> GetUsersV1_5([FromQuery] string filter)
            {
                var users = await _userService.GetUsersAsync(filter);
                var v1_5Users = users.Select(u => new UserV1_5Dto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    CreatedAt = u.CreatedAt
                }).ToList();
                
                return Ok(v1_5Users);
            }
            
            [HttpGet]
            [MapToApiVersion("2.0")]
            public async Task<ActionResult<PaginatedResult<UserV2Dto>>> GetUsersV2([FromQuery] UserQueryV2 query)
            {
                var result = await _userService.GetUsersPaginatedAsync(query);
                var v2Users = result.Items.Select(u => new UserV2Dto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Status = u.Status.ToString(),
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    Metadata = u.Metadata
                }).ToList();
                
                var paginatedResult = new PaginatedResult<UserV2Dto>
                {
                    Items = v2Users,
                    TotalCount = result.TotalCount,
                    Page = result.Page,
                    PageSize = result.PageSize,
                    TotalPages = result.TotalPages
                };
                
                return Ok(paginatedResult);
            }
        }
        
        public class UserV1Dto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
        }
        
        public class UserV1_5Dto
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public DateTime CreatedAt { get; set; }
        }
        
        public class UserV2Dto
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Status { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public Dictionary<string, object> Metadata { get; set; }
        }
        
        public class UserQueryV2
        {
            public string SearchTerm { get; set; }
            public string Status { get; set; }
            public DateTime? CreatedFrom { get; set; }
            public DateTime? CreatedTo { get; set; }
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public string SortBy { get; set; } = "CreatedAt";
            public string SortDirection { get; set; } = "Desc";
        }
        
        public class PaginatedResult<T>
        {
            public List<T> Items { get; set; }
            public int TotalCount { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public int TotalPages { get; set; }
        }
    }
    
    // ===== MIGRACIÓN DE DATOS =====
    namespace DataMigration
    {
        public interface IDataMigrationService
        {
            Task<MigrationResult> MigrateAsync(string fromVersion, string toVersion);
            Task<MigrationResult> RollbackAsync(string fromVersion, string toVersion);
            Task<List<MigrationHistory>> GetMigrationHistoryAsync();
            Task<bool> IsMigrationRequiredAsync(string currentVersion, string targetVersion);
        }
        
        public class DataMigrationService : IDataMigrationService
        {
            private readonly ILogger<DataMigrationService> _logger;
            private readonly ApplicationDbContext _context;
            private readonly List<IDataMigration> _migrations;
            
            public DataMigrationService(ILogger<DataMigrationService> logger, ApplicationDbContext context)
            {
                _logger = logger;
                _context = context;
                _migrations = InitializeMigrations();
            }
            
            public async Task<MigrationResult> MigrateAsync(string fromVersion, string toVersion)
            {
                try
                {
                    _logger.LogInformation("Starting data migration from {FromVersion} to {ToVersion}", fromVersion, toVersion);
                    
                    var migrationsToApply = GetMigrationsToApply(fromVersion, toVersion);
                    var migrationHistory = new List<MigrationHistory>();
                    
                    foreach (var migration in migrationsToApply)
                    {
                        var startTime = DateTime.UtcNow;
                        
                        _logger.LogInformation("Applying migration {MigrationName}", migration.GetType().Name);
                        
                        await migration.MigrateAsync(_context);
                        
                        var endTime = DateTime.UtcNow;
                        var duration = endTime - startTime;
                        
                        var history = new MigrationHistory
                        {
                            MigrationName = migration.GetType().Name,
                            FromVersion = migration.FromVersion,
                            ToVersion = migration.ToVersion,
                            AppliedAt = endTime,
                            Duration = duration,
                            Status = MigrationStatus.Success
                        };
                        
                        migrationHistory.Add(history);
                        _context.MigrationHistory.Add(history);
                        
                        _logger.LogInformation("Migration {MigrationName} completed in {Duration}ms", 
                            migration.GetType().Name, duration.TotalMilliseconds);
                    }
                    
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Data migration completed successfully from {FromVersion} to {ToVersion}", 
                        fromVersion, toVersion);
                    
                    return new MigrationResult
                    {
                        IsSuccess = true,
                        Message = $"Migration completed from {fromVersion} to {toVersion}",
                        AppliedMigrations = migrationHistory,
                        Duration = DateTime.UtcNow - migrationHistory.First().AppliedAt
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Data migration failed from {FromVersion} to {ToVersion}", fromVersion, toVersion);
                    
                    return new MigrationResult
                    {
                        IsSuccess = false,
                        Message = $"Migration failed: {ex.Message}",
                        Error = ex
                    };
                }
            }
            
            public async Task<MigrationResult> RollbackAsync(string fromVersion, string toVersion)
            {
                try
                {
                    _logger.LogInformation("Starting rollback from {FromVersion} to {ToVersion}", fromVersion, toVersion);
                    
                    var migrationsToRollback = GetMigrationsToRollback(fromVersion, toVersion);
                    var rollbackHistory = new List<MigrationHistory>();
                    
                    foreach (var migration in migrationsToRollback)
                    {
                        var startTime = DateTime.UtcNow;
                        
                        _logger.LogInformation("Rolling back migration {MigrationName}", migration.GetType().Name);
                        
                        await migration.RollbackAsync(_context);
                        
                        var endTime = DateTime.UtcNow;
                        var duration = endTime - startTime;
                        
                        var history = new MigrationHistory
                        {
                            MigrationName = $"{migration.GetType().Name}_Rollback",
                            FromVersion = migration.FromVersion,
                            ToVersion = migration.ToVersion,
                            AppliedAt = endTime,
                            Duration = duration,
                            Status = MigrationStatus.Rollback
                        };
                        
                        rollbackHistory.Add(history);
                        _context.MigrationHistory.Add(history);
                        
                        _logger.LogInformation("Rollback {MigrationName} completed in {Duration}ms", 
                            migration.GetType().Name, duration.TotalMilliseconds);
                    }
                    
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Rollback completed successfully from {FromVersion} to {ToVersion}", 
                        fromVersion, toVersion);
                    
                    return new MigrationResult
                    {
                        IsSuccess = true,
                        Message = $"Rollback completed from {fromVersion} to {toVersion}",
                        AppliedMigrations = rollbackHistory,
                        Duration = DateTime.UtcNow - rollbackHistory.First().AppliedAt
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Rollback failed from {FromVersion} to {ToVersion}", fromVersion, toVersion);
                    
                    return new MigrationResult
                    {
                        IsSuccess = false,
                        Message = $"Rollback failed: {ex.Message}",
                        Error = ex
                    };
                }
            }
            
            public async Task<List<MigrationHistory>> GetMigrationHistoryAsync()
            {
                return await _context.MigrationHistory
                    .OrderByDescending(m => m.AppliedAt)
                    .ToListAsync();
            }
            
            public async Task<bool> IsMigrationRequiredAsync(string currentVersion, string targetVersion)
            {
                var migrations = GetMigrationsToApply(currentVersion, targetVersion);
                return await Task.FromResult(migrations.Any());
            }
            
            private List<IDataMigration> GetMigrationsToApply(string fromVersion, string toVersion)
            {
                var fromVersionNumber = ParseVersion(fromVersion);
                var toVersionNumber = ParseVersion(toVersion);
                
                if (fromVersionNumber >= toVersionNumber)
                {
                    return new List<IDataMigration>();
                }
                
                return _migrations
                    .Where(m => ParseVersion(m.FromVersion) > fromVersionNumber && 
                               ParseVersion(m.ToVersion) <= toVersionNumber)
                    .OrderBy(m => ParseVersion(m.FromVersion))
                    .ToList();
            }
            
            private List<IDataMigration> GetMigrationsToRollback(string fromVersion, string toVersion)
            {
                var fromVersionNumber = ParseVersion(fromVersion);
                var toVersionNumber = ParseVersion(toVersion);
                
                if (fromVersionNumber <= toVersionNumber)
                {
                    return new List<IDataMigration>();
                }
                
                return _migrations
                    .Where(m => ParseVersion(m.FromVersion) <= fromVersionNumber && 
                               ParseVersion(m.ToVersion) > toVersionNumber)
                    .OrderByDescending(m => ParseVersion(m.FromVersion))
                    .ToList();
            }
            
            private double ParseVersion(string version)
            {
                if (version.StartsWith("v"))
                {
                    version = version.Substring(1);
                }
                
                if (double.TryParse(version, out var result))
                {
                    return result;
                }
                
                return 0;
            }
            
            private List<IDataMigration> InitializeMigrations()
            {
                return new List<IDataMigration>
                {
                    new UserSchemaV1ToV1_5Migration(),
                    new UserSchemaV1_5ToV2Migration(),
                    new AddUserMetadataMigration()
                };
            }
        }
        
        public interface IDataMigration
        {
            string FromVersion { get; }
            string ToVersion { get; }
            Task MigrateAsync(ApplicationDbContext context);
            Task RollbackAsync(ApplicationDbContext context);
        }
        
        public class UserSchemaV1ToV1_5Migration : IDataMigration
        {
            public string FromVersion => "v1.0";
            public string ToVersion => "v1.5";
            
            public async Task MigrateAsync(ApplicationDbContext context)
            {
                // Split Name field into FirstName and LastName
                var users = await context.Users.ToListAsync();
                
                foreach (var user in users)
                {
                    if (string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(user.Name))
                    {
                        var nameParts = user.Name.Split(' ', 2);
                        user.FirstName = nameParts[0];
                        user.LastName = nameParts.Length > 1 ? nameParts[1] : "";
                    }
                }
                
                await context.SaveChangesAsync();
            }
            
            public async Task RollbackAsync(ApplicationDbContext context)
            {
                // Combine FirstName and LastName back to Name
                var users = await context.Users.ToListAsync();
                
                foreach (var user in users)
                {
                    user.Name = $"{user.FirstName} {user.LastName}".Trim();
                }
                
                await context.SaveChangesAsync();
            }
        }
        
        public class UserSchemaV1_5ToV2Migration : IDataMigration
        {
            public string FromVersion => "v1.5";
            public string ToVersion => "v2.0";
            
            public async Task MigrateAsync(ApplicationDbContext context)
            {
                // Add Status field and Metadata
                var users = await context.Users.ToListAsync();
                
                foreach (var user in users)
                {
                    if (user.Status == UserStatus.Unknown)
                    {
                        user.Status = UserStatus.Active;
                    }
                    
                    if (user.Metadata == null)
                    {
                        user.Metadata = new Dictionary<string, object>
                        {
                            ["migrated_from"] = "v1.5",
                            ["migration_date"] = DateTime.UtcNow
                        };
                    }
                }
                
                await context.SaveChangesAsync();
            }
            
            public async Task RollbackAsync(ApplicationDbContext context)
            {
                // Remove Status and Metadata
                var users = await context.Users.ToListAsync();
                
                foreach (var user in users)
                {
                    user.Status = UserStatus.Unknown;
                    user.Metadata = null;
                }
                
                await context.SaveChangesAsync();
            }
        }
        
        public class AddUserMetadataMigration : IDataMigration
        {
            public string FromVersion => "v2.0";
            public string ToVersion => "v2.1";
            
            public async Task MigrateAsync(ApplicationDbContext context)
            {
                // Add additional metadata fields
                var users = await context.Users.ToListAsync();
                
                foreach (var user in users)
                {
                    if (user.Metadata == null)
                    {
                        user.Metadata = new Dictionary<string, object>();
                    }
                    
                    if (!user.Metadata.ContainsKey("last_login"))
                    {
                        user.Metadata["last_login"] = null;
                    }
                    
                    if (!user.Metadata.ContainsKey("preferences"))
                    {
                        user.Metadata["preferences"] = new Dictionary<string, object>
                        {
                            ["theme"] = "default",
                            ["language"] = "en"
                        };
                    }
                }
                
                await context.SaveChangesAsync();
            }
            
            public async Task RollbackAsync(ApplicationDbContext context)
            {
                // Remove additional metadata fields
                var users = await context.Users.ToListAsync();
                
                foreach (var user in users)
                {
                    if (user.Metadata != null)
                    {
                        user.Metadata.Remove("last_login");
                        user.Metadata.Remove("preferences");
                    }
                }
                
                await context.SaveChangesAsync();
            }
        }
        
        public class MigrationResult
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
            public List<MigrationHistory> AppliedMigrations { get; set; }
            public TimeSpan Duration { get; set; }
            public Exception Error { get; set; }
        }
        
        public class MigrationHistory
        {
            public int Id { get; set; }
            public string MigrationName { get; set; }
            public string FromVersion { get; set; }
            public string ToVersion { get; set; }
            public DateTime AppliedAt { get; set; }
            public TimeSpan Duration { get; set; }
            public MigrationStatus Status { get; set; }
        }
        
        public enum MigrationStatus
        {
            Success,
            Failed,
            Rollback
        }
    }
    
    // ===== FEATURE FLAGS =====
    namespace FeatureFlags
    {
        public interface IFeatureFlagService
        {
            Task<bool> IsFeatureEnabledAsync(string featureName, string userId = null);
            Task<bool> IsFeatureEnabledForPercentageAsync(string featureName, string userId);
            Task<bool> IsFeatureEnabledForEnvironmentAsync(string featureName, string environment);
            Task<FeatureFlag> GetFeatureFlagAsync(string featureName);
            Task<List<FeatureFlag>> GetAllFeatureFlagsAsync();
            Task<FeatureFlag> CreateFeatureFlagAsync(FeatureFlag featureFlag);
            Task<FeatureFlag> UpdateFeatureFlagAsync(FeatureFlag featureFlag);
            Task<bool> DeleteFeatureFlagAsync(string featureName);
        }
        
        public class FeatureFlagService : IFeatureFlagService
        {
            private readonly ILogger<FeatureFlagService> _logger;
            private readonly ApplicationDbContext _context;
            private readonly IMemoryCache _cache;
            private readonly string _environment;
            
            public FeatureFlagService(
                ILogger<FeatureFlagService> logger,
                ApplicationDbContext context,
                IMemoryCache cache,
                IConfiguration configuration)
            {
                _logger = logger;
                _context = context;
                _cache = cache;
                _environment = configuration["Environment"] ?? "Development";
            }
            
            public async Task<bool> IsFeatureEnabledAsync(string featureName, string userId = null)
            {
                var featureFlag = await GetFeatureFlagAsync(featureName);
                
                if (featureFlag == null)
                {
                    return false;
                }
                
                if (!featureFlag.IsEnabled)
                {
                    return false;
                }
                
                if (featureFlag.EnvironmentRestrictions.Any() && 
                    !featureFlag.EnvironmentRestrictions.Contains(_environment))
                {
                    return false;
                }
                
                if (featureFlag.UserRestrictions.Any() && userId != null)
                {
                    return featureFlag.UserRestrictions.Contains(userId);
                }
                
                return true;
            }
            
            public async Task<bool> IsFeatureEnabledForPercentageAsync(string featureName, string userId)
            {
                var featureFlag = await GetFeatureFlagAsync(featureName);
                
                if (featureFlag == null || !featureFlag.IsEnabled)
                {
                    return false;
                }
                
                if (featureFlag.RolloutPercentage <= 0)
                {
                    return false;
                }
                
                if (featureFlag.RolloutPercentage >= 100)
                {
                    return true;
                }
                
                // Simple hash-based percentage calculation
                var hash = GetHashCode(userId + featureName);
                var percentage = hash % 100;
                
                return percentage < featureFlag.RolloutPercentage;
            }
            
            public async Task<bool> IsFeatureEnabledForEnvironmentAsync(string featureName, string environment)
            {
                var featureFlag = await GetFeatureFlagAsync(featureName);
                
                if (featureFlag == null || !featureFlag.IsEnabled)
                {
                    return false;
                }
                
                if (featureFlag.EnvironmentRestrictions.Any())
                {
                    return featureFlag.EnvironmentRestrictions.Contains(environment);
                }
                
                return true;
            }
            
            public async Task<FeatureFlag> GetFeatureFlagAsync(string featureName)
            {
                var cacheKey = $"feature_flag_{featureName}";
                
                if (_cache.TryGetValue(cacheKey, out FeatureFlag cachedFlag))
                {
                    return cachedFlag;
                }
                
                var featureFlag = await _context.FeatureFlags
                    .FirstOrDefaultAsync(f => f.Name == featureName);
                
                if (featureFlag != null)
                {
                    _cache.Set(cacheKey, featureFlag, TimeSpan.FromMinutes(5));
                }
                
                return featureFlag;
            }
            
            public async Task<List<FeatureFlag>> GetAllFeatureFlagsAsync()
            {
                return await _context.FeatureFlags.ToListAsync();
            }
            
            public async Task<FeatureFlag> CreateFeatureFlagAsync(FeatureFlag featureFlag)
            {
                featureFlag.CreatedAt = DateTime.UtcNow;
                featureFlag.UpdatedAt = DateTime.UtcNow;
                
                _context.FeatureFlags.Add(featureFlag);
                await _context.SaveChangesAsync();
                
                _cache.Remove($"feature_flag_{featureFlag.Name}");
                
                _logger.LogInformation("Feature flag {FeatureName} created", featureFlag.Name);
                
                return featureFlag;
            }
            
            public async Task<FeatureFlag> UpdateFeatureFlagAsync(FeatureFlag featureFlag)
            {
                var existingFlag = await _context.FeatureFlags
                    .FirstOrDefaultAsync(f => f.Name == featureFlag.Name);
                
                if (existingFlag == null)
                {
                    throw new FeatureFlagNotFoundException($"Feature flag {featureFlag.Name} not found");
                }
                
                existingFlag.IsEnabled = featureFlag.IsEnabled;
                existingFlag.Description = featureFlag.Description;
                existingFlag.RolloutPercentage = featureFlag.RolloutPercentage;
                existingFlag.EnvironmentRestrictions = featureFlag.EnvironmentRestrictions;
                existingFlag.UserRestrictions = featureFlag.UserRestrictions;
                existingFlag.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                _cache.Remove($"feature_flag_{featureFlag.Name}");
                
                _logger.LogInformation("Feature flag {FeatureName} updated", featureFlag.Name);
                
                return existingFlag;
            }
            
            public async Task<bool> DeleteFeatureFlagAsync(string featureName)
            {
                var featureFlag = await _context.FeatureFlags
                    .FirstOrDefaultAsync(f => f.Name == featureName);
                
                if (featureFlag == null)
                {
                    return false;
                }
                
                _context.FeatureFlags.Remove(featureFlag);
                await _context.SaveChangesAsync();
                
                _cache.Remove($"feature_flag_{featureName}");
                
                _logger.LogInformation("Feature flag {FeatureName} deleted", featureName);
                
                return true;
            }
            
            private int GetHashCode(string input)
            {
                var hash = 0;
                foreach (char c in input)
                {
                    hash = ((hash << 5) - hash) + c;
                    hash = hash & hash; // Convert to 32-bit integer
                }
                return Math.Abs(hash);
            }
        }
        
        public class FeatureFlag
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public bool IsEnabled { get; set; }
            public int RolloutPercentage { get; set; }
            public List<string> EnvironmentRestrictions { get; set; }
            public List<string> UserRestrictions { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            
            public FeatureFlag()
            {
                EnvironmentRestrictions = new List<string>();
                UserRestrictions = new List<string>();
                RolloutPercentage = 100;
            }
        }
        
        public class FeatureFlagNotFoundException : Exception
        {
            public FeatureFlagNotFoundException(string message) : base(message) { }
        }
        
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
        public class FeatureFlagAttribute : Attribute
        {
            public string FeatureName { get; }
            public int RolloutPercentage { get; }
            
            public FeatureFlagAttribute(string featureName, int rolloutPercentage = 100)
            {
                FeatureName = featureName;
                RolloutPercentage = rolloutPercentage;
            }
        }
        
        public class FeatureFlagMiddleware
        {
            private readonly RequestDelegate _next;
            private readonly IFeatureFlagService _featureFlagService;
            
            public FeatureFlagMiddleware(RequestDelegate next, IFeatureFlagService featureFlagService)
            {
                _next = next;
                _featureFlagService = featureFlagService;
            }
            
            public async Task InvokeAsync(HttpContext context)
            {
                var endpoint = context.GetEndpoint();
                var featureFlagAttribute = endpoint?.Metadata?.GetMetadata<FeatureFlagAttribute>();
                
                if (featureFlagAttribute != null)
                {
                    var userId = context.User?.Identity?.Name;
                    var isEnabled = await _featureFlagService.IsFeatureEnabledAsync(
                        featureFlagAttribute.FeatureName, userId);
                    
                    if (!isEnabled)
                    {
                        context.Response.StatusCode = 404;
                        await context.Response.WriteAsync("Feature not available");
                        return;
                    }
                }
                
                await _next(context);
            }
        }
    }
    
    // ===== DEPENDENCY INJECTION =====
    namespace Infrastructure.DependencyInjection
    {
        public static class ServiceCollectionExtensions
        {
            public static IServiceCollection AddEvolutionaryArchitecture(this IServiceCollection services)
            {
                // API Versioning
                services.AddScoped<IApiVersionManager, ApiVersionManager>();
                
                // Data Migration
                services.AddScoped<IDataMigrationService, DataMigrationService>();
                
                // Feature Flags
                services.AddScoped<IFeatureFlagService, FeatureFlagService>();
                services.AddMemoryCache();
                
                return services;
            }
        }
    }
}

// Uso de Arquitectura Evolutiva
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Arquitectura Evolutiva ===\n");
        
        Console.WriteLine("Los componentes implementados incluyen:");
        Console.WriteLine("1. Versionado de APIs con múltiples versiones");
        Console.WriteLine("2. Sistema de migración de datos");
        Console.WriteLine("3. Feature flags para despliegue gradual");
        Console.WriteLine("4. Control de versiones y compatibilidad");
        Console.WriteLine("5. Migraciones reversibles");
        Console.WriteLine("6. Rollout gradual de funcionalidades");
        
        Console.WriteLine("\nBeneficios de esta arquitectura:");
        Console.WriteLine("- Evolución continua sin interrupciones");
        Console.WriteLine("- Compatibilidad hacia atrás");
        Console.WriteLine("- Despliegue gradual de cambios");
        Console.WriteLine("- Migración de datos controlada");
        Console.WriteLine("- Experimentación con features");
        Console.WriteLine("- Rollback rápido de cambios");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Implementar API Versioning
Crea un sistema de versionado para una API existente.

### Ejercicio 2: Sistema de Migraciones
Implementa migraciones de datos para cambios de esquema.

### Ejercicio 3: Feature Flags Avanzados
Crea feature flags con lógica de negocio compleja.

## 🔍 Puntos Clave

1. **API Versioning** permite evolucionar APIs sin romper compatibilidad
2. **Migración de datos** facilita cambios de esquema controlados
3. **Feature flags** permiten despliegue gradual y experimentación
4. **Arquitectura evolutiva** mantiene sistemas actualizados
5. **Rollback** proporciona seguridad en cambios

## 📚 Recursos Adicionales

- [API Versioning](https://docs.microsoft.com/en-us/aspnet/core/web-api/versioning)
- [Feature Flags](https://martinfowler.com/articles/feature-toggles.html)
- [Evolutionary Architecture](https://martinfowler.com/articles/evolutionary-architecture.html)

---

**🎯 ¡Has completado la Clase 8! Ahora comprendes Arquitectura Evolutiva**

**📚 [Siguiente: Clase 9 - Arquitectura de Seguridad Enterprise](clase_9_seguridad_enterprise.md)**
