# Clase 1: Introducción a C# - Fundamentos Básicos

## 🎯 Objetivos de la Clase
- Comprender qué es C# y por qué es importante
- Instalar y configurar el entorno de desarrollo .NET
- Crear el primer programa "Hello World"
- Entender la estructura básica de un programa C#

## 📚 Contenido Teórico

### 1. ¿Qué es C#?

C# (pronunciado "C Sharp") es un lenguaje de programación moderno, orientado a objetos, desarrollado por Microsoft en el año 2000. Es parte de la plataforma .NET y está diseñado para ser:

- **Simple y Moderno**: Sintaxis clara y fácil de aprender
- **Orientado a Objetos**: Sigue los principios de la programación orientada a objetos
- **Tipado Fuerte**: El compilador verifica los tipos de datos en tiempo de compilación
- **Multiplataforma**: Puede ejecutarse en Windows, macOS y Linux
- **Eficiente**: Compilado a código nativo para mejor rendimiento

#### Aplicaciones de C#
- **Aplicaciones de Escritorio**: Windows Forms, WPF, UWP
- **Desarrollo Web**: ASP.NET Core, Blazor
- **Aplicaciones Móviles**: Xamarin, .NET MAUI
- **Juegos**: Unity Game Engine
- **Servicios Web**: Web APIs, Microservicios
- **Aplicaciones de Consola**: Herramientas de línea de comandos

### 2. ¿Qué es .NET?

.NET es una plataforma de desarrollo gratuita y de código abierto desarrollada por Microsoft. Incluye:

- **Framework de Desarrollo**: Librerías y herramientas para crear aplicaciones
- **Runtime**: Entorno de ejecución para las aplicaciones
- **Lenguajes**: C#, F#, Visual Basic
- **Herramientas**: Compiladores, debuggers, editores

#### Versiones de .NET
- **.NET Framework**: Versión tradicional para Windows
- **.NET Core**: Versión multiplataforma (ahora .NET 5+)
- **.NET 8**: Versión LTS más reciente (recomendada para aprender)

### 3. Instalación del Entorno de Desarrollo

