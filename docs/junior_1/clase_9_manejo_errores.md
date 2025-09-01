# Clase 9: Manejo de Errores Básico en C#

## 🎯 Objetivos de la Clase
- Comprender qué son las excepciones y por qué son importantes
- Aprender a usar try-catch para manejar errores
- Entender diferentes tipos de excepciones comunes
- Dominar el uso de finally y throw para manejo robusto de errores

## 📚 Contenido Teórico

### 1. ¿Qué son las Excepciones?

Las excepciones son **eventos inesperados** que ocurren durante la ejecución de un programa y que pueden interrumpir el flujo normal. En C#, las excepciones son objetos que contienen información sobre el error, incluyendo el tipo de error, un mensaje descriptivo y la pila de llamadas.

#### ¿Por qué son Importantes las Excepciones?
- **Robustez**: Permiten que el programa continúe ejecutándose después de un error
- **Debugging**: Proporcionan información detallada sobre qué salió mal
- **Recuperación**: Permiten implementar lógica para manejar situaciones de error
- **Logging**: Facilitan el registro de errores para análisis posterior

### 2. Tipos de Excepciones Comunes

#### 2.1 Excepciones del Sistema

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        // Ejemplos de excepciones comunes
        Console.WriteLine("=== EJEMPLOS DE EXCEPCIONES ===");
        
        // 1. DivideByZeroException - División por cero
        try
        {
            int resultado = 10 / 0;  // Esto causará una excepción
            Console.WriteLine($"Resultado: {resultado}");
        }
        catch (DivideByZeroException ex)
        {
            Console.WriteLine($"Error de división por cero: {ex.Message}");
        }
        
        // 2. IndexOutOfRangeException - Índice fuera de rango
        try
        {
            int[] numeros = { 1, 2, 3, 4, 5 };
            int elemento = numeros[10];  // Índice fuera de rango
            Console.WriteLine($"Elemento: {elemento}");
        }
        catch (IndexOutOfRangeException ex)
        {
            Console.WriteLine($"Error de índice fuera de rango: {ex.Message}");
        }
        
        // 3. NullReferenceException - Referencia nula
        try
        {
            string texto = null;
            int longitud = texto.Length;  // Acceso a propiedad de objeto nulo
            Console.WriteLine($"Longitud: {longitud}");
        }
        catch (NullReferenceException ex)
        {
            Console.WriteLine($"Error de referencia nula: {ex.Message}");
        }
        
        // 4. FormatException - Formato inválido
        try
        {
            string numeroTexto = "abc";
            int numero = int.Parse(numeroTexto);  // Conversión inválida
            Console.WriteLine($"Número: {numero}");
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"Error de formato: {ex.Message}");
        }
        
        // 5. ArgumentException - Argumento inválido
        try
        {
            string texto = "Hola Mundo";
            string subcadena = texto.Substring(5, 20);  // Longitud inválida
            Console.WriteLine($"Subcadena: {subcadena}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error de argumento: {ex.Message}");
        }
    }
}
```

#### Explicación de las Excepciones del Sistema:

**Línea 12: `int resultado = 10 / 0;`**
- Intenta dividir 10 por 0
- Esto causa una `DivideByZeroException`
- El programa se detendría sin el try-catch

**Línea 15: `catch (DivideByZeroException ex)`**
- Captura específicamente la excepción de división por cero
- `ex` es el objeto de excepción que contiene información del error

**Línea 16: `Console.WriteLine($"Error de división por cero: {ex.Message}");`**
- `ex.Message` contiene el mensaje descriptivo del error
- Permite mostrar información útil al usuario

**Línea 23: `int elemento = numeros[10];`**
- Intenta acceder al elemento en la posición 10
- El array solo tiene 5 elementos (índices 0-4)
- Causa `IndexOutOfRangeException`

**Línea 30: `int longitud = texto.Length;`**
- `texto` es `null`
- Acceder a la propiedad `Length` de un objeto nulo causa `NullReferenceException`

**Línea 37: `int numero = int.Parse(numeroTexto);`**
- `int.Parse()` espera un string que represente un número
- "abc" no es un número válido, causa `FormatException`

**Línea 44: `string subcadena = texto.Substring(5, 20);`**
- `Substring(5, 20)` intenta extraer 20 caracteres desde la posición 5
- El string solo tiene 10 caracteres, causa `ArgumentException`

### 3. Estructura try-catch Básica

#### 3.1 Sintaxis Básica

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== MANEJO BÁSICO DE EXCEPCIONES ===");
        
        // Ejemplo 1: Captura de excepción específica
        try
        {
            Console.Write("Ingrese un número: ");
            string entrada = Console.ReadLine();
            int numero = int.Parse(entrada);
            
            Console.WriteLine($"El número ingresado es: {numero}");
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"Error: Debe ingresar un número válido. {ex.Message}");
        }
        
        // Ejemplo 2: Captura de múltiples tipos de excepción
        try
        {
            Console.Write("Ingrese el primer número: ");
            string entrada1 = Console.ReadLine();
            int numero1 = int.Parse(entrada1);
            
            Console.Write("Ingrese el segundo número: ");
            string entrada2 = Console.ReadLine();
            int numero2 = int.Parse(entrada2);
            
            if (numero2 == 0)
            {
                throw new DivideByZeroException("El segundo número no puede ser cero");
            }
            
            double resultado = (double)numero1 / numero2;
            Console.WriteLine($"Resultado de la división: {resultado:F2}");
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"Error de formato: {ex.Message}");
        }
        catch (DivideByZeroException ex)
        {
            Console.WriteLine($"Error de división: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inesperado: {ex.Message}");
        }
        
        // Ejemplo 3: Captura de cualquier excepción
        try
        {
            Console.Write("Ingrese un texto: ");
            string texto = Console.ReadLine();
            
            if (string.IsNullOrEmpty(texto))
            {
                throw new ArgumentException("El texto no puede estar vacío");
            }
            
            Console.WriteLine($"Texto ingresado: {texto}");
            Console.WriteLine($"Longitud: {texto.Length}");
            Console.WriteLine($"Primer carácter: {texto[0]}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Tipo de excepción: {ex.GetType().Name}");
        }
        
        Console.WriteLine("\nPrograma continuó ejecutándose después de los errores.");
    }
}
```

