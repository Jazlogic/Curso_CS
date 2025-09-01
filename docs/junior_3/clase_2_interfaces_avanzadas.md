# 🚀 Clase 2: Interfaces Avanzadas

## 📋 Información de la Clase

- **Módulo**: Junior Level 3 - Programación Orientada a Objetos Avanzada
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 1 (Herencia Múltiple y Composición)

## 🎯 Objetivos de Aprendizaje

- Dominar interfaces genéricas con restricciones
- Implementar interfaces explícitas e implícitas
- Crear interfaces de marcador y funcionales
- Diseñar sistemas de contratos robustos
- Utilizar interfaces para especificaciones y validaciones

---

## 📚 Navegación del Módulo 3

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_herencia_multiple.md) | Herencia Múltiple y Composición | ← Anterior |
| **Clase 2** | **Interfaces Avanzadas** | ← Estás aquí |
| [Clase 3](clase_3_polimorfismo_avanzado.md) | Polimorfismo Avanzado | Siguiente → |
| [Clase 4](clase_4_patrones_diseno.md) | Patrones de Diseño Básicos | |
| [Clase 5](clase_5_principios_solid.md) | Principios SOLID | |
| [Clase 6](clase_6_arquitectura_modular.md) | Arquitectura Modular | |
| [Clase 7](clase_7_reflection_avanzada.md) | Reflection Avanzada | |
| [Clase 8](clase_8_serializacion_avanzada.md) | Serialización Avanzada | |
| [Clase 9](clase_9_testing_unitario.md) | Testing Unitario | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final Integrador | |

**← [Volver al README del Módulo 3](../junior_3/README.md)**

---

## 📚 Contenido Teórico

### 1. Interfaces Genéricas con Restricciones

Las interfaces genéricas permiten crear contratos reutilizables para diferentes tipos de datos, mientras que las restricciones garantizan que los tipos cumplan ciertos requisitos.

```csharp
// Interfaz genérica básica
public interface IRepository<T>
{
    T GetById(int id);
    IEnumerable<T> GetAll();
    void Add(T entity);
    void Update(T entity);
    void Delete(int id);
}

// Interfaz genérica con restricciones
public interface IRepository<T> where T : class, IEntity, new()
{
    T GetById(int id);
    IEnumerable<T> GetAll();
    void Add(T entity);
    void Update(T entity);
    void Delete(int id);
}

// Interfaz que define una entidad
public interface IEntity
{
    int Id { get; set; }
    DateTime FechaCreacion { get; set; }
    bool Activo { get; set; }
}

// Interfaz genérica con múltiples restricciones
public interface IComparableRepository<T, TKey> 
    where T : class, IEntity, IComparable<T>
    where TKey : struct, IComparable<TKey>
{
    T GetByKey(TKey key);
    IEnumerable<T> GetOrdered();
    T GetMin();
    T GetMax();
}

// Implementación de la interfaz genérica
public class UsuarioRepository : IRepository<Usuario>
{
    private List<Usuario> _usuarios = new List<Usuario>();
    
    public Usuario GetById(int id)
    {
        return _usuarios.FirstOrDefault(u => u.Id == id);
    }
    
    public IEnumerable<Usuario> GetAll()
    {
        return _usuarios.Where(u => u.Activo);
    }
    
    public void Add(Usuario entity)
    {
        entity.Id = _usuarios.Count > 0 ? _usuarios.Max(u => u.Id) + 1 : 1;
        entity.FechaCreacion = DateTime.Now;
        entity.Activo = true;
        _usuarios.Add(entity);
    }
    
    public void Update(Usuario entity)
    {
        var index = _usuarios.FindIndex(u => u.Id == entity.Id);
        if (index != -1)
        {
            _usuarios[index] = entity;
        }
    }
    
    public void Delete(int id)
    {
        var usuario = GetById(id);
        if (usuario != null)
        {
            usuario.Activo = false;
        }
    }
}

// Clase Usuario que implementa IEntity
public class Usuario : IEntity
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Email { get; set; }
    public DateTime FechaCreacion { get; set; }
    public bool Activo { get; set; }
}
```

