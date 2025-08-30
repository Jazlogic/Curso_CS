# 🎯 Junior Level 1: Fundamentos Básicos de C#

## 📚 Descripción

En este nivel aprenderás los conceptos más fundamentales de C#: variables, tipos de datos, operadores y la estructura básica de un programa.

## 🎯 Objetivos de Aprendizaje

- Entender qué es C# y .NET
- Crear tu primer programa "Hello World"
- Declarar y usar variables
- Conocer los tipos de datos básicos
- Usar operadores aritméticos, de comparación y lógicos
- Entender la conversión de tipos

## 📖 Contenido Teórico

### 1. ¿Qué es C#?

C# es un lenguaje de programación moderno, orientado a objetos, desarrollado por Microsoft como parte de la plataforma .NET. Es ampliamente utilizado para:
- Desarrollo de aplicaciones Windows
- Desarrollo web backend
- Aplicaciones móviles
- Juegos con Unity
- Aplicaciones de escritorio

### 2. Estructura Básica de un Programa C#

```csharp
using System;

namespace MiPrimerPrograma
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("¡Hola Mundo!");
        }
    }
}
```

### 3. Variables y Tipos de Datos

#### Tipos Numéricos
- `int`: Números enteros (32 bits)
- `long`: Números enteros largos (64 bits)
- `float`: Números decimales de precisión simple
- `double`: Números decimales de precisión doble
- `decimal`: Números decimales de alta precisión

#### Tipos de Texto
- `string`: Cadenas de texto
- `char`: Un solo carácter

#### Tipos Booleanos
- `bool`: Verdadero o falso

#### Tipos de Referencia
- `object`: Tipo base de todos los tipos

### 4. Declaración de Variables

```csharp
// Declaración e inicialización
int edad = 25;
string nombre = "Juan";
bool esEstudiante = true;

// Declaración sin inicialización
int numero;
numero = 10;

// Declaración múltiple
int x = 1, y = 2, z = 3;

// Inferencia de tipos (var)
var precio = 19.99m; // El compilador infiere que es decimal
```

### 5. Operadores

#### Operadores Aritméticos
- `+` : Suma
- `-` : Resta
- `*` : Multiplicación
- `/` : División
- `%` : Módulo (resto de la división)
- `++` : Incremento
- `--` : Decremento

#### Operadores de Asignación
- `=` : Asignación simple
- `+=` : Suma y asignación
- `-=` : Resta y asignación
- `*=` : Multiplicación y asignación
- `/=` : División y asignación

#### Operadores de Comparación
- `==` : Igual a
- `!=` : Diferente de
- `<` : Menor que
- `>` : Mayor que
- `<=` : Menor o igual que
- `>=` : Mayor o igual que

#### Operadores Lógicos
- `&&` : AND lógico
- `||` : OR lógico
- `!` : NOT lógico

### 6. Conversión de Tipos

```csharp
// Conversión implícita (automática)
int numeroEntero = 10;
long numeroLargo = numeroEntero; // OK

// Conversión explícita (casting)
double numeroDecimal = 10.5;
int numeroEntero = (int)numeroDecimal; // Resultado: 10

// Conversión con métodos
string texto = "123";
int numero = Convert.ToInt32(texto);
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Hello World
Crea un programa que muestre "¡Hola Mundo!" en la consola.

### Ejercicio 2: Variables Básicas
Declara variables para almacenar tu nombre, edad, altura y si eres estudiante.

### Ejercicio 3: Calculadora Simple
Crea un programa que calcule la suma, resta, multiplicación y división de dos números.

### Ejercicio 4: Conversión de Temperatura
Convierte una temperatura de Celsius a Fahrenheit usando la fórmula: F = C * 9/5 + 32

### Ejercicio 5: Cálculo de Área
Calcula el área de un círculo dado su radio (A = π * r²)

### Ejercicio 6: Operadores de Comparación
Compara dos números y muestra cuál es mayor, menor o si son iguales.

### Ejercicio 7: Operadores Lógicos
Crea un programa que determine si un número está en un rango específico usando operadores lógicos.

### Ejercicio 8: Conversión de Tipos
Convierte diferentes tipos de datos entre sí y observa los resultados.

### Ejercicio 9: Cálculo de Promedio
Calcula el promedio de tres números enteros.

### Ejercicio 10: Proyecto Integrador - Calculadora de IMC
Crea un programa que calcule el Índice de Masa Corporal (IMC) usando peso y altura.

## 📝 Quiz de Autoevaluación

1. ¿Qué tipo de dato usarías para almacenar el precio de un producto?
2. ¿Cuál es la diferencia entre `int` y `long`?
3. ¿Qué operador usarías para verificar si dos valores son iguales?
4. ¿Cómo declararías una variable que almacene tu nombre completo?
5. ¿Qué significa el operador `%`?

## 🚀 Siguiente Nivel

Una vez que hayas completado todos los ejercicios y comprendas los conceptos, estarás listo para el **Junior Level 2: Estructuras de Control y Funciones**.

## 💡 Consejos de Estudio

- Practica cada concepto con ejemplos simples
- Experimenta cambiando valores en los ejercicios
- Usa el debugger de Visual Studio para entender el flujo del programa
- No te frustres si algo no funciona al primer intento
- Documenta tu código con comentarios

¡Manos a la obra! 🎯
