# 🚀 **Clase 8: Monitoreo, Analytics y Crash Reporting**

## 🎯 **Objetivos de la Clase**
- Implementar analytics y tracking de usuarios
- Configurar crash reporting y error tracking
- Implementar performance monitoring
- Configurar user feedback y support
- Integrar servicios de monitoreo

## 📚 **Contenido Teórico**

### **1. Analytics y User Tracking**

**Métricas importantes:**
- **User Engagement**: Tiempo en app, sesiones, retención
- **Feature Usage**: Uso de funcionalidades específicas
- **Conversion Funnels**: Flujos de conversión
- **User Behavior**: Patrones de navegación

### **2. Crash Reporting**

**Información crítica:**
- **Stack Traces**: Trazas de error detalladas
- **Device Information**: Modelo, OS, memoria
- **User Context**: Estado de la app al momento del crash
- **Breadcrumbs**: Secuencia de eventos antes del crash

### **3. Performance Monitoring**

**Métricas de rendimiento:**
- **App Launch Time**: Tiempo de inicio
- **Screen Load Time**: Tiempo de carga de pantallas
- **API Response Time**: Tiempo de respuesta de APIs
- **Memory Usage**: Uso de memoria
- **Battery Usage**: Consumo de batería

## 💻 **Implementación Práctica**

### **1. Analytics Service**

```csharp
// Services/IAnalyticsService.cs
public interface IAnalyticsService
{
    Task InitializeAsync();
    Task TrackEventAsync(string eventName, Dictionary<string, object> parameters = null);
    Task TrackScreenViewAsync(string screenName);
    Task TrackUserPropertyAsync(string propertyName, object value);
    Task SetUserIdAsync(string userId);
    Task TrackExceptionAsync(Exception exception, Dictionary<string, object> context = null);
    Task TrackPerformanceAsync(string operationName, long duration);
}

// Services/AnalyticsService.cs
public class AnalyticsService : IAnalyticsService
{
    private readonly ILogger<AnalyticsService> _logger;
    private readonly IUserService _userService;
    private string _userId;
    private readonly Dictionary<string, object> _userProperties;

    public AnalyticsService(ILogger<AnalyticsService> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
        _userProperties = new Dictionary<string, object>();
    }

    public async Task InitializeAsync()
    {
        try
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            if (currentUser != null)
            {
                await SetUserIdAsync(currentUser.Id.ToString());
            }

            await TrackEventAsync("app_initialized", new Dictionary<string, object>
            {
                ["platform"] = DeviceInfo.Platform.ToString(),
                ["version"] = AppInfo.VersionString,
                ["build"] = AppInfo.BuildString
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing analytics");
        }
    }

    public async Task TrackEventAsync(string eventName, Dictionary<string, object> parameters = null)
    {
        try
        {
            var eventData = new Dictionary<string, object>
            {
                ["event_name"] = eventName,
                ["timestamp"] = DateTime.UtcNow,
                ["user_id"] = _userId,
                ["platform"] = DeviceInfo.Platform.ToString(),
                ["app_version"] = AppInfo.VersionString
            };

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    eventData[param.Key] = param.Value;
                }
            }

            // En una implementación real, enviarías esto a un servicio de analytics
            // como Firebase Analytics, App Center, o Mixpanel
            _logger.LogInformation("Analytics Event: {EventName} with parameters: {@Parameters}", 
                eventName, parameters);

            // Simular envío a servicio de analytics
            await SendToAnalyticsServiceAsync(eventData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking event: {EventName}", eventName);
        }
    }

    public async Task TrackScreenViewAsync(string screenName)
    {
        await TrackEventAsync("screen_view", new Dictionary<string, object>
        {
            ["screen_name"] = screenName
        });
    }

    public async Task TrackUserPropertyAsync(string propertyName, object value)
    {
        try
        {
            _userProperties[propertyName] = value;
            
            // Enviar propiedad de usuario al servicio de analytics
            await SendUserPropertyToAnalyticsServiceAsync(propertyName, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking user property: {PropertyName}", propertyName);
        }
    }

    public async Task SetUserIdAsync(string userId)
    {
        _userId = userId;
        await SendUserIdToAnalyticsServiceAsync(userId);
    }

    public async Task TrackExceptionAsync(Exception exception, Dictionary<string, object> context = null)
    {
        try
        {
            var exceptionData = new Dictionary<string, object>
            {
                ["exception_type"] = exception.GetType().Name,
                ["exception_message"] = exception.Message,
                ["stack_trace"] = exception.StackTrace,
                ["timestamp"] = DateTime.UtcNow,
                ["user_id"] = _userId,
                ["platform"] = DeviceInfo.Platform.ToString(),
                ["app_version"] = AppInfo.VersionString
            };

            if (context != null)
            {
                foreach (var item in context)
                {
                    exceptionData[item.Key] = item.Value;
                }
            }

            await SendExceptionToAnalyticsServiceAsync(exceptionData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking exception");
        }
    }

    public async Task TrackPerformanceAsync(string operationName, long duration)
    {
        await TrackEventAsync("performance_metric", new Dictionary<string, object>
        {
            ["operation_name"] = operationName,
            ["duration_ms"] = duration
        });
    }

    private async Task SendToAnalyticsServiceAsync(Dictionary<string, object> eventData)
    {
        // Implementar envío a servicio de analytics real
        await Task.Delay(100); // Simular envío
    }

    private async Task SendUserPropertyToAnalyticsServiceAsync(string propertyName, object value)
    {
        // Implementar envío de propiedades de usuario
        await Task.Delay(50);
    }

    private async Task SendUserIdToAnalyticsServiceAsync(string userId)
    {
        // Implementar envío de ID de usuario
        await Task.Delay(50);
    }

    private async Task SendExceptionToAnalyticsServiceAsync(Dictionary<string, object> exceptionData)
    {
        // Implementar envío de excepciones
        await Task.Delay(100);
    }
}
```

