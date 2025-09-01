# Clase 3: Interfaces en C#

## 🎯 Objetivos de la Clase
- Comprender qué son las interfaces y su propósito
- Aprender a definir e implementar interfaces
- Entender la implementación múltiple de interfaces
- Dominar el uso de interfaces para desacoplamiento

## 📚 Contenido Teórico

### 1. ¿Qué son las Interfaces?

Una **interfaz** es un **contrato** que define qué métodos y propiedades debe implementar una clase. En C#, las interfaces son como "plantillas" que especifican el comportamiento que las clases deben tener, sin proporcionar la implementación.

#### Características de las Interfaces:
- **No tienen implementación**: Solo definen la "firma" de métodos y propiedades
- **No pueden tener campos**: Solo propiedades y métodos
- **No pueden tener constructores**: No se pueden instanciar directamente
- **Pueden tener implementación por defecto**: Desde C# 8.0
- **Permiten herencia múltiple**: Una clase puede implementar varias interfaces

### 2. Definición y Implementación Básica

#### 2.1 Sintaxis de Definición

```csharp
using System;

// Definir una interfaz
public interface IReproductor
{
    // Propiedades
    string Nombre { get; set; }
    bool EstaReproduciendo { get; }
    
    // Métodos (sin implementación)
    void Reproducir();
    void Pausar();
    void Detener();
    void Siguiente();
    void Anterior();
    
    // Método con implementación por defecto (C# 8.0+)
    void MostrarEstado()
    {
        Console.WriteLine($"Reproductor: {Nombre}");
        Console.WriteLine($"Estado: {(EstaReproduciendo ? "Reproduciendo" : "Detenido")}");
    }
}

// Otra interfaz para funcionalidades adicionales
public interface IVolumen
{
    int Volumen { get; set; }
    int VolumenMaximo { get; }
    
    void SubirVolumen();
    void BajarVolumen();
    void Silenciar();
}

// Interfaz para funcionalidades de playlist
public interface IPlaylist
{
    List<string> Canciones { get; set; }
    int CancionActual { get; set; }
    
    void AgregarCancion(string cancion);
    void EliminarCancion(string cancion);
    void MostrarPlaylist();
}
```

#### 2.2 Implementación de Interfaces

```csharp
// Clase que implementa una interfaz
public class ReproductorMP3 : IReproductor
{
    // Implementar propiedades de la interfaz
    public string Nombre { get; set; }
    public bool EstaReproduciendo { get; private set; }
    
    // Propiedades específicas de la clase
    public string ArchivoActual { get; private set; }
    public TimeSpan Duracion { get; private set; }
    
    // Constructor
    public ReproductorMP3(string nombre)
    {
        Nombre = nombre;
        EstaReproduciendo = false;
        ArchivoActual = "";
        Duracion = TimeSpan.Zero;
    }
    
    // Implementar métodos de la interfaz
    public void Reproducir()
    {
        if (string.IsNullOrEmpty(ArchivoActual))
        {
            Console.WriteLine("No hay archivo seleccionado para reproducir");
            return;
        }
        
        EstaReproduciendo = true;
        Console.WriteLine($"Reproduciendo: {ArchivoActual}");
    }
    
    public void Pausar()
    {
        if (EstaReproduciendo)
        {
            EstaReproduciendo = false;
            Console.WriteLine("Reproducción pausada");
        }
        else
        {
            Console.WriteLine("No hay nada reproduciéndose");
        }
    }
    
    public void Detener()
    {
        EstaReproduciendo = false;
        Console.WriteLine("Reproducción detenida");
    }
    
    public void Siguiente()
    {
        Console.WriteLine("Siguiente canción");
        // Lógica para cambiar a la siguiente canción
    }
    
    public void Anterior()
    {
        Console.WriteLine("Canción anterior");
        // Lógica para cambiar a la canción anterior
    }
    
    // Métodos específicos de la clase
    public void CargarArchivo(string archivo, TimeSpan duracion)
    {
        ArchivoActual = archivo;
        Duracion = duracion;
        Console.WriteLine($"Archivo cargado: {archivo} (Duración: {duracion:mm\\:ss})");
    }
    
    public void MostrarInformacion()
    {
        Console.WriteLine("=== REPRODUCTOR MP3 ===");
        MostrarEstado(); // Usar implementación por defecto de la interfaz
        Console.WriteLine($"Archivo: {ArchivoActual}");
        Console.WriteLine($"Duración: {Duracion:mm\\:ss}");
    }
}
```

