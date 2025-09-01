# Clase 2: Variables y Tipos de Datos en C#

## 🎯 Objetivos de la Clase
- Comprender qué son las variables y cómo declararlas
- Conocer todos los tipos de datos básicos en C#
- Aprender a convertir entre diferentes tipos de datos
- Entender el concepto de inferencia de tipos con `var`

## 📚 Contenido Teórico

### 1. ¿Qué son las Variables?

Una variable es un **contenedor** en la memoria de la computadora que almacena un valor. Es como una "caja" donde puedes guardar información y cambiarla durante la ejecución del programa.

#### Características de las Variables:
- **Nombre**: Identificador único para acceder al valor
- **Tipo**: Define qué tipo de datos puede almacenar
- **Valor**: El dato actual almacenado en la variable
- **Scope**: Dónde puede ser accedida la variable

### 2. Declaración de Variables

#### Sintaxis Básica
```csharp
tipoDeDato nombreVariable = valor;
```

#### Ejemplos de Declaración:
```csharp
int edad = 25;                    // Variable entera con valor 25
string nombre = "Juan";           // Variable string con valor "Juan"
double altura = 1.75;             // Variable double con valor 1.75
bool esEstudiante = true;         // Variable booleana con valor true
```

#### Explicación Línea por Línea:

**Línea 1: `int edad = 25;`**
- `int` es el tipo de dato (número entero)
- `edad` es el nombre de la variable
- `=` es el operador de asignación
- `25` es el valor inicial
- `;` indica el final de la instrucción

**Línea 2: `string nombre = "Juan";`**
- `string` es el tipo de dato (texto)
- `nombre` es el nombre de la variable
- `"Juan"` es un string literal (texto entre comillas)

**Línea 3: `double altura = 1.75;`**
- `double` es el tipo de dato (número decimal de doble precisión)
- `altura` es el nombre de la variable
- `1.75` es el valor decimal

**Línea 4: `bool esEstudiante = true;`**
- `bool` es el tipo de dato (booleano: true/false)
- `esEstudiante` es el nombre de la variable
- `true` es un valor booleano

### 3. Tipos de Datos Básicos

#### 3.1 Tipos Numéricos Enteros

```csharp
byte numeroPequeno = 255;         // 0 a 255 (8 bits)
sbyte numeroConSigno = -128;      // -128 a 127 (8 bits)
short numeroCorto = 32767;        // -32,768 a 32,767 (16 bits)
ushort numeroCortoPositivo = 65535; // 0 a 65,535 (16 bits)
int numeroEntero = 2147483647;    // -2,147,483,648 a 2,147,483,647 (32 bits)
uint numeroEnteroPositivo = 4294967295; // 0 a 4,294,967,295 (32 bits)
long numeroLargo = 9223372036854775807; // -9,223,372,036,854,775,808 a 9,223,372,036,854,775,807 (64 bits)
ulong numeroLargoPositivo = 18446744073709551615; // 0 a 18,446,744,073,709,551,615 (64 bits)
```

#### Explicación de los Tipos Enteros:

**`byte` (8 bits)**
- Rango: 0 a 255
- Uso: Valores pequeños positivos (edad, contadores)
- Ejemplo: `byte edad = 25;`

**`int` (32 bits)**
- Rango: -2,147,483,648 a 2,147,483,647
- Uso: Tipo entero estándar para la mayoría de aplicaciones
- Ejemplo: `int cantidad = 1000;`

**`long` (64 bits)**
- Rango: Muy amplio para números grandes
- Uso: Identificadores únicos, cálculos financieros grandes
- Ejemplo: `long idUsuario = 123456789012345;`

#### 3.2 Tipos Numéricos de Punto Flotante

```csharp
float numeroFloat = 3.14f;        // Precisión simple (32 bits)
double numeroDouble = 3.14159265359; // Precisión doble (64 bits) - POR DEFECTO
decimal numeroDecimal = 3.14159265359m; // Alta precisión (128 bits)
```

#### Explicación de los Tipos Decimales:

**`float` (32 bits)**
- Precisión: ~7 dígitos decimales
- Sufijo: `f` o `F`
- Uso: Gráficos 3D, cálculos científicos simples
- Ejemplo: `float temperatura = 36.5f;`

**`double` (64 bits)**
- Precisión: ~15-17 dígitos decimales
- Sufijo: Ninguno (es el tipo por defecto)
- Uso: Cálculos científicos, matemáticas generales
- Ejemplo: `double pi = 3.14159265359;`

