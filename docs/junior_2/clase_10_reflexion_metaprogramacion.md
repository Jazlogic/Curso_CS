# Clase 10: Reflexión y Metaprogramación en C#

## 🎯 Objetivos de la Clase
- Comprender qué es la reflexión y su propósito
- Aprender a inspeccionar tipos en tiempo de ejecución
- Entender cómo crear y manipular objetos dinámicamente
- Dominar el uso de atributos personalizados

## 📚 Contenido Teórico

### 1. ¿Qué es la Reflexión?

La **reflexión** es la capacidad de un programa para **inspeccionar y manipular** su propia estructura en tiempo de ejecución. Permite examinar tipos, propiedades, métodos y otros metadatos del código, así como crear instancias y llamar métodos dinámicamente.

#### Usos de la Reflexión:
- **Inspección de tipos**: Obtener información sobre clases, métodos, propiedades
- **Creación dinámica**: Instanciar objetos sin conocer su tipo en tiempo de compilación
- **Inyección de dependencias**: Crear objetos basándose en metadatos
- **Serialización**: Convertir objetos a diferentes formatos
- **Testing**: Crear mocks y stubs dinámicamente

### 2. Inspección de Tipos Básica

#### 2.1 Obtener Información de Tipos

```csharp
using System;
using System.Reflection;
using System.Collections.Generic;

// Clase de ejemplo para demostrar reflexión
public class Persona
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public int Edad { get; set; }
    public string Email { get; set; }
    
    private string informacionPrivada;
    
    public Persona()
    {
        Id = 0;
        Nombre = "";
        Apellido = "";
        Edad = 0;
        Email = "";
        informacionPrivada = "Información privada";
    }
    
    public Persona(int id, string nombre, string apellido, int edad, string email)
    {
        Id = id;
        Nombre = nombre;
        Apellido = apellido;
        Edad = edad;
        Email = email;
        informacionPrivada = "Información privada";
    }
    
    public string ObtenerNombreCompleto()
    {
        return $"{Nombre} {Apellido}";
    }
    
    private void MetodoPrivado()
    {
        Console.WriteLine("Método privado ejecutado");
    }
    
    public override string ToString()
    {
        return $"ID: {Id}, Nombre: {ObtenerNombreCompleto()}, Edad: {Edad}, Email: {Email}";
    }
}

// Clase que demuestra inspección básica de tipos
public class InspeccionTipos
{
    // Obtener información básica de un tipo
    public void InspeccionarTipoBasico()
    {
        Console.WriteLine("=== INSPECCIÓN BÁSICA DE TIPOS ===");
        
        Type tipoPersona = typeof(Persona);
        
        Console.WriteLine($"Nombre del tipo: {tipoPersona.Name}");
        Console.WriteLine($"Nombre completo: {tipoPersona.FullName}");
        Console.WriteLine($"Namespace: {tipoPersona.Namespace}");
        Console.WriteLine($"Assembly: {tipoPersona.Assembly.GetName().Name}");
        Console.WriteLine($"Es clase: {tipoPersona.IsClass}");
        Console.WriteLine($"Es público: {tipoPersona.IsPublic}");
        Console.WriteLine($"Es abstracto: {tipoPersona.IsAbstract}");
        Console.WriteLine($"Es sealed: {tipoPersona.IsSealed}");
        Console.WriteLine($"Tipo base: {tipoPersona.BaseType?.Name ?? "Object"}");
        
        // Obtener interfaces implementadas
        Type[] interfaces = tipoPersona.GetInterfaces();
        Console.WriteLine($"Interfaces implementadas: {interfaces.Length}");
        foreach (Type interfaz in interfaces)
        {
            Console.WriteLine($"  - {interfaz.Name}");
        }
    }
    
    // Inspeccionar constructores
    public void InspeccionarConstructores()
    {
        Console.WriteLine("\n=== INSPECCIÓN DE CONSTRUCTORES ===");
        
        Type tipoPersona = typeof(Persona);
        ConstructorInfo[] constructores = tipoPersona.GetConstructors();
        
        Console.WriteLine($"Constructores encontrados: {constructores.Length}");
        
        for (int i = 0; i < constructores.Length; i++)
        {
            ConstructorInfo constructor = constructores[i];
            Console.WriteLine($"\nConstructor {i + 1}:");
            Console.WriteLine($"  Nombre: {constructor.Name}");
            Console.WriteLine($"  Es público: {constructor.IsPublic}");
            Console.WriteLine($"  Parámetros: {constructor.GetParameters().Length}");
            
            ParameterInfo[] parametros = constructor.GetParameters();
            foreach (ParameterInfo parametro in parametros)
            {
                Console.WriteLine($"    - {parametro.ParameterType.Name} {parametro.Name}");
            }
        }
    }
    
    // Inspeccionar propiedades
    public void InspeccionarPropiedades()
    {
        Console.WriteLine("\n=== INSPECCIÓN DE PROPIEDADES ===");
        
        Type tipoPersona = typeof(Persona);
        
        // Obtener propiedades públicas
        PropertyInfo[] propiedadesPublicas = tipoPersona.GetProperties();
        Console.WriteLine($"Propiedades públicas: {propiedadesPublicas.Length}");
        
        foreach (PropertyInfo propiedad in propiedadesPublicas)
        {
            Console.WriteLine($"\nPropiedad: {propiedad.Name}");
            Console.WriteLine($"  Tipo: {propiedad.PropertyType.Name}");
            Console.WriteLine($"  Puede leer: {propiedad.CanRead}");
            Console.WriteLine($"  Puede escribir: {propiedad.CanWrite}");
            Console.WriteLine($"  Es estática: {propiedad.GetGetMethod()?.IsStatic ?? false}");
            
            // Obtener atributos de la propiedad
            object[] atributos = propiedad.GetCustomAttributes(true);
            if (atributos.Length > 0)
            {
                Console.WriteLine($"  Atributos: {atributos.Length}");
                foreach (object atributo in atributos)
                {
                    Console.WriteLine($"    - {atributo.GetType().Name}");
                }
            }
        }
        
        // Obtener todas las propiedades (incluyendo privadas)
        PropertyInfo[] todasLasPropiedades = tipoPersona.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        Console.WriteLine($"\nTotal de propiedades (públicas y privadas): {todasLasPropiedades.Length}");
    }
    
    // Inspeccionar métodos
    public void InspeccionarMetodos()
    {
        Console.WriteLine("\n=== INSPECCIÓN DE MÉTODOS ===");
        
        Type tipoPersona = typeof(Persona);
        
        // Obtener métodos públicos
        MethodInfo[] metodosPublicos = tipoPersona.GetMethods();
        Console.WriteLine($"Métodos públicos: {metodosPublicos.Length}");
        
        foreach (MethodInfo metodo in metodosPublicos)
        {
            // Filtrar métodos heredados de Object
            if (metodo.DeclaringType == tipoPersona)
            {
                Console.WriteLine($"\nMétodo: {metodo.Name}");
                Console.WriteLine($"  Tipo de retorno: {metodo.ReturnType.Name}");
                Console.WriteLine($"  Es estático: {metodo.IsStatic}");
                Console.WriteLine($"  Es público: {metodo.IsPublic}");
                Console.WriteLine($"  Parámetros: {metodo.GetParameters().Length}");
                
                ParameterInfo[] parametros = metodo.GetParameters();
                foreach (ParameterInfo parametro in parametros)
                {
                    Console.WriteLine($"    - {parametro.ParameterType.Name} {parametro.Name}");
                }
            }
        }
        
        // Obtener métodos privados
        MethodInfo[] metodosPrivados = tipoPersona.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
        Console.WriteLine($"\nMétodos privados: {metodosPrivados.Length}");
        
        foreach (MethodInfo metodo in metodosPrivados)
        {
            if (metodo.DeclaringType == tipoPersona)
            {
                Console.WriteLine($"  - {metodo.Name} (privado)");
            }
        }
    }
    
    // Inspeccionar campos
    public void InspeccionarCampos()
    {
        Console.WriteLine("\n=== INSPECCIÓN DE CAMPOS ===");
        
        Type tipoPersona = typeof(Persona);
        
        // Obtener campos públicos
        FieldInfo[] camposPublicos = tipoPersona.GetFields();
        Console.WriteLine($"Campos públicos: {camposPublicos.Length}");
        
        foreach (FieldInfo campo in camposPublicos)
        {
            Console.WriteLine($"\nCampo: {campo.Name}");
            Console.WriteLine($"  Tipo: {campo.FieldType.Name}");
            Console.WriteLine($"  Es estático: {campo.IsStatic}");
            Console.WriteLine($"  Es público: {campo.IsPublic}");
        }
        
        // Obtener campos privados
        FieldInfo[] camposPrivados = tipoPersona.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        Console.WriteLine($"\nCampos privados: {camposPrivados.Length}");
        
        foreach (FieldInfo campo in camposPrivados)
        {
            Console.WriteLine($"  - {campo.Name} (privado, tipo: {campo.FieldType.Name})");
        }
    }
}
```

