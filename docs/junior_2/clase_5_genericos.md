# Clase 5: Genéricos en C#

## 🎯 Objetivos de la Clase
- Comprender qué son los genéricos y su propósito
- Aprender a crear clases, métodos e interfaces genéricas
- Entender las restricciones de tipos en genéricos
- Dominar el uso de genéricos para código reutilizable

## 📚 Contenido Teórico

### 1. ¿Qué son los Genéricos?

Los **genéricos** son una característica de C# que permite crear **código reutilizable** que funciona con diferentes tipos de datos sin especificar el tipo concreto hasta el momento de su uso. Los genéricos proporcionan **type safety** (seguridad de tipos) en tiempo de compilación.

#### Ventajas de los Genéricos:
- **Reutilización de código**: Un solo algoritmo para múltiples tipos
- **Type safety**: El compilador verifica los tipos en tiempo de compilación
- **Mejor rendimiento**: Evita el boxing/unboxing de tipos de valor
- **IntelliSense**: Mejor soporte del IDE para autocompletado
- **Flexibilidad**: Funciona con cualquier tipo que cumpla las restricciones

### 2. Clases Genéricas

#### 2.1 Sintaxis Básica

```csharp
using System;
using System.Collections.Generic;

// Clase genérica que puede trabajar con cualquier tipo
public class Contenedor<T>
{
    // Campo privado del tipo genérico
    private T elemento;
    
    // Propiedad pública del tipo genérico
    public T Elemento
    {
        get { return elemento; }
        set { elemento = value; }
    }
    
    // Constructor que acepta el tipo genérico
    public Contenedor(T elemento)
    {
        this.elemento = elemento;
    }
    
    // Método que retorna el tipo genérico
    public T ObtenerElemento()
    {
        return elemento;
    }
    
    // Método que acepta el tipo genérico como parámetro
    public void EstablecerElemento(T nuevoElemento)
    {
        elemento = nuevoElemento;
        Console.WriteLine($"Elemento cambiado a: {elemento}");
    }
    
    // Método que compara con otro elemento del mismo tipo
    public bool EsIgual(T otroElemento)
    {
        return elemento.Equals(otroElemento);
    }
    
    // Método que muestra información del tipo
    public void MostrarTipo()
    {
        Console.WriteLine($"Tipo del elemento: {typeof(T).Name}");
        Console.WriteLine($"Valor del elemento: {elemento}");
    }
}

// Clase genérica con múltiples tipos de parámetros
public class ParClaveValor<TClave, TValor>
{
    // Propiedades de diferentes tipos genéricos
    public TClave Clave { get; set; }
    public TValor Valor { get; set; }
    
    // Constructor
    public ParClaveValor(TClave clave, TValor valor)
    {
        Clave = clave;
        Valor = valor;
    }
    
    // Método que usa ambos tipos genéricos
    public void MostrarPar()
    {
        Console.WriteLine($"Clave: {Clave} ({typeof(TClave).Name})");
        Console.WriteLine($"Valor: {Valor} ({typeof(TValor).Name})");
    }
    
    // Método que intercambia los valores
    public void Intercambiar()
    {
        TClave claveTemp = Clave;
        TValor valorTemp = Valor;
        
        Clave = claveTemp;
        Valor = valorTemp;
        
        Console.WriteLine("Valores intercambiados");
    }
}

// Clase genérica con restricciones
public class Comparador<T> where T : IComparable<T>
{
    // Método que compara dos elementos del tipo genérico
    public int Comparar(T elemento1, T elemento2)
    {
        return elemento1.CompareTo(elemento2);
    }
    
    // Método que encuentra el mayor de dos elementos
    public T ObtenerMayor(T elemento1, T elemento2)
    {
        return elemento1.CompareTo(elemento2) > 0 ? elemento1 : elemento2;
    }
    
    // Método que encuentra el menor de dos elementos
    public T ObtenerMenor(T elemento1, T elemento2)
    {
        return elemento1.CompareTo(elemento2) < 0 ? elemento1 : elemento2;
    }
    
    // Método que ordena un array
    public void Ordenar(T[] elementos)
    {
        Array.Sort(elementos);
        Console.WriteLine("Array ordenado");
    }
}
```

