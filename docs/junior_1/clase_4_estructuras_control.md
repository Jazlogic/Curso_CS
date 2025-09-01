# Clase 4: Estructuras de Control en C#

## 🎯 Objetivos de la Clase
- Comprender las estructuras condicionales (if, else, switch)
- Aprender a usar bucles (for, while, do-while)
- Entender el control de flujo con break, continue y return
- Dominar las estructuras anidadas y su uso

## 📚 Contenido Teórico

### 1. ¿Qué son las Estructuras de Control?

Las estructuras de control son **instrucciones** que permiten controlar el flujo de ejecución de un programa. Determinan qué código se ejecuta, cuándo se ejecuta y cuántas veces se ejecuta.

#### Tipos de Estructuras de Control:
- **Condicionales**: Ejecutan código solo si se cumple una condición
- **Bucles**: Repiten código múltiples veces
- **Control de Flujo**: Modifican el comportamiento normal de ejecución

### 2. Estructuras Condicionales

#### 2.1 Declaración if Simple

La declaración `if` ejecuta un bloque de código solo si una condición es verdadera:

```csharp
int edad = 18;                    // Variable edad con valor 18

if (edad >= 18)                   // Condición: ¿es mayor o igual a 18?
{
    Console.WriteLine("Eres mayor de edad");  // Código que se ejecuta si la condición es true
}

Console.WriteLine("Esta línea siempre se ejecuta"); // Código fuera del if
```

#### Explicación Línea por Línea:

**Línea 1: `int edad = 18;`**
- Declara una variable `edad` con valor 18

**Línea 3: `if (edad >= 18)`**
- `if` es la palabra clave que inicia la estructura condicional
- `(edad >= 18)` es la condición que se evalúa
- `edad >= 18` evalúa a `true` (18 >= 18 es verdadero)
- Los paréntesis son obligatorios alrededor de la condición

**Línea 4: `{`**
- Llave de apertura que indica el inicio del bloque de código del if
- Todo lo que esté entre `{` y `}` se ejecuta solo si la condición es `true`

**Línea 5: `Console.WriteLine("Eres mayor de edad");`**
- Código que se ejecuta porque la condición es `true`
- Imprime "Eres mayor de edad" en la consola

**Línea 6: `}`**
- Llave de cierre que indica el fin del bloque de código del if

**Línea 8: `Console.WriteLine("Esta línea siempre se ejecuta");`**
- Código que está fuera del if, por lo que siempre se ejecuta
- No depende de la condición del if

#### 2.2 Declaración if-else

La declaración `if-else` ejecuta un bloque de código si la condición es verdadera, y otro bloque si es falsa:

```csharp
int edad = 16;                    // Variable edad con valor 16

if (edad >= 18)                   // Condición: ¿es mayor o igual a 18?
{
    Console.WriteLine("Eres mayor de edad");      // Código si la condición es true
}
else                               // Si la condición es false
{
    Console.WriteLine("Eres menor de edad");      // Código si la condición es false
}

Console.WriteLine("Programa terminado");           // Código que siempre se ejecuta
```

#### Explicación Línea por Línea:

**Línea 1: `int edad = 16;`**
- Declara variable `edad` con valor 16

**Línea 3: `if (edad >= 18)`**
- Evalúa si `edad >= 18`
- `16 >= 18` evalúa a `false`
- Como la condición es `false`, se salta el bloque del if

**Línea 4-6: Bloque del if**
- Este código NO se ejecuta porque la condición es `false`
- Se salta completamente

**Línea 7: `else`**
- `else` indica qué hacer cuando la condición del if es `false`
- Como la condición fue `false`, se ejecuta este bloque

**Línea 8-10: Bloque del else**
- Este código SÍ se ejecuta porque la condición del if fue `false`
- Imprime "Eres menor de edad"

**Línea 12: `Console.WriteLine("Programa terminado");`**
- Código que siempre se ejecuta, independientemente del resultado del if-else

#### 2.3 Declaración if-else if-else

La declaración `if-else if-else` permite evaluar múltiples condiciones en secuencia:

```csharp
int puntuacion = 85;              // Variable puntuación con valor 85

if (puntuacion >= 90)             // Primera condición: ¿es mayor o igual a 90?
{
    Console.WriteLine("Calificación: A (Excelente)");
}
else if (puntuacion >= 80)        // Segunda condición: ¿es mayor o igual a 80?
{
    Console.WriteLine("Calificación: B (Bueno)");
}
else if (puntuacion >= 70)        // Tercera condición: ¿es mayor o igual a 70?
{
    Console.WriteLine("Calificación: C (Satisfactorio)");
}
else if (puntuacion >= 60)        // Cuarta condición: ¿es mayor o igual a 60?
{
    Console.WriteLine("Calificación: D (Deficiente)");
}
else                               // Si ninguna condición anterior es true
{
    Console.WriteLine("Calificación: F (Reprobado)");
}

Console.WriteLine("Evaluación completada");
```

