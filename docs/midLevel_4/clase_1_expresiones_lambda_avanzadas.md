# 🚀 Clase 1: Expresiones Lambda Avanzadas

## 📋 Información de la Clase

- **Módulo**: Mid Level 4 - LINQ y Expresiones Lambda
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Conocimientos básicos de C# y programación funcional

## 🎯 Objetivos de Aprendizaje

- Dominar expresiones lambda complejas y anidadas
- Implementar delegados personalizados con lambda
- Usar expresiones lambda con eventos y callbacks
- Crear funciones de orden superior con lambda

---

## 📚 Navegación del Módulo 4

| Clase | Tema | Enlace |
|-------|------|--------|
| **Clase 1** | **Expresiones Lambda Avanzadas** | ← Estás aquí |
| [Clase 2](clase_2_operadores_linq_basicos.md) | Operadores LINQ Básicos | Siguiente → |
| [Clase 3](clase_3_operadores_linq_avanzados.md) | Operadores LINQ Avanzados | |
| [Clase 4](clase_4_linq_to_objects.md) | LINQ to Objects | |
| [Clase 5](clase_5_linq_to_xml.md) | LINQ to XML | |
| [Clase 6](clase_6_linq_to_sql.md) | LINQ to SQL | |
| [Clase 7](clase_7_linq_performance.md) | LINQ y Performance | |
| [Clase 8](clase_8_linq_optimization.md) | Optimización de LINQ | |
| [Clase 9](clase_9_linq_extension_methods.md) | Métodos de Extensión LINQ | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Biblioteca | |

**← [Volver al README del Módulo 4](../midLevel_4/README.md)**

---

## 📚 Contenido Teórico

### 1. Expresiones Lambda Avanzadas

Las expresiones lambda avanzadas permiten crear código más funcional, legible y mantenible. Vamos a explorar técnicas avanzadas.

