# 🚀 **Clase 5: Optimización de Rendimiento y Manejo de Memoria**

## 🎯 **Objetivos de la Clase**
- Optimizar el rendimiento de la aplicación
- Manejar eficientemente la memoria
- Implementar lazy loading y virtualización
- Configurar profiling y debugging
- Aplicar mejores prácticas de performance

## 📚 **Contenido Teórico**

### **1. Optimización de Rendimiento**

Factores clave de rendimiento:
- **UI Thread**: Mantener la interfaz responsiva
- **Async/Await**: Operaciones no bloqueantes
- **Caching**: Reducir operaciones costosas
- **Lazy Loading**: Cargar datos bajo demanda
- **Virtualización**: Renderizar solo elementos visibles

### **2. Manejo de Memoria**

Gestión eficiente de memoria:
- **Garbage Collection**: Liberación automática de memoria
- **Memory Leaks**: Evitar referencias circulares
- **Image Caching**: Optimizar imágenes
- **Dispose Pattern**: Liberar recursos no administrados
- **Weak References**: Referencias débiles para evitar leaks

### **3. Profiling y Debugging**

Herramientas de análisis:
- **.NET MAUI Profiler**: Análisis de rendimiento
- **Memory Profiler**: Detección de memory leaks
- **XAML Hot Reload**: Desarrollo rápido
- **Performance Counters**: Métricas en tiempo real

## 💻 **Implementación Práctica**

### **1. Lazy Loading para Listas**

```csharp
// ViewModels/LazyLoadingViewModel.cs
public class LazyLoadingViewModel : BaseViewModel
{
    private readonly IMusicianRepository _musicianRepository;
    private readonly int _pageSize = 20;
    private int _currentPage = 0;
    private bool _hasMoreItems = true;
    private bool _isLoadingMore = false;

    private ObservableCollection<Musician> _musicians;
    private MusicianSearchCriteria _searchCriteria;

    public LazyLoadingViewModel(IMusicianRepository musicianRepository)
    {
        _musicianRepository = musicianRepository;
        _musicians = new ObservableCollection<Musician>();
        _searchCriteria = new MusicianSearchCriteria();
        
        LoadMoreCommand = new Command(async () => await LoadMoreItemsAsync(), () => !IsLoadingMore && HasMoreItems);
        RefreshCommand = new Command(async () => await RefreshAsync());
    }

    public ObservableCollection<Musician> Musicians
    {
        get => _musicians;
        set => SetProperty(ref _musicians, value);
    }

    public bool HasMoreItems
    {
        get => _hasMoreItems;
        set
        {
            SetProperty(ref _hasMoreItems, value);
            LoadMoreCommand.ChangeCanExecute();
        }
    }

    public bool IsLoadingMore
    {
        get => _isLoadingMore;
        set
        {
            SetProperty(ref _isLoadingMore, value);
            LoadMoreCommand.ChangeCanExecute();
        }
    }

    public Command LoadMoreCommand { get; }
    public Command RefreshCommand { get; }

    private async Task LoadMoreItemsAsync()
    {
        if (IsLoadingMore || !HasMoreItems) return;

        try
        {
            IsLoadingMore = true;
            
            var searchCriteria = new MusicianSearchCriteria
            {
                Page = _currentPage + 1,
                PageSize = _pageSize,
                Genre = _searchCriteria.Genre,
                Location = _searchCriteria.Location
            };

            var newMusicians = await _musicianRepository.GetMusiciansAsync(searchCriteria);
            
            if (newMusicians.Any())
            {
                foreach (var musician in newMusicians)
                {
                    Musicians.Add(musician);
                }
                
                _currentPage++;
                HasMoreItems = newMusicians.Count() == _pageSize;
            }
            else
            {
                HasMoreItems = false;
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsLoadingMore = false;
        }
    }

    private async Task RefreshAsync()
    {
        try
        {
            IsBusy = true;
            
            _currentPage = 0;
            HasMoreItems = true;
            Musicians.Clear();
            
            await LoadMoreItemsAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

### **2. Image Caching Service**

```csharp
// Services/IImageCacheService.cs
public interface IImageCacheService
{
    Task<ImageSource> GetImageAsync(string url);
    Task<ImageSource> GetImageAsync(string url, int maxWidth, int maxHeight);
    Task ClearCacheAsync();
    Task<long> GetCacheSizeAsync();
    Task<bool> IsImageCachedAsync(string url);
}

