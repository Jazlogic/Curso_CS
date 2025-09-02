# 🔔 Clase 5: Sistema de Notificaciones y Alertas

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 4: Sistema de Mensajería y Chat](../senior_9/clase_4_sistema_mensajeria_chat.md)
- **🏠 Inicio del Módulo**: [Módulo 16: Maestría Total y Liderazgo Técnico](../senior_9/README.md)
- **➡️ Siguiente**: [Clase 6: Sistema de Pagos y Facturación](../senior_9/clase_6_sistema_pagos_facturacion.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** sistema de notificaciones multi-canal
2. **Crear** notificaciones push y email
3. **Configurar** alertas automáticas
4. **Implementar** plantillas de notificaciones
5. **Configurar** preferencias de usuario

---

## 🔔 **Sistema de Notificaciones Multi-Canal**

### **Servicio de Notificaciones**

```csharp
// MusicalMatching.Application/Services/INotificationService.cs
namespace MusicalMatching.Application.Services;

public interface INotificationService
{
    Task SendNotificationAsync(Guid userId, NotificationType type, object data);
    Task SendPushNotificationAsync(Guid userId, object payload);
    Task SendEmailNotificationAsync(Guid userId, string subject, string body, string template = null);
    Task SendSmsNotificationAsync(Guid userId, string message);
    Task SendInAppNotificationAsync(Guid userId, string title, string message, NotificationPriority priority);
    Task SendWebhookNotificationAsync(string url, object payload);
    Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 20);
    Task MarkNotificationAsReadAsync(Guid notificationId, Guid userId);
    Task<int> GetUnreadNotificationCountAsync(Guid userId);
}

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IPushNotificationService _pushService;
    private readonly ISmsService _smsService;
    private readonly IWebhookService _webhookService;
    private readonly INotificationTemplateService _templateService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        IPushNotificationService pushService,
        ISmsService smsService,
        IWebhookService webhookService,
        INotificationTemplateService templateService,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _pushService = pushService;
        _smsService = smsService;
        _webhookService = webhookService;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task SendNotificationAsync(Guid userId, NotificationType type, object data)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found for notification", userId);
            return;
        }

        var preferences = await GetUserNotificationPreferencesAsync(userId);
        var notification = await CreateNotificationAsync(userId, type, data);

        // Enviar según las preferencias del usuario
        if (preferences.InAppEnabled)
        {
            await SendInAppNotificationAsync(userId, notification.Title, notification.Message, notification.Priority);
        }

        if (preferences.EmailEnabled && ShouldSendEmail(type, preferences))
        {
            await SendEmailNotificationAsync(userId, notification.Title, notification.Message, notification.Template);
        }

        if (preferences.PushEnabled && ShouldSendPush(type, preferences))
        {
            await SendPushNotificationAsync(userId, new
            {
                Title = notification.Title,
                Body = notification.Message,
                Data = notification.Data,
                Type = type.ToString()
            });
        }

        if (preferences.SmsEnabled && ShouldSendSms(type, preferences))
        {
            await SendSmsNotificationAsync(userId, notification.Message);
        }

        _logger.LogInformation("Notification sent to user {UserId} of type {Type}", userId, type);
    }

    public async Task SendPushNotificationAsync(Guid userId, object payload)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user?.DeviceTokens?.Any() != true)
            {
                _logger.LogWarning("No device tokens found for user {UserId}", userId);
                return;
            }

            foreach (var deviceToken in user.DeviceTokens)
            {
                await _pushService.SendAsync(deviceToken.Token, deviceToken.Platform, payload);
            }

            _logger.LogInformation("Push notification sent to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification to user {UserId}", userId);
        }
    }

    public async Task SendEmailNotificationAsync(Guid userId, string subject, string body, string template = null)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                _logger.LogWarning("User {UserId} not found or has no email", userId);
                return;
            }

            var emailBody = body;
            if (!string.IsNullOrEmpty(template))
            {
                emailBody = await _templateService.RenderTemplateAsync(template, new
                {
                    UserName = user.FullName,
                    Body = body,
                    Subject = subject
                });
            }

            await _emailService.SendAsync(user.Email, subject, emailBody);
            _logger.LogInformation("Email notification sent to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email notification to user {UserId}", userId);
        }
    }

    public async Task SendSmsNotificationAsync(Guid userId, string message)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.PhoneNumber))
            {
                _logger.LogWarning("User {UserId} not found or has no phone number", userId);
                return;
            }

            await _smsService.SendAsync(user.PhoneNumber, message);
            _logger.LogInformation("SMS notification sent to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS notification to user {UserId}", userId);
        }
    }

    public async Task SendInAppNotificationAsync(Guid userId, string title, string message, NotificationPriority priority)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Type = NotificationType.InApp,
            Priority = priority,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _notificationRepository.AddAsync(notification);
        _logger.LogInformation("In-app notification created for user {UserId}", userId);
    }

    public async Task SendWebhookNotificationAsync(string url, object payload)
    {
        try
        {
            await _webhookService.SendAsync(url, payload);
            _logger.LogInformation("Webhook notification sent to {Url}", url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send webhook notification to {Url}", url);
        }
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        return await _notificationRepository.GetUserNotificationsAsync(userId, page, pageSize);
    }

    public async Task MarkNotificationAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        if (notification?.UserId == userId)
        {
            notification.MarkAsRead();
            await _notificationRepository.UpdateAsync(notification);
        }
    }

    public async Task<int> GetUnreadNotificationCountAsync(Guid userId)
    {
        return await _notificationRepository.GetUnreadCountAsync(userId);
    }

    private async Task<Notification> CreateNotificationAsync(Guid userId, NotificationType type, object data)
    {
        var template = await _templateService.GetTemplateAsync(type);
        var title = await _templateService.RenderTemplateAsync(template.TitleTemplate, data);
        var message = await _templateService.RenderTemplateAsync(template.MessageTemplate, data);

        return new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            Priority = template.Priority,
            Data = JsonSerializer.Serialize(data),
            Template = template.Name,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    private async Task<UserNotificationPreferences> GetUserNotificationPreferencesAsync(Guid userId)
    {
        // Obtener preferencias del usuario o usar valores por defecto
        return await _notificationRepository.GetUserPreferencesAsync(userId) ?? 
               new UserNotificationPreferences
               {
                   UserId = userId,
                   InAppEnabled = true,
                   EmailEnabled = true,
                   PushEnabled = true,
                   SmsEnabled = false
               };
    }

    private bool ShouldSendEmail(NotificationType type, UserNotificationPreferences preferences)
    {
        return type switch
        {
            NotificationType.ApplicationAccepted => preferences.EmailApplicationUpdates,
            NotificationType.ApplicationRejected => preferences.EmailApplicationUpdates,
            NotificationType.NewMessage => preferences.EmailMessages,
            NotificationType.PaymentReceived => preferences.EmailPayments,
            NotificationType.EventReminder => preferences.EmailReminders,
            _ => true
        };
    }

    private bool ShouldSendPush(NotificationType type, UserNotificationPreferences preferences)
    {
        return type switch
        {
            NotificationType.ApplicationAccepted => preferences.PushApplicationUpdates,
            NotificationType.ApplicationRejected => preferences.PushApplicationUpdates,
            NotificationType.NewMessage => preferences.PushMessages,
            NotificationType.PaymentReceived => preferences.PushPayments,
            NotificationType.EventReminder => preferences.PushReminders,
            _ => true
        };
    }

    private bool ShouldSendSms(NotificationType type, UserNotificationPreferences preferences)
    {
        return type switch
        {
            NotificationType.ApplicationAccepted => preferences.SmsApplicationUpdates,
            NotificationType.ApplicationRejected => preferences.SmsApplicationUpdates,
            NotificationType.PaymentReceived => preferences.SmsPayments,
            NotificationType.EventReminder => preferences.SmsReminders,
            _ => false
        };
    }
}
```

---

## 📱 **Entidades del Sistema de Notificaciones**

### **Notification y UserNotificationPreferences**

```csharp
// MusicalMatching.Domain/Entities/Notification.cs
namespace MusicalMatching.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; private set; }
    public virtual User User { get; private set; }
    
    // Contenido de la notificación
    public string Title { get; private set; }
    public string Message { get; private set; }
    public NotificationType Type { get; private set; }
    public NotificationPriority Priority { get; private set; }
    public string? Data { get; private set; }
    public string? Template { get; private set; }
    
    // Estado
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ScheduledFor { get; private set; }
    
    // Canales de entrega
    public bool SentViaEmail { get; private set; }
    public bool SentViaPush { get; private set; }
    public bool SentViaSms { get; private set; }
    public bool SentViaInApp { get; private set; }
    
    // Metadatos
    public string? ActionUrl { get; private set; }
    public string? ImageUrl { get; private set; }
    public Dictionary<string, string> Metadata { get; private set; } = new();

    private Notification() { }

    public Notification(
        Guid userId, string title, string message, 
        NotificationType type, NotificationPriority priority)
    {
        UserId = userId;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Type = type;
        Priority = priority;
        
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
        SentViaEmail = false;
        SentViaPush = false;
        SentViaSms = false;
        SentViaInApp = false;
    }

    // Métodos de dominio
    public void MarkAsRead()
    {
        if (IsRead) return;
        
        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }

    public void MarkAsUnread()
    {
        IsRead = false;
        ReadAt = null;
    }

    public void SetScheduledFor(DateTime scheduledTime)
    {
        if (scheduledTime <= DateTime.UtcNow)
            throw new DomainException("Scheduled time must be in the future");

        ScheduledFor = scheduledTime;
    }

    public void MarkSentViaEmail()
    {
        SentViaEmail = true;
    }

    public void MarkSentViaPush()
    {
        SentViaPush = true;
    }

    public void MarkSentViaSms()
    {
        SentViaSms = true;
    }

    public void MarkSentViaInApp()
    {
        SentViaInApp = true;
    }

    public void SetActionUrl(string url)
    {
        ActionUrl = url;
    }

    public void SetImageUrl(string url)
    {
        ImageUrl = url;
    }

    public void AddMetadata(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be empty");

        Metadata[key] = value;
    }

    public bool IsScheduled => ScheduledFor.HasValue && ScheduledFor > DateTime.UtcNow;
    public bool IsOverdue => ScheduledFor.HasValue && ScheduledFor <= DateTime.UtcNow && !IsRead;
}

// MusicalMatching.Domain/Entities/UserNotificationPreferences.cs
public class UserNotificationPreferences : BaseEntity
{
    public Guid UserId { get; private set; }
    public virtual User User { get; private set; }
    
    // Configuración general
    public bool InAppEnabled { get; private set; } = true;
    public bool EmailEnabled { get; private set; } = true;
    public bool PushEnabled { get; private set; } = true;
    public bool SmsEnabled { get; private set; } = false;
    
    // Preferencias por tipo de notificación
    public bool EmailApplicationUpdates { get; private set; } = true;
    public bool EmailMessages { get; private set; } = true;
    public bool EmailPayments { get; private set; } = true;
    public bool EmailReminders { get; private set; } = true;
    public bool EmailMarketing { get; private set; } = false;
    
    public bool PushApplicationUpdates { get; private set; } = true;
    public bool PushMessages { get; private set; } = true;
    public bool PushPayments { get; private set; } = true;
    public bool PushReminders { get; private set; } = true;
    public bool PushMarketing { get; private set; } = false;
    
    public bool SmsApplicationUpdates { get; private set; } = false;
    public bool SmsPayments { get; private set; } = true;
    public bool SmsReminders { get; private set; } = false;
    public bool SmsMarketing { get; private set; } = false;
    
    // Configuración de horarios
    public TimeSpan? QuietHoursStart { get; private set; }
    public TimeSpan? QuietHoursEnd { get; private set; }
    public List<DayOfWeek> QuietDays { get; private set; } = new();
    
    // Configuración de frecuencia
    public int MaxNotificationsPerDay { get; private set; } = 50;
    public int MaxEmailsPerDay { get; private set; } = 10;
    public int MaxSmsPerDay { get; private set; } = 5;

    private UserNotificationPreferences() { }

    public UserNotificationPreferences(Guid userId)
    {
        UserId = userId;
    }

    // Métodos de dominio
    public void UpdateGeneralSettings(bool inApp, bool email, bool push, bool sms)
    {
        InAppEnabled = inApp;
        EmailEnabled = email;
        PushEnabled = push;
        SmsEnabled = sms;
    }

    public void UpdateEmailPreferences(bool applicationUpdates, bool messages, bool payments, bool reminders, bool marketing)
    {
        EmailApplicationUpdates = applicationUpdates;
        EmailMessages = messages;
        EmailPayments = payments;
        EmailReminders = reminders;
        EmailMarketing = marketing;
    }

    public void UpdatePushPreferences(bool applicationUpdates, bool messages, bool payments, bool reminders, bool marketing)
    {
        PushApplicationUpdates = applicationUpdates;
        PushMessages = messages;
        PushPayments = payments;
        PushReminders = reminders;
        PushMarketing = marketing;
    }

    public void UpdateSmsPreferences(bool applicationUpdates, bool payments, bool reminders, bool marketing)
    {
        SmsApplicationUpdates = applicationUpdates;
        SmsPayments = payments;
        SmsReminders = reminders;
        SmsMarketing = marketing;
    }

    public void SetQuietHours(TimeSpan start, TimeSpan end)
    {
        if (start >= end)
            throw new DomainException("Start time must be before end time");

        QuietHoursStart = start;
        QuietHoursEnd = end;
    }

    public void ClearQuietHours()
    {
        QuietHoursStart = null;
        QuietHoursEnd = null;
    }

    public void AddQuietDay(DayOfWeek day)
    {
        if (!QuietDays.Contains(day))
            QuietDays.Add(day);
    }

    public void RemoveQuietDay(DayOfWeek day)
    {
        QuietDays.Remove(day);
    }

    public void SetMaxNotificationsPerDay(int max)
    {
        if (max < 0)
            throw new DomainException("Maximum notifications per day cannot be negative");

        MaxNotificationsPerDay = max;
    }

    public bool IsInQuietHours()
    {
        if (!QuietHoursStart.HasValue || !QuietHoursEnd.HasValue)
            return false;

        var now = DateTime.UtcNow.TimeOfDay;
        var start = QuietHoursStart.Value;
        var end = QuietHoursEnd.Value;

        if (start <= end)
        {
            return now >= start && now <= end;
        }
        else
        {
            // Horario que cruza medianoche
            return now >= start || now <= end;
        }
    }

    public bool IsQuietDay()
    {
        return QuietDays.Contains(DateTime.UtcNow.DayOfWeek);
    }

    public bool CanReceiveNotification(NotificationType type, NotificationChannel channel)
    {
        if (IsInQuietHours() || IsQuietDay())
            return false;

        return channel switch
        {
            NotificationChannel.InApp => InAppEnabled,
            NotificationChannel.Email => EmailEnabled && ShouldSendEmail(type),
            NotificationChannel.Push => PushEnabled && ShouldSendPush(type),
            NotificationChannel.Sms => SmsEnabled && ShouldSendSms(type),
            _ => false
        };
    }

    private bool ShouldSendEmail(NotificationType type)
    {
        return type switch
        {
            NotificationType.ApplicationAccepted => EmailApplicationUpdates,
            NotificationType.ApplicationRejected => EmailApplicationUpdates,
            NotificationType.NewMessage => EmailMessages,
            NotificationType.PaymentReceived => EmailPayments,
            NotificationType.EventReminder => EmailReminders,
            NotificationType.Marketing => EmailMarketing,
            _ => true
        };
    }

    private bool ShouldSendPush(NotificationType type)
    {
        return type switch
        {
            NotificationType.ApplicationAccepted => PushApplicationUpdates,
            NotificationType.ApplicationRejected => PushApplicationUpdates,
            NotificationType.NewMessage => PushMessages,
            NotificationType.PaymentReceived => PushPayments,
            NotificationType.EventReminder => PushReminders,
            NotificationType.Marketing => PushMarketing,
            _ => true
        };
    }

    private bool ShouldSendSms(NotificationType type)
    {
        return type switch
        {
            NotificationType.ApplicationAccepted => SmsApplicationUpdates,
            NotificationType.ApplicationRejected => SmsApplicationUpdates,
            NotificationType.PaymentReceived => SmsPayments,
            NotificationType.EventReminder => SmsReminders,
            NotificationType.Marketing => SmsMarketing,
            _ => false
        };
    }
}

// MusicalMatching.Domain/Enums/NotificationType.cs
public enum NotificationType
{
    ApplicationAccepted = 0,
    ApplicationRejected = 1,
    NewMessage = 2,
    PaymentReceived = 3,
    PaymentFailed = 4,
    EventReminder = 5,
    ContractSigned = 6,
    ContractExpired = 7,
    ReviewReceived = 8,
    ProfileUpdated = 9,
    Marketing = 10,
    System = 11,
    Security = 12
}

public enum NotificationPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}

public enum NotificationChannel
{
    InApp = 0,
    Email = 1,
    Push = 2,
    Sms = 3,
    Webhook = 4
}
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Sistema de Notificaciones**
```csharp
// Implementa:
// - Servicio de notificaciones multi-canal
// - Preferencias de usuario
// - Plantillas de notificaciones
// - Programación de notificaciones
```

### **Ejercicio 2: Alertas Automáticas**
```csharp
// Crea:
// - Alertas de eventos próximos
// - Notificaciones de pagos
// - Recordatorios de contratos
// - Alertas de seguridad
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **🔔 Sistema Multi-Canal**: Notificaciones por email, push, SMS e in-app
2. **📱 Entidad Notification**: Gestión completa de notificaciones
3. **⚙️ Preferencias de Usuario**: Configuración personalizada
4. **📧 Servicios de Entrega**: Email, push, SMS y webhooks
5. **🎯 Plantillas**: Sistema de plantillas dinámicas

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Sistema de Pagos y Facturación**, implementando procesamiento de pagos y gestión de facturas.

---

**¡Has completado la quinta clase del Módulo 16! 🔔🎯**