#### Paso 1: Instalar .NET SDK
1. Ve a [dotnet.microsoft.com](https://dotnet.microsoft.com)
2. Descarga .NET 8.0 SDK para tu sistema operativo
3. Ejecuta el instalador y sigue las instrucciones

#### Paso 2: Verificar la Instalación
Abre una terminal o command prompt y ejecuta:
```bash
dotnet --version
```
Deberías ver algo como: `8.0.100`

#### Paso 3: Instalar Visual Studio Code
1. Ve a [code.visualstudio.com](https://code.visualstudio.com)
2. Descarga e instala VS Code
3. Instala la extensión "C#" de Microsoft

### 4. Estructura Básica de un Programa C#

Vamos a analizar línea por línea el primer programa:

```csharp
using System;                    // Línea 1: Importa el namespace System
                                 // System contiene las clases básicas como Console

namespace MiPrimerPrograma       // Línea 2: Define un namespace (espacio de nombres)
{                                // Línea 3: Abre el bloque del namespace
    class Program                // Línea 4: Define una clase llamada Program
    {                            // Línea 5: Abre el bloque de la clase
        static void Main(string[] args)  // Línea 6: Método principal del programa
        {                        // Línea 7: Abre el bloque del método Main
            Console.WriteLine("¡Hola Mundo!");  // Línea 8: Imprime texto en consola
        }                        // Línea 9: Cierra el bloque del método Main
    }                            // Línea 10: Cierra el bloque de la clase
}                                // Línea 11: Cierra el bloque del namespace
```

#### Explicación Línea por Línea:

**Línea 1: `using System;`**
- `using` es una directiva que importa un namespace
- `System` es el namespace principal que contiene las clases básicas de .NET
- `;` indica el final de la instrucción

**Línea 2: `namespace MiPrimerPrograma`**
- `namespace` define un espacio de nombres para organizar el código
- `MiPrimerPrograma` es el nombre del namespace (puede ser cualquier nombre)
- No lleva punto y coma porque es una declaración

**Línea 3: `{`**
- Llave de apertura que indica el inicio del bloque del namespace
- Todo lo que esté dentro de estas llaves pertenece al namespace

**Línea 4: `class Program`**
- `class` declara una nueva clase
- `Program` es el nombre de la clase (puede ser cualquier nombre)
- Las clases son plantillas para crear objetos

**Línea 5: `{`**
- Llave de apertura que indica el inicio del bloque de la clase
- Todo lo que esté dentro de estas llaves pertenece a la clase

**Línea 6: `static void Main(string[] args)`**
- `static` significa que el método pertenece a la clase, no a una instancia
- `void` indica que el método no retorna ningún valor
- `Main` es el nombre del método (debe ser exactamente "Main")
- `string[] args` son los argumentos de línea de comandos
- `()` contienen los parámetros del método

**Línea 7: `{`**
- Llave de apertura que indica el inicio del bloque del método
- Todo lo que esté dentro de estas llaves se ejecuta cuando se llama al método

**Línea 8: `Console.WriteLine("¡Hola Mundo!");`**
- `Console` es una clase que representa la consola
- `.` accede a un miembro de la clase
- `WriteLine` es un método que imprime texto y salta a la siguiente línea
- `"¡Hola Mundo!"` es un string literal (texto entre comillas)
- `;` indica el final de la instrucción

**Líneas 9-11: Cierre de bloques**
- Cada `}` cierra el bloque correspondiente en orden inverso

### 5. Creando el Primer Programa

#### Paso 1: Crear el Proyecto
Abre una terminal y ejecuta:
```bash
# Crear un directorio para el proyecto
mkdir MiPrimerPrograma
cd MiPrimerPrograma

# Crear un nuevo proyecto de consola
dotnet new console
```

#### Paso 2: Explorar la Estructura
El comando `dotnet new console` crea:
```
MiPrimerPrograma/
├── MiPrimerPrograma.csproj    # Archivo de proyecto
├── Program.cs                  # Archivo principal del código
└── obj/                       # Archivos temporales de compilación
```

#### Paso 3: Compilar y Ejecutar
```bash
# Compilar el proyecto
dotnet build

# Ejecutar el programa
dotnet run
```

### 6. Conceptos Clave Explicados

#### Namespaces
Los namespaces son como "carpetas" que organizan el código:
```csharp
using System;                    // Namespace del sistema
using System.Collections;        // Namespace para colecciones
using System.IO;                 // Namespace para entrada/salida
```

#### Clases
Las clases son plantillas para crear objetos:
```csharp
class MiClase                    // Declaración de clase
{
    // Propiedades y métodos van aquí
}
```

#### Métodos
Los métodos son funciones que realizan acciones:
```csharp
static void MiMetodo()           // Método estático que no retorna valor
{
    // Código del método
}

static int Sumar(int a, int b)   // Método que retorna un entero
{
    return a + b;                // Retorna el resultado
}
```

#### Método Main
- Es el punto de entrada del programa
- Debe ser `static` y `void`
- Se ejecuta automáticamente al iniciar el programa
- Puede recibir argumentos de línea de comandos

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Hello World Personalizado
Modifica el programa para que muestre tu nombre:
```csharp
Console.WriteLine("¡Hola, mi nombre es [Tu Nombre]!");
```

### Ejercicio 2: Múltiples Líneas
Agrega más líneas de salida:
```csharp
Console.WriteLine("Primera línea");
Console.WriteLine("Segunda línea");
Console.WriteLine("Tercera línea");
```

### Ejercicio 3: Información Personal
Crea un programa que muestre información sobre ti:
```csharp
Console.WriteLine("Nombre: [Tu Nombre]");
Console.WriteLine("Edad: [Tu Edad]");
Console.WriteLine("Profesión: [Tu Profesión]");
```

### Ejercicio 4: Cálculo Simple
Agrega un cálculo básico:
```csharp
int numero1 = 10;
int numero2 = 5;
int suma = numero1 + numero2;
Console.WriteLine($"La suma de {numero1} y {numero2} es: {suma}");
```

## 🔍 Conceptos Importantes a Recordar

1. **C# es un lenguaje orientado a objetos** que compila a código nativo
2. **.NET es la plataforma** que incluye el runtime y las librerías
3. **El método Main es el punto de entrada** de cualquier programa C#
4. **Los namespaces organizan el código** y evitan conflictos de nombres
5. **Las clases son plantillas** para crear objetos
6. **Console.WriteLine imprime texto** en la consola
7. **Cada instrucción termina con punto y coma** (;)
8. **Las llaves {} definen bloques** de código

## ❓ Preguntas de Repaso

1. ¿Qué significa "C#" y por qué se llama así?
2. ¿Cuál es la diferencia entre C# y .NET?
3. ¿Por qué es importante el método Main?
4. ¿Qué función tienen los namespaces?
5. ¿Qué hace Console.WriteLine?

## 🚀 Siguiente Paso

En la próxima clase aprenderemos sobre **Variables y Tipos de Datos**, donde veremos cómo almacenar y manipular información en nuestros programas.

---

## 📚 Recursos Adicionales

- [Documentación oficial de C#](https://docs.microsoft.com/en-us/dotnet/csharp/)
- [Tutorial de .NET](https://dotnet.microsoft.com/learn)
- [Visual Studio Code](https://code.visualstudio.com/)

---

**¡Felicidades! Has creado tu primer programa en C#! 🎉**
