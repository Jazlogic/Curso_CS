# 🚀 Clase 7: Reflection Avanzada

## 📋 Información de la Clase

- **Módulo**: Junior Level 3 - Programación Orientada a Objetos Avanzada
- **Duración**: 2 horas
- **Nivel**: Intermedio
- **Prerrequisitos**: Completar Clase 6 (Arquitectura Modular)

## 🎯 Objetivos de Aprendizaje

- Dominar el uso avanzado de reflection en C#
- Crear objetos dinámicamente en tiempo de ejecución
- Implementar sistemas de plugins y extensibilidad
- Utilizar reflection para serialización dinámica
- Crear frameworks de metaprogramación

---

## 📚 Navegación del Módulo 3

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_herencia_multiple.md) | Herencia Múltiple y Composición | |
| [Clase 2](clase_2_interfaces_avanzadas.md) | Interfaces Avanzadas | |
| [Clase 3](clase_3_polimorfismo_avanzado.md) | Polimorfismo Avanzado | |
| [Clase 4](clase_4_patrones_diseno.md) | Patrones de Diseño Básicos | |
| [Clase 5](clase_5_principios_solid.md) | Principios SOLID | |
| [Clase 6](clase_6_arquitectura_modular.md) | Arquitectura Modular | ← Anterior |
| **Clase 7** | **Reflection Avanzada** | ← Estás aquí |
| [Clase 8](clase_8_serializacion_avanzada.md) | Serialización Avanzada | Siguiente → |
| [Clase 9](clase_9_testing_unitario.md) | Testing Unitario | |
| [Clase 10](clase_10_proyecto_final.md) | Proyecto Final Integrador | |

**← [Volver al README del Módulo 3](../junior_3/README.md)**

---

## 📚 Contenido Teórico

### 1. Creación Dinámica de Objetos

La reflection permite crear objetos y ejecutar métodos de forma dinámica en tiempo de ejecución.

```csharp
// Clases de ejemplo para reflection
public class Persona
{
    public string Nombre { get; set; }
    public int Edad { get; set; }
    
    public Persona() { }
    
    public Persona(string nombre, int edad)
    {
        Nombre = nombre;
        Edad = edad;
    }
    
    public void Saludar()
    {
        Console.WriteLine($"Hola, soy {Nombre} y tengo {Edad} años");
    }
    
    public string ObtenerInformacion()
    {
        return $"Nombre: {Nombre}, Edad: {Edad}";
    }
}

// Factory dinámico usando reflection
public class DynamicObjectFactory
{
    public static T CreateInstance<T>(params object[] parameters)
    {
        var type = typeof(T);
        var constructor = type.GetConstructor(parameters.Select(p => p.GetType()).ToArray());
        
        if (constructor == null)
        {
            throw new InvalidOperationException($"Constructor no encontrado para {type.Name}");
        }
        
        return (T)constructor.Invoke(parameters);
    }
    
    public static object CreateInstance(Type type, params object[] parameters)
    {
        var constructor = type.GetConstructor(parameters.Select(p => p.GetType()).ToArray());
        
        if (constructor == null)
        {
            throw new InvalidOperationException($"Constructor no encontrado para {type.Name}");
        }
        
        return constructor.Invoke(parameters);
    }
    
    public static T CreateInstanceWithDefaultConstructor<T>()
    {
        var type = typeof(T);
        var constructor = type.GetConstructor(Type.EmptyTypes);
        
        if (constructor == null)
        {
            throw new InvalidOperationException($"Constructor sin parámetros no encontrado para {type.Name}");
        }
        
        return (T)constructor.Invoke(null);
    }
}

// Uso del factory dinámico
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Creación Dinámica de Objetos ===\n");
        
        // Crear instancia con constructor sin parámetros
        var persona1 = DynamicObjectFactory.CreateInstanceWithDefaultConstructor<Persona>();
        persona1.Nombre = "Juan";
        persona1.Edad = 25;
        persona1.Saludar();
        
        // Crear instancia con constructor con parámetros
        var persona2 = DynamicObjectFactory.CreateInstance<Persona>("María", 30);
        persona2.Saludar();
        
        // Crear instancia usando Type
        var tipoPersona = typeof(Persona);
        var persona3 = (Persona)DynamicObjectFactory.CreateInstance(tipoPersona, "Carlos", 35);
        persona3.Saludar();
        
        // Crear instancia desde string (nombre de clase)
        var nombreClase = "Persona";
        var assembly = Assembly.GetExecutingAssembly();
        var tipo = assembly.GetType($"YourNamespace.{nombreClase}");
        
        if (tipo != null)
        {
            var persona4 = (Persona)DynamicObjectFactory.CreateInstance(tipo, "Ana", 28);
            persona4.Saludar();
        }
    }
}
```

