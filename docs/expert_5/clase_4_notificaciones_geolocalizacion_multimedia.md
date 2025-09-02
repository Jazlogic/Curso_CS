# 🚀 **Clase 4: Notificaciones, Geolocalización y Multimedia**

## 🎯 **Objetivos de la Clase**
- Implementar push notifications
- Configurar notificaciones locales
- Integrar geolocalización y mapas
- Manejar cámara y multimedia
- Implementar servicios nativos

## 📚 **Contenido Teórico**

### **1. Sistema de Notificaciones**

Las notificaciones móviles incluyen:
- **Push Notifications**: Enviadas desde el servidor
- **Local Notifications**: Programadas en el dispositivo
- **In-App Notifications**: Mostradas dentro de la app
- **Badge Notifications**: Contadores en el icono

### **2. Geolocalización y Mapas**

Funcionalidades de ubicación:
- **GPS y Network Location**: Precisión de ubicación
- **Mapas interactivos**: Visualización de ubicaciones
- **Geofencing**: Detección de entrada/salida de áreas
- **Rutas y direcciones**: Navegación

### **3. Multimedia y Cámara**

Capacidades multimedia:
- **Cámara**: Captura de fotos y videos
- **Galería**: Selección de medios existentes
- **Audio**: Grabación y reproducción
- **Procesamiento**: Edición y filtros

## 💻 **Implementación Práctica**

### **1. Servicio de Notificaciones**

```csharp
// Services/INotificationService.cs
public interface INotificationService
{
    Task<bool> RequestPermissionAsync();
    Task<bool> IsPermissionGrantedAsync();
    Task SendLocalNotificationAsync(string title, string message, int delaySeconds = 0);
    Task ScheduleNotificationAsync(string title, string message, DateTime scheduledTime);
    Task CancelNotificationAsync(int notificationId);
    Task CancelAllNotificationsAsync();
    Task<string> GetDeviceTokenAsync();
    Task RegisterForPushNotificationsAsync();
}

// Services/NotificationService.cs
public class NotificationService : INotificationService
{
    private readonly IPlatformNotificationService _platformNotificationService;

    public NotificationService(IPlatformNotificationService platformNotificationService)
    {
        _platformNotificationService = platformNotificationService;
    }

    public async Task<bool> RequestPermissionAsync()
    {
        return await _platformNotificationService.RequestPermissionAsync();
    }

    public async Task<bool> IsPermissionGrantedAsync()
    {
        return await _platformNotificationService.IsPermissionGrantedAsync();
    }

    public async Task SendLocalNotificationAsync(string title, string message, int delaySeconds = 0)
    {
        var notification = new LocalNotification
        {
            Title = title,
            Message = message,
            DelaySeconds = delaySeconds
        };

        await _platformNotificationService.SendLocalNotificationAsync(notification);
    }

    public async Task ScheduleNotificationAsync(string title, string message, DateTime scheduledTime)
    {
        var notification = new LocalNotification
        {
            Title = title,
            Message = message,
            ScheduledTime = scheduledTime
        };

        await _platformNotificationService.ScheduleNotificationAsync(notification);
    }

    public async Task CancelNotificationAsync(int notificationId)
    {
        await _platformNotificationService.CancelNotificationAsync(notificationId);
    }

    public async Task CancelAllNotificationsAsync()
    {
        await _platformNotificationService.CancelAllNotificationsAsync();
    }

    public async Task<string> GetDeviceTokenAsync()
    {
        return await _platformNotificationService.GetDeviceTokenAsync();
    }

    public async Task RegisterForPushNotificationsAsync()
    {
        var hasPermission = await RequestPermissionAsync();
        if (hasPermission)
        {
            var deviceToken = await GetDeviceTokenAsync();
            // Enviar token al servidor para registro
            await RegisterTokenWithServerAsync(deviceToken);
        }
    }

    private async Task RegisterTokenWithServerAsync(string deviceToken)
    {
        try
        {
            // Implementar registro del token en el servidor
            var registrationRequest = new
            {
                DeviceToken = deviceToken,
                Platform = DeviceInfo.Platform.ToString(),
                UserId = await GetCurrentUserIdAsync()
            };

            // Enviar al servidor MussikOn
            // await _httpClient.PostAsync("/api/notifications/register", registrationRequest);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error registering device token: {ex.Message}");
        }
    }

    private async Task<string> GetCurrentUserIdAsync()
    {
        // Obtener ID del usuario actual
        return "current_user_id";
    }
}
```

### **2. Servicio de Geolocalización**

