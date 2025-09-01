# 🚀 Clase 3: Programación Paralela y TPL

## 📋 Información de la Clase

- **Módulo**: Mid Level 1 - Programación Avanzada y Patrones de Diseño
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 2 (Programación Asíncrona Avanzada)

## 🎯 Objetivos de Aprendizaje

- Dominar la Task Parallel Library (TPL) para programación paralela
- Implementar bucles paralelos con Parallel.For y Parallel.ForEach
- Utilizar PLINQ para consultas paralelas
- Crear pipelines de procesamiento con Dataflow
- Implementar sincronización thread-safe en aplicaciones paralelas

---

## 📚 Navegación del Módulo 4

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_patrones_diseno_intermedios.md) | Patrones de Diseño Intermedios | |
| [Clase 2](clase_2_programacion_asincrona_avanzada.md) | Programación Asíncrona Avanzada | ← Anterior |
| **Clase 3** | **Programación Paralela y TPL** | ← Estás aquí |
| [Clase 4](clase_4_clean_architecture.md) | Clean Architecture | Siguiente → |
| [Clase 5](clase_5_dependency_injection.md) | Dependency Injection Avanzada | |
| [Clase 6](clase_6_logging_monitoreo.md) | Logging y Monitoreo | |
| [Clase 7](clase_7_refactoring_clean_code.md) | Refactoring y Clean Code | |
| [Clase 8](clase_8_testing_integracion.md) | Testing de Integración | |
| [Clase 9](clase_9_testing_comportamiento.md) | Testing de Comportamiento (BDD) | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de E-commerce | |

**← [Volver al README del Módulo 4](../midLevel_1/README.md)**

---

## 📚 Contenido Teórico

### 1. Task Parallel Library (TPL) - Bucles Paralelos

TPL proporciona herramientas para ejecutar código en paralelo de forma eficiente.

