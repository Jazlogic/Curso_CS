# 🚀 Clase 10: Proyecto Final Integrador

## 📋 Información de la Clase

- **Módulo**: Junior Level 3 - Programación Orientada a Objetos Avanzada
- **Duración**: 4 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar todas las clases anteriores del módulo

## 🎯 Objetivos de Aprendizaje

- Integrar todos los conceptos aprendidos en un proyecto completo
- Aplicar patrones de diseño y principios SOLID
- Implementar arquitectura modular y testing unitario
- Crear un sistema robusto y escalable
- Documentar y presentar el proyecto final

---

## 📚 Navegación del Módulo 3

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_herencia_multiple.md) | Herencia Múltiple y Composición | |
| [Clase 2](clase_2_interfaces_avanzadas.md) | Interfaces Avanzadas | |
| [Clase 3](clase_3_polimorfismo_avanzado.md) | Polimorfismo Avanzado | |
| [Clase 4](clase_4_patrones_diseno.md) | Patrones de Diseño Básicos | |
| [Clase 5](clase_5_principios_solid.md) | Principios SOLID | |
| [Clase 6](clase_6_arquitectura_modular.md) | Arquitectura Modular | |
| [Clase 7](clase_7_reflection_avanzada.md) | Reflection Avanzada | |
| [Clase 8](clase_8_serializacion_avanzada.md) | Serialización Avanzada | |
| [Clase 9](clase_9_testing_unitario.md) | Testing Unitario | ← Anterior |
| **Clase 10** | **Proyecto Final Integrador** | ← Estás aquí |

**← [Volver al README del Módulo 3](../junior_3/README.md)**

---

## 🎯 Proyecto Final: Sistema de Gestión de Biblioteca

### Descripción del Proyecto

Desarrollaremos un sistema completo de gestión de biblioteca que integre todos los conceptos aprendidos:
- **Patrones de Diseño**: Singleton, Factory Method, Observer, Repository
- **Principios SOLID**: Responsabilidad única, Abierto/Cerrado, Sustitución de Liskov
- **Arquitectura Modular**: Separación en capas, inyección de dependencias
- **Testing Unitario**: Tests completos con mocks y stubs
- **Reflection**: Sistema de plugins para funcionalidades extendidas
- **Serialización**: Persistencia en múltiples formatos

---

## 📚 Arquitectura del Sistema

### 1. Estructura de Capas

