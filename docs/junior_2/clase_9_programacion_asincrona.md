# Clase 9: Programación Asíncrona en C#

## 🎯 Objetivos de la Clase
- Comprender qué es la programación asíncrona y su propósito
- Aprender a usar async/await en C#
- Entender el patrón Task y Task<T>
- Dominar el manejo de operaciones asíncronas

## 📚 Contenido Teórico

### 1. ¿Qué es la Programación Asíncrona?

La **programación asíncrona** es un modelo de programación que permite ejecutar operaciones **sin bloquear** el hilo principal de la aplicación. Esto es especialmente útil para operaciones que pueden tomar tiempo, como acceso a archivos, bases de datos, APIs web, etc.

#### Ventajas de la Programación Asíncrona:
- **Responsividad**: La interfaz de usuario no se congela
- **Escalabilidad**: Mejor uso de recursos del sistema
- **Eficiencia**: No se bloquean hilos innecesariamente
- **Mejor experiencia de usuario**: Aplicaciones más fluidas

### 2. Conceptos Básicos: async/await

#### 2.1 Sintaxis Básica

```csharp
using System;
using System.Threading.Tasks;
using System.Net.Http;

// Clase que demuestra programación asíncrona básica
public class ProgramacionAsincronaBasica
{
    private readonly HttpClient httpClient;
    
    public ProgramacionAsincronaBasica()
    {
        httpClient = new HttpClient();
    }
    
    // Método asíncrono básico
    public async Task<string> ObtenerContenidoWebAsync(string url)
    {
        Console.WriteLine($"Iniciando descarga de: {url}");
        
        try
        {
            // await pausa la ejecución hasta que se complete la operación
            string contenido = await httpClient.GetStringAsync(url);
            
            Console.WriteLine($"Descarga completada. Longitud: {contenido.Length} caracteres");
            return contenido;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error en la descarga: {ex.Message}");
            return string.Empty;
        }
    }
    
    // Método que llama a métodos asíncronos
    public async Task ProcesarMultiplesUrlsAsync()
    {
        Console.WriteLine("=== PROCESANDO MÚLTIPLES URLs ===");
        
        string[] urls = {
            "https://httpbin.org/delay/1",
            "https://httpbin.org/delay/2",
            "https://httpbin.org/delay/3"
        };
        
        var tareas = new List<Task<string>>();
        
        // Iniciar todas las descargas en paralelo
        foreach (string url in urls)
        {
            tareas.Add(ObtenerContenidoWebAsync(url));
        }
        
        Console.WriteLine("Todas las descargas iniciadas. Esperando completación...");
        
        // Esperar a que todas las tareas se completen
        string[] resultados = await Task.WhenAll(tareas);
        
        Console.WriteLine($"Todas las descargas completadas. Total de resultados: {resultados.Length}");
        
        for (int i = 0; i < resultados.Length; i++)
        {
            Console.WriteLine($"URL {i + 1}: {resultados[i].Length} caracteres");
        }
    }
    
    // Método que demuestra el uso de Task.Delay
    public async Task DemostrarDelayAsync()
    {
        Console.WriteLine("=== DEMOSTRACIÓN DE DELAY ===");
        
        Console.WriteLine("Iniciando proceso...");
        
        for (int i = 1; i <= 5; i++)
        {
            Console.WriteLine($"Paso {i} completado");
            
            // Simular trabajo que toma tiempo
            await Task.Delay(1000); // Esperar 1 segundo
        }
        
        Console.WriteLine("Proceso completado!");
    }
    
    // Método que combina operaciones síncronas y asíncronas
    public async Task<string> ProcesarDatosCombinadosAsync()
    {
        Console.WriteLine("=== PROCESAMIENTO COMBINADO ===");
        
        // Operación síncrona
        Console.WriteLine("Iniciando procesamiento...");
        string resultadoInicial = "Datos iniciales procesados";
        
        // Operación asíncrona
        await Task.Delay(2000); // Simular trabajo asíncrono
        string resultadoAsincrono = "Datos asíncronos procesados";
        
        // Otra operación síncrona
        string resultadoFinal = $"{resultadoInicial} + {resultadoAsincrono}";
        
        Console.WriteLine("Procesamiento completado");
        return resultadoFinal;
    }
}

// Clase que demuestra diferentes patrones de async/await
public class PatronesAsyncAwait
{
    // Método que retorna Task (equivalente a void)
    public async Task MetodoVoidAsync()
    {
        Console.WriteLine("Iniciando método void async...");
        await Task.Delay(1000);
        Console.WriteLine("Método void async completado");
    }
    
    // Método que retorna Task<T>
    public async Task<string> MetodoConRetornoAsync()
    {
        Console.WriteLine("Iniciando método con retorno async...");
        await Task.Delay(1000);
        return "Resultado del método async";
    }
    
    // Método que retorna Task<T> con operaciones complejas
    public async Task<int> CalcularSumaAsync(int[] numeros)
    {
        Console.WriteLine("Iniciando cálculo asíncrono...");
        
        // Simular trabajo que toma tiempo
        await Task.Delay(1000);
        
        int suma = 0;
        foreach (int numero in numeros)
        {
            suma += numero;
            // Simular procesamiento individual
            await Task.Delay(100);
        }
        
        Console.WriteLine($"Cálculo completado. Suma: {suma}");
        return suma;
    }
    
    // Método que maneja excepciones en async
    public async Task<string> MetodoConManejoErroresAsync()
    {
        try
        {
            Console.WriteLine("Iniciando método con manejo de errores...");
            
            // Simular operación que puede fallar
            await Task.Delay(1000);
            
            // Simular error aleatorio
            if (new Random().Next(1, 4) == 1)
            {
                throw new InvalidOperationException("Error simulado en operación asíncrona");
            }
            
            return "Operación exitosa";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error capturado: {ex.Message}");
            return $"Error: {ex.Message}";
        }
    }
}
```

