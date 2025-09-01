# Clase 3: Operadores y Expresiones en C#

## 🎯 Objetivos de la Clase
- Comprender los diferentes tipos de operadores en C#
- Aprender a realizar operaciones matemáticas básicas
- Entender los operadores de comparación y lógicos
- Dominar los operadores de asignación y su uso

---

## 📚 Navegación del Módulo 1

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_introduccion.md) | Introducción a C# y .NET | |
| [Clase 2](clase_2_variables_tipos.md) | Variables y Tipos de Datos | ← Anterior |
| **Clase 3** | **Operadores y Expresiones** | ← Estás aquí |
| [Clase 4](clase_4_estructuras_control.md) | Estructuras de Control | Siguiente → |
| [Clase 5](clase_5_arrays_collections.md) | Arrays y Colecciones | |
| [Clase 6](clase_6_strings.md) | Manipulación de Strings | |
| [Clase 7](clase_7_metodos.md) | Métodos y Funciones | |
| [Clase 8](clase_8_namespaces.md) | Namespaces y Organización | |
| [Clase 9](clase_9_manejo_errores.md) | Manejo de Errores | |
| [Clase 10](clase_10_poo_basica.md) | Programación Orientada a Objetos Básica | |

**← [Volver al README del Módulo 1](../junior_1/README.md)**

## 📚 Contenido Teórico

### 1. ¿Qué son los Operadores?

Los operadores son **símbolos especiales** que realizan operaciones específicas en uno, dos o tres operandos. Son la base para realizar cálculos, comparaciones y asignaciones en nuestros programas.

#### Tipos de Operadores:
- **Unarios**: Operan sobre un solo operando (ej: `++`, `--`, `!`)
- **Binarios**: Operan sobre dos operandos (ej: `+`, `-`, `*`, `/`)
- **Ternarios**: Operan sobre tres operandos (ej: `?:`)

### 2. Operadores Aritméticos

Los operadores aritméticos realizan operaciones matemáticas básicas:

#### 2.1 Operadores Básicos

```csharp
int a = 10;                       // Variable a con valor 10
int b = 3;                        // Variable b con valor 3

int suma = a + b;                 // Suma: 10 + 3 = 13
int resta = a - b;                // Resta: 10 - 3 = 7
int multiplicacion = a * b;       // Multiplicación: 10 * 3 = 30
int division = a / b;             // División: 10 / 3 = 3 (división entera)
int modulo = a % b;               // Módulo: 10 % 3 = 1 (resto de la división)

Console.WriteLine($"a = {a}, b = {b}");
Console.WriteLine($"Suma: {a} + {b} = {suma}");
Console.WriteLine($"Resta: {a} - {b} = {resta}");
Console.WriteLine($"Multiplicación: {a} * {b} = {multiplicacion}");
Console.WriteLine($"División: {a} / {b} = {division}");
Console.WriteLine($"Módulo: {a} % {b} = {modulo}");
```

#### Explicación Línea por Línea:

**Líneas 1-2: Declaración de Variables**
- `int a = 10;` - Declara variable `a` con valor 10
- `int b = 3;` - Declara variable `b` con valor 3

**Línea 3: Suma**
- `int suma = a + b;` - Declara variable `suma` y asigna el resultado de `a + b`
- El operador `+` suma los valores de `a` (10) y `b` (3)
- Resultado: `suma = 13`

**Línea 4: Resta**
- `int resta = a - b;` - Declara variable `resta` y asigna el resultado de `a - b`
- El operador `-` resta el valor de `b` (3) del valor de `a` (10)
- Resultado: `resta = 7`

**Línea 5: Multiplicación**
- `int multiplicacion = a * b;` - Declara variable `multiplicacion` y asigna el resultado de `a * b`
- El operador `*` multiplica los valores de `a` (10) y `b` (3)
- Resultado: `multiplicacion = 30`

**Línea 6: División**
- `int division = a / b;` - Declara variable `division` y asigna el resultado de `a / b`
- El operador `/` divide el valor de `a` (10) entre el valor de `b` (3)
- **Importante**: Como ambos son `int`, el resultado es división entera
- Resultado: `division = 3` (no 3.33...)

**Línea 7: Módulo**
- `int modulo = a % b;` - Declara variable `modulo` y asigna el resultado de `a % b`
- El operador `%` calcula el resto de la división de `a` (10) entre `b` (3)
- Resultado: `modulo = 1` (porque 10 ÷ 3 = 3 con resto 1)

#### 2.2 División con Tipos Decimales

