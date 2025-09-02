# 🚀 **Clase 2: Navegación Avanzada, Temas y Estilos**

## 🎯 **Objetivos de la Clase**
- Implementar navegación avanzada con Shell
- Crear temas y estilos personalizados
- Configurar recursos y localización
- Manejar errores y validaciones
- Implementar animaciones y transiciones

## 📚 **Contenido Teórico**

### **1. Navegación con Shell**

**Shell** es el sistema de navegación moderno de .NET MAUI que proporciona:
- Navegación jerárquica
- Flyout y TabBar
- Deep linking
- Navegación con parámetros

### **2. Estructura de Navegación**

```
AppShell
├── FlyoutItem (Menú lateral)
│   ├── TabBar (Navegación por tabs)
│   │   ├── HomePage
│   │   ├── SearchPage
│   │   └── ProfilePage
│   └── SettingsPage
└── LoginPage (Modal)
```

### **3. Temas y Estilos**

Los temas permiten:
- Consistencia visual
- Modo oscuro/claro
- Personalización de marca
- Reutilización de estilos

## 💻 **Implementación Práctica**

### **1. AppShell Configuration**

```csharp
// AppShell.xaml
<?xml version="1.0" encoding="utf-8" ?>
<Shell
    x:Class="MussikOn.Mobile.AppShell"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:views="clr-namespace:MussikOn.Mobile.Views"
    Title="MussikOn"
    FlyoutBehavior="Flyout">

    <!-- Flyout Header -->
    <Shell.FlyoutHeaderTemplate>
        <DataTemplate>
            <Grid BackgroundColor="{AppThemeBinding Light={StaticResource Primary}, Dark={StaticResource PrimaryDark}}"
                  HeightRequest="200">
                <StackLayout VerticalOptions="Center" HorizontalOptions="Center">
                    <Image Source="logo.png" HeightRequest="80" />
                    <Label Text="MussikOn" 
                           FontSize="24" 
                           FontAttributes="Bold" 
                           TextColor="White" />
                </StackLayout>
            </Grid>
        </DataTemplate>
    </Shell.FlyoutHeaderTemplate>

    <!-- Main Tab Bar -->
    <TabBar x:Name="MainTabBar">
        <ShellContent Title="Inicio" 
                      Icon="home.png" 
                      ContentTemplate="{DataTemplate views:HomePage}" />
        <ShellContent Title="Buscar" 
                      Icon="search.png" 
                      ContentTemplate="{DataTemplate views:SearchPage}" />
        <ShellContent Title="Chats" 
                      Icon="chat.png" 
                      ContentTemplate="{DataTemplate views:ChatsPage}" />
        <ShellContent Title="Perfil" 
                      Icon="profile.png" 
                      ContentTemplate="{DataTemplate views:ProfilePage}" />
    </TabBar>

    <!-- Flyout Items -->
    <FlyoutItem Title="Configuración" 
                Icon="settings.png">
        <ShellContent ContentTemplate="{DataTemplate views:SettingsPage}" />
    </FlyoutItem>

    <FlyoutItem Title="Ayuda" 
                Icon="help.png">
        <ShellContent ContentTemplate="{DataTemplate views:HelpPage}" />
    </FlyoutItem>

    <!-- Modal Pages -->
    <ShellContent x:Name="LoginShellContent" 
                  Title="Login" 
                  ContentTemplate="{DataTemplate views:LoginPage}" />

</Shell>
```

### **2. AppShell Code-Behind**

```csharp
// AppShell.xaml.cs
public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        RegisterRoutes();
    }

    private void RegisterRoutes()
    {
        // Registrar rutas para navegación
        Routing.RegisterRoute("musician-detail", typeof(MusicianDetailPage));
        Routing.RegisterRoute("event-detail", typeof(EventDetailPage));
        Routing.RegisterRoute("chat-detail", typeof(ChatDetailPage));
        Routing.RegisterRoute("edit-profile", typeof(EditProfilePage));
        Routing.RegisterRoute("create-event", typeof(CreateEventPage));
        Routing.RegisterRoute("apply-musician", typeof(ApplyMusicianPage));
    }

    public async Task NavigateToLoginAsync()
    {
        await GoToAsync("//LoginShellContent");
    }

    public async Task NavigateToMainAsync()
    {
        await GoToAsync("//MainTabBar");
    }
}
```