### 3. Task y Task<T>

#### 3.1 Creación y Uso de Tasks

```csharp
public class ManejoTasks
{
    // Crear Task simple
    public Task CrearTaskSimple()
    {
        return Task.Run(() =>
        {
            Console.WriteLine("Task simple ejecutándose...");
            Thread.Sleep(2000);
            Console.WriteLine("Task simple completado");
        });
    }
    
    // Crear Task<T> con retorno
    public Task<string> CrearTaskConRetorno()
    {
        return Task.Run(() =>
        {
            Console.WriteLine("Task con retorno ejecutándose...");
            Thread.Sleep(2000);
            Console.WriteLine("Task con retorno completado");
            return "Resultado del Task";
        });
    }
    
    // Crear Task con parámetros
    public Task<int> CrearTaskConParametros(int numero)
    {
        return Task.Run(() =>
        {
            Console.WriteLine($"Task con parámetro {numero} ejecutándose...");
            Thread.Sleep(1000);
            int resultado = numero * numero;
            Console.WriteLine($"Task completado. Resultado: {resultado}");
            return resultado;
        });
    }
    
    // Crear Task con cancelación
    public Task<string> CrearTaskConCancelacion(CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            Console.WriteLine("Task con cancelación iniciado...");
            
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    // Verificar si se solicitó cancelación
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    Console.WriteLine($"Paso {i + 1}/10");
                    await Task.Delay(500, cancellationToken);
                }
                
                Console.WriteLine("Task completado exitosamente");
                return "Completado";
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Task cancelado");
                return "Cancelado";
            }
        }, cancellationToken);
    }
    
    // Ejecutar múltiples Tasks en paralelo
    public async Task EjecutarTasksEnParalelo()
    {
        Console.WriteLine("=== EJECUTANDO TASKS EN PARALELO ===");
        
        var tareas = new List<Task<int>>();
        
        // Crear múltiples tasks
        for (int i = 1; i <= 5; i++)
        {
            tareas.Add(CrearTaskConParametros(i));
        }
        
        Console.WriteLine("Todas las tareas iniciadas. Esperando completación...");
        
        // Esperar a que todas se completen
        int[] resultados = await Task.WhenAll(tareas);
        
        Console.WriteLine("Todas las tareas completadas:");
        for (int i = 0; i < resultados.Length; i++)
        {
            Console.WriteLine($"Task {i + 1}: {resultados[i]}");
        }
    }
    
    // Ejecutar Tasks con timeout
    public async Task<string> EjecutarTaskConTimeout()
    {
        Console.WriteLine("=== TASK CON TIMEOUT ===");
        
        try
        {
            // Crear task que tarda 5 segundos
            var tareaLenta = Task.Run(async () =>
            {
                Console.WriteLine("Task lento iniciado...");
                await Task.Delay(5000);
                Console.WriteLine("Task lento completado");
                return "Completado después de 5 segundos";
            });
            
            // Esperar con timeout de 3 segundos
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            
            var tareaConTimeout = Task.Run(async () =>
            {
                try
                {
                    return await tareaLenta.WaitAsync(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    return "Timeout alcanzado";
                }
            });
            
            string resultado = await tareaConTimeout;
            Console.WriteLine($"Resultado: {resultado}");
            return resultado;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return $"Error: {ex.Message}";
        }
    }
}
```

