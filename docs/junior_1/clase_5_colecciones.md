# Clase 5: Colecciones Básicas en C#

## 🎯 Objetivos de la Clase
- Comprender qué son los arrays y cómo usarlos
- Aprender a trabajar con listas genéricas (List<T>)
- Entender el concepto de genéricos en C#
- Dominar las operaciones básicas con colecciones

---

## 📚 Navegación del Módulo 1

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_introduccion.md) | Introducción a C# y .NET | |
| [Clase 2](clase_2_variables_tipos.md) | Variables y Tipos de Datos | |
| [Clase 3](clase_3_operadores.md) | Operadores y Expresiones | |
| [Clase 4](clase_4_estructuras_control.md) | Estructuras de Control | ← Anterior |
| **Clase 5** | **Colecciones** | ← Estás aquí |
| [Clase 6](clase_6_strings.md) | Manipulación de Strings | Siguiente → |
| [Clase 7](clase_7_funciones.md) | Métodos y Funciones | |
| [Clase 8](clase_8_namespaces.md) | Namespaces y Organización | |
| [Clase 9](clase_9_manejo_errores.md) | Manejo de Errores | |
| [Clase 10](clase_10_poo_basica.md) | Programación Orientada a Objetos Básica | |

**← [Volver al README del Módulo 1](../junior_1/README.md)**

## 📚 Contenido Teórico

### 1. ¿Qué son las Colecciones?

Las colecciones son **estructuras de datos** que permiten almacenar múltiples elementos del mismo tipo en una sola variable. Son fundamentales para organizar y manipular datos en nuestros programas.

#### Tipos de Colecciones:
- **Arrays**: Colecciones de tamaño fijo
- **Listas**: Colecciones de tamaño dinámico
- **Colecciones Genéricas**: Colecciones tipadas que garantizan la seguridad de tipos

### 2. Arrays (Arreglos)

Los arrays son colecciones de **tamaño fijo** que almacenan elementos del mismo tipo.

#### 2.1 Declaración y Inicialización de Arrays

```csharp
// Declaración de array sin inicializar
int[] numeros;                     // Declara un array de enteros (sin elementos)

// Inicialización con valores específicos
int[] numerosInicializados = { 1, 2, 3, 4, 5 }; // Array con 5 elementos

// Declaración e inicialización en una línea
string[] nombres = { "Juan", "María", "Carlos", "Ana" }; // Array de strings

// Declaración con tamaño específico
double[] precios = new double[4];  // Array de 4 elementos double (inicializados en 0.0)

// Declaración con tamaño y valores iniciales
char[] letras = new char[3] { 'A', 'B', 'C' }; // Array de 3 caracteres

Console.WriteLine($"Array números: [{string.Join(", ", numerosInicializados)}]");
Console.WriteLine($"Array nombres: [{string.Join(", ", nombres)}]");
Console.WriteLine($"Array precios: [{string.Join(", ", precios)}]");
Console.WriteLine($"Array letras: [{string.Join(", ", letras)}]");
```

#### Explicación Línea por Línea:

**Línea 2: `int[] numeros;`**
- `int[]` declara un array de enteros
- `numeros` es el nombre de la variable
- No tiene elementos inicialmente

**Línea 5: `int[] numerosInicializados = { 1, 2, 3, 4, 5 };`**
- `int[]` declara un array de enteros
- `{ 1, 2, 3, 4, 5 }` inicializa el array con 5 valores
- El tamaño se determina automáticamente por el número de elementos

**Línea 8: `string[] nombres = { "Juan", "María", "Carlos", "Ana" };`**
- `string[]` declara un array de strings
- Inicializado con 4 nombres

**Línea 11: `double[] precios = new double[4];`**
- `new double[4]` crea un array de 4 elementos double
- Todos los elementos se inicializan en `0.0` (valor por defecto)

**Línea 14: `char[] letras = new char[3] { 'A', 'B', 'C' };`**
- `new char[3]` especifica el tamaño
- `{ 'A', 'B', 'C' }` proporciona los valores iniciales

#### 2.2 Acceso a Elementos del Array