```csharp
// Services/ILocationService.cs
public interface ILocationService
{
    Task<bool> RequestLocationPermissionAsync();
    Task<bool> IsLocationPermissionGrantedAsync();
    Task<Location> GetCurrentLocationAsync();
    Task<Location> GetLastKnownLocationAsync();
    Task StartLocationUpdatesAsync();
    Task StopLocationUpdatesAsync();
    event EventHandler<LocationChangedEventArgs> LocationChanged;
    Task<double> CalculateDistanceAsync(Location location1, Location location2);
    Task<string> GetAddressFromLocationAsync(Location location);
    Task<Location> GetLocationFromAddressAsync(string address);
}

// Services/LocationService.cs
public class LocationService : ILocationService
{
    private readonly IGeolocation _geolocation;
    private readonly IGeocoding _geocoding;
    private readonly IConnectivity _connectivity;
    private CancellationTokenSource _cancellationTokenSource;

    public event EventHandler<LocationChangedEventArgs> LocationChanged;

    public LocationService(IGeolocation geolocation, IGeocoding geocoding, IConnectivity connectivity)
    {
        _geolocation = geolocation;
        _geocoding = geocoding;
        _connectivity = connectivity;
    }

    public async Task<bool> RequestLocationPermissionAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            return status == PermissionStatus.Granted;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error requesting location permission: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> IsLocationPermissionGrantedAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        return status == PermissionStatus.Granted;
    }

    public async Task<Location> GetCurrentLocationAsync()
    {
        try
        {
            var hasPermission = await IsLocationPermissionGrantedAsync();
            if (!hasPermission)
            {
                throw new PermissionException("Location permission not granted");
            }

            var request = new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Medium,
                Timeout = TimeSpan.FromSeconds(10)
            };

            var location = await _geolocation.GetLocationAsync(request);
            return location;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting current location: {ex.Message}");
            throw;
        }
    }

    public async Task<Location> GetLastKnownLocationAsync()
    {
        try
        {
            var location = await _geolocation.GetLastKnownLocationAsync();
            return location;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting last known location: {ex.Message}");
            return null;
        }
    }

    public async Task StartLocationUpdatesAsync()
    {
        try
        {
            var hasPermission = await IsLocationPermissionGrantedAsync();
            if (!hasPermission)
            {
                throw new PermissionException("Location permission not granted");
            }

            _cancellationTokenSource = new CancellationTokenSource();

            var request = new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Medium,
                Timeout = TimeSpan.FromSeconds(10)
            };

            _geolocation.LocationChanged += OnLocationChanged;

            await _geolocation.StartListeningAsync(request, _cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error starting location updates: {ex.Message}");
            throw;
        }
    }

    public async Task StopLocationUpdatesAsync()
    {
        try
        {
            _geolocation.LocationChanged -= OnLocationChanged;
            _cancellationTokenSource?.Cancel();
            await _geolocation.StopListeningAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error stopping location updates: {ex.Message}");
        }
    }

    private void OnLocationChanged(object sender, GeolocationLocationChangedEventArgs e)
    {
        LocationChanged?.Invoke(this, new LocationChangedEventArgs(e.Location));
    }

    public async Task<double> CalculateDistanceAsync(Location location1, Location location2)
    {
        return Location.CalculateDistance(location1, location2, DistanceUnits.Kilometers);
    }

    public async Task<string> GetAddressFromLocationAsync(Location location)
    {
        try
        {
            if (!_connectivity.NetworkAccess.HasFlag(NetworkAccess.Internet))
            {
                return "Sin conexión a internet";
            }

            var placemarks = await _geocoding.GetPlacemarksAsync(location);
            var placemark = placemarks?.FirstOrDefault();

            if (placemark != null)
            {
                return $"{placemark.Thoroughfare} {placemark.Locality}, {placemark.AdminArea}";
            }

            return "Dirección no encontrada";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting address from location: {ex.Message}");
            return "Error al obtener dirección";
        }
    }

    public async Task<Location> GetLocationFromAddressAsync(string address)
    {
        try
        {
            if (!_connectivity.NetworkAccess.HasFlag(NetworkAccess.Internet))
            {
                throw new NoInternetException("Sin conexión a internet");
            }

            var locations = await _geocoding.GetLocationsAsync(address);
            return locations?.FirstOrDefault();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting location from address: {ex.Message}");
            throw;
        }
    }
}
```

### **3. Servicio de Multimedia**