**Explicación línea por línea:**
- `public interface IRepository<T>`: Define una interfaz genérica para repositorios
- `where T : class, IEntity, new()`: Restricciones que T debe ser una clase, implementar IEntity y tener constructor sin parámetros
- `T GetById(int id)`: Método genérico que retorna una entidad por ID
- `IEnumerable<T> GetAll()`: Método que retorna todas las entidades
- `void Add(T entity)`: Método para agregar una nueva entidad
- `void Update(T entity)`: Método para actualizar una entidad existente
- `void Delete(int id)`: Método para eliminar (desactivar) una entidad
- `public interface IEntity`: Interfaz que define propiedades comunes de entidades
- `int Id { get; set; }`: Propiedad para el identificador único
- `DateTime FechaCreacion { get; set; }`: Propiedad para la fecha de creación
- `bool Activo { get; set; }`: Propiedad para el estado activo/inactivo
- `public interface IComparableRepository<T, TKey>`: Interfaz genérica con dos tipos de parámetros
- `where T : class, IEntity, IComparable<T>`: T debe ser clase, implementar IEntity e IComparable
- `where TKey : struct, IComparable<TKey>`: TKey debe ser tipo valor e implementar IComparable
- `T GetByKey(TKey key)`: Método para obtener entidad por clave específica
- `IEnumerable<T> GetOrdered()`: Método para obtener entidades ordenadas
- `T GetMin()`: Método para obtener la entidad mínima
- `T GetMax()`: Método para obtener la entidad máxima
- `public class UsuarioRepository : IRepository<Usuario>`: Implementa la interfaz genérica para Usuario
- `private List<Usuario> _usuarios = new List<Usuario>()`: Lista privada para almacenar usuarios
- `public Usuario GetById(int id)`: Implementa el método de búsqueda por ID
- `return _usuarios.FirstOrDefault(u => u.Id == id)`: Retorna el primer usuario que coincida o null
- `public IEnumerable<Usuario> GetAll()`: Implementa el método para obtener todos los usuarios
- `return _usuarios.Where(u => u.Activo)`: Retorna solo usuarios activos
- `public void Add(Usuario entity)`: Implementa el método para agregar usuarios
- `entity.Id = _usuarios.Count > 0 ? _usuarios.Max(u => u.Id) + 1 : 1`: Asigna ID único
- `entity.FechaCreacion = DateTime.Now`: Establece fecha actual
- `entity.Activo = true`: Marca como activo
- `_usuarios.Add(entity)`: Agrega a la lista
- `public void Update(Usuario entity)`: Implementa el método de actualización
- `var index = _usuarios.FindIndex(u => u.Id == entity.Id)`: Encuentra el índice del usuario
- `if (index != -1)`: Verifica que se encontró el usuario
- `_usuarios[index] = entity`: Reemplaza el usuario existente
- `public void Delete(int id)`: Implementa el método de eliminación
- `var usuario = GetById(id)`: Obtiene el usuario a eliminar
- `if (usuario != null)`: Verifica que el usuario existe
- `usuario.Activo = false`: Marca como inactivo (soft delete)
- `public class Usuario : IEntity`: Clase que implementa la interfaz IEntity
- `public int Id { get; set; }`: Implementa la propiedad Id
- `public string Nombre { get; set; }`: Propiedad específica del usuario
- `public string Email { get; set; }`: Propiedad específica del usuario
- `public DateTime FechaCreacion { get; set; }`: Implementa la propiedad FechaCreacion
- `public bool Activo { get; set; }`: Implementa la propiedad Activo

### 2. Interfaces Explícitas e Implícitas

Las interfaces explícitas permiten implementar métodos con el mismo nombre de diferentes interfaces, mientras que las implícitas son la implementación estándar.

