# 🚀 Clase 3: Generics Avanzados

## 📋 Información de la Clase

- **Módulo**: Mid Level 3 - Manejo de Excepciones y Generics
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 2 (Generics Básicos)

## 🎯 Objetivos de Aprendizaje

- Implementar generics avanzados con restricciones complejas
- Crear patrones de diseño con generics
- Usar generics con reflection y metaprogramación
- Implementar generics para operaciones asíncronas

---

## 📚 Navegación del Módulo 3

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_manejo_excepciones.md) | Manejo de Excepciones | |
| [Clase 2](clase_2_generics_basicos.md) | Generics Básicos | ← Anterior |
| **Clase 3** | **Generics Avanzados** | ← Estás aquí |
| [Clase 4](clase_4_colecciones_genericas.md) | Colecciones Genéricas | Siguiente → |
| [Clase 5](clase_5_interfaces_genericas.md) | Interfaces Genéricas | |
| [Clase 6](clase_6_restricciones_generics.md) | Restricciones de Generics | |
| [Clase 7](clase_7_generics_reflection.md) | Generics y Reflection | |
| [Clase 8](clase_8_generics_performance.md) | Generics y Performance | |
| [Clase 9](clase_9_patrones_generics.md) | Patrones con Generics | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Biblioteca | |

**← [Volver al README del Módulo 3](../midLevel_3/README.md)**

---

## 📚 Contenido Teórico

### 1. Generics Avanzados

Los generics avanzados permiten crear patrones más sofisticados y flexibles, combinando múltiples restricciones y técnicas avanzadas.

