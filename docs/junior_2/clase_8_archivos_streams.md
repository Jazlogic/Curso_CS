# Clase 8: Manejo de Archivos y Streams en C#

## 🎯 Objetivos de la Clase
- Comprender cómo trabajar con archivos en C#
- Aprender a usar streams para lectura y escritura
- Entender el manejo de directorios y archivos
- Dominar la serialización y deserialización de datos

## 📚 Contenido Teórico

### 1. ¿Qué son los Streams?

Un **stream** es una abstracción que representa una secuencia de bytes que fluye de una fuente a un destino. En C#, los streams son la base para el manejo de archivos, redes, memoria y otros recursos de entrada/salida.

#### Tipos de Streams:
- **FileStream**: Para archivos del sistema
- **MemoryStream**: Para datos en memoria
- **NetworkStream**: Para comunicación de red
- **BufferedStream**: Para operaciones con buffer
- **StreamReader/StreamWriter**: Para texto

### 2. Manejo Básico de Archivos

#### 2.1 Operaciones con File y Directory

```csharp
using System;
using System.IO;
using System.Text;

// Clase que demuestra operaciones básicas con archivos
public class ManejoBasicoArchivos
{
    private string directorioBase;
    
    public ManejoBasicoArchivos()
    {
        // Crear directorio base para las demostraciones
        directorioBase = Path.Combine(Environment.CurrentDirectory, "ArchivosDemo");
        if (!Directory.Exists(directorioBase))
        {
            Directory.CreateDirectory(directorioBase);
        }
    }
    
    // Demostrar operaciones básicas de archivos
    public void DemostrarOperacionesBasicas()
    {
        Console.WriteLine("=== OPERACIONES BÁSICAS CON ARCHIVOS ===");
        
        string rutaArchivo = Path.Combine(directorioBase, "ejemplo.txt");
        
        // Verificar si existe el archivo
        if (File.Exists(rutaArchivo))
        {
            Console.WriteLine($"El archivo {rutaArchivo} ya existe");
            
            // Obtener información del archivo
            FileInfo infoArchivo = new FileInfo(rutaArchivo);
            Console.WriteLine($"Tamaño: {infoArchivo.Length} bytes");
            Console.WriteLine($"Fecha de creación: {infoArchivo.CreationTime}");
            Console.WriteLine($"Última modificación: {infoArchivo.LastWriteTime}");
            Console.WriteLine($"Solo lectura: {infoArchivo.IsReadOnly}");
        }
        else
        {
            Console.WriteLine($"El archivo {rutaArchivo} no existe");
        }
        
        // Crear un archivo de texto simple
        string contenido = "Este es un archivo de ejemplo.\nSegunda línea del archivo.\nTercera línea.";
        File.WriteAllText(rutaArchivo, contenido, Encoding.UTF8);
        Console.WriteLine($"\nArchivo creado: {rutaArchivo}");
        
        // Leer todo el contenido del archivo
        string contenidoLeido = File.ReadAllText(rutaArchivo);
        Console.WriteLine($"\nContenido del archivo:");
        Console.WriteLine(contenidoLeido);
        
        // Leer archivo línea por línea
        string[] lineas = File.ReadAllLines(rutaArchivo);
        Console.WriteLine($"\nArchivo leído línea por línea ({lineas.Length} líneas):");
        for (int i = 0; i < lineas.Length; i++)
        {
            Console.WriteLine($"Línea {i + 1}: {lineas[i]}");
        }
        
        // Agregar contenido al final del archivo
        string nuevaLinea = "\nCuarta línea agregada al final.";
        File.AppendAllText(rutaArchivo, nuevaLinea, Encoding.UTF8);
        Console.WriteLine("\nLínea agregada al final del archivo");
        
        // Leer el archivo actualizado
        string contenidoActualizado = File.ReadAllText(rutaArchivo);
        Console.WriteLine($"\nContenido actualizado:");
        Console.WriteLine(contenidoActualizado);
    }
    
    // Demostrar operaciones con directorios
    public void DemostrarOperacionesDirectorio()
    {
        Console.WriteLine("\n=== OPERACIONES CON DIRECTORIOS ===");
        
        // Crear subdirectorios
        string subDir1 = Path.Combine(directorioBase, "Subdirectorio1");
        string subDir2 = Path.Combine(directorioBase, "Subdirectorio2");
        
        if (!Directory.Exists(subDir1))
        {
            Directory.CreateDirectory(subDir1);
            Console.WriteLine($"Directorio creado: {subDir1}");
        }
        
        if (!Directory.Exists(subDir2))
        {
            Directory.CreateDirectory(subDir2);
            Console.WriteLine($"Directorio creado: {subDir2}");
        }
        
        // Crear archivos en los subdirectorios
        string archivo1 = Path.Combine(subDir1, "archivo1.txt");
        string archivo2 = Path.Combine(subDir2, "archivo2.txt");
        
        File.WriteAllText(archivo1, "Contenido del archivo 1", Encoding.UTF8);
        File.WriteAllText(archivo2, "Contenido del archivo 2", Encoding.UTF8);
        
        // Listar archivos en el directorio base
        Console.WriteLine($"\nArchivos en {directorioBase}:");
        string[] archivos = Directory.GetFiles(directorioBase);
        foreach (string archivo in archivos)
        {
            string nombreArchivo = Path.GetFileName(archivo);
            Console.WriteLine($"- {nombreArchivo}");
        }
        
        // Listar subdirectorios
        Console.WriteLine($"\nSubdirectorios en {directorioBase}:");
        string[] subdirectorios = Directory.GetDirectories(directorioBase);
        foreach (string subdir in subdirectorios)
        {
            string nombreSubdir = Path.GetFileName(subdir);
            Console.WriteLine($"- {nombreSubdir}");
        }
        
        // Obtener información del directorio
        DirectoryInfo infoDir = new DirectoryInfo(directorioBase);
        Console.WriteLine($"\nInformación del directorio base:");
        Console.WriteLine($"Ruta completa: {infoDir.FullName}");
        Console.WriteLine($"Nombre: {infoDir.Name}");
        Console.WriteLine($"Fecha de creación: {infoDir.CreationTime}");
        Console.WriteLine($"Contiene {infoDir.GetFiles().Length} archivos");
        Console.WriteLine($"Contiene {infoDir.GetDirectories().Length} subdirectorios");
    }
    
    // Demostrar operaciones de archivo con bytes
    public void DemostrarOperacionesBytes()
    {
        Console.WriteLine("\n=== OPERACIONES CON BYTES ===");
        
        string rutaArchivoBytes = Path.Combine(directorioBase, "datos.bin");
        
        // Crear array de bytes
        byte[] datosOriginales = { 65, 66, 67, 68, 69, 70, 71, 72, 73, 74 }; // A, B, C, D, E, F, G, H, I, J
        
        // Escribir bytes al archivo
        File.WriteAllBytes(rutaArchivoBytes, datosOriginales);
        Console.WriteLine($"Archivo de bytes creado: {rutaArchivoBytes}");
        
        // Leer bytes del archivo
        byte[] datosLeidos = File.ReadAllBytes(rutaArchivoBytes);
        Console.WriteLine($"Bytes leídos ({datosLeidos.Length} bytes):");
        foreach (byte b in datosLeidos)
        {
            Console.Write($"{b} ");
        }
        Console.WriteLine();
        
        // Convertir bytes a caracteres
        string texto = Encoding.ASCII.GetString(datosLeidos);
        Console.WriteLine($"Texto representado: {texto}");
        
        // Agregar más bytes al final
        byte[] bytesAdicionales = { 75, 76, 77, 78, 79 }; // K, L, M, N, O
        File.AppendAllBytes(rutaArchivoBytes, bytesAdicionales);
        
        // Leer archivo completo actualizado
        byte[] todosLosBytes = File.ReadAllBytes(rutaArchivoBytes);
        Console.WriteLine($"\nTodos los bytes ({todosLosBytes.Length} bytes):");
        foreach (byte b in todosLosBytes)
        {
            Console.Write($"{b} ");
        }
        Console.WriteLine();
        
        string textoCompleto = Encoding.ASCII.GetString(todosLosBytes);
        Console.WriteLine($"Texto completo: {textoCompleto}");
    }
}
```

