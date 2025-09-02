# 🚀 **Clase 7: Deployment y Distribución en App Stores**

## 🎯 **Objetivos de la Clase**
- Configurar deployment para iOS y Android
- Implementar code signing y certificados
- Preparar aplicaciones para App Store y Google Play
- Configurar release management
- Implementar distribución automática

## 📚 **Contenido Teórico**

### **1. Deployment de Aplicaciones Móviles**

**iOS Deployment:**
- **Provisioning Profiles**: Certificados de desarrollo y distribución
- **Code Signing**: Firma de código con certificados Apple
- **App Store Connect**: Portal de gestión de aplicaciones
- **TestFlight**: Distribución beta

**Android Deployment:**
- **Keystore**: Certificados de firma de aplicaciones
- **Google Play Console**: Portal de gestión de aplicaciones
- **Internal Testing**: Distribución beta interna
- **Closed Testing**: Distribución beta cerrada

### **2. Release Management**

Proceso de release:
- **Versioning**: Control de versiones semántico
- **Build Configuration**: Configuraciones de build
- **Automated Testing**: Testing automático en CI/CD
- **Release Notes**: Documentación de cambios

### **3. Distribución y Marketing**

Estrategias de distribución:
- **App Store Optimization (ASO)**: Optimización para stores
- **Metadata**: Títulos, descripciones, keywords
- **Screenshots**: Capturas de pantalla atractivas
- **App Icons**: Iconos optimizados

## 💻 **Implementación Práctica**

### **1. Configuración de Build para iOS**

```xml
<!-- Platforms/iOS/Info.plist -->
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleDisplayName</key>
    <string>MussikOn</string>
    <key>CFBundleIdentifier</key>
    <string>com.mussikon.mobile</string>
    <key>CFBundleVersion</key>
    <string>1.0.0</string>
    <key>CFBundleShortVersionString</key>
    <string>1.0.0</string>
    
    <!-- Permissions -->
    <key>NSCameraUsageDescription</key>
    <string>MussikOn necesita acceso a la cámara para tomar fotos de perfil</string>
    <key>NSPhotoLibraryUsageDescription</key>
    <string>MussikOn necesita acceso a la galería para seleccionar fotos</string>
    <key>NSLocationWhenInUseUsageDescription</key>
    <string>MussikOn necesita acceso a la ubicación para encontrar músicos cercanos</string>
    <key>NSMicrophoneUsageDescription</key>
    <string>MussikOn necesita acceso al micrófono para grabar audios</string>
    
    <!-- URL Schemes -->
    <key>CFBundleURLTypes</key>
    <array>
        <dict>
            <key>CFBundleURLName</key>
            <string>com.mussikon.mobile</string>
            <key>CFBundleURLSchemes</key>
            <array>
                <string>mussikon</string>
            </array>
        </dict>
    </array>
    
    <!-- Background Modes -->
    <key>UIBackgroundModes</key>
    <array>
        <string>background-fetch</string>
        <string>remote-notification</string>
    </array>
    
    <!-- App Transport Security -->
    <key>NSAppTransportSecurity</key>
    <dict>
        <key>NSAllowsArbitraryLoads</key>
        <false/>
        <key>NSExceptionDomains</key>
        <dict>
            <key>api.mussikon.com</key>
            <dict>
                <key>NSExceptionAllowsInsecureHTTPLoads</key>
                <false/>
                <key>NSExceptionMinimumTLSVersion</key>
                <string>TLSv1.2</string>
            </dict>
        </dict>
    </dict>
</dict>
</plist>
```

### **2. Configuración de Build para Android**

