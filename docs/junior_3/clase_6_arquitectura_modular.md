# 🚀 Clase 6: Arquitectura Modular

## 📋 Información de la Clase

- **Módulo**: Junior Level 3 - Programación Orientada a Objetos Avanzada
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 5 (Principios SOLID)

## 🎯 Objetivos de Aprendizaje

- Comprender los conceptos de arquitectura modular
- Implementar separación de responsabilidades por capas
- Crear módulos independientes y cohesivos
- Aplicar patrones de arquitectura limpia
- Diseñar sistemas escalables y mantenibles

---

## 📚 Navegación del Módulo 3

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_herencia_multiple.md) | Herencia Múltiple y Composición | |
| [Clase 2](clase_2_interfaces_avanzadas.md) | Interfaces Avanzadas | |
| [Clase 3](clase_3_polimorfismo_avanzado.md) | Polimorfismo Avanzado | |
| [Clase 4](clase_4_patrones_diseno.md) | Patrones de Diseño Básicos | |
| [Clase 5](clase_5_principios_solid.md) | Principios SOLID | ← Anterior |
| **Clase 6** | **Arquitectura Modular** | ← Estás aquí |
| [Clase 7](clase_7_reflection_avanzada.md) | Reflection Avanzada | Siguiente → |
| [Clase 8](clase_8_serializacion_avanzada.md) | Serialización Avanzada | |
| [Clase 9](clase_9_testing_unitario.md) | Testing Unitario | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final Integrador | |

**← [Volver al README del Módulo 3](../junior_3/README.md)**

---

## 📚 Contenido Teórico

### 1. Arquitectura en Capas

La arquitectura en capas separa las responsabilidades del sistema en niveles bien definidos.

```csharp
// Capa de Presentación (UI)
public class UsuarioController
{
    private readonly IUsuarioService _usuarioService;
    
    public UsuarioController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }
    
    public async Task<IActionResult> CrearUsuario(CrearUsuarioRequest request)
    {
        try
        {
            var usuario = await _usuarioService.CrearUsuarioAsync(request);
            return Ok(usuario);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

// Capa de Lógica de Negocio (Business Logic)
public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IEmailService _emailService;
    private readonly IUsuarioValidator _validator;
    
    public UsuarioService(
        IUsuarioRepository usuarioRepository,
        IEmailService emailService,
        IUsuarioValidator validator)
    {
        _usuarioRepository = usuarioRepository;
        _emailService = emailService;
        _validator = validator;
    }
    
    public async Task<Usuario> CrearUsuarioAsync(CrearUsuarioRequest request)
    {
        // Validación
        if (!_validator.EsValido(request))
        {
            throw new ValidationException("Datos de usuario inválidos");
        }
        
        // Lógica de negocio
        var usuario = new Usuario
        {
            Nombre = request.Nombre,
            Email = request.Email,
            FechaCreacion = DateTime.UtcNow
        };
        
        // Persistencia
        await _usuarioRepository.GuardarAsync(usuario);
        
        // Notificación
        await _emailService.EnviarEmailBienvenidaAsync(usuario.Email);
        
        return usuario;
    }
}

// Capa de Acceso a Datos (Data Access)
public class UsuarioRepository : IUsuarioRepository
{
    private readonly DbContext _context;
    
    public UsuarioRepository(DbContext context)
    {
        _context = context;
    }
    
    public async Task<Usuario> GuardarAsync(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }
    
    public async Task<Usuario> ObtenerPorIdAsync(int id)
    {
        return await _context.Usuarios.FindAsync(id);
    }
}
```

### 2. Módulos Independientes

Los módulos deben ser independientes entre sí, comunicándose solo a través de interfaces bien definidas.

```csharp
// Módulo de Usuarios
namespace Usuarios.Module
{
    public interface IUsuarioModule
    {
        IUsuarioService UsuarioService { get; }
        IUsuarioRepository UsuarioRepository { get; }
    }
    
    public class UsuarioModule : IUsuarioModule
    {
        public IUsuarioService UsuarioService { get; }
        public IUsuarioRepository UsuarioRepository { get; }
        
        public UsuarioModule()
        {
            UsuarioRepository = new UsuarioRepository();
            UsuarioService = new UsuarioService(UsuarioRepository);
        }
    }
}

// Módulo de Notificaciones
namespace Notificaciones.Module
{
    public interface INotificacionModule
    {
        IEmailService EmailService { get; }
        ISMSService SMSService { get; }
    }
    
    public class NotificacionModule : INotificacionModule
    {
        public IEmailService EmailService { get; }
        public ISMSService SMSService { get; }
        
        public NotificacionModule()
        {
            EmailService = new EmailService();
            SMSService = new SMSService();
        }
    }
}

// Módulo de Autenticación
namespace Autenticacion.Module
{
    public interface IAutenticacionModule
    {
        IAuthService AuthService { get; }
        IJwtService JwtService { get; }
    }
    
    public class AutenticacionModule : IAutenticacionModule
    {
        public IAuthService AuthService { get; }
        public IJwtService JwtService { get; }
        
        public AutenticacionModule()
        {
            JwtService = new JwtService();
            AuthService = new AuthService(JwtService);
        }
    }
}
```

### 3. Inyección de Dependencias

La inyección de dependencias permite desacoplar los módulos y facilitar las pruebas.

