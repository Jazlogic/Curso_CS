# 📡 Clase 2: Comunicación en Tiempo Real

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 1: Arquitectura de Plataformas Empresariales](../senior_7/clase_1_arquitectura_plataformas_empresariales.md)
- **🏠 Inicio del Módulo**: [Módulo 14: Plataformas Empresariales Reales](../senior_7/README.md)
- **➡️ Siguiente**: [Clase 3: Lógica de Negocio Avanzada](../senior_7/clase_3_logica_negocio_avanzada.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** SignalR Hubs para comunicación en tiempo real
2. **Crear** sistema de notificaciones instantáneas
3. **Desarrollar** chat en vivo entre usuarios
4. **Gestionar** grupos y conexiones de usuarios
5. **Integrar** SignalR con la arquitectura limpia

---

## 📡 **SignalR Hubs para Notificaciones**

### **Configuración del Hub Principal**

SignalR permite comunicación bidireccional en tiempo real entre el servidor y los clientes:

```csharp
// Hub principal para notificaciones del sistema
public class NotificationHub : Hub
{
    private readonly IMusicianRequestService _requestService;        // Servicio de solicitudes
    private readonly IUserService _userService;                      // Servicio de usuarios
    private readonly ILogger<NotificationHub> _logger;               // Logger para auditoría
    private readonly IConnectionTracker _connectionTracker;          // Rastreador de conexiones

    public NotificationHub(
        IMusicianRequestService requestService,
        IUserService userService,
        ILogger<NotificationHub> logger,
        IConnectionTracker connectionTracker)
    {
        _requestService = requestService;
        _userService = userService;
        _logger = logger;
        _connectionTracker = connectionTracker;
    }

    // Método llamado cuando un cliente se conecta al hub
    public override async Task OnConnectedAsync()
    {
        try
        {
            var userId = GetUserIdFromContext();                      // Obtener ID del usuario del contexto
            var userType = await _userService.GetUserTypeAsync(userId); // Obtener tipo de usuario
            
            // Registrar la conexión del usuario
            await _connectionTracker.RegisterConnectionAsync(userId, Context.ConnectionId, userType);
            
            // Unir al usuario a grupos específicos según su tipo
            await JoinUserToGroupsAsync(userId, userType);
            
            _logger.LogInformation("User {UserId} connected with connection {ConnectionId}", 
                userId, Context.ConnectionId);
            
            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnConnectedAsync for connection {ConnectionId}", Context.ConnectionId);
            throw;
        }
    }

    // Método llamado cuando un cliente se desconecta del hub
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        try
        {
            var userId = GetUserIdFromContext();
            
            // Remover la conexión del rastreador
            await _connectionTracker.RemoveConnectionAsync(userId, Context.ConnectionId);
            
            _logger.LogInformation("User {UserId} disconnected from connection {ConnectionId}", 
                userId, Context.ConnectionId);
            
            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnDisconnectedAsync for connection {ConnectionId}", Context.ConnectionId);
        }
    }

    // Método para unirse a grupos específicos según el tipo de usuario
    private async Task JoinUserToGroupsAsync(Guid userId, UserType userType)
    {
        switch (userType)
        {
            case UserType.Musician:
                // Unir a grupos de instrumentos que toca el músico
                var instruments = await _userService.GetUserInstrumentsAsync(userId);
                foreach (var instrument in instruments)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"musicians_{instrument}");
                    _logger.LogDebug("Musician {UserId} joined instrument group: {Instrument}", userId, instrument);
                }
                
                // Unir al grupo general de músicos
                await Groups.AddToGroupAsync(Context.ConnectionId, "musicians_all");
                break;
                
            case UserType.Organizer:
                // Unir al grupo de organizadores
                await Groups.AddToGroupAsync(Context.ConnectionId, "organizers_all");
                
                // Unir al grupo específico del organizador
                await Groups.AddToGroupAsync(Context.ConnectionId, $"organizer_{userId}");
                break;
                
            case UserType.Admin:
                // Los administradores se unen a todos los grupos
                await Groups.AddToGroupAsync(Context.ConnectionId, "admins_all");
                await Groups.AddToGroupAsync(Context.ConnectionId, "musicians_all");
                await Groups.AddToGroupAsync(Context.ConnectionId, "organizers_all");
                break;
        }
    }

    // Método para unirse manualmente a un grupo específico
    public async Task JoinGroup(string groupName)
    {
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Client {ConnectionId} joined group {GroupName}", Context.ConnectionId, groupName);
            
            // Notificar a otros miembros del grupo
            await Clients.OthersInGroup(groupName).SendAsync("UserJoinedGroup", 
                new { ConnectionId = Context.ConnectionId, GroupName = groupName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining group {GroupName} for connection {ConnectionId}", 
                groupName, Context.ConnectionId);
            throw;
        }
    }

    // Método para salir de un grupo específico
    public async Task LeaveGroup(string groupName)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("Client {ConnectionId} left group {GroupName}", Context.ConnectionId, groupName);
            
            // Notificar a otros miembros del grupo
            await Clients.OthersInGroup(groupName).SendAsync("UserLeftGroup", 
                new { ConnectionId = Context.ConnectionId, GroupName = groupName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving group {GroupName} for connection {ConnectionId}", 
                groupName, Context.ConnectionId);
            throw;
        }
    }

    // Método para enviar mensaje a un grupo específico
    public async Task SendMessageToGroup(string groupName, object message)
    {
        try
        {
            var userId = GetUserIdFromContext();
            var userName = await _userService.GetUserNameAsync(userId);
            
            var messageData = new
            {
                SenderId = userId,
                SenderName = userName,
                GroupName = groupName,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
            
            // Enviar mensaje a todos los miembros del grupo
            await Clients.Group(groupName).SendAsync("ReceiveGroupMessage", messageData);
            
            _logger.LogInformation("Message sent to group {GroupName} by user {UserId}", groupName, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to group {GroupName}", groupName);
            throw;
        }
    }

    // Método para enviar mensaje privado a un usuario específico
    public async Task SendPrivateMessage(string targetUserId, object message)
    {
        try
        {
            var senderId = GetUserIdFromContext();
            var senderName = await _userService.GetUserNameAsync(senderId);
            var targetName = await _userService.GetUserNameAsync(Guid.Parse(targetUserId));
            
            var messageData = new
            {
                SenderId = senderId,
                SenderName = senderName,
                TargetId = targetUserId,
                TargetName = targetName,
                Message = message,
                Timestamp = DateTime.UtcNow,
                IsPrivate = true
            };
            
            // Obtener todas las conexiones del usuario objetivo
            var targetConnections = await _connectionTracker.GetUserConnectionsAsync(Guid.Parse(targetUserId));
            
            // Enviar mensaje a todas las conexiones del usuario objetivo
            foreach (var connectionId in targetConnections)
            {
                await Clients.Client(connectionId).SendAsync("ReceivePrivateMessage", messageData);
            }
            
            // Enviar confirmación al remitente
            await Clients.Caller.SendAsync("MessageSent", new { TargetId = targetUserId, Timestamp = DateTime.UtcNow });
            
            _logger.LogInformation("Private message sent from {SenderId} to {TargetId}", senderId, targetUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending private message to user {TargetUserId}", targetUserId);
            throw;
        }
    }

    // Método para indicar que el usuario está escribiendo
    public async Task UserTyping(string groupName)
    {
        try
        {
            var userId = GetUserIdFromContext();
            var userName = await _userService.GetUserNameAsync(userId);
            
            // Notificar a otros miembros del grupo que el usuario está escribiendo
            await Clients.OthersInGroup(groupName).SendAsync("UserTyping", 
                new { UserId = userId, UserName = userName, GroupName = groupName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying typing status for group {GroupName}", groupName);
        }
    }

    // Método para detener la indicación de escritura
    public async Task UserStoppedTyping(string groupName)
    {
        try
        {
            var userId = GetUserIdFromContext();
            var userName = await _userService.GetUserNameAsync(userId);
            
            // Notificar a otros miembros del grupo que el usuario dejó de escribir
            await Clients.OthersInGroup(groupName).SendAsync("UserStoppedTyping", 
                new { UserId = userId, UserName = userName, GroupName = groupName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying stopped typing status for group {GroupName}", groupName);
        }
    }

    // Método auxiliar para obtener el ID del usuario del contexto
    private Guid GetUserIdFromContext()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in context");
        }
        return userId;
    }
}
```

---

## 💬 **ChatHub para Comunicación Directa**

### **Sistema de Chat en Tiempo Real**

El ChatHub maneja la comunicación directa entre usuarios para solicitudes específicas:

```csharp
// Hub especializado para chat entre usuarios
public class ChatHub : Hub
{
    private readonly IChatService _chatService;                      // Servicio de chat
    private readonly IUserService _userService;                      // Servicio de usuarios
    private readonly ILogger<ChatHub> _logger;                       // Logger para auditoría
    private readonly IConnectionTracker _connectionTracker;          // Rastreador de conexiones

    public ChatHub(
        IChatService chatService,
        IUserService userService,
        ILogger<ChatHub> logger,
        IConnectionTracker connectionTracker)
    {
        _chatService = chatService;
        _userService = userService;
        _logger = logger;
        _connectionTracker = connectionTracker;
    }

    // Método para unirse al chat de una solicitud específica
    public async Task JoinRequestChat(Guid requestId)
    {
        try
        {
            var userId = GetUserIdFromContext();
            
            // Verificar que el usuario tiene acceso a esta solicitud
            var hasAccess = await _chatService.UserCanAccessRequestChatAsync(userId, requestId);
            if (!hasAccess)
            {
                throw new UnauthorizedAccessException("User cannot access this request chat");
            }
            
            // Unirse al grupo de chat de la solicitud
            var groupName = $"request_chat_{requestId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            
            // Registrar la conexión en el rastreador
            await _connectionTracker.RegisterConnectionAsync(userId, Context.ConnectionId, "Chat");
            
            // Obtener historial de mensajes
            var messages = await _chatService.GetRequestChatHistoryAsync(requestId, 50); // Últimos 50 mensajes
            
            // Enviar historial al cliente
            await Clients.Caller.SendAsync("ChatHistory", messages);
            
            // Notificar a otros usuarios que alguien se unió al chat
            await Clients.OthersInGroup(groupName).SendAsync("UserJoinedChat", 
                new { UserId = userId, RequestId = requestId, Timestamp = DateTime.UtcNow });
            
            _logger.LogInformation("User {UserId} joined chat for request {RequestId}", userId, requestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining request chat {RequestId} for connection {ConnectionId}", 
                requestId, Context.ConnectionId);
            throw;
        }
    }

    // Método para enviar mensaje en el chat de una solicitud
    public async Task SendChatMessage(Guid requestId, string message)
    {
        try
        {
            var userId = GetUserIdFromContext();
            var userName = await _userService.GetUserNameAsync(userId);
            
            // Validar el mensaje
            if (string.IsNullOrWhiteSpace(message) || message.Length > 1000)
            {
                throw new ArgumentException("Message is invalid or too long");
            }
            
            // Crear el mensaje de chat
            var chatMessage = new ChatMessage
            {
                Id = Guid.NewGuid(),
                RequestId = requestId,
                SenderId = userId,
                SenderName = userName,
                Content = message,
                Timestamp = DateTime.UtcNow,
                MessageType = ChatMessageType.Text
            };
            
            // Guardar el mensaje en la base de datos
            await _chatService.SaveMessageAsync(chatMessage);
            
            // Preparar datos para envío
            var messageData = new
            {
                chatMessage.Id,
                chatMessage.SenderId,
                chatMessage.SenderName,
                chatMessage.Content,
                chatMessage.Timestamp,
                chatMessage.MessageType,
                RequestId = requestId
            };
            
            // Enviar mensaje a todos los usuarios en el chat de la solicitud
            var groupName = $"request_chat_{requestId}";
            await Clients.Group(groupName).SendAsync("ReceiveChatMessage", messageData);
            
            // Notificar a usuarios que no están en el chat (notificaciones push)
            await NotifyOfflineUsersAsync(requestId, chatMessage);
            
            _logger.LogInformation("Chat message sent in request {RequestId} by user {UserId}", requestId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending chat message in request {RequestId}", requestId);
            throw;
        }
    }

    // Método para enviar mensaje de sistema (notificaciones automáticas)
    public async Task SendSystemMessage(Guid requestId, string message, string messageType = "Info")
    {
        try
        {
            var systemMessage = new ChatMessage
            {
                Id = Guid.NewGuid(),
                RequestId = requestId,
                SenderId = Guid.Empty, // ID vacío para mensajes del sistema
                SenderName = "Sistema",
                Content = message,
                Timestamp = DateTime.UtcNow,
                MessageType = ChatMessageType.System
            };
            
            // Guardar el mensaje del sistema
            await _chatService.SaveMessageAsync(systemMessage);
            
            // Preparar datos para envío
            var messageData = new
            {
                systemMessage.Id,
                systemMessage.SenderId,
                systemMessage.SenderName,
                systemMessage.Content,
                systemMessage.Timestamp,
                systemMessage.MessageType,
                RequestId = requestId,
                IsSystemMessage = true,
                SystemMessageType = messageType
            };
            
            // Enviar mensaje del sistema a todos los usuarios en el chat
            var groupName = $"request_chat_{requestId}";
            await Clients.Group(groupName).SendAsync("ReceiveSystemMessage", messageData);
            
            _logger.LogInformation("System message sent in request {RequestId}: {Message}", requestId, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending system message in request {RequestId}", requestId);
            throw;
        }
    }

    // Método para indicar que el usuario está escribiendo en el chat
    public async Task UserTypingInChat(Guid requestId)
    {
        try
        {
            var userId = GetUserIdFromContext();
            var userName = await _userService.GetUserNameAsync(userId);
            
            var groupName = $"request_chat_{requestId}";
            
            // Notificar a otros usuarios que el usuario está escribiendo
            await Clients.OthersInGroup(groupName).SendAsync("UserTypingInChat", 
                new { UserId = userId, UserName = userName, RequestId = requestId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying typing status in chat for request {RequestId}", requestId);
        }
    }

    // Método para detener la indicación de escritura en el chat
    public async Task UserStoppedTypingInChat(Guid requestId)
    {
        try
        {
            var userId = GetUserIdFromContext();
            var userName = await _userService.GetUserNameAsync(userId);
            
            var groupName = $"request_chat_{requestId}";
            
            // Notificar a otros usuarios que el usuario dejó de escribir
            await Clients.OthersInGroup(groupName).SendAsync("UserStoppedTypingInChat", 
                new { UserId = userId, UserName = userName, RequestId = requestId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying stopped typing status in chat for request {RequestId}", requestId);
        }
    }

    // Método para marcar mensajes como leídos
    public async Task MarkMessagesAsRead(Guid requestId, List<Guid> messageIds)
    {
        try
        {
            var userId = GetUserIdFromContext();
            
            // Marcar mensajes como leídos en la base de datos
            await _chatService.MarkMessagesAsReadAsync(userId, requestId, messageIds);
            
            // Notificar a otros usuarios que los mensajes fueron leídos
            var groupName = $"request_chat_{requestId}";
            await Clients.OthersInGroup(groupName).SendAsync("MessagesRead", 
                new { UserId = userId, RequestId = requestId, MessageIds = messageIds });
            
            _logger.LogDebug("Messages marked as read by user {UserId} in request {RequestId}", userId, requestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking messages as read in request {RequestId}", requestId);
            throw;
        }
    }

    // Método auxiliar para notificar a usuarios offline
    private async Task NotifyOfflineUsersAsync(Guid requestId, ChatMessage message)
    {
        try
        {
            // Obtener usuarios que deberían recibir notificaciones pero no están en el chat
            var offlineUsers = await _chatService.GetOfflineUsersForRequestAsync(requestId);
            
            foreach (var userId in offlineUsers)
            {
                // Enviar notificación push o email
                await _chatService.SendOfflineNotificationAsync(userId, requestId, message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying offline users for request {RequestId}", requestId);
        }
    }

    // Método auxiliar para obtener el ID del usuario del contexto
    private Guid GetUserIdFromContext()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in context");
        }
        return userId;
    }
}
```

---

## 🔔 **Servicio de Notificaciones en Tiempo Real**

### **Implementación del Servicio de Notificaciones**

El servicio coordina las notificaciones entre diferentes canales (SignalR, email, push):

```csharp
// Interface para el servicio de notificaciones
public interface INotificationService
{
    Task NotifyMusiciansOfNewRequest(MusicianRequest request);
    Task NotifyOrganizerOfMusicianApplication(Guid requestId, Guid musicianId);
    Task NotifyMusicianOfRequestAssignment(Guid requestId, Guid musicianId);
    Task NotifyAllOfStatusChange(Guid requestId, RequestStatus newStatus);
    Task NotifyTopMatches(Guid requestId, IEnumerable<MusicianMatch> matches);
    Task SendPushNotificationAsync(Guid userId, string title, string message, object data = null);
    Task SendEmailNotificationAsync(Guid userId, string subject, string body, string template = null);
}

// Implementación del servicio de notificaciones con SignalR
public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _notificationHubContext;    // Contexto del hub de notificaciones
    private readonly IHubContext<ChatHub> _chatHubContext;                   // Contexto del hub de chat
    private readonly IConnectionTracker _connectionTracker;                  // Rastreador de conexiones
    private readonly IUserService _userService;                              // Servicio de usuarios
    private readonly IEmailService _emailService;                            // Servicio de email
    private readonly IPushNotificationService _pushService;                  // Servicio de notificaciones push
    private readonly ILogger<SignalRNotificationService> _logger;           // Logger para auditoría

    public SignalRNotificationService(
        IHubContext<NotificationHub> notificationHubContext,
        IHubContext<ChatHub> chatHubContext,
        IConnectionTracker connectionTracker,
        IUserService userService,
        IEmailService emailService,
        IPushNotificationService pushService,
        ILogger<SignalRNotificationService> logger)
    {
        _notificationHubContext = notificationHubContext;
        _chatHubContext = chatHubContext;
        _connectionTracker = connectionTracker;
        _userService = userService;
        _emailService = emailService;
        _pushService = pushService;
        _logger = logger;
    }

    // Notificar a músicos sobre nueva solicitud
    public async Task NotifyMusiciansOfNewRequest(MusicianRequest request)
    {
        try
        {
            _logger.LogInformation("Notifying musicians of new request: {RequestId}", request.Id);
            
            // Preparar datos de la notificación
            var notificationData = new
            {
                Type = "NewRequest",
                RequestId = request.Id,
                EventType = request.EventType,
                Date = request.Date,
                Location = request.Location,
                Instrument = request.Instrument,
                Budget = request.Budget,
                Description = request.Description,
                Timestamp = DateTime.UtcNow
            };
            
            // 1. Notificar a músicos del instrumento específico via SignalR
            var instrumentGroup = $"musicians_{request.Instrument}";
            await _notificationHubContext.Clients.Group(instrumentGroup)
                .SendAsync("ReceiveNotification", notificationData);
            
            // 2. Notificar a todos los músicos (grupo general)
            await _notificationHubContext.Clients.Group("musicians_all")
                .SendAsync("ReceiveNotification", notificationData);
            
            // 3. Enviar notificaciones push a músicos offline
            await SendPushNotificationsToMusiciansAsync(request);
            
            // 4. Enviar emails a músicos que prefieren notificaciones por email
            await SendEmailNotificationsToMusiciansAsync(request);
            
            _logger.LogInformation("Successfully notified musicians of new request: {RequestId}", request.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying musicians of new request: {RequestId}", request.Id);
            throw;
        }
    }

    // Notificar al organizador sobre aplicación de músico
    public async Task NotifyOrganizerOfMusicianApplication(Guid requestId, Guid musicianId)
    {
        try
        {
            _logger.LogInformation("Notifying organizer of musician application: Request {RequestId}, Musician {MusicianId}", 
                requestId, musicianId);
            
            // Obtener datos del músico
            var musician = await _userService.GetUserByIdAsync(musicianId);
            var request = await _userService.GetRequestByIdAsync(requestId);
            
            var notificationData = new
            {
                Type = "MusicianApplication",
                RequestId = requestId,
                MusicianId = musicianId,
                MusicianName = musician.FullName,
                MusicianRating = musician.AverageRating,
                MusicianExperience = musician.YearsOfExperience,
                Timestamp = DateTime.UtcNow
            };
            
            // 1. Notificar al organizador via SignalR
            var organizerGroup = $"organizer_{request.OrganizerId}";
            await _notificationHubContext.Clients.Group(organizerGroup)
                .SendAsync("ReceiveNotification", notificationData);
            
            // 2. Enviar mensaje al chat de la solicitud
            await _chatHubContext.Clients.Group($"request_chat_{requestId}")
                .SendAsync("ReceiveSystemMessage", new
                {
                    Type = "MusicianApplication",
                    Message = $"{musician.FullName} ha aplicado a tu solicitud",
                    MusicianId = musicianId,
                    Timestamp = DateTime.UtcNow
                });
            
            // 3. Enviar notificación push al organizador
            await SendPushNotificationAsync(request.OrganizerId, 
                "Nueva Aplicación", 
                $"{musician.FullName} ha aplicado a tu solicitud");
            
            _logger.LogInformation("Successfully notified organizer of musician application: Request {RequestId}", requestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying organizer of musician application: Request {RequestId}", requestId);
            throw;
        }
    }

    // Notificar al músico sobre asignación de solicitud
    public async Task NotifyMusicianOfRequestAssignment(Guid requestId, Guid musicianId)
    {
        try
        {
            _logger.LogInformation("Notifying musician of request assignment: Request {RequestId}, Musician {MusicianId}", 
                requestId, musicianId);
            
            // Obtener datos de la solicitud
            var request = await _userService.GetRequestByIdAsync(requestId);
            
            var notificationData = new
            {
                Type = "RequestAssignment",
                RequestId = requestId,
                EventType = request.EventType,
                Date = request.Date,
                Location = request.Location,
                Budget = request.Budget,
                OrganizerName = request.Organizer.FullName,
                Timestamp = DateTime.UtcNow
            };
            
            // 1. Notificar al músico via SignalR
            var musicianConnections = await _connectionTracker.GetUserConnectionsAsync(musicianId);
            foreach (var connectionId in musicianConnections)
            {
                await _notificationHubContext.Clients.Client(connectionId)
                    .SendAsync("ReceiveNotification", notificationData);
            }
            
            // 2. Enviar mensaje al chat de la solicitud
            await _chatHubContext.Clients.Group($"request_chat_{requestId}")
                .SendAsync("ReceiveSystemMessage", new
                {
                    Type = "RequestAssignment",
                    Message = $"La solicitud ha sido asignada a {request.AssignedMusician.FullName}",
                    MusicianId = musicianId,
                    Timestamp = DateTime.UtcNow
                });
            
            // 3. Enviar notificación push al músico
            await SendPushNotificationAsync(musicianId, 
                "Solicitud Asignada", 
                $"Has sido asignado a la solicitud: {request.EventType}");
            
            // 4. Enviar email de confirmación
            await SendEmailNotificationAsync(musicianId, 
                "Solicitud Asignada - MussikOn", 
                $"Has sido asignado a la solicitud: {request.EventType}",
                "RequestAssignment");
            
            _logger.LogInformation("Successfully notified musician of request assignment: Request {RequestId}", requestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying musician of request assignment: Request {RequestId}", requestId);
            throw;
        }
    }

    // Notificar cambio de estado de solicitud
    public async Task NotifyAllOfStatusChange(Guid requestId, RequestStatus newStatus)
    {
        try
        {
            _logger.LogInformation("Notifying all users of status change: Request {RequestId} -> {NewStatus}", 
                requestId, newStatus);
            
            // Obtener datos de la solicitud
            var request = await _userService.GetRequestByIdAsync(requestId);
            
            var notificationData = new
            {
                Type = "StatusChange",
                RequestId = requestId,
                OldStatus = request.Status,
                NewStatus = newStatus,
                EventType = request.EventType,
                Timestamp = DateTime.UtcNow
            };
            
            // 1. Notificar a todos los usuarios involucrados via SignalR
            var allUsers = new List<Guid> { request.OrganizerId };
            if (request.AssignedMusicianId.HasValue)
                allUsers.Add(request.AssignedMusicianId.Value);
            
            foreach (var userId in allUsers)
            {
                var userConnections = await _connectionTracker.GetUserConnectionsAsync(userId);
                foreach (var connectionId in userConnections)
                {
                    await _notificationHubContext.Clients.Client(connectionId)
                        .SendAsync("ReceiveNotification", notificationData);
                }
            }
            
            // 2. Enviar mensaje al chat de la solicitud
            await _chatHubContext.Clients.Group($"request_chat_{requestId}")
                .SendAsync("ReceiveSystemMessage", new
                {
                    Type = "StatusChange",
                    Message = $"El estado de la solicitud cambió a: {newStatus}",
                    OldStatus = request.Status,
                    NewStatus = newStatus,
                    Timestamp = DateTime.UtcNow
                });
            
            // 3. Enviar notificaciones push
            foreach (var userId in allUsers)
            {
                await SendPushNotificationAsync(userId, 
                    "Cambio de Estado", 
                    $"La solicitud {request.EventType} cambió a: {newStatus}");
            }
            
            _logger.LogInformation("Successfully notified all users of status change: Request {RequestId}", requestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying users of status change: Request {RequestId}", requestId);
            throw;
        }
    }

    // Notificar a los mejores matches
    public async Task NotifyTopMatches(Guid requestId, IEnumerable<MusicianMatch> matches)
    {
        try
        {
            _logger.LogInformation("Notifying top matches for request: {RequestId}", requestId);
            
            foreach (var match in matches)
            {
                var notificationData = new
                {
                    Type = "TopMatch",
                    RequestId = requestId,
                    MatchScore = match.Score,
                    MatchReason = match.MatchReason,
                    Timestamp = DateTime.UtcNow
                };
                
                // Notificar al músico via SignalR
                var musicianConnections = await _connectionTracker.GetUserConnectionsAsync(match.Musician.Id);
                foreach (var connectionId in musicianConnections)
                {
                    await _notificationHubContext.Clients.Client(connectionId)
                        .SendAsync("ReceiveNotification", notificationData);
                }
                
                // Enviar notificación push
                await SendPushNotificationAsync(match.Musician.Id, 
                    "¡Eres un Gran Match!", 
                    $"Tienes un {match.Score}% de compatibilidad con una solicitud");
            }
            
            _logger.LogInformation("Successfully notified {Count} top matches for request: {RequestId}", 
                matches.Count(), requestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying top matches for request: {RequestId}", requestId);
            throw;
        }
    }

    // Enviar notificación push
    public async Task SendPushNotificationAsync(Guid userId, string title, string message, object data = null)
    {
        try
        {
            await _pushService.SendNotificationAsync(userId, title, message, data);
            _logger.LogDebug("Push notification sent to user {UserId}: {Title}", userId, title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification to user {UserId}", userId);
        }
    }

    // Enviar notificación por email
    public async Task SendEmailNotificationAsync(Guid userId, string subject, string body, string template = null)
    {
        try
        {
            await _emailService.SendEmailAsync(userId, subject, body, template);
            _logger.LogDebug("Email notification sent to user {UserId}: {Subject}", userId, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email notification to user {UserId}", userId);
        }
    }

    // Métodos auxiliares para notificaciones masivas
    private async Task SendPushNotificationsToMusiciansAsync(MusicianRequest request)
    {
        try
        {
            // Obtener músicos del instrumento específico
            var musicians = await _userService.GetMusiciansByInstrumentAsync(request.Instrument);
            
            foreach (var musician in musicians)
            {
                await SendPushNotificationAsync(musician.Id, 
                    "Nueva Solicitud Disponible", 
                    $"Nueva solicitud para {request.Instrument} en {request.Location}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notifications to musicians for request {RequestId}", request.Id);
        }
    }

    private async Task SendEmailNotificationsToMusiciansAsync(MusicianRequest request)
    {
        try
        {
            // Obtener músicos que prefieren notificaciones por email
            var musicians = await _userService.GetMusiciansByInstrumentAsync(request.Instrument);
            var emailPreferenceMusicians = musicians.Where(m => m.NotificationPreference == NotificationType.Email);
            
            foreach (var musician in emailPreferenceMusicians)
            {
                await SendEmailNotificationAsync(musician.Id, 
                    "Nueva Solicitud Musical - MussikOn", 
                    $"Hay una nueva solicitud disponible para {request.Instrument}",
                    "NewRequestAvailable");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email notifications to musicians for request {RequestId}", request.Id);
        }
    }
}
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Implementar SignalR Hub**
```csharp
// Crea un hub para un sistema de chat de soporte:
// - Unirse a canales de soporte
// - Enviar mensajes en tiempo real
// - Indicar estado de escritura
// - Manejar archivos adjuntos
```

### **Ejercicio 2: Sistema de Notificaciones**
```csharp
// Implementa un sistema que:
// - Envíe notificaciones push
// - Genere emails automáticos
// - Use SignalR para tiempo real
// - Maneje preferencias de usuario
```

### **Ejercicio 3: Chat en Tiempo Real**
```csharp
// Crea un chat que:
// - Permita grupos de conversación
// - Maneje mensajes privados
// - Indique usuarios online/offline
// - Persista historial de mensajes
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **📡 SignalR Hubs**: Implementación de comunicación bidireccional en tiempo real
2. **💬 ChatHub**: Sistema de chat para solicitudes específicas
3. **🔔 Notificaciones**: Servicio integrado para múltiples canales de comunicación
4. **👥 Grupos**: Manejo de grupos y conexiones de usuarios
5. **🔄 Integración**: Conexión entre SignalR y la arquitectura limpia

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Lógica de Negocio Avanzada**, implementando algoritmos de matching musical y sistemas de estados complejos.

---

**¡Has completado la segunda clase del Módulo 14! 📡💬**