```xml
<!-- Platforms/Android/AndroidManifest.xml -->
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
    
    <!-- Permissions -->
    <uses-permission android:name="android.permission.INTERNET" />
    <uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
    <uses-permission android:name="android.permission.CAMERA" />
    <uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE" />
    <uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
    <uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
    <uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
    <uses-permission android:name="android.permission.RECORD_AUDIO" />
    <uses-permission android:name="android.permission.WAKE_LOCK" />
    <uses-permission android:name="android.permission.VIBRATE" />
    <uses-permission android:name="com.google.android.c2dm.permission.RECEIVE" />
    
    <!-- Features -->
    <uses-feature android:name="android.hardware.camera" android:required="false" />
    <uses-feature android:name="android.hardware.camera.autofocus" android:required="false" />
    <uses-feature android:name="android.hardware.location" android:required="false" />
    <uses-feature android:name="android.hardware.location.gps" android:required="false" />
    
    <application android:allowBackup="true"
                 android:icon="@mipmap/appicon"
                 android:label="@string/app_name"
                 android:theme="@style/Maui.SplashTheme"
                 android:usesCleartextTraffic="false">
        
        <activity android:name=".MainActivity"
                  android:exported="true"
                  android:launchMode="singleTop"
                  android:theme="@style/Maui.SplashTheme"
                  android:windowSoftInputMode="adjustResize">
            <intent-filter>
                <action android:name="android.intent.action.MAIN" />
                <category android:name="android.intent.category.LAUNCHER" />
            </intent-filter>
        </activity>
        
        <!-- Push Notifications -->
        <service android:name=".FirebaseMessagingService"
                 android:exported="false">
            <intent-filter>
                <action android:name="com.google.firebase.MESSAGING_EVENT" />
            </intent-filter>
        </service>
        
        <!-- Deep Links -->
        <activity android:name=".DeepLinkActivity"
                  android:exported="true">
            <intent-filter>
                <action android:name="android.intent.action.VIEW" />
                <category android:name="android.intent.category.DEFAULT" />
                <category android:name="android.intent.category.BROWSABLE" />
                <data android:scheme="mussikon" />
            </intent-filter>
        </activity>
        
    </application>
</manifest>
```

### **3. Configuración de Build Scripts**

```bash
#!/bin/bash
# build-ios.sh

set -e

echo "🚀 Building iOS App for MussikOn"

# Variables
APP_NAME="MussikOn"
BUNDLE_ID="com.mussikon.mobile"
VERSION="1.0.0"
BUILD_NUMBER="1"

# Clean previous builds
echo "🧹 Cleaning previous builds..."
rm -rf bin/
rm -rf obj/

# Restore packages
echo "📦 Restoring packages..."
dotnet restore

# Build iOS app
echo "🔨 Building iOS app..."
dotnet build -f net8.0-ios -c Release

# Archive iOS app
echo "📦 Archiving iOS app..."
dotnet publish -f net8.0-ios -c Release -o ./bin/Release/net8.0-ios/publish/

echo "✅ iOS build completed successfully!"
```

```bash
#!/bin/bash
# build-android.sh

set -e

echo "🚀 Building Android App for MussikOn"

# Variables
APP_NAME="MussikOn"
PACKAGE_NAME="com.mussikon.mobile"
VERSION="1.0.0"
VERSION_CODE="1"

# Clean previous builds
echo "🧹 Cleaning previous builds..."
rm -rf bin/
rm -rf obj/

# Restore packages
echo "📦 Restoring packages..."
dotnet restore

# Build Android app
echo "🔨 Building Android app..."
dotnet build -f net8.0-android -c Release

# Create APK
echo "📦 Creating APK..."
dotnet publish -f net8.0-android -c Release -o ./bin/Release/net8.0-android/publish/

# Sign APK (if keystore is available)
if [ -f "mussikon-release-key.keystore" ]; then
    echo "🔐 Signing APK..."
    jarsigner -verbose -sigalg SHA1withRSA -digestalg SHA1 \
        -keystore mussikon-release-key.keystore \
        ./bin/Release/net8.0-android/publish/com.mussikon.mobile-Signed.apk \
        mussikon-key
    
    echo "✅ Android build completed successfully!"
else
    echo "⚠️  Keystore not found. APK is unsigned."
fi
```

### **4. GitHub Actions para CI/CD**

