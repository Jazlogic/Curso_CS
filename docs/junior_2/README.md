# 🎯 Junior Level 2: Estructuras de Control y Funciones

## 📚 Descripción

En este nivel aprenderás a controlar el flujo de tu programa usando estructuras condicionales, bucles y cómo crear funciones reutilizables.

## 🎯 Objetivos de Aprendizaje

- Usar estructuras condicionales (if, else, switch)
- Implementar bucles (for, while, do-while, foreach)
- Crear y usar funciones/métodos
- Entender el scope de variables
- Manejar la entrada del usuario
- Crear programas interactivos

## 📖 Contenido Teórico

### 1. Estructuras Condicionales

#### If - Else
```csharp
if (condicion)
{
    // Código a ejecutar si la condición es verdadera
}
else if (otraCondicion)
{
    // Código a ejecutar si otraCondicion es verdadera
}
else
{
    // Código a ejecutar si ninguna condición es verdadera
}
```

#### Switch Statement
```csharp
switch (variable)
{
    case valor1:
        // Código para valor1
        break;
    case valor2:
        // Código para valor2
        break;
    default:
        // Código por defecto
        break;
}
```

### 2. Bucles

#### For Loop
```csharp
for (int i = 0; i < 10; i++)
{
    Console.WriteLine($"Iteración {i}");
}
```

#### While Loop
```csharp
int contador = 0;
while (contador < 5)
{
    Console.WriteLine($"Contador: {contador}");
    contador++;
}
```

#### Do-While Loop
```csharp
int numero;
do
{
    Console.WriteLine("Ingresa un número positivo: ");
    numero = Convert.ToInt32(Console.ReadLine());
} while (numero <= 0);
```

#### Foreach Loop
```csharp
string[] colores = { "Rojo", "Verde", "Azul" };
foreach (string color in colores)
{
    Console.WriteLine(color);
}
```

### 3. Funciones y Métodos

#### Sintaxis Básica
```csharp
[modificador] tipoRetorno NombreMetodo([parametros])
{
    // Cuerpo del método
    return valor; // Solo si el tipo de retorno no es void
}
```

#### Tipos de Métodos
```csharp
// Método sin parámetros ni retorno
public void Saludar()
{
    Console.WriteLine("¡Hola!");
}

// Método con parámetros
public void SaludarPersona(string nombre)
{
    Console.WriteLine($"¡Hola {nombre}!");
}

// Método con retorno
public int Sumar(int a, int b)
{
    return a + b;
}

// Método con parámetros opcionales
public void MostrarInfo(string nombre, int edad = 18)
{
    Console.WriteLine($"Nombre: {nombre}, Edad: {edad}");
}
```

### 4. Scope de Variables

```csharp
public class Program
{
    static int variableGlobal = 10; // Variable de clase
    
    static void Main(string[] args)
    {
        int variableLocal = 20; // Variable local del método
        
        if (true)
        {
            int variableBloque = 30; // Variable solo visible en este bloque
            Console.WriteLine(variableBloque); // OK
        }
        
        // Console.WriteLine(variableBloque); // ERROR - No visible aquí
        Console.WriteLine(variableLocal); // OK
        Console.WriteLine(variableGlobal); // OK
    }
}
```

### 5. Entrada del Usuario

```csharp
// Leer string
string nombre = Console.ReadLine();

// Leer número entero
int edad = Convert.ToInt32(Console.ReadLine());

// Leer número decimal
double precio = Convert.ToDouble(Console.ReadLine());

// Leer carácter
char letra = Convert.ToChar(Console.ReadKey().KeyChar);
```

### 6. Control de Flujo Avanzado

#### Break y Continue
```csharp
for (int i = 0; i < 10; i++)
{
    if (i == 5)
        break; // Sale del bucle
    
    if (i == 3)
        continue; // Salta a la siguiente iteración
    
    Console.WriteLine(i);
}
```

#### Goto (Usar con precaución)
```csharp
for (int i = 0; i < 10; i++)
{
    if (i == 5)
        goto fin; // Salta a la etiqueta
}

fin:
Console.WriteLine("Bucle terminado");
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Calculadora con Menú
Crea un programa que muestre un menú con opciones para sumar, restar, multiplicar y dividir. Usa switch para manejar las opciones.

### Ejercicio 2: Verificador de Edad
Crea un programa que verifique si una persona es menor de edad, mayor de edad o anciana (más de 65 años).

### Ejercicio 3: Tabla de Multiplicar
Genera la tabla de multiplicar de un número ingresado por el usuario usando un bucle for.

### Ejercicio 4: Adivina el Número
Crea un juego donde el usuario debe adivinar un número aleatorio entre 1 y 100. Usa while para continuar hasta que adivine.

### Ejercicio 5: Factorial
Calcula el factorial de un número usando un bucle. El factorial de n es n * (n-1) * (n-2) * ... * 1.

### Ejercicio 6: Números Primos
Crea un programa que determine si un número es primo o no.

### Ejercicio 7: Serie Fibonacci
Genera los primeros n números de la serie Fibonacci usando un bucle.

### Ejercicio 8: Palíndromo
Verifica si una palabra es un palíndromo (se lee igual de izquierda a derecha que de derecha a izquierda).

### Ejercicio 9: Calculadora de Promedio con Validación
Crea un programa que calcule el promedio de n números, validando que los números estén en un rango específico.

### Ejercicio 10: Proyecto Integrador - Juego de Adivinanzas
Crea un juego completo que incluya:
- Menú principal con opciones
- Diferentes tipos de adivinanzas
- Sistema de puntuación
- Validación de entrada del usuario
- Opción de jugar de nuevo

## 📝 Quiz de Autoevaluación

1. ¿Cuál es la diferencia entre `while` y `do-while`?
2. ¿Cuándo usarías `break` en un bucle?
3. ¿Qué significa el scope de una variable?
4. ¿Cómo pasarías múltiples parámetros a una función?
5. ¿Cuál es la diferencia entre `==` y `=`?

## 🚀 Siguiente Nivel

Una vez que hayas completado todos los ejercicios y comprendas los conceptos, estarás listo para el **Junior Level 3: Arrays, Listas y Colecciones Básicas**.

## 💡 Consejos de Estudio

- Practica creando diferentes tipos de bucles
- Experimenta con diferentes condiciones en if-else
- Crea funciones que resuelvan problemas específicos
- Usa el debugger para seguir el flujo de tu programa
- Documenta tus funciones con comentarios explicativos

¡Continúa construyendo tu base sólida en C#! 🚀