```csharp
// Interfaces con métodos del mismo nombre
public interface ILogger
{
    void Log(string mensaje);
    void LogError(string error);
}

public interface IFileLogger
{
    void Log(string mensaje);
    void LogError(string error);
    void LogToFile(string mensaje, string archivo);
}

// Clase que implementa ambas interfaces
public class LoggerAvanzado : ILogger, IFileLogger
{
    // Implementación implícita para ILogger
    public void Log(string mensaje)
    {
        Console.WriteLine($"[INFO] {mensaje}");
    }
    
    public void LogError(string error)
    {
        Console.WriteLine($"[ERROR] {error}");
    }
    
    // Implementación explícita para IFileLogger
    void IFileLogger.Log(string mensaje)
    {
        File.AppendAllText("log.txt", $"[FILE] {mensaje}\n");
    }
    
    void IFileLogger.LogError(string error)
    {
        File.AppendAllText("error.log", $"[FILE_ERROR] {error}\n");
    }
    
    public void LogToFile(string mensaje, string archivo)
    {
        File.AppendAllText(archivo, $"[CUSTOM] {mensaje}\n");
    }
}

// Uso de implementaciones explícitas
public class Program
{
    public static void Main()
    {
        var logger = new LoggerAvanzado();
        
        // Llama a la implementación implícita de ILogger
        logger.Log("Mensaje de información");
        logger.LogError("Error de sistema");
        
        // Llama a la implementación explícita de IFileLogger
        ((IFileLogger)logger).Log("Mensaje de archivo");
        ((IFileLogger)logger).LogError("Error de archivo");
        
        // Llama al método específico de IFileLogger
        logger.LogToFile("Mensaje personalizado", "custom.log");
        
        // También se puede usar con variables de tipo interfaz
        ILogger consoleLogger = logger;
        IFileLogger fileLogger = logger;
        
        consoleLogger.Log("Mensaje desde ILogger");
        fileLogger.Log("Mensaje desde IFileLogger");
    }
}
```

**Explicación línea por línea:**
- `public interface ILogger`: Primera interfaz para logging en consola
- `void Log(string mensaje)`: Método para log de información
- `void LogError(string error)`: Método para log de errores
- `public interface IFileLogger`: Segunda interfaz para logging en archivos
- `void Log(string mensaje)`: Método con el mismo nombre que ILogger
- `void LogError(string error)`: Método con el mismo nombre que ILogger
- `void LogToFile(string mensaje, string archivo)`: Método específico de IFileLogger
- `public class LoggerAvanzado : ILogger, IFileLogger`: Clase que implementa ambas interfaces
- `public void Log(string mensaje)`: Implementación implícita para ILogger
- `Console.WriteLine($"[INFO] {mensaje}")`: Escribe en consola con prefijo INFO
- `public void LogError(string error)`: Implementación implícita para ILogger
- `Console.WriteLine($"[ERROR] {error}")`: Escribe en consola con prefijo ERROR
- `void IFileLogger.Log(string mensaje)`: Implementación explícita para IFileLogger
- `File.AppendAllText("log.txt", $"[FILE] {mensaje}\n")`: Escribe en archivo log.txt
- `void IFileLogger.LogError(string error)`: Implementación explícita para IFileLogger
- `File.AppendAllText("error.log", $"[FILE_ERROR] {error}\n")`: Escribe en archivo error.log
- `public void LogToFile(string mensaje, string archivo)`: Método público específico
- `File.AppendAllText(archivo, $"[CUSTOM] {mensaje}\n")`: Escribe en archivo personalizado
- `var logger = new LoggerAvanzado()`: Crea instancia de la clase
- `logger.Log("Mensaje de información")`: Llama implementación implícita de ILogger
- `logger.LogError("Error de sistema")`: Llama implementación implícita de ILogger
- `((IFileLogger)logger).Log("Mensaje de archivo")`: Llama implementación explícita con cast
- `((IFileLogger)logger).LogError("Error de archivo")`: Llama implementación explícita con cast
- `logger.LogToFile("Mensaje personalizado", "custom.log")`: Llama método específico
- `ILogger consoleLogger = logger`: Crea referencia de tipo ILogger
- `IFileLogger fileLogger = logger`: Crea referencia de tipo IFileLogger
- `consoleLogger.Log("Mensaje desde ILogger")`: Llama implementación implícita
- `fileLogger.Log("Mensaje desde IFileLogger")`: Llama implementación explícita

