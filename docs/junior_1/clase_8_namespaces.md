# Clase 8: Namespaces y Organización en C#

## 🎯 Objetivos de la Clase
- Comprender qué son los namespaces y por qué son importantes
- Aprender a organizar el código en proyectos C#
- Entender cómo usar namespaces estándar de .NET
- Dominar la creación y organización de namespaces personalizados

---

## 📚 Navegación del Módulo 1

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_introduccion.md) | Introducción a C# y .NET | |
| [Clase 2](clase_2_variables_tipos.md) | Variables y Tipos de Datos | |
| [Clase 3](clase_3_operadores.md) | Operadores y Expresiones | |
| [Clase 4](clase_4_estructuras_control.md) | Estructuras de Control | |
| [Clase 5](clase_5_colecciones.md) | Colecciones | |
| [Clase 6](clase_6_strings.md) | Manipulación de Strings | |
| [Clase 7](clase_7_funciones.md) | Métodos y Funciones | ← Anterior |
| **Clase 8** | **Namespaces y Organización** | ← Estás aquí |
| [Clase 9](clase_9_manejo_errores.md) | Manejo de Errores | Siguiente → |
| [Clase 10](clase_10_poo_basica.md) | Programación Orientada a Objetos Básica | |

**← [Volver al README del Módulo 1](../junior_1/README.md)**

## 📚 Contenido Teórico

### 1. ¿Qué son los Namespaces?

Los namespaces son **contenedores lógicos** que organizan y agrupan clases, interfaces, estructuras y otros tipos relacionados. Son fundamentales para evitar conflictos de nombres y crear código bien organizado y mantenible.

#### Propósitos de los Namespaces:
- **Organización**: Agrupan código relacionado
- **Evitar Conflictos**: Previenen colisiones de nombres
- **Modularidad**: Permiten separar funcionalidades
- **Reutilización**: Facilitan el uso de código de otros proyectos

### 2. Namespaces Estándar de .NET

#### 2.1 System - Namespace Principal

El namespace `System` contiene las clases fundamentales de .NET:

```csharp
using System;                    // Importa el namespace System

class Program
{
    static void Main(string[] args)
    {
        // Clases del namespace System
        Console.WriteLine("Hola Mundo");           // System.Console
        string texto = "Ejemplo";                  // System.String
        int numero = 42;                          // System.Int32
        DateTime fecha = DateTime.Now;             // System.DateTime
        Random random = new Random();              // System.Random
        
        Console.WriteLine($"Texto: {texto}");
        Console.WriteLine($"Número: {numero}");
        Console.WriteLine($"Fecha: {fecha}");
        Console.WriteLine($"Número aleatorio: {random.Next(1, 101)}");
    }
}
```

#### Explicación del Namespace System:

**Línea 1: `using System;`**
- Importa todas las clases del namespace System
- Permite usar `Console` en lugar de `System.Console`

**Línea 8: `Console.WriteLine("Hola Mundo");`**
- `Console` es una clase del namespace System
- Sin `using System;` tendrías que escribir `System.Console.WriteLine`

**Línea 9: `string texto = "Ejemplo";`**
- `string` es un alias para `System.String`
- Siempre disponible sin importar explícitamente

**Línea 10: `int numero = 42;`**
- `int` es un alias para `System.Int32`
- Tipo primitivo siempre disponible

#### 2.2 System.Collections - Colecciones

El namespace `System.Collections` contiene estructuras de datos:

```csharp
using System;
using System.Collections;           // Importa colecciones no genéricas
using System.Collections.Generic;   // Importa colecciones genéricas

class Program
{
    static void Main(string[] args)
    {
        // Colecciones no genéricas (legacy)
        ArrayList listaLegacy = new ArrayList();
        listaLegacy.Add("Elemento 1");
        listaLegacy.Add(42);
        listaLegacy.Add(true);
        
        Console.WriteLine("ArrayList (legacy):");
        foreach (object item in listaLegacy)
        {
            Console.WriteLine($"- {item} ({item.GetType()})");
        }
        
        // Colecciones genéricas (recomendadas)
        List<string> listaStrings = new List<string>();
        listaStrings.Add("Primer string");
        listaStrings.Add("Segundo string");
        listaStrings.Add("Tercer string");
        
        Console.WriteLine("\nList<string> (genérica):");
        foreach (string item in listaStrings)
        {
            Console.WriteLine($"- {item}");
        }
        
        // Dictionary genérico
        Dictionary<string, int> edades = new Dictionary<string, int>();
        edades.Add("Juan", 25);
        edades.Add("María", 30);
        edades.Add("Carlos", 28);
        
        Console.WriteLine("\nDictionary<string, int>:");
        foreach (var par in edades)
        {
            Console.WriteLine($"- {par.Key}: {par.Value} años");
        }
    }
}
```

