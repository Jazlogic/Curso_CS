# 🚀 Clase 3: Polimorfismo Avanzado

## 📋 Información de la Clase

- **Módulo**: Junior Level 3 - Programación Orientada a Objetos Avanzada
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 2 (Interfaces Avanzadas)

## 🎯 Objetivos de Aprendizaje

- Dominar el polimorfismo paramétrico con genéricos
- Implementar polimorfismo de inclusión con herencia
- Crear sistemas polimórficos complejos
- Utilizar polimorfismo para diseño de frameworks
- Aplicar polimorfismo en patrones de diseño

---

## 📚 Navegación del Módulo 3

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_herencia_multiple.md) | Herencia Múltiple y Composición | |
| [Clase 2](clase_2_interfaces_avanzadas.md) | Interfaces Avanzadas | ← Anterior |
| **Clase 3** | **Polimorfismo Avanzado** | ← Estás aquí |
| [Clase 4](clase_4_patrones_diseno.md) | Patrones de Diseño Básicos | Siguiente → |
| [Clase 5](clase_5_principios_solid.md) | Principios SOLID | |
| [Clase 6](clase_6_arquitectura_modular.md) | Arquitectura Modular | |
| [Clase 7](clase_7_reflection_avanzada.md) | Reflection Avanzada | |
| [Clase 8](clase_8_serializacion_avanzada.md) | Serialización Avanzada | |
| [Clase 9](clase_9_testing_unitario.md) | Testing Unitario | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final Integrador | |

**← [Volver al README del Módulo 3](../junior_3/README.md)**

---

## 📚 Contenido Teórico

### 1. Polimorfismo Paramétrico con Genéricos

El polimorfismo paramétrico permite crear código que funciona con diferentes tipos sin conocer el tipo específico en tiempo de compilación.

```csharp
// Clase genérica que implementa polimorfismo paramétrico
public class Contenedor<T>
{
    private T _valor;
    
    public Contenedor(T valor)
    {
        _valor = valor;
    }
    
    public T ObtenerValor()
    {
        return _valor;
    }
    
    public void EstablecerValor(T nuevoValor)
    {
        _valor = nuevoValor;
    }
    
    public bool EsIgual(T otroValor)
    {
        return EqualityComparer<T>.Default.Equals(_valor, otroValor);
    }
    
    public override string ToString()
    {
        return _valor?.ToString() ?? "null";
    }
}

// Clase genérica con restricciones
public class ContenedorComparable<T> where T : IComparable<T>
{
    private T _valor;
    
    public ContenedorComparable(T valor)
    {
        _valor = valor;
    }
    
    public T ObtenerValor()
    {
        return _valor;
    }
    
    public bool EsMayorQue(T otroValor)
    {
        return _valor.CompareTo(otroValor) > 0;
    }
    
    public bool EsMenorQue(T otroValor)
    {
        return _valor.CompareTo(otroValor) < 0;
    }
    
    public bool EsIgual(T otroValor)
    {
        return _valor.CompareTo(otroValor) == 0;
    }
}

// Clase genérica con múltiples restricciones
public class ContenedorComplejo<T> where T : class, IComparable<T>, new()
{
    private T _valor;
    
    public ContenedorComplejo()
    {
        _valor = new T();
    }
    
    public ContenedorComplejo(T valor)
    {
        _valor = valor ?? new T();
    }
    
    public T ObtenerValor()
    {
        return _valor;
    }
    
    public void EstablecerValor(T nuevoValor)
    {
        _valor = nuevoValor ?? new T();
    }
    
    public ContenedorComplejo<T> Clonar()
    {
        return new ContenedorComplejo<T>(_valor);
    }
}

// Uso del polimorfismo paramétrico
public class Program
{
    public static void Main()
    {
        // Contenedor con string
        var contenedorString = new Contenedor<string>("Hola Mundo");
        Console.WriteLine($"Contenedor String: {contenedorString}");
        
        // Contenedor con int
        var contenedorInt = new Contenedor<int>(42);
        Console.WriteLine($"Contenedor Int: {contenedorInt}");
        
        // Contenedor con DateTime
        var contenedorFecha = new Contenedor<DateTime>(DateTime.Now);
        Console.WriteLine($"Contenedor Fecha: {contenedorFecha}");
        
        // Contenedor comparable con int
        var contenedorComparable = new ContenedorComparable<int>(100);
        Console.WriteLine($"¿100 es mayor que 50? {contenedorComparable.EsMayorQue(50)}");
        Console.WriteLine($"¿100 es menor que 200? {contenedorComparable.EsMenorQue(200)}");
        
        // Contenedor comparable con string
        var contenedorStringComparable = new ContenedorComparable<string>("Zebra");
        Console.WriteLine($"¿'Zebra' es mayor que 'Abeja'? {contenedorStringComparable.EsMayorQue("Abeja")}");
        
        // Contenedor complejo
        var contenedorComplejo = new ContenedorComplejo<string>("Complejo");
        var clon = contenedorComplejo.Clonar();
        Console.WriteLine($"Clon del contenedor: {clon}");
    }
}
```

