# 🏗️ Clase 2: Entidades y Agregados del Dominio

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 1: Arquitectura del Dominio Musical](../senior_9/clase_1_arquitectura_dominio_musical.md)
- **🏠 Inicio del Módulo**: [Módulo 16: Maestría Total y Liderazgo Técnico](../senior_9/README.md)
- **➡️ Siguiente**: [Clase 3: Solicitudes y Matching de Músicos](../senior_9/clase_3_solicitudes_matching_musicos.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** entidades de solicitudes musicales
2. **Definir** agregados del dominio
3. **Crear** entidades de eventos y contratos
4. **Implementar** sistema de reviews y calificaciones
5. **Configurar** relaciones entre entidades

---

## 🎵 **Entidades de Solicitudes Musicales**

### **MusicianRequest - Solicitud de Músico**

```csharp
// MusicalMatching.Domain/Entities/MusicianRequest.cs
namespace MusicalMatching.Domain.Entities;

public class MusicianRequest : BaseEntity
{
    // Información del evento
    public string EventType { get; private set; }
    public DateTime Date { get; private set; }
    public TimeSpan Time { get; private set; }
    public TimeSpan Duration { get; private set; }
    public string Location { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    
    // Requisitos musicales
    public string Instrument { get; private set; }
    public List<string> AdditionalInstruments { get; private set; } = new();
    public string Genre { get; private set; }
    public string Style { get; private set; }
    
    // Presupuesto y términos
    public decimal Budget { get; private set; }
    public decimal MinBudget { get; private set; }
    public decimal MaxBudget { get; private set; }
    public bool IsNegotiable { get; private set; }
    
    // Detalles del evento
    public string EventDescription { get; private set; }
    public int ExpectedAttendees { get; private set; }
    public string DressCode { get; private set; }
    public bool RequiresEquipment { get; private set; }
    public string EquipmentDetails { get; private set; }
    
    // Estado y asignación
    public MusicianRequestStatus Status { get; private set; }
    public Guid CreatedById { get; private set; }
    public virtual User CreatedBy { get; private set; }
    public Guid? AssignedMusicianId { get; private set; }
    public virtual User? AssignedMusician { get; private set; }
    public DateTime? AssignedAt { get; private set; }
    
    // Fechas importantes
    public DateTime CreatedAt { get; private set; }
    public DateTime? Deadline { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    
    // Navegación
    public virtual List<MusicianRequestApplication> Applications { get; private set; } = new();
    public virtual List<MusicianRequestMessage> Messages { get; private set; } = new();
    public virtual Contract? Contract { get; private set; }

    private MusicianRequest() { }

    public MusicianRequest(
        string eventType, DateTime date, TimeSpan time, TimeSpan duration,
        string location, double latitude, double longitude,
        string instrument, string genre, string style,
        decimal budget, decimal minBudget, decimal maxBudget,
        string eventDescription, int expectedAttendees,
        Guid createdById)
    {
        EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
        Date = date;
        Time = time;
        Duration = duration;
        Location = location ?? throw new ArgumentNullException(nameof(location));
        Latitude = latitude;
        Longitude = longitude;
        Instrument = instrument ?? throw new ArgumentNullException(nameof(instrument));
        Genre = genre ?? throw new ArgumentNullException(nameof(genre));
        Style = style ?? throw new ArgumentNullException(nameof(style));
        Budget = budget;
        MinBudget = minBudget;
        MaxBudget = maxBudget;
        EventDescription = eventDescription ?? throw new ArgumentNullException(nameof(eventDescription));
        ExpectedAttendees = expectedAttendees;
        CreatedById = createdById;
        
        Status = MusicianRequestStatus.Open;
        CreatedAt = DateTime.UtcNow;
        Deadline = date.AddDays(-7); // 1 semana antes del evento
        
        ValidateRequest();
    }

    private void ValidateRequest()
    {
        if (Date <= DateTime.UtcNow)
            throw new DomainException("Event date must be in the future");

        if (Duration <= TimeSpan.Zero)
            throw new DomainException("Duration must be positive");

        if (Budget < 0)
            throw new DomainException("Budget cannot be negative");

        if (MinBudget > MaxBudget)
            throw new DomainException("Minimum budget cannot be greater than maximum budget");

        if (ExpectedAttendees <= 0)
            throw new DomainException("Expected attendees must be positive");
    }

    // Métodos de dominio
    public void UpdateDetails(
        string eventType, DateTime date, TimeSpan time, TimeSpan duration,
        string location, double latitude, double longitude,
        string eventDescription, int expectedAttendees)
    {
        if (Status != MusicianRequestStatus.Open)
            throw new DomainException("Cannot update details of non-open request");

        EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
        Date = date;
        Time = time;
        Duration = duration;
        Location = location ?? throw new ArgumentNullException(nameof(location));
        Latitude = latitude;
        Longitude = longitude;
        EventDescription = eventDescription ?? throw new ArgumentNullException(nameof(eventDescription));
        ExpectedAttendees = expectedAttendees;
        
        ValidateRequest();
    }

    public void UpdateBudget(decimal budget, decimal minBudget, decimal maxBudget, bool isNegotiable)
    {
        if (Status != MusicianRequestStatus.Open)
            throw new DomainException("Cannot update budget of non-open request");

        Budget = budget;
        MinBudget = minBudget;
        MaxBudget = maxBudget;
        IsNegotiable = isNegotiable;
        
        if (MinBudget > MaxBudget)
            throw new DomainException("Minimum budget cannot be greater than maximum budget");
    }

    public void AddAdditionalInstrument(string instrument)
    {
        if (string.IsNullOrWhiteSpace(instrument))
            throw new DomainException("Instrument cannot be empty");

        if (!AdditionalInstruments.Contains(instrument))
            AdditionalInstruments.Add(instrument);
    }

    public void AssignMusician(Guid musicianId)
    {
        if (Status != MusicianRequestStatus.Open)
            throw new DomainException("Cannot assign musician to non-open request");

        if (AssignedMusicianId.HasValue)
            throw new DomainException("Musician is already assigned");

        AssignedMusicianId = musicianId;
        AssignedAt = DateTime.UtcNow;
        Status = MusicianRequestStatus.Assigned;
    }

    public void CompleteRequest()
    {
        if (Status != MusicianRequestStatus.Assigned)
            throw new DomainException("Only assigned requests can be completed");

        Status = MusicianRequestStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void CancelRequest()
    {
        if (Status == MusicianRequestStatus.Completed)
            throw new DomainException("Completed requests cannot be cancelled");

        Status = MusicianRequestStatus.Cancelled;
    }

    public bool CanBeAssigned()
    {
        return Status == MusicianRequestStatus.Open && DateTime.UtcNow <= Deadline;
    }

    public bool IsExpired()
    {
        return DateTime.UtcNow > Deadline;
    }
}

// MusicalMatching.Domain/Entities/MusicianRequestApplication.cs
public class MusicianRequestApplication : BaseEntity
{
    public Guid MusicianRequestId { get; private set; }
    public virtual MusicianRequest MusicianRequest { get; private set; }
    public Guid MusicianId { get; private set; }
    public virtual User Musician { get; private set; }
    
    // Propuesta
    public decimal ProposedRate { get; private set; }
    public string Proposal { get; private set; }
    public TimeSpan EstimatedResponseTime { get; private set; }
    public bool CanProvideEquipment { get; private set; }
    public string EquipmentDetails { get; private set; }
    
    // Estado
    public ApplicationStatus Status { get; private set; }
    public DateTime AppliedAt { get; private set; }
    public DateTime? RespondedAt { get; private set; }
    public string? RejectionReason { get; private set; }

    private MusicianRequestApplication() { }

    public MusicianRequestApplication(
        Guid musicianRequestId, Guid musicianId, decimal proposedRate,
        string proposal, TimeSpan estimatedResponseTime,
        bool canProvideEquipment, string equipmentDetails)
    {
        MusicianRequestId = musicianRequestId;
        MusicianId = musicianId;
        ProposedRate = proposedRate;
        Proposal = proposal ?? throw new ArgumentNullException(nameof(proposal));
        EstimatedResponseTime = estimatedResponseTime;
        CanProvideEquipment = canProvideEquipment;
        EquipmentDetails = equipmentDetails ?? string.Empty;
        
        Status = ApplicationStatus.Pending;
        AppliedAt = DateTime.UtcNow;
    }

    // Métodos de dominio
    public void Accept()
    {
        if (Status != ApplicationStatus.Pending)
            throw new DomainException("Only pending applications can be accepted");

        Status = ApplicationStatus.Accepted;
        RespondedAt = DateTime.UtcNow;
    }

    public void Reject(string reason)
    {
        if (Status != ApplicationStatus.Pending)
            throw new DomainException("Only pending applications can be rejected");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Rejection reason is required");

        Status = ApplicationStatus.Rejected;
        RejectionReason = reason;
        RespondedAt = DateTime.UtcNow;
    }

    public void Withdraw()
    {
        if (Status != ApplicationStatus.Pending)
            throw new DomainException("Only pending applications can be withdrawn");

        Status = ApplicationStatus.Withdrawn;
        RespondedAt = DateTime.UtcNow;
    }
}
```

---

## 📋 **Entidades de Eventos y Contratos**

### **Event y Contract**

```csharp
// MusicalMatching.Domain/Entities/Event.cs
public class Event : BaseEntity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTime Date { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public string Location { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    
    // Organizador
    public Guid OrganizerId { get; private set; }
    public virtual User Organizer { get; private set; }
    
    // Detalles del evento
    public EventType Type { get; private set; }
    public EventCategory Category { get; private set; }
    public int ExpectedAttendees { get; private set; }
    public string DressCode { get; private set; }
    public bool IsPublic { get; private set; }
    public string? CoverImageUrl { get; private set; }
    
    // Estado
    public EventStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    
    // Navegación
    public virtual List<MusicianRequest> MusicianRequests { get; private set; } = new();
    public virtual List<EventAttendee> Attendees { get; private set; } = new();

    private Event() { }

    public Event(
        string name, string description, DateTime date,
        TimeSpan startTime, TimeSpan endTime,
        string location, double latitude, double longitude,
        Guid organizerId, EventType type, EventCategory category,
        int expectedAttendees, string dressCode, bool isPublic)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Date = date;
        StartTime = startTime;
        EndTime = endTime;
        Location = location ?? throw new ArgumentNullException(nameof(location));
        Latitude = latitude;
        Longitude = longitude;
        OrganizerId = organizerId;
        Type = type;
        Category = category;
        ExpectedAttendees = expectedAttendees;
        DressCode = dressCode ?? throw new ArgumentNullException(nameof(dressCode));
        IsPublic = isPublic;
        
        Status = EventStatus.Draft;
        CreatedAt = DateTime.UtcNow;
        
        ValidateEvent();
    }

    private void ValidateEvent()
    {
        if (Date <= DateTime.UtcNow)
            throw new DomainException("Event date must be in the future");

        if (StartTime >= EndTime)
            throw new DomainException("Start time must be before end time");

        if (ExpectedAttendees <= 0)
            throw new DomainException("Expected attendees must be positive");
    }

    // Métodos de dominio
    public void Publish()
    {
        if (Status != EventStatus.Draft)
            throw new DomainException("Only draft events can be published");

        Status = EventStatus.Published;
        PublishedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == EventStatus.Cancelled)
            throw new DomainException("Event is already cancelled");

        Status = EventStatus.Cancelled;
    }

    public void UpdateDetails(
        string name, string description, DateTime date,
        TimeSpan startTime, TimeSpan endTime,
        string location, double latitude, double longitude)
    {
        if (Status != EventStatus.Draft)
            throw new DomainException("Only draft events can be updated");

        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Date = date;
        StartTime = startTime;
        EndTime = endTime;
        Location = location ?? throw new ArgumentNullException(nameof(location));
        Latitude = latitude;
        Longitude = longitude;
        
        ValidateEvent();
    }

    public void AddMusicianRequest(MusicianRequest request)
    {
        if (Status != EventStatus.Published)
            throw new DomainException("Can only add musician requests to published events");

        MusicianRequests.Add(request);
    }
}

// MusicalMatching.Domain/Entities/Contract.cs
public class Contract : BaseEntity
{
    public Guid MusicianRequestId { get; private set; }
    public virtual MusicianRequest MusicianRequest { get; private set; }
    public Guid MusicianId { get; private set; }
    public virtual User Musician { get; private set; }
    public Guid ClientId { get; private set; }
    public virtual User Client { get; private set; }
    
    // Términos del contrato
    public decimal AgreedRate { get; private set; }
    public TimeSpan Duration { get; private set; }
    public DateTime EventDate { get; private set; }
    public string EventLocation { get; private set; }
    public string EventDescription { get; private set; }
    
    // Condiciones
    public bool IncludesEquipment { get; private set; }
    public string EquipmentDetails { get; private set; }
    public string PaymentTerms { get; private set; }
    public string CancellationPolicy { get; private set; }
    
    // Estado
    public ContractStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? SignedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    
    // Firmas
    public DateTime? MusicianSignedAt { get; private set; }
    public DateTime? ClientSignedAt { get; private set; }

    private Contract() { }

    public Contract(
        Guid musicianRequestId, Guid musicianId, Guid clientId,
        decimal agreedRate, TimeSpan duration, DateTime eventDate,
        string eventLocation, string eventDescription,
        bool includesEquipment, string equipmentDetails,
        string paymentTerms, string cancellationPolicy)
    {
        MusicianRequestId = musicianRequestId;
        MusicianId = musicianId;
        ClientId = clientId;
        AgreedRate = agreedRate;
        Duration = duration;
        EventDate = eventDate;
        EventLocation = eventLocation ?? throw new ArgumentNullException(nameof(eventLocation));
        EventDescription = eventDescription ?? throw new ArgumentNullException(nameof(eventDescription));
        IncludesEquipment = includesEquipment;
        EquipmentDetails = equipmentDetails ?? string.Empty;
        PaymentTerms = paymentTerms ?? throw new ArgumentNullException(nameof(paymentTerms));
        CancellationPolicy = cancellationPolicy ?? throw new ArgumentNullException(nameof(cancellationPolicy));
        
        Status = ContractStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    // Métodos de dominio
    public void SignByMusician()
    {
        if (Status != ContractStatus.Draft)
            throw new DomainException("Contract must be in draft status to be signed");

        MusicianSignedAt = DateTime.UtcNow;
        
        if (ClientSignedAt.HasValue)
        {
            Status = ContractStatus.Active;
            SignedAt = DateTime.UtcNow;
        }
    }

    public void SignByClient()
    {
        if (Status != ContractStatus.Draft)
            throw new DomainException("Contract must be in draft status to be signed");

        ClientSignedAt = DateTime.UtcNow;
        
        if (MusicianSignedAt.HasValue)
        {
            Status = ContractStatus.Active;
            SignedAt = DateTime.UtcNow;
        }
    }

    public void Complete()
    {
        if (Status != ContractStatus.Active)
            throw new DomainException("Only active contracts can be completed");

        Status = ContractStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason)
    {
        if (Status == ContractStatus.Completed)
            throw new DomainException("Completed contracts cannot be cancelled");

        Status = ContractStatus.Cancelled;
    }
}
```

---

## ⭐ **Sistema de Reviews y Calificaciones**

### **Review y Rating**

```csharp
// MusicalMatching.Domain/Entities/Review.cs
public class Review : BaseEntity
{
    public Guid ReviewerId { get; private set; }
    public virtual User Reviewer { get; private set; }
    public Guid ReviewedUserId { get; private set; }
    public virtual User ReviewedUser { get; private set; }
    
    // Contenido de la review
    public double Rating { get; private set; }
    public string Comment { get; private set; }
    public ReviewCategory Category { get; private set; }
    public List<string> Tags { get; private set; } = new();
    
    // Contexto
    public Guid? MusicianRequestId { get; private set; }
    public virtual MusicianRequest? MusicianRequest { get; private set; }
    public Guid? ContractId { get; private set; }
    public virtual Contract? Contract { get; private set; }
    
    // Estado
    public ReviewStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsVerified { get; private set; }

    private Review() { }

    public Review(
        Guid reviewerId, Guid reviewedUserId, double rating,
        string comment, ReviewCategory category,
        Guid? musicianRequestId = null, Guid? contractId = null)
    {
        ReviewerId = reviewerId;
        ReviewedUserId = reviewedUserId;
        Rating = rating;
        Comment = comment ?? throw new ArgumentNullException(nameof(comment));
        Category = category;
        MusicianRequestId = musicianRequestId;
        ContractId = contractId;
        
        Status = ReviewStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        IsVerified = false;
        
        ValidateReview();
    }

    private void ValidateReview()
    {
        if (Rating < 1 || Rating > 5)
            throw new DomainException("Rating must be between 1 and 5");

        if (string.IsNullOrWhiteSpace(Comment) || Comment.Length < 10)
            throw new DomainException("Comment must be at least 10 characters long");

        if (ReviewerId == ReviewedUserId)
            throw new DomainException("Cannot review yourself");
    }

    // Métodos de dominio
    public void Approve()
    {
        if (Status != ReviewStatus.Pending)
            throw new DomainException("Only pending reviews can be approved");

        Status = ReviewStatus.Approved;
        IsVerified = true;
    }

    public void Reject(string reason)
    {
        if (Status != ReviewStatus.Pending)
            throw new DomainException("Only pending reviews can be rejected");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Rejection reason is required");

        Status = ReviewStatus.Rejected;
    }

    public void UpdateReview(double rating, string comment)
    {
        if (Status != ReviewStatus.Approved)
            throw new DomainException("Only approved reviews can be updated");

        Rating = rating;
        Comment = comment ?? throw new ArgumentNullException(nameof(comment));
        UpdatedAt = DateTime.UtcNow;
        
        ValidateReview();
    }

    public void AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new DomainException("Tag cannot be empty");

        if (!Tags.Contains(tag))
            Tags.Add(tag);
    }

    public void RemoveTag(string tag)
    {
        Tags.Remove(tag);
    }
}
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Entidades del Sistema**
```csharp
// Implementa:
// - MusicianRequest con validaciones
// - Event con gestión de estado
// - Contract con flujo de firma
// - Review con sistema de aprobación
```

### **Ejercicio 2: Agregados del Dominio**
```csharp
// Crea:
// - Agregado MusicianRequest
// - Agregado Event
// - Agregado Contract
// - Relaciones entre entidades
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **🎵 MusicianRequest**: Solicitudes de músicos con estado y validaciones
2. **📋 Event**: Gestión de eventos musicales
3. **📄 Contract**: Contratos entre músicos y clientes
4. **⭐ Review**: Sistema de reviews y calificaciones
5. **🏗️ Agregados**: Estructura de entidades del dominio

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Solicitudes y Matching de Músicos**, implementando el algoritmo de matching y la gestión de solicitudes.

---

**¡Has completado la segunda clase del Módulo 16! 🏗️🎯**