```csharp
// Capa de Dominio (Core)
namespace Biblioteca.Core.Entities
{
    public abstract class EntidadBase
    {
        public int Id { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        
        protected EntidadBase()
        {
            FechaCreacion = DateTime.UtcNow;
        }
        
        public virtual void Actualizar()
        {
            FechaModificacion = DateTime.UtcNow;
        }
    }
    
    public class Libro : EntidadBase
    {
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public string Isbn { get; set; }
        public int AnioPublicacion { get; set; }
        public string Genero { get; set; }
        public int CantidadDisponible { get; set; }
        public int CantidadTotal { get; set; }
        public decimal Precio { get; set; }
        
        public bool EstaDisponible => CantidadDisponible > 0;
        
        public void Prestar()
        {
            if (!EstaDisponible)
                throw new InvalidOperationException("No hay ejemplares disponibles para prestar");
            
            CantidadDisponible--;
            Actualizar();
        }
        
        public void Devolver()
        {
            if (CantidadDisponible >= CantidadTotal)
                throw new InvalidOperationException("No se pueden devolver más ejemplares de los prestados");
            
            CantidadDisponible++;
            Actualizar();
        }
    }
    
    public class Usuario : EntidadBase
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public TipoUsuario Tipo { get; set; }
        public bool Activo { get; set; }
        
        public string NombreCompleto => $"{Nombre} {Apellido}";
        
        public virtual void Desactivar()
        {
            Activo = false;
            Actualizar();
        }
        
        public virtual void Activar()
        {
            Activo = true;
            Actualizar();
        }
    }
    
    public class Prestamo : EntidadBase
    {
        public int UsuarioId { get; set; }
        public int LibroId { get; set; }
        public DateTime FechaPrestamo { get; set; }
        public DateTime FechaDevolucionEsperada { get; set; }
        public DateTime? FechaDevolucionReal { get; set; }
        public EstadoPrestamo Estado { get; set; }
        
        public virtual Usuario Usuario { get; set; }
        public virtual Libro Libro { get; set; }
        
        public bool EstaVencido => DateTime.Now > FechaDevolucionEsperada && Estado == EstadoPrestamo.Prestado;
        
        public void Devolver()
        {
            if (Estado != EstadoPrestamo.Prestado)
                throw new InvalidOperationException("El préstamo no está en estado prestado");
            
            FechaDevolucionReal = DateTime.Now;
            Estado = EstadoPrestamo.Devuelto;
            Actualizar();
        }
        
        public void MarcarComoPerdido()
        {
            if (Estado != EstadoPrestamo.Prestado)
                throw new InvalidOperationException("El préstamo no está en estado prestado");
            
            Estado = EstadoPrestamo.Perdido;
            Actualizar();
        }
    }
    
    public enum TipoUsuario
    {
        Estudiante,
        Profesor,
        Personal,
        Administrador
    }
    
    public enum EstadoPrestamo
    {
        Prestado,
        Devuelto,
        Vencido,
        Perdido
    }
}

// Capa de Interfaces (Contracts)
namespace Biblioteca.Core.Interfaces
{
    public interface IRepository<T> where T : EntidadBase
    {
        Task<T> ObtenerPorIdAsync(int id);
        Task<IEnumerable<T>> ObtenerTodosAsync();
        Task<T> AgregarAsync(T entidad);
        Task<T> ActualizarAsync(T entidad);
        Task<bool> EliminarAsync(int id);
        Task<bool> ExisteAsync(int id);
    }
    
    public interface ILibroRepository : IRepository<Libro>
    {
        Task<IEnumerable<Libro>> BuscarPorTituloAsync(string titulo);
        Task<IEnumerable<Libro>> BuscarPorAutorAsync(string autor);
        Task<IEnumerable<Libro>> BuscarPorGeneroAsync(string genero);
        Task<IEnumerable<Libro>> ObtenerDisponiblesAsync();
    }
    
    public interface IUsuarioRepository : IRepository<Usuario>
    {
        Task<Usuario> ObtenerPorEmailAsync(string email);
        Task<bool> ExisteEmailAsync(string email);
        Task<IEnumerable<Usuario>> ObtenerActivosAsync();
    }
    
    public interface IPrestamoRepository : IRepository<Prestamo>
    {
        Task<IEnumerable<Prestamo>> ObtenerPorUsuarioAsync(int usuarioId);
        Task<IEnumerable<Prestamo>> ObtenerVencidosAsync();
        Task<IEnumerable<Prestamo>> ObtenerPorEstadoAsync(EstadoPrestamo estado);
    }
    
    public interface INotificacionService
    {
        Task EnviarNotificacionPrestamoAsync(Prestamo prestamo);
        Task EnviarNotificacionVencimientoAsync(Prestamo prestamo);
        Task EnviarNotificacionDevolucionAsync(Prestamo prestamo);
    }
    
    public interface IReporteService
    {
        Task<string> GenerarReportePrestamosAsync(DateTime fechaInicio, DateTime fechaFin);
        Task<string> GenerarReporteLibrosAsync();
        Task<string> GenerarReporteUsuariosAsync();
    }
}
```

### 2. Implementación de Servicios