**`decimal` (128 bits)**
- Precisión: 28-29 dígitos decimales
- Sufijo: `m` o `M`
- Uso: Cálculos financieros, donde la precisión es crítica
- Ejemplo: `decimal precio = 19.99m;`

#### 3.3 Tipos de Texto

```csharp
char caracter = 'A';              // Un solo carácter (16 bits)
string texto = "Hola Mundo";      // Secuencia de caracteres
```

#### Explicación de los Tipos de Texto:

**`char` (16 bits)**
- Almacena un solo carácter Unicode
- Comillas simples: `'A'`, `'1'`, `'@'`
- Uso: Caracteres individuales, códigos ASCII
- Ejemplo: `char letra = 'B';`

**`string** (referencia)
- Secuencia de caracteres Unicode
- Comillas dobles: `"Texto"`
- Uso: Nombres, direcciones, cualquier texto
- Ejemplo: `string nombre = "María García";`

#### 3.4 Tipos Booleanos

```csharp
bool esVerdadero = true;          // true o false
bool esFalso = false;             // Solo dos valores posibles
```

#### Explicación del Tipo Booleano:

**`bool`**
- Solo dos valores: `true` o `false`
- Uso: Condiciones, flags, estados
- Ejemplo: `bool estaActivo = true;`

#### 3.5 Tipos de Referencia

```csharp
object objetoGenerico = "Cualquier cosa"; // Tipo base de todos los tipos
dynamic tipoDinamico = 42;        // Tipo resuelto en tiempo de ejecución
```

### 4. Declaración de Variables sin Inicialización

Puedes declarar variables sin asignarles un valor inicial:

```csharp
int numero;                        // Declaración sin inicialización
string nombre;                     // Declaración sin inicialización
double precio;                     // Declaración sin inicialización

// Más tarde puedes asignar valores:
numero = 10;                       // Asignación posterior
nombre = "Producto";               // Asignación posterior
precio = 29.99;                    // Asignación posterior
```

#### Explicación:
- **Línea 1**: `int numero;` - Declara una variable entera sin valor
- **Línea 2**: `string nombre;` - Declara una variable string sin valor
- **Línea 3**: `double precio;` - Declara una variable double sin valor
- **Línea 5**: `numero = 10;` - Asigna el valor 10 a la variable numero
- **Línea 6**: `nombre = "Producto";` - Asigna el texto "Producto"
- **Línea 7**: `precio = 29.99;` - Asigna el valor decimal 29.99

### 5. Declaración Múltiple

Puedes declarar múltiples variables del mismo tipo en una sola línea:

```csharp
int x = 1, y = 2, z = 3;          // Tres variables int en una línea
string nombre = "Juan", apellido = "Pérez"; // Dos variables string
double precio1 = 10.50, precio2 = 20.75; // Dos variables double
```

#### Explicación:
- **Línea 1**: Declara tres variables int (`x`, `y`, `z`) con valores 1, 2 y 3
- **Línea 2**: Declara dos variables string (`nombre`, `apellido`) con valores "Juan" y "Pérez"
- **Línea 3**: Declara dos variables double (`precio1`, `precio2`) con valores 10.50 y 20.75

### 6. Inferencia de Tipos con `var`

C# puede inferir automáticamente el tipo de una variable basándose en el valor asignado:

```csharp
var numero = 42;                   // C# infiere que es int
var texto = "Hola";                // C# infere que es string
var decimal = 3.14;                // C# infere que es double
var booleano = true;               // C# infere que es bool
var caracter = 'A';                // C# infere que es char
```

#### Explicación:
- **Línea 1**: `var numero = 42;` - C# ve el valor 42 y deduce que es `int`
- **Línea 2**: `var texto = "Hola";` - C# ve el texto "Hola" y deduce que es `string`
- **Línea 3**: `var decimal = 3.14;` - C# ve 3.14 y deduce que es `double`
- **Línea 4**: `var booleano = true;` - C# ve true y deduce que es `bool`
- **Línea 5**: `var caracter = 'A';` - C# ve 'A' y deduce que es `char`

#### Limitaciones de `var`:
- **Debe inicializarse**: `var numero;` // ❌ Error
- **No puede ser null**: `var valor = null;` // ❌ Error
- **Debe ser obvio el tipo**: `var resultado = Sumar(5, 3);` // ✅ OK si Sumar retorna int

### 7. Conversión de Tipos

#### 7.1 Conversión Implícita (Automática)

```csharp
int numeroEntero = 10;             // Variable int con valor 10
long numeroLargo = numeroEntero;    // Conversión automática de int a long
double numeroDecimal = numeroEntero; // Conversión automática de int a double

