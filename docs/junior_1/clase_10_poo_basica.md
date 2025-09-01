# Clase 10: Programación Orientada a Objetos Básica en C#

## 🎯 Objetivos de la Clase
- Comprender los conceptos fundamentales de POO
- Aprender a crear y usar clases en C#
- Entender constructores, propiedades y métodos
- Dominar la instanciación y uso de objetos

---

## 📚 Navegación del Módulo 1

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_introduccion.md) | Introducción a C# y .NET | |
| [Clase 2](clase_2_variables_tipos.md) | Variables y Tipos de Datos | |
| [Clase 3](clase_3_operadores.md) | Operadores y Expresiones | |
| [Clase 4](clase_4_estructuras_control.md) | Estructuras de Control | |
| [Clase 5](clase_5_colecciones.md) | Colecciones | |
| [Clase 6](clase_6_strings.md) | Manipulación de Strings | |
| [Clase 7](clase_7_funciones.md) | Métodos y Funciones | |
| [Clase 8](clase_8_namespaces.md) | Namespaces y Organización | |
| [Clase 9](clase_9_manejo_errores.md) | Manejo de Errores | ← Anterior |
| **Clase 10** | **Programación Orientada a Objetos Básica** | ← Estás aquí |

**← [Volver al README del Módulo 1](../junior_1/README.md)**

**🎉 ¡Módulo 1 Completado! → [Ir al Módulo 2](../junior_2/README.md)**

## 📚 Contenido Teórico

### 1. ¿Qué es la Programación Orientada a Objetos?

La POO es un **paradigma de programación** que organiza el código en objetos que contienen datos y comportamiento. Los objetos son instancias de clases que representan entidades del mundo real.

#### Conceptos Fundamentales:
- **Clase**: Plantilla para crear objetos
- **Objeto**: Instancia de una clase
- **Encapsulación**: Ocultar datos y exponer métodos
- **Herencia**: Reutilizar código de otras clases
- **Polimorfismo**: Mismo método, diferentes comportamientos

### 2. Creación de Clases Básicas

#### 2.1 Estructura de una Clase

```csharp
using System;

// Definición de una clase
public class Persona
{
    // Campos (variables de instancia)
    private string nombre;
    private int edad;
    private string email;
    
    // Constructor
    public Persona(string nombre, int edad, string email)
    {
        this.nombre = nombre;
        this.edad = edad;
        this.email = email;
    }
    
    // Propiedades
    public string Nombre
    {
        get { return nombre; }
        set { nombre = value; }
    }
    
    public int Edad
    {
        get { return edad; }
        set 
        { 
            if (value >= 0 && value <= 150)
                edad = value;
            else
                throw new ArgumentException("La edad debe estar entre 0 y 150");
        }
    }
    
    public string Email
    {
        get { return email; }
        set { email = value; }
    }
    
    // Métodos
    public void MostrarInformacion()
    {
        Console.WriteLine($"Nombre: {nombre}");
        Console.WriteLine($"Edad: {edad}");
        Console.WriteLine($"Email: {email}");
    }
    
    public bool EsMayorDeEdad()
    {
        return edad >= 18;
    }
    
    public void CumplirAnios()
    {
        edad++;
        Console.WriteLine($"¡{nombre} ha cumplido {edad} años!");
    }
}

// Programa principal
class Program
{
    static void Main(string[] args)
    {
        // Crear objetos (instanciar la clase)
        Persona persona1 = new Persona("Juan Pérez", 25, "juan@email.com");
        Persona persona2 = new Persona("María García", 17, "maria@email.com");
        
        // Usar métodos de los objetos
        Console.WriteLine("=== INFORMACIÓN DE PERSONA 1 ===");
        persona1.MostrarInformacion();
        Console.WriteLine($"¿Es mayor de edad? {persona1.EsMayorDeEdad()}");
        
        Console.WriteLine("\n=== INFORMACIÓN DE PERSONA 2 ===");
        persona2.MostrarInformacion();
        Console.WriteLine($"¿Es mayor de edad? {persona2.EsMayorDeEdad()}");
        
        // Usar propiedades
        persona1.Edad = 26;
        persona1.Email = "juan.nuevo@email.com";
        
        Console.WriteLine("\n=== DESPUÉS DE MODIFICAR ===");
        persona1.MostrarInformacion();
        
        // Llamar métodos
        persona1.CumplirAnios();
    }
}
```

#### Explicación de la Clase:

**Línea 4: `public class Persona`**
- `public` permite acceder desde otros archivos
- `class` define una nueva clase
- `Persona` es el nombre de la clase

**Línea 6-8: Campos privados**
- `private` oculta los campos desde fuera de la clase
- Son variables que almacenan el estado del objeto

**Línea 11: Constructor**
- Método especial que se ejecuta al crear un objeto
- Inicializa los campos con valores proporcionados
- `this` se refiere al objeto actual

**Línea 20-25: Propiedades**
- `get` permite leer el valor
- `set` permite modificar el valor
- `value` es el nuevo valor asignado

**Línea 35: Métodos**
- Funciones que definen el comportamiento del objeto
- Pueden acceder a los campos privados

### 3. Constructores y Sobrecarga

#### 3.1 Múltiples Constructores

```csharp
public class Producto
{
    private string nombre;
    private decimal precio;
    private int stock;
    
    // Constructor por defecto
    public Producto()
    {
        nombre = "Sin nombre";
        precio = 0.0m;
        stock = 0;
    }
    
    // Constructor con parámetros
    public Producto(string nombre, decimal precio)
    {
        this.nombre = nombre;
        this.precio = precio;
        this.stock = 0;
    }
    
    // Constructor completo
    public Producto(string nombre, decimal precio, int stock)
    {
        this.nombre = nombre;
        this.precio = precio;
        this.stock = stock;
    }
    
    // Propiedades
    public string Nombre { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    
    // Métodos
    public void MostrarInformacion()
    {
        Console.WriteLine($"Producto: {nombre}");
        Console.WriteLine($"Precio: ${precio:F2}");
        Console.WriteLine($"Stock: {stock} unidades");
    }
    
    public bool HayStock()
    {
        return stock > 0;
    }
    
    public void Vender(int cantidad)
    {
        if (cantidad <= stock)
        {
            stock -= cantidad;
            Console.WriteLine($"Se vendieron {cantidad} unidades de {nombre}");
        }
        else
        {
            Console.WriteLine($"No hay suficiente stock. Disponible: {stock}");
        }
    }
}

// Uso de constructores
class Program
{
    static void Main(string[] args)
    {
        // Usar diferentes constructores
        Producto producto1 = new Producto();                    // Constructor por defecto
        Producto producto2 = new Producto("Laptop", 999.99m);   // Constructor con 2 parámetros
        Producto producto3 = new Producto("Mouse", 25.50m, 50); // Constructor completo
        
        producto1.MostrarInformacion();
        Console.WriteLine();
        
        producto2.MostrarInformacion();
        Console.WriteLine();
        
        producto3.MostrarInformacion();
        producto3.Vender(10);
        producto3.MostrarInformacion();
    }
}
```

### 4. Propiedades Automáticas

#### 4.1 Sintaxis Simplificada

```csharp
public class Empleado
{
    // Propiedades automáticas (C# 3.0+)
    public string Nombre { get; set; }
    public string Cargo { get; set; }
    public decimal Salario { get; set; }
    public DateTime FechaContratacion { get; set; }
    
    // Propiedad de solo lectura
    public int AniosAntiguedad 
    { 
        get 
        { 
            return DateTime.Now.Year - FechaContratacion.Year; 
        } 
    }
    
    // Propiedad con lógica personalizada
    public decimal SalarioAnual
    {
        get { return Salario * 12; }
    }
    
    // Constructor
    public Empleado(string nombre, string cargo, decimal salario)
    {
        Nombre = nombre;
        Cargo = cargo;
        Salario = salario;
        FechaContratacion = DateTime.Now;
    }
    
    // Métodos
    public void MostrarInformacion()
    {
        Console.WriteLine($"Empleado: {Nombre}");
        Console.WriteLine($"Cargo: {Cargo}");
        Console.WriteLine($"Salario mensual: ${Salario:F2}");
        Console.WriteLine($"Salario anual: ${SalarioAnual:F2}");
        Console.WriteLine($"Años de antigüedad: {AniosAntiguedad}");
    }
    
    public void AumentarSalario(decimal porcentaje)
    {
        Salario += Salario * (porcentaje / 100);
        Console.WriteLine($"Salario aumentado en {porcentaje}%. Nuevo salario: ${Salario:F2}");
    }
}
```

### 5. Métodos de Instancia vs. Estáticos

