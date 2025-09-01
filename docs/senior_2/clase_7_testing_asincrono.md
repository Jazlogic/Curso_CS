# 🚀 Clase 7: Testing de Código Asíncrono

## 📋 Información de la Clase

- **Módulo**: Senior Level 2 - Testing y TDD
- **Duración**: 2 horas
- **Nivel**: Avanzado
- **Prerrequisitos**: Frameworks de Mocking (Clase 6)

## 🎯 Objetivos de Aprendizaje

- Implementar testing de código asíncrono
- Manejar Tasks y async/await en pruebas
- Probar operaciones concurrentes
- Implementar testing de timeouts y cancelación

---

## 📚 Navegación del Módulo 2

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_fundamentos_testing.md) | Fundamentos de Testing | |
| [Clase 2](clase_2_desarrollo_dirigido_pruebas.md) | Desarrollo Dirigido por Pruebas (TDD) | |
| [Clase 3](clase_3_testing_unitario.md) | Testing Unitario | |
| [Clase 4](clase_4_testing_integracion.md) | Testing de Integración | |
| [Clase 5](clase_5_testing_comportamiento.md) | Testing de Comportamiento | |
| [Clase 6](clase_6_mocking_frameworks.md) | Frameworks de Mocking | ← Anterior |
| **Clase 7** | **Testing de Código Asíncrono** | ← Estás aquí |
| [Clase 8](clase_8_testing_apis.md) | Testing de APIs | Siguiente → |
| [Clase 9](clase_9_testing_database.md) | Testing de Base de Datos | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Testing | |

**← [Volver al README del Módulo 2](../senior_2/README.md)**

---

## 📚 Contenido Teórico

### 1. ¿Qué es el Testing Asíncrono?

El testing asíncrono permite probar código que utiliza `async/await`, `Task<T>`, y operaciones concurrentes, asegurando que las operaciones asíncronas se completen correctamente.

### 2. Características del Testing Asíncrono

- **Manejo de Tasks** y operaciones asíncronas
- **Testing de concurrencia** y paralelismo
- **Manejo de timeouts** y cancelación
- **Verificación de estados** asíncronos

