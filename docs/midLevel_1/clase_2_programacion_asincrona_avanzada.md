# 🚀 Clase 2: Programación Asíncrona Avanzada

## 📋 Información de la Clase

- **Módulo**: Mid Level 1 - Programación Avanzada y Patrones de Diseño
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 1 (Patrones de Diseño Intermedios)

## 🎯 Objetivos de Aprendizaje

- Dominar técnicas avanzadas de programación asíncrona en C#
- Implementar patrones async/await avanzados
- Utilizar ConfigureAwait para control del contexto
- Implementar cancelación con CancellationToken
- Crear y usar async streams para procesamiento de secuencias

---

## 📚 Navegación del Módulo 4

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_patrones_diseno_intermedios.md) | Patrones de Diseño Intermedios | ← Anterior |
| **Clase 2** | **Programación Asíncrona Avanzada** | ← Estás aquí |
| [Clase 3](clase_3_programacion_paralela.md) | Programación Paralela y TPL | Siguiente → |
| [Clase 4](clase_4_clean_architecture.md) | Clean Architecture | |
| [Clase 5](clase_5_dependency_injection.md) | Dependency Injection Avanzada | |
| [Clase 6](clase_6_logging_monitoreo.md) | Logging y Monitoreo | |
| [Clase 7](clase_7_refactoring_clean_code.md) | Refactoring y Clean Code | |
| [Clase 8](clase_8_testing_integracion.md) | Testing de Integración | |
| [Clase 9](clase_9_testing_comportamiento.md) | Testing de Comportamiento (BDD) | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de E-commerce | |

**← [Volver al README del Módulo 4](../midLevel_1/README.md)**

---

## 📚 Contenido Teórico

### 1. Async/Await Avanzado con Patrones

La programación asíncrona avanzada permite crear sistemas más eficientes y responsivos.

