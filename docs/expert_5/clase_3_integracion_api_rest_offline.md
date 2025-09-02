# 🚀 **Clase 3: Integración con APIs REST y Capacidades Offline**

## 🎯 **Objetivos de la Clase**
- Integrar con APIs REST de MussikOn
- Implementar capacidades offline
- Configurar caché y sincronización
- Manejar estados de conectividad
- Implementar retry policies y circuit breakers

## 📚 **Contenido Teórico**

### **1. Integración con APIs REST**

La integración con APIs REST en .NET MAUI incluye:
- **HttpClient** para comunicación HTTP
- **Serialización JSON** con System.Text.Json
- **Autenticación** con JWT tokens
- **Manejo de errores** y timeouts

### **2. Capacidades Offline**

Las aplicaciones móviles deben funcionar sin conexión:
- **Almacenamiento local** con SQLite
- **Sincronización** cuando se restaura la conexión
- **Conflict resolution** para datos modificados
- **Caché inteligente** de datos frecuentes

### **3. Patrón Repository con Caché**

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   ViewModel     │───▶│   Repository    │───▶│   API Service   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                              │
                              ▼
                       ┌─────────────────┐
                       │   Local Cache   │
                       │    (SQLite)     │
                       └─────────────────┘
```

## 💻 **Implementación Práctica**

### **1. Configuración de HttpClient**

```csharp
// Services/HttpClientService.cs
public class HttpClientService
{
    private readonly HttpClient _httpClient;
    private readonly ISecureStorage _secureStorage;
    private readonly IConnectivity _connectivity;

    public HttpClientService(HttpClient httpClient, ISecureStorage secureStorage, IConnectivity connectivity)
    {
        _httpClient = httpClient;
        _secureStorage = secureStorage;
        _connectivity = connectivity;
        
        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri("https://api.mussikon.com");
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "MussikOn-Mobile/1.0");
    }

    public async Task<T> GetAsync<T>(string endpoint, bool useCache = true)
    {
        try
        {
            // Verificar conectividad
            if (!_connectivity.NetworkAccess.HasFlag(NetworkAccess.Internet))
            {
                if (useCache)
                {
                    return await GetFromCacheAsync<T>(endpoint);
                }
                throw new NoInternetException("Sin conexión a internet");
            }

            // Agregar token de autenticación
            await AddAuthenticationHeaderAsync();

            var response = await _httpClient.GetAsync(endpoint);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<T>(content, GetJsonOptions());
                
                // Guardar en caché si es exitoso
                if (useCache)
                {
                    await SaveToCacheAsync(endpoint, result);
                }
                
                return result;
            }
            else
            {
                throw new ApiException($"Error API: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            // Intentar obtener de caché en caso de error de red
            if (useCache)
            {
                return await GetFromCacheAsync<T>(endpoint);
            }
            throw;
        }
    }

    public async Task<T> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            if (!_connectivity.NetworkAccess.HasFlag(NetworkAccess.Internet))
            {
                throw new NoInternetException("Sin conexión a internet");
            }

            await AddAuthenticationHeaderAsync();

            var json = JsonSerializer.Serialize(data, GetJsonOptions());
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, GetJsonOptions());
            }
            else
            {
                throw new ApiException($"Error API: {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException($"Error de conexión: {ex.Message}");
        }
    }

    private async Task AddAuthenticationHeaderAsync()
    {
        var token = await _secureStorage.GetAsync("auth_token");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    private async Task SaveToCacheAsync<T>(string key, T data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, GetJsonOptions());
            await _secureStorage.SetAsync($"cache_{key}", json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving to cache: {ex.Message}");
        }
    }

    private async Task<T> GetFromCacheAsync<T>(string key)
    {
        try
        {
            var cachedJson = await _secureStorage.GetAsync($"cache_{key}");
            if (!string.IsNullOrEmpty(cachedJson))
            {
                return JsonSerializer.Deserialize<T>(cachedJson, GetJsonOptions());
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error reading from cache: {ex.Message}");
        }
        
        throw new NoDataException("No hay datos disponibles offline");
    }
}
```

### **2. Repository Pattern con Caché**

```csharp
// Repositories/IMusicianRepository.cs
public interface IMusicianRepository
{
    Task<IEnumerable<Musician>> GetMusiciansAsync(MusicianSearchCriteria criteria);
    Task<Musician> GetMusicianByIdAsync(int id);
    Task<IEnumerable<Musician>> GetFeaturedMusiciansAsync();
    Task<bool> SaveMusicianAsync(Musician musician);
    Task<bool> DeleteMusicianAsync(int id);
}