#### 5.1 Diferencias y Uso

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
    
    // Método estático que no necesita instancia
    public static double CalcularPromedio(int[] numeros)
    {
        if (numeros.Length == 0) return 0;
        
        int suma = 0;
        foreach (int num in numeros)
        {
            suma += num;
        }
        
        return (double)suma / numeros.Length;
    }
}

// Uso de métodos
class Program
{
    static void Main(string[] args)
    {
        // Método de instancia - necesita crear objeto
        Calculadora calc = new Calculadora();
        int resultado = calc.Sumar(5, 3);
        Console.WriteLine($"5 + 3 = {resultado}");
        
        // Método estático - se llama directamente desde la clase
        int producto = Calculadora.Multiplicar(4, 6);
        Console.WriteLine($"4 * 6 = {producto}");
        
        // Método estático con array
        int[] numeros = { 10, 20, 30, 40, 50 };
        double promedio = Calculadora.CalcularPromedio(numeros);
        Console.WriteLine($"Promedio: {promedio}");
    }
}
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Clase Rectángulo
```csharp
public class Rectangulo
{
    public double Base { get; set; }
    public double Altura { get; set; }
    
    public Rectangulo(double baseRect, double altura)
    {
        Base = baseRect;
        Altura = altura;
    }
    
    public double CalcularArea()
    {
        return Base * Altura;
    }
    
    public double CalcularPerimetro()
    {
        return 2 * (Base + Altura);
    }
    
    public bool EsCuadrado()
    {
        return Base == Altura;
    }
    
    public void MostrarInformacion()
    {
        Console.WriteLine($"Rectángulo: {Base} x {Altura}");
        Console.WriteLine($"Área: {CalcularArea():F2}");
        Console.WriteLine($"Perímetro: {CalcularPerimetro():F2}");
        Console.WriteLine($"¿Es cuadrado? {EsCuadrado()}");
    }
}
```

### Ejercicio 2: Clase Banco
```csharp
public class CuentaBancaria
{
    public string NumeroCuenta { get; private set; }
    public string Titular { get; set; }
    public decimal Saldo { get; private set; }
    
    public CuentaBancaria(string numeroCuenta, string titular, decimal saldoInicial = 0)
    {
        NumeroCuenta = numeroCuenta;
        Titular = titular;
        Saldo = saldoInicial;
    }
    
    public void Depositar(decimal cantidad)
    {
        if (cantidad > 0)
        {
            Saldo += cantidad;
            Console.WriteLine($"Depósito de ${cantidad:F2} realizado. Nuevo saldo: ${Saldo:F2}");
        }
        else
        {
            Console.WriteLine("La cantidad debe ser mayor a 0");
        }
    }
    
    public bool Retirar(decimal cantidad)
    {
        if (cantidad > 0 && cantidad <= Saldo)
        {
            Saldo -= cantidad;
            Console.WriteLine($"Retiro de ${cantidad:F2} realizado. Nuevo saldo: ${Saldo:F2}");
            return true;
        }
        else
        {
            Console.WriteLine("No se puede realizar el retiro");
            return false;
        }
    }
    
    public void MostrarSaldo()
    {
        Console.WriteLine($"Cuenta: {NumeroCuenta}");
        Console.WriteLine($"Titular: {Titular}");
        Console.WriteLine($"Saldo: ${Saldo:F2}");
    }
}
```

## 🔍 Conceptos Importantes a Recordar

1. **Las clases son plantillas** para crear objetos
2. **Los objetos son instancias** de una clase
3. **Los constructores inicializan** los objetos
4. **Las propiedades controlan** el acceso a los datos
5. **Los métodos definen** el comportamiento del objeto
6. **Los métodos estáticos** no necesitan instancia
7. **La encapsulación protege** los datos internos
8. **Las propiedades automáticas** simplifican el código
9. **this se refiere** al objeto actual

## ❓ Preguntas de Repaso

1. ¿Cuál es la diferencia entre una clase y un objeto?
2. ¿Qué es un constructor y cuándo se ejecuta?
3. ¿Cuándo usarías métodos estáticos vs. de instancia?
4. ¿Qué significa la palabra clave this?
5. ¿Cómo funcionan las propiedades automáticas?

## 🚀 Siguiente Paso

En el próximo módulo aprenderemos sobre **Herencia y Polimorfismo**, donde veremos cómo crear jerarquías de clases en C#.

---

## 📚 Recursos Adicionales

- [Clases en C#](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/)
- [Constructores en C#](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/constructors/)
- [Propiedades en C#](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/properties/)

---

**¡Excelente! Has completado el Módulo 1 de C#! 🎯**