### **3. Recursos y Estilos**

```xml
<!-- Resources/Styles.xaml -->
<?xml version="1.0" encoding="utf-8" ?>
<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">

    <!-- Colores -->
    <Color x:Key="Primary">#6366F1</Color>
    <Color x:Key="PrimaryDark">#4F46E5</Color>
    <Color x:Key="Secondary">#EC4899</Color>
    <Color x:Key="Accent">#F59E0B</Color>
    <Color x:Key="Success">#10B981</Color>
    <Color x:Key="Warning">#F59E0B</Color>
    <Color x:Key="Error">#EF4444</Color>
    <Color x:Key="Background">#FFFFFF</Color>
    <Color x:Key="Surface">#F8FAFC</Color>
    <Color x:Key="OnSurface">#1E293B</Color>

    <!-- Estilos de Botones -->
    <Style x:Key="PrimaryButton" TargetType="Button">
        <Setter Property="BackgroundColor" Value="{StaticResource Primary}" />
        <Setter Property="TextColor" Value="White" />
        <Setter Property="FontSize" Value="16" />
        <Setter Property="FontAttributes" Value="Bold" />
        <Setter Property="CornerRadius" Value="8" />
        <Setter Property="Padding" Value="16,12" />
    </Style>

    <Style x:Key="SecondaryButton" TargetType="Button">
        <Setter Property="BackgroundColor" Value="Transparent" />
        <Setter Property="TextColor" Value="{StaticResource Primary}" />
        <Setter Property="FontSize" Value="16" />
        <Setter Property="BorderColor" Value="{StaticResource Primary}" />
        <Setter Property="BorderWidth" Value="2" />
        <Setter Property="CornerRadius" Value="8" />
        <Setter Property="Padding" Value="16,12" />
    </Style>

    <Style x:Key="LinkButton" TargetType="Button">
        <Setter Property="BackgroundColor" Value="Transparent" />
        <Setter Property="TextColor" Value="{StaticResource Primary}" />
        <Setter Property="FontSize" Value="14" />
        <Setter Property="TextDecorations" Value="Underline" />
    </Style>

    <!-- Estilos de Labels -->
    <Style x:Key="TitleLabel" TargetType="Label">
        <Setter Property="FontSize" Value="24" />
        <Setter Property="FontAttributes" Value="Bold" />
        <Setter Property="TextColor" Value="{StaticResource OnSurface}" />
    </Style>

    <Style x:Key="SubtitleLabel" TargetType="Label">
        <Setter Property="FontSize" Value="18" />
        <Setter Property="FontAttributes" Value="Bold" />
        <Setter Property="TextColor" Value="{StaticResource OnSurface}" />
    </Style>

    <Style x:Key="BodyLabel" TargetType="Label">
        <Setter Property="FontSize" Value="16" />
        <Setter Property="TextColor" Value="{StaticResource OnSurface}" />
    </Style>

    <Style x:Key="CaptionLabel" TargetType="Label">
        <Setter Property="FontSize" Value="12" />
        <Setter Property="TextColor" Value="{StaticResource OnSurface}" />
        <Setter Property="Opacity" Value="0.7" />
    </Style>

    <!-- Estilos de Entries -->
    <Style x:Key="PrimaryEntry" TargetType="Entry">
        <Setter Property="BackgroundColor" Value="{StaticResource Surface}" />
        <Setter Property="TextColor" Value="{StaticResource OnSurface}" />
        <Setter Property="PlaceholderColor" Value="{StaticResource OnSurface}" />
        <Setter Property="PlaceholderColor" Value="0.5" />
        <Setter Property="FontSize" Value="16" />
        <Setter Property="Padding" Value="12" />
    </Style>

    <!-- Estilos de Cards -->
    <Style x:Key="Card" TargetType="Frame">
        <Setter Property="BackgroundColor" Value="{StaticResource Background}" />
        <Setter Property="BorderColor" Value="{StaticResource Surface}" />
        <Setter Property="CornerRadius" Value="12" />
        <Setter Property="Padding" Value="16" />
        <Setter Property="HasShadow" Value="True" />
    </Style>

    <!-- Converters -->
    <ResourceDictionary>
        <local:InvertedBoolConverter x:Key="InvertedBoolConverter" />
        <local:DateTimeToStringConverter x:Key="DateTimeToStringConverter" />
        <local:UserTypeToStringConverter x:Key="UserTypeToStringConverter" />
    </ResourceDictionary>

</ResourceDictionary>
```

