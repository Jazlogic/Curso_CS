# 💾 **Clase 6: Cloud Storage y CDN**

## 🎯 **Objetivo de la Clase**
Dominar el almacenamiento en la nube y Content Delivery Networks (CDN), implementando soluciones escalables para archivos, imágenes y contenido multimedia.

## 📚 **Contenido Teórico**

### **1. Azure Blob Storage**

#### **Configuración de Blob Storage**
```csharp
// Services/BlobStorageService.cs
public class BlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(IConfiguration configuration, ILogger<BlobStorageService> logger)
    {
        var connectionString = configuration.GetConnectionString("BlobStorage");
        _blobServiceClient = new BlobServiceClient(connectionString);
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName, string contentType = null)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            var blobName = GenerateBlobName(fileName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var blobHttpHeaders = new BlobHttpHeaders();
            if (!string.IsNullOrEmpty(contentType))
            {
                blobHttpHeaders.ContentType = contentType;
            }

            var blobUploadOptions = new BlobUploadOptions
            {
                HttpHeaders = blobHttpHeaders,
                Metadata = new Dictionary<string, string>
                {
                    { "UploadedAt", DateTime.UtcNow.ToString("O") },
                    { "OriginalFileName", fileName }
                }
            };

            await blobClient.UploadAsync(fileStream, blobUploadOptions);
            
            _logger.LogInformation("File uploaded successfully: {BlobName}", blobName);
            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", fileName);
            throw;
        }
    }

    public async Task<Stream> DownloadFileAsync(string blobUrl)
    {
        try
        {
            var blobClient = new BlobClient(new Uri(blobUrl));
            var response = await blobClient.DownloadStreamingAsync();
            return response.Value.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file: {BlobUrl}", blobUrl);
            throw;
        }
    }

    public async Task<string> GenerateSasTokenAsync(string blobUrl, TimeSpan expiration)
    {
        try
        {
            var blobClient = new BlobClient(new Uri(blobUrl));
            
            if (await blobClient.CanGenerateSasUriAsync())
            {
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = blobClient.BlobContainerName,
                    BlobName = blobClient.Name,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.UtcNow.Add(expiration)
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);
                
                var sasUri = blobClient.GenerateSasUri(sasBuilder);
                return sasUri.ToString();
            }

            return blobUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SAS token for: {BlobUrl}", blobUrl);
            throw;
        }
    }

    public async Task DeleteFileAsync(string blobUrl)
    {
        try
        {
            var blobClient = new BlobClient(new Uri(blobUrl));
            await blobClient.DeleteIfExistsAsync();
            
            _logger.LogInformation("File deleted successfully: {BlobUrl}", blobUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {BlobUrl}", blobUrl);
            throw;
        }
    }

    public async Task<List<BlobItem>> ListFilesAsync(string containerName, string prefix = null)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobs = new List<BlobItem>();

            await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: prefix))
            {
                blobs.Add(blobItem);
            }

            return blobs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files in container: {ContainerName}", containerName);
            throw;
        }
    }

    private string GenerateBlobName(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var sanitizedName = SanitizeFileName(nameWithoutExtension);
        
        return $"{DateTime.UtcNow:yyyy/MM/dd}/{sanitizedName}_{Guid.NewGuid()}{extension}";
    }

    private string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }
}
```