#### Explicación de System.Collections:

**Línea 3: `using System.Collections;`**
- Importa colecciones no genéricas (legacy)
- Incluye ArrayList, Hashtable, Queue, Stack

**Línea 4: `using System.Collections.Generic;`**
- Importa colecciones genéricas (modernas)
- Incluye List<T>, Dictionary<TKey, TValue>, HashSet<T>

**Línea 9: `ArrayList listaLegacy = new ArrayList();`**
- `ArrayList` puede contener cualquier tipo de objeto
- No es type-safe, puede causar errores en tiempo de ejecución

**Línea 20: `List<string> listaStrings = new List<string>();`**
- `List<string>` solo puede contener strings
- Type-safe, errores detectados en tiempo de compilación

**Línea 30: `Dictionary<string, int> edades = new Dictionary<string, int>();`**
- `Dictionary` almacena pares clave-valor
- `<string, int>` especifica que las claves son strings y los valores son int

#### 2.3 System.IO - Entrada/Salida

El namespace `System.IO` maneja archivos y directorios:

```csharp
using System;
using System.IO;                    // Importa funcionalidades de archivos

class Program
{
    static void Main(string[] args)
    {
        // Información del directorio actual
        string directorioActual = Directory.GetCurrentDirectory();
        Console.WriteLine($"Directorio actual: {directorioActual}");
        
        // Listar archivos en el directorio
        string[] archivos = Directory.GetFiles(directorioActual);
        Console.WriteLine($"\nArchivos en el directorio:");
        foreach (string archivo in archivos)
        {
            FileInfo info = new FileInfo(archivo);
            Console.WriteLine($"- {info.Name} ({info.Length} bytes)");
        }
        
        // Crear y escribir en un archivo
        string rutaArchivo = Path.Combine(directorioActual, "ejemplo.txt");
        
        try
        {
            // Escribir en archivo
            File.WriteAllText(rutaArchivo, "Este es un archivo de ejemplo\nCreado con C#");
            Console.WriteLine($"\nArchivo creado: {rutaArchivo}");
            
            // Leer archivo
            string contenido = File.ReadAllText(rutaArchivo);
            Console.WriteLine($"\nContenido del archivo:");
            Console.WriteLine(contenido);
            
            // Información del archivo
            FileInfo infoArchivo = new FileInfo(rutaArchivo);
            Console.WriteLine($"\nTamaño: {infoArchivo.Length} bytes");
            Console.WriteLine($"Creado: {infoArchivo.CreationTime}");
            Console.WriteLine($"Modificado: {infoArchivo.LastWriteTime}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
```

#### Explicación de System.IO:

**Línea 3: `using System.IO;`**
- Importa funcionalidades para manejo de archivos y directorios

**Línea 9: `Directory.GetCurrentDirectory()`**
- Obtiene la ruta del directorio de trabajo actual
- Retorna un string con la ruta completa

**Línea 13: `Directory.GetFiles(directorioActual)`**
- Obtiene un array con las rutas de todos los archivos en el directorio
- No incluye subdirectorios

**Línea 18: `new FileInfo(archivo)`**
- `FileInfo` proporciona información detallada sobre un archivo
- Propiedades como Name, Length, CreationTime, etc.

**Línea 25: `Path.Combine(directorioActual, "ejemplo.txt")`**
- Combina rutas de manera segura para el sistema operativo
- Maneja automáticamente separadores de directorio

**Línea 30: `File.WriteAllText(rutaArchivo, contenido)`**
- Escribe todo el contenido en un archivo
- Si el archivo no existe, lo crea; si existe, lo sobrescribe

**Línea 34: `File.ReadAllText(rutaArchivo)`**
- Lee todo el contenido del archivo como un string

#### 2.4 System.Text - Manipulación de Texto

El namespace `System.Text` incluye StringBuilder y codificación:

```csharp
using System;
using System.Text;                  // Importa StringBuilder y codificación

class Program
{
    static void Main(string[] args)
    {
        // StringBuilder para manipulación eficiente de strings
        StringBuilder sb = new StringBuilder();
        
        sb.AppendLine("=== REPORTE ===");
        sb.AppendLine($"Fecha: {DateTime.Now:yyyy-MM-dd}");
        sb.AppendLine($"Hora: {DateTime.Now:HH:mm:ss}");
        sb.AppendLine();
        
        // Agregar datos en formato de tabla
        sb.AppendLine("DATOS:");
        sb.AppendLine("------");
        sb.AppendLine($"Nombre: {"Juan Pérez",-20} | Edad: {25,3}");
        sb.AppendLine($"Email:  {"juan@email.com",-20} | Ciudad: {"Madrid",-10}");
        sb.AppendLine($"Teléfono: {"+34 123 456 789",-20} | País: {"España",-10}");
        
        // Mostrar el resultado
        Console.WriteLine(sb.ToString());
        
        // Codificación de strings
        string textoOriginal = "Hola Mundo con acentos: áéíóúñ";
        Console.WriteLine($"\nTexto original: {textoOriginal}");
        
        // Convertir a bytes usando diferentes codificaciones
        byte[] bytesUTF8 = Encoding.UTF8.GetBytes(textoOriginal);
        byte[] bytesASCII = Encoding.ASCII.GetBytes(textoOriginal);
        
        Console.WriteLine($"\nUTF-8 ({bytesUTF8.Length} bytes): {BitConverter.ToString(bytesUTF8)}");
        Console.WriteLine($"ASCII ({bytesASCII.Length} bytes): {BitConverter.ToString(bytesASCII)}");
        
        // Convertir bytes de vuelta a string
        string textoUTF8 = Encoding.UTF8.GetString(bytesUTF8);
        string textoASCII = Encoding.ASCII.GetString(bytesASCII);
        
        Console.WriteLine($"\nUTF-8 decodificado: {textoUTF8}");
        Console.WriteLine($"ASCII decodificado: {textoASCII}");
    }
}
```

#### Explicación de System.Text:

**Línea 3: `using System.Text;`**
- Importa StringBuilder y clases de codificación

**Línea 8: `StringBuilder sb = new StringBuilder();`**
- `StringBuilder` es eficiente para construir strings complejos
- Evita crear múltiples objetos string temporales

**Línea 10: `sb.AppendLine("=== REPORTE ===");`**
- `AppendLine` agrega texto y un salto de línea
- Más eficiente que concatenación con `+`

**Línea 19: `{"Juan Pérez",-20}`**
- Formato de string con alineación
- `-20` significa alinear a la izquierda con 20 espacios

**Línea 30: `Encoding.UTF8.GetBytes(textoOriginal)`**
- `Encoding.UTF8` es la codificación estándar para texto
- `GetBytes` convierte el string a un array de bytes

**Línea 31: `Encoding.ASCII.GetBytes(textoOriginal)`**
- `Encoding.ASCII` es una codificación más simple
- Solo soporta 128 caracteres (sin acentos)

**Línea 35: `BitConverter.ToString(bytesUTF8)`**
- Convierte el array de bytes a una representación hexadecimal
- Útil para debugging y análisis

### 3. Creación de Namespaces Personalizados

#### 3.1 Estructura Básica

```csharp
using System;

// Namespace personalizado para matemáticas
namespace Matematicas
{
    // Clase para cálculos básicos
    public class Calculadora
    {
        public static int Sumar(int a, int b)
        {
            return a + b;
        }
        
        public static int Restar(int a, int b)
        {
            return a - b;
        }
        
        public static int Multiplicar(int a, int b)
        {
            return a * b;
        }
        
        public static double Dividir(int a, int b)
        {
            if (b == 0)
                throw new DivideByZeroException("No se puede dividir por cero");
            
            return (double)a / b;
        }
    }
    
    // Clase para geometría
    public class Geometria
    {
        public static double CalcularAreaCirculo(double radio)
        {
            return Math.PI * radio * radio;
        }
        
        public static double CalcularPerimetroCirculo(double radio)
        {
            return 2 * Math.PI * radio;
        }
        
        public static double CalcularAreaRectangulo(double baseRect, double altura)
        {
            return baseRect * altura;
        }
    }
}

// Namespace para utilidades
namespace Utilidades
{
    public class Validador
    {
        public static bool EsNumeroPar(int numero)
        {
            return numero % 2 == 0;
        }
        
        public static bool EsNumeroPrimo(int numero)
        {
            if (numero < 2) return false;
            
            for (int i = 2; i <= Math.Sqrt(numero); i++)
            {
                if (numero % i == 0) return false;
            }
            return true;
        }
        
        public static bool EsPalindromo(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return false;
            
            string textoLimpio = texto.ToLower().Replace(" ", "");
            string textoReverso = new string(textoLimpio.Reverse().ToArray());
            
            return textoLimpio == textoReverso;
        }
    }
}

// Programa principal
class Program
{
    static void Main(string[] args)
    {
        // Usar clases del namespace Matematicas
        Console.WriteLine("=== MATEMÁTICAS ===");
        Console.WriteLine($"5 + 3 = {Matematicas.Calculadora.Sumar(5, 3)}");
        Console.WriteLine($"10 - 4 = {Matematicas.Calculadora.Restar(10, 4)}");
        Console.WriteLine($"6 * 7 = {Matematicas.Calculadora.Multiplicar(6, 7)}");
        Console.WriteLine($"15 / 3 = {Matematicas.Calculadora.Dividir(15, 3)}");
        
        Console.WriteLine($"\nÁrea del círculo (radio 5): {Matematicas.Geometria.CalcularAreaCirculo(5):F2}");
        Console.WriteLine($"Perímetro del círculo (radio 5): {Matematicas.Geometria.CalcularPerimetroCirculo(5):F2}");
        
        // Usar clases del namespace Utilidades
        Console.WriteLine("\n=== VALIDACIONES ===");
        Console.WriteLine($"¿8 es par? {Utilidades.Validador.EsNumeroPar(8)}");
        Console.WriteLine($"¿17 es primo? {Utilidades.Validador.EsNumeroPrimo(17)}");
        Console.WriteLine($"¿'Anita lava la tina' es palíndromo? {Utilidades.Validador.EsPalindromo("Anita lava la tina")}");
    }
}
```

