# 🚀 Clase 4: Patrones de Diseño Básicos

## 📋 Información de la Clase

- **Módulo**: Junior Level 3 - Programación Orientada a Objetos Avanzada
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 3 (Polimorfismo Avanzado)

## 🎯 Objetivos de Aprendizaje

- Comprender los patrones de diseño fundamentales
- Implementar el patrón Singleton
- Aplicar el patrón Factory Method
- Utilizar el patrón Observer
- Crear sistemas flexibles y mantenibles

---

## 📚 Navegación del Módulo 3

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_herencia_multiple.md) | Herencia Múltiple y Composición | |
| [Clase 2](clase_2_interfaces_avanzadas.md) | Interfaces Avanzadas | |
| [Clase 3](clase_3_polimorfismo_avanzado.md) | Polimorfismo Avanzado | ← Anterior |
| **Clase 4** | **Patrones de Diseño Básicos** | ← Estás aquí |
| [Clase 5](clase_5_principios_solid.md) | Principios SOLID | Siguiente → |
| [Clase 6](clase_6_arquitectura_modular.md) | Arquitectura Modular | |
| [Clase 7](clase_7_reflection_avanzada.md) | Reflection Avanzada | |
| [Clase 8](clase_8_serializacion_avanzada.md) | Serialización Avanzada | |
| [Clase 9](clase_9_testing_unitario.md) | Testing Unitario | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final Integrador | |

**← [Volver al README del Módulo 3](../junior_3/README.md)**

---

## 📚 Contenido Teórico

### 1. Introducción a los Patrones de Diseño

Los patrones de diseño son soluciones reutilizables para problemas comunes en el desarrollo de software. Proporcionan un vocabulario común y mejores prácticas para diseñar sistemas.

```csharp
// Clasificación de patrones de diseño
public enum TipoPatron
{
    Creacional,    // Patrones de creación de objetos
    Estructural,   // Patrones de composición de clases y objetos
    Comportamental // Patrones de comunicación entre objetos
}

// Interfaz base para todos los patrones
public interface IPatronDiseño
{
    string Nombre { get; }
    TipoPatron Tipo { get; }
    string Descripcion { get; }
    void Demostrar();
}

// Clase base abstracta para patrones
public abstract class PatronDiseñoBase : IPatronDiseño
{
    public abstract string Nombre { get; }
    public abstract TipoPatron Tipo { get; }
    public abstract string Descripcion { get; }
    
    public abstract void Demostrar();
    
    public virtual void MostrarInformacion()
    {
        Console.WriteLine($"=== {Nombre} ===");
        Console.WriteLine($"Tipo: {Tipo}");
        Console.WriteLine($"Descripción: {Descripcion}");
        Console.WriteLine();
    }
}
```

**Explicación línea por línea:**
- `public enum TipoPatron`: Define los tipos principales de patrones de diseño
- `Creacional`: Patrones que se enfocan en la creación de objetos
- `Estructural`: Patrones que se enfocan en la composición de clases y objetos
- `Comportamental`: Patrones que se enfocan en la comunicación entre objetos
- `public interface IPatronDiseño`: Interfaz que define el contrato para todos los patrones
- `string Nombre { get; }`: Propiedad para el nombre del patrón
- `TipoPatron Tipo { get; }`: Propiedad para el tipo del patrón
- `string Descripcion { get; }`: Propiedad para la descripción del patrón
- `void Demostrar()`: Método para demostrar el patrón en acción
- `public abstract class PatronDiseñoBase`: Clase base abstracta que implementa la interfaz
- `public abstract string Nombre { get; }`: Propiedad abstracta que debe implementarse
- `public abstract TipoPatron Tipo { get; }`: Propiedad abstracta que debe implementarse
- `public abstract string Descripcion { get; }`: Propiedad abstracta que debe implementarse
- `public abstract void Demostrar()`: Método abstracto que debe implementarse
- `public virtual void MostrarInformacion()`: Método virtual que muestra información del patrón
- `Console.WriteLine($"=== {Nombre} ===")`: Muestra el nombre del patrón
- `Console.WriteLine($"Tipo: {Tipo}")`: Muestra el tipo del patrón
- `Console.WriteLine($"Descripción: {Descripcion}")`: Muestra la descripción del patrón

### 2. Patrón Singleton

El patrón Singleton garantiza que una clase tenga solo una instancia y proporciona un punto de acceso global a ella.

