# 📋 **Clase 7: Security Compliance y Auditing**

## 🎯 **Objetivo de la Clase**
Dominar los aspectos de cumplimiento de seguridad, auditoría de sistemas y implementación de controles de seguridad para cumplir con regulaciones y estándares.

## 📚 **Contenido de la Clase**

### **1. Security Compliance Framework**

#### **1.1 Compliance Management System**
```csharp
// Sistema de gestión de cumplimiento
public class ComplianceManagementService
{
    private readonly ILogger<ComplianceManagementService> _logger;
    private readonly IComplianceRepository _complianceRepository;
    private readonly IAuditService _auditService;
    
    public ComplianceManagementService(
        ILogger<ComplianceManagementService> logger,
        IComplianceRepository complianceRepository,
        IAuditService auditService)
    {
        _logger = logger;
        _complianceRepository = complianceRepository;
        _auditService = auditService;
    }
    
    // Evaluar cumplimiento
    public async Task<ComplianceAssessment> AssessComplianceAsync(string framework, string version)
    {
        try
        {
            var assessment = new ComplianceAssessment
            {
                Framework = framework,
                Version = version,
                AssessmentDate = DateTime.UtcNow,
                Controls = new List<ComplianceControl>()
            };
            
            // Obtener controles del framework
            var frameworkControls = await GetFrameworkControlsAsync(framework, version);
            
            foreach (var control in frameworkControls)
            {
                var controlAssessment = await AssessControlAsync(control);
                assessment.Controls.Add(controlAssessment);
            }
            
            // Calcular puntuación de cumplimiento
            assessment.ComplianceScore = CalculateComplianceScore(assessment.Controls);
            assessment.Status = DetermineComplianceStatus(assessment.ComplianceScore);
            
            // Almacenar evaluación
            await _complianceRepository.StoreAssessmentAsync(assessment);
            
            _logger.LogInformation("Compliance assessment completed: {Framework} - Score: {Score}", 
                framework, assessment.ComplianceScore);
            
            return assessment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assessing compliance for framework: {Framework}", framework);
            throw;
        }
    }
    
    // Evaluar control individual
    private async Task<ComplianceControl> AssessControlAsync(FrameworkControl frameworkControl)
    {
        var control = new ComplianceControl
        {
            Id = frameworkControl.Id,
            Name = frameworkControl.Name,
            Description = frameworkControl.Description,
            Category = frameworkControl.Category,
            Evidence = new List<ComplianceEvidence>()
        };
        
        // Evaluar implementación del control
        var implementation = await EvaluateControlImplementationAsync(frameworkControl);
        control.Implementation = implementation;
        
        // Determinar estado de cumplimiento
        control.Status = DetermineControlStatus(implementation);
        
        // Recopilar evidencia
        control.Evidence = await CollectEvidenceAsync(frameworkControl);
        
        return control;
    }
    
    // Evaluar implementación del control
    private async Task<ControlImplementation> EvaluateControlImplementationAsync(FrameworkControl frameworkControl)
    {
        var implementation = new ControlImplementation
        {
            ControlId = frameworkControl.Id,
            Implemented = false,
            PartiallyImplemented = false,
            NotImplemented = false,
            NotApplicable = false,
            Notes = string.Empty
        };
        
        // Evaluar según el tipo de control
        switch (frameworkControl.Type)
        {
            case ControlType.Technical:
                implementation = await EvaluateTechnicalControlAsync(frameworkControl);
                break;
            case ControlType.Administrative:
                implementation = await EvaluateAdministrativeControlAsync(frameworkControl);
                break;
            case ControlType.Physical:
                implementation = await EvaluatePhysicalControlAsync(frameworkControl);
                break;
        }
        
        return implementation;
    }
    
    // Evaluar control técnico
    private async Task<ControlImplementation> EvaluateTechnicalControlAsync(FrameworkControl frameworkControl)
    {
        var implementation = new ControlImplementation
        {
            ControlId = frameworkControl.Id,
            Implemented = false,
            PartiallyImplemented = false,
            NotImplemented = false,
            NotApplicable = false,
            Notes = string.Empty
        };
        
        // Implementar lógica de evaluación específica para controles técnicos
        // Esto podría incluir verificación de configuraciones, políticas, etc.
        
        return implementation;
    }
    
    // Evaluar control administrativo
    private async Task<ControlImplementation> EvaluateAdministrativeControlAsync(FrameworkControl frameworkControl)
    {
        var implementation = new ControlImplementation
        {
            ControlId = frameworkControl.Id,
            Implemented = false,
            PartiallyImplemented = false,
            NotImplemented = false,
            NotApplicable = false,
            Notes = string.Empty
        };
        
        // Implementar lógica de evaluación específica para controles administrativos
        // Esto podría incluir verificación de políticas, procedimientos, etc.
        
        return implementation;
    }
    
    // Evaluar control físico
    private async Task<ControlImplementation> EvaluatePhysicalControlAsync(FrameworkControl frameworkControl)
    {
        var implementation = new ControlImplementation
        {
            ControlId = frameworkControl.Id,
            Implemented = false,
            PartiallyImplemented = false,
            NotImplemented = false,
            NotApplicable = false,
            Notes = string.Empty
        };
        
        // Implementar lógica de evaluación específica para controles físicos
        // Esto podría incluir verificación de acceso físico, etc.
        
        return implementation;
    }
    
    // Recopilar evidencia
    private async Task<List<ComplianceEvidence>> CollectEvidenceAsync(FrameworkControl frameworkControl)
    {
        var evidence = new List<ComplianceEvidence>();
        
        // Implementar lógica para recopilar evidencia
        // Esto podría incluir documentos, capturas de pantalla, logs, etc.
        
        return evidence;
    }
    
    // Calcular puntuación de cumplimiento
    private double CalculateComplianceScore(List<ComplianceControl> controls)
    {
        if (controls.Count == 0) return 0;
        
        var implementedControls = controls.Count(c => c.Status == ComplianceStatus.Implemented);
        var partiallyImplementedControls = controls.Count(c => c.Status == ComplianceStatus.PartiallyImplemented);
        
        var score = (implementedControls + (partiallyImplementedControls * 0.5)) / controls.Count * 100;
        
        return Math.Round(score, 2);
    }
    
    // Determinar estado de cumplimiento
    private ComplianceStatus DetermineComplianceStatus(double score)
    {
        if (score >= 90) return ComplianceStatus.Implemented;
        if (score >= 70) return ComplianceStatus.PartiallyImplemented;
        if (score >= 50) return ComplianceStatus.NotImplemented;
        return ComplianceStatus.NotApplicable;
    }
    
    // Obtener controles del framework
    private async Task<List<FrameworkControl>> GetFrameworkControlsAsync(string framework, string version)
    {
        // Implementar lógica para obtener controles del framework
        // Esto podría incluir consulta a base de datos, API externa, etc.
        
        return new List<FrameworkControl>(); // Simplificado
    }
}

// Modelos para gestión de cumplimiento
public class ComplianceAssessment
{
    public string Id { get; set; }
    public string Framework { get; set; }
    public string Version { get; set; }
    public DateTime AssessmentDate { get; set; }
    public List<ComplianceControl> Controls { get; set; }
    public double ComplianceScore { get; set; }
    public ComplianceStatus Status { get; set; }
}

public class ComplianceControl
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public ControlImplementation Implementation { get; set; }
    public ComplianceStatus Status { get; set; }
    public List<ComplianceEvidence> Evidence { get; set; }
}

public class ControlImplementation
{
    public string ControlId { get; set; }
    public bool Implemented { get; set; }
    public bool PartiallyImplemented { get; set; }
    public bool NotImplemented { get; set; }
    public bool NotApplicable { get; set; }
    public string Notes { get; set; }
}

public class ComplianceEvidence
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public string FilePath { get; set; }
    public DateTime CollectedAt { get; set; }
    public string CollectedBy { get; set; }
}

public class FrameworkControl
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public ControlType Type { get; set; }
}

public enum ComplianceStatus
{
    Implemented,
    PartiallyImplemented,
    NotImplemented,
    NotApplicable
}

public enum ControlType
{
    Technical,
    Administrative,
    Physical
}
```

