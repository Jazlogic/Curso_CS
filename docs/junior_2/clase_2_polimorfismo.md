# Clase 2: Polimorfismo y Métodos Virtuales en C#

## 🎯 Objetivos de la Clase
- Comprender qué es el polimorfismo y sus tipos
- Aprender a usar métodos virtuales y override
- Entender el polimorfismo en tiempo de ejecución
- Dominar el uso de sealed y new

## 📚 Contenido Teórico

### 1. ¿Qué es el Polimorfismo?

El polimorfismo es la **capacidad de objetos de diferentes clases** de responder al mismo mensaje de manera diferente. En C#, esto se logra principalmente a través de herencia y métodos virtuales.

#### Tipos de Polimorfismo:
- **Polimorfismo de Sobrecarga**: Múltiples métodos con el mismo nombre pero diferentes parámetros
- **Polimorfismo de Sobrescritura**: Métodos en clases derivadas que reemplazan métodos de la clase base
- **Polimorfismo de Inclusión**: Usar una referencia de clase base para referenciar objetos de clases derivadas

### 2. Métodos Virtuales y Override

#### 2.1 Sintaxis Básica

```csharp
using System;

// Clase base con métodos virtuales
public class Forma
{
    public string Color { get; set; }
    public bool Relleno { get; set; }
    
    public Forma(string color, bool relleno)
    {
        Color = color;
        Relleno = relleno;
    }
    
    // Método virtual que puede ser sobrescrito
    public virtual double CalcularArea()
    {
        Console.WriteLine("Calculando área de forma genérica");
        return 0;
    }
    
    // Método virtual para perímetro
    public virtual double CalcularPerimetro()
    {
        Console.WriteLine("Calculando perímetro de forma genérica");
        return 0;
    }
    
    // Método virtual para mostrar información
    public virtual void MostrarInformacion()
    {
        Console.WriteLine($"Color: {Color}");
        Console.WriteLine($"Relleno: {Relleno}");
        Console.WriteLine($"Área: {CalcularArea():F2}");
        Console.WriteLine($"Perímetro: {CalcularPerimetro():F2}");
    }
    
    // Método que no es virtual (no se puede sobrescribir)
    public void CambiarColor(string nuevoColor)
    {
        Color = nuevoColor;
        Console.WriteLine($"Color cambiado a: {Color}");
    }
}

// Clase derivada que sobrescribe métodos virtuales
public class Rectangulo : Forma
{
    public double Base { get; set; }
    public double Altura { get; set; }
    
    public Rectangulo(string color, bool relleno, double baseRect, double altura) 
        : base(color, relleno)
    {
        Base = baseRect;
        Altura = altura;
    }
    
    // Sobrescribe el método virtual de la clase base
    public override double CalcularArea()
    {
        Console.WriteLine("Calculando área del rectángulo");
        return Base * Altura;
    }
    
    // Sobrescribe el método virtual de la clase base
    public override double CalcularPerimetro()
    {
        Console.WriteLine("Calculando perímetro del rectángulo");
        return 2 * (Base + Altura);
    }
    
    // Sobrescribe el método virtual de la clase base
    public override void MostrarInformacion()
    {
        Console.WriteLine("=== RECTÁNGULO ===");
        base.MostrarInformacion(); // Llama al método de la clase base
        Console.WriteLine($"Base: {Base}");
        Console.WriteLine($"Altura: {Altura}");
    }
    
    // Método específico del rectángulo
    public bool EsCuadrado()
    {
        return Base == Altura;
    }
}

// Otra clase derivada
public class Circulo : Forma
{
    public double Radio { get; set; }
    
    public Circulo(string color, bool relleno, double radio) 
        : base(color, relleno)
    {
        Radio = radio;
    }
    
    // Sobrescribe el método virtual de la clase base
    public override double CalcularArea()
    {
        Console.WriteLine("Calculando área del círculo");
        return Math.PI * Radio * Radio;
    }
    
    // Sobrescribe el método virtual de la clase base
    public override double CalcularPerimetro()
    {
        Console.WriteLine("Calculando perímetro del círculo");
        return 2 * Math.PI * Radio;
    }
    
    // Sobrescribe el método virtual de la clase base
    public override void MostrarInformacion()
    {
        Console.WriteLine("=== CÍRCULO ===");
        base.MostrarInformacion(); // Llama al método de la clase base
        Console.WriteLine($"Radio: {Radio}");
    }
    
    // Método específico del círculo
    public double CalcularDiametro()
    {
        return 2 * Radio;
    }
}

// Programa principal
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== POLIMORFISMO CON MÉTODOS VIRTUALES ===");
        
        // Crear instancias de las clases derivadas
        Rectangulo rectangulo = new Rectangulo("Rojo", true, 5, 3);
        Circulo circulo = new Circulo("Azul", false, 4);
        
        // Usar métodos específicos de cada clase
        Console.WriteLine("\n=== RECTÁNGULO ===");
        rectangulo.MostrarInformacion();
        Console.WriteLine($"¿Es cuadrado? {rectangulo.EsCuadrado()}");
        
        Console.WriteLine("\n=== CÍRCULO ===");
        circulo.MostrarInformacion();
        Console.WriteLine($"Diámetro: {circulo.CalcularDiametro():F2}");
        
        // Demostrar polimorfismo
        Console.WriteLine("\n=== POLIMORFISMO ===");
        Forma[] formas = { rectangulo, circulo };
        
        foreach (Forma forma in formas)
        {
            Console.WriteLine($"\nProcesando {forma.GetType().Name}:");
            forma.CalcularArea();        // Llamada polimórfica
            forma.CalcularPerimetro();   // Llamada polimórfica
            forma.CambiarColor("Verde"); // Método no virtual
        }
    }
}
```