Console.WriteLine($"Entero: {numeroEntero}");      // Salida: Entero: 10
Console.WriteLine($"Largo: {numeroLargo}");       // Salida: Largo: 10
Console.WriteLine($"Decimal: {numeroDecimal}");   // Salida: Decimal: 10
```

#### Explicación:
- **Línea 1**: Declara una variable `int` con valor 10
- **Línea 2**: Asigna el valor de `numeroEntero` a `numeroLargo` (conversión automática)
- **Línea 3**: Asigna el valor de `numeroEntero` a `numeroDecimal` (conversión automática)
- **Líneas 5-7**: Muestran los valores convertidos

#### Reglas de Conversión Implícita:
- `byte` → `short`, `int`, `long`, `float`, `double`, `decimal`
- `short` → `int`, `long`, `float`, `double`, `decimal`
- `int` → `long`, `float`, `double`, `decimal`
- `long` → `float`, `double`, `decimal`
- `float` → `double`
- `char` → `int`, `long`, `float`, `double`, `decimal`

#### 7.2 Conversión Explícita (Casting)

```csharp
double numeroDecimal = 10.5;        // Variable double con valor 10.5
int numeroEntero = (int)numeroDecimal; // Conversión explícita (casting)

Console.WriteLine($"Decimal original: {numeroDecimal}");  // Salida: 10.5
Console.WriteLine($"Entero convertido: {numeroEntero}"); // Salida: 10
```

#### Explicación:
- **Línea 1**: Declara una variable `double` con valor 10.5
- **Línea 2**: Convierte explícitamente `double` a `int` usando `(int)`
- **Línea 4**: Muestra el valor decimal original (10.5)
- **Línea 5**: Muestra el valor entero convertido (10) - se pierde la parte decimal

#### Ejemplos de Casting:
```csharp
double precio = 19.99;
int precioEntero = (int)precio;    // Resultado: 19

long numeroGrande = 123456789;
int numeroPequeno = (int)numeroGrande; // Resultado: 123456789 (si cabe en int)

char letra = 'A';
int codigoAscii = (int)letra;      // Resultado: 65 (código ASCII de 'A')
```

#### 7.3 Conversión con Métodos

```csharp
string texto = "123";              // String que representa un número
int numero = Convert.ToInt32(texto); // Conversión usando Convert.ToInt32()

string textoDecimal = "19.99";     // String que representa un decimal
double precio = Convert.ToDouble(textoDecimal); // Conversión a double

Console.WriteLine($"Texto: {texto}, Número: {numero}");           // Salida: Texto: 123, Número: 123
Console.WriteLine($"Texto: {textoDecimal}, Precio: {precio}");   // Salida: Texto: 19.99, Precio: 19.99
```

#### Explicación:
- **Línea 1**: Declara un string con el texto "123"
- **Línea 2**: Convierte el string a int usando `Convert.ToInt32()`
- **Línea 4**: Declara un string con el texto "19.99"
- **Línea 5**: Convierte el string a double usando `Convert.ToDouble()`
- **Líneas 7-8**: Muestran los valores convertidos

#### Métodos de Conversión Comunes:
```csharp
string texto = "42";
int numero = Convert.ToInt32(texto);      // String a int
double decimal = Convert.ToDouble(texto); // String a double
bool booleano = Convert.ToBoolean("true"); // String a bool
string resultado = numero.ToString();     // int a string
```

### 8. Convenciones de Nomenclatura

#### Reglas para Nombres de Variables:
1. **Pueden contener**: letras, dígitos, guiones bajos
2. **Deben empezar con**: letra o guión bajo
3. **No pueden ser**: palabras reservadas de C#
4. **Sensible a mayúsculas**: `edad` y `Edad` son diferentes

#### Estilos de Nomenclatura:
```csharp
// camelCase (recomendado para variables locales)
string nombreUsuario = "Juan";
int edadPersona = 25;
bool estaActivo = true;

// PascalCase (recomendado para métodos y clases)
string NombreCompleto = "Juan Pérez";
int EdadMaxima = 100;

// UPPER_CASE (recomendado para constantes)
const int MAXIMO_INTENTOS = 3;
const string VERSION_APP = "1.0.0";
```

### 9. Constantes

Las constantes son variables cuyo valor no puede cambiar después de la inicialización:

```csharp
const double PI = 3.14159265359;   // Constante para el número Pi
const string NOMBRE_APP = "Mi Aplicación"; // Constante para el nombre de la app
const int MAXIMO_USUARIOS = 1000;  // Constante para el máximo de usuarios