**Explicación línea por línea:**
- `public class Contenedor<T>`: Define una clase genérica que puede trabajar con cualquier tipo T
- `private T _valor`: Campo privado del tipo genérico T
- `public Contenedor(T valor)`: Constructor que recibe un valor del tipo T
- `_valor = valor`: Asigna el valor recibido al campo privado
- `public T ObtenerValor()`: Método que retorna el valor almacenado
- `return _valor`: Retorna el valor del campo privado
- `public void EstablecerValor(T nuevoValor)`: Método para cambiar el valor
- `_valor = nuevoValor`: Asigna el nuevo valor al campo
- `public bool EsIgual(T otroValor)`: Método para comparar con otro valor
- `return EqualityComparer<T>.Default.Equals(_valor, otroValor)`: Usa comparador por defecto del tipo T
- `public override string ToString()`: Sobrescribe el método ToString
- `return _valor?.ToString() ?? "null"`: Retorna string del valor o "null" si es null
- `public class ContenedorComparable<T> where T : IComparable<T>`: Clase genérica con restricción
- `where T : IComparable<T>`: Restricción que T debe implementar IComparable<T>
- `public bool EsMayorQue(T otroValor)`: Método para comparar si es mayor
- `return _valor.CompareTo(otroValor) > 0`: Retorna true si _valor es mayor que otroValor
- `public bool EsMenorQue(T otroValor)`: Método para comparar si es menor
- `return _valor.CompareTo(otroValor) < 0`: Retorna true si _valor es menor que otroValor
- `public bool EsIgual(T otroValor)`: Método para comparar si es igual
- `return _valor.CompareTo(otroValor) == 0`: Retorna true si _valor es igual a otroValor
- `public class ContenedorComplejo<T> where T : class, IComparable<T>, new()`: Clase con múltiples restricciones
- `where T : class, IComparable<T>, new()`: T debe ser clase, comparable y tener constructor sin parámetros
- `public ContenedorComplejo()`: Constructor sin parámetros
- `_valor = new T()`: Crea nueva instancia del tipo T
- `public ContenedorComplejo(T valor)`: Constructor con parámetro
- `_valor = valor ?? new T()`: Asigna valor o crea nueva instancia si es null
- `public ContenedorComplejo<T> Clonar()`: Método que retorna un clon
- `return new ContenedorComplejo<T>(_valor)`: Crea nueva instancia con el valor actual
- `var contenedorString = new Contenedor<string>("Hola Mundo")`: Crea contenedor con string
- `Console.WriteLine($"Contenedor String: {contenedorString}")`: Muestra el contenedor
- `var contenedorInt = new Contenedor<int>(42)`: Crea contenedor con int
- `Console.WriteLine($"Contenedor Int: {contenedorInt}")`: Muestra el contenedor
- `var contenedorFecha = new Contenedor<DateTime>(DateTime.Now)`: Crea contenedor con DateTime
- `Console.WriteLine($"Contenedor Fecha: {contenedorFecha}")`: Muestra el contenedor
- `var contenedorComparable = new ContenedorComparable<int>(100)`: Crea contenedor comparable con int
- `Console.WriteLine($"¿100 es mayor que 50? {contenedorComparable.EsMayorQue(50)}")`: Compara valores
- `Console.WriteLine($"¿100 es menor que 200? {contenedorComparable.EsMenorQue(200)}")`: Compara valores
- `var contenedorStringComparable = new ContenedorComparable<string>("Zebra")`: Crea contenedor comparable con string
- `Console.WriteLine($"¿'Zebra' es mayor que 'Abeja'? {contenedorStringComparable.EsMayorQue("Abeja")}")`: Compara strings
- `var contenedorComplejo = new ContenedorComplejo<string>("Complejo")`: Crea contenedor complejo
- `var clon = contenedorComplejo.Clonar()`: Crea un clon del contenedor
- `Console.WriteLine($"Clon del contenedor: {clon}")`: Muestra el clon

### 2. Polimorfismo de Inclusión con Herencia

El polimorfismo de inclusión permite que objetos de clases derivadas sean tratados como objetos de la clase base.