#### Explicación Línea por Línea:

**Línea 20: `public virtual double CalcularArea()`**
- `virtual` permite que las clases derivadas sobrescriban este método
- Proporciona comportamiento por defecto
- Se puede llamar desde la clase base

**Línea 35: `public override double CalcularArea()`**
- `override` sobrescribe el método virtual de la clase base
- Proporciona implementación específica para rectángulos
- Se ejecuta cuando se llama desde una referencia de tipo `Rectangulo`

**Línea 42: `base.MostrarInformacion();`**
- `base` llama al método de la clase base
- Permite reutilizar código común
- Luego agrega información específica

**Línea 70: `Forma[] formas = { rectangulo, circulo };`**
- Array de tipo `Forma` que contiene objetos de clases derivadas
- Demuestra polimorfismo de inclusión

**Línea 75: `forma.CalcularArea();`**
- Llamada polimórfica
- El método ejecutado depende del tipo real del objeto
- `rectangulo` ejecuta `Rectangulo.CalcularArea()`
- `circulo` ejecuta `Circulo.CalcularArea()`

### 3. Polimorfismo en Tiempo de Ejecución

#### 3.1 Ejemplo con Empleados

```csharp
public class Empleado
{
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public decimal SalarioBase { get; set; }
    
    public Empleado(string nombre, string apellido, decimal salarioBase)
    {
        Nombre = nombre;
        Apellido = apellido;
        SalarioBase = salarioBase;
    }
    
    // Método virtual para calcular salario
    public virtual decimal CalcularSalario()
    {
        return SalarioBase;
    }
    
    // Método virtual para mostrar información
    public virtual void MostrarInformacion()
    {
        Console.WriteLine($"Empleado: {Nombre} {Apellido}");
        Console.WriteLine($"Salario Base: ${SalarioBase:F2}");
        Console.WriteLine($"Salario Total: ${CalcularSalario():F2}");
    }
    
    // Método que no es virtual
    public void Saludar()
    {
        Console.WriteLine($"Hola, soy {Nombre} {Apellido}");
    }
}

public class EmpleadoTiempoCompleto : Empleado
{
    public decimal Bono { get; set; }
    
    public EmpleadoTiempoCompleto(string nombre, string apellido, decimal salarioBase, decimal bono) 
        : base(nombre, apellido, salarioBase)
    {
        Bono = bono;
    }
    
    public override decimal CalcularSalario()
    {
        return SalarioBase + Bono;
    }
    
    public override void MostrarInformacion()
    {
        Console.WriteLine("=== EMPLEADO TIEMPO COMPLETO ===");
        base.MostrarInformacion();
        Console.WriteLine($"Bono: ${Bono:F2}");
    }
}

public class EmpleadoTiempoParcial : Empleado
{
    public int HorasTrabajadas { get; set; }
    public decimal TarifaPorHora { get; set; }
    
    public EmpleadoTiempoParcial(string nombre, string apellido, decimal salarioBase, 
                                 int horasTrabajadas, decimal tarifaPorHora) 
        : base(nombre, apellido, salarioBase)
    {
        HorasTrabajadas = horasTrabajadas;
        TarifaPorHora = tarifaPorHora;
    }
    
    public override decimal CalcularSalario()
    {
        return HorasTrabajadas * TarifaPorHora;
    }
    
    public override void MostrarInformacion()
    {
        Console.WriteLine("=== EMPLEADO TIEMPO PARCIAL ===");
        base.MostrarInformacion();
        Console.WriteLine($"Horas trabajadas: {HorasTrabajadas}");
        Console.WriteLine($"Tarifa por hora: ${TarifaPorHora:F2}");
    }
}

public class Consultor : Empleado
{
    public decimal TarifaPorProyecto { get; set; }
    public int ProyectosCompletados { get; set; }
    
    public Consultor(string nombre, string apellido, decimal salarioBase, 
                    decimal tarifaPorProyecto, int proyectosCompletados) 
        : base(nombre, apellido, salarioBase)
    {
        TarifaPorProyecto = tarifaPorProyecto;
        ProyectosCompletados = proyectosCompletados;
    }
    
    public override decimal CalcularSalario()
    {
        return SalarioBase + (TarifaPorProyecto * ProyectosCompletados);
    }
    
    public override void MostrarInformacion()
    {
        Console.WriteLine("=== CONSULTOR ===");
        base.MostrarInformacion();
        Console.WriteLine($"Tarifa por proyecto: ${TarifaPorProyecto:F2}");
        Console.WriteLine($"Proyectos completados: {ProyectosCompletados}");
    }
}
```

