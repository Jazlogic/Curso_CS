# 🚀 Clase 9: Patrones con Generics

## 📋 Información de la Clase

- **Módulo**: Mid Level 3 - Manejo de Excepciones y Generics
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 8 (Generics y Performance)

## 🎯 Objetivos de Aprendizaje

- Implementar patrones de diseño clásicos con generics
- Crear patrones genéricos personalizados
- Usar generics para mejorar la flexibilidad de patrones
- Aplicar patrones genéricos en escenarios reales

---

## 📚 Navegación del Módulo 3

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_manejo_excepciones.md) | Manejo de Excepciones | |
| [Clase 2](clase_2_generics_basicos.md) | Generics Básicos | |
| [Clase 3](clase_3_generics_avanzados.md) | Generics Avanzados | |
| [Clase 4](clase_4_colecciones_genericas.md) | Colecciones Genéricas | |
| [Clase 5](clase_5_interfaces_genericas.md) | Interfaces Genéricas | |
| [Clase 6](clase_6_restricciones_generics.md) | Restricciones de Generics | |
| [Clase 7](clase_7_generics_reflection.md) | Generics y Reflection | |
| [Clase 8](clase_8_generics_performance.md) | Generics y Performance | ← Anterior |
| **Clase 9** | **Patrones con Generics** | ← Estás aquí |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Biblioteca | Siguiente → |

**← [Volver al README del Módulo 3](../midLevel_3/README.md)**

---

## 📚 Contenido Teórico

### 1. Patrones con Generics

Los generics permiten implementar patrones de diseño de manera más flexible y type-safe, adaptándolos a diferentes tipos de datos.