### **2. Crash Reporting Service**

```csharp
// Services/ICrashReportingService.cs
public interface ICrashReportingService
{
    Task InitializeAsync();
    Task ReportCrashAsync(Exception exception, Dictionary<string, object> context = null);
    Task ReportNonFatalErrorAsync(Exception exception, Dictionary<string, object> context = null);
    Task SetUserContextAsync(string userId, Dictionary<string, object> userInfo = null);
    Task AddBreadcrumbAsync(string message, Dictionary<string, object> data = null);
    Task SetTagAsync(string key, string value);
    Task SetExtraAsync(string key, object value);
}

// Services/CrashReportingService.cs
public class CrashReportingService : ICrashReportingService
{
    private readonly ILogger<CrashReportingService> _logger;
    private readonly IUserService _userService;
    private readonly List<Breadcrumb> _breadcrumbs;
    private readonly Dictionary<string, string> _tags;
    private readonly Dictionary<string, object> _extras;
    private string _userId;

    public CrashReportingService(ILogger<CrashReportingService> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
        _breadcrumbs = new List<Breadcrumb>();
        _tags = new Dictionary<string, string>();
        _extras = new Dictionary<string, object>();
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Configurar manejo global de excepciones
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            
            var currentUser = await _userService.GetCurrentUserAsync();
            if (currentUser != null)
            {
                await SetUserContextAsync(currentUser.Id.ToString(), new Dictionary<string, object>
                {
                    ["email"] = currentUser.Email,
                    ["name"] = currentUser.Name,
                    ["user_type"] = currentUser.Type.ToString()
                });
            }

            await AddBreadcrumbAsync("Crash reporting initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing crash reporting");
        }
    }

    public async Task ReportCrashAsync(Exception exception, Dictionary<string, object> context = null)
    {
        try
        {
            var crashData = new CrashReport
            {
                Exception = exception,
                Context = context ?? new Dictionary<string, object>(),
                Breadcrumbs = _breadcrumbs.ToList(),
                Tags = _tags.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                Extras = _extras.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                UserId = _userId,
                Timestamp = DateTime.UtcNow,
                DeviceInfo = GetDeviceInfo(),
                AppInfo = GetAppInfo()
            };

            await SendCrashReportAsync(crashData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reporting crash");
        }
    }

    public async Task ReportNonFatalErrorAsync(Exception exception, Dictionary<string, object> context = null)
    {
        try
        {
            var errorData = new ErrorReport
            {
                Exception = exception,
                Context = context ?? new Dictionary<string, object>(),
                Breadcrumbs = _breadcrumbs.TakeLast(10).ToList(), // Solo últimos 10 breadcrumbs
                UserId = _userId,
                Timestamp = DateTime.UtcNow,
                DeviceInfo = GetDeviceInfo()
            };

            await SendErrorReportAsync(errorData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reporting non-fatal error");
        }
    }

    public async Task SetUserContextAsync(string userId, Dictionary<string, object> userInfo = null)
    {
        _userId = userId;
        
        if (userInfo != null)
        {
            foreach (var item in userInfo)
            {
                _extras[$"user_{item.Key}"] = item.Value;
            }
        }

        await AddBreadcrumbAsync("User context set", new Dictionary<string, object>
        {
            ["user_id"] = userId
        });
    }

    public async Task AddBreadcrumbAsync(string message, Dictionary<string, object> data = null)
    {
        var breadcrumb = new Breadcrumb
        {
            Message = message,
            Data = data ?? new Dictionary<string, object>(),
            Timestamp = DateTime.UtcNow,
            Level = "info"
        };

        _breadcrumbs.Add(breadcrumb);

        // Mantener solo los últimos 50 breadcrumbs
        if (_breadcrumbs.Count > 50)
        {
            _breadcrumbs.RemoveAt(0);
        }
    }

    public async Task SetTagAsync(string key, string value)
    {
        _tags[key] = value;
    }

    public async Task SetExtraAsync(string key, object value)
    {
        _extras[key] = value;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            _ = Task.Run(async () =>
            {
                await ReportCrashAsync(exception, new Dictionary<string, object>
                {
                    ["unhandled"] = true
                });
            });
        }
    }

    private DeviceInfo GetDeviceInfo()
    {
        return new DeviceInfo
        {
            Platform = DeviceInfo.Platform.ToString(),
            Model = DeviceInfo.Model,
            Manufacturer = DeviceInfo.Manufacturer,
            Version = DeviceInfo.VersionString,
            Build = DeviceInfo.BuildString,
            Idiom = DeviceInfo.Idiom.ToString()
        };
    }

    private AppInfo GetAppInfo()
    {
        return new AppInfo
        {
            Name = AppInfo.Name,
            Version = AppInfo.VersionString,
            Build = AppInfo.BuildString,
            PackageName = AppInfo.PackageName
        };
    }

    private async Task SendCrashReportAsync(CrashReport crashReport)
    {
        // Implementar envío a servicio de crash reporting real
        // como App Center, Crashlytics, o Sentry
        _logger.LogCritical("CRASH REPORT: {Exception}", crashReport.Exception);
        await Task.Delay(100);
    }

    private async Task SendErrorReportAsync(ErrorReport errorReport)
    {
        // Implementar envío de errores no fatales
        _logger.LogError("ERROR REPORT: {Exception}", errorReport.Exception);
        await Task.Delay(100);
    }
}

// Models/CrashReport.cs
public class CrashReport
{
    public Exception Exception { get; set; }
    public Dictionary<string, object> Context { get; set; }
    public List<Breadcrumb> Breadcrumbs { get; set; }
    public Dictionary<string, string> Tags { get; set; }
    public Dictionary<string, object> Extras { get; set; }
    public string UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public DeviceInfo DeviceInfo { get; set; }
    public AppInfo AppInfo { get; set; }
}

// Models/ErrorReport.cs
public class ErrorReport
{
    public Exception Exception { get; set; }
    public Dictionary<string, object> Context { get; set; }
    public List<Breadcrumb> Breadcrumbs { get; set; }
    public string UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public DeviceInfo DeviceInfo { get; set; }
}

// Models/Breadcrumb.cs
public class Breadcrumb
{
    public string Message { get; set; }
    public Dictionary<string, object> Data { get; set; }
    public DateTime Timestamp { get; set; }
    public string Level { get; set; }
}

// Models/DeviceInfo.cs
public class DeviceInfo
{
    public string Platform { get; set; }
    public string Model { get; set; }
    public string Manufacturer { get; set; }
    public string Version { get; set; }
    public string Build { get; set; }
    public string Idiom { get; set; }
}

// Models/AppInfo.cs
public class AppInfo
{
    public string Name { get; set; }
    public string Version { get; set; }
    public string Build { get; set; }
    public string PackageName { get; set; }
}
```

