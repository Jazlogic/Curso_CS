# Clase 7: LINQ en C#

## 🎯 Objetivos de la Clase
- Comprender qué es LINQ y su propósito
- Aprender a usar operadores de consulta LINQ
- Entender la sintaxis de método y consulta
- Dominar el uso de LINQ para manipular colecciones

## 📚 Contenido Teórico

### 1. ¿Qué es LINQ?

**LINQ** (Language Integrated Query) es una característica de C# que permite **consultar y manipular datos** de diferentes fuentes (colecciones, bases de datos, XML, etc.) usando una sintaxis unificada y declarativa.

#### Ventajas de LINQ:
- **Sintaxis unificada**: Mismo lenguaje para diferentes fuentes de datos
- **Type safety**: Verificación de tipos en tiempo de compilación
- **IntelliSense**: Mejor soporte del IDE
- **Legibilidad**: Código más expresivo y fácil de entender
- **Reutilización**: Consultas que se pueden reutilizar
- **Optimización**: El compilador optimiza las consultas

### 2. Sintaxis de Método vs. Sintaxis de Consulta

#### 2.1 Comparación de Sintaxis

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

// Clase de ejemplo para demostrar LINQ
public class Estudiante
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public int Edad { get; set; }
    public string Curso { get; set; }
    public decimal Promedio { get; set; }
    public bool Activo { get; set; }
    
    public Estudiante(int id, string nombre, string apellido, int edad, string curso, decimal promedio, bool activo)
    {
        Id = id;
        Nombre = nombre;
        Apellido = apellido;
        Edad = edad;
        Curso = curso;
        Promedio = promedio;
        Activo = activo;
    }
    
    public string NombreCompleto => $"{Nombre} {Apellido}";
    
    public override string ToString()
    {
        return $"ID: {Id}, Nombre: {NombreCompleto}, Edad: {Edad}, Curso: {Curso}, Promedio: {Promedio:F2}, Activo: {Activo}";
    }
}

// Clase que demuestra diferentes sintaxis de LINQ
public class DemostracionLINQ
{
    private List<Estudiante> estudiantes;
    
    public DemostracionLINQ()
    {
        // Inicializar lista de estudiantes
        estudiantes = new List<Estudiante>
        {
            new Estudiante(1, "Juan", "Pérez", 20, "Matemáticas", 8.5m, true),
            new Estudiante(2, "María", "García", 22, "Física", 9.2m, true),
            new Estudiante(3, "Carlos", "López", 19, "Matemáticas", 7.8m, true),
            new Estudiante(4, "Ana", "Martínez", 21, "Química", 8.9m, false),
            new Estudiante(5, "Luis", "Rodríguez", 23, "Física", 9.5m, true),
            new Estudiante(6, "Elena", "Fernández", 20, "Matemáticas", 8.1m, true),
            new Estudiante(7, "Pedro", "González", 22, "Química", 7.5m, true),
            new Estudiante(8, "Sofia", "Hernández", 19, "Física", 9.0m, true),
            new Estudiante(9, "Diego", "Torres", 21, "Matemáticas", 8.7m, false),
            new Estudiante(10, "Laura", "Jiménez", 20, "Química", 8.3m, true)
        };
    }
    
    // Método que demuestra la diferencia entre sintaxis
    public void CompararSintaxis()
    {
        Console.WriteLine("=== COMPARACIÓN DE SINTAXIS LINQ ===");
        
        // Filtrar estudiantes activos con promedio mayor a 8.0
        Console.WriteLine("\n--- Sintaxis de Método ---");
        var resultadoMetodo = estudiantes
            .Where(e => e.Activo && e.Promedio > 8.0m)
            .OrderBy(e => e.Promedio)
            .Select(e => new { e.NombreCompleto, e.Curso, e.Promedio });
        
        foreach (var item in resultadoMetodo)
        {
            Console.WriteLine($"{item.NombreCompleto} - {item.Curso}: {item.Promedio:F2}");
        }
        
        Console.WriteLine("\n--- Sintaxis de Consulta ---");
        var resultadoConsulta = from e in estudiantes
                               where e.Activo && e.Promedio > 8.0m
                               orderby e.Promedio
                               select new { e.NombreCompleto, e.Curso, e.Promedio };
        
        foreach (var item in resultadoConsulta)
        {
            Console.WriteLine($"{item.NombreCompleto} - {item.Curso}: {item.Promedio:F2}");
        }
    }
}
```

### 3. Operadores de Filtrado

#### 3.1 Where, OfType, Take, Skip

```csharp
public class OperadoresFiltrado
{
    private List<Estudiante> estudiantes;
    
