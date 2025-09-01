# Clase 1: Herencia en C#

## 🎯 Objetivos de la Clase
- Comprender qué es la herencia y por qué es importante
- Aprender a crear jerarquías de clases
- Entender la relación entre clases base y derivadas
- Dominar el uso de constructores en herencia

## 📚 Contenido Teórico

### 1. ¿Qué es la Herencia?

La herencia es un **mecanismo fundamental** de la programación orientada a objetos que permite crear nuevas clases basadas en clases existentes. Una clase derivada (hija) hereda todos los miembros públicos y protegidos de su clase base (padre).

#### Beneficios de la Herencia:
- **Reutilización de código**: Evita duplicar código común
- **Jerarquía de clases**: Organiza clases en relaciones lógicas
- **Extensibilidad**: Permite agregar funcionalidad sin modificar código existente
- **Polimorfismo**: Base para implementar comportamientos diferentes

### 2. Sintaxis Básica de Herencia

#### 2.1 Declaración de Herencia

```csharp
using System;

// Clase base (padre)
public class Animal
{
    // Propiedades comunes a todos los animales
    public string Nombre { get; set; }
    public int Edad { get; set; }
    public string Especie { get; set; }
    
    // Constructor de la clase base
    public Animal(string nombre, int edad, string especie)
    {
        Nombre = nombre;
        Edad = edad;
        Especie = especie;
    }
    
    // Método común a todos los animales
    public virtual void HacerSonido()
    {
        Console.WriteLine("El animal hace un sonido");
    }
    
    public void MostrarInformacion()
    {
        Console.WriteLine($"Nombre: {Nombre}");
        Console.WriteLine($"Edad: {Edad} años");
        Console.WriteLine($"Especie: {Especie}");
    }
}

// Clase derivada (hija) - hereda de Animal
public class Perro : Animal
{
    // Propiedad específica de Perro
    public string Raza { get; set; }
    
    // Constructor de Perro que llama al constructor de Animal
    public Perro(string nombre, int edad, string raza) 
        : base(nombre, edad, "Perro") // Llama al constructor de la clase base
    {
        Raza = raza;
    }
    
    // Sobrescribe el método de la clase base
    public override void HacerSonido()
    {
        Console.WriteLine("¡Guau! ¡Guau!");
    }
    
    // Método específico de Perro
    public void Ladrar()
    {
        Console.WriteLine($"{Nombre} está ladrando");
    }
    
    // Sobrescribe el método de mostrar información
    public new void MostrarInformacion()
    {
        base.MostrarInformacion(); // Llama al método de la clase base
        Console.WriteLine($"Raza: {Raza}");
    }
}

// Otra clase derivada
public class Gato : Animal
{
    public string Color { get; set; }
    
    public Gato(string nombre, int edad, string color) 
        : base(nombre, edad, "Gato")
    {
        Color = color;
    }
    
    public override void HacerSonido()
    {
        Console.WriteLine("¡Miau! ¡Miau!");
    }
    
    public void Ronronear()
    {
        Console.WriteLine($"{Nombre} está ronroneando");
    }
}

// Programa principal
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== HERENCIA EN C# ===");
        
        // Crear instancias de las clases derivadas
        Perro miPerro = new Perro("Buddy", 3, "Golden Retriever");
        Gato miGato = new Gato("Whiskers", 2, "Naranja");
        
        // Usar métodos heredados
        Console.WriteLine("\n=== INFORMACIÓN DEL PERRO ===");
        miPerro.MostrarInformacion();
        miPerro.HacerSonido();
        miPerro.Ladrar();
        
        Console.WriteLine("\n=== INFORMACIÓN DEL GATO ===");
        miGato.MostrarInformacion();
        miGato.HacerSonido();
        miGato.Ronronear();
        
        // Demostrar polimorfismo
        Console.WriteLine("\n=== POLIMORFISMO ===");
        Animal[] animales = { miPerro, miGato };
        
        foreach (Animal animal in animales)
        {
            Console.WriteLine($"\n{animal.Nombre} ({animal.Especie}):");
            animal.HacerSonido(); // Llamada polimórfica
        }
    }
}
```

#### Explicación Línea por Línea:

**Línea 5: `public class Animal`**
- Define la clase base (padre)
- `public` permite que otras clases hereden de ella

**Línea 6-8: Propiedades comunes**
- `Nombre`, `Edad`, `Especie` son propiedades que todos los animales comparten
- Se heredarán automáticamente a las clases derivadas

**Línea 11: Constructor de la clase base**
- Se ejecuta cuando se crea cualquier animal
- Inicializa las propiedades comunes