### **3. Performance Monitoring Service**

```csharp
// Services/IPerformanceMonitoringService.cs
public interface IPerformanceMonitoringService
{
    Task InitializeAsync();
    Task StartTraceAsync(string traceName);
    Task StopTraceAsync(string traceName);
    Task RecordMetricAsync(string metricName, double value, Dictionary<string, string> attributes = null);
    Task RecordCustomEventAsync(string eventName, Dictionary<string, object> attributes = null);
    Task TrackNetworkRequestAsync(string url, string method, int statusCode, long duration);
    Task TrackScreenLoadTimeAsync(string screenName, long loadTime);
    Task TrackAppLaunchTimeAsync(long launchTime);
}

// Services/PerformanceMonitoringService.cs
public class PerformanceMonitoringService : IPerformanceMonitoringService
{
    private readonly ILogger<PerformanceMonitoringService> _logger;
    private readonly Dictionary<string, Stopwatch> _activeTraces;
    private readonly Dictionary<string, List<PerformanceMetric>> _metrics;

    public PerformanceMonitoringService(ILogger<PerformanceMonitoringService> logger)
    {
        _logger = logger;
        _activeTraces = new Dictionary<string, Stopwatch>();
        _metrics = new Dictionary<string, List<PerformanceMetric>>();
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Inicializar monitoreo de rendimiento
            await RecordCustomEventAsync("performance_monitoring_initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing performance monitoring");
        }
    }

    public async Task StartTraceAsync(string traceName)
    {
        try
        {
            if (_activeTraces.ContainsKey(traceName))
            {
                _activeTraces[traceName].Restart();
            }
            else
            {
                _activeTraces[traceName] = Stopwatch.StartNew();
            }

            await RecordCustomEventAsync("trace_started", new Dictionary<string, object>
            {
                ["trace_name"] = traceName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting trace: {TraceName}", traceName);
        }
    }

    public async Task StopTraceAsync(string traceName)
    {
        try
        {
            if (_activeTraces.TryGetValue(traceName, out var stopwatch))
            {
                stopwatch.Stop();
                var duration = stopwatch.ElapsedMilliseconds;

                await RecordMetricAsync("trace_duration", duration, new Dictionary<string, string>
                {
                    ["trace_name"] = traceName
                });

                _activeTraces.Remove(traceName);

                await RecordCustomEventAsync("trace_completed", new Dictionary<string, object>
                {
                    ["trace_name"] = traceName,
                    ["duration_ms"] = duration
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping trace: {TraceName}", traceName);
        }
    }

    public async Task RecordMetricAsync(string metricName, double value, Dictionary<string, string> attributes = null)
    {
        try
        {
            var metric = new PerformanceMetric
            {
                Name = metricName,
                Value = value,
                Attributes = attributes ?? new Dictionary<string, string>(),
                Timestamp = DateTime.UtcNow
            };

            if (!_metrics.ContainsKey(metricName))
            {
                _metrics[metricName] = new List<PerformanceMetric>();
            }

            _metrics[metricName].Add(metric);

            // Mantener solo los últimos 1000 métricas por nombre
            if (_metrics[metricName].Count > 1000)
            {
                _metrics[metricName].RemoveAt(0);
            }

            await SendMetricToServiceAsync(metric);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording metric: {MetricName}", metricName);
        }
    }

    public async Task RecordCustomEventAsync(string eventName, Dictionary<string, object> attributes = null)
    {
        try
        {
            var customEvent = new CustomEvent
            {
                Name = eventName,
                Attributes = attributes ?? new Dictionary<string, object>(),
                Timestamp = DateTime.UtcNow
            };

            await SendCustomEventToServiceAsync(customEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording custom event: {EventName}", eventName);
        }
    }

    public async Task TrackNetworkRequestAsync(string url, string method, int statusCode, long duration)
    {
        await RecordMetricAsync("network_request_duration", duration, new Dictionary<string, string>
        {
            ["url"] = url,
            ["method"] = method,
            ["status_code"] = statusCode.ToString()
        });

        await RecordCustomEventAsync("network_request", new Dictionary<string, object>
        {
            ["url"] = url,
            ["method"] = method,
            ["status_code"] = statusCode,
            ["duration_ms"] = duration
        });
    }

    public async Task TrackScreenLoadTimeAsync(string screenName, long loadTime)
    {
        await RecordMetricAsync("screen_load_time", loadTime, new Dictionary<string, string>
        {
            ["screen_name"] = screenName
        });
    }

    public async Task TrackAppLaunchTimeAsync(long launchTime)
    {
        await RecordMetricAsync("app_launch_time", launchTime);
    }

    private async Task SendMetricToServiceAsync(PerformanceMetric metric)
    {
        // Implementar envío a servicio de monitoreo de rendimiento
        _logger.LogDebug("Performance Metric: {MetricName} = {Value}", metric.Name, metric.Value);
        await Task.Delay(10);
    }

    private async Task SendCustomEventToServiceAsync(CustomEvent customEvent)
    {
        // Implementar envío de eventos personalizados
        _logger.LogDebug("Custom Event: {EventName}", customEvent.Name);
        await Task.Delay(10);
    }
}

// Models/PerformanceMetric.cs
public class PerformanceMetric
{
    public string Name { get; set; }
    public double Value { get; set; }
    public Dictionary<string, string> Attributes { get; set; }
    public DateTime Timestamp { get; set; }
}

// Models/CustomEvent.cs
public class CustomEvent
{
    public string Name { get; set; }
    public Dictionary<string, object> Attributes { get; set; }
    public DateTime Timestamp { get; set; }
}
```