#### **Servicio de Imágenes**
```csharp
// Services/ImageProcessingService.cs
public class ImageProcessingService
{
    private readonly BlobStorageService _blobStorageService;
    private readonly ILogger<ImageProcessingService> _logger;

    public ImageProcessingService(BlobStorageService blobStorageService, ILogger<ImageProcessingService> logger)
    {
        _blobStorageService = blobStorageService;
        _logger = logger;
    }

    public async Task<string> ProcessAndUploadImageAsync(Stream imageStream, string fileName, ImageProcessingOptions options)
    {
        try
        {
            // Procesar imagen
            var processedImage = await ProcessImageAsync(imageStream, options);
            
            // Subir imagen procesada
            var blobUrl = await _blobStorageService.UploadFileAsync(
                processedImage, 
                fileName, 
                "processed-images", 
                "image/jpeg");

            return blobUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing and uploading image: {FileName}", fileName);
            throw;
        }
    }

    public async Task<string> CreateThumbnailAsync(string originalImageUrl, int width, int height)
    {
        try
        {
            // Descargar imagen original
            var originalStream = await _blobStorageService.DownloadFileAsync(originalImageUrl);
            
            // Crear thumbnail
            var thumbnailStream = await CreateThumbnailAsync(originalStream, width, height);
            
            // Subir thumbnail
            var thumbnailUrl = await _blobStorageService.UploadFileAsync(
                thumbnailStream, 
                $"thumb_{width}x{height}_{Path.GetFileName(originalImageUrl)}", 
                "thumbnails", 
                "image/jpeg");

            return thumbnailUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating thumbnail for: {OriginalImageUrl}", originalImageUrl);
            throw;
        }
    }

    private async Task<Stream> ProcessImageAsync(Stream imageStream, ImageProcessingOptions options)
    {
        // Implementar procesamiento de imagen
        // Redimensionar, aplicar filtros, etc.
        return imageStream;
    }

    private async Task<Stream> CreateThumbnailAsync(Stream imageStream, int width, int height)
    {
        // Implementar creación de thumbnail
        return imageStream;
    }
}

// Models/ImageProcessingOptions.cs
public class ImageProcessingOptions
{
    public int? MaxWidth { get; set; }
    public int? MaxHeight { get; set; }
    public int Quality { get; set; } = 85;
    public bool CreateThumbnail { get; set; } = true;
    public int ThumbnailWidth { get; set; } = 300;
    public int ThumbnailHeight { get; set; } = 300;
}
```

### **2. Amazon S3**

#### **Servicio de S3**
```csharp
// Services/S3StorageService.cs
public class S3StorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3StorageService> _logger;

    public S3StorageService(IAmazonS3 s3Client, ILogger<S3StorageService> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string bucketName, string contentType = null)
    {
        try
        {
            var key = GenerateS3Key(fileName);
            
            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                InputStream = fileStream,
                ContentType = contentType ?? GetContentType(fileName),
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256,
                Metadata = new Dictionary<string, string>
                {
                    { "UploadedAt", DateTime.UtcNow.ToString("O") },
                    { "OriginalFileName", fileName }
                }
            };

            await _s3Client.PutObjectAsync(request);
            
            var fileUrl = $"https://{bucketName}.s3.amazonaws.com/{key}";
            _logger.LogInformation("File uploaded successfully to S3: {Key}", key);
            
            return fileUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to S3: {FileName}", fileName);
            throw;
        }
    }

    public async Task<Stream> DownloadFileAsync(string bucketName, string key)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            var response = await _s3Client.GetObjectAsync(request);
            return response.ResponseStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from S3: {Key}", key);
            throw;
        }
    }

    public async Task<string> GeneratePresignedUrlAsync(string bucketName, string key, TimeSpan expiration)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = key,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.Add(expiration)
            };

            return await _s3Client.GetPreSignedURLAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned URL for: {Key}", key);
            throw;
        }
    }

    public async Task DeleteFileAsync(string bucketName, string key)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(request);
            _logger.LogInformation("File deleted successfully from S3: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from S3: {Key}", key);
            throw;
        }
    }

    public async Task<List<S3Object>> ListFilesAsync(string bucketName, string prefix = null)
    {
        try
        {
            var request = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = prefix
            };

            var response = await _s3Client.ListObjectsV2Async(request);
            return response.S3Objects;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files in S3 bucket: {BucketName}", bucketName);
            throw;
        }
    }

    private string GenerateS3Key(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var sanitizedName = SanitizeFileName(nameWithoutExtension);
        
        return $"uploads/{DateTime.UtcNow:yyyy/MM/dd}/{sanitizedName}_{Guid.NewGuid()}{extension}";
    }

    private string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".mp3" => "audio/mpeg",
            ".mp4" => "video/mp4",
            _ => "application/octet-stream"
        };
    }
}
```

