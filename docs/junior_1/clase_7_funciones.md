# Clase 7: Funciones Básicas en C#

## 🎯 Objetivos de la Clase
- Comprender qué son los métodos y cómo se definen en C#
- Aprender a crear métodos con diferentes tipos de parámetros
- Entender los tipos de retorno y la palabra clave return
- Dominar el scope de variables y la organización del código

---

## 📚 Navegación del Módulo 1

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_introduccion.md) | Introducción a C# y .NET | |
| [Clase 2](clase_2_variables_tipos.md) | Variables y Tipos de Datos | |
| [Clase 3](clase_3_operadores.md) | Operadores y Expresiones | |
| [Clase 4](clase_4_estructuras_control.md) | Estructuras de Control | |
| [Clase 5](clase_5_colecciones.md) | Colecciones | |
| [Clase 6](clase_6_strings.md) | Manipulación de Strings | ← Anterior |
| **Clase 7** | **Métodos y Funciones** | ← Estás aquí |
| [Clase 8](clase_8_namespaces.md) | Namespaces y Organización | Siguiente → |
| [Clase 9](clase_9_manejo_errores.md) | Manejo de Errores | |
| [Clase 10](clase_10_poo_basica.md) | Programación Orientada a Objetos Básica | |

**← [Volver al README del Módulo 1](../junior_1/README.md)**

## 📚 Contenido Teórico

### 1. ¿Qué son los Métodos?

Los métodos son **bloques de código** que realizan una tarea específica y pueden ser llamados (invocados) desde otras partes del programa. Son fundamentales para organizar el código, evitar la repetición y crear programas más mantenibles.

#### Características de los Métodos:
- **Reutilizables**: Se pueden llamar múltiples veces
- **Organizados**: Agrupan código relacionado
- **Parametrizados**: Pueden recibir datos de entrada
- **Con retorno**: Pueden devolver resultados
- **Con nombre**: Tienen un identificador descriptivo

### 2. Estructura Básica de un Método

#### 2.1 Sintaxis de Declaración

```csharp
// Método básico sin parámetros y sin retorno
static void Saludar()
{
    Console.WriteLine("¡Hola Mundo!");
}

// Método con parámetros y retorno
static int Sumar(int a, int b)
{
    int resultado = a + b;
    return resultado;
}

// Método con parámetros pero sin retorno
static void MostrarInformacion(string nombre, int edad)
{
    Console.WriteLine($"Nombre: {nombre}");
    Console.WriteLine($"Edad: {edad}");
}

// Método con retorno pero sin parámetros
static string ObtenerSaludo()
{
    return "¡Buenos días!";
}

// Llamada a los métodos
Saludar();                           // Llama al método Saludar
int suma = Sumar(5, 3);             // Llama al método Sumar y guarda el resultado
MostrarInformacion("Juan", 25);      // Llama al método MostrarInformacion
string saludo = ObtenerSaludo();     // Llama al método ObtenerSaludo y guarda el resultado

Console.WriteLine($"Suma: {suma}");
Console.WriteLine($"Saludo: {saludo}");
```

#### Explicación Línea por Línea:

**Línea 2: `static void Saludar()`**
- `static` significa que el método pertenece a la clase, no a una instancia
- `void` indica que el método no retorna ningún valor
- `Saludar` es el nombre del método
- `()` indica que no recibe parámetros

**Línea 4: `Console.WriteLine("¡Hola Mundo!");`**
- Código que se ejecuta cuando se llama al método
- Imprime "¡Hola Mundo!" en la consola

**Línea 7: `static int Sumar(int a, int b)`**
- `static` método estático
- `int` indica que el método retorna un valor entero
- `Sumar` es el nombre del método
- `(int a, int b)` son los parámetros: dos enteros llamados `a` y `b`

**Línea 9: `return resultado;`**
- `return` termina la ejecución del método
- Devuelve el valor de `resultado` al código que llamó al método

**Línea 12: `static void MostrarInformacion(string nombre, int edad)`**
- `void` no retorna valor
- Recibe dos parámetros: `string nombre` e `int edad`

**Línea 17: `static string ObtenerSaludo()`**
- `string` indica que retorna un string
- No recibe parámetros

**Línea 25: `Saludar();`**
- Llama al método `Saludar`
- No necesita parámetros y no retorna valor