### 3. Interfaces de Marcador y Funcionales

Las interfaces de marcador no tienen métodos, solo indican capacidades, mientras que las funcionales definen un solo método.

```csharp
// Interfaces de marcador (Marker Interfaces)
public interface ISerializable { }  // Indica que se puede serializar
public interface ICloneable { }     // Indica que se puede clonar
public interface IComparable { }    // Indica que se puede comparar
public interface IDisposable { }    // Indica que se puede disponer

// Interfaces funcionales (Functional Interfaces)
public interface IValidator<T>
{
    bool IsValid(T item);
}

public interface IFormatter<T>
{
    string Format(T item);
}

public interface IConverter<TInput, TOutput>
{
    TOutput Convert(TInput input);
}

// Implementación de interfaces de marcador
public class Documento : ISerializable, ICloneable, IComparable<Documento>
{
    public string Titulo { get; set; }
    public string Contenido { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int Prioridad { get; set; }
    
    public Documento(string titulo, string contenido, int prioridad = 1)
    {
        Titulo = titulo;
        Contenido = contenido;
        FechaCreacion = DateTime.Now;
        Prioridad = prioridad;
    }
    
    // Implementación de ICloneable
    public object Clone()
    {
        return new Documento(Titulo, Contenido, Prioridad)
        {
            FechaCreacion = this.FechaCreacion
        };
    }
    
    // Implementación de IComparable<Documento>
    public int CompareTo(Documento other)
    {
        if (other == null) return 1;
        return Prioridad.CompareTo(other.Prioridad);
    }
}

// Implementación de interfaces funcionales
public class DocumentoValidator : IValidator<Documento>
{
    public bool IsValid(Documento item)
    {
        return !string.IsNullOrEmpty(item.Titulo) && 
               !string.IsNullOrEmpty(item.Contenido) && 
               item.Prioridad > 0;
    }
}

public class DocumentoFormatter : IFormatter<Documento>
{
    public string Format(Documento item)
    {
        return $"Título: {item.Titulo}\nContenido: {item.Contenido}\nPrioridad: {item.Prioridad}\nFecha: {item.FechaCreacion:dd/MM/yyyy}";
    }
}

public class DocumentoConverter : IConverter<Documento, string>
{
    public string Convert(Documento input)
    {
        return $"{input.Titulo}|{input.Contenido}|{input.Prioridad}|{input.FechaCreacion:yyyy-MM-dd}";
    }
}

// Uso de interfaces de marcador y funcionales
public class Program
{
    public static void Main()
    {
        var documento = new Documento("Mi Documento", "Contenido del documento", 3);
        
        // Verificar capacidades usando interfaces de marcador
        if (documento is ISerializable)
        {
            Console.WriteLine("El documento se puede serializar");
        }
        
        if (documento is ICloneable)
        {
            var clon = (Documento)documento.Clone();
            Console.WriteLine($"Documento clonado: {clon.Titulo}");
        }
        
        if (documento is IComparable<Documento>)
        {
            var otroDoc = new Documento("Otro", "Contenido", 1);
            var comparacion = documento.CompareTo(otroDoc);
            Console.WriteLine($"Comparación: {comparacion}");
        }
        
        // Usar interfaces funcionales
        var validator = new DocumentoValidator();
        var formatter = new DocumentoFormatter();
        var converter = new DocumentoConverter();
        
        if (validator.IsValid(documento))
        {
            Console.WriteLine("Documento válido");
            Console.WriteLine(formatter.Format(documento));
            Console.WriteLine(converter.Convert(documento));
        }
    }
}
```