    public OperadoresFiltrado()
    {
        estudiantes = new List<Estudiante>
        {
            new Estudiante(1, "Juan", "Pérez", 20, "Matemáticas", 8.5m, true),
            new Estudiante(2, "María", "García", 22, "Física", 9.2m, true),
            new Estudiante(3, "Carlos", "López", 19, "Matemáticas", 7.8m, true),
            new Estudiante(4, "Ana", "Martínez", 21, "Química", 8.9m, false),
            new Estudiante(5, "Luis", "Rodríguez", 23, "Física", 9.5m, true),
            new Estudiante(6, "Elena", "Fernández", 20, "Matemáticas", 8.1m, true),
            new Estudiante(7, "Pedro", "González", 22, "Química", 7.5m, true),
            new Estudiante(8, "Sofia", "Hernández", 19, "Física", 9.0m, true),
            new Estudiante(9, "Diego", "Torres", 21, "Matemáticas", 8.7m, false),
            new Estudiante(10, "Laura", "Jiménez", 20, "Química", 8.3m, true)
        };
    }
    
    // Demostrar operador Where
    public void DemostrarWhere()
    {
        Console.WriteLine("=== OPERADOR WHERE ===");
        
        // Filtrar estudiantes de Matemáticas
        var estudiantesMatematicas = estudiantes.Where(e => e.Curso == "Matemáticas");
        Console.WriteLine("Estudiantes de Matemáticas:");
        foreach (var e in estudiantesMatematicas)
        {
            Console.WriteLine($"- {e.NombreCompleto}: {e.Promedio:F2}");
        }
        
        // Filtrar estudiantes activos con promedio alto
        var estudiantesDestacados = estudiantes.Where(e => e.Activo && e.Promedio >= 9.0m);
        Console.WriteLine("\nEstudiantes destacados (activos con promedio >= 9.0):");
        foreach (var e in estudiantesDestacados)
        {
            Console.WriteLine($"- {e.NombreCompleto} ({e.Curso}): {e.Promedio:F2}");
        }
        
        // Filtrar estudiantes por rango de edad
        var estudiantesJovenes = estudiantes.Where(e => e.Edad >= 19 && e.Edad <= 21);
        Console.WriteLine("\nEstudiantes jóvenes (19-21 años):");
        foreach (var e in estudiantesJovenes)
        {
            Console.WriteLine($"- {e.NombreCompleto}: {e.Edad} años");
        }
    }
    
    // Demostrar operadores Take y Skip
    public void DemostrarTakeSkip()
    {
        Console.WriteLine("\n=== OPERADORES TAKE Y SKIP ===");
        
        // Tomar los primeros 3 estudiantes
        var primeros3 = estudiantes.Take(3);
        Console.WriteLine("Primeros 3 estudiantes:");
        foreach (var e in primeros3)
        {
            Console.WriteLine($"- {e.NombreCompleto}");
        }
        
        // Saltar los primeros 2 y tomar los siguientes 3
        var siguientes3 = estudiantes.Skip(2).Take(3);
        Console.WriteLine("\nSiguientes 3 estudiantes (saltando los primeros 2):");
        foreach (var e in siguientes3)
        {
            Console.WriteLine($"- {e.NombreCompleto}");
        }
        
        // Paginación: página 2 con 3 elementos por página
        var pagina2 = estudiantes.Skip(3).Take(3);
        Console.WriteLine("\nPágina 2 (elementos 4-6):");
        foreach (var e in pagina2)
        {
            Console.WriteLine($"- {e.NombreCompleto}");
        }
    }
    