```csharp
// Clase base abstracta
public abstract class Forma
{
    public string Color { get; set; }
    public bool Relleno { get; set; }
    
    protected Forma(string color, bool relleno)
    {
        Color = color;
        Relleno = relleno;
    }
    
    public abstract double CalcularArea();
    public abstract double CalcularPerimetro();
    
    public virtual void Dibujar()
    {
        Console.WriteLine($"Dibujando forma de color {Color}, relleno: {Relleno}");
    }
    
    public virtual string ObtenerDescripcion()
    {
        return $"Forma - Color: {Color}, Relleno: {Relleno}";
    }
}

// Clase derivada Circulo
public class Circulo : Forma
{
    public double Radio { get; set; }
    
    public Circulo(string color, bool relleno, double radio) : base(color, relleno)
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
    
    public override void Dibujar()
    {
        Console.WriteLine($"Dibujando círculo de radio {Radio}, color {Color}");
    }
    
    public override string ObtenerDescripcion()
    {
        return $"Círculo - Radio: {Radio}, {base.ObtenerDescripcion()}";
    }
}

// Clase derivada Rectangulo
public class Rectangulo : Forma
{
    public double Ancho { get; set; }
    public double Alto { get; set; }
    
    public Rectangulo(string color, bool relleno, double ancho, double alto) : base(color, relleno)
    {
        Ancho = ancho;
        Alto = alto;
    }
    
    public override double CalcularArea()
    {
        return Ancho * Alto;
    }
    
    public override double CalcularPerimetro()
    {
        return 2 * (Ancho + Alto);
    }
    
    public override void Dibujar()
    {
        Console.WriteLine($"Dibujando rectángulo de {Ancho}x{Alto}, color {Color}");
    }
    
    public override string ObtenerDescripcion()
    {
        return $"Rectángulo - Ancho: {Ancho}, Alto: {Alto}, {base.ObtenerDescripcion()}";
    }
}

// Clase derivada Triangulo
public class Triangulo : Forma
{
    public double LadoA { get; set; }
    public double LadoB { get; set; }
    public double LadoC { get; set; }
    
    public Triangulo(string color, bool relleno, double ladoA, double ladoB, double ladoC) : base(color, relleno)
    {
        LadoA = ladoA;
        LadoB = ladoB;
        LadoC = ladoC;
    }
    
    public override double CalcularArea()
    {
        // Fórmula de Herón
        double semiperimetro = (LadoA + LadoB + LadoC) / 2;
        return Math.Sqrt(semiperimetro * (semiperimetro - LadoA) * (semiperimetro - LadoB) * (semiperimetro - LadoC));
    }
    
    public override double CalcularPerimetro()
    {
        return LadoA + LadoB + LadoC;
    }
    
    public override void Dibujar()
    {
        Console.WriteLine($"Dibujando triángulo de lados {LadoA}, {LadoB}, {LadoC}, color {Color}");
    }
    
    public override string ObtenerDescripcion()
    {
        return $"Triángulo - Lados: {LadoA}, {LadoB}, {LadoC}, {base.ObtenerDescripcion()}";
    }
}

// Uso del polimorfismo de inclusión
public class Program
{
    public static void Main()
    {
        // Crear diferentes formas
        var formas = new List<Forma>
        {
            new Circulo("Rojo", true, 5.0),
            new Rectangulo("Azul", false, 4.0, 6.0),
            new Triangulo("Verde", true, 3.0, 4.0, 5.0)
        };
        
        // Usar polimorfismo para procesar todas las formas
        foreach (var forma in formas)
        {
            Console.WriteLine($"\n{forma.ObtenerDescripcion()}");
            Console.WriteLine($"Área: {forma.CalcularArea():F2}");
            Console.WriteLine($"Perímetro: {forma.CalcularPerimetro():F2}");
            forma.Dibujar();
        }
        
        // Calcular área total de todas las formas
        double areaTotal = formas.Sum(f => f.CalcularArea());
        Console.WriteLine($"\nÁrea total de todas las formas: {areaTotal:F2}");
        
        // Encontrar la forma con mayor área
        var formaMayorArea = formas.OrderByDescending(f => f.CalcularArea()).First();
        Console.WriteLine($"Forma con mayor área: {formaMayorArea.GetType().Name}");
        
        // Filtrar formas por color
        var formasRojas = formas.Where(f => f.Color.Equals("Rojo", StringComparison.OrdinalIgnoreCase));
        Console.WriteLine($"\nFormas rojas encontradas: {formasRojas.Count()}");
    }
    
    // Método genérico que demuestra polimorfismo
    public static void ProcesarFormas<T>(List<T> formas) where T : Forma
    {
        foreach (var forma in formas)
        {
            Console.WriteLine($"Procesando: {forma.GetType().Name}");
            forma.Dibujar();
        }
    }
}
```