**Explicación línea por línea:**
- `public interface ISerializable { }`: Interfaz de marcador sin métodos
- `public interface ICloneable { }`: Interfaz de marcador para clonación
- `public interface IComparable { }`: Interfaz de marcador para comparación
- `public interface IDisposable { }`: Interfaz de marcador para disposición
- `public interface IValidator<T>`: Interfaz funcional para validación
- `bool IsValid(T item)`: Método único que define la funcionalidad
- `public interface IFormatter<T>`: Interfaz funcional para formateo
- `string Format(T item)`: Método único para formatear
- `public interface IConverter<TInput, TOutput>`: Interfaz funcional para conversión
- `TOutput Convert(TInput input)`: Método único para convertir
- `public class Documento : ISerializable, ICloneable, IComparable<Documento>`: Implementa múltiples interfaces
- `public string Titulo { get; set; }`: Propiedad del título
- `public string Contenido { get; set; }`: Propiedad del contenido
- `public DateTime FechaCreacion { get; set; }`: Propiedad de fecha de creación
- `public int Prioridad { get; set; }`: Propiedad de prioridad
- `public Documento(string titulo, string contenido, int prioridad = 1)`: Constructor con parámetros opcionales
- `Titulo = titulo`: Asigna el título recibido
- `Contenido = contenido`: Asigna el contenido recibido
- `FechaCreacion = DateTime.Now`: Establece fecha actual
- `Prioridad = prioridad`: Asigna la prioridad (por defecto 1)
- `public object Clone()`: Implementa el método de clonación
- `return new Documento(Titulo, Contenido, Prioridad)`: Crea nueva instancia
- `FechaCreacion = this.FechaCreacion`: Copia la fecha de creación
- `public int CompareTo(Documento other)`: Implementa comparación
- `if (other == null) return 1`: Maneja caso de objeto null
- `return Prioridad.CompareTo(other.Prioridad)`: Compara por prioridad
- `public class DocumentoValidator : IValidator<Documento>`: Implementa validador
- `public bool IsValid(Documento item)`: Valida el documento
- `return !string.IsNullOrEmpty(item.Titulo) && ...`: Verifica campos requeridos
- `public class DocumentoFormatter : IFormatter<Documento>`: Implementa formateador
- `public string Format(Documento item)`: Formatea el documento
- `return $"Título: {item.Titulo}\nContenido: {item.Contenido}..."`: Retorna formato legible
- `public class DocumentoConverter : IConverter<Documento, string>`: Implementa conversor
- `public string Convert(Documento input)`: Convierte documento a string
- `return $"{input.Titulo}|{input.Contenido}|{input.Prioridad}..."`: Retorna formato separado por pipes
- `var documento = new Documento("Mi Documento", "Contenido del documento", 3)`: Crea documento de prueba
- `if (documento is ISerializable)`: Verifica si implementa interfaz de marcador
- `Console.WriteLine("El documento se puede serializar")`: Confirma capacidad
- `if (documento is ICloneable)`: Verifica capacidad de clonación
- `var clon = (Documento)documento.Clone()`: Crea clon del documento
- `Console.WriteLine($"Documento clonado: {clon.Titulo}")`: Muestra clon exitoso
- `if (documento is IComparable<Documento>)`: Verifica capacidad de comparación
- `var otroDoc = new Documento("Otro", "Contenido", 1)`: Crea documento para comparar
- `var comparacion = documento.CompareTo(otroDoc)`: Compara documentos
- `Console.WriteLine($"Comparación: {comparacion}")`: Muestra resultado
- `var validator = new DocumentoValidator()`: Crea validador
- `var formatter = new DocumentoFormatter()`: Crea formateador
- `var converter = new DocumentoConverter()`: Crea conversor
- `if (validator.IsValid(documento))`: Valida el documento
- `Console.WriteLine("Documento válido")`: Confirma validez
- `Console.WriteLine(formatter.Format(documento))`: Muestra formato legible
- `Console.WriteLine(converter.Convert(documento))`: Muestra formato de conversión