### 2. Invocación Dinámica de Métodos

La reflection permite invocar métodos de forma dinámica, incluso métodos privados y protegidos.

```csharp
// Clase con métodos públicos y privados
public class Calculadora
{
    private int _contador;
    
    public Calculadora()
    {
        _contador = 0;
    }
    
    public int Sumar(int a, int b)
    {
        _contador++;
        return a + b;
    }
    
    public int Restar(int a, int b)
    {
        _contador++;
        return a - b;
    }
    
    private int Multiplicar(int a, int b)
    {
        _contador++;
        return a * b;
    }
    
    protected int Dividir(int a, int b)
    {
        if (b == 0)
            throw new DivideByZeroException();
        
        _contador++;
        return a / b;
    }
    
    public int ObtenerContador() => _contador;
}

// Invocador dinámico de métodos
public class DynamicMethodInvoker
{
    public static object InvokeMethod(object instance, string methodName, params object[] parameters)
    {
        var type = instance.GetType();
        var method = type.GetMethod(methodName, 
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (method == null)
        {
            throw new InvalidOperationException($"Método '{methodName}' no encontrado en {type.Name}");
        }
        
        return method.Invoke(instance, parameters);
    }
    
    public static T InvokeMethod<T>(object instance, string methodName, params object[] parameters)
    {
        var result = InvokeMethod(instance, methodName, parameters);
        return (T)result;
    }
    
    public static object InvokeStaticMethod(Type type, string methodName, params object[] parameters)
    {
        var method = type.GetMethod(methodName, 
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        
        if (method == null)
        {
            throw new InvalidOperationException($"Método estático '{methodName}' no encontrado en {type.Name}");
        }
        
        return method.Invoke(null, parameters);
    }
    
    public static void SetPropertyValue(object instance, string propertyName, object value)
    {
        var type = instance.GetType();
        var property = type.GetProperty(propertyName);
        
        if (property == null)
        {
            throw new InvalidOperationException($"Propiedad '{propertyName}' no encontrada en {type.Name}");
        }
        
        property.SetValue(instance, value);
    }
    
    public static object GetPropertyValue(object instance, string propertyName)
    {
        var type = instance.GetType();
        var property = type.GetProperty(propertyName);
        
        if (property == null)
        {
            throw new InvalidOperationException($"Propiedad '{propertyName}' no encontrada en {type.Name}");
        }
        
        return property.GetValue(instance);
    }
}

// Uso del invocador dinámico
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Invocación Dinámica de Métodos ===\n");
        
        var calculadora = new Calculadora();
        
        // Invocar método público
        var resultadoSuma = DynamicMethodInvoker.InvokeMethod<int>(calculadora, "Sumar", 10, 5);
        Console.WriteLine($"Suma: {resultadoSuma}");
        
        // Invocar método privado
        var resultadoMultiplicacion = DynamicMethodInvoker.InvokeMethod<int>(calculadora, "Multiplicar", 4, 6);
        Console.WriteLine($"Multiplicación: {resultadoMultiplicacion}");
        
        // Invocar método protegido
        var resultadoDivision = DynamicMethodInvoker.InvokeMethod<int>(calculadora, "Dividir", 20, 4);
        Console.WriteLine($"División: {resultadoDivision}");
        
        // Obtener valor de propiedad
        var contador = DynamicMethodInvoker.GetPropertyValue(calculadora, "ObtenerContador");
        Console.WriteLine($"Contador: {contador}");
        
        // Invocar método estático (ejemplo con Math)
        var raizCuadrada = DynamicMethodInvoker.InvokeStaticMethod<double>(typeof(Math), "Sqrt", 16.0);
        Console.WriteLine($"Raíz cuadrada de 16: {raizCuadrada}");
    }
}
```

