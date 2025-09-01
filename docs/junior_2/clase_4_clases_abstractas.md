# Clase 4: Clases Abstractas en C#

## 🎯 Objetivos de la Clase
- Comprender qué son las clases abstractas y su propósito
- Aprender a definir clases abstractas con métodos abstractos
- Entender la diferencia entre clases abstractas e interfaces
- Dominar el uso de clases abstractas para crear jerarquías

## 📚 Contenido Teórico

### 1. ¿Qué son las Clases Abstractas?

Una **clase abstracta** es una clase que **no se puede instanciar directamente** y que puede contener métodos con implementación (concretos) y métodos sin implementación (abstractos). Las clases abstractas sirven como **plantillas base** para otras clases.

#### Características de las Clases Abstractas:
- **No se pueden instanciar**: Solo se pueden usar como clase base
- **Pueden tener implementación**: Métodos concretos con código
- **Pueden tener métodos abstractos**: Que las clases derivadas deben implementar
- **Pueden tener constructores**: Para inicializar estado común
- **Pueden tener campos y propiedades**: Con implementación completa
- **Solo permiten herencia simple**: Una clase solo puede heredar de una clase abstracta

### 2. Definición y Uso Básico

#### 2.1 Sintaxis de Definición

```csharp
using System;
using System.Collections.Generic;

// Clase abstracta base
public abstract class FormaGeometrica
{
    // Propiedades con implementación
    public string Color { get; set; }
    public bool Relleno { get; set; }
    public DateTime FechaCreacion { get; private set; }
    
    // Campo privado con implementación
    private static int contadorFormas = 0;
    
    // Constructor (se ejecuta cuando se crean instancias de clases derivadas)
    protected FormaGeometrica(string color, bool relleno)
    {
        Color = color;
        Relleno = relleno;
        FechaCreacion = DateTime.Now;
        contadorFormas++;
    }
    
    // Métodos concretos (con implementación)
    public void CambiarColor(string nuevoColor)
    {
        Color = nuevoColor;
        Console.WriteLine($"Color cambiado a: {Color}");
    }
    
    public void CambiarRelleno(bool nuevoRelleno)
    {
        Relleno = nuevoRelleno;
        Console.WriteLine($"Relleno cambiado a: {(Relleno ? "Sí" : "No")}");
    }
    
    public virtual void MostrarInformacion()
    {
        Console.WriteLine($"Color: {Color}");
        Console.WriteLine($"Relleno: {Relleno}");
        Console.WriteLine($"Fecha de creación: {FechaCreacion:dd/MM/yyyy HH:mm:ss}");
        Console.WriteLine($"Total de formas creadas: {contadorFormas}");
    }
    
    // Métodos abstractos (sin implementación - deben ser implementados)
    public abstract double CalcularArea();
    public abstract double CalcularPerimetro();
    
    // Método abstracto para dibujar
    public abstract void Dibujar();
    
    // Método abstracto para obtener tipo de forma
    public abstract string ObtenerTipo();
    
    // Método concreto que usa métodos abstractos
    public void MostrarResumen()
    {
        Console.WriteLine($"=== RESUMEN DE {ObtenerTipo().ToUpper()} ===");
        MostrarInformacion();
        Console.WriteLine($"Área: {CalcularArea():F2}");
        Console.WriteLine($"Perímetro: {CalcularPerimetro():F2}");
        Dibujar();
    }
    
    // Método estático (no requiere instancia)
    public static int ObtenerTotalFormas()
    {
        return contadorFormas;
    }
    
    // Método virtual que puede ser sobrescrito
    public virtual void Rotar(double angulo)
    {
        Console.WriteLine($"Rotando forma {angulo} grados");
    }
}
```

#### 2.2 Implementación de Clases Derivadas

