# 📊 Clase 8: Sistema de Analytics y Reportes

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 7: Sistema de Reviews y Calificaciones](../senior_9/clase_7_sistema_reviews_calificaciones.md)
- **🏠 Inicio del Módulo**: [Módulo 16: Maestría Total y Liderazgo Técnico](../senior_9/README.md)
- **➡️ Siguiente**: [Clase 9: Sistema de Seguridad Avanzada](../senior_9/clase_9_sistema_seguridad_avanzada.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** sistema de analytics y métricas
2. **Crear** dashboards de administración
3. **Configurar** reportes automáticos
4. **Implementar** análisis de rendimiento
5. **Configurar** alertas y notificaciones

---

## 📊 **Sistema de Analytics y Reportes**

### **Servicio de Analytics**

```csharp
// MusicalMatching.Application/Services/IAnalyticsService.cs
namespace MusicalMatching.Application.Services;

public interface IAnalyticsService
{
    Task<DashboardMetrics> GetDashboardMetricsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<UserAnalytics> GetUserAnalyticsAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<MusicianAnalytics> GetMusicianAnalyticsAsync(Guid musicianId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<EventAnalytics> GetEventAnalyticsAsync(Guid eventId);
    Task<PlatformAnalytics> GetPlatformAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<PerformanceMetric>> GetPerformanceMetricsAsync(MetricType type, DateTime? fromDate = null, DateTime? toDate = null);
    Task<RevenueAnalytics> GetRevenueAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<Alert>> GetActiveAlertsAsync();
    Task<Alert> CreateAlertAsync(CreateAlertRequest request);
    Task<bool> ResolveAlertAsync(Guid alertId, Guid userId);
    Task<List<Report>> GenerateReportsAsync(ReportType type, DateTime? fromDate = null, DateTime? toDate = null);
    Task<Report> ScheduleReportAsync(ScheduleReportRequest request);
}

public class AnalyticsService : IAnalyticsService
{
    private readonly IUserRepository _userRepository;
    private readonly IMusicianProfileRepository _musicianRepository;
    private readonly IMusicianRequestRepository _requestRepository;
    private readonly IContractRepository _contractRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IAnalyticsRepository _analyticsRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(
        IUserRepository userRepository,
        IMusicianProfileRepository musicianRepository,
        IMusicianRequestRepository requestRepository,
        IContractRepository contractRepository,
        IReviewRepository reviewRepository,
        IPaymentRepository paymentRepository,
        IAnalyticsRepository analyticsRepository,
        INotificationService notificationService,
        ILogger<AnalyticsService> logger)
    {
        _userRepository = userRepository;
        _musicianRepository = musicianRepository;
        _requestRepository = requestRepository;
        _contractRepository = contractRepository;
        _reviewRepository = reviewRepository;
        _paymentRepository = paymentRepository;
        _analyticsRepository = analyticsRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<DashboardMetrics> GetDashboardMetricsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
        var to = toDate ?? DateTime.UtcNow;

        _logger.LogInformation("Generating dashboard metrics from {FromDate} to {ToDate}", from, to);

        var metrics = new DashboardMetrics
        {
            Period = new DateRange(from, to),
            UserMetrics = await GetUserMetricsAsync(from, to),
            MusicianMetrics = await GetMusicianMetricsAsync(from, to),
            RequestMetrics = await GetRequestMetricsAsync(from, to),
            ContractMetrics = await GetContractMetricsAsync(from, to),
            RevenueMetrics = await GetRevenueMetricsAsync(from, to),
            ReviewMetrics = await GetReviewMetricsAsync(from, to),
            PerformanceMetrics = await GetPerformanceMetricsAsync(from, to)
        };

        return metrics;
    }

    public async Task<UserAnalytics> GetUserAnalyticsAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
        var to = toDate ?? DateTime.UtcNow;

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found");

        var analytics = new UserAnalytics
        {
            UserId = userId,
            Period = new DateRange(from, to),
            ProfileViews = await _analyticsRepository.GetProfileViewsAsync(userId, from, to),
            RequestsCreated = await _requestRepository.GetUserRequestsCountAsync(userId, from, to),
            ContractsCompleted = await _contractRepository.GetUserContractsCountAsync(userId, from, to),
            ReviewsReceived = await _reviewRepository.GetUserReviewsCountAsync(userId, from, to),
            AverageRating = await _reviewRepository.GetUserAverageRatingAsync(userId, from, to),
            RevenueGenerated = await _paymentRepository.GetUserRevenueAsync(userId, from, to),
            ActivityTimeline = await _analyticsRepository.GetUserActivityTimelineAsync(userId, from, to),
            PopularGenres = await _analyticsRepository.GetUserPopularGenresAsync(userId, from, to),
            GeographicDistribution = await _analyticsRepository.GetUserGeographicDistributionAsync(userId, from, to)
        };

        return analytics;
    }

    public async Task<MusicianAnalytics> GetMusicianAnalyticsAsync(Guid musicianId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
        var to = toDate ?? DateTime.UtcNow;

        var musician = await _musicianRepository.GetMusicianProfileAsync(musicianId);
        if (musician == null)
            throw new NotFoundException("Musician not found");

        var analytics = new MusicianAnalytics
        {
            MusicianId = musicianId,
            Period = new DateRange(from, to),
            ProfileViews = await _analyticsRepository.GetProfileViewsAsync(musicianId, from, to),
            ApplicationsSent = await _requestRepository.GetMusicianApplicationsCountAsync(musicianId, from, to),
            ApplicationsAccepted = await _requestRepository.GetMusicianAcceptedApplicationsCountAsync(musicianId, from, to),
            ContractsCompleted = await _contractRepository.GetMusicianContractsCountAsync(musicianId, from, to),
            ReviewsReceived = await _reviewRepository.GetMusicianReviewsCountAsync(musicianId, from, to),
            AverageRating = await _reviewRepository.GetMusicianAverageRatingAsync(musicianId, from, to),
            RevenueEarned = await _paymentRepository.GetMusicianRevenueAsync(musicianId, from, to),
            AvailabilityUtilization = await _analyticsRepository.GetMusicianAvailabilityUtilizationAsync(musicianId, from, to),
            PopularInstruments = await _analyticsRepository.GetMusicianPopularInstrumentsAsync(musicianId, from, to),
            ResponseTimeMetrics = await _analyticsRepository.GetMusicianResponseTimeMetricsAsync(musicianId, from, to),
            CancellationRate = await _analyticsRepository.GetMusicianCancellationRateAsync(musicianId, from, to)
        };

        return analytics;
    }

    public async Task<PlatformAnalytics> GetPlatformAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
        var to = toDate ?? DateTime.UtcNow;

        var analytics = new PlatformAnalytics
        {
            Period = new DateRange(from, to),
            TotalUsers = await _userRepository.GetTotalUsersCountAsync(),
            ActiveUsers = await _userRepository.GetActiveUsersCountAsync(from, to),
            NewUsers = await _userRepository.GetNewUsersCountAsync(from, to),
            TotalMusicians = await _musicianRepository.GetTotalMusiciansCountAsync(),
            ActiveMusicians = await _musicianRepository.GetActiveMusiciansCountAsync(from, to),
            TotalRequests = await _requestRepository.GetTotalRequestsCountAsync(),
            CompletedRequests = await _requestRepository.GetCompletedRequestsCountAsync(from, to),
            TotalContracts = await _contractRepository.GetTotalContractsCountAsync(),
            CompletedContracts = await _contractRepository.GetCompletedContractsCountAsync(from, to),
            TotalRevenue = await _paymentRepository.GetTotalRevenueAsync(from, to),
            AverageContractValue = await _contractRepository.GetAverageContractValueAsync(from, to),
            UserRetentionRate = await _analyticsRepository.GetUserRetentionRateAsync(from, to),
            MusicianRetentionRate = await _analyticsRepository.GetMusicianRetentionRateAsync(from, to),
            ConversionRate = await _analyticsRepository.GetConversionRateAsync(from, to),
            GeographicDistribution = await _analyticsRepository.GetPlatformGeographicDistributionAsync(from, to),
            GenreDistribution = await _analyticsRepository.GetPlatformGenreDistributionAsync(from, to),
            InstrumentDistribution = await _analyticsRepository.GetPlatformInstrumentDistributionAsync(from, to),
            PerformanceTrends = await _analyticsRepository.GetPlatformPerformanceTrendsAsync(from, to)
        };

        return analytics;
    }

    public async Task<List<Alert>> GetActiveAlertsAsync()
    {
        return await _analyticsRepository.GetActiveAlertsAsync();
    }

    public async Task<Alert> CreateAlertAsync(CreateAlertRequest request)
    {
        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            Type = request.Type,
            Severity = request.Severity,
            Title = request.Title,
            Description = request.Description,
            Condition = request.Condition,
            Threshold = request.Threshold,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        await _analyticsRepository.AddAlertAsync(alert);

        _logger.LogInformation("Alert {AlertId} created: {Title}", alert.Id, alert.Title);

        return alert;
    }

    public async Task<bool> ResolveAlertAsync(Guid alertId, Guid userId)
    {
        var alert = await _analyticsRepository.GetAlertByIdAsync(alertId);
        if (alert == null)
            throw new NotFoundException("Alert not found");

        alert.Resolve(userId);
        await _analyticsRepository.UpdateAlertAsync(alert);

        _logger.LogInformation("Alert {AlertId} resolved by user {UserId}", alertId, userId);

        return true;
    }

    public async Task<List<Report>> GenerateReportsAsync(ReportType type, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
        var to = toDate ?? DateTime.UtcNow;

        var reports = new List<Report>();

        switch (type)
        {
            case ReportType.UserGrowth:
                reports.Add(await GenerateUserGrowthReportAsync(from, to));
                break;
            case ReportType.Revenue:
                reports.Add(await GenerateRevenueReportAsync(from, to));
                break;
            case ReportType.Performance:
                reports.Add(await GeneratePerformanceReportAsync(from, to));
                break;
            case ReportType.Reviews:
                reports.Add(await GenerateReviewsReportAsync(from, to));
                break;
            case ReportType.Comprehensive:
                reports.AddRange(await GenerateComprehensiveReportAsync(from, to));
                break;
        }

        return reports;
    }

    private async Task<DashboardMetrics> GetUserMetricsAsync(DateTime from, DateTime to)
    {
        return new DashboardMetrics
        {
            TotalUsers = await _userRepository.GetTotalUsersCountAsync(),
            NewUsers = await _userRepository.GetNewUsersCountAsync(from, to),
            ActiveUsers = await _userRepository.GetActiveUsersCountAsync(from, to),
            UserGrowthRate = await _analyticsRepository.GetUserGrowthRateAsync(from, to)
        };
    }

    private async Task<DashboardMetrics> GetMusicianMetricsAsync(DateTime from, DateTime to)
    {
        return new DashboardMetrics
        {
            TotalMusicians = await _musicianRepository.GetTotalMusiciansCountAsync(),
            NewMusicians = await _musicianRepository.GetNewMusiciansCountAsync(from, to),
            ActiveMusicians = await _musicianRepository.GetActiveMusiciansCountAsync(from, to),
            MusicianGrowthRate = await _analyticsRepository.GetMusicianGrowthRateAsync(from, to)
        };
    }

    private async Task<DashboardMetrics> GetRequestMetricsAsync(DateTime from, DateTime to)
    {
        return new DashboardMetrics
        {
            TotalRequests = await _requestRepository.GetTotalRequestsCountAsync(),
            NewRequests = await _requestRepository.GetNewRequestsCountAsync(from, to),
            CompletedRequests = await _requestRepository.GetCompletedRequestsCountAsync(from, to),
            RequestCompletionRate = await _analyticsRepository.GetRequestCompletionRateAsync(from, to)
        };
    }

    private async Task<DashboardMetrics> GetContractMetricsAsync(DateTime from, DateTime to)
    {
        return new DashboardMetrics
        {
            TotalContracts = await _contractRepository.GetTotalContractsCountAsync(),
            NewContracts = await _contractRepository.GetNewContractsCountAsync(from, to),
            CompletedContracts = await _contractRepository.GetCompletedContractsCountAsync(from, to),
            ContractCompletionRate = await _analyticsRepository.GetContractCompletionRateAsync(from, to)
        };
    }

    private async Task<DashboardMetrics> GetRevenueMetricsAsync(DateTime from, DateTime to)
    {
        return new DashboardMetrics
        {
            TotalRevenue = await _paymentRepository.GetTotalRevenueAsync(from, to),
            AverageContractValue = await _contractRepository.GetAverageContractValueAsync(from, to),
            RevenueGrowthRate = await _analyticsRepository.GetRevenueGrowthRateAsync(from, to)
        };
    }

    private async Task<DashboardMetrics> GetReviewMetricsAsync(DateTime from, DateTime to)
    {
        return new DashboardMetrics
        {
            TotalReviews = await _reviewRepository.GetTotalReviewsCountAsync(),
            NewReviews = await _reviewRepository.GetNewReviewsCountAsync(from, to),
            AverageRating = await _reviewRepository.GetAverageRatingAsync(from, to),
            ReviewGrowthRate = await _analyticsRepository.GetReviewGrowthRateAsync(from, to)
        };
    }

    private async Task<DashboardMetrics> GetPerformanceMetricsAsync(DateTime from, DateTime to)
    {
        return new DashboardMetrics
        {
            ResponseTime = await _analyticsRepository.GetAverageResponseTimeAsync(from, to),
            Uptime = await _analyticsRepository.GetUptimeAsync(from, to),
            ErrorRate = await _analyticsRepository.GetErrorRateAsync(from, to)
        };
    }

    private async Task<Report> GenerateUserGrowthReportAsync(DateTime from, DateTime to)
    {
        // Implementación del reporte de crecimiento de usuarios
        return new Report
        {
            Id = Guid.NewGuid(),
            Type = ReportType.UserGrowth,
            Title = "User Growth Report",
            Period = new DateRange(from, to),
            GeneratedAt = DateTime.UtcNow,
            Data = await _analyticsRepository.GetUserGrowthDataAsync(from, to)
        };
    }

    private async Task<Report> GenerateRevenueReportAsync(DateTime from, DateTime to)
    {
        // Implementación del reporte de ingresos
        return new Report
        {
            Id = Guid.NewGuid(),
            Type = ReportType.Revenue,
            Title = "Revenue Report",
            Period = new DateRange(from, to),
            GeneratedAt = DateTime.UtcNow,
            Data = await _analyticsRepository.GetRevenueDataAsync(from, to)
        };
    }

    private async Task<Report> GeneratePerformanceReportAsync(DateTime from, DateTime to)
    {
        // Implementación del reporte de rendimiento
        return new Report
        {
            Id = Guid.NewGuid(),
            Type = ReportType.Performance,
            Title = "Performance Report",
            Period = new DateRange(from, to),
            GeneratedAt = DateTime.UtcNow,
            Data = await _analyticsRepository.GetPerformanceDataAsync(from, to)
        };
    }

    private async Task<Report> GenerateReviewsReportAsync(DateTime from, DateTime to)
    {
        // Implementación del reporte de reviews
        return new Report
        {
            Id = Guid.NewGuid(),
            Type = ReportType.Reviews,
            Title = "Reviews Report",
            Period = new DateRange(from, to),
            GeneratedAt = DateTime.UtcNow,
            Data = await _analyticsRepository.GetReviewsDataAsync(from, to)
        };
    }

    private async Task<List<Report>> GenerateComprehensiveReportAsync(DateTime from, DateTime to)
    {
        var reports = new List<Report>();
        
        reports.Add(await GenerateUserGrowthReportAsync(from, to));
        reports.Add(await GenerateRevenueReportAsync(from, to));
        reports.Add(await GeneratePerformanceReportAsync(from, to));
        reports.Add(await GenerateReviewsReportAsync(from, to));
        
        return reports;
    }

    public async Task<EventAnalytics> GetEventAnalyticsAsync(Guid eventId)
    {
        var eventEntity = await _contractRepository.GetEventByIdAsync(eventId);
        if (eventEntity == null)
            throw new NotFoundException("Event not found");

        return new EventAnalytics
        {
            EventId = eventId,
            Views = await _analyticsRepository.GetEventViewsAsync(eventId),
            Applications = await _requestRepository.GetEventApplicationsCountAsync(eventId),
            Contracts = await _contractRepository.GetEventContractsCountAsync(eventId),
            Revenue = await _paymentRepository.GetEventRevenueAsync(eventId),
            Reviews = await _reviewRepository.GetEventReviewsCountAsync(eventId),
            AverageRating = await _reviewRepository.GetEventAverageRatingAsync(eventId)
        };
    }

    public async Task<List<PerformanceMetric>> GetPerformanceMetricsAsync(MetricType type, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
        var to = toDate ?? DateTime.UtcNow;

        return await _analyticsRepository.GetPerformanceMetricsAsync(type, from, to);
    }

    public async Task<RevenueAnalytics> GetRevenueAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
        var to = toDate ?? DateTime.UtcNow;

        return new RevenueAnalytics
        {
            Period = new DateRange(from, to),
            TotalRevenue = await _paymentRepository.GetTotalRevenueAsync(from, to),
            PlatformRevenue = await _paymentRepository.GetPlatformRevenueAsync(from, to),
            MusicianRevenue = await _paymentRepository.GetMusicianRevenueAsync(null, from, to),
            AverageTransactionValue = await _paymentRepository.GetAverageTransactionValueAsync(from, to),
            TransactionCount = await _paymentRepository.GetTransactionCountAsync(from, to),
            RevenueByMonth = await _analyticsRepository.GetRevenueByMonthAsync(from, to),
            RevenueByGenre = await _analyticsRepository.GetRevenueByGenreAsync(from, to),
            RevenueByLocation = await _analyticsRepository.GetRevenueByLocationAsync(from, to)
        };
    }

    public async Task<Report> ScheduleReportAsync(ScheduleReportRequest request)
    {
        var scheduledReport = new ScheduledReport
        {
            Id = Guid.NewGuid(),
            Type = request.Type,
            Title = request.Title,
            Schedule = request.Schedule,
            Recipients = request.Recipients,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        await _analyticsRepository.AddScheduledReportAsync(scheduledReport);

        _logger.LogInformation("Scheduled report {ReportId} created: {Title}", scheduledReport.Id, scheduledReport.Title);

        return new Report
        {
            Id = scheduledReport.Id,
            Type = request.Type,
            Title = request.Title,
            GeneratedAt = DateTime.UtcNow
        };
    }
}
```

---

## 📊 **Entidades del Sistema de Analytics**

### **Alert, Report y Métricas**

```csharp
// MusicalMatching.Domain/Entities/Alert.cs
namespace MusicalMatching.Domain.Entities;

public class Alert : BaseEntity
{
    public AlertType Type { get; private set; }
    public AlertSeverity Severity { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string Condition { get; private set; }
    public double Threshold { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsResolved { get; private set; }
    public Guid? ResolvedBy { get; private set; }
    public virtual User? Resolver { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public string? ResolutionNotes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public virtual User Creator { get; private set; }

    private Alert() { }

    public Alert(
        AlertType type, AlertSeverity severity, string title,
        string description, string condition, double threshold, Guid createdBy)
    {
        Type = type;
        Severity = severity;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        Threshold = threshold;
        IsActive = true;
        IsResolved = false;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
    }

    public void Resolve(Guid resolvedBy, string? notes = null)
    {
        if (!IsActive)
            throw new DomainException("Cannot resolve inactive alert");

        IsResolved = true;
        ResolvedBy = resolvedBy;
        ResolvedAt = DateTime.UtcNow;
        ResolutionNotes = notes;
    }

    public void Activate()
    {
        IsActive = true;
        IsResolved = false;
        ResolvedBy = null;
        ResolvedAt = null;
        ResolutionNotes = null;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}

// MusicalMatching.Domain/Entities/Report.cs
public class Report : BaseEntity
{
    public ReportType Type { get; private set; }
    public string Title { get; private set; }
    public DateRange Period { get; private set; }
    public Dictionary<string, object> Data { get; private set; } = new();
    public ReportStatus Status { get; private set; }
    public DateTime GeneratedAt { get; private set; }
    public Guid? GeneratedBy { get; private set; }
    public virtual User? Generator { get; private set; }
    public string? FilePath { get; private set; }
    public long? FileSize { get; private set; }
    public string? FileFormat { get; private set; }

    private Report() { }

    public Report(ReportType type, string title, DateRange period, Guid? generatedBy = null)
    {
        Type = type;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Period = period;
        Status = ReportStatus.Generated;
        GeneratedAt = DateTime.UtcNow;
        GeneratedBy = generatedBy;
    }

    public void AddData(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Data key cannot be empty");

        Data[key] = value;
    }

    public void SetFile(string filePath, long fileSize, string fileFormat)
    {
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        FileSize = fileSize;
        FileFormat = fileFormat ?? throw new ArgumentNullException(nameof(fileFormat));
    }

    public void MarkAsExported()
    {
        Status = ReportStatus.Exported;
    }

    public void MarkAsFailed()
    {
        Status = ReportStatus.Failed;
    }
}

// MusicalMatching.Domain/ValueObjects/AnalyticsModels.cs
public class DashboardMetrics
{
    public DateRange Period { get; set; }
    public int TotalUsers { get; set; }
    public int NewUsers { get; set; }
    public int ActiveUsers { get; set; }
    public double UserGrowthRate { get; set; }
    public int TotalMusicians { get; set; }
    public int NewMusicians { get; set; }
    public int ActiveMusicians { get; set; }
    public double MusicianGrowthRate { get; set; }
    public int TotalRequests { get; set; }
    public int NewRequests { get; set; }
    public int CompletedRequests { get; set; }
    public double RequestCompletionRate { get; set; }
    public int TotalContracts { get; set; }
    public int NewContracts { get; set; }
    public int CompletedContracts { get; set; }
    public double ContractCompletionRate { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageContractValue { get; set; }
    public double RevenueGrowthRate { get; set; }
    public int TotalReviews { get; set; }
    public int NewReviews { get; set; }
    public double AverageRating { get; set; }
    public double ReviewGrowthRate { get; set; }
    public double ResponseTime { get; set; }
    public double Uptime { get; set; }
    public double ErrorRate { get; set; }
}

public class UserAnalytics
{
    public Guid UserId { get; set; }
    public DateRange Period { get; set; }
    public int ProfileViews { get; set; }
    public int RequestsCreated { get; set; }
    public int ContractsCompleted { get; set; }
    public int ReviewsReceived { get; set; }
    public double AverageRating { get; set; }
    public decimal RevenueGenerated { get; set; }
    public List<ActivityEvent> ActivityTimeline { get; set; } = new();
    public List<GenreStatistic> PopularGenres { get; set; } = new();
    public List<LocationStatistic> GeographicDistribution { get; set; } = new();
}

public class MusicianAnalytics
{
    public Guid MusicianId { get; set; }
    public DateRange Period { get; set; }
    public int ProfileViews { get; set; }
    public int ApplicationsSent { get; set; }
    public int ApplicationsAccepted { get; set; }
    public int ContractsCompleted { get; set; }
    public int ReviewsReceived { get; set; }
    public double AverageRating { get; set; }
    public decimal RevenueEarned { get; set; }
    public double AvailabilityUtilization { get; set; }
    public List<InstrumentStatistic> PopularInstruments { get; set; } = new();
    public ResponseTimeMetrics ResponseTimeMetrics { get; set; } = new();
    public double CancellationRate { get; set; }
}

public class PlatformAnalytics
{
    public DateRange Period { get; set; }
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int NewUsers { get; set; }
    public int TotalMusicians { get; set; }
    public int ActiveMusicians { get; set; }
    public int TotalRequests { get; set; }
    public int CompletedRequests { get; set; }
    public int TotalContracts { get; set; }
    public int CompletedContracts { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageContractValue { get; set; }
    public double UserRetentionRate { get; set; }
    public double MusicianRetentionRate { get; set; }
    public double ConversionRate { get; set; }
    public List<LocationStatistic> GeographicDistribution { get; set; } = new();
    public List<GenreStatistic> GenreDistribution { get; set; } = new();
    public List<InstrumentStatistic> InstrumentDistribution { get; set; } = new();
    public List<PerformanceTrend> PerformanceTrends { get; set; } = new();
}

public class EventAnalytics
{
    public Guid EventId { get; set; }
    public int Views { get; set; }
    public int Applications { get; set; }
    public int Contracts { get; set; }
    public decimal Revenue { get; set; }
    public int Reviews { get; set; }
    public double AverageRating { get; set; }
}

public class RevenueAnalytics
{
    public DateRange Period { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PlatformRevenue { get; set; }
    public decimal MusicianRevenue { get; set; }
    public decimal AverageTransactionValue { get; set; }
    public int TransactionCount { get; set; }
    public List<MonthlyRevenue> RevenueByMonth { get; set; } = new();
    public List<GenreRevenue> RevenueByGenre { get; set; } = new();
    public List<LocationRevenue> RevenueByLocation { get; set; } = new();
}

public class PerformanceMetric
{
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string> Tags { get; set; } = new();
}

public class ResponseTimeMetrics
{
    public double AverageResponseTime { get; set; }
    public double MedianResponseTime { get; set; }
    public double P95ResponseTime { get; set; }
    public double P99ResponseTime { get; set; }
}

public class ActivityEvent
{
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class GenreStatistic
{
    public string Genre { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class InstrumentStatistic
{
    public string Instrument { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class LocationStatistic
{
    public string Location { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class PerformanceTrend
{
    public DateTime Date { get; set; }
    public double Value { get; set; }
    public string Metric { get; set; } = string.Empty;
}

public class MonthlyRevenue
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Revenue { get; set; }
}

public class GenreRevenue
{
    public string Genre { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public double Percentage { get; set; }
}

public class LocationRevenue
{
    public string Location { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public double Percentage { get; set; }
}

public class DateRange
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }

    public DateRange(DateTime from, DateTime to)
    {
        From = from;
        To = to;
    }

    public TimeSpan Duration => To - From;
}

// MusicalMatching.Domain/Enums/AnalyticsEnums.cs
public enum AlertType
{
    Performance = 0,
    Security = 1,
    Business = 2,
    System = 3
}

public enum AlertSeverity
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public enum ReportType
{
    UserGrowth = 0,
    Revenue = 1,
    Performance = 2,
    Reviews = 3,
    Comprehensive = 4
}

public enum ReportStatus
{
    Generated = 0,
    Exported = 1,
    Failed = 2
}

public enum MetricType
{
    User = 0,
    Musician = 1,
    Request = 2,
    Contract = 3,
    Revenue = 4,
    Performance = 5
}
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Dashboard de Analytics**
```csharp
// Implementa:
// - Métricas en tiempo real
// - Gráficos y visualizaciones
// - Filtros y rangos de fechas
// - Exportación de datos
```

### **Ejercicio 2: Sistema de Alertas**
```csharp
// Crea:
// - Configuración de umbrales
// - Notificaciones automáticas
// - Escalamiento de alertas
// - Dashboard de monitoreo
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **📊 Sistema de Analytics**: Métricas y análisis de datos
2. **🚨 Sistema de Alertas**: Monitoreo y notificaciones
3. **📈 Reportes**: Generación y programación de reportes
4. **📊 Dashboards**: Visualización de métricas
5. **🔍 Análisis de Rendimiento**: Métricas de performance

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Sistema de Seguridad Avanzada**, implementando autenticación y autorización robusta.

---

**¡Has completado la octava clase del Módulo 16! 📊🎯**