#### 2.3 Implementación Múltiple de Interfaces

```csharp
// Clase que implementa múltiples interfaces
public class ReproductorAvanzado : IReproductor, IVolumen, IPlaylist
{
    // Propiedades de IReproductor
    public string Nombre { get; set; }
    public bool EstaReproduciendo { get; private set; }
    
    // Propiedades de IVolumen
    public int Volumen { get; set; }
    public int VolumenMaximo { get; } = 100;
    
    // Propiedades de IPlaylist
    public List<string> Canciones { get; set; }
    public int CancionActual { get; set; }
    
    // Propiedades específicas de la clase
    public string ArchivoActual { get; private set; }
    
    // Constructor
    public ReproductorAvanzado(string nombre)
    {
        Nombre = nombre;
        EstaReproduciendo = false;
        Volumen = 50;
        Canciones = new List<string>();
        CancionActual = 0;
        ArchivoActual = "";
    }
    
    // Implementación de IReproductor
    public void Reproducir()
    {
        if (Canciones.Count == 0)
        {
            Console.WriteLine("No hay canciones en la playlist");
            return;
        }
        
        EstaReproduciendo = true;
        ArchivoActual = Canciones[CancionActual];
        Console.WriteLine($"Reproduciendo: {ArchivoActual}");
    }
    
    public void Pausar()
    {
        EstaReproduciendo = false;
        Console.WriteLine("Reproducción pausada");
    }
    
    public void Detener()
    {
        EstaReproduciendo = false;
        Console.WriteLine("Reproducción detenida");
    }
    
    public void Siguiente()
    {
        if (Canciones.Count > 0)
        {
            CancionActual = (CancionActual + 1) % Canciones.Count;
            ArchivoActual = Canciones[CancionActual];
            Console.WriteLine($"Siguiente: {ArchivoActual}");
        }
    }
    
    public void Anterior()
    {
        if (Canciones.Count > 0)
        {
            CancionActual = (CancionActual - 1 + Canciones.Count) % Canciones.Count;
            ArchivoActual = Canciones[CancionActual];
            Console.WriteLine($"Anterior: {ArchivoActual}");
        }
    }
    
    // Implementación de IVolumen
    public void SubirVolumen()
    {
        if (Volumen < VolumenMaximo)
        {
            Volumen += 5;
            Console.WriteLine($"Volumen: {Volumen}");
        }
        else
        {
            Console.WriteLine("Volumen máximo alcanzado");
        }
    }
    
    public void BajarVolumen()
    {
        if (Volumen > 0)
        {
            Volumen -= 5;
            Console.WriteLine($"Volumen: {Volumen}");
        }
        else
        {
            Console.WriteLine("Volumen mínimo alcanzado");
        }
    }
    
    public void Silenciar()
    {
        Volumen = 0;
        Console.WriteLine("Reproductor silenciado");
    }
    
    // Implementación de IPlaylist
    public void AgregarCancion(string cancion)
    {
        Canciones.Add(cancion);
        Console.WriteLine($"Canción agregada: {cancion}");
    }
    
    public void EliminarCancion(string cancion)
    {
        if (Canciones.Remove(cancion))
        {
            Console.WriteLine($"Canción eliminada: {cancion}");
        }
        else
        {
            Console.WriteLine($"Canción no encontrada: {cancion}");
        }
    }
    
    public void MostrarPlaylist()
    {
        Console.WriteLine("=== PLAYLIST ===");
        if (Canciones.Count == 0)
        {
            Console.WriteLine("Playlist vacía");
            return;
        }
        
        for (int i = 0; i < Canciones.Count; i++)
        {
            string indicador = (i == CancionActual) ? "▶ " : "  ";
            Console.WriteLine($"{indicador}{i + 1}. {Canciones[i]}");
        }
    }
    
    // Método específico de la clase
    public void MostrarInformacionCompleta()
    {
        Console.WriteLine("=== REPRODUCTOR AVANZADO ===");
        MostrarEstado(); // De IReproductor
        Console.WriteLine($"Volumen: {Volumen}/{VolumenMaximo}");
        MostrarPlaylist();
    }
}
```

### 3. Uso de Interfaces para Desacoplamiento

#### 3.1 Ejemplo con Sistema de Pagos