#### **1.2 Regulatory Compliance**
```csharp
// Servicio de cumplimiento regulatorio
public class RegulatoryComplianceService
{
    private readonly ILogger<RegulatoryComplianceService> _logger;
    private readonly IComplianceRepository _complianceRepository;
    
    public RegulatoryComplianceService(
        ILogger<RegulatoryComplianceService> logger,
        IComplianceRepository complianceRepository)
    {
        _logger = logger;
        _complianceRepository = complianceRepository;
    }
    
    // Evaluar cumplimiento GDPR
    public async Task<GdprComplianceResult> AssessGdprComplianceAsync()
    {
        try
        {
            var result = new GdprComplianceResult
            {
                AssessmentDate = DateTime.UtcNow,
                Requirements = new List<GdprRequirement>()
            };
            
            // Evaluar cada requisito de GDPR
            var requirements = GetGdprRequirements();
            
            foreach (var requirement in requirements)
            {
                var assessment = await AssessGdprRequirementAsync(requirement);
                result.Requirements.Add(assessment);
            }
            
            // Calcular puntuación de cumplimiento
            result.ComplianceScore = CalculateGdprComplianceScore(result.Requirements);
            result.Status = DetermineGdprComplianceStatus(result.ComplianceScore);
            
            _logger.LogInformation("GDPR compliance assessment completed: Score: {Score}", result.ComplianceScore);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assessing GDPR compliance");
            throw;
        }
    }
    
    // Evaluar requisito de GDPR
    private async Task<GdprRequirement> AssessGdprRequirementAsync(GdprRequirement requirement)
    {
        var assessment = new GdprRequirement
        {
            Id = requirement.Id,
            Name = requirement.Name,
            Description = requirement.Description,
            Category = requirement.Category,
            Implemented = false,
            Evidence = new List<ComplianceEvidence>()
        };
        
        // Evaluar implementación del requisito
        assessment.Implemented = await EvaluateGdprRequirementImplementationAsync(requirement);
        
        // Recopilar evidencia
        assessment.Evidence = await CollectGdprEvidenceAsync(requirement);
        
        return assessment;
    }
    
    // Evaluar implementación del requisito de GDPR
    private async Task<bool> EvaluateGdprRequirementImplementationAsync(GdprRequirement requirement)
    {
        // Implementar lógica de evaluación específica para cada requisito de GDPR
        // Esto podría incluir verificación de políticas, procedimientos, etc.
        
        return true; // Simplificado
    }
    
    // Recopilar evidencia de GDPR
    private async Task<List<ComplianceEvidence>> CollectGdprEvidenceAsync(GdprRequirement requirement)
    {
        var evidence = new List<ComplianceEvidence>();
        
        // Implementar lógica para recopilar evidencia específica de GDPR
        // Esto podría incluir documentos, capturas de pantalla, logs, etc.
        
        return evidence;
    }
    
    // Calcular puntuación de cumplimiento de GDPR
    private double CalculateGdprComplianceScore(List<GdprRequirement> requirements)
    {
        if (requirements.Count == 0) return 0;
        
        var implementedRequirements = requirements.Count(r => r.Implemented);
        var score = (double)implementedRequirements / requirements.Count * 100;
        
        return Math.Round(score, 2);
    }
    
    // Determinar estado de cumplimiento de GDPR
    private ComplianceStatus DetermineGdprComplianceStatus(double score)
    {
        if (score >= 90) return ComplianceStatus.Implemented;
        if (score >= 70) return ComplianceStatus.PartiallyImplemented;
        if (score >= 50) return ComplianceStatus.NotImplemented;
        return ComplianceStatus.NotApplicable;
    }
    
    // Obtener requisitos de GDPR
    private List<GdprRequirement> GetGdprRequirements()
    {
        return new List<GdprRequirement>
        {
            new GdprRequirement
            {
                Id = "GDPR-001",
                Name = "Data Protection by Design and by Default",
                Description = "Implement data protection principles in the design and operation of systems",
                Category = "Technical"
            },
            new GdprRequirement
            {
                Id = "GDPR-002",
                Name = "Lawfulness of Processing",
                Description = "Ensure that personal data is processed lawfully",
                Category = "Legal"
            },
            new GdprRequirement
            {
                Id = "GDPR-003",
                Name = "Purpose Limitation",
                Description = "Process personal data only for specified, explicit and legitimate purposes",
                Category = "Legal"
            },
            new GdprRequirement
            {
                Id = "GDPR-004",
                Name = "Data Minimisation",
                Description = "Process only the personal data that is necessary for the purpose",
                Category = "Technical"
            },
            new GdprRequirement
            {
                Id = "GDPR-005",
                Name = "Accuracy",
                Description = "Keep personal data accurate and up to date",
                Category = "Technical"
            },
            new GdprRequirement
            {
                Id = "GDPR-006",
                Name = "Storage Limitation",
                Description = "Keep personal data only for as long as necessary",
                Category = "Technical"
            },
            new GdprRequirement
            {
                Id = "GDPR-007",
                Name = "Integrity and Confidentiality",
                Description = "Ensure the security of personal data",
                Category = "Technical"
            },
            new GdprRequirement
            {
                Id = "GDPR-008",
                Name = "Accountability",
                Description = "Demonstrate compliance with GDPR principles",
                Category = "Administrative"
            }
        };
    }
}

// Modelos para cumplimiento regulatorio
public class GdprComplianceResult
{
    public DateTime AssessmentDate { get; set; }
    public List<GdprRequirement> Requirements { get; set; }
    public double ComplianceScore { get; set; }
    public ComplianceStatus Status { get; set; }
}

public class GdprRequirement
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public bool Implemented { get; set; }
    public List<ComplianceEvidence> Evidence { get; set; }
}
```

