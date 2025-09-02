# 🚀 **Clase 6: Testing en Aplicaciones Móviles**

## 🎯 **Objetivos de la Clase**
- Implementar unit testing para ViewModels
- Configurar integration testing
- Crear UI testing automatizado
- Implementar performance testing
- Configurar CI/CD para testing móvil

## 📚 **Contenido Teórico**

### **1. Tipos de Testing en Móviles**

**Unit Testing:**
- Testing de ViewModels y lógica de negocio
- Mocking de dependencias
- Testing de servicios y repositorios

**Integration Testing:**
- Testing de integración con APIs
- Testing de base de datos local
- Testing de servicios externos

**UI Testing:**
- Testing automatizado de interfaz
- Testing de navegación
- Testing de interacciones de usuario

**Performance Testing:**
- Testing de rendimiento
- Testing de memoria
- Testing de carga

### **2. Herramientas de Testing**

- **xUnit**: Framework de unit testing
- **Moq**: Framework de mocking
- **MAUI Test Framework**: Testing de UI
- **Appium**: Testing automatizado multiplataforma
- **Xamarin.UITest**: Testing de UI específico

## 💻 **Implementación Práctica**

### **1. Configuración de Testing**

```csharp
// MussikOn.Mobile.Tests/MusicianViewModelTests.cs
using xUnit;
using Moq;
using MussikOn.Mobile.ViewModels;
using MussikOn.Mobile.Services;
using MussikOn.Mobile.Models;

namespace MussikOn.Mobile.Tests
{
    public class MusicianViewModelTests
    {
        private Mock<IMusicianRepository> _mockMusicianRepository;
        private Mock<INavigationService> _mockNavigationService;
        private Mock<IConnectivityService> _mockConnectivityService;
        private MusicianViewModel _viewModel;

        public MusicianViewModelTests()
        {
            _mockMusicianRepository = new Mock<IMusicianRepository>();
            _mockNavigationService = new Mock<INavigationService>();
            _mockConnectivityService = new Mock<IConnectivityService>();
            
            _viewModel = new MusicianViewModel(
                _mockMusicianRepository.Object,
                _mockNavigationService.Object,
                _mockConnectivityService.Object);
        }

        [Fact]
        public async Task LoadMusicians_WhenCalled_ShouldLoadMusicians()
        {
            // Arrange
            var expectedMusicians = new List<Musician>
            {
                new Musician { Id = 1, Name = "John Doe", Genres = "Rock, Pop" },
                new Musician { Id = 2, Name = "Jane Smith", Genres = "Jazz, Blues" }
            };

            _mockMusicianRepository
                .Setup(x => x.GetMusiciansAsync(It.IsAny<MusicianSearchCriteria>()))
                .ReturnsAsync(expectedMusicians);

            _mockConnectivityService
                .Setup(x => x.IsConnected)
                .Returns(true);

            // Act
            await _viewModel.LoadMusiciansAsync();

            // Assert
            Assert.Equal(2, _viewModel.Musicians.Count);
            Assert.Equal("John Doe", _viewModel.Musicians[0].Name);
            Assert.Equal("Jane Smith", _viewModel.Musicians[1].Name);
        }

        [Fact]
        public async Task SearchMusicians_WhenOffline_ShouldShowOfflineMessage()
        {
            // Arrange
            _mockConnectivityService
                .Setup(x => x.IsConnected)
                .Returns(false);

            _mockMusicianRepository
                .Setup(x => x.GetMusiciansAsync(It.IsAny<MusicianSearchCriteria>()))
                .ThrowsAsync(new NoInternetException("Sin conexión"));

            // Act & Assert
            await Assert.ThrowsAsync<NoInternetException>(() => _viewModel.SearchMusiciansAsync());
            Assert.True(_viewModel.IsOfflineMode);
        }

        [Fact]
        public void SearchCriteria_WhenSet_ShouldUpdateMusicians()
        {
            // Arrange
            var searchCriteria = new MusicianSearchCriteria
            {
                Genre = "Rock",
                Location = "Madrid"
            };

            // Act
            _viewModel.SearchCriteria = searchCriteria;

            // Assert
            Assert.Equal("Rock", _viewModel.SearchCriteria.Genre);
            Assert.Equal("Madrid", _viewModel.SearchCriteria.Location);
        }

        [Fact]
        public async Task SelectMusician_WhenCalled_ShouldNavigateToDetail()
        {
            // Arrange
            var musician = new Musician { Id = 1, Name = "John Doe" };
            _viewModel.Musicians.Add(musician);

            // Act
            await _viewModel.SelectMusicianAsync(musician);

            // Assert
            _mockNavigationService.Verify(
                x => x.NavigateToAsync("musician-detail", It.IsAny<Dictionary<string, object>>()),
                Times.Once);
        }
    }
}
```

