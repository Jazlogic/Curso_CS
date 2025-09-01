# 🚀 Clase 7: Generics y Reflection

## 📋 Información de la Clase

- **Módulo**: Mid Level 3 - Manejo de Excepciones y Generics
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 6 (Restricciones de Generics)

## 🎯 Objetivos de Aprendizaje

- Comprender la relación entre generics y reflection
- Implementar reflection genérico para inspección de tipos
- Crear instancias genéricas dinámicamente
- Usar reflection para manipulación genérica avanzada

---

## 📚 Navegación del Módulo 3

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_manejo_excepciones.md) | Manejo de Excepciones | |
| [Clase 2](clase_2_generics_basicos.md) | Generics Básicos | |
| [Clase 3](clase_3_generics_avanzados.md) | Generics Avanzados | |
| [Clase 4](clase_4_colecciones_genericas.md) | Colecciones Genéricas | |
| [Clase 5](clase_5_interfaces_genericas.md) | Interfaces Genéricas | |
| [Clase 6](clase_6_restricciones_generics.md) | Restricciones de Generics | ← Anterior |
| **Clase 7** | **Generics y Reflection** | ← Estás aquí |
| [Clase 8](clase_8_generics_performance.md) | Generics y Performance | Siguiente → |
| [Clase 9](clase_9_patrones_generics.md) | Patrones con Generics | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final: Sistema de Biblioteca | |

**← [Volver al README del Módulo 3](../midLevel_3/README.md)**

---

## 📚 Contenido Teórico

### 1. Generics y Reflection

La combinación de generics y reflection permite crear código dinámico y flexible que puede trabajar con tipos genéricos en tiempo de ejecución.