#### Explicación de la Estructura try-catch:

**Línea 12: `try`**
- Inicia un bloque de código que puede generar excepciones
- Si no hay excepciones, el código se ejecuta normalmente

**Línea 18: `catch (FormatException ex)`**
- Captura específicamente excepciones de tipo `FormatException`
- `ex` es la variable que contiene la información de la excepción
- Solo se ejecuta si ocurre ese tipo específico de excepción

**Línea 25: `throw new DivideByZeroException("El segundo número no puede ser cero");`**
- `throw` lanza manualmente una excepción
- Útil para validaciones personalizadas
- El mensaje describe el problema específico

**Línea 35: `catch (Exception ex)`**
- `Exception` es la clase base de todas las excepciones
- Captura cualquier excepción que no haya sido capturada por los catch anteriores
- Debe ir al final de la cadena de catch

**Línea 52: `catch (Exception ex)`**
- Captura cualquier tipo de excepción
- `ex.GetType().Name` obtiene el nombre del tipo de excepción
- Útil para debugging y logging

#### 3.2 Captura de Excepciones Específicas vs. Generales

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== CAPTURA ESPECÍFICA VS GENERAL ===");
        
        // Ejemplo 1: Captura específica (RECOMENDADO)
        try
        {
            string[] frutas = { "Manzana", "Banana", "Naranja" };
            Console.Write("Ingrese el índice de la fruta (0-2): ");
            string entrada = Console.ReadLine();
            int indice = int.Parse(entrada);
            
            string fruta = frutas[indice];
            Console.WriteLine($"Fruta seleccionada: {fruta}");
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"Error de formato: Debe ingresar un número. {ex.Message}");
        }
        catch (IndexOutOfRangeException ex)
        {
            Console.WriteLine($"Error de índice: Debe ser entre 0 y 2. {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inesperado: {ex.Message}");
        }
        
        // Ejemplo 2: Captura general (NO RECOMENDADO para casos específicos)
        try
        {
            string[] colores = { "Rojo", "Verde", "Azul" };
            Console.Write("Ingrese el índice del color (0-2): ");
            string entrada2 = Console.ReadLine();
            int indice2 = int.Parse(entrada2);
            
            string color = colores[indice2];
            Console.WriteLine($"Color seleccionado: {color}");
        }
        catch (Exception ex)
        {
            // Captura general - menos informativo
            Console.WriteLine($"Error: {ex.Message}");
        }
        
        // Ejemplo 3: Manejo de excepciones con información adicional
        try
        {
            Console.Write("Ingrese su edad: ");
            string entradaEdad = Console.ReadLine();
            int edad = int.Parse(entradaEdad);
            
            if (edad < 0)
            {
                throw new ArgumentOutOfRangeException("edad", edad, "La edad no puede ser negativa");
            }
            if (edad > 150)
            {
                throw new ArgumentOutOfRangeException("edad", edad, "La edad no puede ser mayor a 150");
            }
            
            Console.WriteLine($"Edad válida: {edad} años");
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"Error de formato: Debe ingresar un número válido. {ex.Message}");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Console.WriteLine($"Error de rango: {ex.Message}");
            Console.WriteLine($"Valor ingresado: {ex.ActualValue}");
            Console.WriteLine($"Parámetro: {ex.ParamName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inesperado: {ex.Message}");
            Console.WriteLine($"Tipo: {ex.GetType().Name}");
        }
    }
}
```

#### Explicación de Captura Específica vs. General:

**Captura Específica (Recomendada):**
- **Línea 20: `catch (FormatException ex)`**
  - Captura solo errores de formato
  - Permite mensajes de error específicos y útiles
  - El usuario sabe exactamente qué hacer

- **Línea 23: `catch (IndexOutOfRangeException ex)`**
  - Captura solo errores de índice fuera de rango
  - Proporciona información específica sobre el rango válido

**Captura General (No recomendada para casos específicos):**
- **Línea 42: `catch (Exception ex)`**
  - Captura cualquier tipo de excepción
  - Menos informativo para el usuario
  - Puede ocultar errores específicos que deberían manejarse de manera diferente

**Excepción Personalizada con Información Adicional:**
- **Línea 58: `throw new ArgumentOutOfRangeException("edad", edad, "La edad no puede ser negativa");`**
  - Lanza una excepción con información adicional
  - `"edad"` es el nombre del parámetro
  - `edad` es el valor actual que causó el error
  - El tercer parámetro es el mensaje descriptivo

- **Línea 67: `catch (ArgumentOutOfRangeException ex)`**
  - Captura la excepción personalizada
  - Accede a propiedades adicionales como `ActualValue` y `ParamName`

### 4. Bloque finally

#### 4.1 Uso Básico de finally

```csharp
using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== USO DEL BLOQUE FINALLY ===");
        
        // Ejemplo 1: finally básico
        try
        {
            Console.WriteLine("Ejecutando código que puede fallar...");
            int numero = int.Parse("abc");  // Esto causará una excepción
            Console.WriteLine($"Número: {numero}");
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"Error capturado: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Este código SIEMPRE se ejecuta, sin importar si hubo error o no.");
        }
        
        // Ejemplo 2: finally con manejo de archivos
        StreamWriter writer = null;
        try
        {
            writer = new StreamWriter("archivo.txt");
            writer.WriteLine("Línea 1 del archivo");
            writer.WriteLine("Línea 2 del archivo");
            
            // Simular un error
            throw new Exception("Error simulado durante la escritura");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error durante la escritura: {ex.Message}");
        }
        finally
        {
            // Garantizar que el archivo se cierre
            if (writer != null)
            {
                writer.Close();
                Console.WriteLine("Archivo cerrado correctamente en el bloque finally.");
            }
        }
        
        // Ejemplo 3: finally con return
        Console.WriteLine($"\nResultado de la función: {FuncionConFinally()}");
    }
    
    static int FuncionConFinally()
    {
        try
        {
            Console.WriteLine("Ejecutando función...");
            return 42;  // Valor de retorno
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en función: {ex.Message}");
            return -1;  // Valor de retorno en caso de error
        }
        finally
        {
            Console.WriteLine("Finally se ejecuta ANTES del return.");
        }
    }
}
```

#### Explicación del Bloque finally:

**Línea 20: `finally`**
- El bloque `finally` SIEMPRE se ejecuta
- Se ejecuta después del `try` y `catch` (si existe)
- Se ejecuta incluso si hay un `return` en el `try` o `catch`

**Línea 30: `StreamWriter writer = null;`**
- Declara la variable fuera del try para que sea accesible en finally
- Se inicializa en `null` para poder verificar si se creó correctamente

**Línea 40: `if (writer != null)`**
- Verifica si el writer se creó antes de intentar cerrarlo
- Previene errores adicionales si la creación del writer falló

**Línea 42: `writer.Close();`**
- Cierra el archivo para liberar recursos
- Es crucial para evitar pérdida de datos y liberar memoria

**Línea 55: `return 42;`**
- Aunque hay un return, el bloque `finally` se ejecuta ANTES
- El orden es: `try` → `finally` → `return`

**Importancia del finally:**
- **Limpieza de recursos**: Garantiza que archivos, conexiones, etc. se cierren
- **Código de limpieza**: Se ejecuta sin importar si hubo errores
- **Orden de ejecución**: Siempre se ejecuta antes de cualquier return

#### 4.2 Using Statement vs. try-finally

```csharp
using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== USING STATEMENT VS TRY-FINALLY ===");
        
        // Ejemplo 1: Using statement (RECOMENDADO)
        Console.WriteLine("Usando using statement:");
        try
        {
            using (StreamWriter writer = new StreamWriter("archivo1.txt"))
            {
                writer.WriteLine("Primera línea");
                writer.WriteLine("Segunda línea");
                writer.WriteLine("Tercera línea");
                
                Console.WriteLine("Archivo escrito correctamente");
            } // El archivo se cierra automáticamente aquí
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        
        // Ejemplo 2: try-finally manual
        Console.WriteLine("\nUsando try-finally manual:");
        StreamWriter writer2 = null;
        try
        {
            writer2 = new StreamWriter("archivo2.txt");
            writer2.WriteLine("Línea A");
            writer2.WriteLine("Línea B");
            writer2.WriteLine("Línea C");
            
            Console.WriteLine("Archivo escrito correctamente");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            if (writer2 != null)
            {
                writer2.Close();
                Console.WriteLine("Archivo cerrado manualmente");
            }
        }
        
        // Ejemplo 3: Múltiples recursos con using
        Console.WriteLine("\nMúltiples recursos con using:");
        try
        {
            using (StreamWriter writer3 = new StreamWriter("archivo3.txt"))
            using (StreamWriter writer4 = new StreamWriter("archivo4.txt"))
            {
                writer3.WriteLine("Contenido del archivo 3");
                writer4.WriteLine("Contenido del archivo 4");
                
                Console.WriteLine("Dos archivos escritos correctamente");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        
        // Verificar archivos creados
        Console.WriteLine("\n=== ARCHIVOS CREADOS ===");
        string[] archivos = Directory.GetFiles(".", "archivo*.txt");
        foreach (string archivo in archivos)
        {
            FileInfo info = new FileInfo(archivo);
            Console.WriteLine($"- {info.Name}: {info.Length} bytes");
        }
    }
}
```

#### Explicación de Using Statement vs. try-finally:

**Using Statement (Recomendado):**
- **Línea 15: `using (StreamWriter writer = new StreamWriter("archivo1.txt"))`**
  - Crea el recurso y garantiza que se disponga automáticamente
  - Equivalente a try-finally con Dispose()
  - Más limpio y legible

- **Línea 22: `}` // El archivo se cierra automáticamente aquí**
  - Al salir del bloque using, se llama automáticamente a `writer.Dispose()`
  - No necesitas manejar manualmente el cierre

**Try-finally Manual:**
- **Línea 30: `StreamWriter writer2 = null;`**
  - Declaración manual de la variable
  - Necesitas verificar si es null antes de cerrar

- **Línea 45: `writer2.Close();`**
  - Cierre manual del archivo
  - Más propenso a errores si se olvida

**Múltiples Recursos con Using:**
- **Línea 58-59: Múltiples using anidados**
  - Permite manejar múltiples recursos en un solo bloque
  - Todos se disponen automáticamente al salir del bloque

**Ventajas del Using Statement:**
- **Automático**: No necesitas recordar cerrar recursos
- **Seguro**: Garantiza disposición incluso con excepciones
- **Legible**: El código es más claro sobre el ciclo de vida del recurso
- **Menos código**: No necesitas try-finally manual

### 5. Lanzamiento de Excepciones (throw)

#### 5.1 Lanzamiento Básico

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== LANZAMIENTO DE EXCEPCIONES ===");
        
        // Ejemplo 1: Lanzamiento básico
        try
        {
            Console.Write("Ingrese su edad: ");
            string entrada = Console.ReadLine();
            ValidarEdad(entrada);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error de validación: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inesperado: {ex.Message}");
        }
        
        // Ejemplo 2: Lanzamiento con información adicional
        try
        {
            Console.Write("Ingrese un número entre 1 y 100: ");
            string entradaNumero = Console.ReadLine();
            int numero = ValidarNumero(entradaNumero, 1, 100);
            Console.WriteLine($"Número válido: {numero}");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Console.WriteLine($"Error de rango: {ex.Message}");
            Console.WriteLine($"Valor ingresado: {ex.ActualValue}");
            Console.WriteLine($"Rango válido: {ex.ParamName}");
        }
        
        // Ejemplo 3: Lanzamiento de excepción personalizada
        try
        {
            Console.Write("Ingrese su nombre: ");
            string nombre = Console.ReadLine();
            ValidarNombre(nombre);
        }
        catch (NombreInvalidoException ex)
        {
            Console.WriteLine($"Error de nombre: {ex.Message}");
            Console.WriteLine($"Nombre ingresado: {ex.NombreIngresado}");
        }
    }
    
    // Método que lanza ArgumentException
    static void ValidarEdad(string entrada)
    {
        if (string.IsNullOrEmpty(entrada))
        {
            throw new ArgumentException("La edad no puede estar vacía", "edad");
        }
        
        if (!int.TryParse(entrada, out int edad))
        {
            throw new ArgumentException("La edad debe ser un número válido", "edad");
        }
        
        if (edad < 0)
        {
            throw new ArgumentException("La edad no puede ser negativa", "edad");
        }
        
        if (edad > 150)
        {
            throw new ArgumentException("La edad no puede ser mayor a 150", "edad");
        }
        
        Console.WriteLine($"Edad válida: {edad} años");
    }
    
    // Método que lanza ArgumentOutOfRangeException
    static int ValidarNumero(string entrada, int min, int max)
    {
        if (!int.TryParse(entrada, out int numero))
        {
            throw new ArgumentException("Debe ingresar un número válido", "numero");
        }
        
        if (numero < min || numero > max)
        {
            throw new ArgumentOutOfRangeException("numero", numero, 
                $"El número debe estar entre {min} y {max}");
        }
        
        return numero;
    }
    
    // Método que lanza excepción personalizada
    static void ValidarNombre(string nombre)
    {
        if (string.IsNullOrEmpty(nombre))
        {
            throw new NombreInvalidoException("El nombre no puede estar vacío", nombre);
        }
        
        if (nombre.Length < 2)
        {
            throw new NombreInvalidoException("El nombre debe tener al menos 2 caracteres", nombre);
        }
        
        if (nombre.Length > 50)
        {
            throw new NombreInvalidoException("El nombre no puede tener más de 50 caracteres", nombre);
        }
        
        // Verificar que solo contenga letras y espacios
        foreach (char c in nombre)
        {
            if (!char.IsLetter(c) && !char.IsWhiteSpace(c))
            {
                throw new NombreInvalidoException("El nombre solo puede contener letras y espacios", nombre);
            }
        }
        
        Console.WriteLine($"Nombre válido: {nombre}");
    }
}

// Excepción personalizada
public class NombreInvalidoException : Exception
{
    public string NombreIngresado { get; }
    
    public NombreInvalidoException(string message, string nombreIngresado) 
        : base(message)
    {
        NombreIngresado = nombreIngresado;
    }
}
```