// Repositories/MusicianRepository.cs
public class MusicianRepository : IMusicianRepository
{
    private readonly HttpClientService _httpClient;
    private readonly ILocalDatabase _localDatabase;
    private readonly IConnectivity _connectivity;

    public MusicianRepository(HttpClientService httpClient, ILocalDatabase localDatabase, IConnectivity connectivity)
    {
        _httpClient = httpClient;
        _localDatabase = localDatabase;
        _connectivity = connectivity;
    }

    public async Task<IEnumerable<Musician>> GetMusiciansAsync(MusicianSearchCriteria criteria)
    {
        try
        {
            // Intentar obtener de la API primero
            if (_connectivity.NetworkAccess.HasFlag(NetworkAccess.Internet))
            {
                var queryParams = BuildQueryParams(criteria);
                var musicians = await _httpClient.GetAsync<IEnumerable<Musician>>($"/api/musicians{queryParams}");
                
                // Guardar en base de datos local
                await _localDatabase.SaveMusiciansAsync(musicians);
                
                return musicians;
            }
            else
            {
                // Obtener de base de datos local
                return await _localDatabase.GetMusiciansAsync(criteria);
            }
        }
        catch (Exception ex)
        {
            // Fallback a base de datos local
            return await _localDatabase.GetMusiciansAsync(criteria);
        }
    }

    public async Task<Musician> GetMusicianByIdAsync(int id)
    {
        try
        {
            if (_connectivity.NetworkAccess.HasFlag(NetworkAccess.Internet))
            {
                var musician = await _httpClient.GetAsync<Musician>($"/api/musicians/{id}");
                await _localDatabase.SaveMusicianAsync(musician);
                return musician;
            }
            else
            {
                return await _localDatabase.GetMusicianByIdAsync(id);
            }
        }
        catch (Exception ex)
        {
            return await _localDatabase.GetMusicianByIdAsync(id);
        }
    }

    public async Task<IEnumerable<Musician>> GetFeaturedMusiciansAsync()
    {
        try
        {
            if (_connectivity.NetworkAccess.HasFlag(NetworkAccess.Internet))
            {
                var musicians = await _httpClient.GetAsync<IEnumerable<Musician>>("/api/musicians/featured");
                await _localDatabase.SaveFeaturedMusiciansAsync(musicians);
                return musicians;
            }
            else
            {
                return await _localDatabase.GetFeaturedMusiciansAsync();
            }
        }
        catch (Exception ex)
        {
            return await _localDatabase.GetFeaturedMusiciansAsync();
        }
    }

    public async Task<bool> SaveMusicianAsync(Musician musician)
    {
        try
        {
            if (_connectivity.NetworkAccess.HasFlag(NetworkAccess.Internet))
            {
                var result = await _httpClient.PostAsync<Musician>("/api/musicians", musician);
                await _localDatabase.SaveMusicianAsync(result);
                return true;
            }
            else
            {
                // Guardar localmente para sincronizar después
                await _localDatabase.SavePendingMusicianAsync(musician);
                return true;
            }
        }
        catch (Exception ex)
        {
            // Guardar localmente para sincronizar después
            await _localDatabase.SavePendingMusicianAsync(musician);
            return false;
        }
    }

    private string BuildQueryParams(MusicianSearchCriteria criteria)
    {
        var queryParams = new List<string>();
        
        if (!string.IsNullOrEmpty(criteria.Genre))
            queryParams.Add($"genre={Uri.EscapeDataString(criteria.Genre)}");
        
        if (!string.IsNullOrEmpty(criteria.Location))
            queryParams.Add($"location={Uri.EscapeDataString(criteria.Location)}");
        
        if (criteria.MinPrice.HasValue)
            queryParams.Add($"minPrice={criteria.MinPrice.Value}");
        
        if (criteria.MaxPrice.HasValue)
            queryParams.Add($"maxPrice={criteria.MaxPrice.Value}");
        
        if (criteria.AvailabilityDate.HasValue)
            queryParams.Add($"availabilityDate={criteria.AvailabilityDate.Value:yyyy-MM-dd}");
        
        return queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
    }
}
```

### **3. Base de Datos Local con SQLite**

```csharp
// Database/ILocalDatabase.cs
public interface ILocalDatabase
{
    Task<IEnumerable<Musician>> GetMusiciansAsync(MusicianSearchCriteria criteria);
    Task<Musician> GetMusicianByIdAsync(int id);
    Task<IEnumerable<Musician>> GetFeaturedMusiciansAsync();
    Task SaveMusicianAsync(Musician musician);
    Task SaveMusiciansAsync(IEnumerable<Musician> musicians);
    Task SaveFeaturedMusiciansAsync(IEnumerable<Musician> musicians);
    Task SavePendingMusicianAsync(Musician musician);
    Task<IEnumerable<Musician>> GetPendingMusiciansAsync();
    Task ClearPendingMusiciansAsync();
}