#### 2.2 Uso de Clases Genéricas

```csharp
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== CLASES GENÉRICAS EN C# ===");
        
        // Crear contenedor para diferentes tipos
        Contenedor<int> contenedorInt = new Contenedor<int>(42);
        Contenedor<string> contenedorString = new Contenedor<string>("Hola Mundo");
        Contenedor<double> contenedorDouble = new Contenedor<double>(3.14159);
        
        // Usar métodos de la clase genérica
        Console.WriteLine("\n=== CONTENEDOR DE ENTEROS ===");
        contenedorInt.MostrarTipo();
        contenedorInt.EstablecerElemento(100);
        Console.WriteLine($"¿Es igual a 100? {contenedorInt.EsIgual(100)}");
        
        Console.WriteLine("\n=== CONTENEDOR DE STRINGS ===");
        contenedorString.MostrarTipo();
        contenedorString.EstablecerElemento("Nuevo mensaje");
        
        Console.WriteLine("\n=== CONTENEDOR DE DOUBLES ===");
        contenedorDouble.MostrarTipo();
        contenedorDouble.EstablecerElemento(2.71828);
        
        // Usar clase genérica con múltiples tipos
        Console.WriteLine("\n=== PARES CLAVE-VALOR ===");
        ParClaveValor<int, string> par1 = new ParClaveValor<int, string>(1, "Uno");
        ParClaveValor<string, double> par2 = new ParClaveValor<string, double>("Pi", 3.14159);
        
        par1.MostrarPar();
        par2.MostrarPar();
        
        // Usar comparador genérico
        Console.WriteLine("\n=== COMPARADOR GENÉRICO ===");
        Comparador<int> comparadorInt = new Comparador<int>();
        Comparador<string> comparadorString = new Comparador<string>();
        
        int mayorInt = comparadorInt.ObtenerMayor(10, 20);
        string mayorString = comparadorString.ObtenerMayor("A", "Z");
        
        Console.WriteLine($"Mayor entre 10 y 20: {mayorInt}");
        Console.WriteLine($"Mayor entre 'A' y 'Z': {mayorString}");
        
        // Ordenar arrays
        int[] numeros = { 5, 2, 8, 1, 9 };
        string[] palabras = { "Zebra", "Abeja", "Mono", "Delfin" };
        
        comparadorInt.Ordenar(numeros);
        comparadorString.Ordenar(palabras);
        
        Console.WriteLine("Números ordenados: " + string.Join(", ", numeros));
        Console.WriteLine("Palabras ordenadas: " + string.Join(", ", palabras));
    }
}
```

### 3. Métodos Genéricos

#### 3.1 Métodos Genéricos en Clases No Genéricas