```csharp
// Implementación básica del patrón Singleton
public class SingletonBasico
{
    private static SingletonBasico _instancia;
    private static readonly object _lock = new object();
    
    // Constructor privado para evitar instanciación externa
    private SingletonBasico()
    {
        Console.WriteLine("Singleton básico creado");
    }
    
    // Propiedad estática para acceder a la instancia
    public static SingletonBasico Instancia
    {
        get
        {
            if (_instancia == null)
            {
                lock (_lock)
                {
                    if (_instancia == null)
                    {
                        _instancia = new SingletonBasico();
                    }
                }
            }
            return _instancia;
        }
    }
    
    public void MetodoEjemplo()
    {
        Console.WriteLine("Método del singleton ejecutado");
    }
}

// Singleton thread-safe con inicialización lazy
public class SingletonThreadSafe
{
    private static readonly Lazy<SingletonThreadSafe> _lazy = 
        new Lazy<SingletonThreadSafe>(() => new SingletonThreadSafe());
    
    private SingletonThreadSafe()
    {
        Console.WriteLine("Singleton thread-safe creado");
    }
    
    public static SingletonThreadSafe Instancia => _lazy.Value;
    
    public void MetodoEjemplo()
    {
        Console.WriteLine("Método del singleton thread-safe ejecutado");
    }
}

// Singleton genérico
public class SingletonGenerico<T> where T : class, new()
{
    private static T _instancia;
    private static readonly object _lock = new object();
    
    public static T Instancia
    {
        get
        {
            if (_instancia == null)
            {
                lock (_lock)
                {
                    if (_instancia == null)
                    {
                        _instancia = new T();
                    }
                }
            }
            return _instancia;
        }
    }
}

// Clase de ejemplo para el singleton genérico
public class Configuracion
{
    public string NombreAplicacion { get; set; } = "Mi Aplicación";
    public int Puerto { get; set; } = 8080;
    public bool ModoDebug { get; set; } = true;
    
    public void MostrarConfiguracion()
    {
        Console.WriteLine($"Aplicación: {NombreAplicacion}");
        Console.WriteLine($"Puerto: {Puerto}");
        Console.WriteLine($"Debug: {ModoDebug}");
    }
}

// Uso del patrón Singleton
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Demostración del Patrón Singleton ===\n");
        
        // Singleton básico
        var singleton1 = SingletonBasico.Instancia;
        var singleton2 = SingletonBasico.Instancia;
        
        Console.WriteLine($"¿Son la misma instancia? {ReferenceEquals(singleton1, singleton2)}");
        singleton1.MetodoEjemplo();
        
        // Singleton thread-safe
        var singletonThreadSafe1 = SingletonThreadSafe.Instancia;
        var singletonThreadSafe2 = SingletonThreadSafe.Instancia;
        
        Console.WriteLine($"¿Son la misma instancia? {ReferenceEquals(singletonThreadSafe1, singletonThreadSafe2)}");
        singletonThreadSafe1.MetodoEjemplo();
        
        // Singleton genérico
        var config1 = SingletonGenerico<Configuracion>.Instancia;
        var config2 = SingletonGenerico<Configuracion>.Instancia;
        
        Console.WriteLine($"¿Son la misma instancia? {ReferenceEquals(config1, config2)}");
        config1.MostrarConfiguracion();
        
        // Modificar la configuración
        config1.NombreAplicacion = "Aplicación Modificada";
        config2.MostrarConfiguracion(); // Muestra la configuración modificada
    }
}
```