### **4. Converters**

```csharp
// Converters/InvertedBoolConverter.cs
public class InvertedBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;
        return false;
    }
}

// Converters/DateTimeToStringConverter.cs
public class DateTimeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            var format = parameter?.ToString() ?? "dd/MM/yyyy HH:mm";
            return dateTime.ToString(format);
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue && DateTime.TryParse(stringValue, out DateTime result))
            return result;
        return DateTime.MinValue;
    }
}

// Converters/UserTypeToStringConverter.cs
public class UserTypeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is UserType userType)
        {
            return userType switch
            {
                UserType.Musician => "Músico",
                UserType.EventOrganizer => "Organizador",
                _ => "Usuario"
            };
        }
        return "Usuario";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            return stringValue switch
            {
                "Músico" => UserType.Musician,
                "Organizador" => UserType.EventOrganizer,
                _ => UserType.Musician
            };
        }
        return UserType.Musician;
    }
}
```

### **5. HomePage con Navegación**

```xml
<!-- Views/HomePage.xaml -->
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage x:Class="MussikOn.Mobile.Views.HomePage"
             xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:MussikOn.Mobile.ViewModels"
             x:DataType="vm:HomeViewModel"
             Title="Inicio">

    <RefreshView IsRefreshing="{Binding IsRefreshing}" 
                 Command="{Binding RefreshCommand}">
        <ScrollView>
            <StackLayout Padding="16" Spacing="16">
                
                <!-- Welcome Section -->
                <Frame Style="{StaticResource Card}">
                    <StackLayout>
                        <Label Text="{Binding WelcomeMessage}" 
                               Style="{StaticResource TitleLabel}" />
                        <Label Text="Encuentra músicos para tu evento o aplica a oportunidades" 
                               Style="{StaticResource BodyLabel}" />
                    </StackLayout>
                </Frame>

                <!-- Quick Actions -->
                <Label Text="Acciones Rápidas" 
                       Style="{StaticResource SubtitleLabel}" />
                
                <Grid ColumnDefinitions="*,*" RowDefinitions="*,*" ColumnSpacing="12" RowSpacing="12">
                    <Button Grid.Row="0" Grid.Column="0" 
                            Text="Buscar Músicos" 
                            Style="{StaticResource PrimaryButton}"
                            Command="{Binding SearchMusiciansCommand}" />
                    <Button Grid.Row="0" Grid.Column="1" 
                            Text="Crear Evento" 
                            Style="{StaticResource SecondaryButton}"
                            Command="{Binding CreateEventCommand}" />
                    <Button Grid.Row="1" Grid.Column="0" 
                            Text="Ver Aplicaciones" 
                            Style="{StaticResource SecondaryButton}"
                            Command="{Binding ViewApplicationsCommand}" />
                    <Button Grid.Row="1" Grid.Column="1" 
                            Text="Mis Chats" 
                            Style="{StaticResource SecondaryButton}"
                            Command="{Binding ViewChatsCommand}" />
                </Grid>

                <!-- Recent Activity -->
                <Label Text="Actividad Reciente" 
                       Style="{StaticResource SubtitleLabel}" />
                
                <CollectionView ItemsSource="{Binding RecentActivity}">
                    <CollectionView.ItemTemplate>
                        <DataTemplate>
                            <Grid Padding="0,8">
                                <Frame Style="{StaticResource Card}">
                                    <Grid ColumnDefinitions="Auto,*,Auto" RowDefinitions="Auto,Auto">
                                        <Image Grid.Row="0" Grid.Column="0" 
                                               Source="{Binding Icon}" 
                                               WidthRequest="40" 
                                               HeightRequest="40" />
                                        <StackLayout Grid.Row="0" Grid.Column="1" 
                                                     Grid.RowSpan="2" 
                                                     Margin="12,0,0,0">
                                            <Label Text="{Binding Title}" 
                                                   Style="{StaticResource BodyLabel}" 
                                                   FontAttributes="Bold" />
                                            <Label Text="{Binding Description}" 
                                                   Style="{StaticResource CaptionLabel}" />
                                        </StackLayout>
                                        <Label Grid.Row="0" Grid.Column="2" 
                                               Text="{Binding Time, Converter={StaticResource DateTimeToStringConverter}, ConverterParameter='HH:mm'}" 
                                               Style="{StaticResource CaptionLabel}" />
                                    </Grid>
                                </Frame>
                            </Grid>
                        </DataTemplate>
                    </CollectionView.ItemTemplate>
                </CollectionView>

            </StackLayout>
        </ScrollView>
    </RefreshView>
</ContentPage>
```