**Línea 26: `int suma = Sumar(5, 3);`**
- Llama al método `Sumar` con parámetros 5 y 3
- Guarda el valor retornado en la variable `suma`

#### 2.2 Partes de un Método

```csharp
// [modificadores] [tipo_retorno] [nombre_metodo]([parametros])
static int CalcularArea(int baseRectangulo, int alturaRectangulo)
{
    // Cuerpo del método
    int area = baseRectangulo * alturaRectangulo;
    
    // Instrucción de retorno
    return area;
}

// Llamada al método
int areaCalculada = CalcularArea(10, 5);
Console.WriteLine($"El área del rectángulo es: {areaCalculada}");
```

#### Explicación de las Partes:

**Modificadores de Acceso:**
- `static`: El método pertenece a la clase (no necesita instancia)
- `public`, `private`, `protected`: Controlan quién puede acceder al método

**Tipo de Retorno:**
- `int`, `string`, `double`, etc.: El método retorna un valor de ese tipo
- `void`: El método no retorna ningún valor

**Nombre del Método:**
- Debe ser descriptivo y seguir convenciones de nomenclatura
- Usar PascalCase (primera letra de cada palabra en mayúscula)

**Parámetros:**
- Lista de variables que recibe el método
- Cada parámetro tiene un tipo y un nombre
- Los parámetros son opcionales

**Cuerpo del Método:**
- Código que se ejecuta cuando se llama al método
- Puede contener variables locales, lógica, llamadas a otros métodos

**Instrucción Return:**
- Termina la ejecución del método
- Devuelve un valor (excepto en métodos void)

### 3. Tipos de Parámetros

#### 3.1 Parámetros por Valor

Los parámetros por valor reciben una copia del valor original:

```csharp
static void ModificarNumero(int numero)
{
    numero = numero * 2;             // Modifica la copia local
    Console.WriteLine($"Dentro del método: {numero}");
}

// Uso del método
int numeroOriginal = 10;
Console.WriteLine($"Antes de llamar al método: {numeroOriginal}");

ModificarNumero(numeroOriginal);     // Llama al método

Console.WriteLine($"Después de llamar al método: {numeroOriginal}");
```

#### Explicación de Parámetros por Valor:

**Línea 1: `static void ModificarNumero(int numero)`**
- `numero` es un parámetro por valor
- Recibe una copia del valor original

**Línea 3: `numero = numero * 2;`**
- Modifica la copia local del parámetro
- No afecta la variable original

**Línea 8: `ModificarNumero(numeroOriginal);`**
- Pasa el valor de `numeroOriginal` (10) al método
- El método recibe una copia del valor

**Resultado:**
- Antes: `numeroOriginal = 10`
- Dentro del método: `numero = 20` (copia modificada)
- Después: `numeroOriginal = 10` (original sin cambios)

#### 3.2 Parámetros por Referencia

Los parámetros por referencia permiten modificar la variable original:

```csharp
static void ModificarNumeroPorReferencia(ref int numero)
{
    numero = numero * 2;             // Modifica la variable original
    Console.WriteLine($"Dentro del método: {numero}");
}

// Uso del método
int numeroOriginal = 10;
Console.WriteLine($"Antes de llamar al método: {numeroOriginal}");

ModificarNumeroPorReferencia(ref numeroOriginal); // Llama al método con ref

Console.WriteLine($"Después de llamar al método: {numeroOriginal}");
```

#### Explicación de Parámetros por Referencia:

**Línea 1: `static void ModificarNumeroPorReferencia(ref int numero)`**
- `ref int numero` indica que es un parámetro por referencia
- El método puede modificar la variable original

**Línea 8: `ModificarNumeroPorReferencia(ref numeroOriginal);`**
- `ref` indica que se pasa la referencia a la variable
- El método puede modificar `numeroOriginal` directamente

**Resultado:**
- Antes: `numeroOriginal = 10`
- Dentro del método: `numero = 20` (modifica la original)
- Después: `numeroOriginal = 20` (original modificada)

#### 3.3 Parámetros de Salida (out)

Los parámetros `out` permiten que el método devuelva múltiples valores:

```csharp
static void DividirConResto(int dividendo, int divisor, out int cociente, out int resto)
{
    cociente = dividendo / divisor;  // Asigna valor al parámetro out
    resto = dividendo % divisor;     // Asigna valor al parámetro out
}

// Uso del método
int numero1 = 17;
int numero2 = 5;
int cociente, resto;                 // Variables para recibir los resultados

DividirConResto(numero1, numero2, out cociente, out resto);

Console.WriteLine($"{numero1} ÷ {numero2} = {cociente} con resto {resto}");
```

#### Explicación de Parámetros out:

**Línea 1: `static void DividirConResto(int dividendo, int divisor, out int cociente, out int resto)`**
- `out int cociente` y `out int resto` son parámetros de salida
- El método debe asignar valores a estos parámetros

**Línea 3-4: Asignación de valores**
- `cociente = dividendo / divisor;` asigna el resultado de la división
- `resto = dividendo % divisor;` asigna el resto de la división

**Línea 9: `int cociente, resto;`**
- Declara variables para recibir los valores de salida
- No es necesario inicializarlas

**Línea 11: `DividirConResto(numero1, numero2, out cociente, out resto);`**
- `out` indica que estas variables recibirán valores del método

**Resultado:**
- `cociente = 17 ÷ 5 = 3`
- `resto = 17 % 5 = 2`

#### 3.4 Parámetros Opcionales

Los parámetros opcionales tienen valores por defecto:

```csharp
static void SaludarPersona(string nombre, string saludo = "Hola", bool usarExclamacion = true)
{
    string mensaje = $"{saludo}, {nombre}";
    
    if (usarExclamacion)
    {
        mensaje += "!";
    }
    
    Console.WriteLine(mensaje);
}

// Diferentes formas de llamar al método
SaludarPersona("Juan");                              // Usa valores por defecto
SaludarPersona("María", "Buenos días");              // Cambia saludo, usa exclamación por defecto
SaludarPersona("Carlos", "Buenas tardes", false);    // Cambia ambos parámetros
SaludarPersona("Ana", usarExclamacion: false);       // Usa parámetro nombrado
```

#### Explicación de Parámetros Opcionales:

**Línea 1: `static void SaludarPersona(string nombre, string saludo = "Hola", bool usarExclamacion = true)`**
- `nombre` es obligatorio (no tiene valor por defecto)
- `saludo = "Hola"` tiene valor por defecto "Hola"
- `usarExclamacion = true` tiene valor por defecto true

**Línea 8: `SaludarPersona("Juan");`**
- Solo proporciona el parámetro obligatorio
- `saludo` será "Hola" y `usarExclamacion` será true

**Línea 9: `SaludarPersona("María", "Buenos días");`**
- Proporciona nombre y saludo
- `usarExclamacion` mantiene su valor por defecto (true)

**Línea 10: `SaludarPersona("Carlos", "Buenas tardes", false);`**
- Proporciona todos los parámetros
- Ignora los valores por defecto

**Línea 11: `SaludarPersona("Ana", usarExclamacion: false);`**
- Usa parámetros nombrados para especificar solo algunos valores
- `saludo` mantiene su valor por defecto "Hola"

### 4. Tipos de Retorno

#### 4.1 Métodos con Retorno

```csharp
// Método que retorna un entero
static int CalcularCuadrado(int numero)
{
    return numero * numero;
}

// Método que retorna un string
static string ObtenerEstado(int puntuacion)
{
    if (puntuacion >= 90)
        return "Excelente";
    else if (puntuacion >= 80)
        return "Bueno";
    else if (puntuacion >= 70)
        return "Satisfactorio";
    else
        return "Necesita mejorar";
}

// Método que retorna un double
static double CalcularPromedio(int[] numeros)
{
    if (numeros.Length == 0)
        return 0.0;
    
    int suma = 0;
    for (int i = 0; i < numeros.Length; i++)
    {
        suma += numeros[i];
    }
    
    return (double)suma / numeros.Length;
}

// Método que retorna un bool
static bool EsNumeroPar(int numero)
{
    return numero % 2 == 0;
}

// Uso de los métodos
int cuadrado = CalcularCuadrado(5);
string estado = ObtenerEstado(85);
double promedio = CalcularPromedio(new int[] { 10, 20, 30, 40, 50 });
bool esPar = EsNumeroPar(7);

Console.WriteLine($"Cuadrado de 5: {cuadrado}");
Console.WriteLine($"Estado con 85 puntos: {estado}");
Console.WriteLine($"Promedio: {promedio}");
Console.WriteLine($"¿7 es par? {esPar}");
```