**Explicación línea por línea:**
- `public class SingletonBasico`: Implementación básica del patrón Singleton
- `private static SingletonBasico _instancia`: Campo estático privado para la instancia única
- `private static readonly object _lock`: Objeto de bloqueo para thread-safety
- `private SingletonBasico()`: Constructor privado que evita instanciación externa
- `Console.WriteLine("Singleton básico creado")`: Mensaje cuando se crea la instancia
- `public static SingletonBasico Instancia`: Propiedad estática para acceder a la instancia
- `if (_instancia == null)`: Verifica si la instancia ya existe
- `lock (_lock)`: Bloquea el acceso para evitar condiciones de carrera
- `if (_instancia == null)`: Verificación doble dentro del lock
- `_instancia = new SingletonBasico()`: Crea la instancia si no existe
- `return _instancia`: Retorna la instancia existente o creada
- `public void MetodoEjemplo()`: Método de ejemplo del singleton
- `Console.WriteLine("Método del singleton ejecutado")`: Mensaje de confirmación
- `public class SingletonThreadSafe`: Implementación thread-safe usando Lazy<T>
- `private static readonly Lazy<SingletonThreadSafe> _lazy`: Lazy<T> para inicialización diferida
- `new Lazy<SingletonThreadSafe>(() => new SingletonThreadSafe())`: Crea lazy con factory
- `() => new SingletonThreadSafe()`: Expresión lambda que crea la instancia
- `public static SingletonThreadSafe Instancia => _lazy.Value`: Propiedad que accede al valor lazy
- `_lazy.Value`: Accede al valor, creándolo si es necesario
- `public class SingletonGenerico<T> where T : class, new()`: Singleton genérico con restricciones
- `where T : class, new()`: T debe ser clase y tener constructor sin parámetros
- `private static T _instancia`: Campo genérico para la instancia
- `public static T Instancia`: Propiedad genérica para acceder a la instancia
- `_instancia = new T()`: Crea nueva instancia del tipo genérico T
- `public class Configuracion`: Clase de ejemplo para el singleton genérico
- `public string NombreAplicacion { get; set; } = "Mi Aplicación"`: Propiedad con valor por defecto
- `public int Puerto { get; set; } = 8080`: Propiedad con valor por defecto
- `public bool ModoDebug { get; set; } = true`: Propiedad con valor por defecto
- `public void MostrarConfiguracion()`: Método para mostrar la configuración
- `Console.WriteLine($"Aplicación: {NombreAplicacion}")`: Muestra nombre de la aplicación
- `Console.WriteLine($"Puerto: {Puerto}")`: Muestra puerto
- `Console.WriteLine($"Debug: {ModoDebug}")`: Muestra modo debug
- `var singleton1 = SingletonBasico.Instancia`: Obtiene primera instancia del singleton
- `var singleton2 = SingletonBasico.Instancia`: Obtiene segunda referencia al singleton
- `ReferenceEquals(singleton1, singleton2)`: Compara si son la misma instancia en memoria
- `singleton1.MetodoEjemplo()`: Llama método del singleton
- `var config1 = SingletonGenerico<Configuracion>.Instancia`: Obtiene instancia del singleton genérico
- `var config2 = SingletonGenerico<Configuracion>.Instancia`: Obtiene segunda referencia
- `ReferenceEquals(config1, config2)`: Verifica que sean la misma instancia
- `config1.MostrarConfiguracion()`: Muestra configuración inicial
- `config1.NombreAplicacion = "Aplicación Modificada"`: Modifica la configuración
- `config2.MostrarConfiguracion()`: Muestra configuración modificada (misma instancia)

### 3. Patrón Factory Method

El patrón Factory Method define una interfaz para crear objetos, pero permite a las subclases decidir qué clase instanciar.