// Database/LocalDatabase.cs
public class LocalDatabase : ILocalDatabase
{
    private readonly SQLiteAsyncConnection _database;

    public LocalDatabase()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "mussikon.db");
        _database = new SQLiteAsyncConnection(dbPath);
        
        InitializeDatabaseAsync();
    }

    private async Task InitializeDatabaseAsync()
    {
        await _database.CreateTableAsync<Musician>();
        await _database.CreateTableAsync<PendingMusician>();
        await _database.CreateTableAsync<CacheEntry>();
    }

    public async Task<IEnumerable<Musician>> GetMusiciansAsync(MusicianSearchCriteria criteria)
    {
        var query = _database.Table<Musician>();

        if (!string.IsNullOrEmpty(criteria.Genre))
            query = query.Where(m => m.Genres.Contains(criteria.Genre));

        if (!string.IsNullOrEmpty(criteria.Location))
            query = query.Where(m => m.Location.Contains(criteria.Location));

        if (criteria.MinPrice.HasValue)
            query = query.Where(m => m.HourlyRate >= criteria.MinPrice.Value);

        if (criteria.MaxPrice.HasValue)
            query = query.Where(m => m.HourlyRate <= criteria.MaxPrice.Value);

        return await query.ToListAsync();
    }

    public async Task<Musician> GetMusicianByIdAsync(int id)
    {
        return await _database.Table<Musician>()
            .Where(m => m.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Musician>> GetFeaturedMusiciansAsync()
    {
        return await _database.Table<Musician>()
            .Where(m => m.IsFeatured)
            .OrderByDescending(m => m.Rating)
            .Take(10)
            .ToListAsync();
    }

    public async Task SaveMusicianAsync(Musician musician)
    {
        await _database.InsertOrReplaceAsync(musician);
    }

    public async Task SaveMusiciansAsync(IEnumerable<Musician> musicians)
    {
        await _database.InsertOrReplaceAllAsync(musicians);
    }

    public async Task SaveFeaturedMusiciansAsync(IEnumerable<Musician> musicians)
    {
        // Marcar como featured y guardar
        foreach (var musician in musicians)
        {
            musician.IsFeatured = true;
        }
        await _database.InsertOrReplaceAllAsync(musicians);
    }

    public async Task SavePendingMusicianAsync(Musician musician)
    {
        var pendingMusician = new PendingMusician
        {
            MusicianData = JsonSerializer.Serialize(musician),
            CreatedAt = DateTime.UtcNow,
            Action = "Create"
        };
        
        await _database.InsertAsync(pendingMusician);
    }

    public async Task<IEnumerable<Musician>> GetPendingMusiciansAsync()
    {
        var pendingMusicians = await _database.Table<PendingMusician>().ToListAsync();
        return pendingMusicians.Select(p => JsonSerializer.Deserialize<Musician>(p.MusicianData));
    }

    public async Task ClearPendingMusiciansAsync()
    {
        await _database.DeleteAllAsync<PendingMusician>();
    }
}
```

### **4. Servicio de Sincronización**

```csharp
// Services/ISyncService.cs
public interface ISyncService
{
    Task SyncPendingDataAsync();
    Task SyncMusiciansAsync();
    Task SyncEventsAsync();
    Task SyncChatsAsync();
    Task<bool> IsSyncInProgressAsync();
}

// Services/SyncService.cs
public class SyncService : ISyncService
{
    private readonly IMusicianRepository _musicianRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IChatRepository _chatRepository;
    private readonly ILocalDatabase _localDatabase;
    private readonly IConnectivity _connectivity;
    private bool _isSyncInProgress;

    public SyncService(
        IMusicianRepository musicianRepository,
        IEventRepository eventRepository,
        IChatRepository chatRepository,
        ILocalDatabase localDatabase,
        IConnectivity connectivity)
    {
        _musicianRepository = musicianRepository;
        _eventRepository = eventRepository;
        _chatRepository = chatRepository;
        _localDatabase = localDatabase;
        _connectivity = connectivity;
    }

    public async Task SyncPendingDataAsync()
    {
        if (_isSyncInProgress || !_connectivity.NetworkAccess.HasFlag(NetworkAccess.Internet))
            return;

        try
        {
            _isSyncInProgress = true;

            await Task.WhenAll(
                SyncPendingMusiciansAsync(),
                SyncPendingEventsAsync(),
                SyncPendingChatsAsync()
            );
        }
        finally
        {
            _isSyncInProgress = false;
        }
    }

    private async Task SyncPendingMusiciansAsync()
    {
        try
        {
            var pendingMusicians = await _localDatabase.GetPendingMusiciansAsync();
            
            foreach (var musician in pendingMusicians)
            {
                await _musicianRepository.SaveMusicianAsync(musician);
            }
            
            await _localDatabase.ClearPendingMusiciansAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error syncing pending musicians: {ex.Message}");
        }
    }

    private async Task SyncPendingEventsAsync()
    {
        // Implementar sincronización de eventos pendientes
    }

    private async Task SyncPendingChatsAsync()
    {
        // Implementar sincronización de chats pendientes
    }

    public async Task SyncMusiciansAsync()
    {
        if (!_connectivity.NetworkAccess.HasFlag(NetworkAccess.Internet))
            return;

        try
        {
            // Sincronizar músicos destacados
            await _musicianRepository.GetFeaturedMusiciansAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error syncing musicians: {ex.Message}");
        }
    }

    public async Task SyncEventsAsync()
    {
        if (!_connectivity.NetworkAccess.HasFlag(NetworkAccess.Internet))
            return;

        try
        {
            // Implementar sincronización de eventos
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error syncing events: {ex.Message}");
        }
    }

    public async Task SyncChatsAsync()
    {
        if (!_connectivity.NetworkAccess.HasFlag(NetworkAccess.Internet))
            return;

        try
        {
            // Implementar sincronización de chats
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error syncing chats: {ex.Message}");
        }
    }

    public async Task<bool> IsSyncInProgressAsync()
    {
        return _isSyncInProgress;
    }
}
```

### **5. Connectivity Service**

```csharp
// Services/IConnectivityService.cs
public interface IConnectivityService
{
    bool IsConnected { get; }
    NetworkAccess NetworkAccess { get; }
    event EventHandler<ConnectivityChangedEventArgs> ConnectivityChanged;
    Task<bool> WaitForConnectionAsync(TimeSpan timeout);
}

// Services/ConnectivityService.cs
public class ConnectivityService : IConnectivityService
{
    private readonly IConnectivity _connectivity;

    public ConnectivityService(IConnectivity connectivity)
    {
        _connectivity = connectivity;
        _connectivity.ConnectivityChanged += OnConnectivityChanged;
    }

    public bool IsConnected => _connectivity.NetworkAccess == NetworkAccess.Internet;
    public NetworkAccess NetworkAccess => _connectivity.NetworkAccess;

    public event EventHandler<ConnectivityChangedEventArgs> ConnectivityChanged;

    public async Task<bool> WaitForConnectionAsync(TimeSpan timeout)
    {
        if (IsConnected)
            return true;

        var tcs = new TaskCompletionSource<bool>();
        var timeoutTask = Task.Delay(timeout);
        
        EventHandler<ConnectivityChangedEventArgs> handler = null;
        handler = (sender, args) =>
        {
            if (IsConnected)
            {
                _connectivity.ConnectivityChanged -= handler;
                tcs.SetResult(true);
            }
        };

        _connectivity.ConnectivityChanged += handler;

        var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);
        
        _connectivity.ConnectivityChanged -= handler;
        
        return completedTask == tcs.Task;
    }

    private void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        ConnectivityChanged?.Invoke(sender, e);
    }
}
```

### **6. Retry Policy con Polly**

```csharp
// Services/RetryPolicyService.cs
public class RetryPolicyService
{
    private readonly IAsyncPolicy _retryPolicy;