// Services/ImageCacheService.cs
public class ImageCacheService : IImageCacheService
{
    private readonly HttpClient _httpClient;
    private readonly string _cacheDirectory;
    private readonly SemaphoreSlim _semaphore;
    private readonly Dictionary<string, Task<ImageSource>> _loadingTasks;

    public ImageCacheService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _cacheDirectory = Path.Combine(FileSystem.AppDataDirectory, "image_cache");
        _semaphore = new SemaphoreSlim(10, 10); // Máximo 10 descargas concurrentes
        _loadingTasks = new Dictionary<string, Task<ImageSource>>();
        
        EnsureCacheDirectoryExists();
    }

    public async Task<ImageSource> GetImageAsync(string url)
    {
        return await GetImageAsync(url, 0, 0);
    }

    public async Task<ImageSource> GetImageAsync(string url, int maxWidth, int maxHeight)
    {
        if (string.IsNullOrEmpty(url))
            return null;

        try
        {
            // Verificar si ya está cargando
            if (_loadingTasks.ContainsKey(url))
            {
                return await _loadingTasks[url];
            }

            // Crear tarea de carga
            var loadingTask = LoadImageAsync(url, maxWidth, maxHeight);
            _loadingTasks[url] = loadingTask;

            try
            {
                return await loadingTask;
            }
            finally
            {
                _loadingTasks.Remove(url);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading image {url}: {ex.Message}");
            return null;
        }
    }

    private async Task<ImageSource> LoadImageAsync(string url, int maxWidth, int maxHeight)
    {
        await _semaphore.WaitAsync();
        
        try
        {
            var cacheKey = GenerateCacheKey(url, maxWidth, maxHeight);
            var localPath = Path.Combine(_cacheDirectory, cacheKey);

            // Verificar si existe en caché
            if (File.Exists(localPath))
            {
                return ImageSource.FromFile(localPath);
            }

            // Descargar imagen
            var imageBytes = await DownloadImageAsync(url);
            if (imageBytes == null)
                return null;

            // Procesar imagen si es necesario
            if (maxWidth > 0 || maxHeight > 0)
            {
                imageBytes = await ResizeImageAsync(imageBytes, maxWidth, maxHeight);
            }

            // Guardar en caché
            await File.WriteAllBytesAsync(localPath, imageBytes);

            return ImageSource.FromFile(localPath);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<byte[]> DownloadImageAsync(string url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error downloading image {url}: {ex.Message}");
        }
        
        return null;
    }

    private async Task<byte[]> ResizeImageAsync(byte[] imageBytes, int maxWidth, int maxHeight)
    {
        // Implementar redimensionado de imagen
        // Por simplicidad, retornamos los bytes originales
        // En una implementación real, usarías una librería como SkiaSharp
        return imageBytes;
    }

    private string GenerateCacheKey(string url, int maxWidth, int maxHeight)
    {
        var hash = url.GetHashCode();
        return $"{hash}_{maxWidth}_{maxHeight}.jpg";
    }

    private void EnsureCacheDirectoryExists()
    {
        if (!Directory.Exists(_cacheDirectory))
        {
            Directory.CreateDirectory(_cacheDirectory);
        }
    }

    public async Task ClearCacheAsync()
    {
        try
        {
            if (Directory.Exists(_cacheDirectory))
            {
                Directory.Delete(_cacheDirectory, true);
                EnsureCacheDirectoryExists();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error clearing cache: {ex.Message}");
        }
    }

    public async Task<long> GetCacheSizeAsync()
    {
        try
        {
            if (!Directory.Exists(_cacheDirectory))
                return 0;

            var files = Directory.GetFiles(_cacheDirectory, "*", SearchOption.AllDirectories);
            return files.Sum(file => new FileInfo(file).Length);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting cache size: {ex.Message}");
            return 0;
        }
    }

    public async Task<bool> IsImageCachedAsync(string url)
    {
        var cacheKey = GenerateCacheKey(url, 0, 0);
        var localPath = Path.Combine(_cacheDirectory, cacheKey);
        return File.Exists(localPath);
    }
}
```

### **3. Memory Management Service**

```csharp
// Services/IMemoryManagementService.cs
public interface IMemoryManagementService
{
    Task<MemoryInfo> GetMemoryInfoAsync();
    Task ForceGarbageCollectionAsync();
    Task ClearImageCacheAsync();
    Task ClearUnusedDataAsync();
    void RegisterWeakReference(object target, string key);
    void UnregisterWeakReference(string key);
}

// Services/MemoryManagementService.cs
public class MemoryManagementService : IMemoryManagementService
{
    private readonly IImageCacheService _imageCacheService;
    private readonly Dictionary<string, WeakReference> _weakReferences;

    public MemoryManagementService(IImageCacheService imageCacheService)
    {
        _imageCacheService = imageCacheService;
        _weakReferences = new Dictionary<string, WeakReference>();
    }

    public async Task<MemoryInfo> GetMemoryInfoAsync()
    {
        var process = Process.GetCurrentProcess();
        
        return new MemoryInfo
        {
            WorkingSet = process.WorkingSet64,
            PrivateMemorySize = process.PrivateMemorySize64,
            VirtualMemorySize = process.VirtualMemorySize64,
            PagedMemorySize = process.PagedMemorySize64,
            NonpagedSystemMemorySize = process.NonpagedSystemMemorySize64,
            PagedSystemMemorySize = process.PagedSystemMemorySize64
        };
    }

    public async Task ForceGarbageCollectionAsync()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    public async Task ClearImageCacheAsync()
    {
        await _imageCacheService.ClearCacheAsync();
    }

    public async Task ClearUnusedDataAsync()
    {
        // Limpiar referencias débiles que ya no están vivas
        var deadKeys = new List<string>();
        
        foreach (var kvp in _weakReferences)
        {
            if (!kvp.Value.IsAlive)
            {
                deadKeys.Add(kvp.Key);
            }
        }
        
        foreach (var key in deadKeys)
        {
            _weakReferences.Remove(key);
        }
        
        // Forzar garbage collection
        await ForceGarbageCollectionAsync();
    }

    public void RegisterWeakReference(object target, string key)
    {
        _weakReferences[key] = new WeakReference(target);
    }

    public void UnregisterWeakReference(string key)
    {
        _weakReferences.Remove(key);
    }
}

// Models/MemoryInfo.cs
public class MemoryInfo
{
    public long WorkingSet { get; set; }
    public long PrivateMemorySize { get; set; }
    public long VirtualMemorySize { get; set; }
    public long PagedMemorySize { get; set; }
    public long NonpagedSystemMemorySize { get; set; }
    public long PagedSystemMemorySize { get; set; }
    
    public string GetFormattedWorkingSet()
    {
        return FormatBytes(WorkingSet);
    }
    
    public string GetFormattedPrivateMemory()
    {
        return FormatBytes(PrivateMemorySize);
    }
    
    private string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        
        return $"{len:0.##} {sizes[order]}";
    }
}
```

### **4. Performance Monitoring Service**

```csharp
// Services/IPerformanceMonitoringService.cs
public interface IPerformanceMonitoringService
{
    void StartTimer(string operationName);
    void StopTimer(string operationName);
    Task<PerformanceMetrics> GetMetricsAsync();
    void LogMemoryUsage(string context);
    void LogPerformanceIssue(string issue, string details);
}

// Services/PerformanceMonitoringService.cs
public class PerformanceMonitoringService : IPerformanceMonitoringService
{
    private readonly Dictionary<string, Stopwatch> _timers;
    private readonly List<PerformanceMetric> _metrics;
    private readonly IMemoryManagementService _memoryManagementService;

    public PerformanceMonitoringService(IMemoryManagementService memoryManagementService)
    {
        _timers = new Dictionary<string, Stopwatch>();
        _metrics = new List<PerformanceMetric>();
        _memoryManagementService = memoryManagementService;
    }

    public void StartTimer(string operationName)
    {
        if (_timers.ContainsKey(operationName))
        {
            _timers[operationName].Restart();
        }
        else
        {
            _timers[operationName] = Stopwatch.StartNew();
        }
    }

    public void StopTimer(string operationName)
    {
        if (_timers.TryGetValue(operationName, out var stopwatch))
        {
            stopwatch.Stop();
            
            var metric = new PerformanceMetric
            {
                OperationName = operationName,
                Duration = stopwatch.ElapsedMilliseconds,
                Timestamp = DateTime.UtcNow
            };
            
            _metrics.Add(metric);
            
            // Mantener solo los últimos 1000 métricas
            if (_metrics.Count > 1000)
            {
                _metrics.RemoveAt(0);
            }
        }
    }

    public async Task<PerformanceMetrics> GetMetricsAsync()
    {
        var memoryInfo = await _memoryManagementService.GetMemoryInfoAsync();
        
        return new PerformanceMetrics
        {
            MemoryInfo = memoryInfo,
            RecentMetrics = _metrics.TakeLast(100).ToList(),
            AverageResponseTime = _metrics.Any() ? _metrics.Average(m => m.Duration) : 0,
            TotalOperations = _metrics.Count
        };
    }

    public void LogMemoryUsage(string context)
    {
        var memoryInfo = _memoryManagementService.GetMemoryInfoAsync().Result;
        System.Diagnostics.Debug.WriteLine($"[{context}] Memory: {memoryInfo.GetFormattedWorkingSet()}");
    }

    public void LogPerformanceIssue(string issue, string details)
    {
        var logEntry = $"[PERFORMANCE ISSUE] {issue}: {details}";
        System.Diagnostics.Debug.WriteLine(logEntry);
        
        // En una implementación real, enviarías esto a un servicio de logging
    }
}

// Models/PerformanceMetric.cs
public class PerformanceMetric
{
    public string OperationName { get; set; }
    public long Duration { get; set; }
    public DateTime Timestamp { get; set; }
}

// Models/PerformanceMetrics.cs
public class PerformanceMetrics
{
    public MemoryInfo MemoryInfo { get; set; }
    public List<PerformanceMetric> RecentMetrics { get; set; }
    public double AverageResponseTime { get; set; }
    public int TotalOperations { get; set; }
}
```

### **5. Optimized CollectionView**

```xml
<!-- Views/OptimizedMusicianListView.xaml -->
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage x:Class="MussikOn.Mobile.Views.OptimizedMusicianListView"
             xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:MussikOn.Mobile.ViewModels"
             x:DataType="vm:LazyLoadingViewModel"
             Title="Músicos">

    <Grid RowDefinitions="*,Auto">
        
        <!-- Optimized CollectionView -->
        <CollectionView Grid.Row="0" 
                        ItemsSource="{Binding Musicians}"
                        RemainingItemsThreshold="5"
                        RemainingItemsThresholdReachedCommand="{Binding LoadMoreCommand}">
            
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <Grid Padding="16,8" ColumnDefinitions="Auto,*,Auto">
                        
                        <!-- Optimized Image with Caching -->
                        <Image Grid.Column="0" 
                               Source="{Binding ProfileImageUrl}" 
                               WidthRequest="60" 
                               HeightRequest="60" 
                               Aspect="AspectFill">
                            <Image.Triggers>
                                <DataTrigger TargetType="Image" Binding="{Binding ProfileImageUrl}" Value="{x:Null}">
                                    <Setter Property="Source" Value="default_avatar.png" />
                                </DataTrigger>
                            </Image.Triggers>
                        </Image>
                        
                        <!-- Musician Info -->
                        <StackLayout Grid.Column="1" Margin="12,0,0,0">
                            <Label Text="{Binding Name}" 
                                   Style="{StaticResource BodyLabel}" 
                                   FontAttributes="Bold" />
                            <Label Text="{Binding Genres, StringFormat='Géneros: {0}'}" 
                                   Style="{StaticResource CaptionLabel}" />
                            <Label Text="{Binding Location}" 
                                   Style="{StaticResource CaptionLabel}" />
                            <Label Text="{Binding Rating, StringFormat='⭐ {0:F1}'}" 
                                   Style="{StaticResource CaptionLabel}" />
                        </StackLayout>
                        
                        <!-- Action Button -->
                        <Button Grid.Column="2" 
                                Text="Ver" 
                                Style="{StaticResource LinkButton}" />
                        
                    </Grid>
                </DataTemplate>
            </CollectionView.ItemTemplate>
            
            <!-- Footer with Loading Indicator -->
            <CollectionView.Footer>
                <StackLayout>
                    <ActivityIndicator IsVisible="{Binding IsLoadingMore}" 
                                       IsRunning="{Binding IsLoadingMore}" 
                                       HorizontalOptions="Center" />
                    <Label Text="{Binding HasMoreItems, Converter={StaticResource BoolToTextConverter}}" 
                           HorizontalOptions="Center" 
                           Style="{StaticResource CaptionLabel}" />
                </StackLayout>
            </CollectionView.Footer>
            
        </CollectionView>

        <!-- Performance Info (Debug Only) -->
        <Frame Grid.Row="1" 
               IsVisible="{Binding Source={x:Static Application.Current}, Path=Properties[IsDebugMode]}"
               BackgroundColor="LightGray" 
               Padding="8">
            <StackLayout Orientation="Horizontal" HorizontalOptions="Center">
                <Label Text="{Binding Musicians.Count, StringFormat='Items: {0}'}" 
                       Style="{StaticResource CaptionLabel}" />
                <Label Text=" | " Style="{StaticResource CaptionLabel}" />
                <Label Text="{Binding IsLoadingMore, StringFormat='Loading: {0}'}" 
                       Style="{StaticResource CaptionLabel}" />
            </StackLayout>
        </Frame>

    </Grid>
</ContentPage>
```

### **6. Dispose Pattern Implementation**

```csharp
// Services/DisposableService.cs
public class DisposableService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly Timer _timer;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private bool _disposed = false;

    public DisposableService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _timer = new Timer(TimerCallback, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        _cancellationTokenSource = new CancellationTokenSource();
    }

    private void TimerCallback(object state)
    {
        // Lógica del timer
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Liberar recursos administrados
                _timer?.Dispose();
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
            }
            
            _disposed = true;
        }
    }

    ~DisposableService()
    {
        Dispose(false);
    }
}
```

### **7. Performance-Optimized ViewModel**

```csharp
// ViewModels/PerformanceOptimizedViewModel.cs
public class PerformanceOptimizedViewModel : BaseViewModel, IDisposable
{
    private readonly IPerformanceMonitoringService _performanceMonitoring;
    private readonly IMemoryManagementService _memoryManagement;
    private readonly Timer _memoryCheckTimer;
    private bool _disposed = false;