### **4. User Feedback Service**

```csharp
// Services/IUserFeedbackService.cs
public interface IUserFeedbackService
{
    Task ShowFeedbackDialogAsync();
    Task SubmitFeedbackAsync(string feedback, int rating, Dictionary<string, object> metadata = null);
    Task ReportBugAsync(string description, Dictionary<string, object> context = null);
    Task RequestFeatureAsync(string feature, string description);
    Task GetFeedbackHistoryAsync();
}

// Services/UserFeedbackService.cs
public class UserFeedbackService : IUserFeedbackService
{
    private readonly ILogger<UserFeedbackService> _logger;
    private readonly IAnalyticsService _analyticsService;
    private readonly ICrashReportingService _crashReportingService;

    public UserFeedbackService(
        ILogger<UserFeedbackService> logger,
        IAnalyticsService analyticsService,
        ICrashReportingService crashReportingService)
    {
        _logger = logger;
        _analyticsService = analyticsService;
        _crashReportingService = crashReportingService;
    }

    public async Task ShowFeedbackDialogAsync()
    {
        try
        {
            var result = await Application.Current.MainPage.DisplayActionSheet(
                "¿Cómo podemos mejorar?",
                "Cancelar",
                null,
                "Enviar Comentarios",
                "Reportar Error",
                "Solicitar Función");

            switch (result)
            {
                case "Enviar Comentarios":
                    await ShowFeedbackFormAsync();
                    break;
                case "Reportar Error":
                    await ShowBugReportFormAsync();
                    break;
                case "Solicitar Función":
                    await ShowFeatureRequestFormAsync();
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing feedback dialog");
        }
    }

    public async Task SubmitFeedbackAsync(string feedback, int rating, Dictionary<string, object> metadata = null)
    {
        try
        {
            var feedbackData = new UserFeedback
            {
                Feedback = feedback,
                Rating = rating,
                Metadata = metadata ?? new Dictionary<string, object>(),
                Timestamp = DateTime.UtcNow,
                UserId = await GetCurrentUserIdAsync(),
                AppVersion = AppInfo.VersionString,
                Platform = DeviceInfo.Platform.ToString()
            };

            await SendFeedbackToServiceAsync(feedbackData);

            await _analyticsService.TrackEventAsync("feedback_submitted", new Dictionary<string, object>
            {
                ["rating"] = rating,
                ["feedback_length"] = feedback.Length
            });

            await Application.Current.MainPage.DisplayAlert(
                "Gracias",
                "Tu comentario ha sido enviado. ¡Lo revisaremos pronto!",
                "OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting feedback");
            await Application.Current.MainPage.DisplayAlert("Error", "No se pudo enviar el comentario", "OK");
        }
    }

    public async Task ReportBugAsync(string description, Dictionary<string, object> context = null)
    {
        try
        {
            var bugReport = new BugReport
            {
                Description = description,
                Context = context ?? new Dictionary<string, object>(),
                Timestamp = DateTime.UtcNow,
                UserId = await GetCurrentUserIdAsync(),
                AppVersion = AppInfo.VersionString,
                Platform = DeviceInfo.Platform.ToString(),
                DeviceInfo = GetDeviceInfo()
            };

            await SendBugReportToServiceAsync(bugReport);

            await _analyticsService.TrackEventAsync("bug_reported", new Dictionary<string, object>
            {
                ["description_length"] = description.Length
            });

            await Application.Current.MainPage.DisplayAlert(
                "Reporte Enviado",
                "Gracias por reportar el error. Lo investigaremos y solucionaremos pronto.",
                "OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reporting bug");
            await Application.Current.MainPage.DisplayAlert("Error", "No se pudo enviar el reporte", "OK");
        }
    }

    public async Task RequestFeatureAsync(string feature, string description)
    {
        try
        {
            var featureRequest = new FeatureRequest
            {
                Feature = feature,
                Description = description,
                Timestamp = DateTime.UtcNow,
                UserId = await GetCurrentUserIdAsync(),
                AppVersion = AppInfo.VersionString
            };

            await SendFeatureRequestToServiceAsync(featureRequest);

            await _analyticsService.TrackEventAsync("feature_requested", new Dictionary<string, object>
            {
                ["feature"] = feature
            });

            await Application.Current.MainPage.DisplayAlert(
                "Solicitud Enviada",
                "Gracias por tu sugerencia. La consideraremos para futuras versiones.",
                "OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting feature");
            await Application.Current.MainPage.DisplayAlert("Error", "No se pudo enviar la solicitud", "OK");
        }
    }

    public async Task GetFeedbackHistoryAsync()
    {
        // Implementar obtención de historial de feedback
    }

    private async Task ShowFeedbackFormAsync()
    {
        var feedback = await Application.Current.MainPage.DisplayPromptAsync(
            "Comentarios",
            "¿Qué te gusta o qué podríamos mejorar?",
            "Enviar",
            "Cancelar",
            "Escribe tus comentarios aquí...");

        if (!string.IsNullOrEmpty(feedback))
        {
            var rating = await ShowRatingDialogAsync();
            await SubmitFeedbackAsync(feedback, rating);
        }
    }

    private async Task<int> ShowRatingDialogAsync()
    {
        var result = await Application.Current.MainPage.DisplayActionSheet(
            "¿Cómo calificarías la app?",
            "Cancelar",
            null,
            "⭐ (1)",
            "⭐⭐ (2)",
            "⭐⭐⭐ (3)",
            "⭐⭐⭐⭐ (4)",
            "⭐⭐⭐⭐⭐ (5)");

        return result switch
        {
            "⭐ (1)" => 1,
            "⭐⭐ (2)" => 2,
            "⭐⭐⭐ (3)" => 3,
            "⭐⭐⭐⭐ (4)" => 4,
            "⭐⭐⭐⭐⭐ (5)" => 5,
            _ => 3
        };
    }

    private async Task ShowBugReportFormAsync()
    {
        var description = await Application.Current.MainPage.DisplayPromptAsync(
            "Reportar Error",
            "Describe el error que encontraste:",
            "Enviar",
            "Cancelar",
            "Describe el problema...");

        if (!string.IsNullOrEmpty(description))
        {
            await ReportBugAsync(description);
        }
    }

    private async Task ShowFeatureRequestFormAsync()
    {
        var feature = await Application.Current.MainPage.DisplayPromptAsync(
            "Solicitar Función",
            "¿Qué función te gustaría que agregáramos?",
            "Siguiente",
            "Cancelar",
            "Nombre de la función...");

        if (!string.IsNullOrEmpty(feature))
        {
            var description = await Application.Current.MainPage.DisplayPromptAsync(
                "Descripción",
                "Describe cómo funcionaría:",
                "Enviar",
                "Cancelar",
                "Describe la función...");

            if (!string.IsNullOrEmpty(description))
            {
                await RequestFeatureAsync(feature, description);
            }
        }
    }

    private async Task<string> GetCurrentUserIdAsync()
    {
        // Implementar obtención del ID del usuario actual
        return "current_user_id";
    }

    private Dictionary<string, object> GetDeviceInfo()
    {
        return new Dictionary<string, object>
        {
            ["platform"] = DeviceInfo.Platform.ToString(),
            ["model"] = DeviceInfo.Model,
            ["version"] = DeviceInfo.VersionString,
            ["app_version"] = AppInfo.VersionString
        };
    }

    private async Task SendFeedbackToServiceAsync(UserFeedback feedback)
    {
        // Implementar envío a servicio de feedback
        _logger.LogInformation("User Feedback: {Rating} stars - {Feedback}", feedback.Rating, feedback.Feedback);
        await Task.Delay(100);
    }

    private async Task SendBugReportToServiceAsync(BugReport bugReport)
    {
        // Implementar envío a servicio de bug reports
        _logger.LogWarning("Bug Report: {Description}", bugReport.Description);
        await Task.Delay(100);
    }

    private async Task SendFeatureRequestToServiceAsync(FeatureRequest featureRequest)
    {
        // Implementar envío a servicio de feature requests
        _logger.LogInformation("Feature Request: {Feature}", featureRequest.Feature);
        await Task.Delay(100);
    }
}

// Models/UserFeedback.cs
public class UserFeedback
{
    public string Feedback { get; set; }
    public int Rating { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    public DateTime Timestamp { get; set; }
    public string UserId { get; set; }
    public string AppVersion { get; set; }
    public string Platform { get; set; }
}

// Models/BugReport.cs
public class BugReport
{
    public string Description { get; set; }
    public Dictionary<string, object> Context { get; set; }
    public DateTime Timestamp { get; set; }
    public string UserId { get; set; }
    public string AppVersion { get; set; }
    public string Platform { get; set; }
    public Dictionary<string, object> DeviceInfo { get; set; }
}

// Models/FeatureRequest.cs
public class FeatureRequest
{
    public string Feature { get; set; }
    public string Description { get; set; }
    public DateTime Timestamp { get; set; }
    public string UserId { get; set; }
    public string AppVersion { get; set; }
}
```

