# 🚀 Mid Level 4: LINQ y Expresiones Lambda

## 📋 Contenido del Nivel

### 🎯 Objetivos de Aprendizaje
- Dominar LINQ (Language Integrated Query) para consultas de datos
- Comprender y usar expresiones lambda de manera efectiva
- Implementar consultas complejas con métodos de extensión
- Crear consultas personalizadas y optimizadas

### ⏱️ Tiempo Estimado
- **Teoría**: 3-4 horas
- **Ejercicios**: 5-7 horas
- **Proyecto Integrador**: 3-4 horas
- **Total**: 11-15 horas

---

## 📚 Contenido Teórico

### 1. Expresiones Lambda

#### 1.1 ¿Qué son las Expresiones Lambda?
Las expresiones lambda son funciones anónimas que pueden contener expresiones y declaraciones, y se pueden usar para crear delegados o árboles de expresión.

#### 1.2 Sintaxis Básica

```csharp
// Sintaxis básica
Func<int, int> square = x => x * x;

// Múltiples parámetros
Func<int, int, int> add = (x, y) => x + y;

// Sin parámetros
Func<string> getMessage = () => "Hola Mundo";

// Múltiples líneas
Func<int, int> factorial = n =>
{
    if (n <= 1) return 1;
    return n * factorial(n - 1);
};

// Uso
Console.WriteLine(square(5));        // 25
Console.WriteLine(add(3, 4));        // 7
Console.WriteLine(getMessage());     // Hola Mundo
Console.WriteLine(factorial(5));     // 120
```

#### 1.3 Delegados y Func

```csharp
// Delegado personalizado
public delegate int MathOperation(int x, int y);

// Usando delegado
MathOperation multiply = (x, y) => x * y;
Console.WriteLine(multiply(4, 5)); // 20

// Func predefinido
Func<int, int, int> divide = (x, y) => y != 0 ? x / y : 0;
Console.WriteLine(divide(10, 2)); // 5

// Action (sin retorno)
Action<string> printMessage = message => Console.WriteLine(message);
printMessage("Mensaje desde lambda");
```

#### 1.4 Predicate

```csharp
// Predicate<T> - retorna bool
Predicate<int> isEven = x => x % 2 == 0;
Predicate<string> isLong = s => s.Length > 10;

// Uso
Console.WriteLine(isEven(4));    // True
Console.WriteLine(isEven(7));    // False
Console.WriteLine(isLong("Hola")); // False
Console.WriteLine(isLong("Este es un mensaje muy largo")); // True
```

### 2. LINQ (Language Integrated Query)

#### 2.1 ¿Qué es LINQ?
LINQ es un conjunto de características que extiende capacidades de consulta potentes al lenguaje C# y proporciona una manera consistente de trabajar con datos de diferentes fuentes.

#### 2.2 Sintaxis de Consulta vs Sintaxis de Método

```csharp
var numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

// Sintaxis de consulta (Query Syntax)
var evenNumbersQuery = from n in numbers
                       where n % 2 == 0
                       select n;

// Sintaxis de método (Method Syntax)
var evenNumbersMethod = numbers.Where(n => n % 2 == 0);

// Ambas producen el mismo resultado
foreach (var num in evenNumbersQuery)
{
    Console.WriteLine(num); // 2, 4, 6, 8, 10
}
```

#### 2.3 Operadores de Filtrado

```csharp
var students = new List<Student>
{
    new Student { Id = 1, Name = "Ana", Age = 20, Grade = 85 },
    new Student { Id = 2, Name = "Carlos", Age = 22, Grade = 92 },
    new Student { Id = 3, Name = "María", Age = 19, Grade = 78 },
    new Student { Id = 4, Name = "Juan", Age = 21, Grade = 95 },
    new Student { Id = 5, Name = "Laura", Age = 20, Grade = 88 }
};

// Where - Filtrado
var highGrades = students.Where(s => s.Grade >= 90);
var youngStudents = students.Where(s => s.Age < 21);

// OfType - Filtrar por tipo
var objects = new List<object> { 1, "Hola", 2.5, "Mundo", 3 };
var strings = objects.OfType<string>(); // "Hola", "Mundo"
```

#### 2.4 Operadores de Ordenamiento

```csharp
// OrderBy - Ordenamiento ascendente
var orderedByName = students.OrderBy(s => s.Name);
var orderedByGrade = students.OrderBy(s => s.Grade);

// OrderByDescending - Ordenamiento descendente
var topStudents = students.OrderByDescending(s => s.Grade);

// ThenBy - Ordenamiento secundario
var orderedByAgeThenGrade = students
    .OrderBy(s => s.Age)
    .ThenByDescending(s => s.Grade);

// Reverse - Invertir orden
var reversedStudents = students.Reverse();
```

#### 2.5 Operadores de Proyección