#### Explicación de Namespaces Personalizados:

**Línea 4: `namespace Matematicas`**
- Define un namespace llamado "Matematicas"
- Agrupa clases relacionadas con matemáticas

**Línea 6: `public class Calculadora`**
- `public` permite que la clase sea accesible desde otros namespaces
- Sin `public`, la clase solo sería accesible dentro del namespace

**Línea 8: `public static int Sumar(int a, int b)`**
- `static` permite llamar al método sin crear una instancia
- `public` permite acceder desde fuera del namespace

**Línea 25: `throw new DivideByZeroException("No se puede dividir por cero");`**
- `throw` lanza una excepción cuando se intenta dividir por cero
- Previene errores en tiempo de ejecución

**Línea 50: `namespace Utilidades`**
- Segundo namespace personalizado
- Agrupa clases de utilidades generales

**Línea 75: `Matematicas.Calculadora.Sumar(5, 3)`**
- Acceso completo al método usando el nombre del namespace
- Sintaxis: `Namespace.Clase.Metodo`

#### 3.2 Namespaces Anidados

```csharp
using System;

// Namespace principal
namespace MiAplicacion
{
    // Namespace anidado para la interfaz de usuario
    namespace UI
    {
        public class Formulario
        {
            public string Titulo { get; set; }
            public int Ancho { get; set; }
            public int Alto { get; set; }
            
            public void Mostrar()
            {
                Console.WriteLine($"Formulario: {Titulo}");
                Console.WriteLine($"Dimensiones: {Ancho}x{Alto}");
            }
        }
        
        public class Boton
        {
            public string Texto { get; set; }
            public bool Habilitado { get; set; }
            
            public void HacerClic()
            {
                if (Habilitado)
                {
                    Console.WriteLine($"Botón '{Texto}' fue clickeado");
                }
                else
                {
                    Console.WriteLine($"Botón '{Texto}' está deshabilitado");
                }
            }
        }
    }
    
    // Namespace anidado para lógica de negocio
    namespace Logica
    {
        public class Usuario
        {
            public string Nombre { get; set; }
            public string Email { get; set; }
            public bool Activo { get; set; }
            
            public bool ValidarEmail()
            {
                return !string.IsNullOrEmpty(Email) && Email.Contains("@");
            }
        }
        
        public class GestorUsuarios
        {
            private List<Usuario> usuarios = new List<Usuario>();
            
            public void AgregarUsuario(Usuario usuario)
            {
                if (usuario.ValidarEmail())
                {
                    usuarios.Add(usuario);
                    Console.WriteLine($"Usuario {usuario.Nombre} agregado correctamente");
                }
                else
                {
                    Console.WriteLine($"Email inválido para {usuario.Nombre}");
                }
            }
            
            public void ListarUsuarios()
            {
                Console.WriteLine("\n=== LISTA DE USUARIOS ===");
                foreach (var usuario in usuarios)
                {
                    Console.WriteLine($"- {usuario.Nombre} ({usuario.Email}) - {(usuario.Activo ? "Activo" : "Inactivo")}");
                }
            }
        }
    }
}

// Programa principal
class Program
{
    static void Main(string[] args)
    {
        // Crear elementos de UI
        var formulario = new MiAplicacion.UI.Formulario
        {
            Titulo = "Registro de Usuario",
            Ancho = 800,
            Alto = 600
        };
        
        var boton = new MiAplicacion.UI.Boton
        {
            Texto = "Guardar",
            Habilitado = true
        };
        
        // Crear usuarios
        var usuario1 = new MiAplicacion.Logica.Usuario
        {
            Nombre = "Juan Pérez",
            Email = "juan@email.com",
            Activo = true
        };
        
        var usuario2 = new MiAplicacion.Logica.Usuario
        {
            Nombre = "María García",
            Email = "maria@email.com",
            Activo = false
        };
        
        // Usar la lógica de negocio
        var gestor = new MiAplicacion.Logica.GestorUsuarios();
        gestor.AgregarUsuario(usuario1);
        gestor.AgregarUsuario(usuario2);
        
        // Mostrar elementos de UI
        formulario.Mostrar();
        boton.HacerClic();
        
        // Listar usuarios
        gestor.ListarUsuarios();
    }
}
```

