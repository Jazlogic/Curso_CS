# Clase 6: Delegados y Eventos en C#

## 🎯 Objetivos de la Clase
- Comprender qué son los delegados y su propósito
- Aprender a crear y usar delegados
- Entender el sistema de eventos en C#
- Dominar la programación orientada a eventos

## 📚 Contenido Teórico

### 1. ¿Qué son los Delegados?

Un **delegado** es un **tipo de referencia** que puede contener referencias a métodos con una firma específica. Los delegados son como "punteros a funciones" que permiten pasar métodos como parámetros y almacenar referencias a métodos.

#### Características de los Delegados:
- **Type safety**: Solo pueden referenciar métodos con la firma correcta
- **Multicast**: Pueden referenciar múltiples métodos
- **Inmutables**: Una vez creados, no se pueden modificar
- **Sincrónicos**: Los métodos se ejecutan en secuencia
- **Reutilizables**: Se pueden usar en diferentes contextos

### 2. Delegados Básicos

#### 2.1 Definición y Uso

```csharp
using System;

// Definir un delegado
public delegate void OperacionMatematica(int a, int b);

// Otro delegado con retorno
public delegate int CalculoMatematico(int a, int b);

// Delegado para operaciones de string
public delegate string OperacionString(string texto);

// Delegado para operaciones sin parámetros
public delegate void OperacionSinParametros();

// Clase que demuestra el uso de delegados
public class Calculadora
{
    // Métodos que coinciden con la firma del delegado
    public static void Sumar(int a, int b)
    {
        Console.WriteLine($"Suma: {a} + {b} = {a + b}");
    }
    
    public static void Restar(int a, int b)
    {
        Console.WriteLine($"Resta: {a} - {b} = {a - b}");
    }
    
    public static void Multiplicar(int a, int b)
    {
        Console.WriteLine($"Multiplicación: {a} * {b} = {a * b}");
    }
    
    public static void Dividir(int a, int b)
    {
        if (b != 0)
        {
            Console.WriteLine($"División: {a} / {b} = {a / b}");
        }
        else
        {
            Console.WriteLine("Error: División por cero");
        }
    }
    
    // Métodos que coinciden con CalculoMatematico
    public static int CalcularSuma(int a, int b)
    {
        return a + b;
    }
    
    public static int CalcularResta(int a, int b)
    {
        return a - b;
    }
    
    public static int CalcularMaximo(int a, int b)
    {
        return Math.Max(a, b);
    }
    
    // Métodos que coinciden con OperacionString
    public static string ConvertirMayusculas(string texto)
    {
        return texto.ToUpper();
    }
    
    public static string ConvertirMinusculas(string texto)
    {
        return texto.ToLower();
    }
    
    public static string InvertirTexto(string texto)
    {
        char[] caracteres = texto.ToCharArray();
        Array.Reverse(caracteres);
        return new string(caracteres);
    }
    
    // Método que usa delegados como parámetros
    public static void EjecutarOperacion(OperacionMatematica operacion, int a, int b)
    {
        Console.WriteLine($"Ejecutando operación con {a} y {b}:");
        operacion(a, b);
    }
    
    // Método que usa delegados con retorno
    public static void EjecutarCalculo(CalculoMatematico calculo, int a, int b)
    {
        int resultado = calculo(a, b);
        Console.WriteLine($"Resultado del cálculo: {resultado}");
    }
    
    // Método que combina múltiples operaciones
    public static void EjecutarTodasLasOperaciones(int a, int b)
    {
        // Crear delegado multicast
        OperacionMatematica todasLasOperaciones = Sumar;
        todasLasOperaciones += Restar;
        todasLasOperaciones += Multiplicar;
        todasLasOperaciones += Dividir;
        
        Console.WriteLine("=== EJECUTANDO TODAS LAS OPERACIONES ===");
        todasLasOperaciones(a, b);
    }
}

// Programa principal que demuestra delegados
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== DELEGADOS EN C# ===");
        
        // Crear instancias de delegados
        OperacionMatematica operacion = Calculadora.Sumar;
        CalculoMatematico calculo = Calculadora.CalcularSuma;
        OperacionString operacionString = Calculadora.ConvertirMayusculas;
        
        // Usar delegados
        Console.WriteLine("\n=== USO BÁSICO DE DELEGADOS ===");
        operacion(10, 5);
        int resultado = calculo(10, 5);
        Console.WriteLine($"Resultado: {resultado}");
        
        string textoMayusculas = operacionString("hola mundo");
        Console.WriteLine($"Texto en mayúsculas: {textoMayusculas}");
        
        // Cambiar la referencia del delegado
        Console.WriteLine("\n=== CAMBIANDO REFERENCIAS ===");
        operacion = Calculadora.Restar;
        operacion(10, 5);
        
        operacionString = Calculadora.InvertirTexto;
        string textoInvertido = operacionString("hola mundo");
        Console.WriteLine($"Texto invertido: {textoInvertido}");
        
        // Usar métodos que aceptan delegados
        Console.WriteLine("\n=== DELEGADOS COMO PARÁMETROS ===");
        Calculadora.EjecutarOperacion(Calculadora.Multiplicar, 6, 7);
        Calculadora.EjecutarCalculo(Calculadora.CalcularMaximo, 15, 8);
        
        // Delegados multicast
        Console.WriteLine("\n=== DELEGADOS MULTICAST ===");
        OperacionMatematica operaciones = Calculadora.Sumar;
        operaciones += Calculadora.Restar;
        operaciones += Calculadora.Multiplicar;
        
        Console.WriteLine("Ejecutando múltiples operaciones:");
        operaciones(20, 4);
        
        // Ejecutar todas las operaciones
        Calculadora.EjecutarTodasLasOperaciones(12, 3);
    }
}
```