Los elementos de un array se acceden mediante **índices** que comienzan en 0:

```csharp
int[] numeros = { 10, 20, 30, 40, 50 }; // Array con 5 elementos

// Acceso a elementos individuales
int primerElemento = numeros[0];          // Primer elemento (índice 0)
int segundoElemento = numeros[1];         // Segundo elemento (índice 1)
int ultimoElemento = numeros[4];          // Último elemento (índice 4)

// Cambio de valores
numeros[2] = 35;                          // Cambia el tercer elemento de 30 a 35

Console.WriteLine($"Primer elemento: {primerElemento}");      // Salida: 10
Console.WriteLine($"Segundo elemento: {segundoElemento}");    // Salida: 20
Console.WriteLine($"Tercer elemento (modificado): {numeros[2]}"); // Salida: 35
Console.WriteLine($"Último elemento: {ultimoElemento}");      // Salida: 50

// Acceso a todos los elementos
Console.WriteLine("Todos los elementos:");
for (int i = 0; i < numeros.Length; i++)
{
    Console.WriteLine($"Elemento {i}: {numeros[i]}");
}
```

#### Explicación del Acceso a Elementos:

**Línea 1: `int[] numeros = { 10, 20, 30, 40, 50 };`**
- Crea un array con 5 elementos
- Los índices van de 0 a 4

**Línea 4: `int primerElemento = numeros[0];`**
- `numeros[0]` accede al elemento en la posición 0
- `primerElemento = 10`

**Línea 5: `int segundoElemento = numeros[1];`**
- `numeros[1]` accede al elemento en la posición 1
- `segundoElemento = 20`

**Línea 6: `int ultimoElemento = numeros[4];`**
- `numeros[4]` accede al elemento en la posición 4
- `ultimoElemento = 50`

**Línea 9: `numeros[2] = 35;`**
- Cambia el valor del elemento en la posición 2
- El array ahora es: `{ 10, 20, 35, 40, 50 }`

**Línea 18: `numeros.Length`**
- `Length` es una propiedad que retorna el número de elementos del array
- En este caso: `numeros.Length = 5`

#### 2.3 Propiedades y Métodos del Array

```csharp
int[] numeros = { 15, 8, 23, 7, 42, 19, 3 };

Console.WriteLine($"Tamaño del array: {numeros.Length}");
Console.WriteLine($"Primer elemento: {numeros[0]}");
Console.WriteLine($"Último elemento: {numeros[numeros.Length - 1]}");

// Búsqueda de elementos
int indiceDelOcho = Array.IndexOf(numeros, 8);        // Busca el valor 8
bool contieneVeintitres = Array.Exists(numeros, x => x == 23); // Verifica si existe 23

Console.WriteLine($"El número 8 está en la posición: {indiceDelOcho}");
Console.WriteLine($"¿Contiene el número 23? {contieneVeintitres}");

// Ordenamiento
Array.Sort(numeros);                                   // Ordena el array de menor a mayor
Console.WriteLine($"Array ordenado: [{string.Join(", ", numeros)}]");

// Reversión
Array.Reverse(numeros);                                // Invierte el orden del array
Console.WriteLine($"Array invertido: [{string.Join(", ", numeros)}]");
```

#### Explicación de Propiedades y Métodos:

**Línea 1: `int[] numeros = { 15, 8, 23, 7, 42, 19, 3 };`**
- Array desordenado con 7 elementos

**Línea 3: `numeros.Length`**
- Propiedad que retorna el número de elementos (7)

**Línea 5: `numeros[numeros.Length - 1]`**
- `numeros.Length - 1 = 7 - 1 = 6`
- `numeros[6]` accede al último elemento (3)

**Línea 8: `Array.IndexOf(numeros, 8)`**
- Método estático que busca el valor 8 en el array
- Retorna el índice donde se encuentra (1)
- Si no se encuentra, retorna -1

**Línea 9: `Array.Exists(numeros, x => x == 23)`**
- Método que verifica si existe algún elemento que cumpla la condición
- `x => x == 23` es una expresión lambda que verifica si x es igual a 23
- Retorna `true` porque 23 existe en el array