**Explicación línea por línea:**
- `public abstract class Forma`: Define clase base abstracta para formas
- `public string Color { get; set; }`: Propiedad para el color de la forma
- `public bool Relleno { get; set; }`: Propiedad para indicar si está rellena
- `protected Forma(string color, bool relleno)`: Constructor protegido
- `Color = color`: Asigna el color recibido
- `Relleno = relleno`: Asigna el valor de relleno
- `public abstract double CalcularArea()`: Método abstracto para calcular área
- `public abstract double CalcularPerimetro()`: Método abstracto para calcular perímetro
- `public virtual void Dibujar()`: Método virtual para dibujar la forma
- `Console.WriteLine($"Dibujando forma de color {Color}, relleno: {Relleno}")`: Mensaje genérico
- `public virtual string ObtenerDescripcion()`: Método virtual para obtener descripción
- `return $"Forma - Color: {Color}, Relleno: {Relleno}"`: Descripción genérica
- `public class Circulo : Forma`: Clase derivada que hereda de Forma
- `public double Radio { get; set; }`: Propiedad específica del círculo
- `public Circulo(string color, bool relleno, double radio) : base(color, relleno)`: Constructor que llama al base
- `Radio = radio`: Asigna el radio recibido
- `public override double CalcularArea()`: Implementa el cálculo de área del círculo
- `return Math.PI * Radio * Radio`: Retorna área usando fórmula πr²
- `public override double CalcularPerimetro()`: Implementa el cálculo de perímetro
- `return 2 * Math.PI * Radio`: Retorna perímetro usando fórmula 2πr
- `public override void Dibujar()`: Sobrescribe el método de dibujo
- `Console.WriteLine($"Dibujando círculo de radio {Radio}, color {Color}")`: Mensaje específico del círculo
- `public override string ObtenerDescripcion()`: Sobrescribe la descripción
- `return $"Círculo - Radio: {Radio}, {base.ObtenerDescripcion()}"`: Descripción específica + base
- `public class Rectangulo : Forma`: Clase derivada para rectángulos
- `public double Ancho { get; set; }`: Propiedad para el ancho
- `public double Alto { get; set; }`: Propiedad para el alto
- `public Rectangulo(string color, bool relleno, double ancho, double alto) : base(color, relleno)`: Constructor
- `Ancho = ancho`: Asigna el ancho
- `Alto = alto`: Asigna el alto
- `public override double CalcularArea()`: Implementa área del rectángulo
- `return Ancho * Alto`: Retorna área usando fórmula base × altura
- `public override double CalcularPerimetro()`: Implementa perímetro del rectángulo
- `return 2 * (Ancho + Alto)`: Retorna perímetro usando fórmula 2(base + altura)
- `public override void Dibujar()`: Sobrescribe dibujo del rectángulo
- `Console.WriteLine($"Dibujando rectángulo de {Ancho}x{Alto}, color {Color}")`: Mensaje específico
- `public override string ObtenerDescripcion()`: Sobrescribe descripción del rectángulo
- `return $"Rectángulo - Ancho: {Ancho}, Alto: {Alto}, {base.ObtenerDescripcion()}"`: Descripción específica
- `public class Triangulo : Forma`: Clase derivada para triángulos
- `public double LadoA { get; set; }`: Propiedad para primer lado
- `public double LadoB { get; set; }`: Propiedad para segundo lado
- `public double LadoC { get; set; }`: Propiedad para tercer lado
- `public Triangulo(string color, bool relleno, double ladoA, double ladoB, double ladoC) : base(color, relleno)`: Constructor
- `LadoA = ladoA`: Asigna primer lado
- `LadoB = ladoB`: Asigna segundo lado
- `LadoC = ladoC`: Asigna tercer lado
- `public override double CalcularArea()`: Implementa área del triángulo
- `double semiperimetro = (LadoA + LadoB + LadoC) / 2`: Calcula semiperímetro
- `return Math.Sqrt(semiperimetro * (semiperimetro - LadoA) * (semiperimetro - LadoB) * (semiperimetro - LadoC))`: Fórmula de Herón
- `public override double CalcularPerimetro()`: Implementa perímetro del triángulo
- `return LadoA + LadoB + LadoC`: Retorna suma de los tres lados
- `public override void Dibujar()`: Sobrescribe dibujo del triángulo
- `Console.WriteLine($"Dibujando triángulo de lados {LadoA}, {LadoB}, {LadoC}, color {Color}")`: Mensaje específico
- `public override string ObtenerDescripcion()`: Sobrescribe descripción del triángulo
- `return $"Triángulo - Lados: {LadoA}, {LadoB}, {LadoC}, {base.ObtenerDescripcion()}"`: Descripción específica
- `var formas = new List<Forma> { ... }`: Crea lista de formas usando polimorfismo
- `new Circulo("Rojo", true, 5.0)`: Crea círculo rojo relleno de radio 5
- `new Rectangulo("Azul", false, 4.0, 6.0)`: Crea rectángulo azul sin relleno de 4x6
- `new Triangulo("Verde", true, 3.0, 4.0, 5.0)`: Crea triángulo verde relleno de lados 3,4,5
- `foreach (var forma in formas)`: Itera sobre todas las formas
- `Console.WriteLine($"\n{forma.ObtenerDescripcion()}")`: Muestra descripción de cada forma
- `Console.WriteLine($"Área: {forma.CalcularArea():F2}")`: Muestra área con 2 decimales
- `Console.WriteLine($"Perímetro: {forma.CalcularPerimetro():F2}")`: Muestra perímetro con 2 decimales
- `forma.Dibujar()`: Llama al método de dibujo específico de cada forma
- `double areaTotal = formas.Sum(f => f.CalcularArea())`: Calcula área total usando LINQ
- `Console.WriteLine($"\nÁrea total de todas las formas: {areaTotal:F2}")`: Muestra área total
- `var formaMayorArea = formas.OrderByDescending(f => f.CalcularArea()).First()`: Encuentra forma con mayor área
- `Console.WriteLine($"Forma con mayor área: {formaMayorArea.GetType().Name}")`: Muestra nombre de la clase
- `var formasRojas = formas.Where(f => f.Color.Equals("Rojo", StringComparison.OrdinalIgnoreCase))`: Filtra formas rojas
- `Console.WriteLine($"\nFormas rojas encontradas: {formasRojas.Count()}")`: Muestra conteo de formas rojas
- `public static void ProcesarFormas<T>(List<T> formas) where T : Forma`: Método genérico con restricción
- `where T : Forma`: Restricción que T debe heredar de Forma
- `foreach (var forma in formas)`: Itera sobre formas del tipo T
- `Console.WriteLine($"Procesando: {forma.GetType().Name}")`: Muestra nombre de la clase
- `forma.Dibujar()`: Llama al método de dibujo