### **2. Integration Testing**

```csharp
// MussikOn.Mobile.Tests.Integration/MusicianRepositoryIntegrationTests.cs
using xUnit;
using MussikOn.Mobile.Repositories;
using MussikOn.Mobile.Database;
using MussikOn.Mobile.Services;

namespace MussikOn.Mobile.Tests.Integration
{
    public class MusicianRepositoryIntegrationTests : IDisposable
    {
        private readonly ILocalDatabase _localDatabase;
        private readonly IMusicianRepository _musicianRepository;
        private readonly HttpClientService _httpClientService;
        private readonly IConnectivity _connectivity;

        public MusicianRepositoryIntegrationTests()
        {
            // Configurar base de datos de prueba
            _localDatabase = new LocalDatabase();
            _httpClientService = new HttpClientService(new HttpClient(), new SecureStorage(), _connectivity);
            _musicianRepository = new MusicianRepository(_httpClientService, _localDatabase, _connectivity);
        }

        [Fact]
        public async Task GetMusicians_WhenOffline_ShouldReturnCachedData()
        {
            // Arrange
            var testMusician = new Musician
            {
                Id = 1,
                Name = "Test Musician",
                Genres = "Rock",
                Location = "Madrid"
            };

            await _localDatabase.SaveMusicianAsync(testMusician);

            // Act
            var result = await _musicianRepository.GetMusiciansAsync(new MusicianSearchCriteria());

            // Assert
            Assert.Single(result);
            Assert.Equal("Test Musician", result.First().Name);
        }

        [Fact]
        public async Task SaveMusician_WhenOffline_ShouldSaveLocally()
        {
            // Arrange
            var musician = new Musician
            {
                Name = "New Musician",
                Genres = "Jazz",
                Location = "Barcelona"
            };

            // Act
            var result = await _musicianRepository.SaveMusicianAsync(musician);

            // Assert
            Assert.True(result);
            
            var savedMusicians = await _localDatabase.GetPendingMusiciansAsync();
            Assert.Single(savedMusicians);
            Assert.Equal("New Musician", savedMusicians.First().Name);
        }

        [Fact]
        public async Task SyncPendingData_WhenOnline_ShouldSyncToServer()
        {
            // Arrange
            var pendingMusician = new Musician
            {
                Name = "Pending Musician",
                Genres = "Pop"
            };

            await _localDatabase.SavePendingMusicianAsync(pendingMusician);

            // Act
            await _musicianRepository.SyncPendingDataAsync();

            // Assert
            var pendingMusicians = await _localDatabase.GetPendingMusiciansAsync();
            Assert.Empty(pendingMusicians);
        }

        public void Dispose()
        {
            // Limpiar base de datos de prueba
            _localDatabase?.Dispose();
        }
    }
}
```

### **3. UI Testing con MAUI Test Framework**