```csharp
// Clase no genérica que contiene métodos genéricos
public class Utilidades
{
    // Método genérico que intercambia dos elementos
    public static void Intercambiar<T>(ref T elemento1, ref T elemento2)
    {
        T temporal = elemento1;
        elemento1 = elemento2;
        elemento2 = temporal;
        Console.WriteLine("Elementos intercambiados");
    }
    
    // Método genérico que busca un elemento en un array
    public static bool Contiene<T>(T[] array, T elemento)
    {
        foreach (T item in array)
        {
            if (item.Equals(elemento))
            {
                return true;
            }
        }
        return false;
    }
    
    // Método genérico que cuenta ocurrencias
    public static int ContarOcurrencias<T>(T[] array, T elemento)
    {
        int contador = 0;
        foreach (T item in array)
        {
            if (item.Equals(elemento))
            {
                contador++;
            }
        }
        return contador;
    }
    
    // Método genérico que encuentra el primer elemento que cumple una condición
    public static T EncontrarPrimero<T>(T[] array, Func<T, bool> predicado)
    {
        foreach (T item in array)
        {
            if (predicado(item))
            {
                return item;
            }
        }
        throw new InvalidOperationException("No se encontró ningún elemento que cumpla la condición");
    }
    
    // Método genérico que filtra elementos
    public static List<T> Filtrar<T>(T[] array, Func<T, bool> predicado)
    {
        List<T> resultado = new List<T>();
        foreach (T item in array)
        {
            if (predicado(item))
            {
                resultado.Add(item);
            }
        }
        return resultado;
    }
    
    // Método genérico que mapea elementos
    public static List<TResultado> Mapear<T, TResultado>(T[] array, Func<T, TResultado> mapeador)
    {
        List<TResultado> resultado = new List<TResultado>();
        foreach (T item in array)
        {
            resultado.Add(mapeador(item));
        }
        return resultado;
    }
}

// Clase que demuestra el uso de métodos genéricos
public class DemostracionMetodosGenericos
{
    public static void Ejecutar()
    {
        Console.WriteLine("=== MÉTODOS GENÉRICOS ===");
        
        // Intercambiar elementos
        int a = 10, b = 20;
        Console.WriteLine($"Antes: a={a}, b={b}");
        Utilidades.Intercambiar(ref a, ref b);
        Console.WriteLine($"Después: a={a}, b={b}");
        
        string str1 = "Hola", str2 = "Mundo";
        Console.WriteLine($"Antes: str1={str1}, str2={str2}");
        Utilidades.Intercambiar(ref str1, ref str2);
        Console.WriteLine($"Después: str1={str1}, str2={str2}");
        
        // Buscar elementos
        int[] numeros = { 1, 2, 3, 4, 5 };
        string[] palabras = { "Manzana", "Banana", "Naranja" };
        
        Console.WriteLine($"¿Contiene 3? {Utilidades.Contiene(numeros, 3)}");
        Console.WriteLine($"¿Contiene 'Pera'? {Utilidades.Contiene(palabras, "Pera")}");
        
        // Contar ocurrencias
        int[] numerosRepetidos = { 1, 2, 2, 3, 2, 4 };
        Console.WriteLine($"Ocurrencias del número 2: {Utilidades.ContarOcurrencias(numerosRepetidos, 2)}");
        
        // Encontrar primer elemento que cumpla condición
        try
        {
            int primerPar = Utilidades.EncontrarPrimero(numeros, n => n % 2 == 0);
            Console.WriteLine($"Primer número par: {primerPar}");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        
        // Filtrar elementos
        List<int> numerosPares = Utilidades.Filtrar(numeros, n => n % 2 == 0);
        Console.WriteLine($"Números pares: {string.Join(", ", numerosPares)}");
        
        // Mapear elementos
        List<string> numerosComoTexto = Utilidades.Mapear(numeros, n => $"Número {n}");
        Console.WriteLine($"Números como texto: {string.Join(", ", numerosComoTexto)}");
    }
}
```

### 4. Interfaces Genéricas

#### 4.1 Definición de Interfaces Genéricas