```csharp
// ===== GENERICS AVANZADOS - IMPLEMENTACIÓN COMPLETA =====
namespace AdvancedGenerics
{
    // ===== GENERICS CON RESTRICCIONES MÚLTIPLES =====
    namespace MultipleConstraints
    {
        public class AdvancedRepository<TEntity, TKey> 
            where TEntity : class, IEntity<TKey>, new()
            where TKey : IEquatable<TKey>, IComparable<TKey>
        {
            private readonly Dictionary<TKey, TEntity> _entities = new Dictionary<TKey, TEntity>();
            
            public void Add(TEntity entity)
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));
                
                _entities[entity.Id] = entity;
            }
            
            public TEntity GetById(TKey id)
            {
                if (_entities.TryGetValue(id, out var entity))
                {
                    return entity;
                }
                
                return null;
            }
            
            public IEnumerable<TEntity> GetAll()
            {
                return _entities.Values;
            }
            
            public void Update(TEntity entity)
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));
                
                if (!_entities.ContainsKey(entity.Id))
                    throw new KeyNotFoundException($"Entity with ID {entity.Id} not found");
                
                _entities[entity.Id] = entity;
            }
            
            public bool Delete(TKey id)
            {
                return _entities.Remove(id);
            }
            
            public IEnumerable<TEntity> Find(Func<TEntity, bool> predicate)
            {
                return _entities.Values.Where(predicate);
            }
            
            public TEntity FindFirst(Func<TEntity, bool> predicate)
            {
                return _entities.Values.FirstOrDefault(predicate);
            }
        }
        
        public class AdvancedCache<TKey, TValue, TMetadata>
            where TKey : IEquatable<TKey>
            where TValue : class
            where TMetadata : ICacheMetadata
        {
            private readonly Dictionary<TKey, CacheEntry<TValue, TMetadata>> _cache = new Dictionary<TKey, CacheEntry<TValue, TMetadata>>();
            
            public void Set(TKey key, TValue value, TMetadata metadata)
            {
                var entry = new CacheEntry<TValue, TMetadata>(value, metadata);
                _cache[key] = entry;
            }
            
            public bool TryGet(TKey key, out TValue value, out TMetadata metadata)
            {
                if (_cache.TryGetValue(key, out var entry))
                {
                    if (!entry.IsExpired)
                    {
                        value = entry.Value;
                        metadata = entry.Metadata;
                        return true;
                    }
                    
                    _cache.Remove(key);
                }
                
                value = null;
                metadata = default(TMetadata);
                return false;
            }
            
            public void CleanupExpired()
            {
                var expiredKeys = _cache.Where(kvp => kvp.Value.IsExpired)
                                       .Select(kvp => kvp.Key)
                                       .ToList();
                
                foreach (var key in expiredKeys)
                {
                    _cache.Remove(key);
                }
            }
        }
        
        public class CacheEntry<TValue, TMetadata>
        {
            public TValue Value { get; set; }
            public TMetadata Metadata { get; set; }
            public DateTime CreatedAt { get; set; }
            
            public bool IsExpired => DateTime.UtcNow > Metadata.ExpiresAt;
            
            public CacheEntry(TValue value, TMetadata metadata)
            {
                Value = value;
                Metadata = metadata;
                CreatedAt = DateTime.UtcNow;
            }
        }
    }
    
    // ===== GENERICS CON PATRONES DE DISEÑO =====
    namespace GenericDesignPatterns
    {
        public class GenericFactory<T> where T : class, new()
        {
            private readonly Dictionary<string, Func<T>> _creators = new Dictionary<string, Func<T>>();
            
            public void RegisterCreator(string key, Func<T> creator)
            {
                _creators[key] = creator;
            }
            
            public T Create(string key)
            {
                if (_creators.TryGetValue(key, out var creator))
                {
                    return creator();
                }
                
                return new T(); // Default constructor
            }
            
            public T CreateWithDefault()
            {
                return new T();
            }
        }
        
        public class GenericBuilder<T> where T : class, new()
        {
            private readonly T _instance;
            private readonly Dictionary<string, Action<T, object>> _setters = new Dictionary<string, Action<T, object>>();
            
            public GenericBuilder()
            {
                _instance = new T();
            }
            
            public GenericBuilder<T> WithProperty<TProperty>(string propertyName, TProperty value)
            {
                var property = typeof(T).GetProperty(propertyName);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(_instance, value);
                }
                return this;
            }
            
            public GenericBuilder<T> WithAction(Action<T> action)
            {
                action(_instance);
                return this;
            }
            
            public T Build()
            {
                return _instance;
            }
        }
        
        public class GenericObserver<T>
        {
            private readonly List<IObserver<T>> _observers = new List<IObserver<T>>();
            
            public void Subscribe(IObserver<T> observer)
            {
                if (!_observers.Contains(observer))
                {
                    _observers.Add(observer);
                }
            }
            
            public void Unsubscribe(IObserver<T> observer)
            {
                _observers.Remove(observer);
            }
            
            public void Notify(T data)
            {
                foreach (var observer in _observers)
                {
                    observer.OnNext(data);
                }
            }
            
            public void Complete()
            {
                foreach (var observer in _observers)
                {
                    observer.OnCompleted();
                }
                _observers.Clear();
            }
        }
        
        public class GenericCommand<T>
        {
            private readonly Action<T> _execute;
            private readonly Action<T> _undo;
            
            public GenericCommand(Action<T> execute, Action<T> undo = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _undo = undo;
            }
            
            public void Execute(T parameter)
            {
                _execute(parameter);
            }
            
            public void Undo(T parameter)
            {
                _undo?.Invoke(parameter);
            }
        }
        
        public class GenericCommandInvoker<T>
        {
            private readonly Stack<GenericCommand<T>> _undoStack = new Stack<GenericCommand<T>>();
            private readonly Stack<GenericCommand<T>> _redoStack = new Stack<GenericCommand<T>>();
            
            public void ExecuteCommand(GenericCommand<T> command, T parameter)
            {
                command.Execute(parameter);
                _undoStack.Push(command);
                _redoStack.Clear(); // Clear redo stack when new command is executed
            }
            
            public void Undo(T parameter)
            {
                if (_undoStack.Count > 0)
                {
                    var command = _undoStack.Pop();
                    command.Undo(parameter);
                    _redoStack.Push(command);
                }
            }
            
            public void Redo(T parameter)
            {
                if (_redoStack.Count > 0)
                {
                    var command = _redoStack.Pop();
                    command.Execute(parameter);
                    _undoStack.Push(command);
                }
            }
            
            public bool CanUndo => _undoStack.Count > 0;
            public bool CanRedo => _redoStack.Count > 0;
        }
    }
    
    // ===== GENERICS CON OPERACIONES ASÍNCRONAS =====
    namespace AsyncGenerics
    {
        public class GenericAsyncProcessor<TInput, TOutput>
        {
            private readonly Func<TInput, Task<TOutput>> _processor;
            private readonly ILogger<GenericAsyncProcessor<TInput, TOutput>> _logger;
            
            public GenericAsyncProcessor(Func<TInput, Task<TOutput>> processor, ILogger<GenericAsyncProcessor<TInput, TOutput>> logger)
            {
                _processor = processor ?? throw new ArgumentNullException(nameof(processor));
                _logger = logger;
            }
            
            public async Task<TOutput> ProcessAsync(TInput input)
            {
                try
                {
                    _logger.LogInformation("Processing input of type {InputType}", typeof(TInput).Name);
                    var result = await _processor(input);
                    _logger.LogInformation("Successfully processed input to output of type {OutputType}", typeof(TOutput).Name);
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing input of type {InputType}", typeof(TInput).Name);
                    throw;
                }
            }
            
            public async Task<IEnumerable<TOutput>> ProcessBatchAsync(IEnumerable<TInput> inputs)
            {
                var tasks = inputs.Select(input => ProcessAsync(input));
                return await Task.WhenAll(tasks);
            }
            
            public async Task<TOutput> ProcessWithRetryAsync(TInput input, int maxRetries = 3, TimeSpan? delay = null)
            {
                var actualDelay = delay ?? TimeSpan.FromSeconds(1);
                
                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        return await ProcessAsync(input);
                    }
                    catch (Exception ex) when (attempt < maxRetries)
                    {
                        _logger.LogWarning(ex, "Attempt {Attempt} failed, retrying in {Delay}ms", attempt, actualDelay.TotalMilliseconds);
                        await Task.Delay(actualDelay);
                        actualDelay = TimeSpan.FromMilliseconds(actualDelay.TotalMilliseconds * 2); // Exponential backoff
                    }
                }
                
                throw new InvalidOperationException($"Processing failed after {maxRetries} attempts");
            }
        }
        
        public class GenericAsyncQueue<T>
        {
            private readonly Queue<T> _queue = new Queue<T>();
            private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);
            private readonly object _lock = new object();
            
            public void Enqueue(T item)
            {
                lock (_lock)
                {
                    _queue.Enqueue(item);
                    _semaphore.Release();
                }
            }
            
            public async Task<T> DequeueAsync(CancellationToken cancellationToken = default)
            {
                await _semaphore.WaitAsync(cancellationToken);
                
                lock (_lock)
                {
                    return _queue.Dequeue();
                }
            }
            
            public async Task<T> DequeueAsync(TimeSpan timeout)
            {
                using var cts = new CancellationTokenSource(timeout);
                return await DequeueAsync(cts.Token);
            }
            
            public int Count
            {
                get
                {
                    lock (_lock)
                    {
                        return _queue.Count;
                    }
                }
            }
            
            public bool IsEmpty
            {
                get
                {
                    lock (_lock)
                    {
                        return _queue.Count == 0;
                    }
                }
            }
        }
        
        public class GenericAsyncLazy<T>
        {
            private readonly Func<Task<T>> _factory;
            private Task<T> _task;
            
            public GenericAsyncLazy(Func<Task<T>> factory)
            {
                _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            }
            
            public Task<T> Value
            {
                get
                {
                    if (_task == null)
                    {
                        lock (this)
                        {
                            if (_task == null)
                            {
                                _task = _factory();
                            }
                        }
                    }
                    return _task;
                }
            }
            
            public void Reset()
            {
                lock (this)
                {
                    _task = null;
                }
            }
        }
    }
    
    // ===== GENERICS CON VALIDACIÓN AVANZADA =====
    namespace AdvancedValidation
    {
        public class ValidationRule<T>
        {
            public string PropertyName { get; set; }
            public Func<T, bool> Predicate { get; set; }
            public string ErrorMessage { get; set; }
            
            public ValidationRule(string propertyName, Func<T, bool> predicate, string errorMessage)
            {
                PropertyName = propertyName;
                Predicate = predicate;
                ErrorMessage = errorMessage;
            }
            
            public ValidationResult Validate(T item)
            {
                var isValid = Predicate(item);
                return new ValidationResult
                {
                    IsValid = isValid,
                    PropertyName = PropertyName,
                    ErrorMessage = isValid ? null : ErrorMessage
                };
            }
        }
        
        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public string PropertyName { get; set; }
            public string ErrorMessage { get; set; }
        }
        
        public class GenericValidator<T>
        {
            private readonly List<ValidationRule<T>> _rules = new List<ValidationRule<T>>();
            
            public void AddRule(ValidationRule<T> rule)
            {
                _rules.Add(rule);
            }
            
            public void AddRule(string propertyName, Func<T, bool> predicate, string errorMessage)
            {
                AddRule(new ValidationRule<T>(propertyName, predicate, errorMessage));
            }
            
            public ValidationResult[] Validate(T item)
            {
                return _rules.Select(rule => rule.Validate(item)).ToArray();
            }
            
            public bool IsValid(T item)
            {
                return Validate(item).All(result => result.IsValid);
            }
            
            public string[] GetErrors(T item)
            {
                return Validate(item)
                    .Where(result => !result.IsValid)
                    .Select(result => result.ErrorMessage)
                    .ToArray();
            }
        }
        
        public class ConditionalValidator<T>
        {
            private readonly List<ConditionalValidationRule<T>> _rules = new List<ConditionalValidationRule<T>>();
            
            public void AddConditionalRule(Func<T, bool> condition, ValidationRule<T> rule)
            {
                _rules.Add(new ConditionalValidationRule<T>(condition, rule));
            }
            
            public ValidationResult[] Validate(T item)
            {
                var results = new List<ValidationResult>();
                
                foreach (var conditionalRule in _rules)
                {
                    if (conditionalRule.Condition(item))
                    {
                        results.Add(conditionalRule.Rule.Validate(item));
                    }
                }
                
                return results.ToArray();
            }
        }
        
        public class ConditionalValidationRule<T>
        {
            public Func<T, bool> Condition { get; set; }
            public ValidationRule<T> Rule { get; set; }
            
            public ConditionalValidationRule(Func<T, bool> condition, ValidationRule<T> rule)
            {
                Condition = condition;
                Rule = rule;
            }
        }
    }
    
    // ===== GENERICS CON SERIALIZACIÓN =====
    namespace GenericSerialization
    {
        public interface ISerializer<T>
        {
            string Serialize(T obj);
            T Deserialize(string data);
        }
        
        public class JsonSerializer<T> : ISerializer<T>
        {
            private readonly JsonSerializerOptions _options;
            
            public JsonSerializer(JsonSerializerOptions options = null)
            {
                _options = options ?? new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };
            }
            
            public string Serialize(T obj)
            {
                if (obj == null) return "null";
                return System.Text.Json.JsonSerializer.Serialize(obj, _options);
            }
            
            public T Deserialize(string data)
            {
                if (string.IsNullOrEmpty(data) || data == "null")
                    return default(T);
                
                return System.Text.Json.JsonSerializer.Deserialize<T>(data, _options);
            }
        }
        
        public class XmlSerializer<T> : ISerializer<T>
        {
            private readonly System.Xml.Serialization.XmlSerializer _serializer;
            
            public XmlSerializer()
            {
                _serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            }
            
            public string Serialize(T obj)
            {
                if (obj == null) return string.Empty;
                
                using var stringWriter = new StringWriter();
                _serializer.Serialize(stringWriter, obj);
                return stringWriter.ToString();
            }
            
            public T Deserialize(string data)
            {
                if (string.IsNullOrEmpty(data))
                    return default(T);
                
                using var stringReader = new StringReader(data);
                return (T)_serializer.Deserialize(stringReader);
            }
        }
        
        public class GenericSerializationManager<T>
        {
            private readonly Dictionary<string, ISerializer<T>> _serializers = new Dictionary<string, ISerializer<T>>();
            
            public void RegisterSerializer(string format, ISerializer<T> serializer)
            {
                _serializers[format.ToLower()] = serializer;
            }
            
            public string Serialize(T obj, string format)
            {
                if (_serializers.TryGetValue(format.ToLower(), out var serializer))
                {
                    return serializer.Serialize(obj);
                }
                
                throw new NotSupportedException($"Serialization format '{format}' is not supported");
            }
            
            public T Deserialize(string data, string format)
            {
                if (_serializers.TryGetValue(format.ToLower(), out var serializer))
                {
                    return serializer.Deserialize(data);
                }
                
                throw new NotSupportedException($"Deserialization format '{format}' is not supported");
            }
            
            public IEnumerable<string> SupportedFormats => _serializers.Keys;
        }
    }
    
    // ===== INTERFACES Y MODELOS =====
    namespace Interfaces
    {
        public interface IEntity<TKey>
        {
            TKey Id { get; set; }
        }
        
        public interface ICacheMetadata
        {
            DateTime ExpiresAt { get; }
            string CacheKey { get; }
        }
        
        public interface IObserver<T>
        {
            void OnNext(T value);
            void OnCompleted();
            void OnError(Exception error);
        }
    }
    
    namespace Models
    {
        public class User : IEntity<int>
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public int Age { get; set; }
        }
        
        public class Product : IEntity<string>
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int StockQuantity { get; set; }
        }
        
        public class CacheMetadata : ICacheMetadata
        {
            public DateTime ExpiresAt { get; set; }
            public string CacheKey { get; set; }
            public string Description { get; set; }
        }
    }
}

// Uso de Generics Avanzados
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Generics Avanzados - Clase 3 ===\n");
        
        Console.WriteLine("Los componentes implementados incluyen:");
        Console.WriteLine("1. Generics con restricciones múltiples");
        Console.WriteLine("2. Patrones de diseño genéricos (Factory, Builder, Observer, Command)");
        Console.WriteLine("3. Generics con operaciones asíncronas");
        Console.WriteLine("4. Validación avanzada con generics");
        Console.WriteLine("5. Serialización genérica");
        Console.WriteLine("6. Cache avanzado con metadatos");
        
        Console.WriteLine("\nEjemplos de uso:");
        
        // Repository genérico avanzado
        var userRepo = new AdvancedRepository<User, int>();
        var productRepo = new AdvancedRepository<Product, string>();
        
        // Factory genérico
        var factory = new GenericFactory<User>();
        factory.RegisterCreator("default", () => new User { Username = "default", Email = "default@example.com" });
        
        var user = factory.Create("default");
        Console.WriteLine($"User creado: {user.Username}");
        
        // Builder genérico
        var userBuilder = new GenericBuilder<User>()
            .WithProperty("Username", "john_doe")
            .WithProperty("Email", "john@example.com")
            .WithProperty("Age", 30);
        
        var builtUser = userBuilder.Build();
        Console.WriteLine($"User construido: {builtUser.Username}, {builtUser.Email}");
        
        // Validator genérico avanzado
        var validator = new GenericValidator<User>();
        validator.AddRule("Username", u => !string.IsNullOrEmpty(u.Username), "Username es requerido");
        validator.AddRule("Email", u => !string.IsNullOrEmpty(u.Email), "Email es requerido");
        validator.AddRule("Age", u => u.Age >= 0 && u.Age <= 150, "Edad debe estar entre 0 y 150");
        
        var validationResult = validator.Validate(builtUser);
        Console.WriteLine($"Validación: {validationResult.Count(r => r.IsValid)}/{validationResult.Length} válidas");
        
        // Serialización genérica
        var serializationManager = new GenericSerializationManager<User>();
        serializationManager.RegisterSerializer("json", new JsonSerializer<User>());
        serializationManager.RegisterSerializer("xml", new XmlSerializer<User>());
        
        var jsonData = serializationManager.Serialize(builtUser, "json");
        Console.WriteLine($"JSON serializado: {jsonData.Substring(0, Math.Min(50, jsonData.Length))}...");
        
        Console.WriteLine("\nBeneficios de esta implementación:");
        Console.WriteLine("- Patrones de diseño flexibles y reutilizables");
        Console.WriteLine("- Operaciones asíncronas type-safe");
        Console.WriteLine("- Validación avanzada y condicional");
        Console.WriteLine("- Serialización múltiple formatos");
        Console.WriteLine("- Cache con metadatos personalizados");
        Console.WriteLine("- Comandos con undo/redo genéricos");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Factory Genérico Avanzado
Implementa un factory genérico que soporte múltiples tipos de creación y configuración.

### Ejercicio 2: Validator Condicional
Crea un validador genérico que aplique reglas solo cuando se cumplan ciertas condiciones.

### Ejercicio 3: Serializador Múltiple
Implementa un sistema de serialización genérico que soporte múltiples formatos.

## 🔍 Puntos Clave

1. **Restricciones múltiples** para generics complejos
2. **Patrones de diseño** implementados con generics
3. **Operaciones asíncronas** type-safe
4. **Validación avanzada** con reglas condicionales
5. **Serialización genérica** multi-formato

## 📚 Recursos Adicionales

- [Microsoft Docs - Generic Constraints](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/constraints-on-type-parameters)
- [Design Patterns with Generics](https://docs.microsoft.com/en-us/dotnet/standard/generics/)

---

**🎯 ¡Has completado la Clase 3! Ahora comprendes los Generics Avanzados**

**📚 [Siguiente: Clase 4 - Colecciones Genéricas](clase_4_colecciones_genericas.md)**