### 4. Interfaces para Especificaciones y Validaciones

Las interfaces pueden usarse para crear sistemas de especificaciones que permitan validaciones complejas y reutilizables.

```csharp
// Interfaz base para especificaciones
public interface ISpecification<T>
{
    bool IsSatisfiedBy(T item);
    ISpecification<T> And(ISpecification<T> other);
    ISpecification<T> Or(ISpecification<T> other);
    ISpecification<T> Not();
}

// Implementación base de especificaciones
public abstract class Specification<T> : ISpecification<T>
{
    public abstract bool IsSatisfiedBy(T item);
    
    public ISpecification<T> And(ISpecification<T> other)
    {
        return new AndSpecification<T>(this, other);
    }
    
    public ISpecification<T> Or(ISpecification<T> other)
    {
        return new OrSpecification<T>(this, other);
    }
    
    public ISpecification<T> Not()
    {
        return new NotSpecification<T>(this);
    }
}

// Especificación AND
public class AndSpecification<T> : Specification<T>
{
    private readonly ISpecification<T> _left;
    private readonly ISpecification<T> _right;
    
    public AndSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        _left = left;
        _right = right;
    }
    
    public override bool IsSatisfiedBy(T item)
    {
        return _left.IsSatisfiedBy(item) && _right.IsSatisfiedBy(item);
    }
}

// Especificación OR
public class OrSpecification<T> : Specification<T>
{
    private readonly ISpecification<T> _left;
    private readonly ISpecification<T> _right;
    
    public OrSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        _left = left;
        _right = right;
    }
    
    public override bool IsSatisfiedBy(T item)
    {
        return _left.IsSatisfiedBy(item) || _right.IsSatisfiedBy(item);
    }
}

// Especificación NOT
public class NotSpecification<T> : Specification<T>
{
    private readonly ISpecification<T> _specification;
    
    public NotSpecification(ISpecification<T> specification)
    {
        _specification = specification;
    }
    
    public override bool IsSatisfiedBy(T item)
    {
        return !_specification.IsSatisfiedBy(item);
    }
}

// Especificaciones específicas para Usuario
public class UsuarioNombreSpecification : Specification<Usuario>
{
    private readonly string _nombre;
    
    public UsuarioNombreSpecification(string nombre)
    {
        _nombre = nombre;
    }
    
    public override bool IsSatisfiedBy(Usuario item)
    {
        return item.Nombre.Contains(_nombre, StringComparison.OrdinalIgnoreCase);
    }
}

public class UsuarioEmailSpecification : Specification<Usuario>
{
    private readonly string _dominio;
    
    public UsuarioEmailSpecification(string dominio)
    {
        _dominio = dominio;
    }
    
    public override bool IsSatisfiedBy(Usuario item)
    {
        return item.Email.EndsWith($"@{_dominio}", StringComparison.OrdinalIgnoreCase);
    }
}

public class UsuarioActivoSpecification : Specification<Usuario>
{
    public override bool IsSatisfiedBy(Usuario item)
    {
        return item.Activo;
    }
}

// Uso de especificaciones
public class Program
{
    public static void Main()
    {
        var usuarios = new List<Usuario>
        {
            new Usuario { Nombre = "Juan Pérez", Email = "juan@empresa.com", Activo = true },
            new Usuario { Nombre = "María García", Email = "maria@empresa.com", Activo = false },
            new Usuario { Nombre = "Carlos López", Email = "carlos@otro.com", Activo = true }
        };
        
        // Crear especificaciones
        var nombreSpec = new UsuarioNombreSpecification("Juan");
        var emailSpec = new UsuarioEmailSpecification("empresa.com");
        var activoSpec = new UsuarioActivoSpecification();
        
        // Combinar especificaciones
        var especificacionCompleja = nombreSpec.And(emailSpec).And(activoSpec);
        
        // Aplicar especificaciones
        var usuariosFiltrados = usuarios.Where(u => especificacionCompleja.IsSatisfiedBy(u));
        
        foreach (var usuario in usuariosFiltrados)
        {
            Console.WriteLine($"Usuario encontrado: {usuario.Nombre} - {usuario.Email}");
        }
        
        // Usar especificaciones individuales
        var usuariosEmpresa = usuarios.Where(u => emailSpec.IsSatisfiedBy(u));
        var usuariosInactivos = usuarios.Where(u => activoSpec.Not().IsSatisfiedBy(u));
        
        Console.WriteLine($"\nUsuarios de empresa: {usuariosEmpresa.Count()}");
        Console.WriteLine($"Usuarios inactivos: {usuariosInactivos.Count()}");
    }
}
```