```csharp
// Servicio de procesamiento paralelo de datos
public class ParallelDataProcessingService
{
    // Procesamiento paralelo con Parallel.For
    public List<int> ProcessDataWithParallelFor(int start, int count, Func<int, int> processor)
    {
        var results = new int[count];
        
        var stopwatch = Stopwatch.StartNew();
        
        Parallel.For(start, start + count, i =>
        {
            var index = i - start;
            results[index] = processor(i);
            
            // Simular trabajo variable
            Thread.Sleep(Random.Shared.Next(10, 50));
        });
        
        stopwatch.Stop();
        Console.WriteLine($"Parallel.For completado en: {stopwatch.ElapsedMilliseconds}ms");
        
        return results.ToList();
    }
    
    // Procesamiento paralelo con Parallel.ForEach
    public List<string> ProcessItemsWithParallelForEach<T>(IEnumerable<T> items, Func<T, string> processor)
    {
        var results = new ConcurrentBag<string>();
        
        var stopwatch = Stopwatch.StartNew();
        
        Parallel.ForEach(items, item =>
        {
            var result = processor(item);
            results.Add(result);
            
            // Simular trabajo variable
            Thread.Sleep(Random.Shared.Next(20, 100));
        });
        
        stopwatch.Stop();
        Console.WriteLine($"Parallel.ForEach completado en: {stopwatch.ElapsedMilliseconds}ms");
        
        return results.ToList();
    }
    
    // Procesamiento paralelo con opciones personalizadas
    public List<int> ProcessWithParallelOptions(int start, int count, Func<int, int> processor)
    {
        var results = new int[count];
        
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount, // Usar todos los cores disponibles
            CancellationToken = CancellationToken.None
        };
        
        var stopwatch = Stopwatch.StartNew();
        
        Parallel.For(start, start + count, parallelOptions, i =>
        {
            var index = i - start;
            results[index] = processor(i);
            
            // Simular trabajo variable
            Thread.Sleep(Random.Shared.Next(10, 50));
        });
        
        stopwatch.Stop();
        Console.WriteLine($"Parallel.For con opciones completado en: {stopwatch.ElapsedMilliseconds}ms");
        
        return results.ToList();
    }
    
    // Procesamiento paralelo con estado compartido thread-safe
    public long ProcessWithSharedState(int start, int count, Func<int, long> processor)
    {
        var sharedSum = 0L;
        var lockObject = new object();
        
        var stopwatch = Stopwatch.StartNew();
        
        Parallel.For(start, start + count, () => 0L, // Inicialización local
            (i, loopState, localSum) =>
            {
                var result = processor(i);
                return localSum + result; // Acumular localmente
            },
            localSum =>
            {
                lock (lockObject) // Solo al final, para minimizar bloqueos
                {
                    sharedSum += localSum;
                }
            });
        
        stopwatch.Stop();
        Console.WriteLine($"Parallel.For con estado compartido completado en: {stopwatch.ElapsedMilliseconds}ms");
        
        return sharedSum;
    }
    
    // Procesamiento paralelo con cancelación
    public List<int> ProcessWithCancellation(int start, int count, Func<int, int> processor, CancellationToken cancellationToken)
    {
        var results = new int[count];
        
        var parallelOptions = new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };
        
        try
        {
            Parallel.For(start, start + count, parallelOptions, i =>
            {
                var index = i - start;
                results[index] = processor(i);
                
                // Simular trabajo variable
                Thread.Sleep(Random.Shared.Next(10, 50));
            });
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Procesamiento paralelo cancelado");
        }
        
        return results.ToList();
    }
}

// Servicio de procesamiento de imágenes en paralelo
public class ParallelImageProcessingService
{
    // Procesar múltiples imágenes en paralelo
    public List<string> ProcessImagesParallel(IEnumerable<string> imagePaths, Func<string, string> processor)
    {
        var results = new ConcurrentBag<string>();
        
        var stopwatch = Stopwatch.StartNew();
        
        Parallel.ForEach(imagePaths, imagePath =>
        {
            try
            {
                var result = processor(imagePath);
                results.Add(result);
                Console.WriteLine($"Imagen procesada: {Path.GetFileName(imagePath)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando {imagePath}: {ex.Message}");
            }
        });
        
        stopwatch.Stop();
        Console.WriteLine($"Procesamiento de {imagePaths.Count()} imágenes completado en: {stopwatch.ElapsedMilliseconds}ms");
        
        return results.ToList();
    }
    
    // Procesamiento con particionamiento personalizado
    public List<string> ProcessImagesWithCustomPartitioning(IEnumerable<string> imagePaths, Func<string, string> processor)
    {
        var results = new ConcurrentBag<string>();
        
        // Crear particiones personalizadas
        var partitions = Partitioner.Create(imagePaths.ToList(), true);
        
        var stopwatch = Stopwatch.StartNew();
        
        Parallel.ForEach(partitions, partition =>
        {
            foreach (var imagePath in partition)
            {
                try
                {
                    var result = processor(imagePath);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error procesando {imagePath}: {ex.Message}");
                }
            }
        });
        
        stopwatch.Stop();
        Console.WriteLine($"Procesamiento con particionamiento personalizado completado en: {stopwatch.ElapsedMilliseconds}ms");
        
        return results.ToList();
    }
}

// Uso de bucles paralelos
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Task Parallel Library - Bucles Paralelos ===\n");
        
        var parallelService = new ParallelDataProcessingService();
        var imageService = new ParallelImageProcessingService();
        
        // 1. Procesamiento con Parallel.For
        Console.WriteLine("1. Procesamiento con Parallel.For:");
        var parallelForResults = parallelService.ProcessDataWithParallelFor(1, 100, x => x * x);
        Console.WriteLine($"Resultados: {parallelForResults.Count} elementos procesados");
        
        Console.WriteLine();
        
        // 2. Procesamiento con Parallel.ForEach
        Console.WriteLine("2. Procesamiento con Parallel.ForEach:");
        var items = Enumerable.Range(1, 50).Select(i => $"Item_{i}");
        var parallelForEachResults = parallelService.ProcessItemsWithParallelForEach(items, item => $"Procesado_{item}");
        Console.WriteLine($"Resultados: {parallelForEachResults.Count} elementos procesados");
        
        Console.WriteLine();
        
        // 3. Procesamiento con opciones personalizadas
        Console.WriteLine("3. Procesamiento con Opciones Personalizadas:");
        var parallelOptionsResults = parallelService.ProcessWithParallelOptions(1, 50, x => x * x);
        Console.WriteLine($"Resultados: {parallelOptionsResults.Count} elementos procesados");
        
        Console.WriteLine();
        
        // 4. Procesamiento con estado compartido
        Console.WriteLine("4. Procesamiento con Estado Compartido:");
        var sharedStateResult = parallelService.ProcessWithSharedState(1, 100, x => (long)x);
        Console.WriteLine($"Suma total: {sharedStateResult}");
        
        Console.WriteLine();
        
        // 5. Procesamiento con cancelación
        Console.WriteLine("5. Procesamiento con Cancelación:");
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        var cancellationResults = parallelService.ProcessWithCancellation(1, 200, x => x * x, cts.Token);
        Console.WriteLine($"Resultados con cancelación: {cancellationResults.Count} elementos procesados");
        
        Console.WriteLine();
        
        // 6. Procesamiento de imágenes en paralelo
        Console.WriteLine("6. Procesamiento de Imágenes en Paralelo:");
        var imagePaths = new[]
        {
            "imagen1.jpg", "imagen2.jpg", "imagen3.jpg", "imagen4.jpg", "imagen5.jpg",
            "imagen6.jpg", "imagen7.jpg", "imagen8.jpg", "imagen9.jpg", "imagen10.jpg"
        };
        
        var imageResults = imageService.ProcessImagesParallel(imagePaths, path => 
        {
            // Simular procesamiento de imagen
            Thread.Sleep(Random.Shared.Next(100, 300));
            return $"Imagen procesada: {Path.GetFileName(path)}";
        });
        
        Console.WriteLine($"Imágenes procesadas: {imageResults.Count}");
    }
}
```

