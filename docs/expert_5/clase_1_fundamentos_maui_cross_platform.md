# 🚀 **Clase 1: Fundamentos de .NET MAUI y Desarrollo Cross-Platform**

## 🎯 **Objetivos de la Clase**
- Comprender los fundamentos de .NET MAUI
- Configurar el entorno de desarrollo
- Crear la primera aplicación cross-platform
- Entender la arquitectura MVVM
- Implementar navegación básica

## 📚 **Contenido Teórico**

### **1. ¿Qué es .NET MAUI?**

**.NET Multi-platform App UI (MAUI)** es el framework de Microsoft para crear aplicaciones nativas que se ejecutan en múltiples plataformas desde una sola base de código.

#### **Plataformas Soportadas:**
- **iOS** (iPhone, iPad)
- **Android** (teléfonos y tablets)
- **Windows** (WinUI 3)
- **macOS** (Catalina y superior)

### **2. Arquitectura de .NET MAUI**

```
┌─────────────────────────────────────┐
│           .NET MAUI App             │
├─────────────────────────────────────┤
│         Shared Code                 │
│  - Views (XAML)                     │
│  - ViewModels (C#)                  │
│  - Models (C#)                      │
│  - Services (C#)                    │
├─────────────────────────────────────┤
│      Platform-Specific Code         │
│  - iOS (UIKit)                      │
│  - Android (Android Views)          │
│  - Windows (WinUI 3)                │
│  - macOS (AppKit)                   │
└─────────────────────────────────────┘
```

### **3. Patrón MVVM en .NET MAUI**

**Model-View-ViewModel (MVVM)** es el patrón arquitectónico recomendado:

- **Model**: Representa los datos y la lógica de negocio
- **View**: Interfaz de usuario (XAML)
- **ViewModel**: Conecta View y Model, maneja la lógica de presentación

### **4. Estructura de Proyecto MAUI**

```
MussikOn.Mobile/
├── Platforms/
│   ├── Android/
│   ├── iOS/
│   ├── Windows/
│   └── MacCatalyst/
├── Views/
├── ViewModels/
├── Models/
├── Services/
├── Resources/
└── App.xaml
```

## 💻 **Implementación Práctica**

### **1. Configuración del Proyecto**

```csharp
// App.xaml.cs
public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
    }
}
```

### **2. Modelo de Usuario**

```csharp
// Models/User.cs
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string ProfileImageUrl { get; set; }
    public UserType Type { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum UserType
{
    Musician,
    EventOrganizer
}
```

### **3. ViewModel Base**

```csharp
// ViewModels/BaseViewModel.cs
public class BaseViewModel : INotifyPropertyChanged
{
    private bool _isBusy;
    private string _title;

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
```

### **4. ViewModel de Login**

```csharp
// ViewModels/LoginViewModel.cs
public class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private string _email;
    private string _password;
    private bool _rememberMe;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
        LoginCommand = new Command(async () => await LoginAsync());
        RegisterCommand = new Command(async () => await RegisterAsync());
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public bool RememberMe
    {
        get => _rememberMe;
        set => SetProperty(ref _rememberMe, value);
    }

    public Command LoginCommand { get; }
    public Command RegisterCommand { get; }

    private async Task LoginAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor completa todos los campos", "OK");
                return;
            }

            var result = await _authService.LoginAsync(Email, Password, RememberMe);
            
            if (result.Success)
            {
                await Shell.Current.GoToAsync("//main");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", result.ErrorMessage, "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error inesperado: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RegisterAsync()
    {
        await Shell.Current.GoToAsync("register");
    }
}
```

### **5. Vista de Login (XAML)**

```xml
<!-- Views/LoginPage.xaml -->
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage x:Class="MussikOn.Mobile.Views.LoginPage"
             xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:MussikOn.Mobile.ViewModels"
             x:DataType="vm:LoginViewModel"
             Title="Iniciar Sesión">

    <ScrollView>
        <StackLayout Padding="20" Spacing="20">
            
            <!-- Logo -->
            <Image Source="logo.png" 
                   HeightRequest="120" 
                   HorizontalOptions="Center" />
            
            <!-- Título -->
            <Label Text="Bienvenido a MussikOn" 
                   FontSize="24" 
                   FontAttributes="Bold" 
                   HorizontalOptions="Center" />
            
            <!-- Email -->
            <Entry Placeholder="Email" 
                   Text="{Binding Email}" 
                   Keyboard="Email" />
            
            <!-- Contraseña -->
            <Entry Placeholder="Contraseña" 
                   Text="{Binding Password}" 
                   IsPassword="True" />
            
            <!-- Recordar -->
            <CheckBox IsChecked="{Binding RememberMe}" />
            <Label Text="Recordar sesión" />
            
            <!-- Botón Login -->
            <Button Text="Iniciar Sesión" 
                    Command="{Binding LoginCommand}" 
                    IsEnabled="{Binding IsBusy, Converter={StaticResource InvertedBoolConverter}}" />
            
            <!-- Botón Registro -->
            <Button Text="Crear Cuenta" 
                    Command="{Binding RegisterCommand}" 
                    Style="{StaticResource LinkButtonStyle}" />
            
            <!-- Indicador de carga -->
            <ActivityIndicator IsVisible="{Binding IsBusy}" 
                               IsRunning="{Binding IsBusy}" />
            
        </StackLayout>
    </ScrollView>
</ContentPage>
```