```csharp
// Servicio de procesamiento de archivos asíncrono
public class FileProcessingService
{
    private readonly SemaphoreSlim _semaphore;
    private readonly int _maxConcurrentOperations;
    
    public FileProcessingService(int maxConcurrentOperations = 3)
    {
        _maxConcurrentOperations = maxConcurrentOperations;
        _semaphore = new SemaphoreSlim(maxConcurrentOperations, maxConcurrentOperations);
    }
    
    // Procesamiento asíncrono con control de concurrencia
    public async Task<List<string>> ProcessFilesAsync(IEnumerable<string> filePaths)
    {
        var tasks = filePaths.Select(filePath => ProcessFileWithSemaphoreAsync(filePath));
        var results = await Task.WhenAll(tasks);
        return results.ToList();
    }
    
    // Procesamiento con semáforo para limitar operaciones concurrentes
    private async Task<string> ProcessFileWithSemaphoreAsync(string filePath)
    {
        await _semaphore.WaitAsync();
        try
        {
            return await ProcessFileAsync(filePath);
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    // Procesamiento individual de archivo
    private async Task<string> ProcessFileAsync(string filePath)
    {
        Console.WriteLine($"Iniciando procesamiento de: {filePath}");
        
        // Simular trabajo asíncrono
        await Task.Delay(Random.Shared.Next(1000, 3000));
        
        var result = $"Archivo procesado: {Path.GetFileName(filePath)} - {DateTime.Now:HH:mm:ss}";
        Console.WriteLine($"Completado: {result}");
        
        return result;
    }
    
    // Procesamiento con timeout
    public async Task<string> ProcessFileWithTimeoutAsync(string filePath, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        
        try
        {
            return await ProcessFileAsync(filePath).WaitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            return $"Timeout procesando archivo: {Path.GetFileName(filePath)}";
        }
    }
    
    // Procesamiento con retry automático
    public async Task<string> ProcessFileWithRetryAsync(string filePath, int maxRetries = 3)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await ProcessFileAsync(filePath);
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                Console.WriteLine($"Intento {attempt} falló para {filePath}: {ex.Message}");
                await Task.Delay(1000 * attempt); // Backoff exponencial
            }
        }
        
        throw new InvalidOperationException($"No se pudo procesar {filePath} después de {maxRetries} intentos");
    }
}

// Servicio de descarga asíncrona con progreso
public class DownloadService
{
    public async Task DownloadFileAsync(string url, string destinationPath, 
        IProgress<int> progress = null, CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync(url, cancellationToken);
        
        var totalBytes = response.Content.Headers.ContentLength ?? -1;
        var downloadedBytes = 0L;
        
        using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var fileStream = File.Create(destinationPath);
        
        var buffer = new byte[8192];
        int bytesRead;
        
        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
        {
            await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
            
            downloadedBytes += bytesRead;
            if (totalBytes > 0 && progress != null)
            {
                var percentage = (int)((downloadedBytes * 100) / totalBytes);
                progress.Report(percentage);
            }
        }
    }
    
    // Descarga múltiple con límite de concurrencia
    public async Task DownloadMultipleFilesAsync(Dictionary<string, string> urlToPathMapping, 
        int maxConcurrentDownloads = 3, IProgress<int> overallProgress = null)
    {
        var semaphore = new SemaphoreSlim(maxConcurrentDownloads, maxConcurrentDownloads);
        var tasks = new List<Task>();
        var completedDownloads = 0;
        var totalDownloads = urlToPathMapping.Count;
        
        foreach (var kvp in urlToPathMapping)
        {
            var task = DownloadFileWithSemaphoreAsync(kvp.Key, kvp.Value, semaphore, 
                () => 
                {
                    completedDownloads++;
                    overallProgress?.Report((completedDownloads * 100) / totalDownloads);
                });
            
            tasks.Add(task);
        }
        
        await Task.WhenAll(tasks);
    }
    
    private async Task DownloadFileWithSemaphoreAsync(string url, string destinationPath, 
        SemaphoreSlim semaphore, Action onComplete)
    {
        await semaphore.WaitAsync();
        try
        {
            await DownloadFileAsync(url, destinationPath);
            onComplete();
        }
        finally
        {
            semaphore.Release();
        }
    }
}

// Uso de los servicios asíncronos
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Programación Asíncrona Avanzada ===\n");
        
        // 1. Procesamiento de archivos con control de concurrencia
        Console.WriteLine("1. Procesamiento de Archivos con Control de Concurrencia:");
        var fileService = new FileProcessingService(maxConcurrentOperations: 2);
        
        var filePaths = new[]
        {
            "archivo1.txt", "archivo2.txt", "archivo3.txt", 
            "archivo4.txt", "archivo5.txt"
        };
        
        var startTime = DateTime.Now;
        var results = await fileService.ProcessFilesAsync(filePaths);
        var endTime = DateTime.Now;
        
        Console.WriteLine($"\nProcesamiento completado en: {(endTime - startTime).TotalSeconds:F2} segundos");
        Console.WriteLine($"Archivos procesados: {results.Count}");
        
        // 2. Procesamiento con timeout
        Console.WriteLine("\n2. Procesamiento con Timeout:");
        var timeoutResult = await fileService.ProcessFileWithTimeoutAsync("archivo_lento.txt", TimeSpan.FromSeconds(2));
        Console.WriteLine($"Resultado: {timeoutResult}");
        
        // 3. Procesamiento con retry
        Console.WriteLine("\n3. Procesamiento con Retry:");
        try
        {
            var retryResult = await fileService.ProcessFileWithRetryAsync("archivo_problematico.txt", maxRetries: 2);
            Console.WriteLine($"Resultado: {retryResult}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error después de retries: {ex.Message}");
        }
        
        // 4. Descarga con progreso
        Console.WriteLine("\n4. Descarga con Progreso:");
        var downloadService = new DownloadService();
        
        var progress = new Progress<int>(percentage => 
        {
            Console.Write($"\rProgreso: {percentage}%");
            if (percentage == 100) Console.WriteLine();
        });
        
        var overallProgress = new Progress<int>(percentage => 
        {
            Console.Write($"\rProgreso General: {percentage}%");
            if (percentage == 100) Console.WriteLine();
        });
        
        // Simular descarga de archivos
        var downloads = new Dictionary<string, string>
        {
            { "https://ejemplo.com/archivo1.zip", "descargas/archivo1.zip" },
            { "https://ejemplo.com/archivo2.zip", "descargas/archivo2.zip" },
            { "https://ejemplo.com/archivo3.zip", "descargas/archivo3.zip" }
        };
        
        try
        {
            await downloadService.DownloadMultipleFilesAsync(downloads, 2, overallProgress);
            Console.WriteLine("Todas las descargas completadas");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en descargas: {ex.Message}");
        }
    }
}
```

### 2. ConfigureAwait y Control del Contexto

ConfigureAwait permite controlar cómo se maneja el contexto de sincronización.