```csharp
// ===== TESTING DE CÓDIGO ASÍNCRONO - IMPLEMENTACIÓN COMPLETA =====
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace AsyncTesting
{
    // ===== INTERFACES Y MODELOS =====
    namespace Interfaces
    {
        public interface IDataProcessor
        {
            Task<ProcessResult> ProcessDataAsync(string data, CancellationToken cancellationToken = default);
            Task<IEnumerable<ProcessResult>> ProcessBatchAsync(IEnumerable<string> dataItems, CancellationToken cancellationToken = default);
            Task<bool> ValidateDataAsync(string data, TimeSpan timeout);
        }
        
        public interface IAsyncRepository<T>
        {
            Task<T> GetByIdAsync(int id, CancellationToken cancellationToken = default);
            Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
            Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);
            Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default);
            Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        }
        
        public interface IAsyncCacheService
        {
            Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default);
            Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
            Task RemoveAsync(string key, CancellationToken cancellationToken = default);
            Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
        }
        
        public interface IAsyncNotificationService
        {
            Task<bool> SendNotificationAsync(string message, string recipient, CancellationToken cancellationToken = default);
            Task<IEnumerable<bool>> SendBulkNotificationsAsync(IEnumerable<string> messages, string recipient, CancellationToken cancellationToken = default);
            Task<bool> SendNotificationWithRetryAsync(string message, string recipient, int maxRetries, CancellationToken cancellationToken = default);
        }
    }
    
    // ===== MODELOS DE DOMINIO =====
    namespace Models
    {
        public class ProcessResult
        {
            public string Id { get; set; }
            public string InputData { get; set; }
            public string ProcessedData { get; set; }
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public DateTime ProcessedAt { get; set; }
            public TimeSpan ProcessingTime { get; set; }
        }
        
        public class DataItem
        {
            public int Id { get; set; }
            public string Content { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool IsProcessed { get; set; }
            public DateTime? ProcessedAt { get; set; }
        }
        
        public class NotificationRequest
        {
            public string Message { get; set; }
            public string Recipient { get; set; }
            public int Priority { get; set; }
            public DateTime RequestedAt { get; set; }
        }
        
        public class ProcessingJob
        {
            public Guid Id { get; set; }
            public string Status { get; set; }
            public int Progress { get; set; }
            public DateTime StartedAt { get; set; }
            public DateTime? CompletedAt { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
        }
    }
    
    // ===== SERVICIOS ASÍNCRONOS =====
    namespace Services
    {
        public interface IAsyncDataService
        {
            Task<ProcessResult> ProcessSingleItemAsync(string data, CancellationToken cancellationToken = default);
            Task<IEnumerable<ProcessResult>> ProcessMultipleItemsAsync(IEnumerable<string> items, CancellationToken cancellationToken = default);
            Task<ProcessingJob> StartProcessingJobAsync(IEnumerable<string> items, CancellationToken cancellationToken = default);
            Task<ProcessingJob> GetJobStatusAsync(Guid jobId, CancellationToken cancellationToken = default);
            Task<bool> CancelJobAsync(Guid jobId, CancellationToken cancellationToken = default);
        }
        
        public class AsyncDataService : IAsyncDataService
        {
            private readonly IDataProcessor _dataProcessor;
            private readonly IAsyncRepository<DataItem> _repository;
            private readonly IAsyncCacheService _cache;
            private readonly IAsyncNotificationService _notificationService;
            private readonly Dictionary<Guid, ProcessingJob> _activeJobs;
            
            public AsyncDataService(
                IDataProcessor dataProcessor,
                IAsyncRepository<DataItem> repository,
                IAsyncCacheService cache,
                IAsyncNotificationService notificationService)
            {
                _dataProcessor = dataProcessor;
                _repository = repository;
                _cache = cache;
                _notificationService = notificationService;
                _activeJobs = new Dictionary<Guid, ProcessingJob>();
            }
            
            public async Task<ProcessResult> ProcessSingleItemAsync(string data, CancellationToken cancellationToken = default)
            {
                try
                {
                    // Verificar cache primero
                    var cacheKey = $"processed:{data.GetHashCode()}";
                    var cachedResult = await _cache.GetAsync<ProcessResult>(cacheKey, cancellationToken);
                    if (cachedResult != null)
                    {
                        return cachedResult;
                    }
                    
                    // Procesar datos
                    var startTime = DateTime.Now;
                    var result = await _dataProcessor.ProcessDataAsync(data, cancellationToken);
                    var processingTime = DateTime.Now - startTime;
                    
                    result.ProcessingTime = processingTime;
                    result.ProcessedAt = DateTime.Now;
                    
                    // Cachear resultado
                    await _cache.SetAsync(cacheKey, result, TimeSpan.FromHours(1), cancellationToken);
                    
                    // Guardar en repositorio
                    var dataItem = new DataItem
                    {
                        Content = data,
                        IsProcessed = result.Success,
                        ProcessedAt = result.Success ? DateTime.Now : (DateTime?)null
                    };
                    
                    await _repository.CreateAsync(dataItem, cancellationToken);
                    
                    return result;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    return new ProcessResult
                    {
                        Id = Guid.NewGuid().ToString(),
                        InputData = data,
                        Success = false,
                        ErrorMessage = ex.Message,
                        ProcessedAt = DateTime.Now
                    };
                }
            }
            
            public async Task<IEnumerable<ProcessResult>> ProcessMultipleItemsAsync(IEnumerable<string> items, CancellationToken cancellationToken = default)
            {
                var results = new List<ProcessResult>();
                var tasks = new List<Task<ProcessResult>>();
                
                foreach (var item in items)
                {
                    var task = ProcessSingleItemAsync(item, cancellationToken);
                    tasks.Add(task);
                }
                
                // Esperar a que todas las tareas se completen
                var completedResults = await Task.WhenAll(tasks);
                results.AddRange(completedResults);
                
                return results;
            }
            
            public async Task<ProcessingJob> StartProcessingJobAsync(IEnumerable<string> items, CancellationToken cancellationToken = default)
            {
                var job = new ProcessingJob
                {
                    Id = Guid.NewGuid(),
                    Status = "Running",
                    Progress = 0,
                    StartedAt = DateTime.Now
                };
                
                _activeJobs[job.Id] = job;
                
                // Ejecutar procesamiento en background
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var itemsList = items.ToList();
                        var totalItems = itemsList.Count;
                        
                        for (int i = 0; i < totalItems; i++)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            
                            var item = itemsList[i];
                            await ProcessSingleItemAsync(item, cancellationToken);
                            
                            job.Progress = (i + 1) * 100 / totalItems;
                            
                            // Simular delay para mostrar progreso
                            await Task.Delay(100, cancellationToken);
                        }
                        
                        job.Status = "Completed";
                        job.CompletedAt = DateTime.Now;
                        
                        // Enviar notificación de completado
                        await _notificationService.SendNotificationAsync(
                            $"Job {job.Id} completed successfully", 
                            "admin@example.com", 
                            cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        job.Status = "Cancelled";
                        job.CompletedAt = DateTime.Now;
                    }
                    catch (Exception ex)
                    {
                        job.Status = "Failed";
                        job.CompletedAt = DateTime.Now;
                        job.Errors.Add(ex.Message);
                        
                        // Enviar notificación de error
                        await _notificationService.SendNotificationAsync(
                            $"Job {job.Id} failed: {ex.Message}", 
                            "admin@example.com", 
                            cancellationToken);
                    }
                }, cancellationToken);
                
                return job;
            }
            
            public async Task<ProcessingJob> GetJobStatusAsync(Guid jobId, CancellationToken cancellationToken = default)
            {
                if (_activeJobs.TryGetValue(jobId, out var job))
                {
                    return job;
                }
                
                return null;
            }
            
            public async Task<bool> CancelJobAsync(Guid jobId, CancellationToken cancellationToken = default)
            {
                if (_activeJobs.TryGetValue(jobId, out var job) && job.Status == "Running")
                {
                    job.Status = "Cancelling";
                    return true;
                }
                
                return false;
            }
        }
        
        public class AsyncValidationService
        {
            private readonly IDataProcessor _dataProcessor;
            
            public AsyncValidationService(IDataProcessor dataProcessor)
            {
                _dataProcessor = dataProcessor;
            }
            
            public async Task<bool> ValidateDataWithTimeoutAsync(string data, TimeSpan timeout)
            {
                using var cts = new CancellationTokenSource(timeout);
                
                try
                {
                    var result = await _dataProcessor.ValidateDataAsync(data, timeout);
                    return result;
                }
                catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
                {
                    throw new TimeoutException($"Validation timed out after {timeout.TotalSeconds} seconds");
                }
            }
            
            public async Task<IEnumerable<bool>> ValidateMultipleItemsWithRetryAsync(
                IEnumerable<string> items, 
                int maxRetries, 
                TimeSpan delayBetweenRetries,
                CancellationToken cancellationToken = default)
            {
                var results = new List<bool>();
                
                foreach (var item in items)
                {
                    var success = false;
                    var attempts = 0;
                    
                    while (!success && attempts < maxRetries && !cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            success = await _dataProcessor.ValidateDataAsync(item, TimeSpan.FromSeconds(5));
                            if (success) break;
                        }
                        catch (Exception)
                        {
                            // Log error but continue with retry
                        }
                        
                        attempts++;
                        if (attempts < maxRetries)
                        {
                            await Task.Delay(delayBetweenRetries, cancellationToken);
                        }
                    }
                    
                    results.Add(success);
                }
                
                return results;
            }
        }
    }
    
    // ===== TESTING ASÍNCRONO BÁSICO =====
    namespace BasicAsyncTests
    {
        public class AsyncDataServiceBasicTests
        {
            private readonly Mock<IDataProcessor> _mockDataProcessor;
            private readonly Mock<IAsyncRepository<DataItem>> _mockRepository;
            private readonly Mock<IAsyncCacheService> _mockCache;
            private readonly Mock<IAsyncNotificationService> _mockNotificationService;
            private readonly AsyncDataService _dataService;
            
            public AsyncDataServiceBasicTests()
            {
                _mockDataProcessor = new Mock<IDataProcessor>();
                _mockRepository = new Mock<IAsyncRepository<DataItem>>();
                _mockCache = new Mock<IAsyncCacheService>();
                _mockNotificationService = new Mock<IAsyncNotificationService>();
                
                _dataService = new AsyncDataService(
                    _mockDataProcessor.Object,
                    _mockRepository.Object,
                    _mockCache.Object,
                    _mockNotificationService.Object);
            }
            
            [Fact]
            public async Task ProcessSingleItem_WithValidData_ShouldSucceed()
            {
                // Arrange
                var inputData = "test data";
                var expectedResult = new ProcessResult
                {
                    Id = "1",
                    InputData = inputData,
                    ProcessedData = "processed test data",
                    Success = true
                };
                
                _mockCache.Setup(c => c.GetAsync<ProcessResult>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((ProcessResult)null);
                
                _mockDataProcessor.Setup(p => p.ProcessDataAsync(inputData, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedResult);
                
                _mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<ProcessResult>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                
                _mockRepository.Setup(r => r.CreateAsync(It.IsAny<DataItem>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                
                // Act
                var result = await _dataService.ProcessSingleItemAsync(inputData);
                
                // Assert
                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.Equal(inputData, result.InputData);
                Assert.True(result.ProcessedAt > DateTime.MinValue);
                Assert.True(result.ProcessingTime >= TimeSpan.Zero);
            }
            
            [Fact]
            public async Task ProcessSingleItem_WithCachedResult_ShouldReturnFromCache()
            {
                // Arrange
                var inputData = "cached data";
                var cachedResult = new ProcessResult
                {
                    Id = "1",
                    InputData = inputData,
                    Success = true
                };
                
                _mockCache.Setup(c => c.GetAsync<ProcessResult>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(cachedResult);
                
                // Act
                var result = await _dataService.ProcessSingleItemAsync(inputData);
                
                // Assert
                Assert.Equal(cachedResult, result);
                
                // Verificar que no se llamó al procesador
                _mockDataProcessor.Verify(p => p.ProcessDataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            }
            
            [Fact]
            public async Task ProcessMultipleItems_WithValidItems_ShouldProcessAll()
            {
                // Arrange
                var items = new[] { "item1", "item2", "item3" };
                var expectedResults = items.Select((item, index) => new ProcessResult
                {
                    Id = index.ToString(),
                    InputData = item,
                    Success = true
                }).ToList();
                
                _mockCache.Setup(c => c.GetAsync<ProcessResult>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((ProcessResult)null);
                
                _mockDataProcessor.Setup(p => p.ProcessDataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .Returns<string, CancellationToken>((data, token) => 
                        Task.FromResult(expectedResults.First(r => r.InputData == data)));
                
                _mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<ProcessResult>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                
                _mockRepository.Setup(r => r.CreateAsync(It.IsAny<DataItem>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                
                // Act
                var results = await _dataService.ProcessMultipleItemsAsync(items);
                
                // Assert
                Assert.NotNull(results);
                Assert.Equal(items.Length, results.Count());
                Assert.All(results, r => Assert.True(r.Success));
            }
        }
    }
    
    // ===== TESTING DE CANCELACIÓN =====
    namespace CancellationTests
    {
        public class AsyncDataServiceCancellationTests
        {
            private readonly Mock<IDataProcessor> _mockDataProcessor;
            private readonly Mock<IAsyncRepository<DataItem>> _mockRepository;
            private readonly Mock<IAsyncCacheService> _mockCache;
            private readonly Mock<IAsyncNotificationService> _mockNotificationService;
            private readonly AsyncDataService _dataService;
            
            public AsyncDataServiceCancellationTests()
            {
                _mockDataProcessor = new Mock<IDataProcessor>();
                _mockRepository = new Mock<IAsyncRepository<DataItem>>();
                _mockCache = new Mock<IAsyncCacheService>();
                _mockNotificationService = new Mock<IAsyncNotificationService>();
                
                _dataService = new AsyncDataService(
                    _mockDataProcessor.Object,
                    _mockRepository.Object,
                    _mockCache.Object,
                    _mockNotificationService.Object);
            }
            
            [Fact]
            public async Task ProcessSingleItem_WithCancellation_ShouldThrowOperationCanceledException()
            {
                // Arrange
                var inputData = "test data";
                var cts = new CancellationTokenSource();
                
                _mockCache.Setup(c => c.GetAsync<ProcessResult>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((ProcessResult)null);
                
                _mockDataProcessor.Setup(p => p.ProcessDataAsync(inputData, It.IsAny<CancellationToken>()))
                    .Returns<string, CancellationToken>(async (data, token) =>
                    {
                        await Task.Delay(1000, token); // Simular trabajo largo
                        return new ProcessResult { Success = true };
                    });
                
                // Act & Assert
                cts.CancelAfter(100); // Cancelar después de 100ms
                
                await Assert.ThrowsAsync<OperationCanceledException>(() =>
                    _dataService.ProcessSingleItemAsync(inputData, cts.Token));
            }
            
            [Fact]
            public async Task StartProcessingJob_WithCancellation_ShouldCancelJob()
            {
                // Arrange
                var items = new[] { "item1", "item2", "item3" };
                var cts = new CancellationTokenSource();
                
                _mockCache.Setup(c => c.GetAsync<ProcessResult>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((ProcessResult)null);
                
                _mockDataProcessor.Setup(p => p.ProcessDataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .Returns<string, CancellationToken>(async (data, token) =>
                    {
                        await Task.Delay(100, token); // Simular trabajo
                        return new ProcessResult { Success = true };
                    });
                
                _mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<ProcessResult>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                
                _mockRepository.Setup(r => r.CreateAsync(It.IsAny<DataItem>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                
                // Act
                var job = await _dataService.StartProcessingJobAsync(items, cts.Token);
                
                // Esperar un poco para que el job comience
                await Task.Delay(200);
                
                // Cancelar el job
                var cancelResult = await _dataService.CancelJobAsync(job.Id);
                
                // Esperar a que se complete la cancelación
                await Task.Delay(200);
                
                var finalJobStatus = await _dataService.GetJobStatusAsync(job.Id);
                
                // Assert
                Assert.True(cancelResult);
                Assert.Equal("Cancelled", finalJobStatus.Status);
            }
        }
    }
    
    // ===== TESTING DE TIMEOUTS =====
    namespace TimeoutTests
    {
        public class AsyncValidationServiceTimeoutTests
        {
            private readonly Mock<IDataProcessor> _mockDataProcessor;
            private readonly AsyncValidationService _validationService;
            
            public AsyncValidationServiceTimeoutTests()
            {
                _mockDataProcessor = new Mock<IDataProcessor>();
                _validationService = new AsyncValidationService(_mockDataProcessor.Object);
            }
            
            [Fact]
            public async Task ValidateDataWithTimeout_WithinTimeout_ShouldSucceed()
            {
                // Arrange
                var data = "valid data";
                var timeout = TimeSpan.FromSeconds(2);
                
                _mockDataProcessor.Setup(p => p.ValidateDataAsync(data, timeout))
                    .ReturnsAsync(true);
                
                // Act
                var result = await _validationService.ValidateDataWithTimeoutAsync(data, timeout);
                
                // Assert
                Assert.True(result);
            }
            
            [Fact]
            public async Task ValidateDataWithTimeout_ExceedingTimeout_ShouldThrowTimeoutException()
            {
                // Arrange
                var data = "slow data";
                var timeout = TimeSpan.FromMilliseconds(100);
                
                _mockDataProcessor.Setup(p => p.ValidateDataAsync(data, timeout))
                    .Returns<string, TimeSpan>(async (d, t) =>
                    {
                        await Task.Delay(500); // Simular operación lenta
                        return true;
                    });
                
                // Act & Assert
                var exception = await Assert.ThrowsAsync<TimeoutException>(() =>
                    _validationService.ValidateDataWithTimeoutAsync(data, timeout));
                
                Assert.Contains("timed out", exception.Message);
            }
        }
    }
    
    // ===== TESTING DE CONCURRENCIA =====
    namespace ConcurrencyTests
    {
        public class AsyncDataServiceConcurrencyTests
        {
            private readonly Mock<IDataProcessor> _mockDataProcessor;
            private readonly Mock<IAsyncRepository<DataItem>> _mockRepository;
            private readonly Mock<IAsyncCacheService> _mockCache;
            private readonly Mock<IAsyncNotificationService> _mockNotificationService;
            private readonly AsyncDataService _dataService;
            
            public AsyncDataServiceConcurrencyTests()
            {
                _mockDataProcessor = new Mock<IDataProcessor>();
                _mockRepository = new Mock<IAsyncRepository<DataItem>>();
                _mockCache = new Mock<IAsyncCacheService>();
                _mockNotificationService = new Mock<IAsyncNotificationService>();
                
                _dataService = new AsyncDataService(
                    _mockDataProcessor.Object,
                    _mockRepository.Object,
                    _mockCache.Object,
                    _mockNotificationService.Object);
            }
            
            [Fact]
            public async Task ProcessMultipleItems_Concurrently_ShouldHandleRaceConditions()
            {
                // Arrange
                var items = Enumerable.Range(1, 10).Select(i => $"item{i}").ToList();
                var processingOrder = new List<string>();
                var lockObject = new object();
                
                _mockCache.Setup(c => c.GetAsync<ProcessResult>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((ProcessResult)null);
                
                _mockDataProcessor.Setup(p => p.ProcessDataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .Returns<string, CancellationToken>(async (data, token) =>
                    {
                        // Simular procesamiento con orden variable
                        await Task.Delay(Random.Shared.Next(10, 100), token);
                        
                        lock (lockObject)
                        {
                            processingOrder.Add(data);
                        }
                        
                        return new ProcessResult
                        {
                            Id = Guid.NewGuid().ToString(),
                            InputData = data,
                            Success = true
                        };
                    });
                
                _mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<ProcessResult>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                
                _mockRepository.Setup(r => r.CreateAsync(It.IsAny<DataItem>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                
                // Act
                var results = await _dataService.ProcessMultipleItemsAsync(items);
                
                // Assert
                Assert.NotNull(results);
                Assert.Equal(items.Count, results.Count());
                Assert.Equal(items.Count, processingOrder.Count);
                
                // Verificar que todos los items fueron procesados
                var processedItems = results.Select(r => r.InputData).ToList();
                Assert.All(items, item => Assert.Contains(item, processedItems));
            }
            
            [Fact]
            public async Task MultipleJobs_Concurrently_ShouldNotInterfere()
            {
                // Arrange
                var items1 = new[] { "job1_item1", "job1_item2" };
                var items2 = new[] { "job2_item1", "job2_item2" };
                
                _mockCache.Setup(c => c.GetAsync<ProcessResult>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((ProcessResult)null);
                
                _mockDataProcessor.Setup(p => p.ProcessDataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .Returns<string, CancellationToken>(async (data, token) =>
                    {
                        await Task.Delay(50, token);
                        return new ProcessResult
                        {
                            Id = Guid.NewGuid().ToString(),
                            InputData = data,
                            Success = true
                        };
                    });
                
                _mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<ProcessResult>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                
                _mockRepository.Setup(r => r.CreateAsync(It.IsAny<DataItem>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                
                _mockNotificationService.Setup(n => n.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);
                
                // Act - Iniciar dos jobs simultáneamente
                var job1 = await _dataService.StartProcessingJobAsync(items1);
                var job2 = await _dataService.StartProcessingJobAsync(items2);
                
                // Esperar a que ambos jobs se completen
                await Task.Delay(500);
                
                var finalJob1Status = await _dataService.GetJobStatusAsync(job1.Id);
                var finalJob2Status = await _dataService.GetJobStatusAsync(job2.Id);
                
                // Assert
                Assert.NotNull(finalJob1Status);
                Assert.NotNull(finalJob2Status);
                Assert.Equal("Completed", finalJob1Status.Status);
                Assert.Equal("Completed", finalJob2Status.Status);
                Assert.Equal(100, finalJob1Status.Progress);
                Assert.Equal(100, finalJob2Status.Progress);
            }
        }
    }
}

// ===== DEMOSTRACIÓN DE TESTING ASÍNCRONO =====
public class AsyncTestingDemonstration
{
    public static async Task DemonstrateAsyncTesting()
    {
        Console.WriteLine("=== Testing de Código Asíncrono - Clase 7 ===\n");
        
        Console.WriteLine("1. CREANDO SERVICIOS ASÍNCRONOS:");
        var mockDataProcessor = new Moq.Mock<AsyncTesting.Interfaces.IDataProcessor>();
        var mockRepository = new Moq.Mock<AsyncTesting.Interfaces.IAsyncRepository<AsyncTesting.Models.DataItem>>();
        var mockCache = new Moq.Mock<AsyncTesting.Interfaces.IAsyncCacheService>();
        var mockNotification = new Moq.Mock<AsyncTesting.Interfaces.IAsyncNotificationService>();
        
        Console.WriteLine("✅ Servicios mock creados");
        
        Console.WriteLine("\n2. CONFIGURANDO COMPORTAMIENTOS ASÍNCRONOS:");
        mockDataProcessor.Setup(p => p.ProcessDataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, CancellationToken>(async (data, token) =>
            {
                await Task.Delay(100, token); // Simular procesamiento
                return new AsyncTesting.Models.ProcessResult
                {
                    Id = Guid.NewGuid().ToString(),
                    InputData = data,
                    ProcessedData = $"processed_{data}",
                    Success = true
                };
            });
        
        mockCache.Setup(c => c.GetAsync<AsyncTesting.Models.ProcessResult>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AsyncTesting.Models.ProcessResult)null);
        
        mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<AsyncTesting.Models.ProcessResult>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        mockRepository.Setup(r => r.CreateAsync(It.IsAny<AsyncTesting.Models.DataItem>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        Console.WriteLine("✅ Comportamientos asíncronos configurados");
        
        Console.WriteLine("\n3. CREANDO SERVICIO DE DATOS:");
        var dataService = new AsyncTesting.Services.AsyncDataService(
            mockDataProcessor.Object,
            mockRepository.Object,
            mockCache.Object,
            mockNotification.Object);
        
        Console.WriteLine("✅ Servicio de datos creado");
        
        Console.WriteLine("\n4. PROBANDO PROCESAMIENTO ASÍNCRONO:");
        var items = new[] { "item1", "item2", "item3" };
        
        var startTime = DateTime.Now;
        var results = await dataService.ProcessMultipleItemsAsync(items);
        var totalTime = DateTime.Now - startTime;
        
        Console.WriteLine($"✅ Procesamiento completado en {totalTime.TotalMilliseconds:F0}ms");
        Console.WriteLine($"✅ Items procesados: {results.Count()}");
        
        Console.WriteLine("\n5. PROBANDO CANCELACIÓN:");
        var cts = new CancellationTokenSource();
        cts.CancelAfter(50); // Cancelar después de 50ms
        
        try
        {
            await dataService.ProcessSingleItemAsync("slow_item", cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("✅ Cancelación funcionando correctamente");
        }
        
        Console.WriteLine("\n✅ Testing de Código Asíncrono demostrado!");
        Console.WriteLine("El testing asíncrono permite probar operaciones concurrentes, cancelación y timeouts.");
    }
}

// Programa principal
public class Program
{
    public static async Task Main()
    {
        await AsyncTestingDemonstration.DemonstrateAsyncTesting();
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Testing de Operaciones Concurrentes
Implementa pruebas para:
- Múltiples tareas ejecutándose simultáneamente
- Race conditions y sincronización
- Límites de concurrencia

### Ejercicio 2: Testing de Cancelación
Crea pruebas que verifiquen:
- Cancelación de operaciones largas
- Propagación de tokens de cancelación
- Limpieza de recursos al cancelar

### Ejercicio 3: Testing de Timeouts
Implementa testing para:
- Operaciones que exceden límites de tiempo
- Manejo de timeouts en diferentes escenarios
- Configuración de timeouts dinámicos

## 🔍 Puntos Clave

1. **Testing asíncrono** requiere manejo de `Task<T>` y `async/await`
2. **Cancelación** se maneja con `CancellationToken`
3. **Timeouts** se implementan con `CancellationTokenSource`
4. **Concurrencia** se prueba con múltiples tareas simultáneas
5. **Race conditions** se detectan con testing de orden de ejecución
6. **Mocks asíncronos** deben retornar `Task` o `Task<T>`
7. **Verificación** de operaciones asíncronas requiere `await`
8. **Testing de cancelación** verifica limpieza de recursos

## 📚 Recursos Adicionales

- [Async Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/)
- [Task-based Asynchronous Pattern](https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap)
- [Cancellation in Managed Threads](https://docs.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads)

---

**🎯 ¡Has completado la Clase 7! Ahora comprendes el Testing de Código Asíncrono**

**📚 [Siguiente: Clase 8 - Testing de APIs](clase_8_testing_apis.md)**