### **5. Monitoring Dashboard ViewModel**

```csharp
// ViewModels/MonitoringDashboardViewModel.cs
public class MonitoringDashboardViewModel : BaseViewModel
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ICrashReportingService _crashReportingService;
    private readonly IPerformanceMonitoringService _performanceMonitoring;
    private readonly IUserFeedbackService _userFeedbackService;

    private ObservableCollection<MetricItem> _metrics;
    private ObservableCollection<EventItem> _recentEvents;
    private string _appVersion;
    private string _deviceInfo;

    public MonitoringDashboardViewModel(
        IAnalyticsService analyticsService,
        ICrashReportingService crashReportingService,
        IPerformanceMonitoringService performanceMonitoring,
        IUserFeedbackService userFeedbackService)
    {
        _analyticsService = analyticsService;
        _crashReportingService = crashReportingService;
        _performanceMonitoring = performanceMonitoring;
        _userFeedbackService = userFeedbackService;

        _metrics = new ObservableCollection<MetricItem>();
        _recentEvents = new ObservableCollection<EventItem>();

        InitializeAsync();
    }

    public ObservableCollection<MetricItem> Metrics
    {
        get => _metrics;
        set => SetProperty(ref _metrics, value);
    }

    public ObservableCollection<EventItem> RecentEvents
    {
        get => _recentEvents;
        set => SetProperty(ref _recentEvents, value);
    }

    public string AppVersion
    {
        get => _appVersion;
        set => SetProperty(ref _appVersion, value);
    }

    public string DeviceInfo
    {
        get => _deviceInfo;
        set => SetProperty(ref _deviceInfo, value);
    }

    private async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;

            AppVersion = AppInfo.VersionString;
            DeviceInfo = $"{DeviceInfo.Platform} {DeviceInfo.Model}";

            await LoadMetricsAsync();
            await LoadRecentEventsAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadMetricsAsync()
    {
        // Cargar métricas de rendimiento
        Metrics.Clear();
        
        Metrics.Add(new MetricItem { Name = "Tiempo de Inicio", Value = "2.3s", Status = "Good" });
        Metrics.Add(new MetricItem { Name = "Memoria Usada", Value = "45MB", Status = "Good" });
        Metrics.Add(new MetricItem { Name = "Crashes", Value = "0", Status = "Good" });
        Metrics.Add(new MetricItem { Name = "Sesiones", Value = "1,234", Status = "Good" });
    }

    private async Task LoadRecentEventsAsync()
    {
        // Cargar eventos recientes
        RecentEvents.Clear();
        
        RecentEvents.Add(new EventItem { Type = "Event", Message = "Usuario inició sesión", Timestamp = DateTime.Now.AddMinutes(-5) });
        RecentEvents.Add(new EventItem { Type = "Performance", Message = "Pantalla cargada en 1.2s", Timestamp = DateTime.Now.AddMinutes(-10) });
        RecentEvents.Add(new EventItem { Type = "Error", Message = "Error de red temporal", Timestamp = DateTime.Now.AddMinutes(-15) });
    }
}

// Models/MetricItem.cs
public class MetricItem
{
    public string Name { get; set; }
    public string Value { get; set; }
    public string Status { get; set; }
}

// Models/EventItem.cs
public class EventItem
{
    public string Type { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
}
```

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Analytics Implementation**
1. Implementar tracking de eventos
2. Configurar métricas de usuario
3. Crear dashboard de analytics

### **Ejercicio 2: Crash Reporting**
1. Configurar crash reporting
2. Implementar breadcrumbs
3. Crear sistema de reportes

### **Ejercicio 3: Performance Monitoring**
1. Implementar métricas de rendimiento
2. Configurar traces personalizados
3. Crear alertas de performance

## 📝 **Resumen de la Clase**

En esta clase hemos aprendido:

✅ **Analytics** y tracking de usuarios
✅ **Crash reporting** y error tracking
✅ **Performance monitoring** y métricas
✅ **User feedback** y support
✅ **Breadcrumbs** y contexto de errores
✅ **Monitoring dashboard** y visualización

## 🚀 **Próxima Clase**

En la siguiente clase implementaremos:
- **Proyecto final** completo
- **Integración** de todas las funcionalidades
- **Testing** integral
- **Deployment** final

---

**💡 Tip del Día**: El monitoreo y analytics son esenciales para el éxito de una aplicación móvil. Implementa un sistema completo de observabilidad desde el primer día.
