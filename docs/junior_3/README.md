# 🎯 Junior Level 3: Arrays, Listas y Colecciones Básicas

## 🧭 Navegación del Curso

**← Anterior**: [Junior Level 2: Estructuras de Control](../junior_2/README.md)  
**Siguiente →**: [Mid Level 1: Programación Orientada a Objetos](../midLevel_1/README.md)

---

## 📚 Descripción

En este nivel aprenderás a trabajar con colecciones de datos: arrays, listas y otras estructuras que te permitirán manejar múltiples valores de manera eficiente.

## 🎯 Objetivos de Aprendizaje

- Crear y manipular arrays unidimensionales y multidimensionales
- Trabajar con List<T> y sus métodos principales
- Entender las diferencias entre arrays y listas
- Usar colecciones genéricas básicas
- Implementar algoritmos de búsqueda y ordenamiento
- Crear programas que manejen grandes cantidades de datos

## 📖 Contenido Teórico

### 1. Arrays (Arreglos)

#### Arrays Unidimensionales
```csharp
// Declaración e inicialización
int[] numeros = new int[5]; // Array de 5 elementos
int[] numeros2 = { 1, 2, 3, 4, 5 }; // Inicialización directa

// Acceso a elementos
numeros[0] = 10; // Primer elemento
int primerNumero = numeros[0];

// Propiedades
int longitud = numeros.Length; // Número de elementos
```

#### Arrays Multidimensionales
```csharp
// Array bidimensional (matriz)
int[,] matriz = new int[3, 4]; // 3 filas, 4 columnas
int[,] matriz2 = { { 1, 2, 3 }, { 4, 5, 6 } };

// Array tridimensional
int[,,] cubo = new int[2, 3, 4]; // 2x3x4

// Arrays de arrays (jagged arrays)
int[][] jaggedArray = new int[3][];
jaggedArray[0] = new int[4];
jaggedArray[1] = new int[2];
jaggedArray[2] = new int[3];
```

#### Métodos Útiles de Arrays
```csharp
int[] numeros = { 3, 1, 4, 1, 5, 9, 2, 6 };

// Ordenar
Array.Sort(numeros);

// Revertir
Array.Reverse(numeros);

// Buscar elemento
int indice = Array.IndexOf(numeros, 5);

// Copiar array
int[] copia = new int[numeros.Length];
Array.Copy(numeros, copia, numeros.Length);

// Limpiar array
Array.Clear(numeros, 0, numeros.Length);
```

### 2. List<T> - Listas Genéricas

#### Creación y Manipulación
```csharp
// Crear lista
List<string> nombres = new List<string>();
List<int> numeros = new List<int> { 1, 2, 3, 4, 5 };

// Agregar elementos
nombres.Add("Juan");
nombres.AddRange(new string[] { "María", "Pedro" });

// Insertar en posición específica
nombres.Insert(1, "Ana");

// Eliminar elementos
nombres.Remove("Juan");
nombres.RemoveAt(0);
nombres.RemoveRange(0, 2);

// Limpiar lista
nombres.Clear();
```

#### Propiedades y Métodos Principales
```csharp
List<int> numeros = new List<int> { 1, 2, 3, 4, 5 };

// Propiedades
int cantidad = numeros.Count; // Número de elementos
int capacidad = numeros.Capacity; // Capacidad interna

// Búsqueda
bool contiene = numeros.Contains(3);
int indice = numeros.IndexOf(3);
int ultimoIndice = numeros.LastIndexOf(3);

// Ordenamiento
numeros.Sort(); // Orden ascendente
numeros.Reverse(); // Invertir orden

// Conversión
int[] arrayNumeros = numeros.ToArray();
```

### 3. Otras Colecciones Básicas

#### Dictionary<TKey, TValue>
```csharp
// Diccionario clave-valor
Dictionary<string, int> edades = new Dictionary<string, int>();

// Agregar elementos
edades.Add("Juan", 25);
edades["María"] = 30; // Sintaxis alternativa

// Verificar si existe clave
if (edades.ContainsKey("Juan"))
{
    int edad = edades["Juan"];
}

// Obtener valor de forma segura
if (edades.TryGetValue("Pedro", out int edadPedro))
{
    Console.WriteLine($"Edad de Pedro: {edadPedro}");
}

// Iterar sobre diccionario
foreach (var kvp in edades)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
}
```

#### HashSet<T>
```csharp
// Conjunto de elementos únicos
HashSet<int> numerosUnicos = new HashSet<int>();

// Agregar elementos
numerosUnicos.Add(1);
numerosUnicos.Add(2);
numerosUnicos.Add(1); // No se agrega, ya existe

// Operaciones de conjunto
HashSet<int> conjunto1 = new HashSet<int> { 1, 2, 3 };
HashSet<int> conjunto2 = new HashSet<int> { 3, 4, 5 };

// Unión
conjunto1.UnionWith(conjunto2);

// Intersección
conjunto1.IntersectWith(conjunto2);

// Diferencia
conjunto1.ExceptWith(conjunto2);
```