#### Explicación del Lanzamiento de Excepciones:

**Lanzamiento Básico:**
- **Línea 25: `throw new ArgumentException("La edad no puede estar vacía", "edad");`**
  - `throw` lanza una nueva excepción
  - `new ArgumentException()` crea una nueva instancia de la excepción
  - Primer parámetro: mensaje descriptivo del error
  - Segundo parámetro: nombre del parámetro que causó el error

**Lanzamiento con Información Adicional:**
- **Línea 35: `throw new ArgumentOutOfRangeException("numero", numero, ...)`**
  - `"numero"` es el nombre del parámetro
  - `numero` es el valor actual que causó el error
  - Tercer parámetro: mensaje descriptivo

**Excepción Personalizada:**
- **Línea 75: `public class NombreInvalidoException : Exception`**
  - Hereda de la clase base `Exception`
  - Agrega propiedades específicas para el contexto del error

- **Línea 78: `public NombreInvalidoException(string message, string nombreIngresado) : base(message)`**
  - Constructor que llama al constructor de la clase base
  - Almacena información adicional en la propiedad personalizada

**Uso de las Excepciones:**
- **Línea 15: `catch (ArgumentException ex)`**
  - Captura específicamente excepciones de argumento
  - Permite manejo específico para este tipo de error

- **Línea 19: `catch (Exception ex)`**
  - Captura cualquier otra excepción no manejada
  - Última línea de defensa para errores inesperados