```yaml
# .github/workflows/mobile-deploy.yml
name: Mobile Deploy

on:
  push:
    tags:
      - 'v*'
  workflow_dispatch:

jobs:
  build-ios:
    runs-on: macos-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Setup Xcode
      uses: maxim-lobanov/setup-xcode@v1
      with:
        xcode-version: latest-stable
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build iOS
      run: dotnet build -f net8.0-ios -c Release
    
    - name: Archive iOS
      run: dotnet publish -f net8.0-ios -c Release -o ./bin/Release/net8.0-ios/publish/
    
    - name: Upload iOS artifacts
      uses: actions/upload-artifact@v3
      with:
        name: ios-build
        path: ./bin/Release/net8.0-ios/publish/

  build-android:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Setup Java
      uses: actions/setup-java@v3
      with:
        distribution: 'temurin'
        java-version: '11'
    
    - name: Setup Android SDK
      uses: android-actions/setup-android@v2
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build Android
      run: dotnet build -f net8.0-android -c Release
    
    - name: Create APK
      run: dotnet publish -f net8.0-android -c Release -o ./bin/Release/net8.0-android/publish/
    
    - name: Upload Android artifacts
      uses: actions/upload-artifact@v3
      with:
        name: android-build
        path: ./bin/Release/net8.0-android/publish/

  deploy-ios:
    needs: build-ios
    runs-on: macos-latest
    if: startsWith(github.ref, 'refs/tags/v')
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Download iOS artifacts
      uses: actions/download-artifact@v3
      with:
        name: ios-build
        path: ./ios-build/
    
    - name: Deploy to App Store Connect
      env:
        APP_STORE_CONNECT_API_KEY: ${{ secrets.APP_STORE_CONNECT_API_KEY }}
        APP_STORE_CONNECT_ISSUER_ID: ${{ secrets.APP_STORE_CONNECT_ISSUER_ID }}
        APP_STORE_CONNECT_KEY_ID: ${{ secrets.APP_STORE_CONNECT_KEY_ID }}
      run: |
        # Deploy to App Store Connect using fastlane or similar tool
        echo "Deploying to App Store Connect..."

  deploy-android:
    needs: build-android
    runs-on: ubuntu-latest
    if: startsWith(github.ref, 'refs/tags/v')
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Download Android artifacts
      uses: actions/download-artifact@v3
      with:
        name: android-build
        path: ./android-build/
    
    - name: Deploy to Google Play
      env:
        GOOGLE_PLAY_SERVICE_ACCOUNT_JSON: ${{ secrets.GOOGLE_PLAY_SERVICE_ACCOUNT_JSON }}
      run: |
        # Deploy to Google Play using fastlane or similar tool
        echo "Deploying to Google Play..."
```

### **5. Release Management Service**

```csharp
// Services/IReleaseManagementService.cs
public interface IReleaseManagementService
{
    Task<string> GetCurrentVersionAsync();
    Task<string> GetBuildNumberAsync();
    Task<bool> IsUpdateAvailableAsync();
    Task<ReleaseInfo> GetLatestReleaseAsync();
    Task DownloadUpdateAsync();
    Task ShowUpdateDialogAsync();
}

// Services/ReleaseManagementService.cs
public class ReleaseManagementService : IReleaseManagementService
{
    private readonly HttpClient _httpClient;
    private readonly IAppInfo _appInfo;

    public ReleaseManagementService(HttpClient httpClient, IAppInfo appInfo)
    {
        _httpClient = httpClient;
        _appInfo = appInfo;
    }

    public async Task<string> GetCurrentVersionAsync()
    {
        return _appInfo.VersionString;
    }

    public async Task<string> GetBuildNumberAsync()
    {
        return _appInfo.BuildString;
    }

    public async Task<bool> IsUpdateAvailableAsync()
    {
        try
        {
            var latestRelease = await GetLatestReleaseAsync();
            var currentVersion = new Version(await GetCurrentVersionAsync());
            var latestVersion = new Version(latestRelease.Version);

            return latestVersion > currentVersion;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error checking for updates: {ex.Message}");
            return false;
        }
    }

    public async Task<ReleaseInfo> GetLatestReleaseAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/releases/latest");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ReleaseInfo>(content);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting latest release: {ex.Message}");
        }

        return null;
    }

    public async Task DownloadUpdateAsync()
    {
        try
        {
            var latestRelease = await GetLatestReleaseAsync();
            if (latestRelease != null)
            {
                // En una implementación real, descargarías el archivo de actualización
                // y lo instalarías usando el sistema de actualizaciones de la plataforma
                await Application.Current.MainPage.DisplayAlert(
                    "Actualización Disponible",
                    $"Nueva versión {latestRelease.Version} disponible. Por favor, actualiza desde la tienda de aplicaciones.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    public async Task ShowUpdateDialogAsync()
    {
        var isUpdateAvailable = await IsUpdateAvailableAsync();
        if (isUpdateAvailable)
        {
            var latestRelease = await GetLatestReleaseAsync();
            
            var result = await Application.Current.MainPage.DisplayAlert(
                "Actualización Disponible",
                $"Nueva versión {latestRelease.Version} disponible.\n\n{latestRelease.ReleaseNotes}",
                "Actualizar",
                "Más Tarde");

            if (result)
            {
                await DownloadUpdateAsync();
            }
        }
    }
}

// Models/ReleaseInfo.cs
public class ReleaseInfo
{
    public string Version { get; set; }
    public string BuildNumber { get; set; }
    public string ReleaseNotes { get; set; }
    public DateTime ReleaseDate { get; set; }
    public bool IsMandatory { get; set; }
    public string DownloadUrl { get; set; }
}
```

