# 💳 Clase 6: Sistema de Pagos y Facturación

## 🧭 Navegación del Módulo

- **⬅️ Anterior**: [Clase 5: Sistema de Notificaciones y Alertas](../senior_9/clase_5_sistema_notificaciones_alertas.md)
- **🏠 Inicio del Módulo**: [Módulo 16: Maestría Total y Liderazgo Técnico](../senior_9/README.md)
- **➡️ Siguiente**: [Clase 7: Sistema de Reviews y Calificaciones](../senior_9/clase_7_sistema_reviews_calificaciones.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)** | **[🧭 Navegación Rápida](../../NAVEGACION_RAPIDA.md)**

---

## 🎯 **Objetivos de la Clase**

1. **Implementar** sistema de procesamiento de pagos
2. **Crear** gestión de facturas y recibos
3. **Configurar** múltiples métodos de pago
4. **Implementar** sistema de comisiones
5. **Configurar** reportes financieros

---

## 💳 **Sistema de Procesamiento de Pagos**

### **Servicio de Pagos**

```csharp
// MusicalMatching.Application/Services/IPaymentService.cs
namespace MusicalMatching.Application.Services;

public interface IPaymentService
{
    Task<PaymentResult> ProcessPaymentAsync(ProcessPaymentRequest request);
    Task<PaymentResult> ProcessRefundAsync(ProcessRefundRequest request);
    Task<PaymentResult> ProcessPartialRefundAsync(ProcessPartialRefundRequest request);
    Task<PaymentResult> ProcessChargebackAsync(ProcessChargebackRequest request);
    Task<PaymentResult> ProcessDisputeAsync(ProcessDisputeRequest request);
    Task<List<Payment>> GetUserPaymentsAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<Payment> GetPaymentByIdAsync(Guid paymentId);
    Task<List<Payment>> GetPaymentsByContractAsync(Guid contractId);
    Task<PaymentSummary> GetPaymentSummaryAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null);
}

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IContractRepository _contractRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPaymentGatewayService _gatewayService;
    private readonly ICommissionService _commissionService;
    private readonly IInvoiceService _invoiceService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IContractRepository contractRepository,
        IUserRepository userRepository,
        IPaymentGatewayService gatewayService,
        ICommissionService commissionService,
        IInvoiceService invoiceService,
        INotificationService notificationService,
        ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _contractRepository = contractRepository;
        _userRepository = userRepository;
        _gatewayService = gatewayService;
        _commissionService = commissionService;
        _invoiceService = invoiceService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(ProcessPaymentRequest request)
    {
        _logger.LogInformation("Processing payment for contract {ContractId}", request.ContractId);

        try
        {
            // Validar el contrato
            var contract = await _contractRepository.GetByIdAsync(request.ContractId);
            if (contract == null)
                throw new NotFoundException("Contract not found");

            if (contract.Status != ContractStatus.Active)
                throw new DomainException("Contract must be active to process payment");

            // Validar el monto
            if (request.Amount <= 0)
                throw new DomainException("Payment amount must be positive");

            if (request.Amount > contract.AgreedRate)
                throw new DomainException("Payment amount cannot exceed agreed rate");

            // Crear el pago
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                ContractId = request.ContractId,
                MusicianId = contract.MusicianId,
                ClientId = contract.ClientId,
                Amount = request.Amount,
                Currency = request.Currency,
                PaymentMethod = request.PaymentMethod,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                PaymentGateway = request.PaymentGateway,
                GatewayTransactionId = null,
                Metadata = request.Metadata ?? new Dictionary<string, string>()
            };

            await _paymentRepository.AddAsync(payment);

            // Procesar el pago con la pasarela
            var gatewayResult = await _gatewayService.ProcessPaymentAsync(new GatewayPaymentRequest
            {
                Amount = request.Amount,
                Currency = request.Currency,
                PaymentMethod = request.PaymentMethod,
                CustomerId = request.ClientId.ToString(),
                Description = $"Payment for contract {contract.Id}",
                Metadata = new Dictionary<string, string>
                {
                    ["contract_id"] = contract.Id.ToString(),
                    ["musician_id"] = contract.MusicianId.ToString(),
                    ["client_id"] = contract.ClientId.ToString()
                }
            });

            // Actualizar el pago con el resultado
            payment.GatewayTransactionId = gatewayResult.TransactionId;
            payment.Status = gatewayResult.Success ? PaymentStatus.Completed : PaymentStatus.Failed;
            payment.ProcessedAt = DateTime.UtcNow;
            payment.GatewayResponse = gatewayResult.Response;

            if (gatewayResult.Success)
            {
                // Calcular y procesar comisiones
                var commission = await _commissionService.CalculateCommissionAsync(payment);
                payment.CommissionAmount = commission.Amount;
                payment.NetAmount = payment.Amount - commission.Amount;

                // Crear factura
                var invoice = await _invoiceService.CreateInvoiceAsync(payment);
                payment.InvoiceId = invoice.Id;

                // Notificar a las partes
                await _notificationService.SendNotificationAsync(
                    contract.MusicianId, 
                    NotificationType.PaymentReceived, 
                    new { PaymentId = payment.Id, Amount = payment.Amount });

                await _notificationService.SendNotificationAsync(
                    contract.ClientId, 
                    NotificationType.PaymentProcessed, 
                    new { PaymentId = payment.Id, Amount = payment.Amount });

                _logger.LogInformation("Payment {PaymentId} processed successfully", payment.Id);
            }
            else
            {
                _logger.LogWarning("Payment {PaymentId} failed: {Error}", payment.Id, gatewayResult.ErrorMessage);
            }

            await _paymentRepository.UpdateAsync(payment);

            return new PaymentResult
            {
                Success = gatewayResult.Success,
                PaymentId = payment.Id,
                TransactionId = gatewayResult.TransactionId,
                Amount = payment.Amount,
                Status = payment.Status,
                ErrorMessage = gatewayResult.ErrorMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for contract {ContractId}", request.ContractId);
            throw;
        }
    }

    public async Task<PaymentResult> ProcessRefundAsync(ProcessRefundRequest request)
    {
        _logger.LogInformation("Processing refund for payment {PaymentId}", request.PaymentId);

        var payment = await _paymentRepository.GetByIdAsync(request.PaymentId);
        if (payment == null)
            throw new NotFoundException("Payment not found");

        if (payment.Status != PaymentStatus.Completed)
            throw new DomainException("Only completed payments can be refunded");

        if (request.Amount > payment.Amount)
            throw new DomainException("Refund amount cannot exceed original payment amount");

        // Procesar reembolso con la pasarela
        var gatewayResult = await _gatewayService.ProcessRefundAsync(new GatewayRefundRequest
        {
            TransactionId = payment.GatewayTransactionId,
            Amount = request.Amount,
            Reason = request.Reason
        });

        if (gatewayResult.Success)
        {
            // Crear registro de reembolso
            var refund = new Refund
            {
                Id = Guid.NewGuid(),
                PaymentId = payment.Id,
                Amount = request.Amount,
                Reason = request.Reason,
                Status = RefundStatus.Completed,
                GatewayTransactionId = gatewayResult.TransactionId,
                ProcessedAt = DateTime.UtcNow,
                ProcessedBy = request.ProcessedBy
            };

            await _paymentRepository.AddRefundAsync(refund);

            // Actualizar el pago
            payment.RefundedAmount += request.Amount;
            if (payment.RefundedAmount >= payment.Amount)
            {
                payment.Status = PaymentStatus.Refunded;
            }
            else
            {
                payment.Status = PaymentStatus.PartiallyRefunded;
            }

            await _paymentRepository.UpdateAsync(payment);

            // Notificar a las partes
            await _notificationService.SendNotificationAsync(
                payment.MusicianId, 
                NotificationType.PaymentRefunded, 
                new { PaymentId = payment.Id, RefundAmount = request.Amount });

            await _notificationService.SendNotificationAsync(
                payment.ClientId, 
                NotificationType.RefundProcessed, 
                new { PaymentId = payment.Id, RefundAmount = request.Amount });
        }

        return new PaymentResult
        {
            Success = gatewayResult.Success,
            PaymentId = payment.Id,
            TransactionId = gatewayResult.TransactionId,
            Amount = request.Amount,
            Status = payment.Status,
            ErrorMessage = gatewayResult.ErrorMessage
        };
    }

    public async Task<PaymentSummary> GetPaymentSummaryAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var from = fromDate ?? DateTime.UtcNow.AddMonths(-1);
        var to = toDate ?? DateTime.UtcNow;

        var payments = await _paymentRepository.GetUserPaymentsAsync(userId, from, to);
        
        var summary = new PaymentSummary
        {
            UserId = userId,
            Period = new DateRange(from, to),
            TotalPayments = payments.Count,
            TotalAmount = payments.Sum(p => p.Amount),
            TotalCommission = payments.Sum(p => p.CommissionAmount),
            TotalNetAmount = payments.Sum(p => p.NetAmount),
            TotalRefunded = payments.Sum(p => p.RefundedAmount),
            CompletedPayments = payments.Count(p => p.Status == PaymentStatus.Completed),
            FailedPayments = payments.Count(p => p.Status == PaymentStatus.Failed),
            PendingPayments = payments.Count(p => p.Status == PaymentStatus.Pending),
            RefundedPayments = payments.Count(p => p.Status == PaymentStatus.Refunded),
            AveragePaymentAmount = payments.Any() ? payments.Average(p => p.Amount) : 0,
            PaymentMethods = payments.GroupBy(p => p.PaymentMethod)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return summary;
    }

    public async Task<List<Payment>> GetUserPaymentsAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        return await _paymentRepository.GetUserPaymentsAsync(userId, page, pageSize);
    }

    public async Task<Payment> GetPaymentByIdAsync(Guid paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null)
            throw new NotFoundException("Payment not found");

        return payment;
    }

    public async Task<List<Payment>> GetPaymentsByContractAsync(Guid contractId)
    {
        return await _paymentRepository.GetPaymentsByContractAsync(contractId);
    }

    public async Task<PaymentResult> ProcessPartialRefundAsync(ProcessPartialRefundRequest request)
    {
        // Implementación similar a ProcessRefundAsync pero con validaciones específicas para reembolsos parciales
        return await ProcessRefundAsync(new ProcessRefundRequest
        {
            PaymentId = request.PaymentId,
            Amount = request.Amount,
            Reason = request.Reason,
            ProcessedBy = request.ProcessedBy
        });
    }

    public async Task<PaymentResult> ProcessChargebackAsync(ProcessChargebackRequest request)
    {
        _logger.LogInformation("Processing chargeback for payment {PaymentId}", request.PaymentId);

        var payment = await _paymentRepository.GetByIdAsync(request.PaymentId);
        if (payment == null)
            throw new NotFoundException("Payment not found");

        // Crear registro de chargeback
        var chargeback = new Chargeback
        {
            Id = Guid.NewGuid(),
            PaymentId = payment.Id,
            Amount = request.Amount,
            Reason = request.Reason,
            Status = ChargebackStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            GatewayChargebackId = request.GatewayChargebackId
        };

        await _paymentRepository.AddChargebackAsync(chargeback);

        // Actualizar el pago
        payment.Status = PaymentStatus.Chargeback;
        await _paymentRepository.UpdateAsync(payment);

        // Notificar a las partes
        await _notificationService.SendNotificationAsync(
            payment.MusicianId, 
            NotificationType.PaymentChargeback, 
            new { PaymentId = payment.Id, Amount = request.Amount });

        return new PaymentResult
        {
            Success = true,
            PaymentId = payment.Id,
            Amount = request.Amount,
            Status = payment.Status
        };
    }

    public async Task<PaymentResult> ProcessDisputeAsync(ProcessDisputeRequest request)
    {
        // Implementación similar a ProcessChargebackAsync pero para disputas
        return await ProcessChargebackAsync(new ProcessChargebackRequest
        {
            PaymentId = request.PaymentId,
            Amount = request.Amount,
            Reason = request.Reason,
            GatewayChargebackId = request.GatewayDisputeId
        });
    }
}
```