**Línea 20: `public virtual void HacerSonido()`**
- `virtual` permite que las clases derivadas sobrescriban este método
- Proporciona comportamiento por defecto

**Línea 25: `public class Perro : Animal`**
- `: Animal` indica que `Perro` hereda de `Animal`
- `Perro` obtiene automáticamente todas las propiedades y métodos públicos de `Animal`

**Línea 30: `public Perro(string nombre, int edad, string raza) : base(nombre, edad, "Perro")`**
- Constructor de `Perro` que llama al constructor de `Animal`
- `: base(...)` pasa parámetros al constructor de la clase base
- `"Perro"` se pasa como especie automáticamente

**Línea 36: `public override void HacerSonido()`**
- `override` sobrescribe el método virtual de la clase base
- Proporciona comportamiento específico para perros

**Línea 44: `public new void MostrarInformacion()`**
- `new` oculta el método de la clase base
- Permite implementar comportamiento completamente diferente

**Línea 46: `base.MostrarInformacion();`**
- `base` se refiere a la clase base
- Llama al método original de `Animal`

### 3. Tipos de Herencia

#### 3.1 Herencia Simple

```csharp
// Herencia simple - una clase hereda de otra
public class Vehiculo
{
    public string Marca { get; set; }
    public string Modelo { get; set; }
    public int Anio { get; set; }
    
    public Vehiculo(string marca, string modelo, int anio)
    {
        Marca = marca;
        Modelo = modelo;
        Anio = anio;
    }
    
    public virtual void Arrancar()
    {
        Console.WriteLine("El vehículo está arrancando");
    }
    
    public void MostrarInformacion()
    {
        Console.WriteLine($"Marca: {Marca}");
        Console.WriteLine($"Modelo: {Modelo}");
        Console.WriteLine($"Año: {Anio}");
    }
}

public class Coche : Vehiculo
{
    public int NumeroPuertas { get; set; }
    
    public Coche(string marca, string modelo, int anio, int numeroPuertas) 
        : base(marca, modelo, anio)
    {
        NumeroPuertas = numeroPuertas;
    }
    
    public override void Arrancar()
    {
        Console.WriteLine("El coche está arrancando con llave");
    }
    
    public void AbrirPuertas()
    {
        Console.WriteLine($"Abriendo {NumeroPuertas} puertas");
    }
}

public class Moto : Vehiculo
{
    public int Cilindrada { get; set; }
    
    public Moto(string marca, string modelo, int anio, int cilindrada) 
        : base(marca, modelo, anio)
    {
        Cilindrada = cilindrada;
    }
    
    public override void Arrancar()
    {
        Console.WriteLine("La moto está arrancando con patada");
    }
    
    public void PonerCasco()
    {
        Console.WriteLine("Poniéndose el casco de seguridad");
    }
}
```

#### 3.2 Herencia Multinivel

```csharp
// Herencia multinivel - cadena de herencia
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
    
    public virtual decimal CalcularSalario()
    {
        return SalarioBase;
    }
    
    public virtual void MostrarInformacion()
    {
        Console.WriteLine($"Empleado: {Nombre} {Apellido}");
        Console.WriteLine($"Salario Base: ${SalarioBase:F2}");
    }
}

public class Gerente : Empleado
{
    public decimal Bono { get; set; }
    
    public Gerente(string nombre, string apellido, decimal salarioBase, decimal bono) 
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
        base.MostrarInformacion();
        Console.WriteLine($"Bono: ${Bono:F2}");
        Console.WriteLine($"Salario Total: ${CalcularSalario():F2}");
    }
    
    public void GestionarEquipo()
    {
        Console.WriteLine($"{Nombre} está gestionando el equipo");
    }
}

public class Director : Gerente
{
    public decimal Acciones { get; set; }
    
    public Director(string nombre, string apellido, decimal salarioBase, decimal bono, decimal acciones) 
        : base(nombre, apellido, salarioBase, bono)
    {
        Acciones = acciones;
    }
    
    public override decimal CalcularSalario()
    {
        return SalarioBase + Bono + Acciones;
    }
    
    public override void MostrarInformacion()
    {
        base.MostrarInformacion();
        Console.WriteLine($"Acciones: ${Acciones:F2}");
        Console.WriteLine($"Salario Total con Acciones: ${CalcularSalario():F2}");
    }
    
    public void TomarDecisionesEstrategicas()
    {
        Console.WriteLine($"{Nombre} está tomando decisiones estratégicas");
    }
}
```