```csharp
// Capa de Servicios (Business Logic)
namespace Biblioteca.Services
{
    public class LibroService
    {
        private readonly ILibroRepository _libroRepository;
        private readonly INotificacionService _notificacionService;
        
        public LibroService(ILibroRepository libroRepository, INotificacionService notificacionService)
        {
            _libroRepository = libroRepository;
            _notificacionService = notificacionService;
        }
        
        public async Task<Libro> AgregarLibroAsync(Libro libro)
        {
            if (string.IsNullOrWhiteSpace(libro.Titulo))
                throw new ArgumentException("El título del libro es obligatorio");
            
            if (string.IsNullOrWhiteSpace(libro.Autor))
                throw new ArgumentException("El autor del libro es obligatorio");
            
            if (libro.CantidadTotal <= 0)
                throw new ArgumentException("La cantidad total debe ser mayor a cero");
            
            libro.CantidadDisponible = libro.CantidadTotal;
            
            return await _libroRepository.AgregarAsync(libro);
        }
        
        public async Task<Libro> PrestarLibroAsync(int libroId, int usuarioId)
        {
            var libro = await _libroRepository.ObtenerPorIdAsync(libroId);
            if (libro == null)
                throw new KeyNotFoundException("Libro no encontrado");
            
            if (!libro.EstaDisponible)
                throw new InvalidOperationException("No hay ejemplares disponibles");
            
            libro.Prestar();
            await _libroRepository.ActualizarAsync(libro);
            
            return libro;
        }
        
        public async Task<Libro> DevolverLibroAsync(int libroId)
        {
            var libro = await _libroRepository.ObtenerPorIdAsync(libroId);
            if (libro == null)
                throw new KeyNotFoundException("Libro no encontrado");
            
            libro.Devolver();
            await _libroRepository.ActualizarAsync(libro);
            
            return libro;
        }
        
        public async Task<IEnumerable<Libro>> BuscarLibrosAsync(string criterio)
        {
            if (string.IsNullOrWhiteSpace(criterio))
                return await _libroRepository.ObtenerTodosAsync();
            
            var librosPorTitulo = await _libroRepository.BuscarPorTituloAsync(criterio);
            var librosPorAutor = await _libroRepository.BuscarPorAutorAsync(criterio);
            
            return librosPorTitulo.Union(librosPorAutor).Distinct();
        }
    }
    
    public class PrestamoService
    {
        private readonly IPrestamoRepository _prestamoRepository;
        private readonly ILibroRepository _libroRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly INotificacionService _notificacionService;
        
        public PrestamoService(
            IPrestamoRepository prestamoRepository,
            ILibroRepository libroRepository,
            IUsuarioRepository usuarioRepository,
            INotificacionService notificacionService)
        {
            _prestamoRepository = prestamoRepository;
            _libroRepository = libroRepository;
            _notificamoRepository = prestamoRepository;
            _notificacionService = notificacionService;
        }
        
        public async Task<Prestamo> CrearPrestamoAsync(int libroId, int usuarioId, int diasPrestamo)
        {
            var libro = await _libroRepository.ObtenerPorIdAsync(libroId);
            if (libro == null)
                throw new KeyNotFoundException("Libro no encontrado");
            
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId);
            if (usuario == null)
                throw new KeyNotFoundException("Usuario no encontrado");
            
            if (!usuario.Activo)
                throw new InvalidOperationException("El usuario no está activo");
            
            if (!libro.EstaDisponible)
                throw new InvalidOperationException("No hay ejemplares disponibles");
            
            var prestamo = new Prestamo
            {
                UsuarioId = usuarioId,
                LibroId = libroId,
                FechaPrestamo = DateTime.Now,
                FechaDevolucionEsperada = DateTime.Now.AddDays(diasPrestamo),
                Estado = EstadoPrestamo.Prestado
            };
            
            // Prestar el libro
            libro.Prestar();
            await _libroRepository.ActualizarAsync(libro);
            
            // Crear el préstamo
            var prestamoCreado = await _prestamoRepository.AgregarAsync(prestamo);
            
            // Enviar notificación
            await _notificacionService.EnviarNotificacionPrestamoAsync(prestamoCreado);
            
            return prestamoCreado;
        }
        
        public async Task<Prestamo> DevolverPrestamoAsync(int prestamoId)
        {
            var prestamo = await _prestamoRepository.ObtenerPorIdAsync(prestamoId);
            if (prestamo == null)
                throw new KeyNotFoundException("Préstamo no encontrado");
            
            if (prestamo.Estado != EstadoPrestamo.Prestado)
                throw new InvalidOperationException("El préstamo no está en estado prestado");
            
            // Devolver el libro
            var libro = await _libroRepository.ObtenerPorIdAsync(prestamo.LibroId);
            libro.Devolver();
            await _libroRepository.ActualizarAsync(libro);
            
            // Actualizar el préstamo
            prestamo.Devolver();
            await _prestamoRepository.ActualizarAsync(prestamo);
            
            // Enviar notificación
            await _notificacionService.EnviarNotificacionDevolucionAsync(prestamo);
            
            return prestamo;
        }
        
        public async Task<IEnumerable<Prestamo>> ObtenerPrestamosVencidosAsync()
        {
            return await _prestamoRepository.ObtenerVencidosAsync();
        }
        
        public async Task ProcesarVencimientosAsync()
        {
            var prestamosVencidos = await ObtenerPrestamosVencidosAsync();
            
            foreach (var prestamo in prestamosVencidos)
            {
                if (prestamo.Estado == EstadoPrestamo.Prestado)
                {
                    await _notificacionService.EnviarNotificacionVencimientoAsync(prestamo);
                }
            }
        }
    }
}
```