### 4. Operaciones Asíncronas con Archivos

#### 4.1 Lectura y Escritura Asíncrona

```csharp
using System.IO;
using System.Text;

public class OperacionesArchivoAsync
{
    private string directorioBase;
    
    public OperacionesArchivoAsync()
    {
        directorioBase = Path.Combine(Environment.CurrentDirectory, "ArchivosAsync");
        if (!Directory.Exists(directorioBase))
        {
            Directory.CreateDirectory(directorioBase);
        }
    }
    
    // Escribir archivo de forma asíncrona
    public async Task EscribirArchivoAsync(string nombreArchivo, string contenido)
    {
        string rutaCompleta = Path.Combine(directorioBase, nombreArchivo);
        
        Console.WriteLine($"Iniciando escritura asíncrona en: {rutaCompleta}");
        
        try
        {
            // Escribir archivo de forma asíncrona
            await File.WriteAllTextAsync(rutaCompleta, contenido, Encoding.UTF8);
            
            Console.WriteLine($"Archivo escrito exitosamente: {rutaCompleta}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error escribiendo archivo: {ex.Message}");
            throw;
        }
    }
    
    // Leer archivo de forma asíncrona
    public async Task<string> LeerArchivoAsync(string nombreArchivo)
    {
        string rutaCompleta = Path.Combine(directorioBase, nombreArchivo);
        
        Console.WriteLine($"Iniciando lectura asíncrona de: {rutaCompleta}");
        
        try
        {
            // Leer archivo de forma asíncrona
            string contenido = await File.ReadAllTextAsync(rutaCompleta, Encoding.UTF8);
            
            Console.WriteLine($"Archivo leído exitosamente. Longitud: {contenido.Length} caracteres");
            return contenido;
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Archivo no encontrado: {rutaCompleta}");
            return string.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error leyendo archivo: {ex.Message}");
            throw;
        }
    }
    
    // Escribir archivo grande de forma asíncrona con buffer
    public async Task EscribirArchivoGrandeAsync(string nombreArchivo, int numeroLineas)
    {
        string rutaCompleta = Path.Combine(directorioBase, nombreArchivo);
        
        Console.WriteLine($"Iniciando escritura de archivo grande: {numeroLineas} líneas");
        
        try
        {
            using var streamWriter = new StreamWriter(rutaCompleta, false, Encoding.UTF8);
            
            for (int i = 1; i <= numeroLineas; i++)
            {
                string linea = $"Línea {i}: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - Contenido de ejemplo para línea {i}";
                await streamWriter.WriteLineAsync(linea);
                
                // Mostrar progreso cada 1000 líneas
                if (i % 1000 == 0)
                {
                    Console.WriteLine($"Progreso: {i}/{numeroLineas} líneas escritas");
                }
            }
            
            Console.WriteLine($"Archivo grande escrito exitosamente: {rutaCompleta}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error escribiendo archivo grande: {ex.Message}");
            throw;
        }
    }
    
    // Leer archivo grande de forma asíncrona línea por línea
    public async Task LeerArchivoGrandeAsync(string nombreArchivo)
    {
        string rutaCompleta = Path.Combine(directorioBase, nombreArchivo);
        
        Console.WriteLine($"Iniciando lectura de archivo grande: {rutaCompleta}");
        
        try
        {
            using var streamReader = new StreamReader(rutaCompleta, Encoding.UTF8);
            
            int contadorLineas = 0;
            string linea;
            
            while ((linea = await streamReader.ReadLineAsync()) != null)
            {
                contadorLineas++;
                
                // Mostrar progreso cada 1000 líneas
                if (contadorLineas % 1000 == 0)
                {
                    Console.WriteLine($"Progreso: {contadorLineas} líneas leídas");
                }
                
                // Procesar línea (aquí solo contamos)
            }
            
            Console.WriteLine($"Archivo grande leído exitosamente. Total de líneas: {contadorLineas}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error leyendo archivo grande: {ex.Message}");
            throw;
        }
    }
    
    // Copiar archivo de forma asíncrona
    public async Task CopiarArchivoAsync(string nombreOrigen, string nombreDestino)
    {
        string rutaOrigen = Path.Combine(directorioBase, nombreOrigen);
        string rutaDestino = Path.Combine(directorioBase, nombreDestino);
        
        Console.WriteLine($"Iniciando copia asíncrona: {rutaOrigen} -> {rutaDestino}");
        
        try
        {
            using var streamOrigen = new FileStream(rutaOrigen, FileMode.Open, FileAccess.Read);
            using var streamDestino = new FileStream(rutaDestino, FileMode.Create, FileAccess.Write);
            
            byte[] buffer = new byte[8192]; // Buffer de 8KB
            int bytesLeidos;
            long totalBytesCopiados = 0;
            
            while ((bytesLeidos = await streamOrigen.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await streamDestino.WriteAsync(buffer, 0, bytesLeidos);
                totalBytesCopiados += bytesLeidos;
                
                // Mostrar progreso
                if (totalBytesCopiados % (1024 * 1024) == 0) // Cada MB
                {
                    Console.WriteLine($"Progreso: {totalBytesCopiados / (1024 * 1024)} MB copiados");
                }
            }
            
            Console.WriteLine($"Archivo copiado exitosamente. Total: {totalBytesCopiados} bytes");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error copiando archivo: {ex.Message}");
            throw;
        }
    }
}
```

