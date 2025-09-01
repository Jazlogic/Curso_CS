# 🚀 Clase 6: Calidad del Código y Métricas

## 📋 Información de la Clase

- **Módulo**: Senior 1 - Arquitectura de Software Empresarial
- **Duración**: 2 horas
- **Nivel**: Senior
- **Prerrequisitos**: Completar Clase 5 (Arquitectura de Datos Avanzada)

## 🎯 Objetivos de Aprendizaje

- Implementar métricas de calidad del código
- Aplicar técnicas de refactoring avanzadas
- Identificar y eliminar code smells
- Implementar análisis estático de código

---

## 📚 Navegación del Módulo 6

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_arquitectura_limpia_avanzada.md) | Arquitectura Limpia Avanzada | |
| [Clase 2](clase_2_event_driven_architecture.md) | Event-Driven Architecture | |
| [Clase 3](clase_3_microservicios_avanzada.md) | Arquitectura de Microservicios Avanzada | |
| [Clase 4](clase_4_patrones_enterprise.md) | Patrones de Diseño Enterprise | |
| [Clase 5](clase_5_arquitectura_datos_avanzada.md) | Arquitectura de Datos Avanzada | ← Anterior |
| **Clase 6** | **Calidad del Código y Métricas** | ← Estás aquí |
| [Clase 7](clase_7_monitoreo_observabilidad.md) | Monitoreo y Observabilidad | Siguiente → |
| [Clase 8](clase_8_arquitectura_evolutiva.md) | Arquitectura Evolutiva | |
| [Clase 9](clase_9_seguridad_enterprise.md) | Arquitectura de Seguridad Enterprise | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Plataforma Empresarial | |

**← [Volver al README del Módulo 6](../senior_1/README.md)**

---

## 📚 Contenido Teórico

### 1. Calidad del Código y Métricas

La calidad del código es fundamental para mantener sistemas empresariales escalables y mantenibles.

