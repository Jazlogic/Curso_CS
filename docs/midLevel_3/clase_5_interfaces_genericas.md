# 🚀 Clase 5: Interfaces Genéricas

## 📋 Información de la Clase

- **Módulo**: Mid Level 3 - Manejo de Excepciones y Generics
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 4 (Colecciones Genéricas)

## 🎯 Objetivos de Aprendizaje

- Implementar interfaces genéricas avanzadas
- Crear contratos genéricos para diferentes tipos de datos
- Usar interfaces genéricas con restricciones
- Implementar patrones de diseño con interfaces genéricas

---

## 📚 Navegación del Módulo 3

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_manejo_excepciones.md) | Manejo de Excepciones | |
| [Clase 2](clase_2_generics_basicos.md) | Generics Básicos | |
| [Clase 3](clase_3_generics_avanzados.md) | Generics Avanzados | |
| [Clase 4](clase_4_colecciones_genericas.md) | Colecciones Genéricas | ← Anterior |
| **Clase 5** | **Interfaces Genéricas** | ← Estás aquí |
| [Clase 6](clase_6_restricciones_generics.md) | Restricciones de Generics | Siguiente → |
| [Clase 7](clase_7_generics_reflection.md) | Generics y Reflection | |
| [Clase 8](clase_8_generics_performance.md) | Generics y Performance | |
| [Clase 9](clase_9_patrones_generics.md) | Patrones con Generics | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Biblioteca | |

**← [Volver al README del Módulo 3](../midLevel_3/README.md)**

---

## 📚 Contenido Teórico

### 1. Interfaces Genéricas Avanzadas

Las interfaces genéricas permiten crear contratos flexibles que pueden trabajar con diferentes tipos de datos mientras mantienen type safety.