### 3. Creación Dinámica de Objetos

#### 3.1 Instanciación y Llamada de Métodos

```csharp
public class CreacionDinamica
{
    // Crear instancia usando constructor sin parámetros
    public void CrearInstanciaSinParametros()
    {
        Console.WriteLine("=== CREACIÓN DE INSTANCIA SIN PARÁMETROS ===");
        
        Type tipoPersona = typeof(Persona);
        
        try
        {
            // Crear instancia usando el constructor sin parámetros
            object instancia = Activator.CreateInstance(tipoPersona);
            
            if (instancia is Persona persona)
            {
                Console.WriteLine($"Instancia creada: {persona}");
                Console.WriteLine($"Tipo: {instancia.GetType().Name}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creando instancia: {ex.Message}");
        }
    }
    
    // Crear instancia usando constructor con parámetros
    public void CrearInstanciaConParametros()
    {
        Console.WriteLine("\n=== CREACIÓN DE INSTANCIA CON PARÁMETROS ===");
        
        Type tipoPersona = typeof(Persona);
        
        try
        {
            // Crear instancia usando el constructor con parámetros
            object[] parametros = { 1, "Juan", "Pérez", 25, "juan.perez@email.com" };
            object instancia = Activator.CreateInstance(tipoPersona, parametros);
            
            if (instancia is Persona persona)
            {
                Console.WriteLine($"Instancia creada: {persona}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creando instancia: {ex.Message}");
        }
    }
    
    // Crear instancia usando constructor específico
    public void CrearInstanciaConConstructorEspecifico()
    {
        Console.WriteLine("\n=== CREACIÓN CON CONSTRUCTOR ESPECÍFICO ===");
        
        Type tipoPersona = typeof(Persona);
        
        try
        {
            // Buscar constructor específico
            ConstructorInfo constructor = tipoPersona.GetConstructor(new Type[] { typeof(int), typeof(string), typeof(string), typeof(int), typeof(string) });
            
            if (constructor != null)
            {
                object[] parametros = { 2, "María", "García", 30, "maria.garcia@email.com" };
                object instancia = constructor.Invoke(parametros);
                
                if (instancia is Persona persona)
                {
                    Console.WriteLine($"Instancia creada usando constructor específico: {persona}");
                }
            }
            else
            {
                Console.WriteLine("Constructor específico no encontrado");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creando instancia: {ex.Message}");
        }
    }
    
    // Llamar métodos dinámicamente
    public void LlamarMetodosDinamicamente()
    {
        Console.WriteLine("\n=== LLAMADA DINÁMICA DE MÉTODOS ===");
        
        // Crear instancia
        Persona persona = new Persona(3, "Carlos", "López", 28, "carlos.lopez@email.com");
        
        Type tipoPersona = persona.GetType();
        
        try
        {
            // Llamar método sin parámetros
            MethodInfo metodoToString = tipoPersona.GetMethod("ToString");
            if (metodoToString != null)
            {
                object resultado = metodoToString.Invoke(persona, null);
                Console.WriteLine($"Resultado de ToString(): {resultado}");
            }
            
            // Llamar método con parámetros
            MethodInfo metodoObtenerNombreCompleto = tipoPersona.GetMethod("ObtenerNombreCompleto");
            if (metodoObtenerNombreCompleto != null)
            {
                object resultado = metodoObtenerNombreCompleto.Invoke(persona, null);
                Console.WriteLine($"Resultado de ObtenerNombreCompleto(): {resultado}");
            }
            
            // Llamar método privado
            MethodInfo metodoPrivado = tipoPersona.GetMethod("MetodoPrivado", BindingFlags.NonPublic | BindingFlags.Instance);
            if (metodoPrivado != null)
            {
                Console.WriteLine("Llamando método privado...");
                metodoPrivado.Invoke(persona, null);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error llamando métodos: {ex.Message}");
        }
    }
    
    // Establecer y obtener valores de propiedades dinámicamente
    public void ManipularPropiedadesDinamicamente()
    {
        Console.WriteLine("\n=== MANIPULACIÓN DINÁMICA DE PROPIEDADES ===");
        
        Persona persona = new Persona();
        Type tipoPersona = persona.GetType();
        
        try
        {
            // Establecer valor de propiedad
            PropertyInfo propiedadNombre = tipoPersona.GetProperty("Nombre");
            if (propiedadNombre != null && propiedadNombre.CanWrite)
            {
                propiedadNombre.SetValue(persona, "Ana");
                Console.WriteLine($"Propiedad Nombre establecida a: {persona.Nombre}");
            }
            
            // Obtener valor de propiedad
            PropertyInfo propiedadId = tipoPersona.GetProperty("Id");
            if (propiedadId != null && propiedadId.CanRead)
            {
                object valor = propiedadId.GetValue(persona);
                Console.WriteLine($"Valor de la propiedad Id: {valor}");
            }
            
            // Establecer múltiples propiedades
            var propiedades = new Dictionary<string, object>
            {
                { "Id", 4 },
                { "Apellido", "Martínez" },
                { "Edad", 27 },
                { "Email", "ana.martinez@email.com" }
            };
            
            foreach (var kvp in propiedades)
            {
                PropertyInfo propiedad = tipoPersona.GetProperty(kvp.Key);
                if (propiedad != null && propiedad.CanWrite)
                {
                    propiedad.SetValue(persona, kvp.Value);
                    Console.WriteLine($"Propiedad {kvp.Key} establecida a: {kvp.Value}");
                }
            }
            
            Console.WriteLine($"Persona final: {persona}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error manipulando propiedades: {ex.Message}");
        }
    }
}
```

