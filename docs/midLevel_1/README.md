# 🎯 Mid Level 1: Programación Orientada a Objetos

## 📚 Descripción

En este nivel aprenderás los fundamentos de la Programación Orientada a Objetos (POO) en C#: clases, objetos, encapsulación, constructores y métodos. Este es un paso crucial para convertirte en un desarrollador más avanzado.

## 🎯 Objetivos de Aprendizaje

- Entender los conceptos fundamentales de POO
- Crear clases y objetos en C#
- Implementar encapsulación con propiedades
- Usar constructores y sobrecarga de métodos
- Entender el concepto de this y static
- Crear aplicaciones modulares y reutilizables

## 📖 Contenido Teórico

### 1. Conceptos Fundamentales de POO

#### ¿Qué es la Programación Orientada a Objetos?
La POO es un paradigma de programación que organiza el código en objetos que contienen datos y comportamiento. Los principios fundamentales son:

- **Encapsulación**: Ocultar datos y exponer solo lo necesario
- **Abstracción**: Simplificar la complejidad del mundo real
- **Herencia**: Reutilizar código de clases existentes
- **Polimorfismo**: Usar diferentes implementaciones de la misma interfaz

#### Clases vs Objetos
```csharp
// Clase: Es el "molde" o "plantilla"
public class Persona
{
    // Propiedades y métodos van aquí
}

// Objeto: Es una "instancia" de la clase
Persona juan = new Persona(); // juan es un objeto de tipo Persona
```

### 2. Creación de Clases

#### Estructura Básica de una Clase
```csharp
[modificadores] class NombreClase
{
    // Campos (variables de instancia)
    private string nombre;
    private int edad;
    
    // Propiedades
    public string Nombre { get; set; }
    public int Edad { get; set; }
    
    // Métodos
    public void Saludar()
    {
        Console.WriteLine($"¡Hola! Soy {Nombre}");
    }
    
    // Constructor
    public NombreClase()
    {
        // Código de inicialización
    }
}
```

#### Modificadores de Acceso
```csharp
public class Ejemplo
{
    public string publico;        // Accesible desde cualquier lugar
    private string privado;       // Solo accesible desde la misma clase
    protected string protegido;   // Accesible desde la clase y sus derivadas
    internal string interno;      // Accesible desde el mismo assembly
    protected internal string protegidoInterno; // Combinación de ambos
}
```

### 3. Campos y Propiedades

#### Campos (Fields)
```csharp
public class Producto
{
    // Campos privados (encapsulación)
    private string nombre;
    private decimal precio;
    private int stock;
    
    // Campo constante
    private const decimal IVA = 0.21m;
    
    // Campo readonly (solo se puede asignar en constructor)
    private readonly DateTime fechaCreacion;
}
```

#### Propiedades
```csharp
public class Producto
{
    private string nombre;
    private decimal precio;
    
    // Propiedad auto-implementada
    public string Nombre { get; set; }
    
    // Propiedad con lógica personalizada
    public decimal Precio
    {
        get { return precio; }
        set 
        { 
            if (value >= 0)
                precio = value;
            else
                throw new ArgumentException("El precio no puede ser negativo");
        }
    }
    
    // Propiedad de solo lectura
    public decimal PrecioConIVA => precio * (1 + 0.21m);
    
    // Propiedad de solo escritura
    public string CodigoInterno { set; }
}
```

### 4. Constructores

#### Constructor por Defecto
```csharp
public class Persona
{
    public string Nombre { get; set; }
    public int Edad { get; set; }
    
    // Constructor por defecto (se crea automáticamente si no hay otros)
    public Persona()
    {
        // Inicialización por defecto
    }
}
```

#### Constructor con Parámetros
```csharp
public class Persona
{
    public string Nombre { get; set; }
    public int Edad { get; set; }
    
    public Persona(string nombre, int edad)
    {
        Nombre = nombre;
        Edad = edad;
    }
}
```

#### Sobrecarga de Constructores
```csharp
public class Persona
{
    public string Nombre { get; set; }
    public int Edad { get; set; }
    public string Email { get; set; }
    
    // Constructor básico
    public Persona(string nombre, int edad)
    {
        Nombre = nombre;
        Edad = edad;
    }
    
    // Constructor completo
    public Persona(string nombre, int edad, string email) : this(nombre, edad)
    {
        Email = email;
    }
    
    // Constructor de copia
    public Persona(Persona otra) : this(otra.Nombre, otra.Edad, otra.Email)
    {
    }
}
```

### 5. Métodos

#### Tipos de Métodos
```csharp
public class Calculadora
{
    // Método de instancia
    public int Sumar(int a, int b)
    {
        return a + b;
    }
    
    // Método estático
    public static int Multiplicar(int a, int b)
    {
        return a * b;
    }
    
    // Método privado (solo usado internamente)
    private void ValidarParametros(int a, int b)
    {
        if (a < 0 || b < 0)
            throw new ArgumentException("Los números deben ser positivos");
    }
    
    // Método con parámetros opcionales
    public decimal CalcularPrecio(decimal precio, decimal descuento = 0)
    {
        return precio * (1 - descuento);
    }
}
```