### 3. Polimorfismo de Sobre carga

El polimorfismo de sobrecarga permite tener múltiples métodos con el mismo nombre pero diferentes parámetros.

```csharp
// Clase que demuestra sobrecarga de métodos
public class Calculadora
{
    // Sobrecarga de métodos para suma
    public int Sumar(int a, int b)
    {
        return a + b;
    }
    
    public double Sumar(double a, double b)
    {
        return a + b;
    }
    
    public int Sumar(int a, int b, int c)
    {
        return a + b + c;
    }
    
    public double Sumar(params double[] numeros)
    {
        return numeros.Sum();
    }
    
    // Sobrecarga de métodos para multiplicación
    public int Multiplicar(int a, int b)
    {
        return a * b;
    }
    
    public double Multiplicar(double a, double b)
    {
        return a * b;
    }
    
    public int Multiplicar(int a, int b, int c)
    {
        return a * b * c;
    }
    
    // Sobrecarga de métodos para división
    public double Dividir(int a, int b)
    {
        if (b == 0)
            throw new DivideByZeroException("No se puede dividir por cero");
        return (double)a / b;
    }
    
    public double Dividir(double a, double b)
    {
        if (b == 0)
            throw new DivideByZeroException("No se puede dividir por cero");
        return a / b;
    }
    
    // Sobrecarga de métodos para potencia
    public int Potencia(int baseNum, int exponente)
    {
        return (int)Math.Pow(baseNum, exponente);
    }
    
    public double Potencia(double baseNum, double exponente)
    {
        return Math.Pow(baseNum, exponente);
    }
    
    // Sobrecarga de métodos para raíz cuadrada
    public double RaizCuadrada(int numero)
    {
        if (numero < 0)
            throw new ArgumentException("No se puede calcular raíz cuadrada de número negativo");
        return Math.Sqrt(numero);
    }
    
    public double RaizCuadrada(double numero)
    {
        if (numero < 0)
            throw new ArgumentException("No se puede calcular raíz cuadrada de número negativo");
        return Math.Sqrt(numero);
    }
}

// Clase que demuestra sobrecarga de constructores
public class Persona
{
    public string Nombre { get; set; }
    public int Edad { get; set; }
    public string Email { get; set; }
    public string Telefono { get; set; }
    
    // Constructor por defecto
    public Persona()
    {
        Nombre = "Sin nombre";
        Edad = 0;
        Email = "";
        Telefono = "";
    }
    
    // Constructor con nombre
    public Persona(string nombre)
    {
        Nombre = nombre;
        Edad = 0;
        Email = "";
        Telefono = "";
    }
    
    // Constructor con nombre y edad
    public Persona(string nombre, int edad)
    {
        Nombre = nombre;
        Edad = edad;
        Email = "";
        Telefono = "";
    }
    
    // Constructor con nombre, edad y email
    public Persona(string nombre, int edad, string email)
    {
        Nombre = nombre;
        Edad = edad;
        Email = email;
        Telefono = "";
    }
    
    // Constructor completo
    public Persona(string nombre, int edad, string email, string telefono)
    {
        Nombre = nombre;
        Edad = edad;
        Email = email;
        Telefono = telefono;
    }
    
    // Constructor de copia
    public Persona(Persona otra)
    {
        Nombre = otra.Nombre;
        Edad = otra.Edad;
        Email = otra.Email;
        Telefono = otra.Telefono;
    }
    
    public override string ToString()
    {
        return $"Persona: {Nombre}, Edad: {Edad}, Email: {Email}, Teléfono: {Telefono}";
    }
}

// Uso de la sobrecarga
public class Program
{
    public static void Main()
    {
        var calculadora = new Calculadora();
        
        // Usar diferentes sobrecargas de Sumar
        Console.WriteLine($"Suma de enteros: {calculadora.Sumar(5, 3)}");
        Console.WriteLine($"Suma de doubles: {calculadora.Sumar(5.5, 3.3)}");
        Console.WriteLine($"Suma de tres enteros: {calculadora.Sumar(5, 3, 2)}");
        Console.WriteLine($"Suma de múltiples doubles: {calculadora.Sumar(1.1, 2.2, 3.3, 4.4)}");
        
        // Usar diferentes sobrecargas de Multiplicar
        Console.WriteLine($"\nMultiplicación de enteros: {calculadora.Multiplicar(5, 3)}");
        Console.WriteLine($"Multiplicación de doubles: {calculadora.Multiplicar(5.5, 3.3)}");
        Console.WriteLine($"Multiplicación de tres enteros: {calculadora.Multiplicar(5, 3, 2)}");
        
        // Usar diferentes sobrecargas de Dividir
        Console.WriteLine($"\nDivisión de enteros: {calculadora.Dividir(10, 3)}");
        Console.WriteLine($"División de doubles: {calculadora.Dividir(10.0, 3.0)}");
        
        // Usar diferentes sobrecargas de Potencia
        Console.WriteLine($"\nPotencia de enteros: {calculadora.Potencia(2, 8)}");
        Console.WriteLine($"Potencia de doubles: {calculadora.Potencia(2.5, 3.0)}");
        
        // Usar diferentes sobrecargas de RaizCuadrada
        Console.WriteLine($"\nRaíz cuadrada de entero: {calculadora.RaizCuadrada(16)}");
        Console.WriteLine($"Raíz cuadrada de double: {calculadora.RaizCuadrada(16.0)}");
        
        // Crear personas usando diferentes constructores
        var persona1 = new Persona();
        var persona2 = new Persona("Juan");
        var persona3 = new Persona("María", 25);
        var persona4 = new Persona("Carlos", 30, "carlos@email.com");
        var persona5 = new Persona("Ana", 28, "ana@email.com", "123-456-789");
        var persona6 = new Persona(persona5); // Constructor de copia
        
        Console.WriteLine($"\n{persona1}");
        Console.WriteLine($"{persona2}");
        Console.WriteLine($"{persona3}");
        Console.WriteLine($"{persona4}");
        Console.WriteLine($"{persona5}");
        Console.WriteLine($"{persona6}");
    }
}
```