#### Explicación del Flujo:

**Evaluación de Condiciones:**
1. **Primera condición**: `puntuacion >= 90` → `85 >= 90` → `false`
2. **Segunda condición**: `puntuacion >= 80` → `85 >= 80` → `true` ✅
3. **Se ejecuta el bloque**: `Console.WriteLine("Calificación: B (Bueno)");`
4. **Se saltan las demás condiciones**: No se evalúan las condiciones restantes
5. **Se ejecuta el código final**: `Console.WriteLine("Evaluación completada");`

#### 2.4 Declaración switch

La declaración `switch` es útil cuando tienes múltiples opciones basadas en el valor de una variable:

```csharp
int diaSemana = 3;                // Variable que representa el día de la semana

switch (diaSemana)                 // Evalúa el valor de diaSemana
{
    case 1:                        // Si diaSemana es igual a 1
        Console.WriteLine("Lunes");
        break;                     // Termina la ejecución del switch
        
    case 2:                        // Si diaSemana es igual a 2
        Console.WriteLine("Martes");
        break;                     // Termina la ejecución del switch
        
    case 3:                        // Si diaSemana es igual a 3
        Console.WriteLine("Miércoles");
        break;                     // Termina la ejecución del switch
        
    case 4:                        // Si diaSemana es igual a 4
        Console.WriteLine("Jueves");
        break;                     // Termina la ejecución del switch
        
    case 5:                        // Si diaSemana es igual a 5
        Console.WriteLine("Viernes");
        break;                     // Termina la ejecución del switch
        
    case 6:                        // Si diaSemana es igual a 6
        Console.WriteLine("Sábado");
        break;                     // Termina la ejecución del switch
        
    case 7:                        // Si diaSemana es igual a 7
        Console.WriteLine("Domingo");
        break;                     // Termina la ejecución del switch
        
    default:                       // Si diaSemana no coincide con ningún case
        Console.WriteLine("Día inválido");
        break;                     // Termina la ejecución del switch
}

Console.WriteLine("Switch completado");
```

#### Explicación del Switch:

**Línea 1: `int diaSemana = 3;`**
- Declara variable `diaSemana` con valor 3

**Línea 3: `switch (diaSemana)`**
- `switch` evalúa el valor de `diaSemana`
- Se compara con cada `case`

**Línea 5: `case 3:`**
- Como `diaSemana = 3`, coincide con este case
- Se ejecuta `Console.WriteLine("Miércoles");`

**Línea 7: `break;`**
- `break` termina la ejecución del switch
- Sin `break`, el código continuaría ejecutándose en los siguientes cases

**Línea 25: `default:`**
- Se ejecuta solo si ningún `case` coincide
- Es opcional pero recomendado para manejar casos inesperados

#### 2.5 Switch con Expresiones (C# 8.0+)

En versiones modernas de C#, el switch puede ser más expresivo:

```csharp
int puntuacion = 85;              // Variable puntuación

string calificacion = puntuacion switch
{
    >= 90 => "A (Excelente)",      // Si puntuacion >= 90
    >= 80 => "B (Bueno)",          // Si puntuacion >= 80
    >= 70 => "C (Satisfactorio)",  // Si puntuacion >= 70
    >= 60 => "D (Deficiente)",     // Si puntuacion >= 60
    _ => "F (Reprobado)"           // Para cualquier otro valor (_ significa "cualquier cosa")
};

Console.WriteLine($"Calificación: {calificacion}");
```

### 3. Bucles (Loops)

Los bucles permiten ejecutar un bloque de código múltiples veces.

#### 3.1 Bucle for

El bucle `for` se usa cuando conoces el número exacto de iteraciones:

```csharp
Console.WriteLine("Contando del 1 al 5:");

for (int i = 1; i <= 5; i++)      // Bucle for con 3 partes
{
    Console.WriteLine($"Número: {i}");
}

Console.WriteLine("Bucle for completado");
```

#### Explicación del Bucle for:

**Sintaxis: `for (inicialización; condición; incremento)`**

**Línea 3: `for (int i = 1; i <= 5; i++)`**
- **Inicialización**: `int i = 1` - Crea variable `i` con valor inicial 1
- **Condición**: `i <= 5` - El bucle continúa mientras `i <= 5` sea `true`
- **Incremento**: `i++` - Incrementa `i` en 1 después de cada iteración