### 3. Sistema de Plugins con Reflection

La reflection permite crear sistemas de plugins que cargan y ejecutan código dinámicamente.

```csharp
// Interfaz base para plugins
public interface IPlugin
{
    string Nombre { get; }
    string Version { get; }
    void Ejecutar();
    string ObtenerDescripcion();
}

// Atributo personalizado para plugins
[AttributeUsage(AttributeTargets.Class)]
public class PluginAttribute : Attribute
{
    public string Nombre { get; }
    public string Version { get; }
    public string Descripcion { get; }
    
    public PluginAttribute(string nombre, string version, string descripcion)
    {
        Nombre = nombre;
        Version = version;
        Descripcion = descripcion;
    }
}

// Plugins de ejemplo
[Plugin("Calculadora Básica", "1.0", "Realiza operaciones matemáticas básicas")]
public class CalculadoraPlugin : IPlugin
{
    public string Nombre => "Calculadora Básica";
    public string Version => "1.0";
    
    public void Ejecutar()
    {
        Console.WriteLine("=== Calculadora Básica ===");
        Console.Write("Ingrese primer número: ");
        var num1 = Convert.ToDouble(Console.ReadLine());
        
        Console.Write("Ingrese segundo número: ");
        var num2 = Convert.ToDouble(Console.ReadLine());
        
        Console.WriteLine($"Suma: {num1 + num2}");
        Console.WriteLine($"Resta: {num1 - num2}");
        Console.WriteLine($"Multiplicación: {num1 * num2}");
        Console.WriteLine($"División: {num1 / num2}");
    }
    
    public string ObtenerDescripcion()
    {
        return "Plugin que realiza operaciones matemáticas básicas";
    }
}

[Plugin("Generador de Contraseñas", "1.0", "Genera contraseñas seguras")]
public class PasswordGeneratorPlugin : IPlugin
{
    public string Nombre => "Generador de Contraseñas";
    public string Version => "1.0";
    
    public void Ejecutar()
    {
        Console.WriteLine("=== Generador de Contraseñas ===");
        var random = new Random();
        var caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var contraseña = new char[12];
        
        for (int i = 0; i < 12; i++)
        {
            contraseña[i] = caracteres[random.Next(caracteres.Length)];
        }
        
        Console.WriteLine($"Contraseña generada: {new string(contraseña)}");
    }
    
    public string ObtenerDescripcion()
    {
        return "Plugin que genera contraseñas seguras aleatorias";
    }
}

// Cargador de plugins
public class PluginLoader
{
    public static List<IPlugin> CargarPlugins(string directorio)
    {
        var plugins = new List<IPlugin>();
        
        if (!Directory.Exists(directorio))
        {
            Console.WriteLine($"Directorio {directorio} no encontrado");
            return plugins;
        }
        
        var archivos = Directory.GetFiles(directorio, "*.dll");
        
        foreach (var archivo in archivos)
        {
            try
            {
                var assembly = Assembly.LoadFrom(archivo);
                var tipos = assembly.GetTypes();
                
                foreach (var tipo in tipos)
                {
                    if (typeof(IPlugin).IsAssignableFrom(tipo) && !tipo.IsInterface && !tipo.IsAbstract)
                    {
                        var plugin = (IPlugin)Activator.CreateInstance(tipo);
                        plugins.Add(plugin);
                        
                        Console.WriteLine($"Plugin cargado: {plugin.Nombre} v{plugin.Version}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar plugin desde {archivo}: {ex.Message}");
            }
        }
        
        return plugins;
    }
    
    public static List<IPlugin> CargarPluginsDelAssembly(Assembly assembly)
    {
        var plugins = new List<IPlugin>();
        var tipos = assembly.GetTypes();
        
        foreach (var tipo in tipos)
        {
            if (typeof(IPlugin).IsAssignableFrom(tipo) && !tipo.IsInterface && !tipo.IsAbstract)
            {
                try
                {
                    var plugin = (IPlugin)Activator.CreateInstance(tipo);
                    plugins.Add(plugin);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al crear instancia de {tipo.Name}: {ex.Message}");
                }
            }
        }
        
        return plugins;
    }
}

// Gestor de plugins
public class PluginManager
{
    private readonly List<IPlugin> _plugins;
    
    public PluginManager()
    {
        _plugins = new List<IPlugin>();
    }
    
    public void CargarPlugins()
    {
        // Cargar plugins del assembly actual
        var assembly = Assembly.GetExecutingAssembly();
        var plugins = PluginLoader.CargarPluginsDelAssembly(assembly);
        _plugins.AddRange(plugins);
        
        Console.WriteLine($"\nTotal de plugins cargados: {_plugins.Count}");
    }
    
    public void MostrarPlugins()
    {
        Console.WriteLine("\n=== Plugins Disponibles ===");
        for (int i = 0; i < _plugins.Count; i++)
        {
            var plugin = _plugins[i];
            Console.WriteLine($"{i + 1}. {plugin.Nombre} v{plugin.Version}");
            Console.WriteLine($"   {plugin.ObtenerDescripcion()}");
            Console.WriteLine();
        }
    }
    
    public void EjecutarPlugin(int indice)
    {
        if (indice < 0 || indice >= _plugins.Count)
        {
            Console.WriteLine("Índice de plugin inválido");
            return;
        }
        
        var plugin = _plugins[indice];
        Console.WriteLine($"\nEjecutando plugin: {plugin.Nombre}");
        Console.WriteLine(new string('-', 50));
        
        try
        {
            plugin.Ejecutar();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al ejecutar plugin: {ex.Message}");
        }
        
        Console.WriteLine(new string('-', 50));
    }
    
    public void EjecutarTodosLosPlugins()
    {
        Console.WriteLine("\n=== Ejecutando Todos los Plugins ===");
        
        foreach (var plugin in _plugins)
        {
            Console.WriteLine($"\n--- {plugin.Nombre} ---");
            try
            {
                plugin.Ejecutar();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}

// Uso del sistema de plugins
public class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Sistema de Plugins con Reflection ===\n");
        
        var pluginManager = new PluginManager();
        
        // Cargar plugins
        pluginManager.CargarPlugins();
        
        // Mostrar plugins disponibles
        pluginManager.MostrarPlugins();
        
        // Ejecutar plugin específico
        Console.Write("Seleccione un plugin para ejecutar (1-2): ");
        if (int.TryParse(Console.ReadLine(), out int seleccion))
        {
            pluginManager.EjecutarPlugin(seleccion - 1);
        }
        
        // Ejecutar todos los plugins
        Console.WriteLine("\nPresione Enter para ejecutar todos los plugins...");
        Console.ReadLine();
        pluginManager.EjecutarTodosLosPlugins();
    }
}
```