### 6. Mejores Prácticas

#### 6.1 Principios del Manejo de Excepciones

```csharp
using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== MEJORES PRÁCTICAS ===");
        
        // PRINCIPIO 1: No capturar excepciones que no puedes manejar
        try
        {
            string resultado = ProcesarDatos("datos_validos");
            Console.WriteLine($"Resultado: {resultado}");
        }
        catch (Exception ex)
        {
            // ❌ MAL: Capturar Exception genérica sin poder manejarla
            Console.WriteLine($"Error: {ex.Message}");
            // El programa continúa pero no sabemos si los datos se procesaron correctamente
        }
        
        // PRINCIPIO 2: Capturar excepciones específicas cuando sea posible
        try
        {
            string resultado2 = ProcesarDatos("datos_invalidos");
            Console.WriteLine($"Resultado: {resultado2}");
        }
        catch (ArgumentException ex)
        {
            // ✅ BIEN: Capturar excepción específica y manejarla apropiadamente
            Console.WriteLine($"Error de validación: {ex.Message}");
            Console.WriteLine("Por favor, verifique los datos de entrada.");
        }
        catch (IOException ex)
        {
            // ✅ BIEN: Manejo específico para errores de archivo
            Console.WriteLine($"Error de archivo: {ex.Message}");
            Console.WriteLine("Verifique que el archivo existe y es accesible.");
        }
        
        // PRINCIPIO 3: Usar finally para limpieza de recursos
        try
        {
            using (StreamWriter writer = new StreamWriter("archivo.txt"))
            {
                writer.WriteLine("Datos importantes");
                // Simular un error
                throw new Exception("Error durante la escritura");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            // El archivo se cierra automáticamente gracias al using
        }
        
        // PRINCIPIO 4: Proporcionar mensajes de error útiles
        try
        {
            ValidarEmail("email_invalido");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error de validación: {ex.Message}");
            Console.WriteLine("Formato esperado: usuario@dominio.com");
        }
        
        // PRINCIPIO 5: Logging de excepciones
        try
        {
            ProcesarOperacionCompleja();
        }
        catch (Exception ex)
        {
            // Log de la excepción para debugging
            Console.WriteLine($"Error en operación compleja:");
            Console.WriteLine($"Mensaje: {ex.Message}");
            Console.WriteLine($"Tipo: {ex.GetType().Name}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            
            // Re-lanzar la excepción si no se puede manejar
            throw;
        }
    }
    
    // Método que puede fallar
    static string ProcesarDatos(string datos)
    {
        if (datos == "datos_invalidos")
        {
            throw new ArgumentException("Los datos proporcionados no son válidos", "datos");
        }
        
        if (datos == "datos_archivo")
        {
            throw new IOException("No se puede acceder al archivo de datos");
        }
        
        return $"Datos procesados: {datos}";
    }
    
    // Validación de email
    static void ValidarEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            throw new ArgumentException("El email no puede estar vacío", "email");
        }
        
        if (!email.Contains("@"))
        {
            throw new ArgumentException("El email debe contener el símbolo @", "email");
        }
        
        if (!email.Contains("."))
        {
            throw new ArgumentException("El email debe contener un dominio válido", "email");
        }
        
        Console.WriteLine($"Email válido: {email}");
    }
    
    // Operación compleja que puede fallar
    static void ProcesarOperacionCompleja()
    {
        // Simular operación compleja
        Random random = new Random();
        if (random.Next(1, 4) == 1)
        {
            throw new InvalidOperationException("La operación falló debido a condiciones externas");
        }
        
        Console.WriteLine("Operación completada exitosamente");
    }
}
```