### **6. Code-Behind de la Vista**

```csharp
// Views/LoginPage.xaml.cs
public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
```

### **7. Servicio de Autenticación**

```csharp
// Services/IAuthService.cs
public interface IAuthService
{
    Task<AuthResult> LoginAsync(string email, string password, bool rememberMe);
    Task<AuthResult> RegisterAsync(string email, string password, string name, UserType userType);
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<User> GetCurrentUserAsync();
}

// Services/AuthService.cs
public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ISecureStorage _secureStorage;
    private User _currentUser;

    public AuthService(HttpClient httpClient, ISecureStorage secureStorage)
    {
        _httpClient = httpClient;
        _secureStorage = secureStorage;
    }

    public async Task<AuthResult> LoginAsync(string email, string password, bool rememberMe)
    {
        try
        {
            var loginRequest = new
            {
                Email = email,
                Password = password,
                RememberMe = rememberMe
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/auth/login", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var authResult = JsonSerializer.Deserialize<AuthResult>(responseContent);
                
                if (authResult.Success)
                {
                    await _secureStorage.SetAsync("auth_token", authResult.Token);
                    _currentUser = authResult.User;
                }
                
                return authResult;
            }
            else
            {
                return new AuthResult { Success = false, ErrorMessage = "Error de autenticación" };
            }
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await _secureStorage.GetAsync("auth_token");
        return !string.IsNullOrEmpty(token);
    }

    public async Task<User> GetCurrentUserAsync()
    {
        if (_currentUser != null)
            return _currentUser;

        var token = await _secureStorage.GetAsync("auth_token");
        if (string.IsNullOrEmpty(token))
            return null;

        // Obtener usuario actual desde la API
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.GetAsync("/api/auth/me");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            _currentUser = JsonSerializer.Deserialize<User>(content);
        }

        return _currentUser;
    }

    public async Task LogoutAsync()
    {
        await _secureStorage.RemoveAsync("auth_token");
        _currentUser = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
}
```

### **8. Configuración de Dependencias**

```csharp
// MauiProgram.cs
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Configurar HttpClient
        builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
        {
            client.BaseAddress = new Uri("https://api.mussikon.com");
        });

        // Registrar servicios
        builder.Services.AddSingleton<ISecureStorage, SecureStorage>();
        builder.Services.AddTransient<IAuthService, AuthService>();

        // Registrar ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<MainViewModel>();

        // Registrar Views
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<MainPage>();

        return builder.Build();
    }
}
```

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Configuración Inicial**
1. Crear un nuevo proyecto .NET MAUI
2. Configurar la estructura de carpetas
3. Implementar el patrón MVVM básico

### **Ejercicio 2: Pantalla de Login**
1. Crear la vista de login con XAML
2. Implementar el ViewModel correspondiente
3. Conectar con el servicio de autenticación

### **Ejercicio 3: Navegación**
1. Configurar Shell navigation
2. Implementar navegación entre pantallas
3. Manejar parámetros de navegación

## 📝 **Resumen de la Clase**

En esta clase hemos aprendido:

✅ **Fundamentos de .NET MAUI** y desarrollo cross-platform
✅ **Arquitectura MVVM** y su implementación
✅ **Configuración del proyecto** y estructura de carpetas
✅ **Servicios de autenticación** con HttpClient
✅ **Navegación básica** con Shell
✅ **Manejo de estado** con ViewModels

## 🚀 **Próxima Clase**

En la siguiente clase implementaremos:
- **Navegación avanzada** con Shell
- **Temas y estilos** personalizados
- **Recursos y localización**
- **Manejo de errores** y validaciones

---

**💡 Tip del Día**: .NET MAUI te permite desarrollar una sola aplicación que funciona en todas las plataformas principales, reduciendo significativamente el tiempo de desarrollo y mantenimiento.