```csharp
// ===== EXPRESIONES LAMBDA AVANZADAS - IMPLEMENTACIÓN COMPLETA =====
namespace AdvancedLambdaExpressions
{
    // ===== DELEGADOS PERSONALIZADOS =====
    namespace CustomDelegates
    {
        // Delegado para operaciones matemáticas
        public delegate T MathOperation<T>(T a, T b);
        
        // Delegado para validación
        public delegate bool ValidationRule<T>(T item);
        
        // Delegado para transformación
        public delegate TResult Transform<TInput, TResult>(TInput input);
        
        // Delegado para acciones con contexto
        public delegate void ContextAction<T>(T item, object context);
        
        // Delegado para comparación
        public delegate int Comparison<T>(T x, T y);
        
        public class LambdaDelegates
        {
            // Operaciones matemáticas con lambda
            public static MathOperation<int> Add = (a, b) => a + b;
            public static MathOperation<int> Subtract = (a, b) => a - b;
            public static MathOperation<int> Multiply = (a, b) => a * b;
            public static MathOperation<double> Divide = (a, b) => b != 0 ? a / b : 0;
            
            // Validaciones con lambda
            public static ValidationRule<string> IsNotEmpty = s => !string.IsNullOrWhiteSpace(s);
            public static ValidationRule<int> IsPositive = n => n > 0;
            public static ValidationRule<int> IsEven = n => n % 2 == 0;
            public static ValidationRule<string> IsEmail = s => s.Contains("@") && s.Contains(".");
            
            // Transformaciones con lambda
            public static Transform<string, string> ToUpper = s => s?.ToUpper();
            public static Transform<string, int> GetLength = s => s?.Length ?? 0;
            public static Transform<int, string> ToBinary = n => Convert.ToString(n, 2);
            
            // Acciones con contexto
            public static ContextAction<string> LogWithContext = (item, context) => 
                Console.WriteLine($"[{context}] Processing: {item}");
            
            // Comparaciones con lambda
            public static Comparison<string> StringLengthComparison = (x, y) => 
                x?.Length.CompareTo(y?.Length) ?? 0;
            public static Comparison<int> NumericComparison = (x, y) => x.CompareTo(y);
        }
    }
    
    // ===== FUNCIONES DE ORDEN SUPERIOR =====
    namespace HigherOrderFunctions
    {
        public class FunctionalOperations
        {
            // Función que toma una función como parámetro
            public static T ApplyOperation<T>(T value, Func<T, T> operation)
            {
                return operation(value);
            }
            
            // Función que retorna una función
            public static Func<T, T> CreateMultiplier<T>(T factor) where T : struct
            {
                return x => (dynamic)x * (dynamic)factor;
            }
            
            // Función que combina dos funciones
            public static Func<T, TResult> Compose<T, TIntermediate, TResult>(
                Func<T, TIntermediate> first, 
                Func<TIntermediate, TResult> second)
            {
                return x => second(first(x));
            }
            
            // Función que aplica una función a cada elemento
            public static IEnumerable<TResult> Map<T, TResult>(
                IEnumerable<T> items, 
                Func<T, TResult> mapper)
            {
                foreach (var item in items)
                {
                    yield return mapper(item);
                }
            }
            
            // Función que filtra elementos
            public static IEnumerable<T> Filter<T>(
                IEnumerable<T> items, 
                Func<T, bool> predicate)
            {
                foreach (var item in items)
                {
                    if (predicate(item))
                    {
                        yield return item;
                    }
                }
            }
            
            // Función que reduce una colección
            public static TAccumulate Reduce<T, TAccumulate>(
                IEnumerable<T> items, 
                TAccumulate seed, 
                Func<TAccumulate, T, TAccumulate> reducer)
            {
                var result = seed;
                foreach (var item in items)
                {
                    result = reducer(result, item);
                }
                return result;
            }
            
            // Función que aplica una función hasta que se cumpla una condición
            public static T ApplyUntil<T>(T value, Func<T, T> operation, Func<T, bool> condition)
            {
                var current = value;
                while (!condition(current))
                {
                    current = operation(current);
                }
                return current;
            }
            
            // Función que memoiza resultados
            public static Func<T, TResult> Memoize<T, TResult>(Func<T, TResult> function)
            {
                var cache = new Dictionary<T, TResult>();
                return input =>
                {
                    if (cache.TryGetValue(input, out var result))
                    {
                        return result;
                    }
                    
                    result = function(input);
                    cache[input] = result;
                    return result;
                };
            }
        }
    }
    
    // ===== LAMBDA CON EVENTOS =====
    namespace LambdaEvents
    {
        public class EventPublisher
        {
            // Eventos con lambda
            public event Action<string> MessageReceived;
            public event Action<int, string> DataProcessed;
            public event Func<string, bool> ValidationRequested;
            
            public void PublishMessage(string message)
            {
                MessageReceived?.Invoke(message);
            }
            
            public void ProcessData(int id, string data)
            {
                DataProcessed?.Invoke(id, data);
            }
            
            public bool ValidateData(string data)
            {
                return ValidationRequested?.Invoke(data) ?? true;
            }
        }
        
        public class EventSubscriber
        {
            private readonly EventPublisher _publisher;
            
            public EventSubscriber(EventPublisher publisher)
            {
                _publisher = publisher;
                
                // Suscribirse a eventos usando lambda
                _publisher.MessageReceived += message => 
                    Console.WriteLine($"Received: {message}");
                
                _publisher.DataProcessed += (id, data) => 
                    Console.WriteLine($"Processed ID {id}: {data}");
                
                _publisher.ValidationRequested += data => 
                    !string.IsNullOrEmpty(data) && data.Length > 3;
            }
        }
    }
    
    // ===== LAMBDA CON CALLBACKS =====
    namespace LambdaCallbacks
    {
        public class AsyncProcessor
        {
            // Procesamiento asíncrono con callbacks lambda
            public async Task ProcessAsync<T>(T data, Action<T> onSuccess, Action<Exception> onError)
            {
                try
                {
                    await Task.Delay(100); // Simular procesamiento
                    onSuccess?.Invoke(data);
                }
                catch (Exception ex)
                {
                    onError?.Invoke(ex);
                }
            }
            
            // Procesamiento con progreso
            public async Task ProcessWithProgress<T>(IEnumerable<T> items, Action<int> onProgress)
            {
                var total = items.Count();
                var current = 0;
                
                foreach (var item in items)
                {
                    await Task.Delay(50); // Simular procesamiento
                    current++;
                    onProgress?.Invoke((current * 100) / total);
                }
            }
            
            // Procesamiento con retry
            public async Task<T> ProcessWithRetry<T>(Func<Task<T>> operation, int maxRetries = 3)
            {
                for (int i = 0; i < maxRetries; i++)
                {
                    try
                    {
                        return await operation();
                    }
                    catch (Exception) when (i < maxRetries - 1)
                    {
                        await Task.Delay(1000 * (i + 1)); // Backoff exponencial
                    }
                }
                
                return await operation(); // Último intento
            }
        }
        
        public class DataProcessor
        {
            // Procesamiento de datos con callbacks lambda
            public void ProcessData<T>(IEnumerable<T> data, 
                Func<T, bool> filter, 
                Action<T> processor, 
                Action<int> onComplete)
            {
                var count = 0;
                foreach (var item in data)
                {
                    if (filter(item))
                    {
                        processor(item);
                        count++;
                    }
                }
                onComplete?.Invoke(count);
            }
            
            // Transformación con validación
            public IEnumerable<TResult> TransformWithValidation<TInput, TResult>(
                IEnumerable<TInput> items,
                Func<TInput, TResult> transformer,
                Func<TResult, bool> validator)
            {
                foreach (var item in items)
                {
                    var result = transformer(item);
                    if (validator(result))
                    {
                        yield return result;
                    }
                }
            }
        }
    }
    
    // ===== LAMBDA ANIDADAS =====
    namespace NestedLambdas
    {
        public class LambdaNesting
        {
            // Lambda anidada simple
            public static Func<int, Func<int, int>> CreateAdder = x => y => x + y;
            
            // Lambda anidada con múltiples niveles
            public static Func<int, Func<int, Func<int, int>>> CreateTripleAdder = 
                x => y => z => x + y + z;
            
            // Lambda que retorna una lambda con contexto
            public static Func<string, Func<string, string>> CreatePrefixer = 
                prefix => text => $"{prefix}: {text}";
            
            // Lambda con closure
            public static Func<int, Func<int, int>> CreateMultiplierWithBase(int baseValue)
            {
                return multiplier => value => baseValue * multiplier * value;
            }
            
            // Lambda que retorna una función de validación
            public static Func<int, Func<string, bool>> CreateLengthValidator = 
                minLength => text => text?.Length >= minLength;
            
            // Lambda que retorna una función de comparación
            public static Func<string, Func<string, int>> CreateStringComparer = 
                comparisonType => (x, y) => 
                {
                    return comparisonType.ToLower() switch
                    {
                        "length" => x?.Length.CompareTo(y?.Length) ?? 0,
                        "ignorecase" => string.Compare(x, y, StringComparison.OrdinalIgnoreCase),
                        "ordinal" => string.Compare(x, y, StringComparison.Ordinal),
                        _ => string.Compare(x, y)
                    };
                };
        }
        
        public class AdvancedNesting
        {
            // Lambda que retorna una función que retorna otra función
            public static Func<int, Func<Func<int, int>, Func<int, int>>> CreateFunctionComposer = 
                factor => operation => value => operation(value) * factor;
            
            // Lambda con múltiples parámetros anidados
            public static Func<int, Func<int, Func<int, Func<int, int>>>> CreateQuadrupleAdder = 
                a => b => c => d => a + b + c + d;
            
            // Lambda que retorna una función de filtrado
            public static Func<Func<T, bool>, Func<IEnumerable<T>, IEnumerable<T>>> CreateFilter = 
                predicate => items => items.Where(predicate);
            
            // Lambda que retorna una función de mapeo
            public static Func<Func<T, TResult>, Func<IEnumerable<T>, IEnumerable<TResult>>> CreateMapper = 
                mapper => items => items.Select(mapper);
        }
    }
    
    // ===== LAMBDA CON PATRONES =====
    namespace LambdaPatterns
    {
        public class StrategyPattern
        {
            // Estrategias como lambda
            public static Func<int, int, int> AddStrategy = (a, b) => a + b;
            public static Func<int, int, int> SubtractStrategy = (a, b) => a - b;
            public static Func<int, int, int> MultiplyStrategy = (a, b) => a * b;
            
            public static int ExecuteStrategy(int a, int b, Func<int, int, int> strategy)
            {
                return strategy(a, b);
            }
        }
        
        public class CommandPattern
        {
            // Comandos como lambda
            public static Action<string> LogCommand = message => Console.WriteLine($"[LOG] {message}");
            public static Action<string> SaveCommand = data => Console.WriteLine($"[SAVE] {data}");
            public static Action<string> SendCommand = message => Console.WriteLine($"[SEND] {message}");
            
            public static void ExecuteCommand(Action<string> command, string parameter)
            {
                command(parameter);
            }
        }
        
        public class ObserverPattern
        {
            // Observadores como lambda
            public static Action<string> ConsoleObserver = message => Console.WriteLine($"Console: {message}");
            public static Action<string> FileObserver = message => Console.WriteLine($"File: {message}");
            public static Action<string> EmailObserver = message => Console.WriteLine($"Email: {message}");
            
            private readonly List<Action<string>> _observers = new List<Action<string>>();
            
            public void AddObserver(Action<string> observer)
            {
                _observers.Add(observer);
            }
            
            public void NotifyObservers(string message)
            {
                foreach (var observer in _observers)
                {
                    observer(message);
                }
            }
        }
    }
    
    // ===== LAMBDA CON LINQ =====
    namespace LambdaWithLinq
    {
        public class LinqLambdaExamples
        {
            // Lambda en Where
            public static IEnumerable<int> GetEvenNumbers(IEnumerable<int> numbers)
            {
                return numbers.Where(n => n % 2 == 0);
            }
            
            // Lambda en Select
            public static IEnumerable<string> GetSquares(IEnumerable<int> numbers)
            {
                return numbers.Select(n => (n * n).ToString());
            }
            
            // Lambda en OrderBy
            public static IEnumerable<string> SortByLength(IEnumerable<string> strings)
            {
                return strings.OrderBy(s => s.Length);
            }
            
            // Lambda en GroupBy
            public static IEnumerable<IGrouping<int, string>> GroupByLength(IEnumerable<string> strings)
            {
                return strings.GroupBy(s => s.Length);
            }
            
            // Lambda en Aggregate
            public static string ConcatenateWithSeparator(IEnumerable<string> strings, string separator)
            {
                return strings.Aggregate((current, next) => current + separator + next);
            }
            
            // Lambda en Any/All
            public static bool HasAnyLongString(IEnumerable<string> strings, int minLength)
            {
                return strings.Any(s => s.Length >= minLength);
            }
            
            public static bool AllStringsAreValid(IEnumerable<string> strings)
            {
                return strings.All(s => !string.IsNullOrWhiteSpace(s) && s.Length > 0);
            }
            
            // Lambda en First/FirstOrDefault
            public static string GetFirstLongString(IEnumerable<string> strings, int minLength)
            {
                return strings.FirstOrDefault(s => s.Length >= minLength);
            }
            
            // Lambda en TakeWhile/SkipWhile
            public static IEnumerable<string> TakeUntilLong(IEnumerable<string> strings, int maxLength)
            {
                return strings.TakeWhile(s => s.Length <= maxLength);
            }
        }
    }
    
    // ===== LAMBDA CON EXCEPCIONES =====
    namespace LambdaExceptions
    {
        public class ExceptionHandling
        {
            // Lambda con manejo de excepciones
            public static Func<T, TResult> SafeExecute<T, TResult>(
                Func<T, TResult> operation, 
                TResult defaultValue = default(TResult))
            {
                return input =>
                {
                    try
                    {
                        return operation(input);
                    }
                    catch
                    {
                        return defaultValue;
                    }
                };
            }
            
            // Lambda con retry
            public static Func<T, TResult> RetryExecute<T, TResult>(
                Func<T, TResult> operation, 
                int maxRetries = 3)
            {
                return input =>
                {
                    for (int i = 0; i < maxRetries; i++)
                    {
                        try
                        {
                            return operation(input);
                        }
                        catch when (i < maxRetries - 1)
                        {
                            Thread.Sleep(1000 * (i + 1));
                        }
                    }
                    
                    return operation(input); // Último intento
                };
            }
            
            // Lambda con logging de excepciones
            public static Func<T, TResult> LogExceptions<T, TResult>(
                Func<T, TResult> operation, 
                Action<Exception> logger)
            {
                return input =>
                {
                    try
                    {
                        return operation(input);
                    }
                    catch (Exception ex)
                    {
                        logger(ex);
                        throw;
                    }
                };
            }
        }
    }
    
    // ===== LAMBDA CON CACHE =====
    namespace LambdaCaching
    {
        public class CachedLambda
        {
            private static readonly Dictionary<string, object> _cache = new Dictionary<string, object>();
            
            // Lambda con cache simple
            public static Func<T, TResult> Cached<T, TResult>(
                Func<T, TResult> operation, 
                string cacheKey)
            {
                return input =>
                {
                    var key = $"{cacheKey}_{input}";
                    
                    if (_cache.TryGetValue(key, out var cachedResult))
                    {
                        return (TResult)cachedResult;
                    }
                    
                    var result = operation(input);
                    _cache[key] = result;
                    return result;
                };
            }
            
            // Lambda con cache con TTL
            public static Func<T, TResult> CachedWithTTL<T, TResult>(
                Func<T, TResult> operation, 
                string cacheKey, 
                TimeSpan ttl)
            {
                var cache = new Dictionary<string, (TResult Value, DateTime Expiry)>();
                
                return input =>
                {
                    var key = $"{cacheKey}_{input}";
                    
                    if (cache.TryGetValue(key, out var cached) && 
                        DateTime.UtcNow < cached.Expiry)
                    {
                        return cached.Value;
                    }
                    
                    var result = operation(input);
                    cache[key] = (result, DateTime.UtcNow.Add(ttl));
                    return result;
                };
            }
        }
    }
}

// Uso de Expresiones Lambda Avanzadas
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Expresiones Lambda Avanzadas - Clase 1 ===\n");
        
        // Ejemplos de delegados personalizados
        Console.WriteLine("1. Delegados Personalizados:");
        var result = LambdaDelegates.Add(5, 3);
        Console.WriteLine($"Add(5, 3) = {result}");
        
        var isValid = LambdaDelegates.IsEmail("test@example.com");
        Console.WriteLine($"IsEmail('test@example.com') = {isValid}");
        
        // Ejemplos de funciones de orden superior
        Console.WriteLine("\n2. Funciones de Orden Superior:");
        var doubled = FunctionalOperations.ApplyOperation(10, x => x * 2);
        Console.WriteLine($"ApplyOperation(10, x => x * 2) = {doubled}");
        
        var multiplier = FunctionalOperations.CreateMultiplier(5);
        var multiplied = multiplier(3);
        Console.WriteLine($"CreateMultiplier(5)(3) = {multiplied}");
        
        // Ejemplos de lambda anidadas
        Console.WriteLine("\n3. Lambda Anidadas:");
        var adder = LambdaNesting.CreateAdder(10);
        var sum = adder(5);
        Console.WriteLine($"CreateAdder(10)(5) = {sum}");
        
        var prefixer = LambdaNesting.CreatePrefixer("INFO");
        var prefixed = prefixer("Hello World");
        Console.WriteLine($"CreatePrefixer('INFO')('Hello World') = {prefixed}");
        
        // Ejemplos con LINQ
        Console.WriteLine("\n4. Lambda con LINQ:");
        var numbers = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var evenSquares = numbers
            .Where(n => n % 2 == 0)
            .Select(n => n * n);
        
        Console.WriteLine("Even squares: " + string.Join(", ", evenSquares));
        
        // Ejemplos de patrones
        Console.WriteLine("\n5. Patrones con Lambda:");
        var strategyResult = StrategyPattern.ExecuteStrategy(10, 5, StrategyPattern.AddStrategy);
        Console.WriteLine($"Strategy result: {strategyResult}");
        
        Console.WriteLine("\n✅ Expresiones Lambda Avanzadas funcionando correctamente!");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Delegados Personalizados
Crea delegados personalizados para diferentes tipos de operaciones y úsalos con expresiones lambda.

### Ejercicio 2: Funciones de Orden Superior
Implementa funciones que tomen otras funciones como parámetros y las usen de manera efectiva.

### Ejercicio 3: Lambda Anidadas
Crea expresiones lambda anidadas que retornen otras funciones con contexto específico.

## 🔍 Puntos Clave

1. **Delegados personalizados** para operaciones específicas
2. **Funciones de orden superior** que toman funciones como parámetros
3. **Lambda anidadas** para crear funciones con contexto
4. **Patrones de diseño** implementados con lambda
5. **Manejo de excepciones** y cache con lambda

## 📚 Recursos Adicionales

- [Microsoft Docs - Lambda Expressions](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-expressions)
- [Functional Programming with C#](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/)

---

**🎯 ¡Has completado la Clase 1! Ahora comprendes las Expresiones Lambda Avanzadas**

**📚 [Siguiente: Clase 2 - Operadores LINQ Básicos](clase_2_operadores_linq_basicos.md)**