```csharp
// Contenedor de dependencias
public class DependencyContainer
{
    private readonly Dictionary<Type, Type> _registrations = new();
    private readonly Dictionary<Type, object> _singletons = new();
    
    public void Register<TInterface, TImplementation>()
    {
        _registrations[typeof(TInterface)] = typeof(TImplementation);
    }
    
    public void RegisterSingleton<TInterface, TImplementation>()
    {
        _registrations[typeof(TInterface)] = typeof(TImplementation);
        var instance = CreateInstance<TInterface>();
        _singletons[typeof(TInterface)] = instance;
    }
    
    public T Resolve<T>()
    {
        return (T)Resolve(typeof(T));
    }
    
    private object Resolve(Type type)
    {
        if (_singletons.ContainsKey(type))
        {
            return _singletons[type];
        }
        
        if (_registrations.ContainsKey(type))
        {
            var implementationType = _registrations[type];
            var instance = CreateInstance(implementationType);
            
            if (_singletons.ContainsKey(type))
            {
                _singletons[type] = instance;
            }
            
            return instance;
        }
        
        throw new InvalidOperationException($"Tipo {type.Name} no registrado");
    }
    
    private T CreateInstance<T>()
    {
        return (T)CreateInstance(typeof(T));
    }
    
    private object CreateInstance(Type type)
    {
        var constructors = type.GetConstructors();
        var constructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();
        var parameters = constructor.GetParameters();
        var parameterInstances = parameters.Select(p => Resolve(p.ParameterType)).ToArray();
        
        return Activator.CreateInstance(type, parameterInstances);
    }
}

// Configuración del contenedor
public class ModuleConfiguration
{
    public static DependencyContainer Configure()
    {
        var container = new DependencyContainer();
        
        // Registrar módulos
        container.Register<IUsuarioModule, UsuarioModule>();
        container.Register<INotificacionModule, NotificacionModule>();
        container.Register<IAutenticacionModule, AutenticacionModule>();
        
        // Registrar servicios como singletons
        container.RegisterSingleton<IEmailService, EmailService>();
        container.RegisterSingleton<ISMSService, SMSService>();
        
        return container;
    }
}
```

### 4. Patrón de Arquitectura Limpia

La arquitectura limpia organiza el código en capas concéntricas con dependencias hacia adentro.

```csharp
// Capa de Entidades (Core)
namespace Core.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public DateTime FechaCreacion { get; set; }
        
        public bool EsValido()
        {
            return !string.IsNullOrEmpty(Nombre) && 
                   !string.IsNullOrEmpty(Email) && 
                   Email.Contains("@");
        }
    }
}

// Capa de Casos de Uso (Use Cases)
namespace Core.UseCases
{
    public interface ICrearUsuarioUseCase
    {
        Task<Usuario> EjecutarAsync(CrearUsuarioRequest request);
    }
    
    public class CrearUsuarioUseCase : ICrearUsuarioUseCase
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IEmailService _emailService;
        
        public CrearUsuarioUseCase(
            IUsuarioRepository usuarioRepository,
            IEmailService emailService)
        {
            _usuarioRepository = usuarioRepository;
            _emailService = emailService;
        }
        
        public async Task<Usuario> EjecutarAsync(CrearUsuarioRequest request)
        {
            var usuario = new Usuario
            {
                Nombre = request.Nombre,
                Email = request.Email,
                FechaCreacion = DateTime.UtcNow
            };
            
            if (!usuario.EsValido())
            {
                throw new ValidationException("Usuario inválido");
            }
            
            await _usuarioRepository.GuardarAsync(usuario);
            await _emailService.EnviarEmailBienvenidaAsync(usuario.Email);
            
            return usuario;
        }
    }
}

// Capa de Interfaces (Adapters)
namespace Infrastructure.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario> GuardarAsync(Usuario usuario);
        Task<Usuario> ObtenerPorIdAsync(int id);
    }
    
    public interface IEmailService
    {
        Task EnviarEmailBienvenidaAsync(string email);
    }
}

// Capa de Implementación (Infrastructure)
namespace Infrastructure.Implementation
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly DbContext _context;
        
        public UsuarioRepository(DbContext context)
        {
            _context = context;
        }
        
        public async Task<Usuario> GuardarAsync(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }
        
        public async Task<Usuario> ObtenerPorIdAsync(int id)
        {
            return await _context.Usuarios.FindAsync(id);
        }
    }
    
    public class EmailService : IEmailService
    {
        public async Task EnviarEmailBienvenidaAsync(string email)
        {
            // Implementación del envío de email
            await Task.Delay(100); // Simulación
            Console.WriteLine($"Email de bienvenida enviado a {email}");
        }
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Diseñar Módulo de Productos
Crea un módulo independiente para gestión de productos con arquitectura en capas.

### Ejercicio 2: Implementar Sistema de Módulos
Desarrolla un sistema que permita cargar y descargar módulos dinámicamente.

### Ejercicio 3: Arquitectura Limpia para E-commerce
Diseña una arquitectura limpia para un sistema de comercio electrónico.

## 🔍 Puntos Clave

1. **Separación de responsabilidades** por capas bien definidas
2. **Módulos independientes** que se comunican por interfaces
3. **Inyección de dependencias** para desacoplamiento
4. **Arquitectura limpia** con dependencias hacia adentro
5. **Interfaces bien definidas** para comunicación entre módulos

## 📚 Recursos Adicionales

- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Dependency Injection - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [Modular Architecture Patterns](https://martinfowler.com/articles/modular-architecture.html)

---

**🎯 ¡Has completado la Clase 6! Ahora entiendes la arquitectura modular en C#**

**📚 [Siguiente: Clase 7 - Reflection Avanzada](clase_7_reflection_avanzada.md)**
