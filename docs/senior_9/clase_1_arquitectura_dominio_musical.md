# 🎵 Clase 1: Arquitectura del Dominio Musical

## 🧭 Navegación del Módulo

- **🏠 Inicio del Módulo**: [Módulo 16: Maestría Total y Liderazgo Técnico](../senior_9/README.md)
- **➡️ Siguiente**: [Clase 2: Entidades y Agregados del Dominio](../senior_9/clase_2_entidades_agregados_dominio.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Diseñar** la arquitectura del dominio musical
2. **Implementar** entidades principales del sistema
3. **Definir** agregados y raíces de agregado
4. **Establecer** reglas de negocio del dominio
5. **Configurar** el contexto del dominio

---

## 🏗️ **Arquitectura del Dominio Musical**

### **Estructura del Dominio**

```csharp
// MusicalMatching.Domain/Entities/User.cs
namespace MusicalMatching.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; private set; }
    public string FullName { get; private set; }
    public string PhoneNumber { get; private set; }
    public UserRole Role { get; private set; }
    public UserType UserType { get; private set; }
    public bool IsVerified { get; private set; }
    public SubscriptionTier SubscriptionTier { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastLoginAt { get; private set; }
    
    // Navegación
    public virtual MusicianProfile? MusicianProfile { get; private set; }
    public virtual List<MusicianRequest> CreatedRequests { get; private set; } = new();
    public virtual List<MusicianRequest> AssignedRequests { get; private set; } = new();
    public virtual List<Review> GivenReviews { get; private set; } = new();
    public virtual List<Review> ReceivedReviews { get; private set; } = new();

    // Constructor privado para EF Core
    private User() { }

    public User(string email, string fullName, string phoneNumber, UserRole role, UserType userType)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
        FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
        PhoneNumber = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));
        Role = role;
        UserType = userType;
        IsVerified = false;
        SubscriptionTier = SubscriptionTier.Free;
        CreatedAt = DateTime.UtcNow;
        LastLoginAt = DateTime.UtcNow;
    }

    // Métodos de dominio
    public void UpdateProfile(string fullName, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name cannot be empty");

        FullName = fullName;
        PhoneNumber = phoneNumber;
    }

    public void VerifyAccount()
    {
        if (IsVerified)
            throw new DomainException("Account is already verified");

        IsVerified = true;
    }

    public void UpgradeSubscription(SubscriptionTier newTier)
    {
        if (newTier <= SubscriptionTier)
            throw new DomainException("New subscription tier must be higher than current");

        SubscriptionTier = newTier;
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void CreateMusicianProfile(
        List<string> instruments, 
        List<string> genres, 
        string bio, 
        int yearsOfExperience, 
        decimal hourlyRate, 
        string location)
    {
        if (MusicianProfile != null)
            throw new DomainException("User already has a musician profile");

        if (UserType != UserType.Musician)
            throw new DomainException("Only musicians can create musician profiles");

        MusicianProfile = new MusicianProfile(
            Id, instruments, genres, bio, yearsOfExperience, hourlyRate, location);
    }
}

// MusicalMatching.Domain/Entities/MusicianProfile.cs
public class MusicianProfile : BaseEntity
{
    public Guid UserId { get; private set; }
    public virtual User User { get; private set; }
    
    // Información musical
    public List<string> Instruments { get; private set; } = new();
    public List<string> Genres { get; private set; } = new();
    public string Bio { get; private set; }
    public int YearsOfExperience { get; private set; }
    public decimal HourlyRate { get; private set; }
    public string Location { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    
    // Disponibilidad
    public bool IsAvailable { get; private set; }
    public List<AvailabilitySlot> AvailabilitySlots { get; private set; } = new();
    
    // Calificaciones
    public double AverageRating { get; private set; }
    public int TotalReviews { get; private set; }
    public int RecentCancellations { get; private set; }
    public TimeSpan AverageResponseTime { get; private set; }
    
    // Portafolio
    public List<string> PortfolioUrls { get; private set; } = new();
    public List<string> Certifications { get; private set; } = new();
    public List<string> Languages { get; private set; } = new();

    private MusicianProfile() { }

    public MusicianProfile(
        Guid userId, 
        List<string> instruments, 
        List<string> genres, 
        string bio, 
        int yearsOfExperience, 
        decimal hourlyRate, 
        string location)
    {
        UserId = userId;
        Instruments = instruments ?? new List<string>();
        Genres = genres ?? new List<string>();
        Bio = bio ?? throw new ArgumentNullException(nameof(bio));
        YearsOfExperience = yearsOfExperience;
        HourlyRate = hourlyRate;
        Location = location ?? throw new ArgumentNullException(nameof(location));
        IsAvailable = true;
        AverageRating = 0.0;
        TotalReviews = 0;
        RecentCancellations = 0;
        AverageResponseTime = TimeSpan.Zero;
    }

    // Métodos de dominio
    public void UpdateAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
    }

    public void AddAvailabilitySlot(DayOfWeek day, TimeSpan startTime, TimeSpan endTime)
    {
        var slot = new AvailabilitySlot(day, startTime, endTime);
        AvailabilitySlots.Add(slot);
    }

    public void UpdateHourlyRate(decimal newRate)
    {
        if (newRate < 0)
            throw new DomainException("Hourly rate cannot be negative");

        HourlyRate = newRate;
    }

    public void AddInstrument(string instrument)
    {
        if (string.IsNullOrWhiteSpace(instrument))
            throw new DomainException("Instrument cannot be empty");

        if (!Instruments.Contains(instrument))
            Instruments.Add(instrument);
    }

    public void AddGenre(string genre)
    {
        if (string.IsNullOrWhiteSpace(genre))
            throw new DomainException("Genre cannot be empty");

        if (!Genres.Contains(genre))
            Genres.Add(genre);
    }

    public void UpdateLocation(string location, double latitude, double longitude)
    {
        Location = location ?? throw new ArgumentNullException(nameof(location));
        Latitude = latitude;
        Longitude = longitude;
    }

    public void AddPortfolioUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("Portfolio URL cannot be empty");

        if (!PortfolioUrls.Contains(url))
            PortfolioUrls.Add(url);
    }

    public void AddCertification(string certification)
    {
        if (string.IsNullOrWhiteSpace(certification))
            throw new DomainException("Certification cannot be empty");

        if (!Certifications.Contains(certification))
            Certifications.Add(certification);
    }

    public void UpdateRating(double newRating)
    {
        if (newRating < 0 || newRating > 5)
            throw new DomainException("Rating must be between 0 and 5");

        TotalReviews++;
        AverageRating = ((AverageRating * (TotalReviews - 1)) + newRating) / TotalReviews;
    }
}

// MusicalMatching.Domain/Entities/AvailabilitySlot.cs
public class AvailabilitySlot : BaseEntity
{
    public DayOfWeek Day { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public bool IsAvailable { get; private set; }

    private AvailabilitySlot() { }

    public AvailabilitySlot(DayOfWeek day, TimeSpan startTime, TimeSpan endTime)
    {
        Day = day;
        StartTime = startTime;
        EndTime = endTime;
        IsAvailable = true;

        ValidateTimeRange();
    }

    private void ValidateTimeRange()
    {
        if (StartTime >= EndTime)
            throw new DomainException("Start time must be before end time");

        if (StartTime < TimeSpan.Zero || EndTime > TimeSpan.FromHours(24))
            throw new DomainException("Time must be within 24-hour range");
    }

    public void SetAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
    }

    public bool IsTimeSlotAvailable(DateTime dateTime)
    {
        if (dateTime.DayOfWeek != Day)
            return false;

        var timeOfDay = dateTime.TimeOfDay;
        return IsAvailable && timeOfDay >= StartTime && timeOfDay <= EndTime;
    }
}
```

---

## 🎯 **Enums y Valores del Dominio**

```csharp
// MusicalMatching.Domain/Enums/UserRole.cs
namespace MusicalMatching.Domain.Enums;

public enum UserRole
{
    User = 0,
    Musician = 1,
    EventOrganizer = 2,
    Admin = 3,
    Moderator = 4
}

public enum UserType
{
    Individual = 0,
    Musician = 1,
    EventOrganizer = 2,
    MusicAgency = 3
}

public enum SubscriptionTier
{
    Free = 0,
    Basic = 1,
    Premium = 2,
    Professional = 3,
    Enterprise = 4
}

// MusicalMatching.Domain/ValueObjects/Location.cs
namespace MusicalMatching.Domain.ValueObjects;

public class Location : ValueObject
{
    public string Address { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string Country { get; private set; }
    public string PostalCode { get; private set; }

    public Location(string address, double latitude, double longitude, 
                   string city, string state, string country, string postalCode)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address));
        Latitude = latitude;
        Longitude = longitude;
        City = city ?? throw new ArgumentNullException(nameof(city));
        State = state ?? throw new ArgumentNullException(nameof(state));
        Country = country ?? throw new ArgumentNullException(nameof(country));
        PostalCode = postalCode ?? throw new ArgumentNullException(nameof(postalCode));

        ValidateCoordinates();
    }

    private void ValidateCoordinates()
    {
        if (Latitude < -90 || Latitude > 90)
            throw new DomainException("Latitude must be between -90 and 90");

        if (Longitude < -180 || Longitude > 180)
            throw new DomainException("Longitude must be between -180 and 180");
    }

    public double CalculateDistanceTo(Location other)
    {
        const double earthRadius = 6371; // Earth's radius in kilometers

        var lat1Rad = Latitude * Math.PI / 180;
        var lat2Rad = other.Latitude * Math.PI / 180;
        var deltaLatRad = (other.Latitude - Latitude) * Math.PI / 180;
        var deltaLonRad = (other.Longitude - Longitude) * Math.PI / 180;

        var a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadius * c;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Address;
        yield return Latitude;
        yield return Longitude;
        yield return City;
        yield return State;
        yield return Country;
        yield return PostalCode;
    }
}
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Entidades del Dominio**
```csharp
// Implementa:
// - Clase User con validaciones
// - Clase MusicianProfile con métodos de dominio
// - Clase AvailabilitySlot con lógica de disponibilidad
// - Value Object Location con cálculo de distancias
```

### **Ejercicio 2: Reglas de Negocio**
```csharp
// Crea:
// - Validaciones de dominio
// - Métodos de negocio
// - Invariantes de entidad
// - Lógica de negocio encapsulada
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **🏗️ Arquitectura del Dominio**: Diseño de entidades principales
2. **👤 Entidad User**: Gestión de usuarios y perfiles
3. **🎵 Entidad MusicianProfile**: Perfil completo del músico
4. **⏰ Entidad AvailabilitySlot**: Gestión de disponibilidad
5. **📍 Value Object Location**: Manejo de ubicaciones y distancias

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Entidades y Agregados del Dominio**, implementando las relaciones entre entidades y los agregados del sistema.

---

**¡Has completado la primera clase del Módulo 16! 🎵🎯**


