# 🚀 Clase 1: Optimización de Performance y Profiling

## 🧭 Navegación
- **⬅️ Anterior**: [Módulo 12: Clean Architecture y Microservicios](../senior_5/README.md)
- **🏠 [Volver al Módulo 6](../README.md)**
- **➡️ Siguiente**: [Clase 2: Optimización de Código y Memoria](clase_2_optimizacion_codigo.md)
- **📚 [Índice Completo](../../INDICE_COMPLETO.md)**

---

## 🎯 Objetivos de la Clase

Al finalizar esta clase, serás capaz de:
- Implementar profiling de performance en aplicaciones .NET
- Utilizar BenchmarkDotNet para medir rendimiento
- Analizar el uso de memoria y optimizarlo
- Implementar performance counters para monitoreo

---

## 📚 Contenido Teórico

### 1.1 Profiling y Análisis de Performance

El profiling de performance es fundamental para identificar cuellos de botella y optimizar aplicaciones .NET. Vamos a explorar las herramientas y técnicas más efectivas.

#### ¿Qué es el Profiling?

El profiling es el proceso de analizar el rendimiento de una aplicación para identificar:
- **Cuellos de botella**: Operaciones que consumen más tiempo
- **Uso de memoria**: Patrones de asignación y liberación
- **Llamadas a métodos**: Frecuencia y duración de ejecución
- **Recursos del sistema**: CPU, I/O, red

#### Herramientas de Profiling en .NET

1. **Visual Studio Profiler**: Integrado en Visual Studio
2. **dotnet-trace**: Herramienta de línea de comandos
3. **PerfView**: Herramienta gratuita de Microsoft
4. **JetBrains dotTrace**: Herramienta comercial avanzada

### 1.2 Benchmarking con BenchmarkDotNet

BenchmarkDotNet es la biblioteca estándar para benchmarking en .NET. Permite medir el rendimiento de diferentes implementaciones de manera precisa y confiable.

#### Instalación

```bash
dotnet add package BenchmarkDotNet
```

#### Estructura Básica de un Benchmark

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Text;

namespace PerformanceBenchmarks
{
    [MemoryDiagnoser] // Analiza el uso de memoria
    [SimpleJob] // Configuración básica del job
    public class StringConcatenationBenchmarks
    {
        private const int Iterations = 1000;
        
        [Benchmark]
        public string StringConcatenation()
        {
            // ❌ Malo: concatenación de strings ineficiente
            string result = "";
            for (int i = 0; i < Iterations; i++)
            {
                result += i.ToString(); // Crea un nuevo string cada vez
            }
            return result;
        }
        
        [Benchmark]
        public string StringBuilderConcatenation()
        {
            // ✅ Bueno: StringBuilder para múltiples concatenaciones
            var sb = new StringBuilder();
            for (int i = 0; i < Iterations; i++)
            {
                sb.Append(i.ToString());
            }
            return sb.ToString();
        }
        