### 3. Delegados Func y Action

#### 3.1 Delegados Predefinidos de .NET

```csharp
using System;

// Clase que demuestra el uso de delegados predefinidos
public class DemostracionDelegadosPredefinidos
{
    // Métodos que coinciden con Action (sin retorno)
    public static void Saludar(string nombre)
    {
        Console.WriteLine($"¡Hola {nombre}!");
    }
    
    public static void Despedir(string nombre)
    {
        Console.WriteLine($"¡Adiós {nombre}!");
    }
    
    public static void MostrarInformacion(string nombre, int edad)
    {
        Console.WriteLine($"Nombre: {nombre}, Edad: {edad}");
    }
    
    // Métodos que coinciden con Func (con retorno)
    public static int ElevarAlCuadrado(int numero)
    {
        return numero * numero;
    }
    
    public static string FormatearNumero(int numero)
    {
        return $"El número es: {numero}";
    }
    
    public static bool EsPar(int numero)
    {
        return numero % 2 == 0;
    }
    
    public static double CalcularPromedio(int a, int b, int c)
    {
        return (a + b + c) / 3.0;
    }
    
    // Método que usa Action
    public static void ProcesarNombres(Action<string> accion, string[] nombres)
    {
        foreach (string nombre in nombres)
        {
            accion(nombre);
        }
    }
    
    // Método que usa Func
    public static void ProcesarNumeros(Func<int, int> funcion, int[] numeros)
    {
        foreach (int numero in numeros)
        {
            int resultado = funcion(numero);
            Console.WriteLine($"f({numero}) = {resultado}");
        }
    }
    
    // Método que usa Predicate (equivalente a Func<T, bool>)
    public static List<int> FiltrarNumeros(Predicate<int> predicado, int[] numeros)
    {
        List<int> resultado = new List<int>();
        foreach (int numero in numeros)
        {
            if (predicado(numero))
            {
                resultado.Add(numero);
            }
        }
        return resultado;
    }
}

// Clase que demuestra el uso práctico
public class UsoDelegadosPredefinidos
{
    public static void Ejecutar()
    {
        Console.WriteLine("=== DELEGADOS PREDEFINIDOS ===");
        
        // Action (sin retorno)
        Console.WriteLine("\n=== ACTION ===");
        Action<string> saludar = DemostracionDelegadosPredefinidos.Saludar;
        Action<string> despedir = DemostracionDelegadosPredefinidos.Despedir;
        
        saludar("Juan");
        despedir("María");
        
        // Action con múltiples parámetros
        Action<string, int> mostrarInfo = DemostracionDelegadosPredefinidos.MostrarInformacion;
        mostrarInfo("Carlos", 25);
        
        // Func (con retorno)
        Console.WriteLine("\n=== FUNC ===");
        Func<int, int> elevarCuadrado = DemostracionDelegadosPredefinidos.ElevarAlCuadrado;
        Func<int, string> formatear = DemostracionDelegadosPredefinidos.FormatearNumero;
        Func<int, bool> esPar = DemostracionDelegadosPredefinidos.EsPar;
        Func<int, int, int, double> promedio = DemostracionDelegadosPredefinidos.CalcularPromedio;
        
        int cuadrado = elevarCuadrado(5);
        Console.WriteLine($"5 al cuadrado: {cuadrado}");
        
        string formato = formatear(42);
        Console.WriteLine(formato);
        
        bool par = esPar(10);
        Console.WriteLine($"¿10 es par? {par}");
        
        double prom = promedio(10, 20, 30);
        Console.WriteLine($"Promedio: {prom}");
        
        // Usar delegados como parámetros
        Console.WriteLine("\n=== DELEGADOS COMO PARÁMETROS ===");
        string[] nombres = { "Ana", "Luis", "Elena", "Pedro" };
        int[] numeros = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        
        DemostracionDelegadosPredefinidos.ProcesarNombres(saludar, nombres);
        DemostracionDelegadosPredefinidos.ProcesarNumeros(elevarCuadrado, numeros);
        
        // Filtrar números usando Predicate
        List<int> numerosPares = DemostracionDelegadosPredefinidos.FiltrarNumeros(esPar, numeros);
        Console.WriteLine($"Números pares: {string.Join(", ", numerosPares)}");
        
        // Usar lambda expressions con delegados
        Console.WriteLine("\n=== LAMBDA EXPRESSIONS ===");
        Action<string> imprimir = (texto) => Console.WriteLine($"Lambda: {texto}");
        Func<int, int> triple = (numero) => numero * 3;
        Predicate<int> esMayorQue5 = (numero) => numero > 5;
        
        imprimir("Hola desde lambda");
        Console.WriteLine($"Triple de 7: {triple(7)}");
        
        List<int> mayoresQue5 = DemostracionDelegadosPredefinidos.FiltrarNumeros(esMayorQue5, numeros);
        Console.WriteLine($"Números mayores que 5: {string.Join(", ", mayoresQue5)}");
    }
}
```