---

## 📄 **Entidades del Sistema de Pagos**

### **Payment, Invoice y Commission**

```csharp
// MusicalMatching.Domain/Entities/Payment.cs
namespace MusicalMatching.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid ContractId { get; private set; }
    public virtual Contract Contract { get; private set; }
    public Guid MusicianId { get; private set; }
    public virtual User Musician { get; private set; }
    public Guid ClientId { get; private set; }
    public virtual User Client { get; private set; }
    
    // Información del pago
    public decimal Amount { get; private set; }
    public decimal NetAmount { get; private set; }
    public decimal CommissionAmount { get; private set; }
    public decimal RefundedAmount { get; private set; }
    public string Currency { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public PaymentStatus Status { get; private set; }
    
    // Información de la pasarela
    public PaymentGateway PaymentGateway { get; private set; }
    public string? GatewayTransactionId { get; private set; }
    public string? GatewayResponse { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    
    // Facturación
    public Guid? InvoiceId { get; private set; }
    public virtual Invoice? Invoice { get; private set; }
    
    // Metadatos
    public Dictionary<string, string> Metadata { get; private set; } = new();
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Payment() { }

    public Payment(
        Guid contractId, Guid musicianId, Guid clientId,
        decimal amount, string currency, PaymentMethod paymentMethod,
        PaymentGateway paymentGateway)
    {
        ContractId = contractId;
        MusicianId = musicianId;
        ClientId = clientId;
        Amount = amount;
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
        PaymentMethod = paymentMethod;
        PaymentGateway = paymentGateway;
        
        NetAmount = amount;
        CommissionAmount = 0;
        RefundedAmount = 0;
        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    // Métodos de dominio
    public void MarkAsCompleted(string gatewayTransactionId, string gatewayResponse)
    {
        if (Status != PaymentStatus.Pending)
            throw new DomainException("Only pending payments can be marked as completed");

        Status = PaymentStatus.Completed;
        GatewayTransactionId = gatewayTransactionId;
        GatewayResponse = gatewayResponse;
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string gatewayResponse)
    {
        if (Status != PaymentStatus.Pending)
            throw new DomainException("Only pending payments can be marked as failed");

        Status = PaymentStatus.Failed;
        GatewayResponse = gatewayResponse;
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCommission(decimal commissionAmount)
    {
        if (commissionAmount < 0)
            throw new DomainException("Commission amount cannot be negative");

        CommissionAmount = commissionAmount;
        NetAmount = Amount - commissionAmount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddRefund(decimal refundAmount)
    {
        if (refundAmount <= 0)
            throw new DomainException("Refund amount must be positive");

        if (RefundedAmount + refundAmount > Amount)
            throw new DomainException("Total refund amount cannot exceed original payment amount");

        RefundedAmount += refundAmount;
        
        if (RefundedAmount >= Amount)
        {
            Status = PaymentStatus.Refunded;
        }
        else
        {
            Status = PaymentStatus.PartiallyRefunded;
        }
        
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetInvoice(Guid invoiceId)
    {
        InvoiceId = invoiceId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddMetadata(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be empty");

        Metadata[key] = value;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsCompleted => Status == PaymentStatus.Completed;
    public bool IsFailed => Status == PaymentStatus.Failed;
    public bool IsRefunded => Status == PaymentStatus.Refunded || Status == PaymentStatus.PartiallyRefunded;
    public bool IsChargeback => Status == PaymentStatus.Chargeback;
    public decimal RemainingAmount => Amount - RefundedAmount;
}

// MusicalMatching.Domain/Entities/Invoice.cs
public class Invoice : BaseEntity
{
    public Guid PaymentId { get; private set; }
    public virtual Payment Payment { get; private set; }
    public Guid MusicianId { get; private set; }
    public virtual User Musician { get; private set; }
    public Guid ClientId { get; private set; }
    public virtual User Client { get; private set; }
    
    // Información de la factura
    public string InvoiceNumber { get; private set; }
    public decimal Amount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; }
    public InvoiceStatus Status { get; private set; }
    
    // Fechas
    public DateTime IssueDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime? PaidDate { get; private set; }
    
    // Información fiscal
    public string? MusicianTaxId { get; private set; }
    public string? ClientTaxId { get; private set; }
    public string? MusicianAddress { get; private set; }
    public string? ClientAddress { get; private set; }
    
    // Archivos
    public string? PdfUrl { get; private set; }
    public string? XmlUrl { get; private set; }

    private Invoice() { }

    public Invoice(
        Guid paymentId, Guid musicianId, Guid clientId,
        decimal amount, string currency, string invoiceNumber)
    {
        PaymentId = paymentId;
        MusicianId = musicianId;
        ClientId = clientId;
        Amount = amount;
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
        InvoiceNumber = invoiceNumber ?? throw new ArgumentNullException(nameof(invoiceNumber));
        
        TaxAmount = 0; // Se calcula según la jurisdicción
        TotalAmount = amount + TaxAmount;
        Status = InvoiceStatus.Draft;
        IssueDate = DateTime.UtcNow;
        DueDate = DateTime.UtcNow.AddDays(30);
    }

    // Métodos de dominio
    public void SetTaxAmount(decimal taxAmount)
    {
        if (taxAmount < 0)
            throw new DomainException("Tax amount cannot be negative");

        TaxAmount = taxAmount;
        TotalAmount = Amount + taxAmount;
    }

    public void MarkAsSent()
    {
        if (Status != InvoiceStatus.Draft)
            throw new DomainException("Only draft invoices can be marked as sent");

        Status = InvoiceStatus.Sent;
    }

    public void MarkAsPaid()
    {
        if (Status != InvoiceStatus.Sent)
            throw new DomainException("Only sent invoices can be marked as paid");

        Status = InvoiceStatus.Paid;
        PaidDate = DateTime.UtcNow;
    }

    public void MarkAsOverdue()
    {
        if (Status != InvoiceStatus.Sent)
            throw new DomainException("Only sent invoices can be marked as overdue");

        Status = InvoiceStatus.Overdue;
    }

    public void SetPdfUrl(string url)
    {
        PdfUrl = url;
    }

    public void SetXmlUrl(string url)
    {
        XmlUrl = url;
    }

    public void SetMusicianTaxInfo(string taxId, string address)
    {
        MusicianTaxId = taxId;
        MusicianAddress = address;
    }

    public void SetClientTaxInfo(string taxId, string address)
    {
        ClientTaxId = taxId;
        ClientAddress = address;
    }

    public bool IsOverdue => Status == InvoiceStatus.Sent && DateTime.UtcNow > DueDate;
    public bool IsPaid => Status == InvoiceStatus.Paid;
    public bool IsDraft => Status == InvoiceStatus.Draft;
}

// MusicalMatching.Domain/Entities/Commission.cs
public class Commission : BaseEntity
{
    public Guid PaymentId { get; private set; }
    public virtual Payment Payment { get; private set; }
    public Guid MusicianId { get; private set; }
    public virtual User Musician { get; private set; }
    
    // Información de la comisión
    public decimal Amount { get; private set; }
    public decimal Percentage { get; private set; }
    public CommissionType Type { get; private set; }
    public CommissionStatus Status { get; private set; }
    
    // Fechas
    public DateTime CalculatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    
    // Metadatos
    public string? Description { get; private set; }
    public Dictionary<string, string> Metadata { get; private set; } = new();

    private Commission() { }

    public Commission(
        Guid paymentId, Guid musicianId, decimal amount, 
        decimal percentage, CommissionType type)
    {
        PaymentId = paymentId;
        MusicianId = musicianId;
        Amount = amount;
        Percentage = percentage;
        Type = type;
        
        Status = CommissionStatus.Pending;
        CalculatedAt = DateTime.UtcNow;
    }

    // Métodos de dominio
    public void MarkAsPaid()
    {
        if (Status != CommissionStatus.Pending)
            throw new DomainException("Only pending commissions can be marked as paid");

        Status = CommissionStatus.Paid;
        PaidAt = DateTime.UtcNow;
    }

    public void SetDescription(string description)
    {
        Description = description;
    }

    public void AddMetadata(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be empty");

        Metadata[key] = value;
    }

    public bool IsPaid => Status == CommissionStatus.Paid;
    public bool IsPending => Status == CommissionStatus.Pending;
}

// MusicalMatching.Domain/Enums/PaymentEnums.cs
public enum PaymentStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4,
    Refunded = 5,
    PartiallyRefunded = 6,
    Chargeback = 7,
    Disputed = 8
}

public enum PaymentMethod
{
    CreditCard = 0,
    DebitCard = 1,
    BankTransfer = 2,
    PayPal = 3,
    Stripe = 4,
    ApplePay = 5,
    GooglePay = 6,
    Cryptocurrency = 7
}

public enum PaymentGateway
{
    Stripe = 0,
    PayPal = 1,
    Square = 2,
    Razorpay = 3,
    MercadoPago = 4,
    Adyen = 5
}

public enum InvoiceStatus
{
    Draft = 0,
    Sent = 1,
    Paid = 2,
    Overdue = 3,
    Cancelled = 4
}

public enum CommissionType
{
    Platform = 0,
    Processing = 1,
    Service = 2
}

public enum CommissionStatus
{
    Pending = 0,
    Paid = 1,
    Waived = 2
}
```

---

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Sistema de Pagos**
```csharp
// Implementa:
// - Procesamiento de pagos
// - Manejo de reembolsos
// - Gestión de chargebacks
// - Integración con pasarelas
```

### **Ejercicio 2: Facturación y Comisiones**
```csharp
// Crea:
// - Generación de facturas
// - Cálculo de comisiones
// - Reportes financieros
// - Gestión de impuestos
```

---

## 📚 **Resumen de la Clase**

En esta clase hemos aprendido:

1. **💳 Sistema de Pagos**: Procesamiento completo de transacciones
2. **📄 Entidad Payment**: Gestión de pagos y estados
3. **🧾 Entidad Invoice**: Facturación automática
4. **💰 Entidad Commission**: Cálculo de comisiones
5. **🔄 Integración**: Pasarelas de pago y notificaciones

---

## 🚀 **Próximos Pasos**

En la siguiente clase aprenderemos sobre **Sistema de Reviews y Calificaciones**, implementando el sistema de evaluación de músicos.

---

**¡Has completado la sexta clase del Módulo 16! 💳🎯**