        [Benchmark]
        public string StringInterpolation()
        {
            // ✅ Alternativo: interpolación de strings
            var result = new List<string>();
            for (int i = 0; i < Iterations; i++)
            {
                result.Add($"{i}");
            }
            return string.Join("", result);
        }
    }
}
```

#### Ejecución del Benchmark

```csharp
// Program.cs
class Program
{
    static void Main(string[] args)
    {
        // Ejecuta todos los benchmarks en la clase
        var summary = BenchmarkRunner.Run<StringConcatenationBenchmarks>();
        
        // También puedes ejecutar benchmarks específicos
        // var summary = BenchmarkRunner.Run(typeof(StringConcatenationBenchmarks));
    }
}
```

#### Configuración Avanzada de Benchmarks

```csharp
[MemoryDiagnoser] // Analiza memoria
[SimpleJob(RuntimeMoniker.Net80)] // Especifica versión de .NET
[SimpleJob(RuntimeMoniker.Net70)] // Compara múltiples versiones
[WarmupCount(3)] // Número de calentamientos
[IterationCount(10)] // Número de iteraciones
[MinIterationCount(5)] // Mínimo de iteraciones
[MaxIterationCount(20)] // Máximo de iteraciones
public class AdvancedBenchmarks
{
    [Benchmark]
    [Arguments(100)]
    [Arguments(1000)]
    [Arguments(10000)]
    public void ProcessData(int count)
    {
        // Benchmark con diferentes parámetros
        var data = Enumerable.Range(0, count).ToArray();
        // ... procesamiento
    }
}
```

### 1.3 Memory Profiling y Optimización

El profiling de memoria es crucial para identificar memory leaks y optimizar el uso de recursos.

#### Memory Profiler Básico

```csharp
public class MemoryProfiler
{
    public static void LogMemoryUsage(string operation)
    {
        // Obtiene información detallada del GC
        var memoryInfo = GC.GetGCMemoryInfo();
        
        // Memoria total asignada por el proceso
        var totalMemory = GC.GetTotalMemory(false);
        
        // Memoria del heap del GC
        var heapSize = memoryInfo.HeapSizeBytes;
        
        // Número de colecciones por generación
        var gen0Collections = GC.CollectionCount(0);
        var gen1Collections = GC.CollectionCount(1);
        var gen2Collections = GC.CollectionCount(2);
        
        Console.WriteLine($"[{operation}] Total Memory: {totalMemory / 1024 / 1024} MB");
        Console.WriteLine($"[{operation}] GC Heap Size: {heapSize / 1024 / 1024} MB");
        Console.WriteLine($"[{operation}] GC Collections: {gen0Collections} (Gen 0), {gen1Collections} (Gen 1), {gen2Collections} (Gen 2)");
    }
    
    public static void ForceGarbageCollection()
    {
        // Fuerza una colección completa (usar con precaución)
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
    
    public static long GetMemorySnapshot()
    {
        // Toma una instantánea de la memoria actual
        return GC.GetTotalMemory(false);
    }
}
```

#### Análisis de Objetos en Memoria

```csharp
public class ObjectMemoryAnalyzer
{
    public static void AnalyzeObjectMemory<T>(T obj, string objectName) where T : class
    {
        // Obtiene el tamaño aproximado del objeto
        var size = GetObjectSize(obj);
        
        Console.WriteLine($"[{objectName}] Tamaño aproximado: {size} bytes");
        
        // Verifica si el objeto está en el heap del GC
        if (GC.GetGeneration(obj) >= 0)
        {
            Console.WriteLine($"[{objectName}] Está en el heap del GC, Generación: {GC.GetGeneration(obj)}");
        }
    }
    
    private static long GetObjectSize(object obj)
    {
        // Método simplificado para estimar tamaño
        // En producción, usar herramientas especializadas
        if (obj is string str)
            return str.Length * 2 + 24; // UTF-16 + overhead del objeto
        
        if (obj is Array array)
            return array.Length * 8 + 24; // Estimación básica
        
        return 24; // Tamaño mínimo de un objeto
    }
}
```

### 1.4 Performance Counters

Los Performance Counters permiten monitorear métricas del sistema en tiempo real.

#### Implementación de Performance Counters

```csharp
public class PerformanceMonitor : IDisposable
{
    private readonly PerformanceCounter _cpuCounter;
    private readonly PerformanceCounter _memoryCounter;
    private readonly PerformanceCounter _diskCounter;
    private readonly PerformanceCounter _networkCounter;
    