### 4. Atributos Personalizados

#### 4.1 Creación y Uso de Atributos

```csharp
// Atributo personalizado para validación
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ValidacionAttribute : Attribute
{
    public int LongitudMinima { get; set; }
    public int LongitudMaxima { get; set; }
    public bool Requerido { get; set; }
    public string MensajeError { get; set; }
    
    public ValidacionAttribute(int longitudMinima = 0, int longitudMaxima = int.MaxValue, bool requerido = false, string mensajeError = "")
    {
        LongitudMinima = longitudMinima;
        LongitudMaxima = longitudMaxima;
        Requerido = requerido;
        MensajeError = mensajeError;
    }
}

// Atributo para documentación
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
public class DocumentacionAttribute : Attribute
{
    public string Descripcion { get; set; }
    public string Autor { get; set; }
    public string Version { get; set; }
    public DateTime FechaCreacion { get; set; }
    
    public DocumentacionAttribute(string descripcion, string autor = "", string version = "1.0")
    {
        Descripcion = descripcion;
        Autor = autor;
        Version = version;
        FechaCreacion = DateTime.Now;
    }
}

// Clase que usa atributos personalizados
[Documentacion("Clase que representa un usuario del sistema", "Desarrollador", "2.0")]
public class Usuario
{
    [Validacion(1, 50, true, "El nombre debe tener entre 1 y 50 caracteres")]
    [Documentacion("Nombre completo del usuario")]
    public string Nombre { get; set; }
    
    [Validacion(1, 100, true, "El email debe tener entre 1 y 100 caracteres")]
    [Documentacion("Dirección de correo electrónico del usuario")]
    public string Email { get; set; }
    
    [Validacion(0, 150, false, "La edad debe estar entre 0 y 150 años")]
    [Documentacion("Edad del usuario en años")]
    public int Edad { get; set; }
    
    [Validacion(0, 1000, false, "El salario debe estar entre 0 y 1000")]
    [Documentacion("Salario del usuario")]
    public decimal Salario { get; set; }
    
    public Usuario()
    {
        Nombre = "";
        Email = "";
        Edad = 0;
        Salario = 0;
    }
    
    public Usuario(string nombre, string email, int edad, decimal salario)
    {
        Nombre = nombre;
        Email = email;
        Edad = edad;
        Salario = salario;
    }
    
    public override string ToString()
    {
        return $"Usuario: {Nombre}, Email: {Email}, Edad: {Edad}, Salario: {Salario:C}";
    }
}

// Clase para trabajar con atributos personalizados
public class TrabajoConAtributos
{
    // Obtener información de atributos de una clase
    public void InspeccionarAtributosClase()
    {
        Console.WriteLine("=== INSPECCIÓN DE ATRIBUTOS DE CLASE ===");
        
        Type tipoUsuario = typeof(Usuario);
        
        // Obtener atributos de la clase
        object[] atributosClase = tipoUsuario.GetCustomAttributes(true);
        Console.WriteLine($"Atributos de la clase {tipoUsuario.Name}: {atributosClase.Length}");
        
        foreach (object atributo in atributosClase)
        {
            if (atributo is DocumentacionAttribute doc)
            {
                Console.WriteLine($"  Documentación:");
                Console.WriteLine($"    Descripción: {doc.Descripcion}");
                Console.WriteLine($"    Autor: {doc.Autor}");
                Console.WriteLine($"    Versión: {doc.Version}");
                Console.WriteLine($"    Fecha de creación: {doc.FechaCreacion:dd/MM/yyyy HH:mm:ss}");
            }
        }
    }
    
    // Obtener información de atributos de propiedades
    public void InspeccionarAtributosPropiedades()
    {
        Console.WriteLine("\n=== INSPECCIÓN DE ATRIBUTOS DE PROPIEDADES ===");
        
        Type tipoUsuario = typeof(Usuario);
        PropertyInfo[] propiedades = tipoUsuario.GetProperties();
        
        foreach (PropertyInfo propiedad in propiedades)
        {
            Console.WriteLine($"\nPropiedad: {propiedad.Name}");
            
            // Obtener atributos de validación
            ValidacionAttribute validacion = propiedad.GetCustomAttribute<ValidacionAttribute>();
            if (validacion != null)
            {
                Console.WriteLine($"  Validación:");
                Console.WriteLine($"    Longitud mínima: {validacion.LongitudMinima}");
                Console.WriteLine($"    Longitud máxima: {validacion.LongitudMaxima}");
                Console.WriteLine($"    Requerido: {validacion.Requerido}");
                Console.WriteLine($"    Mensaje de error: {validacion.MensajeError}");
            }
            
            // Obtener atributos de documentación
            DocumentacionAttribute documentacion = propiedad.GetCustomAttribute<DocumentacionAttribute>();
            if (documentacion != null)
            {
                Console.WriteLine($"  Documentación: {documentacion.Descripcion}");
            }
        }
    }
    
    // Validar objeto usando atributos
    public List<string> ValidarObjeto(Usuario usuario)
    {
        Console.WriteLine("\n=== VALIDACIÓN DE OBJETO ===");
        
        List<string> errores = new List<string>();
        Type tipoUsuario = usuario.GetType();
        PropertyInfo[] propiedades = tipoUsuario.GetProperties();
        
        foreach (PropertyInfo propiedad in propiedades)
        {
            ValidacionAttribute validacion = propiedad.GetCustomAttribute<ValidacionAttribute>();
            if (validacion != null)
            {
                object valor = propiedad.GetValue(usuario);
                
                // Validar si es requerido
                if (validacion.Requerido && (valor == null || (valor is string str && string.IsNullOrEmpty(str))))
                {
                    string mensaje = string.IsNullOrEmpty(validacion.MensajeError) 
                        ? $"La propiedad {propiedad.Name} es requerida"
                        : validacion.MensajeError;
                    errores.Add(mensaje);
                    continue;
                }
                
                // Validar longitud si es string
                if (valor is string valorString)
                {
                    if (valorString.Length < validacion.LongitudMinima || valorString.Length > validacion.LongitudMaxima)
                    {
                        string mensaje = string.IsNullOrEmpty(validacion.MensajeError)
                            ? $"La propiedad {propiedad.Name} debe tener entre {validacion.LongitudMinima} y {validacion.LongitudMaxima} caracteres"
                            : validacion.MensajeError;
                        errores.Add(mensaje);
                    }
                }
                
                // Validar rango si es numérico
                if (valor is int valorInt)
                {
                    if (valorInt < validacion.LongitudMinima || valorInt > validacion.LongitudMaxima)
                    {
                        string mensaje = string.IsNullOrEmpty(validacion.MensajeError)
                            ? $"La propiedad {propiedad.Name} debe estar entre {validacion.LongitudMinima} y {validacion.LongitudMaxima}"
                            : validacion.MensajeError;
                        errores.Add(mensaje);
                    }
                }
                
                if (valor is decimal valorDecimal)
                {
                    if (valorDecimal < validacion.LongitudMinima || valorDecimal > validacion.LongitudMaxima)
                    {
                        string mensaje = string.IsNullOrEmpty(validacion.MensajeError)
                            ? $"La propiedad {propiedad.Name} debe estar entre {validacion.LongitudMinima} y {validacion.LongitudMaxima}"
                            : validacion.MensajeError;
                        errores.Add(mensaje);
                    }
                }
            }
        }
        
        if (errores.Count == 0)
        {
            Console.WriteLine("✅ Objeto válido - No se encontraron errores");
        }
        else
        {
            Console.WriteLine($"❌ Objeto inválido - {errores.Count} errores encontrados:");
            foreach (string error in errores)
            {
                Console.WriteLine($"  - {error}");
            }
        }
        
        return errores;
    }
}
```