```csharp
// Interfaz genérica para operaciones de comparación
public interface IComparable<T>
{
    int CompararCon(T otro);
    bool EsMayorQue(T otro);
    bool EsMenorQue(T otro);
    bool EsIgualA(T otro);
}

// Interfaz genérica para operaciones de colección
public interface IColeccion<T>
{
    void Agregar(T elemento);
    bool Eliminar(T elemento);
    bool Contiene(T elemento);
    int Contar { get; }
    void Limpiar();
    T[] ObtenerTodos();
}

// Interfaz genérica para operaciones de iteración
public interface IIterable<T>
{
    IIterador<T> CrearIterador();
}

public interface IIterador<T>
{
    T Actual { get; }
    bool TieneSiguiente { get; }
    void Siguiente();
    void Reiniciar();
}

// Implementación de interfaces genéricas
public class ListaGenerica<T> : IColeccion<T>, IIterable<T>
{
    private List<T> elementos;
    
    public ListaGenerica()
    {
        elementos = new List<T>();
    }
    
    // Implementación de IColeccion<T>
    public void Agregar(T elemento)
    {
        elementos.Add(elemento);
        Console.WriteLine($"Elemento agregado: {elemento}");
    }
    
    public bool Eliminar(T elemento)
    {
        bool eliminado = elementos.Remove(elemento);
        if (eliminado)
        {
            Console.WriteLine($"Elemento eliminado: {elemento}");
        }
        return eliminado;
    }
    
    public bool Contiene(T elemento)
    {
        return elementos.Contains(elemento);
    }
    
    public int Contar => elementos.Count;
    
    public void Limpiar()
    {
        elementos.Clear();
        Console.WriteLine("Lista limpiada");
    }
    
    public T[] ObtenerTodos()
    {
        return elementos.ToArray();
    }
    
    // Implementación de IIterable<T>
    public IIterador<T> CrearIterador()
    {
        return new IteradorLista<T>(elementos);
    }
    
    // Método específico de la clase
    public void MostrarElementos()
    {
        Console.WriteLine($"Lista contiene {Contar} elementos:");
        foreach (T elemento in elementos)
        {
            Console.WriteLine($"- {elemento}");
        }
    }
}

// Implementación del iterador
public class IteradorLista<T> : IIterador<T>
{
    private List<T> elementos;
    private int indiceActual;
    
    public IteradorLista(List<T> elementos)
    {
        this.elementos = elementos;
        this.indiceActual = -1;
    }
    
    public T Actual => elementos[indiceActual];
    
    public bool TieneSiguiente => indiceActual < elementos.Count - 1;
    
    public void Siguiente()
    {
        if (TieneSiguiente)
        {
            indiceActual++;
        }
    }
    
    public void Reiniciar()
    {
        indiceActual = -1;
    }
}
```

### 5. Restricciones de Tipos

#### 5.1 Tipos de Restricciones

```csharp
// Clase genérica con restricción de tipo de referencia
public class ContenedorReferencia<T> where T : class
{
    private T elemento;
    
    public ContenedorReferencia(T elemento)
    {
        this.elemento = elemento;
    }
    
    public T Elemento
    {
        get { return elemento; }
        set { elemento = value; }
    }
    
    public bool EsNull()
    {
        return elemento == null;
    }
}

// Clase genérica con restricción de tipo de valor
public class ContenedorValor<T> where T : struct
{
    private T elemento;
    
    public ContenedorValor(T elemento)
    {
        this.elemento = elemento;
    }
    
    public T Elemento
    {
        get { return elemento; }
        set { elemento = value; }
    }
    
    public bool EsDefault()
    {
        return elemento.Equals(default(T));
    }
}

// Clase genérica con restricción de constructor
public class ContenedorConstructor<T> where T : new()
{
    private T elemento;
    
    public ContenedorConstructor()
    {
        elemento = new T(); // Solo funciona si T tiene constructor sin parámetros
    }
    
    public T Elemento
    {
        get { return elemento; }
        set { elemento = value; }
    }
}

// Clase genérica con restricción de interfaz
public class ContenedorComparable<T> where T : IComparable<T>
{
    private T elemento;
    
    public ContenedorComparable(T elemento)
    {
        this.elemento = elemento;
    }
    
    public T Elemento
    {
        get { return elemento; }
        set { elemento = value; }
    }
    
    public int CompararCon(T otro)
    {
        return elemento.CompareTo(otro);
    }
}

// Clase genérica con restricción de clase base
public class ContenedorAnimal<T> where T : Animal
{
    private T animal;
    
    public ContenedorAnimal(T animal)
    {
        this.animal = animal;
    }
    
    public T Animal
    {
        get { return animal; }
        set { animal = value; }
    }
    
    public void HacerSonido()
    {
        animal.HacerSonido();
    }
}

// Clase genérica con múltiples restricciones
public class ContenedorComplejo<T> where T : class, IComparable<T>, new()
{
    private T elemento;
    
    public ContenedorComplejo()
    {
        elemento = new T();
    }
    
    public T Elemento
    {
        get { return elemento; }
        set { elemento = value; }
    }
    
    public int CompararCon(T otro)
    {
        return elemento.CompareTo(otro);
    }
    
    public bool EsNull()
    {
        return elemento == null;
    }
}

// Clase base para demostrar restricciones
public class Animal
{
    public string Nombre { get; set; }
    
    public Animal(string nombre)
    {
        Nombre = nombre;
    }
    
    public virtual void HacerSonido()
    {
        Console.WriteLine($"{Nombre} hace un sonido");
    }
}

public class Perro : Animal
{
    public Perro(string nombre) : base(nombre) { }
    
    public override void HacerSonido()
    {
        Console.WriteLine($"{Nombre} dice: ¡Guau! ¡Guau!");
    }
}
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Pila Genérica
```csharp
// Crear una clase Pila<T> genérica
public class Pila<T>
{
    private List<T> elementos;
    