### **3. Azure CDN**

#### **Configuración de CDN**
```csharp
// Services/CDNService.cs
public class CDNService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CDNService> _logger;

    public CDNService(IConfiguration configuration, ILogger<CDNService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string GetCDNUrl(string originalUrl)
    {
        var cdnEndpoint = _configuration["CDN:Endpoint"];
        
        if (string.IsNullOrEmpty(cdnEndpoint))
        {
            return originalUrl;
        }

        // Extraer el path del blob URL
        var uri = new Uri(originalUrl);
        var cdnUrl = $"{cdnEndpoint}{uri.AbsolutePath}";
        
        _logger.LogInformation("CDN URL generated: {CdnUrl}", cdnUrl);
        return cdnUrl;
    }

    public async Task PurgeCacheAsync(string url)
    {
        try
        {
            var cdnEndpoint = _configuration["CDN:Endpoint"];
            var subscriptionId = _configuration["CDN:SubscriptionId"];
            var resourceGroupName = _configuration["CDN:ResourceGroupName"];
            var profileName = _configuration["CDN:ProfileName"];
            var endpointName = _configuration["CDN:EndpointName"];

            // Implementar purga de cache usando Azure SDK
            // Esto requeriría configurar Azure SDK para CDN
            
            _logger.LogInformation("Cache purged for URL: {Url}", url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purging cache for URL: {Url}", url);
            throw;
        }
    }

    public string GetOptimizedImageUrl(string imageUrl, ImageOptimizationOptions options)
    {
        var cdnUrl = GetCDNUrl(imageUrl);
        
        // Agregar parámetros de optimización
        var queryParams = new List<string>();
        
        if (options.Width.HasValue)
            queryParams.Add($"w={options.Width}");
        
        if (options.Height.HasValue)
            queryParams.Add($"h={options.Height}");
        
        if (options.Quality.HasValue)
            queryParams.Add($"q={options.Quality}");
        
        if (options.Format != null)
            queryParams.Add($"f={options.Format}");

        if (queryParams.Any())
        {
            cdnUrl += "?" + string.Join("&", queryParams);
        }

        return cdnUrl;
    }
}

// Models/ImageOptimizationOptions.cs
public class ImageOptimizationOptions
{
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? Quality { get; set; }
    public string Format { get; set; }
}
```

### **4. CloudFront (AWS CDN)**

#### **Configuración de CloudFront**
```csharp
// Services/CloudFrontService.cs
public class CloudFrontService
{
    private readonly IAmazonCloudFront _cloudFrontClient;
    private readonly ILogger<CloudFrontService> _logger;

    public CloudFrontService(IAmazonCloudFront cloudFrontClient, ILogger<CloudFrontService> logger)
    {
        _cloudFrontClient = cloudFrontClient;
        _logger = logger;
    }

    public string GetCloudFrontUrl(string s3Url, string distributionDomain)
    {
        try
        {
            var uri = new Uri(s3Url);
            var path = uri.AbsolutePath;
            
            var cloudFrontUrl = $"https://{distributionDomain}{path}";
            
            _logger.LogInformation("CloudFront URL generated: {CloudFrontUrl}", cloudFrontUrl);
            return cloudFrontUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating CloudFront URL for: {S3Url}", s3Url);
            return s3Url;
        }
    }

    public async Task CreateInvalidationAsync(string distributionId, string path)
    {
        try
        {
            var request = new CreateInvalidationRequest
            {
                DistributionId = distributionId,
                InvalidationBatch = new InvalidationBatch
                {
                    Paths = new Paths
                    {
                        Quantity = 1,
                        Items = new List<string> { path }
                    },
                    CallerReference = Guid.NewGuid().ToString()
                }
            };

            var response = await _cloudFrontClient.CreateInvalidationAsync(request);
            
            _logger.LogInformation("Invalidation created: {InvalidationId}", response.Invalidation.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invalidation for path: {Path}", path);
            throw;
        }
    }
}
```