### 3. Implementación de Repositorios

```csharp
// Capa de Infraestructura (Data Access)
namespace Biblioteca.Infrastructure.Repositories
{
    public class LibroRepository : ILibroRepository
    {
        private readonly List<Libro> _libros = new();
        private int _nextId = 1;
        
        public async Task<Libro> ObtenerPorIdAsync(int id)
        {
            await Task.Delay(10); // Simular operación asíncrona
            return _libros.FirstOrDefault(l => l.Id == id);
        }
        
        public async Task<IEnumerable<Libro>> ObtenerTodosAsync()
        {
            await Task.Delay(10);
            return _libros.ToList();
        }
        
        public async Task<Libro> AgregarAsync(Libro libro)
        {
            await Task.Delay(10);
            libro.Id = _nextId++;
            _libros.Add(libro);
            return libro;
        }
        
        public async Task<Libro> ActualizarAsync(Libro libro)
        {
            await Task.Delay(10);
            var index = _libros.FindIndex(l => l.Id == libro.Id);
            if (index != -1)
            {
                _libros[index] = libro;
                return libro;
            }
            throw new KeyNotFoundException("Libro no encontrado");
        }
        
        public async Task<bool> EliminarAsync(int id)
        {
            await Task.Delay(10);
            var libro = _libros.FirstOrDefault(l => l.Id == id);
            if (libro != null)
            {
                _libros.Remove(libro);
                return true;
            }
            return false;
        }
        
        public async Task<bool> ExisteAsync(int id)
        {
            await Task.Delay(10);
            return _libros.Any(l => l.Id == id);
        }
        
        public async Task<IEnumerable<Libro>> BuscarPorTituloAsync(string titulo)
        {
            await Task.Delay(10);
            return _libros.Where(l => l.Titulo.Contains(titulo, StringComparison.OrdinalIgnoreCase));
        }
        
        public async Task<IEnumerable<Libro>> BuscarPorAutorAsync(string autor)
        {
            await Task.Delay(10);
            return _libros.Where(l => l.Autor.Contains(autor, StringComparison.OrdinalIgnoreCase));
        }
        
        public async Task<IEnumerable<Libro>> BuscarPorGeneroAsync(string genero)
        {
            await Task.Delay(10);
            return _libros.Where(l => l.Genero.Equals(genero, StringComparison.OrdinalIgnoreCase));
        }
        
        public async Task<IEnumerable<Libro>> ObtenerDisponiblesAsync()
        {
            await Task.Delay(10);
            return _libros.Where(l => l.EstaDisponible);
        }
    }
    
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly List<Usuario> _usuarios = new();
        private int _nextId = 1;
        
        public async Task<Usuario> ObtenerPorIdAsync(int id)
        {
            await Task.Delay(10);
            return _usuarios.FirstOrDefault(u => u.Id == id);
        }
        
        public async Task<IEnumerable<Usuario> ObtenerTodosAsync()
        {
            await Task.Delay(10);
            return _usuarios.ToList();
        }
        
        public async Task<Usuario> AgregarAsync(Usuario usuario)
        {
            await Task.Delay(10);
            usuario.Id = _nextId++;
            _usuarios.Add(usuario);
            return usuario;
        }
        
        public async Task<Usuario> ActualizarAsync(Usuario usuario)
        {
            await Task.Delay(10);
            var index = _usuarios.FindIndex(u => u.Id == usuario.Id);
            if (index != -1)
            {
                _usuarios[index] = usuario;
                return usuario;
            }
            throw new KeyNotFoundException("Usuario no encontrado");
        }
        
        public async Task<bool> EliminarAsync(int id)
        {
            await Task.Delay(10);
            var usuario = _usuarios.FirstOrDefault(u => u.Id == id);
            if (usuario != null)
            {
                _usuarios.Remove(usuario);
                return true;
            }
            return false;
        }
        
        public async Task<bool> ExisteAsync(int id)
        {
            await Task.Delay(10);
            return _usuarios.Any(u => u.Id == id);
        }
        
        public async Task<Usuario> ObtenerPorEmailAsync(string email)
        {
            await Task.Delay(10);
            return _usuarios.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
        
        public async Task<bool> ExisteEmailAsync(string email)
        {
            await Task.Delay(10);
            return _usuarios.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
        
        public async Task<IEnumerable<Usuario>> ObtenerActivosAsync()
        {
            await Task.Delay(10);
            return _usuarios.Where(u => u.Activo);
        }
    }
}
```