    public Pila()
    {
        elementos = new List<T>();
    }
    
    public void Apilar(T elemento);
    public T Desapilar();
    public T VerTope();
    public bool EstaVacia();
    public int Contar { get; }
}
```

### Ejercicio 2: Cola Genérica
```csharp
// Crear una clase Cola<T> genérica
public class Cola<T>
{
    private List<T> elementos;
    
    public Cola()
    {
        elementos = new List<T>();
    }
    
    public void Encolar(T elemento);
    public T Desencolar();
    public T VerPrimero();
    public bool EstaVacia();
    public int Contar { get; }
}
```

### Ejercicio 3: Diccionario Genérico
```csharp
// Crear una clase Diccionario<K, V> genérica
public class Diccionario<K, V>
{
    private List<ParClaveValor<K, V>> pares;
    
    public Diccionario()
    {
        pares = new List<ParClaveValor<K, V>>();
    }
    
    public void Agregar(K clave, V valor);
    public bool ContieneClave(K clave);
    public V ObtenerValor(K clave);
    public bool Eliminar(K clave);
    public int Contar { get; }
}
```

## 🔍 Conceptos Importantes a Recordar

1. **Los genéricos permiten código reutilizable** para diferentes tipos
2. **Proporcionan type safety** en tiempo de compilación
3. **Evitan el boxing/unboxing** de tipos de valor
4. **Se pueden aplicar a clases, métodos e interfaces**
5. **Las restricciones limitan** qué tipos se pueden usar
6. **Permiten múltiples parámetros de tipo** separados por comas
7. **Se pueden combinar con herencia** y polimorfismo
8. **Son fundamentales** para las colecciones de .NET

## ❓ Preguntas de Repaso

1. ¿Cuál es la diferencia entre genéricos y tipos object?
2. ¿Por qué son importantes las restricciones en genéricos?
3. ¿Cuándo usarías una clase genérica vs. una clase específica?
4. ¿Qué ventajas tienen los genéricos sobre el casting de tipos?
5. ¿Cómo funcionan los genéricos con herencia?

## 🚀 Siguiente Paso

En la próxima clase aprenderemos sobre **Delegados y Eventos en C#**, donde veremos cómo implementar programación orientada a eventos.

---

## 📚 Navegación del Módulo 2

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_herencia.md) | Herencia en C# | |
| [Clase 2](clase_2_polimorfismo.md) | Polimorfismo y Métodos Virtuales | |
| [Clase 3](clase_3_interfaces.md) | Interfaces en C# | |
| [Clase 4](clase_4_clases_abstractas.md) | Clases Abstractas | ← Anterior |
| **Clase 5** | **Genéricos en C#** | ← Estás aquí |
| [Clase 6](clase_6_delegados_eventos.md) | Delegados y Eventos | Siguiente → |
| [Clase 7](clase_7_linq.md) | LINQ en C# | |
| [Clase 8](clase_8_archivos_streams.md) | Manejo de Archivos y Streams | |
| [Clase 9](clase_9_programacion_asincrona.md) | Programación Asíncrona | |
| [Clase 10](clase_10_reflexion_metaprogramacion.md) | Reflexión y Metaprogramación | |

**← [Volver al README del Módulo 2](../junior_2/README.md)**

---

## 📚 Recursos Adicionales

- [Genéricos en C#](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/)
- [Restricciones de tipos genéricos](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/constraints-on-type-parameters)
- [Colecciones genéricas](https://docs.microsoft.com/en-us/dotnet/standard/collections/generic)

---

**¡Excelente! Ahora entiendes los genéricos en C#! 🎯**
