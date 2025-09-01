# 🚀 Clase 7: Source Generators y Compile-time Code Generation

## 📋 Información de la Clase

- **Módulo**: Mid Level 2 - Arquitectura de Software y Patrones Avanzados
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 6 (Async Streams)

## 🎯 Objetivos de Aprendizaje

- Implementar Source Generators
- Generar código en tiempo de compilación
- Crear generadores de código personalizados
- Optimizar el rendimiento con metaprogramación

---

## 📚 Navegación del Módulo 5

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_arquitectura_hexagonal.md) | Arquitectura Hexagonal (Ports & Adapters) | |
| [Clase 2](clase_2_event_sourcing.md) | Event Sourcing y CQRS Avanzado | |
| [Clase 3](clase_3_microservicios.md) | Arquitectura de Microservicios | |
| [Clase 4](clase_4_patrones_arquitectonicos.md) | Patrones Arquitectónicos | |
| [Clase 5](clase_5_domain_driven_design.md) | Domain Driven Design (DDD) | |
| [Clase 6](clase_6_async_streams.md) | Async Streams y IAsyncEnumerable | ← Anterior |
| **Clase 7** | **Source Generators y Compile-time Code Generation** | ← Estás aquí |
| [Clase 8](clase_8_high_performance.md) | High Performance Programming | Siguiente → |
| [Clase 9](clase_9_seguridad_avanzada.md) | Seguridad Avanzada en .NET | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Gestión Empresarial | |

**← [Volver al README del Módulo 5](../midLevel_2/README.md)**

---

## 📚 Contenido Teórico

### 1. Source Generators

Los Source Generators permiten generar código C# durante la compilación, reduciendo la repetición de código y mejorando el rendimiento.