```csharp
// Interfaz para productos
public interface IProducto
{
    string Nombre { get; }
    decimal Precio { get; }
    void MostrarInformacion();
}

// Productos concretos
public class ProductoElectronico : IProducto
{
    public string Nombre { get; set; }
    public decimal Precio { get; set; }
    public string Marca { get; set; }
    
    public ProductoElectronico(string nombre, decimal precio, string marca)
    {
        Nombre = nombre;
        Precio = precio;
        Marca = marca;
    }
    
    public void MostrarInformacion()
    {
        Console.WriteLine($"Electrónico: {Nombre} - Marca: {Marca} - Precio: ${Precio}");
    }
}

public class ProductoRopa : IProducto
{
    public string Nombre { get; set; }
    public decimal Precio { get; set; }
    public string Talla { get; set; }
    
    public ProductoRopa(string nombre, decimal precio, string talla)
    {
        Nombre = nombre;
        Precio = precio;
        Talla = talla;
    }
    
    public void MostrarInformacion()
    {
        Console.WriteLine($"Ropa: {Nombre} - Talla: {Talla} - Precio: ${Precio}");
    }
}

public class ProductoLibro : IProducto
{
    public string Nombre { get; set; }
    public decimal Precio { get; set; }
    public string Autor { get; set; }
    
    public ProductoLibro(string nombre, decimal precio, string autor)
    {
        Nombre = nombre;
        Precio = precio;
        Autor = autor;
    }
    
    public void MostrarInformacion()
    {
        Console.WriteLine($"Libro: {Nombre} - Autor: {Autor} - Precio: ${Precio}");
    }
}

// Interfaz para el factory method
public interface IProductoFactory
{
    IProducto CrearProducto(string nombre, decimal precio, Dictionary<string, string> propiedades);
}

// Implementaciones concretas del factory
public class ProductoElectronicoFactory : IProductoFactory
{
    public IProducto CrearProducto(string nombre, decimal precio, Dictionary<string, string> propiedades)
    {
        string marca = propiedades.ContainsKey("Marca") ? propiedades["Marca"] : "Sin marca";
        return new ProductoElectronico(nombre, precio, marca);
    }
}

public class ProductoRopaFactory : IProductoFactory
{
    public IProducto CrearProducto(string nombre, decimal precio, Dictionary<string, string> propiedades)
    {
        string talla = propiedades.ContainsKey("Talla") ? propiedades["Talla"] : "M";
        return new ProductoRopa(nombre, precio, talla);
    }
}

public class ProductoLibroFactory : IProductoFactory
{
    public IProducto CrearProducto(string nombre, decimal precio, Dictionary<string, string> propiedades)
    {
        string autor = propiedades.ContainsKey("Autor") ? propiedades["Autor"] : "Anónimo";
        return new ProductoLibro(nombre, precio, autor);
    }
}

// Factory principal que usa factory methods
public class ProductoFactory
{
    private readonly Dictionary<string, IProductoFactory> _factories;
    
    public ProductoFactory()
    {
        _factories = new Dictionary<string, IProductoFactory>
        {
            { "Electronico", new ProductoElectronicoFactory() },
            { "Ropa", new ProductoRopaFactory() },
            { "Libro", new ProductoLibroFactory() }
        };
    }
    
    public IProducto CrearProducto(string tipo, string nombre, decimal precio, Dictionary<string, string> propiedades)
    {
        if (_factories.ContainsKey(tipo))
        {
            return _factories[tipo].CrearProducto(nombre, precio, propiedades);
        }
        
        throw new ArgumentException($"Tipo de producto '{tipo}' no soportado");
    }
    
    public List<string> ObtenerTiposDisponibles()
    {
        return _factories.Keys.ToList();
    }
}

// Uso del patrón Factory Method
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Demostración del Patrón Factory Method ===\n");
        
        var factory = new ProductoFactory();
        
        // Mostrar tipos disponibles
        Console.WriteLine("Tipos de productos disponibles:");
        foreach (var tipo in factory.ObtenerTiposDisponibles())
        {
            Console.WriteLine($"- {tipo}");
        }
        Console.WriteLine();
        
        // Crear productos usando factory methods
        var propiedadesElectronico = new Dictionary<string, string> { { "Marca", "Samsung" } };
        var productoElectronico = factory.CrearProducto("Electronico", "Smartphone", 599.99m, propiedadesElectronico);
        
        var propiedadesRopa = new Dictionary<string, string> { { "Talla", "L" } };
        var productoRopa = factory.CrearProducto("Ropa", "Camiseta", 29.99m, propiedadesRopa);
        
        var propiedadesLibro = new Dictionary<string, string> { { "Autor", "Gabriel García Márquez" } };
        var productoLibro = factory.CrearProducto("Libro", "Cien años de soledad", 19.99m, propiedadesLibro);
        
        // Mostrar información de los productos
        productoElectronico.MostrarInformacion();
        productoRopa.MostrarInformacion();
        productoLibro.MostrarInformacion();
        
        // Crear productos sin propiedades específicas
        var productoSimple = factory.CrearProducto("Electronico", "Auriculares", 49.99m, new Dictionary<string, string>());
        productoSimple.MostrarInformacion();
    }
}
```