### 4. Sistema de Plugins con Reflection

```csharp
// Sistema de Plugins
namespace Biblioteca.Plugins
{
    public interface IPluginBiblioteca
    {
        string Nombre { get; }
        string Version { get; }
        string Descripcion { get; }
        void Ejecutar();
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class PluginBibliotecaAttribute : Attribute
    {
        public string Nombre { get; }
        public string Version { get; }
        public string Descripcion { get; }
        
        public PluginBibliotecaAttribute(string nombre, string version, string descripcion)
        {
            Nombre = nombre;
            Version = version;
            Descripcion = descripcion;
        }
    }
    
    [PluginBiblioteca("Calculadora de Multas", "1.0", "Calcula multas por préstamos vencidos")]
    public class CalculadoraMultasPlugin : IPluginBiblioteca
    {
        public string Nombre => "Calculadora de Multas";
        public string Version => "1.0";
        public string Descripcion => "Calcula multas por préstamos vencidos";
        
        public void Ejecutar()
        {
            Console.WriteLine("=== Calculadora de Multas ===");
            Console.Write("Ingrese días de retraso: ");
            
            if (int.TryParse(Console.ReadLine(), out int diasRetraso))
            {
                var multa = CalcularMulta(diasRetraso);
                Console.WriteLine($"Multa por {diasRetraso} días de retraso: ${multa:F2}");
            }
        }
        
        private decimal CalcularMulta(int diasRetraso)
        {
            const decimal multaPorDia = 0.50m;
            const decimal multaMaxima = 25.00m;
            
            var multa = diasRetraso * multaPorDia;
            return Math.Min(multa, multaMaxima);
        }
    }
    
    [PluginBiblioteca("Generador de Reportes", "1.0", "Genera reportes en diferentes formatos")]
    public class GeneradorReportesPlugin : IPluginBiblioteca
    {
        public string Nombre => "Generador de Reportes";
        public string Version => "1.0";
        public string Descripcion => "Genera reportes en diferentes formatos";
        
        public void Ejecutar()
        {
            Console.WriteLine("=== Generador de Reportes ===");
            Console.WriteLine("1. Reporte de Préstamos");
            Console.WriteLine("2. Reporte de Libros");
            Console.WriteLine("3. Reporte de Usuarios");
            Console.Write("Seleccione tipo de reporte: ");
            
            if (int.TryParse(Console.ReadLine(), out int opcion))
            {
                GenerarReporte(opcion);
            }
        }
        
        private void GenerarReporte(int tipo)
        {
            switch (tipo)
            {
                case 1:
                    Console.WriteLine("Generando reporte de préstamos...");
                    break;
                case 2:
                    Console.WriteLine("Generando reporte de libros...");
                    break;
                case 3:
                    Console.WriteLine("Generando reporte de usuarios...");
                    break;
                default:
                    Console.WriteLine("Opción inválida");
                    break;
            }
        }
    }
    
    public class PluginManager
    {
        private readonly List<IPluginBiblioteca> _plugins = new();
        
        public void CargarPlugins()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var tipos = assembly.GetTypes();
            
            foreach (var tipo in tipos)
            {
                if (typeof(IPluginBiblioteca).IsAssignableFrom(tipo) && 
                    !tipo.IsInterface && 
                    !tipo.IsAbstract &&
                    tipo.GetCustomAttribute<PluginBibliotecaAttribute>() != null)
                {
                    try
                    {
                        var plugin = (IPluginBiblioteca)Activator.CreateInstance(tipo);
                        _plugins.Add(plugin);
                        Console.WriteLine($"Plugin cargado: {plugin.Nombre} v{plugin.Version}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al cargar plugin {tipo.Name}: {ex.Message}");
                    }
                }
            }
        }
        
        public void MostrarPlugins()
        {
            Console.WriteLine("\n=== Plugins Disponibles ===");
            for (int i = 0; i < _plugins.Count; i++)
            {
                var plugin = _plugins[i];
                Console.WriteLine($"{i + 1}. {plugin.Nombre} v{plugin.Version}");
                Console.WriteLine($"   {plugin.Descripcion}");
            }
        }
        
        public void EjecutarPlugin(int indice)
        {
            if (indice < 0 || indice >= _plugins.Count)
            {
                Console.WriteLine("Índice de plugin inválido");
                return;
            }
            
            var plugin = _plugins[indice];
            Console.WriteLine($"\nEjecutando plugin: {plugin.Nombre}");
            Console.WriteLine(new string('-', 50));
            
            try
            {
                plugin.Ejecutar();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al ejecutar plugin: {ex.Message}");
            }
            
            Console.WriteLine(new string('-', 50));
        }
    }
}
```

