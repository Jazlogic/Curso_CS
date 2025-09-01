# 🚀 Clase 2: Generics Básicos

## 📋 Información de la Clase

- **Módulo**: Mid Level 3 - Manejo de Excepciones y Generics
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 1 (Manejo de Excepciones)

## 🎯 Objetivos de Aprendizaje

- Comprender los fundamentos de generics en C#
- Crear clases y métodos genéricos básicos
- Implementar contenedores genéricos personalizados
- Usar generics para código reutilizable y type-safe

---

## 📚 Navegación del Módulo 3

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_manejo_excepciones.md) | Manejo de Excepciones | ← Anterior |
| **Clase 2** | **Generics Básicos** | ← Estás aquí |
| [Clase 3](clase_3_generics_avanzados.md) | Generics Avanzados | Siguiente → |
| [Clase 4](clase_4_colecciones_genericas.md) | Colecciones Genéricas | |
| [Clase 5](clase_5_interfaces_genericas.md) | Interfaces Genéricas | |
| [Clase 6](clase_6_restricciones_generics.md) | Restricciones de Generics | |
| [Clase 7](clase_7_generics_reflection.md) | Generics y Reflection | |
| [Clase 8](clase_8_generics_performance.md) | Generics y Performance | |
| [Clase 9](clase_9_patrones_generics.md) | Patrones con Generics | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Biblioteca | |

**← [Volver al README del Módulo 3](../midLevel_3/README.md)**

---

## 📚 Contenido Teórico

### 1. Fundamentos de Generics

Los generics permiten crear clases, interfaces y métodos que trabajan con tipos de datos no especificados, proporcionando type safety y reutilización de código.