#### 3.2 Demostración del Polimorfismo

```csharp
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== POLIMORFISMO EN TIEMPO DE EJECUCIÓN ===");
        
        // Crear diferentes tipos de empleados
        EmpleadoTiempoCompleto empleado1 = new EmpleadoTiempoCompleto("Juan", "Pérez", 3000, 500);
        EmpleadoTiempoParcial empleado2 = new EmpleadoTiempoParcial("María", "García", 0, 20, 15);
        Consultor consultor = new Consultor("Carlos", "López", 1000, 200, 3);
        
        // Array de tipo Empleado que contiene objetos de clases derivadas
        Empleado[] empleados = { empleado1, empleado2, consultor };
        
        // Procesar todos los empleados de manera polimórfica
        foreach (Empleado empleado in empleados)
        {
            Console.WriteLine("\n" + new string('=', 50));
            empleado.Saludar();           // Método no virtual - comportamiento común
            empleado.MostrarInformacion(); // Método virtual - comportamiento específico
        }
        
        // Calcular salario total de todos los empleados
        decimal salarioTotal = 0;
        foreach (Empleado empleado in empleados)
        {
            salarioTotal += empleado.CalcularSalario(); // Llamada polimórfica
        }
        
        Console.WriteLine($"\n=== SALARIO TOTAL DE TODOS LOS EMPLEADOS: ${salarioTotal:F2} ===");
    }
}
```

### 4. Palabra Clave sealed

#### 4.1 Prevenir Sobrescritura

