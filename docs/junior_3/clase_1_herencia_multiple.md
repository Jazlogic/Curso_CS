# 🚀 Clase 1: Herencia Múltiple y Composición

## 📋 Información de la Clase

- **Módulo**: Junior Level 3 - Programación Orientada a Objetos Avanzada
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Módulo 2 (POO Básica)

## 🎯 Objetivos de Aprendizaje

- Entender las limitaciones de herencia múltiple en C#
- Implementar múltiples interfaces en una sola clase
- Dominar el concepto de composición vs herencia
- Crear sistemas flexibles usando composición
- Resolver conflictos de nombres en interfaces múltiples

---

## 📚 Navegación del Módulo 3

| Clase | Tema | Enlace |
|-------|------|--------|
| **Clase 1** | **Herencia Múltiple y Composición** | ← Estás aquí |
| [Clase 2](clase_2_interfaces_avanzadas.md) | Interfaces Avanzadas | Siguiente → |
| [Clase 3](clase_3_polimorfismo_avanzado.md) | Polimorfismo Avanzado | |
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

### 1. Limitaciones de Herencia Múltiple en C#

C# no permite herencia múltiple de clases, pero sí permite implementar múltiples interfaces. Esto es una limitación intencional para evitar la complejidad del "diamond problem".

```csharp
// ❌ NO PERMITIDO en C#
public class MiClase : ClaseA, ClaseB  // Error de compilación
{
    // Código de la clase
}

// ✅ PERMITIDO en C#
public class MiClase : ClaseA, IInterfaz1, IInterfaz2
{
    // Código de la clase
}
```

**Explicación línea por línea:**
- `public class MiClase`: Declara una clase pública llamada MiClase
- `: ClaseA`: Hereda de la clase base ClaseA (solo una clase base permitida)
- `, IInterfaz1, IInterfaz2`: Implementa múltiples interfaces separadas por comas
- `{ }`: Define el cuerpo de la clase

### 2. Implementación de Múltiples Interfaces

```csharp
// Definición de interfaces
public interface ILogeable
{
    void Log(string mensaje);
}

public interface ISerializable
{
    string Serializar();
    void Deserializar(string datos);
}

public interface IValidable
{
    bool EsValido();
}

// Clase que implementa múltiples interfaces
public class Usuario : ILogeable, ISerializable, IValidable
{
    public string Nombre { get; set; }
    public string Email { get; set; }
    
    // Implementación de ILogeable
    public void Log(string mensaje)
    {
        Console.WriteLine($"[LOG] Usuario {Nombre}: {mensaje}");
    }
    
    // Implementación de ISerializable
    public string Serializar()
    {
        return $"{Nombre}|{Email}";
    }
    
    public void Deserializar(string datos)
    {
        var partes = datos.Split('|');
        if (partes.Length == 2)
        {
            Nombre = partes[0];
            Email = partes[1];
        }
    }
    
    // Implementación de IValidable
    public bool EsValido()
    {
        return !string.IsNullOrEmpty(Nombre) && 
               !string.IsNullOrEmpty(Email) && 
               Email.Contains("@");
    }
}
```

**Explicación línea por línea:**
- `public interface ILogeable`: Define una interfaz para funcionalidad de logging
- `void Log(string mensaje)`: Método que debe implementar cualquier clase que use esta interfaz
- `public class Usuario : ILogeable, ISerializable, IValidable`: La clase Usuario implementa tres interfaces
- `public void Log(string mensaje)`: Implementa el método de ILogeable
- `Console.WriteLine($"[LOG] Usuario {Nombre}: {mensaje}")`: Escribe un mensaje de log formateado
- `public string Serializar()`: Implementa el método de ISerializable
- `return $"{Nombre}|{Email}"`: Retorna los datos del usuario separados por pipe
- `public void Deserializar(string datos)`: Implementa el método de deserialización
- `var partes = datos.Split('|')`: Divide la cadena por el separador pipe
- `if (partes.Length == 2)`: Verifica que hay exactamente 2 partes
- `Nombre = partes[0]`: Asigna la primera parte como nombre
- `Email = partes[1]`: Asigna la segunda parte como email
- `public bool EsValido()`: Implementa el método de IValidable
- `return !string.IsNullOrEmpty(Nombre) && ...`: Retorna true si todos los campos son válidos

### 3. Composición vs Herencia

La composición es preferible a la herencia porque proporciona mayor flexibilidad y evita problemas de acoplamiento.