    public PerformanceMonitor()
    {
        try
        {
            // Contador de CPU (porcentaje de uso)
            _cpuCounter = new PerformanceCounter(
                "Processor", 
                "% Processor Time", 
                "_Total"
            );
            
            // Contador de memoria disponible
            _memoryCounter = new PerformanceCounter(
                "Memory", 
                "Available MBytes"
            );
            
            // Contador de disco (lecturas por segundo)
            _diskCounter = new PerformanceCounter(
                "PhysicalDisk", 
                "Disk Reads/sec", 
                "_Total"
            );
            
            // Contador de red (bytes por segundo)
            _networkCounter = new PerformanceCounter(
                "Network Interface", 
                "Bytes Total/sec", 
                GetMainNetworkInterface()
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inicializando performance counters: {ex.Message}");
        }
    }
    
    public PerformanceMetrics GetCurrentMetrics()
    {
        return new PerformanceMetrics
        {
            CpuUsage = _cpuCounter?.NextValue() ?? 0,
            AvailableMemory = _memoryCounter?.NextValue() ?? 0,
            DiskReadsPerSecond = _diskCounter?.NextValue() ?? 0,
            NetworkBytesPerSecond = _networkCounter?.NextValue() ?? 0,
            Timestamp = DateTime.UtcNow
        };
    }
    
    public void StartMonitoring(int intervalMs = 1000)
    {
        var timer = new Timer(_ =>
        {
            var metrics = GetCurrentMetrics();
            LogMetrics(metrics);
        }, null, 0, intervalMs);
    }
    
    private void LogMetrics(PerformanceMetrics metrics)
    {
        Console.WriteLine($"[{metrics.Timestamp:HH:mm:ss}] " +
                         $"CPU: {metrics.CpuUsage:F1}% | " +
                         $"Mem: {metrics.AvailableMemory:F0} MB | " +
                         $"Disk: {metrics.DiskReadsPerSecond:F1} reads/s | " +
                         $"Net: {metrics.NetworkBytesPerSecond / 1024:F1} KB/s");
    }
    
    private string GetMainNetworkInterface()
    {
        // Obtiene la interfaz de red principal
        // En producción, usar configuración específica
        return "Intel[R] Ethernet Connection";
    }
    
    public void Dispose()
    {
        _cpuCounter?.Dispose();
        _memoryCounter?.Dispose();
        _diskCounter?.Dispose();
        _networkCounter?.Dispose();
    }
}

public class PerformanceMetrics
{
    public float CpuUsage { get; set; }
    public float AvailableMemory { get; set; }
    public float DiskReadsPerSecond { get; set; }
    public float NetworkBytesPerSecond { get; set; }
    public DateTime Timestamp { get; set; }
}
```

#### Uso de Performance Counters Personalizados

```csharp
public class CustomPerformanceCounter
{
    private readonly PerformanceCounter _counter;
    
    public CustomPerformanceCounter(string categoryName, string counterName, string instanceName = "")
    {
        try
        {
            _counter = new PerformanceCounter(categoryName, counterName, instanceName, false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creando counter personalizado: {ex.Message}");
        }
    }
    
    public void Increment()
    {
        _counter?.Increment();
    }
    
    public void IncrementBy(long value)
    {
        _counter?.IncrementBy(value);
    }
    
    public void SetValue(long value)
    {
        _counter?.RawValue = value;
    }
    
    public float GetValue()
    {
        return _counter?.NextValue() ?? 0;
    }
    
    public void Dispose()
    {
        _counter?.Dispose();
    }
}

// Uso en la aplicación
public class ApplicationMetrics
{
    private readonly CustomPerformanceCounter _requestsPerSecond;
    private readonly CustomPerformanceCounter _averageResponseTime;
    private readonly CustomPerformanceCounter _errorRate;
    
    public ApplicationMetrics()
    {
        _requestsPerSecond = new CustomPerformanceCounter("Application", "Requests/sec");
        _averageResponseTime = new CustomPerformanceCounter("Application", "Avg Response Time");
        _errorRate = new CustomPerformanceCounter("Application", "Error Rate");
    }
    
    public void RecordRequest()
    {
        _requestsPerSecond.Increment();
    }
    
    public void RecordResponseTime(long milliseconds)
    {
        _averageResponseTime.SetValue(milliseconds);
    }
    
    public void RecordError()
    {
        _errorRate.Increment();
    }
}
```

---

## 💻 Ejercicios Prácticos

### Ejercicio 1: Benchmark de Operaciones de Lista

Crea benchmarks para comparar diferentes operaciones de lista:

```csharp
[MemoryDiagnoser]
public class ListOperationsBenchmarks
{
    private List<int> _data;
    
    [GlobalSetup]
    public void Setup()
    {
        _data = Enumerable.Range(0, 10000).ToList();
    }
    
    [Benchmark]
    public List<int> FilterAndSort()
    {
        return _data
            .Where(x => x % 2 == 0)
            .OrderBy(x => x)
            .ToList();
    }
    