### 4. Eventos

#### 4.1 Sistema de Eventos Básico

```csharp
using System;

// Clase que define un evento
public class SensorTemperatura
{
    // Campo privado para el evento
    private decimal temperatura;
    
    // Propiedad pública para la temperatura
    public decimal Temperatura
    {
        get { return temperatura; }
        set
        {
            decimal temperaturaAnterior = temperatura;
            temperatura = value;
            
            // Disparar eventos cuando cambia la temperatura
            if (temperatura != temperaturaAnterior)
            {
                OnTemperaturaCambiada(temperaturaAnterior, temperatura);
                
                // Verificar si la temperatura está fuera del rango normal
                if (temperatura > 30)
                {
                    OnTemperaturaAlta(temperatura);
                }
                else if (temperatura < 10)
                {
                    OnTemperaturaBaja(temperatura);
                }
            }
        }
    }
    
    // Eventos
    public event Action<decimal, decimal> TemperaturaCambiada;
    public event Action<decimal> TemperaturaAlta;
    public event Action<decimal> TemperaturaBaja;
    public event Action<string> MensajeGenerado;
    
    // Métodos protegidos para disparar eventos
    protected virtual void OnTemperaturaCambiada(decimal temperaturaAnterior, decimal temperaturaNueva)
    {
        TemperaturaCambiada?.Invoke(temperaturaAnterior, temperaturaNueva);
    }
    
    protected virtual void OnTemperaturaAlta(decimal temperatura)
    {
        TemperaturaAlta?.Invoke(temperatura);
        MensajeGenerado?.Invoke($"¡ALERTA! Temperatura alta: {temperatura}°C");
    }
    
    protected virtual void OnTemperaturaBaja(decimal temperatura)
    {
        TemperaturaBaja?.Invoke(temperatura);
        MensajeGenerado?.Invoke($"¡ALERTA! Temperatura baja: {temperatura}°C");
    }
    
    // Método para simular cambios de temperatura
    public void SimularCambioTemperatura(decimal nuevaTemperatura)
    {
        Console.WriteLine($"Cambiando temperatura de {Temperatura}°C a {nuevaTemperatura}°C");
        Temperatura = nuevaTemperatura;
    }
}

// Clase que maneja los eventos del sensor
public class MonitorTemperatura
{
    private string nombre;
    
    public MonitorTemperatura(string nombre)
    {
        this.nombre = nombre;
    }
    
    // Métodos que manejan los eventos
    public void OnTemperaturaCambiada(decimal temperaturaAnterior, decimal temperaturaNueva)
    {
        Console.WriteLine($"[{nombre}] Temperatura cambió de {temperaturaAnterior}°C a {temperaturaNueva}°C");
    }
    
    public void OnTemperaturaAlta(decimal temperatura)
    {
        Console.WriteLine($"[{nombre}] ⚠️  Temperatura ALTA detectada: {temperatura}°C");
    }
    
    public void OnTemperaturaBaja(decimal temperatura)
    {
        Console.WriteLine($"[{nombre}] ❄️  Temperatura BAJA detectada: {temperatura}°C");
    }
    
    public void OnMensajeGenerado(string mensaje)
    {
        Console.WriteLine($"[{nombre}] 📢 {mensaje}");
    }
}

// Clase que registra eventos en un archivo
public class RegistradorEventos
{
    public void OnTemperaturaCambiada(decimal temperaturaAnterior, decimal temperaturaNueva)
    {
        string mensaje = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Temperatura: {temperaturaAnterior}°C → {temperaturaNueva}°C";
        Console.WriteLine($"[REGISTRO] {mensaje}");
        // Aquí se podría escribir en un archivo de log
    }
    
    public void OnMensajeGenerado(string mensaje)
    {
        string log = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {mensaje}";
        Console.WriteLine($"[LOG] {log}");
        // Aquí se podría escribir en un archivo de log
    }
}

// Clase que demuestra el uso de eventos
public class DemostracionEventos
{
    public static void Ejecutar()
    {
        Console.WriteLine("=== SISTEMA DE EVENTOS ===");
        
        // Crear sensor de temperatura
        SensorTemperatura sensor = new SensorTemperatura();
        
        // Crear monitores
        MonitorTemperatura monitor1 = new MonitorTemperatura("Monitor Principal");
        MonitorTemperatura monitor2 = new MonitorTemperatura("Monitor Secundario");
        RegistradorEventos registrador = new RegistradorEventos();
        
        // Suscribir a eventos
        Console.WriteLine("=== SUSCRIBIENDO A EVENTOS ===");
        sensor.TemperaturaCambiada += monitor1.OnTemperaturaCambiada;
        sensor.TemperaturaCambiada += monitor2.OnTemperaturaCambiada;
        sensor.TemperaturaCambiada += registrador.OnTemperaturaCambiada;
        
        sensor.TemperaturaAlta += monitor1.OnTemperaturaAlta;
        sensor.TemperaturaAlta += monitor2.OnTemperaturaAlta;
        
        sensor.TemperaturaBaja += monitor1.OnTemperaturaBaja;
        sensor.TemperaturaBaja += monitor2.OnTemperaturaBaja;
        
        sensor.MensajeGenerado += monitor1.OnMensajeGenerado;
        sensor.MensajeGenerado += registrador.OnMensajeGenerado;
        
        // Simular cambios de temperatura
        Console.WriteLine("\n=== SIMULANDO CAMBIOS DE TEMPERATURA ===");
        sensor.SimularCambioTemperatura(25);
        sensor.SimularCambioTemperatura(35); // Dispara evento de temperatura alta
        sensor.SimularCambioTemperatura(5);  // Dispara evento de temperatura baja
        sensor.SimularCambioTemperatura(20);
        
        // Desuscribir de eventos
        Console.WriteLine("\n=== DESUSCRIBIENDO DE EVENTOS ===");
        sensor.TemperaturaCambiada -= monitor2.OnTemperaturaCambiada;
        sensor.TemperaturaAlta -= monitor2.OnTemperaturaAlta;
        
        Console.WriteLine("Monitor secundario desuscrito de eventos de cambio y alta temperatura");
        
        // Simular otro cambio
        sensor.SimularCambioTemperatura(40);
    }
}
```