### 4. Constructores en Herencia

#### 4.1 Llamada a Constructores de Clase Base

```csharp
public class Persona
{
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public DateTime FechaNacimiento { get; set; }
    
    // Constructor por defecto
    public Persona()
    {
        Console.WriteLine("Constructor de Persona (por defecto)");
    }
    
    // Constructor con parámetros
    public Persona(string nombre, string apellido, DateTime fechaNacimiento)
    {
        Console.WriteLine("Constructor de Persona (con parámetros)");
        Nombre = nombre;
        Apellido = apellido;
        FechaNacimiento = fechaNacimiento;
    }
    
    public virtual void Saludar()
    {
        Console.WriteLine($"Hola, soy {Nombre} {Apellido}");
    }
}

public class Estudiante : Persona
{
    public string Matricula { get; set; }
    public string Carrera { get; set; }
    
    // Constructor por defecto
    public Estudiante() : base()
    {
        Console.WriteLine("Constructor de Estudiante (por defecto)");
    }
    
    // Constructor con parámetros
    public Estudiante(string nombre, string apellido, DateTime fechaNacimiento, 
                     string matricula, string carrera) 
        : base(nombre, apellido, fechaNacimiento)
    {
        Console.WriteLine("Constructor de Estudiante (con parámetros)");
        Matricula = matricula;
        Carrera = carrera;
    }
    
    public override void Saludar()
    {
        base.Saludar();
        Console.WriteLine($"Soy estudiante de {Carrera} con matrícula {Matricula}");
    }
}

public class Profesor : Persona
{
    public string Departamento { get; set; }
    public int AniosExperiencia { get; set; }
    
    public Profesor(string nombre, string apellido, DateTime fechaNacimiento, 
                   string departamento, int aniosExperiencia) 
        : base(nombre, apellido, fechaNacimiento)
    {
        Departamento = departamento;
        AniosExperiencia = aniosExperiencia;
    }
    
    public override void Saludar()
    {
        base.Saludar();
        Console.WriteLine($"Soy profesor del departamento de {Departamento} con {AniosExperiencia} años de experiencia");
    }
}
```

#### 4.2 Orden de Ejecución de Constructores

```csharp
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== ORDEN DE EJECUCIÓN DE CONSTRUCTORES ===");
        
        Console.WriteLine("\n1. Creando Estudiante por defecto:");
        Estudiante estudiante1 = new Estudiante();
        
        Console.WriteLine("\n2. Creando Estudiante con parámetros:");
        Estudiante estudiante2 = new Estudiante("Juan", "Pérez", 
            new DateTime(2000, 5, 15), "2024001", "Ingeniería Informática");
        
        Console.WriteLine("\n3. Creando Profesor:");
        Profesor profesor = new Profesor("María", "García", 
            new DateTime(1985, 8, 20), "Informática", 10);
        
        Console.WriteLine("\n=== SALUDOS ===");
        estudiante1.Saludar();
        estudiante2.Saludar();
        profesor.Saludar();
    }
}
```

### 5. Acceso a Miembros Heredados

#### 5.1 Palabra Clave base

```csharp
public class Forma
{
    public string Color { get; set; }
    public bool Relleno { get; set; }
    
    public Forma(string color, bool relleno)
    {
        Color = color;
        Relleno = relleno;
    }
    
    public virtual double CalcularArea()
    {
        return 0;
    }
    
    public virtual double CalcularPerimetro()
    {
        return 0;
    }
    
    public virtual void MostrarInformacion()
    {
        Console.WriteLine($"Color: {Color}");
        Console.WriteLine($"Relleno: {Relleno}");
        Console.WriteLine($"Área: {CalcularArea():F2}");
        Console.WriteLine($"Perímetro: {CalcularPerimetro():F2}");
    }
}

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
    
    public override double CalcularArea()
    {
        return Base * Altura;
    }
    
    public override double CalcularPerimetro()
    {
        return 2 * (Base + Altura);
    }
    
    public override void MostrarInformacion()
    {
        Console.WriteLine("=== RECTÁNGULO ===");
        base.MostrarInformacion(); // Llama al método de la clase base
        Console.WriteLine($"Base: {Base}");
        Console.WriteLine($"Altura: {Altura}");
    }
}

public class Circulo : Forma
{
    public double Radio { get; set; }
    
    public Circulo(string color, bool relleno, double radio) 
        : base(color, relleno)
    {
        Radio = radio;
    }
    
    public override double CalcularArea()
    {
        return Math.PI * Radio * Radio;
    }
    
    public override double CalcularPerimetro()
    {
        return 2 * Math.PI * Radio;
    }
    
    public override void MostrarInformacion()
    {
        Console.WriteLine("=== CÍRCULO ===");
        base.MostrarInformacion(); // Llama al método de la clase base
        Console.WriteLine($"Radio: {Radio}");
    }
}
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Jerarquía de Empleados
```csharp
// Crear una jerarquía de empleados con diferentes tipos
public class EmpleadoBase
{
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public decimal SalarioBase { get; set; }
    