    // Demostrar operador OfType
    public void DemostrarOfType()
    {
        Console.WriteLine("\n=== OPERADOR OFTYPE ===");
        
        // Crear lista mixta de objetos
        var listaMixta = new List<object>
        {
            "Hola",
            42,
            new Estudiante(1, "Juan", "Pérez", 20, "Matemáticas", 8.5m, true),
            "Mundo",
            3.14,
            new Estudiante(2, "María", "García", 22, "Física", 9.2m, true),
            100
        };
        
        // Extraer solo strings
        var soloStrings = listaMixta.OfType<string>();
        Console.WriteLine("Solo strings:");
        foreach (var item in soloStrings)
        {
            Console.WriteLine($"- {item}");
        }
        
        // Extraer solo estudiantes
        var soloEstudiantes = listaMixta.OfType<Estudiante>();
        Console.WriteLine("\nSolo estudiantes:");
        foreach (var e in soloEstudiantes)
        {
            Console.WriteLine($"- {e.NombreCompleto}");
        }
        
        // Extraer solo números enteros
        var soloEnteros = listaMixta.OfType<int>();
        Console.WriteLine("\nSolo enteros:");
        foreach (var item in soloEnteros)
        {
            Console.WriteLine($"- {item}");
        }
    }
}
```

### 4. Operadores de Ordenamiento

#### 4.1 OrderBy, OrderByDescending, ThenBy, ThenByDescending

```csharp
public class OperadoresOrdenamiento
{
    private List<Estudiante> estudiantes;
    
    public OperadoresOrdenamiento()
    {
        estudiantes = new List<Estudiante>
        {
            new Estudiante(1, "Juan", "Pérez", 20, "Matemáticas", 8.5m, true),
            new Estudiante(2, "María", "García", 22, "Física", 9.2m, true),
            new Estudiante(3, "Carlos", "López", 19, "Matemáticas", 7.8m, true),
            new Estudiante(4, "Ana", "Martínez", 21, "Química", 8.9m, false),
            new Estudiante(5, "Luis", "Rodríguez", 23, "Física", 9.5m, true),
            new Estudiante(6, "Elena", "Fernández", 20, "Matemáticas", 8.1m, true),
            new Estudiante(7, "Pedro", "González", 22, "Química", 7.5m, true),
            new Estudiante(8, "Sofia", "Hernández", 19, "Física", 9.0m, true),
            new Estudiante(9, "Diego", "Torres", 21, "Matemáticas", 8.7m, false),
            new Estudiante(10, "Laura", "Jiménez", 20, "Química", 8.3m, true)
        };
    }
    
    // Demostrar ordenamiento básico
    public void DemostrarOrdenamientoBasico()
    {
        Console.WriteLine("=== ORDENAMIENTO BÁSICO ===");
        
        // Ordenar por nombre ascendente
        var ordenadosPorNombre = estudiantes.OrderBy(e => e.Nombre);
        Console.WriteLine("Ordenados por nombre (ascendente):");
        foreach (var e in ordenadosPorNombre)
        {
            Console.WriteLine($"- {e.NombreCompleto}");
        }
        
        // Ordenar por promedio descendente
        var ordenadosPorPromedio = estudiantes.OrderByDescending(e => e.Promedio);
        Console.WriteLine("\nOrdenados por promedio (descendente):");
        foreach (var e in ordenadosPorPromedio)
        {
            Console.WriteLine($"- {e.NombreCompleto}: {e.Promedio:F2}");
        }
        
        // Ordenar por edad ascendente
        var ordenadosPorEdad = estudiantes.OrderBy(e => e.Edad);
        Console.WriteLine("\nOrdenados por edad (ascendente):");
        foreach (var e in ordenadosPorEdad)
        {
            Console.WriteLine($"- {e.NombreCompleto}: {e.Edad} años");
        }
    }
    