**Ejecución del Bucle:**
1. **Iteración 1**: `i = 1`, `1 <= 5` es `true` → Imprime "Número: 1", `i++` → `i = 2`
2. **Iteración 2**: `i = 2`, `2 <= 5` es `true` → Imprime "Número: 2", `i++` → `i = 3`
3. **Iteración 3**: `i = 3`, `3 <= 5` es `true` → Imprime "Número: 3", `i++` → `i = 4`
4. **Iteración 4**: `i = 4`, `4 <= 5` es `true` → Imprime "Número: 4", `i++` → `i = 5`
5. **Iteración 5**: `i = 5`, `5 <= 5` es `true` → Imprime "Número: 5", `i++` → `i = 6`
6. **Fin del bucle**: `i = 6`, `6 <= 5` es `false` → El bucle termina

#### 3.2 Bucle while

El bucle `while` se ejecuta mientras una condición sea verdadera:

```csharp
int contador = 1;                  // Variable contador inicializada en 1

Console.WriteLine("Contando con while:");

while (contador <= 5)              // Mientras contador <= 5 sea true
{
    Console.WriteLine($"Contador: {contador}");
    contador++;                    // Incrementa contador en 1
}

Console.WriteLine($"Contador final: {contador}");
Console.WriteLine("Bucle while completado");
```

#### Explicación del Bucle while:

**Línea 1: `int contador = 1;`**
- Declara variable `contador` con valor inicial 1

**Línea 5: `while (contador <= 5)`**
- Evalúa la condición `contador <= 5`
- Si es `true`, ejecuta el bloque del bucle
- Si es `false`, termina el bucle

**Línea 7: `contador++;`**
- **IMPORTANTE**: Debe haber algo que cambie la condición
- Sin esto, el bucle sería infinito

**Ejecución del Bucle:**
1. **Iteración 1**: `contador = 1`, `1 <= 5` es `true` → Imprime "Contador: 1", `contador++` → `contador = 2`
2. **Iteración 2**: `contador = 2`, `2 <= 5` es `true` → Imprime "Contador: 2", `contador++` → `contador = 3`
3. **Iteración 3**: `contador = 3`, `3 <= 5` es `true` → Imprime "Contador: 3", `contador++` → `contador = 4`
4. **Iteración 4**: `contador = 4`, `4 <= 5` es `true` → Imprime "Contador: 4", `contador++` → `contador = 5`
5. **Iteración 5**: `contador = 5`, `5 <= 5` es `true` → Imprime "Contador: 5", `contador++` → `contador = 6`
6. **Fin del bucle**: `contador = 6`, `6 <= 5` es `false` → El bucle termina

#### 3.3 Bucle do-while

El bucle `do-while` ejecuta el código al menos una vez, luego evalúa la condición:

```csharp
int numero = 1;                    // Variable numero inicializada en 1

Console.WriteLine("Contando con do-while:");

do                                 // Hacer al menos una vez
{
    Console.WriteLine($"Número: {numero}");
    numero++;                      // Incrementa numero en 1
} while (numero <= 5);             // Mientras numero <= 5 sea true

Console.WriteLine($"Número final: {numero}");
Console.WriteLine("Bucle do-while completado");
```

#### Explicación del Bucle do-while:

**Diferencia clave con while:**
- **while**: Evalúa la condición ANTES de ejecutar el código
- **do-while**: Ejecuta el código PRIMERO, luego evalúa la condición

**Ejecución del Bucle:**
1. **Primera ejecución**: Siempre se ejecuta, imprime "Número: 1", `numero++` → `numero = 2`
2. **Evalúa condición**: `2 <= 5` es `true` → Continúa el bucle
3. **Segunda ejecución**: Imprime "Número: 2", `numero++` → `numero = 3`
4. **Evalúa condición**: `3 <= 5` es `true` → Continúa el bucle
5. **Continúa...** hasta que `numero > 5`

#### 3.4 Bucle foreach

El bucle `foreach` itera sobre colecciones (arrays, listas, etc.):

```csharp
string[] frutas = { "Manzana", "Banana", "Naranja", "Uva" }; // Array de strings

Console.WriteLine("Lista de frutas:");

foreach (string fruta in frutas)   // Para cada fruta en el array frutas
{
    Console.WriteLine($"- {fruta}");
}

Console.WriteLine("Bucle foreach completado");
```

#### Explicación del Bucle foreach:

**Línea 1: `string[] frutas = { "Manzana", "Banana", "Naranja", "Uva" };`**
- Crea un array de strings con 4 elementos

**Línea 5: `foreach (string fruta in frutas)`**
- `string fruta` - Variable temporal que toma el valor de cada elemento
- `in frutas` - Especifica la colección a iterar

**Ejecución del Bucle:**
1. **Iteración 1**: `fruta = "Manzana"` → Imprime "- Manzana"
2. **Iteración 2**: `fruta = "Banana"` → Imprime "- Banana"
3. **Iteración 3**: `fruta = "Naranja"` → Imprime "- Naranja"
4. **Iteración 4**: `fruta = "Uva"` → Imprime "- Uva"
5. **Fin del bucle**: No hay más elementos

### 4. Control de Flujo

#### 4.1 break

La instrucción `break` termina la ejecución de un bucle o switch:

```csharp
Console.WriteLine("Buscando el número 3:");

for (int i = 1; i <= 10; i++)     // Bucle del 1 al 10
{
    if (i == 3)                    // Si i es igual a 3
    {
        Console.WriteLine($"¡Encontrado el número {i}!");
        break;                      // Termina el bucle inmediatamente
    }
    
    Console.WriteLine($"Revisando número {i}");
}

Console.WriteLine("Búsqueda completada");
```

#### Explicación del break:

**Ejecución del Bucle:**
1. **Iteración 1**: `i = 1`, `1 == 3` es `false` → Imprime "Revisando número 1"
2. **Iteración 2**: `i = 2`, `2 == 3` es `false` → Imprime "Revisando número 2"
3. **Iteración 3**: `i = 3`, `3 == 3` es `true` → Imprime "¡Encontrado el número 3!", `break` → Termina el bucle
4. **No se ejecutan más iteraciones**

#### 4.2 continue

La instrucción `continue` salta a la siguiente iteración del bucle:

```csharp
Console.WriteLine("Números del 1 al 10 (excluyendo múltiplos de 3):");

for (int i = 1; i <= 10; i++)     // Bucle del 1 al 10
{
    if (i % 3 == 0)                // Si i es múltiplo de 3
    {
        continue;                   // Salta a la siguiente iteración
    }
    
    Console.WriteLine($"Número: {i}"); // Solo se ejecuta si i NO es múltiplo de 3
}

Console.WriteLine("Bucle completado");
```

#### Explicación del continue:

**Ejecución del Bucle:**
1. **Iteración 1**: `i = 1`, `1 % 3 == 0` es `false` → Imprime "Número: 1"
2. **Iteración 2**: `i = 2`, `2 % 3 == 0` es `false` → Imprime "Número: 2"
3. **Iteración 3**: `i = 3`, `3 % 3 == 0` es `true` → `continue` → Salta a la siguiente iteración
4. **Iteración 4**: `i = 4`, `4 % 3 == 0` es `false` → Imprime "Número: 4"
5. **Continúa...** excluyendo 6, 9 (múltiplos de 3)

#### 4.3 return

La instrucción `return` termina la ejecución de un método:

```csharp
static string VerificarEdad(int edad)  // Método que recibe un parámetro edad
{
    if (edad < 0)                      // Si la edad es negativa
    {
        return "Edad inválida";        // Retorna y termina el método
    }
    
    if (edad < 18)                     // Si la edad es menor a 18
    {
        return "Eres menor de edad";   // Retorna y termina el método
    }
    
    return "Eres mayor de edad";       // Retorna y termina el método
}

// Uso del método
Console.WriteLine(VerificarEdad(25));  // Salida: "Eres mayor de edad"
Console.WriteLine(VerificarEdad(16));  // Salida: "Eres menor de edad"
Console.WriteLine(VerificarEdad(-5));  // Salida: "Edad inválida"
```

### 5. Estructuras Anidadas

Las estructuras de control pueden anidarse (una dentro de otra):

```csharp
int edad = 25;                      // Variable edad
bool tieneLicencia = true;          // Variable tieneLicencia

Console.WriteLine("Verificación de conducir:");

if (edad >= 18)                     // Primera condición: ¿es mayor de 18?
{
    Console.WriteLine("✓ Cumples la edad mínima");
    
    if (tieneLicencia)              // Segunda condición anidada: ¿tiene licencia?
    {
        Console.WriteLine("✓ Tienes licencia de conducir");
        Console.WriteLine("✅ Puedes conducir legalmente");
    }
    else                            // Si no tiene licencia
    {
        Console.WriteLine("✗ No tienes licencia de conducir");
        Console.WriteLine("❌ No puedes conducir legalmente");
    }
}
else                                // Si no cumple la edad mínima
{
    Console.WriteLine("✗ No cumples la edad mínima");
    Console.WriteLine("❌ No puedes conducir legalmente");
}

Console.WriteLine("Verificación completada");
```