```csharp
// MussikOn.Mobile.Tests.UI/LoginPageUITests.cs
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using xUnit;

namespace MussikOn.Mobile.Tests.UI
{
    public class LoginPageUITests
    {
        [Fact]
        public void LoginPage_ShouldHaveCorrectElements()
        {
            // Arrange
            var page = new LoginPage();

            // Act & Assert
            Assert.NotNull(page.FindByName("EmailEntry"));
            Assert.NotNull(page.FindByName("PasswordEntry"));
            Assert.NotNull(page.FindByName("LoginButton"));
            Assert.NotNull(page.FindByName("RegisterButton"));
        }

        [Fact]
        public void LoginButton_WhenEmailEmpty_ShouldBeDisabled()
        {
            // Arrange
            var page = new LoginPage();
            var viewModel = new LoginViewModel(new Mock<IAuthService>().Object);
            page.BindingContext = viewModel;

            // Act
            viewModel.Email = "";
            viewModel.Password = "password";

            // Assert
            var loginButton = page.FindByName<Button>("LoginButton");
            Assert.False(loginButton.IsEnabled);
        }

        [Fact]
        public void LoginButton_WhenFieldsFilled_ShouldBeEnabled()
        {
            // Arrange
            var page = new LoginPage();
            var viewModel = new LoginViewModel(new Mock<IAuthService>().Object);
            page.BindingContext = viewModel;

            // Act
            viewModel.Email = "test@example.com";
            viewModel.Password = "password";

            // Assert
            var loginButton = page.FindByName<Button>("LoginButton");
            Assert.True(loginButton.IsEnabled);
        }
    }
}
```

### **4. Performance Testing**

```csharp
// MussikOn.Mobile.Tests.Performance/PerformanceTests.cs
using xUnit;
using System.Diagnostics;
using MussikOn.Mobile.Services;
using MussikOn.Mobile.ViewModels;

namespace MussikOn.Mobile.Tests.Performance
{
    public class PerformanceTests
    {
        [Fact]
        public async Task LoadMusicians_ShouldCompleteWithinTimeLimit()
        {
            // Arrange
            var stopwatch = Stopwatch.StartNew();
            var viewModel = CreateMusicianViewModel();

            // Act
            await viewModel.LoadMusiciansAsync();
            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds < 2000, 
                $"LoadMusicians took {stopwatch.ElapsedMilliseconds}ms, expected < 2000ms");
        }

        [Fact]
        public async Task ImageLoading_ShouldNotExceedMemoryLimit()
        {
            // Arrange
            var imageCacheService = new ImageCacheService(new HttpClient());
            var initialMemory = GC.GetTotalMemory(false);

            // Act
            for (int i = 0; i < 100; i++)
            {
                await imageCacheService.GetImageAsync($"https://example.com/image{i}.jpg");
            }

            var finalMemory = GC.GetTotalMemory(true);
            var memoryIncrease = finalMemory - initialMemory;

            // Assert
            Assert.True(memoryIncrease < 50 * 1024 * 1024, // 50MB limit
                $"Memory increase: {memoryIncrease / 1024 / 1024}MB, expected < 50MB");
        }

        [Fact]
        public async Task DatabaseOperations_ShouldBeEfficient()
        {
            // Arrange
            var localDatabase = new LocalDatabase();
            var stopwatch = Stopwatch.StartNew();

            // Act
            for (int i = 0; i < 1000; i++)
            {
                var musician = new Musician
                {
                    Id = i,
                    Name = $"Musician {i}",
                    Genres = "Rock"
                };
                await localDatabase.SaveMusicianAsync(musician);
            }
            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds < 5000,
                $"Database operations took {stopwatch.ElapsedMilliseconds}ms, expected < 5000ms");
        }

        private MusicianViewModel CreateMusicianViewModel()
        {
            var mockRepository = new Mock<IMusicianRepository>();
            var mockNavigation = new Mock<INavigationService>();
            var mockConnectivity = new Mock<IConnectivityService>();

            mockRepository
                .Setup(x => x.GetMusiciansAsync(It.IsAny<MusicianSearchCriteria>()))
                .ReturnsAsync(new List<Musician>());

            mockConnectivity
                .Setup(x => x.IsConnected)
                .Returns(true);

            return new MusicianViewModel(
                mockRepository.Object,
                mockNavigation.Object,
                mockConnectivity.Object);
        }
    }
}
```