#### Explicación de Métodos con Retorno:

**Línea 2: `static int CalcularCuadrado(int numero)`**
- `int` indica que retorna un entero
- `return numero * numero;` devuelve el cuadrado del número

**Línea 8: `static string ObtenerEstado(int puntuacion)`**
- `string` indica que retorna un string
- Múltiples `return` statements según la condición

**Línea 20: `static double CalcularPromedio(int[] numeros)`**
- `double` indica que retorna un decimal
- Maneja el caso especial de array vacío

**Línea 32: `static bool EsNumeroPar(int numero)`**
- `bool` indica que retorna true o false
- Retorna el resultado de la expresión booleana

#### 4.2 Métodos void (sin retorno)

```csharp
// Método que no retorna valor
static void MostrarMenu()
{
    Console.WriteLine("=== MENÚ PRINCIPAL ===");
    Console.WriteLine("1. Opción A");
    Console.WriteLine("2. Opción B");
    Console.WriteLine("3. Opción C");
    Console.WriteLine("4. Salir");
    Console.WriteLine("=====================");
}

// Método que procesa datos pero no retorna
static void ProcesarDatos(string[] nombres)
{
    Console.WriteLine("Procesando nombres...");
    
    for (int i = 0; i < nombres.Length; i++)
    {
        Console.WriteLine($"Procesando: {nombres[i]}");
        // Aquí iría la lógica de procesamiento
    }
    
    Console.WriteLine("Procesamiento completado.");
}

// Método que modifica parámetros por referencia
static void InicializarArray(int[] numeros, int valor)
{
    for (int i = 0; i < numeros.Length; i++)
    {
        numeros[i] = valor;
    }
}

// Uso de los métodos
MostrarMenu();

string[] nombres = { "Juan", "María", "Carlos" };
ProcesarDatos(nombres);

int[] array = new int[5];
InicializarArray(array, 0);
Console.WriteLine($"Array inicializado: [{string.Join(", ", array)}]");
```

#### Explicación de Métodos void:

**Línea 2: `static void MostrarMenu()`**
- `void` indica que no retorna ningún valor
- Solo ejecuta código (muestra el menú)

**Línea 15: `static void ProcesarDatos(string[] nombres)`**
- Procesa los datos pero no retorna resultado
- Muestra información en consola

**Línea 26: `static void InicializarArray(int[] numeros, int valor)`**
- Modifica el array pasado como parámetro
- No necesita retornar nada porque modifica directamente

### 5. Scope de Variables

#### 5.1 Variables Locales

```csharp
static void EjemploScope()
{
    int variableLocal = 10;          // Variable local del método
    Console.WriteLine($"Variable local: {variableLocal}");
    
    if (variableLocal > 5)
    {
        int variableBloque = 20;     // Variable local del bloque if
        Console.WriteLine($"Variable del bloque: {variableBloque}");
        Console.WriteLine($"Variable local: {variableLocal}"); // Accesible
    }
    
    // Console.WriteLine(variableBloque); // ❌ Error: variableBloque no es accesible aquí
    Console.WriteLine($"Variable local: {variableLocal}"); // ✅ Accesible
}

// Console.WriteLine(variableLocal); // ❌ Error: variableLocal no es accesible aquí
```

#### Explicación del Scope:

**Línea 3: `int variableLocal = 10;`**
- Variable local del método `EjemploScope`
- Solo accesible dentro del método

**Línea 8: `int variableBloque = 20;`**
- Variable local del bloque `if`
- Solo accesible dentro del bloque `if`

**Línea 11: `Console.WriteLine(variableBloque);`**
- ✅ Accesible porque está dentro del bloque `if`

**Línea 15: `Console.WriteLine(variableBloque);`**
- ❌ Error porque `variableBloque` no es accesible fuera del bloque `if`

#### 5.2 Variables de Clase (Campos)