```csharp
// ===== SOURCE GENERATORS - IMPLEMENTACIÓN COMPLETA =====
namespace SourceGenerators
{
    // ===== GENERADOR DE DTOs AUTOMÁTICO =====
    [Generator]
    public class DtoGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // Registrar para sintaxis
            context.RegisterForSyntaxNotifications(() => new DtoSyntaxReceiver());
        }
        
        public void Execute(GeneratorExecutionContext context)
        {
            var receiver = (DtoSyntaxReceiver)context.SyntaxReceiver;
            
            foreach (var classDeclaration in receiver.CandidateClasses)
            {
                var dtoSource = GenerateDtoSource(classDeclaration);
                context.AddSource($"{classDeclaration.Identifier.ValueText}Dto.g.cs", dtoSource);
            }
        }
        
        private SourceText GenerateDtoSource(ClassDeclarationSyntax classDeclaration)
        {
            var className = classDeclaration.Identifier.ValueText;
            var properties = GetProperties(classDeclaration);
            
            var source = $@"
using System;

namespace GeneratedDtos
{{
    public class {className}Dto
    {{
{string.Join("\n", properties.Select(p => $"        public {p.Type} {p.Name} {{ get; set; }}"))}
        
        public {className}Dto()
        {{
        }}
        
        public {className}Dto({className} entity)
        {{
{string.Join("\n", properties.Select(p => $"            {p.Name} = entity.{p.Name};"))}
        }}
    }}
}}";
            
            return SourceText.From(source, Encoding.UTF8);
        }
        
        private List<PropertyInfo> GetProperties(ClassDeclarationSyntax classDeclaration)
        {
            var properties = new List<PropertyInfo>();
            
            foreach (var member in classDeclaration.Members)
            {
                if (member is PropertyDeclarationSyntax property)
                {
                    properties.Add(new PropertyInfo
                    {
                        Name = property.Identifier.ValueText,
                        Type = property.Type.ToString()
                    });
                }
            }
            
            return properties;
        }
    }
    
    public class DtoSyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = new();
        
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classDeclaration)
            {
                if (HasDtoAttribute(classDeclaration))
                {
                    CandidateClasses.Add(classDeclaration);
                }
            }
        }
        
        private bool HasDtoAttribute(ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(a => a.Name.ToString().Contains("Dto"));
        }
    }
    
    public class PropertyInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
    
    // ===== GENERADOR DE REPOSITORIES =====
    [Generator]
    public class RepositoryGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new RepositorySyntaxReceiver());
        }
        
        public void Execute(GeneratorExecutionContext context)
        {
            var receiver = (RepositorySyntaxReceiver)context.SyntaxReceiver;
            
            foreach (var entityClass in receiver.EntityClasses)
            {
                var repositorySource = GenerateRepositorySource(entityClass);
                context.AddSource($"I{entityClass.Name}Repository.g.cs", repositorySource);
                
                var implementationSource = GenerateRepositoryImplementation(entityClass);
                context.AddSource($"{entityClass.Name}Repository.g.cs", implementationSource);
            }
        }
        
        private SourceText GenerateRepositorySource(EntityInfo entity)
        {
            var source = $@"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeneratedRepositories
{{
    public interface I{entity.Name}Repository
    {{
        Task<{entity.Name}> GetByIdAsync(int id);
        Task<IEnumerable<{entity.Name}>> GetAllAsync();
        Task<{entity.Name}> AddAsync({entity.Name} entity);
        Task UpdateAsync({entity.Name} entity);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }}
}}";
            
            return SourceText.From(source, Encoding.UTF8);
        }
        
        private SourceText GenerateRepositoryImplementation(EntityInfo entity)
        {
            var source = $@"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GeneratedRepositories
{{
    public class {entity.Name}Repository : I{entity.Name}Repository
    {{
        private readonly ApplicationDbContext _context;
        
        public {entity.Name}Repository(ApplicationDbContext context)
        {{
            _context = context;
        }}
        
        public async Task<{entity.Name}> GetByIdAsync(int id)
        {{
            return await _context.Set<{entity.Name}>().FindAsync(id);
        }}
        
        public async Task<IEnumerable<{entity.Name}>> GetAllAsync()
        {{
            return await _context.Set<{entity.Name}>().ToListAsync();
        }}
        
        public async Task<{entity.Name}> AddAsync({entity.Name} entity)
        {{
            var result = await _context.Set<{entity.Name}>().AddAsync(entity);
            await _context.SaveChangesAsync();
            return result.Entity;
        }}
        
        public async Task UpdateAsync({entity.Name} entity)
        {{
            _context.Set<{entity.Name}>().Update(entity);
            await _context.SaveChangesAsync();
        }}
        
        public async Task DeleteAsync(int id)
        {{
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {{
                _context.Set<{entity.Name}>().Remove(entity);
                await _context.SaveChangesAsync();
            }}
        }}
        
        public async Task<bool> ExistsAsync(int id)
        {{
            return await _context.Set<{entity.Name}>().AnyAsync(e => e.Id == id);
        }}
    }}
}}";
            
            return SourceText.From(source, Encoding.UTF8);
        }
    }
    
    public class RepositorySyntaxReceiver : ISyntaxReceiver
    {
        public List<EntityInfo> EntityClasses { get; } = new();
        
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classDeclaration)
            {
                if (IsEntity(classDeclaration))
                {
                    EntityClasses.Add(new EntityInfo
                    {
                        Name = classDeclaration.Identifier.ValueText,
                        Namespace = GetNamespace(classDeclaration)
                    });
                }
            }
        }
        
        private bool IsEntity(ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(a => a.Name.ToString().Contains("Entity"));
        }
        
        private string GetNamespace(ClassDeclarationSyntax classDeclaration)
        {
            var namespaceDeclaration = classDeclaration.Parent as NamespaceDeclarationSyntax;
            return namespaceDeclaration?.Name.ToString() ?? "Default";
        }
    }
    
    public class EntityInfo
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
    }
    
    // ===== GENERADOR DE VALIDACIONES =====
    [Generator]
    public class ValidationGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ValidationSyntaxReceiver());
        }
        
        public void Execute(GeneratorExecutionContext context)
        {
            var receiver = (ValidationSyntaxReceiver)context.SyntaxReceiver;
            
            foreach (var modelClass in receiver.ModelClasses)
            {
                var validatorSource = GenerateValidatorSource(modelClass);
                context.AddSource($"{modelClass.Name}Validator.g.cs", validatorSource);
            }
        }
        
        private SourceText GenerateValidatorSource(ModelInfo model)
        {
            var source = $@"
using FluentValidation;

namespace GeneratedValidators
{{
    public class {model.Name}Validator : AbstractValidator<{model.Name}>
    {{
        public {model.Name}Validator()
        {{
{string.Join("\n", model.Properties.Select(p => GenerateValidationRule(p)))}
        }}
    }}
}}";
            
            return SourceText.From(source, Encoding.UTF8);
        }
        
        private string GenerateValidationRule(PropertyInfo property)
        {
            var rules = new List<string>();
            
            if (property.Type == "string")
            {
                rules.Add($"RuleFor(x => x.{property.Name}).NotEmpty().WithMessage(\"{property.Name} is required\")");
                rules.Add($"RuleFor(x => x.{property.Name}).MaximumLength(255).WithMessage(\"{property.Name} cannot exceed 255 characters\")");
            }
            else if (property.Type == "int" || property.Type == "decimal")
            {
                rules.Add($"RuleFor(x => x.{property.Name}).GreaterThan(0).WithMessage(\"{property.Name} must be greater than 0\")");
            }
            else if (property.Type == "DateTime")
            {
                rules.Add($"RuleFor(x => x.{property.Name}).NotEmpty().WithMessage(\"{property.Name} is required\")");
            }
            
            return string.Join("\n            ", rules);
        }
    }
    
    public class ValidationSyntaxReceiver : ISyntaxReceiver
    {
        public List<ModelInfo> ModelClasses { get; } = new();
        
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classDeclaration)
            {
                if (IsModel(classDeclaration))
                {
                    ModelClasses.Add(new ModelInfo
                    {
                        Name = classDeclaration.Identifier.ValueText,
                        Properties = GetProperties(classDeclaration)
                    });
                }
            }
        }
        
        private bool IsModel(ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(a => a.Name.ToString().Contains("Model") || a.Name.ToString().Contains("Request"));
        }
        
        private List<PropertyInfo> GetProperties(ClassDeclarationSyntax classDeclaration)
        {
            var properties = new List<PropertyInfo>();
            
            foreach (var member in classDeclaration.Members)
            {
                if (member is PropertyDeclarationSyntax property)
                {
                    properties.Add(new PropertyInfo
                    {
                        Name = property.Identifier.ValueText,
                        Type = property.Type.ToString()
                    });
                }
            }
            
            return properties;
        }
    }
    
    public class ModelInfo
    {
        public string Name { get; set; }
        public List<PropertyInfo> Properties { get; set; } = new();
    }
    
    // ===== GENERADOR DE MAPPERS =====
    [Generator]
    public class MapperGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new MapperSyntaxReceiver());
        }
        
        public void Execute(GeneratorExecutionContext context)
        {
            var receiver = (MapperSyntaxReceiver)context.SyntaxReceiver;
            
            foreach (var mapping in receiver.Mappings)
            {
                var mapperSource = GenerateMapperSource(mapping);
                context.AddSource($"{mapping.SourceName}To{mapping.TargetName}Mapper.g.cs", mapperSource);
            }
        }
        
        private SourceText GenerateMapperSource(MappingInfo mapping)
        {
            var source = $@"
using System;

namespace GeneratedMappers
{{
    public static class {mapping.SourceName}To{mapping.TargetName}Mapper
    {{
        public static {mapping.TargetName} Map({mapping.SourceName} source)
        {{
            if (source == null)
                return null;
                
            return new {mapping.TargetName}
            {{
{string.Join(",\n", mapping.PropertyMappings.Select(pm => $"                {pm.TargetProperty} = source.{pm.SourceProperty}"))}
            }};
        }}
        
        public static {mapping.SourceName} MapBack({mapping.TargetName} target)
        {{
            if (target == null)
                return null;
                
            return new {mapping.SourceName}
            {{
{string.Join(",\n", mapping.PropertyMappings.Select(pm => $"                {pm.SourceProperty} = target.{pm.TargetProperty}"))}
            }};
        }}
    }}
}}";
            
            return SourceText.From(source, Encoding.UTF8);
        }
    }
    
    public class MapperSyntaxReceiver : ISyntaxReceiver
    {
        public List<MappingInfo> Mappings { get; } = new();
        
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classDeclaration)
            {
                var mappingAttribute = GetMappingAttribute(classDeclaration);
                if (mappingAttribute != null)
                {
                    Mappings.Add(new MappingInfo
                    {
                        SourceName = classDeclaration.Identifier.ValueText,
                        TargetName = GetTargetName(mappingAttribute),
                        PropertyMappings = GetPropertyMappings(classDeclaration)
                    });
                }
            }
        }
        
        private AttributeSyntax GetMappingAttribute(ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.AttributeLists
                .SelectMany(al => al.Attributes)
                .FirstOrDefault(a => a.Name.ToString().Contains("MapTo"));
        }
        
        private string GetTargetName(AttributeSyntax attribute)
        {
            var argument = attribute.ArgumentList?.Arguments.FirstOrDefault();
            return argument?.Expression.ToString().Trim('"') ?? "Unknown";
        }
        
        private List<PropertyMapping> GetPropertyMappings(ClassDeclarationSyntax classDeclaration)
        {
            var mappings = new List<PropertyMapping>();
            
            foreach (var member in classDeclaration.Members)
            {
                if (member is PropertyDeclarationSyntax property)
                {
                    mappings.Add(new PropertyMapping
                    {
                        SourceProperty = property.Identifier.ValueText,
                        TargetProperty = property.Identifier.ValueText
                    });
                }
            }
            
            return mappings;
        }
    }
    
    public class MappingInfo
    {
        public string SourceName { get; set; }
        public string TargetName { get; set; }
        public List<PropertyMapping> PropertyMappings { get; set; } = new();
    }
    
    public class PropertyMapping
    {
        public string SourceProperty { get; set; }
        public string TargetProperty { get; set; }
    }
    
    // ===== GENERADOR DE LOGGING =====
    [Generator]
    public class LoggingGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new LoggingSyntaxReceiver());
        }
        
        public void Execute(GeneratorExecutionContext context)
        {
            var receiver = (LoggingSyntaxReceiver)context.SyntaxReceiver;
            
            foreach (var serviceClass in receiver.ServiceClasses)
            {
                var loggingSource = GenerateLoggingSource(serviceClass);
                context.AddSource($"{serviceClass.Name}LoggingDecorator.g.cs", loggingSource);
            }
        }
        
        private SourceText GenerateLoggingSource(ServiceInfo service)
        {
            var source = $@"
using System;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace GeneratedLogging
{{
    public class {service.Name}LoggingDecorator : I{service.Name}
    {{
        private readonly I{service.Name} _service;
        private readonly ILogger<{service.Name}LoggingDecorator> _logger;
        
        public {service.Name}LoggingDecorator(I{service.Name} service, ILogger<{service.Name}LoggingDecorator> logger)
        {{
            _service = service;
            _logger = logger;
        }}
        
{string.Join("\n\n", service.Methods.Select(m => GenerateLoggingMethod(m)))}
    }}
}}";
            
            return SourceText.From(source, Encoding.UTF8);
        }
        
        private string GenerateLoggingMethod(MethodInfo method)
        {
            var parameters = string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"));
            var arguments = string.Join(", ", method.Parameters.Select(p => p.Name));
            
            return $@"        public async {method.ReturnType} {method.Name}({parameters})
        {{
            _logger.LogInformation(\"Calling {method.Name} with parameters: {string.Join(", ", method.Parameters.Select(p => $"{{{p.Name}}}"))}\", {arguments});
            
            try
            {{
                var result = await _service.{method.Name}({arguments});
                _logger.LogInformation(\"Method {method.Name} completed successfully\");
                return result;
            }}
            catch (Exception ex)
            {{
                _logger.LogError(ex, \"Error in method {method.Name}\");
                throw;
            }}
        }}";
        }
    }
    
    public class LoggingSyntaxReceiver : ISyntaxReceiver
    {
        public List<ServiceInfo> ServiceClasses { get; } = new();
        
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InterfaceDeclarationSyntax interfaceDeclaration)
            {
                if (IsService(interfaceDeclaration))
                {
                    ServiceClasses.Add(new ServiceInfo
                    {
                        Name = interfaceDeclaration.Identifier.ValueText,
                        Methods = GetMethods(interfaceDeclaration)
                    });
                }
            }
        }
        
        private bool IsService(InterfaceDeclarationSyntax interfaceDeclaration)
        {
            return interfaceDeclaration.Identifier.ValueText.EndsWith("Service");
        }
        
        private List<MethodInfo> GetMethods(InterfaceDeclarationSyntax interfaceDeclaration)
        {
            var methods = new List<MethodInfo>();
            
            foreach (var member in interfaceDeclaration.Members)
            {
                if (member is MethodDeclarationSyntax method)
                {
                    methods.Add(new MethodInfo
                    {
                        Name = method.Identifier.ValueText,
                        ReturnType = method.ReturnType.ToString(),
                        Parameters = GetParameters(method)
                    });
                }
            }
            
            return methods;
        }
        
        private List<ParameterInfo> GetParameters(MethodDeclarationSyntax method)
        {
            var parameters = new List<ParameterInfo>();
            
            foreach (var parameter in method.ParameterList.Parameters)
            {
                parameters.Add(new ParameterInfo
                {
                    Name = parameter.Identifier.ValueText,
                    Type = parameter.Type.ToString()
                });
            }
            
            return parameters;
        }
    }
    
    public class ServiceInfo
    {
        public string Name { get; set; }
        public List<MethodInfo> Methods { get; set; } = new();
    }
    
    public class MethodInfo
    {
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public List<ParameterInfo> Parameters { get; set; } = new();
    }
    
    public class ParameterInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
    
    // ===== ATRIBUTOS PERSONALIZADOS =====
    [AttributeUsage(AttributeTargets.Class)]
    public class GenerateDtoAttribute : Attribute
    {
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class GenerateRepositoryAttribute : Attribute
    {
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class GenerateValidatorAttribute : Attribute
    {
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class MapToAttribute : Attribute
    {
        public string TargetType { get; }
        
        public MapToAttribute(string targetType)
        {
            TargetType = targetType;
        }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class GenerateLoggingAttribute : Attribute
    {
    }
}

// ===== USO DE SOURCE GENERATORS =====
[GenerateDto]
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}

[GenerateRepository]
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

[GenerateValidator]
public class CreateUserRequest
{
    public string Name { get; set; }
    public string Email { get; set; }
}

[MapTo("UserDto")]
public class UserModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

public interface IUserService
{
    Task<User> GetUserAsync(int id);
    Task<User> CreateUserAsync(CreateUserRequest request);
}

// Uso de Source Generators
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Source Generators y Compile-time Code Generation ===\n");
        
        Console.WriteLine("Los Source Generators proporcionan:");
        Console.WriteLine("1. Generación automática de código en tiempo de compilación");
        Console.WriteLine("2. Reducción de código repetitivo");
        Console.WriteLine("3. Mejora del rendimiento en tiempo de ejecución");
        Console.WriteLine("4. Generación de DTOs, Repositories y Validators");
        Console.WriteLine("5. Creación automática de Mappers y Logging");
        
        Console.WriteLine("\nGeneradores implementados:");
        Console.WriteLine("- DTO Generator para transferencia de datos");
        Console.WriteLine("- Repository Generator para acceso a datos");
        Console.WriteLine("- Validation Generator para validaciones");
        Console.WriteLine("- Mapper Generator para mapeo entre objetos");
        Console.WriteLine("- Logging Generator para decoradores de logging");
        
        Console.WriteLine("\nBeneficios principales:");
        Console.WriteLine("- Código más limpio y mantenible");
        Console.WriteLine("- Menos errores de implementación");
        Console.WriteLine("- Mejor rendimiento en tiempo de ejecución");
        Console.WriteLine("- Consistencia en patrones de código");
        Console.WriteLine("- Desarrollo más rápido");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Generador de DTOs
Implementa un Source Generator que genere DTOs automáticamente basado en entidades.

### Ejercicio 2: Generador de Tests
Crea un generador que genere tests unitarios básicos para servicios.

### Ejercicio 3: Generador de Configuración
Implementa un generador que genere clases de configuración basado en appsettings.

## 🔍 Puntos Clave

1. **Source Generators** ejecutan durante la compilación
2. **ISourceGenerator** es la interfaz principal
3. **ISyntaxReceiver** analiza el código fuente
4. **Context.AddSource** añade código generado
5. **Atributos personalizados** controlan la generación

## 📚 Recursos Adicionales

- [Source Generators - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview)
- [Source Generators Tutorial](https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.md)
- [Source Generators Cookbook](https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md)

---

**🎯 ¡Has completado la Clase 7! Ahora comprendes Source Generators y Compile-time Code Generation en C#**

**📚 [Siguiente: Clase 8 - High Performance Programming](clase_8_high_performance.md)**