### **2. Security Auditing**

#### **2.1 Audit Service**
```csharp
// Servicio de auditoría de seguridad
public class SecurityAuditService
{
    private readonly ILogger<SecurityAuditService> _logger;
    private readonly IAuditRepository _auditRepository;
    private readonly IComplianceRepository _complianceRepository;
    
    public SecurityAuditService(
        ILogger<SecurityAuditService> logger,
        IAuditRepository auditRepository,
        IComplianceRepository complianceRepository)
    {
        _logger = logger;
        _auditRepository = auditRepository;
        _complianceRepository = complianceRepository;
    }
    
    // Realizar auditoría de seguridad
    public async Task<SecurityAudit> ConductSecurityAuditAsync(string auditType, string scope)
    {
        try
        {
            var audit = new SecurityAudit
            {
                Id = Guid.NewGuid().ToString(),
                Type = auditType,
                Scope = scope,
                StartDate = DateTime.UtcNow,
                Findings = new List<AuditFinding>(),
                Recommendations = new List<AuditRecommendation>()
            };
            
            // Ejecutar auditoría según el tipo
            switch (auditType)
            {
                case "Compliance":
                    await ConductComplianceAuditAsync(audit);
                    break;
                case "Technical":
                    await ConductTechnicalAuditAsync(audit);
                    break;
                case "Administrative":
                    await ConductAdministrativeAuditAsync(audit);
                    break;
                case "Physical":
                    await ConductPhysicalAuditAsync(audit);
                    break;
            }
            
            // Finalizar auditoría
            audit.EndDate = DateTime.UtcNow;
            audit.Duration = audit.EndDate - audit.StartDate;
            audit.Status = AuditStatus.Completed;
            
            // Almacenar auditoría
            await _auditRepository.StoreAuditAsync(audit);
            
            _logger.LogInformation("Security audit completed: {AuditType} - {FindingsCount} findings", 
                auditType, audit.Findings.Count);
            
            return audit;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error conducting security audit: {AuditType}", auditType);
            throw;
        }
    }
    
    // Realizar auditoría de cumplimiento
    private async Task ConductComplianceAuditAsync(SecurityAudit audit)
    {
        // Implementar lógica de auditoría de cumplimiento
        // Esto podría incluir verificación de políticas, procedimientos, etc.
    }
    
    // Realizar auditoría técnica
    private async Task ConductTechnicalAuditAsync(SecurityAudit audit)
    {
        // Implementar lógica de auditoría técnica
        // Esto podría incluir verificación de configuraciones, sistemas, etc.
    }
    
    // Realizar auditoría administrativa
    private async Task ConductAdministrativeAuditAsync(SecurityAudit audit)
    {
        // Implementar lógica de auditoría administrativa
        // Esto podría incluir verificación de procesos, roles, etc.
    }
    
    // Realizar auditoría física
    private async Task ConductPhysicalAuditAsync(SecurityAudit audit)
    {
        // Implementar lógica de auditoría física
        // Esto podría incluir verificación de acceso físico, etc.
    }
    
    // Generar reporte de auditoría
    public async Task<string> GenerateAuditReportAsync(string auditId)
    {
        try
        {
            var audit = await _auditRepository.GetAuditAsync(auditId);
            if (audit == null)
            {
                throw new AuditNotFoundException($"Audit {auditId} not found");
            }
            
            var report = new AuditReport
            {
                Audit = audit,
                ExecutiveSummary = GenerateExecutiveSummary(audit),
                DetailedFindings = GenerateDetailedFindings(audit),
                Recommendations = GenerateRecommendations(audit),
                ComplianceStatus = GenerateComplianceStatus(audit)
            };
            
            var reportJson = JsonSerializer.Serialize(report, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            _logger.LogInformation("Audit report generated: {AuditId}", auditId);
            
            return reportJson;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating audit report: {AuditId}", auditId);
            throw;
        }
    }
    
    // Generar resumen ejecutivo
    private string GenerateExecutiveSummary(SecurityAudit audit)
    {
        var totalFindings = audit.Findings.Count;
        var criticalFindings = audit.Findings.Count(f => f.Severity == AuditSeverity.Critical);
        var highFindings = audit.Findings.Count(f => f.Severity == AuditSeverity.High);
        var mediumFindings = audit.Findings.Count(f => f.Severity == AuditSeverity.Medium);
        var lowFindings = audit.Findings.Count(f => f.Severity == AuditSeverity.Low);
        
        return $"Security audit completed for {audit.Scope}. " +
               $"Total findings: {totalFindings}. " +
               $"Critical: {criticalFindings}, High: {highFindings}, " +
               $"Medium: {mediumFindings}, Low: {lowFindings}. " +
               "Immediate action required for critical and high severity findings.";
    }
    
    // Generar hallazgos detallados
    private List<DetailedFinding> GenerateDetailedFindings(SecurityAudit audit)
    {
        return audit.Findings.Select(f => new DetailedFinding
        {
            Id = f.Id,
            Title = f.Title,
            Description = f.Description,
            Severity = f.Severity,
            Category = f.Category,
            Evidence = f.Evidence,
            Impact = f.Impact,
            Likelihood = f.Likelihood,
            RiskScore = f.RiskScore
        }).ToList();
    }
    
    // Generar recomendaciones
    private List<DetailedRecommendation> GenerateRecommendations(SecurityAudit audit)
    {
        return audit.Recommendations.Select(r => new DetailedRecommendation
        {
            Id = r.Id,
            Title = r.Title,
            Description = r.Description,
            Priority = r.Priority,
            Category = r.Category,
            ImplementationEffort = r.ImplementationEffort,
            EstimatedCost = r.EstimatedCost,
            Timeline = r.Timeline
        }).ToList();
    }
    
    // Generar estado de cumplimiento
    private ComplianceStatus GenerateComplianceStatus(SecurityAudit audit)
    {
        var totalFindings = audit.Findings.Count;
        var criticalFindings = audit.Findings.Count(f => f.Severity == AuditSeverity.Critical);
        var highFindings = audit.Findings.Count(f => f.Severity == AuditSeverity.High);
        
        if (criticalFindings > 0 || highFindings > 2)
        {
            return ComplianceStatus.NotImplemented;
        }
        
        if (highFindings > 0 || totalFindings > 10)
        {
            return ComplianceStatus.PartiallyImplemented;
        }
        
        return ComplianceStatus.Implemented;
    }
}

// Modelos para auditoría de seguridad
public class SecurityAudit
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string Scope { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan Duration { get; set; }
    public AuditStatus Status { get; set; }
    public List<AuditFinding> Findings { get; set; }
    public List<AuditRecommendation> Recommendations { get; set; }
}

public class AuditFinding
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public AuditSeverity Severity { get; set; }
    public string Category { get; set; }
    public string Evidence { get; set; }
    public string Impact { get; set; }
    public string Likelihood { get; set; }
    public int RiskScore { get; set; }
}

public class AuditRecommendation
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Priority { get; set; }
    public string Category { get; set; }
    public string ImplementationEffort { get; set; }
    public string EstimatedCost { get; set; }
    public string Timeline { get; set; }
}

public class AuditReport
{
    public SecurityAudit Audit { get; set; }
    public string ExecutiveSummary { get; set; }
    public List<DetailedFinding> DetailedFindings { get; set; }
    public List<DetailedRecommendation> Recommendations { get; set; }
    public ComplianceStatus ComplianceStatus { get; set; }
}

public class DetailedFinding
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public AuditSeverity Severity { get; set; }
    public string Category { get; set; }
    public string Evidence { get; set; }
    public string Impact { get; set; }
    public string Likelihood { get; set; }
    public int RiskScore { get; set; }
}

public class DetailedRecommendation
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Priority { get; set; }
    public string Category { get; set; }
    public string ImplementationEffort { get; set; }
    public string EstimatedCost { get; set; }
    public string Timeline { get; set; }
}

public enum AuditStatus
{
    Planned,
    InProgress,
    Completed,
    Cancelled
}

public enum AuditSeverity
{
    Low,
    Medium,
    High,
    Critical
}
```