    // Demostrar ordenamiento múltiple
    public void DemostrarOrdenamientoMultiple()
    {
        Console.WriteLine("\n=== ORDENAMIENTO MÚLTIPLE ===");
        
        // Ordenar por curso y luego por promedio descendente
        var ordenadosPorCursoYPromedio = estudiantes
            .OrderBy(e => e.Curso)
            .ThenByDescending(e => e.Promedio);
        
        Console.WriteLine("Ordenados por curso y luego por promedio (descendente):");
        string cursoActual = "";
        foreach (var e in ordenadosPorCursoYPromedio)
        {
            if (e.Curso != cursoActual)
            {
                cursoActual = e.Curso;
                Console.WriteLine($"\n--- {cursoActual} ---");
            }
            Console.WriteLine($"  - {e.NombreCompleto}: {e.Promedio:F2}");
        }
        
        // Ordenar por estado activo y luego por nombre
        var ordenadosPorEstadoYNombre = estudiantes
            .OrderByDescending(e => e.Activo)
            .ThenBy(e => e.Nombre);
        
        Console.WriteLine("\nOrdenados por estado activo (primero) y luego por nombre:");
        foreach (var e in ordenadosPorEstadoYNombre)
        {
            string estado = e.Activo ? "ACTIVO" : "INACTIVO";
            Console.WriteLine($"- [{estado}] {e.NombreCompleto}");
        }
    }
    
    // Demostrar ordenamiento con comparadores personalizados
    public void DemostrarOrdenamientoPersonalizado()
    {
        Console.WriteLine("\n=== ORDENAMIENTO PERSONALIZADO ===");
        
        // Ordenar por longitud del nombre (más corto primero)
        var ordenadosPorLongitudNombre = estudiantes
            .OrderBy(e => e.Nombre.Length)
            .ThenBy(e => e.Nombre);
        
        Console.WriteLine("Ordenados por longitud del nombre (ascendente):");
        foreach (var e in ordenadosPorLongitudNombre)
        {
            Console.WriteLine($"- {e.Nombre} ({e.Nombre.Length} letras)");
        }
        
        // Ordenar por curso y luego por edad descendente
        var ordenadosPorCursoYEdad = estudiantes
            .OrderBy(e => e.Curso)
            .ThenByDescending(e => e.Edad);
        
        Console.WriteLine("\nOrdenados por curso y luego por edad (descendente):");
        cursoActual = "";
        foreach (var e in ordenadosPorCursoYEdad)
        {
            if (e.Curso != cursoActual)
            {
                cursoActual = e.Curso;
                Console.WriteLine($"\n--- {cursoActual} ---");
            }
            Console.WriteLine($"  - {e.NombreCompleto}: {e.Edad} años");
        }
    }
}
```

### 5. Operadores de Proyección

#### 5.1 Select, SelectMany, GroupBy

```csharp
public class OperadoresProyeccion
{
    private List<Estudiante> estudiantes;
    
    public OperadoresProyeccion()
    {
        estudiantes = new List<Estudiante>
        {
            new Estudiante(1, "Juan", "Pérez", 20, "Matemáticas", 8.5m, true),
            new Estudiante(2, "María", "García", 22, "Física", 9.2m, true),
            new Estudiante(3, "Carlos", "López", 19, "Matemáticas", 7.8m, true),
            new Estudiante(4, "Ana", "Martínez", 21, "Química", 8.9m, false),
            new Estudiante(5, "Luis", "Rodríguez", 23, "Física", 9.5m, true),
            new Estudiante(6, "Elena", "Fernández", 20, "Matemáticas", 8.1m, true),
            new Estudiante(7, "Pedro", "González", 22, "Química", 7.5m, true),
            new Estudiante(8, "Sofia", "Hernández", 19, "Física", 9.0m, true),
            new Estudiante(9, "Diego", "Torres", 21, "Matemáticas", 8.7m, false),
            new Estudiante(10, "Laura", "Jiménez", 20, "Química", 8.3m, true)
        };
    }
    