**Explicación línea por línea:**
- `public interface IProducto`: Interfaz base para todos los productos
- `string Nombre { get; }`: Propiedad para el nombre del producto
- `decimal Precio { get; }`: Propiedad para el precio del producto
- `void MostrarInformacion()`: Método para mostrar información del producto
- `public class ProductoElectronico : IProducto`: Implementación concreta para productos electrónicos
- `public string Marca { get; set; }`: Propiedad específica de productos electrónicos
- `public ProductoElectronico(string nombre, decimal precio, string marca)`: Constructor
- `Nombre = nombre`: Asigna el nombre recibido
- `Precio = precio`: Asigna el precio recibido
- `Marca = marca`: Asigna la marca recibida
- `public void MostrarInformacion()`: Implementa el método de la interfaz
- `Console.WriteLine($"Electrónico: {Nombre} - Marca: {Marca} - Precio: ${Precio}")`: Muestra información
- `public class ProductoRopa : IProducto`: Implementación para productos de ropa
- `public string Talla { get; set; }`: Propiedad específica de productos de ropa
- `public ProductoRopa(string nombre, decimal precio, string talla)`: Constructor
- `Talla = talla`: Asigna la talla recibida
- `Console.WriteLine($"Ropa: {Nombre} - Talla: {Talla} - Precio: ${Precio}")`: Muestra información
- `public class ProductoLibro : IProducto`: Implementación para productos de libros
- `public string Autor { get; set; }`: Propiedad específica de productos de libros
- `public ProductoLibro(string nombre, decimal precio, string autor)`: Constructor
- `Autor = autor`: Asigna el autor recibido
- `Console.WriteLine($"Libro: {Nombre} - Autor: {Autor} - Precio: ${Precio}")`: Muestra información
- `public interface IProductoFactory`: Interfaz para el factory method
- `IProducto CrearProducto(string nombre, decimal precio, Dictionary<string, string> propiedades)`: Método factory
- `public class ProductoElectronicoFactory : IProductoFactory`: Factory para productos electrónicos
- `public IProducto CrearProducto(string nombre, decimal precio, Dictionary<string, string> propiedades)`: Implementa factory
- `string marca = propiedades.ContainsKey("Marca") ? propiedades["Marca"] : "Sin marca"`: Obtiene marca o valor por defecto
- `return new ProductoElectronico(nombre, precio, marca)`: Crea y retorna producto electrónico
- `public class ProductoRopaFactory : IProductoFactory`: Factory para productos de ropa
- `string talla = propiedades.ContainsKey("Talla") ? propiedades["Talla"] : "M"`: Obtiene talla o valor por defecto
- `return new ProductoRopa(nombre, precio, talla)`: Crea y retorna producto de ropa
- `public class ProductoLibroFactory : IProductoFactory`: Factory para productos de libros
- `string autor = propiedades.ContainsKey("Autor") ? propiedades["Autor"] : "Anónimo"`: Obtiene autor o valor por defecto
- `return new ProductoLibro(nombre, precio, autor)`: Crea y retorna producto de libro
- `public class ProductoFactory`: Factory principal que coordina los factory methods
- `private readonly Dictionary<string, IProductoFactory> _factories`: Diccionario de factories por tipo
- `public ProductoFactory()`: Constructor que inicializa los factories
- `_factories = new Dictionary<string, IProductoFactory> { ... }`: Inicializa diccionario con factories
- `{ "Electronico", new ProductoElectronicoFactory() }`: Asocia tipo con factory correspondiente
- `{ "Ropa", new ProductoRopaFactory() }`: Asocia tipo con factory correspondiente
- `{ "Libro", new ProductoLibroFactory() }`: Asocia tipo con factory correspondiente
- `public IProducto CrearProducto(string tipo, string nombre, decimal precio, Dictionary<string, string> propiedades)`: Método principal
- `if (_factories.ContainsKey(tipo))`: Verifica si existe factory para el tipo
- `return _factories[tipo].CrearProducto(nombre, precio, propiedades)`: Delega creación al factory específico
- `throw new ArgumentException($"Tipo de producto '{tipo}' no soportado")`: Lanza excepción si tipo no soportado
- `public List<string> ObtenerTiposDisponibles()`: Método para obtener tipos disponibles
- `return _factories.Keys.ToList()`: Retorna lista de tipos disponibles
- `var factory = new ProductoFactory()`: Crea instancia del factory principal
- `Console.WriteLine("Tipos de productos disponibles:")`: Muestra encabezado
- `foreach (var tipo in factory.ObtenerTiposDisponibles())`: Itera sobre tipos disponibles
- `Console.WriteLine($"- {tipo}")`: Muestra cada tipo disponible
- `var propiedadesElectronico = new Dictionary<string, string> { { "Marca", "Samsung" } }`: Crea propiedades para electrónico
- `var productoElectronico = factory.CrearProducto("Electronico", "Smartphone", 599.99m, propiedadesElectronico)`: Crea producto
- `var propiedadesRopa = new Dictionary<string, string> { { "Talla", "L" } }`: Crea propiedades para ropa
- `var productoRopa = factory.CrearProducto("Ropa", "Camiseta", 29.99m, propiedadesRopa)`: Crea producto
- `var propiedadesLibro = new Dictionary<string, string> { { "Autor", "Gabriel García Márquez" } }`: Crea propiedades para libro
- `var productoLibro = factory.CrearProducto("Libro", "Cien años de soledad", 19.99m, propiedadesLibro)`: Crea producto
- `productoElectronico.MostrarInformacion()`: Muestra información del producto electrónico
- `productoRopa.MostrarInformacion()`: Muestra información del producto de ropa
- `productoLibro.MostrarInformacion()`: Muestra información del producto de libro
- `var productoSimple = factory.CrearProducto("Electronico", "Auriculares", 49.99m, new Dictionary<string, string>())`: Crea sin propiedades
- `productoSimple.MostrarInformacion()`: Muestra información del producto simple

### 4. Patrón Observer

El patrón Observer define una dependencia uno-a-muchos entre objetos, de modo que cuando un objeto cambia su estado, todos sus dependientes son notificados automáticamente.

