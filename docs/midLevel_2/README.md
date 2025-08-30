# 🎯 Mid Level 2: Herencia, Polimorfismo e Interfaces

## 📚 Descripción

En este nivel profundizarás en conceptos avanzados de POO: herencia, polimorfismo, interfaces y clases abstractas. Estos conceptos son fundamentales para crear arquitecturas de software robustas y mantenibles.

## 🎯 Objetivos de Aprendizaje

- Implementar herencia entre clases
- Entender y usar polimorfismo
- Crear y usar interfaces
- Trabajar con clases abstractas
- Implementar métodos virtuales y override
- Crear jerarquías de clases bien estructuradas

## 📖 Contenido Teórico

### 1. Herencia

#### Concepto de Herencia
La herencia permite que una clase (clase derivada) herede características de otra clase (clase base). Esto promueve la reutilización de código y establece una relación "es-un".

```csharp
// Clase base (padre)
public class Animal
{
    public string Nombre { get; set; }
    public int Edad { get; set; }
    
    public virtual void HacerSonido()
    {
        Console.WriteLine("Hace algún sonido");
    }
}

// Clase derivada (hija)
public class Perro : Animal
{
    public string Raza { get; set; }
    
    public override void HacerSonido()
    {
        Console.WriteLine("¡Guau! ¡Guau!");
    }
    
    public void Ladrar()
    {
        Console.WriteLine("¡Ladrando!");
    }
}
```

#### Herencia Simple vs Múltiple
```csharp
// C# solo permite herencia simple de clases
public class Mamifero : Animal // OK
{
    public bool TienePelo { get; set; }
}

// Pero permite implementar múltiples interfaces
public class Perro : Animal, ICompanero, IEntrenable // OK
{
    // Implementación
}
```

#### Constructor en Herencia
```csharp
public class Animal
{
    public string Nombre { get; set; }
    
    public Animal(string nombre)
    {
        Nombre = nombre;
    }
}

public class Perro : Animal
{
    public string Raza { get; set; }
    
    // Llamada al constructor de la clase base
    public Perro(string nombre, string raza) : base(nombre)
    {
        Raza = raza;
    }
}
```

### 2. Polimorfismo

#### Polimorfismo de Inclusión
```csharp
public class Animal
{
    public virtual void HacerSonido()
    {
        Console.WriteLine("Hace algún sonido");
    }
}

public class Perro : Animal
{
    public override void HacerSonido()
    {
        Console.WriteLine("¡Guau! ¡Guau!");
    }
}

public class Gato : Animal
{
    public override void HacerSonido()
    {
        Console.WriteLine("¡Miau! ¡Miau!");
    }
}

// Uso del polimorfismo
Animal[] animales = new Animal[]
{
    new Perro(),
    new Gato(),
    new Animal()
};

foreach (var animal in animales)
{
    animal.HacerSonido(); // Cada uno hace su sonido específico
}
```

#### Polimorfismo de Sobrecarga
```csharp
public class Calculadora
{
    public int Sumar(int a, int b)
    {
        return a + b;
    }
    
    public double Sumar(double a, double b)
    {
        return a + b;
    }
    
    public string Sumar(string a, string b)
    {
        return a + b;
    }
}
```

### 3. Métodos Virtuales y Override

#### Métodos Virtuales
```csharp
public class Forma
{
    public virtual double CalcularArea()
    {
        return 0;
    }
    
    public virtual double CalcularPerimetro()
    {
        return 0;
    }
}

public class Rectangulo : Forma
{
    public double Base { get; set; }
    public double Altura { get; set; }
    
    public override double CalcularArea()
    {
        return Base * Altura;
    }
    
    public override double CalcularPerimetro()
    {
        return 2 * (Base + Altura);
    }
}

public class Circulo : Forma
{
    public double Radio { get; set; }
    
    public override double CalcularArea()
    {
        return Math.PI * Radio * Radio;
    }
    
    public override double CalcularPerimetro()
    {
        return 2 * Math.PI * Radio;
    }
}
```