## 🧪 Ejercicios Prácticos

### Ejercicio 1: Factory de Objetos Dinámico
Crea un factory que permita crear objetos de diferentes tipos usando reflection.

### Ejercicio 2: Sistema de Validación por Reflection
Implementa un sistema de validación que use reflection para validar propiedades de objetos.

### Ejercicio 3: Cargador de Módulos
Desarrolla un sistema que permita cargar módulos dinámicamente desde archivos DLL.

## 🔍 Puntos Clave

1. **Reflection** permite inspeccionar y manipular tipos en tiempo de ejecución
2. **Creación dinámica** de objetos usando constructores y parámetros
3. **Invocación dinámica** de métodos públicos, privados y protegidos
4. **Sistemas de plugins** que cargan código dinámicamente
5. **Metaprogramación** para crear frameworks flexibles y extensibles

## 📚 Recursos Adicionales

- [Reflection - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/framework/reflection-and-codedom/reflection)
- [Dynamic Programming - C# Guide](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/types/using-type-dynamic)
- [Plugin Architecture Patterns](https://martinfowler.com/articles/injection.html)

---

**🎯 ¡Has completado la Clase 7! Ahora dominas la reflection avanzada en C#**

**📚 [Siguiente: Clase 8 - Serialización Avanzada](clase_8_serializacion_avanzada.md)**