    // Demostrar operador Select
    public void DemostrarSelect()
    {
        Console.WriteLine("=== OPERADOR SELECT ===");
        
        // Proyectar solo nombres
        var nombres = estudiantes.Select(e => e.Nombre);
        Console.WriteLine("Solo nombres:");
        foreach (var nombre in nombres)
        {
            Console.WriteLine($"- {nombre}");
        }
        
        // Proyectar objetos anónimos con información específica
        var resumenEstudiantes = estudiantes.Select(e => new
        {
            e.NombreCompleto,
            e.Curso,
            e.Promedio,
            Estado = e.Activo ? "Activo" : "Inactivo"
        });
        
        Console.WriteLine("\nResumen de estudiantes:");
        foreach (var resumen in resumenEstudiantes)
        {
            Console.WriteLine($"- {resumen.NombreCompleto} ({resumen.Curso}): {resumen.Promedio:F2} - {resumen.Estado}");
        }
        
        // Proyectar con cálculos
        var estudiantesConCalificacion = estudiantes.Select(e => new
        {
            e.NombreCompleto,
            e.Promedio,
            Calificacion = e.Promedio >= 9.0m ? "Excelente" :
                          e.Promedio >= 8.0m ? "Muy Bueno" :
                          e.Promedio >= 7.0m ? "Bueno" : "Regular"
        });
        
        Console.WriteLine("\nEstudiantes con calificación:");
        foreach (var item in estudiantesConCalificacion)
        {
            Console.WriteLine($"- {item.NombreCompleto}: {item.Promedio:F2} ({item.Calificacion})");
        }
    }
    
    // Demostrar operador GroupBy
    public void DemostrarGroupBy()
    {
        Console.WriteLine("\n=== OPERADOR GROUPBY ===");
        
        // Agrupar por curso
        var agrupadosPorCurso = estudiantes.GroupBy(e => e.Curso);
        
        Console.WriteLine("Estudiantes agrupados por curso:");
        foreach (var grupo in agrupadosPorCurso)
        {
            Console.WriteLine($"\n--- {grupo.Key} ({grupo.Count()} estudiantes) ---");
            foreach (var estudiante in grupo)
            {
                Console.WriteLine($"  - {estudiante.NombreCompleto}: {estudiante.Promedio:F2}");
            }
        }
        
        // Agrupar por rango de edad
        var agrupadosPorEdad = estudiantes.GroupBy(e =>
        {
            if (e.Edad < 20) return "19 años o menos";
            if (e.Edad < 22) return "20-21 años";
            return "22 años o más";
        });
        
        Console.WriteLine("\nEstudiantes agrupados por rango de edad:");
        foreach (var grupo in agrupadosPorEdad)
        {
            Console.WriteLine($"\n--- {grupo.Key} ({grupo.Count()} estudiantes) ---");
            foreach (var estudiante in grupo)
            {
                Console.WriteLine($"  - {estudiante.NombreCompleto}: {estudiante.Edad} años");
            }
        }
        
        // Agrupar por curso y calcular estadísticas
        var estadisticasPorCurso = estudiantes.GroupBy(e => e.Curso)
            .Select(g => new
            {
                Curso = g.Key,
                Cantidad = g.Count(),
                PromedioGeneral = g.Average(e => e.Promedio),
                PromedioMaximo = g.Max(e => e.Promedio),
                PromedioMinimo = g.Min(e => e.Promedio),
                EstudiantesActivos = g.Count(e => e.Activo)
            });
        
        Console.WriteLine("\nEstadísticas por curso:");
        foreach (var estadistica in estadisticasPorCurso)
        {
            Console.WriteLine($"\n--- {estadistica.Curso} ---");
            Console.WriteLine($"  Cantidad: {estadistica.Cantidad}");
            Console.WriteLine($"  Promedio general: {estadistica.PromedioGeneral:F2}");
            Console.WriteLine($"  Promedio máximo: {estadistica.PromedioMaximo:F2}");
            Console.WriteLine($"  Promedio mínimo: {estadistica.PromedioMinimo:F2}");
            Console.WriteLine($"  Estudiantes activos: {estadistica.EstudiantesActivos}");
        }
    }
    