### 5. Eventos Personalizados

#### 5.1 Eventos con Argumentos Personalizados

```csharp
using System;

// Clase base para argumentos de eventos
public class EventArgsTemperatura : EventArgs
{
    public decimal TemperaturaAnterior { get; }
    public decimal TemperaturaNueva { get; }
    public DateTime Timestamp { get; }
    
    public EventArgsTemperatura(decimal temperaturaAnterior, decimal temperaturaNueva)
    {
        TemperaturaAnterior = temperaturaAnterior;
        TemperaturaNueva = temperaturaNueva;
        Timestamp = DateTime.Now;
    }
}

// Clase para argumentos de alerta
public class EventArgsAlerta : EventArgs
{
    public string TipoAlerta { get; }
    public decimal Temperatura { get; }
    public string Mensaje { get; }
    public DateTime Timestamp { get; }
    
    public EventArgsAlerta(string tipoAlerta, decimal temperatura, string mensaje)
    {
        TipoAlerta = tipoAlerta;
        Temperatura = temperatura;
        Mensaje = mensaje;
        Timestamp = DateTime.Now;
    }
}

// Clase que usa eventos personalizados
public class SensorTemperaturaAvanzado
{
    private decimal temperatura;
    
    public decimal Temperatura
    {
        get { return temperatura; }
        set
        {
            decimal temperaturaAnterior = temperatura;
            temperatura = value;
            
            if (temperatura != temperaturaAnterior)
            {
                // Disparar evento con argumentos personalizados
                OnTemperaturaCambiada(new EventArgsTemperatura(temperaturaAnterior, temperatura));
                
                if (temperatura > 30)
                {
                    OnAlertaGenerada(new EventArgsAlerta("ALTA", temperatura, "Temperatura crítica alta"));
                }
                else if (temperatura < 10)
                {
                    OnAlertaGenerada(new EventArgsAlerta("BAJA", temperatura, "Temperatura crítica baja"));
                }
            }
        }
    }
    
    // Eventos con argumentos personalizados
    public event EventHandler<EventArgsTemperatura> TemperaturaCambiada;
    public event EventHandler<EventArgsAlerta> AlertaGenerada;
    
    // Métodos protegidos para disparar eventos
    protected virtual void OnTemperaturaCambiada(EventArgsTemperatura e)
    {
        TemperaturaCambiada?.Invoke(this, e);
    }
    
    protected virtual void OnAlertaGenerada(EventArgsAlerta e)
    {
        AlertaGenerada?.Invoke(this, e);
    }
    
    public void SimularCambioTemperatura(decimal nuevaTemperatura)
    {
        Console.WriteLine($"Cambiando temperatura de {Temperatura}°C a {nuevaTemperatura}°C");
        Temperatura = nuevaTemperatura;
    }
}

// Clase que maneja eventos personalizados
public class MonitorTemperaturaAvanzado
{
    private string nombre;
    
    public MonitorTemperaturaAvanzado(string nombre)
    {
        this.nombre = nombre;
    }
    
    // Métodos que manejan eventos personalizados
    public void OnTemperaturaCambiada(object sender, EventArgsTemperatura e)
    {
        Console.WriteLine($"[{nombre}] Temperatura cambió de {e.TemperaturaAnterior}°C a {e.TemperaturaNueva}°C");
        Console.WriteLine($"Timestamp: {e.Timestamp:HH:mm:ss.fff}");
    }
    
    public void OnAlertaGenerada(object sender, EventArgsAlerta e)
    {
        string icono = e.TipoAlerta == "ALTA" ? "🔥" : "❄️";
        Console.WriteLine($"[{nombre}] {icono} ALERTA {e.TipoAlerta}: {e.Mensaje}");
        Console.WriteLine($"Temperatura: {e.Temperatura}°C - {e.Timestamp:HH:mm:ss.fff}");
    }
}

// Clase que demuestra eventos personalizados
public class DemostracionEventosPersonalizados
{
    public static void Ejecutar()
    {
        Console.WriteLine("=== EVENTOS PERSONALIZADOS ===");
        
        // Crear sensor avanzado
        SensorTemperaturaAvanzado sensor = new SensorTemperaturaAvanzado();
        
        // Crear monitores
        MonitorTemperaturaAvanzado monitor1 = new MonitorTemperaturaAvanzado("Monitor Avanzado 1");
        MonitorTemperaturaAvanzado monitor2 = new MonitorTemperaturaAvanzado("Monitor Avanzado 2");
        
        // Suscribir a eventos
        sensor.TemperaturaCambiada += monitor1.OnTemperaturaCambiada;
        sensor.TemperaturaCambiada += monitor2.OnTemperaturaCambiada;
        
        sensor.AlertaGenerada += monitor1.OnAlertaGenerada;
        sensor.AlertaGenerada += monitor2.OnAlertaGenerada;
        
        // Simular cambios
        Console.WriteLine("\n=== SIMULANDO CAMBIOS ===");
        sensor.SimularCambioTemperatura(25);
        sensor.SimularCambioTemperatura(35); // Dispara alerta alta
        sensor.SimularCambioTemperatura(5);  // Dispara alerta baja
        sensor.SimularCambioTemperatura(20);
    }
}
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Sistema de Notificaciones con Delegados
```csharp
// Crear un sistema de notificaciones usando delegados
public delegate void NotificacionHandler(string mensaje, string tipo);