#### Explicación de Namespaces Anidados:

**Línea 4: `namespace MiAplicacion`**
- Namespace principal que contiene toda la aplicación

**Línea 7: `namespace UI`**
- Namespace anidado para componentes de interfaz de usuario
- Acceso completo: `MiAplicacion.UI.Formulario`

**Línea 9: `public class Formulario`**
- Clase pública accesible desde otros namespaces
- Propiedades para configurar el formulario

**Línea 35: `namespace Logica`**
- Namespace anidado para lógica de negocio
- Separado de la interfaz de usuario

**Línea 37: `public class Usuario`**
- Clase que representa un usuario del sistema
- Método para validar email

**Línea 50: `public class GestorUsuarios`**
- Clase que maneja la colección de usuarios
- Métodos para agregar y listar usuarios

**Línea 85: `new MiAplicacion.UI.Formulario`**
- Acceso completo a la clase usando la ruta del namespace
- Sintaxis: `NamespacePrincipal.NamespaceAnidado.Clase`

### 4. Using Statements y Aliases

#### 4.1 Using Directives

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

// Using con alias para evitar conflictos de nombres
using ConsoleApp = System.Console;
using FileSystem = System.IO.File;
using DirectorySystem = System.IO.Directory;

// Using estático para acceder a métodos estáticos directamente
using static System.Math;
using static System.Console;