```csharp
// ===== GENERICS Y REFLECTION - IMPLEMENTACIÓN COMPLETA =====
namespace GenericsAndReflection
{
    // ===== REFLECTION BÁSICO CON GENERICS =====
    namespace BasicReflection
    {
        public class GenericTypeInspector
        {
            // Obtiene información sobre tipos genéricos
            public static TypeInfo GetGenericTypeInfo<T>()
            {
                var type = typeof(T);
                return new TypeInfo
                {
                    Name = type.Name,
                    IsGenericType = type.IsGenericType,
                    IsGenericTypeDefinition = type.IsGenericTypeDefinition,
                    GenericTypeArguments = type.GenericTypeArguments,
                    GenericTypeParameters = type.GetGenericTypeParameters(),
                    BaseType = type.BaseType,
                    Interfaces = type.GetInterfaces()
                };
            }
            
            // Obtiene información sobre tipos genéricos construidos
            public static TypeInfo GetConstructedGenericTypeInfo(Type constructedType)
            {
                if (!constructedType.IsGenericType)
                    throw new ArgumentException("Type must be a constructed generic type");
                
                return new TypeInfo
                {
                    Name = constructedType.Name,
                    IsGenericType = constructedType.IsGenericType,
                    IsGenericTypeDefinition = false,
                    GenericTypeArguments = constructedType.GenericTypeArguments,
                    GenericTypeParameters = constructedType.GetGenericTypeParameters(),
                    BaseType = constructedType.BaseType,
                    Interfaces = constructedType.GetInterfaces()
                };
            }
            
            // Verifica si un tipo es genérico
            public static bool IsGenericType<T>() => typeof(T).IsGenericType;
            
            // Verifica si un tipo es una definición genérica
            public static bool IsGenericTypeDefinition<T>() => typeof(T).IsGenericTypeDefinition;
            
            // Obtiene los argumentos de tipo genérico
            public static Type[] GetGenericArguments<T>() => typeof(T).GenericTypeArguments;
            
            // Obtiene los parámetros de tipo genérico
            public static Type[] GetGenericParameters<T>() => typeof(T).GetGenericTypeParameters();
        }
        
        public class TypeInfo
        {
            public string Name { get; set; }
            public bool IsGenericType { get; set; }
            public bool IsGenericTypeDefinition { get; set; }
            public Type[] GenericTypeArguments { get; set; }
            public Type[] GenericTypeParameters { get; set; }
            public Type BaseType { get; set; }
            public Type[] Interfaces { get; set; }
        }
    }
    
    // ===== CREACIÓN DINÁMICA DE TIPOS GENÉRICOS =====
    namespace DynamicGenericCreation
    {
        public class GenericTypeFactory
        {
            // Crea un tipo genérico construido
            public static Type CreateGenericType(Type genericTypeDefinition, params Type[] typeArguments)
            {
                if (!genericTypeDefinition.IsGenericTypeDefinition)
                    throw new ArgumentException("Type must be a generic type definition");
                
                if (genericTypeDefinition.GetGenericArguments().Length != typeArguments.Length)
                    throw new ArgumentException("Number of type arguments must match generic parameters");
                
                return genericTypeDefinition.MakeGenericType(typeArguments);
            }
            
            // Crea una instancia de un tipo genérico
            public static object CreateGenericInstance(Type genericTypeDefinition, params Type[] typeArguments)
            {
                var constructedType = CreateGenericType(genericTypeDefinition, typeArguments);
                return Activator.CreateInstance(constructedType);
            }
            
            // Crea una instancia de un tipo genérico con parámetros de constructor
            public static object CreateGenericInstance(Type genericTypeDefinition, Type[] typeArguments, object[] constructorArgs)
            {
                var constructedType = CreateGenericType(genericTypeDefinition, typeArguments);
                return Activator.CreateInstance(constructedType, constructorArgs);
            }
            
            // Crea un tipo genérico anidado
            public static Type CreateNestedGenericType(Type outerGenericType, Type innerGenericType, params Type[] typeArguments)
            {
                var constructedOuter = CreateGenericType(outerGenericType, typeArguments);
                var constructedInner = CreateGenericType(innerGenericType, typeArguments);
                
                // Buscar el tipo anidado
                var nestedType = constructedOuter.GetNestedType(innerGenericType.Name);
                if (nestedType != null)
                    return nestedType.MakeGenericType(typeArguments);
                
                throw new InvalidOperationException("Nested generic type not found");
            }
        }
        
        public class GenericActivator
        {
            // Crea instancias genéricas usando reflection
            public static T CreateInstance<T>(params object[] args) where T : class
            {
                var type = typeof(T);
                if (type.IsGenericType)
                {
                    var constructedType = type.IsGenericTypeDefinition ? 
                        type.MakeGenericType(type.GetGenericArguments()) : type;
                    return (T)Activator.CreateInstance(constructedType, args);
                }
                
                return (T)Activator.CreateInstance(type, args);
            }
            
            // Crea instancias genéricas con tipos específicos
            public static object CreateGenericInstance(Type genericTypeDefinition, Type[] typeArguments, object[] constructorArgs = null)
            {
                var constructedType = genericTypeDefinition.MakeGenericType(typeArguments);
                return constructorArgs != null ? 
                    Activator.CreateInstance(constructedType, constructorArgs) : 
                    Activator.CreateInstance(constructedType);
            }
            
            // Crea instancias genéricas usando el patrón factory
            public static T CreateUsingFactory<T>(string factoryMethodName, params object[] args) where T : class
            {
                var type = typeof(T);
                var factoryType = type.GetGenericTypeDefinition();
                var factoryMethod = factoryType.GetMethod(factoryMethodName);
                
                if (factoryMethod == null)
                    throw new InvalidOperationException($"Factory method '{factoryMethodName}' not found");
                
                var result = factoryMethod.Invoke(null, args);
                return (T)result;
            }
        }
    }
    
    // ===== REFLECTION AVANZADO CON GENERICS =====
    namespace AdvancedReflection
    {
        public class GenericMethodInspector
        {
            // Obtiene métodos genéricos de un tipo
            public static MethodInfo[] GetGenericMethods(Type type)
            {
                return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .Where(m => m.IsGenericMethod)
                    .ToArray();
            }
            
            // Obtiene métodos genéricos con un nombre específico
            public static MethodInfo[] GetGenericMethodsByName(Type type, string methodName)
            {
                return GetGenericMethods(type)
                    .Where(m => m.Name == methodName)
                    .ToArray();
            }
            
            // Obtiene información sobre un método genérico
            public static GenericMethodInfo GetGenericMethodInfo(MethodInfo method)
            {
                if (!method.IsGenericMethod)
                    throw new ArgumentException("Method must be a generic method");
                
                return new GenericMethodInfo
                {
                    Name = method.Name,
                    IsGenericMethod = method.IsGenericMethod,
                    IsGenericMethodDefinition = method.IsGenericMethodDefinition,
                    GenericTypeArguments = method.GetGenericArguments(),
                    GenericTypeParameters = method.GetGenericMethodDefinition().GetGenericArguments(),
                    Parameters = method.GetParameters(),
                    ReturnType = method.ReturnType
                };
            }
            
            // Invoca un método genérico
            public static object InvokeGenericMethod(MethodInfo method, object instance, Type[] typeArguments, object[] parameters)
            {
                if (!method.IsGenericMethod)
                    throw new ArgumentException("Method must be a generic method");
                
                var constructedMethod = method.MakeGenericMethod(typeArguments);
                return constructedMethod.Invoke(instance, parameters);
            }
        }
        
        public class GenericMethodInfo
        {
            public string Name { get; set; }
            public bool IsGenericMethod { get; set; }
            public bool IsGenericMethodDefinition { get; set; }
            public Type[] GenericTypeArguments { get; set; }
            public Type[] GenericTypeParameters { get; set; }
            public ParameterInfo[] Parameters { get; set; }
            public Type ReturnType { get; set; }
        }
        
        public class GenericPropertyInspector
        {
            // Obtiene propiedades genéricas de un tipo
            public static PropertyInfo[] GetGenericProperties(Type type)
            {
                return type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .Where(p => p.PropertyType.IsGenericType)
                    .ToArray();
            }
            
            // Obtiene información sobre una propiedad genérica
            public static GenericPropertyInfo GetGenericPropertyInfo(PropertyInfo property)
            {
                if (!property.PropertyType.IsGenericType)
                    throw new ArgumentException("Property type must be generic");
                
                return new GenericPropertyInfo
                {
                    Name = property.Name,
                    PropertyType = property.PropertyType,
                    IsGenericType = property.PropertyType.IsGenericType,
                    GenericTypeArguments = property.PropertyType.GenericTypeArguments,
                    CanRead = property.CanRead,
                    CanWrite = property.CanWrite,
                    GetMethod = property.GetMethod,
                    SetMethod = property.SetMethod
                };
            }
            
            // Obtiene el valor de una propiedad genérica
            public static object GetGenericPropertyValue(object instance, PropertyInfo property)
            {
                if (!property.CanRead)
                    throw new InvalidOperationException("Property is not readable");
                
                return property.GetValue(instance);
            }
            
            // Establece el valor de una propiedad genérica
            public static void SetGenericPropertyValue(object instance, PropertyInfo property, object value)
            {
                if (!property.CanWrite)
                    throw new InvalidOperationException("Property is not writable");
                
                property.SetValue(instance, value);
            }
        }
        
        public class GenericPropertyInfo
        {
            public string Name { get; set; }
            public Type PropertyType { get; set; }
            public bool IsGenericType { get; set; }
            public Type[] GenericTypeArguments { get; set; }
            public bool CanRead { get; set; }
            public bool CanWrite { get; set; }
            public MethodInfo GetMethod { get; set; }
            public MethodInfo SetMethod { get; set; }
        }
    }
    
    // ===== REFLECTION PARA COLECCIONES GENÉRICAS =====
    namespace CollectionReflection
    {
        public class GenericCollectionInspector
        {
            // Verifica si un tipo es una colección genérica
            public static bool IsGenericCollection(Type type)
            {
                if (!type.IsGenericType) return false;
                
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                return genericTypeDefinition == typeof(List<>) ||
                       genericTypeDefinition == typeof(Dictionary<,>) ||
                       genericTypeDefinition == typeof(HashSet<>) ||
                       genericTypeDefinition == typeof(Queue<>) ||
                       genericTypeDefinition == typeof(Stack<>) ||
                       genericTypeDefinition == typeof(LinkedList<>) ||
                       genericTypeDefinition == typeof(SortedList<,>) ||
                       genericTypeDefinition == typeof(SortedDictionary<,>);
            }
            
            // Obtiene información sobre una colección genérica
            public static GenericCollectionInfo GetGenericCollectionInfo(Type type)
            {
                if (!IsGenericCollection(type))
                    throw new ArgumentException("Type must be a generic collection");
                
                var genericArguments = type.GenericTypeArguments;
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                
                return new GenericCollectionInfo
                {
                    CollectionType = genericTypeDefinition,
                    ElementType = genericArguments[0],
                    KeyType = genericArguments.Length > 1 ? genericArguments[0] : null,
                    ValueType = genericArguments.Length > 1 ? genericArguments[1] : null,
                    IsKeyValueCollection = genericArguments.Length == 2,
                    GenericArguments = genericArguments
                };
            }
            
            // Crea una colección genérica vacía
            public static object CreateEmptyGenericCollection(Type collectionType, Type elementType)
            {
                var constructedType = collectionType.MakeGenericType(elementType);
                return Activator.CreateInstance(constructedType);
            }
            
            // Crea una colección genérica con elementos iniciales
            public static object CreateGenericCollectionWithItems(Type collectionType, Type elementType, IEnumerable items)
            {
                var constructedType = collectionType.MakeGenericType(elementType);
                var collection = Activator.CreateInstance(constructedType);
                
                var addMethod = constructedType.GetMethod("Add");
                if (addMethod != null)
                {
                    foreach (var item in items)
                    {
                        addMethod.Invoke(collection, new[] { item });
                    }
                }
                
                return collection;
            }
            
            // Convierte una colección genérica a array
            public static Array ConvertGenericCollectionToArray(object collection)
            {
                var collectionType = collection.GetType();
                if (!IsGenericCollection(collectionType))
                    throw new ArgumentException("Object must be a generic collection");
                
                var elementType = collectionType.GenericTypeArguments[0];
                var toArrayMethod = collectionType.GetMethod("ToArray");
                
                if (toArrayMethod != null)
                {
                    return (Array)toArrayMethod.Invoke(collection, null);
                }
                
                // Fallback: usar reflection para iterar
                var enumerable = (IEnumerable)collection;
                var list = new List<object>();
                foreach (var item in enumerable)
                {
                    list.Add(item);
                }
                
                var array = Array.CreateInstance(elementType, list.Count);
                for (int i = 0; i < list.Count; i++)
                {
                    array.SetValue(list[i], i);
                }
                
                return array;
            }
        }
        
        public class GenericCollectionInfo
        {
            public Type CollectionType { get; set; }
            public Type ElementType { get; set; }
            public Type KeyType { get; set; }
            public Type ValueType { get; set; }
            public bool IsKeyValueCollection { get; set; }
            public Type[] GenericArguments { get; set; }
        }
    }
    
    // ===== REFLECTION PARA SERIALIZACIÓN GENÉRICA =====
    namespace SerializationReflection
    {
        public class GenericSerializationInspector
        {
            // Verifica si un tipo es serializable genéricamente
            public static bool IsGenericSerializable(Type type)
            {
                if (!type.IsGenericType) return false;
                
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                var genericArguments = type.GenericTypeArguments;
                
                // Verificar que todos los argumentos genéricos sean serializables
                return genericArguments.All(arg => IsTypeSerializable(arg));
            }
            
            // Verifica si un tipo individual es serializable
            public static bool IsTypeSerializable(Type type)
            {
                if (type.IsPrimitive || type == typeof(string) || type == typeof(DateTime))
                    return true;
                
                if (type.IsGenericType)
                    return IsGenericSerializable(type);
                
                if (type.IsClass)
                {
                    // Verificar atributos de serialización
                    var serializableAttribute = type.GetCustomAttribute<SerializableAttribute>();
                    return serializableAttribute != null;
                }
                
                return false;
            }
            
            // Obtiene información de serialización de un tipo genérico
            public static GenericSerializationInfo GetGenericSerializationInfo(Type type)
            {
                if (!type.IsGenericType)
                    throw new ArgumentException("Type must be generic");
                
                var genericArguments = type.GenericTypeArguments;
                var serializableArguments = genericArguments.Select(IsTypeSerializable).ToArray();
                
                return new GenericSerializationInfo
                {
                    Type = type,
                    GenericArguments = genericArguments,
                    SerializableArguments = serializableArguments,
                    IsFullySerializable = serializableArguments.All(x => x),
                    SerializationComplexity = CalculateSerializationComplexity(type)
                };
            }
            
            // Calcula la complejidad de serialización
            private static SerializationComplexity CalculateSerializationComplexity(Type type)
            {
                if (type.IsPrimitive || type == typeof(string))
                    return SerializationComplexity.Simple;
                
                if (type.IsGenericType)
                {
                    var genericArguments = type.GenericTypeArguments;
                    var maxComplexity = genericArguments.Max(arg => CalculateSerializationComplexity(arg));
                    
                    if (type.GetGenericTypeDefinition() == typeof(List<>) ||
                        type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    {
                        return (SerializationComplexity)(Math.Min((int)maxComplexity + 1, 3));
                    }
                }
                
                if (type.IsClass)
                    return SerializationComplexity.Complex;
                
                return SerializationComplexity.Simple;
            }
            
            // Crea un serializador genérico usando reflection
            public static object CreateGenericSerializer(Type serializerType, Type targetType)
            {
                if (!serializerType.IsGenericType)
                    throw new ArgumentException("Serializer type must be generic");
                
                var constructedType = serializerType.MakeGenericType(targetType);
                return Activator.CreateInstance(constructedType);
            }
        }
        
        public class GenericSerializationInfo
        {
            public Type Type { get; set; }
            public Type[] GenericArguments { get; set; }
            public bool[] SerializableArguments { get; set; }
            public bool IsFullySerializable { get; set; }
            public SerializationComplexity SerializationComplexity { get; set; }
        }
        
        public enum SerializationComplexity
        {
            Simple = 1,
            Moderate = 2,
            Complex = 3
        }
    }
    
    // ===== REFLECTION PARA VALIDACIÓN GENÉRICA =====
    namespace ValidationReflection
    {
        public class GenericValidationInspector
        {
            // Verifica si un tipo tiene validadores genéricos
            public static bool HasGenericValidators(Type type)
            {
                if (!type.IsGenericType) return false;
                
                var genericArguments = type.GenericTypeArguments;
                var validatorTypes = GetValidatorTypesForType(type);
                
                return validatorTypes.Any();
            }
            
            // Obtiene tipos de validador para un tipo genérico
            public static Type[] GetValidatorTypesForType(Type type)
            {
                var validatorInterface = typeof(IValidator<>);
                var validatorTypes = new List<Type>();
                
                // Buscar en el assembly actual
                var types = type.Assembly.GetTypes();
                foreach (var t in types)
                {
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == validatorInterface)
                    {
                        var genericArguments = t.GenericTypeArguments;
                        if (genericArguments.Length == 1 && CanValidateType(genericArguments[0], type))
                        {
                            validatorTypes.Add(t);
                        }
                    }
                }
                
                return validatorTypes.ToArray();
            }
            
            // Verifica si un validador puede validar un tipo
            private static bool CanValidateType(Type validatorType, Type targetType)
            {
                if (validatorType == targetType) return true;
                
                if (targetType.IsGenericType && validatorType.IsGenericType)
                {
                    var targetGenericDef = targetType.GetGenericTypeDefinition();
                    var validatorGenericDef = validatorType.GetGenericTypeDefinition();
                    
                    if (targetGenericDef == validatorGenericDef)
                    {
                        var targetArgs = targetType.GenericTypeArguments;
                        var validatorArgs = validatorType.GenericTypeArguments;
                        
                        if (targetArgs.Length == validatorArgs.Length)
                        {
                            for (int i = 0; i < targetArgs.Length; i++)
                            {
                                if (!CanValidateType(validatorArgs[i], targetArgs[i]))
                                    return false;
                            }
                            return true;
                        }
                    }
                }
                
                return false;
            }
            
            // Crea un validador genérico usando reflection
            public static object CreateGenericValidator(Type validatorType, Type targetType)
            {
                if (!validatorType.IsGenericType)
                    throw new ArgumentException("Validator type must be generic");
                
                var constructedType = validatorType.MakeGenericType(targetType);
                return Activator.CreateInstance(constructedType);
            }
            
            // Ejecuta validación genérica usando reflection
            public static ValidationResult ExecuteGenericValidation(object validator, object target)
            {
                var validatorType = validator.GetType();
                var validateMethod = validatorType.GetMethod("Validate");
                
                if (validateMethod == null)
                    throw new InvalidOperationException("Validator must have a Validate method");
                
                var result = validateMethod.Invoke(validator, new[] { target });
                return (ValidationResult)result;
            }
        }
        
        public interface IValidator<T>
        {
            ValidationResult Validate(T item);
        }
        
        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
            public List<string> Warnings { get; set; } = new List<string>();
        }
    }
    
    // ===== REFLECTION PARA CACHE GENÉRICO =====
    namespace CacheReflection
    {
        public class GenericCacheInspector
        {
            // Verifica si un tipo es un cache genérico
            public static bool IsGenericCache(Type type)
            {
                if (!type.IsGenericType) return false;
                
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                return genericTypeDefinition.Name.Contains("Cache") ||
                       genericTypeDefinition.GetInterfaces().Any(i => i.Name.Contains("ICache"));
            }
            
            // Obtiene información sobre un cache genérico
            public static GenericCacheInfo GetGenericCacheInfo(Type type)
            {
                if (!IsGenericCache(type))
                    throw new ArgumentException("Type must be a generic cache");
                
                var genericArguments = type.GenericTypeArguments;
                
                return new GenericCacheInfo
                {
                    CacheType = type,
                    KeyType = genericArguments[0],
                    ValueType = genericArguments.Length > 1 ? genericArguments[1] : genericArguments[0],
                    IsKeyValueCache = genericArguments.Length == 2,
                    GenericArguments = genericArguments
                };
            }
            
            // Crea un cache genérico usando reflection
            public static object CreateGenericCache(Type cacheType, Type keyType, Type valueType)
            {
                if (!IsGenericCache(cacheType))
                    throw new ArgumentException("Type must be a generic cache");
                
                var constructedType = cacheType.MakeGenericType(keyType, valueType);
                return Activator.CreateInstance(constructedType);
            }
            
            // Ejecuta operaciones de cache genérico usando reflection
            public static object ExecuteCacheOperation(object cache, string operation, object key, object value = null)
            {
                var cacheType = cache.GetType();
                var method = cacheType.GetMethod(operation);
                
                if (method == null)
                    throw new InvalidOperationException($"Method '{operation}' not found");
                
                var parameters = method.GetParameters();
                var args = new List<object>();
                
                if (parameters.Length > 0)
                    args.Add(key);
                
                if (parameters.Length > 1 && value != null)
                    args.Add(value);
                
                return method.Invoke(cache, args.ToArray());
            }
        }
        
        public class GenericCacheInfo
        {
            public Type CacheType { get; set; }
            public Type KeyType { get; set; }
            public Type ValueType { get; set; }
            public bool IsKeyValueCache { get; set; }
            public Type[] GenericArguments { get; set; }
        }
    }
    
    // ===== REFLECTION PARA WORKFLOW GENÉRICO =====
    namespace WorkflowReflection
    {
        public class GenericWorkflowInspector
        {
            // Verifica si un tipo es un workflow genérico
            public static bool IsGenericWorkflow(Type type)
            {
                if (!type.IsGenericType) return false;
                
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                return genericTypeDefinition.Name.Contains("Workflow") ||
                       genericTypeDefinition.GetInterfaces().Any(i => i.Name.Contains("IWorkflow"));
            }
            
            // Obtiene información sobre un workflow genérico
            public static GenericWorkflowInfo GetGenericWorkflowInfo(Type type)
            {
                if (!IsGenericWorkflow(type))
                    throw new ArgumentException("Type must be a generic workflow");
                
                var genericArguments = type.GenericTypeArguments;
                
                return new GenericWorkflowInfo
                {
                    WorkflowType = type,
                    ContextType = genericArguments[0],
                    GenericArguments = genericArguments
                };
            }
            
            // Crea un workflow genérico usando reflection
            public static object CreateGenericWorkflow(Type workflowType, Type contextType)
            {
                if (!IsGenericWorkflow(workflowType))
                    throw new ArgumentException("Type must be a generic workflow");
                
                var constructedType = workflowType.MakeGenericType(contextType);
                return Activator.CreateInstance(constructedType);
            }
            
            // Ejecuta un workflow genérico usando reflection
            public static async Task<object> ExecuteGenericWorkflowAsync(object workflow, object context)
            {
                var workflowType = workflow.GetType();
                var executeMethod = workflowType.GetMethod("ExecuteAsync");
                
                if (executeMethod == null)
                    throw new InvalidOperationException("Workflow must have an ExecuteAsync method");
                
                var result = executeMethod.Invoke(workflow, new[] { context, CancellationToken.None });
                
                if (result is Task task)
                {
                    await task;
                    var resultProperty = task.GetType().GetProperty("Result");
                    return resultProperty?.GetValue(task);
                }
                
                return result;
            }
        }
        
        public class GenericWorkflowInfo
        {
            public Type WorkflowType { get; set; }
            public Type ContextType { get; set; }
            public Type[] GenericArguments { get; set; }
        }
    }
}

// Uso de Generics y Reflection
public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Generics y Reflection - Clase 7 ===\n");
        
        Console.WriteLine("Los componentes implementados incluyen:");
        Console.WriteLine("1. Reflection básico con generics para inspección de tipos");
        Console.WriteLine("2. Creación dinámica de tipos genéricos usando reflection");
        Console.WriteLine("3. Reflection avanzado para métodos y propiedades genéricas");
        Console.WriteLine("4. Reflection para colecciones genéricas");
        Console.WriteLine("5. Reflection para serialización genérica");
        Console.WriteLine("6. Reflection para validación genérica");
        Console.WriteLine("7. Reflection para cache genérico");
        Console.WriteLine("8. Reflection para workflow genérico");
        
        Console.WriteLine("\nBeneficios de esta implementación:");
        Console.WriteLine("- Código dinámico y flexible");
        Console.WriteLine("- Inspección de tipos genéricos en tiempo de ejecución");
        Console.WriteLine("- Creación de instancias genéricas dinámicamente");
        Console.WriteLine("- Manipulación avanzada de tipos genéricos");
        Console.WriteLine("- Patrones de diseño más flexibles");
        
        Console.WriteLine("\nCasos de uso principales:");
        Console.WriteLine("- Factories genéricas dinámicas");
        Console.WriteLine("- Serialización genérica automática");
        Console.WriteLine("- Validación genérica basada en reflection");
        Console.WriteLine("- Cache genérico configurado dinámicamente");
        Console.WriteLine("- Workflows genéricos ejecutados dinámicamente");
        Console.WriteLine("- Plugins genéricos cargados dinámicamente");
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Inspector de Tipos Genéricos
Implementa un inspector que analice tipos genéricos usando reflection.

### Ejercicio 2: Factory Genérica Dinámica
Crea una factory que genere instancias genéricas dinámicamente.

### Ejercicio 3: Validador Genérico con Reflection
Implementa un validador que use reflection para validar objetos genéricos.

## 🔍 Puntos Clave

1. **Reflection básico** para inspección de tipos genéricos
2. **Creación dinámica** de tipos genéricos construidos
3. **Reflection avanzado** para métodos y propiedades genéricas
4. **Reflection para colecciones** y estructuras genéricas
5. **Reflection para operaciones** genéricas especializadas

## 📚 Recursos Adicionales

- [Microsoft Docs - Reflection and Generics](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/reflection-and-generics)
- [Advanced Reflection with Generics](https://docs.microsoft.com/en-us/dotnet/framework/reflection-and-codedom/reflection-and-generic-types)

---

**🎯 ¡Has completado la Clase 7! Ahora comprendes Generics y Reflection**

**📚 [Siguiente: Clase 8 - Generics y Performance](clase_8_generics_performance.md)**