    public PerformanceOptimizedViewModel(
        IPerformanceMonitoringService performanceMonitoring,
        IMemoryManagementService memoryManagement)
    {
        _performanceMonitoring = performanceMonitoring;
        _memoryManagement = memoryManagement;
        
        // Verificar memoria cada 30 segundos
        _memoryCheckTimer = new Timer(CheckMemoryUsage, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        
        InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        _performanceMonitoring.StartTimer("Initialize");
        
        try
        {
            // Inicialización optimizada
            await Task.Run(() =>
            {
                // Operaciones que no requieren UI thread
                ProcessInitialData();
            });
        }
        finally
        {
            _performanceMonitoring.StopTimer("Initialize");
        }
    }

    private void ProcessInitialData()
    {
        // Procesar datos iniciales de forma eficiente
    }

    private void CheckMemoryUsage(object state)
    {
        _performanceMonitoring.LogMemoryUsage("Periodic Check");
        
        // Limpiar datos no utilizados si la memoria es alta
        var memoryInfo = _memoryManagement.GetMemoryInfoAsync().Result;
        if (memoryInfo.WorkingSet > 100 * 1024 * 1024) // 100MB
        {
            _ = Task.Run(async () =>
            {
                await _memoryManagement.ClearUnusedDataAsync();
                _performanceMonitoring.LogPerformanceIssue("High Memory Usage", "Cleared unused data");
            });
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _memoryCheckTimer?.Dispose();
            }
            
            _disposed = true;
        }
    }
}
```

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Lazy Loading**
1. Implementar carga paginada de datos
2. Configurar indicadores de carga
3. Manejar estados de carga

### **Ejercicio 2: Image Caching**
1. Crear servicio de caché de imágenes
2. Implementar descarga asíncrona
3. Optimizar uso de memoria

### **Ejercicio 3: Performance Monitoring**
1. Implementar monitoreo de rendimiento
2. Crear métricas de memoria
3. Configurar alertas de performance

## 📝 **Resumen de la Clase**

En esta clase hemos aprendido:

✅ **Optimización de rendimiento** y lazy loading
✅ **Manejo eficiente de memoria** y garbage collection
✅ **Image caching** y optimización de recursos
✅ **Performance monitoring** y métricas
✅ **Dispose pattern** y gestión de recursos
✅ **Mejores prácticas** de performance

## 🚀 **Próxima Clase**

En la siguiente clase implementaremos:
- **Testing** en aplicaciones móviles
- **Unit testing** y integration testing
- **UI testing** automatizado
- **Performance testing**

---

**💡 Tip del Día**: El rendimiento en aplicaciones móviles es crítico. Siempre monitorea el uso de memoria y optimiza las operaciones costosas para mantener una experiencia fluida.