    public RetryPolicyService()
    {
        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    System.Diagnostics.Debug.WriteLine($"Retry {retryCount} after {timespan} seconds");
                });
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        return await _retryPolicy.ExecuteAsync(operation);
    }

    public async Task ExecuteAsync(Func<Task> operation)
    {
        await _retryPolicy.ExecuteAsync(operation);
    }
}
```

### **7. ViewModel con Capacidades Offline**

```csharp
// ViewModels/SearchViewModel.cs
public class SearchViewModel : BaseViewModel
{
    private readonly IMusicianRepository _musicianRepository;
    private readonly IConnectivityService _connectivityService;
    private readonly ISyncService _syncService;
    
    private ObservableCollection<Musician> _musicians;
    private MusicianSearchCriteria _searchCriteria;
    private bool _isOfflineMode;

    public SearchViewModel(
        IMusicianRepository musicianRepository,
        IConnectivityService connectivityService,
        ISyncService syncService)
    {
        _musicianRepository = musicianRepository;
        _connectivityService = connectivityService;
        _syncService = syncService;
        
        _musicians = new ObservableCollection<Musician>();
        _searchCriteria = new MusicianSearchCriteria();
        
        SearchCommand = new Command(async () => await SearchAsync());
        LoadMoreCommand = new Command(async () => await LoadMoreAsync());
        RefreshCommand = new Command(async () => await RefreshAsync());
        
        // Suscribirse a cambios de conectividad
        _connectivityService.ConnectivityChanged += OnConnectivityChanged;
        
        LoadInitialDataAsync();
    }

