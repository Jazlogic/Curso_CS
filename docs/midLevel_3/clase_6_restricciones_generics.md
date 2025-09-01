# 🚀 Clase 6: Restricciones de Generics

## 📋 Información de la Clase

- **Módulo**: Mid Level 3 - Manejo de Excepciones y Generics
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 5 (Interfaces Genéricas)

## 🎯 Objetivos de Aprendizaje

- Comprender las restricciones de tipos en generics
- Implementar restricciones de clase, interfaz y constructor
- Usar restricciones múltiples y combinadas
- Aplicar restricciones para mejorar type safety

---

## 📚 Navegación del Módulo 3

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_manejo_excepciones.md) | Manejo de Excepciones | |
| [Clase 2](clase_2_generics_basicos.md) | Generics Básicos | |
| [Clase 3](clase_3_generics_avanzados.md) | Generics Avanzados | |
| [Clase 4](clase_4_colecciones_genericas.md) | Colecciones Genéricas | |
| [Clase 5](clase_5_interfaces_genericas.md) | Interfaces Genéricas | ← Anterior |
| **Clase 6** | **Restricciones de Generics** | ← Estás aquí |
| [Clase 7](clase_7_generics_reflection.md) | Generics y Reflection | Siguiente → |
| [Clase 8](clase_8_generics_performance.md) | Generics y Performance | |
| [Clase 9](clase_9_patrones_generics.md) | Patrones con Generics | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Biblioteca | |

**← [Volver al README del Módulo 3](../midLevel_3/README.md)**

---

## 📚 Contenido Teórico

### 1. Restricciones de Generics

Las restricciones permiten limitar los tipos que pueden ser usados como parámetros de tipo genérico, mejorando la type safety y permitiendo operaciones específicas.