```csharp
// Services/IMediaService.cs
public interface IMediaService
{
    Task<bool> RequestCameraPermissionAsync();
    Task<bool> RequestPhotoPermissionAsync();
    Task<FileResult> TakePhotoAsync();
    Task<FileResult> PickPhotoAsync();
    Task<FileResult> TakeVideoAsync();
    Task<FileResult> PickVideoAsync();
    Task<Stream> GetImageStreamAsync(string filePath);
    Task<string> SaveImageAsync(Stream imageStream, string fileName);
    Task<bool> DeleteFileAsync(string filePath);
}

// Services/MediaService.cs
public class MediaService : IMediaService
{
    private readonly IMediaPicker _mediaPicker;

    public MediaService(IMediaPicker mediaPicker)
    {
        _mediaPicker = mediaPicker;
    }

    public async Task<bool> RequestCameraPermissionAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
            }

            return status == PermissionStatus.Granted;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error requesting camera permission: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> RequestPhotoPermissionAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Photos>();
            
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Photos>();
            }

            return status == PermissionStatus.Granted;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error requesting photo permission: {ex.Message}");
            return false;
        }
    }

    public async Task<FileResult> TakePhotoAsync()
    {
        try
        {
            var hasPermission = await RequestCameraPermissionAsync();
            if (!hasPermission)
            {
                throw new PermissionException("Camera permission not granted");
            }

            var photo = await _mediaPicker.CapturePhotoAsync();
            return photo;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error taking photo: {ex.Message}");
            throw;
        }
    }

    public async Task<FileResult> PickPhotoAsync()
    {
        try
        {
            var hasPermission = await RequestPhotoPermissionAsync();
            if (!hasPermission)
            {
                throw new PermissionException("Photo permission not granted");
            }

            var photo = await _mediaPicker.PickPhotoAsync();
            return photo;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error picking photo: {ex.Message}");
            throw;
        }
    }

    public async Task<FileResult> TakeVideoAsync()
    {
        try
        {
            var hasPermission = await RequestCameraPermissionAsync();
            if (!hasPermission)
            {
                throw new PermissionException("Camera permission not granted");
            }

            var video = await _mediaPicker.CaptureVideoAsync();
            return video;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error taking video: {ex.Message}");
            throw;
        }
    }

    public async Task<FileResult> PickVideoAsync()
    {
        try
        {
            var hasPermission = await RequestPhotoPermissionAsync();
            if (!hasPermission)
            {
                throw new PermissionException("Photo permission not granted");
            }

            var video = await _mediaPicker.PickVideoAsync();
            return video;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error picking video: {ex.Message}");
            throw;
        }
    }

    public async Task<Stream> GetImageStreamAsync(string filePath)
    {
        try
        {
            return await FileSystem.OpenAppPackageFileAsync(filePath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting image stream: {ex.Message}");
            throw;
        }
    }

    public async Task<string> SaveImageAsync(Stream imageStream, string fileName)
    {
        try
        {
            var localPath = Path.Combine(FileSystem.AppDataDirectory, "images", fileName);
            var directory = Path.GetDirectoryName(localPath);
            
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var fileStream = File.Create(localPath);
            await imageStream.CopyToAsync(fileStream);
            
            return localPath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving image: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting file: {ex.Message}");
            return false;
        }
    }
}
```

### **4. ViewModel con Geolocalización**