#### Métodos Sellados (Sealed)
```csharp
public class Forma
{
    public virtual void Dibujar()
    {
        Console.WriteLine("Dibujando forma");
    }
}

public class Rectangulo : Forma
{
    public override void Dibujar()
    {
        Console.WriteLine("Dibujando rectángulo");
    }
}

public class Cuadrado : Rectangulo
{
    // No se puede hacer override porque el método está sellado
    // public override void Dibujar() { } // ERROR
}
```

### 4. Interfaces

#### Concepto de Interfaces
Las interfaces definen un contrato que las clases deben implementar. Solo contienen declaraciones de métodos, propiedades, eventos e indexadores.

```csharp
public interface IReproducible
{
    void Reproducir();
    void Pausar();
    void Detener();
    bool EstaReproduciendo { get; }
}

public class ReproductorMP3 : IReproducible
{
    public bool EstaReproduciendo { get; private set; }
    
    public void Reproducir()
    {
        EstaReproduciendo = true;
        Console.WriteLine("Reproduciendo MP3...");
    }
    
    public void Pausar()
    {
        EstaReproduciendo = false;
        Console.WriteLine("MP3 pausado");
    }
    
    public void Detener()
    {
        EstaReproduciendo = false;
        Console.WriteLine("MP3 detenido");
    }
}
```

#### Interfaces Múltiples
```csharp
public interface IReproducible
{
    void Reproducir();
}

public interface IGrabable
{
    void Grabar();
}

public interface IConectable
{
    void Conectar();
    void Desconectar();
}

public class Smartphone : IReproducible, IGrabable, IConectable
{
    public void Reproducir()
    {
        Console.WriteLine("Reproduciendo en smartphone");
    }
    
    public void Grabar()
    {
        Console.WriteLine("Grabando con smartphone");
    }
    
    public void Conectar()
    {
        Console.WriteLine("Smartphone conectado");
    }
    
    public void Desconectar()
    {
        Console.WriteLine("Smartphone desconectado");
    }
}
```

#### Interfaces con Implementación por Defecto (C# 8.0+)
```csharp
public interface ILogger
{
    void Log(string mensaje);
    
    // Implementación por defecto
    void LogError(string mensaje)
    {
        Log($"ERROR: {mensaje}");
    }
}

public class ConsoleLogger : ILogger
{
    public void Log(string mensaje)
    {
        Console.WriteLine(mensaje);
    }
    
    // LogError se hereda automáticamente
}
```

### 5. Clases Abstractas

#### Concepto de Clases Abstractas
Las clases abstractas son clases que no se pueden instanciar directamente y pueden contener métodos abstractos que deben ser implementados por las clases derivadas.

```csharp
public abstract class Forma
{
    public string Color { get; set; }
    
    // Método abstracto (debe ser implementado)
    public abstract double CalcularArea();
    
    // Método virtual (puede ser sobrescrito)
    public virtual void Dibujar()
    {
        Console.WriteLine($"Dibujando forma de color {Color}");
    }
    
    // Método concreto (implementación por defecto)
    public void CambiarColor(string nuevoColor)
    {
        Color = nuevoColor;
    }
}

public class Rectangulo : Forma
{
    public double Base { get; set; }
    public double Altura { get; set; }
    
    public override double CalcularArea()
    {
        return Base * Altura;
    }
    
    public override void Dibujar()
    {
        Console.WriteLine($"Dibujando rectángulo {Base}x{Altura} de color {Color}");
    }
}
```

#### Diferencias entre Interfaces y Clases Abstractas
```csharp
// Clase abstracta
public abstract class Animal
{
    protected string nombre; // Puede tener campos
    
    public abstract void HacerSonido();
    
    public virtual void Mover()
    {
        Console.WriteLine("El animal se mueve");
    }
}

// Interface
public interface IAnimal
{
    void HacerSonido(); // Solo métodos
    void Mover();
    
    // No puede tener campos ni implementación
}
```

### 6. Patrones de Diseño Básicos