```csharp
class Calculadora
{
    // Variables de clase (campos)
    private int resultado;
    private string operacion;
    
    // Método que usa las variables de clase
    public void RealizarOperacion(int a, int b, string operador)
    {
        operacion = $"{a} {operador} {b}";
        
        switch (operador)
        {
            case "+":
                resultado = a + b;
                break;
            case "-":
                resultado = a - b;
                break;
            case "*":
                resultado = a * b;
                break;
            case "/":
                resultado = a / b;
                break;
        }
    }
    
    // Método que muestra el resultado
    public void MostrarResultado()
    {
        Console.WriteLine($"{operacion} = {resultado}");
    }
}

// Uso de la clase
Calculadora calc = new Calculadora();
calc.RealizarOperacion(10, 5, "+");
calc.MostrarResultado();

calc.RealizarOperacion(20, 4, "*");
calc.MostrarResultado();
```

#### Explicación de Variables de Clase:

**Línea 3-4: Variables de clase**
- `resultado` y `operacion` son accesibles desde todos los métodos de la clase
- Mantienen su valor entre llamadas a métodos

**Línea 7: `public void RealizarOperacion(int a, int b, string operador)`**
- Modifica las variables de clase `operacion` y `resultado`
- No necesita retornar nada porque modifica el estado de la clase

**Línea 32: `calc.RealizarOperacion(10, 5, "+");`**
- Llama al método y modifica el estado interno de la clase
- `operacion` se convierte en "10 + 5"
- `resultado` se convierte en 15

**Línea 33: `calc.MostrarResultado();`**
- Muestra el resultado de la operación anterior
- Usa las variables de clase modificadas

### 6. Sobrecarga de Métodos

La sobrecarga permite tener múltiples métodos con el mismo nombre pero diferentes parámetros:

```csharp
static int Sumar(int a, int b)
{
    return a + b;
}

static int Sumar(int a, int b, int c)
{
    return a + b + c;
}

static double Sumar(double a, double b)
{
    return a + b;
}

static string Sumar(string a, string b)
{
    return a + b;
}

// Uso de métodos sobrecargados
int suma1 = Sumar(5, 3);                    // Llama al primer método
int suma2 = Sumar(1, 2, 3);                 // Llama al segundo método
double suma3 = Sumar(3.5, 2.5);             // Llama al tercer método
string suma4 = Sumar("Hola ", "Mundo");     // Llama al cuarto método

Console.WriteLine($"Suma de 2 enteros: {suma1}");
Console.WriteLine($"Suma de 3 enteros: {suma2}");
Console.WriteLine($"Suma de 2 doubles: {suma3}");
Console.WriteLine($"Concatenación de strings: {suma4}");
```

#### Explicación de la Sobrecarga:

**Diferentes versiones del método Sumar:**
1. **`Sumar(int a, int b)`**: Suma dos enteros
2. **`Sumar(int a, int b, int c)`**: Suma tres enteros
3. **`Sumar(double a, double b)`**: Suma dos doubles
4. **`Sumar(string a, string b)`**: Concatena dos strings

**C# determina qué método llamar basándose en:**
- El número de parámetros
- Los tipos de los parámetros
- El orden de los parámetros

**Línea 25: `Sumar(5, 3)`**
- C# ve dos parámetros int
- Llama al primer método: `Sumar(int a, int b)`

**Línea 26: `Sumar(1, 2, 3)`**
- C# ve tres parámetros int
- Llama al segundo método: `Sumar(int a, int b, int c)`

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Métodos Básicos
```csharp
// Método para calcular el área de un círculo
static double CalcularAreaCirculo(double radio)
{
    const double PI = 3.14159;
    return PI * radio * radio;
}

// Método para verificar si un número es primo
static bool EsPrimo(int numero)
{
    if (numero < 2) return false;
    
    for (int i = 2; i <= Math.Sqrt(numero); i++)
    {
        if (numero % i == 0) return false;
    }
    return true;
}

// Método para mostrar información de una persona
static void MostrarPersona(string nombre, int edad, string ciudad)
{
    Console.WriteLine($"Nombre: {nombre}");
    Console.WriteLine($"Edad: {edad}");
    Console.WriteLine($"Ciudad: {ciudad}");
}

// Uso de los métodos
double area = CalcularAreaCirculo(5.0);
Console.WriteLine($"Área del círculo: {area:F2}");

bool esPrimo = EsPrimo(17);
Console.WriteLine($"¿17 es primo? {esPrimo}");

MostrarPersona("Ana", 28, "Madrid");
```