```csharp
// Select - Proyección simple
var studentNames = students.Select(s => s.Name);
var studentGrades = students.Select(s => s.Grade);

// Select con objeto anónimo
var studentInfo = students.Select(s => new
{
    s.Name,
    s.Age,
    Status = s.Grade >= 90 ? "Excelente" : "Bueno"
});

// SelectMany - Aplanar colecciones
var courses = new List<Course>
{
    new Course { Name = "Matemáticas", Students = new List<string> { "Ana", "Carlos" } },
    new Course { Name = "Física", Students = new List<string> { "María", "Juan" } }
};

var allStudents = courses.SelectMany(c => c.Students);
```

#### 2.6 Operadores de Agrupación

```csharp
// GroupBy - Agrupar por criterio
var studentsByAge = students.GroupBy(s => s.Age);

foreach (var group in studentsByAge)
{
    Console.WriteLine($"Edad: {group.Key}");
    foreach (var student in group)
    {
        Console.WriteLine($"  - {student.Name}: {student.Grade}");
    }
}

// Agrupación con proyección
var gradeGroups = students.GroupBy(s => s.Grade / 10, s => s.Name);
```

#### 2.7 Operadores de Agregación

```csharp
// Count - Contar elementos
var totalStudents = students.Count();
var excellentStudents = students.Count(s => s.Grade >= 90);

// Sum, Average, Min, Max
var totalGrade = students.Sum(s => s.Grade);
var averageGrade = students.Average(s => s.Grade);
var highestGrade = students.Max(s => s.Grade);
var lowestGrade = students.Min(s => s.Grade);

// Aggregate - Agregación personalizada
var gradeRange = students.Aggregate(
    new { Min = int.MaxValue, Max = int.MinValue },
    (acc, student) => new
    {
        Min = Math.Min(acc.Min, student.Grade),
        Max = Math.Max(acc.Max, student.Grade)
    }
);
```

#### 2.8 Operadores de Particionamiento

```csharp
// Take - Tomar primeros elementos
var top3Students = students.OrderByDescending(s => s.Grade).Take(3);

// Skip - Saltar elementos
var remainingStudents = students.Skip(2);

// TakeWhile - Tomar mientras se cumpla condición
var studentsUntil90 = students.TakeWhile(s => s.Grade < 90);

// SkipWhile - Saltar mientras se cumpla condición
var studentsAfter90 = students.SkipWhile(s => s.Grade < 90);
```

#### 2.9 Operadores de Conjunto

```csharp
var list1 = new List<int> { 1, 2, 3, 4, 5 };
var list2 = new List<int> { 4, 5, 6, 7, 8 };

// Union - Unión de conjuntos
var union = list1.Union(list2); // 1, 2, 3, 4, 5, 6, 7, 8

// Intersect - Intersección
var intersection = list1.Intersect(list2); // 4, 5

// Except - Diferencia
var difference = list1.Except(list2); // 1, 2, 3

// Distinct - Elementos únicos
var uniqueNumbers = new List<int> { 1, 2, 2, 3, 3, 4 }.Distinct();
```

#### 2.10 Operadores de Elemento

```csharp
// First, FirstOrDefault
var firstStudent = students.First();
var firstHighGrade = students.FirstOrDefault(s => s.Grade >= 90);

// Last, LastOrDefault
var lastStudent = students.Last();
var lastYoungStudent = students.LastOrDefault(s => s.Age < 21);

// Single, SingleOrDefault
var studentWithId3 = students.Single(s => s.Id == 3);
var studentWithGrade95 = students.SingleOrDefault(s => s.Grade == 95);

// ElementAt, ElementAtOrDefault
var thirdStudent = students.ElementAt(2);
var studentAt10 = students.ElementAtOrDefault(10); // null si no existe
```

### 3. LINQ con Diferentes Fuentes de Datos

#### 3.1 LINQ to Objects

```csharp
// Colecciones en memoria
var numbers = Enumerable.Range(1, 100);
var squares = numbers.Select(n => n * n);
var evenSquares = squares.Where(n => n % 2 == 0);
```

#### 3.2 LINQ to XML

```csharp
var xmlString = @"
<students>
    <student id='1' name='Ana' grade='85' />
    <student id='2' name='Carlos' grade='92' />
</students>";

var doc = XDocument.Parse(xmlString);
var studentNames = doc.Descendants("student")
                     .Select(s => s.Attribute("name").Value);
```

#### 3.3 LINQ to SQL (Entity Framework)

```csharp
// Ejemplo conceptual - en la práctica usarías DbContext
var highGradeStudents = context.Students
    .Where(s => s.Grade >= 90)
    .OrderByDescending(s => s.Grade)
    .Select(s => new { s.Name, s.Grade })
    .ToList();
```

---

## 🎯 Ejercicios Prácticos

### Ejercicio 1: Filtrado y Ordenamiento de Productos
Crea una lista de productos y usa LINQ para filtrar por categoría, ordenar por precio y mostrar información específica.

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public int Stock { get; set; }
}

var products = new List<Product>
{
    new Product { Id = 1, Name = "Laptop", Price = 999.99m, Category = "Electronics", Stock = 10 },
    new Product { Id = 2, Name = "Mouse", Price = 25.50m, Category = "Electronics", Stock = 50 },
    new Product { Id = 3, Name = "Book", Price = 15.99m, Category = "Books", Stock = 100 }
};