```csharp
// Interfaz para métodos de pago
public interface IMetodoPago
{
    decimal Monto { get; set; }
    string Moneda { get; set; }
    
    bool ProcesarPago();
    string ObtenerConfirmacion();
    void MostrarDetalles();
}

// Interfaz para notificaciones
public interface INotificacion
{
    void EnviarNotificacion(string mensaje);
    bool NotificacionEnviada { get; }
}

// Clase que implementa ambas interfaces
public class PagoTarjeta : IMetodoPago, INotificacion
{
    public decimal Monto { get; set; }
    public string Moneda { get; set; }
    public bool NotificacionEnviada { get; private set; }
    
    // Propiedades específicas
    public string NumeroTarjeta { get; set; }
    public string Titular { get; set; }
    public DateTime FechaVencimiento { get; set; }
    
    public PagoTarjeta(decimal monto, string moneda, string numeroTarjeta, string titular, DateTime fechaVencimiento)
    {
        Monto = monto;
        Moneda = moneda;
        NumeroTarjeta = numeroTarjeta;
        Titular = titular;
        FechaVencimiento = fechaVencimiento;
        NotificacionEnviada = false;
    }
    
    // Implementación de IMetodoPago
    public bool ProcesarPago()
    {
        Console.WriteLine($"Procesando pago con tarjeta: {NumeroTarjeta.Substring(0, 4)}****");
        
        // Simular validación
        if (FechaVencimiento < DateTime.Now)
        {
            Console.WriteLine("Error: Tarjeta vencida");
            return false;
        }
        
        if (Monto <= 0)
        {
            Console.WriteLine("Error: Monto inválido");
            return false;
        }
        
        Console.WriteLine($"Pago procesado exitosamente: {Monto} {Moneda}");
        return true;
    }
    
    public string ObtenerConfirmacion()
    {
        return $"Pago confirmado - Tarjeta: {NumeroTarjeta.Substring(0, 4)}**** - Monto: {Monto} {Moneda}";
    }
    
    public void MostrarDetalles()
    {
        Console.WriteLine("=== DETALLES DEL PAGO ===");
        Console.WriteLine($"Monto: {Monto} {Moneda}");
        Console.WriteLine($"Tarjeta: {NumeroTarjeta.Substring(0, 4)}****");
        Console.WriteLine($"Titular: {Titular}");
        Console.WriteLine($"Vencimiento: {FechaVencimiento:MM/yyyy}");
    }
    
    // Implementación de INotificacion
    public void EnviarNotificacion(string mensaje)
    {
        Console.WriteLine($"Notificación enviada: {mensaje}");
        NotificacionEnviada = true;
    }
}

// Clase que usa interfaces para desacoplamiento
public class ProcesadorPagos
{
    // Método que acepta cualquier implementación de IMetodoPago
    public bool ProcesarPago(IMetodoPago metodoPago)
    {
        Console.WriteLine("=== PROCESANDO PAGO ===");
        
        bool resultado = metodoPago.ProcesarPago();
        
        if (resultado)
        {
            string confirmacion = metodoPago.ObtenerConfirmacion();
            Console.WriteLine($"Confirmación: {confirmacion}");
            
            // Si también implementa INotificacion, enviar notificación
            if (metodoPago is INotificacion notificacion)
            {
                notificacion.EnviarNotificacion($"Pago exitoso: {confirmacion}");
            }
        }
        
        return resultado;
    }
    
    // Método que procesa múltiples pagos
    public void ProcesarPagos(List<IMetodoPago> metodosPago)
    {
        Console.WriteLine($"\n=== PROCESANDO {metodosPago.Count} PAGOS ===");
        
        foreach (var metodoPago in metodosPago)
        {
            metodoPago.MostrarDetalles();
            bool resultado = ProcesarPago(metodoPago);
            Console.WriteLine($"Resultado: {(resultado ? "ÉXITO" : "FALLO")}");
            Console.WriteLine(new string('-', 40));
        }
    }
}
```

#### 3.2 Demostración del Desacoplamiento

