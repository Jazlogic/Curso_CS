# 🚀 Mid Level 3: Manejo de Excepciones y Generics

## 🧭 Navegación del Curso

**← Anterior**: [Mid Level 2: Herencia y Polimorfismo](../midLevel_2/README.md)  
**Siguiente →**: [Mid Level 4: LINQ y Expresiones Lambda](../midLevel_4/README.md)

---

## 📋 Contenido del Nivel

### 🎯 Objetivos de Aprendizaje
- Comprender el sistema de manejo de excepciones en C#
- Dominar el uso de generics para crear código reutilizable
- Implementar manejo robusto de errores en aplicaciones
- Crear colecciones y métodos genéricos personalizados

### ⏱️ Tiempo Estimado
- **Teoría**: 2-3 horas
- **Ejercicios**: 4-6 horas
- **Proyecto Integrador**: 2-3 horas
- **Total**: 8-12 horas

---

## 📚 Contenido Teórico

### 1. Manejo de Excepciones

#### 1.1 ¿Qué son las Excepciones?
Las excepciones son eventos que ocurren durante la ejecución de un programa que interrumpen el flujo normal de instrucciones.

```csharp
try
{
    // Código que puede generar una excepción
    int result = 10 / 0;
}
catch (DivideByZeroException ex)
{
    // Manejo de la excepción específica
    Console.WriteLine($"Error: {ex.Message}");
}
catch (Exception ex)
{
    // Captura cualquier otra excepción
    Console.WriteLine($"Error general: {ex.Message}");
}
finally
{
    // Código que siempre se ejecuta
    Console.WriteLine("Limpieza de recursos");
}
```

#### 1.2 Tipos de Excepciones
- **SystemException**: Excepciones del sistema
- **ApplicationException**: Excepciones de la aplicación
- **ArgumentException**: Argumentos inválidos
- **NullReferenceException**: Referencia nula
- **IndexOutOfRangeException**: Índice fuera de rango

#### 1.3 Crear Excepciones Personalizadas

```csharp
public class CustomException : Exception
{
    public CustomException() : base() { }
    
    public CustomException(string message) : base(message) { }
    
    public CustomException(string message, Exception innerException) 
        : base(message, innerException) { }
}

// Uso
throw new CustomException("Este es un error personalizado");
```

#### 1.4 Using Statement
Para manejo automático de recursos:

```csharp
using (var stream = new FileStream("archivo.txt", FileMode.Open))
{
    // El stream se cierra automáticamente
    byte[] buffer = new byte[1024];
    stream.Read(buffer, 0, buffer.Length);
}
```

### 2. Generics

#### 2.1 ¿Qué son los Generics?
Los generics permiten crear clases, interfaces y métodos que trabajan con tipos de datos no especificados.

#### 2.2 Clases Genéricas

```csharp
public class Container<T>
{
    private T item;
    
    public T Item
    {
        get { return item; }
        set { item = value; }
    }
    
    public Container(T item)
    {
        this.item = item;
    }
}

// Uso
var stringContainer = new Container<string>("Hola");
var intContainer = new Container<int>(42);
```

#### 2.3 Métodos Genéricos

```csharp
public class Utilities
{
    public static void Swap<T>(ref T a, ref T b)
    {
        T temp = a;
        a = b;
        b = temp;
    }
    
    public static bool AreEqual<T>(T a, T b) where T : IEquatable<T>
    {
        return a.Equals(b);
    }
}

// Uso
int x = 5, y = 10;
Utilities.Swap(ref x, ref y);
```

#### 2.4 Restricciones de Generics

```csharp
// Restricción de tipo base
public class Repository<T> where T : class
{
    public void Save(T entity) { }
}

// Restricción de interfaz
public class Sorter<T> where T : IComparable<T>
{
    public void Sort(T[] items) { }
}

// Restricción de constructor
public class Factory<T> where T : new()
{
    public T Create() => new T();
}

// Restricción de tipo de referencia
public class Cache<T> where T : class
{
    private T[] items;
}

// Restricción de tipo de valor
public class Calculator<T> where T : struct
{
    public T Add(T a, T b) { }
}
```

#### 2.5 Colecciones Genéricas Personalizadas

```csharp
public class GenericStack<T>
{
    private List<T> items = new List<T>();
    
    public void Push(T item)
    {
        items.Add(item);
    }
    
    public T Pop()
    {
        if (items.Count == 0)
            throw new InvalidOperationException("Stack está vacío");
            
        T item = items[items.Count - 1];
        items.RemoveAt(items.Count - 1);
        return item;
    }
    
    public T Peek()
    {
        if (items.Count == 0)
            throw new InvalidOperationException("Stack está vacío");
            
        return items[items.Count - 1];
    }
    
    public int Count => items.Count;
    public bool IsEmpty => items.Count == 0;
}
```

#### 2.6 Interfaces Genéricas