```csharp
// ❌ Herencia (menos flexible)
public class AutoElectrico : Auto
{
    public int CapacidadBateria { get; set; }
    public void Cargar() { /* lógica de carga */ }
}

// ✅ Composición (más flexible)
public class Auto
{
    public Motor Motor { get; set; }
    public SistemaFrenos SistemaFrenos { get; set; }
    public List<Componente> Componentes { get; set; }
    
    public Auto()
    {
        Componentes = new List<Componente>();
    }
    
    public void AgregarComponente(Componente componente)
    {
        Componentes.Add(componente);
    }
}

public class Motor
{
    public int Potencia { get; set; }
    public string TipoCombustible { get; set; }
    
    public void Encender()
    {
        Console.WriteLine("Motor encendido");
    }
}

public class MotorElectrico : Motor
{
    public int CapacidadBateria { get; set; }
    
    public void Cargar()
    {
        Console.WriteLine($"Cargando batería de {CapacidadBateria} kWh");
    }
}

public class Componente
{
    public string Nombre { get; set; }
    public decimal Precio { get; set; }
}
```

**Explicación línea por línea:**
- `public class AutoElectrico : Auto`: Herencia directa (menos flexible)
- `public int CapacidadBateria`: Propiedad específica del auto eléctrico
- `public class Auto`: Clase base que usa composición
- `public Motor Motor`: Propiedad que contiene un motor (composición)
- `public SistemaFrenos SistemaFrenos`: Propiedad que contiene el sistema de frenos
- `public List<Componente> Componentes`: Lista de componentes adicionales
- `public Auto()`: Constructor que inicializa la lista de componentes
- `Componentes = new List<Componente>()`: Crea una nueva lista vacía
- `public void AgregarComponente(Componente componente)`: Método para agregar componentes
- `Componentes.Add(componente)`: Agrega el componente a la lista
- `public class Motor`: Clase base para motores
- `public int Potencia`: Propiedad de potencia del motor
- `public string TipoCombustible`: Tipo de combustible que usa
- `public void Encender()`: Método para encender el motor
- `Console.WriteLine("Motor encendido")`: Mensaje de confirmación
- `public class MotorElectrico : Motor`: Motor específico que hereda de Motor
- `public int CapacidadBateria`: Capacidad de la batería en kWh
- `public void Cargar()`: Método específico para cargar la batería
- `Console.WriteLine($"Cargando batería de {CapacidadBateria} kWh")`: Mensaje informativo
- `public class Componente`: Clase base para componentes genéricos
- `public string Nombre`: Nombre del componente
- `public decimal Precio`: Precio del componente

### 4. Resolución de Conflictos de Nombres

Cuando implementas múltiples interfaces, pueden surgir conflictos de nombres que se resuelven usando implementación explícita.

```csharp
public interface ILogger
{
    void Log(string mensaje);
}

public interface IFileLogger
{
    void Log(string mensaje);
}

public class LoggerAvanzado : ILogger, IFileLogger
{
    // Implementación implícita para ILogger
    public void Log(string mensaje)
    {
        Console.WriteLine($"Console: {mensaje}");
    }
    
    // Implementación explícita para IFileLogger
    void IFileLogger.Log(string mensaje)
    {
        File.WriteAllText("log.txt", $"File: {mensaje}");
    }
}

// Uso de implementaciones explícitas
public class Program
{
    public static void Main()
    {
        var logger = new LoggerAvanzado();
        
        // Llama a la implementación implícita
        logger.Log("Mensaje normal");
        
        // Llama a la implementación explícita
        ((IFileLogger)logger).Log("Mensaje de archivo");
        
        // También se puede usar así:
        IFileLogger fileLogger = logger;
        fileLogger.Log("Otro mensaje de archivo");
    }
}
```

**Explicación línea por línea:**
- `public interface ILogger`: Primera interfaz con método Log
- `void Log(string mensaje)`: Método que debe implementarse
- `public interface IFileLogger`: Segunda interfaz con método Log del mismo nombre
- `public class LoggerAvanzado : ILogger, IFileLogger`: Clase que implementa ambas interfaces
- `public void Log(string mensaje)`: Implementación implícita para ILogger
- `Console.WriteLine($"Console: {mensaje}")`: Escribe en consola
- `void IFileLogger.Log(string mensaje)`: Implementación explícita para IFileLogger
- `File.WriteAllText("log.txt", $"File: {mensaje}")`: Escribe en archivo
- `var logger = new LoggerAvanzado()`: Crea instancia de la clase
- `logger.Log("Mensaje normal")`: Llama implementación implícita
- `((IFileLogger)logger).Log("Mensaje de archivo")`: Llama implementación explícita con cast
- `IFileLogger fileLogger = logger`: Crea referencia de tipo interfaz
- `fileLogger.Log("Otro mensaje de archivo")`: Llama implementación explícita

### 5. Mixins y Traits en C#