    public EmpleadoBase(string nombre, string apellido, decimal salarioBase)
    {
        Nombre = nombre;
        Apellido = apellido;
        SalarioBase = salarioBase;
    }
    
    public virtual decimal CalcularSalario()
    {
        return SalarioBase;
    }
    
    public virtual void MostrarInformacion()
    {
        Console.WriteLine($"Empleado: {Nombre} {Apellido}");
        Console.WriteLine($"Salario Base: ${SalarioBase:F2}");
    }
}

// Crear clases derivadas: EmpleadoTiempoCompleto, EmpleadoTiempoParcial, Consultor
// Cada una con sus propias características y cálculo de salario
```

### Ejercicio 2: Sistema de Vehículos
```csharp
// Crear una jerarquía de vehículos
public class VehiculoBase
{
    public string Marca { get; set; }
    public string Modelo { get; set; }
    public int Anio { get; set; }
    public string Color { get; set; }
    
    public VehiculoBase(string marca, string modelo, int anio, string color)
    {
        Marca = marca;
        Modelo = modelo;
        Anio = anio;
        Color = color;
    }
    
    public virtual void Arrancar()
    {
        Console.WriteLine("El vehículo está arrancando");
    }
    
    public virtual void Detener()
    {
        Console.WriteLine("El vehículo se está deteniendo");
    }
}

// Crear clases derivadas: Coche, Moto, Camion, Bicicleta
// Cada una con métodos específicos y comportamiento único
```

### Ejercicio 3: Jerarquía de Animales
```csharp
// Crear una jerarquía más compleja de animales
public class AnimalBase
{
    public string Nombre { get; set; }
    public int Edad { get; set; }
    public double Peso { get; set; }
    
    public AnimalBase(string nombre, int edad, double peso)
    {
        Nombre = nombre;
        Edad = edad;
        Peso = peso;
    }
    
    public virtual void Comer()
    {
        Console.WriteLine($"{Nombre} está comiendo");
    }
    
    public virtual void Dormir()
    {
        Console.WriteLine($"{Nombre} está durmiendo");
    }
}

// Crear clases derivadas: Mamifero, Ave, Reptil
// Y luego clases más específicas: Perro, Gato, Aguila, Serpiente
```

## 🔍 Conceptos Importantes a Recordar

1. **La herencia permite reutilizar código** de clases existentes
2. **Una clase derivada hereda** todos los miembros públicos y protegidos de su clase base
3. **El constructor de la clase base** se ejecuta antes que el de la clase derivada
4. **La palabra clave `base`** permite acceder a miembros de la clase base
5. **Los métodos `virtual`** pueden ser sobrescritos en las clases derivadas
6. **La herencia multinivel** crea cadenas de clases relacionadas
7. **El orden de ejecución** es: constructor de clase base → constructor de clase derivada
8. **La herencia establece relaciones** "es-un" entre clases

## ❓ Preguntas de Repaso

1. ¿Cuál es la diferencia entre una clase base y una clase derivada?
2. ¿Por qué es importante llamar al constructor de la clase base?
3. ¿Cuándo usarías la palabra clave `base`?
4. ¿Qué significa `virtual` en un método?
5. ¿Cómo funciona la herencia multinivel?

## 🚀 Siguiente Paso

En la próxima clase aprenderemos sobre **Polimorfismo y Métodos Virtuales**, donde veremos cómo implementar comportamientos diferentes en clases derivadas.

---

## 📚 Navegación del Módulo 2

| Clase | Tema | Enlace |
|-------|------|--------|
| **Clase 1** | **Herencia en C#** | ← Estás aquí |
| [Clase 2](clase_2_polimorfismo.md) | Polimorfismo y Métodos Virtuales | Siguiente → |
| [Clase 3](clase_3_interfaces.md) | Interfaces en C# | |
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

- [Herencia en C#](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/inheritance)
- [Constructores en herencia](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/constructors#constructor-inheritance)
- [Palabra clave base](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/base)

---

**¡Excelente! Ahora entiendes los fundamentos de la herencia en C#! 🎯**