class Program
{
    static void Main(string[] args)
    {
        // Usar alias para evitar conflictos
        ConsoleApp.WriteLine("Usando alias para Console");
        
        // Usar alias para File
        string contenido = FileSystem.ReadAllText("ejemplo.txt");
        ConsoleApp.WriteLine($"Contenido del archivo: {contenido}");
        
        // Usar alias para Directory
        string[] archivos = DirectorySystem.GetFiles(".");
        ConsoleApp.WriteLine($"Archivos en el directorio actual: {archivos.Length}");
        
        // Usar métodos estáticos directamente (using static)
        double raizCuadrada = Sqrt(16);        // En lugar de Math.Sqrt(16)
        double potencia = Pow(2, 8);           // En lugar de Math.Pow(2, 8)
        double seno = Sin(PI / 2);             // En lugar de Math.Sin(Math.PI / 2)
        
        WriteLine($"Raíz cuadrada de 16: {raizCuadrada}");     // En lugar de Console.WriteLine
        WriteLine($"2 elevado a 8: {potencia}");
        WriteLine($"Seno de π/2: {seno}");
        
        // Usar métodos estáticos de Math sin Math.
        double logaritmo = Log(100, 10);
        double valorAbsoluto = Abs(-42.5);
        double redondeo = Round(3.7);
        
        WriteLine($"Logaritmo base 10 de 100: {logaritmo}");
        WriteLine($"Valor absoluto de -42.5: {valorAbsoluto}");
        WriteLine($"3.7 redondeado: {redondeo}");
    }
}
```

#### Explicación de Using Directives:

**Línea 1-4: Using básicos**
- Importan namespaces completos
- Permiten usar clases sin el nombre completo del namespace

**Línea 6-8: Using con alias**
- `using ConsoleApp = System.Console;` crea un alias para Console
- Útil cuando hay conflictos de nombres
- `ConsoleApp.WriteLine()` es equivalente a `System.Console.WriteLine()`

**Línea 10-11: Using estático**
- `using static System.Math;` permite usar métodos estáticos sin el nombre de la clase
- `Sqrt(16)` en lugar de `Math.Sqrt(16)`

**Línea 12: `using static System.Console;`**
- Permite usar métodos de Console sin escribir `Console.`
- `WriteLine()` en lugar de `Console.WriteLine()`

**Línea 25: `Sqrt(16)`**
- Acceso directo al método estático
- Equivalente a `Math.Sqrt(16)`

**Línea 26: `Pow(2, 8)`**
- Acceso directo al método estático
- Equivalente a `Math.Pow(2, 8)`

#### 4.2 Using Statements para Recursos

```csharp
using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        // Using statement para manejo automático de recursos
        using (StreamWriter writer = new StreamWriter("archivo.txt"))
        {
            writer.WriteLine("Primera línea del archivo");
            writer.WriteLine("Segunda línea del archivo");
            writer.WriteLine("Tercera línea del archivo");
            
            Console.WriteLine("Archivo escrito correctamente");
        } // El archivo se cierra automáticamente aquí
        
        // Leer el archivo usando using statement
        using (StreamReader reader = new StreamReader("archivo.txt"))
        {
            string contenido = reader.ReadToEnd();
            Console.WriteLine("\nContenido del archivo:");
            Console.WriteLine(contenido);
        } // El archivo se cierra automáticamente aquí
        
        // Using statement con múltiples recursos
        using (StreamWriter writer1 = new StreamWriter("archivo1.txt"))
        using (StreamWriter writer2 = new StreamWriter("archivo2.txt"))
        {
            writer1.WriteLine("Contenido del archivo 1");
            writer2.WriteLine("Contenido del archivo 2");
            
            Console.WriteLine("Dos archivos escritos correctamente");
        }
        
        // Verificar que los archivos se crearon
        string[] archivos = Directory.GetFiles(".", "archivo*.txt");
        Console.WriteLine($"\nArchivos creados: {archivos.Length}");
        foreach (string archivo in archivos)
        {
            FileInfo info = new FileInfo(archivo);
            Console.WriteLine($"- {info.Name}: {info.Length} bytes");
        }
    }
}
```

#### Explicación de Using Statements:

**Línea 12: `using (StreamWriter writer = new StreamWriter("archivo.txt"))`**
- `using` statement crea un bloque de código
- El recurso se dispone automáticamente al salir del bloque
- Equivalente a try-finally con Dispose()

**Línea 18: `}` // El archivo se cierra automáticamente aquí**
- Al salir del bloque using, se llama automáticamente a `writer.Dispose()`
- Esto cierra el archivo y libera recursos

**Línea 22: `using (StreamReader reader = new StreamReader("archivo.txt"))`**
- Otro ejemplo de using statement
- El archivo se abre para lectura y se cierra automáticamente

**Línea 30-31: Múltiples using statements anidados**
- Permite manejar múltiples recursos en un solo bloque
- Todos los recursos se disponen al salir del bloque

**Ventajas del using statement:**
- **Automático**: No necesitas llamar manualmente a Dispose()
- **Seguro**: Garantiza que los recursos se liberen incluso si hay excepciones
- **Legible**: El código es más claro sobre el ciclo de vida del recurso

### 5. Organización de Proyectos

#### 5.1 Estructura de Archivos

```csharp
// Archivo: Models/Usuario.cs
namespace MiAplicacion.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public DateTime FechaCreacion { get; set; }
        
        public Usuario()
        {
            FechaCreacion = DateTime.Now;
        }
        
        public bool ValidarEmail()
        {
            return !string.IsNullOrEmpty(Email) && Email.Contains("@");
        }
    }
}

// Archivo: Services/UsuarioService.cs
namespace MiAplicacion.Services
{
    using MiAplicacion.Models;
    
    public class UsuarioService
    {
        private List<Usuario> usuarios = new List<Usuario>();
        
        public void AgregarUsuario(Usuario usuario)
        {
            if (usuario.ValidarEmail())
            {
                usuario.Id = usuarios.Count + 1;
                usuarios.Add(usuario);
            }
            else
            {
                throw new ArgumentException("Email inválido");
            }
        }
        
        public Usuario ObtenerUsuario(int id)
        {
            return usuarios.FirstOrDefault(u => u.Id == id);
        }
        
        public List<Usuario> ObtenerTodos()
        {
            return new List<Usuario>(usuarios);
        }
    }
}