**Explicación línea por línea:**
- `public interface ISpecification<T>`: Interfaz genérica para especificaciones
- `bool IsSatisfiedBy(T item)`: Método principal para verificar si un item satisface la especificación
- `ISpecification<T> And(ISpecification<T> other)`: Método para combinar con AND
- `ISpecification<T> Or(ISpecification<T> other)`: Método para combinar con OR
- `ISpecification<T> Not()`: Método para negar la especificación
- `public abstract class Specification<T> : ISpecification<T>`: Clase abstracta base
- `public abstract bool IsSatisfiedBy(T item)`: Método abstracto que debe implementarse
- `public ISpecification<T> And(ISpecification<T> other)`: Implementa operación AND
- `return new AndSpecification<T>(this, other)`: Retorna nueva especificación AND
- `public ISpecification<T> Or(ISpecification<T> other)`: Implementa operación OR
- `return new OrSpecification<T>(this, other)`: Retorna nueva especificación OR
- `public ISpecification<T> Not()`: Implementa operación NOT
- `return new NotSpecification<T>(this)`: Retorna nueva especificación NOT
- `public class AndSpecification<T> : Specification<T>`: Especificación que combina con AND
- `private readonly ISpecification<T> _left`: Especificación izquierda
- `private readonly ISpecification<T> _right`: Especificación derecha
- `public AndSpecification(ISpecification<T> left, ISpecification<T> right)`: Constructor
- `_left = left`: Asigna especificación izquierda
- `_right = right`: Asigna especificación derecha
- `public override bool IsSatisfiedBy(T item)`: Implementa la lógica AND
- `return _left.IsSatisfiedBy(item) && _right.IsSatisfiedBy(item)`: Retorna true si ambas son true
- `public class OrSpecification<T> : Specification<T>`: Especificación que combina con OR
- `public override bool IsSatisfiedBy(T item)`: Implementa la lógica OR
- `return _left.IsSatisfiedBy(item) || _right.IsSatisfiedBy(item)`: Retorna true si al menos una es true
- `public class NotSpecification<T> : Specification<T>`: Especificación que niega
- `private readonly ISpecification<T> _specification`: Especificación a negar
- `public NotSpecification(ISpecification<T> specification)`: Constructor
- `_specification = specification`: Asigna la especificación
- `public override bool IsSatisfiedBy(T item)`: Implementa la lógica NOT
- `return !_specification.IsSatisfiedBy(item)`: Retorna el opuesto del resultado
- `public class UsuarioNombreSpecification : Specification<Usuario>`: Especificación para nombre
- `private readonly string _nombre`: Nombre a buscar
- `public UsuarioNombreSpecification(string nombre)`: Constructor
- `_nombre = nombre`: Asigna el nombre
- `public override bool IsSatisfiedBy(Usuario item)`: Verifica si el nombre contiene el texto
- `return item.Nombre.Contains(_nombre, StringComparison.OrdinalIgnoreCase)`: Compara ignorando mayúsculas
- `public class UsuarioEmailSpecification : Specification<Usuario>`: Especificación para email
- `private readonly string _dominio`: Dominio a verificar
- `public UsuarioEmailSpecification(string dominio)`: Constructor
- `_dominio = dominio`: Asigna el dominio
- `public override bool IsSatisfiedBy(Usuario item)`: Verifica si el email termina con el dominio
- `return item.Email.EndsWith($"@{_dominio}", StringComparison.OrdinalIgnoreCase)`: Compara final del email
- `public class UsuarioActivoSpecification : Specification<Usuario>`: Especificación para usuarios activos
- `public override bool IsSatisfiedBy(Usuario item)`: Verifica si el usuario está activo
- `return item.Activo`: Retorna el valor de la propiedad Activo
- `var usuarios = new List<Usuario> { ... }`: Crea lista de usuarios de prueba
- `new Usuario { Nombre = "Juan Pérez", Email = "juan@empresa.com", Activo = true }`: Usuario activo
- `new Usuario { Nombre = "María García", Email = "maria@empresa.com", Activo = false }`: Usuario inactivo
- `new Usuario { Nombre = "Carlos López", Email = "carlos@otro.com", Activo = true }`: Usuario de otro dominio
- `var nombreSpec = new UsuarioNombreSpecification("Juan")`: Especificación para nombre "Juan"
- `var emailSpec = new UsuarioEmailSpecification("empresa.com")`: Especificación para dominio empresa.com
- `var activoSpec = new UsuarioActivoSpecification()`: Especificación para usuarios activos
- `var especificacionCompleja = nombreSpec.And(emailSpec).And(activoSpec)`: Combina tres especificaciones
- `var usuariosFiltrados = usuarios.Where(u => especificacionCompleja.IsSatisfiedBy(u))`: Filtra usuarios
- `foreach (var usuario in usuariosFiltrados)`: Itera sobre usuarios filtrados
- `Console.WriteLine($"Usuario encontrado: {usuario.Nombre} - {usuario.Email}")`: Muestra resultados
- `var usuariosEmpresa = usuarios.Where(u => emailSpec.IsSatisfiedBy(u))`: Filtra por dominio empresa
- `var usuariosInactivos = usuarios.Where(u => activoSpec.Not().IsSatisfiedBy(u))`: Filtra usuarios inactivos
- `Console.WriteLine($"\nUsuarios de empresa: {usuariosEmpresa.Count()}")`: Muestra conteo
- `Console.WriteLine($"Usuarios inactivos: {usuariosInactivos.Count()}")`: Muestra conteo

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Sistema de Filtros Avanzados
Crea un sistema de filtros para productos que permita combinar múltiples criterios (precio, categoría, marca, disponibilidad) usando especificaciones.