```csharp
// Servicio que demuestra el uso de ConfigureAwait
public class ConfigureAwaitService
{
    private readonly SynchronizationContext _originalContext;
    
    public ConfigureAwaitService()
    {
        _originalContext = SynchronizationContext.Current;
    }
    
    // Método que preserva el contexto de sincronización
    public async Task<string> PreserveContextAsync()
    {
        Console.WriteLine($"Thread ID antes de await: {Thread.CurrentThread.ManagedThreadId}");
        Console.WriteLine($"SynchronizationContext: {SynchronizationContext.Current?.GetType().Name ?? "null"}");
        
        await Task.Delay(100); // Preserva el contexto
        
        Console.WriteLine($"Thread ID después de await: {Thread.CurrentThread.ManagedThreadId}");
        Console.WriteLine($"SynchronizationContext: {SynchronizationContext.Current?.GetType().Name ?? "null"}");
        
        return "Contexto preservado";
    }
    
    // Método que no preserva el contexto de sincronización
    public async Task<string> DontPreserveContextAsync()
    {
        Console.WriteLine($"Thread ID antes de await: {Thread.CurrentThread.ManagedThreadId}");
        Console.WriteLine($"SynchronizationContext: {SynchronizationContext.Current?.GetType().Name ?? "null"}");
        
        await Task.Delay(100).ConfigureAwait(false); // No preserva el contexto
        
        Console.WriteLine($"Thread ID después de await: {Thread.CurrentThread.ManagedThreadId}");
        Console.WriteLine($"SynchronizationContext: {SynchronizationContext.Current?.GetType().Name ?? "null"}");
        
        return "Contexto no preservado";
    }
    
    // Método que demuestra el impacto en aplicaciones de UI
    public async Task UpdateUIAsync()
    {
        // Simular trabajo en background
        await Task.Delay(1000).ConfigureAwait(false);
        
        // En una aplicación real de UI, esto podría causar problemas
        // si no se usa ConfigureAwait(false) en métodos de background
        Console.WriteLine("Actualizando UI desde thread de background");
        
        // Para actualizar UI, se necesitaría volver al contexto original
        if (_originalContext != null)
        {
            _originalContext.Post(_ => 
            {
                Console.WriteLine("Volviendo al contexto original para actualizar UI");
            }, null);
        }
    }
    
    // Método que demuestra el uso correcto en diferentes escenarios
    public async Task<string> CorrectConfigureAwaitUsageAsync()
    {
        // Para operaciones de CPU intensivas o I/O
        var result1 = await PerformIOOperationAsync().ConfigureAwait(false);
        
        // Para operaciones que necesitan volver al contexto original
        var result2 = await PerformUIUpdateAsync(); // Sin ConfigureAwait
        
        return $"{result1} - {result2}";
    }
    
    private async Task<string> PerformIOOperationAsync()
    {
        await Task.Delay(100); // Simular I/O
        return "I/O completado";
    }
    
    private async Task<string> PerformUIUpdateAsync()
    {
        await Task.Delay(100); // Simular operación que necesita contexto original
        return "UI actualizada";
    }
}

// Servicio de procesamiento de datos con ConfigureAwait
public class DataProcessingService
{
    // Procesamiento en background sin preservar contexto
    public async Task<List<int>> ProcessDataInBackgroundAsync(IEnumerable<int> data)
    {
        var results = new List<int>();
        
        foreach (var item in data)
        {
            // Usar ConfigureAwait(false) para operaciones de background
            var processed = await ProcessItemAsync(item).ConfigureAwait(false);
            results.Add(processed);
        }
        
        return results;
    }
    
    // Procesamiento que necesita volver al contexto original
    public async Task<List<int>> ProcessDataWithContextAsync(IEnumerable<int> data)
    {
        var results = new List<int>();
        
        foreach (var item in data)
        {
            // Preservar contexto para operaciones que lo necesiten
            var processed = await ProcessItemAsync(item);
            results.Add(processed);
        }
        
        return results;
    }
    
    private async Task<int> ProcessItemAsync(int item)
    {
        await Task.Delay(10); // Simular procesamiento
        return item * 2;
    }
}

// Uso de ConfigureAwait
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== ConfigureAwait y Control del Contexto ===\n");
        
        var service = new ConfigureAwaitService();
        
        // 1. Preservar contexto
        Console.WriteLine("1. Preservando Contexto:");
        await service.PreserveContextAsync();
        
        Console.WriteLine();
        
        // 2. No preservar contexto
        Console.WriteLine("2. No Preservando Contexto:");
        await service.DontPreserveContextAsync();
        
        Console.WriteLine();
        
        // 3. Uso correcto de ConfigureAwait
        Console.WriteLine("3. Uso Correcto de ConfigureAwait:");
        var result = await service.CorrectConfigureAwaitUsageAsync();
        Console.WriteLine($"Resultado: {result}");
        
        Console.WriteLine();
        
        // 4. Procesamiento de datos
        Console.WriteLine("4. Procesamiento de Datos:");
        var dataService = new DataProcessingService();
        
        var data = Enumerable.Range(1, 10);
        
        var backgroundResults = await dataService.ProcessDataInBackgroundAsync(data);
        Console.WriteLine($"Procesamiento en background: {string.Join(", ", backgroundResults)}");
        
        var contextResults = await dataService.ProcessDataWithContextAsync(data);
        Console.WriteLine($"Procesamiento con contexto: {string.Join(", ", contextResults)}");
    }
}
```