```csharp
// Interfaz para observadores
public interface IObserver
{
    void Actualizar(string mensaje);
}

// Interfaz para sujetos observables
public interface ISujeto
{
    void RegistrarObservador(IObserver observador);
    void RemoverObservador(IObserver observador);
    void NotificarObservadores(string mensaje);
}

// Implementación del sujeto observable
public class SujetoObservable : ISujeto
{
    private readonly List<IObserver> _observadores = new List<IObserver>();
    private string _estado;
    
    public string Estado
    {
        get => _estado;
        set
        {
            _estado = value;
            NotificarObservadores($"Estado cambiado a: {_estado}");
        }
    }
    
    public void RegistrarObservador(IObserver observador)
    {
        if (!_observadores.Contains(observador))
        {
            _observadores.Add(observador);
            Console.WriteLine($"Observador registrado: {observador.GetType().Name}");
        }
    }
    
    public void RemoverObservador(IObserver observador)
    {
        if (_observadores.Contains(observador))
        {
            _observadores.Remove(observador);
            Console.WriteLine($"Observador removido: {observador.GetType().Name}");
        }
    }
    
    public void NotificarObservadores(string mensaje)
    {
        Console.WriteLine($"\nNotificando a {_observadores.Count} observadores...");
        foreach (var observador in _observadores)
        {
            observador.Actualizar(mensaje);
        }
    }
    
    public void RealizarAccion(string accion)
    {
        Console.WriteLine($"\nRealizando acción: {accion}");
        NotificarObservadores($"Acción realizada: {accion}");
    }
}

// Implementaciones concretas de observadores
public class ObservadorConsola : IObserver
{
    private readonly string _nombre;
    
    public ObservadorConsola(string nombre)
    {
        _nombre = nombre;
    }
    
    public void Actualizar(string mensaje)
    {
        Console.WriteLine($"[{_nombre}] Notificación recibida: {mensaje}");
    }
}

public class ObservadorArchivo : IObserver
{
    private readonly string _nombreArchivo;
    
    public ObservadorArchivo(string nombreArchivo)
    {
        _nombreArchivo = nombreArchivo;
    }
    
    public void Actualizar(string mensaje)
    {
        try
        {
            File.AppendAllText(_nombreArchivo, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {mensaje}\n");
            Console.WriteLine($"[Archivo {_nombreArchivo}] Notificación guardada: {mensaje}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Archivo {_nombreArchivo}] Error al guardar: {ex.Message}");
        }
    }
}

public class ObservadorEmail : IObserver
{
    private readonly string _email;
    
    public ObservadorEmail(string email)
    {
        _email = email;
    }
    
    public void Actualizar(string mensaje)
    {
        Console.WriteLine($"[Email {_email}] Enviando notificación: {mensaje}");
        // Aquí iría la lógica real de envío de email
    }
}

// Sistema de notificaciones usando Observer
public class SistemaNotificaciones
{
    private readonly SujetoObservable _sujeto;
    private readonly List<IObserver> _observadores;
    
    public SistemaNotificaciones()
    {
        _sujeto = new SujetoObservable();
        _observadores = new List<IObserver>();
    }
    
    public void AgregarObservador(IObserver observador)
    {
        _observadores.Add(observador);
        _sujeto.RegistrarObservador(observador);
    }
    
    public void RemoverObservador(IObserver observador)
    {
        _observadores.Remove(observador);
        _sujeto.RemoverObservador(observador);
    }
    
    public void CambiarEstado(string nuevoEstado)
    {
        _sujeto.Estado = nuevoEstado;
    }
    
    public void RealizarAccion(string accion)
    {
        _sujeto.RealizarAccion(accion);
    }
    
    public void MostrarEstadisticas()
    {
        Console.WriteLine($"\n=== Estadísticas del Sistema ===");
        Console.WriteLine($"Observadores activos: {_observadores.Count}");
        Console.WriteLine($"Estado actual: {_sujeto.Estado}");
    }
}

// Uso del patrón Observer
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Demostración del Patrón Observer ===\n");
        
        var sistema = new SistemaNotificaciones();
        
        // Crear y registrar observadores
        var observadorConsola1 = new ObservadorConsola("Consola Principal");
        var observadorConsola2 = new ObservadorConsola("Consola Secundaria");
        var observadorArchivo = new ObservadorArchivo("notificaciones.log");
        var observadorEmail = new ObservadorEmail("admin@empresa.com");
        
        sistema.AgregarObservador(observadorConsola1);
        sistema.AgregarObservador(observadorConsola2);
        sistema.AgregarObservador(observadorArchivo);
        sistema.AgregarObservador(observadorEmail);
        
        // Cambiar estado y realizar acciones
        sistema.CambiarEstado("Iniciando");
        sistema.RealizarAccion("Cargar configuración");
        sistema.CambiarEstado("Ejecutando");
        sistema.RealizarAccion("Procesar datos");
        sistema.CambiarEstado("Completado");
        
        // Mostrar estadísticas
        sistema.MostrarEstadisticas();
        
        // Remover un observador
        Console.WriteLine("\n--- Removiendo observador ---");
        sistema.RemoverObservador(observadorConsola2);
        
        // Realizar acción sin el observador removido
        sistema.RealizarAccion("Finalizar proceso");
        
        // Mostrar estadísticas finales
        sistema.MostrarEstadisticas();
    }
}
```