```csharp
// ViewModels/MapViewModel.cs
public class MapViewModel : BaseViewModel
{
    private readonly ILocationService _locationService;
    private readonly IMusicianRepository _musicianRepository;
    
    private Location _currentLocation;
    private ObservableCollection<Musician> _nearbyMusicians;
    private string _searchRadius;
    private bool _isLocationEnabled;

    public MapViewModel(ILocationService locationService, IMusicianRepository musicianRepository)
    {
        _locationService = locationService;
        _musicianRepository = musicianRepository;
        
        _nearbyMusicians = new ObservableCollection<Musician>();
        _searchRadius = "10"; // km
        
        InitializeLocationAsync();
        
        SearchNearbyCommand = new Command(async () => await SearchNearbyMusiciansAsync());
        RefreshLocationCommand = new Command(async () => await RefreshLocationAsync());
    }

    public Location CurrentLocation
    {
        get => _currentLocation;
        set => SetProperty(ref _currentLocation, value);
    }

    public ObservableCollection<Musician> NearbyMusicians
    {
        get => _nearbyMusicians;
        set => SetProperty(ref _nearbyMusicians, value);
    }

    public string SearchRadius
    {
        get => _searchRadius;
        set => SetProperty(ref _searchRadius, value);
    }

    public bool IsLocationEnabled
    {
        get => _isLocationEnabled;
        set => SetProperty(ref _isLocationEnabled, value);
    }

    public Command SearchNearbyCommand { get; }
    public Command RefreshLocationCommand { get; }

    private async Task InitializeLocationAsync()
    {
        try
        {
            IsBusy = true;
            
            var hasPermission = await _locationService.RequestLocationPermissionAsync();
            IsLocationEnabled = hasPermission;
            
            if (hasPermission)
            {
                CurrentLocation = await _locationService.GetCurrentLocationAsync();
                await SearchNearbyMusiciansAsync();
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

    private async Task SearchNearbyMusiciansAsync()
    {
        try
        {
            if (CurrentLocation == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Ubicación no disponible", "OK");
                return;
            }

            IsBusy = true;
            
            var searchCriteria = new MusicianSearchCriteria
            {
                Location = $"{CurrentLocation.Latitude},{CurrentLocation.Longitude}",
                Radius = double.Parse(SearchRadius)
            };

            var musicians = await _musicianRepository.GetMusiciansAsync(searchCriteria);
            
            // Filtrar por distancia
            var nearbyMusicians = new List<Musician>();
            foreach (var musician in musicians)
            {
                if (musician.Location != null)
                {
                    var distance = await _locationService.CalculateDistanceAsync(
                        CurrentLocation, 
                        musician.Location);
                    
                    if (distance <= double.Parse(SearchRadius))
                    {
                        musician.DistanceFromUser = distance;
                        nearbyMusicians.Add(musician);
                    }
                }
            }

            NearbyMusicians.Clear();
            foreach (var musician in nearbyMusicians.OrderBy(m => m.DistanceFromUser))
            {
                NearbyMusicians.Add(musician);
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

    private async Task RefreshLocationAsync()
    {
        try
        {
            IsBusy = true;
            CurrentLocation = await _locationService.GetCurrentLocationAsync();
            await SearchNearbyMusiciansAsync();
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
}
```

### **5. ViewModel con Multimedia**

```csharp
// ViewModels/ProfileViewModel.cs
public class ProfileViewModel : BaseViewModel
{
    private readonly IMediaService _mediaService;
    private readonly IUserService _userService;
    
    private User _currentUser;
    private string _profileImagePath;
    private bool _isImageLoading;

    public ProfileViewModel(IMediaService mediaService, IUserService userService)
    {
        _mediaService = mediaService;
        _userService = userService;
        
        TakePhotoCommand = new Command(async () => await TakePhotoAsync());
        PickPhotoCommand = new Command(async () => await PickPhotoAsync());
        SaveProfileCommand = new Command(async () => await SaveProfileAsync());
        
        LoadUserProfileAsync();
    }

    public User CurrentUser
    {
        get => _currentUser;
        set => SetProperty(ref _currentUser, value);
    }

    public string ProfileImagePath
    {
        get => _profileImagePath;
        set => SetProperty(ref _profileImagePath, value);
    }

    public bool IsImageLoading
    {
        get => _isImageLoading;
        set => SetProperty(ref _isImageLoading, value);
    }

    public Command TakePhotoCommand { get; }
    public Command PickPhotoCommand { get; }
    public Command SaveProfileCommand { get; }

    private async Task LoadUserProfileAsync()
    {
        try
        {
            IsBusy = true;
            CurrentUser = await _userService.GetCurrentUserAsync();
            ProfileImagePath = CurrentUser?.ProfileImageUrl;
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

    private async Task TakePhotoAsync()
    {
        try
        {
            IsImageLoading = true;
            
            var photo = await _mediaService.TakePhotoAsync();
            if (photo != null)
            {
                await ProcessSelectedImageAsync(photo);
            }
        }
        catch (PermissionException)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Permisos", 
                "Se necesita acceso a la cámara para tomar fotos", 
                "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsImageLoading = false;
        }
    }

    private async Task PickPhotoAsync()
    {
        try
        {
            IsImageLoading = true;
            
            var photo = await _mediaService.PickPhotoAsync();
            if (photo != null)
            {
                await ProcessSelectedImageAsync(photo);
            }
        }
        catch (PermissionException)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Permisos", 
                "Se necesita acceso a la galería para seleccionar fotos", 
                "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsImageLoading = false;
        }
    }

    private async Task ProcessSelectedImageAsync(FileResult photo)
    {
        try
        {
            // Guardar imagen localmente
            var fileName = $"profile_{CurrentUser.Id}_{DateTime.UtcNow.Ticks}.jpg";
            var localPath = await _mediaService.SaveImageAsync(await photo.OpenReadAsync(), fileName);
            
            ProfileImagePath = localPath;
            
            // Subir al servidor (implementar según necesidad)
            // await _userService.UploadProfileImageAsync(localPath);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async Task SaveProfileAsync()
    {
        try
        {
            IsBusy = true;
            
            CurrentUser.ProfileImageUrl = ProfileImagePath;
            await _userService.UpdateUserAsync(CurrentUser);
            
            await Application.Current.MainPage.DisplayAlert(
                "Éxito", 
                "Perfil actualizado correctamente", 
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
}
```