#### Explicación de las Mejores Prácticas:

**PRINCIPIO 1: No capturar excepciones que no puedes manejar**
- **Línea 18: `catch (Exception ex)`**
  - ❌ Captura cualquier excepción sin poder manejarla apropiadamente
  - El programa continúa pero puede estar en un estado inconsistente

**PRINCIPIO 2: Capturar excepciones específicas**
- **Línea 28: `catch (ArgumentException ex)`**
  - ✅ Captura solo errores de validación
  - Permite proporcionar ayuda específica al usuario

- **Línea 33: `catch (IOException ex)`**
  - ✅ Manejo específico para errores de archivo
  - Diferentes acciones para diferentes tipos de error

**PRINCIPIO 3: Usar finally para limpieza**
- **Línea 40: `using (StreamWriter writer = new StreamWriter("archivo.txt"))`**
  - Garantiza que el archivo se cierre automáticamente
  - Incluso si hay una excepción

**PRINCIPIO 4: Mensajes de error útiles**
- **Línea 58: `Console.WriteLine("Formato esperado: usuario@dominio.com");`**
  - Proporciona información específica sobre cómo corregir el error
  - Ayuda al usuario a resolver el problema

**PRINCIPIO 5: Logging de excepciones**
- **Línea 72: `Console.WriteLine($"Stack Trace: {ex.StackTrace}");`**
  - Registra información detallada para debugging
  - Útil para desarrolladores y soporte técnico