### 2. PLINQ - LINQ Paralelo

PLINQ permite ejecutar consultas LINQ en paralelo para mejorar el rendimiento.

```csharp
// Servicio de consultas paralelas con PLINQ
public class PLINQService
{
    // Consulta paralela básica
    public List<int> ParallelQuery(IEnumerable<int> data, Func<int, bool> filter, Func<int, int> projection)
    {
        var stopwatch = Stopwatch.StartNew();
        
        var results = data.AsParallel()
            .Where(filter)
            .Select(projection)
            .ToList();
        
        stopwatch.Stop();
        Console.WriteLine($"Consulta PLINQ completada en: {stopwatch.ElapsedMilliseconds}ms");
        
        return results;
    }
    
    // Consulta paralela con opciones personalizadas
    public List<int> ParallelQueryWithOptions(IEnumerable<int> data, Func<int, bool> filter, Func<int, int> projection)
    {
        var stopwatch = Stopwatch.StartNew();
        
        var results = data.AsParallel()
            .WithDegreeOfParallelism(Environment.ProcessorCount)
            .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
            .WithMergeOptions(ParallelMergeOptions.FullyBuffered)
            .Where(filter)
            .Select(projection)
            .ToList();
        
        stopwatch.Stop();
        Console.WriteLine($"Consulta PLINQ con opciones completada en: {stopwatch.ElapsedMilliseconds}ms");
        
        return results;
    }
    
    // Consulta paralela con cancelación
    public List<int> ParallelQueryWithCancellation(IEnumerable<int> data, Func<int, bool> filter, 
        Func<int, int> projection, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        var results = data.AsParallel()
            .WithCancellation(cancellationToken)
            .Where(filter)
            .Select(projection)
            .ToList();
        
        stopwatch.Stop();
        Console.WriteLine($"Consulta PLINQ con cancelación completada en: {stopwatch.ElapsedMilliseconds}ms");
        
        return results;
    }
    
    // Consulta paralela con orden preservado
    public List<int> ParallelQueryPreservingOrder(IEnumerable<int> data, Func<int, bool> filter, Func<int, int> projection)
    {
        var stopwatch = Stopwatch.StartNew();
        
        var results = data.AsParallel()
            .AsOrdered() // Preservar el orden original
            .Where(filter)
            .Select(projection)
            .ToList();
        
        stopwatch.Stop();
        Console.WriteLine($"Consulta PLINQ preservando orden completada en: {stopwatch.ElapsedMilliseconds}ms");
        
        return results;
    }
    
    // Consulta paralela con agregación
    public double ParallelAggregation(IEnumerable<int> data)
    {
        var stopwatch = Stopwatch.StartNew();
        
        var result = data.AsParallel()
            .Average();
        
        stopwatch.Stop();
        Console.WriteLine($"Agregación PLINQ completada en: {stopwatch.ElapsedMilliseconds}ms");
        
        return result;
    }
    
    // Consulta paralela con múltiples operaciones
    public (double Average, int Count, int Sum, int Min, int Max) ParallelMultipleOperations(IEnumerable<int> data)
    {
        var stopwatch = Stopwatch.StartNew();
        
        var results = data.AsParallel()
            .Aggregate(
                seed: new { Count = 0, Sum = 0, Min = int.MaxValue, Max = int.MinValue },
                updateAccumulatorFunc: (acc, item) => new
                {
                    Count = acc.Count + 1,
                    Sum = acc.Sum + item,
                    Min = Math.Min(acc.Min, item),
                    Max = Math.Max(acc.Max, item)
                },
                combineAccumulatorsFunc: (acc1, acc2) => new
                {
                    Count = acc1.Count + acc2.Count,
                    Sum = acc1.Sum + acc2.Sum,
                    Min = Math.Min(acc1.Min, acc2.Min),
                    Max = Math.Max(acc1.Max, acc2.Max)
                },
                resultSelector: acc => new
                {
                    acc.Count,
                    acc.Sum,
                    acc.Min,
                    acc.Max
                });
        
        var average = (double)results.Sum / results.Count;
        
        stopwatch.Stop();
        Console.WriteLine($"Múltiples operaciones PLINQ completadas en: {stopwatch.ElapsedMilliseconds}ms");
        
        return (average, results.Count, results.Sum, results.Min, results.Max);
    }
}

// Servicio de análisis de datos con PLINQ
public class DataAnalysisService
{
    // Análisis paralelo de datos grandes
    public Dictionary<string, object> AnalyzeLargeDataset(IEnumerable<int> data)
    {
        var stopwatch = Stopwatch.StartNew();
        
        var analysis = new Dictionary<string, object>();
        
        // Estadísticas básicas
        analysis["Count"] = data.AsParallel().Count();
        analysis["Sum"] = data.AsParallel().Sum();
        analysis["Average"] = data.AsParallel().Average();
        analysis["Min"] = data.AsParallel().Min();
        analysis["Max"] = data.AsParallel().Max();
        
        // Análisis de distribución
        var grouped = data.AsParallel()
            .GroupBy(x => x / 100) // Agrupar por rangos de 100
            .Select(g => new { Range = g.Key, Count = g.Count() })
            .OrderBy(x => x.Range)
            .ToList();
        
        analysis["Distribution"] = grouped;
        
        // Análisis de valores únicos
        analysis["UniqueCount"] = data.AsParallel().Distinct().Count();
        
        // Análisis de valores pares e impares
        analysis["EvenCount"] = data.AsParallel().Count(x => x % 2 == 0);
        analysis["OddCount"] = data.AsParallel().Count(x => x % 2 != 0);
        
        stopwatch.Stop();
        analysis["ProcessingTime"] = stopwatch.ElapsedMilliseconds;
        
        Console.WriteLine($"Análisis de dataset completado en: {stopwatch.ElapsedMilliseconds}ms");
        
        return analysis;
    }
    
    // Búsqueda paralela en datos
    public List<int> ParallelSearch(IEnumerable<int> data, int target, int tolerance)
    {
        var stopwatch = Stopwatch.StartNew();
        
        var results = data.AsParallel()
            .Where(x => Math.Abs(x - target) <= tolerance)
            .OrderBy(x => Math.Abs(x - target))
            .ToList();
        
        stopwatch.Stop();
        Console.WriteLine($"Búsqueda paralela completada en: {stopwatch.ElapsedMilliseconds}ms");
        
        return results;
    }
}

// Uso de PLINQ
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== PLINQ - LINQ Paralelo ===\n");
        
        var plinqService = new PLINQService();
        var analysisService = new DataAnalysisService();
        
        // Generar datos de prueba
        var data = Enumerable.Range(1, 1000000).ToList();
        
        // 1. Consulta paralela básica
        Console.WriteLine("1. Consulta PLINQ Básica:");
        var basicResults = plinqService.ParallelQuery(data, x => x % 2 == 0, x => x * 2);
        Console.WriteLine($"Resultados: {basicResults.Count} elementos");
        
        Console.WriteLine();
        
        // 2. Consulta paralela con opciones
        Console.WriteLine("2. Consulta PLINQ con Opciones:");
        var optionsResults = plinqService.ParallelQueryWithOptions(data, x => x % 3 == 0, x => x * 3);
        Console.WriteLine($"Resultados: {optionsResults.Count} elementos");
        
        Console.WriteLine();
        
        // 3. Consulta paralela con cancelación
        Console.WriteLine("3. Consulta PLINQ con Cancelación:");
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        var cancellationResults = plinqService.ParallelQueryWithCancellation(data, x => x % 5 == 0, x => x * 5, cts.Token);
        Console.WriteLine($"Resultados: {cancellationResults.Count} elementos");
        
        Console.WriteLine();
        
        // 4. Consulta paralela preservando orden
        Console.WriteLine("4. Consulta PLINQ Preservando Orden:");
        var orderedResults = plinqService.ParallelQueryPreservingOrder(data.Take(100), x => x % 7 == 0, x => x * 7);
        Console.WriteLine($"Resultados ordenados: {orderedResults.Count} elementos");
        
        Console.WriteLine();
        
        // 5. Agregación paralela
        Console.WriteLine("5. Agregación PLINQ:");
        var average = plinqService.ParallelAggregation(data);
        Console.WriteLine($"Promedio: {average:F2}");
        
        Console.WriteLine();
        
        // 6. Múltiples operaciones paralelas
        Console.WriteLine("6. Múltiples Operaciones PLINQ:");
        var multipleResults = plinqService.ParallelMultipleOperations(data.Take(10000));
        Console.WriteLine($"Promedio: {multipleResults.Average:F2}, Count: {multipleResults.Count}, Sum: {multipleResults.Sum}");
        Console.WriteLine($"Min: {multipleResults.Min}, Max: {multipleResults.Max}");
        
        Console.WriteLine();
        
        // 7. Análisis de dataset grande
        Console.WriteLine("7. Análisis de Dataset Grande:");
        var analysis = analysisService.AnalyzeLargeDataset(data.Take(100000));
        Console.WriteLine($"Count: {analysis["Count"]}, Sum: {analysis["Sum"]}");
        Console.WriteLine($"Average: {analysis["Average"]}, Min: {analysis["Min"]}, Max: {analysis["Max"]}");
        Console.WriteLine($"Unique: {analysis["UniqueCount"]}, Even: {analysis["EvenCount"]}, Odd: {analysis["OddCount"]}");
        Console.WriteLine($"Tiempo de procesamiento: {analysis["ProcessingTime"]}ms");
        
        Console.WriteLine();
        
        // 8. Búsqueda paralela
        Console.WriteLine("8. Búsqueda Paralela:");
        var searchResults = analysisService.ParallelSearch(data.Take(10000), 5000, 100);
        Console.WriteLine($"Búsqueda completada: {searchResults.Count} resultados encontrados");
        if (searchResults.Any())
        {
            Console.WriteLine($"Mejores coincidencias: {string.Join(", ", searchResults.Take(5))}");
        }
    }
}
```