**Línea 13: `Array.Sort(numeros)`**
- Ordena el array de menor a mayor
- El array original se modifica: `{ 3, 7, 8, 15, 19, 23, 42 }`

**Línea 16: `Array.Reverse(numeros)`**
- Invierte el orden del array
- El array ordenado se invierte: `{ 42, 23, 19, 15, 8, 7, 3 }`

#### 2.4 Arrays Multidimensionales

Los arrays pueden tener múltiples dimensiones:

```csharp
// Array bidimensional (matriz)
int[,] matriz = new int[3, 4];    // Matriz de 3 filas y 4 columnas

// Inicialización de matriz
int[,] matrizInicializada = {
    { 1, 2, 3, 4 },               // Primera fila
    { 5, 6, 7, 8 },               // Segunda fila
    { 9, 10, 11, 12 }             // Tercera fila
};

Console.WriteLine("Matriz inicializada:");
for (int fila = 0; fila < matrizInicializada.GetLength(0); fila++)
{
    for (int columna = 0; columna < matrizInicializada.GetLength(1); columna++)
    {
        Console.Write($"{matrizInicializada[fila, columna],3} "); // Formato de 3 espacios
    }
    Console.WriteLine(); // Nueva línea después de cada fila
}

// Acceso a elementos específicos
int elemento = matrizInicializada[1, 2];  // Fila 1, Columna 2
Console.WriteLine($"\nElemento en [1,2]: {elemento}"); // Salida: 7
```

#### Explicación de Arrays Multidimensionales:

**Línea 2: `int[,] matriz = new int[3, 4];`**
- `int[,]` declara un array bidimensional de enteros
- `new int[3, 4]` crea una matriz de 3 filas y 4 columnas
- Todos los elementos se inicializan en 0

**Línea 5-9: Inicialización de matriz**
- Cada fila se define entre llaves `{}`
- Las filas se separan por comas
- Cada fila debe tener el mismo número de columnas

**Línea 12: `matrizInicializada.GetLength(0)`**
- `GetLength(0)` retorna el tamaño de la primera dimensión (filas = 3)

**Línea 13: `matrizInicializada.GetLength(1)`**
- `GetLength(1)` retorna el tamaño de la segunda dimensión (columnas = 4)

**Línea 16: `matrizInicializada[fila, columna]`**
- Accede al elemento en la posición [fila, columna]
- `[0,0] = 1`, `[0,1] = 2`, `[1,2] = 7`, etc.

### 3. Listas Genéricas (List<T>)

Las listas son colecciones de **tamaño dinámico** que pueden crecer o reducirse según sea necesario.

#### 3.1 Declaración y Uso de Listas

```csharp
// Declaración de lista vacía
List<string> nombres = new List<string>();

// Agregar elementos a la lista
nombres.Add("Juan");               // Agrega "Juan" al final de la lista
nombres.Add("María");              // Agrega "María" al final de la lista
nombres.Add("Carlos");             // Agrega "Carlos" al final de la lista

// Inicialización con elementos
List<int> numeros = new List<int> { 10, 20, 30, 40, 50 };

Console.WriteLine($"Lista nombres: [{string.Join(", ", nombres)}]");
Console.WriteLine($"Lista números: [{string.Join(", ", numeros)}]");
Console.WriteLine($"Tamaño de nombres: {nombres.Count}");
Console.WriteLine($"Tamaño de números: {numeros.Count}");
```

#### Explicación de Listas:

**Línea 2: `List<string> nombres = new List<string>();`**
- `List<string>` declara una lista de strings
- `new List<string>()` crea una lista vacía
- `<string>` es el tipo genérico que especifica qué tipo de elementos puede contener

**Línea 5: `nombres.Add("Juan");`**
- `Add()` agrega un elemento al final de la lista
- La lista ahora contiene: `["Juan"]`

**Línea 6: `nombres.Add("María");`**
- Agrega "María" al final
- La lista ahora contiene: `["Juan", "María"]`

**Línea 7: `nombres.Add("Carlos");`**
- Agrega "Carlos" al final
- La lista ahora contiene: `["Juan", "María", "Carlos"]`