- **Línea 75: `throw;`**
  - Re-lanza la excepción después de loggearla
  - Permite que otros niveles de la aplicación la manejen

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Validación de Datos con Excepciones
```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== VALIDACIÓN DE DATOS ===");
        
        // Validar diferentes tipos de entrada
        string[] entradas = { "25", "-5", "abc", "200", "0" };
        
        foreach (string entrada in entradas)
        {
            try
            {
                int edad = ValidarEdad(entrada);
                Console.WriteLine($"Edad válida: {edad}");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error con '{entrada}': {ex.Message}");
            }
        }
    }
    
    static int ValidarEdad(string entrada)
    {
        if (string.IsNullOrEmpty(entrada))
        {
            throw new ArgumentException("La entrada no puede estar vacía", "entrada");
        }
        
        if (!int.TryParse(entrada, out int edad))
        {
            throw new ArgumentException("Debe ingresar un número válido", "entrada");
        }
        
        if (edad < 0)
        {
            throw new ArgumentException("La edad no puede ser negativa", "entrada");
        }
        
        if (edad > 120)
        {
            throw new ArgumentException("La edad no puede ser mayor a 120", "entrada");
        }
        
        return edad;
    }
}
```

### Ejercicio 2: Manejo de Archivos con Excepciones
```csharp
using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== MANEJO DE ARCHIVOS ===");
        
        try
        {
            // Crear archivo
            using (StreamWriter writer = new StreamWriter("datos.txt"))
            {
                writer.WriteLine("Línea 1: Datos importantes");
                writer.WriteLine("Línea 2: Más información");
                writer.WriteLine("Línea 3: Última línea");
            }
            
            Console.WriteLine("Archivo creado correctamente");
            
            // Leer archivo
            using (StreamReader reader = new StreamReader("datos.txt"))
            {
                string contenido = reader.ReadToEnd();
                Console.WriteLine("\nContenido del archivo:");
                Console.WriteLine(contenido);
            }
            
            // Intentar leer archivo que no existe
            using (StreamReader reader2 = new StreamReader("archivo_inexistente.txt"))
            {
                string contenido2 = reader2.ReadToEnd();
            }
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Archivo no encontrado: {ex.Message}");
            Console.WriteLine($"Archivo buscado: {ex.FileName}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error de archivo: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inesperado: {ex.Message}");
        }
        finally
        {
            // Limpiar archivos temporales si es necesario
            if (File.Exists("datos.txt"))
            {
                Console.WriteLine("\nArchivo 'datos.txt' existe y está listo para uso.");
            }
        }
    }
}
```