### 3. Dataflow - Pipeline de Procesamiento

Dataflow permite crear pipelines de procesamiento de datos complejos y eficientes.

```csharp
// Servicio de pipeline de procesamiento con Dataflow
public class DataflowPipelineService
{
    // Pipeline simple de procesamiento
    public async Task<List<string>> SimplePipelineAsync(IEnumerable<string> input)
    {
        var buffer = new BufferBlock<string>();
        var processor = new TransformBlock<string, string>(item =>
        {
            // Simular procesamiento
            Thread.Sleep(Random.Shared.Next(50, 150));
            return $"Procesado: {item}";
        });
        
        var output = new BufferBlock<string>();
        
        // Conectar los bloques
        buffer.LinkTo(processor, new DataflowLinkOptions { PropagateCompletion = true });
        processor.LinkTo(output, new DataflowLinkOptions { PropagateCompletion = true });
        
        // Enviar datos
        foreach (var item in input)
        {
            await buffer.SendAsync(item);
        }
        
        buffer.Complete();
        
        // Recopilar resultados
        var results = new List<string>();
        while (await output.OutputAvailableAsync())
        {
            if (output.TryReceive(out var result))
            {
                results.Add(result);
            }
        }
        
        return results;
    }
    
    // Pipeline con múltiples etapas
    public async Task<List<string>> MultiStagePipelineAsync(IEnumerable<string> input)
    {
        var buffer = new BufferBlock<string>();
        
        // Etapa 1: Validación
        var validator = new TransformBlock<string, string>(item =>
        {
            if (string.IsNullOrEmpty(item))
                return null;
            return item.ToUpper();
        });
        
        // Etapa 2: Procesamiento
        var processor = new TransformBlock<string, string>(item =>
        {
            if (item == null) return null;
            Thread.Sleep(Random.Shared.Next(50, 150));
            return $"Procesado: {item}";
        });
        
        // Etapa 3: Filtrado
        var filter = new TransformBlock<string, string>(item =>
        {
            if (item == null) return null;
            if (item.Contains("ERROR")) return null;
            return item;
        });
        
        // Etapa 4: Formateo final
        var formatter = new TransformBlock<string, string>(item =>
        {
            if (item == null) return null;
            return $"[{DateTime.Now:HH:mm:ss}] {item}";
        });
        
        var output = new BufferBlock<string>();
        
        // Conectar todas las etapas
        buffer.LinkTo(validator, new DataflowLinkOptions { PropagateCompletion = true });
        validator.LinkTo(processor, new DataflowLinkOptions { PropagateCompletion = true });
        processor.LinkTo(filter, new DataflowLinkOptions { PropagateCompletion = true });
        filter.LinkTo(formatter, new DataflowLinkOptions { PropagateCompletion = true });
        formatter.LinkTo(output, new DataflowLinkOptions { PropagateCompletion = true });
        
        // Enviar datos
        foreach (var item in input)
        {
            await buffer.SendAsync(item);
        }
        
        buffer.Complete();
        
        // Recopilar resultados
        var results = new List<string>();
        while (await output.OutputAvailableAsync())
        {
            if (output.TryReceive(out var result))
            {
                results.Add(result);
            }
        }
        
        return results;
    }
    
    // Pipeline con procesamiento paralelo
    public async Task<List<string>> ParallelPipelineAsync(IEnumerable<string> input)
    {
        var buffer = new BufferBlock<string>();
        
        // Múltiples procesadores en paralelo
        var processors = new TransformBlock<string, string>[Environment.ProcessorCount];
        var merger = new BatchBlock<string>(Environment.ProcessorCount);
        
        for (int i = 0; i < processors.Length; i++)
        {
            processors[i] = new TransformBlock<string, string>(item =>
            {
                Thread.Sleep(Random.Shared.Next(50, 150));
                return $"Procesado por worker {i}: {item}";
            });
            
            buffer.LinkTo(processors[i], new DataflowLinkOptions { PropagateCompletion = true });
            processors[i].LinkTo(merger, new DataflowLinkOptions { PropagateCompletion = true });
        }
        
        var output = new BufferBlock<string>();
        merger.LinkTo(output, new DataflowLinkOptions { PropagateCompletion = true });
        
        // Enviar datos
        foreach (var item in input)
        {
            await buffer.SendAsync(item);
        }
        
        buffer.Complete();
        
        // Recopilar resultados
        var results = new List<string>();
        while (await output.OutputAvailableAsync())
        {
            if (output.TryReceive(out var result))
            {
                results.Add(result);
            }
        }
        
        return results;
    }
    
    // Pipeline con cancelación
    public async Task<List<string>> CancellablePipelineAsync(IEnumerable<string> input, CancellationToken cancellationToken)
    {
        var buffer = new BufferBlock<string>();
        
        var processor = new TransformBlock<string, string>(item =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            Thread.Sleep(Random.Shared.Next(50, 150));
            return $"Procesado: {item}";
        }, new ExecutionDataflowBlockOptions { CancellationToken = cancellationToken });
        
        var output = new BufferBlock<string>();
        
        buffer.LinkTo(processor, new DataflowLinkOptions { PropagateCompletion = true });
        processor.LinkTo(output, new DataflowLinkOptions { PropagateCompletion = true });
        
        // Enviar datos
        foreach (var item in input)
        {
            await buffer.SendAsync(item);
        }
        
        buffer.Complete();
        
        // Recopilar resultados
        var results = new List<string>();
        try
        {
            while (await output.OutputAvailableAsync())
            {
                if (output.TryReceive(out var result))
                {
                    results.Add(result);
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Pipeline cancelado");
        }
        
        return results;
    }
}

// Uso de Dataflow
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Dataflow - Pipeline de Procesamiento ===\n");
        
        var dataflowService = new DataflowPipelineService();
        
        var input = new[]
        {
            "Item_1", "Item_2", "Item_3", "Item_4", "Item_5",
            "Item_6", "Item_7", "Item_8", "Item_9", "Item_10"
        };
        
        // 1. Pipeline simple
        Console.WriteLine("1. Pipeline Simple:");
        var simpleResults = await dataflowService.SimplePipelineAsync(input);
        Console.WriteLine($"Resultados: {simpleResults.Count} elementos procesados");
        
        Console.WriteLine();
        
        // 2. Pipeline multi-etapa
        Console.WriteLine("2. Pipeline Multi-Etapa:");
        var multiStageResults = await dataflowService.MultiStagePipelineAsync(input);
        Console.WriteLine($"Resultados: {multiStageResults.Count} elementos procesados");
        
        Console.WriteLine();
        
        // 3. Pipeline paralelo
        Console.WriteLine("3. Pipeline Paralelo:");
        var parallelResults = await dataflowService.ParallelPipelineAsync(input);
        Console.WriteLine($"Resultados: {parallelResults.Count} elementos procesados");
        
        Console.WriteLine();
        
        // 4. Pipeline con cancelación
        Console.WriteLine("4. Pipeline con Cancelación:");
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var cancellableResults = await dataflowService.CancellablePipelineAsync(input, cts.Token);
        Console.WriteLine($"Resultados: {cancellableResults.Count} elementos procesados");
        
        Console.WriteLine();
        
        // Mostrar algunos resultados
        Console.WriteLine("=== Muestra de Resultados ===");
        Console.WriteLine("Pipeline Simple:");
        foreach (var result in simpleResults.Take(3))
        {
            Console.WriteLine($"  {result}");
        }
        
        Console.WriteLine("\nPipeline Multi-Etapa:");
        foreach (var result in multiStageResults.Take(3))
        {
            Console.WriteLine($"  {result}");
        }
        
        Console.WriteLine("\nPipeline Paralelo:");
        foreach (var result in parallelResults.Take(3))
        {
            Console.WriteLine($"  {result}");
        }
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Sistema de Procesamiento de Archivos Paralelo
Implementa un sistema que procese múltiples archivos en paralelo usando TPL y PLINQ.

### Ejercicio 2: Pipeline de Transformación de Datos
Crea un pipeline con Dataflow que transforme datos en múltiples etapas con validación y filtrado.

### Ejercicio 3: Sistema de Análisis de Rendimiento
Desarrolla un sistema que compare el rendimiento de operaciones secuenciales vs. paralelas.

## 🔍 Puntos Clave

1. **TPL** proporciona bucles paralelos eficientes con Parallel.For y Parallel.ForEach
2. **PLINQ** permite ejecutar consultas LINQ en paralelo para mejorar el rendimiento
3. **Dataflow** crea pipelines de procesamiento complejos y escalables
4. **Sincronización** es crucial en aplicaciones paralelas para evitar condiciones de carrera
5. **Configuración** de paralelismo debe ajustarse según el hardware disponible

## 📚 Recursos Adicionales

- [Task Parallel Library - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/)
- [PLINQ - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/parallel-linq-plinq)
- [Dataflow - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library)

---

**🎯 ¡Has completado la Clase 3! Ahora dominas la programación paralela y TPL en C#**

**📚 [Siguiente: Clase 4 - Clean Architecture](clase_4_clean_architecture.md)**