#### Explicación de las Estructuras Anidadas:

**Flujo de Ejecución:**
1. **Primera condición**: `edad >= 18` → `25 >= 18` es `true`
2. **Se ejecuta el primer bloque**: Imprime "✓ Cumples la edad mínima"
3. **Segunda condición anidada**: `tieneLicencia` → `true`
4. **Se ejecuta el segundo bloque**: Imprime "✓ Tienes licencia de conducir" y "✅ Puedes conducir legalmente"
5. **No se ejecuta el else del if anidado**
6. **No se ejecuta el else del if principal**
7. **Se ejecuta el código final**: "Verificación completada"

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Verificador de Números
```csharp
int numero = 7;

if (numero > 0)
{
    if (numero % 2 == 0)
    {
        Console.WriteLine($"{numero} es un número positivo par");
    }
    else
    {
        Console.WriteLine($"{numero} es un número positivo impar");
    }
}
else if (numero < 0)
{
    Console.WriteLine($"{numero} es un número negativo");
}
else
{
    Console.WriteLine($"{numero} es cero");
}
```

### Ejercicio 2: Tabla de Multiplicar
```csharp
int numero = 5;
Console.WriteLine($"Tabla de multiplicar del {numero}:");

for (int i = 1; i <= 10; i++)
{
    int resultado = numero * i;
    Console.WriteLine($"{numero} x {i} = {resultado}");
}
```

### Ejercicio 3: Búsqueda en Array
```csharp
int[] numeros = { 10, 25, 8, 42, 15, 30 };
int numeroBuscado = 25;
bool encontrado = false;

Console.WriteLine($"Buscando el número {numeroBuscado}...");

for (int i = 0; i < numeros.Length; i++)
{
    if (numeros[i] == numeroBuscado)
    {
        Console.WriteLine($"¡Encontrado en la posición {i}!");
        encontrado = true;
        break;
    }
}

if (!encontrado)
{
    Console.WriteLine("Número no encontrado");
}
```

### Ejercicio 4: Calculadora de Promedio
```csharp
int[] calificaciones = { 85, 92, 78, 96, 88 };
int suma = 0;

Console.WriteLine("Calificaciones:");
for (int i = 0; i < calificaciones.Length; i++)
{
    Console.WriteLine($"Calificación {i + 1}: {calificaciones[i]}");
    suma += calificaciones[i];
}

double promedio = (double)suma / calificaciones.Length;
Console.WriteLine($"\nPromedio: {promedio:F2}");

if (promedio >= 90)
{
    Console.WriteLine("¡Excelente rendimiento!");
}
else if (promedio >= 80)
{
    Console.WriteLine("¡Buen rendimiento!");
}
else if (promedio >= 70)
{
    Console.WriteLine("Rendimiento satisfactorio");
}
else
{
    Console.WriteLine("Necesitas mejorar");
}
```

## 🔍 Conceptos Importantes a Recordar

1. **Las estructuras condicionales** controlan qué código se ejecuta
2. **Los bucles** repiten código múltiples veces
3. **break** termina la ejecución de un bucle o switch
4. **continue** salta a la siguiente iteración de un bucle
5. **return** termina la ejecución de un método
6. **Las estructuras pueden anidarse** para crear lógica compleja
7. **El bucle for** es ideal cuando conoces el número de iteraciones
8. **El bucle while** es ideal cuando no conoces el número de iteraciones
9. **El bucle foreach** es ideal para iterar sobre colecciones

## ❓ Preguntas de Repaso

1. ¿Cuál es la diferencia entre `if` y `switch`?
2. ¿Qué hace la instrucción `break` en un bucle?
3. ¿Cuándo usarías un bucle `for` en lugar de `while`?
4. ¿Qué significa anidar estructuras de control?
5. ¿Cómo funciona el bucle `do-while`?

## 🚀 Siguiente Paso

En la próxima clase aprenderemos sobre **Colecciones Básicas**, donde veremos cómo trabajar con arrays, listas y otras estructuras de datos en C#.

---

## 📚 Recursos Adicionales

- [Estructuras de control en C#](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/statements/)
- [Bucles en C#](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/statements/iteration-statements/)
- [Instrucciones de salto](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/statements/jump-statements/)

---

**¡Excelente! Ahora dominas las estructuras de control en C#! 🎯**