    public ObservableCollection<Musician> Musicians
    {
        get => _musicians;
        set => SetProperty(ref _musicians, value);
    }

    public MusicianSearchCriteria SearchCriteria
    {
        get => _searchCriteria;
        set => SetProperty(ref _searchCriteria, value);
    }

    public bool IsOfflineMode
    {
        get => _isOfflineMode;
        set => SetProperty(ref _isOfflineMode, value);
    }

    public Command SearchCommand { get; }
    public Command LoadMoreCommand { get; }
    public Command RefreshCommand { get; }

    private async Task LoadInitialDataAsync()
    {
        try
        {
            IsBusy = true;
            IsOfflineMode = !_connectivityService.IsConnected;
            
            var musicians = await _musicianRepository.GetFeaturedMusiciansAsync();
            Musicians.Clear();
            
            foreach (var musician in musicians)
            {
                Musicians.Add(musician);
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SearchAsync()
    {
        try
        {
            IsBusy = true;
            IsOfflineMode = !_connectivityService.IsConnected;
            
            var musicians = await _musicianRepository.GetMusiciansAsync(SearchCriteria);
            Musicians.Clear();
            
            foreach (var musician in musicians)
            {
                Musicians.Add(musician);
            }
        }
        catch (NoInternetException)
        {
            IsOfflineMode = true;
            await Application.Current.MainPage.DisplayAlert(
                "Sin Conexión", 
                "Mostrando resultados guardados localmente", 
                "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshAsync()
    {
        if (_connectivityService.IsConnected)
        {
            await _syncService.SyncMusiciansAsync();
        }
        
        await LoadInitialDataAsync();
    }

    private void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        IsOfflineMode = !_connectivityService.IsConnected;
        
        if (_connectivityService.IsConnected)
        {
            // Sincronizar datos cuando se restaura la conexión
            _ = Task.Run(async () => await _syncService.SyncPendingDataAsync());
        }
    }
}
```

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Configurar HttpClient**
1. Implementar HttpClientService con autenticación
2. Configurar manejo de errores y timeouts
3. Implementar retry policies

### **Ejercicio 2: Base de Datos Local**
1. Configurar SQLite con Entity Framework
2. Implementar operaciones CRUD
3. Crear sistema de caché

### **Ejercicio 3: Sincronización Offline**
1. Implementar sincronización de datos
2. Manejar conflictos de datos
3. Crear indicadores de estado offline

## 📝 **Resumen de la Clase**

En esta clase hemos aprendido:

✅ **Integración con APIs REST** usando HttpClient
✅ **Capacidades offline** con SQLite
✅ **Patrón Repository** con caché
✅ **Sincronización de datos** automática
✅ **Manejo de conectividad** y estados
✅ **Retry policies** y circuit breakers

## 🚀 **Próxima Clase**

En la siguiente clase implementaremos:
- **Push notifications** y notificaciones locales
- **Geolocalización** y mapas
- **Cámara y multimedia**
- **Integración con servicios nativos**

---

**💡 Tip del Día**: Las aplicaciones móviles deben funcionar sin conexión. Implementa siempre un sistema de caché local y sincronización para mejorar la experiencia del usuario.