```csharp
// Clase derivada que implementa la clase abstracta
public class Rectangulo : FormaGeometrica
{
    // Propiedades específicas del rectángulo
    public double Base { get; set; }
    public double Altura { get; set; }
    
    // Constructor que llama al constructor de la clase base
    public Rectangulo(string color, bool relleno, double baseRect, double altura) 
        : base(color, relleno)
    {
        Base = baseRect;
        Altura = altura;
    }
    
    // Implementar métodos abstractos OBLIGATORIAMENTE
    public override double CalcularArea()
    {
        return Base * Altura;
    }
    
    public override double CalcularPerimetro()
    {
        return 2 * (Base + Altura);
    }
    
    public override void Dibujar()
    {
        Console.WriteLine("Dibujando rectángulo:");
        for (int i = 0; i < Altura; i++)
        {
            string linea = "";
            for (int j = 0; j < Base; j++)
            {
                linea += Relleno ? "█" : "□";
            }
            Console.WriteLine(linea);
        }
    }
    
    public override string ObtenerTipo()
    {
        return "Rectángulo";
    }
    
    // Sobrescribir método virtual de la clase base
    public override void Rotar(double angulo)
    {
        Console.WriteLine($"Rotando rectángulo {angulo} grados");
        // Intercambiar base y altura si la rotación es de 90 grados
        if (Math.Abs(angulo % 180) == 90)
        {
            double temp = Base;
            Base = Altura;
            Altura = temp;
            Console.WriteLine("Base y altura intercambiadas por rotación de 90°");
        }
    }
    
    // Métodos específicos del rectángulo
    public bool EsCuadrado()
    {
        return Base == Altura;
    }
    
    public void CambiarDimensiones(double nuevaBase, double nuevaAltura)
    {
        Base = nuevaBase;
        Altura = nuevaAltura;
        Console.WriteLine($"Dimensiones cambiadas: Base={Base}, Altura={Altura}");
    }
}

// Otra clase derivada
public class Circulo : FormaGeometrica
{
    // Propiedades específicas del círculo
    public double Radio { get; set; }
    
    // Constructor
    public Circulo(string color, bool relleno, double radio) 
        : base(color, relleno)
    {
        Radio = radio;
    }
    
    // Implementar métodos abstractos
    public override double CalcularArea()
    {
        return Math.PI * Radio * Radio;
    }
    
    public override double CalcularPerimetro()
    {
        return 2 * Math.PI * Radio;
    }
    
    public override void Dibujar()
    {
        Console.WriteLine("Dibujando círculo:");
        int radio = (int)Radio;
        for (int y = -radio; y <= radio; y++)
        {
            string linea = "";
            for (int x = -radio; x <= radio; x++)
            {
                double distancia = Math.Sqrt(x * x + y * y);
                if (distancia <= radio)
                {
                    linea += Relleno ? "●" : "○";
                }
                else
                {
                    linea += " ";
                }
            }
            Console.WriteLine(linea);
        }
    }
    
    public override string ObtenerTipo()
    {
        return "Círculo";
    }
    
    // Métodos específicos del círculo
    public double CalcularDiametro()
    {
        return 2 * Radio;
    }
    
    public void CambiarRadio(double nuevoRadio)
    {
        Radio = nuevoRadio;
        Console.WriteLine($"Radio cambiado a: {Radio}");
    }
}
```

### 3. Uso de Clases Abstractas

#### 3.1 Programa Principal

```csharp
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== CLASES ABSTRACTAS EN C# ===");
        
        // ❌ NO se puede crear instancia de clase abstracta
        // FormaGeometrica forma = new FormaGeometrica("Rojo", true); // Error de compilación
        
        // ✅ Crear instancias de clases concretas
        Rectangulo rectangulo = new Rectangulo("Rojo", true, 5, 3);
        Circulo circulo = new Circulo("Azul", false, 4);
        
        // Usar métodos concretos heredados
        Console.WriteLine("\n=== RECTÁNGULO ===");
        rectangulo.MostrarResumen();
        rectangulo.CambiarColor("Verde");
        rectangulo.Rotar(90);
        rectangulo.MostrarResumen();
        
        Console.WriteLine("\n=== CÍRCULO ===");
        circulo.MostrarResumen();
        circulo.CambiarRelleno(true);
        circulo.CambiarRadio(6);
        circulo.MostrarResumen();
        
        // Usar polimorfismo con la clase abstracta
        Console.WriteLine("\n=== POLIMORFISMO CON CLASE ABSTRACTA ===");
        FormaGeometrica[] formas = { rectangulo, circulo };
        
        foreach (FormaGeometrica forma in formas)
        {
            Console.WriteLine($"\nProcesando {forma.ObtenerTipo()}:");
            forma.MostrarInformacion();
            Console.WriteLine($"Área: {forma.CalcularArea():F2}");
            Console.WriteLine($"Perímetro: {forma.CalcularPerimetro():F2}");
            forma.Dibujar();
        }
        
        // Usar método estático
        Console.WriteLine($"\nTotal de formas creadas: {FormaGeometrica.ObtenerTotalFormas()}");
    }
}
```