    // Demostrar operador SelectMany
    public void DemostrarSelectMany()
    {
        Console.WriteLine("\n=== OPERADOR SELECTMANY ===");
        
        // Crear lista de cursos con múltiples estudiantes
        var cursos = new List<string> { "Matemáticas", "Física", "Química" };
        
        // Obtener todos los estudiantes de cada curso
        var todosLosEstudiantes = cursos.SelectMany(curso =>
            estudiantes.Where(e => e.Curso == curso)
        );
        
        Console.WriteLine("Todos los estudiantes de todos los cursos:");
        foreach (var estudiante in todosLosEstudiantes)
        {
            Console.WriteLine($"- {estudiante.NombreCompleto} ({estudiante.Curso})");
        }
        
        // Crear lista de letras de todos los nombres
        var todasLasLetras = estudiantes.SelectMany(e => e.Nombre.ToCharArray());
        
        Console.WriteLine("\nTodas las letras de todos los nombres:");
        var letrasUnicas = todasLasLetras.Distinct().OrderBy(l => l);
        foreach (var letra in letrasUnicas)
        {
            int cantidad = todasLasLetras.Count(l => l == letra);
            Console.WriteLine($"- '{letra}': {cantidad} veces");
        }
    }
}
```

### 6. Operadores de Agregación

#### 6.1 Count, Sum, Average, Min, Max, Aggregate

```csharp
public class OperadoresAgregacion
{
    private List<Estudiante> estudiantes;
    
    public OperadoresAgregacion()
    {
        estudiantes = new List<Estudiante>
        {
            new Estudiante(1, "Juan", "Pérez", 20, "Matemáticas", 8.5m, true),
            new Estudiante(2, "María", "García", 22, "Física", 9.2m, true),
            new Estudiante(3, "Carlos", "López", 19, "Matemáticas", 7.8m, true),
            new Estudiante(4, "Ana", "Martínez", 21, "Química", 8.9m, false),
            new Estudiante(5, "Luis", "Rodríguez", 23, "Física", 9.5m, true),
            new Estudiante(6, "Elena", "Fernández", 20, "Matemáticas", 8.1m, true),
            new Estudiante(7, "Pedro", "González", 22, "Química", 7.5m, true),
            new Estudiante(8, "Sofia", "Hernández", 19, "Física", 9.0m, true),
            new Estudiante(9, "Diego", "Torres", 21, "Matemáticas", 8.7m, false),
            new Estudiante(10, "Laura", "Jiménez", 20, "Química", 8.3m, true)
        };
    }
    
    // Demostrar operadores básicos de agregación
    public void DemostrarAgregacionBasica()
    {
        Console.WriteLine("=== OPERADORES BÁSICOS DE AGREGACIÓN ===");
        
        // Contar estudiantes
        int totalEstudiantes = estudiantes.Count();
        Console.WriteLine($"Total de estudiantes: {totalEstudiantes}");
        
        // Contar estudiantes activos
        int estudiantesActivos = estudiantes.Count(e => e.Activo);
        Console.WriteLine($"Estudiantes activos: {estudiantesActivos}");
        
        // Contar estudiantes por curso
        var cantidadPorCurso = estudiantes.GroupBy(e => e.Curso)
            .Select(g => new { Curso = g.Key, Cantidad = g.Count() });
        
        Console.WriteLine("\nCantidad de estudiantes por curso:");
        foreach (var item in cantidadPorCurso)
        {
            Console.WriteLine($"- {item.Curso}: {item.Cantidad}");
        }
        
        // Sumar promedios
        decimal sumaPromedios = estudiantes.Sum(e => e.Promedio);
        Console.WriteLine($"\nSuma de todos los promedios: {sumaPromedios:F2}");
        
        // Calcular promedio general
        decimal promedioGeneral = estudiantes.Average(e => e.Promedio);
        Console.WriteLine($"Promedio general: {promedioGeneral:F2}");
        
        // Encontrar promedio máximo y mínimo
        decimal promedioMaximo = estudiantes.Max(e => e.Promedio);
        decimal promedioMinimo = estudiantes.Min(e => e.Promedio);
        Console.WriteLine($"Promedio máximo: {promedioMaximo:F2}");
        Console.WriteLine($"Promedio mínimo: {promedioMinimo:F2}");
    }
    