### **5. Mock Services para Testing**

```csharp
// MussikOn.Mobile.Tests/Mocks/MockServices.cs
using Moq;
using MussikOn.Mobile.Services;
using MussikOn.Mobile.Models;

namespace MussikOn.Mobile.Tests.Mocks
{
    public static class MockServices
    {
        public static Mock<IMusicianRepository> CreateMusicianRepositoryMock()
        {
            var mock = new Mock<IMusicianRepository>();
            
            mock.Setup(x => x.GetMusiciansAsync(It.IsAny<MusicianSearchCriteria>()))
                .ReturnsAsync(new List<Musician>());
            
            mock.Setup(x => x.GetMusicianByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => new Musician { Id = id, Name = $"Musician {id}" });
            
            mock.Setup(x => x.SaveMusicianAsync(It.IsAny<Musician>()))
                .ReturnsAsync(true);
            
            return mock;
        }

        public static Mock<IAuthService> CreateAuthServiceMock()
        {
            var mock = new Mock<IAuthService>();
            
            mock.Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new AuthResult { Success = true, Token = "test_token" });
            
            mock.Setup(x => x.IsAuthenticatedAsync())
                .ReturnsAsync(true);
            
            mock.Setup(x => x.GetCurrentUserAsync())
                .ReturnsAsync(new User { Id = 1, Name = "Test User", Email = "test@example.com" });
            
            return mock;
        }

        public static Mock<INavigationService> CreateNavigationServiceMock()
        {
            var mock = new Mock<INavigationService>();
            
            mock.Setup(x => x.NavigateToAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
                .Returns(Task.CompletedTask);
            
            mock.Setup(x => x.NavigateBackAsync())
                .Returns(Task.CompletedTask);
            
            return mock;
        }

        public static Mock<IConnectivityService> CreateConnectivityServiceMock()
        {
            var mock = new Mock<IConnectivityService>();
            
            mock.Setup(x => x.IsConnected)
                .Returns(true);
            
            mock.Setup(x => x.NetworkAccess)
                .Returns(NetworkAccess.Internet);
            
            return mock;
        }
    }
}
```

### **6. Test Base Class**

```csharp
// MussikOn.Mobile.Tests/TestBase.cs
using Microsoft.Extensions.DependencyInjection;
using MussikOn.Mobile.Services;
using MussikOn.Mobile.ViewModels;

namespace MussikOn.Mobile.Tests
{
    public abstract class TestBase : IDisposable
    {
        protected ServiceProvider ServiceProvider { get; private set; }
        protected Mock<IMusicianRepository> MockMusicianRepository { get; private set; }
        protected Mock<IAuthService> MockAuthService { get; private set; }
        protected Mock<INavigationService> MockNavigationService { get; private set; }
        protected Mock<IConnectivityService> MockConnectivityService { get; private set; }

        protected TestBase()
        {
            SetupMocks();
            SetupServices();
        }

        private void SetupMocks()
        {
            MockMusicianRepository = MockServices.CreateMusicianRepositoryMock();
            MockAuthService = MockServices.CreateAuthServiceMock();
            MockNavigationService = MockServices.CreateNavigationServiceMock();
            MockConnectivityService = MockServices.CreateConnectivityServiceMock();
        }

        private void SetupServices()
        {
            var services = new ServiceCollection();
            
            services.AddSingleton(MockMusicianRepository.Object);
            services.AddSingleton(MockAuthService.Object);
            services.AddSingleton(MockNavigationService.Object);
            services.AddSingleton(MockConnectivityService.Object);
            
            ServiceProvider = services.BuildServiceProvider();
        }

        protected T GetService<T>() where T : class
        {
            return ServiceProvider.GetService<T>();
        }

        protected MusicianViewModel CreateMusicianViewModel()
        {
            return new MusicianViewModel(
                MockMusicianRepository.Object,
                MockNavigationService.Object,
                MockConnectivityService.Object);
        }

        protected LoginViewModel CreateLoginViewModel()
        {
            return new LoginViewModel(MockAuthService.Object);
        }

        public void Dispose()
        {
            ServiceProvider?.Dispose();
        }
    }
}
```