public class SistemaNotificaciones
{
    public event NotificacionHandler NotificacionEnviada;
    
    public void EnviarNotificacion(string mensaje, string tipo)
    {
        // Lógica para enviar notificación
        NotificacionEnviada?.Invoke(mensaje, tipo);
    }
}

// Implementar diferentes manejadores de notificaciones
```

### Ejercicio 2: Sistema de Logging con Eventos
```csharp
// Crear un sistema de logging usando eventos
public class Logger
{
    public event EventHandler<string> MensajeLog;
    
    public void Log(string mensaje)
    {
        // Lógica para registrar log
        OnMensajeLog(mensaje);
    }
    
    protected virtual void OnMensajeLog(string mensaje)
    {
        MensajeLog?.Invoke(this, mensaje);
    }
}

// Implementar diferentes manejadores de logs
```

### Ejercicio 3: Sistema de Validación con Delegados
```csharp
// Crear un sistema de validación usando delegados
public delegate bool Validador<T>(T item);
public delegate string MensajeError<T>(T item);

public class ValidadorGenerico<T>
{
    private List<Validador<T>> validadores;
    private List<MensajeError<T>> mensajesError;
    
    // Implementar métodos para agregar validadores y ejecutar validaciones
}
```

## 🔍 Conceptos Importantes a Recordar

1. **Los delegados son tipos de referencia** que pueden contener referencias a métodos
2. **Proporcionan type safety** y permiten pasar métodos como parámetros
3. **Los eventos son una forma especial** de usar delegados para notificaciones
4. **Action, Func y Predicate** son delegados predefinidos útiles
5. **Los eventos siguen el patrón Observer** para comunicación entre objetos
6. **Los delegados pueden ser multicast** (referenciar múltiples métodos)
7. **Los argumentos de eventos** pueden ser personalizados
8. **La suscripción y desuscripción** a eventos es fundamental

## ❓ Preguntas de Repaso

1. ¿Cuál es la diferencia entre un delegado y un evento?
2. ¿Por qué son útiles los delegados para el callback de métodos?
3. ¿Qué ventajas tienen los eventos sobre los delegados directos?
4. ¿Cómo funcionan los delegados multicast?
5. ¿Cuándo usarías Action vs. Func vs. delegados personalizados?

## 🚀 Siguiente Paso

En la próxima clase aprenderemos sobre **LINQ en C#**, donde veremos cómo consultar y manipular colecciones de datos de manera declarativa.

---

## 📚 Navegación del Módulo 2

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_herencia.md) | Herencia en C# | |
| [Clase 2](clase_2_polimorfismo.md) | Polimorfismo y Métodos Virtuales | |
| [Clase 3](clase_3_interfaces.md) | Interfaces en C# | |
| [Clase 4](clase_4_clases_abstractas.md) | Clases Abstractas | |
| [Clase 5](clase_5_genericos.md) | Genéricos en C# | ← Anterior |
| **Clase 6** | **Delegados y Eventos** | ← Estás aquí |
| [Clase 7](clase_7_linq.md) | LINQ en C# | Siguiente → |
| [Clase 8](clase_8_archivos_streams.md) | Manejo de Archivos y Streams | |
| [Clase 9](clase_9_programacion_asincrona.md) | Programación Asíncrona | |
| [Clase 10](clase_10_reflexion_metaprogramacion.md) | Reflexión y Metaprogramación | |

**← [Volver al README del Módulo 2](../junior_2/README.md)**

---

## 📚 Recursos Adicionales

- [Delegados en C#](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/delegates/)
- [Eventos en C#](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/events/)
- [Patrón Observer](https://docs.microsoft.com/en-us/dotnet/standard/events/)

---

**¡Excelente! Ahora entiendes los delegados y eventos en C#! 🎯**