    // Demostrar operador Aggregate
    public void DemostrarAggregate()
    {
        Console.WriteLine("\n=== OPERADOR AGGREGATE ===");
        
        // Concatenar todos los nombres
        string todosLosNombres = estudiantes
            .Select(e => e.Nombre)
            .Aggregate((actual, siguiente) => actual + ", " + siguiente);
        
        Console.WriteLine($"Todos los nombres: {todosLosNombres}");
        
        // Calcular el producto de todas las edades
        int productoEdades = estudiantes
            .Select(e => e.Edad)
            .Aggregate(1, (actual, siguiente) => actual * siguiente);
        
        Console.WriteLine($"Producto de todas las edades: {productoEdades}");
        
        // Crear un resumen personalizado
        string resumen = estudiantes
            .Aggregate("Resumen de estudiantes:", (actual, siguiente) =>
                actual + $"\n- {siguiente.NombreCompleto} ({siguiente.Curso}): {siguiente.Promedio:F2}");
        
        Console.WriteLine($"\n{resumen}");
        
        // Calcular estadísticas personalizadas
        var estadisticas = estudiantes.Aggregate(
            new { Total = 0, SumaPromedios = 0.0m, SumaEdades = 0 },
            (acc, e) => new
            {
                Total = acc.Total + 1,
                SumaPromedios = acc.SumaPromedios + e.Promedio,
                SumaEdades = acc.SumaEdades + e.Edad
            },
            acc => new
            {
                acc.Total,
                PromedioGeneral = acc.SumaPromedios / acc.Total,
                EdadPromedio = acc.SumaEdades / acc.Total
            });
        
        Console.WriteLine($"\nEstadísticas calculadas con Aggregate:");
        Console.WriteLine($"Total: {estadisticas.Total}");
        Console.WriteLine($"Promedio general: {estadisticas.PromedioGeneral:F2}");
        Console.WriteLine($"Edad promedio: {estadisticas.EdadPromedio:F1}");
    }
    
    // Demostrar operadores de agregación con filtros
    public void DemostrarAgregacionConFiltros()
    {
        Console.WriteLine("\n=== AGREGACIÓN CON FILTROS ===");
        
        // Promedio solo de estudiantes activos
        decimal promedioActivos = estudiantes
            .Where(e => e.Activo)
            .Average(e => e.Promedio);
        
        Console.WriteLine($"Promedio de estudiantes activos: {promedioActivos:F2}");
        
        // Estadísticas por curso
        var estadisticasPorCurso = estudiantes
            .GroupBy(e => e.Curso)
            .Select(g => new
            {
                Curso = g.Key,
                Cantidad = g.Count(),
                Promedio = g.Average(e => e.Promedio),
                PromedioActivos = g.Where(e => e.Activo).Average(e => e.Promedio),
                EdadPromedio = g.Average(e => e.Edad)
            });
        
        Console.WriteLine("\nEstadísticas detalladas por curso:");
        foreach (var estadistica in estadisticasPorCurso)
        {
            Console.WriteLine($"\n--- {estadistica.Curso} ---");
            Console.WriteLine($"  Cantidad total: {estadistica.Cantidad}");
            Console.WriteLine($"  Promedio general: {estadistica.Promedio:F2}");
            Console.WriteLine($"  Promedio de activos: {estadistica.PromedioActivos:F2}");
            Console.WriteLine($"  Edad promedio: {estadistica.EdadPromedio:F1}");
        }
    }
}
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Sistema de Inventario con LINQ
```csharp
// Crear una clase Producto con propiedades: Id, Nombre, Precio, Categoria, Stock
// Usar LINQ para:
// - Filtrar productos por categoría
// - Calcular valor total del inventario
// - Encontrar productos con stock bajo
// - Agrupar productos por categoría
// - Ordenar por precio
```