### 4. Clases Abstractas con Métodos Mixtos

#### 4.1 Ejemplo con Sistema de Empleados

```csharp
// Clase abstracta para empleados
public abstract class Empleado
{
    // Propiedades con implementación
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string DNI { get; set; }
    public DateTime FechaContratacion { get; set; }
    public decimal SalarioBase { get; set; }
    
    // Campo privado
    private static int contadorEmpleados = 0;
    
    // Constructor
    protected Empleado(string nombre, string apellido, string dni, decimal salarioBase)
    {
        Nombre = nombre;
        Apellido = apellido;
        DNI = dni;
        SalarioBase = salarioBase;
        FechaContratacion = DateTime.Now;
        contadorEmpleados++;
    }
    
    // Métodos concretos
    public string ObtenerNombreCompleto()
    {
        return $"{Nombre} {Apellido}";
    }
    
    public int ObtenerAntiguedad()
    {
        return (int)(DateTime.Now - FechaContratacion).TotalDays / 365;
    }
    
    public virtual void MostrarInformacion()
    {
        Console.WriteLine($"Empleado: {ObtenerNombreCompleto()}");
        Console.WriteLine($"DNI: {DNI}");
        Console.WriteLine($"Fecha de contratación: {FechaContratacion:dd/MM/yyyy}");
        Console.WriteLine($"Antigüedad: {ObtenerAntiguedad()} años");
        Console.WriteLine($"Salario base: ${SalarioBase:F2}");
    }
    
    // Métodos abstractos (deben ser implementados)
    public abstract decimal CalcularSalario();
    public abstract string ObtenerCargo();
    public abstract void RealizarTrabajo();
    
    // Método concreto que usa métodos abstractos
    public void MostrarResumen()
    {
        Console.WriteLine($"=== RESUMEN DE {ObtenerCargo().ToUpper()} ===");
        MostrarInformacion();
        Console.WriteLine($"Salario total: ${CalcularSalario():F2}");
        RealizarTrabajo();
    }
    
    // Método estático
    public static int ObtenerTotalEmpleados()
    {
        return contadorEmpleados;
    }
    
    // Método virtual que puede ser sobrescrito
    public virtual void TomarDescanso()
    {
        Console.WriteLine($"{ObtenerNombreCompleto()} está tomando un descanso");
    }
}

// Clase derivada: Empleado por horas
public class EmpleadoPorHoras : Empleado
{
    // Propiedades específicas
    public int HorasTrabajadas { get; set; }
    public decimal TarifaPorHora { get; set; }
    
    public EmpleadoPorHoras(string nombre, string apellido, string dni, 
                           decimal salarioBase, decimal tarifaPorHora) 
        : base(nombre, apellido, dni, salarioBase)
    {
        TarifaPorHora = tarifaPorHora;
        HorasTrabajadas = 0;
    }
    
    // Implementar métodos abstractos
    public override decimal CalcularSalario()
    {
        return HorasTrabajadas * TarifaPorHora;
    }
    
    public override string ObtenerCargo()
    {
        return "Empleado por Horas";
    }
    
    public override void RealizarTrabajo()
    {
        Console.WriteLine($"{ObtenerNombreCompleto()} está trabajando por horas");
        Console.WriteLine($"Horas trabajadas: {HorasTrabajadas}");
        Console.WriteLine($"Tarifa por hora: ${TarifaPorHora:F2}");
    }
    
    // Métodos específicos
    public void RegistrarHoras(int horas)
    {
        HorasTrabajadas += horas;
        Console.WriteLine($"{horas} horas registradas. Total: {HorasTrabajadas}");
    }
    
    public void LimpiarHoras()
    {
        HorasTrabajadas = 0;
        Console.WriteLine("Horas limpiadas para nuevo período");
    }
}

// Clase derivada: Empleado fijo
public class EmpleadoFijo : Empleado
{
    // Propiedades específicas
    public decimal BonoAnual { get; set; }
    public int DiasVacaciones { get; set; }
    
    public EmpleadoFijo(string nombre, string apellido, string dni, 
                       decimal salarioBase, decimal bonoAnual) 
        : base(nombre, apellido, dni, salarioBase)
    {
        BonoAnual = bonoAnual;
        DiasVacaciones = 20;
    }
    
    // Implementar métodos abstractos
    public override decimal CalcularSalario()
    {
        return SalarioBase + (BonoAnual / 12); // Bono mensual
    }
    
    public override string ObtenerCargo()
    {
        return "Empleado Fijo";
    }
    
    public override void RealizarTrabajo()
    {
        Console.WriteLine($"{ObtenerNombreCompleto()} está trabajando como empleado fijo");
        Console.WriteLine($"Bono anual: ${BonoAnual:F2}");
        Console.WriteLine($"Días de vacaciones disponibles: {DiasVacaciones}");
    }
    
    // Sobrescribir método virtual
    public override void TomarDescanso()
    {
        if (DiasVacaciones > 0)
        {
            DiasVacaciones--;
            Console.WriteLine($"{ObtenerNombreCompleto()} está de vacaciones. Días restantes: {DiasVacaciones}");
        }
        else
        {
            Console.WriteLine($"{ObtenerNombreCompleto()} no tiene días de vacaciones disponibles");
        }
    }
    
    // Métodos específicos
    public void SolicitarVacaciones(int dias)
    {
        if (dias <= DiasVacaciones)
        {
            DiasVacaciones -= dias;
            Console.WriteLine($"Vacaciones aprobadas: {dias} días. Restantes: {DiasVacaciones}");
        }
        else
        {
            Console.WriteLine($"Solo tienes {DiasVacaciones} días disponibles");
        }
    }
}
```