```csharp
// ===== GENERICS BÁSICOS - IMPLEMENTACIÓN COMPLETA =====
namespace BasicGenerics
{
    // ===== CLASES GENÉRICAS BÁSICAS =====
    namespace GenericClasses
    {
        public class Container<T>
        {
            private T _item;
            
            public T Item
            {
                get { return _item; }
                set { _item = value; }
            }
            
            public Container(T item)
            {
                _item = item;
            }
            
            public bool IsNull()
            {
                return _item == null;
            }
            
            public void Clear()
            {
                _item = default(T);
            }
            
            public override string ToString()
            {
                return _item?.ToString() ?? "null";
            }
        }
        
        public class Pair<TFirst, TSecond>
        {
            public TFirst First { get; set; }
            public TSecond Second { get; set; }
            
            public Pair(TFirst first, TSecond second)
            {
                First = first;
                Second = second;
            }
            
            public void Swap()
            {
                var temp = First;
                First = (TFirst)(object)Second;
                Second = (TSecond)(object)temp;
            }
            
            public override string ToString()
            {
                return $"({First}, {Second})";
            }
        }
        
        public class Triple<T1, T2, T3>
        {
            public T1 First { get; set; }
            public T2 Second { get; set; }
            public T3 Third { get; set; }
            
            public Triple(T1 first, T2 second, T3 third)
            {
                First = first;
                Second = second;
                Third = third;
            }
            
            public override string ToString()
            {
                return $"({First}, {Second}, {Third})";
            }
        }
        
        public class Result<T>
        {
            public bool IsSuccess { get; private set; }
            public T Value { get; private set; }
            public string ErrorMessage { get; private set; }
            
            private Result(bool isSuccess, T value, string errorMessage)
            {
                IsSuccess = isSuccess;
                Value = value;
                ErrorMessage = errorMessage;
            }
            
            public static Result<T> Success(T value)
            {
                return new Result<T>(true, value, null);
            }
            
            public static Result<T> Failure(string errorMessage)
            {
                return new Result<T>(false, default(T), errorMessage);
            }
            
            public T GetValueOrDefault(T defaultValue = default(T))
            {
                return IsSuccess ? Value : defaultValue;
            }
        }
    }
    
    // ===== MÉTODOS GENÉRICOS =====
    namespace GenericMethods
    {
        public class Utilities
        {
            public static void Swap<T>(ref T a, ref T b)
            {
                T temp = a;
                a = b;
                b = temp;
            }
            
            public static bool AreEqual<T>(T a, T b)
            {
                if (a == null && b == null) return true;
                if (a == null || b == null) return false;
                return a.Equals(b);
            }
            
            public static T Max<T>(T a, T b) where T : IComparable<T>
            {
                return a.CompareTo(b) > 0 ? a : b;
            }
            
            public static T Min<T>(T a, T b) where T : IComparable<T>
            {
                return a.CompareTo(b) < 0 ? a : b;
            }
            
            public static bool IsInRange<T>(T value, T min, T max) where T : IComparable<T>
            {
                return value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
            }
            
            public static List<T> CreateList<T>(params T[] items)
            {
                return new List<T>(items);
            }
            
            public static T[] CreateArray<T>(int size, T defaultValue = default(T))
            {
                var array = new T[size];
                for (int i = 0; i < size; i++)
                {
                    array[i] = defaultValue;
                }
                return array;
            }
            
            public static Dictionary<TKey, TValue> CreateDictionary<TKey, TValue>()
            {
                return new Dictionary<TKey, TValue>();
            }
        }
        
        public class ArrayUtilities
        {
            public static T[] Reverse<T>(T[] array)
            {
                var reversed = new T[array.Length];
                for (int i = 0; i < array.Length; i++)
                {
                    reversed[i] = array[array.Length - 1 - i];
                }
                return reversed;
            }
            
            public static T[] Shuffle<T>(T[] array)
            {
                var shuffled = (T[])array.Clone();
                var random = new Random();
                
                for (int i = shuffled.Length - 1; i > 0; i--)
                {
                    int j = random.Next(i + 1);
                    T temp = shuffled[i];
                    shuffled[i] = shuffled[j];
                    shuffled[j] = temp;
                }
                
                return shuffled;
            }
            
            public static T[] Slice<T>(T[] array, int start, int length)
            {
                if (start < 0 || start >= array.Length || length <= 0)
                {
                    return new T[0];
                }
                
                var actualLength = Math.Min(length, array.Length - start);
                var result = new T[actualLength];
                
                Array.Copy(array, start, result, 0, actualLength);
                return result;
            }
            
            public static bool Contains<T>(T[] array, T item)
            {
                return Array.IndexOf(array, item) >= 0;
            }
        }
    }
    
    // ===== CONTENEDORES GENÉRICOS PERSONALIZADOS =====
    namespace GenericContainers
    {
        public class GenericStack<T>
        {
            private readonly List<T> _items = new List<T>();
            
            public int Count => _items.Count;
            public bool IsEmpty => _items.Count == 0;
            
            public void Push(T item)
            {
                _items.Add(item);
            }
            
            public T Pop()
            {
                if (IsEmpty)
                {
                    throw new InvalidOperationException("Stack está vacío");
                }
                
                var item = _items[_items.Count - 1];
                _items.RemoveAt(_items.Count - 1);
                return item;
            }
            
            public T Peek()
            {
                if (IsEmpty)
                {
                    throw new InvalidOperationException("Stack está vacío");
                }
                
                return _items[_items.Count - 1];
            }
            
            public void Clear()
            {
                _items.Clear();
            }
            
            public bool Contains(T item)
            {
                return _items.Contains(item);
            }
            
            public T[] ToArray()
            {
                return _items.ToArray();
            }
        }
        
        public class GenericQueue<T>
        {
            private readonly List<T> _items = new List<T>();
            
            public int Count => _items.Count;
            public bool IsEmpty => _items.Count == 0;
            
            public void Enqueue(T item)
            {
                _items.Add(item);
            }
            
            public T Dequeue()
            {
                if (IsEmpty)
                {
                    throw new InvalidOperationException("Queue está vacío");
                }
                
                var item = _items[0];
                _items.RemoveAt(0);
                return item;
            }
            
            public T Peek()
            {
                if (IsEmpty)
                {
                    throw new InvalidOperationException("Queue está vacío");
                }
                
                return _items[0];
            }
            
            public void Clear()
            {
                _items.Clear();
            }
            
            public bool Contains(T item)
            {
                return _items.Contains(item);
            }
            
            public T[] ToArray()
            {
                return _items.ToArray();
            }
        }
        
        public class GenericLinkedList<T>
        {
            private class Node
            {
                public T Data { get; set; }
                public Node Next { get; set; }
                
                public Node(T data)
                {
                    Data = data;
                    Next = null;
                }
            }
            
            private Node _head;
            private int _count;
            
            public int Count => _count;
            public bool IsEmpty => _count == 0;
            
            public void AddFirst(T item)
            {
                var newNode = new Node(item);
                newNode.Next = _head;
                _head = newNode;
                _count++;
            }
            
            public void AddLast(T item)
            {
                var newNode = new Node(item);
                
                if (_head == null)
                {
                    _head = newNode;
                }
                else
                {
                    var current = _head;
                    while (current.Next != null)
                    {
                        current = current.Next;
                    }
                    current.Next = newNode;
                }
                
                _count++;
            }
            
            public T RemoveFirst()
            {
                if (IsEmpty)
                {
                    throw new InvalidOperationException("LinkedList está vacío");
                }
                
                var item = _head.Data;
                _head = _head.Next;
                _count--;
                return item;
            }
            
            public bool Remove(T item)
            {
                if (_head == null) return false;
                
                if (_head.Data.Equals(item))
                {
                    _head = _head.Next;
                    _count--;
                    return true;
                }
                
                var current = _head;
                while (current.Next != null)
                {
                    if (current.Next.Data.Equals(item))
                    {
                        current.Next = current.Next.Next;
                        _count--;
                        return true;
                    }
                    current = current.Next;
                }
                
                return false;
            }
            
            public bool Contains(T item)
            {
                var current = _head;
                while (current != null)
                {
                    if (current.Data.Equals(item))
                    {
                        return true;
                    }
                    current = current.Next;
                }
                return false;
            }
            
            public T[] ToArray()
            {
                var array = new T[_count];
                var current = _head;
                var index = 0;
                
                while (current != null)
                {
                    array[index++] = current.Data;
                    current = current.Next;
                }
                
                return array;
            }
        }
    }
    
    // ===== CACHE GENÉRICO =====
    namespace GenericCache
    {
        public class CacheItem<T>
        {
            public T Value { get; set; }
            public DateTime ExpiresAt { get; set; }
            
            public bool IsExpired => DateTime.UtcNow > ExpiresAt;
            
            public CacheItem(T value, TimeSpan ttl)
            {
                Value = value;
                ExpiresAt = DateTime.UtcNow.Add(ttl);
            }
        }
        
        public class GenericCache<TKey, TValue>
        {
            private readonly Dictionary<TKey, CacheItem<TValue>> _cache = new Dictionary<TKey, CacheItem<TValue>>();
            private readonly TimeSpan _defaultTtl;
            
            public GenericCache(TimeSpan defaultTtl = default)
            {
                _defaultTtl = defaultTtl == default ? TimeSpan.FromMinutes(30) : defaultTtl;
            }
            
            public void Set(TKey key, TValue value, TimeSpan? ttl = null)
            {
                var cacheItem = new CacheItem<TValue>(value, ttl ?? _defaultTtl);
                _cache[key] = cacheItem;
            }
            
            public bool TryGet(TKey key, out TValue value)
            {
                if (_cache.TryGetValue(key, out var cacheItem))
                {
                    if (cacheItem.IsExpired)
                    {
                        _cache.Remove(key);
                        value = default(TValue);
                        return false;
                    }
                    
                    value = cacheItem.Value;
                    return true;
                }
                
                value = default(TValue);
                return false;
            }
            
            public TValue Get(TKey key)
            {
                if (TryGet(key, out var value))
                {
                    return value;
                }
                
                throw new KeyNotFoundException($"Clave '{key}' no encontrada en el cache");
            }
            
            public bool Remove(TKey key)
            {
                return _cache.Remove(key);
            }
            
            public void Clear()
            {
                _cache.Clear();
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
            
            public int Count => _cache.Count;
            public bool IsEmpty => _cache.Count == 0;
        }
    }
    
    // ===== VALIDADORES GENÉRICOS =====
    namespace GenericValidators
    {
        public interface IValidator<T>
        {
            bool IsValid(T item);
            List<string> GetErrors(T item);
        }
        
        public class UserValidator : IValidator<User>
        {
            public bool IsValid(User user)
            {
                return GetErrors(user).Count == 0;
            }
            
            public List<string> GetErrors(User user)
            {
                var errors = new List<string>();
                
                if (string.IsNullOrWhiteSpace(user.Username))
                {
                    errors.Add("Username es requerido");
                }
                else if (user.Username.Length < 3)
                {
                    errors.Add("Username debe tener al menos 3 caracteres");
                }
                
                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    errors.Add("Email es requerido");
                }
                else if (!IsValidEmail(user.Email))
                {
                    errors.Add("Email no tiene formato válido");
                }
                
                if (user.Age < 0 || user.Age > 150)
                {
                    errors.Add("Edad debe estar entre 0 y 150");
                }
                
                return errors;
            }
            
            private bool IsValidEmail(string email)
            {
                try
                {
                    var addr = new System.Net.Mail.MailAddress(email);
                    return addr.Address == email;
                }
                catch
                {
                    return false;
                }
            }
        }
        
        public class ProductValidator : IValidator<Product>
        {
            public bool IsValid(Product product)
            {
                return GetErrors(product).Count == 0;
            }
            
            public List<string> GetErrors(Product product)
            {
                var errors = new List<string>();
                
                if (string.IsNullOrWhiteSpace(product.Name))
                {
                    errors.Add("Nombre del producto es requerido");
                }
                
                if (product.Price < 0)
                {
                    errors.Add("Precio no puede ser negativo");
                }
                
                if (product.StockQuantity < 0)
                {
                    errors.Add("Stock no puede ser negativo");
                }
                
                if (string.IsNullOrWhiteSpace(product.Category))
                {
                    errors.Add("Categoría es requerida");
                }
                
                return errors;
            }
        }
        
        public class GenericValidator<T>
        {
            private readonly List<IValidator<T>> _validators = new List<IValidator<T>>();
            
            public void AddValidator(IValidator<T> validator)
            {
                _validators.Add(validator);
            }
            
            public bool IsValid(T item)
            {
                return _validators.All(v => v.IsValid(item));
            }
            
            public List<string> GetAllErrors(T item)
            {
                var allErrors = new List<string>();
                
                foreach (var validator in _validators)
                {
                    allErrors.AddRange(validator.GetErrors(item));
                }
                
                return allErrors;
            }
        }
    }
    
    // ===== MODELOS DE DATOS =====
    namespace Models
    {
        public class User
        {
            public string Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public int Age { get; set; }
            public DateTime CreatedAt { get; set; }
        }
        
        public class Product
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int StockQuantity { get; set; }
            public string Category { get; set; }
        }
    }
}

// Uso de Generics Básicos
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Generics Básicos - Clase 2 ===\n");
        
        Console.WriteLine("Los componentes implementados incluyen:");
        Console.WriteLine("1. Clases genéricas básicas (Container, Pair, Triple, Result)");
        Console.WriteLine("2. Métodos genéricos (Swap, Max, Min, IsInRange)");
        Console.WriteLine("3. Contenedores genéricos personalizados (Stack, Queue, LinkedList)");
        Console.WriteLine("4. Cache genérico con TTL");
        Console.WriteLine("5. Validadores genéricos");
        Console.WriteLine("6. Utilidades de arrays genéricos");
        
        Console.WriteLine("\nEjemplos de uso:");
        
        // Container genérico
        var stringContainer = new Container<string>("Hola Mundo");
        var intContainer = new Container<int>(42);
        
        Console.WriteLine($"String container: {stringContainer}");
        Console.WriteLine($"Int container: {intContainer}");
        
        // Pair genérico
        var pair = new Pair<string, int>("Edad", 25);
        Console.WriteLine($"Pair: {pair}");
        
        // Métodos genéricos
        int a = 5, b = 10;
        Utilities.Swap(ref a, ref b);
        Console.WriteLine($"Después del swap: a={a}, b={b}");
        
        Console.WriteLine($"Máximo entre 15 y 8: {Utilities.Max(15, 8)}");
        
        // Stack genérico
        var stack = new GenericStack<int>();
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);
        
        Console.WriteLine($"Stack count: {stack.Count}");
        Console.WriteLine($"Top of stack: {stack.Peek()}");
        
        // Cache genérico
        var cache = new GenericCache<string, string>(TimeSpan.FromMinutes(5));
        cache.Set("key1", "value1");
        
        if (cache.TryGet("key1", out var value))
        {
            Console.WriteLine($"Cache hit: {value}");
        }
        
        Console.WriteLine("\nBeneficios de esta implementación:");
        Console.WriteLine("- Código reutilizable y type-safe");
        Console.WriteLine("- Contenedores personalizados flexibles");
        Console.WriteLine("- Validación genérica para diferentes tipos");
        Console.WriteLine("- Cache con expiración automática");
        Console.WriteLine("- Métodos de utilidad genéricos");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Stack Genérico Personalizado
Implementa un stack genérico con métodos Push, Pop, Peek y validaciones.

### Ejercicio 2: Cache Genérico con TTL
Crea un sistema de cache genérico que expire elementos después de un tiempo determinado.

### Ejercicio 3: Validador Genérico
Implementa un validador genérico que pueda validar diferentes tipos de objetos.

## 🔍 Puntos Clave

1. **Clases genéricas** para contenedores reutilizables
2. **Métodos genéricos** para operaciones comunes
3. **Contenedores personalizados** (Stack, Queue, LinkedList)
4. **Cache genérico** con expiración automática
5. **Validadores genéricos** para diferentes tipos de datos

## 📚 Recursos Adicionales

- [Microsoft Docs - Generics](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/types/generics)
- [C# Generics Tutorial](https://www.tutorialsteacher.com/csharp/csharp-generics)

---

**🎯 ¡Has completado la Clase 2! Ahora comprendes los Generics Básicos**

**📚 [Siguiente: Clase 3 - Generics Avanzados](clase_3_generics_avanzados.md)**