### **6. HomeViewModel**

```csharp
// ViewModels/HomeViewModel.cs
public class HomeViewModel : BaseViewModel
{
    private readonly IUserService _userService;
    private readonly INavigationService _navigationService;
    private User _currentUser;
    private ObservableCollection<ActivityItem> _recentActivity;

    public HomeViewModel(IUserService userService, INavigationService navigationService)
    {
        _userService = userService;
        _navigationService = navigationService;
        _recentActivity = new ObservableCollection<ActivityItem>();
        
        RefreshCommand = new Command(async () => await RefreshAsync());
        SearchMusiciansCommand = new Command(async () => await SearchMusiciansAsync());
        CreateEventCommand = new Command(async () => await CreateEventAsync());
        ViewApplicationsCommand = new Command(async () => await ViewApplicationsAsync());
        ViewChatsCommand = new Command(async () => await ViewChatsAsync());
        
        LoadDataAsync();
    }

    public string WelcomeMessage
    {
        get
        {
            if (_currentUser == null) return "¡Bienvenido a MussikOn!";
            return $"¡Hola, {_currentUser.Name}!";
        }
    }

    public ObservableCollection<ActivityItem> RecentActivity
    {
        get => _recentActivity;
        set => SetProperty(ref _recentActivity, value);
    }

    public Command RefreshCommand { get; }
    public Command SearchMusiciansCommand { get; }
    public Command CreateEventCommand { get; }
    public Command ViewApplicationsCommand { get; }
    public Command ViewChatsCommand { get; }

    private async Task LoadDataAsync()
    {
        try
        {
            IsBusy = true;
            
            _currentUser = await _userService.GetCurrentUserAsync();
            OnPropertyChanged(nameof(WelcomeMessage));
            
            await LoadRecentActivityAsync();
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
        await LoadDataAsync();
    }

    private async Task SearchMusiciansAsync()
    {
        await _navigationService.NavigateToAsync("search");
    }

    private async Task CreateEventAsync()
    {
        await _navigationService.NavigateToAsync("create-event");
    }

    private async Task ViewApplicationsAsync()
    {
        await _navigationService.NavigateToAsync("applications");
    }

    private async Task ViewChatsAsync()
    {
        await _navigationService.NavigateToAsync("chats");
    }

    private async Task LoadRecentActivityAsync()
    {
        try
        {
            var activities = await _userService.GetRecentActivityAsync();
            RecentActivity.Clear();
            
            foreach (var activity in activities)
            {
                RecentActivity.Add(activity);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't show to user for non-critical data
            System.Diagnostics.Debug.WriteLine($"Error loading recent activity: {ex.Message}");
        }
    }
}
```