### **7. CI/CD Configuration para Testing**

```yaml
# .github/workflows/mobile-tests.yml
name: Mobile Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  unit-tests:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Run unit tests
      run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
    
    - name: Upload coverage reports
      uses: codecov/codecov-action@v3
      with:
        file: ./coverage.xml

  integration-tests:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Run integration tests
      run: dotnet test --no-build --filter Category=Integration

  ui-tests:
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
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Run UI tests
      run: dotnet test --no-build --filter Category=UI
```

### **8. Test Configuration**

```csharp
// MussikOn.Mobile.Tests/TestConfiguration.cs
using Microsoft.Extensions.DependencyInjection;
using MussikOn.Mobile.Services;
using MussikOn.Mobile.Database;

namespace MussikOn.Mobile.Tests
{
    public static class TestConfiguration
    {
        public static IServiceCollection AddTestServices(this IServiceCollection services)
        {
            // Configurar servicios de prueba
            services.AddSingleton<ILocalDatabase, TestLocalDatabase>();
            services.AddSingleton<IConnectivityService, TestConnectivityService>();
            services.AddSingleton<INotificationService, TestNotificationService>();
            
            return services;
        }
    }

    public class TestLocalDatabase : ILocalDatabase
    {
        private readonly List<Musician> _musicians = new();
        private readonly List<PendingMusician> _pendingMusicians = new();

        public Task<IEnumerable<Musician>> GetMusiciansAsync(MusicianSearchCriteria criteria)
        {
            return Task.FromResult(_musicians.AsEnumerable());
        }

        public Task<Musician> GetMusicianByIdAsync(int id)
        {
            return Task.FromResult(_musicians.FirstOrDefault(m => m.Id == id));
        }

        public Task SaveMusicianAsync(Musician musician)
        {
            _musicians.Add(musician);
            return Task.CompletedTask;
        }

        public Task SavePendingMusicianAsync(Musician musician)
        {
            _pendingMusicians.Add(new PendingMusician { MusicianData = System.Text.Json.JsonSerializer.Serialize(musician) });
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Musician>> GetPendingMusiciansAsync()
        {
            var musicians = _pendingMusicians.Select(p => 
                System.Text.Json.JsonSerializer.Deserialize<Musician>(p.MusicianData));
            return Task.FromResult(musicians);
        }

        public Task ClearPendingMusiciansAsync()
        {
            _pendingMusicians.Clear();
            return Task.CompletedTask;
        }
    }
}
```

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Unit Testing**
1. Crear tests para ViewModels
2. Implementar mocking de servicios
3. Configurar assertions y validaciones

### **Ejercicio 2: Integration Testing**
1. Crear tests de integración con base de datos
2. Testear servicios de API
3. Validar sincronización de datos

### **Ejercicio 3: UI Testing**
1. Crear tests de interfaz de usuario
2. Testear navegación entre páginas
3. Validar interacciones de usuario

## 📝 **Resumen de la Clase**

En esta clase hemos aprendido:

✅ **Unit testing** para ViewModels y servicios
✅ **Integration testing** con base de datos y APIs
✅ **UI testing** automatizado
✅ **Performance testing** y métricas
✅ **Mocking** y test doubles
✅ **CI/CD** para testing móvil

## 🚀 **Próxima Clase**

En la siguiente clase implementaremos:
- **Deployment** y distribución
- **App Store** y Google Play
- **Code signing** y certificados
- **Release management**

---

**💡 Tip del Día**: El testing en aplicaciones móviles es crucial para garantizar calidad. Implementa una estrategia completa de testing que cubra unit, integration, UI y performance testing.