```csharp
// ===== INTERFACES GENÉRICAS - IMPLEMENTACIÓN COMPLETA =====
namespace GenericInterfaces
{
    // ===== INTERFACES BÁSICAS GENÉRICAS =====
    namespace BasicGenericInterfaces
    {
        public interface IRepository<TEntity, TKey> where TEntity : class
        {
            TEntity GetById(TKey id);
            IEnumerable<TEntity> GetAll();
            void Add(TEntity entity);
            void Update(TEntity entity);
            void Delete(TKey id);
            bool Exists(TKey id);
            int Count();
        }
        
        public interface IReadOnlyRepository<TEntity, TKey> where TEntity : class
        {
            TEntity GetById(TKey id);
            IEnumerable<TEntity> GetAll();
            bool Exists(TKey id);
            int Count();
        }
        
        public interface IWriteRepository<TEntity, TKey> where TEntity : class
        {
            void Add(TEntity entity);
            void Update(TEntity entity);
            void Delete(TKey id);
        }
        
        public interface ICache<TKey, TValue>
        {
            void Set(TKey key, TValue value);
            bool TryGet(TKey key, out TValue value);
            void Remove(TKey key);
            void Clear();
            bool ContainsKey(TKey key);
            int Count { get; }
        }
        
        public interface IValidator<T>
        {
            bool IsValid(T item);
            IEnumerable<string> GetErrors(T item);
            ValidationResult Validate(T item);
        }
        
        public interface IComparer<T>
        {
            int Compare(T x, T y);
            bool Equals(T x, T y);
            int GetHashCode(T obj);
        }
        
        public interface IEqualityComparer<T>
        {
            bool Equals(T x, T y);
            int GetHashCode(T obj);
        }
    }
    
    // ===== INTERFACES GENÉRICAS CON RESTRICCIONES =====
    namespace ConstrainedGenericInterfaces
    {
        public interface IEntity<TKey> where TKey : IEquatable<TKey>
        {
            TKey Id { get; set; }
            DateTime CreatedAt { get; set; }
            DateTime? UpdatedAt { get; set; }
        }
        
        public interface IAuditableEntity<TKey, TUserId> 
            where TKey : IEquatable<TKey>
            where TUserId : IEquatable<TUserId>
        {
            TKey Id { get; set; }
            TUserId CreatedBy { get; set; }
            DateTime CreatedAt { get; set; }
            TUserId? UpdatedBy { get; set; }
            DateTime? UpdatedAt { get; set; }
            TUserId? DeletedBy { get; set; }
            DateTime? DeletedAt { get; set; }
            bool IsDeleted { get; set; }
        }
        
        public interface IComparableEntity<TKey, TComparable> 
            where TKey : IEquatable<TKey>
            where TComparable : IComparable<TComparable>
        {
            TKey Id { get; set; }
            TComparable SortValue { get; set; }
        }
        
        public interface IHierarchicalEntity<TKey, TParentKey> 
            where TKey : IEquatable<TKey>
            where TParentKey : IEquatable<TParentKey>
        {
            TKey Id { get; set; }
            TParentKey? ParentId { get; set; }
            int Level { get; set; }
            string Path { get; set; }
        }
    }
    
    // ===== INTERFACES GENÉRICAS PARA PATRONES DE DISEÑO =====
    namespace DesignPatternInterfaces
    {
        public interface IFactory<TProduct, TConfig> where TProduct : class
        {
            TProduct Create(TConfig config);
            TProduct CreateDefault();
            bool CanCreate(TConfig config);
        }
        
        public interface IBuilder<TProduct> where TProduct : class
        {
            IBuilder<TProduct> WithProperty<TValue>(string propertyName, TValue value);
            IBuilder<TProduct> WithAction(Action<TProduct> action);
            TProduct Build();
        }
        
        public interface IObserver<TSubject>
        {
            void OnNext(TSubject subject);
            void OnCompleted();
            void OnError(Exception error);
        }
        
        public interface IObservable<TSubject>
        {
            IDisposable Subscribe(IObserver<TSubject> observer);
            void Unsubscribe(IObserver<TSubject> observer);
            void Notify(TSubject subject);
        }
        
        public interface ICommand<TParameter>
        {
            void Execute(TParameter parameter);
            bool CanExecute(TParameter parameter);
        }
        
        public interface ICommandHandler<TCommand, TResult> where TCommand : class
        {
            Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
        }
        
        public interface IQueryHandler<TQuery, TResult> where TQuery : class
        {
            Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
        }
        
        public interface IMediator
        {
            Task<TResult> SendAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default) 
                where TCommand : class;
            Task<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default) 
                where TQuery : class;
        }
    }
    
    // ===== INTERFACES GENÉRICAS PARA SERIALIZACIÓN =====
    namespace SerializationInterfaces
    {
        public interface ISerializer<T>
        {
            string Serialize(T obj);
            T Deserialize(string data);
            byte[] SerializeToBytes(T obj);
            T DeserializeFromBytes(byte[] data);
        }
        
        public interface IAsyncSerializer<T>
        {
            Task<string> SerializeAsync(T obj);
            Task<T> DeserializeAsync(string data);
            Task<byte[]> SerializeToBytesAsync(T obj);
            Task<T> DeserializeFromBytesAsync(byte[] data);
        }
        
        public interface IFormatSerializer<T>
        {
            string Serialize(T obj, string format);
            T Deserialize(string data, string format);
            IEnumerable<string> SupportedFormats { get; }
        }
        
        public interface ICompressionSerializer<T> : ISerializer<T>
        {
            string SerializeCompressed(T obj);
            T DeserializeCompressed(string data);
            byte[] SerializeToCompressedBytes(T obj);
            T DeserializeFromCompressedBytes(byte[] data);
        }
    }
    
    // ===== INTERFACES GENÉRICAS PARA VALIDACIÓN =====
    namespace ValidationInterfaces
    {
        public interface IValidationRule<T>
        {
            string PropertyName { get; }
            string ErrorMessage { get; }
            bool IsValid(T item);
            ValidationResult Validate(T item);
        }
        
        public interface IConditionalValidationRule<T> : IValidationRule<T>
        {
            bool ShouldValidate(T item);
        }
        
        public interface IAsyncValidationRule<T>
        {
            string PropertyName { get; }
            string ErrorMessage { get; }
            Task<bool> IsValidAsync(T item);
            Task<ValidationResult> ValidateAsync(T item);
        }
        
        public interface IValidationContext<T>
        {
            T Item { get; }
            IDictionary<string, object> ContextData { get; }
            void AddError(string propertyName, string errorMessage);
            void AddWarning(string propertyName, string warningMessage);
            bool HasErrors { get; }
            bool HasWarnings { get; }
            IEnumerable<ValidationResult> Errors { get; }
            IEnumerable<ValidationResult> Warnings { get; }
        }
        
        public interface IValidator<T>
        {
            ValidationResult Validate(T item);
            ValidationResult Validate(T item, IValidationContext<T> context);
            Task<ValidationResult> ValidateAsync(T item);
            Task<ValidationResult> ValidateAsync(T item, IValidationContext<T> context);
            void AddRule(IValidationRule<T> rule);
            void AddRule(IConditionalValidationRule<T> rule);
            void AddAsyncRule(IAsyncValidationRule<T> rule);
        }
    }
    
    // ===== INTERFACES GENÉRICAS PARA CACHE =====
    namespace CacheInterfaces
    {
        public interface ICacheItem<TValue>
        {
            TValue Value { get; set; }
            DateTime CreatedAt { get; set; }
            DateTime? ExpiresAt { get; set; }
            bool IsExpired { get; }
            int AccessCount { get; set; }
            DateTime LastAccessed { get; set; }
        }
        
        public interface IExpiringCache<TKey, TValue> : ICache<TKey, TValue>
        {
            void Set(TKey key, TValue value, TimeSpan? ttl = null);
            void Set(TKey key, TValue value, DateTime? expiresAt = null);
            IEnumerable<TKey> GetExpiredKeys();
            void CleanupExpired();
            TimeSpan DefaultTtl { get; set; }
        }
        
        public interface IAsyncCache<TKey, TValue> : ICache<TKey, TValue>
        {
            Task SetAsync(TKey key, TValue value);
            Task<bool> TryGetAsync(TKey key, out TValue value);
            Task RemoveAsync(TKey key);
            Task ClearAsync();
            Task<bool> ContainsKeyAsync(TKey key);
        }
        
        public interface ICompressedCache<TKey, TValue> : ICache<TKey, TValue>
        {
            void SetCompressed(TKey key, TValue value);
            bool TryGetCompressed(TKey key, out TValue value);
            bool TryGetCompressed(TKey key, out TValue value, out bool wasCompressed);
        }
        
        public interface IDistributedCache<TKey, TValue> : ICache<TKey, TValue>
        {
            Task<bool> TryGetAsync(TKey key, out TValue value);
            Task SetAsync(TKey key, TValue value);
            Task RemoveAsync(TKey key);
            Task ClearAsync();
            Task<bool> ContainsKeyAsync(TKey key);
            Task<IEnumerable<TKey>> GetAllKeysAsync();
        }
    }
    
    // ===== INTERFACES GENÉRICAS PARA LOGGING =====
    namespace LoggingInterfaces
    {
        public interface ILogger<T>
        {
            void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter);
            bool IsEnabled(LogLevel logLevel);
            IDisposable BeginScope<TState>(TState state);
        }
        
        public interface IStructuredLogger<T> : ILogger<T>
        {
            void LogStructured<TData>(LogLevel logLevel, string message, TData data);
            void LogStructured<TData>(LogLevel logLevel, string message, TData data, Exception exception);
            void LogStructured<TData>(LogLevel logLevel, string message, TData data, IDictionary<string, object> properties);
        }
        
        public interface IAuditLogger<TEntity, TKey> where TEntity : class
        {
            Task LogCreationAsync(TEntity entity, TKey userId);
            Task LogUpdateAsync(TEntity entity, TKey userId, TEntity originalEntity);
            Task LogDeletionAsync(TEntity entity, TKey userId);
            Task LogAccessAsync(TEntity entity, TKey userId, string action);
            Task<IEnumerable<AuditLogEntry<TEntity, TKey>>> GetAuditLogAsync(TKey entityId);
        }
        
        public interface IPerformanceLogger<T>
        {
            IDisposable MeasureOperation(string operationName);
            void LogPerformance<TMetric>(string operationName, TMetric metric);
            Task<PerformanceMetrics> GetPerformanceMetricsAsync(string operationName);
        }
    }
    
    // ===== INTERFACES GENÉRICAS PARA NOTIFICACIONES =====
    namespace NotificationInterfaces
    {
        public interface INotification<TData>
        {
            string Id { get; }
            string Type { get; }
            TData Data { get; }
            DateTime CreatedAt { get; }
            NotificationPriority Priority { get; }
            NotificationStatus Status { get; set; }
        }
        
        public interface INotificationHandler<TNotification, TData> where TNotification : INotification<TData>
        {
            Task HandleAsync(TNotification notification, CancellationToken cancellationToken = default);
            bool CanHandle(TNotification notification);
        }
        
        public interface INotificationPublisher<TData>
        {
            Task PublishAsync(INotification<TData> notification, CancellationToken cancellationToken = default);
            Task PublishAsync(IEnumerable<INotification<TData>> notifications, CancellationToken cancellationToken = default);
        }
        
        public interface INotificationSubscriber<TData>
        {
            IDisposable Subscribe<TNotification>(INotificationHandler<TNotification, TData> handler) 
                where TNotification : INotification<TData>;
            void Unsubscribe<TNotification>(INotificationHandler<TNotification, TData> handler) 
                where TNotification : INotification<TData>;
        }
    }
    
    // ===== INTERFACES GENÉRICAS PARA WORKFLOW =====
    namespace WorkflowInterfaces
    {
        public interface IWorkflowStep<TContext>
        {
            string StepName { get; }
            int Order { get; }
            bool CanExecute(TContext context);
            Task<WorkflowStepResult> ExecuteAsync(TContext context, CancellationToken cancellationToken = default);
            Task<bool> CanRollbackAsync(TContext context);
            Task RollbackAsync(TContext context, CancellationToken cancellationToken = default);
        }
        
        public interface IWorkflow<TContext>
        {
            string WorkflowName { get; }
            IEnumerable<IWorkflowStep<TContext>> Steps { get; }
            Task<WorkflowResult> ExecuteAsync(TContext context, CancellationToken cancellationToken = default);
            Task<WorkflowResult> ExecuteStepAsync(string stepName, TContext context, CancellationToken cancellationToken = default);
            Task<bool> RollbackToStepAsync(string stepName, TContext context, CancellationToken cancellationToken = default);
        }
        
        public interface IWorkflowEngine<TContext>
        {
            void RegisterWorkflow(IWorkflow<TContext> workflow);
            Task<WorkflowResult> ExecuteWorkflowAsync(string workflowName, TContext context, CancellationToken cancellationToken = default);
            Task<IEnumerable<string>> GetAvailableWorkflowsAsync();
            Task<WorkflowStatus> GetWorkflowStatusAsync(string workflowName, TContext context);
        }
    }
    
    // ===== MODELOS Y RESULTADOS =====
    namespace Models
    {
        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public string PropertyName { get; set; }
            public string Message { get; set; }
            public ValidationSeverity Severity { get; set; }
            public IDictionary<string, object> AdditionalData { get; set; }
        }
        
        public enum ValidationSeverity
        {
            Info,
            Warning,
            Error,
            Critical
        }
        
        public class AuditLogEntry<TEntity, TKey>
        {
            public TKey Id { get; set; }
            public TKey EntityId { get; set; }
            public TEntity Entity { get; set; }
            public TKey UserId { get; set; }
            public string Action { get; set; }
            public DateTime Timestamp { get; set; }
            public IDictionary<string, object> Changes { get; set; }
        }
        
        public class PerformanceMetrics
        {
            public string OperationName { get; set; }
            public long TotalExecutions { get; set; }
            public TimeSpan AverageExecutionTime { get; set; }
            public TimeSpan MinExecutionTime { get; set; }
            public TimeSpan MaxExecutionTime { get; set; }
            public long TotalErrors { get; set; }
            public double SuccessRate { get; set; }
        }
        
        public class WorkflowStepResult
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
            public Exception Error { get; set; }
            public IDictionary<string, object> OutputData { get; set; }
        }
        
        public class WorkflowResult
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
            public Exception Error { get; set; }
            public IEnumerable<WorkflowStepResult> StepResults { get; set; }
            public IDictionary<string, object> FinalOutput { get; set; }
        }
        
        public class WorkflowStatus
        {
            public string CurrentStep { get; set; }
            public int CompletedSteps { get; set; }
            public int TotalSteps { get; set; }
            public bool IsCompleted { get; set; }
            public bool HasErrors { get; set; }
            public DateTime StartedAt { get; set; }
            public DateTime? CompletedAt { get; set; }
        }
        
        public enum NotificationPriority
        {
            Low,
            Normal,
            High,
            Critical
        }
        
        public enum NotificationStatus
        {
            Pending,
            Sent,
            Delivered,
            Read,
            Failed
        }
    }
}

// Uso de Interfaces Genéricas
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Interfaces Genéricas - Clase 5 ===\n");
        
        Console.WriteLine("Los componentes implementados incluyen:");
        Console.WriteLine("1. Interfaces básicas genéricas (Repository, Cache, Validator)");
        Console.WriteLine("2. Interfaces con restricciones (Entity, AuditableEntity)");
        Console.WriteLine("3. Interfaces para patrones de diseño (Factory, Builder, Observer)");
        Console.WriteLine("4. Interfaces de serialización genéricas");
        Console.WriteLine("5. Interfaces de validación avanzadas");
        Console.WriteLine("6. Interfaces de cache especializadas");
        Console.WriteLine("7. Interfaces de logging estructurado");
        Console.WriteLine("8. Interfaces de notificaciones");
        Console.WriteLine("9. Interfaces de workflow");
        
        Console.WriteLine("\nBeneficios de esta implementación:");
        Console.WriteLine("- Contratos flexibles y reutilizables");
        Console.WriteLine("- Type safety en tiempo de compilación");
        Console.WriteLine("- Separación clara de responsabilidades");
        Console.WriteLine("- Patrones de diseño genéricos");
        Console.WriteLine("- Extensibilidad para diferentes tipos");
        Console.WriteLine("- Consistencia en la arquitectura");
        
        Console.WriteLine("\nCasos de uso principales:");
        Console.WriteLine("- Repositorios genéricos para diferentes entidades");
        Console.WriteLine("- Validación genérica para cualquier tipo de objeto");
        Console.WriteLine("- Cache genérico con diferentes estrategias");
        Console.WriteLine("- Logging estructurado para diferentes contextos");
        Console.WriteLine("- Workflows genéricos para diferentes procesos de negocio");
        Console.WriteLine("- Notificaciones genéricas para diferentes tipos de eventos");
    }
}

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Repository Genérico
Implementa un repositorio genérico que implemente las interfaces IRepository<TEntity, TKey>.

### Ejercicio 2: Validator Genérico
Crea un validador genérico que implemente IValidator<T> con reglas personalizables.

### Ejercicio 3: Cache Genérico
Implementa un cache genérico que implemente ICache<TKey, TValue> con expiración.

## 🔍 Puntos Clave

1. **Interfaces básicas genéricas** para operaciones comunes
2. **Interfaces con restricciones** para tipos específicos
3. **Interfaces para patrones de diseño** genéricos
4. **Interfaces de serialización** multi-formato
5. **Interfaces de validación** avanzadas y asíncronas

## 📚 Recursos Adicionales

- [Microsoft Docs - Generic Interfaces](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/generic-interfaces)
- [Design Patterns with Generics](https://docs.microsoft.com/en-us/dotnet/standard/generics/)

---

**🎯 ¡Has completado la Clase 5! Ahora comprendes las Interfaces Genéricas**

**📚 [Siguiente: Clase 6 - Restricciones de Generics](clase_6_restricciones_generics.md)**