### 5. Tests Unitarios Completos

```csharp
// Tests Unitarios
namespace Biblioteca.Tests
{
    [TestClass]
    public class LibroServiceTests
    {
        private Mock<ILibroRepository> _mockRepository;
        private Mock<INotificacionService> _mockNotificacionService;
        private LibroService _libroService;
        
        [TestInitialize]
        public void Setup()
        {
            _mockRepository = new Mock<ILibroRepository>();
            _mockNotificacionService = new Mock<INotificacionService>();
            _libroService = new LibroService(_mockRepository.Object, _mockNotificacionService.Object);
        }
        
        [TestMethod]
        public async Task AgregarLibro_DatosValidos_RetornaLibro()
        {
            // Arrange
            var libro = new Libro
            {
                Titulo = "Test Book",
                Autor = "Test Author",
                CantidadTotal = 5,
                Precio = 29.99m
            };
            
            _mockRepository.Setup(x => x.AgregarAsync(libro)).ReturnsAsync(libro);
            
            // Act
            var resultado = await _libroService.AgregarLibroAsync(libro);
            
            // Assert
            Assert.IsNotNull(resultado);
            Assert.AreEqual(5, resultado.CantidadDisponible);
            _mockRepository.Verify(x => x.AgregarAsync(libro), Times.Once);
        }
        
        [TestMethod]
        public async Task AgregarLibro_TituloVacio_LanzaArgumentException()
        {
            // Arrange
            var libro = new Libro
            {
                Titulo = "",
                Autor = "Test Author",
                CantidadTotal = 5
            };
            
            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => _libroService.AgregarLibroAsync(libro));
        }
        
        [TestMethod]
        public async Task PrestarLibro_LibroDisponible_RetornaLibroActualizado()
        {
            // Arrange
            var libro = new Libro
            {
                Id = 1,
                Titulo = "Test Book",
                CantidadDisponible = 3,
                CantidadTotal = 5
            };
            
            _mockRepository.Setup(x => x.ObtenerPorIdAsync(1)).ReturnsAsync(libro);
            _mockRepository.Setup(x => x.ActualizarAsync(libro)).ReturnsAsync(libro);
            
            // Act
            var resultado = await _libroService.PrestarLibroAsync(1, 1);
            
            // Assert
            Assert.AreEqual(2, resultado.CantidadDisponible);
            _mockRepository.Verify(x => x.ActualizarAsync(libro), Times.Once);
        }
    }
    
    [TestClass]
    public class PrestamoServiceTests
    {
        private Mock<IPrestamoRepository> _mockPrestamoRepository;
        private Mock<ILibroRepository> _mockLibroRepository;
        private Mock<IUsuarioRepository> _mockUsuarioRepository;
        private Mock<INotificacionService> _mockNotificacionService;
        private PrestamoService _prestamoService;
        
        [TestInitialize]
        public void Setup()
        {
            _mockPrestamoRepository = new Mock<IPrestamoRepository>();
            _mockLibroRepository = new Mock<ILibroRepository>();
            _mockUsuarioRepository = new Mock<IUsuarioRepository>();
            _mockNotificacionService = new Mock<INotificacionService>();
            
            _prestamoService = new PrestamoService(
                _mockPrestamoRepository.Object,
                _mockLibroRepository.Object,
                _mockUsuarioRepository.Object,
                _mockNotificacionService.Object);
        }
        
        [TestMethod]
        public async Task CrearPrestamo_DatosValidos_RetornaPrestamo()
        {
            // Arrange
            var libro = new Libro { Id = 1, CantidadDisponible = 3, CantidadTotal = 5 };
            var usuario = new Usuario { Id = 1, Activo = true };
            
            _mockLibroRepository.Setup(x => x.ObtenerPorIdAsync(1)).ReturnsAsync(libro);
            _mockUsuarioRepository.Setup(x => x.ObtenerPorIdAsync(1)).ReturnsAsync(usuario);
            _mockLibroRepository.Setup(x => x.ActualizarAsync(libro)).ReturnsAsync(libro);
            _mockPrestamoRepository.Setup(x => x.AgregarAsync(It.IsAny<Prestamo>()))
                .ReturnsAsync((Prestamo p) => { p.Id = 1; return p; });
            _mockNotificacionService.Setup(x => x.EnviarNotificacionPrestamoAsync(It.IsAny<Prestamo>()))
                .Returns(Task.CompletedTask);
            
            // Act
            var resultado = await _prestamoService.CrearPrestamoAsync(1, 1, 14);
            
            // Assert
            Assert.IsNotNull(resultado);
            Assert.AreEqual(EstadoPrestamo.Prestado, resultado.Estado);
            Assert.AreEqual(2, libro.CantidadDisponible);
            _mockNotificacionService.Verify(x => x.EnviarNotificacionPrestamoAsync(It.IsAny<Prestamo>()), Times.Once);
        }
    }
}
```