### **6. Vista de Mapa con Geolocalización**

```xml
<!-- Views/MapPage.xaml -->
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage x:Class="MussikOn.Mobile.Views.MapPage"
             xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:MussikOn.Mobile.ViewModels"
             x:DataType="vm:MapViewModel"
             Title="Músicos Cercanos">

    <Grid RowDefinitions="Auto,*,Auto">
        
        <!-- Search Controls -->
        <StackLayout Grid.Row="0" Padding="16" Spacing="12">
            <Grid ColumnDefinitions="*,Auto,Auto" ColumnSpacing="12">
                <Entry Grid.Column="0" 
                       Text="{Binding SearchRadius}" 
                       Placeholder="Radio (km)" 
                       Keyboard="Numeric" />
                <Button Grid.Column="1" 
                        Text="Buscar" 
                        Command="{Binding SearchNearbyCommand}" 
                        Style="{StaticResource PrimaryButton}" />
                <Button Grid.Column="2" 
                        Text="📍" 
                        Command="{Binding RefreshLocationCommand}" 
                        Style="{StaticResource SecondaryButton}" />
            </Grid>
        </StackLayout>

        <!-- Map (Placeholder for actual map control) -->
        <Frame Grid.Row="1" 
               Margin="16" 
               Style="{StaticResource Card}">
            <StackLayout VerticalOptions="Center" HorizontalOptions="Center">
                <Label Text="🗺️" FontSize="48" HorizontalOptions="Center" />
                <Label Text="Mapa de Músicos" 
                       Style="{StaticResource TitleLabel}" 
                       HorizontalOptions="Center" />
                <Label Text="{Binding CurrentLocation, StringFormat='Ubicación: {0:F4}, {1:F4}'}" 
                       Style="{StaticResource CaptionLabel}" 
                       HorizontalOptions="Center" />
            </StackLayout>
        </Frame>

        <!-- Nearby Musicians List -->
        <CollectionView Grid.Row="2" 
                        ItemsSource="{Binding NearbyMusicians}" 
                        HeightRequest="200">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <Grid Padding="16,8" ColumnDefinitions="Auto,*,Auto">
                        <Image Grid.Column="0" 
                               Source="{Binding ProfileImageUrl}" 
                               WidthRequest="50" 
                               HeightRequest="50" 
                               Aspect="AspectFill" />
                        <StackLayout Grid.Column="1" Margin="12,0,0,0">
                            <Label Text="{Binding Name}" 
                                   Style="{StaticResource BodyLabel}" 
                                   FontAttributes="Bold" />
                            <Label Text="{Binding Genres, StringFormat='Géneros: {0}'}" 
                                   Style="{StaticResource CaptionLabel}" />
                            <Label Text="{Binding DistanceFromUser, StringFormat='{0:F1} km'}" 
                                   Style="{StaticResource CaptionLabel}" />
                        </StackLayout>
                        <Button Grid.Column="2" 
                                Text="Ver" 
                                Style="{StaticResource LinkButton}" />
                    </Grid>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>

    </Grid>
</ContentPage>
```

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Notificaciones**
1. Implementar notificaciones locales
2. Configurar push notifications
3. Manejar permisos de notificación

### **Ejercicio 2: Geolocalización**
1. Implementar servicios de ubicación
2. Crear vista de mapa
3. Buscar músicos cercanos

### **Ejercicio 3: Multimedia**
1. Implementar captura de fotos
2. Manejar selección de galería
3. Procesar y guardar imágenes

## 📝 **Resumen de la Clase**

En esta clase hemos aprendido:

✅ **Sistema de notificaciones** push y locales
✅ **Geolocalización** y servicios de ubicación
✅ **Multimedia** y manejo de cámara
✅ **Permisos** y manejo de errores
✅ **Integración** con servicios nativos
✅ **UX** para funcionalidades móviles

## 🚀 **Próxima Clase**

En la siguiente clase implementaremos:
- **Optimización de rendimiento**
- **Manejo de memoria**
- **Testing** en dispositivos móviles
- **Deployment** y distribución

---

**💡 Tip del Día**: Las funcionalidades nativas como notificaciones, geolocalización y multimedia son clave para una experiencia móvil completa. Siempre maneja los permisos y errores apropiadamente.