### 5. Diferencias entre Clases Abstractas e Interfaces

#### 5.1 Comparación Detallada

```csharp
// INTERFAZ: Solo define contrato
public interface IReproductor
{
    void Reproducir();
    void Pausar();
    void Detener();
}

// CLASE ABSTRACTA: Puede tener implementación
public abstract class ReproductorBase
{
    // Campos y propiedades con implementación
    public string Nombre { get; set; }
    public bool EstaReproduciendo { get; protected set; }
    
    // Constructor
    protected ReproductorBase(string nombre)
    {
        Nombre = nombre;
        EstaReproduciendo = false;
    }
    
    // Métodos concretos
    public virtual void MostrarEstado()
    {
        Console.WriteLine($"Reproductor: {Nombre}");
        Console.WriteLine($"Estado: {(EstaReproduciendo ? "Reproduciendo" : "Detenido")}");
    }
    
    // Métodos abstractos
    public abstract void Reproducir();
    public abstract void Pausar();
    public abstract void Detener();
    
    // Método concreto que usa métodos abstractos
    public void CicloReproduccion()
    {
        Reproducir();
        System.Threading.Thread.Sleep(2000); // Simular reproducción
        Pausar();
        System.Threading.Thread.Sleep(1000); // Simular pausa
        Detener();
    }
}

// Implementación de interfaz
public class ReproductorMP3 : IReproductor
{
    public void Reproducir() { Console.WriteLine("MP3 reproduciendo"); }
    public void Pausar() { Console.WriteLine("MP3 pausado"); }
    public void Detener() { Console.WriteLine("MP3 detenido"); }
}

// Implementación de clase abstracta
public class ReproductorCD : ReproductorBase
{
    public ReproductorCD(string nombre) : base(nombre) { }
    
    public override void Reproducir() 
    { 
        EstaReproduciendo = true;
        Console.WriteLine("CD reproduciendo"); 
    }
    
    public override void Pausar() 
    { 
        EstaReproduciendo = false;
        Console.WriteLine("CD pausado"); 
    }
    
    public override void Detener() 
    { 
        EstaReproduciendo = false;
        Console.WriteLine("CD detenido"); 
    }
}
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Sistema de Vehículos
```csharp
// Crear clase abstracta Vehiculo
public abstract class Vehiculo
{
    public string Marca { get; set; }
    public string Modelo { get; set; }
    public int Año { get; set; }
    