**Explicación línea por línea:**
- `public interface IObserver`: Interfaz para observadores
- `void Actualizar(string mensaje)`: Método que se llama cuando el sujeto cambia
- `public interface ISujeto`: Interfaz para sujetos observables
- `void RegistrarObservador(IObserver observador)`: Método para registrar observadores
- `void RemoverObservador(IObserver observador)`: Método para remover observadores
- `void NotificarObservadores(string mensaje)`: Método para notificar a todos los observadores
- `public class SujetoObservable : ISujeto`: Implementación del sujeto observable
- `private readonly List<IObserver> _observadores = new List<IObserver>()`: Lista de observadores registrados
- `private string _estado`: Campo privado para el estado
- `public string Estado`: Propiedad para el estado con notificación automática
- `get => _estado`: Getter que retorna el estado
- `set`: Setter que cambia el estado y notifica
- `_estado = value`: Asigna el nuevo valor
- `NotificarObservadores($"Estado cambiado a: {_estado}")`: Notifica el cambio de estado
- `public void RegistrarObservador(IObserver observador)`: Implementa registro de observadores
- `if (!_observadores.Contains(observador))`: Verifica que no esté ya registrado
- `_observadores.Add(observador)`: Agrega el observador a la lista
- `Console.WriteLine($"Observador registrado: {observador.GetType().Name}")`: Confirma registro
- `public void RemoverObservador(IObserver observador)`: Implementa remoción de observadores
- `if (_observadores.Contains(observador))`: Verifica que esté registrado
- `_observadores.Remove(observador)`: Remueve el observador de la lista
- `Console.WriteLine($"Observador removido: {observador.GetType().Name}")`: Confirma remoción
- `public void NotificarObservadores(string mensaje)`: Implementa notificación a observadores
- `Console.WriteLine($"\nNotificando a {_observadores.Count} observadores...")`: Muestra conteo
- `foreach (var observador in _observadores)`: Itera sobre todos los observadores
- `observador.Actualizar(mensaje)`: Llama al método Actualizar de cada observador
- `public void RealizarAccion(string accion)`: Método para realizar acciones
- `Console.WriteLine($"\nRealizando acción: {accion}")`: Muestra la acción
- `NotificarObservadores($"Acción realizada: {accion}")`: Notifica la acción realizada
- `public class ObservadorConsola : IObserver`: Observador que muestra en consola
- `private readonly string _nombre`: Nombre del observador
- `public ObservadorConsola(string nombre)`: Constructor
- `_nombre = nombre`: Asigna el nombre
- `public void Actualizar(string mensaje)`: Implementa la actualización
- `Console.WriteLine($"[{_nombre}] Notificación recibida: {mensaje}")`: Muestra notificación
- `public class ObservadorArchivo : IObserver`: Observador que guarda en archivo
- `private readonly string _nombreArchivo`: Nombre del archivo
- `public ObservadorArchivo(string nombreArchivo)`: Constructor
- `_nombreArchivo = nombreArchivo`: Asigna el nombre del archivo
- `public void Actualizar(string mensaje)`: Implementa la actualización
- `try`: Bloque try-catch para manejo de errores
- `File.AppendAllText(_nombreArchivo, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {mensaje}\n")`: Escribe en archivo
- `Console.WriteLine($"[Archivo {_nombreArchivo}] Notificación guardada: {mensaje}")`: Confirma guardado
- `catch (Exception ex)`: Captura excepciones
- `Console.WriteLine($"[Archivo {_nombreArchivo}] Error al guardar: {ex.Message}")`: Muestra error
- `public class ObservadorEmail : IObserver`: Observador que simula envío de email
- `private readonly string _email`: Dirección de email
- `public ObservadorEmail(string email)`: Constructor
- `_email = email`: Asigna el email
- `public void Actualizar(string mensaje)`: Implementa la actualización
- `Console.WriteLine($"[Email {_email}] Enviando notificación: {mensaje}")`: Simula envío
- `public class SistemaNotificaciones`: Sistema que coordina el patrón Observer
- `private readonly SujetoObservable _sujeto`: Referencia al sujeto observable
- `private readonly List<IObserver> _observadores`: Lista de observadores del sistema
- `public SistemaNotificaciones()`: Constructor
- `_sujeto = new SujetoObservable()`: Crea nuevo sujeto observable
- `_observadores = new List<IObserver>()`: Inicializa lista de observadores
- `public void AgregarObservador(IObserver observador)`: Método para agregar observadores
- `_observadores.Add(observador)`: Agrega a la lista local
- `_sujeto.RegistrarObservador(observador)`: Registra en el sujeto observable
- `public void RemoverObservador(IObserver observador)`: Método para remover observadores
- `_observadores.Remove(observador)`: Remueve de la lista local
- `_sujeto.RemoverObservador(observador)`: Remueve del sujeto observable
- `public void CambiarEstado(string nuevoEstado)`: Método para cambiar estado
- `_sujeto.Estado = nuevoEstado`: Cambia el estado del sujeto
- `public void RealizarAccion(string accion)`: Método para realizar acciones
- `_sujeto.RealizarAccion(accion)`: Delega la acción al sujeto
- `public void MostrarEstadisticas()`: Método para mostrar estadísticas
- `Console.WriteLine($"\n=== Estadísticas del Sistema ===")`: Encabezado
- `Console.WriteLine($"Observadores activos: {_observadores.Count}")`: Muestra conteo
- `Console.WriteLine($"Estado actual: {_sujeto.Estado}")`: Muestra estado actual
- `var sistema = new SistemaNotificaciones()`: Crea instancia del sistema
- `var observadorConsola1 = new ObservadorConsola("Consola Principal")`: Crea primer observador de consola
- `var observadorConsola2 = new ObservadorConsola("Consola Secundaria")`: Crea segundo observador de consola
- `var observadorArchivo = new ObservadorArchivo("notificaciones.log")`: Crea observador de archivo
- `var observadorEmail = new ObservadorEmail("admin@empresa.com")`: Crea observador de email
- `sistema.AgregarObservador(observadorConsola1)`: Registra primer observador
- `sistema.AgregarObservador(observadorConsola2)`: Registra segundo observador
- `sistema.AgregarObservador(observadorArchivo)`: Registra observador de archivo
- `sistema.AgregarObservador(observadorEmail)`: Registra observador de email
- `sistema.CambiarEstado("Iniciando")`: Cambia estado a "Iniciando"
- `sistema.RealizarAccion("Cargar configuración")`: Realiza acción de cargar configuración
- `sistema.CambiarEstado("Ejecutando")`: Cambia estado a "Ejecutando"
- `sistema.RealizarAccion("Procesar datos")`: Realiza acción de procesar datos
- `sistema.CambiarEstado("Completado")`: Cambia estado a "Completado"
- `sistema.MostrarEstadisticas()`: Muestra estadísticas del sistema
- `Console.WriteLine("\n--- Removiendo observador ---")`: Muestra separador
- `sistema.RemoverObservador(observadorConsola2)`: Remueve segundo observador
- `sistema.RealizarAccion("Finalizar proceso")`: Realiza acción final
- `sistema.MostrarEstadisticas()`: Muestra estadísticas finales

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Sistema de Logging con Singleton
Implementa un sistema de logging usando el patrón Singleton que permita escribir mensajes en consola, archivo y base de datos.

