# ⭐ Clase 7: Sistema de Reviews y Calificaciones

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 6: Sistema de Pagos y Facturación](../senior_9/clase_6_sistema_pagos_facturacion.md)
- **🏠 Inicio del Módulo**: [Módulo 16: Maestría Total y Liderazgo Técnico](../senior_9/README.md)
- **➡️ Siguiente**: [Clase 8: Sistema de Analytics y Reportes](../senior_9/clase_8_sistema_analytics_reportes.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** sistema de reviews y calificaciones
2. **Crear** algoritmo de scoring de músicos
3. **Configurar** sistema de verificación de reviews
4. **Implementar** análisis de sentimientos
5. **Configurar** reportes de reputación

---

## ⭐ **Sistema de Reviews y Calificaciones**

### **Servicio de Reviews**

```csharp
// MusicalMatching.Application/Services/IReviewService.cs
namespace MusicalMatching.Application.Services;

public interface IReviewService
{
    Task<Review> CreateReviewAsync(CreateReviewRequest request);
    Task<Review> UpdateReviewAsync(UpdateReviewRequest request);
    Task<bool> DeleteReviewAsync(Guid reviewId, Guid userId);
    Task<Review> GetReviewByIdAsync(Guid reviewId);
    Task<List<Review>> GetUserReviewsAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<List<Review>> GetMusicianReviewsAsync(Guid musicianId, int page = 1, int pageSize = 20);
    Task<ReviewSummary> GetMusicianReviewSummaryAsync(Guid musicianId);
    Task<List<Review>> GetContractReviewsAsync(Guid contractId);
    Task<bool> ReportReviewAsync(ReportReviewRequest request);
    Task<List<Review>> GetReportedReviewsAsync(int page = 1, int pageSize = 20);
    Task<Review> ModerateReviewAsync(ModerateReviewRequest request);
}

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IUserRepository _userRepository;
    private readonly IContractRepository _contractRepository;
    private readonly IMusicianProfileRepository _musicianRepository;
    private readonly INotificationService _notificationService;
    private readonly ISentimentAnalysisService _sentimentService;
    private readonly IReviewVerificationService _verificationService;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(
        IReviewRepository reviewRepository,
        IUserRepository userRepository,
        IContractRepository contractRepository,
        IMusicianProfileRepository musicianRepository,
        INotificationService notificationService,
        ISentimentAnalysisService sentimentService,
        IReviewVerificationService verificationService,
        ILogger<ReviewService> logger)
    {
        _reviewRepository = reviewRepository;
        _userRepository = userRepository;
        _contractRepository = contractRepository;
        _musicianRepository = musicianRepository;
        _notificationService = notificationService;
        _sentimentService = sentimentService;
        _verificationService = verificationService;
        _logger = logger;
    }

    public async Task<Review> CreateReviewAsync(CreateReviewRequest request)
    {
        _logger.LogInformation("Creating review for musician {MusicianId} by user {UserId}", 
            request.MusicianId, request.UserId);

        // Validar que el usuario existe
        var reviewer = await _userRepository.GetByIdAsync(request.UserId);
        if (reviewer == null)
            throw new NotFoundException("Reviewer not found");

        // Validar que el músico existe
        var musician = await _musicianRepository.GetMusicianProfileAsync(request.MusicianId);
        if (musician == null)
            throw new NotFoundException("Musician not found");

        // Validar que no se puede hacer review a uno mismo
        if (request.UserId == request.MusicianId)
            throw new DomainException("Cannot review yourself");

        // Validar que existe un contrato completado
        if (request.ContractId.HasValue)
        {
            var contract = await _contractRepository.GetByIdAsync(request.ContractId.Value);
            if (contract == null)
                throw new NotFoundException("Contract not found");

            if (contract.Status != ContractStatus.Completed)
                throw new DomainException("Can only review completed contracts");

            if (contract.ClientId != request.UserId)
                throw new UnauthorizedException("Only contract participants can review");
        }

        // Verificar que no existe una review previa
        var existingReview = await _reviewRepository.GetByUserAndMusicianAsync(request.UserId, request.MusicianId);
        if (existingReview != null)
            throw new DomainException("Review already exists for this musician");

        // Analizar sentimiento del comentario
        var sentimentResult = await _sentimentService.AnalyzeSentimentAsync(request.Comment);

        // Crear la review
        var review = new Review
        {
            Id = Guid.NewGuid(),
            ReviewerId = request.UserId,
            ReviewedUserId = request.MusicianId,
            Rating = request.Rating,
            Comment = request.Comment,
            Category = request.Category,
            ContractId = request.ContractId,
            Status = ReviewStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            SentimentScore = sentimentResult.Score,
            SentimentLabel = sentimentResult.Label,
            Tags = request.Tags ?? new List<string>(),
            Metadata = new Dictionary<string, string>()
        };

        // Verificar la review
        var verificationResult = await _verificationService.VerifyReviewAsync(review);
        review.IsVerified = verificationResult.IsVerified;
        review.VerificationScore = verificationResult.Score;

        await _reviewRepository.AddAsync(review);

        // Actualizar el perfil del músico
        await UpdateMusicianRatingAsync(request.MusicianId);

        // Notificar al músico
        await _notificationService.SendNotificationAsync(
            request.MusicianId,
            NotificationType.ReviewReceived,
            new { ReviewId = review.Id, Rating = review.Rating, ReviewerName = reviewer.FullName });

        _logger.LogInformation("Review {ReviewId} created successfully", review.Id);

        return review;
    }

    public async Task<Review> UpdateReviewAsync(UpdateReviewRequest request)
    {
        var review = await _reviewRepository.GetByIdAsync(request.ReviewId);
        if (review == null)
            throw new NotFoundException("Review not found");

        if (review.ReviewerId != request.UserId)
            throw new UnauthorizedException("Only the reviewer can update the review");

        if (review.Status != ReviewStatus.Approved)
            throw new DomainException("Only approved reviews can be updated");

        // Analizar nuevo sentimiento
        var sentimentResult = await _sentimentService.AnalyzeSentimentAsync(request.Comment);

        // Actualizar la review
        review.Rating = request.Rating;
        review.Comment = request.Comment;
        review.Tags = request.Tags ?? review.Tags;
        review.SentimentScore = sentimentResult.Score;
        review.SentimentLabel = sentimentResult.Label;
        review.UpdatedAt = DateTime.UtcNow;

        // Re-verificar la review
        var verificationResult = await _verificationService.VerifyReviewAsync(review);
        review.IsVerified = verificationResult.IsVerified;
        review.VerificationScore = verificationResult.Score;

        await _reviewRepository.UpdateAsync(review);

        // Actualizar el perfil del músico
        await UpdateMusicianRatingAsync(review.ReviewedUserId);

        _logger.LogInformation("Review {ReviewId} updated successfully", review.Id);

        return review;
    }

    public async Task<bool> DeleteReviewAsync(Guid reviewId, Guid userId)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null)
            throw new NotFoundException("Review not found");

        if (review.ReviewerId != userId)
            throw new UnauthorizedException("Only the reviewer can delete the review");

        await _reviewRepository.DeleteAsync(review);

        // Actualizar el perfil del músico
        await UpdateMusicianRatingAsync(review.ReviewedUserId);

        _logger.LogInformation("Review {ReviewId} deleted by user {UserId}", reviewId, userId);

        return true;
    }

    public async Task<ReviewSummary> GetMusicianReviewSummaryAsync(Guid musicianId)
    {
        var reviews = await _reviewRepository.GetMusicianReviewsAsync(musicianId);
        var approvedReviews = reviews.Where(r => r.Status == ReviewStatus.Approved).ToList();

        if (!approvedReviews.Any())
        {
            return new ReviewSummary
            {
                MusicianId = musicianId,
                TotalReviews = 0,
                AverageRating = 0,
                RatingDistribution = new Dictionary<int, int>(),
                RecentReviews = new List<Review>(),
                TopTags = new List<string>(),
                SentimentAnalysis = new SentimentAnalysisResult()
            };
        }

        var summary = new ReviewSummary
        {
            MusicianId = musicianId,
            TotalReviews = approvedReviews.Count,
            AverageRating = approvedReviews.Average(r => r.Rating),
            RatingDistribution = approvedReviews
                .GroupBy(r => (int)r.Rating)
                .ToDictionary(g => g.Key, g => g.Count()),
            RecentReviews = approvedReviews
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToList(),
            TopTags = approvedReviews
                .SelectMany(r => r.Tags)
                .GroupBy(t => t)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => g.Key)
                .ToList(),
            SentimentAnalysis = new SentimentAnalysisResult
            {
                PositiveCount = approvedReviews.Count(r => r.SentimentLabel == "positive"),
                NegativeCount = approvedReviews.Count(r => r.SentimentLabel == "negative"),
                NeutralCount = approvedReviews.Count(r => r.SentimentLabel == "neutral"),
                AverageSentimentScore = approvedReviews.Average(r => r.SentimentScore)
            }
        };

        return summary;
    }

    public async Task<bool> ReportReviewAsync(ReportReviewRequest request)
    {
        var review = await _reviewRepository.GetByIdAsync(request.ReviewId);
        if (review == null)
            throw new NotFoundException("Review not found");

        var report = new ReviewReport
        {
            Id = Guid.NewGuid(),
            ReviewId = request.ReviewId,
            ReporterId = request.ReporterId,
            Reason = request.Reason,
            Description = request.Description,
            Status = ReportStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _reviewRepository.AddReportAsync(report);

        // Notificar a los moderadores
        await _notificationService.SendNotificationAsync(
            Guid.Empty, // Moderadores
            NotificationType.ReviewReported,
            new { ReviewId = request.ReviewId, Reason = request.Reason });

        _logger.LogInformation("Review {ReviewId} reported by user {UserId}", request.ReviewId, request.ReporterId);

        return true;
    }

    public async Task<Review> ModerateReviewAsync(ModerateReviewRequest request)
    {
        var review = await _reviewRepository.GetByIdAsync(request.ReviewId);
        if (review == null)
            throw new NotFoundException("Review not found");

        switch (request.Action)
        {
            case ModerationAction.Approve:
                review.Approve();
                break;
            case ModerationAction.Reject:
                review.Reject(request.Reason);
                break;
            case ModerationAction.Hide:
                review.Hide(request.Reason);
                break;
            default:
                throw new ArgumentException("Invalid moderation action");
        }

        review.ModeratedBy = request.ModeratorId;
        review.ModeratedAt = DateTime.UtcNow;
        review.ModerationReason = request.Reason;

        await _reviewRepository.UpdateAsync(review);

        // Actualizar el perfil del músico si se aprobó
        if (request.Action == ModerationAction.Approve)
        {
            await UpdateMusicianRatingAsync(review.ReviewedUserId);
        }

        _logger.LogInformation("Review {ReviewId} moderated with action {Action}", request.ReviewId, request.Action);

        return review;
    }

    private async Task UpdateMusicianRatingAsync(Guid musicianId)
    {
        var reviews = await _reviewRepository.GetMusicianReviewsAsync(musicianId);
        var approvedReviews = reviews.Where(r => r.Status == ReviewStatus.Approved).ToList();

        if (approvedReviews.Any())
        {
            var musician = await _musicianRepository.GetMusicianProfileAsync(musicianId);
            if (musician != null)
            {
                musician.UpdateRating(approvedReviews.Average(r => r.Rating));
                await _musicianRepository.UpdateAsync(musician);
            }
        }
    }

    public async Task<Review> GetReviewByIdAsync(Guid reviewId)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null)
            throw new NotFoundException("Review not found");

        return review;
    }

    public async Task<List<Review>> GetUserReviewsAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        return await _reviewRepository.GetUserReviewsAsync(userId, page, pageSize);
    }

    public async Task<List<Review>> GetMusicianReviewsAsync(Guid musicianId, int page = 1, int pageSize = 20)
    {
        return await _reviewRepository.GetMusicianReviewsAsync(musicianId, page, pageSize);
    }

    public async Task<List<Review>> GetContractReviewsAsync(Guid contractId)
    {
        return await _reviewRepository.GetContractReviewsAsync(contractId);
    }

    public async Task<List<Review>> GetReportedReviewsAsync(int page = 1, int pageSize = 20)
    {
        return await _reviewRepository.GetReportedReviewsAsync(page, pageSize);
    }
}
```

---

## 📊 **Entidades del Sistema de Reviews**

### **Review, ReviewReport y ReviewSummary**

```csharp
// MusicalMatching.Domain/Entities/Review.cs
namespace MusicalMatching.Domain.Entities;

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
    public Guid? ContractId { get; private set; }
    public virtual Contract? Contract { get; private set; }
    
    // Estado y moderación
    public ReviewStatus Status { get; private set; }
    public bool IsVerified { get; private set; }
    public double VerificationScore { get; private set; }
    public Guid? ModeratedBy { get; private set; }
    public virtual User? Moderator { get; private set; }
    public DateTime? ModeratedAt { get; private set; }
    public string? ModerationReason { get; private set; }
    
    // Análisis de sentimientos
    public double SentimentScore { get; private set; }
    public string SentimentLabel { get; private set; } = string.Empty;
    
    // Fechas
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    // Metadatos
    public Dictionary<string, string> Metadata { get; private set; } = new();

    private Review() { }

    public Review(
        Guid reviewerId, Guid reviewedUserId, double rating,
        string comment, ReviewCategory category, Guid? contractId = null)
    {
        ReviewerId = reviewerId;
        ReviewedUserId = reviewedUserId;
        Rating = rating;
        Comment = comment ?? throw new ArgumentNullException(nameof(comment));
        Category = category;
        ContractId = contractId;
        
        Status = ReviewStatus.Pending;
        IsVerified = false;
        VerificationScore = 0;
        CreatedAt = DateTime.UtcNow;
        
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
    }

    public void Reject(string reason)
    {
        if (Status != ReviewStatus.Pending)
            throw new DomainException("Only pending reviews can be rejected");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Rejection reason is required");

        Status = ReviewStatus.Rejected;
        ModerationReason = reason;
    }

    public void Hide(string reason)
    {
        if (Status != ReviewStatus.Approved)
            throw new DomainException("Only approved reviews can be hidden");

        Status = ReviewStatus.Hidden;
        ModerationReason = reason;
    }

    public void UpdateContent(double rating, string comment, List<string> tags)
    {
        if (Status != ReviewStatus.Approved)
            throw new DomainException("Only approved reviews can be updated");

        Rating = rating;
        Comment = comment ?? throw new ArgumentNullException(nameof(comment));
        Tags = tags ?? new List<string>();
        UpdatedAt = DateTime.UtcNow;
        
        ValidateReview();
    }

    public void SetSentimentAnalysis(double score, string label)
    {
        SentimentScore = score;
        SentimentLabel = label ?? throw new ArgumentNullException(nameof(label));
    }

    public void SetVerification(bool isVerified, double score)
    {
        IsVerified = isVerified;
        VerificationScore = score;
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

    public void AddMetadata(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be empty");

        Metadata[key] = value;
    }

    public bool IsApproved => Status == ReviewStatus.Approved;
    public bool IsPending => Status == ReviewStatus.Pending;
    public bool IsRejected => Status == ReviewStatus.Rejected;
    public bool IsHidden => Status == ReviewStatus.Hidden;
    public bool IsPositive => SentimentLabel == "positive";
    public bool IsNegative => SentimentLabel == "negative";
    public bool IsNeutral => SentimentLabel == "neutral";
}

// MusicalMatching.Domain/Entities/ReviewReport.cs
public class ReviewReport : BaseEntity
{
    public Guid ReviewId { get; private set; }
    public virtual Review Review { get; private set; }
    public Guid ReporterId { get; private set; }
    public virtual User Reporter { get; private set; }
    
    // Información del reporte
    public ReportReason Reason { get; private set; }
    public string Description { get; private set; }
    public ReportStatus Status { get; private set; }
    
    // Moderación
    public Guid? ModeratedBy { get; private set; }
    public virtual User? Moderator { get; private set; }
    public DateTime? ModeratedAt { get; private set; }
    public string? ModerationNotes { get; private set; }
    
    // Fechas
    public DateTime CreatedAt { get; private set; }

    private ReviewReport() { }

    public ReviewReport(Guid reviewId, Guid reporterId, ReportReason reason, string description)
    {
        ReviewId = reviewId;
        ReporterId = reporterId;
        Reason = reason;
        Description = description ?? throw new ArgumentNullException(nameof(description));
        
        Status = ReportStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    // Métodos de dominio
    public void Resolve(Guid moderatorId, string notes)
    {
        if (Status != ReportStatus.Pending)
            throw new DomainException("Only pending reports can be resolved");

        Status = ReportStatus.Resolved;
        ModeratedBy = moderatorId;
        ModeratedAt = DateTime.UtcNow;
        ModerationNotes = notes;
    }

    public void Dismiss(Guid moderatorId, string notes)
    {
        if (Status != ReportStatus.Pending)
            throw new DomainException("Only pending reports can be dismissed");

        Status = ReportStatus.Dismissed;
        ModeratedBy = moderatorId;
        ModeratedAt = DateTime.UtcNow;
        ModerationNotes = notes;
    }

    public bool IsPending => Status == ReportStatus.Pending;
    public bool IsResolved => Status == ReportStatus.Resolved;
    public bool IsDismissed => Status == ReportStatus.Dismissed;
}

// MusicalMatching.Domain/ValueObjects/ReviewSummary.cs
public class ReviewSummary
{
    public Guid MusicianId { get; set; }
    public int TotalReviews { get; set; }
    public double AverageRating { get; set; }
    public Dictionary<int, int> RatingDistribution { get; set; } = new();
    public List<Review> RecentReviews { get; set; } = new();
    public List<string> TopTags { get; set; } = new();
    public SentimentAnalysisResult SentimentAnalysis { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class SentimentAnalysisResult
{
    public int PositiveCount { get; set; }
    public int NegativeCount { get; set; }
    public int NeutralCount { get; set; }
    public double AverageSentimentScore { get; set; }
    public string OverallSentiment => AverageSentimentScore > 0.1 ? "positive" : 
                                     AverageSentimentScore < -0.1 ? "negative" : "neutral";
}

// MusicalMatching.Domain/Enums/ReviewEnums.cs
public enum ReviewStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Hidden = 3
}

public enum ReviewCategory
{
    Performance = 0,
    Professionalism = 1,
    Communication = 2,
    Punctuality = 3,
    Overall = 4
}

public enum ReportReason
{
    Inappropriate = 0,
    Spam = 1,
    Fake = 2,
    Harassment = 3,
    Offensive = 4,
    Other = 5
}

public enum ReportStatus
{
    Pending = 0,
    Resolved = 1,
    Dismissed = 2
}

public enum ModerationAction
{
    Approve = 0,
    Reject = 1,
    Hide = 2
}
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Sistema de Reviews**
```csharp
// Implementa:
// - Creación y gestión de reviews
// - Sistema de moderación
// - Análisis de sentimientos
// - Verificación de reviews
```

### **Ejercicio 2: Analytics de Reputación**
```csharp
// Crea:
// - Cálculo de ratings promedio
// - Distribución de calificaciones
// - Análisis de tendencias
// - Reportes de reputación
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **⭐ Sistema de Reviews**: Gestión completa de calificaciones
2. **📊 Entidad Review**: Reviews con análisis de sentimientos
3. **🚨 Entidad ReviewReport**: Sistema de reportes y moderación
4. **📈 ReviewSummary**: Analytics y resúmenes de reputación
5. **🔍 Verificación**: Sistema de verificación de reviews

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Sistema de Analytics y Reportes**, implementando métricas y dashboards.

---

**¡Has completado la séptima clase del Módulo 16! ⭐🎯**