```csharp
double a = 10.0;                  // Variable double con valor 10.0
double b = 3.0;                   // Variable double con valor 3.0

double division = a / b;          // División decimal: 10.0 / 3.0 = 3.333...

Console.WriteLine($"División decimal: {a} / {b} = {division}");
Console.WriteLine($"División decimal con formato: {a} / {b} = {division:F2}");
```

#### Explicación:
- **Línea 1**: `double a = 10.0;` - Declara variable `double` con valor 10.0
- **Línea 2**: `double b = 3.0;` - Declara variable `double` con valor 3.0
- **Línea 4**: `double division = a / b;` - División decimal que produce 3.333...
- **Línea 6**: `{division:F2}` - Formatea el resultado con 2 decimales

#### 2.3 Operadores de Incremento y Decremento

```csharp
int numero = 5;                   // Variable inicial con valor 5

// Incremento y decremento postfix (después de la variable)
int resultado1 = numero++;        // Asigna el valor actual (5) y luego incrementa
int resultado2 = numero--;        // Asigna el valor actual (6) y luego decrementa

Console.WriteLine($"Número original: {numero}");           // Salida: 5
Console.WriteLine($"Resultado 1 (postfix ++): {resultado1}"); // Salida: 5
Console.WriteLine($"Número después del primer incremento: {numero}"); // Salida: 6
Console.WriteLine($"Resultado 2 (postfix --): {resultado2}"); // Salida: 6
Console.WriteLine($"Número después del decremento: {numero}"); // Salida: 5

// Incremento y decremento prefix (antes de la variable)
int numero2 = 5;                  // Nueva variable con valor 5
int resultado3 = ++numero2;       // Incrementa primero y luego asigna
int resultado4 = --numero2;       // Decrementa primero y luego asigna

Console.WriteLine($"\nNúmero 2 original: {numero2}");     // Salida: 5
Console.WriteLine($"Resultado 3 (prefix ++): {resultado3}"); // Salida: 6
Console.WriteLine($"Resultado 4 (prefix --): {resultado4}"); // Salida: 5
```

#### Explicación Detallada:

**Operadores Postfix (`numero++`, `numero--`):**
- **Línea 3**: `int resultado1 = numero++;`
  1. Se asigna el valor actual de `numero` (5) a `resultado1`
  2. Luego se incrementa `numero` en 1
  3. `resultado1 = 5`, `numero = 6`

- **Línea 4**: `int resultado2 = numero--;`
  1. Se asigna el valor actual de `numero` (6) a `resultado2`
  2. Luego se decrementa `numero` en 1
  3. `resultado2 = 6`, `numero = 5`

**Operadores Prefix (`++numero`, `--numero`):**
- **Línea 12**: `int resultado3 = ++numero2;`
  1. Se incrementa `numero2` en 1 primero
  2. Luego se asigna el nuevo valor (6) a `resultado3`
  3. `resultado3 = 6`, `numero2 = 6`

- **Línea 13**: `int resultado4 = --numero2;`
  1. Se decrementa `numero2` en 1 primero
  2. Luego se asigna el nuevo valor (5) a `resultado4`
  3. `resultado4 = 5`, `numero2 = 5`

### 3. Operadores de Comparación

Los operadores de comparación comparan dos valores y retornan un resultado booleano (`true` o `false`):

```csharp
int a = 10;                       // Variable a con valor 10
int b = 5;                        // Variable b con valor 5

bool igual = a == b;              // ¿a es igual a b? false
bool diferente = a != b;          // ¿a es diferente de b? true
bool mayor = a > b;               // ¿a es mayor que b? true
bool menor = a < b;               // ¿a es menor que b? false
bool mayorIgual = a >= b;         // ¿a es mayor o igual a b? true
bool menorIgual = a <= b;         // ¿a es menor o igual a b? false

Console.WriteLine($"a = {a}, b = {b}");
Console.WriteLine($"¿{a} == {b}? {igual}");
Console.WriteLine($"¿{a} != {b}? {diferente}");
Console.WriteLine($"¿{a} > {b}? {mayor}");
Console.WriteLine($"¿{a} < {b}? {menor}");
Console.WriteLine($"¿{a} >= {b}? {mayorIgual}");
Console.WriteLine($"¿{a} <= {b}? {menorIgual}");
```

#### Explicación de los Operadores de Comparación:

**`==` (Igual a):**
- Compara si dos valores son iguales
- Retorna `true` si son iguales, `false` si son diferentes
- Ejemplo: `10 == 5` retorna `false`

**`!=` (Diferente de):**
- Compara si dos valores son diferentes
- Retorna `true` si son diferentes, `false` si son iguales
- Ejemplo: `10 != 5` retorna `true`