```csharp
// ===== RESTRICCIONES DE GENERICS - IMPLEMENTACIÓN COMPLETA =====
namespace GenericConstraints
{
    // ===== RESTRICCIONES BÁSICAS =====
    namespace BasicConstraints
    {
        // Restricción de clase - T debe ser una clase
        public class ClassConstraint<T> where T : class
        {
            private T _value;
            
            public ClassConstraint(T value)
            {
                _value = value ?? throw new ArgumentNullException(nameof(value));
            }
            
            public bool IsNull() => _value == null;
            public T GetValue() => _value;
            public void SetValue(T value) => _value = value ?? throw new ArgumentNullException(nameof(value));
        }
        
        // Restricción de struct - T debe ser un tipo de valor
        public class StructConstraint<T> where T : struct
        {
            private T _value;
            
            public StructConstraint(T value)
            {
                _value = value;
            }
            
            public T GetValue() => _value;
            public void SetValue(T value) => _value = value;
            public bool IsDefault() => _value.Equals(default(T));
        }
        
        // Restricción de new() - T debe tener constructor sin parámetros
        public class NewConstraint<T> where T : new()
        {
            public T CreateInstance() => new T();
            public T[] CreateArray(int count) => Enumerable.Range(0, count).Select(_ => new T()).ToArray();
            public List<T> CreateList(int count) => Enumerable.Range(0, count).Select(_ => new T()).ToList();
        }
        
        // Restricción de interfaz - T debe implementar una interfaz específica
        public class InterfaceConstraint<T> where T : IComparable<T>
        {
            public T GetMax(T a, T b) => a.CompareTo(b) > 0 ? a : b;
            public T GetMin(T a, T b) => a.CompareTo(b) < 0 ? a : b;
            public bool IsGreaterThan(T a, T b) => a.CompareTo(b) > 0;
            public bool IsLessThan(T a, T b) => a.CompareTo(b) < 0;
        }
    }
    
    // ===== RESTRICCIONES MÚLTIPLES =====
    namespace MultipleConstraints
    {
        // Múltiples restricciones de interfaz
        public class MultipleInterfaceConstraint<T> 
            where T : class, IComparable<T>, IEquatable<T>
        {
            public T GetMax(IEnumerable<T> items) => items.Max();
            public T GetMin(IEnumerable<T> items) => items.Min();
            public bool AreEqual(T a, T b) => a.Equals(b);
            public int Compare(T a, T b) => a.CompareTo(b);
        }
        
        // Restricción de clase base e interfaz
        public class BaseClassAndInterfaceConstraint<T> 
            where T : BaseEntity, IAuditable
        {
            public void AuditOperation(T entity, string operation)
            {
                entity.LastModified = DateTime.UtcNow;
                entity.ModifiedBy = "System";
                entity.AuditTrail.Add(new AuditEntry(operation, DateTime.UtcNow));
            }
            
            public bool IsRecentlyModified(T entity, TimeSpan threshold)
            {
                return entity.LastModified.HasValue && 
                       DateTime.UtcNow - entity.LastModified.Value < threshold;
            }
        }
        
        // Restricción de constructor y interfaz
        public class ConstructorAndInterfaceConstraint<T> 
            where T : class, ICloneable, new()
        {
            public T CreateAndClone()
            {
                var instance = new T();
                return (T)instance.Clone();
            }
            
            public T[] CreateAndCloneArray(int count)
            {
                var instances = new T[count];
                for (int i = 0; i < count; i++)
                {
                    instances[i] = new T();
                }
                return instances;
            }
        }
        
        // Restricción de tipo de valor e interfaz
        public class ValueTypeAndInterfaceConstraint<T> 
            where T : struct, IComparable<T>, IEquatable<T>
        {
            public T GetMax(T a, T b) => a.CompareTo(b) > 0 ? a : b;
            public T GetMin(T a, T b) => a.CompareTo(b) < 0 ? a : b;
            public bool IsInRange(T value, T min, T max) => value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
            public T Clamp(T value, T min, T max)
            {
                if (value.CompareTo(min) < 0) return min;
                if (value.CompareTo(max) > 0) return max;
                return value;
            }
        }
    }
    
    // ===== RESTRICCIONES AVANZADAS =====
    namespace AdvancedConstraints
    {
        // Restricción de tipo de enumeración
        public class EnumConstraint<T> where T : struct, Enum
        {
            public T[] GetAllValues() => Enum.GetValues<T>();
            public string GetName(T value) => Enum.GetName(value);
            public bool IsDefined(T value) => Enum.IsDefined(value);
            public T Parse(string name) => Enum.Parse<T>(name);
            public bool TryParse(string name, out T value) => Enum.TryParse(name, out value);
        }
        
        // Restricción de tipo de delegado
        public class DelegateConstraint<T> where T : Delegate
        {
            public T Combine(T a, T b) => (T)Delegate.Combine(a, b);
            public T Remove(T source, T value) => (T)Delegate.Remove(source, value);
            public T RemoveAll(T source, T value) => (T)Delegate.RemoveAll(source, value);
            public bool IsMulticast(T del) => del.GetInvocationList().Length > 1;
        }
        
        // Restricción de tipo de excepción
        public class ExceptionConstraint<T> where T : Exception
        {
            public T CreateWithMessage(string message) => (T)Activator.CreateInstance(typeof(T), message);
            public T CreateWithInnerException(string message, Exception innerException) 
                => (T)Activator.CreateInstance(typeof(T), message, innerException);
            public void LogException(T exception)
            {
                Console.WriteLine($"Exception Type: {typeof(T).Name}");
                Console.WriteLine($"Message: {exception.Message}");
                Console.WriteLine($"StackTrace: {exception.StackTrace}");
            }
        }
        
        // Restricción de tipo de atributo
        public class AttributeConstraint<T> where T : Attribute
        {
            public T GetAttribute(Type type) => type.GetCustomAttribute<T>();
            public T[] GetAttributes(Type type) => type.GetCustomAttributes<T>().ToArray();
            public bool HasAttribute(Type type) => type.GetCustomAttribute<T>() != null;
            public IEnumerable<Type> GetTypesWithAttribute(Assembly assembly) 
                => assembly.GetTypes().Where(t => t.GetCustomAttribute<T>() != null);
        }
    }
    
    // ===== RESTRICCIONES PARA COLECCIONES =====
    namespace CollectionConstraints
    {
        // Restricción para colecciones genéricas
        public class CollectionConstraint<T, TCollection> 
            where T : class, IEquatable<T>
            where TCollection : class, ICollection<T>, new()
        {
            private TCollection _collection;
            
            public CollectionConstraint()
            {
                _collection = new TCollection();
            }
            
            public void Add(T item) => _collection.Add(item);
            public void AddRange(IEnumerable<T> items)
            {
                foreach (var item in items)
                {
                    _collection.Add(item);
                }
            }
            
            public bool Contains(T item) => _collection.Contains(item);
            public void Remove(T item) => _collection.Remove(item);
            public void Clear() => _collection.Clear();
            public int Count => _collection.Count;
            
            public T[] ToArray() => _collection.ToArray();
            public List<T> ToList() => _collection.ToList();
        }
        
        // Restricción para diccionarios genéricos
        public class DictionaryConstraint<TKey, TValue, TDictionary> 
            where TKey : IEquatable<TKey>
            where TDictionary : class, IDictionary<TKey, TValue>, new()
        {
            private TDictionary _dictionary;
            
            public DictionaryConstraint()
            {
                _dictionary = new TDictionary();
            }
            
            public void Add(TKey key, TValue value) => _dictionary.Add(key, value);
            public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);
            public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);
            public void Remove(TKey key) => _dictionary.Remove(key);
            public void Clear() => _dictionary.Clear();
            public int Count => _dictionary.Count;
            
            public ICollection<TKey> Keys => _dictionary.Keys;
            public ICollection<TValue> Values => _dictionary.Values;
        }
        
        // Restricción para listas genéricas
        public class ListConstraint<T, TList> 
            where T : class, IComparable<T>
            where TList : class, IList<T>, new()
        {
            private TList _list;
            
            public ListConstraint()
            {
                _list = new TList();
            }
            
            public void Add(T item) => _list.Add(item);
            public void Insert(int index, T item) => _list.Insert(index, item);
            public void RemoveAt(int index) => _list.RemoveAt(index);
            public T this[int index]
            {
                get => _list[index];
                set => _list[index] = value;
            }
            
            public void Sort() => Sort(Comparer<T>.Default);
            public void Sort(IComparer<T> comparer)
            {
                var array = _list.ToArray();
                Array.Sort(array, comparer);
                _list.Clear();
                foreach (var item in array)
                {
                    _list.Add(item);
                }
            }
            
            public int Count => _list.Count;
            public bool IsReadOnly => _list.IsReadOnly;
        }
    }
    
    // ===== RESTRICCIONES PARA VALIDACIÓN =====
    namespace ValidationConstraints
    {
        // Restricción para objetos validables
        public class ValidatableConstraint<T> 
            where T : class, IValidatable
        {
            public ValidationResult Validate(T item)
            {
                var result = new ValidationResult();
                
                if (item == null)
                {
                    result.AddError("Item", "Item cannot be null");
                    return result;
                }
                
                var validationErrors = item.Validate();
                foreach (var error in validationErrors)
                {
                    result.AddError(error.PropertyName, error.Message);
                }
                
                return result;
            }
            
            public bool IsValid(T item) => Validate(item).IsValid;
            public IEnumerable<string> GetErrors(T item) => Validate(item).Errors.Select(e => e.Message);
        }
        
        // Restricción para objetos con ID
        public class IdentifiableConstraint<T, TKey> 
            where T : class, IIdentifiable<TKey>
            where TKey : IEquatable<TKey>
        {
            public bool HasId(T item) => item != null && !item.Id.Equals(default(TKey));
            public bool IsSameId(T a, T b) => a != null && b != null && a.Id.Equals(b.Id);
            public TKey GetId(T item) => item?.Id ?? default(TKey);
        }
        
        // Restricción para objetos auditables
        public class AuditableConstraint<T, TKey> 
            where T : class, IAuditable<TKey>
            where TKey : IEquatable<TKey>
        {
            public void MarkAsModified(T item, TKey userId)
            {
                if (item != null)
                {
                    item.LastModified = DateTime.UtcNow;
                    item.ModifiedBy = userId;
                }
            }
            
            public void MarkAsDeleted(T item, TKey userId)
            {
                if (item != null)
                {
                    item.IsDeleted = true;
                    item.DeletedAt = DateTime.UtcNow;
                    item.DeletedBy = userId;
                }
            }
            
            public bool IsRecentlyModified(T item, TimeSpan threshold)
            {
                return item?.LastModified.HasValue == true && 
                       DateTime.UtcNow - item.LastModified.Value < threshold;
            }
        }
    }
    
    // ===== RESTRICCIONES PARA SERIALIZACIÓN =====
    namespace SerializationConstraints
    {
        // Restricción para objetos serializables
        public class SerializableConstraint<T> 
            where T : class, ISerializable
        {
            public string Serialize(T obj) => obj.Serialize();
            public T Deserialize(string data) => obj.Deserialize(data);
            public byte[] SerializeToBytes(T obj) => obj.SerializeToBytes();
            public T DeserializeFromBytes(byte[] data) => obj.DeserializeFromBytes(data);
        }
        
        // Restricción para objetos JSON serializables
        public class JsonSerializableConstraint<T> 
            where T : class, IJsonSerializable
        {
            public string ToJson(T obj) => obj.ToJson();
            public T FromJson(string json) => obj.FromJson(json);
            public string ToPrettyJson(T obj) => obj.ToPrettyJson();
        }
        
        // Restricción para objetos XML serializables
        public class XmlSerializableConstraint<T> 
            where T : class, IXmlSerializable
        {
            public string ToXml(T obj) => obj.ToXml();
            public T FromXml(string xml) => obj.FromXml(xml);
            public string ToFormattedXml(T obj) => obj.ToFormattedXml();
        }
    }
    
    // ===== RESTRICCIONES PARA COMPARACIÓN =====
    namespace ComparisonConstraints
    {
        // Restricción para objetos comparables
        public class ComparableConstraint<T> 
            where T : class, IComparable<T>, IEquatable<T>
        {
            public T GetMax(T a, T b) => a.CompareTo(b) > 0 ? a : b;
            public T GetMin(T a, T b) => a.CompareTo(b) < 0 ? a : b;
            public bool IsInRange(T value, T min, T max) => value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;
            public T Clamp(T value, T min, T max)
            {
                if (value.CompareTo(min) < 0) return min;
                if (value.CompareTo(max) > 0) return max;
                return value;
            }
            
            public T[] Sort(IEnumerable<T> items) => items.OrderBy(x => x).ToArray();
            public T[] SortDescending(IEnumerable<T> items) => items.OrderByDescending(x => x).ToArray();
        }
        
        // Restricción para objetos con ordenamiento personalizado
        public class SortableConstraint<T, TSortKey> 
            where T : class
            where TSortKey : IComparable<TSortKey>
        {
            public T[] SortBy(IEnumerable<T> items, Func<T, TSortKey> keySelector) 
                => items.OrderBy(keySelector).ToArray();
            
            public T[] SortByDescending(IEnumerable<T> items, Func<T, TSortKey> keySelector) 
                => items.OrderByDescending(keySelector).ToArray();
            
            public T GetMinBy(IEnumerable<T> items, Func<T, TSortKey> keySelector) 
                => items.MinBy(keySelector);
            
            public T GetMaxBy(IEnumerable<T> items, Func<T, TSortKey> keySelector) 
                => items.MaxBy(keySelector);
        }
    }
    
    // ===== RESTRICCIONES PARA OPERACIONES MATEMÁTICAS =====
    namespace MathematicalConstraints
    {
        // Restricción para tipos numéricos
        public class NumericConstraint<T> 
            where T : struct, IComparable<T>, IEquatable<T>
        {
            public T Add(T a, T b) => (T)((dynamic)a + (dynamic)b);
            public T Subtract(T a, T b) => (T)((dynamic)a - (dynamic)b);
            public T Multiply(T a, T b) => (T)((dynamic)a * (dynamic)b);
            public T Divide(T a, T b) => (T)((dynamic)a / (dynamic)b);
            
            public T Abs(T value) => (T)((dynamic)Math.Abs((dynamic)value));
            public T Max(T a, T b) => a.CompareTo(b) > 0 ? a : b;
            public T Min(T a, T b) => a.CompareTo(b) < 0 ? a : b;
            
            public bool IsPositive(T value) => value.CompareTo(default(T)) > 0;
            public bool IsNegative(T value) => value.CompareTo(default(T)) < 0;
            public bool IsZero(T value) => value.Equals(default(T));
        }
        
        // Restricción para tipos de punto flotante
        public class FloatingPointConstraint<T> 
            where T : struct, IComparable<T>, IEquatable<T>
        {
            public T Round(T value) => (T)((dynamic)Math.Round((dynamic)value));
            public T Ceiling(T value) => (T)((dynamic)Math.Ceiling((dynamic)value));
            public T Floor(T value) => (T)((dynamic)Math.Floor((dynamic)value));
            
            public T Pow(T value, double power) => (T)((dynamic)Math.Pow((dynamic)value, power));
            public T Sqrt(T value) => (T)((dynamic)Math.Sqrt((dynamic)value));
            public T Log(T value) => (T)((dynamic)Math.Log((dynamic)value));
            
            public bool IsNaN(T value) => double.IsNaN((dynamic)value);
            public bool IsInfinity(T value) => double.IsInfinity((dynamic)value);
            public bool IsFinite(T value) => !double.IsNaN((dynamic)value) && !double.IsInfinity((dynamic)value);
        }
    }
    
    // ===== INTERFACES Y CLASES BASE =====
    namespace Interfaces
    {
        public interface IValidatable
        {
            IEnumerable<ValidationError> Validate();
        }
        
        public interface IIdentifiable<TKey>
        {
            TKey Id { get; set; }
        }
        
        public interface IAuditable<TKey>
        {
            DateTime? LastModified { get; set; }
            TKey ModifiedBy { get; set; }
            bool IsDeleted { get; set; }
            DateTime? DeletedAt { get; set; }
            TKey DeletedBy { get; set; }
        }
        
        public interface ISerializable
        {
            string Serialize();
            T Deserialize<T>(string data);
            byte[] SerializeToBytes();
            T DeserializeFromBytes<T>(byte[] data);
        }
        
        public interface IJsonSerializable
        {
            string ToJson();
            T FromJson<T>(string json);
            string ToPrettyJson();
        }
        
        public interface IXmlSerializable
        {
            string ToXml();
            T FromXml<T>(string xml);
            string ToFormattedXml();
        }
    }
    
    // ===== CLASES BASE =====
    namespace BaseClasses
    {
        public abstract class BaseEntity
        {
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public string CreatedBy { get; set; } = "System";
        }
        
        public class ValidationError
        {
            public string PropertyName { get; set; }
            public string Message { get; set; }
        }
        
        public class ValidationResult
        {
            private List<ValidationError> _errors = new List<ValidationError>();
            
            public bool IsValid => !_errors.Any();
            public IEnumerable<ValidationError> Errors => _errors;
            
            public void AddError(string propertyName, string message)
            {
                _errors.Add(new ValidationError { PropertyName = propertyName, Message = message });
            }
        }
        
        public class AuditEntry
        {
            public string Operation { get; set; }
            public DateTime Timestamp { get; set; }
            
            public AuditEntry(string operation, DateTime timestamp)
            {
                Operation = operation;
                Timestamp = timestamp;
            }
        }
    }
}

// Uso de Restricciones de Generics
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Restricciones de Generics - Clase 6 ===\n");
        
        Console.WriteLine("Los tipos de restricciones implementados incluyen:");
        Console.WriteLine("1. Restricciones básicas (class, struct, new(), interface)");
        Console.WriteLine("2. Restricciones múltiples y combinadas");
        Console.WriteLine("3. Restricciones avanzadas (Enum, Delegate, Exception, Attribute)");
        Console.WriteLine("4. Restricciones para colecciones genéricas");
        Console.WriteLine("5. Restricciones para validación y auditoría");
        Console.WriteLine("6. Restricciones para serialización");
        Console.WriteLine("7. Restricciones para comparación y ordenamiento");
        Console.WriteLine("8. Restricciones para operaciones matemáticas");
        
        Console.WriteLine("\nBeneficios de las restricciones:");
        Console.WriteLine("- Type safety mejorado en tiempo de compilación");
        Console.WriteLine("- Acceso a métodos y propiedades específicos del tipo");
        Console.WriteLine("- Prevención de errores en tiempo de ejecución");
        Console.WriteLine("- Mejor IntelliSense y autocompletado");
        Console.WriteLine("- Código más robusto y mantenible");
        
        Console.WriteLine("\nCasos de uso principales:");
        Console.WriteLine("- Repositorios genéricos con restricciones de entidad");
        Console.WriteLine("- Validadores genéricos para objetos validables");
        Console.WriteLine("- Colecciones genéricas con restricciones de tipo");
        Console.WriteLine("- Operaciones matemáticas genéricas");
        Console.WriteLine("- Serialización genérica con restricciones de formato");
        Console.WriteLine("- Comparación y ordenamiento genérico");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Repository con Restricciones
Implementa un repositorio genérico con restricciones para entidades auditables.

### Ejercicio 2: Validator con Restricciones
Crea un validador genérico que solo acepte objetos que implementen IValidatable.

### Ejercicio 3: Colección con Restricciones
Implementa una colección genérica que solo acepte tipos comparables.

## 🔍 Puntos Clave

1. **Restricciones básicas** para controlar tipos genéricos
2. **Restricciones múltiples** para tipos más específicos
3. **Restricciones avanzadas** para casos especiales
4. **Restricciones para colecciones** y operaciones específicas
5. **Restricciones para validación** y auditoría

## 📚 Recursos Adicionales

- [Microsoft Docs - Generic Constraints](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/constraints-on-type-parameters)
- [Advanced Generic Constraints](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/constraints-on-type-parameters)

---

**🎯 ¡Has completado la Clase 6! Ahora comprendes las Restricciones de Generics**

**📚 [Siguiente: Clase 7 - Generics y Reflection](clase_7_generics_reflection.md)**