### **5. Gestión de Archivos Multimedia**

#### **Servicio de Archivos Multimedia**
```csharp
// Services/MediaFileService.cs
public class MediaFileService
{
    private readonly BlobStorageService _blobStorageService;
    private readonly S3StorageService _s3StorageService;
    private readonly CDNService _cdnService;
    private readonly ILogger<MediaFileService> _logger;

    public MediaFileService(
        BlobStorageService blobStorageService,
        S3StorageService s3StorageService,
        CDNService cdnService,
        ILogger<MediaFileService> logger)
    {
        _blobStorageService = blobStorageService;
        _s3StorageService = s3StorageService;
        _cdnService = cdnService;
        _logger = logger;
    }

    public async Task<MediaFileResult> UploadMediaFileAsync(Stream fileStream, string fileName, MediaFileType fileType)
    {
        try
        {
            var contentType = GetContentType(fileType);
            var containerName = GetContainerName(fileType);
            
            // Subir a Azure Blob Storage
            var blobUrl = await _blobStorageService.UploadFileAsync(fileStream, fileName, containerName, contentType);
            
            // Subir a S3 (backup)
            fileStream.Position = 0;
            var s3Url = await _s3StorageService.UploadFileAsync(fileStream, fileName, "mussikon-media", contentType);
            
            // Generar URL de CDN
            var cdnUrl = _cdnService.GetCDNUrl(blobUrl);
            
            var result = new MediaFileResult
            {
                FileName = fileName,
                FileType = fileType,
                BlobUrl = blobUrl,
                S3Url = s3Url,
                CdnUrl = cdnUrl,
                ContentType = contentType,
                Size = fileStream.Length,
                UploadedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Media file uploaded successfully: {FileName}", fileName);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading media file: {FileName}", fileName);
            throw;
        }
    }

    public async Task<Stream> DownloadMediaFileAsync(string fileUrl, MediaFileType fileType)
    {
        try
        {
            // Intentar descargar desde CDN primero
            if (fileUrl.Contains("cdn"))
            {
                return await _blobStorageService.DownloadFileAsync(fileUrl);
            }
            
            // Fallback a S3
            if (fileUrl.Contains("s3"))
            {
                var uri = new Uri(fileUrl);
                var bucketName = uri.Host.Split('.')[0];
                var key = uri.AbsolutePath.TrimStart('/');
                return await _s3StorageService.DownloadFileAsync(bucketName, key);
            }
            
            // Fallback a Blob Storage
            return await _blobStorageService.DownloadFileAsync(fileUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading media file: {FileUrl}", fileUrl);
            throw;
        }
    }

    public async Task<string> GenerateThumbnailAsync(string mediaFileUrl, MediaFileType fileType)
    {
        try
        {
            if (fileType != MediaFileType.Image)
            {
                throw new InvalidOperationException("Thumbnails can only be generated for images");
            }

            var imageStream = await DownloadMediaFileAsync(mediaFileUrl, fileType);
            var thumbnailStream = await CreateThumbnailAsync(imageStream);
            
            var thumbnailUrl = await _blobStorageService.UploadFileAsync(
                thumbnailStream, 
                $"thumb_{Path.GetFileName(mediaFileUrl)}", 
                "thumbnails", 
                "image/jpeg");

            return _cdnService.GetCDNUrl(thumbnailUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating thumbnail for: {MediaFileUrl}", mediaFileUrl);
            throw;
        }
    }

    private string GetContentType(MediaFileType fileType)
    {
        return fileType switch
        {
            MediaFileType.Image => "image/jpeg",
            MediaFileType.Audio => "audio/mpeg",
            MediaFileType.Video => "video/mp4",
            MediaFileType.Document => "application/pdf",
            _ => "application/octet-stream"
        };
    }

    private string GetContainerName(MediaFileType fileType)
    {
        return fileType switch
        {
            MediaFileType.Image => "images",
            MediaFileType.Audio => "audio",
            MediaFileType.Video => "videos",
            MediaFileType.Document => "documents",
            _ => "files"
        };
    }

    private async Task<Stream> CreateThumbnailAsync(Stream imageStream)
    {
        // Implementar creación de thumbnail
        return imageStream;
    }
}

// Models/MediaFileResult.cs
public class MediaFileResult
{
    public string FileName { get; set; } = string.Empty;
    public MediaFileType FileType { get; set; }
    public string BlobUrl { get; set; } = string.Empty;
    public string S3Url { get; set; } = string.Empty;
    public string CdnUrl { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime UploadedAt { get; set; }
}

// Enums/MediaFileType.cs
public enum MediaFileType
{
    Image,
    Audio,
    Video,
    Document
}
```