### 6. Programa Principal

```csharp
// Programa Principal
namespace Biblioteca
{
    public class Program
    {
        private static LibroService _libroService;
        private static PrestamoService _prestamoService;
        private static PluginManager _pluginManager;
        
        public static async Task Main(string[] args)
        {
            Console.WriteLine("=== Sistema de Gestión de Biblioteca ===");
            Console.WriteLine("Integrando todos los conceptos aprendidos\n");
            
            // Configurar servicios
            ConfigurarServicios();
            
            // Cargar plugins
            _pluginManager = new PluginManager();
            _pluginManager.CargarPlugins();
            
            // Menú principal
            await MostrarMenuPrincipal();
        }
        
        private static void ConfigurarServicios()
        {
            var libroRepository = new LibroRepository();
            var usuarioRepository = new UsuarioRepository();
            var prestamoRepository = new PrestamoRepository();
            
            var notificacionService = new NotificacionService();
            
            _libroService = new LibroService(libroRepository, notificacionService);
            _prestamoService = new PrestamoService(prestamoRepository, libroRepository, usuarioRepository, notificacionService);
        }
        
        private static async Task MostrarMenuPrincipal()
        {
            while (true)
            {
                Console.WriteLine("\n=== Menú Principal ===");
                Console.WriteLine("1. Gestión de Libros");
                Console.WriteLine("2. Gestión de Usuarios");
                Console.WriteLine("3. Gestión de Préstamos");
                Console.WriteLine("4. Plugins");
                Console.WriteLine("5. Salir");
                Console.Write("Seleccione una opción: ");
                
                if (int.TryParse(Console.ReadLine(), out int opcion))
                {
                    switch (opcion)
                    {
                        case 1:
                            await GestionarLibros();
                            break;
                        case 2:
                            await GestionarUsuarios();
                            break;
                        case 3:
                            await GestionarPrestamos();
                            break;
                        case 4:
                            GestionarPlugins();
                            break;
                        case 5:
                            Console.WriteLine("¡Hasta luego!");
                            return;
                        default:
                            Console.WriteLine("Opción inválida");
                            break;
                    }
                }
            }
        }
        
        private static async Task GestionarLibros()
        {
            Console.WriteLine("\n=== Gestión de Libros ===");
            Console.WriteLine("1. Agregar Libro");
            Console.WriteLine("2. Buscar Libros");
            Console.WriteLine("3. Ver Libros Disponibles");
            Console.Write("Seleccione una opción: ");
            
            if (int.TryParse(Console.ReadLine(), out int opcion))
            {
                switch (opcion)
                {
                    case 1:
                        await AgregarLibro();
                        break;
                    case 2:
                        await BuscarLibros();
                        break;
                    case 3:
                        await VerLibrosDisponibles();
                        break;
                }
            }
        }
        
        private static async Task AgregarLibro()
        {
            Console.Write("Título: ");
            var titulo = Console.ReadLine();
            
            Console.Write("Autor: ");
            var autor = Console.ReadLine();
            
            Console.Write("Cantidad Total: ");
            if (int.TryParse(Console.ReadLine(), out int cantidad))
            {
                var libro = new Libro
                {
                    Titulo = titulo,
                    Autor = autor,
                    CantidadTotal = cantidad,
                    Precio = 29.99m
                };
                
                try
                {
                    var resultado = await _libroService.AgregarLibroAsync(libro);
                    Console.WriteLine($"Libro agregado: {resultado.Titulo}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
        
        private static async Task BuscarLibros()
        {
            Console.Write("Criterio de búsqueda: ");
            var criterio = Console.ReadLine();
            
            try
            {
                var libros = await _libroService.BuscarLibrosAsync(criterio);
                Console.WriteLine($"\nLibros encontrados: {libros.Count()}");
                
                foreach (var libro in libros)
                {
                    Console.WriteLine($"- {libro.Titulo} por {libro.Autor} ({libro.CantidadDisponible} disponibles)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        
        private static async Task VerLibrosDisponibles()
        {
            try
            {
                var libros = await _libroService.ObtenerLibrosDisponiblesAsync();
                Console.WriteLine($"\nLibros disponibles: {libros.Count()}");
                
                foreach (var libro in libros)
                {
                    Console.WriteLine($"- {libro.Titulo} por {libro.Autor}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        
        private static void GestionarPlugins()
        {
            Console.WriteLine("\n=== Gestión de Plugins ===");
            _pluginManager.MostrarPlugins();
            
            Console.Write("Seleccione un plugin para ejecutar (0 para volver): ");
            if (int.TryParse(Console.ReadLine(), out int opcion) && opcion > 0)
            {
                _pluginManager.EjecutarPlugin(opcion - 1);
            }
        }
    }
}
```