**`>` (Mayor que):**
- Compara si el primer valor es mayor que el segundo
- Retorna `true` si es mayor, `false` si no
- Ejemplo: `10 > 5` retorna `true`

**`<` (Menor que):**
- Compara si el primer valor es menor que el segundo
- Retorna `true` si es menor, `false` si no
- Ejemplo: `10 < 5` retorna `false`

**`>=` (Mayor o igual que):**
- Compara si el primer valor es mayor o igual al segundo
- Retorna `true` si es mayor o igual, `false` si es menor
- Ejemplo: `10 >= 5` retorna `true`

**`<=` (Menor o igual que):**
- Compara si el primer valor es menor o igual al segundo
- Retorna `true` si es menor o igual, `false` si es mayor
- Ejemplo: `10 <= 5` retorna `false`

### 4. Operadores Lógicos

Los operadores lógicos combinan expresiones booleanas:

#### 4.1 Operadores Básicos

```csharp
bool condicion1 = true;           // Primera condición
bool condicion2 = false;          // Segunda condición

bool resultadoAND = condicion1 && condicion2;    // AND lógico: true && false = false
bool resultadoOR = condicion1 || condicion2;     // OR lógico: true || false = true
bool resultadoNOT1 = !condicion1;                // NOT lógico: !true = false
bool resultadoNOT2 = !condicion2;                // NOT lógico: !false = true

Console.WriteLine($"Condición 1: {condicion1}");
Console.WriteLine($"Condición 2: {condicion2}");
Console.WriteLine($"Condición 1 AND Condición 2: {resultadoAND}");
Console.WriteLine($"Condición 1 OR Condición 2: {resultadoOR}");
Console.WriteLine($"NOT Condición 1: {resultadoNOT1}");
Console.WriteLine($"NOT Condición 2: {resultadoNOT2}");
```

#### Explicación de los Operadores Lógicos:

**`&&` (AND lógico):**
- Retorna `true` solo si AMBAS condiciones son `true`
- Tabla de verdad:
  - `true && true = true`
  - `true && false = false`
  - `false && true = false`
  - `false && false = false`

**`||` (OR lógico):**
- Retorna `true` si AL MENOS UNA condición es `true`
- Tabla de verdad:
  - `true || true = true`
  - `true || false = true`
  - `false || true = true`
  - `false || false = false`

**`!` (NOT lógico):**
- Invierte el valor booleano
- `!true = false`
- `!false = true`

#### 4.2 Ejemplos Prácticos

```csharp
int edad = 25;                    // Edad del usuario
bool tieneLicencia = true;        // ¿Tiene licencia de conducir?
bool esEstudiante = false;        // ¿Es estudiante?

// Condiciones compuestas
bool puedeConducir = edad >= 18 && tieneLicencia;           // Debe ser mayor de 18 Y tener licencia
bool tieneDescuento = edad < 18 || esEstudiante;           // Descuento si es menor de 18 O es estudiante
bool noPuedeConducir = !puedeConducir;                     // Negación de puedeConducir

Console.WriteLine($"Edad: {edad}");
Console.WriteLine($"¿Tiene licencia? {tieneLicencia}");
Console.WriteLine($"¿Es estudiante? {esEstudiante}");
Console.WriteLine($"¿Puede conducir? {puedeConducir}");
Console.WriteLine($"¿Tiene descuento? {tieneDescuento}");
Console.WriteLine($"¿No puede conducir? {noPuedeConducir}");
```

#### Explicación del Ejemplo:

**Línea 5: `bool puedeConducir = edad >= 18 && tieneLicencia;`**
1. `edad >= 18` evalúa a `true` (25 >= 18)
2. `tieneLicencia` es `true`
3. `true && true` evalúa a `true`
4. `puedeConducir = true`

**Línea 6: `bool tieneDescuento = edad < 18 || esEstudiante;`**
1. `edad < 18` evalúa a `false` (25 < 18)
2. `esEstudiante` es `false`
3. `false || false` evalúa a `false`
4. `tieneDescuento = false`

**Línea 7: `bool noPuedeConducir = !puedeConducir;`**
1. `puedeConducir` es `true`
2. `!true` evalúa a `false`
3. `noPuedeConducir = false`

### 5. Operadores de Asignación

Los operadores de asignación asignan valores a variables:

#### 5.1 Asignación Simple

```csharp
int numero = 10;                  // Asignación simple: numero = 10
Console.WriteLine($"Número inicial: {numero}");

numero = 20;                      // Cambio de valor: numero = 20
Console.WriteLine($"Número después del cambio: {numero}");
```