// Encontrar productos electrónicos con stock > 20, ordenados por precio
var availableElectronics = products
    .Where(p => p.Category == "Electronics" && p.Stock > 20)
    .OrderBy(p => p.Price)
    .Select(p => new { p.Name, p.Price, p.Stock });
```

### Ejercicio 2: Análisis de Ventas
Implementa un sistema de análisis de ventas usando LINQ para calcular estadísticas.

### Ejercicio 3: Búsqueda de Empleados
Crea un sistema de búsqueda de empleados con múltiples criterios usando LINQ.

### Ejercicio 4: Gestión de Inventario
Implementa consultas LINQ para gestionar inventario con alertas de stock bajo.

### Ejercicio 5: Sistema de Calificaciones
Crea un sistema de calificaciones que use LINQ para calcular promedios y estadísticas.

### Ejercicio 6: Filtros Dinámicos
Implementa un sistema de filtros dinámicos que permita al usuario construir consultas LINQ.

### Ejercicio 7: Agrupación Avanzada
Crea consultas LINQ que agrupen datos por múltiples criterios y calculen métricas.

### Ejercicio 8: Paginación con LINQ
Implementa un sistema de paginación usando operadores Skip y Take.

### Ejercicio 9: Consultas de Texto
Crea un sistema de búsqueda de texto que use LINQ para filtrar y ordenar resultados.

### Ejercicio 10: Optimización de Consultas
Implementa técnicas de optimización para consultas LINQ complejas.

---

## 🚀 Proyecto Integrador: Sistema de Gestión de Biblioteca con LINQ

### Descripción
Crea un sistema de gestión de biblioteca que use LINQ extensivamente para consultas y reportes.

### Requisitos
- Consultas LINQ para búsqueda de libros por múltiples criterios
- Sistema de préstamos con consultas de estado
- Reportes estadísticos usando operadores de agregación
- Búsqueda de texto con filtros avanzados
- Sistema de recomendaciones basado en patrones de préstamo

### Estructura Sugerida
```
LibraryLINQ/
├── Models/
│   ├── Book.cs
│   ├── Author.cs
│   ├── Category.cs
│   ├── Loan.cs
│   └── User.cs
├── Services/
│   ├── BookService.cs
│   ├── LoanService.cs
│   └── ReportService.cs
├── Queries/
│   ├── BookQueries.cs
│   ├── LoanQueries.cs
│   └── ReportQueries.cs
└── Program.cs
```

### Ejemplo de Consultas LINQ

```csharp
public class BookQueries
{
    public IEnumerable<Book> SearchBooks(string searchTerm, string category = null)
    {
        var query = books.AsQueryable();
        
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(b => 
                b.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                b.Author.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }
        
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(b => b.Category.Name == category);
        }
        
        return query.OrderBy(b => b.Title).ToList();
    }
    
    public IEnumerable<Book> GetPopularBooks(int days = 30)
    {
        var cutoffDate = DateTime.Now.AddDays(-days);
        
        return loans.Where(l => l.LoanDate >= cutoffDate)
                   .GroupBy(l => l.Book)
                   .OrderByDescending(g => g.Count())
                   .Take(10)
                   .Select(g => g.Key);
    }
}
```

---

## 📝 Autoevaluación

### Preguntas Teóricas
1. ¿Cuál es la diferencia entre sintaxis de consulta y sintaxis de método en LINQ?
2. ¿Qué son las expresiones lambda y cuándo se usan?
3. ¿Cuál es la diferencia entre `First()` y `FirstOrDefault()`?
4. ¿Cómo funciona el operador `GroupBy` en LINQ?
5. ¿Qué ventajas ofrece LINQ sobre consultas tradicionales?

### Preguntas Prácticas
1. Implementa una consulta LINQ que encuentre el segundo elemento más grande en una colección
2. Crea una consulta que agrupe productos por categoría y calcule el precio promedio
3. Implementa paginación usando operadores LINQ
4. Crea una consulta que use `SelectMany` para aplanar una colección anidada

---

## 🔗 Enlaces de Referencia

- [Microsoft Docs - LINQ](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/)
- [Microsoft Docs - Lambda Expressions](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-expressions)
- [101 LINQ Samples](https://github.com/dotnet/try-samples/tree/main/101-linq-samples)
- [LINQ Performance](https://docs.microsoft.com/en-us/dotnet/standard/linq/performance)

---

## 📚 Siguiente Nivel

**Progreso**: 7 de 12 niveles completados

**Siguiente**: [Senior Level 1: Patrones de Diseño y SOLID](../senior_1/README.md)

**Anterior**: [Mid Level 3: Manejo de Excepciones y Generics](../midLevel_3/README.md)

---

## 🎉 ¡Felicidades!

Has completado el nivel intermedio de LINQ y expresiones lambda. Ahora puedes:
- Escribir consultas complejas y eficientes usando LINQ
- Usar expresiones lambda para crear código más legible
- Implementar filtros, ordenamiento y agrupación avanzados
- Crear consultas personalizadas para diferentes fuentes de datos

¡Continúa con el siguiente nivel para dominar patrones de diseño y principios SOLID!