### 5. Operaciones Asíncronas con Base de Datos

#### 5.1 Simulación de Operaciones de Base de Datos

```csharp
public class OperacionesBaseDatosAsync
{
    // Simular operación de base de datos que toma tiempo
    public async Task<List<string>> ObtenerUsuariosAsync()
    {
        Console.WriteLine("Iniciando consulta de usuarios en base de datos...");
        
        // Simular tiempo de consulta
        await Task.Delay(2000);
        
        var usuarios = new List<string>
        {
            "Juan Pérez",
            "María García",
            "Carlos López",
            "Ana Martínez",
            "Luis Rodríguez"
        };
        
        Console.WriteLine($"Consulta completada. {usuarios.Count} usuarios encontrados");
        return usuarios;
    }
    
    // Simular operación de inserción
    public async Task<bool> InsertarUsuarioAsync(string nombre, string email)
    {
        Console.WriteLine($"Iniciando inserción de usuario: {nombre}");
        
        // Simular validación y tiempo de inserción
        await Task.Delay(1500);
        
        // Simular validación
        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(email))
        {
            Console.WriteLine("Error: Nombre y email son requeridos");
            return false;
        }
        
        Console.WriteLine($"Usuario {nombre} insertado exitosamente");
        return true;
    }
    
    // Simular operación de actualización
    public async Task<bool> ActualizarUsuarioAsync(int id, string nuevoNombre)
    {
        Console.WriteLine($"Iniciando actualización de usuario ID: {id}");
        
        // Simular tiempo de actualización
        await Task.Delay(1000);
        
        // Simular validación
        if (id <= 0)
        {
            Console.WriteLine("Error: ID inválido");
            return false;
        }
        
        Console.WriteLine($"Usuario ID {id} actualizado exitosamente a: {nuevoNombre}");
        return true;
    }
    
    // Simular operación de eliminación
    public async Task<bool> EliminarUsuarioAsync(int id)
    {
        Console.WriteLine($"Iniciando eliminación de usuario ID: {id}");
        
        // Simular tiempo de eliminación
        await Task.Delay(800);
        
        // Simular validación
        if (id <= 0)
        {
            Console.WriteLine("Error: ID inválido");
            return false;
        }
        
        Console.WriteLine($"Usuario ID {id} eliminado exitosamente");
        return true;
    }
    
    // Operación compleja que combina múltiples operaciones de BD
    public async Task<string> ProcesarTransaccionComplejaAsync()
    {
        Console.WriteLine("=== INICIANDO TRANSACCIÓN COMPLEJA ===");
        
        try
        {
            // Obtener usuarios existentes
            var usuarios = await ObtenerUsuariosAsync();
            
            // Insertar nuevo usuario
            bool insercionExitosa = await InsertarUsuarioAsync("Nuevo Usuario", "nuevo@email.com");
            if (!insercionExitosa)
            {
                throw new InvalidOperationException("Error en la inserción");
            }
            
            // Actualizar usuario existente
            bool actualizacionExitosa = await ActualizarUsuarioAsync(1, "Juan Pérez Actualizado");
            if (!actualizacionExitosa)
            {
                throw new InvalidOperationException("Error en la actualización");
            }
            
            // Obtener usuarios actualizados
            var usuariosActualizados = await ObtenerUsuariosAsync();
            
            Console.WriteLine("Transacción completada exitosamente");
            return $"Transacción exitosa. Total usuarios: {usuariosActualizados.Count}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en transacción: {ex.Message}");
            return $"Error en transacción: {ex.Message}";
        }
    }
    
    // Operación con timeout y cancelación
    public async Task<string> OperacionConTimeoutAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("=== OPERACIÓN CON TIMEOUT ===");
        
        try
        {
            // Combinar timeout con cancelación
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5)); // Timeout de 5 segundos
            
            // Ejecutar operación con timeout
            var tarea = Task.Run(async () =>
            {
                Console.WriteLine("Iniciando operación de base de datos...");
                
                // Simular trabajo que puede tomar tiempo
                for (int i = 1; i <= 10; i++)
                {
                    cts.Token.ThrowIfCancellationRequested();
                    
                    Console.WriteLine($"Paso {i}/10");
                    await Task.Delay(800, cts.Token);
                }
                
                return "Operación completada exitosamente";
            }, cts.Token);
            
            string resultado = await tarea;
            Console.WriteLine($"Resultado: {resultado}");
            return resultado;
        }
        catch (OperationCanceledException)
        {
            string mensaje = "Operación cancelada por timeout o cancelación";
            Console.WriteLine(mensaje);
            return mensaje;
        }
        catch (Exception ex)
        {
            string mensaje = $"Error en operación: {ex.Message}";
            Console.WriteLine(mensaje);
            return mensaje;
        }
    }
}
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Descargador de Archivos Asíncrono
```csharp
// Crear una clase DescargadorArchivos que:
// - Descargue múltiples archivos en paralelo
// - Muestre progreso de descarga
// - Maneje errores y reintentos
// - Permita cancelación de descargas
```

### Ejercicio 2: Procesador de Imágenes Asíncrono
```csharp
// Crear una clase ProcesadorImagenes que:
// - Procese múltiples imágenes en paralelo
// - Aplique filtros y transformaciones
// - Guarde resultados de forma asíncrona
// - Maneje diferentes formatos de imagen
```

### Ejercicio 3: Cliente de API Asíncrono
```csharp
// Crear una clase ClienteAPI que:
// - Realice múltiples llamadas HTTP en paralelo
// - Implemente reintentos automáticos
// - Cachee respuestas de forma asíncrona
// - Maneje rate limiting
```

## 🔍 Conceptos Importantes a Recordar

1. **async/await** simplifica la programación asíncrona
2. **Task** representa una operación asíncrona
3. **Task<T>** representa una operación asíncrona con retorno
4. **await** pausa la ejecución hasta que se complete la operación
5. **Task.WhenAll** ejecuta múltiples tareas en paralelo
6. **CancellationToken** permite cancelar operaciones asíncronas
7. **Los métodos async deben retornar Task o Task<T>**
8. **El manejo de errores** es crucial en operaciones asíncronas

## ❓ Preguntas de Repaso

1. ¿Cuál es la diferencia entre Task y Task<T>?
2. ¿Por qué es importante usar async/await en lugar de .Result?
3. ¿Cómo manejas excepciones en métodos async?
4. ¿Cuándo usarías Task.WhenAll vs. Task.WhenAny?
5. ¿Cómo implementas cancelación en operaciones asíncronas?

## 🚀 Siguiente Paso

En la próxima clase aprenderemos sobre **Reflexión y Metaprogramación en C#**, donde veremos cómo inspeccionar y manipular código en tiempo de ejecución.

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
| [Clase 7](clase_7_linq.md) | LINQ en C# | |
| [Clase 8](clase_8_archivos_streams.md) | Manejo de Archivos y Streams | ← Anterior |
| **Clase 9** | **Programación Asíncrona** | ← Estás aquí |
| [Clase 10](clase_10_reflexion_metaprogramacion.md) | Reflexión y Metaprogramación | Siguiente → |

**← [Volver al README del Módulo 2](../junior_2/README.md)**

---

## 📚 Recursos Adicionales

- [Programación asíncrona en C#](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/)
- [Async/Await en C#](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/async)
- [Task Parallel Library](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl)

---

**¡Excelente! Ahora entiendes la programación asíncrona en C#! 🎯**