### **6. App Store Optimization (ASO)**

```csharp
// Services/IASOService.cs
public interface IASOService
{
    Task<ASOData> GetASODataAsync();
    Task TrackAppStoreMetricsAsync();
    Task OptimizeKeywordsAsync();
}

// Services/ASOService.cs
public class ASOService : IASOService
{
    private readonly HttpClient _httpClient;

    public ASOService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ASOData> GetASODataAsync()
    {
        return new ASOData
        {
            AppName = "MussikOn - Conecta Músicos",
            ShortDescription = "Encuentra músicos profesionales para tu evento o aplica a oportunidades musicales",
            LongDescription = GetLongDescription(),
            Keywords = GetKeywords(),
            Category = "Music",
            Subcategory = "Entertainment",
            Screenshots = GetScreenshots(),
            AppIcon = "app_icon_1024.png"
        };
    }

    private string GetLongDescription()
    {
        return @"
🎵 MussikOn - La plataforma que conecta músicos con organizadores de eventos

¿Eres músico buscando oportunidades? ¿O organizas eventos y necesitas talento musical? MussikOn es la solución perfecta para ambos.

🎯 PARA MÚSICOS:
• Crea tu perfil profesional con portafolio
• Encuentra eventos que coincidan con tu estilo
• Aplica a oportunidades musicales
• Gestiona tu agenda y disponibilidad
• Recibe notificaciones de nuevas oportunidades

🎪 PARA ORGANIZADORES:
• Busca músicos por género, ubicación y presupuesto
• Filtra por experiencia y calificaciones
• Chatea directamente con los músicos
• Gestiona contratos y pagos
• Califica y reseña el servicio

✨ CARACTERÍSTICAS:
• Búsqueda inteligente con filtros avanzados
• Chat en tiempo real
• Sistema de calificaciones y reseñas
• Geolocalización para músicos cercanos
• Notificaciones push
• Funciona offline
• Interfaz intuitiva y fácil de usar

🎼 GÉNEROS MUSICALES:
Rock, Pop, Jazz, Blues, Clásica, Electrónica, Folk, Reggae, Salsa, Banda, Mariachi y muchos más.

📱 COMPATIBILIDAD:
• iPhone y iPad (iOS 14.0+)
• Android (API 21+)
• Sincronización entre dispositivos

¡Únete a la comunidad musical más grande y conecta con oportunidades increíbles!

Descarga MussikOn ahora y comienza tu viaje musical.
        ";
    }

    private string GetKeywords()
    {
        return "músicos, eventos, música, contratar músicos, oportunidades musicales, bandas, solistas, DJ, eventos corporativos, bodas, fiestas, conciertos, booking musical, agencia musical";
    }

    private List<string> GetScreenshots()
    {
        return new List<string>
        {
            "screenshot_1.png",
            "screenshot_2.png",
            "screenshot_3.png",
            "screenshot_4.png",
            "screenshot_5.png"
        };
    }

    public async Task TrackAppStoreMetricsAsync()
    {
        try
        {
            var metrics = new AppStoreMetrics
            {
                Downloads = await GetDownloadCountAsync(),
                Ratings = await GetAverageRatingAsync(),
                Reviews = await GetReviewCountAsync(),
                Rank = await GetAppStoreRankAsync()
            };

            await _httpClient.PostAsync("/api/aso/metrics", 
                new StringContent(JsonSerializer.Serialize(metrics), 
                Encoding.UTF8, "application/json"));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error tracking ASO metrics: {ex.Message}");
        }
    }

    public async Task OptimizeKeywordsAsync()
    {
        // Implementar optimización de keywords basada en datos
    }

    private async Task<int> GetDownloadCountAsync()
    {
        // Implementar obtención de métricas de descarga
        return 0;
    }

    private async Task<double> GetAverageRatingAsync()
    {
        // Implementar obtención de calificación promedio
        return 0.0;
    }

    private async Task<int> GetReviewCountAsync()
    {
        // Implementar obtención de número de reseñas
        return 0;
    }

    private async Task<int> GetAppStoreRankAsync()
    {
        // Implementar obtención de ranking en App Store
        return 0;
    }
}

// Models/ASOData.cs
public class ASOData
{
    public string AppName { get; set; }
    public string ShortDescription { get; set; }
    public string LongDescription { get; set; }
    public string Keywords { get; set; }
    public string Category { get; set; }
    public string Subcategory { get; set; }
    public List<string> Screenshots { get; set; }
    public string AppIcon { get; set; }
}

// Models/AppStoreMetrics.cs
public class AppStoreMetrics
{
    public int Downloads { get; set; }
    public double Ratings { get; set; }
    public int Reviews { get; set; }
    public int Rank { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
```