**Explicación línea por línea:**
- `public class Calculadora`: Clase que demuestra sobrecarga de métodos
- `public int Sumar(int a, int b)`: Método para sumar dos enteros
- `return a + b`: Retorna suma de enteros
- `public double Sumar(double a, double b)`: Sobrecarga para sumar dos doubles
- `return a + b`: Retorna suma de doubles
- `public int Sumar(int a, int b, int c)`: Sobrecarga para sumar tres enteros
- `return a + b + c`: Retorna suma de tres enteros
- `public double Sumar(params double[] numeros)`: Sobrecarga con parámetros variables
- `return numeros.Sum()`: Retorna suma de array de doubles usando LINQ
- `public int Multiplicar(int a, int b)`: Método para multiplicar dos enteros
- `return a * b`: Retorna producto de enteros
- `public double Multiplicar(double a, double b)`: Sobrecarga para multiplicar doubles
- `return a * b`: Retorna producto de doubles
- `public int Multiplicar(int a, int b, int c)`: Sobrecarga para multiplicar tres enteros
- `return a * b * c`: Retorna producto de tres enteros
- `public double Dividir(int a, int b)`: Método para dividir enteros
- `if (b == 0)`: Verifica división por cero
- `throw new DivideByZeroException("No se puede dividir por cero")`: Lanza excepción
- `return (double)a / b`: Retorna división convertida a double
- `public double Dividir(double a, double b)`: Sobrecarga para dividir doubles
- `if (b == 0)`: Verifica división por cero
- `throw new DivideByZeroException("No se puede dividir por cero")`: Lanza excepción
- `return a / b`: Retorna división de doubles
- `public int Potencia(int baseNum, int exponente)`: Método para potencia de enteros
- `return (int)Math.Pow(baseNum, exponente)`: Retorna potencia usando Math.Pow
- `public double Potencia(double baseNum, double exponente)`: Sobrecarga para potencia de doubles
- `return Math.Pow(baseNum, exponente)`: Retorna potencia de doubles
- `public double RaizCuadrada(int numero)`: Método para raíz cuadrada de entero
- `if (numero < 0)`: Verifica número negativo
- `throw new ArgumentException("No se puede calcular raíz cuadrada de número negativo")`: Lanza excepción
- `return Math.Sqrt(numero)`: Retorna raíz cuadrada usando Math.Sqrt
- `public double RaizCuadrada(double numero)`: Sobrecarga para raíz cuadrada de double
- `if (numero < 0)`: Verifica número negativo
- `throw new ArgumentException("No se puede calcular raíz cuadrada de número negativo")`: Lanza excepción
- `return Math.Sqrt(numero)`: Retorna raíz cuadrada de double
- `public class Persona`: Clase que demuestra sobrecarga de constructores
- `public string Nombre { get; set; }`: Propiedad para el nombre
- `public int Edad { get; set; }`: Propiedad para la edad
- `public string Email { get; set; }`: Propiedad para el email
- `public string Telefono { get; set; }`: Propiedad para el teléfono
- `public Persona()`: Constructor por defecto
- `Nombre = "Sin nombre"`: Asigna valor por defecto al nombre
- `Edad = 0`: Asigna valor por defecto a la edad
- `Email = ""`: Asigna string vacío al email
- `Telefono = ""`: Asigna string vacío al teléfono
- `public Persona(string nombre)`: Constructor con solo nombre
- `Nombre = nombre`: Asigna el nombre recibido
- `Edad = 0`: Asigna valor por defecto a la edad
- `Email = ""`: Asigna string vacío al email
- `Telefono = ""`: Asigna string vacío al teléfono
- `public Persona(string nombre, int edad)`: Constructor con nombre y edad
- `Nombre = nombre`: Asigna el nombre recibido
- `Edad = edad`: Asigna la edad recibida
- `Email = ""`: Asigna string vacío al email
- `Telefono = ""`: Asigna string vacío al teléfono
- `public Persona(string nombre, int edad, string email)`: Constructor con nombre, edad y email
- `Nombre = nombre`: Asigna el nombre recibido
- `Edad = edad`: Asigna la edad recibida
- `Email = email`: Asigna el email recibido
- `Telefono = ""`: Asigna string vacío al teléfono
- `public Persona(string nombre, int edad, string email, string telefono)`: Constructor completo
- `Nombre = nombre`: Asigna el nombre recibido
- `Edad = edad`: Asigna la edad recibida
- `Email = email`: Asigna el email recibido
- `Telefono = telefono`: Asigna el teléfono recibido
- `public Persona(Persona otra)`: Constructor de copia
- `Nombre = otra.Nombre`: Copia el nombre de la otra persona
- `Edad = otra.Edad`: Copia la edad de la otra persona
- `Email = otra.Email`: Copia el email de la otra persona
- `Telefono = otra.Telefono`: Copia el teléfono de la otra persona
- `public override string ToString()`: Sobrescribe el método ToString
- `return $"Persona: {Nombre}, Edad: {Edad}, Email: {Email}, Teléfono: {Telefono}"`: Retorna descripción completa
- `var calculadora = new Calculadora()`: Crea instancia de la calculadora
- `Console.WriteLine($"Suma de enteros: {calculadora.Sumar(5, 3)}")`: Usa sobrecarga de enteros
- `Console.WriteLine($"Suma de doubles: {calculadora.Sumar(5.5, 3.3)}")`: Usa sobrecarga de doubles
- `Console.WriteLine($"Suma de tres enteros: {calculadora.Sumar(5, 3, 2)}")`: Usa sobrecarga de tres parámetros
- `Console.WriteLine($"Suma de múltiples doubles: {calculadora.Sumar(1.1, 2.2, 3.3, 4.4)}")`: Usa sobrecarga con params
- `Console.WriteLine($"\nMultiplicación de enteros: {calculadora.Multiplicar(5, 3)}")`: Usa sobrecarga de multiplicación
- `Console.WriteLine($"Multiplicación de doubles: {calculadora.Multiplicar(5.5, 3.3)}")`: Usa sobrecarga de multiplicación
- `Console.WriteLine($"Multiplicación de tres enteros: {calculadora.Multiplicar(5, 3, 2)}")`: Usa sobrecarga de tres parámetros
- `Console.WriteLine($"\nDivisión de enteros: {calculadora.Dividir(10, 3)}")`: Usa sobrecarga de división
- `Console.WriteLine($"División de doubles: {calculadora.Dividir(10.0, 3.0)}")`: Usa sobrecarga de división
- `Console.WriteLine($"\nPotencia de enteros: {calculadora.Potencia(2, 8)}")`: Usa sobrecarga de potencia
- `Console.WriteLine($"Potencia de doubles: {calculadora.Potencia(2.5, 3.0)}")`: Usa sobrecarga de potencia
- `Console.WriteLine($"\nRaíz cuadrada de entero: {calculadora.RaizCuadrada(16)}")`: Usa sobrecarga de raíz cuadrada
- `Console.WriteLine($"Raíz cuadrada de double: {calculadora.RaizCuadrada(16.0)}")`: Usa sobrecarga de raíz cuadrada
- `var persona1 = new Persona()`: Crea persona usando constructor por defecto
- `var persona2 = new Persona("Juan")`: Crea persona usando constructor con nombre
- `var persona3 = new Persona("María", 25)`: Crea persona usando constructor con nombre y edad
- `var persona4 = new Persona("Carlos", 30, "carlos@email.com")`: Crea persona usando constructor con nombre, edad y email
- `var persona5 = new Persona("Ana", 28, "ana@email.com", "123-456-789")`: Crea persona usando constructor completo
- `var persona6 = new Persona(persona5)`: Crea persona usando constructor de copia
- `Console.WriteLine($"\n{persona1}")`: Muestra persona creada con constructor por defecto
- `Console.WriteLine($"{persona2}")`: Muestra persona creada con constructor de nombre
- `Console.WriteLine($"{persona3}")`: Muestra persona creada con constructor de nombre y edad
- `Console.WriteLine($"{persona4}")`: Muestra persona creada con constructor de nombre, edad y email
- `Console.WriteLine($"{persona5}")`: Muestra persona creada con constructor completo
- `Console.WriteLine($"{persona6}")`: Muestra persona creada con constructor de copia

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Sistema de Figuras Geométricas
Crea un sistema de figuras geométricas que demuestre polimorfismo de inclusión, incluyendo métodos para calcular área, perímetro y dibujar.