```csharp
public class Forma
{
    public string Color { get; set; }
    
    public Forma(string color)
    {
        Color = color;
    }
    
    public virtual void MostrarInformacion()
    {
        Console.WriteLine($"Color: {Color}");
    }
}

public class Rectangulo : Forma
{
    public double Base { get; set; }
    public double Altura { get; set; }
    
    public Rectangulo(string color, double baseRect, double altura) : base(color)
    {
        Base = baseRect;
        Altura = altura;
    }
    
    // Este método no se puede sobrescribir en clases que hereden de Rectangulo
    public sealed override void MostrarInformacion()
    {
        Console.WriteLine("=== RECTÁNGULO ===");
        base.MostrarInformacion();
        Console.WriteLine($"Base: {Base}, Altura: {Altura}");
    }
}

// Esta clase NO puede sobrescribir MostrarInformacion
public class Cuadrado : Rectangulo
{
    public Cuadrado(string color, double lado) : base(color, lado, lado)
    {
    }
    
    // ❌ Esto causaría un error de compilación:
    // public override void MostrarInformacion() { }
    
    // ✅ Pero puede agregar nuevos métodos
    public void MostrarComoCuadrado()
    {
        Console.WriteLine($"Cuadrado de lado {Base}");
    }
}
```

#### 4.2 Clases Selladas

```csharp
// Clase sellada - no se puede heredar de ella
public sealed class Configuracion
{
    public string NombreAplicacion { get; set; }
    public string Version { get; set; }
    public bool ModoDebug { get; set; }
    
    public Configuracion()
    {
        NombreAplicacion = "Mi Aplicación";
        Version = "1.0.0";
        ModoDebug = false;
    }
    
    public void MostrarConfiguracion()
    {
        Console.WriteLine($"Aplicación: {NombreAplicacion}");
        Console.WriteLine($"Versión: {Version}");
        Console.WriteLine($"Modo Debug: {ModoDebug}");
    }
}

// ❌ Esto causaría un error de compilación:
// public class ConfiguracionAvanzada : Configuracion { }

// ✅ Pero se puede usar normalmente
class Program
{
    static void Main(string[] args)
    {
        Configuracion config = new Configuracion();
        config.MostrarConfiguracion();
    }
}
```

### 5. Palabra Clave new

#### 5.1 Ocultar Métodos de la Clase Base

```csharp
public class Animal
{
    public string Nombre { get; set; }
    
    public Animal(string nombre)
    {
        Nombre = nombre;
    }
    
    public void HacerSonido()
    {
        Console.WriteLine("El animal hace un sonido");
    }
    
    public virtual void Comer()
    {
        Console.WriteLine("El animal está comiendo");
    }
}

public class Perro : Animal
{
    public string Raza { get; set; }
    
    public Perro(string nombre, string raza) : base(nombre)
    {
        Raza = raza;
    }
    
    // Ocultar el método de la clase base (no virtual)
    public new void HacerSonido()
    {
        Console.WriteLine("¡Guau! ¡Guau!");
    }
    
    // Sobrescribir el método virtual de la clase base
    public override void Comer()
    {
        Console.WriteLine("El perro está comiendo croquetas");
    }
    
    // Método específico del perro
    public void Ladrar()
    {
        Console.WriteLine($"{Nombre} está ladrando");
    }
}

// Demostración de la diferencia entre new y override
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== DIFERENCIA ENTRE NEW Y OVERRIDE ===");
        
        // Crear instancia de Perro
        Perro miPerro = new Perro("Buddy", "Golden Retriever");
        
        Console.WriteLine("\n=== LLAMANDO DESDE REFERENCIA DE PERRO ===");
        miPerro.HacerSonido();  // Llama al método de Perro
        miPerro.Comer();         // Llama al método de Perro
        
        Console.WriteLine("\n=== LLAMANDO DESDE REFERENCIA DE ANIMAL ===");
        Animal miAnimal = miPerro;  // Referencia de tipo Animal
        
        miAnimal.HacerSonido();  // Llama al método de Animal (oculto)
        miAnimal.Comer();        // Llama al método de Perro (sobrescrito)
        
        Console.WriteLine("\n=== EXPLICACIÓN ===");
        Console.WriteLine("HacerSonido: new oculta el método de la clase base");
        Console.WriteLine("Comer: override sobrescribe el método de la clase base");
    }
}
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Sistema de Pagos Polimórfico
```csharp
// Crear un sistema de pagos con diferentes métodos
public abstract class MetodoPago
{
    public decimal Monto { get; set; }
    