### Ejercicio 2: Métodos con Parámetros de Referencia
```csharp
// Método que intercambia dos valores
static void Intercambiar(ref int a, ref int b)
{
    int temporal = a;
    a = b;
    b = temporal;
}

// Método que encuentra máximo y mínimo
static void EncontrarMaxMin(int[] numeros, out int maximo, out int minimo)
{
    if (numeros.Length == 0)
    {
        maximo = 0;
        minimo = 0;
        return;
    }
    
    maximo = numeros[0];
    minimo = numeros[0];
    
    for (int i = 1; i < numeros.Length; i++)
    {
        if (numeros[i] > maximo) maximo = numeros[i];
        if (numeros[i] < minimo) minimo = numeros[i];
    }
}

// Uso de los métodos
int x = 10, y = 20;
Console.WriteLine($"Antes: x = {x}, y = {y}");

Intercambiar(ref x, ref y);
Console.WriteLine($"Después: x = {x}, y = {y}");

int[] array = { 15, 8, 23, 7, 42, 19, 3 };
int max, min;

EncontrarMaxMin(array, out max, out min);
Console.WriteLine($"Máximo: {max}, Mínimo: {min}");
```

### Ejercicio 3: Métodos Sobrecargados
```csharp
// Diferentes versiones del método Calcular
static int Calcular(int a, int b)
{
    return a + b;
}

static int Calcular(int a, int b, int c)
{
    return a + b + c;
}

static double Calcular(double a, double b)
{
    return a * b;
}

static string Calcular(string texto, int veces)
{
    string resultado = "";
    for (int i = 0; i < veces; i++)
    {
        resultado += texto;
    }
    return resultado;
}

// Uso de métodos sobrecargados
Console.WriteLine($"Suma de 2 enteros: {Calcular(5, 3)}");
Console.WriteLine($"Suma de 3 enteros: {Calcular(1, 2, 3)}");
Console.WriteLine($"Multiplicación de doubles: {Calcular(3.5, 2.0)}");
Console.WriteLine($"Texto repetido: {Calcular("Hola ", 3)}");
```

### Ejercicio 4: Métodos con Parámetros Opcionales
```csharp
// Método con parámetros opcionales
static void CrearMensaje(string nombre, string saludo = "Hola", bool formal = false, string despedida = "Adiós")
{
    string mensaje = "";
    
    if (formal)
    {
        mensaje = $"{saludo}, estimado/a {nombre}. {despedida}.";
    }
    else
    {
        mensaje = $"{saludo} {nombre}! {despedida}";
    }
    
    Console.WriteLine(mensaje);
}

// Diferentes formas de llamar al método
CrearMensaje("Juan");
CrearMensaje("María", "Buenos días");
CrearMensaje("Carlos", "Buenas tardes", true);
CrearMensaje("Ana", despedida: "Hasta luego");
CrearMensaje("Pedro", "Saludos", true, "Que tengas un buen día");
```

## 🔍 Conceptos Importantes a Recordar

1. **Los métodos organizan el código** en bloques reutilizables
2. **Los parámetros por valor** reciben copias, no modifican el original
3. **Los parámetros por referencia** (`ref`) permiten modificar variables originales
4. **Los parámetros `out`** permiten retornar múltiples valores
5. **Los parámetros opcionales** tienen valores por defecto
6. **El scope de variables** determina dónde son accesibles
7. **La sobrecarga de métodos** permite múltiples versiones con el mismo nombre
8. **Los métodos `void`** no retornan valores
9. **Los métodos estáticos** pertenecen a la clase, no a instancias

## ❓ Preguntas de Repaso

1. ¿Cuál es la diferencia entre parámetros por valor y por referencia?
2. ¿Qué significa `void` en la declaración de un método?
3. ¿Cuándo usarías parámetros `out`?
4. ¿Qué es la sobrecarga de métodos?
5. ¿Cómo funciona el scope de variables en C#?

## 🚀 Siguiente Paso

En la próxima clase aprenderemos sobre **Namespaces y Organización**, donde veremos cómo organizar nuestro código en C#.

---

## 📚 Recursos Adicionales

- [Métodos en C#](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/methods/)
- [Parámetros de métodos](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/passing-parameters/)
- [Sobrecarga de métodos](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/methods#method-overloading)

---

**¡Excelente! Ahora dominas las funciones básicas en C#! 🎯**