### Ejercicio 2: Validador de Formularios
Implementa un sistema de validación que permita crear reglas complejas para formularios usando interfaces funcionales.

### Ejercicio 3: Sistema de Notificaciones Inteligente
Crea un sistema que permita a diferentes tipos de usuarios implementar múltiples canales de notificación con interfaces avanzadas.

## 🔍 Puntos Clave

1. **Las interfaces genéricas** permiten crear contratos reutilizables para diferentes tipos
2. **Las restricciones** garantizan que los tipos genéricos cumplan ciertos requisitos
3. **Las implementaciones explícitas** resuelven conflictos de nombres entre interfaces
4. **Las interfaces de marcador** indican capacidades sin definir comportamiento
5. **Las interfaces funcionales** definen un solo método para funcionalidad específica
6. **Las especificaciones** permiten crear validaciones complejas y reutilizables

## 📚 Recursos Adicionales

- [Interfaces Genéricas en C# - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/generic-interfaces)
- [Specification Pattern - Martin Fowler](https://martinfowler.com/apsupp/specifications.pdf)
- [Design Patterns - GoF](https://refactoring.guru/design-patterns)

---

**🎯 ¡Has completado la Clase 2! Ahora dominas las interfaces avanzadas en C#**

**📚 [Siguiente: Clase 3 - Polimorfismo Avanzado](clase_3_polimorfismo_avanzado.md)**