Aunque C# no tiene mixins nativos, podemos simularlos usando interfaces y métodos de extensión.

```csharp
// Interfaz que actúa como mixin
public interface ICacheable
{
    DateTime FechaCreacion { get; }
    bool HaExpirado(int minutosExpiracion);
}

// Implementación por defecto usando método de extensión
public static class CacheableExtensions
{
    public static bool HaExpirado(this ICacheable cacheable, int minutosExpiracion)
    {
        return DateTime.Now > cacheable.FechaCreacion.AddMinutes(minutosExpiracion);
    }
    
    public static void Renovar(this ICacheable cacheable)
    {
        // Lógica para renovar el cache
        Console.WriteLine("Cache renovado");
    }
}

// Clase que usa el mixin
public class UsuarioCache : ICacheable
{
    public string Nombre { get; set; }
    public DateTime FechaCreacion { get; set; }
    
    public UsuarioCache(string nombre)
    {
        Nombre = nombre;
        FechaCreacion = DateTime.Now;
    }
}

// Uso del mixin
public class Program
{
    public static void Main()
    {
        var usuario = new UsuarioCache("Juan");
        
        // Usa métodos del mixin
        Console.WriteLine($"¿Ha expirado? {usuario.HaExpirado(30)}");
        usuario.Renovar();
        
        // Espera un poco y verifica expiración
        Thread.Sleep(1000);
        Console.WriteLine($"¿Ha expirado después de 1 segundo? {usuario.HaExpirado(0)}");
    }
}
```

**Explicación línea por línea:**
- `public interface ICacheable`: Interfaz que define funcionalidad de cache
- `DateTime FechaCreacion`: Propiedad para la fecha de creación
- `bool HaExpirado(int minutosExpiracion)`: Método para verificar expiración
- `public static class CacheableExtensions`: Clase estática para métodos de extensión
- `public static bool HaExpirado(this ICacheable cacheable, int minutosExpiracion)`: Método de extensión
- `this ICacheable cacheable`: Palabra clave this para indicar método de extensión
- `return DateTime.Now > cacheable.FechaCreacion.AddMinutes(minutosExpiracion)`: Compara fechas
- `public static void Renovar(this ICacheable cacheable)`: Método de extensión para renovar
- `Console.WriteLine("Cache renovado")`: Mensaje de confirmación
- `public class UsuarioCache : ICacheable`: Clase que implementa el mixin
- `public string Nombre`: Propiedad del usuario
- `public DateTime FechaCreacion`: Implementa la propiedad del mixin
- `public UsuarioCache(string nombre)`: Constructor que inicializa propiedades
- `Nombre = nombre`: Asigna el nombre recibido
- `FechaCreacion = DateTime.Now`: Establece la fecha actual
- `var usuario = new UsuarioCache("Juan")`: Crea instancia del usuario
- `Console.WriteLine($"¿Ha expirado? {usuario.HaExpirado(30)}")`: Usa método del mixin
- `usuario.Renovar()`: Llama método de extensión del mixin
- `Thread.Sleep(1000)`: Pausa la ejecución por 1 segundo
- `Console.WriteLine($"¿Ha expirado después de 1 segundo? {usuario.HaExpirado(0)}")`: Verifica expiración

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Sistema de Notificaciones
Crea un sistema que permita a diferentes tipos de usuarios (Admin, Usuario, Moderador) implementar múltiples interfaces de notificación (Email, SMS, Push).

### Ejercicio 2: Sistema de Persistencia
Implementa un sistema que permita a entidades ser persistidas en diferentes formatos (JSON, XML, Base de Datos) usando composición.

### Ejercicio 3: Logger Universal
Crea un logger que pueda escribir en múltiples destinos (Consola, Archivo, Base de Datos) implementando múltiples interfaces.

## 🔍 Puntos Clave

1. **C# no permite herencia múltiple de clases**, solo de interfaces
2. **La composición es preferible a la herencia** para mayor flexibilidad
3. **Las implementaciones explícitas** resuelven conflictos de nombres
4. **Los métodos de extensión** simulan funcionalidad de mixins
5. **Las interfaces múltiples** proporcionan contratos claros y flexibles

## 📚 Recursos Adicionales

- [Interfaces en C# - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/interfaces/)
- [Composición vs Herencia - Wikipedia](https://en.wikipedia.org/wiki/Composition_over_inheritance)
- [Design Patterns - GoF](https://refactoring.guru/design-patterns)

---

**🎯 ¡Has completado la Clase 1! Ahora entiendes la herencia múltiple y composición en C#**

**📚 [Siguiente: Clase 2 - Interfaces Avanzadas](clase_2_interfaces_avanzadas.md)**