```csharp
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== INTERFACES PARA DESACOPLAMIENTO ===");
        
        // Crear diferentes métodos de pago
        PagoTarjeta pagoTarjeta = new PagoTarjeta(150.50m, "USD", "1234567890123456", "Juan Pérez", new DateTime(2025, 12, 31));
        
        // Crear procesador de pagos
        ProcesadorPagos procesador = new ProcesadorPagos();
        
        // Procesar pago individual
        bool resultado = procesador.ProcesarPago(pagoTarjeta);
        
        // Crear lista de pagos para procesamiento en lote
        List<IMetodoPago> pagos = new List<IMetodoPago>
        {
            pagoTarjeta,
            // Aquí se podrían agregar otros métodos de pago que implementen IMetodoPago
        };
        
        // Procesar todos los pagos
        procesador.ProcesarPagos(pagos);
        
        // Demostrar que la interfaz permite flexibilidad
        Console.WriteLine("\n=== FLEXIBILIDAD DE INTERFACES ===");
        
        // Usar la referencia de interfaz
        IMetodoPago metodoPago = pagoTarjeta;
        metodoPago.MostrarDetalles();
        
        // Verificar si implementa otra interfaz
        if (metodoPago is INotificacion notificacion)
        {
            Console.WriteLine("Este método de pago también implementa notificaciones");
            notificacion.EnviarNotificacion("Notificación de prueba");
        }
    }
}
```

### 4. Interfaces con Implementación por Defecto

#### 4.1 Características de C# 8.0+

```csharp
public interface ILogger
{
    // Propiedades
    string Nombre { get; set; }
    bool Habilitado { get; set; }
    
    // Métodos abstractos (deben ser implementados)
    void Log(string mensaje);
    void LogError(string mensaje, Exception ex);
    
    // Métodos con implementación por defecto
    void LogInfo(string mensaje)
    {
        if (Habilitado)
        {
            Log($"[INFO] {mensaje}");
        }
    }
    
    void LogWarning(string mensaje)
    {
        if (Habilitado)
        {
            Log($"[WARNING] {mensaje}");
        }
    }
    
    void LogDebug(string mensaje)
    {
        if (Habilitado)
        {
            Log($"[DEBUG] {mensaje}");
        }
    }
    
    // Método con implementación por defecto que usa otros métodos
    void LogMultiple(params string[] mensajes)
    {
        foreach (string mensaje in mensajes)
        {
            LogInfo(mensaje);
        }
    }
}

// Clase que implementa la interfaz
public class LoggerConsola : ILogger
{
    public string Nombre { get; set; }
    public bool Habilitado { get; set; }
    
    public LoggerConsola(string nombre)
    {
        Nombre = nombre;
        Habilitado = true;
    }
    
    // Solo necesitamos implementar los métodos abstractos
    public void Log(string mensaje)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {Nombre}: {mensaje}");
    }
    
    public void LogError(string mensaje, Exception ex)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {Nombre}: ERROR - {mensaje}");
        Console.WriteLine($"Excepción: {ex.Message}");
    }
    
    // Los métodos con implementación por defecto se heredan automáticamente
    // Podemos sobrescribirlos si queremos comportamiento personalizado
    public void LogWarning(string mensaje)
    {
        if (Habilitado)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {Nombre}: ⚠️  {mensaje}");
        }
    }
}
```

### 5. Herencia de Interfaces

#### 5.1 Interfaces que Heredan de Otras Interfaces

```csharp
// Interfaz base
public interface IAnimal
{
    string Nombre { get; set; }
    int Edad { get; set; }
    
    void Comer();
    void Dormir();
}

// Interfaz que hereda de IAnimal
public interface IMamifero : IAnimal
{
    bool TienePelo { get; set; }
    
    void Amamantar();
}

// Interfaz que hereda de IMamifero
public interface IPerro : IMamifero
{
    string Raza { get; set; }
    
    void Ladrar();
    void Jugar();
}

// Clase que implementa la interfaz más específica
public class Perro : IPerro
{
    public string Nombre { get; set; }
    public int Edad { get; set; }
    public bool TienePelo { get; set; }
    public string Raza { get; set; }
    
    public Perro(string nombre, int edad, string raza)
    {
        Nombre = nombre;
        Edad = edad;
        TienePelo = true;
        Raza = raza;
    }
    
    // Implementar todos los métodos de todas las interfaces
    public void Comer()
    {
        Console.WriteLine($"{Nombre} está comiendo croquetas");
    }
    
    public void Dormir()
    {
        Console.WriteLine($"{Nombre} está durmiendo");
    }
    
    public void Amamantar()
    {
        Console.WriteLine($"{Nombre} está amamantando");
    }
    
    public void Ladrar()
    {
        Console.WriteLine($"{Nombre} dice: ¡Guau! ¡Guau!");
    }
    
    public void Jugar()
    {
        Console.WriteLine($"{Nombre} está jugando con su pelota");
    }
    
    public void MostrarInformacion()
    {
        Console.WriteLine("=== INFORMACIÓN DEL PERRO ===");
        Console.WriteLine($"Nombre: {Nombre}");
        Console.WriteLine($"Edad: {Edad} años");
        Console.WriteLine($"Raza: {Raza}");
        Console.WriteLine($"Tiene pelo: {TienePelo}");
    }
}
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Sistema de Notificaciones con Interfaces
```csharp
// Crear interfaces para diferentes tipos de notificaciones
public interface INotificacion
{
    string Mensaje { get; set; }
    DateTime FechaEnvio { get; set; }
    
