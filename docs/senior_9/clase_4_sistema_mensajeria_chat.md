# 💬 Clase 4: Sistema de Mensajería y Chat

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 3: Solicitudes y Matching de Músicos](../senior_9/clase_3_solicitudes_matching_musicos.md)
- **🏠 Inicio del Módulo**: [Módulo 16: Maestría Total y Liderazgo Técnico](../senior_9/README.md)
- **➡️ Siguiente**: [Clase 5: Sistema de Notificaciones y Alertas](../senior_9/clase_5_sistema_notificaciones_alertas.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** sistema de mensajería en tiempo real
2. **Crear** chat entre músicos y clientes
3. **Configurar** SignalR para comunicación bidireccional
4. **Implementar** gestión de conversaciones
5. **Configurar** notificaciones de mensajes

---

## 💬 **Sistema de Mensajería en Tiempo Real**

### **SignalR Hub para Chat**

```csharp
// MusicalMatching.API/Hubs/ChatHub.cs
using Microsoft.AspNetCore.SignalR;
using MusicalMatching.Application.Services;
using MusicalMatching.Domain.Entities;

namespace MusicalMatching.API.Hubs;

public interface IChatHub
{
    Task SendMessageAsync(string message, Guid recipientId, Guid? requestId = null);
    Task JoinGroupAsync(string groupName);
    Task LeaveGroupAsync(string groupName);
    Task TypingAsync(Guid recipientId, bool isTyping);
}

public class ChatHub : Hub, IChatHub
{
    private readonly IChatService _chatService;
    private readonly IUserService _userService;
    private readonly ILogger<ChatHub> _logger;
    private readonly IConnectionTracker _connectionTracker;

    public ChatHub(
        IChatService chatService,
        IUserService userService,
        ILogger<ChatHub> logger,
        IConnectionTracker connectionTracker)
    {
        _chatService = chatService;
        _userService = userService;
        _connectionTracker = connectionTracker;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserIdFromContext();
        if (userId.HasValue)
        {
            await _connectionTracker.TrackConnectionAsync(userId.Value, Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId.Value}");
            
            _logger.LogInformation("User {UserId} connected with connection {ConnectionId}", 
                userId.Value, Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserIdFromContext();
        if (userId.HasValue)
        {
            await _connectionTracker.TrackDisconnectionAsync(userId.Value, Context.ConnectionId);
            
            _logger.LogInformation("User {UserId} disconnected from connection {ConnectionId}", 
                userId.Value, Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessageAsync(string message, Guid recipientId, Guid? requestId = null)
    {
        var senderId = GetUserIdFromContext();
        if (!senderId.HasValue)
        {
            await Clients.Caller.SendAsync("Error", "User not authenticated");
            return;
        }

        try
        {
            // Validar que el mensaje no esté vacío
            if (string.IsNullOrWhiteSpace(message))
            {
                await Clients.Caller.SendAsync("Error", "Message cannot be empty");
                return;
            }

            // Crear el mensaje en la base de datos
            var chatMessage = await _chatService.CreateMessageAsync(
                senderId.Value, recipientId, message, requestId);

            // Enviar el mensaje al destinatario
            await Clients.Group($"user_{recipientId}").SendAsync("ReceiveMessage", new
            {
                chatMessage.Id,
                chatMessage.SenderId,
                chatMessage.RecipientId,
                chatMessage.Content,
                chatMessage.CreatedAt,
                chatMessage.MessageType,
                RequestId = requestId
            });

            // Confirmar envío al remitente
            await Clients.Caller.SendAsync("MessageSent", chatMessage.Id);

            // Notificar al destinatario si no está en línea
            await NotifyOfflineUserAsync(recipientId, chatMessage);

            _logger.LogInformation("Message sent from {SenderId} to {RecipientId}", 
                senderId.Value, recipientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message from {SenderId} to {RecipientId}", 
                senderId.Value, recipientId);
            await Clients.Caller.SendAsync("Error", "Failed to send message");
        }
    }

    public async Task JoinGroupAsync(string groupName)
    {
        var userId = GetUserIdFromContext();
        if (userId.HasValue)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("UserJoinedGroup", userId.Value, groupName);
            
            _logger.LogInformation("User {UserId} joined group {GroupName}", userId.Value, groupName);
        }
    }

    public async Task LeaveGroupAsync(string groupName)
    {
        var userId = GetUserIdFromContext();
        if (userId.HasValue)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("UserLeftGroup", userId.Value, groupName);
            
            _logger.LogInformation("User {UserId} left group {GroupName}", userId.Value, groupName);
        }
    }

    public async Task TypingAsync(Guid recipientId, bool isTyping)
    {
        var senderId = GetUserIdFromContext();
        if (senderId.HasValue)
        {
            await Clients.Group($"user_{recipientId}").SendAsync("UserTyping", senderId.Value, isTyping);
        }
    }

    private Guid? GetUserIdFromContext()
    {
        var userIdClaim = Context.User?.FindFirst("sub")?.Value;
        return userIdClaim != null ? Guid.Parse(userIdClaim) : null;
    }

    private async Task NotifyOfflineUserAsync(Guid recipientId, ChatMessage message)
    {
        var isOnline = await _connectionTracker.IsUserOnlineAsync(recipientId);
        if (!isOnline)
        {
            // Enviar notificación push o email
            await _chatService.SendOfflineNotificationAsync(recipientId, message);
        }
    }
}

// MusicalMatching.Application/Services/IChatService.cs
public interface IChatService
{
    Task<ChatMessage> CreateMessageAsync(Guid senderId, Guid recipientId, string content, Guid? requestId = null);
    Task<List<ChatMessage>> GetConversationAsync(Guid user1Id, Guid user2Id, int page = 1, int pageSize = 50);
    Task<List<ConversationSummary>> GetUserConversationsAsync(Guid userId);
    Task<ChatMessage> GetMessageByIdAsync(Guid messageId);
    Task<bool> MarkMessageAsReadAsync(Guid messageId, Guid userId);
    Task<int> GetUnreadMessageCountAsync(Guid userId);
    Task SendOfflineNotificationAsync(Guid recipientId, ChatMessage message);
}

public class ChatService : IChatService
{
    private readonly IChatMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        IChatMessageRepository messageRepository,
        IConversationRepository conversationRepository,
        INotificationService notificationService,
        ILogger<ChatService> logger)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<ChatMessage> CreateMessageAsync(
        Guid senderId, Guid recipientId, string content, Guid? requestId = null)
    {
        // Validar que el contenido no esté vacío
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Message content cannot be empty");

        // Crear el mensaje
        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            SenderId = senderId,
            RecipientId = recipientId,
            Content = content.Trim(),
            MessageType = requestId.HasValue ? ChatMessageType.RequestRelated : ChatMessageType.General,
            RequestId = requestId,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        // Guardar en la base de datos
        await _messageRepository.AddAsync(message);

        // Actualizar o crear conversación
        await UpdateConversationAsync(senderId, recipientId, message);

        _logger.LogInformation("Chat message {MessageId} created from {SenderId} to {RecipientId}", 
            message.Id, senderId, recipientId);

        return message;
    }

    public async Task<List<ChatMessage>> GetConversationAsync(
        Guid user1Id, Guid user2Id, int page = 1, int pageSize = 50)
    {
        var messages = await _messageRepository.GetConversationAsync(user1Id, user2Id, page, pageSize);
        
        // Marcar mensajes como leídos
        var unreadMessages = messages.Where(m => m.RecipientId == user1Id && !m.IsRead);
        foreach (var message in unreadMessages)
        {
            await MarkMessageAsReadAsync(message.Id, user1Id);
        }

        return messages.OrderBy(m => m.CreatedAt).ToList();
    }

    public async Task<List<ConversationSummary>> GetUserConversationsAsync(Guid userId)
    {
        var conversations = await _conversationRepository.GetUserConversationsAsync(userId);
        
        // Enriquecer con información del último mensaje
        foreach (var conversation in conversations)
        {
            var lastMessage = await _messageRepository.GetLastMessageAsync(
                conversation.User1Id, conversation.User2Id);
            
            if (lastMessage != null)
            {
                conversation.LastMessage = lastMessage.Content;
                conversation.LastMessageAt = lastMessage.CreatedAt;
                conversation.LastMessageSenderId = lastMessage.SenderId;
            }
        }

        return conversations.OrderByDescending(c => c.LastMessageAt).ToList();
    }

    public async Task<ChatMessage> GetMessageByIdAsync(Guid messageId)
    {
        var message = await _messageRepository.GetByIdAsync(messageId);
        if (message == null)
            throw new NotFoundException("Message not found");

        return message;
    }

    public async Task<bool> MarkMessageAsReadAsync(Guid messageId, Guid userId)
    {
        var message = await _messageRepository.GetByIdAsync(messageId);
        if (message == null)
            return false;

        if (message.RecipientId != userId)
            return false;

        message.IsRead = true;
        message.ReadAt = DateTime.UtcNow;
        
        await _messageRepository.UpdateAsync(message);
        return true;
    }

    public async Task<int> GetUnreadMessageCountAsync(Guid userId)
    {
        return await _messageRepository.GetUnreadCountAsync(userId);
    }

    public async Task SendOfflineNotificationAsync(Guid recipientId, ChatMessage message)
    {
        try
        {
            await _notificationService.SendPushNotificationAsync(recipientId, new
            {
                Title = "New Message",
                Body = $"You have a new message: {message.Content.Substring(0, Math.Min(50, message.Content.Length))}...",
                Data = new { MessageId = message.Id, SenderId = message.SenderId }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send offline notification for message {MessageId}", message.Id);
        }
    }

    private async Task UpdateConversationAsync(Guid user1Id, Guid user2Id, ChatMessage message)
    {
        var conversation = await _conversationRepository.GetConversationAsync(user1Id, user2Id);
        
        if (conversation == null)
        {
            // Crear nueva conversación
            conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                User1Id = user1Id,
                User2Id = user2Id,
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = message.CreatedAt,
                LastMessage = message.Content,
                LastMessageSenderId = message.SenderId
            };
            
            await _conversationRepository.AddAsync(conversation);
        }
        else
        {
            // Actualizar conversación existente
            conversation.LastMessageAt = message.CreatedAt;
            conversation.LastMessage = message.Content;
            conversation.LastMessageSenderId = message.SenderId;
            
            await _conversationRepository.UpdateAsync(conversation);
        }
    }
}
```

---

## 📱 **Entidades del Sistema de Chat**

### **ChatMessage y Conversation**

```csharp
// MusicalMatching.Domain/Entities/ChatMessage.cs
namespace MusicalMatching.Domain.Entities;

public class ChatMessage : BaseEntity
{
    public Guid SenderId { get; set; }
    public virtual User Sender { get; set; }
    public Guid RecipientId { get; set; }
    public virtual User Recipient { get; set; }
    
    // Contenido del mensaje
    public string Content { get; set; } = string.Empty;
    public ChatMessageType MessageType { get; set; }
    public Guid? RequestId { get; set; }
    public virtual MusicianRequest? Request { get; set; }
    
    // Estado del mensaje
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsEdited { get; set; }
    public DateTime? EditedAt { get; set; }
    public string? OriginalContent { get; set; }
    
    // Metadatos
    public DateTime CreatedAt { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentType { get; set; }
    public long? AttachmentSize { get; set; }

    private ChatMessage() { }

    public ChatMessage(Guid senderId, Guid recipientId, string content, 
                      ChatMessageType messageType = ChatMessageType.General, 
                      Guid? requestId = null)
    {
        SenderId = senderId;
        RecipientId = recipientId;
        Content = content ?? throw new ArgumentNullException(nameof(content));
        MessageType = messageType;
        RequestId = requestId;
        
        CreatedAt = DateTime.UtcNow;
        IsRead = false;
        IsEdited = false;
    }

    // Métodos de dominio
    public void MarkAsRead()
    {
        if (IsRead) return;
        
        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }

    public void EditMessage(string newContent)
    {
        if (string.IsNullOrWhiteSpace(newContent))
            throw new ArgumentException("New content cannot be empty");

        if (IsEdited)
            throw new DomainException("Message has already been edited");

        OriginalContent = Content;
        Content = newContent.Trim();
        IsEdited = true;
        EditedAt = DateTime.UtcNow;
    }

    public void AddAttachment(string url, string type, long size)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Attachment URL cannot be empty");

        AttachmentUrl = url;
        AttachmentType = type;
        AttachmentSize = size;
    }

    public bool HasAttachment => !string.IsNullOrEmpty(AttachmentUrl);

    public bool CanBeEdited => !IsRead && DateTime.UtcNow.Subtract(CreatedAt).TotalMinutes <= 5;
}

// MusicalMatching.Domain/Entities/Conversation.cs
public class Conversation : BaseEntity
{
    public Guid User1Id { get; set; }
    public virtual User User1 { get; set; }
    public Guid User2Id { get; set; }
    public virtual User User2 { get; set; }
    
    // Información de la conversación
    public DateTime CreatedAt { get; set; }
    public DateTime LastMessageAt { get; set; }
    public string LastMessage { get; set; } = string.Empty;
    public Guid LastMessageSenderId { get; set; }
    public virtual User LastMessageSender { get; set; }
    
    // Estado
    public bool IsActive { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public Guid? ArchivedByUserId { get; set; }
    public virtual User? ArchivedByUser { get; set; }
    
    // Configuración
    public bool NotificationsEnabled { get; set; } = true;
    public bool Muted { get; set; } = false;
    public DateTime? MutedUntil { get; set; }

    private Conversation() { }

    public Conversation(Guid user1Id, Guid user2Id)
    {
        User1Id = user1Id;
        User2Id = user2Id;
        
        CreatedAt = DateTime.UtcNow;
        LastMessageAt = DateTime.UtcNow;
        IsActive = true;
        NotificationsEnabled = true;
    }

    // Métodos de dominio
    public void UpdateLastMessage(string content, Guid senderId)
    {
        LastMessage = content ?? throw new ArgumentNullException(nameof(content));
        LastMessageSenderId = senderId;
        LastMessageAt = DateTime.UtcNow;
        IsActive = true;
    }

    public void Archive(Guid archivedByUserId)
    {
        if (!IsActive) return;
        
        IsActive = false;
        ArchivedAt = DateTime.UtcNow;
        ArchivedByUserId = archivedByUserId;
    }

    public void Unarchive()
    {
        if (IsActive) return;
        
        IsActive = true;
        ArchivedAt = null;
        ArchivedByUserId = null;
    }

    public void Mute(TimeSpan? duration = null)
    {
        Muted = true;
        MutedUntil = duration.HasValue ? DateTime.UtcNow.Add(duration.Value) : null;
    }

    public void Unmute()
    {
        Muted = false;
        MutedUntil = null;
    }

    public bool IsMuted => Muted && (MutedUntil == null || MutedUntil > DateTime.UtcNow);

    public void ToggleNotifications()
    {
        NotificationsEnabled = !NotificationsEnabled;
    }

    public Guid GetOtherUserId(Guid currentUserId)
    {
        return currentUserId == User1Id ? User2Id : User1Id;
    }
}

// MusicalMatching.Domain/Enums/ChatMessageType.cs
public enum ChatMessageType
{
    General = 0,
    RequestRelated = 1,
    System = 2,
    File = 3,
    Image = 4,
    Audio = 5,
    Video = 6
}
```

---

## 🔌 **Servicio de Conexiones**

### **Connection Tracker**

```csharp
// MusicalMatching.Application/Services/IConnectionTracker.cs
public interface IConnectionTracker
{
    Task TrackConnectionAsync(Guid userId, string connectionId);
    Task TrackDisconnectionAsync(Guid userId, string connectionId);
    Task<bool> IsUserOnlineAsync(Guid userId);
    Task<List<string>> GetUserConnectionsAsync(Guid userId);
    Task<List<Guid>> GetOnlineUsersAsync();
    Task<int> GetOnlineUserCountAsync();
}

public class ConnectionTracker : IConnectionTracker
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<ConnectionTracker> _logger;
    private const string ConnectionPrefix = "user_connections_";
    private const string OnlineUsersKey = "online_users";

    public ConnectionTracker(IMemoryCache cache, ILogger<ConnectionTracker> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task TrackConnectionAsync(Guid userId, string connectionId)
    {
        var cacheKey = $"{ConnectionPrefix}{userId}";
        var connections = await GetUserConnectionsFromCacheAsync(userId);
        
        if (!connections.Contains(connectionId))
        {
            connections.Add(connectionId);
            await SetUserConnectionsInCacheAsync(userId, connections);
        }

        // Actualizar lista de usuarios en línea
        await UpdateOnlineUsersListAsync(userId, true);
        
        _logger.LogInformation("User {UserId} connected with {ConnectionId}. Total connections: {Count}", 
            userId, connectionId, connections.Count);
    }

    public async Task TrackDisconnectionAsync(Guid userId, string connectionId)
    {
        var cacheKey = $"{ConnectionPrefix}{userId}";
        var connections = await GetUserConnectionsFromCacheAsync(userId);
        
        connections.Remove(connectionId);
        
        if (connections.Any())
        {
            await SetUserConnectionsInCacheAsync(userId, connections);
        }
        else
        {
            // Usuario completamente desconectado
            await RemoveUserConnectionsFromCacheAsync(userId);
            await UpdateOnlineUsersListAsync(userId, false);
        }
        
        _logger.LogInformation("User {UserId} disconnected from {ConnectionId}. Remaining connections: {Count}", 
            userId, connectionId, connections.Count);
    }

    public async Task<bool> IsUserOnlineAsync(Guid userId)
    {
        var connections = await GetUserConnectionsFromCacheAsync(userId);
        return connections.Any();
    }

    public async Task<List<string>> GetUserConnectionsAsync(Guid userId)
    {
        return await GetUserConnectionsFromCacheAsync(userId);
    }

    public async Task<List<Guid>> GetOnlineUsersAsync()
    {
        var onlineUsers = _cache.Get<List<Guid>>(OnlineUsersKey);
        return onlineUsers ?? new List<Guid>();
    }

    public async Task<int> GetOnlineUserCountAsync()
    {
        var onlineUsers = await GetOnlineUsersAsync();
        return onlineUsers.Count;
    }

    private async Task<List<string>> GetUserConnectionsFromCacheAsync(Guid userId)
    {
        var cacheKey = $"{ConnectionPrefix}{userId}";
        var connections = _cache.Get<List<string>>(cacheKey);
        return connections ?? new List<string>();
    }

    private async Task SetUserConnectionsInCacheAsync(Guid userId, List<string> connections)
    {
        var cacheKey = $"{ConnectionPrefix}{userId}";
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        };
        
        _cache.Set(cacheKey, connections, options);
    }

    private async Task RemoveUserConnectionsFromCacheAsync(Guid userId)
    {
        var cacheKey = $"{ConnectionPrefix}{userId}";
        _cache.Remove(cacheKey);
    }

    private async Task UpdateOnlineUsersListAsync(Guid userId, bool isOnline)
    {
        var onlineUsers = await GetOnlineUsersAsync();
        
        if (isOnline && !onlineUsers.Contains(userId))
        {
            onlineUsers.Add(userId);
        }
        else if (!isOnline && onlineUsers.Contains(userId))
        {
            onlineUsers.Remove(userId);
        }
        
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        };
        
        _cache.Set(OnlineUsersKey, onlineUsers, options);
    }
}
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Chat en Tiempo Real**
```csharp
// Implementa:
// - SignalR Hub para chat
// - Envío de mensajes en tiempo real
// - Indicador de escritura
// - Grupos de chat
```

### **Ejercicio 2: Gestión de Conversaciones**
```csharp
// Crea:
// - Historial de mensajes
// - Marcado como leído
// - Archivo de conversaciones
// - Notificaciones offline
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **💬 SignalR Hub**: Comunicación en tiempo real
2. **📱 ChatMessage**: Entidad de mensajes del chat
3. **🗂️ Conversation**: Gestión de conversaciones
4. **🔌 ConnectionTracker**: Seguimiento de conexiones
5. **📨 ChatService**: Lógica de negocio del chat

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Sistema de Notificaciones y Alertas**, implementando notificaciones push y email.

---

**¡Has completado la cuarta clase del Módulo 16! 💬🎯**