    public MetodoPago(decimal monto)
    {
        Monto = monto;
    }
    
    public abstract void ProcesarPago();
    public abstract void MostrarConfirmacion();
}

// Implementar clases: TarjetaCredito, TransferenciaBancaria, PayPal
// Cada una con su propia lógica de procesamiento
```

### Ejercicio 2: Jerarquía de Notificaciones
```csharp
// Crear un sistema de notificaciones
public class Notificacion
{
    public string Mensaje { get; set; }
    public DateTime FechaEnvio { get; set; }
    
    public Notificacion(string mensaje)
    {
        Mensaje = mensaje;
        FechaEnvio = DateTime.Now;
    }
    
    public virtual void Enviar()
    {
        Console.WriteLine($"Enviando notificación: {Mensaje}");
    }
}

// Implementar clases: Email, SMS, PushNotification
// Cada una con su método de envío específico
```

### Ejercicio 3: Sistema de Formas Geométricas
```csharp
// Extender el sistema de formas con más tipos
public abstract class FormaGeometrica
{
    public string Color { get; set; }
    
    public FormaGeometrica(string color)
    {
        Color = color;
    }
    
    public abstract double CalcularArea();
    public abstract double CalcularPerimetro();
    
    public virtual void MostrarInformacion()
    {
        Console.WriteLine($"Color: {Color}");
        Console.WriteLine($"Área: {CalcularArea():F2}");
        Console.WriteLine($"Perímetro: {CalcularPerimetro():F2}");
    }
}

// Implementar: Triangulo, Pentagono, Hexagono
// Cada uno con sus fórmulas específicas
```

## 🔍 Conceptos Importantes a Recordar

1. **El polimorfismo permite** que objetos de diferentes clases respondan al mismo mensaje
2. **Los métodos virtuales** pueden ser sobrescritos en clases derivadas
3. **Override sobrescribe** métodos virtuales de la clase base
4. **New oculta** métodos de la clase base sin sobrescribirlos
5. **Sealed previene** la herencia o sobrescritura adicional
6. **El polimorfismo en tiempo de ejecución** determina qué método ejecutar
7. **La referencia de tipo base** puede contener objetos de clases derivadas
8. **Los métodos no virtuales** siempre se ejecutan desde la clase base

## ❓ Preguntas de Repaso

1. ¿Cuál es la diferencia entre `virtual` y `override`?
2. ¿Qué significa `sealed` en un método o clase?
3. ¿Cuándo usarías `new` en lugar de `override`?
4. ¿Cómo funciona el polimorfismo en tiempo de ejecución?
5. ¿Por qué es importante el polimorfismo en POO?

## 🚀 Siguiente Paso

En la próxima clase aprenderemos sobre **Interfaces en C#**, donde veremos cómo definir contratos y comportamientos que las clases deben implementar.

---

## 📚 Navegación del Módulo 2

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_herencia.md) | Herencia en C# | ← Anterior |
| **Clase 2** | **Polimorfismo y Métodos Virtuales** | ← Estás aquí |
| [Clase 3](clase_3_interfaces.md) | Interfaces en C# | Siguiente → |
| [Clase 4](clase_4_clases_abstractas.md) | Clases Abstractas | |
| [Clase 5](clase_5_genericos.md) | Genéricos en C# | |
| [Clase 6](clase_6_delegados_eventos.md) | Delegados y Eventos | |
| [Clase 7](clase_7_linq.md) | LINQ en C# | |
| [Clase 8](clase_8_archivos_streams.md) | Manejo de Archivos y Streams | |
| [Clase 9](clase_9_programacion_asincrona.md) | Programación Asíncrona | |
| [Clase 10](clase_10_reflexion_metaprogramacion.md) | Reflexión y Metaprogramación | |

**← [Volver al README del Módulo 2](../junior_2/README.md)**

---

## 📚 Recursos Adicionales

- [Polimorfismo en C#](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/polymorphism)
- [Métodos virtuales](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/virtual)
- [Palabra clave sealed](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/sealed)

---

**¡Excelente! Ahora entiendes el polimorfismo y los métodos virtuales en C#! 🎯**