## 🎯 **Ejercicios Prácticos**

### **Ejercicio 1: Implementar Evaluación de Cumplimiento**
```csharp
// Crear una evaluación de cumplimiento personalizada
public class CustomComplianceAssessment
{
    public async Task<ComplianceAssessment> AssessCustomComplianceAsync(string framework)
    {
        // Implementar evaluación de cumplimiento personalizada
        // 1. Definir controles del framework
        // 2. Evaluar implementación de cada control
        // 3. Calcular puntuación de cumplimiento
    }
}
```

### **Ejercicio 2: Implementar Auditoría Personalizada**
```csharp
// Crear una auditoría personalizada
public class CustomSecurityAudit
{
    public async Task<SecurityAudit> ConductCustomAuditAsync(string scope)
    {
        // Implementar auditoría personalizada
        // 1. Definir alcance de la auditoría
        // 2. Ejecutar pruebas de auditoría
        // 3. Generar hallazgos y recomendaciones
    }
}
```

## 📝 **Resumen de la Clase**

### **Conceptos Clave Aprendidos:**
1. **Compliance Management**: Gestión de cumplimiento de seguridad
2. **Regulatory Compliance**: Cumplimiento de regulaciones
3. **Security Auditing**: Auditoría de seguridad
4. **Audit Reporting**: Generación de reportes de auditoría
5. **Compliance Assessment**: Evaluación de cumplimiento
6. **Audit Findings**: Hallazgos de auditoría

### **Próxima Clase:**
En la siguiente clase exploraremos **Security Testing y Quality Assurance**, incluyendo pruebas de seguridad y aseguramiento de calidad.

---

## 🔗 **Recursos Adicionales**

- [ISO 27001](https://www.iso.org/isoiec-27001-information-security.html)
- [SOC 2](https://www.aicpa.org/interestareas/frc/assuranceadvisoryservices/aicpasoc2report)
- [PCI DSS](https://www.pcisecuritystandards.org/)
- [HIPAA](https://www.hhs.gov/hipaa/index.html)
- [GDPR](https://gdpr.eu/)