```csharp
public interface IRepository<T> where T : class
{
    T GetById(int id);
    IEnumerable<T> GetAll();
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
}

public class UserRepository : IRepository<User>
{
    private List<User> users = new List<User>();
    
    public User GetById(int id)
    {
        return users.FirstOrDefault(u => u.Id == id);
    }
    
    public IEnumerable<User> GetAll()
    {
        return users;
    }
    
    public void Add(User entity)
    {
        users.Add(entity);
    }
    
    public void Update(User entity)
    {
        var existingUser = users.FirstOrDefault(u => u.Id == entity.Id);
        if (existingUser != null)
        {
            var index = users.IndexOf(existingUser);
            users[index] = entity;
        }
    }
    
    public void Delete(User entity)
    {
        users.Remove(entity);
    }
}
```

---

## 🎯 Ejercicios Prácticos

### Ejercicio 1: Calculadora con Manejo de Excepciones
Crea una calculadora que maneje excepciones para división por cero y entradas inválidas.

```csharp
public class SafeCalculator
{
    public double Divide(double a, double b)
    {
        if (b == 0)
            throw new DivideByZeroException("No se puede dividir por cero");
            
        return a / b;
    }
    
    public int ParseNumber(string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("La entrada no puede estar vacía");
            
        if (!int.TryParse(input, out int result))
            throw new FormatException("La entrada debe ser un número válido");
            
        return result;
    }
}
```

### Ejercicio 2: Stack Genérico Personalizado
Implementa un stack genérico con métodos Push, Pop, Peek y validaciones.

### Ejercicio 3: Cache Genérico con TTL
Crea un sistema de cache genérico que expire elementos después de un tiempo determinado.

### Ejercicio 4: Validador Genérico
Implementa un validador genérico que pueda validar diferentes tipos de objetos.

### Ejercicio 5: Logger Genérico
Crea un sistema de logging genérico que pueda manejar diferentes tipos de mensajes.

### Ejercicio 6: Repository Genérico con Filtros
Implementa un repository genérico que permita filtrar por diferentes criterios.

### Ejercicio 7: Serializador Genérico
Crea un serializador genérico que pueda convertir objetos a JSON y viceversa.

### Ejercicio 8: Comparador Genérico
Implementa un comparador genérico que pueda ordenar diferentes tipos de objetos.

### Ejercicio 9: Factory Genérico
Crea un factory genérico que pueda instanciar diferentes tipos de objetos.

### Ejercicio 10: Observer Genérico
Implementa un patrón observer genérico que pueda notificar a diferentes tipos de observadores.

---

## 🚀 Proyecto Integrador: Sistema de Gestión de Biblioteca

### Descripción
Crea un sistema de gestión de biblioteca usando generics y manejo robusto de excepciones.

### Requisitos
- Clases genéricas para diferentes tipos de recursos (libros, DVDs, revistas)
- Repository genérico para operaciones CRUD
- Manejo de excepciones para casos como recursos no encontrados
- Sistema de préstamos con validaciones
- Cache genérico para mejorar el rendimiento

### Estructura Sugerida
```
Library/
├── Models/
│   ├── Book.cs
│   ├── DVD.cs
│   └── Magazine.cs
├── Repositories/
│   ├── IRepository.cs
│   └── GenericRepository.cs
├── Services/
│   ├── LibraryService.cs
│   └── CacheService.cs
├── Exceptions/
│   ├── ResourceNotFoundException.cs
│   └── InvalidOperationException.cs
└── Program.cs
```

---

## 📝 Autoevaluación

### Preguntas Teóricas
1. ¿Cuál es la diferencia entre `catch` y `finally`?
2. ¿Qué son las restricciones de generics y cuándo se usan?
3. ¿Cómo se crea una excepción personalizada?
4. ¿Qué ventajas ofrecen los generics sobre el uso de `object`?
5. ¿Cuándo usarías `using` statement?

### Preguntas Prácticas
1. Implementa un método genérico que encuentre el elemento máximo en una colección
2. Crea una excepción personalizada para validación de email
3. Implementa una cola genérica con prioridad
4. Crea un validador genérico que use reflection

---

## 🔗 Enlaces de Referencia

- [Microsoft Docs - Exception Handling](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/exceptions/)
- [Microsoft Docs - Generics](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/types/generics)
- [C# Generics Tutorial](https://www.tutorialsteacher.com/csharp/csharp-generics)
- [Exception Handling Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)

---

## 📚 Siguiente Nivel

**Progreso**: 6 de 12 niveles completados

**Siguiente**: [Mid Level 4: LINQ y Expresiones Lambda](../midLevel_4/README.md)

**Anterior**: [Mid Level 2: Herencia, Polimorfismo e Interfaces](../midLevel_2/README.md)

---

## 🎉 ¡Felicidades!

Has completado el nivel intermedio de manejo de excepciones y generics. Ahora puedes:
- Manejar errores de manera robusta en tus aplicaciones
- Crear código reutilizable y type-safe con generics
- Implementar patrones de diseño más avanzados
- Construir aplicaciones más robustas y mantenibles

¡Continúa con el siguiente nivel para dominar LINQ y expresiones lambda!