### Ejercicio 3: Excepción Personalizada
```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== EXCEPCIÓN PERSONALIZADA ===");
        
        string[] nombres = { "Juan", "", "A", "EsteEsUnNombreMuyLargoQueExcedeElLimitePermitido", "María123" };
        
        foreach (string nombre in nombres)
        {
            try
            {
                ValidarNombre(nombre);
                Console.WriteLine($"Nombre válido: {nombre}");
            }
            catch (NombreInvalidoException ex)
            {
                Console.WriteLine($"Error con '{ex.NombreIngresado}': {ex.Message}");
            }
        }
    }
    
    static void ValidarNombre(string nombre)
    {
        if (string.IsNullOrEmpty(nombre))
        {
            throw new NombreInvalidoException("El nombre no puede estar vacío", nombre);
        }
        
        if (nombre.Length < 2)
        {
            throw new NombreInvalidoException("El nombre debe tener al menos 2 caracteres", nombre);
        }
        
        if (nombre.Length > 30)
        {
            throw new NombreInvalidoException("El nombre no puede tener más de 30 caracteres", nombre);
        }
        
        // Verificar que solo contenga letras y espacios
        foreach (char c in nombre)
        {
            if (!char.IsLetter(c) && !char.IsWhiteSpace(c))
            {
                throw new NombreInvalidoException("El nombre solo puede contener letras y espacios", nombre);
            }
        }
    }
}

// Excepción personalizada para nombres inválidos
public class NombreInvalidoException : Exception
{
    public string NombreIngresado { get; }
    
    public NombreInvalidoException(string message, string nombreIngresado) 
        : base(message)
    {
        NombreIngresado = nombreIngresado;
    }
}
```