    void Enviar();
    bool ConfirmarEnvio();
}

// Implementar: NotificacionEmail, NotificacionSMS, NotificacionPush
// Cada una con su lógica específica de envío
```

### Ejercicio 2: Sistema de Almacenamiento
```csharp
// Crear interfaces para diferentes tipos de almacenamiento
public interface IAlmacenamiento
{
    string Nombre { get; set; }
    long Capacidad { get; set; }
    
    bool Guardar(string ruta, byte[] datos);
    byte[] Leer(string ruta);
    bool Eliminar(string ruta);
    bool Existe(string ruta);
}

// Implementar: AlmacenamientoArchivo, AlmacenamientoMemoria, AlmacenamientoBaseDatos
```

### Ejercicio 3: Sistema de Validación
```csharp
// Crear interfaces para validación de datos
public interface IValidador<T>
{
    bool EsValido(T item);
    List<string> ObtenerErrores();
    void Validar(T item);
}

// Implementar: ValidadorUsuario, ValidadorProducto, ValidadorPedido
```

## 🔍 Conceptos Importantes a Recordar

1. **Las interfaces definen contratos** que las clases deben cumplir
2. **No tienen implementación** (excepto métodos por defecto en C# 8.0+)
3. **Permiten herencia múltiple** de interfaces
4. **Facilitan el desacoplamiento** entre componentes
5. **Se pueden usar como tipos** para referencias polimórficas
6. **Las interfaces pueden heredar** de otras interfaces
7. **Los métodos por defecto** proporcionan implementación común
8. **El casting con `is`** permite verificar implementaciones múltiples

## ❓ Preguntas de Repaso

1. ¿Cuál es la diferencia entre una interfaz y una clase abstracta?
2. ¿Por qué son útiles las interfaces para el desacoplamiento?
3. ¿Cuándo usarías implementación por defecto en interfaces?
4. ¿Cómo funciona la herencia múltiple con interfaces?
5. ¿Qué ventajas tiene usar interfaces como tipos de referencia?

## 🚀 Siguiente Paso

En la próxima clase aprenderemos sobre **Clases Abstractas en C#**, donde veremos cómo crear clases base que no se pueden instanciar pero pueden tener implementación.

---

## 📚 Navegación del Módulo 2

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_herencia.md) | Herencia en C# | |
| [Clase 2](clase_2_polimorfismo.md) | Polimorfismo y Métodos Virtuales | ← Anterior |
| **Clase 3** | **Interfaces en C#** | ← Estás aquí |
| [Clase 4](clase_4_clases_abstractas.md) | Clases Abstractas | Siguiente → |
| [Clase 5](clase_5_genericos.md) | Genéricos en C# | |
| [Clase 6](clase_6_delegados_eventos.md) | Delegados y Eventos | |
| [Clase 7](clase_7_linq.md) | LINQ en C# | |
| [Clase 8](clase_8_archivos_streams.md) | Manejo de Archivos y Streams | |
| [Clase 9](clase_9_programacion_asincrona.md) | Programación Asíncrona | |
| [Clase 10](clase_10_reflexion_metaprogramacion.md) | Reflexión y Metaprogramación | |

**← [Volver al README del Módulo 2](../junior_2/README.md)**

---

## 📚 Recursos Adicionales

- [Interfaces en C#](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/interfaces/)
- [Implementación por defecto en interfaces](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/default-interface-methods)
- [Patrones de diseño con interfaces](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/interface)

---

**¡Excelente! Ahora entiendes las interfaces en C#! 🎯**