### Ejercicio 2: Factory de Conexiones de Base de Datos
Crea un factory method para crear diferentes tipos de conexiones de base de datos (SQL Server, MySQL, PostgreSQL).

### Ejercicio 3: Sistema de Notificaciones con Observer
Implementa un sistema de notificaciones que permita a usuarios suscribirse a diferentes tipos de eventos (email, SMS, push).

## 🔍 Puntos Clave

1. **Los patrones de diseño** proporcionan soluciones reutilizables para problemas comunes
2. **El patrón Singleton** garantiza una única instancia de una clase
3. **El patrón Factory Method** delega la creación de objetos a subclases
4. **El patrón Observer** permite comunicación flexible entre objetos
5. **Los patrones** mejoran la mantenibilidad y flexibilidad del código

## 📚 Recursos Adicionales

- [Design Patterns - GoF](https://refactoring.guru/design-patterns)
- [Patrones de Diseño en C# - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/standard/design-patterns/)
- [Singleton Pattern - Wikipedia](https://en.wikipedia.org/wiki/Singleton_pattern)

---

**🎯 ¡Has completado la Clase 4! Ahora entiendes los patrones de diseño básicos en C#**

**📚 [Siguiente: Clase 5 - Principios SOLID](clase_5_principios_solid.md)**