    [Benchmark]
    public List<int> FilterAndSortOptimized()
    {
        // Implementa una versión optimizada
        // Considera usar arrays, Span<T>, o algoritmos más eficientes
        return null; // TODO: Implementar
    }
}
```

### Ejercicio 2: Memory Profiler

Implementa un profiler de memoria que rastree objetos específicos:

```csharp
public class ObjectTracker
{
    private readonly Dictionary<string, List<WeakReference>> _trackedObjects;
    
    public void TrackObject(object obj, string category)
    {
        // Implementa el tracking de objetos
    }
    
    public void ReportMemoryUsage()
    {
        // Reporta el uso de memoria por categoría
    }
}
```

---

## 🔍 Casos de Uso Reales

### 1. Optimización de API

```csharp
[ApiController]
[Route("api/[controller]")]
public class OptimizedController : ControllerBase
{
    private readonly IMemoryProfiler _memoryProfiler;
    private readonly IPerformanceMonitor _performanceMonitor;
    
    [HttpGet("data")]
    public async Task<IActionResult> GetData()
    {
        // Inicia monitoreo de performance
        var stopwatch = Stopwatch.StartNew();
        var initialMemory = GC.GetTotalMemory(false);
        
        try
        {
            // Operación principal
            var result = await ProcessDataAsync();
            
            // Registra métricas
            var finalMemory = GC.GetTotalMemory(false);
            var memoryUsed = finalMemory - initialMemory;
            
            _performanceMonitor.RecordOperation("GetData", stopwatch.ElapsedMilliseconds, memoryUsed);
            
            return Ok(result);
        }
        finally
        {
            stopwatch.Stop();
        }
    }
}
```

### 2. Monitoreo de Background Services

```csharp
public class MonitoredBackgroundService : BackgroundService
{
    private readonly IPerformanceMonitor _performanceMonitor;
    private readonly ILogger<MonitoredBackgroundService> _logger;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var stopwatch = Stopwatch.StartNew();
            var initialMemory = GC.GetTotalMemory(false);
            
            try
            {
                await ProcessBatchAsync();
            }
            finally
            {
                stopwatch.Stop();
                var finalMemory = GC.GetTotalMemory(false);
                var memoryUsed = finalMemory - initialMemory;
                
                _performanceMonitor.RecordOperation("ProcessBatch", stopwatch.ElapsedMilliseconds, memoryUsed);
            }
            
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
```

---

## 📊 Métricas y KPIs

### Métricas de Performance Clave

1. **Response Time**: Tiempo de respuesta de las operaciones
2. **Throughput**: Operaciones por segundo
3. **Memory Usage**: Uso de memoria y patrones de GC
4. **CPU Usage**: Utilización del procesador
5. **I/O Operations**: Operaciones de disco y red

### Alertas Recomendadas

- CPU > 80% por más de 5 minutos
- Memoria > 90% del heap disponible
- Response time > 2 segundos
- Error rate > 5%

---

## 🎯 Resumen de la Clase

En esta clase hemos aprendido:

✅ **Profiling de Performance**: Herramientas y técnicas para analizar rendimiento
✅ **BenchmarkDotNet**: Biblioteca estándar para benchmarking en .NET
✅ **Memory Profiling**: Análisis del uso de memoria y optimización
✅ **Performance Counters**: Monitoreo en tiempo real de métricas del sistema
✅ **Implementación Práctica**: Código real para monitoreo de performance

---

## 🚀 Próximos Pasos

En la siguiente clase aprenderemos sobre:
- **Optimización de Código y Memoria**
- Object pooling y reducción de GC pressure
- Optimización de LINQ y estructuras de datos
- Async/await optimizado

---

## 🔗 Enlaces de Referencia

- [BenchmarkDotNet Documentation](https://benchmarkdotnet.org/)
- [.NET Performance](https://docs.microsoft.com/en-us/dotnet/fundamentals/performance/)
- [Performance Counters](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.performancecounter)
- [Memory Profiling](https://docs.microsoft.com/en-us/dotnet/core/diagnostics/memory-profiling)