```csharp
// ===== CALIDAD DEL CÓDIGO Y MÉTRICAS - IMPLEMENTACIÓN COMPLETA =====
namespace CodeQualityAndMetrics
{
    // ===== MÉTRICAS DE CÓDIGO =====
    namespace CodeMetrics
    {
        public interface ICodeMetricsAnalyzer
        {
            Task<CodeMetricsResult> AnalyzeAsync(string filePath);
            Task<CodeMetricsResult> AnalyzeDirectoryAsync(string directoryPath);
            Task<CodeMetricsReport> GenerateReportAsync(string projectPath);
        }
        
        public class CodeMetricsAnalyzer : ICodeMetricsAnalyzer
        {
            private readonly ILogger<CodeMetricsAnalyzer> _logger;
            
            public CodeMetricsAnalyzer(ILogger<CodeMetricsAnalyzer> logger)
            {
                _logger = logger;
            }
            
            public async Task<CodeMetricsResult> AnalyzeAsync(string filePath)
            {
                try
                {
                    var sourceCode = await File.ReadAllTextAsync(filePath);
                    var lines = sourceCode.Split('\n');
                    
                    var metrics = new CodeMetricsResult
                    {
                        FilePath = filePath,
                        TotalLines = lines.Length,
                        CodeLines = lines.Count(l => !string.IsNullOrWhiteSpace(l) && !l.TrimStart().StartsWith("//") && !l.TrimStart().StartsWith("/*")),
                        CommentLines = lines.Count(l => l.TrimStart().StartsWith("//") || l.TrimStart().StartsWith("/*")),
                        BlankLines = lines.Count(l => string.IsNullOrWhiteSpace(l)),
                        Complexity = CalculateCyclomaticComplexity(sourceCode),
                        MaintainabilityIndex = CalculateMaintainabilityIndex(sourceCode),
                        LinesOfCode = CalculateLinesOfCode(sourceCode)
                    };
                    
                    _logger.LogInformation("Analyzed file {FilePath}: {Complexity} complexity, {Maintainability} maintainability", 
                        filePath, metrics.Complexity, metrics.MaintainabilityIndex);
                    
                    return metrics;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error analyzing file {FilePath}", filePath);
                    throw;
                }
            }
            
            public async Task<CodeMetricsResult> AnalyzeDirectoryAsync(string directoryPath)
            {
                var files = Directory.GetFiles(directoryPath, "*.cs", SearchOption.AllDirectories);
                var results = new List<CodeMetricsResult>();
                
                foreach (var file in files)
                {
                    var result = await AnalyzeAsync(file);
                    results.Add(result);
                }
                
                return AggregateResults(results);
            }
            
            public async Task<CodeMetricsReport> GenerateReportAsync(string projectPath)
            {
                var directoryResult = await AnalyzeDirectoryAsync(projectPath);
                var files = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories);
                
                var report = new CodeMetricsReport
                {
                    ProjectPath = projectPath,
                    TotalFiles = files.Length,
                    OverallMetrics = directoryResult,
                    GeneratedAt = DateTime.UtcNow,
                    QualityScore = CalculateQualityScore(directoryResult)
                };
                
                _logger.LogInformation("Generated report for {ProjectPath}: Quality Score {QualityScore}", 
                    projectPath, report.QualityScore);
                
                return report;
            }
            
            private int CalculateCyclomaticComplexity(string sourceCode)
            {
                var complexity = 1; // Base complexity
                
                // Count decision points
                var decisionKeywords = new[] { "if", "else", "for", "foreach", "while", "do", "switch", "case", "catch", "&&", "||", "?" };
                
                foreach (var keyword in decisionKeywords)
                {
                    complexity += Regex.Matches(sourceCode, $@"\b{keyword}\b", RegexOptions.IgnoreCase).Count;
                }
                
                return complexity;
            }
            
            private double CalculateMaintainabilityIndex(string sourceCode)
            {
                var lines = sourceCode.Split('\n').Length;
                var complexity = CalculateCyclomaticComplexity(sourceCode);
                
                // Simplified Maintainability Index calculation
                var mi = 171 - 5.2 * Math.Log(complexity) - 0.23 * Math.Log(lines) - 16.2 * Math.Log(1);
                
                return Math.Max(0, Math.Min(100, mi));
            }
            
            private int CalculateLinesOfCode(string sourceCode)
            {
                var lines = sourceCode.Split('\n');
                return lines.Count(l => !string.IsNullOrWhiteSpace(l) && !l.TrimStart().StartsWith("//") && !l.TrimStart().StartsWith("/*"));
            }
            
            private CodeMetricsResult AggregateResults(List<CodeMetricsResult> results)
            {
                return new CodeMetricsResult
                {
                    TotalLines = results.Sum(r => r.TotalLines),
                    CodeLines = results.Sum(r => r.CodeLines),
                    CommentLines = results.Sum(r => r.CommentLines),
                    BlankLines = results.Sum(r => r.BlankLines),
                    Complexity = results.Sum(r => r.Complexity),
                    MaintainabilityIndex = results.Average(r => r.MaintainabilityIndex),
                    LinesOfCode = results.Sum(r => r.LinesOfCode)
                };
            }
            
            private double CalculateQualityScore(CodeMetricsResult metrics)
            {
                var complexityScore = Math.Max(0, 100 - metrics.Complexity * 2);
                var maintainabilityScore = metrics.MaintainabilityIndex;
                var commentRatio = metrics.TotalLines > 0 ? (double)metrics.CommentLines / metrics.TotalLines * 100 : 0;
                var commentScore = Math.Min(100, commentRatio * 2);
                
                return (complexityScore + maintainabilityScore + commentScore) / 3;
            }
        }
        
        public class CodeMetricsResult
        {
            public string FilePath { get; set; }
            public int TotalLines { get; set; }
            public int CodeLines { get; set; }
            public int CommentLines { get; set; }
            public int BlankLines { get; set; }
            public int Complexity { get; set; }
            public double MaintainabilityIndex { get; set; }
            public int LinesOfCode { get; set; }
        }
        
        public class CodeMetricsReport
        {
            public string ProjectPath { get; set; }
            public int TotalFiles { get; set; }
            public CodeMetricsResult OverallMetrics { get; set; }
            public DateTime GeneratedAt { get; set; }
            public double QualityScore { get; set; }
        }
    }
    
    // ===== CODE SMELLS DETECTOR =====
    namespace CodeSmells
    {
        public interface ICodeSmellDetector
        {
            Task<List<CodeSmell>> DetectAsync(string filePath);
            Task<List<CodeSmell>> DetectInDirectoryAsync(string directoryPath);
            Task<CodeSmellReport> GenerateReportAsync(string projectPath);
        }
        
        public class CodeSmellDetector : ICodeSmellDetector
        {
            private readonly ILogger<CodeSmellDetector> _logger;
            private readonly List<ICodeSmellRule> _rules;
            
            public CodeSmellDetector(ILogger<CodeSmellDetector> logger)
            {
                _logger = logger;
                _rules = InitializeRules();
            }
            
            public async Task<List<CodeSmell>> DetectAsync(string filePath)
            {
                try
                {
                    var sourceCode = await File.ReadAllTextAsync(filePath);
                    var smells = new List<CodeSmell>();
                    
                    foreach (var rule in _rules)
                    {
                        var ruleSmells = rule.Detect(sourceCode, filePath);
                        smells.AddRange(ruleSmells);
                    }
                    
                    _logger.LogInformation("Detected {SmellCount} code smells in {FilePath}", smells.Count, filePath);
                    
                    return smells;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error detecting code smells in {FilePath}", filePath);
                    throw;
                }
            }
            
            public async Task<List<CodeSmell>> DetectInDirectoryAsync(string directoryPath)
            {
                var files = Directory.GetFiles(directoryPath, "*.cs", SearchOption.AllDirectories);
                var allSmells = new List<CodeSmell>();
                
                foreach (var file in files)
                {
                    var smells = await DetectAsync(file);
                    allSmells.AddRange(smells);
                }
                
                return allSmells;
            }
            
            public async Task<CodeSmellReport> GenerateReportAsync(string projectPath)
            {
                var smells = await DetectInDirectoryAsync(projectPath);
                
                var report = new CodeSmellReport
                {
                    ProjectPath = projectPath,
                    TotalSmells = smells.Count,
                    SmellsByType = smells.GroupBy(s => s.Type).ToDictionary(g => g.Key, g => g.Count()),
                    SmellsBySeverity = smells.GroupBy(s => s.Severity).ToDictionary(g => g.Key, g => g.Count()),
                    Smells = smells,
                    GeneratedAt = DateTime.UtcNow
                };
                
                _logger.LogInformation("Generated code smell report for {ProjectPath}: {TotalSmells} smells found", 
                    projectPath, report.TotalSmells);
                
                return report;
            }
            
            private List<ICodeSmellRule> InitializeRules()
            {
                return new List<ICodeSmellRule>
                {
                    new LongMethodRule(),
                    new LongClassRule(),
                    new DuplicateCodeRule(),
                    new MagicNumberRule(),
                    new DeadCodeRule(),
                    new ComplexConditionRule(),
                    new LargeParameterListRule()
                };
            }
        }
        
        public interface ICodeSmellRule
        {
            List<CodeSmell> Detect(string sourceCode, string filePath);
        }
        
        public class LongMethodRule : ICodeSmellRule
        {
            public List<CodeSmell> Detect(string sourceCode, string filePath)
            {
                var smells = new List<CodeSmell>();
                var lines = sourceCode.Split('\n');
                var methodStart = -1;
                var braceCount = 0;
                
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    
                    if (Regex.IsMatch(line, @"^\s*(public|private|protected|internal)\s+\w+\s+\w+\s*\([^)]*\)\s*$"))
                    {
                        if (methodStart != -1)
                        {
                            var methodLength = i - methodStart;
                            if (methodLength > 20) // Threshold for long method
                            {
                                smells.Add(new CodeSmell
                                {
                                    Type = CodeSmellType.LongMethod,
                                    Severity = CodeSmellSeverity.Medium,
                                    LineNumber = methodStart + 1,
                                    Message = $"Method is {methodLength} lines long (threshold: 20)",
                                    FilePath = filePath
                                });
                            }
                        }
                        methodStart = i;
                        braceCount = 0;
                    }
                    
                    if (line.Contains("{")) braceCount++;
                    if (line.Contains("}")) braceCount--;
                    
                    if (braceCount == 0 && methodStart != -1)
                    {
                        var methodLength = i - methodStart;
                        if (methodLength > 20)
                        {
                            smells.Add(new CodeSmell
                            {
                                Type = CodeSmellType.LongMethod,
                                Severity = CodeSmellSeverity.Medium,
                                LineNumber = methodStart + 1,
                                Message = $"Method is {methodLength} lines long (threshold: 20)",
                                FilePath = filePath
                            });
                        }
                        methodStart = -1;
                    }
                }
                
                return smells;
            }
        }
        
        public class LongClassRule : ICodeSmellRule
        {
            public List<CodeSmell> Detect(string sourceCode, string filePath)
            {
                var smells = new List<CodeSmell>();
                var lines = sourceCode.Split('\n');
                var classStart = -1;
                var braceCount = 0;
                
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    
                    if (Regex.IsMatch(line, @"^\s*class\s+\w+"))
                    {
                        if (classStart != -1)
                        {
                            var classLength = i - classStart;
                            if (classLength > 500) // Threshold for long class
                            {
                                smells.Add(new CodeSmell
                                {
                                    Type = CodeSmellType.LongClass,
                                    Severity = CodeSmellSeverity.High,
                                    LineNumber = classStart + 1,
                                    Message = $"Class is {classLength} lines long (threshold: 500)",
                                    FilePath = filePath
                                });
                            }
                        }
                        classStart = i;
                        braceCount = 0;
                    }
                    
                    if (line.Contains("{")) braceCount++;
                    if (line.Contains("}")) braceCount--;
                    
                    if (braceCount == 0 && classStart != -1)
                    {
                        var classLength = i - classStart;
                        if (classLength > 500)
                        {
                            smells.Add(new CodeSmell
                            {
                                Type = CodeSmellType.LongClass,
                                Severity = CodeSmellSeverity.High,
                                LineNumber = classStart + 1,
                                Message = $"Class is {classLength} lines long (threshold: 500)",
                                FilePath = filePath
                            });
                        }
                        classStart = -1;
                    }
                }
                
                return smells;
            }
        }
        
        public class DuplicateCodeRule : ICodeSmellRule
        {
            public List<CodeSmell> Detect(string sourceCode, string filePath)
            {
                var smells = new List<CodeSmell>();
                var lines = sourceCode.Split('\n');
                
                // Simple duplicate detection (can be enhanced with more sophisticated algorithms)
                for (int i = 0; i < lines.Length - 3; i++)
                {
                    var sequence = string.Join("\n", lines.Skip(i).Take(3));
                    
                    for (int j = i + 3; j < lines.Length - 2; j++)
                    {
                        var compareSequence = string.Join("\n", lines.Skip(j).Take(3));
                        
                        if (sequence == compareSequence && !string.IsNullOrWhiteSpace(sequence))
                        {
                            smells.Add(new CodeSmell
                            {
                                Type = CodeSmellType.DuplicateCode,
                                Severity = CodeSmellSeverity.Medium,
                                LineNumber = i + 1,
                                Message = "Duplicate code detected (3+ lines)",
                                FilePath = filePath
                            });
                            break;
                        }
                    }
                }
                
                return smells;
            }
        }
        
        public class MagicNumberRule : ICodeSmellRule
        {
            public List<CodeSmell> Detect(string sourceCode, string filePath)
            {
                var smells = new List<CodeSmell>();
                var lines = sourceCode.Split('\n');
                
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    var matches = Regex.Matches(line, @"\b\d{2,}\b");
                    
                    foreach (Match match in matches)
                    {
                        var number = int.Parse(match.Value);
                        if (number > 10 && !IsCommonNumber(number))
                        {
                            smells.Add(new CodeSmell
                            {
                                Type = CodeSmellType.MagicNumber,
                                Severity = CodeSmellSeverity.Low,
                                LineNumber = i + 1,
                                Message = $"Magic number detected: {number}",
                                FilePath = filePath
                            });
                        }
                    }
                }
                
                return smells;
            }
            
            private bool IsCommonNumber(int number)
            {
                var commonNumbers = new[] { 100, 1000, 1024, 60, 24, 7, 365, 12 };
                return commonNumbers.Contains(number);
            }
        }
        
        public class DeadCodeRule : ICodeSmellRule
        {
            public List<CodeSmell> Detect(string sourceCode, string filePath)
            {
                var smells = new List<CodeSmell>();
                var lines = sourceCode.Split('\n');
                
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    
                    // Detect commented-out code
                    if (line.StartsWith("//") && Regex.IsMatch(line, @"\b(if|for|while|foreach|switch|try|catch|using)\b"))
                    {
                        smells.Add(new CodeSmell
                        {
                            Type = CodeSmellType.DeadCode,
                            Severity = CodeSmellSeverity.Low,
                            LineNumber = i + 1,
                            Message = "Commented-out code detected",
                            FilePath = filePath
                        });
                    }
                }
                
                return smells;
            }
        }
        
        public class ComplexConditionRule : ICodeSmellRule
        {
            public List<CodeSmell> Detect(string sourceCode, string filePath)
            {
                var smells = new List<CodeSmell>();
                var lines = sourceCode.Split('\n');
                
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    
                    // Count logical operators in conditions
                    var logicalOperators = Regex.Matches(line, @"\b(&&|\|\||and|or)\b").Count;
                    var comparisonOperators = Regex.Matches(line, @"[=!<>]=?|<=|>=").Count;
                    
                    if (logicalOperators > 2 || comparisonOperators > 3)
                    {
                        smells.Add(new CodeSmell
                        {
                            Type = CodeSmellType.ComplexCondition,
                            Severity = CodeSmellSeverity.Medium,
                            LineNumber = i + 1,
                            Message = "Complex condition detected",
                            FilePath = filePath
                        });
                    }
                }
                
                return smells;
            }
        }
        
        public class LargeParameterListRule : ICodeSmellRule
        {
            public List<CodeSmell> Detect(string sourceCode, string filePath)
            {
                var smells = new List<CodeSmell>();
                var lines = sourceCode.Split('\n');
                
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    
                    if (Regex.IsMatch(line, @"^\s*(public|private|protected|internal)\s+\w+\s+\w+\s*\([^)]*\)"))
                    {
                        var parameters = Regex.Match(line, @"\(([^)]*)\)").Groups[1].Value;
                        var parameterCount = parameters.Split(',').Length;
                        
                        if (parameterCount > 4)
                        {
                            smells.Add(new CodeSmell
                            {
                                Type = CodeSmellType.LargeParameterList,
                                Severity = CodeSmellSeverity.Medium,
                                LineNumber = i + 1,
                                Message = $"Method has {parameterCount} parameters (threshold: 4)",
                                FilePath = filePath
                            });
                        }
                    }
                }
                
                return smells;
            }
        }
        
        public class CodeSmell
        {
            public CodeSmellType Type { get; set; }
            public CodeSmellSeverity Severity { get; set; }
            public int LineNumber { get; set; }
            public string Message { get; set; }
            public string FilePath { get; set; }
        }
        
        public enum CodeSmellType
        {
            LongMethod,
            LongClass,
            DuplicateCode,
            MagicNumber,
            DeadCode,
            ComplexCondition,
            LargeParameterList
        }
        
        public enum CodeSmellSeverity
        {
            Low,
            Medium,
            High,
            Critical
        }
        
        public class CodeSmellReport
        {
            public string ProjectPath { get; set; }
            public int TotalSmells { get; set; }
            public Dictionary<CodeSmellType, int> SmellsByType { get; set; }
            public Dictionary<CodeSmellSeverity, int> SmellsBySeverity { get; set; }
            public List<CodeSmell> Smells { get; set; }
            public DateTime GeneratedAt { get; set; }
        }
    }
    
    // ===== REFACTORING TOOLS =====
    namespace RefactoringTools
    {
        public interface IRefactoringTool
        {
            Task<RefactoringResult> ExtractMethodAsync(string filePath, int startLine, int endLine, string methodName);
            Task<RefactoringResult> ExtractClassAsync(string filePath, string className, List<string> methodNames);
            Task<RefactoringResult> RenameVariableAsync(string filePath, string oldName, string newName);
            Task<RefactoringResult> RemoveUnusedCodeAsync(string filePath);
        }
        
        public class RefactoringTool : IRefactoringTool
        {
            private readonly ILogger<RefactoringTool> _logger;
            
            public RefactoringTool(ILogger<RefactoringTool> logger)
            {
                _logger = logger;
            }
            
            public async Task<RefactoringResult> ExtractMethodAsync(string filePath, int startLine, int endLine, string methodName)
            {
                try
                {
                    var sourceCode = await File.ReadAllTextAsync(filePath);
                    var lines = sourceCode.Split('\n');
                    
                    if (startLine < 1 || endLine > lines.Length || startLine > endLine)
                    {
                        return RefactoringResult.Failure("Invalid line range");
                    }
                    
                    // Extract the code to be moved to a new method
                    var extractedCode = string.Join("\n", lines.Skip(startLine - 1).Take(endLine - startLine + 1));
                    
                    // Create method signature
                    var methodSignature = $"private void {methodName}()\n{{\n{extractedCode}\n}}";
                    
                    // Replace extracted code with method call
                    var newLines = new List<string>(lines);
                    for (int i = startLine - 1; i < endLine; i++)
                    {
                        if (i == startLine - 1)
                        {
                            newLines[i] = $"{methodName}();";
                        }
                        else
                        {
                            newLines[i] = "";
                        }
                    }
                    
                    // Add method to the end of the class
                    var classEndIndex = FindClassEnd(newLines);
                    newLines.Insert(classEndIndex, methodSignature);
                    
                    var newSourceCode = string.Join("\n", newLines);
                    await File.WriteAllTextAsync(filePath, newSourceCode);
                    
                    _logger.LogInformation("Extracted method {MethodName} from {FilePath}", methodName, filePath);
                    
                    return RefactoringResult.Success($"Method {methodName} extracted successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error extracting method from {FilePath}", filePath);
                    return RefactoringResult.Failure($"Error extracting method: {ex.Message}");
                }
            }
            
            public async Task<RefactoringResult> ExtractClassAsync(string filePath, string className, List<string> methodNames)
            {
                try
                {
                    var sourceCode = await File.ReadAllTextAsync(filePath);
                    var lines = sourceCode.Split('\n');
                    
                    var extractedMethods = new List<string>();
                    var newLines = new List<string>(lines);
                    
                    foreach (var methodName in methodNames)
                    {
                        var methodStart = FindMethodStart(newLines, methodName);
                        var methodEnd = FindMethodEnd(newLines, methodStart);
                        
                        if (methodStart != -1 && methodEnd != -1)
                        {
                            var methodCode = string.Join("\n", newLines.Skip(methodStart).Take(methodEnd - methodStart + 1));
                            extractedMethods.Add(methodCode);
                            
                            // Remove method from original file
                            for (int i = methodStart; i <= methodEnd; i++)
                            {
                                newLines[i] = "";
                            }
                        }
                    }
                    
                    // Create new class file
                    var newClassName = $"{className}.cs";
                    var newClassContent = GenerateNewClass(className, extractedMethods);
                    await File.WriteAllTextAsync(newClassName, newClassContent);
                    
                    // Update original file
                    var updatedSourceCode = string.Join("\n", newLines.Where(l => !string.IsNullOrWhiteSpace(l)));
                    await File.WriteAllTextAsync(filePath, updatedSourceCode);
                    
                    _logger.LogInformation("Extracted class {ClassName} from {FilePath}", className, filePath);
                    
                    return RefactoringResult.Success($"Class {className} extracted successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error extracting class from {FilePath}", filePath);
                    return RefactoringResult.Failure($"Error extracting class: {ex.Message}");
                }
            }
            
            public async Task<RefactoringResult> RenameVariableAsync(string filePath, string oldName, string newName)
            {
                try
                {
                    var sourceCode = await File.ReadAllTextAsync(filePath);
                    var newSourceCode = Regex.Replace(sourceCode, $@"\b{oldName}\b", newName);
                    
                    await File.WriteAllTextAsync(filePath, newSourceCode);
                    
                    _logger.LogInformation("Renamed variable {OldName} to {NewName} in {FilePath}", oldName, newName, filePath);
                    
                    return RefactoringResult.Success($"Variable renamed from {oldName} to {newName}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error renaming variable in {FilePath}", filePath);
                    return RefactoringResult.Failure($"Error renaming variable: {ex.Message}");
                }
            }
            
            public async Task<RefactoringResult> RemoveUnusedCodeAsync(string filePath)
            {
                try
                {
                    var sourceCode = await File.ReadAllTextAsync(filePath);
                    var lines = sourceCode.Split('\n');
                    var newLines = new List<string>();
                    var inCommentBlock = false;
                    
                    foreach (var line in lines)
                    {
                        var trimmedLine = line.Trim();
                        
                        // Skip commented-out code
                        if (trimmedLine.StartsWith("/*"))
                        {
                            inCommentBlock = true;
                            continue;
                        }
                        
                        if (trimmedLine.EndsWith("*/"))
                        {
                            inCommentBlock = false;
                            continue;
                        }
                        
                        if (inCommentBlock)
                        {
                            continue;
                        }
                        
                        // Skip empty lines and simple comments
                        if (!string.IsNullOrWhiteSpace(trimmedLine) && !trimmedLine.StartsWith("//"))
                        {
                            newLines.Add(line);
                        }
                    }
                    
                    var newSourceCode = string.Join("\n", newLines);
                    await File.WriteAllTextAsync(filePath, newSourceCode);
                    
                    _logger.LogInformation("Removed unused code from {FilePath}", filePath);
                    
                    return RefactoringResult.Success("Unused code removed successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error removing unused code from {FilePath}", filePath);
                    return RefactoringResult.Failure($"Error removing unused code: {ex.Message}");
                }
            }
            
            private int FindClassEnd(List<string> lines)
            {
                var braceCount = 0;
                var inClass = false;
                
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    
                    if (line.StartsWith("class "))
                    {
                        inClass = true;
                    }
                    
                    if (inClass)
                    {
                        if (line.Contains("{")) braceCount++;
                        if (line.Contains("}")) braceCount--;
                        
                        if (braceCount == 0 && inClass)
                        {
                            return i;
                        }
                    }
                }
                
                return lines.Count - 1;
            }
            
            private int FindMethodStart(List<string> lines, string methodName)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (Regex.IsMatch(line, $@"^\s*\w+\s+\w+\s*{methodName}\s*\("))
                    {
                        return i;
                    }
                }
                
                return -1;
            }
            
            private int FindMethodEnd(List<string> lines, int startLine)
            {
                var braceCount = 0;
                var inMethod = false;
                
                for (int i = startLine; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    
                    if (line.Contains("{"))
                    {
                        if (!inMethod) inMethod = true;
                        braceCount++;
                    }
                    
                    if (line.Contains("}"))
                    {
                        braceCount--;
                        if (braceCount == 0 && inMethod)
                        {
                            return i;
                        }
                    }
                }
                
                return -1;
            }
            
            private string GenerateNewClass(string className, List<string> methods)
            {
                var classContent = $@"using System;

public class {className}
{{
{string.Join("\n\n", methods)}
}}";
                
                return classContent;
            }
        }
        
        public class RefactoringResult
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
            public string Details { get; set; }
            
            public static RefactoringResult Success(string message, string details = null)
            {
                return new RefactoringResult
                {
                    IsSuccess = true,
                    Message = message,
                    Details = details
                };
            }
            
            public static RefactoringResult Failure(string message, string details = null)
            {
                return new RefactoringResult
                {
                    IsSuccess = false,
                    Message = message,
                    Details = details
                };
            }
        }
    }
    
    // ===== DEPENDENCY INJECTION =====
    namespace Infrastructure.DependencyInjection
    {
        public static class ServiceCollectionExtensions
        {
            public static IServiceCollection AddCodeQualityTools(this IServiceCollection services)
            {
                // Code Metrics
                services.AddScoped<ICodeMetricsAnalyzer, CodeMetricsAnalyzer>();
                
                // Code Smells
                services.AddScoped<ICodeSmellDetector, CodeSmellDetector>();
                
                // Refactoring Tools
                services.AddScoped<IRefactoringTool, RefactoringTool>();
                
                return services;
            }
        }
    }
}

// Uso de Calidad del Código y Métricas
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Calidad del Código y Métricas ===\n");
        
        Console.WriteLine("Los componentes implementados incluyen:");
        Console.WriteLine("1. Analizador de métricas de código");
        Console.WriteLine("2. Detector de code smells");
        Console.WriteLine("3. Herramientas de refactoring");
        Console.WriteLine("4. Reglas de detección automática");
        Console.WriteLine("5. Generación de reportes");
        Console.WriteLine("6. Análisis de complejidad ciclomática");
        
        Console.WriteLine("\nBeneficios de estas herramientas:");
        Console.WriteLine("- Detección temprana de problemas de calidad");
        Console.WriteLine("- Métricas objetivas del código");
        Console.WriteLine("- Refactoring automatizado");
        Console.WriteLine("- Mejora continua de la calidad");
        Console.WriteLine("- Cumplimiento de estándares");
        Console.WriteLine("- Reducción de deuda técnica");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Implementar Métricas Personalizadas
Crea métricas específicas para tu dominio de negocio.

### Ejercicio 2: Reglas de Code Smells
Implementa reglas personalizadas para detectar patrones específicos.

### Ejercicio 3: Herramientas de Refactoring
Crea herramientas de refactoring para patrones comunes.

## 🔍 Puntos Clave

1. **Métricas de código** proporcionan mediciones objetivas de calidad
2. **Code smells** indican problemas potenciales en el código
3. **Herramientas de refactoring** automatizan mejoras del código
4. **Análisis estático** detecta problemas sin ejecutar el código
5. **Reportes de calidad** facilitan la toma de decisiones

## 📚 Recursos Adicionales

- [Code Metrics](https://docs.microsoft.com/en-us/visualstudio/code-quality/code-metrics-values)
- [Code Smells](https://martinfowler.com/bliki/CodeSmell.html)
- [Refactoring](https://refactoring.com/)

---

**🎯 ¡Has completado la Clase 6! Ahora comprendes Calidad del Código y Métricas**

**📚 [Siguiente: Clase 7 - Monitoreo y Observabilidad](clase_7_monitoreo_observabilidad.md)**