## 🛠️ **Ejercicio Práctico**

### **Ejercicio 1: Implementar Sistema de Almacenamiento Completo**

Crea un sistema completo de almacenamiento en la nube:

```csharp
// 1. Configurar servicios de almacenamiento
public class StorageServiceConfig
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Azure Blob Storage
        services.AddSingleton<BlobStorageService>();
        
        // Amazon S3
        services.AddAWSService<IAmazonS3>();
        services.AddSingleton<S3StorageService>();
        
        // CDN Services
        services.AddSingleton<CDNService>();
        services.AddSingleton<CloudFrontService>();
        
        // Media File Service
        services.AddSingleton<MediaFileService>();
    }
}

// 2. Implementar controlador para archivos
[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly MediaFileService _mediaFileService;
    private readonly ILogger<MediaController> _logger;

    [HttpPost("upload")]
    public async Task<ActionResult<MediaFileResult>> UploadFile(IFormFile file, MediaFileType fileType)
    {
        using var stream = file.OpenReadStream();
        var result = await _mediaFileService.UploadMediaFileAsync(stream, file.FileName, fileType);
        return Ok(result);
    }

    [HttpGet("download/{fileId}")]
    public async Task<IActionResult> DownloadFile(string fileId)
    {
        var fileStream = await _mediaFileService.DownloadMediaFileAsync(fileId, MediaFileType.Image);
        return File(fileStream, "application/octet-stream");
    }

    [HttpPost("thumbnail")]
    public async Task<ActionResult<string>> GenerateThumbnail(string mediaFileUrl, MediaFileType fileType)
    {
        var thumbnailUrl = await _mediaFileService.GenerateThumbnailAsync(mediaFileUrl, fileType);
        return Ok(thumbnailUrl);
    }
}

// 3. Implementar servicio de optimización de imágenes
public class ImageOptimizationService
{
    private readonly CDNService _cdnService;
    private readonly ILogger<ImageOptimizationService> _logger;

    public string GetOptimizedImageUrl(string imageUrl, int? width = null, int? height = null, int? quality = null)
    {
        var options = new ImageOptimizationOptions
        {
            Width = width,
            Height = height,
            Quality = quality
        };

        return _cdnService.GetOptimizedImageUrl(imageUrl, options);
    }
}
```

## 📋 **Resumen de la Clase**

### **Conceptos Clave:**
- **Azure Blob Storage**: Almacenamiento de objetos
- **Amazon S3**: Almacenamiento en la nube
- **Azure CDN**: Content Delivery Network
- **CloudFront**: CDN de AWS
- **Media Files**: Gestión de archivos multimedia
- **Image Optimization**: Optimización de imágenes

### **Próxima Clase:**
**Cloud Security y Best Practices** - Seguridad en la nube

## 🎯 **Objetivos de Aprendizaje**

Al finalizar esta clase, serás capaz de:
- ✅ Implementar Azure Blob Storage
- ✅ Configurar Amazon S3
- ✅ Usar Azure CDN y CloudFront
- ✅ Gestionar archivos multimedia
- ✅ Optimizar imágenes con CDN
- ✅ Implementar backup y redundancia