**Línea 10: `List<int> numeros = new List<int> { 10, 20, 30, 40, 50 };`**
- Crea una lista de enteros inicializada con 5 valores
- Equivalente a crear una lista vacía y agregar cada elemento

**Línea 15: `nombres.Count`**
- `Count` es la propiedad que retorna el número de elementos en la lista
- Similar a `Length` en arrays

#### 3.2 Métodos de Lista

```csharp
List<string> frutas = new List<string> { "Manzana", "Banana", "Naranja" };

Console.WriteLine($"Lista original: [{string.Join(", ", frutas)}]");

// Agregar elementos
frutas.Add("Uva");                 // Agrega al final
frutas.Insert(1, "Pera");         // Inserta "Pera" en la posición 1

Console.WriteLine($"Después de agregar: [{string.Join(", ", frutas)}]");

// Eliminar elementos
frutas.Remove("Banana");           // Elimina la primera ocurrencia de "Banana"
frutas.RemoveAt(0);               // Elimina el elemento en la posición 0

Console.WriteLine($"Después de eliminar: [{string.Join(", ", frutas)}]");

// Búsqueda
bool contieneNaranja = frutas.Contains("Naranja");    // Verifica si contiene "Naranja"
int indicePera = frutas.IndexOf("Pera");              // Busca el índice de "Pera"

Console.WriteLine($"¿Contiene Naranja? {contieneNaranja}");
Console.WriteLine($"Índice de Pera: {indicePera}");

// Ordenamiento
frutas.Sort();                     // Ordena alfabéticamente
Console.WriteLine($"Ordenada: [{string.Join(", ", frutas)}]");

// Limpiar lista
frutas.Clear();                    // Elimina todos los elementos
Console.WriteLine($"Después de limpiar: [{string.Join(", ", frutas)}]");
Console.WriteLine($"Tamaño después de limpiar: {frutas.Count}");
```

#### Explicación de Métodos de Lista:

**Línea 1: `List<string> frutas = new List<string> { "Manzana", "Banana", "Naranja" };`**
- Lista inicial con 3 frutas

**Línea 6: `frutas.Add("Uva");`**
- Agrega "Uva" al final de la lista
- Lista: `["Manzana", "Banana", "Naranja", "Uva"]`

**Línea 7: `frutas.Insert(1, "Pera");`**
- Inserta "Pera" en la posición 1
- Lista: `["Manzana", "Pera", "Banana", "Naranja", "Uva"]`

**Línea 12: `frutas.Remove("Banana");`**
- Elimina la primera ocurrencia de "Banana"
- Lista: `["Manzana", "Pera", "Naranja", "Uva"]`

**Línea 13: `frutas.RemoveAt(0);`**
- Elimina el elemento en la posición 0 ("Manzana")
- Lista: `["Pera", "Naranja", "Uva"]`

**Línea 17: `frutas.Contains("Naranja")`**
- Verifica si la lista contiene "Naranja"
- Retorna `true`

**Línea 18: `frutas.IndexOf("Pera")`**
- Busca el índice de "Pera"
- Retorna `0` (primera posición)

**Línea 22: `frutas.Sort()`**
- Ordena la lista alfabéticamente
- Lista: `["Naranja", "Pera", "Uva"]`

**Línea 26: `frutas.Clear()`**
- Elimina todos los elementos de la lista
- Lista: `[]` (vacía)

#### 3.3 Iteración sobre Listas

```csharp
List<int> numeros = new List<int> { 15, 8, 23, 7, 42, 19, 3 };

Console.WriteLine("Iteración con for:");
for (int i = 0; i < numeros.Count; i++)
{
    Console.WriteLine($"Elemento {i}: {numeros[i]}");
}

Console.WriteLine("\nIteración con foreach:");
foreach (int numero in numeros)
{
    Console.WriteLine($"Número: {numero}");
}

Console.WriteLine("\nIteración con foreach y filtro:");
foreach (int numero in numeros)
{
    if (numero > 20)               // Solo números mayores a 20
    {
        Console.WriteLine($"Número grande: {numero}");
    }
}
```