```csharp
// ===== PATRONES CON GENERICS - IMPLEMENTACIÓN COMPLETA =====
namespace GenericPatterns
{
    // ===== PATRÓN FACTORY GENÉRICO =====
    namespace FactoryPattern
    {
        // Factory genérico básico
        public interface IGenericFactory<TProduct> where TProduct : class
        {
            TProduct Create();
            TProduct Create(params object[] parameters);
            bool CanCreate();
        }
        
        public class GenericFactory<TProduct> : IGenericFactory<TProduct> where TProduct : class
        {
            private readonly Func<TProduct> _createFunc;
            private readonly Func<object[], TProduct> _createWithParamsFunc;
            
            public GenericFactory(Func<TProduct> createFunc, Func<object[], TProduct> createWithParamsFunc = null)
            {
                _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
                _createWithParamsFunc = createWithParamsFunc;
            }
            
            public TProduct Create() => _createFunc();
            
            public TProduct Create(params object[] parameters)
            {
                if (_createWithParamsFunc != null)
                    return _createWithParamsFunc(parameters);
                
                return Create();
            }
            
            public bool CanCreate() => _createFunc != null;
        }
        
        // Factory genérico con configuración
        public interface IConfigurableFactory<TProduct, TConfig> where TProduct : class
        {
            TProduct Create(TConfig config);
            TProduct CreateDefault();
            bool CanCreate(TConfig config);
        }
        
        public class ConfigurableFactory<TProduct, TConfig> : IConfigurableFactory<TProduct, TConfig> 
            where TProduct : class
        {
            private readonly Func<TConfig, TProduct> _createFunc;
            private readonly Func<TProduct> _createDefaultFunc;
            private readonly Func<TConfig, bool> _canCreateFunc;
            
            public ConfigurableFactory(
                Func<TConfig, TProduct> createFunc,
                Func<TProduct> createDefaultFunc = null,
                Func<TConfig, bool> canCreateFunc = null)
            {
                _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
                _createDefaultFunc = createDefaultFunc;
                _canCreateFunc = canCreateFunc;
            }
            
            public TProduct Create(TConfig config) => _createFunc(config);
            
            public TProduct CreateDefault()
            {
                if (_createDefaultFunc != null)
                    return _createDefaultFunc();
                
                throw new InvalidOperationException("Default creation not supported");
            }
            
            public bool CanCreate(TConfig config)
            {
                if (_canCreateFunc != null)
                    return _canCreateFunc(config);
                
                return true;
            }
        }
        
        // Factory genérico con registro de tipos
        public class GenericFactoryRegistry
        {
            private readonly Dictionary<Type, object> _factories = new Dictionary<Type, object>();
            
            public void Register<TProduct>(IGenericFactory<TProduct> factory) where TProduct : class
            {
                _factories[typeof(TProduct)] = factory;
            }
            
            public void Register<TProduct, TConfig>(IConfigurableFactory<TProduct, TConfig> factory) 
                where TProduct : class
            {
                _factories[typeof(TProduct)] = factory;
            }
            
            public IGenericFactory<TProduct> GetFactory<TProduct>() where TProduct : class
            {
                if (_factories.TryGetValue(typeof(TProduct), out var factory))
                    return (IGenericFactory<TProduct>)factory;
                
                throw new InvalidOperationException($"Factory for {typeof(TProduct).Name} not registered");
            }
            
            public IConfigurableFactory<TProduct, TConfig> GetConfigurableFactory<TProduct, TConfig>() 
                where TProduct : class
            {
                if (_factories.TryGetValue(typeof(TProduct), out var factory))
                    return (IConfigurableFactory<TProduct, TConfig>)factory;
                
                throw new InvalidOperationException($"Configurable factory for {typeof(TProduct).Name} not registered");
            }
        }
    }
    
    // ===== PATRÓN BUILDER GENÉRICO =====
    namespace BuilderPattern
    {
        // Builder genérico básico
        public interface IGenericBuilder<TProduct> where TProduct : class
        {
            IGenericBuilder<TProduct> WithProperty<TValue>(string propertyName, TValue value);
            IGenericBuilder<TProduct> WithAction(Action<TProduct> action);
            TProduct Build();
        }
        
        public class GenericBuilder<TProduct> : IGenericBuilder<TProduct> where TProduct : class, new()
        {
            private readonly List<Action<TProduct>> _actions = new List<Action<TProduct>>();
            private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();
            
            public IGenericBuilder<TProduct> WithProperty<TValue>(string propertyName, TValue value)
            {
                _properties[propertyName] = value;
                return this;
            }
            
            public IGenericBuilder<TProduct> WithAction(Action<TProduct> action)
            {
                if (action != null)
                    _actions.Add(action);
                return this;
            }
            
            public TProduct Build()
            {
                var product = new TProduct();
                
                // Aplicar propiedades usando reflection
                var type = typeof(TProduct);
                foreach (var kvp in _properties)
                {
                    var property = type.GetProperty(kvp.Key);
                    if (property?.CanWrite == true)
                    {
                        property.SetValue(product, kvp.Value);
                    }
                }
                
                // Aplicar acciones
                foreach (var action in _actions)
                {
                    action(product);
                }
                
                return product;
            }
        }
        
        // Builder genérico con director
        public class GenericBuilderDirector<TProduct> where TProduct : class
        {
            private readonly IGenericBuilder<TProduct> _builder;
            
            public GenericBuilderDirector(IGenericBuilder<TProduct> builder)
            {
                _builder = builder ?? throw new ArgumentNullException(nameof(builder));
            }
            
            public TProduct BuildStandard()
            {
                return _builder.Build();
            }
            
            public TProduct BuildWithDefaults()
            {
                // Aplicar valores por defecto
                return _builder.Build();
            }
            
            public TProduct BuildCustom(Action<IGenericBuilder<TProduct>> customizations)
            {
                customizations?.Invoke(_builder);
                return _builder.Build();
            }
        }
        
        // Builder genérico con validación
        public class ValidatingGenericBuilder<TProduct> : IGenericBuilder<TProduct> 
            where TProduct : class, new()
        {
            private readonly IGenericBuilder<TProduct> _innerBuilder;
            private readonly List<Func<TProduct, bool>> _validators = new List<Func<TProduct, bool>>();
            private readonly List<string> _validationErrors = new List<string>();
            
            public ValidatingGenericBuilder(IGenericBuilder<TProduct> innerBuilder)
            {
                _innerBuilder = innerBuilder ?? throw new ArgumentNullException(nameof(innerBuilder));
            }
            
            public IGenericBuilder<TProduct> WithProperty<TValue>(string propertyName, TValue value)
            {
                _innerBuilder.WithProperty(propertyName, value);
                return this;
            }
            
            public IGenericBuilder<TProduct> WithAction(Action<TProduct> action)
            {
                _innerBuilder.WithAction(action);
                return this;
            }
            
            public IGenericBuilder<TProduct> WithValidator(Func<TProduct, bool> validator, string errorMessage = null)
            {
                if (validator != null)
                {
                    _validators.Add(validator);
                }
                return this;
            }
            
            public TProduct Build()
            {
                var product = _innerBuilder.Build();
                _validationErrors.Clear();
                
                // Ejecutar validaciones
                foreach (var validator in _validators)
                {
                    if (!validator(product))
                    {
                        _validationErrors.Add("Validation failed");
                    }
                }
                
                if (_validationErrors.Any())
                {
                    throw new InvalidOperationException($"Validation failed: {string.Join(", ", _validationErrors)}");
                }
                
                return product;
            }
            
            public IEnumerable<string> GetValidationErrors() => _validationErrors;
        }
    }
    
    // ===== PATRÓN SINGLETON GENÉRICO =====
    namespace SingletonPattern
    {
        // Singleton genérico thread-safe
        public class GenericSingleton<T> where T : class, new()
        {
            private static readonly Lazy<T> _instance = new Lazy<T>(() => new T(), LazyThreadSafetyMode.ExecutionAndPublication);
            
            public static T Instance => _instance.Value;
            
            protected GenericSingleton() { }
        }
        
        // Singleton genérico con factory
        public class GenericSingletonWithFactory<T> where T : class
        {
            private static readonly Lazy<T> _instance;
            private static readonly object _lock = new object();
            
            static GenericSingletonWithFactory()
            {
                _instance = new Lazy<T>(() => CreateInstance(), LazyThreadSafetyMode.ExecutionAndPublication);
            }
            
            public static T Instance => _instance.Value;
            
            private static T CreateInstance()
            {
                // Usar reflection para crear la instancia
                var type = typeof(T);
                var constructor = type.GetConstructor(Type.EmptyTypes);
                
                if (constructor != null)
                    return (T)constructor.Invoke(null);
                
                throw new InvalidOperationException($"Type {type.Name} must have a parameterless constructor");
            }
        }
        
        // Singleton genérico con configuración
        public class ConfigurableGenericSingleton<T, TConfig> where T : class
        {
            private static readonly Dictionary<TConfig, T> _instances = new Dictionary<TConfig, T>();
            private static readonly object _lock = new object();
            private readonly Func<TConfig, T> _factory;
            
            public ConfigurableGenericSingleton(Func<TConfig, T> factory)
            {
                _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            }
            
            public T GetInstance(TConfig config)
            {
                lock (_lock)
                {
                    if (!_instances.TryGetValue(config, out var instance))
                    {
                        instance = _factory(config);
                        _instances[config] = instance;
                    }
                    
                    return instance;
                }
            }
            
            public void ClearInstances()
            {
                lock (_lock)
                {
                    _instances.Clear();
                }
            }
        }
    }
    
    // ===== PATRÓN OBSERVER GENÉRICO =====
    namespace ObserverPattern
    {
        // Observer genérico básico
        public interface IGenericObserver<TSubject>
        {
            void OnNext(TSubject subject);
            void OnCompleted();
            void OnError(Exception error);
        }
        
        public interface IGenericObservable<TSubject>
        {
            IDisposable Subscribe(IGenericObserver<TSubject> observer);
            void Unsubscribe(IGenericObserver<TSubject> observer);
            void Notify(TSubject subject);
        }
        
        public class GenericObservable<TSubject> : IGenericObservable<TSubject>
        {
            private readonly List<IGenericObserver<TSubject>> _observers = new List<IGenericObserver<TSubject>>();
            private readonly object _lock = new object();
            
            public IDisposable Subscribe(IGenericObserver<TSubject> observer)
            {
                if (observer == null) throw new ArgumentNullException(nameof(observer));
                
                lock (_lock)
                {
                    _observers.Add(observer);
                }
                
                return new ObserverSubscription<TSubject>(this, observer);
            }
            
            public void Unsubscribe(IGenericObserver<TSubject> observer)
            {
                if (observer == null) return;
                
                lock (_lock)
                {
                    _observers.Remove(observer);
                }
            }
            
            public void Notify(TSubject subject)
            {
                List<IGenericObserver<TSubject>> observersCopy;
                
                lock (_lock)
                {
                    observersCopy = _observers.ToList();
                }
                
                foreach (var observer in observersCopy)
                {
                    try
                    {
                        observer.OnNext(subject);
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                }
            }
            
            public void Complete()
            {
                List<IGenericObserver<TSubject>> observersCopy;
                
                lock (_lock)
                {
                    observersCopy = _observers.ToList();
                    _observers.Clear();
                }
                
                foreach (var observer in observersCopy)
                {
                    try
                    {
                        observer.OnCompleted();
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                }
            }
        }
        
        public class ObserverSubscription<TSubject> : IDisposable
        {
            private readonly GenericObservable<TSubject> _observable;
            private readonly IGenericObserver<TSubject> _observer;
            private bool _disposed;
            
            public ObserverSubscription(GenericObservable<TSubject> observable, IGenericObserver<TSubject> observer)
            {
                _observable = observable;
                _observer = observer;
            }
            
            public void Dispose()
            {
                if (!_disposed)
                {
                    _observable.Unsubscribe(_observer);
                    _disposed = true;
                }
            }
        }
        
        // Observer genérico con filtros
        public class FilteredGenericObserver<TSubject> : IGenericObserver<TSubject>
        {
            private readonly IGenericObserver<TSubject> _innerObserver;
            private readonly Func<TSubject, bool> _filter;
            
            public FilteredGenericObserver(IGenericObserver<TSubject> innerObserver, Func<TSubject, bool> filter)
            {
                _innerObserver = innerObserver ?? throw new ArgumentNullException(nameof(innerObserver));
                _filter = filter ?? throw new ArgumentNullException(nameof(filter));
            }
            
            public void OnNext(TSubject subject)
            {
                if (_filter(subject))
                {
                    _innerObserver.OnNext(subject);
                }
            }
            
            public void OnCompleted() => _innerObserver.OnCompleted();
            public void OnError(Exception error) => _innerObserver.OnError(error);
        }
    }
    
    // ===== PATRÓN COMMAND GENÉRICO =====
    namespace CommandPattern
    {
        // Command genérico básico
        public interface IGenericCommand<TParameter>
        {
            void Execute(TParameter parameter);
            bool CanExecute(TParameter parameter);
        }
        
        public interface IGenericCommandHandler<TCommand, TResult> where TCommand : class
        {
            Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
        }
        
        public class GenericCommandInvoker<TParameter>
        {
            private readonly IGenericCommand<TParameter> _command;
            
            public GenericCommandInvoker(IGenericCommand<TParameter> command)
            {
                _command = command ?? throw new ArgumentNullException(nameof(command));
            }
            
            public bool TryExecute(TParameter parameter)
            {
                if (_command.CanExecute(parameter))
                {
                    _command.Execute(parameter);
                    return true;
                }
                
                return false;
            }
            
            public void ExecuteOrThrow(TParameter parameter)
            {
                if (!_command.CanExecute(parameter))
                {
                    throw new InvalidOperationException("Command cannot be executed");
                }
                
                _command.Execute(parameter);
            }
        }
        
        // Command genérico con undo/redo
        public interface IGenericUndoableCommand<TParameter> : IGenericCommand<TParameter>
        {
            void Undo(TParameter parameter);
            bool CanUndo(TParameter parameter);
        }
        
        public class GenericCommandHistory<TParameter>
        {
            private readonly Stack<IGenericUndoableCommand<TParameter>> _executedCommands = new Stack<IGenericUndoableCommand<TParameter>>();
            private readonly Stack<IGenericUndoableCommand<TParameter>> _undoneCommands = new Stack<IGenericUndoableCommand<TParameter>>();
            
            public void ExecuteCommand(IGenericUndoableCommand<TParameter> command, TParameter parameter)
            {
                if (command.CanExecute(parameter))
                {
                    command.Execute(parameter);
                    _executedCommands.Push(command);
                    _undoneCommands.Clear(); // Limpiar comandos deshechos al ejecutar uno nuevo
                }
            }
            
            public bool CanUndo() => _executedCommands.Count > 0;
            
            public void Undo(TParameter parameter)
            {
                if (CanUndo())
                {
                    var command = _executedCommands.Pop();
                    if (command.CanUndo(parameter))
                    {
                        command.Undo(parameter);
                        _undoneCommands.Push(command);
                    }
                }
            }
            
            public bool CanRedo() => _undoneCommands.Count > 0;
            
            public void Redo(TParameter parameter)
            {
                if (CanRedo())
                {
                    var command = _undoneCommands.Pop();
                    if (command.CanExecute(parameter))
                    {
                        command.Execute(parameter);
                        _executedCommands.Push(command);
                    }
                }
            }
            
            public void Clear()
            {
                _executedCommands.Clear();
                _undoneCommands.Clear();
            }
        }
    }
    
    // ===== PATRÓN STRATEGY GENÉRICO =====
    namespace StrategyPattern
    {
        // Strategy genérico básico
        public interface IGenericStrategy<TInput, TOutput>
        {
            TOutput Execute(TInput input);
            bool CanExecute(TInput input);
            string StrategyName { get; }
        }
        
        public class GenericStrategyContext<TInput, TOutput>
        {
            private readonly List<IGenericStrategy<TInput, TOutput>> _strategies = new List<IGenericStrategy<TInput, TOutput>>();
            private IGenericStrategy<TInput, TOutput> _defaultStrategy;
            
            public void AddStrategy(IGenericStrategy<TInput, TOutput> strategy)
            {
                if (strategy != null)
                    _strategies.Add(strategy);
            }
            
            public void SetDefaultStrategy(IGenericStrategy<TInput, TOutput> strategy)
            {
                _defaultStrategy = strategy;
            }
            
            public TOutput Execute(TInput input)
            {
                // Buscar la primera estrategia que pueda ejecutar
                var strategy = _strategies.FirstOrDefault(s => s.CanExecute(input));
                
                if (strategy != null)
                    return strategy.Execute(input);
                
                // Usar estrategia por defecto si está disponible
                if (_defaultStrategy != null)
                    return _defaultStrategy.Execute(input);
                
                throw new InvalidOperationException("No strategy can execute the input");
            }
            
            public TOutput ExecuteWithStrategy(string strategyName, TInput input)
            {
                var strategy = _strategies.FirstOrDefault(s => s.StrategyName == strategyName);
                
                if (strategy == null)
                    throw new ArgumentException($"Strategy '{strategyName}' not found");
                
                if (!strategy.CanExecute(input))
                    throw new InvalidOperationException($"Strategy '{strategyName}' cannot execute the input");
                
                return strategy.Execute(input);
            }
        }
        
        // Strategy genérico con fallback
        public class FallbackGenericStrategy<TInput, TOutput> : IGenericStrategy<TInput, TOutput>
        {
            private readonly IGenericStrategy<TInput, TOutput> _primaryStrategy;
            private readonly IGenericStrategy<TInput, TOutput> _fallbackStrategy;
            
            public FallbackGenericStrategy(
                IGenericStrategy<TInput, TOutput> primaryStrategy,
                IGenericStrategy<TInput, TOutput> fallbackStrategy)
            {
                _primaryStrategy = primaryStrategy ?? throw new ArgumentNullException(nameof(primaryStrategy));
                _fallbackStrategy = fallbackStrategy ?? throw new ArgumentNullException(nameof(fallbackStrategy));
            }
            
            public TOutput Execute(TInput input)
            {
                try
                {
                    if (_primaryStrategy.CanExecute(input))
                        return _primaryStrategy.Execute(input);
                }
                catch
                {
                    // Fallback a la estrategia secundaria
                }
                
                if (_fallbackStrategy.CanExecute(input))
                    return _fallbackStrategy.Execute(input);
                
                throw new InvalidOperationException("Neither primary nor fallback strategy can execute the input");
            }
            
            public bool CanExecute(TInput input)
            {
                return _primaryStrategy.CanExecute(input) || _fallbackStrategy.CanExecute(input);
            }
            
            public string StrategyName => $"Fallback({_primaryStrategy.StrategyName}, {_fallbackStrategy.StrategyName})";
        }
    }
    
    // ===== PATRÓN DECORATOR GENÉRICO =====
    namespace DecoratorPattern
    {
        // Decorator genérico básico
        public abstract class GenericDecorator<T> where T : class
        {
            protected readonly T _component;
            
            protected GenericDecorator(T component)
            {
                _component = component ?? throw new ArgumentNullException(nameof(component));
            }
            
            public virtual T GetComponent() => _component;
        }
        
        // Decorator genérico con logging
        public class LoggingGenericDecorator<T> : GenericDecorator<T> where T : class
        {
            private readonly ILogger _logger;
            
            public LoggingGenericDecorator(T component, ILogger logger) : base(component)
            {
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }
            
            public T ExecuteWithLogging<TResult>(Func<T, TResult> operation, string operationName)
            {
                _logger.LogInformation($"Starting operation: {operationName}");
                
                try
                {
                    var result = operation(_component);
                    _logger.LogInformation($"Operation {operationName} completed successfully");
                    return _component;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Operation {operationName} failed");
                    throw;
                }
            }
        }
        
        // Decorator genérico con caching
        public class CachingGenericDecorator<T> : GenericDecorator<T> where T : class
        {
            private readonly ICache<string, object> _cache;
            private readonly TimeSpan _cacheTimeout;
            
            public CachingGenericDecorator(T component, ICache<string, object> cache, TimeSpan? cacheTimeout = null) 
                : base(component)
            {
                _cache = cache ?? throw new ArgumentNullException(nameof(cache));
                _cacheTimeout = cacheTimeout ?? TimeSpan.FromMinutes(5);
            }
            
            public TResult ExecuteWithCaching<TResult>(Func<T, TResult> operation, string cacheKey)
            {
                if (_cache.TryGet(cacheKey, out object cachedResult))
                {
                    return (TResult)cachedResult;
                }
                
                var result = operation(_component);
                _cache.Set(cacheKey, result, _cacheTimeout);
                
                return result;
            }
        }
        
        // Decorator genérico con retry
        public class RetryGenericDecorator<T> : GenericDecorator<T> where T : class
        {
            private readonly int _maxRetries;
            private readonly TimeSpan _retryDelay;
            
            public RetryGenericDecorator(T component, int maxRetries = 3, TimeSpan? retryDelay = null) 
                : base(component)
            {
                _maxRetries = maxRetries;
                _retryDelay = retryDelay ?? TimeSpan.FromSeconds(1);
            }
            
            public async Task<TResult> ExecuteWithRetryAsync<TResult>(Func<T, Task<TResult>> operation)
            {
                var lastException = default(Exception);
                
                for (int attempt = 1; attempt <= _maxRetries; attempt++)
                {
                    try
                    {
                        return await operation(_component);
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                        
                        if (attempt < _maxRetries)
                        {
                            await Task.Delay(_retryDelay);
                        }
                    }
                }
                
                throw new InvalidOperationException($"Operation failed after {_maxRetries} attempts", lastException);
            }
        }
    }
    
    // ===== PATRÓN ADAPTER GENÉRICO =====
    namespace AdapterPattern
    {
        // Adapter genérico básico
        public interface IGenericAdapter<TSource, TTarget>
        {
            TTarget Adapt(TSource source);
            TSource ReverseAdapt(TTarget target);
        }
        
        public class GenericAdapter<TSource, TTarget> : IGenericAdapter<TSource, TTarget>
        {
            private readonly Func<TSource, TTarget> _adaptFunc;
            private readonly Func<TTarget, TSource> _reverseAdaptFunc;
            
            public GenericAdapter(Func<TSource, TTarget> adaptFunc, Func<TTarget, TSource> reverseAdaptFunc = null)
            {
                _adaptFunc = adaptFunc ?? throw new ArgumentNullException(nameof(adaptFunc));
                _reverseAdaptFunc = reverseAdaptFunc;
            }
            
            public TTarget Adapt(TSource source) => _adaptFunc(source);
            
            public TSource ReverseAdapt(TTarget target)
            {
                if (_reverseAdaptFunc != null)
                    return _reverseAdaptFunc(target);
                
                throw new NotSupportedException("Reverse adaptation not supported");
            }
        }
        
        // Adapter genérico con validación
        public class ValidatingGenericAdapter<TSource, TTarget> : IGenericAdapter<TSource, TTarget>
        {
            private readonly IGenericAdapter<TSource, TTarget> _innerAdapter;
            private readonly Func<TSource, bool> _sourceValidator;
            private readonly Func<TTarget, bool> _targetValidator;
            
            public ValidatingGenericAdapter(
                IGenericAdapter<TSource, TTarget> innerAdapter,
                Func<TSource, bool> sourceValidator = null,
                Func<TTarget, bool> targetValidator = null)
            {
                _innerAdapter = innerAdapter ?? throw new ArgumentNullException(nameof(innerAdapter));
                _sourceValidator = sourceValidator;
                _targetValidator = targetValidator;
            }
            
            public TTarget Adapt(TSource source)
            {
                if (_sourceValidator != null && !_sourceValidator(source))
                {
                    throw new ArgumentException("Source validation failed");
                }
                
                var result = _innerAdapter.Adapt(source);
                
                if (_targetValidator != null && !_targetValidator(result))
                {
                    throw new InvalidOperationException("Target validation failed");
                }
                
                return result;
            }
            
            public TSource ReverseAdapt(TTarget target)
            {
                if (_targetValidator != null && !_targetValidator(target))
                {
                    throw new ArgumentException("Target validation failed");
                }
                
                var result = _innerAdapter.ReverseAdapt(target);
                
                if (_sourceValidator != null && !_sourceValidator(result))
                {
                    throw new InvalidOperationException("Source validation failed");
                }
                
                return result;
            }
        }
    }
    
    // ===== PATRÓN COMPOSITE GENÉRICO =====
    namespace CompositePattern
    {
        // Componente genérico base
        public interface IGenericComponent<T>
        {
            void Operation(T context);
            void Add(IGenericComponent<T> component);
            void Remove(IGenericComponent<T> component);
            IEnumerable<IGenericComponent<T>> GetChildren();
        }
        
        // Hoja genérica
        public class GenericLeaf<T> : IGenericComponent<T>
        {
            private readonly string _name;
            private readonly Action<T> _operation;
            
            public GenericLeaf(string name, Action<T> operation)
            {
                _name = name ?? throw new ArgumentNullException(nameof(name));
                _operation = operation ?? throw new ArgumentNullException(nameof(operation));
            }
            
            public void Operation(T context) => _operation(context);
            
            public void Add(IGenericComponent<T> component)
            {
                throw new InvalidOperationException("Cannot add to leaf");
            }
            
            public void Remove(IGenericComponent<T> component)
            {
                throw new InvalidOperationException("Cannot remove from leaf");
            }
            
            public IEnumerable<IGenericComponent<T>> GetChildren()
            {
                return Enumerable.Empty<IGenericComponent<T>>();
            }
            
            public override string ToString() => _name;
        }
        
        // Composite genérico
        public class GenericComposite<T> : IGenericComponent<T>
        {
            private readonly string _name;
            private readonly List<IGenericComponent<T>> _children = new List<IGenericComponent<T>>();
            
            public GenericComposite(string name)
            {
                _name = name ?? throw new ArgumentNullException(nameof(name));
            }
            
            public void Operation(T context)
            {
                foreach (var child in _children)
                {
                    child.Operation(context);
                }
            }
            
            public void Add(IGenericComponent<T> component)
            {
                if (component != null)
                    _children.Add(component);
            }
            
            public void Remove(IGenericComponent<T> component)
            {
                if (component != null)
                    _children.Remove(component);
            }
            
            public IEnumerable<IGenericComponent<T>> GetChildren() => _children.ToList();
            
            public override string ToString() => $"{_name} ({_children.Count} children)";
        }
    }
    
    // ===== PATRÓN ITERATOR GENÉRICO =====
    namespace IteratorPattern
    {
        // Iterator genérico básico
        public interface IGenericIterator<T>
        {
            T Current { get; }
            bool MoveNext();
            void Reset();
            bool HasNext { get; }
        }
        
        public class GenericListIterator<T> : IGenericIterator<T>
        {
            private readonly IList<T> _list;
            private int _currentIndex;
            
            public GenericListIterator(IList<T> list)
            {
                _list = list ?? throw new ArgumentNullException(nameof(list));
                _currentIndex = -1;
            }
            
            public T Current
            {
                get
                {
                    if (_currentIndex < 0 || _currentIndex >= _list.Count)
                        throw new InvalidOperationException("Iterator is not positioned on a valid element");
                    
                    return _list[_currentIndex];
                }
            }
            
            public bool MoveNext()
            {
                _currentIndex++;
                return _currentIndex < _list.Count;
            }
            
            public void Reset()
            {
                _currentIndex = -1;
            }
            
            public bool HasNext => _currentIndex + 1 < _list.Count;
        }
        
        // Iterator genérico con filtros
        public class FilteredGenericIterator<T> : IGenericIterator<T>
        {
            private readonly IGenericIterator<T> _innerIterator;
            private readonly Func<T, bool> _filter;
            private T _current;
            private bool _hasCurrent;
            
            public FilteredGenericIterator(IGenericIterator<T> innerIterator, Func<T, bool> filter)
            {
                _innerIterator = innerIterator ?? throw new ArgumentNullException(nameof(innerIterator));
                _filter = filter ?? throw new ArgumentNullException(nameof(filter));
            }
            
            public T Current
            {
                get
                {
                    if (!_hasCurrent)
                        throw new InvalidOperationException("Iterator is not positioned on a valid element");
                    
                    return _current;
                }
            }
            
            public bool MoveNext()
            {
                while (_innerIterator.MoveNext())
                {
                    if (_filter(_innerIterator.Current))
                    {
                        _current = _innerIterator.Current;
                        _hasCurrent = true;
                        return true;
                    }
                }
                
                _hasCurrent = false;
                return false;
            }
            
            public void Reset()
            {
                _innerIterator.Reset();
                _hasCurrent = false;
            }
            
            public bool HasNext
            {
                get
                {
                    var tempIterator = new FilteredGenericIterator<T>(_innerIterator, _filter);
                    return tempIterator.MoveNext();
                }
            }
        }
    }
    
    // ===== INTERFACES AUXILIARES =====
    namespace Interfaces
    {
        public interface ILogger
        {
            void LogInformation(string message);
            void LogError(Exception ex, string message);
        }
        
        public interface ICache<TKey, TValue>
        {
            bool TryGet(TKey key, out TValue value);
            void Set(TKey key, TValue value, TimeSpan? ttl = null);
        }
    }
}

// Uso de Patrones con Generics
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Patrones con Generics - Clase 9 ===\n");
        
        Console.WriteLine("Los patrones implementados incluyen:");
        Console.WriteLine("1. Factory Pattern genérico con configuración");
        Console.WriteLine("2. Builder Pattern genérico con validación");
        Console.WriteLine("3. Singleton Pattern genérico thread-safe");
        Console.WriteLine("4. Observer Pattern genérico con filtros");
        Console.WriteLine("5. Command Pattern genérico con undo/redo");
        Console.WriteLine("6. Strategy Pattern genérico con fallback");
        Console.WriteLine("7. Decorator Pattern genérico con funcionalidades");
        Console.WriteLine("8. Adapter Pattern genérico con validación");
        Console.WriteLine("9. Composite Pattern genérico");
        Console.WriteLine("10. Iterator Pattern genérico con filtros");
        
        Console.WriteLine("\nBeneficios de esta implementación:");
        Console.WriteLine("- Patrones type-safe y flexibles");
        Console.WriteLine("- Reutilización de código genérico");
        Console.WriteLine("- Adaptabilidad a diferentes tipos de datos");
        Console.WriteLine("- Mejor mantenibilidad y extensibilidad");
        Console.WriteLine("- Patrones más robustos y seguros");
        
        Console.WriteLine("\nCasos de uso principales:");
        Console.WriteLine("- Frameworks de aplicación genéricos");
        Console.WriteLine("- Sistemas de plugins genéricos");
        Console.WriteLine("- Librerías de utilidades genéricas");
        Console.WriteLine("- Patrones de arquitectura genéricos");
        Console.WriteLine("- Sistemas de configuración genéricos");
        Console.WriteLine("- Frameworks de testing genéricos");
    }
}