// Archivo: Controllers/UsuarioController.cs
namespace MiAplicacion.Controllers
{
    using MiAplicacion.Models;
    using MiAplicacion.Services;
    
    public class UsuarioController
    {
        private readonly UsuarioService _usuarioService;
        
        public UsuarioController()
        {
            _usuarioService = new UsuarioService();
        }
        
        public void CrearUsuario(string nombre, string email)
        {
            var usuario = new Usuario
            {
                Nombre = nombre,
                Email = email
            };
            
            _usuarioService.AgregarUsuario(usuario);
            Console.WriteLine($"Usuario {usuario.Nombre} creado con ID {usuario.Id}");
        }
        
        public void ListarUsuarios()
        {
            var usuarios = _usuarioService.ObtenerTodos();
            Console.WriteLine("\n=== LISTA DE USUARIOS ===");
            
            foreach (var usuario in usuarios)
            {
                Console.WriteLine($"ID: {usuario.Id}, Nombre: {usuario.Nombre}, Email: {usuario.Email}");
            }
        }
    }
}

// Archivo: Program.cs (archivo principal)
using MiAplicacion.Controllers;

class Program
{
    static void Main(string[] args)
    {
        var controller = new UsuarioController();
        
        // Crear usuarios
        controller.CrearUsuario("Juan Pérez", "juan@email.com");
        controller.CrearUsuario("María García", "maria@email.com");
        controller.CrearUsuario("Carlos López", "carlos@email.com");
        
        // Listar usuarios
        controller.ListarUsuarios();
    }
}
```

#### Explicación de la Organización:

**Estructura de archivos:**
- **Models/**: Contiene las clases de datos (Usuario)
- **Services/**: Contiene la lógica de negocio (UsuarioService)
- **Controllers/**: Contiene la lógica de control (UsuarioController)
- **Program.cs**: Punto de entrada de la aplicación

**Namespaces organizados:**
- `MiAplicacion.Models`: Para clases de datos
- `MiAplicacion.Services`: Para servicios de negocio
- `MiAplicacion.Controllers`: Para controladores

**Separación de responsabilidades:**
- **Models**: Solo datos y validaciones básicas
- **Services**: Lógica de negocio y operaciones
- **Controllers**: Coordinación entre la interfaz y los servicios

**Using statements específicos:**
- Cada archivo importa solo los namespaces que necesita
- Mantiene el código limpio y organizado

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Namespace de Matemáticas
```csharp
using System;

namespace Matematicas
{
    public static class Calculadora
    {
        public static double CalcularAreaTriangulo(double baseTriangulo, double altura)
        {
            return 0.5 * baseTriangulo * altura;
        }
        
        public static double CalcularVolumenCubo(double lado)
        {
            return Math.Pow(lado, 3);
        }
        
        public static bool EsTrianguloValido(double lado1, double lado2, double lado3)
        {
            return lado1 + lado2 > lado3 && 
                   lado1 + lado3 > lado2 && 
                   lado2 + lado3 > lado1;
        }
    }
}

// Programa principal
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== CÁLCULOS MATEMÁTICOS ===");
        
        double areaTriangulo = Matematicas.Calculadora.CalcularAreaTriangulo(5, 3);
        Console.WriteLine($"Área del triángulo: {areaTriangulo}");
        
        double volumenCubo = Matematicas.Calculadora.CalcularVolumenCubo(4);
        Console.WriteLine($"Volumen del cubo: {volumenCubo}");
        
        bool esValido = Matematicas.Calculadora.EsTrianguloValido(3, 4, 5);
        Console.WriteLine($"¿Triángulo 3,4,5 es válido? {esValido}");
    }
}
```

### Ejercicio 2: Namespace de Utilidades
```csharp
using System;
using System.Text;

namespace Utilidades
{
    public static class StringUtils
    {
        public static string Invertir(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return texto;
            
            char[] caracteres = texto.ToCharArray();
            Array.Reverse(caracteres);
            return new string(caracteres);
        }
        
        public static int ContarVocales(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return 0;
            
            string vocales = "aeiouáéíóúüAEIOUÁÉÍÓÚÜ";
            int contador = 0;
            
            foreach (char c in texto)
            {
                if (vocales.Contains(c))
                    contador++;
            }
            
            return contador;
        }
        
        public static string FormatearTelefono(string telefono)
        {
            if (string.IsNullOrEmpty(telefono)) return telefono;
            
            // Eliminar caracteres no numéricos
            string soloNumeros = new string(telefono.Where(char.IsDigit).ToArray());
            
            if (soloNumeros.Length == 10)
            {
                return $"({soloNumeros.Substring(0, 3)}) {soloNumeros.Substring(3, 3)}-{soloNumeros.Substring(6, 4)}";
            }
            
            return telefono; // Retornar original si no tiene 10 dígitos
        }
    }
}