#### Explicación de Iteración:

**Iteración con for:**
- `for (int i = 0; i < numeros.Count; i++)`
- Usa un índice `i` para acceder a cada elemento
- Útil cuando necesitas conocer la posición del elemento

**Iteración con foreach:**
- `foreach (int numero in numeros)`
- Más simple y legible
- No necesitas manejar índices
- No puedes modificar la lista durante la iteración

**Iteración con filtro:**
- Combina `foreach` con una condición `if`
- Solo procesa elementos que cumplan la condición

### 4. Concepto de Genéricos

Los genéricos permiten crear clases, métodos y estructuras que trabajan con diferentes tipos de datos de manera segura.

#### 4.1 Ventajas de los Genéricos

```csharp
// Sin genéricos (problemático)
object[] objetos = new object[3];
objetos[0] = "Texto";             // String
objetos[1] = 42;                  // Int
objetos[2] = true;                // Bool

// Al acceder, necesitas casting y puede fallar
string texto = (string)objetos[0]; // OK
int numero = (int)objetos[1];      // OK
// int numero2 = (int)objetos[0]; // ❌ Error en tiempo de ejecución!

// Con genéricos (seguro)
List<string> strings = new List<string>();
strings.Add("Hola");              // Solo strings
strings.Add("Mundo");             // Solo strings
// strings.Add(42);               // ❌ Error en tiempo de compilación!

List<int> enteros = new List<int>();
enteros.Add(10);                  // Solo enteros
enteros.Add(20);                  // Solo enteros
// enteros.Add("Texto");          // ❌ Error en tiempo de compilación!

Console.WriteLine("Lista de strings:");
foreach (string s in strings)
{
    Console.WriteLine(s);          // No necesita casting
}

Console.WriteLine("Lista de enteros:");
foreach (int n in enteros)
{
    Console.WriteLine(n);          // No necesita casting
}
```

#### Explicación de Genéricos:

**Problema sin genéricos:**
- `object[]` puede contener cualquier tipo
- Necesitas casting al acceder a los elementos
- Los errores se detectan en tiempo de ejecución

**Solución con genéricos:**
- `List<string>` solo puede contener strings
- `List<int>` solo puede contener enteros
- No necesitas casting
- Los errores se detectan en tiempo de compilación

### 5. Otras Colecciones Básicas

#### 5.1 Dictionary<TKey, TValue>

Los diccionarios almacenan pares clave-valor:

```csharp
// Crear diccionario de string a int
Dictionary<string, int> edades = new Dictionary<string, int>();

// Agregar elementos
edades.Add("Juan", 25);
edades.Add("María", 30);
edades.Add("Carlos", 28);

// Acceso a elementos
Console.WriteLine($"Edad de Juan: {edades["Juan"]}");
Console.WriteLine($"Edad de María: {edades["María"]}");

// Verificar si existe una clave
if (edades.ContainsKey("Ana"))
{
    Console.WriteLine($"Edad de Ana: {edades["Ana"]}");
}
else
{
    Console.WriteLine("Ana no está en el diccionario");
}

// Iteración sobre diccionario
Console.WriteLine("\nTodas las edades:");
foreach (KeyValuePair<string, int> par in edades)
{
    Console.WriteLine($"{par.Key}: {par.Value} años");
}
```

#### 5.2 HashSet<T>

Los HashSet almacenan elementos únicos sin orden específico:

```csharp
HashSet<int> numerosUnicos = new HashSet<int>();

// Agregar elementos
numerosUnicos.Add(1);
numerosUnicos.Add(2);
numerosUnicos.Add(3);
numerosUnicos.Add(2);             // No se agrega porque ya existe

Console.WriteLine($"Elementos únicos: [{string.Join(", ", numerosUnicos)}]");
Console.WriteLine($"Tamaño: {numerosUnicos.Count}");

// Verificar si contiene un elemento
bool contieneDos = numerosUnicos.Contains(2);
Console.WriteLine($"¿Contiene 2? {contieneDos}");
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Array de Números
```csharp
int[] numeros = { 12, 5, 8, 19, 3, 25, 7, 14 };