### 3. Streams para Lectura y Escritura

#### 3.1 FileStream Básico

```csharp
public class ManejoStreams
{
    private string directorioBase;
    
    public ManejoStreams()
    {
        directorioBase = Path.Combine(Environment.CurrentDirectory, "ArchivosDemo");
        if (!Directory.Exists(directorioBase))
        {
            Directory.CreateDirectory(directorioBase);
        }
    }
    
    // Demostrar FileStream básico
    public void DemostrarFileStreamBasico()
    {
        Console.WriteLine("=== FILESTREAM BÁSICO ===");
        
        string rutaArchivo = Path.Combine(directorioBase, "stream.txt");
        
        // Escribir usando FileStream
        using (FileStream fileStream = new FileStream(rutaArchivo, FileMode.Create, FileAccess.Write))
        {
            string texto = "Hola Mundo desde FileStream!\nSegunda línea.\nTercera línea.";
            byte[] bytes = Encoding.UTF8.GetBytes(texto);
            
            fileStream.Write(bytes, 0, bytes.Length);
            fileStream.Flush(); // Asegurar que se escriban todos los datos
            
            Console.WriteLine($"Archivo escrito usando FileStream: {rutaArchivo}");
            Console.WriteLine($"Bytes escritos: {bytes.Length}");
        }
        
        // Leer usando FileStream
        using (FileStream fileStream = new FileStream(rutaArchivo, FileMode.Open, FileAccess.Read))
        {
            byte[] buffer = new byte[fileStream.Length];
            int bytesLeidos = fileStream.Read(buffer, 0, buffer.Length);
            
            string contenidoLeido = Encoding.UTF8.GetString(buffer, 0, bytesLeidos);
            Console.WriteLine($"\nContenido leído usando FileStream:");
            Console.WriteLine(contenidoLeido);
            Console.WriteLine($"Bytes leídos: {bytesLeidos}");
        }
    }
    
    // Demostrar FileStream con buffer
    public void DemostrarFileStreamConBuffer()
    {
        Console.WriteLine("\n=== FILESTREAM CON BUFFER ===");
        
        string rutaArchivo = Path.Combine(directorioBase, "buffer.txt");
        
        // Escribir archivo grande usando buffer
        using (FileStream fileStream = new FileStream(rutaArchivo, FileMode.Create, FileAccess.Write))
        {
            byte[] buffer = new byte[1024]; // Buffer de 1KB
            Random random = new Random();
            
            for (int i = 0; i < 10; i++) // Escribir 10KB
            {
                random.NextBytes(buffer); // Llenar buffer con bytes aleatorios
                fileStream.Write(buffer, 0, buffer.Length);
            }
            
            fileStream.Flush();
            Console.WriteLine($"Archivo de 10KB creado: {rutaArchivo}");
        }
        
        // Leer archivo usando buffer
        using (FileStream fileStream = new FileStream(rutaArchivo, FileMode.Open, FileAccess.Read))
        {
            byte[] buffer = new byte[1024]; // Buffer de 1KB
            int totalBytesLeidos = 0;
            int bytesLeidos;
            
            Console.WriteLine("Leyendo archivo por chunks de 1KB:");
            while ((bytesLeidos = fileStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                totalBytesLeidos += bytesLeidos;
                Console.WriteLine($"Chunk leído: {bytesLeidos} bytes (Total: {totalBytesLeidos} bytes)");
                
                // Mostrar los primeros 10 bytes del chunk
                Console.Write("  Primeros 10 bytes: ");
                for (int i = 0; i < Math.Min(10, bytesLeidos); i++)
                {
                    Console.Write($"{buffer[i]:X2} ");
                }
                Console.WriteLine();
            }
        }
    }
    
    // Demostrar FileStream con posicionamiento
    public void DemostrarFileStreamPosicionamiento()
    {
        Console.WriteLine("\n=== FILESTREAM CON POSICIONAMIENTO ===");
        
        string rutaArchivo = Path.Combine(directorioBase, "posicion.txt");
        
        // Crear archivo con contenido específico
        using (FileStream fileStream = new FileStream(rutaArchivo, FileMode.Create, FileAccess.Write))
        {
            string contenido = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            byte[] bytes = Encoding.ASCII.GetBytes(contenido);
            fileStream.Write(bytes, 0, bytes.Length);
            Console.WriteLine($"Archivo creado: {rutaArchivo}");
        }
        
        // Leer archivo con posicionamiento
        using (FileStream fileStream = new FileStream(rutaArchivo, FileMode.Open, FileAccess.Read))
        {
            Console.WriteLine($"\nTamaño del archivo: {fileStream.Length} bytes");
            
            // Leer desde el principio
            fileStream.Position = 0;
            byte[] buffer = new byte[5];
            fileStream.Read(buffer, 0, 5);
            string inicio = Encoding.ASCII.GetString(buffer);
            Console.WriteLine($"Primeros 5 caracteres: {inicio}");
            
            // Leer desde la posición 10
            fileStream.Position = 10;
            fileStream.Read(buffer, 0, 5);
            string medio = Encoding.ASCII.GetString(buffer);
            Console.WriteLine($"Caracteres desde posición 10: {medio}");
            
            // Leer desde el final
            fileStream.Position = fileStream.Length - 5;
            fileStream.Read(buffer, 0, 5);
            string final = Encoding.ASCII.GetString(buffer);
            Console.WriteLine($"Últimos 5 caracteres: {final}");
            
            // Leer hacia atrás
            fileStream.Position = fileStream.Length - 1;
            byte ultimoByte = (byte)fileStream.ReadByte();
            char ultimoCaracter = (char)ultimoByte;
            Console.WriteLine($"Último carácter: {ultimoCaracter}");
        }
    }
}
```