Console.WriteLine($"Valor de Pi: {PI}");
Console.WriteLine($"Nombre de la aplicación: {NOMBRE_APP}");
Console.WriteLine($"Máximo de usuarios: {MAXIMO_USUARIOS}");
```

#### Explicación:
- **Línea 1**: `const double PI = 3.14159265359;` - Declara una constante double
- **Línea 2**: `const string NOMBRE_APP = "Mi Aplicación";` - Constante string
- **Línea 3**: `const int MAXIMO_USUARIOS = 1000;` - Constante int
- **Líneas 5-7**: Muestran los valores de las constantes

#### Características de las Constantes:
- **Deben inicializarse** al declararlas
- **No pueden cambiar** después de la inicialización
- **Se calculan en tiempo de compilación**
- **Mejoran el rendimiento** del programa

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Declaración de Variables
Declara variables para almacenar información personal:
```csharp
// Tu código aquí
string nombre = "Tu Nombre";
int edad = 25;
double altura = 1.75;
bool esEstudiante = true;
char inicial = 'T';

Console.WriteLine($"Nombre: {nombre}");
Console.WriteLine($"Edad: {edad}");
Console.WriteLine($"Altura: {altura}");
Console.WriteLine($"¿Es estudiante? {esEstudiante}");
Console.WriteLine($"Inicial: {inicial}");
```

### Ejercicio 2: Conversiones de Tipo
Practica diferentes tipos de conversiones:
```csharp
// Conversión implícita
int numero = 42;
double decimal = numero; // Conversión automática

// Conversión explícita (casting)
double precio = 19.99;
int precioEntero = (int)precio;

// Conversión con métodos
string texto = "123";
int numeroConvertido = Convert.ToInt32(texto);

Console.WriteLine($"Número original: {numero}");
Console.WriteLine($"Decimal convertido: {decimal}");
Console.WriteLine($"Precio original: {precio}");
Console.WriteLine($"Precio entero: {precioEntero}");
Console.WriteLine($"Texto convertido: {numeroConvertido}");
```

### Ejercicio 3: Cálculos con Diferentes Tipos
```csharp
int numero1 = 10;
double numero2 = 5.5;
decimal numero3 = 2.5m;

// Conversiones automáticas en cálculos
double resultado1 = numero1 + numero2; // int + double = double
decimal resultado2 = numero3 * 2;      // decimal * int = decimal

Console.WriteLine($"Resultado 1: {resultado1}");
Console.WriteLine($"Resultado 2: {resultado2}");
```

### Ejercicio 4: Uso de Var
```csharp
var numero = 42;           // C# infere int
var texto = "Hola";        // C# infere string
var decimal = 3.14;        // C# infere double
var booleano = true;       // C# infere bool

Console.WriteLine($"Tipo de numero: {numero.GetType()}");
Console.WriteLine($"Tipo de texto: {texto.GetType()}");
Console.WriteLine($"Tipo de decimal: {decimal.GetType()}");
Console.WriteLine($"Tipo de booleano: {booleano.GetType()}");
```

## 🔍 Conceptos Importantes a Recordar

1. **Las variables son contenedores** para almacenar datos en memoria
2. **Cada variable tiene un tipo** que define qué datos puede almacenar
3. **Los tipos numéricos tienen diferentes rangos** y precisiones
4. **La conversión implícita** ocurre automáticamente cuando es segura
5. **El casting explícito** es necesario cuando puede haber pérdida de datos
6. **`var` permite inferencia de tipos** pero debe inicializarse
7. **Las constantes** no pueden cambiar después de la inicialización
8. **La nomenclatura** sigue convenciones estándar de C#

## ❓ Preguntas de Repaso

1. ¿Qué es una variable y para qué sirve?
2. ¿Cuál es la diferencia entre `int`, `double` y `decimal`?
3. ¿Cuándo usarías `var` en lugar de un tipo explícito?
4. ¿Qué pasa si intentas convertir un `double` grande a `int`?
5. ¿Por qué es importante elegir el tipo de dato correcto?

## 🚀 Siguiente Paso

En la próxima clase aprenderemos sobre **Operadores y Expresiones**, donde veremos cómo realizar operaciones matemáticas y lógicas con nuestras variables.

---

## 📚 Recursos Adicionales

- [Tipos de datos en C#](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/)
- [Conversión de tipos](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/types/casting-and-type-conversions/)
- [Convenciones de nomenclatura](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/naming-guidelines/)

---

**¡Excelente! Ahora entiendes cómo trabajar con variables y tipos de datos en C#! 🎯**