// Programa principal
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== UTILIDADES DE STRING ===");
        
        string texto = "Hola Mundo";
        string invertido = Utilidades.StringUtils.Invertir(texto);
        Console.WriteLine($"Texto original: {texto}");
        Console.WriteLine($"Texto invertido: {invertido}");
        
        int vocales = Utilidades.StringUtils.ContarVocales(texto);
        Console.WriteLine($"Número de vocales: {vocales}");
        
        string telefono = "1234567890";
        string formateado = Utilidades.StringUtils.FormatearTelefono(telefono);
        Console.WriteLine($"Teléfono formateado: {formateado}");
    }
}
```

### Ejercicio 3: Namespaces Anidados
```csharp
using System;

namespace MiEmpresa
{
    namespace Empleados
    {
        public class Empleado
        {
            public int Id { get; set; }
            public string Nombre { get; set; }
            public string Cargo { get; set; }
            public decimal Salario { get; set; }
            
            public override string ToString()
            {
                return $"ID: {Id}, Nombre: {Nombre}, Cargo: {Cargo}, Salario: ${Salario:F2}";
            }
        }
    }
    
    namespace Departamentos
    {
        public class Departamento
        {
            public string Nombre { get; set; }
            public string Ubicacion { get; set; }
            public List<Empleados.Empleado> Empleados { get; set; }
            
            public Departamento()
            {
                Empleados = new List<Empleados.Empleado>();
            }
            
            public void AgregarEmpleado(Empleados.Empleado empleado)
            {
                Empleados.Add(empleado);
            }
            
            public void MostrarEmpleados()
            {
                Console.WriteLine($"\n=== DEPARTAMENTO: {Nombre} ===");
                Console.WriteLine($"Ubicación: {Ubicacion}");
                Console.WriteLine($"Número de empleados: {Empleados.Count}");
                
                foreach (var empleado in Empleados)
                {
                    Console.WriteLine($"- {empleado}");
                }
            }
        }
    }
}

// Programa principal
class Program
{
    static void Main(string[] args)
    {
        // Crear empleados
        var empleado1 = new MiEmpresa.Empleados.Empleado
        {
            Id = 1,
            Nombre = "Juan Pérez",
            Cargo = "Desarrollador",
            Salario = 50000
        };
        
        var empleado2 = new MiEmpresa.Empleados.Empleado
        {
            Id = 2,
            Nombre = "María García",
            Cargo = "Diseñadora",
            Salario = 45000
        };
        
        // Crear departamento
        var departamento = new MiEmpresa.Departamentos.Departamento
        {
            Nombre = "Tecnología",
            Ubicacion = "Piso 3"
        };
        
        // Agregar empleados al departamento
        departamento.AgregarEmpleado(empleado1);
        departamento.AgregarEmpleado(empleado2);
        
        // Mostrar información
        departamento.MostrarEmpleados();
    }
}
```

## 🔍 Conceptos Importantes a Recordar

1. **Los namespaces organizan el código** en grupos lógicos
2. **System es el namespace principal** que contiene clases fundamentales
3. **Los using statements** importan namespaces para uso directo
4. **Los namespaces anidados** crean jerarquías organizativas
5. **Los using statements con alias** evitan conflictos de nombres
6. **Los using statements estáticos** permiten acceso directo a métodos estáticos
7. **Los using statements para recursos** manejan automáticamente la disposición
8. **La organización de archivos** debe reflejar la estructura de namespaces
9. **La separación de responsabilidades** mejora la mantenibilidad del código

## ❓ Preguntas de Repaso

1. ¿Por qué son importantes los namespaces en C#?
2. ¿Cuál es la diferencia entre `using System;` y `using static System.Math;`?
3. ¿Cómo se organizan los namespaces anidados?
4. ¿Cuándo usarías un alias en un using statement?
5. ¿Cómo funciona el using statement para recursos?

## 🚀 Siguiente Paso

En la próxima clase aprenderemos sobre **Manejo de Errores Básico**, donde veremos cómo manejar excepciones y errores en C#.

---

## 📚 Recursos Adicionales

- [Namespaces en C#](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/namespaces/)
- [Using Directives](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/using-directive)
- [Using Statement](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/using-statement)

---

**¡Excelente! Ahora dominas la organización con namespaces en C#! 🎯**