    public abstract void Arrancar();
    public abstract void Detener();
    public abstract double CalcularConsumo();
    
    public virtual void MostrarInformacion()
    {
        Console.WriteLine($"Marca: {Marca}, Modelo: {Modelo}, Año: {Año}");
    }
}

// Implementar: Coche, Moto, Camion
// Cada uno con su comportamiento específico
```

### Ejercicio 2: Sistema de Animales
```csharp
// Crear clase abstracta Animal
public abstract class Animal
{
    public string Nombre { get; set; }
    public int Edad { get; set; }
    
    public abstract void HacerSonido();
    public abstract void Moverse();
    public abstract string ObtenerTipo();
    
    public virtual void MostrarInformacion()
    {
        Console.WriteLine($"Animal: {Nombre}, Edad: {Edad} años, Tipo: {ObtenerTipo()}");
    }
}

// Implementar: Perro, Gato, Pajaro
// Cada uno con su comportamiento específico
```

### Ejercicio 3: Sistema de Formas 3D
```csharp
// Crear clase abstracta Forma3D
public abstract class Forma3D
{
    public string Color { get; set; }
    
    public abstract double CalcularVolumen();
    public abstract double CalcularAreaSuperficie();
    public abstract void MostrarDimensiones();
    
    public virtual void MostrarInformacion()
    {
        Console.WriteLine($"Color: {Color}");
        Console.WriteLine($"Volumen: {CalcularVolumen():F2}");
        Console.WriteLine($"Área de superficie: {CalcularAreaSuperficie():F2}");
    }
}

// Implementar: Cubo, Esfera, Cilindro
// Cada uno con sus fórmulas específicas
```

## 🔍 Conceptos Importantes a Recordar

1. **Las clases abstractas no se pueden instanciar** directamente
2. **Pueden tener implementación** en métodos concretos
3. **Los métodos abstractos deben ser implementados** por las clases derivadas
4. **Pueden tener constructores** para inicializar estado común
5. **Permiten herencia simple** (una clase solo puede heredar de una clase abstracta)
6. **Los métodos abstractos no tienen implementación** en la clase base
7. **Se pueden usar como tipos** para referencias polimórficas
8. **Son útiles para crear jerarquías** con comportamiento común

## ❓ Preguntas de Repaso

1. ¿Cuál es la diferencia principal entre una clase abstracta y una interfaz?
2. ¿Por qué no se pueden instanciar las clases abstractas?
3. ¿Cuándo usarías una clase abstracta en lugar de una interfaz?
4. ¿Qué sucede si no implementas todos los métodos abstractos?
5. ¿Pueden las clases abstractas tener constructores? ¿Por qué?

## 🚀 Siguiente Paso

En la próxima clase aprenderemos sobre **Genéricos en C#**, donde veremos cómo crear código reutilizable que funciona con diferentes tipos de datos.

---

## 📚 Navegación del Módulo 2

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_herencia.md) | Herencia en C# | |
| [Clase 2](clase_2_polimorfismo.md) | Polimorfismo y Métodos Virtuales | |
| [Clase 3](clase_3_interfaces.md) | Interfaces en C# | ← Anterior |
| **Clase 4** | **Clases Abstractas** | ← Estás aquí |
| [Clase 5](clase_5_genericos.md) | Genéricos en C# | Siguiente → |
| [Clase 6](clase_6_delegados_eventos.md) | Delegados y Eventos | |
| [Clase 7](clase_7_linq.md) | LINQ en C# | |
| [Clase 8](clase_8_archivos_streams.md) | Manejo de Archivos y Streams | |
| [Clase 9](clase_9_programacion_asincrona.md) | Programación Asíncrona | |
| [Clase 10](clase_10_reflexion_metaprogramacion.md) | Reflexión y Metaprogramación | |

**← [Volver al README del Módulo 2](../junior_2/README.md)**

---

## 📚 Recursos Adicionales

- [Clases abstractas en C#](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/abstract-classes)
- [Métodos abstractos](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/abstract)
- [Herencia en C#](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/inheritance)

---

**¡Excelente! Ahora entiendes las clases abstractas en C#! 🎯**