## 🔍 Conceptos Importantes a Recordar

1. **Las excepciones son eventos inesperados** que interrumpen el flujo normal del programa
2. **try-catch permite manejar errores** sin que el programa se detenga
3. **finally garantiza la ejecución** de código de limpieza
4. **throw lanza excepciones** para indicar condiciones de error
5. **Las excepciones específicas** son mejores que las genéricas
6. **Using statement maneja automáticamente** la disposición de recursos
7. **Las excepciones personalizadas** pueden contener información adicional del contexto
8. **El logging de excepciones** es importante para debugging
9. **No captures excepciones** que no puedes manejar apropiadamente

## ❓ Preguntas de Repaso

1. ¿Cuál es la diferencia entre try-catch y try-finally?
2. ¿Por qué es importante capturar excepciones específicas?
3. ¿Cuándo usarías el bloque finally?
4. ¿Qué significa la palabra clave throw?
5. ¿Cuáles son las mejores prácticas para el manejo de excepciones?

## 🚀 Siguiente Paso

En la próxima clase aprenderemos sobre **Programación Orientada a Objetos Básica**, donde veremos cómo crear y usar clases en C#.

---

## 📚 Recursos Adicionales

- [Manejo de excepciones en C#](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/exceptions/)
- [Try-catch en C#](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/try-catch)
- [Using statement](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/using-statement)

---

**¡Excelente! Ahora dominas el manejo básico de errores en C#! 🎯**