### 5. Casos de Uso Avanzados

#### 5.1 Serialización Dinámica y Plugins

```csharp
// Interfaz para plugins
public interface IPlugin
{
    string Nombre { get; }
    string Version { get; }
    void Ejecutar();
}

// Plugin de ejemplo
public class PluginCalculadora : IPlugin
{
    public string Nombre => "Calculadora";
    public string Version => "1.0";
    
    public void Ejecutar()
    {
        Console.WriteLine("Plugin Calculadora ejecutándose...");
        Console.WriteLine("2 + 2 = 4");
    }
}

// Otro plugin
public class PluginSaludo : IPlugin
{
    public string Nombre => "Saludo";
    public string Version => "1.0";
    
    public void Ejecutar()
    {
        Console.WriteLine("Plugin Saludo ejecutándose...");
        Console.WriteLine("¡Hola desde el plugin!");
    }
}

// Sistema de plugins que usa reflexión
public class SistemaPlugins
{
    private List<IPlugin> plugins = new List<IPlugin>();
    
    // Cargar plugins desde tipos conocidos
    public void CargarPluginsConocidos()
    {
        Console.WriteLine("=== CARGA DE PLUGINS CONOCIDOS ===");
        
        Type[] tiposPlugin = { typeof(PluginCalculadora), typeof(PluginSaludo) };
        
        foreach (Type tipo in tiposPlugin)
        {
            if (typeof(IPlugin).IsAssignableFrom(tipo))
            {
                try
                {
                    IPlugin plugin = (IPlugin)Activator.CreateInstance(tipo);
                    plugins.Add(plugin);
                    Console.WriteLine($"Plugin cargado: {plugin.Nombre} v{plugin.Version}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error cargando plugin {tipo.Name}: {ex.Message}");
                }
            }
        }
        
        Console.WriteLine($"Total de plugins cargados: {plugins.Count}");
    }
    
    // Ejecutar todos los plugins
    public void EjecutarPlugins()
    {
        Console.WriteLine("\n=== EJECUCIÓN DE PLUGINS ===");
        
        foreach (IPlugin plugin in plugins)
        {
            Console.WriteLine($"\nEjecutando {plugin.Nombre}...");
            try
            {
                plugin.Ejecutar();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ejecutando plugin {plugin.Nombre}: {ex.Message}");
            }
        }
    }
    
    // Buscar y cargar plugins dinámicamente
    public void BuscarPluginsDinamicamente()
    {
        Console.WriteLine("\n=== BÚSQUEDA DINÁMICA DE PLUGINS ===");
        
        // Buscar en el assembly actual
        Assembly assembly = Assembly.GetExecutingAssembly();
        Type[] tipos = assembly.GetTypes();
        
        foreach (Type tipo in tipos)
        {
            // Verificar si implementa IPlugin y no es abstracto
            if (typeof(IPlugin).IsAssignableFrom(tipo) && !tipo.IsAbstract && tipo.IsClass)
            {
                try
                {
                    IPlugin plugin = (IPlugin)Activator.CreateInstance(tipo);
                    
                    // Verificar si ya está cargado
                    if (!plugins.Any(p => p.GetType() == tipo))
                    {
                        plugins.Add(plugin);
                        Console.WriteLine($"Plugin dinámico cargado: {plugin.Nombre} v{plugin.Version}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error cargando plugin dinámico {tipo.Name}: {ex.Message}");
                }
            }
        }
        
        Console.WriteLine($"Total de plugins después de búsqueda dinámica: {plugins.Count}");
    }
}

// Serializador dinámico usando reflexión
public class SerializadorDinamico
{
    // Serializar objeto a formato personalizado
    public string SerializarObjeto(object objeto)
    {
        if (objeto == null) return "null";
        
        Type tipo = objeto.GetType();
        StringBuilder sb = new StringBuilder();
        
        sb.AppendLine($"{tipo.Name} {{");
        
        PropertyInfo[] propiedades = tipo.GetProperties();
        foreach (PropertyInfo propiedad in propiedades)
        {
            if (propiedad.CanRead)
            {
                object valor = propiedad.GetValue(objeto);
                string valorStr = valor?.ToString() ?? "null";
                
                sb.AppendLine($"  {propiedad.Name}: {valorStr}");
            }
        }
        
        sb.AppendLine("}");
        return sb.ToString();
    }
    
    // Deserializar desde formato personalizado (simplificado)
    public T DeserializarObjeto<T>(string contenido) where T : class, new()
    {
        Type tipo = typeof(T);
        T instancia = new T();
        
        // Parseo simplificado - en un caso real usarías un parser más robusto
        string[] lineas = contenido.Split('\n');
        
        foreach (string linea in lineas)
        {
            if (linea.Contains(":") && !linea.Contains("{") && !linea.Contains("}"))
            {
                string[] partes = linea.Split(':');
                if (partes.Length == 2)
                {
                    string nombrePropiedad = partes[0].Trim();
                    string valor = partes[1].Trim();
                    
                    PropertyInfo propiedad = tipo.GetProperty(nombrePropiedad);
                    if (propiedad != null && propiedad.CanWrite)
                    {
                        try
                        {
                            // Conversión básica de tipos
                            object valorConvertido = Convert.ChangeType(valor, propiedad.PropertyType);
                            propiedad.SetValue(instancia, valorConvertido);
                        }
                        catch
                        {
                            // Ignorar errores de conversión
                        }
                    }
                }
            }
        }
        
        return instancia;
    }
}
```