#### Sobrecarga de Métodos
```csharp
public class Calculadora
{
    public int Sumar(int a, int b)
    {
        return a + b;
    }
    
    public int Sumar(int a, int b, int c)
    {
        return a + b + c;
    }
    
    public double Sumar(double a, double b)
    {
        return a + b;
    }
    
    public string Sumar(string a, string b)
    {
        return a + b; // Concatenación
    }
}
```

### 6. Palabra Clave this

```csharp
public class Persona
{
    private string nombre;
    private int edad;
    
    public Persona(string nombre, int edad)
    {
        this.nombre = nombre; // this se refiere a la instancia actual
        this.edad = edad;
    }
    
    public void CompararEdad(Persona otra)
    {
        if (this.edad > otra.edad)
            Console.WriteLine($"{this.nombre} es mayor que {otra.nombre}");
    }
}
```

### 7. Miembros Estáticos

```csharp
public class Utilidades
{
    // Campo estático (compartido entre todas las instancias)
    public static int contadorInstancias = 0;
    
    // Propiedad estática
    public static DateTime FechaActual => DateTime.Now;
    
    // Constructor estático (se ejecuta una sola vez)
    static Utilidades()
    {
        Console.WriteLine("Clase Utilidades inicializada");
    }
    
    // Método estático
    public static bool EsPar(int numero)
    {
        return numero % 2 == 0;
    }
    
    // Método de instancia que usa miembros estáticos
    public void IncrementarContador()
    {
        contadorInstancias++;
    }
}

// Uso de miembros estáticos
int numero = 10;
bool esPar = Utilidades.EsPar(numero); // No necesitas crear una instancia
```

### 8. Encapsulación Avanzada

#### Propiedades con Validación
```csharp
public class CuentaBancaria
{
    private decimal saldo;
    
    public decimal Saldo
    {
        get { return saldo; }
        private set // Solo se puede modificar desde dentro de la clase
        {
            if (value < 0)
                throw new InvalidOperationException("El saldo no puede ser negativo");
            saldo = value;
        }
    }
    
    public void Depositar(decimal monto)
    {
        if (monto <= 0)
            throw new ArgumentException("El monto debe ser positivo");
        
        Saldo += monto; // Usa la propiedad, no el campo directamente
    }
    
    public bool Retirar(decimal monto)
    {
        if (monto <= 0)
            throw new ArgumentException("El monto debe ser positivo");
        
        if (monto > Saldo)
            return false;
        
        Saldo -= monto;
        return true;
    }
}
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Clase Rectángulo
Crea una clase Rectángulo con propiedades para base y altura, y métodos para calcular área y perímetro.

### Ejercicio 2: Clase Estudiante
Implementa una clase Estudiante con nombre, edad, calificaciones y métodos para calcular promedio y determinar si aprobó.

### Ejercicio 3: Clase Banco
Crea una clase Banco que maneje cuentas bancarias con métodos para depositar, retirar y consultar saldo.

### Ejercicio 4: Clase Calculadora Avanzada
Implementa una calculadora con métodos estáticos para operaciones matemáticas y métodos de instancia para historial.

### Ejercicio 5: Clase Producto con Validación
Crea una clase Producto con validaciones para precio, stock y métodos para actualizar inventario.

### Ejercicio 6: Clase Persona con Constructores
Implementa múltiples constructores para la clase Persona y un método de copia.

### Ejercicio 7: Clase Utilidades Matemáticas
Crea una clase estática con métodos para cálculos matemáticos comunes (factorial, potencia, etc.).

### Ejercicio 8: Clase Agenda de Contactos
Implementa una agenda que permita agregar, buscar y eliminar contactos usando encapsulación.

### Ejercicio 9: Clase Biblioteca
Crea un sistema de biblioteca con clases para Libro, Usuario y Préstamo.

### Ejercicio 10: Proyecto Integrador - Sistema de Gestión de Empleados
Implementa un sistema completo que incluya:
- Clase Empleado con propiedades y métodos
- Cálculo de salario con bonificaciones
- Gestión de departamentos
- Reportes y estadísticas
- Validaciones y manejo de errores

## 📝 Quiz de Autoevaluación

1. ¿Cuál es la diferencia entre una clase y un objeto?
2. ¿Qué significa encapsulación en POO?
3. ¿Cuándo usarías métodos estáticos?
4. ¿Qué es la sobrecarga de métodos?
5. ¿Por qué es importante usar propiedades en lugar de campos públicos?

## 🚀 Siguiente Nivel

Una vez que hayas completado todos los ejercicios y comprendas los conceptos, estarás listo para el **Mid Level 2: Herencia, Polimorfismo e Interfaces**.

## 💡 Consejos de Estudio

- Practica creando diferentes tipos de clases
- Experimenta con diferentes niveles de encapsulación
- Usa constructores para inicializar objetos correctamente
- Implementa validaciones en las propiedades
- Crea relaciones entre clases para entender mejor la POO

¡Estás avanzando hacia un nivel más profesional de desarrollo! 🚀