### Ejercicio 2: Calculadora Avanzada
Implementa una calculadora que use polimorfismo de sobrecarga para operaciones matemáticas con diferentes tipos de datos.

### Ejercicio 3: Sistema de Empleados
Crea un sistema de empleados con diferentes tipos (Empleado, Gerente, Vendedor) que demuestre polimorfismo de inclusión.

## 🔍 Puntos Clave

1. **El polimorfismo paramétrico** permite crear código genérico que funciona con múltiples tipos
2. **El polimorfismo de inclusión** permite tratar objetos derivados como objetos de la clase base
3. **El polimorfismo de sobrecarga** permite múltiples métodos con el mismo nombre pero diferentes parámetros
4. **Las restricciones genéricas** garantizan que los tipos cumplan ciertos requisitos
5. **El polimorfismo** es fundamental para crear frameworks y patrones de diseño flexibles

## 📚 Recursos Adicionales

- [Polimorfismo en C# - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/polymorphism)
- [Genéricos en C# - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/)
- [Design Patterns - GoF](https://refactoring.guru/design-patterns)

---

**🎯 ¡Has completado la Clase 3! Ahora dominas el polimorfismo avanzado en C#**

**📚 [Siguiente: Clase 4 - Patrones de Diseño Básicos](clase_4_patrones_diseno.md)**