#### Patrón Factory
```csharp
public interface IVehiculo
{
    void Arrancar();
}

public class Coche : IVehiculo
{
    public void Arrancar()
    {
        Console.WriteLine("Coche arrancando...");
    }
}

public class Moto : IVehiculo
{
    public void Arrancar()
    {
        Console.WriteLine("Moto arrancando...");
    }
}

public class VehiculoFactory
{
    public static IVehiculo CrearVehiculo(string tipo)
    {
        switch (tipo.ToLower())
        {
            case "coche":
                return new Coche();
            case "moto":
                return new Moto();
            default:
                throw new ArgumentException("Tipo de vehículo no válido");
        }
    }
}
```

#### Patrón Strategy
```csharp
public interface IEstrategiaPago
{
    void Pagar(decimal monto);
}

public class PagoEfectivo : IEstrategiaPago
{
    public void Pagar(decimal monto)
    {
        Console.WriteLine($"Pagando ${monto} en efectivo");
    }
}

public class PagoTarjeta : IEstrategiaPago
{
    public void Pagar(decimal monto)
    {
        Console.WriteLine($"Pagando ${monto} con tarjeta");
    }
}

public class Compra
{
    private IEstrategiaPago estrategiaPago;
    
    public void EstablecerEstrategiaPago(IEstrategiaPago estrategia)
    {
        estrategiaPago = estrategia;
    }
    
    public void RealizarPago(decimal monto)
    {
        estrategiaPago?.Pagar(monto);
    }
}
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Jerarquía de Empleados
Crea una jerarquía de clases: Empleado (base), EmpleadoTiempoCompleto y EmpleadoTiempoParcial. Implementa métodos para calcular salario.

### Ejercicio 2: Sistema de Formas Geométricas
Implementa una jerarquía de formas: Forma (abstracta), Rectangulo, Circulo, Triangulo. Cada una debe calcular área y perímetro.

### Ejercicio 3: Sistema de Notificaciones
Crea interfaces para diferentes tipos de notificaciones (Email, SMS, Push) e implementa el patrón Strategy.

### Ejercicio 4: Jerarquía de Animales
Implementa una jerarquía de animales con métodos virtuales y override. Usa polimorfismo para manejar diferentes tipos.

### Ejercicio 5: Sistema de Pagos
Crea un sistema de pagos con diferentes métodos (Efectivo, Tarjeta, Transferencia) usando interfaces.

### Ejercicio 6: Factory de Vehículos
Implementa el patrón Factory para crear diferentes tipos de vehículos con propiedades específicas.

### Ejercicio 7: Sistema de Logging
Crea un sistema de logging con diferentes implementaciones (Consola, Archivo, Base de Datos) usando interfaces.

### Ejercicio 8: Jerarquía de Usuarios
Implementa un sistema de usuarios con diferentes roles (Usuario, Admin, Moderador) usando herencia.

### Ejercicio 9: Sistema de Reportes
Crea un sistema de reportes con diferentes formatos (PDF, Excel, HTML) usando el patrón Strategy.

### Ejercicio 10: Proyecto Integrador - Sistema de Biblioteca Avanzado
Implementa un sistema completo que incluya:
- Jerarquía de usuarios (Usuario, Bibliotecario, Admin)
- Diferentes tipos de materiales (Libro, Revista, DVD)
- Sistema de préstamos con diferentes estrategias
- Interfaces para diferentes operaciones
- Patrones de diseño aplicados

## 📝 Quiz de Autoevaluación

1. ¿Cuál es la diferencia entre herencia e interfaces?
2. ¿Qué significa polimorfismo de inclusión?
3. ¿Cuándo usarías una clase abstracta en lugar de una interfaz?
4. ¿Qué es el patrón Factory y cuándo lo usarías?
5. ¿Por qué es importante usar métodos virtuales en la clase base?

## 🚀 Siguiente Nivel

Una vez que hayas completado todos los ejercicios y comprendas los conceptos, estarás listo para el **Mid Level 3: Manejo de Excepciones y Generics**.

## 💡 Consejos de Estudio

- Practica creando jerarquías de clases realistas
- Experimenta con diferentes patrones de diseño
- Usa interfaces para definir contratos claros
- Implementa polimorfismo en situaciones prácticas
- Crea diagramas de clases para visualizar las relaciones

¡Estás construyendo una base sólida para el desarrollo profesional! 🚀