#### 5.2 Operadores de Asignación Compuesta

```csharp
int valor = 10;                   // Valor inicial

valor += 5;                       // Equivale a: valor = valor + 5
Console.WriteLine($"Después de += 5: {valor}"); // Salida: 15

valor -= 3;                       // Equivale a: valor = valor - 3
Console.WriteLine($"Después de -= 3: {valor}"); // Salida: 12

valor *= 2;                       // Equivale a: valor = valor * 2
Console.WriteLine($"Después de *= 2: {valor}"); // Salida: 24

valor /= 4;                       // Equivale a: valor = valor / 4
Console.WriteLine($"Después de /= 4: {valor}"); // Salida: 6

valor %= 4;                       // Equivale a: valor = valor % 4
Console.WriteLine($"Después de %= 4: {valor}"); // Salida: 2
```

#### Explicación de los Operadores de Asignación Compuesta:

**`+=` (Suma y asignación):**
- `valor += 5` es equivalente a `valor = valor + 5`
- Suma 5 al valor actual y asigna el resultado

**`-=` (Resta y asignación):**
- `valor -= 3` es equivalente a `valor = valor - 3`
- Resta 3 al valor actual y asigna el resultado

**`*=` (Multiplicación y asignación):**
- `valor *= 2` es equivalente a `valor = valor * 2`
- Multiplica el valor actual por 2 y asigna el resultado

**`/=` (División y asignación):**
- `valor /= 4` es equivalente a `valor = valor / 4`
- Divide el valor actual entre 4 y asigna el resultado

**`%=` (Módulo y asignación):**
- `valor %= 4` es equivalente a `valor = valor % 4`
- Calcula el resto de dividir el valor actual entre 4 y asigna el resultado

### 6. Precedencia de Operadores

La precedencia determina el orden en que se evalúan los operadores:

```csharp
int resultado = 2 + 3 * 4;        // ¿Cuál es el resultado?

Console.WriteLine($"2 + 3 * 4 = {resultado}"); // Salida: 14

// Con paréntesis para cambiar la precedencia
int resultadoConParentesis = (2 + 3) * 4;
Console.WriteLine($"(2 + 3) * 4 = {resultadoConParentesis}"); // Salida: 20
```

#### Explicación de la Precedencia:

**Sin paréntesis: `2 + 3 * 4`**
1. Se evalúa primero `3 * 4 = 12` (multiplicación tiene mayor precedencia)
2. Luego se evalúa `2 + 12 = 14`
3. Resultado: `14`

**Con paréntesis: `(2 + 3) * 4`**
1. Se evalúa primero `(2 + 3) = 5` (paréntesis tienen mayor precedencia)
2. Luego se evalúa `5 * 4 = 20`
3. Resultado: `20`

#### Orden de Precedencia (de mayor a menor):
1. **Paréntesis** `()`
2. **Incremento/Decremento** `++`, `--`
3. **Multiplicación, División, Módulo** `*`, `/`, `%`
4. **Suma, Resta** `+`, `-`
5. **Comparación** `<`, `>`, `<=`, `>=`
6. **Igualdad** `==`, `!=`
7. **AND lógico** `&&`
8. **OR lógico** `||`
9. **Asignación** `=`, `+=`, `-=`, etc.

### 7. Operador Ternario

El operador ternario es una forma abreviada de escribir una declaración if-else:

```csharp
int edad = 20;                    // Edad del usuario

// Forma tradicional con if-else
string mensaje;
if (edad >= 18)
{
    mensaje = "Eres mayor de edad";
}
else
{
    mensaje = "Eres menor de edad";
}

// Forma abreviada con operador ternario
string mensajeTernario = edad >= 18 ? "Eres mayor de edad" : "Eres menor de edad";

Console.WriteLine($"Edad: {edad}");
Console.WriteLine($"Mensaje tradicional: {mensaje}");
Console.WriteLine($"Mensaje ternario: {mensajeTernario}");

// Otro ejemplo
int numero = 7;
string parImpar = numero % 2 == 0 ? "par" : "impar";
Console.WriteLine($"El número {numero} es {parImpar}");
```

#### Explicación del Operador Ternario:

**Sintaxis:**
```csharp
condicion ? valorSiVerdadero : valorSiFalso
```

**Línea 18: `string mensajeTernario = edad >= 18 ? "Eres mayor de edad" : "Eres menor de edad";`**
1. `edad >= 18` evalúa a `true` (20 >= 18)
2. Como la condición es `true`, se asigna el primer valor
3. `mensajeTernario = "Eres mayor de edad"`