### **7. Version Management**

```csharp
// Services/IVersionService.cs
public interface IVersionService
{
    string GetVersion();
    string GetBuildNumber();
    string GetFullVersion();
    bool IsDebugBuild();
    bool IsReleaseBuild();
}

// Services/VersionService.cs
public class VersionService : IVersionService
{
    private readonly IAppInfo _appInfo;

    public VersionService(IAppInfo appInfo)
    {
        _appInfo = appInfo;
    }

    public string GetVersion()
    {
        return _appInfo.VersionString;
    }

    public string GetBuildNumber()
    {
        return _appInfo.BuildString;
    }

    public string GetFullVersion()
    {
        return $"{GetVersion()} ({GetBuildNumber()})";
    }

    public bool IsDebugBuild()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }

    public bool IsReleaseBuild()
    {
        return !IsDebugBuild();
    }
}
```

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Configurar Build**
1. Configurar scripts de build para iOS y Android
2. Implementar code signing
3. Crear configuraciones de release

### **Ejercicio 2: CI/CD Pipeline**
1. Configurar GitHub Actions
2. Implementar deployment automático
3. Configurar testing en pipeline

### **Ejercicio 3: App Store Optimization**
1. Crear metadata para App Store
2. Optimizar descripciones y keywords
3. Preparar screenshots y assets

## 📝 **Resumen de la Clase**

En esta clase hemos aprendido:

✅ **Deployment** para iOS y Android
✅ **Code signing** y certificados
✅ **CI/CD** con GitHub Actions
✅ **Release management** y versioning
✅ **App Store Optimization** (ASO)
✅ **Distribución** en stores

## 🚀 **Próxima Clase**

En la siguiente clase implementaremos:
- **Monitoreo** y analytics
- **Crash reporting** y error tracking
- **Performance monitoring**
- **User feedback** y support

---

**💡 Tip del Día**: El deployment y distribución son procesos críticos. Automatiza todo lo posible y mantén un proceso de release consistente y confiable.