### 4. StreamReader y StreamWriter

#### 4.1 Lectura y Escritura de Texto

```csharp
public class ManejoTexto
{
    private string directorioBase;
    
    public ManejoTexto()
    {
        directorioBase = Path.Combine(Environment.CurrentDirectory, "ArchivosDemo");
        if (!Directory.Exists(directorioBase))
        {
            Directory.CreateDirectory(directorioBase);
        }
    }
    
    // Demostrar StreamWriter
    public void DemostrarStreamWriter()
    {
        Console.WriteLine("=== STREAMWRITER ===");
        
        string rutaArchivo = Path.Combine(directorioBase, "streamwriter.txt");
        
        // Escribir usando StreamWriter
        using (StreamWriter writer = new StreamWriter(rutaArchivo, false, Encoding.UTF8))
        {
            writer.WriteLine("Primera línea del archivo");
            writer.WriteLine("Segunda línea del archivo");
            writer.WriteLine("Tercera línea del archivo");
            
            // Escribir sin salto de línea
            writer.Write("Cuarta línea ");
            writer.Write("sin salto de línea");
            writer.WriteLine(); // Agregar salto de línea
            
            // Escribir con formato
            writer.WriteLine("Fecha: {0:yyyy-MM-dd}", DateTime.Now);
            writer.WriteLine("Hora: {0:HH:mm:ss}", DateTime.Now);
            
            // Escribir array de strings
            string[] lineas = { "Línea del array 1", "Línea del array 2", "Línea del array 3" };
            foreach (string linea in lineas)
            {
                writer.WriteLine(linea);
            }
            
            Console.WriteLine($"Archivo escrito usando StreamWriter: {rutaArchivo}");
        }
        
        // Verificar contenido del archivo
        string contenido = File.ReadAllText(rutaArchivo);
        Console.WriteLine($"\nContenido del archivo:");
        Console.WriteLine(contenido);
    }
    
    // Demostrar StreamReader
    public void DemostrarStreamReader()
    {
        Console.WriteLine("\n=== STREAMREADER ===");
        
        string rutaArchivo = Path.Combine(directorioBase, "streamwriter.txt");
        
        if (!File.Exists(rutaArchivo))
        {
            Console.WriteLine("El archivo no existe. Ejecuta primero DemostrarStreamWriter().");
            return;
        }
        
        // Leer usando StreamReader
        using (StreamReader reader = new StreamReader(rutaArchivo, Encoding.UTF8))
        {
            Console.WriteLine("Leyendo archivo línea por línea:");
            
            string linea;
            int numeroLinea = 1;
            while ((linea = reader.ReadLine()) != null)
            {
                Console.WriteLine($"Línea {numeroLinea}: {linea}");
                numeroLinea++;
            }
        }
        
        // Leer todo el contenido de una vez
        using (StreamReader reader = new StreamReader(rutaArchivo, Encoding.UTF8))
        {
            string contenidoCompleto = reader.ReadToEnd();
            Console.WriteLine($"\nContenido completo del archivo:");
            Console.WriteLine(contenidoCompleto);
        }
        
        // Leer caracteres individuales
        using (StreamReader reader = new StreamReader(rutaArchivo, Encoding.UTF8))
        {
            Console.WriteLine($"\nPrimeros 20 caracteres del archivo:");
            for (int i = 0; i < 20; i++)
            {
                int caracter = reader.Read();
                if (caracter == -1) break; // Fin del archivo
                
                char c = (char)caracter;
                Console.Write(c);
            }
            Console.WriteLine();
        }
    }
    
    // Demostrar StreamWriter con append
    public void DemostrarStreamWriterAppend()
    {
        Console.WriteLine("\n=== STREAMWRITER CON APPEND ===");
        
        string rutaArchivo = Path.Combine(directorioBase, "append.txt");
        
        // Crear archivo inicial
        using (StreamWriter writer = new StreamWriter(rutaArchivo, false, Encoding.UTF8))
        {
            writer.WriteLine("=== ARCHIVO INICIAL ===");
            writer.WriteLine("Esta es la primera versión del archivo");
            writer.WriteLine("Fecha de creación: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }
        
        Console.WriteLine("Archivo inicial creado");
        
        // Agregar contenido al final
        using (StreamWriter writer = new StreamWriter(rutaArchivo, true, Encoding.UTF8))
        {
            writer.WriteLine();
            writer.WriteLine("=== CONTENIDO AGREGADO ===");
            writer.WriteLine("Esta línea se agregó después");
            writer.WriteLine("Fecha de modificación: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }
        
        Console.WriteLine("Contenido agregado al final");
        
        // Mostrar archivo final
        string contenidoFinal = File.ReadAllText(rutaArchivo);
        Console.WriteLine($"\nContenido final del archivo:");
        Console.WriteLine(contenidoFinal);
    }
}
```

