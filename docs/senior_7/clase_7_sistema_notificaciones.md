# 🔔 Clase 7: Sistema de Notificaciones

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 6: Validaciones de Negocio Avanzadas](../senior_7/clase_6_validaciones_negocio_avanzadas.md)
- **🏠 Inicio del Módulo**: [Módulo 14: Plataformas Empresariales Reales](../senior_7/README.md)
- **➡️ Siguiente**: [Clase 8: Caching y Performance](../senior_7/clase_8_caching_performance.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** sistema de notificaciones multi-canal
2. **Crear** notificaciones inteligentes y personalizadas
3. **Desarrollar** sistema de preferencias de usuario
4. **Diseñar** notificaciones en tiempo real
5. **Aplicar** patrones de notificación avanzados

---

## 📡 **Sistema de Notificaciones Multi-Canal**

### **Interface de Notificaciones**

```csharp
public interface INotificationService
{
    Task SendNotificationAsync(NotificationRequest request);
    Task SendBulkNotificationAsync(IEnumerable<NotificationRequest> requests);
    Task<NotificationPreference> GetUserPreferencesAsync(Guid userId);
    Task UpdateUserPreferencesAsync(Guid userId, NotificationPreference preferences);
    Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 20);
    Task MarkAsReadAsync(Guid userId, Guid notificationId);
    Task MarkAllAsReadAsync(Guid userId);
    Task DeleteNotificationAsync(Guid userId, Guid notificationId);
}

public class NotificationRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; }
    public Dictionary<string, object> Data { get; set; }
    public List<NotificationChannel> Channels { get; set; }
    public DateTime? ScheduledFor { get; set; }
    public TimeSpan? ExpiresAfter { get; set; }
    public string Category { get; set; }
    public string Template { get; set; }
}

public enum NotificationType
{
    Info,           // Información general
    Success,        // Operación exitosa
    Warning,        // Advertencia
    Error,          // Error
    Reminder,       // Recordatorio
    Update,         // Actualización
    Alert,          // Alerta importante
    System          // Notificación del sistema
}

public enum NotificationPriority
{
    Low,            // Baja prioridad
    Normal,         // Prioridad normal
    High,           // Alta prioridad
    Urgent          // Urgente
}

public enum NotificationChannel
{
    Email,          // Correo electrónico
    Push,           // Notificación push
    SMS,            // Mensaje de texto
    InApp,          // Notificación en la aplicación
    SignalR,        // Tiempo real
    Webhook         // Webhook externo
}

// Servicio principal de notificaciones
public class MultiChannelNotificationService : INotificationService
{
    private readonly IEmailService _emailService;
    private readonly IPushNotificationService _pushService;
    private readonly ISmsService _smsService;
    private readonly IInAppNotificationService _inAppService;
    private readonly ISignalRNotificationService _signalRService;
    private readonly IWebhookService _webhookService;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserPreferenceService _userPreferenceService;
    private readonly ILogger<MultiChannelNotificationService> _logger;

    public MultiChannelNotificationService(
        IEmailService emailService,
        IPushNotificationService pushService,
        ISmsService smsService,
        IInAppNotificationService inAppService,
        ISignalRNotificationService signalRService,
        IWebhookService webhookService,
        INotificationRepository notificationRepository,
        IUserPreferenceService userPreferenceService,
        ILogger<MultiChannelNotificationService> logger)
    {
        _emailService = emailService;
        _pushService = pushService;
        _smsService = smsService;
        _inAppService = inAppService;
        _signalRService = signalRService;
        _webhookService = webhookService;
        _notificationRepository = notificationRepository;
        _userPreferenceService = userPreferenceService;
        _logger = logger;
    }

    public async Task SendNotificationAsync(NotificationRequest request)
    {
        try
        {
            _logger.LogInformation("Sending notification {NotificationId} to user {UserId}", request.Id, request.UserId);

            // Obtener preferencias del usuario
            var userPreferences = await _userPreferenceService.GetUserPreferencesAsync(request.UserId);
            
            // Filtrar canales según preferencias del usuario
            var allowedChannels = request.Channels
                .Where(channel => IsChannelAllowed(channel, userPreferences))
                .ToList();

            if (!allowedChannels.Any())
            {
                _logger.LogWarning("No allowed channels for notification {NotificationId}", request.Id);
                return;
            }

            // Crear notificación en base de datos
            var notification = new Notification
            {
                Id = request.Id,
                UserId = request.UserId,
                Title = request.Title,
                Message = request.Message,
                Type = request.Type,
                Priority = request.Priority,
                Category = request.Category,
                Data = request.Data != null ? JsonSerializer.Serialize(request.Data) : null,
                CreatedAt = DateTime.UtcNow,
                ScheduledFor = request.ScheduledFor,
                ExpiresAt = request.ExpiresAfter.HasValue ? DateTime.UtcNow.Add(request.ExpiresAfter.Value) : null
            };

            await _notificationRepository.CreateAsync(notification);

            // Enviar por cada canal permitido
            var tasks = new List<Task>();
            
            foreach (var channel in allowedChannels)
            {
                tasks.Add(SendToChannelAsync(request, channel, userPreferences));
            }

            await Task.WhenAll(tasks);

            _logger.LogInformation("Notification {NotificationId} sent successfully to {ChannelCount} channels", 
                request.Id, allowedChannels.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification {NotificationId}", request.Id);
            throw;
        }
    }

    private async Task SendToChannelAsync(NotificationRequest request, NotificationChannel channel, NotificationPreference userPreferences)
    {
        try
        {
            switch (channel)
            {
                case NotificationChannel.Email:
                    await _emailService.SendEmailAsync(request.UserId, request.Title, request.Message, request.Template);
                    break;
                    
                case NotificationChannel.Push:
                    await _pushService.SendNotificationAsync(request.UserId, request.Title, request.Message, request.Data);
                    break;
                    
                case NotificationChannel.SMS:
                    await _smsService.SendSmsAsync(request.UserId, request.Message);
                    break;
                    
                case NotificationChannel.InApp:
                    await _inAppService.SendNotificationAsync(request.UserId, request);
                    break;
                    
                case NotificationChannel.SignalR:
                    await _signalRService.SendNotificationAsync(request.UserId, request);
                    break;
                    
                case NotificationChannel.Webhook:
                    await _webhookService.SendWebhookAsync(request.UserId, request);
                    break;
            }

            _logger.LogDebug("Notification {NotificationId} sent to channel {Channel}", request.Id, channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification {NotificationId} to channel {Channel}", request.Id, channel);
        }
    }

    private bool IsChannelAllowed(NotificationChannel channel, NotificationPreference userPreferences)
    {
        return channel switch
        {
            NotificationChannel.Email => userPreferences.EmailEnabled,
            NotificationChannel.Push => userPreferences.PushEnabled,
            NotificationChannel.SMS => userPreferences.SmsEnabled,
            NotificationChannel.InApp => userPreferences.InAppEnabled,
            NotificationChannel.SignalR => userPreferences.RealTimeEnabled,
            NotificationChannel.Webhook => userPreferences.WebhookEnabled,
            _ => false
        };
    }

    public async Task SendBulkNotificationAsync(IEnumerable<NotificationRequest> requests)
    {
        var tasks = requests.Select(request => SendNotificationAsync(request));
        await Task.WhenAll(tasks);
    }

    public async Task<NotificationPreference> GetUserPreferencesAsync(Guid userId)
    {
        return await _userPreferenceService.GetUserPreferencesAsync(userId);
    }

    public async Task UpdateUserPreferencesAsync(Guid userId, NotificationPreference preferences)
    {
        await _userPreferenceService.UpdateUserPreferencesAsync(userId, preferences);
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        return await _notificationRepository.GetUserNotificationsAsync(userId, page, pageSize);
    }

    public async Task MarkAsReadAsync(Guid userId, Guid notificationId)
    {
        await _notificationRepository.MarkAsReadAsync(userId, notificationId);
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        await _notificationRepository.MarkAllAsReadAsync(userId);
    }

    public async Task DeleteNotificationAsync(Guid userId, Guid notificationId)
    {
        await _notificationRepository.DeleteAsync(userId, notificationId);
    }
}
```

---

## 🎨 **Notificaciones Inteligentes y Personalizadas**

### **Sistema de Plantillas y Personalización**

```csharp
public interface INotificationTemplateService
{
    Task<string> RenderTemplateAsync(string templateName, object model);
    Task<NotificationTemplate> GetTemplateAsync(string templateName);
    Task<List<NotificationTemplate>> GetAllTemplatesAsync();
    Task CreateTemplateAsync(NotificationTemplate template);
    Task UpdateTemplateAsync(NotificationTemplate template);
}

public class NotificationTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public NotificationChannel Channel { get; set; }
    public string Category { get; set; }
    public Dictionary<string, string> Variables { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// Servicio de plantillas de notificación
public class NotificationTemplateService : INotificationTemplateService
{
    private readonly INotificationTemplateRepository _templateRepository;
    private readonly ITemplateEngine _templateEngine;
    private readonly ILogger<NotificationTemplateService> _logger;

    public NotificationTemplateService(
        INotificationTemplateRepository templateRepository,
        ITemplateEngine templateEngine,
        ILogger<NotificationTemplateService> logger)
    {
        _templateRepository = templateRepository;
        _templateEngine = templateEngine;
        _logger = logger;
    }

    public async Task<string> RenderTemplateAsync(string templateName, object model)
    {
        try
        {
            var template = await GetTemplateAsync(templateName);
            if (template == null)
            {
                throw new InvalidOperationException($"Template '{templateName}' not found");
            }

            var renderedTemplate = await _templateEngine.RenderAsync(template.Body, model);
            return renderedTemplate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering template {TemplateName}", templateName);
            throw;
        }
    }

    public async Task<NotificationTemplate> GetTemplateAsync(string templateName)
    {
        return await _templateRepository.GetByNameAsync(templateName);
    }

    public async Task<List<NotificationTemplate>> GetAllTemplatesAsync()
    {
        return await _templateRepository.GetAllAsync();
    }

    public async Task CreateTemplateAsync(NotificationTemplate template)
    {
        template.CreatedAt = DateTime.UtcNow;
        await _templateRepository.CreateAsync(template);
    }

    public async Task UpdateTemplateAsync(NotificationTemplate template)
    {
        template.UpdatedAt = DateTime.UtcNow;
        await _templateRepository.UpdateAsync(template);
    }
}

// Plantillas predefinidas para diferentes tipos de notificaciones
public static class NotificationTemplates
{
    public static class MusicianRequest
    {
        public const string NewRequest = "NewMusicianRequest";
        public const string RequestAssigned = "RequestAssigned";
        public const string RequestCompleted = "RequestCompleted";
        public const string RequestCancelled = "RequestCancelled";
        public const string Reminder = "RequestReminder";
    }

    public static class System
    {
        public const string Welcome = "WelcomeEmail";
        public const string PasswordReset = "PasswordReset";
        public const string AccountVerification = "AccountVerification";
        public const string SubscriptionExpiry = "SubscriptionExpiry";
    }

    public static class Marketing
    {
        public const string Newsletter = "Newsletter";
        public const string PromotionalOffer = "PromotionalOffer";
        public const string EventInvitation = "EventInvitation";
    }
}
```

---

## ⚙️ **Sistema de Preferencias de Usuario**

### **Gestión de Preferencias de Notificación**

```csharp
public class NotificationPreference
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    // Canales habilitados
    public bool EmailEnabled { get; set; } = true;
    public bool PushEnabled { get; set; } = true;
    public bool SmsEnabled { get; set; } = false;
    public bool InAppEnabled { get; set; } = true;
    public bool RealTimeEnabled { get; set; } = true;
    public bool WebhookEnabled { get; set; } = false;
    
    // Preferencias por tipo
    public Dictionary<NotificationType, bool> TypePreferences { get; set; } = new();
    
    // Preferencias por categoría
    public Dictionary<string, bool> CategoryPreferences { get; set; } = new();
    
    // Configuración de horarios
    public TimeSpan? QuietHoursStart { get; set; }
    public TimeSpan? QuietHoursEnd { get; set; }
    public bool RespectQuietHours { get; set; } = true;
    
    // Frecuencia de notificaciones
    public NotificationFrequency Frequency { get; set; } = NotificationFrequency.Immediate;
    
    // Configuración de email
    public string EmailFormat { get; set; } = "HTML";
    public string Language { get; set; } = "es-ES";
    public string TimeZone { get; set; } = "UTC";
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public enum NotificationFrequency
{
    Immediate,       // Inmediata
    Hourly,         // Cada hora
    Daily,          // Diaria
    Weekly,         // Semanal
    Never           // Nunca
}

public interface IUserPreferenceService
{
    Task<NotificationPreference> GetUserPreferencesAsync(Guid userId);
    Task UpdateUserPreferencesAsync(Guid userId, NotificationPreference preferences);
    Task<bool> IsNotificationAllowedAsync(Guid userId, NotificationType type, string category);
    Task<bool> IsQuietHoursAsync(Guid userId);
    Task<List<NotificationChannel>> GetAllowedChannelsAsync(Guid userId, NotificationType type, string category);
}

public class UserPreferenceService : IUserPreferenceService
{
    private readonly INotificationPreferenceRepository _preferenceRepository;
    private readonly ILogger<UserPreferenceService> _logger;

    public UserPreferenceService(
        INotificationPreferenceRepository preferenceRepository,
        ILogger<UserPreferenceService> logger)
    {
        _preferenceRepository = preferenceRepository;
        _logger = logger;
    }

    public async Task<NotificationPreference> GetUserPreferencesAsync(Guid userId)
    {
        var preferences = await _preferenceRepository.GetByUserIdAsync(userId);
        
        if (preferences == null)
        {
            // Crear preferencias por defecto
            preferences = CreateDefaultPreferences(userId);
            await _preferenceRepository.CreateAsync(preferences);
        }
        
        return preferences;
    }

    public async Task UpdateUserPreferencesAsync(Guid userId, NotificationPreference preferences)
    {
        preferences.UpdatedAt = DateTime.UtcNow;
        await _preferenceRepository.UpdateAsync(preferences);
    }

    public async Task<bool> IsNotificationAllowedAsync(Guid userId, NotificationType type, string category)
    {
        var preferences = await GetUserPreferencesAsync(userId);
        
        // Verificar si el tipo está habilitado
        if (preferences.TypePreferences.TryGetValue(type, out var typeEnabled) && !typeEnabled)
        {
            return false;
        }
        
        // Verificar si la categoría está habilitada
        if (preferences.CategoryPreferences.TryGetValue(category, out var categoryEnabled) && !categoryEnabled)
        {
            return false;
        }
        
        // Verificar horas de silencio
        if (preferences.RespectQuietHours && await IsQuietHoursAsync(userId))
        {
            return false;
        }
        
        return true;
    }

    public async Task<bool> IsQuietHoursAsync(Guid userId)
    {
        var preferences = await GetUserPreferencesAsync(userId);
        
        if (!preferences.RespectQuietHours || !preferences.QuietHoursStart.HasValue || !preferences.QuietHoursEnd.HasValue)
        {
            return false;
        }
        
        var now = DateTime.UtcNow.TimeOfDay;
        var start = preferences.QuietHoursStart.Value;
        var end = preferences.QuietHoursEnd.Value;
        
        if (start <= end)
        {
            return now >= start && now <= end;
        }
        else
        {
            // Horas de silencio que cruzan la medianoche
            return now >= start || now <= end;
        }
    }

    public async Task<List<NotificationChannel>> GetAllowedChannelsAsync(Guid userId, NotificationType type, string category)
    {
        var preferences = await GetUserPreferencesAsync(userId);
        var allowedChannels = new List<NotificationChannel>();
        
        if (preferences.EmailEnabled) allowedChannels.Add(NotificationChannel.Email);
        if (preferences.PushEnabled) allowedChannels.Add(NotificationChannel.Push);
        if (preferences.SmsEnabled) allowedChannels.Add(NotificationChannel.SMS);
        if (preferences.InAppEnabled) allowedChannels.Add(NotificationChannel.InApp);
        if (preferences.RealTimeEnabled) allowedChannels.Add(NotificationChannel.SignalR);
        if (preferences.WebhookEnabled) allowedChannels.Add(NotificationChannel.Webhook);
        
        return allowedChannels;
    }

    private NotificationPreference CreateDefaultPreferences(Guid userId)
    {
        return new NotificationPreference
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EmailEnabled = true,
            PushEnabled = true,
            SmsEnabled = false,
            InAppEnabled = true,
            RealTimeEnabled = true,
            WebhookEnabled = false,
            TypePreferences = new Dictionary<NotificationType, bool>
            {
                { NotificationType.Info, true },
                { NotificationType.Success, true },
                { NotificationType.Warning, true },
                { NotificationType.Error, true },
                { NotificationType.Reminder, true },
                { NotificationType.Update, true },
                { NotificationType.Alert, true },
                { NotificationType.System, false }
            },
            CategoryPreferences = new Dictionary<string, bool>
            {
                { "MusicianRequest", true },
                { "System", false },
                { "Marketing", false },
                { "Security", true }
            },
            RespectQuietHours = true,
            QuietHoursStart = new TimeSpan(22, 0, 0), // 10:00 PM
            QuietHoursEnd = new TimeSpan(8, 0, 0),    // 8:00 AM
            Frequency = NotificationFrequency.Immediate,
            EmailFormat = "HTML",
            Language = "es-ES",
            TimeZone = "UTC",
            CreatedAt = DateTime.UtcNow
        };
    }
}
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Sistema Multi-Canal**
```csharp
// Implementa un sistema que:
// - Envíe notificaciones por múltiples canales
// - Maneje fallos en canales específicos
// - Implemente reintentos automáticos
// - Proporcione confirmación de entrega
```

### **Ejercicio 2: Notificaciones Inteligentes**
```csharp
// Crea notificaciones que:
// - Se personalicen según el usuario
// - Se adapten al contexto
// - Implementen plantillas dinámicas
// - Manejen múltiples idiomas
```

### **Ejercicio 3: Preferencias de Usuario**
```csharp
// Implementa un sistema que:
// - Permita configurar preferencias granulares
// - Respete horarios de silencio
// - Aprenda de las preferencias del usuario
// - Proporcione opciones de configuración avanzadas
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **📡 Sistema Multi-Canal**: Notificaciones por email, push, SMS, in-app y SignalR
2. **🎨 Plantillas Inteligentes**: Sistema de plantillas personalizables y dinámicas
3. **⚙️ Preferencias de Usuario**: Configuración granular de notificaciones
4. **🔔 Notificaciones en Tiempo Real**: Integración con SignalR
5. **🎯 Personalización Avanzada**: Notificaciones adaptadas al contexto del usuario

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Caching y Performance**, implementando estrategias de optimización y sistemas de cache inteligentes.

---

**¡Has completado la séptima clase del Módulo 14! 🔔📡**