## 🏋️ Ejercicios Prácticos

### Ejercicio 1: Validador de Objetos Genérico
```csharp
// Crear una clase ValidadorGenerico que:
// - Use reflexión para validar objetos
// - Lea atributos de validación personalizados
// - Genere reportes de validación
// - Soporte validaciones anidadas
```

### Ejercicio 2: Sistema de Configuración por Atributos
```csharp
// Crear un sistema que:
// - Lea configuración desde atributos en clases
// - Genere archivos de configuración automáticamente
// - Valide configuración en tiempo de ejecución
// - Soporte configuración por defecto
```

### Ejercicio 3: Inyector de Dependencias Simple
```csharp
// Crear un contenedor IoC que:
// - Use reflexión para crear instancias
// - Detecte dependencias automáticamente
// - Soporte inyección por constructor
// - Maneje ciclos de dependencias
```

## 🔍 Conceptos Importantes a Recordar

1. **La reflexión permite inspeccionar** tipos en tiempo de ejecución
2. **Activator.CreateInstance** crea instancias dinámicamente
3. **GetCustomAttribute** obtiene atributos personalizados
4. **Los atributos personalizados** extienden metadatos del código
5. **BindingFlags** controlan qué miembros se obtienen
6. **La reflexión tiene costo de rendimiento** - úsala con moderación
7. **Los atributos se pueden usar** para validación y documentación
8. **La reflexión es fundamental** para frameworks y herramientas