### 5. Serialización y Deserialización

#### 5.1 JSON y XML

```csharp
using System.Text.Json;
using System.Xml.Serialization;

// Clase para demostrar serialización
public class Persona
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public int Edad { get; set; }
    public string Email { get; set; }
    public DateTime FechaNacimiento { get; set; }
    public List<string> Hobbies { get; set; }
    
    public Persona()
    {
        Hobbies = new List<string>();
    }
    
    public Persona(int id, string nombre, string apellido, int edad, string email, DateTime fechaNacimiento)
    {
        Id = id;
        Nombre = nombre;
        Apellido = apellido;
        Edad = edad;
        Email = email;
        FechaNacimiento = fechaNacimiento;
        Hobbies = new List<string>();
    }
    
    public override string ToString()
    {
        return $"ID: {Id}, Nombre: {Nombre} {Apellido}, Edad: {Edad}, Email: {Email}, Fecha Nacimiento: {FechaNacimiento:dd/MM/yyyy}";
    }
}

public class Serializacion
{
    private string directorioBase;
    
    public Serializacion()
    {
        directorioBase = Path.Combine(Environment.CurrentDirectory, "ArchivosDemo");
        if (!Directory.Exists(directorioBase))
        {
            Directory.CreateDirectory(directorioBase);
        }
    }
    
    // Demostrar serialización JSON
    public void DemostrarSerializacionJSON()
    {
        Console.WriteLine("=== SERIALIZACIÓN JSON ===");
        
        // Crear lista de personas
        var personas = new List<Persona>
        {
            new Persona(1, "Juan", "Pérez", 25, "juan.perez@email.com", new DateTime(1998, 5, 15))
            {
                Hobbies = { "Fútbol", "Música", "Viajar" }
            },
            new Persona(2, "María", "García", 30, "maria.garcia@email.com", new DateTime(1993, 8, 22))
            {
                Hobbies = { "Pintura", "Cocina", "Lectura" }
            },
            new Persona(3, "Carlos", "López", 28, "carlos.lopez@email.com", new DateTime(1995, 12, 10))
            {
                Hobbies = { "Programación", "Videojuegos", "Cine" }
            }
        };
        
        // Serializar a JSON
        string json = JsonSerializer.Serialize(personas, new JsonSerializerOptions
        {
            WriteIndented = true, // Formato legible
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        Console.WriteLine("JSON generado:");
        Console.WriteLine(json);
        
        // Guardar JSON en archivo
        string rutaArchivo = Path.Combine(directorioBase, "personas.json");
        File.WriteAllText(rutaArchivo, json, Encoding.UTF8);
        Console.WriteLine($"\nJSON guardado en: {rutaArchivo}");
        
        // Deserializar desde archivo
        string jsonLeido = File.ReadAllText(rutaArchivo);
        var personasDeserializadas = JsonSerializer.Deserialize<List<Persona>>(jsonLeido);
        
        Console.WriteLine("\nPersonas deserializadas:");
        foreach (var persona in personasDeserializadas)
        {
            Console.WriteLine(persona);
            Console.WriteLine($"  Hobbies: {string.Join(", ", persona.Hobbies)}");
        }
    }
    
    // Demostrar serialización XML
    public void DemostrarSerializacionXML()
    {
        Console.WriteLine("\n=== SERIALIZACIÓN XML ===");
        
        // Crear persona para XML
        var persona = new Persona(4, "Ana", "Martínez", 27, "ana.martinez@email.com", new DateTime(1996, 3, 8))
        {
            Hobbies = { "Yoga", "Meditación", "Jardinería" }
        };
        
        // Serializar a XML
        var serializer = new XmlSerializer(typeof(Persona));
        string rutaArchivo = Path.Combine(directorioBase, "persona.xml");
        
        using (StreamWriter writer = new StreamWriter(rutaArchivo))
        {
            serializer.Serialize(writer, persona);
        }
        
        Console.WriteLine($"XML guardado en: {rutaArchivo}");
        
        // Mostrar contenido XML
        string contenidoXML = File.ReadAllText(rutaArchivo);
        Console.WriteLine("\nContenido XML:");
        Console.WriteLine(contenidoXML);
        
        // Deserializar desde XML
        using (StreamReader reader = new StreamReader(rutaArchivo))
        {
            var personaDeserializada = (Persona)serializer.Deserialize(reader);
            Console.WriteLine("\nPersona deserializada desde XML:");
            Console.WriteLine(personaDeserializada);
            Console.WriteLine($"  Hobbies: {string.Join(", ", personaDeserializada.Hobbies)}");
        }
    }
    
    // Demostrar serialización de lista a XML
    public void DemostrarSerializacionListaXML()
    {
        Console.WriteLine("\n=== SERIALIZACIÓN DE LISTA A XML ===");
        
        // Crear lista de personas
        var personas = new List<Persona>
        {
            new Persona(5, "Luis", "Rodríguez", 32, "luis.rodriguez@email.com", new DateTime(1991, 7, 14))
            {
                Hobbies = { "Tenis", "Natación", "Fotografía" }
            },
            new Persona(6, "Elena", "Fernández", 29, "elena.fernandez@email.com", new DateTime(1994, 11, 3))
            {
                Hobbies = { "Baile", "Teatro", "Arte" }
            }
        };
        
        // Serializar lista a XML
        var serializer = new XmlSerializer(typeof(List<Persona>));
        string rutaArchivo = Path.Combine(directorioBase, "personas.xml");
        
        using (StreamWriter writer = new StreamWriter(rutaArchivo))
        {
            serializer.Serialize(writer, personas);
        }
        
        Console.WriteLine($"Lista XML guardada en: {rutaArchivo}");
        
        // Deserializar lista desde XML
        using (StreamReader reader = new StreamReader(rutaArchivo))
        {
            var personasDeserializadas = (List<Persona>)serializer.Deserialize(reader);
            Console.WriteLine("\nPersonas deserializadas desde XML:");
            foreach (var persona in personasDeserializadas)
            {
                Console.WriteLine(persona);
                Console.WriteLine($"  Hobbies: {string.Join(", ", persona.Hobbies)}");
            }
        }
    }
}
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Sistema de Logs
```csharp
// Crear una clase Logger que escriba logs en archivos
// - Escribir logs con timestamp
// - Rotar archivos por fecha
// - Diferentes niveles de log (Info, Warning, Error)
// - Configurar directorio de logs
```

### Ejercicio 2: Gestor de Configuración
```csharp
// Crear una clase Configuracion que lea/escriba archivos de configuración
// - Soporte para JSON y XML
// - Configuración por defecto
// - Validación de configuración
// - Backup automático de configuraciones
```

### Ejercicio 3: Procesador de Archivos CSV
```csharp
// Crear una clase para procesar archivos CSV
// - Leer archivos CSV
// - Convertir a objetos
// - Escribir archivos CSV
// - Validar formato de datos
```

## 🔍 Conceptos Importantes a Recordar

1. **Los streams son la base** para el manejo de archivos en C#
2. **File y Directory** proporcionan métodos estáticos para operaciones básicas
3. **FileStream** es para operaciones de bajo nivel con bytes
4. **StreamReader/StreamWriter** son para texto con encoding
5. **Using statement** asegura la liberación de recursos
6. **La serialización** convierte objetos a formatos de almacenamiento
7. **JSON y XML** son formatos comunes para datos estructurados
8. **El manejo de errores** es crucial en operaciones de archivo

## ❓ Preguntas de Repaso

1. ¿Cuál es la diferencia entre File.WriteAllText y StreamWriter?
2. ¿Por qué es importante usar using statement con streams?
3. ¿Cuándo usarías FileStream vs. StreamReader?
4. ¿Qué ventajas tiene JSON sobre XML para serialización?
5. ¿Cómo manejas archivos grandes sin cargarlos completamente en memoria?

## 🚀 Siguiente Paso

En la próxima clase aprenderemos sobre **Programación Asíncrona en C#**, donde veremos cómo usar async/await para operaciones no bloqueantes.

---

## 📚 Navegación del Módulo 2

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_herencia.md) | Herencia en C# | |
| [Clase 2](clase_2_polimorfismo.md) | Polimorfismo y Métodos Virtuales | |
| [Clase 3](clase_3_interfaces.md) | Interfaces en C# | |
| [Clase 4](clase_4_clases_abstractas.md) | Clases Abstractas | |
| [Clase 5](clase_5_genericos.md) | Genéricos en C# | |
| [Clase 6](clase_6_delegados_eventos.md) | Delegados y Eventos | |
| [Clase 7](clase_7_linq.md) | LINQ en C# | ← Anterior |
| **Clase 8** | **Manejo de Archivos y Streams** | ← Estás aquí |
| [Clase 9](clase_9_programacion_asincrona.md) | Programación Asíncrona | Siguiente → |
| [Clase 10](clase_10_reflexion_metaprogramacion.md) | Reflexión y Metaprogramación | |

**← [Volver al README del Módulo 2](../junior_2/README.md)**

---

## 📚 Recursos Adicionales

- [Manejo de archivos en C#](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/file-system/)
- [Streams en .NET](https://docs.microsoft.com/en-us/dotnet/api/system.io.stream)
- [Serialización en C#](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/serialization/)

---

**¡Excelente! Ahora entiendes el manejo de archivos y streams en C#! 🎯**