Console.WriteLine("Array original:");
for (int i = 0; i < numeros.Length; i++)
{
    Console.WriteLine($"Posición {i}: {numeros[i]}");
}

// Encontrar el máximo y mínimo
int maximo = numeros[0];
int minimo = numeros[0];

for (int i = 1; i < numeros.Length; i++)
{
    if (numeros[i] > maximo) maximo = numeros[i];
    if (numeros[i] < minimo) minimo = numeros[i];
}

Console.WriteLine($"\nMáximo: {maximo}");
Console.WriteLine($"Mínimo: {minimo}");
```

### Ejercicio 2: Lista de Nombres
```csharp
List<string> nombres = new List<string>();

// Agregar nombres
nombres.Add("Ana");
nombres.Add("Juan");
nombres.Add("María");
nombres.Add("Carlos");
nombres.Add("Elena");

Console.WriteLine("Lista de nombres:");
foreach (string nombre in nombres)
{
    Console.WriteLine($"- {nombre}");
}

// Buscar nombres que empiecen con 'A'
Console.WriteLine("\nNombres que empiezan con 'A':");
foreach (string nombre in nombres)
{
    if (nombre.StartsWith("A"))
    {
        Console.WriteLine($"- {nombre}");
    }
}
```

### Ejercicio 3: Matriz de Multiplicación
```csharp
int[,] tablaMultiplicar = new int[10, 10];

// Llenar la matriz
for (int i = 0; i < 10; i++)
{
    for (int j = 0; j < 10; j++)
    {
        tablaMultiplicar[i, j] = (i + 1) * (j + 1);
    }
}

// Mostrar la matriz
Console.WriteLine("Tabla de multiplicar del 1 al 10:");
for (int i = 0; i < 10; i++)
{
    for (int j = 0; j < 10; j++)
    {
        Console.Write($"{tablaMultiplicar[i, j],3} ");
    }
    Console.WriteLine();
}
```

### Ejercicio 4: Diccionario de Productos
```csharp
Dictionary<string, decimal> productos = new Dictionary<string, decimal>();

// Agregar productos
productos.Add("Laptop", 999.99m);
productos.Add("Mouse", 25.50m);
productos.Add("Teclado", 45.75m);
productos.Add("Monitor", 299.99m);

Console.WriteLine("Catálogo de productos:");
foreach (var producto in productos)
{
    Console.WriteLine($"{producto.Key}: ${producto.Value:F2}");
}

// Buscar producto específico
string productoBuscado = "Laptop";
if (productos.ContainsKey(productoBuscado))
{
    Console.WriteLine($"\nEl precio de {productoBuscado} es: ${productos[productoBuscado]:F2}");
}
else
{
    Console.WriteLine($"\nEl producto {productoBuscado} no está disponible");
}
```

## 🔍 Conceptos Importantes a Recordar

1. **Los arrays** tienen tamaño fijo y se accede por índices
2. **Las listas** tienen tamaño dinámico y métodos útiles
3. **Los genéricos** garantizan la seguridad de tipos en tiempo de compilación
4. **Length** es para arrays, **Count** es para listas
5. **Los índices** comienzan en 0, no en 1
6. **foreach** es ideal para iterar sobre colecciones
7. **Dictionary** almacena pares clave-valor
8. **HashSet** almacena elementos únicos
9. **Las matrices** son arrays multidimensionales

## ❓ Preguntas de Repaso

1. ¿Cuál es la diferencia entre un array y una lista?
2. ¿Por qué son importantes los genéricos en C#?
3. ¿Cómo accedes al primer elemento de un array?
4. ¿Qué método usas para agregar elementos a una lista?
5. ¿Cuándo usarías un Dictionary en lugar de una List?

## 🚀 Siguiente Paso

En la próxima clase aprenderemos sobre **Strings y Texto**, donde veremos cómo manipular y trabajar con texto en C#.

---

## 📚 Recursos Adicionales

- [Colecciones en C#](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/collections/)
- [Arrays en C#](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/arrays/)
- [List<T> en C#](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1)

---

**¡Excelente! Ahora dominas las colecciones básicas en C#! 🎯**