## ❓ Preguntas de Repaso

1. ¿Cuál es la diferencia entre typeof() y GetType()?
2. ¿Por qué es importante usar BindingFlags en reflexión?
3. ¿Cuándo usarías reflexión vs. código estático?
4. ¿Cómo implementas validación usando atributos personalizados?
5. ¿Qué ventajas y desventajas tiene la reflexión?

## 🚀 Siguiente Paso

¡Felicidades! Has completado el **Módulo 2 (Junior Level 2)** de C#. En el siguiente módulo aprenderemos sobre **Patrones de Diseño y Arquitectura**, donde veremos cómo estructurar aplicaciones de manera profesional.

---

## 📚 Navegación del Módulo 2

| Clase | Tema | Enlace |
|-------|------|--------|
| [Clase 1](clase_1_herencia.md) | Herencia en C# | |
| [Clase 2](clase_2_polimorfismo.md) | Polimorfismo y Métodos Virtuales | |
| [Clase 3](clase_3_interfaces.md) | Interfaces en C# | |
| [Clase 4](clase_4_clases_abstractas.md) | Clases Abstractas | |
| [Clase 5](clase_5_genericos.md) | Genéricos en C# | |
| [Clase 6](clase_6_delegados_eventos.md) | Delegados y Eventos | |
| [Clase 7](clase_7_linq.md) | LINQ en C# | |
| [Clase 8](clase_8_archivos_streams.md) | Manejo de Archivos y Streams | |
| [Clase 9](clase_9_programacion_asincrona.md) | Programación Asíncrona | ← Anterior |
| **Clase 10** | **Reflexión y Metaprogramación** | ← Estás aquí |

**← [Volver al README del Módulo 2](../junior_2/README.md)**

**🎉 ¡Módulo 2 Completado! → [Ir al Módulo 3](../midLevel_1/README.md)**

---

## 📚 Recursos Adicionales

- [Reflexión en C#](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/reflection)
- [Atributos personalizados](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/attributes/)
- [Activator.CreateInstance](https://docs.microsoft.com/en-us/dotnet/api/system.activator.createinstance)

---

**¡Excelente! Has completado el módulo de reflexión y metaprogramación en C#! 🎯**

**¡Felicidades por completar el Módulo 2 completo! 🎉**