### 3. CancellationToken y Cancelación de Operaciones

CancellationToken permite cancelar operaciones asíncronas de forma cooperativa.

```csharp
// Servicio de procesamiento con cancelación
public class CancellableProcessingService
{
    // Procesamiento con cancelación
    public async Task<List<string>> ProcessWithCancellationAsync(
        IEnumerable<string> items, 
        CancellationToken cancellationToken)
    {
        var results = new List<string>();
        
        foreach (var item in items)
        {
            // Verificar cancelación antes de cada operación
            cancellationToken.ThrowIfCancellationRequested();
            
            var result = await ProcessItemWithCancellationAsync(item, cancellationToken);
            results.Add(result);
        }
        
        return results;
    }
    
    // Procesamiento individual con cancelación
    private async Task<string> ProcessItemWithCancellationAsync(string item, CancellationToken cancellationToken)
    {
        // Simular trabajo que puede ser cancelado
        await Task.Delay(500, cancellationToken);
        
        return $"Procesado: {item}";
    }
    
    // Procesamiento con timeout automático
    public async Task<List<string>> ProcessWithTimeoutAsync(
        IEnumerable<string> items, 
        TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        return await ProcessWithCancellationAsync(items, cts.Token);
    }
    
    // Procesamiento con múltiples tokens de cancelación
    public async Task<List<string>> ProcessWithMultipleTokensAsync(
        IEnumerable<string> items,
        CancellationToken userCancellation,
        CancellationToken systemCancellation)
    {
        // Combinar múltiples tokens
        using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(
            userCancellation, systemCancellation);
        
        return await ProcessWithCancellationAsync(items, combinedCts.Token);
    }
    
    // Procesamiento con cancelación condicional
    public async Task<List<string>> ProcessWithConditionalCancellationAsync(
        IEnumerable<string> items,
        Func<string, bool> shouldCancel,
        CancellationToken cancellationToken)
    {
        var results = new List<string>();
        
        foreach (var item in items)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            // Cancelación condicional basada en el contenido
            if (shouldCancel(item))
            {
                Console.WriteLine($"Cancelando procesamiento de: {item}");
                break;
            }
            
            var result = await ProcessItemWithCancellationAsync(item, cancellationToken);
            results.Add(result);
        }
        
        return results;
    }
}

// Servicio de descarga con cancelación
public class CancellableDownloadService
{
    public async Task DownloadWithCancellationAsync(
        string url, 
        string destinationPath, 
        CancellationToken cancellationToken,
        IProgress<int> progress = null)
    {
        using var httpClient = new HttpClient();
        
        // Configurar timeout en el HttpClient
        httpClient.Timeout = TimeSpan.FromMinutes(5);
        
        using var response = await httpClient.GetAsync(url, cancellationToken);
        var totalBytes = response.Content.Headers.ContentLength ?? -1;
        var downloadedBytes = 0L;
        
        using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var fileStream = File.Create(destinationPath);
        
        var buffer = new byte[8192];
        int bytesRead;
        
        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
        {
            await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
            
            downloadedBytes += bytesRead;
            if (totalBytes > 0 && progress != null)
            {
                var percentage = (int)((downloadedBytes * 100) / totalBytes);
                progress.Report(percentage);
            }
        }
    }
    
    // Descarga múltiple con cancelación
    public async Task DownloadMultipleWithCancellationAsync(
        Dictionary<string, string> urlToPathMapping,
        CancellationToken cancellationToken,
        int maxConcurrentDownloads = 3)
    {
        var semaphore = new SemaphoreSlim(maxConcurrentDownloads, maxConcurrentDownloads);
        var tasks = new List<Task>();
        
        foreach (var kvp in urlToPathMapping)
        {
            var task = DownloadWithSemaphoreAsync(kvp.Key, kvp.Value, semaphore, cancellationToken);
            tasks.Add(task);
        }
        
        // Esperar todas las tareas o cancelación
        await Task.WhenAll(tasks).WaitAsync(cancellationToken);
    }
    
    private async Task DownloadWithSemaphoreAsync(
        string url, 
        string destinationPath, 
        SemaphoreSlim semaphore,
        CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            await DownloadWithCancellationAsync(url, destinationPath, cancellationToken);
        }
        finally
        {
            semaphore.Release();
        }
    }
}

// Uso de cancelación
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== CancellationToken y Cancelación de Operaciones ===\n");
        
        var processingService = new CancellableProcessingService();
        var downloadService = new CancellableDownloadService();
        
        // 1. Procesamiento con cancelación manual
        Console.WriteLine("1. Procesamiento con Cancelación Manual:");
        using var cts = new CancellationTokenSource();
        
        var processingTask = Task.Run(async () =>
        {
            try
            {
                var items = Enumerable.Range(1, 20).Select(i => $"Item_{i}");
                var results = await processingService.ProcessWithCancellationAsync(items, cts.Token);
                Console.WriteLine($"Procesamiento completado: {results.Count} items");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Procesamiento cancelado por el usuario");
            }
        });
        
        // Cancelar después de 2 segundos
        await Task.Delay(2000);
        cts.Cancel();
        
        try
        {
            await processingTask;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Tarea cancelada exitosamente");
        }
        
        Console.WriteLine();
        
        // 2. Procesamiento con timeout
        Console.WriteLine("2. Procesamiento con Timeout:");
        try
        {
            var items = Enumerable.Range(1, 10).Select(i => $"Item_{i}");
            var results = await processingService.ProcessWithTimeoutAsync(items, TimeSpan.FromSeconds(1));
            Console.WriteLine($"Procesamiento con timeout completado: {results.Count} items");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Procesamiento cancelado por timeout");
        }
        
        Console.WriteLine();
        
        // 3. Cancelación condicional
        Console.WriteLine("3. Cancelación Condicional:");
        try
        {
            var items = new[] { "Item_A", "Item_B", "Item_C", "Item_D", "Item_E" };
            var results = await processingService.ProcessWithConditionalCancellationAsync(
                items, 
                item => item.Contains("C"), // Cancelar cuando encuentre "C"
                CancellationToken.None);
            
            Console.WriteLine($"Procesamiento condicional completado: {results.Count} items");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en procesamiento condicional: {ex.Message}");
        }
        
        Console.WriteLine();
        
        // 4. Descarga con cancelación
        Console.WriteLine("4. Descarga con Cancelación:");
        using var downloadCts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        
        var progress = new Progress<int>(percentage => 
        {
            Console.Write($"\rProgreso de descarga: {percentage}%");
            if (percentage == 100) Console.WriteLine();
        });
        
        try
        {
            // Simular descarga (en realidad no se descargará nada)
            await downloadService.DownloadWithCancellationAsync(
                "https://ejemplo.com/archivo.zip",
                "descargas/archivo.zip",
                downloadCts.Token,
                progress);
            
            Console.WriteLine("Descarga completada");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\nDescarga cancelada por timeout");
        }
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Sistema de Procesamiento de Imágenes Asíncrono
Implementa un sistema que procese múltiples imágenes de forma asíncrona con control de concurrencia y cancelación.

### Ejercicio 2: API Client con Retry y Timeout
Crea un cliente HTTP que implemente retry automático, timeout configurable y cancelación de operaciones.

### Ejercicio 3: Pipeline de Procesamiento de Datos
Desarrolla un pipeline asíncrono que procese datos en etapas con capacidad de cancelación en cualquier punto.

## 🔍 Puntos Clave

1. **ConfigureAwait(false)** mejora el rendimiento en operaciones de background
2. **CancellationToken** permite cancelación cooperativa de operaciones asíncronas
3. **SemaphoreSlim** controla la concurrencia en operaciones asíncronas
4. **Task.WhenAll** ejecuta múltiples tareas en paralelo
5. **Async streams** permiten procesamiento asíncrono de secuencias de datos

## 📚 Recursos Adicionales

- [Async/Await Best Practices - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/standard/async-in-depth)
- [ConfigureAwait FAQ - Stephen Toub](https://devblogs.microsoft.com/dotnet/configureawait-faq/)
- [Cancellation in Managed Threads - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads)

---

**🎯 ¡Has completado la Clase 2! Ahora dominas la programación asíncrona avanzada en C#**

**📚 [Siguiente: Clase 3 - Programación Paralela y TPL](clase_3_programacion_paralela.md)**