**Línea 24: `string parImpar = numero % 2 == 0 ? "par" : "impar";`**
1. `numero % 2 == 0` evalúa a `false` (7 % 2 = 1, 1 == 0 es false)
2. Como la condición es `false`, se asigna el segundo valor
3. `parImpar = "impar"`

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Calculadora Básica
```csharp
int numero1 = 15;
int numero2 = 4;

int suma = numero1 + numero2;
int resta = numero1 - numero2;
int multiplicacion = numero1 * numero2;
int division = numero1 / numero2;
int modulo = numero1 % numero2;

Console.WriteLine($"Calculadora para {numero1} y {numero2}:");
Console.WriteLine($"Suma: {numero1} + {numero2} = {suma}");
Console.WriteLine($"Resta: {numero1} - {numero2} = {resta}");
Console.WriteLine($"Multiplicación: {numero1} * {numero2} = {multiplicacion}");
Console.WriteLine($"División: {numero1} / {numero2} = {division}");
Console.WriteLine($"Módulo: {numero1} % {numero2} = {modulo}");
```

### Ejercicio 2: Comparaciones y Lógica
```csharp
int temperatura = 25;
bool esDia = true;
bool estaLloviendo = false;

bool esAgradable = temperatura >= 20 && temperatura <= 30;
bool necesitaParaguas = estaLloviendo || temperatura < 10;
bool esDiaAgradable = esDia && esAgradable;

Console.WriteLine($"Temperatura: {temperatura}°C");
Console.WriteLine($"¿Es día? {esDia}");
Console.WriteLine($"¿Está lloviendo? {estaLloviendo}");
Console.WriteLine($"¿Es temperatura agradable? {esAgradable}");
Console.WriteLine($"¿Necesita paraguas? {necesitaParaguas}");
Console.WriteLine($"¿Es un día agradable? {esDiaAgradable}");
```

### Ejercicio 3: Operadores de Asignación
```csharp
int contador = 0;
Console.WriteLine($"Contador inicial: {contador}");

contador += 5;                    // Incrementa en 5
Console.WriteLine($"Después de += 5: {contador}");

contador *= 2;                    // Multiplica por 2
Console.WriteLine($"Después de *= 2: {contador}");

contador -= 3;                    // Resta 3
Console.WriteLine($"Después de -= 3: {contador}");

contador /= 2;                    // Divide entre 2
Console.WriteLine($"Después de /= 2: {contador}");

contador %= 3;                    // Módulo 3
Console.WriteLine($"Después de %= 3: {contador}");
```

### Ejercicio 4: Operador Ternario
```csharp
int puntuacion = 85;
string calificacion = puntuacion >= 90 ? "A" :
                     puntuacion >= 80 ? "B" :
                     puntuacion >= 70 ? "C" :
                     puntuacion >= 60 ? "D" : "F";

Console.WriteLine($"Puntuación: {puntuacion}");
Console.WriteLine($"Calificación: {calificacion}");

// Otro ejemplo
int hora = 14;
string saludo = hora < 12 ? "Buenos días" : 
                hora < 18 ? "Buenas tardes" : "Buenas noches";

Console.WriteLine($"Hora: {hora}:00");
Console.WriteLine($"Saludo: {saludo}");
```

## 🔍 Conceptos Importantes a Recordar

1. **Los operadores aritméticos** realizan operaciones matemáticas básicas
2. **Los operadores de comparación** retornan valores booleanos
3. **Los operadores lógicos** combinan condiciones booleanas
4. **Los operadores de asignación** asignan valores a variables
5. **La precedencia de operadores** determina el orden de evaluación
6. **El operador ternario** es una forma abreviada de if-else
7. **Los operadores de incremento/decremento** pueden ser prefix o postfix
8. **Los paréntesis** pueden cambiar la precedencia de operadores

## ❓ Preguntas de Repaso

1. ¿Cuál es la diferencia entre `++numero` y `numero++`?
2. ¿Qué operador usarías para verificar si un número es par?
3. ¿Cuál es el resultado de `5 / 2` si ambas variables son `int`?
4. ¿Qué significa el operador `%` y cuándo lo usarías?
5. ¿Cómo funciona el operador ternario?

## 🚀 Siguiente Paso

En la próxima clase aprenderemos sobre **Estructuras de Control**, donde veremos cómo usar condicionales y bucles para controlar el flujo de nuestros programas.

---

## 📚 Recursos Adicionales

- [Operadores en C#](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/)
- [Precedencia de operadores](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/#operator-precedence)
- [Operadores lógicos](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/boolean-logical-operators/)

---

**¡Excelente! Ahora dominas los operadores y expresiones en C#! 🎯**