#### Queue<T> y Stack<T>
```csharp
// Cola (FIFO - First In, First Out)
Queue<string> cola = new Queue<string>();
cola.Enqueue("Primero");
cola.Enqueue("Segundo");
string primero = cola.Dequeue(); // Obtiene y elimina el primero

// Pila (LIFO - Last In, First Out)
Stack<string> pila = new Stack<string>();
pila.Push("Primero");
pila.Push("Segundo");
string ultimo = pila.Pop(); // Obtiene y elimina el último
```

### 4. Algoritmos Básicos

#### Búsqueda Lineal
```csharp
public static int BusquedaLineal(int[] array, int valor)
{
    for (int i = 0; i < array.Length; i++)
    {
        if (array[i] == valor)
            return i;
    }
    return -1; // No encontrado
}
```

#### Búsqueda Binaria (Array debe estar ordenado)
```csharp
public static int BusquedaBinaria(int[] array, int valor)
{
    int izquierda = 0;
    int derecha = array.Length - 1;
    
    while (izquierda <= derecha)
    {
        int medio = (izquierda + derecha) / 2;
        
        if (array[medio] == valor)
            return medio;
        
        if (array[medio] < valor)
            izquierda = medio + 1;
        else
            derecha = medio - 1;
    }
    
    return -1; // No encontrado
}
```

#### Ordenamiento Burbuja
```csharp
public static void OrdenamientoBurbuja(int[] array)
{
    for (int i = 0; i < array.Length - 1; i++)
    {
        for (int j = 0; j < array.Length - 1 - i; j++)
        {
            if (array[j] > array[j + 1])
            {
                // Intercambiar elementos
                int temp = array[j];
                array[j] = array[j + 1];
                array[j + 1] = temp;
            }
        }
    }
}
```

### 5. Iteración y LINQ Básico

#### Iteración Tradicional
```csharp
// Con for
for (int i = 0; i < numeros.Count; i++)
{
    Console.WriteLine(numeros[i]);
}

// Con foreach
foreach (int numero in numeros)
{
    Console.WriteLine(numero);
}

// Con forEach (método de List)
numeros.ForEach(numero => Console.WriteLine(numero));
```

#### LINQ Básico
```csharp
List<int> numeros = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

// Filtrar números pares
var pares = numeros.Where(n => n % 2 == 0);

// Obtener números mayores que 5
var mayoresQue5 = numeros.Where(n => n > 5);

// Contar elementos
int cantidad = numeros.Count();

// Obtener el máximo y mínimo
int maximo = numeros.Max();
int minimo = numeros.Min();
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Array de Números
Crea un array de 10 números enteros, calcula su suma, promedio, máximo y mínimo.

### Ejercicio 2: Matriz de Multiplicación
Crea una matriz 10x10 que contenga las tablas de multiplicar del 1 al 10.

### Ejercicio 3: Lista de Nombres
Crea una lista de nombres, permite agregar, eliminar y buscar nombres.

### Ejercicio 4: Diccionario de Contactos
Implementa una agenda de contactos usando Dictionary con nombre y teléfono.

### Ejercicio 5: Búsqueda en Array
Implementa búsqueda lineal y binaria en un array ordenado. Compara el rendimiento.

### Ejercicio 6: Ordenamiento Personalizado
Implementa ordenamiento burbuja y compara con Array.Sort().

### Ejercicio 7: Conjuntos Únicos
Crea dos HashSet con números y demuestra las operaciones de conjunto.

### Ejercicio 8: Cola de Impresión
Simula una cola de impresión usando Queue<T>.

### Ejercicio 9: Pila de Operaciones
Implementa una calculadora usando Stack para evaluar expresiones postfix.

### Ejercicio 10: Proyecto Integrador - Sistema de Inventario
Crea un sistema de inventario que incluya:
- Lista de productos con código, nombre, precio y stock
- Funcionalidad para agregar, eliminar y buscar productos
- Cálculo de valor total del inventario
- Reporte de productos con stock bajo
- Ordenamiento por diferentes criterios

## 📝 Quiz de Autoevaluación

1. ¿Cuál es la diferencia entre un array y una List<T>?
2. ¿Qué es un jagged array?
3. ¿Cuándo usarías HashSet<T> en lugar de List<T>?
4. ¿Qué significa FIFO y LIFO?
5. ¿Por qué es importante que un array esté ordenado para la búsqueda binaria?

## 🚀 Siguiente Nivel

Una vez que hayas completado todos los ejercicios y comprendas los conceptos, estarás listo para el **Mid Level 1: Programación Orientada a Objetos**.

## 💡 Consejos de Estudio

- Practica creando diferentes tipos de arrays y listas
- Experimenta con los métodos de las colecciones
- Implementa los algoritmos de búsqueda y ordenamiento desde cero
- Usa el debugger para entender cómo funcionan las colecciones internamente
- Crea proyectos que requieran manejar múltiples datos

¡Estás construyendo una base sólida para la programación orientada a objetos! 🚀