---

## 🧪 Ejercicios del Proyecto Final

### Ejercicio 1: Extender el Sistema
Agrega funcionalidades como:
- Sistema de multas por retrasos
- Búsqueda avanzada de libros
- Reportes en formato PDF
- API REST para integración web

### Ejercicio 2: Mejorar la Arquitectura
Implementa:
- Base de datos real (SQL Server, PostgreSQL)
- Logging y monitoreo
- Caché distribuido
- Autenticación y autorización

### Ejercicio 3: Testing Completo
Desarrolla:
- Tests de integración
- Tests de rendimiento
- Tests de seguridad
- Cobertura de código del 90%+

---

## 🔍 Puntos Clave del Proyecto

1. **Integración completa** de todos los conceptos aprendidos
2. **Arquitectura limpia** con separación de responsabilidades
3. **Patrones de diseño** aplicados correctamente
4. **Testing unitario** completo con mocks y stubs
5. **Sistema de plugins** usando reflection
6. **Principios SOLID** implementados en todo el código

---

## 🎯 ¡Felicidades! Has Completado el Módulo 3

**Has dominado exitosamente:**
- ✅ Herencia Múltiple y Composición
- ✅ Interfaces Avanzadas
- ✅ Polimorfismo Avanzado
- ✅ Patrones de Diseño Básicos
- ✅ Principios SOLID
- ✅ Arquitectura Modular
- ✅ Reflection Avanzada
- ✅ Serialización Avanzada
- ✅ Testing Unitario
- ✅ Proyecto Final Integrador

**Estás listo para continuar con el siguiente nivel de tu formación en C#! 🚀**

---

**📚 [Volver al README del Módulo 3](../junior_3/README.md)**