### **7. Navigation Service**

```csharp
// Services/INavigationService.cs
public interface INavigationService
{
    Task NavigateToAsync(string route, Dictionary<string, object> parameters = null);
    Task NavigateBackAsync();
    Task NavigateToModalAsync(string route, Dictionary<string, object> parameters = null);
    Task CloseModalAsync();
}

// Services/NavigationService.cs
public class NavigationService : INavigationService
{
    public async Task NavigateToAsync(string route, Dictionary<string, object> parameters = null)
    {
        try
        {
            if (parameters != null && parameters.Any())
            {
                await Shell.Current.GoToAsync(route, parameters);
            }
            else
            {
                await Shell.Current.GoToAsync(route);
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error de navegación: {ex.Message}", "OK");
        }
    }

    public async Task NavigateBackAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error de navegación: {ex.Message}", "OK");
        }
    }

    public async Task NavigateToModalAsync(string route, Dictionary<string, object> parameters = null)
    {
        try
        {
            if (parameters != null && parameters.Any())
            {
                await Shell.Current.GoToAsync(route, parameters);
            }
            else
            {
                await Shell.Current.GoToAsync(route);
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error de navegación: {ex.Message}", "OK");
        }
    }

    public async Task CloseModalAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error de navegación: {ex.Message}", "OK");
        }
    }
}
```

### **8. Manejo de Errores Global**

```csharp
// Services/ErrorHandlingService.cs
public class ErrorHandlingService
{
    public static async Task HandleExceptionAsync(Exception exception, string context = "")
    {
        var errorMessage = GetUserFriendlyMessage(exception);
        
        await Application.Current.MainPage.DisplayAlert(
            "Error", 
            $"{context}\n{errorMessage}", 
            "OK");
        
        // Log the actual exception for debugging
        System.Diagnostics.Debug.WriteLine($"Exception in {context}: {exception}");
    }

    private static string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            HttpRequestException => "Error de conexión. Verifica tu internet.",
            UnauthorizedAccessException => "No tienes permisos para realizar esta acción.",
            ArgumentException => "Datos inválidos proporcionados.",
            TimeoutException => "La operación tardó demasiado. Intenta nuevamente.",
            _ => "Ha ocurrido un error inesperado. Intenta nuevamente."
        };
    }
}
```

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Configurar Shell Navigation**
1. Crear AppShell con Flyout y TabBar
2. Registrar rutas de navegación
3. Implementar navegación entre páginas

### **Ejercicio 2: Temas y Estilos**
1. Crear ResourceDictionary con estilos
2. Implementar modo oscuro/claro
3. Crear componentes reutilizables

### **Ejercicio 3: Navegación con Parámetros**
1. Implementar navegación con datos
2. Manejar parámetros de entrada
3. Validar datos de navegación

## 📝 **Resumen de la Clase**

En esta clase hemos aprendido:

✅ **Navegación avanzada** con Shell
✅ **Temas y estilos** personalizados
✅ **Recursos y convertidores**
✅ **Servicios de navegación**
✅ **Manejo de errores** global
✅ **Arquitectura de navegación** robusta

## 🚀 **Próxima Clase**

En la siguiente clase implementaremos:
- **Integración con APIs** REST
- **Manejo de datos** offline
- **Caché y sincronización**
- **Gestión de estado** avanzada

---

**💡 Tip del Día**: Usa Shell para crear una experiencia de navegación consistente y familiar para los usuarios, similar a las aplicaciones nativas de cada plataforma.