### Ejercicio 2: Sistema de Empleados con LINQ
```csharp
// Crear una clase Empleado con propiedades: Id, Nombre, Departamento, Salario, FechaContratacion
// Usar LINQ para:
// - Calcular salario promedio por departamento
// - Encontrar empleados con mayor antigüedad
// - Agrupar por departamento y calcular estadísticas
// - Filtrar empleados por rango salarial
```

### Ejercicio 3: Sistema de Ventas con LINQ
```csharp
// Crear clases: Venta, Cliente, Producto
// Usar LINQ para:
// - Calcular total de ventas por cliente
// - Encontrar productos más vendidos
// - Analizar ventas por período
// - Generar reportes de rendimiento
```

## 🔍 Conceptos Importantes a Recordar

1. **LINQ proporciona sintaxis unificada** para consultar diferentes fuentes de datos
2. **La sintaxis de método y consulta** son equivalentes en funcionalidad
3. **Los operadores de filtrado** permiten seleccionar elementos específicos
4. **Los operadores de ordenamiento** organizan los resultados
5. **Los operadores de proyección** transforman los datos
6. **Los operadores de agregación** calculan valores resumidos
7. **LINQ es lazy evaluation** (no se ejecuta hasta que se itera)
8. **Se puede combinar múltiples operadores** en una sola consulta

## ❓ Preguntas de Repaso

1. ¿Cuál es la diferencia entre sintaxis de método y sintaxis de consulta?
2. ¿Por qué LINQ es útil para manipular colecciones?
3. ¿Cuándo usarías GroupBy vs. Select?
4. ¿Qué ventajas tiene la evaluación lazy de LINQ?
5. ¿Cómo optimizas consultas LINQ complejas?

## 🚀 Siguiente Paso

En la próxima clase aprenderemos sobre **Manejo de Archivos y Streams en C#**, donde veremos cómo trabajar con archivos del sistema.

---

## 📚 Navegación del Módulo 2

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_herencia.md) | Herencia en C# | |
| [Clase 2](clase_2_polimorfismo.md) | Polimorfismo y Métodos Virtuales | |
| [Clase 3](clase_3_interfaces.md) | Interfaces en C# | |
| [Clase 4](clase_4_clases_abstractas.md) | Clases Abstractas | |
| [Clase 5](clase_5_genericos.md) | Genéricos en C# | |
| [Clase 6](clase_6_delegados_eventos.md) | Delegados y Eventos | ← Anterior |
| **Clase 7** | **LINQ en C#** | ← Estás aquí |
| [Clase 8](clase_8_archivos_streams.md) | Manejo de Archivos y Streams | Siguiente → |
| [Clase 9](clase_9_programacion_asincrona.md) | Programación Asíncrona | |
| [Clase 10](clase_10_reflexion_metaprogramacion.md) | Reflexión y Metaprogramación | |

**← [Volver al README del Módulo 2](../junior_2/README.md)**

---

## 📚 Recursos Adicionales

- [LINQ en C#](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/)
- [Operadores de consulta estándar](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/query-syntax-and-method-syntax-in-linq)
- [LINQ to Objects](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/linq-to-objects)

---

**¡Excelente! Ahora entiendes LINQ en C#! 🎯**
